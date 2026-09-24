using System;
using AutoEra.Algorithms;
using AutoEra.Events;
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

        internal AutoEraWorldSession(PersistentIdAllocator idAllocator, PersistentObjectRegistry objectRegistry, WorldClock clock,
            AutoEraEventService events)
        {
            IdAllocator = idAllocator ?? throw new ArgumentNullException(nameof(idAllocator));
            ObjectRegistry = objectRegistry ?? throw new ArgumentNullException(nameof(objectRegistry));
            Clock = clock ?? throw new ArgumentNullException(nameof(clock));
            Events = events ?? throw new ArgumentNullException(nameof(events));
            Machines = new AutoEra.Machines.MachineRoster(IdAllocator, ObjectRegistry);
            AlgorithmTemplates = new AlgorithmTemplateLibrary(IdAllocator);
            InitialAlgorithmTemplates.Seed(AlgorithmTemplates);
        }

        public PersistentIdAllocator IdAllocator { get; }

        public PersistentObjectRegistry ObjectRegistry { get; }

        public WorldClock Clock { get; }
        public AutoEraEventService Events { get; }
        public AutoEra.Machines.MachineRoster Machines { get; }

        /// <summary>世界级算法模板库：跨机器共享的模板列表/详情/实例化数据源，生命周期随世界。</summary>
        public AlgorithmTemplateLibrary AlgorithmTemplates { get; }

        public bool IsActive => !_isDisposed;

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            Machines.Dispose();
            Events.Dispose();
            ObjectRegistry.Clear();
        }
    }
}
