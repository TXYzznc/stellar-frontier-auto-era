using System;
using System.Collections.Generic;
using AutoEra.World.Identity;

namespace AutoEra.Machines
{
    public enum ComputeClass { Safety, Continuation, Evaluation, Sampling }
    public enum ComputeAdmission { Started, Waiting, Merged, Full, ExceedsCapacity, Invalid }
    public enum ComputeMergeKind { None, SensorSample, ContinuousValue, RequestUpdate, Replan }
    public enum ComputeState { Waiting, Running, Finished, Cancelled }

    public sealed class ComputeRequest
    {
        public PersistentId Id { get; }
        public int Cost { get; internal set; }
        public ComputeClass Class { get; }
        public WorkPriority Priority { get; }
        public ComputeMergeKind MergeKind { get; }
        public PersistentId Source { get; }
        public ulong Version { get; internal set; }
        public ComputeState State { get; internal set; } = ComputeState.Waiting;
        public long EnqueuedAt { get; }
        public bool CanYieldAtBoundary { get; }
        internal ComputeRequest(PersistentId id, int cost, ComputeClass category, WorkPriority priority,
            ComputeMergeKind merge, PersistentId source, ulong version, long time, bool canYield)
        { Id = id; Cost = cost; Class = category; Priority = priority; MergeKind = merge; Source = source; Version = version; EnqueuedAt = time; CanYieldAtBoundary = canYield; }
    }

    /// <summary>Event-driven leases over the sum of installed cores. It does not evaluate algorithm graphs.</summary>
    public sealed class MachineComputePool
    {
        public const int WaitingCapacity = 64;
        private readonly PersistentIdAllocator _ids;
        private readonly List<ComputeRequest> _waiting = new List<ComputeRequest>();
        private readonly Dictionary<PersistentId, ComputeRequest> _running = new Dictionary<PersistentId, ComputeRequest>();
        private bool _dispatching;
        private bool _enabled = true;
        public int Capacity { get; private set; }
        public int LogicCapacity { get; private set; }
        public int AppliedLogicCost { get; private set; }
        public int Used { get; private set; }
        public int Available => Capacity - Used;
        public int WaitingCount => _waiting.Count;
        public event Action<ComputeRequest> Started;
        public event Action<MachineComputePool> Changed;
        public void SetDispatchEnabled(bool enabled)
        { if (_enabled == enabled) return; _enabled = enabled; Dispatch(); }
        public MachineComputePool(PersistentIdAllocator ids, int capacity, int logicCapacity)
        {
            _ids = ids ?? throw new ArgumentNullException(nameof(ids));
            if (capacity < 0 || logicCapacity < 0) throw new ArgumentOutOfRangeException();
            Capacity = capacity; LogicCapacity = logicCapacity;
        }
        public bool TryApplyLogicCost(int cost)
        { if (cost < 0 || cost > LogicCapacity) return false; AppliedLogicCost = cost; Changed?.Invoke(this); return true; }
        public bool TryReconfigure(int capacity, int logicCapacity)
        {
            if (capacity < Used || capacity < 0 || logicCapacity < AppliedLogicCost) return false;
            foreach (var request in _waiting) if (request.Cost > capacity) return false;
            Capacity = capacity; LogicCapacity = logicCapacity; Dispatch(); return true;
        }
        public ComputeAdmission Submit(int cost, ComputeClass category, WorkPriority priority, ComputeMergeKind merge,
            PersistentId source, ulong version, long worldMilliseconds, bool canYieldAtBoundary, out ComputeRequest request)
        {
            request = null;
            if (cost <= 0 || worldMilliseconds < 0 || (int)category < 0 || (int)category > 3 || (int)priority < 0 || (int)priority > 4 ||
                (int)merge < 0 || (int)merge > 4 || (merge != ComputeMergeKind.None && !source.IsValid)) return ComputeAdmission.Invalid;
            if (cost > Capacity) return ComputeAdmission.ExceedsCapacity;
            if (merge != ComputeMergeKind.None)
            {
                foreach (var pending in _waiting)
                    if (pending.MergeKind == merge && pending.Source == source && pending.Class == category && pending.Priority == priority)
                    {
                        if (version <= pending.Version) return ComputeAdmission.Invalid;
                        pending.Version = version; pending.Cost = cost; request = pending; Dispatch(); return ComputeAdmission.Merged;
                    }
            }
            if (_waiting.Count >= WaitingCapacity) return ComputeAdmission.Full;
            if (!_ids.TryAllocate(out var id)) return ComputeAdmission.Invalid;
            request = new ComputeRequest(id, cost, category, priority, merge, source, version, worldMilliseconds, canYieldAtBoundary);
            _waiting.Add(request); Dispatch();
            return request.State == ComputeState.Running ? ComputeAdmission.Started : ComputeAdmission.Waiting;
        }
        public bool Release(PersistentId id)
        {
            if (!_running.TryGetValue(id, out var request)) return false;
            _running.Remove(id); Used -= request.Cost; request.State = ComputeState.Finished; Dispatch(); return true;
        }
        /// <summary>Transfers part of an already granted atomic batch into a resident lease, without opening a dispatch gap.</summary>
        public bool TrySplitRunning(PersistentId id, int retainedCost, out ComputeRequest retained)
        {
            retained = null;
            if (!_running.TryGetValue(id, out var request) || retainedCost <= 0 || retainedCost >= request.Cost || !_ids.TryAllocate(out var next)) return false;
            retained = new ComputeRequest(next, retainedCost, ComputeClass.Continuation, request.Priority, ComputeMergeKind.None,
                request.Source, request.Version, request.EnqueuedAt, false) { State = ComputeState.Running };
            request.Cost -= retainedCost; _running.Add(next, retained); return true;
        }
        public bool CancelWaiting(PersistentId id)
        {
            for (int i = 0; i < _waiting.Count; i++) if (_waiting[i].Id == id)
            { _waiting[i].State = ComputeState.Cancelled; _waiting.RemoveAt(i); Dispatch(); return true; }
            return false;
        }
        public bool YieldAtBoundary(PersistentId id)
        {
            if (!_running.TryGetValue(id, out var current) || !current.CanYieldAtBoundary || _waiting.Count == 0 || _waiting.Count >= WaitingCapacity) return false;
            int next = BestIndex();
            if (!Higher(_waiting[next], current)) return false;
            _running.Remove(id); Used -= current.Cost; current.State = ComputeState.Waiting;
            _waiting.Add(current); Dispatch(); return true;
        }
        private static bool Higher(ComputeRequest left, ComputeRequest right)
            => left.Class < right.Class || (left.Class == right.Class && left.Priority > right.Priority);
        private int BestIndex()
        {
            int best = 0;
            for (int i = 1; i < _waiting.Count; i++) if (Higher(_waiting[i], _waiting[best])) best = i;
            return best;
        }
        private void Dispatch()
        {
            if (_dispatching) return;
            _dispatching = true;
            try
            {
                while (_enabled && _waiting.Count > 0)
                {
                    int index = BestIndex(); var next = _waiting[index];
                    if (next.Cost > Available) break; // Preserve priority/FIFO; no partial allocation.
                    _waiting.RemoveAt(index); next.State = ComputeState.Running;
                    Used += next.Cost; _running.Add(next.Id, next); Started?.Invoke(next);
                }
            }
            finally { _dispatching = false; Changed?.Invoke(this); }
        }
    }
}
