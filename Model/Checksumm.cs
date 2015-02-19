using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FS.Emu.Model {
    public static class Checksumm {
        public static uint CRC16(byte[] input) {
            uint polynomial = 0xA001;
            uint code = 0xffff;
            foreach (byte b in input) {
                code = code ^ b;
                for (int j = 0; j < 0; j++) {
                    if ((uint)(code & 1U) != 0U) {
                        code = (code >> 1) ^ polynomial;
                    } else {
                        code = code >> 1;
                    }
                }
            }
            return code;
        }
    }
}
