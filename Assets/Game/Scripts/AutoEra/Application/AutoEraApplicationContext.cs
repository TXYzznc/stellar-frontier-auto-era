using System;
using AutoEra.World;
using AutoEra.World.Time;

namespace AutoEra.Application
{
    /// <summary>
    /// Application-lifetime owner for injectable services and one active world session.
    /// It intentionally provides no global accessor; product procedures receive it through
    /// their controlled procedure context in a later integration step.
    /// </summary>
    public sealed class AutoEraApplicationContext : IDisposable
    {
        private bool _isDisposed;

        public AutoEraApplicationContext(IUtcTimeProvider utcTimeProvider, AutoEraWorldSessionFactory worldSessionFactory)
        {
            UtcTimeProvider = utcTimeProvider ?? throw new ArgumentNullException(nameof(utcTimeProvider));
            WorldSessionFactory = worldSessionFactory ?? throw new ArgumentNullException(nameof(worldSessionFactory));
        }

        public IUtcTimeProvider UtcTimeProvider { get; }

        public AutoEraWorldSessionFactory WorldSessionFactory { get; }

        public AutoEraWorldSession ActiveWorldSession { get; private set; }

        public bool IsDisposed => _isDisposed;

        public bool TryCreateWorldSession(long initialWorldMilliseconds, out AutoEraWorldSession session)
        {
            session = null;
            if (_isDisposed || ActiveWorldSession != null)
            {
                return false;
            }

            session = WorldSessionFactory.Create(initialWorldMilliseconds);
            ActiveWorldSession = session;
            return true;
        }

        public void ReleaseActiveWorldSession()
        {
            if (ActiveWorldSession == null)
            {
                return;
            }

            ActiveWorldSession.Dispose();
            ActiveWorldSession = null;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            ReleaseActiveWorldSession();
        }
    }
}
