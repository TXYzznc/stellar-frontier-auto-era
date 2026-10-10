using System;
using System.Collections.Generic;
using AutoEra.Buildings;
using AutoEra.Logistics;
using AutoEra.Save;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Time;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    /// <summary>Provider protocol tests. Probe providers are not implementations of gameplay domains.</summary>
    public sealed class OfflineProviderContractEditModeTests
    {
        private AutoEraWorldSession _world;
        private OfflineSettlementReport _report;
        private PersistentId _subject;
        private static readonly DateTimeOffset Utc = new DateTimeOffset(2026, 10, 8, 0, 0, 0, TimeSpan.Zero);
        [SetUp]
        public void SetUp()
        {
            _world = new AutoEraWorldSessionFactory().Create(0);
            _world.Resources.Configure(new ResourceItemCatalog(new[] {
                new ResourceItemDefinition(ResourceItemCatalog.Ore, CargoItemClass.CommonResource) }));
            _world.IdAllocator.TryAllocate(out _subject);
            _report = new OfflineSettlementReport("providers", 0, 10);
            _report.TryRefreshResources(_world, true, out _);
        }
        [TearDown] public void TearDown() => _world.Dispose();

        private sealed class Probe : IOfflineWorldDomainProvider
        {
            public string Domain { get; set; }
            public int Version { get; set; } = 1;
            public WorldEventPhase Phase { get; set; }
            public bool IsAtCommitBoundary => true;
            internal readonly List<string> Trace;
            internal PersistentId Subject;
            internal bool Applied, Planned, FailAdvance, FailApply, InvalidDomain, ThrowAdvance, ThrowApply, Repeat, Stopped;
            internal Probe(string name, WorldEventPhase phase, PersistentId subject, List<string> trace)
            { Domain = name; Phase = phase; Subject = subject; Trace = trace; }
            public bool IsCurrent(OfflineScheduledEvent item) => !Stopped && item.Generation == 1;
            public void CollectNextEvents(long time, List<OfflineEventSpec> into)
            {
                if (Stopped || Planned && !Repeat) return;
                Planned = true;
                into.Add(new OfflineEventSpec { Domain = InvalidDomain ? "foreign" : Domain, Due = 5,
                    Phase = Phase, Subject = Subject, Generation = 1, Payload = new OfflineEventPayload(),
                    Kind = Phase == WorldEventPhase.Energy ? OfflineEventKind.EnergyBoundary : Phase == WorldEventPhase.Algorithm ? OfflineEventKind.AlgorithmWake : OfflineEventKind.ForestBoundary });
            }
            public bool TryAdvanceContinuous(long from, long to, out string reason)
            { if (ThrowAdvance) throw new InvalidOperationException("continuous-exception"); Trace.Add("advance:" + Domain + ":" + from + ":" + to); reason = FailAdvance ? "continuous-failed" : null; return !FailAdvance; }
            public bool TryApply(OfflineScheduledEvent item, out string reason)
            { if (ThrowApply) throw new InvalidOperationException("apply-exception"); Applied = true; Trace.Add("apply:" + Domain); reason = FailApply ? "apply-failed" : null; return !FailApply; }
        }
        private OfflineWorldDomainExecutor Executor(params Probe[] probes) =>
            new OfflineWorldDomainExecutor(_world, probes, new OfflineProviderManifest {
                RequiredDomains = Array.ConvertAll(probes, p => p.Domain) }, _report);
        private OfflineEventScheduler Scheduler(OfflineWorldDomainExecutor executor) =>
            new OfflineEventScheduler(_world.Clock, executor, Utc, Utc.AddMilliseconds(10), "providers");

        [Test]
        public void CompleteWhitelistRejectsMissingRealDomains_EmptyManifestIsNotAnOfflineCapability()
        {
            var energy = new Probe("energy", WorldEventPhase.Energy, _subject, new List<string>());
            Assert.That(OfflineProviderManifest.FirstVersion().TryValidate(new[] { energy }, out var reason), Is.False);
            Assert.That(reason, Does.Contain("缺少真实离线提供者"));
            Assert.That(new OfflineProviderManifest { RequiredDomains = Array.Empty<string>() }.TryValidate(Array.Empty<Probe>(), out _), Is.False);
            Assert.That(OfflineProviderManifest.FirstVersion().RequiredDomains, Does.Contain("water"));
            Assert.That(OfflineProviderManifest.FirstVersion().RequiredDomains, Does.Contain("construction"));
            Assert.That(_world.Clock.WorldMilliseconds, Is.Zero);
        }

        [Test]
        public void RegistrationOrderDoesNotChangePhaseOrder_NewPlansAreEmittedOnce()
        {
            var trace = new List<string>();
            var production = new Probe("production", WorldEventPhase.WorldState, _subject, trace);
            var energy = new Probe("energy", WorldEventPhase.Energy, _subject, trace);
            var executor = Executor(production, energy);
            var scheduler = Scheduler(executor);
            executor.Seed(scheduler);
            for (int i = 0; i < 20 && !scheduler.Completed; i++) scheduler.Pump(1, 1000);
            Assert.That(scheduler.FailureReason, Is.Null);
            Assert.That(scheduler.Completed, Is.True);
            CollectionAssert.AreEqual(new[] { "apply:energy", "apply:production" }, trace.FindAll(s => s.StartsWith("apply:")));
            Assert.That(trace[0], Is.EqualTo("advance:energy:0:5"));
            Assert.That(trace[1], Is.EqualTo("advance:production:0:5"));
            Assert.That(scheduler.ProcessedEvents, Is.EqualTo(2));
            Assert.That(_report.TryRefreshResources(_world, false, out _), Is.True);
            Assert.That(_report.TryComplete(scheduler), Is.True);
            Assert.That(_report.CapturePersistent().Events.Length, Is.EqualTo(2));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ProviderFailurePreventsSavingPartialCandidate(bool failDuringAdvance)
        {
            var probe = new Probe("energy", WorldEventPhase.Energy, _subject, new List<string>()) {
                FailAdvance = failDuringAdvance, FailApply = !failDuringAdvance };
            var executor = Executor(probe);
            var scheduler = Scheduler(executor);
            executor.Seed(scheduler);
            scheduler.Pump(10, 1000);
            Assert.That(scheduler.FailureReason, Is.Not.Null);
            Assert.That(scheduler.Completed, Is.False);
            Assert.That(executor.IsAtCommitBoundary, Is.False);
            Assert.That(scheduler.TryCapturePersistent(out _), Is.False);
            Assert.That(_report.Completed, Is.False);
        }

        [Test]
        public void ProviderCannotEmitForeignDomainEvent_OriginalClockIsUnchanged()
        {
            var probe = new Probe("energy", WorldEventPhase.Energy, _subject, new List<string>()) { InvalidDomain = true };
            var executor = Executor(probe);
            Assert.Throws<ArgumentException>(() => executor.Seed(Scheduler(executor)));
            Assert.That(_world.Clock.WorldMilliseconds, Is.Zero);
            Assert.That(executor.IsAtCommitBoundary, Is.False);
        }

        [Test]
        public void AlgorithmProviderRequiresRealNoProgressProtection_AndReportRunMustMatch()
        {
            var probe = new Probe("algorithms", WorldEventPhase.Algorithm, _subject, new List<string>());
            Assert.Throws<ArgumentException>(() => Executor(probe));
            var energy = new Probe("energy", WorldEventPhase.Energy, _subject, new List<string>());
            var executor = Executor(energy);
            var wrong = new OfflineEventScheduler(_world.Clock, executor, Utc, Utc.AddMilliseconds(10), "another-run");
            Assert.Throws<ArgumentException>(() => executor.Seed(wrong));
            Assert.That(_world.Clock.WorldMilliseconds, Is.Zero);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ThrownProviderFailureAlsoPoisonsCandidate(bool duringAdvance)
        {
            var probe = new Probe("energy", WorldEventPhase.Energy, _subject, new List<string>()) {
                ThrowAdvance = duringAdvance, ThrowApply = !duringAdvance };
            var executor = Executor(probe);
            var scheduler = Scheduler(executor);
            executor.Seed(scheduler);
            scheduler.Pump(10, 1000);
            Assert.That(scheduler.FailureReason, Does.Contain("exception"));
            Assert.That(executor.IsAtCommitBoundary, Is.False);
            Assert.That(scheduler.TryCapturePersistent(out _), Is.False);
        }

        [Test]
        public void MissingProviderInRestoredEventIsAnError_NotAnObsoleteGeneration()
        {
            var executor = Executor(new Probe("energy", WorldEventPhase.Energy, _subject, new List<string>()));
            Assert.Throws<InvalidOperationException>(() => executor.IsCurrent(new OfflineScheduledEvent {
                Domain = "missing-domain", Phase = WorldEventPhase.WorldState, Generation = 1 }));
            Assert.That(executor.IsAtCommitBoundary, Is.False);
        }

        private sealed class ProtectionProbe : IOfflineAlgorithmProtection
        {
            internal Probe Domain;
            internal WorldClock Clock;
            internal int Stops;
            internal bool Unavailable;
            public bool IsAtCommitBoundary => true;
            public bool IsCurrent(OfflineScheduledEvent item) => true;
            public bool TryAdvanceTo(long time, out string reason) { reason = null; return Clock.TryAdvanceTo(time); }
            public bool TryExecute(OfflineScheduledEvent item, OfflineEventScheduler scheduler, out string reason) { reason = null; return false; }
            public bool TryObserveAlgorithm(OfflineScheduledEvent item, out OfflineAlgorithmObservation observation)
            {
                if (Unavailable) { observation = null; return false; }
                observation = new OfflineAlgorithmObservation { Algorithm = item.Subject, Generation = item.Generation,
                    Cause = 1, ProgressRevision = 0, WorldMilliseconds = Clock.WorldMilliseconds };
                return true;
            }
            public bool TryStopNoProgressAlgorithm(PersistentId algorithm, out string reason)
            { Domain.Stopped = true; Stops++; reason = null; return true; }
        }
        [Test]
        public void ProtectedNoProgressEventIsAlsoRecorded_CompletionCheckpointDoesNotLoseItsReceipt()
        {
            var probe = new Probe("algorithms", WorldEventPhase.Algorithm, _subject, new List<string>()) { Repeat = true };
            var protection = new ProtectionProbe { Domain = probe, Clock = _world.Clock };
            var executor = new OfflineWorldDomainExecutor(_world, new[] { probe },
                new OfflineProviderManifest { RequiredDomains = new[] { "algorithms" } }, _report, protection);
            var scheduler = Scheduler(executor);
            executor.Seed(scheduler);
            scheduler.Pump(10, 1000);
            Assert.That(scheduler.FailureReason, Is.Null);
            Assert.That(scheduler.Completed, Is.True);
            Assert.That(protection.Stops, Is.EqualTo(1));
            _report.TryRefreshResources(_world, false, out _);
            Assert.That(_report.TryComplete(scheduler), Is.True);
            var root = new WorldSnapshotDocument(10, _subject.Value, 1, "Protocol fixture", new[] {
                new WorldSnapshotSection("fixture", 1, new Dictionary<string, string> { { "origin", "protocol test" } }) });
            Assert.That(OfflineWorldCheckpoint.TryCapture(root, scheduler, _report, out _, out var reason), Is.True, reason);
            Assert.That(_report.CapturePersistent().Events[0].Count, Is.EqualTo(scheduler.ProcessedEvents));
        }

        [Test]
        public void AlgorithmWithoutSemanticProgressObservationIsRejected_BeforeItCanLoop()
        {
            var probe = new Probe("algorithms", WorldEventPhase.Algorithm, _subject, new List<string>()) { Repeat = true };
            var executor = new OfflineWorldDomainExecutor(_world, new[] { probe },
                new OfflineProviderManifest { RequiredDomains = new[] { "algorithms" } }, _report,
                new ProtectionProbe { Domain = probe, Clock = _world.Clock, Unavailable = true });
            var scheduler = Scheduler(executor);
            executor.Seed(scheduler);
            scheduler.Pump(10, 1000);
            Assert.That(scheduler.FailureReason, Does.Contain("进展观测"));
            Assert.That(probe.Applied, Is.False);
            Assert.That(scheduler.TryCapturePersistent(out _), Is.False);
        }

        [Test]
        public void MissingOriginalResourceBaselinePreventsBeginningOfflineExecution()
        {
            var probe = new Probe("energy", WorldEventPhase.Energy, _subject, new List<string>());
            Assert.Throws<ArgumentException>(() => new OfflineWorldDomainExecutor(_world, new[] { probe },
                new OfflineProviderManifest { RequiredDomains = new[] { "energy" } }, new OfflineSettlementReport("providers", 0, 10)));
            Assert.That(_world.Clock.WorldMilliseconds, Is.Zero);
        }
    }
}
