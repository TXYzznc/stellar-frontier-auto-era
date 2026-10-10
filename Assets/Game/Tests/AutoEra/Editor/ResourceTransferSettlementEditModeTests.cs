using System;
using System.Collections.Generic;
using AutoEra.Buildings;
using AutoEra.Logistics;
using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class ResourceTransferSettlementEditModeTests
    {
        private PersistentIdAllocator _ids;
        private PersistentObjectRegistry _registry;
        private CargoOwnershipAuthority _authority;
        private CargoContainer _source, _target, _warehouse;
        private static ResourceItemCatalog Catalog() => new ResourceItemCatalog(new[] {
            new ResourceItemDefinition(ResourceItemCatalog.Ore, CargoItemClass.CommonResource),
            new ResourceItemDefinition("seed", CargoItemClass.Seed),
            new ResourceItemDefinition("crop", CargoItemClass.FarmProduct),
            new ResourceItemDefinition("component", CargoItemClass.Component, 2001),
            new ResourceItemDefinition("carrier", CargoItemClass.MachineCarrier, 1001),
            new ResourceItemDefinition(ResourceItemCatalog.Gold, CargoItemClass.Gold) });
        [SetUp] public void SetUp()
        {
            _ids = new PersistentIdAllocator(); _registry = new PersistentObjectRegistry(_ids);
            _authority = new CargoOwnershipAuthority(_ids, Catalog(), _registry);
            _source = _authority.RegisterContainer(new CargoOwner(CargoOwnerKind.WorldFree, Next()), CargoContainerKind.WorldFree, long.MaxValue);
            _target = _authority.RegisterContainer(new CargoOwner(CargoOwnerKind.Receiver, Next()), CargoContainerKind.MachineCargo, 20);
            _warehouse = _authority.RegisterContainer(new CargoOwner(CargoOwnerKind.Receiver, Next()), CargoContainerKind.Warehouse, 120);
        }
        [TearDown] public void TearDown() { _authority.Dispose(); }
        private PersistentId Next() { Assert.That(_ids.TryAllocate(out var id), Is.True); return id; }
        private CargoLotSnapshot Mint(string item, int units)
        { Assert.That(_authority.TryMint(_source.Owner, item, units, out var lot, out var reason), Is.True, reason); return lot; }
        private ResourceReservation Reserve(CargoLotSnapshot lot, CargoContainer destination, int amount)
        {
            Assert.That(_authority.TryReserve(Next(), Next(), lot.Id, lot.Version, destination.Owner, destination.Generation, amount, out var token, out var reason), Is.True, reason);
            return token;
        }
        [Test] public void CompetingConsumers_ReserveSourceOnceAndRespectSharedCapacity()
        {
            var lot = Mint(ResourceItemCatalog.Ore, 30); var first = Reserve(lot, _target, 14); var second = Reserve(lot, _target, 14);
            Assert.That(second.CommittedLimit, Is.EqualTo(6)); Assert.That(_target.Remaining, Is.Zero);
            Assert.That(_authority.TryReserve(Next(), Next(), lot.Id, lot.Version, _target.Owner, _target.Generation, 1, out _, out _), Is.False);
            Assert.That(_authority.Commit(first, 1, 100).ActualUnits, Is.EqualTo(14));
            Assert.That(_authority.Commit(second, 1, 100).ActualUnits, Is.EqualTo(6));
            Assert.That(_source.Used, Is.EqualTo(10)); Assert.That(_target.Used, Is.EqualTo(20));
        }
        [Test] public void RepeatedCompletion_ReturnsOriginalReceiptAndPublishesOneFact()
        {
            var token = Reserve(Mint(ResourceItemCatalog.Ore, 10), _target, 10); int facts = 0;
            _authority.Committed += _ => facts++;
            var first = _authority.Commit(token, 1, 4); var repeated = _authority.Commit(token, 1, 999);
            Assert.That(repeated.ActualUnits, Is.EqualTo(4)); Assert.That(repeated.ReceivedLotId, Is.EqualTo(first.ReceivedLotId));
            Assert.That(_target.Used, Is.EqualTo(4)); Assert.That(facts, Is.EqualTo(1));
            _authority.Cancel(token, 2); Assert.That(_authority.Commit(token, 1, 4).State, Is.EqualTo(ResourceTransferState.Partial));
            Assert.That(_authority.Commit(token, 3, 4).ActualUnits, Is.Zero); Assert.That(facts, Is.EqualTo(1));
        }
        [Test] public void PartialCancel_PreservesCommittedUnitsAndReleasesBothClaims()
        {
            var lot = Mint(ResourceItemCatalog.Ore, 10); var token = Reserve(lot, _target, 10);
            var first = _authority.Commit(token, 1, 3); var cancelled = _authority.Cancel(token, 2);
            Assert.That(cancelled.TotalCommittedUnits, Is.EqualTo(3)); Assert.That(cancelled.RemainingReservedUnits, Is.Zero);
            Assert.That(_target.Remaining, Is.EqualTo(17)); Assert.That(_source.Used, Is.EqualTo(7));
            Assert.That(_authority.TryReadLot(first.ReceivedLotId, out var received), Is.True); Assert.That(received.Owner, Is.EqualTo(_target.Owner));
            var remainder = Reserve(lot, _target, 100); Assert.That(remainder.CommittedLimit, Is.EqualTo(7));
            _authority.Commit(remainder, 1, 100); Assert.That(_target.Used, Is.EqualTo(10));
        }
        [Test] public void PartialHandoff_SplitsIdentityWhileFullHandoffKeepsIdentityAndChangesOwnerVersion()
        {
            var lot = Mint(ResourceItemCatalog.Ore, 10); var token = Reserve(lot, _target, 10);
            var part = _authority.Commit(token, 1, 3);
            Assert.That(part.ReceivedLotId, Is.Not.EqualTo(lot.Id)); Assert.That(_authority.TryReadLot(lot.Id, out var source), Is.True);
            Assert.That(source.Version, Is.EqualTo(1)); Assert.That(source.Units, Is.EqualTo(7));
            var rest = _authority.Commit(token, 2, 7); Assert.That(rest.ReceivedLotId, Is.EqualTo(lot.Id));
            Assert.That(_authority.TryReadLot(lot.Id, out var moved), Is.True); Assert.That(moved.Version, Is.EqualTo(2));
            Assert.That(moved.Owner, Is.EqualTo(_target.Owner));
            Assert.That(_authority.TryReserve(Next(), Next(), lot.Id, 1, _warehouse.Owner, 1, 1, out _, out _), Is.False);
        }
        [Test] public void SourceCompetition_SecondReservationCanCommitAfterFirstPartialSplit()
        {
            var lot = Mint("seed", 10); var first = Reserve(lot, _target, 6); var second = Reserve(lot, _warehouse, 9);
            Assert.That(second.CommittedLimit, Is.EqualTo(4)); _authority.Commit(first, 1, 3); _authority.Commit(second, 1, 4);
            _authority.Commit(first, 2, 3); Assert.That(_source.Used, Is.Zero); Assert.That(_target.Used + _warehouse.Used, Is.EqualTo(10));
        }
        [Test] public void GenerationChange_CancelsRemainingReservationWithoutUndoingActualDelivery()
        {
            var token = Reserve(Mint("seed", 10), _warehouse, 10); _authority.Commit(token, 1, 3);
            var changed = new HashSet<CargoOwner>(); _authority.ContainerChanged += c => changed.Add(c.Owner);
            Assert.That(_authority.TryResize(_warehouse.Owner, 3), Is.True); Assert.That(_warehouse.Generation, Is.EqualTo(2));
            var result = _authority.Commit(token, 2, 7); Assert.That(result.State, Is.EqualTo(ResourceTransferState.Cancelled));
            Assert.That(result.TotalCommittedUnits, Is.EqualTo(3)); Assert.That(_source.Used, Is.EqualTo(7));
            Assert.That(changed, Does.Contain(_source.Owner)); Assert.That(_authority.TryResize(_warehouse.Owner, 2), Is.False);
        }
        [Test] public void WarehouseCommonResources_ConvertOnceWithoutTakingEntityCapacityOrAllowingWithdrawal()
        {
            var seed = Reserve(Mint("seed", 120), _warehouse, 120); _authority.Commit(seed, 1, 120);
            var ore = Mint(ResourceItemCatalog.Ore, 200); var token = Reserve(ore, _warehouse, 200);
            Assert.That(_authority.Commit(token, 1, 200).Route, Is.EqualTo(WarehouseDestination.GlobalBalance));
            Assert.That(_authority.Balance(ResourceItemCatalog.Ore), Is.EqualTo(200)); Assert.That(_warehouse.Used, Is.EqualTo(120));
            Assert.That(_warehouse.Count(ResourceItemCatalog.Ore), Is.Zero); Assert.That(_authority.TryReadLot(ore.Id, out _), Is.False);
            _authority.Commit(token, 1, 200); Assert.That(_authority.Balance(ResourceItemCatalog.Ore), Is.EqualTo(200));
            Assert.That(_authority.TryFindAvailableLot(_warehouse.Owner, ResourceItemCatalog.Ore, out _), Is.False);
        }
        [Test] public void UnknownItemGoldAndUnverifiedLibraryPayload_DoNotCreateAnInventory()
        {
            Assert.That(_authority.TryMint(_source.Owner, "unknown", 1, out _, out _), Is.False);
            Assert.That(_authority.TryMint(_source.Owner, ResourceItemCatalog.Gold, 1, out _, out _), Is.False);
            Assert.That(_authority.TryMint(_source.Owner, "component", 1, out _, out _), Is.False);
            Assert.That(_authority.TryMint(_target.Owner, "carrier", 1, out _, out _, Next()), Is.False);
            var payload = Next(); Assert.That(_authority.TryMint(_source.Owner, "component", 1, out _, out var reason, payload), Is.False);
            Assert.That(reason, Is.EqualTo("LibraryUnavailable"));
        }
        [Test] public void ForgedTokenAndConflictingTransaction_RejectBeforeMutation()
        {
            var lot = Mint("seed", 10); var token = Reserve(lot, _target, 5);
            Assert.That(_authority.TryReserve(token.TransactionId, token.TaskId, lot.Id, lot.Version, _target.Owner, 1, 5, out var same, out _), Is.True);
            Assert.That(same.CommittedLimit, Is.EqualTo(5)); Assert.That(_target.Remaining, Is.EqualTo(15));
            Assert.That(_authority.TryReserve(token.TransactionId, token.TaskId, lot.Id, lot.Version, _target.Owner, 1, 6, out _, out _), Is.False);
            var forged = new ResourceReservation(token.TransactionId, Next(), lot, _target.Owner, 1, 5);
            Assert.Throws<ArgumentException>(() => _authority.Commit(forged, 1, 5)); Assert.That(_source.Used, Is.EqualTo(10));
        }
        [Test] public void OutOfOrderCallback_DoesNotAdvanceOrDestroyTheRemainingReservation()
        {
            var token = Reserve(Mint("seed", 10), _target, 10); _authority.Commit(token, 4, 2);
            var stale = _authority.Commit(token, 3, 3); Assert.That(stale.State, Is.EqualTo(ResourceTransferState.Waiting));
            Assert.That(stale.ActualUnits, Is.Zero); Assert.That(_authority.TryReadResult(token.TransactionId, 3, out _), Is.False);
            Assert.That(_authority.Commit(token, 5, 8).TotalCommittedUnits, Is.EqualTo(10));
        }
        [Test] public void Notifications_SeeCompleteStateAndCannotReenterMutation()
        {
            var lot = Mint("seed", 10); var token = Reserve(lot, _target, 10); bool observed = false;
            _authority.Committed += fact => {
                observed = true; Assert.That(_authority.IsAtCommitBoundary, Is.False);
                Assert.That(_source.Used + _target.Used, Is.EqualTo(10));
                Assert.That(_authority.TryReadResult(token.TransactionId, 1, out var stored), Is.True);
                Assert.That(stored.ActualUnits, Is.EqualTo(fact.Result.ActualUnits));
                Assert.That(_authority.TryMint(_source.Owner, "seed", 1, out _, out var reason), Is.False); Assert.That(reason, Is.EqualTo("CommitInProgress"));
                Assert.That(_authority.Commit(token, 2, 1).State, Is.EqualTo(ResourceTransferState.Waiting));
            };
            _authority.Commit(token, 1, 4); Assert.That(observed, Is.True); Assert.That(_authority.IsAtCommitBoundary, Is.True);
            Assert.That(_authority.TryReadResult(token.TransactionId, 2, out _), Is.False);
        }
        [Test] public void BrokenObserver_DoesNotHideCommitFromOtherObserversOrAllowRetryToDuplicateIt()
        {
            var token = Reserve(Mint("seed", 10), _target, 10); int facts = 0;
            _authority.ContainerChanged += _ => throw new InvalidOperationException("test-observer");
            _authority.Committed += _ => facts++;
            Assert.Throws<AggregateException>(() => _authority.Commit(token, 1, 10));
            Assert.That(_authority.IsAtCommitBoundary, Is.True); Assert.That(_target.Used, Is.EqualTo(10)); Assert.That(facts, Is.EqualTo(1));
            Assert.That(_authority.Commit(token, 1, 10).ActualUnits, Is.EqualTo(10)); Assert.That(facts, Is.EqualTo(1));
        }
        [Test] public void ContainerViews_AreReadOnlyAndWorldDisposeUnregistersItsLotsAndTransactions()
        {
            var lot = Mint("seed", 10); var token = Reserve(lot, _target, 10);
            Assert.Throws<NotSupportedException>(() => ((IDictionary<string, long>)_source.Items)["seed"] = 100);
            Assert.That(_registry.Count, Is.EqualTo(2)); var last = Next(); _authority.Dispose();
            Assert.That(_registry.Count, Is.Zero); Assert.That(Next().Value, Is.GreaterThan(last.Value));
            Assert.That(_authority.TryReadLot(lot.Id, out _), Is.False); Assert.That(_authority.IsAtCommitBoundary, Is.False);
        }
        [TestCase(7)] [TestCase(91)] [TestCase(203)] public void RandomizedReserveCommitCancel_ConservesAllPhysicalAndConvertedUnits(int seed)
        {
            var random = new Random(seed); long minted = 0;
            for (int i = 0; i < 400; i++)
            {
                int units = random.Next(1, 25); var lot = Mint(ResourceItemCatalog.Ore, units); minted += units;
                var destination = random.Next(2) == 0 ? _warehouse : _target;
                if (!_authority.TryReserve(Next(), Next(), lot.Id, lot.Version, destination.Owner, destination.Generation, random.Next(1, 30), out var token, out _)) continue;
                var result = _authority.Commit(token, 1, random.Next(1, 10));
                Assert.That(_authority.Commit(token, 1, 999).ActualUnits, Is.EqualTo(result.ActualUnits));
                if (random.Next(2) == 0) _authority.Cancel(token, 2); else _authority.Commit(token, 2, 100);
                if (_authority.TryFindAvailableLot(_target.Owner, ResourceItemCatalog.Ore, out var cargo))
                { var unload = Reserve(cargo, _warehouse, cargo.Units); _authority.Commit(unload, 1, cargo.Units); }
                Assert.That(_source.Used + _target.Used + _authority.Balance(ResourceItemCatalog.Ore), Is.EqualTo(minted));
                Assert.That(_target.Used, Is.LessThanOrEqualTo(_target.Capacity));
            }
        }
        [Test] public void Snapshot_IsDetachedAndIncludesUncommittedResponsibilitiesAndReceipts()
        {
            var lot = Mint("seed", 10); var token = Reserve(lot, _target, 10); _authority.Commit(token, 1, 3);
            Assert.That(_authority.TryCapture(out var snapshot), Is.True); Assert.That(snapshot.Lots.Count, Is.EqualTo(2));
            Assert.That(snapshot.Transactions[0].TotalCommittedUnits, Is.EqualTo(3)); Assert.That(snapshot.Transactions[0].RemainingReservedUnits, Is.EqualTo(7));
            Assert.That(snapshot.Receipts[0].ActualUnits, Is.EqualTo(3));
            Assert.Throws<NotSupportedException>(() => ((IList<CargoLotState>)snapshot.Lots).Clear());
            bool rejectedDuringCommit = false;
            _authority.Committed += fact => rejectedDuringCommit = !_authority.TryCapture(out _);
            _authority.Commit(token, 2, 7); Assert.That(rejectedDuringCommit, Is.True);
            Assert.That(snapshot.Transactions[0].RemainingReservedUnits, Is.EqualTo(7));
            Assert.That(snapshot.Lots[0].Lot.Units + snapshot.Lots[1].Lot.Units, Is.EqualTo(10));
            Assert.That(_authority.TryCapture(out var current), Is.True); Assert.That(current.Transactions[0].RemainingReservedUnits, Is.Zero);
        }
        [Test] public void PersistentIdentities_DoNotAliasContainersLotsTransactionsOrTasks()
        {
            var lot = Mint("seed", 10);
            Assert.That(_authority.TryReserve(_source.Owner.Id, Next(), lot.Id, 1, _target.Owner, 1, 1, out _, out _), Is.False);
            var id = Next(); Assert.That(_authority.TryReserve(id, id, lot.Id, 1, _target.Owner, 1, 1, out _, out _), Is.False);
            Assert.That(_authority.TryReserve(Next(), lot.Id, lot.Id, 1, _target.Owner, 1, 1, out _, out _), Is.False);
            Assert.Throws<ArgumentException>(() => _authority.RegisterContainer(new CargoOwner(CargoOwnerKind.OutputPort, lot.Id), CargoContainerKind.Store, 10));
            Assert.That(_source.Used, Is.EqualTo(10)); Assert.That(_target.Used, Is.Zero);
        }
        [Test] public void WarehousePhysicalItems_KeepTheirIdentityAndCannotBeReadFromAnotherWarehouse()
        {
            var other = _authority.RegisterContainer(new CargoOwner(CargoOwnerKind.Receiver, Next()), CargoContainerKind.Warehouse, 120);
            var lot = Mint("seed", 5); _authority.Commit(Reserve(lot, _warehouse, 5), 1, 5);
            Assert.That(_authority.TryReadLot(lot.Id, out var received), Is.True);
            Assert.That(received.Id, Is.EqualTo(lot.Id)); Assert.That(received.Owner, Is.EqualTo(_warehouse.Owner));
            Assert.That(_warehouse.Count("seed"), Is.EqualTo(5)); Assert.That(other.Count("seed"), Is.Zero);
            Assert.That(_authority.TryFindAvailableLot(other.Owner, "seed", out _), Is.False); Assert.That(_authority.Balance("seed"), Is.Zero);
        }
    }
}
