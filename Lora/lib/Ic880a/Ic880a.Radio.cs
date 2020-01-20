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
using System.Threading;

using BlubbFish.Utils;

namespace Fraunhofer.Fit.Iot.Lora.lib.Ic880a {
  public partial class Ic880a {
    public static partial class Registers {
      #region SX1276 Registers
      public static Byte SX1276_REG_FIFO => 0x00;
      // Common settings
      public static Byte SX1276_REG_OPMODE => 0x01;
      public static Byte SX1276_REG_BITRATEMSB => 0x02;
      public static Byte SX1276_REG_BITRATELSB => 0x03;
      public static Byte SX1276_REG_FDEVMSB => 0x04;
      public static Byte SX1276_REG_FDEVLSB => 0x05;
      public static Byte SX1276_REG_FRFMSB => 0x06;
      public static Byte SX1276_REG_FRFMID => 0x07;
      public static Byte SX1276_REG_FRFLSB => 0x08;
      // Tx settings
      public static Byte SX1276_REG_PACONFIG => 0x09;
      public static Byte SX1276_REG_PARAMP => 0x0A;
      public static Byte SX1276_REG_OCP => 0x0B;
      // Rx settings
      public static Byte SX1276_REG_LNA => 0x0C;
      public static Byte SX1276_REG_RXCONFIG => 0x0D;
      public static Byte SX1276_REG_RSSICONFIG => 0x0E;
      public static Byte SX1276_REG_RSSICOLLISION => 0x0F;
      public static Byte SX1276_REG_RSSITHRESH => 0x10;
      public static Byte SX1276_REG_RSSIVALUE => 0x11;
      public static Byte SX1276_REG_RXBW => 0x12;
      public static Byte SX1276_REG_AFCBW => 0x13;
      public static Byte SX1276_REG_OOKPEAK => 0x14;
      public static Byte SX1276_REG_OOKFIX => 0x15;
      public static Byte SX1276_REG_OOKAVG => 0x16;
      public static Byte SX1276_REG_RES17 => 0x17;
      public static Byte SX1276_REG_RES18 => 0x18;
      public static Byte SX1276_REG_RES19 => 0x19;
      public static Byte SX1276_REG_AFCFEI => 0x1A;
      public static Byte SX1276_REG_AFCMSB => 0x1B;
      public static Byte SX1276_REG_AFCLSB => 0x1C;
      public static Byte SX1276_REG_FEIMSB => 0x1D;
      public static Byte SX1276_REG_FEILSB => 0x1E;
      public static Byte SX1276_REG_PREAMBLEDETECT => 0x1F;
      public static Byte SX1276_REG_RXTIMEOUT1 => 0x20;
      public static Byte SX1276_REG_RXTIMEOUT2 => 0x21;
      public static Byte SX1276_REG_RXTIMEOUT3 => 0x22;
      public static Byte SX1276_REG_RXDELAY => 0x23;
      // Oscillator settings
      public static Byte SX1276_REG_OSC => 0x24;
      // Packet handler settings
      public static Byte SX1276_REG_PREAMBLEMSB => 0x25;
      public static Byte SX1276_REG_PREAMBLELSB => 0x26;
      public static Byte SX1276_REG_SYNCCONFIG => 0x27;
      public static Byte SX1276_REG_SYNCVALUE1 => 0x28;
      public static Byte SX1276_REG_SYNCVALUE2 => 0x29;
      public static Byte SX1276_REG_SYNCVALUE3 => 0x2A;
      public static Byte SX1276_REG_SYNCVALUE4 => 0x2B;
      public static Byte SX1276_REG_SYNCVALUE5 => 0x2C;
      public static Byte SX1276_REG_SYNCVALUE6 => 0x2D;
      public static Byte SX1276_REG_SYNCVALUE7 => 0x2E;
      public static Byte SX1276_REG_SYNCVALUE8 => 0x2F;
      public static Byte SX1276_REG_PACKETCONFIG1 => 0x30;
      public static Byte SX1276_REG_PACKETCONFIG2 => 0x31;
      public static Byte SX1276_REG_PAYLOADLENGTH => 0x32;
      public static Byte SX1276_REG_NODEADRS => 0x33;
      public static Byte SX1276_REG_BROADCASTADRS => 0x34;
      public static Byte SX1276_REG_FIFOTHRESH => 0x35;
      // SM settings
      public static Byte SX1276_REG_SEQCONFIG1 => 0x36;
      public static Byte SX1276_REG_SEQCONFIG2 => 0x37;
      public static Byte SX1276_REG_TIMERRESOL => 0x38;
      public static Byte SX1276_REG_TIMER1COEF => 0x39;
      public static Byte SX1276_REG_TIMER2COEF => 0x3A;
      // Service settings
      public static Byte SX1276_REG_IMAGECAL => 0x3B;
      public static Byte SX1276_REG_TEMP => 0x3C;
      public static Byte SX1276_REG_LOWBAT => 0x3D;
      // Status
      public static Byte SX1276_REG_IRQFLAGS1 => 0x3E;
      public static Byte SX1276_REG_IRQFLAGS2 => 0x3F;
      // I/O settings
      public static Byte SX1276_REG_DIOMAPPING1 => 0x40;
      public static Byte SX1276_REG_DIOMAPPING2 => 0x41;
      // Version
      public static Byte SX1276_REG_VERSION => 0x42;
      // Additional settings
      public static Byte SX1276_REG_PLLHOP => 0x44;
      public static Byte SX1276_REG_TCXO => 0x4B;
      public static Byte SX1276_REG_PADAC => 0x4D;
      public static Byte SX1276_REG_FORMERTEMP => 0x5B;
      public static Byte SX1276_REG_BITRATEFRAC => 0x5D;
      public static Byte SX1276_REG_AGCREF => 0x61;
      public static Byte SX1276_REG_AGCTHRESH1 => 0x62;
      public static Byte SX1276_REG_AGCTHRESH2 => 0x63;
      public static Byte SX1276_REG_AGCTHRESH3 => 0x64;
      public static Byte SX1276_REG_PLL => 0x70;
      #endregion

