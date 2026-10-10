using System;
using System.Collections.Generic;
using AutoEra.Algorithms;
using AutoEra.Events;
using AutoEra.Machines;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;

namespace AutoEra.Save
{
    public sealed class WorldSessionSnapshot
    {
        public double FractionalMilliseconds;
        public MachineRosterSnapshot Roster;
        public AlgorithmTemplateSnapshot[] Templates;
        public EventServiceSnapshot Events;
    }
    public sealed class RegionPresentationBinding
    {
        public ulong Object;
        public int SeedIndex, ContentVersion;
        public string Asset;
    }
    public sealed class WorldRegionSnapshot
    {
        public InitialRegionSnapshot State;
        public RegionWorkQueueSnapshot[] Queues;
        public RegionPresentationBinding[] Presentation;
    }

    /// <summary>Required quantity/energy domains supply their own versions, identities and restoration. Missing domains are never defaulted.</summary>
    public interface IWorldDomainPersistence
    {
        IReadOnlyDictionary<string,int> Versions { get; }
        bool TryCapture(AutoEraWorldSession world, out WorldSnapshotSection[] sections, out ulong[] identities, out string reason);
        bool TryReadIdentities(LoadedWorldSnapshot snapshot, out ulong[] identities, out string reason);
        bool TryRestore(AutoEraWorldSession world, InitialRegion region, LoadedWorldSnapshot snapshot, out string reason);
    }

    public static class WorldPersistenceProfile
    {
        public const string SessionSection="session", RegionSection="region", ExecutionSection="execution";
        public static IReadOnlyDictionary<string,int> Versions(IWorldDomainPersistence domains)
        {
            if(domains?.Versions==null) throw new ArgumentNullException(nameof(domains));
            var versions=new Dictionary<string,int>(StringComparer.Ordinal) { {SessionSection,1},{RegionSection,1},{ExecutionSection,1} };
            foreach(var pair in domains.Versions)
            {
                if(string.IsNullOrWhiteSpace(pair.Key) || pair.Value<=0 || versions.ContainsKey(pair.Key))
                    throw new ArgumentException("Invalid required world domain.");
                versions.Add(pair.Key,pair.Value);
            }
            return versions;
        }

        /// <summary>Main thread, after every physical authority has committed this world step.</summary>
        public static bool TryCapture(AutoEraWorldSession world, InitialRegion region, RegionMachineRuntimeRegistry runtime,
            double navigationSeconds, RegionWorkQueueSnapshot[] queues, RegionPresentationBinding[] presentation,
            IWorldDomainPersistence domains,long revision,string summary,out WorldSnapshotDocument snapshot,out string reason)
        {
            snapshot=null;reason="世界尚未到达完整快照边界";
            if(world==null || !world.IsActive || region==null || runtime==null || queues==null || presentation==null || domains==null || revision<0 ||
                !runtime.MatchesPersistentWorld(world,region) || !runtime.TryCapturePersistent(world.Clock.WorldMilliseconds,navigationSeconds,out var execution))return false;
            if(!domains.TryCapture(world,out var additional,out var claims,out reason))return false;
            var session=new WorldSessionSnapshot { FractionalMilliseconds=world.Clock.FractionalMilliseconds,
                Roster=world.Machines.CapturePersistentConfiguration(),Templates=world.AlgorithmTemplates.CapturePersistent(),Events=world.Events.Capture() };
            var queueCopies=new RegionWorkQueueSnapshot[queues.Length];
            for(int i=0;i<queues.Length;i++)
            {
                var row=queues[i];if(row?.Waiting==null)return false;
                var waiting=new RegionWorkWaiterSnapshot[row.Waiting.Length];
                for(int j=0;j<waiting.Length;j++)
                { var item=row.Waiting[j];if(item==null)return false;waiting[j]=new RegionWorkWaiterSnapshot { Machine=item.Machine,Priority=item.Priority }; }
                queueCopies[i]=new RegionWorkQueueSnapshot { Target=row.Target,Owner=row.Owner,Channel=row.Channel,
                    AreaPosition=row.AreaPosition,AreaSize=row.AreaSize,Waiting=waiting };
            }
            var bindingCopies=new RegionPresentationBinding[presentation.Length];
            for(int i=0;i<bindingCopies.Length;i++)
            { var row=presentation[i];if(row==null)return false;bindingCopies[i]=new RegionPresentationBinding { Object=row.Object,SeedIndex=row.SeedIndex,ContentVersion=row.ContentVersion,Asset=row.Asset }; }
            var territory=new WorldRegionSnapshot { State=region.CapturePersistentState(),Queues=queueCopies,Presentation=bindingCopies };
            ulong through=world.IdAllocator.IsExhausted ? ulong.MaxValue : world.IdAllocator.NextId.Value-1;
            if(!WorldSnapshotIdentityValidator.Validate(session,territory,execution,through,claims,out reason))return false;
            var sections=new List<WorldSnapshotSection> { new WorldSnapshotSection(SessionSection,1,session),
                new WorldSnapshotSection(RegionSection,1,territory),new WorldSnapshotSection(ExecutionSection,1,execution) };
            if(additional==null)return false;
            sections.AddRange(additional);
            var expected=Versions(domains);
            if(sections.Count!=expected.Count)return false;
            var seen=new HashSet<string>(StringComparer.Ordinal);
            foreach(var section in sections)
                if(section==null || !seen.Add(section.Name) || !expected.TryGetValue(section.Name,out int version) || version!=section.Version)return false;
            snapshot=new WorldSnapshotDocument(world.Clock.WorldMilliseconds,through,revision,summary,sections);reason=null;return true;
        }
    }
}
