using System;
using System.Collections.Generic;
using System.Text;

namespace Fraunhofer.Fit.Iot.Lora.lib.Dragino {
  public partial class Dragino {
    private struct RadioLibTypes {
      public static Byte UseSpi => 0x00;
      public static Byte IntBoth => 0x03;
    }

    private struct Constances {
      public static Byte ChipVersion => 0x12;
    }

    private struct RegisterAdresses {
      public static Byte Version => 0x42;
    }
  }
}
