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
    #region Abstracts - LoraBoard
    public Dragino(Dictionary<String, String> settings) : base(settings) {
      Pi.Init<BootstrapWiringPi>();
      this.ParseConfig();
    }

    public override Boolean Begin() {
      // set module properties
      this.SetupIO(RadioLibTypes.RADIOLIB_USE_SPI, RadioLibTypes.RADIOLIB_INT_0);

      // try to find the SX127x chip
      if(!this.FindChip(Constances.SX1278_CHIP_VERSION)) {
        throw new Exception("No SX127x found!");
      } else {
        this.Debug("Found SX127x!");
      }

      // check active modem
      Int16 state;
      if(this.GetActiveModem() != Constances.SX127X_LORA) {
        // set LoRa mode
        state = this.SetActiveModem(Constances.SX127X_LORA);
        if(state != Errorcodes.ERR_NONE) {
          throw new Exception("SetActiveModem Failed: " + state);
        }
      }

      // set LoRa sync word
      state = this.SetSyncWord(this._syncWord);
      if(state != Errorcodes.ERR_NONE) {
        throw new Exception("SetSyncWord Failed: " + state);
      }

      // set over current protection
      state = this.SetCurrentLimit(this._currentLimit);
      if(state != Errorcodes.ERR_NONE) {
        throw new Exception("SetCurrentLimit Failed: " + state);
      }

      // set preamble length
      state = this.SetPreambleLength(this._preambleLength);
      if(state != Errorcodes.ERR_NONE) {
        throw new Exception("SetPreambleLength Failed: " + state);
      }

      // configure settings not accessible by API
      state = this.Config();
      if(state != Errorcodes.ERR_NONE) {
        throw new Exception("Config Failed: " + state);
      }

      // configure publicly accessible settings
      state = this.SetFrequency(this._freq);
      if(state != Errorcodes.ERR_NONE) {
        throw new Exception("SetFrequency Failed: " + state);
      }

      state = this.SetBandwidth(this._bw);
      if(state != Errorcodes.ERR_NONE) {
        throw new Exception("SetBandwidth Failed: " + state);
      }

      state = this.SetSpreadingFactor(this._sf);
      if(state != Errorcodes.ERR_NONE) {
        throw new Exception("SetSpreadingFactor Failed: " + state);
      }

      state = this.SetCodingRate(this._cr);
      if(state != Errorcodes.ERR_NONE) {
        throw new Exception("SetCodingRate Failed: " + state);
      }

      state = this.SetOutputPower(this._power);
      if (state != Errorcodes.ERR_NONE) {
        throw new Exception("SetOutputPower Failed: " + state);
      }

      state = this.SetGain(this._gain);
      if (state != Errorcodes.ERR_NONE) {
        throw new Exception("SetOutputPower Failed: " + state);
      }
      return true;
    }

    public override void Dispose() => throw new NotImplementedException();
    public override Boolean Send(Byte[] data, Byte @interface) {
      this._istransmitting = true;
      Int16 state = this.Transmit(data);
      DragionoSendedObj d = new DragionoSendedObj {
        Data = data
      };
      if(state == Errorcodes.ERR_NONE) {
        // the packet was successfully transmitted
        this.Debug("[SX1278] Transmitting packet ... success!");

        // print measured data rate
        d.Datarate = this.GetDataRate();
        this.Debug("[SX1278] Datarate:\t"+d.Datarate+" bps");

      } else if(state == Errorcodes.ERR_PACKET_TOO_LONG) {
        // the supplied packet was longer than 256 bytes
        this.Debug("[SX1278] Transmitting packet ... too long!");
        d.Tolong = true;
      } else if(state == Errorcodes.ERR_TX_TIMEOUT) {
        // timeout occured while transmitting packet
        this.Debug("[SX1278] Transmitting packet ... timeout!");
        d.Txtimeout = true;
      } else {
        // some other error occurred
        this.Debug("[SX1278] Transmitting packet ... failed, code "+ state);
        d.Errorcode = state;
      }
      this.RaiseSendedEvent(d);
      this._istransmitting = false;
      if(this._isrecieving) {
        state = this.StartReceive(0, Constances.SX127X_RXCONTINUOUS);
      }
      return state == Errorcodes.ERR_NONE;
    }

    public override Boolean StartEventRecieving() {
      this.PinInt0.RegisterInterruptCallback(EdgeDetection.RisingEdge, this.HandleRecievedData);
      Int16 state = this.StartReceive(0, Constances.SX127X_RXCONTINUOUS);
      if(state != Errorcodes.ERR_NONE) {
        throw new Exception("StartReceive Failed: " + state);
      }
      this._isrecieving = true;
      return true;
    }
    #endregion

    #region Private Methodes
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
      if(this.GetActiveModem() != Constances.SX127X_LORA) {
        return Errorcodes.ERR_WRONG_MODEM;
      }

      // set mode to standby
      _ = this.SetMode(Constances.SX127X_STANDBY);

      // write register
      return this.SPIsetRegValue(RegisterAdresses.SX127X_REG_SYNC_WORD, syncWord);
    }

    private Int16 SetCurrentLimit(Byte currentLimit) {
      // check allowed range
      if(!(currentLimit >= 45 && currentLimit <= 240 || currentLimit == 0)) {
        return Errorcodes.ERR_INVALID_CURRENT_LIMIT;
      }

      // set mode to standby
      Int16 state = this.SetMode(Constances.SX127X_STANDBY);

      // set OCP limit
      Byte raw;
      if(currentLimit == 0) {
        // limit set to 0, disable OCP
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_OCP, Constances.SX127X_OCP_OFF, 5, 5);
      } else if(currentLimit <= 120) {
        raw = (Byte)(Constances.SX127X_OCP_ON | ((currentLimit - 45) / 5));
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_OCP, raw, 5, 0);
      } else if(currentLimit <= 240) {
        raw = (Byte)(Constances.SX127X_OCP_ON | ((currentLimit + 30) / 10));
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_OCP, raw, 5, 0);
      }
      return state;
    }

    private Int16 SetPreambleLength(UInt16 preambleLength) {
      // set mode to standby
      Int16 state = this.SetMode(Constances.SX127X_STANDBY);
      if(state != Errorcodes.ERR_NONE) {
        return state;
      }

      // check active modem
      Int16 modem = this.GetActiveModem();
      if(modem == Constances.SX127X_LORA) {
        // check allowed range
        if(preambleLength < 6) {
          return Errorcodes.ERR_INVALID_PREAMBLE_LENGTH;
        }

        // set preamble length
        state = this.SPIsetRegValue(RegisterAdresses.SX127X_REG_PREAMBLE_MSB, (Byte)((preambleLength >> 8) & 0xFF));
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_PREAMBLE_LSB, (Byte)(preambleLength & 0xFF));
        return state;

      } else if(modem == Constances.SX127X_FSK_OOK) {
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
      if(freq < 137000000 || freq > 1020000000) {
        return Errorcodes.ERR_INVALID_FREQUENCY;
      }

      // SX1276/77/78 Errata fixes
      if(this.GetActiveModem() == Constances.SX127X_LORA) {
        // sensitivity optimization for 500kHz bandwidth
        // see SX1276/77/78 Errata, section 2.1 for details
        if(this._bw == Bandwidths.Freq_500k) {
          if(freq >= 862000000 && freq <= 1020000000) {
            this.SPIwriteRegister(0x36, 0x02);
            this.SPIwriteRegister(0x3a, 0x64);
          } else if(freq >= 410000000 && freq <= 525000000) {
            this.SPIwriteRegister(0x36, 0x02);
            this.SPIwriteRegister(0x3a, 0x7F);
          }
        }

        // mitigation of receiver spurious response
        // see SX1276/77/78 Errata, section 2.3 for details
        if(this._bw == Bandwidths.Freq_7k8) {
          _ = this.SPIsetRegValue(0x31, 0b0000000, 7, 7);
          _ = this.SPIsetRegValue(0x2F, 0x48);
          _ = this.SPIsetRegValue(0x30, 0x00);
          freq += Bandwidths.Freq_7k8;
        } else if(this._bw == Bandwidths.Freq_10k4) {
          _ = this.SPIsetRegValue(0x31, 0b0000000, 7, 7);
          _ = this.SPIsetRegValue(0x2F, 0x44);
          _ = this.SPIsetRegValue(0x30, 0x00);
          freq += Bandwidths.Freq_10k4;
        } else if(this._bw == Bandwidths.Freq_15k6) {
          _ = this.SPIsetRegValue(0x31, 0b0000000, 7, 7);
          _ = this.SPIsetRegValue(0x2F, 0x44);
          _ = this.SPIsetRegValue(0x30, 0x00);
          freq += Bandwidths.Freq_15k6;
        } else if(this._bw == Bandwidths.Freq_20k8) {
          _ = this.SPIsetRegValue(0x31, 0b0000000, 7, 7);
          _ = this.SPIsetRegValue(0x2F, 0x44);
          _ = this.SPIsetRegValue(0x30, 0x00);
          freq += Bandwidths.Freq_20k8;
        } else if(this._bw == Bandwidths.Freq_31k25) {
          _ = this.SPIsetRegValue(0x31, 0b0000000, 7, 7);
          _ = this.SPIsetRegValue(0x2F, 0x44);
          _ = this.SPIsetRegValue(0x30, 0x00);
          freq += Bandwidths.Freq_31k25;
        } else if(this._bw == Bandwidths.Freq_41k7) {
          _ = this.SPIsetRegValue(0x31, 0b0000000, 7, 7);
          _ = this.SPIsetRegValue(0x2F, 0x44);
          _ = this.SPIsetRegValue(0x30, 0x00);
          freq += Bandwidths.Freq_41k7;
        } else if(this._bw == Bandwidths.Freq_62k5) {
          _ = this.SPIsetRegValue(0x31, 0b0000000, 7, 7);
          _ = this.SPIsetRegValue(0x2F, 0x40);
          _ = this.SPIsetRegValue(0x30, 0x00);
        } else if(this._bw == Bandwidths.Freq_125k) {
          _ = this.SPIsetRegValue(0x31, 0b0000000, 7, 7);
          _ = this.SPIsetRegValue(0x2F, 0x40);
          _ = this.SPIsetRegValue(0x30, 0x00);
        } else if(this._bw == Bandwidths.Freq_250k) {
          _ = this.SPIsetRegValue(0x31, 0b0000000, 7, 7);
          _ = this.SPIsetRegValue(0x2F, 0x40);
          _ = this.SPIsetRegValue(0x30, 0x00);
        } else if(this._bw == Bandwidths.Freq_500k) {
          _ = this.SPIsetRegValue(0x31, 0b1000000, 7, 7);
        }
      }

      // set frequency
      return this.SetFrequencyRaw(freq);
    }

    private Int16 SetFrequencyRaw(Int32 newFreq) {
      // set mode to standby
      Int16 state = this.SetMode(Constances.SX127X_STANDBY);

      // calculate register values
      UInt32 FRF = (UInt32)(newFreq * (1 << Constances.SX127X_DIV_EXPONENT) / Constances.SX127X_CRYSTAL_FREQ);

      // write registers
      state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_FRF_MSB, (Byte)((FRF & 0xFF0000) >> 16));
      state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_FRF_MID, (Byte)((FRF & 0x00FF00) >> 8));
      state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_FRF_LSB, (Byte)(FRF & 0x0000FF));
      return state;
    }

    private Int16 SetBandwidth(Int32 bw) {
      // check active modem
      if(this.GetActiveModem() != Constances.SX127X_LORA) {
        return Errorcodes.ERR_WRONG_MODEM;
      }

      Byte newBandwidth;

      // check allowed bandwidth values
      if(bw == Bandwidths.Freq_7k8) {
        newBandwidth = Bandwidths.SX1278_BW_7_80_KHZ;
      } else if(bw == Bandwidths.Freq_10k4) {
        newBandwidth = Bandwidths.SX1278_BW_10_40_KHZ;
      } else if(bw == Bandwidths.Freq_15k6) {
        newBandwidth = Bandwidths.SX1278_BW_15_60_KHZ;
      } else if(bw == Bandwidths.Freq_20k8) {
        newBandwidth = Bandwidths.SX1278_BW_20_80_KHZ;
      } else if(bw == Bandwidths.Freq_31k25) {
        newBandwidth = Bandwidths.SX1278_BW_31_25_KHZ;
      } else if(bw == Bandwidths.Freq_41k7) {
        newBandwidth = Bandwidths.SX1278_BW_41_70_KHZ;
      } else if(bw == Bandwidths.Freq_62k5) {
        newBandwidth = Bandwidths.SX1278_BW_62_50_KHZ;
      } else if(bw == Bandwidths.Freq_125k) {
        newBandwidth = Bandwidths.SX1278_BW_125_00_KHZ;
      } else if(bw == Bandwidths.Freq_250k) {
        newBandwidth = Bandwidths.SX1278_BW_250_00_KHZ;
      } else if(bw == Bandwidths.Freq_500k) {
        newBandwidth = Bandwidths.SX1278_BW_500_00_KHZ;
      } else {
        return Errorcodes.ERR_INVALID_BANDWIDTH;
      }

      // set bandwidth and if successful, save the new setting
      Int16 state = this.SetBandwidthRaw(newBandwidth);
      if(state == Errorcodes.ERR_NONE) {
        this._bw = bw;

        // calculate symbol length and set low data rate optimization, if needed
        Single symbolLength = (Single)(1 << this._sf) / this._bw;
        this.Debug("Symbol length: " + symbolLength + " ms");
        state = symbolLength >= 16.0 ? this.SPIsetRegValue(RegisterAdresses.SX1278_REG_MODEM_CONFIG_3, Constances.SX1278_LOW_DATA_RATE_OPT_ON, 3, 3) : this.SPIsetRegValue(RegisterAdresses.SX1278_REG_MODEM_CONFIG_3, Constances.SX1278_LOW_DATA_RATE_OPT_OFF, 3, 3);
      }
      return state;
    }

    private Int16 SetBandwidthRaw(Byte newBandwidth) {
      // set mode to standby
      Int16 state = this.SetMode(Constances.SX127X_STANDBY);

      // write register
      state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_MODEM_CONFIG_1, newBandwidth, 7, 4);
      return state;
    }

    private Int16 SetSpreadingFactor(Byte sf) {
      // check active modem
      if(this.GetActiveModem() != Constances.SX127X_LORA) {
        return Errorcodes.ERR_WRONG_MODEM;
      }

      Byte newSpreadingFactor;

      // check allowed spreading factor values
      if(sf == SpreadingFactors.SF6) {
        newSpreadingFactor = SpreadingFactors.SX127X_SF_6;
      } else if(sf == SpreadingFactors.SF7) {
        newSpreadingFactor = SpreadingFactors.SX127X_SF_7;
      } else if(sf == SpreadingFactors.SF8) {
        newSpreadingFactor = SpreadingFactors.SX127X_SF_8;
      } else if(sf == SpreadingFactors.SF9) {
        newSpreadingFactor = SpreadingFactors.SX127X_SF_9;
      } else if(sf == SpreadingFactors.SF10) {
        newSpreadingFactor = SpreadingFactors.SX127X_SF_10;
      } else if(sf == SpreadingFactors.SF11) {
        newSpreadingFactor = SpreadingFactors.SX127X_SF_11;
      } else if(sf == SpreadingFactors.SF12) {
        newSpreadingFactor = SpreadingFactors.SX127X_SF_12;
      } else {
        return Errorcodes.ERR_INVALID_SPREADING_FACTOR;
      }

      // set spreading factor and if successful, save the new setting
      Int16 state = this.SetSpreadingFactorRaw(newSpreadingFactor);
      if(state == Errorcodes.ERR_NONE) {
        this._sf = sf;

        // calculate symbol length and set low data rate optimization, if needed
        Single symbolLength = (Single)(1 << this._sf) / this._bw;
        this.Debug("Symbol length: " + symbolLength + " ms");
        state = symbolLength >= 16.0 ? this.SPIsetRegValue(RegisterAdresses.SX1278_REG_MODEM_CONFIG_3, Constances.SX1278_LOW_DATA_RATE_OPT_ON, 3, 3) : this.SPIsetRegValue(RegisterAdresses.SX1278_REG_MODEM_CONFIG_3, Constances.SX1278_LOW_DATA_RATE_OPT_OFF, 3, 3);
      }
      return state;
    }

    private Int16 SetSpreadingFactorRaw(Byte newSpreadingFactor) {
      // set mode to standby
      Int16 state = this.SetMode(Constances.SX127X_STANDBY);

      // write registers
      if(newSpreadingFactor == SpreadingFactors.SX127X_SF_6) {
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_MODEM_CONFIG_1, Constances.SX1278_HEADER_IMPL_MODE, 0, 0);
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_MODEM_CONFIG_2, (Byte)(SpreadingFactors.SX127X_SF_6 | Constances.SX127X_TX_MODE_SINGLE | Constances.SX1278_RX_CRC_MODE_ON), 7, 2);
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_DETECT_OPTIMIZE, Constances.SX127X_DETECT_OPTIMIZE_SF_6, 2, 0);
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_DETECTION_THRESHOLD, Constances.SX127X_DETECTION_THRESHOLD_SF_6);
      } else {
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_MODEM_CONFIG_1, Constances.SX1278_HEADER_EXPL_MODE, 0, 0);
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_MODEM_CONFIG_2, (Byte)(newSpreadingFactor | Constances.SX127X_TX_MODE_SINGLE | Constances.SX1278_RX_CRC_MODE_ON), 7, 2);
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_DETECT_OPTIMIZE, Constances.SX127X_DETECT_OPTIMIZE_SF_7_12, 2, 0);
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_DETECTION_THRESHOLD, Constances.SX127X_DETECTION_THRESHOLD_SF_7_12);
      }
      return state;
    }

    private Int16 SetCodingRate(Byte cr) {
      // check active modem
      if(this.GetActiveModem() != Constances.SX127X_LORA) {
        return Errorcodes.ERR_WRONG_MODEM;
      }

      Byte newCodingRate;

      // check allowed coding rate values
      if(cr == Codingrates.CR5) {
        newCodingRate = Codingrates.SX1278_CR_4_5;
      } else if(cr == Codingrates.CR6) {
        newCodingRate = Codingrates.SX1278_CR_4_6;
      } else if(cr == Codingrates.CR7) {
        newCodingRate = Codingrates.SX1278_CR_4_7;
      } else if(cr == Codingrates.CR8) {
        newCodingRate = Codingrates.SX1278_CR_4_8;
      } else {
        return Errorcodes.ERR_INVALID_CODING_RATE;
      }


      // set coding rate and if successful, save the new setting
      Int16 state = this.SetCodingRateRaw(newCodingRate);
      if(state == Errorcodes.ERR_NONE) {
        this._cr = cr;
      }
      return state;
    }

    private Int16 SetCodingRateRaw(Byte newCodingRate) {
      // set mode to standby
      Int16 state = this.SetMode(Constances.SX127X_STANDBY);

      // write register
      state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_MODEM_CONFIG_1, newCodingRate, 3, 1);
      return state;
    }

    private Int16 SetOutputPower(SByte power) {
      // check allowed power range
      if(!(power >= -3 && power <= 17 || power == 20)) {
        return Errorcodes.ERR_INVALID_OUTPUT_POWER;
      }

      // set mode to standby
      Int16 state = this.SetMode(Constances.SX127X_STANDBY);

      // set output power
      if(power < 2) {
        // power is less than 2 dBm, enable PA on RFO
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_PA_CONFIG, Constances.SX127X_PA_SELECT_RFO, 7, 7);
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_PA_CONFIG, (Byte)(Constances.SX1278_LOW_POWER | (power + 3)), 6, 0);
        state |= this.SPIsetRegValue(RegisterAdresses.SX1278_REG_PA_DAC, Constances.SX127X_PA_BOOST_OFF, 2, 0);
      } else if(power >= 2 && power <= 17) {
        // power is 2 - 17 dBm, enable PA1 + PA2 on PA_BOOST
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_PA_CONFIG, Constances.SX127X_PA_SELECT_BOOST, 7, 7);
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_PA_CONFIG, (Byte)(Constances.SX1278_MAX_POWER | (power - 2)), 6, 0);
        state |= this.SPIsetRegValue(RegisterAdresses.SX1278_REG_PA_DAC, Constances.SX127X_PA_BOOST_OFF, 2, 0);
      } else if(power == 20) {
        // power is 20 dBm, enable PA1 + PA2 on PA_BOOST and enable high power mode
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_PA_CONFIG, Constances.SX127X_PA_SELECT_BOOST, 7, 7);
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_PA_CONFIG, (Byte)(Constances.SX1278_MAX_POWER | (power - 5)), 6, 0);
        state |= this.SPIsetRegValue(RegisterAdresses.SX1278_REG_PA_DAC, Constances.SX127X_PA_BOOST_ON, 2, 0);
      }
      return state;
    }

    private Int16 SetGain(Byte gain) {
      // check active modem
      if(this.GetActiveModem() != Constances.SX127X_LORA) {
        return Errorcodes.ERR_WRONG_MODEM;
      }

      // check allowed range
      if(gain > 6) {
        return Errorcodes.ERR_INVALID_GAIN;
      }

      // set mode to standby
      Int16 state = this.SetMode(Constances.SX127X_STANDBY);

      // set gain
      if(gain == 0) {
        // gain set to 0, enable AGC loop
        state |= this.SPIsetRegValue(RegisterAdresses.SX1278_REG_MODEM_CONFIG_3, Constances.SX1278_AGC_AUTO_ON, 2, 2);
      } else {
        state |= this.SPIsetRegValue(RegisterAdresses.SX1278_REG_MODEM_CONFIG_3, Constances.SX1278_AGC_AUTO_OFF, 2, 2);
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_LNA, (Byte)((gain << 5) | Constances.SX127X_LNA_BOOST_ON));
      }
      return state;
    }
    #endregion

    #region Recieve
    private Int16 StartReceive(Byte len, Byte mode) {
      // set mode to standby
      Int16 state = this.SetMode(Constances.SX127X_STANDBY);

      Int16 modem = this.GetActiveModem();
      if(modem == Constances.SX127X_LORA) {
        // set DIO pin mapping
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_DIO_MAPPING_1, (Byte)(Constances.SX127X_DIO0_RX_DONE | Constances.SX127X_DIO1_RX_TIMEOUT), 7, 4);

        // set expected packet length for SF6
        if(this._sf == SpreadingFactors.SF6) {
          state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_PAYLOAD_LENGTH, len);
        }

        // clear interrupt flags
        this.ClearIRQFlags();

        // set FIFO pointers
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_FIFO_RX_BASE_ADDR, Constances.SX127X_FIFO_RX_BASE_ADDR_MAX);
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_FIFO_ADDR_PTR, Constances.SX127X_FIFO_RX_BASE_ADDR_MAX);
        if(state != Errorcodes.ERR_NONE) {
          return state;
        }

      } else if(modem == Constances.SX127X_FSK_OOK) {
        // set DIO pin mapping
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_DIO_MAPPING_1, Constances.SX127X_DIO0_PACK_PAYLOAD_READY, 7, 6);
        if(state != Errorcodes.ERR_NONE) {
          return state;
        }
        // clear interrupt flags
        this.ClearIRQFlags();

        // FSK modem does not distinguish between Rx single and continuous
        if(mode == Constances.SX127X_RXCONTINUOUS) {
          return this.SetMode(Constances.SX127X_RX);
        }
      }

      // set mode to receive
      return this.SetMode(mode);
    }

    private void ClearIRQFlags() {
      Int16 modem = this.GetActiveModem();
      if(modem == Constances.SX127X_LORA) {
        this.SPIwriteRegister(RegisterAdresses.SX127X_REG_IRQ_FLAGS, 0b11111111);
      } else if(modem == Constances.SX127X_FSK_OOK) {
        this.SPIwriteRegister(RegisterAdresses.SX127X_REG_IRQ_FLAGS_1, 0b11111111);
        this.SPIwriteRegister(RegisterAdresses.SX127X_REG_IRQ_FLAGS_2, 0b11111111);
      }
    }

    private void ClearFIFO(Byte count) {
      for(Byte i = 0; i < count; i++) {
        _ = this.SPIreadRegister(RegisterAdresses.SX127X_REG_FIFO);
      }
    }

    private void HandleRecievedData() {
      if(this._istransmitting) {
        return;
      }
      lock(this.HandleRecievedDataLock) {
        // you can read received data as an Arduino String
        Int16 state = this.ReadData(out Byte[] data);
        DragionoRecievedObj d = new DragionoRecievedObj();
        if(state == Errorcodes.ERR_NONE) {
          // packet was successfully received
          this.Debug("[SX1278] Received packet!");

          // print data of the packet
          d.Data = data;
          this.Debug("[SX1278] Data:\t\t"+ BitConverter.ToString(d.Data).Replace("-", " "));


          // print RSSI (Received Signal Strength Indicator)
          d.Rssi = this.GetRSSI();
          this.Debug("[SX1278] RSSI:\t\t" + d.Rssi + " dBm");

          // print SNR (Signal-to-Noise Ratio)
          d.Snr = this.GetSNR();
          this.Debug("[SX1278] SNR:\t\t"+d.Snr+ " dB");

          // print frequency error
          d.FreqError = this.GetFrequencyError();
          this.Debug("[SX1278] Frequency error:\t"+ d.FreqError+ " Hz");

          this.RaiseRecieveEvent(d);

        } else if(state == Errorcodes.ERR_CRC_MISMATCH) {
          // packet was received, but is malformed
          this.Debug("[SX1278] CRC error!");
          d.Data = data;
          this.Debug("[SX1278] Data:\t\t" + BitConverter.ToString(d.Data).Replace("-", " "));
          d.Crc = false;
          this.RaiseRecieveEvent(d);
        } else {
          // some other error occurred
          this.Debug("[SX1278] Failed, code " + state);
        }

        // put module back to listen mode
        _ = this.StartReceive(0, Constances.SX127X_RXCONTINUOUS);
      }
    }

    private Double GetFrequencyError(Boolean autoCorrect = false) {
      Int16 modem = this.GetActiveModem();
      if(modem == Constances.SX127X_LORA) {
        // get raw frequency error
        UInt32 raw = (UInt32)this.SPIgetRegValue(RegisterAdresses.SX127X_REG_FEI_MSB, 3, 0) << 16;
        raw |= (UInt32)this.SPIgetRegValue(RegisterAdresses.SX127X_REG_FEI_MID) << 8;
        raw |= (Byte)this.SPIgetRegValue(RegisterAdresses.SX127X_REG_FEI_LSB);

        UInt32 @base = (UInt32)2 << 23;
        Double error;

        // check the first bit
        if((raw & 0x80000) == 0) {
          // frequency error is negative
          raw |= 0xFFF00000;
          raw = ~raw + 1;
          error = raw * (Double)@base / 32000000.0 * (this._bw / 500.0) * -1.0;
        } else {
          error = raw * (Double)@base / 32000000.0 * (this._bw / 500.0);
        }

        if(autoCorrect) {
          // adjust LoRa modem data rate
          Double ppmOffset = 0.95 * (error / 32.0);
          this.SPIwriteRegister(0x27, (Byte)ppmOffset);
        }

        return error;

      } else if(modem == Constances.SX127X_FSK_OOK) {
        // get raw frequency error
        UInt16 raw = (UInt16)((UInt32)this.SPIgetRegValue(RegisterAdresses.SX127X_REG_FEI_MSB_FSK) << 8);
        raw |= (Byte)this.SPIgetRegValue(RegisterAdresses.SX127X_REG_FEI_LSB_FSK);

        UInt32 @base = 1;
        Double error;

        // check the first bit
        if((raw & 0x8000) == 0) {
          // frequency error is negative
          raw |= 0xFFF0;
          raw = (UInt16)((UInt16)~raw + 1);
          error = raw * (32000000.0 / (Double)(@base << 19)) * -1.0;
        } else {
          error = raw * (32000000.0 / (Double)(@base << 19));
        }

        return error;
      }

      return Errorcodes.ERR_UNKNOWN;
    }

    private Double GetSNR() {
      // check active modem
      if(this.GetActiveModem() != Constances.SX127X_LORA) {
        return Errorcodes.ERR_WRONG_MODEM;
      }

      // get SNR value
      SByte rawSNR = (SByte)this.SPIgetRegValue(RegisterAdresses.SX127X_REG_PKT_SNR_VALUE);
      return rawSNR / 4.0;
    }

    private Double GetRSSI() {
      if(this.GetActiveModem() == Constances.SX127X_LORA) {
        // for LoRa, get RSSI of the last packet
        Double lastPacketRSSI = this._freq < 868000000 ? -164 + this.SPIgetRegValue(RegisterAdresses.SX127X_REG_PKT_RSSI_VALUE) : -157 + this.SPIgetRegValue(RegisterAdresses.SX127X_REG_PKT_RSSI_VALUE);

        // RSSI calculation uses different constant for low-frequency and high-frequency ports

        // spread-spectrum modulation signal can be received below noise floor
        // check last packet SNR and if it's less than 0, add it to reported RSSI to get the correct value
        Double lastPacketSNR = this.GetSNR();
        if(lastPacketSNR < 0.0) {
          lastPacketRSSI += lastPacketSNR;
        }

        return lastPacketRSSI;

      } else {
        // enable listen mode
        _ = this.StartReceive(0, Constances.SX127X_RXCONTINUOUS);

        // read the value for FSK
        Double rssi = this.SPIgetRegValue(RegisterAdresses.SX127X_REG_RSSI_VALUE_FSK) / -2.0;

        // set mode back to standby
        _ = this.SetMode(Constances.SX127X_STANDBY);

        // return the value
        return rssi;
      }
    }

    private Int16 ReadData(out Byte[] data) {
      Int16 modem = this.GetActiveModem();
      Byte length = 0;

      // put module to standby
      Int16 state = this.SetMode(Constances.SX127X_STANDBY);

      if(modem == Constances.SX127X_LORA) {
        //read the number of actually received bytes
        length = this.GetPacketLength();

        // check integrity CRC
        if(this.SPIgetRegValue(RegisterAdresses.SX127X_REG_IRQ_FLAGS, 5, 5) == Constances.SX127X_CLEAR_IRQ_FLAG_PAYLOAD_CRC_ERROR) {
          state = Errorcodes.ERR_CRC_MISMATCH;
        }

      } else if(modem == Constances.SX127X_FSK_OOK) {
        // read packet length (always required in FSK)
        length = this.GetPacketLength();

        // check address filtering
        Int16 filter = this.SPIgetRegValue(RegisterAdresses.SX127X_REG_PACKET_CONFIG_1, 2, 1);
        if(filter == Constances.SX127X_ADDRESS_FILTERING_NODE || filter == Constances.SX127X_ADDRESS_FILTERING_NODE_BROADCAST) {
          _ = this.SPIreadRegister(RegisterAdresses.SX127X_REG_FIFO);
        }
      }

      // read packet data
      data = this.SPIreadRegisterBurst(RegisterAdresses.SX127X_REG_FIFO, length);

      // dump bytes that weren't requested
      Byte packetLength = this.GetPacketLength();
      if(packetLength > length) {
        this.ClearFIFO((Byte)(packetLength - length));
      }

      // clear internal flag so getPacketLength can return the new packet length
      this._packetLengthQueried = false;

      // clear interrupt flags
      this.ClearIRQFlags();

      return state;
    }

    private Byte GetPacketLength(Boolean update = true) {
      Int16 modem = this.GetActiveModem();

      if(modem == Constances.SX127X_LORA) {
        if(this._sf != SpreadingFactors.SF6) {
          // get packet length for SF7 - SF12
          return this.SPIreadRegister(RegisterAdresses.SX127X_REG_RX_NB_BYTES);

        } else {
          // return the maximum value for SF6
          return Constances.SX127X_MAX_PACKET_LENGTH;
        }

      } else if(modem == Constances.SX127X_FSK_OOK) {
        // get packet length
        if(!this._packetLengthQueried && update) {
          this._packetLength = this.SPIreadRegister(RegisterAdresses.SX127X_REG_FIFO);
          this._packetLengthQueried = true;
        }
      }

      return this._packetLength;
    }
    #endregion

    #region Transmitting
    private Int16 Transmit(Byte[] data, Byte addr = 0) {
      // set mode to standby
      _ = this.SetMode(Constances.SX127X_STANDBY);
      Int16 state;

      Int16 modem = this.GetActiveModem();
      DateTime start;
      if(modem == Constances.SX127X_LORA) {
        // calculate timeout (150 % of expected time-one-air)
        Double symbolLength = (1 << this._sf) / (Single)this._bw;
        Double de = 0;
        if(symbolLength >= 16.0) {
          de = 1;
        }
        Double ih = this.SPIgetRegValue(RegisterAdresses.SX127X_REG_MODEM_CONFIG_1, 0, 0);
        Double crc = this.SPIgetRegValue(RegisterAdresses.SX127X_REG_MODEM_CONFIG_2, 2, 2) >> 2;
        Double n_pre = ((UInt16)this.SPIgetRegValue(RegisterAdresses.SX127X_REG_PREAMBLE_MSB) << 8) | (Byte)this.SPIgetRegValue(RegisterAdresses.SX127X_REG_PREAMBLE_LSB);
        Double n_pay = 8.0 + Math.Max(Math.Ceiling((8.0 * data.Length - 4.0 * this._sf + 28.0 + 16.0 * crc - 20.0 * ih) / (4.0 * this._sf - 8.0 * de)) * this._cr, 0.0);
        UInt32 timeout = (UInt32)Math.Ceiling(symbolLength * (n_pre + n_pay + 4.25) * 1500.0);

        // start transmission
        state = this.StartTransmit(data, addr);
        if(state != Errorcodes.ERR_NONE) {
          return state;
        }

        // wait for packet transmission or timeout
        start = DateTime.Now;
        TimeSpan ms = new TimeSpan(timeout * 10000);
        while(!this.PinInt0.Value) {
          if(DateTime.Now - start > ms) {
            this.ClearIRQFlags();
            return Errorcodes.ERR_TX_TIMEOUT;
          }
          Thread.Sleep(1);
        }

      } else if(modem == Constances.SX127X_FSK_OOK) {
        // calculate timeout (5ms + 500 % of expected time-on-air)
        UInt32 timeout = 5000000 + (UInt32)(data.Length * 8 / (this._br * 1000.0) * 5000000.0);

        // start transmission
        state = this.StartTransmit(data, addr);
        if(state != Errorcodes.ERR_NONE) {
          return state;
        }

        // wait for transmission end or timeout
        start = DateTime.Now;
        TimeSpan ms = new TimeSpan(timeout * 10000);
        while(!this.PinInt0.Value) {
          if(DateTime.Now - start > ms) {
            this.ClearIRQFlags();
            _ = this.SetMode(Constances.SX127X_STANDBY);
            return Errorcodes.ERR_TX_TIMEOUT;
          }
        }
      } else {
        return Errorcodes.ERR_UNKNOWN;
      }

      // update data rate
      UInt32 elapsed = (UInt32)((DateTime.Now - start).Ticks / 10000);
      this._dataRate = data.Length * 8.0 / (elapsed / 1000000.0);

      // clear interrupt flags
      this.ClearIRQFlags();

      // set mode to standby to disable transmitter
      return this.SetMode(Constances.SX127X_STANDBY);
    }

    private Double GetDataRate() => this._dataRate;

    private Int16 StartTransmit(Byte[] data, Byte addr = 0) {
      // set mode to standby
      Int16 state = this.SetMode(Constances.SX127X_STANDBY);

      Int16 modem = this.GetActiveModem();
      if(modem == Constances.SX127X_LORA) {
        // check packet length
        if(data.Length >= Constances.SX127X_MAX_PACKET_LENGTH) {
          return Errorcodes.ERR_PACKET_TOO_LONG;
        }

        // set DIO mapping
        _ = this.SPIsetRegValue(RegisterAdresses.SX127X_REG_DIO_MAPPING_1, Constances.SX127X_DIO0_TX_DONE, 7, 6);

        // clear interrupt flags
        this.ClearIRQFlags();

        // set packet length
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_PAYLOAD_LENGTH, (Byte)data.Length);

        // set FIFO pointers
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_FIFO_TX_BASE_ADDR, Constances.SX127X_FIFO_TX_BASE_ADDR_MAX);
        state |= this.SPIsetRegValue(RegisterAdresses.SX127X_REG_FIFO_ADDR_PTR, Constances.SX127X_FIFO_TX_BASE_ADDR_MAX);

        // write packet to FIFO
        this.SPIwriteRegisterBurst(RegisterAdresses.SX127X_REG_FIFO, data);

        // start transmission
        state |= this.SetMode(Constances.SX127X_TX);
        return state != Errorcodes.ERR_NONE ? state : Errorcodes.ERR_NONE;

      } else if(modem == Constances.SX127X_FSK_OOK) {
        // check packet length
        if(data.Length >= Constances.SX127X_MAX_PACKET_LENGTH_FSK) {
          return Errorcodes.ERR_PACKET_TOO_LONG;
        }

        // set DIO mapping
        _ = this.SPIsetRegValue(RegisterAdresses.SX127X_REG_DIO_MAPPING_1, Constances.SX127X_DIO0_PACK_PACKET_SENT, 7, 6);

        // clear interrupt flags
        this.ClearIRQFlags();

        // set packet length
        this.SPIwriteRegister(RegisterAdresses.SX127X_REG_FIFO, (Byte)data.Length);

        // check address filtering
        Int16 filter = this.SPIgetRegValue(RegisterAdresses.SX127X_REG_PACKET_CONFIG_1, 2, 1);
        if(filter == Constances.SX127X_ADDRESS_FILTERING_NODE || filter == Constances.SX127X_ADDRESS_FILTERING_NODE_BROADCAST) {
          this.SPIwriteRegister(RegisterAdresses.SX127X_REG_FIFO, addr);
        }

        // write packet to FIFO
        this.SPIwriteRegisterBurst(RegisterAdresses.SX127X_REG_FIFO, data);

        // start transmission
        state |= this.SetMode(Constances.SX127X_TX);
        return state != Errorcodes.ERR_NONE ? state : Errorcodes.ERR_NONE;
      }

      return Errorcodes.ERR_UNKNOWN;
    }
    #endregion

    private void SetupIO(Byte @interface, Byte gpio) {
      // select interface
      if(@interface == RadioLibTypes.RADIOLIB_USE_SPI) {
        this.PinChipSelect.PinMode = GpioPinDriveMode.Output;
        this.PinChipSelect.Write(GpioPinValue.High);
        Pi.Spi.Channel0Frequency = 250000;
      }
      // select GPIO
      if(gpio == RadioLibTypes.RADIOLIB_INT_0) {
        this.PinInt0.PinMode = GpioPinDriveMode.Input;
      }
    }
  }
}
