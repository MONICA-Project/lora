using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using BlubbFish.Utils;

using Fraunhofer.Fit.Iot.Lora.Events;

namespace Fraunhofer.Fit.Iot.Lora.lib {
  public abstract class LoraBoard : SpiCom, IDisposable {
    protected Dictionary<String, String> config;
    public delegate void RecieveUpdate(Object sender, RecievedData e);
    public delegate void TransmittedUpdate(Object sender, TransmittedData e);

    public Byte Interfaces { get; protected set; }

    #region Constructor and deconstructor
    protected LoraBoard(Dictionary<String, String> settings) => this.config = settings;
    public static LoraBoard GetInstance(Dictionary<String, String> settings) {
      if(settings.Count == 0) {
        throw new ArgumentException("Missing argument for [lora] in settingsfile");
      }
      String object_sensor = "Fraunhofer.Fit.Iot.Lora.lib." + settings["type"].ToUpperLower() + "." + settings["type"].ToUpperLower();
      try {
        Type t = Type.GetType(object_sensor, true);
        return (LoraBoard)t.GetConstructor(new Type[] { typeof(Dictionary<String, String>) }).Invoke(new Object[] { settings });
      } catch(TypeLoadException) {
        throw new ArgumentException("Configuration: " + settings["type"] + " is not a LoraBoard");
      } catch(FileNotFoundException) {
        throw new Exception("Driver " + object_sensor + " could not load!");
      }

    }
    public abstract void Dispose();
    #endregion

    #region Start
    public abstract void Begin();
    #endregion

    #region Send and Recieve
    public abstract void Send(Byte[] data, Byte @interface);
    public abstract void StartEventRecieving();
    #endregion

    #region Events
    public event RecieveUpdate Recieved;
    public event TransmittedUpdate Transmitted;
    #endregion

    #region Protected methods for child classes
    protected void Debug(String text) => Console.WriteLine(text);
    protected async void RaiseRecieveEvent(RecievedData obj) => await Task.Run(() => this.Recieved?.Invoke(this, obj));
    protected async void RaiseTransmittedEvent(TransmittedData obj) => await Task.Run(() => this.Transmitted?.Invoke(this, obj));
    #endregion
  }
}
