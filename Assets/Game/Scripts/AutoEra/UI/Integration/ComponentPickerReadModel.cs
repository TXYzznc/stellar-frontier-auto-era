using System;
using System.Collections.Generic;
using AutoEra.Machines;
using AutoEra.World.Identity;

namespace AutoEra.UI
{
    /// <summary>
    /// 候选组件行：一件**散件**，外加它对这个目标槽位是否兼容。
    ///
    /// 兼容判据只有一条——**硬件的类别与槽位类别一致**。刻意不加别的条件：
    /// 「什么时候允许改硬件」是领域的 `HardwareGate` 的职责，界面自己再定一套
    /// （比如「待资源的组件不能装」）就会造出「界面说不让、领域说可以」的第二套规则。
    /// 因此其它不可安装因素照原样展示（可用性一行），而不是在这里拦下来。
    /// </summary>
    public readonly struct UiComponentCandidate
    {
        public UiComponentCandidate(UiComponentRow row, bool compatible, string incompatibleReason)
        {
            Row = row;
            Compatible = compatible;
            IncompatibleReason = incompatibleReason;
        }

        public UiComponentRow Row { get; }
        public PersistentId Id => Row.Id;
        public bool Compatible { get; }

        /// <summary>不兼容的具体原因；兼容时为空串。</summary>
        public string IncompatibleReason { get; }

        public string Label => Row.Name + "（" + KindLabel + " Lv" + Level + "）";

        public string KindLabel => Row.Definition != null ? Row.Definition.KindLabel : "未知";

        public int Level => Row.Definition != null ? Row.Definition.Level : 0;

        /// <summary>行副标题：兼容时给规格增量，不兼容时说清为什么。</summary>
        public string Value => Compatible ? Specification : "不兼容：" + IncompatibleReason;

        /// <summary>规格摘要（容量／算力／逻辑），与比较栏用的是同一份数字。</summary>
        public string Specification => Row.Definition == null
            ? "目录里没有这件组件的型号行（数据表与实例不一致）"
            : "容量 +" + Row.Definition.AddedCapacity
              + " ／ 算力 +" + Row.Definition.ComputeCapacity
              + " ／ 逻辑 +" + Row.Definition.LogicCapacity;
    }

    /// <summary>
    /// 组件选择器的一屏读数：目标槽位、当前安装、候选列表、预选项与「与当前安装比较」。
    ///
    /// 候选与比较**同源**：比较栏算的就是预选那一件的数字，所以两栏不可能互相打架。
    /// </summary>
    public readonly struct ComponentPickerSnapshot
    {
        public ComponentPickerSnapshot(UiDataState state, string unavailableReason, string slotLabel,
            string occupantLabel, IReadOnlyList<UiComponentCandidate> candidates, PersistentId selectedId,
            IReadOnlyList<UiDetailField> comparison, string confirmBlockedReason)
        {
            State = state;
            UnavailableReason = unavailableReason;
            SlotLabel = slotLabel;
            OccupantLabel = occupantLabel;
            Candidates = candidates;
            SelectedId = selectedId;
            Comparison = comparison;
            ConfirmBlockedReason = confirmBlockedReason;
        }

        public UiDataState State { get; }
        public string UnavailableReason { get; }

        /// <summary>目标槽位的人读名字，例如「核心槽 0」。</summary>
        public string SlotLabel { get; }

        /// <summary>当前安装的组件；空格时为「空」。</summary>
        public string OccupantLabel { get; }

        public IReadOnlyList<UiComponentCandidate> Candidates { get; }

        /// <summary>预选项；未选择时为无效 Id。</summary>
        public PersistentId SelectedId { get; }

        /// <summary>与当前安装的比较行（属性差值／槽位／算法绑定影响／不兼容原因）。</summary>
        public IReadOnlyList<UiDetailField> Comparison { get; }

        /// <summary>「使用该组件」被拦下的原因；可以确认时为空。</summary>
        public string ConfirmBlockedReason { get; }

        public int CandidateCount => Candidates == null ? 0 : Candidates.Count;

        public bool HasSelection => SelectedId.IsValid;

        public bool CanConfirm => SelectedId.IsValid && string.IsNullOrEmpty(ConfirmBlockedReason);

        public static ComponentPickerSnapshot Unavailable(string reason) =>
            new ComponentPickerSnapshot(UiDataState.Unavailable, reason, null, null, null,
                PersistentId.Invalid, null, reason);
    }

