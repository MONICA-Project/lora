﻿using System;
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
    private readonly Object lockReceivePacket = new Object();

    public delegate void UpdateDataEvent(Object sender, DataUpdateEvent e);
    public delegate void UpdatePanicEvent(Object sender, PanicUpdateEvent e);
    public delegate void UpdateStatusEvent(Object sender, StatusUpdateEvent e);
    public event UpdateDataEvent DataUpdate;
    public event UpdatePanicEvent PanicUpdate;
    public event UpdateStatusEvent StatusUpdate;
    public Dictionary<String, Tracker> trackers = new Dictionary<String, Tracker>();
    public LoraController(Dictionary<String, String> settings, Boolean receive = true) {
      this.settings = settings;
      try {
        this.loraconnector = LoraConnector.GetInstance(this.settings);
        _ = this.loraconnector.Begin();
        this.loraconnector.ParseConfig();
        if(receive) {
          this.loraconnector.Receive(0);
        } else {
          this.loraconnector.SetTxPower(17);
          while(true) {
            _ = this.loraconnector.BeginPacket();
            _ = this.loraconnector.Write(System.Text.Encoding.UTF8.GetBytes("TEST TEST TEST"));
            _ = this.loraconnector.EndPacket();
            Console.WriteLine("Send!");
            System.Threading.Thread.Sleep(1000);
          }
        }
        this.loraconnector.Update += this.ReceivePacket;
        _ = this.loraconnector.StartRadio();
        this.loraconnector.AttachUpdateEvent();
      } catch(Exception e) {
        Helper.WriteError("Error while Loading Fraunhofer.Fit.Iot.Lora.LoraController.LoraController: " + e.Message + "\n\n" + e.StackTrace);
        throw;
      }
    }

    private async void ReceivePacket(Object sender, LoraClientEvent e) => await Task.Run(() => {
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

    private async void DataUpdates(Object sender, DataUpdateEvent e) => await Task.Run(() => this.DataUpdate?.Invoke(sender, e));
    #region IDisposable Support
    private Boolean disposedValue = false;

    protected virtual void Dispose(Boolean disposing) {
      Console.WriteLine("Fraunhofer.Fit.Iot.Lora.LoraController.Dispose(" + disposing + ")");
      if(!this.disposedValue) {
        if(disposing) {
          if(this.loraconnector != null) {
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
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }
    #endregion
  }
}
