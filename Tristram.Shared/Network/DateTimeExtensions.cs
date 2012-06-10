using System;

namespace Tristram.Shared.Network
{
    public static class DateTimeExtensions
    {
        private const long UNIX_EPOCH = 621355968000000000L;

        public static uint ToUnixTime(this DateTime pDateTime)
        {
            return (uint)((pDateTime.ToUniversalTime().Ticks - UNIX_EPOCH) / 10000000L);
        }

        public static ulong ToExtendedEpoch(this DateTime pDateTime)
        {
            return (ulong)((pDateTime.ToUniversalTime().Ticks - UNIX_EPOCH) / 10L);
        }
    }
}
