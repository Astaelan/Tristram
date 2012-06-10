using System;
using System.IO;
using System.Text;
using Tristram.Shared.Network;
using Tristram.Shared.Network.Messages;

namespace Tristram.Lobby.Network.ClientServices.Connection
{
    public sealed class ContentHandle : IPackable
    {
        public const uint RegionTag = 1;
        public const uint UsageTag = 2;
        public const uint HashTag = 3;
        public const uint ProtoUrlTag = 4;

        private uint mRegion = 0;
        private uint mUsage = 0;
        private byte[] mHash = null;
        private bool mHasProtoUrl = false;
        private string mProtoUrl = "";

        public uint Region { get { return mRegion; } set { mRegion = value; } }
        public uint Usage { get { return mUsage; } set { mUsage = value; } }
        public byte[] Hash { get { return mHash; } set { mHash = value; } }
        public bool HasProtoUrl { get { return mHasProtoUrl; } set { mHasProtoUrl = value; if (!mHasProtoUrl) mProtoUrl = ""; } }
        public string ProtoUrl { get { return mProtoUrl; } set { mProtoUrl = value; } }

        public bool Read(MemoryStream pStream)
        {
            EMessageKeyType type = EMessageKeyType.Variable;
            uint tag = 0;
            while (pStream.ReadKey(out type, out tag))
            {
                switch (tag)
                {
                    case RegionTag:
                        {
                            if (!pStream.ReadFixed32(out mRegion)) return false;
                            break;
                        }
                    case UsageTag:
                        {
                            if (!pStream.ReadFixed32(out mUsage)) return false;
                            break;
                        }
                    case HashTag:
                        {
                            if (!pStream.ReadPrefixed(out mHash)) return false;
                            break;
                        }
                    case ProtoUrlTag:
                        {
                            mHasProtoUrl = true;
                            if (!pStream.ReadPrefixed(Encoding.GetEncoding(1252), out mProtoUrl)) return false;
                            break;
                        }
                    default: return false;
                }
            }
            return true;
        }

        public void Write(MemoryStream pStream)
        {
            pStream.WriteKey(EMessageKeyType.Fixed32, RegionTag);
            pStream.WriteFixed32(mRegion);
            pStream.WriteKey(EMessageKeyType.Fixed32, UsageTag);
            pStream.WriteFixed32(mUsage);
            pStream.WriteKey(EMessageKeyType.Prefixed, HashTag);
            pStream.WritePrefixed(mHash);
            if (mHasProtoUrl)
            {
                pStream.WriteKey(EMessageKeyType.Prefixed, ProtoUrlTag);
                pStream.WritePrefixed(Encoding.GetEncoding(1252), mProtoUrl);
            }
        }

        public void Reset()
        {
            mRegion = 0;
            mUsage = 0;
            mHash = null;
            HasProtoUrl = false;
        }
    }
}
