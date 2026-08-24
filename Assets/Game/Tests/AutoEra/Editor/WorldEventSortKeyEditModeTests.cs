using System.Collections.Generic;
using AutoEra.World.Identity;
using AutoEra.World.Time;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class WorldEventSortKeyEditModeTests
    {
        [Test]
        public void Sort_IsStableAcrossDifferentInsertionOrders()
        {
            var keys = new[]
            {
                new WorldEventSortKey(100L, WorldEventPhase.TaskAndBehavior, new PersistentId(4UL), 2UL),
                new WorldEventSortKey(99L, WorldEventPhase.ResourceAndReward, new PersistentId(1UL), 1UL),
                new WorldEventSortKey(100L, WorldEventPhase.Energy, new PersistentId(9UL), 1UL),
                new WorldEventSortKey(100L, WorldEventPhase.TaskAndBehavior, new PersistentId(4UL), 1UL),
                new WorldEventSortKey(100L, WorldEventPhase.TaskAndBehavior, new PersistentId(2UL), 9UL),
            };

            var firstOrder = new List<WorldEventSortKey> { keys[0], keys[1], keys[2], keys[3], keys[4] };
            var secondOrder = new List<WorldEventSortKey> { keys[4], keys[3], keys[2], keys[1], keys[0] };
            firstOrder.Sort();
            secondOrder.Sort();

            Assert.That(secondOrder, Is.EqualTo(firstOrder));
            Assert.That(firstOrder, Is.EqualTo(new[] { keys[1], keys[2], keys[4], keys[3], keys[0] }));
        }

        [Test]
        public void Construction_RejectsNegativeTimeAndInvalidPersistentId()
        {
            Assert.That(() => new WorldEventSortKey(-1L, WorldEventPhase.Energy, new PersistentId(1UL), 1UL), Throws.TypeOf<System.ArgumentOutOfRangeException>());
            Assert.That(() => new WorldEventSortKey(0L, WorldEventPhase.Energy, PersistentId.Invalid, 1UL), Throws.TypeOf<System.ArgumentException>());
        }
    }
}
