using System;
using AutoEra.Machines;
using AutoEra.Machines.Sensors;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class MachineSensorEditModeTests
    {
        private sealed class Fixture : ISensorAnchor, IDisposable
        {
            public readonly AutoEraWorldSession Session = new AutoEraWorldSessionFactory().Create(0);
            public readonly InitialRegion Region;
            public readonly MachineInstance Machine;
            public readonly MachineExecutionContext Context;
            public readonly RegionObject Target;
            public readonly RegionSensorReadProvider Provider;
            public readonly RegionSensorEnvironment Environment;
            public readonly MachineSensor Sensor;
            public Vector3 Position;
            public bool AnchorValid = true;
            public Fixture(int compute = 20, int level = 1)
            {
                Region = new InitialRegion(Session, new Rect(-50,-50,100,100));
                Machine = Session.Machines.Create(new MachineDefinition(1,"sensor fixture",1,2,1,0,30,true,true,100));
                var core = Session.Machines.CreateComponent(new ComponentDefinition(2,HardwareKind.Core,1,0,compute,40,false));
                var component = Session.Machines.CreateComponent(new ComponentDefinition(3,HardwareKind.Sensor,level,0,0,0,false));
                Session.Machines.Install(Machine.Id,ManagementOrigin.Library,core.Id,0);
                Session.Machines.Install(Machine.Id,ManagementOrigin.Library,component.Id,0);
                Region.DeployMachine(Machine.Id,Vector2.zero,Vector2.one,out _);
                Machine.Activate(ManagementOrigin.Field); Machine.UpdateEnvironment(true,true); Machine.SetRunState(ManagementOrigin.Field,MachineRunState.Running);
                Context = new MachineExecutionContext(Machine,Session.IdAllocator);
                Target = Region.Register(PersistentObjectKind.Building,"public",new Vector2(10,0),new Vector2(10,10));
                Provider = new RegionSensorReadProvider(Target,p => new Vector3(Mathf.Clamp(p.x,5,15),0,Mathf.Clamp(p.z,-5,5)));
                Environment = new RegionSensorEnvironment(Region); Environment.Register(Provider);
                Sensor = Context.Sensors.Bind(component,new SensorProfile(3,level,SensorKind.ObjectState),Environment,this);
                Sensor.Bind(Provider.Target);
            }
            public bool TryGetPosition(out Vector3 position) { position=Position; return AnchorValid; }
            public void Dispose() { Context.Dispose(); Provider.Dispose(); Environment.Dispose(); Region.Dispose(); Session.Dispose(); }
        }
        [Test] public void FixedLevelsAndClosestRegion_AreNotCentreDistance()
        {
            for(int level=1;level<=2;level++) using(var f=new Fixture(20,level))
            { f.Sensor.Tick(0); Assert.That(f.Sensor.TryRead(out _),Is.True); Assert.That(f.Context.Compute.Used,Is.EqualTo(10)); Assert.That(f.Sensor.Profile.IntervalMilliseconds,Is.EqualTo(1000)); }
        }
        [Test] public void ConstantSamples_KeepLease_AndDoNotCatchUp()
        {
            using(var f=new Fixture()) { int samples=0,changed=0; f.Sensor.Sampled+=_=>samples++; f.Sensor.Changed+=_=>changed++;
                f.Sensor.Tick(0); f.Sensor.Tick(999); f.Sensor.Tick(1000); f.Sensor.Tick(100000);
                Assert.That(samples,Is.EqualTo(3)); Assert.That(changed,Is.EqualTo(1)); Assert.That(f.Context.Compute.Used,Is.EqualTo(10)); }
        }
        [Test] public void RepeatedPublicStateNotification_DoesNotInventValueChanges()
        {
            using (var f = new Fixture())
            {
                int changed = 0;
                f.Sensor.Changed += e => { if (e.Snapshot != null) changed++; };
                f.Sensor.Tick(0);
                var first = f.Sensor.LastSample;
                f.Target.SetPublicState(f.Target.PublicStatus);
                f.Sensor.Tick(1000);
                Assert.That(f.Sensor.LastSample, Is.SameAs(first));
                Assert.That(changed, Is.EqualTo(1));
                f.Target.SetPublicState("changed",12);
                f.Sensor.Tick(2000);
                Assert.That(changed, Is.EqualTo(2));
                Assert.That(f.Sensor.LastSample.ResourceAmount, Is.EqualTo(12));
            }
        }
        [Test] public void OutOfRangeRetainsLease_ButDeletionReleasesAndOldDataIsDiagnostic()
        {
            using(var f=new Fixture()) { f.Sensor.Tick(0); f.Position=new Vector3(-2,0,0); f.Sensor.Tick(1);
                Assert.That(f.Sensor.Reason,Is.EqualTo(SensorReadReason.OutOfRange)); Assert.That(f.Sensor.TryRead(out _),Is.False); Assert.That(f.Sensor.LastSample,Is.Not.Null); Assert.That(f.Context.Compute.Used,Is.EqualTo(10));
                f.Region.Remove(f.Target.Id); f.Sensor.Tick(2); Assert.That(f.Context.Compute.Used,Is.Zero); Assert.That(f.Sensor.Reason,Is.EqualTo(SensorReadReason.TargetMissing)); }
        }
        [Test] public void StopPowerAndRegionExit_ReleaseAndResumeFresh()
        {
            using(var f=new Fixture()) { f.Sensor.Tick(0); f.Machine.SetRunState(ManagementOrigin.Field,MachineRunState.Sleeping);
                Assert.That(f.Context.Compute.Used,Is.Zero); Assert.That(f.Sensor.TryRead(out _),Is.False);
                f.Machine.SetRunState(ManagementOrigin.Field,MachineRunState.Running); f.Sensor.Tick(1); Assert.That(f.Sensor.TryRead(out _),Is.True);
                f.Machine.UpdateEnvironment(false,true); Assert.That(f.Context.Compute.Used,Is.Zero);
                f.Machine.UpdateEnvironment(true,true); f.Sensor.Tick(2); f.Region.Dispose(); f.Sensor.Tick(3); Assert.That(f.Context.Compute.Used,Is.Zero); }
        }
        [Test] public void BudgetWait_IsSingle_AndYieldsOnlyAtSampleBoundary()
        {
            using(var f=new Fixture(10)) { f.Sensor.Tick(0); f.Context.Compute.Submit(10,ComputeClass.Evaluation,WorkPriority.Normal,ComputeMergeKind.None,f.Machine.Id,1,0,false,out var urgent);
                f.Sensor.Tick(999); Assert.That(urgent.State,Is.EqualTo(ComputeState.Waiting)); f.Sensor.Tick(1000); Assert.That(urgent.State,Is.EqualTo(ComputeState.Running));
                for(int i=1001;i<1020;i++) f.Sensor.Tick(i); Assert.That(f.Context.Compute.WaitingCount,Is.EqualTo(1));
                f.Context.Compute.Release(urgent.Id); f.Sensor.Tick(1020); Assert.That(f.Sensor.TryRead(out _),Is.True); }
        }
        [Test] public void MissingProviderAnchorAndUnbind_AreExplicit()
        {
            using(var f=new Fixture()) { f.AnchorValid=false; f.Sensor.Tick(0); Assert.That(f.Sensor.Reason,Is.EqualTo(SensorReadReason.AnchorUnavailable)); Assert.That(f.Context.Compute.Used,Is.Zero);
                f.AnchorValid=true; f.Provider.Dispose(); f.Sensor.Tick(1); Assert.That(f.Sensor.Reason,Is.EqualTo(SensorReadReason.ProviderUnavailable));
                f.Sensor.Bind(default); f.Sensor.Tick(2); Assert.That(f.Sensor.Reason,Is.EqualTo(SensorReadReason.NoTarget)); }
        }
        [Test] public void RebindDuringEvent_DoesNotPublishOldChanged()
        {
            using(var f=new Fixture()) { int stale=0; f.Sensor.Sampled+=e=>f.Sensor.Bind(default); f.Sensor.Changed+=e=>{if(e.Snapshot!=null)stale++;};
                f.Sensor.Tick(0); Assert.That(stale,Is.Zero); Assert.That(f.Sensor.TryRead(out _),Is.False); Assert.That(f.Context.Compute.Used,Is.Zero); }
        }
        [Test] public void DisposeSetDuringSample_DoesNotInvalidateIteration()
        {
            using (var f = new Fixture())
            {
                f.Sensor.Sampled += _ => f.Context.Sensors.Dispose();
                Assert.DoesNotThrow(() => f.Context.Sensors.Tick(0));
                Assert.That(f.Context.Compute.Used, Is.Zero);
                Assert.That(f.Sensor.Reason, Is.EqualTo(SensorReadReason.Disposed));
            }
        }
        [Test] public void BetweenSampleBoundaries_WarmedSetTickDoesNotAllocate()
        {
            using (var f = new Fixture())
            {
                for (int i = 0; i < 16; i++) f.Context.Sensors.Tick(0);
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int i = 0; i < 1000; i++) f.Context.Sensors.Tick(0);
                long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
                Assert.That(allocated, Is.Zero, "No per-frame collection enumeration allocation.");
            }
        }
        [Test] public void DisableStopAndUninstall_ReleaseWithoutBlockingHardwareGate()
        {
            using (var f = new Fixture())
            {
                f.Sensor.Tick(0);
                f.Machine.SetComponentEnabled(ManagementOrigin.Field, HardwareKind.Sensor, 0, false);
                Assert.That(f.Context.Compute.Used, Is.Zero);
                f.Machine.SetComponentEnabled(ManagementOrigin.Field, HardwareKind.Sensor, 0, true);
                f.Sensor.Tick(1); Assert.That(f.Context.Compute.Used, Is.EqualTo(10));
                f.Machine.SetRunState(ManagementOrigin.Field, MachineRunState.Stopped);
                Assert.That(f.Context.Compute.Used, Is.Zero);
                Assert.That(f.Session.Machines.Remove(f.Machine.Id, ManagementOrigin.Field, HardwareKind.Sensor, 0), Is.EqualTo(MachineManagementResult.Completed));
                f.Sensor.Tick(2); Assert.That(f.Sensor.Reason, Is.EqualTo(SensorReadReason.NotInstalled));
                Assert.That(f.Sensor.TryRead(out _), Is.False);
            }
        }
        [Test] public void RangeBoundary_IsInclusive_AndFarMonitoringYieldsOnlyOnBoundary()
        {
            using (var f = new Fixture(10))
            {
                f.Position = new Vector3(-1,0,0); f.Sensor.Tick(0);
                Assert.That(f.Sensor.TryRead(out _), Is.True);
                f.Context.Compute.Submit(10,ComputeClass.Evaluation,WorkPriority.Normal,ComputeMergeKind.None,f.Machine.Id,1,0,false,out var urgent);
                f.Position = new Vector3(-1.001f,0,0); f.Sensor.Tick(1);
                Assert.That(f.Sensor.TryRead(out _), Is.False);
                Assert.That(urgent.State, Is.EqualTo(ComputeState.Waiting));
                f.Sensor.Tick(1000); Assert.That(urgent.State, Is.EqualTo(ComputeState.Running));
                f.Context.Compute.Release(urgent.Id);
            }
        }
        [Test] public void SleepDuringSample_DoesNotPublishStaleValidChange()
        {
            using (var f = new Fixture())
            {
                int validChanges = 0;
                f.Sensor.Sampled += _ => f.Machine.SetRunState(ManagementOrigin.Field, MachineRunState.Sleeping);
                f.Sensor.Changed += e => { if (e.Snapshot != null) validChanges++; };
                f.Sensor.Tick(0);
                Assert.That(validChanges, Is.Zero);
                Assert.That(f.Sensor.TryRead(out _), Is.False);
                Assert.That(f.Context.Compute.Used, Is.Zero);
            }
        }
    }
}
