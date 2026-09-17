using System;

namespace AutoEra.Events
{
    /// <summary>
    /// Monotonic session-scoped allocator for correlation IDs. Main-thread only; it is
    /// deliberately independent of the persistent identity allocator.
    /// </summary>
    public sealed class CorrelationAllocator
    {
        private ulong _nextValue = 1UL;

        public CorrelationId NextId => _nextValue == 0UL ? CorrelationId.Invalid : new CorrelationId(_nextValue);

        public bool TryAllocate(out CorrelationId correlation)
        {
            if (_nextValue == 0UL)
            {
                correlation = CorrelationId.Invalid;
                return false;
            }

            correlation = new CorrelationId(_nextValue);
            _nextValue = _nextValue == ulong.MaxValue ? 0UL : _nextValue + 1UL;
            return true;
        }
    }
}