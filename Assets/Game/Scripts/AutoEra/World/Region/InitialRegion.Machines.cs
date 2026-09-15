using AutoEra.Machines;
using AutoEra.World.Identity;
using UnityEngine;

namespace AutoEra.World.Region
{
    public enum RegionMachineDeploymentResult
    {
        Bound, AlreadyBound, InvalidRegion, InvalidMachine, Destroyed, BoundElsewhere, InvalidPlacement
    }

    public sealed partial class InitialRegion
    {
        /// <summary>Attaches an existing world identity. This is deployment, not movement or arrival.</summary>
        public RegionMachineDeploymentResult DeployMachine(PersistentId id, Vector2 position, Vector2 size,
            out RegionObject model, float yaw = 0, bool blocksNavigation = true)
        {
            model = null;
            if (!IsActive || !_session.Machines.IsActive) return RegionMachineDeploymentResult.InvalidRegion;
            if (!_session.Machines.TryGet(id, out MachineInstance machine) ||
                _session.ObjectRegistry.TryResolve(id, PersistentObjectKind.Machine, out object registered) != PersistentRegistryResult.Success ||
                !ReferenceEquals(registered, machine)) return RegionMachineDeploymentResult.InvalidMachine;
            if (machine.Integrity <= 0) return RegionMachineDeploymentResult.Destroyed;
            if (machine.RegionBindingOwner != null && !ReferenceEquals(machine.RegionBindingOwner, this))
                return RegionMachineDeploymentResult.BoundElsewhere;
            if (!RegionPlacement.IsFinite(position) || !RegionPlacement.IsFinite(size) || !RegionPlacement.IsFinite(yaw) ||
                size.x <= 0 || size.y <= 0) return RegionMachineDeploymentResult.InvalidPlacement;
            if (_objects.TryGetValue(id, out RegionObject existing))
            {
                // Never turn a repeated deployment into a teleport or a fixed carrier movement API.
                if (!ReferenceEquals(existing.Machine, machine)) return RegionMachineDeploymentResult.InvalidMachine;
                if (!existing.Position.Equals(position) || !existing.Size.Equals(size) || existing.Yaw != yaw ||
                    existing.BlocksNavigation != blocksNavigation) return RegionMachineDeploymentResult.InvalidPlacement;
                model = existing;
                return RegionMachineDeploymentResult.AlreadyBound;
            }
            if (!CanPlace(position, size, yaw, PersistentId.Invalid, out _)) return RegionMachineDeploymentResult.InvalidPlacement;

            model = new RegionObject(id, PersistentObjectKind.Machine, machine.Name, position, size, yaw, blocksNavigation, machine);
            _objects.Add(id, model);
            model.IsRegistered = true;
            machine.RegionBindingOwner = this;
            machine.Changed += OnMachineChanged;
            // All spatial/identity checks are complete before the roster publishes its deployed state.
            if (!machine.Deployed) _session.Machines.Deploy(id);
            ObjectsChanged?.Invoke();
            return RegionMachineDeploymentResult.Bound;
        }

        public bool TryGetMachine(PersistentId id, out MachineInstance machine)
        {
            machine = null;
            if (!TryGet(id, out RegionObject model) || model.Machine == null) return false;
            machine = model.Machine;
            return true;
        }

        private void OnMachineChanged(MachineInstance machine)
        {
            if (!machine.Deployed) Remove(machine.Id);
            else if (_objects.TryGetValue(machine.Id, out RegionObject model)) model.SynchronizeMachineName();
        }

        private void ReleaseMachineBinding(MachineInstance machine)
        {
            machine.Changed -= OnMachineChanged;
            if (ReferenceEquals(machine.RegionBindingOwner, this)) machine.RegionBindingOwner = null;
        }

        private void OnRosterDisposed(MachineRoster roster) => Dispose();
    }
}
