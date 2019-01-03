using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fraunhofer.Fit.Iot.Lora.Events;

namespace Fraunhofer.Fit.Iot.Lora.lib {
  public abstract class LoraConnector {
    public delegate void DataUpdate(Object sender, LoraClientEvent e);
    public event DataUpdate Update;

    public abstract void OnReceive();
    public abstract Boolean Begin(Int64 freq);
    public abstract void SetSignalBandwith(Int64 sbw);
    public abstract void SetSpreadingFactor(Byte sf);
    public abstract void SetCodingRate4(Byte denominator);
    public abstract void DisableCrc();
    public abstract void EnableCrc();
    public abstract void Receive(Byte size);
    public abstract void SetTxPower(Int32 level, Pa outputPin = Pa.OUTPUT_PA_BOOST_PIN);
    public abstract Boolean BeginPacket(Boolean implictHeader = false);
    public abstract Byte Write(Byte[] buffer);
    public abstract Boolean EndPacket(Boolean async = false);
    public abstract void End();
    public abstract void Dispose();
  }
}
