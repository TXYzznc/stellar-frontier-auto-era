using AutoEra.Algorithms;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class AlgorithmGraphEditModeTests
    {
        private static AlgorithmDocument Graph()
        {
            var graph = new AlgorithmDocument { DocumentId = 100 };
            graph.Nodes.Add(new AlgorithmNode { Id = 101, Kind = AlgorithmNodeKind.Constant, Default = AlgorithmValue.Numeric(2) });
            graph.Nodes.Add(new AlgorithmNode { Id = 102, Kind = AlgorithmNodeKind.Arithmetic, Operator = AlgorithmOperator.Add });
            graph.Edges.Add(new AlgorithmEdge { From = 101, To = 102, Input = "a" });
            graph.Edges.Add(new AlgorithmEdge { From = 101, To = 102, Input = "b" });
            return graph;
        }
        [Test]
        public void RoundTrip_PlanIsPrivate_DeletedNodeRetainsEdges()
        {
            var original = Graph();
            var restored = JsonUtility.FromJson<AlgorithmDocument>(JsonUtility.ToJson(original));
            Assert.That(AlgorithmValidator.TryCompile(restored, 40, out var plan, out var errors), Is.True);
            restored.Nodes[0].Default.Number = 99;
            Assert.That(plan.CopyDocument().Nodes[0].Default.Number, Is.EqualTo(2));
            Assert.That(restored.DeleteNode(101), Is.True);
            Assert.That(restored.Edges.Count, Is.EqualTo(2));
            Assert.That(AlgorithmValidator.TryCompile(restored, 40, out _, out errors), Is.False);
            Assert.That(errors.Exists(x => x.Code == "DanglingEdge"), Is.True);
        }
        [Test]
        public void UnitCapacityAndCycles_RejectInsteadOfConverting()
        {
            var graph = Graph();
            graph.Nodes[0].ValueType.Unit = "m"; graph.Nodes[0].Default.Type.Unit = "m";
            Assert.That(AlgorithmValidator.TryCompile(graph, 40, out _, out var errors), Is.False);
            Assert.That(errors.Exists(x => x.Code == "TypeUnitCapabilityMismatch"), Is.True);
            graph = Graph(); graph.Edges[0].From = 102;
            Assert.That(AlgorithmValidator.TryCompile(graph, 1, out _, out errors), Is.False);
            Assert.That(errors.Exists(x => x.Code == "SynchronousCycle"), Is.True);
            Assert.That(errors.Exists(x => x.Code == "LogicCapacity"), Is.True);
        }
        [Test]
        public void RequiredBindingAndEnumFamily_AreExplicit()
        {
            var graph = new AlgorithmDocument { DocumentId = 1 };
            graph.Nodes.Add(new AlgorithmNode { Id = 2, Kind = AlgorithmNodeKind.Input, BindingKey = "sensor" });
            Assert.That(AlgorithmValidator.TryCompile(graph, 40, out _, out _), Is.False);
            Assert.That(AlgorithmValidator.TryCompile(graph, 40, out _, out _, true), Is.True);
            Assert.That(AlgorithmCatalog.Compatible(new AlgorithmType { Kind = AlgorithmValueKind.Enumeration, EnumFamily = "machine" },
                new AlgorithmType { Kind = AlgorithmValueKind.Enumeration, EnumFamily = "crop" }), Is.False);
        }
        [Test]
        public void ConstantUnitInference_IsPrivateAndRejectsConflictingFanout()
        {
            var g=Graph();g.Nodes[1].ValueType.Unit="m";
            Assert.That(AlgorithmValidator.TryCompile(g,40,out var plan,out _),Is.True);
            Assert.That(g.Nodes[0].ValueType.Unit,Is.Empty);Assert.That(plan.CopyDocument().Nodes[0].ValueType.Unit,Is.EqualTo("m"));
            g.Nodes.Add(new AlgorithmNode { Id=103,Kind=AlgorithmNodeKind.Arithmetic,Operator=AlgorithmOperator.Add,ValueType=AlgorithmType.Of(AlgorithmValueKind.Number,"s") });
            g.Edges.Add(new AlgorithmEdge { From=101,To=103,Input="a" });g.Edges.Add(new AlgorithmEdge { From=101,To=103,Input="b" });
            Assert.That(AlgorithmValidator.TryCompile(g,40,out _,out var issues),Is.False);Assert.That(issues.Exists(x=>x.Code=="ConstantUnitConflict"),Is.True);
        }
    }
}
