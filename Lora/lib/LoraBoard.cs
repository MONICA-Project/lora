using System;
using System.Collections.Generic;

using Fraunhofer.Fit.Iot.Lora.Events;

namespace Fraunhofer.Fit.Iot.Lora.lib {
  public abstract class LoraBoard : SpiCom, IDisposable {
    protected Dictionary<String, String> config;

    protected LoraBoard(Dictionary<String, String> settings) => this.config = settings;
    public abstract void Dispose();
    public abstract Boolean Begin();
    public abstract Boolean End();
    public abstract Boolean Send(Byte[] data, Byte @interface);
    public abstract Boolean StartRecieving();

    protected void RaiseRecieveEvent(LoraClientEvent data) => this.Recieve?.Invoke(this, data);
    public delegate void DataUpdate(Object sender, LoraClientEvent e);
    public event DataUpdate Recieve;

    protected void Debug(String text) => Console.WriteLine(text);
  }
}
