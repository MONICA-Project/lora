using System;

namespace Fraunhofer.Fit.Iot.Lora.Events {
  public class DragionoTransmittedObj : TransmittedData {
    public Double Datarate {
      get; set;
    }
    public Boolean Msgtolong {
      get; set;
    } = false;
    public Boolean Txtimeout {
      get; set;
    } = false;
    public Int16 Errorcode {
      get; set;
    }

    public override String ToString() => "Dragino: Datarate: " + this.Datarate.ToString("F2") + " bps, MsgToLong: " + this.Msgtolong + ", Timeout: " + this.Txtimeout + ", Error: " + this.Errorcode + ", Data: " + BitConverter.ToString(this.Data).Replace("-", " ");
  }
}
