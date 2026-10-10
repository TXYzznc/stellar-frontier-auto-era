using System;
using System.Collections.Generic;
using AutoEra.Events;
using AutoEra.Save;
using AutoEra.World.Identity;
using AutoEra.World.Time;
using GameFramework;
using GameFramework.Event;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class EventServiceSnapshotEditModeTests
    {
        private sealed class Publisher : IEventPublisher
        {
            internal int Count;
            public void Publish(GameEventArgs args) { Count++; ReferencePool.Release(args); }
        }
        private static void Fact(AutoEraEventService events,CorrelationId correlation)
        {
            var fact=ReferencePool.Acquire<EventAccountabilityEditModeTests.TestFactEventArgs>();
            fact.Initialize(correlation,EventDomain.Task,new PersistentId(7),"task.ended",true,EventOutcome.Succeeded);events.PublishFact(fact);
        }
        [Test] public void JsonRoundTrip_PreservesTraceAndCountersWithoutReplayingFacts()
        {
            using(var source=new AutoEraEventService(new WorldClock(1000),new Publisher()))
            {
                var first=source.OpenCommand(EventDomain.Task,new PersistentId(7),"task.submit");source.PublishNotice(EventDomain.Energy,new PersistentId(7),"energy.powered");Fact(source,first);
                string content=WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(1000,7,1,"Fixture",new[] {new WorldSnapshotSection("events",1,source.Capture())}));
                Assert.That(WorldSnapshotCodec.TryRead(content,new Dictionary<string,int>{{"events",1}},out var file,out var reason),Is.True,reason);
                Assert.That(file.TryReadSection<EventServiceSnapshot>("events",out var snapshot,out reason),Is.True,reason);
                var publisher=new Publisher();using(var restored=new AutoEraEventService(new WorldClock(1000),publisher))
                {
                    restored.Restore(snapshot);Assert.That(publisher.Count,Is.Zero);
                    Assert.That(restored.TryGetTrace(first,out var trace),Is.True);Assert.That(trace.Resolved,Is.True);Assert.That(trace.Outcome,Is.EqualTo(EventOutcome.Succeeded));
                    var next=restored.OpenCommand(EventDomain.Task,new PersistentId(7),"task.next");Assert.That(next.Value,Is.GreaterThan(first.Value));Fact(restored,next);
                    Assert.That(publisher.Count,Is.EqualTo(1));Assert.That(restored.Capture().DispatchSequence,Is.EqualTo(3));
                }
            }
        }
        [Test] public void InvalidSequence_RejectsWholeCandidateBeforeCommittingRecords()
        {
            using(var source=new AutoEraEventService(new WorldClock(1000),null))
            using(var restored=new AutoEraEventService(new WorldClock(1000),null))
            {
                source.PublishNotice(EventDomain.Energy,new PersistentId(7),"first");source.PublishNotice(EventDomain.Energy,new PersistentId(7),"second");
                var snapshot=source.Capture();snapshot.Records[1].Sequence=1;
                Assert.Throws<ArgumentException>(()=>restored.Restore(snapshot));Assert.That(restored.Journal.Count,Is.Zero);
                restored.PublishNotice(EventDomain.Energy,new PersistentId(7),"still usable");Assert.That(restored.Capture().DispatchSequence,Is.EqualTo(1));
            }
        }
        [Test] public void FutureTimeAndUnknownCorrelation_AreRejected()
        {
            using(var source=new AutoEraEventService(new WorldClock(1000),null))
            {
                source.OpenCommand(EventDomain.Task,new PersistentId(7),"command");var snapshot=source.Capture();snapshot.Records[0].WorldMilliseconds=1001;
                using(var restored=new AutoEraEventService(new WorldClock(1000),null))Assert.Throws<ArgumentException>(()=>restored.Restore(snapshot));
                snapshot=source.Capture();snapshot.Records[0].Correlation=2;
                using(var restored=new AutoEraEventService(new WorldClock(1000),null))Assert.Throws<ArgumentException>(()=>restored.Restore(snapshot));
            }
        }
        [Test] public void ExhaustedCounters_RestoreWithoutWrappingToZeroOrReusingIds()
        {
            using(var restored=new AutoEraEventService(new WorldClock(1000),null))
            {
                restored.Restore(new EventServiceSnapshot { CorrelationsAllocatedThrough=ulong.MaxValue,DispatchSequence=ulong.MaxValue,Records=Array.Empty<EventJournalSnapshotRecord>() });
                Assert.Throws<InvalidOperationException>(()=>restored.OpenCommand(EventDomain.Task,new PersistentId(7),"no id"));
                Assert.Throws<InvalidOperationException>(()=>restored.PublishNotice(EventDomain.Energy,new PersistentId(7),"no sequence"));
                Assert.That(restored.Journal.Count,Is.Zero);Assert.That(restored.Capture().DispatchSequence,Is.EqualTo(ulong.MaxValue));
            }
        }
    }
}
