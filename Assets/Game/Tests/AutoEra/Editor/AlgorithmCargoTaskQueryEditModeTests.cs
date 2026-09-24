using AutoEra.Algorithms;
using AutoEra.Machines;
using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class AlgorithmCargoTaskQueryEditModeTests
    {
        private sealed class QuerySink : IAlgorithmCommandSink
        {
            public int Capacity = 20, Used = 6;
            public string HasItemType = "银穗麦";
            public bool HasActiveTask;
            public ulong ActiveTaskId = 42;
            public bool IsSafe => true;
            public ulong Submit(AlgorithmTrigger trigger, AlgorithmIntent intent) => trigger.TaskId;
            public void EndBatch(AlgorithmTrigger trigger) { }
            public void Cancel() { }
            public bool TryReadCargo(string field, string itemType, out AlgorithmValue value)
            {
                switch (field)
                {
                    case "capacity": value = AlgorithmValue.Numeric(Capacity, "count"); return true;
                    case "remaining": value = AlgorithmValue.Numeric(Capacity - Used, "count"); return true;
                    case "has_item": value = AlgorithmValue.Bool(itemType == HasItemType); return true;
                    default: value = null; return false;
                }
            }
            public bool TryQueryTask(string name, out AlgorithmValue task)
            {
                task = new AlgorithmValue { Type = AlgorithmType.Of(AlgorithmValueKind.Object), ObjectId = HasActiveTask ? ActiveTaskId : 0, IsValid = true };
                return true;
            }
        }

        [Test]
        public void Cargo_LoadUnload_And_OverflowReject()
        {
            var cargo = new MachineCargo(20);
            Assert.That(cargo.TryLoad("银穗麦", 6), Is.True);
            Assert.That(cargo.Remaining, Is.EqualTo(14));
            Assert.That(cargo.Has("银穗麦"), Is.True);
            Assert.That(cargo.TryLoad("铁", 100), Is.False, "超容拒绝");
            Assert.That(cargo.TryUnload("银穗麦", 2), Is.True);
            Assert.That(cargo.Count("银穗麦"), Is.EqualTo(4));
            Assert.That(cargo.Remaining, Is.EqualTo(16));
        }

        [Test]
        public void CargoNode_ReadsCapacityRemainingHasItem()
        {
            var sink = new QuerySink { Capacity = 20, Used = 6, HasItemType = "银穗麦" };
            var g = new AlgorithmDocument { DocumentId = 1 };
            g.Nodes.Add(new AlgorithmNode { Id = 1, Kind = AlgorithmNodeKind.Startup });
            g.Nodes.Add(new AlgorithmNode { Id = 2, Kind = AlgorithmNodeKind.Cargo, Field = "银穗麦" });
            g.Nodes.Add(new AlgorithmNode { Id = 3, Kind = AlgorithmNodeKind.SetVariable, StateKey = "cap", ValueType = AlgorithmType.Of(AlgorithmValueKind.Number) });
            g.Nodes.Add(new AlgorithmNode { Id = 4, Kind = AlgorithmNodeKind.SetVariable, StateKey = "rem", ValueType = AlgorithmType.Of(AlgorithmValueKind.Number) });
            g.Nodes.Add(new AlgorithmNode { Id = 5, Kind = AlgorithmNodeKind.SetVariable, StateKey = "has", ValueType = AlgorithmType.Of(AlgorithmValueKind.Boolean) });
            g.Edges.Add(new AlgorithmEdge { From = 1, To = 3, Output = "event", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 1, To = 4, Output = "event", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 1, To = 5, Output = "event", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 2, To = 3, Output = "capacity", Input = "value" });
            g.Edges.Add(new AlgorithmEdge { From = 2, To = 4, Output = "remaining", Input = "value" });
            g.Edges.Add(new AlgorithmEdge { From = 2, To = 5, Output = "has_item", Input = "value" });
            Assert.That(AlgorithmValidator.TryCompile(g, 100, out var plan, out _), Is.True);
            using var runtime = new AlgorithmRuntime(new PersistentId(1), plan, new MachineComputePool(new PersistentIdAllocator(), 100, 100), sink);
            runtime.Enqueue(new AlgorithmTrigger { NodeId = 1, Port = "event", Revision = 1, Generation = 1 });
            runtime.Pump(0);
            var state = runtime.CopyState();
            Assert.That(state["cap"].Number, Is.EqualTo(20));
            Assert.That(state["rem"].Number, Is.EqualTo(14));
            Assert.That(state["has"].Boolean, Is.True);
        }

        [Test]
        public void QueryTask_Found_True_WhenActive()
        {
            var sink = new QuerySink { HasActiveTask = true, ActiveTaskId = 42 };
            var state = RunQuery(sink);
            Assert.That(state["found"].Boolean, Is.True);
        }

        [Test]
        public void QueryTask_Found_False_WhenIdle()
        {
            var sink = new QuerySink { HasActiveTask = false };
            var state = RunQuery(sink);
            Assert.That(state["found"].Boolean, Is.False);
        }

        private static System.Collections.Generic.Dictionary<string, AlgorithmValue> RunQuery(QuerySink sink)
        {
            var g = new AlgorithmDocument { DocumentId = 1 };
            g.Nodes.Add(new AlgorithmNode { Id = 1, Kind = AlgorithmNodeKind.Startup });
            g.Nodes.Add(new AlgorithmNode { Id = 2, Kind = AlgorithmNodeKind.QueryTask, Field = "transport" });
            g.Nodes.Add(new AlgorithmNode { Id = 3, Kind = AlgorithmNodeKind.SetVariable, StateKey = "found", ValueType = AlgorithmType.Of(AlgorithmValueKind.Boolean) });
            g.Edges.Add(new AlgorithmEdge { From = 1, To = 3, Output = "event", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 2, To = 3, Output = "found", Input = "value" });
            Assert.That(AlgorithmValidator.TryCompile(g, 100, out var plan, out _), Is.True);
            var runtime = new AlgorithmRuntime(new PersistentId(1), plan, new MachineComputePool(new PersistentIdAllocator(), 100, 100), sink);
            runtime.Enqueue(new AlgorithmTrigger { NodeId = 1, Port = "event", Revision = 1, Generation = 1 });
            runtime.Pump(0);
            var state = runtime.CopyState();
            runtime.Dispose();
            return state;
        }

        [Test]
        public void TaskQueue_TryFindActive_ExcludesTerminal()
        {
            var ids = new PersistentIdAllocator();
            var queue = new MachineTaskQueue(ids);
            queue.Submit("transport", WorkPriority.Normal, out var active);
            Assert.That(queue.TryFindActive("transport", out var found), Is.True);
            Assert.That(found.Id, Is.EqualTo(active.Id));
            queue.CloseChain(active.Id);
            Assert.That(queue.TryFindActive("transport", out _), Is.False, "终止态不计入未结束任务");
        }
    }
}
