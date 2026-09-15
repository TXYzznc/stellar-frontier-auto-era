using System;
using AutoEra.World.Identity;
using AutoEra.World.Time;

namespace AutoEra.World
{
    /// <summary>
    /// Owns state that is valid for exactly one loaded world. It contains no static state
    /// and is safe to construct in EditMode without a Unity scene.
    /// </summary>
    public sealed class AutoEraWorldSession : IDisposable
    {
        private bool _isDisposed;

        internal AutoEraWorldSession(PersistentIdAllocator idAllocator, PersistentObjectRegistry objectRegistry, WorldClock clock)
        {
            IdAllocator = idAllocator ?? throw new ArgumentNullException(nameof(idAllocator));
            ObjectRegistry = objectRegistry ?? throw new ArgumentNullException(nameof(objectRegistry));
            Clock = clock ?? throw new ArgumentNullException(nameof(clock));
            Machines = new AutoEra.Machines.MachineRoster(IdAllocator, ObjectRegistry);
        }

        public PersistentIdAllocator IdAllocator { get; }

        public PersistentObjectRegistry ObjectRegistry { get; }

        public WorldClock Clock { get; }
        public AutoEra.Machines.MachineRoster Machines { get; }

        public bool IsActive => !_isDisposed;

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            Machines.Dispose();
            ObjectRegistry.Clear();
        }
    }
}
