using System;
using System.Collections.Generic;
using AutoEra.Machines;
using AutoEra.World.Identity;

namespace AutoEra.Algorithms
{
    [Flags]
    public enum AlgorithmPauseReason { User = 1, Machine = 2, Application = 4 }
    public interface IAlgorithmCommandSink
    {
        bool IsSafe { get; }
        ulong Submit(AlgorithmTrigger trigger, AlgorithmIntent intent);
        void EndBatch(AlgorithmTrigger trigger);
        void Cancel();
        /// <summary>读取本机货舱字段（DEC-111）：capacity/remaining/has_item。无实现时返回 false。</summary>
        bool TryReadCargo(string field, string itemType, out AlgorithmValue value);
        /// <summary>按名称查询本机未结束任务（DEC-123 显式防重）。无实现时返回 false。</summary>
        bool TryQueryTask(string name, out AlgorithmValue task);
    }
    public sealed class AlgorithmRunRecord
    {
        private readonly AlgorithmTrigger _trigger;
        private readonly ulong[] _path;
        public string Error { get; }
        public ulong FailedNode { get; }
        public int Cost { get; }
        public ulong RunId { get; }
        public AlgorithmTrigger CopyTrigger() => _trigger.Copy();
        public ulong[] CopyPath() => (ulong[])_path.Clone();
        internal AlgorithmRunRecord(ulong runId, AlgorithmTrigger trigger, AlgorithmBatch batch)
        { RunId = runId; _trigger = trigger.Copy(); _path = batch.Path.ToArray(); Error = batch.Error; FailedNode = batch.FailedNode; Cost = batch.Cost; }
    }

    /// <summary>Explicit world-clock pump; no frame-wide graph traversal and no world changes in evaluation.</summary>
    public sealed class AlgorithmRuntime : IDisposable
    {
        private AlgorithmPlan _plan;
        private AlgorithmEvaluation _evaluator;
        private readonly MachineComputePool _compute;
        private readonly IAlgorithmCommandSink _sink;
        private readonly PersistentId _instanceId;
        private readonly Queue<AlgorithmTrigger> _events = new Queue<AlgorithmTrigger>();
        private readonly Queue<AlgorithmRunRecord> _history = new Queue<AlgorithmRunRecord>();
        private readonly Dictionary<string, AlgorithmValue> _state = new Dictionary<string, AlgorithmValue>(StringComparer.Ordinal);
        private readonly Dictionary<string, ComputeRequest> _stateLeases = new Dictionary<string, ComputeRequest>(StringComparer.Ordinal);
        private readonly List<string> _newStateKeys = new List<string>();
        private ComputeRequest _lease;
        private AlgorithmTrigger _current;
        private AlgorithmBatch _batch;
        private bool _pumping, _disposed;
        private ulong _runSequence;
        private readonly List<Deferred> _delays = new List<Deferred>();
        public ulong Revision => _plan.Revision;
        public PersistentId InstanceId => _instanceId;
        public int LogicCost => _plan.LogicCost;
        public ulong Generation { get; private set; } = 1;
        public bool Invalid { get; private set; }
        private AlgorithmPauseReason _pauseReasons;
        public bool Paused => _pauseReasons != 0;
        private long _pauseAt;
        public string LastReason { get; private set; }
        public int WaitingCount => _events.Count;
        public bool IsSafe => !_pumping && _lease == null && _sink.IsSafe;
        public event Action Changed;
        public event Action AppliedChanged;
        public AlgorithmDocument CopyApplied() => _plan.CopyDocument();
        public bool HasTaskWork(ulong task)
        {
            if (_current != null && _current.TaskId == task) return true;
            foreach (var item in _events) if (item.TaskId == task) return true;
            foreach (var item in _delays) if (item.Trigger.TaskId == task) return true;
            return false;
        }
        public void SetPaused(bool paused, long now, AlgorithmPauseReason reason = AlgorithmPauseReason.User)
        {
            bool wasPaused = Paused;
            _pauseReasons = paused ? _pauseReasons | reason : _pauseReasons & ~reason;
            if (wasPaused == Paused) return;
            if (Paused) { _pauseAt = now; ReleaseResidents(); }
            else
            {
                long elapsed = Math.Max(0, now - _pauseAt);
                foreach (var delay in _delays) delay.Due = delay.Due > long.MaxValue - elapsed ? long.MaxValue : delay.Due + elapsed;
            }
        }

