using System;
using System.Collections.Generic;
using Fraunhofer.Fit.Iot.Lora.Trackers;
using Fraunhofer.Fit.Iot.Lora.Events;
using Fraunhofer.Fit.Iot.Lora.lib;
using BlubbFish.Utils;

// Hope RFM96
// http://www.hoperf.com/upload/rf/RFM95_96_97_98W.pdf
// The RFM97 offers bandwidth options ranging from 7.8 kHz to 500 kHz with spreading factors ranging from 6 to 12, and covering all available frequency bands.

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
      Console.WriteLine("Fraunhofer.Fit.Iot.Lora.LoraController.LoraController()");
      this.settings = settings;
      try {
        this.CreateLoraController(receive);
        this.loraconnector.Update += this.ReceivePacket;
        this.loraconnector.OnReceive();
      } catch { }
    }

    private void CreateLoraController(Boolean receive = true) {
      if(!this.settings.ContainsKey("frequency") || !this.settings.ContainsKey("spreadingfactor") | !this.settings.ContainsKey("signalbandwith") || !this.settings.ContainsKey("codingrate")) {
        Helper.WriteError("Not all Settings set!: [lora]\nfrequency=868100000\nspreadingfactor=8\nsignalbandwith=125000\ncodingrate=6 missing");
        return;
      }
      this.loraconnector = new DraginoLora(Unosquare.RaspberryIO.Pi.Gpio.Pin06, Unosquare.RaspberryIO.Pi.Gpio.Pin07, Unosquare.RaspberryIO.Pi.Gpio.Pin00);
      this.loraconnector.Begin(Int64.Parse(this.settings["frequency"])); //868125100
      this.loraconnector.SetSignalBandwith(Int64.Parse(this.settings["signalbandwith"])); //125000
      this.loraconnector.SetSpreadingFactor(Byte.Parse(this.settings["spreadingfactor"])); //8 - 11
      this.loraconnector.SetCodingRate4(Byte.Parse(this.settings["codingrate"]));
      //this.loraconnector.EnableCrc();
      this.loraconnector.DisableCrc();
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

    private void ReceivePacket(Object sender, LoraClientEvent e) {
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
        if(binaryUpdate.Length > 0) {
          this.trackers[trackerName].SetUpdate(e, binaryUpdate);
        }
        if(textStatus != "") {
          this.trackers[trackerName].SetStatus(e, textStatus);
        }
        if(textUpdate != "") {
          this.trackers[trackerName].SetUpdate(e, textUpdate);
        }
      }
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
