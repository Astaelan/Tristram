using System;
using System.IO;

namespace Tristram.Shared.Network.Messages
{
    public sealed class ObjectAddress : IPackable
    {
        public const uint HostTag = 1;
        public const uint ObjectIdTag = 2;

        private ProcessId mHost = new ProcessId();
        private bool mHasObjectId = false;
        private ulong mObjectId = 0;

        public ProcessId Host { get { return mHost; } }
        public bool HasObjectId { get { return mHasObjectId; } set { mHasObjectId = value; if (!mHasObjectId) mObjectId = 0; } }
        public ulong ObjectId { get { return mObjectId; } set { mObjectId = value; } }

        public bool Read(MemoryStream pStream)
        {
            EMessageKeyType type = EMessageKeyType.Variable;
            uint tag = 0;
            while (pStream.ReadKey(out type, out tag))
            {
                switch (tag)
                {
                    case HostTag:
                        if (!pStream.ReadPackable(mHost)) return false;
                        break;
                    case ObjectIdTag:
                        mHasObjectId = true;
                        if (!pStream.ReadVariable(out mObjectId)) return false;
                        break;
                    default: return false;
                }
            }
            return true;
        }

        public void Write(MemoryStream pStream)
        {
            pStream.WriteKey(EMessageKeyType.Prefixed, HostTag);
            pStream.WritePackable(mHost);
            if (mHasObjectId)
            {
                pStream.WriteKey(EMessageKeyType.Variable, ObjectIdTag);
                pStream.WriteVariable(mObjectId);
            }
        }

        public void Reset()
        {
            mHost.Reset();
            mObjectId = 0;
        }
    }
}