    /// <summary>组件选择器的读模型：只暴露这一页需要的字段，并订阅花名册变化。</summary>
    public interface IComponentPickerReadModel : IDisposable
    {
        ComponentPickerSnapshot Snapshot { get; }

        /// <summary>候选、占用或比较内容发生变化。</summary>
        event Action Changed;

        /// <summary>按**实例**的稳定 Id 预选一个候选；不存在时返回 false，不改变当前预选。</summary>
        bool Select(PersistentId candidateId);

        void ClearSelection();
    }

    /// <summary>请求无会话／机器，或槽位失效时的诚实空实现。</summary>
    internal sealed class UnavailableComponentPickerReadModel : IComponentPickerReadModel
    {
        public UnavailableComponentPickerReadModel(string reason)
        {
            Snapshot = ComponentPickerSnapshot.Unavailable(reason);
        }

        public ComponentPickerSnapshot Snapshot { get; }

        public event Action Changed
        {
            add { }
            remove { }
        }

        public bool Select(PersistentId candidateId) => false;

        public void ClearSelection() { }

        public void Dispose() { }
    }

    /// <summary>
    /// 真实实现：候选来自组件域读模型的**散件**一列，占用与槽位来自机器实例，比较栏算预选那一件。
    ///
    /// 为什么组合 `IComponentReadModel` 而不是自己再扫一遍花名册：「哪些组件算散件」「型号叫什么名字」
    /// 都已经有一处实现（`RosterComponentReadModel`），复制一份迟早会出现两种口径
    /// （上一批就踩过：确认页按型号编号显示、整备页按目录名显示，同一件组件在两页里长得不一样）。
    /// 本读模型只加「对这个槽位是否兼容」与「与当前安装比较」两层。
    /// </summary>
    internal sealed class RosterComponentPickerReadModel : IComponentPickerReadModel
    {
        private readonly IComponentReadModel _components;
        private readonly MachineRoster _roster;
        private readonly PersistentId _machineId;
        private readonly HardwareKind _kind;
        private readonly int _slotIndex;
        private readonly List<UiComponentCandidate> _candidates = new List<UiComponentCandidate>(8);
        private readonly List<UiDetailField> _comparison = new List<UiDetailField>(10);
        private ComponentPickerSnapshot _snapshot;
        private PersistentId _selectedId = PersistentId.Invalid;
        private bool _disposed;

        public RosterComponentPickerReadModel(IComponentReadModel components, MachineRoster roster,
            PersistentId machineId, HardwareKind kind, int slotIndex)
        {
            _components = components ?? throw new ArgumentNullException(nameof(components));
            _roster = roster ?? throw new ArgumentNullException(nameof(roster));
            _machineId = machineId;
            _kind = kind;
            _slotIndex = slotIndex;
            _components.Changed += OnComponentsChanged;
            Rebuild();
        }

        public ComponentPickerSnapshot Snapshot => _snapshot;

        public event Action Changed;

        public bool Select(PersistentId candidateId)
        {
            if (_disposed || !TryGetCandidate(candidateId, out _))
            {
                return false;
            }

            _selectedId = candidateId;
            Rebuild();
            Changed?.Invoke();
            return true;
        }

        public void ClearSelection()
        {
            if (_disposed || !_selectedId.IsValid)
            {
                return;
            }

            _selectedId = PersistentId.Invalid;
            Rebuild();
            Changed?.Invoke();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _components.Changed -= OnComponentsChanged;
            // 组合来的读模型由本读模型持有所有权：它只服务于这一页，页走了就该一起释放。
            _components.Dispose();
            Changed = null;
        }

        private void OnComponentsChanged(ComponentDomainSection section)
        {
            if (_disposed)
            {
                return;
            }

            Rebuild();
            Changed?.Invoke();
        }

