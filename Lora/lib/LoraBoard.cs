using System;

using Fraunhofer.Fit.Iot.Lora.Events;

namespace Fraunhofer.Fit.Iot.Lora.lib {
  public abstract class LoraBoard {
    public abstract Boolean Begin();
    public abstract void Dispose();
    public abstract Boolean Send(Byte[] data, Byte @interface);
    public abstract Boolean StartRecieving();

    protected void RaiseUpdateEvent(LoraClientEvent data) => this.Recieve?.Invoke(this, data);
    public delegate void DataUpdate(Object sender, LoraClientEvent e);
    public event DataUpdate Recieve;
  }
}
