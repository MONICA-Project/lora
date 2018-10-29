using System;
using Fraunhofer.Fit.Iot.Lora.Trackers;

namespace Fraunhofer.Fit.Iot.Lora.Events {
  public class GpsUpdateEvent : UpdateEventHelper {
    public Boolean Fix { get; private set; }
    public Double Hdop { get; private set; }
    public Double Height { get; private set; }
    public Double Latitude { get; private set; }
    public Double Longitude { get; private set; }
    public TimeSpan Time { get; private set; }

    public GpsUpdateEvent(GpsInfo gps) {
      this.Fix = gps.Fix;
      this.Hdop = gps.Hdop;
      this.Height = gps.Height;
      this.Latitude = gps.Latitude;
      this.Longitude = gps.Longitude;
      this.Time = gps.Time;
    }

    public override String ToString() {
      return "Lat: " + this.Latitude + " Lon: " + this.Longitude + " Height: " + this.Height + " -- Time: " + this.Time + " HDOP: " + this.Hdop + " Fix: " + this.Fix;
    }
  }
}
