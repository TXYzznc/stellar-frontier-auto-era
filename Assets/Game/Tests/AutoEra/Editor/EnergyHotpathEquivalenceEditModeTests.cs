using System;
using System.Collections.Generic;
using AutoEra.Energy;
using AutoEra.Tests.EnergyBench;
using AutoEra.World.Identity;
using NUnit.Framework;
using Unity.Profiling;

namespace AutoEra.Tests.Editor
{
    public sealed class EnergyHotpathEquivalenceEditModeTests
    {
        private sealed class Inputs
        {
            public readonly EnergyGrid Actual;
            public readonly EnergyGridReference Reference;
            public readonly List<EnergyConsumer> Loads = new List<EnergyConsumer>();
            public readonly EnvironmentGenerator Solar = new EnvironmentGenerator(new PersistentId(1), 15);
            public readonly FuelGenerator Fuel = new FuelGenerator(new PersistentId(2), 60, 120);
            public readonly BatteryStorage Battery = new BatteryStorage(new PersistentId(3), 240, 100);
            public Inputs(int count, bool reference)
            {
                if (reference) Reference = new EnergyGridReference(); else Actual = new EnergyGrid();
                if (reference) { Reference.AddGenerator(Solar); Reference.AddGenerator(Fuel); Reference.AddStorage(Battery); }
                else { Actual.AddGenerator(Solar); Actual.AddGenerator(Fuel); Actual.AddStorage(Battery); }
                for (int i = 0; i < count; i++)
                {
                    var load = new EnergyConsumer(new PersistentId((ulong)(100 + i)), (PowerPriority)(i % 4), .2f, 2.5f) { IsWorking = true };
                    Loads.Add(load); if (reference) Reference.AddConsumer(load); else Actual.AddConsumer(load);
                }
            }
            public EnergyGridSnapshot Tick(float seconds, long world)
            { bool day = Solar.UpdateEnvironment(world); return Actual != null ? Actual.Tick(seconds, day) : Reference.Tick(seconds, day); }
        }

