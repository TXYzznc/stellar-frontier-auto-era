using System;
using System.Collections.Generic;
using AutoEra.Buildings;
using AutoEra.Events;
using AutoEra.Logistics;
using AutoEra.Machines;
using AutoEra.Save;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using GameFramework;
using GameFramework.Event;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    // This fixture substitutes arm contact only; native geometry/navigation are verified separately.
    public sealed class ProductionTransportEditModeTests
    {
        private AutoEraWorldSession _world;
        private InitialRegion _region;
        private MachineInstance _machine;
        private ComponentInstance _arm;
        private MachineExecutionContext _context;
        private EffectorBehaviorQueue<EffectorBehaviorParameters> _queue;
        private RegionTransferEndpoint _source, _destination;
        private ResourceTransferEffectorExecutor _executor;
        private ResourceTransferConfig _config;
        private Contact _contact;
        private readonly List<IRegionEffectorOperation> _operations=new List<IRegionEffectorOperation>();
        private CargoOwnershipAuthority Authority => _world.Resources.Authority;
        private const string Seed="39999";
        [SetUp] public void Setup()
        {
            _world=new AutoEraWorldSessionFactory().Create(7,new Publisher());
            _world.Resources.Configure(new ResourceItemCatalog(new[] { new ResourceItemDefinition(ResourceItemCatalog.Ore,CargoItemClass.CommonResource),new ResourceItemDefinition(Seed,CargoItemClass.Seed) }));
            _region=new InitialRegion(_world,new Rect(-50,-50,100,100));
            var source=_region.Register(PersistentObjectKind.ResourcePoint,"Fixture source",Vector2.zero,Vector2.one,blocksNavigation:false);
            var destination=_region.Register(PersistentObjectKind.Building,"Fixture warehouse",new Vector2(10,0),Vector2.one,blocksNavigation:false);
            Authority.RegisterContainer(new CargoOwner(CargoOwnerKind.WorldFree,source.Id),CargoContainerKind.WorldFree,100);
            Authority.RegisterContainer(new CargoOwner(CargoOwnerKind.Receiver,destination.Id),CargoContainerKind.Warehouse,2);
            _config=ScriptableObject.CreateInstance<ResourceTransferConfig>();
            _source=Endpoint(source,false); _destination=Endpoint(destination,true);
            _machine=_world.Machines.Create(new MachineDefinition(1001,"Fixture",1,0,1,1,4,true,true,100));
            _arm=_world.Machines.CreateComponent(new ComponentDefinition(2201,HardwareKind.Effector,1,0,0,0,true));
            Assert.That(_world.Machines.Install(_machine.Id,ManagementOrigin.Library,_arm.Id,0),Is.EqualTo(MachineManagementResult.Completed));
            Assert.That(_region.DeployMachine(_machine.Id,new Vector2(0,-2.4f),Vector2.one,out _),Is.EqualTo(RegionMachineDeploymentResult.Bound));
            _machine.Activate(ManagementOrigin.Field); _machine.UpdateEnvironment(true,true); _machine.SetRunState(ManagementOrigin.Field,MachineRunState.Running);
            _context=new MachineExecutionContext(_machine,_world.IdAllocator,_world.Events,_world.Resources.GetCargo(_machine));
            _queue=new EffectorBehaviorQueue<EffectorBehaviorParameters>(_world.IdAllocator,_context.Tasks);
            _contact=new Contact(); _executor=new ResourceTransferEffectorExecutor(_region,new Dictionary<PersistentId,RegionTransferEndpoint> { {_source.Id,_source},{_destination.Id,_destination} },_contact);
        }
        private RegionTransferEndpoint Endpoint(RegionObject value,bool warehouse)
        {
            var root=new GameObject(value.Name); var contact=new GameObject("Contact").transform; var dock=new GameObject("Dock").transform;
            contact.SetParent(root.transform); dock.SetParent(root.transform); contact.position=new Vector3(value.Position.x,.15f,value.Position.y); dock.position=new Vector3(value.Position.x,0,value.Position.y-2.4f);
            var endpoint=root.AddComponent<RegionTransferEndpoint>(); endpoint.ConfigureForEditor(_config,contact,dock,warehouse); endpoint.Initialize(_world,_region,value); return endpoint;
        }
        [TearDown] public void Teardown()
        {
            foreach(var operation in _operations) operation.Dispose(); _operations.Clear();
            if (_queue.Current!=null) _queue.Finish(BehaviorOutcome.Cancelled); _queue.InvalidateUnstarted(); _queue.TryDetach();
            _context.Dispose(); UnityEngine.Object.DestroyImmediate(_source.gameObject); UnityEngine.Object.DestroyImmediate(_destination.gameObject); UnityEngine.Object.DestroyImmediate(_config); _region.Dispose(); _world.Dispose();
        }
        private PersistentId Next() { Assert.That(_world.IdAllocator.TryAllocate(out var id),Is.True); return id; }
        private void Mint(string item,int units,CargoOwner? owner=null)
        { Assert.That(Authority.TryMint(owner ?? _source.Owner,item,units,out _,out var reason),Is.True,reason); }
        private BehaviorRequest<EffectorBehaviorParameters> Request(bool unloading,int count,string item=ResourceItemCatalog.Ore)
        {
            Assert.That(_context.Tasks.Submit("transport",WorkPriority.Normal,out var task),Is.EqualTo(QueueAdmission.Accepted)); Assert.That(_context.Tasks.TryStart(task.Id),Is.True);
            var target=unloading?_destination:_source; var parameters=new EffectorBehaviorParameters("Transfer") { TransferMode=unloading?"unload":"load", Enumeration=int.Parse(item) };
            parameters.Numbers["count"]=count; parameters.Objects["source"]=new PersistentObjectReference(_source.Id,PersistentObjectKind.ResourcePoint); parameters.Objects["destination"]=new PersistentObjectReference(_destination.Id,PersistentObjectKind.Building);
            Assert.That(_queue.Submit(task.Id,Next(),Next(),new PersistentObjectReference(target.Id,unloading?PersistentObjectKind.Building:PersistentObjectKind.ResourcePoint),WorkPriority.Normal,InterruptionRule.Immediate,parameters,out var request),Is.EqualTo(QueueAdmission.Accepted)); return request;
        }
        private IRegionEffectorOperation Start(bool unloading,int count,string item=ResourceItemCatalog.Ore,long now=0)
        { Assert.That(_executor.TryStart(_context,_arm,Request(unloading,count,item),now,out var operation,out var reason),Is.True,reason); _operations.Add(operation); return operation; }
        private void End(IRegionEffectorOperation operation,BehaviorOutcome outcome=BehaviorOutcome.Completed)
        { operation.Stop(outcome); _queue.Finish(outcome); }
        private void Dock(RegionTransferEndpoint endpoint) => Assert.That(_region.TryUpdateMachinePose(_machine.Id,new Vector2(endpoint.DockPosition.x,endpoint.DockPosition.z),0),Is.True);
        [Test] public void Preparation_IntegerCommit_CancelPreservesCargoAndResponsibility()
        {
            Mint(ResourceItemCatalog.Ore,3); var operation=Start(false,3); operation.Advance(500); Assert.That(_context.Cargo.Used,Is.Zero);
            operation.Advance(700); Assert.That(_context.Cargo.Used,Is.EqualTo(1));
            Assert.That(_world.Resources.Transport.TryRead(_machine.Id,ResourceItemCatalog.Ore,out var reserved),Is.True);
            Assert.That(reserved.RequestedUnits,Is.EqualTo(3)); Assert.That(reserved.ReservedUnits,Is.EqualTo(2));
            Assert.That(reserved.Phase,Is.EqualTo(TransportPhase.Loading));
            End(operation,BehaviorOutcome.Cancelled);
            operation.Advance(5000); Assert.That(_context.Cargo.Used,Is.EqualTo(1));
            Assert.That(_world.Resources.Transport.TryRead(_machine.Id,ResourceItemCatalog.Ore,out var value),Is.True); Assert.That(value.Pending,Is.EqualTo(1)); Assert.That(value.Destination,Is.EqualTo(_destination.Id)); Assert.That(value.Phase,Is.EqualTo(TransportPhase.Cancelled));
            Assert.That(Authority.TryCapture(out var snapshot),Is.True); Assert.That(snapshot.Transactions[0].RemainingReservedUnits,Is.Zero);
            Assert.That(value.ReservedUnits,Is.Zero);
        }
        private RegionEffectorOperationSnapshot Checkpoint(IRegionEffectorOperation operation,long now)
        {
            Assert.That(((IRegionPersistentEffectorOperation)operation).TryCapturePersistent(now,out var snapshot),Is.True);
            string json=WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(now,_world.IdAllocator.NextId.Value-1,1,"Fixture",new[] {new WorldSnapshotSection("operation",1,snapshot)}));
            Assert.That(WorldSnapshotCodec.TryRead(json,new Dictionary<string,int>{{"operation",1}},out var file,out var reason),Is.True,reason);
            Assert.That(file.TryReadSection<RegionEffectorOperationSnapshot>("operation",out var result,out reason),Is.True,reason);return result;
        }
        // The same restored authorities are borrowed here to isolate operation rebinding. Whole-world restoration has a separate gate.
        [Test] public void OperationCheckpoint_RebindsOriginalReservationWithoutRepeatingIntegerCommit()
        {
            Mint(ResourceItemCatalog.Ore,3);var original=Start(false,3);original.Advance(700);
            Assert.That(_context.Cargo.Used,Is.EqualTo(1));var snapshot=Checkpoint(original,700);
            long revision=Authority.Revision,ledger=_world.Resources.Transport.Revision;
            Assert.That(_executor.TryRestorePersistent(_context,_arm,_queue.Current,snapshot,700,out var restored,out var reason),Is.True,reason);
            _operations.Add(restored);Assert.That(Authority.Revision,Is.EqualTo(revision));Assert.That(_world.Resources.Transport.Revision,Is.EqualTo(ledger));
            restored.Advance(900);Assert.That(_context.Cargo.Used,Is.EqualTo(2));
            Assert.That(Authority.TryCapture(out var inventory),Is.True);Assert.That(inventory.Transactions.Count,Is.EqualTo(1));
            Assert.That(inventory.Transactions[0].Reservation.TransactionId.Value,Is.EqualTo(snapshot.Transfer.Transaction));
            Assert.That(inventory.Transactions[0].LastCompletionSequence,Is.EqualTo(2));
            End(restored,BehaviorOutcome.Cancelled);Assert.That(_context.Cargo.Used,Is.EqualTo(2));
        }
        [Test] public void OperationCheckpoint_PreservesPausedPreparationAndFractionWithoutCatchUp()
        {
            Mint(ResourceItemCatalog.Ore,3);var original=Start(false,3);original.Advance(600);original.SetPaused(true,600);
            var snapshot=Checkpoint(original,600);
            Assert.That(_executor.TryRestorePersistent(_context,_arm,_queue.Current,snapshot,600,out var restored,out var reason),Is.True,reason);
            _operations.Add(restored);restored.Advance(10000);Assert.That(_context.Cargo.Used,Is.Zero);
            restored.SetPaused(false,10000);restored.Advance(10100);Assert.That(_context.Cargo.Used,Is.EqualTo(1));
        }
        [TestCase("sequence")] [TestCase("transaction")] [TestCase("progress")] [TestCase("version")]
        public void InvalidOperationCheckpoint_DoesNotMutateReservationOrTransport(string damage)
        {
            Mint(ResourceItemCatalog.Ore,3);var original=Start(false,3);original.Advance(700);var snapshot=Checkpoint(original,700);
            if(damage=="sequence") snapshot.Transfer.Sequence++;
            if(damage=="transaction") snapshot.Transfer.Transaction=999;
            if(damage=="progress") snapshot.Transfer.Progress=double.NaN;
            if(damage=="version") snapshot.Version=2;
            long revision=Authority.Revision,ledger=_world.Resources.Transport.Revision;
            Assert.That(_executor.TryRestorePersistent(_context,_arm,_queue.Current,snapshot,700,out var restored,out _),Is.False);
            Assert.That(restored,Is.Null);Assert.That(Authority.Revision,Is.EqualTo(revision));Assert.That(_world.Resources.Transport.Revision,Is.EqualTo(ledger));
            original.Advance(900);Assert.That(_context.Cargo.Used,Is.EqualTo(2));
        }
        [Test] public void NavigationFailure_PreservesCargoAndShowsResponsibilityUntilDelivered()
        {
            Mint(ResourceItemCatalog.Ore,2); var operation=Start(false,2); operation.Advance(700);
            Assert.That(_world.Resources.Transport.TryRead(_machine.Id,ResourceItemCatalog.Ore,out var carrying),Is.True);
            End(operation);
            _world.Resources.Transport.ReportNavigationFailure(carrying.Task,false,"导航未到达停靠点，货物等待交付");
            Assert.That(_world.Resources.Transport.TryRead(_machine.Id,ResourceItemCatalog.Ore,out var waiting),Is.True);
            Assert.That(waiting.Phase,Is.EqualTo(TransportPhase.Waiting)); Assert.That(waiting.Pending,Is.EqualTo(1));
            Assert.That(waiting.Source,Is.EqualTo(_source.Id)); Assert.That(waiting.Destination,Is.EqualTo(_destination.Id));
            Assert.That(waiting.Reason,Does.Contain("导航")); Assert.That(_context.Cargo.Used,Is.EqualTo(1));
            Dock(_destination); var unload=Start(true,1); unload.Advance(700); End(unload);
            Assert.That(Authority.Balance(ResourceItemCatalog.Ore),Is.EqualTo(1)); Assert.That(_context.Cargo.Used,Is.Zero);
            _world.Resources.Transport.ReportNavigationFailure(carrying.Task,true,"旧任务取消");
            Assert.That(_world.Resources.Transport.TryRead(_machine.Id,ResourceItemCatalog.Ore,out var delivered),Is.True);
            Assert.That(delivered.Pending,Is.Zero); Assert.That(delivered.Reason,Is.Null);
        }
        [Test] public void MachineDetails_ShowUnresolvedRouteAndUnsubscribeWhenClosed()
        {
            Mint(ResourceItemCatalog.Ore,2); var operation=Start(false,2); operation.Advance(700); End(operation,BehaviorOutcome.Cancelled);
            using (var application=new AutoEra.Application.AutoEraApplicationContext(new AutoEra.World.Time.SystemUtcTimeProvider(),new AutoEraWorldSessionFactory()))
            {
                var model=MachineReadModels.Create(AutoEraUiSession.ForWorld(application,_world));
                Assert.That(model.Select(_machine.Id),Is.True);
                var fields=new Dictionary<string,string>(); foreach (var field in model.Snapshot.Detail) fields[field.Label]=field.Value;
                Assert.That(fields["运输来源"],Is.EqualTo(_source.Id.ToString())); Assert.That(fields["交付目标"],Is.EqualTo(_destination.Id.ToString()));
                Assert.That(fields["已装 / 已交付 / 待交付"],Is.EqualTo("1 / 0 / 1")); Assert.That(fields["最近运输原因"],Does.Contain("取消"));
                int notifications=0; model.Changed+=section=>notifications++;
                Assert.That(_world.Resources.Transport.TryRead(_machine.Id,ResourceItemCatalog.Ore,out var value),Is.True);
                _world.Resources.Transport.ReportNavigationFailure(value.Task,false,"路径阻塞，货物等待交付");
                Assert.That(notifications,Is.EqualTo(1)); model.Dispose();
                _world.Resources.Transport.ReportNavigationFailure(value.Task,true,"取消导航"); Assert.That(notifications,Is.EqualTo(1));
            }
        }
        [Test] public void ResumeMustDeliverCargoFirst_ReceiptAndObserverAreConsistent()
        {
            Mint(ResourceItemCatalog.Ore,3); var load=Start(false,3); load.Advance(900); End(load,BehaviorOutcome.Cancelled);
            Assert.That(_executor.TryStart(_context,_arm,Request(false,1),0,out _,out var reason),Is.False); Assert.That(reason,Is.EqualTo("DeliverExistingCargoFirst")); _queue.Finish(BehaviorOutcome.Failed);
            Dock(_destination); int seen=0;
            Authority.ContainerChanged+=container=> { if (_context.Cargo.Used==1) { Assert.That(_world.Resources.Transport.TryRead(_machine.Id,ResourceItemCatalog.Ore,out var value),Is.True); Assert.That(value.Pending,Is.EqualTo(1)); seen++; } };
            var unload=Start(true,2); unload.Advance(900); Assert.That(unload.Outcome,Is.EqualTo(BehaviorOutcome.Completed)); End(unload);
            Assert.That(seen,Is.GreaterThan(0)); Assert.That(_context.Cargo.Used,Is.Zero); Assert.That(Authority.Balance(ResourceItemCatalog.Ore),Is.EqualTo(2));
            Assert.That(_world.Resources.Transport.TryRead(_machine.Id,ResourceItemCatalog.Ore,out var delivered),Is.True); Assert.That(delivered.Delivered,Is.EqualTo(2)); Assert.That(delivered.Pending,Is.Zero);
            unload.Advance(2000); Assert.That(Authority.Balance(ResourceItemCatalog.Ore),Is.EqualTo(2));
        }
        [Test] public void PausePreservesFraction_ResumeDoesNotCatchUp()
        {
            Mint(ResourceItemCatalog.Ore,3); var operation=Start(false,3); operation.Advance(600); operation.SetPaused(true,600); operation.Advance(10000); Assert.That(_context.Cargo.Used,Is.Zero);
            operation.SetPaused(false,10000); operation.Advance(10100); Assert.That(_context.Cargo.Used,Is.EqualTo(1));
        }
        [Test] public void ArrivalWithoutLegalDock_DoesNotReserveOrLoad()
        {
            Mint(ResourceItemCatalog.Ore,2); Dock(_destination); var operation=Start(false,2); operation.Advance(9000); Assert.That(_context.Cargo.Used,Is.Zero);
            Assert.That(Authority.TryCapture(out var snapshot),Is.True); Assert.That(snapshot.Transactions,Is.Empty);
        }
        [Test] public void UnreachableActualContact_DoesNotReserveOrLoad()
        { Mint(ResourceItemCatalog.Ore,2); _contact.Reachable=false; var operation=Start(false,2); operation.Advance(9000); Assert.That(_context.Cargo.Used,Is.Zero); Assert.That(((IRegionEffectorOperationStatus)operation).WaitingReason,Is.EqualTo("Fixture unreachable")); }
        [Test] public void SourceInsufficient_CompletesOnlyReservedUnits()
        { Mint(ResourceItemCatalog.Ore,2); var operation=Start(false,3); operation.Advance(1100); Assert.That(operation.Outcome,Is.EqualTo(BehaviorOutcome.Partial)); Assert.That(_context.Cargo.Used,Is.EqualTo(2)); }
        [Test] public void CargoCapacityLimitsActualLoad()
        { Mint(ResourceItemCatalog.Ore,6); var operation=Start(false,6); operation.Advance(2000); Assert.That(operation.Outcome,Is.EqualTo(BehaviorOutcome.Partial)); Assert.That(_context.Cargo.Used,Is.EqualTo(4)); }
        [Test] public void DestinationDisappears_PreservesLoadedCargo()
        { Mint(ResourceItemCatalog.Ore,3); var operation=Start(false,3); operation.Advance(700); _region.Remove(_destination.Id); operation.Advance(900); Assert.That(operation.Outcome,Is.EqualTo(BehaviorOutcome.TargetInvalid)); Assert.That(_context.Cargo.Used,Is.EqualTo(1)); }
        [Test] public void RepeatedStartDoesNotReplaceActiveResponsibility()
        { Mint(ResourceItemCatalog.Ore,3); var operation=Start(false,3); Assert.That(_executor.TryStart(_context,_arm,Request(false,3),0,out _,out var reason),Is.False); Assert.That(reason,Is.EqualTo("ActiveTransferExists")); operation.Advance(700); Assert.That(_context.Cargo.Used,Is.EqualTo(1)); }
        [Test] public void FullLocalWarehouse_ResponsibilitySurvivesAndRecovers()
        {
            Mint(Seed,2,_destination.Owner); Mint(Seed,2); var load=Start(false,2,Seed); load.Advance(1000); End(load); Dock(_destination);
            var blocked=Start(true,2,Seed); blocked.Advance(1000); Assert.That(blocked.Outcome,Is.EqualTo(BehaviorOutcome.Partial)); End(blocked,BehaviorOutcome.Partial); Assert.That(_context.Cargo.Used,Is.EqualTo(2));
            var store=Authority.RegisterContainer(new CargoOwner(CargoOwnerKind.Receiver,Next()),CargoContainerKind.Store,2);
            Assert.That(Authority.TryFindAvailableLot(_destination.Owner,Seed,out var lot),Is.True);
            Assert.That(Authority.TryReserve(Next(),Next(),lot.Id,lot.Version,store.Owner,store.Generation,2,out var reservation,out var reason),Is.True,reason); Authority.Commit(reservation,1,2);
            var recovered=Start(true,2,Seed); recovered.Advance(1000); Assert.That(recovered.Outcome,Is.EqualTo(BehaviorOutcome.Completed)); Assert.That(_context.Cargo.Used,Is.Zero);
            Assert.That(Authority.TryReadContainer(_destination.Owner,out var warehouse),Is.True); Assert.That(warehouse.Used,Is.EqualTo(2));
        }
        [Test] public void WarehouseReadModel_DefersCaptureAndUnsubscribes()
        {
            Mint(ResourceItemCatalog.Ore,2); using(var model=new WarehouseReadModel(_world.Resources))
            {
                var before=model.Snapshot; Action<CargoContainer> listener=container=> { Assert.That(model.Refresh(),Is.False); Assert.That(model.Snapshot,Is.SameAs(before)); }; Authority.ContainerChanged+=listener;
                var load=Start(false,2); load.Advance(1000); End(load); Assert.That(model.NeedsRefresh,Is.True); Assert.That(model.Refresh(),Is.True);
                Assert.That(model.Snapshot.Items[0].CargoUnits,Is.EqualTo(2)); Authority.ContainerChanged-=listener; model.Dispose(); Mint(ResourceItemCatalog.Ore,1); Assert.That(model.NeedsRefresh,Is.False);
            }
        }
        private ResourceWorldSnapshot PersistentResources()
        {
            Assert.That(_world.Resources.TryCapturePersistent(out var state),Is.True);
            string json=WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(7,state.Cargo.AllocatedThrough,1,"Resources",new[] {new WorldSnapshotSection("resources",1,state)}));
            Assert.That(WorldSnapshotCodec.TryRead(json,new Dictionary<string,int>{{"resources",1}},out var file,out var reason),Is.True,reason);
            Assert.That(file.TryReadSection<ResourceWorldSnapshot>("resources",out var restored,out reason),Is.True,reason);return restored;
        }
        private AutoEraWorldSession ResourceCandidate(ResourceWorldSnapshot state)
        {
            var world=new AutoEraWorldSessionFactory().CreateRestoreCandidate(7,state.Cargo.AllocatedThrough);
            world.Events.Restore(_world.Events.Capture());
            world.Machines.RestoreConfiguration(_world.Machines.CapturePersistentConfiguration(),(id,level)=>_machine.Definition,(id,level)=>_arm.Definition);
            world.Resources.Configure(_world.Resources.Catalog);return world;
        }
        [Test] public void TransportCheckpoint_RestoresOriginalBindingAndCargo_ContinuesOnlyUncommittedUnits()
        {
            Mint(ResourceItemCatalog.Ore,3);var operation=Start(false,3);operation.Advance(700);var data=PersistentResources();
            Assert.That(Authority.TryCapture(out var inventory),Is.True);var token=inventory.Transactions[0].Reservation;
            using(var candidate=ResourceCandidate(data))
            {
                Assert.That(candidate.Resources.TryRestorePersistent(data,out var reason),Is.True,reason);
                candidate.Machines.TryGet(_machine.Id,out var machine);var cargo=candidate.Resources.GetCargo(machine);
                Assert.That(cargo.Used,Is.EqualTo(1));Assert.That(candidate.Resources.Transport.TryRead(machine.Id,ResourceItemCatalog.Ore,out var ledger),Is.True);
                Assert.That(ledger.Loaded,Is.EqualTo(1));Assert.That(ledger.ReservedUnits,Is.EqualTo(2));Assert.That(ledger.Task,Is.EqualTo(_queue.Current.TaskId));
                long revision=candidate.Resources.Transport.Revision;candidate.Resources.Authority.Commit(token,1,99);
                Assert.That(candidate.Resources.Transport.Revision,Is.EqualTo(revision));Assert.That(cargo.Used,Is.EqualTo(1));
                candidate.Resources.Authority.Commit(token,2,99);candidate.Resources.Transport.TryRead(machine.Id,ResourceItemCatalog.Ore,out ledger);
                Assert.That(cargo.Used,Is.EqualTo(3));Assert.That(machine.UsedCapacity,Is.EqualTo(3));Assert.That(ledger.Loaded,Is.EqualTo(3));Assert.That(ledger.ReservedUnits,Is.Zero);
                Assert.That(_context.Cargo.Used,Is.EqualTo(1));
            }
        }
        [Test] public void LostTransportBinding_RejectsTheUnpublishedCandidateInsteadOfInventingDeliveryResponsibility()
        {
            Mint(ResourceItemCatalog.Ore,3);var operation=Start(false,3);operation.Advance(700);var data=PersistentResources();data.Transport.Bindings=Array.Empty<TransportLedgerSnapshot.Binding>();
            using(var candidate=ResourceCandidate(data))
            {
                Assert.That(candidate.Resources.TryRestorePersistent(data,out var reason),Is.False);Assert.That(reason,Does.Contain("reservation"));
                Assert.That(candidate.Resources.Transport.CapturePersistent().Records,Is.Empty);Assert.That(_context.Cargo.Used,Is.EqualTo(1));
            }
        }
        private sealed class Contact:IResourceTransferContact
        { internal bool Reachable=true; public bool TryReachTransfer(ComponentInstance component,PersistentId endpoint,Vector3 position,double range,out string reason) { reason=Reachable?null:"Fixture unreachable"; return Reachable; } public void SafeStop(ComponentInstance component) { } }
        private sealed class Publisher:IEventPublisher { public void Publish(GameEventArgs args) => ReferencePool.Release(args); }
    }
}
