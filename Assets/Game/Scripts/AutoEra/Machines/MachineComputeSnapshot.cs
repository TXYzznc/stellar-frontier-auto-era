using System;
using System.Collections.Generic;
using AutoEra.World.Identity;

namespace AutoEra.Machines
{
    public sealed class ComputeRequestSnapshot
    {
        public PersistentId Id, Source;
        public int Cost;
        public ComputeClass Class;
        public WorkPriority Priority;
        public ComputeMergeKind MergeKind;
        public ComputeState State;
        public ulong Version;
        public long EnqueuedAt;
        public bool CanYieldAtBoundary;
        internal static ComputeRequestSnapshot Capture(ComputeRequest value) => new ComputeRequestSnapshot { Id = value.Id, Source = value.Source,
            Cost = value.Cost, Class = value.Class, Priority = value.Priority, MergeKind = value.MergeKind, State = value.State,
            Version = value.Version, EnqueuedAt = value.EnqueuedAt, CanYieldAtBoundary = value.CanYieldAtBoundary };
    }
    public sealed class MachineComputeSnapshot
    {
        public int Capacity, LogicCapacity, AppliedLogicCost;
        public bool DispatchEnabled;
        public ComputeRequestSnapshot[] Running, Waiting;
    }
    public sealed partial class MachineComputePool
    {
        internal void DiscardFailedPersistentCandidate()
        {
            if(_dispatching || _enabled) throw new InvalidOperationException("Disable an unpublished candidate before releasing unbound reservations.");
            foreach(var request in _running.Values) request.State=ComputeState.Cancelled;
            foreach(var request in _waiting) request.State=ComputeState.Cancelled;
            _running.Clear();_waiting.Clear();Used=0;AppliedLogicCost=0;
        }
        public bool TryFindPersistentRequest(PersistentId id, out ComputeRequest request)
        {
            if (_running.TryGetValue(id, out request)) return true;
            foreach (var item in _waiting) if (item.Id == id) { request = item; return true; }
            request = null; return false;
        }
        public bool TryCapturePersistent(out MachineComputeSnapshot snapshot)
        {
            snapshot = null; if (_dispatching) return false;
            var running = new List<ComputeRequestSnapshot>(_running.Count); foreach (var item in _running.Values) running.Add(ComputeRequestSnapshot.Capture(item));
            running.Sort((a, b) => a.Id.CompareTo(b.Id));
            var waiting = new ComputeRequestSnapshot[_waiting.Count]; for (int i = 0; i < waiting.Length; i++) waiting[i] = ComputeRequestSnapshot.Capture(_waiting[i]);
            snapshot = new MachineComputeSnapshot { Capacity = Capacity, LogicCapacity = LogicCapacity, AppliedLogicCost = AppliedLogicCost,
                DispatchEnabled = _enabled, Running = running.ToArray(), Waiting = waiting }; return true;
        }
        public bool RestorePersistent(MachineComputeSnapshot snapshot, long now)
        {
            if (_dispatching || _running.Count != 0 || _waiting.Count != 0 || AppliedLogicCost != 0 || snapshot == null ||
                snapshot.Capacity != Capacity || snapshot.LogicCapacity != LogicCapacity || snapshot.AppliedLogicCost < 0 || snapshot.AppliedLogicCost > LogicCapacity ||
                snapshot.Running == null || snapshot.Waiting == null || snapshot.Waiting.Length > WaitingCapacity || now < 0) return false;
            var identities = new HashSet<PersistentId>(); var merges = new HashSet<(ComputeClass, WorkPriority, ComputeMergeKind, PersistentId)>();
            ComputeRequest Build(ComputeRequestSnapshot row, ComputeState expected)
            {
                if (row == null || !row.Id.IsValid || !identities.Add(row.Id) || row.Cost <= 0 || row.Cost > Capacity || row.State != expected ||
                    row.EnqueuedAt < 0 || row.EnqueuedAt > now || !Enum.IsDefined(typeof(ComputeClass), row.Class) ||
                    !Enum.IsDefined(typeof(WorkPriority), row.Priority) || !Enum.IsDefined(typeof(ComputeMergeKind), row.MergeKind) ||
                    row.MergeKind != ComputeMergeKind.None && !row.Source.IsValid) return null;
                if (expected == ComputeState.Waiting && row.MergeKind != ComputeMergeKind.None && !merges.Add((row.Class, row.Priority, row.MergeKind, row.Source))) return null;
                return new ComputeRequest(row.Id, row.Cost, row.Class, row.Priority, row.MergeKind, row.Source, row.Version, row.EnqueuedAt, row.CanYieldAtBoundary) { State = expected };
            }
            var running = new List<ComputeRequest>(snapshot.Running.Length); var waiting = new List<ComputeRequest>(snapshot.Waiting.Length); long used = 0;
            foreach (var row in snapshot.Running) { var request = Build(row, ComputeState.Running); if (request == null) return false; used += request.Cost; running.Add(request); }
            if (used > Capacity) return false;
            foreach (var row in snapshot.Waiting) { var request = Build(row, ComputeState.Waiting); if (request == null) return false; waiting.Add(request); }
            foreach (var request in running) _running.Add(request.Id, request);
            _waiting.AddRange(waiting); Used = (int)used; AppliedLogicCost = snapshot.AppliedLogicCost; _enabled = snapshot.DispatchEnabled;
            foreach (var id in identities) _ids.TryRestore(id);
            // Owners attach their restored references before normal dispatch resumes; no Started/Changed is replayed.
            return true;
        }
    }
}
