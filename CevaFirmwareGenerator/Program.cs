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
        static string PROCESS_DIRECTORY = "process";
        static string OUTPUT_DIRECTORY = "output";
        static ArrayList mCoffFileList;

        static void Main(string[] args)
        {
            mCoffFileList = new ArrayList();
            Directory.CreateDirectory(PROCESS_DIRECTORY);
            Directory.CreateDirectory(OUTPUT_DIRECTORY);

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

            Console.WriteLine("Start to generate ceva firmware:");
            Console.ReadKey();
        }
    }
}
