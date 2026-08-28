using AutoEra.World;
using AutoEra.World.Time;

namespace AutoEra.Application
{
    /// <summary>
    /// Creates the small application-lifetime object graph. Procedures receive its result
    /// through the dedicated procedure context slot; this type is not a service locator.
    /// </summary>
    public sealed class AutoEraApplicationCompositionRoot
    {
        public AutoEraApplicationContext Create()
        {
            return Create(new SystemUtcTimeProvider(), new AutoEraWorldSessionFactory());
        }

        public AutoEraApplicationContext Create(
            IUtcTimeProvider utcTimeProvider,
            AutoEraWorldSessionFactory worldSessionFactory)
        {
            return new AutoEraApplicationContext(utcTimeProvider, worldSessionFactory);
        }
    }
}
