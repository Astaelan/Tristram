using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tristram.Shared.Network.Messages
{
    public sealed class Header : IPackable
    {
        public const uint ServiceIdTag = 1;
        public const uint MethodIdTag = 2;
        public const uint TokenTag = 3;
        public const uint ObjectIdTag = 4;
        public const uint SizeTag = 5;
        public const uint StatusTag = 6;
        public const uint ErrorTag = 7;

        private uint mServiceId = 0;
        private bool mHasMethodId = false;
        private uint mMethodId = 0;
        private uint mToken = 0;
        private bool mHasObjectId = false;
        private ulong mObjectId = 0;
        private bool mHasSize = false;
        private uint mSize = 0;
        private bool mHasStatus = false;
        private uint mStatus = 0;
        private List<ErrorInfo> mErrors = new List<ErrorInfo>();

        public uint ServiceId { get { return mServiceId; } set { mServiceId = value; } }
        public bool HasMethodId { get { return mHasMethodId; } set { mHasMethodId = value; if (!mHasMethodId) mMethodId = 0; } }
        public uint MethodId { get { return mMethodId; } set { mMethodId = value; } }
        public uint Token { get { return mToken; } set { mToken = value; } }
        public bool HasObjectId { get { return mHasObjectId; } set { mHasObjectId = value; if (!mHasObjectId) mObjectId = 0; } }
        public ulong ObjectId { get { return mObjectId; } set { mObjectId = value; } }
        public bool HasSize { get { return mHasSize; } set { mHasSize = value; if (!mHasSize) mSize = 0; } }
        public uint Size { get { return mSize; } set { mSize = value; } }
        public bool HasStatus { get { return mHasStatus; } set { mHasStatus = value; if (!mHasStatus) mStatus = 0; } }
        public uint Status { get { return mStatus; } set { mStatus = value; } }
        public bool HasErrors { get { return mErrors.Count > 0; } }
        public List<ErrorInfo> Errors { get { return mErrors; } }

        public bool Read(MemoryStream pStream)
        {
            EMessageKeyType type = EMessageKeyType.Variable;
            uint tag = 0;
            while (pStream.ReadKey(out type, out tag))
            {
                switch (tag)
                {
                    case ServiceIdTag:
                        if (!pStream.ReadVariable(out mServiceId)) return false;
                        break;
                    case MethodIdTag:
                        mHasMethodId = true;
                        if (!pStream.ReadVariable(out mMethodId)) return false;
                        break;
                    case TokenTag:
                        if (!pStream.ReadVariable(out mToken)) return false;
                        break;
                    case ObjectIdTag:
                        mHasObjectId = true;
                        if (!pStream.ReadVariable(out mObjectId)) return false;
                        break;
                    case SizeTag:
                        mHasSize = true;
                        if (!pStream.ReadVariable(out mSize)) return false;
                        break;
                    case StatusTag:
                        mHasStatus = true;
                        if (!pStream.ReadVariable(out mStatus)) return false;
                        break;
                    case ErrorTag:
                        {
                            ErrorInfo errorInfo = new ErrorInfo();
                            if (!pStream.ReadPackable(errorInfo)) return false;
                            mErrors.Add(errorInfo);
                            break;
                        }
                    default: return false;
                }
            }
            return true;
        }
        public void Write(MemoryStream pStream)
        {
            pStream.WriteKey(EMessageKeyType.Variable, ServiceIdTag);
            pStream.WriteVariable(mServiceId);
            if (mHasMethodId)
            {
                pStream.WriteKey(EMessageKeyType.Variable, MethodIdTag);
                pStream.WriteVariable(mMethodId);
            }
            pStream.WriteKey(EMessageKeyType.Variable, TokenTag);
            pStream.WriteVariable(mToken);
            if (mHasObjectId)
            {
                pStream.WriteKey(EMessageKeyType.Variable, ObjectIdTag);
                pStream.WriteVariable(mObjectId);
            }
            if (mHasSize)
            {
                pStream.WriteKey(EMessageKeyType.Variable, SizeTag);
                pStream.WriteVariable(mSize);
            }
            if (mHasStatus)
            {
                pStream.WriteKey(EMessageKeyType.Variable, StatusTag);
                pStream.WriteVariable(mStatus);
            }
            mErrors.ForEach(e =>
            {
                pStream.WriteKey(EMessageKeyType.Prefixed, ErrorTag);
                pStream.WritePackable(e);
            });
        }

        public void Reset()
        {
            mServiceId = 0;
            HasMethodId = false;
            mToken = 0;
            HasObjectId = false;
            HasSize = false;
            HasStatus = false;
            mErrors.Clear();
        }
    }
}
