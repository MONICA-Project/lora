using BlubbFish.Utils;
using Fraunhofer.Fit.Iot.Lora.Events;
using System;
using System.Collections.Generic;
using System.Text;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Gpio;

// Hope RFM96
// http://www.hoperf.com/upload/rf/RFM95_96_97_98W.pdf
// The RFM97 offers bandwidth options ranging from 7.8 kHz to 500 kHz with spreading factors ranging from 6 to 12, and covering all available frequency bands.

namespace Fraunhofer.Fit.Iot.Lora.lib
{
  public class Draginolora : LoraConnector, IDisposable {

    #region Private Vars
    private const Byte MaxPKTLength = 255;
    private Int64 _frequency = 0;
    private Byte _packetIndex = 0;
    private Boolean _implictHeaderMode = false;
    private GpioPin PinSlaveSelect;
    private GpioPin PinDataInput;
    private GpioPin PinReset;
    private Boolean _init = false;
    private Boolean disposedValue = false;
    #endregion

    #region Registers, Modes, Pa, Irq
    enum Registers : Byte {
      FIFO = 0x00,
      OP_MODE = 0x01,
      FRF_MSB = 0x06,
      FRF_MID = 0x07,
      FRF_LSB = 0x08,
      PA_CONFIG = 0x09,
      PRE_PA_RAMP = 0x0A,
      OCP = 0x0B,
      LNA = 0x0C,
      FIFO_ADDR_PTR = 0x0D,
      FIFO_TX_BASE_ADDR = 0x0E,
      FIFO_RX_BASE_ADDR = 0x0F,
      FIFO_RX_CURRENT_ADDR = 0x10,
      IRQ_FLAGS = 0x12,
      RX_NB_BYTES = 0x13,
      PKT_SNR_VALUE = 0x19,
      PKT_RSSI_VALUE = 0x1A,
      RSSI_VALUE = 0x1B,
      MODEM_CONFIG_1 = 0x1D,
      MODEM_CONFIG_2 = 0x1E,
      PREAMBLE_MSB = 0x20,
      PREAMBLE_LSB = 0x21,
      PAYLOAD_LENGTH = 0x22,
      MODEM_CONFIG_3 = 0x26,
      FREQ_ERROR_MSB = 0x28,
      FREQ_ERROR_MID = 0x29,
      FREQ_ERROR_LSB = 0x2A,
      RSSI_WIDEBAND = 0x2C,
      DETECTION_OPTIMIZE = 0x31,
      INVERTIQ = 0x33,
      DEDECTION_THRESHOLD = 0x37,
      SYNC_WORD = 0x39,
      INVERTIQ2 = 0x3B,
      DIO_MAPPING_1 = 0x40,
      VERSION = 0x42,
      PA_DAC = 0x4D
    };

    enum Modes : Byte {
      SLEEP = 0x00,
      STDBY = 0x01,
      TX = 0x03,
      RX_CONTINOUS = 0x05,
      RX_SINGLE = 0x06,
      LONG_RANGE_MODE = 0x80
    };

    public enum Pa : Byte {
      BOOST = 0x80,
      OUTPUT_RFO_PIN = 0,
      OUTPUT_PA_BOOST_PIN = 1
    };

    enum Irq : Byte {
      TX_DONE_MASK = 0x08,
      PAYLOAD_CRC_ERROR_MASK = 0x20,
      RX_DONE_MASK = 0x40
    }
    #endregion
    
    #region Constructor
    public Draginolora(Dictionary<String, String> settings) : base(settings) {
      this.PinSlaveSelect = (GpioPin)Pi.Gpio.GetProperty(this.config["pin_sspin"]);
      this.PinDataInput = (GpioPin)Pi.Gpio.GetProperty(this.config["pin_dio0"]);
      this.PinReset = (GpioPin)Pi.Gpio.GetProperty(this.config["pin_rst"]);
    }

    public override Boolean Begin() {
      Int64 freq = Int64.Parse(this.config["frequency"]);
      if (this._init) {
        return false;
      }
      this.SetupIO();
      this.Reset();
      Byte version = this.ReadRegister(Registers.VERSION);
      if (version != 0x12) {
        return false;
      }
      this.Sleep();
      this.SetFrequency(freq);
      //set base Addr
      this.WriteRegister(Registers.FIFO_TX_BASE_ADDR, 0);
      this.WriteRegister(Registers.FIFO_RX_BASE_ADDR, 0);
      //set LNA boost
      this.WriteRegister(Registers.LNA, (Byte)(this.ReadRegister(Registers.LNA) | 0x03));
      //set auto AGC
      this.WriteRegister(Registers.MODEM_CONFIG_3, 0x04);
      this.SetTxPower(17);
      this.Ilde();
      this._init = true;
      return true;
    }

