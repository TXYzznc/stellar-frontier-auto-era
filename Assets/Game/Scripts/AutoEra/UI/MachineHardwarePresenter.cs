using System;
using System.Collections.Generic;
using AutoEra.Machines;
using AutoEra.World.Identity;

namespace AutoEra.UI
{
    public enum MachinePanelLayer { Overview, Hardware, Picker, Confirm, Waiting, Success, Rejected, Invalid }

    /// <summary>Lifecycle-owned projection of existing machine authority, not a second inventory or operation queue.</summary>
    public sealed class MachineHardwarePresenter : IDisposable
    {
        private readonly MachineRoster _roster;
        private readonly MachineHardwareOperation _operation;
        private MachineInstance _machine;
        private bool _disposed, _remove;
        private long _confirmedRevision;
        private PersistentId _candidate;
        private readonly List<ComponentInstance> _candidates = new List<ComponentInstance>();
        public MachineInstance Machine => _machine;
        public ManagementOrigin Origin { get; private set; }
        public MachinePanelLayer Layer { get; private set; }
        public HardwareKind Kind { get; private set; }
        public int Slot { get; private set; }
        public MachineManagementResult Result { get; private set; }
        public IReadOnlyList<ComponentInstance> Candidates => _candidates;
        public PersistentId CandidateId => _candidate;
        public event Action Changed;
        public bool IsValid => !_disposed && _roster.IsActive && _machine != null &&
            _roster.TryGet(_machine.Id, out var current) && ReferenceEquals(current, _machine);
        public bool CanManage => IsValid && (Origin == ManagementOrigin.Field ? _machine.Deployed :
            Origin == ManagementOrigin.Hub ? _machine.Connected : Origin == ManagementOrigin.Library && !_machine.Deployed);
        public bool IsWaiting => _operation.State == HardwareOperationState.Waiting;
        public bool IsRemoval => _remove;
        public bool CanChangeHardware => CanManage && Origin != ManagementOrigin.Hub && _machine.Integrity > 0;

