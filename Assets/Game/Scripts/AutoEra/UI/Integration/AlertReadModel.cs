using System;
using System.Collections.Generic;
using AutoEra.Alerts;

namespace AutoEra.UI
{
    /// <summary>警报列表的一行（规格 15-警报列表与详情：「活跃／已恢复；等级；已读／未读；来源；首次／最近时间；次数」）。</summary>
    public readonly struct UiAlertRow
    {
        public UiAlertRow(int id, AlertKind kind, AlertSeverity severity, AlertState state, string sourceName,
            int count, long firstMilliseconds, long lastMilliseconds, long recoveredMilliseconds, bool read)
        {
            Id = id;
            Kind = kind;
            Severity = severity;
            State = state;
            SourceName = sourceName;
            Count = count;
            FirstMilliseconds = firstMilliseconds;
            LastMilliseconds = lastMilliseconds;
            RecoveredMilliseconds = recoveredMilliseconds;
            Read = read;
        }

        /// <summary>账本里的稳定标识；界面用它选中与标记已读。</summary>
        public int Id { get; }

        public AlertKind Kind { get; }
        public AlertSeverity Severity { get; }
        public AlertState State { get; }

        /// <summary>来源对象名；取不到名字时为 null（界面显示占位符，不编名字）。</summary>
        public string SourceName { get; }

        public int Count { get; }
        public long FirstMilliseconds { get; }
        public long LastMilliseconds { get; }
        public long RecoveredMilliseconds { get; }
        public bool Read { get; }

        public bool IsActive => State == AlertState.Active;

        /// <summary>列表行的标题：等级 · 种类 · 来源。</summary>
        public string Title => AutoEraUiFormat.AlertSeverity(Severity) + " · " + AlertCatalog.Label(Kind)
            + (string.IsNullOrEmpty(SourceName) ? string.Empty : " · " + SourceName);

        /// <summary>列表行的副标题：状态、次数、首发与最近时间。</summary>
        public string Describe()
        {
            string when = IsActive
                ? "最近 " + AutoEraUiFormat.WorldTime(LastMilliseconds)
                : "恢复于 " + AutoEraUiFormat.WorldTime(RecoveredMilliseconds);

            return (IsActive ? "活跃" : "已恢复")
                + (Read ? " · 已读" : " · 未读")
                + " · 发生 " + AutoEraUiFormat.Count(Count) + " 次"
                + " · 首次 " + AutoEraUiFormat.WorldTime(FirstMilliseconds)
                + " · " + when;
        }
    }

    /// <summary>警报域的只读快照。</summary>
    public readonly struct AlertDomainSnapshot
    {
        public AlertDomainSnapshot(UiDataState state, string reason, IReadOnlyList<UiAlertRow> alerts,
            int selectedId, int activeCount, int unreadCount, AlertSeverity? highestActive)
        {
            State = state;
            Reason = reason;
            Alerts = alerts ?? Array.Empty<UiAlertRow>();
            SelectedId = selectedId;
            ActiveCount = activeCount;
            UnreadCount = unreadCount;
            HighestActive = highestActive;
        }

        public UiDataState State { get; }
        public string Reason { get; }
        public IReadOnlyList<UiAlertRow> Alerts { get; }

        /// <summary>当前选中警报的标识；没有选中时为 0。</summary>
        public int SelectedId { get; }

        public int ActiveCount { get; }
        public int UnreadCount { get; }

        /// <summary>活跃警报里的最高等级；没有活跃警报时为 null。</summary>
        public AlertSeverity? HighestActive { get; }

        public bool HasSelection => SelectedId != 0;

        public UiAlertRow Selected
        {
            get
            {
                for (int i = 0; i < Alerts.Count; i++)
                {
                    if (Alerts[i].Id == SelectedId) return Alerts[i];
                }

                return default;
            }
        }

        public static AlertDomainSnapshot Unavailable(string reason) =>
            new AlertDomainSnapshot(UiDataState.Unavailable, reason, null, 0, 0, 0, null);
    }

