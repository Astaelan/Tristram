using System;
using System.IO;

namespace Tristram.Shared.Network.Messages
{
    public sealed class ErrorInfo : IPackable
    {
        public const uint ObjectAddressTag = 1;
        public const uint StatusTag = 2;
        public const uint ServiceHashTag = 3;
        public const uint MethodIdTag = 4;

        private ObjectAddress mObjectAddress = new ObjectAddress();
        private uint mStatus = 0;
        private uint mServiceHash = 0;
        private uint mMethodId = 0;

        public ObjectAddress ObjectAddress { get { return mObjectAddress; } }
        public uint Status { get { return mStatus; } set { mStatus = value; } }
        public uint ServiceHash { get { return mServiceHash; } set { mServiceHash = value; } }
        public uint MethodId { get { return mMethodId; } set { mMethodId = value; } }

        public bool Read(MemoryStream pStream)
        {
            EMessageKeyType type = EMessageKeyType.Variable;
            uint tag = 0;
            while (pStream.ReadKey(out type, out tag))
            {
                switch (tag)
                {
                    case ObjectAddressTag:
                        if (!pStream.ReadPackable(mObjectAddress)) return false;
                        break;
                    case StatusTag:
                        if (!pStream.ReadVariable(out mStatus)) return false;
                        break;
                    case ServiceHashTag:
                        if (!pStream.ReadVariable(out mServiceHash)) return false;
                        break;
                    case MethodIdTag:
                        if (!pStream.ReadVariable(out mMethodId)) return false;
                        break;
                    default: return false;
                }
            }

            return true;
        }

        public void Write(MemoryStream pStream)
        {
            pStream.WriteKey(EMessageKeyType.Prefixed, ObjectAddressTag);
            pStream.WritePackable(mObjectAddress);
            pStream.WriteKey(EMessageKeyType.Variable, StatusTag);
            pStream.WriteVariable(mStatus);
            pStream.WriteKey(EMessageKeyType.Variable, ServiceHashTag);
            pStream.WriteVariable(mServiceHash);
            pStream.WriteKey(EMessageKeyType.Variable, MethodIdTag);
            pStream.WriteVariable(mMethodId);
        }

        public void Reset()
        {
            mObjectAddress.Reset();
            mStatus = 0;
            mServiceHash = 0;
            mMethodId = 0;
        }
    }
}