      #region SX1272 Registers
      public static Byte SX1272_REG_FIFO => 0x00;
      // Common settings;
      public static Byte SX1272_REG_OPMODE => 0x01;
      public static Byte SX1272_REG_BITRATEMSB => 0x02;
      public static Byte SX1272_REG_BITRATELSB => 0x03;
      public static Byte SX1272_REG_FDEVMSB => 0x04;
      public static Byte SX1272_REG_FDEVLSB => 0x05;
      public static Byte SX1272_REG_FRFMSB => 0x06;
      public static Byte SX1272_REG_FRFMID => 0x07;
      public static Byte SX1272_REG_FRFLSB => 0x08;
      // Tx settings;
      public static Byte SX1272_REG_PACONFIG => 0x09;
      public static Byte SX1272_REG_PARAMP => 0x0A;
      public static Byte SX1272_REG_OCP => 0x0B;
      // Rx settings;
      public static Byte SX1272_REG_LNA => 0x0C;
      public static Byte SX1272_REG_RXCONFIG => 0x0D;
      public static Byte SX1272_REG_RSSICONFIG => 0x0E;
      public static Byte SX1272_REG_RSSICOLLISION => 0x0F;
      public static Byte SX1272_REG_RSSITHRESH => 0x10;
      public static Byte SX1272_REG_RSSIVALUE => 0x11;
      public static Byte SX1272_REG_RXBW => 0x12;
      public static Byte SX1272_REG_AFCBW => 0x13;
      public static Byte SX1272_REG_OOKPEAK => 0x14;
      public static Byte SX1272_REG_OOKFIX => 0x15;
      public static Byte SX1272_REG_OOKAVG => 0x16;
      public static Byte SX1272_REG_RES17 => 0x17;
      public static Byte SX1272_REG_RES18 => 0x18;
      public static Byte SX1272_REG_RES19 => 0x19;
      public static Byte SX1272_REG_AFCFEI => 0x1A;
      public static Byte SX1272_REG_AFCMSB => 0x1B;
      public static Byte SX1272_REG_AFCLSB => 0x1C;
      public static Byte SX1272_REG_FEIMSB => 0x1D;
      public static Byte SX1272_REG_FEILSB => 0x1E;
      public static Byte SX1272_REG_PREAMBLEDETECT => 0x1F;
      public static Byte SX1272_REG_RXTIMEOUT1 => 0x20;
      public static Byte SX1272_REG_RXTIMEOUT2 => 0x21;
      public static Byte SX1272_REG_RXTIMEOUT3 => 0x22;
      public static Byte SX1272_REG_RXDELAY => 0x23;
      // Oscillator settings;
      public static Byte SX1272_REG_OSC => 0x24;
      // Packet handler settings;
      public static Byte SX1272_REG_PREAMBLEMSB => 0x25;
      public static Byte SX1272_REG_PREAMBLELSB => 0x26;
      public static Byte SX1272_REG_SYNCCONFIG => 0x27;
      public static Byte SX1272_REG_SYNCVALUE1 => 0x28;
      public static Byte SX1272_REG_SYNCVALUE2 => 0x29;
      public static Byte SX1272_REG_SYNCVALUE3 => 0x2A;
      public static Byte SX1272_REG_SYNCVALUE4 => 0x2B;
      public static Byte SX1272_REG_SYNCVALUE5 => 0x2C;
      public static Byte SX1272_REG_SYNCVALUE6 => 0x2D;
      public static Byte SX1272_REG_SYNCVALUE7 => 0x2E;
      public static Byte SX1272_REG_SYNCVALUE8 => 0x2F;
      public static Byte SX1272_REG_PACKETCONFIG1 => 0x30;
      public static Byte SX1272_REG_PACKETCONFIG2 => 0x31;
      public static Byte SX1272_REG_PAYLOADLENGTH => 0x32;
      public static Byte SX1272_REG_NODEADRS => 0x33;
      public static Byte SX1272_REG_BROADCASTADRS => 0x34;
      public static Byte SX1272_REG_FIFOTHRESH => 0x35;
      // SM settings;
      public static Byte SX1272_REG_SEQCONFIG1 => 0x36;
      public static Byte SX1272_REG_SEQCONFIG2 => 0x37;
      public static Byte SX1272_REG_TIMERRESOL => 0x38;
      public static Byte SX1272_REG_TIMER1COEF => 0x39;
      public static Byte SX1272_REG_TIMER2COEF => 0x3A;
      // Service settings;
      public static Byte SX1272_REG_IMAGECAL => 0x3B;
      public static Byte SX1272_REG_TEMP => 0x3C;
      public static Byte SX1272_REG_LOWBAT => 0x3D;
      // Status;
      public static Byte SX1272_REG_IRQFLAGS1 => 0x3E;
      public static Byte SX1272_REG_IRQFLAGS2 => 0x3F;
      // I/O settings;
      public static Byte SX1272_REG_DIOMAPPING1 => 0x40;
      public static Byte SX1272_REG_DIOMAPPING2 => 0x41;
      // Version;
      public static Byte SX1272_REG_VERSION => 0x42;
      // Additional settings;
      public static Byte SX1272_REG_AGCREF => 0x43;
      public static Byte SX1272_REG_AGCTHRESH1 => 0x44;
      public static Byte SX1272_REG_AGCTHRESH2 => 0x45;
      public static Byte SX1272_REG_AGCTHRESH3 => 0x46;
      public static Byte SX1272_REG_PLLHOP => 0x4B;
      public static Byte SX1272_REG_TCXO => 0x58;
      public static Byte SX1272_REG_PADAC => 0x5A;
      public static Byte SX1272_REG_PLL => 0x5C;
      public static Byte SX1272_REG_PLLLOWPN => 0x5E;
      public static Byte SX1272_REG_FORMERTEMP => 0x6C;
      public static Byte SX1272_REG_BITRATEFRAC => 0x70;
      #endregion
    }

