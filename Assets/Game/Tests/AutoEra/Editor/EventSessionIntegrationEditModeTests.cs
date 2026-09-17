using AutoEra.Application;
using AutoEra.Events;
using AutoEra.Machines;
using AutoEra.World;
using AutoEra.World.Identity;
using GameFramework;
using GameFramework.Event;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class EventSessionIntegrationEditModeTests
    {
        [Test]
        public void Session_WithoutPublisher_TracesFactsJournalOnly()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                Assert.That(session.Events, Is.Not.Null);
                var correlation = session.Events.OpenCommand(EventDomain.Task, PersistentId.Invalid, "task.submit");
                var fact = ReferencePool.Acquire<EventAccountabilityEditModeTests.TestFactEventArgs>();
                fact.Initialize(correlation, EventDomain.Task, PersistentId.Invalid, "task.ended", true, EventOutcome.Succeeded);
                session.Events.PublishFact(fact);
                Assert.That(session.Events.TryGetTrace(correlation, out var trace), Is.True);
                Assert.That(trace.Resolved, Is.True);
                Assert.That(trace.Outcome, Is.EqualTo(EventOutcome.Succeeded));
            }
        }

        [Test]
        public void Session_Disposal_RejectsFurtherPublishing()
        {
            var session = new AutoEraWorldSessionFactory().Create(0);
            session.Dispose();
            Assert.That(() => session.Events.OpenCommand(EventDomain.Task, PersistentId.Invalid, "task.submit"),
                Throws.TypeOf<System.ObjectDisposedException>());
        }

        [Test]
        public void Session_WithPublisher_DeliversFacts()
        {
            var publisher = new RecordingPublisherStub();
            using (var session = new AutoEraWorldSessionFactory().Create(0, publisher))
            {
                var correlation = session.Events.OpenCommand(EventDomain.Alert, PersistentId.Invalid, "alert.raise");
                var fact = ReferencePool.Acquire<EventAccountabilityEditModeTests.TestFactEventArgs>();
                fact.Initialize(correlation, EventDomain.Alert, PersistentId.Invalid, "alert.raised", false, EventOutcome.None);
                session.Events.PublishFact(fact);
                Assert.That(publisher.Published.Count, Is.EqualTo(1));
                Assert.That(publisher.Published[0].Id, Is.EqualTo(EventAccountabilityEditModeTests.TestFactEventArgs.EventId));
            }
        }

        [Test]
        public void GfEventPublisher_InEditMode_ReleasesFactsWithoutBus()
        {
            var fact = ReferencePool.Acquire<EventAccountabilityEditModeTests.TestFactEventArgs>();
            fact.Initialize(new CorrelationId(5UL), EventDomain.Task, PersistentId.Invalid, "task.queued", false, EventOutcome.None);
            Assert.DoesNotThrow(() => new GfEventPublisher().Publish(fact));
        }

        [Test]
        public void TaskQueue_WithService_PublishesLifecycleFactsAndResolvesTrace()
        {
            var publisher = new RecordingPublisherStub();
            using (var session = new AutoEraWorldSessionFactory().Create(0, publisher))
            {
                var ids = session.IdAllocator;
                var roster = new MachineRoster(ids, session.ObjectRegistry);
                var machine = roster.Create(new MachineDefinition(1, "Fixture", 1, 2, 1, 2, 30, true, true, 100));
                roster.Deploy(machine.Id);
                machine.Activate(ManagementOrigin.Field);
                machine.UpdateEnvironment(true, true);
                machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);
                using (var context = new MachineExecutionContext(machine, ids, session.Events))
                {
                    Assert.That(context.Tasks.Submit("T", WorkPriority.Normal, out var task), Is.EqualTo(QueueAdmission.Accepted));
                    context.Tasks.StartNext();
                    context.Tasks.CloseChain(task.Id);

                    Assert.That(task.Correlation, Is.Not.EqualTo(CorrelationId.Invalid));
                    Assert.That(publisher.Published.Count, Is.EqualTo(3));
                    CorrelationId factCorrelation = default;
                    for (int i = 0; i < publisher.Published.Count; i++)
                    {
                        var fact = (MachineTaskFactEventArgs)publisher.Published[i];
                        if (i == 0) factCorrelation = fact.Correlation;
                        Assert.That(fact.Correlation, Is.EqualTo(factCorrelation));
                        Assert.That(fact.Source, Is.EqualTo(machine.Id));
                        Assert.That(fact.TaskId, Is.EqualTo(task.Id));
                        Assert.That(fact.TaskName, Is.EqualTo("T"));
                    }
                    var ended = (MachineTaskFactEventArgs)publisher.Published[2];
                    Assert.That(ended.State, Is.EqualTo(MachineTaskState.Completed));
                    Assert.That(ended.Outcome, Is.EqualTo(EventOutcome.Succeeded));
                    Assert.That(ended.Terminal, Is.True);
                    Assert.That(session.Events.TryGetTrace(task.Correlation, out var trace), Is.True);
                    Assert.That(trace.Resolved, Is.True);
                    Assert.That(trace.Outcome, Is.EqualTo(EventOutcome.Succeeded));
                    Assert.That(trace.FactCount, Is.EqualTo(3));
                }
            }
        }

        [Test]
        public void TaskQueue_Cancellation_PublishesCancelledTerminal()
        {
            var publisher = new RecordingPublisherStub();
            using (var session = new AutoEraWorldSessionFactory().Create(0, publisher))
            {
                var roster = new MachineRoster(session.IdAllocator, session.ObjectRegistry);
                var machine = roster.Create(new MachineDefinition(1, "Fixture", 1, 2, 1, 2, 30, true, true, 100));
                roster.Deploy(machine.Id);
                machine.Activate(ManagementOrigin.Field);
                machine.UpdateEnvironment(true, true);
                machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);
                using (var context = new MachineExecutionContext(machine, session.IdAllocator, session.Events))
                {
                    Assert.That(context.Tasks.Submit("T", WorkPriority.Normal, out var task), Is.EqualTo(QueueAdmission.Accepted));
                    context.Tasks.Cancel(task.Id);
                    Assert.That(task.State, Is.EqualTo(MachineTaskState.Cancelled));
                    var ended = (MachineTaskFactEventArgs)publisher.Published[publisher.Published.Count - 1];
                    Assert.That(ended.Terminal, Is.True);
                    Assert.That(ended.Outcome, Is.EqualTo(EventOutcome.Cancelled));
                }
            }
        }

        [Test]
        public void TaskQueue_WithoutService_KeepsLegacyBehavior()
        {
            var ids = new PersistentIdAllocator();
            var roster = new MachineRoster(ids, new PersistentObjectRegistry(ids));
            var machine = roster.Create(new MachineDefinition(1, "Fixture", 1, 2, 1, 2, 30, true, true, 100));
            roster.Deploy(machine.Id);
            machine.Activate(ManagementOrigin.Field);
            machine.UpdateEnvironment(true, true);
            machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);
            using (var context = new MachineExecutionContext(machine, ids))
            {
                Assert.That(context.Tasks.Submit("T", WorkPriority.Normal, out var task), Is.EqualTo(QueueAdmission.Accepted));
                Assert.That(task.Correlation, Is.EqualTo(CorrelationId.Invalid));
                context.Tasks.StartNext();
                context.Tasks.CloseChain(task.Id);
                Assert.That(task.State, Is.EqualTo(MachineTaskState.Completed));
            }
        }

        private sealed class RecordingPublisherStub : IEventPublisher
        {
            public readonly System.Collections.Generic.List<GameEventArgs> Published =
                new System.Collections.Generic.List<GameEventArgs>();

            public void Publish(GameEventArgs args)
            {
                if (args != null) Published.Add(args);
            }
        }
    }
}