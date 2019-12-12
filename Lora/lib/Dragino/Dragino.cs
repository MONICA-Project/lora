using System;
using System.Collections.Generic;
using BlubbFish.Utils;
using Unosquare.RaspberryIO;
using Unosquare.WiringPi;

// Semtech SX1276/SX1278
// ../../../doc/DS_SX1276-7-8-9_W_APP_V6.pdf
// The SX1276/77/78/79 transceivers feature the LoRa TM long range modem that provides ultra-long range spread spectrum communication and high interference immunity whilst minimising current consumption.


namespace Fraunhofer.Fit.Iot.Lora.lib.Dragino {
  public class Dragino : LoraBoard {
    private GpioPin PinSlaveSelect;
    private GpioPin PinDataInput;
    private GpioPin PinReset;

    #region Abstracts - LoraBoard
    public Dragino(Dictionary<String, String> settings) : base(settings) {
      Pi.Init<BootstrapWiringPi>();
      this.ParseConfig();
    }

    private void ParseConfig() {
      try {
        this.PinSlaveSelect = (GpioPin)Pi.Gpio.GetProperty(this.config["pin_sspin"]);
        this.PinDataInput = (GpioPin)Pi.Gpio.GetProperty(this.config["pin_dio0"]);
        this.PinReset = (GpioPin)Pi.Gpio.GetProperty(this.config["pin_rst"]);
      } catch (Exception e) {
        throw new ArgumentException("Some Argument is not set in settings.ini: " + e.Message);
      }
    }

    public override Boolean Begin() {
      // set module properties
      //_mod->init(RADIOLIB_USE_SPI, RADIOLIB_INT_BOTH);

      // try to find the SX127x chip
      /*if (!SX127x::findChip(chipVersion)) {
        RADIOLIB_DEBUG_PRINTLN(F("No SX127x found!"));
        _mod->term();
        return (ERR_CHIP_NOT_FOUND);
      } else {
        RADIOLIB_DEBUG_PRINTLN(F("Found SX127x!"));
      }*/

      // check active modem
      /*int16_t state;
      if (getActiveModem() != SX127X_LORA) {
        // set LoRa mode
        state = setActiveModem(SX127X_LORA);
        if (state != ERR_NONE) {
          return (state);
        }
      }*/

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
  }
}
