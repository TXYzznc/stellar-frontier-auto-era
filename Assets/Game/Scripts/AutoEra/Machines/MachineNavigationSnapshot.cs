using System;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using UnityEngine;

namespace AutoEra.Machines
{
    public sealed class MachineNavigationTargetSnapshot
    {
        public PersistentId ObjectId;
        public Vector3[] Candidates;
        public float? FacingYaw;
        public Vector3? WaitingPosition;
        public bool HasApproachArea;
        public Vector2 ApproachPosition, ApproachSize, MaximumSize;
    }

    public sealed partial class MachineNavigationTarget
    {
        public MachineNavigationTargetSnapshot CapturePersistent() => new MachineNavigationTargetSnapshot { ObjectId = ObjectId,
            Candidates = (Vector3[])Candidates.Clone(), FacingYaw = FacingYaw, WaitingPosition = WaitingPosition,
            HasApproachArea = _area.HasValue, ApproachPosition = _area?.position ?? default,
            ApproachSize = _area?.size ?? default, MaximumSize = _maximumSize };

        /// <summary>Resolver rebuilds public ability checks. Missing objects keep their IDs and remain invalid targets.</summary>
        public static MachineNavigationTarget RestorePersistent(MachineNavigationTargetSnapshot snapshot, InitialRegion region,
            RegionWorkQueue queue = null, Func<MachineInstance, Vector3, bool> envelope = null)
        {
            if (snapshot == null || region == null || snapshot.Candidates == null || snapshot.Candidates.Length == 0 ||
                snapshot.Candidates.Length > 9 || snapshot.FacingYaw.HasValue && !Finite(snapshot.FacingYaw.Value) ||
                snapshot.WaitingPosition.HasValue && !MachineNavigationSettings.Finite(snapshot.WaitingPosition.Value)) return null;
            foreach (var position in snapshot.Candidates) if (!MachineNavigationSettings.Finite(position)) return null;
            if (snapshot.ObjectId.IsValid && !snapshot.HasApproachArea) return null;
            if (!snapshot.ObjectId.IsValid && (snapshot.HasApproachArea || snapshot.Candidates.Length != 1 || queue != null)) return null;
            if (snapshot.HasApproachArea && (!snapshot.ObjectId.IsValid || !MachineNavigationSettings.Finite(new Vector3(snapshot.ApproachPosition.x, 0, snapshot.ApproachPosition.y)) ||
                !MachineNavigationSettings.Positive(snapshot.ApproachSize.x) || !MachineNavigationSettings.Positive(snapshot.ApproachSize.y) ||
                !MachineNavigationSettings.Positive(snapshot.MaximumSize.x) || !MachineNavigationSettings.Positive(snapshot.MaximumSize.y))) return null;
            bool present = snapshot.ObjectId.IsValid && region.TryGet(snapshot.ObjectId, out _);
            if (present && (queue == null || !queue.BelongsTo(region, snapshot.ObjectId) || envelope == null)) return null;
            return new MachineNavigationTarget(snapshot, region, queue, envelope);
        }
        private MachineNavigationTarget(MachineNavigationTargetSnapshot snapshot, InitialRegion region, RegionWorkQueue queue,
            Func<MachineInstance, Vector3, bool> envelope)
        {
            Region = region; ObjectId = snapshot.ObjectId; WorkQueue = queue; Candidates = (Vector3[])snapshot.Candidates.Clone();
            FacingYaw = snapshot.FacingYaw; WaitingPosition = snapshot.WaitingPosition;
            _area = snapshot.HasApproachArea ? (Rect?)new Rect(snapshot.ApproachPosition, snapshot.ApproachSize) : null;
            _maximumSize = snapshot.MaximumSize; _envelope = envelope;
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }

    public sealed class MachineNavigationSnapshot
    {
        public bool Active, Reserved, WorkWaiting, WaitSettled, PathWarning, WorkWaitPrompt;
        public MachineNavigationState State;
        public BehaviorOutcome? Outcome;
        public PersistentId TaskId;
        public PersistentId PlanningLeaseId, MovingLeaseId;
        public WorkPriority Priority;
        public ulong Version;
        public int PlanCount;
        public Vector3 Position, ProgressPosition;
        public float Yaw;
        public double ProgressElapsed, PlanElapsed, BlockedElapsed, WorkElapsed;
        public MachineNavigationTargetSnapshot Target;
    }

    public sealed partial class MachineNavigation
    {
        private double _blockedElapsedCarry;
        public bool TryCapturePersistent(double now, out MachineNavigationSnapshot snapshot)
        {
            snapshot = null;
            if (_disposed || !ValidTime(now) || now < _now || !MachineNavigationSettings.Finite(_driver.Position) ||
                float.IsNaN(_driver.Yaw) || float.IsInfinity(_driver.Yaw)) return false;
            snapshot = new MachineNavigationSnapshot { Active = _active, Reserved = _reserved, WorkWaiting = _workWaiting, WaitSettled = _waitSettled,
                State = State, Outcome = Outcome, TaskId = _task, Priority = _priority, Version = _version, PlanCount = PlanCount,
                PlanningLeaseId = _planning?.Id ?? PersistentId.Invalid, MovingLeaseId = _moving?.Id ?? PersistentId.Invalid,
                Position = _driver.Position, Yaw = _driver.Yaw, ProgressPosition = _progressPosition,
                ProgressElapsed = Math.Max(0, now - _lastProgress), PlanElapsed = Math.Max(0, now - _lastPlan),
                BlockedElapsed = _blockedSince < 0 ? -1 : Math.Max(0, now - _blockedSince + _blockedElapsedCarry), WorkElapsed = Math.Max(0, now - _workSince),
                PathWarning = PathWarning, WorkWaitPrompt = WorkWaitPrompt, Target = _target?.CapturePersistent() };
            return true;
        }

