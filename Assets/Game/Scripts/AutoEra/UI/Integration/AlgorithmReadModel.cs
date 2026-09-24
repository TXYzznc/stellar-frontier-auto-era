using System;
using System.Collections.Generic;
using AutoEra.Algorithms;
using AutoEra.World.Identity;
using AutoEra.World.Region;

namespace AutoEra.UI
{
    /// <summary>算法域内发生变化的区域。</summary>
    public enum AlgorithmDomainSection
    {
        /// <summary>模板列表发生变化。</summary>
        List,

        /// <summary>当前选中模板的详情发生变化。</summary>
        Detail,
    }

    /// <summary>模板列表的一行：稳定身份 + 名称 + 版本 + 是否系统模板。</summary>
    public readonly struct UiAlgorithmTemplateRow
    {
        public UiAlgorithmTemplateRow(ulong id, string name, string version, bool isSystem)
        {
            Id = id;
            Name = name;
            Version = version;
            IsSystem = isSystem;
        }

        public ulong Id { get; }
        public string Name { get; }
        public string Version { get; }

        /// <summary>系统模板只读；玩家模板可改名/删除。</summary>
        public bool IsSystem { get; }
    }

    /// <summary>机器上一台算法实例的一行：稳定身份 + 版本三元组 + 算力占用 + 应用请求状态。</summary>
    public readonly struct UiAlgorithmInstanceRow
    {
        public UiAlgorithmInstanceRow(AlgorithmInstanceInfo info)
        {
            Id = info.Id;
            AppliedRevision = info.AppliedRevision;
            DraftRevision = info.DraftRevision;
            SavedRevision = info.SavedRevision;
            LogicCost = info.LogicCost;
            RequestState = info.RequestState;
            RequestId = info.RequestId;
            RequestReason = info.RequestReason;
        }

        public ulong Id { get; }
        public ulong AppliedRevision { get; }
        public ulong DraftRevision { get; }
        public ulong SavedRevision { get; }
        public int LogicCost { get; }
        public AlgorithmApplyState RequestState { get; }
        public ulong RequestId { get; }
        public string RequestReason { get; }

        /// <summary>草稿是否领先于已应用版本。</summary>
        public bool HasUnappliedDraft => DraftRevision > AppliedRevision;

        /// <summary>一行列表用的短标签。</summary>
        public string Label => "算法实例 " + Id;

        /// <summary>一行列表用的状态摘要。</summary>
        public string Status =>
            "已应用 r" + AppliedRevision + " ／ 草稿 r" + DraftRevision
            + (HasUnappliedDraft ? "（未应用）" : string.Empty)
            + " ／ 逻辑算力 " + LogicCost
            + (RequestState == AlgorithmApplyState.None ? string.Empty : " ／ 请求 " + RequestState);
    }

    /// <summary>算法域的只读快照。</summary>
    public readonly struct AlgorithmDomainSnapshot
    {
        public AlgorithmDomainSnapshot(
            UiDataState state,
            string unavailableReason,
            IReadOnlyList<UiAlgorithmTemplateRow> templates,
            IReadOnlyList<UiDetailField> detail,
            int selectedIndex,
            PersistentId machineId = default,
            string machineName = null,
            IReadOnlyList<UiAlgorithmInstanceRow> instances = null)
        {
            State = state;
            UnavailableReason = unavailableReason;
            Templates = templates;
            Detail = detail;
            SelectedIndex = selectedIndex;
            MachineId = machineId;
            MachineName = machineName;
            Instances = instances;
        }

        public UiDataState State { get; }
        public string UnavailableReason { get; }
        public IReadOnlyList<UiAlgorithmTemplateRow> Templates { get; }
        public IReadOnlyList<UiDetailField> Detail { get; }
        public int SelectedIndex { get; }

        /// <summary>本次快照对应的机器；未解析到机器时为 <see cref="PersistentId.Invalid"/>。</summary>
        public PersistentId MachineId { get; }

        /// <summary>机器显示名；未解析到机器时为 null。</summary>
        public string MachineName { get; }

        /// <summary>机器上的算法实例（真实数据；模板库仍未接线时这是界面能展示的全部内容）。</summary>
        public IReadOnlyList<UiAlgorithmInstanceRow> Instances { get; }

        public int Count => Templates == null ? 0 : Templates.Count;

        /// <summary>实例数量。与 <see cref="Count"/>（模板数量）刻意分开：它们是两个域。</summary>
        public int InstanceCount => Instances == null ? 0 : Instances.Count;

        public bool HasSelection => SelectedIndex >= 0 && SelectedIndex < Count;

        /// <summary>当前选中的实例；没有实例或未选中时为 null。</summary>
        public UiAlgorithmInstanceRow? SelectedInstance
        {
            get
            {
                if (Instances == null || SelectedIndex < 0 || SelectedIndex >= Instances.Count)
                {
                    return null;
                }

                return Instances[SelectedIndex];
            }
        }

        public int SystemCount => CountBySystem(true);
        public int PlayerCount => CountBySystem(false);

        private int CountBySystem(bool system)
        {
            if (Templates == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < Templates.Count; i++)
            {
                if (Templates[i].IsSystem == system)
                {
                    count++;
                }
            }

            return count;
        }

        public static AlgorithmDomainSnapshot Unavailable(string reason) =>
            new AlgorithmDomainSnapshot(UiDataState.Unavailable, reason, null, null, -1);
    }

