using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CevaFirmwareGenerator
{
    class FirmwareGenerator
    {
        private Firmware mFirmware;
        private string mPath;
        private string mName;

        public FirmwareGenerator(Firmware firmware, string path, string name)
        {
            mPath = path;
            mName = name;
            mFirmware = firmware;
        }

        public bool Generate()
        {
            ArrayList extractFiles = mFirmware.GetExecutableFile().Extract();
            if (extractFiles.Count == 0)
            {
                Console.WriteLine("Cannot extract executable file");
                return false;
            }

            foreach(ExtractFile extractFile in extractFiles)
            {
                extractFile.Parse(mFirmware);
            }

            // Save firmware to file
            string path = string.Format("{0}/{1}", mPath, mName);
            FileStream f = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            mFirmware.Save(f);
            f.Close();

            return true;
        }
    }
}
