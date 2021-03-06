﻿/*
 * Copyright (c) 2013, SEMTECH S.A.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * * Redistributions of source code must retain the above copyright
 *   notice, this list of conditions and the following disclaimer.
 * * Redistributions in binary form must reproduce the above copyright
 *   notice, this list of conditions and the following disclaimer in the
 *   documentation and/or other materials provided with the distribution.
 * * Neither the name of the Semtech corporation nor the
 *   names of its contributors may be used to endorse or promote products
 *   derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL SEMTECH S.A. BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;

using BlubbFish.Utils;

namespace Fraunhofer.Fit.Iot.Lora.lib.Ic880a {
  public partial class Ic880a {
    private Boolean _lbt_enabled = false;
    private Byte _lbt_nb_active_channel = 0;
    private SByte _lbt_rssi_target_dBm = 0;
    private SByte _lbt_rssi_offset_dB = 0;
    private UInt32 _lbt_start_freq = 0;
    private readonly LbtChan[] _lbt_channel_cfg = new LbtChan[8];

    private void LbtParseConfig() {
      this._lbt_enabled = Boolean.Parse(this.config["lbt_enable"]);
      if(this._lbt_enabled) {
        this._lbt_nb_active_channel = Byte.Parse(this.config["lbt_nb_channel"]);
        if(this._lbt_nb_active_channel < 1 || this._lbt_nb_active_channel > 8) {
          throw new Exception("ERROR: Number of defined LBT channels is out of range (" + this._lbt_nb_active_channel + ")");
        }
        this._lbt_rssi_target_dBm = SByte.Parse(this.config["lbt_rssi_target"]);
        this._lbt_rssi_offset_dB = SByte.Parse(this.config["lbt_rssi_offset"]);
        for(Int32 i = 0; i < this._lbt_nb_active_channel; i++) {
          this._lbt_channel_cfg[i].freq_hz = UInt32.Parse(this.config["lbt_channel" + i + "_freq_hz"]);
          this._lbt_channel_cfg[i].scan_time_us = UInt16.Parse(this.config["lbt_channel" + i + "_scan_time_us"]);
        }
      }
    }

    private void LbtSetup() {
      // Check if LBT feature is supported by FPGA
      if(this.BitCheck((Byte)this.FpgaRegisterRead(Registers.FPGA_FEATURE), 2, 1) != 1) {
        throw new Exception("ERROR: No support for LBT in FPGA");
      }

      // Get FPGA lowest frequency for LBT channels
      Int32 val = this.FpgaRegisterRead(Registers.FPGA_LBT_INITIAL_FREQ);
      this._lbt_start_freq = val switch
      {
        0 => 915000000,
        1 => 863000000,
        _ => throw new Exception("ERROR: LBT start frequency " + val + " is not supported"),
      };

      // Configure SX127x for FSK 
      this.SetupSx127x(this._lbt_start_freq, 0x20, Sx127xRxbwE.LGW_SX127X_RXBW_100K_HZ, this._lbt_rssi_offset_dB); // 200KHz LBT channels 


      // Configure FPGA for LBT 
      val = -2 * this._lbt_rssi_target_dBm; // Convert RSSI target in dBm to FPGA register format 
      this.FpgaRegisterWrite(Registers.FPGA_RSSI_TARGET, val);
      // Set default values for non-active LBT channels 
      for(Int32 i = this._lbt_nb_active_channel; i < 8; i++) {
        this._lbt_channel_cfg[i].freq_hz = this._lbt_start_freq;
        this._lbt_channel_cfg[i].scan_time_us = 128; // fastest scan for non-active channels 
      }
      // Configure FPGA for both active and non-active LBT channels 
      for(Int32 i = 0; i < 8; i++) {
        // Check input parameters 
        if(this._lbt_channel_cfg[i].freq_hz < this._lbt_start_freq) {
          throw new ArgumentException("ERROR: LBT channel frequency is out of range (" + this._lbt_channel_cfg[i].freq_hz + ")");
        }
        if(this._lbt_channel_cfg[i].scan_time_us != 128 && this._lbt_channel_cfg[i].scan_time_us != 5000) {
          throw new ArgumentException("ERROR: LBT channel scan time is not supported (" + this._lbt_channel_cfg[i].scan_time_us + ")");
        }
        // Configure 
        UInt32 freq_offset = (UInt32)((this._lbt_channel_cfg[i].freq_hz - this._lbt_start_freq) / 100E3); // 100kHz unit 
        this.FpgaRegisterWrite((FpgaRegisters)Helper.GetField(typeof(Registers), "FPGA_LBT_CH" + i + "_FREQ_OFFSET"), (Int32)freq_offset);
        if(this._lbt_channel_cfg[i].scan_time_us == 5000) { // configured to 128 by default 
          this.FpgaRegisterWrite((FpgaRegisters)Helper.GetField(typeof(Registers), "FPGA_LBT_SCAN_TIME_CH" + i), 1);
        }
      }

      Console.WriteLine("Note: LBT configuration:");
      Console.WriteLine("\tlbt_enable: " + this._lbt_enabled);
      Console.WriteLine("\tlbt_nb_active_channel: " + this._lbt_nb_active_channel);
      Console.WriteLine("\tlbt_start_freq: " + this._lbt_start_freq);
      Console.WriteLine("\tlbt_rssi_target: " + this._lbt_rssi_target_dBm);
      for(Int32 i = 0; i < 8; i++) {
        Console.WriteLine("\tlbt_channel_cfg[" + i + "].freq_hz: " + this._lbt_channel_cfg[i].freq_hz);
        Console.WriteLine("\tlbt_channel_cfg[" + i + "].scan_time_us: " + this._lbt_channel_cfg[i].scan_time_us);
      }
    }

    private void LbtStart() => this.FpgaRegisterWrite(Registers.FPGA_CTRL_FEATURE_START, 1);

    private Boolean LbtIsChannelFree(SendingPacket pkt_data, UInt16 tx_start_delay) {
      // Check if TX is allowed 
      if(this._lbt_enabled == true) {
        // TX allowed for LoRa only 
        if(pkt_data.modulation != Modulation.Lora) {
          Console.WriteLine("INFO: TX is not allowed for this modulation (" + pkt_data.modulation + ")");
          return false;
        }

        // Get SX1301 time at last PPS 
        UInt32 sx1301_time = (UInt32)this.RegisterRead(Registers.TIMESTAMP);

        Console.WriteLine("################################");
        UInt32 tx_start_time;
        switch(pkt_data.tx_mode) {
          case SendingMode.TIMESTAMPED:
            Console.WriteLine("tx_mode                    = TIMESTAMPED");
            tx_start_time = pkt_data.count_us & 0x007FF000;
            break;
          case SendingMode.ON_GPS:
            Console.WriteLine("tx_mode                    = ON_GPS");
            tx_start_time = (sx1301_time + tx_start_delay + 1000000) & 0x007FF000;
            break;
          case SendingMode.IMMEDIATE:
            throw new Exception("ERROR: tx_mode IMMEDIATE is not supported when LBT is enabled");
          // FALLTHROUGH  
          default:
            throw new Exception("ERROR: Mode is not supported");
        }

        // Select LBT Channel corresponding to required TX frequency 
        Int32 lbt_channel_decod_1 = -1;
        Int32 lbt_channel_decod_2 = -1;
        UInt32 tx_max_time = 0;
        if(pkt_data.bandwidth == BW.BW_125KHZ) {
          for(Int32 i = 0; i < this._lbt_nb_active_channel; i++) {
            if(Math.Abs(pkt_data.freq_hz - this._lbt_channel_cfg[i].freq_hz) < 1000) {
              Console.WriteLine("LBT: select channel " + i + " (" + this._lbt_channel_cfg[i].freq_hz + " Hz)");
              lbt_channel_decod_1 = i;
              lbt_channel_decod_2 = i;
              tx_max_time = this._lbt_channel_cfg[i].scan_time_us == 5000 ? 4000000 : (UInt32)400000;
              break;
            }
          }
        } else if(pkt_data.bandwidth == BW.BW_250KHZ) {
          // In case of 250KHz, the TX freq has to be in between 2 consecutive channels of 200KHz BW.
          //    The TX can only be over 2 channels, not more 
          for(Int32 i = 0; i < this._lbt_nb_active_channel - 1; i++) {
            if(Math.Abs(pkt_data.freq_hz - (this._lbt_channel_cfg[i].freq_hz + this._lbt_channel_cfg[i + 1].freq_hz) / 2) < 1000 && this._lbt_channel_cfg[i + 1].freq_hz - this._lbt_channel_cfg[i].freq_hz == 200E3) {
              Console.WriteLine("LBT: select channels " + i + "," + (i + 1) + " (" + (this._lbt_channel_cfg[i].freq_hz + this._lbt_channel_cfg[i + 1].freq_hz) / 2 + " Hz)");
              lbt_channel_decod_1 = i;
              lbt_channel_decod_2 = i + 1;
              tx_max_time = this._lbt_channel_cfg[i].scan_time_us == 5000 ? 4000000 : (UInt32)200000;
              break;
            }
          }
        } else {
          // Nothing to do for now 
        }

        // Get last time when selected channel was free 
        UInt32 lbt_time;
        UInt32 lbt_time1 = 0;
        UInt32 lbt_time2 = 0;
        if(lbt_channel_decod_1 >= 0 && lbt_channel_decod_2 >= 0) {
          this.FpgaRegisterWrite(Registers.FPGA_LBT_TIMESTAMP_SELECT_CH, lbt_channel_decod_1);
          lbt_time = lbt_time1 = (UInt32)(this.FpgaRegisterRead(Registers.FPGA_LBT_TIMESTAMP_CH) & 0x0000FFFF) * 256; // 16bits (1LSB = 256µs) 

          if(lbt_channel_decod_1 != lbt_channel_decod_2) {
            this.FpgaRegisterWrite(Registers.FPGA_LBT_TIMESTAMP_SELECT_CH, lbt_channel_decod_2);
            lbt_time2 = (UInt32)(this.FpgaRegisterRead(Registers.FPGA_LBT_TIMESTAMP_CH) & 0x0000FFFF) * 256; // 16bits (1LSB = 256µs) 

            if(lbt_time2 < lbt_time1) {
              lbt_time = lbt_time2;
            }
          }
        } else {
          lbt_time = 0;
        }

        UInt32 packet_duration = (UInt32)(this.TimeOnAir(pkt_data) * 1000UL);
        UInt32 tx_end_time = (tx_start_time + packet_duration) & 0x007FF000;
        UInt32 delta_time;
        if(lbt_time < tx_end_time) {
          delta_time = tx_end_time - lbt_time;
        } else {
          // It means LBT counter has wrapped 
          Console.WriteLine("LBT: lbt counter has wrapped");
          delta_time = 0x007FF000 - lbt_time + tx_end_time;
        }

        Console.WriteLine("sx1301_time                = " + (sx1301_time & 0x007FF000));
        Console.WriteLine("tx_freq                    = " + pkt_data.freq_hz);
        Console.WriteLine("------------------------------------------------");
        Console.WriteLine("packet_duration            = " + packet_duration);
        Console.WriteLine("tx_start_time              = " + tx_start_time);
        Console.WriteLine("lbt_time1                  = " + lbt_time1);
        Console.WriteLine("lbt_time2                  = " + lbt_time2);
        Console.WriteLine("lbt_time                   = " + lbt_time);
        Console.WriteLine("delta_time                 = " + delta_time);
        Console.WriteLine("------------------------------------------------");

        // send data if allowed 
        // lbt_time: last time when channel was free 
        // tx_max_time: maximum time allowed to send packet since last free time 
        // 2048: some margin 
        if(delta_time < tx_max_time - 2048 && lbt_time != 0) {
          return true;
        } else {
          Console.WriteLine("ERROR: TX request rejected (LBT)");
          return false;
        }
      } else {
        // Always allow if LBT is disabled 
        return true;
      }

    }
  }
}
