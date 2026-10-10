using System;
using System.Collections.Generic;
using AutoEra.Machines;
using AutoEra.World.Identity;

namespace AutoEra.Algorithms
{
    public enum AlgorithmApplyState { None, AwaitingWarningConfirmation, WaitingSafePoint, Applying, Succeeded, Rejected, Cancelled }
    public sealed class AlgorithmApplyRequest
    {
        public ulong RequestId { get; internal set; }
        public ulong DraftRevision { get; internal set; }
        public ulong ExpectedAppliedRevision { get; internal set; }
        public ulong HardwareRevision { get; internal set; }
        public AlgorithmApplyState State { get; internal set; }
        public string Reason { get; internal set; }
        internal AlgorithmDocument Document;
        internal bool ParametersOnly;
        internal bool WarningsConfirmed;
        internal AlgorithmApplyRequest Copy() => new AlgorithmApplyRequest { RequestId = RequestId, DraftRevision = DraftRevision,
            ExpectedAppliedRevision = ExpectedAppliedRevision, HardwareRevision = HardwareRevision, State = State, Reason = Reason,
            Document = Document?.Copy(), ParametersOnly = ParametersOnly, WarningsConfirmed = WarningsConfirmed };
    }
    public sealed class AlgorithmInstanceCheckpoint
    {
        internal AlgorithmMemorySnapshot Runtime;
        internal AlgorithmDocument Draft, Saved;
        internal AlgorithmApplyRequest Request;
    }

    /// <summary>
    /// 一台机器上某个算法实例的只读状态快照（界面用）。
    ///
    /// 单独开这个类型而不是把 <c>Entry</c> 暴露出去：界面需要的是「这个实例现在处于什么版本、
    /// 占多少逻辑算力、有没有待处理的应用请求」，而不是可变的领域对象。
    /// </summary>
    public readonly struct AlgorithmInstanceInfo
    {
        public AlgorithmInstanceInfo(ulong id, ulong appliedRevision, ulong draftRevision, ulong savedRevision,
            int logicCost, AlgorithmApplyState requestState, ulong requestId, string requestReason)
        {
            Id = id;
            AppliedRevision = appliedRevision;
            DraftRevision = draftRevision;
            SavedRevision = savedRevision;
            LogicCost = logicCost;
            RequestState = requestState;
            RequestId = requestId;
            RequestReason = requestReason;
        }

        public ulong Id { get; }
        public ulong AppliedRevision { get; }
        public ulong DraftRevision { get; }
        public ulong SavedRevision { get; }
        public int LogicCost { get; }
        public AlgorithmApplyState RequestState { get; }
        public ulong RequestId { get; }
        public string RequestReason { get; }

        /// <summary>草稿是否领先于已应用版本——界面据此区分「已生效」与「改了还没应用」。</summary>
        public bool HasUnappliedDraft => DraftRevision > AppliedRevision;
    }

    /// <summary>One machine. UI observers never own pending requests or running state.</summary>
    public sealed partial class AlgorithmInstanceService : IDisposable
    {
        private readonly PersistentIdAllocator _ids;
        private readonly MachineComputePool _compute;
        private readonly Func<ulong> _hardwareRevision;
        private readonly Func<AlgorithmDocument, bool> _bindingsValid;
        private readonly Func<bool> _machineSafe;
        private readonly Dictionary<ulong, Entry> _entries = new Dictionary<ulong, Entry>();
        private readonly List<Entry> _ordered = new List<Entry>();
        private bool _orderDirty;
        private readonly List<AlgorithmRuntime> _restartPaused = new List<AlgorithmRuntime>();
        private Entry _publishing;
        private long _now;
        private bool _disposed;
        public event Action Changed;
        public event Action PersistentApplied;
        public AlgorithmInstanceService(PersistentIdAllocator ids, MachineComputePool compute, Func<ulong> hardwareRevision, Func<AlgorithmDocument, bool> bindingsValid,
            Func<bool> machineSafe = null)
        { _ids = ids ?? throw new ArgumentNullException(nameof(ids)); _compute = compute ?? throw new ArgumentNullException(nameof(compute));
            _hardwareRevision = hardwareRevision ?? throw new ArgumentNullException(nameof(hardwareRevision));
            _bindingsValid = bindingsValid ?? throw new ArgumentNullException(nameof(bindingsValid));
            _machineSafe = machineSafe; }
        public bool Add(AlgorithmRuntime runtime)
        {
            if (_disposed || runtime == null || _entries.ContainsKey(runtime.InstanceId.Value) || !runtime.IsSafe) return false;
            int cost = TotalCost() + runtime.LogicCost;
            if (!_compute.TryApplyLogicCost(cost)) return false;
            var doc = runtime.CopyApplied(); _entries.Add(runtime.InstanceId.Value, new Entry { Runtime = runtime, Draft = doc, Saved = doc.Copy() });
            runtime.Changed += OnRuntimeChanged;
            _orderDirty = true;
            // 新增实例同样是一次状态变化，必须发事件：否则订阅者（界面的读模型）会一直停在
            // 「这台机器没有实例」上，直到别的操作恰好触发一次 Changed。
            // 这条曾经漏过一次：数据对了、界面却是旧状态，看起来像界面没接线。
            Changed?.Invoke();
            return true;
        }

