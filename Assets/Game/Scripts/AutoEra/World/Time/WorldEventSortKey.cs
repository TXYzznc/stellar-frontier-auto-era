using System;
using AutoEra.World.Identity;

namespace AutoEra.World.Time
{
    /// <summary>
    /// Fixed ordering phases for events that share the same world millisecond.
    /// Values match the confirmed first-version causality order.
    /// </summary>
    public enum WorldEventPhase
    {
        Energy = 0,
        WorldState = 1,
        Sensor = 2,
        Algorithm = 3,
        TaskAndBehavior = 4,
        ResourceAndReward = 5,
    }

    /// <summary>
    /// A deterministic value key; event collections may sort it independently of insertion
    /// order and without requiring a runtime event scheduler.
    /// </summary>
    public readonly struct WorldEventSortKey : IEquatable<WorldEventSortKey>, IComparable<WorldEventSortKey>
    {
        public WorldEventSortKey(long worldMilliseconds, WorldEventPhase phase, PersistentId persistentId, ulong sequence)
        {
            if (worldMilliseconds < 0L)
            {
                throw new ArgumentOutOfRangeException(nameof(worldMilliseconds));
            }

            if (!persistentId.IsValid)
            {
                throw new ArgumentException("A stable world event key requires a valid persistent ID.", nameof(persistentId));
            }

            if (!Enum.IsDefined(typeof(WorldEventPhase), phase))
            {
                throw new ArgumentOutOfRangeException(nameof(phase));
            }

            WorldMilliseconds = worldMilliseconds;
            Phase = phase;
            PersistentId = persistentId;
            Sequence = sequence;
        }

        public long WorldMilliseconds { get; }

        public WorldEventPhase Phase { get; }

        public PersistentId PersistentId { get; }

        public ulong Sequence { get; }

        public int CompareTo(WorldEventSortKey other)
        {
            int result = WorldMilliseconds.CompareTo(other.WorldMilliseconds);
            if (result != 0)
            {
                return result;
            }

            result = Phase.CompareTo(other.Phase);
            if (result != 0)
            {
                return result;
            }

            result = PersistentId.CompareTo(other.PersistentId);
            return result != 0 ? result : Sequence.CompareTo(other.Sequence);
        }

        public bool Equals(WorldEventSortKey other)
        {
            return WorldMilliseconds == other.WorldMilliseconds
                && Phase == other.Phase
                && PersistentId == other.PersistentId
                && Sequence == other.Sequence;
        }

        public override bool Equals(object obj)
        {
            return obj is WorldEventSortKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = WorldMilliseconds.GetHashCode();
                hash = (hash * 397) ^ (int)Phase;
                hash = (hash * 397) ^ PersistentId.GetHashCode();
                return (hash * 397) ^ Sequence.GetHashCode();
            }
        }

        public static bool operator ==(WorldEventSortKey left, WorldEventSortKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WorldEventSortKey left, WorldEventSortKey right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(WorldEventSortKey left, WorldEventSortKey right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(WorldEventSortKey left, WorldEventSortKey right)
        {
            return left.CompareTo(right) > 0;
        }
    }
}
