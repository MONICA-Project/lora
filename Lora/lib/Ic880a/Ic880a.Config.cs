using System;
using System.Threading;
using BlubbFish.Utils;

using Unosquare.RaspberryIO;
using Unosquare.WiringPi;

namespace Fraunhofer.Fit.Iot.Lora.lib.Ic880a {
  public partial class Ic880a {
    private GpioPin PinSlaveSelect;
    private GpioPin PinReset;

    private readonly Boolean[] _radioEnabled = new Boolean[2];
    private readonly Reciever[] _interfaceChain = new Reciever[10];
    private readonly UInt32[] _radioFrequency = new UInt32[2];
    private readonly Int32[] _interfaceFrequency = new Int32[10];
    private BW _loraBandwidth = BW.BW_250KHZ;
    private BW _fskBandwidth = BW.BW_125KHZ;
    private UInt32 _fskDatarate = 50000;
    private Boolean _CrcEnabled = true;

    private Thread _recieverThread;
    private Boolean _recieverThreadRunning = false;
    private Boolean _isrecieving = false;
    private Boolean _deviceStarted = false;

    private Byte _selectedPage;

    

    private void ParseConfig() {
      try {
        this.SpiChannel = (SpiChannel)Pi.Spi.GetProperty(this.config["spichan"]);
        this.PinSlaveSelect = (GpioPin)Pi.Gpio.GetProperty(this.config["pin_sspin"]);  //Physical pin 24, BCM pin  8, Wiring Pi pin 10 (SPI0 CE0)
        this.PinReset = (GpioPin)Pi.Gpio.GetProperty(this.config["pin_rst"]);          //Physical pin 29, BCM pin  5, Wiring Pi pin 21 (GPCLK1)
        /*this.PinChipSelect = (GpioPin)Pi.Gpio.GetProperty(this.config["pin_sspin"]);
        this.PinInt0 = (GpioPin)Pi.Gpio.GetProperty(this.config["pin_dio0"]);
        this.PinReset = (GpioPin)Pi.Gpio.GetProperty(this.config["pin_rst"]);
        this.SpiChannel = (SpiChannel)Pi.Spi.GetProperty(this.config["spichan"]);

        this._freq = Int32.Parse(this.config["freq"]);
        this._sf = Byte.Parse(this.config["sf"]);
        this._bw = Int32.Parse(this.config["bw"]);
        this._cr = Byte.Parse(this.config["cr"]);

        this._syncWord = Byte.Parse(this.config["syncword"]);
        this._preambleLength = UInt16.Parse(this.config["preamblelength"]);

        this._currentLimit = Byte.Parse(this.config["currentlimit"]);
        this._power = SByte.Parse(this.config["power"]);
        this._gain = Byte.Parse(this.config["gain"]);*/
      } catch(Exception e) {
        throw new ArgumentException("Some Argument is not set in settings.ini: " + e.Message);
      }
    }
  }
}
