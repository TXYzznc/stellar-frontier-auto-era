using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AutoEra.Logistics;

namespace AutoEra.Machines
{
    /// <summary>
    /// 本机货舱（DEC-111）：第一版统一容量单位，不区分体积/重量/特殊容器。
    /// 正式运行只投影世界唯一归属权威，装卸通过预留事务；独立构造保留隔离领域模型入口。
    /// </summary>
    public sealed class MachineCargo
    {
        private readonly Dictionary<string, int> _items = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly ReadOnlyDictionary<string, int> _readOnlyItems;
        private readonly CargoOwnershipAuthority _authority;
        private readonly CargoContainer _container;
        public bool IsAuthorityProjection => _authority != null;
        public int Capacity { get; private set; }
        public int Used { get; private set; }
        public int Remaining => _container != null ? checked((int)_container.Remaining) : Capacity - Used;
        public IReadOnlyDictionary<string, int> Items => _readOnlyItems;
        public event Action<MachineCargo> Changed;
        public MachineCargo(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            Capacity = capacity;
            _readOnlyItems = new ReadOnlyDictionary<string, int>(_items);
        }
        internal MachineCargo(CargoOwnershipAuthority authority, CargoContainer container) : this(checked((int)container.Capacity))
        { _authority = authority; _container = container; RefreshProjection(); }
        internal void RefreshProjection(bool publish = true)
        {
            _items.Clear();
            foreach (var pair in _container.Items) _items.Add(pair.Key, checked((int)pair.Value));
            Capacity = checked((int)_container.Capacity); Used = checked((int)_container.Used);
            if (publish) NotifyProjectionChanged();
        }
        internal void NotifyProjectionChanged() => Changed?.Invoke(this);
        internal bool TryReconfigure(int capacity)
        {
            if (capacity < Used || capacity < 0) return false;
            if (capacity == Capacity) return true;
            if (_authority != null) return _authority.TryResize(_container.Owner, capacity);
            Capacity = capacity; Changed?.Invoke(this); return true;
        }
        public bool TryLoad(string itemType, int amount)
        {
            if (_authority != null) return false;
            if (string.IsNullOrWhiteSpace(itemType) || amount <= 0 || amount > Remaining) return false;
            _items[itemType] = _items.TryGetValue(itemType, out var existing) ? existing + amount : amount;
            Used += amount; Changed?.Invoke(this); return true;
        }
        public bool TryUnload(string itemType, int amount)
        {
            if (_authority != null) return false;
            if (string.IsNullOrWhiteSpace(itemType) || amount <= 0 || !_items.TryGetValue(itemType, out var existing) || existing < amount) return false;
            var remaining = existing - amount;
            if (remaining == 0) _items.Remove(itemType); else _items[itemType] = remaining;
            Used -= amount; Changed?.Invoke(this); return true;
        }
        public int Count(string itemType) => _items.TryGetValue(itemType, out var count) ? count : 0;
        public bool Has(string itemType) => Count(itemType) > 0;
    }
}
