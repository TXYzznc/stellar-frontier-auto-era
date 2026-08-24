using System;

namespace AutoEra.World.Time
{
    /// <summary>
    /// A monotonic world clock with an integer-millisecond authoritative value. Fractional
    /// real-time input is retained only until it can form the next whole world millisecond.
    /// </summary>
    public sealed class WorldClock
    {
        private double _fractionalMilliseconds;

        public WorldClock(long initialWorldMilliseconds = 0L)
        {
            if (initialWorldMilliseconds < 0L)
            {
                throw new ArgumentOutOfRangeException(nameof(initialWorldMilliseconds));
            }

            WorldMilliseconds = initialWorldMilliseconds;
        }

        public long WorldMilliseconds { get; private set; }

        public double FractionalMilliseconds => _fractionalMilliseconds;

        public bool TryAdvanceMilliseconds(long deltaMilliseconds)
        {
            if (deltaMilliseconds < 0L || deltaMilliseconds > long.MaxValue - WorldMilliseconds)
            {
                return false;
            }

            WorldMilliseconds += deltaMilliseconds;
            return true;
        }

        public bool TryAdvanceTo(long targetWorldMilliseconds)
        {
            if (targetWorldMilliseconds < WorldMilliseconds)
            {
                return false;
            }

            return TryAdvanceMilliseconds(targetWorldMilliseconds - WorldMilliseconds);
        }

        public bool TryAdvanceRealtimeSeconds(double elapsedSeconds)
        {
            if (double.IsNaN(elapsedSeconds) || double.IsInfinity(elapsedSeconds) || elapsedSeconds < 0d)
            {
                return false;
            }

            double pendingMilliseconds = _fractionalMilliseconds + (elapsedSeconds * 1000d);
            if (double.IsNaN(pendingMilliseconds) || double.IsInfinity(pendingMilliseconds))
            {
                return false;
            }

            double availableMilliseconds = long.MaxValue - (double)WorldMilliseconds;
            if (pendingMilliseconds > availableMilliseconds)
            {
                return false;
            }

            long wholeMilliseconds = (long)Math.Floor(pendingMilliseconds);
            if (wholeMilliseconds > 0L && !TryAdvanceMilliseconds(wholeMilliseconds))
            {
                return false;
            }

            _fractionalMilliseconds = pendingMilliseconds - wholeMilliseconds;
            return true;
        }
    }
}
