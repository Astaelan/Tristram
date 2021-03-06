﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Tristram.Lobby
{
    public static class AccountCache
    {
        private static Dictionary<string, Account> sAccounts = new Dictionary<string, Account>();
        private static Queue<Account> sExpirationOrder = new Queue<Account>();
        private static DateTime sNextExpirationCheck = DateTime.Now;

        public static bool Create(string pEmail, byte[] pSalt, byte[] pVerifier, string pBattleTagName)
        {
            bool result = false;
            using (SqlConnection dbConnection = new SqlConnection(Config.Instance.Database))
            {
                dbConnection.Open();

                SqlCommand dbCommand = dbConnection.CreateCommand();
                dbCommand.CommandType = CommandType.StoredProcedure;
                dbCommand.CommandText = "spAccountCreate";
                dbCommand.Parameters.AddWithValue("@Email", pEmail);
                dbCommand.Parameters.AddWithValue("@Salt", pSalt);
                dbCommand.Parameters.AddWithValue("@Verifier", pVerifier);
                dbCommand.Parameters.AddWithValue("@BattleTagName", pBattleTagName);
                dbCommand.Parameters.AddWithValue("@Result", 0).Direction = ParameterDirection.Output;
                dbCommand.Parameters.AddWithValue("@Identifier", 0).Direction = ParameterDirection.Output;
                dbCommand.ExecuteNonQuery();
                result = (byte)dbCommand.Parameters["@Result"].Value != 0;
            }
            return result;
        }
        public static Account RetrieveAccountByEmail(string pEmail)
        {
            pEmail = pEmail.ToLower();
            Account account = null;
            if (!sAccounts.TryGetValue(pEmail, out account))
            {
                using (SqlConnection dbConnection = new SqlConnection(Config.Instance.Database))
                {
                    dbConnection.Open();

                    SqlCommand dbCommand = dbConnection.CreateCommand();
                    dbCommand.CommandType = CommandType.StoredProcedure;
                    dbCommand.CommandText = "spAccountLookupByEmail";
                    dbCommand.Parameters.AddWithValue("@Email", pEmail);
                    using (SqlDataReader dbReader = dbCommand.ExecuteReader())
                    {
                        if (dbReader.Read())
                        {
                            account = new Account();
                            account.Load(dbReader);
                            account.CacheExpiration = DateTime.Now.AddSeconds(Config.Instance.AccountCacheExpirationDelay);
                            sExpirationOrder.Enqueue(account);
                        }
                    }
                }
                if (account != null) sAccounts[pEmail] = account;
            }
            return account;
        }

        public static void CheckExpired()
        {
            DateTime now = DateTime.Now;
            if (sNextExpirationCheck <= now)
            {
                Account account = null;
                while (sExpirationOrder.Count > 0)
                {
                    account = sExpirationOrder.Peek();
                    if (account.CacheExpiration <= now)
                    {
                        sExpirationOrder.Dequeue();
                        if (!account.IsOnline) sAccounts.Remove(account.Email.ToLower());
                    }
                }
                sNextExpirationCheck = DateTime.Now.AddSeconds(1);
            }
        }
    }
}
