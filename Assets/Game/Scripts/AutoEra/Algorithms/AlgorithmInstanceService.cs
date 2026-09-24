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
    public sealed class AlgorithmInstanceService : IDisposable
    {
        private readonly PersistentIdAllocator _ids;
        private readonly MachineComputePool _compute;
        private readonly Func<ulong> _hardwareRevision;
        private readonly Func<AlgorithmDocument, bool> _bindingsValid;
        private readonly Dictionary<ulong, Entry> _entries = new Dictionary<ulong, Entry>();
        private readonly List<AlgorithmRuntime> _restartPaused = new List<AlgorithmRuntime>();
        private Entry _publishing;
        private bool _disposed;
        public event Action Changed;
        public AlgorithmInstanceService(PersistentIdAllocator ids, MachineComputePool compute, Func<ulong> hardwareRevision, Func<AlgorithmDocument, bool> bindingsValid)
        { _ids = ids ?? throw new ArgumentNullException(nameof(ids)); _compute = compute ?? throw new ArgumentNullException(nameof(compute));
            _hardwareRevision = hardwareRevision ?? throw new ArgumentNullException(nameof(hardwareRevision));
            _bindingsValid = bindingsValid ?? throw new ArgumentNullException(nameof(bindingsValid)); }
        public bool Add(AlgorithmRuntime runtime)
        {
            if (_disposed || runtime == null || _entries.ContainsKey(runtime.InstanceId.Value) || !runtime.IsSafe) return false;
            int cost = TotalCost() + runtime.LogicCost;
            if (!_compute.TryApplyLogicCost(cost)) return false;
            var doc = runtime.CopyApplied(); _entries.Add(runtime.InstanceId.Value, new Entry { Runtime = runtime, Draft = doc, Saved = doc.Copy() });
            // 新增实例同样是一次状态变化，必须发事件：否则订阅者（界面的读模型）会一直停在
            // 「这台机器没有实例」上，直到别的操作恰好触发一次 Changed。
            // 这条曾经漏过一次：数据对了、界面却是旧状态，看起来像界面没接线。
            Changed?.Invoke();
            return true;
        }
        public AlgorithmDocument ReadDraft(ulong id) => _entries[id].Draft.Copy();
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
                    entry.Runtime.Revision,
                    entry.Draft.Revision,
                    entry.Saved.Revision,
                    entry.Runtime.LogicCost,
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
        public bool Apply(ulong id, ulong expectedDraft, ulong expectedApplied, ulong hardware, out AlgorithmApplyRequest request)
        {
            request = null;
            if (_disposed || !_entries.TryGetValue(id, out var entry) || IsPending(entry.Request) || entry.Draft.Revision != expectedDraft ||
                entry.Runtime.Revision != expectedApplied || hardware != _hardwareRevision() || expectedDraft <= expectedApplied || !_ids.TryAllocate(out var requestId)) return false;
            var pending = new AlgorithmApplyRequest { RequestId = requestId.Value, DraftRevision = expectedDraft, ExpectedAppliedRevision = expectedApplied,
                HardwareRevision = hardware, Document = entry.Draft.Copy(), State = AlgorithmApplyState.WaitingSafePoint };
            pending.ParametersOnly = SameStructure(entry.Runtime.CopyApplied(), pending.Document);
            entry.Request = pending;
            if (!Validate(entry, pending, out var compiled)) pending.State = pending.Reason == "WarningConfirmationRequired" ? AlgorithmApplyState.AwaitingWarningConfirmation : AlgorithmApplyState.Rejected;
            else pending.ParametersOnly = SameStructure(entry.Runtime.CopyApplied(), compiled.CopyDocument());
            request = pending.Copy(); Changed?.Invoke(); return true;
        }
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
                _restartPaused.Clear(); _publishing = null; Changed?.Invoke();
                return;
            }
            foreach (var entry in _entries.Values) entry.Runtime.Pump(now);
            foreach (var entry in _entries.Values)
            {
                var request = entry.Request;
                if (request?.State != AlgorithmApplyState.WaitingSafePoint) continue;
                if (!Validate(entry, request, out _)) { request.State = AlgorithmApplyState.Rejected; Changed?.Invoke(); continue; }
                bool safe = entry.Runtime.IsSafe;
                if (!request.ParametersOnly) foreach (var other in _entries.Values) safe &= other.Runtime.IsSafe;
                if (!safe) continue;
                request.State = AlgorithmApplyState.Applying; _publishing = entry;
                if (!request.ParametersOnly) foreach (var other in _entries.Values)
                    { other.Runtime.SetPaused(true, now, AlgorithmPauseReason.Application); _restartPaused.Add(other.Runtime); }
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
            if (_publishing != null || !_entries.TryGetValue(id, out var e) || !e.Runtime.TryCapture(now, out var runtime)) return false;
            checkpoint = new AlgorithmInstanceCheckpoint { Runtime = runtime, Draft = e.Draft.Copy(), Saved = e.Saved.Copy(), Request = e.Request?.Copy() }; return true;
        }
        public bool Restore(ulong id, AlgorithmInstanceCheckpoint checkpoint, long now)
        {
            if (_publishing != null || checkpoint == null || !_entries.TryGetValue(id, out var e) ||
                !_bindingsValid(checkpoint.Runtime.Applied.Copy()) ||
                !AlgorithmValidator.TryCompile(checkpoint.Runtime.Applied, _compute.LogicCapacity - TotalCost() + e.Runtime.LogicCost, out _, out _) ||
                !e.Runtime.Restore(checkpoint.Runtime, now)) return false;
            e.Draft = checkpoint.Draft.Copy(); e.Saved = checkpoint.Saved.Copy(); e.Request = checkpoint.Request?.Copy();
            if (e.Request != null) _ids.TryRestore(new PersistentId(e.Request.RequestId));
            _compute.TryApplyLogicCost(TotalCost()); return true;
        }
        private int TotalCost() { int sum = 0; foreach (var entry in _entries.Values) sum += entry.Runtime.LogicCost; return sum; }
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
            foreach(var entry in _entries.Values) entry.Runtime.Dispose();
            _entries.Clear(); _compute.TryApplyLogicCost(0); Changed=null;
        }
        private sealed class Entry { internal AlgorithmRuntime Runtime; internal AlgorithmDocument Draft,Saved; internal AlgorithmApplyRequest Request; }
    }

    public sealed class AlgorithmTemplateLibrary
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