        /// <summary>
        /// 从未绑定、未编译的文档创建「草稿实例」（无运行时）。模板实例化的产物在玩家完成绑定、
        /// 编译并应用之前只存在于草稿里；本方法与 <see cref="Add(AlgorithmRuntime)"/> 并列，
        /// 是写路径「模板 → 实例」的入口。实例 Id 复用文档的 <c>DocumentId</c>（与运行时同一 Id 空间）。
        /// </summary>
        public bool AddDraft(AlgorithmDocument draft)
        {
            if (_disposed || draft == null || draft.DocumentId == 0 || draft.Nodes == null || draft.Edges == null ||
                draft.Bindings == null || _entries.ContainsKey(draft.DocumentId))
            {
                return false;
            }

            _entries.Add(draft.DocumentId, new Entry { Runtime = null, Draft = draft.Copy(), Saved = draft.Copy() });
            _orderDirty = true;
            Changed?.Invoke();
            return true;
        }

        /// <summary>
        /// 把外部编译好的运行时挂接到草稿实例（<c>Runtime == null</c>）——写路径「绑定 → 编译 → 激活」的最后一环。
        /// 是 <see cref="Add(AlgorithmRuntime)"/> 的「就地激活」变体：草稿实例已在 <c>_entries</c> 里，
        /// 只是还没运行时；校验运行时 Id 一致、<c>IsSafe</c> 与算力容量后替换空运行时。
        /// </summary>
        public bool CompileDraft(ulong id, AlgorithmRuntime runtime)
        {
            if (_disposed || runtime == null || runtime.InstanceId.Value != id ||
                !_entries.TryGetValue(id, out var entry) || entry.Runtime != null || !runtime.IsSafe)
            {
                return false;
            }

            if (!_compute.TryApplyLogicCost(TotalCost() + runtime.LogicCost))
            {
                return false;
            }

            entry.Runtime = runtime;
            runtime.Changed += OnRuntimeChanged;
            Changed?.Invoke();
            return true;
        }

        public int AvailableLogicCapacity => Math.Max(0, _compute.LogicCapacity - TotalCost());
        internal bool HasRuntime(ulong id) => _entries.TryGetValue(id, out var entry) && entry.Runtime != null;
        internal bool OwnsRuntime(ulong id, AlgorithmRuntime runtime)
            => _entries.TryGetValue(id, out var entry) && ReferenceEquals(entry.Runtime, runtime);

        public bool TryReadDraft(ulong id, out AlgorithmDocument draft)
        {
            draft = null;
            if (_disposed || !_entries.TryGetValue(id, out var entry)) return false;
            draft = entry.Draft.Copy();
            return true;
        }

        /// <summary>提交机器级预备运行时。先校验全部条件，绑定成功后才发布状态变化。</summary>
        internal bool CommitActivation(ulong id, ulong revision, ulong hardware, AlgorithmRuntime runtime,
            Action attach, long now, out string reason)
        {
            reason = null;
            if (_disposed || runtime == null || runtime.InstanceId.Value != id ||
                !_entries.TryGetValue(id, out var entry) || entry.Runtime != null || entry.Draft.Revision != revision ||
                runtime.Revision != revision || _hardwareRevision() != hardware)
            { reason = "StaleRevision"; return false; }
            if (!_bindingsValid(entry.Draft.Copy())) { reason = "HardwareOrBindingChanged"; return false; }
            if (!runtime.IsSafe || runtime.LogicCost > AvailableLogicCapacity)
            { reason = "LogicCapacityExceeded"; return false; }
            int previousCost = TotalCost();
            if (!_compute.TryApplyLogicCost(previousCost + runtime.LogicCost))
            { reason = "LogicCapacityExceeded"; return false; }
            if (!_entries.TryGetValue(id, out var current) || !ReferenceEquals(current, entry) ||
                entry.Draft.Revision != revision || _hardwareRevision() != hardware)
            {
                _compute.TryApplyLogicCost(previousCost);
                reason = "StaleRevision";
                return false;
            }
            try
            {
                attach();
                entry.Runtime = runtime;
                runtime.Changed += OnRuntimeChanged;
                foreach (var node in runtime.CopyApplied().Nodes)
                    if (!node.Deleted && node.Kind == AlgorithmNodeKind.Startup)
                        runtime.Enqueue(new AlgorithmTrigger { NodeId = node.Id, Revision = runtime.Revision,
                            Generation = runtime.Generation, Time = now });
            }
            catch
            {
                runtime.Changed -= OnRuntimeChanged;
                entry.Runtime = null;
                _compute.TryApplyLogicCost(previousCost);
                throw;
            }
            PersistentApplied?.Invoke();Changed?.Invoke();
            return true;
        }

