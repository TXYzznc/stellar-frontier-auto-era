using System;
using AutoEra.World.Time;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class UtcTimeEditModeTests
    {
        [Test]
        public void OfflineDuration_UsesInjectedUtcProviderInsteadOfSystemClock()
        {
            var provider = new FixedUtcTimeProvider(new DateTimeOffset(2026, 8, 24, 8, 30, 0, TimeSpan.Zero));
            DateTimeOffset saved = new DateTimeOffset(2026, 8, 24, 8, 0, 0, TimeSpan.Zero);

            Assert.That(TimeUtil.GetOfflineDuration(saved, provider), Is.EqualTo(TimeSpan.FromMinutes(30)));
        }

        [Test]
        public void OfflineDuration_UsesUtcInstantsRegardlessOfInputOffset()
        {
            DateTimeOffset savedAtUtcPlusEight = new DateTimeOffset(2026, 8, 24, 16, 0, 0, TimeSpan.FromHours(8));
            DateTimeOffset currentAtUtc = new DateTimeOffset(2026, 8, 24, 9, 30, 0, TimeSpan.Zero);

            Assert.That(TimeUtil.GetNonNegativeDuration(savedAtUtcPlusEight, currentAtUtc), Is.EqualTo(TimeSpan.FromMinutes(90)));
            long milliseconds = TimeUtil.ToUnixTimeMilliseconds(savedAtUtcPlusEight);
            Assert.That(TimeUtil.FromUnixTimeMilliseconds(milliseconds), Is.EqualTo(savedAtUtcPlusEight.ToUniversalTime()));
        }

        [Test]
        public void OfflineDuration_ClampsClockRollbackToZero()
        {
            var provider = new FixedUtcTimeProvider(new DateTimeOffset(2026, 8, 24, 7, 59, 59, TimeSpan.Zero));
            DateTimeOffset saved = new DateTimeOffset(2026, 8, 24, 8, 0, 0, TimeSpan.Zero);

            Assert.That(TimeUtil.GetOfflineDuration(saved, provider), Is.EqualTo(TimeSpan.Zero));
        }

        private sealed class FixedUtcTimeProvider : IUtcTimeProvider
        {
            private readonly DateTimeOffset _utcNow;

            public FixedUtcTimeProvider(DateTimeOffset utcNow)
            {
                _utcNow = utcNow;
            }

            public DateTimeOffset GetUtcNow()
            {
                return _utcNow;
            }
        }
    }
}
