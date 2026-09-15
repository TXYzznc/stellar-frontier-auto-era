using System;
using AutoEra.World.Identity;

namespace AutoEra.Machines
{
    public enum HardwareOperationState { Idle, Waiting, Completed, Rejected, Cancelled }

    /// <summary>One already-confirmed management intent. No device, UI, timer, or simulated work completion.</summary>
    public sealed class MachineHardwareOperation : IDisposable
    {
        private readonly MachineRoster _roster;
        private MachineInstance _machine;
        private PersistentId _component;
        private int _slot;
        private HardwareKind _kind;
        private bool _remove, _applying;
        private bool _disposed;
        private ManagementOrigin _origin;
        public HardwareOperationState State { get; private set; }
        public MachineManagementResult Result { get; private set; }
        public ulong RequestVersion { get; private set; }
        public event Action<MachineHardwareOperation> Changed;
        public MachineHardwareOperation(MachineRoster roster) { _roster = roster ?? throw new ArgumentNullException(nameof(roster)); }

        public bool Begin(PersistentId machineId, ManagementOrigin origin, bool remove, HardwareKind kind, int slot, PersistentId componentId)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(MachineHardwareOperation));
            if (State == HardwareOperationState.Waiting) return false;
            Unsubscribe(); RequestVersion++;
            if (!_roster.TryGet(machineId, out _machine))
            { Complete(HardwareOperationState.Rejected, MachineManagementResult.InvalidState); return true; }
            _origin = origin; _remove = remove; _kind = kind; _slot = slot; _component = componentId;
            // Never grant Hub the ability to stop and modify hardware through this coordinator.
            if ((_machine.Deployed && origin != ManagementOrigin.Field) || (!_machine.Deployed && origin != ManagementOrigin.Library))
            { Complete(HardwareOperationState.Rejected, MachineManagementResult.InvalidOrigin); return true; }
            var stop = _machine.SetRunState(origin, MachineRunState.Stopped);
            if (stop != MachineManagementResult.Completed && stop != MachineManagementResult.WaitingForSafeStop)
            { Complete(HardwareOperationState.Rejected, stop); return true; }
            State = HardwareOperationState.Waiting; Result = MachineManagementResult.WaitingForSafeStop;
            _machine.Changed += OnMachineChanged;
            TryApply();
            if (State == HardwareOperationState.Waiting) Changed?.Invoke(this);
            return true;
        }

        private void OnMachineChanged(MachineInstance machine) { TryApply(); }
        private void TryApply()
        {
            if (_applying || State != HardwareOperationState.Waiting || _machine == null) return;
            if (_machine.HasActiveBehavior) return;
            _applying = true;
            try
            {
                // Resolve again: a late completion never writes to a stale removed instance.
                if (!_roster.TryGet(_machine.Id, out var current) || !ReferenceEquals(current, _machine))
                { Complete(HardwareOperationState.Rejected, MachineManagementResult.InvalidState); return; }
                var result = _remove ? _roster.Remove(_machine.Id, _origin, _kind, _slot)
                    : _roster.Install(_machine.Id, _origin, _component, _slot);
                if (result == MachineManagementResult.WaitingForSafeStop) return;
                Complete(result == MachineManagementResult.Completed ? HardwareOperationState.Completed : HardwareOperationState.Rejected, result);
            }
            finally { _applying = false; }
        }

        public bool Cancel(ulong requestVersion)
        {
            if (requestVersion != RequestVersion || State != HardwareOperationState.Waiting || _applying) return false;
            Complete(HardwareOperationState.Cancelled, MachineManagementResult.Completed);
            // Cancelling the uncommitted intent does not undo the already-issued Stop or reinstall anything.
            return true;
        }
        private void Complete(HardwareOperationState state, MachineManagementResult result)
        { Unsubscribe(); State = state; Result = result; Changed?.Invoke(this); }
        private void Unsubscribe() { if (_machine != null) _machine.Changed -= OnMachineChanged; }
        public void Dispose()
        { Unsubscribe(); if (State == HardwareOperationState.Waiting) State = HardwareOperationState.Cancelled; Changed = null; _disposed = true; }
    }
}