        internal bool Remove(ulong id, Action detach)
        {
            if (_disposed || !_entries.TryGetValue(id, out var entry)) return false;
            if (ReferenceEquals(_publishing, entry))
            {
                foreach (var paused in _restartPaused) paused.SetPaused(false, _now, AlgorithmPauseReason.Application);
                _restartPaused.Clear(); _publishing = null;
            }
            else if (entry.Runtime != null) _restartPaused.Remove(entry.Runtime);
            _entries.Remove(id);
            _orderDirty = true;
            if (entry.Runtime != null) entry.Runtime.Changed -= OnRuntimeChanged;
            entry.Runtime?.Dispose();
            detach();
            _compute.TryApplyLogicCost(TotalCost());
            Changed?.Invoke();
            return true;
        }

        public AlgorithmDocument ReadDraft(ulong id) => _entries[id].Draft.Copy();
        public AlgorithmDocument ReadSaved(ulong id) => _entries[id].Saved.Copy();
        public ulong SavedDraftRevision(ulong id) => _entries[id].Saved.Revision;
        public AlgorithmApplyRequest ReadRequest(ulong id) => _entries[id].Request?.Copy();

        /// <summary>
        /// 该实例运行时最近运行记录的快照；无实例/无运行时返回空数组。
        /// 这是界面观察诊断历史的唯一只读入口（`_entries` 私有，运行时不能绕过本服务直取）。
        /// </summary>
        public AlgorithmRunRecord[] ReadHistory(ulong id)
        {
            if (_disposed || !_entries.TryGetValue(id, out var entry) || entry.Runtime == null)
            {
                return Array.Empty<AlgorithmRunRecord>();
            }

            return entry.Runtime.History();
        }

        /// <summary>
        /// 本机上全部算法实例的只读状态，按实例 Id 排序。
        ///
        /// **界面观察算法域的唯一入口**：`_entries` 是私有的，没有它界面就只能报「不可用」——
        /// 那正是「算法界面永远说域没接线」这条旧状态的成因。返回新数组，调用方不持有内部状态。
        /// </summary>
        public AlgorithmInstanceInfo[] ListInstances()
        {
            var result = new List<AlgorithmInstanceInfo>(_entries.Count);
            foreach (var pair in _entries)
            {
                Entry entry = pair.Value;
                result.Add(new AlgorithmInstanceInfo(
                    pair.Key,
                    entry.Runtime?.Revision ?? 0,
                    entry.Draft.Revision,
                    entry.Saved.Revision,
                    entry.Runtime?.LogicCost ?? 0,
                    entry.Request?.State ?? AlgorithmApplyState.None,
                    entry.Request?.RequestId ?? 0,
                    entry.Request?.Reason));
            }

            result.Sort((a, b) => a.Id.CompareTo(b.Id));
            return result.ToArray();
        }

        /// <summary>实例是否存在。界面在读草稿前用它做前置判断，避免直接索引抛异常。</summary>
        public bool HasInstance(ulong id) => _entries.ContainsKey(id);
        public bool Edit(ulong id, ulong expectedRevision, AlgorithmDocument replacement)
        {
            if (_disposed || !_entries.TryGetValue(id, out var entry) || entry.Draft.Revision != expectedRevision ||
                replacement == null || replacement.DocumentId != entry.Draft.DocumentId || replacement.Nodes == null || replacement.Edges == null || replacement.Bindings == null ||
                entry.Request?.State == AlgorithmApplyState.Applying) return false;
            entry.Draft = replacement.Copy(); entry.Draft.Revision = checked(expectedRevision + 1); Changed?.Invoke(); return true;
        }
        public bool SaveDraft(ulong id, ulong expectedRevision)
        {
            if (_disposed || !_entries.TryGetValue(id, out var entry) || entry.Draft.Revision != expectedRevision) return false;
            entry.Saved = entry.Draft.Copy(); Changed?.Invoke(); return true;
        }