    /// <summary>
    /// 算法域读取模型。四个算法界面只依赖本接口。
    ///
    /// 实现有两条：<see cref="MachineAlgorithmReadModel"/>（有机器运行时，读真实实例服务）
    /// 与 <see cref="UnavailableAlgorithmReadModel"/>（缺能力时只报原因，绝不伪造数据）。
    /// </summary>
    public interface IAlgorithmReadModel : IDisposable
    {
        AlgorithmDomainSnapshot Snapshot { get; }

        event Action<AlgorithmDomainSection> Changed;

        int SelectedIndex { get; }

        void Refresh();

        /// <summary>按**实例**的稳定 Id 选中。不存在时返回 false，不改变当前选中。</summary>
        bool Select(ulong instanceId);

        void ClearSelection();
    }

    /// <summary>
    /// 算法界面读模型：**真实实现**，数据来自这台机器的区域运行时。
    ///
    /// 为什么以「机器」为入口：`MachineExecutionContext`（任务队列／算力池／传感器）与
    /// `AlgorithmInstanceService` 现在只在 <c>RegionMachineRuntimeRegistry.TryAttach</c> 里创建
    /// ——也就是**部署一台机器**。在那之前，算法域在界面眼里确实没有创建者。
    ///
    /// 机器身份来自区域当前的选中对象，**不按名字查找**（规格要求传稳定 ID）。选中对象
    /// 不是机器、或那台机器没有运行时，都会落到 Unavailable 并给出各自可辨的原因。
    ///
    /// 模板库（`AlgorithmTemplateLibrary`）仍然没有生产创建者——它是存档级数据，
    /// 不属于任何一台机器。所以实例列表可以是真实的、而模板列表仍为空：
    /// 两者刻意分成 <see cref="AlgorithmDomainSnapshot.Instances"/> 与
    /// <see cref="AlgorithmDomainSnapshot.Templates"/>，界面能区分「没有实例」和「没有模板库」。
    /// </summary>
    internal sealed class MachineAlgorithmReadModel : IAlgorithmReadModel
    {
        private readonly AlgorithmInstanceService _instances;
        private readonly List<UiAlgorithmInstanceRow> _rows = new List<UiAlgorithmInstanceRow>(8);
        private readonly List<UiDetailField> _detail = new List<UiDetailField>(12);
        private AlgorithmDomainSnapshot _snapshot;
        private int _selectedIndex = -1;
        private bool _autoSelectPending = true;
        private bool _disposed;

        public MachineAlgorithmReadModel(RegionMachineRuntime runtime)
        {
            Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _instances = runtime.Instances;
            if (_instances != null)
            {
                _instances.Changed += OnServiceChanged;
            }

            Publish();
        }

        public RegionMachineRuntime Runtime { get; }

        public AlgorithmDomainSnapshot Snapshot => _snapshot;

        public event Action<AlgorithmDomainSection> Changed;

        public int SelectedIndex => _selectedIndex;

        public void Refresh() => Publish();

        /// <summary>按实例的稳定 Id 选中；不存在时返回 false 且不改选中。</summary>
        public bool Select(ulong instanceId)
        {
            if (_instances == null || !_instances.HasInstance(instanceId))
            {
                return false;
            }

            int index = -1;
            for (int i = 0; i < _rows.Count; i++)
            {
                if (_rows[i].Id == instanceId)
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                return false;
            }

            _selectedIndex = index;
            _autoSelectPending = false;
            Publish(AlgorithmDomainSection.Detail);
            return true;
        }

