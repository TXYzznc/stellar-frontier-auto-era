using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class PersistentObjectRegistryEditModeTests
    {
        [Test]
        public void Register_RejectsInvalidAndDuplicateEntriesWithoutReplacingOriginal()
        {
            var registry = new PersistentObjectRegistry(new PersistentIdAllocator());
            var original = new object();
            PersistentId id = new PersistentId(8UL);

            Assert.That(registry.TryRegister(PersistentId.Invalid, PersistentObjectKind.Machine, original), Is.EqualTo(PersistentRegistryResult.InvalidId));
            Assert.That(registry.TryRegister(id, PersistentObjectKind.Machine, original), Is.EqualTo(PersistentRegistryResult.Success));
            Assert.That(registry.TryRegister(id, PersistentObjectKind.Building, new object()), Is.EqualTo(PersistentRegistryResult.DuplicateId));
            Assert.That(registry.TryResolve(id, PersistentObjectKind.Machine, out object resolved), Is.EqualTo(PersistentRegistryResult.Success));
            Assert.That(resolved, Is.SameAs(original));
        }

        [Test]
        public void ResolveAndUnregister_ExposeKindAndInstanceMismatches()
        {
            var registry = new PersistentObjectRegistry(new PersistentIdAllocator());
            PersistentId id = new PersistentId(11UL);
            var registered = new object();

            Assert.That(registry.TryRegister(id, PersistentObjectKind.Machine, registered), Is.EqualTo(PersistentRegistryResult.Success));
            Assert.That(registry.TryResolve(id, PersistentObjectKind.Building, out object mismatched), Is.EqualTo(PersistentRegistryResult.KindMismatch));
            Assert.That(mismatched, Is.Null);
            Assert.That(registry.TryUnregister(id, PersistentObjectKind.Machine, new object()), Is.EqualTo(PersistentRegistryResult.InstanceMismatch));
            Assert.That(registry.TryUnregister(id, PersistentObjectKind.Building), Is.EqualTo(PersistentRegistryResult.KindMismatch));
            Assert.That(registry.TryUnregister(id, PersistentObjectKind.Machine, registered), Is.EqualTo(PersistentRegistryResult.Success));
            Assert.That(registry.TryUnregister(id, PersistentObjectKind.Machine), Is.EqualTo(PersistentRegistryResult.Missing));
        }

        [Test]
        public void Register_RestoredIdAdvancesAllocatorAndClearDropsAllReferences()
        {
            var allocator = new PersistentIdAllocator();
            var registry = new PersistentObjectRegistry(allocator);

            Assert.That(registry.TryRegister(new PersistentId(20UL), PersistentObjectKind.Building, new object()), Is.EqualTo(PersistentRegistryResult.Success));
            Assert.That(allocator.TryAllocate(out PersistentId next), Is.True);
            Assert.That(next.Value, Is.EqualTo(21UL));

            registry.Clear();
            Assert.That(registry.Count, Is.EqualTo(0));
            Assert.That(registry.TryResolve(new PersistentId(20UL), PersistentObjectKind.Building, out _), Is.EqualTo(PersistentRegistryResult.Missing));
        }
    }
}
