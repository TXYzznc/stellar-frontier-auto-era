using System;
using System.Collections.Generic;
using AutoEra.Energy;
using AutoEra.Events;
using AutoEra.Machines;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;

namespace AutoEra.UI
{
    /// <summary>事件域内发生变化的区域。</summary>
    public enum EventDomainSection
    {
        /// <summary>记录列表发生变化。</summary>
        List,

        /// <summary>当前选中记录的详情（追溯）发生变化。</summary>
        Detail,
    }

    /// <summary>记录列表的一行。</summary>
    public readonly struct UiEventRow
    {
        public UiEventRow(ulong sequence, string kind, string domain, string action, string time, string outcome, bool terminal, CorrelationId correlation, PersistentId source)
        {
            Sequence = sequence;
            Kind = kind;
            Domain = domain;
            Action = action;
            Time = time;
            Outcome = outcome;
            Terminal = terminal;
            Correlation = correlation;
            Source = source;
        }

        /// <summary>派发序号：单调递增，且与环形缓冲回绕无关，因此可以作为稳定选中键。</summary>
        public ulong Sequence { get; }

        public string Kind { get; }
        public string Domain { get; }

        /// <summary>动作名（事件生产者给的字符串）。</summary>
        public string Action { get; }

        public string Time { get; }

        /// <summary>终态结果；非终态记录为空。</summary>
        public string Outcome { get; }

        public bool Terminal { get; }

        /// <summary>关联 Id：把它交给日志就能取到整条追溯链。</summary>
        public CorrelationId Correlation { get; }

        public PersistentId Source { get; }

        /// <summary>列表行的显示文本：动作 + 时间，终态记录带结果。</summary>
        public string Describe() =>
            string.IsNullOrEmpty(Outcome) ? Action + " · " + Time : Action + " · " + Time + " · " + Outcome;
    }

    /// <summary>
    /// 事件域的只读快照。
    ///
    /// 记录按**三个展示域**分好组，因为记录阅读界面的三页正是这么分的，而界面不该自己
    /// 去理解 `EventDomain` 的分类语义。分组映射见 <see cref="EventReadModels"/> 的说明。
    /// </summary>
    public readonly struct EventDomainSnapshot
    {
        public EventDomainSnapshot(
            UiDataState state,
            string unavailableReason,
            IReadOnlyList<UiEventRow> allRecords,
            IReadOnlyList<UiEventRow> machineRecords,
            IReadOnlyList<UiEventRow> algorithmRecords,
            IReadOnlyList<UiEventRow> energyRecords,
            IReadOnlyList<UiDetailField> detail,
            ulong selectedSequence)
        {
            State = state;
            UnavailableReason = unavailableReason;
            AllRecords = allRecords;
            MachineRecords = machineRecords;
            AlgorithmRecords = algorithmRecords;
            EnergyRecords = energyRecords;
            Detail = detail;
            SelectedSequence = selectedSequence;
        }

        public UiDataState State { get; }
        public string UnavailableReason { get; }

        /// <summary>全部记录（由旧到新）。枢纽统计这类「看整体历史」的页面用它。</summary>
        public IReadOnlyList<UiEventRow> AllRecords { get; }

        /// <summary>机器历史：任务域与执行域的事实。</summary>
        public IReadOnlyList<UiEventRow> MachineRecords { get; }

        /// <summary>算法历史：算法域的事实。</summary>
        public IReadOnlyList<UiEventRow> AlgorithmRecords { get; }

        /// <summary>能源历史：事件分类里没有能源域，因此本列表当前恒为空（原因由界面说明）。</summary>
        public IReadOnlyList<UiEventRow> EnergyRecords { get; }

        /// <summary>选中记录的追溯详情。</summary>
        public IReadOnlyList<UiDetailField> Detail { get; }

        public ulong SelectedSequence { get; }

        public int Count => AllRecords?.Count ?? 0;
        public bool HasSelection => Detail != null && Detail.Count > 0;

        public static EventDomainSnapshot Unavailable(string reason) =>
            new EventDomainSnapshot(UiDataState.Unavailable, reason, null, null, null, null, null, 0UL);
    }

