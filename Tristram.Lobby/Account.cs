using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Tristram.Lobby
{
    public sealed class Account
    {
        public long Identifier { get; set; }
        public string Email { get; set; }
        public byte[] Salt { get; set; }
        public byte[] Verifier { get; set; }
        public string BattleTagName { get; set; }
        public short BattleTagCode { get; set; }
        public string Permissions { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastOnline { get; set; }

        public DateTime CacheExpiration { get; set; }

        public void Load(SqlDataReader pReader)
        {
            Identifier = (long)pReader["Identifier"];
            Email = (string)pReader["Email"];
            Salt = (byte[])pReader["Salt"];
            Verifier = (byte[])pReader["Verifier"];
            BattleTagName = (string)pReader["BattleTagName"];
            BattleTagCode = (short)pReader["BattleTagCode"];
            Permissions = (string)pReader["Permissions"];
            IsOnline = (bool)pReader["IsOnline"];
            LastOnline = (DateTime)pReader["LastOnline"];
        }
    }
}
