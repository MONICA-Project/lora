using System;
using Fraunhofer.Fit.Iot.Lora.lib;
using static Fraunhofer.Fit.Iot.Lora.lib.Ic880alora;

namespace Fraunhofer.Fit.Iot.Lora.Events {
  public struct IC880ADataFrame {
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
    public override String ToString() {
      return "IC880A: " + this.modulation.ToString("g") + " " +
        "Size: " + this.size + " " +
        "Rssi: " + this.rssi + " " +
        "SNR: (" + this.snr + "/" + this.snr_max + "/" + this.snr_min + ") "+
        "CRC: " + this.calccrc + " " +
        "CRCStatus: " + this.status.ToString("g") + " " + 
        "RF: " + this.radio.ToString("g") + " " + 
        "IF" + this.@interface + " " + 
        this.freq_hz + "Hz "+
        "BW: " + this.bandwidth.ToString("g") + " " +
        "DR: " + this.spreadingfactor.ToString("g") + " " + 
        "CR: " + this.coderate.ToString("g") + " " + 
        this.count_us + "us \n" + 
        this.payload;
    }
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
      this.UpdateTime = DateTime.Now;
    }

    private Byte ParseSpreadingFactor(SF spreadingfactor) {
      switch(spreadingfactor) {
        case SF.DR_LORA_SF7: return 7;
        case SF.DR_LORA_SF8: return 8;
        case SF.DR_LORA_SF9: return 9;
        case SF.DR_LORA_SF10: return 10;
        case SF.DR_LORA_SF11: return 11;
        case SF.DR_LORA_SF12: return 12;
      }
      return 0;
    }

    private Byte ParseRadio(Reciever radio) {
      switch(radio) {
        case Reciever.Chain0: return 0;
        case Reciever.Chain1: return 1;
      }
      return 0;
    }

    private String ParseModulation(Modulation modulation) {
      switch(modulation) {
        case Ic880alora.Modulation.Fsk: return "FSK";
        case Ic880alora.Modulation.Lora: return "Lora";
      }
      return "";
    }

    private String ParseCrcStatus(Crc status) {
      switch(status) {
        case Crc.CrcOk: return "Ok";
        case Crc.CrcBad: return "Bad";
        case Crc.CrcNo: return "No";
      }
      return "";
    }

    private Byte ParseCodingRate(CR coderate) {
      switch(coderate) {
        case CR.CR_LORA_4_5: return 5;
        case CR.CR_LORA_4_6: return 6;
        case CR.CR_LORA_4_7: return 7;
        case CR.CR_LORA_4_8: return 8;
      }
      return 0;
    }

    private Int32 ParseBandwidth(BW bandwidth) {
      switch(bandwidth) {
        case BW.BW_7K8HZ: return 7800;
        case BW.BW_15K6HZ: return 15600;
        case BW.BW_31K2HZ: return 31250;
        case BW.BW_62K5HZ: return 62500;
        case BW.BW_125KHZ: return 125000;
        case BW.BW_250KHZ: return 250000;
        case BW.BW_500KHZ: return 500000;
      }
      return 0;
    }
  }
}
