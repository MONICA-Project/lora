/*
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

namespace Fraunhofer.Fit.Iot.Lora.lib {
  public partial class Ic880alora : LoraConnector {
    #region Private Variables
    private readonly GpioPin PinSlaveSelect;
    private readonly GpioPin PinReset;
    private readonly SpiChannel SpiChannel;
    private Thread receiveThread;
    private Boolean ReceiveRunnerAlive;

    #region Sending Correction
    private readonly SByte[] cal_offset_a_i = new SByte[8];
    private readonly SByte[] cal_offset_a_q = new SByte[8];
    private readonly SByte[] cal_offset_b_i = new SByte[8];
    private readonly SByte[] cal_offset_b_q = new SByte[8];
    #endregion

    #region RadioConstants
    private readonly Boolean[] radioEnableTx = new Boolean[] { false, false };
    private readonly Byte fskSyncWordSize = 3;
    private readonly UInt64 fskSyncWord = 0xC194C1;
    #endregion

    #region Running Properties
    private readonly Boolean[] radioEnabled = new Boolean[2];
    private readonly UInt32[] radioFrequency = new UInt32[2];
    private readonly Boolean[] interfaceEnabled = new Boolean[10];
    private readonly Reciever[] interfaceChain = new Reciever[10];
    private readonly Int32[] interfaceFrequency = new Int32[10];
    private BW fskBandwidth = BW.BW_125KHZ;
    private UInt32 fskDatarate = 50000;
    private BW loraBandwidth = BW.BW_250KHZ;
    private SF loraSpreadingFactor = SF.DR_LORA_SF7;
    private Byte selectedPage;
    private Boolean deviceStarted;
    private Boolean CrcEnabled;
    
    #endregion
    #endregion

    #region Registers, Modes, Pa, Irq
    public enum BW {
      Undefined = 0,
      BW_500KHZ = 1,
      BW_250KHZ = 2,
      BW_125KHZ = 3,
      BW_62K5HZ = 4,
      BW_31K2HZ = 5,
      BW_15K6HZ = 6,
      BW_7K8HZ = 7      
    }

    public enum SF : Byte {
      Undefined = 0,
      DR_LORA_SF7 = 2,
      DR_LORA_SF8 = 4,
      DR_LORA_SF9 = 8,
      DR_LORA_SF10 = 16,
      DR_LORA_SF11 = 32,
      DR_LORA_SF12 = 64,
      DR_LORA_SFMULTI = 126
    }

    public enum CR : Byte {
      Undefined = 0,
      CR_LORA_4_5 = 1,
      CR_LORA_4_6 = 2,
      CR_LORA_4_7 = 3,
      CR_LORA_4_8 = 4
    }

    public enum Reciever : Byte {
      Chain0 = 0,
      Chain1 = 1
    }

    enum RadioType : Byte {
      SX1255 = 0,
      SX1257 = 1
    }

    public enum RadioDataType : Byte {
      Undefined = 0,
      Lora = 16,
      LoraMulti = 17,
      FSK = 32
    }

    public enum Modulation : Byte {
      Undefined = 0,
      Lora = 0x10,
      Fsk = 0x20
    }

    public enum Crc : Byte {
      CrcOk = 0x10,
      CrcBad = 0x11,
      CrcNo = 0x01,
      CrcUndefined = 0x00
    }

    public static class Txgain_lut {
      public static Byte size = 2;
      public static Lutstruct[] lut = new Lutstruct[2] { new Lutstruct(0,2,3,10,14), new Lutstruct(0,3,3,14,27) };
    }

    public struct Lutstruct {
      public Byte dig_gain;
      public Byte pa_gain;
      public Byte dac_gain;
      public Byte mix_gain;
      public Byte rf_power;
      public Lutstruct(Byte dig_gain, Byte pa_gain, Byte dac_gain, Byte mix_gain, Byte rf_power) {
        this.dig_gain = dig_gain;
        this.pa_gain = pa_gain;
        this.dac_gain = dac_gain;
        this.mix_gain = mix_gain;
        this.rf_power = rf_power;
      }
    }
    #endregion

    #region Constructor
    public Ic880alora(Dictionary<String, String> settings) : base(settings) {
      Pi.Init<BootstrapWiringPi>();
      this.SpiChannel = (SpiChannel)Pi.Spi.GetProperty(this.config["spichan"]);
      this.PinSlaveSelect = (GpioPin)Pi.Gpio.GetProperty(this.config["pin_sspin"]);  //Physical pin 24, BCM pin  8, Wiring Pi pin 10 (SPI0 CE0)
      this.PinReset = (GpioPin)Pi.Gpio.GetProperty(this.config["pin_rst"]);          //Physical pin 29, BCM pin  5, Wiring Pi pin 21 (GPCLK1)
    }

    public override Boolean Begin() {
      this.SetupIO();
      this.Reset();
      if (this.RegisterRead(Registers.VERSION) != Registers.VERSION.DefaultValue) {
        Helper.WriteError("Register VERSION did not match!");
        return false;
      }
      this.PageSwitch(0);
      _ = this.RegisterWrite(Registers.SOFT_RESET, 1); //reset the registers (also shuts the radios down)
      this.receiveThread = new Thread(this.ReceiveRunner);
      return true;
    }

    public override void End() {
      this.deviceStarted = false;
      _ = this.RegisterWrite(Registers.SOFT_RESET, 1); //reset the registers (also shuts the radios down)
    }

    public override void Dispose() {
      this.ReceiveRunnerAlive = false;
      while(this.receiveThread.IsAlive) {
        Thread.Sleep(10);
      }
      this.receiveThread = null;
    }

    public override Boolean StartRadio() {
      _ = this.RegisterWrite(Registers.GLOBAL_EN, 0); //gate clocks
      _ = this.RegisterWrite(Registers.CLK32M_EN, 0);

      _ = this.RegisterWrite(Registers.RADIO_A_EN, 1); //switch on and reset the radios (also starts the 32 MHz XTAL)
      _ = this.RegisterWrite(Registers.RADIO_B_EN, 1);
      Thread.Sleep(500);
      _ = this.RegisterWrite(Registers.RADIO_RST, 1);
      Thread.Sleep(5);
      _ = this.RegisterWrite(Registers.RADIO_RST, 0);

      _ = this.Sx125xSetup(0, 1, this.radioEnabled[0], RadioType.SX1257, this.radioFrequency[0]);
      _ = this.Sx125xSetup(1, 1, this.radioEnabled[1], RadioType.SX1257, this.radioFrequency[1]);

      _ = this.RegisterWrite(Registers.GPIO_MODE, 31); /* gives AGC control of GPIOs to enable Tx external digital filter */
      _ = this.RegisterWrite(Registers.GPIO_SELECT_OUTPUT, 0); /* Set all GPIOs as output */

      //TODO Lib part for LBT (Listen before Talk)

      _ = this.RegisterWrite(Registers.GLOBAL_EN, 1); /* Enable clocks */
      _ = this.RegisterWrite(Registers.CLK32M_EN, 1);

      Byte cal_cmd = 0; /* select calibration command */
      cal_cmd |= this.radioEnabled[0] ? (Byte)0x01 : (Byte)0x00; /* Bit 0: Calibrate Rx IQ mismatch compensation on radio A */
      cal_cmd |= this.radioEnabled[1] ? (Byte)0x02 : (Byte)0x00; /* Bit 1: Calibrate Rx IQ mismatch compensation on radio B */
      cal_cmd |= (this.radioEnabled[0] && this.radioEnableTx[0]) ? (Byte)0x04 : (Byte)0x00; /* Bit 2: Calibrate Tx DC offset on radio A */
      cal_cmd |= (this.radioEnabled[1] && this.radioEnableTx[1]) ? (Byte)0x08 : (Byte)0x00; /* Bit 3: Calibrate Tx DC offset on radio B */
      cal_cmd |= 0x10; /* Bit 4: 0: calibrate with DAC gain=2, 1: with DAC gain=3 (use 3) */

      //switch (this.rf_radio_type[0]) { /* we assume that there is only one radio type on the board */
      //  case RadioType.SX1255: //LGW_RADIO_TYPE_SX1255:
      //    cal_cmd |= 0x20; /* Bit 5: 0: SX1257, 1: SX1255 */
      //    break;
      //  case RadioType.SX1257: //LGW_RADIO_TYPE_SX1257:
          cal_cmd |= 0x00; /* Bit 5: 0: SX1257, 1: SX1255 */
      //    break;
      //}

      cal_cmd |= 0x00; /* Bit 6-7: Board type 0: ref, 1: FPGA, 3: board X */
      UInt16 cal_time = 2300; /* measured between 2.1 and 2.2 sec, because 1 TX only */

      _ = this.LoadFirmware(Firmware.CAL); /* Load the calibration firmware  */
      _ = this.RegisterWrite(Registers.FORCE_HOST_RADIO_CTRL, 0); /* gives to AGC MCU the control of the radios */
      _ = this.RegisterWrite(Registers.RADIO_SELECT, cal_cmd); /* send calibration configuration word */
      _ = this.RegisterWrite(Registers.MCU_RST_1, 0);

      _ = this.RegisterWrite(Registers.DBG_AGC_MCU_RAM_ADDR, Firmware.CAL.Address); /* Check firmware version */
      Int32 fw_version = this.RegisterRead(Registers.DBG_AGC_MCU_RAM_DATA);
      if (fw_version != Firmware.CAL.Version) {
        throw new Exception("ERROR: Version of calibration firmware not expected, actual: " + fw_version + " expected: " + Firmware.CAL.Version);
      }

      _ = this.RegisterWrite(Registers.PAGE_REG, 3); /* Calibration will start on this condition as soon as MCU can talk to concentrator registers */
      _ = this.RegisterWrite(Registers.EMERGENCY_FORCE_HOST_CTRL, 0); /* Give control of concentrator registers to MCU */

      //Console.WriteLine("Note: calibration started (time: "+ cal_time + " ms)"); /* Wait for calibration to end */
      Thread.Sleep(cal_time); /* Wait for end of calibration */
      _ = this.RegisterWrite(Registers.EMERGENCY_FORCE_HOST_CTRL, 1); /* Take back control */

      Int32 cal_status = this.RegisterRead(Registers.MCU_AGC_STATUS);
      if ((cal_status & 0x81) != 0x81) {  //bit 0: could access SX1301 registers
        throw new Exception("ERROR: CALIBRATION FAILURE (STATUS = " + cal_status + ")");
      } else { //bit 7: calibration finished
        //Console.WriteLine("Note: calibration finished (status = "+ cal_status + ")");
      }
      if (this.radioEnabled[0] && (cal_status & 0x02) == 0) { //bit 1: could access radio A registers
        throw new Exception("WARNING: calibration could not access radio A\n");
      }
      if (this.radioEnabled[1] && (cal_status & 0x04) == 0) { //bit 2: could access radio B registers
        throw new Exception("WARNING: calibration could not access radio B\n");
      }
      if (this.radioEnabled[0] && (cal_status & 0x08) == 0) { //bit 3: radio A RX image rejection successful
        throw new Exception("WARNING: problem in calibration of radio A for image rejection\n");
      }
      if (this.radioEnabled[1] && (cal_status & 0x10) == 0) { //bit 4: radio B RX image rejection successful
        throw new Exception("WARNING: problem in calibration of radio B for image rejection\n");
      }
      if (this.radioEnabled[0] && this.radioEnableTx[0] && (cal_status & 0x20) == 0) { //bit 5: radio A TX DC Offset correction successful
        throw new Exception("WARNING: problem in calibration of radio A for TX DC offset\n");
      }
      if (this.radioEnabled[1] && this.radioEnableTx[1] && (cal_status & 0x40) == 0) { //bit 6: radio B TX DC Offset correction successful
        throw new Exception("WARNING: problem in calibration of radio B for TX DC offset\n");
      }

      for (Byte i = 0; i <= 7; ++i) { /* Get TX DC offset values */
        _ = this.RegisterWrite(Registers.DBG_AGC_MCU_RAM_ADDR, 0xA0 + i);
        Int32 read_val = this.RegisterRead(Registers.DBG_AGC_MCU_RAM_DATA);
        this.cal_offset_a_i[i] = (SByte)read_val;
        _ = this.RegisterWrite(Registers.DBG_AGC_MCU_RAM_ADDR, 0xA8 + i);
        read_val = this.RegisterRead(Registers.DBG_AGC_MCU_RAM_DATA);
        this.cal_offset_a_q[i] = (SByte)read_val;
        _ = this.RegisterWrite(Registers.DBG_AGC_MCU_RAM_ADDR, 0xB0 + i);
        read_val = this.RegisterRead(Registers.DBG_AGC_MCU_RAM_DATA);
        this.cal_offset_b_i[i] = (SByte)read_val;
        _ = this.RegisterWrite(Registers.DBG_AGC_MCU_RAM_ADDR, 0xB8 + i);
        read_val = this.RegisterRead(Registers.DBG_AGC_MCU_RAM_DATA);
        this.cal_offset_b_q[i] = (SByte)read_val;
      }

      this.LoadAdjustConstants(); /* load adjusted parameters */

      if (this.radioFrequency[0] == 0) { /* Sanity check for RX frequency */
        throw new Exception("ERROR: wrong configuration, rf_rx_freq[0] is not set\n");
      }

      UInt32 x = 4096000000 / (this.radioFrequency[0] >> 1); /* Freq-to-time-drift calculation */ /* dividend: (4*2048*1000000) >> 1, rescaled to avoid 32b overflow */
      x = (x > 63) ? 63 : x; /* saturation */
      _ = this.RegisterWrite(Registers.FREQ_TO_TIME_DRIFT, (Int32)x); /* default 9 */

      x = 4096000000 / (this.radioFrequency[0] >> 3); /* dividend: (16*2048*1000000) >> 3, rescaled to avoid 32b overflow */
      x = (x > 63) ? 63 : x; /* saturation */
      _ = this.RegisterWrite(Registers.MBWSSF_FREQ_TO_TIME_DRIFT, (Int32)x); /* default 36 */

      _ = this.RegisterWrite(Registers.IF_FREQ_0, (this.interfaceFrequency[0] << 5) / 15625); /* default -384 */
      _ = this.RegisterWrite(Registers.IF_FREQ_1, (this.interfaceFrequency[1] << 5) / 15625); /* default -128 */
      _ = this.RegisterWrite(Registers.IF_FREQ_2, (this.interfaceFrequency[2] << 5) / 15625); /* default 128 */
      _ = this.RegisterWrite(Registers.IF_FREQ_3, (this.interfaceFrequency[3] << 5) / 15625); /* default 384 */
      _ = this.RegisterWrite(Registers.IF_FREQ_4, (this.interfaceFrequency[4] << 5) / 15625); /* default -384 */
      _ = this.RegisterWrite(Registers.IF_FREQ_5, (this.interfaceFrequency[5] << 5) / 15625); /* default -128 */
      _ = this.RegisterWrite(Registers.IF_FREQ_6, (this.interfaceFrequency[6] << 5) / 15625); /* default 128 */
      _ = this.RegisterWrite(Registers.IF_FREQ_7, (this.interfaceFrequency[7] << 5) / 15625); /* default 384 */

      _ = this.RegisterWrite(Registers.CORR0_DETECT_EN, this.interfaceEnabled[0] ? (Byte)SF.DR_LORA_SFMULTI : 0); /* default 0 */
      _ = this.RegisterWrite(Registers.CORR1_DETECT_EN, this.interfaceEnabled[1] ? (Byte)SF.DR_LORA_SFMULTI : 0); /* default 0 */
      _ = this.RegisterWrite(Registers.CORR2_DETECT_EN, this.interfaceEnabled[2] ? (Byte)SF.DR_LORA_SFMULTI : 0); /* default 0 */
      _ = this.RegisterWrite(Registers.CORR3_DETECT_EN, this.interfaceEnabled[3] ? (Byte)SF.DR_LORA_SFMULTI : 0); /* default 0 */
      _ = this.RegisterWrite(Registers.CORR4_DETECT_EN, this.interfaceEnabled[4] ? (Byte)SF.DR_LORA_SFMULTI : 0); /* default 0 */
      _ = this.RegisterWrite(Registers.CORR5_DETECT_EN, this.interfaceEnabled[5] ? (Byte)SF.DR_LORA_SFMULTI : 0); /* default 0 */
      _ = this.RegisterWrite(Registers.CORR6_DETECT_EN, this.interfaceEnabled[6] ? (Byte)SF.DR_LORA_SFMULTI : 0); /* default 0 */
      _ = this.RegisterWrite(Registers.CORR7_DETECT_EN, this.interfaceEnabled[7] ? (Byte)SF.DR_LORA_SFMULTI : 0); /* default 0 */

      _ = this.RegisterWrite(Registers.PPM_OFFSET, 0x60); /* as the threshold is 16ms, use 0x60 to enable ppm_offset for SF12 and SF11 @125kHz*/

      _ = this.RegisterWrite(Registers.CONCENTRATOR_MODEM_ENABLE, 1); /* default 0 */


      _ = this.RegisterWrite(Registers.IF_FREQ_8, (this.interfaceFrequency[8] << 5) / 15625); /* configure LoRa 'stand-alone' modem (IF8) */ /* MBWSSF modem (default 0) */
      if (this.interfaceEnabled[8] == true) {
        _ = this.RegisterWrite(Registers.MBWSSF_RADIO_SELECT, (Byte)this.interfaceChain[8]);
        switch (this.loraBandwidth) {
          case BW.BW_125KHZ:
            _ = this.RegisterWrite(Registers.MBWSSF_MODEM_BW, 0); break;
          case BW.BW_250KHZ:
            _ = this.RegisterWrite(Registers.MBWSSF_MODEM_BW, 1); break;
          case BW.BW_500KHZ:
            _ = this.RegisterWrite(Registers.MBWSSF_MODEM_BW, 2); break;
        }
        switch (this.loraSpreadingFactor) {
          case SF.DR_LORA_SF7:
            _ = this.RegisterWrite(Registers.MBWSSF_RATE_SF, 7); break;
          case SF.DR_LORA_SF8:
            _ = this.RegisterWrite(Registers.MBWSSF_RATE_SF, 8); break;
          case SF.DR_LORA_SF9:
            _ = this.RegisterWrite(Registers.MBWSSF_RATE_SF, 9); break;
          case SF.DR_LORA_SF10:
            _ = this.RegisterWrite(Registers.MBWSSF_RATE_SF, 10); break;
          case SF.DR_LORA_SF11:
            _ = this.RegisterWrite(Registers.MBWSSF_RATE_SF, 11); break;
          case SF.DR_LORA_SF12:
            _ = this.RegisterWrite(Registers.MBWSSF_RATE_SF, 12); break;
        }
        _ = this.RegisterWrite(Registers.MBWSSF_PPM_OFFSET, this.loraBandwidth == BW.BW_125KHZ && (this.loraSpreadingFactor == SF.DR_LORA_SF11 || this.loraSpreadingFactor == SF.DR_LORA_SF12) || this.loraBandwidth == BW.BW_250KHZ && this.loraSpreadingFactor == SF.DR_LORA_SF12 ? 1 : 0); /* default 0 */
        _ = this.RegisterWrite(Registers.MBWSSF_MODEM_ENABLE, 1); /* default 0 */
      } else {
        _ = this.RegisterWrite(Registers.MBWSSF_MODEM_ENABLE, 0);
      }

      _ = this.RegisterWrite(Registers.IF_FREQ_9, (this.interfaceFrequency[9] << 5) / 15625);/* configure FSK modem (IF9) */ /* FSK modem, default 0 */
      _ = this.RegisterWrite(Registers.FSK_PSIZE, this.fskSyncWordSize - 1);
      _ = this.RegisterWrite(Registers.FSK_TX_PSIZE, this.fskSyncWordSize - 1);
      UInt64 fsk_sync_word_reg = this.fskSyncWord << (8 * (8 - this.fskSyncWordSize));
      _ = this.RegisterWrite(Registers.FSK_REF_PATTERN_LSB, (Int32)(UInt32)(0xFFFFFFFF & fsk_sync_word_reg));
      _ = this.RegisterWrite(Registers.FSK_REF_PATTERN_MSB, (Int32)(UInt32)(0xFFFFFFFF & (fsk_sync_word_reg >> 32)));
      if (this.interfaceEnabled[9] == true) {
        _ = this.RegisterWrite(Registers.FSK_RADIO_SELECT, (Byte)this.interfaceChain[9]);
        _ = this.RegisterWrite(Registers.FSK_BR_RATIO, (Int32)(32000000 / this.fskDatarate)); /* setting the dividing ratio for datarate */
        _ = this.RegisterWrite(Registers.FSK_CH_BW_EXPO, (Byte)this.fskBandwidth);
        _ = this.RegisterWrite(Registers.FSK_MODEM_ENABLE, 1); /* default 0 */
      } else {
        _ = this.RegisterWrite(Registers.FSK_MODEM_ENABLE, 0);
      }

      _ = this.LoadFirmware(Firmware.ARB); /* Load firmware */
      _ = this.LoadFirmware(Firmware.AGC);

      _ = this.RegisterWrite(Registers.FORCE_HOST_RADIO_CTRL, 0); /* gives the AGC MCU control over radio, RF front-end and filter gain */
      _ = this.RegisterWrite(Registers.FORCE_HOST_FE_CTRL, 0);
      _ = this.RegisterWrite(Registers.FORCE_DEC_FILTER_GAIN, 0);

      _ = this.RegisterWrite(Registers.RADIO_SELECT, 0); /* Get MCUs out of reset */ /* MUST not be = to 1 or 2 at firmware init */
      _ = this.RegisterWrite(Registers.MCU_RST_0, 0);
      _ = this.RegisterWrite(Registers.MCU_RST_1, 0);

      _ = this.RegisterWrite(Registers.DBG_AGC_MCU_RAM_ADDR, Firmware.AGC.Address); /* Check firmware version */
      fw_version = this.RegisterRead(Registers.DBG_AGC_MCU_RAM_DATA);
      if (fw_version != Firmware.AGC.Version) {
        throw new Exception("ERROR: Version of AGC firmware not expected, actual: " + fw_version + " expected: " + Firmware.AGC.Version);
      }
      _ = this.RegisterWrite(Registers.DBG_ARB_MCU_RAM_ADDR, Firmware.ARB.Address);
      fw_version = this.RegisterRead(Registers.DBG_ARB_MCU_RAM_DATA);
      if (fw_version != Firmware.ARB.Version) {
        throw new Exception("ERROR: Version of arbiter firmware not expected, actual: " + fw_version + " expected: " + Firmware.ARB.Version);
      }

      //Console.WriteLine("Info: Initialising AGC firmware...");
      Thread.Sleep(1);

      Int32 status = this.RegisterRead(Registers.MCU_AGC_STATUS);
      if (status != 0x10) {
        throw new Exception("ERROR: AGC FIRMWARE INITIALIZATION FAILURE, STATUS " + status.ToString("X2"));
      }

      for (Byte i = 0; i < Txgain_lut.size; ++i) { /* Update Tx gain LUT and start AGC */
        _ = this.RegisterWrite(Registers.RADIO_SELECT, 16); /* start a transaction */
        Thread.Sleep(1);
        _ = this.RegisterWrite(Registers.RADIO_SELECT, Txgain_lut.lut[i].mix_gain + 16 * Txgain_lut.lut[i].dac_gain + 64 * Txgain_lut.lut[i].pa_gain);
        Thread.Sleep(1);
        status = this.RegisterRead(Registers.MCU_AGC_STATUS);
        if (status != 0x30 + i) {
          throw new Exception("ERROR: AGC FIRMWARE INITIALIZATION FAILURE, STATUS " + status.ToString("X2"));
        }
      }

      if (Txgain_lut.size < 16) { /* As the AGC fw is waiting for 16 entries, we need to abort the transaction if we get less entries */
        _ = this.RegisterWrite(Registers.RADIO_SELECT, 16);
        Thread.Sleep(1);
        _ = this.RegisterWrite(Registers.RADIO_SELECT, 17);
        Thread.Sleep(1);
        status = this.RegisterRead(Registers.MCU_AGC_STATUS);
        if (status != 0x30) {
          throw new Exception("ERROR: AGC FIRMWARE INITIALIZATION FAILURE, STATUS " + status.ToString("X2"));
        }
      }


      _ = this.RegisterWrite(Registers.RADIO_SELECT, 16); /* Load Tx freq MSBs (always 3 if f > 768 for SX1257 or f > 384 for SX1255 */
      Thread.Sleep(1);
      _ = this.RegisterWrite(Registers.RADIO_SELECT, 3);
      Thread.Sleep(1);
      status = this.RegisterRead(Registers.MCU_AGC_STATUS);
      if (status != 0x33) {
        throw new Exception("ERROR: AGC FIRMWARE INITIALIZATION FAILURE, STATUS " + status.ToString("X2"));
      }


      _ = this.RegisterWrite(Registers.RADIO_SELECT, 16); /* Load chan_select firmware option */
      Thread.Sleep(1);
      _ = this.RegisterWrite(Registers.RADIO_SELECT, 0);
      Thread.Sleep(1);
      status = this.RegisterRead(Registers.MCU_AGC_STATUS);
      if (status != 0x30) {
        throw new Exception("ERROR: AGC FIRMWARE INITIALIZATION FAILURE, STATUS " + status.ToString("X2"));
      }

      Byte radio_select = 0; /* IF mapping to radio A/B (per bit, 0=A, 1=B) */
      for (Byte i = 0; i < 8; ++i) { /* configure LoRa 'multi' demodulators aka. LoRa 'sensor' channels (IF0-3) */
        radio_select += (Byte)(this.interfaceChain[i] == Reciever.Chain1 ? 1 << i : 0); /* transform bool array into binary word */
      }

      _ = this.RegisterWrite(Registers.RADIO_SELECT, 16); /* End AGC firmware init and check status */
      Thread.Sleep(1);
      _ = this.RegisterWrite(Registers.RADIO_SELECT, radio_select); /* Load intended value of RADIO_SELECT */
      Thread.Sleep(1);
      //Console.WriteLine("Info: putting back original RADIO_SELECT value");
      status = this.RegisterRead(Registers.MCU_AGC_STATUS);
      if (status != 0x40) {
        throw new Exception("ERROR: AGC FIRMWARE INITIALIZATION FAILURE, STATUS " + status.ToString("X2"));
      }


      _ = this.RegisterWrite(Registers.GPS_EN, 1); /* enable GPS event capture */

      //TODO Lib part for LBT (Listen before Talk)
      /* 
      if (this.Lbt_is_enabled() == true) {
        printf("INFO: Configuring LBT, this may take few seconds, please wait...\n");
        wait_ms(8400);
      }*/

      this.deviceStarted = true;
      return true;
    }

    public override void ParseConfig() {
      try {
        if (this.GetType() == typeof(Ic880alora)) {
          this.SetFrequency(UInt32.Parse(this.config["frequency0"]), 0);
          this.SetFrequency(UInt32.Parse(this.config["frequency1"]), 1);

          for (Byte i = 0; i < 10; i++) {
            this.SetInterfaceEnable(Boolean.Parse(this.config["interface" + i + "enable"]), i);
            this.SetInterfaceChain(Byte.Parse(this.config["interface" + i + "chain"]), i);
            this.SetInterfaceFrequency(Int32.Parse(this.config["interface" + i + "frequency"]), i);
          }

          this.SetSignalBandwith(Int64.Parse(this.config["lorabandwith"]), RadioDataType.Lora);
          this.SetSpreadingFactor(Byte.Parse(this.config["loraspreadingfactor"]));
          this.SetSignalBandwith(Int64.Parse(this.config["fskbandwith"]), RadioDataType.FSK);
          this.SetDatarate(UInt32.Parse(this.config["fskdatarate"]));
          this.EnableCrc();
        } 
      } catch (Exception e) {
        Helper.WriteError("Failed to ParseConfig! " + e.Message + "\n" + e.StackTrace);
        throw;
      }
    }
    #endregion

    #region Packets, Read, Write
    public override Boolean BeginPacket(Boolean implictHeader = false) => throw new NotImplementedException();
    public override Boolean EndPacket(Boolean async = false) => throw new NotImplementedException();
    public override Byte Write(Byte[] buffer) => throw new NotImplementedException();

    public override void Receive(Byte size) {
      Byte[] recieveregister = this.RegisterReadArray(Registers.RX_PACKET_DATA_FIFO_NUM_STORED, 5);
      if (recieveregister[0] > 0 && recieveregister[0] <= 16) {
        /* 0:   number of packets available in RX data buffer */
        /* 1,2: start address of the current packet in RX data buffer */
        /* 3:   CRC status of the current packet */
        /* 4:   size of the current packet payload in byte */
        //Console.WriteLine("FIFO content: " + recieveregister[0] + " " + recieveregister[1] + " " + recieveregister[2] + " " + recieveregister[3] + " " + recieveregister[4]);

        IC880ADataFrame p = new IC880ADataFrame {
          size = recieveregister[4]
        };
        Byte sz = p.size;
        Byte stat_fifo = recieveregister[3];

        Byte[] buff2 = this.RegisterReadArray(Registers.RX_DATA_BUF_DATA, (UInt16)(sz + 16));
        p.payload = new Byte[sz];
        for (Byte i = 0; i < sz; i++) {
          p.payload[i] = buff2[i];
        }
        //Console.WriteLine("Text: " + p.payload);
        p.@interface = buff2[sz + 0];
        if (p.@interface >= 10) {
          Helper.WriteError("WARNING: "+p.@interface+" NOT A VALID IF_CHAIN NUMBER, ABORTING");
          return ;
        }

        RadioDataType ifmod = RadioDataType.Undefined;
        if(p.@interface < 8 && p.@interface >= 0) {
          ifmod = RadioDataType.LoraMulti;
        } else if(p.@interface == 8) {
          ifmod = RadioDataType.Lora;
        } else if(p.@interface == 9) {
          ifmod = RadioDataType.FSK;
        }
        //Console.WriteLine("["+ p.if_chain + " "+ ifmod + "]");

        p.radio = this.interfaceChain[p.@interface];
        p.freq_hz = (UInt32)((Int32)this.radioFrequency[(Byte)p.radio] + this.interfaceFrequency[p.@interface]);
        p.rssi = buff2[sz + 5] + -166;

        p.status = Crc.CrcUndefined;
        Boolean crc_en = false;
        UInt32 timestamp_correction;
        if (ifmod == RadioDataType.LoraMulti || ifmod == RadioDataType.Lora) {
          //Console.WriteLine("Note: LoRa packet\n");
          p.status = Crc.CrcUndefined;
          switch (stat_fifo & 0x07) {
            case 5:
              p.status = Crc.CrcOk;
              crc_en = true;
              break;
            case 7:
              p.status = Crc.CrcBad;
              crc_en = true;
              break;
            case 1:
              p.status = Crc.CrcNo;
              crc_en = false;
              break;
          }
          p.modulation = Modulation.Lora;
          p.snr = (Single)(SByte)buff2[sz + 2] / 4;
          p.snr_min = (Single)(SByte)buff2[sz + 3] / 4;
          p.snr_max = (Single)(SByte)buff2[sz + 4] / 4;
          p.bandwidth = ifmod == RadioDataType.LoraMulti ? BW.BW_125KHZ : this.loraBandwidth;
          UInt32 sf = (UInt32)((buff2[sz + 1] >> 4) & 0x0F);
          p.spreadingfactor = SF.Undefined;
          switch (sf) {
            case 7:
              p.spreadingfactor = SF.DR_LORA_SF7;
              break;
            case 8:
              p.spreadingfactor = SF.DR_LORA_SF8;
              break;
            case 9:
              p.spreadingfactor = SF.DR_LORA_SF9;
              break;
            case 10:
              p.spreadingfactor = SF.DR_LORA_SF10;
              break;
            case 11:
              p.spreadingfactor = SF.DR_LORA_SF11;
              break;
            case 12:
              p.spreadingfactor = SF.DR_LORA_SF12;
              break;
          }
          UInt32 cr = (UInt32)((buff2[sz + 1] >> 1) & 0x07);
          p.coderate = CR.Undefined;
          switch (cr) {
            case 1:
              p.coderate = CR.CR_LORA_4_5;
              break;
            case 2:
              p.coderate = CR.CR_LORA_4_6;
              break;
            case 3:
              p.coderate = CR.CR_LORA_4_7;
              break;
            case 4:
              p.coderate = CR.CR_LORA_4_8;
              break;
          }

          /* determine if 'PPM mode' is on, needed for timestamp correction */
          Boolean ppm = p.bandwidth == BW.BW_125KHZ && (p.spreadingfactor == SF.DR_LORA_SF11 || p.spreadingfactor == SF.DR_LORA_SF12) || p.bandwidth == BW.BW_250KHZ && p.spreadingfactor == SF.DR_LORA_SF12;

          UInt32 delay_x = 0;
          UInt32 bw_pow = 0;
          /* timestamp correction code, base delay */
          if (ifmod == RadioDataType.Lora) { /* if packet was received on the stand-alone LoRa modem */
            switch (this.loraBandwidth) {
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
          } else { /* packet was received on one of the sensor channels = 125kHz */
            delay_x = 114;
            bw_pow = 1;
          }

          /* timestamp correction code, variable delay */
          if(sf >= 6 && sf <= 12 && bw_pow > 0) {
            UInt32 delay_y;
            UInt32 delay_z;
            if(2 * (sz + 2 * (crc_en ? 1 : 0)) - (sf - 7) <= 0) { /* payload fits entirely in first 8 symbols */
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

          /* RSSI correction */
          if (ifmod == RadioDataType.LoraMulti) {
            p.rssi -= -35;
          }

        } else if (ifmod == RadioDataType.FSK) {
          //Console.WriteLine("Note: FSK packet");
          p.status = Crc.CrcUndefined;
          switch (stat_fifo & 0x07) {
            case 5:
              p.status = Crc.CrcOk;
              break;
            case 7:
              p.status = Crc.CrcBad;
              break;
            case 1:
              p.status = Crc.CrcNo;
              break;
          }
          p.modulation = Modulation.Fsk;
          p.snr = -128;
          p.snr_min = -128;
          p.snr_max = -128;
          p.bandwidth = this.fskBandwidth;
          //p.datarate = this.fsk_rx_dr;
          p.coderate = CR.Undefined;
          timestamp_correction = 680000 / this.fskDatarate - 20;

          /* RSSI correction */
          p.rssi = (Single)(60 + 1.5351 * p.rssi + 0.003 * Math.Pow(p.rssi, 2));
        } else {
          Helper.WriteError("ERROR: UNEXPECTED PACKET ORIGIN");
          p.status = Crc.CrcUndefined;
          p.modulation = Modulation.Undefined;
          p.rssi = -128;
          p.snr = -128;
          p.snr_min = -128;
          p.snr_max = -128;
          p.bandwidth = BW.Undefined;
          p.spreadingfactor = SF.Undefined;
          p.coderate = CR.Undefined;
          timestamp_correction = 0;
        }

        UInt32 raw_timestamp = buff2[sz + 6] + ((UInt32)buff2[sz + 7] << 8) + ((UInt32)buff2[sz + 8] << 16) + ((UInt32)buff2[sz + 9] << 24);
        p.count_us = raw_timestamp - timestamp_correction;
        p.calccrc = (UInt16)(buff2[sz + 10] + (buff2[sz + 11] << 8));

        if (p.status == Crc.CrcOk && this.CrcEnabled || !this.CrcEnabled) {
          this.RaiseUpdateEvent(new Ic800ALoraClientEvent(p));
          //Console.WriteLine(p.ToString());
        }
        _ = this.RegisterWrite(Registers.RX_PACKET_DATA_FIFO_NUM_STORED, 0);
      }

    }
    #endregion

    #region RadioSettings
    public void SetInterfaceFrequency(Int32 offset, Byte @interface) {
      if (@interface >= 0 && @interface <= 9) {
        if(offset >= -500000 && offset <= 500000) {
          this.interfaceFrequency[@interface] = offset;
        } else {
          Helper.WriteError("Fraunhofer.Fit.Iot.Lora.lib.Ic880alora.SetInterfaceEnable(" + offset + "," + @interface + "): Offset " + offset + " is not allowed!");
        }
      } else {
        Helper.WriteError("Fraunhofer.Fit.Iot.Lora.lib.Ic880alora.SetInterfaceEnable(" + offset + "," + @interface + "): Interface " + @interface + " is not allowed!");
      }
    }

    public void SetInterfaceChain(Byte chain, Byte @interface) {
      if (@interface >= 0 && @interface <= 9) {
        if (chain == 0) {
          this.interfaceChain[@interface] = Reciever.Chain0;
        } else if(chain ==1) {
          this.interfaceChain[@interface] = Reciever.Chain1;
        } else {
          Helper.WriteError("Fraunhofer.Fit.Iot.Lora.lib.Ic880alora.SetInterfaceEnable(" + chain + "," + @interface + "): Chain " + chain + " is not allowed!");
        }
      } else {
        Helper.WriteError("Fraunhofer.Fit.Iot.Lora.lib.Ic880alora.SetInterfaceEnable(" + chain + "," + @interface + "): Interface " + @interface + " is not allowed!");
      }
    }

    public void SetInterfaceEnable(Boolean enable, Byte @interface) {
      if (@interface >= 0 && @interface <= 9) {
        this.interfaceEnabled[@interface] = enable;
      } else {
        Helper.WriteError("Fraunhofer.Fit.Iot.Lora.lib.Ic880alora.SetInterfaceEnable(" + enable + "," + @interface + "): Interface " + @interface + " is not allowed!");
      }
    }

    public void SetFrequency(UInt32 freq, Byte chain) {
      if (chain >= 0 && chain <= 1) {
        this.radioFrequency[chain] = freq;
        this.radioEnabled[chain] = true;
      } else {
        Helper.WriteError("Fraunhofer.Fit.Iot.Lora.lib.Ic880alora.SetFrequency(" + freq + "," + chain + "): Chain " + chain + " is not allowed!");
      }
    }

    public void SetSpreadingFactor(Byte sf) => this.loraSpreadingFactor = sf <= 7 ? SF.DR_LORA_SF7
      : sf <= 8 ? SF.DR_LORA_SF8
      : sf <= 9 ? SF.DR_LORA_SF9 
      : sf <= 10 ? SF.DR_LORA_SF10 
      : sf <= 11 ? SF.DR_LORA_SF11 
      : SF.DR_LORA_SF12;

    public void SetDatarate(UInt32 dr) => this.fskDatarate = dr;

    public void SetSignalBandwith(Int64 sbw, RadioDataType @interface) {
      BW bw = sbw <= 7800 ? BW.BW_7K8HZ
        : sbw <= 15600 ? BW.BW_15K6HZ
        : sbw <= 31250 ? BW.BW_31K2HZ
        : sbw <= 62500 ? BW.BW_62K5HZ 
        : sbw <= 125000 ? BW.BW_125KHZ 
        : sbw <= 250000 ? BW.BW_250KHZ 
        : BW.BW_500KHZ;
      if(@interface == RadioDataType.Lora) {
        this.loraBandwidth = bw;
      } else if(@interface == RadioDataType.FSK) {
        this.fskBandwidth = bw;
      }
    }

    public void EnableCrc() => this.CrcEnabled = true;

    public void DisableCrc() => this.CrcEnabled = false;
    #endregion

    #region Powserusage
    public override void SetTxPower(Int32 level) => throw new NotImplementedException();
    #endregion

    #region Register Communication
    private Int32 RegisterRead(LGWRegisters register) {
      if (register.RegisterPage != -1 && register.RegisterPage != this.selectedPage) {
        this.PageSwitch((Byte)register.RegisterPage);
      }
      Byte[] bufu = new Byte[4];
      SByte[] bufs = new SByte[4];
      if (register.BitOffset + register.SizeInBits <= 8) { /* read one byte, then shift and mask bits to get reg value with sign extension if needed */
        bufu[0] = this.SingleSPI((Byte)(0x00 | (register.Address & 0x7F)));
        bufu[1] = (Byte)(bufu[0] << (8 - register.SizeInBits - register.BitOffset)); /* left-align the data */
        if (register.SignedInt == true) {
          bufs[2] = (SByte)(bufs[1] >> (8 - register.SizeInBits)); /* right align the data with sign extension (ARITHMETIC right shift) */
          return bufs[2]; /* signed pointer -> 32b sign extension */
        } else {
          bufu[2] = (Byte)(bufu[1] >> (8 - register.SizeInBits)); /* right align the data, no sign extension */
          return bufu[2]; /* unsigned pointer -> no sign extension */
        }
      } else if (register.BitOffset == 0 && register.SizeInBits > 0 && register.SizeInBits <= 32) {
        Byte size = (Byte)((register.SizeInBits + 7) / 8); /* add a byte if it's not an exact multiple of 8 */
        bufu = this.MultiSPI((Byte)(0x00 | (register.Address & 0x7F)), new Byte[size], size);
        UInt32 u = 0;
        for (SByte i = (SByte)(size - 1); i >= 0; --i) {
          u = bufu[i] + (u << 8); /* transform a 4-byte array into a 32 bit word */
        }
        if (register.SignedInt == true) {
          u <<= 32 - register.SizeInBits; /* left-align the data */
          return (Int32)u >> (32 - register.SizeInBits); /* right-align the data with sign extension (ARITHMETIC right shift) */
        } else {
          return (Int32)u; /* unsigned value -> return 'as is' */
        }
      } else { /* register spanning multiple memory bytes but with an offset */
        Helper.WriteError("ERROR: REGISTER SIZE AND OFFSET ARE NOT SUPPORTED");
        return 0;
      }
    }

    private Byte[] RegisterReadArray(LGWRegisters register, UInt16 size) {
      if (register.RegisterPage != -1 && register.RegisterPage != this.selectedPage) { /* select proper register page if needed */
        this.PageSwitch((Byte)register.RegisterPage);
      }
      return this.MultiSPI((Byte)(0x00 | (register.Address & 0x7F)), new Byte[size], size);
    }

    private Boolean RegisterWrite(LGWRegisters register, Int32 value) {
      if (register.Equals(Registers.PAGE_REG)) {
        this.PageSwitch((Byte)value);
        return true;
      } else if (register.Equals(Registers.SOFT_RESET)) {
        if ((value & 0x01) != 0) {
          _ = this.SingleSPI((Byte)(0x80 | (Registers.SOFT_RESET.Address & 0x7F)), 0x80);
        }
        return true;
      }
      if (register.ReadonlyRegister) {
        return false;
      }
      if (register.RegisterPage != -1 && register.RegisterPage != this.selectedPage) {
        this.PageSwitch((Byte)register.RegisterPage);
      }
      Byte[] buf = new Byte[4];
      if (register.SizeInBits == 8 && register.BitOffset == 0) {
        _ = this.SingleSPI((Byte)(0x80 | (register.Address & 0x7F)), (Byte)value);
      } else if (register.BitOffset + register.SizeInBits <= 8) { // single-byte read-modify-write, offs:[0-7], leng:[1-7]
        buf[0] = this.SingleSPI((Byte)(0x00 | (register.Address & 0x7F)));
        buf[1] = (Byte)(((1 << register.SizeInBits) - 1) << register.BitOffset); // bit mask
        buf[2] = (Byte)(((Byte)value) << register.BitOffset); // new data offsetted
        buf[3] = (Byte)((~buf[1] & buf[0]) | (buf[1] & buf[2])); // mixing old & new data
        _ = this.SingleSPI((Byte)(0x80 | (register.Address & 0x7F)), buf[3]);
      } else if (register.BitOffset == 0 && register.SizeInBits > 0 && register.SizeInBits <= 32) { // multi-byte direct write routine
        Byte size = (Byte)((register.SizeInBits + 7) / 8); // add a byte if it's not an exact multiple of 8
        for (Byte i = 0; i < size; ++i) { // big endian register file for a file on N bytes Least significant byte is stored in buf[0], most one in buf[N - 1]
          buf[i] = (Byte)(0x000000FF & value);
          value >>= 8;
        }
        _ = this.MultiSPI((Byte)(0x80 | (register.Address & 0x7F)), buf, size); // write the register in one burst
      } else { // register spanning multiple memory bytes but with an offset
        return false;
      }
      return true;
    }

    private Boolean RegisterWriteArray(LGWRegisters register, Byte[] value, UInt16 size) {
      if (register.ReadonlyRegister) {
        Helper.WriteError("ERROR: TRYING TO BURST WRITE A READ-ONLY REGISTER");
        return false;
      }
      if (register.RegisterPage != -1 && register.RegisterPage != this.selectedPage) { /* select proper register page if needed */
        this.PageSwitch((Byte)register.RegisterPage);
      }
      _ = this.MultiSPI((Byte)(0x80 | (register.Address & 0x7F)), value, size); /* do the burst write */
      return true;
    }
    #endregion

    #region SX 125x Communiaction Helper
    private Boolean Sx125xSetup(Byte rf_chain, Byte rf_clkout, Boolean rf_enable, RadioType rf_radio_type, UInt32 freq_hz) {
      Int32 cpt_attempts = 0;

      if (rf_chain >= 2) {
        throw new Exception("ERROR: INVALID RF_CHAIN");
      }

      /* Get version to identify SX1255/57 silicon revision */
      //Console.WriteLine("Note: SX125x #" + rf_chain + " version register returned " + this.Sx125xRead(rf_chain, 0x07).ToString("X2"));

      /* General radio setup */
      if (rf_clkout == rf_chain) {
        this.RegisterWriteSx125x(rf_chain, 0x10, (Byte)(SX125X.TX_DAC_CLK_SEL + 2));
        //Console.WriteLine("Note: SX125x #"+ rf_chain + " clock output enabled");
      } else {
        this.RegisterWriteSx125x(rf_chain, 0x10, SX125X.TX_DAC_CLK_SEL);
        //Console.WriteLine("Note: SX125x #"+ rf_chain + " clock output disabled");
      }

      switch (rf_radio_type) {
        case RadioType.SX1255: //LGW_RADIO_TYPE_SX1255
          this.RegisterWriteSx125x(rf_chain, 0x28, (Byte)(SX125X.XOSC_GM_STARTUP + SX125X.XOSC_DISABLE * 16));
          break;
        case RadioType.SX1257: //LGW_RADIO_TYPE_SX1257
          this.RegisterWriteSx125x(rf_chain, 0x26, (Byte)(SX125X.XOSC_GM_STARTUP + SX125X.XOSC_DISABLE * 16));
          break;
        default:
          throw new Exception("ERROR: UNEXPECTED VALUE " + rf_radio_type + " FOR RADIO TYPE");
      }

      if (rf_enable == true) {
        /* Tx gain and trim */
        this.RegisterWriteSx125x(rf_chain, 0x08, (Byte)(SX125X.TX_MIX_GAIN + SX125X.TX_DAC_GAIN * 16));
        this.RegisterWriteSx125x(rf_chain, 0x0A, (Byte)(SX125X.TX_ANA_BW + SX125X.TX_PLL_BW * 32));
        this.RegisterWriteSx125x(rf_chain, 0x0B, SX125X.TX_DAC_BW);

        /* Rx gain and trim */
        this.RegisterWriteSx125x(rf_chain, 0x0C, (Byte)(SX125X.LNA_ZIN + SX125X.RX_BB_GAIN * 2 + SX125X.RX_LNA_GAIN * 32));
        this.RegisterWriteSx125x(rf_chain, 0x0D, (Byte)(SX125X.RX_BB_BW + SX125X.RX_ADC_TRIM * 4 + SX125X.RX_ADC_BW * 32));
        this.RegisterWriteSx125x(rf_chain, 0x0E, (Byte)(SX125X.ADC_TEMP + SX125X.RX_PLL_BW * 2));

        UInt32 part_int;
        UInt32 part_frac;
        /* set RX PLL frequency */
        switch(rf_radio_type) {
          case RadioType.SX1255: //LGW_RADIO_TYPE_SX1255:
            part_int = freq_hz / (SX125X.FRAC_32MHz << 7); /* integer part, gives the MSB */
            part_frac = ((freq_hz % (SX125X.FRAC_32MHz << 7)) << 9) / SX125X.FRAC_32MHz; /* fractional part, gives middle part and LSB */
            break;
          case RadioType.SX1257: //LGW_RADIO_TYPE_SX1257:
            part_int = freq_hz / (SX125X.FRAC_32MHz << 8); /* integer part, gives the MSB */
            part_frac = ((freq_hz % (SX125X.FRAC_32MHz << 8)) << 8) / SX125X.FRAC_32MHz; /* fractional part, gives middle part and LSB */
            break;
          default:
            throw new Exception("ERROR: UNEXPECTED VALUE " + rf_radio_type + " FOR RADIO TYPE");
        }

        this.RegisterWriteSx125x(rf_chain, 0x01, (Byte)(0xFF & part_int)); /* Most Significant Byte */
        this.RegisterWriteSx125x(rf_chain, 0x02, (Byte)(0xFF & (part_frac >> 8))); /* middle byte */
        this.RegisterWriteSx125x(rf_chain, 0x03, (Byte)(0xFF & part_frac)); /* Least Significant Byte */

        /* start and PLL lock */
        do {
          if (cpt_attempts >= 5) {
            throw new Exception("ERROR: FAIL TO LOCK PLL");
          }
          this.RegisterWriteSx125x(rf_chain, 0x00, 1); /* enable Xtal oscillator */
          this.RegisterWriteSx125x(rf_chain, 0x00, 3); /* Enable RX (PLL+FE) */
          ++cpt_attempts;
          //Console.WriteLine("Note: SX125x #"+ rf_chain + " PLL start (attempt "+ cpt_attempts + ")");
          Thread.Sleep(2);
        } while ((this.RegisterReadSx125x(rf_chain, 0x11) & 0x02) == 0);
      } else {
        Console.WriteLine("Note: SX125x #" + rf_chain + " kept in standby mode");
      }
      return true;
    }

    private void RegisterWriteSx125x(Byte channel, Byte addr, Byte data) {
      LGWRegisters reg_add, reg_dat, reg_cs;
      if (channel >= 2) { /* checking input parameters */
        Helper.WriteError("ERROR: INVALID RF_CHAIN\n");
        return;
      }
      if (addr >= 0x7F) {
        Helper.WriteError("ERROR: ADDRESS OUT OF RANGE\n");
        return;
      }
      switch (channel) { /* selecting the target radio */
        case 0:
          reg_add = Registers.SPI_RADIO_A__ADDR;
          reg_dat = Registers.SPI_RADIO_A__DATA;
          reg_cs = Registers.SPI_RADIO_A__CS;
          break;
        case 1:
          reg_add = Registers.SPI_RADIO_B__ADDR;
          reg_dat = Registers.SPI_RADIO_B__DATA;
          reg_cs = Registers.SPI_RADIO_B__CS;
          break;
        default:
          Helper.WriteError("ERROR: UNEXPECTED VALUE " + channel + " IN SWITCH STATEMENT");
          return;
      }
      _ = this.RegisterWrite(reg_cs, 0); /* SPI master data write procedure */
      _ = this.RegisterWrite(reg_add, 0x80 | addr); /* MSB at 1 for write operation */
      _ = this.RegisterWrite(reg_dat, data);
      _ = this.RegisterWrite(reg_cs, 1);
      _ = this.RegisterWrite(reg_cs, 0);
    }

    private Byte RegisterReadSx125x(Byte channel, Byte addr) {
      LGWRegisters reg_add, reg_dat, reg_cs, reg_rb;
      if (channel >= 2) { /* checking input parameters */
        Helper.WriteError("ERROR: INVALID RF_CHAIN\n");
        return 0;
      }
      if (addr >= 0x7F) {
        Helper.WriteError("ERROR: ADDRESS OUT OF RANGE\n");
        return 0;
      }
      switch (channel) { /* selecting the target radio */
        case 0:
          reg_add = Registers.SPI_RADIO_A__ADDR;
          reg_dat = Registers.SPI_RADIO_A__DATA;
          reg_cs = Registers.SPI_RADIO_A__CS;
          reg_rb = Registers.SPI_RADIO_A__DATA_READBACK;
          break;
        case 1:
          reg_add = Registers.SPI_RADIO_B__ADDR;
          reg_dat = Registers.SPI_RADIO_B__DATA;
          reg_cs = Registers.SPI_RADIO_B__CS;
          reg_rb = Registers.SPI_RADIO_B__DATA_READBACK;
          break;
        default:
          Helper.WriteError("ERROR: UNEXPECTED VALUE " + channel + " IN SWITCH STATEMENT");
          return 0;
      }
      _ = this.RegisterWrite(reg_cs, 0); /* SPI master data read procedure */
      _ = this.RegisterWrite(reg_add, addr); /* MSB at 0 for read operation */
      _ = this.RegisterWrite(reg_dat, 0);
      _ = this.RegisterWrite(reg_cs, 1);
      _ = this.RegisterWrite(reg_cs, 0);
      return (Byte)this.RegisterRead(reg_rb);
    }
    #endregion

    #region Private Functions
    private void PageSwitch(Byte targetPage) {
      this.selectedPage = (Byte)(0x03 & targetPage);
      _ = this.SingleSPI((Byte)(0x80 | (Registers.PAGE_REG.Address & 0x7F)), this.selectedPage);
    }

    private Boolean LoadFirmware(Firmwaredata fw) {
      LGWRegisters reg_rst;
      LGWRegisters reg_sel;

      if (fw.Mcu == 0) {
        if (fw.Size != 8192) {
          Helper.WriteError("ERROR: NOT A VALID SIZE FOR MCU ARG FIRMWARE");
          return false;
        }
        reg_rst = Registers.MCU_RST_0;
        reg_sel = Registers.MCU_SELECT_MUX_0;
      } else if (fw.Mcu == 1) {
        if (fw.Size != 8192) {
          Helper.WriteError("ERROR: NOT A VALID SIZE FOR MCU AGC FIRMWARE");
          return false;
        }
        reg_rst = Registers.MCU_RST_1;
        reg_sel = Registers.MCU_SELECT_MUX_1;
      } else {
        Helper.WriteError("ERROR: NOT A VALID TARGET FOR LOADING FIRMWARE");
        return false;
      }

      _ = this.RegisterWrite(reg_rst, 1); /* reset the targeted MCU */

      _ = this.RegisterWrite(reg_sel, 0); /* set mux to access MCU program RAM and set address to 0 */
      _ = this.RegisterWrite(Registers.MCU_PROM_ADDR, 0);

      _ = this.RegisterWriteArray(Registers.MCU_PROM_DATA, fw.Data, fw.Size);      /* write the program in one burst */

      _ = this.RegisterRead(Registers.MCU_PROM_DATA);       /* Read back firmware code for check */ /* bug workaround */
      Byte[] fw_check = this.RegisterReadArray(Registers.MCU_PROM_DATA, fw.Size);
      for (Int32 i = 0; i < fw.Size; i++) {
        if (fw.Data[i] != fw_check[i]) {
          Console.WriteLine("[" + i + "]in: " + fw.Data[i].ToString("X2") + " out: " + fw_check[i].ToString("X2"));
          Helper.WriteError("ERROR: Failed to load fw " + fw.Mcu);
          return false;
        }
      }
      _ = this.RegisterWrite(reg_sel, 1);      /* give back control of the MCU program ram to the MCU */
      return true;
    }

    private void LoadAdjustConstants() {
      /* I/Q path setup */
      // lgw_reg_w(LGW_RX_INVERT_IQ,0); /* default 0 */
      // lgw_reg_w(LGW_MODEM_INVERT_IQ,1); /* default 1 */
      // lgw_reg_w(LGW_CHIRP_INVERT_RX,1); /* default 1 */
      // lgw_reg_w(LGW_RX_EDGE_SELECT,0); /* default 0 */
      // lgw_reg_w(LGW_MBWSSF_MODEM_INVERT_IQ,0); /* default 0 */
      // lgw_reg_w(LGW_DC_NOTCH_EN,1); /* default 1 */
      _ = this.RegisterWrite(Registers.RSSI_BB_FILTER_ALPHA, 6); /* default 7 */
      _ = this.RegisterWrite(Registers.RSSI_DEC_FILTER_ALPHA, 7); /* default 5 */
      _ = this.RegisterWrite(Registers.RSSI_CHANN_FILTER_ALPHA, 7); /* default 8 */
      _ = this.RegisterWrite(Registers.RSSI_BB_DEFAULT_VALUE, 23); /* default 32 */
      _ = this.RegisterWrite(Registers.RSSI_CHANN_DEFAULT_VALUE, 85); /* default 100 */
      _ = this.RegisterWrite(Registers.RSSI_DEC_DEFAULT_VALUE, 66); /* default 100 */
      _ = this.RegisterWrite(Registers.DEC_GAIN_OFFSET, 7); /* default 8 */
      _ = this.RegisterWrite(Registers.CHAN_GAIN_OFFSET, 6); /* default 7 */

      /* Correlator setup */
      // lgw_reg_w(LGW_CORR_DETECT_EN,126); /* default 126 */
      // lgw_reg_w(LGW_CORR_NUM_SAME_PEAK,4); /* default 4 */
      // lgw_reg_w(LGW_CORR_MAC_GAIN,5); /* default 5 */
      // lgw_reg_w(LGW_CORR_SAME_PEAKS_OPTION_SF6,0); /* default 0 */
      // lgw_reg_w(LGW_CORR_SAME_PEAKS_OPTION_SF7,1); /* default 1 */
      // lgw_reg_w(LGW_CORR_SAME_PEAKS_OPTION_SF8,1); /* default 1 */
      // lgw_reg_w(LGW_CORR_SAME_PEAKS_OPTION_SF9,1); /* default 1 */
      // lgw_reg_w(LGW_CORR_SAME_PEAKS_OPTION_SF10,1); /* default 1 */
      // lgw_reg_w(LGW_CORR_SAME_PEAKS_OPTION_SF11,1); /* default 1 */
      // lgw_reg_w(LGW_CORR_SAME_PEAKS_OPTION_SF12,1); /* default 1 */
      // lgw_reg_w(LGW_CORR_SIG_NOISE_RATIO_SF6,4); /* default 4 */
      // lgw_reg_w(LGW_CORR_SIG_NOISE_RATIO_SF7,4); /* default 4 */
      // lgw_reg_w(LGW_CORR_SIG_NOISE_RATIO_SF8,4); /* default 4 */
      // lgw_reg_w(LGW_CORR_SIG_NOISE_RATIO_SF9,4); /* default 4 */
      // lgw_reg_w(LGW_CORR_SIG_NOISE_RATIO_SF10,4); /* default 4 */
      // lgw_reg_w(LGW_CORR_SIG_NOISE_RATIO_SF11,4); /* default 4 */
      // lgw_reg_w(LGW_CORR_SIG_NOISE_RATIO_SF12,4); /* default 4 */

      /* LoRa 'multi' demodulators setup */
      // lgw_reg_w(LGW_PREAMBLE_SYMB1_NB,10); /* default 10 */
      // lgw_reg_w(LGW_FREQ_TO_TIME_INVERT,29); /* default 29 */
      // lgw_reg_w(LGW_FRAME_SYNCH_GAIN,1); /* default 1 */
      // lgw_reg_w(LGW_SYNCH_DETECT_TH,1); /* default 1 */
      // lgw_reg_w(LGW_ZERO_PAD,0); /* default 0 */
      _ = this.RegisterWrite(Registers.SNR_AVG_CST, 3); /* default 2 */
                                                        //if (this.lorawan_public) { /* LoRa network */
                                                        //  this.RegisterWrite(Registers.FRAME_SYNCH_PEAK1_POS, 3); /* default 1 */
                                                        //  this.RegisterWrite(Registers.FRAME_SYNCH_PEAK2_POS, 4); /* default 2 */
                                                        //} else { /* private network */
      _ = this.RegisterWrite(Registers.FRAME_SYNCH_PEAK1_POS, 1); /* default 1 */
      _ = this.RegisterWrite(Registers.FRAME_SYNCH_PEAK2_POS, 2); /* default 2 */
                                                                  //}

      // lgw_reg_w(LGW_PREAMBLE_FINE_TIMING_GAIN,1); /* default 1 */
      // lgw_reg_w(LGW_ONLY_CRC_EN,1); /* default 1 */
      // lgw_reg_w(LGW_PAYLOAD_FINE_TIMING_GAIN,2); /* default 2 */
      // lgw_reg_w(LGW_TRACKING_INTEGRAL,0); /* default 0 */
      // lgw_reg_w(LGW_ADJUST_MODEM_START_OFFSET_RDX8,0); /* default 0 */
      // lgw_reg_w(LGW_ADJUST_MODEM_START_OFFSET_SF12_RDX4,4092); /* default 4092 */
      // lgw_reg_w(LGW_MAX_PAYLOAD_LEN,255); /* default 255 */

      /* LoRa standalone 'MBWSSF' demodulator setup */
      // lgw_reg_w(LGW_MBWSSF_PREAMBLE_SYMB1_NB,10); /* default 10 */
      // lgw_reg_w(LGW_MBWSSF_FREQ_TO_TIME_INVERT,29); /* default 29 */
      // lgw_reg_w(LGW_MBWSSF_FRAME_SYNCH_GAIN,1); /* default 1 */
      // lgw_reg_w(LGW_MBWSSF_SYNCH_DETECT_TH,1); /* default 1 */
      // lgw_reg_w(LGW_MBWSSF_ZERO_PAD,0); /* default 0 */
      //if (this.lorawan_public) { /* LoRa network */
      //  this.RegisterWrite(Registers.MBWSSF_FRAME_SYNCH_PEAK1_POS, 3); /* default 1 */
      //  this.RegisterWrite(Registers.MBWSSF_FRAME_SYNCH_PEAK2_POS, 4); /* default 2 */
      //} else {
      _ = this.RegisterWrite(Registers.MBWSSF_FRAME_SYNCH_PEAK1_POS, 1); /* default 1 */
      _ = this.RegisterWrite(Registers.MBWSSF_FRAME_SYNCH_PEAK2_POS, 2); /* default 2 */
      //}
      // lgw_reg_w(LGW_MBWSSF_ONLY_CRC_EN,1); /* default 1 */
      // lgw_reg_w(LGW_MBWSSF_PAYLOAD_FINE_TIMING_GAIN,2); /* default 2 */
      // lgw_reg_w(LGW_MBWSSF_PREAMBLE_FINE_TIMING_GAIN,1); /* default 1 */
      // lgw_reg_w(LGW_MBWSSF_TRACKING_INTEGRAL,0); /* default 0 */
      // lgw_reg_w(LGW_MBWSSF_AGC_FREEZE_ON_DETECT,1); /* default 1 */

      /* Improvement of reference clock frequency error tolerance */
      _ = this.RegisterWrite(Registers.ADJUST_MODEM_START_OFFSET_RDX4, 1); /* default 0 */
      _ = this.RegisterWrite(Registers.ADJUST_MODEM_START_OFFSET_SF12_RDX4, 4094); /* default 4092 */
      _ = this.RegisterWrite(Registers.CORR_MAC_GAIN, 7); /* default 5 */

      /* FSK datapath setup */
      _ = this.RegisterWrite(Registers.FSK_RX_INVERT, 1); /* default 0 */
      _ = this.RegisterWrite(Registers.FSK_MODEM_INVERT_IQ, 1); /* default 0 */

      /* FSK demodulator setup */
      _ = this.RegisterWrite(Registers.FSK_RSSI_LENGTH, 4); /* default 0 */
      _ = this.RegisterWrite(Registers.FSK_PKT_MODE, 1); /* variable length, default 0 */
      _ = this.RegisterWrite(Registers.FSK_CRC_EN, 1); /* default 0 */
      _ = this.RegisterWrite(Registers.FSK_DCFREE_ENC, 2); /* default 0 */
                                                           // lgw_reg_w(LGW_FSK_CRC_IBM,0); /* default 0 */
      _ = this.RegisterWrite(Registers.FSK_ERROR_OSR_TOL, 10); /* default 0 */
      _ = this.RegisterWrite(Registers.FSK_PKT_LENGTH, 255); /* max packet length in variable length mode */
                                                             // lgw_reg_w(LGW_FSK_NODE_ADRS,0); /* default 0 */
                                                             // lgw_reg_w(LGW_FSK_BROADCAST,0); /* default 0 */
                                                             // lgw_reg_w(LGW_FSK_AUTO_AFC_ON,0); /* default 0 */
      _ = this.RegisterWrite(Registers.FSK_PATTERN_TIMEOUT_CFG, 128); /* sync timeout (allow 8 bytes preamble + 8 bytes sync word, default 0 */

      /* TX general parameters */
      _ = this.RegisterWrite(Registers.TX_START_DELAY, 1497); /* default 0 */
      /* Calibrated value for 500KHz BW and notch filter disabled */

      /* TX LoRa */
      // lgw_reg_w(LGW_TX_MODE,0); /* default 0 */
      _ = this.RegisterWrite(Registers.TX_SWAP_IQ, 1); /* "normal" polarity; default 0 */
                                                       //if (this.lorawan_public) { /* LoRa network */
                                                       //  this.RegisterWrite(Registers.TX_FRAME_SYNCH_PEAK1_POS, 3); /* default 1 */
                                                       //  this.RegisterWrite(Registers.TX_FRAME_SYNCH_PEAK2_POS, 4); /* default 2 */
                                                       //} else { /* Private network */
      _ = this.RegisterWrite(Registers.TX_FRAME_SYNCH_PEAK1_POS, 1); /* default 1 */
      _ = this.RegisterWrite(Registers.TX_FRAME_SYNCH_PEAK2_POS, 2); /* default 2 */
      //}

      /* TX FSK */
      // lgw_reg_w(LGW_FSK_TX_GAUSSIAN_EN,1); /* default 1 */
      _ = this.RegisterWrite(Registers.FSK_TX_GAUSSIAN_SELECT_BT, 2); /* Gaussian filter always on TX, default 0 */
                                                   // lgw_reg_w(LGW_FSK_TX_PATTERN_EN,1); /* default 1 */
                                                   // lgw_reg_w(LGW_FSK_TX_PREAMBLE_SEQ,0); /* default 0 */
    }

    private void ReceiveRunner() {
      Console.WriteLine("Fraunhofer.Fit.Iot.Lora.lib.Ic880alora.ReceiveRunner(): gestartet!");
      while (this.ReceiveRunnerAlive) {
        if (this.deviceStarted) {
          this.Receive(0);
          Thread.Sleep(1);
        }
      }
    }
    #endregion

    #region Hardware IO
    private void Reset() {
      this.PinReset.Write(true);
      Thread.Sleep(150);
      this.PinReset.Write(false);
      _ = this.SingleSPI(0, 128);
      _ = this.SingleSPI(0, 0);
      Thread.Sleep(32); // provide at least 16 cycles on CLKHS and 16 cycles CLK32M
      _ = this.SingleSPI(18, 1);
      Thread.Sleep(42); // provide at least 4 cycles on CLKHS and 32 cycles CLK32M and 4 cycles on HOST_SCK
      _ = this.SingleSPI(18, 2);
      Thread.Sleep(8); // provide at least 4 cycles CLK32M and 4 cycles on HOST_SCK
      _ = this.SingleSPI(0, 128);
      _ = this.SingleSPI(0, 0);
      //this.PinReset.PinMode = GpioPinDriveMode.Input;
    }

    private void SetupIO() {
      Pi.Spi.SetProperty(this.config["spichan"] + "Frequency", 100000.ToString());
      this.PinSlaveSelect.PinMode = GpioPinDriveMode.Output;
      this.PinReset.PinMode = GpioPinDriveMode.Output;
    }

    private void Selectreceiver() => this.PinSlaveSelect.Write(false);

    private void Unselectreceiver() => this.PinSlaveSelect.Write(true);

    private Byte SingleSPI(Byte address, Byte value = 0) {
      this.Selectreceiver();
      Byte[] spibuf = Pi.Spi.Channel0.SendReceive(new Byte[] { address, value });
      this.Unselectreceiver();
      return spibuf[1];
    }

    private Byte[] MultiSPI(Byte address, Byte[] value, UInt16 size) {
      Byte[] tx = new Byte[size + 1];
      tx[0] = address;
      for (UInt16 i = 0; i < size; i++) {
        tx[i + 1] = value[i];
      }
      this.Selectreceiver();
      Byte[] rx = this.SpiChannel.SendReceive(tx);
      this.Unselectreceiver();
      Byte[] spibuf = new Byte[size];
      for (UInt16 i = 0; i < size; i++) {
        spibuf[i] = rx[i + 1];
      }
      return spibuf;
    }

    public override void AttachUpdateEvent() {
      Console.WriteLine("Fraunhofer.Fit.Iot.Lora.lib.Ic880alora.AttachUpdateEvent(): Start Recieving Thread!");
      this.ReceiveRunnerAlive = true;
      this.receiveThread.Start();
    }
    #endregion
  }
}
