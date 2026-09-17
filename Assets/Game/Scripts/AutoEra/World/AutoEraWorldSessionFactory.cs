using AutoEra.Events;
using AutoEra.World.Identity;
using AutoEra.World.Time;

namespace AutoEra.World
{
    /// <summary>
    /// Creates isolated session graphs. The application context owns this factory rather
    /// than exposing a global registry or service locator.
    /// </summary>
    public sealed class AutoEraWorldSessionFactory
    {
        public AutoEraWorldSession Create(long initialWorldMilliseconds)
        {
            return Create(initialWorldMilliseconds, null);
        }

        /// <summary>Journal-only sessions pass null; runtime passes the application event publisher.</summary>
        public AutoEraWorldSession Create(long initialWorldMilliseconds, IEventPublisher eventPublisher)
        {
            var allocator = new PersistentIdAllocator();
            var registry = new PersistentObjectRegistry(allocator);
            var clock = new WorldClock(initialWorldMilliseconds);
            var events = new AutoEraEventService(clock, eventPublisher);
            return new AutoEraWorldSession(allocator, registry, clock, events);
        }
    }
}