        /// <summary>
        /// 重建快照后再发事件。值类型快照必须在每次内容变化时重建——只发事件会让页面
        /// 继续读到上一份数组（这条在存档域、区域域、算法域与组件域各踩过一次）。
        /// </summary>
        private void Rebuild()
        {
            _candidates.Clear();
            _comparison.Clear();

            if (!_roster.TryGet(_machineId, out MachineInstance machine))
            {
                _selectedId = PersistentId.Invalid;
                _snapshot = ComponentPickerSnapshot.Unavailable(
                    "请求里的机器不在花名册里：它可能已经被移除。");
                return;
            }

            MachineDefinition definition = machine.Definition;
            string slotLabel = AutoEraUiFormat.Slot(_kind, _slotIndex);
            if (definition == null || _slotIndex < 0 || _slotIndex >= definition.SlotCount(_kind))
            {
                _selectedId = PersistentId.Invalid;
                _snapshot = ComponentPickerSnapshot.Unavailable(
                    "槽位 " + slotLabel + " 不存在于这台机器上：请从整备页重新选择槽位。");
                return;
            }

            ComponentInstance occupant = machine.GetComponent(_kind, _slotIndex);
            string occupantLabel = occupant == null ? "空" : OccupantName(occupant);

            BuildCandidates();

            // 预选项必须在当前候选里还找得到：被装到别处、被别的机器拿走之后就清掉，
            // 不能继续拿着一个已经不属于候选的实例去提交。
            if (_selectedId.IsValid && !TryGetCandidate(_selectedId, out _))
            {
                _selectedId = PersistentId.Invalid;
            }

            BuildComparison(machine, slotLabel, occupantLabel, occupant);

            bool empty = _candidates.Count == 0;
            _snapshot = new ComponentPickerSnapshot(
                empty ? UiDataState.Empty : UiDataState.Ready,
                empty ? ComponentPickerReadModels.NoCandidateReason : null,
                slotLabel,
                occupantLabel,
                _candidates.ToArray(),
                _selectedId,
                _comparison.ToArray(),
                ConfirmBlockedReason());
        }

        private void BuildCandidates()
        {
            ComponentDomainSnapshot components = _components.Snapshot;
            if (components.Loose == null)
            {
                return;
            }

            for (int i = 0; i < components.Loose.Count; i++)
            {
                UiComponentRow row = components.Loose[i];
                bool compatible = row.Definition != null && row.Definition.Kind == _kind;
                string reason = compatible
                    ? string.Empty
                    : row.Definition == null
                        ? "型号行缺失，无法确认它属于哪一类硬件"
                        : "它是" + row.Definition.KindLabel + "，而本槽位是" + AutoEraUiFormat.SlotKind(_kind);

                _candidates.Add(new UiComponentCandidate(row, compatible, reason));
            }

            // 兼容的排在前面：候选列表的作用是回答「能装什么」，把不能装的排在前面会让人白点。
            _candidates.Sort((left, right) =>
            {
                int byCompatibility = right.Compatible.CompareTo(left.Compatible);
                return byCompatibility != 0 ? byCompatibility : left.Id.Value.CompareTo(right.Id.Value);
            });
        }

        private string ConfirmBlockedReason()
        {
            if (!_selectedId.IsValid)
            {
                return "还没有预选组件：先在候选列表里点一行（规格：选择只改变预选项）。";
            }

            return TryGetCandidate(_selectedId, out UiComponentCandidate candidate) && !candidate.Compatible
                ? candidate.IncompatibleReason
                : null;
        }

        /// <summary>
        /// 「与当前安装比较」。数字取的是**当前值 → 装有之后的值**，而不是孤零零一个增量：
        /// 只写「+3」回答不了「装完还剩多少容量」，而那正是玩家要判断的事。
        /// </summary>
        private void BuildComparison(MachineInstance machine, string slotLabel, string occupantLabel,
            ComponentInstance occupant)
        {
            _comparison.Add(new UiDetailField("槽位",
                slotLabel + "（" + AutoEraUiFormat.SlotKind(_kind) + "）"));
            _comparison.Add(new UiDetailField("当前安装", occupantLabel));

            bool hasCandidate = TryGetCandidate(_selectedId, out UiComponentCandidate candidate);
            ComponentDisplayRow incoming = hasCandidate ? candidate.Row.Definition : null;
            _comparison.Add(new UiDetailField("预选", hasCandidate ? candidate.Label : "未选择"));

            if (incoming == null)
            {
                _comparison.Add(new UiDetailField("属性变化", "预选一件组件后显示"));
                _comparison.Add(new UiDetailField("算法绑定影响", BindingImpact));
                _comparison.Add(new UiDetailField("不兼容原因", hasCandidate
                    ? candidate.IncompatibleReason
                    : "还没有预选组件。"));
                return;
            }

            // 装入一个已经装着的槽位会被领域判 Occupied；界面对此**如实提示**而不是提前拦下，
            // 因为「先拆后装」是玩家的自由选择顺序。
            if (occupant != null)
            {
                _comparison.Add(new UiDetailField("注意", "该槽位已装着 " + occupantLabel
                    + "：需要先拆下它，否则领域会以「槽位已经被占用」拒绝。"));
            }

            _comparison.Add(new UiDetailField("容量", Delta("容量", machine.TotalCapacity, incoming.AddedCapacity)));
            _comparison.Add(new UiDetailField("算力", Delta("算力", machine.ComputeCapacity, incoming.ComputeCapacity)));
            _comparison.Add(new UiDetailField("逻辑算力",
                Delta("逻辑", machine.LogicCapacity, incoming.LogicCapacity)));
            _comparison.Add(new UiDetailField("带行为", incoming.HasBehavior ? "是" : "否"));
            _comparison.Add(new UiDetailField("可用性", AvailabilityLabel(incoming.Availability)));
            _comparison.Add(new UiDetailField("算法绑定影响", BindingImpact));
            _comparison.Add(new UiDetailField("不兼容原因", candidate.Compatible
                ? "兼容：类别与槽位一致。"
                : candidate.IncompatibleReason));
        }

