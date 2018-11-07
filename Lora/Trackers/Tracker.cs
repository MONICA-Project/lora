using System;
using System.Text.RegularExpressions;
using Fraunhofer.Fit.Iot.Lora.Events;

namespace Fraunhofer.Fit.Iot.Lora.Trackers {
  public class Tracker {
    public delegate void UpdateDataEvent(Object sender, DataUpdateEvent e);
    public delegate void UpdateStatusEvent(Object sender, StatusUpdateEvent e);
    public event UpdateDataEvent DataUpdate;
    public event UpdateStatusEvent StatusUpdate;

    public Double BatteryLevel { get; private set; }
    public Int32 FrequencyOffset { get; private set; }
    public GpsInfo Gps { get; private set; }
    public String IpAddress { get; private set; }
    public String Name { get; private set; }
    public Byte PacketRssi { get; private set; }
    public DateTime Receivedtime { get; private set; }
    public Byte Rssi { get; private set; }
    public Double Snr { get; private set; }
    public Int32 Version { get; private set; }
    public Boolean WifiActive { get; private set; }
    public String WifiSsid { get; private set; }

    public Tracker() { }

    #region Private Parsers and Helpers
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
        this.DataUpdate?.Invoke(this, new DataUpdateEvent(this));
      }
    }

    private void Parse(String text) {
      String[] texts = text.Split(new String[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
      this.Name = GetName(text, 0);
      String[] infos = texts[1].Split(',');
      if (infos.Length >= 6 && Double.TryParse(infos[5], out Double batteryLevel)) {
        this.BatteryLevel = batteryLevel;
      }
      this.Gps = new GpsInfo(texts[1]);
      this.DataUpdate?.Invoke(this, new DataUpdateEvent(this));
    }

    private void ParseStatus(String text) {
      String[] texts = text.Split(new String[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
      if(texts.Length != 3) {
        return;
      }
      this.Name = GetName(text, 1);
      String[] infos = texts[2].Split(',');
      if(infos.Length != 6) {
        return;
      }
      if (Int32.TryParse(infos[0], out Int32 version)) {
        this.Version = version;
      }
      this.IpAddress = infos[1];
      this.WifiSsid = infos[2];
      this.WifiActive = infos[3] == "t";
      if (Double.TryParse(infos[4], out Double batteryLevel)) {
        this.BatteryLevel = batteryLevel;
      }
      if (Int32.TryParse(infos[5], out Int32 freqOffset)) {
        this.FrequencyOffset = freqOffset;
      }
      this.StatusUpdate?.Invoke(this, new StatusUpdateEvent(this));
    }

    private Byte[] From5to4(Byte[] data, Int32 start) {
      if (data.Length < start + 6) {
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

    private void SetUpdate(LoraClientEvent e) {
      this.PacketRssi = e.Packetrssi;
      this.Rssi = e.Rssi;
      this.Snr = e.Snr;
      this.Receivedtime = e.UpdateTime;
    }
    #endregion

    #region External update Methods
    public void SetUpdate(LoraClientEvent e, String data) {
      this.SetUpdate(e);
      this.Parse(data);
    }

    public void SetUpdate(LoraClientEvent e, Byte[] data) {
      this.SetUpdate(e);
      this.Parse(data);
    }

    public void SetStatus(LoraClientEvent e, String textStatus) {
      this.SetUpdate(e);
      this.ParseStatus(textStatus);
    }
    #endregion

    #region Static Functions
    public static Boolean CheckPacket(String message) {
      String[] m;
      if (message.Contains("\r\n")) {
        m = message.Split(new String[] { "\r\n" }, StringSplitOptions.None);
      } else if (message.Contains("\n")) {
        m = message.Split(new String[] { "\n" }, StringSplitOptions.None);
      } else {
        return false;
      }
      if (m.Length == 2) { //Normal Data Packet
        if (m[0] == "") { //Name should not be empty
          return false;
        }
        if (!Regex.Match(m[1], "[0-9]+.[0-9]{5,10},[0-9]+.[0-9]{5,10},[0-9]{6},[0-9]+.[0-9]{2},[0-9]+.[0-9],[0-9].[0-9]{2}").Success) { //lon,lat,hhmmss,hdop,height,bat not match
          return false;
        }
        return true;
      }
      if (m.Length == 3) { //Debug Packet
        if (m[0] != "deb") { //first must be "deb"
          return false;
        }
        if (m[1] == "") { //Name should not be empty
          return false;
        }
        //version,ip,ssid,wififlag,battery,offset
        if (!Regex.Match(m[2], "^[0-9]+,[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3},[^,]+,[tf],[0-9].[0-9]{2},(-[0-9]+|[0-9]+)").Success) {
          return false;
        }
        return true;
      }
      return false;
    }

    public static String GetName(String message, Int32 index) {
      if (message.Contains("\r\n")) {
        return message.Split(new String[] { "\r\n" }, StringSplitOptions.None)[index].Trim();
      } else if (message.Contains("\n")) {
        return message.Split(new String[] { "\n" }, StringSplitOptions.None)[index].Trim();
      } else {
        return "";
      }
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
    #endregion
  }
}
