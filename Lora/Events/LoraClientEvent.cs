using System;

namespace Fraunhofer.Fit.Iot.Lora.Events {
  public class LoraClientEvent : EventArgs {
    public Byte Length { get; }
    public String Text { get; }
    public Double Snr { get; }
    public Byte Packetrssi { get; }
    public Byte Rssi { get; }
    public DateTime UpdateTime { get; }

    public LoraClientEvent(Byte Length, String Text, Double Snr, Byte PacketRssi, Byte Rssi) {
      this.Length = Length;
      this.Text = Text;
      this.Snr = Snr;
      this.Packetrssi = PacketRssi;
      this.Rssi = Rssi;
      this.UpdateTime = DateTime.Now;
    }
  }
}
