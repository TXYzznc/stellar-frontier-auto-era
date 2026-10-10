using System;
using AutoEra.DataTable;
using AutoEra.Logistics;
using UnityEngine;

namespace AutoEra.World.Region
{
    /// <summary>Configured warehouse binding; visual enable/disable never owns or destroys inventory.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RegionObjectView))]
    public sealed class RegionWarehouseFacility : MonoBehaviour
    {
        [SerializeField] private int _definitionRowId;
        private ResourceWorldService _resources;
        private InitialRegion _region;
        private RegionObject _object;
        public CargoContainer Container { get; private set; }
        public int DefinitionRowId => _definitionRowId;
        public void Initialize(ResourceWorldService resources, InitialRegion region, RegionObject obj)
        {
            if (_resources != null) throw new InvalidOperationException("Warehouse is already bound.");
            if (resources == null || region == null || obj == null || !obj.IsRegistered) throw new ArgumentException("Warehouse requires an existing region object.");
            var row = GF.DataTable.GetDataTable<BuildingDefinitions>()?.GetDataRow(_definitionRowId);
            if (row == null || row.StorageCapacity <= 0) throw new InvalidOperationException("Warehouse capacity definition is missing.");
            var owner = new CargoOwner(CargoOwnerKind.Receiver, obj.Id);
            if (!resources.Authority.TryReadContainer(owner, out var container))
                container = resources.Authority.RegisterContainer(owner, CargoContainerKind.Warehouse, row.StorageCapacity);
            if (container.Kind != CargoContainerKind.Warehouse || container.Capacity != row.StorageCapacity)
                throw new InvalidOperationException("Restored warehouse capacity differs from its definition.");
            _resources = resources; _region = region; _object = obj; Container = container;
            resources.Authority.ContainerChanged += OnChanged; region.ObjectRemoved += OnRemoved; Refresh();
        }
        private void OnChanged(CargoContainer container) { if (ReferenceEquals(container, Container)) Refresh(); }
        private void Refresh() => _object.SetPublicState(Container.Remaining == 0 ? "实体仓位已满" : "可入库", cachedAmount: Container.Used, cacheCapacity: Container.Capacity);
        private void OnRemoved(AutoEra.World.Identity.PersistentId id)
        {
            if (_object == null || id != _object.Id) return;
            // Domain removal invalidates future deliveries, but retains already owned cargo quantities.
            _resources.Authority.TryResize(Container.Owner, Container.Capacity, false); Release();
        }
        private void Release()
        {
            if (_resources == null) return;
            _resources.Authority.ContainerChanged -= OnChanged; _region.ObjectRemoved -= OnRemoved;
            _resources = null; _region = null; _object = null;
        }
        private void OnDestroy() => Release();
#if UNITY_EDITOR
        public void ConfigureForEditor(int definitionRowId)
        { if (definitionRowId <= 0) throw new ArgumentOutOfRangeException(nameof(definitionRowId)); _definitionRowId = definitionRowId; }
#endif
    }
}
