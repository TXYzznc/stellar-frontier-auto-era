using System;
using System.Collections.Generic;
using AutoEra.Events;
using AutoEra.World.Identity;
using GameFramework;

namespace AutoEra.Machines
{
    public enum WorkPriority { Lowest, Low, Normal, High, Urgent }
    public enum InterruptionRule { Immediate, SafePoint, CannotInterrupt }
    public enum BehaviorOutcome { Completed, Partial, Cancelled, Preempted, Failed, TargetInvalid }
    public enum QueueAdmission { Accepted, Full, InvalidTask, InvalidRequest }
    public enum MachineTaskState { Queued, Running, Paused, Cancelling, Completed, Failed, Cancelled }
    [Flags]
    public enum MachineWaitReason { None = 0, Compute = 1, Effector = 2, SafePoint = 4, Power = 8, Connection = 16, Algorithm = 32, Resource = 64 }

    public sealed class MachineTaskRecord
    {
        public PersistentId Id { get; }
        public string Name { get; }
        public WorkPriority Priority { get; }
        public CorrelationId Correlation { get; internal set; }
        public MachineTaskState State { get; internal set; } = MachineTaskState.Queued;
        public MachineWaitReason WaitReasons { get; internal set; }
        internal MachineTaskState BeforePause;
        internal int Activities;
        internal bool Failed;
        internal bool ChainClosed;
        internal MachineTaskRecord(PersistentId id, string name, WorkPriority priority) { Id = id; Name = name; Priority = priority; }
    }

