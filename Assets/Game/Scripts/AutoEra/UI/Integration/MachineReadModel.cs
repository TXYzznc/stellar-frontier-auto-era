using System;
using System.Collections.Generic;
using System.Globalization;
using AutoEra.Machines;
using AutoEra.World.Identity;

namespace AutoEra.UI
{
    /// <summary>机器域内发生变化的区域，使页面只刷新对应栏位，而不是整页重刷。</summary>
    public enum MachineDomainSection
    {
        /// <summary>索引列表发生变化（增删机、状态变化）。</summary>
        List,

        /// <summary>当前选中对象的详情发生变化。</summary>
        Detail,
    }

    /// <summary>索引栏的一行：稳定身份 + 名称 + 状态摘要 + 部署状态。</summary>
    public readonly struct UiMachineRow
    {
        public UiMachineRow(PersistentId id, string name, string status, bool deployed)
        {
            Id = id;
            Name = name;
            Status = status;
            Deployed = deployed;
        }

        public PersistentId Id { get; }
        public string Name { get; }
        public string Status { get; }

        /// <summary>
        /// 是否已部署到世界。机器库按它分成「库中机器」与「已部署机器」两页，
        /// 而这两页看的是同一个花名册，所以部署状态必须由行携带，而不是让页面各自去查领域对象。
        /// </summary>
        public bool Deployed { get; }
    }

    /// <summary>详情栏的一行：字段名 + 值，对上契约为 RowLabel / RowValue 两个文本。</summary>
    public readonly struct UiDetailField
    {
        public UiDetailField(string label, string value)
        {
            Label = label;
            Value = value;
        }

        public string Label { get; }
        public string Value { get; }
    }

    /// <summary>
    /// 机器域的只读快照。界面只读它，不接触 <see cref="MachineRoster"/> 的内部结构。
    ///
    /// <see cref="State"/> 为 <see cref="UiDataState.Unavailable"/> 时，
    /// <see cref="UnavailableReason"/> 必须给出可展示的原因。
    /// </summary>
    public readonly struct MachineDomainSnapshot
    {
        public MachineDomainSnapshot(
            UiDataState state,
            string unavailableReason,
            IReadOnlyList<UiMachineRow> machines,
            IReadOnlyList<UiDetailField> detail)
        {
            State = state;
            UnavailableReason = unavailableReason;
            Machines = machines;
            Detail = detail;
        }

        public UiDataState State { get; }
        public string UnavailableReason { get; }
        public IReadOnlyList<UiMachineRow> Machines { get; }
        public IReadOnlyList<UiDetailField> Detail { get; }

        public int Count => Machines == null ? 0 : Machines.Count;
        public bool HasSelection => Detail != null && Detail.Count > 0;

        public static MachineDomainSnapshot Unavailable(string reason) =>
            new MachineDomainSnapshot(UiDataState.Unavailable, reason, null, null);
    }

    /// <summary>
    /// 机器域读取模型：快照 + 变化订阅 + 选中。
    ///
    /// 它存在的理由不是「多包一层」，而是三件界面自己做不了的事：把领域变化聚合成
    /// 「列表变了 / 详情变了」、只暴露页面需要的字段、以及在领域尚未接入时给出
    /// Unavailable 而不是抛异常。它被多个页面复用（HUD 机器页、机器库、机器整备、
    /// 选择器、本页），因此值得单独存在。
    /// </summary>
    public interface IMachineReadModel : IDisposable
    {
        MachineDomainSnapshot Snapshot { get; }

        /// <summary>按区域通知变化，页面只刷对应栏位。</summary>
        event Action<MachineDomainSection> Changed;

        PersistentId SelectedId { get; }

        bool Select(PersistentId id);

        void ClearSelection();
    }

    /// <summary>领域未接入／无世界会话时的诚实空实现：只报 Unavailable，不伪造数据。</summary>
    internal sealed class UnavailableMachineReadModel : IMachineReadModel
    {
        public UnavailableMachineReadModel(string reason)
        {
            Snapshot = MachineDomainSnapshot.Unavailable(reason);
        }

        public MachineDomainSnapshot Snapshot { get; }

        public event Action<MachineDomainSection> Changed
        {
            add { }
            remove { }
        }

        public PersistentId SelectedId => PersistentId.Invalid;

        public bool Select(PersistentId id) => false;

        public void ClearSelection() { }

        public void Dispose() { }
    }

    /// <summary>基于 <see cref="MachineRoster"/> 的只读实现。持有期间订阅领域变化，释放时退订。</summary>
    internal sealed class MachineReadModel : IMachineReadModel
    {
        private readonly MachineRoster _roster;
        private readonly List<UiMachineRow> _rows = new List<UiMachineRow>();
        private readonly List<UiDetailField> _detail = new List<UiDetailField>(16);
        private MachineDomainSnapshot _snapshot;
        private PersistentId _selected = PersistentId.Invalid;
        private bool _disposed;

