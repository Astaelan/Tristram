using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using Tristram.Lobby.Network.ClientServices.Authentication;
using Tristram.Shared;
using Tristram.Shared.Network;
using Tristram.Shared.Network.Messages;

namespace Tristram.Lobby.Network.ClientServices
{
    [ClientService("bnet.protocol.authentication.AuthenticationServer", ClientServiceIds.AuthenticationService)]
    public static class AuthenticationService
    {
        private static class AuthenticationMethodIds
        {
            public const uint LogonRequest = 1;
            public const uint ModuleNotification = 2;
            public const uint ModuleMessage = 3;
            public const uint SelectGameAccount = 4;

            public static string ToString(uint pMethodId)
            {
                switch (pMethodId)
                {
                    case LogonRequest: return "LogonRequest";
                    case ModuleNotification: return "ModuleNotification";
                    case ModuleMessage: return "ModuleMessage";
                    case SelectGameAccount: return "SelectGameAccount";
                    default: return "Unknown";
                }
            }
        }

        [ClientServiceMethod(ClientServiceIds.AuthenticationService, AuthenticationMethodIds.LogonRequest, true)]
        public static void OnLogonRequest(Client pClient, Header pHeader, MemoryStream pData)
        {
            pClient.LogCall(ClientServiceIds.ToString(ClientServiceIds.AuthenticationService), AuthenticationMethodIds.ToString(AuthenticationMethodIds.LogonRequest));
            LogonRequest logonRequest = new LogonRequest();
            if (!logonRequest.Read(pData) || !logonRequest.HasEmail) return;
            Program.AddCallback(() => LogonRequestCallback(pClient, pHeader, logonRequest));
        }

        private static void LogonRequestCallback(Client pClient, Header pHeader, LogonRequest pLogonRequest)
        {
            Account account = AccountCache.RetrieveAccountByEmail(pLogonRequest.Email);
            if (account == null)
            {
                LogonResult logonResult = new LogonResult();
                logonResult.ErrorCode = LogonResult.ErrorCodeInvalidCredentials;
                MemoryStream response = new MemoryStream(16);
                logonResult.Write(response);
                pClient.SendRPC(2, 5, 0, 0, response);
            }
        }
    }
}
