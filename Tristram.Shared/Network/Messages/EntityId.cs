using System;
using System.IO;

namespace Tristram.Shared.Network.Messages
{
    public sealed class EntityId : IPackable
    {
        public const uint HighTag = 1;
        public const uint LowTag = 2;

        private ulong mHigh = 0;
        private ulong mLow = 0;

        public EntityId() { }
        public EntityId(ulong pLow) { mLow = pLow; }
        public EntityId(ulong pHigh, ulong pLow)
        {
            mHigh = pHigh;
            mLow = pLow;
        }

        public ulong High { get { return mHigh; } set { mHigh = value; } }
        public ulong Low { get { return mLow; } set { mLow = value; } }

        public bool Read(MemoryStream pStream)
        {
            EMessageKeyType type = EMessageKeyType.Variable;
            uint tag = 0;
            while (pStream.ReadKey(out type, out tag))
            {
                switch (tag)
                {
                    case HighTag:
                        if (!pStream.ReadFixed64(out mHigh)) return false;
                        break;
                    case LowTag:
                        if (!pStream.ReadFixed64(out mLow)) return false;
                        break;
                    default: return false;
                }
            }
            return true;
        }

        public void Write(MemoryStream pStream)
        {
            pStream.WriteKey(EMessageKeyType.Fixed64, HighTag);
            pStream.WriteFixed64(mHigh);
            pStream.WriteKey(EMessageKeyType.Fixed64, LowTag);
            pStream.WriteFixed64(mLow);
        }

        public void Reset()
        {
            mHigh = 0;
            mLow = 0;
        }
    }
}
