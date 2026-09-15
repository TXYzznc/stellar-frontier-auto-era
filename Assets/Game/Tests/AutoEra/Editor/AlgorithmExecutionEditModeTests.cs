using System.Collections.Generic;
using AutoEra.Algorithms;
using AutoEra.Machines;
using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class AlgorithmExecutionEditModeTests
    {
        private sealed class Sink : IAlgorithmCommandSink
        {
            public bool IsSafe => true;
            public int Commands, Cancellations;
            public ulong Submit(AlgorithmTrigger trigger, AlgorithmIntent intent) { Commands++; return trigger.TaskId; }
            public void EndBatch(AlgorithmTrigger trigger) { }
            public void Cancel() { Cancellations++; }
        }
        internal static AlgorithmDocument Graph(bool divideByZero = false)
        {
            var g = new AlgorithmDocument { DocumentId = 1 };
            g.Nodes.Add(new AlgorithmNode { Id = 1, Kind = AlgorithmNodeKind.Startup });
            g.Nodes.Add(new AlgorithmNode { Id = 2, Kind = AlgorithmNodeKind.Constant, Default = AlgorithmValue.Numeric(12) });
            g.Nodes.Add(new AlgorithmNode { Id = 3, Kind = AlgorithmNodeKind.SetVariable, StateKey = "counter" });
            g.Nodes.Add(new AlgorithmNode { Id = 4, Kind = AlgorithmNodeKind.Log });
            g.Edges.Add(new AlgorithmEdge { From = 1, To = 3, Output = "event", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 2, To = 3, Input = "value" });
            g.Edges.Add(new AlgorithmEdge { From = 1, To = 4, Output = "event", Input = "event" });
            if (divideByZero)
            {
                g.Nodes.Add(new AlgorithmNode { Id = 5, Kind = AlgorithmNodeKind.Constant, Default = AlgorithmValue.Numeric(0) });
                g.Nodes.Add(new AlgorithmNode { Id = 6, Kind = AlgorithmNodeKind.Arithmetic, Operator = AlgorithmOperator.Divide });
                g.Nodes.Add(new AlgorithmNode { Id = 7, Kind = AlgorithmNodeKind.SetVariable, StateKey = "invalid" });
                g.Edges.Add(new AlgorithmEdge { From = 2, To = 6, Input = "a" });
                g.Edges.Add(new AlgorithmEdge { From = 5, To = 6, Input = "b" });
                g.Edges.Add(new AlgorithmEdge { From = 6, To = 7, Input = "value" });
                g.Edges.Add(new AlgorithmEdge { From = 1, To = 7, Output = "event", Input = "event" });
            }
            return g;
        }
        private static AlgorithmPlan Compile(AlgorithmDocument graph)
        { Assert.That(AlgorithmValidator.TryCompile(graph, 100, out var plan, out _), Is.True); return plan; }
        [Test]
        public void BatchError_CommitsNoStateOrCommand_AndRetainsCause()
        {
            var sink = new Sink(); var pool = new MachineComputePool(new PersistentIdAllocator(), 100, 100);
            using (var runtime = new AlgorithmRuntime(new PersistentId(999), Compile(Graph(true)), pool, sink))
            {
                runtime.Enqueue(new AlgorithmTrigger { NodeId = 1, Revision = 1, Generation = 1 }); runtime.Pump(0);
                Assert.That(runtime.Invalid, Is.True); Assert.That(runtime.CopyState(), Is.Empty); Assert.That(sink.Commands, Is.Zero);
                Assert.That(runtime.History()[0].Error, Is.EqualTo("DivisionByZero")); Assert.That(runtime.History()[0].FailedNode, Is.EqualTo(6));
                Assert.That(pool.Used, Is.Zero);
            }
        }
        [Test]
        public void WaitingIsNotFailure_WholeBatchRunsAfterLease_StaleEventsRejected()
        {
            var sink = new Sink(); var pool = new MachineComputePool(new PersistentIdAllocator(), 100, 100);
            pool.Submit(100, ComputeClass.Evaluation, WorkPriority.Normal, ComputeMergeKind.None, new PersistentId(90), 1, 0, false, out var held);
            using (var runtime = new AlgorithmRuntime(new PersistentId(999), Compile(Graph()), pool, sink))
            {
                Assert.That(runtime.Enqueue(new AlgorithmTrigger { NodeId = 1, Revision = 0, Generation = 1 }), Is.False);
                runtime.Enqueue(new AlgorithmTrigger { NodeId = 1, Revision = 1, Generation = 1 }); runtime.Pump(0);
                Assert.That(runtime.LastReason, Is.EqualTo("WaitingCompute")); Assert.That(runtime.Invalid, Is.False);
                Assert.That(runtime.CopyState(), Is.Empty); Assert.That(sink.Commands, Is.Zero);
                pool.Release(held.Id); runtime.Pump(1);
                Assert.That(runtime.CopyState()["counter"].Number, Is.EqualTo(12)); Assert.That(sink.Commands, Is.EqualTo(1));
                Assert.That(pool.Used, Is.EqualTo(2), "An active variable retains its state cost.");
                runtime.SetPaused(true,1); Assert.That(pool.Used,Is.Zero);
                runtime.SetPaused(false,2); runtime.Pump(2); Assert.That(pool.Used,Is.EqualTo(2));
                for (int i = 0; i < 32; i++) Assert.That(runtime.Enqueue(new AlgorithmTrigger { NodeId = 1, Revision = 1, Generation = 1 }), Is.True);
                Assert.That(runtime.Enqueue(new AlgorithmTrigger { NodeId = 1, Revision = 1, Generation = 1 }), Is.False);
                runtime.CancelPending(); Assert.That(runtime.WaitingCount, Is.Zero); Assert.That(pool.WaitingCount, Is.Zero);
            }
        }
        [Test]
        public void HistoryIsBounded_AndSnapshotsAreNotMutableAliases()
        {
            var pool = new MachineComputePool(new PersistentIdAllocator(), 100, 100);
            using (var runtime = new AlgorithmRuntime(new PersistentId(999), Compile(Graph()), pool, new Sink()))
            {
                for (ulong i = 1; i <= 55; i++) { runtime.Enqueue(new AlgorithmTrigger { NodeId = 1, Revision = 1, Generation = 1, Sequence = i }); runtime.Pump((long)i); }
                Assert.That(runtime.History().Length, Is.EqualTo(50)); Assert.That(runtime.History()[0].CopyTrigger().Sequence, Is.EqualTo(6));
                var copy = runtime.CopyState(); copy["counter"].Number = -9; Assert.That(runtime.CopyState()["counter"].Number, Is.EqualTo(12));
            }
        }
        [Test]
        public void DelayOverflowIsAtomic_AndPauseFreezesRemainingDuration()
        {
            var g = Graph(); g.Nodes.Add(new AlgorithmNode { Id = 8, Kind = AlgorithmNodeKind.Constant, ValueType = AlgorithmType.Of(AlgorithmValueKind.Number,"s"), Default = AlgorithmValue.Numeric(1,"s") });
            g.Nodes.Add(new AlgorithmNode { Id = 9, Kind = AlgorithmNodeKind.Delay });
            g.Nodes.Add(new AlgorithmNode { Id = 10, Kind = AlgorithmNodeKind.Log });
            g.Edges.Add(new AlgorithmEdge { From = 1, To = 9, Output = "event", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 8, To = 9, Input = "seconds" });
            g.Edges.Add(new AlgorithmEdge { From = 9, To = 10, Output = "event", Input = "event" });
            var sink = new Sink(); var pool = new MachineComputePool(new PersistentIdAllocator(),100,100);
            using (var r = new AlgorithmRuntime(new PersistentId(999),Compile(g),pool,sink))
            {
                r.Enqueue(new AlgorithmTrigger { NodeId = 1, Revision = 1, Generation = 1 }); r.Pump(0);
                r.SetPaused(true,200); r.SetPaused(false,1200); r.Pump(1999); Assert.That(sink.Commands,Is.EqualTo(1));
                r.Pump(2000); Assert.That(sink.Commands,Is.EqualTo(2));
            }
            g.Nodes.Find(x=>x.Id==8).Default.Number = double.MaxValue;
            using (var r = new AlgorithmRuntime(new PersistentId(999),Compile(g),pool,new Sink()))
            { r.Enqueue(new AlgorithmTrigger { NodeId = 1, Revision = 1, Generation = 1 }); r.Pump(1); Assert.That(r.LastReason,Is.EqualTo("DelayOverflow")); Assert.That(r.CopyState(),Is.Empty); }
        }
        [Test]
        public void ShadowStateIsVisibleWithinBatch_LayoutDoesNotChangeOrder_AndIdleAllocatesNothing()
        {
            var g=Graph();g.Nodes.Add(new AlgorithmNode { Id=7,Kind=AlgorithmNodeKind.Variable,StateKey="counter",Default=AlgorithmValue.Numeric(0) });
            g.Nodes.Add(new AlgorithmNode { Id=8,Kind=AlgorithmNodeKind.SetVariable,StateKey="copy",LayoutX=-999 });
            g.Edges.Add(new AlgorithmEdge { From=1,To=8,Output="event",Input="event" });g.Edges.Add(new AlgorithmEdge { From=7,To=8,Input="value" });
            var pool=new MachineComputePool(new PersistentIdAllocator(),100,100);
            using(var r=new AlgorithmRuntime(new PersistentId(999),Compile(g),pool,new Sink()))
            {
                r.Enqueue(new AlgorithmTrigger { NodeId=1,Revision=1,Generation=1 });r.Pump(0);
                Assert.That(r.CopyState()["copy"].Number,Is.EqualTo(12));r.Pump(1);
                long before=System.GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<1000;i++)r.Pump(2);
                Assert.That(System.GC.GetAllocatedBytesForCurrentThread()-before,Is.Zero);
            }
            Assert.That(pool.Used,Is.Zero);
        }
        [Test]
        public void PauseReasonsAreIndependent_AndCheckpointPreservesThem()
        {
            var pool = new MachineComputePool(new PersistentIdAllocator(),100,100);
            using (var r = new AlgorithmRuntime(new PersistentId(999),Compile(Graph()),pool,new Sink()))
            {
                r.SetPaused(true,0,AlgorithmPauseReason.Machine);
                r.SetPaused(true,1,AlgorithmPauseReason.Application);
                r.SetPaused(false,2,AlgorithmPauseReason.Application);
                Assert.That(r.Paused,Is.True);
                Assert.That(r.TryCapture(2,out var snapshot),Is.True);
                r.SetPaused(false,3,AlgorithmPauseReason.Machine);
                Assert.That(r.Paused,Is.False);
                Assert.That(r.Restore(snapshot,4),Is.True);Assert.That(r.Paused,Is.True);
                r.SetPaused(false,5);Assert.That(r.Paused,Is.True);
                r.SetPaused(false,6,AlgorithmPauseReason.Machine);Assert.That(r.Paused,Is.False);
            }
        }
        [Test]
        public void OnlyExplicitContinuousRootsCoalesce_InstantEventsRemainOrdered()
        {
            var pool=new MachineComputePool(new PersistentIdAllocator(),100,100);
            using(var r=new AlgorithmRuntime(new PersistentId(999),Compile(Graph()),pool,new Sink()))
            {
                r.Enqueue(new AlgorithmTrigger { NodeId=1,Revision=1,Generation=1,Sequence=1,Continuous=true });
                r.Enqueue(new AlgorithmTrigger { NodeId=1,Revision=1,Generation=1,Sequence=2 });
                r.Enqueue(new AlgorithmTrigger { NodeId=1,Revision=1,Generation=1,Sequence=3,Continuous=true });
                Assert.That(r.WaitingCount,Is.EqualTo(2));r.Pump(0);r.Pump(1);
                Assert.That(r.History()[0].CopyTrigger().Sequence,Is.EqualTo(3));Assert.That(r.History()[1].CopyTrigger().Sequence,Is.EqualTo(2));
                Assert.That(r.History()[0].RunId,Is.Not.EqualTo(r.History()[1].RunId));
            }
        }
    }
}
