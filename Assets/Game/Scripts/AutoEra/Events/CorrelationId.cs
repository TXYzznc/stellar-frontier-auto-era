using System;
using System.Globalization;

namespace AutoEra.Events
{
    /// <summary>
    /// A session-scoped trigger identity. Zero is reserved for an unresolved correlation.
    /// B47 world checkpoints preserve these diagnostics and their allocator for causal continuity.
    /// They remain independent of permanent object IDs.
    /// </summary>
    [Serializable]
    public readonly struct CorrelationId : IEquatable<CorrelationId>, IComparable<CorrelationId>
    {
        public static readonly CorrelationId Invalid = default;

        public CorrelationId(ulong value)
        {
            Value = value;
        }

        public ulong Value { get; }

        public bool IsValid => Value != 0UL;

        public int CompareTo(CorrelationId other)
        {
            return Value.CompareTo(other.Value);
        }

        public bool Equals(CorrelationId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is CorrelationId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        public static bool operator ==(CorrelationId left, CorrelationId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CorrelationId left, CorrelationId right)
        {
            return !left.Equals(right);
        }
    }
}
