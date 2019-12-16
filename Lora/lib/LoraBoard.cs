using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fraunhofer.Fit.Iot.Lora.Events;

namespace Fraunhofer.Fit.Iot.Lora.lib {

  public class RecievedData {  }

  public class SendedData {  }

  public class DragionoRecievedObj : RecievedData {
    public Byte[] Data {
      get; set;
    }
    public Double Rssi {
      get; set;
    }
    public Double Snr {
      get; set;
    }
    public Double FreqError {
      get; set;
    }
    public Boolean Crc { get; set; } = true;
  }

  public class DragionoSendedObj : SendedData {
    public Byte[] Data {
      get; set;
    }
    public Double Datarate {
      get; set;
    }
    public Boolean Tolong { get; set; } = false;
    public Boolean Txtimeout { get; set; } = false;
    public Int16 Errorcode {
      get; set;
    }
  }

  public abstract class LoraBoard : SpiCom, IDisposable {
    protected Dictionary<String, String> config;

    protected LoraBoard(Dictionary<String, String> settings) => this.config = settings;
    public abstract void Dispose();
    public abstract Boolean Begin();
    //public abstract Boolean End();
    public abstract Boolean Send(Byte[] data, Byte @interface);
    public abstract Boolean StartEventRecieving();

    protected async void RaiseRecieveEvent(RecievedData obj) => await Task.Run(() => this.Recieved?.Invoke(this, obj));
    protected async void RaiseSendedEvent(SendedData obj) => await Task.Run(() => this.Sended?.Invoke(this, obj));
    public delegate void RecieveUpdate(Object sender, RecievedData e);
    public delegate void SendUpdate(Object sender, SendedData e);
    public event RecieveUpdate Recieved;
    public event SendUpdate Sended;

    protected void Debug(String text) => Console.WriteLine(text);
  }
}
