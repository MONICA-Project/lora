using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Gpio;

namespace Fraunhofer.Fit.Iot.Lora.lib {
  public class LoraConnector {
    enum Registers : Byte {
      REG_VERSION = 0x42
    };
    public LoraConnector() {
      Pi.Spi.Channel0Frequency = SpiChannel.MinFrequency;
      //ssPin = 6;
      Pi.Gpio.Pin06.PinMode = GpioPinDriveMode.Output;
      //RST = 0;
      Pi.Gpio.Pin00.PinMode = GpioPinDriveMode.Output;

      Pi.Gpio.Pin00.Write(false);
      System.Threading.Thread.Sleep(100);
      Pi.Gpio.Pin00.Write(true);
      System.Threading.Thread.Sleep(100);

      
      Byte version = ReadRegister((Byte)Registers.REG_VERSION);
      Console.WriteLine(version);
    }
    private Byte ReadRegister(Byte address) {
      return this.SingleTransfer((Byte)(address & 0x7F), 0x00);
    }
    private void WriteRegister(Byte address, Byte value) {
      this.SingleTransfer((Byte)(address | 0x80), value);
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
  }
}
