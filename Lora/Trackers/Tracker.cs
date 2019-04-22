using System;
using System.Text.RegularExpressions;
using Fraunhofer.Fit.Iot.Lora.Events;

namespace Fraunhofer.Fit.Iot.Lora.Trackers {
  public class Tracker {
    private enum ParseType {
      Update,
      Panic
    }

    public delegate void UpdateDataEvent(Object sender, DataUpdateEvent e);
    public delegate void UpdatePanicEvent(Object sender, PanicUpdateEvent e);
    public delegate void UpdateStatusEvent(Object sender, StatusUpdateEvent e);
    public event UpdateDataEvent DataUpdate;
    public event UpdatePanicEvent PanicUpdate;
    public event UpdateStatusEvent StatusUpdate;

    public Int32 Bandwidth { get; private set; }
    public Double BatteryLevel { get; private set; }
    public UInt16 CalculatedCRC { get; private set; }
    public Byte CodingRate { get; private set; }
    public String CRCStatus { get; private set; }
    public Status DeviceStatus { get; private set; }
    public UInt32 Frequency { get; private set; }
    public Int32 FrequencyOffset { get; private set; }
    public GpsInfo Gps { get; private set; }
    public String IpAddress { get; private set; }
    public String Modulation { get; private set; }
    public String Name { get; private set; }
    public Double PacketRssi { get; private set; }
    public Byte RecieverInterface { get; private set; }
    public Byte RecieverRadio { get; private set; }
    public DateTime ReceivedTime { get; private set; }
    public Double Rssi { get; private set; }
    public Double Snr { get; private set; }
    public Double SnrMax { get; private set; }
    public Double SnrMin { get; private set; }
    public Byte SpreadingFactor { get; private set; }
    public UInt32 Time { get; private set; }
    public Int32 Version { get; private set; }
    public Boolean WifiActive { get; private set; }
    public String WifiSsid { get; private set; }

    public enum Status {
      Unknown,
      Startup,
      Powersave,
      Shutdown
    }

    public Tracker() => this.Gps = new GpsInfo();

    #region Private Parsers and Helpers
    private void Parse(Byte[] data, ParseType dataType) {
      if (data.Length == 21) {
        this.Name = GetName(data);
        Single lat = BitConverter.ToSingle(data, 3);
        Single lon = BitConverter.ToSingle(data, 7);
        Single hdop = ((Single)data[11]) / 10;
        Single height = ((Single)BitConverter.ToUInt16(data, 12)) / 10;
        Byte hour = data[14];
        Byte minute = data[15];
        Byte second = data[16];
        Byte day = data[17];
        Byte month = data[18];
        UInt16 year = (UInt16)(data[19] + 2000);
        this.BatteryLevel = (((Single)data[20]) + 230) / 100;
        this.Gps.SetUpdate(lat, lon, height, hdop, hour, minute, second, day, month, year);
        //Console.WriteLine("lat: " + lat + " lon: " + lon + " hdop: " + hdop + " heigt: " + height + " hh:mm:ss: " + hour + ":" + minute + ":" + second + " DD.MM.YY: " + day + "." + month + "." + year + " bat: " + this.BatteryLevel);
        if (dataType == ParseType.Panic) {
          this.PanicUpdate?.Invoke(this, new PanicUpdateEvent(this));
        }
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
      this.Gps.SetUpdate(texts[1]);
      this.DataUpdate?.Invoke(this, new DataUpdateEvent(this));
    }

    private void ParseStatus(String text) {
      String[] texts = text.Split(new String[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
      if(texts.Length != 3) {
        return;
      }
      this.Name = GetName(text, 1);
      String[] infos = texts[2].Split(',');
      if(infos.Length != 7) {
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
      if(Int32.TryParse(infos[6], out Int32 deviceStatus)) {
        if(deviceStatus == 0) {
          this.DeviceStatus = Status.Shutdown;
        } else if(deviceStatus == 1) {
          this.DeviceStatus = Status.Startup;
        } else if(deviceStatus == 2) {
          this.DeviceStatus = Status.Powersave;
        }
      }
      this.StatusUpdate?.Invoke(this, new StatusUpdateEvent(this));
    }

    private void SetUpdate(LoraClientEvent e) {
      if(e is Ic800ALoraClientEvent) {
        Ic800ALoraClientEvent ic = e as Ic800ALoraClientEvent;
        this.Bandwidth = ic.Bandwidth;
        this.CalculatedCRC = ic.Calculatedcrc;
        this.CodingRate = ic.CodingRate;
        this.CRCStatus = ic.CrcStatus;
        this.Frequency = ic.Frequency;
        this.RecieverInterface = ic.Interface;
        this.Modulation = ic.Modulation;
        this.RecieverRadio = ic.Radio;
        this.SnrMax = ic.SnrMax;
        this.SnrMin = ic.SnrMin;
        this.SpreadingFactor = ic.Spreadingfactor;
        this.Time = ic.Time;
      } 
      this.PacketRssi = e.Packetrssi;
      this.Rssi = e.Rssi;
      this.Snr = e.Snr;
      this.ReceivedTime = e.UpdateTime;
    }
    #endregion

    #region External update Methods
    public void SetUpdate(LoraClientEvent e, String data) {
      this.SetUpdate(e);
      this.Parse(data);
    }

    public void SetUpdate(LoraClientEvent e, Byte[] data) {
      this.SetUpdate(e);
      this.Parse(data, ParseType.Update);
    }
    public void SetPanics(LoraClientEvent e, Byte[] data) {
      this.SetUpdate(e);
      this.Parse(data, ParseType.Panic);
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
        //version,ip,ssid,wififlag,battery,offset,statusmode
        if (!Regex.Match(m[2], "^[0-9]+,[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3},[^,]+,[tf],[0-9].[0-9]{2},(-[0-9]+|[0-9]+),[0-9]").Success) {
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
      if(data.Length >= 3) {
        Byte[] ret = new Byte[2];
        for (Int32 i = 0; i < 2; i++) {
          ret[i] = data[i + 1];
        }
        if (ret[1] == 0) {
          return System.Text.Encoding.ASCII.GetString(new Byte[] { ret[0] }).Trim();
        } else {
          return System.Text.Encoding.ASCII.GetString(ret).Trim();
        }
      }
      return "";
    }
    #endregion
  }
}
