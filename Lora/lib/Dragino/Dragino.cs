using System;
using System.Collections.Generic;
using System.Threading;

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

    public override Boolean Begin() {
      // set module properties
      this.SetupIO(RadioLibTypes.RADIOLIB_USE_SPI, RadioLibTypes.RADIOLIB_INT_BOTH);

      // try to find the SX127x chip
      if (!this.FindChip(Constances.SX1278_CHIP_VERSION)) {
        throw new Exception("No SX127x found!");
      } else {
        this.Debug("Found SX127x!");
      }

      // check active modem
      Int16 state;
      if (this.GetActiveModem() != Constances.SX127X_LORA) {
        // set LoRa mode
        state = this.SetActiveModem(Constances.SX127X_LORA);
        if (state != Errorcodes.ERR_NONE) {
          throw new Exception("SetActiveModem Failed: " + state);
        }
      }

      // set LoRa sync word
      state = this.SetSyncWord(this._syncWord);
      if (state != Errorcodes.ERR_NONE) {
        throw new Exception("SetSyncWord Failed: " + state);
      }

      // set over current protection
      state = this.SetCurrentLimit(this._currentLimit);
      if (state != Errorcodes.ERR_NONE) {
        throw new Exception("SetCurrentLimit Failed: " + state);
      }

      // set preamble length
      state = this.SetPreambleLength(this._preambleLength);
      if (state != Errorcodes.ERR_NONE) {
        throw new Exception("SetPreambleLength Failed: " + state);
      }

      // configure settings not accessible by API
      state = this.Config();
      if (state != Errorcodes.ERR_NONE) {
        throw new Exception("Config Failed: " + state);
      }

      // configure publicly accessible settings
      state = this.SetFrequency(this._freq);
      if (state != Errorcodes.ERR_NONE) {
        throw new Exception("SetFrequency Failed: " + state);
      }

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
        Byte version = this.SPIreadRegister(RegisterAdresses.SX127X_REG_VERSION);
        if(version == ver) {
          flagFound = true;
        } else {
          Thread.Sleep(1000);
          i++;
        }
      }

      return flagFound;
    }

    private Int16 GetActiveModem() => this.SPIgetRegValue(RegisterAdresses.SX127X_REG_OP_MODE, 7, 7);

    private Int16 SetActiveModem(Byte modem) {
      // set mode to SLEEP
      Int16 state = this.SetMode(Constances.SX127X_SLEEP);

      // set modem
      state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_OP_MODE, modem, 7, 7, 5);

      // set mode to STANDBY
      state |= this.SetMode(Constances.SX127X_STANDBY);
      return state;
    }

    private Int16 SetMode(Byte mode) => this.SPIsetRegValue(RegisterAdresses.SX127X_REG_OP_MODE, mode, 2, 0, 5);

    private Int16 SetSyncWord(Byte syncWord) {
      // check active modem
      if (this.GetActiveModem() != Constances.SX127X_LORA) {
        return Errorcodes.ERR_WRONG_MODEM;
      }

      // set mode to standby
      _ = this.SetMode(Constances.SX127X_STANDBY);

      // write register
      return this.SPIsetRegValue(RegisterAdresses.SX127X_REG_SYNC_WORD, syncWord);
    }

    private Int16 SetCurrentLimit(Byte currentLimit) {
      // check allowed range
      if (!(currentLimit >= 45 && currentLimit <= 240 || currentLimit == 0)) {
        return Errorcodes.ERR_INVALID_CURRENT_LIMIT;
      }

      // set mode to standby
      Int16 state = this.SetMode(Constances.SX127X_STANDBY);

      // set OCP limit
      Byte raw;
      if (currentLimit == 0) {
        // limit set to 0, disable OCP
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_OCP, Constances.SX127X_OCP_OFF, 5, 5);
      } else if (currentLimit <= 120) {
        raw = (Byte)(Constances.SX127X_OCP_ON | ((currentLimit - 45) / 5));
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_OCP, raw, 5, 0);
      } else if (currentLimit <= 240) {
        raw = (Byte)(Constances.SX127X_OCP_ON | ((currentLimit + 30) / 10));
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_OCP, raw, 5, 0);
      }
      return state;
    }

    private Int16 SetPreambleLength(UInt16 preambleLength) {
      // set mode to standby
      Int16 state = this.SetMode(Constances.SX127X_STANDBY);
      if (state != Errorcodes.ERR_NONE) {
        return state;
      }

      // check active modem
      Int16 modem = this.GetActiveModem();
      if (modem == Constances.SX127X_LORA) {
        // check allowed range
        if (preambleLength < 6) {
          return Errorcodes.ERR_INVALID_PREAMBLE_LENGTH;
        }

        // set preamble length
        state = this.SPIsetRegValue(RegisterAdresses.SX127X_REG_PREAMBLE_MSB, (Byte)((preambleLength >> 8) & 0xFF));
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_PREAMBLE_LSB, (Byte)(preambleLength & 0xFF));
        return state;

      } else if (modem == Constances.SX127X_FSK_OOK) {
        // set preamble length
        state = this.SPIsetRegValue(RegisterAdresses.SX127X_REG_PREAMBLE_MSB_FSK, (Byte)((preambleLength >> 8) & 0xFF));
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_PREAMBLE_LSB_FSK, (Byte)(preambleLength & 0xFF));
        return state;
      }

      return Errorcodes.ERR_UNKNOWN;
    }

    private Int16 Config() => this.SPIsetRegValue(RegisterAdresses.SX127X_REG_HOP_PERIOD, Constances.SX127X_HOP_PERIOD_OFF);

    private Int16 SetFrequency(Int32 freq) {
      // check frequency range
      if (freq < 137000000 || freq > 1020000000) {
        return Errorcodes.ERR_INVALID_FREQUENCY;
      }

      // SX1276/77/78 Errata fixes
      if (this.GetActiveModem() == Constances.SX127X_LORA) {
        // sensitivity optimization for 500kHz bandwidth
        // see SX1276/77/78 Errata, section 2.1 for details
        if (abs(_bw - 500.0) <= 0.001) {
          if ((freq >= 862.0) && (freq <= 1020.0)) {
            _mod->SPIwriteRegister(0x36, 0x02);
            _mod->SPIwriteRegister(0x3a, 0x64);
          } else if ((freq >= 410.0) && (freq <= 525.0)) {
            _mod->SPIwriteRegister(0x36, 0x02);
            _mod->SPIwriteRegister(0x3a, 0x7F);
          }
        }

        // mitigation of receiver spurious response
        // see SX1276/77/78 Errata, section 2.3 for details
        if (abs(_bw - 7.8) <= 0.001) {
          _mod->SPIsetRegValue(0x31, 0b0000000, 7, 7);
          _mod->SPIsetRegValue(0x2F, 0x48);
          _mod->SPIsetRegValue(0x30, 0x00);
          freq += 7.8;
        } else if (abs(_bw - 10.4) <= 0.001) {
          _mod->SPIsetRegValue(0x31, 0b0000000, 7, 7);
          _mod->SPIsetRegValue(0x2F, 0x44);
          _mod->SPIsetRegValue(0x30, 0x00);
          freq += 10.4;
        } else if (abs(_bw - 15.6) <= 0.001) {
          _mod->SPIsetRegValue(0x31, 0b0000000, 7, 7);
          _mod->SPIsetRegValue(0x2F, 0x44);
          _mod->SPIsetRegValue(0x30, 0x00);
          freq += 15.6;
        } else if (abs(_bw - 20.8) <= 0.001) {
          _mod->SPIsetRegValue(0x31, 0b0000000, 7, 7);
          _mod->SPIsetRegValue(0x2F, 0x44);
          _mod->SPIsetRegValue(0x30, 0x00);
          freq += 20.8;
        } else if (abs(_bw - 31.25) <= 0.001) {
          _mod->SPIsetRegValue(0x31, 0b0000000, 7, 7);
          _mod->SPIsetRegValue(0x2F, 0x44);
          _mod->SPIsetRegValue(0x30, 0x00);
          freq += 31.25;
        } else if (abs(_bw - 41.7) <= 0.001) {
          _mod->SPIsetRegValue(0x31, 0b0000000, 7, 7);
          _mod->SPIsetRegValue(0x2F, 0x44);
          _mod->SPIsetRegValue(0x30, 0x00);
          freq += 41.7;
        } else if (abs(_bw - 62.5) <= 0.001) {
          _mod->SPIsetRegValue(0x31, 0b0000000, 7, 7);
          _mod->SPIsetRegValue(0x2F, 0x40);
          _mod->SPIsetRegValue(0x30, 0x00);
        } else if (abs(_bw - 125.0) <= 0.001) {
          _mod->SPIsetRegValue(0x31, 0b0000000, 7, 7);
          _mod->SPIsetRegValue(0x2F, 0x40);
          _mod->SPIsetRegValue(0x30, 0x00);
        } else if (abs(_bw - 250.0) <= 0.001) {
          _mod->SPIsetRegValue(0x31, 0b0000000, 7, 7);
          _mod->SPIsetRegValue(0x2F, 0x40);
          _mod->SPIsetRegValue(0x30, 0x00);
        } else if (abs(_bw - 500.0) <= 0.001) {
          _mod->SPIsetRegValue(0x31, 0b1000000, 7, 7);
        }
      }

      // set frequency
      return (SX127x::setFrequencyRaw(freq));
    }

    private void SetupIO(Byte @interface, Byte gpio) {
      // select interface
      if(@interface == RadioLibTypes.RADIOLIB_USE_SPI) {
        this.PinChipSelect.PinMode = GpioPinDriveMode.Output;
        this.PinChipSelect.Write(GpioPinValue.High);
        Pi.Spi.Channel0Frequency = 250000;
      }
      // select GPIO
      if(gpio == RadioLibTypes.RADIOLIB_INT_BOTH) {
        this.PinInt0.PinMode = GpioPinDriveMode.Input;
        this.PinInt1.PinMode = GpioPinDriveMode.Input;
      }
    }
  }
}
