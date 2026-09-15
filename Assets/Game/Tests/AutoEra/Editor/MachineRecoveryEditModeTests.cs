using System;
using System.Linq;
using AutoEra.Machines;
using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class MachineRecoveryEditModeTests
    {
        private static MachineDefinition Definition(int id, int level) => new MachineDefinition(id, "Fixture", level, 2, 1, 2, 30, true, true, 100);
        private static ComponentDefinition Component(int id, int level) => new ComponentDefinition(id, HardwareKind.Sensor, level, 0, 0, 0, false);

        [Test]
        public void Configuration_RoundTripPreservesOwnershipButReacquiresEnvironment()
        {
            var ids = new PersistentIdAllocator(); var registry = new PersistentObjectRegistry(ids);
            var source = new MachineRoster(ids, registry);
            var machine = source.Create(Definition(1, 1)); var component = source.CreateComponent(Component(2, 1));
            source.Install(machine.Id, ManagementOrigin.Library, component.Id, 1);
            source.Rename(machine.Id, "Named"); source.Deploy(machine.Id);
            machine.UpdateEnvironment(true, true); machine.Activate(ManagementOrigin.Field);
            machine.SetComponentEnabled(ManagementOrigin.Field, HardwareKind.Sensor, 1, false);
            machine.SetRunState(ManagementOrigin.Field, MachineRunState.Sleeping);
            var snapshot = source.CaptureConfiguration(); source.Dispose();
            Assert.That(registry.Count, Is.Zero);
            var restored = new MachineRoster(ids, registry); restored.RestoreConfiguration(snapshot, Definition, Component);
            Assert.That(restored.TryGet(machine.Id, out var next), Is.True);
            Assert.That(next.Name, Is.EqualTo("Named"));
            Assert.That(next.RequestedRunState, Is.EqualTo(MachineRunState.Sleeping));
            Assert.That(next.Connected, Is.False); Assert.That(next.Activated, Is.True);
            Assert.That(next.GetComponent(HardwareKind.Sensor, 1).Id, Is.EqualTo(component.Id));
            Assert.That(next.GetComponent(HardwareKind.Sensor, 1).Enabled, Is.False);
            next.UpdateEnvironment(true, true); Assert.That(next.Connected, Is.True);
            next.SetRunState(ManagementOrigin.Field, MachineRunState.Stopped);
            Assert.That(restored.RecoverToLibrary(next.Id, ManagementOrigin.Field), Is.EqualTo(MachineManagementResult.Completed));
            Assert.That(next.Deployed, Is.False);
            Assert.That(restored.Create(Definition(1, 1)).ModelSerial, Is.EqualTo(2));
            restored.Dispose(); Assert.That(registry.Count, Is.Zero);
            Assert.Throws<ObjectDisposedException>(() => restored.Create(Definition(1, 1)));
        }

        [Test]
        public void InvalidRecovery_IsAtomicAndActiveBehaviorCannotBeSilentlyLost()
        {
            var ids = new PersistentIdAllocator(); var source = new MachineRoster(ids, new PersistentObjectRegistry(ids));
            var machine = source.Create(Definition(1, 1));
            machine.UpdateBehaviorActivity(true);
            Assert.Throws<InvalidOperationException>(() => source.CaptureConfiguration());
            machine.UpdateBehaviorActivity(false);
            var snapshot = source.CaptureConfiguration(); snapshot.Machines[0].UsedCapacity = 999;
            var targetIds = new PersistentIdAllocator(); var registry = new PersistentObjectRegistry(targetIds);
            var target = new MachineRoster(targetIds, registry);
            Assert.Throws<ArgumentException>(() => target.RestoreConfiguration(snapshot, Definition, Component));
            Assert.That(target.Machines.Count(), Is.Zero); Assert.That(registry.Count, Is.Zero);
            Assert.That(targetIds.NextId.Value, Is.EqualTo(1));
            snapshot.Machines[0].UsedCapacity = 0;
            target.RestoreConfiguration(snapshot, Definition, Component);
            Assert.That(target.Create(Definition(1, 1)).Id.Value, Is.GreaterThan(machine.Id.Value));
        }
    }
}
