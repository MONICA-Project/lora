using System;

namespace Fraunhofer.Fit.Iot.Lora.Events {
  public class DragionoRecievedObj : RecievedData {
    public Double FreqError {
      get; set;
    }

    public override String ToString() => "Dragino: RSSI: " + this.Rssi + " dBm, SNR: " + this.Snr + " dB, FError: " + Math.Round(this.FreqError) + " Hz, CRC: " + this.Crc + ", Data: " + BitConverter.ToString(this.Data).Replace("-", " ");
  }
}
