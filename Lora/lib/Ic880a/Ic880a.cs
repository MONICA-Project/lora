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
using System.Collections.Generic;
using System.Threading;

using BlubbFish.Utils;

using Fraunhofer.Fit.Iot.Lora.Events;

using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;
using Unosquare.WiringPi;

namespace Fraunhofer.Fit.Iot.Lora.lib.Ic880a {
  public partial class Ic880a : LoraBoard {
    #region Abstracts - LoraBoard
    public Ic880a(Dictionary<String, String> settings) : base(settings) {
      Pi.Init<BootstrapWiringPi>();
      this.ParseConfig();
      this.Interfaces = 8;
    }

    public override void Begin() {
      this.SetupIO();
      this.Reset();
      this.Connect();
      this.StartRadio();
      this.Debug("Fraunhofer.Fit.Iot.Lora.lib.Ic880a.Ic880a.Begin(): Succsessfull init Ic880a-board with SX1231 Chip!");
    }

    public override void Dispose() {
      this._deviceStarted = false;
      this._recieverThreadRunning = false;
      while(this._recieverThread.IsAlive) {
        Thread.Sleep(10);
      }
      this._recieverThread = null;
      this.SoftReset(); //reset the registers (also shuts the radios down)
      this.SPIwriteRegisterRaw(0, 128);
      this.PinReset.Write(GpioPinValue.High);
      Console.WriteLine("Reset Hardware!");
      Thread.Sleep(500);
      this.PinReset.Write(GpioPinValue.Low);
      this.Debug("Fraunhofer.Fit.Iot.Lora.lib.Ic880a.Ic880a.Dispose(): Succsessfull shutdown Ic880a-board with SX1231 Chip!");
    }

    public override void Send(Byte[] data, Byte @interface) {
      SendingPacket p = this.PrepareSend(data, @interface);
      Ic880aTransmittedObj d = new Ic880aTransmittedObj {
        Data = data,
        Msgtolong = p.payload.Length > 255
      };
      if(this._deviceStarted && !d.Msgtolong) {
        this._istransmitting = true;
        lock(this.HandleControllerIOLock) {
          try {
            this.Transmit(p);
            d.Datarate = this._dataRate;
            d.Txtimeout = this._SendTimeout;
          } catch(Exception e) {
            Helper.WriteError(e.Message);
          }
        }
        this._istransmitting = false;
      }
      this.RaiseTransmittedEvent(d);
    }

    public override void StartEventRecieving() {
      this._recieverThread = new Thread(this.RecieverThreadRunner);
      this._recieverThreadRunning = true;
      this._recieverThread.Start();
      //this._isrecieving = true;
    }
    #endregion

