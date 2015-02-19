using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.IO.Ports;
using System.Threading;

using System.Reflection;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

using FS.Emu.Model;
using FSProtocolLib;

namespace FS.Emu.Module {

    [StructLayout(LayoutKind.Sequential)]
    internal struct COMMTIMEOUTS {
        public int ReadIntervalTimeout;
        public int ReadTotalTimeoutMultiplier;
        public int ReadTotalTimeoutConstant;
        public int WriteTotalTimeoutMultiplier;
        public int WriteTotalTimeoutConstant;
    }
    public class SerialReader : IDisposable {
        private SerialPort serialPort;
        MemoryStream mem = new MemoryStream();

        private Queue<byte> recievedData = new Queue<byte>();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool SetCommTimeouts(SafeFileHandle hFile, ref COMMTIMEOUTS lpCommTimeouts);

        private void open() {
            try {
                serialPort.StopBits = StopBits.One;
                serialPort.RtsEnable = true;
                serialPort.DtrEnable = true;
                serialPort.Open();
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                return;
            }
            unsafe {
                object stream = typeof(SerialPort).GetField("internalSerialStream", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(serialPort);
                var handle = (SafeFileHandle)stream.GetType().GetField("_handle", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(stream);

                COMMTIMEOUTS communications_timeouts;
                communications_timeouts.ReadIntervalTimeout = 5;
                communications_timeouts.ReadTotalTimeoutConstant = 1000;
                communications_timeouts.ReadTotalTimeoutMultiplier = -1;
                communications_timeouts.WriteTotalTimeoutConstant = -1;
                communications_timeouts.WriteTotalTimeoutMultiplier = 5;

                SetCommTimeouts(handle, ref communications_timeouts);
            }
            Thread process2 = new Thread(new ThreadStart(() => {
                while (true) {
                    int lth = 256;
                    if (lth > 0) {
                        byte[] data = new byte[lth];
                        int x;
                        try {
                            x = serialPort.Read(data, 0, lth);
                        }
                        catch (TimeoutException) {
                            continue;
                        }
                        (new Task<Task>(async () => {
                            await mem.WriteAsync(data, 0, x);
                            if (mem.Length >= 11) {
                                byte[] d = new byte[mem.Length];
                                mem.Position = 0;
                                await mem.ReadAsync(d, 0, d.Length);
                                if ((  d[0] == 0xFF
                                    && d[1] == 0xFF
                                    && d[2] == 0xFF
                                    && d[3] == 0xFF
                                    && d[4] == 0xFF
                                    && d[5] == 0xFF
                                    && d[6] == 0x55)) {
                                    d.ToList().ForEach(b => recievedData.Enqueue(b));
                                    processData();
                                }
                                mem.SetLength(0);
                            }
                        }
                        )).Start();
                    }
                }
            }));
            var zz = (new Func<int>(() => { return 1; }))();
            process2.Start();
        }

        public SerialReader() {
            serialPort = new SerialPort();
            open();
        }
        public SerialReader(string COM) {
            serialPort = new SerialPort(COM);
            open();
        }
        public SerialReader(string COM, int bRate) {
            serialPort = new SerialPort(COM, bRate);
            open();
        }
        static string ByteArrayToHexViaLookup(byte[] bytes) {
            string[] hexStringTable = new string[] {
                "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "0A", "0B", "0C", "0D", "0E", "0F",
                "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "1A", "1B", "1C", "1D", "1E", "1F",
                "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "2A", "2B", "2C", "2D", "2E", "2F",
                "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "3A", "3B", "3C", "3D", "3E", "3F",
                "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "4A", "4B", "4C", "4D", "4E", "4F",
                "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "5A", "5B", "5C", "5D", "5E", "5F",
                "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "6A", "6B", "6C", "6D", "6E", "6F",
                "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "7A", "7B", "7C", "7D", "7E", "7F",
                "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "8A", "8B", "8C", "8D", "8E", "8F",
                "90", "91", "92", "93", "94", "95", "96", "97", "98", "99", "9A", "9B", "9C", "9D", "9E", "9F",
                "A0", "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8", "A9", "AA", "AB", "AC", "AD", "AE", "AF",
                "B0", "B1", "B2", "B3", "B4", "B5", "B6", "B7", "B8", "B9", "BA", "BB", "BC", "BD", "BE", "BF",
                "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "CA", "CB", "CC", "CD", "CE", "CF",
                "D0", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "DA", "DB", "DC", "DD", "DE", "DF",
                "E0", "E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8", "E9", "EA", "EB", "EC", "ED", "EE", "EF",
                "F0", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "FA", "FB", "FC", "FD", "FE", "FF",
            };
            StringBuilder result = new StringBuilder(bytes.Length * 2);
            result.Append("0x");
            foreach (byte b in bytes) {
                result.Append(hexStringTable[b]);
            }
            result.Append(Environment.NewLine);
            return result.ToString();
        }
        public static byte[] StringToByteArray(string hex) {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
        void processData() {
            Func<int, IEnumerable<byte>> next = (z) => {
                return Enumerable.Range(0, z).Select(i => recievedData.Dequeue());
            };

            /* header + addr */
            next(8).ToArray();
            var func = next(1).FirstOrDefault();

            /* stat */
            next(1).ToArray();
            var getlen = next(1).FirstOrDefault(); /* How much to read */

            var lambdaRead = new Action<int>((t) => {
                int al = 0;
                switch (t) {
                    case 1: al = Cfg.sf21.HourArchives.Length; break;
                    case 2: al = Cfg.sf21.DayArchives.Length; break;
                }
                int len = Math.Min(getlen, al);
                if (len != 0) {
                    var headsize = Marshal.SizeOf(Cfg.sf21.header);
                    int asz = 0;
                    switch (t) {
                        case 1: asz = Marshal.SizeOf(Cfg.sf21.HourArchives[0]); break;
                        case 2: asz = Marshal.SizeOf(Cfg.sf21.DayArchives[0]); break;
                    }
                    var bufsize = len * asz;
                    var size = headsize + bufsize;

                    var processBuffer = new Action<int, int>((l, frm) => {
                        var rbufsize = l * asz;
                        var sz = headsize + rbufsize;
                        byte[] buff = new byte[sz];
                        var headr = BlockConvert.RawSerialize(Cfg.sf21.header);
                        Buffer.BlockCopy(headr, 0, buff, 0, headsize);
                        int asize = 0;
                        using (MemoryStream ms = new MemoryStream()) {
                            using (BinaryWriter bw = new BinaryWriter(ms)) {
                                for (int j = 0; j < l; j++) {
                                    switch (t) {
                                        case 1: bw.Write(BlockConvert.RawSerialize(Cfg.sf21.HourArchives[frm + j])); break;
                                        case 2: bw.Write(BlockConvert.RawSerialize(Cfg.sf21.DayArchives[frm + j])); break;
                                    }
                                }
                                var archives = ms.ToArray();
                                asize = archives.Length;
                                Buffer.BlockCopy(archives, 0, buff, headsize, asize);
                            }
                        }
                        var crc = BitConverter.GetBytes((ushort)Checksumm.CRC16(buff));
                        var newsize = headsize + asize + 2;
                        byte[] crcbuff = new byte[newsize];
                        Buffer.BlockCopy(buff, 0, crcbuff, 0, sz);
                        Buffer.BlockCopy(crc, 0, crcbuff, sz, 2);
                        serialPort.Write(crcbuff, 0, newsize);
                    });

                    if (size < 253) {
                        processBuffer(len, 0);
                    }
                    else {
                        int iterations = (int)Math.Ceiling(size / 253.0);
                        int l = (int)(Math.Ceiling((253.0 - headsize) / asz) - 1);
                        for (int i = 0; i < iterations; i++) {
                            if (len > l) {
                                processBuffer(l, l * i);
                                len -= l;
                            }
                            else {
                                processBuffer(len, l * i);
                            }
                        }
                    }
                    recievedData.Clear();
                }
            });

            switch (func) {
                case 0x90:
                case 0x91:
                    Int16 baseINdex = BitConverter.ToInt16(next(2).ToArray(), 0);
                    switch (baseINdex) {
                        case 0:
                            /* instant */
                            break;
                        case 1: lambdaRead(1); break;
                        case 2: lambdaRead(2); break;
                    }
                    break;
                case 0x00:
                    break;
            }
        }
        public void Change(string COM) {
            serialPort.Dispose();
            serialPort = new SerialPort(COM);
        }
        public void Change(string COM, int bRate) {
            if (serialPort.IsOpen) {
                serialPort.Close();
            }
            serialPort.Dispose();
            serialPort = null;
            serialPort = new SerialPort(COM, bRate);
            open();
        }
        public void Dispose() {
            if (serialPort != null) {
                if (serialPort.IsOpen) {
                    serialPort.Close();
                }
                serialPort.Dispose();
            }
        }
    }
}