    private readonly Lgw_sx127x_FSK_bandwidth_s[] sx127x_FskBandwidths = {
      new Lgw_sx127x_FSK_bandwidth_s(2600  , 2, 7),
      new Lgw_sx127x_FSK_bandwidth_s(2600  , 2, 7 ),   // LGW_SX127X_RXBW_2K6_HZ 
      new Lgw_sx127x_FSK_bandwidth_s(3100  , 1, 7 ),   // LGW_SX127X_RXBW_3K1_HZ 
      new Lgw_sx127x_FSK_bandwidth_s(3900  , 0, 7 ),   // ... 
      new Lgw_sx127x_FSK_bandwidth_s(5200  , 2, 6 ),
      new Lgw_sx127x_FSK_bandwidth_s(6300  , 1, 6 ),
      new Lgw_sx127x_FSK_bandwidth_s(7800  , 0, 6 ),
      new Lgw_sx127x_FSK_bandwidth_s(10400 , 2, 5 ),
      new Lgw_sx127x_FSK_bandwidth_s(12500 , 1, 5 ),
      new Lgw_sx127x_FSK_bandwidth_s(15600 , 0, 5 ),
      new Lgw_sx127x_FSK_bandwidth_s(20800 , 2, 4 ),
      new Lgw_sx127x_FSK_bandwidth_s(25000 , 1, 4 ),   // ... 
      new Lgw_sx127x_FSK_bandwidth_s(31300 , 0, 4 ),
      new Lgw_sx127x_FSK_bandwidth_s(41700 , 2, 3 ),
      new Lgw_sx127x_FSK_bandwidth_s(50000 , 1, 3 ),
      new Lgw_sx127x_FSK_bandwidth_s(62500 , 0, 3 ),
      new Lgw_sx127x_FSK_bandwidth_s(83333 , 2, 2 ),
      new Lgw_sx127x_FSK_bandwidth_s(100000, 1, 2 ),
      new Lgw_sx127x_FSK_bandwidth_s(125000, 0, 2 ),
      new Lgw_sx127x_FSK_bandwidth_s(166700, 2, 1 ),
      new Lgw_sx127x_FSK_bandwidth_s(200000, 1, 1 ),   // ... 
      new Lgw_sx127x_FSK_bandwidth_s(250000, 0, 1 )    // LGW_SX127X_RXBW_250K_HZ 
    };