    #region Private Methodes
    private void StartRadio() {
      //reset the registers (also shuts the radios down)
      this.SoftReset();

      //gate clocks
      this.RegisterWrite(Registers.GLOBAL_EN, 0); 
      this.RegisterWrite(Registers.CLK32M_EN, 0);

      //switch on and reset the radios (also starts the 32 MHz XTAL)
      this.RegisterWrite(Registers.RADIO_A_EN, 1); 
      this.RegisterWrite(Registers.RADIO_B_EN, 1);
      Thread.Sleep(500);
      this.RegisterWrite(Registers.RADIO_RST, 1);
      Thread.Sleep(5);
      this.RegisterWrite(Registers.RADIO_RST, 0);

      this.SetupSx125x(0, 1, this._radioEnabled[0], RadioType.SX1257, this._radioFrequency[0]);
      this.SetupSx125x(1, 1, this._radioEnabled[1], RadioType.SX1257, this._radioFrequency[1]);

      // gives AGC control of GPIOs to enable Tx external digital filter 
      this.RegisterWrite(Registers.GPIO_MODE, 31); // Set all GPIOs as output 
      this.RegisterWrite(Registers.GPIO_SELECT_OUTPUT, 0); 

      // Configure LBT 
      if(this._lbt_enabled) {
        this.RegisterWrite(Registers.CLK32M_EN, 1);
        this.LbtSetup();

        // Start SX1301 counter and LBT FSM at the same time to be in sync 
        this.RegisterWrite(Registers.CLK32M_EN, 0);
        this.LbtStart();
      }

      this.RegisterWrite(Registers.GLOBAL_EN, 1); // Enable clocks 
      this.RegisterWrite(Registers.CLK32M_EN, 1);

      Byte cal_cmd = 0; // select calibration command 
      cal_cmd |= this._radioEnabled[0] ? (Byte)0x01 : (Byte)0x00; // Bit 0: Calibrate Rx IQ mismatch compensation on radio A
      cal_cmd |= this._radioEnabled[1] ? (Byte)0x02 : (Byte)0x00; // Bit 1: Calibrate Rx IQ mismatch compensation on radio B
      cal_cmd |= (this._radioEnabled[0] && this._radioEnableTx[0]) ? (Byte)0x04 : (Byte)0x00; // Bit 2: Calibrate Tx DC offset on radio A 
      cal_cmd |= (this._radioEnabled[1] && this._radioEnableTx[1]) ? (Byte)0x08 : (Byte)0x00; // Bit 3: Calibrate Tx DC offset on radio B 
      cal_cmd |= 0x10; // Bit 4: 0: calibrate with DAC gain=2, 1: with DAC gain=3 (use 3) 

      switch (this._rf_radio_type[0]) { // we assume that there is only one radio type on the board 
        case RadioType.SX1255: //LGW_RADIO_TYPE_SX1255:
          cal_cmd |= 0x20; // Bit 5: 0: SX1257, 1: SX1255 
          break;
        case RadioType.SX1257: //LGW_RADIO_TYPE_SX1257:
          cal_cmd |= 0x00; // Bit 5: 0: SX1257, 1: SX1255 
          break;
      }

      cal_cmd |= 0x00; // Bit 6-7: Board type 0: ref, 1: FPGA, 3: board X 
      UInt16 cal_time = 2300; // measured between 2.1 and 2.2 sec, because 1 TX only 

      this.LoadFirmware(Firmware.CAL); // Load the calibration firmware  
      this.RegisterWrite(Registers.FORCE_HOST_RADIO_CTRL, 0); // gives to AGC MCU the control of the radios 
      this.RegisterWrite(Registers.RADIO_SELECT, cal_cmd); // send calibration configuration word 
      this.RegisterWrite(Registers.MCU_RST_1, 0);

      this.RegisterWrite(Registers.DBG_AGC_MCU_RAM_ADDR, Firmware.CAL.Address); // Check firmware version 
      Int32 fw_version = this.RegisterRead(Registers.DBG_AGC_MCU_RAM_DATA);
      if(fw_version != Firmware.CAL.Version) {
        throw new Exception("ERROR: Version of calibration firmware not expected, actual: " + fw_version + " expected: " + Firmware.CAL.Version);
      }

      this.RegisterWrite(Registers.PAGE_REG, 3); // Calibration will start on this condition as soon as MCU can talk to concentrator registers 
      this.RegisterWrite(Registers.EMERGENCY_FORCE_HOST_CTRL, 0); // Give control of concentrator registers to MCU 

      //Console.WriteLine("Note: calibration started (time: "+ cal_time + " ms)"); // Wait for calibration to end 
      Thread.Sleep(cal_time); // Wait for end of calibration 
      this.RegisterWrite(Registers.EMERGENCY_FORCE_HOST_CTRL, 1); // Take back control 

      Int32 cal_status = this.RegisterRead(Registers.MCU_AGC_STATUS);
      if((cal_status & 0x81) != 0x81) {  //bit 0: could access SX1301 registers
        for(Int32 i = 0; i < 10; i++) {
          Int32 st = this.RegisterRead(Registers.MCU_AGC_STATUS);
          Console.WriteLine(st);
          Thread.Sleep(1000);
        }
        throw new Exception("ERROR: CALIBRATION FAILURE (STATUS = " + cal_status + ")");
      } else { //bit 7: calibration finished
        //Console.WriteLine("Note: calibration finished (status = "+ cal_status + ")");
      }
      if(this._radioEnabled[0] && (cal_status & 0x02) == 0) { //bit 1: could access radio A registers
        throw new Exception("WARNING: calibration could not access radio A\n");
      }
      if(this._radioEnabled[1] && (cal_status & 0x04) == 0) { //bit 2: could access radio B registers
        throw new Exception("WARNING: calibration could not access radio B\n");
      }
      if(this._radioEnabled[0] && (cal_status & 0x08) == 0) { //bit 3: radio A RX image rejection successful
        throw new Exception("WARNING: problem in calibration of radio A for image rejection\n");
      }
      if(this._radioEnabled[1] && (cal_status & 0x10) == 0) { //bit 4: radio B RX image rejection successful
        throw new Exception("WARNING: problem in calibration of radio B for image rejection\n");
      }
      if(this._radioEnabled[0] && this._radioEnableTx[0] && (cal_status & 0x20) == 0) { //bit 5: radio A TX DC Offset correction successful
        throw new Exception("WARNING: problem in calibration of radio A for TX DC offset\n");
      }
      if(this._radioEnabled[1] && this._radioEnableTx[1] && (cal_status & 0x40) == 0) { //bit 6: radio B TX DC Offset correction successful
        throw new Exception("WARNING: problem in calibration of radio B for TX DC offset\n");
      }

      for(Byte i = 0; i <= 7; ++i) { // Get TX DC offset values 
        this.RegisterWrite(Registers.DBG_AGC_MCU_RAM_ADDR, 0xA0 + i);
        Int32 read_val = this.RegisterRead(Registers.DBG_AGC_MCU_RAM_DATA);
        this._cal_offset_a_i[i] = (SByte)read_val;
        this.RegisterWrite(Registers.DBG_AGC_MCU_RAM_ADDR, 0xA8 + i);
        read_val = this.RegisterRead(Registers.DBG_AGC_MCU_RAM_DATA);
        this._cal_offset_a_q[i] = (SByte)read_val;
        this.RegisterWrite(Registers.DBG_AGC_MCU_RAM_ADDR, 0xB0 + i);
        read_val = this.RegisterRead(Registers.DBG_AGC_MCU_RAM_DATA);
        this._cal_offset_b_i[i] = (SByte)read_val;
        this.RegisterWrite(Registers.DBG_AGC_MCU_RAM_ADDR, 0xB8 + i);
        read_val = this.RegisterRead(Registers.DBG_AGC_MCU_RAM_DATA);
        this._cal_offset_b_q[i] = (SByte)read_val;
      }

      this.LoadAdjustConstants(); // load adjusted parameters 

      if(this._radioFrequency[0] == 0) { // Sanity check for RX frequency 
        throw new Exception("ERROR: wrong configuration, rf_rx_freq[0] is not set\n");
      }

      UInt32 x = 4096000000 / (this._radioFrequency[0] >> 1); // Freq-to-time-drift calculation  dividend: (4*2048*1000000) >> 1, rescaled to avoid 32b overflow 
      x = (x > 63) ? 63 : x; // saturation 
      this.RegisterWrite(Registers.FREQ_TO_TIME_DRIFT, (Int32)x); // default 9 

      x = 4096000000 / (this._radioFrequency[0] >> 3); // dividend: (16*2048*1000000) >> 3, rescaled to avoid 32b overflow 
      x = (x > 63) ? 63 : x; // saturation 
      this.RegisterWrite(Registers.MBWSSF_FREQ_TO_TIME_DRIFT, (Int32)x); // default 36 

      this.RegisterWrite(Registers.IF_FREQ_0, (this._interfaceFrequency[0] << 5) / 15625); // default -384 
      this.RegisterWrite(Registers.IF_FREQ_1, (this._interfaceFrequency[1] << 5) / 15625); // default -128 
      this.RegisterWrite(Registers.IF_FREQ_2, (this._interfaceFrequency[2] << 5) / 15625); // default 128 
      this.RegisterWrite(Registers.IF_FREQ_3, (this._interfaceFrequency[3] << 5) / 15625); // default 384 
      this.RegisterWrite(Registers.IF_FREQ_4, (this._interfaceFrequency[4] << 5) / 15625); // default -384 
      this.RegisterWrite(Registers.IF_FREQ_5, (this._interfaceFrequency[5] << 5) / 15625); // default -128 
      this.RegisterWrite(Registers.IF_FREQ_6, (this._interfaceFrequency[6] << 5) / 15625); // default 128 
      this.RegisterWrite(Registers.IF_FREQ_7, (this._interfaceFrequency[7] << 5) / 15625); // default 384 

      this.RegisterWrite(Registers.CORR0_DETECT_EN, this._interfaceEnabled[0] ? (Byte)SF.DR_LORA_SFMULTI : 0); // default 0 
      this.RegisterWrite(Registers.CORR1_DETECT_EN, this._interfaceEnabled[1] ? (Byte)SF.DR_LORA_SFMULTI : 0); // default 0 
      this.RegisterWrite(Registers.CORR2_DETECT_EN, this._interfaceEnabled[2] ? (Byte)SF.DR_LORA_SFMULTI : 0); // default 0 
      this.RegisterWrite(Registers.CORR3_DETECT_EN, this._interfaceEnabled[3] ? (Byte)SF.DR_LORA_SFMULTI : 0); // default 0 
      this.RegisterWrite(Registers.CORR4_DETECT_EN, this._interfaceEnabled[4] ? (Byte)SF.DR_LORA_SFMULTI : 0); // default 0 
      this.RegisterWrite(Registers.CORR5_DETECT_EN, this._interfaceEnabled[5] ? (Byte)SF.DR_LORA_SFMULTI : 0); // default 0 
      this.RegisterWrite(Registers.CORR6_DETECT_EN, this._interfaceEnabled[6] ? (Byte)SF.DR_LORA_SFMULTI : 0); // default 0
      this.RegisterWrite(Registers.CORR7_DETECT_EN, this._interfaceEnabled[7] ? (Byte)SF.DR_LORA_SFMULTI : 0); // default 0 

      this.RegisterWrite(Registers.PPM_OFFSET, 0x60); // as the threshold is 16ms, use 0x60 to enable ppm_offset for SF12 and SF11 @125kHz

      this.RegisterWrite(Registers.CONCENTRATOR_MODEM_ENABLE, 1); // default 0 


      this.RegisterWrite(Registers.IF_FREQ_8, (this._interfaceFrequency[8] << 5) / 15625); // configure LoRa 'stand-alone' modem (IF8)  MBWSSF modem (default 0) 
      if(this._interfaceEnabled[8] == true) {
        this.RegisterWrite(Registers.MBWSSF_RADIO_SELECT, (Byte)this._interfaceChain[8]);
        switch(this._loraBandwidth) {
          case BW.BW_125KHZ:
            this.RegisterWrite(Registers.MBWSSF_MODEM_BW, 0);
            break;
          case BW.BW_250KHZ:
            this.RegisterWrite(Registers.MBWSSF_MODEM_BW, 1);
            break;
          case BW.BW_500KHZ:
            this.RegisterWrite(Registers.MBWSSF_MODEM_BW, 2);
            break;
        }
        switch(this._loraSpreadingFactor) {
          case SF.DR_LORA_SF7:
            this.RegisterWrite(Registers.MBWSSF_RATE_SF, 7);
            break;
          case SF.DR_LORA_SF8:
            this.RegisterWrite(Registers.MBWSSF_RATE_SF, 8);
            break;
          case SF.DR_LORA_SF9:
            this.RegisterWrite(Registers.MBWSSF_RATE_SF, 9);
            break;
          case SF.DR_LORA_SF10:
            this.RegisterWrite(Registers.MBWSSF_RATE_SF, 10);
            break;
          case SF.DR_LORA_SF11:
            this.RegisterWrite(Registers.MBWSSF_RATE_SF, 11);
            break;
          case SF.DR_LORA_SF12:
            this.RegisterWrite(Registers.MBWSSF_RATE_SF, 12);
            break;
        }
        this.RegisterWrite(Registers.MBWSSF_PPM_OFFSET, this._loraBandwidth == BW.BW_125KHZ && (this._loraSpreadingFactor == SF.DR_LORA_SF11 || this._loraSpreadingFactor == SF.DR_LORA_SF12) || this._loraBandwidth == BW.BW_250KHZ && this._loraSpreadingFactor == SF.DR_LORA_SF12 ? 1 : 0); // default 0 
        this.RegisterWrite(Registers.MBWSSF_MODEM_ENABLE, 1); // default 0 
      } else {
        this.RegisterWrite(Registers.MBWSSF_MODEM_ENABLE, 0);
      }

      this.RegisterWrite(Registers.IF_FREQ_9, (this._interfaceFrequency[9] << 5) / 15625);// configure FSK modem (IF9) FSK modem, default 0 
      this.RegisterWrite(Registers.FSK_PSIZE, this._fskSyncWordSize - 1);
      this.RegisterWrite(Registers.FSK_TX_PSIZE, this._fskSyncWordSize - 1);
      UInt64 fsk_sync_word_reg = this._fskSyncWord << (8 * (8 - this._fskSyncWordSize));
      this.RegisterWrite(Registers.FSK_REF_PATTERN_LSB, (Int32)(UInt32)(0xFFFFFFFF & fsk_sync_word_reg));
      this.RegisterWrite(Registers.FSK_REF_PATTERN_MSB, (Int32)(UInt32)(0xFFFFFFFF & (fsk_sync_word_reg >> 32)));
      if(this._interfaceEnabled[9] == true) {
        this.RegisterWrite(Registers.FSK_RADIO_SELECT, (Byte)this._interfaceChain[9]);
        this.RegisterWrite(Registers.FSK_BR_RATIO, (Int32)(32000000 / this._fskDatarate)); // setting the dividing ratio for datarate 
        this.RegisterWrite(Registers.FSK_CH_BW_EXPO, (Byte)this._fskBandwidth);
        this.RegisterWrite(Registers.FSK_MODEM_ENABLE, 1); // default 0 
      } else {
        this.RegisterWrite(Registers.FSK_MODEM_ENABLE, 0);
      }

      this.LoadFirmware(Firmware.ARB); // Load firmware 
      this.LoadFirmware(Firmware.AGC);

      this.RegisterWrite(Registers.FORCE_HOST_RADIO_CTRL, 0); // gives the AGC MCU control over radio, RF front-end and filter gain 
      this.RegisterWrite(Registers.FORCE_HOST_FE_CTRL, 0);
      this.RegisterWrite(Registers.FORCE_DEC_FILTER_GAIN, 0);

      this.RegisterWrite(Registers.RADIO_SELECT, 0); // Get MCUs out of reset */ /* MUST not be = to 1 or 2 at firmware init 
      this.RegisterWrite(Registers.MCU_RST_0, 0);
      this.RegisterWrite(Registers.MCU_RST_1, 0);

      this.RegisterWrite(Registers.DBG_AGC_MCU_RAM_ADDR, Firmware.AGC.Address); // Check firmware version 
      fw_version = this.RegisterRead(Registers.DBG_AGC_MCU_RAM_DATA);
      if(fw_version != Firmware.AGC.Version) {
        throw new Exception("ERROR: Version of AGC firmware not expected, actual: " + fw_version + " expected: " + Firmware.AGC.Version);
      }
      this.RegisterWrite(Registers.DBG_ARB_MCU_RAM_ADDR, Firmware.ARB.Address);
      fw_version = this.RegisterRead(Registers.DBG_ARB_MCU_RAM_DATA);
      if(fw_version != Firmware.ARB.Version) {
        throw new Exception("ERROR: Version of arbiter firmware not expected, actual: " + fw_version + " expected: " + Firmware.ARB.Version);
      }

      //Console.WriteLine("Info: Initialising AGC firmware...");
      Thread.Sleep(1);

      Int32 status = this.RegisterRead(Registers.MCU_AGC_STATUS);
      if(status != 0x10) {
        throw new Exception("ERROR: AGC FIRMWARE INITIALIZATION FAILURE, STATUS " + status.ToString("X2"));
      }

      for(Byte i = 0; i < this._lut.Length; ++i) { // Update Tx gain LUT and start AGC 
        this.RegisterWrite(Registers.RADIO_SELECT, 16); // start a transaction 
        Thread.Sleep(1);
        this.RegisterWrite(Registers.RADIO_SELECT, this._lut[i].mix_gain + 16 * this._lut[i].dac_gain + 64 * this._lut[i].pa_gain);
        Thread.Sleep(1);
        status = this.RegisterRead(Registers.MCU_AGC_STATUS);
        if(status != 0x30 + i) {
          throw new Exception("ERROR: AGC FIRMWARE INITIALIZATION FAILURE, STATUS " + status.ToString("X2"));
        }
      }

      if(this._lut.Length < 16) { // As the AGC fw is waiting for 16 entries, we need to abort the transaction if we get less entries 
        this.RegisterWrite(Registers.RADIO_SELECT, 16);
        Thread.Sleep(1);
        this.RegisterWrite(Registers.RADIO_SELECT, 17);
        Thread.Sleep(1);
        status = this.RegisterRead(Registers.MCU_AGC_STATUS);
        if(status != 0x30) {
          throw new Exception("ERROR: AGC FIRMWARE INITIALIZATION FAILURE, STATUS " + status.ToString("X2"));
        }
      }


      this.RegisterWrite(Registers.RADIO_SELECT, 16); // Load Tx freq MSBs (always 3 if f > 768 for SX1257 or f > 384 for SX1255 
      Thread.Sleep(1);
      this.RegisterWrite(Registers.RADIO_SELECT, 3);
      Thread.Sleep(1);
      status = this.RegisterRead(Registers.MCU_AGC_STATUS);
      if(status != 0x33) {
        throw new Exception("ERROR: AGC FIRMWARE INITIALIZATION FAILURE, STATUS " + status.ToString("X2"));
      }


      this.RegisterWrite(Registers.RADIO_SELECT, 16); // Load chan_select firmware option 
      Thread.Sleep(1);
      this.RegisterWrite(Registers.RADIO_SELECT, 0);
      Thread.Sleep(1);
      status = this.RegisterRead(Registers.MCU_AGC_STATUS);
      if(status != 0x30) {
        throw new Exception("ERROR: AGC FIRMWARE INITIALIZATION FAILURE, STATUS " + status.ToString("X2"));
      }

      Byte radio_select = 0; // IF mapping to radio A/B (per bit, 0=A, 1=B) 
      for(Byte i = 0; i < 8; ++i) { // configure LoRa 'multi' demodulators aka. LoRa 'sensor' channels (IF0-3) 
        radio_select += (Byte)(this._interfaceChain[i] == Reciever.Chain1 ? 1 << i : 0); // transform bool array into binary word 
      }

      this.RegisterWrite(Registers.RADIO_SELECT, 16); // End AGC firmware init and check status 
      Thread.Sleep(1);
      this.RegisterWrite(Registers.RADIO_SELECT, radio_select); // Load intended value of RADIO_SELECT 
      Thread.Sleep(1);
      //Console.WriteLine("Info: putting back original RADIO_SELECT value");
      status = this.RegisterRead(Registers.MCU_AGC_STATUS);
      if(status != 0x40) {
        throw new Exception("ERROR: AGC FIRMWARE INITIALIZATION FAILURE, STATUS " + status.ToString("X2"));
      }


      this.RegisterWrite(Registers.GPS_EN, 1); // enable GPS event capture 


      if (this._lbt_enabled == true) {
        Console.WriteLine("INFO: Configuring LBT, this take 8.4 seconds, please wait...");
        Thread.Sleep(8400);
      }

      this._deviceStarted = true;
    }