        private static string Delta(string label, int current, int added) =>
            label + " " + (added >= 0 ? "+" : "−") + Math.Abs(added)
            + "（当前 " + current + " → 装有后 " + (current + added) + "）";

        private const string BindingImpact =
            "不自动重新绑定：算法里指向该槽位的端点需要重新应用算法后才会指向新组件。";

        private bool TryGetCandidate(PersistentId id, out UiComponentCandidate candidate)
        {
            for (int i = 0; i < _candidates.Count; i++)
            {
                if (_candidates[i].Id == id)
                {
                    candidate = _candidates[i];
                    return true;
                }
            }

            candidate = default;
            return false;
        }

        /// <summary>
        /// 槽位里那件组件的显示名。取的是组件域读模型**已安装**那一列的行，
        /// 也就是组件库页看到的同一个名字——不再另走一条目录解析路径。
        /// </summary>
        private string OccupantName(ComponentInstance occupant)
        {
            ComponentDomainSnapshot components = _components.Snapshot;
            if (components.Installed != null)
            {
                for (int i = 0; i < components.Installed.Count; i++)
                {
                    if (components.Installed[i].Id == occupant.Id)
                    {
                        return components.Installed[i].Name;
                    }
                }
            }

            return "型号 " + (occupant.Definition != null ? occupant.Definition.Id : 0);
        }

        /// <summary>可用性的可展示文案；与组件库页共用一处实现，避免同一字段两种说法。</summary>
        private static string AvailabilityLabel(DefinitionAvailability availability) => availability switch
        {
            DefinitionAvailability.Ready => "已就绪",
            DefinitionAvailability.PendingConfiguration => "待配置",
            DefinitionAvailability.PendingResource => "待资源",
            _ => "未知",
        };
    }

    /// <summary>组件选择器的统一入口。</summary>
    public static class ComponentPickerReadModels
    {
        /// <summary>花名册里没有散件。这是 Empty，不是 Unavailable——装走或买进都会改变它。</summary>
        public const string NoCandidateReason =
            "组件库里没有散件可选：组件由组件库购入、或从机器上拆下后成为散件。"
            + "经济域尚未接入，因此现在只能装已经在库里的组件。";

        public static IComponentPickerReadModel Create(AutoEraUiSession session,
            AutoEraComponentPickRequest request, MachineCatalog catalog = null)
        {
            if (request == null)
            {
                return new UnavailableComponentPickerReadModel(
                    "本界面由调用方带参数打开：它需要知道「为哪台机器的哪一格挑组件」。当前没有请求。");
            }

            if (session == null || !session.HasWorld)
            {
                return new UnavailableComponentPickerReadModel(
                    "组件属于某个世界里的机器，请先从主菜单进入区域。");
            }

            IComponentReadModel components = ComponentReadModels.Create(session, catalog);
            ComponentDomainSnapshot componentState = components.Snapshot;
            if (componentState.State == UiDataState.Unavailable)
            {
                // 组件域自己不可用时把它的原因原样带上去——选择器没有第二套解释。
                components.Dispose();
                return new UnavailableComponentPickerReadModel(
                    "组件数据不可用：" + (componentState.UnavailableReason ?? "候选组件没有来源。"));
            }

            return new RosterComponentPickerReadModel(components, session.World.Machines,
                request.MachineId, request.Kind, request.SlotIndex);
        }
    }
}
