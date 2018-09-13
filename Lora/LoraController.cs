using System;
using System.Collections.Generic;
using BlubbFish.Utils.IoT.Bots;
using Fraunhofer.Fit.Iot.Lora.Devices;
using Fraunhofer.Fit.Iot.Lora.Events;
using Fraunhofer.Fit.Iot.Lora.lib;

// Hope RFM96
// http://www.hoperf.com/upload/rf/RFM95_96_97_98W.pdf
// The RFM97 offers bandwidth options ranging from 7.8 kHz to 500 kHz with spreading factors ranging from 6 to 12, and covering all available frequency bands.

namespace Fraunhofer.Fit.Iot.Lora {
  public class LoraController : IDisposable {
    private LoraConnector loraconnector;
    private readonly Dictionary<String, String> settings;

    public delegate void DataUpdate(Object sender, DeviceUpdateEvent e);
    public event DataUpdate Update;
    public Dictionary<String, LoraClient> devices = new Dictionary<String, LoraClient>();
    public LoraController(Dictionary<String, String> settings) {
      Console.WriteLine("LoraController.LoraController()");
      this.settings = settings;
      try {
        this.CreateLoraController(true);
        this.loraconnector.Update += this.ReceivePacket;
        this.loraconnector.OnReceive();
      } catch { }
    }

    private void CreateLoraController(Boolean rescieve = true) {
      if(!this.settings.ContainsKey("frequency") || !this.settings.ContainsKey("spreadingfactor") | !this.settings.ContainsKey("signalbandwith") || !this.settings.ContainsKey("codingrate")) {
        Helper.WriteError("Not all Settings set!: [lora]\nfrequency=868100000\nspreadingfactor=8\nsignalbandwith=125000\ncodingrate=6 missing");
        return;
      }
      this.loraconnector = new LoraConnector(Unosquare.RaspberryIO.Pi.Gpio.Pin06, Unosquare.RaspberryIO.Pi.Gpio.Pin07, Unosquare.RaspberryIO.Pi.Gpio.Pin00);
      this.loraconnector.Begin(Int64.Parse(this.settings["frequency"])); //868125100
      this.loraconnector.SetSignalBandwith(Int64.Parse(this.settings["signalbandwith"])); //125000
      this.loraconnector.SetSpreadingFactor(Byte.Parse(this.settings["spreadingfactor"])); //8 - 11
      this.loraconnector.SetCodingRate4(Byte.Parse(this.settings["codingrate"]));
      //this.loraconnector.EnableCrc();
      this.loraconnector.DisableCrc();
      if (rescieve) {
        this.loraconnector.Receive(0);
      } else {
        this.loraconnector.SetTxPower(17);
        while (true) {
          this.loraconnector.BeginPacket();
          this.loraconnector.Write(System.Text.Encoding.UTF8.GetBytes("TESTTESTTESTTESTTESTTESTTESTAsdasdasdh ahsdk jahsdkdja shdas"));
          this.loraconnector.EndPacket();
          Console.WriteLine("Send!");
          System.Threading.Thread.Sleep(1000);
        }
      }
    }

    private void ReceivePacket(Object sender, LoraClientEvent e) {
      Console.WriteLine("Fraunhofer.Fit.Iot.Lora.LoraController.ReceivePacket: " + e.Text.Length.ToString());
      if (e.Text.StartsWith("b") && e.Text.Length == 27) {
        Byte[] data = System.Text.Encoding.ASCII.GetBytes(e.Text);
        Console.WriteLine("Fraunhofer.Fit.Iot.Lora.LoraController.ReceivePacket: |" + BitConverter.ToString(data).Replace("-", " ") + "| PRSSI: " + e.Packetrssi + " RSSI:" + e.Rssi + " SNR:" + e.Snr);
        String deviceName = LoraClient.GetName(data);
        if (this.devices.ContainsKey(deviceName)) {
          this.devices[deviceName].SetUpdate(e, data);
        } else {
          this.devices.Add(deviceName, new LoraClient());
          this.devices[deviceName].Update += this.AllUpdate;
          this.devices[deviceName].SetUpdate(e, data);
        }

      } else {
        Console.WriteLine("Fraunhofer.Fit.Iot.Lora.LoraController.ReceivePacket: |" + e.Text + "| PRSSI: " + e.Packetrssi + " RSSI:" + e.Rssi + " SNR:" + e.Snr);
        if (LoraClient.CheckPacket(e.Text)) {
          String deviceName = LoraClient.GetName(e.Text);
          if (this.devices.ContainsKey(deviceName)) {
            this.devices[deviceName].SetUpdate(e, e.Text);
          } else {
            this.devices.Add(deviceName, new LoraClient());
            this.devices[deviceName].Update += this.AllUpdate;
            this.devices[deviceName].SetUpdate(e, e.Text);
          }
        } else {
          Console.WriteLine("Fraunhofer.Fit.Iot.Lora.LoraController.ReceivePacket: Packet not Match!");
        }
      }
    }

    private void AllUpdate(Object sender, DeviceUpdateEvent e) {
      this.Update?.Invoke(sender, e);
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
