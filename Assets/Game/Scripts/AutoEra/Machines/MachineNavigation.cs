using System;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using UnityEngine;

namespace AutoEra.Machines
{
    /// <summary>One carrier actuator. Task causality and compute remain owned by the execution context.</summary>
    public sealed class MachineNavigation : IDisposable
    {
        private readonly MachineExecutionContext _context;
        private readonly IMachineNavigationDriver _driver;
        private readonly MachineNavigationSettings _settings;
        private MachineNavigationTarget _target;
        private MachineNavigationTarget _waitingTarget;
        private bool _workWaiting, _waitSettled;
        private PersistentId _task;
        private WorkPriority _priority;
        private ComputeRequest _planning, _moving;
        private Vector3 _destination, _progressPosition;
        private double _now, _lastProgress, _lastPlan, _blockedSince = -1, _workSince;
        private bool _active, _disposed, _needsPlan, _reserved;
        private ulong _version;
        public MachineNavigationState State { get; private set; }
        public BehaviorOutcome? Outcome { get; private set; }
        public bool PathWarning { get; private set; }
        public bool WorkWaitPrompt { get; private set; }
        public bool IsActive => _active;
        public PersistentId CurrentTaskId => _task;
        public Vector3 Position => _driver.Position;
        public int PlanCount { get; private set; }
        public event Action<MachineNavigation> Ended;

