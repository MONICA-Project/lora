using System;
using BlubbFish.Utils;
using Unosquare.RaspberryIO;
using Unosquare.WiringPi;

namespace Fraunhofer.Fit.Iot.Lora.lib.Dragino {
  public partial class Dragino {
    private Byte _syncWord = Constances.SX127X_SYNC_WORD;
    private Byte _currentLimit = 100;
    private UInt16 _preambleLength = 8;
    private Double _dataRate;
    private Int32 _freq = 434000000;

    private void ParseConfig() {
      try {
        this.PinChipSelect = (GpioPin)Pi.Gpio.GetProperty(this.config["pin_sspin"]);
        this.PinInt0 = (GpioPin)Pi.Gpio.GetProperty(this.config["pin_dio0"]);
        this.PinInt1 = (GpioPin)Pi.Gpio.GetProperty(this.config["pin_dio1"]);
        this.PinReset = (GpioPin)Pi.Gpio.GetProperty(this.config["pin_rst"]);
        this._syncWord = Byte.Parse(this.config["syncword"]);
      } catch (Exception e) {
        throw new ArgumentException("Some Argument is not set in settings.ini: " + e.Message);
      }
    }
  }
}
