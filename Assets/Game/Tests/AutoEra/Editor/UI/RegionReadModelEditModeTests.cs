using System;
using AutoEra.Application;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using AutoEra.World.Time;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 区域域读取模型的数据流：三种不可用原因、对象列表与机器分组、选中详情、变化订阅与退订。
    ///
    /// 用真实 <see cref="InitialRegion"/>（不需要场景），因此顺带覆盖了「区域是场景级对象、
    /// 随会话一起交给界面」这条链。
    /// </summary>
    public sealed class RegionReadModelEditModeTests
    {
        private AutoEraApplicationContext _context;
        private AutoEraWorldSession _world;
        private InitialRegion _region;

        [SetUp]
        public void SetUp()
        {
            _context = new AutoEraApplicationContext(new SystemUtcTimeProvider(), new AutoEraWorldSessionFactory());
            Assert.That(_context.TryCreateWorldSession(0L, out _world), Is.True);
            _region = new InitialRegion(_world, new Rect(-50, -50, 100, 100));
        }

        [TearDown]
        public void TearDown()
        {
            _region?.Dispose();
            _region = null;
            _context?.Dispose();
            _context = null;
        }

        [Test]
        public void MissingSession_ReportsUnavailableWithReason()
        {
            using (IRegionReadModel model = RegionReadModels.Create(null))
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                Assert.That(model.Snapshot.UnavailableReason, Is.Not.Null.And.Not.Empty);
                Assert.That(model.Snapshot.Count, Is.Zero, "不可用时不得伪造区域对象。");
            }
        }

        [Test]
        public void SessionWithoutRegion_SaysRegionIsNotLoaded()
        {
            using (IRegionReadModel model = RegionReadModels.Create(AutoEraUiSession.ForWorld(_context, _world)))
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                Assert.That(model.Snapshot.UnavailableReason, Does.Contain("区域"),
                    "世界已就绪但区域没加载时，原因必须指向区域本身。");
            }
        }

        [Test]
        public void EmptyRegion_ReportsEmptyStateNotUnavailable()
        {
            using (IRegionReadModel model = Model())
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Empty),
                    "区域存在但没有对象时是 Empty——空区域是正常状态，不是不可用。");
                Assert.That(model.Snapshot.Count, Is.Zero);
            }
        }

        [Test]
        public void RegisteredObjects_AppearAndAreSplitIntoMachinesAndSites()
        {
            using (IRegionReadModel model = Model())
            {
                int notifications = 0;
                model.Changed += _ => notifications++;

                _region.Register(PersistentObjectKind.Building, "温室", new Vector2(4, 4), new Vector2(4, 4));
                _region.Register(PersistentObjectKind.ResourcePoint, "铁矿", new Vector2(10, 10), new Vector2(3, 3));

                Assert.That(notifications, Is.GreaterThan(0), "注册对象必须通知读模型。");
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Ready));
                Assert.That(model.Snapshot.Count, Is.EqualTo(2));
                Assert.That(model.Snapshot.SiteCount, Is.EqualTo(2));
                Assert.That(model.Snapshot.MachineCount, Is.Zero, "没有部署机器时机器分组应为空。");
            }
        }

        [Test]
        public void ObjectStateChange_PropagatesWithoutReRegistering()
        {
            RegionObject site = _region.Register(PersistentObjectKind.ResourcePoint, "铁矿", new Vector2(10, 10), new Vector2(3, 3));

            using (IRegionReadModel model = Model())
            {
                site.SetPublicState("开采中", 42);

                Assert.That(model.Snapshot.Count, Is.EqualTo(1), "状态变化不得让对象重复出现在列表里。");
                Assert.That(model.Snapshot.Sites[0].State, Does.Contain("42"),
                    "公开资源量必须出现在列表行上，否则资源页看不到真实数量。");
            }
        }

        [Test]
        public void Selection_PublishesDetailWithoutRebuildingList()
        {
            RegionObject site = _region.Register(PersistentObjectKind.Building, "泵站", new Vector2(2, 2), new Vector2(2, 2));

            using (IRegionReadModel model = Model())
            {
                RegionDomainSection? lastSection = null;
                model.Changed += section => lastSection = section;

                Assert.That(_region.Select(site.Id, inputBlocked: false), Is.True);

                Assert.That(lastSection, Is.EqualTo(RegionDomainSection.Selection),
                    "选中只影响选中区，不应被当成列表变化。");
                Assert.That(model.Snapshot.HasSelection, Is.True);
                Assert.That(Contains(model.Snapshot.Detail, "泵站"), Is.True, "详情必须读到选中对象。");
                Assert.That(Contains(model.Snapshot.Detail, "建筑"), Is.True, "详情必须说明对象类别。");
            }
        }

        [Test]
        public void Dispose_StopsNotifyingAndUnsubscribes()
        {
            RegionObject site = _region.Register(PersistentObjectKind.Building, "泵站", new Vector2(2, 2), new Vector2(2, 2));

            IRegionReadModel model = Model();
            int notifications = 0;
            model.Changed += _ => notifications++;
            model.Dispose();
            int before = notifications;

            site.SetPublicState("运行", 1);
            _region.Register(PersistentObjectKind.ResourcePoint, "另一个", new Vector2(20, 20), new Vector2(2, 2));

            Assert.That(notifications, Is.EqualTo(before),
                "释放后不得再通知——界面可能已经销毁，对象本身还在世界里活着。");
        }

        [Test]
        public void DescribeObject_WorksWithoutSelection_ForPickerPreview()
        {
            RegionObject site = _region.Register(PersistentObjectKind.ResourcePoint, "铁矿", new Vector2(6, 6), new Vector2(3, 3));
            site.SetPublicState("开采中", 12);

            using (IRegionReadModel model = Model())
            {
                // 关键：没有选中任何对象时也要能拿详情——选择器要预览一个还没被选中的候选。
                Assert.That(model.Snapshot.HasSelection, Is.False);

                System.Collections.Generic.IReadOnlyList<UiDetailField> described = model.DescribeObject(site.Id);
                Assert.That(Contains(described, "铁矿"), Is.True, "按 Id 取详情必须与选中详情用同一份字段。");
                Assert.That(Contains(described, "12"), Is.True, "公开资源量必须在详情里。");

                Assert.That(model.DescribeObject(new PersistentId(99999)), Is.Empty,
                    "不存在的对象返回空，而不是抛异常或编造字段。");
            }
        }

        private IRegionReadModel Model() => RegionReadModels.Create(AutoEraUiSession.ForWorld(_context, _world, _region));
        private static bool Contains(System.Collections.Generic.IReadOnlyList<UiDetailField> fields, string fragment)
        {
            for (int i = 0; i < fields.Count; i++)
            {
                if ((fields[i].Label != null && fields[i].Label.Contains(fragment))
                    || (fields[i].Value != null && fields[i].Value.Contains(fragment)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
