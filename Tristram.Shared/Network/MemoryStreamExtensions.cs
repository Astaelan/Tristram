using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tristram.Shared.Network
{
    public static class MemoryStreamExtensions
    {
        public static int GetRemaining(this MemoryStream pMemoryStream) { return (int)(pMemoryStream.Length - pMemoryStream.Position); }
        public static bool ReadVariable(this MemoryStream pMemoryStream, out ulong pValue)
        {
            pValue = 0;
            if (pMemoryStream.GetRemaining() == 0) return false;

            int length = 1;
            bool anotherByte = false;
            byte data = 0x00;
            while (length < 9 && length <= pMemoryStream.GetRemaining())
            {
                data = (byte)pMemoryStream.ReadByte();
                pValue |= (ulong)(data & 0x7F) << ((length - 1) * 7);
                anotherByte = (data & 0x80) == 0x80;
                if (!anotherByte) break;
                ++length;
            }
            if (length == 9)
            {
                if (length > pMemoryStream.GetRemaining()) return false;
                data = (byte)pMemoryStream.ReadByte();
                pValue |= (ulong)data << 56;
            }
            return true;
        }
        public static bool ReadVariable(this MemoryStream pMemoryStream, out uint pValue)
        {
            pValue = 0;
            ulong value = 0;
            if (!pMemoryStream.ReadVariable(out value)) return false;
            pValue = (uint)value;
            return true;
        }
        public static void WriteVariable(this MemoryStream pMemoryStream, ulong pValue)
        {
            bool empty = false;
            byte data = 0x00;
            int length = 1;
            while (!empty && length < 9)
            {
                data = (byte)(pValue & 0x7F);
                pValue >>= 7;
                empty = pValue == 0;
                if (!empty)
                {
                    data |= 0x80;
                    ++length;
                }
                pMemoryStream.WriteByte(data);
            }
            if (length == 9)
            {
                pMemoryStream.WriteByte((byte)(pValue & 0xFF));
            }
        }
        public static bool ReadFixed32(this MemoryStream pMemoryStream, out uint pValue)
        {
            pValue = 0;
            if (pMemoryStream.GetRemaining() < 4) return false;
            for (int offset = 0; offset < 4; ++offset)
            {
                pValue |= (uint)pMemoryStream.ReadByte() << (offset * 8);
            }
            return true;
        }
        public static void WriteFixed32(this MemoryStream pMemoryStream, uint pValue)
        {
            for (int offset = 0; offset < 4; ++offset)
            {
                pMemoryStream.WriteByte((byte)((pValue >> (offset * 8)) & 0xFF));
            }
        }
        public static bool ReadFixed64(this MemoryStream pMemoryStream, out ulong pValue)
        {
            pValue = 0;
            if (pMemoryStream.GetRemaining() < 8) return false;
            for (int offset = 0; offset < 8; ++offset)
            {
                pValue |= (ulong)pMemoryStream.ReadByte() << (offset * 8);
            }
            return true;
        }
        public static void WriteFixed64(this MemoryStream pMemoryStream, ulong pValue)
        {
            for (int offset = 0; offset < 8; ++offset)
            {
                pMemoryStream.WriteByte((byte)((pValue >> (offset * 8)) & 0xFF));
            }
        }
        public static bool ReadPrefixed(this MemoryStream pMemoryStream, out byte[] pValue)
        {
            pValue = null;
            ulong length = 0;
            if (!pMemoryStream.ReadVariable(out length)) return false;
            if (pMemoryStream.GetRemaining() < (int)length) return false;
            pValue = new byte[(int)length];
            pMemoryStream.Read(pValue, 0, (int)length);
            return true;
        }
        public static bool ReadPrefixed(this MemoryStream pMemoryStream, out MemoryStream pDestinationStream)
        {
            pDestinationStream = null;
            byte[] data = null;
            if (!pMemoryStream.ReadPrefixed(out data)) return false;
            pDestinationStream = new MemoryStream(data);
            return true;
        }
        public static void WritePrefixed(this MemoryStream pMemoryStream, byte[] pValue)
        {
            pMemoryStream.WriteVariable((uint)pValue.Length);
            pMemoryStream.Write(pValue, 0, pValue.Length);
        }
        public static void WritePrefixed(this MemoryStream pMemoryStream, MemoryStream pSourceStream)
        {
            pMemoryStream.WriteVariable((uint)pSourceStream.Length);
            pMemoryStream.Write(pSourceStream.ToArray(), 0, (int)pSourceStream.Length);
        }
        public static bool ReadPrefixed(this MemoryStream pMemoryStream, Encoding pEncoding, out string pValue)
        {
            pValue = null;
            byte[] data = null;
            if (!pMemoryStream.ReadPrefixed(out data)) return false;
            pValue = pEncoding.GetString(data);
            return true;
        }
        public static void WritePrefixed(this MemoryStream pMemoryStream, Encoding pEncoding, string pValue)
        {
            pMemoryStream.WritePrefixed(pEncoding.GetBytes(pValue));
        }
        public static bool ReadKey(this MemoryStream pMemoryStream, out EMessageKeyType pType, out uint pTag)
        {
            pType = EMessageKeyType.Variable;
            pTag = 0;
            uint key = 0;
            if (!pMemoryStream.ReadVariable(out key)) return false;
            pType = (EMessageKeyType)(key & 0x07);
            pTag = key >> 3;
            return true;
        }
        public static void WriteKey(this MemoryStream pMemoryStream, EMessageKeyType pType, uint pTag)
        {
            pMemoryStream.WriteVariable((pTag << 3) | (byte)pType);
        }
        public static bool ReadPackable(this MemoryStream pMemoryStream, IPackable pValue)
        {
            MemoryStream stream = null;
            if (!pMemoryStream.ReadPrefixed(out stream)) return false;
            if (!pValue.Read(stream)) return false;
            return true;
        }
        public static void WritePackable(this MemoryStream pMemoryStream, IPackable pValue)
        {
            MemoryStream stream = new MemoryStream();
            pValue.Write(stream);
            pMemoryStream.WritePrefixed(stream);
        }
    }
}
