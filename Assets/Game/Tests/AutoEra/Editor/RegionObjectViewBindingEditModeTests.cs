using System.Reflection;
using AutoEra.Machines;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// `RegionObjectView` 的两条来源通路必须互不串味：
    ///
    /// ① 场景种子走 `Initialize` —— 它**注册**一个新区域对象，释放时**移除**它；
    /// ② 已部署机器走 `BindDeployed` —— 它只**指向**领域已存在的对象，释放时**不得**移除它。
    ///
    /// 为什么必须有第 ② 条：机器的部署状态由花名册与区域拥有，视图只是呈现。
    /// 如果拿 ① 去处理已部署机器，`region.Register` 会再建一个对象（重复注册），
    /// 而释放视图又会把领域对象删掉（机器从区域里凭空消失）。这两条都在下面有断言。
    /// </summary>
    public sealed class RegionObjectViewBindingEditModeTests
    {
        private static MachineInstance Machine(AutoEraWorldSession session, bool mobile = true)
            => session.Machines.Create(new MachineDefinition(mobile ? 1 : 2, "Fixture", 1, 2, 1, 1, 10, mobile, true, 100));

        private static RegionObjectView NewView(string name = "MachineView")
            => new GameObject(name).AddComponent<RegionObjectView>();

        /// <summary>场景种子路径要用到的序列化字段；测试里显式写入并断言字段确实存在。</summary>
        private static void SeedAs(RegionObjectView view, PersistentObjectKind kind, string displayName, Vector2 footprint)
        {
            Set(view, "_kind", kind);
            Set(view, "_displayName", displayName);
            Set(view, "_footprint", footprint);
        }

        private static void Set(RegionObjectView view, string field, object value)
        {
            FieldInfo info = typeof(RegionObjectView).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(info, Is.Not.Null, $"区域视图缺少期望的序列化字段 {field}——测试前提已变，请同步更新。");
            info.SetValue(view, value);
        }

        [Test]
        public void BindDeployed_PointsAtTheExistingObjectWithoutRegisteringAnother()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = new InitialRegion(session, new Rect(-30, -30, 60, 60)))
            {
                MachineInstance machine = Machine(session);
                Assert.That(region.DeployMachine(machine.Id, new Vector2(5, 5), new Vector2(1.8f, 2.4f), out RegionObject deployed, 90),
                    Is.EqualTo(RegionMachineDeploymentResult.Bound));
                int countAfterDeploy = region.Count;

                RegionObjectView view = NewView();
                try
                {
                    view.BindDeployed(region, machine.Id);

                    Assert.That(region.Count, Is.EqualTo(countAfterDeploy),
                        "绑定已部署机器绝不能新建区域对象——那正是「重复注册」这个陷阱。");
                    Assert.That(view.Model, Is.SameAs(deployed), "视图必须指向领域里那个已经存在的对象。");
                }
                finally
                {
                    Object.DestroyImmediate(view.gameObject);
                }
            }
        }

        [Test]
        public void Release_LeavesTheDeployedMachineOwnedByTheDomain()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = new InitialRegion(session, new Rect(-30, -30, 60, 60)))
            {
                MachineInstance machine = Machine(session);
                Assert.That(region.DeployMachine(machine.Id, Vector2.zero, Vector2.one, out RegionObject deployed),
                    Is.EqualTo(RegionMachineDeploymentResult.Bound));

                RegionObjectView first = NewView("first");
                first.BindDeployed(region, machine.Id);
                first.Release();
                Object.DestroyImmediate(first.gameObject);

                Assert.That(region.Count, Is.EqualTo(1),
                    "视图释放不得删除领域对象——机器仍在区域里。");
                Assert.That(region.TryGet(machine.Id, out RegionObject still), Is.True);
                Assert.That(still, Is.SameAs(deployed));
                Assert.That(machine.Deployed, Is.True, "机器必须保持已部署。");

                // 领域对象还在，所以可以再绑一个新视图——这是「表现缺失不等于数据损坏」的可验证形态。
                RegionObjectView second = NewView("second");
                try
                {
                    Assert.DoesNotThrow(() => second.BindDeployed(region, machine.Id),
                        "领域对象仍在，重新绑定必须成功。");
                    Assert.That(second.Model, Is.SameAs(deployed));
                    Assert.That(region.Count, Is.EqualTo(1));
                }
                finally
                {
                    Object.DestroyImmediate(second.gameObject);
                }
            }
        }

        [Test]
        public void BindDeployed_RefusesWhenNothingIsDeployed()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = new InitialRegion(session, new Rect(-30, -30, 60, 60)))
            {
                MachineInstance machine = Machine(session);
                RegionObjectView view = NewView();
                try
                {
                    // 机器在册但尚未部署：绑定必须先失败，否则视图会指向一个不存在的对象。
                    Assert.Throws<System.InvalidOperationException>(() => view.BindDeployed(region, machine.Id));
                    Assert.That(view.Model, Is.Null, "失败的绑定不得留下半绑状态。");

                    Assert.Throws<System.ArgumentException>(() => view.BindDeployed(region, PersistentId.Invalid));
                    Assert.Throws<System.ArgumentNullException>(() => view.BindDeployed(null, machine.Id));
                }
                finally
                {
                    Object.DestroyImmediate(view.gameObject);
                }
            }
        }

        [Test]
        public void BindDeployed_CannotBindTwice()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = new InitialRegion(session, new Rect(-30, -30, 60, 60)))
            {
                MachineInstance machine = Machine(session);
                Assert.That(region.DeployMachine(machine.Id, Vector2.zero, Vector2.one, out _),
                    Is.EqualTo(RegionMachineDeploymentResult.Bound));

                RegionObjectView view = NewView();
                try
                {
                    view.BindDeployed(region, machine.Id);
                    Assert.Throws<System.InvalidOperationException>(() => view.BindDeployed(region, machine.Id),
                        "同一个视图不得再次绑定——那会让上一份绑定无声泄漏。");
                }
                finally
                {
                    Object.DestroyImmediate(view.gameObject);
                }
            }
        }

        [Test]
        public void SceneSeedPath_StillRegistersAndRemoves()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = new InitialRegion(session, new Rect(-30, -30, 60, 60)))
            {
                RegionObjectView view = NewView("seed");
                try
                {
                    SeedAs(view, PersistentObjectKind.Building, "场景建筑", new Vector2(4, 3));
                    view.Initialize(region);

                    Assert.That(region.Count, Is.EqualTo(1), "场景种子路径仍然注册新对象。");
                    Assert.That(region.TryGet(view.Model.Id, out _), Is.True);

                    view.Release();
                    Assert.That(region.Count, Is.Zero,
                        "场景种子路径仍然在释放时移除自己的对象——这条通路的行为刻意保持不变。");
                }
                finally
                {
                    Object.DestroyImmediate(view.gameObject);
                }
            }
        }
    }
}
