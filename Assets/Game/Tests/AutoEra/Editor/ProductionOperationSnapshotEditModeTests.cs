using System;
using System.Collections.Generic;
using AutoEra.Buildings;
using AutoEra.Logistics;
using AutoEra.Machines;
using AutoEra.Machines.Sensors;
using AutoEra.ResourcePoints;
using AutoEra.Save;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    // Real production authorities; contact is controlled here. These tests isolate rebinding, not whole-world restoration.
    public sealed class ProductionOperationSnapshotEditModeTests
    {
        private sealed class Contact : IProductionWorkContact
        {
            public bool TryContact(ComponentInstance component,PersistentId point,Rect area,PersistentId tree,Vector3 target,double height,out string reason)
            { reason=null;return true; }
            public void SetWorking(ComponentInstance component,bool working,double elapsed) { }
            public void SafeStop(ComponentInstance component) { }
        }
        private sealed class Fixture : IDisposable
        {
            internal readonly AutoEraWorldSession World=new AutoEraWorldSessionFactory().Create(0);
            internal readonly InitialRegion Region;
            internal readonly MachineExecutionContext Context;
            internal readonly ComponentInstance Tool;
            internal readonly RegionProductionFacility Facility;
            internal readonly EffectorBehaviorQueue<EffectorBehaviorParameters> Queue;
            internal readonly ProductionEffectorExecutor Executor;
            internal readonly ForestMineralProductionConfig Config;
            internal readonly GameObject Root;
            internal readonly List<IRegionEffectorOperation> Operations=new List<IRegionEffectorOperation>();
            internal Fixture(bool cut)
            {
                World.Resources.Configure(new ResourceItemCatalog(new[] {
                    new ResourceItemDefinition(ResourceItemCatalog.Ore,CargoItemClass.CommonResource),
                    new ResourceItemDefinition(ResourceItemCatalog.Wood,CargoItemClass.CommonResource) }));
                Region=new InitialRegion(World,new Rect(-50,-50,100,100));
                Root=new GameObject("Production checkpoint fixture"); var view=Root.AddComponent<RegionObjectView>();
                var authoring=new SerializedObject(view);
                authoring.FindProperty("_displayName").stringValue="Resource";
                authoring.FindProperty("_kind").enumValueIndex=(int)PersistentObjectKind.ResourcePoint;
                authoring.FindProperty("_footprint").vector2Value=new Vector2(20,20);
                authoring.FindProperty("_blocksNavigation").boolValue=false;
                var channels=authoring.FindProperty("_workChannels");channels.arraySize=1;channels.GetArrayElementAtIndex(0).stringValue="作业";
                authoring.ApplyModifiedPropertiesWithoutUndo();view.Initialize(Region);
                Config=ScriptableObject.CreateInstance<ForestMineralProductionConfig>();
                var rules=Config.Read();var trees=new Transform[cut ? rules.TreeCount(0) : 0];
                for(int i=0;i<trees.Length;i++)
                {
                    var tree=new GameObject("Tree "+i);tree.transform.SetParent(Root.transform);tree.transform.position=new Vector3(i%4*2,0,i/4*2);
                    tree.AddComponent<ProductionTreePresentation>();trees[i]=tree.transform;
                }
                var rocks=new Transform[cut ? 0 : 6];
                for(int i=0;i<rocks.Length;i++) { rocks[i]=new GameObject("Rock "+i).transform;rocks[i].SetParent(Root.transform); }
                Facility=Root.AddComponent<RegionProductionFacility>();Facility.ConfigureForEditor(cut ? ProductionFacilityKind.Forest : ProductionFacilityKind.Mineral,0,Config,trees,rocks,Root.transform);
                Facility.Initialize(World,Region,view.Model);
                var machine=World.Machines.Create(new MachineDefinition(1,"Fixture",1,0,1,1,30,true,true,100));
                Tool=World.Machines.CreateComponent(new ComponentDefinition(cut ? 2203 : 2204,HardwareKind.Effector,1,0,0,0,true));
                World.Machines.Install(machine.Id,ManagementOrigin.Library,Tool.Id,0);
                Region.DeployMachine(machine.Id,new Vector2(-15,0),Vector2.one,out _);Region.TryUpdateMachinePose(machine.Id,Vector2.zero,0);
                machine.Activate(ManagementOrigin.Field);machine.UpdateEnvironment(true,true);machine.SetRunState(ManagementOrigin.Field,MachineRunState.Running);
                Context=new MachineExecutionContext(machine,World.IdAllocator,World.Events,World.Resources.GetCargo(machine));
                Queue=Context.BindEffector<EffectorBehaviorParameters>(Tool);
                Executor=new ProductionEffectorExecutor(Region,new Dictionary<PersistentId,RegionProductionFacility>{{view.Model.Id,Facility}},new Contact());
            }
            internal IRegionEffectorOperation Start(bool cut)
            {
                Context.Tasks.Submit("production",WorkPriority.Normal,out var task);Context.Tasks.TryStart(task.Id);
                var parameters=new EffectorBehaviorParameters(cut ? "Cut" : "Drill");parameters.Numbers["ratio"]=.25;parameters.Numbers["count"]=3;parameters.Numbers["power"]=1;
                if(cut) parameters.Objects["tree"]=new PersistentObjectReference(Facility.Forest.ReadAt(0).Id,PersistentObjectKind.Tree);
                Assert.That(Queue.Submit(task.Id,default,default,Facility.Target,WorkPriority.Normal,InterruptionRule.Immediate,parameters,out var request),Is.EqualTo(QueueAdmission.Accepted));
                Assert.That(Executor.TryStart(Context,Tool,request,0,out var operation,out var reason),Is.True,reason);Operations.Add(operation);return operation;
            }
            internal IRegionEffectorOperation Restore(RegionEffectorOperationSnapshot snapshot,long now)
            {
                Assert.That(Executor.TryRestorePersistent(Context,Tool,Queue.Current,snapshot,now,out var operation,out var reason),Is.True,reason);
                Operations.Add(operation);return operation;
            }
            public void Dispose()
            {
                foreach(var operation in Operations) operation.Dispose();
                Queue.InvalidateUnstarted();if(Queue.Current!=null)Queue.Finish(BehaviorOutcome.Cancelled);Context.Dispose();
                Facility.Release();UnityEngine.Object.DestroyImmediate(Root);UnityEngine.Object.DestroyImmediate(Config);Region.Dispose();World.Dispose();
            }
        }
        private static RegionEffectorOperationSnapshot Json(IRegionEffectorOperation operation,long now)
        {
            Assert.That(((IRegionPersistentEffectorOperation)operation).TryCapturePersistent(now,out var snapshot),Is.True);
            string content=WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(now,1000,1,"Fixture",new[] {new WorldSnapshotSection("operation",1,snapshot)}));
            Assert.That(WorldSnapshotCodec.TryRead(content,new Dictionary<string,int>{{"operation",1}},out var file,out var reason),Is.True,reason);
            Assert.That(file.TryReadSection<RegionEffectorOperationSnapshot>("operation",out var result,out reason),Is.True,reason);return result;
        }
        [Test] public void Drill_RebindsOriginalSequenceAndFractionWithoutRepeatingProduction()
        {
            using(var f=new Fixture(false))
            {
                var original=f.Start(false);original.Advance(6000);
                Assert.That(f.Facility.Mineral.CachedUnits,Is.EqualTo(1));Assert.That(f.Facility.Mineral.FractionalContribution,Is.EqualTo(.5).Within(1e-9));
                var snapshot=Json(original,6000);long revision=f.World.Resources.Authority.Revision;
                var restored=f.Restore(snapshot,6000);Assert.That(f.World.Resources.Authority.Revision,Is.EqualTo(revision));
                restored.Advance(8000);Assert.That(f.Facility.Mineral.CachedUnits,Is.EqualTo(2));
                var next=Json(restored,8000);Assert.That(next.Production.Sequence,Is.EqualTo(snapshot.Production.Sequence+1));Assert.That(next.Production.Produced,Is.EqualTo(2));
            }
        }
        [Test] public void Cut_PreservesOriginalTreeDamageAndPausedElapsed()
        {
            using(var f=new Fixture(true))
            {
                var original=f.Start(true);original.Advance(1000);original.SetPaused(true,1000);
                var tree=f.Facility.Forest.ReadAt(0);Assert.That(tree.HP,Is.EqualTo(2.75).Within(1e-9));
                var restored=f.Restore(Json(original,1000),1000);restored.Advance(10000);Assert.That(f.Facility.Forest.ReadAt(0).HP,Is.EqualTo(tree.HP));
                restored.SetPaused(false,10000);restored.Advance(11000);
                Assert.That(f.Facility.Forest.ReadAt(0).Id,Is.EqualTo(tree.Id));Assert.That(f.Facility.Forest.ReadAt(0).HP,Is.EqualTo(2.5).Within(1e-9));
                Assert.That(Json(restored,11000).Production.Elapsed,Is.EqualTo(2));
            }
        }
        [TestCase("sequence")] [TestCase("produced")] [TestCase("time")]
        public void DamagedDrillCheckpoint_DoesNotAlterOriginalAuthority(string damage)
        {
            using(var f=new Fixture(false))
            {
                var original=f.Start(false);original.Advance(6000);var snapshot=Json(original,6000);
                if(damage=="sequence")snapshot.Production.Sequence++;
                if(damage=="produced")snapshot.Production.Produced=0;
                if(damage=="time")snapshot.Production.LastWorldMilliseconds=7000;
                long revision=f.World.Resources.Authority.Revision;
                Assert.That(f.Executor.TryRestorePersistent(f.Context,f.Tool,f.Queue.Current,snapshot,6000,out var operation,out _),Is.False);
                Assert.That(operation,Is.Null);Assert.That(f.World.Resources.Authority.Revision,Is.EqualTo(revision));
                original.Advance(8000);Assert.That(f.Facility.Mineral.CachedUnits,Is.EqualTo(2));
            }
        }
        [Test] public void ActiveDrillResponsibility_RestoresOriginalMachineTaskBehaviorAndCorrelation_WithoutReissuingTask()
        {
            using(var source=new Fixture(false))
            {
                var operation=source.Start(false);operation.Advance(6000);source.World.Clock.TryAdvanceTo(6000);source.World.Production.Advance(6000);
                Assert.That(source.World.Production.TryCapturePersistent(out var production),Is.True);Assert.That(production.Responsibilities.Length,Is.EqualTo(1));
                Assert.That(source.World.Resources.TryCapturePersistent(out var resources),Is.True);var roster=source.World.Machines.CapturePersistentConfiguration();
                using(var target=new AutoEraWorldSessionFactory().CreateRestoreCandidate(6000,roster.AllocatedThrough))
                {
                    target.Events.Restore(source.World.Events.Capture());target.Machines.RestoreConfiguration(roster,
                        (id,level)=>source.Context.Machine.Definition,(id,level)=>source.Tool.Definition);
                    target.Resources.Configure(new ResourceItemCatalog(new[] {new ResourceItemDefinition(ResourceItemCatalog.Wood,CargoItemClass.CommonResource),
                        new ResourceItemDefinition(ResourceItemCatalog.Ore,CargoItemClass.CommonResource)}));
                    Assert.That(target.Resources.TryRestorePersistent(resources,out var reason),Is.True,reason);
                    ulong next=target.IdAllocator.NextId.Value;ulong sequence=target.Events.Capture().DispatchSequence;
                    Assert.That(target.Production.TryRestorePersistent(production,out reason),Is.True,reason);
                    Assert.That(target.Production.TryCapturePersistent(out var restored),Is.True);
                    var before=production.Responsibilities[0];var after=restored.Responsibilities[0];
                    Assert.That(after.Producer,Is.EqualTo(before.Producer));Assert.That(after.Machine,Is.EqualTo(before.Machine));Assert.That(after.Component,Is.EqualTo(before.Component));
                    Assert.That(after.Task,Is.EqualTo(before.Task));Assert.That(after.Behavior,Is.EqualTo(before.Behavior));Assert.That(after.Correlation,Is.EqualTo(before.Correlation));
                    Assert.That(target.IdAllocator.NextId.Value,Is.EqualTo(next));Assert.That(target.Events.Capture().DispatchSequence,Is.EqualTo(sequence));
                    target.Production.TryGetMineral(source.Facility.Target.Id,out var mine);Assert.That(mine.CachedUnits,Is.EqualTo(1));
                }
            }
        }
        [Test] public void ProductionPublicCache_RestoresOriginalVersionAndTreeReadout_WithoutDomainChanges()
        {
            using(var source=new Fixture(true))using(var target=new Fixture(true))
            {
                Assert.That(source.Facility.TryRead(SensorKind.ObjectState,out var cached),Is.True);var saved=source.Facility.CapturePublicPersistent();
                long revision=target.World.Resources.Authority.Revision;Assert.That(target.Facility.RestorePublicPersistent(saved),Is.True);
                Assert.That(target.Facility.TryRead(SensorKind.ObjectState,out var restored),Is.True);Assert.That(restored.Version,Is.EqualTo(cached.Version));
                Assert.That(restored.Trees[0].Id,Is.EqualTo(cached.Trees[0].Id));Assert.That(restored.Trees[0].Height,Is.EqualTo(cached.Trees[0].Height));
                Assert.That(target.World.Resources.Authority.Revision,Is.EqualTo(revision));Assert.That(target.Facility.CapturePublicPersistent().NavigationRevision,Is.EqualTo(saved.NavigationRevision));
            }
        }
        [Test] public void ProductionPublicCache_WrongVersionIsRejectedBeforeInstallingIt()
        {
            using(var source=new Fixture(false))using(var target=new Fixture(false))
            {
                source.Facility.TryRead(SensorKind.ObjectState,out _);var saved=source.Facility.CapturePublicPersistent();saved.Version++;
                Assert.That(target.Facility.RestorePublicPersistent(saved),Is.False);Assert.That(target.Facility.CapturePublicPersistent().Version,Is.Zero);
                Assert.That(target.Facility.CapturePublicPersistent().Cached,Is.Null);
            }
        }
        [Test] public void PersistentProductionBinding_RejectsChangedRulesBeforeAttachingAView()
        {
            using(var f=new Fixture(false))
            {
                var root=new GameObject("Changed production view");var changed=ScriptableObject.CreateInstance<ForestMineralProductionConfig>();
                try
                {
                    var serialized=new SerializedObject(changed);var damage=serialized.FindProperty("_miningResistance");Assert.That(damage,Is.Not.Null);damage.doubleValue=21;serialized.ApplyModifiedPropertiesWithoutUndo();
                    var view=root.AddComponent<RegionProductionFacility>();view.ConfigureForEditor(ProductionFacilityKind.Mineral,0,changed,Array.Empty<Transform>(),Array.Empty<Transform>(),root.transform);
                    f.Region.TryGet(f.Facility.Target.Id,out var model);long revision=f.World.Resources.Authority.Revision;
                    Assert.That(()=>view.InitializePersistent(f.World,f.Region,model),Throws.InvalidOperationException);Assert.That(view.IsAvailable,Is.False);Assert.That(f.World.Resources.Authority.Revision,Is.EqualTo(revision));
                }
                finally {UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(changed);}
            }
        }
    }
}
