using System;
using System.Collections.Generic;
using System.IO;
using Tristram.Shared.Network;
using Tristram.Shared.Network.Messages;

namespace Tristram.Lobby.Network.ClientServices.Connection
{
    public sealed class BindRequest : IPackable
    {
        public const uint ImportedServiceHashTag = 1;
        public const uint ExportedServiceTag = 2;

        private List<uint> mImportedServiceHashes = new List<uint>();
        private List<BoundService> mExportedServices = new List<BoundService>();

        public bool HasImportedServiceHashes { get { return mImportedServiceHashes.Count > 0; } }
        public List<uint> ImportedServiceHashes { get { return mImportedServiceHashes; } }
        public bool HasExportedServices { get { return mExportedServices.Count > 0; } }
        public List<BoundService> ExportedServices { get { return mExportedServices; } }

        public bool Read(MemoryStream pStream)
        {
            EMessageKeyType type = EMessageKeyType.Variable;
            uint tag = 0;
            while (pStream.ReadKey(out type, out tag))
            {
                switch (tag)
                {
                    case ImportedServiceHashTag:
                        {
                            MemoryStream stream = null;
                            if (!pStream.ReadPrefixed(out stream)) return false;
                            uint hash = 0;
                            while (stream.ReadFixed32(out hash)) mImportedServiceHashes.Add(hash);
                            break;
                        }
                    case ExportedServiceTag:
                        {
                            BoundService boundService = new BoundService();
                            if (!pStream.ReadPackable(boundService)) return false;
                            mExportedServices.Add(boundService);
                            break;
                        }
                    default: return false;
                }
            }
            return true;
        }

        public void Write(MemoryStream pStream)
        {
            if (mImportedServiceHashes.Count > 0)
            {
                pStream.WriteKey(EMessageKeyType.Prefixed, ImportedServiceHashTag);
                MemoryStream stream = new MemoryStream();
                mImportedServiceHashes.ForEach(h => stream.WriteFixed32(h));
                pStream.WritePrefixed(stream);
            }
            mExportedServices.ForEach(s =>
            {
                pStream.WriteKey(EMessageKeyType.Prefixed, ExportedServiceTag);
                pStream.WritePackable(s);
            });
        }

        public void Reset()
        {
            mImportedServiceHashes.Clear();
            mExportedServices.Clear();
        }
    }
}
