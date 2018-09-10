using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Fraunhofer.Fit.Iot.Lora.Events;
using Fraunhofer.Fit.Iot.Lora.Interfaces;

namespace Fraunhofer.Fit.Iot.Lora.Devices {
  public class LoraClient : AConnector {
    public delegate void DataUpdate(Object sender, DeviceUpdateEvent e);
    public event DataUpdate Update;

    public Byte PacketRssi { get; private set; }
    public Byte Rssi { get; private set; }
    public Double Snr { get; private set; }
    public DateTime Receivedtime { get; private set; }
    public String Name { get; private set; }
    public ReadOnlyDictionary<String, WlanNetwork> Wifi { get; private set; }
    public GpsInfo Gps { get; private set; }
    public Int32 BatteryLevel { get; private set; }

    public LoraClient(LoraClientEvent e) {
      this.PacketRssi = e.Packetrssi;
      this.Rssi = e.Rssi;
      this.Snr = e.Snr;
      this.Receivedtime = e.UpdateTime;
    }

    public LoraClient(LoraClientEvent e, String data) : this(e) {
      this.Parse(data);
    }

    public LoraClient(LoraClientEvent e, Byte[] data) : this(e) {
      this.Parse(data);
    }

    private void Parse(Byte[] data) {
      if (data.Length == 28) {
        this.Name = GetName(data);
        Single lat = BitConverter.ToSingle(this.From5to4(data, 9), 0);
        Single lon = BitConverter.ToSingle(this.From5to4(data, 14), 0);
        Byte hour = data[19];
        Byte minute = data[20];
        Byte second = data[21];
        Single hdop = BitConverter.ToSingle(this.From5to4(data, 22), 0);
        this.BatteryLevel = data[27];
        Boolean fix = (lat != 0 && lon != 0);
        this.Gps = new GpsInfo(lat, lon, hour, minute, second, hdop, fix);
        this.Wifi = new ReadOnlyDictionary<String, WlanNetwork>(new Dictionary<String, WlanNetwork>());
        this.Update?.Invoke(this, new DeviceUpdateEvent(this, DateTime.Now, this));
      }
    }

    private Byte[] From5to4(Byte[] data, Int32 start) {
      if(data.Length < start + 6) {
        return new Byte[] { 0, 0, 0, 0 };
      }
      UInt64 t = 0;
      t = data[start + 4];
      t += ((UInt64)data[start + 3] << 7);
      t += ((UInt64)data[start + 2] << 14);
      t += ((UInt64)data[start + 1] << 21);
      t += ((UInt64)data[start + 0] << 28);
      return BitConverter.GetBytes(t);
    }

    private void Parse(String text) {
      String[] texts = text.Split(new String[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
      this.Name = GetName(text);
      Dictionary<String, WlanNetwork> wifis = new Dictionary<String, WlanNetwork>();
      for(Int16 i=1;i<texts.Length-1;i++) {
        WlanNetwork w = new WlanNetwork(texts[i]);
        if(w.Success) {
          String id = w.MacAddr;
          wifis.Add(id, w);
        }
      }
      this.Wifi = new ReadOnlyDictionary<String, WlanNetwork>(wifis);
      String[] infos = texts[texts.Length - 1].Split(',');
      if (infos.Length >= 5 && Int32.TryParse(infos[4], out Int32 batteryLevel)) {
        this.BatteryLevel = batteryLevel;
      }
      this.Gps = new GpsInfo(texts[texts.Length - 1]);
      this.Update?.Invoke(this, new DeviceUpdateEvent(this, DateTime.Now, this));
    }

    public override String ToString() {
      String ret = this.Name + "\n" + "Packet: PRssi: "+this.PacketRssi+" Rssi: "+this.Rssi+" SNR: "+this.Snr+" Time: "+this.Receivedtime.ToString() + " Battery: " + this.BatteryLevel+"\n";
      ret += "WLAN:\n";
      foreach (KeyValuePair<String, WlanNetwork> item in this.Wifi) {
        ret += item.ToString() + "\n";
      }
      ret += "GPS:\n" + this.Gps.ToString();
      return ret;
    }

    private void SetUpdate(LoraClientEvent e) {
      this.PacketRssi = e.Packetrssi;
      this.Rssi = e.Rssi;
      this.Snr = e.Snr;
      this.Receivedtime = e.UpdateTime;
    }

    public void SetUpdate(LoraClientEvent e, String data) {
      this.SetUpdate(e);
      this.Parse(data);
    }

    public void SetUpdate(LoraClientEvent e, Byte[] data) {
      this.SetUpdate(e);
      this.Parse(data);
    }

    public static Boolean CheckPacket(String message)
    {
      String[] m;
      if (message.Contains("\r\n")) {
        m = message.Split(new String[] { "\r\n" }, StringSplitOptions.None);
      } else if (message.Contains("\n")) {
        m = message.Split(new String[] { "\n" }, StringSplitOptions.None);
      } else {
        return false;
      }
      if (m.Length == 5) {
        //Console.WriteLine(m[0]);
        if(m[0] == "") {
          //Console.WriteLine("Name Match Fail");
          return false;
        }
        for (Int32 i = 1; i < 4; i++) {
          //Console.WriteLine(m[i]);
          if (!Regex.Match(m[i], "[A-F0-9]{12},[-0-9]+,[0-9]+").Success) {
            return false;
          }
        }
        if (!Regex.Match(m[4], "[0-9]+.[0-9]{5,10},[0-9]+.[0-9]{5,10},[0-9]{6},[0-9]+.[0-9]{2},[0-9]+").Success) {
          return false;
        }
        return true;
      }
      if (m.Length == 2) {
        //Console.WriteLine("L2");
        if (m[0] == "") {
          //Console.WriteLine("Name Match Fail");
          return false;
        }
        if (!Regex.Match(m[1], "[0-9]+.[0-9]{5,10},[0-9]+.[0-9]{5,10},[0-9]{6},[0-9]+.[0-9]{2},[0-9]+").Success) {
          //Console.WriteLine("GPS Match Fail");
          return false;
        }
        return true;
      }
      return false;
    }

    public static String GetName(String message) {
      return message.Split(new String[] { "\r\n" }, StringSplitOptions.None)[0].Trim();
    }

    public static String GetName(Byte[] data) {
      if(data.Length >= 9) {
        Byte[] ret = new Byte[8];
        for (Int32 i = 0; i < 8; i++) {
          ret[i] = data[i + 1];
        }
        return System.Text.Encoding.ASCII.GetString(ret).Trim();
      }
      return "";
    }

    public override String MqttTopic() {
      return base.MqttTopic() + "-" + this.Name;
    }
  }
}
