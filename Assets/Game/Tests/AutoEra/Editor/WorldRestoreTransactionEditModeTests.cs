using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoEra.Algorithms;
using AutoEra.Application;
using AutoEra.Machines;
using AutoEra.Save;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using AutoEra.World.Time;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    // Domain bridge fixture verifies transaction semantics, not production/energy persistence.
    public sealed class WorldRestoreTransactionEditModeTests
    {
        private sealed class Facts { public int Amount; }
        private sealed class Domains : IRegionWorldPersistence
        {
            public IReadOnlyDictionary<string,int> Versions { get; }=new Dictionary<string,int> { {"fixtureFacts",1} };
            internal bool Reject,LastWasDisposed;
            internal AutoEraWorldSession Last;
            public bool TryCapture(AutoEraWorldSession world,out WorldSnapshotSection[] sections,out ulong[] identities,out string reason)
            { sections=new[] {new WorldSnapshotSection("fixtureFacts",1,new Facts {Amount=7})};identities=Array.Empty<ulong>();reason=null;return true; }
            public bool TryReadIdentities(LoadedWorldSnapshot snapshot,out ulong[] identities,out string reason)
            { identities=Array.Empty<ulong>();return snapshot.TryReadSection<Facts>("fixtureFacts",out _,out reason); }
            public bool TryRestore(AutoEraWorldSession world,InitialRegion region,LoadedWorldSnapshot snapshot,out string reason)
            { Last=world;LastWasDisposed=!world.IsActive;reason=Reject ? "FixtureAuthorityRejected" : null;return !Reject; }
            public bool TryBindScene(InitialRegionScene scene,WorldRestoreCandidate candidate,out string reason) {reason="FixtureHasNoScene";return false;}
            public MachineNavigationTarget ResolveNavigation(InitialRegionScene scene,MachineNavigationTargetSnapshot target)=>null;
        }
        private sealed class Source : IWorldSnapshotSource
        {
            public bool TryCapture(long revision,out WorldSnapshotDocument snapshot,out string reason)
            { snapshot=null;reason="FixtureNoBoundary";return false; }
        }
        private sealed class SnapshotSource : IWorldSnapshotSource
        {
            private readonly Fixture _fixture;
            internal SnapshotSource(Fixture fixture) { _fixture=fixture; }
            public bool TryCapture(long revision,out WorldSnapshotDocument snapshot,out string reason)
                =>WorldPersistenceProfile.TryCapture(_fixture.World,_fixture.Region,_fixture.Registry,0,new[] {_fixture.Queue.CapturePersistentState()},_fixture.Bindings,_fixture.Domains,revision,"Fixture",out snapshot,out reason);
        }
        private sealed class CountingWriter : IWorldSnapshotWriter
        {
            internal int Writes;
            public Task<WorldSaveWriteResult> WriteAsync(int slot,WorldSnapshotDocument snapshot,CancellationToken token)
            { Writes++;return Task.FromResult(new WorldSaveWriteResult(true)); }
        }
        private static ulong Allocate(AutoEraWorldSession world)
        { Assert.That(world.IdAllocator.TryAllocate(out var id),Is.True);return id.Value; }
        private sealed class Utc : IUtcTimeProvider { public DateTimeOffset GetUtcNow()=>new DateTimeOffset(2026,10,8,0,0,0,TimeSpan.Zero); }
        private sealed class MovingUtc : IUtcTimeProvider
        { internal DateTimeOffset Now=new DateTimeOffset(2026,10,8,0,0,0,TimeSpan.Zero);public DateTimeOffset GetUtcNow()=>Now; }
        private sealed class Setup : IWorldProgressSetup
        {
            internal string Unavailable;
            internal bool Offline;
            public string NewProgressUnavailableReason=>Unavailable;
            public bool SupportsOfflineContinuation=>Offline;
            public IRegionWorldPersistence CreatePersistence()=>new Domains();
            public MachineDefinition ResolveMachine(int model,int level)=>Fixture.Definition;
            public ComponentDefinition ResolveComponent(int model,int level)=>Fixture.Core;
            public bool TryInitializeNew(AutoEraWorldSession world,out string reason) {reason="FixtureNoFormalInitializer";return false;}
            public void BeginOfflineContinuation(WorldRestoreCandidate candidate,InitialRegionScene scene,WorldSlotEntryRequest request,Action completed,Action<string> failed)
                =>failed("FixtureHasNoOfflineEngine");
        }
        private sealed class SlotFixture : IDisposable
        {
            internal readonly string DirectoryPath=Path.Combine(Path.GetTempPath(),"AutoEra-B47-SlotFlow-"+Guid.NewGuid().ToString("N"));
            internal readonly MovingUtc Utc=new MovingUtc();
            internal readonly AutoEraApplicationContext App;
            internal readonly Setup Setup=new Setup();
            internal SlotFixture()
            { App=new AutoEraApplicationContext(Utc,new AutoEraWorldSessionFactory(),null,new SaveSlotService(DirectoryPath,()=>new DateTime(2026,10,8,0,0,0,DateTimeKind.Utc)));App.Slots.Configure(Setup); }
            internal void Write(Fixture source,long outerTime=20,bool pending=false)
                =>Assert.That(App.SaveSlots.Overwrite(0,"Fixture",outerTime,WorldSnapshotCodec.Serialize(source.Capture()),pending),Is.True);
            public void Dispose()
            {
                App.Dispose();
                string absolute=Path.GetFullPath(DirectoryPath),root=Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar);
                Assert.That(Path.GetDirectoryName(absolute),Is.EqualTo(root));Assert.That(Path.GetFileName(absolute),Does.StartWith("AutoEra-B47-SlotFlow-"));
                if(Directory.Exists(absolute))Directory.Delete(absolute,true);
            }
        }
        private sealed class Fixture : IDisposable
        {
            internal readonly Domains Domains=new Domains();
            internal readonly AutoEraWorldSession World=new AutoEraWorldSessionFactory().Create(20);
            internal readonly InitialRegion Region;
            internal readonly RegionMachineRuntimeRegistry Registry;
            internal readonly MachineInstance Machine;
            internal readonly RegionObject Target;
            internal readonly RegionWorkQueue Queue;
            internal readonly RegionPresentationBinding[] Bindings;
            internal static readonly MachineDefinition Definition=new MachineDefinition(1,"Fixture",1,0,1,0,30,false,false,100);
            internal static readonly ComponentDefinition Core=new ComponentDefinition(2001,HardwareKind.Core,1,0,100,100,false);
            internal Fixture()
            {
                Region=new InitialRegion(World,new Rect(-30,-30,60,60));
                Machine=World.Machines.Create(Definition);var core=World.Machines.CreateComponent(Core);
                Assert.That(World.Machines.Install(Machine.Id,ManagementOrigin.Library,core.Id,0),Is.EqualTo(MachineManagementResult.Completed));
                Assert.That(Region.DeployMachine(Machine.Id,new Vector2(-4,-4),Vector2.one,out _),Is.EqualTo(RegionMachineDeploymentResult.Bound));
                Target=Region.Register(PersistentObjectKind.Building,"原建筑",new Vector2(4,4),Vector2.one);
                Machine.Activate(ManagementOrigin.Field);Machine.SetRunState(ManagementOrigin.Field,MachineRunState.Running);Machine.UpdateEnvironment(true,true);
                Registry=new RegionMachineRuntimeRegistry(World,Region,null,new MachineNavigationSettings(),.32f,1.8f);
                Assert.That(Registry.TryAttach(Machine,null,out _,out var reason),Is.True,reason);
                Queue=new RegionWorkQueue(Region,Target.Id,new Rect(4,4,1,1),"FixtureWork");Queue.Request(Machine.Id,new Vector2(4.5f,4.5f));
                Bindings=new[] {new RegionPresentationBinding {Object=Machine.Id.Value,SeedIndex=-1,ContentVersion=1,Asset="FixtureMachine"},
                    new RegionPresentationBinding {Object=Target.Id.Value,SeedIndex=0,ContentVersion=1,Asset="FixtureBuilding"}};
                World.Clock.TryAdvanceRealtimeSeconds(.00025);
            }
            internal WorldSnapshotDocument Capture()
            {
                Assert.That(WorldPersistenceProfile.TryCapture(World,Region,Registry,0,new[] {Queue.CapturePersistentState()},Bindings,Domains,9,"Fixture",
                    out var data,out var reason),Is.True,reason);return data;
            }
            internal bool Prepare(string json,out WorldRestoreCandidate candidate,out string reason)
                => WorldRestoreTransaction.TryPrepare(json,new AutoEraWorldSessionFactory(),Domains,(id,level)=>Definition,(id,level)=>Core,out candidate,out reason);
            internal static RegionMachineRuntimeRegistry BindRuntime(WorldRestoreCandidate candidate)
            {
                var registry=new RegionMachineRuntimeRegistry(candidate.World,candidate.Region,null,new MachineNavigationSettings(),.32f,1.8f);
                foreach(var machine in candidate.World.Machines.Machines)
                { machine.UpdateEnvironment(true,true);Assert.That(registry.TryAttach(machine,null,out _,out var reason),Is.True,reason); }
                return registry;
            }
            internal static RegionWorkQueue BindQueue(WorldRestoreCandidate candidate)
            { var row=candidate.Territory.Queues[0];return new RegionWorkQueue(candidate.Region,new PersistentId(row.Target),new Rect(row.AreaPosition,row.AreaSize),row.Channel); }
            public void Dispose() { Queue.Dispose();Registry.Dispose();Region.Dispose();World.Dispose(); }
        }

        [Test] public void CandidateRestoresOriginalIdentitiesQueueAndFraction_CommitOnlyAfterRuntime()
        {
            using(var source=new Fixture())
            {
                Assert.That(source.Prepare(WorldSnapshotCodec.Serialize(source.Capture()),out var candidate,out var reason),Is.True,reason);
                using(candidate)
                using(var queue=Fixture.BindQueue(candidate))
                {
                    Assert.That(candidate.IsReady,Is.False);Assert.That(candidate.World.Clock.FractionalMilliseconds,Is.EqualTo(.25));
                    Assert.That(candidate.TryComplete(Fixture.BindRuntime(candidate),new[] {queue},0,null,out reason),Is.True,reason);
                    Assert.That(queue.Owner,Is.EqualTo(source.Machine.Id));Assert.That(candidate.IsReady,Is.True);
                    Assert.That(candidate.World.IdAllocator.NextId,Is.EqualTo(source.World.IdAllocator.NextId));
                    Assert.That(source.World.IsActive,Is.True);Assert.That(source.Queue.Owner,Is.EqualTo(source.Machine.Id));
                }
            }
        }
        [Test] public void MissingAuthoritySectionOrUnsupportedContent_RejectsBeforeConstructingCandidate()
        {
            using(var source=new Fixture())
            {
                var data=source.Capture();var reduced=new List<WorldSnapshotSection>();
                foreach(var row in data.Sections)if(row.Name!="fixtureFacts")reduced.Add(row);
                Assert.That(source.Prepare(WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(20,data.AllocatedThrough,9,"Fixture",reduced)),out _,out _),Is.False);
                Assert.That(source.Domains.Last,Is.Null);Assert.That(source.World.IsActive,Is.True);
                source.Bindings[0].ContentVersion=2;
                Assert.That(WorldPersistenceProfile.TryCapture(source.World,source.Region,source.Registry,0,new[] {source.Queue.CapturePersistentState()},source.Bindings,source.Domains,9,"Fixture",out _,out _),Is.False);
            }
        }
        [Test] public void CrossDomainComponentRegionIdentityCollision_RejectsAndPreservesOriginal()
        {
            using(var source=new Fixture())
            {
                var data=source.Capture();foreach(var section in data.Sections)if(section.Data is WorldRegionSnapshot region)
                    foreach(var row in region.State.Objects)if(row.Id==source.Target.Id.Value)row.Id=source.Machine.GetComponent(HardwareKind.Core,0).Id.Value;
                Assert.That(source.Prepare(WorldSnapshotCodec.Serialize(data),out _,out _),Is.False);
                Assert.That(source.Domains.Last,Is.Null);Assert.That(source.Region.TryGet(source.Target.Id,out _),Is.True);
            }
        }
        [Test] public void DomainFailureDisposesUnpublishedCandidate_CurrentWorldStillActive()
        {
            using(var source=new Fixture())
            {
                string json=WorldSnapshotCodec.Serialize(source.Capture());source.Domains.Reject=true;
                Assert.That(source.Prepare(json,out _,out var reason),Is.False);Assert.That(reason,Is.EqualTo("FixtureAuthorityRejected"));
                Assert.That(source.Domains.Last.IsActive,Is.False);Assert.That(source.World.IsActive,Is.True);
            }
        }
        [Test] public void QueueMismatchRejectsCompletionAndDisposalDoesNotCancelOriginalWork()
        {
            using(var source=new Fixture())
            {
                Assert.That(source.Prepare(WorldSnapshotCodec.Serialize(source.Capture()),out var candidate,out _),Is.True);
                var world=candidate.World;
                Assert.That(candidate.TryComplete(Fixture.BindRuntime(candidate),Array.Empty<RegionWorkQueue>(),0,null,out _),Is.False);
                candidate.Dispose();Assert.That(world.IsActive,Is.False);Assert.That(source.Queue.Owner,Is.EqualTo(source.Machine.Id));
            }
        }
        [Test] public void CapturedBindingsAndQueueWaitersAreDetachedFromCaller()
        {
            using(var source=new Fixture())
            {
                var queue=source.Queue.CapturePersistentState();
                Assert.That(WorldPersistenceProfile.TryCapture(source.World,source.Region,source.Registry,0,new[] {queue},source.Bindings,source.Domains,9,"Fixture",out var saved,out _),Is.True);
                queue.Owner=0;source.Bindings[0].Asset="ChangedLater";
                Assert.That(source.Prepare(WorldSnapshotCodec.Serialize(saved),out var candidate,out _),Is.True);
                using(candidate) { Assert.That(candidate.Territory.Queues[0].Owner,Is.EqualTo(source.Machine.Id.Value));Assert.That(candidate.Territory.Presentation[0].Asset,Is.EqualTo("FixtureMachine")); }
            }
        }
        [Test] public void ApplicationRejectsUnreadyCandidateAndAtomicallyCommitsCompletedCandidate()
        {
            using(var source=new Fixture())
            {
                string directory=Path.Combine(Path.GetTempPath(),"AutoEra-B47-Transaction-"+Guid.NewGuid().ToString("N"));
                try
                {
                    using(var app=new AutoEraApplicationContext(new Utc(),new AutoEraWorldSessionFactory(),null,new SaveSlotService(directory)))
                    {
                        app.TryCreateWorldSession(0,out var old);
                        Assert.That(source.Prepare(WorldSnapshotCodec.Serialize(source.Capture()),out var candidate,out _),Is.True);
                        using(candidate)
                        using(var queue=Fixture.BindQueue(candidate))
                        using(var runtime=Fixture.BindRuntime(candidate))
                        {
                            Assert.That(app.TryCommitRestoredWorld(candidate,1,new Source()),Is.False);Assert.That(app.ActiveWorldSession,Is.SameAs(old));
                            Assert.That(candidate.TryComplete(runtime,new[] {queue},0,null,out var reason),Is.True,reason);
                            Assert.That(app.TryCommitRestoredWorld(candidate,1,new Source()),Is.True);Assert.That(old.IsActive,Is.False);
                            Assert.That(app.ActiveWorldSession,Is.SameAs(candidate.World));Assert.That(app.CurrentSlotIndex,Is.EqualTo(1));
                            Assert.That(app.SaveCoordinator.SavedRevision,Is.EqualTo(9));Assert.That(app.TryCommitRestoredWorld(candidate,1,new Source()),Is.False);
                        }
                    }
                }
                finally { if(Directory.Exists(directory))Directory.Delete(directory,true); }
            }
        }
        [Test] public void SlotFlowNewRequestDoesNotOverwriteOccupiedSlotsOrBypassInitialContentGate()
        {
            using(var source=new Fixture())using(var slots=new SlotFixture())
            {
                slots.Setup.Unavailable="正式开局内容未交付";
                Assert.That(slots.App.Slots.TryRequestNew(1,out var reason),Is.False);Assert.That(reason,Is.EqualTo(slots.Setup.Unavailable));
                slots.Setup.Unavailable=null;slots.Write(source);
                Assert.That(slots.App.Slots.TryRequestNew(0,out _),Is.False);Assert.That(slots.App.Slots.TryRequestNew(1,out reason),Is.True,reason);
                Assert.That(slots.App.ActiveWorldSession,Is.Null);Assert.That(slots.App.SaveSlots.Exists(1),Is.False);
                Assert.That(slots.App.Slots.TryRequestNew(2,out _),Is.False);Assert.That(slots.App.Slots.TryConsume(out var request),Is.True);
                using(request)Assert.That(request.IsNew && request.SlotIndex==1,Is.True);
            }
        }
        [Test] public void SlotFlowBackupRequiresConfirmationInsteadOfSilentContinuation()
        {
            using(var source=new Fixture())using(var slots=new SlotFixture())
            {
                slots.Write(source);slots.Write(source);File.WriteAllText(slots.App.SaveSlots.GetSlotPath(0),"FixtureCorrupt");
                Assert.That(slots.App.SaveSlots.Read(0).IsFromBackup,Is.True);
                Assert.That(slots.App.Slots.TryRequestContinue(0,out var reason),Is.False);Assert.That(reason,Does.Contain("确认"));
                Assert.That(slots.App.Slots.HasPendingRequest,Is.False);Assert.That(slots.App.SaveSlots.RestoreFromBackup(0),Is.True);
                Assert.That(slots.App.Slots.TryRequestContinue(0,out reason),Is.True,reason);
                slots.App.Slots.TryConsume(out var request);using(request)Assert.That(request.Candidate.IsReady,Is.False);
            }
        }
        [Test] public void SlotFlowOuterAndInnerClockMismatchRejectsUnpublishedWorld()
        {
            using(var source=new Fixture())using(var slots=new SlotFixture())
            {
                slots.Write(source,21);
                Assert.That(slots.App.Slots.TryRequestContinue(0,out var reason),Is.False);Assert.That(reason,Does.Contain("时刻"));
                Assert.That(slots.App.Slots.HasPendingRequest,Is.False);Assert.That(slots.App.ActiveWorldSession,Is.Null);
            }
        }
        [Test] public void SlotFlowOfflineGateCannotBeSkipped_TargetUtcIsLockedForRequest()
        {
            using(var source=new Fixture())using(var slots=new SlotFixture())
            {
                slots.Write(source);slots.Utc.Now=slots.Utc.Now.AddHours(1);
                Assert.That(slots.App.Slots.TryRequestContinue(0,out var reason),Is.False);Assert.That(reason,Does.Contain("离线"));
                slots.Setup.Offline=true;var locked=slots.Utc.Now;
                Assert.That(slots.App.Slots.TryRequestContinue(0,out reason),Is.True,reason);slots.Utc.Now=slots.Utc.Now.AddHours(2);
                slots.App.Slots.TryConsume(out var request);
                using(request) { Assert.That(request.RequiresOffline,Is.True);Assert.That(request.TargetUtc,Is.EqualTo(locked));Assert.That(request.SavedUtc,Is.EqualTo(locked.AddHours(-1))); }
            }
        }
        [Test] public void SlotFlowPendingOfflineCheckpointStillRequiresResumeWhenUtcMovesBackwards()
        {
            using(var source=new Fixture())using(var slots=new SlotFixture())
            {
                slots.Write(source,pending:true);slots.Utc.Now=slots.Utc.Now.AddDays(-1);
                Assert.That(slots.App.Slots.TryRequestContinue(0,out _),Is.False);slots.Setup.Offline=true;
                Assert.That(slots.App.Slots.TryRequestContinue(0,out var reason),Is.True,reason);
                slots.App.Slots.TryConsume(out var request);
                using(request) { Assert.That(request.RequiresOffline && request.ResumesOfflineCheckpoint,Is.True);Assert.That(slots.App.ActiveWorldSession,Is.Null); }
            }
        }
        [Test] public void CriticalBindingManagementRequestsSave_RuntimeProjectionDoesNot()
        {
            using(var source=new Fixture())
            {
                var writer=new CountingWriter();
                using(var saving=new WorldSaveCoordinator(0,new SnapshotSource(source),writer))
                using(var binding=new WorldCriticalSaveBinding(source.World.Machines,source.Registry,saving))
                {
                    source.Machine.UpdateEnvironment(false,false);source.Machine.UpdateEnvironment(true,true);
                    Assert.That(saving.HasPendingRequest,Is.False);
                    Assert.That(source.Machine.SetPowerSwitch(ManagementOrigin.Hub,false),Is.EqualTo(MachineManagementResult.InvalidOrigin));
                    Assert.That(saving.DirtyRevision,Is.Zero);
                    source.Machine.SetPowerSwitch(ManagementOrigin.Field,false);
                    Assert.That(saving.HasPendingRequest,Is.True);Assert.That(saving.DirtyRevision,Is.EqualTo(1));
                    saving.Pump(0);saving.Pump(0);Assert.That(writer.Writes,Is.EqualTo(1));Assert.That(saving.SavedRevision,Is.EqualTo(1));
                    binding.Dispose();source.Machine.SetPowerSwitch(ManagementOrigin.Field,true);Assert.That(saving.DirtyRevision,Is.EqualTo(1));
                }
            }
        }
        [Test] public void CriticalBindingSuccessfulHardwareChangeRequestsSave_RejectedChangeDoesNot()
        {
            using(var source=new Fixture())using(var saving=new WorldSaveCoordinator(0,new SnapshotSource(source),new CountingWriter()))
            using(var binding=new WorldCriticalSaveBinding(source.World.Machines,source.Registry,saving))
            {
                source.Machine.SetRunState(ManagementOrigin.Field,MachineRunState.Stopped);
                saving.Pump(0);saving.Pump(0);long prior=saving.DirtyRevision;
                Assert.That(source.World.Machines.Remove(source.Machine.Id,ManagementOrigin.Hub,HardwareKind.Core,0),Is.EqualTo(MachineManagementResult.InvalidOrigin));
                Assert.That(saving.DirtyRevision,Is.EqualTo(prior));
                Assert.That(source.World.Machines.Remove(source.Machine.Id,ManagementOrigin.Field,HardwareKind.Core,0),Is.EqualTo(MachineManagementResult.Completed));
                Assert.That(saving.DirtyRevision,Is.EqualTo(prior+1));Assert.That(saving.HasPendingRequest,Is.True);
            }
        }
        [Test] public void CriticalBindingTracksAlgorithmInitialApplyAndReattachedRuntimeWithoutStartupSaveReplay()
        {
            using(var source=new Fixture())using(var saving=new WorldSaveCoordinator(0,new SnapshotSource(source),new CountingWriter()))
            using(var binding=new WorldCriticalSaveBinding(source.World.Machines,source.Registry,saving))
            {
                source.Registry.TryGet(source.Machine.Id,out var runtime);
                ulong id=Allocate(source.World),node=Allocate(source.World);
                var graph=new AlgorithmDocument {DocumentId=id};graph.Nodes.Add(new AlgorithmNode {Id=node,Kind=AlgorithmNodeKind.Startup});
                Assert.That(runtime.Instances.AddDraft(graph),Is.True);Assert.That(saving.HasPendingRequest,Is.False);
                Assert.That(runtime.TryActivateDraft(id,out var reason),Is.True,reason);Assert.That(saving.HasPendingRequest,Is.True);
                saving.Pump(0);saving.Pump(0);Assert.That(saving.SavedRevision,Is.EqualTo(1));
                Assert.That(source.Registry.Detach(source.Machine.Id),Is.True);
                Assert.That(source.Registry.TryAttach(source.Machine,null,out runtime,out reason),Is.True,reason);
                id=Allocate(source.World);node=Allocate(source.World);
                graph=new AlgorithmDocument {DocumentId=id};graph.Nodes.Add(new AlgorithmNode {Id=node,Kind=AlgorithmNodeKind.Startup});
                Assert.That(runtime.Instances.AddDraft(graph),Is.True);Assert.That(runtime.TryActivateDraft(id,out reason),Is.True,reason);
                Assert.That(saving.DirtyRevision,Is.EqualTo(2));
            }
        }
    }
}
