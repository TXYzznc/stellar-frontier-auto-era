using System;
using System.Collections.Generic;
using AutoEra.Energy;
using AutoEra.Machines;
using AutoEra.Save;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class EnergyPersistenceEditModeTests
    {
        private static T Json<T>(T state) where T:class
        {
            string content=WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(0,1000,1,"Energy",new[] {new WorldSnapshotSection("energy",1,state)}));
            Assert.That(WorldSnapshotCodec.TryRead(content,new Dictionary<string,int>{{"energy",1}},out var file,out var reason),Is.True,reason);
            Assert.That(file.TryReadSection<T>("energy",out var result,out reason),Is.True,reason);return result;
        }
        private static EnergyGrid Grid()
        {
            var grid=new EnergyGrid();
            grid.AddGenerator(new EnvironmentGenerator(new PersistentId(1),15) {EnvironmentPower=15});
            grid.AddGenerator(new FuelGenerator(new PersistentId(2),60,20) {AllowsCharging=true,ChargeTargetRatio=.8f});
            grid.AddGenerator(new FuelGenerator(new PersistentId(3),60,30));
            grid.AddStorage(new BatteryStorage(new PersistentId(4),240,1));
            grid.AddConsumer(new EnergyConsumer(new PersistentId(5),PowerPriority.Production,70,70));
            grid.AddConsumer(new EnergyConsumer(new PersistentId(6),PowerPriority.Secondary,20,20));
            return grid;
        }
        private static void Same(EnergyGrid a,EnergyGrid b)
        {
            Assert.That(b.TryCapturePersistent(out var bs),Is.True);Assert.That(a.TryCapturePersistent(out var ass),Is.True);
            string A=WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(0,1000,1,"Energy",new[] {new WorldSnapshotSection("energy",1,ass)}));
            string B=WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(0,1000,1,"Energy",new[] {new WorldSnapshotSection("energy",1,bs)}));Assert.That(B,Is.EqualTo(A));
        }
        [Test] public void Grid_RestoresFuelStorageReadModelAndOrder_ThenContinuesTheSameSettlement()
        {
            var source=Grid();source.Tick(13,true);Assert.That(source.TryCapturePersistent(out var state),Is.True);
            Assert.That(EnergyGrid.TryRestorePersistent(Json(state),null,out var target,out var reason),Is.True,reason);Same(source,target);
            source.Tick(67,false);target.Tick(67,false);Same(source,target);
            source.Tick(11,true);target.Tick(11,true);Same(source,target);
        }
        [Test] public void RemovedConsumerQueueGapAndShortageOrder_SurviveRestoreAndNextRegistration()
        {
            var source=new EnergyGrid();source.AddGenerator(new EnvironmentGenerator(new PersistentId(1),0));
            source.AddConsumer(new EnergyConsumer(new PersistentId(2),PowerPriority.Production,5,5));
            source.AddConsumer(new EnergyConsumer(new PersistentId(3),PowerPriority.Production,5,5));
            source.AddConsumer(new EnergyConsumer(new PersistentId(4),PowerPriority.Production,5,5));source.RemoveConsumer(new PersistentId(3));source.Tick(0,false);
            source.TryCapturePersistent(out var state);Assert.That(EnergyGrid.TryRestorePersistent(Json(state),null,out var target,out var reason),Is.True,reason);
            CollectionAssert.AreEqual(source.Snapshot.StoppedByShortage,target.Snapshot.StoppedByShortage);
            var added=new EnergyConsumer(new PersistentId(5),PowerPriority.Production,5,5);target.AddConsumer(added);Assert.That(added.QueueOrder,Is.EqualTo(3));
            target.Tick(0,false);Assert.That(target.Snapshot.StoppedByShortage[0],Is.EqualTo(added.Id));
        }
        [TestCase("fuel")] [TestCase("capacity")] [TestCase("queue")] [TestCase("stopped")]
        public void InvalidEnergyCheckpoint_IsRejectedWithoutChangingSource(string field)
        {
            var source=Grid();source.Tick(1,true);source.TryCapturePersistent(out var saved);var data=Json(saved);
            if(field=="fuel")data.Generators[1].Fuel=float.NaN;
            if(field=="capacity")data.Storages[0].Charge=data.Storages[0].Capacity+1;
            if(field=="queue")data.Consumers[1].QueueOrder=data.Consumers[0].QueueOrder;
            if(field=="stopped")data.LastSettlement.Stopped=new[] {999UL};
            Assert.That(EnergyGrid.TryRestorePersistent(data,null,out var target,out _),Is.False);Assert.That(target,Is.Null);
            Assert.That(source.Generators[1].FuelEnergyAvailable,Is.EqualTo(saved.Generators[1].Fuel));Assert.That(source.Revision,Is.EqualTo(saved.Revision));
        }
        [Test] public void MachineConsumer_BorrowsOriginalIdentity_WithoutPublishingSupplyOrChangingMachine()
        {
            using(var source=new AutoEraWorldSessionFactory().Create(0))using(var target=new AutoEraWorldSessionFactory().Create(0))
            {
                var definition=new MachineDefinition(1,"Energy",1,0,0,0,30,false,false,100);var machine=source.Machines.Create(definition);source.Machines.Deploy(machine.Id);
                var grid=new EnergyGrid();grid.AddGenerator(new EnvironmentGenerator(new PersistentId(500),15) {EnvironmentPower=15});grid.AddConsumer(new MachineEnergyConsumer(machine,PowerPriority.Critical));grid.Tick(0,true);
                var roster=source.Machines.CaptureConfiguration();target.Machines.RestoreConfiguration(roster,(id,level)=>definition,(id,level)=>null);target.Machines.TryGet(machine.Id,out var restoredMachine);
                Assert.That(restoredMachine.SupplyAvailable,Is.False);grid.TryCapturePersistent(out var state);
                Assert.That(EnergyGrid.TryRestorePersistent(Json(state),id=>target.Machines.TryGet(id,out var m) ? m : null,out var restored,out var reason),Is.True,reason);
                Assert.That(((MachineEnergyConsumer)restored.Consumers[0]).Machine,Is.SameAs(restoredMachine));Assert.That(restoredMachine.SupplyAvailable,Is.False);
                Assert.That(EnergyGrid.TryRestorePersistent(state,id=>null,out _,out _),Is.False);
            }
        }
        private static RegionEnergyFacility Facility(RegionEnergyFacilityKind kind,ulong id,List<GameObject> hosts)
        {
            var root=new GameObject("Energy restore fixture");hosts.Add(root);var view=root.AddComponent<RegionEnergyFacility>();var serialized=new SerializedObject(view);
            serialized.FindProperty("_kind").enumValueIndex=(int)kind;
            if(kind==RegionEnergyFacilityKind.FuelGenerator)serialized.FindProperty("_ratedPower").floatValue=60;
            serialized.ApplyModifiedPropertiesWithoutUndo();view.Initialize(new PersistentId(id));return view;
        }
        [Test] public void Recorder_RestoresPreviousShortageAndFuelMemory_WithoutRepeatingNotices()
        {
            var hosts=new List<GameObject>();
            try
            {
                var fuel=Facility(RegionEnergyFacilityKind.FuelGenerator,101,hosts);fuel.Generator.FuelEnergyAvailable=0;
                var snapshot=new EnergyGridSnapshot(0,0,0,240,0,0,0,new[] {new PersistentId(12)},false);
                var source=new EnergyEventRecorder();var events=new List<EnergyDiscreteEvent>();source.Capture(snapshot,new[] {fuel},events);Assert.That(events.Count,Is.EqualTo(2));
                var target=new EnergyEventRecorder();Assert.That(target.RestorePersistent(Json(source.CapturePersistent())),Is.True);
                events.Clear();target.Capture(snapshot,new[] {fuel},events);Assert.That(events,Is.Empty);
                target.Capture(new EnergyGridSnapshot(1,0,1,240,0,1,0,Array.Empty<PersistentId>(),false),new[] {fuel},events);
                Assert.That(events.Count,Is.EqualTo(2));Assert.That(events[0].Kind,Is.EqualTo(EnergyEventKind.ShortageRecovered));
            }
            finally {foreach(var host in hosts)UnityEngine.Object.DestroyImmediate(host);}
        }
        [Test] public void RegionFacilities_BindOriginalFuelAndBatteryObjects_WithoutResettingTheirCharge()
        {
            var hosts=new List<GameObject>();
            try
            {
                using(var world=new AutoEraWorldSessionFactory().Create(0))using(var source=new RegionEnergyService())
                {
                    var fuel=Facility(RegionEnergyFacilityKind.FuelGenerator,101,hosts);var battery=Facility(RegionEnergyFacilityKind.Battery,102,hosts);
                    source.Register(fuel);source.Register(battery);source.Reconcile(world.Machines);fuel.Generator.AllowsCharging=true;fuel.Generator.ChargeTargetRatio=.5f;
                    source.Tick(60000,60);Assert.That(source.TryCapturePersistent(60000,out var saved),Is.True);
                    Assert.That(RegionEnergyService.TryRestorePersistent(Json(saved),60000,world.Machines,out var target,out var reason),Is.True,reason);
                    using(target)
                    {
                        var secondFuel=Facility(RegionEnergyFacilityKind.FuelGenerator,101,hosts);var secondBattery=Facility(RegionEnergyFacilityKind.Battery,102,hosts);
                        Assert.That(target.TryBindPersistentFacilities(new[] {secondBattery,secondFuel},out reason),Is.True,reason);
                        Assert.That(secondFuel.Generator,Is.SameAs(target.Grid.Generators[0]));Assert.That(secondBattery.Storage,Is.SameAs(target.Grid.Storages[0]));
                        Assert.That(secondFuel.RemainingBiomass,Is.EqualTo(fuel.RemainingBiomass));Assert.That(secondBattery.Charge,Is.EqualTo(battery.Charge));
                        Assert.That(target.Facilities[0],Is.SameAs(secondFuel));Same(source.Grid,target.Grid);
                        source.Tick(120000,60);target.Tick(120000,60);Same(source.Grid,target.Grid);
                    }
                }
            }
            finally {foreach(var host in hosts)UnityEngine.Object.DestroyImmediate(host);}
        }
    }
}
