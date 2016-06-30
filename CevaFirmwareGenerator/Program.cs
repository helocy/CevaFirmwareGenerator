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
    class CoffFile
    {
        private int mId;
        private string mName;
        private string mPath;

        public CoffFile(int id, string name, string path)
        {
            mId = id;
            mName = name;
            mPath = path;
        }

        public int GetId()
        {
            return mId;
        }

        public string GetName()
        {
            return mName;
        }

        public string GetPath()
        {
            return mPath;
        }

        public void Display()
        {
            Console.WriteLine("CoffFile: id={0}, name={1}, path={2}", mId, mName, mPath);
        }

        public void ExtractInformation()
        {
            System.Diagnostics.Process exep = new System.Diagnostics.Process();
            exep.StartInfo.FileName = "coffutil.exe";
            exep.StartInfo.Arguments = string.Format("-c -b {0} {1}", mName, mPath);
            Console.WriteLine("Execute: {0} {1}", exep.StartInfo.FileName, exep.StartInfo.Arguments);
            exep.StartInfo.CreateNoWindow = true;
            exep.StartInfo.UseShellExecute = false;
            exep.Start();
            exep.WaitForExit();
            exep.Close();

            System.Diagnostics.Process cmdp = new System.Diagnostics.Process();
            cmdp.StartInfo.FileName = "cmd.exe";
            cmdp.StartInfo.Arguments = "/C move *out process";
            cmdp.StartInfo.UseShellExecute = false;
            cmdp.StartInfo.CreateNoWindow = true;
            cmdp.Start();
            cmdp.WaitForExit();
            cmdp.Close();
        }
    }

    class ConfigFile
    {
        public static string CONFIG_FILE = "config.xml";
        public static string NODE_NAME = "Name";
        public static string NODE_ID = "Id";
        public static string NODE_PATH = "Path";
        public static string NODE_COFF = "CevaFirmwareGen/CoffFileList/CoffFile";
    }

    class Section
    {
        enum SectionType { CodeInt, CodeExt, DataInt, DataExt };
        int data;
        int code;
        int type;
    }

    class Firmware
    {

    }

    class Program
    {
        static string VERSION = "V0.0.1";
        static string PROCESS_DIRECTORY = "process";
        static string OUTPUT_DIRECTORY = "output";
        static ArrayList mCoffFileList;

        static void Prepare()
        {
            Directory.CreateDirectory(PROCESS_DIRECTORY);
            Directory.CreateDirectory(OUTPUT_DIRECTORY);
        }

        static void CreateCoffFileList()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(ConfigFile.CONFIG_FILE);
            XmlNodeList nodeList = xmlDoc.SelectNodes(ConfigFile.NODE_COFF);
            foreach(XmlNode node in nodeList) {
                int id = int.Parse(node.SelectSingleNode(ConfigFile.NODE_ID).InnerText.Trim());
                string name = node.SelectSingleNode(ConfigFile.NODE_NAME).InnerText.Trim();
                string path = node.SelectSingleNode(ConfigFile.NODE_PATH).InnerText.Trim();

                CoffFile coffFile = new CoffFile(id, name, path);
                coffFile.Display();
                mCoffFileList.Add(coffFile);
            }
        }

        static void ExtraCoffInformation()
        {
            foreach (CoffFile coffFile in mCoffFileList) {
                coffFile.ExtractInformation();
            }
        }

        static void Main(string[] args)
        {
            mCoffFileList = new ArrayList();

            Console.WriteLine("Ceva Firmware Generator {0} [for Rockchip platforms]", VERSION);
            Console.WriteLine("Copyright (C) 2016 Rockchip Electronics Co., Ltd.");
            Console.WriteLine("");

            /* Check necessary tools */
            if (System.IO.File.Exists(@"coffutil.exe") == false) {
                Console.WriteLine("coffutil.exe is necessary, please copy it from CEVA toolbox");
                return;
            }

            Prepare();
            CreateCoffFileList();
            ExtraCoffInformation();
           
            Console.ReadKey();
        }
    }
}