    /// <summary>Task causality, not a global one-action lock. Several tasks may have active children.</summary>
    public sealed class MachineTaskQueue
    {
        public const int WaitingCapacity = 32;
        private readonly PersistentIdAllocator _ids;
        private readonly AutoEraEventService _events;
        private readonly PersistentId _source;
        private readonly List<MachineTaskRecord> _waiting = new List<MachineTaskRecord>();
        private readonly Dictionary<PersistentId, MachineTaskRecord> _live = new Dictionary<PersistentId, MachineTaskRecord>();
        private readonly Queue<MachineTaskRecord> _history = new Queue<MachineTaskRecord>();
        private MachineWaitReason _pauseReasons;
        public event Action<MachineTaskRecord> Ended;
        public event Action<MachineTaskRecord> CancellationRequested;
        public int WaitingCount => _waiting.Count;
        public IEnumerable<MachineTaskRecord> History => _history;
        public MachineTaskQueue(PersistentIdAllocator ids, AutoEraEventService events = null, PersistentId source = default)
        { _ids = ids ?? throw new ArgumentNullException(nameof(ids)); _events = events; _source = source; }
        public QueueAdmission Submit(string name, WorkPriority priority, out MachineTaskRecord task)
        {
            task = null;
            if (string.IsNullOrWhiteSpace(name) || (int)priority < 0 || (int)priority > 4) return QueueAdmission.InvalidRequest;
            if (_waiting.Count >= WaitingCapacity) return QueueAdmission.Full;
            if (!_ids.TryAllocate(out var id)) return QueueAdmission.InvalidRequest;
            task = new MachineTaskRecord(id, name, priority); _waiting.Add(task); _live.Add(id, task);
            if (_events != null)
            {
                task.Correlation = _events.OpenCommand(EventDomain.Task, _source, "task.submit");
                PublishTaskFact(task, MachineTaskState.Queued, false, EventOutcome.None);
            }
            if (_pauseReasons != MachineWaitReason.None)
            { task.BeforePause = MachineTaskState.Queued; task.State = MachineTaskState.Paused; task.WaitReasons = _pauseReasons; }
            return QueueAdmission.Accepted;
        }
        public MachineTaskRecord StartNext()
        {
            int selected = -1;
            for (int i = 0; i < _waiting.Count; i++)
                if (_waiting[i].State == MachineTaskState.Queued && (selected < 0 || _waiting[i].Priority > _waiting[selected].Priority)) selected = i;
            if (selected < 0) return null;
            var task = _waiting[selected]; _waiting.RemoveAt(selected); task.State = MachineTaskState.Running;
            PublishTaskFact(task, MachineTaskState.Running, false, EventOutcome.None);
            return task;
        }
        public bool TryStart(PersistentId id)
        {
            int selected = -1;
            for (int i = 0; i < _waiting.Count; i++)
                if (_waiting[i].State == MachineTaskState.Queued && (selected < 0 || _waiting[i].Priority > _waiting[selected].Priority)) selected = i;
            if (selected < 0 || _waiting[selected].Id != id) return false;
            var task = _waiting[selected]; _waiting.RemoveAt(selected); task.State = MachineTaskState.Running;
            PublishTaskFact(task, MachineTaskState.Running, false, EventOutcome.None);
            return true;
        }
        public bool TryGet(PersistentId id, out MachineTaskRecord task) => _live.TryGetValue(id, out task);
        /// <summary>按名称查询未结束任务（DEC-123 显式防重）；终止态（完成/失败/取消）不计。</summary>
        public bool TryFindActive(string name, out MachineTaskRecord task)
        {
            task = null;
            foreach (var candidate in _live.Values)
                if (candidate.Name == name && candidate.State != MachineTaskState.Completed && candidate.State != MachineTaskState.Failed && candidate.State != MachineTaskState.Cancelled)
                { task = candidate; return true; }
            return false;
        }
        public bool AddActivity(PersistentId id)
        {
            if (!_live.TryGetValue(id, out var task) || task.State != MachineTaskState.Running || task.ChainClosed) return false;
            task.Activities = checked(task.Activities + 1); return true;
        }
        public void EndActivity(PersistentId id, bool unhandledFailure, bool unhandledCancellation = false)
        {
            if (!_live.TryGetValue(id, out var task) || task.Activities == 0) throw new InvalidOperationException("Unmatched task activity.");
            task.Activities--; task.Failed |= unhandledFailure;
            if (unhandledCancellation) Cancel(id);
            TryFinish(task);
        }
        public void CloseChain(PersistentId id)
        { if (_live.TryGetValue(id, out var task)) { task.ChainClosed = true; TryFinish(task); } }
        public void Cancel(PersistentId id)
        {
            if (!_live.TryGetValue(id, out var task) || task.State == MachineTaskState.Cancelling) return;
            task.State = MachineTaskState.Cancelling; task.ChainClosed = true; _waiting.Remove(task);
            try { CancellationRequested?.Invoke(task); }
            finally { TryFinish(task); }
        }
        public void SetPaused(bool paused, MachineWaitReason reason)
        {
            if (reason == MachineWaitReason.None) return;
            if (paused) _pauseReasons |= reason;
            else _pauseReasons &= ~reason;
            foreach (var task in _live.Values)
            {
                if (task.State == MachineTaskState.Cancelling) continue;
                if (paused) task.WaitReasons |= reason;
                else task.WaitReasons &= ~reason;
                if (_pauseReasons != MachineWaitReason.None && task.State != MachineTaskState.Paused)
                { task.BeforePause = task.State; task.State = MachineTaskState.Paused; }
                else if (_pauseReasons == MachineWaitReason.None && task.State == MachineTaskState.Paused)
                    task.State = task.BeforePause;
            }
        }
        private void TryFinish(MachineTaskRecord task)
        {
            if (!task.ChainClosed || task.Activities != 0 || !_live.ContainsKey(task.Id)) return;
            task.State = task.State == MachineTaskState.Cancelling ? MachineTaskState.Cancelled : task.Failed ? MachineTaskState.Failed : MachineTaskState.Completed;
            _live.Remove(task.Id); _waiting.Remove(task);
            if (_history.Count == 100) _history.Dequeue();
            _history.Enqueue(task); Ended?.Invoke(task);
            EventOutcome outcome = task.State == MachineTaskState.Completed ? EventOutcome.Succeeded
                : task.State == MachineTaskState.Failed ? EventOutcome.Failed : EventOutcome.Cancelled;
            PublishTaskFact(task, task.State, true, outcome);
        }
        private void PublishTaskFact(MachineTaskRecord task, MachineTaskState state, bool terminal, EventOutcome outcome)
        {
            if (_events == null || !task.Correlation.IsValid) return;
            string action = state == MachineTaskState.Queued ? "task.queued"
                : state == MachineTaskState.Running ? "task.started" : "task.ended";
            var fact = ReferencePool.Acquire<MachineTaskFactEventArgs>();
            fact.Initialize(task.Correlation, _source, task.Id, task.Name, task.Priority, state, action, terminal, outcome);
            _events.PublishFact(fact);
        }
    }

