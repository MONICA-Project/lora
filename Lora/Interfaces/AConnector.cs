using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BlubbFish.Utils.IoT.Bots;
using BlubbFish.Utils.IoT.Interfaces;
using LitJson;

namespace Fraunhofer.Fit.Iot.Lora.Interfaces {
  public class AConnector : IMqtt {
    #region IMqtt
    public String ToJson() {
      return JsonMapper.ToJson(this.ToDictionary());
    }

    public virtual String MqttTopic() {
      return "Lora";
    }
    #endregion

    public virtual Dictionary<String, Object> ToDictionary() {
      Dictionary<String, Object> dictionary = new Dictionary<String, Object>();
      foreach (PropertyInfo item in this.GetType().GetProperties()) {
        if (item.CanRead && item.GetValue(this) != null) {
          if (item.GetValue(this).GetType().GetMethod("ToDictionary") != null) {
            dictionary.Add(item.Name, item.GetValue(this).GetType().GetMethod("ToDictionary").Invoke(item.GetValue(this), null));
          } else if (item.GetValue(this).GetType().HasInterface(typeof(IDictionary))) {
            Dictionary<String, Object> subdict = new Dictionary<String, Object>();
            foreach (DictionaryEntry subitem in (IDictionary)item.GetValue(this)) {
              if (subitem.Value.GetType().GetMethod("ToDictionary") != null) {
                subdict.Add(subitem.Key.ToString(), subitem.Value.GetType().GetMethod("ToDictionary").Invoke(subitem.Value, null));
              }
            }
            dictionary.Add(item.Name, subdict);
          } else if (item.GetValue(this).GetType().BaseType == typeof(Enum)) {
            dictionary.Add(item.Name, Helper.GetEnumDescription((Enum)item.GetValue(this)));
          } else {
            dictionary.Add(item.Name, item.GetValue(this));
          }
        }
      }
      return dictionary;
    }
  }
}
