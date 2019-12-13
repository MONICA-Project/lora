using System;

namespace Fraunhofer.Fit.Iot.Lora.lib.Dragino {
  public partial class Dragino {
    private struct RadioLibTypes {
      public static Byte RADIOLIB_USE_SPI => 0x00;
      public static Byte RADIOLIB_INT_BOTH => 0x03;
    }

    private struct Constances {
      public static Byte SX1278_CHIP_VERSION => 0x12;
      public static Byte SX127X_FSK_OOK => 0b00000000;
      public static Byte SX127X_LORA => 0b10000000;
      public static Byte SX127X_SLEEP => 0b00000000;
      public static Byte SX127X_STANDBY => 0b00000001;
      public static Byte SX127X_SYNC_WORD => 0x12;
      public static Byte SX127X_OCP_OFF => 0b00000000;
      public static Byte SX127X_OCP_ON => 0b00100000;
      public static Byte SX127X_HOP_PERIOD_OFF => 0b00000000;
    }

    private struct RegisterAdresses {
      public static Byte SX127X_REG_OP_MODE => 0x01;
      public static Byte SX127X_REG_OCP => 0x0B;
      public static Byte SX127X_REG_PREAMBLE_MSB => 0x20;
      public static Byte SX127X_REG_PREAMBLE_LSB => 0x21;
      public static Byte SX127X_REG_HOP_PERIOD => 0x24;
      public static Byte SX127X_REG_PREAMBLE_MSB_FSK => 0x25;
      public static Byte SX127X_REG_PREAMBLE_LSB_FSK => 0x26;
      public static Byte SX127X_REG_SYNC_WORD => 0x39;
      public static Byte SX127X_REG_VERSION => 0x42;
    }
  }
}
