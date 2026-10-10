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
    public sealed partial class AlgorithmMachineAdapter : IAlgorithmCommandSink, IDisposable
    {
        private readonly MachineExecutionContext _context;
        private readonly MachineNavigation _navigation;
        private readonly InitialRegion _region;
        private readonly IReadOnlyDictionary<PersistentId, AutoEra.Logistics.RegionTransferEndpoint> _transferEndpoints;
        private readonly List<Pending> _waiting = new List<Pending>();
        private readonly List<SensorSubscription> _sensors = new List<SensorSubscription>();
        private readonly List<MachineSensor> _registeredSensors = new List<MachineSensor>();
        private readonly HashSet<PersistentId> _ownedTasks = new HashSet<PersistentId>();
        private readonly Dictionary<PersistentId, ulong> _taskOwners = new Dictionary<PersistentId, ulong>();
        private readonly Dictionary<ulong, AlgorithmRuntime> _runtimes = new Dictionary<ulong, AlgorithmRuntime>();
        private readonly Queue<PendingResult> _results = new Queue<PendingResult>();
        private readonly Dictionary<PersistentId, EffectorBehaviorQueue<EffectorBehaviorParameters>> _effectors = new Dictionary<PersistentId, EffectorBehaviorQueue<EffectorBehaviorParameters>>();
        private readonly Dictionary<PersistentId, ComponentInstance> _effectorComponents = new Dictionary<PersistentId, ComponentInstance>();
        private readonly Dictionary<PersistentId, ulong> _effectorGenerations = new Dictionary<PersistentId, ulong>();
        public Func<PersistentId, AlgorithmEffectorAction, bool> CanExecuteEffector { get; set; }
        public string LastCommandUnavailableReason { get; private set; }
        private readonly List<EffectorPending> _effectorPending = new List<EffectorPending>();
        private Pending _active;
        private AlgorithmRuntime _runtime;
        private bool _disposed;
        private long _now;
        public bool IsSafe
        {
            get
            {
                if (_active != null || _waiting.Count != 0 || _effectorPending.Count != 0 || _results.Count != 0 || (_navigation != null && _navigation.IsActive)) return false;
                foreach (var queue in _effectors.Values) if (queue.Current != null || queue.WaitingCount != 0) return false;
                return true;
            }
        }
        public event Action<ulong, PersistentId, string> Result;

        /// <summary>
        /// 本机是否拥有导航。**不可移动的机器（定义 `CanMove == false`）没有导航，这是正常形态而不是失败**：
        /// 它照样有算力池、任务队列与传感器，只是不能执行 Navigate。刻意用 null 表示「没有」，
        /// 而不是给它伪造一个永远失败的导航对象——后者会把「不可移动」伪装成「导航出错」。
        /// </summary>
        public bool HasNavigation => _navigation != null;

        /// <summary>
        /// 是否已绑定任何活动实例。各实例使用隔离的命令入口，共享本机任务、导航与算力权威。
        /// </summary>
        public bool HasRuntime => _runtimes.Count != 0;

        public IAlgorithmCommandSink CreateInstanceSink(ulong instanceId)
        {
            if (_disposed || instanceId == 0) throw new InvalidOperationException("Active adapter and instance identity required.");
            return new InstanceSink(this, instanceId);
        }

        public AlgorithmMachineAdapter(MachineExecutionContext context, MachineNavigation navigation, InitialRegion region,
            IReadOnlyDictionary<PersistentId, AutoEra.Logistics.RegionTransferEndpoint> transferEndpoints = null)
        {
            _context = context; _navigation = navigation; _region = region;
            _transferEndpoints = transferEndpoints;
            if (_navigation != null) _navigation.Ended += OnNavigationEnded;
            _context.Machine.Changed += OnMachineChanged;
        }
        public void Attach(AlgorithmRuntime runtime)
        {
            if (_disposed || runtime == null || _runtimes.ContainsKey(runtime.InstanceId.Value))
                throw new InvalidOperationException("Active adapter and unique runtime required.");
            _runtimes.Add(runtime.InstanceId.Value, runtime);
            if (_runtime == null) _runtime = runtime;
            runtime.AppliedChanged += RebindApplied;
            OnMachineChanged(_context.Machine);
            RebindApplied();
        }
        public bool DetachInstance(ulong instanceId)
        {
            if (!_runtimes.TryGetValue(instanceId, out var runtime)) return false;
            CancelInstance(instanceId);
            runtime.AppliedChanged -= RebindApplied;
            _runtimes.Remove(instanceId);
            if (ReferenceEquals(_runtime, runtime))
            {
                _runtime = null;
                foreach (var candidate in _runtimes.Values) { _runtime = candidate; break; }
            }
            RebindApplied();
            return true;
        }
        private void OnMachineChanged(MachineInstance machine)
        {
            foreach (var runtime in _runtimes.Values)
                runtime.SetPaused(!machine.CanRun, _now, AlgorithmPauseReason.Machine);
        }

        private AlgorithmRuntime FindRuntime(AlgorithmTrigger trigger) => trigger.InstanceId == 0
            ? _runtime : (_runtimes.TryGetValue(trigger.InstanceId, out var runtime) ? runtime : null);
        private bool Matches(MachineSensor sensor, AlgorithmBinding binding) =>
            binding.ComponentId == sensor.Id.Value && binding.TargetId == sensor.Target.Id.Value && binding.Generation == sensor.Generation &&
            _region.TryGet(sensor.Target.Id, out var target) && target.Kind == sensor.Target.ExpectedKind &&
            sensor.Reason != SensorReadReason.NotInstalled && sensor.Reason != SensorReadReason.Disposed && sensor.Reason != SensorReadReason.TargetMissing;
        private void RebindApplied()
        {
            foreach (var subscription in _sensors) subscription.Dispose();
            _sensors.Clear();
            foreach (var runtime in _runtimes.Values) BindAppliedSensors(runtime);
        }
        private void BindAppliedSensors(AlgorithmRuntime runtime)
        {
            var graph = runtime.CopyApplied();
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
                _sensors.Add(new SensorSubscription(this, runtime, sensor, fields));
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
            RegisterSensor(sensor);
        }
        public void RegisterSensor(MachineSensor sensor)
        {
            if (_disposed || sensor == null || _registeredSensors.Contains(sensor)) return;
            _registeredSensors.Add(sensor); RebindApplied();
        }
        public void UnregisterSensor(MachineSensor sensor)
        {
            if (!_registeredSensors.Remove(sensor)) return;
            foreach (var runtime in _runtimes.Values) runtime.DiscardStaleSensorEvents(sensor.Id.Value, 0, ulong.MaxValue);
            RebindApplied();
        }
        public void RefreshSensorBindings()
        {
            if (_disposed) return;
            foreach (var runtime in _runtimes.Values)
                foreach (var sensor in _registeredSensors)
                    runtime.DiscardStaleSensorEvents(sensor.Id.Value, sensor.Target.Id.Value, sensor.Generation);
            RebindApplied();
        }
        public bool ValidateBindings(AlgorithmDocument document)
        {
            if (_disposed || !_region.IsActive) return false;
            foreach (var binding in document.Bindings)
            {
                bool found = false;
                foreach (var sensor in _registeredSensors) if (Matches(sensor, binding)) { found = true; break; }
                var id = new PersistentId(binding.ComponentId);
                if (!found && _effectorComponents.TryGetValue(id, out var component) && component.OwnerId == _context.Machine.Id &&
                    _effectorGenerations[id] == binding.Generation && _region.TryGet(new PersistentId(binding.TargetId), out _)) found = true;
                if (!found) return false;
            }
            foreach (var node in document.Nodes)
            {
                if (node.Deleted || (node.Kind != AlgorithmNodeKind.Input && node.Kind != AlgorithmNodeKind.Effector)) continue;
                var binding = document.Bindings.Find(b => b.Key == node.BindingKey);
                if (binding == null) return false;
                if (node.Kind == AlgorithmNodeKind.Input && !_registeredSensors.Exists(s => Matches(s, binding))) return false;
                if (node.Kind == AlgorithmNodeKind.Effector && !_effectors.ContainsKey(new PersistentId(binding.ComponentId))) return false;
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
            // 不可移动机器收到 Navigate：立刻按「拒绝」回报，而不是排队等一个永远不会发生的执行。
            // 这样界面与日志拿到的是一个明确原因，而不是卡在等待里。
            if (_navigation == null) { Publish(trigger, intent.NodeId, PersistentId.Invalid, "rejected"); return trigger.TaskId; }
            MachineTaskRecord task = null;
            if (trigger.TaskId != 0) _context.Tasks.TryGet(new PersistentId(trigger.TaskId), out task);
            if (task == null)
            {
                if (_context.Tasks.Submit("Algorithm " + trigger.NodeId, WorkPriority.Normal, out task) != QueueAdmission.Accepted)
                { Publish(trigger, intent.NodeId, PersistentId.Invalid, "rejected"); return trigger.TaskId; }
                _ownedTasks.Add(task.Id);
                _taskOwners[task.Id] = trigger.InstanceId != 0 ? trigger.InstanceId : (_runtime?.InstanceId.Value ?? 0);
            }
            float? facing = null;
            AutoEra.Logistics.TransportResponsibilityLedger transport = null;
            if (intent.BindingKey == "arm_load" || intent.BindingKey == "arm_unload")
            {
                var binding = FindRuntime(trigger)?.CopyApplied()?.Bindings.Find(b => b.Key == intent.BindingKey);
                if (binding == null || _transferEndpoints == null || !_transferEndpoints.TryGetValue(new PersistentId(binding.TargetId),out var endpoint) || !endpoint.IsAvailable)
                { LastCommandUnavailableReason="显式装卸停靠点已失效"; Publish(trigger,intent.NodeId,task.Id,"targetInvalid"); TryClose(task.Id); return task.Id.Value; }
                facing = endpoint.FacingYaw;
                transport = endpoint.World.Resources.Transport;
            }
            _waiting.Add(new Pending { Trigger = trigger.Copy(), NodeId = intent.NodeId, Task = task.Id, Position = new Vector3((float)intent.Value.X, (float)intent.Value.Y, (float)intent.Value.Z), FacingYaw = facing, Transport = transport });
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
        public void RegisterEffector(ComponentInstance component, EffectorBehaviorQueue<EffectorBehaviorParameters> queue, ulong generation = 1)
        {
            if (_disposed || component == null || queue == null) return;
            if (_effectors.TryGetValue(component.Id, out var existing) && existing != null)
            { existing.Ended -= OnEffectorEnded; existing.Started -= OnEffectorStarted; }
            _effectors[component.Id] = queue;
            _effectorComponents[component.Id] = component; _effectorGenerations[component.Id] = generation;
            queue.Ended += OnEffectorEnded;
            queue.Started += OnEffectorStarted;
        }
        public void UnregisterEffector(PersistentId component)
        {
            if (_effectors.TryGetValue(component, out var queue)) { queue.Ended -= OnEffectorEnded; queue.Started -= OnEffectorStarted; }
            _effectors.Remove(component); _effectorComponents.Remove(component); _effectorGenerations.Remove(component);
        }
        private ulong SubmitEffector(AlgorithmTrigger trigger, AlgorithmIntent intent)
        {
            if (string.IsNullOrEmpty(intent.BindingKey) || intent.Action == null)
            { Publish(trigger, intent.NodeId, new PersistentId(trigger.TaskId), "rejected"); return trigger.TaskId; }
            var graph = FindRuntime(trigger)?.CopyApplied();
            var binding = graph?.Bindings.Find(b => b.Key == intent.BindingKey);
            if (binding == null || binding.ComponentId == 0 || binding.TargetId == 0 || !_effectors.TryGetValue(new PersistentId(binding.ComponentId), out var queue))
            { Publish(trigger, intent.NodeId, new PersistentId(trigger.TaskId), "rejected"); return trigger.TaskId; }
            var componentId = new PersistentId(binding.ComponentId);
            if (_effectorComponents[componentId].OwnerId != _context.Machine.Id || _effectorGenerations[componentId] != binding.Generation ||
                !_region.TryGet(new PersistentId(binding.TargetId), out var target))
            { LastCommandUnavailableReason = "组件或目标绑定已失效"; Publish(trigger, intent.NodeId, new PersistentId(trigger.TaskId), "targetInvalid"); return trigger.TaskId; }
            if (!Enum.IsDefined(typeof(AlgorithmEffectorAction), intent.Action.Value) ||
                (CanExecuteEffector != null && !CanExecuteEffector(componentId, intent.Action.Value)))
            { LastCommandUnavailableReason = "该动作尚未接入执行器"; Publish(trigger, intent.NodeId, new PersistentId(trigger.TaskId), "rejected"); return trigger.TaskId; }
            LastCommandUnavailableReason = null;
            var task = EnsureRunningTask(trigger, intent.Field == "resource" ? null : intent.Field);
            if (task == null) { Publish(trigger, intent.NodeId, PersistentId.Invalid, "rejected"); return trigger.TaskId; }
            var parameters = new EffectorBehaviorParameters(intent.Action.Value.ToString())
            { Target = new PersistentObjectReference(target.Id, target.Kind) };
            if (intent.Action == AlgorithmEffectorAction.Transfer)
            {
                parameters.TransferMode = intent.Field;
                var source = graph.Bindings.Find(b => b.Key == "arm_load"); var destination = graph.Bindings.Find(b => b.Key == "arm_unload");
                if (source == null || destination == null || source.ComponentId != componentId.Value || destination.ComponentId != componentId.Value ||
                    source.Generation != binding.Generation || destination.Generation != binding.Generation ||
                    !_region.TryGet(new PersistentId(source.TargetId),out var from) || !_region.TryGet(new PersistentId(destination.TargetId),out var to))
                { LastCommandUnavailableReason = "运输来源或目的地绑定失效"; Publish(trigger,intent.NodeId,task.Id,"targetInvalid"); return task.Id.Value; }
                parameters.Objects["source"] = new PersistentObjectReference(from.Id,from.Kind);
                parameters.Objects["destination"] = new PersistentObjectReference(to.Id,to.Kind);
            }
            foreach (var pair in intent.Parameters ?? new Dictionary<string, AlgorithmValue>())
            {
                var value = pair.Value;
                if (value.Type.Kind == AlgorithmValueKind.Number) parameters.Numbers[pair.Key] = value.Number;
                else if (value.Type.Kind == AlgorithmValueKind.Enumeration) parameters.Enumeration = value.EnumValue;
                else if (value.Type.Kind == AlgorithmValueKind.Boolean) parameters.Flag = value.Boolean;
                else if (value.Type.Kind == AlgorithmValueKind.Object && value.Type.ObjectCategory == "Tree")
                    parameters.Objects[pair.Key] = new PersistentObjectReference(new PersistentId(value.ObjectId), PersistentObjectKind.Tree);
            }
            var admission = queue.Submit(task.Id, new PersistentId(trigger.InstanceId != 0 ? trigger.InstanceId : _runtime.InstanceId.Value), new PersistentId(intent.NodeId), parameters.Target,
                WorkPriority.Normal, InterruptionRule.SafePoint, parameters, out var request);
            if (admission != QueueAdmission.Accepted)
            { Publish(trigger, intent.NodeId, task.Id, "rejected"); return task.Id.Value; }
            _effectorPending.Add(new EffectorPending { Trigger = trigger.Copy(), NodeId = intent.NodeId, Task = task.Id, Behavior = request.Id });
            Publish(trigger, intent.NodeId, task.Id, "accepted");
            if (ReferenceEquals(queue.Current, request)) Publish(trigger, intent.NodeId, task.Id, "started");
            return task.Id.Value;
        }
        private void OnEffectorStarted(BehaviorRequest<EffectorBehaviorParameters> request)
        {
            if (_disposed) return;
            foreach (var pending in _effectorPending)
                if (pending.Behavior == request.Id) { Publish(pending.Trigger, pending.NodeId, pending.Task, "started"); break; }
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
                _taskOwners[task.Id] = trigger.InstanceId != 0 ? trigger.InstanceId : (_runtime?.InstanceId.Value ?? 0);
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
            FlushResults();
            if (_disposed || _active != null || _waiting.Count == 0 || !_region.IsActive) return;
            if (_navigation != null && _navigation.IsActive) return;
            var pending = _waiting[0];
            if (!_context.Tasks.TryGet(pending.Task, out var task)) { _waiting.RemoveAt(0); Publish(pending.Trigger, pending.NodeId, pending.Task, "cancelled"); return; }
            if (task.State == MachineTaskState.Queued && !_context.Tasks.TryStart(task.Id)) return;
            if (task.State != MachineTaskState.Running) return;
            _waiting.RemoveAt(0); _active = pending;
            if (_navigation == null)
            {
                // 兜底：无导航的机器不该走到这里（Submit 已拦），但真到了也要给出可辨结局而不是静默卡住。
                _active = null; _context.Tasks.Cancel(task.Id); Publish(pending.Trigger, pending.NodeId, task.Id, "rejected"); return;
            }

            var admission = _navigation.Start(task.Id, new MachineNavigationTarget(_region, pending.Position, pending.FacingYaw), navigationSeconds);
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
            if (navigation.Outcome != BehaviorOutcome.Completed)
                active.Transport?.ReportNavigationFailure(active.Task,
                    navigation.Outcome == BehaviorOutcome.Cancelled || navigation.Outcome == BehaviorOutcome.Preempted,
                    navigation.Outcome == BehaviorOutcome.TargetInvalid ? "导航目标已失效，货物等待交付" : navigation.Outcome == BehaviorOutcome.Cancelled || navigation.Outcome == BehaviorOutcome.Preempted ? "导航已中止，货物等待交付" : "导航未到达停靠点，货物等待交付");
            Publish(active.Trigger, active.NodeId, active.Task, port);
            TryClose(active.Task);
        }
        private void Publish(AlgorithmTrigger source, ulong node, PersistentId task, string port)
        {
            if (_disposed) return;
            var runtime = FindRuntime(source);
            var next = source.Copy(); next.NodeId = node; next.Port = port; next.TaskId = task.Value; next.Time = _now;
            if (runtime != null) _results.Enqueue(new PendingResult { Runtime = runtime, Trigger = next, Task = task });
        }
        /// <summary>在求值前同步暂停原因和上一步结果；事件回调自身不递归求值。</summary>
        public void BeginWorldStep(long now)
        {
            if (_disposed) return;
            _now = now;
            OnMachineChanged(_context.Machine);
            FlushResults();
        }
        internal void SetWorldTime(long now) { _now = now; }
        private void FlushResults()
        {
            int count = _results.Count;
            for (int i = 0; i < count && !_disposed; i++)
            {
                var result = _results.Dequeue();
                if (!_runtimes.TryGetValue(result.Runtime.InstanceId.Value, out var current) ||
                    !ReferenceEquals(current, result.Runtime)) { TryClose(result.Task); continue; }
                if (!current.Enqueue(result.Trigger)) TryClose(result.Task);
                Result?.Invoke(result.Trigger.NodeId, result.Task, result.Trigger.Port);
            }
        }
        public void EndBatch(AlgorithmTrigger trigger) { if (trigger.TaskId != 0) TryClose(new PersistentId(trigger.TaskId)); }
        private void TryClose(PersistentId task)
        {
            if (!task.IsValid || !_ownedTasks.Contains(task)) return;
            if (_active != null && _active.Task == task) return;
            foreach (var pending in _waiting) if (pending.Task == task) return;
            foreach (var pending in _effectorPending) if (pending.Task == task) return;
            foreach (var pending in _results) if (pending.Task == task) return;
            if (_taskOwners.TryGetValue(task, out var owner) && _runtimes.TryGetValue(owner, out var runtime) && runtime.HasTaskWork(task.Value)) return;
            _context.Tasks.CloseChain(task); _ownedTasks.Remove(task); _taskOwners.Remove(task);
        }
        public void Cancel()
        {
            foreach (var item in _waiting) _context.Tasks.Cancel(item.Task);
            _waiting.Clear();
            if (_active != null && _navigation != null) _navigation.Cancel();
            foreach (var task in new List<PersistentId>(_ownedTasks)) _context.Tasks.Cancel(task);
            _ownedTasks.Clear();
            _taskOwners.Clear();
            _effectorPending.Clear();
            _results.Clear();
        }
        private bool IsInstanceSafe(ulong instanceId)
        {
            if (_active != null && _taskOwners.TryGetValue(_active.Task, out var owner) && owner == instanceId) return false;
            foreach (var pending in _waiting) if (pending.Trigger.InstanceId == instanceId) return false;
            foreach (var pending in _effectorPending) if (pending.Trigger.InstanceId == instanceId) return false;
            foreach (var pending in _results) if (pending.Runtime.InstanceId.Value == instanceId) return false;
            return true;
        }
        private void CancelInstance(ulong instanceId)
        {
            if (_active != null && _taskOwners.TryGetValue(_active.Task, out var activeOwner) && activeOwner == instanceId)
                _navigation?.Cancel();
            for (int i = _waiting.Count - 1; i >= 0; i--)
                if (_waiting[i].Trigger.InstanceId == instanceId) _waiting.RemoveAt(i);
            for (int i = _effectorPending.Count - 1; i >= 0; i--)
                if (_effectorPending[i].Trigger.InstanceId == instanceId) _effectorPending.RemoveAt(i);
            var owned = new List<PersistentId>();
            foreach (var pair in _taskOwners) if (pair.Value == instanceId) owned.Add(pair.Key);
            foreach (var task in owned) { _context.Tasks.Cancel(task); _ownedTasks.Remove(task); _taskOwners.Remove(task); }
            int count = _results.Count;
            for (int i = 0; i < count; i++)
            {
                var result = _results.Dequeue();
                if (result.Runtime.InstanceId.Value != instanceId) _results.Enqueue(result);
            }
        }
        public void Dispose()
        {
            if (_disposed) return; _disposed = true; Cancel();
            if (_navigation != null) _navigation.Ended -= OnNavigationEnded;
            _context.Machine.Changed -= OnMachineChanged;
            foreach (var runtime in _runtimes.Values) runtime.AppliedChanged -= RebindApplied;
            _runtimes.Clear(); _runtime = null;
            foreach (var sensor in _sensors) sensor.Dispose(); _sensors.Clear(); _registeredSensors.Clear();
            foreach (var queue in _effectors.Values) { queue.Ended -= OnEffectorEnded; queue.Started -= OnEffectorStarted; }
            _effectors.Clear(); _effectorComponents.Clear(); _effectorGenerations.Clear(); _effectorPending.Clear();
            CanExecuteEffector = null;
            Result = null;
        }
        private sealed class Pending { internal AlgorithmTrigger Trigger; internal ulong NodeId; internal PersistentId Task; internal Vector3 Position; internal float? FacingYaw; internal AutoEra.Logistics.TransportResponsibilityLedger Transport; }
        private sealed class EffectorPending { internal AlgorithmTrigger Trigger; internal ulong NodeId; internal PersistentId Task; internal PersistentId Behavior; }
        private sealed class PendingResult { internal AlgorithmRuntime Runtime; internal AlgorithmTrigger Trigger; internal PersistentId Task; }
        private sealed class InstanceSink : IAlgorithmCommandSink
        {
            private readonly AlgorithmMachineAdapter _owner;
            private readonly ulong _id;
            internal InstanceSink(AlgorithmMachineAdapter owner, ulong id) { _owner = owner; _id = id; }
            public bool IsSafe => _owner.IsInstanceSafe(_id);
            public ulong Submit(AlgorithmTrigger trigger, AlgorithmIntent intent)
            { trigger.InstanceId = _id; return _owner.Submit(trigger, intent); }
            public void EndBatch(AlgorithmTrigger trigger) => _owner.EndBatch(trigger);
            public void Cancel() => _owner.CancelInstance(_id);
            public bool TryReadCargo(string field, string itemType, out AlgorithmValue value) => _owner.TryReadCargo(field, itemType, out value);
            public bool TryQueryTask(string name, out AlgorithmValue task) => _owner.TryQueryTask(name, out task);
        }
        private sealed class FieldBinding { internal ulong Node; internal string Key; internal string Field; }
        private sealed class SensorSubscription : IDisposable
        {
            private readonly AlgorithmMachineAdapter _owner;
            private readonly AlgorithmRuntime _runtime;
            private readonly MachineSensor _sensor;
            private readonly ulong _generation;
            private readonly List<FieldBinding> _fields;
            internal SensorSubscription(AlgorithmMachineAdapter owner, AlgorithmRuntime runtime, MachineSensor sensor, List<FieldBinding> fields)
            { _owner = owner; _runtime = runtime; _sensor = sensor; _fields = fields; _generation = sensor.Generation; sensor.Sampled += OnSample; }
            private void OnSample(SensorReadEvent sample)
            {
                if (_owner._disposed || sample.Generation != _generation || _sensor.Generation != _generation || !_sensor.TryRead(out var current) || !ReferenceEquals(current, sample.Snapshot)) return;
                var runtime = _runtime;
                foreach (var field in _fields)
                {
                    var trigger = new AlgorithmTrigger { NodeId = field.Node, Port = "sampled", Revision = runtime.Revision, Generation = runtime.Generation, Sequence = sample.Sequence, Time = sample.WorldMilliseconds,
                        SourceId = sample.SensorId.Value, SourceTargetId = sample.Target.Id.Value, BindingGeneration = sample.Generation };
                    foreach (var f in _fields)
                        trigger.Inputs[f.Key] = ReadField(f.Field, sample.Snapshot);
                    runtime.Enqueue(trigger);
                }
            }
            private static AlgorithmValue ReadField(string field, SensorSnapshot snapshot)
            {
                switch (field)
                {
                    case "trees": return new AlgorithmValue { Type = AlgorithmType.Of(AlgorithmValueKind.TreeGrid), Trees = snapshot.Trees == null ? null : new AlgorithmTreeGrid(snapshot.Trees), IsValid = snapshot.Trees != null };
                    case "mature_count":
                        int matureCount = 0;
                        if (snapshot.Trees != null) foreach (var tree in snapshot.Trees) if (tree.Stage == AutoEra.ResourcePoints.TreeStage.Mature) matureCount++;
                        return new AlgorithmValue { Type = AlgorithmType.Of(AlgorithmValueKind.Number), Number = matureCount, IsValid = snapshot.Trees != null };
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
                    case "resource":
                        var resource = AlgorithmValue.Numeric(snapshot.ResourceAmount ?? 0, "");
                        resource.IsValid = snapshot.ResourceAmount.HasValue && !snapshot.Infinite;
                        return resource;
                    default: return new AlgorithmValue { IsValid = false };
                }
            }
            public void Dispose() { _sensor.Sampled -= OnSample; }
        }
    }
}
