using System;
using Fraunhofer.Fit.Iot.Lora.Trackers;

namespace Fraunhofer.Fit.Iot.Lora.Events {
  public class TrackerUpdateEvent : UpdateEventHelper {
    public String Name { get; private set; }
    public Byte PacketRssi { get; private set; }
    public Byte Rssi { get; private set; }
    public Double Snr { get; private set; }
    public DateTime Receivedtime { get; private set; }
    public Double BatteryLevel { get; private set; }

    public TrackerUpdateEvent(Tracker tracker) {
      this.PacketRssi = tracker.PacketRssi;
      this.Rssi = tracker.Rssi;
      this.Snr = tracker.Snr;
      this.Receivedtime = tracker.Receivedtime;
      this.Name = tracker.Name;
      this.BatteryLevel = tracker.BatteryLevel;
    }

    public override String MqttTopic() {
      return base.MqttTopic() + this.Name;
    }

    public override String ToString() {
      return this.Name + " -- " + "Packet: PRssi: " + this.PacketRssi + " Rssi: " + this.Rssi + " SNR: " + this.Snr + " Time: " + this.Receivedtime.ToString() + " Battery: " + this.BatteryLevel;
    }
  }
}
