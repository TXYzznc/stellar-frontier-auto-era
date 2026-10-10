using System;
using System.Collections.Generic;
using AutoEra.Buildings;
using AutoEra.Logistics;
using AutoEra.ResourcePoints;
using AutoEra.Save;
using AutoEra.World;
using AutoEra.World.Identity;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class ProductionDomainSnapshotEditModeTests
    {
        private sealed class Fixture : IDisposable
        {
            internal readonly PersistentIdAllocator Ids=new PersistentIdAllocator();
            internal readonly PersistentObjectRegistry Registry;
            internal readonly CargoOwnershipAuthority Cargo;
            internal readonly ProductionRules Rules;
            internal ForestProduction Forest;
            internal MineralProduction Mineral;
            internal Fixture(bool populate=true)
            {
                Registry=new PersistentObjectRegistry(Ids);
                Cargo=new CargoOwnershipAuthority(Ids,new ResourceItemCatalog(new[] {new ResourceItemDefinition(ResourceItemCatalog.Wood,CargoItemClass.LocalPhysicalItem),
                    new ResourceItemDefinition(ResourceItemCatalog.Ore,CargoItemClass.CommonResource)}),Registry);
                var asset=ScriptableObject.CreateInstance<ForestMineralProductionConfig>();Rules=asset.Read();UnityEngine.Object.DestroyImmediate(asset);
                if(populate)
                {
                    Forest=new ForestProduction(Next(),Ids,Registry,Rules,Cargo,new[] {Vector3.zero,new Vector3(2,0,0)},0);
                    Mineral=new MineralProduction(Next(),0,1234,Rules,Cargo,0);
                }
            }
            internal PersistentId Next() {Assert.That(Ids.TryAllocate(out var id),Is.True);return id;}
            internal void RestoreCargo(Fixture source)
            {Assert.That(source.Cargo.TryCapturePersistent(out var state),Is.True);Assert.That(Cargo.TryRestorePersistent(Json(state),out var reason),Is.True,reason);}
            public void Dispose() {Forest?.Dispose();Cargo.Dispose();}
        }
        private static T Json<T>(T state) where T:class
        {
            string text=WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(0,ulong.MaxValue,1,"Production",new[] {new WorldSnapshotSection("state",1,state)}));
            Assert.That(WorldSnapshotCodec.TryRead(text,new Dictionary<string,int>{{"state",1}},out var file,out var reason),Is.True,reason);
            Assert.That(file.TryReadSection<T>("state",out var result,out reason),Is.True,reason);return result;
        }
        [Test] public void FallingTree_RestoresOriginalIdentityDirectionAndSequence_WithoutAllocatingOrSettling()
        {
            using(var source=new Fixture())using(var target=new Fixture(false))
            {
                var tree=source.Forest.ReadAt(0);Assert.That(source.Forest.TryDamage(tree.Id,100,1,0,out bool fell,out var reason),Is.True,reason);Assert.That(fell,Is.True);
                var data=Json(source.Forest.CapturePersistent());target.RestoreCargo(source);ulong next=target.Ids.NextId.Value;
                target.Forest=ForestProduction.RestorePersistent(data,target.Registry,target.Cargo,0);
                Assert.That(target.Ids.NextId.Value,Is.EqualTo(next));Assert.That(target.Forest.ReadAt(0).Id,Is.EqualTo(tree.Id));
                Assert.That(target.Forest.ReadAt(0).Stage,Is.EqualTo(TreeStage.Falling));Assert.That(target.Forest.CachedUnits,Is.Zero);
                Assert.That(target.Forest.CapturePersistent().Trees[0].FallDirection,Is.EqualTo(data.Trees[0].FallDirection));
                Assert.That(target.Forest.TrySettle(tree.Id,data.Trees[0].FellingSequence,10000,out reason),Is.True,reason);
                long units=target.Forest.CachedUnits;Assert.That(units,Is.GreaterThan(0));
                Assert.That(target.Forest.TrySettle(tree.Id,data.Trees[0].FellingSequence,10000,out reason),Is.True,reason);Assert.That(target.Forest.CachedUnits,Is.EqualTo(units));
                Assert.That(source.Forest.CachedUnits,Is.Zero);
            }
        }
        [Test] public void SettledStump_RegrowsAtOriginalDeadline_AndKeepsOriginalDamageRetry()
        {
            using(var source=new Fixture())using(var target=new Fixture(false))
            {
                var tree=source.Forest.ReadAt(0);source.Forest.TryDamage(tree.Id,100,1,0,out _,out _);source.Forest.TrySettle(tree.Id,1,0,out _);
                var data=Json(source.Forest.CapturePersistent());target.RestoreCargo(source);target.Forest=ForestProduction.RestorePersistent(data,target.Registry,target.Cargo,0);
                Assert.That(target.Forest.TryDamage(tree.Id,100,1,0,out var retry,out _),Is.True);Assert.That(retry,Is.True);
                target.Forest.Advance(data.Trees[0].RecoverAt-1);Assert.That(target.Forest.ReadAt(0).Stage,Is.EqualTo(TreeStage.Stump));
                source.Forest.Advance(data.Trees[0].RecoverAt+1000);target.Forest.Advance(data.Trees[0].RecoverAt+1000);
                Assert.That(target.Forest.ReadAt(0).Height,Is.EqualTo(source.Forest.ReadAt(0).Height));Assert.That(target.Forest.ReadAt(0).HP,Is.EqualTo(source.Forest.ReadAt(0).HP));
                Assert.That(target.Forest.CachedUnits,Is.EqualTo(source.Forest.CachedUnits));
            }
        }
        [Test] public void RemovedTree_KeepsItsOriginalSlotAndNeverRegistersALiveReplacement()
        {
            using(var source=new Fixture())using(var target=new Fixture(false))
            {
                var tree=source.Forest.ReadAt(0);Assert.That(source.Forest.RemoveTree(tree.Id),Is.True);target.RestoreCargo(source);
                target.Forest=ForestProduction.RestorePersistent(Json(source.Forest.CapturePersistent()),target.Registry,target.Cargo,0);
                Assert.That(target.Forest.Count,Is.EqualTo(2));Assert.That(target.Forest.ReadAt(0).Id,Is.EqualTo(tree.Id));Assert.That(target.Forest.TryRead(tree.Id,out _),Is.False);
                Assert.That(target.Registry.TryGetKind(tree.Id,out _),Is.False);
            }
        }
        [Test] public void MineralFractionAndZeroUnitRetry_RestoreBeforeNextWholeUnit_KeepOriginalRockOrder()
        {
            using(var source=new Fixture())using(var target=new Fixture(false))
            {
                var behavior=source.Next();Assert.That(source.Mineral.TryDrill(behavior,1,1,0,1000,out var first,out var reason),Is.True,reason);Assert.That(first.Units,Is.Zero);
                var data=Json(source.Mineral.CapturePersistent());target.RestoreCargo(source);target.Mineral=MineralProduction.RestorePersistent(data,target.Cargo,1000);
                Assert.That(target.Mineral.TryDrill(behavior,1,99,1,1000,out var repeated,out reason),Is.True,reason);Assert.That(repeated.Units,Is.Zero);
                Assert.That(target.Mineral.FractionalContribution,Is.EqualTo(source.Mineral.FractionalContribution));
                for(int i=0;i<data.FullRockCount;i++)Assert.That(target.Mineral.IsRockVisible(i),Is.EqualTo(source.Mineral.IsRockVisible(i)));
                source.Mineral.TryDrill(behavior,2,4,1,5000,out _,out _);target.Mineral.TryDrill(behavior,2,4,1,5000,out _,out _);
                Assert.That(target.Mineral.ProducedUnits,Is.EqualTo(source.Mineral.ProducedUnits));Assert.That(target.Mineral.CachedUnits,Is.EqualTo(source.Mineral.CachedUnits));
            }
        }
        [Test] public void DepletedDeposit_RestoresOriginalCleanupTimeAndNeverGeneratesAnotherUnit()
        {
            using(var source=new Fixture())using(var target=new Fixture(false))
            {
                var behavior=source.Next();Assert.That(source.Mineral.TryDrill(behavior,1,1000,1,1000,out _,out var reason),Is.True,reason);
                var data=Json(source.Mineral.CapturePersistent());target.RestoreCargo(source);target.Mineral=MineralProduction.RestorePersistent(data,target.Cargo,1000);
                Assert.That(target.Mineral.RemainingExact,Is.Zero);Assert.That(target.Mineral.TryDrill(behavior,2,1,1,1001,out _,out _),Is.False);
                Assert.That(target.Mineral.TryCleanup(data.CleanupAt-1),Is.False);Assert.That(target.Mineral.TryCleanup(data.CleanupAt),Is.True);
                Assert.That(target.Mineral.CachedUnits,Is.EqualTo(100));
            }
        }
        [Test] public void MissingStumpReceiptAndChangedMineralStock_AreRejectedBeforeReplayingProduction()
        {
            using(var source=new Fixture())using(var target=new Fixture(false))
            {
                var tree=source.Forest.ReadAt(0);source.Forest.TryDamage(tree.Id,100,1,0,out _,out _);var data=Json(source.Forest.CapturePersistent());
                data.Trees[0].Stage=TreeStage.Stump;data.Trees[0].Height=data.Trees[0].HP=data.Trees[0].FractureHeight;
                target.RestoreCargo(source);Assert.That(()=>ForestProduction.RestorePersistent(data,target.Registry,target.Cargo,0),Throws.ArgumentException);
                var mineral=Json(source.Mineral.CapturePersistent());mineral.Produced=1;
                Assert.That(()=>MineralProduction.RestorePersistent(mineral,target.Cargo,0),Throws.ArgumentException);Assert.That(source.Forest.CachedUnits,Is.Zero);
            }
        }
        private static ResourceItemCatalog Catalog() => new ResourceItemCatalog(new[] {
            new ResourceItemDefinition(ResourceItemCatalog.Wood,CargoItemClass.LocalPhysicalItem),
            new ResourceItemDefinition(ResourceItemCatalog.Ore,CargoItemClass.CommonResource)});
        private static PersistentId Next(AutoEraWorldSession world)
        { Assert.That(world.IdAllocator.TryAllocate(out var id),Is.True);return id; }
        private static ProductionRules Rules()
        { var asset=ScriptableObject.CreateInstance<ForestMineralProductionConfig>();try {return asset.Read();}finally {UnityEngine.Object.DestroyImmediate(asset);} }
        private static AutoEraWorldSession CloneResources(AutoEraWorldSession source)
        {
            Assert.That(source.Resources.TryCapturePersistent(out var saved),Is.True);
            var target=new AutoEraWorldSessionFactory().CreateRestoreCandidate(source.Clock.WorldMilliseconds,saved.Cargo.AllocatedThrough);
            target.Events.Restore(source.Events.Capture());target.Resources.Configure(Catalog());
            Assert.That(target.Resources.TryRestorePersistent(Json(saved),out var reason),Is.True,reason);return target;
        }
        [Test] public void ProductionWorld_RestoresOriginalOrderGroundLocationsAndGrowth_WithoutEventsOrAllocation()
        {
            using(var source=new AutoEraWorldSessionFactory().Create(0))
            {
                source.Resources.Configure(Catalog());var forestId=Next(source);var mineId=Next(source);var rules=Rules();
                var forest=source.Production.RegisterForest(forestId,rules,new[] {new Vector3(2,0,3),new Vector3(4,0,3)});
                var mine=source.Production.RegisterMineral(mineId,0,987,rules);
                source.Production.BindGroundPosition(forestId,new Vector3(1,0,3));source.Production.BindGroundPosition(mineId,new Vector3(-2,0,0));
                source.Clock.TryAdvanceTo(1000);source.Production.Advance(1000);
                Assert.That(source.Production.TryCapturePersistent(out var saved),Is.True);
                using(var target=CloneResources(source))
                {
                    ulong next=target.IdAllocator.NextId.Value;var events=target.Events.Capture();
                    Assert.That(target.Production.TryRestorePersistent(Json(saved),out var reason),Is.True,reason);
                    Assert.That(target.IdAllocator.NextId.Value,Is.EqualTo(next));Assert.That(target.Events.Capture().DispatchSequence,Is.EqualTo(events.DispatchSequence));
                    Assert.That(target.Production.TryGetForest(forestId,out var restored),Is.True);
                    Assert.That(restored.ReadAt(1).Id,Is.EqualTo(forest.ReadAt(1).Id));
                    Assert.That(target.Production.TryGetGroundPosition(mineId,out var position),Is.True);Assert.That(position,Is.EqualTo(new Vector3(-2,0,0)));
                    Assert.That(target.Production.TryGetMineral(mineId,out var restoredMine),Is.True);
                    CollectionAssert.AreEqual(mine.CapturePersistent().RockOrder,restoredMine.CapturePersistent().RockOrder);
                    source.Clock.TryAdvanceTo(100000);target.Clock.TryAdvanceTo(100000);source.Production.Advance(100000);target.Production.Advance(100000);
                    Assert.That(restored.ReadAt(1).Height,Is.EqualTo(forest.ReadAt(1).Height));Assert.That(restored.Revision,Is.EqualTo(forest.Revision));
                }
            }
        }
        [Test] public void ProductionCapture_RejectsHalfStepAndSettlementCallback_UntilEntireAdvanceCompletes()
        {
            using(var world=new AutoEraWorldSessionFactory().Create(0))
            {
                world.Resources.Configure(Catalog());var forest=world.Production.RegisterForest(Next(world),Rules(),new[] {Vector3.zero});
                forest.TryDamage(forest.ReadAt(0).Id,100,1,0,out _,out _);world.Clock.TryAdvanceTo(10000);
                Assert.That(world.Production.TryCapturePersistent(out _),Is.False);
                bool callback=false;world.Resources.Authority.Produced+=receipt=> {callback=true;Assert.That(world.Production.TryCapturePersistent(out _),Is.False);};
                world.Production.Advance(10000);Assert.That(callback,Is.True);Assert.That(world.Production.TryCapturePersistent(out _),Is.True);
            }
        }
        [Test] public void ProductionRestore_InvalidSecondForest_DiscardsStagedTreesAndKeepsSourceUntouched()
        {
            using(var source=new AutoEraWorldSessionFactory().Create(0))
            {
                source.Resources.Configure(Catalog());var first=source.Production.RegisterForest(Next(source),Rules(),new[] {Vector3.zero});
                source.Production.RegisterForest(Next(source),Rules(),new[] {Vector3.right*2});
                Assert.That(source.Production.TryCapturePersistent(out var saved),Is.True);var data=Json(saved);data.Forests[1].WoodRemainder=2;
                using(var target=CloneResources(source))
                {
                    ulong next=target.IdAllocator.NextId.Value;Assert.That(target.Production.TryRestorePersistent(data,out _),Is.False);
                    Assert.That(target.ObjectRegistry.TryGetKind(first.ReadAt(0).Id,out _),Is.False);
                    Assert.That(target.Production.TryGetForest(first.Id,out _),Is.False);Assert.That(target.IdAllocator.NextId.Value,Is.EqualTo(next));
                    Assert.That(source.Production.TryGetForest(first.Id,out var unchanged),Is.True);Assert.That(unchanged,Is.SameAs(first));
                }
            }
        }
        [Test] public void ProductionRestore_RejectsMixedWorldTimestamp_BeforeRegisteringTrees()
        {
            using(var source=new AutoEraWorldSessionFactory().Create(0))
            {
                source.Resources.Configure(Catalog());var forest=source.Production.RegisterForest(Next(source),Rules(),new[] {Vector3.zero});
                source.Clock.TryAdvanceTo(1000);source.Production.Advance(1000);source.Production.TryCapturePersistent(out var saved);
                var data=Json(saved);data.Forests[0].Now=999;
                using(var target=CloneResources(source))
                { Assert.That(target.Production.TryRestorePersistent(data,out _),Is.False);Assert.That(target.ObjectRegistry.TryGetKind(forest.ReadAt(0).Id,out _),Is.False); }
            }
        }
    }
}
