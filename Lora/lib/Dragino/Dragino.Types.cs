using System;

namespace Fraunhofer.Fit.Iot.Lora.lib.Dragino {
  public partial class Dragino {
    private struct RadioLibTypes {
      public static Byte RADIOLIB_USE_SPI => 0x00;
      public static Byte RADIOLIB_INT_0 => 0x01;
      public static Byte RADIOLIB_INT_BOTH => 0x03;
    }

    private struct Constances {
      public static Byte SX1278_CHIP_VERSION => 0x12;
      public static Byte SX127X_FSK_OOK => 0b00000000;
      public static Byte SX127X_LORA => 0b10000000;
      public static Byte SX127X_SLEEP => 0b00000000;
      public static Byte SX127X_STANDBY => 0b00000001;
      public static Byte SX127X_SYNC_WORD => 0x12;
      public static Byte SX127X_OCP_OFF => 0b00000000;
      public static Byte SX127X_OCP_ON => 0b00100000;
      public static Byte SX127X_HOP_PERIOD_OFF => 0b00000000;
      public static Byte SX127X_DIV_EXPONENT => 19;
      public static Byte SX127X_CRYSTAL_FREQ => 32;
      public static Byte SX1278_LOW_DATA_RATE_OPT_OFF => 0b00000000;
      public static Byte SX1278_LOW_DATA_RATE_OPT_ON => 0b00001000;
      public static Byte SX1278_HEADER_IMPL_MODE => 0b00000001;
      public static Byte SX1278_HEADER_EXPL_MODE => 0b00000000;
      public static Byte SX127X_TX_MODE_SINGLE => 0b00000000;
      public static Byte SX1278_RX_CRC_MODE_ON => 0b00000100;
      public static Byte SX127X_DETECT_OPTIMIZE_SF_6 => 0b00000101;
      public static Byte SX127X_DETECT_OPTIMIZE_SF_7_12 => 0b00000011;
      public static Byte SX127X_DETECTION_THRESHOLD_SF_6 => 0b00001100;
      public static Byte SX127X_DETECTION_THRESHOLD_SF_7_12 => 0b00001010;
      public static Byte SX127X_PA_SELECT_RFO => 0b00000000;
      public static Byte SX1278_LOW_POWER => 0b00100000;
      public static Byte SX127X_PA_BOOST_OFF => 0b00000100;
      public static Byte SX127X_PA_SELECT_BOOST => 0b10000000;
      public static Byte SX1278_MAX_POWER => 0b01110000;
      public static Byte SX127X_PA_BOOST_ON => 0b00000111;
      public static Byte SX1278_AGC_AUTO_OFF => 0b00000000;
      public static Byte SX1278_AGC_AUTO_ON => 0b00000100;
      public static Byte SX127X_LNA_BOOST_ON => 0b00000011;
      public static Byte SX127X_RXCONTINUOUS => 0b00000101;
      public static Byte SX127X_RX => 0b00000101;
      public static Byte SX127X_DIO0_RX_DONE => 0b00000000;
      public static Byte SX127X_DIO1_RX_TIMEOUT => 0b00000000;
      public static Byte SX127X_FIFO_RX_BASE_ADDR_MAX => 0b00000000;
      public static Byte SX127X_DIO0_PACK_PAYLOAD_READY => 0b00000000;
      public static Byte SX127X_CLEAR_IRQ_FLAG_PAYLOAD_CRC_ERROR => 0b00100000;
      public static Byte SX127X_ADDRESS_FILTERING_NODE => 0b00000010;
      public static Byte SX127X_ADDRESS_FILTERING_NODE_BROADCAST => 0b00000100;
      public static Byte SX127X_MAX_PACKET_LENGTH => 255;
      public static Byte SX127X_DIO0_TX_DONE => 0b01000000;
      public static Byte SX127X_FIFO_TX_BASE_ADDR_MAX => 0b00000000;
      public static Byte SX127X_TX => 0b00000011;
      public static Byte SX127X_MAX_PACKET_LENGTH_FSK => 64;
      public static Byte SX127X_DIO0_PACK_PACKET_SENT => 0b00000000;
    }

