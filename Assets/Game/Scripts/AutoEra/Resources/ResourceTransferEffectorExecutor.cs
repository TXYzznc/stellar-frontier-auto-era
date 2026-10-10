using System;
using System.Collections.Generic;
using System.Globalization;
using AutoEra.Algorithms;
using AutoEra.Machines;
using AutoEra.Motion;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using UnityEngine;

namespace AutoEra.Logistics
{
    public interface IResourceTransferContact
    {
        bool TryReachTransfer(ComponentInstance component, PersistentId endpoint, Vector3 position, double range, out string reason);
        void SafeStop(ComponentInstance component);
    }
    /// <summary>Preparation and integer safe units use the real work slot, installed arm and B43 authority.</summary>
    public sealed class ResourceTransferEffectorExecutor : IRegionPersistentEffectorExecutor
    {
        private readonly InitialRegion _region;
        private readonly IReadOnlyDictionary<PersistentId, RegionTransferEndpoint> _endpoints;
        private readonly IResourceTransferContact _contact;
        public ResourceTransferEffectorExecutor(InitialRegion region, IReadOnlyDictionary<PersistentId, RegionTransferEndpoint> endpoints, IResourceTransferContact contact)
        { _region = region ?? throw new ArgumentNullException(nameof(region)); _endpoints = endpoints ?? throw new ArgumentNullException(nameof(endpoints)); _contact = contact ?? throw new ArgumentNullException(nameof(contact)); }
        public bool Supports(ComponentDefinition component, AlgorithmEffectorAction action) => component != null && component.Id == 2201 && component.Kind == HardwareKind.Effector && action == AlgorithmEffectorAction.Transfer;
        public bool TryStart(MachineExecutionContext context, ComponentInstance component, BehaviorRequest<EffectorBehaviorParameters> request, long now, out IRegionEffectorOperation operation, out string reason)
            => TryCreate(context,component,request,now,true,out operation,out reason);

