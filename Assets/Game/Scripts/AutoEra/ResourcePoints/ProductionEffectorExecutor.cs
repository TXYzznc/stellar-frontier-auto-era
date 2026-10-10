using System;
using System.Collections.Generic;
using AutoEra.Algorithms;
using AutoEra.Machines;
using AutoEra.Motion;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using UnityEngine;

namespace AutoEra.ResourcePoints
{
    public interface IProductionWorkContact
    {
        bool TryContact(ComponentInstance component, PersistentId resourcePoint, Rect workArea, PersistentId treeTarget, Vector3 target, double height, out string reason);
        void SetWorking(ComponentInstance component, bool working, double elapsedSeconds);
        void SafeStop(ComponentInstance component);
    }
    /// <summary>Real action authority, sharing existing queues, work ownership, energy and B43 cargo settlement.</summary>
    public sealed class ProductionEffectorExecutor : IRegionPersistentEffectorExecutor
    {
        private readonly InitialRegion _region;
        private readonly IReadOnlyDictionary<PersistentId, RegionProductionFacility> _facilities;
        private readonly IProductionWorkContact _contact;
        public ProductionEffectorExecutor(InitialRegion region, IReadOnlyDictionary<PersistentId, RegionProductionFacility> facilities, IProductionWorkContact contact)
        { _region = region ?? throw new ArgumentNullException(nameof(region)); _facilities = facilities ?? throw new ArgumentNullException(nameof(facilities)); _contact = contact ?? throw new ArgumentNullException(nameof(contact)); }
        public bool Supports(ComponentDefinition component, AlgorithmEffectorAction action)
            => component != null && component.Kind == HardwareKind.Effector && component.HasBehavior &&
                (action == AlgorithmEffectorAction.Cut && component.Id == 2203 || action == AlgorithmEffectorAction.Drill && component.Id == 2204);
        public bool TryStart(MachineExecutionContext context, ComponentInstance component, BehaviorRequest<EffectorBehaviorParameters> request, long now,
            out IRegionEffectorOperation operation, out string reason)
            => TryCreate(context,component,request,now,true,out operation,out reason);

