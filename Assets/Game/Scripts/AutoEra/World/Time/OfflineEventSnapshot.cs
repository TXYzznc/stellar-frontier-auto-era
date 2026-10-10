using System;
using System.Collections.Generic;
using AutoEra.World.Identity;

namespace AutoEra.World.Time
{
    public enum OfflineEventKind
    {EnergyBoundary,ForestBoundary,MineralCleanup,SensorSample,AlgorithmWake,TaskBoundary,ResourceCommit,AgricultureBoundary,ManufacturingBoundary,ProgressBoundary}
    /// <summary>Closed data payload. IDs borrow the existing domain responsibility; no delegates or Unity objects.</summary>
    public sealed class OfflineEventPayload
    {
        public ulong Component,Task,Behavior,Target,Step;
        public double Value;
        public string Label;
        internal OfflineEventPayload Copy()=>new OfflineEventPayload {Component=Component,Task=Task,Behavior=Behavior,Target=Target,Step=Step,Value=Value,Label=Label};
    }
    public sealed class OfflineEventSpec
    {
        public string Domain;
        public long Due;
        public WorldEventPhase Phase;
        public PersistentId Subject;
        public ulong Generation;
        public OfflineEventKind Kind;
        public OfflineEventPayload Payload;
    }
    public sealed class OfflineScheduledEvent
    {
        public string Domain;
        public long Due;
        public WorldEventPhase Phase;
        public ulong Subject,Sequence,Generation,Batch;
        public OfflineEventKind Kind;
        public OfflineEventPayload Payload;
        internal WorldEventSortKey Key=>new WorldEventSortKey(Due,Phase,new PersistentId(Subject),Sequence);
        internal OfflineScheduledEvent Copy()=>new OfflineScheduledEvent {Domain=Domain,Due=Due,Phase=Phase,Subject=Subject,Sequence=Sequence,Generation=Generation,Batch=Batch,Kind=Kind,Payload=Payload.Copy()};
    }
    public sealed class OfflineEventSnapshot
    {
        public int Version=1;
        public string RunId;
        public long SavedUtcTicks,TargetUtcTicks,InitialWorldMilliseconds,WorldMilliseconds,TargetWorldMilliseconds,LastBatchWorldMilliseconds;
        public ulong NextSequence,Processed,Obsolete,LastBatch;
        public bool HasClosedBatch,Cancelled,Completed;
        public OfflineScheduledEvent[] Events;
        public OfflineAlgorithmObservation[] AlgorithmProtection;
    }
    public interface IOfflineEventExecutor
    {
        bool IsAtCommitBoundary { get; }
        bool IsCurrent(OfflineScheduledEvent item);
        bool TryAdvanceTo(long worldMilliseconds,out string reason);
        bool TryExecute(OfflineScheduledEvent item,OfflineEventScheduler scheduler,out string reason);
    }
    public sealed partial class OfflineEventScheduler
    {
        public bool TryCapturePersistent(out OfflineEventSnapshot saved)
        {
            saved=null;if(!IsAtCheckpointBoundary)return false;
            var events=new List<OfflineScheduledEvent>(_heap.Count);foreach(var row in _heap)events.Add(row.Copy());events.Sort(Compare);
            saved=new OfflineEventSnapshot {RunId=RunId,SavedUtcTicks=_savedUtcTicks,TargetUtcTicks=_targetUtcTicks,InitialWorldMilliseconds=_initial,
                WorldMilliseconds=WorldMilliseconds,TargetWorldMilliseconds=_target,NextSequence=_nextSequence,Processed=_processed,Obsolete=_obsolete,
                LastBatchWorldMilliseconds=_lastBatchTime,LastBatch=_lastBatch,HasClosedBatch=_hasClosedBatch,Cancelled=_cancelled,Completed=_completed,Events=events.ToArray(),AlgorithmProtection=_algorithmProtection.Capture()};return true;
        }
        public static bool TryRestorePersistent(OfflineEventSnapshot saved,WorldClock clock,IOfflineEventExecutor executor,out OfflineEventScheduler restored,out string reason)
        {
            restored=null;reason="离线检查点不完整";
            if(saved?.Version!=1 || clock==null || executor==null || !executor.IsAtCommitBoundary || clock.WorldMilliseconds!=saved.WorldMilliseconds ||
                saved.InitialWorldMilliseconds<0 || saved.WorldMilliseconds<saved.InitialWorldMilliseconds || saved.WorldMilliseconds>saved.TargetWorldMilliseconds ||
                saved.Events==null || saved.AlgorithmProtection==null || saved.LastBatchWorldMilliseconds< -1 || saved.LastBatchWorldMilliseconds>saved.WorldMilliseconds ||
                saved.HasClosedBatch!=(saved.LastBatchWorldMilliseconds>=0) || saved.Completed && (saved.WorldMilliseconds!=saved.TargetWorldMilliseconds || saved.Cancelled))return false;
            try
            {
                var origin=new WorldClock(saved.InitialWorldMilliseconds);
                var candidate=new OfflineEventScheduler(origin,executor,new DateTimeOffset(saved.SavedUtcTicks,TimeSpan.Zero),new DateTimeOffset(saved.TargetUtcTicks,TimeSpan.Zero),saved.RunId);
                if(candidate._target!=saved.TargetWorldMilliseconds)throw new ArgumentException("Offline target was changed after locking UTC.");
                if(!candidate._algorithmProtection.Restore(saved.AlgorithmProtection,saved.WorldMilliseconds))throw new ArgumentException("Invalid algorithm cycle protection checkpoint.");
                var sequences=new HashSet<ulong>();
                ulong allocated=saved.NextSequence==0 ? ulong.MaxValue : saved.NextSequence-1;
                if(checked(saved.Processed+saved.Obsolete+(ulong)saved.Events.Length)!=allocated)throw new ArgumentException("Offline queue lost an allocated event.");
                foreach(var row in saved.Events)
                {
                    if(row==null)throw new ArgumentException("Null pending event.");
                    Validate(new OfflineEventSpec {Domain=row.Domain,Due=row.Due,Phase=row.Phase,Subject=new PersistentId(row.Subject),Generation=row.Generation,Kind=row.Kind,Payload=row.Payload});
                    if(row.Due<saved.WorldMilliseconds || row.Sequence==0 || saved.NextSequence!=0 && row.Sequence>=saved.NextSequence || !sequences.Add(row.Sequence) ||
                        row.Due==saved.LastBatchWorldMilliseconds && row.Batch<=saved.LastBatch ||
                        row.Due>saved.LastBatchWorldMilliseconds && row.Batch!=0 || saved.Completed && row.Due<=saved.TargetWorldMilliseconds)
                        throw new ArgumentException("Invalid original offline ordering identity.");
                }
                restored=new OfflineEventScheduler(clock,executor,candidate,saved);reason=null;return true;
            }
            catch(Exception error) when(error is ArgumentException || error is InvalidOperationException || error is OverflowException)
            {reason=error.Message;return false;}
        }
        private OfflineEventScheduler(WorldClock clock,IOfflineEventExecutor executor,OfflineEventScheduler header,OfflineEventSnapshot saved)
        {
            _clock=clock;_executor=executor;RunId=header.RunId;_savedUtcTicks=header._savedUtcTicks;_targetUtcTicks=header._targetUtcTicks;_initial=header._initial;_target=header._target;
            _nextSequence=saved.NextSequence;_processed=saved.Processed;_obsolete=saved.Obsolete;_lastBatchTime=saved.LastBatchWorldMilliseconds;
            _lastBatch=saved.LastBatch;_hasClosedBatch=saved.HasClosedBatch;_cancelled=saved.Cancelled;_cancelRequested=saved.Cancelled;_completed=saved.Completed;
            _algorithmProtection.Restore(saved.AlgorithmProtection,saved.WorldMilliseconds);
            foreach(var row in saved.Events)Push(row.Copy());
        }
    }
}
