using AutoEra.World.Time;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class WorldDayNightRulesEditModeTests
    {
        [Test]
        public void FirstVersionBoundaries_SwitchAtTheConfiguredSunlightEnd()
        {
            var rules = new WorldDayNightRules(1440000L, 960000L);

            Assert.That(rules.GetPhase(959999L), Is.EqualTo(WorldDayNightPhase.Sunlit));
            Assert.That(rules.GetPhase(960000L), Is.EqualTo(WorldDayNightPhase.Dark));
            Assert.That(rules.GetPhase(1439999L), Is.EqualTo(WorldDayNightPhase.Dark));
            Assert.That(rules.GetPhase(1440000L), Is.EqualTo(WorldDayNightPhase.Sunlit));
        }

        [Test]
        public void ConstructionAndQueries_RejectInvalidDurationsAndTime()
        {
            Assert.That(() => new WorldDayNightRules(0L, 0L), Throws.TypeOf<System.ArgumentOutOfRangeException>());
            Assert.That(() => new WorldDayNightRules(10L, -1L), Throws.TypeOf<System.ArgumentOutOfRangeException>());
            Assert.That(() => new WorldDayNightRules(10L, 11L), Throws.TypeOf<System.ArgumentOutOfRangeException>());
            Assert.That(() => new WorldDayNightRules(10L, 5L).GetPhase(-1L), Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }
    }
}
