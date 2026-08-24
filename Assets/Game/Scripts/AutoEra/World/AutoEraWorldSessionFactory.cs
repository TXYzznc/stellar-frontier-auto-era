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
            var allocator = new PersistentIdAllocator();
            var registry = new PersistentObjectRegistry(allocator);
            var clock = new WorldClock(initialWorldMilliseconds);
            return new AutoEraWorldSession(allocator, registry, clock);
        }
    }
}
