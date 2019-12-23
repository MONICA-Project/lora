using System;

namespace Fraunhofer.Fit.Iot.Lora.lib {
  /*public partial class Ic880alora {
    
    public static class Registers {
      #region Global Registers
      public static LGWRegisters PAGE_REG = new LGWRegisters(-1, 0, 0, false, 2, false, 0);
      public static LGWRegisters SOFT_RESET = new LGWRegisters(-1, 0, 7, false, 1, false, 0);
      public static LGWRegisters VERSION = new LGWRegisters(-1, 1, 0, false, 8, true, 103);
      public static LGWRegisters RX_DATA_BUF_ADDR = new LGWRegisters(-1, 2, 0, false, 16, false, 0);
      public static LGWRegisters RX_DATA_BUF_DATA = new LGWRegisters(-1, 4, 0, false, 8, false, 0);
      public static LGWRegisters TX_DATA_BUF_ADDR = new LGWRegisters(-1, 5, 0, false, 8, false, 0);
      public static LGWRegisters TX_DATA_BUF_DATA = new LGWRegisters(-1, 6, 0, false, 8, false, 0);
      public static LGWRegisters CAPTURE_RAM_ADDR = new LGWRegisters(-1, 7, 0, false, 8, false, 0);
      public static LGWRegisters CAPTURE_RAM_DATA = new LGWRegisters(-1, 8, 0, false, 8, true, 0);
      public static LGWRegisters MCU_PROM_ADDR = new LGWRegisters(-1, 9, 0, false, 8, false, 0);
      public static LGWRegisters MCU_PROM_DATA = new LGWRegisters(-1, 10, 0, false, 8, false, 0);
      public static LGWRegisters RX_PACKET_DATA_FIFO_NUM_STORED = new LGWRegisters(-1, 11, 0, false, 8, false, 0);
      public static LGWRegisters RX_PACKET_DATA_FIFO_ADDR_POINTER = new LGWRegisters(-1, 12, 0, false, 16, true, 0);
      public static LGWRegisters RX_PACKET_DATA_FIFO_STATUS = new LGWRegisters(-1, 14, 0, false, 8, true, 0);
      public static LGWRegisters RX_PACKET_DATA_FIFO_PAYLOAD_SIZE = new LGWRegisters(-1, 15, 0, false, 8, true, 0);
      public static LGWRegisters MBWSSF_MODEM_ENABLE = new LGWRegisters(-1, 16, 0, false, 1, false, 0);
      public static LGWRegisters CONCENTRATOR_MODEM_ENABLE = new LGWRegisters(-1, 16, 1, false, 1, false, 0);
      public static LGWRegisters FSK_MODEM_ENABLE = new LGWRegisters(-1, 16, 2, false, 1, false, 0);
      public static LGWRegisters GLOBAL_EN = new LGWRegisters(-1, 16, 3, false, 1, false, 0);
      public static LGWRegisters CLK32M_EN = new LGWRegisters(-1, 17, 0, false, 1, false, 1);
      public static LGWRegisters CLKHS_EN = new LGWRegisters(-1, 17, 1, false, 1, false, 1);
      public static LGWRegisters START_BIST0 = new LGWRegisters(-1, 18, 0, false, 1, false, 0);
      public static LGWRegisters START_BIST1 = new LGWRegisters(-1, 18, 1, false, 1, false, 0);
      public static LGWRegisters CLEAR_BIST0 = new LGWRegisters(-1, 18, 2, false, 1, false, 0);
      public static LGWRegisters CLEAR_BIST1 = new LGWRegisters(-1, 18, 3, false, 1, false, 0);
      public static LGWRegisters BIST0_FINISHED = new LGWRegisters(-1, 19, 0, false, 1, true, 0);
      public static LGWRegisters BIST1_FINISHED = new LGWRegisters(-1, 19, 1, false, 1, true, 0);
      public static LGWRegisters MCU_AGC_PROG_RAM_BIST_STATUS = new LGWRegisters(-1, 20, 0, false, 1, true, 0);
      public static LGWRegisters MCU_ARB_PROG_RAM_BIST_STATUS = new LGWRegisters(-1, 20, 1, false, 1, true, 0);
      public static LGWRegisters CAPTURE_RAM_BIST_STATUS = new LGWRegisters(-1, 20, 2, false, 1, true, 0);
      public static LGWRegisters CHAN_FIR_RAM0_BIST_STATUS = new LGWRegisters(-1, 20, 3, false, 1, true, 0);
      public static LGWRegisters CHAN_FIR_RAM1_BIST_STATUS = new LGWRegisters(-1, 20, 4, false, 1, true, 0);
      public static LGWRegisters CORR0_RAM_BIST_STATUS = new LGWRegisters(-1, 21, 0, false, 1, true, 0);
      public static LGWRegisters CORR1_RAM_BIST_STATUS = new LGWRegisters(-1, 21, 1, false, 1, true, 0);
      public static LGWRegisters CORR2_RAM_BIST_STATUS = new LGWRegisters(-1, 21, 2, false, 1, true, 0);
      public static LGWRegisters CORR3_RAM_BIST_STATUS = new LGWRegisters(-1, 21, 3, false, 1, true, 0);
      public static LGWRegisters CORR4_RAM_BIST_STATUS = new LGWRegisters(-1, 21, 4, false, 1, true, 0);
      public static LGWRegisters CORR5_RAM_BIST_STATUS = new LGWRegisters(-1, 21, 5, false, 1, true, 0);
      public static LGWRegisters CORR6_RAM_BIST_STATUS = new LGWRegisters(-1, 21, 6, false, 1, true, 0);
      public static LGWRegisters CORR7_RAM_BIST_STATUS = new LGWRegisters(-1, 21, 7, false, 1, true, 0);
      public static LGWRegisters MODEM0_RAM0_BIST_STATUS = new LGWRegisters(-1, 22, 0, false, 1, true, 0);
      public static LGWRegisters MODEM1_RAM0_BIST_STATUS = new LGWRegisters(-1, 22, 1, false, 1, true, 0);
      public static LGWRegisters MODEM2_RAM0_BIST_STATUS = new LGWRegisters(-1, 22, 2, false, 1, true, 0);
      public static LGWRegisters MODEM3_RAM0_BIST_STATUS = new LGWRegisters(-1, 22, 3, false, 1, true, 0);
      public static LGWRegisters MODEM4_RAM0_BIST_STATUS = new LGWRegisters(-1, 22, 4, false, 1, true, 0);
      public static LGWRegisters MODEM5_RAM0_BIST_STATUS = new LGWRegisters(-1, 22, 5, false, 1, true, 0);
      public static LGWRegisters MODEM6_RAM0_BIST_STATUS = new LGWRegisters(-1, 22, 6, false, 1, true, 0);
      public static LGWRegisters MODEM7_RAM0_BIST_STATUS = new LGWRegisters(-1, 22, 7, false, 1, true, 0);
      public static LGWRegisters MODEM0_RAM1_BIST_STATUS = new LGWRegisters(-1, 23, 0, false, 1, true, 0);
      public static LGWRegisters MODEM1_RAM1_BIST_STATUS = new LGWRegisters(-1, 23, 1, false, 1, true, 0);
      public static LGWRegisters MODEM2_RAM1_BIST_STATUS = new LGWRegisters(-1, 23, 2, false, 1, true, 0);
      public static LGWRegisters MODEM3_RAM1_BIST_STATUS = new LGWRegisters(-1, 23, 3, false, 1, true, 0);
      public static LGWRegisters MODEM4_RAM1_BIST_STATUS = new LGWRegisters(-1, 23, 4, false, 1, true, 0);
      public static LGWRegisters MODEM5_RAM1_BIST_STATUS = new LGWRegisters(-1, 23, 5, false, 1, true, 0);
      public static LGWRegisters MODEM6_RAM1_BIST_STATUS = new LGWRegisters(-1, 23, 6, false, 1, true, 0);
      public static LGWRegisters MODEM7_RAM1_BIST_STATUS = new LGWRegisters(-1, 23, 7, false, 1, true, 0);
      public static LGWRegisters MODEM0_RAM2_BIST_STATUS = new LGWRegisters(-1, 24, 0, false, 1, true, 0);
      public static LGWRegisters MODEM1_RAM2_BIST_STATUS = new LGWRegisters(-1, 24, 1, false, 1, true, 0);
      public static LGWRegisters MODEM2_RAM2_BIST_STATUS = new LGWRegisters(-1, 24, 2, false, 1, true, 0);
      public static LGWRegisters MODEM3_RAM2_BIST_STATUS = new LGWRegisters(-1, 24, 3, false, 1, true, 0);
      public static LGWRegisters MODEM4_RAM2_BIST_STATUS = new LGWRegisters(-1, 24, 4, false, 1, true, 0);
      public static LGWRegisters MODEM5_RAM2_BIST_STATUS = new LGWRegisters(-1, 24, 5, false, 1, true, 0);
      public static LGWRegisters MODEM6_RAM2_BIST_STATUS = new LGWRegisters(-1, 24, 6, false, 1, true, 0);
      public static LGWRegisters MODEM7_RAM2_BIST_STATUS = new LGWRegisters(-1, 24, 7, false, 1, true, 0);
      public static LGWRegisters MODEM_MBWSSF_RAM0_BIST_STATUS = new LGWRegisters(-1, 25, 0, false, 1, true, 0);
      public static LGWRegisters MODEM_MBWSSF_RAM1_BIST_STATUS = new LGWRegisters(-1, 25, 1, false, 1, true, 0);
      public static LGWRegisters MODEM_MBWSSF_RAM2_BIST_STATUS = new LGWRegisters(-1, 25, 2, false, 1, true, 0);
      public static LGWRegisters MCU_AGC_DATA_RAM_BIST0_STATUS = new LGWRegisters(-1, 26, 0, false, 1, true, 0);
      public static LGWRegisters MCU_AGC_DATA_RAM_BIST1_STATUS = new LGWRegisters(-1, 26, 1, false, 1, true, 0);
      public static LGWRegisters MCU_ARB_DATA_RAM_BIST0_STATUS = new LGWRegisters(-1, 26, 2, false, 1, true, 0);
      public static LGWRegisters MCU_ARB_DATA_RAM_BIST1_STATUS = new LGWRegisters(-1, 26, 3, false, 1, true, 0);
      public static LGWRegisters TX_TOP_RAM_BIST0_STATUS = new LGWRegisters(-1, 26, 4, false, 1, true, 0);
      public static LGWRegisters TX_TOP_RAM_BIST1_STATUS = new LGWRegisters(-1, 26, 5, false, 1, true, 0);
      public static LGWRegisters DATA_MNGT_RAM_BIST0_STATUS = new LGWRegisters(-1, 26, 6, false, 1, true, 0);
      public static LGWRegisters DATA_MNGT_RAM_BIST1_STATUS = new LGWRegisters(-1, 26, 7, false, 1, true, 0);
      public static LGWRegisters GPIO_SELECT_INPUT = new LGWRegisters(-1, 27, 0, false, 4, false, 0);
      public static LGWRegisters GPIO_SELECT_OUTPUT = new LGWRegisters(-1, 28, 0, false, 4, false, 0);
      public static LGWRegisters GPIO_MODE = new LGWRegisters(-1, 29, 0, false, 5, false, 0);
      public static LGWRegisters GPIO_PIN_REG_IN = new LGWRegisters(-1, 30, 0, false, 5, true, 0);
      public static LGWRegisters GPIO_PIN_REG_OUT = new LGWRegisters(-1, 31, 0, false, 5, false, 0);
      public static LGWRegisters MCU_AGC_STATUS = new LGWRegisters(-1, 32, 0, false, 8, true, 0);
      public static LGWRegisters MCU_ARB_STATUS = new LGWRegisters(-1, 125, 0, false, 8, true, 0);
      public static LGWRegisters CHIP_ID = new LGWRegisters(-1, 126, 0, false, 8, true, 1);
      public static LGWRegisters EMERGENCY_FORCE_HOST_CTRL = new LGWRegisters(-1, 127, 0, false, 1, false, 1);
      #endregion

      #region Page 0 Registgers
      public static LGWRegisters RX_INVERT_IQ = new LGWRegisters(0, 33, 0, false, 1, false, 0);
      public static LGWRegisters MODEM_INVERT_IQ = new LGWRegisters(0, 33, 1, false, 1, false, 1);
      public static LGWRegisters MBWSSF_MODEM_INVERT_IQ = new LGWRegisters(0, 33, 2, false, 1, false, 0);
      public static LGWRegisters RX_EDGE_SELECT = new LGWRegisters(0, 33, 3, false, 1, false, 0);
      public static LGWRegisters MISC_RADIO_EN = new LGWRegisters(0, 33, 4, false, 1, false, 0);
      public static LGWRegisters FSK_MODEM_INVERT_IQ = new LGWRegisters(0, 33, 5, false, 1, false, 0);
      public static LGWRegisters FILTER_GAIN = new LGWRegisters(0, 34, 0, false, 4, false, 7);
      public static LGWRegisters RADIO_SELECT = new LGWRegisters(0, 35, 0, false, 8, false, 240);
      public static LGWRegisters IF_FREQ_0 = new LGWRegisters(0, 36, 0, true, 13, false, -384);
      public static LGWRegisters IF_FREQ_1 = new LGWRegisters(0, 38, 0, true, 13, false, -128);
      public static LGWRegisters IF_FREQ_2 = new LGWRegisters(0, 40, 0, true, 13, false, 128);
      public static LGWRegisters IF_FREQ_3 = new LGWRegisters(0, 42, 0, true, 13, false, 384);
      public static LGWRegisters IF_FREQ_4 = new LGWRegisters(0, 44, 0, true, 13, false, -384);
      public static LGWRegisters IF_FREQ_5 = new LGWRegisters(0, 46, 0, true, 13, false, -128);
      public static LGWRegisters IF_FREQ_6 = new LGWRegisters(0, 48, 0, true, 13, false, 128);
      public static LGWRegisters IF_FREQ_7 = new LGWRegisters(0, 50, 0, true, 13, false, 384);
      public static LGWRegisters IF_FREQ_8 = new LGWRegisters(0, 52, 0, true, 13, false, 0);
      public static LGWRegisters IF_FREQ_9 = new LGWRegisters(0, 54, 0, true, 13, false, 0);
      public static LGWRegisters CHANN_OVERRIDE_AGC_GAIN = new LGWRegisters(0, 64, 0, false, 1, false, 0);
      public static LGWRegisters CHANN_AGC_GAIN = new LGWRegisters(0, 64, 1, false, 4, false, 7);
      public static LGWRegisters CORR0_DETECT_EN = new LGWRegisters(0, 65, 0, false, 7, false, 0);
      public static LGWRegisters CORR1_DETECT_EN = new LGWRegisters(0, 66, 0, false, 7, false, 0);
      public static LGWRegisters CORR2_DETECT_EN = new LGWRegisters(0, 67, 0, false, 7, false, 0);
      public static LGWRegisters CORR3_DETECT_EN = new LGWRegisters(0, 68, 0, false, 7, false, 0);
      public static LGWRegisters CORR4_DETECT_EN = new LGWRegisters(0, 69, 0, false, 7, false, 0);
      public static LGWRegisters CORR5_DETECT_EN = new LGWRegisters(0, 70, 0, false, 7, false, 0);
      public static LGWRegisters CORR6_DETECT_EN = new LGWRegisters(0, 71, 0, false, 7, false, 0);
      public static LGWRegisters CORR7_DETECT_EN = new LGWRegisters(0, 72, 0, false, 7, false, 0);
      public static LGWRegisters CORR_SAME_PEAKS_OPTION_SF6 = new LGWRegisters(0, 73, 0, false, 1, false, 0);
      public static LGWRegisters CORR_SAME_PEAKS_OPTION_SF7 = new LGWRegisters(0, 73, 1, false, 1, false, 1);
      public static LGWRegisters CORR_SAME_PEAKS_OPTION_SF8 = new LGWRegisters(0, 73, 2, false, 1, false, 1);
      public static LGWRegisters CORR_SAME_PEAKS_OPTION_SF9 = new LGWRegisters(0, 73, 3, false, 1, false, 1);
      public static LGWRegisters CORR_SAME_PEAKS_OPTION_SF10 = new LGWRegisters(0, 73, 4, false, 1, false, 1);
      public static LGWRegisters CORR_SAME_PEAKS_OPTION_SF11 = new LGWRegisters(0, 73, 5, false, 1, false, 1);
      public static LGWRegisters CORR_SAME_PEAKS_OPTION_SF12 = new LGWRegisters(0, 73, 6, false, 1, false, 1);
      public static LGWRegisters CORR_SIG_NOISE_RATIO_SF6 = new LGWRegisters(0, 74, 0, false, 4, false, 4);
      public static LGWRegisters CORR_SIG_NOISE_RATIO_SF7 = new LGWRegisters(0, 74, 4, false, 4, false, 4);
      public static LGWRegisters CORR_SIG_NOISE_RATIO_SF8 = new LGWRegisters(0, 75, 0, false, 4, false, 4);
      public static LGWRegisters CORR_SIG_NOISE_RATIO_SF9 = new LGWRegisters(0, 75, 4, false, 4, false, 4);
      public static LGWRegisters CORR_SIG_NOISE_RATIO_SF10 = new LGWRegisters(0, 76, 0, false, 4, false, 4);
      public static LGWRegisters CORR_SIG_NOISE_RATIO_SF11 = new LGWRegisters(0, 76, 4, false, 4, false, 4);
      public static LGWRegisters CORR_SIG_NOISE_RATIO_SF12 = new LGWRegisters(0, 77, 0, false, 4, false, 4);
      public static LGWRegisters CORR_NUM_SAME_PEAK = new LGWRegisters(0, 78, 0, false, 4, false, 4);
      public static LGWRegisters CORR_MAC_GAIN = new LGWRegisters(0, 78, 4, false, 3, false, 5);
      public static LGWRegisters ADJUST_MODEM_START_OFFSET_RDX4 = new LGWRegisters(0, 81, 0, false, 12, false, 0);
      public static LGWRegisters ADJUST_MODEM_START_OFFSET_SF12_RDX4 = new LGWRegisters(0, 83, 0, false, 12, false, 4092);
      public static LGWRegisters DBG_CORR_SELECT_SF = new LGWRegisters(0, 85, 0, false, 8, false, 7);
      public static LGWRegisters DBG_CORR_SELECT_CHANNEL = new LGWRegisters(0, 86, 0, false, 8, false, 0);
      public static LGWRegisters DBG_DETECT_CPT = new LGWRegisters(0, 87, 0, false, 8, true, 0);
      public static LGWRegisters DBG_SYMB_CPT = new LGWRegisters(0, 88, 0, false, 8, true, 0);
      public static LGWRegisters CHIRP_INVERT_RX = new LGWRegisters(0, 89, 0, false, 1, false, 1);
      public static LGWRegisters DC_NOTCH_EN = new LGWRegisters(0, 89, 1, false, 1, false, 1);
      public static LGWRegisters IMPLICIT_CRC_EN = new LGWRegisters(0, 90, 0, false, 1, false, 0);
      public static LGWRegisters IMPLICIT_CODING_RATE = new LGWRegisters(0, 90, 1, false, 3, false, 0);
      public static LGWRegisters IMPLICIT_PAYLOAD_LENGHT = new LGWRegisters(0, 91, 0, false, 8, false, 0);
      public static LGWRegisters FREQ_TO_TIME_INVERT = new LGWRegisters(0, 92, 0, false, 8, false, 29);
      public static LGWRegisters FREQ_TO_TIME_DRIFT = new LGWRegisters(0, 93, 0, false, 6, false, 9);
      public static LGWRegisters PAYLOAD_FINE_TIMING_GAIN = new LGWRegisters(0, 94, 0, false, 2, false, 2);
      public static LGWRegisters PREAMBLE_FINE_TIMING_GAIN = new LGWRegisters(0, 94, 2, false, 2, false, 1);
      public static LGWRegisters TRACKING_INTEGRAL = new LGWRegisters(0, 94, 4, false, 2, false, 0);
      public static LGWRegisters FRAME_SYNCH_PEAK1_POS = new LGWRegisters(0, 95, 0, false, 4, false, 1);
      public static LGWRegisters FRAME_SYNCH_PEAK2_POS = new LGWRegisters(0, 95, 4, false, 4, false, 2);
      public static LGWRegisters PREAMBLE_SYMB1_NB = new LGWRegisters(0, 96, 0, false, 16, false, 10);
      public static LGWRegisters FRAME_SYNCH_GAIN = new LGWRegisters(0, 98, 0, false, 1, false, 1);
      public static LGWRegisters SYNCH_DETECT_TH = new LGWRegisters(0, 98, 1, false, 1, false, 1);
      public static LGWRegisters LLR_SCALE = new LGWRegisters(0, 99, 0, false, 4, false, 8);
      public static LGWRegisters SNR_AVG_CST = new LGWRegisters(0, 99, 4, false, 2, false, 2);
      public static LGWRegisters PPM_OFFSET = new LGWRegisters(0, 100, 0, false, 7, false, 0);
      public static LGWRegisters MAX_PAYLOAD_LEN = new LGWRegisters(0, 101, 0, false, 8, false, 255);
      public static LGWRegisters ONLY_CRC_EN = new LGWRegisters(0, 102, 0, false, 1, false, 1);
      public static LGWRegisters ZERO_PAD = new LGWRegisters(0, 103, 0, false, 8, false, 0);
      public static LGWRegisters DEC_GAIN_OFFSET = new LGWRegisters(0, 104, 0, false, 4, false, 8);
      public static LGWRegisters CHAN_GAIN_OFFSET = new LGWRegisters(0, 104, 4, false, 4, false, 7);
      public static LGWRegisters FORCE_HOST_RADIO_CTRL = new LGWRegisters(0, 105, 1, false, 1, false, 1);
      public static LGWRegisters FORCE_HOST_FE_CTRL = new LGWRegisters(0, 105, 2, false, 1, false, 1);
      public static LGWRegisters FORCE_DEC_FILTER_GAIN = new LGWRegisters(0, 105, 3, false, 1, false, 1);
      public static LGWRegisters MCU_RST_0 = new LGWRegisters(0, 106, 0, false, 1, false, 1);
      public static LGWRegisters MCU_RST_1 = new LGWRegisters(0, 106, 1, false, 1, false, 1);
      public static LGWRegisters MCU_SELECT_MUX_0 = new LGWRegisters(0, 106, 2, false, 1, false, 0);
      public static LGWRegisters MCU_SELECT_MUX_1 = new LGWRegisters(0, 106, 3, false, 1, false, 0);
      public static LGWRegisters MCU_CORRUPTION_DETECTED_0 = new LGWRegisters(0, 106, 4, false, 1, true, 0);
      public static LGWRegisters MCU_CORRUPTION_DETECTED_1 = new LGWRegisters(0, 106, 5, false, 1, true, 0);
      public static LGWRegisters MCU_SELECT_EDGE_0 = new LGWRegisters(0, 106, 6, false, 1, false, 0);
      public static LGWRegisters MCU_SELECT_EDGE_1 = new LGWRegisters(0, 106, 7, false, 1, false, 0);
      public static LGWRegisters CHANN_SELECT_RSSI = new LGWRegisters(0, 107, 0, false, 8, false, 1);
      public static LGWRegisters RSSI_BB_DEFAULT_VALUE = new LGWRegisters(0, 108, 0, false, 8, false, 32);
      public static LGWRegisters RSSI_DEC_DEFAULT_VALUE = new LGWRegisters(0, 109, 0, false, 8, false, 100);
      public static LGWRegisters RSSI_CHANN_DEFAULT_VALUE = new LGWRegisters(0, 110, 0, false, 8, false, 100);
      public static LGWRegisters RSSI_BB_FILTER_ALPHA = new LGWRegisters(0, 111, 0, false, 5, false, 7);
      public static LGWRegisters RSSI_DEC_FILTER_ALPHA = new LGWRegisters(0, 112, 0, false, 5, false, 5);
      public static LGWRegisters RSSI_CHANN_FILTER_ALPHA = new LGWRegisters(0, 113, 0, false, 5, false, 8);
      public static LGWRegisters IQ_MISMATCH_A_AMP_COEFF = new LGWRegisters(0, 114, 0, false, 6, false, 0);
      public static LGWRegisters IQ_MISMATCH_A_PHI_COEFF = new LGWRegisters(0, 115, 0, false, 6, false, 0);
      public static LGWRegisters IQ_MISMATCH_B_AMP_COEFF = new LGWRegisters(0, 116, 0, false, 6, false, 0);
      public static LGWRegisters IQ_MISMATCH_B_SEL_I = new LGWRegisters(0, 116, 6, false, 1, false, 0);
      public static LGWRegisters IQ_MISMATCH_B_PHI_COEFF = new LGWRegisters(0, 117, 0, false, 6, false, 0);
      #endregion

      #region Page 1 Registers
      public static LGWRegisters TX_TRIG_ALL = new LGWRegisters(1, 33, 0, false, 8, false, 0);
      public static LGWRegisters TX_TRIG_IMMEDIATE = new LGWRegisters(1, 33, 0, false, 1, false, 0);
      public static LGWRegisters TX_TRIG_DELAYED = new LGWRegisters(1, 33, 1, false, 1, false, 0);
      public static LGWRegisters TX_TRIG_GPS = new LGWRegisters(1, 33, 2, false, 1, false, 0);
      public static LGWRegisters TX_START_DELAY = new LGWRegisters(1, 34, 0, false, 16, false, 0);
      public static LGWRegisters TX_FRAME_SYNCH_PEAK1_POS = new LGWRegisters(1, 36, 0, false, 4, false, 1);
      public static LGWRegisters TX_FRAME_SYNCH_PEAK2_POS = new LGWRegisters(1, 36, 4, false, 4, false, 2);
      public static LGWRegisters TX_RAMP_DURATION = new LGWRegisters(1, 37, 0, false, 3, false, 0);
      public static LGWRegisters TX_OFFSET_I = new LGWRegisters(1, 39, 0, true, 8, false, 0);
      public static LGWRegisters TX_OFFSET_Q = new LGWRegisters(1, 40, 0, true, 8, false, 0);
      public static LGWRegisters TX_MODE = new LGWRegisters(1, 41, 0, false, 1, false, 0);
      public static LGWRegisters TX_ZERO_PAD = new LGWRegisters(1, 41, 1, false, 4, false, 0);
      public static LGWRegisters TX_EDGE_SELECT = new LGWRegisters(1, 41, 5, false, 1, false, 0);
      public static LGWRegisters TX_EDGE_SELECT_TOP = new LGWRegisters(1, 41, 6, false, 1, false, 0);
      public static LGWRegisters TX_GAIN = new LGWRegisters(1, 42, 0, false, 2, false, 0);
      public static LGWRegisters TX_CHIRP_LOW_PASS = new LGWRegisters(1, 42, 2, false, 3, false, 5);
      public static LGWRegisters TX_FCC_WIDEBAND = new LGWRegisters(1, 42, 5, false, 2, false, 0);
      public static LGWRegisters TX_SWAP_IQ = new LGWRegisters(1, 42, 7, false, 1, false, 1);
      public static LGWRegisters MBWSSF_IMPLICIT_HEADER = new LGWRegisters(1, 43, 0, false, 1, false, 0);
      public static LGWRegisters MBWSSF_IMPLICIT_CRC_EN = new LGWRegisters(1, 43, 1, false, 1, false, 0);
      public static LGWRegisters MBWSSF_IMPLICIT_CODING_RATE = new LGWRegisters(1, 43, 2, false, 3, false, 0);
      public static LGWRegisters MBWSSF_IMPLICIT_PAYLOAD_LENGHT = new LGWRegisters(1, 44, 0, false, 8, false, 0);
      public static LGWRegisters MBWSSF_AGC_FREEZE_ON_DETECT = new LGWRegisters(1, 45, 0, false, 1, false, 1);
      public static LGWRegisters MBWSSF_FRAME_SYNCH_PEAK1_POS = new LGWRegisters(1, 46, 0, false, 4, false, 1);
      public static LGWRegisters MBWSSF_FRAME_SYNCH_PEAK2_POS = new LGWRegisters(1, 46, 4, false, 4, false, 2);
      public static LGWRegisters MBWSSF_PREAMBLE_SYMB1_NB = new LGWRegisters(1, 47, 0, false, 16, false, 10);
      public static LGWRegisters MBWSSF_FRAME_SYNCH_GAIN = new LGWRegisters(1, 49, 0, false, 1, false, 1);
      public static LGWRegisters MBWSSF_SYNCH_DETECT_TH = new LGWRegisters(1, 49, 1, false, 1, false, 1);
      public static LGWRegisters MBWSSF_DETECT_MIN_SINGLE_PEAK = new LGWRegisters(1, 50, 0, false, 8, false, 10);
      public static LGWRegisters MBWSSF_DETECT_TRIG_SAME_PEAK_NB = new LGWRegisters(1, 51, 0, false, 3, false, 3);
      public static LGWRegisters MBWSSF_FREQ_TO_TIME_INVERT = new LGWRegisters(1, 52, 0, false, 8, false, 29);
      public static LGWRegisters MBWSSF_FREQ_TO_TIME_DRIFT = new LGWRegisters(1, 53, 0, false, 6, false, 36);
      public static LGWRegisters MBWSSF_PPM_CORRECTION = new LGWRegisters(1, 54, 0, false, 12, false, 0);
      public static LGWRegisters MBWSSF_PAYLOAD_FINE_TIMING_GAIN = new LGWRegisters(1, 56, 0, false, 2, false, 2);
      public static LGWRegisters MBWSSF_PREAMBLE_FINE_TIMING_GAIN = new LGWRegisters(1, 56, 2, false, 2, false, 1);
      public static LGWRegisters MBWSSF_TRACKING_INTEGRAL = new LGWRegisters(1, 56, 4, false, 2, false, 0);
      public static LGWRegisters MBWSSF_ZERO_PAD = new LGWRegisters(1, 57, 0, false, 8, false, 0);
      public static LGWRegisters MBWSSF_MODEM_BW = new LGWRegisters(1, 58, 0, false, 2, false, 0);
      public static LGWRegisters MBWSSF_RADIO_SELECT = new LGWRegisters(1, 58, 2, false, 1, false, 0);
      public static LGWRegisters MBWSSF_RX_CHIRP_INVERT = new LGWRegisters(1, 58, 3, false, 1, false, 1);
      public static LGWRegisters MBWSSF_LLR_SCALE = new LGWRegisters(1, 59, 0, false, 4, false, 8);
      public static LGWRegisters MBWSSF_SNR_AVG_CST = new LGWRegisters(1, 59, 4, false, 2, false, 3);
      public static LGWRegisters MBWSSF_PPM_OFFSET = new LGWRegisters(1, 59, 6, false, 1, false, 0);
      public static LGWRegisters MBWSSF_RATE_SF = new LGWRegisters(1, 60, 0, false, 4, false, 7);
      public static LGWRegisters MBWSSF_ONLY_CRC_EN = new LGWRegisters(1, 60, 4, false, 1, false, 1);
      public static LGWRegisters MBWSSF_MAX_PAYLOAD_LEN = new LGWRegisters(1, 61, 0, false, 8, false, 255);
      public static LGWRegisters TX_STATUS = new LGWRegisters(1, 62, 0, false, 8, true, 128);
      public static LGWRegisters FSK_CH_BW_EXPO = new LGWRegisters(1, 63, 0, false, 3, false, 0);
      public static LGWRegisters FSK_RSSI_LENGTH = new LGWRegisters(1, 63, 3, false, 3, false, 0);
      public static LGWRegisters FSK_RX_INVERT = new LGWRegisters(1, 63, 6, false, 1, false, 0);
      public static LGWRegisters FSK_PKT_MODE = new LGWRegisters(1, 63, 7, false, 1, false, 0);
      public static LGWRegisters FSK_PSIZE = new LGWRegisters(1, 64, 0, false, 3, false, 0);
      public static LGWRegisters FSK_CRC_EN = new LGWRegisters(1, 64, 3, false, 1, false, 0);
      public static LGWRegisters FSK_DCFREE_ENC = new LGWRegisters(1, 64, 4, false, 2, false, 0);
      public static LGWRegisters FSK_CRC_IBM = new LGWRegisters(1, 64, 6, false, 1, false, 0);
      public static LGWRegisters FSK_ERROR_OSR_TOL = new LGWRegisters(1, 65, 0, false, 5, false, 0);
      public static LGWRegisters FSK_RADIO_SELECT = new LGWRegisters(1, 65, 7, false, 1, false, 0);
      public static LGWRegisters FSK_BR_RATIO = new LGWRegisters(1, 66, 0, false, 16, false, 0);
      public static LGWRegisters FSK_REF_PATTERN_LSB = new LGWRegisters(1, 68, 0, false, 32, false, 0);
      public static LGWRegisters FSK_REF_PATTERN_MSB = new LGWRegisters(1, 72, 0, false, 32, false, 0);
      public static LGWRegisters FSK_PKT_LENGTH = new LGWRegisters(1, 76, 0, false, 8, false, 0);
      public static LGWRegisters FSK_TX_GAUSSIAN_EN = new LGWRegisters(1, 77, 0, false, 1, false, 1);
      public static LGWRegisters FSK_TX_GAUSSIAN_SELECT_BT = new LGWRegisters(1, 77, 1, false, 2, false, 0);
      public static LGWRegisters FSK_TX_PATTERN_EN = new LGWRegisters(1, 77, 3, false, 1, false, 1);
      public static LGWRegisters FSK_TX_PREAMBLE_SEQ = new LGWRegisters(1, 77, 4, false, 1, false, 0);
      public static LGWRegisters FSK_TX_PSIZE = new LGWRegisters(1, 77, 5, false, 3, false, 0);
      public static LGWRegisters FSK_NODE_ADRS = new LGWRegisters(1, 80, 0, false, 8, false, 0);
      public static LGWRegisters FSK_BROADCAST = new LGWRegisters(1, 81, 0, false, 8, false, 0);
      public static LGWRegisters FSK_AUTO_AFC_ON = new LGWRegisters(1, 82, 0, false, 1, false, 1);
      public static LGWRegisters FSK_PATTERN_TIMEOUT_CFG = new LGWRegisters(1, 83, 0, false, 10, false, 0);
      #endregion

      #region Page 2 Registers
      public static LGWRegisters SPI_RADIO_A__DATA = new LGWRegisters(2, 33, 0, false, 8, false, 0);
      public static LGWRegisters SPI_RADIO_A__DATA_READBACK = new LGWRegisters(2, 34, 0, false, 8, true, 0);
      public static LGWRegisters SPI_RADIO_A__ADDR = new LGWRegisters(2, 35, 0, false, 8, false, 0);
      public static LGWRegisters SPI_RADIO_A__CS = new LGWRegisters(2, 37, 0, false, 1, false, 0);
      public static LGWRegisters SPI_RADIO_B__DATA = new LGWRegisters(2, 38, 0, false, 8, false, 0);
      public static LGWRegisters SPI_RADIO_B__DATA_READBACK = new LGWRegisters(2, 39, 0, false, 8, true, 0);
      public static LGWRegisters SPI_RADIO_B__ADDR = new LGWRegisters(2, 40, 0, false, 8, false, 0);
      public static LGWRegisters SPI_RADIO_B__CS = new LGWRegisters(2, 42, 0, false, 1, false, 0);
      public static LGWRegisters RADIO_A_EN = new LGWRegisters(2, 43, 0, false, 1, false, 0);
      public static LGWRegisters RADIO_B_EN = new LGWRegisters(2, 43, 1, false, 1, false, 0);
      public static LGWRegisters RADIO_RST = new LGWRegisters(2, 43, 2, false, 1, false, 1);
      public static LGWRegisters LNA_A_EN = new LGWRegisters(2, 43, 3, false, 1, false, 0);
      public static LGWRegisters PA_A_EN = new LGWRegisters(2, 43, 4, false, 1, false, 0);
      public static LGWRegisters LNA_B_EN = new LGWRegisters(2, 43, 5, false, 1, false, 0);
      public static LGWRegisters PA_B_EN = new LGWRegisters(2, 43, 6, false, 1, false, 0);
      public static LGWRegisters PA_GAIN = new LGWRegisters(2, 44, 0, false, 2, false, 0);
      public static LGWRegisters LNA_A_CTRL_LUT = new LGWRegisters(2, 45, 0, false, 4, false, 2);
      public static LGWRegisters PA_A_CTRL_LUT = new LGWRegisters(2, 45, 4, false, 4, false, 4);
      public static LGWRegisters LNA_B_CTRL_LUT = new LGWRegisters(2, 46, 0, false, 4, false, 2);
      public static LGWRegisters PA_B_CTRL_LUT = new LGWRegisters(2, 46, 4, false, 4, false, 4);
      public static LGWRegisters CAPTURE_SOURCE = new LGWRegisters(2, 47, 0, false, 5, false, 0);
      public static LGWRegisters CAPTURE_START = new LGWRegisters(2, 47, 5, false, 1, false, 0);
      public static LGWRegisters CAPTURE_FORCE_TRIGGER = new LGWRegisters(2, 47, 6, false, 1, false, 0);
      public static LGWRegisters CAPTURE_WRAP = new LGWRegisters(2, 47, 7, false, 1, false, 0);
      public static LGWRegisters CAPTURE_PERIOD = new LGWRegisters(2, 48, 0, false, 16, false, 0);
      public static LGWRegisters MODEM_STATUS = new LGWRegisters(2, 51, 0, false, 8, true, 0);
      public static LGWRegisters VALID_HEADER_COUNTER_0 = new LGWRegisters(2, 52, 0, false, 8, true, 0);
      public static LGWRegisters VALID_PACKET_COUNTER_0 = new LGWRegisters(2, 54, 0, false, 8, true, 0);
      public static LGWRegisters VALID_HEADER_COUNTER_MBWSSF = new LGWRegisters(2, 56, 0, false, 8, true, 0);
      public static LGWRegisters VALID_HEADER_COUNTER_FSK = new LGWRegisters(2, 57, 0, false, 8, true, 0);
      public static LGWRegisters VALID_PACKET_COUNTER_MBWSSF = new LGWRegisters(2, 58, 0, false, 8, true, 0);
      public static LGWRegisters VALID_PACKET_COUNTER_FSK = new LGWRegisters(2, 59, 0, false, 8, true, 0);
      public static LGWRegisters CHANN_RSSI = new LGWRegisters(2, 60, 0, false, 8, true, 0);
      public static LGWRegisters BB_RSSI = new LGWRegisters(2, 61, 0, false, 8, true, 0);
      public static LGWRegisters DEC_RSSI = new LGWRegisters(2, 62, 0, false, 8, true, 0);
      public static LGWRegisters DBG_MCU_DATA = new LGWRegisters(2, 63, 0, false, 8, true, 0);
      public static LGWRegisters DBG_ARB_MCU_RAM_DATA = new LGWRegisters(2, 64, 0, false, 8, true, 0);
      public static LGWRegisters DBG_AGC_MCU_RAM_DATA = new LGWRegisters(2, 65, 0, false, 8, true, 0);
      public static LGWRegisters NEXT_PACKET_CNT = new LGWRegisters(2, 66, 0, false, 16, true, 0);
      public static LGWRegisters ADDR_CAPTURE_COUNT = new LGWRegisters(2, 68, 0, false, 16, true, 0);
      public static LGWRegisters TIMESTAMP = new LGWRegisters(2, 70, 0, false, 32, true, 0);
      public static LGWRegisters DBG_CHANN0_GAIN = new LGWRegisters(2, 74, 0, false, 4, true, 0);
      public static LGWRegisters DBG_CHANN1_GAIN = new LGWRegisters(2, 74, 4, false, 4, true, 0);
      public static LGWRegisters DBG_CHANN2_GAIN = new LGWRegisters(2, 75, 0, false, 4, true, 0);
      public static LGWRegisters DBG_CHANN3_GAIN = new LGWRegisters(2, 75, 4, false, 4, true, 0);
      public static LGWRegisters DBG_CHANN4_GAIN = new LGWRegisters(2, 76, 0, false, 4, true, 0);
      public static LGWRegisters DBG_CHANN5_GAIN = new LGWRegisters(2, 76, 4, false, 4, true, 0);
      public static LGWRegisters DBG_CHANN6_GAIN = new LGWRegisters(2, 77, 0, false, 4, true, 0);
      public static LGWRegisters DBG_CHANN7_GAIN = new LGWRegisters(2, 77, 4, false, 4, true, 0);
      public static LGWRegisters DBG_DEC_FILT_GAIN = new LGWRegisters(2, 78, 0, false, 4, true, 0);
      public static LGWRegisters SPI_DATA_FIFO_PTR = new LGWRegisters(2, 79, 0, false, 3, true, 0);
      public static LGWRegisters PACKET_DATA_FIFO_PTR = new LGWRegisters(2, 79, 3, false, 3, true, 0);
      public static LGWRegisters DBG_ARB_MCU_RAM_ADDR = new LGWRegisters(2, 80, 0, false, 8, false, 0);
      public static LGWRegisters DBG_AGC_MCU_RAM_ADDR = new LGWRegisters(2, 81, 0, false, 8, false, 0);
      public static LGWRegisters SPI_MASTER_CHIP_SELECT_POLARITY = new LGWRegisters(2, 82, 0, false, 1, false, 0);
      public static LGWRegisters SPI_MASTER_CPOL = new LGWRegisters(2, 82, 1, false, 1, false, 0);
      public static LGWRegisters SPI_MASTER_CPHA = new LGWRegisters(2, 82, 2, false, 1, false, 0);
      public static LGWRegisters SIG_GEN_ANALYSER_MUX_SEL = new LGWRegisters(2, 83, 0, false, 1, false, 0);
      public static LGWRegisters SIG_GEN_EN = new LGWRegisters(2, 84, 0, false, 1, false, 0);
      public static LGWRegisters SIG_ANALYSER_EN = new LGWRegisters(2, 84, 1, false, 1, false, 0);
      public static LGWRegisters SIG_ANALYSER_AVG_LEN = new LGWRegisters(2, 84, 2, false, 2, false, 0);
      public static LGWRegisters SIG_ANALYSER_PRECISION = new LGWRegisters(2, 84, 4, false, 3, false, 0);
      public static LGWRegisters SIG_ANALYSER_VALID_OUT = new LGWRegisters(2, 84, 7, false, 1, true, 0);
      public static LGWRegisters SIG_GEN_FREQ = new LGWRegisters(2, 85, 0, false, 8, false, 0);
      public static LGWRegisters SIG_ANALYSER_FREQ = new LGWRegisters(2, 86, 0, false, 8, false, 0);
      public static LGWRegisters SIG_ANALYSER_I_OUT = new LGWRegisters(2, 87, 0, false, 8, true, 0);
      public static LGWRegisters SIG_ANALYSER_Q_OUT = new LGWRegisters(2, 88, 0, false, 8, true, 0);
      public static LGWRegisters GPS_EN = new LGWRegisters(2, 89, 0, false, 1, false, 0);
      public static LGWRegisters GPS_POL = new LGWRegisters(2, 89, 1, false, 1, false, 1);
      public static LGWRegisters SW_TEST_REG1 = new LGWRegisters(2, 90, 0, true, 8, false, 0);
      public static LGWRegisters SW_TEST_REG2 = new LGWRegisters(2, 91, 2, true, 6, false, 0);
      public static LGWRegisters SW_TEST_REG3 = new LGWRegisters(2, 92, 0, true, 16, false, 0);
      public static LGWRegisters DATA_MNGT_STATUS = new LGWRegisters(2, 94, 0, false, 4, true, 0);
      public static LGWRegisters DATA_MNGT_CPT_FRAME_ALLOCATED = new LGWRegisters(2, 95, 0, false, 5, true, 0);
      public static LGWRegisters DATA_MNGT_CPT_FRAME_FINISHED = new LGWRegisters(2, 96, 0, false, 5, true, 0);
      public static LGWRegisters DATA_MNGT_CPT_FRAME_READEN = new LGWRegisters(2, 97, 0, false, 5, true, 0);
      #endregion
    }
   
    public struct LGWRegisters {
      /// <summary>
      /// page containing the register (-1 for all pages)
      /// </summary>
      public SByte RegisterPage;
      /// <summary>
      /// base address of the register (7 bit)
      /// </summary>
      public Byte Address;
      /// <summary>
      /// position of the register LSB (between 0 to 7)
      /// </summary>
      public Byte BitOffset;
      /// <summary>
      /// indicates the register is signed (2 complem.)
      /// </summary>
      public Boolean SignedInt;
      /// <summary>
      /// number of bits in the register
      /// </summary>
      public Byte SizeInBits;
      /// <summary>
      /// indicates a read-only register
      /// </summary>
      public Boolean ReadonlyRegister;
      /// <summary>
      /// register default value
      /// </summary>
      public Int32 DefaultValue;
      /// <summary>
      /// A Register of SX3101
      /// </summary>
      /// <param name="registerPage">page containing the register (-1 for all pages)</param>
      /// <param name="address">base address of the register (7 bit)</param>
      /// <param name="bitOffset">position of the register LSB (between 0 to 7)</param>
      /// <param name="signedInt">indicates the register is signed (2 complem.)</param>
      /// <param name="sizeInBits">number of bits in the register</param>
      /// <param name="readonlyRegister">indicates a read-only register</param>
      /// <param name="defaultValue">register default value</param>
      public LGWRegisters(SByte registerPage, Byte address, Byte bitOffset, Boolean signedInt, Byte sizeInBits, Boolean readonlyRegister, Int32 defaultValue) {
        this.RegisterPage = registerPage;
        this.Address = address;
        this.BitOffset = bitOffset;
        this.SignedInt = signedInt;
        this.SizeInBits = sizeInBits;
        this.ReadonlyRegister = readonlyRegister;
        this.DefaultValue = defaultValue;
      }
      public override String ToString() => "Reg: [P:" + this.RegisterPage + ",A:" + this.Address + ",O:" + this.BitOffset + "]";
    };

    public static class SX125X {
      public static Byte TX_DAC_CLK_SEL = 1;
      public static Byte XOSC_GM_STARTUP = 13;
      public static Byte XOSC_DISABLE = 2;
      public static Byte TX_MIX_GAIN = 14;
      public static Byte TX_DAC_GAIN = 2;
      public static Byte TX_ANA_BW = 0;
      public static Byte TX_PLL_BW = 1;
      public static Byte TX_DAC_BW = 5;
      public static Byte LNA_ZIN = 1;
      public static Byte RX_BB_GAIN = 12;
      public static Byte RX_LNA_GAIN = 1;
      public static Byte RX_BB_BW = 0;
      public static Byte RX_ADC_TRIM = 6;
      public static Byte RX_ADC_BW = 7;
      public static Byte ADC_TEMP = 0;
      public static Byte RX_PLL_BW = 0;
      public static UInt32 FRAC_32MHz = 15625;
    }
  }*/
}