        public void ClearSelection()
        {
            // 清空是**玩家的意图**：这次之后不再自动选中，否则「取消选中」会被下一次 Publish
            // 悄悄撤销（这正是第一版的行为，测试直接抓到了）。
            _autoSelectPending = false;
            _selectedIndex = -1;
            Publish(AlgorithmDomainSection.Detail);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (_instances != null)
            {
                _instances.Changed -= OnServiceChanged;
            }

            Changed = null;
        }

        private void OnServiceChanged()
        {
            if (!_disposed)
            {
                Publish();
            }
        }

        /// <summary>
        /// 重建快照后再发事件。**值类型快照必须在每次内容变化时重建**：
        /// 只发事件会让页面继续读到上一份数组（这条在存档域与区域域各踩过一次）。
        /// </summary>
        private void Publish(AlgorithmDomainSection section = AlgorithmDomainSection.List)
        {
            _rows.Clear();
            if (_instances != null)
            {
                AlgorithmInstanceInfo[] infos = _instances.ListInstances();
                for (int i = 0; i < infos.Length; i++)
                {
                    _rows.Add(new UiAlgorithmInstanceRow(infos[i]));
                }
            }

            // 首次拿到实例时自动选中第一行（否则详情栏永远空着）；
            // 一旦玩家显式清空过选中，就不再替他选回来。
            if (_autoSelectPending && _rows.Count > 0)
            {
                _selectedIndex = 0;
                _autoSelectPending = false;
            }
            else if (_selectedIndex >= _rows.Count)
            {
                _selectedIndex = _rows.Count > 0 ? _rows.Count - 1 : -1;
            }

            BuildDetail();

            // 有运行时但一台算法实例都没有：这是 **Empty**（领域接线了，只是还没有东西），
            // 不是 Unavailable——把它们混成一个状态会让「界面在撒谎」与「域没接线」分不清。
            UiDataState state = _rows.Count > 0 ? UiDataState.Ready : UiDataState.Empty;
            _snapshot = new AlgorithmDomainSnapshot(
                state,
                state == UiDataState.Empty ? AlgorithmReadModels.NoInstanceReason : null,
                new UiAlgorithmTemplateRow[0],
                _detail.ToArray(),
                _selectedIndex,
                Runtime.MachineId,
                ResolveMachineName(),
                _rows.ToArray());

            Changed?.Invoke(section);
        }

        private string ResolveMachineName()
        {
            var machine = Runtime.Context != null ? Runtime.Context.Machine : null;
            return machine != null && machine.Definition != null ? machine.Definition.Name : null;
        }

        private void BuildDetail()
        {
            _detail.Clear();
            _detail.Add(new UiDetailField("机器", ResolveMachineName() ?? "—"));
            _detail.Add(new UiDetailField("运行时", "已建立"));
            _detail.Add(new UiDetailField("导航", Runtime.HasNavigation
                ? "已绑定"
                : Runtime.IsNavigationDegraded ? "降级：" + Runtime.NavigationUnavailableReason : "不需要（不可移动）"));
            _detail.Add(new UiDetailField("算力池", Runtime.Context != null && Runtime.Context.Compute != null
                ? "占用 " + Runtime.Context.Compute.Used + " ／ 等待 " + Runtime.Context.Compute.WaitingCount
                : "—"));

            UiAlgorithmInstanceRow? selected = _snapshot.SelectedInstance;
            if (selected.HasValue)
            {
                UiAlgorithmInstanceRow row = selected.Value;
                _detail.Add(new UiDetailField("实例", row.Id.ToString()));
                _detail.Add(new UiDetailField("已应用版本", "r" + row.AppliedRevision));
                _detail.Add(new UiDetailField("草稿版本", "r" + row.DraftRevision + (row.HasUnappliedDraft ? "（未应用）" : "（与已应用一致）")));
                _detail.Add(new UiDetailField("已保存版本", "r" + row.SavedRevision));
                _detail.Add(new UiDetailField("逻辑算力", row.LogicCost.ToString()));
                _detail.Add(new UiDetailField("应用请求", row.RequestState == AlgorithmApplyState.None
                    ? "无"
                    : row.RequestState + (string.IsNullOrEmpty(row.RequestReason) ? string.Empty : "：" + row.RequestReason)));
                return;
            }

            // 没有实例时，详情栏要解释「为什么没有」——这台机器的运行时是真实存在的。
            _detail.Add(new UiDetailField("算法实例", "无"));
            _detail.Add(new UiDetailField("原因", AlgorithmReadModels.NoInstanceReason));
        }
    }