        [TestCase(0)] [TestCase(1)] [TestCase(10)] [TestCase(50)] [TestCase(100)]
        public void SeededSeries_MatchesFrozenReferenceAcrossFuelStorageDaylightAndLoadChanges(int count)
        {
            for (int seed = 0; seed < 5; seed++)
            {
                var a = new Inputs(count, false); var b = new Inputs(count, true); var random = new Random(740 + seed);
                long world = DaylightCycle.DaylightMilliseconds - 1000;
                float[] durations = { 0, 1f / 60, .125f, 1, 60, 240, 480 };
                for (int step = 0; step < 80; step++)
                {
                    float duration = durations[random.Next(durations.Length)]; world += (long)(duration * 1000);
                    if (count > 0)
                    {
                        int index = random.Next(count); bool active = random.Next(3) != 0, working = random.Next(2) != 0;
                        var priority = (PowerPriority)random.Next(4);
                        a.Loads[index].IsDemandActive = b.Loads[index].IsDemandActive = active;
                        a.Loads[index].IsWorking = b.Loads[index].IsWorking = working;
                        a.Loads[index].Priority = b.Loads[index].Priority = priority;
                    }
                    if (step % 7 == 0)
                    {
                        bool allowed = random.Next(2) != 0; float ratio = random.Next(5) / 4f;
                        a.Fuel.AllowsCharging = b.Fuel.AllowsCharging = allowed;
                        a.Fuel.ChargeTargetRatio = b.Fuel.ChargeTargetRatio = ratio;
                        float fuel = random.Next(40); a.Fuel.FuelEnergyAvailable = b.Fuel.FuelEnergyAvailable = fuel;
                    }
                    Compare(a.Tick(duration, world), b.Tick(duration, world));
                    Assert.That(a.Fuel.FuelEnergyAvailable, Is.EqualTo(b.Fuel.FuelEnergyAvailable).Within(1e-5));
                    Assert.That(a.Fuel.FuelEnergyConsumed, Is.EqualTo(b.Fuel.FuelEnergyConsumed).Within(1e-5));
                    for (int i = 0; i < count; i++)
                    {
                        Assert.That(a.Loads[i].IsPowered, Is.EqualTo(b.Loads[i].IsPowered));
                        Assert.That(a.Loads[i].ActualPower, Is.EqualTo(b.Loads[i].ActualPower));
                    }
                }
            }
        }
        private static void Compare(EnergyGridSnapshot a, EnergyGridSnapshot b)
        {
            Assert.That(a.GeneratedPower, Is.EqualTo(b.GeneratedPower).Within(1e-5));
            Assert.That(a.ConsumedPower, Is.EqualTo(b.ConsumedPower).Within(1e-5));
            Assert.That(a.StoredCharge, Is.EqualTo(b.StoredCharge).Within(1e-5));
            Assert.That(a.StorageCapacity, Is.EqualTo(b.StorageCapacity));
            Assert.That(a.DiscardedPower, Is.EqualTo(b.DiscardedPower).Within(1e-5));
            Assert.That(a.ChargingPower, Is.EqualTo(b.ChargingPower).Within(1e-5));
            Assert.That(a.DischargingPower, Is.EqualTo(b.DischargingPower).Within(1e-5));
            Assert.That(a.HasShortfall, Is.EqualTo(b.HasShortfall));
            CollectionAssert.AreEqual(b.StoppedByShortage, a.StoppedByShortage);
        }
        [Test]
        public void ParticipantAndPriorityChanges_RebuildOrderWithoutChangingExistingQueuePositions()
        {
            var a = new Inputs(10, false); var b = new Inputs(10, true);
            a.Fuel.IsOn = b.Fuel.IsOn = false; a.Battery.Charge = b.Battery.Charge = 0;
            Compare(a.Tick(1, DaylightCycle.DaylightMilliseconds), b.Tick(1, DaylightCycle.DaylightMilliseconds));
            long retained = a.Loads[1].QueueOrder;
            a.Actual.RemoveConsumer(a.Loads[0].Id); b.Reference.RemoveConsumer(b.Loads[0].Id);
            a.Loads[2].Priority = b.Loads[2].Priority = PowerPriority.Critical;
            a.Loads[3].QueueOrder = b.Loads[3].QueueOrder = a.Loads[4].QueueOrder; // Tie retains registration order.
            Compare(a.Tick(1, DaylightCycle.DaylightMilliseconds), b.Tick(1, DaylightCycle.DaylightMilliseconds));
            a.Actual.AddConsumer(a.Loads[0]); b.Reference.AddConsumer(b.Loads[0]);
            Assert.That(a.Loads[1].QueueOrder, Is.EqualTo(retained));
            Compare(a.Tick(1, DaylightCycle.DaylightMilliseconds), b.Tick(1, DaylightCycle.DaylightMilliseconds));
            Assert.Throws<ArgumentException>(() => a.Actual.AddConsumer(a.Loads[0]));
        }
        [Test]
        public void PublishedStops_AreImmutableAndReusedOnlyWhenTheOrderedSetIsUnchanged()
        {
            var a = new Inputs(10, false); a.Fuel.IsOn = false; a.Battery.Charge = 0;
            var old = a.Tick(1, DaylightCycle.DaylightMilliseconds); var ids = new List<PersistentId>(old.StoppedByShortage);
            Assert.Throws<NotSupportedException>(() => ((IList<PersistentId>)old.StoppedByShortage)[0] = new PersistentId(99));
            long revision = a.Actual.Revision;
            Assert.That(a.Tick(1, DaylightCycle.DaylightMilliseconds).StoppedByShortage, Is.SameAs(old.StoppedByShortage));
            Assert.That(a.Actual.Revision, Is.EqualTo(revision));
            a.Fuel.IsOn = true; a.Fuel.FuelEnergyAvailable = 100;
            Assert.That(a.Tick(1, DaylightCycle.DaylightMilliseconds).StoppedByShortage, Is.Empty);
            CollectionAssert.AreEqual(ids, old.StoppedByShortage); Assert.That(old.ConsumedPower, Is.Zero);
        }
        [Test]
        public void StableShortage_UsesNoAllocationAfterWarmupAndAfterOnePriorityChange()
        {
            using (var probe = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "GC.Alloc", 8, ProfilerRecorderOptions.CollectOnlyOnCurrentThread))
            {
                var allocation = new byte[4096]; GC.KeepAlive(allocation); probe.Stop();
                Assert.That(probe.Count, Is.GreaterThan(0), "Calibrate the allocation meter before accepting zero.");
            }
            var input = new Inputs(100, false); input.Fuel.IsOn = false; input.Battery.Charge = 0;
            for (int pass = 0; pass < 2; pass++)
            {
                input.Loads[1].Priority = pass == 0 ? PowerPriority.Critical : PowerPriority.Pausable;
                for (int i = 0; i < 100; i++) input.Tick(1, DaylightCycle.DaylightMilliseconds);
                using var recorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "GC.Alloc", 1024, ProfilerRecorderOptions.CollectOnlyOnCurrentThread);
                for (int i = 0; i < 500; i++) input.Tick(1, DaylightCycle.DaylightMilliseconds);
                recorder.Stop(); Assert.That(recorder.Count, Is.Zero);
            }
        }
    }
}
