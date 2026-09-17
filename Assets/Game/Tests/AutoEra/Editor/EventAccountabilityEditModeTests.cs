using System.Collections.Generic;
using AutoEra.Events;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Time;
using GameFramework;
using GameFramework.Event;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class EventAccountabilityEditModeTests
    {
        public sealed class TestFactEventArgs : AutoEraFactEventArgs
        {
            public static readonly int EventId = typeof(TestFactEventArgs).GetHashCode();
            public override int Id => EventId;

            public TestFactEventArgs Initialize(CorrelationId correlation, EventDomain domain, PersistentId source,
                string action, bool terminal, EventOutcome outcome)
            {
                Initialize(domain, correlation, source, action, terminal, outcome);
                return this;
            }
        }

        private sealed class RecordingPublisher : IEventPublisher
        {
            public readonly List<GameEventArgs> Published = new List<GameEventArgs>();
            public void Publish(GameEventArgs args)
            {
                if (args != null) Published.Add(args);
            }
        }

        private static AutoEraEventService CreateService(RecordingPublisher publisher, int capacity = 512)
        {
            return new AutoEraEventService(new WorldClock(1000), publisher, capacity);
        }

        [Test]
        public void CorrelationAllocator_AllocatesMonotonicValidIds()
        {
            var allocator = new CorrelationAllocator();
            Assert.That(allocator.TryAllocate(out var first), Is.True);
            Assert.That(allocator.TryAllocate(out var second), Is.True);
            Assert.That(first.IsValid && second.IsValid, Is.True);
            Assert.That(second.Value, Is.GreaterThan(first.Value));
            Assert.That(allocator.NextId.Value, Is.GreaterThan(second.Value));
        }

        [Test]
        public void OpenCommand_JournalsPendingCommandWithoutBusEvent()
        {
            var publisher = new RecordingPublisher();
            using (var events = CreateService(publisher))
            {
                var correlation = events.OpenCommand(EventDomain.Task, new PersistentId(7), "task.submit");
                Assert.That(correlation.IsValid, Is.True);
                Assert.That(publisher.Published.Count, Is.EqualTo(0));
                Assert.That(events.TryGetTrace(correlation, out var trace), Is.True);
                Assert.That(trace.HasCommand, Is.True);
                Assert.That(trace.Resolved, Is.False);
                Assert.That(trace.FactCount, Is.EqualTo(0));
                Assert.That(trace.Command.Action, Is.EqualTo("task.submit"));
                Assert.That(trace.Command.Source, Is.EqualTo(new PersistentId(7)));
            }
        }

        [Test]
        public void Facts_ResolveTraceWithFinalOutcome()
        {
            var publisher = new RecordingPublisher();
            using (var events = CreateService(publisher))
            {
                var correlation = events.OpenCommand(EventDomain.Effector, new PersistentId(3), "effector.run");
                var running = ReferencePool.Acquire<TestFactEventArgs>();
                running.Initialize(correlation, EventDomain.Effector, new PersistentId(3), "effector.started", false, EventOutcome.None);
                events.PublishFact(running);
                var terminal = ReferencePool.Acquire<TestFactEventArgs>();
                terminal.Initialize(correlation, EventDomain.Effector, new PersistentId(3), "effector.ended", true, EventOutcome.Succeeded);
                events.PublishFact(terminal);

                Assert.That(publisher.Published.Count, Is.EqualTo(2));
                Assert.That(events.TryGetTrace(correlation, out var trace), Is.True);
                Assert.That(trace.Resolved, Is.True);
                Assert.That(trace.FactCount, Is.EqualTo(2));
                Assert.That(trace.Outcome, Is.EqualTo(EventOutcome.Succeeded));
                Assert.That(trace.Terminal.WorldMilliseconds, Is.EqualTo(1000));
                Assert.That(trace.Terminal.Sequence, Is.GreaterThan(0));
            }
        }

        [Test]
        public void Journal_WrapsAndKeepsConstantCapacity()
        {
            var publisher = new RecordingPublisher();
            using (var events = CreateService(publisher, 4))
            {
                var oldest = events.OpenCommand(EventDomain.Task, PersistentId.Invalid, "task.submit");
                CorrelationId newest = default;
                for (int i = 0; i < 7; i++) newest = events.OpenCommand(EventDomain.Task, PersistentId.Invalid, "task.submit");

                Assert.That(events.Journal.Count, Is.EqualTo(4));
                Assert.That(events.Journal.Capacity, Is.EqualTo(4));
                Assert.That(events.TryGetTrace(oldest, out _), Is.False);
                Assert.That(events.TryGetTrace(newest, out var trace), Is.True);
                Assert.That(trace.HasCommand, Is.True);

                var fact = ReferencePool.Acquire<TestFactEventArgs>();
                fact.Initialize(newest, EventDomain.Task, PersistentId.Invalid, "task.ended", true, EventOutcome.Cancelled);
                ulong before = 0;
                Assert.That(events.TryGetTrace(newest, out var pre), Is.True);
                before = pre.Terminal.Sequence;
                events.PublishFact(fact);
                Assert.That(events.TryGetTrace(newest, out var after), Is.True);
                Assert.That(after.Terminal.Sequence, Is.GreaterThan(before));
            }
        }

        [Test]
        public void TraceQuery_IsReadOnly()
        {
            var publisher = new RecordingPublisher();
            using (var events = CreateService(publisher))
            {
                var correlation = events.OpenCommand(EventDomain.Resource, PersistentId.Invalid, "resource.change");
                var fact = ReferencePool.Acquire<TestFactEventArgs>();
                fact.Initialize(correlation, EventDomain.Resource, PersistentId.Invalid, "resource.changed", true, EventOutcome.Succeeded);
                events.PublishFact(fact);
                int countBefore = events.Journal.Count;
                Assert.That(events.TryGetTrace(correlation, out var first), Is.True);
                Assert.That(events.TryGetTrace(correlation, out var second), Is.True);
                Assert.That(events.Journal.Count, Is.EqualTo(countBefore));
                Assert.That(first.FactCount, Is.EqualTo(second.FactCount));
                Assert.That(first.Outcome, Is.EqualTo(second.Outcome));
            }
        }

        [Test]
        public void Service_AfterDispose_RejectsPublishing()
        {
            var events = CreateService(new RecordingPublisher());
            events.Dispose();
            Assert.That(() => events.OpenCommand(EventDomain.Task, PersistentId.Invalid, "task.submit"),
                Throws.TypeOf<System.ObjectDisposedException>());
            Assert.That(() => events.PublishFact(ReferencePool.Acquire<TestFactEventArgs>()
                    .Initialize(new CorrelationId(99UL), EventDomain.Task, PersistentId.Invalid, "x", false, EventOutcome.None)),
                Throws.TypeOf<System.ObjectDisposedException>());
        }

        [Test]
        public void Commands_RequireValidAction()
        {
            using (var events = CreateService(new RecordingPublisher()))
            {
                Assert.That(() => events.OpenCommand(EventDomain.Task, PersistentId.Invalid, " "),
                    Throws.TypeOf<System.ArgumentException>());
            }
        }
    }
}