using System;
using System.Collections.Generic;
using Fraunhofer.Fit.Iot.Lora.Interfaces;

namespace Fraunhofer.Fit.Iot.Lora.Devices {
  public class GpsInfo : AConnector {

    public GpsInfo(String str) {
      String[] infos = str.Split(',');

      if (Double.TryParse(infos[0], out Double breitengrad)) {
        this.Latitude = breitengrad;
      }
      if (Double.TryParse(infos[1], out Double laengengrad)) {
        this.Longitude = laengengrad;
      }
      String d = "01.01.2000 " + infos[2][0] + infos[2][1] + ":" + infos[2][2] + infos[2][3] + ":" + infos[2][4] + infos[2][5];
      if (DateTime.TryParse(d, out DateTime dv)) {
        this.Time = dv.TimeOfDay;
      }
      if (Double.TryParse(infos[3], out Double hdop)) {
        this.Hdop = hdop;
      }
      this.Fix = !(Math.Abs(this.Latitude) < 0.000001 && Math.Abs(this.Longitude) < 0.000001); //Check for 0 lat and long

    }

    public GpsInfo(Single lat, Single lon, Byte hour, Byte minute, Byte second, Single hdop, Boolean fix) {
      this.Latitude = lat;
      this.Longitude = lon;
      String d = "01.01.2000 " + hour.ToString() + ":" + minute.ToString() + ":" + second.ToString();
      if (DateTime.TryParse(d, out DateTime dv)) {
        this.Time = dv.TimeOfDay;
      }
      this.Hdop = hdop;
      this.Fix = fix;
    }

    public Double Latitude { get; private set; }
    public Double Longitude { get; private set; }
    public TimeSpan Time { get; private set; }
    public Double Hdop { get; private set; }
    public Boolean Fix { get; private set; }

    public override String ToString() {
      return "Lat: " + this.Latitude + " Lon: " + this.Longitude + "\nTime: " + this.Time + " HDOP: " + this.Hdop + " Fix: " + this.Fix;
    }
  }
}
