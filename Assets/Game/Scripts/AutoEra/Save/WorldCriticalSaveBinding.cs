using System;
using System.Collections.Generic;
using AutoEra.Algorithms;
using AutoEra.Machines;
using AutoEra.World.Region;

namespace AutoEra.Save
{
    /// <summary>Low-frequency successful management/apply commits request saves; runtime projections only use the periodic dirty marker.</summary>
    public sealed class WorldCriticalSaveBinding : IDisposable
    {
        private readonly MachineRoster _roster;
        private readonly RegionMachineRuntimeRegistry _registry;
        private readonly WorldSaveCoordinator _saving;
        private readonly HashSet<AlgorithmInstanceService> _instances=new HashSet<AlgorithmInstanceService>();
        private bool _disposed;
        public WorldCriticalSaveBinding(MachineRoster roster,RegionMachineRuntimeRegistry registry,WorldSaveCoordinator saving)
        {
            _roster=roster ?? throw new ArgumentNullException(nameof(roster));_registry=registry ?? throw new ArgumentNullException(nameof(registry));_saving=saving ?? throw new ArgumentNullException(nameof(saving));
            roster.PersistentConfigurationChanged+=Request;roster.DeploymentChanged+=OnDeployment;
            registry.RuntimeAdded+=OnAdded;registry.RuntimeRemoved+=OnRemoved;
            foreach(var runtime in registry.Runtimes)OnAdded(runtime);
        }
        private void Request() { if(!_disposed)_saving.MarkDirty(true); }
        private void OnDeployment(MachineInstance machine)=>Request();
        private void OnAdded(RegionMachineRuntime runtime)
        { if(runtime.Instances!=null && _instances.Add(runtime.Instances))runtime.Instances.PersistentApplied+=Request; }
        private void OnRemoved(RegionMachineRuntime runtime)
        { if(runtime.Instances!=null && _instances.Remove(runtime.Instances))runtime.Instances.PersistentApplied-=Request; }
        public void Dispose()
        {
            if(_disposed)return;_disposed=true;
            _roster.PersistentConfigurationChanged-=Request;_roster.DeploymentChanged-=OnDeployment;
            _registry.RuntimeAdded-=OnAdded;_registry.RuntimeRemoved-=OnRemoved;
            foreach(var instance in _instances)instance.PersistentApplied-=Request;_instances.Clear();
        }
    }
}
