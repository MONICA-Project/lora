using System;
using System.Collections.Generic;

using Fraunhofer.Fit.Iot.Lora.Events;

namespace Fraunhofer.Fit.Iot.Lora.lib {

  public struct Dagionodata {
    public Byte[] Data {
      get;
      internal set;
    }
    public Double Rssi {
      get;
      internal set;
    }
    public Double Snr {
      get;
      internal set;
    }
    public Double FreqError {
      get;
      internal set;
    }
    public Boolean Crc {
      get;
      internal set;
    } = true;
  }

  public abstract class LoraBoard : SpiCom, IDisposable {
    protected Dictionary<String, String> config;

    protected LoraBoard(Dictionary<String, String> settings) => this.config = settings;
    public abstract void Dispose();
    public abstract Boolean Begin();
    //public abstract Boolean End();
    public abstract Boolean Send(Byte[] data, Byte @interface);
    public abstract Boolean StartEventRecieving();

    protected void RaiseRecieveEvent(LoraClientEvent data) => this.Recieve?.Invoke(this, data);
    public delegate void DataUpdate(Object sender, LoraClientEvent e);
    public event DataUpdate Recieve;

    protected void Debug(String text) => Console.WriteLine(text);
  }
}
