using System;
using System.Collections.Generic;
using System.Threading;

using BlubbFish.Utils;

using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;
using Unosquare.WiringPi;

// Semtech SX1276/SX1278
// ../../../doc/DS_SX1276-7-8-9_W_APP_V6.pdf
// The SX1276/77/78/79 transceivers feature the LoRa TM long range modem that provides ultra-long range spread spectrum communication and high interference immunity whilst minimising current consumption.


namespace Fraunhofer.Fit.Iot.Lora.lib.Dragino {
  public partial class Dragino : LoraBoard {
    private GpioPin PinInt0;
    private GpioPin PinInt1;
    private GpioPin PinReset;

    #region Abstracts - LoraBoard
    public Dragino(Dictionary<String, String> settings) : base(settings) {
      Pi.Init<BootstrapWiringPi>();
      this.ParseConfig();
    }

    private void ParseConfig() {
      try {
        this.PinChipSelect = (GpioPin)Pi.Gpio.GetProperty(this.config["pin_sspin"]);
        this.PinInt0 = (GpioPin)Pi.Gpio.GetProperty(this.config["pin_dio0"]);
        this.PinInt1 = (GpioPin)Pi.Gpio.GetProperty(this.config["pin_dio1"]);
        this.PinReset = (GpioPin)Pi.Gpio.GetProperty(this.config["pin_rst"]);
      } catch (Exception e) {
        throw new ArgumentException("Some Argument is not set in settings.ini: " + e.Message);
      }
    }

    public override Boolean Begin() {
      // set module properties
      this.SetupIO(RadioLibTypes.UseSpi, RadioLibTypes.IntBoth);

      // try to find the SX127x chip
      if (!this.FindChip(Constances.ChipVersion)) {
        throw new Exception("No SX127x found!");
      } else {
        this.Debug("Found SX127x!");
      }

      // check active modem
      if (this.GetActiveModem() != Constances.Lora) {
        // set LoRa mode
        Int16 state = this.SetActiveModem(Constances.Lora);
        if (state != Errorcodes.None) {
          throw new Exception("SetActive Modem Failed: " + state);
        }
      }

      // set LoRa sync word
      /*state = SX127x::setSyncWord(syncWord);
      if (state != ERR_NONE) {
        return (state);
      }*/

      // set over current protection
      /*state = SX127x::setCurrentLimit(currentLimit);
      if (state != ERR_NONE) {
        return (state);
      }*/

      // set preamble length
      /*state = SX127x::setPreambleLength(preambleLength);
      if (state != ERR_NONE) {
        return (state);
      }*/

      // initalize internal variables
      //_dataRate = 0.0;

      // configure settings not accessible by API
      /*state = config();
      if (state != ERR_NONE) {
        return (state);
      }*/

      // configure publicly accessible settings
      /*state = setFrequency(freq);
      if (state != ERR_NONE) {
        return (state);
      }*/

      /*state = setBandwidth(bw);
      if (state != ERR_NONE) {
        return (state);
      }*/

      /*state = setSpreadingFactor(sf);
      if (state != ERR_NONE) {
        return (state);
      }*/

      /*state = setCodingRate(cr);
      if (state != ERR_NONE) {
        return (state);
      }*/

      /*state = setOutputPower(power);
      if (state != ERR_NONE) {
        return (state);
      }*/

      /*state = setGain(gain);
      if (state != ERR_NONE) {
        return (state);
      }*/
      return true;
    }

    


    public override void Dispose() => throw new NotImplementedException();
    public override Boolean End() => throw new NotImplementedException();
    public override Boolean Send(Byte[] data, Byte @interface) => throw new NotImplementedException();
    public override Boolean StartRecieving() => throw new NotImplementedException();
    #endregion

    private Boolean FindChip(Byte ver) {
      Byte i = 0;
      Boolean flagFound = false;
      while(i < 10 && !flagFound) {
        Byte version = this.SPIreadRegister(RegisterAdresses.Version);
        if(version == ver) {
          flagFound = true;
        } else {
          Thread.Sleep(1000);
          i++;
        }
      }

      return flagFound;
    }

    private Int16 GetActiveModem() => this.SPIgetRegValue(RegisterAdresses.OpMode, 7, 7);

    private Int16 SetActiveModem(Byte modem) {
      // set mode to SLEEP
      Int16 state = this.SetMode(Constances.Sleep);

      // set modem
      state |= this.SPIsetRegValue(RegisterAdresses.OpMode, modem, 7, 7, 5);

      // set mode to STANDBY
      state |= this.SetMode(Constances.Standby);
      return state;
    }

    private Int16 SetMode(Byte mode) => this.SPIsetRegValue(RegisterAdresses.OpMode, mode, 2, 0, 5);

    private void SetupIO(Byte @interface, Byte gpio) {
      // select interface
      if(@interface == RadioLibTypes.UseSpi) {
        this.PinChipSelect.PinMode = GpioPinDriveMode.Output;
        this.PinChipSelect.Write(GpioPinValue.High);
        Pi.Spi.Channel0Frequency = 250000;
      }
      // select GPIO
      if(gpio == RadioLibTypes.IntBoth) {
        this.PinInt0.PinMode = GpioPinDriveMode.Input;
        this.PinInt1.PinMode = GpioPinDriveMode.Input;
      }
    }
  }
}
