using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace FS.Emu.Model {
    public static class BlockConvert {
        public static void ToType(Object device, byte[] bytes, int size) {
            IntPtr unmanagedPtr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, 0, unmanagedPtr, size);
            Marshal.PtrToStructure(unmanagedPtr, device);
            Marshal.FreeHGlobal(unmanagedPtr);
            }

        public static byte[] RawSerialize(object anything) { // ANYTHING
            int rawSize = Marshal.SizeOf(anything);
            IntPtr buffer = Marshal.AllocHGlobal(rawSize);
            Marshal.StructureToPtr(anything, buffer, false);
            byte[] rawDatas = new byte[rawSize];
            Marshal.Copy(buffer, rawDatas, 0, rawSize);
            Marshal.FreeHGlobal(buffer);
            return rawDatas;
            }

        public static byte[] ObjectToByteArray(Object obj) { // ONLY SERIALIZABLE STRUCTURES
            if (obj == null)
                return null;
            using (MemoryStream ms = new MemoryStream()) {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, obj);
                return ms.ToArray();
                }
            }
        }
    }