    private struct RegisterAdresses {
      public static Byte SX127X_REG_FIFO => 0x00;
      public static Byte SX127X_REG_OP_MODE => 0x01;
      public static Byte SX127X_REG_FRF_MSB => 0x06;
      public static Byte SX127X_REG_FRF_MID => 0x07;
      public static Byte SX127X_REG_FRF_LSB => 0x08;
      public static Byte SX127X_REG_PA_CONFIG => 0x09;
      public static Byte SX127X_REG_OCP => 0x0B;
      public static Byte SX127X_REG_LNA => 0x0C;
      public static Byte SX127X_REG_FIFO_ADDR_PTR => 0x0D;
      public static Byte SX127X_REG_FIFO_TX_BASE_ADDR => 0x0E;
      public static Byte SX127X_REG_FIFO_RX_BASE_ADDR => 0x0F;
      public static Byte SX127X_REG_RSSI_VALUE_FSK => 0x11;
      public static Byte SX127X_REG_IRQ_FLAGS => 0x12;
      public static Byte SX127X_REG_RX_NB_BYTES => 0x13;
      public static Byte SX127X_REG_PKT_SNR_VALUE => 0x19;
      public static Byte SX127X_REG_PKT_RSSI_VALUE => 0x1A;
      public static Byte SX127X_REG_MODEM_CONFIG_1 => 0x1D;
      public static Byte SX127X_REG_FEI_MSB_FSK => 0x1D;
      public static Byte SX127X_REG_FEI_LSB_FSK => 0x1E;
      public static Byte SX127X_REG_MODEM_CONFIG_2 => 0x1E;
      public static Byte SX127X_REG_PREAMBLE_MSB => 0x20;
      public static Byte SX127X_REG_PREAMBLE_LSB => 0x21;
      public static Byte SX127X_REG_PAYLOAD_LENGTH => 0x22;
      public static Byte SX127X_REG_HOP_PERIOD => 0x24;
      public static Byte SX127X_REG_PREAMBLE_MSB_FSK => 0x25;
      public static Byte SX127X_REG_PREAMBLE_LSB_FSK => 0x26;
      public static Byte SX1278_REG_MODEM_CONFIG_3 => 0x26;
      public static Byte SX127X_REG_FEI_MSB => 0x28;
      public static Byte SX127X_REG_FEI_MID => 0x29;
      public static Byte SX127X_REG_FEI_LSB => 0x2A;
      public static Byte SX127X_REG_PACKET_CONFIG_1 => 0x30;
      public static Byte SX127X_REG_DETECT_OPTIMIZE => 0x31;
      public static Byte SX127X_REG_DETECTION_THRESHOLD => 0x37;
      public static Byte SX127X_REG_SYNC_WORD => 0x39;
      public static Byte SX127X_REG_IRQ_FLAGS_1 => 0x3E;
      public static Byte SX127X_REG_IRQ_FLAGS_2 => 0x3F;
      public static Byte SX127X_REG_DIO_MAPPING_1 => 0x40;
      public static Byte SX127X_REG_VERSION => 0x42;
      public static Byte SX1278_REG_PA_DAC => 0x4D;
    }

    public struct Bandwidths {
      public static Int32 Freq_7k8 => 7800;
      public static Byte SX1278_BW_7_80_KHZ => 0b00000000;
      public static Int32 Freq_10k4 => 10400;
      public static Byte SX1278_BW_10_40_KHZ => 0b00010000;
      public static Int32 Freq_15k6 => 15600;
      public static Byte SX1278_BW_15_60_KHZ => 0b00100000;
      public static Int32 Freq_20k8 => 20800;
      public static Byte SX1278_BW_20_80_KHZ => 0b00110000;
      public static Int32 Freq_31k25 => 31250;
      public static Byte SX1278_BW_31_25_KHZ => 0b01000000;
      public static Int32 Freq_41k7 => 41700;
      public static Byte SX1278_BW_41_70_KHZ => 0b01010000;
      public static Int32 Freq_62k5 => 62500;
      public static Byte SX1278_BW_62_50_KHZ => 0b01100000;
      public static Int32 Freq_125k => 125000;
      public static Byte SX1278_BW_125_00_KHZ => 0b01110000;
      public static Int32 Freq_250k => 250000;
      public static Byte SX1278_BW_250_00_KHZ => 0b10000000;
      public static Int32 Freq_500k => 500000;
      public static Byte SX1278_BW_500_00_KHZ => 0b10010000;
    }

    public struct SpreadingFactors {
      public static Byte SF6 => 6;
      public static Byte SX127X_SF_6 => 0b01100000;
      public static Byte SF7 => 7;
      public static Byte SX127X_SF_7 => 0b01110000;
      public static Byte SF8 => 8;
      public static Byte SX127X_SF_8 => 0b10000000;
      public static Byte SF9 => 9;
      public static Byte SX127X_SF_9 => 0b10010000;
      public static Byte SF10 => 10;
      public static Byte SX127X_SF_10 => 0b10100000;
      public static Byte SF11 => 11;
      public static Byte SX127X_SF_11 => 0b10110000;
      public static Byte SF12 => 12;
      public static Byte SX127X_SF_12 => 0b11000000;
    }

    public struct Codingrates {
      public static Byte CR5 => 5;
      public static Byte SX1278_CR_4_5 => 0b00000010;
      public static Byte CR6 => 6;
      public static Byte SX1278_CR_4_6 => 0b00000100;
      public static Byte CR7 => 7;
      public static Byte SX1278_CR_4_7 => 0b00000110;
      public static Byte CR8 => 8;
      public static Byte SX1278_CR_4_8 => 0b00001000;
    }
  }
}
