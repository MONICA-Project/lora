using System;

namespace Fraunhofer.Fit.Iot.Lora.Events {
  public class DragionoRecievedObj : RecievedData {
    public Double Rssi {
      get; set;
    }
    public Double Snr {
      get; set;
    }
    public Double FreqError {
      get; set;
    }
    public Boolean Crc {
      get; set;
    } = true;

    public override String ToString() => "Dragino: RSSI: " + this.Rssi + " dBm, SNR: " + this.Snr + " dB, FError: " + Math.Round(this.FreqError) + " Hz, CRC: " + this.Crc + ", Data: " + BitConverter.ToString(this.Data).Replace("-", " ");
  }
}
