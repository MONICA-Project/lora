using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fraunhofer.Fit.Iot.Lora.Events {
  public class LoraClientEvent : EventArgs {

    public LoraClientEvent() {
    }

    public LoraClientEvent(Byte Length, String Text, Double Snr, Byte PacketRssi, Byte Rssi) {
      this.Length = Length;
      this.Text = Text;
      this.Snr = Snr;
      this.Packetrssi = PacketRssi;
      this.Rssi = Rssi;
      this.UpdateTime = DateTime.Now;
    }

    public Byte Length { get; }
    public String Text { get; }
    public Double Snr { get; }
    public Byte Packetrssi { get; }
    public Byte Rssi { get; }
    public DateTime UpdateTime { get; }
  }
  public class DeviceUpdateEvent : EventArgs {

    public DeviceUpdateEvent() {
    }

    public DeviceUpdateEvent(Object value, DateTime time, Object parent) {
      this.GetValue = value;
      this.UpdateTime = time;
      this.Parent = parent;
    }

    public Object GetValue { get; }
    public DateTime UpdateTime { get; }
    public Object Parent { get; private set; }
  }
}