        /// <summary>Bind at the saved pose and restore task/work queues first. Rebuilding a derived route does not start another activity.</summary>
        public bool RestorePersistent(MachineNavigationSnapshot snapshot, double now,
            Func<MachineNavigationTargetSnapshot, MachineNavigationTarget> resolveTarget)
        {
            if (_disposed || _active || _reserved || _target != null || _task.IsValid || State != MachineNavigationState.Idle ||
                snapshot == null || !ValidTime(now) || resolveTarget == null || !Enum.IsDefined(typeof(MachineNavigationState), snapshot.State) ||
                !Enum.IsDefined(typeof(WorkPriority), snapshot.Priority) || snapshot.PlanCount < 0 || snapshot.Version == ulong.MaxValue ||
                !MachineNavigationSettings.Finite(snapshot.Position) || !MachineNavigationSettings.Finite(snapshot.ProgressPosition) ||
                float.IsNaN(snapshot.Yaw) || float.IsInfinity(snapshot.Yaw) ||
                !ValidAge(snapshot.ProgressElapsed) || !ValidAge(snapshot.PlanElapsed) || !ValidAge(snapshot.WorkElapsed) ||
                snapshot.BlockedElapsed != -1 && !ValidAge(snapshot.BlockedElapsed) ||
                Vector3.Distance(_driver.Position, snapshot.Position) > .001f || Mathf.Abs(Mathf.DeltaAngle(_driver.Yaw, snapshot.Yaw)) > .001f) return false;
            if (snapshot.Outcome.HasValue && !Enum.IsDefined(typeof(BehaviorOutcome), snapshot.Outcome.Value)) return false;
            if (snapshot.Active && (snapshot.Outcome.HasValue || snapshot.Target == null || !snapshot.TaskId.IsValid ||
                snapshot.State == MachineNavigationState.Idle || snapshot.State == MachineNavigationState.Finished ||
                !_context.Tasks.TryGet(snapshot.TaskId, out var task) || task.Activities <= 0 ||
                task.State != MachineTaskState.Running && task.State != MachineTaskState.Paused && task.State != MachineTaskState.Cancelling)) return false;
            if (snapshot.Reserved && snapshot.Target == null) return false;
            var target = snapshot.Target == null ? null : resolveTarget(snapshot.Target);
            if (snapshot.Target != null && (target == null || target.ObjectId != snapshot.Target.ObjectId)) return false;
            if (snapshot.Reserved && target.IsValid && target.WorkQueue?.GetRequestState(_context.Machine.Id) != WorkRequestState.Granted &&
                target.WorkQueue?.GetRequestState(_context.Machine.Id) != WorkRequestState.Waiting) return false;
            ComputeRequest Lease(PersistentId id, int cost, ComputeClass category, ComputeMergeKind merge)
            {
                if (!id.IsValid) return null;
                return _context.Compute.TryFindPersistentRequest(id, out var request) && request.Source == _context.Machine.Id &&
                    request.Cost == cost && request.Class == category && request.MergeKind == merge && !request.CanYieldAtBoundary ? request : null;
            }
            var planning = Lease(snapshot.PlanningLeaseId, _settings.PlanningCompute, ComputeClass.Evaluation, ComputeMergeKind.Replan);
            var moving = Lease(snapshot.MovingLeaseId, _settings.MovingCompute, ComputeClass.Continuation, ComputeMergeKind.None);
            if (snapshot.PlanningLeaseId.IsValid && planning == null || snapshot.MovingLeaseId.IsValid && moving == null ||
                snapshot.PlanningLeaseId.IsValid && snapshot.PlanningLeaseId == snapshot.MovingLeaseId ||
                !snapshot.Active && (planning != null || moving != null)) return false;
            _task = snapshot.TaskId; _priority = snapshot.Priority; _version = snapshot.Version; _now = now;
            _lastProgress = now - snapshot.ProgressElapsed; _lastPlan = now - _settings.ReplanSeconds;
            _blockedSince = snapshot.BlockedElapsed < 0 ? -1 : now;
            _blockedElapsedCarry = Math.Max(0, snapshot.BlockedElapsed); _workSince = now - snapshot.WorkElapsed;
            _progressPosition = snapshot.ProgressPosition; _target = target;
            _waitingTarget = target?.WaitingPosition.HasValue == true ? new MachineNavigationTarget(target.Region, target.WaitingPosition.Value) : null;
            _active = snapshot.Active; _reserved = snapshot.Reserved; _workWaiting = snapshot.WorkWaiting; _waitSettled = snapshot.WaitSettled;
            _planning = planning; _moving = moving;
            Outcome = snapshot.Outcome; State = snapshot.State; PlanCount = snapshot.PlanCount;
            PathWarning = snapshot.PathWarning; WorkWaitPrompt = snapshot.WorkWaitPrompt; _needsPlan = _active;
            if (target != null) target.Region.ObjectRemoved += OnRemoved;
            _driver.Stop(); _context.SetNavigationActivity(this, _active);
            return true;
        }
        private static bool ValidAge(double value) => value >= 0 && !double.IsNaN(value) && !double.IsInfinity(value) && value <= long.MaxValue / 1000d;
    }
}
