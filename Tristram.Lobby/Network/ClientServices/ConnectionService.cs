﻿using System;
using System.IO;
using System.Text;
using Tristram.Lobby.Network.ClientServices.Connection;
using Tristram.Shared;
using Tristram.Shared.Network;
using Tristram.Shared.Network.Messages;

namespace Tristram.Lobby.Network.ClientServices
{
    [ClientImportedService("bnet.protocol.connection.ConnectionService", ClientImportedServiceIds.ConnectionService)]
    public static class ConnectionService
    {
        private static class ConnectionMethodIds
        {
            public const uint Connect = 1;
            public const uint Bind = 2;
            public const uint Echo = 3;
            public const uint ForceDisconnect = 4;
            public const uint Null = 5;
            public const uint Encrypt = 6;
            public const uint RequestDisconnect = 7;
        }

        [ClientImportedServiceMethod(ClientImportedServiceIds.ConnectionService, ConnectionMethodIds.Connect)]
        public static void OnConnectRequest(Client pClient, Header pHeader, MemoryStream pData)
        {
            ConnectRequest connectRequest = new ConnectRequest();
            if (!connectRequest.Read(pData)) return;

            ConnectResponse connectResponse = new ConnectResponse();
            connectResponse.ServerId.Label = 0;
            connectResponse.ServerId.Epoch = DateTime.Now.ToUnixTime();
            connectResponse.HasClientId = true;
            connectResponse.ClientId.Label = 1;
            connectResponse.ClientId.Epoch = connectResponse.ServerId.Epoch;
            if (connectRequest.HasClientId) connectResponse.ClientId.Label = connectResponse.ClientId.Label;
            ContentHandle contentHandle = new ContentHandle();
            byte[] region = Encoding.ASCII.GetBytes("US");
            contentHandle.Region = (uint)(region[0] << 8) | region[1];
            byte[] usage = Encoding.ASCII.GetBytes("mtrz");
            contentHandle.Usage = (uint)(usage[0] << 24) | (uint)(usage[1] << 16) | (uint)(usage[2] << 8) | usage[3];
            contentHandle.Hash = new byte[] { 0x18, 0xe9, 0x8c, 0xde, 0x12, 0x83, 0x71, 0x49, 0x62, 0x19, 0x88, 0xce, 0xee, 0x55, 0x12, 0x3b,
                                              0xf2, 0xbe, 0x83, 0x9a, 0x6d, 0xc1, 0xd6, 0xbb, 0x00, 0xa3, 0x99, 0x52, 0x06, 0x56, 0xb2, 0xa6 };
            connectResponse.HasConnectionMeteringContentHandles = true;
            connectResponse.ConnectionMeteringContentHandles.ContentHandles.Add(contentHandle);

            pClient.PermittedServices.Add(ClientImportedServiceIds.AuthenticationServer);
            //pClient.PermittedServices.Add(ClientServiceIds.ChallengeService);
            //pClient.PermittedServices.Add(ClientServiceIds.ChannelService);
            //pClient.PermittedServices.Add(ClientServiceIds.AchievementsService);
            //pClient.PermittedServices.Add(ClientServiceIds.ReportService);

            MemoryStream response = new MemoryStream(128);
            connectResponse.Write(response);
            pClient.SendResponse(pHeader.Token, 0, 0, null, response);
        }

        [ClientImportedServiceMethod(ClientImportedServiceIds.ConnectionService, ConnectionMethodIds.Bind)]
        public static void OnBindRequest(Client pClient, Header pHeader, MemoryStream pData)
        {
            BindRequest bindRequest = new BindRequest();
            if (!bindRequest.Read(pData)) return;

            BindResponse bindResponse = new BindResponse();
            foreach (uint hash in bindRequest.ImportedServiceHashes)
            {
                uint serviceId = 0;
                if (Program.GetClientServiceId(hash, out serviceId) && !pClient.ImportedServices.Contains(serviceId))
                {
                    if (pClient.PermittedServices.Contains(serviceId))
                    {
                        pClient.Log(ELogLevel.Debug, "Importing Service {0}", serviceId);
                        pClient.ImportedServices.Add(serviceId);
                        bindResponse.ImportedServiceIds.Add(serviceId);
                    }
                    else pClient.RequestedServices.Add(new Tuple<uint, uint>(serviceId, pHeader.Token));
                }
            }
            if (bindResponse.HasImportedServiceIds)
            {
                MemoryStream response = new MemoryStream(128);
                bindResponse.Write(response);
                pClient.SendResponse(pHeader.Token, 0, 0, null, response);
            }
        }
    }
}
