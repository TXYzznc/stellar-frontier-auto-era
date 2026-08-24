using System;

namespace AutoEra.World.Time
{
    public sealed class SystemUtcTimeProvider : IUtcTimeProvider
    {
        public DateTimeOffset GetUtcNow()
        {
            return DateTimeOffset.UtcNow;
        }
    }
}
