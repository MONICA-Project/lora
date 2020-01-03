using System;
using System.Threading;
using BlubbFish.Utils;

using Unosquare.RaspberryIO;
using Unosquare.WiringPi;

namespace Fraunhofer.Fit.Iot.Lora.lib.Ic880a {
  public partial class Ic880a {
    private GpioPin PinReset;

    private readonly Boolean[] _radioEnabled = new Boolean[2];
    private readonly UInt32[] _radioFrequency = new UInt32[2];
    private readonly RadioType[] _rf_radio_type = new RadioType[] { RadioType.SX1257, RadioType.SX1257 };

    private readonly Boolean[] _radioEnableTx = new Boolean[2];
    private readonly Boolean[] _interfaceEnabled = new Boolean[10];
    private readonly Reciever[] _interfaceChain = new Reciever[10];
    
    private readonly Int32[] _interfaceFrequency = new Int32[10];
    private BW _loraBandwidth = BW.BW_250KHZ;
    private BW _fskBandwidth = BW.BW_125KHZ;
    private SF _loraSpreadingFactor = SF.DR_LORA_SF7;
    private UInt32 _fskDatarate = 50000;
    private readonly Byte _fskSyncWordSize = 3;
    private readonly UInt64 _fskSyncWord = 0xC194C1;
    private Boolean _CrcEnabled = true;
    private Boolean _lorawan_public = false;
    

    #region Sending Parameters
    private readonly SByte[] _cal_offset_a_i = new SByte[8];
    private readonly SByte[] _cal_offset_a_q = new SByte[8];
    private readonly SByte[] _cal_offset_b_i = new SByte[8];
    private readonly SByte[] _cal_offset_b_q = new SByte[8];
    private readonly Lutstruct[] _lut = new Lutstruct[2] { new Lutstruct(0, 2, 3, 10, 14), new Lutstruct(0, 3, 3, 14, 27) };
    private readonly Lgw_sx127x_FSK_bandwidth_s[] sx127x_FskBandwidths = {
      new Lgw_sx127x_FSK_bandwidth_s(2600  , 2, 7),
      new Lgw_sx127x_FSK_bandwidth_s(2600  , 2, 7 ),   /* LGW_SX127X_RXBW_2K6_HZ */
      new Lgw_sx127x_FSK_bandwidth_s(3100  , 1, 7 ),   /* LGW_SX127X_RXBW_3K1_HZ */
      new Lgw_sx127x_FSK_bandwidth_s(3900  , 0, 7 ),   /* ... */
      new Lgw_sx127x_FSK_bandwidth_s(5200  , 2, 6 ),
      new Lgw_sx127x_FSK_bandwidth_s(6300  , 1, 6 ),
      new Lgw_sx127x_FSK_bandwidth_s(7800  , 0, 6 ),
      new Lgw_sx127x_FSK_bandwidth_s(10400 , 2, 5 ),
      new Lgw_sx127x_FSK_bandwidth_s(12500 , 1, 5 ),
      new Lgw_sx127x_FSK_bandwidth_s(15600 , 0, 5 ),
      new Lgw_sx127x_FSK_bandwidth_s(20800 , 2, 4 ),
      new Lgw_sx127x_FSK_bandwidth_s(25000 , 1, 4 ),   /* ... */
      new Lgw_sx127x_FSK_bandwidth_s(31300 , 0, 4 ),
      new Lgw_sx127x_FSK_bandwidth_s(41700 , 2, 3 ),
      new Lgw_sx127x_FSK_bandwidth_s(50000 , 1, 3 ),
      new Lgw_sx127x_FSK_bandwidth_s(62500 , 0, 3 ),
      new Lgw_sx127x_FSK_bandwidth_s(83333 , 2, 2 ),
      new Lgw_sx127x_FSK_bandwidth_s(100000, 1, 2 ),
      new Lgw_sx127x_FSK_bandwidth_s(125000, 0, 2 ),
      new Lgw_sx127x_FSK_bandwidth_s(166700, 2, 1 ),
      new Lgw_sx127x_FSK_bandwidth_s(200000, 1, 1 ),   /* ... */
      new Lgw_sx127x_FSK_bandwidth_s(250000, 0, 1 )    /* LGW_SX127X_RXBW_250K_HZ */
    };
    #endregion

