using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fraunhofer.Fit.Iot.Lora.Events;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Gpio;

namespace Fraunhofer.Fit.Iot.Lora.lib {
  public class LoraConnector {
    private Int64 _frequency = 0;
    private Byte _packetIndex = 0;
    private Boolean _implictHeaderMode = false;
    public delegate void DataUpdate(Object sender, DeviceUpdateEvent e);
    public event DataUpdate Update;

    enum Registers : Byte {
      FIFO                  = 0x00,
      OP_MODE               = 0x01,
      FRF_MSB               = 0x06,
      FRF_MID               = 0x07,
      FRF_LSB               = 0x08,
      PA_CONFIG             = 0x09,
      PRE_PA_RAMP           = 0x0A,
      LNA                   = 0x0C,
      FIFO_ADDR_PTR         = 0x0D,
      FIFO_TX_BASE_ADDR     = 0x0E,
      FIFO_RX_BASE_ADDR     = 0x0F,
      FIFO_RX_CURRENT_ADDR  = 0x10,
      IRQ_FLAGS             = 0x12,
      RX_NB_BYTES           = 0x13,
      PKT_SNR_VALUE         = 0x19,
      PKT_RSSI_VALUE        = 0x1A,
      RSSI_VALUE            = 0x1B,
      MODEM_CONFIG_1        = 0x1D,
      MODEM_CONFIG_2        = 0x1E,
      PREAMBLE_MSB          = 0x20,
      PREAMBLE_LSB          = 0x21,
      PAYLOAD_LENGTH        = 0x22,
      MODEM_CONFIG_3        = 0x26,
      FREQ_ERROR_MSB        = 0x28,
      FREQ_ERROR_MID        = 0x29,
      FREQ_ERROR_LSB        = 0x2A,
      RSSI_WIDEBAND         = 0x2C,
      DETECTION_OPTIMIZE    = 0x31,
      DEDECTION_THRESHOLD   = 0x37,
      SYNC_WORD             = 0x39,
      DIO_MAPPING_1         = 0x40,
      VERSION               = 0x42
    };

    enum Modes : Byte {
      SLEEP           = 0x00,
      STDBY           = 0x01,
      TX              = 0x03,
      RX_CONTINOUS    = 0x05,
      RX_SINGLE       = 0x06,
      LONG_RANGE_MODE = 0x80
    };

    enum Pa : Byte {
      BOOST = 0x80
    };

    enum Irq : Byte {
      TX_DONE_MASK = 0x08,
      PAYLOAD_CRC_ERROR_MASK = 0x20,
      RX_DONE_MASK = 0x40
    }

    public LoraConnector(Int64 freq) {
      Console.WriteLine("LoraConnector.LoraConnector("+ freq + ")");
      this.SetupIO();
      this.Reset();
      Byte version = ReadRegister(Registers.VERSION);
      if(version != 0x12) {
        throw new Exception("Wrong Hardware!");
      }
      this.Sleep();
      this.SetFrequency(freq);
      //set base Addr
      this.WriteRegister(Registers.FIFO_TX_BASE_ADDR, 0);
      this.WriteRegister(Registers.FIFO_RX_BASE_ADDR, 0);
      //set LNA boost
      this.WriteRegister(Registers.LNA, (Byte)(ReadRegister(Registers.LNA) | 0x03));
      //set auto AGC
      this.WriteRegister(Registers.MODEM_CONFIG_3, 0x04);
      this.SetTxPower(17);
      this.Ilde();
    }
    
    #region Methods
    public void Sleep() {
      Console.WriteLine("LoraConnector.Sleep()");
      this.WriteRegister(Registers.OP_MODE, (Byte)Modes.LONG_RANGE_MODE | (Byte)Modes.SLEEP);
    }
    private void Ilde() {
      Console.WriteLine("LoraConnector.Ilde()");
      this.WriteRegister(Registers.OP_MODE, (Byte)Modes.LONG_RANGE_MODE | (Byte)Modes.STDBY);
    }
    public void SetFrequency(Int64 freq) {
      Console.WriteLine("LoraConnector.SetFrequencey(" + freq + ")");
      this._frequency = freq;

      UInt64 frf = ((UInt64)freq << 19) / 32000000;
      this.WriteRegister(Registers.FRF_MSB, (Byte)(frf >> 16));
      this.WriteRegister(Registers.FRF_MID, (Byte)(frf >> 8));
      this.WriteRegister(Registers.FRF_LSB, (Byte)(frf >> 0));
    }
    public void SetTxPower(Int32 level, Int32 outputPin = 1) {
      Console.WriteLine("LoraConnector.SetTxPower(" + level + "," + outputPin + ")");
      if (outputPin == 1) {
        if(level < 0) {
          level = 0;
        } else if(level > 14) {
          level = 14;
        }
        this.WriteRegister(Registers.PA_CONFIG, (Byte)(0x70 | level));
      } else {
        if(level < 2) {
          level = 2;
        } else if(level > 17) {
          level = 17;
        }
        this.WriteRegister(Registers.PA_CONFIG, (Byte)((Byte)Pa.BOOST | (level - 2)));
      }
    }
    public void EnableCrc() {
      Console.WriteLine("LoraConnector.EnableCrc()");
      this.WriteRegister(Registers.MODEM_CONFIG_2, (Byte)(this.ReadRegister(Registers.MODEM_CONFIG_2) | 0x04));
    }
    public void SetPrePaRamp() {
      Console.WriteLine("LoraConnector.SetPreRamp()");
      this.WriteRegister(Registers.PRE_PA_RAMP, (Byte)((this.ReadRegister(Registers.PRE_PA_RAMP) & 0xF0) | 0x08));
    }
    public void Receive(Byte size) {
      Console.WriteLine("LoraConnector.Receive(" + size + ")");
      if (size > 0) {
        this.ImplicitHeaderMode();
        this.WriteRegister(Registers.PAYLOAD_LENGTH, (Byte)(size & 0xff));
      } else {
        this.ExplicitHeaderMode();
      }
      this.WriteRegister(Registers.OP_MODE, (Byte)Modes.LONG_RANGE_MODE | (Byte)Modes.RX_CONTINOUS);
    }
    public void ExplicitHeaderMode() {
      Console.WriteLine("LoraConnector.ExplictHeaderMode()");
      this._implictHeaderMode = false;
      this.WriteRegister(Registers.MODEM_CONFIG_1, (Byte)(this.ReadRegister(Registers.MODEM_CONFIG_1) & 0xfe));
    }