    public sealed class BehaviorRequest<TParameters>
    {
        public PersistentId Id { get; }
        public PersistentId TaskId { get; }
        public PersistentId AlgorithmId { get; }
        public PersistentId NodeId { get; }
        public PersistentObjectReference Target { get; }
        public WorkPriority Priority { get; }
        public InterruptionRule Interruption { get; }
        public TParameters Parameters { get; }
        public BehaviorOutcome? Outcome { get; internal set; }
        public bool FailureHandled { get; private set; }
        public void MarkFailureHandled() { FailureHandled = true; }
        internal bool CancellationRequested;
        internal BehaviorRequest(PersistentId id, PersistentId task, PersistentId algorithm, PersistentId node,
            PersistentObjectReference target, WorkPriority priority, InterruptionRule interruption, TParameters parameters)
        { Id = id; TaskId = task; AlgorithmId = algorithm; NodeId = node; Target = target; Priority = priority; Interruption = interruption; Parameters = parameters; }
    }

    /// <summary>
    /// 第一版效应器行为统一参数：算法层桥接到效应器队列的弱类型载体。
    /// 强类型动作参数契约（每种动作专属 struct）在效应器行为契约变更中定义；这里先承载动作类型、目标与数值参数，
    /// 使「算法图 → 效应器队列」链路可接通，效应器物理执行器独立消费 <see cref="BehaviorRequest{TParameters}"/>。
    /// </summary>
    public sealed class EffectorBehaviorParameters
    {
        public string Action;
        public PersistentObjectReference Target;
        public readonly Dictionary<string, double> Numbers = new Dictionary<string, double>();
        public int Enumeration;
        public bool Flag;
        public EffectorBehaviorParameters(string action) { Action = action; }
    }

    /// <summary>One installed active effector. The parameter type belongs to that action, never to an algorithm graph.</summary>
    public sealed class EffectorBehaviorQueue<TParameters>
    {
        public const int WaitingCapacity = 16;
        private readonly PersistentIdAllocator _ids;
        private readonly MachineTaskQueue _tasks;
        private readonly List<BehaviorRequest<TParameters>> _waiting = new List<BehaviorRequest<TParameters>>();
        private bool _dispatching;
        private bool _dispatchEnabled = true;
        private bool _canAdvanceCurrent = true;
        private bool _detached;
        public BehaviorRequest<TParameters> Current { get; private set; }
        public int WaitingCount => _waiting.Count;
        public bool CanExecuteCurrent => _canAdvanceCurrent && Current != null;
        public bool SafeStopRequested => !_dispatchEnabled && _canAdvanceCurrent && Current != null;
        public void SetDispatchEnabled(bool enabled) => SetExecutionPermission(enabled, enabled);
        public void SetExecutionPermission(bool startNew, bool advanceCurrent)
        { _dispatchEnabled = startNew; _canAdvanceCurrent = advanceCurrent; if (startNew) StartNext(); }
        public event Action<BehaviorRequest<TParameters>> Started;
        public event Action<BehaviorRequest<TParameters>> Ended;
        public EffectorBehaviorQueue(PersistentIdAllocator ids, MachineTaskQueue tasks)
        {
            _ids = ids ?? throw new ArgumentNullException(nameof(ids)); _tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
            _tasks.CancellationRequested += OnTaskCancellationRequested;
        }
        public bool TryDetach()
        {
            if (Current != null || _waiting.Count != 0) return false;
            _tasks.CancellationRequested -= OnTaskCancellationRequested;
            _detached = true;
            return true;
        }
        public void InvalidateUnstarted()
        {
            _dispatchEnabled = false;
            while (_waiting.Count > 0)
            {
                var request = _waiting[0]; _waiting.RemoveAt(0);
                Publish(request, BehaviorOutcome.TargetInvalid);
            }
        }
        private void OnTaskCancellationRequested(MachineTaskRecord task)
        {
            // Remove each before publishing, so callbacks cannot observe or complete it twice.
            for (int i = 0; i < _waiting.Count;)
            {
                if (_waiting[i].TaskId != task.Id) { i++; continue; }
                var request = _waiting[i]; _waiting.RemoveAt(i); Publish(request, BehaviorOutcome.Cancelled);
                i = 0;
            }
            if (Current != null && Current.TaskId == task.Id) Cancel(Current.Id);
        }

