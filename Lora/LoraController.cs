using System;
using System.Collections.Generic;
using Fraunhofer.Fit.Iot.Lora.Trackers;
using Fraunhofer.Fit.Iot.Lora.Events;
using Fraunhofer.Fit.Iot.Lora.lib;
using BlubbFish.Utils;
using System.Threading.Tasks;

namespace Fraunhofer.Fit.Iot.Lora {
  public class LoraController : IDisposable {
    private LoraConnector loraconnector;
    private readonly Dictionary<String, String> settings;

    public delegate void UpdateDataEvent(Object sender, DataUpdateEvent e);
    public delegate void UpdateStatusEvent(Object sender, StatusUpdateEvent e);
    public event UpdateDataEvent DataUpdate;
    public event UpdateStatusEvent StatusUpdate;
    public Dictionary<String, Tracker> trackers = new Dictionary<String, Tracker>();
    public LoraController(Dictionary<String, String> settings, Boolean receive = true) {
      this.settings = settings;
      try {
        this.CreateLoraController(receive);
        this.loraconnector.Update += this.ReceivePacket;
        this.loraconnector.StartRadio();
        this.loraconnector.AttachUpdateEvent();
      } catch(Exception e) {
        Helper.WriteError("Error while Loading Fraunhofer.Fit.Iot.Lora.LoraController.LoraController: " + e.Message + "\n\n" + e.StackTrace);
        throw;
      }
    }

    private void CreateLoraController(Boolean receive = true) {
      if((!this.settings.ContainsKey("frequency") || !this.settings.ContainsKey("spreadingfactor") || !this.settings.ContainsKey("signalbandwith") || !this.settings.ContainsKey("codingrate")) && this.settings.ContainsKey("type") && this.settings["type"].ToUpperLower() == "Draginolora") {
        Helper.WriteError("Not all Settings set!: [lora]\nfrequency=868100000\nspreadingfactor=8\nsignalbandwith=125000\ncodingrate=6 missing");
        return;
      }
      this.loraconnector = LoraConnector.GetInstance(this.settings);
      this.loraconnector.Begin();
      this.loraconnector.ParseConfig();
      if (receive) {
        this.loraconnector.Receive(0);
      } else {
        this.loraconnector.SetTxPower(17);
        while (true) {
          this.loraconnector.BeginPacket();
          this.loraconnector.Write(System.Text.Encoding.UTF8.GetBytes("TEST TEST TEST"));
          this.loraconnector.EndPacket();
          Console.WriteLine("Send!");
          System.Threading.Thread.Sleep(1000);
        }
      }
    }

    private async void ReceivePacket(Object sender, LoraClientEvent e) {
      await Task.Run(() => {
        Console.WriteLine("Fraunhofer.Fit.Iot.Lora.LoraController.ReceivePacket: " + e.Text.Length.ToString());
        String trackerName = "";
        Byte[] binaryUpdate = { };
        String textStatus = "";
        String textUpdate = "";
        if (e.Text.StartsWith("b") && e.Text.Length == 27) {
          //###### Binary Packet, starts with "b" #########
          binaryUpdate = System.Text.Encoding.ASCII.GetBytes(e.Text);
          trackerName = Tracker.GetName(binaryUpdate);
          Console.WriteLine("Fraunhofer.Fit.Iot.Lora.LoraController.ReceivePacket: |" + BitConverter.ToString(binaryUpdate).Replace("-", " ") + "| PRSSI: " + e.Packetrssi + " RSSI:" + e.Rssi + " SNR:" + e.Snr);
        } else if (e.Text.Length == 256) {
          binaryUpdate = System.Text.Encoding.ASCII.GetBytes(e.Text);
          Console.WriteLine("Fraunhofer.Fit.Iot.Lora.LoraController.ReceivePacket: |" + BitConverter.ToString(binaryUpdate).Replace("-", " ") + "| PRSSI: " + e.Packetrssi + " RSSI:" + e.Rssi + " SNR:" + e.Snr);
        } else if (e.Text.StartsWith("deb")) {
          //###### Debug Packet, three lines #############
          Console.WriteLine("Fraunhofer.Fit.Iot.Lora.LoraController.ReceivePacket: |" + e.Text + "| PRSSI: " + e.Packetrssi + " RSSI:" + e.Rssi + " SNR:" + e.Snr);
          if (Tracker.CheckPacket(e.Text)) {
            textStatus = e.Text;
            trackerName = Tracker.GetName(textStatus, 1);
          } else {
            Console.WriteLine("Fraunhofer.Fit.Iot.Lora.LoraController.ReceivePacket: Debug-Packet not Match!");
          }
        } else {
          //###### Normal Packet, two lines #############
          Console.WriteLine("Fraunhofer.Fit.Iot.Lora.LoraController.ReceivePacket: |" + e.Text + "| PRSSI: " + e.Packetrssi + " RSSI:" + e.Rssi + " SNR:" + e.Snr);
          if (Tracker.CheckPacket(e.Text)) {
            textUpdate = e.Text;
            trackerName = Tracker.GetName(textUpdate, 0);
          } else {
            Console.WriteLine("Fraunhofer.Fit.Iot.Lora.LoraController.ReceivePacket: Packet not Match!");
          }
        }
        if (trackerName != "") {
          if (!this.trackers.ContainsKey(trackerName)) {
            this.trackers.Add(trackerName, new Tracker());
            this.trackers[trackerName].DataUpdate += this.DataUpdates;
            this.trackers[trackerName].StatusUpdate += this.StatusUpdates;
          }
          if (binaryUpdate.Length > 0) {
            this.trackers[trackerName].SetUpdate(e, binaryUpdate);
          }
          if (textStatus != "") {
            this.trackers[trackerName].SetStatus(e, textStatus);
          }
          if (textUpdate != "") {
            this.trackers[trackerName].SetUpdate(e, textUpdate);
          }
        }
      });
    }

    private void StatusUpdates(Object sender, StatusUpdateEvent e) {
      this.StatusUpdate?.Invoke(sender, e);
    }

    private void DataUpdates(Object sender, DataUpdateEvent e) {
      this.DataUpdate?.Invoke(sender, e);
    }

    #region IDisposable Support
    private Boolean disposedValue = false;

    protected virtual void Dispose(Boolean disposing) {
      Console.WriteLine("LoraController.Dispose(" + disposing + ")");
      if (!this.disposedValue) {
        if (disposing) {
          if (this.loraconnector != null) {
            this.loraconnector.Update -= this.ReceivePacket;
            this.loraconnector.End();
            this.loraconnector.Dispose();
          }
        }
        this.loraconnector = null;
        this.disposedValue = true;
      }
    }

    public void Dispose() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
    #endregion
  }
}
