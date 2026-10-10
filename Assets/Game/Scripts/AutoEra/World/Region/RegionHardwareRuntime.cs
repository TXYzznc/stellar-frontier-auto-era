using System;
using System.Collections.Generic;
using AutoEra.Algorithms;
using AutoEra.Machines;
using AutoEra.Machines.Sensors;
using AutoEra.World.Identity;
using UnityEngine;

namespace AutoEra.World.Region
{
    /// <summary>动作权威负责校验、提交与安全取消；表现对象不能实现这个接口。</summary>
    public interface IRegionEffectorExecutor
    {
        bool Supports(ComponentDefinition component, AlgorithmEffectorAction action);
        bool TryStart(MachineExecutionContext context, ComponentInstance component,
            BehaviorRequest<EffectorBehaviorParameters> request, long now,
            out IRegionEffectorOperation operation, out string reason);
    }

    public interface IRegionEffectorOperation : IDisposable
    {
        bool IsCompleted { get; }
        BehaviorOutcome Outcome { get; }
        void Advance(long worldMilliseconds);
        /// <summary>停电/休眠期间冻结动作时钟；恢复时不能追补暂停期间的产出。</summary>
        void SetPaused(bool paused, long worldMilliseconds);
        /// <summary>在领域安全边界终止并释放预留；不补做未提交的结算。停止必须同步完成。</summary>
        void Stop(BehaviorOutcome outcome);
    }
    public interface IRegionEffectorOperationStatus { string WaitingReason { get; } }

    public sealed class RegionEffectorExecutorRegistry
    {
        private readonly List<IRegionEffectorExecutor> _executors = new List<IRegionEffectorExecutor>();
        public void Register(IRegionEffectorExecutor executor)
        {
            if (executor == null) throw new ArgumentNullException(nameof(executor));
            if (!_executors.Contains(executor)) _executors.Add(executor);
        }
        public bool Unregister(IRegionEffectorExecutor executor) => _executors.Remove(executor);
        public bool TryGet(ComponentDefinition component, AlgorithmEffectorAction action, out IRegionEffectorExecutor executor)
        {
            executor = null;
            foreach (var candidate in _executors)
            {
                if (!candidate.Supports(component, action)) continue;
                if (executor != null) { executor = null; return false; }
                executor = candidate;
            }
            return executor != null;
        }
    }

