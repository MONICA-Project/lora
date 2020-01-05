using System;

using BlubbFish.Utils;

namespace Fraunhofer.Fit.Iot.Lora.lib.Ic880a {
  public partial class Ic880a {
    private void LbtSetup() {
      // Check if LBT feature is supported by FPGA
      if(this.BitCheck((Byte)this.RegisterFpgaRead(Registers.FPGA_FEATURE), 2, 1) != 1) {
        throw new Exception("ERROR: No support for LBT in FPGA");
      }

      // Get FPGA lowest frequency for LBT channels
      Int32 val = this.RegisterFpgaRead(Registers.FPGA_LBT_INITIAL_FREQ);
      this._lbt_start_freq = val switch
      {
        0 => 915000000,
        1 => 863000000,
        _ => throw new Exception("ERROR: LBT start frequency " + val + " is not supported"),
      };

      // Configure SX127x for FSK 
      this.RegisterSx127xSetup(this._lbt_start_freq, 0x20, Sx127xRxbwE.LGW_SX127X_RXBW_100K_HZ, this._lbt_rssi_offset_dB); // 200KHz LBT channels 


      // Configure FPGA for LBT 
      val = -2 * this._lbt_rssi_target_dBm; // Convert RSSI target in dBm to FPGA register format 
      this.RegisterFpgaWrite(Registers.FPGA_RSSI_TARGET, val);
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
        this.RegisterFpgaWrite((FpgaRegisters)Helper.GetField(typeof(Registers), "FPGA_LBT_CH" + i + "_FREQ_OFFSET"), (Int32)freq_offset);
        if(this._lbt_channel_cfg[i].scan_time_us == 5000) { // configured to 128 by default 
          this.RegisterFpgaWrite((FpgaRegisters)Helper.GetField(typeof(Registers), "FPGA_LBT_SCAN_TIME_CH" + i), 1);
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

    private void LbtStart() => this.RegisterFpgaWrite(Registers.FPGA_CTRL_FEATURE_START, 1);

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
          this.RegisterFpgaWrite(Registers.FPGA_LBT_TIMESTAMP_SELECT_CH, lbt_channel_decod_1);
          lbt_time = lbt_time1 = (UInt32)(this.RegisterFpgaRead(Registers.FPGA_LBT_TIMESTAMP_CH) & 0x0000FFFF) * 256; // 16bits (1LSB = 256µs) 

          if(lbt_channel_decod_1 != lbt_channel_decod_2) {
            this.RegisterFpgaWrite(Registers.FPGA_LBT_TIMESTAMP_SELECT_CH, lbt_channel_decod_2);
            lbt_time2 = (UInt32)(this.RegisterFpgaRead(Registers.FPGA_LBT_TIMESTAMP_CH) & 0x0000FFFF) * 256; // 16bits (1LSB = 256µs) 

            if(lbt_time2 < lbt_time1) {
              lbt_time = lbt_time2;
            }
          }
        } else {
          lbt_time = 0;
        }

        UInt32 packet_duration = (UInt32)(this.LgwTimeOnAir(pkt_data) * 1000UL);
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