        public QueueAdmission Submit(PersistentId task, PersistentId algorithm, PersistentId node, PersistentObjectReference target,
            WorkPriority priority, InterruptionRule interruption, TParameters parameters, out BehaviorRequest<TParameters> request)
        {
            request = null;
            if (_detached) return QueueAdmission.InvalidRequest;
            if ((int)priority < 0 || (int)priority > 4 || (int)interruption < 0 || (int)interruption > 2) return QueueAdmission.InvalidRequest;
            if (_waiting.Count >= WaitingCapacity) return QueueAdmission.Full;
            if (!_tasks.TryGet(task, out var record) || record.State != MachineTaskState.Running) return QueueAdmission.InvalidTask;
            if (!_ids.TryAllocate(out var id)) return QueueAdmission.InvalidRequest;
            if (!_tasks.AddActivity(task)) return QueueAdmission.InvalidTask;
            request = new BehaviorRequest<TParameters>(id, task, algorithm, node, target, priority, interruption, parameters);
            _waiting.Add(request);
            if (_dispatchEnabled && Current != null && priority > Current.Priority && Current.Interruption == InterruptionRule.Immediate)
                Finish(BehaviorOutcome.Preempted);
            else StartNext();
            return QueueAdmission.Accepted;
        }
        public void Cancel(PersistentId id)
        {
            if (Current != null && Current.Id == id)
            {
                Current.CancellationRequested = true;
                if (Current.Interruption == InterruptionRule.Immediate) Finish(BehaviorOutcome.Cancelled);
                return;
            }
            for (int i = 0; i < _waiting.Count; i++) if (_waiting[i].Id == id)
            { var request = _waiting[i]; _waiting.RemoveAt(i); Publish(request, BehaviorOutcome.Cancelled); return; }
        }
        public void ReachSafePoint()
        {
            if (Current == null || Current.Interruption == InterruptionRule.CannotInterrupt) return;
            if (Current.CancellationRequested) { Finish(BehaviorOutcome.Cancelled); return; }
            if (SafeStopRequested) { Finish(BehaviorOutcome.Partial); return; }
            foreach (var request in _waiting)
                if (request.Priority > Current.Priority && _tasks.TryGet(request.TaskId, out var task) && task.State == MachineTaskState.Running)
                { Finish(BehaviorOutcome.Preempted); return; }
        }
        public bool Finish(BehaviorOutcome outcome)
        {
            if ((int)outcome < 0 || (int)outcome > 5) throw new ArgumentOutOfRangeException(nameof(outcome));
            if (Current == null) return false;
            var request = Current; Current = null; Publish(request, outcome); StartNext(); return true;
        }
        private void Publish(BehaviorRequest<TParameters> request, BehaviorOutcome outcome)
        {
            if (request.Outcome.HasValue) return;
            request.Outcome = outcome;
            // The receiving algorithm may register a continuation before this activity closes.
            try { Ended?.Invoke(request); }
            finally { _tasks.EndActivity(request.TaskId, !request.FailureHandled && (outcome == BehaviorOutcome.Failed || outcome == BehaviorOutcome.TargetInvalid || outcome == BehaviorOutcome.Preempted),
                !request.FailureHandled && outcome == BehaviorOutcome.Cancelled); }
        }
        public void Pump() => StartNext();
        private void StartNext()
        {
            if (!_dispatchEnabled || _dispatching || Current != null || _waiting.Count == 0) return;
            _dispatching = true;
            try
            {
            // Task cancellation is independent of player queue capacity and drains unstarted work.
            for (int i = _waiting.Count - 1; i >= 0; i--)
                if (_tasks.TryGet(_waiting[i].TaskId, out var pending) && pending.State == MachineTaskState.Cancelling)
                { var cancelled = _waiting[i]; _waiting.RemoveAt(i); Publish(cancelled, BehaviorOutcome.Cancelled); }
            int selected = -1;
            for (int i = 0; i < _waiting.Count; i++)
                if (_tasks.TryGet(_waiting[i].TaskId, out var candidate) && candidate.State == MachineTaskState.Running &&
                    (selected < 0 || _waiting[i].Priority > _waiting[selected].Priority)) selected = i;
            if (selected < 0) return;
            var next = _waiting[selected];
            _waiting.RemoveAt(selected); Current = next; Started?.Invoke(next);
            }
            finally { _dispatching = false; }
            // A synchronous executor may have finished inside Started.
            if (Current == null && _waiting.Count > 0) StartNext();
        }
    }
}
