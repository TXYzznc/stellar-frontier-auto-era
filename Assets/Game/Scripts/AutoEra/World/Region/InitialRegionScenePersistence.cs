using System;
using System.Collections.Generic;
using AutoEra.Machines;
using AutoEra.Save;
using AutoEra.World.Identity;
using GameFramework;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace AutoEra.World.Region
{
    /// <summary>Actual quantity/energy domains bind restored facility facts before execution resumes.</summary>
    public interface IRegionWorldPersistence : IWorldDomainPersistence
    {
        bool TryBindScene(InitialRegionScene scene,WorldRestoreCandidate candidate,out string reason);
        MachineNavigationTarget ResolveNavigation(InitialRegionScene scene,MachineNavigationTargetSnapshot target);
    }
    public sealed partial class InitialRegionScene : IWorldSnapshotSource
    {
        private readonly Dictionary<PersistentId,RegionPresentationBinding> _persistentBindings=new Dictionary<PersistentId,RegionPresentationBinding>();
        private readonly Dictionary<PersistentId,RegionObjectView> _persistentViews=new Dictionary<PersistentId,RegionObjectView>();
        private IRegionWorldPersistence _persistentDomains;
        private WorldCriticalSaveBinding _criticalSaving;
        private bool _restoringPresentation;
        public bool BlocksNewPlayerCommands { get; internal set; }
        public IEnumerable<AutoEra.ResourcePoints.RegionProductionFacility> ProductionFacilities => _productionFacilities;
        public AutoEra.Energy.EnergyEventRecorderSnapshot CaptureEnergyRecorderPersistent() => _energyRecorder?.CapturePersistent();
        public bool TryAdoptPersistentEnergy(RegionEnergyService saved,AutoEra.Energy.EnergyEventRecorderSnapshot recorder,out string reason)
        {
            reason="原能源服务与场景设施不匹配";if(saved==null || ReferenceEquals(saved,_energy))return false;
            var views=_energy?.Facilities ?? Array.Empty<RegionEnergyFacility>();
            var restoredRecorder=new AutoEra.Energy.EnergyEventRecorder();
            if(recorder!=null && !restoredRecorder.RestorePersistent(recorder) || !saved.TryBindPersistentFacilities(views,out reason))return false;
            _energy?.Dispose();_energy=saved;_energyRecorder=recorder==null ? null : restoredRecorder;
            // No execution hosts exist yet. Reacquire supply from the saved authoritative result before they subscribe.
            _energy.ApplySupply();reason=null;return true;
        }

        public void BindPersistence(IRegionWorldPersistence domains) => _persistentDomains=domains ?? throw new ArgumentNullException(nameof(domains));
        public void BindSaveRequests(WorldSaveCoordinator saving)
        { _criticalSaving?.Dispose();_criticalSaving=new WorldCriticalSaveBinding(_session.Machines,_runtimes,saving); }
        private void RegisterPersistentView(RegionObjectView view,int seed,string asset)
        {
            if(view?.Model==null || string.IsNullOrWhiteSpace(asset))throw new InvalidOperationException("Persistent view binding is incomplete.");
            _persistentViews[view.Model.Id]=view;
            _persistentBindings[view.Model.Id]=new RegionPresentationBinding {Object=view.Model.Id.Value,SeedIndex=seed,ContentVersion=1,Asset=asset};
        }
        private void ClearPersistenceBinding()
        { _criticalSaving?.Dispose();_criticalSaving=null;_persistentViews.Clear();_persistentBindings.Clear();_persistentDomains=null;_restoringPresentation=false;BlocksNewPlayerCommands=false; }
        private List<RegionWorkQueue> PersistentQueues()
        {
            var queues=new List<RegionWorkQueue>();var seen=new HashSet<RegionWorkQueue>();
            foreach(var pair in _persistentViews)
            {
                if(pair.Value==null || !Region.TryGet(pair.Key,out _))continue;
                for(int i=0;;i++) { var queue=pair.Value.GetWorkChannel(i);if(queue==null)break;if(seen.Add(queue))queues.Add(queue); }
            }
            foreach(var endpoint in _transferEndpoints.Values)if(endpoint?.Queue!=null && seen.Add(endpoint.Queue))queues.Add(endpoint.Queue);
            return queues;
        }
        public bool TryCapture(long revision,out WorldSnapshotDocument snapshot,out string reason)
        {
            snapshot=null;reason="等待区域完整提交边界";
            if(_advancing || _restoringPresentation || _pendingEntities.Count!=0 || _session==null || Region==null || _runtimes==null || _persistentDomains==null)return false;
            var bindings=new List<RegionPresentationBinding>();
            foreach(var obj in Region.Objects)
            { if(!_persistentBindings.TryGetValue(obj.Id,out var binding)) {reason="区域对象缺少原资产绑定";return false;}bindings.Add(binding); }
            bindings.Sort((a,b)=>a.Object.CompareTo(b.Object));
            var queues=PersistentQueues();var states=new RegionWorkQueueSnapshot[queues.Count];
            for(int i=0;i<states.Length;i++)states[i]=queues[i].CapturePersistentState();
            return WorldPersistenceProfile.TryCapture(_session,Region,_runtimes,UnityEngine.Time.realtimeSinceStartupAsDouble,
                states,bindings.ToArray(),_persistentDomains,revision,"初始区域",out snapshot,out reason);
        }

        public void InitializePersistent(WorldRestoreCandidate candidate,IRegionWorldPersistence domains,Action ready,Action<string> failed)
        {
            if(Region!=null || candidate==null || candidate.IsReady || domains==null)throw new InvalidOperationException("Invalid persistent region initialization.");
            if(candidate.Region.Bounds!=_bounds || _objects==null || _entityPrefabs==null || _objects.Length!=_entityPrefabs.Length)
                throw new InvalidOperationException("Region content bounds or seed configuration changed.");
            // Check every explicit asset binding before showing any entity. Display names never select identities or assets.
            foreach(var binding in candidate.Territory.Presentation)
            {
                if(binding.ContentVersion!=1)throw new InvalidOperationException("Unsupported region content version.");
                if(binding.SeedIndex>=0)
                {
                    if(binding.SeedIndex>=_objects.Length || _objects[binding.SeedIndex]==null || _entityPrefabs[binding.SeedIndex]!=binding.Asset)
                        throw new InvalidOperationException("Saved seed asset no longer matches this content version.");
                }
                else if(!candidate.World.Machines.TryGet(new PersistentId(binding.Object),out var machine) || !machine.Definition.HasPrefab || machine.Definition.Prefab!=binding.Asset)
                    throw new InvalidOperationException("Saved machine asset no longer matches its definition.");
            }
            _session=candidate.World;Region=candidate.Region;_persistentDomains=domains;_restoringPresentation=true;_failure=failed;
            int version=++_entityVersion,remaining=candidate.Territory.Presentation.Length;
            GF.Event.Subscribe(ShowEntityFailureEventArgs.EventId,OnEntityFailure);_entityEvents=true;
            Action finish=()=>
            {
                try
                {
                    if(!domains.TryBindScene(this,candidate,out var reason))throw new InvalidOperationException(reason ?? "Restored scene domains are incomplete.");
                    InitializeNavigation();
                    foreach(var pair in _persistentViews)
                        if(_session.Machines.TryGet(pair.Key,out var machine))
                        {
                            _productionTools?.RegisterMachineView(machine,pair.Value.gameObject);
                            if(!_runtimes.TryAttach(machine,pair.Value.gameObject,out _,out reason))throw new InvalidOperationException(reason);
                        }
                    _productionTools?.Reconcile();
                    if(!candidate.TryComplete(_runtimes,PersistentQueues(),UnityEngine.Time.realtimeSinceStartupAsDouble,
                        saved=>domains.ResolveNavigation(this,saved),out reason))throw new InvalidOperationException(reason);
                    _restoringPresentation=false;EnvironmentReady(ready,failed);
                }
                catch(Exception error) { _failure?.Invoke(error.Message); }
            };
            try
            {
                BeginEnvironment();
                foreach(var saved in candidate.Territory.Presentation)
                {
                    var binding=saved;
                    if(binding.SeedIndex>=0)_objects[binding.SeedIndex].gameObject.SetActive(false);
                    Region.TryGet(new PersistentId(binding.Object),out var obj);
                    var parameters=EntityParams.Create(new Vector3(obj.Position.x,0f,obj.Position.y),new Vector3(0f,obj.Yaw,0f));
                    int entityId=parameters.Id;_pendingEntities.Add(entityId,parameters);
                    if(binding.SeedIndex>=0)_entityIds.Add(entityId);else _machineEntities.Add(entityId,obj.Id);
                    parameters.OnShowCallback=logic=>
                    {
                        _pendingEntities.Remove(entityId);
                        if(version!=_entityVersion || Region==null) {GF.Entity.HideEntitySafe(entityId);return;}
                        try
                        {
                            RegionObjectView view;
                            if(binding.SeedIndex>=0)
                            {
                                var entity=(InitialRegionEntity)logic;entity.BindPersistent(Region,obj.Id);view=entity.View;
                                AttachEnergyFacility(entity);AttachWarehouse(view,true);AttachProduction(view,true);
                            }
                            else { var entity=(InitialRegionMachineEntity)logic;entity.Bind(Region,obj.Id);view=entity.View;AttachEnvironmentReceiver(view); }
                            RegisterPersistentView(view,binding.SeedIndex,binding.Asset);
                            if(--remaining==0)finish();
                        }
                        catch(Exception error) { _failure?.Invoke(error.Message); }
                    };
                    if(binding.SeedIndex>=0)GF.Entity.ShowEntity<InitialRegionEntity>(binding.Asset,_entityGroup,entityId,parameters);
                    else GF.Entity.ShowEntity<InitialRegionMachineEntity>(binding.Asset,_entityGroup,entityId,parameters);
                }
                if(remaining==0)finish();
            }
            catch { Release();throw; }
        }
    }
}