    private void Reset() {
      this.SPIwriteRegisterRaw(0, 128);
      this.SPIwriteRegisterRaw(0, 0);
      Thread.Sleep(32); // provide at least 16 cycles on CLKHS and 16 cycles CLK32M
      this.SPIwriteRegisterRaw(18, 1);
      Thread.Sleep(42); // provide at least 4 cycles on CLKHS and 32 cycles CLK32M and 4 cycles on HOST_SCK
      this.SPIwriteRegisterRaw(18, 2);
      Thread.Sleep(8); // provide at least 4 cycles CLK32M and 4 cycles on HOST_SCK
      this.SPIwriteRegisterRaw(0, 128);
      this.SPIwriteRegisterRaw(0, 0);
      //this.PinReset.PinMode = GpioPinDriveMode.Input;
    }

    UInt32 TimeOnAir(SendingPacket packet) {
      if(packet.modulation == Modulation.Lora) {
        // Get bandwidth 
        UInt16 bw = packet.bandwidth switch
        {
          BW.BW_7K8HZ => 7800 / 1000,
          BW.BW_15K6HZ => 15600 / 1000,
          BW.BW_31K2HZ => 31200 / 1000,
          BW.BW_62K5HZ => 62500 / 1000,
          BW.BW_125KHZ => 125000 / 1000,
          BW.BW_250KHZ => 250000 / 1000,
          BW.BW_500KHZ => 500000 / 1000,
          _ => throw new Exception("ERROR: Cannot compute time on air for this packet, unsupported bandwidth (" + packet.bandwidth + ")")
        };
        // Get datarate
        Byte sf = packet.datarate_lora switch
        {
          SF.DR_LORA_SF7 => 7,
          SF.DR_LORA_SF8 => 8,
          SF.DR_LORA_SF9 => 9,
          SF.DR_LORA_SF10 => 10,
          SF.DR_LORA_SF11 => 11,
          SF.DR_LORA_SF12 => 12,
          _ => throw new Exception("ERROR: Cannot compute time on air for this packet, unsupported datarate (" + packet.datarate_lora + ")")
        };

        // Duration of 1 symbol 
        Double Tsym = Math.Pow(2, sf) / bw;

        // Duration of preamble 
        Double Tpreamble = (packet.preamble + 4.25) * Tsym;

        // Duration of payload 
        Byte h = (Byte)((packet.no_header == false) ? 0 : 1); // header is always enabled, except for beacons 
        Byte de = (Byte)((sf >= 11) ? 1 : 0); // Low datarate optimization enabled for SF11 and SF12 

        UInt32 payloadSymbNb = (UInt32)(8 + Math.Ceiling((8 * packet.payload.Length - 4 * sf + 28 + 16 - 20 * h) / (Double)(4 * (sf - 2 * de))) * ((Byte)packet.coderate + 4)); // Explicitely cast to double to keep precision of the division 

        Double Tpayload = payloadSymbNb * Tsym;

        // Duration of packet 
        return (UInt32)(Tpreamble + Tpayload);
      } else if(packet.modulation == Modulation.Fsk) {
        // PREAMBLE + SYNC_WORD + PKT_LEN + PKT_PAYLOAD + CRC
        //        PREAMBLE: default 5 bytes
        //        SYNC_WORD: default 3 bytes
        //        PKT_LEN: 1 byte (variable length mode)
        //        PKT_PAYLOAD: x bytes
        //        CRC: 0 or 2 bytes

        Double Tfsk = 8 * (Double)(packet.preamble + this._fskSyncWordSize + 1 + packet.payload.Length + ((packet.no_crc == true) ? 0 : 2)) / packet.datarate_fsk * 1E3;

        // Duration of packet 
        return (UInt32)Tfsk + 1; // add margin for rounding 
      } else {
        throw new Exception("ERROR: Cannot compute time on air for this packet, unsupported modulation (" + packet.modulation + ")");
      }
    }
    
