using System;
using System.Collections.Generic;
using AutoEra.Logistics;
using AutoEra.Save;
using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class CargoOwnershipSnapshotEditModeTests
    {
        private sealed class Fixture : IDisposable
        {
            internal readonly PersistentIdAllocator Ids=new PersistentIdAllocator();
            internal readonly PersistentObjectRegistry Registry;
            internal readonly CargoOwnershipAuthority Authority;
            internal CargoContainer Source,Target,Warehouse;
            internal Fixture(bool populate=true)
            {
                Registry=new PersistentObjectRegistry(Ids);
                Authority=new CargoOwnershipAuthority(Ids,new ResourceItemCatalog(new[] {new ResourceItemDefinition(ResourceItemCatalog.Ore,AutoEra.Buildings.CargoItemClass.CommonResource)}),Registry);
                if(populate)
                {
                    Source=Authority.RegisterContainer(new CargoOwner(CargoOwnerKind.WorldFree,Next()),CargoContainerKind.WorldFree,100);
                    Target=Authority.RegisterContainer(new CargoOwner(CargoOwnerKind.Receiver,Next()),CargoContainerKind.MachineCargo,20);
                    Warehouse=Authority.RegisterContainer(new CargoOwner(CargoOwnerKind.Receiver,Next()),CargoContainerKind.Warehouse,100);
                }
            }
            internal PersistentId Next() {Assert.That(Ids.TryAllocate(out var id),Is.True);return id;}
            internal CargoLotSnapshot Mint(int units)
            {Assert.That(Authority.TryMint(Source.Owner,ResourceItemCatalog.Ore,units,out var lot,out var reason),Is.True,reason);return lot;}
            internal ResourceReservation Reserve(CargoLotSnapshot lot,CargoContainer target,int units)
            {Assert.That(Authority.TryReserve(Next(),Next(),lot.Id,lot.Version,target.Owner,target.Generation,units,out var token,out var reason),Is.True,reason);return token;}
            internal CargoOwnershipSnapshot Capture()
            {Assert.That(Authority.TryCapturePersistent(out var state),Is.True);return Json(state);}
            internal void Restore(CargoOwnershipSnapshot saved)
            {
                Assert.That(Authority.TryRestorePersistent(saved,out var reason),Is.True,reason);
                foreach(var row in saved.Containers)
                {
                    Assert.That(Authority.TryReadContainer(new CargoOwner(row.OwnerKind,new PersistentId(row.Owner)),out var container),Is.True);
                    if(row.Kind==CargoContainerKind.WorldFree)Source=container;
                    else if(row.Kind==CargoContainerKind.MachineCargo)Target=container;
                    else Warehouse=container;
                }
            }
            public void Dispose()=>Authority.Dispose();
        }
        private static CargoOwnershipSnapshot Json(CargoOwnershipSnapshot state)
        {
            string json=WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(1,state.AllocatedThrough,1,"Cargo",new[] {new WorldSnapshotSection("cargo",1,state)}));
            Assert.That(WorldSnapshotCodec.TryRead(json,new Dictionary<string,int>{{"cargo",1}},out var file,out var reason),Is.True,reason);
            Assert.That(file.TryReadSection<CargoOwnershipSnapshot>("cargo",out var restored,out reason),Is.True,reason);return restored;
        }

        [Test] public void PartialReservation_RestoresOwnerCapacityAndOriginalReceipt_ContinuesOnce()
        {
            using(var source=new Fixture())using(var target=new Fixture(false))
            {
                var token=source.Reserve(source.Mint(10),source.Target,8);var first=source.Authority.Commit(token,1,3);
                int facts=0;target.Authority.Committed+=_=>facts++;var state=source.Capture();target.Restore(state);
                Assert.That(facts,Is.Zero);Assert.That(target.Target.Used,Is.EqualTo(3));Assert.That(target.Target.Remaining,Is.EqualTo(12));
                var repeated=target.Authority.Commit(token,1,999);
                Assert.That(repeated.ReceivedLotId,Is.EqualTo(first.ReceivedLotId));Assert.That(repeated.ActualUnits,Is.EqualTo(3));Assert.That(facts,Is.Zero);
                var completed=target.Authority.Commit(token,2,99);Assert.That(completed.ActualUnits,Is.EqualTo(5));Assert.That(facts,Is.EqualTo(1));
                Assert.That(target.Source.Used,Is.EqualTo(2));Assert.That(target.Target.Used,Is.EqualTo(8));Assert.That(target.Target.Remaining,Is.EqualTo(12));
                Assert.That(source.Source.Used,Is.EqualTo(7));Assert.That(source.Target.Used,Is.EqualTo(3));
                Assert.That(target.Ids.NextId.Value,Is.GreaterThan(state.AllocatedThrough));
            }
        }
        [Test] public void CancelledReceipt_RetainsTerminalAcknowledgementAndNeverSettlesTwice()
        {
            using(var source=new Fixture())using(var target=new Fixture(false))
            {
                var token=source.Reserve(source.Mint(10),source.Target,8);source.Authority.Commit(token,1,3);source.Authority.Cancel(token,2);
                int settled=0;target.Authority.Settled+=_=>settled++;target.Restore(source.Capture());
                Assert.That(target.Authority.Cancel(token,2).State,Is.EqualTo(ResourceTransferState.Cancelled));
                Assert.That(target.Authority.Commit(token,3,10).ActualUnits,Is.Zero);Assert.That(settled,Is.Zero);
                Assert.That(target.Target.Used,Is.EqualTo(3));Assert.That(target.Target.Remaining,Is.EqualTo(17));
            }
        }
        [Test] public void GlobalBalance_RestoresAlreadySettledUnitsAndOnlyAddsRemainingUnits()
        {
            using(var source=new Fixture())using(var target=new Fixture(false))
            {
                var token=source.Reserve(source.Mint(10),source.Warehouse,8);source.Authority.Commit(token,1,3);target.Restore(source.Capture());
                Assert.That(target.Authority.Balance(ResourceItemCatalog.Ore),Is.EqualTo(3));
                Assert.That(target.Authority.Commit(token,1,3).ActualUnits,Is.EqualTo(3));Assert.That(target.Authority.Balance(ResourceItemCatalog.Ore),Is.EqualTo(3));
                target.Authority.Commit(token,2,99);Assert.That(target.Authority.Balance(ResourceItemCatalog.Ore),Is.EqualTo(8));Assert.That(target.Warehouse.Used,Is.Zero);
            }
        }
        [Test] public void CorruptDestinationReservation_IsRejectedBeforePublishingAnyIdentity()
        {
            using(var source=new Fixture())using(var target=new Fixture(false))
            {
                source.Reserve(source.Mint(10),source.Target,8);var state=source.Capture();
                foreach(var row in state.Containers)if(row.Kind==CargoContainerKind.MachineCargo)row.ReservedCapacity--;
                Assert.That(target.Authority.TryRestorePersistent(state,out var reason),Is.False);Assert.That(reason,Does.Contain("reservation"));
                Assert.That(target.Authority.TryCapturePersistent(out var empty),Is.True);Assert.That(empty.Lots,Is.Empty);Assert.That(empty.Containers,Is.Empty);
                Assert.That(target.Registry.TryGetKind(new PersistentId(state.Lots[0].Id),out _),Is.False);
            }
        }
        [Test] public void ChangedSourceVersion_IsRejectedWhileOriginalReservationRemainsPending()
        {
            using(var source=new Fixture())using(var target=new Fixture(false))
            {
                source.Reserve(source.Mint(10),source.Target,8);var state=source.Capture();state.Lots[0].Version++;
                Assert.That(target.Authority.TryRestorePersistent(state,out _),Is.False);Assert.That(source.Source.Remaining,Is.EqualTo(90));
            }
        }
        [Test] public void LostTerminalFlag_IsRejectedInsteadOfRepublishingAnAlreadySettledFact()
        {
            using(var source=new Fixture())using(var target=new Fixture(false))
            {
                var token=source.Reserve(source.Mint(10),source.Target,8);source.Authority.Cancel(token,1);var state=source.Capture();state.Transactions[0].TerminalReported=false;
                Assert.That(target.Authority.TryRestorePersistent(state,out var reason),Is.False);Assert.That(reason,Does.Contain("acknowledgement"));
            }
        }
        [Test] public void LotSelectionOrderAndSavedRevisions_SurviveJsonRestoration()
        {
            using(var source=new Fixture())using(var target=new Fixture(false))
            {
                var first=source.Mint(4);source.Mint(6);var saved=source.Capture();target.Restore(saved);
                Assert.That(target.Authority.TryFindAvailableLot(target.Source.Owner,ResourceItemCatalog.Ore,out var lot),Is.True);Assert.That(lot.Id,Is.EqualTo(first.Id));
                Assert.That(target.Source.Revision,Is.EqualTo(source.Source.Revision));Assert.That(target.Authority.Revision,Is.EqualTo(source.Authority.Revision));
                var again=target.Capture();Assert.That(again.Containers[0].LotOrder,Is.EqualTo(saved.Containers[0].LotOrder));
            }
        }
    }
}