    public override void End() {
      this.Sleep();
      this._init = false;
    }

    public override void Dispose() {
      if (!this.disposedValue) {
        this.PinDataInput = null;
        this.PinSlaveSelect = null;
        this.PinReset = null;
        this.disposedValue = true;
      }
    }

    public override Boolean StartRadio() => true;

    public override void ParseConfig() {
      if (!this.config.ContainsKey("frequency") || 
        !this.config.ContainsKey("spreadingfactor") || 
        !this.config.ContainsKey("signalbandwith") || 
        !this.config.ContainsKey("codingrate")) {
        throw new Exception("Fraunhofer.Fit.Iot.Lora.lib.Draginolora.ParseConfig(): Not all Settings set!: [lora]\ntype=Draginolora\nfrequency=868100000\nspreadingfactor=9\nsignalbandwith=125000\ncodingrate=5 missing");
      }
      this.SetSignalBandwith(Int64.Parse(this.config["signalbandwith"]));
      this.SetSpreadingFactor(Byte.Parse(this.config["spreadingfactor"]));
      this.SetCodingRate4(Byte.Parse(this.config["codingrate"]));
      this.DisableCrc();
    }
    #endregion

    #region Packets, Read, Write
    public override Boolean BeginPacket(Boolean implictHeader = false) {
      if(this.IsTransmitting()) {
        return false;
      }
      this.Ilde();
      if (implictHeader) {
        this.ImplicitHeaderMode();
      } else {
        this.ExplicitHeaderMode();
      }
      this.WriteRegister(Registers.FIFO_ADDR_PTR, 0);
      this.WriteRegister(Registers.PAYLOAD_LENGTH, 0);
      return true;
    }

    public override Boolean EndPacket(Boolean async = false) {
      this.WriteRegister(Registers.OP_MODE, (Byte)Modes.LONG_RANGE_MODE | (Byte)Modes.TX);
      if (async) {
        System.Threading.Thread.Sleep(150);
      } else {
        while ((this.ReadRegister(Registers.IRQ_FLAGS) & (Byte)Irq.TX_DONE_MASK) == 0) {
          System.Threading.Thread.Sleep(1);
        }
        this.WriteRegister(Registers.IRQ_FLAGS, (Byte)Irq.TX_DONE_MASK);
      }
      return true;
    }

    public Byte ParsePacket(Byte size) {
      Byte packetLenth = 0;
      Byte irqflags = this.ReadRegister(Registers.IRQ_FLAGS);
      if (size > 0) {
        this.ImplicitHeaderMode();
        this.WriteRegister(Registers.PAYLOAD_LENGTH, (Byte)(size & 0xff));
      } else {
        this.ExplicitHeaderMode();
      }
      this.WriteRegister(Registers.FIFO_ADDR_PTR, this.ReadRegister(Registers.FIFO_RX_CURRENT_ADDR));
      if ((irqflags & (Byte)Irq.RX_DONE_MASK) != 0 && (irqflags & (Byte)Irq.PAYLOAD_CRC_ERROR_MASK) == 0) {
        this._packetIndex = 0;
        if (this._implictHeaderMode) {
          packetLenth = this.ReadRegister(Registers.PAYLOAD_LENGTH);
        } else {
          packetLenth = this.ReadRegister(Registers.RX_NB_BYTES);
        }
        this.WriteRegister(Registers.FIFO_ADDR_PTR, this.ReadRegister(Registers.FIFO_RX_CURRENT_ADDR));
        this.Ilde();
      } else if (this.ReadRegister(Registers.OP_MODE) != ((Byte)Modes.LONG_RANGE_MODE | (Byte)Modes.RX_SINGLE)) {
        this.WriteRegister(Registers.FIFO_ADDR_PTR, 0);
        this.WriteRegister(Registers.OP_MODE, (Byte)Modes.LONG_RANGE_MODE | (Byte)Modes.RX_SINGLE);
      }
      return packetLenth;
    }

    public override Byte Write(Byte[] buffer) {
      Byte currentLength = this.ReadRegister(Registers.PAYLOAD_LENGTH);
      Byte size = buffer.Length > 255 ? MaxPKTLength : (Byte)buffer.Length;
      if ((currentLength + buffer.Length) > MaxPKTLength) {
        size = (Byte)(MaxPKTLength - currentLength);
      }
      for (Byte i = 0; i < size; i++) {
        this.WriteRegister(Registers.FIFO, buffer[i]);
      }
      this.WriteRegister(Registers.PAYLOAD_LENGTH, (Byte)(currentLength + size));
      return size;
    }