    /// <summary>只在装配身份变化时对账；世界步推进现有传感器和每部件行为队列。</summary>
    public sealed partial class RegionHardwareRuntime : IDisposable, ISensorAnchor
    {
        private readonly MachineExecutionContext _context;
        private readonly InitialRegion _region;
        private readonly RegionSensorEnvironment _environment;
        private readonly SensorCatalog _catalog;
        private readonly RegionEffectorExecutorRegistry _executors;
        private readonly AlgorithmMachineAdapter _adapter;
        private readonly GameObject _view;
        private readonly Dictionary<PersistentId, MachineSensor> _sensors = new Dictionary<PersistentId, MachineSensor>();
        private readonly Dictionary<PersistentId, Effector> _effectors = new Dictionary<PersistentId, Effector>();
        private readonly List<Effector> _ordered = new List<Effector>();
        private readonly List<PersistentId> _removed = new List<PersistentId>();
        private readonly Dictionary<PersistentId, string> _unavailable = new Dictionary<PersistentId, string>();
        private bool _disposed, _reconciling, _updatingPause, _advancing;
        private long _now;
        private ulong _generation;
        public int SensorCount => _sensors.Count;
        public int EffectorCount => _effectors.Count;
        public string PresentationUnavailableReason => _view == null ? "表现对象未加载" : null;
        public bool TryGetUnavailableReason(PersistentId component, out string reason) => _unavailable.TryGetValue(component, out reason);
        public bool TryGetEffector(PersistentId component, out EffectorBehaviorQueue<EffectorBehaviorParameters> queue)
        {
            queue = _effectors.TryGetValue(component, out var item) ? item.Queue : null; return queue != null;
        }
        public RegionObject[] ReadBindingTargets(PersistentId component)
        {
            var targets = new List<RegionObject>();
            if (_disposed) return targets.ToArray();
            foreach (var item in _region.Objects)
            {
                bool supported = _effectors.ContainsKey(component);
                if (_sensors.TryGetValue(component, out var sensor))
                    supported = _environment.TryGetProvider(new PersistentObjectReference(item.Id, item.Kind), out var provider)
                        && provider.IsAvailable && provider.Supports(sensor.Profile.Kind);
                if (supported) targets.Add(item);
            }
            targets.Sort((a, b) => a.Id.CompareTo(b.Id)); return targets.ToArray();
        }
        internal RegionHardwareRuntime(MachineExecutionContext context, InitialRegion region,
            RegionSensorEnvironment environment, SensorCatalog catalog, RegionEffectorExecutorRegistry executors,
            AlgorithmMachineAdapter adapter, GameObject view)
        {
            _context = context; _region = region; _environment = environment; _catalog = catalog;
            _executors = executors; _adapter = adapter; _view = view;
            context.Machine.Changed += OnMachineChanged;
            adapter.CanExecuteEffector = CanExecute;
            Reconcile();
        }
        private void OnMachineChanged(MachineInstance _)
        {
            Reconcile();
            if (_disposed || _updatingPause) return;
            _updatingPause = true;
            try { foreach (var item in _ordered) item.Operation?.SetPaused(!item.Queue.CanExecuteCurrent, _now); }
            finally { _updatingPause = false; }
        }
        internal void SetWorldTime(long now) { _now = now; }
        public void Reconcile()
        {
            if (_disposed || _reconciling) return;
            _reconciling = true;
            try
            {
                _removed.Clear();
                foreach (var pair in _sensors) if (!Installed(pair.Key, HardwareKind.Sensor)) _removed.Add(pair.Key);
                foreach (var id in _removed) { _adapter.UnregisterSensor(_sensors[id]); _context.Sensors.Remove(id); _sensors.Remove(id); _unavailable.Remove(id); }
                _removed.Clear();
                foreach (var pair in _effectors) if (!Installed(pair.Key, HardwareKind.Effector)) _removed.Add(pair.Key);
                foreach (var id in _removed)
                {
                    var effector = _effectors[id]; Stop(effector, BehaviorOutcome.TargetInvalid);
                    _adapter.UnregisterEffector(id); _context.UnbindEffector(id);
                    _ordered.Remove(effector); _effectors.Remove(id); _unavailable.Remove(id);
                }
                for (int slot = 0; slot < _context.Machine.Definition.SensorSlots; slot++)
                {
                    var component = _context.Machine.GetComponent(HardwareKind.Sensor, slot);
                    if (component == null || _sensors.ContainsKey(component.Id)) continue;
                    if (_catalog == null || !_catalog.TryGet(component.Definition.Id * 10 + component.Definition.Level, out var profile))
                    { _unavailable[component.Id] = "传感器型号尚无可用配置"; continue; }
                    var sensor = _context.Sensors.Bind(component, profile, _environment, this);
                    _sensors.Add(component.Id, sensor); _adapter.RegisterSensor(sensor); _unavailable.Remove(component.Id);
                }
                for (int slot = 0; slot < _context.Machine.Definition.EffectorSlots; slot++)
                {
                    var component = _context.Machine.GetComponent(HardwareKind.Effector, slot);
                    if (component == null || !component.Definition.HasBehavior || _effectors.ContainsKey(component.Id)) continue;
                    var queue = _context.BindEffector<EffectorBehaviorParameters>(component);
                    var item = new Effector { Component = component, Queue = queue, Generation = checked(++_generation) };
                    _effectors.Add(component.Id, item); _ordered.Add(item);
                    _adapter.RegisterEffector(component, queue, item.Generation);
                }
                _ordered.Sort((a, b) => a.Component.Id.CompareTo(b.Component.Id));
            }
            finally { _reconciling = false; }
        }
        private bool Installed(PersistentId id, HardwareKind kind)
        {
            for (int slot = 0; slot < _context.Machine.Definition.SlotCount(kind); slot++)
                if (_context.Machine.GetComponent(kind, slot)?.Id == id) return true;
            return false;
        }
        public bool TryGetPosition(out Vector3 position)
        {
            position = default;
            if (_disposed || !_region.IsActive || !_region.TryGet(_context.Machine.Id, out var item)) return false;
            position = new Vector3(item.Position.x, 0, item.Position.y); return true;
        }
        /// <summary>绑定是用户命令；宿主不猜目标。返回真实代次，供草稿引用。</summary>
        public bool TryBindEndpoint(PersistentId component, PersistentId target, out ulong generation, out string reason)
        {
            generation = 0; reason = null;
            if (_disposed || !_region.TryGet(target, out var item)) { reason = "目标不存在"; return false; }
            if (_sensors.TryGetValue(component, out var sensor))
            {
                var reference = new PersistentObjectReference(item.Id, item.Kind);
                if (sensor.Target != reference) sensor.Bind(reference);
                generation = sensor.Generation; _adapter.RefreshSensorBindings(); return true;
            }
            if (_effectors.TryGetValue(component, out var effector)) { generation = effector.Generation; return true; }
            reason = _unavailable.TryGetValue(component, out var missing) ? missing : "组件未安装或无行为能力"; return false;
        }
        private bool CanExecute(PersistentId id, AlgorithmEffectorAction action)
            => !_disposed && _effectors.TryGetValue(id, out var item) && _executors.TryGet(item.Component.Definition, action, out _);

