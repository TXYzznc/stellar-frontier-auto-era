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
    /// 算法域已接入世界运行路径：<see cref="AutoEraWorldSession.AlgorithmTemplates"/> 持有世界级模板库，
    /// <see cref="AlgorithmReadModels.Create"/> 在世界就绪时返回真实读模型（模板列表 + 详情）。
    /// 世界外／无会话仍返回 Unavailable，并各自给出可展示原因。
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
        public void WorldReady_ReturnsAvailableReadModel()
        {
            var context = new AutoEraApplicationContext(new SystemUtcTimeProvider(), new AutoEraWorldSessionFactory());
            using (context)
            using (IAlgorithmReadModel model = CreateWithWorld(context))
            {
                // 世界已就绪且模板库已接线 → 不再是 Unavailable。模板库尚未录入五模板 → Empty。
                Assert.That(model.Snapshot.State, Is.Not.EqualTo(UiDataState.Unavailable),
                    "世界就绪后读模型必须返回真实数据，不能再报「未接入」。");
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Empty),
                    "未录入模板时是空态，而不是伪造模板或报不可用。");
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
