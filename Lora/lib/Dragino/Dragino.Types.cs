using System;

namespace Fraunhofer.Fit.Iot.Lora.lib.Dragino {
  public partial class Dragino {
    private struct RadioLibTypes {
      public static Byte UseSpi => 0x00;
      public static Byte IntBoth => 0x03;
    }

    private struct Constances {
      public static Byte ChipVersion => 0x12;
      public static Byte Lora => 0b10000000;
      public static Byte Sleep => 0b00000000;
      public static Byte Standby => 0b00000001;
    }

    private struct RegisterAdresses {
      public static Byte OpMode => 0x01;
      public static Byte Version => 0x42;
    }
  }
}
