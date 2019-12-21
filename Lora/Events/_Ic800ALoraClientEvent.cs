using System;
using Fraunhofer.Fit.Iot.Lora.lib;
using static Fraunhofer.Fit.Iot.Lora.lib.Ic880alora;

namespace Fraunhofer.Fit.Iot.Lora.Events {
  /*public struct IC880ADataFrame {
    public Modulation modulation;
    public Byte size;
    public Double rssi;
    public Double snr;
    public Double snr_max;
    public Double snr_min;
    public UInt16 calccrc;
    public Crc status;
    public Reciever radio;
    public Byte @interface; 
    public UInt32 freq_hz;
    public BW bandwidth;
    public SF spreadingfactor;
    public CR coderate;
    public UInt32 count_us;
    public Byte[] payload;
    public override String ToString() => "IC880A: " + this.modulation.ToString("g") + " " +
        "Size: " + this.size + " " +
        "Rssi: " + this.rssi + " " +
        "SNR: (" + this.snr + "/" + this.snr_max + "/" + this.snr_min + ") " +
        "CRC: " + this.calccrc + " " +
        "CRCStatus: " + this.status.ToString("g") + " " +
        "RF: " + this.radio.ToString("g") + " " +
        "IF" + this.@interface + " " +
        this.freq_hz + "Hz " +
        "BW: " + this.bandwidth.ToString("g") + " " +
        "DR: " + this.spreadingfactor.ToString("g") + " " +
        "CR: " + this.coderate.ToString("g") + " " +
        this.count_us + "us \n" +
        this.payload;
  }

  public class Ic800ALoraClientEvent : LoraClientEvent {
    public Int32 Bandwidth { get; }
    public UInt16 Calculatedcrc { get; }
    public Byte CodingRate { get; }
    public String CrcStatus { get; }
    public UInt32 Frequency { get; }
    public Byte Interface { get; }
    public String Modulation { get; }
    public Byte Radio { get; }
    public Double SnrMax { get; }
    public Double SnrMin { get; }
    public Byte Spreadingfactor { get; }
    public UInt32 Time { get; }

    public Ic800ALoraClientEvent(IC880ADataFrame p) {
      this.Bandwidth = this.ParseBandwidth(p.bandwidth);
      this.Calculatedcrc = p.calccrc;
      this.CodingRate = this.ParseCodingRate(p.coderate);
      this.CrcStatus = this.ParseCrcStatus(p.status);
      this.Frequency = p.freq_hz;
      this.Interface = p.@interface;
      this.Length = p.size;
      this.Modulation = this.ParseModulation(p.modulation);
      this.Radio = this.ParseRadio(p.radio);
      this.Rssi = p.rssi;
      this.Snr = p.snr;
      this.SnrMax = p.snr_max;
      this.SnrMin = p.snr_min;
      this.Spreadingfactor = this.ParseSpreadingFactor(p.spreadingfactor);
      this.Text = p.payload;
      this.Time = p.count_us;
      this.UpdateTime = DateTime.UtcNow;
    }

    private Byte ParseSpreadingFactor(SF spreadingfactor) => spreadingfactor switch
    {
      SF.DR_LORA_SF7 => 7,
      SF.DR_LORA_SF8 => 8,
      SF.DR_LORA_SF9 => 9,
      SF.DR_LORA_SF10 => 10,
      SF.DR_LORA_SF11 => 11,
      SF.DR_LORA_SF12 => 12,
      _ => 0,
    };

    private Byte ParseRadio(Reciever radio) => radio switch
    {
      Reciever.Chain0 => 0,
      Reciever.Chain1 => 1,
      _ => 0,
    };

    private String ParseModulation(Modulation modulation) => modulation switch
    {
      Ic880alora.Modulation.Fsk => "FSK",
      Ic880alora.Modulation.Lora => "Lora",
      _ => "",
    };

    private String ParseCrcStatus(Crc status) => status switch
    {
      Crc.CrcOk => "Ok",
      Crc.CrcBad => "Bad",
      Crc.CrcNo => "No",
      _ => "",
    };

    private Byte ParseCodingRate(CR coderate) => coderate switch
    {
      CR.CR_LORA_4_5 => 5,
      CR.CR_LORA_4_6 => 6,
      CR.CR_LORA_4_7 => 7,
      CR.CR_LORA_4_8 => 8,
      _ => 0,
    };

    private Int32 ParseBandwidth(BW bandwidth) => bandwidth switch
    {
      BW.BW_7K8HZ => 7800,
      BW.BW_15K6HZ => 15600,
      BW.BW_31K2HZ => 31250,
      BW.BW_62K5HZ => 62500,
      BW.BW_125KHZ => 125000,
      BW.BW_250KHZ => 250000,
      BW.BW_500KHZ => 500000,
      _ => 0,
    };
  }*/
}
