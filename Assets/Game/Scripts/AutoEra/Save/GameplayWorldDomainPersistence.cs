using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AutoEra.Energy;
using AutoEra.Logistics;
using AutoEra.Machines;
using AutoEra.ResourcePoints;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;

namespace AutoEra.Save
{
    public sealed class GameplayEnergySnapshot
    {public RegionEnergyPersistenceSnapshot State;public EnergyEventRecorderSnapshot Recorder;}
    public sealed class GameplayProductionSnapshot
    {public ProductionWorldSnapshot State;public ProductionFacilityPublicSnapshot[] PublicViews;}
    /// <summary>Composes delivered domain authorities. Economy and progress are mandatory real modules, never empty placeholders.</summary>
    public sealed class GameplayWorldDomainPersistence : IRegionWorldPersistence
    {
        private sealed class Pending
        {internal RegionEnergyService Energy;internal EnergyEventRecorderSnapshot Recorder;internal ProductionFacilityPublicSnapshot[] Public;}
        public const string ResourcesSection="resources",ProductionSection="production",EnergySection="energy";
        private readonly IWorldDomainPersistence _economy,_progress;
        private readonly ResourceItemCatalog _catalog;
        private readonly Func<AutoEraWorldSession,InitialRegionScene> _scene;
        private readonly Func<InitialRegionScene,MachineNavigationTargetSnapshot,MachineNavigationTarget> _navigation;
        private readonly ConditionalWeakTable<AutoEraWorldSession,Pending> _pending=new ConditionalWeakTable<AutoEraWorldSession,Pending>();
        private readonly IReadOnlyDictionary<string,int> _versions;
        public GameplayWorldDomainPersistence(ResourceItemCatalog catalog,IWorldDomainPersistence economy,IWorldDomainPersistence progress,
            Func<AutoEraWorldSession,InitialRegionScene> scene,Func<InitialRegionScene,MachineNavigationTargetSnapshot,MachineNavigationTarget> navigation)
        {
            _catalog=catalog;_economy=economy;_progress=progress;_scene=scene;_navigation=navigation;
            var versions=new Dictionary<string,int>(StringComparer.Ordinal) {{ResourcesSection,1},{ProductionSection,1},{EnergySection,1}};
            foreach(var module in new[] {economy,progress})if(module?.Versions!=null)foreach(var pair in module.Versions)
            {if(string.IsNullOrWhiteSpace(pair.Key) || pair.Value<1 || !versions.TryAdd(pair.Key,pair.Value))throw new ArgumentException("Conflicting world domain version.");}
            _versions=versions;
        }
        public IReadOnlyDictionary<string,int> Versions=>_versions;
        public string UnavailableReason=>_economy?.Versions==null || _economy.Versions.Count==0 ? "经济进度保存领域尚未交付。" :
            _progress?.Versions==null || _progress.Versions.Count==0 ? "任务、成长与日常补给保存领域尚未交付。" : _catalog==null ? "资源目录尚未配置。" : null;
        public bool TryCapture(AutoEraWorldSession world,out WorldSnapshotSection[] sections,out ulong[] identities,out string reason)
        {
            sections=null;identities=null;reason=UnavailableReason;if(reason!=null || world==null || !world.IsActive)return false;
            var scene=_scene?.Invoke(world);reason="实际区域领域尚未绑定";
            if(scene==null || scene.Session!=world || scene.Region==null || !world.Resources.TryCapturePersistent(out var resources) ||
                !world.Production.TryCapturePersistent(out var production))return false;
            RegionEnergyPersistenceSnapshot energy;
            if(scene.Energy==null) {using(var empty=new RegionEnergyService())if(!empty.TryCapturePersistent(world.Clock.WorldMilliseconds,out energy))return false;}
            else if(!scene.Energy.TryCapturePersistent(world.Clock.WorldMilliseconds,out energy))return false;
            var views=new List<ProductionFacilityPublicSnapshot>();foreach(var view in scene.ProductionFacilities)views.Add(view.CapturePublicPersistent());views.Sort((a,b)=>a.Point.CompareTo(b.Point));
            var all=new List<WorldSnapshotSection> {new WorldSnapshotSection(ResourcesSection,1,resources),
                new WorldSnapshotSection(ProductionSection,1,new GameplayProductionSnapshot {State=production,PublicViews=views.ToArray()}),
                new WorldSnapshotSection(EnergySection,1,new GameplayEnergySnapshot {State=energy,Recorder=scene.CaptureEnergyRecorderPersistent()})};
            var claims=new List<ulong>();if(!ReadOwnIdentities(resources,production,claims))return false;
            foreach(var module in new[] {_economy,_progress})
            {if(!module.TryCapture(world,out var extra,out var own,out reason) || extra==null || own==null)return false;all.AddRange(extra);claims.AddRange(own);}
            sections=all.ToArray();identities=claims.ToArray();reason=null;return true;
        }
        public bool TryReadIdentities(LoadedWorldSnapshot snapshot,out ulong[] identities,out string reason)
        {
            identities=null;reason=UnavailableReason;if(reason!=null)return false;
            if(!Read(snapshot,out var resources,out var production,out _,out reason))return false;
            var claims=new List<ulong>();if(!ReadOwnIdentities(resources,production.State,claims))return false;
            foreach(var module in new[] {_economy,_progress})
            {if(!module.TryReadIdentities(snapshot,out var own,out reason) || own==null)return false;claims.AddRange(own);}
            identities=claims.ToArray();reason=null;return true;
        }
        public bool TryRestore(AutoEraWorldSession world,InitialRegion region,LoadedWorldSnapshot snapshot,out string reason)
        {
            reason=UnavailableReason;if(reason!=null || world==null || region==null || _pending.TryGetValue(world,out _))return false;
            if(!Read(snapshot,out var resources,out var production,out var energy,out reason))return false;
            if(!ValidatePoints(region,production.State) || production.State.CapturedAt!=snapshot.WorldMilliseconds || resources.Cargo.AllocatedThrough!=snapshot.AllocatedThrough)
            {reason="生产点借用身份或领域时刻无效";return false;}
            world.Resources.Configure(_catalog);
            if(!world.Resources.TryRestorePersistent(resources,out reason) || !world.Production.TryRestorePersistent(production.State,out reason) ||
                !RegionEnergyService.TryRestorePersistent(energy.State,snapshot.WorldMilliseconds,world.Machines,out var service,out reason))return false;
            bool completed=false;
            try
            {
                foreach(var g in service.Grid.Generators)if(!region.TryGet(g.Id,out _)) {reason="原发电设施不在区域中";return false;}
                foreach(var s in service.Grid.Storages)if(!region.TryGet(s.Id,out _)) {reason="原储能设施不在区域中";return false;}
                foreach(var module in new[] {_economy,_progress})if(!module.TryRestore(world,region,snapshot,out reason))return false;
                _pending.Add(world,new Pending {Energy=service,Recorder=energy.Recorder,Public=production.PublicViews});completed=true;reason=null;return true;
            }
            finally {if(!completed)service.Dispose();}
        }
        public bool TryBindScene(InitialRegionScene scene,WorldRestoreCandidate candidate,out string reason)
        {
            reason="原领域与候选场景未匹配";
            if(scene==null || candidate==null || scene.Session!=candidate.World || scene.Region!=candidate.Region || !_pending.TryGetValue(candidate.World,out var pending))return false;
            var views=new Dictionary<ulong,RegionProductionFacility>();foreach(var view in scene.ProductionFacilities)if(!views.TryAdd(view.Target.Id.Value,view))return false;
            if(views.Count!=pending.Public.Length)return false;
            foreach(var state in pending.Public)if(state==null || !views.TryGetValue(state.Point,out var view) || !view.RestorePublicPersistent(state))return false;
            if(!scene.TryAdoptPersistentEnergy(pending.Energy,pending.Recorder,out reason))return false;
            foreach(var module in new[] {_economy,_progress})if(module is IRegionWorldPersistence regionModule && !regionModule.TryBindScene(scene,candidate,out reason))return false;
            _pending.Remove(candidate.World);reason=null;return true;
        }
        public MachineNavigationTarget ResolveNavigation(InitialRegionScene scene,MachineNavigationTargetSnapshot target)=>_navigation?.Invoke(scene,target);
        private static bool Read(LoadedWorldSnapshot snapshot,out ResourceWorldSnapshot resources,out GameplayProductionSnapshot production,out GameplayEnergySnapshot energy,out string reason)
        {
            resources=null;production=null;energy=null;reason="领域段缺失";
            return snapshot!=null && snapshot.TryReadSection(ResourcesSection,out resources,out reason) && snapshot.TryReadSection(ProductionSection,out production,out reason) &&
                snapshot.TryReadSection(EnergySection,out energy,out reason) && resources?.Cargo!=null && production?.State!=null && production.PublicViews!=null && energy?.State!=null;
        }
        private static bool ReadOwnIdentities(ResourceWorldSnapshot resources,ProductionWorldSnapshot production,List<ulong> identities)
        {
            if(resources?.Cargo?.Lots==null || resources.Cargo.Transactions==null || production?.Forests==null)return false;
            foreach(var lot in resources.Cargo.Lots) {if(lot==null)return false;identities.Add(lot.Id);}
            foreach(var tx in resources.Cargo.Transactions) {if(tx==null)return false;identities.Add(tx.Id);}
            foreach(var forest in production.Forests)
            {if(forest?.Trees==null)return false;foreach(var tree in forest.Trees) {if(tree==null)return false;identities.Add(tree.Id);} }
            return true;
        }
        private static bool ValidatePoints(InitialRegion region,ProductionWorldSnapshot saved)
        {
            if(saved?.Forests==null || saved.Minerals==null)return false;
            foreach(var f in saved.Forests)if(f==null || !region.TryGet(new PersistentId(f.Point),out var obj) || obj.Kind!=PersistentObjectKind.ResourcePoint)return false;
            foreach(var m in saved.Minerals)if(m==null || (!region.TryGet(new PersistentId(m.Point),out var obj) ? !m.Removed : obj.Kind!=PersistentObjectKind.ResourcePoint))return false;
            return true;
        }
    }
}
