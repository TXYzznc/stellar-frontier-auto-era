using System;
using AutoEra.Application;
using AutoEra.World;
using AutoEra.World.Time;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class AutoEraApplicationContextEditModeTests
    {
        [Test]
        public void Context_InjectsUtcProviderAndPreventsDuplicateActiveSession()
        {
            var provider = new FixedUtcTimeProvider(new DateTimeOffset(2026, 8, 24, 9, 0, 0, TimeSpan.Zero));
            using (var context = new AutoEraApplicationContext(provider, new AutoEraWorldSessionFactory()))
            {
                Assert.That(context.UtcTimeProvider, Is.SameAs(provider));
                Assert.That(context.TryCreateWorldSession(100L, out AutoEraWorldSession first), Is.True);
                Assert.That(first.IsActive, Is.True);
                Assert.That(context.TryCreateWorldSession(200L, out _), Is.False);
                Assert.That(context.ActiveWorldSession, Is.SameAs(first));
            }
        }

        [Test]
        public void ReleaseAndDispose_AreIdempotentAndAllowANewIndependentSession()
        {
            var context = new AutoEraApplicationContext(new FixedUtcTimeProvider(DateTimeOffset.UtcNow), new AutoEraWorldSessionFactory());
            Assert.That(context.TryCreateWorldSession(10L, out AutoEraWorldSession first), Is.True);

            context.ReleaseActiveWorldSession();
            context.ReleaseActiveWorldSession();
            Assert.That(first.IsActive, Is.False);
            Assert.That(context.ActiveWorldSession, Is.Null);
            Assert.That(context.TryCreateWorldSession(20L, out AutoEraWorldSession second), Is.True);
            Assert.That(second, Is.Not.SameAs(first));

            context.Dispose();
            context.Dispose();
            Assert.That(second.IsActive, Is.False);
            Assert.That(context.TryCreateWorldSession(30L, out _), Is.False);
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
