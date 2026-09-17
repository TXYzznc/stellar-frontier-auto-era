using AutoEra.Events;
using AutoEra.World.Identity;
using GameFramework;

namespace AutoEra.Machines
{
    /// <summary>Task lifecycle fact: queued, started or a terminal ended state.</summary>
    public sealed class MachineTaskFactEventArgs : AutoEraFactEventArgs
    {
        public static readonly int EventId = typeof(MachineTaskFactEventArgs).GetHashCode();
        public override int Id => EventId;

        public PersistentId TaskId { get; private set; }
        public string TaskName { get; private set; }
        public WorkPriority Priority { get; private set; }
        public MachineTaskState State { get; private set; }

        internal MachineTaskFactEventArgs Initialize(CorrelationId correlation, PersistentId machineId, PersistentId taskId,
            string taskName, WorkPriority priority, MachineTaskState state, string action, bool terminal, EventOutcome outcome)
        {
            Initialize(EventDomain.Task, correlation, machineId, action, terminal, outcome);
            TaskId = taskId;
            TaskName = taskName;
            Priority = priority;
            State = state;
            return this;
        }

        public override void Clear()
        {
            base.Clear();
            TaskId = default;
            TaskName = null;
            Priority = default;
            State = default;
        }
    }
}