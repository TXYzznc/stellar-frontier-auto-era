using System;
using System.Collections.Generic;
using AutoEra.Buildings;
using AutoEra.Logistics;
using AutoEra.ResourcePoints;
using AutoEra.Save;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    // Missing economy/progress modules are represented solely to exercise their required contract gates.
    // These fixtures are not product economy, rewards, startup content or G7 evidence.
    public sealed class GameplayDomainSnapshotEditModeTests
    {
        private sealed class Marker {public bool Present;}
        private sealed class ContractModule : IWorldDomainPersistence
        {
            private readonly string _name;internal bool Reject;
            internal ContractModule(string name) {_name=name;Versions=new Dictionary<string,int>{{name,1}};}
            public IReadOnlyDictionary<string,int> Versions {get;}
            public bool TryCapture(AutoEraWorldSession world,out WorldSnapshotSection[] sections,out ulong[] identities,out string reason)
            {sections=new[] {new WorldSnapshotSection(_name,1,new Marker {Present=true})};identities=Array.Empty<ulong>();reason=null;return true;}
            public bool TryReadIdentities(LoadedWorldSnapshot snapshot,out ulong[] identities,out string reason)
            {identities=Array.Empty<ulong>();return snapshot.TryReadSection<Marker>(_name,out _,out reason);}
            public bool TryRestore(AutoEraWorldSession world,InitialRegion region,LoadedWorldSnapshot snapshot,out string reason)
            {reason=Reject ? "ContractRejected" : null;return !Reject;}
        }
        private sealed class Fixture : IDisposable
        {
            internal readonly AutoEraWorldSession World=new AutoEraWorldSessionFactory().Create(0);
            internal readonly InitialRegion Region;
            internal readonly ForestProduction Forest;
            internal readonly ContractModule Economy=new ContractModule("fixtureEconomy"),Progress=new ContractModule("fixtureProgress");
            internal readonly GameplayWorldDomainPersistence Domains;
            internal Fixture()
            {
                World.Resources.Configure(Catalog());Region=new InitialRegion(World,new Rect(-50,-50,100,100));
                var point=Region.Register(PersistentObjectKind.ResourcePoint,"Forest",Vector2.zero,new Vector2(10,10),blocksNavigation:false);
                var asset=ScriptableObject.CreateInstance<ForestMineralProductionConfig>();var rules=asset.Read();UnityEngine.Object.DestroyImmediate(asset);
                Forest=World.Production.RegisterForest(point.Id,rules,new[] {Vector3.zero});World.Production.BindGroundPosition(point.Id,Vector3.zero);
                Forest.TryDamage(Forest.ReadAt(0).Id,100,1,0,out _,out _);World.Clock.TryAdvanceTo(10000);World.Production.Advance(10000);
                Domains=new GameplayWorldDomainPersistence(Catalog(),Economy,Progress,null,null);
            }
            internal LoadedWorldSnapshot Document(bool omitProgress=false)
            {
                Assert.That(World.Resources.TryCapturePersistent(out var resources),Is.True);Assert.That(World.Production.TryCapturePersistent(out var production),Is.True);
                RegionEnergyPersistenceSnapshot energy;using(var empty=new RegionEnergyService())Assert.That(empty.TryCapturePersistent(10000,out energy),Is.True);
                var sections=new List<WorldSnapshotSection> {new WorldSnapshotSection("resources",1,resources),new WorldSnapshotSection("production",1,new GameplayProductionSnapshot {State=production,PublicViews=Array.Empty<ProductionFacilityPublicSnapshot>()}),
                    new WorldSnapshotSection("energy",1,new GameplayEnergySnapshot {State=energy}),new WorldSnapshotSection("fixtureEconomy",1,new Marker {Present=true})};
                if(!omitProgress)sections.Add(new WorldSnapshotSection("fixtureProgress",1,new Marker {Present=true}));
                string text=WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(10000,World.IdAllocator.NextId.Value-1,1,"Domain contract",sections));
                var expected=new Dictionary<string,int>(Domains.Versions);if(omitProgress)expected.Remove("fixtureProgress");
                Assert.That(WorldSnapshotCodec.TryRead(text,expected,out var file,out var reason),Is.True,reason);return file;
            }
            public void Dispose() {Region.Dispose();World.Dispose();}
        }
        private static ResourceItemCatalog Catalog()=>new ResourceItemCatalog(new[] {new ResourceItemDefinition(ResourceItemCatalog.Wood,CargoItemClass.LocalPhysicalItem),new ResourceItemDefinition(ResourceItemCatalog.Ore,CargoItemClass.CommonResource)});
        [Test] public void DeliveredDomainComposition_RestoresRealProductionReceiptsAndBorrowedRegionIdentity_WithoutAllocation()
        {
            using(var source=new Fixture())
            {
                var document=source.Document();Assert.That(source.Domains.TryReadIdentities(document,out var ids,out var reason),Is.True,reason);
                CollectionAssert.Contains(ids,source.Forest.ReadAt(0).Id.Value);CollectionAssert.DoesNotContain(ids,source.Forest.Id.Value);
                using(var target=new AutoEraWorldSessionFactory().CreateRestoreCandidate(10000,document.AllocatedThrough))using(var region=new InitialRegion(target,source.Region.Bounds))
                {
                    target.Events.Restore(source.World.Events.Capture());region.RestorePersistentState(source.Region.CapturePersistentState());ulong next=target.IdAllocator.NextId.Value;
                    Assert.That(source.Domains.TryRestore(target,region,document,out reason),Is.True,reason);Assert.That(target.Production.TryGetForest(source.Forest.Id,out var forest),Is.True);
                    Assert.That(forest.CachedUnits,Is.EqualTo(source.Forest.CachedUnits));Assert.That(forest.ReadAt(0).Stage,Is.EqualTo(TreeStage.Stump));
                    Assert.That(target.IdAllocator.NextId.Value,Is.EqualTo(next));Assert.That(target.Events.Capture().DispatchSequence,Is.EqualTo(source.World.Events.Capture().DispatchSequence));
                }
            }
        }
        [Test] public void MissingProgressSection_RefusesIdentityValidation_BeforeCandidateCreation()
        {
            using(var source=new Fixture())Assert.That(source.Domains.TryReadIdentities(source.Document(true),out _,out _),Is.False);
        }
        [Test] public void DownstreamDomainRejection_DoesNotChangeOriginalWorld_AndCandidateCanBeDiscarded()
        {
            using(var source=new Fixture())
            {
                source.Progress.Reject=true;var file=source.Document();long revision=source.World.Resources.Authority.Revision;
                using(var target=new AutoEraWorldSessionFactory().CreateRestoreCandidate(10000,file.AllocatedThrough))using(var region=new InitialRegion(target,source.Region.Bounds))
                {
                    target.Events.Restore(source.World.Events.Capture());region.RestorePersistentState(source.Region.CapturePersistentState());
                    Assert.That(source.Domains.TryRestore(target,region,file,out var reason),Is.False);Assert.That(reason,Is.EqualTo("ContractRejected"));
                    Assert.That(source.World.Resources.Authority.Revision,Is.EqualTo(revision));Assert.That(source.Forest.CachedUnits,Is.GreaterThan(0));
                }
            }
        }
    }
}
