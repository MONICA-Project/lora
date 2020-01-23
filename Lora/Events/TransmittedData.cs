using System;

namespace Fraunhofer.Fit.Iot.Lora.Events {
  public class TransmittedData: EventArgs {
    public Byte[] Data {
      get; set;
    }

    public Boolean Msgtolong {
      get; set;
    } = false;

    public Boolean Txtimeout {
      get; set;
    } = false;

    public Double Datarate {
      get; set;
    }
  }
}
