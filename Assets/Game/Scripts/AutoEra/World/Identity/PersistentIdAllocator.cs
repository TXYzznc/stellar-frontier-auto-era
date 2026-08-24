namespace AutoEra.World.Identity
{
    /// <summary>
    /// Owns one monotonically increasing identity sequence for a single world session.
    /// IDs are never returned to the sequence after allocation or restoration.
    /// </summary>
    public sealed class PersistentIdAllocator
    {
        private ulong _nextValue = 1UL;
        private bool _isExhausted;

        public bool IsExhausted => _isExhausted;

        public PersistentId NextId => _isExhausted ? PersistentId.Invalid : new PersistentId(_nextValue);

        public bool TryAllocate(out PersistentId id)
        {
            if (_isExhausted)
            {
                id = PersistentId.Invalid;
                return false;
            }

            id = new PersistentId(_nextValue);
            if (_nextValue == ulong.MaxValue)
            {
                _isExhausted = true;
            }
            else
            {
                _nextValue++;
            }

            return true;
        }

        /// <summary>
        /// Records an ID restored from persistent state. The caller remains responsible
        /// for duplicate detection in the world registry.
        /// </summary>
        public bool TryRestore(PersistentId id)
        {
            if (!id.IsValid)
            {
                return false;
            }

            if (_isExhausted)
            {
                return true;
            }

            if (id.Value < _nextValue)
            {
                return true;
            }

            if (id.Value == ulong.MaxValue)
            {
                _isExhausted = true;
                return true;
            }

            _nextValue = id.Value + 1UL;
            return true;
        }
    }
}
