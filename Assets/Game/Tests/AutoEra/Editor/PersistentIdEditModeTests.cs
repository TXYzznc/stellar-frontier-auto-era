using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class PersistentIdEditModeTests
    {
        [Test]
        public void ValueSemantics_KeepZeroInvalidAndProvideStableFormatting()
        {
            PersistentId id = new PersistentId(42UL);

            Assert.That(PersistentId.Invalid.IsValid, Is.False);
            Assert.That(id.IsValid, Is.True);
            Assert.That(id, Is.EqualTo(new PersistentId(42UL)));
            Assert.That(id, Is.GreaterThan(new PersistentId(41UL)));
            Assert.That(id.ToString(), Is.EqualTo("42"));
            Assert.That(PersistentId.TryParse("42", out PersistentId parsed), Is.True);
            Assert.That(parsed, Is.EqualTo(id));
        }

        [Test]
        public void Allocator_UsesOneMonotonicSequenceForAllCallers()
        {
            var allocator = new PersistentIdAllocator();

            Assert.That(allocator.TryAllocate(out PersistentId machine), Is.True);
            Assert.That(allocator.TryAllocate(out PersistentId building), Is.True);
            Assert.That(allocator.TryAllocate(out PersistentId task), Is.True);

            Assert.That(machine.Value, Is.EqualTo(1UL));
            Assert.That(building.Value, Is.EqualTo(2UL));
            Assert.That(task.Value, Is.EqualTo(3UL));
        }

        [Test]
        public void Restore_UnorderedIdsAdvancesHighWaterWithoutReusingValues()
        {
            var allocator = new PersistentIdAllocator();

            Assert.That(allocator.TryRestore(new PersistentId(20UL)), Is.True);
            Assert.That(allocator.TryRestore(new PersistentId(5UL)), Is.True);
            Assert.That(allocator.TryRestore(new PersistentId(12UL)), Is.True);
            Assert.That(allocator.TryAllocate(out PersistentId allocated), Is.True);

            Assert.That(allocated.Value, Is.EqualTo(21UL));
        }

        [Test]
        public void Restore_InvalidIdIsRejectedAndMaximumIdExhaustsSequenceWithoutWrapping()
        {
            var allocator = new PersistentIdAllocator();

            Assert.That(allocator.TryRestore(PersistentId.Invalid), Is.False);
            Assert.That(allocator.TryRestore(new PersistentId(ulong.MaxValue)), Is.True);
            Assert.That(allocator.IsExhausted, Is.True);
            Assert.That(allocator.TryAllocate(out PersistentId allocated), Is.False);
            Assert.That(allocated, Is.EqualTo(PersistentId.Invalid));
        }
    }
}
