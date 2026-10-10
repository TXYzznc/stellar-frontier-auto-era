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
    public sealed class OfflineSettlementReportEditModeTests
    {
        private AutoEraWorldSession _world;
        private CargoContainer _ground;
        private static readonly DateTimeOffset Utc = new DateTimeOffset(2026, 10, 8, 0, 0, 0, TimeSpan.Zero);

        [SetUp]
        public void SetUp()
        {
            _world = new AutoEraWorldSessionFactory().Create(0);
            _world.Resources.Configure(new ResourceItemCatalog(new[] {
                new ResourceItemDefinition(ResourceItemCatalog.Ore, CargoItemClass.CommonResource) }));
            _ground = _world.Resources.Authority.RegisterContainer(
                new CargoOwner(CargoOwnerKind.WorldFree, Next()), CargoContainerKind.WorldFree, long.MaxValue);
        }

        [TearDown] public void TearDown() => _world.Dispose();
        private PersistentId Next()
        {
            Assert.That(_world.IdAllocator.TryAllocate(out var id), Is.True);
            return id;
        }
        private static OfflineScheduledEvent Event(ulong sequence, string domain = "production") =>
            new OfflineScheduledEvent { Sequence = sequence, Domain = domain, Due = 1,
                Kind = OfflineEventKind.ForestBoundary };
        private static T RoundTrip<T>(T value) where T : class
        {
            string json = WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(0, 100, 1, "Report", new[] {
                new WorldSnapshotSection("report", 1, value) }));
            Assert.That(WorldSnapshotCodec.TryRead(json, new Dictionary<string, int> { { "report", 1 } }, out var file, out var reason), Is.True, reason);
            Assert.That(file.TryReadSection<T>("report", out var result, out reason), Is.True, reason);
            return result;
        }

        [Test]
        public void OutOfOrderAndDuplicateEventSequences_MergeWithoutCountingTwice_AfterRestore()
        {
            var report = new OfflineSettlementReport("original", 0, 10);
            foreach (ulong sequence in new ulong[] { 3, 1, 5, 2, 4, 3 })
                Assert.That(report.TryRecordEvent(Event(sequence), out var reason), Is.True, reason);
            var saved = RoundTrip(report.CapturePersistent());
            Assert.That(saved.Events[0].Count, Is.EqualTo(5));
            Assert.That(saved.Receipts.Length, Is.EqualTo(1));
            Assert.That(saved.Receipts[0].First, Is.EqualTo(1));
            Assert.That(saved.Receipts[0].Last, Is.EqualTo(5));
            Assert.That(OfflineSettlementReport.TryRestorePersistent(saved, out var restored, out var error), Is.True, error);
            Assert.That(restored.TryRecordEvent(Event(4), out error), Is.True, error);
            Assert.That(restored.CapturePersistent().Events[0].Count, Is.EqualTo(5));
            Assert.That(restored.RunId, Is.EqualTo("original"));
        }

        [Test]
        public void ReportResources_UseActualLotsAndWarehouseBalance_ReadingDoesNotCommitOrAllocate()
        {
            var authority = _world.Resources.Authority;
            Assert.That(authority.TryMint(_ground.Owner, ResourceItemCatalog.Ore, 10, out var lot, out var reason), Is.True, reason);
            var report = new OfflineSettlementReport("facts", 0, 10);
            Assert.That(report.TryRefreshResources(_world, true, out reason), Is.True, reason);
            var warehouse = authority.RegisterContainer(new CargoOwner(CargoOwnerKind.Receiver, Next()), CargoContainerKind.Warehouse, 100);
            Assert.That(authority.TryReserve(Next(), Next(), lot.Id, lot.Version, warehouse.Owner, warehouse.Generation, 4, out var token, out reason), Is.True, reason);
            authority.Commit(token, 1, 4);
            Assert.That(authority.TryMint(_ground.Owner, ResourceItemCatalog.Ore, 2, out _, out reason), Is.True, reason);
            _world.Clock.TryAdvanceTo(10);
            Assert.That(authority.TryCapturePersistent(out var before), Is.True);
            Assert.That(report.TryRefreshResources(_world, false, out reason), Is.True, reason);
            for (int i = 0; i < 10; i++) RoundTrip(report.CapturePersistent());
            Assert.That(authority.TryCapturePersistent(out var after), Is.True);
            var row = report.CapturePersistent().Resources[0];
            Assert.That(row.Initial, Is.EqualTo(10));
            Assert.That(row.Current, Is.EqualTo(12));
            Assert.That(authority.Balance(ResourceItemCatalog.Ore), Is.EqualTo(4));
            Assert.That(after.AllocatedThrough, Is.EqualTo(before.AllocatedThrough));
            Assert.That(after.Receipts.Length, Is.EqualTo(before.Receipts.Length));
            Assert.That(after.Production.Length, Is.EqualTo(before.Production.Length));
        }

        [Test]
        public void InitialResourcesMustBeCapturedAtOriginalTime_EmptyBaselineCannotBeRecaptured()
        {
            var report = new OfflineSettlementReport("baseline", 0, 10);
            Assert.That(report.TryRefreshResources(_world, false, out _), Is.False);
            Assert.That(report.TryRefreshResources(_world, true, out _), Is.True);
            Assert.That(report.TryRefreshResources(_world, true, out _), Is.False);
            var late = new OfflineSettlementReport("late", 0, 10);
            _world.Clock.TryAdvanceTo(1);
            Assert.That(late.TryRefreshResources(_world, true, out _), Is.False);
        }

        [Test]
        public void DetachedReportSnapshot_DoesNotChangeRuntimeCountsOrResourceRows()
        {
            var report = new OfflineSettlementReport("detached", 0, 10);
            report.TryRefreshResources(_world, true, out _);
            report.TryRecordEvent(Event(2), out _);
            var first = report.CapturePersistent();
            first.Events[0].Count = 99;
            first.Receipts[0].Last = 100;
            var second = report.CapturePersistent();
            Assert.That(second.Events[0].Count, Is.EqualTo(1));
            Assert.That(second.Receipts[0].Last, Is.EqualTo(2));
        }

        private sealed class ClockOnlyExecutor : IOfflineEventExecutor
        {
            private readonly WorldClock _clock;
            internal ClockOnlyExecutor(WorldClock clock) { _clock = clock; }
            public bool IsAtCommitBoundary => true;
            public bool IsCurrent(OfflineScheduledEvent item) => true;
            public bool TryAdvanceTo(long time, out string reason) { reason = null; return _clock.TryAdvanceTo(time); }
            public bool TryExecute(OfflineScheduledEvent item, OfflineEventScheduler scheduler, out string reason) { reason = null; return true; }
        }
        [Test]
        public void CompletionRequiresMatchingFinishedScheduler_ConfirmationIsReadOnlyAndPersistent()
        {
            var report = new OfflineSettlementReport("complete", 0, 10);
            report.TryRefreshResources(_world, true, out _);
            var scheduler = new OfflineEventScheduler(_world.Clock, new ClockOnlyExecutor(_world.Clock), Utc, Utc.AddMilliseconds(10), "complete");
            scheduler.Seed(Array.Empty<OfflineEventSpec>());
            Assert.That(report.TryConfirm(), Is.False);
            Assert.That(report.TryComplete(scheduler), Is.False);
            scheduler.Pump(1, 1000);
            report.TryRefreshResources(_world, false, out _);
            Assert.That(report.TryComplete(scheduler), Is.True);
            Assert.That(report.TryConfirm(), Is.True);
            Assert.That(report.TryConfirm(), Is.True);
            Assert.That(OfflineSettlementReport.TryRestorePersistent(RoundTrip(report.CapturePersistent()), out var restored, out var reason), Is.True, reason);
            Assert.That(restored.Completed && restored.Confirmed, Is.True);
            Assert.That(restored.TryRefreshResources(_world, false, out _), Is.False);
            Assert.That(_world.Clock.WorldMilliseconds, Is.EqualTo(10));
        }

        [TestCase("counts")]
        [TestCase("overlap")]
        [TestCase("confirmation")]
        [TestCase("baseline")]
        [TestCase("duplicate-resource")]
        public void CorruptReportIsRejected_WithoutPublishingCandidate(string mutation)
        {
            var report = new OfflineSettlementReport("corrupt", 0, 10);
            report.TryRefreshResources(_world, true, out _);
            report.TryRecordEvent(Event(1), out _);
            var data = report.CapturePersistent();
            if (mutation == "counts") data.Events[0].Count = 2;
            if (mutation == "overlap") data.Receipts = new[] { new OfflineReportSnapshot.SequenceRange { First = 1, Last = 1 }, new OfflineReportSnapshot.SequenceRange { First = 1, Last = 1 } };
            if (mutation == "confirmation") data.Confirmed = true;
            if (mutation == "baseline") { data.ResourcesInitialized = false; data.CurrentWorld = 1; }
            if (mutation == "duplicate-resource") data.Resources = new[] { new OfflineReportSnapshot.ResourceRow { Item = "ore" }, new OfflineReportSnapshot.ResourceRow { Item = "ore" } };
            Assert.That(OfflineSettlementReport.TryRestorePersistent(data, out var restored, out _), Is.False);
            Assert.That(restored, Is.Null);
            Assert.That(_world.Clock.WorldMilliseconds, Is.Zero);
        }
    }
}
