using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Tristram.Lobby.Network.ClientServices.Authentication;
using Tristram.Shared;
using Tristram.Shared.Network;
using Tristram.Shared.Network.Messages;

namespace Tristram.Lobby.Network
{
    public sealed class Client : Descriptor
    {
        private HashSet<uint> mPermittedServices = new HashSet<uint>();
        private HashSet<Tuple<uint, uint>> mRequestedServices = new HashSet<Tuple<uint, uint>>();
        private HashSet<uint> mImportedServices = new HashSet<uint>();
        private HashSet<uint> mExportedServices = new HashSet<uint>();

        internal Client(Socket pSocket)
            : base(pSocket, Config.Instance.ClientBuffer)
        {
            mPermittedServices.Add(ClientImportedServiceIds.ConnectionService);
            mImportedServices.Add(ClientImportedServiceIds.ConnectionService);
        }

        public void Log(ELogLevel pLogLevel, string pFormat, params object[] pArgs) { Logger.WriteLine(pLogLevel, "[Client:" + Host + "] " + pFormat, pArgs); }
        public void LogCall(string pService, string pMethod) { Log(ELogLevel.Debug, "Called {0}.{1}", pService, pMethod); }

        public HashSet<uint> PermittedServices { get { return mPermittedServices; } }
        public HashSet<Tuple<uint, uint>> RequestedServices { get { return mRequestedServices; } }
        public HashSet<uint> ImportedServices { get { return mImportedServices; } }
        public HashSet<uint> ExportedServices { get { return mExportedServices; } }

        protected override void OnConnected()
        {
            Log(ELogLevel.Info, "Connected");
        }

        protected override void OnDisconnect()
        {
            Program.DisconnectClient(this);
        }

        protected override void OnDisconnected()
        {
            Log(ELogLevel.Info, "Disconnected");
        }

        protected override int OnDataReceived(byte[] pBuffer, int pStart, int pLength)
        {
            int consumed = 0;
            while (pLength > 2)
            {
                int totalSize = 2;
                if (pLength < totalSize) return consumed;

                ushort headerSize = (ushort)((pBuffer[pStart + 0] << 8) | pBuffer[pStart + 1]);
                totalSize += headerSize;
                if (pLength < totalSize) return consumed;

                Header header = new Header();
                if (!header.Read(new MemoryStream(pBuffer, pStart + 2, headerSize))) return consumed;
                totalSize += (int)header.Size;
                if (pLength < totalSize) return consumed;

                MemoryStream data = new MemoryStream((int)header.Size);
                data.Write(pBuffer, pStart + 2 + headerSize, (int)header.Size);
                data.Seek(0, SeekOrigin.Begin);
                Program.AddCallback(() => OnMessage(header, data));

                pStart += totalSize;
                pLength -= totalSize;
                consumed += totalSize;
            }

            return consumed;
        }

        private void OnMessage(Header pHeader, MemoryStream pData)
        {
            Log(ELogLevel.Debug, "Received Message: {0}.{1} Token = {2}, {3} Bytes", pHeader.ServiceId, pHeader.MethodId, pHeader.Token, pHeader.Size);

            if (!mImportedServices.Contains(pHeader.ServiceId))
            {
                Log(ELogLevel.Warn, "Unavailable Service: {0}", pHeader.ServiceId);
                return;
            }
            ClientImportedServiceMethodAttribute clientServiceMethod = null;
            if (!Program.GetClientServiceMethod(pHeader.ServiceId, pHeader.MethodId, out clientServiceMethod))
            {
                Log(ELogLevel.Warn, "Unknown Service Method: {0}.{1}", pHeader.ServiceId, pHeader.MethodId);
                return;
            }
            if (clientServiceMethod.Dump) Logger.Dump(pData.ToArray(), 0, (int)pData.Length);
            clientServiceMethod.Method(this, pHeader, pData);
        }

        public void SendResponse(uint pToken, ulong pObjectId, uint pStatus, List<ErrorInfo> pErrors, MemoryStream pData)
        {
            Header header = new Header();
            header.ServiceId = ClientImportedServiceIds.Response;
            header.Token = pToken;
            if (pObjectId != 0)
            {
                header.HasObjectId = true;
                header.ObjectId = pObjectId;
            }
            if (pData != null && pData.Length > 0)
            {
                header.HasSize = true;
                header.Size = (uint)pData.Length;
            }
            header.HasStatus = true;
            header.Status = pStatus;
            if (pErrors != null) pErrors.ForEach(e => header.Errors.Add(e));

            int size = 16;
            if (pData != null) size += (int)pData.Length;
            MemoryStream packet = new MemoryStream(size);
            packet.Seek(2, SeekOrigin.Begin);
            header.Write(packet);
            packet.Seek(0, SeekOrigin.Begin);
            packet.WriteByte((byte)(((packet.Length - 2) >> 8) & 0xFF));
            packet.WriteByte((byte)((packet.Length - 2) & 0xFF));
            packet.Seek(0, SeekOrigin.End);
            if (pData != null) pData.WriteTo(packet);
            Send(packet.ToArray());

            Log(ELogLevel.Debug, "Sent Message: {0}.{1} Token = {2}, {3} Bytes", header.ServiceId, header.MethodId, header.Token, header.Size);
            if (pData != null) Logger.Dump(pData.ToArray(), 0, (int)pData.Length);
        }

        public void SendRPC(uint pServiceId, uint pMethodId, uint pToken, ulong pObjectId, MemoryStream pData)
        {
            Header header = new Header();
            header.ServiceId = pServiceId;
            header.HasMethodId = true;
            header.MethodId = pMethodId;
            header.Token = pToken;
            if (pObjectId != 0)
            {
                header.HasObjectId = true;
                header.ObjectId = pObjectId;
            }
            if (pData != null && pData.Length > 0)
            {
                header.HasSize = true;
                header.Size = (uint)pData.Length;
            }

            int size = 16;
            if (pData != null) size += (int)pData.Length;
            MemoryStream packet = new MemoryStream(size);
            packet.Seek(2, SeekOrigin.Begin);
            header.Write(packet);
            packet.Seek(0, SeekOrigin.Begin);
            packet.WriteByte((byte)(((packet.Length - 2) >> 8) & 0xFF));
            packet.WriteByte((byte)((packet.Length - 2) & 0xFF));
            packet.Seek(0, SeekOrigin.End);
            if (pData != null) pData.WriteTo(packet);
            Send(packet.ToArray());

            Log(ELogLevel.Debug, "Sent Message: {0}.{1} Token = {2}, {3} Bytes", header.ServiceId, header.MethodId, header.Token, header.Size);
            if (pData != null) Logger.Dump(pData.ToArray(), 0, (int)pData.Length);
        }


        private static class AuthenticationClientMethodIds
        {
            public const uint ModuleLoad = 1;
            public const uint ModuleMessage = 2;
            public const uint AccountSettings = 3;
            public const uint ServerStateChange = 4;
            public const uint LogonComplete = 5;
            public const uint MemModuleLoad = 6;
        }

        public void SendAuthenticationClientLogonComplete(LogonResult pLogonResult)
        {
            MemoryStream response = new MemoryStream(64);
            pLogonResult.Write(response);
            SendRPC(ClientExportedServiceIds.AuthenticationClient, AuthenticationClientMethodIds.LogonComplete, 0, 0, response);
        }
    }
}
