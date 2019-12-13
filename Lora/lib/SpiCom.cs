﻿using System;

using Unosquare.RaspberryIO.Abstractions;
using Unosquare.WiringPi;

namespace Fraunhofer.Fit.Iot.Lora.lib {

  public abstract class SpiCom {
    protected GpioPin PinChipSelect;
    protected SpiChannel SpiChannel;


    private struct SPICommands {
      public static Byte Read => 0b00000000;
      public static Byte Write => 0b10000000;
    }

    protected struct Errorcodes {
      public static Int16 ERR_WRONG_MODEM => -20;
      public static Int16 ERR_INVALID_PREAMBLE_LENGTH => -18;
      public static Int16 ERR_INVALID_CURRENT_LIMIT => -17;
      public static Int16 ERR_SPI_WRITE_FAILED => -16;
      public static Int16 ERR_INVALID_FREQUENCY => -12;
      public static Int16 ERR_INVALID_BIT_RANGE => -11;
      public static Int16 ERR_UNKNOWN => -1;
      public static Int16 ERR_NONE => 0;
    }

    protected Byte SPIreadRegister(Byte address) => this.MultiSPI(SPICommands.Read, address, null, 1)[0];

    protected void SPIwriteRegister(Byte address, Byte data) => this.MultiSPI(SPICommands.Write, address, new Byte[] { data }, 1);

    protected Int16 SPIgetRegValue(Byte address, Byte msb = 7, Byte lsb = 0) {
      if(msb > 7 || lsb > 7 || lsb > msb) {
        return Errorcodes.ERR_INVALID_BIT_RANGE;
      }

      Byte rawValue = this.SPIreadRegister(address);
      return (Byte)(rawValue & ((0b11111111 << lsb) & (0b11111111 >> (7 - msb))));
    }

    protected Int16 SPIsetRegValue(Byte address, Byte value, Byte msb = 7, Byte lsb = 0, Byte checkInterval = 2) {
      if(msb > 7 || lsb > 7 || lsb > msb) {
        return Errorcodes.ERR_INVALID_BIT_RANGE;
      }

      Byte currentValue = this.SPIreadRegister(address);
      Byte mask = (Byte)~((0b11111111 << (msb + 1)) | (0b11111111 >> (8 - lsb)));
      Byte newValue = (Byte)((currentValue & ~mask) | (value & mask));
      this.SPIwriteRegister(address, newValue);

      // check register value each millisecond until check interval is reached
      // some registers need a bit of time to process the change (e.g. SX127X_REG_OP_MODE)
      DateTime start = DateTime.Now;
      TimeSpan ms = new TimeSpan(checkInterval * 10000);
      while(DateTime.Now - start < ms) {
        if(this.SPIreadRegister(address) == newValue) {
          // check passed, we can stop the loop
          return Errorcodes.ERR_NONE;
        }
      }

      // check failed, print debug info
      return Errorcodes.ERR_SPI_WRITE_FAILED;
    }

    private Byte[] MultiSPI(Byte cmd, Byte address, Byte[] value, UInt16 size) {
      Byte[] tx = new Byte[size + 1];
      tx[0] = (Byte)(address | cmd);
      if(cmd == SPICommands.Write) {
        for(UInt16 i = 0; i < size; i++) {
          tx[i + 1] = value[i];
        }
      }
      this.PinChipSelect.Write(GpioPinValue.Low);
      Byte[] rx = this.SpiChannel.SendReceive(tx);
      this.PinChipSelect.Write(GpioPinValue.High);
      Byte[] spibuf = new Byte[size];
      for(UInt16 i = 0; i < size; i++) {
        spibuf[i] = rx[i + 1];
      }
      return spibuf;
    }
  }
}