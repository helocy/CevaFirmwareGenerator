using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using System.Collections;

namespace CevaFirmwareGenerator
{
    class Program
    {
        public static string VERSION = "V0.4.2";
        public static string PROCESS_DIRECTORY = "process";
        public static string OUTPUT_DIRECTORY = "output";

        static void Prepare()
        {
            string arg = string.Format("/C rmdir /s /q {0} {1}", PROCESS_DIRECTORY, OUTPUT_DIRECTORY);
            System.Diagnostics.Process cmdp = new System.Diagnostics.Process();
            cmdp.StartInfo.FileName = "cmd.exe";
            cmdp.StartInfo.Arguments = arg;
            cmdp.StartInfo.UseShellExecute = false;
            cmdp.StartInfo.CreateNoWindow = true;
            cmdp.Start();
            cmdp.WaitForExit();
            cmdp.Close();

            Directory.CreateDirectory(PROCESS_DIRECTORY);
            Directory.CreateDirectory(OUTPUT_DIRECTORY);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Ceva Firmware Generator {0} [for Rockchip platforms]", VERSION);
            Console.WriteLine("Copyright (C) 2016 Rockchip Electronics Co., Ltd.");
            Console.WriteLine("-------------------------------------------------------");

            /* Check necessary tools */
            if (System.IO.File.Exists(@"coffutil.exe") == false)
            {
                Console.WriteLine("coffutil.exe is necessary, please copy it from CEVA toolbox");
                return;
            }

            Prepare();

            FirmwareGenerator generator = new FirmwareGenerator(ConfigParser.parse("FwConfig.xml"), OUTPUT_DIRECTORY, "rkdsp.bin");
            generator.Generate();
        }
    }
}
