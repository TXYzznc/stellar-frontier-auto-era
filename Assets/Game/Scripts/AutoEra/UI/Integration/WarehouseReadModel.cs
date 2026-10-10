using System;
using System.Collections.Generic;
using AutoEra.Logistics;

namespace AutoEra.UI
{
    public readonly struct UiInventoryItem
    {
        public string Id { get; }
        public string Name { get; }
        public long Balance { get; }
        public long WarehouseUnits { get; }
        public long CargoUnits { get; }
        public long GroundUnits { get; }
        public UiInventoryItem(string id, string name, long balance, long warehouse, long cargo, long ground)
        { Id=id; Name=name; Balance=balance; WarehouseUnits=warehouse; CargoUnits=cargo; GroundUnits=ground; }
    }
    public sealed class WarehouseDomainSnapshot
    {
        public UiDataState State { get; }
        public string Reason { get; }
        public long Revision { get; }
        public IReadOnlyList<UiInventoryItem> Items { get; }
        public IReadOnlyList<ResourceTransferResult> Receipts { get; }
        public IReadOnlyList<UiDetailField> Summary { get; }
        internal WarehouseDomainSnapshot(UiDataState state, string reason, long revision, List<UiInventoryItem> items,
            List<ResourceTransferResult> receipts, List<UiDetailField> summary)
        { State=state; Reason=reason; Revision=revision; Items=items.AsReadOnly(); Receipts=receipts.AsReadOnly(); Summary=summary.AsReadOnly(); }
    }
    /// <summary>Detached inventory view. Notifications invalidate; capture waits for the commit boundary.</summary>
    public sealed class WarehouseReadModel : IDisposable
    {
        private readonly ResourceWorldService _resources;
        private readonly Func<string,string> _name;
        private bool _dirty=true, _disposed;
        public WarehouseDomainSnapshot Snapshot { get; private set; }
        public bool NeedsRefresh => !_disposed && _dirty;
        public WarehouseReadModel(ResourceWorldService resources, Func<string,string> name = null)
        {
            _resources=resources; _name=name;
            Snapshot=new WarehouseDomainSnapshot(UiDataState.Unavailable,"世界库存尚未就绪。",0,new List<UiInventoryItem>(),new List<ResourceTransferResult>(),new List<UiDetailField>());
            if (_resources != null) { _resources.Authority.ContainerChanged+=OnContainerChanged; _resources.Authority.Settled+=OnSettled; }
            Refresh();
        }
        private void OnContainerChanged(CargoContainer value) => _dirty=true;
        private void OnSettled(ResourceTransferCommitted value) => _dirty=true;
        public bool Refresh()
        {
            if (_disposed || !_dirty) return false;
            if (_resources == null || !_resources.CatalogReady) { _dirty=false; return true; }
            if (!_resources.Authority.TryCapture(out var inventory)) return false;
            var items=new List<UiInventoryItem>(); var summary=new List<UiDetailField>(); var receipts=new List<ResourceTransferResult>();
            var kinds=new Dictionary<CargoOwner,CargoContainerKind>(); long capacity=0, used=0; int warehouses=0;
            foreach (var container in inventory.Containers)
            { kinds.Add(container.Owner,container.Kind); if (container.Kind==CargoContainerKind.Warehouse && container.IsAvailable) { warehouses++; capacity+=container.Capacity; } }
            foreach (var definition in _resources.Catalog.Definitions)
            {
                long balance=0, warehouse=0, cargo=0, ground=0;
                foreach (var amount in inventory.Balances) if (amount.Key==definition.Id) balance=amount.Value;
                foreach (var state in inventory.Lots)
                {
                    var lot=state.Lot; if (lot.ItemType!=definition.Id || !kinds.TryGetValue(lot.Owner,out var kind)) continue;
                    if (kind==CargoContainerKind.Warehouse) warehouse+=lot.Units;
                    else if (kind==CargoContainerKind.MachineCargo) cargo+=lot.Units;
                    else if (kind==CargoContainerKind.WorldFree) ground+=lot.Units;
                }
                used+=warehouse;
                items.Add(new UiInventoryItem(definition.Id,_name?.Invoke(definition.Id) ?? KnownName(definition.Id),balance,warehouse,cargo,ground));
            }
            foreach (var receipt in inventory.Receipts)
                if (receipt.ActualUnits>0 && kinds.TryGetValue(receipt.Destination,out var kind) && kind==CargoContainerKind.Warehouse) receipts.Add(receipt);
            summary.Add(new UiDetailField("可用仓库",warehouses.ToString()));
            summary.Add(new UiDetailField("仓库占用／容量",used+" / "+capacity));
            summary.Add(new UiDetailField("库存版本",inventory.Revision.ToString()));
            summary.Add(new UiDetailField("入库记录",receipts.Count.ToString()));
            summary.Add(new UiDetailField("结算规则","基础资源与金币入全局余额；种子等占本地库存；组件和载体回库。"));
            Snapshot=new WarehouseDomainSnapshot(UiDataState.Ready,null,inventory.Revision,items,receipts,summary); _dirty=false; return true;
        }
        private static string KnownName(string id)
        { switch(id) { case ResourceItemCatalog.Gold:return "金币"; case ResourceItemCatalog.Wood:return "木材"; case ResourceItemCatalog.Ore:return "矿石"; case ResourceItemCatalog.Water:return "水"; case ResourceItemCatalog.Biomass:return "生物质"; default:return id; } }
        public void Dispose()
        { if (_disposed) return; _disposed=true; if (_resources!=null) { _resources.Authority.ContainerChanged-=OnContainerChanged; _resources.Authority.Settled-=OnSettled; } }
    }
}