    private void Sx125xWrite(Byte channel, Byte addr, Byte data) {
      LGWRegisters reg_add, reg_dat, reg_cs;
      if(channel >= 2) { // checking input parameters 
        throw new Exception("ERROR: INVALID RF_CHAIN\n");
      }
      if(addr >= 0x7F) {
        throw new Exception("ERROR: ADDRESS OUT OF RANGE\n");
      }
      switch(channel) { // selecting the target radio 
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
          throw new Exception("ERROR: UNEXPECTED VALUE " + channel + " IN SWITCH STATEMENT");
      }
      this.RegisterWrite(reg_cs, 0); // SPI master data write procedure 
      this.RegisterWrite(reg_add, 0x80 | addr); // MSB at 1 for write operation 
      this.RegisterWrite(reg_dat, data);
      this.RegisterWrite(reg_cs, 1);
      this.RegisterWrite(reg_cs, 0);
    }

    private Byte Sx125xRead(Byte channel, Byte addr) {
      LGWRegisters reg_add, reg_dat, reg_cs, reg_rb;
      if(channel >= 2) { // checking input parameters 
        throw new Exception("ERROR: INVALID RF_CHAIN\n");
      }
      if(addr >= 0x7F) {
        throw new Exception("ERROR: ADDRESS OUT OF RANGE\n");
      }
      switch(channel) { // selecting the target radio 
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
          throw new Exception("ERROR: UNEXPECTED VALUE " + channel + " IN SWITCH STATEMENT");
      }
      this.RegisterWrite(reg_cs, 0); // SPI master data read procedure 
      this.RegisterWrite(reg_add, addr); // MSB at 0 for read operation 
      this.RegisterWrite(reg_dat, 0);
      this.RegisterWrite(reg_cs, 1);
      this.RegisterWrite(reg_cs, 0);
      return (Byte)this.RegisterRead(reg_rb);
    }

