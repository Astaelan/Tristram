using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tristram.Shared.Network;
using Tristram.Shared.Network.Messages;

namespace Tristram.Lobby.Network.ClientServices.Authentication
{
    public sealed class LogonResult : IPackable
    {
        public const uint ErrorCodeTag = 1;
        public const uint AccountTag = 2;
        public const uint GameAccountTag = 3;

        public const uint ErrorCodeSuccess = 0;
        public const uint ErrorCodeInvalidCredentials = 3;
        public const uint ErrorCodeNoCharacterSelected = 11;
        public const uint ErrorCodeNoGameAccount = 12;

        private uint mErrorCode = 0;
        private bool mHasAccount = false;
        private EntityId mAccount = new EntityId();
        private List<EntityId> mGameAccounts = new List<EntityId>();


        public uint ErrorCode { get { return mErrorCode; } set { mErrorCode = value; } }
        public bool HasAccount { get { return mHasAccount; } set { mHasAccount = value; if (!mHasAccount) mAccount.Reset(); } }
        public EntityId Account { get { return mAccount; } set { mAccount = value; } }
        public bool HasGameAccounts { get { return mGameAccounts.Count > 0; } }
        public List<EntityId> GameAccounts { get { return mGameAccounts; } }

        public bool Read(MemoryStream pStream)
        {
            EMessageKeyType type = EMessageKeyType.Variable;
            uint tag = 0;
            while (pStream.ReadKey(out type, out tag))
            {
                switch (tag)
                {
                    case ErrorCodeTag:
                        if (!pStream.ReadVariable(out mErrorCode)) return false;
                        break;
                    case AccountTag:
                        mHasAccount = true;
                        if (!pStream.ReadPackable(mAccount)) return false;
                        break;
                    case GameAccountTag:
                        {
                            EntityId entityId = new EntityId();
                            if (!pStream.ReadPackable(entityId)) return false;
                            mGameAccounts.Add(entityId);
                            break;
                        }
                    default: return false;
                }
            }
            return true;
        }

        public void Write(MemoryStream pStream)
        {
            pStream.WriteKey(EMessageKeyType.Variable, ErrorCodeTag);
            pStream.WriteVariable(mErrorCode);
            if (mHasAccount)
            {
                pStream.WriteKey(EMessageKeyType.Prefixed, AccountTag);
                pStream.WritePackable(mAccount);
            }
            mGameAccounts.ForEach(a =>
            {
                pStream.WriteKey(EMessageKeyType.Prefixed, GameAccountTag);
                pStream.WritePackable(a);
            });
        }

        public void Reset()
        {
            mErrorCode = 0;
            HasAccount = false;
            mGameAccounts.Clear();
        }
    }
}
