using AutoEra.World;
using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class AutoEraWorldSessionEditModeTests
    {
        [Test]
        public void Dispose_IsIdempotentAndClearsWorldLocalRegistry()
        {
            var session = new AutoEraWorldSessionFactory().Create(123L);
            PersistentId id = new PersistentId(7UL);
            Assert.That(session.ObjectRegistry.TryRegister(id, PersistentObjectKind.Machine, new object()), Is.EqualTo(PersistentRegistryResult.Success));

            session.Dispose();
            session.Dispose();

            Assert.That(session.IsActive, Is.False);
            Assert.That(session.ObjectRegistry.Count, Is.EqualTo(0));
            Assert.That(session.ObjectRegistry.TryResolve(id, PersistentObjectKind.Machine, out _), Is.EqualTo(PersistentRegistryResult.Missing));
        }

        [Test]
        public void Factory_CreatesIndependentWorldStateForEachSession()
        {
            var factory = new AutoEraWorldSessionFactory();
            var first = factory.Create(10L);
            var second = factory.Create(20L);

            Assert.That(first.Clock.WorldMilliseconds, Is.EqualTo(10L));
            Assert.That(second.Clock.WorldMilliseconds, Is.EqualTo(20L));
            Assert.That(first.IdAllocator.TryAllocate(out PersistentId firstId), Is.True);
            Assert.That(second.IdAllocator.TryAllocate(out PersistentId secondId), Is.True);
            Assert.That(firstId.Value, Is.EqualTo(1UL));
            Assert.That(secondId.Value, Is.EqualTo(1UL));

            first.Dispose();
            Assert.That(second.IsActive, Is.True);
            Assert.That(second.ObjectRegistry.Count, Is.EqualTo(0));
        }
    }
}
