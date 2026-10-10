using System;
using System.Collections.Generic;
using System.Linq;
using AutoEra.Save;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Time;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class OfflineEventDeterminismEditModeTests
    {
        private static readonly DateTimeOffset Utc=new DateTimeOffset(2026,10,8,0,0,0,TimeSpan.Zero);
        private sealed class Executor : IOfflineAlgorithmProtection
        {
            internal readonly WorldClock Clock;
            internal readonly List<string> Trace=new List<string>();
            internal readonly HashSet<string> Receipts=new HashSet<string>();
            internal readonly Dictionary<ulong,ulong> Generations=new Dictionary<ulong,ulong>();
            internal Action<OfflineScheduledEvent,OfflineEventScheduler> After;
            internal bool Boundary=true,Observe;internal int Quantity,Stopped;
            internal Executor(WorldClock clock) {Clock=clock;}
            public bool IsAtCommitBoundary=>Boundary;
            public bool IsCurrent(OfflineScheduledEvent item)=>!Generations.TryGetValue(item.Subject,out var generation) || item.Generation==generation;
            public bool TryAdvanceTo(long time,out string reason) {reason=null;return Clock.TryAdvanceTo(time);}
            public bool TryObserveAlgorithm(OfflineScheduledEvent item,out OfflineAlgorithmObservation observation)
            {observation=Observe ? new OfflineAlgorithmObservation {Algorithm=item.Subject,Generation=item.Generation,Cause=item.Payload.Step,ProgressRevision=(ulong)Quantity,WorldMilliseconds=Clock.WorldMilliseconds} : null;return Observe;}
            public bool TryStopNoProgressAlgorithm(PersistentId algorithm,out string reason)
            {Stopped++;Generations[algorithm.Value]=2;reason=null;return true;}
            public bool TryExecute(OfflineScheduledEvent item,OfflineEventScheduler scheduler,out string reason)
            {
                reason=null;Trace.Add(item.Due+":"+item.Batch+":"+item.Phase+":"+item.Subject+":"+item.Sequence);
                if(item.Kind==OfflineEventKind.ResourceCommit && Receipts.Add(item.Subject+":"+item.Payload.Step))Quantity++;
                After?.Invoke(item,scheduler);return true;
            }
        }
        private static OfflineEventSpec Event(long time,WorldEventPhase phase,ulong subject,ulong step=1)
        {
            var kinds=new[] {OfflineEventKind.EnergyBoundary,OfflineEventKind.ForestBoundary,OfflineEventKind.SensorSample,OfflineEventKind.AlgorithmWake,OfflineEventKind.TaskBoundary,OfflineEventKind.ResourceCommit};
            return new OfflineEventSpec {Due=time,Phase=phase,Subject=new PersistentId(subject),Generation=1,Kind=kinds[(int)phase],Domain=phase.ToString(),Payload=new OfflineEventPayload {Step=step}};
        }
        private static T Json<T>(T state) where T:class
        {
            var text=WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(0,1000,1,"Offline",new[] {new WorldSnapshotSection("offline",1,state)}));
            Assert.That(WorldSnapshotCodec.TryRead(text,new Dictionary<string,int>{{"offline",1}},out var file,out var reason),Is.True,reason);
            Assert.That(file.TryReadSection<T>("offline",out var result,out reason),Is.True,reason);return result;
        }
        private static void Finish(OfflineEventScheduler scheduler,int budget)
        {for(int i=0;i<1000 && !scheduler.Completed && scheduler.FailureReason==null;i++)scheduler.Pump(budget,1000);Assert.That(scheduler.FailureReason,Is.Null);Assert.That(scheduler.Completed,Is.True);}
        [Test] public void DifferentRegistrationOrderAndFrameBudgets_ProduceExactlyTheSameTraceAndReceipts()
        {
            var rows=new List<OfflineEventSpec>();for(int p=5;p>=0;p--)for(ulong id=3;id>0;id--)rows.Add(Event(10,(WorldEventPhase)p,id));rows.Add(Event(20,WorldEventPhase.ResourceAndReward,1,2));
            var aClock=new WorldClock();var bClock=new WorldClock();var a=new Executor(aClock);var b=new Executor(bClock);
            var sa=new OfflineEventScheduler(aClock,a,Utc,Utc.AddMilliseconds(30),"same");var sb=new OfflineEventScheduler(bClock,b,Utc,Utc.AddMilliseconds(30),"same");
            sa.Seed(rows);rows.Reverse();sb.Seed(rows);Finish(sa,1);Finish(sb,100);
            CollectionAssert.AreEqual(a.Trace,b.Trace);CollectionAssert.AreEquivalent(a.Receipts,b.Receipts);Assert.That(b.Quantity,Is.EqualTo(a.Quantity));
            sa.TryCapturePersistent(out var savedA);sb.TryCapturePersistent(out var savedB);Assert.That(Json(savedA).NextSequence,Is.EqualTo(savedB.NextSequence));
        }
        [Test] public void EarlierPhaseGeneratedAtSameTime_RunsOnlyInTheFollowingBatch_AndCheckpointWaitsForWholeBatch()
        {
            var clock=new WorldClock();var executor=new Executor(clock);var scheduler=new OfflineEventScheduler(clock,executor,Utc,Utc.AddMilliseconds(20),"batch");
            executor.After=(item,s)=> {if(item.Phase==WorldEventPhase.Algorithm)s.Schedule(Event(10,WorldEventPhase.Energy,1));};
            scheduler.Seed(new[] {Event(10,WorldEventPhase.Algorithm,1),Event(10,WorldEventPhase.TaskAndBehavior,2)});
            scheduler.Pump(1,1000);Assert.That(scheduler.TryCapturePersistent(out _),Is.False);
            scheduler.Pump(1,1000);Assert.That(scheduler.TryCapturePersistent(out var boundary),Is.True);Assert.That(boundary.Events[0].Batch,Is.EqualTo(1));
            Assert.That(OfflineEventScheduler.TryRestorePersistent(Json(boundary),clock,executor,out var restored,out var reason),Is.True,reason);
            Finish(restored,1);Assert.That(executor.Trace[2],Does.StartWith("10:1:Energy"));Assert.That(executor.Trace[1],Does.Contain("TaskAndBehavior"));
        }
        [Test] public void CancelInsideBatch_ClosesOnlyAfterRemainingEvents_ThenResumesWithOriginalRunAndLockedUtc()
        {
            var clock=new WorldClock();var executor=new Executor(clock);var scheduler=new OfflineEventScheduler(clock,executor,Utc,Utc.AddMilliseconds(30),"resume-original");
            scheduler.Seed(new[] {Event(10,WorldEventPhase.ResourceAndReward,1),Event(10,WorldEventPhase.ResourceAndReward,2),Event(20,WorldEventPhase.ResourceAndReward,3)});
            scheduler.Pump(1,1000);scheduler.RequestCancel();scheduler.Pump(1,1000);Assert.That(scheduler.Cancelled,Is.True);Assert.That(executor.Quantity,Is.EqualTo(2));
            Assert.That(scheduler.TryCapturePersistent(out var saved),Is.True);var newClock=new WorldClock(saved.WorldMilliseconds);var next=new Executor(newClock) {Quantity=executor.Quantity};foreach(var id in executor.Receipts)next.Receipts.Add(id);
            Assert.That(OfflineEventScheduler.TryRestorePersistent(Json(saved),newClock,next,out var restored,out var reason),Is.True,reason);Assert.That(restored.RunId,Is.EqualTo("resume-original"));
            restored.Resume();Finish(restored,1);Assert.That(next.Quantity,Is.EqualTo(3));restored.TryCapturePersistent(out var final);Assert.That(final.TargetUtcTicks,Is.EqualTo(saved.TargetUtcTicks));
        }
        [Test] public void ObsoleteGeneration_IsDiscardedWithoutTouchingReplacement_AndDetachedPayloadCannotChangeTheHeap()
        {
            var clock=new WorldClock();var executor=new Executor(clock);executor.Generations[1]=2;var scheduler=new OfflineEventScheduler(clock,executor,Utc,Utc.AddMilliseconds(20),"generation");
            var old=Event(10,WorldEventPhase.ResourceAndReward,1);var next=Event(11,WorldEventPhase.ResourceAndReward,1);next.Generation=2;scheduler.Seed(new[] {old,next});next.Payload.Step=99;
            Finish(scheduler,1);Assert.That(executor.Quantity,Is.EqualTo(1));Assert.That(scheduler.ObsoleteEvents,Is.EqualTo(1));Assert.That(executor.Receipts.Contains("1:1"),Is.True);
        }
        [Test] public void ClockRollback_DoesNotReverseWorldOrGrantAnything_AndCompletedCancelCannotCreateInvalidCheckpoint()
        {
            var clock=new WorldClock(500);var executor=new Executor(clock);var scheduler=new OfflineEventScheduler(clock,executor,Utc,Utc.AddDays(-1),"rollback");
            scheduler.Seed(new[] {Event(600,WorldEventPhase.ResourceAndReward,1)});Finish(scheduler,1);scheduler.RequestCancel();scheduler.Pump(1,1000);
            Assert.That(clock.WorldMilliseconds,Is.EqualTo(500));Assert.That(executor.Quantity,Is.Zero);Assert.That(scheduler.Cancelled,Is.False);
            Assert.That(scheduler.TryCapturePersistent(out var saved),Is.True);Assert.That(OfflineEventScheduler.TryRestorePersistent(Json(saved),clock,executor,out _,out var reason),Is.True,reason);
        }
        [TestCase("target")] [TestCase("sequence")] [TestCase("batch")] [TestCase("lost")]
        public void CorruptCheckpointOrderingOrLockedTarget_IsRejectedBeforeAnyDomainEvent(string field)
        {
            var clock=new WorldClock();var executor=new Executor(clock);var scheduler=new OfflineEventScheduler(clock,executor,Utc,Utc.AddMilliseconds(30),"bad");
            scheduler.Seed(new[] {Event(10,WorldEventPhase.ResourceAndReward,1),Event(20,WorldEventPhase.ResourceAndReward,2)});scheduler.Pump(1,1000);scheduler.TryCapturePersistent(out var saved);var data=Json(saved);
            if(field=="target")data.TargetWorldMilliseconds++;
            if(field=="sequence")data.Events[0].Sequence=data.NextSequence;
            if(field=="batch")data.Events[0].Batch=1;
            if(field=="lost")data.Events=Array.Empty<OfflineScheduledEvent>();
            Assert.That(OfflineEventScheduler.TryRestorePersistent(data,clock,executor,out var target,out _),Is.False);Assert.That(target,Is.Null);Assert.That(executor.Quantity,Is.EqualTo(1));
        }
        [Test] public void PartialDomainFailure_DoesNotProduceARecoverableCheckpoint()
        {
            var clock=new WorldClock();var executor=new Executor(clock);executor.After=(item,s)=>executor.Boundary=false;
            var scheduler=new OfflineEventScheduler(clock,executor,Utc,Utc.AddMilliseconds(20),"partial");scheduler.Schedule(Event(10,WorldEventPhase.ResourceAndReward,1));scheduler.Pump(10,1000);
            Assert.That(scheduler.FailureReason,Is.Not.Null);Assert.That(scheduler.TryCapturePersistent(out _),Is.False);
        }
        [Test] public void MissingRequiredEconomyOrProgress_RefusesBeforeProducingAnyDomainSections()
        {
            using(var world=new AutoEraWorldSessionFactory().Create(0))
            {
                var domains=new GameplayWorldDomainPersistence(null,null,null,null,null);
                Assert.That(domains.TryCapture(world,out var sections,out var ids,out var reason),Is.False);Assert.That(reason,Does.Contain("经济"));Assert.That(sections,Is.Null);Assert.That(ids,Is.Null);
            }
        }
        [Test] public void SupplyDateAndCommittedMarks_PreserveRollbackAndRejectDuplicatedTransactions()
        {
            var marks=new OfflineSettlementMarks {CommittedPurchases=new[] {1UL},CommittedRewards=new[] {2UL},LastClaimedLocalDate=20261008,LatestSeenLocalDate=20261009};
            var saved=Json(marks);Assert.That(saved.Validate(2),Is.True);Assert.That(saved.IsSupplyEligible(20261007),Is.False);Assert.That(saved.IsSupplyEligible(20261008),Is.False);
            Assert.That(saved.IsSupplyEligible(20261009),Is.True);saved.CommittedRewards=new[] {1UL};Assert.That(saved.Validate(2),Is.False);saved.CommittedRewards=Array.Empty<ulong>();saved.LastClaimedLocalDate=20260230;Assert.That(saved.Validate(2),Is.False);
        }
        [Test] public void DeterministicZeroTimeCycle_StopsOnlyItsAlgorithm_AndOtherDomainStillCommits()
        {
            var clock=new WorldClock();var executor=new Executor(clock) {Observe=true};
            executor.After=(item,s)=> {if(item.Kind==OfflineEventKind.AlgorithmWake)s.Schedule(Event(item.Due,WorldEventPhase.Algorithm,item.Subject));};
            var scheduler=new OfflineEventScheduler(clock,executor,Utc,Utc.AddMilliseconds(30),"cycle");
            scheduler.Seed(new[] {Event(10,WorldEventPhase.Algorithm,1),Event(20,WorldEventPhase.ResourceAndReward,2)});Finish(scheduler,1);
            Assert.That(executor.Stopped,Is.EqualTo(1));Assert.That(executor.Quantity,Is.EqualTo(1));scheduler.TryCapturePersistent(out var saved);
            Assert.That(saved.AlgorithmProtection[0].Protected,Is.True);
        }
        [Test] public void RealSemanticProgress_IsNotStoppedAsAZeroTimeCycle()
        {
            var clock=new WorldClock();var executor=new Executor(clock) {Observe=true};
            executor.After=(item,s)=> {executor.Quantity++;if(executor.Quantity<3)s.Schedule(Event(item.Due,WorldEventPhase.Algorithm,item.Subject));};
            var scheduler=new OfflineEventScheduler(clock,executor,Utc,Utc.AddMilliseconds(20),"progress");scheduler.Seed(new[] {Event(10,WorldEventPhase.Algorithm,1)});Finish(scheduler,1);
            Assert.That(executor.Stopped,Is.Zero);Assert.That(executor.Quantity,Is.EqualTo(3));
        }
        [Test] public void AlgorithmProtectionObservation_SurvivesCheckpointAndGenerationRebind_BeforeCycleCanRepeat()
        {
            var clock=new WorldClock();var executor=new Executor(clock) {Observe=true};
            executor.After=(item,s)=> {if(item.Kind==OfflineEventKind.AlgorithmWake)s.Schedule(Event(10,WorldEventPhase.Energy,2));};
            var scheduler=new OfflineEventScheduler(clock,executor,Utc,Utc.AddMilliseconds(20),"protected-resume");scheduler.Seed(new[] {Event(10,WorldEventPhase.Algorithm,1)});scheduler.Pump(1,1000);
            Assert.That(scheduler.TryCapturePersistent(out var saved),Is.True);Assert.That(saved.AlgorithmProtection.Length,Is.EqualTo(1));
            var nextClock=new WorldClock(saved.WorldMilliseconds);var next=new Executor(nextClock) {Observe=true};next.Generations[1]=2;
            next.After=(item,s)=> {if(item.Kind==OfflineEventKind.EnergyBoundary) {var wake=Event(10,WorldEventPhase.Algorithm,1);wake.Generation=2;s.Schedule(wake);} };
            Assert.That(OfflineEventScheduler.TryRestorePersistent(Json(saved),nextClock,next,out var restored,out var reason),Is.True,reason);restored.RebindAlgorithmGeneration(new PersistentId(1),1,2);
            Finish(restored,1);Assert.That(next.Stopped,Is.EqualTo(1));Assert.That(next.Trace.Any(row=>row.Contains(":Algorithm:")),Is.False);
        }
    }
}
