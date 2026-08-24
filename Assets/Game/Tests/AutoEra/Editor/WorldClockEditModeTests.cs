using AutoEra.World.Time;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class WorldClockEditModeTests
    {
        [Test]
        public void RealtimeFramePartitions_ProduceTheSameWholeMilliseconds()
        {
            var wholeSecond = new WorldClock();
            var sixtyFrames = new WorldClock();

            Assert.That(wholeSecond.TryAdvanceRealtimeSeconds(1d), Is.True);
            for (int i = 0; i < 60; i++)
            {
                Assert.That(sixtyFrames.TryAdvanceRealtimeSeconds(1d / 60d), Is.True);
            }

            Assert.That(wholeSecond.WorldMilliseconds, Is.EqualTo(1000L));
            Assert.That(sixtyFrames.WorldMilliseconds, Is.EqualTo(wholeSecond.WorldMilliseconds));
        }

        [Test]
        public void FractionalRealtime_IsRetainedUntilItFormsAWholeMillisecond()
        {
            var clock = new WorldClock();

            Assert.That(clock.TryAdvanceRealtimeSeconds(0.0004d), Is.True);
            Assert.That(clock.WorldMilliseconds, Is.EqualTo(0L));
            Assert.That(clock.FractionalMilliseconds, Is.EqualTo(0.4d).Within(0.000001d));
            Assert.That(clock.TryAdvanceRealtimeSeconds(0.0006d), Is.True);
            Assert.That(clock.WorldMilliseconds, Is.EqualTo(1L));
            Assert.That(clock.FractionalMilliseconds, Is.EqualTo(0d).Within(0.000001d));
        }

        [Test]
        public void NegativeBackwardAndOverflowRequests_AreRejectedWithoutChangingTime()
        {
            var clock = new WorldClock(10L);

            Assert.That(clock.TryAdvanceMilliseconds(-1L), Is.False);
            Assert.That(clock.TryAdvanceTo(9L), Is.False);
            Assert.That(clock.TryAdvanceRealtimeSeconds(-0.01d), Is.False);
            Assert.That(clock.WorldMilliseconds, Is.EqualTo(10L));

            var nearLimit = new WorldClock(long.MaxValue - 1L);
            Assert.That(nearLimit.TryAdvanceMilliseconds(2L), Is.False);
            Assert.That(nearLimit.TryAdvanceRealtimeSeconds(0.002d), Is.False);
            Assert.That(nearLimit.WorldMilliseconds, Is.EqualTo(long.MaxValue - 1L));
        }
    }
}
