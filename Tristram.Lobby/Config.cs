using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Tristram.Lobby
{
    public sealed class Config
    {
        internal static Config Instance { get; private set; }

        internal static void Load()
        {
            try { using (XmlReader reader = XmlReader.Create("Tristram.Lobby.xml")) Instance = (Config)(new XmlSerializer(typeof(Config))).Deserialize(reader); }
            catch (Exception) { Instance = new Config(); Save(); }
        }
        internal static void Save() { using (XmlWriter writer = XmlWriter.Create("Tristram.Lobby.xml", new XmlWriterSettings() { Indent = true, NewLineOnAttributes = true })) new XmlSerializer(typeof(Config)).Serialize(writer, Instance); }

        public string Database = "Server=localhost;Database=Tristram;Trusted_Connection=True;";
        public ushort ClientPort = 1119;
        public byte ClientBacklog = 8;
        public int ClientBuffer = 65536;
        public ushort AccountCacheExpirationDelay = 300;
    }
}