    private void SetupSx1272Fsk(UInt32 frequency, Int32 rxbw_khz, SByte rssi_offset) {
      // Set in FSK mode 
      this.Sx127xWrite(Registers.SX1272_REG_OPMODE, 0);
      Thread.Sleep(100);
      Byte ModulationShaping = 0;
      this.Sx127xWrite(Registers.SX1272_REG_OPMODE, (Byte)(0 | (ModulationShaping << 3))); // Sleep mode, no FSK shaping 
      Thread.Sleep(100);
      this.Sx127xWrite(Registers.SX1272_REG_OPMODE, (Byte)(1 | (ModulationShaping << 3))); // Standby mode, no FSK shaping 
      Thread.Sleep(100);

      // Set RF carrier frequency 
      Byte PllHop = 1;
      this.Sx127xWrite(Registers.SX1272_REG_PLLHOP, (Byte)(PllHop << 7));
      UInt64 freq_reg = ((UInt64)frequency << 19) / 32000000;
      this.Sx127xWrite(Registers.SX1272_REG_FRFMSB, (Byte)((freq_reg >> 16) & 0xFF));
      this.Sx127xWrite(Registers.SX1272_REG_FRFMID, (Byte)((freq_reg >> 8) & 0xFF));
      this.Sx127xWrite(Registers.SX1272_REG_FRFLSB, (Byte)((freq_reg >> 0) & 0xFF));

      // Config 
      Byte LnaGain = 1;
      Byte LnaBoost = 3;
      this.Sx127xWrite(Registers.SX1272_REG_LNA, (Byte)(LnaBoost | (LnaGain << 5))); // Improved sensitivity, highest gain 
      Byte AdcBwAuto = 0;
      Byte AdcBw = 7;
      this.Sx127xWrite(0x68, (Byte)(AdcBw | (AdcBwAuto << 3)));
      Byte AdcLowPwr = 0;
      Byte AdcTrim = 6;
      Byte AdcTest = 0;
      this.Sx127xWrite(0x69, (Byte)(AdcTest | (AdcTrim << 4) | (AdcLowPwr << 7)));

      // set BR and FDEV for 200 kHz bandwidth
      this.Sx127xWrite(Registers.SX1272_REG_BITRATEMSB, 125);
      this.Sx127xWrite(Registers.SX1272_REG_BITRATELSB, 0);
      this.Sx127xWrite(Registers.SX1272_REG_FDEVMSB, 2);
      this.Sx127xWrite(Registers.SX1272_REG_FDEVLSB, 225);

      // Config continues... 
      this.Sx127xWrite(Registers.SX1272_REG_RXCONFIG, 0); // Disable AGC 
      Byte RssiOffsetReg = (rssi_offset >= 0) ? (Byte)rssi_offset : (Byte)(~-rssi_offset + 1); // 2's complement 
      Byte RssiSmoothing = 5;
      this.Sx127xWrite(Registers.SX1272_REG_RSSICONFIG, (Byte)(RssiSmoothing | (RssiOffsetReg << 3))); // Set RSSI smoothing to 64 samples, RSSI offset to given value 
      Byte RxBwExp = this.sx127x_FskBandwidths[rxbw_khz].RxBwExp;
      Byte RxBwMant = this.sx127x_FskBandwidths[rxbw_khz].RxBwMant;
      this.Sx127xWrite(Registers.SX1272_REG_RXBW, (Byte)(RxBwExp | (RxBwMant << 3)));
      this.Sx127xWrite(Registers.SX1272_REG_RXDELAY, 2);
      this.Sx127xWrite(Registers.SX1272_REG_PLL, 0x10); // PLL BW set to 75 KHz 
      this.Sx127xWrite(0x47, 1); // optimize PLL start-up time 

      // set Rx continuous mode 
      this.Sx127xWrite(Registers.SX1272_REG_OPMODE, (Byte)(5 | (ModulationShaping << 3))); // Receiver Mode, no FSK shaping 
      Thread.Sleep(500);
      Byte reg_val = this.Sx127xRead(Registers.SX1272_REG_IRQFLAGS1);
      // Check if RxReady and ModeReady 
      if(this.BitCheck(reg_val, 6, 1) == 0 || this.BitCheck(reg_val, 7, 1) == 0) {
        throw new Exception("ERROR: SX1272 failed to enter RX continuous mode");
      }
      Thread.Sleep(500);

      Console.WriteLine("INFO: Successfully configured SX1272 for FSK modulation (rxbw=" + rxbw_khz + ")");
    }

