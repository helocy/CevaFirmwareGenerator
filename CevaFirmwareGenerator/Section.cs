using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CevaFirmwareGenerator
{
    public enum SectionType
    {
        CodeInt,
        CodeExt,
        DataInt,
        DataExt
    };

    public class Section
    {
        public static int BIT_ALIGN = 128;
        public static int BYTE_ALIGN = BIT_ALIGN / 8;
        public static UInt32 ADDRESS_MASK = 0x7ffff;      // Section max size 1MB
        public static UInt32 INVALID_ADDRESS = 0xffffffff;

        private int mValid;
        private SectionType mType;
        private Byte[] mData;

        private UInt32 mStartAddress;
        private UInt32 mEndAddress;

        private UInt32 mValueStart;
        private UInt32 mValueEnd;

        Section()
        {

        }

        public Section(SectionType type)
        {
            mValueStart = mValueEnd = mStartAddress = mEndAddress = INVALID_ADDRESS;
            mType = type;
            mValid = 0;
        }

        public int Save(FileStream file, int offset)
        {
            int fileOffset = offset;

            file.Seek(fileOffset, SeekOrigin.Begin);
            Console.WriteLine("    Writing section: type={0}, size={1}, address=0x{2:X}", mType, GetValueCount(), mStartAddress);

            // 4 bytes type
            Byte[] type = BitConverter.GetBytes((int)mType);
            file.Write(type, 0, type.Count());
            fileOffset += type.Count();

            // 4 bytes size
            Byte[] size = BitConverter.GetBytes(GetValueCount());
            file.Write(size, 0, size.Count());
            fileOffset += size.Count();

            // 4bytes load address
            Byte[] address = BitConverter.GetBytes(mStartAddress);
            file.Write(address, 0, address.Count());
            fileOffset += address.Count();

            for (int idx = 0; idx < GetValueCount(); idx += BYTE_ALIGN)
            {
                Byte[] values = new Byte[BYTE_ALIGN];
                Array.Copy(mData, idx, values, 0, BYTE_ALIGN);
                if (mType == SectionType.CodeExt || mType == SectionType.CodeInt)
                    Array.Reverse(values);

                file.Write(values, 0, values.Count());
                fileOffset += values.Count();
            }

            return fileOffset - offset;
        }

        public UInt32 GetValueCount()
        {
            UInt32 align = (UInt32)BYTE_ALIGN;

            if (mValid == 0)
                return 0;

            if (mValueEnd == INVALID_ADDRESS)
                return 0;
            else
                return align * (((ADDRESS_MASK & (mValueEnd + 1)) + align - 1) / align);
        }

        public void AddValue(UInt32 address, Byte value)
        {
            if (value == 0)
                return;

            if (mValueStart == 0xffffffff)
                mValueStart = mValueEnd = address;

            if (address < mValueStart)
                mValueStart = address;
            if (address > mValueEnd)
                mValueEnd = address;

            mData.SetValue(value, ADDRESS_MASK & address);
        }

        public void SetRange(UInt32 start, UInt32 end)
        {
            if (start >= end)
            {
                Console.WriteLine("Invalid section range");
                return;
            }

            mStartAddress = start;
            mEndAddress = end;

            mData = new Byte[end - start];
            mValid = 1;
        }

        public bool InRange(UInt32 address)
        {
            if (address >= mStartAddress && address <= mEndAddress)
                return true;
            else
                return false;
        }
    }
}
