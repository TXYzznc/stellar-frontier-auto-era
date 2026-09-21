using AutoEra.Machines;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 区域机器运行时注册表的验收（变更 `region-machine-deployment-runtime` 的第 4 节）。
    ///
    /// 三条必须立住的语义：
    /// ① **不可移动不是失败**。固定式机器同样有算力池、任务队列与传感器（这些与能不能移动无关），
    ///    所以它有运行时、`HasNavigation == false`，而 `NavigationUnavailableReason` 必须是 null——
    ///    如果那里写了原因，界面就会把「本来就该如此」显示成故障。
    /// ② **降级原因必须各自可辨**。四种「本可移动却没有导航」的原因混成一个字符串，
    ///    玩家与日志就都拿不到可行动的信息。
    /// ③ **运行时不进存档**（design.md D4）。运行时是派生的：销毁之后只凭领域事实重建，
    ///    得到的是**空队列**的新运行时，而不是把上一次的工作带回来。
    ///
    /// 这里刻意不放需要真实 NavMesh 的用例——那些属于 `MachineDeploymentRuntimePlayModeTests`
    /// （`NavMeshBuildSource`／`NavMeshAgent` 只有在运行期才是真实形态）。
    /// </summary>
    public sealed class MachineDeploymentRuntimeEditModeTests
    {
        private const double CarrierX = 1.8d;
        private const double CarrierZ = 2.6d;

        private static MachineInstance Machine(AutoEraWorldSession session, bool canMove = true,
            double footprintX = CarrierX, double footprintZ = CarrierZ)
            => session.Machines.Create(new MachineDefinition(1, "Fixture", 1, 2, 1, 1, 10, canMove, true, 100, footprintX, footprintZ));

        private static InitialRegion Region(AutoEraWorldSession session)
            => new InitialRegion(session, new Rect(-30, -30, 60, 60));

        private static RegionMachineRuntimeRegistry Registry(AutoEraWorldSession session, InitialRegion region,
            RegionNavigation navigation = null)
            => new RegionMachineRuntimeRegistry(session, region, navigation, new MachineNavigationSettings(), .32f, 1.8f);

        /// <summary>一块足以让 <see cref="RegionNavigation"/> 构造成功的平地；不重建就不会真的建 NavMesh。</summary>
        private static MeshFilter Ground()
        {
            var ground = new GameObject("RuntimeTestGround");
            var mesh = new Mesh
            {
                vertices = new[]
                {
                    new Vector3(-30, 0, -30), new Vector3(30, 0, -30), new Vector3(30, 0, 30), new Vector3(-30, 0, 30),
                },
                triangles = new[] { 0, 2, 1, 0, 3, 2 },
            };
            mesh.RecalculateBounds();
            ground.AddComponent<MeshFilter>().sharedMesh = mesh;
            return ground.GetComponent<MeshFilter>();
        }

        [Test]
        public void Attach_GivesAnImmovableMachineARuntimeWithoutNavigation()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = Region(session))
            {
                var registry = Registry(session, region);
                try
                {
                    MachineInstance machine = Machine(session, canMove: false);

                    Assert.That(registry.TryAttach(machine, null, out RegionMachineRuntime runtime, out string reason),
                        Is.True, reason);
                    Assert.That(reason, Is.Null);
                    Assert.That(runtime, Is.Not.Null);
                    Assert.That(runtime.MachineId, Is.EqualTo(machine.Id));
                    Assert.That(runtime.HasNavigation, Is.False);
                    Assert.That(runtime.NavigationUnavailableReason, Is.Null,
                        "不可移动不是失败：这里必须是 null，有没有导航能力看 HasNavigation。");
                    Assert.That(runtime.IsNavigationDegraded, Is.False);
                    Assert.That(runtime.Context, Is.Not.Null);
                    Assert.That(runtime.Context.Tasks, Is.Not.Null);
                    Assert.That(runtime.Context.Compute, Is.Not.Null);
                    Assert.That(runtime.Context.Sensors, Is.Not.Null,
                        "算力与传感器与能不能移动无关，不可移动机器同样必须具备。");
                    Assert.That(runtime.Adapter, Is.Not.Null);
                    Assert.That(runtime.Adapter.HasNavigation, Is.False);
                    Assert.That(runtime.Instances, Is.Not.Null);

                    Assert.That(registry.Count, Is.EqualTo(1));
                    Assert.That(registry.TryGet(machine.Id, out RegionMachineRuntime found), Is.True);
                    Assert.That(found, Is.SameAs(runtime), "按身份取回的必须是同一个运行时。");
                    CollectionAssert.AreEquivalent(new[] { runtime }, registry.Runtimes);
                }
                finally { registry.Dispose(); }
            }
        }

        [Test]
        public void Attach_NamesEachReasonAMovableMachineHasNoNavigation()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = Region(session))
            {
                MeshFilter ground = Ground();
                var navigation = new RegionNavigation(region, ground, 1.6f, 2f);
                var withoutGround = Registry(session, region);
                var withoutSurface = Registry(session, region, navigation);
                var withSurface = Registry(session, region, navigation);
                var view = new GameObject("MovableMachineView");
                try
                {
                    // ① 区域未配置导航地面：有视图，但区域根本没有导航。
                    MachineInstance noGround = Machine(session);
                    Assert.That(withoutGround.TryAttach(noGround, view, out RegionMachineRuntime a, out _), Is.True);
                    StringAssert.Contains("未配置导航地面", a.NavigationUnavailableReason);
                    Assert.That(a.HasNavigation, Is.False);
                    Assert.That(a.IsNavigationDegraded, Is.True);

                    // ② 导航面尚未就绪：区域配了导航，但那一帧还没建出来（新建后未 Rebuild）。
                    Assert.That(navigation.IsReady, Is.False);
                    MachineInstance notReady = Machine(session);
                    Assert.That(withoutSurface.TryAttach(notReady, view, out RegionMachineRuntime b, out _), Is.True);
                    StringAssert.Contains("尚未就绪", b.NavigationUnavailableReason);

                    // ③ 没有实体视图：这一条优先于上面两条——没有视图时那才是第一原因。
                    MachineInstance noView = Machine(session);
                    Assert.That(withSurface.TryAttach(noView, null, out RegionMachineRuntime c, out _), Is.True);
                    StringAssert.Contains("没有实体视图", c.NavigationUnavailableReason);

                    // 三个原因互不相同，也都不等于「不可移动」的 null。
                    Assert.That(new[] { a.NavigationUnavailableReason, b.NavigationUnavailableReason, c.NavigationUnavailableReason },
                        Is.Unique);
                }
                finally
                {
                    withoutGround.Dispose();
                    withoutSurface.Dispose();
                    withSurface.Dispose();
                    navigation.Dispose();
                    Object.DestroyImmediate(view);
                    Object.DestroyImmediate(ground.gameObject);
                }
            }
        }

        [Test]
        public void Attach_IsIdempotent_AndADisposedRegistryRefusesWork()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = Region(session))
            {
                var registry = Registry(session, region);
                MachineInstance machine = Machine(session, canMove: false);

                Assert.That(registry.TryAttach(machine, null, out RegionMachineRuntime first, out _), Is.True);
                Assert.That(registry.TryAttach(machine, null, out RegionMachineRuntime second, out string again), Is.True, again);
                Assert.That(second, Is.SameAs(first), "同一台机器重复登记必须幂等，不能出现第二个运行时。");
                Assert.That(registry.Count, Is.EqualTo(1));

                registry.Dispose();
                Assert.That(registry.Count, Is.Zero);
                Assert.That(registry.Runtimes, Is.Empty);
                Assert.That(registry.TryGet(machine.Id, out _), Is.False);

                Assert.That(registry.TryAttach(machine, null, out RegionMachineRuntime none, out string disposed), Is.False);
                Assert.That(none, Is.Null);
                StringAssert.Contains("已被释放", disposed);
                Assert.DoesNotThrow(() => registry.Dispose(), "释放必须幂等。");
            }
        }

        [Test]
        public void Detach_RemovesOnlyTheNamedRuntime()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = Region(session))
            {
                var registry = Registry(session, region);
                try
                {
                    MachineInstance a = Machine(session, canMove: false);
                    MachineInstance b = Machine(session, canMove: false);
                    Assert.That(registry.TryAttach(a, null, out _, out _), Is.True);
                    Assert.That(registry.TryAttach(b, null, out _, out _), Is.True);
                    Assert.That(registry.Count, Is.EqualTo(2));

                    Assert.That(registry.Detach(a.Id), Is.True);
                    Assert.That(registry.Count, Is.EqualTo(1));
                    Assert.That(registry.TryGet(a.Id, out _), Is.False);
                    Assert.That(registry.TryGet(b.Id, out RegionMachineRuntime kept), Is.True);
                    Assert.That(kept.MachineId, Is.EqualTo(b.Id), "撤下一台不得动到另一台。");
                    Assert.That(registry.Detach(a.Id), Is.False, "重复撤下是 false，不是异常。");
                }
                finally { registry.Dispose(); }
            }
        }

        [Test]
        public void RuntimesAreNotPersisted_TheRegionRebuildsThemFromDomainFactsOnly()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = Region(session))
            {
                MachineInstance machine = Machine(session, canMove: false);
                Assert.That(region.DeployMachine(machine.Id, new Vector2(10, 10), machine.Definition.Footprint, out _),
                    Is.EqualTo(RegionMachineDeploymentResult.Bound));

                var first = Registry(session, region);
                Assert.That(first.TryAttach(machine, null, out RegionMachineRuntime before, out _), Is.True);
                Assert.That(before.Context.Tasks.Submit("进行中的工作", WorkPriority.Normal, out _),
                    Is.EqualTo(QueueAdmission.Accepted));
                Assert.That(before.Context.Tasks.WaitingCount, Is.EqualTo(1));

                // 区域释放：运行时整体销毁。注册表里没有任何序列化状态，所以
                // 「重建」就只是再建一次——不需要、也不可能从存档里恢复运行时。
                first.Dispose();

                var rebuilt = Registry(session, region);
                try
                {
                    Assert.That(rebuilt.TryAttach(machine, null, out RegionMachineRuntime after, out _), Is.True);
                    Assert.That(after, Is.Not.SameAs(before));
                    Assert.That(after.Context.Tasks.WaitingCount, Is.Zero,
                        "运行时是派生的：重建得到空队列，不会把上一次的工作带回来。");
                    Assert.That(machine.Deployed, Is.True, "领域事实（已部署）与运行时无关，重建后必须仍然成立。");
                    Assert.That(region.TryGet(machine.Id, out _), Is.True);
                }
                finally { rebuilt.Dispose(); }
            }
        }
    }
}
