using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AutoEra.Energy;
using AutoEra.Machines;
using AutoEra.World.Identity;
using UnityEngine;
using UnityEngine.Profiling;
using Unity.Profiling;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace AutoEra.Tests.EnergyBench
{
    // Copied into an isolated benchmark project only. Never installed in the product scene.
    public sealed class EnergyBenchmarkRunner : MonoBehaviour
    {
        private IEnumerator Start()
        {
            yield return null;
            yield return null;
            try
            {
                int probeCount;
                using (var probe = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "GC.Alloc", 8, ProfilerRecorderOptions.CollectOnlyOnCurrentThread))
                {
                    var allocated = new byte[1024 * 1024]; GC.KeepAlive(allocated); probe.Stop();
                    probeCount = probe.Count;
                }
                if (probeCount < 1) throw new InvalidOperationException("GC.Alloc calibration failed; zero cannot be claimed.");
                var results = new List<Measurement>();
                foreach (int count in new[] { 10, 50, 100 })
                {
                    var actual = new Fixture(count, false); var reference = new Fixture(count, true);
                    foreach (string phase in new[] { "normal", "shortage", "recovery" })
                    {
                        actual.Solar.EnvironmentPower = reference.Solar.EnvironmentPower = phase == "shortage" ? 0 : count * 10;
                        for (int round = 0; round < 3; round++)
                        {
                            if ((round & 1) == 0) { results.Add(Measure(reference, count, phase, round)); results.Add(Measure(actual, count, phase, round)); }
                            else { results.Add(Measure(actual, count, phase, round)); results.Add(Measure(reference, count, phase, round)); }
                        }
                    }
                }
                var report = new Report { unity = UnityEngine.Application.unityVersion, development = Debug.isDebugBuild,
                    cpu = SystemInfo.processorType, logicalProcessors = SystemInfo.processorCount,
                    os = SystemInfo.operatingSystem, allocationProbeSamples = probeCount, measurements = results.ToArray() };
                string output = Argument("-benchmarkOutput");
                if (string.IsNullOrEmpty(output)) throw new InvalidOperationException("Missing -benchmarkOutput.");
                File.WriteAllText(output, JsonUtility.ToJson(report, true));
                UnityEngine.Application.Quit(0);
            }
            catch (Exception error) { Debug.LogException(error); UnityEngine.Application.Quit(1); }
        }

        private static Measurement Measure(Fixture fixture, int count, string phase, int round)
        {
            const int samples = 6000;
            for (int i = 0; i < 512; i++) fixture.Step();
            var durations = new double[samples];
            int capacity = (count + 8) * samples * 2;
            using var allocations = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "GC.Alloc", capacity, ProfilerRecorderOptions.CollectOnlyOnCurrentThread);
            long started = Stopwatch.GetTimestamp();
            for (int i = 0; i < samples; i++)
            {
                long stepStart = Stopwatch.GetTimestamp();
                fixture.Step();
                durations[i] = (Stopwatch.GetTimestamp() - stepStart) * (1000000d / Stopwatch.Frequency);
            }
            long elapsed = Stopwatch.GetTimestamp() - started;
            allocations.Stop();
            if (allocations.Count == capacity) throw new InvalidOperationException("GC.Alloc samples truncated.");
            // GC.Alloc is a timing marker: Sample.Value is not allocated bytes.
            // A calibrated count of zero proves zero allocations; positive byte size is unmeasured.
            long allocation = allocations.Count == 0 ? 0 : -1;
            Array.Sort(durations);
            return new Measurement { implementation = fixture.Reference ? "reference" : "production", machines = count,
                phase = phase, round = round, samples = samples, allocationBytes = allocation, allocationSamples = allocations.Count,
                p50Microseconds = durations[samples / 2], p95Microseconds = durations[(int)(samples * .95)],
                p99Microseconds = durations[(int)(samples * .99)], seconds = elapsed / (double)Stopwatch.Frequency,
                stopped = fixture.Last.StoppedByShortage.Count, consumed = fixture.Last.ConsumedPower };
        }

        private sealed class Fixture
        {
            private readonly EnergyGrid _actual;
            private readonly EnergyGridReference _reference;
            private readonly MachineEnergyConsumer[] _loads;
            public readonly bool Reference;
            public readonly EnvironmentGenerator Solar;
            public EnergyGridSnapshot Last;
            public Fixture(int count, bool reference)
            {
                Reference = reference;
                if (reference) _reference = new EnergyGridReference(); else _actual = new EnergyGrid();
                Solar = new EnvironmentGenerator(new PersistentId(1), count * 10);
                if (reference) _reference.AddGenerator(Solar); else _actual.AddGenerator(Solar);
                _loads = new MachineEnergyConsumer[count];
                // Explicit synthetic benchmark configuration, identical for both implementations.
                var definition = new MachineDefinition(1, "Benchmark", 1, 2, 1, 2, 20, true, true, 100,
                    idlePower: .2, workingPower: 2);
                for (int i = 0; i < count; i++)
                {
                    var machine = new MachineInstance(new PersistentId((ulong)(100 + i)), definition, (ulong)(i + 1), "Benchmark") { Deployed = true };
                    machine.Install(ManagementOrigin.Field, new ComponentInstance(new PersistentId((ulong)(1000 + i)),
                        new ComponentDefinition(2, HardwareKind.Sensor, 1, 0, 0, 0, false, .05, .5)), 0);
                    machine.Activate(ManagementOrigin.Field); machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);
                    machine.UpdateNavigationActivity(true);
                    var load = new MachineEnergyConsumer(machine, (PowerPriority)(i % 4));
                    _loads[i] = load;
                    if (reference) _reference.AddConsumer(load); else _actual.AddConsumer(load);
                }
            }
            public void Step()
            {
                Profiler.BeginSample(Reference ? "AutoEra.Energy.ReferenceTick" : "AutoEra.Energy.Tick");
                Last = Reference ? _reference.Tick(1f / 60f, true) : _actual.Tick(1f / 60f, true);
                for (int i = 0; i < _loads.Length; i++) _loads[i].ApplySupply();
                Profiler.EndSample();
            }
        }
        private static string Argument(string name)
        { var args = Environment.GetCommandLineArgs(); for (int i = 0; i + 1 < args.Length; i++) if (args[i] == name) return args[i + 1]; return null; }
        [Serializable] private sealed class Report
        { public string unity, cpu, os; public bool development; public int logicalProcessors, allocationProbeSamples; public Measurement[] measurements; }
        [Serializable] private sealed class Measurement
        {
            public string implementation, phase; public int machines, round, samples, stopped, allocationSamples; public long allocationBytes;
            public double p50Microseconds, p95Microseconds, p99Microseconds, seconds; public float consumed;
        }
    }
}
