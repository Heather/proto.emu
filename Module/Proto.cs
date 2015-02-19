using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using FSProtocolLib;
using Newtonsoft.Json.Converters;

using System.Runtime.InteropServices;

namespace FS.Emu.Module {
    [Serializable()]
    public struct Header {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] start;
        public byte sync;
        public byte address;
        public byte cmd;
        public byte stat;
        public byte len;
        }
    [Serializable()]
    public struct SF21X {
        public Header header;
        [MarshalAs(UnmanagedType.Struct)]
        public RECORDSF21V[] HourArchives;
        public RECORDSF21V[] DayArchives;
        }

    public static class Cfg {
        public static SF21X sf21 = new SF21X() {
            header = new Header {
                start = new byte[3] {
                    0xFF,
                    0xFF,
                    0xFF
                },
                sync = 0xAA,
                address = 0x01,
                cmd = 0x90,
                stat = 0x00,
                len = 0x02
            },
            HourArchives = new RECORDSF21V[2] {
                new RECORDSF21V() {
                    BaseIndex = 1,
                    FlowRate = 1.1F
                },
                new RECORDSF21V() {
                    BaseIndex = 1,
                    FlowRate = 1.1F
                }
            },
            DayArchives = new RECORDSF21V[2] {
                new RECORDSF21V() {
                    BaseIndex = 2,
                    FlowRate = 2.2F
                },
                new RECORDSF21V() {
                    BaseIndex = 2,
                    FlowRate = 2.2F
                }
            }
        };

        public static void GetJson() {
            JsonSerializer serializer = new JsonSerializer();
            sf21 = JsonConvert.DeserializeObject<SF21X>(File.ReadAllText("json.json"));
            }

        public static void SetJson() {
            JsonSerializer serializer = new JsonSerializer();

            serializer.NullValueHandling = NullValueHandling.Include;
            serializer.Formatting = Formatting.Indented;

            using (StreamWriter sw = new StreamWriter("json.json"))
            using (JsonWriter writer = new JsonTextWriter(sw)) {
                serializer.Serialize(writer, sf21);
                }
            }
        }
    }
