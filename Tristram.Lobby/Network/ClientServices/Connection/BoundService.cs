using System;
using System.Collections.Generic;
using System.IO;
using Tristram.Shared.Network;
using Tristram.Shared.Network.Messages;

namespace Tristram.Lobby.Network.ClientServices.Connection
{
    public sealed class BoundService : IPackable
    {
        public const uint HashTag = 1;
        public const uint IdTag = 2;

        private uint mHash = 0;
        private uint mId = 0;

        public uint Hash { get { return mHash; } set { mHash = value; } }
        public uint Id { get { return mId; } set { mId = value; } }

        public bool Read(MemoryStream pStream)
        {
            EMessageKeyType type = EMessageKeyType.Variable;
            uint tag = 0;
            while (pStream.ReadKey(out type, out tag))
            {
                switch (tag)
                {
                    case HashTag:
                        if (!pStream.ReadFixed32(out mHash)) return false;
                        break;
                    case IdTag:
                        if (!pStream.ReadVariable(out mId)) return false;
                        break;
                    default: return false;
                }
            }
            return true;
        }

        public void Write(MemoryStream pStream)
        {
            pStream.WriteKey(EMessageKeyType.Fixed32, HashTag);
            pStream.WriteFixed32(mHash);
            pStream.WriteKey(EMessageKeyType.Variable, IdTag);
            pStream.WriteVariable(mId);
        }

        public void Reset()
        {
            mHash = 0;
            mId = 0;
        }
    }
}
