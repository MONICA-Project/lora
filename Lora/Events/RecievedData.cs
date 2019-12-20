using System;

namespace Fraunhofer.Fit.Iot.Lora.Events {
  public class RecievedData : EventArgs {
    public Byte[] Data {
      get; set;
    }

    public Double Rssi {
      get; set;
    }

    public Double Snr {
      get; set;
    }

    public Boolean Crc {
      get; set;
    } = true;

    public DateTime RecievedTime {
      get;
    } = DateTime.Now;
  }
}
