using System;
using System.Collections.Generic;
using AutoEra.World;
using AutoEra.World.Time;

namespace AutoEra.Save
{
    /// <summary>Explicit whitelist. An absent provider cannot be replaced by a guessed yield or an empty domain.</summary>
    public interface IOfflineWorldDomainProvider
    {
        string Domain {get;}
        int Version {get;}
        WorldEventPhase Phase {get;}
        bool IsAtCommitBoundary {get;}
        bool IsCurrent(OfflineScheduledEvent item);
        /// <summary>Append only new plans. Already queued plans retain their original identity in the scheduler checkpoint.</summary>
        void CollectNextEvents(long worldMilliseconds,List<OfflineEventSpec> into);
        bool TryAdvanceContinuous(long from,long to,out string reason);
        bool TryApply(OfflineScheduledEvent item,out string reason);
    }
    public sealed class OfflineProviderManifest
    {
        public string[] RequiredDomains;
        public bool TryValidate(IEnumerable<IOfflineWorldDomainProvider> providers,out string reason)
        {
            reason="离线领域能力清单不完整";if(RequiredDomains==null || RequiredDomains.Length==0 || providers==null)return false;
            var required=new HashSet<string>(StringComparer.Ordinal);foreach(string name in RequiredDomains)
                if(string.IsNullOrWhiteSpace(name) || !required.Add(name))return false;
            var actual=new HashSet<string>(StringComparer.Ordinal);foreach(var provider in providers)
                if(provider==null || string.IsNullOrWhiteSpace(provider.Domain) || provider.Version<1 || !Enum.IsDefined(typeof(WorldEventPhase),provider.Phase) || !actual.Add(provider.Domain))return false;
            foreach(string name in required)if(!actual.Contains(name)) {reason="缺少真实离线提供者："+name;return false;}
            reason=null;return true;
        }
        /// <summary>Player continuation requires the complete approved first-version whitelist.</summary>
        public static OfflineProviderManifest FirstVersion()=>new OfflineProviderManifest {RequiredDomains=new[] {
            "energy","production","agriculture","water","sensors","algorithms","tasks","navigation","transport","construction","manufacturing","progress"}};
    }
    public sealed class OfflineWorldDomainExecutor : IOfflineAlgorithmProtection
    {
        private readonly AutoEraWorldSession _world;
        private readonly Dictionary<string,IOfflineWorldDomainProvider> _domains=new Dictionary<string,IOfflineWorldDomainProvider>(StringComparer.Ordinal);
        private readonly List<IOfflineWorldDomainProvider> _ordered=new List<IOfflineWorldDomainProvider>();
        private readonly List<OfflineEventSpec> _planned=new List<OfflineEventSpec>();
        private readonly IOfflineAlgorithmProtection _algorithmProtection;
        private readonly OfflineSettlementReport _report;
        private bool _failed;
        private OfflineScheduledEvent _observedAlgorithmEvent;
        public OfflineWorldDomainExecutor(AutoEraWorldSession world,IEnumerable<IOfflineWorldDomainProvider> providers,OfflineProviderManifest manifest,
            OfflineSettlementReport report,IOfflineAlgorithmProtection algorithmProtection=null)
        {
            _world=world ?? throw new ArgumentNullException(nameof(world));_report=report ?? throw new ArgumentNullException(nameof(report));_algorithmProtection=algorithmProtection;
            foreach(var provider in providers ?? throw new ArgumentNullException(nameof(providers)))
            {if(provider==null || !_domains.TryAdd(provider.Domain,provider))throw new ArgumentException("Duplicate offline provider.");_ordered.Add(provider);}
            if(manifest==null)throw new ArgumentNullException(nameof(manifest));
            if(!manifest.TryValidate(_ordered,out var reason))throw new ArgumentException(reason ?? "Missing provider manifest.");
            if(!_report.ResourcesInitialized || _report.CurrentWorld!=world.Clock.WorldMilliseconds)throw new ArgumentException("Report baseline and world checkpoint differ.");
            if(_domains.ContainsKey("algorithms") && algorithmProtection==null)throw new ArgumentException("Real algorithm protection is required.");
            _ordered.Sort((a,b)=> {int phase=a.Phase.CompareTo(b.Phase);return phase!=0 ? phase : StringComparer.Ordinal.Compare(a.Domain,b.Domain);});
        }
        public bool IsAtCommitBoundary
        {get {if(_failed || !_world.IsActive || !_world.Resources.Authority.IsAtCommitBoundary)return false;foreach(var d in _ordered)if(!d.IsAtCommitBoundary)return false;return true;} }
        public bool IsCurrent(OfflineScheduledEvent item)
        {
            try
            {
                if(_failed || !_world.IsActive || item==null || !_domains.TryGetValue(item.Domain,out var domain) || domain.Phase!=item.Phase)
                    throw new InvalidOperationException("原离线事件的领域提供者缺失或不一致");
                return domain.IsCurrent(item);
            }
            catch { _failed=true;throw; }
        }
        public void Seed(OfflineEventScheduler scheduler)
        {
            if(scheduler==null || !IsAtCommitBoundary || scheduler.WorldMilliseconds!=_world.Clock.WorldMilliseconds || scheduler.RunId!=_report.RunId ||
                scheduler.TargetWorldMilliseconds!=_report.TargetWorld)throw new ArgumentException("Offline world and report identities differ.");
            try { _planned.Clear();foreach(var domain in _ordered)CollectPlans(domain);scheduler.Seed(_planned); }
            catch { _failed=true;throw; }
        }
        private void CollectPlans(IOfflineWorldDomainProvider domain)
        {
            int first=_planned.Count;domain.CollectNextEvents(_world.Clock.WorldMilliseconds,_planned);
            for(int i=first;i<_planned.Count;i++)
                if(_planned[i]==null || _planned[i].Domain!=domain.Domain || _planned[i].Phase!=domain.Phase || _planned[i].Due<_world.Clock.WorldMilliseconds)
                { _failed=true;throw new ArgumentException("Provider emitted an event outside its domain contract."); }
        }
        public bool TryAdvanceTo(long time,out string reason)
        {
            reason="离线区间或世界无效";long from=_world.Clock.WorldMilliseconds;
            if(!_world.IsActive || time<from || !IsAtCommitBoundary || !_world.Clock.TryAdvanceTo(time))return false;
            try { foreach(var domain in _ordered)if(!domain.TryAdvanceContinuous(from,time,out reason)) {_failed=true;return false;} }
            catch { _failed=true;throw; }
            reason=null;return true;
        }
        public bool TryExecute(OfflineScheduledEvent item,OfflineEventScheduler scheduler,out string reason)
        {
            try
            {
            reason="原离线事件提供者缺失";if(_failed || item==null || item.Due!=_world.Clock.WorldMilliseconds || !_domains.TryGetValue(item.Domain,out var domain) || domain.Phase!=item.Phase || !domain.TryApply(item,out reason)) {_failed=true;return false;}
            if(!_report.TryRecordEvent(item,out reason)) {_failed=true;return false;}
            _planned.Clear();CollectPlans(domain);
            foreach(var next in _planned)scheduler.Schedule(next);reason=null;return true;
            }
            catch { _failed=true;throw; }
        }
        public bool TryObserveAlgorithm(OfflineScheduledEvent item,out OfflineAlgorithmObservation observation)
        {
            observation=null;_observedAlgorithmEvent=null;
            try
            {
                if(_algorithmProtection==null || !_algorithmProtection.TryObserveAlgorithm(item,out observation) || observation==null)
                    throw new InvalidOperationException("真实算法进展观测合同尚未接入");
                _observedAlgorithmEvent=item;return true;
            }
            catch { _failed=true;throw; }
        }
        public bool TryStopNoProgressAlgorithm(AutoEra.World.Identity.PersistentId algorithm,out string reason)
        {
            reason="真实算法停止/报警合同缺失";
            try
            {
                if(_observedAlgorithmEvent==null || _observedAlgorithmEvent.Subject!=algorithm.Value || _algorithmProtection==null ||
                    !_algorithmProtection.TryStopNoProgressAlgorithm(algorithm,out reason)) {_failed=true;return false;}
                bool recorded=_report.TryRecordEvent(_observedAlgorithmEvent,out reason);_observedAlgorithmEvent=null;
                if(!recorded)_failed=true;return recorded;
            }
            catch { _failed=true;throw; }
        }
    }
}
