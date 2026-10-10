using System;
using System.Collections.Generic;
using AutoEra.World;
using AutoEra.World.Identity;
using UnityEngine;
using AutoEra.Machines;
using AutoEra.Events;
using AutoEra.Logistics;
using GameFramework;

namespace AutoEra.ResourcePoints
{
    public sealed partial class ResourceProductionWorldService : IDisposable
    {
        private readonly AutoEraWorldSession _world;
        private readonly Dictionary<PersistentId, ForestProduction> _forests = new Dictionary<PersistentId, ForestProduction>();
        private readonly Dictionary<PersistentId, MineralProduction> _minerals = new Dictionary<PersistentId, MineralProduction>();
        private readonly List<ForestProduction> _orderedForests = new List<ForestProduction>();
        private readonly Dictionary<PersistentId, Vector3> _groundPositions = new Dictionary<PersistentId, Vector3>();
        private readonly Dictionary<PersistentId, Responsibility> _responsibilities = new Dictionary<PersistentId, Responsibility>();
        private readonly struct Responsibility
        {
            internal readonly PersistentId Machine, Component, Task, Behavior;
            internal readonly CorrelationId Correlation;
            internal Responsibility(PersistentId machine, PersistentId component, MachineTaskRecord task, PersistentId behavior)
            { Machine = machine; Component = component; Task = task.Id; Behavior = behavior; Correlation = task.Correlation; }
            internal Responsibility(PersistentId machine, PersistentId component, PersistentId task, PersistentId behavior,CorrelationId correlation)
            {Machine=machine;Component=component;Task=task;Behavior=behavior;Correlation=correlation;}
        }
        private bool _disposed;
        private bool _advancing;
        public ResourceProductionWorldService(AutoEraWorldSession world)
        { _world = world ?? throw new ArgumentNullException(nameof(world)); _world.Resources.Authority.Produced += OnProduced; }
        internal void Track(PersistentId producer, MachineExecutionContext context, ComponentInstance component, BehaviorRequest<EffectorBehaviorParameters> request)
        {
            if (context.Tasks.TryGet(request.TaskId, out var task))
                _responsibilities[producer] = new Responsibility(context.Machine.Id, component.Id, task, request.Id);
        }
        internal void ReleaseResponsibility(PersistentId producer) => _responsibilities.Remove(producer);
        internal bool MatchesResponsibility(PersistentId producer, PersistentId machine, PersistentId component, PersistentId task, PersistentId behavior)
            => _responsibilities.TryGetValue(producer,out var value) && value.Machine==machine && value.Component==component && value.Task==task && value.Behavior==behavior;
        internal bool TryGetPersistentProducedUnits(PersistentId producer,out int units) => _world.Resources.Authority.TryGetPersistentProducedUnits(producer,out units);
        private void OnProduced(ResourceProductionReceipt receipt)
        {
            if (!_responsibilities.TryGetValue(receipt.ProducerId, out var context) || !context.Correlation.IsValid) return;
            _world.Events.PublishFact(ReferencePool.Acquire<ResourceProductionFactEventArgs>().Initialize(context.Correlation,
                context.Machine, context.Component, context.Task, context.Behavior, receipt));
            if (receipt.Item == ResourceItemCatalog.Wood) _responsibilities.Remove(receipt.ProducerId);
        }
        public void BindGroundPosition(PersistentId point, Vector3 position)
        {
            if (_disposed || !point.IsValid || !ProductionRules.Finite(position.x) || !ProductionRules.Finite(position.y) || !ProductionRules.Finite(position.z)) throw new ArgumentException("Invalid production pile location.");
            if (_groundPositions.TryGetValue(point, out var prior) && prior != position) throw new InvalidOperationException("Ground pile location changed without a relocation transaction.");
            _groundPositions[point] = position;
        }
        public bool TryGetGroundPosition(PersistentId point, out Vector3 position) => _groundPositions.TryGetValue(point, out position);
        public bool TryGetForest(PersistentId id, out ForestProduction forest) => _forests.TryGetValue(id, out forest);
        public bool TryGetMineral(PersistentId id, out MineralProduction mineral) => _minerals.TryGetValue(id, out mineral);
        public ForestProduction RegisterForest(PersistentId id, ProductionRules rules, Vector3[] positions)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ResourceProductionWorldService));
            if (_forests.TryGetValue(id, out var existing)) return existing;
            if (_minerals.ContainsKey(id)) throw new ArgumentException("Resource identity already belongs to a deposit.");
            var forest = new ForestProduction(id, _world.IdAllocator, _world.ObjectRegistry, rules, _world.Resources.Authority, positions, _world.Clock.WorldMilliseconds);
            _forests.Add(id, forest); _orderedForests.Add(forest); _orderedForests.Sort((a, b) => a.Id.CompareTo(b.Id)); return forest;
        }
        public MineralProduction RegisterMineral(PersistentId id, int size, ulong seed, ProductionRules rules)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ResourceProductionWorldService));
            if (_minerals.TryGetValue(id, out var existing)) return existing;
            if (_forests.ContainsKey(id)) throw new ArgumentException("Resource identity already belongs to a forest.");
            var mineral = new MineralProduction(id, size, seed, rules, _world.Resources.Authority, _world.Clock.WorldMilliseconds);
            _minerals.Add(id, mineral); return mineral;
        }
        public void Advance(long now)
        {
            if (_disposed) return;
            if(_advancing)throw new InvalidOperationException("Production advancement is not reentrant.");
            _advancing=true;
            try {for (int i = 0; i < _orderedForests.Count; i++) _orderedForests[i].Advance(now);}
            finally {_advancing=false;}
        }
        public void Dispose()
        { if (_disposed) return; _disposed = true; _world.Resources.Authority.Produced -= OnProduced;
            foreach (var forest in _orderedForests) forest.Dispose(); _orderedForests.Clear(); _forests.Clear(); _minerals.Clear(); _groundPositions.Clear(); _responsibilities.Clear(); }
    }
}
