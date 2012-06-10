namespace Tristram.Lobby.Network
{
    public static class ClientServiceIds
    {
        public const uint ConnectionService         = 0x00; // bnet.protocol.connection.ConnectionService
        public const uint AuthenticationService     = 0x01; // bnet.protocol.authentication.AuthenticationServer
        public const uint ChallengeService          = 0x02; // bnet.protocol.challenge.ChallengeService
        public const uint ChannelService            = 0x10; // bnet.protocol.channel.Channel
        public const uint AchievementsService       = 0x20; // bnet.protocol.achievements.AchievementsService
        public const uint ReportService             = 0x63; // bnet.protocol.report.ReportService
        public const uint Response                  = 0xFE;

        public static string ToString(uint pServiceId)
        {
            switch (pServiceId)
            {
                case ConnectionService:         return "ConnectionService";
                case AuthenticationService:     return "AuthenticationService";
                case ChallengeService:          return "ChallengeService";
                case ChannelService:            return "ChannelService";
                case AchievementsService:       return "AchievementService";
                case ReportService:             return "ReportService";
                case Response:                  return "Response";
                default:                        return "Unknown";
            }
        }
    }
}
