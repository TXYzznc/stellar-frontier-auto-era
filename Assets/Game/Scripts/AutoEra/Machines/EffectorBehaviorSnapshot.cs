using System;
using System.Collections.Generic;
using AutoEra.World.Identity;

namespace AutoEra.Machines
{
    public sealed class BehaviorRequestSnapshot<TParameters>
    {
        public PersistentId Id, TaskId, AlgorithmId, NodeId;
        public PersistentObjectReference Target;
        public WorkPriority Priority;
        public InterruptionRule Interruption;
        public TParameters Parameters;
        public bool FailureHandled, CancellationRequested;
    }

    public sealed class EffectorBehaviorSnapshot<TParameters>
    {
        public BehaviorRequestSnapshot<TParameters> Current;
        public BehaviorRequestSnapshot<TParameters>[] Waiting;
        public bool DispatchEnabled, CanAdvanceCurrent;
    }

    public static class EffectorParameterSnapshot
    {
        public static EffectorBehaviorParameters Copy(EffectorBehaviorParameters source)
        {
            if (source == null) return null;
            var copy = new EffectorBehaviorParameters(source.Action) { TransferMode = source.TransferMode, Target = source.Target,
                Enumeration = source.Enumeration, Flag = source.Flag };
            foreach (var pair in source.Numbers) copy.Numbers.Add(pair.Key, pair.Value);
            foreach (var pair in source.Objects) copy.Objects.Add(pair.Key, pair.Value);
            return copy;
        }
        public static bool Valid(EffectorBehaviorParameters source)
        {
            if (source == null || !Enum.TryParse(source.Action, out Algorithms.AlgorithmEffectorAction action) ||
                !Enum.IsDefined(typeof(Algorithms.AlgorithmEffectorAction), action)) return false;
            foreach (var pair in source.Numbers)
                if (string.IsNullOrEmpty(pair.Key) || double.IsNaN(pair.Value) || double.IsInfinity(pair.Value)) return false;
            foreach (var pair in source.Objects) if (string.IsNullOrEmpty(pair.Key)) return false;
            return true;
        }
    }

    public sealed partial class EffectorBehaviorQueue<TParameters>
    {
        internal bool TryFindPersistentBehavior(PersistentId id, out BehaviorRequest<TParameters> request)
        {
            request = null;
            if (_detached || _dispatching || !id.IsValid) return false;
            if (Current?.Id == id) { request=Current; return true; }
            foreach (var candidate in _waiting) if (candidate.Id == id) { request=candidate; return true; }
            return false;
        }

        public bool TryCapturePersistent(Func<TParameters, TParameters> copyParameters, out EffectorBehaviorSnapshot<TParameters> snapshot)
        {
            snapshot = null;
            if (_detached || _dispatching || copyParameters == null) return false;
            BehaviorRequestSnapshot<TParameters> Copy(BehaviorRequest<TParameters> request) => request == null ? null :
                new BehaviorRequestSnapshot<TParameters> { Id = request.Id, TaskId = request.TaskId, AlgorithmId = request.AlgorithmId,
                    NodeId = request.NodeId, Target = request.Target, Priority = request.Priority, Interruption = request.Interruption,
                    Parameters = copyParameters(request.Parameters), FailureHandled = request.FailureHandled, CancellationRequested = request.CancellationRequested };
            var waiting = new BehaviorRequestSnapshot<TParameters>[_waiting.Count];
            for (int i = 0; i < waiting.Length; i++) waiting[i] = Copy(_waiting[i]);
            snapshot = new EffectorBehaviorSnapshot<TParameters> { Current = Copy(Current), Waiting = waiting,
                DispatchEnabled = _dispatchEnabled, CanAdvanceCurrent = _canAdvanceCurrent };
            return true;
        }

        /// <summary>Restore after the task queue, without adding activities or emitting Started/Ended.</summary>
        public bool RestorePersistent(EffectorBehaviorSnapshot<TParameters> snapshot, Func<TParameters, bool> validateParameters,
            Func<TParameters, TParameters> copyParameters)
        {
            if (_detached || _dispatching || Current != null || _waiting.Count != 0 || snapshot?.Waiting == null ||
                snapshot.Waiting.Length > WaitingCapacity || validateParameters == null || copyParameters == null) return false;
            var identities = new HashSet<PersistentId>();
            BehaviorRequest<TParameters> Build(BehaviorRequestSnapshot<TParameters> row, bool current)
            {
                if (row == null || !row.Id.IsValid || !row.TaskId.IsValid || !identities.Add(row.Id) ||
                    !Enum.IsDefined(typeof(WorkPriority), row.Priority) || !Enum.IsDefined(typeof(InterruptionRule), row.Interruption) ||
                    !validateParameters(row.Parameters) || !current && row.CancellationRequested ||
                    !_tasks.TryGet(row.TaskId, out var task) || task.Activities <= 0 ||
                    task.State != MachineTaskState.Running && task.State != MachineTaskState.Paused && task.State != MachineTaskState.Cancelling) return null;
                var request = new BehaviorRequest<TParameters>(row.Id, row.TaskId, row.AlgorithmId, row.NodeId, row.Target,
                    row.Priority, row.Interruption, copyParameters(row.Parameters)) { CancellationRequested = row.CancellationRequested };
                if (row.FailureHandled) request.MarkFailureHandled();
                return request;
            }
            var active = snapshot.Current == null ? null : Build(snapshot.Current, true);
            if (snapshot.Current != null && active == null) return false;
            var waiting = new List<BehaviorRequest<TParameters>>(snapshot.Waiting.Length);
            foreach (var row in snapshot.Waiting) { var request = Build(row, false); if (request == null) return false; waiting.Add(request); }
            var activityCounts = new Dictionary<PersistentId, int>();
            if (active != null) activityCounts.Add(active.TaskId, 1);
            foreach (var request in waiting) { activityCounts.TryGetValue(request.TaskId, out int count); activityCounts[request.TaskId] = count + 1; }
            foreach (var pair in activityCounts) if (!_tasks.TryGet(pair.Key, out var task) || pair.Value > task.Activities) return false;
            Current = active; _waiting.AddRange(waiting); _dispatchEnabled = snapshot.DispatchEnabled; _canAdvanceCurrent = snapshot.CanAdvanceCurrent;
            foreach (var id in identities) _ids.TryRestore(id);
            return true;
        }
    }
}
