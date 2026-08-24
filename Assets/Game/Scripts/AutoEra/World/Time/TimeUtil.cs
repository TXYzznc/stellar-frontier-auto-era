using System;

namespace AutoEra.World.Time
{
    /// <summary>
    /// Stateless UTC conversions and durations. It deliberately owns neither a clock nor
    /// a time provider so callers can inject their own trusted source.
    /// </summary>
    public static class TimeUtil
    {
        public static TimeSpan GetOfflineDuration(DateTimeOffset savedUtc, IUtcTimeProvider timeProvider)
        {
            if (timeProvider == null)
            {
                throw new ArgumentNullException(nameof(timeProvider));
            }

            return GetNonNegativeDuration(savedUtc, timeProvider.GetUtcNow());
        }

        public static TimeSpan GetNonNegativeDuration(DateTimeOffset fromUtc, DateTimeOffset toUtc)
        {
            TimeSpan duration = toUtc.ToUniversalTime() - fromUtc.ToUniversalTime();
            return duration < TimeSpan.Zero ? TimeSpan.Zero : duration;
        }

        public static long ToUnixTimeMilliseconds(DateTimeOffset value)
        {
            return value.ToUniversalTime().ToUnixTimeMilliseconds();
        }

        public static DateTimeOffset FromUnixTimeMilliseconds(long milliseconds)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
        }
    }
}
