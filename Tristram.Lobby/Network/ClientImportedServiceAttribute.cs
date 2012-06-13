using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tristram.Shared.Network.Messages;

namespace Tristram.Lobby.Network
{
    public sealed class ClientImportedServiceAttribute : Attribute
    {
        private const uint HASH_SEED = 0x811C9DC5;
        private const uint HASH_MULTIPLIER = 0x1000193;

        public readonly string Name;
        public readonly uint ServiceId;
        public readonly uint Hash;
        private Dictionary<uint, ClientImportedServiceMethodAttribute> mMethods = new Dictionary<uint, ClientImportedServiceMethodAttribute>();

        public ClientImportedServiceAttribute(string pName, uint pServiceId)
        {
            Name = pName;
            ServiceId = pServiceId;
            Hash = Encoding.ASCII.GetBytes(Name).Aggregate(HASH_SEED, (c, v) => HASH_MULTIPLIER * (c ^ v));
        }
        public void AddMethod(uint pMethodId, ClientImportedServiceMethodAttribute pMethod) { mMethods[pMethodId] = pMethod; }
        public bool TryGetMethod(uint pMethodId, out ClientImportedServiceMethodAttribute pMethod) { return mMethods.TryGetValue(pMethodId, out pMethod); }
        public int MethodCount { get { return mMethods.Count; } }
    }
}
