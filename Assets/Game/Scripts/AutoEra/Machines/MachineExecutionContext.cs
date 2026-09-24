using System;
using System.Collections.Generic;
using AutoEra.Events;
using AutoEra.World.Identity;

namespace AutoEra.Machines
{
    /// <summary>Event-driven composition of state, task and compute services. Physical executors consume CanExecuteCurrent.</summary>
    public sealed class MachineExecutionContext : IDisposable
    {
        private readonly MachineInstance _machine;
        private readonly PersistentIdAllocator _ids;
        private readonly Dictionary<PersistentId, IBinding> _effectors = new Dictionary<PersistentId, IBinding>();
        private readonly List<PersistentId> _removed = new List<PersistentId>();
        private bool _synchronizing, _repeat, _disposed;
        private object _navigationOwner;
        private bool _navigationActive;
        public MachineInstance Machine => _machine;
        public MachineTaskQueue Tasks { get; }
        public MachineComputePool Compute { get; }
        public Sensors.MachineSensorSet Sensors { get; }
        /// <summary>本机货舱（DEC-111）：统一容量单位，第一版由执行上下文持有。</summary>
        public MachineCargo Cargo { get; }
        public MachineExecutionContext(MachineInstance machine, PersistentIdAllocator ids, AutoEraEventService events = null)
        {
            _machine = machine ?? throw new ArgumentNullException(nameof(machine));
            _ids = ids ?? throw new ArgumentNullException(nameof(ids));
            Tasks = new MachineTaskQueue(ids, events, machine.Id); Compute = new MachineComputePool(ids, machine.ComputeCapacity, machine.LogicCapacity);
            Sensors = new Sensors.MachineSensorSet(this);
            Cargo = new MachineCargo(100);
            _machine.Changed += OnMachineChanged; Compute.Changed += OnComputeChanged;
            Synchronize();
        }

        public EffectorBehaviorQueue<T> BindEffector<T>(ComponentInstance component)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(MachineExecutionContext));
            if (component == null || component.OwnerId != _machine.Id || !component.Definition.HasBehavior ||
                _effectors.ContainsKey(component.Id)) throw new ArgumentException("An installed active effector can bind only once.");
            var queue = new EffectorBehaviorQueue<T>(_ids, Tasks);
            var binding = new Binding<T>(component, queue, Synchronize);
            _effectors.Add(component.Id, binding); Synchronize(); return queue;
        }

        private void OnMachineChanged(MachineInstance _) => Synchronize();
        internal bool TryBindNavigation(object owner)
        {
            if (_disposed || _navigationOwner != null) return false;
            _navigationOwner = owner; return true;
        }
        internal void SetNavigationActivity(object owner, bool active)
        {
            if (!ReferenceEquals(owner, _navigationOwner)) throw new InvalidOperationException("Navigation owner mismatch.");
            _navigationActive = active; Synchronize();
        }
        internal void UnbindNavigation(object owner)
        {
            if (!ReferenceEquals(owner, _navigationOwner)) return;
            _navigationActive = false; _navigationOwner = null; Synchronize();
        }
        private void OnComputeChanged(MachineComputePool _) => Synchronize();
        private void Synchronize()
        {
            if (_disposed) return;
            if (_synchronizing) { _repeat = true; return; }
            _synchronizing = true;
            try
            {
                do
                {
                    _repeat = false;
                    Tasks.SetPaused(!_machine.Powered, MachineWaitReason.Power);
                    Tasks.SetPaused(!_machine.Activated || _machine.RequestedRunState != MachineRunState.Running, MachineWaitReason.Algorithm);
                    if (Compute.Capacity != _machine.ComputeCapacity || Compute.LogicCapacity != _machine.LogicCapacity)
                        if (!Compute.TryReconfigure(_machine.ComputeCapacity, _machine.LogicCapacity))
                            throw new InvalidOperationException("Hardware changed while compute/logic reservations were still owned.");
                    Compute.SetDispatchEnabled(_machine.CanRun);
                    _machine.UpdateComputeUsage(Compute.Used, Compute.AppliedLogicCost, Compute.WaitingCount);
                    bool active = _navigationActive;
                    // 移动与「效应器在执行动作」是两件事：区域电网按部件逐项求和耗电，
                    // 载体只在**移动**时算工作负载，所以这里把移动单独推送给机器。
                    _machine.UpdateNavigationActivity(_navigationActive);
                    _removed.Clear();
                    foreach (var pair in _effectors)
                    {
                        var binding = pair.Value;
                        if (binding.Component.OwnerId != _machine.Id)
                        {
                            binding.Invalidate();
                            if (binding.Active) throw new InvalidOperationException("Active effector was removed outside the hardware gate.");
                            binding.Dispose(); _removed.Add(pair.Key); continue;
                        }
                        bool mayAdvance = _machine.Powered && _machine.Activated && binding.Component.Enabled &&
                            _machine.RequestedRunState != MachineRunState.Sleeping;
                        binding.SetEnabled(_machine.CanRun && binding.Component.Enabled, mayAdvance);
                        // 每个效应器各自的执行状态单独推送：能耗必须按部件求和，
                        // 用一个整机倍率替代正是规格禁止的做法。
                        _machine.SetComponentActivity(binding.Component.Id, binding.Active);
                        active |= binding.Active;
                    }
                    foreach (var id in _removed) _effectors.Remove(id);
                    _machine.UpdateBehaviorActivity(active);
                } while (_repeat);
            }
            finally { _synchronizing = false; }
        }

        public void Dispose()
        {
            if (_disposed) return;
            if (_navigationOwner != null) throw new InvalidOperationException("Dispose navigation before execution context.");
            // Shutdown must not discard an in-flight physical action. Caller drains/cancels at its safe point first.
            foreach (var binding in _effectors.Values)
                if (binding.Active || binding.WaitingCount > 0) throw new InvalidOperationException("Drain effector queues before releasing execution context.");
            Sensors.Dispose();
            if (Compute.Used != 0 || Compute.WaitingCount != 0) throw new InvalidOperationException("Release compute leases before shutdown.");
            _disposed = true;
            _machine.Changed -= OnMachineChanged; Compute.Changed -= OnComputeChanged;
            foreach (var binding in _effectors.Values) binding.Dispose();
            _effectors.Clear();
        }

        private interface IBinding : IDisposable
        {
            ComponentInstance Component { get; }
            bool Active { get; }
            int WaitingCount { get; }
            void SetEnabled(bool enabled, bool advanceCurrent);
            void Invalidate();
        }
        private sealed class Binding<T> : IBinding
        {
            private readonly EffectorBehaviorQueue<T> _queue;
            private readonly Action _changed;
            public ComponentInstance Component { get; }
            public bool Active => _queue.Current != null;
            public int WaitingCount => _queue.WaitingCount;
            public Binding(ComponentInstance component, EffectorBehaviorQueue<T> queue, Action changed)
            { Component = component; _queue = queue; _changed = changed; queue.Started += OnChanged; queue.Ended += OnChanged; }
            private void OnChanged(BehaviorRequest<T> _) => _changed();
            public void SetEnabled(bool enabled, bool advanceCurrent) => _queue.SetExecutionPermission(enabled, advanceCurrent);
            public void Invalidate() => _queue.InvalidateUnstarted();
            public void Dispose() { _queue.Started -= OnChanged; _queue.Ended -= OnChanged; _queue.TryDetach(); }
        }
    }
}