    /// <summary>算法界面的统一入口：把「算法域为什么不可用」讲清楚。</summary>
    public static class AlgorithmReadModels
    {
        /// <summary>
        /// 模板库还没有生产创建者。模板是**存档级**数据（玩家模板要跨机器存在），
        /// 不属于任何一台机器，所以它不随机器运行时一起出现。
        /// </summary>
        public const string NotWiredReason =
            "算法模板库尚未接入世界运行路径：模板属于存档级数据，生产里还没有创建者，"
            + "因此模板列表与模板详情暂不可用。";

        /// <summary>机器运行时存在、但还没有任何算法实例。</summary>
        public const string NoInstanceReason =
            "这台机器还没有算法实例：实例由模板实例化创建，而模板库还没有生产创建者。";

        /// <summary>
        /// 建立算法读模型。
        ///
        /// 分支顺序即「缺什么」的优先级，每一层都给**可展示且可辨**的原因：
        /// 会话 → 世界 → 区域 → 运行时注册表 → 选中的机器 → 那台机器的运行时。
        /// 前四层是能力缺失，第五层是「玩家还没选机器」，第六层是「选了但它在运行时里不存在」。
        /// </summary>
        public static IAlgorithmReadModel Create(AutoEraUiSession session)
        {
            if (session == null)
            {
                return new UnavailableAlgorithmReadModel("没有界面会话：算法数据不可用。");
            }

            if (!session.HasWorld)
            {
                return new UnavailableAlgorithmReadModel("算法属于某个世界里的机器，请先从主菜单进入区域。");
            }

            if (!session.HasRegion)
            {
                return new UnavailableAlgorithmReadModel(
                    "算法属于现场的一台机器：当前会话没有可用的现场区域，请先进入区域。");
            }

            if (!session.HasMachineRuntimes)
            {
                return new UnavailableAlgorithmReadModel(
                    "现场区域尚未建立机器运行时：区域就绪后部署一台机器，算法界面才会有数据来源。");
            }

            if (!TryResolveMachine(session, out PersistentId machineId))
            {
                return new UnavailableAlgorithmReadModel(
                    "没有选中的机器：请先在世界里选中一台机器，再从它的算法入口打开本页。");
            }

            if (!session.MachineRuntimes.TryGet(machineId, out RegionMachineRuntime runtime))
            {
                return new UnavailableAlgorithmReadModel(
                    "这台机器还没有运行时（可能尚未部署，或区域刚重建）：算法实例与执行上下文都建立在运行时之上。");
            }

            return new MachineAlgorithmReadModel(runtime);
        }

        /// <summary>
        /// 机器身份来自区域当前的选中对象。**只认稳定 Id**：不为「猜一台机器」留任何回退，
        /// 否则界面就会在玩家没选机器时悄悄展示另一台机器的数据。
        /// </summary>
        private static bool TryResolveMachine(AutoEraUiSession session, out PersistentId machineId)
        {
            machineId = PersistentId.Invalid;
            if (session.Region == null || !session.Region.IsActive)
            {
                return false;
            }

            PersistentId selected = session.Region.SelectedId;
            if (!selected.IsValid)
            {
                return false;
            }

            machineId = selected;
            return true;
        }
    }

    /// <summary>算法域未接入时的诚实空实现：只报 Unavailable，不伪造模板与实例。</summary>
    internal sealed class UnavailableAlgorithmReadModel : IAlgorithmReadModel
    {
        public UnavailableAlgorithmReadModel(string reason)
        {
            Snapshot = AlgorithmDomainSnapshot.Unavailable(reason);
        }

        public AlgorithmDomainSnapshot Snapshot { get; }

        public event Action<AlgorithmDomainSection> Changed
        {
            add { }
            remove { }
        }

        public int SelectedIndex => -1;

        public void Refresh() { }

        public bool Select(ulong templateId) => false;

        public void ClearSelection() { }

        public void Dispose() { }
    }
}
