using AutoEra.Application;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Time;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 算法域读取模型的状态契约。
    ///
    /// 算法域的服务层已经实现（有 <c>AlgorithmInstanceService</c> 与集成测试），但生产运行路径
    /// 还没有创建机器执行上下文与算法实例服务，所以这三种情形都必须落到 Unavailable，
    /// 并且**各自给出不同的、可展示的原因**——界面据此解释自己，而不是显示一片空白。
    ///
    /// 接线完成后这些断言仍然成立（世界已就绪那条会变成真实数据分支），
    /// 因此它们同时是替换工厂实现时的回归围栏。
    /// </summary>
    public sealed class AlgorithmReadModelEditModeTests
    {
        [Test]
        public void MissingSession_ReportsUnavailableWithReason()
        {
            using (IAlgorithmReadModel model = AlgorithmReadModels.Create(null))
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                Assert.That(model.Snapshot.UnavailableReason, Is.Not.Null.And.Not.Empty);
                Assert.That(model.Snapshot.Count, Is.Zero, "不可用时不得伪造模板。");
            }
        }

        [Test]
        public void SessionWithoutWorld_SaysAlgorithmBelongsToAWorld()
        {
            using (var context = new AutoEraApplicationContext(new SystemUtcTimeProvider(), new AutoEraWorldSessionFactory()))
            using (IAlgorithmReadModel model = AlgorithmReadModels.Create(AutoEraUiSession.ForApplication(context)))
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                Assert.That(model.Snapshot.UnavailableReason, Does.Contain("区域").Or.Contain("世界"),
                    "世界外的原因要说清「算法属于某个世界」，而不是笼统报不可用。");
            }
        }

        [Test]
        public void WorldReady_StillUnavailableBecauseDomainIsNotWired()
        {
            var context = new AutoEraApplicationContext(new SystemUtcTimeProvider(), new AutoEraWorldSessionFactory());
            using (context)
            using (IAlgorithmReadModel model = CreateWithWorld(context))
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                Assert.That(model.Snapshot.UnavailableReason, Is.EqualTo(AlgorithmReadModels.NotWiredReason),
                    "世界就绪时原因必须指向真正的缺口：算法域的服务没有创建者。");
                Assert.That(model.Snapshot.UnavailableReason, Does.Contain("执行上下文"),
                    "原因要具体到缺什么，方便排查的人直接找到接线点。");
            }
        }

        [Test]
        public void UnavailableModel_SelectionIsNoOpAndDisposeIsSafe()
        {
            using (IAlgorithmReadModel model = AlgorithmReadModels.Create(null))
            {
                int notifications = 0;
                model.Changed += _ => notifications++;

                Assert.That(model.Select(1), Is.False, "没有数据时选中必须失败而不是假装成功。");
                model.ClearSelection();
                model.Refresh();
                model.Dispose();

                Assert.That(model.SelectedIndex, Is.EqualTo(-1));
                Assert.That(notifications, Is.Zero);
            }
        }

        private static IAlgorithmReadModel CreateWithWorld(AutoEraApplicationContext context)
        {
            Assert.That(context.TryCreateWorldSession(0L, out AutoEraWorldSession world), Is.True);
            return AlgorithmReadModels.Create(AutoEraUiSession.ForWorld(context, world));
        }
    }
}