    /// <summary>
    /// 事件域读取模型：日志记录 + 按关联 Id 取追溯。
    ///
    /// 日志是**追加型环形缓冲**、没有变化事件，所以这里只有显式 <see cref="Refresh"/>——
    /// 与存档域同理，不假装有推送。
    /// </summary>
    public interface IEventReadModel : IDisposable
    {
        EventDomainSnapshot Snapshot { get; }

        event Action<EventDomainSection> Changed;

        /// <summary>重新读取日志（打开界面、或玩家主动刷新时调用）。</summary>
        void Refresh();

        /// <summary>按派发序号选中一条记录，并重建它的追溯详情。</summary>
        bool Select(ulong sequence);

        void ClearSelection();
    }

    /// <summary>事件域不可用时的诚实空实现。</summary>
    internal sealed class UnavailableEventReadModel : IEventReadModel
    {
        public UnavailableEventReadModel(string reason)
        {
            Snapshot = EventDomainSnapshot.Unavailable(reason);
        }

        public EventDomainSnapshot Snapshot { get; }

        public event Action<EventDomainSection> Changed
        {
            add { }
            remove { }
        }

        public void Refresh() { }

        public bool Select(ulong sequence) => false;

        public void ClearSelection() { }

        public void Dispose() { }
    }

    /// <summary>基于 <see cref="EventJournal"/> 的只读实现。</summary>
    internal sealed class EventReadModel : IEventReadModel
    {
        private readonly EventJournal _journal;
        private readonly AutoEraWorldSession _world;
        private readonly InitialRegion _region;

        /// <summary>区域电网：只用于把设施身份翻译成名字，不参与任何结算。</summary>
        private readonly RegionEnergyService _energyService;
        private readonly List<EventJournalRecord> _records = new List<EventJournalRecord>(128);
        private readonly List<UiEventRow> _all = new List<UiEventRow>(128);
        private readonly List<UiEventRow> _machines = new List<UiEventRow>(64);
        private readonly List<UiEventRow> _algorithms = new List<UiEventRow>(32);
        private readonly List<UiEventRow> _energy = new List<UiEventRow>(8);
        private readonly List<UiDetailField> _detail = new List<UiDetailField>(10);
        private EventDomainSnapshot _snapshot;
        private ulong _selected;
        private bool _disposed;

        /// <summary>
        /// 世界会话与区域只用于**把 Id 翻译成名字**（以及算活跃事件的持续时长）。
        /// 传 null 也能工作——那时对象名显示为「—」而不是编一个名字。
        /// </summary>
        public EventReadModel(EventJournal journal, AutoEraWorldSession world = null,
            InitialRegion region = null, RegionEnergyService energy = null)
        {
            _journal = journal ?? throw new ArgumentNullException(nameof(journal));
            _world = world;
            _region = region;
            _energyService = energy;
            Refresh();
        }

        public EventDomainSnapshot Snapshot => _snapshot;

        public event Action<EventDomainSection> Changed;

        public void Refresh()
        {
            if (_disposed)
            {
                return;
            }

            _records.Clear();
            _journal.CopyRecent(_records);

            _all.Clear();
            _machines.Clear();
            _algorithms.Clear();
            _energy.Clear();
            for (int i = 0; i < _records.Count; i++)
            {
                EventJournalRecord record = _records[i];
                // 能源事件的「动作」在日志里是稳定的种类名（生产侧与读取侧共用
                // EnergyEventText 的常量）；列表行补上对象名，否则多台机器时这一行读不出是谁。
                string action = record.Domain == EventDomain.Energy
                    ? DescribeEnergyAction(record)
                    : record.Action;
                var row = new UiEventRow(
                    record.Sequence,
                    record.Kind == EventKind.Command ? "命令" : "事实",
                    DescribeDomain(record.Domain),
                    action,
                    AutoEraUiFormat.WorldTime(record.WorldMilliseconds),
                    DescribeOutcome(record.Terminal, record.Outcome),
                    record.Terminal,
                    record.Correlation,
                    record.Source);
                _all.Add(row);

                switch (record.Domain)
                {
                    case EventDomain.Algorithm:
                        _algorithms.Add(row);
                        break;
                    case EventDomain.Task:
                    case EventDomain.Effector:
                        // 机器做的事以任务与执行两类事实记账；机器历史的两个来源都收在这里。
                        _machines.Add(row);
                        break;
                    case EventDomain.Energy:
                        // 电网自己产生的离散事件（缺电停机／恢复、电量耗尽／恢复、燃料耗尽、供电缺口）。
                        _energy.Add(row);
                        break;
                    case EventDomain.Resource:
                        // 资源域与能源域是两件事：资源产出不得冒充能源事件。
                        break;
                    default:
                        break;
                }
            }

            if (_selected != 0 && !ContainsSequence(_records, _selected))
            {
                _selected = 0;
            }

            RebuildDetail();
            PublishSnapshot(EventDomainSection.List);
        }

