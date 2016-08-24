using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CevaFirmwareGenerator
{
    class ExecutableFile
    {
        public static string NAME = "RKDSP";
        private string mPath;

        public ExecutableFile(string path)
        {
            mPath = path;
            if (System.IO.File.Exists(mPath) == false)
            {
                Console.WriteLine("Cannot find the executable file specified in ConfigFile");
                throw new System.IO.FileNotFoundException();
            }
        }

        public ArrayList Extract()
        {
            System.Diagnostics.Process exep = new System.Diagnostics.Process();
            exep.StartInfo.FileName = "coffutil.exe";
            exep.StartInfo.Arguments = string.Format("-c -b {0} {1}", NAME, mPath);
            Console.WriteLine("Execute: {0} {1}", exep.StartInfo.FileName, exep.StartInfo.Arguments);
            exep.StartInfo.CreateNoWindow = true;
            exep.StartInfo.UseShellExecute = false;
            exep.Start();
            exep.WaitForExit();
            exep.Close();

            string arg = string.Format("/C move *out {0}", Program.PROCESS_DIRECTORY);
            System.Diagnostics.Process cmdp = new System.Diagnostics.Process();
            cmdp.StartInfo.FileName = "cmd.exe";
            cmdp.StartInfo.Arguments = arg;
            cmdp.StartInfo.UseShellExecute = false;
            cmdp.StartInfo.CreateNoWindow = true;
            cmdp.Start();
            cmdp.WaitForExit();
            cmdp.Close();

            ArrayList extractFileList = new ArrayList();
            
            while (true)
            {
                string extractFilePath = string.Format("{0}/{1}{2}.out",
                                    Program.PROCESS_DIRECTORY, NAME, extractFileList.Count);
                if (System.IO.File.Exists(extractFilePath) == true)
                {
                    ExtractFile extractFile = new ExtractFile(extractFilePath);
                    extractFileList.Add(extractFile);
                }
                else
                {
                    break;
                }
            }

            return extractFileList;
        }
    }
}
