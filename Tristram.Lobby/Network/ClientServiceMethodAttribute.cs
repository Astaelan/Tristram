using System;
using System.IO;
using Tristram.Shared.Network.Messages;

namespace Tristram.Lobby.Network
{
    public delegate void ClientServiceMethodDelegate(Client pClient, Header pHeader, MemoryStream pData);
    public sealed class ClientServiceMethodAttribute : Attribute
    {
        public readonly uint ServiceId;
        public readonly uint MethodId;
        public readonly bool Dump;
        public ClientServiceMethodDelegate Method;

        public ClientServiceMethodAttribute(uint pServiceId, uint pMethodId, bool pDump = false) { ServiceId = pServiceId; MethodId = pMethodId; Dump = pDump; }
    }
}
