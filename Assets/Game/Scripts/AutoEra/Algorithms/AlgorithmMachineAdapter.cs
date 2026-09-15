using System;
using System.Collections.Generic;
using AutoEra.Machines;
using AutoEra.Machines.Sensors;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using UnityEngine;

namespace AutoEra.Algorithms
{
    /// <summary>Consumes existing authorities. No movement, sensor readings or results are simulated here.</summary>
    public sealed class AlgorithmMachineAdapter : IAlgorithmCommandSink, IDisposable
    {
        private readonly MachineExecutionContext _context;
        private readonly MachineNavigation _navigation;
        private readonly InitialRegion _region;
        private readonly List<Pending> _waiting = new List<Pending>();
        private readonly List<SensorSubscription> _sensors = new List<SensorSubscription>();
        private readonly List<MachineSensor> _registeredSensors = new List<MachineSensor>();
        private readonly HashSet<PersistentId> _ownedTasks = new HashSet<PersistentId>();
        private Pending _active;
        private AlgorithmRuntime _runtime;
        private bool _disposed;
        private long _now;
        public bool IsSafe => _active == null && _waiting.Count == 0 && !_navigation.IsActive;
        public event Action<ulong, PersistentId, string> Result;
        public AlgorithmMachineAdapter(MachineExecutionContext context, MachineNavigation navigation, InitialRegion region)
        {
            _context = context; _navigation = navigation; _region = region;
            _navigation.Ended += OnNavigationEnded;
        }
        public void Attach(AlgorithmRuntime runtime)
        {
            if (_runtime != null) throw new InvalidOperationException("Already attached.");
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _runtime.AppliedChanged += RebindApplied;
            _context.Machine.Changed += OnMachineChanged;
            OnMachineChanged(_context.Machine);
        }
        private void OnMachineChanged(MachineInstance machine) => _runtime?.SetPaused(!machine.CanRun, _now, AlgorithmPauseReason.Machine);
        private static bool Matches(MachineSensor sensor, AlgorithmBinding binding) =>
            binding.ComponentId == sensor.Id.Value && binding.TargetId == sensor.Target.Id.Value && binding.Generation == sensor.Generation &&
            sensor.Reason != SensorReadReason.NotInstalled && sensor.Reason != SensorReadReason.Disposed && sensor.Reason != SensorReadReason.TargetMissing;
        private void RebindApplied()
        {
            foreach (var subscription in _sensors) subscription.Dispose();
            _sensors.Clear();
            var graph = _runtime.CopyApplied();
            foreach (var node in graph.Nodes)
            {
                if (node.Deleted || node.Kind != AlgorithmNodeKind.Input) continue;
                var binding = graph.Bindings.Find(b => b.Key == node.BindingKey);
                if (binding == null) continue;
                foreach (var sensor in _registeredSensors)
                    if (Matches(sensor, binding)) { _sensors.Add(new SensorSubscription(this, sensor, node.Id, node.BindingKey)); break; }
            }
        }
        public void BindResourceAmount(MachineSensor sensor, ulong nodeId, string bindingKey)
        {
            if (_runtime == null || sensor == null) throw new InvalidOperationException("Runtime and sensor required.");
            var graph = _runtime.CopyApplied();
            var slot = graph.Bindings.Find(b => b.Key == bindingKey);
            var node = graph.Nodes.Find(n => n.Id == nodeId && n.Kind == AlgorithmNodeKind.Input && n.BindingKey == bindingKey);
            if (node == null || slot == null || slot.ComponentId != sensor.Id.Value || slot.TargetId != sensor.Target.Id.Value || slot.Generation != sensor.Generation)
                throw new ArgumentException("Explicit applied sensor identity and generation must match.");
            if (!_registeredSensors.Contains(sensor)) _registeredSensors.Add(sensor);
            RebindApplied();
        }
        public bool ValidateBindings(AlgorithmDocument document)
        {
            if (_disposed || !_region.IsActive) return false;
            foreach (var binding in document.Bindings)
            {
                bool found = false;
                foreach (var sensor in _registeredSensors) if (Matches(sensor, binding)) { found = true; break; }
                if (!found) return false;
            }
            return true;
        }
        public ulong Submit(AlgorithmTrigger trigger, AlgorithmIntent intent)
        {
            if (_disposed) return trigger.TaskId;
            if (intent.Kind == AlgorithmNodeKind.Log) { Result?.Invoke(intent.NodeId, new PersistentId(trigger.TaskId), "Recorded"); return trigger.TaskId; }
            if (intent.Kind != AlgorithmNodeKind.Navigate) throw new InvalidOperationException("Unsupported authority endpoint.");
            MachineTaskRecord task = null;
            if (trigger.TaskId != 0) _context.Tasks.TryGet(new PersistentId(trigger.TaskId), out task);
            if (task == null)
            {
                if (_context.Tasks.Submit("Algorithm " + trigger.NodeId, WorkPriority.Normal, out task) != QueueAdmission.Accepted)
                { Publish(trigger, intent.NodeId, PersistentId.Invalid, "rejected"); return trigger.TaskId; }
                _ownedTasks.Add(task.Id);
            }
            _waiting.Add(new Pending { Trigger = trigger.Copy(), NodeId = intent.NodeId, Task = task.Id, Position = new Vector3((float)intent.Value.X, (float)intent.Value.Y, (float)intent.Value.Z) });
            Publish(trigger,intent.NodeId,task.Id,"accepted");
            return task.Id.Value;
        }
        public void Pump(long now, double navigationSeconds)
        {
            _now = now;
            if (!_disposed) OnMachineChanged(_context.Machine);
            if (_disposed || _active != null || _waiting.Count == 0 || !_region.IsActive) return;
            var pending = _waiting[0];
            if (!_context.Tasks.TryGet(pending.Task, out var task)) { _waiting.RemoveAt(0); Publish(pending.Trigger, pending.NodeId, pending.Task, "cancelled"); return; }
            if (task.State == MachineTaskState.Queued && !_context.Tasks.TryStart(task.Id)) return;
            if (task.State != MachineTaskState.Running) return;
            _waiting.RemoveAt(0); _active = pending;
            var admission = _navigation.Start(task.Id, new MachineNavigationTarget(_region, pending.Position), navigationSeconds);
            if (admission != NavigationAdmission.Accepted)
            {
                _active = null; _context.Tasks.Cancel(task.Id); Publish(pending.Trigger, pending.NodeId, task.Id, "rejected");
            }
            else Publish(pending.Trigger,pending.NodeId,task.Id,"started");
        }
        private void OnNavigationEnded(MachineNavigation navigation)
        {
            var active = _active; if (active == null) return; _active = null;
            navigation.ReleaseWorkReservation();
            string port;
            switch(navigation.Outcome)
            {
                case BehaviorOutcome.Completed: port="completed";break;
                case BehaviorOutcome.Cancelled: port="cancelled";break;
                case BehaviorOutcome.Partial: port="partial";break;
                case BehaviorOutcome.Preempted: port="preempted";break;
                case BehaviorOutcome.TargetInvalid: port="targetInvalid";break;
                default: port="failed";break;
            }
            Publish(active.Trigger, active.NodeId, active.Task, port);
            TryClose(active.Task);
        }
        private void Publish(AlgorithmTrigger source, ulong node, PersistentId task, string port)
        {
            Result?.Invoke(node, task, port);
            if (_runtime == null || _disposed) return;
            var next = source.Copy(); next.NodeId = node; next.Port = port; next.TaskId = task.Value; next.Time = _now;
            if (!_runtime.Enqueue(next)) TryClose(task);
        }
        public void EndBatch(AlgorithmTrigger trigger) { if (trigger.TaskId != 0) TryClose(new PersistentId(trigger.TaskId)); }
        private void TryClose(PersistentId task)
        {
            if (!task.IsValid || !_ownedTasks.Contains(task)) return;
            if (_active != null && _active.Task == task) return;
            foreach (var pending in _waiting) if (pending.Task == task) return;
            if (_runtime != null && _runtime.HasTaskWork(task.Value)) return;
            _context.Tasks.CloseChain(task); _ownedTasks.Remove(task);
        }
        public void Cancel()
        {
            foreach (var item in _waiting) _context.Tasks.Cancel(item.Task);
            _waiting.Clear();
            if (_active != null) _navigation.Cancel();
            foreach (var task in new List<PersistentId>(_ownedTasks)) _context.Tasks.Cancel(task);
            _ownedTasks.Clear();
        }
        public void Dispose()
        {
            if (_disposed) return; _disposed = true; Cancel(); _navigation.Ended -= OnNavigationEnded;
            _context.Machine.Changed -= OnMachineChanged;
            if (_runtime != null) _runtime.AppliedChanged -= RebindApplied;
            foreach (var sensor in _sensors) sensor.Dispose(); _sensors.Clear(); _registeredSensors.Clear(); Result = null;
        }
        private sealed class Pending { internal AlgorithmTrigger Trigger; internal ulong NodeId; internal PersistentId Task; internal Vector3 Position; }
        private sealed class SensorSubscription : IDisposable
        {
            private readonly AlgorithmMachineAdapter _owner;
            private readonly MachineSensor _sensor;
            private readonly ulong _node, _generation;
            private readonly string _key;
            internal SensorSubscription(AlgorithmMachineAdapter owner, MachineSensor sensor, ulong node, string key)
            { _owner = owner; _sensor = sensor; _node = node; _key = key; _generation = sensor.Generation; sensor.Sampled += OnSample; }
            internal bool Matches(AlgorithmBinding binding) => binding.Key == _key && binding.ComponentId == _sensor.Id.Value &&
                binding.TargetId == _sensor.Target.Id.Value && binding.Generation == _sensor.Generation && _sensor.Generation == _generation &&
                _sensor.Reason != SensorReadReason.NotInstalled && _sensor.Reason != SensorReadReason.Disposed && _sensor.Reason != SensorReadReason.TargetMissing;
            private void OnSample(SensorReadEvent sample)
            {
                if (_owner._disposed || sample.Generation != _generation || _sensor.Generation != _generation || !_sensor.TryRead(out var current) || !ReferenceEquals(current, sample.Snapshot)) return;
                var runtime = _owner._runtime;
                var trigger = new AlgorithmTrigger { NodeId = _node, Port = "sampled", Revision = runtime.Revision, Generation = runtime.Generation, Sequence = sample.Sequence, Time = sample.WorldMilliseconds,
                    SourceId = sample.SensorId.Value, SourceTargetId = sample.Target.Id.Value, BindingGeneration = sample.Generation };
                var value = AlgorithmValue.Numeric(sample.Snapshot.ResourceAmount ?? 0, "resource");
                value.IsValid = sample.Snapshot.ResourceAmount.HasValue && !sample.Snapshot.Infinite;
                trigger.Inputs.Add(_key, value); runtime.Enqueue(trigger);
            }
            public void Dispose() { _sensor.Sampled -= OnSample; }
        }
    }
}
