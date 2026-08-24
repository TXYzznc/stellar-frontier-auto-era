using System;
using System.Globalization;

namespace AutoEra.World.Identity
{
    /// <summary>
    /// An immutable, world-local identity. Zero is reserved for an unresolved identity.
    /// </summary>
    [Serializable]
    public readonly struct PersistentId : IEquatable<PersistentId>, IComparable<PersistentId>
    {
        public static readonly PersistentId Invalid = default;

        public PersistentId(ulong value)
        {
            Value = value;
        }

        public ulong Value { get; }

        public bool IsValid => Value != 0UL;

        public int CompareTo(PersistentId other)
        {
            return Value.CompareTo(other.Value);
        }

        public bool Equals(PersistentId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is PersistentId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        public static bool TryParse(string value, out PersistentId id)
        {
            if (ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out ulong parsed))
            {
                id = new PersistentId(parsed);
                return true;
            }

            id = Invalid;
            return false;
        }

        public static bool operator ==(PersistentId left, PersistentId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PersistentId left, PersistentId right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(PersistentId left, PersistentId right)
        {
            return left.Value < right.Value;
        }

        public static bool operator >(PersistentId left, PersistentId right)
        {
            return left.Value > right.Value;
        }

        public static bool operator <=(PersistentId left, PersistentId right)
        {
            return left.Value <= right.Value;
        }

        public static bool operator >=(PersistentId left, PersistentId right)
        {
            return left.Value >= right.Value;
        }
    }
}
