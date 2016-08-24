using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CevaFirmwareGenerator
{
    class PermanentImage : Image
    {
        public PermanentImage(int id, string name) 
            : base(id, name, new ImageType(ImageType.PERMANENT_STRING))
        {
            SetInternalCodeRange(0x0, 0x8000);
            SetInternalDataRange(0x0, 0x20000);
            SetExternalCodeRange(0x62000000, 0x6203ffff);
            SetExternalDataRange(0x62040000, 0x620bffff);
        }

        public static Image CreateFromXml(XmlNode node, int id, string name)
        {
            PermanentImage image = new PermanentImage(id, name);
            return image;
        }

        public override bool InCodeRange(UInt32 address)
        {
            if (mInternalCodeSection.InRange(address) || mExternalCodeSection.InRange(address))
                return true;
            else
                return false;
        }

        public override bool InDataRange(UInt32 address)
        {
            if (mInternalDataSection.InRange(address) || mExternalDataSection.InRange(address))
                return true;
            else
                return false;
        }

        public override void AddCode(UInt32 address, Byte code)
        {
            if (mInternalCodeSection.InRange(address))
                mInternalCodeSection.AddValue(address, code);
            else if (mExternalCodeSection.InRange(address))
                mExternalCodeSection.AddValue(address, code);
        }

        public override void AddData(UInt32 address, Byte data)
        {
            if (mInternalDataSection.InRange(address))
                mInternalDataSection.AddValue(address, data);
            else if (mExternalDataSection.InRange(address))
                mExternalDataSection.AddValue(address, data);
        }
    }
}
