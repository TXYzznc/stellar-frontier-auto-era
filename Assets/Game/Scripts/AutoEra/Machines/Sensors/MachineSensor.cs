using System;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using UnityEngine;

namespace AutoEra.Machines.Sensors
{
    public sealed class MachineSensor : IDisposable
    {
        private readonly MachineExecutionContext _context;
        private readonly ComponentInstance _component;
        private readonly ISensorEnvironment _environment;
        private readonly ISensorAnchor _anchor;
        private ComputeRequest _lease;
        private long _now, _next;
        private ulong _requestVersion, _sequence;
        private bool _disposed;
        public SensorProfile Profile { get; }
        public PersistentId Id => _component.Id;
        public PersistentObjectReference Target { get; private set; }
        public ulong Generation { get; private set; }
        public SensorReadReason Reason { get; private set; } = SensorReadReason.NoTarget;
        public SensorSnapshot LastSample { get; private set; }
        public event Action<SensorReadEvent> Sampled;
        public event Action<SensorReadEvent> Changed;
        public MachineSensor(MachineExecutionContext context, ComponentInstance component, SensorProfile profile,
            ISensorEnvironment environment, ISensorAnchor anchor)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _component = component ?? throw new ArgumentNullException(nameof(component));
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _anchor = anchor ?? throw new ArgumentNullException(nameof(anchor));
            if (component.OwnerId != context.Machine.Id || component.Definition.Kind != HardwareKind.Sensor ||
                component.Definition.Id != profile.ComponentDefinitionId || component.Definition.Level != profile.Level)
                throw new ArgumentException("Sensor profile must match an installed sensor.");
            context.Machine.Changed += OnMachineChanged;
        }
        public void Bind(PersistentObjectReference target)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(MachineSensor));
            Generation++; Target = target; _next = _now; Release(); SetReason(SensorReadReason.NoTarget);
        }
        public bool TryRead(out SensorSnapshot sample)
        { sample = Reason == SensorReadReason.None && !_disposed ? LastSample : null; return sample != null; }
        private SensorReadReason Permission()
        {
            if (_component.OwnerId != _context.Machine.Id) return SensorReadReason.NotInstalled;
            if (!_context.Machine.Powered) return SensorReadReason.PowerOff;
            if (!_component.Enabled || _context.Machine.RequestedRunState == MachineRunState.Sleeping) return SensorReadReason.Sleeping;
            if (!_context.Machine.CanRun) return SensorReadReason.NotRunning;
            return SensorReadReason.None;
        }
        private void OnMachineChanged(MachineInstance _)
        { var reason = Permission(); if (reason != SensorReadReason.None) { Release(); _next = _now; SetReason(reason); } }
        public void Tick(long worldMilliseconds)
        {
            if (_disposed) return;
            if (worldMilliseconds < _now) throw new ArgumentOutOfRangeException(nameof(worldMilliseconds));
            _now = worldMilliseconds;
            var permission = Permission();
            if (permission != SensorReadReason.None) { Release(); SetReason(permission); _next = _now; return; }
            if (!_environment.IsActive) { Invalidate(SensorReadReason.RegionUnavailable); return; }
            if (!Target.IsValid) { Invalidate(SensorReadReason.NoTarget); return; }
            if (!_environment.Contains(Target)) { Invalidate(SensorReadReason.TargetMissing); return; }
            if (!_environment.TryGetProvider(Target, out var provider) || provider == null || !provider.IsAvailable || provider.Target != Target)
            { Invalidate(SensorReadReason.ProviderUnavailable); return; }
            if (!provider.Supports(Profile.Kind)) { Invalidate(SensorReadReason.UnsupportedTarget); return; }
            if (!_anchor.TryGetPosition(out var position) || !SoilCellReadout.Finite(position)) { Invalidate(SensorReadReason.AnchorUnavailable); return; }
            if (_lease == null)
                _context.Compute.Submit(Profile.ComputeCost, ComputeClass.Sampling, WorkPriority.Normal, ComputeMergeKind.SensorSample,
                    Id, ++_requestVersion, _now, true, out _lease);
            if (_disposed) { Release(); return; }
            permission = Permission();
            if (permission != SensorReadReason.None) { Invalidate(permission); return; }
            if (_lease == null || _lease.State != ComputeState.Running) { SetReason(SensorReadReason.WaitingCompute); _next = _now; return; }
            Vector3 closest = provider.ClosestPoint(position);
            if (!SoilCellReadout.Finite(closest)) { Invalidate(SensorReadReason.InvalidData); return; }
            if (Vector3.Distance(position, closest) > Profile.Range)
            {
                SetReason(SensorReadReason.OutOfRange);
                if (_now >= _next) { _next = _now + Profile.IntervalMilliseconds; Yield(); }
                return;
            }
            if (_now < _next) return;
            ulong generation = Generation;
            bool valid = provider.TryRead(Profile.Kind, out var snapshot);
            if (_disposed || Generation != generation) return;
            if (Permission() != SensorReadReason.None) { OnMachineChanged(_context.Machine); return; }
            if (!_environment.IsActive) { Invalidate(SensorReadReason.RegionUnavailable); return; }
            if (!_environment.Contains(Target)) { Invalidate(SensorReadReason.TargetMissing); return; }
            if (!provider.IsAvailable) { Invalidate(SensorReadReason.ProviderUnavailable); return; }
            if (!valid || snapshot == null || (Profile.Kind == SensorKind.Soil && !snapshot.TryGetWeightedMoisture(out _)))
            { Invalidate(SensorReadReason.InvalidData); return; }
            bool changed = Reason != SensorReadReason.None || LastSample == null || LastSample.Version != snapshot.Version;
            LastSample = snapshot; Reason = SensorReadReason.None; _next = _now + Profile.IntervalMilliseconds;
            var notification = new SensorReadEvent(Id, Target, Generation, ++_sequence, _now, Reason, snapshot);
            Sampled?.Invoke(notification);
            if (!_disposed && Generation == generation && Reason == SensorReadReason.None && changed) Changed?.Invoke(notification);
            if (!_disposed && Generation == generation) Yield();
        }
        private void Yield()
        {
            if (_lease != null && _context.Compute.YieldAtBoundary(_lease.Id)) SetReason(SensorReadReason.WaitingCompute);
        }
        private void Invalidate(SensorReadReason reason) { Release(); SetReason(reason); _next = _now; }
        private void SetReason(SensorReadReason reason)
        {
            if (Reason == reason) return; Reason = reason;
            Changed?.Invoke(new SensorReadEvent(Id, Target, Generation, ++_sequence, _now, reason, null));
        }
        private void Release()
        {
            var lease = _lease; _lease = null;
            if (lease == null) return;
            if (lease.State == ComputeState.Waiting) _context.Compute.CancelWaiting(lease.Id);
            else if (lease.State == ComputeState.Running) _context.Compute.Release(lease.Id);
        }
        public void Dispose()
        {
            if (_disposed) return; _disposed = true; Generation++;
            _context.Machine.Changed -= OnMachineChanged; Release(); SetReason(SensorReadReason.Disposed);
            Sampled = null; Changed = null;
        }
    }
}
