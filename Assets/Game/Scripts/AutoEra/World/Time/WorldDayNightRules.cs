using System;

namespace AutoEra.World.Time
{
    public enum WorldDayNightPhase
    {
        Sunlit = 0,
        Dark = 1,
    }

    /// <summary>
    /// Validated day/night rules independent of configuration storage. World creation will
    /// supply the confirmed initial time and these parameters in a later integration step.
    /// </summary>
    public readonly struct WorldDayNightRules : IEquatable<WorldDayNightRules>
    {
        public WorldDayNightRules(long cycleMilliseconds, long sunlitMilliseconds)
        {
            if (cycleMilliseconds <= 0L)
            {
                throw new ArgumentOutOfRangeException(nameof(cycleMilliseconds));
            }

            if (sunlitMilliseconds < 0L || sunlitMilliseconds > cycleMilliseconds)
            {
                throw new ArgumentOutOfRangeException(nameof(sunlitMilliseconds));
            }

            CycleMilliseconds = cycleMilliseconds;
            SunlitMilliseconds = sunlitMilliseconds;
        }

        public long CycleMilliseconds { get; }

        public long SunlitMilliseconds { get; }

        public WorldDayNightPhase GetPhase(long worldMilliseconds)
        {
            if (worldMilliseconds < 0L)
            {
                throw new ArgumentOutOfRangeException(nameof(worldMilliseconds));
            }

            long cyclePosition = worldMilliseconds % CycleMilliseconds;
            return cyclePosition < SunlitMilliseconds ? WorldDayNightPhase.Sunlit : WorldDayNightPhase.Dark;
        }

        public bool Equals(WorldDayNightRules other)
        {
            return CycleMilliseconds == other.CycleMilliseconds && SunlitMilliseconds == other.SunlitMilliseconds;
        }

        public override bool Equals(object obj)
        {
            return obj is WorldDayNightRules other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (CycleMilliseconds.GetHashCode() * 397) ^ SunlitMilliseconds.GetHashCode();
            }
        }

        public static bool operator ==(WorldDayNightRules left, WorldDayNightRules right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WorldDayNightRules left, WorldDayNightRules right)
        {
            return !left.Equals(right);
        }
    }
}