    public Byte Available() => (Byte)(this.ReadRegister(Registers.RX_NB_BYTES) - this._packetIndex);

    public Int16 Read() {
      if (this.Available() == 0) {
        return -1;
      }
      this._packetIndex++;
      return this.ReadRegister(Registers.FIFO);
    }

    public Int16 Peek() {
      if (this.Available() == 0) {
        return -1;
      }
      Byte currentAddress = this.ReadRegister(Registers.FIFO_ADDR_PTR);
      Byte b = this.ReadRegister(Registers.FIFO);
      this.WriteRegister(Registers.FIFO_ADDR_PTR, currentAddress);
      return b;
    }

    public override void Receive(Byte size) {
      if (size > 0) {
        this.ImplicitHeaderMode();
        this.WriteRegister(Registers.PAYLOAD_LENGTH, (Byte)(size & 0xff));
      } else {
        this.ExplicitHeaderMode();
      }
      this.WriteRegister(Registers.OP_MODE, (Byte)Modes.LONG_RANGE_MODE | (Byte)Modes.RX_CONTINOUS);
    }

    public void SetPreambleLength(UInt16 length) {
      this.WriteRegister(Registers.PREAMBLE_MSB, (Byte)(length >> 8));
      this.WriteRegister(Registers.PREAMBLE_LSB, (Byte)(length >> 0));
    }

    public void SetSyncWord(Byte sw) => this.WriteRegister(Registers.SYNC_WORD, sw);

    public void EnableCrc() => this.WriteRegister(Registers.MODEM_CONFIG_2, (Byte)(this.ReadRegister(Registers.MODEM_CONFIG_2) | 0x04));

    public void DisableCrc() => this.WriteRegister(Registers.MODEM_CONFIG_2, (Byte)(this.ReadRegister(Registers.MODEM_CONFIG_2) & 0xfb));
    #endregion

    #region RadioSettings
    public Byte Rssi() => (Byte)(this.ReadRegister(Registers.RSSI_VALUE) - (this._frequency < 868E6 ? 164 : 157));

    public Byte PacketRssi() => (Byte)(this.ReadRegister(Registers.PKT_RSSI_VALUE) - (this._frequency < 868E6 ? 164 : 157));

    public Double PacketSnr() => ((SByte)this.ReadRegister(Registers.PKT_SNR_VALUE)) * 0.25;

    public Int64 PacketFrequencyError() {
      Int32 freqError = 0;
      freqError = this.ReadRegister(Registers.FREQ_ERROR_MSB) & 0x07;
      freqError <<= 8;
      freqError += this.ReadRegister(Registers.FREQ_ERROR_MID);
      freqError <<= 8;
      freqError += this.ReadRegister(Registers.FREQ_ERROR_LSB);
      if ((this.ReadRegister(Registers.FREQ_ERROR_MSB) & 0x08) != 0) { // Sign bit is on
        freqError -= 524288; // B1000'0000'0000'0000'0000
      }
      Double fXtal = 32E6; // FXOSC: crystal oscillator (XTAL) frequency (2.5. Chip Specification, p. 14)
      Double fError = ((((Double)freqError) * (1L << 24)) / fXtal) * (this.GetSignalBandwidth() / 500000.0f); // p. 37
      return (Int64)fError;
    }

    public void SetFrequency(Int64 freq) {
      this._frequency = freq;
      UInt64 frf = ((UInt64)freq << 19) / 32000000;
      this.WriteRegister(Registers.FRF_MSB, (Byte)(frf >> 16));
      this.WriteRegister(Registers.FRF_MID, (Byte)(frf >> 8));
      this.WriteRegister(Registers.FRF_LSB, (Byte)(frf >> 0));
    }

    public Byte GetSpreadingFactor() => (Byte)(this.ReadRegister(Registers.MODEM_CONFIG_2) >> 4);

    public void SetSpreadingFactor(Byte sf) {
      if(sf < 6) {
        sf = 6;
      } else if(sf > 12) {
        sf = 12;
      }
      if(sf == 6) {
        this.WriteRegister(Registers.DETECTION_OPTIMIZE, 0xC5);
        this.WriteRegister(Registers.DEDECTION_THRESHOLD, 0x0C);
      } else {
        this.WriteRegister(Registers.DETECTION_OPTIMIZE, 0xC3);
        this.WriteRegister(Registers.DEDECTION_THRESHOLD, 0x0A);
      }
      this.WriteRegister(Registers.MODEM_CONFIG_2, (Byte)((this.ReadRegister(Registers.MODEM_CONFIG_2) & 0x0f) | ((sf << 4) & 0xf0)));
      this.SetLdoFlag();
    }