    UInt16 GetTxStartDelay(Boolean tx_notch_enable, BW bw) {
      Single notch_delay_us = 0.0f;
      Single bw_delay_us = 0.0f;
      Single tx_start_delay;

      // Notch filtering performed by FPGA adds a constant delay (group delay) that we need to compensate 
      if(tx_notch_enable) {
        notch_delay_us = this.FpgaGetTxNotchDelay();
      }

      // Calibrated delay brought by SX1301 depending on signal bandwidth 
      switch(bw) {
        case BW.BW_125KHZ:
          bw_delay_us = 1.5f;
          break;
        case BW.BW_500KHZ:
        // Intended fall-through: it is the calibrated reference 
        default:
          break;
      }

      tx_start_delay = 1497 - bw_delay_us - notch_delay_us;

      //Console.WriteLine("INFO: tx_start_delay=" + (UInt16)tx_start_delay + " (" + tx_start_delay + ") - (" + 1497 + ", bw_delay=" + bw_delay_us + ", notch_delay=" + notch_delay_us + ")");

      return (UInt16)tx_start_delay; // keep truncating instead of rounding: better behaviour measured 
    }
    #endregion
    
    #region Recieve
    private void RecieverThreadRunner() {
      Console.WriteLine("Fraunhofer.Fit.Iot.Lora.lib.Ic880alora.ReceiveRunner(): gestartet!");
      while(this._recieverThreadRunning) {
        if(this._deviceStarted && !this._istransmitting) {
          lock(this.HandleControllerIOLock) {
            this.Receive();
          }
        }
        Thread.Sleep(1);
      }
    }

