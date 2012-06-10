using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tristram.Shared.Network;
using Tristram.Shared.Network.Messages;

namespace Tristram.Lobby.Network.ClientServices.Authentication
{
    public sealed class LogonRequest : IPackable
    {
        public const uint ProgramTag = 1;
        public const uint PlatformTag = 2;
        public const uint LocaleTag = 3;
        public const uint EmailTag = 4;
        public const uint VersionTag = 5;
        public const uint ApplicationVersionTag = 6;
        public const uint CookieOnlyTag = 7;
        public const uint UnknownTag = 9;

        private bool mHasProgram = false;
        private string mProgram = "";
        private bool mHasPlatform = false;
        private string mPlatform = "";
        private bool mHasLocale = false;
        private string mLocale = "";
        private bool mHasEmail = false;
        private string mEmail = "";
        private bool mHasVersion = false;
        private string mVersion = "";
        private bool mHasApplicationVersion = false;
        private uint mApplicationVersion = 0;
        private bool mHasCookieOnly = false;
        private bool mCookieOnly = false;
        private bool mHasUnknown = false;
        private uint mUnknown = 0;

        public bool HasProgram { get { return mHasProgram; } set { mHasProgram = value; if (!mHasProgram) mProgram = ""; } }
        public string Program { get { return mProgram; } set { mProgram = value; } }
        public bool HasPlatform { get { return mHasPlatform; } set { mHasPlatform = value; if (!mHasPlatform) mPlatform = ""; } }
        public string Platform { get { return mPlatform; } set { mPlatform = value; } }
        public bool HasLocale { get { return mHasLocale; } set { mHasLocale = value; if (!mHasLocale) mLocale = ""; } }
        public string Locale { get { return mLocale; } set { mLocale = value; } }
        public bool HasEmail { get { return mHasEmail; } set { mHasEmail = value; if (!mHasEmail) mEmail = ""; } }
        public string Email { get { return mEmail; } set { mEmail = value; } }
        public bool HasVersion { get { return mHasVersion; } set { mHasVersion = value; if (!mHasVersion) mVersion = ""; } }
        public string Version { get { return mVersion; } set { mVersion = value; } }
        public bool HasApplicationVersion { get { return mHasApplicationVersion; } set { mHasApplicationVersion = value; if (!mHasApplicationVersion) mApplicationVersion = 0; } }
        public uint ApplicationVersion { get { return mApplicationVersion; } set { mApplicationVersion = value; } }
        public bool HasCookieOnly { get { return mHasCookieOnly; } set { mHasCookieOnly = value; if (!mHasCookieOnly) mCookieOnly = false; } }
        public bool CookieOnly { get { return mCookieOnly; } set { mCookieOnly = value; } }
        public bool HasUnknown { get { return mHasUnknown; } set { mHasUnknown = value; if (!mHasUnknown) mUnknown = 0; } }
        public uint Unknown { get { return mUnknown; } set { mUnknown = value; } }

        public bool Read(MemoryStream pStream)
        {
            EMessageKeyType type = EMessageKeyType.Variable;
            uint tag = 0;
            while (pStream.ReadKey(out type, out tag))
            {
                switch (tag)
                {
                    case ProgramTag:
                        mHasProgram = true;
                        if (!pStream.ReadPrefixed(Encoding.GetEncoding(1252), out mProgram)) return false;
                        break;
                    case PlatformTag:
                        mHasPlatform = true;
                        if (!pStream.ReadPrefixed(Encoding.GetEncoding(1252), out mPlatform)) return false;
                        break;
                    case LocaleTag:
                        mHasLocale = true;
                        if (!pStream.ReadPrefixed(Encoding.GetEncoding(1252), out mLocale)) return false;
                        break;
                    case EmailTag:
                        mHasEmail = true;
                        if (!pStream.ReadPrefixed(Encoding.GetEncoding(1252), out mEmail)) return false;
                        break;
                    case VersionTag:
                        mHasVersion = true;
                        if (!pStream.ReadPrefixed(Encoding.GetEncoding(1252), out mVersion)) return false;
                        break;
                    case ApplicationVersionTag:
                        mHasApplicationVersion = true;
                        if (!pStream.ReadVariable(out mApplicationVersion)) return false;
                        break;
                    case CookieOnlyTag:
                        {
                            uint cookieOnly = 0;
                            mHasCookieOnly = true;
                            if (!pStream.ReadVariable(out cookieOnly)) return false;
                            mCookieOnly = Convert.ToBoolean(cookieOnly);
                            break;
                        }
                    case UnknownTag:
                        mHasUnknown = true;
                        pStream.ReadFixed32(out mUnknown);
                        break;
                    default: return false;
                }
            }
            return true;
        }

        public void Write(MemoryStream pStream)
        {
            if (mHasProgram)
            {
                pStream.WriteKey(EMessageKeyType.Prefixed, ProgramTag);
                pStream.WritePrefixed(Encoding.GetEncoding(1252), mProgram);
            }
            if (mHasPlatform)
            {
                pStream.WriteKey(EMessageKeyType.Prefixed, PlatformTag);
                pStream.WritePrefixed(Encoding.GetEncoding(1252), mPlatform);
            }
            if (mHasLocale)
            {
                pStream.WriteKey(EMessageKeyType.Prefixed, LocaleTag);
                pStream.WritePrefixed(Encoding.GetEncoding(1252), mLocale);
            }
            if (mHasEmail)
            {
                pStream.WriteKey(EMessageKeyType.Prefixed, EmailTag);
                pStream.WritePrefixed(Encoding.GetEncoding(1252), mEmail);
            }
            if (mHasVersion)
            {
                pStream.WriteKey(EMessageKeyType.Prefixed, VersionTag);
                pStream.WritePrefixed(Encoding.GetEncoding(1252), mVersion);
            }
            if (mHasApplicationVersion)
            {
                pStream.WriteKey(EMessageKeyType.Variable, ApplicationVersionTag);
                pStream.WriteVariable(mApplicationVersion);
            }
            if (mHasCookieOnly)
            {
                pStream.WriteKey(EMessageKeyType.Variable, CookieOnlyTag);
                pStream.WriteVariable((ulong)(mCookieOnly ? 1 : 0));
            }
        }

        public void Reset()
        {
            HasProgram = false;
            HasPlatform = false;
            HasLocale = false;
            HasEmail = false;
            HasVersion = false;
            HasApplicationVersion = false;
            HasCookieOnly = false;
        }
    }
}
