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

    /*private readonly Object lockReceivePacket = new Object();
    
    public Dictionary<String, Tracker> trackers = new Dictionary<String, Tracker>();*/
    

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
            this.loraboard.Send(System.Text.Encoding.UTF8.GetBytes("TEST TEST TEST"), 0);
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

    /*private async void ReceivePacket(Object sender, LoraClientEvent e) => await Task.Run(() => {
      Console.WriteLine("Fraunhofer.Fit.Iot.Lora.LoraController.ReceivePacket: " + e.Text.Length.ToString());
      String trackerName = "";
      Byte[] binaryUpdate = { };
      Byte[] binaryPanics = { };
      String textStatus = "";
      String textUpdate = "";
      if(e.Text.Length == 21 && e.Text[0] == 'b') {
        //###### Binary Packet, starts with "b" #########
        binaryUpdate = e.Text;
        trackerName = Tracker.GetName(binaryUpdate);
        Console.WriteLine("Fraunhofer.Fit.Iot.Lora.LoraController.ReceivePacket: |" + BitConverter.ToString(binaryUpdate).Replace("-", " ") + "| PRSSI: " + e.Packetrssi + " RSSI:" + e.Rssi + " SNR:" + e.Snr);
      } else if(e.Text.Length == 21 && e.Text[0] == 'p') {
        //###### Panic Packet, starts with "p" #########
        binaryPanics = e.Text;
        trackerName = Tracker.GetName(binaryPanics);
        Console.WriteLine("Fraunhofer.Fit.Iot.Lora.LoraController.ReceivePacket: |" + BitConverter.ToString(binaryPanics).Replace("-", " ") + "| PRSSI: " + e.Packetrssi + " RSSI:" + e.Rssi + " SNR:" + e.Snr);
      } else if(e.Text.Length > 3 && e.Text[0] == 'd' && e.Text[1] == 'e' && e.Text[2] == 'b') {
        //###### Debug Packet, three lines #############
        String text = System.Text.Encoding.ASCII.GetString(e.Text).Trim();
        Console.WriteLine("Fraunhofer.Fit.Iot.Lora.LoraController.ReceivePacket: |" + text + "| PRSSI: " + e.Packetrssi + " RSSI:" + e.Rssi + " SNR:" + e.Snr);
        if(Tracker.CheckPacket(text)) {
          textStatus = text;
          trackerName = Tracker.GetName(textStatus, 1);
        } else {
          Console.WriteLine("Fraunhofer.Fit.Iot.Lora.LoraController.ReceivePacket: Debug-Packet not Match!");
        }
      } else {
        //###### Normal Packet, two lines #############
        String text = System.Text.Encoding.ASCII.GetString(e.Text).Trim();
        Console.WriteLine("Fraunhofer.Fit.Iot.Lora.LoraController.ReceivePacket: |" + text + "| PRSSI: " + e.Packetrssi + " RSSI:" + e.Rssi + " SNR:" + e.Snr);
        if(Tracker.CheckPacket(text)) {
          textUpdate = text;
          trackerName = Tracker.GetName(textUpdate, 0);
        } else {
          Console.WriteLine("Fraunhofer.Fit.Iot.Lora.LoraController.ReceivePacket: Packet not Match!");
        }
      }
      if(trackerName != "") {
        lock(this.lockReceivePacket) {
          if(!this.trackers.ContainsKey(trackerName)) {
            this.trackers.Add(trackerName, new Tracker());
            this.trackers[trackerName].DataUpdate += this.DataUpdates;
            this.trackers[trackerName].StatusUpdate += this.StatusUpdates;
            this.trackers[trackerName].PanicUpdate += this.PanicUpdates;
          }
          if(binaryUpdate.Length > 0) {
            this.trackers[trackerName].SetUpdate(e, binaryUpdate);
          }
          if(binaryPanics.Length > 0) {
            this.trackers[trackerName].SetPanics(e, binaryPanics);
          }
          if(textStatus != "") {
            this.trackers[trackerName].SetStatus(e, textStatus);
          }
          if(textUpdate != "") {
            this.trackers[trackerName].SetUpdate(e, textUpdate);
          }
        }
      }
    });

    private async void PanicUpdates(Object sender, PanicUpdateEvent e) => await Task.Run(() => this.PanicUpdate?.Invoke(sender, e));

    private async void StatusUpdates(Object sender, StatusUpdateEvent e) => await Task.Run(() => this.StatusUpdate?.Invoke(sender, e));

    private async void DataUpdates(Object sender, DataUpdateEvent e) => await Task.Run(() => this.DataUpdate?.Invoke(sender, e));*/

    
  }
}
