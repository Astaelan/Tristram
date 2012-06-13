namespace Tristram.Lobby.Network
{
    public static class ClientImportedServiceIds
    {
        public const uint ConnectionService         = 0x00; // bnet.protocol.connection.ConnectionService
        public const uint AuthenticationServer      = 0x01; // bnet.protocol.authentication.AuthenticationServer
        public const uint ChallengeService          = 0x02; // bnet.protocol.challenge.ChallengeService
        public const uint ChannelService            = 0x10; // bnet.protocol.channel.Channel
        public const uint AchievementsService       = 0x20; // bnet.protocol.achievements.AchievementsService
        public const uint ReportService             = 0x63; // bnet.protocol.report.ReportService
        public const uint Response                  = 0xFE;
    }
}
