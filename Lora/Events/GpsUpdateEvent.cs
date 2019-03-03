using System;
using Fraunhofer.Fit.Iot.Lora.Trackers;

namespace Fraunhofer.Fit.Iot.Lora.Events {
  public class GpsUpdateEvent : UpdateEventHelper {
    public Boolean Fix { get; private set; }
    public Double Hdop { get; private set; }
    public Double Height { get; private set; }
    public Double Latitude { get; private set; }
    public Double Longitude { get; private set; }
    public Double LastLatitude { get; private set; }
    public Double LastLongitude { get; private set; }
    public DateTime Time { get; private set; }

    public GpsUpdateEvent(GpsInfo gps) {
      this.Fix = gps.Fix;
      this.Hdop = gps.Hdop;
      this.Height = gps.Height;
      this.Latitude = gps.Latitude;
      this.Longitude = gps.Longitude;
      this.Time = gps.Time;
      this.LastLatitude = gps.LastLatitude;
      this.LastLongitude = gps.LastLongitude;
    }

    public override String ToString() {
      return "Lat: " + this.Latitude + " [" + this.LastLatitude + "] Lon: " + this.Longitude + " [" + this.LastLongitude + "] Height: " + this.Height + " -- Time: " + this.Time + " HDOP: " + this.Hdop + " Fix: " + this.Fix;
    }
  }
}