        public bool TryRestorePersistent(MachineExecutionContext context, ComponentInstance component, BehaviorRequest<EffectorBehaviorParameters> request,
            RegionEffectorOperationSnapshot snapshot, long now, out IRegionEffectorOperation operation, out string reason)
        {
            operation=null; reason="装卸操作快照无效";
            if(snapshot==null || snapshot.Version!=1 || snapshot.Kind!=RegionEffectorCheckpointKind.ResourceTransfer || snapshot.Transfer==null || snapshot.Production!=null || now<0 ||
                !TryCreate(context,component,request,now,false,out var candidate,out reason)) return false;
            if(!((Operation)candidate).Restore(snapshot.Transfer,now)) { reason="装卸快照与原预留事务不一致"; return false; }
            operation=candidate; reason=null; return true;
        }
        private bool TryCreate(MachineExecutionContext context, ComponentInstance component, BehaviorRequest<EffectorBehaviorParameters> request, long now,
            bool begin, out IRegionEffectorOperation operation, out string reason)
        {
            operation = null; reason = null;
            if(context==null || component==null || request?.Parameters==null || component.OwnerId!=context.Machine.Id)
            { reason="装卸请求或组件无效"; return false; }
            if (!Supports(component.Definition, AlgorithmEffectorAction.Transfer) || !_endpoints.TryGetValue(request.Target.Id, out var endpoint) || !endpoint.IsAvailable)
            { reason = "装卸目标已失效或工具不兼容"; return false; }
            var parameters = request.Parameters; bool unloading = parameters.TransferMode == "unload";
            if (parameters.TransferMode != "load" && !unloading || !parameters.Objects.TryGetValue("source",out var source) || !parameters.Objects.TryGetValue("destination",out var destination) ||
                !_endpoints.TryGetValue(source.Id,out var from) || !_endpoints.TryGetValue(destination.Id,out var to) || !from.IsAvailable || !to.IsAvailable || from.Warehouse || !to.Warehouse ||
                request.Target.Id != (unloading ? destination.Id : source.Id)) { reason = "运输需要显式来源、目的地与装卸方向"; return false; }
            string item = parameters.Enumeration.ToString(CultureInfo.InvariantCulture);
            if (!endpoint.World.Resources.Catalog.TryGet(item,out _) || !parameters.Numbers.TryGetValue("count",out var amount) || double.IsNaN(amount) || double.IsInfinity(amount) || amount <= 0 || amount > int.MaxValue || amount != Math.Floor(amount))
            { reason = "装卸物品或数量无效"; return false; }
            var resources = endpoint.World.Resources; var machineOwner = resources.MachineOwner(context.Machine);
            int existing = context.Cargo.Count(item);
            if(begin)
            { if (!resources.Transport.TryBegin(context.Machine.Id,request.TaskId,source.Id,destination.Id,item,existing,unloading,(int)amount,out reason)) return false; }
            else if(!resources.Transport.MatchesActive(context.Machine.Id,request.TaskId,source.Id,destination.Id,item,(int)amount))
            { reason="原运输责任未恢复"; return false; }
            operation = new Operation(context,component,request,endpoint,from,to,_region,_contact,resources,item,machineOwner,unloading,(int)amount,now); return true;
        }
        private sealed class Operation : IRegionEffectorOperation, IRegionEffectorOperationStatus, IRegionPersistentEffectorOperation
        {
            private readonly MachineExecutionContext _context;
            private readonly ComponentInstance _component;
            private readonly BehaviorRequest<EffectorBehaviorParameters> _request;
            private readonly RegionTransferEndpoint _endpoint;
            private readonly RegionTransferEndpoint _source, _destination;
            private readonly InitialRegion _region;
            private readonly IResourceTransferContact _contact;
            private readonly ResourceWorldService _resources;
            private readonly CargoOwner _machineOwner;
            private readonly RegionWorkQueue _queue;
            private readonly MotionWorkBridge _bridge = MotionWorkBridge.ForWorkReservation();
            private readonly string _item;
            private readonly bool _unloading;
            private readonly int _requested;
            private ResourceReservation _reservation;
            private bool _reserved, _paused, _disposed;
            private int _committed;
            private ulong _sequence;
            private long _last;
            private double _progress, _prepared;
            public bool IsCompleted { get; private set; }
            public BehaviorOutcome Outcome { get; private set; } = BehaviorOutcome.Failed;
            public string WaitingReason { get; private set; }
            internal Operation(MachineExecutionContext context, ComponentInstance component, BehaviorRequest<EffectorBehaviorParameters> request,
                RegionTransferEndpoint endpoint, RegionTransferEndpoint source, RegionTransferEndpoint destination, InitialRegion region, IResourceTransferContact contact, ResourceWorldService resources, string item, CargoOwner machineOwner, bool unloading, int requested, long now)
            { _context=context; _component=component; _request=request; _endpoint=endpoint; _region=region; _contact=contact; _resources=resources; _item=item;
                _machineOwner=machineOwner; _unloading=unloading; _requested=requested; _last=now; _queue=endpoint.Queue; _source=source; _destination=destination; }
            public bool TryCapturePersistent(long now,out RegionEffectorOperationSnapshot snapshot)
            {
                snapshot=null;
                if(_disposed || IsCompleted || now<_last || !_resources.Authority.IsAtCommitBoundary) return false;
                snapshot=new RegionEffectorOperationSnapshot { Kind=RegionEffectorCheckpointKind.ResourceTransfer,
                    Transfer=new ResourceTransferOperationSnapshot { Transaction=_reserved ? _reservation.TransactionId.Value : 0,
                        Sequence=_sequence, LastWorldMilliseconds=_last, Committed=_committed, Progress=_progress, Prepared=_prepared,
                        Reserved=_reserved, Paused=_paused, Completed=IsCompleted, Outcome=Outcome, WaitingReason=WaitingReason } };
                return true;
            }
            internal bool Restore(ResourceTransferOperationSnapshot state,long now)
            {
                if(state==null || state.Completed || state.LastWorldMilliseconds<0 || state.LastWorldMilliseconds>now ||
                    state.Committed<0 || state.Committed>=_requested || !Valid(state.Progress) || !Valid(state.Prepared) ||
                    state.Prepared>_endpoint.Rules.PreparationSeconds/_endpoint.Rules.Speed(_component.Definition.Level)+1e-9 ||
                    !Enum.IsDefined(typeof(BehaviorOutcome),state.Outcome) || !state.Reserved && (state.Transaction!=0 || state.Committed!=0 || state.Sequence!=0)) return false;
                ResourceReservation token=default;
                if(state.Reserved && (!_resources.Authority.TryGetPersistentReservation(new PersistentId(state.Transaction),out token,out var remaining,out var sequence) ||
                    remaining<=0 || remaining>_requested-state.Committed || sequence!=state.Sequence || token.TaskId!=_request.TaskId ||
                    !token.Source.Equals(_unloading ? _machineOwner : _endpoint.Owner) || !token.Destination.Equals(_unloading ? _endpoint.Owner : _machineOwner))) return false;
                _reservation=token; _reserved=state.Reserved; _sequence=state.Sequence; _last=state.LastWorldMilliseconds;
                _committed=state.Committed; _progress=state.Progress; _prepared=state.Prepared; _paused=state.Paused;
                Outcome=state.Outcome; WaitingReason=state.WaitingReason; return true;
            }
            private static bool Valid(double value) => value>=0 && !double.IsNaN(value) && !double.IsInfinity(value);
            public void SetPaused(bool paused, long now)
            { if (now < _last) throw new ArgumentOutOfRangeException(nameof(now)); if (_paused == paused) return; _paused=paused; _last=now; if (paused) { _contact.SafeStop(_component); WaitingReason="装卸暂停：单位贡献已保留"; } }
            public void Advance(long now)
            {
                if (IsCompleted || _disposed) return; if (now < _last) throw new ArgumentOutOfRangeException(nameof(now)); double seconds=(now-_last)/1000d; _last=now; if (_paused) return;
                if (!_endpoint.IsAvailable || !_source.IsAvailable || !_destination.IsAvailable || !_region.TryGet(_context.Machine.Id,out var machine)) { WaitingReason="装卸目标已失效"; Stop(BehaviorOutcome.TargetInvalid); return; }
                if (_context.Machine.IsMoving) { WaitingReason="等待停稳"; return; }
                var slot=_bridge.RequestWork(_queue,_context.Machine.Id,machine.Position,(int)_request.Priority);
                if (slot != WorkRequestResult.Granted) { WaitingReason=slot == WorkRequestResult.Waiting ? "等待装卸点" : "尚未合法停靠"; return; }
                if (!_contact.TryReachTransfer(_component,_endpoint.Id,_endpoint.ContactPosition,_endpoint.Rules.Range(_component.Definition.Level),out var reach)) { WaitingReason=reach; return; }
                if (!_reserved && !TryReserveRemaining()) { Stop(BehaviorOutcome.Partial); return; }
                WaitingReason=null; double speed=_endpoint.Rules.Speed(_component.Definition.Level);
                double preparation=_endpoint.Rules.PreparationSeconds/speed;
                double used=Math.Min(seconds,Math.Max(0,preparation-_prepared)); _prepared+=used; seconds-=used;
                _progress+=seconds*_endpoint.Rules.UnitsPerSecond*speed;
                while (_progress + 1e-9 >= 1 && !IsCompleted)
                {
                    var result=_resources.Authority.Commit(_reservation,++_sequence,1);
                    if (result.State == ResourceTransferState.Waiting) { _sequence--; return; }
                    if (result.ActualUnits == 0) { WaitingReason=result.Reason; Stop(BehaviorOutcome.Partial); return; }
                    if (result.ActualUnits > 0) { _progress=Math.Max(0,_progress-result.ActualUnits); _committed+=result.ActualUnits; }
                    if (result.RemainingReservedUnits == 0)
                    {
                        _reserved=false;
                        if (_committed == _requested) { Stop(BehaviorOutcome.Completed); }
                        else if (!TryReserveRemaining()) { Stop(BehaviorOutcome.Partial); }
                    }
                }
            }
            private bool TryReserveRemaining()
            {
                var source=_unloading ? _machineOwner : _endpoint.Owner; var destination=_unloading ? _endpoint.Owner : _machineOwner;
                if (!_resources.Authority.TryFindAvailableLot(source,_item,out var lot) || !_resources.Authority.TryReadContainer(destination,out var container))
                { WaitingReason="来源暂无可转移物品"; return false; }
                string reason = null;
                if (!_endpoint.World.IdAllocator.TryAllocate(out var transaction) || !_resources.Authority.TryReserve(transaction,_request.TaskId,lot.Id,lot.Version,destination,container.Generation,_requested-_committed,out _reservation,out reason))
                { WaitingReason=reason ?? "无法预留装卸单位"; return false; }
                _sequence=0; _reserved=true; _resources.Transport.Bind(_reservation,_context.Machine.Id,_item,_unloading); return true;
            }
            public void Stop(BehaviorOutcome outcome)
            {
                if (IsCompleted) return;
                if (_reserved) _resources.Authority.Cancel(_reservation,++_sequence);
                if (outcome == BehaviorOutcome.Cancelled && WaitingReason == null) WaitingReason = "装卸已取消，已装货物等待交付";
                _contact.SafeStop(_component); _bridge.ReleaseWork(_queue,_context.Machine.Id);
                _resources.Transport.Stop(_context.Machine.Id,_item,outcome == BehaviorOutcome.Cancelled || outcome == BehaviorOutcome.TargetInvalid,WaitingReason);
                Outcome=outcome; IsCompleted=true;
            }
            public void Dispose() { if (_disposed) return; if (!IsCompleted) Stop(BehaviorOutcome.Cancelled); _disposed=true; _bridge.Dispose(); }
        }
    }
}