    private void Receive() {
      Byte[] recieveregister = this.RegisterReadArray(Registers.RX_PACKET_DATA_FIFO_NUM_STORED, 5);
      if(recieveregister[0] > 0 && recieveregister[0] <= 16) {
        // 0:   number of packets available in RX data buffer 
        // 1,2: start address of the current packet in RX data buffer 
        // 7 3:   CRC status of the current packet
        // 7 4:   size of the current packet payload in byte
        //Console.WriteLine("FIFO content: " + recieveregister[0] + " " + recieveregister[1] + " " + recieveregister[2] + " " + recieveregister[3] + " " + recieveregister[4]);

        Ic880aRecievedObj p = new Ic880aRecievedObj();
        Byte sz = recieveregister[4];
        Byte stat_fifo = recieveregister[3];

        Byte[] buff2 = this.RegisterReadArray(Registers.RX_DATA_BUF_DATA, (UInt16)(sz + 16));
        p.Data = new Byte[sz];
        for(Byte i = 0; i < sz; i++) {
          p.Data[i] = buff2[i];
        }
        //Console.WriteLine("Text: " + p.Data);
        p.Interface = buff2[sz + 0];
        if(p.Interface >= 10) {
          Helper.WriteError("WARNING: " + p.Interface + " NOT A VALID IF_CHAIN NUMBER, ABORTING");
          return;
        }

        RadioDataType ifmod = RadioDataType.Undefined;
        if(p.Interface < 8 && p.Interface >= 0) {
          ifmod = RadioDataType.LoraMulti;
        } else if(p.Interface == 8) {
          ifmod = RadioDataType.Lora;
        } else if(p.Interface == 9) {
          ifmod = RadioDataType.FSK;
        }
        //Console.WriteLine("["+ p.if_chain + " "+ ifmod + "]");

        p.Radio = Ic880aRecievedObj.ParseRadio(this._interfaceChain[p.Interface]);
        p.Frequency = (UInt32)((Int32)this._radioFrequency[p.Radio] + this._interfaceFrequency[p.Interface]);
        p.Rssi = buff2[sz + 5] + this._rf_rssi_offset[p.Radio];

        Boolean crc_en = false;
        UInt32 timestamp_correction;
        if(ifmod == RadioDataType.LoraMulti || ifmod == RadioDataType.Lora) {
          //Console.WriteLine("Note: LoRa packet\n");
          p.CrcStatus = Ic880aRecievedObj.ParseCrcStatus(Crc.CrcUndefined);
          switch(stat_fifo & 0x07) {
            case 5:
              p.CrcStatus = Ic880aRecievedObj.ParseCrcStatus(Crc.CrcOk);
              crc_en = true;
              break;
            case 7:
              p.CrcStatus = Ic880aRecievedObj.ParseCrcStatus(Crc.CrcBad);
              crc_en = true;
              break;
            case 1:
              p.CrcStatus = Ic880aRecievedObj.ParseCrcStatus(Crc.CrcNo);
              crc_en = false;
              break;
          }
          p.Modulation = Ic880aRecievedObj.ParseModulation(Modulation.Lora);
          p.Snr = (Single)(SByte)buff2[sz + 2] / 4;
          p.SnrMin = (Single)(SByte)buff2[sz + 3] / 4;
          p.SnrMax = (Single)(SByte)buff2[sz + 4] / 4;
          p.Bandwidth = Ic880aRecievedObj.ParseBandwidth(ifmod == RadioDataType.LoraMulti ? BW.BW_125KHZ : this._loraBandwidth);
          UInt32 sf = (UInt32)((buff2[sz + 1] >> 4) & 0x0F);
          switch(sf) {
            case 7:
              p.Spreadingfactor = Ic880aRecievedObj.ParseSpreadingFactor(SF.DR_LORA_SF7);
              break;
            case 8:
              p.Spreadingfactor = Ic880aRecievedObj.ParseSpreadingFactor(SF.DR_LORA_SF8);
              break;
            case 9:
              p.Spreadingfactor = Ic880aRecievedObj.ParseSpreadingFactor(SF.DR_LORA_SF9);
              break;
            case 10:
              p.Spreadingfactor = Ic880aRecievedObj.ParseSpreadingFactor(SF.DR_LORA_SF10);
              break;
            case 11:
              p.Spreadingfactor = Ic880aRecievedObj.ParseSpreadingFactor(SF.DR_LORA_SF11);
              break;
            case 12:
              p.Spreadingfactor = Ic880aRecievedObj.ParseSpreadingFactor(SF.DR_LORA_SF12);
              break;
          }
          UInt32 cr = (UInt32)((buff2[sz + 1] >> 1) & 0x07);
          switch(cr) {
            case 1:
              p.CodingRate = Ic880aRecievedObj.ParseCodingRate(CR.CR_LORA_4_5);
              break;
            case 2:
              p.CodingRate = Ic880aRecievedObj.ParseCodingRate(CR.CR_LORA_4_6);
              break;
            case 3:
              p.CodingRate = Ic880aRecievedObj.ParseCodingRate(CR.CR_LORA_4_7);
              break;
            case 4:
              p.CodingRate = Ic880aRecievedObj.ParseCodingRate(CR.CR_LORA_4_8);
              break;
          }

          // determine if 'PPM mode' is on, needed for timestamp correction 
          Boolean ppm = p.Bandwidth == Ic880aRecievedObj.ParseBandwidth(BW.BW_125KHZ) && (p.Spreadingfactor == Ic880aRecievedObj.ParseSpreadingFactor(SF.DR_LORA_SF11) || p.Spreadingfactor == Ic880aRecievedObj.ParseSpreadingFactor(SF.DR_LORA_SF12)) || 
            p.Bandwidth == Ic880aRecievedObj.ParseBandwidth(BW.BW_250KHZ) && p.Spreadingfactor == Ic880aRecievedObj.ParseSpreadingFactor(SF.DR_LORA_SF12);

          UInt32 delay_x = 0;
          UInt32 bw_pow = 0;
          // timestamp correction code, base delay 
          if(ifmod == RadioDataType.Lora) { // if packet was received on the stand-alone LoRa modem 
            switch(this._loraBandwidth) {
              case BW.BW_125KHZ:
                delay_x = 64;
                bw_pow = 1;
                break;
              case BW.BW_250KHZ:
                delay_x = 32;
                bw_pow = 2;
                break;
              case BW.BW_500KHZ:
                delay_x = 16;
                bw_pow = 4;
                break;
            }
          } else { // packet was received on one of the sensor channels = 125kHz 
            delay_x = 114;
            bw_pow = 1;
          }

          // timestamp correction code, variable delay 
          if(sf >= 6 && sf <= 12 && bw_pow > 0) {
            UInt32 delay_y;
            UInt32 delay_z;
            if(2 * (sz + 2 * (crc_en ? 1 : 0)) - (sf - 7) <= 0) { // payload fits entirely in first 8 symbols 
              delay_y = (UInt32)(((1 << ((Int32)sf - 1)) * (sf + 1) + 3 * (1 << ((Int32)sf - 4))) / bw_pow);
              delay_z = (UInt32)(32 * (2 * (sz + 2 * (crc_en ? 1 : 0)) + 5) / bw_pow);
            } else {
              delay_y = (UInt32)(((1 << ((Int32)sf - 1)) * (sf + 1) + (4 - (ppm ? 1 : 0)) * (1 << ((Int32)sf - 4))) / bw_pow);
              delay_z = (UInt32)((16 + 4 * cr) * ((2 * (sz + 2 * (crc_en ? 1 : 0)) - sf + 6) % (sf - 2 * (ppm ? 1 : 0)) + 1) / bw_pow);
            }
            timestamp_correction = delay_x + delay_y + delay_z;
          } else {
            timestamp_correction = 0;
            Helper.WriteError("WARNING: invalid packet, no timestamp correction");
          }

          // RSSI correction 
          if(ifmod == RadioDataType.LoraMulti) {
            p.Rssi -= -35;
          }

        } else if(ifmod == RadioDataType.FSK) {
          //Console.WriteLine("Note: FSK packet");
          switch(stat_fifo & 0x07) {
            case 5:
              p.CrcStatus = Ic880aRecievedObj.ParseCrcStatus(Crc.CrcOk);
              break;
            case 7:
              p.CrcStatus = Ic880aRecievedObj.ParseCrcStatus(Crc.CrcBad);
              break;
            case 1:
              p.CrcStatus = Ic880aRecievedObj.ParseCrcStatus(Crc.CrcNo);
              break;
          }
          p.Modulation = Ic880aRecievedObj.ParseModulation(Modulation.Fsk);
          p.Snr = -128;
          p.SnrMin = -128;
          p.SnrMax = -128;
          p.Bandwidth = Ic880aRecievedObj.ParseBandwidth(this._fskBandwidth);
          //p.datarate = this.fsk_rx_dr;
          timestamp_correction = 680000 / this._fskDatarate - 20;

          // RSSI correction 
          p.Rssi = (Single)(60 + 1.5351 * p.Rssi + 0.003 * Math.Pow(p.Rssi, 2));
        } else {
          Helper.WriteError("ERROR: UNEXPECTED PACKET ORIGIN");
          p.Rssi = -128;
          p.Snr = -128;
          p.SnrMin = -128;
          p.SnrMax = -128;
          timestamp_correction = 0;
        }

        UInt32 raw_timestamp = buff2[sz + 6] + ((UInt32)buff2[sz + 7] << 8) + ((UInt32)buff2[sz + 8] << 16) + ((UInt32)buff2[sz + 9] << 24);
        p.Time = raw_timestamp - timestamp_correction;
        p.Calculatedcrc = (UInt16)(buff2[sz + 10] + (buff2[sz + 11] << 8));

        if(p.CrcStatus == "Ok" && this._CrcEnabled || !this._CrcEnabled) {
          p.Crc = true;
          this.RaiseRecieveEvent(p);
          //Console.WriteLine(p.ToString());
        }
        this.RegisterWrite(Registers.RX_PACKET_DATA_FIFO_NUM_STORED, 0);
      }


    }
    #endregion