    /// <summary>
    /// 警报页的读取与唯一一个写入口。
    ///
    /// 写入口只有「标记已读」，而且它**只改阅读状态**——规格明确「无手动清除故障或重置真实状态按钮」。
    /// 所以这个接口里没有 Resolve：警报的恢复由领域真值决定（见 `RegionAlertMonitor`），
    /// 界面既不能制造恢复，也不能清除一条还存在的问题。
    /// </summary>
    public interface IAlertReadModel : IDisposable
    {
        AlertDomainSnapshot Snapshot { get; }

        event Action Changed;

        void Refresh();

        /// <summary>选中一条警报；越界返回 false。</summary>
        bool Select(int alertId);

        void ClearSelection();

        /// <summary>标记已读；找不到或已经已读返回 false。</summary>
        bool MarkRead(int alertId);
    }

    /// <summary>没有警报账本时的诚实空实现：只报 Unavailable，不伪造「一切正常」。</summary>
    internal sealed class UnavailableAlertReadModel : IAlertReadModel
    {
        public UnavailableAlertReadModel(string reason)
        {
            Snapshot = AlertDomainSnapshot.Unavailable(reason);
        }

        public AlertDomainSnapshot Snapshot { get; }

        public event Action Changed
        {
            add { }
            remove { }
        }

        public void Refresh() { }
        public bool Select(int alertId) => false;
        public void ClearSelection() { }
        public bool MarkRead(int alertId) => false;
        public void Dispose() { }
    }

    /// <summary>基于区域警报账本的只读实现（写入口只有标记已读）。</summary>
    internal sealed class RegionAlertReadModel : IAlertReadModel
    {
        private readonly AutoEraAlertService _alerts;
        private readonly Func<World.Identity.PersistentId, string> _resolveName;
        private readonly List<AlertEntry> _entries = new List<AlertEntry>(32);
        private readonly List<UiAlertRow> _rows = new List<UiAlertRow>(32);
        private AlertDomainSnapshot _snapshot;
        private int _selectedId;
        private bool _disposed;

        public RegionAlertReadModel(AutoEraAlertService alerts, Func<World.Identity.PersistentId, string> resolveName)
        {
            _alerts = alerts ?? throw new ArgumentNullException(nameof(alerts));
            _resolveName = resolveName;
            _alerts.Changed += OnChanged;
            Refresh();
        }

        public AlertDomainSnapshot Snapshot => _snapshot;

        public event Action Changed;

        public void Refresh()
        {
            if (_disposed) return;

            _entries.Clear();
            _alerts.CopyInto(_entries);

            _rows.Clear();
            for (int i = 0; i < _entries.Count; i++)
            {
                AlertEntry entry = _entries[i];
                _rows.Add(new UiAlertRow(entry.Id, entry.Kind, entry.Severity, entry.State,
                    _resolveName != null ? _resolveName(entry.Source) : null,
                    entry.Count, entry.FirstMilliseconds, entry.LastMilliseconds, entry.RecoveredMilliseconds,
                    entry.Read));
            }

            if (_selectedId != 0 && !ContainsId(_selectedId)) _selectedId = 0;

            // 快照必须整体重建：它是值类型，内部持有数组副本，只发事件不换快照会让页面读到旧数据。
            _snapshot = new AlertDomainSnapshot(UiDataState.Ready, null, _rows.ToArray(), _selectedId,
                _alerts.ActiveCount, _alerts.UnreadCount, _alerts.HighestActiveSeverity);
            Changed?.Invoke();
        }

        public bool Select(int alertId)
        {
            if (_disposed || alertId == 0 || !ContainsId(alertId)) return false;
            if (_selectedId == alertId) return true;

            _selectedId = alertId;
            Refresh();
            return true;
        }

        public void ClearSelection()
        {
            if (_disposed || _selectedId == 0) return;
            _selectedId = 0;
            Refresh();
        }

        public bool MarkRead(int alertId)
        {
            if (_disposed) return false;

            // 账本自己会发 Changed；如果没变化（找不到或已读），这里补一次刷新没有意义，
            // 所以直接返回——不为了让界面「看起来响应了」去写一次假状态。
            return _alerts.MarkRead(alertId);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _alerts.Changed -= OnChanged;
            Changed = null;
            _entries.Clear();
            _rows.Clear();
        }

