﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fraunhofer.Fit.Iot.Lora.lib;

namespace Fraunhofer.Fit.Iot.Lora {
  public class LoraController {
    public LoraController() {
      Int64 freq = 868100000;
      Int32 sf = 7;
      LoraConnector l = new LoraConnector(freq);
      //l.EnableCrc();
      l.Receive(0);
      l.Update += this.ReceivePacket;
      Console.WriteLine("Listening at SF"+ sf + " on "+((Double)freq / 1000000) +" Mhz.");
      Console.WriteLine("------------------");
      l.OnReceive();
      while (true) {
        System.Threading.Thread.Sleep(1);
      }
    }

    private void ReceivePacket(Object sender, Events.DeviceUpdateEvent e) {
      Console.WriteLine("Packet RSSI: "+e.Packetrssi+", RSSI: "+e.Rssi+", SNR: "+e.Snr+", Length: "+e.Length);
      Console.WriteLine("Payload: "+e.Text);
    }
  }
}