        public MachineNavigation(MachineExecutionContext context, IMachineNavigationDriver driver, MachineNavigationSettings settings)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings)); settings.Validate();
            if (!context.TryBindNavigation(this)) throw new InvalidOperationException("Carrier navigation already bound.");
            context.Tasks.CancellationRequested += OnTaskCancelled;
            context.Machine.Changed += OnMachineChanged;
        }

        public NavigationAdmission Start(PersistentId task, MachineNavigationTarget target, double now)
        {
            if (_disposed || target == null || !ValidTime(now) || !target.IsValid ||
                !target.Region.TryGetMachine(_context.Machine.Id, out var machine) || !ReferenceEquals(machine, _context.Machine)) return NavigationAdmission.Invalid;
            if (!machine.Definition.CanMove) return NavigationAdmission.Immobile;
            if (_active || _reserved) return NavigationAdmission.Busy;
            if (!_context.Tasks.TryGet(task, out var record) || !_context.Tasks.AddActivity(task)) return NavigationAdmission.Invalid;
            DetachTarget();
            _target = target; _task = task; _priority = record.Priority; _now = now; _lastProgress = now;
            _waitingTarget = target.WaitingPosition.HasValue ? new MachineNavigationTarget(target.Region, target.WaitingPosition.Value) : null;
            _workWaiting = false; _waitSettled = false;
            _lastPlan = now - _settings.ReplanSeconds; _workSince = now; _blockedSince = -1;
            _progressPosition = _driver.Position; _active = true; _needsPlan = true;
            Outcome = null; PathWarning = false; WorkWaitPrompt = false; PlanCount = 0;
            State = target.WorkQueue == null ? MachineNavigationState.Planning : MachineNavigationState.WaitingWork;
            target.Region.ObjectRemoved += OnRemoved;
            _context.SetNavigationActivity(this, true);
            _driver.Stop();
            return NavigationAdmission.Accepted;
        }

        public void Tick(double now, float deltaSeconds)
        {
            if (!ValidTime(now) || now < _now || !MachineNavigationSettings.Finite(new Vector3(deltaSeconds, 0, 0)) || deltaSeconds < 0)
                throw new ArgumentOutOfRangeException(nameof(now));
            _now = now;
            if (!_active) return;
            if (!_target.IsValid || !_target.Region.TryGetMachine(_context.Machine.Id, out _) || !_context.Machine.Deployed)
            { Finish(BehaviorOutcome.TargetInvalid); return; }
            if (!_context.Machine.CanRun)
            { Pause(); return; }
            if (State == MachineNavigationState.Paused)
            { _lastProgress = now; _blockedSince = -1; _needsPlan = true; }

            if (_target.WorkQueue != null)
            {
                var queue = _target.WorkQueue;
                var status = queue.GetRequestState(_context.Machine.Id);
                if (status == WorkRequestState.None)
                {
                    queue.Request(_context.Machine.Id, queue.WorkArea.center, (int)_priority);
                    status = queue.GetRequestState(_context.Machine.Id);
                }
                if (status == WorkRequestState.InvalidRequester || status == WorkRequestState.InvalidTarget)
                { Finish(BehaviorOutcome.TargetInvalid); return; }
                _reserved = true;
                if (status != WorkRequestState.Granted)
                {
                    _workWaiting = true;
                    WorkWaitPrompt = now - _workSince >= _settings.WorkWaitPromptSeconds;
                    if (_waitingTarget == null || _waitSettled)
                    { _driver.Stop(); State = MachineNavigationState.WaitingWork; return; }
                }
                else
                {
                    if (_workWaiting)
                    {
                        _driver.Stop(); _workWaiting = false; _waitSettled = false; _needsPlan = true;
                        Release(ref _planning); _lastPlan = now - _settings.ReplanSeconds; _blockedSince = -1;
                    }
                    WorkWaitPrompt = false;
                }
            }

            if (_blockedSince >= 0 && !_workWaiting)
            {
                PathWarning = now - _blockedSince >= _settings.WarningSeconds;
                if (now - _blockedSince >= _settings.FailureSeconds) { Finish(BehaviorOutcome.Failed); return; }
            }
            if (_needsPlan)
            {
                _driver.Stop(); State = MachineNavigationState.WaitingCompute;
                if (now - _lastPlan < _settings.ReplanSeconds) return;
                if (!Acquire(ref _planning, _settings.PlanningCompute, ComputeClass.Evaluation, ComputeMergeKind.Replan)) return;
                State = MachineNavigationState.Planning; _lastPlan = now; PlanCount++;
                bool valid;
                try { valid = _driver.TryPlan(_workWaiting ? _waitingTarget : _target, _context.Machine, out _destination); }
                finally { Release(ref _planning); }
                if (!valid)
                { if (_workWaiting) SettleWaiting(); else Block(now); return; }
                _needsPlan = false; _lastProgress = now; _progressPosition = _driver.Position;
            }
            if (!Acquire(ref _moving, _settings.MovingCompute, ComputeClass.Continuation, ComputeMergeKind.None))
            { _driver.Stop(); State = MachineNavigationState.WaitingCompute; return; }

            Vector3 position = _driver.Position;
            if (!_target.Region.TryUpdateMachinePose(_context.Machine.Id, new Vector2(position.x, position.z), _driver.Yaw))
            { _driver.Stop(); if (_workWaiting) SettleWaiting(); else { Block(now); _needsPlan = true; } return; }
            if (Vector3.Distance(position, _destination) <= _settings.ArrivalDistance)
            {
                _driver.Stop(); State = MachineNavigationState.Aligning;
                if (_driver.Speed > _settings.StoppedSpeed) return;
                if (_workWaiting) { SettleWaiting(); return; }
                if (_target.FacingYaw.HasValue)
                {
                    _driver.Align(_target.FacingYaw.Value, _settings.AngularSpeed, deltaSeconds);
                    if (Mathf.Abs(Mathf.DeltaAngle(_driver.Yaw, _target.FacingYaw.Value)) > _settings.FacingTolerance) return;
                }
                if (!_target.Allows(_context.Machine, _driver.Position)) { Finish(BehaviorOutcome.TargetInvalid); return; }
                _target.Region.TryUpdateMachinePose(_context.Machine.Id, new Vector2(position.x, position.z), _driver.Yaw);
                Finish(BehaviorOutcome.Completed); return;
            }
            if (!_driver.PathValid)
            { _driver.Stop(); if (_workWaiting) SettleWaiting(); else { Block(now); _needsPlan = true; } return; }
            if (Vector3.Distance(position, _progressPosition) >= _settings.EffectiveDisplacement)
            {
                _progressPosition = position; _lastProgress = now; _blockedSince = -1; PathWarning = false;
            }
            else if (now - _lastProgress >= _settings.ReplanSeconds)
            { _driver.Stop(); if (_workWaiting) SettleWaiting(); else { Block(_lastProgress); _needsPlan = true; } return; }
            State = MachineNavigationState.Moving; _driver.Resume();
        }

        public void Cancel() { if (_active) Finish(BehaviorOutcome.Cancelled); else ReleaseWorkReservation(); }
        public void Preempt() { if (_active) Finish(BehaviorOutcome.Preempted); else ReleaseWorkReservation(); }
        public void InvalidateRoute()
        {
            _driver.Stop();
            if (_active) { _needsPlan = true; State = MachineNavigationState.Planning; }
        }
        public void ReleaseWorkReservation()
        {
            if (_reserved) { _reserved = false; _target?.WorkQueue?.Release(_context.Machine.Id); }
            if (!_active) DetachTarget();
        }
        private bool Acquire(ref ComputeRequest request, int cost, ComputeClass category, ComputeMergeKind merge)
        {
            if (request == null)
                _context.Compute.Submit(cost, category, _priority, merge, _context.Machine.Id,
                    ++_version, (long)(_now * 1000), false, out request);
            return request != null && request.State == ComputeState.Running;
        }
        private void Release(ref ComputeRequest request)
        {
            var owned = request; request = null;
            if (owned == null) return;
            if (owned.State == ComputeState.Waiting) _context.Compute.CancelWaiting(owned.Id);
            else if (owned.State == ComputeState.Running) _context.Compute.Release(owned.Id);
        }
        private void Block(double since) { if (_blockedSince < 0) _blockedSince = since; }
        private void SettleWaiting()
        { _driver.Stop(); _waitSettled = true; State = MachineNavigationState.WaitingWork; Release(ref _moving); }
        private void Pause()
        {
            _driver.Stop(); _needsPlan = true; State = MachineNavigationState.Paused;
            Release(ref _planning); Release(ref _moving);
        }
        private void Finish(BehaviorOutcome outcome)
        {
            if (!_active) return;
            _active = false; Outcome = outcome; State = MachineNavigationState.Finished;
            _driver.Stop(); Release(ref _planning); Release(ref _moving);
            if (outcome != BehaviorOutcome.Completed) ReleaseWorkReservation();
            if (!_reserved) DetachTarget();
            _context.SetNavigationActivity(this, false);
            _context.Tasks.EndActivity(_task, outcome == BehaviorOutcome.Failed || outcome == BehaviorOutcome.TargetInvalid || outcome == BehaviorOutcome.Preempted,
                outcome == BehaviorOutcome.Cancelled);
            Ended?.Invoke(this);
        }
        private void OnTaskCancelled(MachineTaskRecord record) { if (record.Id == _task) Cancel(); }
        private void OnMachineChanged(MachineInstance machine)
        {
            if (_active && (!machine.Deployed || machine.Integrity <= 0)) Finish(BehaviorOutcome.TargetInvalid);
            else if (_active && machine.RequestedRunState != MachineRunState.Running) Finish(BehaviorOutcome.Partial);
            else if (_active && !machine.CanRun) Pause();
        }
        private void OnRemoved(PersistentId id)
        {
            if (_target != null && (id == _context.Machine.Id || id == _target.ObjectId))
            { if (_active) Finish(BehaviorOutcome.TargetInvalid); else ReleaseWorkReservation(); }
        }
        private void DetachTarget()
        { if (_target != null) _target.Region.ObjectRemoved -= OnRemoved; _target = null; }
        private static bool ValidTime(double now) => now >= 0 && !double.IsNaN(now) && !double.IsInfinity(now) && now <= long.MaxValue / 1000d;
        public void Dispose()
        {
            if (_disposed) return;
            Cancel(); ReleaseWorkReservation(); DetachTarget(); _disposed = true;
            _context.Machine.Changed -= OnMachineChanged; _context.Tasks.CancellationRequested -= OnTaskCancelled;
            _context.UnbindNavigation(this); Ended = null;
        }
    }
}