    private Boolean _lbt_enabled = false;
    private SByte _lbt_rssi_offset_dB = 0;
    private UInt32 _lbt_start_freq = 0;
    private SByte _lbt_rssi_target_dBm = 0;
    private Byte _lbt_nb_active_channel = 0;
    private readonly LbtChan[] _lbt_channel_cfg = new LbtChan[8];
    private Boolean _tx_notch_support = false;
    private Byte _tx_notch_offset;

    private Thread _recieverThread;
    private Boolean _recieverThreadRunning = false;
    private Boolean _isrecieving = false;
    private Boolean _istransmitting = false;
    private readonly Object HandleControllerIOLock = new Object();
    private Boolean _deviceStarted = false;

    private Byte _selectedPage;

    

    private void ParseConfig() {
      try {
        this.SpiChannel = (SpiChannel)Pi.Spi.GetProperty(this.config["spichan"]);
        this.PinChipSelect = (GpioPin)Pi.Gpio[Int32.Parse(this.config["pin_sspin"])];  //Physical pin 24, BCM pin  8, Wiring Pi pin 10 (SPI0 CE0)
        this.PinReset = (GpioPin)Pi.Gpio[Int32.Parse(this.config["pin_rst"])];          //Physical pin 29, BCM pin  5, Wiring Pi pin 21 (GPCLK1)
        if(this.config.ContainsKey("frequency0")) {
          this._radioEnabled[0] = true;
          this._radioFrequency[0] = UInt32.Parse(this.config["frequency0"]);
        }
        if(this.config.ContainsKey("frequency1")) {
          this._radioEnabled[1] = true;
          this._radioFrequency[1] = UInt32.Parse(this.config["frequency1"]);
        }

        for(Byte i = 0; i < 10; i++) {
          if(this.config.ContainsKey("interface" + i + "frequency")) {
            Int32 offset = Int32.Parse(this.config["interface" + i + "frequency"]);
            if(offset >= -500000 && offset <= 500000) {
              this._interfaceFrequency[i] = offset;
            } else {
              throw new ArgumentException("interface" + i + "frequency: Offset " + offset + " is not allowed!");
            }
            this._interfaceEnabled[i] = true;
            Byte chain = Byte.Parse(this.config["interface" + i + "chain"]);
            if(chain == 0) {
              this._interfaceChain[i] = Reciever.Chain0;
            } else if(chain == 1) {
              this._interfaceChain[i] = Reciever.Chain1;
            } else {
              throw new ArgumentException("interface" + i + "chain: Chain " + chain + " is not allowed!");
            }
          }
        }

        Int32 lbwc = Int32.Parse(this.config["lorabandwith"]);
        this._loraBandwidth = lbwc <= 7800 ? BW.BW_7K8HZ : lbwc <= 15600 ? BW.BW_15K6HZ : lbwc <= 31250 ? BW.BW_31K2HZ : lbwc <= 62500 ? BW.BW_62K5HZ : lbwc <= 125000 ? BW.BW_125KHZ : lbwc <= 250000 ? BW.BW_250KHZ : BW.BW_500KHZ;

        Byte sf = Byte.Parse(this.config["loraspreadingfactor"]);
        this._loraSpreadingFactor = sf <= 7 ? SF.DR_LORA_SF7 : sf <= 8 ? SF.DR_LORA_SF8 : sf <= 9 ? SF.DR_LORA_SF9 : sf <= 10 ? SF.DR_LORA_SF10 : sf <= 11 ? SF.DR_LORA_SF11 : SF.DR_LORA_SF12;

        Int32 fbwc = Int32.Parse(this.config["fskbandwith"]);
        this._fskBandwidth = fbwc <= 7800 ? BW.BW_7K8HZ : fbwc <= 15600 ? BW.BW_15K6HZ : fbwc <= 31250 ? BW.BW_31K2HZ : fbwc <= 62500 ? BW.BW_62K5HZ : fbwc <= 125000 ? BW.BW_125KHZ : fbwc <= 250000 ? BW.BW_250KHZ : BW.BW_500KHZ;

        this._fskDatarate = UInt32.Parse(this.config["fskdatarate"]);

        this._CrcEnabled = Boolean.Parse(this.config["crc"]);


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
