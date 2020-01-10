/*
 * Copyright (c) 2013, SEMTECH S.A.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * * Redistributions of source code must retain the above copyright
 *   notice, this list of conditions and the following disclaimer.
 * * Redistributions in binary form must reproduce the above copyright
 *   notice, this list of conditions and the following disclaimer in the
 *   documentation and/or other materials provided with the distribution.
 * * Neither the name of the Semtech corporation nor the
 *   names of its contributors may be used to endorse or promote products
 *   derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL SEMTECH S.A. BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

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

    private readonly Boolean[] _radioEnableTx = new Boolean[2] { true, false };
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
    #endregion

    private Thread _recieverThread;
    private Boolean _recieverThreadRunning = false;
    //private Boolean _isrecieving = false;
    private Boolean _istransmitting = false;
    private readonly Object HandleControllerIOLock = new Object();
    private Boolean _deviceStarted = false;

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
        /*if(this.config.ContainsKey("radio0tx") && this._radioEnabled[0]) {
          this._radioEnableTx[0] = Boolean.Parse(this.config["radio0tx"]);
        }
        if(this.config.ContainsKey("radio1tx") && this._radioEnabled[1]) {
          this._radioEnableTx[1] = Boolean.Parse(this.config["radio1tx"]);
        }*/
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

        this._lorawan_public = Boolean.Parse(this.config["lorawan"]);

        this.LbtParseConfig();


        //this.PinChipSelect = (GpioPin)Pi.Gpio.GetProperty(this.config["pin_sspin"]);
        //this.PinInt0 = (GpioPin)Pi.Gpio.GetProperty(this.config["pin_dio0"]);
        //this.PinReset = (GpioPin)Pi.Gpio.GetProperty(this.config["pin_rst"]);
        //this.SpiChannel = (SpiChannel)Pi.Spi.GetProperty(this.config["spichan"]);

        //this._freq = Int32.Parse(this.config["freq"]);
        //this._sf = Byte.Parse(this.config["sf"]);
        //this._bw = Int32.Parse(this.config["bw"]);
        //this._cr = Byte.Parse(this.config["cr"]);

        //this._syncWord = Byte.Parse(this.config["syncword"]);
        //this._preambleLength = UInt16.Parse(this.config["preamblelength"]);

        //this._currentLimit = Byte.Parse(this.config["currentlimit"]);
        //this._power = SByte.Parse(this.config["power"]);
        //this._gain = Byte.Parse(this.config["gain"]);
      } catch(Exception e) {
        throw new ArgumentException("Some Argument is not set in settings.ini: " + e.Message);
      }
    }
  }
}
