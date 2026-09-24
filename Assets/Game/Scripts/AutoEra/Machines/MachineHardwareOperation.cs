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
        private bool _remove, _applying, _removeAll;
        private bool _disposed;
        private ManagementOrigin _origin;
        public HardwareOperationState State { get; private set; }
        public MachineManagementResult Result { get; private set; }
        public ulong RequestVersion { get; private set; }
        public event Action<MachineHardwareOperation> Changed;
        public MachineHardwareOperation(MachineRoster roster) { _roster = roster ?? throw new ArgumentNullException(nameof(roster)); }

        public bool Begin(PersistentId machineId, ManagementOrigin origin, bool remove, HardwareKind kind, int slot, PersistentId componentId)
            => Begin(machineId, origin, remove, kind, slot, componentId, removeAll: false);

        /// <summary>
        /// 一键卸下：与单槽拆卸走**同一条**协调器（同样的来源门禁、同样的等待安全停机、
        /// 同样的请求版本取消），区别只在最后那一次应用是「整台清空」而不是「拆一格」。
        /// 另起一条路径就会让「什么时候不能改硬件」出现第二份判断。
        /// </summary>
        public bool BeginRemoveAll(PersistentId machineId, ManagementOrigin origin)
            => Begin(machineId, origin, remove: true, default, -1, PersistentId.Invalid, removeAll: true);

        private bool Begin(PersistentId machineId, ManagementOrigin origin, bool remove, HardwareKind kind, int slot,
            PersistentId componentId, bool removeAll)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(MachineHardwareOperation));
            if (State == HardwareOperationState.Waiting) return false;
            Unsubscribe(); RequestVersion++;
            if (!_roster.TryGet(machineId, out _machine))
            { Complete(HardwareOperationState.Rejected, MachineManagementResult.InvalidState); return true; }
            _origin = origin; _remove = remove; _kind = kind; _slot = slot; _component = componentId; _removeAll = removeAll;
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
                var result = _removeAll ? _roster.RemoveAll(_machine.Id, _origin)
                    : _remove ? _roster.Remove(_machine.Id, _origin, _kind, _slot)
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
