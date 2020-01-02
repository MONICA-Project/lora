using System;
using System.Threading;
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
      public static Int16 ERR_INVALID_GAIN => -19;
      public static Int16 ERR_INVALID_PREAMBLE_LENGTH => -18;
      public static Int16 ERR_INVALID_CURRENT_LIMIT => -17;
      public static Int16 ERR_SPI_WRITE_FAILED => -16;
      public static Int16 ERR_INVALID_OUTPUT_POWER => -13;
      public static Int16 ERR_INVALID_FREQUENCY => -12;
      public static Int16 ERR_INVALID_BIT_RANGE => -11;
      public static Int16 ERR_INVALID_CODING_RATE => -10;
      public static Int16 ERR_INVALID_SPREADING_FACTOR => -9;
      public static Int16 ERR_INVALID_BANDWIDTH => -8;
      public static Int16 ERR_CRC_MISMATCH => -7;
      public static Int16 ERR_TX_TIMEOUT => -5;
      public static Int16 ERR_PACKET_TOO_LONG => -4;
      public static Int16 ERR_UNKNOWN => -1;
      public static Int16 ERR_NONE => 0;
    }

    protected Int32 BitCheck(Byte b, Int32 p, Int32 n) => ((b) >> (p)) & ((1 << (n)) - 1);

    protected Byte SPIreadRegister(Byte address, Byte mux = 0) => mux == 0 ? this.MultiSPI(SPICommands.Read, address, null, 1)[0] : this.MultiSPIMux(SPICommands.Read, address, null, 1, mux)[1];

    protected void SPIwriteRegister(Byte address, Byte data) => this.MultiSPI(SPICommands.Write, (Byte)(address | SPICommands.Write), new Byte[] { data }, 1);

    protected void SPIwriteRegisterRaw(Byte address, Byte data, Byte mux = 0) => _ = mux == 0 ? this.MultiSPI(SPICommands.Write, address, new Byte[] { data }, 1) : this.MultiSPIMux(SPICommands.Write, address, new Byte[] { data }, 1, mux);

    protected Int16 SPIgetRegValue(Byte address, Byte msb = 7, Byte lsb = 0) {
      if(msb > 7 || lsb > 7 || lsb > msb) {
        return Errorcodes.ERR_INVALID_BIT_RANGE;
      }

      Byte rawValue = this.SPIreadRegister(address);
      return (Byte)(rawValue & ((0b11111111 << lsb) & (0b11111111 >> (7 - msb))));
    }

    protected Int16 SPIsetRegValue(Byte address, Byte value, Byte msb = 7, Byte lsb = 0, UInt16 checkInterval = 2) {
      if(msb > 7 || lsb > 7 || lsb > msb) {
        return Errorcodes.ERR_INVALID_BIT_RANGE;
      }

      Byte currentValue = this.SPIreadRegister(address);
      Byte mask = (Byte)~((0b11111111 << (msb + 1)) | (0b11111111 >> (8 - lsb)));
      Byte newValue = (Byte)((currentValue & ~mask) | (value & mask));
      this.SPIwriteRegister(address, newValue);

      // check register value each millisecond until check interval is reached
      // some registers need a bit of time to process the change (e.g. SX127X_REG_OP_MODE)
      for(UInt16 i = 0; i < checkInterval; i++) {
        currentValue = this.SPIreadRegister(address);
        if(currentValue == newValue) {
          // check passed, we can stop the loop
          return Errorcodes.ERR_NONE;
        }
        Thread.Sleep(1);
      }

      // check failed, print debug info
      return Errorcodes.ERR_SPI_WRITE_FAILED;
    }

    protected Byte[] SPIreadRegisterBurst(Byte address, UInt16 numBytes, Byte mux = 0) => mux == 0 ? this.MultiSPI(SPICommands.Read, address, null, numBytes) : this.MultiSPIMux(SPICommands.Read, address, null, 1, mux);

    protected void SPIwriteRegisterBurst(Byte address, Byte[] data) => this.MultiSPI(SPICommands.Write, (Byte)(address | SPICommands.Write), data, (UInt16)data.Length);

    protected void SPIwriteRegisterBurstRaw(Byte address, Byte[] data, Byte mux = 0) => _ = mux == 0 ? this.MultiSPI(SPICommands.Write, address, data, (UInt16)data.Length) : this.MultiSPIMux(SPICommands.Write, address, data, (UInt16)data.Length, mux);

    private Byte[] MultiSPI(Byte cmd, Byte address, Byte[] value, UInt16 size) {
      Byte[] tx = new Byte[size + 1];
      tx[0] = address;
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

    private Byte[] MultiSPIMux(Byte cmd, Byte address, Byte[] value, UInt16 size, Byte mux) {
      Byte[] tx = new Byte[size + 2];
      tx[0] = mux;
      tx[1] = address;
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
        spibuf[i] = rx[i + 2];
      }
      return spibuf;
    }
  }
}