        public bool ResetDraftNodeDefault(ulong id, ulong expectedRevision, ulong nodeId)
        {
            if (_disposed || !_entries.TryGetValue(id, out var entry) || entry.Draft.Revision != expectedRevision || entry.Saved == null)
                return false;
            AlgorithmNode savedNode = entry.Saved.Nodes.Find(n => n != null && !n.Deleted && n.Id == nodeId && n.Kind == AlgorithmNodeKind.Parameter);
            if (savedNode == null || savedNode.Default == null) return false;
            AlgorithmDocument draft = entry.Draft.Copy();
            AlgorithmNode node = draft.Nodes.Find(n => n != null && !n.Deleted && n.Id == nodeId && n.Kind == AlgorithmNodeKind.Parameter);
            if (node == null) return false;
            node.Default = savedNode.Default.Copy();
            return Edit(id, expectedRevision, draft);
        }

        /// <summary>
        /// 按 <c>BindingKey</c> 更新草稿里某个端点（Input/Effector）的绑定（无则新增）。
        /// 是 <see cref="Edit"/> 的聚焦变体：只改 <c>Bindings</c> 一项三元组，绑定 <c>Type</c>
        /// 从对应节点（<c>BindingKey</c> 匹配且未删除）的 <c>ValueType</c> 派生，<c>Revision</c> 自增。
        /// </summary>
        public bool Rebind(ulong id, ulong expectedRevision, string bindingKey, ulong componentId, ulong targetId, ulong generation)
        {
            if (_disposed || string.IsNullOrEmpty(bindingKey) || !_entries.TryGetValue(id, out var entry) ||
                entry.Draft.Revision != expectedRevision || entry.Request?.State == AlgorithmApplyState.Applying) return false;

            AlgorithmDocument draft = entry.Draft.Copy();
            AlgorithmBinding binding = draft.Bindings.Find(b => b != null && b.Key == bindingKey);
            if (binding == null)
            {
                binding = new AlgorithmBinding { Key = bindingKey, Available = true };
                draft.Bindings.Add(binding);
            }

            binding.ComponentId = componentId;
            binding.TargetId = targetId;
            binding.Generation = generation;
            AlgorithmNode node = draft.Nodes.Find(n => n != null && !n.Deleted && n.BindingKey == bindingKey);
            if (node != null && node.ValueType != null)
            {
                binding.Type = node.ValueType.Copy();
            }

            draft.Revision = checked(expectedRevision + 1);
            entry.Draft = draft;
            Changed?.Invoke();
            return true;
        }

        /// <summary>移动草稿节点画布坐标（画布自由布局）。未找到节点时返回 false。</summary>
        public bool MoveNode(ulong id, ulong expectedRevision, ulong nodeId, float x, float y)
        {
            if (_disposed || !_entries.TryGetValue(id, out var entry) ||
                entry.Draft.Revision != expectedRevision || entry.Request?.State == AlgorithmApplyState.Applying) return false;

            AlgorithmDocument draft = entry.Draft.Copy();
            if (!draft.MoveNode(nodeId, x, y)) return false;

            entry.Draft = draft;
            Changed?.Invoke();
            return true;
        }

        /// <summary>
        /// 在草稿创建一个**最小默认节点**（节点库添加入口）。稳定节点 Id 由服务分配并通过 <c>nodeId</c> 返回，
        /// 调用方只选种类与画布坐标。种类未定义、修订不匹配或正在应用时返回 false 且草稿不变。
        /// 草稿可能含分配器视角之外的外部 Id（模板实例化、恢复、测试图）；分配时跳过草稿已占用的值，
        /// 保证新节点 Id 在文档内唯一（分配器单调递增、草稿 Id 集有限，循环必然终止）。
        /// </summary>
        public bool CreateNode(ulong id, ulong expectedRevision, AlgorithmNodeKind kind, float layoutX, float layoutY, out ulong nodeId)
        {
            nodeId = 0;
            if (_disposed || !Enum.IsDefined(typeof(AlgorithmNodeKind), kind) || !_entries.TryGetValue(id, out var entry) ||
                entry.Draft.Revision != expectedRevision || entry.Request?.State == AlgorithmApplyState.Applying) return false;

            AlgorithmNode node = AlgorithmCatalog.DefaultNode(kind);
            node.LayoutX = layoutX;
            node.LayoutY = layoutY;

            AlgorithmDocument draft = entry.Draft.Copy();
            while (_ids.TryAllocate(out var allocated))
            {
                bool collision = false;
                foreach (var existing in draft.Nodes)
                {
                    if (existing != null && existing.Id == allocated.Value) { collision = true; break; }
                }

                if (!collision)
                {
                    node.Id = allocated.Value;
                    break;
                }
            }

            if (node.Id == 0 || !draft.CreateNode(node)) return false;

            entry.Draft = draft;
            nodeId = node.Id;
            Changed?.Invoke();
            return true;
        }

