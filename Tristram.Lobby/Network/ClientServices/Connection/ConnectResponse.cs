using System;
using System.IO;
using Tristram.Shared.Network;
using Tristram.Shared.Network.Messages;

namespace Tristram.Lobby.Network.ClientServices.Connection
{
    public sealed class ConnectResponse : IPackable
    {
        public const uint ServerIdTag = 1;
        public const uint ClientIdTag = 2;
        public const uint BindResultTag = 3;
        public const uint BindResponseTag = 4;
        public const uint ConnectionMeteringContentHandlesTag = 5;

        private ProcessId mServerId = new ProcessId();
        private bool mHasClientId = false;
        private ProcessId mClientId = new ProcessId();
        private bool mHasBindResult = false;
        private uint mBindResult = 0;
        private bool mHasBindResponse = false;
        private BindResponse mBindResponse = new BindResponse();
        private bool mHasConnectionMeteringContentHandles = false;
        private ConnectionMeteringContentHandles mConnectionMeteringContentHandles = new ConnectionMeteringContentHandles();

        public ProcessId ServerId { get { return mServerId; } }
        public bool HasClientId { get { return mHasClientId; } set { mHasClientId = value; if (!mHasClientId) mClientId.Reset(); } }
        public ProcessId ClientId { get { return mClientId; } }
        public bool HasBindResult { get { return mHasBindResult; } set { mHasBindResult = value; if (!mHasBindResult) mBindResult = 0; } }
        public uint BindResult { get { return mBindResult; } set { mBindResult = value; } }
        public bool HasBindResponse { get { return mHasBindResponse; } set { mHasBindResponse = value; if (!mHasBindResponse) mBindResponse.Reset(); } }
        public BindResponse BindResponse { get { return mBindResponse; } }
        public bool HasConnectionMeteringContentHandles { get { return mHasConnectionMeteringContentHandles; } set { mHasConnectionMeteringContentHandles = value; if (!mHasConnectionMeteringContentHandles) mConnectionMeteringContentHandles.Reset(); } }
        public ConnectionMeteringContentHandles ConnectionMeteringContentHandles { get { return mConnectionMeteringContentHandles; } }

        public bool Read(MemoryStream pStream)
        {
            EMessageKeyType type = EMessageKeyType.Variable;
            uint tag = 0;
            while (pStream.ReadKey(out type, out tag))
            {
                switch (tag)
                {
                    case ServerIdTag:
                        if (!pStream.ReadPackable(mServerId)) return false;
                        break;
                    case ClientIdTag:
                        mHasClientId = true;
                        if (!pStream.ReadPackable(mClientId)) return false;
                        break;
                    case BindResultTag:
                        mHasBindResult = true;
                        if (!pStream.ReadVariable(out mBindResult)) return false;
                        break;
                    case BindResponseTag:
                        mHasBindResponse = true;
                        if (!pStream.ReadPackable(mBindResponse)) return false;
                        break;
                    case ConnectionMeteringContentHandlesTag:
                        mHasConnectionMeteringContentHandles = true;
                        if (!pStream.ReadPackable(mConnectionMeteringContentHandles)) return false;
                        break;
                    default: return false;
                }
            }
            return true;
        }

        public void Write(MemoryStream pStream)
        {
            pStream.WriteKey(EMessageKeyType.Prefixed, ServerIdTag);
            pStream.WritePackable(mServerId);
            if (mHasClientId)
            {
                pStream.WriteKey(EMessageKeyType.Prefixed, ClientIdTag);
                pStream.WritePackable(mClientId);
            }
            if (mHasBindResult)
            {
                pStream.WriteKey(EMessageKeyType.Variable, BindResultTag);
                pStream.WriteVariable(mBindResult);
            }
            if (mHasBindResponse)
            {
                pStream.WriteKey(EMessageKeyType.Prefixed, BindResponseTag);
                pStream.WritePackable(mBindResponse);
            }
            if (mHasConnectionMeteringContentHandles)
            {
                pStream.WriteKey(EMessageKeyType.Prefixed, ConnectionMeteringContentHandlesTag);
                pStream.WritePackable(mConnectionMeteringContentHandles);
            }
        }

        public void Reset()
        {
            mServerId.Reset();
            HasClientId = false;
            HasBindResult = false;
            HasBindResponse = false;
            HasConnectionMeteringContentHandles = false;
        }
    }
}