    private void SetupSx1276Fsk(UInt32 frequency, Int32 rxbw_khz, SByte rssi_offset) {
      // Set in FSK mode 
      this.Sx127xWrite(Registers.SX1276_REG_OPMODE, 0);
      Thread.Sleep(100);
      Byte ModulationShaping = 0;
      this.Sx127xWrite(Registers.SX1276_REG_OPMODE, (Byte)(0 | (ModulationShaping << 3))); // Sleep mode, no FSK shaping 
      Thread.Sleep(100);
      this.Sx127xWrite(Registers.SX1276_REG_OPMODE, (Byte)(1 | (ModulationShaping << 3))); // Standby mode, no FSK shaping 
      Thread.Sleep(100);

      // Set RF carrier frequency 
      Byte PllHop = 1;
      this.Sx127xWrite(Registers.SX1276_REG_PLLHOP, (Byte)(PllHop << 7));
      UInt64 freq_reg = ((UInt64)frequency << 19) / 32000000;
      this.Sx127xWrite(Registers.SX1276_REG_FRFMSB, (Byte)((freq_reg >> 16) & 0xFF));
      this.Sx127xWrite(Registers.SX1276_REG_FRFMID, (Byte)((freq_reg >> 8) & 0xFF));
      this.Sx127xWrite(Registers.SX1276_REG_FRFLSB, (Byte)((freq_reg >> 0) & 0xFF));

      // Config 
      Byte LnaGain = 1;
      Byte LnaBoost = 3;
      this.Sx127xWrite(Registers.SX1276_REG_LNA, (Byte)(LnaBoost | (LnaGain << 5))); // Improved sensitivity, highest gain 
      Byte AdcBwAuto = 0;
      Byte AdcBw = 7;
      this.Sx127xWrite(0x57, (Byte)(AdcBw | (AdcBwAuto << 3)));
      Byte AdcLowPwr = 0;
      Byte AdcTrim = 6;
      Byte AdcTest = 0;
      this.Sx127xWrite(0x58, (Byte)(AdcTest | (AdcTrim << 4) | (AdcLowPwr << 7)));

      // set BR and FDEV for 200 kHz bandwidth
      this.Sx127xWrite(Registers.SX1276_REG_BITRATEMSB, 125);
      this.Sx127xWrite(Registers.SX1276_REG_BITRATELSB, 0);
      this.Sx127xWrite(Registers.SX1276_REG_FDEVMSB, 2);
      this.Sx127xWrite(Registers.SX1276_REG_FDEVLSB, 225);

      // Config continues... 
      this.Sx127xWrite(Registers.SX1276_REG_RXCONFIG, 0); // Disable AGC 
      Byte RssiOffsetReg = (rssi_offset >= 0) ? (Byte)rssi_offset : (Byte)(~-rssi_offset + 1); // 2's complement 
      Byte RssiSmoothing = 5;
      this.Sx127xWrite(Registers.SX1276_REG_RSSICONFIG, (Byte)(RssiSmoothing | (RssiOffsetReg << 3))); // Set RSSI smoothing to 64 samples, RSSI offset 3dB 
      Byte RxBwExp = this.sx127x_FskBandwidths[rxbw_khz].RxBwExp;
      Byte RxBwMant = this.sx127x_FskBandwidths[rxbw_khz].RxBwMant;
      this.Sx127xWrite(Registers.SX1276_REG_RXBW, (Byte)(RxBwExp | (RxBwMant << 3)));
      this.Sx127xWrite(Registers.SX1276_REG_RXDELAY, 2);
      this.Sx127xWrite(Registers.SX1276_REG_PLL, 0x10); // PLL BW set to 75 KHz 
      this.Sx127xWrite(0x43, 1); // optimize PLL start-up time 

      // set Rx continuous mode 
      this.Sx127xWrite(Registers.SX1276_REG_OPMODE, (Byte)(5 | (ModulationShaping << 3))); // Receiver Mode, no FSK shaping 
      Thread.Sleep(500);
      Byte reg_val = this.Sx127xRead(Registers.SX1276_REG_IRQFLAGS1);
      // Check if RxReady and ModeReady 
      if(this.BitCheck(reg_val, 6, 1) == 0 || this.BitCheck(reg_val, 7, 1) == 0) {
        throw new Exception("ERROR: SX1276 failed to enter RX continuous mode");
      }
      Thread.Sleep(500);

      Console.WriteLine("INFO: Successfully configured SX1276 for FSK modulation (rxbw=" + rxbw_khz + ")");
    }