    public Int64 GetSignalBandwidth() {
      Byte bw = (Byte)(this.ReadRegister(Registers.MODEM_CONFIG_1) >> 4);
      switch (bw) {
        case 0: return 7800;
        case 1: return 10400;
        case 2: return 15600;
        case 3: return 20800;
        case 4: return 31250;
        case 5: return 41700;
        case 6: return 62500;
        case 7: return 125000;
        case 8: return 250000;
        case 9: return 500000;
      }
      return 0;
    }

    public void SetSignalBandwith(Int64 sbw) {
      Byte bw;
      if (sbw <= 7800) {
        bw = 0;
      } else if (sbw <= 10400) {
        bw = 1;
      } else if (sbw <= 15600) {
        bw = 2;
      } else if (sbw <= 20800) {
        bw = 3;
      } else if (sbw <= 31250) {
        bw = 4;
      } else if (sbw <= 41700) {
        bw = 5;
      } else if (sbw <= 62500) {
        bw = 6;
      } else if (sbw <= 125000) {
        bw = 7;
      } else if (sbw <= 250000) {
        bw = 8;
      } else /*if (sbw <= 500000)*/ {
        bw = 9;
      }
      this.WriteRegister(Registers.MODEM_CONFIG_1, (Byte)((this.ReadRegister(Registers.MODEM_CONFIG_1) & 0x0f) | (bw << 4)));
      this.SetLdoFlag();
    }

    public void SetCodingRate4(Byte denominator) {
      if (denominator < 5) {
        denominator = 5;
      } else if (denominator > 8) {
        denominator = 8;
      }
      Byte cr = (Byte)(denominator - 4);
      this.WriteRegister(Registers.MODEM_CONFIG_1, (Byte)((this.ReadRegister(Registers.MODEM_CONFIG_1) & 0xF1) | (cr << 1)));
    }

    public void SetPrePaRamp() => this.WriteRegister(Registers.PRE_PA_RAMP, (Byte)((this.ReadRegister(Registers.PRE_PA_RAMP) & 0xF0) | 0x08));

    public void EnableInvertIQ() {
      this.WriteRegister(Registers.INVERTIQ, 0x66);
      this.WriteRegister(Registers.INVERTIQ2, 0x19);
    }

    public void DisableInvertIQ() {
      this.WriteRegister(Registers.INVERTIQ, 0x27);
      this.WriteRegister(Registers.INVERTIQ2, 0x1D);
    }

    public void SetOCP(Byte mA) {
      Byte opcTrim = 27;
      if(mA <= 120) {
        opcTrim = (Byte)((mA - 45) / 5);
      } else if(mA <=240) {
        opcTrim = (Byte)((mA + 30) / 10);
      }
      this.WriteRegister(Registers.OCP, (Byte)(0x20 | (0x1F & opcTrim)));
    }
    #endregion

    #region Debug 
    public Byte Random() => this.ReadRegister(Registers.RSSI_WIDEBAND);

    public String DumpRegisters() {
      String t = "";
      for (Byte i = 0; i < 128; i++) {
        t += "0x";
        t += i.ToString("X");
        t += ": 0x";
        t += this.ReadRegister(i).ToString("X") + "\n";
      }
      return t;
    }
    #endregion

    #region Private Methods
    private Boolean IsTransmitting() {
      if((this.ReadRegister(Registers.OP_MODE) & (Byte)Modes.TX) == (Byte)Modes.TX) {
        return true;
      }
      if((this.ReadRegister(Registers.IRQ_FLAGS) & (Byte)Irq.TX_DONE_MASK) != 0) {
        this.WriteRegister(Registers.IRQ_FLAGS, (Byte)Irq.TX_DONE_MASK);
      }
      return false;
    }

    private void ExplicitHeaderMode() {
      this._implictHeaderMode = false;
      this.WriteRegister(Registers.MODEM_CONFIG_1, (Byte)(this.ReadRegister(Registers.MODEM_CONFIG_1) & 0xfe));
    }

    private void ImplicitHeaderMode() {
      this._implictHeaderMode = true;
      this.WriteRegister(Registers.MODEM_CONFIG_1, (Byte)(this.ReadRegister(Registers.MODEM_CONFIG_1) | 0x01));
    }

