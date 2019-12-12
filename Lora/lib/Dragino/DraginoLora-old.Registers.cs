using System;

namespace Fraunhofer.Fit.Iot.Lora.lib.Dragino {
  public partial class Draginolora_old {
    enum Registers : Byte {
      FIFO = 0x00,
      OP_MODE = 0x01,
      FRF_MSB = 0x06,
      FRF_MID = 0x07,
      FRF_LSB = 0x08,
      PA_CONFIG = 0x09,
      PRE_PA_RAMP = 0x0A,
      OCP = 0x0B,
      LNA = 0x0C,
      FIFO_ADDR_PTR = 0x0D,
      FIFO_TX_BASE_ADDR = 0x0E,
      FIFO_RX_BASE_ADDR = 0x0F,
      FIFO_RX_CURRENT_ADDR = 0x10,
      IRQ_FLAGS = 0x12,
      RX_NB_BYTES = 0x13,
      PKT_SNR_VALUE = 0x19,
      PKT_RSSI_VALUE = 0x1A,
      RSSI_VALUE = 0x1B,
      MODEM_CONFIG_1 = 0x1D,
      MODEM_CONFIG_2 = 0x1E,
      PREAMBLE_MSB = 0x20,
      PREAMBLE_LSB = 0x21,
      PAYLOAD_LENGTH = 0x22,
      MODEM_CONFIG_3 = 0x26,
      FREQ_ERROR_MSB = 0x28,
      FREQ_ERROR_MID = 0x29,
      FREQ_ERROR_LSB = 0x2A,
      RSSI_WIDEBAND = 0x2C,
      DETECTION_OPTIMIZE = 0x31,
      INVERTIQ = 0x33,
      DEDECTION_THRESHOLD = 0x37,
      SYNC_WORD = 0x39,
      INVERTIQ2 = 0x3B,
      DIO_MAPPING_1 = 0x40,
      VERSION = 0x42,
      PA_DAC = 0x4D
    };
  }
}
