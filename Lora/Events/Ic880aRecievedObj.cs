using System;
using System.Collections.Generic;
using System.Text;
using Fraunhofer.Fit.Iot.Lora.lib.Ic880a;

namespace Fraunhofer.Fit.Iot.Lora.Events {
  class Ic880aRecievedObj : RecievedData {
    public Byte Interface {
      get; set;
    }
    public Byte Radio {
      get; set;
    }
    public UInt32 Frequency {
      get; set;
    }

    public String CrcStatus {
      get; set;
    } = "";

    public String Modulation {
      get; set;
    } = "";

    public Double SnrMin {
      get; set;
    }

    public Double SnrMax {
      get; set;
    }

    public Int32 Bandwidth {
      get; set;
    } = 0;

    public Byte Spreadingfactor {
      get; set;
    } = 0;

    public Byte CodingRate {
      get; set;
    } = 0;

    public UInt32 Time {
      get; set;
    }

    public UInt16 Calculatedcrc {
      get; set;
    }


    #region Static helpers
    public static Byte ParseRadio(Ic880a.Reciever radio) => radio switch
    {
      Ic880a.Reciever.Chain0 => 0,
      Ic880a.Reciever.Chain1 => 1,
      _ => 0,
    };

    public static String ParseCrcStatus(Ic880a.Crc status) => status switch
    {
      Ic880a.Crc.CrcOk => "Ok",
      Ic880a.Crc.CrcBad => "Bad",
      Ic880a.Crc.CrcNo => "No",
      _ => "",
    };

    public static String ParseModulation(Ic880a.Modulation modulation) => modulation switch
    {
      Ic880a.Modulation.Fsk => "FSK",
      Ic880a.Modulation.Lora => "Lora",
      _ => "",
    };

    public static Int32 ParseBandwidth(Ic880a.BW bandwidth) => bandwidth switch
    {
      Ic880a.BW.BW_7K8HZ => 7800,
      Ic880a.BW.BW_15K6HZ => 15600,
      Ic880a.BW.BW_31K2HZ => 31250,
      Ic880a.BW.BW_62K5HZ => 62500,
      Ic880a.BW.BW_125KHZ => 125000,
      Ic880a.BW.BW_250KHZ => 250000,
      Ic880a.BW.BW_500KHZ => 500000,
      _ => 0,
    };

    public static Byte ParseSpreadingFactor(Ic880a.SF spreadingfactor) => spreadingfactor switch
    {
      Ic880a.SF.DR_LORA_SF7 => 7,
      Ic880a.SF.DR_LORA_SF8 => 8,
      Ic880a.SF.DR_LORA_SF9 => 9,
      Ic880a.SF.DR_LORA_SF10 => 10,
      Ic880a.SF.DR_LORA_SF11 => 11,
      Ic880a.SF.DR_LORA_SF12 => 12,
      _ => 0,
    };

    public static Byte ParseCodingRate(Ic880a.CR coderate) => coderate switch
    {
      Ic880a.CR.CR_LORA_4_5 => 5,
      Ic880a.CR.CR_LORA_4_6 => 6,
      Ic880a.CR.CR_LORA_4_7 => 7,
      Ic880a.CR.CR_LORA_4_8 => 8,
      _ => 0,
    };
    #endregion
  }
}
