using System;
using System.Collections.Generic;
using System.IO;
using Tristram.Shared.Network;
using Tristram.Shared.Network.Messages;

namespace Tristram.Lobby.Network.ClientServices.Connection
{
    public sealed class ConnectionMeteringContentHandles : IPackable
    {
        public const uint ContentHandleTag = 1;

        private List<ContentHandle> mContentHandles = new List<ContentHandle>();

        public bool HasContentHandles { get { return mContentHandles.Count > 0; } }
        public List<ContentHandle> ContentHandles { get { return mContentHandles; } }

        public bool Read(MemoryStream pStream)
        {
            EMessageKeyType type = EMessageKeyType.Variable;
            uint tag = 0;
            while (pStream.ReadKey(out type, out tag))
            {
                switch (tag)
                {
                    case ContentHandleTag:
                        {
                            ContentHandle contentHandle = new ContentHandle();
                            if (!pStream.ReadPackable(contentHandle)) return false;
                            mContentHandles.Add(contentHandle);
                            break;
                        }
                    default: return false;
                }
            }
            return true;
        }

        public void Write(MemoryStream pStream)
        {
            mContentHandles.ForEach(h =>
            {
                pStream.WriteKey(EMessageKeyType.Prefixed, ContentHandleTag);
                pStream.WritePackable(h);
            });
        }

        public void Reset()
        {
            mContentHandles.Clear();
        }
    }
}
