using System;

namespace Fraunhofer.Fit.Iot.Lora.lib.Dragino {
  public partial class Draginolora {
    enum Modes : Byte {
      SLEEP = 0x00,
      STDBY = 0x01,
      TX = 0x03,
      RX_CONTINOUS = 0x05,
      RX_SINGLE = 0x06,
      LONG_RANGE_MODE = 0x80
    };

    public enum Pa : Byte {
      BOOST = 0x80,
      OUTPUT_RFO_PIN = 0,
      OUTPUT_PA_BOOST_PIN = 1
    };

    enum Irq : Byte {
      TX_DONE_MASK = 0x08,
      PAYLOAD_CRC_ERROR_MASK = 0x20,
      RX_DONE_MASK = 0x40
    }
  }
}