        internal void Advance(long now)
        {
            if (_disposed || _advancing) return;
            _now = now;
            _advancing=true;
            try
            {
                _context.Sensors.Tick(now);
                // 回调可能撤收本机；释放后不得触碰下一条动作。
                for (int i = 0; i < _ordered.Count && !_disposed; i++) Advance(_ordered[i], now);
            }
            finally { _advancing=false; }
        }
        private void Advance(Effector item, long now)
        {
            var current = item.Queue.Current;
            if (item.Request != null && !ReferenceEquals(item.Request, current)) ReleaseOperation(item, BehaviorOutcome.Cancelled);
            if (current == null) return;
            if (!_region.IsActive || !_region.TryGet(current.Target.Id, out var target) || target.Kind != current.Target.ExpectedKind)
            { Stop(item, BehaviorOutcome.TargetInvalid, false); return; }
            if (item.Request == null)
            {
                if (!Enum.TryParse(current.Parameters.Action, out AlgorithmEffectorAction action) ||
                    !_executors.TryGet(item.Component.Definition, action, out var executor))
                { _unavailable[item.Component.Id] = "该动作尚未接入执行器"; item.Queue.Finish(BehaviorOutcome.Failed); return; }
                if (!item.Queue.CanExecuteCurrent) return;
                item.Request = current;
                if (!executor.TryStart(_context, item.Component, current, now, out item.Operation, out string reason) || item.Operation == null)
                { _unavailable[item.Component.Id] = reason ?? "动作权威拒绝请求"; ReleaseOperation(item, BehaviorOutcome.Failed); item.Queue.Finish(BehaviorOutcome.Failed); return; }
                _unavailable.Remove(item.Component.Id);
            }
            if (_disposed || !ReferenceEquals(current, item.Queue.Current)) return;
            if (current.CancellationRequested) { Stop(item, BehaviorOutcome.Cancelled, false); return; }
            item.Operation.SetPaused(!item.Queue.CanExecuteCurrent, now);
            if (!item.Queue.CanExecuteCurrent) return;
            if (item.Queue.SafeStopRequested) { Stop(item, BehaviorOutcome.Partial, false); return; }
            item.Operation.Advance(now);
            if (item.Operation is IRegionEffectorOperationStatus status && !string.IsNullOrEmpty(status.WaitingReason)) _unavailable[item.Component.Id] = status.WaitingReason;
            else _unavailable.Remove(item.Component.Id);
            if (_disposed || !ReferenceEquals(current, item.Queue.Current)) return;
            if (item.Operation.IsCompleted)
            {
                var outcome = item.Operation.Outcome; ReleaseOperation(item, outcome);
                item.Queue.Finish(outcome);
            }
        }
        private static void ReleaseOperation(Effector item, BehaviorOutcome outcome)
        {
            var operation = item.Operation; item.Operation = null; item.Request = null;
            if (operation == null) return;
            try { if (!operation.IsCompleted) operation.Stop(outcome); }
            finally { operation.Dispose(); }
        }
        private static void Stop(Effector item, BehaviorOutcome outcome, bool drain = true)
        {
            if (drain) item.Queue.InvalidateUnstarted();
            ReleaseOperation(item, outcome); item.Queue.Finish(outcome);
        }
        public void Dispose()
        {
            if (_disposed) return; _disposed = true;
            _context.Machine.Changed -= OnMachineChanged; _adapter.CanExecuteEffector = null;
            foreach (var item in _ordered) { Stop(item, BehaviorOutcome.Cancelled); _adapter.UnregisterEffector(item.Component.Id); _context.UnbindEffector(item.Component.Id); }
            foreach (var sensor in _sensors.Values) { _adapter.UnregisterSensor(sensor); _context.Sensors.Remove(sensor.Id); }
            _ordered.Clear(); _effectors.Clear(); _sensors.Clear(); _unavailable.Clear();
        }
        private sealed class Effector
        {
            internal ComponentInstance Component;
            internal ulong Generation;
            internal EffectorBehaviorQueue<EffectorBehaviorParameters> Queue;
            internal BehaviorRequest<EffectorBehaviorParameters> Request;
            internal IRegionEffectorOperation Operation;
        }
    }
}
