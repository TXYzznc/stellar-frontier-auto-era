using System;
using System.Collections.Generic;
using System.Diagnostics;
using AutoEra.World.Identity;

namespace AutoEra.World.Time
{
    /// <summary>Main-thread event heap. Yielding is allowed inside a batch; persistent checkpoints are not.</summary>
    public sealed partial class OfflineEventScheduler
    {
        private readonly WorldClock _clock;
        private readonly IOfflineEventExecutor _executor;
        private readonly List<OfflineScheduledEvent> _heap=new List<OfflineScheduledEvent>();
        private readonly OfflineAlgorithmCycleGuard _algorithmProtection=new OfflineAlgorithmCycleGuard();
        private readonly long _savedUtcTicks,_targetUtcTicks,_initial,_target;
        private ulong _nextSequence=1,_processed,_obsolete,_lastBatch;
        private long _lastBatchTime=-1;
        private bool _batchOpen,_hasClosedBatch,_pumping,_cancelRequested,_cancelled,_completed;
        private WorldEventSortKey _lastKey;
        public string RunId {get;}
        public string FailureReason {get;private set;}
        public long WorldMilliseconds=>_clock.WorldMilliseconds;
        public long TargetWorldMilliseconds=>_target;
        public ulong ProcessedEvents=>_processed;
        public ulong ObsoleteEvents=>_obsolete;
        public int PendingEvents=>_heap.Count;
        public bool Completed=>_completed;
        public bool Cancelled=>_cancelled;
        public bool IsAtCheckpointBoundary=>!_pumping && !_batchOpen && FailureReason==null && _executor.IsAtCommitBoundary;
        public double Progress=>_target==_initial ? (_completed ? 1 : 0) : Math.Max(0,Math.Min(1,(WorldMilliseconds-_initial)/(double)(_target-_initial)));
        public OfflineEventScheduler(WorldClock clock,IOfflineEventExecutor executor,DateTimeOffset savedUtc,DateTimeOffset targetUtc,string runId)
        {
            _clock=clock ?? throw new ArgumentNullException(nameof(clock));_executor=executor ?? throw new ArgumentNullException(nameof(executor));
            if(string.IsNullOrWhiteSpace(runId))throw new ArgumentException("A persistent offline settlement run ID is required.");
            RunId=runId;_savedUtcTicks=savedUtc.UtcDateTime.Ticks;_targetUtcTicks=targetUtc.UtcDateTime.Ticks;_initial=clock.WorldMilliseconds;
            long delta=TimeUtil.GetNonNegativeDuration(savedUtc,targetUtc).Ticks/TimeSpan.TicksPerMillisecond;_target=checked(_initial+delta);
        }
        public void Seed(IEnumerable<OfflineEventSpec> specifications)
        {
            if(_heap.Count!=0 || _processed!=0 || _obsolete!=0 || _pumping || _hasClosedBatch || specifications==null)throw new InvalidOperationException("Seed requires a fresh scheduler.");
            var rows=new List<OfflineEventSpec>();foreach(var spec in specifications) {Validate(spec);if(spec.Due<WorldMilliseconds)throw new ArgumentException("Past seed event.");rows.Add(spec);}
            rows.Sort(CompareSpecs);foreach(var row in rows)Schedule(row);
        }
        public ulong Schedule(OfflineEventSpec spec)
        {
            Validate(spec);
            if(_completed || FailureReason!=null || spec.Due<WorldMilliseconds || _nextSequence==0)throw new InvalidOperationException("Cannot schedule a past or exhausted event.");
            ulong sequence=_nextSequence;
            var row=new OfflineScheduledEvent {Domain=spec.Domain,Due=spec.Due,Phase=spec.Phase,Subject=spec.Subject.Value,Sequence=sequence,
                Generation=spec.Generation,Kind=spec.Kind,Payload=spec.Payload.Copy()};
            if(spec.Due==_lastBatchTime)
            {
                row.Batch=_lastBatch;
                if(!_batchOpen || row.Key.CompareTo(_lastKey)<=0)row.Batch=checked(_lastBatch+1);
            }
            Push(row);_nextSequence=sequence==ulong.MaxValue ? 0 : sequence+1;return sequence;
        }
        public void RequestCancel() {if(!_completed)_cancelRequested=true;}
        public void Resume() {_cancelRequested=false;_cancelled=false;}
        public void RebindAlgorithmGeneration(PersistentId id,ulong original,ulong restored)
        {
            if(_pumping || _processed!=0 && !_hasClosedBatch)throw new InvalidOperationException("Generation rebinding requires a safe restore boundary.");
            _algorithmProtection.Rebind(id,original,restored);
            foreach(var row in _heap)if(row.Kind==OfflineEventKind.AlgorithmWake && row.Subject==id.Value && row.Generation==original)row.Generation=restored;
        }
        public int Pump(int eventBudget,double millisecondBudget)
        {
            if(eventBudget<=0 || double.IsNaN(millisecondBudget) || double.IsInfinity(millisecondBudget) || millisecondBudget<=0)throw new ArgumentOutOfRangeException(nameof(eventBudget));
            if(_pumping)throw new InvalidOperationException("Offline pumping is not reentrant.");
            if(_completed || _cancelled || FailureReason!=null)return 0;
            _pumping=true;int work=0;long started=Stopwatch.GetTimestamp();
            try
            {
                while(work<eventBudget && (Stopwatch.GetTimestamp()-started)*1000d/Stopwatch.Frequency<millisecondBudget)
                {
                    CloseBatch();if(_cancelRequested && !_batchOpen) {_cancelled=true;break;}
                    if(_heap.Count==0 || _heap[0].Due>_target)
                    {
                        if(!Advance(_target))break;_completed=true;work++;break;
                    }
                    var item=Pop();work++;
                    if(!_executor.IsCurrent(item)) {_obsolete=checked(_obsolete+1);continue;}
                    _batchOpen=true;_lastBatchTime=item.Due;_lastBatch=item.Batch;_lastKey=item.Key;
                    if(!Advance(item.Due))break;
                    if(item.Kind==OfflineEventKind.AlgorithmWake)
                    {
                        if(!(_executor is IOfflineAlgorithmProtection protection)) {FailureReason="真实算法离线保护合同尚未接入";break;}
                        if(protection.TryObserveAlgorithm(item,out var observation))
                        {
                            if(observation.Algorithm!=item.Subject || observation.Generation!=item.Generation || observation.WorldMilliseconds!=WorldMilliseconds)
                            {FailureReason="算法进展观测不属于原事件";break;}
                            if(_algorithmProtection.Observe(observation))
                            {
                                if(!protection.TryStopNoProgressAlgorithm(new PersistentId(item.Subject),out var stopReason) || !_executor.IsAtCommitBoundary)
                                {FailureReason=stopReason ?? "算法无进展保护未完成";break;}
                                _processed=checked(_processed+1);CloseBatch();continue;
                            }
                        }
                    }
                    // This record has left the heap; transfer it to the executor without a per-event copy allocation.
                    if(!_executor.TryExecute(item,this,out var reason)) {FailureReason=reason ?? "Offline domain rejected its event.";break;}
                    if(!_executor.IsAtCommitBoundary) {FailureReason="Offline domain left a partial transaction.";break;}
                    _processed=checked(_processed+1);CloseBatch();
                }
                CloseBatch();if(_cancelRequested && !_batchOpen && !_completed)_cancelled=true;
            }
            catch(Exception error) {FailureReason=error.Message;}
            finally {_pumping=false;}
            return work;
        }
        private bool Advance(long target)
        {
            if(target<WorldMilliseconds) {FailureReason="Offline event moved behind the confirmed world clock.";return false;}
            if(!_executor.TryAdvanceTo(target,out var reason) || WorldMilliseconds!=target || !_executor.IsAtCommitBoundary)
            {FailureReason=reason ?? "Offline domain did not complete the requested world boundary.";return false;}return true;
        }
        private void CloseBatch()
        {
            if(_batchOpen && (_heap.Count==0 || _heap[0].Due!=_lastBatchTime || _heap[0].Batch!=_lastBatch))
            {_batchOpen=false;_hasClosedBatch=true;}
        }
        private static void Validate(OfflineEventSpec spec)
        {
            if(spec==null || string.IsNullOrWhiteSpace(spec.Domain) || spec.Due<0 || !spec.Subject.IsValid || spec.Generation==0 ||
                !Enum.IsDefined(typeof(WorldEventPhase),spec.Phase) || !Enum.IsDefined(typeof(OfflineEventKind),spec.Kind) || spec.Payload==null ||
                double.IsNaN(spec.Payload.Value) || double.IsInfinity(spec.Payload.Value))throw new ArgumentException("Invalid typed offline event.");
            WorldEventPhase phase;
            switch(spec.Kind)
            {
                case OfflineEventKind.EnergyBoundary:phase=WorldEventPhase.Energy;break;
                case OfflineEventKind.ForestBoundary:case OfflineEventKind.MineralCleanup:case OfflineEventKind.AgricultureBoundary:phase=WorldEventPhase.WorldState;break;
                case OfflineEventKind.SensorSample:phase=WorldEventPhase.Sensor;break;
                case OfflineEventKind.AlgorithmWake:phase=WorldEventPhase.Algorithm;break;
                case OfflineEventKind.TaskBoundary:case OfflineEventKind.ManufacturingBoundary:phase=WorldEventPhase.TaskAndBehavior;break;
                default:phase=WorldEventPhase.ResourceAndReward;break;
            }
            if(spec.Phase!=phase)throw new ArgumentException("Offline event kind does not match its causal phase.");
        }
        private static int CompareSpecs(OfflineEventSpec a,OfflineEventSpec b)
        {
            int c=new WorldEventSortKey(a.Due,a.Phase,a.Subject,0).CompareTo(new WorldEventSortKey(b.Due,b.Phase,b.Subject,0));if(c!=0)return c;
            c=StringComparer.Ordinal.Compare(a.Domain,b.Domain);if(c!=0)return c;c=a.Kind.CompareTo(b.Kind);if(c!=0)return c;
            c=a.Generation.CompareTo(b.Generation);if(c!=0)return c;
            c=a.Payload.Component.CompareTo(b.Payload.Component);if(c!=0)return c;c=a.Payload.Task.CompareTo(b.Payload.Task);if(c!=0)return c;
            c=a.Payload.Behavior.CompareTo(b.Payload.Behavior);if(c!=0)return c;c=a.Payload.Target.CompareTo(b.Payload.Target);if(c!=0)return c;
            c=a.Payload.Step.CompareTo(b.Payload.Step);if(c!=0)return c;c=a.Payload.Value.CompareTo(b.Payload.Value);return c!=0 ? c : StringComparer.Ordinal.Compare(a.Payload.Label,b.Payload.Label);
        }
        private static int Compare(OfflineScheduledEvent a,OfflineScheduledEvent b)
        {int c=a.Due.CompareTo(b.Due);if(c!=0)return c;c=a.Batch.CompareTo(b.Batch);return c!=0 ? c : a.Key.CompareTo(b.Key);}
        private void Push(OfflineScheduledEvent value)
        {
            int index=_heap.Count;_heap.Add(value);
            while(index>0) {int parent=(index-1)/2;if(Compare(_heap[parent],value)<=0)break;_heap[index]=_heap[parent];index=parent;}_heap[index]=value;
        }
        private OfflineScheduledEvent Pop()
        {
            var first=_heap[0];var last=_heap[_heap.Count-1];_heap.RemoveAt(_heap.Count-1);if(_heap.Count==0)return first;
            int index=0;
            while(index*2+1<_heap.Count)
            {int child=index*2+1;if(child+1<_heap.Count && Compare(_heap[child+1],_heap[child])<0)child++;if(Compare(last,_heap[child])<=0)break;_heap[index]=_heap[child];index=child;}
            _heap[index]=last;return first;
        }
    }
}