        /// <summary>在草稿连接一对端口（强类型连线）。端口缺失、类型/单位/能力不兼容或目标输入已占用时返回 false。</summary>
        public bool Connect(ulong id, ulong expectedRevision, ulong from, string output, ulong to, string input)
        {
            if (_disposed || !_entries.TryGetValue(id, out var entry) ||
                entry.Draft.Revision != expectedRevision || entry.Request?.State == AlgorithmApplyState.Applying) return false;

            AlgorithmDocument draft = entry.Draft.Copy();
            if (!draft.Connect(from, output, to, input)) return false;

            entry.Draft = draft;
            Changed?.Invoke();
            return true;
        }

        /// <summary>按完整边身份断开草稿中的一条连线（精确断开，不影响其余边）；边不存在时返回 false。</summary>
        public bool Disconnect(ulong id, ulong expectedRevision, ulong from, string output, ulong to, string input)
        {
            if (_disposed || !_entries.TryGetValue(id, out var entry) ||
                entry.Draft.Revision != expectedRevision || entry.Request?.State == AlgorithmApplyState.Applying) return false;

            AlgorithmDocument draft = entry.Draft.Copy();
            if (!draft.Disconnect(from, output, to, input)) return false;

            entry.Draft = draft;
            Changed?.Invoke();
            return true;
        }

        /// <summary>删除草稿节点并级联断开其全部关联边（一次草稿变更，一次修订自增）。节点不存在或已删除时返回 false。</summary>
        public bool DeleteNode(ulong id, ulong expectedRevision, ulong nodeId)
        {
            if (_disposed || !_entries.TryGetValue(id, out var entry) ||
                entry.Draft.Revision != expectedRevision || entry.Request?.State == AlgorithmApplyState.Applying) return false;

            AlgorithmDocument draft = entry.Draft.Copy();
            if (!draft.RemoveNode(nodeId)) return false;

            entry.Draft = draft;
            Changed?.Invoke();
            return true;
        }

        public bool Apply(ulong id, ulong expectedDraft, ulong expectedApplied, ulong hardware, out AlgorithmApplyRequest request)
        {
            request = null;
            if (_disposed || !_entries.TryGetValue(id, out var entry) || entry.Runtime == null || IsPending(entry.Request) || entry.Draft.Revision != expectedDraft ||
                entry.Runtime.Revision != expectedApplied || hardware != _hardwareRevision() || expectedDraft <= expectedApplied || !_ids.TryAllocate(out var requestId)) return false;
            var pending = new AlgorithmApplyRequest { RequestId = requestId.Value, DraftRevision = expectedDraft, ExpectedAppliedRevision = expectedApplied,
                HardwareRevision = hardware, Document = entry.Draft.Copy(), State = AlgorithmApplyState.WaitingSafePoint };
            pending.ParametersOnly = SameStructure(entry.Runtime.CopyApplied(), pending.Document);
            entry.Request = pending;
            if (!Validate(entry, pending, out var compiled)) pending.State = pending.Reason == "WarningConfirmationRequired" ? AlgorithmApplyState.AwaitingWarningConfirmation : AlgorithmApplyState.Rejected;
            else pending.ParametersOnly = SameStructure(entry.Runtime.CopyApplied(), compiled.CopyDocument());
            request = pending.Copy(); Changed?.Invoke(); return true;
        }

