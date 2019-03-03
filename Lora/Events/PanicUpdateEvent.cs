using System;
using Fraunhofer.Fit.Iot.Lora.Trackers;

namespace Fraunhofer.Fit.Iot.Lora.Events {
  public class PanicUpdateEvent : TrackerUpdateEvent {
    public GpsUpdateEvent Gps { get; private set; }

    public PanicUpdateEvent(Tracker tracker) : base(tracker) {
      this.Gps = new GpsUpdateEvent(tracker.Gps);
    }

    public override String MqttTopic() {
      return "panic/" + base.MqttTopic();
    }

    public override String ToString() {
      return base.ToString() + " -- " + "GPS: " + this.Gps.ToString();
    }
  }
}
