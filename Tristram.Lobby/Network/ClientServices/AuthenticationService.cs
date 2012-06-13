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
    [ClientImportedService("bnet.protocol.authentication.AuthenticationServer", ClientImportedServiceIds.AuthenticationServer)]
    public static class AuthenticationService
    {
        private static class AuthenticationMethodIds
        {
            public const uint LogonRequest = 1;
            public const uint ModuleNotification = 2;
            public const uint ModuleMessage = 3;
            public const uint SelectGameAccount = 4;
        }

        [ClientImportedServiceMethod(ClientImportedServiceIds.AuthenticationServer, AuthenticationMethodIds.LogonRequest, true)]
        public static void OnLogonRequest(Client pClient, Header pHeader, MemoryStream pData)
        {
            LogonRequest logonRequest = new LogonRequest();
            if (!logonRequest.Read(pData) || !logonRequest.HasEmail) return;
            Program.AddCallback(() => LogonRequestCallback(pClient, pHeader, logonRequest));
        }

        private static void LogonRequestCallback(Client pClient, Header pHeader, LogonRequest pLogonRequest)
        {
            Account account = AccountCache.RetrieveAccountByEmail(pLogonRequest.Email);
            if (account == null)
            {
                pClient.PermittedServices.Clear();
                pClient.ImportedServices.Clear();
                pClient.SendAuthenticationClientLogonComplete(new LogonResult(EErrorCode.LoginInformationWasIncorrect));
                return;
            }
            
        }
    }
}