    #region Transmit
    private void Transmit(SendingPacket pkt_data) {
      // check input range (segfault prevention) 
      if (pkt_data.rf_chain >= 2) {
        throw new Exception("ERROR: INVALID RF_CHAIN TO SEND PACKETS");
      }

      // check input variables 
      if (this._radioEnableTx[pkt_data.rf_chain] == false) {
        throw new Exception("ERROR: SELECTED RF_CHAIN IS DISABLED FOR TX ON SELECTED BOARD");
      }
      if (this._radioEnabled[pkt_data.rf_chain] == false) {
        throw new Exception("ERROR: SELECTED RF_CHAIN IS DISABLED");
      }
      if (pkt_data.tx_mode != SendingMode.IMMEDIATE && pkt_data.tx_mode != SendingMode.TIMESTAMPED && pkt_data.tx_mode != SendingMode.ON_GPS) {
        throw new Exception("ERROR: TX_MODE NOT SUPPORTED");
      }
      if (pkt_data.modulation == Modulation.Lora) {
        if (pkt_data.bandwidth != BW.BW_125KHZ && pkt_data.bandwidth != BW.BW_250KHZ && pkt_data.bandwidth != BW.BW_500KHZ) {
          throw new Exception("ERROR: BANDWIDTH NOT SUPPORTED BY LORA TX");
        }
        if (pkt_data.datarate_lora != SF.DR_LORA_SF7 && pkt_data.datarate_lora != SF.DR_LORA_SF8 && pkt_data.datarate_lora != SF.DR_LORA_SF9 && pkt_data.datarate_lora != SF.DR_LORA_SF10 && pkt_data.datarate_lora != SF.DR_LORA_SF11 && pkt_data.datarate_lora != SF.DR_LORA_SF12) {
          throw new Exception("ERROR: DATARATE NOT SUPPORTED BY LORA TX");
        }
        if (pkt_data.coderate != CR.CR_LORA_4_5 && pkt_data.coderate != CR.CR_LORA_4_6 && pkt_data.coderate != CR.CR_LORA_4_7 && pkt_data.coderate != CR.CR_LORA_4_8) {
          throw new Exception("ERROR: CODERATE NOT SUPPORTED BY LORA TX");
        }
        if (pkt_data.payload.Length > 255) {
          throw new Exception("ERROR: PAYLOAD LENGTH TOO BIG FOR LORA TX");
        }
      } else if (pkt_data.modulation == Modulation.Fsk) {
        if (pkt_data.f_dev < 1 || pkt_data.f_dev > 200) {
          throw new Exception("ERROR: TX FREQUENCY DEVIATION OUT OF ACCEPTABLE RANGE");
        }
        if (!(pkt_data.datarate_fsk >= 500 && pkt_data.datarate_fsk <= 250000)) {
          throw new Exception("ERROR: DATARATE NOT SUPPORTED BY FSK IF CHAIN");
        }
        if (pkt_data.payload.Length > 255) {
          throw new Exception("ERROR: PAYLOAD LENGTH TOO BIG FOR FSK TX");
        }
      } else {
        throw new Exception("ERROR: INVALID TX MODULATION");
      }

      // Enable notch filter for LoRa 125kHz 
      Boolean tx_notch_enable = pkt_data.modulation == Modulation.Lora && pkt_data.bandwidth == BW.BW_125KHZ;

      // Get the TX start delay to be applied for this TX 
      UInt16 tx_start_delay = this.GetTxStartDelay(tx_notch_enable, pkt_data.bandwidth);

      // interpretation of TX power 
      Int32 pow_index; // 4-bit value to set the firmware TX power 
      for (pow_index = this._lut.Length - 1; pow_index > 0; pow_index--) {
        if (this._lut[pow_index].rf_power <= pkt_data.rf_power) {
          break;
        }
      }

      // loading TX imbalance correction 
      Byte target_mix_gain = this._lut[pow_index].mix_gain; // used to select the proper I/Q offset correction 
      if (pkt_data.rf_chain == 0) { // use radio A calibration table 
        this.RegisterWrite(Registers.TX_OFFSET_I, this._cal_offset_a_i[target_mix_gain - 8]);
        this.RegisterWrite(Registers.TX_OFFSET_Q, this._cal_offset_a_q[target_mix_gain - 8]);
      } else { // use radio B calibration table 
        this.RegisterWrite(Registers.TX_OFFSET_I, this._cal_offset_b_i[target_mix_gain - 8]);
        this.RegisterWrite(Registers.TX_OFFSET_Q, this._cal_offset_b_q[target_mix_gain - 8]);
      }

      // Set digital gain from LUT 
      this.RegisterWrite(Registers.TX_GAIN, this._lut[pow_index].dig_gain);

      // fixed metadata, useful payload and misc metadata compositing 
      Int32 transfer_size = 16 + pkt_data.payload.Length;  // data to transfer from host to TX databuffer 
      Int32 payload_offset = 16; // start the payload just after the metadata // start of the payload content in the databuffer 

      // metadata 0 to 2, TX PLL frequency 
      UInt32 part_int; // integer part for PLL register value calculation 
      UInt32 part_frac; // fractional part for PLL register value calculation 
      switch (this._rf_radio_type[0]) { // we assume that there is only one radio type on the board 
        case RadioType.SX1255:
          part_int = pkt_data.freq_hz / (15625 << 7); // integer part, gives the MSB 
          part_frac = ((pkt_data.freq_hz % (15625 << 7)) << 9) / 15625; // fractional part, gives middle part and LSB 
          break;
        case RadioType.SX1257:
          part_int = pkt_data.freq_hz / (15625 << 8); // integer part, gives the MSB 
          part_frac = ((pkt_data.freq_hz % (15625 << 8)) << 8) / 15625; // fractional part, gives middle part and LSB 
          break;
        default:
          throw new Exception("ERROR: UNEXPECTED VALUE " + this._rf_radio_type[0] + " FOR RADIO TYPE");
      }

      Byte[] buff = new Byte[pkt_data.payload.Length + (pkt_data.modulation == Modulation.Lora ? 16 : 17)]; // buffer to prepare the packet to send + metadata before SPI write burst 
      buff[0] = (Byte)(0xFF & part_int); // Most Significant Byte 
      buff[1] = (Byte)(0xFF & (part_frac >> 8)); // middle byte 
      buff[2] = (Byte)(0xFF & part_frac); // Least Significant Byte 

      // metadata 3 to 6, timestamp trigger value 
      // TX state machine must be triggered at (T0 - lgw_i_tx_start_delay_us) for packet to start being emitted at T0 
      UInt32 count_trig; // timestamp value in trigger mode corrected for TX start delay 
      if (pkt_data.tx_mode == SendingMode.TIMESTAMPED) {
        count_trig = pkt_data.count_us - tx_start_delay;
        buff[3] = (Byte)(0xFF & (count_trig >> 24));
        buff[4] = (Byte)(0xFF & (count_trig >> 16));
        buff[5] = (Byte)(0xFF & (count_trig >> 8));
        buff[6] = (Byte)(0xFF & count_trig);
      }

      // parameters depending on modulation  
      if (pkt_data.modulation == Modulation.Lora) {
        // metadata 7, modulation type, radio chain selection and TX power 
        buff[7] = (Byte)((0x20 & (pkt_data.rf_chain << 5)) | (0x0F & pow_index)); // bit 4 is 0 -> LoRa modulation 

        buff[8] = 0; // metadata 8, not used 

        // metadata 9, CRC, LoRa CR & SF 
        buff[9] = pkt_data.datarate_lora switch
        {
          SF.DR_LORA_SF7 => 7,
          SF.DR_LORA_SF8 => 8,
          SF.DR_LORA_SF9 => 9,
          SF.DR_LORA_SF10 => 10,
          SF.DR_LORA_SF11 => 11,
          SF.DR_LORA_SF12 => 12,
          _ => throw new Exception("ERROR: UNEXPECTED VALUE " + pkt_data.datarate_lora + " IN SWITCH STATEMENT"),
        };
        buff[9] |= pkt_data.coderate switch
        {
          CR.CR_LORA_4_5 => (Byte)(1 << 4),
          CR.CR_LORA_4_6 => (Byte)(2 << 4),
          CR.CR_LORA_4_7 => (Byte)(3 << 4),
          CR.CR_LORA_4_8 => (Byte)(4 << 4),
          _ => throw new Exception("ERROR: UNEXPECTED VALUE " + pkt_data.coderate + " IN SWITCH STATEMENT"),
        };
        if (pkt_data.no_crc == false) {
          buff[9] |= 0x80; // set 'CRC enable' bit 
        } else {
          Console.WriteLine("Info: packet will be sent without CRC");
        }

        // metadata 10, payload size 
        buff[10] = (Byte)pkt_data.payload.Length;

        // metadata 11, implicit header, modulation bandwidth, PPM offset & polarity 
        buff[11] = pkt_data.bandwidth switch
        {
          BW.BW_125KHZ => 0,
          BW.BW_250KHZ => 1,
          BW.BW_500KHZ => 2,
          _ => throw new Exception("ERROR: UNEXPECTED VALUE " + pkt_data.bandwidth + " IN SWITCH STATEMENT"),
        };
        if (pkt_data.no_header == true) {
          buff[11] |= 0x04; // set 'implicit header' bit 
        }
        if (pkt_data.bandwidth == BW.BW_125KHZ && (pkt_data.datarate_lora == SF.DR_LORA_SF11 || pkt_data.datarate_lora == SF.DR_LORA_SF12) || pkt_data.bandwidth == BW.BW_250KHZ && pkt_data.datarate_lora == SF.DR_LORA_SF12) {
          buff[11] |= 0x08; // set 'PPM offset' bit at 1 
        }
        if (pkt_data.invert_pol == true) {
          buff[11] |= 0x10; // set 'TX polarity' bit at 1 
        }

        // metadata 12 & 13, LoRa preamble size 
        if (pkt_data.preamble == 0) { // if not explicit, use recommended LoRa preamble size 
          pkt_data.preamble = 8;
        } else if (pkt_data.preamble < 6) { // enforce minimum preamble size 
          pkt_data.preamble = 6;
          Console.WriteLine("Note: preamble length adjusted to respect minimum LoRa preamble size");
        }
        buff[12] = (Byte)(0xFF & (pkt_data.preamble >> 8));
        buff[13] = (Byte)(0xFF & pkt_data.preamble);

        // metadata 14 & 15, not used 
        buff[14] = 0;
        buff[15] = 0;

        // MSB of RF frequency is now used in AGC firmware to implement large/narrow filtering in SX1257/55 
        buff[0] &= 0x3F; // Unset 2 MSBs of frequency code 
        if (pkt_data.bandwidth == BW.BW_500KHZ) {
          buff[0] |= 0x80; // Set MSB bit to enlarge analog filter for 500kHz BW 
        }

        // Set MSB-1 bit to enable digital filter if required 
        if (tx_notch_enable == true) {
          //Console.WriteLine("INFO: Enabling TX notch filter");
          buff[0] |= 0x40;
        }
      } else if (pkt_data.modulation == Modulation.Fsk) {
        // metadata 7, modulation type, radio chain selection and TX power 
        buff[7] = (Byte)((0x20 & (pkt_data.rf_chain << 5)) | 0x10 | (0x0F & pow_index)); // bit 4 is 1 -> FSK modulation 

        buff[8] = 0; // metadata 8, not used 

        // metadata 9, frequency deviation 
        buff[9] = pkt_data.f_dev;

        // metadata 10, payload size 
        buff[10] = (Byte)pkt_data.payload.Length;
        // TODO: how to handle 255 bytes packets ?!? 

        // metadata 11, packet mode, CRC, encoding 
        buff[11] = (Byte)(0x01 | (pkt_data.no_crc ? 0 : 0x02) | (0x02 << 2)); // always in variable length packet mode, whitening, and CCITT CRC if CRC is not disabled  

        // metadata 12 & 13, FSK preamble size 
        if (pkt_data.preamble == 0) { // if not explicit, use LoRa MAC preamble size 
          pkt_data.preamble = 5;
        } else if (pkt_data.preamble < 3) { // enforce minimum preamble size 
          pkt_data.preamble = 3;
          Console.WriteLine("Note: preamble length adjusted to respect minimum FSK preamble size");
        }
        buff[12] = (Byte)(0xFF & (pkt_data.preamble >> 8));
        buff[13] = (Byte)(0xFF & pkt_data.preamble);

        // metadata 14 & 15, FSK baudrate
        UInt16 fsk_dr_div = (UInt16)(32000000 / pkt_data.datarate_fsk); // Ok for datarate between 500bps and 250kbps  // divider to configure for target datarate 
        buff[14] = (Byte)(0xFF & (fsk_dr_div >> 8));
        buff[15] = (Byte)(0xFF & fsk_dr_div);

        // insert payload size in the packet for variable mode 
        buff[16] = (Byte)pkt_data.payload.Length;
        ++transfer_size; // one more byte to transfer to the TX modem 
        ++payload_offset; // start the payload with one more byte of offset 

        // MSB of RF frequency is now used in AGC firmware to implement large/narrow filtering in SX1257/55 
        buff[0] &= 0x7F; // Always use narrow band for FSK (force MSB to 0) 

      } else {
        throw new Exception("ERROR: INVALID TX MODULATION..");
      }

      // Configure TX start delay based on TX notch filter 
      this.RegisterWrite(Registers.TX_START_DELAY, tx_start_delay);

      // copy payload from user struct to buffer containing metadata 
      for (Int32 i = 0; i < pkt_data.payload.Length; i++) {
        buff[i + payload_offset] = pkt_data.payload[i];
      }
      if (buff.Length != transfer_size) {
        throw new Exception("Payload size not match!");
      }

      // reset TX command flags 
      this.RegisterWrite(Registers.TX_TRIG_ALL, 0);

      // put metadata + payload in the TX data buffer 
      this.RegisterWrite(Registers.TX_DATA_BUF_ADDR, 0);
      this.RegisterWriteArray(Registers.TX_DATA_BUF_DATA, buff);
      //DEBUG_ARRAY(i, transfer_size, buff);

      Boolean tx_allowed = this.LbtIsChannelFree(pkt_data, tx_start_delay);
      DateTime start = DateTime.Now;
      if (tx_allowed == true) {
        switch (pkt_data.tx_mode) {
          case SendingMode.IMMEDIATE:
            this.RegisterWrite(Registers.TX_TRIG_IMMEDIATE, 1);
            break;

          case SendingMode.TIMESTAMPED:
            this.RegisterWrite(Registers.TX_TRIG_DELAYED, 1);
            break;

          case SendingMode.ON_GPS:
            this.RegisterWrite(Registers.TX_TRIG_GPS, 1);
            break;

          default:
            throw new Exception("ERROR: UNEXPECTED VALUE " + pkt_data.tx_mode + " IN SWITCH STATEMENT");
        }
      } else {
        throw new Exception("ERROR: Cannot send packet, channel is busy (LBT)");
      }
      UInt16 j = 0;
      Int32 reg;
      do {
        Thread.Sleep(1);
        reg = this.RegisterRead(Registers.TX_STATUS);
        //Console.Write(reg + ":");
        j++;
      } while((reg & 0x10) != 0 && j < 1000);
      this._dataRate = pkt_data.payload.Length * 8.0 / ((DateTime.Now - start).TotalMilliseconds / 1000);
      this._SendTimeout = j >= 1000;
      //this.RegisterWrite(Registers.RX_PACKET_DATA_FIFO_NUM_STORED, 0);
      //Console.WriteLine("TX, done.");
    }