        public AlgorithmRuntime(PersistentId instanceId, AlgorithmPlan plan, MachineComputePool compute, IAlgorithmCommandSink sink)
        {
            if (!instanceId.IsValid) throw new ArgumentException(nameof(instanceId));
            _instanceId = instanceId; _plan = plan ?? throw new ArgumentNullException(nameof(plan));
            _compute = compute ?? throw new ArgumentNullException(nameof(compute)); _sink = sink ?? throw new ArgumentNullException(nameof(sink));
            _evaluator = NewEvaluator(plan);
        }
        private AlgorithmEvaluation NewEvaluator(AlgorithmPlan plan) => new AlgorithmEvaluation(plan,
            (field, itemType) => _sink.TryReadCargo(field, itemType, out var value) ? value : null,
            name => _sink.TryQueryTask(name, out var task) ? task : null);
        public bool Enqueue(AlgorithmTrigger trigger)
        {
            if (_disposed || Invalid || trigger == null || trigger.Revision != Revision || trigger.Generation != Generation) return false;
            if (trigger.Continuous)
            {
                bool replaced = false;
                int count = _events.Count;
                for (int i = 0; i < count; i++)
                {
                    var old = _events.Dequeue();
                    if (old.Continuous && old.NodeId == trigger.NodeId && old.Port == trigger.Port && old.SourceId == trigger.SourceId && old.BindingGeneration == trigger.BindingGeneration && old.TaskId == trigger.TaskId)
                    {
                        if (trigger.Sequence > old.Sequence) { old = trigger.Copy(); if (old.RootNodeId == 0) old.RootNodeId = old.NodeId; }
                        replaced = true;
                    }
                    _events.Enqueue(old);
                }
                if (replaced) return true;
            }
            if (_events.Count >= 32) { LastReason = "EventQueueFull"; Changed?.Invoke(); return false; }
            var snapshot = trigger.Copy(); if (snapshot.RootNodeId == 0) snapshot.RootNodeId = snapshot.NodeId;
            _events.Enqueue(snapshot); return true;
        }
        public AlgorithmRunRecord[] History() => _history.ToArray();
        public Dictionary<string, AlgorithmValue> CopyState()
        { var copy = new Dictionary<string, AlgorithmValue>(StringComparer.Ordinal); foreach (var item in _state) copy.Add(item.Key, item.Value.Copy()); return copy; }
        public void Pump(long now)
        {
            if (_disposed || _pumping || Paused || Invalid || now < 0) return;
            _pumping = true;
            try
            {
                if (!AcquireResidents(now)) { LastReason = "WaitingStateCompute"; return; }
                for (int i = 0; i < _delays.Count;)
                {
                    if (_delays[i].Due > now) { i++; continue; }
                    var ready = _delays[i]; _delays.RemoveAt(i); ReleaseLease(ready.Lease);
                    ready.Trigger.Time = now;
                    if (!Enqueue(ready.Trigger)) _sink.EndBatch(ready.Trigger);
                }
                if (_current == null)
                {
                    if (_events.Count == 0) return;
                    _current = _events.Dequeue();
                    if (_current.Generation != Generation || _current.Revision != Revision) { _current = null; return; }
                    _batch = _evaluator.Evaluate(_current, _state);
                    if (_batch.Error != null) { FinishBatch(); return; }
                    _newStateKeys.Clear();
                    foreach (var item in _batch.Writes) if (!_state.ContainsKey(item.Key)) _newStateKeys.Add(item.Key);
                    int extra = _newStateKeys.Count * 2;
                    foreach (var intent in _batch.Intents) if (intent.Kind == AlgorithmNodeKind.Delay) extra += 2;
                    int requested = _batch.Cost + extra;
                    if (requested + 2 * (_state.Count + _delays.Count) > _compute.Capacity)
                    { _batch.Error = "AtomicComputeExceedsCapacity"; FinishBatch(); return; }
                    var admission = _compute.Submit(requested, ComputeClass.Evaluation, WorkPriority.Normal, ComputeMergeKind.None,
                        _instanceId, _current.Sequence, now, false, out _lease);
                    if (admission == ComputeAdmission.ExceedsCapacity || admission == ComputeAdmission.Invalid)
                    { _batch.Error = "AtomicComputeExceedsCapacity"; FinishBatch(); return; }
                    if (admission == ComputeAdmission.Full) { _batch.Error = "ComputeQueueFull"; FinishBatch(); return; }
                }
                if (_lease == null || _lease.State != ComputeState.Running) { LastReason = "WaitingCompute"; return; }
                // Validate all engine-facing conversions before ANY state or command is committed.
                foreach (var intent in _batch.Intents)
                {
                    if (intent.Kind == AlgorithmNodeKind.Delay && intent.Value.Number * 1000 >= long.MaxValue - (double)now)
                    { _batch.Error = "DelayOverflow"; _batch.FailedNode = intent.NodeId; FinishBatch(); return; }
                    if (intent.Kind == AlgorithmNodeKind.Navigate && (Math.Abs(intent.Value.X) > float.MaxValue || Math.Abs(intent.Value.Y) > float.MaxValue || Math.Abs(intent.Value.Z) > float.MaxValue))
                    { _batch.Error = "PositionOverflow"; _batch.FailedNode = intent.NodeId; FinishBatch(); return; }
                }
                // Split before publishing: retaining state never competes for a second allocation.
                var retained = new List<ComputeRequest>();
                int retainCount = _newStateKeys.Count;
                foreach (var intent in _batch.Intents) if (intent.Kind == AlgorithmNodeKind.Delay) retainCount++;
                for (int i = 0; i < retainCount; i++)
                {
                    if (!_compute.TrySplitRunning(_lease.Id, 2, out var lease))
                    { foreach (var part in retained) ReleaseLease(part); _batch.Error = "StateLeaseUnavailable"; FinishBatch(); return; }
                    retained.Add(lease);
                }
                int retainedIndex = 0;
                foreach (string key in _newStateKeys) _stateLeases.Add(key, retained[retainedIndex++]);
                foreach (var item in _batch.Writes) _state[item.Key] = item.Value.Copy();
                int firstDelay = _delays.Count;
                foreach (var intent in _batch.Intents)
                {
                    if (intent.Kind == AlgorithmNodeKind.Delay)
                    {
                        var next = _current.Copy(); next.NodeId = intent.NodeId; next.Port = "event";
                        double milliseconds = intent.Value.Number * 1000;
                        _delays.Add(new Deferred { Due = now + (long)Math.Ceiling(milliseconds), Trigger = next, Lease = retained[retainedIndex++] });
                    }
                    else _current.TaskId = _sink.Submit(_current.Copy(), intent);
                }
                // Parallel branches of this root share the first world task, including earlier planned delays.
                for (int i = firstDelay; i < _delays.Count; i++) _delays[i].Trigger.TaskId = _current.TaskId;
                FinishBatch();
            }
            finally { _pumping = false; }
        }
        private void FinishBatch()
        {
            LastReason = _batch.Error;
            Invalid = _batch.Error != null && _batch.Error != "InputUnavailable" && _batch.Error != "ComputeQueueFull";
            if (_history.Count == 50) _history.Dequeue();
            _history.Enqueue(new AlgorithmRunRecord(++_runSequence, _current, _batch));
            var finished = _current;
            var lease = _lease; _lease = null; _current = null; _batch = null;
            if (lease != null) { if (lease.State == ComputeState.Running) _compute.Release(lease.Id); else _compute.CancelWaiting(lease.Id); }
            if (Invalid) { _events.Clear(); ReleaseResidents(); _delays.Clear(); _sink.Cancel(); }
            _sink.EndBatch(finished);
            Changed?.Invoke();
        }
        public bool Replace(AlgorithmPlan plan, bool preserveState)
        {
            if (_disposed || !IsSafe || plan == null || plan.Revision <= Revision) return false;
            _plan = plan; _evaluator = NewEvaluator(plan); Generation++; _events.Clear();
            if (!preserveState) { ReleaseResidents(); _state.Clear(); _delays.Clear(); }
            else foreach (var delay in _delays) { delay.Trigger.Revision = Revision; delay.Trigger.Generation = Generation; }
            Invalid = false; LastReason = null; AppliedChanged?.Invoke(); return true;
        }
        public bool TryCapture(long now, out AlgorithmMemorySnapshot snapshot)
        {
            snapshot = null;
            if (_disposed || !IsSafe || now < 0) return false;
            // Task/physical state belongs to its authority, not this DTO.
            foreach (var item in _events) if (item.TaskId != 0) return false;
            foreach (var item in _delays) if (item.Trigger.TaskId != 0) return false;
            snapshot = new AlgorithmMemorySnapshot { Instance = InstanceId.Value, Generation = Generation,
                Applied = CopyApplied(), State = CopyState(), Invalid = Invalid, Reason = LastReason, PauseReasons = _pauseReasons, History = History() };
            foreach (var item in _events) snapshot.Events.Add(item.Copy());
            foreach (var item in _delays)
            { snapshot.Timers.Add(item.Trigger.Copy()); snapshot.Remaining.Add(Math.Max(0, item.Due - (Paused ? _pauseAt : now))); }
            return true;
        }
        public bool Restore(AlgorithmMemorySnapshot snapshot, long now)
        {
            if (_disposed || !IsSafe || snapshot == null || snapshot.Instance != InstanceId.Value || now < 0 ||
                !AlgorithmValidator.TryCompile(snapshot.Applied, _compute.LogicCapacity, out var plan, out _)) return false;
            foreach (long remaining in snapshot.Remaining) if (remaining > long.MaxValue - now) return false;
            CancelPending(); _plan = plan; _evaluator = NewEvaluator(plan);
            Generation = Math.Max(Generation, snapshot.Generation) + 1;
            ReleaseResidents(); _state.Clear(); foreach (var item in snapshot.State) _state.Add(item.Key, item.Value.Copy());
            foreach (var item in snapshot.Events) { var copy = item.Copy(); copy.Generation = Generation; _events.Enqueue(copy); }
            for (int i = 0; i < snapshot.Timers.Count; i++)
            { var copy = snapshot.Timers[i].Copy(); copy.Generation = Generation; _delays.Add(new Deferred { Trigger = copy, Due = now + snapshot.Remaining[i] }); }
            _history.Clear(); foreach (var record in snapshot.History) { _history.Enqueue(record); _runSequence = Math.Max(_runSequence,record.RunId); }
            Invalid = snapshot.Invalid; LastReason = snapshot.Reason; _pauseReasons = snapshot.PauseReasons; _pauseAt = now;
            AppliedChanged?.Invoke();
            return true;
        }
        public void CancelPending()
        {
            _events.Clear(); ReleaseResidents(); _delays.Clear();
            if (_lease != null) { if (_lease.State == ComputeState.Running) _compute.Release(_lease.Id); else _compute.CancelWaiting(_lease.Id); }
            _lease = null; _current = null; _batch = null; Generation++; _sink.Cancel();
        }
        public void Dispose() { if (_disposed) return; CancelPending(); _disposed = true; Changed = null; AppliedChanged = null; }
        private void ReleaseLease(ComputeRequest lease)
        { if (lease == null) return; if (lease.State == ComputeState.Running) _compute.Release(lease.Id); else _compute.CancelWaiting(lease.Id); }
        private void ReleaseResidents()
        {
            foreach (var lease in _stateLeases.Values) ReleaseLease(lease);
            _stateLeases.Clear(); foreach (var delay in _delays) { ReleaseLease(delay.Lease); delay.Lease = null; }
        }
        private bool AcquireResidents(long now)
        {
            bool ready = true;
            foreach (var item in _state)
            {
                if (!_stateLeases.TryGetValue(item.Key, out var lease))
                { _compute.Submit(2, ComputeClass.Continuation, WorkPriority.Normal, ComputeMergeKind.None, InstanceId, Generation, now, false, out lease); if (lease != null) _stateLeases.Add(item.Key, lease); }
                ready &= lease != null && lease.State == ComputeState.Running;
            }
            foreach (var delay in _delays)
            {
                if (delay.Lease == null) _compute.Submit(2, ComputeClass.Continuation, WorkPriority.Normal, ComputeMergeKind.None, InstanceId, Generation, now, false, out delay.Lease);
                ready &= delay.Lease != null && delay.Lease.State == ComputeState.Running;
            }
            return ready;
        }
        private sealed class Deferred { internal long Due; internal AlgorithmTrigger Trigger; internal ComputeRequest Lease; }
    }
}
