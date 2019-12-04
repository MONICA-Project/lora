using System;
using System.Collections.Generic;
using BlubbFish.Utils;
using Fraunhofer.Fit.Iot.Lora.Events;

namespace Fraunhofer.Fit.Iot.Lora.lib {
  public abstract class LoraConnector {
    protected Dictionary<String, String> config;

    public delegate void DataUpdate(Object sender, LoraClientEvent e);
    public event DataUpdate Update;

    protected LoraConnector(Dictionary<String, String> settings) => this.config = settings;

    public static LoraConnector GetInstance(Dictionary<String, String> settings) {
      if (settings.Count == 0) {
        return null;
      }
      String object_sensor = "Fraunhofer.Fit.Iot.Lora.lib." + settings["type"].ToUpperLower();
      try {
        Type t = Type.GetType(object_sensor, true);
        return (LoraConnector)t.GetConstructor(new Type[] { typeof(Dictionary<String, String>) }).Invoke(new Object[] { settings });
      } catch (TypeLoadException) {
        Console.Error.WriteLine("Configuration: " + settings["type"] + " is not a LoraConnector");
        return null;
      } catch (System.IO.FileNotFoundException) {
        Console.Error.WriteLine("Driver " + object_sensor + " could not load!");
        return null;
      }
      
    }

    protected void RaiseUpdateEvent(LoraClientEvent data) => this.Update?.Invoke(this, data);

    protected Boolean HasAttachedUpdateEvent() => this.Update != null;

    #region Constructor
    public abstract Boolean Begin();
    public abstract void End();
    public abstract void Dispose();
    public abstract Boolean StartRadio();
    public abstract void ParseConfig();
    #endregion

    #region Packets, Read, Write
    public abstract Boolean BeginPacket(Boolean implictHeader = false);
    public abstract Boolean EndPacket(Boolean async = false);
    public abstract Byte Write(Byte[] buffer);
    public abstract void Receive(Byte size);
    #endregion

    #region Powserusage
    public abstract void SetTxPower(Int32 level);
    #endregion

    #region Hardware IO
    public abstract void AttachUpdateEvent();
    #endregion
  }
}
