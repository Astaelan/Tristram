using System;
using System.IO;
using Tristram.Shared.Network;
using Tristram.Shared.Network.Messages;

namespace Tristram.Lobby.Network.ClientServices.Connection
{
    public sealed class ConnectRequest : IPackable
    {
        public const uint ClientIdTag = 1;
        public const uint BindRequestTag = 2;

        private bool mHasClientId = false;
        private ProcessId mClientId = new ProcessId();
        private bool mHasBindRequest = false;
        private BindRequest mBindRequest = new BindRequest();

        public bool HasClientId { get { return mHasClientId; } set { mHasClientId = value; if (!mHasClientId) mClientId.Reset(); } }
        public ProcessId ClientId { get { return mClientId; } }
        public bool HasBindRequest { get { return mHasBindRequest; } set { mHasBindRequest = value; if (!mHasBindRequest) mBindRequest.Reset(); } }
        public BindRequest BindRequest { get { return mBindRequest; } }

        public bool Read(MemoryStream pStream)
        {
            EMessageKeyType type = EMessageKeyType.Variable;
            uint tag = 0;
            while (pStream.ReadKey(out type, out tag))
            {
                switch (tag)
                {
                    case ClientIdTag:
                        mHasClientId = true;
                        if (!pStream.ReadPackable(mClientId)) return false;
                        break;
                    case BindRequestTag:
                        mHasBindRequest = true;
                        if (!pStream.ReadPackable(mBindRequest)) return false;
                        break;
                    default: return false;
                }
            }
            return true;
        }

        public void Write(MemoryStream pStream)
        {
            if (mHasClientId)
            {
                pStream.WriteKey(EMessageKeyType.Prefixed, ClientIdTag);
                pStream.WritePackable(mClientId);
            }
            if (mHasBindRequest)
            {
                pStream.WriteKey(EMessageKeyType.Prefixed, BindRequestTag);
                pStream.WritePackable(mBindRequest);
            }
        }

        public void Reset()
        {
            HasClientId = false;
            HasBindRequest = false;
        }
    }
}
