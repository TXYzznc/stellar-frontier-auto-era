using GameFramework.Event;

namespace AutoEra.Events
{
    /// <summary>
    /// Publishes a fact to the platform event bus. Implementations take ownership of the
    /// argument lifetime: they either dispatch it (the bus releases it after handling) or
    /// release it themselves when the fact cannot be delivered.
    /// </summary>
    public interface IEventPublisher
    {
        void Publish(GameEventArgs args);
    }
}