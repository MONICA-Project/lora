using System;
using System.Collections.Generic;
using System.Text;

namespace Fraunhofer.Fit.Iot.Lora.lib.Ic880a {
  public partial class Ic880a {
    public static partial class Registers {
      #region FPGA Registers
      public static FpgaRegisters FPGA_SOFT_RESET = new FpgaRegisters(-1, 0, 0, false, 1, false, 0, 0x1);
      public static FpgaRegisters FPGA_FEATURE = new FpgaRegisters(-1, 0, 1, false, 4, true, 0, 0x1);
      public static FpgaRegisters FPGA_LBT_INITIAL_FREQ = new FpgaRegisters(-1, 0, 5, false, 3, true, 0, 0x1);
      public static FpgaRegisters FPGA_VERSION = new FpgaRegisters(-1, 1, 0, false, 8, true, 0, 0x1);
      public static FpgaRegisters FPGA_STATUS = new FpgaRegisters(-1, 2, 0, false, 8, true, 0, 0x1);
      public static FpgaRegisters FPGA_CTRL_FEATURE_START = new FpgaRegisters(-1, 3, 0, false, 1, false, 0, 0x1);
      public static FpgaRegisters FPGA_CTRL_RADIO_RESET = new FpgaRegisters(-1, 3, 1, false, 1, false, 0, 0x1);
      public static FpgaRegisters FPGA_CTRL_INPUT_SYNC_I = new FpgaRegisters(-1, 3, 2, false, 1, false, 0, 0x1);
      public static FpgaRegisters FPGA_CTRL_INPUT_SYNC_Q = new FpgaRegisters(-1, 3, 3, false, 1, false, 0, 0x1);
      public static FpgaRegisters FPGA_CTRL_OUTPUT_SYNC = new FpgaRegisters(-1, 3, 4, false, 1, false, 0, 0x1);
      public static FpgaRegisters FPGA_CTRL_INVERT_IQ = new FpgaRegisters(-1, 3, 5, false, 1, false, 0, 0x1);
      public static FpgaRegisters FPGA_CTRL_ACCESS_HISTO_MEM = new FpgaRegisters(-1, 3, 6, false, 1, false, 0, 0x1);
      public static FpgaRegisters FPGA_CTRL_CLEAR_HISTO_MEM = new FpgaRegisters(-1, 3, 7, false, 1, false, 0, 0x1);
      public static FpgaRegisters FPGA_HISTO_RAM_ADDR = new FpgaRegisters(-1, 4, 0, false, 8, false, 0, 0x1);
      public static FpgaRegisters FPGA_HISTO_RAM_DATA = new FpgaRegisters(-1, 5, 0, false, 8, true, 0, 0x1);
      public static FpgaRegisters FPGA_HISTO_NB_READ = new FpgaRegisters(-1, 8, 0, false, 16, false, 1000, 0x1);
      public static FpgaRegisters FPGA_LBT_TIMESTAMP_CH = new FpgaRegisters(-1, 14, 0, false, 16, true, 0, 0x1);
      public static FpgaRegisters FPGA_LBT_TIMESTAMP_SELECT_CH = new FpgaRegisters(-1, 17, 0, false, 4, false, 0, 0x1);
      public static FpgaRegisters FPGA_LBT_CH0_FREQ_OFFSET = new FpgaRegisters(-1, 18, 0, false, 8, false, 0, 0x1);
      public static FpgaRegisters FPGA_LBT_CH1_FREQ_OFFSET = new FpgaRegisters(-1, 19, 0, false, 8, false, 0, 0x1);
      public static FpgaRegisters FPGA_LBT_CH2_FREQ_OFFSET = new FpgaRegisters(-1, 20, 0, false, 8, false, 0, 0x1);
      public static FpgaRegisters FPGA_LBT_CH3_FREQ_OFFSET = new FpgaRegisters(-1, 21, 0, false, 8, false, 0, 0x1);
      public static FpgaRegisters FPGA_LBT_CH4_FREQ_OFFSET = new FpgaRegisters(-1, 22, 0, false, 8, false, 0, 0x1);
      public static FpgaRegisters FPGA_LBT_CH5_FREQ_OFFSET = new FpgaRegisters(-1, 23, 0, false, 8, false, 0, 0x1);
      public static FpgaRegisters FPGA_LBT_CH6_FREQ_OFFSET = new FpgaRegisters(-1, 24, 0, false, 8, false, 0, 0x1);
      public static FpgaRegisters FPGA_LBT_CH7_FREQ_OFFSET = new FpgaRegisters(-1, 25, 0, false, 8, false, 0, 0x1);
      public static FpgaRegisters FPGA_SCAN_FREQ_OFFSET = new FpgaRegisters(-1, 26, 0, false, 8, false, 0, 0x1);
      public static FpgaRegisters FPGA_LBT_SCAN_TIME_CH0 = new FpgaRegisters(-1, 28, 0, false, 1, false, 0, 0x1);
      public static FpgaRegisters FPGA_LBT_SCAN_TIME_CH1 = new FpgaRegisters(-1, 28, 1, false, 1, false, 0, 0x1);
      public static FpgaRegisters FPGA_LBT_SCAN_TIME_CH2 = new FpgaRegisters(-1, 28, 2, false, 1, false, 0, 0x1);
      public static FpgaRegisters FPGA_LBT_SCAN_TIME_CH3 = new FpgaRegisters(-1, 28, 3, false, 1, false, 0, 0x1);
      public static FpgaRegisters FPGA_LBT_SCAN_TIME_CH4 = new FpgaRegisters(-1, 28, 4, false, 1, false, 0, 0x1);
      public static FpgaRegisters FPGA_LBT_SCAN_TIME_CH5 = new FpgaRegisters(-1, 28, 5, false, 1, false, 0, 0x1);
      public static FpgaRegisters FPGA_LBT_SCAN_TIME_CH6 = new FpgaRegisters(-1, 28, 6, false, 1, false, 0, 0x1);
      public static FpgaRegisters FPGA_LBT_SCAN_TIME_CH7 = new FpgaRegisters(-1, 28, 7, false, 1, false, 0, 0x1);
      public static FpgaRegisters FPGA_RSSI_TARGET = new FpgaRegisters(-1, 30, 0, false, 8, false, 160, 0x1);
      public static FpgaRegisters FPGA_HISTO_SCAN_FREQ = new FpgaRegisters(-1, 31, 0, false, 24, false, 0, 0x1);
      public static FpgaRegisters FPGA_NOTCH_FREQ_OFFSET = new FpgaRegisters(-1, 34, 0, false, 6, false, 0, 0x1);
      #endregion
    }