        /// <summary>
        /// <see cref="Apply(ulong, ulong, ulong, ulong, out AlgorithmApplyRequest)"/> 的便捷重载：
        /// 硬件修订由服务内部取（<c>_hardwareRevision()</c>）。读模型等不持有硬件修订源的调用方用这个入口，
        /// 避免把硬件修订暴露到界面层。
        /// </summary>
        public bool Apply(ulong id, ulong expectedDraft, ulong expectedApplied, out AlgorithmApplyRequest request)
            => Apply(id, expectedDraft, expectedApplied, _hardwareRevision(), out request);
        public bool CancelApply(ulong id, ulong requestId)
        {
            if (!_entries.TryGetValue(id, out var entry) || entry.Request == null || entry.Request.RequestId != requestId || (entry.Request.State != AlgorithmApplyState.WaitingSafePoint && entry.Request.State != AlgorithmApplyState.AwaitingWarningConfirmation)) return false;
            entry.Request.State = AlgorithmApplyState.Cancelled; Changed?.Invoke(); return true;
        }
        public bool ConfirmWarnings(ulong id, ulong requestId)
        {
            if (!_entries.TryGetValue(id,out var entry) || entry.Request?.RequestId != requestId || entry.Request.State != AlgorithmApplyState.AwaitingWarningConfirmation) return false;
            entry.Request.WarningsConfirmed = true; entry.Request.Reason = null;
            if (!Validate(entry,entry.Request,out var plan)) { entry.Request.State=AlgorithmApplyState.Rejected;Changed?.Invoke();return false; }
            entry.Request.ParametersOnly=SameStructure(entry.Runtime.CopyApplied(),plan.CopyDocument());
            entry.Request.State=AlgorithmApplyState.WaitingSafePoint;Changed?.Invoke();return true;
        }
        public void Pump(long now)
        {
            if (_disposed) return;
            _now = now;
            if (_orderDirty)
            {
                _ordered.Clear();
                foreach (var entry in _entries.Values) _ordered.Add(entry);
                _ordered.Sort((a, b) => a.Draft.DocumentId.CompareTo(b.Draft.DocumentId));
                _orderDirty = false;
            }
            if (_publishing != null)
            {
                var entry = _publishing; var request = entry.Request;
                if (Validate(entry, request, out var plan) && entry.Runtime.Replace(plan, request.ParametersOnly))
                {
                    _compute.TryApplyLogicCost(TotalCost()); request.State = AlgorithmApplyState.Succeeded;
                    // Only explicit Startup nodes receive reapplication; never replay old behavior results.
                    foreach (var node in plan.CopyDocument().Nodes) if (!request.ParametersOnly && !node.Deleted && node.Kind == AlgorithmNodeKind.Startup)
                        entry.Runtime.Enqueue(new AlgorithmTrigger { NodeId = node.Id, Revision = plan.Revision, Generation = entry.Runtime.Generation, Time = now });
                }
                else { request.State = AlgorithmApplyState.Rejected; if (request.Reason == null) request.Reason = "SafePointLost"; }
                foreach (var runtime in _restartPaused) runtime.SetPaused(false, now, AlgorithmPauseReason.Application);
                _restartPaused.Clear(); _publishing = null;
                if(request.State==AlgorithmApplyState.Succeeded)PersistentApplied?.Invoke();
                Changed?.Invoke();
                return;
            }
            foreach (var entry in _ordered)
                if (_entries.TryGetValue(entry.Draft.DocumentId, out var current) && ReferenceEquals(entry, current)) entry.Runtime?.Pump(now);
            foreach (var entry in _ordered)
            {
                if (!_entries.TryGetValue(entry.Draft.DocumentId, out var current) || !ReferenceEquals(entry, current)) continue;
                var request = entry.Request;
                if (request?.State != AlgorithmApplyState.WaitingSafePoint) continue;
                if (!Validate(entry, request, out _)) { request.State = AlgorithmApplyState.Rejected; Changed?.Invoke(); continue; }
                bool safe = entry.Runtime?.IsSafe ?? false;
                if (!request.ParametersOnly)
                {
                    safe = safe && (_machineSafe?.Invoke() ?? true);
                    foreach (var other in _entries.Values) safe = safe && (other.Runtime?.IsSafe ?? true);
                }

                if (!safe) continue;
                request.State = AlgorithmApplyState.Applying; _publishing = entry;
                if (!request.ParametersOnly) foreach (var other in _entries.Values)
                    {
                        if (other.Runtime == null) continue;
                        other.Runtime.SetPaused(true, now, AlgorithmPauseReason.Application);
                        _restartPaused.Add(other.Runtime);
                    }

                Changed?.Invoke(); break;
            }
        }
        private bool Validate(Entry entry, AlgorithmApplyRequest request, out AlgorithmPlan plan)
        {
            plan = null;
            if (entry.Draft.Revision != request.DraftRevision || entry.Runtime.Revision != request.ExpectedAppliedRevision) { request.Reason = "StaleRevision"; return false; }
            if (_hardwareRevision() != request.HardwareRevision || !_bindingsValid(request.Document.Copy())) { request.Reason = "HardwareOrBindingChanged"; return false; }
            if (!AlgorithmValidator.TryCompile(request.Document, _compute.LogicCapacity - TotalCost() + entry.Runtime.LogicCost, out plan, out var issues))
            { request.Reason = issues[0].Code; return false; }
            if (!request.WarningsConfirmed && issues.Exists(issue=>issue.Severity==AlgorithmIssueSeverity.Warning)) { request.Reason="WarningConfirmationRequired";return false; }
            return true;
        }
        public bool Capture(ulong id, long now, out AlgorithmInstanceCheckpoint checkpoint)
        {
            checkpoint = null;
            if (_publishing != null || !_entries.TryGetValue(id, out var e) || e.Runtime == null || !e.Runtime.TryCapture(now, out var runtime)) return false;
            checkpoint = new AlgorithmInstanceCheckpoint { Runtime = runtime, Draft = e.Draft.Copy(), Saved = e.Saved.Copy(), Request = e.Request?.Copy() }; return true;
        }
        public bool Restore(ulong id, AlgorithmInstanceCheckpoint checkpoint, long now)
        {
            if (_publishing != null || checkpoint == null || !_entries.TryGetValue(id, out var e) || e.Runtime == null ||
                !_bindingsValid(checkpoint.Runtime.Applied.Copy()) ||
                !AlgorithmValidator.TryCompile(checkpoint.Runtime.Applied, _compute.LogicCapacity - TotalCost() + e.Runtime.LogicCost, out _, out _) ||
                !e.Runtime.Restore(checkpoint.Runtime, now)) return false;
            e.Draft = checkpoint.Draft.Copy(); e.Saved = checkpoint.Saved.Copy(); e.Request = checkpoint.Request?.Copy();
            if (e.Request != null) _ids.TryRestore(new PersistentId(e.Request.RequestId));
            _compute.TryApplyLogicCost(TotalCost()); return true;
        }
        private int TotalCost() { int sum = 0; foreach (var entry in _entries.Values) sum += entry.Runtime?.LogicCost ?? 0; return sum; }
        private static bool IsPending(AlgorithmApplyRequest request) => request != null && (request.State == AlgorithmApplyState.AwaitingWarningConfirmation || request.State == AlgorithmApplyState.WaitingSafePoint || request.State == AlgorithmApplyState.Applying);
        internal static bool SameStructure(AlgorithmDocument a, AlgorithmDocument b)
        {
            if (a.SchemaVersion != b.SchemaVersion || a.LanguageVersion != b.LanguageVersion || a.DocumentId != b.DocumentId || a.Nodes.Count != b.Nodes.Count || a.Edges.Count != b.Edges.Count || a.Bindings.Count != b.Bindings.Count) return false;
            for (int i = 0; i < a.Nodes.Count; i++)
            {
                var x = a.Nodes[i]; var y = b.Nodes[i];
                if (x == null || y == null || x.Id != y.Id || x.Kind != y.Kind || x.Operator != y.Operator || x.BindingKey != y.BindingKey || x.StateKey != y.StateKey || x.Deleted != y.Deleted || !SameType(x.ValueType,y.ValueType)) return false;
                if (x.Kind != AlgorithmNodeKind.Parameter && !SameValue(x.Default,y.Default)) return false;
            }
            for (int i = 0; i < a.Edges.Count; i++)
            { var x=a.Edges[i]; var y=b.Edges[i]; if(x==null||y==null||x.From!=y.From||x.To!=y.To||x.Output!=y.Output||x.Input!=y.Input)return false; }
            for (int i = 0; i < a.Bindings.Count; i++)
            { var x=a.Bindings[i]; var y=b.Bindings[i]; if(x==null||y==null||x.Key!=y.Key||x.ComponentId!=y.ComponentId||x.TargetId!=y.TargetId||x.Generation!=y.Generation||!SameType(x.Type,y.Type))return false; }
            return true;
        }
        private static bool SameType(AlgorithmType a,AlgorithmType b) => AlgorithmCatalog.Compatible(a,b)&&AlgorithmCatalog.Compatible(b,a);
        private static bool SameValue(AlgorithmValue a,AlgorithmValue b) => a==null ? b==null : b!=null && SameType(a.Type,b.Type)&&a.Number==b.Number&&a.Boolean==b.Boolean&&a.EnumValue==b.EnumValue&&a.ObjectId==b.ObjectId&&a.X==b.X&&a.Y==b.Y&&a.Z==b.Z&&a.IsValid==b.IsValid;
        public void Dispose()
        {
            if (_disposed) return; _disposed=true;
            foreach(var entry in _entries.Values)
                if (entry.Runtime != null) { entry.Runtime.Changed -= OnRuntimeChanged; entry.Runtime.Dispose(); }
            _entries.Clear(); _ordered.Clear(); _restartPaused.Clear(); _publishing = null;
            _compute.TryApplyLogicCost(0); Changed=null;PersistentApplied=null;
        }
        private sealed class Entry { internal AlgorithmRuntime Runtime; internal AlgorithmDocument Draft,Saved; internal AlgorithmApplyRequest Request; }
        private void OnRuntimeChanged() { if (!_disposed) Changed?.Invoke(); }
    }