    private SendingPacket PrepareSend(Byte[] data, Byte @interface) => new SendingPacket {
      bandwidth = this._loraSendBandwidth,
      coderate = this._loraSendCodeRate,
      count_us = 0,
      datarate_fsk = 0,
      datarate_lora = this._loraSendSpreadingFactor,
      freq_hz = (UInt32)(this._radioFrequency[(Byte)this._interfaceChain[@interface]] + this._interfaceFrequency[@interface]),
      f_dev = 0,
      invert_pol = false,
      modulation = Modulation.Lora,
      no_crc = false,
      no_header = false,
      payload = data,
      preamble = this._loraSendPreeamble,
      rf_chain = 0,
      rf_power = 10,
      tx_mode = SendingMode.IMMEDIATE
    };
    #endregion
    
    private void SetupIO() {
      this.PinReset.PinMode = GpioPinDriveMode.Output;
      this.PinReset.Write(GpioPinValue.High);
      Console.WriteLine("Reset Hardware!");
      Thread.Sleep(1000);
      this.PinReset.Write(GpioPinValue.Low);
      Thread.Sleep(1000);
      this.PinChipSelect.PinMode = GpioPinDriveMode.Output;
      Pi.Spi.SetProperty("Channel" + this.SpiChannel.Channel.ToString() + "Frequency", 100000.ToString());
    }
  }
}
