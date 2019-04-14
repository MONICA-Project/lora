using System;
using Fraunhofer.Fit.Iot.Lora.lib;
using Fraunhofer.Fit.Iot.Lora.Trackers;

namespace Fraunhofer.Fit.Iot.Lora.Events {
  public class TrackerUpdateEvent : UpdateEventHelper {
    public String Name { get; private set; }
    public Double PacketRssi { get; private set; }
    public Double Rssi { get; private set; }
    public Double Snr { get; private set; }
    public DateTime Receivedtime { get; private set; }
    public Double BatteryLevel { get; private set; }
    public Byte Recieverradio { get; private set; }
    public Byte Recieverinterface { get; private set; }
    public UInt32 Frequency { get; private set; }
    public Int32 Bandwidth { get; private set; }
    public Byte Codingrate { get; private set; }
    public Byte Spreadingfactor { get; private set; }
    public String Crcstatus { get; private set; }
    public UInt16 Calculatedcrc { get; private set; }
    public Double Snrmax { get; private set; }
    public Double Snrmin { get; private set; }
    public UInt32 Time { get; private set; }
    public String Host => Environment.MachineName;

    public TrackerUpdateEvent(Tracker tracker) {
      this.PacketRssi = tracker.PacketRssi;
      this.Rssi = tracker.Rssi;
      this.Snr = tracker.Snr;
      this.Receivedtime = tracker.ReceivedTime;
      this.Name = tracker.Name;
      this.BatteryLevel = tracker.BatteryLevel;
      this.Recieverradio = tracker.RecieverRadio;
      this.Recieverinterface = tracker.RecieverInterface;
      this.Frequency = tracker.Frequency;
      this.Bandwidth = tracker.Bandwidth;
      this.Codingrate = tracker.CodingRate;
      this.Spreadingfactor = tracker.SpreadingFactor;
      this.Crcstatus = tracker.CRCStatus;
      this.Calculatedcrc = tracker.CalculatedCRC;
      this.Snrmax = tracker.SnrMax;
      this.Snrmin = tracker.SnrMin;
      this.Time = tracker.Time;
    }

    public override String MqttTopic() => base.MqttTopic() + this.Name;

    public override String ToString() => this.Name + " -- " + "Packet: PRssi: " + this.PacketRssi + " Rssi: " + this.Rssi + " SNR: (" + this.Snr + "/" + this.Snrmin + "/" + this.Snrmax + ") Time: " + this.Receivedtime.ToString() +
        " Battery: " + this.BatteryLevel + " Radio: " + this.Recieverradio + " Interface: " + this.Recieverinterface + " Freq: " + this.Frequency + " BW: " + this.Bandwidth +
        " CR: " + this.Codingrate + " SF: " + this.Spreadingfactor + " CRC: " + this.Crcstatus + "(0x" + this.Calculatedcrc.ToString("X4") + ") Time: " + this.Time;
  }
}
