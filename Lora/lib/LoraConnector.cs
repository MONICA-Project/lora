using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Gpio;

namespace Fraunhofer.Fit.Iot.Lora.lib {
  public class LoraConnector {
    private double _frequency = 0.0;
    private int _packetIndex = 0;
    private int _implictHeaderMode = 0;
    private object _onReceive = null;

    enum Registers : Byte {
      FIFO                  = 0x00,
      OP_MODE               = 0x01,
      FRF_MSB               = 0x06,
      FRF_MID               = 0x07,
      FRF_LSB               = 0x08,
      PA_CONFIG             = 0x09,
      LNA                   = 0x0C,
      FIFO_ADDR_PTR         = 0x0D,
      FIFO_TX_BASE_ADDR     = 0x0E,
      FIFO_RX_BASE_ADDR     = 0x0F,
      FIFO_RX_CURRENT_ADDR  = 0x10,
      IRQ_FLAGS             = 0x12,
      RX_NB_BYTES           = 0x13,
      PKT_SNR_VALUE         = 0x19,
      PKT_RSSI_VALUE        = 0x1A,
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
    public LoraConnector() {
      this.SetupIO();
      this.Reset();
      Byte version = ReadRegister((Byte)Registers.VERSION);
      if(version != 0x12) {
        throw new Exception("Wrong Hardware!");
      }
      this.Sleep();
      this.SetFrequency(freq);
      //set base Addr
      this.WriteRegister((Byte)Registers.FIFO_TX_BASE_ADDR, 0);
      this.WriteRegister((Byte)Registers.FIFO_RX_BASE_ADDR, 0);
      //set LNA boost
      this.WriteRegister((Byte)Registers.LNA, (Byte)(ReadRegister((Byte)Registers.LNA) | 0x03));
      //set auto AGC
      this.WriteRegister((Byte)Registers.MODEM_CONFIG_3, 0x04);
      this.SetTxPower(17);
      this.Ilde();
    }
    private Byte ReadRegister(Byte address) {
      return this.SingleTransfer((Byte)(address & 0x7F), 0x00);
    }
    private void WriteRegister(Byte address, Byte value) {
      this.SingleTransfer((Byte)(address | 0x80), value);
    }

    #region Hardware IO
    private void Reset() {
      Pi.Gpio.Pin00.Write(false);
      System.Threading.Thread.Sleep(100);
      Pi.Gpio.Pin00.Write(true);
      System.Threading.Thread.Sleep(100);
    }
    private void SetupIO() {
      Pi.Spi.Channel0Frequency = SpiChannel.MinFrequency;
      //ssPin = 6;
      Pi.Gpio.Pin06.PinMode = GpioPinDriveMode.Output;
      //RST = 0;
      Pi.Gpio.Pin00.PinMode = GpioPinDriveMode.Output;
    }
    private Byte SingleTransfer(Byte address, Byte value) {
      Selectreceiver();
      Byte[] spibuf = Pi.Spi.Channel0.SendReceive(new Byte[] { address, 0x00 });
      Unselectreceiver();
      return spibuf[1];
    }
    private void Selectreceiver() {
      Pi.Gpio.Pin06.Write(false);
    }
    private void Unselectreceiver() {
      Pi.Gpio.Pin06.Write(true);
    }
    #endregion
  }
}