        public MachineReadModel(MachineRoster roster)
        {
            _roster = roster ?? throw new ArgumentNullException(nameof(roster));
            _roster.Changed += OnRosterChanged;
            Rebuild();
        }

        public MachineDomainSnapshot Snapshot => _snapshot;

        public event Action<MachineDomainSection> Changed;

        public PersistentId SelectedId => _selected;

        public bool Select(PersistentId id)
        {
            if (_disposed) return false;
            if (!_roster.TryGet(id, out _)) return false;
            if (_selected == id) return true;
            _selected = id;
            RebuildDetail();
            Changed?.Invoke(MachineDomainSection.Detail);
            return true;
        }

        public void ClearSelection()
        {
            if (_disposed || !_selected.IsValid) return;
            _selected = PersistentId.Invalid;
            RebuildDetail();
            Changed?.Invoke(MachineDomainSection.Detail);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _roster.Changed -= OnRosterChanged;
            Changed = null;
            _rows.Clear();
            _detail.Clear();
        }

        private void OnRosterChanged()
        {
            if (_disposed) return;

            Rebuild();
            // 选中对象可能已被移除：失效时明确清掉，不自动改选另一台。
            if (_selected.IsValid && !_roster.TryGet(_selected, out _))
            {
                _selected = PersistentId.Invalid;
            }

            RebuildDetail();
            Changed?.Invoke(MachineDomainSection.List);
        }

        private void Rebuild()
        {
            _rows.Clear();
            foreach (MachineInstance machine in _roster.Machines)
            {
                _rows.Add(new UiMachineRow(machine.Id, machine.Name, AutoEraUiFormat.MachineSummary(machine), machine.Deployed));
            }

            _rows.Sort(CompareRows);
            PublishState();
        }

        private static int CompareRows(UiMachineRow left, UiMachineRow right) =>
            left.Id.Value.CompareTo(right.Id.Value);

        private void RebuildDetail()
        {
            _detail.Clear();
            if (_selected.IsValid && _roster.TryGet(_selected, out MachineInstance machine))
            {
                AppendDetail(machine);
            }

            PublishState();
        }

        private void AppendDetail(MachineInstance machine)
        {
            _detail.Add(new UiDetailField("名称", machine.Name));
            _detail.Add(new UiDetailField("型号", machine.Definition != null ? machine.Definition.Name : AutoEraUiFormat.Missing));
            _detail.Add(new UiDetailField("序列", machine.ModelSerial.ToString(CultureInfo.InvariantCulture)));
            _detail.Add(new UiDetailField("部署", machine.Deployed ? "已部署" : "库中"));
            _detail.Add(new UiDetailField("运行", AutoEraUiFormat.RunState(machine.RequestedRunState)));
            _detail.Add(new UiDetailField("供电", machine.Powered ? "正常" : "不可用"));
            _detail.Add(new UiDetailField("连接", machine.Connected ? "已连接" : "未连接"));
            _detail.Add(new UiDetailField("完整度", AutoEraUiFormat.Integrity(machine.Integrity)));
            _detail.Add(new UiDetailField("算力", AutoEraUiFormat.Count(machine.ComputeCapacity) + " / 已用 " + AutoEraUiFormat.Count(machine.UsedCapacity)));
            _detail.Add(new UiDetailField("逻辑", AutoEraUiFormat.Count(machine.LogicCapacity)));
            _detail.Add(new UiDetailField("容量", AutoEraUiFormat.Count(machine.TotalCapacity)));
        }

        private void PublishState()
        {
            // 领域就绪但没有任何记录 → Empty（显示空态说明），而不是 Ready 或 Unavailable。
            Publish(_rows.Count == 0 ? UiDataState.Empty : UiDataState.Ready, null);
        }

        private void Publish(UiDataState state, string reason)
        {
            _snapshot = new MachineDomainSnapshot(state, reason, _rows, _detail);
        }
    }

    /// <summary>机器域读取模型的创建入口：领域未接入时返回 Unavailable 实现，而不是 null。</summary>
    public static class MachineReadModels
    {
        public static IMachineReadModel Create(AutoEraUiSession session)
        {
            if (session == null)
            {
                return new UnavailableMachineReadModel("缺少服务会话：机器数据不可用");
            }

            if (!session.HasWorld)
            {
                return new UnavailableMachineReadModel("尚未进入世界：机器数据不可用");
            }

            return new MachineReadModel(session.World.Machines);
        }
    }
}
