using System;
using System.Collections.Generic;

namespace AutoEra.World.Identity
{
    public enum PersistentObjectKind
    {
        None = 0,
        Machine,
        Building,
        ResourcePoint,
        Task,
        Behavior,
    }

    public enum PersistentRegistryResult
    {
        Success = 0,
        InvalidId,
        InvalidKind,
        NullInstance,
        DuplicateId,
        Missing,
        KindMismatch,
        InstanceMismatch,
        AllocationRejected,
    }

    /// <summary>
    /// World-local registry for persistent object references. It owns no global state and
    /// must be discarded together with the world session that owns it.
    /// </summary>
    public sealed class PersistentObjectRegistry
    {
        private readonly Dictionary<PersistentId, Entry> _entries = new Dictionary<PersistentId, Entry>();
        private readonly PersistentIdAllocator _allocator;

        public PersistentObjectRegistry(PersistentIdAllocator allocator)
        {
            _allocator = allocator ?? throw new ArgumentNullException(nameof(allocator));
        }

        public int Count => _entries.Count;

        public PersistentRegistryResult TryRegister(PersistentId id, PersistentObjectKind kind, object instance)
        {
            if (!id.IsValid)
            {
                return PersistentRegistryResult.InvalidId;
            }

            if (kind == PersistentObjectKind.None)
            {
                return PersistentRegistryResult.InvalidKind;
            }

            if (instance == null)
            {
                return PersistentRegistryResult.NullInstance;
            }

            if (_entries.ContainsKey(id))
            {
                return PersistentRegistryResult.DuplicateId;
            }

            if (!_allocator.TryRestore(id))
            {
                return PersistentRegistryResult.AllocationRejected;
            }

            _entries.Add(id, new Entry(kind, instance));
            return PersistentRegistryResult.Success;
        }

        public PersistentRegistryResult TryResolve(PersistentId id, PersistentObjectKind expectedKind, out object instance)
        {
            instance = null;
            if (!id.IsValid)
            {
                return PersistentRegistryResult.InvalidId;
            }

            if (expectedKind == PersistentObjectKind.None)
            {
                return PersistentRegistryResult.InvalidKind;
            }

            if (!_entries.TryGetValue(id, out Entry entry))
            {
                return PersistentRegistryResult.Missing;
            }

            if (entry.Kind != expectedKind)
            {
                return PersistentRegistryResult.KindMismatch;
            }

            instance = entry.Instance;
            return PersistentRegistryResult.Success;
        }

        public PersistentRegistryResult TryUnregister(PersistentId id, PersistentObjectKind expectedKind, object expectedInstance = null)
        {
            if (!id.IsValid)
            {
                return PersistentRegistryResult.InvalidId;
            }

            if (expectedKind == PersistentObjectKind.None)
            {
                return PersistentRegistryResult.InvalidKind;
            }

            if (!_entries.TryGetValue(id, out Entry entry))
            {
                return PersistentRegistryResult.Missing;
            }

            if (entry.Kind != expectedKind)
            {
                return PersistentRegistryResult.KindMismatch;
            }

            if (expectedInstance != null && !ReferenceEquals(entry.Instance, expectedInstance))
            {
                return PersistentRegistryResult.InstanceMismatch;
            }

            _entries.Remove(id);
            return PersistentRegistryResult.Success;
        }

        public void Clear()
        {
            _entries.Clear();
        }

        private readonly struct Entry
        {
            public Entry(PersistentObjectKind kind, object instance)
            {
                Kind = kind;
                Instance = instance;
            }

            public PersistentObjectKind Kind { get; }

            public object Instance { get; }
        }
    }
}
