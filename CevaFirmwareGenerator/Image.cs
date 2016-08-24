using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CevaFirmwareGenerator
{
    public abstract class Image
    {
        public static int NAME_SIZE = 32;
        public static int MAX_SECTION = 4;

        private int mId;
        private string mName;
        private ImageType mType;

        protected Section mInternalCodeSection;
        protected Section mExternalCodeSection;
        protected Section mInternalDataSection;
        protected Section mExternalDataSection;

        public Image()
        {
            mInternalCodeSection = new Section(SectionType.CodeInt);
            mExternalCodeSection = new Section(SectionType.CodeExt);
            mInternalDataSection = new Section(SectionType.DataInt);
            mExternalDataSection = new Section(SectionType.DataExt);
        }

        public Image(int id, string name, ImageType type)
        {
            mId = id;
            mName = name;
            mType = type;

            mInternalCodeSection = new Section(SectionType.CodeInt);
            mExternalCodeSection = new Section(SectionType.CodeExt);
            mInternalDataSection = new Section(SectionType.DataInt);
            mExternalDataSection = new Section(SectionType.DataExt);
        }

        public abstract bool InCodeRange(UInt32 address);
        public abstract bool InDataRange(UInt32 address);
        public abstract void AddCode(UInt32 address, Byte code);
        public abstract void AddData(UInt32 address, Byte data);

        public int GetImageByteCount()
        {
            return 0;
        }

        public int Save(FileStream file, int offset)
        {
            int fileOffset = offset;

            file.Seek(fileOffset, SeekOrigin.Begin);
            Console.WriteLine("  Writing image: name={0}, type={1}", mName, mType.GetName());

            // 4 bytes id
            Byte[] id = BitConverter.GetBytes(mId);
            file.Write(id, 0, id.Count());
            fileOffset += id.Count();

            // 32 bytes name
            Byte[] name = new Byte[NAME_SIZE];
            Encoding.ASCII.GetBytes(mName).CopyTo(name, 0);
            file.Write(name, 0, name.Count());
            fileOffset += name.Count();

            // 4 bytes section count
            Byte[] sectionCount = BitConverter.GetBytes(MAX_SECTION);
            file.Write(sectionCount, 0, sectionCount.Count());
            fileOffset += sectionCount.Count();

            int size = mInternalCodeSection.Save(file, fileOffset);
            fileOffset += size;

            size = mInternalDataSection.Save(file, fileOffset);
            fileOffset += size;

            size = mExternalCodeSection.Save(file, fileOffset);
            fileOffset += size;

            size = mExternalDataSection.Save(file, fileOffset);
            fileOffset += size;

            return fileOffset - offset;
        }

        public void SetInternalCodeRange(UInt32 start, UInt32 end)
        {
            mInternalCodeSection.SetRange(start, end);
        }

        public void SetExternalCodeRange(UInt32 start, UInt32 end)
        {
            mExternalCodeSection.SetRange(start, end);
        }

        public void SetInternalDataRange(UInt32 start, UInt32 end)
        {
            mInternalDataSection.SetRange(start, end);
        }

        public void SetExternalDataRange(UInt32 start, UInt32 end)
        {
            mExternalDataSection.SetRange(start, end);
        }

        public ImageType GetImageType()
        {
            return mType;
        }
    }
}
