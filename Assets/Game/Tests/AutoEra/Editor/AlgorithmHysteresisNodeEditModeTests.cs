using System.Collections.Generic;
using AutoEra.Algorithms;
using AutoEra.Machines;
using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class AlgorithmHysteresisNodeEditModeTests
    {
        private sealed class CapturingSink : IAlgorithmCommandSink
        {
            public bool IsSafe => true;
            public readonly List<ulong> Logged = new List<ulong>();
            public ulong Submit(AlgorithmTrigger trigger, AlgorithmIntent intent) { if (intent.Kind == AlgorithmNodeKind.Log) Logged.Add(intent.NodeId); return trigger.TaskId; }
            public void EndBatch(AlgorithmTrigger trigger) { }
            public void Cancel() { }
            public bool TryReadCargo(string field, string itemType, out AlgorithmValue value) { value = null; return false; }
            public bool TryQueryTask(string name, out AlgorithmValue task) { task = null; return false; }
        }

        // Startup(1) → Hysteresis(2) → Log(3)；湿度经 Input(bindingKey="humidity") 注入，阈值常量 50/65。
        private static AlgorithmDocument HysteresisGraph()
        {
            var g = new AlgorithmDocument { DocumentId = 1 };
            g.Nodes.Add(new AlgorithmNode { Id = 1, Kind = AlgorithmNodeKind.Startup });
            g.Nodes.Add(new AlgorithmNode { Id = 6, Kind = AlgorithmNodeKind.Input, BindingKey = "humidity", ValueType = AlgorithmType.Of(AlgorithmValueKind.Number) });
            g.Nodes.Add(new AlgorithmNode { Id = 7, Kind = AlgorithmNodeKind.Constant, Default = AlgorithmValue.Numeric(50) });
            g.Nodes.Add(new AlgorithmNode { Id = 8, Kind = AlgorithmNodeKind.Constant, Default = AlgorithmValue.Numeric(65) });
            g.Nodes.Add(new AlgorithmNode { Id = 2, Kind = AlgorithmNodeKind.Hysteresis, StateKey = "spray" });
            g.Nodes.Add(new AlgorithmNode { Id = 3, Kind = AlgorithmNodeKind.Log });
            g.Edges.Add(new AlgorithmEdge { From = 1, To = 2, Output = "event", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 6, To = 2, Input = "value" });
            g.Edges.Add(new AlgorithmEdge { From = 7, To = 2, Input = "on" });
            g.Edges.Add(new AlgorithmEdge { From = 8, To = 2, Input = "off" });
            g.Edges.Add(new AlgorithmEdge { From = 2, To = 3, Output = "event", Input = "event" });
            return g;
        }

        private static AlgorithmTrigger Humidity(ulong humidity)
        {
            var trigger = new AlgorithmTrigger { NodeId = 1, Revision = 1, Generation = 1 };
            trigger.Inputs.Add("humidity", AlgorithmValue.Numeric(humidity));
            return trigger;
        }

        [Test]
        public void HysteresisPortsAndCost_AreStateNode()
        {
            var inputs = AlgorithmCatalog.Inputs(new AlgorithmNode { Kind = AlgorithmNodeKind.Hysteresis });
            Assert.That(Port(inputs, "value"), Is.Not.Null);
            Assert.That(Port(inputs, "on"), Is.Not.Null);
            Assert.That(Port(inputs, "off"), Is.Not.Null);
            var outputs = AlgorithmCatalog.Outputs(new AlgorithmNode { Kind = AlgorithmNodeKind.Hysteresis });
            Assert.That(Port(outputs, "value"), Is.Not.Null, "迟滞公开锁存状态");
            Assert.That(Port(outputs, "event"), Is.Not.Null, "迟滞公开状态变化事件");
            Assert.That(AlgorithmCatalog.Cost(AlgorithmNodeKind.Hysteresis), Is.EqualTo(2));
        }

        [Test]
        public void Hysteresis_BelowOnThreshold_LocksAndEmitsEvent()
        {
            var sink = new CapturingSink();
            var pool = new MachineComputePool(new PersistentIdAllocator(), 100, 100);
            Assert.That(AlgorithmValidator.TryCompile(HysteresisGraph(), 100, out var plan, out _, true), Is.True);
            using (var r = new AlgorithmRuntime(new PersistentId(999), plan, pool, sink))
            { r.Enqueue(Humidity(40)); r.Pump(0); }
            Assert.That(sink.Logged, Does.Contain(3UL), "低于开启阈值应锁定并触发事件");
        }

        [Test]
        public void Hysteresis_WithinBand_HoldsWithoutEvent()
        {
            var sink = new CapturingSink();
            var pool = new MachineComputePool(new PersistentIdAllocator(), 100, 100);
            Assert.That(AlgorithmValidator.TryCompile(HysteresisGraph(), 100, out var plan, out _, true), Is.True);
            using (var r = new AlgorithmRuntime(new PersistentId(999), plan, pool, sink))
            { r.Enqueue(Humidity(60)); r.Pump(0); }
            Assert.That(sink.Logged, Is.Empty, "阈值带内保持锁存状态，不触发事件");
        }

        [Test]
        public void Hysteresis_AboveOffThreshold_WhenLocked_UnlocksAndEmitsEvent()
        {
            var sink = new CapturingSink();
            var pool = new MachineComputePool(new PersistentIdAllocator(), 100, 100);
            Assert.That(AlgorithmValidator.TryCompile(HysteresisGraph(), 100, out var plan, out _, true), Is.True);
            using (var r = new AlgorithmRuntime(new PersistentId(999), plan, pool, sink))
            {
                r.Enqueue(Humidity(40)); r.Pump(0);   // 锁定
                r.Enqueue(Humidity(70)); r.Pump(1);   // 停止
            }
            Assert.That(sink.Logged.Count, Is.EqualTo(2), "锁定与解锁各触发一次状态变化事件");
        }

        [Test]
        public void Hysteresis_MissingStateKey_IsRejected()
        {
            var g = HysteresisGraph();
            g.Nodes.Find(x => x.Id == 2).StateKey = "";
            Assert.That(AlgorithmValidator.TryCompile(g, 100, out _, out var issues), Is.False);
            Assert.That(issues.Exists(x => x.Code == "StateKeyMissing"), Is.True);
        }

        private static AlgorithmPort Port(AlgorithmPort[] ports, string key)
        {
            foreach (var port in ports) if (port.Key == key) return port;
            return null;
        }
    }
}
