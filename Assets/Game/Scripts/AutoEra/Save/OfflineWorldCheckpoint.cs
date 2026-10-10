using System;
using System.Collections.Generic;
using AutoEra.World.Time;

namespace AutoEra.Save
{
    /// <summary>Detached write request; actual domains must capture the root at this same complete boundary.</summary>
    public sealed class OfflineWorldCheckpoint
    {
        public const string SectionName = "offline";
        public WorldSnapshotDocument Document { get; }
        public DateTimeOffset SavedUtcWatermark { get; }
        public bool Pending { get; }

        private OfflineWorldCheckpoint(WorldSnapshotDocument document, DateTimeOffset watermark, bool pending)
        { Document = document; SavedUtcWatermark = watermark; Pending = pending; }

        public static bool TryCapture(WorldSnapshotDocument root, OfflineEventScheduler scheduler,
            OfflineSettlementReport report, out OfflineWorldCheckpoint checkpoint, out string reason)
        {
            checkpoint = null;
            reason = "离线世界、进度和报告尚未到达同一完整边界";
            if (root == null || scheduler == null || report == null || !scheduler.TryCapturePersistent(out var events)) return false;
            var facts = report.CapturePersistent();
            if (!TryValidate(events, facts, root.WorldMilliseconds, out reason)) return false;
            var sections = new List<WorldSnapshotSection>();
            foreach (var section in root.Sections)
            {
                if (section.Name == SectionName) { reason = "原世界快照已包含离线段"; return false; }
                sections.Add(section);
            }
            sections.Add(new WorldSnapshotSection(SectionName, 1, new OfflineContinuationSnapshot { Scheduler = events, Report = facts }));
            var document = new WorldSnapshotDocument(root.WorldMilliseconds, root.AllocatedThrough, root.Revision, root.Summary, sections);
            long watermark = Math.Max(events.SavedUtcTicks, events.TargetUtcTicks);
            checkpoint = new OfflineWorldCheckpoint(document, new DateTimeOffset(watermark, TimeSpan.Zero), !events.Completed || !facts.Confirmed);
            reason = null;
            return true;
        }

        public static bool TryRead(LoadedWorldSnapshot world, bool outerPending, long outerSavedUtcTicks,
            out OfflineContinuationSnapshot state, out string reason)
        {
            state = null;
            reason = "离线检查点缺失或外层状态不一致";
            if (world == null || !world.TryReadSection<OfflineContinuationSnapshot>(SectionName, out var data, out reason) || data.Version != 1 ||
                !TryValidate(data.Scheduler, data.Report, world.WorldMilliseconds, out reason)) return false;
            if (outerPending != (!data.Scheduler.Completed || !data.Report.Confirmed) ||
                outerSavedUtcTicks != Math.Max(data.Scheduler.SavedUtcTicks, data.Scheduler.TargetUtcTicks))
            { reason = "离线外层标记或原锁定UTC水位不一致"; return false; }
            state = data;
            reason = null;
            return true;
        }

        private static bool TryValidate(OfflineEventSnapshot events, OfflineReportSnapshot facts, long worldTime, out string reason)
        {
            reason = "离线进度与报告身份或完成状态不一致";
            if (events == null || facts == null || events.Version != 1 || !facts.ResourcesInitialized ||
                events.RunId != facts.RunId || events.InitialWorldMilliseconds != facts.InitialWorld ||
                events.WorldMilliseconds != worldTime || facts.CurrentWorld != worldTime || events.TargetWorldMilliseconds != facts.TargetWorld ||
                events.Completed != facts.Completed || !OfflineSettlementReport.TryRestorePersistent(facts, out _, out reason)) return false;
            // Validate scheduler queue identity using a clock-only validator, never running an event or a domain.
            var clock = new WorldClock(worldTime);
            if (!OfflineEventScheduler.TryRestorePersistent(events, clock, new ValidationExecutor(clock), out _, out reason)) return false;
            ulong count = 0;
            try { foreach (var row in facts.Events) count = checked(count + row.Count); }
            catch (OverflowException) { reason = "离线报告计数溢出"; return false; }
            if (count != events.Processed) { reason = "离线报告缺少已处理事件记录"; return false; }
            reason = null;
            return true;
        }

        private sealed class ValidationExecutor : IOfflineEventExecutor
        {
            private readonly WorldClock _clock;
            internal ValidationExecutor(WorldClock clock) { _clock = clock; }
            public bool IsAtCommitBoundary => true;
            public bool IsCurrent(OfflineScheduledEvent item) => true;
            public bool TryAdvanceTo(long time, out string reason) { reason = null; return _clock.TryAdvanceTo(time); }
            public bool TryExecute(OfflineScheduledEvent item, OfflineEventScheduler scheduler, out string reason)
            { reason = "检查点验证器不得执行领域事件"; return false; }
        }
    }
}