        public bool TryRestorePersistent(MachineExecutionContext context, ComponentInstance component, BehaviorRequest<EffectorBehaviorParameters> request,
            RegionEffectorOperationSnapshot snapshot, long now, out IRegionEffectorOperation operation,out string reason)
        {
            operation=null; reason="生产操作快照无效";
            if(snapshot==null || snapshot.Version!=1 || snapshot.Kind!=RegionEffectorCheckpointKind.ResourceProduction || snapshot.Production==null || snapshot.Transfer!=null || now<0 ||
                !TryCreate(context,component,request,now,false,out var candidate,out reason)) return false;
            if(!((Operation)candidate).Restore(snapshot.Production,now)) { reason="生产快照与原行为或生产责任不一致"; return false; }
            operation=candidate; reason=null; return true;
        }
        private bool TryCreate(MachineExecutionContext context, ComponentInstance component, BehaviorRequest<EffectorBehaviorParameters> request,long now,
            bool begin,out IRegionEffectorOperation operation,out string reason)
        {
            operation = null; reason = null;
            if(context==null || component==null || request?.Parameters==null || component.OwnerId!=context.Machine.Id)
            { reason="生产请求或组件无效"; return false; }
            if (!_facilities.TryGetValue(request.Target.Id, out var facility) || !facility.IsAvailable) { reason = "生产目标已失效"; return false; }
            bool cut = request.Parameters.Action == AlgorithmEffectorAction.Cut.ToString();
            if (!Supports(component.Definition, cut ? AlgorithmEffectorAction.Cut : AlgorithmEffectorAction.Drill) || cut != (facility.Forest != null))
            { reason = "工具与生产目标不兼容"; return false; }
            PersistentId tree = default;
            if (cut && (!request.Parameters.Objects.TryGetValue("tree", out var reference) || reference.ExpectedKind != PersistentObjectKind.Tree || !facility.Forest.TryRead(reference.Id, out _)))
            { reason = "切割需要本人工林中有效的稳定树引用"; return false; }
            if (cut) tree = request.Parameters.Objects["tree"].Id;
            double ratio = Number(request.Parameters, "ratio", .25), power = Number(request.Parameters, "power", 1), count = Number(request.Parameters, "count", 1);
            if (!ProductionRules.Finite(ratio) || ratio < 0 || ratio > .25 || !ProductionRules.Finite(power) || power < 0 || power > 1 ||
                !cut && (!ProductionRules.Finite(count) || count <= 0 || count > int.MaxValue || count != Math.Floor(count)))
            { reason = "生产参数无效"; return false; }
            var view = facility.GetComponent<RegionObjectView>(); var queue = view != null ? view.GetWorkChannel(0) : null;
            if (queue == null || !queue.BelongsTo(_region, request.Target.Id)) { reason = "目标作业通道未就绪"; return false; }
            if (!cut && begin) facility.Production.Track(request.Id, context, component, request);
            if (!cut && !begin && !facility.Production.MatchesResponsibility(request.Id,context.Machine.Id,component.Id,request.TaskId,request.Id))
            { reason="原钻探生产责任未恢复"; return false; }
            operation = new Operation(context, component, request, facility, _region, queue, _contact, tree, ratio, cut ? 1 : (int)count, now); return true;
        }
        private static double Number(EffectorBehaviorParameters parameters, string key, double fallback) => parameters.Numbers.TryGetValue(key, out var value) ? value : fallback;
        private sealed class Operation : IRegionEffectorOperation, IRegionEffectorOperationStatus, IRegionPersistentEffectorOperation
        {
            private readonly MachineExecutionContext _context; private readonly ComponentInstance _component;
            private readonly ResourceProductionWorldService _production;
            private readonly BehaviorRequest<EffectorBehaviorParameters> _request; private readonly RegionProductionFacility _facility;
            private readonly InitialRegion _region; private readonly RegionWorkQueue _queue; private readonly IProductionWorkContact _contact;
            private readonly MotionWorkBridge _bridge = MotionWorkBridge.ForWorkReservation();
            private readonly PersistentId _tree; private readonly double _ratio; private readonly int _requested;
            private long _last; private ulong _sequence; private bool _paused, _disposed, _fell; private int _produced;
            private double _elapsed;
            public string WaitingReason { get; private set; }
            public bool IsCompleted { get; private set; }
            public BehaviorOutcome Outcome { get; private set; } = BehaviorOutcome.Failed;
            internal Operation(MachineExecutionContext context, ComponentInstance component, BehaviorRequest<EffectorBehaviorParameters> request,
                RegionProductionFacility facility, InitialRegion region, RegionWorkQueue queue, IProductionWorkContact contact, PersistentId tree, double ratio, int count, long now)
            { _context = context; _component = component; _request = request; _facility = facility; _region = region; _queue = queue; _contact = contact;
                _production = facility.Production; _tree = tree; _ratio = ratio; _requested = count; _last = now; }
            public bool TryCapturePersistent(long now,out RegionEffectorOperationSnapshot snapshot)
            {
                snapshot=null;
                if(_disposed || IsCompleted || now<_last) return false;
                snapshot=new RegionEffectorOperationSnapshot { Kind=RegionEffectorCheckpointKind.ResourceProduction,
                    Production=new ProductionEffectorOperationSnapshot { Tree=_tree.Value,Sequence=_sequence,LastWorldMilliseconds=_last,
                        Produced=_produced,Elapsed=_elapsed,Fell=_fell,Paused=_paused,Completed=IsCompleted,Outcome=Outcome,WaitingReason=WaitingReason } };
                return true;
            }
            internal bool Restore(ProductionEffectorOperationSnapshot state,long now)
            {
                if(state==null || state.Completed || state.Tree!=_tree.Value || state.LastWorldMilliseconds<0 || state.LastWorldMilliseconds>now ||
                    state.Produced<0 || state.Produced>=_requested || !ProductionRules.Finite(state.Elapsed) || state.Elapsed<0 ||
                    !Enum.IsDefined(typeof(BehaviorOutcome),state.Outcome) || _tree.IsValid && (state.Sequence!=0 || state.Produced!=0) || !_tree.IsValid && state.Fell) return false;
                if(_tree.IsValid)
                {
                    if(!_facility.Forest.TryRead(_tree,out var tree) || state.Fell && tree.Stage!=TreeStage.Falling && tree.Stage!=TreeStage.Stump ||
                        !state.Fell && tree.Stage!=TreeStage.Growing && tree.Stage!=TreeStage.Mature ||
                        state.Fell && tree.Stage==TreeStage.Falling && !_production.MatchesResponsibility(_tree,_context.Machine.Id,_component.Id,_request.TaskId,_request.Id)) return false;
                }
                else if(!_facility.Mineral.MatchesPersistentDrillSequence(_request.Id,state.Sequence) ||
                    !_facility.Production.TryGetPersistentProducedUnits(_request.Id,out var produced) || produced!=state.Produced) return false;
                _last=state.LastWorldMilliseconds; _sequence=state.Sequence; _produced=state.Produced; _elapsed=state.Elapsed; _fell=state.Fell; _paused=state.Paused;
                Outcome=state.Outcome; WaitingReason=state.WaitingReason; return true;
            }
            public void SetPaused(bool paused, long now)
            {
                if (now < _last) throw new ArgumentOutOfRangeException(nameof(now));
                if (_paused == paused) return;
                _paused = paused; _last = now;
                if (paused) { _contact.SafeStop(_component); WaitingReason = "停电或休眠：有效贡献已保留"; }
            }
            public void Advance(long now)
            {
                if (_disposed || IsCompleted) return;
                if (now < _last) throw new ArgumentOutOfRangeException(nameof(now));
                double seconds = (now - _last) / 1000d; _last = now;
                if (_paused) return;
                if (!_facility.IsAvailable || !_region.TryGet(_context.Machine.Id, out var machine)) { Stop(BehaviorOutcome.TargetInvalid); return; }
                Vector3 target = _facility.transform.position; double height = 0;
                if (_tree.IsValid)
                {
                    if (!_facility.Forest.TryRead(_tree, out var tree)) { Stop(BehaviorOutcome.TargetInvalid); return; }
                    if (_fell && tree.Stage == TreeStage.Stump) { Stop(BehaviorOutcome.Completed); return; }
                    if (_fell) return;
                    if (tree.Stage != TreeStage.Mature && tree.Stage != TreeStage.Growing) { Stop(BehaviorOutcome.TargetInvalid); return; }
                    target = tree.Position; height = tree.Height * _ratio;
                }
                else
                {
                    if (_facility.Mineral.RemainingExact <= 0) { Stop(_produced > 0 ? BehaviorOutcome.Partial : BehaviorOutcome.TargetInvalid); return; }
                    target = new Vector3(machine.Position.x, 0, machine.Position.y); // A deposit is worked anywhere in its valid area, not on decorative rocks.
                }
                if (_context.Machine.IsMoving) { WaitingReason = "等待机器停稳"; _contact.SafeStop(_component); return; }
                var result = _bridge.RequestWork(_queue, _context.Machine.Id, machine.Position, (int)_request.Priority);
                if (result != WorkRequestResult.Granted) { WaitingReason = result == WorkRequestResult.Waiting ? "等待作业通道" : "机器不在有效作业区"; return; }
                if (!_contact.TryContact(_component, _request.Target.Id, _queue.WorkArea, _tree, target, height, out var reason)) { WaitingReason = reason; _contact.SafeStop(_component); return; }
                WaitingReason = null; _elapsed += seconds; _contact.SetWorking(_component, true, _elapsed);
                if (seconds <= 0) return;
                if (_tree.IsValid)
                {
                    if (!_facility.Forest.TryCut(_tree, _facility.Rules.SawDamage * seconds, _ratio, now, out _fell, out reason))
                    { WaitingReason = reason; if (reason == "TreeMissing") Stop(BehaviorOutcome.TargetInvalid); }
                    else if (_fell)
                    { _production.Track(_tree, _context, _component, _request); _contact.SafeStop(_component); }
                }
                else
                {
                    double power = Number(_request.Parameters, "power", 1);
                    if (!ProductionRules.Finite(power) || power < 0 || power > 1) { Stop(BehaviorOutcome.Failed); return; }
                    var definition = _component.Definition;
                    _context.Machine.SetComponentWorkingPower(_component.Id, definition.IdlePower + (definition.WorkingPower - definition.IdlePower) * power * power);
                    double rate = _facility.Rules.DrillDamage(power) / _facility.Rules.MiningResistance;
                    seconds = Math.Min(seconds, Math.Max(0, _requested - _produced - _facility.Mineral.FractionalContribution) / rate);
                    if (seconds > 0 && _facility.Mineral.TryDrill(_request.Id, ++_sequence, seconds, power, now, out var receipt, out reason))
                    { _produced += receipt.Units; if (_produced >= _requested) Stop(BehaviorOutcome.Completed); }
                    else WaitingReason = reason;
                }
                _facility.Refresh(now);
            }
            public void Stop(BehaviorOutcome outcome)
            {
                if (IsCompleted) return;
                _contact.SafeStop(_component); _context.Machine.SetComponentWorkingPower(_component.Id, null);
                _bridge.ReleaseWork(_queue, _context.Machine.Id); Outcome = outcome; IsCompleted = true;
                if (!_tree.IsValid) _production.ReleaseResponsibility(_request.Id);
            }
            public void Dispose() { if (_disposed) return; if (!IsCompleted) Stop(BehaviorOutcome.Cancelled); _disposed = true; _bridge.Dispose(); }
        }
    }
}
