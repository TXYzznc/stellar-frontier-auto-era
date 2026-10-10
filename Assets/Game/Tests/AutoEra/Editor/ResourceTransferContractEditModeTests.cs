using System;
using System.Collections.Generic;
using AutoEra.Buildings;
using AutoEra.Logistics;
using AutoEra.Machines;
using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class ResourceTransferContractEditModeTests
    {
        private static ResourceReservation Reservation(int units = 10)
        {
            var source = new CargoLotSnapshot(new PersistentId(1), "ore", units, new CargoOwner(CargoOwnerKind.Receiver, new PersistentId(2)), 1);
            return new ResourceReservation(new PersistentId(3), new PersistentId(4), source, new CargoOwner(CargoOwnerKind.Receiver, new PersistentId(5)), 2, units);
        }
        [Test]
        public void CargoLoad_IntegerOverflowCannotBypassCapacityOrPublishAChange()
        {
            var cargo = new MachineCargo(20); cargo.TryLoad("ore", 10); int changes = 0; cargo.Changed += _ => changes++;
            Assert.That(cargo.TryLoad("ore", int.MaxValue), Is.False);
            Assert.That(cargo.Used, Is.EqualTo(10)); Assert.That(cargo.Count("ore"), Is.EqualTo(10)); Assert.That(changes, Is.Zero);
        }
        [Test]
        public void CargoItems_CannotBypassQuantityAndCapacityAuthority()
        {
            var cargo = new MachineCargo(20); cargo.TryLoad("ore", 3); var view = cargo.Items;
            Assert.Throws<NotSupportedException>(() => ((IDictionary<string, int>)view)["ore"] = 200);
            Assert.That(cargo.Used, Is.EqualTo(3)); cargo.TryUnload("ore", 1);
            Assert.That(view["ore"], Is.EqualTo(2)); Assert.That(cargo.Items, Is.SameAs(view), "Reading inventory must not allocate another wrapper.");
        }
        [Test]
        public void Budget_UsesSourceAndDestinationBoundsWithoutOverflow()
        {
            Assert.That(ResourceTransferBudget.TryPlan(int.MaxValue, int.MaxValue, 3, out var quantity), Is.True); Assert.That(quantity, Is.EqualTo(3));
            Assert.That(ResourceTransferBudget.TryPlan(10, 4, int.MaxValue, out quantity), Is.True); Assert.That(quantity, Is.EqualTo(4));
            Assert.That(ResourceTransferBudget.TryPlan(10, -1, 3, out quantity), Is.False); Assert.That(quantity, Is.Zero);
            Assert.That(ResourceTransferBudget.TryPlan(10, 10, 0, out quantity), Is.False); Assert.That(quantity, Is.Zero);
        }
        [Test]
        public void Reservation_RejectsUnresolvedOwnerOrUnboundedPromise()
        {
            Assert.Throws<ArgumentException>(() => new CargoOwner(CargoOwnerKind.Receiver, PersistentId.Invalid));
            var owner = new CargoOwner(CargoOwnerKind.Receiver, new PersistentId(2));
            var lot = new CargoLotSnapshot(new PersistentId(1), "ore", 10, owner, 1);
            Assert.Throws<ArgumentException>(() => new ResourceReservation(new PersistentId(3), new PersistentId(4), lot, owner, 1, 10));
            Assert.Throws<ArgumentException>(() => new ResourceReservation(new PersistentId(3), new PersistentId(4), lot, new CargoOwner(CargoOwnerKind.Receiver, new PersistentId(5)), 1, 11));
        }
        [Test]
        public void CancellationReceipt_RetainsPartialCommittedQuantityAndTaskResponsibility()
        {
            var token = Reservation();
            var result = new ResourceTransferResult(token, 4, 0, 3, 0, ResourceTransferState.Cancelled);
            Assert.That(result.TaskId, Is.EqualTo(token.TaskId)); Assert.That(result.TotalCommittedUnits, Is.EqualTo(3)); Assert.That(result.ActualUnits, Is.Zero);
            Assert.Throws<ArgumentException>(() => new ResourceTransferResult(token, 4, 0, 3, 7, ResourceTransferState.Cancelled));
            Assert.Throws<ArgumentException>(() => new ResourceTransferResult(token, 5, 8, 11, 0, ResourceTransferState.Completed));
        }
        [Test]
        public void WarehouseRouting_UsesLatestCommonRulesAndRejectsUnknownItems()
        {
            Assert.That(WarehouseClassification.Classify(CargoItemClass.CommonResource), Is.EqualTo(WarehouseDestination.GlobalBalance));
            Assert.That(WarehouseClassification.OccupiesLocalCapacity(CargoItemClass.CommonResource), Is.False);
            Assert.That(WarehouseClassification.Classify(CargoItemClass.Seed), Is.EqualTo(WarehouseDestination.LocalInventory));
            Assert.That(WarehouseClassification.Classify(CargoItemClass.Component), Is.EqualTo(WarehouseDestination.ComponentLibrary));
            Assert.That(WarehouseClassification.Classify(CargoItemClass.MachineCarrier), Is.EqualTo(WarehouseDestination.MachineLibrary));
            Assert.That(WarehouseClassification.Classify((CargoItemClass)100), Is.EqualTo(WarehouseDestination.Rejected));
        }
    }
}
