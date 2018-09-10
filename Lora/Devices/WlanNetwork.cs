using System;
using Fraunhofer.Fit.Iot.Lora.Interfaces;

namespace Fraunhofer.Fit.Iot.Lora.Devices {
  public class WlanNetwork : AConnector {

    public WlanNetwork(String str) {
      String[] infos = str.Split(',');
      String mac = infos[0];
      if(mac == "000000000000") {
        this.Success = false;
        return;
      }
      try {
        this.MacAddr = mac[0].ToString() + mac[1].ToString();
        for (Int32 i = 2; i < 12; i = i + 2) {
          this.MacAddr += ":" + mac[i] + mac[i + 1];
        }
      } catch { }
      if (Int32.TryParse(infos[1], out Int32 rssi)) {
        this.Rssi = rssi;
      }
      if (Int32.TryParse(infos[2], out Int32 channel)) {
        this.Channel = channel;
      }
      this.Success = true;
    }

    public String MacAddr { get; private set; }
    public Int32 Rssi { get; private set; }
    public Int32 Channel { get; private set; }
    public Boolean Success { get; private set; }

    public override String ToString() {
      return "MAC: " + this.MacAddr + "(" + this.Channel + ") RSSI:" + this.Rssi;
    }
  }
}
