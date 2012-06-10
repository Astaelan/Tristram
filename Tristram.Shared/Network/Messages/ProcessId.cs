using System;
using System.IO;

namespace Tristram.Shared.Network.Messages
{
    public sealed class ProcessId : IPackable
    {
        public const uint LabelTag = 1;
        public const uint EpochTag = 2;

        private uint mLabel = 0;
        private uint mEpoch = 0;

        public ProcessId() { }
        public ProcessId(uint pLabel, uint pEpoch)
        {
            mLabel = pLabel;
            mEpoch = pEpoch;
        }

        public uint Label { get { return mLabel; } set { mLabel = value; } }
        public uint Epoch { get { return mEpoch; } set { mEpoch = value; } }

        public bool Read(MemoryStream pStream)
        {
            EMessageKeyType type = EMessageKeyType.Variable;
            uint tag = 0;
            while (pStream.ReadKey(out type, out tag))
            {
                switch (tag)
                {
                    case LabelTag:
                        if (!pStream.ReadVariable(out mLabel)) return false;
                        break;
                    case EpochTag:
                        if (!pStream.ReadVariable(out mEpoch)) return false;
                        break;
                    default: return false;
                }
            }
            return true;
        }

        public void Write(MemoryStream pStream)
        {
            pStream.WriteKey(EMessageKeyType.Variable, LabelTag);
            pStream.WriteVariable(mLabel);
            pStream.WriteKey(EMessageKeyType.Variable, EpochTag);
            pStream.WriteVariable(mEpoch);
        }

        public void Reset()
        {
            mLabel = 0;
            mEpoch = 0;
        }
    }
}