        public bool Select(ulong sequence)
        {
            if (_disposed || sequence == 0 || !ContainsSequence(_records, sequence))
            {
                return false;
            }

            if (_selected == sequence)
            {
                return true;
            }

            _selected = sequence;
            RebuildDetail();
            PublishSnapshot(EventDomainSection.Detail);
            return true;
        }

        public void ClearSelection()
        {
            if (_disposed || _selected == 0)
            {
                return;
            }

            _selected = 0;
            RebuildDetail();
            PublishSnapshot(EventDomainSection.Detail);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Changed = null;
            _records.Clear();
            _all.Clear();
            _machines.Clear();
            _algorithms.Clear();
            _energy.Clear();
            _detail.Clear();
        }

        /// <summary>重建快照并通知；每次内容变化都必须走这里，否则页面读到的是上一份快照。</summary>
        private void PublishSnapshot(EventDomainSection section)
        {
            _snapshot = new EventDomainSnapshot(
                _records.Count == 0 ? UiDataState.Empty : UiDataState.Ready,
                null,
                _all.ToArray(),
                _machines.ToArray(),
                _algorithms.ToArray(),
                _energy.ToArray(),
                _detail.ToArray(),
                _selected);

            Changed?.Invoke(section);
        }

        private void RebuildDetail()
        {
            _detail.Clear();
            if (_selected == 0)
            {
                return;
            }

            EventJournalRecord record = default;
            bool found = false;
            for (int i = 0; i < _records.Count; i++)
            {
                if (_records[i].Sequence == _selected)
                {
                    record = _records[i];
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                return;
            }

            _detail.Add(new UiDetailField("序号", record.Sequence.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            _detail.Add(new UiDetailField("类别", record.Kind == EventKind.Command ? "命令" : "事实"));
            _detail.Add(new UiDetailField("域", DescribeDomain(record.Domain)));
            _detail.Add(new UiDetailField("动作", record.Action));
            _detail.Add(new UiDetailField("世界时间", AutoEraUiFormat.WorldTime(record.WorldMilliseconds)));
            _detail.Add(new UiDetailField("终态", record.Terminal ? "是" : "否"));
            _detail.Add(new UiDetailField("结果", DescribeOutcome(record.Terminal, record.Outcome)));
            _detail.Add(new UiDetailField("来源对象", record.Source.IsValid
                ? AutoEraUiFormat.Count((int)Math.Min(record.Source.Value, int.MaxValue))
                : AutoEraUiFormat.Missing));

            // 追溯链是日志本就能给的东西：一条命令 + 它引发的事实 + 终态。
            if (_journal.TryGetTrace(record.Correlation, out EventTrace trace))
            {
                _detail.Add(new UiDetailField("追溯", trace.Resolved
                    ? "已结案 · 事实 " + AutoEraUiFormat.Count(trace.FactCount)
                    : "处理中 · 事实 " + AutoEraUiFormat.Count(trace.FactCount)));
            }
            else
            {
                _detail.Add(new UiDetailField("追溯", "该记录没有可追溯的关联链"));
            }

            AppendEnergyDetail(record);
        }

        // ---------------------------------------------------------------- 能源事件

        /// <summary>
        /// 能源事件的详情（规格 15「能源停机记录」的事件详情区：时间、对象、持续时长、原因、
        /// 影响、活跃或已恢复）。
        ///
        /// 「活跃或已恢复」和「持续时长」都不能从单条记录读出来——日志里存的是一次状态跨越，
        /// 所以这里把**同一主体、同一事件种类的停供记录**与它后面第一条对应的恢复记录配对。
        /// 配不到恢复记录就是「活跃」，时长按世界时钟算到当前时刻。
        /// 燃料耗尽与供电缺口没有恢复事件（补充燃料、扩容都还没实现），因此如实恒为「活跃」。
        /// </summary>
        private void AppendEnergyDetail(EventJournalRecord record)
        {
            EnergyEventKind? parsed = EnergyEventText.Parse(record.Action);
            if (!parsed.HasValue)
            {
                return;
            }

            EnergyEventKind kind = parsed.Value;
            _detail.Add(new UiDetailField("对象", DescribeSubject(record.Source)));

            if (EnergyEventText.IsRecovery(kind))
            {
                long start = FindPrecedingStop(record, kind);
                _detail.Add(new UiDetailField("状态", "已恢复"));
                _detail.Add(new UiDetailField("持续时长", start >= 0
                    ? AutoEraUiFormat.Duration(record.WorldMilliseconds - start)
                    : AutoEraUiFormat.Missing + "（这条记录之前的那次停供已经不在日志里）"));
            }
            else
            {
                long end = EnergyEventText.RecoveryOf(kind).HasValue ? FindFollowingRecovery(record, kind) : -1L;
                _detail.Add(new UiDetailField("状态", end >= 0 ? "已恢复" : "活跃"));
                _detail.Add(new UiDetailField("持续时长", end >= 0
                    ? AutoEraUiFormat.Duration(end - record.WorldMilliseconds)
                    : AutoEraUiFormat.Duration(NowMilliseconds(record) - record.WorldMilliseconds) + "（至今，仍在持续）"));
            }

            _detail.Add(new UiDetailField("原因", EnergyEventText.Reason(kind)));
            _detail.Add(new UiDetailField("影响", EnergyEventText.Impact(kind)));
        }

        /// <summary>列表行上的能源事件：种类 + 对象名（名字取不到时只留种类，不编造）。</summary>
        private string DescribeEnergyAction(EventJournalRecord record)
        {
            string name = ResolveObjectName(record.Source);
            return string.IsNullOrEmpty(name) ? record.Action : record.Action + " · " + name;
        }

        private string DescribeSubject(PersistentId id)
        {
            if (!id.IsValid)
            {
                // 电量与供电缺口是区域级事件：没有单一设施主体时如实说是合计口径。
                return "本区域储能／电网（合计）";
            }

            return ResolveObjectName(id) ?? "对象已不在区域里（身份 " + AutoEraUiFormat.Count((int)Math.Min(id.Value, int.MaxValue)) + "）";
        }

        /// <summary>机器取花名册名；其次区域对象名；再次是场景声明的设施名。都取不到返回 null。</summary>
        private string ResolveObjectName(PersistentId id)
        {
            if (!id.IsValid) return null;

            if (_world != null && _world.Machines != null &&
                _world.Machines.TryGet(id, out MachineInstance machine))
            {
                return machine.Name;
            }

            if (_region != null && _region.IsActive && _region.TryGet(id, out RegionObject model) &&
                !string.IsNullOrEmpty(model.Name))
            {
                return model.Name;
            }

            if (_energyService != null)
            {
                IReadOnlyList<RegionEnergyFacility> facilities = _energyService.Facilities;
                for (int i = 0; i < facilities.Count; i++)
                {
                    if (facilities[i] != null && facilities[i].ObjectId == id) return facilities[i].DisplayName;
                }
            }

            return null;
        }

        /// <summary>停供事件之后，同一主体上第一条对应的恢复记录；没有则 -1。</summary>
        private long FindFollowingRecovery(EventJournalRecord stop, EnergyEventKind kind)
        {
            EnergyEventKind recovery = EnergyEventText.RecoveryOf(kind) ?? kind;
            for (int i = 0; i < _records.Count; i++)
            {
                EventJournalRecord candidate = _records[i];
                if (candidate.Sequence <= stop.Sequence) continue;
                if (candidate.Domain != EventDomain.Energy) continue;
                if (candidate.Source != stop.Source) continue;
                if (EnergyEventText.Parse(candidate.Action) != recovery) continue;
                return candidate.WorldMilliseconds;
            }

            return -1L;
        }

        /// <summary>恢复事件之前，同一主体上最后一次停供记录的时间；没有则 -1。</summary>
        private long FindPrecedingStop(EventJournalRecord recovery, EnergyEventKind kind)
        {
            EnergyEventKind stop = EnergyEventText.StopOf(kind) ?? kind;
            long found = -1L;
            for (int i = 0; i < _records.Count; i++)
            {
                EventJournalRecord candidate = _records[i];
                if (candidate.Sequence >= recovery.Sequence) break;
                if (candidate.Domain != EventDomain.Energy) continue;
                if (candidate.Source != recovery.Source) continue;
                if (EnergyEventText.Parse(candidate.Action) != stop) continue;
                found = candidate.WorldMilliseconds;
            }

            return found;
        }

        /// <summary>「现在」：优先用世界时钟；没有会话（单元测试）时退回到最新一条记录的时间。</summary>
        private long NowMilliseconds(EventJournalRecord record)
        {
            if (_world != null && _world.IsActive) return _world.Clock.WorldMilliseconds;

            long latest = record.WorldMilliseconds;
            for (int i = 0; i < _records.Count; i++)
            {
                if (_records[i].WorldMilliseconds > latest) latest = _records[i].WorldMilliseconds;
            }

            return latest;
        }

        private static bool ContainsSequence(List<EventJournalRecord> records, ulong sequence)
        {
            for (int i = 0; i < records.Count; i++)
            {
                if (records[i].Sequence == sequence)
                {
                    return true;
                }
            }

            return false;
        }

        private static string DescribeDomain(EventDomain domain)
        {
            switch (domain)
            {
                case EventDomain.Task: return "任务";
                case EventDomain.Algorithm: return "算法";
                case EventDomain.Effector: return "执行";
                case EventDomain.Resource: return "资源";
                case EventDomain.Alert: return "警报";
                case EventDomain.Energy: return "能源";
                default: return domain.ToString();
            }
        }

        private static string DescribeOutcome(bool terminal, EventOutcome outcome)
        {
            if (!terminal || outcome == EventOutcome.None)
            {
                return string.Empty;
            }

            switch (outcome)
            {
                case EventOutcome.Succeeded: return "成功";
                case EventOutcome.Failed: return "失败";
                case EventOutcome.Cancelled: return "已取消";
                default: return outcome.ToString();
            }
        }
    }

    /// <summary>记录阅读界面的统一入口。</summary>
    public static class EventReadModels
    {
        /// <summary>
        /// 能源历史**已经接上真实数据**：能源域由区域电网在每次结算后写入离散事件
        /// （缺电停机／恢复、电量耗尽／恢复、燃料耗尽、供电缺口）。
        /// 这一句是「区域里还没有发生过任何能源事件」时的说明，不是「没有数据来源」。
        /// </summary>
        public const string EnergyHistoryEmpty =
            "这个区域还没有能源事件：缺电停机、电量耗尽、燃料耗尽这类状态跨越发生后会记在这里。";

        public static IEventReadModel Create(AutoEraUiSession session)
        {
            if (session == null)
            {
                return new UnavailableEventReadModel("没有界面会话：记录不可用。");
            }

            if (!session.HasWorld)
            {
                return new UnavailableEventReadModel("记录属于某个世界的日志，请先从主菜单进入。");
            }

            if (session.World.Events == null)
            {
                return new UnavailableEventReadModel("事件服务未接入：本页无法读取记录。");
            }

            // 世界会话与区域一起传进去：它们只用于把记录里的对象 Id 翻译成名字，
            // 以及算活跃事件「至今持续了多久」。缺了也能工作，只是名字显示为占位符。
            return new EventReadModel(session.World.Events.Journal, session.World,
                session.HasRegion ? session.Region : null, session.RegionEnergy);
        }
    }
}
