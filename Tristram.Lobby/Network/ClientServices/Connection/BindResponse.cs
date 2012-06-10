using System;
using System.Collections.Generic;
using System.IO;
using Tristram.Shared.Network;
using Tristram.Shared.Network.Messages;

namespace Tristram.Lobby.Network.ClientServices.Connection
{
    public sealed class BindResponse : IPackable
    {
        public const uint ImportedServiceIdTag = 1;

        private List<uint> mImportedServiceIds = new List<uint>();

        public bool HasImportedServiceIds { get { return mImportedServiceIds.Count > 0; } }
        public List<uint> ImportedServiceIds { get { return mImportedServiceIds; } }

        public bool Read(MemoryStream pStream)
        {
            EMessageKeyType type = EMessageKeyType.Variable;
            uint tag = 0;
            while (pStream.ReadKey(out type, out tag))
            {
                switch (tag)
                {
                    case ImportedServiceIdTag:
                        {
                            MemoryStream stream = null;
                            if (!pStream.ReadPrefixed(out stream)) return false;
                            uint value = 0;
                            while (stream.ReadVariable(out value)) mImportedServiceIds.Add((uint)value);
                            break;
                        }
                    default: return false;
                }
            }
            return true;
        }

        public void Write(MemoryStream pStream)
        {
            if (mImportedServiceIds.Count > 0)
            {
                pStream.WriteKey(EMessageKeyType.Prefixed, ImportedServiceIdTag);
                MemoryStream stream = new MemoryStream();
                mImportedServiceIds.ForEach(i => stream.WriteVariable(i));
                pStream.WritePrefixed(stream);
            }
        }

        public void Reset()
        {
            mImportedServiceIds.Clear();
        }
    }
}
