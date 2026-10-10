using System;
using System.Collections.Generic;
using AutoEra.Machines;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using UnityEngine;

namespace AutoEra.Save
{
    /// <summary>Owns an unpublished candidate. Scene presentation binds explicit saved IDs before completing execution restoration.</summary>
    public sealed class WorldRestoreCandidate : IDisposable
    {
        private RegionMachineRuntimeRegistry _runtime;
        private readonly HardwareOperationSnapshot[] _pendingHardware;
        private bool _disposed,_committed;
        internal WorldRestoreCandidate(AutoEraWorldSession world,InitialRegion region,LoadedWorldSnapshot document,
            WorldRegionSnapshot territory,RegionExecutionSnapshot execution,HardwareOperationSnapshot[] pendingHardware)
        { World=world;Region=region;Document=document;Territory=territory;Execution=execution;_pendingHardware=pendingHardware; }
        public AutoEraWorldSession World { get; }
        public InitialRegion Region { get; }
        public LoadedWorldSnapshot Document { get; }
        public WorldRegionSnapshot Territory { get; }
        public RegionExecutionSnapshot Execution { get; }
        public bool IsReady { get; private set; }

        public bool TryComplete(RegionMachineRuntimeRegistry runtime,IEnumerable<RegionWorkQueue> queues,double navigationSeconds,
            Func<MachineNavigationTargetSnapshot,MachineNavigationTarget> resolveNavigation,out string reason)
        {
            reason="候选运行服务尚未就绪";
            if(_disposed || _committed || IsReady || _runtime!=null || runtime==null || queues==null ||
                !runtime.MatchesPersistentWorld(World,Region))return false;
            // This runtime becomes candidate-owned before any partial restoration can fail.
            _runtime=runtime;
            try
            {
                var channels=new Dictionary<string,RegionWorkQueue>(StringComparer.Ordinal);
                foreach(var queue in queues)
                {
                    if(queue==null)throw new ArgumentException("Missing candidate work channel.");
                    var state=queue.CapturePersistentState();
                    if(state.Owner!=0 || state.Waiting.Length!=0 || !channels.TryAdd(state.Target+":"+state.Channel,queue))
                        throw new ArgumentException("Candidate work channels are not fresh or unique.");
                }
                if(channels.Count!=Territory.Queues.Length)throw new ArgumentException("Candidate work channel count changed.");
                foreach(var saved in Territory.Queues)
                {
                    if(!channels.TryGetValue(saved.Target+":"+saved.Channel,out var queue))throw new ArgumentException("Saved work channel is missing.");
                    queue.RestorePersistentState(saved);
                }
                if(!runtime.RestorePersistent(Execution,Document.WorldMilliseconds,navigationSeconds,resolveNavigation,out reason))return false;
                World.Machines.RestorePersistentHardwareIntents(_pendingHardware);
                IsReady=true;reason=null;return true;
            }
            catch(Exception error) when(error is ArgumentException || error is InvalidOperationException || error is OverflowException)
            { reason=error.Message;return false; }
        }

        internal bool TryCommit()
        {
            if(_disposed || _committed || !IsReady || !World.IsActive)return false;
            _committed=true;return true;
        }
        public void Dispose()
        {
            if(_disposed)return;
            _disposed=true;
            if(_committed)return; // Ownership has moved to the active scene/application.
            World.Machines.DiscardPendingHardwareIntents(); // Teardown callbacks must never apply an unpublished player's intent.
            try { _runtime?.Dispose(); }
            finally { try { Region.Dispose(); } finally { World.Dispose(); } }
        }
    }

    public static class WorldRestoreTransaction
    {
        public static bool TryPrepare(string json,AutoEraWorldSessionFactory factory,IWorldDomainPersistence domains,
            Func<int,int,MachineDefinition> machineDefinition,Func<int,int,ComponentDefinition> componentDefinition,
            out WorldRestoreCandidate candidate,out string reason)
        {
            candidate=null;reason="世界恢复配置不可用";
            if(factory==null || domains==null || machineDefinition==null || componentDefinition==null)return false;
            AutoEraWorldSession world=null;InitialRegion region=null;
            try
            {
                if(!WorldSnapshotCodec.TryRead(json,WorldPersistenceProfile.Versions(domains),out var loaded,out reason) ||
                    !loaded.TryReadSection<WorldSessionSnapshot>(WorldPersistenceProfile.SessionSection,out var state,out reason) ||
                    !loaded.TryReadSection<WorldRegionSnapshot>(WorldPersistenceProfile.RegionSection,out var territory,out reason) ||
                    !loaded.TryReadSection<RegionExecutionSnapshot>(WorldPersistenceProfile.ExecutionSection,out var execution,out reason) ||
                    !domains.TryReadIdentities(loaded,out var identities,out reason) ||
                    !WorldSnapshotIdentityValidator.Validate(state,territory,execution,loaded.AllocatedThrough,identities,out reason))return false;
                world=factory.CreateRestoreCandidate(loaded.WorldMilliseconds,loaded.AllocatedThrough);
                world.Clock.RestorePersistentFraction(state.FractionalMilliseconds);
                world.Events.Restore(state.Events);
                world.Machines.RestoreConfiguration(state.Roster,machineDefinition,componentDefinition);
                if(!world.AlgorithmTemplates.RestorePersistent(state.Templates))throw new ArgumentException("Invalid world templates.");
                region=new InitialRegion(world,new Rect(territory.State.BoundsPosition,territory.State.BoundsSize));
                region.RestorePersistentState(territory.State);
                if(!domains.TryRestore(world,region,loaded,out reason))return false;
                candidate=new WorldRestoreCandidate(world,region,loaded,territory,execution,state.Roster.PendingHardware);reason=null;return true;
            }
            catch(Exception error) when(error is ArgumentException || error is InvalidOperationException || error is OverflowException)
            { reason="候选世界验证失败："+error.Message;return false; }
            finally
            {
                if(candidate==null) { try { region?.Dispose(); } finally { world?.Dispose(); } }
            }
        }
    }
}
