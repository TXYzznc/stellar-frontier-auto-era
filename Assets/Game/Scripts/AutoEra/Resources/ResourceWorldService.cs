using System;
using System.Collections.Generic;
using AutoEra.Buildings;
using AutoEra.Machines;
using AutoEra.World.Identity;
using AutoEra.Events;
using GameFramework;

namespace AutoEra.Logistics
{
    /// <summary>The world owns cargo independently of region views, execution contexts and UI lifetime.</summary>
    public sealed partial class ResourceWorldService : IDisposable, ICargoLibrarySettlement
    {
        private readonly MachineRoster _roster;
        private readonly Dictionary<PersistentId, MachineCargo> _cargo = new Dictionary<PersistentId, MachineCargo>();
        private readonly Dictionary<PersistentId, MachineInstance> _bound = new Dictionary<PersistentId, MachineInstance>();
        private ResourceItemCatalog _catalog;
        private readonly AutoEraEventService _events;
        private readonly Dictionary<PersistentId, CorrelationId> _correlations = new Dictionary<PersistentId, CorrelationId>();
        private bool _disposed, _custodyChanged;
        public CargoOwnershipAuthority Authority { get; }
        public ResourceItemCatalog Catalog => _catalog;
        public TransportResponsibilityLedger Transport { get; } = new TransportResponsibilityLedger();
        public bool CatalogReady { get; private set; }
        public ResourceWorldService(PersistentIdAllocator ids, PersistentObjectRegistry registry, MachineRoster roster,
            AutoEraEventService events = null, Action<Exception> observerError = null)
        {
            _roster = roster ?? throw new ArgumentNullException(nameof(roster));
            _events = events;
            _catalog = new ResourceItemCatalog(Array.Empty<ResourceItemDefinition>());
            Authority = new CargoOwnershipAuthority(ids, _catalog, registry, this, observerError);
            Authority.ContainerChanged += OnContainerChanged;
            Authority.Projecting += OnProjecting;
            Authority.Transferring += Transport.StageCommit;
            Authority.Committed += Transport.NotifyCommit;
            Authority.Reserved += OnReserved; Authority.Settled += OnSettled;
        }
        public void Configure(ResourceItemCatalog catalog)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ResourceWorldService));
            if (CatalogReady) throw new InvalidOperationException("Resource catalog already configured for this world.");
            Authority.Configure(catalog, this); _catalog = catalog; CatalogReady = true;
        }
        public MachineCargo GetCargo(MachineInstance machine)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ResourceWorldService));
            if (machine == null || !_roster.TryGet(machine.Id, out var existing) || !ReferenceEquals(existing, machine))
                throw new ArgumentException("Cargo belongs to a machine in this world.");
            if (_cargo.TryGetValue(machine.Id, out var cargo)) return cargo;
            var owner = new CargoOwner(CargoOwnerKind.Receiver, machine.Id);
            if (!Authority.TryReadContainer(owner, out var container))
            {
                if (machine.UsedCapacity != 0) throw new InvalidOperationException("Restore actual cargo before creating its projection.");
                container = Authority.RegisterContainer(owner, CargoContainerKind.MachineCargo, machine.TotalCapacity);
            }
            if (container.Kind != CargoContainerKind.MachineCargo || container.Capacity != machine.TotalCapacity || container.Used != machine.UsedCapacity)
                throw new InvalidOperationException("Restored machine cargo differs from machine capacity/usage.");
            cargo = new MachineCargo(Authority, container); _cargo.Add(machine.Id, cargo); _bound.Add(machine.Id, machine);
            machine.Changed += OnMachineChanged; return cargo;
        }
        public CargoOwner MachineOwner(MachineInstance machine) { GetCargo(machine); return new CargoOwner(CargoOwnerKind.Receiver, machine.Id); }
        private void OnMachineChanged(MachineInstance machine)
        {
            var owner = new CargoOwner(CargoOwnerKind.Receiver, machine.Id);
            if (Authority.TryReadContainer(owner, out var container) && container.Capacity != machine.TotalCapacity && !Authority.TryResize(owner, machine.TotalCapacity))
                throw new InvalidOperationException("Hardware changed below occupied/reserved cargo capacity.");
        }
        private void OnContainerChanged(CargoContainer container)
        {
            if (_custodyChanged) { _custodyChanged = false; _roster.NotifyCargoCustodyChanged(); }
        }
        private void OnProjecting(CargoProjectionChange change)
        {
            MachineCargo sourceCargo = null, targetCargo = null;
            MachineInstance sourceMachine = null, targetMachine = null;
            bool sourceChanged = PrepareProjection(change.Source, out sourceCargo, out sourceMachine);
            bool targetChanged = !ReferenceEquals(change.Source, change.Destination) && PrepareProjection(change.Destination, out targetCargo, out targetMachine);
            // Stage both cargo views and both machine usages before the first public observer runs.
            if (sourceChanged) sourceMachine.NotifyChanged();
            if (targetChanged) targetMachine.NotifyChanged();
            sourceCargo?.NotifyProjectionChanged(); targetCargo?.NotifyProjectionChanged();
        }
        private bool PrepareProjection(CargoContainer container, out MachineCargo cargo, out MachineInstance machine)
        {
            cargo = null; machine = null;
            if (container == null || container.Kind != CargoContainerKind.MachineCargo || !_cargo.TryGetValue(container.Owner.Id, out cargo)) return false;
            machine = _bound[container.Owner.Id]; cargo.RefreshProjection(false);
            return machine.SetContainerUsageSilently(cargo.Used);
        }
        private void OnReserved(ResourceReservationCreated created)
        {
            if (_events == null) return;
            var token = created.Reservation;
            _correlations.Add(token.TransactionId, _events.OpenCommand(EventDomain.Resource, token.Source.Id, "ReserveTransfer"));
        }
        private void OnSettled(ResourceTransferCommitted settled)
        {
            if (_events == null) return;
            var result = settled.Result;
            if (!_correlations.TryGetValue(result.TransactionId, out var correlation))
                throw new InvalidOperationException("Resource transaction has no responsibility correlation.");
            _events.PublishFact(ReferencePool.Acquire<ResourceTransferFactEventArgs>().Initialize(correlation, result));
        }
        public bool ValidateEntering(ResourceItemDefinition item, PersistentId payload, out string reason)
        {
            reason = null;
            if (_disposed || !_roster.IsActive) { reason = "LibraryUnavailable"; return false; }
            if (item.Class == CargoItemClass.Component && _roster.TryGetComponent(payload, out var component) &&
                component.Definition.Id == item.ModelId && !component.OwnerId.IsValid && !component.IsInCargo) return true;
            if (item.Class == CargoItemClass.MachineCarrier && _roster.TryGet(payload, out var machine) &&
                machine.Definition.Id == item.ModelId && !machine.IsInCargo && !machine.Deployed && machine.UsedCapacity == 0 && !machine.HasActiveBehavior)
            {
                foreach (HardwareKind kind in Enum.GetValues(typeof(HardwareKind)))
                    for (int i = 0; i < machine.Definition.SlotCount(kind); i++) if (machine.GetComponent(kind, i) != null)
                    { reason = "CarrierNotEmpty"; return false; }
                return true;
            }
            reason = "InvalidLibraryPayload"; return false;
        }
        public void Enter(CargoLotSnapshot lot, PersistentId payload)
        {
            if (_roster.TryGetComponent(payload, out var component)) component.CargoLotId = lot.Id;
            else if (_roster.TryGet(payload, out var machine)) machine.CargoLotId = lot.Id;
            _custodyChanged = true;
        }
        public bool Validate(CargoLotSnapshot lot, PersistentId payload, WarehouseDestination destination, out string reason)
        {
            reason = "LibraryPayloadChanged";
            if (_disposed || !_roster.IsActive || !_catalog.TryGet(lot.ItemType, out var item) || lot.Units != 1) return false;
            if (destination == WarehouseDestination.ComponentLibrary && item.Class == CargoItemClass.Component &&
                _roster.TryGetComponent(payload, out var component) && component.CargoLotId == lot.Id && !component.OwnerId.IsValid && component.Definition.Id == item.ModelId)
            { reason = null; return true; }
            if (destination == WarehouseDestination.MachineLibrary && item.Class == CargoItemClass.MachineCarrier &&
                _roster.TryGet(payload, out var machine) && machine.CargoLotId == lot.Id && !machine.Deployed && machine.Definition.Id == item.ModelId)
            { reason = null; return true; }
            return false;
        }
        public void Commit(CargoLotSnapshot lot, PersistentId payload, WarehouseDestination destination)
        {
            if (destination == WarehouseDestination.ComponentLibrary && _roster.TryGetComponent(payload, out var component)) component.CargoLotId = default;
            else if (destination == WarehouseDestination.MachineLibrary && _roster.TryGet(payload, out var machine)) machine.CargoLotId = default;
            _custodyChanged = true;
        }
        public void Dispose()
        {
            if (_disposed) return; _disposed = true;
            foreach (var machine in _bound.Values) machine.Changed -= OnMachineChanged;
            Authority.ContainerChanged -= OnContainerChanged; Authority.Dispose(); _bound.Clear(); _cargo.Clear();
            _correlations.Clear(); Transport.Release();
        }
    }
}
