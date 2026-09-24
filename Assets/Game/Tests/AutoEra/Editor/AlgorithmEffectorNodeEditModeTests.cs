using System.Collections.Generic;
using AutoEra.Algorithms;
using AutoEra.Machines;
using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class AlgorithmEffectorNodeEditModeTests
    {
        private sealed class CapturingSink : IAlgorithmCommandSink
        {
            public bool IsSafe => true;
            public readonly List<AlgorithmIntent> Intents = new List<AlgorithmIntent>();
            public ulong Submit(AlgorithmTrigger trigger, AlgorithmIntent intent) { Intents.Add(intent); return trigger.TaskId; }
            public void EndBatch(AlgorithmTrigger trigger) { }
            public void Cancel() { }
            public bool TryReadCargo(string field, string itemType, out AlgorithmValue value) { value = null; return false; }
            public bool TryQueryTask(string name, out AlgorithmValue task) { task = null; return false; }
        }

        private static AlgorithmNode Effector(ulong id, AlgorithmEffectorAction action) =>
            new AlgorithmNode { Id = id, Kind = AlgorithmNodeKind.Effector, Action = action, BindingKey = "arm" };

        [Test]
        public void EffectorPorts_VaryByAction()
        {
            var spray = AlgorithmCatalog.Inputs(Effector(2, AlgorithmEffectorAction.Spray));
            Assert.That(Port(spray, "on").Required, Is.True, "喷射开关必填");
            Assert.That(Port(spray, "flow").Required, Is.False, "标准流量是选填端口");
            var sow = AlgorithmCatalog.Inputs(Effector(3, AlgorithmEffectorAction.Sow));
            Assert.That(Port(sow, "count").Required, Is.True, "播种的计划数量必填");
            var stop = AlgorithmCatalog.Inputs(Effector(4, AlgorithmEffectorAction.StopSpray));
            Assert.That(Port(stop, "flow"), Is.Null, "停喷无需流量参数");
            var outputs = AlgorithmCatalog.Outputs(Effector(5, AlgorithmEffectorAction.Cut));
            Assert.That(Port(outputs, "completed"), Is.Not.Null);
            Assert.That(Port(outputs, "rejected"), Is.Not.Null);
            Assert.That(Port(outputs, "targetInvalid"), Is.Not.Null);
        }

        [Test]
        public void EffectorCost_IsOne()
        {
            Assert.That(AlgorithmCatalog.Cost(AlgorithmNodeKind.Effector), Is.EqualTo(1));
            Assert.That(AlgorithmCatalog.Cost(AlgorithmNodeKind.SubmitTask), Is.EqualTo(4));
            Assert.That(AlgorithmCatalog.Cost(AlgorithmNodeKind.QueryTask), Is.EqualTo(4));
        }

        [Test]
        public void EffectorEvaluation_ProducesIntentWithActionAndParameters()
        {
            var g = new AlgorithmDocument { DocumentId = 1 };
            g.Nodes.Add(new AlgorithmNode { Id = 1, Kind = AlgorithmNodeKind.Startup });
            g.Nodes.Add(new AlgorithmNode { Id = 3, Kind = AlgorithmNodeKind.Constant, Default = AlgorithmValue.Numeric(10) });
            g.Nodes.Add(Effector(4, AlgorithmEffectorAction.Sow));
            g.Edges.Add(new AlgorithmEdge { From = 1, To = 4, Output = "event", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 3, To = 4, Input = "count" });
            var pool = new MachineComputePool(new PersistentIdAllocator(), 100, 100);
            var sink = new CapturingSink();
            Assert.That(AlgorithmValidator.TryCompile(g, 100, out var plan, out _, true), Is.True);
            using (var runtime = new AlgorithmRuntime(new PersistentId(999), plan, pool, sink))
            {
                runtime.Enqueue(new AlgorithmTrigger { NodeId = 1, Revision = 1, Generation = 1 }); runtime.Pump(0);
            }
            Assert.That(sink.Intents.Count, Is.EqualTo(1));
            var intent = sink.Intents[0];
            Assert.That(intent.Kind, Is.EqualTo(AlgorithmNodeKind.Effector));
            Assert.That(intent.Action, Is.EqualTo(AlgorithmEffectorAction.Sow));
            Assert.That(intent.BindingKey, Is.EqualTo("arm"));
            Assert.That(intent.Parameters["count"].Number, Is.EqualTo(10));
        }

        [Test]
        public void EffectorMissingRequiredInput_IsRejectedAtCompile()
        {
            var g = new AlgorithmDocument { DocumentId = 1 };
            g.Nodes.Add(new AlgorithmNode { Id = 1, Kind = AlgorithmNodeKind.Startup });
            g.Nodes.Add(Effector(2, AlgorithmEffectorAction.Sow));
            g.Edges.Add(new AlgorithmEdge { From = 1, To = 2, Output = "event", Input = "event" });
            // target 与 count 均未连接
            Assert.That(AlgorithmValidator.TryCompile(g, 100, out _, out var issues, true), Is.False);
            Assert.That(issues.Exists(x => x.Code == "InputRequired"), Is.True);
        }

        [Test]
        public void EffectorWithoutBinding_IsRejectedInAppliedGraph()
        {
            var g = new AlgorithmDocument { DocumentId = 1 };
            g.Nodes.Add(new AlgorithmNode { Id = 1, Kind = AlgorithmNodeKind.Startup });
            g.Nodes.Add(new AlgorithmNode { Id = 2, Kind = AlgorithmNodeKind.Effector, Action = AlgorithmEffectorAction.Clean });
            g.Edges.Add(new AlgorithmEdge { From = 1, To = 2, Output = "event", Input = "event" });
            Assert.That(AlgorithmValidator.TryCompile(g, 100, out _, out var issues, false), Is.False, "非模板图缺少效应器绑定应拒绝");
            Assert.That(issues.Exists(x => x.Code == "RequiredBinding"), Is.True);
            Assert.That(AlgorithmValidator.TryCompile(g, 100, out _, out _, true), Is.True, "模板图暂不要求绑定");
        }

        private static AlgorithmPort Port(AlgorithmPort[] ports, string key)
        {
            foreach (var port in ports) if (port.Key == key) return port;
            return null;
        }
    }
}
