using AutoEra.Machines;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// `MachineDeploymentFlow` 的验收：**校验与领域提交分离**，每一类前置失败都各自可辨，
    /// 提交只在预览合法时发生，且领域部署只发生一次。
    ///
    /// 占地一律从机器定义取（本夹具显式传参），流程里没有任何硬编码尺寸——
    /// 这条正是「占地显式化」要保住的性质。
    /// </summary>
    public sealed class MachineDeploymentFlowEditModeTests
    {
        private const double CarrierX = 1.8d;
        private const double CarrierZ = 2.6d;

        private static MachineInstance Machine(AutoEraWorldSession session, double footprintX = CarrierX, double footprintZ = CarrierZ)
            => session.Machines.Create(new MachineDefinition(1, "Fixture", 1, 2, 1, 1, 10, true, true, 100, footprintX, footprintZ));

        private static InitialRegion Region(AutoEraWorldSession session)
            => new InitialRegion(session, new Rect(-30, -30, 60, 60));

        [Test]
        public void TryBegin_RefusesAModelWithoutFootprint()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = Region(session))
            using (var flow = new MachineDeploymentFlow(session, region))
            {
                MachineInstance machine = Machine(session, 0d, 0d);
                Assert.That(machine.Definition.HasFootprint, Is.False);

                Assert.That(flow.TryBegin(machine.Id, out string reason), Is.False);
                StringAssert.Contains("占地", reason);
                Assert.That(flow.IsActive, Is.False, "被拒绝的开始不得留下预览。");
                Assert.That(region.Count, Is.Zero);
            }
        }

        [Test]
        public void TryBegin_RefusesUnknownAndAlreadyDeployedMachines()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = Region(session))
            using (var flow = new MachineDeploymentFlow(session, region))
            {
                Assert.That(flow.TryBegin(new PersistentId(9999), out string unknown), Is.False);
                StringAssert.Contains("花名册", unknown);

                MachineInstance machine = Machine(session);
                Assert.That(region.DeployMachine(machine.Id, Vector2.zero, machine.Definition.Footprint, out _),
                    Is.EqualTo(RegionMachineDeploymentResult.Bound));

                Assert.That(flow.TryBegin(machine.Id, out string deployed), Is.False);
                StringAssert.Contains("已部署", deployed);
                Assert.That(flow.IsActive, Is.False);
            }
        }

        [Test]
        public void Begin_UsesTheDefinitionFootprintAndDeploysOnlyOnValidCommit()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = Region(session))
            using (var flow = new MachineDeploymentFlow(session, region))
            {
                MachineInstance machine = Machine(session);
                Assert.That(flow.TryBegin(machine.Id, out _), Is.True);
                Assert.That(flow.Size.x, Is.EqualTo((float)CarrierX).Within(0.0001f),
                    "占地必须来自定义，而不是调用点写常量。");
                Assert.That(flow.Size.y, Is.EqualTo((float)CarrierZ).Within(0.0001f));
                Assert.That(flow.Preview, Is.Not.Null);

                // 还没选点：预览非法，提交必须什么都不做。
                Assert.That(flow.TryCommit(out RegionObject none, out string noPosition), Is.False);
                Assert.That(none, Is.Null);
                Assert.That(noPosition, Is.Not.Null.And.Not.Empty);
                Assert.That(region.Count, Is.Zero, "未选点的提交不得部署。");
                Assert.That(machine.Deployed, Is.False);

                Assert.That(flow.TryBegin(machine.Id, out _), Is.True);
                flow.Preview.Move(new Vector2(5, 5));
                Assert.That(flow.Preview.IsValid, Is.True, flow.Preview.Reason);

                Assert.That(flow.TryCommit(out RegionObject deployed, out string reason), Is.True, reason);
                Assert.That(deployed, Is.Not.Null);
                Assert.That(region.Count, Is.EqualTo(1));
                Assert.That(region.TryGet(machine.Id, out RegionObject model), Is.True);
                Assert.That(model, Is.SameAs(deployed));
                Assert.That(machine.Deployed, Is.True);
                Assert.That(flow.IsActive, Is.False, "提交后必须结束本次落位。");
                Assert.That(flow.TryCommit(out _, out string twice), Is.False, "结束后再提交必须是明确的失败。");
                StringAssert.Contains("尚未开始", twice);
            }
        }

        [Test]
        public void Commit_ReportsOverlapWithTheRegionReason()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = Region(session))
            using (var flow = new MachineDeploymentFlow(session, region))
            {
                region.Register(PersistentObjectKind.Building, "障碍", Vector2.zero, new Vector2(4, 4));
                MachineInstance machine = Machine(session);

                Assert.That(flow.TryBegin(machine.Id, out _), Is.True);
                flow.Preview.Move(Vector2.zero);
                Assert.That(flow.Preview.IsValid, Is.False);
                StringAssert.Contains("占地冲突", flow.Preview.Reason);

                Assert.That(flow.TryCommit(out _, out string reason), Is.False);
                StringAssert.Contains("占地冲突", reason, "原因必须来自区域校验，不能退化成一句泛化的失败。");
                Assert.That(machine.Deployed, Is.False);
                Assert.That(region.Count, Is.EqualTo(1), "失败提交不得改变区域。");
            }
        }

        [Test]
        public void Commit_RevalidatesAtCommit_AndReportsStalePlacement()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = Region(session))
            using (var flow = new MachineDeploymentFlow(session, region))
            {
                MachineInstance machine = Machine(session);
                Assert.That(flow.TryBegin(machine.Id, out _), Is.True);
                flow.Preview.Move(new Vector2(10, 0));
                Assert.That(flow.Preview.IsValid, Is.True, flow.Preview.Reason);

                // 场地在选点之后变了：确认时必须再校验一次并拒绝，而不是照旧提交。
                region.Register(PersistentObjectKind.Building, "后来者", new Vector2(10, 0), new Vector2(2, 2));

                Assert.That(flow.TryCommit(out RegionObject model, out string reason), Is.False);
                Assert.That(model, Is.Null);
                StringAssert.Contains("失效", reason);
                Assert.That(machine.Deployed, Is.False);
                Assert.That(region.Count, Is.EqualTo(1), "只有「后来者」，机器没有落进来。");
            }
        }

        [Test]
        public void Cancel_LeavesEverythingUntouchedAndIsIdempotent()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = Region(session))
            using (var flow = new MachineDeploymentFlow(session, region))
            {
                MachineInstance machine = Machine(session);
                Assert.That(flow.TryBegin(machine.Id, out _), Is.True);
                flow.Preview.Move(new Vector2(3, 3));
                flow.Cancel();

                Assert.That(flow.IsActive, Is.False);
                Assert.That(flow.Preview, Is.Null);
                Assert.That(region.Count, Is.Zero);
                Assert.That(machine.Deployed, Is.False);
                Assert.DoesNotThrow(() => flow.Cancel());

                // 取消之后可以重新开始——同一个流程实例要能复用（界面就是反复开的）。
                Assert.That(flow.TryBegin(machine.Id, out _), Is.True);
                Assert.That(flow.IsActive, Is.True);
            }
        }

        [Test]
        public void RepeatedDeploymentThroughTheFlow_DoesNotTeleport()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = Region(session))
            using (var flow = new MachineDeploymentFlow(session, region))
            {
                MachineInstance machine = Machine(session);
                Assert.That(flow.TryBegin(machine.Id, out _), Is.True);
                flow.Preview.Move(new Vector2(4, 4));
                Assert.That(flow.TryCommit(out RegionObject first, out string reason), Is.True, reason);

                // 已经部署的机器不能再走一次落位流程。
                Assert.That(flow.TryBegin(machine.Id, out string again), Is.False);
                StringAssert.Contains("已部署", again);

                // 领域规则本身：重复部署同一台机器到**不同**位置不得变成瞬移。
                Assert.That(region.DeployMachine(machine.Id, new Vector2(9, 9), machine.Definition.Footprint, out _, 0),
                    Is.EqualTo(RegionMachineDeploymentResult.InvalidPlacement));
                Assert.That(region.Count, Is.EqualTo(1));
                Assert.That(region.TryGet(machine.Id, out RegionObject still), Is.True);
                Assert.That(still, Is.SameAs(first));
            }
        }
    }
}
