using GameFramework;
using GameFramework.Event;

namespace AutoEra.Events
{
    /// <summary>
    /// Journal-only publisher for pure C# sessions and tests. It takes ownership of the fact
    /// argument and returns it to the reference pool without touching any event bus.
    /// </summary>
    public sealed class NullEventPublisher : IEventPublisher
    {
        public static readonly NullEventPublisher Instance = new NullEventPublisher();

        private NullEventPublisher() { }

        public void Publish(GameEventArgs args)
        {
            if (args != null) ReferencePool.Release(args);
        }
    }
}