    private void ResetSx127x(RadioTypeSx127x radio_type) {
      switch(radio_type) {
        case RadioTypeSx127x.LGW_RADIO_TYPE_SX1276:
          this.FpgaRegisterWrite(Registers.FPGA_CTRL_RADIO_RESET, 0);
          this.FpgaRegisterWrite(Registers.FPGA_CTRL_RADIO_RESET, 1);
          break;
        case RadioTypeSx127x.LGW_RADIO_TYPE_SX1272:
          this.FpgaRegisterWrite(Registers.FPGA_CTRL_RADIO_RESET, 1);
          this.FpgaRegisterWrite(Registers.FPGA_CTRL_RADIO_RESET, 0);
          break;
        default:
          throw new Exception("ERROR: Failed to reset sx127x, not supported (" + radio_type + ")");
      }
    }

    private void SetupSx125x(Byte rf_chain, Byte rf_clkout, Boolean rf_enable, RadioType rf_radio_type, UInt32 freq_hz) {
      Int32 cpt_attempts = 0;

      if(rf_chain >= 2) {
        throw new Exception("ERROR: INVALID RF_CHAIN");
      }

      // Get version to identify SX1255/57 silicon revision 
      //Console.WriteLine("Note: SX125x #" + rf_chain + " version register returned " + this.Sx125xRead(rf_chain, 0x07).ToString("X2"));

      // General radio setup 
      if(rf_clkout == rf_chain) {
        this.Sx125xWrite(rf_chain, 0x10, (Byte)(SX125X.TX_DAC_CLK_SEL + 2));
        //Console.WriteLine("Note: SX125x #"+ rf_chain + " clock output enabled");
      } else {
        this.Sx125xWrite(rf_chain, 0x10, SX125X.TX_DAC_CLK_SEL);
        //Console.WriteLine("Note: SX125x #"+ rf_chain + " clock output disabled");
      }

      switch(rf_radio_type) {
        case RadioType.SX1255: //LGW_RADIO_TYPE_SX1255
          this.Sx125xWrite(rf_chain, 0x28, (Byte)(SX125X.XOSC_GM_STARTUP + SX125X.XOSC_DISABLE * 16));
          break;
        case RadioType.SX1257: //LGW_RADIO_TYPE_SX1257
          this.Sx125xWrite(rf_chain, 0x26, (Byte)(SX125X.XOSC_GM_STARTUP + SX125X.XOSC_DISABLE * 16));
          break;
        default:
          throw new Exception("ERROR: UNEXPECTED VALUE " + rf_radio_type + " FOR RADIO TYPE");
      }

      if(rf_enable == true) {
        // Tx gain and trim 
        this.Sx125xWrite(rf_chain, 0x08, (Byte)(SX125X.TX_MIX_GAIN + SX125X.TX_DAC_GAIN * 16));
        this.Sx125xWrite(rf_chain, 0x0A, (Byte)(SX125X.TX_ANA_BW + SX125X.TX_PLL_BW * 32));
        this.Sx125xWrite(rf_chain, 0x0B, SX125X.TX_DAC_BW);

        // Rx gain and trim 
        this.Sx125xWrite(rf_chain, 0x0C, (Byte)(SX125X.LNA_ZIN + SX125X.RX_BB_GAIN * 2 + SX125X.RX_LNA_GAIN * 32));
        this.Sx125xWrite(rf_chain, 0x0D, (Byte)(SX125X.RX_BB_BW + SX125X.RX_ADC_TRIM * 4 + SX125X.RX_ADC_BW * 32));
        this.Sx125xWrite(rf_chain, 0x0E, (Byte)(SX125X.ADC_TEMP + SX125X.RX_PLL_BW * 2));

        UInt32 part_int;
        UInt32 part_frac;
        // set RX PLL frequency 
        switch(rf_radio_type) {
          case RadioType.SX1255: //LGW_RADIO_TYPE_SX1255:
            part_int = freq_hz / (SX125X.FRAC_32MHz << 7); // integer part, gives the MSB 
            part_frac = ((freq_hz % (SX125X.FRAC_32MHz << 7)) << 9) / SX125X.FRAC_32MHz; // fractional part, gives middle part and LSB 
            break;
          case RadioType.SX1257: //LGW_RADIO_TYPE_SX1257:
            part_int = freq_hz / (SX125X.FRAC_32MHz << 8); // integer part, gives the MSB 
            part_frac = ((freq_hz % (SX125X.FRAC_32MHz << 8)) << 8) / SX125X.FRAC_32MHz; // fractional part, gives middle part and LSB 
            break;
          default:
            throw new Exception("ERROR: UNEXPECTED VALUE " + rf_radio_type + " FOR RADIO TYPE");
        }

        this.Sx125xWrite(rf_chain, 0x01, (Byte)(0xFF & part_int)); // Most Significant Byte 
        this.Sx125xWrite(rf_chain, 0x02, (Byte)(0xFF & (part_frac >> 8))); // middle byte 
        this.Sx125xWrite(rf_chain, 0x03, (Byte)(0xFF & part_frac)); // Least Significant Byte 

        // start and PLL lock 
        do {
          if(cpt_attempts >= 50) {
            throw new Exception("ERROR: FAIL TO LOCK PLL");
          }
          this.Sx125xWrite(rf_chain, 0x00, 1); // enable Xtal oscillator 
          this.Sx125xWrite(rf_chain, 0x00, 3); // Enable RX (PLL+FE) 
          ++cpt_attempts;
          //Console.WriteLine("Note: SX125x #"+ rf_chain + " PLL start (attempt "+ cpt_attempts + ")");
          Thread.Sleep(2);
        } while((this.Sx125xRead(rf_chain, 0x11) & 0x02) == 0);
      } else {
        Console.WriteLine("Note: SX125x #" + rf_chain + " kept in standby mode");
      }
    }