    private void HandleOnDio0Rise() {
      if (!this.disposedValue) {
        Byte irqFlags = this.ReadRegister(Registers.IRQ_FLAGS);

        // clear IRQ's
        this.WriteRegister(Registers.IRQ_FLAGS, irqFlags);

        if ((irqFlags & (Byte)Irq.PAYLOAD_CRC_ERROR_MASK) == 0) {
          // received a packet
          this._packetIndex = 0;

          // read packet length
          Byte packetLength = this._implictHeaderMode ? this.ReadRegister(Registers.PAYLOAD_LENGTH) : this.ReadRegister(Registers.RX_NB_BYTES);

          // set FIFO address to current RX address
          this.WriteRegister(Registers.FIFO_ADDR_PTR, this.ReadRegister(Registers.FIFO_RX_CURRENT_ADDR));

          Byte[] ms = new Byte[packetLength];
          for (Byte i = 0; i < packetLength; i++) {
            Int16 c = this.Read();
            if (c != -1) {
              ms[i] = (Byte)c;
            } else {
              throw new Exception("Message to Short");
            }
          }
          Double snr = this.PacketSnr();
          Byte prssi = this.PacketRssi();
          Byte rssi = this.Rssi();
          this.RaiseUpdateEvent(new LoraClientEvent(packetLength, ms, snr, prssi, rssi));

          // reset FIFO address
          this.WriteRegister(Registers.FIFO_ADDR_PTR, 0);
        }
      }
    }

    private void SetLdoFlag() {
      Int64 symbolDuration = 1000 / (this.GetSignalBandwidth() / (1L << this.GetSpreadingFactor()));
      Boolean ldoOn = symbolDuration > 16;
      Byte config3 = this.ReadRegister(Registers.MODEM_CONFIG_3);
      if (ldoOn) {
        config3 |= 1 << 3;
      } else {
        config3 &= Byte.MaxValue ^ (1 << 3);
      }
      this.WriteRegister(Registers.MODEM_CONFIG_3, config3);
    }
    #endregion

    #region Powserusage
    public void Ilde() => this.WriteRegister(Registers.OP_MODE, (Byte)Modes.LONG_RANGE_MODE | (Byte)Modes.STDBY);

    public void Sleep() => this.WriteRegister(Registers.OP_MODE, (Byte)Modes.LONG_RANGE_MODE | (Byte)Modes.SLEEP);

    public override void SetTxPower(Int32 level) {
      if (level > 17) {
        if (level > 20) {
          level = 20;
        }
        level -= 3;
        // High Power +20 dBm Operation (Semtech SX1276/77/78/79 5.4.3.)
        this.WriteRegister(Registers.PA_DAC, 0x87);
        this.SetOCP(140);
      } else {
        if (level < 2) {
          level = 2;
        }
        //Default value PA_HF/LF or +17dBm
        this.WriteRegister(Registers.PA_DAC, 0x84);
      }
      this.WriteRegister(Registers.PA_CONFIG, (Byte)((Byte)Pa.BOOST | (level - 2)));
    }
    #endregion

    #region Communication
    private Byte ReadRegister(Byte address) => this.SingleTransfer((Byte)(address & 0x7F), 0x00);

    private Byte ReadRegister(Registers reg) => this.ReadRegister((Byte)reg);

    private void WriteRegister(Byte address, Byte value) => this.SingleTransfer((Byte)(address | 0x80), value);

    private void WriteRegister(Registers reg, Byte value) => this.WriteRegister((Byte)reg, value);

    private Byte SingleTransfer(Byte address, Byte value) {
      this.Selectreceiver();
      Byte[] spibuf = Pi.Spi.Channel0.SendReceive(new Byte[] { address, value });
      this.Unselectreceiver();
      return spibuf[1];
    }
    #endregion

    #region Hardware IO
    private void Reset() {
      this.PinReset.Write(false);
      System.Threading.Thread.Sleep(100);
      this.PinReset.Write(true);
      System.Threading.Thread.Sleep(100);
    }

    private void SetupIO() {
      Pi.Spi.Channel0Frequency = 250000;
      this.PinSlaveSelect.PinMode = GpioPinDriveMode.Output;
      this.PinDataInput.PinMode = GpioPinDriveMode.Input;
      this.PinReset.PinMode = GpioPinDriveMode.Output;
    }

    private void Selectreceiver() => this.PinSlaveSelect.Write(false);

    private void Unselectreceiver() => this.PinSlaveSelect.Write(true);

    public override void AttachUpdateEvent() {
      if (this.HasAttachedUpdateEvent()) {
        this.WriteRegister(Registers.DIO_MAPPING_1, 0x00);
        this.PinDataInput.RegisterInterruptCallback(EdgeDetection.RisingEdge, this.HandleOnDio0Rise);
      }
    }
    #endregion
  }
}
