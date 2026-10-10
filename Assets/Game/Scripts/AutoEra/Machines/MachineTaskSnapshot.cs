using System;
using System.Collections.Generic;
using AutoEra.Events;
using AutoEra.World.Identity;

namespace AutoEra.Machines
{
    public sealed class MachineTaskQueueSnapshot
    {
        public MachineWaitReason PauseReasons;
        public MachineTaskSnapshotRecord[] Live, History;
        public ulong[] WaitingOrder;
    }
    public sealed class MachineTaskSnapshotRecord
    {
        public ulong Id, Correlation;
        public string Name;
        public WorkPriority Priority;
        public MachineTaskState State, BeforePause;
        public MachineWaitReason WaitReasons;
        public int Activities;
        public bool Failed, ChainClosed;
    }
    public sealed partial class MachineTaskQueue
    {
        public MachineTaskQueueSnapshot Capture()
        {
            var live = new List<MachineTaskSnapshotRecord>(_live.Count);
            foreach (var value in _live.Values) live.Add(CopyRecord(value));
            live.Sort((a,b) => a.Id.CompareTo(b.Id));
            var history = new List<MachineTaskSnapshotRecord>(_history.Count);
            foreach (var value in _history) history.Add(CopyRecord(value));
            var waiting = new ulong[_waiting.Count];
            for (int i=0;i<waiting.Length;i++) waiting[i] = _waiting[i].Id.Value;
            return new MachineTaskQueueSnapshot { PauseReasons = _pauseReasons, Live = live.ToArray(), History = history.ToArray(), WaitingOrder = waiting };
        }
        public void Restore(MachineTaskQueueSnapshot snapshot)
        {
            if (_live.Count != 0 || _history.Count != 0 || _waiting.Count != 0) throw new InvalidOperationException("Task restoration requires an empty candidate.");
            if (snapshot == null || snapshot.Live == null || snapshot.History == null || snapshot.WaitingOrder == null ||
                snapshot.History.Length > 100 || snapshot.WaitingOrder.Length > WaitingCapacity || !ValidWaitReasons(snapshot.PauseReasons))
                throw new ArgumentException("Invalid task queue snapshot.");
            var identities = new HashSet<ulong>(); var live = new Dictionary<PersistentId,MachineTaskRecord>();
            var history = new List<MachineTaskRecord>(); var waiting = new List<MachineTaskRecord>();
            foreach (var value in snapshot.Live)
            {
                var task = ReadRecord(value,identities,false);
                bool paused = task.State == MachineTaskState.Paused;
                if (paused && (snapshot.PauseReasons == MachineWaitReason.None || (task.WaitReasons & snapshot.PauseReasons) != snapshot.PauseReasons) ||
                    !paused && task.State != MachineTaskState.Cancelling && snapshot.PauseReasons != MachineWaitReason.None ||
                    task.ChainClosed && task.Activities == 0)
                    throw new ArgumentException("Invalid live task phase.");
                live.Add(task.Id,task);
            }
            foreach (var value in snapshot.History) history.Add(ReadRecord(value,identities,true));
            var waitingIds = new HashSet<ulong>();
            foreach (ulong identity in snapshot.WaitingOrder)
            {
                if (!waitingIds.Add(identity) || !live.TryGetValue(new PersistentId(identity),out var task) ||
                    task.State != MachineTaskState.Queued && !(task.State == MachineTaskState.Paused && task.BeforePause == MachineTaskState.Queued))
                    throw new ArgumentException("Invalid waiting task reference.");
                waiting.Add(task);
            }
            foreach (var task in live.Values)
                if ((task.State == MachineTaskState.Queued || task.State == MachineTaskState.Paused && task.BeforePause == MachineTaskState.Queued) && !waitingIds.Contains(task.Id.Value))
                    throw new ArgumentException("Queued task missing from waiting order.");
            // All checks run before committing. Restore never publishes queued/started/ended facts.
            foreach (ulong identity in identities) _ids.TryRestore(new PersistentId(identity));
            foreach (var pair in live) _live.Add(pair.Key,pair.Value);
            foreach (var task in history) _history.Enqueue(task);
            _waiting.AddRange(waiting); _pauseReasons = snapshot.PauseReasons;
        }
        private static MachineTaskSnapshotRecord CopyRecord(MachineTaskRecord value) => new MachineTaskSnapshotRecord
        { Id=value.Id.Value, Correlation=value.Correlation.Value, Name=value.Name, Priority=value.Priority, State=value.State,
            BeforePause=value.BeforePause, WaitReasons=value.WaitReasons, Activities=value.Activities, Failed=value.Failed, ChainClosed=value.ChainClosed };
        private static bool ValidWaitReasons(MachineWaitReason value) => (value & ~(MachineWaitReason.Compute | MachineWaitReason.Effector | MachineWaitReason.SafePoint | MachineWaitReason.Power | MachineWaitReason.Connection | MachineWaitReason.Algorithm | MachineWaitReason.Resource)) == 0;
        private static MachineTaskRecord ReadRecord(MachineTaskSnapshotRecord value, HashSet<ulong> identities, bool history)
        {
            if (value == null || value.Id == 0 || !identities.Add(value.Id) || string.IsNullOrWhiteSpace(value.Name) ||
                !Enum.IsDefined(typeof(WorkPriority),value.Priority) || !Enum.IsDefined(typeof(MachineTaskState),value.State) ||
                !Enum.IsDefined(typeof(MachineTaskState),value.BeforePause) || !ValidWaitReasons(value.WaitReasons) || value.Activities < 0)
                throw new ArgumentException("Invalid task record.");
            bool terminal = value.State == MachineTaskState.Completed || value.State == MachineTaskState.Failed || value.State == MachineTaskState.Cancelled;
            if (history != terminal || history && (value.Activities != 0 || !value.ChainClosed) ||
                value.State == MachineTaskState.Paused && value.BeforePause != MachineTaskState.Queued && value.BeforePause != MachineTaskState.Running)
                throw new ArgumentException("Invalid task lifecycle.");
            return new MachineTaskRecord(new PersistentId(value.Id),value.Name,value.Priority)
            { Correlation=new CorrelationId(value.Correlation), State=value.State, BeforePause=value.BeforePause, WaitReasons=value.WaitReasons,
                Activities=value.Activities, Failed=value.Failed, ChainClosed=value.ChainClosed };
        }
    }
}
