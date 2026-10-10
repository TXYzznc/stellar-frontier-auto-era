using System;
using System.Collections.Generic;
using AutoEra.Machines;
using AutoEra.Save;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class RegionPersistentSnapshotEditModeTests
    {
        private static readonly Rect Bounds = new Rect(-30,-30,60,60);
        private static readonly MachineDefinition Definition = new MachineDefinition(1,"Fixture",1,2,1,1,10,true,true,100);
        private static MachineInstance Machine(AutoEraWorldSession world, InitialRegion region, float x)
        {
            var machine = world.Machines.Create(Definition);
            Assert.That(region.DeployMachine(machine.Id,new Vector2(x,0),Vector2.one,out _),Is.EqualTo(RegionMachineDeploymentResult.Bound));
            return machine;
        }
        private static T Json<T>(string name, T value, ulong through) where T : class
        {
            string content = WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(5000,through,1,"Fixture",new[] {new WorldSnapshotSection(name,1,value)}));
            Assert.That(WorldSnapshotCodec.TryRead(content,new Dictionary<string,int>{{name,1}},out var file,out var reason),Is.True,reason);
            Assert.That(file.TryReadSection<T>(name,out var result,out reason),Is.True,reason);
            return result;
        }

        [Test] public void RestoreCandidate_DoesNotSeedTemplatesOrConsumeIdentity()
        {
            var factory = new AutoEraWorldSessionFactory();
            using (var original = factory.Create(0)) Assert.That(original.AlgorithmTemplates.CapturePersistent(),Is.Not.Empty);
            using (var candidate = factory.CreateRestoreCandidate(5000,100))
            {
                Assert.That(candidate.Clock.WorldMilliseconds,Is.EqualTo(5000));
                Assert.That(candidate.AlgorithmTemplates.CapturePersistent(),Is.Empty);
                Assert.That(candidate.ObjectRegistry.Count,Is.Zero);
                Assert.That(candidate.IdAllocator.NextId.Value,Is.EqualTo(101));
            }
            using (var exhausted = factory.CreateRestoreCandidate(0,ulong.MaxValue)) Assert.That(exhausted.IdAllocator.IsExhausted,Is.True);
        }

        [Test] public void JsonRoundTrip_RestoresOriginalObjectsAndBorrowedMachineInsideWalkablePoint()
        {
            using (var source = new AutoEraWorldSessionFactory().Create(5000))
            using (var region = new InitialRegion(source,Bounds))
            {
                var point = region.Register(PersistentObjectKind.ResourcePoint,"Mine",Vector2.zero,new Vector2(4,4),0,false);
                var facility = region.Register(PersistentObjectKind.Building,"Facility",new Vector2(7,0),Vector2.one);
                Assert.That(region.AttachResourceFacility(facility.Id,point.Id),Is.True);
                point.SetPublicState("生产中",123,false,2,100);
                var machine = Machine(source,region,-7);
                Assert.That(region.TryUpdateMachinePose(machine.Id,Vector2.zero,90),Is.True);
                var snapshot = Json("region",region.CapturePersistentState(),source.IdAllocator.NextId.Value-1);
                var roster = source.Machines.CapturePersistentConfiguration();
                using (var candidate = new AutoEraWorldSessionFactory().CreateRestoreCandidate(5000,snapshot.AllocatedThrough))
                using (var restored = new InitialRegion(candidate,Bounds))
                {
                    candidate.Machines.RestoreConfiguration(roster,(id,level)=>Definition,(id,level)=>null);
                    int changed=0; restored.ObjectsChanged += ()=>changed++;
                    restored.RestorePersistentState(snapshot);
                    Assert.That(changed,Is.Zero); Assert.That(restored.Count,Is.EqualTo(3));
                    Assert.That(restored.TryGet(machine.Id,out var body),Is.True);
                    Assert.That(candidate.Machines.TryGet(machine.Id,out var originalMachine),Is.True);
                    Assert.That(body.Machine,Is.SameAs(originalMachine)); Assert.That(body.Position,Is.EqualTo(Vector2.zero)); Assert.That(body.Yaw,Is.EqualTo(90));
                    Assert.That(restored.TryGet(point.Id,out var samePoint),Is.True);
                    Assert.That(samePoint.PublicResourceAmount,Is.EqualTo(123)); Assert.That(samePoint.PublicCachedAmount,Is.EqualTo(2));
                    Assert.That(restored.TryGet(facility.Id,out var sameFacility),Is.True); Assert.That(sameFacility.AttachedResourcePoint,Is.EqualTo(point.Id));
                    Assert.That(candidate.ObjectRegistry.Count,Is.EqualTo(3));
                    Assert.That(candidate.IdAllocator.NextId.Value,Is.EqualTo(snapshot.AllocatedThrough+1));
                }
            }
        }

        [TestCase("attachment")] [TestCase("duplicate")] [TestCase("bounds")] [TestCase("overlap")]
        public void InvalidRegion_RejectsBeforeRegistrationOrMachineBinding(string damage)
        {
            using (var source = new AutoEraWorldSessionFactory().Create(0))
            using (var region = new InitialRegion(source,Bounds))
            {
                region.Register(PersistentObjectKind.ResourcePoint,"Point",Vector2.zero,Vector2.one);
                region.Register(PersistentObjectKind.Building,"Facility",new Vector2(3,0),Vector2.one);
                var machine = Machine(source,region,-5);
                var roster = source.Machines.CapturePersistentConfiguration(); var snapshot = region.CapturePersistentState();
                if (damage=="attachment") snapshot.Objects[1].AttachedResourcePoint=machine.Id.Value;
                if (damage=="duplicate") snapshot.Objects[1].Id=snapshot.Objects[0].Id;
                if (damage=="bounds") snapshot.Objects[1].Position=new Vector2(100,0);
                if (damage=="overlap") snapshot.Objects[1].Position=snapshot.Objects[0].Position;
                using (var candidate = new AutoEraWorldSessionFactory().CreateRestoreCandidate(0,snapshot.AllocatedThrough))
                using (var restored = new InitialRegion(candidate,Bounds))
                {
                    candidate.Machines.RestoreConfiguration(roster,(id,level)=>Definition,(id,level)=>null);
                    var next = candidate.IdAllocator.NextId;
                    Assert.Throws<ArgumentException>(()=>restored.RestorePersistentState(snapshot));
                    Assert.That(restored.Count,Is.Zero); Assert.That(candidate.ObjectRegistry.Count,Is.EqualTo(1));
                    Assert.That(candidate.IdAllocator.NextId,Is.EqualTo(next));
                    Assert.That(restored.DeployMachine(machine.Id,new Vector2(-5,0),Vector2.one,out _),Is.EqualTo(RegionMachineDeploymentResult.Bound));
                }
            }
        }

        [Test] public void Queue_RestoresOriginalOwnerPriorityAndEqualPriorityFifoWithoutReplay()
        {
            using (var world = new AutoEraWorldSessionFactory().Create(0))
            using (var region = new InitialRegion(world,Bounds))
            {
                var target = region.Register(PersistentObjectKind.ResourcePoint,"Point",Vector2.zero,Vector2.one);
                var a=Machine(world,region,-5); var b=Machine(world,region,-7); var c=Machine(world,region,-9); var d=Machine(world,region,-11);
                var area = new Rect(-1,-1,2,2);
                RegionWorkQueueSnapshot snapshot;
                using (var source = new RegionWorkQueue(region,target.Id,area,"综合"))
                {
                    source.Request(a.Id,Vector2.zero,0); source.Request(b.Id,Vector2.zero,1); source.Request(c.Id,Vector2.zero,2); source.Request(d.Id,Vector2.zero,2);
                    snapshot=Json("queue",source.CapturePersistentState(),world.IdAllocator.NextId.Value-1);
                }
                using (var restored = new RegionWorkQueue(region,target.Id,area,"综合"))
                {
                    int changes=0; restored.Changed += ()=>changes++;
                    restored.RestorePersistentState(snapshot);
                    Assert.That(changes,Is.Zero); Assert.That(restored.Owner,Is.EqualTo(a.Id)); Assert.That(target.WorkSummary,Does.Contain("等待 3"));
                    restored.Release(a.Id); Assert.That(restored.Owner,Is.EqualTo(c.Id));
                    region.Remove(c.Id); Assert.That(restored.Owner,Is.EqualTo(d.Id));
                    restored.Release(d.Id); Assert.That(restored.Owner,Is.EqualTo(b.Id));
                }
            }
        }

        [TestCase("duplicate")] [TestCase("order")] [TestCase("missing")] [TestCase("channel")]
        public void InvalidQueue_RetainsEmptyOwnerAndWaitingList(string damage)
        {
            using (var world = new AutoEraWorldSessionFactory().Create(0))
            using (var region = new InitialRegion(world,Bounds))
            {
                var target=region.Register(PersistentObjectKind.ResourcePoint,"Point",Vector2.zero,Vector2.one);
                var a=Machine(world,region,-5); var b=Machine(world,region,-7); var c=Machine(world,region,-9);
                var area = new Rect(-1,-1,2,2); RegionWorkQueueSnapshot snapshot;
                using (var source = new RegionWorkQueue(region,target.Id,area,"综合"))
                {
                    source.Request(a.Id,Vector2.zero);source.Request(b.Id,Vector2.zero,2);source.Request(c.Id,Vector2.zero,1);
                    snapshot=source.CapturePersistentState();
                }
                if(damage=="duplicate") snapshot.Waiting[1].Machine=snapshot.Owner;
                if(damage=="order") snapshot.Waiting[1].Priority=3;
                if(damage=="missing") snapshot.Waiting[1].Machine=999;
                if(damage=="channel") snapshot.Channel="另一通道";
                using (var restored = new RegionWorkQueue(region,target.Id,area,"综合"))
                {
                    Assert.Throws<ArgumentException>(()=>restored.RestorePersistentState(snapshot));
                    Assert.That(restored.Owner.IsValid,Is.False);Assert.That(restored.WaitingCount,Is.Zero);
                    Assert.That(restored.Request(b.Id,Vector2.zero),Is.EqualTo(WorkRequestResult.Granted));
                }
            }
        }

        [Test] public void LegacyMachineProxy_CannotBeSavedAsRealMachine()
        {
            using (var world = new AutoEraWorldSessionFactory().Create(0))
            using (var region = new InitialRegion(world,Bounds))
            {
                region.Register(PersistentObjectKind.Machine,"Legacy",Vector2.zero,Vector2.one);
                Assert.Throws<InvalidOperationException>(()=>region.CapturePersistentState());
            }
        }
    }
}
