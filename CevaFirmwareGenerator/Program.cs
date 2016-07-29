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
    class Utils
    {
        public static void BytesToArrayList(ArrayList list, Byte[] bytes)
        {
            foreach (Byte b in bytes)
                list.Add(b);
        }
    }
    class CoffFile
    {
        private int mId;
        private string mName;
        private string mPath;
        private ArrayList mExtractFileList;

        public CoffFile(int id, string name, string path)
        {
            mId = id;
            mName = name;
            mPath = path;

            mExtractFileList = new ArrayList();
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

            string arg = string.Format("/C move *out {0}", Program.PROCESS_DIRECTORY);
            System.Diagnostics.Process cmdp = new System.Diagnostics.Process();
            cmdp.StartInfo.FileName = "cmd.exe";
            cmdp.StartInfo.Arguments = arg;
            cmdp.StartInfo.UseShellExecute = false;
            cmdp.StartInfo.CreateNoWindow = true;
            cmdp.Start();
            cmdp.WaitForExit();
            cmdp.Close();

            while (true)
            {
                string extractFilePath = string.Format("{0}/{1}{2}.out",
                                    Program.PROCESS_DIRECTORY, mName, mExtractFileList.Count);
                if (System.IO.File.Exists(extractFilePath) == true)
                {
                    ExtractFile extractFile = new ExtractFile(mName, extractFilePath);
                    Console.WriteLine("Add extact information file: {0}", extractFilePath);
                    mExtractFileList.Add(extractFile);
                }
                else
                {
                    break;
                }
            }
        }

        public Image CreateImage()
        {
            Image img = new Image(mId, mName);

            foreach(ExtractFile extractFile in mExtractFileList)
            {
                Section[] sections = extractFile.Parse();
                foreach(Section section in sections)
                {
                    if (section.IsEmpty() == false)
                        img.AddSection(section);
                }
            }

            return img;
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

    class ExtractFile
    {
        private string mPath;
        private string mParent;

        public ExtractFile(string parent, string path)
        {
            mParent = parent;
            mPath = path;
        }

        public string GetPath()
        {
            return mPath;
        }

        public Section[] Parse()
        {
            Section[] sections = { new Section(SectionType.CodeInt),
                                   new Section(SectionType.CodeExt),
                                   new Section(SectionType.DataInt),
                                   new Section(SectionType.DataExt)};

            StreamReader sr = new StreamReader(mPath, Encoding.Default);

            while (sr.EndOfStream == false)
            {
                string line = sr.ReadLine();
                int idx;

                // Parse line like: C:00000040 32
                char[] split = { ':', ' ' };
                string[] parts = line.Split(split);
                Char type = Convert.ToChar(parts[0]);
                UInt32 address = UInt32.Parse(parts[1], System.Globalization.NumberStyles.HexNumber);
                Byte value = Byte.Parse(parts[2], System.Globalization.NumberStyles.HexNumber);

                //Console.WriteLine("Parse a line: type={0}, address={1}, value={2}", type, address, value);
                //Console.ReadKey();

                if (type.Equals('C'))
                {
                    // Code
                    if (address <= Section.MAX_DATA_SIZE)
                    {
                        // Ceva internal address
                        idx = (int)SectionType.CodeInt;
                        sections[idx].AddData(address, value);
                    }
                    else
                    {
                        // Ceva external address
                        idx = (int)SectionType.CodeExt;
                        sections[idx].AddData(address, value);
                    }
                }
                else
                {
                    // Data
                    if (address <= Section.MAX_DATA_SIZE)
                    {
                        // Ceva internal address
                        idx = (int)SectionType.DataInt;
                        sections[idx].AddData(address, value);
                    }
                    else
                    {
                        // Ceva external address
                        idx = (int)SectionType.DataExt;
                        sections[idx].AddData(address, value);
                    }

                }
            }

            return sections;
        }
    }

    enum SectionType
    {
        CodeInt,
        CodeExt,
        DataInt,
        DataExt
    };

    class Section
    {
        public static UInt32 MAX_DATA_SIZE = 0x20000;
        public static UInt32 ADDRESS_MASK = 0x1ffff;
        public static int BIT_ALIGN = 128;
        public static int BYTE_ALIGN = BIT_ALIGN / 8;
        public static UInt32 INVALID_ADDRESS = 0xffffffff;

        private string mParent;
        private SectionType mType;
        private Byte[] mData;
        private UInt32 mStartAddress;
        private UInt32 mEndAddress;

        private ArrayList mBytes;

        private void SectionHeaderBytes()
        {
            // 4 bytes type
            Byte[] type = BitConverter.GetBytes((int)mType);
            Utils.BytesToArrayList(mBytes, type);

            // 4 bytes size
            Byte[] size = BitConverter.GetBytes(GetDataCount());
            Utils.BytesToArrayList(mBytes, size);

            // 4bytes load address
            Byte[] address = BitConverter.GetBytes((~ADDRESS_MASK) & mStartAddress);
            Utils.BytesToArrayList(mBytes, address);
        }

        public Section()
        {
            mStartAddress = mEndAddress = INVALID_ADDRESS;
            mData = new Byte[MAX_DATA_SIZE];
            mBytes = new ArrayList();
        }

        public Section(SectionType type)
        {
            mStartAddress = mEndAddress = INVALID_ADDRESS;
            mType = type;
            mData = new Byte[MAX_DATA_SIZE];
            mBytes = new ArrayList();
        }

        public void SetType(SectionType type)
        {
            mType = type;
        }

        public UInt32 GetStartAddress()
        {
            return (~ADDRESS_MASK) & mStartAddress;
        }

        public UInt32 GetEndAddress()
        {
            return mEndAddress;
        }

        public UInt32 GetDataCount()
        {
            UInt32 align = (UInt32)BYTE_ALIGN;

            if (mEndAddress == INVALID_ADDRESS)
                return 0;
            else
                return align * (((ADDRESS_MASK & mEndAddress) + align - 1) / align);
        }
        
        public new SectionType GetType()
        {
            return mType;
        }

        public string GetTypeString()
        {
            switch (mType)
            {
                case SectionType.CodeInt:
                    return "CodeInt";
                case SectionType.CodeExt:
                    return "CodeExt";
                case SectionType.DataInt:
                    return "DataInt";
                case SectionType.DataExt:
                    return "DataExt";
                default:
                    return "unknown";
            }
        }

        public void AddData(UInt32 address, Byte data)
        {
            if (data == 0)
                return;

            if (mEndAddress == 0xffffffff)
                mStartAddress = mEndAddress = address;

            if (address < mStartAddress)
                mStartAddress = address;
            if (address > mEndAddress)
                mEndAddress = address;

            mData.SetValue(data, ADDRESS_MASK & address);
        }

        public void SetParent(string parent)
        {
            mParent = parent;
        }

        public Boolean IsEmpty()
        {
            if (GetDataCount() == 0)
                return true;
            else
                return false;
        }

        public void Dump()
        {
            string path = string.Format("{0}/{1}_{2}.bin", Program.PROCESS_DIRECTORY, mParent, GetTypeString());
            FileStream f = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            for (int idx = 0; idx < GetDataCount(); idx += BYTE_ALIGN)
            {
                Byte[] data = new Byte[BYTE_ALIGN];
                Array.Copy(mData, idx, data, 0, BYTE_ALIGN);
                if (mType == SectionType.CodeExt || mType == SectionType.CodeInt)
                    Array.Reverse(data);
                foreach (Byte b in data)
                    f.WriteByte(b);
            }
            f.Close();
        }

        public ArrayList GetBytesArray()
        {
            if (mBytes.Count == 0)
            {
                SectionHeaderBytes();

                for (int idx = 0; idx < GetDataCount(); idx += BYTE_ALIGN)
                {
                    Byte[] data = new Byte[BYTE_ALIGN];
                    Array.Copy(mData, idx, data, 0, BYTE_ALIGN);
                    if (mType == SectionType.CodeExt || mType == SectionType.CodeInt)
                        Array.Reverse(data);

                    mBytes.AddRange(data);
                }
            }

            Console.WriteLine("{0} image section {1} has {2} bytes", mParent, mType, mBytes.Count);
            return mBytes;
        }

        public int GetBytesCount()
        {
            ArrayList bytes = GetBytesArray();

            return bytes.Count;
        }
    }

    class Image
    {
        public static int NAME_SIZE = 32;

        private int mId;
        private string mName;
        private ArrayList mSections;
        private ArrayList mBytes;

        private void ImageHeaderBytes()
        {
            // 4 bytes id
            Byte[] id = BitConverter.GetBytes(mId);
            Utils.BytesToArrayList(mBytes, id);

            // 32bytes name
            Byte[] name = new Byte[NAME_SIZE];
            Encoding.ASCII.GetBytes(mName).CopyTo(name, 0);
            Utils.BytesToArrayList(mBytes, name);

            // 4 bytes section count
            Byte[] sectionCount = BitConverter.GetBytes(mSections.Count);
            Utils.BytesToArrayList(mBytes, sectionCount);
        }

        public Image(int id, string name)
        {
            mId = id;
            mName = name;
            mSections = new ArrayList();
            mBytes = new ArrayList();
        }

        public string GetName()
        {
            return mName;
        }

        public void AddSection(Section section)
        {
            section.SetParent(mName);
            mSections.Add(section);
            section.Dump();
            Console.WriteLine("Add section to image: type={0}, start=0x{1:x8}, count={2}",
                        section.GetType(), section.GetStartAddress(), section.GetDataCount());
        }

        public ArrayList GetBytesArray()
        {
            if (mBytes.Count == 0)
            {
                ImageHeaderBytes();

                foreach (Section section in mSections)
                {
                    ArrayList sectionBytes = section.GetBytesArray();
                    mBytes.AddRange(sectionBytes);
                }
            }

            return mBytes;
        }

        public int GetBytesCount()
        {
            ArrayList bytes = GetBytesArray();

            return bytes.Count;
        }
    }

    class Firmware
    {
        public static string FIRMWARE_NAME = "ceva.bin";
        public static string FIRMWARE_MAGIC = "#RKCPCEVAFW#";
        public static string FIRMWARE_VERSION = "V0.1.4";

        public static int MAGIC_SIZE = 16;
        public static int VERSION_SIZE = 16;
        public static int RESERVE_SIZE = 60;
        public static int MAX_IMAGES = 8;

        private ArrayList mImages;
        private ArrayList mBytes;

        private void FirmwareHeaderBytes()
        {
            Byte[] magic = new Byte[MAGIC_SIZE];
            Byte[] version = new Byte[VERSION_SIZE];

            Encoding.ASCII.GetBytes(FIRMWARE_MAGIC).CopyTo(magic, 0);
            Encoding.ASCII.GetBytes(FIRMWARE_VERSION).CopyTo(version, 0);

            Utils.BytesToArrayList(mBytes, magic);
            Utils.BytesToArrayList(mBytes, version);

            Byte[] imageCount = BitConverter.GetBytes(mImages.Count);
            Utils.BytesToArrayList(mBytes, imageCount);

            int[] imageSize = new int[MAX_IMAGES];
            for (int i = 0; i < mImages.Count; i++)
            {
                Image image = (Image)(mImages.ToArray()[i]);
                imageSize[i] = image.GetBytesCount();
                Console.WriteLine("Image {0} bytes count {1}", image.GetName(), image.GetBytesCount());
            }
            foreach (int size in imageSize)
            {
                Byte[] sizeBytes = BitConverter.GetBytes(size);
                Utils.BytesToArrayList(mBytes, sizeBytes);
            }

            Byte[] reserve = new Byte[RESERVE_SIZE];
            Utils.BytesToArrayList(mBytes, reserve);
        }

        public Firmware()
        {
            mImages = new ArrayList();
            mBytes = new ArrayList();
        }

        public void AddImage(Image image)
        {
            mImages.Add(image);
        }

        public Byte[] GetBytes()
        {
            if (mBytes.Count == 0)
            {
                FirmwareHeaderBytes();

                foreach (Image image in mImages)
                {
                    ArrayList imageBytes = image.GetBytesArray();
                    mBytes.AddRange(imageBytes);
                }
            }
            return mBytes.OfType<Byte>().ToArray();
        }

        public Boolean SaveToFile()
        {
            string path = string.Format("{0}/{1}", Program.OUTPUT_DIRECTORY, FIRMWARE_NAME);
            FileStream f = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            Byte[] firmwareBytes = GetBytes();
            f.Write(firmwareBytes, 0, firmwareBytes.Count());

            f.Close();
            return true;
        }
    }

    class Program
    {
        public static string VERSION = "V0.1.4";
        public static string PROCESS_DIRECTORY = "process";
        public static string OUTPUT_DIRECTORY = "output";

        static ArrayList mCoffFileList;
        static Firmware mFirmware;

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

        static void CreateCoffFileList()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(ConfigFile.CONFIG_FILE);
            XmlNodeList nodeList = xmlDoc.SelectNodes(ConfigFile.NODE_COFF);
            foreach(XmlNode node in nodeList)
            {
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
            foreach (CoffFile coffFile in mCoffFileList)
                coffFile.ExtractInformation();
        }
        
        static void CreateFirmware()
        {
            mFirmware = new Firmware();

            foreach(CoffFile coffFile in mCoffFileList)
            {
                Image img = coffFile.CreateImage();

                mFirmware.AddImage(img);
            }
        }

        static void SaveFirmware()
        {
            mFirmware.SaveToFile();
        }

        static void Main(string[] args)
        {
            mCoffFileList = new ArrayList();

            Console.WriteLine("Ceva Firmware Generator {0} [for Rockchip platforms]", VERSION);
            Console.WriteLine("Copyright (C) 2016 Rockchip Electronics Co., Ltd.");
            Console.WriteLine("");

            /* Check necessary tools */
            if (System.IO.File.Exists(@"coffutil.exe") == false)
            {
                Console.WriteLine("coffutil.exe is necessary, please copy it from CEVA toolbox");
                return;
            }

            Prepare();
            CreateCoffFileList();
            ExtraCoffInformation();
            CreateFirmware();
            SaveFirmware();
           
            Console.ReadKey();
        }
    }
}
