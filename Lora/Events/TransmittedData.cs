using System;

namespace Fraunhofer.Fit.Iot.Lora.Events {
  public class TransmittedData: EventArgs {
    public Byte[] Data {
      get; set;
    }
  }
}