    public sealed partial class AlgorithmTemplateLibrary
    {
        private readonly PersistentIdAllocator _ids;
        private readonly Dictionary<ulong,Template> _items=new Dictionary<ulong,Template>();
        public AlgorithmTemplateLibrary(PersistentIdAllocator ids) { _ids=ids ?? throw new ArgumentNullException(nameof(ids)); }
        public AlgorithmTemplateInfo[] List()
        {
            var result=new List<AlgorithmTemplateInfo>();
            foreach(var pair in _items) result.Add(new AlgorithmTemplateInfo(pair.Key,pair.Value.Version,pair.Value.Name,pair.Value.System));
            result.Sort((a,b)=>a.Id.CompareTo(b.Id));return result.ToArray();
        }
        public ulong CopyAsPlayer(ulong id,string name) => _items.TryGetValue(id,out var template) ? Save(name,template.Document,false) : 0;
        public ulong Save(string name,AlgorithmDocument source,bool system=false)
        {
            if(string.IsNullOrWhiteSpace(name)||!AlgorithmValidator.TryCompile(source,int.MaxValue,out _,out _,true)||!_ids.TryAllocate(out var id)) return 0;
            var copy=source.Copy(); copy.Bindings.Clear();
            foreach(var node in copy.Nodes) if(node.Default!=null && (node.Default.Type.Kind==AlgorithmValueKind.Object || node.Default.Type.Kind==AlgorithmValueKind.Objects))
            { node.Default.ObjectId=0; node.Default.IsValid=false; }
            _items.Add(id.Value,new Template { Name=name,Document=copy,System=system,Version=1 }); return id.Value;
        }
        public bool Rename(ulong id,ulong version,string name)
        { if(!_items.TryGetValue(id,out var t)||t.System||t.Version!=version||string.IsNullOrWhiteSpace(name))return false; t.Name=name;t.Version++;return true; }
        public bool Delete(ulong id,ulong version) => _items.TryGetValue(id,out var t)&&!t.System&&t.Version==version&&_items.Remove(id);
        public AlgorithmDocument Instantiate(ulong id)
        {
            if(!_items.TryGetValue(id,out var t)||!_ids.TryAllocate(out var documentId))return null;
            var copy=t.Document.Copy();copy.DocumentId=documentId.Value;copy.Revision=1;
            var map=new Dictionary<ulong,ulong>();
            foreach(var node in copy.Nodes) { if(!_ids.TryAllocate(out var nodeId))return null;map.Add(node.Id,nodeId.Value);node.Id=nodeId.Value; }
            foreach(var edge in copy.Edges) { edge.From=map[edge.From];edge.To=map[edge.To]; }
            return copy;
        }
        /// <summary>只读查询模板文档（不分配 ID、不复制绑定状态），供界面展示详情与逻辑成本。</summary>
        public bool TryGetDocument(ulong id, out AlgorithmDocument document)
        {
            if(_items.TryGetValue(id,out var template)) { document=template.Document; return true; }
            document=null; return false;
        }
        private sealed class Template { internal string Name;internal ulong Version;internal bool System;internal AlgorithmDocument Document; }
    }
    public sealed class AlgorithmTemplateInfo
    {
        public ulong Id { get; }
        public ulong Version { get; }
        public string Name { get; }
        public bool IsSystem { get; }
        internal AlgorithmTemplateInfo(ulong id,ulong version,string name,bool system) { Id=id;Version=version;Name=name;IsSystem=system; }
    }
}
