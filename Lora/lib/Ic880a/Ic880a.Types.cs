using System;

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

    enum Sx127xRxbwE {
      LGW_SX127X_RXBW_2K6_HZ,
      LGW_SX127X_RXBW_3K1_HZ,
      LGW_SX127X_RXBW_3K9_HZ,
      LGW_SX127X_RXBW_5K2_HZ,
      LGW_SX127X_RXBW_6K3_HZ,
      LGW_SX127X_RXBW_7K8_HZ,
      LGW_SX127X_RXBW_10K4_HZ,
      LGW_SX127X_RXBW_12K5_HZ,
      LGW_SX127X_RXBW_15K6_HZ,
      LGW_SX127X_RXBW_20K8_HZ,
      LGW_SX127X_RXBW_25K_HZ,
      LGW_SX127X_RXBW_31K3_HZ,
      LGW_SX127X_RXBW_41K7_HZ,
      LGW_SX127X_RXBW_50K_HZ,
      LGW_SX127X_RXBW_62K5_HZ,
      LGW_SX127X_RXBW_83K3_HZ,
      LGW_SX127X_RXBW_100K_HZ,
      LGW_SX127X_RXBW_125K_HZ,
      LGW_SX127X_RXBW_166K7_HZ,
      LGW_SX127X_RXBW_200K_HZ,
      LGW_SX127X_RXBW_250K_HZ
    };

    struct Lgw_sx127x_FSK_bandwidth_s {
      public UInt32 RxBwKHz;
      public Byte RxBwMant;
      public Byte RxBwExp;

      public Lgw_sx127x_FSK_bandwidth_s(UInt32 rxbwkhz, Byte rxbwmant, Byte rxbwexp) {
        this.RxBwKHz = rxbwkhz;
        this.RxBwMant = rxbwmant;
        this.RxBwExp = rxbwexp;

      }
    };

    readonly Lgw_sx127x_FSK_bandwidth_s[] sx127x_FskBandwidths = {
      new Lgw_sx127x_FSK_bandwidth_s(2600  , 2, 7),
      new Lgw_sx127x_FSK_bandwidth_s(2600  , 2, 7 ),   /* LGW_SX127X_RXBW_2K6_HZ */
      new Lgw_sx127x_FSK_bandwidth_s(3100  , 1, 7 ),   /* LGW_SX127X_RXBW_3K1_HZ */
      new Lgw_sx127x_FSK_bandwidth_s(3900  , 0, 7 ),   /* ... */
      new Lgw_sx127x_FSK_bandwidth_s(5200  , 2, 6 ),
      new Lgw_sx127x_FSK_bandwidth_s(6300  , 1, 6 ),
      new Lgw_sx127x_FSK_bandwidth_s(7800  , 0, 6 ),
      new Lgw_sx127x_FSK_bandwidth_s(10400 , 2, 5 ),
      new Lgw_sx127x_FSK_bandwidth_s(12500 , 1, 5 ),
      new Lgw_sx127x_FSK_bandwidth_s(15600 , 0, 5 ),
      new Lgw_sx127x_FSK_bandwidth_s(20800 , 2, 4 ),
      new Lgw_sx127x_FSK_bandwidth_s(25000 , 1, 4 ),   /* ... */
      new Lgw_sx127x_FSK_bandwidth_s(31300 , 0, 4 ),
      new Lgw_sx127x_FSK_bandwidth_s(41700 , 2, 3 ),
      new Lgw_sx127x_FSK_bandwidth_s(50000 , 1, 3 ),
      new Lgw_sx127x_FSK_bandwidth_s(62500 , 0, 3 ),
      new Lgw_sx127x_FSK_bandwidth_s(83333 , 2, 2 ),
      new Lgw_sx127x_FSK_bandwidth_s(100000, 1, 2 ),
      new Lgw_sx127x_FSK_bandwidth_s(125000, 0, 2 ),
      new Lgw_sx127x_FSK_bandwidth_s(166700, 2, 1 ),
      new Lgw_sx127x_FSK_bandwidth_s(200000, 1, 1 ),   /* ... */
      new Lgw_sx127x_FSK_bandwidth_s(250000, 0, 1 )    /* LGW_SX127X_RXBW_250K_HZ */
    };

    enum RadioTypeSx127x
    {
      LGW_RADIO_TYPE_NONE,
      LGW_RADIO_TYPE_SX1272,
      LGW_RADIO_TYPE_SX1276
    }

    struct RadioTypeVersion
    {
      public RadioTypeSx127x Type { get; set; }
      public Byte Version { get; set; }
      public RadioTypeVersion(RadioTypeSx127x t, Byte r) {
        this.Type = t;
        this.Version = r;
      }
    }

    public struct LbtChan {
      public UInt32 freq_hz;
      public UInt16 scan_time_us;
    };

    public class LGWRegisters {
      /// <summary>
      /// page containing the register (-1 for all pages)
      /// </summary>
      public SByte RegisterPage;
      /// <summary>
      /// base address of the register (7 bit)
      /// </summary>
      public Byte Address;
      /// <summary>
      /// position of the register LSB (between 0 to 7)
      /// </summary>
      public Byte BitOffset;
      /// <summary>
      /// indicates the register is signed (2 complem.)
      /// </summary>
      public Boolean SignedInt;
      /// <summary>
      /// number of bits in the register
      /// </summary>
      public Byte SizeInBits;
      /// <summary>
      /// indicates a read-only register
      /// </summary>
      public Boolean ReadonlyRegister;
      /// <summary>
      /// register default value
      /// </summary>
      public Int32 DefaultValue;
      /// <summary>
      /// A Register of SX3101
      /// </summary>
      /// <param name="registerPage">page containing the register (-1 for all pages)</param>
      /// <param name="address">base address of the register (7 bit)</param>
      /// <param name="bitOffset">position of the register LSB (between 0 to 7)</param>
      /// <param name="signedInt">indicates the register is signed (2 complem.)</param>
      /// <param name="sizeInBits">number of bits in the register</param>
      /// <param name="readonlyRegister">indicates a read-only register</param>
      /// <param name="defaultValue">register default value</param>
      public LGWRegisters(SByte registerPage, Byte address, Byte bitOffset, Boolean signedInt, Byte sizeInBits, Boolean readonlyRegister, Int32 defaultValue) {
        this.RegisterPage = registerPage;
        this.Address = address;
        this.BitOffset = bitOffset;
        this.SignedInt = signedInt;
        this.SizeInBits = sizeInBits;
        this.ReadonlyRegister = readonlyRegister;
        this.DefaultValue = defaultValue;
      }
      public override String ToString() => "Reg: [P:" + this.RegisterPage + ",A:" + this.Address + ",O:" + this.BitOffset + "]";
    };

    public class FpgaRegisters : LGWRegisters {
      public Byte mux;
      public FpgaRegisters(SByte registerPage, Byte address, Byte bitOffset, Boolean signedInt, Byte sizeInBits, Boolean readonlyRegister, Int32 defaultValue, Byte mux) : base(registerPage, address, bitOffset, signedInt, sizeInBits, readonlyRegister, defaultValue) => this.mux = mux;
    }

    public struct Firmwaredata {
      public Byte Mcu;
      public Byte[] Data;
      public UInt16 Size;
      public Byte Version;
      public Byte Address;
      public Firmwaredata(Byte mcu, Byte[] data, UInt16 size, Byte version, Byte addr) {
        this.Mcu = mcu;
        this.Data = data;
        this.Size = size;
        this.Version = version;
        this.Address = addr;
      }
    }

    public struct Lutstruct {
      public Byte dig_gain;
      public Byte pa_gain;
      public Byte dac_gain;
      public Byte mix_gain;
      public Byte rf_power;
      public Lutstruct(Byte dig_gain, Byte pa_gain, Byte dac_gain, Byte mix_gain, Byte rf_power) {
        this.dig_gain = dig_gain;
        this.pa_gain = pa_gain;
        this.dac_gain = dac_gain;
        this.mix_gain = mix_gain;
        this.rf_power = rf_power;
      }
    }
  }
}
