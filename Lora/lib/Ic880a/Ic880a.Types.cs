using System;
using System.Collections.Generic;
using System.Text;

namespace Fraunhofer.Fit.Iot.Lora.lib.Ic880a {
  public partial class Ic880a {
    public enum RadioDataType : Byte {
      Undefined = 0,
      Lora = 16,
      LoraMulti = 17,
      FSK = 32
    }

    public enum Reciever : Byte {
      Chain0 = 0,
      Chain1 = 1
    }

    public enum Crc : Byte {
      CrcOk = 0x10,
      CrcBad = 0x11,
      CrcNo = 0x01,
      CrcUndefined = 0x00
    }

    public enum Modulation : Byte {
      Undefined = 0,
      Lora = 0x10,
      Fsk = 0x20
    }

    public enum BW {
      Undefined = 0,
      BW_500KHZ = 1,
      BW_250KHZ = 2,
      BW_125KHZ = 3,
      BW_62K5HZ = 4,
      BW_31K2HZ = 5,
      BW_15K6HZ = 6,
      BW_7K8HZ = 7
    }

    public enum SF : Byte {
      Undefined = 0,
      DR_LORA_SF7 = 2,
      DR_LORA_SF8 = 4,
      DR_LORA_SF9 = 8,
      DR_LORA_SF10 = 16,
      DR_LORA_SF11 = 32,
      DR_LORA_SF12 = 64,
      DR_LORA_SFMULTI = 126
    }

    public enum CR : Byte {
      Undefined = 0,
      CR_LORA_4_5 = 1,
      CR_LORA_4_6 = 2,
      CR_LORA_4_7 = 3,
      CR_LORA_4_8 = 4
    }

    enum RadioType : Byte {
      SX1255 = 0,
      SX1257 = 1
    }
  }
}
