using System;
using System.Collections.Generic;
using AutoEra.Algorithms;
using AutoEra.Machines;
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
            (AppliedRevision == 0
                ? "未应用（草稿）"
                : "已应用 r" + AppliedRevision + (HasUnappliedDraft ? "（未应用）" : string.Empty))
            + " ／ 草稿 r" + DraftRevision
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

    /// <summary>选中实例草稿图里的一行节点：稳定身份 + 展示标签/状态 + 诊断状态 + 端口行。</summary>
    public readonly struct UiAlgorithmNodeRow
    {
        public UiAlgorithmNodeRow(ulong id, string label, string status,
            UiAlgorithmNodeDiagnostic diagnostic = UiAlgorithmNodeDiagnostic.None,
            float layoutX = 0f, float layoutY = 0f,
            IReadOnlyList<UiAlgorithmPortRow> inputPorts = null,
            IReadOnlyList<UiAlgorithmPortRow> outputPorts = null,
            AlgorithmValue defaultValue = null)
        {
            Id = id;
            Label = label;
            Status = status;
            Diagnostic = diagnostic;
            LayoutX = layoutX;
            LayoutY = layoutY;
            InputPorts = inputPorts ?? Array.Empty<UiAlgorithmPortRow>();
            OutputPorts = outputPorts ?? Array.Empty<UiAlgorithmPortRow>();
            Default = defaultValue?.Copy();
        }

        public ulong Id { get; }
        public string Label { get; }
        public string Status { get; }

        /// <summary>最近一次运行里该节点的执行状态（供节点栏高亮/标注）。</summary>
        public UiAlgorithmNodeDiagnostic Diagnostic { get; }

        /// <summary>画布布局坐标（供画布节点定位，画布自由布局）。</summary>
        public float LayoutX { get; }
        public float LayoutY { get; }

        /// <summary>该节点的输入端口行（目录序稳定；画布两步连线用）。</summary>
        public IReadOnlyList<UiAlgorithmPortRow> InputPorts { get; }

        /// <summary>该节点的输出端口行（目录序稳定；画布两步连线用）。</summary>
        public IReadOnlyList<UiAlgorithmPortRow> OutputPorts { get; }
        public AlgorithmValue Default { get; }
    }

    /// <summary>选中实例最近一次运行的一行摘要（诊断读路径）。</summary>
    public readonly struct UiAlgorithmRunRow
    {
        public UiAlgorithmRunRow(ulong runId, string error, ulong failedNode, int cost,
            IReadOnlyDictionary<ulong, AlgorithmValue> nodeValues = null)
        {
            RunId = runId;
            Error = error;
            FailedNode = failedNode;
            Cost = cost;
            NodeValues = nodeValues;
        }

        public ulong RunId { get; }
        public string Error { get; }
        public ulong FailedNode { get; }
        public int Cost { get; }
        public bool Succeeded => string.IsNullOrEmpty(Error);
        public string Label => "运行 #" + RunId;
        public string Status => (Succeeded ? "正常" : "错误：" + Error) + " ／ 瞬时成本 " + Cost;

        /// <summary>最近一次运行里各值节点的「当时值」快照（节点 Id → 主输出值），供诊断按值细节。</summary>
        public IReadOnlyDictionary<ulong, AlgorithmValue> NodeValues { get; }
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

    /// <summary>图节点上的一个端口行：端口名 + 类型标签 + 连接状态（是否有边指向/离开该端口）。</summary>
    public readonly struct UiAlgorithmPortRow
    {
        public UiAlgorithmPortRow(string key, string typeLabel, bool connected)
            : this(key, typeLabel, AlgorithmValueKind.Number, connected)
        {
        }

        public UiAlgorithmPortRow(string key, string typeLabel, AlgorithmValueKind valueKind, bool connected)
        {
            Key = key ?? string.Empty;
            TypeLabel = typeLabel ?? string.Empty;
            ValueKind = valueKind;
            Connected = connected;
        }

        public string Key { get; }
        public string TypeLabel { get; }
        public AlgorithmValueKind ValueKind { get; }
        public bool Connected { get; }
        public string Label => Key + "：" + TypeLabel;
    }

    /// <summary>节点库目录行：种类 + 显示名 + 分类 + 逻辑成本。静态能力清单，三个域一致。</summary>
    public readonly struct UiAlgorithmNodeKindRow
    {
        public UiAlgorithmNodeKindRow(AlgorithmNodeKind kind, string label, string category, int cost)
        {
            Kind = kind;
            Label = label ?? string.Empty;
            Category = category ?? string.Empty;
            Cost = cost;
        }

        public AlgorithmNodeKind Kind { get; }
        public string Label { get; }
        public string Category { get; }
        public int Cost { get; }
        public string Status => Category + " · 成本 " + Cost;
    }

    /// <summary>
    /// 节点种类目录（规格「节点库」）：AlgorithmNodeKind 枚举派生的静态清单，
    /// 显示名/分类/成本固定，与实例无关——机器域、模板域、不可用域读到同一份。
    /// 分类沿用规格页的语义分组（输入／判断与运算／状态／流程／行为），值源与调试单列。
    /// </summary>
    public static class AlgorithmNodeLibrary
    {
        /// <summary>全部节点种类目录行（按枚举序稳定排列）。</summary>
        public static readonly UiAlgorithmNodeKindRow[] Rows = BuildRows();

        private static UiAlgorithmNodeKindRow[] BuildRows()
        {
            var kinds = (AlgorithmNodeKind[])Enum.GetValues(typeof(AlgorithmNodeKind));
            var rows = new UiAlgorithmNodeKindRow[kinds.Length];
            for (int i = 0; i < kinds.Length; i++)
            {
                rows[i] = new UiAlgorithmNodeKindRow(kinds[i], LabelOf(kinds[i]), CategoryOf(kinds[i]), AlgorithmCatalog.Cost(kinds[i]));
            }

            return rows;
        }

        /// <summary>种类显示名（搜索过滤也用这个，不是枚举名）。</summary>
        public static string LabelOf(AlgorithmNodeKind kind)
        {
            switch (kind)
            {
                case AlgorithmNodeKind.Constant: return "常量";
                case AlgorithmNodeKind.Parameter: return "参数";
                case AlgorithmNodeKind.Input: return "输入";
                case AlgorithmNodeKind.Startup: return "启动";
                case AlgorithmNodeKind.Arithmetic: return "算术";
                case AlgorithmNodeKind.Compare: return "比较";
                case AlgorithmNodeKind.Boolean: return "布尔";
                case AlgorithmNodeKind.Branch: return "分支";
                case AlgorithmNodeKind.Merge: return "汇合";
                case AlgorithmNodeKind.Variable: return "变量";
                case AlgorithmNodeKind.SetVariable: return "设置变量";
                case AlgorithmNodeKind.Delay: return "延时";
                case AlgorithmNodeKind.Navigate: return "导航";
                case AlgorithmNodeKind.Effector: return "执行器";
                case AlgorithmNodeKind.SubmitTask: return "提交任务";
                case AlgorithmNodeKind.QueryTask: return "查询任务";
                case AlgorithmNodeKind.CancelTask: return "取消任务";
                case AlgorithmNodeKind.Hysteresis: return "滞回";
                case AlgorithmNodeKind.Log: return "日志";
                case AlgorithmNodeKind.Cargo: return "货舱";
                default: return kind.ToString();
            }
        }

        /// <summary>规格页语义分组；未知种类落到「其他」保证目录永远全覆盖。</summary>
        public static string CategoryOf(AlgorithmNodeKind kind)
        {
            switch (kind)
            {
                case AlgorithmNodeKind.Constant:
                case AlgorithmNodeKind.Parameter:
                    return "数值";
                case AlgorithmNodeKind.Input:
                    return "输入";
                case AlgorithmNodeKind.Arithmetic:
                case AlgorithmNodeKind.Compare:
                case AlgorithmNodeKind.Boolean:
                case AlgorithmNodeKind.Hysteresis:
                    return "判断与运算";
                case AlgorithmNodeKind.Variable:
                case AlgorithmNodeKind.SetVariable:
                case AlgorithmNodeKind.Cargo:
                    return "状态";
                case AlgorithmNodeKind.Startup:
                case AlgorithmNodeKind.Branch:
                case AlgorithmNodeKind.Merge:
                case AlgorithmNodeKind.Delay:
                    return "流程";
                case AlgorithmNodeKind.Navigate:
                case AlgorithmNodeKind.Effector:
                case AlgorithmNodeKind.SubmitTask:
                case AlgorithmNodeKind.QueryTask:
                case AlgorithmNodeKind.CancelTask:
                    return "行为";
                case AlgorithmNodeKind.Log:
                    return "调试";
                default:
                    return "其他";
            }
        }
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

    /// <summary>选中实例草稿里一个待绑定/已绑定的端点（Input 传感器 / Effector 效应器）。</summary>
    public readonly struct UiAlgorithmBindingRow
    {
        public UiAlgorithmBindingRow(string bindingKey, AlgorithmNodeKind kind, string field, string action,
            bool bound, ulong componentId, ulong targetId)
        {
            BindingKey = bindingKey ?? string.Empty;
            Kind = kind;
            Field = field ?? string.Empty;
            Action = action ?? string.Empty;
            Bound = bound;
            ComponentId = componentId;
            TargetId = targetId;
        }

        public string BindingKey { get; }
        public AlgorithmNodeKind Kind { get; }
        public string Field { get; }
        public string Action { get; }
        public bool Bound { get; }
        public ulong ComponentId { get; }
        public ulong TargetId { get; }

        /// <summary>端点用途：传感器读哪个字段、效应器做什么动作。</summary>
        public string Port =>
            Kind == AlgorithmNodeKind.Input
                ? (string.IsNullOrEmpty(Field) ? "输入" : "读 " + Field)
                : (string.IsNullOrEmpty(Action) ? "效应器" : Action);

        public string Label => (Kind == AlgorithmNodeKind.Input ? "传感器" : "效应器") + " · " + BindingKey;
        public string Status => Bound ? "已绑定 #" + ComponentId + " → #" + TargetId : "待绑定";
    }

    /// <summary>机器上已安装、可被算法端点绑定的一行候选组件（传感器/效应器）。</summary>
    public readonly struct UiAlgorithmComponentCandidate
    {
        public UiAlgorithmComponentCandidate(ulong componentId, string name, HardwareKind kind, int level, bool enabled)
        {
            ComponentId = componentId;
            Name = name ?? string.Empty;
            Kind = kind;
            Level = level;
            Enabled = enabled;
        }

        public ulong ComponentId { get; }
        public string Name { get; }
        public HardwareKind Kind { get; }
        public int Level { get; }
        public bool Enabled { get; }

        public string Label => Name + " · L" + Level;
        public string Status => Enabled ? "已安装" : "已停用";
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
            UiAlgorithmRunRow? latestRun = null,
            IReadOnlyList<UiAlgorithmBindingRow> pendingBindings = null,
            IReadOnlyList<UiAlgorithmComponentCandidate> componentCandidates = null,
            IReadOnlyList<UiAlgorithmNodeKindRow> nodeKinds = null,
            IReadOnlyList<UiAlgorithmRunRow> runs = null,
            int selectedRunIndex = -1)
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
            PendingBindings = pendingBindings;
            ComponentCandidates = componentCandidates;
            NodeKinds = nodeKinds ?? AlgorithmNodeLibrary.Rows;
            Runs = runs;
            SelectedRunIndex = selectedRunIndex;
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
        public IReadOnlyList<UiAlgorithmRunRow> Runs { get; }
        public int SelectedRunIndex { get; }

        /// <summary>选中实例草稿的绑定端点（Input/Effector 的 BindingKey 与绑定状态）；无实例/库页为 null。</summary>
        public IReadOnlyList<UiAlgorithmBindingRow> PendingBindings { get; }

        /// <summary>待绑定端点数量。</summary>
        public int PendingBindingCount => PendingBindings == null ? 0 : PendingBindings.Count;

        /// <summary>机器上已安装、可被算法端点绑定的候选组件（传感器/效应器）；非机器域为 null。</summary>
        public IReadOnlyList<UiAlgorithmComponentCandidate> ComponentCandidates { get; }

        /// <summary>节点种类目录（节点库数据）；静态能力清单，所有域一致，缺省取 <see cref="AlgorithmNodeLibrary.Rows"/>。</summary>
        public IReadOnlyList<UiAlgorithmNodeKindRow> NodeKinds { get; }

        /// <summary>候选组件数量。</summary>
        public int ComponentCandidateCount => ComponentCandidates == null ? 0 : ComponentCandidates.Count;

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

        /// <summary>
        /// 从模板在当前选中机器上创建「草稿实例」（写路径入口）。返回新实例的稳定 Id；无法解析机器、
        /// 机器无运行时或模板无效时返回 0。机器域/不可用域读模型恒返回 0（它们不持有模板库）。
        /// </summary>
        ulong InstantiateTemplate(ulong templateId);

        /// <summary>
        /// 按 <c>BindingKey</c> 更新选中实例草稿的绑定（写路径「绑定重绑」）。机器域调用实例服务
        /// <c>Rebind</c>；库页/不可用域返回 false。无选中实例时按 <c>instanceId</c> 定位。
        /// </summary>
        bool Rebind(ulong instanceId, string bindingKey, ulong componentId, ulong targetId, ulong generation);

        /// <summary>
        /// 把「绑定完整且校验通过」的草稿实例编译为运行时（写路径「激活」）。机器域读草稿 →
        /// <c>TryCompile</c>（真实算力容量，template:false）→ 构造运行时（adapter 作 sink）→
        /// <c>Adapter.Attach</c> → <c>CompileDraft</c>；库页/不可用域返回 false。
        /// </summary>
        bool ActivateDraft(ulong instanceId);

        /// <summary>
        /// 统一「应用」命令：草稿实例（<c>AppliedRevision == 0</c>）首次应用＝激活（<see cref="ActivateDraft"/>），
        /// 已激活实例且草稿领先＝<c>Apply</c>；无事可做或域不支持返回 false。
        /// </summary>
        bool Apply(ulong instanceId);

        /// <summary>确认应用请求的警告（<c>AwaitingWarningConfirmation</c> → <c>WaitingSafePoint</c>）；库页/不可用域返回 false。</summary>
        bool ConfirmWarnings(ulong instanceId, ulong requestId);

        /// <summary>取消待处理的应用请求；库页/不可用域返回 false。</summary>
        bool CancelApply(ulong instanceId, ulong requestId);

        /// <summary>移动选中实例草稿节点的画布坐标（写路径「画布布局」）；库页/不可用域返回 false。</summary>
        bool MoveNode(ulong instanceId, ulong nodeId, float x, float y);

        /// <summary>
        /// 在选中实例的草稿创建一个**最小默认节点**（写路径「节点库添加」），稳定 Id 由服务分配。
        /// 返回新节点 Id；实例不存在、修订不匹配、正在应用或域不支持（库页/不可用域）时返回 0。
        /// </summary>
        ulong CreateNode(ulong instanceId, AlgorithmNodeKind kind, float layoutX, float layoutY);

        /// <summary>在选中实例草稿连接一对端口（写路径「强类型连线」）；端口缺失、不兼容、目标输入已占用或域不支持时返回 false。</summary>
        bool Connect(ulong instanceId, ulong from, string output, ulong to, string input);

        /// <summary>按完整边身份断开草稿连线（写路径「断开连接」）；边不存在或域不支持时返回 false。</summary>
        bool Disconnect(ulong instanceId, ulong from, string output, ulong to, string input);

        /// <summary>删除草稿节点并级联断开其全部关联边（写路径「删除选中」）；节点不存在或域不支持时返回 false。</summary>
        bool DeleteNode(ulong instanceId, ulong nodeId);

        /// <summary>更新参数节点的草稿默认值；值类型必须与节点声明一致。</summary>
        bool SetNodeDefault(ulong instanceId, ulong nodeId, AlgorithmValue value);

        /// <summary>将参数节点恢复到实例保存版本的默认值。</summary>
        bool ResetNodeDefault(ulong instanceId, ulong nodeId);
        bool SelectPreviousRun();
        bool SelectNextRun();
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
        private readonly MachineCatalog _catalog;
        private readonly List<UiAlgorithmInstanceRow> _rows = new List<UiAlgorithmInstanceRow>(8);
        private readonly List<UiDetailField> _detail = new List<UiDetailField>(12);
        private readonly List<UiAlgorithmNodeRow> _graphNodes = new List<UiAlgorithmNodeRow>(16);
        private readonly List<UiAlgorithmEdgeRow> _graphEdges = new List<UiAlgorithmEdgeRow>(16);
        private readonly List<UiAlgorithmIssueRow> _issues = new List<UiAlgorithmIssueRow>(8);
        private readonly List<UiAlgorithmBindingRow> _bindings = new List<UiAlgorithmBindingRow>(8);
        private readonly List<UiDetailField> _nodeDetail = new List<UiDetailField>(8);
        private readonly List<UiAlgorithmComponentCandidate> _componentCandidates = new List<UiAlgorithmComponentCandidate>(8);
        private AlgorithmDomainSnapshot _snapshot;
        private int _selectedIndex = -1;
        private int _selectedNodeIndex = -1;
        private UiAlgorithmRunRow? _latestRun;
        private readonly List<UiAlgorithmRunRow> _runs = new List<UiAlgorithmRunRow>(8);
        private int _selectedRunIndex = -1;
        private bool _autoSelectPending = true;
        private bool _disposed;

        public MachineAlgorithmReadModel(RegionMachineRuntime runtime, MachineCatalog catalog)
        {
            Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _catalog = catalog;
            _instances = runtime.Instances;
            if (_instances != null)
            {
                _instances.Changed += OnServiceChanged;
            }

            Publish();
        }

        public RegionMachineRuntime Runtime { get; }

        /// <summary>机器上已安装、可被算法端点绑定的候选组件（传感器/效应器）。</summary>
        public IReadOnlyList<UiAlgorithmComponentCandidate> ComponentCandidates => _componentCandidates;

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
            _selectedRunIndex = -1;
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
            _selectedRunIndex = -1;
            Publish(AlgorithmDomainSection.Detail);
        }

        /// <summary>机器域读模型不持有模板库，从模板创建实例恒为 0（入口在库页）。</summary>
        public ulong InstantiateTemplate(ulong templateId) => 0;

        /// <summary>按 BindingKey 更新草稿绑定；实例不存在或修订不匹配时返回 false。</summary>
        public bool Rebind(ulong instanceId, string bindingKey, ulong componentId, ulong targetId, ulong generation)
        {
            if (_disposed || _instances == null)
            {
                return false;
            }

            AlgorithmDocument draft = _instances.ReadDraft(instanceId);
            if (draft == null)
            {
                return false;
            }

            return _instances.Rebind(instanceId, draft.Revision, bindingKey, componentId, targetId, generation);
        }

        /// <summary>把校验通过的草稿编译为运行时并激活；校验失败、域缺 adapter/算力池、或已有活动实例时返回 false。</summary>
        public bool ActivateDraft(ulong instanceId)
        {
            if (_disposed || _instances == null || Runtime.Adapter == null || Runtime.Adapter.HasRuntime ||
                Runtime.Context == null || Runtime.Context.Compute == null)
            {
                return false;
            }

            AlgorithmDocument draft = _instances.ReadDraft(instanceId);
            if (draft == null)
            {
                return false;
            }

            if (!AlgorithmValidator.TryCompile(draft, Runtime.Context.Compute.LogicCapacity, out AlgorithmPlan plan, out _, false))
            {
                return false;
            }

            var runtime = new AlgorithmRuntime(new PersistentId(instanceId), plan, Runtime.Context.Compute, Runtime.Adapter);
            Runtime.Adapter.Attach(runtime);
            return _instances.CompileDraft(instanceId, runtime);
        }

        /// <summary>统一应用：草稿实例首次应用＝激活；已激活实例且草稿领先＝Apply；无事可做返回 false。</summary>
        public bool Apply(ulong instanceId)
        {
            if (_disposed || _instances == null)
            {
                return false;
            }

            AlgorithmInstanceInfo[] infos = _instances.ListInstances();
            AlgorithmInstanceInfo? info = null;
            for (int i = 0; i < infos.Length; i++)
            {
                if (infos[i].Id == instanceId)
                {
                    info = infos[i];
                    break;
                }
            }

            if (!info.HasValue)
            {
                return false;
            }

            if (info.Value.AppliedRevision == 0)
            {
                return ActivateDraft(instanceId);
            }

            return _instances.Apply(instanceId, info.Value.DraftRevision, info.Value.AppliedRevision, out _);
        }

        /// <summary>确认应用请求的警告；实例不存在或请求状态不符时返回 false。</summary>
        public bool ConfirmWarnings(ulong instanceId, ulong requestId)
            => !_disposed && _instances != null && _instances.ConfirmWarnings(instanceId, requestId);

        /// <summary>取消待处理的应用请求。</summary>
        public bool CancelApply(ulong instanceId, ulong requestId)
            => !_disposed && _instances != null && _instances.CancelApply(instanceId, requestId);

        /// <summary>移动草稿节点画布坐标；实例不存在或修订不匹配时返回 false。</summary>
        public bool MoveNode(ulong instanceId, ulong nodeId, float x, float y)
        {
            if (_disposed || _instances == null)
            {
                return false;
            }

            AlgorithmDocument draft = _instances.ReadDraft(instanceId);
            if (draft == null)
            {
                return false;
            }

            return _instances.MoveNode(instanceId, draft.Revision, nodeId, x, y);
        }

        /// <summary>在选中实例草稿创建最小默认节点（稳定 Id 由服务分配）；实例不存在或修订不匹配时返回 0。</summary>
        public ulong CreateNode(ulong instanceId, AlgorithmNodeKind kind, float layoutX, float layoutY)
        {
            if (_disposed || _instances == null)
            {
                return 0;
            }

            AlgorithmDocument draft = _instances.ReadDraft(instanceId);
            if (draft == null)
            {
                return 0;
            }

            return _instances.CreateNode(instanceId, draft.Revision, kind, layoutX, layoutY, out ulong nodeId) ? nodeId : 0;
        }

        /// <summary>在选中实例草稿连接一对强类型兼容端口；实例不存在或修订不匹配时返回 false。</summary>
        public bool Connect(ulong instanceId, ulong from, string output, ulong to, string input)
        {
            if (_disposed || _instances == null)
            {
                return false;
            }

            AlgorithmDocument draft = _instances.ReadDraft(instanceId);
            if (draft == null)
            {
                return false;
            }

            return _instances.Connect(instanceId, draft.Revision, from, output, to, input);
        }

        /// <summary>按完整边身份断开草稿连线；实例不存在或修订不匹配时返回 false。</summary>
        public bool Disconnect(ulong instanceId, ulong from, string output, ulong to, string input)
        {
            if (_disposed || _instances == null)
            {
                return false;
            }

            AlgorithmDocument draft = _instances.ReadDraft(instanceId);
            if (draft == null)
            {
                return false;
            }

            return _instances.Disconnect(instanceId, draft.Revision, from, output, to, input);
        }

        /// <summary>删除草稿节点并级联断开其全部关联边；实例不存在或修订不匹配时返回 false。</summary>
        public bool DeleteNode(ulong instanceId, ulong nodeId)
        {
            if (_disposed || _instances == null)
            {
                return false;
            }

            AlgorithmDocument draft = _instances.ReadDraft(instanceId);
            if (draft == null)
            {
                return false;
            }

            return _instances.DeleteNode(instanceId, draft.Revision, nodeId);
        }

        public bool SetNodeDefault(ulong instanceId, ulong nodeId, AlgorithmValue value)
        {
            if (_disposed || _instances == null || value == null) return false;
            AlgorithmDocument draft = _instances.ReadDraft(instanceId);
            if (draft == null) return false;
            AlgorithmDocument replacement = draft.Copy();
            AlgorithmNode node = replacement.Nodes.Find(n => n != null && !n.Deleted && n.Id == nodeId && n.Kind == AlgorithmNodeKind.Parameter);
            if (node == null || node.ValueType == null || value.Type == null || node.ValueType.Kind != value.Type.Kind) return false;
            node.Default = value.Copy();
            return _instances.Edit(instanceId, draft.Revision, replacement);
        }

        public bool ResetNodeDefault(ulong instanceId, ulong nodeId)
        {
            if (_disposed || _instances == null) return false;
            AlgorithmDocument draft = _instances.ReadDraft(instanceId);
            return draft != null && _instances.ResetDraftNodeDefault(instanceId, draft.Revision, nodeId);
        }

        public bool SelectPreviousRun()
        {
            if (_runs.Count == 0 || _selectedRunIndex <= 0) return false;
            _selectedRunIndex--;
            Publish(AlgorithmDomainSection.Detail);
            return true;
        }

        public bool SelectNextRun()
        {
            if (_runs.Count == 0 || _selectedRunIndex >= _runs.Count - 1) return false;
            _selectedRunIndex++;
            Publish(AlgorithmDomainSection.Detail);
            return true;
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
            BuildComponentCandidates();

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
                latestRun: _latestRun,
                pendingBindings: _bindings.ToArray(),
                componentCandidates: _componentCandidates.ToArray(),
                runs: _runs.ToArray(), selectedRunIndex: _selectedRunIndex);

            Changed?.Invoke(section);
        }

        private string ResolveMachineName()
        {
            var machine = Runtime.Context != null ? Runtime.Context.Machine : null;
            return machine != null && machine.Definition != null ? machine.Definition.Name : null;
        }

        /// <summary>机器上已安装、可被算法端点绑定的候选组件：传感器读输入、效应器执行动作。</summary>
        private void BuildComponentCandidates()
        {
            _componentCandidates.Clear();
            MachineInstance machine = Runtime.Context != null ? Runtime.Context.Machine : null;
            if (machine == null || machine.Definition == null)
            {
                return;
            }

            foreach (HardwareKind kind in CandidateKinds)
            {
                int slots = machine.Definition.SlotCount(kind);
                for (int index = 0; index < slots; index++)
                {
                    ComponentInstance component = machine.GetComponent(kind, index);
                    if (component == null)
                    {
                        continue;
                    }

                    string name = "组件 #" + component.Definition.Id;
                    if (_catalog != null && _catalog.TryGetComponentRow(component.Definition, out ComponentDisplayRow row))
                    {
                        name = row.Name;
                    }

                    _componentCandidates.Add(new UiAlgorithmComponentCandidate(
                        component.Id.Value, name, kind, component.Definition.Level, component.Enabled));
                }
            }
        }

        private static readonly HardwareKind[] CandidateKinds = { HardwareKind.Sensor, HardwareKind.Effector };

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
            _bindings.Clear();
            _nodeDetail.Clear();
            _latestRun = null;
            _runs.Clear();

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
                for (int i = 0; i < history.Length; i++)
                {
                    AlgorithmRunRecord record = history[i];
                    _runs.Add(new UiAlgorithmRunRow(record.RunId, record.Error, record.FailedNode, record.Cost, record.CopyNodeValues()));
                }
                if (_selectedRunIndex < 0 || _selectedRunIndex >= _runs.Count) _selectedRunIndex = _runs.Count - 1;
                AlgorithmRunRecord latest = history[_selectedRunIndex];
                failedNode = latest.FailedNode;
                ulong[] path = latest.CopyPath();
                for (int i = 0; i < path.Length; i++)
                {
                    executedPath.Add(path[i]);
                }

                _latestRun = new UiAlgorithmRunRow(latest.RunId, latest.Error, latest.FailedNode, latest.Cost,
                    latest.CopyNodeValues());
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
                    _graphNodes.Add(new UiAlgorithmNodeRow(node.Id, node.Kind + " #" + node.Id, status, diagnostic,
                        node.LayoutX, node.LayoutY,
                        BuildPortRows(node, draft, true),
                        BuildPortRows(node, draft, false), node.Default));
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

            // 待绑定端点：Input/Effector 节点带 BindingKey，但 Bindings 无对应项或 ComponentId 为 0 即「待绑定」。
            if (draft.Nodes != null)
            {
                for (int i = 0; i < draft.Nodes.Count; i++)
                {
                    AlgorithmNode node = draft.Nodes[i];
                    if (node == null || node.Deleted ||
                        (node.Kind != AlgorithmNodeKind.Input && node.Kind != AlgorithmNodeKind.Effector) ||
                        string.IsNullOrEmpty(node.BindingKey))
                    {
                        continue;
                    }

                    AlgorithmBinding binding = draft.Bindings.Find(b => b != null && b.Key == node.BindingKey);
                    bool bound = binding != null && binding.ComponentId != 0;
                    _bindings.Add(new UiAlgorithmBindingRow(
                        node.BindingKey, node.Kind, node.Field, node.Action.ToString(),
                        bound, bound ? binding.ComponentId : 0, bound ? binding.TargetId : 0));
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

        /// <summary>把目录端口映射为端口行，并按草稿边标记连接状态（输入=有边指向，输出=有边离开）。</summary>
        private static UiAlgorithmPortRow[] BuildPortRows(AlgorithmNode node, AlgorithmDocument draft, bool inputs)
        {
            AlgorithmPort[] ports = inputs ? AlgorithmCatalog.Inputs(node) : AlgorithmCatalog.Outputs(node);
            if (ports == null || ports.Length == 0)
            {
                return Array.Empty<UiAlgorithmPortRow>();
            }

            var rows = new UiAlgorithmPortRow[ports.Length];
            for (int i = 0; i < ports.Length; i++)
            {
                bool connected = false;
                if (draft.Edges != null)
                {
                    for (int e = 0; e < draft.Edges.Count; e++)
                    {
                        AlgorithmEdge edge = draft.Edges[e];
                        if (edge == null)
                        {
                            continue;
                        }

                        if (inputs
                                ? edge.To == node.Id && edge.Input == ports[i].Key
                                : edge.From == node.Id && edge.Output == ports[i].Key)
                        {
                            connected = true;
                            break;
                        }
                    }
                }

                rows[i] = new UiAlgorithmPortRow(ports[i].Key, FormatPortType(ports[i].Type), ports[i].Type?.Kind ?? AlgorithmValueKind.Number, connected);
            }

            return rows;
        }

        /// <summary>端口类型标签：值类型 + 单位/枚举族/对象类别（有则附加）。</summary>
        private static string FormatPortType(AlgorithmType type)
        {
            if (type == null)
            {
                return "—";
            }

            string label = type.Kind.ToString();
            if (!string.IsNullOrEmpty(type.Unit))
            {
                label += ":" + type.Unit;
            }
            else if (type.Kind == AlgorithmValueKind.Enumeration && !string.IsNullOrEmpty(type.EnumFamily))
            {
                label += ":" + type.EnumFamily;
            }
            else if (type.Kind == AlgorithmValueKind.Object && !string.IsNullOrEmpty(type.ObjectCategory))
            {
                label += ":" + type.ObjectCategory;
            }

            return label;
        }

        private void BuildNodeDetail(AlgorithmNode node)
        {            _nodeDetail.Add(new UiDetailField("节点", "#" + node.Id));
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

            // 诊断按值细节：最近运行里该节点的当时值（与「默认值」当前值对比）。
            if (_latestRun.HasValue && _latestRun.Value.NodeValues != null
                && _latestRun.Value.NodeValues.TryGetValue(node.Id, out AlgorithmValue thenValue))
            {
                _nodeDetail.Add(new UiDetailField("当时值", FormatValue(thenValue)));
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
        private readonly AutoEraUiSession _session;
        private readonly List<UiAlgorithmTemplateRow> _rows = new List<UiAlgorithmTemplateRow>(8);
        private readonly List<UiDetailField> _detail = new List<UiDetailField>(8);
        private AlgorithmDomainSnapshot _snapshot;
        private int _selectedTemplateIndex = -1;
        private bool _autoSelectPending = true;
        private bool _disposed;

        public TemplateAlgorithmReadModel(AlgorithmTemplateLibrary library, AutoEraUiSession session)
        {
            _library = library ?? throw new ArgumentNullException(nameof(library));
            _session = session;
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

        /// <summary>
        /// 从模板在当前选中机器上创建草稿实例（写路径「模板 → 实例」入口）。
        /// 解析不到选中机器/机器无运行时/模板无效时返回 0；成功后返回新实例 Id。
        /// </summary>
        public ulong InstantiateTemplate(ulong templateId)
        {
            if (_disposed || _session == null || !_session.HasMachineRuntimes)
            {
                return 0;
            }

            if (!AlgorithmReadModels.TryResolveMachine(_session, out PersistentId machineId))
            {
                return 0;
            }

            if (!_session.MachineRuntimes.TryGet(machineId, out RegionMachineRuntime runtime))
            {
                return 0;
            }

            AlgorithmDocument draft = _library.Instantiate(templateId);
            if (draft == null)
            {
                return 0;
            }

            return runtime.Instances.AddDraft(draft) ? draft.DocumentId : 0;
        }

        /// <summary>库页不持有实例服务，绑定重绑恒为 false（入口在机器域/绑定面板）。</summary>
        public bool Rebind(ulong instanceId, string bindingKey, ulong componentId, ulong targetId, ulong generation) => false;

        /// <summary>库页不持有实例服务与适配器，激活恒为 false（入口在机器域）。</summary>
        public bool ActivateDraft(ulong instanceId) => false;

        /// <summary>库页不持有实例服务，应用恒为 false（入口在机器域/工作台）。</summary>
        public bool Apply(ulong instanceId) => false;

        public bool ConfirmWarnings(ulong instanceId, ulong requestId) => false;

        public bool CancelApply(ulong instanceId, ulong requestId) => false;
        public bool MoveNode(ulong instanceId, ulong nodeId, float x, float y) => false;

        /// <summary>库页不持有实例服务，图编辑命令（创建节点/连接/断开/删除）恒为无操作。</summary>
        public ulong CreateNode(ulong instanceId, AlgorithmNodeKind kind, float layoutX, float layoutY) => 0;
        public bool Connect(ulong instanceId, ulong from, string output, ulong to, string input) => false;
        public bool Disconnect(ulong instanceId, ulong from, string output, ulong to, string input) => false;
        public bool DeleteNode(ulong instanceId, ulong nodeId) => false;
        public bool SetNodeDefault(ulong instanceId, ulong nodeId, AlgorithmValue value) => false;
        public bool ResetNodeDefault(ulong instanceId, ulong nodeId) => false;
        public bool SelectPreviousRun() => false;
        public bool SelectNextRun() => false;

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

            MachineCatalog catalog = MachineCatalog.IsGameDataLoaded ? MachineCatalog.FromLoadedGameData() : null;
            return new MachineAlgorithmReadModel(runtime, catalog);
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

            return new TemplateAlgorithmReadModel(session.World.AlgorithmTemplates, session);
        }

        /// <summary>
        /// 机器身份来自区域当前的选中对象。**只认稳定 Id**：不为「猜一台机器」留任何回退，
        /// 否则界面就会在玩家没选机器时悄悄展示另一台机器的数据。
        /// </summary>
        internal static bool TryResolveMachine(AutoEraUiSession session, out PersistentId machineId)
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

        public ulong InstantiateTemplate(ulong templateId) => 0;

        public bool Rebind(ulong instanceId, string bindingKey, ulong componentId, ulong targetId, ulong generation) => false;

        public bool ActivateDraft(ulong instanceId) => false;

        public bool Apply(ulong instanceId) => false;

        public bool ConfirmWarnings(ulong instanceId, ulong requestId) => false;

        public bool CancelApply(ulong instanceId, ulong requestId) => false;
        public bool MoveNode(ulong instanceId, ulong nodeId, float x, float y) => false;
        public ulong CreateNode(ulong instanceId, AlgorithmNodeKind kind, float layoutX, float layoutY) => 0;
        public bool Connect(ulong instanceId, ulong from, string output, ulong to, string input) => false;
        public bool Disconnect(ulong instanceId, ulong from, string output, ulong to, string input) => false;
        public bool DeleteNode(ulong instanceId, ulong nodeId) => false;
        public bool SetNodeDefault(ulong instanceId, ulong nodeId, AlgorithmValue value) => false;
        public bool ResetNodeDefault(ulong instanceId, ulong nodeId) => false;
        public bool SelectPreviousRun() => false;
        public bool SelectNextRun() => false;

        public void Dispose() { }
    }
}
