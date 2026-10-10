using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using AutoEra.Buildings;
using AutoEra.Logistics;
using AutoEra.Save;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Time;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    /// <summary>Atomic file/continuation contracts with fixture input, not a complete gameplay continuation.</summary>
    public sealed class OfflineCheckpointEditModeTests
    {
        private AutoEraWorldSession _world;
        private OfflineSettlementReport _report;
        private OfflineEventScheduler _scheduler;
        private SaveSlotService _slots;
        private SaveSlotWorldSnapshotWriter _writer;
        private string _root;
        private static readonly DateTimeOffset Utc = new DateTimeOffset(2026, 10, 8, 0, 0, 0, TimeSpan.Zero);
        public sealed class FixtureRoot { public long Time; }

        private sealed class Executor : IOfflineEventExecutor
        {
            internal WorldClock Clock;
            internal OfflineSettlementReport Report;
            public bool IsAtCommitBoundary => true;
            public bool IsCurrent(OfflineScheduledEvent item) => true;
            public bool TryAdvanceTo(long time, out string reason) { reason = null; return Clock.TryAdvanceTo(time); }
            public bool TryExecute(OfflineScheduledEvent item, OfflineEventScheduler scheduler, out string reason) => Report.TryRecordEvent(item, out reason);
        }
        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "AutoEraOfflineCheckpoint-" + Guid.NewGuid().ToString("N"));
            _slots = new SaveSlotService(_root, () => Utc.AddDays(30).UtcDateTime);
            _writer = new SaveSlotWorldSnapshotWriter(_slots);
            _world = new AutoEraWorldSessionFactory().Create(0);
            _world.Resources.Configure(new ResourceItemCatalog(new[] {
                new ResourceItemDefinition(ResourceItemCatalog.Ore, CargoItemClass.CommonResource) }));
            _world.IdAllocator.TryAllocate(out var subject);
            _report = new OfflineSettlementReport("original-run", 0, 10);
            _report.TryRefreshResources(_world, true, out _);
            _scheduler = new OfflineEventScheduler(_world.Clock, new Executor { Clock = _world.Clock, Report = _report }, Utc, Utc.AddMilliseconds(10), "original-run");
            _scheduler.Seed(new[] { new OfflineEventSpec { Domain = "fixture", Due = 5, Phase = WorldEventPhase.ResourceAndReward,
                Subject = subject, Generation = 1, Kind = OfflineEventKind.ResourceCommit, Payload = new OfflineEventPayload() } });
        }
        [TearDown]
        public void TearDown()
        {
            _world.Dispose();
            string absolute = Path.GetFullPath(_root);
            string parent = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            Assert.That(absolute.StartsWith(parent, StringComparison.OrdinalIgnoreCase) && Path.GetFileName(absolute).StartsWith("AutoEraOfflineCheckpoint-", StringComparison.Ordinal), Is.True);
            if (Directory.Exists(absolute)) Directory.Delete(absolute, true);
        }
        private WorldSnapshotDocument Root(long? time = null) => new WorldSnapshotDocument(time ?? _world.Clock.WorldMilliseconds, 1, 1, "Offline contract", new[] {
            new WorldSnapshotSection("fixture-root", 1, new FixtureRoot { Time = _world.Clock.WorldMilliseconds }) });
        private OfflineWorldCheckpoint Capture()
        {
            Assert.That(_report.TryRefreshResources(_world, false, out var reason), Is.True, reason);
            if (_scheduler.Completed) Assert.That(_report.TryComplete(_scheduler), Is.True);
            Assert.That(OfflineWorldCheckpoint.TryCapture(Root(), _scheduler, _report, out var checkpoint, out reason), Is.True, reason);
            return checkpoint;
        }
        private LoadedWorldSnapshot Read(SaveSlotRecord record)
        {
            Assert.That(WorldSnapshotCodec.TryRead(record.ContentJson, new Dictionary<string, int> {
                { "fixture-root", 1 }, { "offline", 1 } }, out var file, out var reason), Is.True, reason);
            return file;
        }

        [Test]
        public void InterruptedCheckpointRetainsOriginalRunAndLockedUtc_InTheActualAtomicRecord()
        {
            _scheduler.Pump(1, 1000);
            var checkpoint = Capture();
            Assert.That(_writer.WriteOfflineAsync(0, checkpoint, CancellationToken.None).GetAwaiter().GetResult().Succeeded, Is.True);
            var record = _slots.Read(0).Record;
            Assert.That(record.SavedUtcTicks, Is.EqualTo(Utc.AddMilliseconds(10).UtcDateTime.Ticks));
            Assert.That(record.OfflineSettlementPending, Is.True);
            Assert.That(OfflineWorldCheckpoint.TryRead(Read(record), record.OfflineSettlementPending, record.SavedUtcTicks, out var saved, out var reason), Is.True, reason);
            Assert.That(saved.Scheduler.RunId, Is.EqualTo("original-run"));
            Assert.That(saved.Scheduler.WorldMilliseconds, Is.EqualTo(5));
            Assert.That(saved.Scheduler.Processed, Is.EqualTo(1));
            Assert.That(saved.Report.Events[0].Count, Is.EqualTo(1));
        }

        [Test]
        public void CompletionAndReportConfirmationAreStoredTogether_HistoricalReadDoesNotExecute()
        {
            _scheduler.Pump(10, 1000);
            var completed = Capture();
            Assert.That(completed.Pending, Is.True);
            Assert.That(_report.TryConfirm(), Is.True);
            Assert.That(OfflineWorldCheckpoint.TryCapture(Root(), _scheduler, _report, out var confirmed, out var reason), Is.True, reason);
            Assert.That(confirmed.Pending, Is.False);
            Assert.That(_writer.WriteOfflineAsync(0, confirmed, CancellationToken.None).GetAwaiter().GetResult().Succeeded, Is.True);
            for (int i = 0; i < 10; i++)
            {
                var record = _slots.Read(0).Record;
                Assert.That(OfflineWorldCheckpoint.TryRead(Read(record), record.OfflineSettlementPending, record.SavedUtcTicks, out var saved, out reason), Is.True, reason);
                Assert.That(saved.Report.Completed && saved.Report.Confirmed, Is.True);
            }
            Assert.That(_scheduler.ProcessedEvents, Is.EqualTo(1));
            Assert.That(_world.Clock.WorldMilliseconds, Is.EqualTo(10));
        }

        [TestCase("time")]
        [TestCase("report-events")]
        public void InconsistentBoundaryOrMissingReportReceiptCannotBeWritten(string mutation)
        {
            _scheduler.Pump(1, 1000);
            _report.TryRefreshResources(_world, false, out _);
            var report = _report;
            if (mutation == "report-events")
            {
                var data = report.CapturePersistent();
                data.Events = Array.Empty<OfflineReportSnapshot.EventRow>();
                data.Receipts = Array.Empty<OfflineReportSnapshot.SequenceRange>();
                Assert.That(OfflineSettlementReport.TryRestorePersistent(data, out report, out _), Is.True);
            }
            Assert.That(OfflineWorldCheckpoint.TryCapture(Root(mutation == "time" ? 4 : 5), _scheduler, report, out var result, out _), Is.False);
            Assert.That(result, Is.Null);
            Assert.That(_slots.Read(0).IsSuccess, Is.False);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ChangedOuterPendingOrUtcWatermarkIsRejected(bool pending)
        {
            _scheduler.Pump(1, 1000);
            var checkpoint = Capture();
            _writer.WriteOfflineAsync(0, checkpoint, CancellationToken.None).GetAwaiter().GetResult();
            var record = _slots.Read(0).Record;
            Assert.That(OfflineWorldCheckpoint.TryRead(Read(record), pending ? false : true,
                pending ? record.SavedUtcTicks : record.SavedUtcTicks + 1, out var saved, out _), Is.False);
            Assert.That(saved, Is.Null);
        }

        [Test]
        public void FailedTemporaryWriteLeavesPreviousRunProgressAndReportTogether()
        {
            _scheduler.Pump(1, 1000);
            _writer.WriteOfflineAsync(0, Capture(), CancellationToken.None).GetAwaiter().GetResult();
            Directory.CreateDirectory(_slots.GetSlotPath(0) + ".tmp");
            _scheduler.Pump(10, 1000);
            Assert.That(_writer.WriteOfflineAsync(0, Capture(), CancellationToken.None).GetAwaiter().GetResult().Succeeded, Is.False);
            var old = _slots.Read(0).Record;
            Assert.That(old.WorldTimeMilliseconds, Is.EqualTo(5));
            Assert.That(OfflineWorldCheckpoint.TryRead(Read(old), old.OfflineSettlementPending, old.SavedUtcTicks, out var saved, out var reason), Is.True, reason);
            Assert.That(saved.Scheduler.Completed, Is.False);
            Assert.That(saved.Report.Completed, Is.False);
            Assert.That(saved.Report.Events[0].Count, Is.EqualTo(1));
        }

        [Test]
        public void CorruptPrimaryRecoversAnEntirePreviousCheckpoint_NotMixedCompletionFacts()
        {
            _scheduler.Pump(1, 1000);
            _writer.WriteOfflineAsync(0, Capture(), CancellationToken.None).GetAwaiter().GetResult();
            _scheduler.Pump(10, 1000);
            Capture();
            _report.TryConfirm();
            OfflineWorldCheckpoint.TryCapture(Root(), _scheduler, _report, out var confirmed, out _);
            _writer.WriteOfflineAsync(0, confirmed, CancellationToken.None).GetAwaiter().GetResult();
            File.WriteAllText(_slots.GetSlotPath(0), "corrupt fixture primary");
            var backup = _slots.Read(0);
            Assert.That(backup.IsFromBackup, Is.True);
            Assert.That(OfflineWorldCheckpoint.TryRead(Read(backup.Record), backup.Record.OfflineSettlementPending,
                backup.Record.SavedUtcTicks, out var saved, out var reason), Is.True, reason);
            Assert.That(saved.Scheduler.WorldMilliseconds, Is.EqualTo(5));
            Assert.That(saved.Report.CurrentWorld, Is.EqualTo(5));
            Assert.That(saved.Report.Confirmed, Is.False);
        }

        [Test]
        public void RollbackUtcProducesNoTimeAndKeepsPreviousExitWatermark()
        {
            var report = new OfflineSettlementReport("rollback", 0, 0);
            report.TryRefreshResources(_world, true, out _);
            var scheduler = new OfflineEventScheduler(_world.Clock, new Executor { Clock = _world.Clock, Report = report }, Utc, Utc.AddSeconds(-1), "rollback");
            scheduler.Seed(Array.Empty<OfflineEventSpec>());
            scheduler.Pump(1, 1000);
            Assert.That(report.TryComplete(scheduler), Is.True);
            Assert.That(OfflineWorldCheckpoint.TryCapture(Root(), scheduler, report, out var checkpoint, out var reason), Is.True, reason);
            Assert.That(checkpoint.SavedUtcWatermark, Is.EqualTo(Utc));
            Assert.That(checkpoint.Document.WorldMilliseconds, Is.Zero);
        }
    }
}
