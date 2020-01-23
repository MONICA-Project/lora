using System;

namespace Fraunhofer.Fit.Iot.Lora.Events {
  public class Ic880aTransmittedObj : TransmittedData {
    public override String ToString() => "IC880A: Datarate: " + this.Datarate.ToString("F2") + " bps, MsgToLong: " + this.Msgtolong + ", Timeout: " + this.Txtimeout + ",  Data: " + BitConverter.ToString(this.Data).Replace("-", " ");
  }
}
