using System;
using System.Collections.Generic;
using AutoEra.Machines;
using AutoEra.Machines.Sensors;
using AutoEra.Save;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class MachineSensorSnapshotEditModeTests
    {
        private sealed class Fixture : ISensorAnchor, IDisposable
        {
            internal readonly AutoEraWorldSession World = new AutoEraWorldSessionFactory().Create(0);
            internal readonly InitialRegion Region;
            internal readonly MachineExecutionContext Context;
            internal readonly RegionObject Target;
            internal readonly RegionSensorReadProvider Provider;
            internal readonly RegionSensorEnvironment Environment;
            internal readonly MachineSensor Sensor;
            internal Fixture(SensorKind kind = SensorKind.ObjectState)
            {
                Region = new InitialRegion(World, new Rect(-50, -50, 100, 100));
                var machine = World.Machines.Create(new MachineDefinition(1, "Fixture", 1, 1, 1, 0, 30, true, true, 100));
                var core = World.Machines.CreateComponent(new ComponentDefinition(2, HardwareKind.Core, 1, 0, 20, 40, false));
                var component = World.Machines.CreateComponent(new ComponentDefinition(3, HardwareKind.Sensor, 1, 0, 0, 0, false));
                World.Machines.Install(machine.Id, ManagementOrigin.Library, core.Id, 0); World.Machines.Install(machine.Id, ManagementOrigin.Library, component.Id, 0);
                Region.DeployMachine(machine.Id, Vector2.zero, Vector2.one, out _);
                machine.Activate(ManagementOrigin.Field); machine.UpdateEnvironment(true, true); machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);
                Context = new MachineExecutionContext(machine, World.IdAllocator);
                Target = Region.Register(PersistentObjectKind.Building, "public", new Vector2(3, 0), Vector2.one); Target.SetPublicState("生产", 12);
                Provider = new RegionSensorReadProvider(Target, p => new Vector3(3, 0, 0));
                Environment = new RegionSensorEnvironment(Region); Environment.Register(Provider);
                Sensor = Context.Sensors.Bind(component, new SensorProfile(3, 1, kind), Environment, this);
            }
            public bool TryGetPosition(out Vector3 position) { position = Vector3.zero; return true; }
            public void Dispose() { Context.Dispose(); Provider.Dispose(); Environment.Dispose(); Region.Dispose(); World.Dispose(); }
        }
        private static MachineSensorSnapshot Json(MachineSensorSnapshot snapshot)
        {
            string json = WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(5000, 1000, 1, "Fixture", new[] { new WorldSnapshotSection("sensor", 1, snapshot) }));
            Assert.That(WorldSnapshotCodec.TryRead(json, new Dictionary<string, int> { { "sensor", 1 } }, out var file, out var reason), Is.True, reason);
            Assert.That(file.TryReadSection<MachineSensorSnapshot>("sensor", out var restored, out reason), Is.True, reason); return restored;
        }
        private static void RestoreCompute(Fixture source, Fixture target)
        {
            Assert.That(source.Context.Compute.TryCapturePersistent(out var snapshot), Is.True);
            string json = WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(5000, 1000, 1, "Fixture", new[] { new WorldSnapshotSection("compute", 1, snapshot) }));
            Assert.That(WorldSnapshotCodec.TryRead(json, new Dictionary<string, int> { { "compute", 1 } }, out var file, out var reason), Is.True, reason);
            Assert.That(file.TryReadSection<MachineComputeSnapshot>("compute", out var saved, out reason), Is.True, reason);
            Assert.That(target.Context.Compute.RestorePersistent(saved, 5000), Is.True);
        }
        [Test] public void Json_RestoresBindingSampleAndRemainingInterval_WithoutReplay()
        {
            using (var source = new Fixture())
            using (var target = new Fixture())
            {
                source.Sensor.Bind(source.Provider.Target); source.Sensor.Bind(source.Provider.Target); source.Sensor.Tick(0);
                Assert.That(source.Sensor.TryCapturePersistent(400, out var snapshot), Is.True);
                RestoreCompute(source, target);
                int samples = 0, changed = 0; ulong sequence = 0; target.Sensor.Sampled += e => { samples++; sequence = e.Sequence; }; target.Sensor.Changed += _ => changed++;
                Assert.That(target.Sensor.RestorePersistent(Json(snapshot), 5000), Is.True);
                Assert.That(samples, Is.Zero); Assert.That(changed, Is.Zero); Assert.That(target.Sensor.Generation, Is.EqualTo(2));
                Assert.That(target.Sensor.TryRead(out var sample), Is.True); Assert.That(sample.ResourceAmount, Is.EqualTo(12));
                target.Sensor.Tick(5599); Assert.That(samples, Is.Zero); target.Sensor.Tick(5600);
                Assert.That(samples, Is.EqualTo(1)); Assert.That(sequence, Is.EqualTo(snapshot.Sequence + 1)); Assert.That(changed, Is.Zero);
                snapshot.Sample.ResourceAmount = 99; Assert.That(source.Sensor.LastSample.ResourceAmount, Is.EqualTo(12));
            }
        }
        [Test] public void SoilAndCropCells_PreserveValidityAndWeightsThroughJson()
        {
            using (var target = new Fixture(SensorKind.Soil))
            {
                var snapshot = new MachineSensorSnapshot { ComponentId = target.Sensor.Id, Target = target.Provider.Target, Generation = 1, Sequence = 3,
                    Reason = SensorReadReason.None, Sample = new SensorReadoutSnapshot { Version = 2,
                        Soil = new[] { new SoilReadoutSnapshot { Id = 1, Area = 1, Moisture = 30, Position = Vector3.one, Valid = true },
                            new SoilReadoutSnapshot { Id = 2, Area = 3, Moisture = 60, Position = Vector3.zero, Valid = true } },
                        Crops = new[] { new CropReadoutSnapshot { Id = 1, Suitability = .5f, GrowthSpeed = .2f, Valid = true } } } };
                Assert.That(target.Sensor.RestorePersistent(Json(snapshot), 5000), Is.True);
                Assert.That(target.Sensor.TryRead(out var sample), Is.True); Assert.That(sample.TryGetWeightedMoisture(out var moisture), Is.True);
                Assert.That(moisture, Is.EqualTo(52.5)); Assert.That(sample.Soil[0].Position, Is.EqualTo(Vector3.one)); Assert.That(sample.Crops[0].IsValid, Is.True);
            }
        }
        [Test] public void MissingTarget_PreservesIdentityAndLastDiagnosticData_ThenReportsMissingNormally()
        {
            using (var source = new Fixture())
            using (var target = new Fixture())
            {
                source.Sensor.Bind(source.Provider.Target); source.Sensor.Tick(0); source.Region.Remove(source.Target.Id); source.Sensor.Tick(1);
                source.Sensor.TryCapturePersistent(1, out var snapshot); target.Region.Remove(target.Target.Id);
                Assert.That(target.Sensor.RestorePersistent(Json(snapshot), 5000), Is.True);
                Assert.That(target.Sensor.Target.Id, Is.EqualTo(source.Target.Id)); Assert.That(target.Sensor.Reason, Is.EqualTo(SensorReadReason.TargetMissing));
                Assert.That(target.Sensor.TryRead(out _), Is.False); Assert.That(target.Sensor.LastSample.ResourceAmount, Is.EqualTo(12));
                target.Sensor.Tick(5001); Assert.That(target.Sensor.Reason, Is.EqualTo(SensorReadReason.TargetMissing)); Assert.That(target.Context.Compute.Used, Is.Zero);
            }
        }
        [Test] public void InvalidCellOrCounterpartIdentity_RejectsBeforeAnyMutation()
        {
            using (var source = new Fixture())
            using (var target = new Fixture())
            {
                source.Sensor.Bind(source.Provider.Target); source.Sensor.Tick(0); source.Sensor.TryCapturePersistent(0, out var snapshot);
                RestoreCompute(source, target);
                var bad = Json(snapshot); bad.ComponentId = new PersistentId(999);
                Assert.That(target.Sensor.RestorePersistent(bad, 5000), Is.False); Assert.That(target.Sensor.Generation, Is.Zero);
                bad = Json(snapshot); bad.Sample.Soil = new[] { new SoilReadoutSnapshot { Id = 1, Area = -1, Moisture = 30 } };
                Assert.That(target.Sensor.RestorePersistent(bad, 5000), Is.False); Assert.That(target.Sensor.LastSample, Is.Null);
                bad = Json(snapshot); bad.RemainingMilliseconds = 1001;
                Assert.That(target.Sensor.RestorePersistent(bad, 5000), Is.False);
                Assert.That(target.Sensor.RestorePersistent(Json(snapshot), 5000), Is.True);
            }
        }
        [Test] public void ExhaustedSampleSequence_DoesNotWrapToAReusedNotification()
        {
            using (var target = new Fixture())
            {
                var snapshot = new MachineSensorSnapshot { ComponentId = target.Sensor.Id, Target = target.Provider.Target, Generation = 1, Sequence = ulong.MaxValue,
                    Reason = SensorReadReason.NoTarget };
                Assert.That(target.Sensor.RestorePersistent(Json(snapshot), 5000), Is.True);
                Assert.Throws<OverflowException>(() => target.Sensor.Tick(5000));
            }
        }
    }
}
