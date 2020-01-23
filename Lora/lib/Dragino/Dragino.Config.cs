using System;
using System.Threading;

using BlubbFish.Utils;

using Unosquare.RaspberryIO;
using Unosquare.WiringPi;

namespace Fraunhofer.Fit.Iot.Lora.lib.Dragino {
  public partial class Dragino {
    private GpioPin PinInt0;
    private GpioPin PinReset;

    private Byte _syncWord = Constances.SX127X_SYNC_WORD;
    private Byte _currentLimit = 100;
    private UInt16 _preambleLength = 8;
    private Int32 _freq = 434000000;
    private Int32 _bw = Bandwidths.Freq_125k;
    private Byte _sf = SpreadingFactors.SF9;
    private Byte _cr = Codingrates.CR7;
    private SByte _power = 17;
    private Byte _gain = 0;
    private readonly Double _br = 48;

    private readonly Object HandleRecievedDataLock = new Object();
    private Thread _recieverThread;
    private Boolean _recieverThreadRunning = false;
    private Boolean _isrecieving = false;
    private Boolean _istransmitting = false;

    private Byte _packetLength = 0;
    private Boolean _packetLengthQueried = false;
    private Double _dataRate = 0;
    private Boolean _headerexplict = false;

    private void ParseConfig() {
      try {
        this.PinChipSelect = (GpioPin)Pi.Gpio.GetProperty(this.config["pin_sspin"]);
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
        this._gain = Byte.Parse(this.config["gain"]);
      } catch (Exception e) {
        throw new ArgumentException("Some Argument is not set in settings.ini: " + e.Message);
      }
    }
  }
}
