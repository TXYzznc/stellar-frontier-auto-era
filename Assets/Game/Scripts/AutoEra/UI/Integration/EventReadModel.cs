using System;
using System.Collections.Generic;
using AutoEra.Events;
using AutoEra.World.Identity;

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
        private readonly List<EventJournalRecord> _records = new List<EventJournalRecord>(128);
        private readonly List<UiEventRow> _all = new List<UiEventRow>(128);
        private readonly List<UiEventRow> _machines = new List<UiEventRow>(64);
        private readonly List<UiEventRow> _algorithms = new List<UiEventRow>(32);
        private readonly List<UiEventRow> _energy = new List<UiEventRow>(8);
        private readonly List<UiDetailField> _detail = new List<UiDetailField>(10);
        private EventDomainSnapshot _snapshot;
        private ulong _selected;
        private bool _disposed;

        public EventReadModel(EventJournal journal)
        {
            _journal = journal ?? throw new ArgumentNullException(nameof(journal));
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
                var row = new UiEventRow(
                    record.Sequence,
                    record.Kind == EventKind.Command ? "命令" : "事实",
                    DescribeDomain(record.Domain),
                    record.Action,
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
                    case EventDomain.Resource:
                        // 资源域目前与能源无对应关系（事件分类里没有能源域），因此不进能源列表。
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
        /// 能源历史当前没有数据来源：事件分类（<see cref="EventDomain"/>）只有任务／算法／执行／资源／警报，
        /// **没有能源域**——能源系统本身也还没接入。界面据此说明，而不是拿资源域凑数。
        /// </summary>
        public const string EnergyHistoryUnavailable =
            "能源历史暂不可用：事件分类里没有能源域，能源系统本身也尚未接入；这里不拿资源域冒充能源。";

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

            return new EventReadModel(session.World.Events.Journal);
        }
    }
}