    private readonly Boolean _tx_notch_support = false;
    private readonly Byte _tx_notch_offset = 0;

    private Single FpgaGetTxNotchDelay() {
      Single tx_notch_delay;

      if(this._tx_notch_support == false) {
        return 0;
      }

      /* Notch filtering performed by FPGA adds a constant delay (group delay) that we need to compensate */
      tx_notch_delay = 31.25f * ((64 + this._tx_notch_offset) / 2f) / 1E3f; /* 32MHz => 31.25ns */

      return tx_notch_delay;
    }

    /*private void FpgaConfigure(UInt32 tx_notch_freq) {
      int x;
      int32_t val;
      bool spectral_scan_support, lbt_support;

      // Check input parameters 
      if((tx_notch_freq < LGW_MIN_NOTCH_FREQ) || (tx_notch_freq > LGW_MAX_NOTCH_FREQ)) {
        DEBUG_PRINTF("WARNING: FPGA TX notch frequency is out of range (%u - [%u..%u]), setting it to default (%u)\n", tx_notch_freq, LGW_MIN_NOTCH_FREQ, LGW_MAX_NOTCH_FREQ, LGW_DEFAULT_NOTCH_FREQ);
        tx_notch_freq = LGW_DEFAULT_NOTCH_FREQ;
      }

      // Get supported FPGA features 
      printf("INFO: FPGA supported features:");
      lgw_fpga_reg_r(LGW_FPGA_FEATURE, &val);
      _tx_notch_support = TAKE_N_BITS_FROM((uint8_t)val, 0, 1);
      if(_tx_notch_support == true) {
        printf(" [TX filter] ");
      }
      spectral_scan_support = TAKE_N_BITS_FROM((uint8_t)val, 1, 1);
      if(spectral_scan_support == true) {
        printf(" [Spectral Scan] ");
      }
      lbt_support = TAKE_N_BITS_FROM((uint8_t)val, 2, 1);
      if(lbt_support == true) {
        printf(" [LBT] ");
      }
      printf("\n");

      x = lgw_fpga_reg_w(LGW_FPGA_CTRL_INPUT_SYNC_I, 1);
      x |= lgw_fpga_reg_w(LGW_FPGA_CTRL_INPUT_SYNC_Q, 1);
      x |= lgw_fpga_reg_w(LGW_FPGA_CTRL_OUTPUT_SYNC, 0);
      if(x != LGW_REG_SUCCESS) {
        DEBUG_MSG("ERROR: Failed to configure FPGA TX synchro\n");
        return LGW_REG_ERROR;
      }

      // Required for Semtech AP2 reference design 
      x = lgw_fpga_reg_w(LGW_FPGA_CTRL_INVERT_IQ, 1);
      if(x != LGW_REG_SUCCESS) {
        DEBUG_MSG("ERROR: Failed to configure FPGA polarity\n");
        return LGW_REG_ERROR;
      }

      // Configure TX notch filter 
      if(_tx_notch_support == true) {
        _tx_notch_offset = (32E6 / (2 * tx_notch_freq)) - 64;
        x = lgw_fpga_reg_w(LGW_FPGA_NOTCH_FREQ_OFFSET, (int32_t)_tx_notch_offset);
        if(x != LGW_REG_SUCCESS) {
          DEBUG_MSG("ERROR: Failed to configure FPGA TX notch filter\n");
          return LGW_REG_ERROR;
        }

        // Readback to check that notch frequency is programmable 
        x = lgw_fpga_reg_r(LGW_FPGA_NOTCH_FREQ_OFFSET, &val);
        if(x != LGW_REG_SUCCESS) {
          DEBUG_MSG("ERROR: Failed to read FPGA TX notch frequency\n");
          return LGW_REG_ERROR;
        }
        if(val != _tx_notch_offset) {
          DEBUG_MSG("WARNING: TX notch filter frequency is not programmable (check your FPGA image)\n");
        } else {
          DEBUG_PRINTF("INFO: TX notch filter frequency set to %u (%i)\n", tx_notch_freq, _tx_notch_offset);
        }
      }

      return LGW_REG_SUCCESS;
    }*/

    private void FpgaRegisterWrite(FpgaRegisters register, Int32 value) => this.RegisterWrite(register, value, register.mux);

    private Int32 FpgaRegisterRead(FpgaRegisters register) => this.RegisterRead(register, register.mux);
  }
}
