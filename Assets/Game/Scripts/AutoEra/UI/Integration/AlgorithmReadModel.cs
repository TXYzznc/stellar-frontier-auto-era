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

    /// <summary>算法读模型的数据域：模板库（世界级）或机器实例（机器级）。</summary>
    public enum AlgorithmReadModelDomain
    {
        /// <summary>机器实例（编辑/绑定页）：需要区域、运行时注册表与选中的机器。</summary>
        Machine,

        /// <summary>模板库（算法库页）：世界级，只需进入世界。</summary>
        Library,
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

        /// <summary>一行列表用的短标签。</summary>
        public string Label => Name;

        /// <summary>一行列表用的状态摘要（系统/玩家 + 版本）。</summary>
        public string Status => (IsSystem ? "系统模板" : "玩家模板") + " ／ v" + Version;
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

    /// <summary>图节点在最近一次运行里的执行状态（诊断读路径）。</summary>
    public enum UiAlgorithmNodeDiagnostic
    {
        /// <summary>无运行记录，或该节点不在最近运行里被求值。</summary>
        None,

        /// <summary>节点在最近一次运行的执行路径上。</summary>
        Executed,

        /// <summary>节点是最近一次运行的失败点。</summary>
        Failed,
    }

    /// <summary>选中实例草稿图里的一行节点：稳定身份 + 展示标签/状态 + 诊断状态。</summary>
    public readonly struct UiAlgorithmNodeRow
    {
        public UiAlgorithmNodeRow(ulong id, string label, string status,
            UiAlgorithmNodeDiagnostic diagnostic = UiAlgorithmNodeDiagnostic.None)
        {
            Id = id;
            Label = label;
            Status = status;
            Diagnostic = diagnostic;
        }

        public ulong Id { get; }
        public string Label { get; }
        public string Status { get; }

        /// <summary>最近一次运行里该节点的执行状态（供节点栏高亮/标注）。</summary>
        public UiAlgorithmNodeDiagnostic Diagnostic { get; }
    }

    /// <summary>选中实例最近一次运行的一行摘要（诊断读路径）。</summary>
    public readonly struct UiAlgorithmRunRow
    {
        public UiAlgorithmRunRow(ulong runId, string error, ulong failedNode, int cost)
        {
            RunId = runId;
            Error = error;
            FailedNode = failedNode;
            Cost = cost;
        }

        public ulong RunId { get; }
        public string Error { get; }
        public ulong FailedNode { get; }
        public int Cost { get; }
        public bool Succeeded => string.IsNullOrEmpty(Error);
        public string Label => "运行 #" + RunId;
        public string Status => (Succeeded ? "正常" : "错误：" + Error) + " ／ 瞬时成本 " + Cost;
    }

    /// <summary>选中实例草稿图里的一行连线：From → To，输出端口 → 输入端口。</summary>
    public readonly struct UiAlgorithmEdgeRow
    {
        public UiAlgorithmEdgeRow(ulong from, ulong to, string output, string input)
        {
            From = from;
            To = to;
            Output = output;
            Input = input;
        }

        public ulong From { get; }
        public ulong To { get; }
        public string Output { get; }
        public string Input { get; }
        public string Label => From + " → " + To;
        public string Status => Output + " → " + Input;
    }

    /// <summary>校验问题清单里的一行：严重度 + 代码 + 节点定位。</summary>
    public readonly struct UiAlgorithmIssueRow
    {
        public UiAlgorithmIssueRow(AlgorithmIssueSeverity severity, string code, ulong nodeId, string portId)
        {
            Severity = severity;
            Code = code ?? string.Empty;
            NodeId = nodeId;
            PortId = portId;
        }

        public AlgorithmIssueSeverity Severity { get; }
        public string Code { get; }
        public ulong NodeId { get; }
        public string PortId { get; }
        public bool IsError => Severity == AlgorithmIssueSeverity.Error;
        public string Label => (IsError ? "错误" : "警告") + " · " + Code;
        public string Status =>
            "节点 #" + (NodeId == 0 ? "—" : NodeId.ToString())
            + (string.IsNullOrEmpty(PortId) ? string.Empty : " ／ 端口 " + PortId);
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
            IReadOnlyList<UiAlgorithmInstanceRow> instances = null,
            int selectedTemplateIndex = -1,
            IReadOnlyList<UiDetailField> templateDetail = null,
            IReadOnlyList<UiAlgorithmNodeRow> graphNodes = null,
            IReadOnlyList<UiAlgorithmEdgeRow> graphEdges = null,
            IReadOnlyList<UiAlgorithmIssueRow> issues = null,
            IReadOnlyList<UiDetailField> nodeDetail = null,
            int selectedNodeIndex = -1,
            UiAlgorithmRunRow? latestRun = null)
        {
            State = state;
            UnavailableReason = unavailableReason;
            Templates = templates;
            Detail = detail;
            SelectedIndex = selectedIndex;
            MachineId = machineId;
            MachineName = machineName;
            Instances = instances;
            SelectedTemplateIndex = selectedTemplateIndex;
            TemplateDetail = templateDetail;
            GraphNodes = graphNodes;
            GraphEdges = graphEdges;
            Issues = issues;
            NodeDetail = nodeDetail;
            SelectedNodeIndex = selectedNodeIndex;
            LatestRun = latestRun;
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

        /// <summary>当前选中模板在 <see cref="Templates"/> 里的下标；未选中为 -1。</summary>
        public int SelectedTemplateIndex { get; }

        /// <summary>当前选中模板的详情字段（库页）；未选中为 null。</summary>
        public IReadOnlyList<UiDetailField> TemplateDetail { get; }

        /// <summary>选中实例草稿图的节点行；无实例/未选中/缺运行时为 null。</summary>
        public IReadOnlyList<UiAlgorithmNodeRow> GraphNodes { get; }

        /// <summary>选中实例草稿图的连线行；无实例/未选中/缺运行时为 null。</summary>
        public IReadOnlyList<UiAlgorithmEdgeRow> GraphEdges { get; }

        /// <summary>选中实例草稿的校验问题行；无实例/未选中/缺运行时为 null。</summary>
        public IReadOnlyList<UiAlgorithmIssueRow> Issues { get; }

        /// <summary>当前选中节点的详情字段（检视器）；未选中节点为 null。</summary>
        public IReadOnlyList<UiDetailField> NodeDetail { get; }

        /// <summary>当前选中节点在 <see cref="GraphNodes"/> 里的下标；未选中为 -1。</summary>
        public int SelectedNodeIndex { get; }

        public int Count => Templates == null ? 0 : Templates.Count;

        /// <summary>图节点数量。</summary>
        public int GraphNodeCount => GraphNodes == null ? 0 : GraphNodes.Count;

        /// <summary>图连线数量。</summary>
        public int GraphEdgeCount => GraphEdges == null ? 0 : GraphEdges.Count;

        /// <summary>校验问题数量。</summary>
        public int IssueCount => Issues == null ? 0 : Issues.Count;

        /// <summary>当前选中的图节点；没有图或未选中时为 null。</summary>
        public UiAlgorithmNodeRow? SelectedNode
        {
            get
            {
                if (GraphNodes == null || SelectedNodeIndex < 0 || SelectedNodeIndex >= GraphNodes.Count)
                {
                    return null;
                }

                return GraphNodes[SelectedNodeIndex];
            }
        }

        /// <summary>选中实例最近一次运行的摘要；无实例/无运行历史时为 null。</summary>
        public UiAlgorithmRunRow? LatestRun { get; }

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

        /// <summary>当前选中的模板；没有模板或未选中时为 null。</summary>
        public UiAlgorithmTemplateRow? SelectedTemplate
        {
            get
            {
                if (Templates == null || SelectedTemplateIndex < 0 || SelectedTemplateIndex >= Templates.Count)
                {
                    return null;
                }

                return Templates[SelectedTemplateIndex];
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

        /// <summary>按**模板**的稳定 Id 选中。不存在时返回 false，不改变当前选中。机器域读模型（实例）返回 false。</summary>
        bool SelectTemplate(ulong templateId);

        /// <summary>按**图节点**的稳定 Id 选中（供检视器展示）。仅在已选中实例且该节点存在于草稿时成功；库页/不可用域返回 false。</summary>
        bool SelectNode(ulong nodeId);

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
    /// 模板库（`AlgorithmTemplateLibrary`）已由 <c>AutoEraWorldSession</c> 创建并种子化，
    /// 但模板列表/详情的读模型通道尚未接线（选中语义需要接口扩展，见 b17 design Open Questions）。
    /// 所以实例列表是真实的、而模板列表仍为空：
    /// 两者刻意分成 <see cref="AlgorithmDomainSnapshot.Instances"/> 与
    /// <see cref="AlgorithmDomainSnapshot.Templates"/>，界面能区分「没有实例」和「没有模板库」。
    /// </summary>
    internal sealed class MachineAlgorithmReadModel : IAlgorithmReadModel
    {
        private readonly AlgorithmInstanceService _instances;
        private readonly List<UiAlgorithmInstanceRow> _rows = new List<UiAlgorithmInstanceRow>(8);
        private readonly List<UiDetailField> _detail = new List<UiDetailField>(12);
        private readonly List<UiAlgorithmNodeRow> _graphNodes = new List<UiAlgorithmNodeRow>(16);
        private readonly List<UiAlgorithmEdgeRow> _graphEdges = new List<UiAlgorithmEdgeRow>(16);
        private readonly List<UiAlgorithmIssueRow> _issues = new List<UiAlgorithmIssueRow>(8);
        private readonly List<UiDetailField> _nodeDetail = new List<UiDetailField>(8);
        private AlgorithmDomainSnapshot _snapshot;
        private int _selectedIndex = -1;
        private int _selectedNodeIndex = -1;
        private UiAlgorithmRunRow? _latestRun;
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
            _selectedNodeIndex = -1;
            Publish(AlgorithmDomainSection.Detail);
            return true;
        }

        /// <summary>机器域读模型不提供模板列表，按模板 Id 选中恒为 false。</summary>
        public bool SelectTemplate(ulong templateId) => false;

        /// <summary>按图节点的稳定 Id 选中，供检视器展示节点属性；节点不存在时返回 false 且不改选中。</summary>
        public bool SelectNode(ulong nodeId)
        {
            for (int i = 0; i < _graphNodes.Count; i++)
            {
                if (_graphNodes[i].Id != nodeId)
                {
                    continue;
                }

                _selectedNodeIndex = i;
                Publish(AlgorithmDomainSection.Detail);
                return true;
            }

            return false;
        }

        public void ClearSelection()
        {
            // 清空是**玩家的意图**：这次之后不再自动选中，否则「取消选中」会被下一次 Publish
            // 悄悄撤销（这正是第一版的行为，测试直接抓到了）。
            _autoSelectPending = false;
            _selectedIndex = -1;
            _selectedNodeIndex = -1;
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
            BuildGraph();

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
                _rows.ToArray(),
                graphNodes: _graphNodes.ToArray(),
                graphEdges: _graphEdges.ToArray(),
                issues: _issues.ToArray(),
                nodeDetail: _nodeDetail.ToArray(),
                selectedNodeIndex: _selectedNodeIndex,
                latestRun: _latestRun);

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

            UiAlgorithmInstanceRow? selected = _selectedIndex >= 0 && _selectedIndex < _rows.Count
                ? _rows[_selectedIndex]
                : (UiAlgorithmInstanceRow?)null;
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

        /// <summary>
        /// 把选中实例的草稿图映射成节点/连线/校验问题，并重建选中节点的详情。
        /// 只读：`ReadDraft` 返回副本，`TryCompile` 不触碰服务状态。
        /// </summary>
        private void BuildGraph()
        {
            _graphNodes.Clear();
            _graphEdges.Clear();
            _issues.Clear();
            _nodeDetail.Clear();
            _latestRun = null;

            if (_instances == null || _selectedIndex < 0 || _selectedIndex >= _rows.Count)
            {
                _selectedNodeIndex = -1;
                return;
            }

            AlgorithmDocument draft = _instances.ReadDraft(_rows[_selectedIndex].Id);
            if (draft == null)
            {
                _selectedNodeIndex = -1;
                return;
            }

            // 最近一次运行：执行路径用于标记节点，失败节点优先于已执行。
            var executedPath = new HashSet<ulong>();
            ulong failedNode = 0;
            AlgorithmRunRecord[] history = _instances.ReadHistory(_rows[_selectedIndex].Id);
            if (history != null && history.Length > 0)
            {
                AlgorithmRunRecord latest = history[history.Length - 1];
                failedNode = latest.FailedNode;
                ulong[] path = latest.CopyPath();
                for (int i = 0; i < path.Length; i++)
                {
                    executedPath.Add(path[i]);
                }

                _latestRun = new UiAlgorithmRunRow(latest.RunId, latest.Error, latest.FailedNode, latest.Cost);
            }

            var nodesById = new Dictionary<ulong, AlgorithmNode>();
            if (draft.Nodes != null)
            {
                for (int i = 0; i < draft.Nodes.Count; i++)
                {
                    AlgorithmNode node = draft.Nodes[i];
                    if (node == null || node.Deleted)
                    {
                        continue;
                    }

                    UiAlgorithmNodeDiagnostic diagnostic = node.Id == failedNode
                        ? UiAlgorithmNodeDiagnostic.Failed
                        : executedPath.Contains(node.Id)
                            ? UiAlgorithmNodeDiagnostic.Executed
                            : UiAlgorithmNodeDiagnostic.None;
                    string status = FormatNodeStatus(node);
                    if (diagnostic == UiAlgorithmNodeDiagnostic.Executed)
                    {
                        status += " ／ 已执行";
                    }
                    else if (diagnostic == UiAlgorithmNodeDiagnostic.Failed)
                    {
                        status += " ／ 失败";
                    }

                    nodesById[node.Id] = node;
                    _graphNodes.Add(new UiAlgorithmNodeRow(node.Id, node.Kind + " #" + node.Id, status, diagnostic));
                }
            }

            if (draft.Edges != null)
            {
                for (int i = 0; i < draft.Edges.Count; i++)
                {
                    AlgorithmEdge edge = draft.Edges[i];
                    if (edge != null)
                    {
                        _graphEdges.Add(new UiAlgorithmEdgeRow(edge.From, edge.To, edge.Output, edge.Input));
                    }
                }
            }

            // 完整实例校验（含绑定）；int.MaxValue 容量＝问题栏只呈现结构正确性，不因算力容量报错。
            AlgorithmValidator.TryCompile(draft, int.MaxValue, out _, out List<AlgorithmIssue> issues, false);
            if (issues != null)
            {
                for (int i = 0; i < issues.Count; i++)
                {
                    AlgorithmIssue issue = issues[i];
                    _issues.Add(new UiAlgorithmIssueRow(issue.Severity, issue.Code, issue.NodeId, issue.PortId));
                }
            }

            if (_selectedNodeIndex < 0 || _selectedNodeIndex >= _graphNodes.Count)
            {
                _selectedNodeIndex = -1;
            }

            if (_selectedNodeIndex >= 0)
            {
                ulong nodeId = _graphNodes[_selectedNodeIndex].Id;
                if (nodesById.TryGetValue(nodeId, out AlgorithmNode selected))
                {
                    BuildNodeDetail(selected);
                }
            }
        }

        private void BuildNodeDetail(AlgorithmNode node)
        {
            _nodeDetail.Add(new UiDetailField("节点", "#" + node.Id));
            _nodeDetail.Add(new UiDetailField("类型", node.Kind.ToString()));
            bool usesOperator = node.Kind == AlgorithmNodeKind.Arithmetic
                || node.Kind == AlgorithmNodeKind.Compare
                || node.Kind == AlgorithmNodeKind.Boolean;
            if (usesOperator)
            {
                _nodeDetail.Add(new UiDetailField("运算符", node.Operator.ToString()));
            }

            _nodeDetail.Add(new UiDetailField("值类型", FormatType(node.ValueType)));
            if (node.Default != null)
            {
                _nodeDetail.Add(new UiDetailField("默认值", FormatValue(node.Default)));
            }

            if (!string.IsNullOrEmpty(node.BindingKey))
            {
                _nodeDetail.Add(new UiDetailField("绑定键", node.BindingKey));
            }

            if (!string.IsNullOrEmpty(node.StateKey))
            {
                _nodeDetail.Add(new UiDetailField("状态键", node.StateKey));
            }

            _nodeDetail.Add(new UiDetailField("字段", string.IsNullOrEmpty(node.Field) ? "—" : node.Field));
            if (node.Kind == AlgorithmNodeKind.Effector)
            {
                _nodeDetail.Add(new UiDetailField("动作", node.Action.ToString()));
            }
        }

        private static string FormatNodeStatus(AlgorithmNode node)
        {
            string type = FormatType(node.ValueType);
            if (!string.IsNullOrEmpty(node.BindingKey))
            {
                return type + " ／ 绑定 " + node.BindingKey;
            }

            if (!string.IsNullOrEmpty(node.StateKey))
            {
                return type + " ／ 状态 " + node.StateKey;
            }

            return type;
        }

        private static string FormatType(AlgorithmType type)
        {
            if (type == null)
            {
                return "未指定类型";
            }

            string suffix = string.IsNullOrEmpty(type.Unit) ? string.Empty : " " + type.Unit;
            string kind = type.Kind.ToString();
            if (type.Kind == AlgorithmValueKind.Object && !string.IsNullOrEmpty(type.ObjectCategory))
            {
                kind += ":" + type.ObjectCategory;
            }
            else if (type.Kind == AlgorithmValueKind.Enumeration && !string.IsNullOrEmpty(type.EnumFamily))
            {
                kind += ":" + type.EnumFamily;
            }

            return kind + suffix;
        }

        private static string FormatValue(AlgorithmValue value)
        {
            if (value == null)
            {
                return "—";
            }

            if (!value.IsValid)
            {
                return "无效";
            }

            AlgorithmValueKind kind = value.Type == null ? AlgorithmValueKind.Number : value.Type.Kind;
            switch (kind)
            {
                case AlgorithmValueKind.Boolean:
                    return value.Boolean ? "true" : "false";
                case AlgorithmValueKind.Number:
                    return value.Number.ToString("0.##")
                        + (value.Type != null && !string.IsNullOrEmpty(value.Type.Unit) ? " " + value.Type.Unit : string.Empty);
                case AlgorithmValueKind.Position:
                    return "(" + value.X.ToString("0.##") + ", " + value.Y.ToString("0.##") + ", " + value.Z.ToString("0.##") + ")";
                case AlgorithmValueKind.Object:
                    return "对象 #" + value.ObjectId;
                case AlgorithmValueKind.Enumeration:
                    return "枚举 " + value.EnumValue;
                default:
                    return value.Number.ToString("0.##");
            }
        }
    }

    /// <summary>
    /// 算法模板库读模型：真实数据来自世界级模板库（<c>AutoEraWorldSession.AlgorithmTemplates</c>）。
    ///
    /// 与 <see cref="MachineAlgorithmReadModel"/> 是两个域：库页看**模板**（世界级、跨机器共享），
    /// 编辑/绑定页看**实例**（机器级）。因此模板读模型只关心 <see cref="AlgorithmDomainSnapshot.Templates"/>
    /// 与 <see cref="AlgorithmDomainSnapshot.TemplateDetail"/>，实例字段恒为空。
    /// </summary>
    internal sealed class TemplateAlgorithmReadModel : IAlgorithmReadModel
    {
        private readonly AlgorithmTemplateLibrary _library;
        private readonly List<UiAlgorithmTemplateRow> _rows = new List<UiAlgorithmTemplateRow>(8);
        private readonly List<UiDetailField> _detail = new List<UiDetailField>(8);
        private AlgorithmDomainSnapshot _snapshot;
        private int _selectedTemplateIndex = -1;
        private bool _autoSelectPending = true;
        private bool _disposed;

        public TemplateAlgorithmReadModel(AlgorithmTemplateLibrary library)
        {
            _library = library ?? throw new ArgumentNullException(nameof(library));
            Publish();
        }

        public AlgorithmDomainSnapshot Snapshot => _snapshot;

        public event Action<AlgorithmDomainSection> Changed;

        /// <summary>库页不涉及实例选中，恒为 -1。</summary>
        public int SelectedIndex => -1;

        public void Refresh() => Publish();

        /// <summary>库页不涉及实例，按实例 Id 选中恒为 false。</summary>
        public bool Select(ulong instanceId) => false;

        /// <summary>库页不涉及图节点，按节点 Id 选中恒为 false。</summary>
        public bool SelectNode(ulong nodeId) => false;

        /// <summary>按模板稳定 Id 选中；不存在时返回 false 且不改选中。</summary>
        public bool SelectTemplate(ulong templateId)
        {
            if (_disposed)
            {
                return false;
            }

            for (int i = 0; i < _rows.Count; i++)
            {
                if (_rows[i].Id != templateId)
                {
                    continue;
                }

                _selectedTemplateIndex = i;
                _autoSelectPending = false;
                Publish(AlgorithmDomainSection.Detail);
                return true;
            }

            return false;
        }

        public void ClearSelection()
        {
            _autoSelectPending = false;
            _selectedTemplateIndex = -1;
            Publish(AlgorithmDomainSection.Detail);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Changed = null;
        }

        private void Publish(AlgorithmDomainSection section = AlgorithmDomainSection.List)
        {
            _rows.Clear();
            foreach (AlgorithmTemplateInfo info in _library.List())
            {
                _rows.Add(new UiAlgorithmTemplateRow(info.Id, info.Name, info.Version.ToString(), info.IsSystem));
            }

            if (_autoSelectPending && _rows.Count > 0)
            {
                _selectedTemplateIndex = 0;
                _autoSelectPending = false;
            }
            else if (_selectedTemplateIndex >= _rows.Count)
            {
                _selectedTemplateIndex = _rows.Count > 0 ? _rows.Count - 1 : -1;
            }

            BuildDetail();

            UiDataState state = _rows.Count > 0 ? UiDataState.Ready : UiDataState.Empty;
            _snapshot = new AlgorithmDomainSnapshot(
                state,
                state == UiDataState.Empty ? "模板库为空。" : null,
                _rows.ToArray(),
                new UiDetailField[0],
                -1,
                default,
                null,
                null,
                _selectedTemplateIndex,
                _detail.ToArray());

            Changed?.Invoke(section);
        }

        private void BuildDetail()
        {
            _detail.Clear();
            if (_selectedTemplateIndex < 0 || _selectedTemplateIndex >= _rows.Count)
            {
                _detail.Add(new UiDetailField("模板", "无"));
                return;
            }

            UiAlgorithmTemplateRow row = _rows[_selectedTemplateIndex];
            _detail.Add(new UiDetailField("名称", row.Name));
            _detail.Add(new UiDetailField("类型", row.IsSystem ? "系统模板" : "玩家模板"));
            _detail.Add(new UiDetailField("版本", "v" + row.Version));

            if (!_library.TryGetDocument(row.Id, out AlgorithmDocument document) || document == null)
            {
                return;
            }

            _detail.Add(new UiDetailField("节点", (document.Nodes == null ? 0 : document.Nodes.Count).ToString()));
            _detail.Add(new UiDetailField("连线", (document.Edges == null ? 0 : document.Edges.Count).ToString()));
            _detail.Add(new UiDetailField("绑定", (document.Bindings == null ? 0 : document.Bindings.Count).ToString()));

            if (AlgorithmValidator.TryCompile(document, int.MaxValue, out AlgorithmPlan plan, out _, true))
            {
                _detail.Add(new UiDetailField("逻辑成本", plan.LogicCost.ToString()));
            }
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
            "算法模板列表尚未接入读模型：世界已持有五套系统模板（`AutoEraWorldSession.AlgorithmTemplates`），"
            + "但模板列表/详情的读模型通道还未接线，因此本页暂不展示模板。";

        /// <summary>机器运行时存在、但还没有任何算法实例。</summary>
        public const string NoInstanceReason =
            "这台机器还没有算法实例：实例由模板实例化创建，而「模板 → 实例」的创建入口尚未接线。";

        /// <summary>
        /// 建立算法读模型。
        ///
        /// 按 <paramref name="domain"/> 分流：<see cref="AlgorithmReadModelDomain.Library"/> 读**模板**
        /// （世界级，`AutoEraWorldSession.AlgorithmTemplates`），<see cref="AlgorithmReadModelDomain.Machine"/> 读
        /// **实例**（机器级，`RegionMachineRuntime.Instances`）。两个域的可用条件不同——模板只要进了世界就有，
        /// 实例还需要区域、运行时注册表与选中的机器。
        ///
        /// Machine 域分支顺序即「缺什么」的优先级，每一层都给**可展示且可辨**的原因：
        /// 会话 → 世界 → 区域 → 运行时注册表 → 选中的机器 → 那台机器的运行时。
        /// </summary>
        public static IAlgorithmReadModel Create(AutoEraUiSession session,
            AlgorithmReadModelDomain domain = AlgorithmReadModelDomain.Machine)
        {
            if (domain == AlgorithmReadModelDomain.Library)
            {
                return CreateLibrary(session);
            }

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
        /// 库页读模型：模板是**世界级**数据，只要进了世界就可用，不需要区域、运行时或选中的机器。
        /// </summary>
        private static IAlgorithmReadModel CreateLibrary(AutoEraUiSession session)
        {
            if (session == null)
            {
                return new UnavailableAlgorithmReadModel("没有界面会话：算法数据不可用。");
            }

            if (!session.HasWorld)
            {
                return new UnavailableAlgorithmReadModel("算法模板属于某个世界，请先从主菜单进入区域。");
            }

            return new TemplateAlgorithmReadModel(session.World.AlgorithmTemplates);
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

        public bool SelectTemplate(ulong templateId) => false;

        public bool SelectNode(ulong nodeId) => false;

        public void ClearSelection() { }

        public void Dispose() { }
    }
}
