using System;
using System.Collections.Generic;
using AutoEra.Buildings;
using AutoEra.Events;
using AutoEra.Logistics;
using AutoEra.Machines;
using AutoEra.Save;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Identity;
using GameFramework;
using GameFramework.Event;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class ResourceWorldCargoEditModeTests
    {
        private AutoEraWorldSession _session;
        private CargoContainer _ground, _warehouse;
        private Publisher _publisher;
        private CargoOwnershipAuthority Authority => _session.Resources.Authority;
        [SetUp] public void SetUp()
        {
            _publisher = new Publisher(); _session = new AutoEraWorldSessionFactory().Create(1234, _publisher);
            _session.Resources.Configure(new ResourceItemCatalog(new[] {
                new ResourceItemDefinition(ResourceItemCatalog.Ore, CargoItemClass.CommonResource),
                new ResourceItemDefinition("seed", CargoItemClass.Seed),
                new ResourceItemDefinition("core", CargoItemClass.Component, 2001),
                new ResourceItemDefinition("carrier", CargoItemClass.MachineCarrier, 1001) }));
            _ground = Authority.RegisterContainer(new CargoOwner(CargoOwnerKind.WorldFree, Next()), CargoContainerKind.WorldFree, long.MaxValue);
            _warehouse = Authority.RegisterContainer(new CargoOwner(CargoOwnerKind.Receiver, Next()), CargoContainerKind.Warehouse, 120);
        }
        [TearDown] public void TearDown() { _session.Dispose(); }
        private PersistentId Next() { Assert.That(_session.IdAllocator.TryAllocate(out var id), Is.True); return id; }
        private MachineInstance Machine() => _session.Machines.Create(new MachineDefinition(1001, "Fixture", 1, 0, 1, 1, 20, true, true, 100));
        private ComponentInstance Core() => _session.Machines.CreateComponent(new ComponentDefinition(2001, HardwareKind.Core, 1, 0, 10, 10, false));
        private CargoLotSnapshot Mint(string item, int units, PersistentId payload = default)
        { Assert.That(Authority.TryMint(_ground.Owner, item, units, out var lot, out var reason, payload), Is.True, reason); return lot; }
        private ResourceReservation Reserve(CargoLotSnapshot lot, CargoOwner destination, int units)
        {
            Assert.That(Authority.TryReadContainer(destination, out var container), Is.True);
            Assert.That(Authority.TryReserve(Next(), Next(), lot.Id, lot.Version, destination, container.Generation, units, out var token, out var reason), Is.True, reason);
            return token;
        }
        [Test] public void RealCargo_ProjectsAuthorityAndSurvivesExecutionContextAndUiLifetime()
        {
            var machine = Machine(); var cargo = _session.Resources.GetCargo(machine); var lot = Mint(ResourceItemCatalog.Ore, 12);
            var load = Reserve(lot, _session.Resources.MachineOwner(machine), 12);
            using (var context = new MachineExecutionContext(machine, _session.IdAllocator, _session.Events, cargo))
            {
                Assert.That(context.Cargo, Is.SameAs(cargo)); Authority.Commit(load, 1, 12);
                Assert.That(cargo.Used, Is.EqualTo(12)); Assert.That(machine.UsedCapacity, Is.EqualTo(12));
                Assert.That(cargo.TryLoad(ResourceItemCatalog.Ore, 1), Is.False); Assert.That(cargo.TryUnload(ResourceItemCatalog.Ore, 1), Is.False);
            }
            Assert.That(_session.Resources.GetCargo(machine), Is.SameAs(cargo));
            var delivery = Reserve(new CargoLotSnapshot(lot.Id, lot.ItemType, 12, _session.Resources.MachineOwner(machine), 2), _warehouse.Owner, 12);
            Authority.Commit(delivery, 1, 12);
            Assert.That(cargo.Used, Is.Zero); Assert.That(machine.UsedCapacity, Is.Zero);
            Assert.That(Authority.Balance(ResourceItemCatalog.Ore), Is.EqualTo(12)); Assert.That(_warehouse.Used, Is.Zero);
        }
        [Test] public void InstalledCapacityAndOccupiedRemovalGate_UseSameWorldCargoWithNoContext()
        {
            var machine = Machine(); var cargo = _session.Resources.GetCargo(machine);
            var capacity = _session.Machines.CreateComponent(new ComponentDefinition(2206, HardwareKind.Effector, 1, 30, 0, 0, false));
            Assert.That(_session.Machines.Install(machine.Id, ManagementOrigin.Library, capacity.Id, 0), Is.EqualTo(MachineManagementResult.Completed));
            Assert.That(cargo.Capacity, Is.EqualTo(50));
            var lot = Mint("seed", 25); var load = Reserve(lot, _session.Resources.MachineOwner(machine), 25); Authority.Commit(load, 1, 25);
            Assert.That(_session.Machines.Remove(machine.Id, ManagementOrigin.Library, HardwareKind.Effector, 0), Is.EqualTo(MachineManagementResult.CapacityInUse));
            Assert.That(Authority.TryReadLot(lot.Id, out lot), Is.True); Authority.Commit(Reserve(lot, _warehouse.Owner, 5), 1, 5);
            Assert.That(_session.Machines.Remove(machine.Id, ManagementOrigin.Library, HardwareKind.Effector, 0), Is.EqualTo(MachineManagementResult.Completed));
            Assert.That(cargo.Capacity, Is.EqualTo(20)); Assert.That(cargo.Used, Is.EqualTo(20));
        }
        [Test] public void PackagedComponent_UsesActualIdentityAndCannotBeInstalledWhileInCargo()
        {
            var machine = Machine(); var component = Core(); var lot = Mint("core", 1, component.Id);
            Assert.That(component.CargoLotId, Is.EqualTo(lot.Id)); Assert.That(component.OwnerId.IsValid, Is.False);
            Assert.That(_session.Machines.Install(machine.Id, ManagementOrigin.Library, component.Id, 0), Is.EqualTo(MachineManagementResult.AlreadyInstalled));
            using (var presenter = new MachineHardwarePresenter(_session.Machines, machine.Id, ManagementOrigin.Library))
            { presenter.SelectSlot(HardwareKind.Core, 0); Assert.That(presenter.Candidates.Count, Is.Zero); }
            var owner = _session.Resources.MachineOwner(machine); Authority.Commit(Reserve(lot, owner, 1), 1, 1);
            Assert.That(_session.Resources.GetCargo(machine).Used, Is.EqualTo(1)); Assert.That(Authority.TryReadLot(lot.Id, out lot), Is.True);
            var receipt = Authority.Commit(Reserve(lot, _warehouse.Owner, 1), 1, 1);
            Assert.That(receipt.Route, Is.EqualTo(WarehouseDestination.ComponentLibrary)); Assert.That(component.IsInCargo, Is.False);
            Assert.That(_session.Machines.TryGetComponent(component.Id, out var same), Is.True); Assert.That(same, Is.SameAs(component));
            Assert.That(_warehouse.Used, Is.Zero); Assert.That(_session.Resources.GetCargo(machine).Used, Is.Zero);
            Assert.That(_session.Machines.Install(machine.Id, ManagementOrigin.Library, component.Id, 0), Is.EqualTo(MachineManagementResult.Completed));
        }
        [Test] public void Carrier_ReturnsSameEmptyMachineToLibraryWithoutOrdinaryCargoOrWarehouseOccupancy()
        {
            var machine = Machine(); var lot = Mint("carrier", 1, machine.Id);
            Assert.That(machine.IsInCargo, Is.True); Assert.That(_session.Machines.Deploy(machine.Id), Is.EqualTo(MachineManagementResult.InvalidState));
            var receiver = Machine(); var receiverOwner = _session.Resources.MachineOwner(receiver);
            Assert.That(Authority.TryReadContainer(receiverOwner, out var container), Is.True);
            Assert.That(Authority.TryReserve(Next(), Next(), lot.Id, lot.Version, receiverOwner, container.Generation, 1, out _, out var reason), Is.False);
            Assert.That(reason, Is.EqualTo("DestinationRejected"));
            Authority.Commit(Reserve(lot, _warehouse.Owner, 1), 1, 1);
            Assert.That(machine.IsInCargo, Is.False); Assert.That(_session.Machines.TryGet(machine.Id, out var same), Is.True);
            Assert.That(same, Is.SameAs(machine)); Assert.That(_warehouse.Used, Is.Zero);
            Assert.That(_session.Machines.Deploy(machine.Id), Is.EqualTo(MachineManagementResult.Completed));
        }
        [Test] public void WrongModelInstalledComponentOrForeignPayload_CannotEnterCargo()
        {
            var machine = Machine(); var component = Core(); _session.Machines.Install(machine.Id, ManagementOrigin.Library, component.Id, 0);
            Assert.That(Authority.TryMint(_ground.Owner, "core", 1, out _, out _, component.Id), Is.False);
            Assert.That(Authority.TryMint(_ground.Owner, "core", 1, out _, out _, Next()), Is.False);
            Assert.That(Authority.TryMint(_ground.Owner, "carrier", 1, out _, out _, machine.Id), Is.False);
            Assert.That(machine.IsInCargo || component.IsInCargo, Is.False);
        }
        [Test] public void QuantityAndTerminalFacts_KeepOneCorrelationAndActualTaskTransactionIdentity()
        {
            var token = Reserve(Mint("seed", 10), _warehouse.Owner, 10);
            Authority.Commit(token, 1, 3); Authority.Commit(token, 1, 99); Authority.Cancel(token, 2); Authority.Cancel(token, 2);
            Authority.Commit(token, 3, 2);
            Assert.That(_publisher.Results.Count, Is.EqualTo(2)); Assert.That(_publisher.Results[0].ActualUnits, Is.EqualTo(3));
            Assert.That(_publisher.Results[1].TotalCommittedUnits, Is.EqualTo(3)); Assert.That(_publisher.Results[1].State, Is.EqualTo(ResourceTransferState.Cancelled));
            Assert.That(_publisher.Results[0].TaskId, Is.EqualTo(token.TaskId)); Assert.That(_publisher.Results[0].TransactionId, Is.EqualTo(token.TransactionId));
            Assert.That(_publisher.Correlations[0], Is.EqualTo(_publisher.Correlations[1]));
            Assert.That(_session.Events.TryGetTrace(_publisher.Correlations[0], out var trace), Is.True);
            Assert.That(trace.Outcome, Is.EqualTo(EventOutcome.Cancelled));
        }
        [Test] public void ProjectionChangeObservers_NeverSeeMachineUsageBeforeItsCargoQuantity()
        {
            var machine = Machine(); var cargo = _session.Resources.GetCargo(machine); int changes = 0;
            cargo.Changed += _ => { changes++; Assert.That(machine.UsedCapacity, Is.EqualTo(cargo.Used)); };
            Authority.Commit(Reserve(Mint("seed", 10), _session.Resources.MachineOwner(machine), 10), 1, 10);
            Assert.That(changes, Is.GreaterThan(0));
        }
        [Test] public void EqualMachineIdentityFromAnotherWorld_DoesNotBindAndDisposalDetachesProjection()
        {
            var machine = Machine(); var cargo = _session.Resources.GetCargo(machine);
            using (var other = new AutoEraWorldSessionFactory().Create(0))
            {
                var foreign = other.Machines.Create(machine.Definition);
                Assert.Throws<ArgumentException>(() => _session.Resources.GetCargo(foreign));
            }
            _session.Dispose(); Assert.Throws<ObjectDisposedException>(() => _session.Resources.GetCargo(machine));
            Assert.That(Authority.IsAtCommitBoundary, Is.False);
        }
        [Test] public void MachineToMachineObservers_SeeBothCargoProjectionsAndUsagesAfterTheAtomicCommit()
        {
            var first = Machine(); var second = Machine(); var firstCargo = _session.Resources.GetCargo(first); var secondCargo = _session.Resources.GetCargo(second);
            var lot = Mint("seed", 10); Authority.Commit(Reserve(lot, _session.Resources.MachineOwner(first), 10), 1, 10);
            Assert.That(Authority.TryReadLot(lot.Id, out lot), Is.True);
            void Verify() {
                Assert.That(firstCargo.Used + secondCargo.Used, Is.EqualTo(10));
                Assert.That(first.UsedCapacity, Is.EqualTo(firstCargo.Used)); Assert.That(second.UsedCapacity, Is.EqualTo(secondCargo.Used));
                Assert.That(firstCargo.Count("seed") + secondCargo.Count("seed"), Is.EqualTo(10));
            }
            first.Changed += machine => Verify(); firstCargo.Changed += cargo => Verify(); second.Changed += machine => Verify();
            Authority.Commit(Reserve(lot, _session.Resources.MachineOwner(second), 10), 1, 10);
            Assert.That(firstCargo.Used, Is.Zero); Assert.That(secondCargo.Used, Is.EqualTo(10));
        }
        private ResourceWorldSnapshot PersistentResources()
        {
            Assert.That(_session.Resources.TryCapturePersistent(out var state),Is.True);
            string json=WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(1234,state.Cargo.AllocatedThrough,1,"Resources",new[] {new WorldSnapshotSection("resources",1,state)}));
            Assert.That(WorldSnapshotCodec.TryRead(json,new Dictionary<string,int>{{"resources",1}},out var file,out var reason),Is.True,reason);
            Assert.That(file.TryReadSection<ResourceWorldSnapshot>("resources",out var restored,out reason),Is.True,reason);return restored;
        }
        private AutoEraWorldSession ResourceCandidate(ResourceWorldSnapshot state)
        {
            var world=new AutoEraWorldSessionFactory().CreateRestoreCandidate(1234,state.Cargo.AllocatedThrough);
            world.Events.Restore(_session.Events.Capture());
            var machines=new Dictionary<(int,int),MachineDefinition>();var components=new Dictionary<(int,int),ComponentDefinition>();
            foreach(var value in _session.Machines.Machines)machines[(value.Definition.Id,value.Definition.Level)]=value.Definition;
            foreach(var value in _session.Machines.Components)components[(value.Definition.Id,value.Definition.Level)]=value.Definition;
            world.Machines.RestoreConfiguration(_session.Machines.CapturePersistentConfiguration(),(id,level)=>machines[(id,level)],(id,level)=>components[(id,level)]);
            world.Resources.Configure(_session.Resources.Catalog);return world;
        }
        [Test] public void ComponentCargoCheckpoint_RestoresOriginalCustodyAndReleasesItOnlyOnRealWarehouseCommit()
        {
            var core=Core();var lot=Mint("core",1,core.Id);var data=PersistentResources();
            using(var candidate=ResourceCandidate(data))
            {
                Assert.That(candidate.Resources.TryRestorePersistent(data,out var reason),Is.True,reason);
                Assert.That(candidate.Machines.TryGetComponent(core.Id,out var restored),Is.True);Assert.That(restored.CargoLotId,Is.EqualTo(lot.Id));
                Assert.That(candidate.Resources.Authority.TryReadLot(lot.Id,out var cargo),Is.True);
                candidate.IdAllocator.TryAllocate(out var transaction);candidate.IdAllocator.TryAllocate(out var task);
                Assert.That(candidate.Resources.Authority.TryReserve(transaction,task,lot.Id,lot.Version,_warehouse.Owner,_warehouse.Generation,1,out var token,out reason),Is.True,reason);
                candidate.Resources.Authority.Commit(token,1,1);Assert.That(restored.IsInCargo,Is.False);Assert.That(restored.OwnerId.IsValid,Is.False);
                Assert.That(core.IsInCargo,Is.True);Assert.That(cargo.Id,Is.EqualTo(lot.Id));
            }
        }
        [Test] public void CarrierCargoCheckpoint_RestoresTheOriginalUndeployedMachineIdentity()
        {
            var machine=Machine();var lot=Mint("carrier",1,machine.Id);var data=PersistentResources();
            using(var candidate=ResourceCandidate(data))
            {
                Assert.That(candidate.Resources.TryRestorePersistent(data,out var reason),Is.True,reason);
                Assert.That(candidate.Machines.TryGet(machine.Id,out var restored),Is.True);Assert.That(restored.CargoLotId,Is.EqualTo(lot.Id));
                Assert.That(restored.Deployed,Is.False);Assert.That(restored.ModelSerial,Is.EqualTo(machine.ModelSerial));
                Assert.That(candidate.Resources.Authority.TryReadPayload(lot.Id,out var payload),Is.True);Assert.That(payload,Is.EqualTo(machine.Id));
            }
        }
        [Test] public void ResourceCorrelationCheckpoint_ContinuesOriginalSettlementWithoutOpeningAnotherCommand()
        {
            var token=Reserve(Mint(ResourceItemCatalog.Ore,3),_warehouse.Owner,3);Authority.Commit(token,1,1);var data=PersistentResources();
            using(var candidate=ResourceCandidate(data))
            {
                ulong allocation=candidate.Events.Capture().CorrelationsAllocatedThrough;
                Assert.That(candidate.Resources.TryRestorePersistent(data,out var reason),Is.True,reason);
                candidate.Resources.Authority.Commit(token,1,99);candidate.Resources.Authority.Commit(token,2,99);
                Assert.That(candidate.Resources.Authority.Balance(ResourceItemCatalog.Ore),Is.EqualTo(3));
                var events=candidate.Events.Capture();Assert.That(events.CorrelationsAllocatedThrough,Is.EqualTo(allocation));
                Assert.That(events.Records[events.Records.Length-1].Correlation,Is.EqualTo(data.Correlations[0].Correlation));
                Assert.That(Authority.Balance(ResourceItemCatalog.Ore),Is.EqualTo(1));
            }
        }
        [Test] public void MissingResourceCorrelation_IsRejectedBeforeAnyCargoIsPublished()
        {
            Reserve(Mint(ResourceItemCatalog.Ore,3),_warehouse.Owner,3);var data=PersistentResources();data.Correlations=Array.Empty<ResourceWorldSnapshot.CorrelationRecord>();
            using(var candidate=ResourceCandidate(data))
            {
                Assert.That(candidate.Resources.TryRestorePersistent(data,out _),Is.False);
                Assert.That(candidate.Resources.Authority.TryCapturePersistent(out var empty),Is.True);Assert.That(empty.Lots,Is.Empty);
            }
        }
        private sealed class Publisher : IEventPublisher
        {
            public readonly List<ResourceTransferResult> Results = new List<ResourceTransferResult>();
            public readonly List<CorrelationId> Correlations = new List<CorrelationId>();
            public void Publish(GameEventArgs args)
            {
                if (args is ResourceTransferFactEventArgs fact) { Results.Add(fact.Result); Correlations.Add(fact.Correlation); }
                ReferencePool.Release(args);
            }
        }
    }
}