    private void Sx127xWrite(Byte address, Byte value) => this.SPIwriteRegisterRaw(address, value, 0x1);

    private Byte Sx127xRead(Byte address) => this.SPIreadRegister(address, 0x1);

    private void SetupSx127x(UInt32 frequency, Byte modulation, Sx127xRxbwE rxbw_khz, SByte rssi_offset) {
      RadioTypeSx127x radio_type = RadioTypeSx127x.LGW_RADIO_TYPE_NONE;
      RadioTypeVersion[] supported_radio_type = new RadioTypeVersion[2] {
        new RadioTypeVersion(RadioTypeSx127x.LGW_RADIO_TYPE_SX1272, 0x22),
        new RadioTypeVersion(RadioTypeSx127x.LGW_RADIO_TYPE_SX1276, 0x12)
      };

      // Check parameters 
      if(modulation != 0x20) { //MOD_FSK
        throw new Exception("ERROR: modulation not supported for SX127x (" + modulation + ")");
      }
      if(rxbw_khz > Sx127xRxbwE.LGW_SX127X_RXBW_250K_HZ) {
        throw new Exception("ERROR: RX bandwidth not supported for SX127x (" + rxbw_khz + ")");
      }

      // Probing radio type 
      for(Int32 i = 0; i < supported_radio_type.Length; i++) {
        // Reset the radio 
        this.ResetSx127x(supported_radio_type[i].Type);
        // Read version register 
        Byte version = this.Sx127xRead(0x42);
        // Check if we got the expected version 
        if(version != supported_radio_type[i].Version) {
          Helper.WriteError("INFO: sx127x version register - read: " + version + ", expected: " + supported_radio_type[i].Version);
          continue;
        } else {
          Console.WriteLine("INFO: sx127x radio has been found (type: " + supported_radio_type[i].Type + ", version: " + version + ")");
          radio_type = supported_radio_type[i].Type;
          break;
        }
      }
      if(radio_type == RadioTypeSx127x.LGW_RADIO_TYPE_NONE) {
        throw new Exception("ERROR: sx127x radio has not been found\n");
      }

      // Setup the radio 
      switch(modulation) {
        case 0x20: //MOD_FSK:
          if(radio_type == RadioTypeSx127x.LGW_RADIO_TYPE_SX1272) {
            this.SetupSx1272Fsk(frequency, (Int32)rxbw_khz, rssi_offset);
          } else {
            this.SetupSx1276Fsk(frequency, (Int32)rxbw_khz, rssi_offset);
          }
          break;
        default:
          // Should not happen 
          break;
      }
    }
  }
}
