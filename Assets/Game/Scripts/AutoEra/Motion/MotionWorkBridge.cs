using System;
using AutoEra.Machines;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using UnityEngine;

namespace AutoEra.Motion
{
    /// <summary>Projects authoritative navigation/work lifecycle into one MotionExecutor owner.</summary>
    public sealed class MotionWorkBridge : IDisposable
    {
        private readonly MachineNavigation _navigation;
        private readonly MotionExecutor _executor;
        private readonly Adapter.MachineNavigationMotionAdapter _motion;
        private readonly string _executionId;
        private RegionWorkQueue _workQueue;
        private PersistentId _workMachine;
        private bool _disposed;

        public MotionExecutionState State { get; private set; } = MotionExecutionState.Prepared;
        public PersistentId TaskId => _navigation.CurrentTaskId;
        public bool IsActive => State == MotionExecutionState.Running;
        public WorkRequestState WorkState { get; private set; } = WorkRequestState.None;
        public WorkRequestResult RequestWork(RegionWorkQueue queue, PersistentId machine, Vector2 position, int priority = 0)
        {
            if (_disposed || queue == null || !machine.IsValid) return WorkRequestResult.InvalidRequester;
            var result = queue.Request(machine, position, priority);
            if (result == WorkRequestResult.Granted || result == WorkRequestResult.Waiting)
            { _workQueue = queue; _workMachine = machine; WorkState = result == WorkRequestResult.Granted ? WorkRequestState.Granted : WorkRequestState.Waiting; }
            return result;
        }
        public bool ReleaseWork(RegionWorkQueue queue, PersistentId machine)
        {
            if (_disposed || queue == null || !machine.IsValid) return false;
            bool released = queue.Release(machine);
            if (ReferenceEquals(queue, _workQueue) && machine == _workMachine)
            { _workQueue = null; _workMachine = PersistentId.Invalid; WorkState = WorkRequestState.None; }
            return released;
        }

        public MotionWorkBridge(MachineNavigation navigation, MotionExecutor executor,
            Adapter.MachineNavigationMotionAdapter motion, string executionId, System.Collections.Generic.IReadOnlyList<string> joints)
        {
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _motion = motion ?? throw new ArgumentNullException(nameof(motion));
            if (string.IsNullOrWhiteSpace(executionId) || joints == null) throw new ArgumentException("Motion execution identity is required.");
            _executionId = executionId; if (!_executor.TryPrepare(executionId, joints)) throw new InvalidOperationException("Motion channels are unavailable.");
            _navigation.Ended += OnEnded;
        }

        public bool Begin(PersistentId task)
        {
            if (_disposed || !task.IsValid || _navigation.CurrentTaskId != task || State != MotionExecutionState.Prepared) return false;
            if (!_executor.TryTransition(_executionId, MotionExecutionState.Prepared, MotionExecutionState.Running)) return false;
            State = MotionExecutionState.Running; return true;
        }

        public void Sample(UnityEngine.Vector3 position, float yaw)
        {
            if (!_disposed && State == MotionExecutionState.Running) _motion.Sample(position, yaw);
        }

        private void OnEnded(MachineNavigation navigation)
        {
            if (_disposed || navigation != _navigation || State != MotionExecutionState.Running) return;
            var next = navigation.Outcome == BehaviorOutcome.Completed ? MotionExecutionState.Completed : MotionExecutionState.Recovering;
            if (_executor.TryTransition(_executionId, MotionExecutionState.Running, next)) State = next;
        }

        public void Dispose()
        {
            if (_disposed) return; _disposed = true; _navigation.Ended -= OnEnded;
            if (_workQueue != null && _workMachine.IsValid) _workQueue.Release(_workMachine);
            _workQueue = null; _workMachine = PersistentId.Invalid; WorkState = WorkRequestState.None;
            if (State == MotionExecutionState.Running || State == MotionExecutionState.Recovering)
            { _executor.TryTransition(_executionId, State, MotionExecutionState.Cancelled); State = MotionExecutionState.Cancelled; }
            _executor.TryRelease(_executionId);
        }
    }
}
