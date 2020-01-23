using System;

namespace Fraunhofer.Fit.Iot.Lora.Events {
  public class LoraClientEvent : EventArgs {
    public Byte Length { get; protected set; }
    public Byte[] Text { get; protected set; }
    public Double Snr { get; protected set; }
    public Double Packetrssi { get; protected set; }
    public Double Rssi { get; protected set; }
    public DateTime UpdateTime { get; protected set; }

    public LoraClientEvent(Byte Length, Byte[] Text, Double Snr, Byte PacketRssi, Byte Rssi) {
      this.Length = Length;
      this.Text = Text;
      this.Snr = Snr;
      this.Packetrssi = PacketRssi;
      this.Rssi = Rssi;
      this.UpdateTime = DateTime.UtcNow;
    }
    public LoraClientEvent() {

    }
  }
}
