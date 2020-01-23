using System;
using System.Collections.Generic;
using System.Threading;

using BlubbFish.Utils;

using Fraunhofer.Fit.Iot.Lora.Events;
using Fraunhofer.Fit.Iot.Lora.lib;

namespace Fraunhofer.Fit.Iot.Lora {
  public class LoraController : IDisposable {
    private LoraBoard loraboard;
    private Boolean _isinit = false;
    private Thread _testThread;
    private Boolean _testThreadRunning = false;

    public delegate void TransmittedEvent(Object sender, TransmittedData e);
    public delegate void ReceivedEvent(Object sender, RecievedData e);
    public event TransmittedEvent Transmitted;
    public event ReceivedEvent Received;

    public LoraController(Dictionary<String, String> settings) {
      try {
        this.loraboard = LoraBoard.GetInstance(settings);
        this.loraboard.Recieved += this.PacketReceived;
        this.loraboard.Transmitted += this.PacketTransmitted;
        this.loraboard.Begin();
        this.loraboard.StartEventRecieving();
        this._isinit = true;
        if(settings.ContainsKey("debug") && settings["debug"] == "true") {
          this._testThread = new Thread(this.TestRunner);
          this._testThreadRunning = true;
          this._testThread.Start();
        }
      } catch(Exception e) {
        Helper.WriteError("Error while Loading Fraunhofer.Fit.Iot.Lora.LoraController.LoraController: " + e.Message + "\n\n" + e.StackTrace);
        throw;
      }
    }

    public void Dispose() {
      if(this._isinit) {
        Console.WriteLine("Fraunhofer.Fit.Iot.Lora.LoraController.Dispose()");
        this._isinit = false;
        if(this._testThreadRunning) {
          this._testThreadRunning = false;
          while(this._testThread.IsAlive) {
            Thread.Sleep(10);
          }
          this._testThread = null;
        }
        this.loraboard.Dispose();
        this.loraboard = null;
      }
    }

    private void TestRunner() {
      DateTime start = DateTime.Now;
      while(this._testThreadRunning) {
        if((DateTime.Now - start).TotalSeconds > 30) {
          try {
            for (Byte i = 0; i < this.loraboard.Interfaces; i++) {
              this.loraboard.Send(System.Text.Encoding.UTF8.GetBytes("TEST TEST TEST"), i);
            }
          } catch(Exception e) {
            Helper.WriteError("Error while Loading Fraunhofer.Fit.Iot.Lora.LoraController.TestRunner: " + e.Message + "\n\n" + e.StackTrace);
          }
          start = DateTime.Now;
        }
        Thread.Sleep(10);
      }
    }

    private void PacketTransmitted(Object sender, TransmittedData e) => this.Transmitted?.Invoke(sender, e);

    private void PacketReceived(Object sender, RecievedData e) => this.Received?.Invoke(sender, e);

    public void Send(Byte[] data, Byte @interface) {
      if(this._isinit) {
        this.loraboard.Send(data, @interface);
      }
    }
  }
}
