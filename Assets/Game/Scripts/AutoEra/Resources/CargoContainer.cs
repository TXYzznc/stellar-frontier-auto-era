using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AutoEra.Buildings;
using AutoEra.World.Identity;

namespace AutoEra.Logistics
{
    public enum CargoContainerKind { Store, MachineCargo, Warehouse, WorldFree }

    /// <summary>Inventory projection owned and written only by CargoOwnershipAuthority.</summary>
    public sealed class CargoContainer
    {
        internal readonly Dictionary<string, long> MutableItems = new Dictionary<string, long>(StringComparer.Ordinal);
        internal readonly List<PersistentId> LotIds = new List<PersistentId>();
        internal long ReservedCapacity;
        public CargoOwner Owner { get; }
        public CargoContainerKind Kind { get; }
        public long Capacity { get; internal set; }
        public long Used { get; internal set; }
        public ulong Generation { get; internal set; }
        public bool IsAvailable { get; internal set; } = true;
        public long Revision { get; internal set; }
        public IReadOnlyDictionary<string, long> Items { get; }
        public long Remaining => Math.Max(0, Capacity - Used - ReservedCapacity);
        internal CargoContainer(CargoOwner owner, CargoContainerKind kind, long capacity, ulong generation)
        {
            if (!owner.IsValid || capacity < 0 || generation == 0 || !Enum.IsDefined(typeof(CargoContainerKind), kind))
                throw new ArgumentException("Invalid cargo container.");
            Owner = owner; Kind = kind; Capacity = capacity; Generation = generation;
            Items = new ReadOnlyDictionary<string, long>(MutableItems);
        }
        public long Count(string item) => item != null && MutableItems.TryGetValue(item, out var count) ? count : 0;
        internal WarehouseDestination Route(ResourceItemDefinition item)
        {
            if (Kind == CargoContainerKind.Warehouse) return WarehouseClassification.Classify(item.Class);
            if (item.Class == CargoItemClass.Gold || (item.Class == CargoItemClass.MachineCarrier && Kind != CargoContainerKind.WorldFree))
                return WarehouseDestination.Rejected;
            return WarehouseDestination.LocalInventory;
        }
        internal void Add(string item, int units)
        { MutableItems[item] = checked(Count(item) + units); Used = checked(Used + units); Revision++; }
        internal void Remove(string item, int units)
        {
            long count = Count(item) - units;
            if (count == 0) MutableItems.Remove(item); else MutableItems[item] = count;
            Used -= units; Revision++;
        }
    }

    /// <summary>Actual library custody, validated before the atomic commit. It must not create a second inventory.</summary>
    public interface ICargoLibrarySettlement
    {
        bool ValidateEntering(ResourceItemDefinition item, PersistentId payloadId, out string reason);
        void Enter(CargoLotSnapshot lot, PersistentId payloadId);
        bool Validate(CargoLotSnapshot lot, PersistentId payloadId, WarehouseDestination destination, out string reason);
        void Commit(CargoLotSnapshot lot, PersistentId payloadId, WarehouseDestination destination);
    }
}