        private void OnChanged() => Refresh();

        private bool ContainsId(int alertId)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Id == alertId) return true;
            }

            return false;
        }
    }

    /// <summary>
    /// 警报详情字段（规格 15：原因；影响；来源与目标；真实恢复条件；相关记录）。
    ///
    /// 放在读取层而不是页面里：详情要说的每一句都来自 <see cref="AlertCatalog"/>，
    /// 而「什么时候恢复、恢复后持续了多久」要由账本的状态算——页面只负责显示，
    /// 自己拼句子就会与列表行、HUD 摘要说出三种不同的版本。
    /// </summary>
    public static class AlertDetails
    {
        /// <summary>把一条警报展开成详情字段；不清空目标。</summary>
        public static void Build(UiAlertRow row, List<UiDetailField> into)
        {
            if (into == null) return;

            into.Add(new UiDetailField("等级", AutoEraUiFormat.AlertSeverity(row.Severity)));
            into.Add(new UiDetailField("状态", row.IsActive ? "活跃（问题仍然存在）" : "已恢复（转历史）"));
            into.Add(new UiDetailField("来源", string.IsNullOrEmpty(row.SourceName) ? AutoEraUiFormat.Missing : row.SourceName));
            into.Add(new UiDetailField("目标", AlertCatalog.Target(row.Kind)));
            into.Add(new UiDetailField("触发原因", AlertCatalog.Reason(row.Kind)));
            into.Add(new UiDetailField("影响", AlertCatalog.Impact(row.Kind)));
            into.Add(new UiDetailField("真实恢复条件", AlertCatalog.RecoveryCondition(row.Kind)
                + "（本页没有清除按钮：把问题解决掉，警报会自己消失）"));
            into.Add(new UiDetailField("首次发生", AutoEraUiFormat.WorldTime(row.FirstMilliseconds)));
            into.Add(new UiDetailField("最近发生", AutoEraUiFormat.WorldTime(row.LastMilliseconds)));
            into.Add(new UiDetailField("恢复时间", row.IsActive
                ? "尚未恢复"
                : AutoEraUiFormat.WorldTime(row.RecoveredMilliseconds)));
            into.Add(new UiDetailField("发生次数", AutoEraUiFormat.Count(row.Count) + " 次（恢复后再次发生计一次）"));
            into.Add(new UiDetailField("阅读状态", row.Read ? "已读" : "未读"));
        }
    }

    /// <summary>警报页的入口：从打开参数里的会话取区域警报账本。</summary>
    public static class AlertReadModels
    {
        /// <summary>没有警报账本时的可展示原因（缺会话／不在世界里／区域未就绪各说各的）。</summary>
        public const string RegionNotReadyReason =
            "当前区域还没有警报账本：警报属于区域，进入区域后才会开始记账。";

        /// <summary>建立读模型。会话或账本缺失时返回诚实空实现。</summary>
        public static IAlertReadModel Create(AutoEraUiSession session)
        {
            if (session == null)
            {
                return new UnavailableAlertReadModel("没有界面会话：警报不可用。");
            }

            if (!session.HasWorld)
            {
                return new UnavailableAlertReadModel("当前不在世界里：警报属于某个区域的运行状态。");
            }

            if (!session.HasRegionAlerts)
            {
                return new UnavailableAlertReadModel(RegionNotReadyReason);
            }

            // 名字解析交给会话里的区域与花名册：这与事件域的做法一致（记录里只有身份，没有名字）。
            return new RegionAlertReadModel(session.RegionAlerts, id => ResolveName(session, id));
        }

        /// <summary>把警报来源身份翻成名字：机器优先，其次是区域对象。</summary>
        private static string ResolveName(AutoEraUiSession session, World.Identity.PersistentId id)
        {
            if (!id.IsValid) return "本区域储能／电网（合计）";

            if (session.World != null && session.World.Machines != null &&
                session.World.Machines.TryGet(id, out Machines.MachineInstance machine))
            {
                return machine.Name;
            }

            return null;
        }
    }
}
