using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class PersistentObjectReferenceEditModeTests
    {
        [Test]
        public void MissingTarget_RemainsUnresolvedAndRetainsItsIdentity()
        {
            PersistentId targetId = new PersistentId(17UL);
            var reference = new PersistentObjectReference(targetId, PersistentObjectKind.Machine);
            var registry = new PersistentObjectRegistry(new PersistentIdAllocator());

            Assert.That(reference.TryResolve(registry, out object resolved), Is.EqualTo(PersistentRegistryResult.Missing));
            Assert.That(resolved, Is.Null);
            Assert.That(reference.Id, Is.EqualTo(targetId));
            Assert.That(reference.ExpectedKind, Is.EqualTo(PersistentObjectKind.Machine));
        }

        [Test]
        public void ReloadedWorld_ResolvesOnlyTheSameIdAndExpectedKind()
        {
            PersistentId targetId = new PersistentId(21UL);
            var reference = new PersistentObjectReference(targetId, PersistentObjectKind.Building);
            var firstWorld = new PersistentObjectRegistry(new PersistentIdAllocator());
            var original = new NamedObject("Depot");
            Assert.That(firstWorld.TryRegister(targetId, PersistentObjectKind.Building, original), Is.EqualTo(PersistentRegistryResult.Success));
            Assert.That(reference.TryResolve(firstWorld, out object resolvedOriginal), Is.EqualTo(PersistentRegistryResult.Success));
            Assert.That(resolvedOriginal, Is.SameAs(original));

            var reloadedWorld = new PersistentObjectRegistry(new PersistentIdAllocator());
            var sameNameDifferentId = new NamedObject("Depot");
            Assert.That(reloadedWorld.TryRegister(new PersistentId(22UL), PersistentObjectKind.Building, sameNameDifferentId), Is.EqualTo(PersistentRegistryResult.Success));
            Assert.That(reference.TryResolve(reloadedWorld, out object unresolved), Is.EqualTo(PersistentRegistryResult.Missing));
            Assert.That(unresolved, Is.Null);

            var restored = new NamedObject("Depot");
            Assert.That(reloadedWorld.TryRegister(targetId, PersistentObjectKind.Building, restored), Is.EqualTo(PersistentRegistryResult.Success));
            Assert.That(reference.TryResolve(reloadedWorld, out object resolvedRestored), Is.EqualTo(PersistentRegistryResult.Success));
            Assert.That(resolvedRestored, Is.SameAs(restored));
        }

        [Test]
        public void CrossWorldRegistry_DoesNotInheritEntriesFromPreviousWorld()
        {
            PersistentId targetId = new PersistentId(5UL);
            var reference = new PersistentObjectReference(targetId, PersistentObjectKind.Task);
            var firstWorld = new PersistentObjectRegistry(new PersistentIdAllocator());
            Assert.That(firstWorld.TryRegister(targetId, PersistentObjectKind.Task, new object()), Is.EqualTo(PersistentRegistryResult.Success));
            Assert.That(reference.TryResolve(firstWorld, out _), Is.EqualTo(PersistentRegistryResult.Success));

            var secondWorld = new PersistentObjectRegistry(new PersistentIdAllocator());
            Assert.That(reference.TryResolve(secondWorld, out _), Is.EqualTo(PersistentRegistryResult.Missing));
        }

        private sealed class NamedObject
        {
            public NamedObject(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }
    }
}
