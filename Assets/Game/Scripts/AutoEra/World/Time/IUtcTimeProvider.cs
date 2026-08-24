using System;

namespace AutoEra.World.Time
{
    public interface IUtcTimeProvider
    {
        DateTimeOffset GetUtcNow();
    }
}