        public MachineHardwarePresenter(MachineRoster roster, PersistentId id, ManagementOrigin origin)
        {
            _roster = roster ?? throw new ArgumentNullException(nameof(roster)); Origin = origin;
            _operation = roster.GetHardwareOperation(id); _operation.Changed += OnOperation;
            _roster.Disposed += OnDisposed;
            _roster.Changed += OnRosterChanged;
            _roster.TryGet(id, out _machine);
            Layer = IsValid ? (IsWaiting ? MachinePanelLayer.Waiting : MachinePanelLayer.Overview) : MachinePanelLayer.Invalid;
        }
        public void SetOrigin(ManagementOrigin origin) { Origin = origin; Changed?.Invoke(); }
        public void ShowPage(bool hardware)
        {
            if (Layer == MachinePanelLayer.Waiting) return;
            Layer = IsValid ? (hardware ? MachinePanelLayer.Hardware : MachinePanelLayer.Overview) : MachinePanelLayer.Invalid;
            Changed?.Invoke();
        }
        public void SelectSlot(HardwareKind kind, int slot)
        {
            if (!CanChangeHardware || IsWaiting || slot < 0 || slot >= _machine.Definition.SlotCount(kind)) return;
            Kind = kind; Slot = slot; _candidate = PersistentId.Invalid; _candidates.Clear();
            foreach (var item in _roster.Components)
                if (item.Definition.Kind == kind) _candidates.Add(item);
            _candidates.Sort((a, b) => a.Id.Value.CompareTo(b.Id.Value));
            Layer = _machine.GetComponent(kind, slot) == null ? MachinePanelLayer.Picker : MachinePanelLayer.Hardware; Changed?.Invoke();
        }
        public bool SelectCandidate(PersistentId id)
        {
            if (Layer != MachinePanelLayer.Picker || !CanChangeHardware) return false;
            foreach (var item in _candidates)
                if (item.Id == id && !item.OwnerId.IsValid) { _candidate = id; Changed?.Invoke(); return true; }
            return false;
        }
        public ComponentInstance SelectedComponent
        {
            get { foreach (var item in _candidates) if (item.Id == _candidate) return item; return null; }
        }
        public bool Preview(bool remove)
        {
            if (!CanChangeHardware || IsWaiting) return false;
            _remove = remove;
            if (remove ? _machine.GetComponent(Kind, Slot) == null : SelectedComponent == null) return false;
            _confirmedRevision = _machine.Revision;
            Layer = MachinePanelLayer.Confirm; Changed?.Invoke(); return true;
        }
        public bool TryGetImpact(out int compute, out int logic, out int capacity)
        {
            compute = logic = capacity = 0;
            if (!CanChangeHardware) return false;
            ComponentInstance item = _remove ? _machine.GetComponent(Kind, Slot) : SelectedComponent;
            if (item == null || (!_remove && (item.OwnerId.IsValid || _machine.GetComponent(Kind, Slot) != null))) return false;
            int sign = _remove ? -1 : 1;
            compute = _machine.ComputeCapacity + sign * item.Definition.ComputeCapacity;
            logic = _machine.LogicCapacity + sign * item.Definition.LogicCapacity;
            capacity = _machine.TotalCapacity + sign * item.Definition.AddedCapacity;
            return compute >= _machine.ReservedCompute && logic >= _machine.AppliedLogicCost && capacity >= _machine.UsedCapacity &&
                !(_remove && Kind == HardwareKind.Core && _machine.ComputeWaitingCount > 0);
        }
        public bool Confirm()
        {
            if (Layer != MachinePanelLayer.Confirm || !TryGetImpact(out _, out _, out _)) return false;
            if (_machine.Revision != _confirmedRevision) { _confirmedRevision = _machine.Revision; Changed?.Invoke(); return false; }
            return _operation.Begin(_machine.Id, Origin, _remove, Kind, Slot, _candidate);
        }
        public void Back()
        {
            Layer = IsValid ? MachinePanelLayer.Hardware : MachinePanelLayer.Invalid;
            Changed?.Invoke();
        }
        public void CancelWaiting() { _operation.Cancel(_operation.RequestVersion); Back(); }
        public MachineManagementResult Rename(string name)
        {
            Result = CanManage ? _roster.Rename(_machine.Id, name) : MachineManagementResult.InvalidOrigin;
            Changed?.Invoke(); return Result;
        }
        public void ToggleRun()
        {
            if (!CanManage || IsWaiting) return;
            Result = _machine.SetRunState(Origin, _machine.RequestedRunState == MachineRunState.Running ? MachineRunState.Stopped : MachineRunState.Running);
            if (Result != MachineManagementResult.Completed && Result != MachineManagementResult.WaitingForSafeStop) Layer = MachinePanelLayer.Rejected;
            Changed?.Invoke();
        }
        public void ToggleComponent(HardwareKind kind, int slot)
        {
            if (!CanChangeHardware || IsWaiting) return;
            var item = _machine.GetComponent(kind, slot);
            if (item == null) return;
            Result = _machine.SetComponentEnabled(Origin, kind, slot, !item.Enabled); Changed?.Invoke();
        }
        private void OnOperation(MachineHardwareOperation operation)
        {
            if (_disposed) return;
            Result = operation.Result;
            Layer = operation.State == HardwareOperationState.Waiting ? MachinePanelLayer.Waiting :
                operation.State == HardwareOperationState.Completed ? MachinePanelLayer.Success :
                operation.State == HardwareOperationState.Cancelled ? MachinePanelLayer.Hardware : MachinePanelLayer.Rejected;
            Changed?.Invoke();
        }
        private void OnRosterChanged()
        {
            if (!IsValid) Layer = MachinePanelLayer.Invalid;
            if (Layer == MachinePanelLayer.Picker)
            {
                _candidates.Clear();
                foreach (var item in _roster.Components) if (item.Definition.Kind == Kind) _candidates.Add(item);
                _candidates.Sort((a,b) => a.Id.CompareTo(b.Id));
            }
            Changed?.Invoke();
        }
        private void OnDisposed(MachineRoster roster) { Layer = MachinePanelLayer.Invalid; Changed?.Invoke(); }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true; _operation.Changed -= OnOperation;
            _roster.Disposed -= OnDisposed;
            _roster.Changed -= OnRosterChanged;
            _candidates.Clear(); Changed = null;
        }
    }
}