    public void ImplicitHeaderMode() {
      Console.WriteLine("LoraConnector.ImplictHeaderMode()");
      this._implictHeaderMode = true;
      this.WriteRegister(Registers.MODEM_CONFIG_1, (Byte)(this.ReadRegister(Registers.MODEM_CONFIG_1) | 0x01));
    }

    private void OnDio0Rise() {
      Console.WriteLine("LoraConnector.OnDio0Rise()");
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
        for(Byte i = 0; i < packetLength; i++) {
          Int16 c = this.Read();
          if(c != -1) {
            ms[i] = (Byte)c;
          } else {
            throw new Exception("Message to Short");
          }
        }
        Double snr = this.PacketSnr();
        Byte prssi = this.PacketRssi();
        Byte rssi = this.Rssi();
        this.Update?.Invoke(this, new DeviceUpdateEvent(packetLength, Encoding.ASCII.GetString(ms), snr, prssi, rssi));
        
        // reset FIFO address
        this.WriteRegister(Registers.FIFO_ADDR_PTR, 0);
      }
    }
    #endregion

    #region Communication
    private Byte ReadRegister(Byte address) {
      Console.WriteLine("LoraConnector.ReadRegister(" + address + ")");
      return this.SingleTransfer((Byte)(address & 0x7F), 0x00);
    }
    private Byte ReadRegister(Registers reg) {
      return ReadRegister((Byte)reg);
    }
    private void WriteRegister(Byte address, Byte value) {
      Console.WriteLine("LoraConnector.WriteRegister(" + address + ","+value+")");
      this.SingleTransfer((Byte)(address | 0x80), value);
    }
    private void WriteRegister(Registers reg, Byte value) {
      this.WriteRegister((Byte)reg, value);
    }
    public Int16 Read() {
      Console.WriteLine("LoraConnector.Read()");
      if (this.Available() == 0) {
        return -1;
      }
      this._packetIndex++;
      return this.ReadRegister(Registers.FIFO);
    }
    public Byte Available() {
      Console.WriteLine("LoraConnector.Available()");
      return (Byte)(this.ReadRegister(Registers.RX_NB_BYTES) - this._packetIndex);
    }
    public Double PacketSnr() {
      Console.WriteLine("LoraConnector.PacketSnr()");
      return ((SByte)this.ReadRegister(Registers.PKT_SNR_VALUE)) * 0.25;
    }
    public Byte PacketRssi() {
      Console.WriteLine("LoraConnector.PacketRssi()");
      return (Byte)(this.ReadRegister(Registers.PKT_RSSI_VALUE) - (this._frequency < 868E6 ? 164 : 157));
    }
    public Byte Rssi() {
      Console.WriteLine("LoraConnector.Rssi()");
      return (Byte)(this.ReadRegister(Registers.RSSI_VALUE) - (this._frequency < 868E6 ? 164 : 157));
    }
    #endregion

    #region Hardware IO
    private void Reset() {
      Console.WriteLine("LoraConnector.Reset()");
      Pi.Gpio.Pin00.Write(false);
      System.Threading.Thread.Sleep(100);
      Pi.Gpio.Pin00.Write(true);
      System.Threading.Thread.Sleep(100);
    }

    private void SetupIO() {
      Console.WriteLine("LoraConnector.SetupIO()");
      Pi.Spi.Channel0Frequency = SpiChannel.MinFrequency;
      //ssPin = 6;
      Pi.Gpio.Pin06.PinMode = GpioPinDriveMode.Output;
      //RST = 0;
      Pi.Gpio.Pin00.PinMode = GpioPinDriveMode.Output;
      //dio0 = 7;
      Pi.Gpio.Pin07.PinMode = GpioPinDriveMode.Input;
    }

    private Byte SingleTransfer(Byte address, Byte value) {
      Console.WriteLine("LoraConnector.singleTransfer(" + address + ","+value+")");
      Selectreceiver();
      Byte[] spibuf = Pi.Spi.Channel0.SendReceive(new Byte[] { address, 0x00 });
      Unselectreceiver();
      return spibuf[1];
    }

    private void Selectreceiver() {
      Console.WriteLine("LoraConnector.Selectreciever()");
      Pi.Gpio.Pin06.Write(false);
    }

    private void Unselectreceiver() {
      Console.WriteLine("LoraConnector.Unselectreciever()");
      Pi.Gpio.Pin06.Write(true);
    }

    public void OnReceive() {
      Console.WriteLine("LoraConnector.OnReceive()");
      if (this.Update != null) {
        this.WriteRegister(Registers.DIO_MAPPING_1, 0x00);
        Pi.Gpio.Pin07.RegisterInterruptCallback(EdgeDetection.RisingEdge, this.OnDio0Rise);
      }
    }
    #endregion
  }
}
