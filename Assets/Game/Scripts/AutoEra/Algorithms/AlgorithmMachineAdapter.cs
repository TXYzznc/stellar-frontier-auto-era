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
        private readonly Dictionary<PersistentId, EffectorBehaviorQueue<EffectorBehaviorParameters>> _effectors = new Dictionary<PersistentId, EffectorBehaviorQueue<EffectorBehaviorParameters>>();
        private readonly List<EffectorPending> _effectorPending = new List<EffectorPending>();
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
            var bySensor = new Dictionary<(ulong component, ulong target, ulong generation), List<FieldBinding>>();
            foreach (var node in graph.Nodes)
            {
                if (node.Deleted || node.Kind != AlgorithmNodeKind.Input) continue;
                var binding = graph.Bindings.Find(b => b.Key == node.BindingKey);
                if (binding == null) continue;
                var key = (binding.ComponentId, binding.TargetId, binding.Generation);
                if (!bySensor.TryGetValue(key, out var list)) bySensor[key] = list = new List<FieldBinding>();
                list.Add(new FieldBinding { Node = node.Id, Key = node.BindingKey, Field = node.Field });
            }
            foreach (var sensor in _registeredSensors)
            {
                var key = (sensor.Id.Value, sensor.Target.Id.Value, sensor.Generation);
                if (!bySensor.TryGetValue(key, out var fields) || fields.Count == 0) continue;
                if (sensor.Reason == SensorReadReason.NotInstalled || sensor.Reason == SensorReadReason.Disposed || sensor.Reason == SensorReadReason.TargetMissing) continue;
                _sensors.Add(new SensorSubscription(this, sensor, fields));
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
            if (intent.Kind == AlgorithmNodeKind.Effector) return SubmitEffector(trigger, intent);
            if (intent.Kind == AlgorithmNodeKind.SubmitTask) return SubmitTaskNode(trigger, intent);
            if (intent.Kind == AlgorithmNodeKind.CancelTask)
            { Publish(trigger, intent.NodeId, new PersistentId(trigger.TaskId), "rejected"); return trigger.TaskId; }
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
        public bool TryReadCargo(string field, string itemType, out AlgorithmValue value)
        {
            value = null;
            if (_disposed || _context.Cargo == null) return false;
            switch (field)
            {
                case "capacity": value = AlgorithmValue.Numeric(_context.Cargo.Capacity, ""); return true;
                case "remaining": value = AlgorithmValue.Numeric(_context.Cargo.Remaining, ""); return true;
                case "amount": value = AlgorithmValue.Numeric(_context.Cargo.Count(itemType ?? ""), ""); return true;
                case "has_item": value = AlgorithmValue.Bool(!string.IsNullOrEmpty(itemType) && _context.Cargo.Has(itemType)); return true;
                default: return false;
            }
        }
        public bool TryQueryTask(string name, out AlgorithmValue task)
        {
            task = null;
            if (_disposed || string.IsNullOrEmpty(name)) return false;
            var type = AlgorithmType.Of(AlgorithmValueKind.Object);
            if (!_context.Tasks.TryFindActive(name, out var record))
            { task = new AlgorithmValue { Type = type, ObjectId = 0, IsValid = true }; return true; }
            task = new AlgorithmValue { Type = type, ObjectId = record.Id.Value, IsValid = true }; return true;
        }
        public void RegisterEffector(ComponentInstance component, EffectorBehaviorQueue<EffectorBehaviorParameters> queue)
        {
            if (_disposed || component == null || queue == null) return;
            if (_effectors.TryGetValue(component.Id, out var existing) && existing != null) existing.Ended -= OnEffectorEnded;
            _effectors[component.Id] = queue;
            queue.Ended += OnEffectorEnded;
        }
        private ulong SubmitEffector(AlgorithmTrigger trigger, AlgorithmIntent intent)
        {
            if (string.IsNullOrEmpty(intent.BindingKey) || intent.Action == null)
            { Publish(trigger, intent.NodeId, new PersistentId(trigger.TaskId), "rejected"); return trigger.TaskId; }
            var graph = _runtime?.CopyApplied();
            var binding = graph?.Bindings.Find(b => b.Key == intent.BindingKey);
            if (binding == null || binding.ComponentId == 0 || binding.TargetId == 0 || !_effectors.TryGetValue(new PersistentId(binding.ComponentId), out var queue))
            { Publish(trigger, intent.NodeId, new PersistentId(trigger.TaskId), "rejected"); return trigger.TaskId; }
            var task = EnsureRunningTask(trigger);
            if (task == null) { Publish(trigger, intent.NodeId, PersistentId.Invalid, "rejected"); return trigger.TaskId; }
            var parameters = new EffectorBehaviorParameters(intent.Action.Value.ToString())
            { Target = new PersistentObjectReference(new PersistentId(binding.TargetId), PersistentObjectKind.ResourcePoint) };
            foreach (var pair in intent.Parameters ?? new Dictionary<string, AlgorithmValue>())
            {
                var value = pair.Value;
                if (value.Type.Kind == AlgorithmValueKind.Number) parameters.Numbers[pair.Key] = value.Number;
                else if (value.Type.Kind == AlgorithmValueKind.Enumeration) parameters.Enumeration = value.EnumValue;
                else if (value.Type.Kind == AlgorithmValueKind.Boolean) parameters.Flag = value.Boolean;
            }
            var admission = queue.Submit(task.Id, new PersistentId(trigger.SourceId), new PersistentId(intent.NodeId), parameters.Target,
                WorkPriority.Normal, InterruptionRule.SafePoint, parameters, out var request);
            if (admission != QueueAdmission.Accepted)
            { Publish(trigger, intent.NodeId, task.Id, "rejected"); return task.Id.Value; }
            _effectorPending.Add(new EffectorPending { Trigger = trigger.Copy(), NodeId = intent.NodeId, Task = task.Id, Behavior = request.Id });
            Publish(trigger, intent.NodeId, task.Id, "accepted");
            return task.Id.Value;
        }
        private ulong SubmitTaskNode(AlgorithmTrigger trigger, AlgorithmIntent intent)
        {
            var task = EnsureRunningTask(trigger, intent.Field);
            if (task == null) { Publish(trigger, intent.NodeId, PersistentId.Invalid, "rejected"); return trigger.TaskId; }
            Publish(trigger, intent.NodeId, task.Id, "accepted");
            return task.Id.Value;
        }
        private MachineTaskRecord EnsureRunningTask(AlgorithmTrigger trigger, string taskName = null)
        {
            MachineTaskRecord task = null;
            if (trigger.TaskId != 0) _context.Tasks.TryGet(new PersistentId(trigger.TaskId), out task);
            if (task == null)
            {
                if (_context.Tasks.Submit(string.IsNullOrEmpty(taskName) ? "Algorithm " + trigger.NodeId : taskName, WorkPriority.Normal, out task) != QueueAdmission.Accepted) return null;
                _ownedTasks.Add(task.Id);
            }
            if (task.State == MachineTaskState.Queued) _context.Tasks.TryStart(task.Id);
            return task.State == MachineTaskState.Running ? task : null;
        }
        private void OnEffectorEnded(BehaviorRequest<EffectorBehaviorParameters> request)
        {
            if (_disposed) return;
            for (int i = 0; i < _effectorPending.Count; i++)
            {
                var pending = _effectorPending[i];
                if (pending.Behavior != request.Id) continue;
                _effectorPending.RemoveAt(i);
                string port;
                switch (request.Outcome ?? BehaviorOutcome.Failed)
                {
                    case BehaviorOutcome.Completed: port = "completed"; break;
                    case BehaviorOutcome.Cancelled: port = "cancelled"; break;
                    case BehaviorOutcome.Partial: port = "partial"; break;
                    case BehaviorOutcome.Preempted: port = "preempted"; break;
                    case BehaviorOutcome.TargetInvalid: port = "targetInvalid"; break;
                    default: port = "failed"; break;
                }
                Publish(pending.Trigger, pending.NodeId, pending.Task, port);
                TryClose(pending.Task);
                return;
            }
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
            foreach (var sensor in _sensors) sensor.Dispose(); _sensors.Clear(); _registeredSensors.Clear();
            foreach (var queue in _effectors.Values) queue.Ended -= OnEffectorEnded; _effectors.Clear(); _effectorPending.Clear();
            Result = null;
        }
        private sealed class Pending { internal AlgorithmTrigger Trigger; internal ulong NodeId; internal PersistentId Task; internal Vector3 Position; }
        private sealed class EffectorPending { internal AlgorithmTrigger Trigger; internal ulong NodeId; internal PersistentId Task; internal PersistentId Behavior; }
        private sealed class FieldBinding { internal ulong Node; internal string Key; internal string Field; }
        private sealed class SensorSubscription : IDisposable
        {
            private readonly AlgorithmMachineAdapter _owner;
            private readonly MachineSensor _sensor;
            private readonly ulong _generation;
            private readonly List<FieldBinding> _fields;
            internal SensorSubscription(AlgorithmMachineAdapter owner, MachineSensor sensor, List<FieldBinding> fields)
            { _owner = owner; _sensor = sensor; _fields = fields; _generation = sensor.Generation; sensor.Sampled += OnSample; }
            private void OnSample(SensorReadEvent sample)
            {
                if (_owner._disposed || sample.Generation != _generation || _sensor.Generation != _generation || !_sensor.TryRead(out var current) || !ReferenceEquals(current, sample.Snapshot)) return;
                var runtime = _owner._runtime;
                foreach (var field in _fields)
                {
                    var trigger = new AlgorithmTrigger { NodeId = field.Node, Port = "sampled", Revision = runtime.Revision, Generation = runtime.Generation, Sequence = sample.Sequence, Time = sample.WorldMilliseconds,
                        SourceId = sample.SensorId.Value, SourceTargetId = sample.Target.Id.Value, BindingGeneration = sample.Generation };
                    foreach (var f in _fields)
                        trigger.Inputs.Add(f.Key, ReadField(f.Field, sample.Snapshot));
                    runtime.Enqueue(trigger);
                }
            }
            private static AlgorithmValue ReadField(string field, SensorSnapshot snapshot)
            {
                switch (field)
                {
                    case "moisture":
                        var valid = snapshot.TryGetWeightedMoisture(out var moisture);
                        return new AlgorithmValue { Type = AlgorithmType.Of(AlgorithmValueKind.Number, "humidity"), Number = valid ? moisture : 0, IsValid = valid };
                    case "cached":
                        return new AlgorithmValue { Type = AlgorithmType.Of(AlgorithmValueKind.Number), Number = snapshot.CachedAmount ?? 0, IsValid = snapshot.CachedAmount.HasValue };
                    case "capacity":
                        return new AlgorithmValue { Type = AlgorithmType.Of(AlgorithmValueKind.Number), Number = snapshot.CacheCapacity ?? 0, IsValid = snapshot.CacheCapacity.HasValue };
                    case "harvestable":
                        return AlgorithmValue.Bool(snapshot.PublicStatus == "可采集");
                    case "depleted":
                        return AlgorithmValue.Bool(snapshot.PublicStatus == "耗尽");
                    case "vacant":
                        return AlgorithmValue.Bool(snapshot.PublicStatus == "空置");
                    case "mature":
                        return AlgorithmValue.Bool(snapshot.PublicStatus == "成熟");
                    case "cleanup":
                        return AlgorithmValue.Bool(snapshot.PublicStatus == "待清理");
                    default:
                        var resource = AlgorithmValue.Numeric(snapshot.ResourceAmount ?? 0, "");
                        resource.IsValid = snapshot.ResourceAmount.HasValue && !snapshot.Infinite;
                        return resource;
                }
            }
            public void Dispose() { _sensor.Sampled -= OnSample; }
        }
    }
}
