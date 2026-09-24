using AutoEra.Algorithms;
using AutoEra.World.Identity;
using NUnit.Framework;
using System.Linq;

namespace AutoEra.Tests.Editor
{
    public sealed class AlgorithmInitialTemplatesEditModeTests
    {
        [Test]
        public void IrrigationTemplate_Compiles_WithCostNine()
        {
            var g = InitialAlgorithmTemplates.Irrigation();
            Assert.That(AlgorithmValidator.TryCompile(g, 100, out var plan, out var issues, true), Is.True, "基础灌溉模板应可通过模板编译");
            Assert.That(plan.LogicCost, Is.EqualTo(9), "DEC-121 基础灌溉正式逻辑成本为 9");
            Assert.That(issues.Count, Is.EqualTo(0));
        }

        [Test]
        public void DrillingTemplate_Compiles()
        {
            var g = InitialAlgorithmTemplates.Drilling();
            Assert.That(AlgorithmValidator.TryCompile(g, 100, out var plan, out var issues, true), Is.True, "储量开采模板应可通过模板编译");
            Assert.That(plan.LogicCost, Is.EqualTo(15));
            Assert.That(issues.Count, Is.EqualTo(0));
        }

        [Test]
        public void HarvestingTemplate_Compiles()
        {
            var g = InitialAlgorithmTemplates.Harvesting();
            Assert.That(AlgorithmValidator.TryCompile(g, 100, out var plan, out var issues, true), Is.True, "生长采集模板应可通过模板编译");
            Assert.That(plan.LogicCost, Is.EqualTo(17));
            Assert.That(issues.Count, Is.EqualTo(0));
        }

        [Test]
        public void FarmingTemplate_Compiles()
        {
            var g = InitialAlgorithmTemplates.Farming();
            Assert.That(AlgorithmValidator.TryCompile(g, 100, out var plan, out var issues, true), Is.True, "农田作业模板应可通过模板编译");
            Assert.That(plan.LogicCost, Is.EqualTo(14));
            Assert.That(issues.Count, Is.EqualTo(0));
        }

        [Test]
        public void TransportTemplate_Compiles()
        {
            var g = InitialAlgorithmTemplates.Transport();
            bool ok = AlgorithmValidator.TryCompile(g, 100, out var plan, out var issues, true);
            if (!ok)
            {
                var msgs = string.Join("; ", issues.Select(i => $"{i.Code}@{i.NodeId}:{i.PortId}"));
                Assert.Fail("固定路线运输模板编译失败: " + msgs);
            }
            // DEC-123 正式逻辑成本 34；+1 来自第一版简化（来源/目的地位置参数、Delay 30 秒的常量节点）。
            Assert.That(plan.LogicCost, Is.EqualTo(35));
            Assert.That(issues.Count, Is.EqualTo(0));
        }

        [Test]
        public void Seed_PopulatesSystemTemplateLibrary()
        {
            var library = new AlgorithmTemplateLibrary(new PersistentIdAllocator());
            InitialAlgorithmTemplates.Seed(library);
            var list = library.List();
            Assert.That(list.Length, Is.EqualTo(5));
            Assert.That(list[0].Name, Is.EqualTo("基础灌溉"));
            Assert.That(list[1].Name, Is.EqualTo("储量资源开采"));
            Assert.That(list[2].Name, Is.EqualTo("生长资源采集"));
            Assert.That(list[3].Name, Is.EqualTo("农田基础作业"));
            Assert.That(list[4].Name, Is.EqualTo("固定路线运输"));
            foreach (var info in list) Assert.That(info.IsSystem, Is.True);

            Assert.That(library.TryGetDocument(list[0].Id, out var doc), Is.True, "系统模板详情可读");
            Assert.That(doc.Bindings.Count, Is.EqualTo(0), "模板库保存时清除绑定，绑定由实例化后补全");

            var instantiated = library.Instantiate(list[1].Id);
            Assert.That(instantiated, Is.Not.Null, "系统模板可实例化为玩家草稿");
            Assert.That(instantiated.DocumentId, Is.Not.EqualTo(doc.DocumentId), "实例化分配新的文档 ID");
        }
    }
}
