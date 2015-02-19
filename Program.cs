using FS.Emu.Module;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using MSScriptControl;

namespace FS.Emu {
    class Program {
        static void Main(string[] args) {

            Cfg.SetJson();
            Cfg.GetJson();

            string recieved = "";
            int brate = 115200;
            using (SerialReader sr = new SerialReader("COM2", brate)) { // defaults
                while (recieved != "q") {
                    recieved = Console.ReadLine();
                    var com = Regex.Match(recieved, "^COM");
                    if (com.Success) {
                        var comSplit = recieved.Split(' ');
                        switch (comSplit.Length) {
                            case 1:
                                Console.WriteLine("Changing to {0}", comSplit[0]);
                                sr.Change(comSplit[0], brate);
                                break;
                            case 2:
                                if (Int32.TryParse(comSplit[1], out brate)) {
                                    Console.WriteLine("Changing to {0}, {1}", comSplit[0], comSplit[1]);
                                    sr.Change(comSplit[0], brate);
                                    }
                                break;
                            default: goto case 2;
                            }
                        }
                    }
                }
            }
        }
    }
