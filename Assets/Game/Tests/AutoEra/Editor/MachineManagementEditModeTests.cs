using AutoEra.Machines;
using AutoEra.World.Identity;
using NUnit.Framework;
using System.Collections.Generic;

namespace AutoEra.Tests.Editor
{
    public sealed class MachineManagementEditModeTests
    {
        // Explicit service fixtures, not approved content/balance data.
        private static MachineDefinition Carrier(int id = 1, int effectors = 2, int capacity = 30)
            => new MachineDefinition(id, "Fixture", 1, 2, 1, effectors, capacity, id == 1, true, 100);
        private static MachineRoster Roster()
        {
            var ids = new PersistentIdAllocator();
            return new MachineRoster(ids, new PersistentObjectRegistry(ids));
        }

        [Test]
        public void Integrity_ZeroDeactivatesAndRepairDoesNotReactivateOrPermitDamagedRecovery()
        {
            var roster = Roster(); var machine = roster.Create(Carrier()); roster.Deploy(machine.Id);
            machine.UpdateEnvironment(true, true); machine.Activate(ManagementOrigin.Field);
            machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);
            machine.UpdateIntegrity(50); Assert.That(machine.CanRun, Is.True);
            machine.SetRunState(ManagementOrigin.Field, MachineRunState.Stopped);
            Assert.That(roster.RecoverToLibrary(machine.Id, ManagementOrigin.Field), Is.EqualTo(MachineManagementResult.RepairRequired));
            machine.UpdateIntegrity(0); Assert.That(machine.Activated, Is.False); Assert.That(machine.PowerSwitchOn, Is.False);
            machine.UpdateIntegrity(100); Assert.That(machine.Activated, Is.False); Assert.That(machine.Connected, Is.False);
        }

        [Test]
        public void Identity_RenameDoesNotChangeIdAndHistoricalCountDoesNotRewind()
        {
            var roster = Roster();
            var first = roster.Create(Carrier());
            var id = first.Id;
            Assert.That(roster.Rename(id, "Renamed"), Is.EqualTo(MachineManagementResult.Completed));
            Assert.That(first.Id, Is.EqualTo(id));
            var second = roster.Create(Carrier());
            Assert.That(second.ModelSerial, Is.EqualTo(2));
            Assert.That(roster.Rename(second.Id, "Renamed"), Is.EqualTo(MachineManagementResult.DuplicateName));
            roster.RestoreHistoricalCounts(new[] { new KeyValuePair<int, ulong>(1, 40) });
            Assert.That(roster.Create(Carrier()).ModelSerial, Is.EqualTo(41));
            Assert.Throws<System.ArgumentException>(() => roster.RestoreHistoricalCounts(new[] { new KeyValuePair<int, ulong>(1, 2) }));
        }

        [Test]
        public void Hardware_UsesConfiguredSlotsSingleOwnerAndOneContainer()
        {
            var roster = Roster();
            var first = roster.Create(Carrier());
            var fixedMachine = roster.Create(Carrier(2, 1, 10));
            var cargo = roster.CreateComponent(new ComponentDefinition(2, HardwareKind.Effector, 1, 20, 0, 0, false));
            Assert.That(roster.Install(first.Id, ManagementOrigin.Library, cargo.Id, 0), Is.EqualTo(MachineManagementResult.Completed));
            Assert.That(first.TotalCapacity, Is.EqualTo(50));
            Assert.That(roster.Install(fixedMachine.Id, ManagementOrigin.Library, cargo.Id, 0), Is.EqualTo(MachineManagementResult.AlreadyInstalled));
            first.UpdateContainerUsage(31);
            Assert.That(roster.Remove(first.Id, ManagementOrigin.Library, HardwareKind.Effector, 0), Is.EqualTo(MachineManagementResult.CapacityInUse));
            first.UpdateContainerUsage(0);
            Assert.That(roster.Remove(first.Id, ManagementOrigin.Library, HardwareKind.Effector, 0), Is.EqualTo(MachineManagementResult.Completed));
            Assert.That(roster.Install(fixedMachine.Id, ManagementOrigin.Library, cargo.Id, 1), Is.EqualTo(MachineManagementResult.InvalidSlot));
            Assert.That(roster.Install(fixedMachine.Id, ManagementOrigin.Library, cargo.Id, 0), Is.EqualTo(MachineManagementResult.Completed));
            Assert.That(fixedMachine.TotalCapacity, Is.EqualTo(30));
        }

        [Test]
        public void State_PowerAndSleepPreserveSelectionsAndFieldPermissions()
        {
            var roster = Roster();
            var machine = roster.Create(Carrier());
            var sensor = roster.CreateComponent(new ComponentDefinition(3, HardwareKind.Sensor, 1, 0, 0, 0, false));
            roster.Install(machine.Id, ManagementOrigin.Library, sensor.Id, 0);
            roster.Deploy(machine.Id);
            machine.UpdateEnvironment(true, true);
            Assert.That(machine.Activate(ManagementOrigin.Hub), Is.EqualTo(MachineManagementResult.InvalidOrigin));
            Assert.That(machine.Activate(ManagementOrigin.Field), Is.EqualTo(MachineManagementResult.Completed));
            machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);
            Assert.That(machine.IsComponentWorking(HardwareKind.Sensor, 0), Is.True);
            machine.SetRunState(ManagementOrigin.Hub, MachineRunState.Sleeping);
            Assert.That(machine.Connected, Is.True);
            Assert.That(sensor.Enabled, Is.True);
            Assert.That(machine.IsComponentWorking(HardwareKind.Sensor, 0), Is.False);
            machine.SetRunState(ManagementOrigin.Hub, MachineRunState.Running);
            Assert.That(machine.SetPowerSwitch(ManagementOrigin.Hub, false), Is.EqualTo(MachineManagementResult.InvalidOrigin));
            machine.SetPowerSwitch(ManagementOrigin.Field, false);
            Assert.That(machine.RequestedRunState, Is.EqualTo(MachineRunState.Running));
            Assert.That(machine.Connected, Is.False);
            machine.SetPowerSwitch(ManagementOrigin.Field, true);
            Assert.That(machine.CanRun, Is.True);
            Assert.That(roster.Remove(machine.Id, ManagementOrigin.Field, HardwareKind.Sensor, 0), Is.EqualTo(MachineManagementResult.MustStop));
            machine.UpdateBehaviorActivity(true);
            Assert.That(machine.SetRunState(ManagementOrigin.Field, MachineRunState.Stopped), Is.EqualTo(MachineManagementResult.WaitingForSafeStop));
            Assert.That(roster.Remove(machine.Id, ManagementOrigin.Field, HardwareKind.Sensor, 0), Is.EqualTo(MachineManagementResult.WaitingForSafeStop));
            machine.UpdateBehaviorActivity(false);
            Assert.That(roster.Remove(machine.Id, ManagementOrigin.Field, HardwareKind.Sensor, 0), Is.EqualTo(MachineManagementResult.Completed));
            Assert.That(machine.Activated, Is.True);
        }
    }
}
