using System;
using System.Collections.Generic;

namespace AutoEra.Machines
{
    /// <summary>
    /// 本机货舱（DEC-111）：第一版统一容量单位，不区分体积/重量/特殊容器。
    /// 物品按类型以整数数量结算，装载/卸载原子化，超容拒绝。
    /// </summary>
    public sealed class MachineCargo
    {
        private readonly Dictionary<string, int> _items = new Dictionary<string, int>(StringComparer.Ordinal);
        public int Capacity { get; }
        public int Used { get; private set; }
        public int Remaining => Capacity - Used;
        public IReadOnlyDictionary<string, int> Items => _items;
        public event Action<MachineCargo> Changed;
        public MachineCargo(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            Capacity = capacity;
        }
        public bool TryLoad(string itemType, int amount)
        {
            if (string.IsNullOrWhiteSpace(itemType) || amount <= 0 || Used + amount > Capacity) return false;
            _items[itemType] = _items.TryGetValue(itemType, out var existing) ? existing + amount : amount;
            Used += amount; Changed?.Invoke(this); return true;
        }
        public bool TryUnload(string itemType, int amount)
        {
            if (string.IsNullOrWhiteSpace(itemType) || amount <= 0 || !_items.TryGetValue(itemType, out var existing) || existing < amount) return false;
            var remaining = existing - amount;
            if (remaining == 0) _items.Remove(itemType); else _items[itemType] = remaining;
            Used -= amount; Changed?.Invoke(this); return true;
        }
        public int Count(string itemType) => _items.TryGetValue(itemType, out var count) ? count : 0;
        public bool Has(string itemType) => Count(itemType) > 0;
    }
}
