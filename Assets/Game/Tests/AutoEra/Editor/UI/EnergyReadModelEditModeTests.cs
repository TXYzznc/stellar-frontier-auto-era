using System.Collections.Generic;
using AutoEra.Application;
using AutoEra.Energy;
using AutoEra.Machines;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using AutoEra.World.Time;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 能源页读取模型（规格 06-能源储存与物流「第一版能源界面」＋ 04-基地中枢/HubEnergy.md）。
    ///
    /// 这一批要修掉的旧行为是「能源页存在但没有任何数据」：页面结构、状态组和控件都在预制体里，
    /// 但没有读模型，因此它是一张死页。这里钉住接上电网之后必须成立的四件事：
    /// <list type="bullet">
    /// <item>三种状态分清：缺会话／缺世界／区域没有电网都是 <c>Unavailable</c>，且**各自给出可辨原因**，
    /// 有设施时才是 <c>Ready</c>——界面不能把「域没接线」和「这次没有内容」说成同一句话；</item>
    /// <item>规模来自**结算真正用的那份快照**（<see cref="RegionEnergyService.Snapshot"/>），
    /// 界面不自己再算一遍功率，否则「界面说 8.2、停机判定说 5.5」迟早出现；</item>
    /// <item>写入口只有燃料设施的充电许可与目标储电比例（<see cref="RegionEnergyFacility.SupportsChargingPolicy"/>），
    /// 环境能源与储能设施必须被拒**并给出原因**，而不是静默失败；</item>
    /// <item>快照是值类型：每次内容变化都必须重建后再发事件，否则页面会停在上一份数组上
    /// （这条规则在存档域与区域域各踩过一次，能源域不能再踩第三次）。</item>
    /// </list>
    /// </summary>
    public sealed class EnergyReadModelEditModeTests
    {
        private static readonly PersistentId FacilityId = new PersistentId(5001);

        private readonly List<GameObject> _hosts = new List<GameObject>();

        private AutoEraApplicationContext _context;
        private AutoEraWorldSession _world;
        private RegionEnergyService _energy;

        [SetUp]
        public void SetUp()
        {
            _context = new AutoEraApplicationContext(new SystemUtcTimeProvider(), new AutoEraWorldSessionFactory());
            Assert.That(_context.TryCreateWorldSession(0L, out _world), Is.True);
            _energy = new RegionEnergyService();
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _hosts.Count; i++)
            {
                if (_hosts[i] != null) Object.DestroyImmediate(_hosts[i]);
            }

            _hosts.Clear();
            _energy = null;
            _context?.Dispose();
            _context = null;
        }

        // ---------------------------------------------------------------- 构造

        /// <summary>
        /// 造一个声明好的能源设施。字段是 private 序列化字段，所以走 `SerializedObject`
        /// ——与编辑器工具写预制体走的是同一条路径（见 RegionEnergyServiceEditModeTests）。
        /// </summary>
        private RegionEnergyFacility Facility(RegionEnergyFacilityKind kind, string name,
            float rated = 0f, float biomass = 0f, float capacity = 0f, float charge = 0f)
        {
            var host = new GameObject(name);
            _hosts.Add(host);
            var facility = host.AddComponent<RegionEnergyFacility>();
            var data = new SerializedObject(facility);
            data.FindProperty("_kind").enumValueIndex = (int)kind;
            if (rated > 0f) data.FindProperty("_ratedPower").floatValue = rated;
            if (biomass > 0f) data.FindProperty("_initialBiomass").floatValue = biomass;
            if (capacity > 0f) data.FindProperty("_capacity").floatValue = capacity;
            data.FindProperty("_initialCharge").floatValue = charge;
            data.ApplyModifiedPropertiesWithoutUndo();
            facility.Initialize(FacilityId);
            return facility;
        }

        private static MachineDefinition Definition() =>
            new MachineDefinition(1001, "夹具载体", 1, 2, 1, 2, 30, true, true, 100d, 1.8d, 2.6d,
                "Machines/WheeledCarrier", 0.2d, 2d);

        private AutoEraUiSession Session() =>
            AutoEraUiSession.ForWorld(_context, _world, regionEnergy: _energy);

        private IEnergyReadModel Model() => EnergyReadModels.Create(Session());

        private static UiDetailField Field(IReadOnlyList<UiDetailField> fields, string label)
        {
            for (int i = 0; i < fields.Count; i++)
            {
                if (fields[i].Label == label) return fields[i];
            }

            Assert.Fail("概要里没有「" + label + "」这一项。");
            return default;
        }

        // ---------------------------------------------------------------- 状态

        [Test]
        public void MissingSession_ReportsUnavailableWithoutFabricatingNumbers()
        {
            using (IEnergyReadModel model = EnergyReadModels.Create(null))
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                Assert.That(model.Snapshot.Reason, Is.Not.Null.And.Not.Empty);
                Assert.That(model.Snapshot.Facilities, Is.Empty, "不可用时不得伪造设施。");
                Assert.That(model.Snapshot.Consumers, Is.Empty, "不可用时不得伪造用电对象。");
                Assert.That(model.Snapshot.Summary, Is.Empty, "不可用时不得伪造供需数字。");
                Assert.That(model.Snapshot.HasSelection, Is.False);
            }
        }

        [Test]
        public void WorldWithoutGrid_SaysTheRegionHasNoFacilities()
        {
            using (IEnergyReadModel model = EnergyReadModels.Create(
                       AutoEraUiSession.ForWorld(_context, _world)))
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                Assert.That(model.Snapshot.Reason, Does.Contain("能源设施"),
                    "世界就绪但区域没有声明设施时，原因必须指向设施本身。");
            }
        }

        [Test]
        public void RegisteredFacilities_MakeThePageReady()
        {
            _energy.Register(Facility(RegionEnergyFacilityKind.FuelGenerator, "生物质发电机", rated: 60f, biomass: 10f));
            _energy.Register(Facility(RegionEnergyFacilityKind.Battery, "蓄电池", capacity: 240f, charge: 120f));

            using (IEnergyReadModel model = Model())
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Ready));
                Assert.That(model.Snapshot.Reason, Is.Null);
                Assert.That(model.Snapshot.Facilities.Count, Is.EqualTo(2));
                Assert.That(model.Snapshot.Facilities[0].Name, Is.EqualTo("生物质发电机"),
                    "设施名取场景对象名，拿不到才退化成类别。");
                Assert.That(model.Snapshot.Facilities[0].Kind, Is.EqualTo("燃料发电"));
                Assert.That(model.Snapshot.Facilities[1].Kind, Is.EqualTo("蓄电"));
                Assert.That(model.Snapshot.Facilities[1].Detail, Does.Contain("120"),
                    "储能设施的明细必须带当前电量，否则玩家看不到它充到哪了。");
            }
        }

        // ---------------------------------------------------------------- 供需概要

        [Test]
        public void SummaryComesFromTheSettlementSnapshotNotARecomputation()
        {
            _energy.Register(Facility(RegionEnergyFacilityKind.EnvironmentGenerator, "太阳能板", rated: 15f));

            using (IEnergyReadModel model = Model())
            {
                _energy.Tick(0L, 60f); // 世界时间 0 是日出
                model.Refresh();

                Assert.That(Field(model.Snapshot.Summary, "当前发电").Value, Is.EqualTo("15 功率"),
                    "白天的环境能源出力必须与结算快照一致。");
                Assert.That(Field(model.Snapshot.Summary, "当前用电").Value, Is.EqualTo("0 功率"));
                Assert.That(Field(model.Snapshot.Summary, "净功率").Value, Is.EqualTo("+15 功率"));

                _energy.Tick(17L * 60L * 1000L, 60f); // 第 17 分钟已经入夜
                model.Refresh();

                Assert.That(Field(model.Snapshot.Summary, "当前发电").Value, Is.EqualTo("0 功率"),
                    "入夜后环境能源出力归零——界面读的必须是结算的那一次。");
                Assert.That(Field(model.Snapshot.Summary, "日照").Value, Does.Contain("无日照"));
            }
        }

        [Test]
        public void EstimateIsLabelledAsAnEstimateThatChangesWithLoad()
        {
            _energy.Register(Facility(RegionEnergyFacilityKind.Battery, "蓄电池", capacity: 240f, charge: 120f));
            MachineInstance machine = _world.Machines.Create(Definition());
            _world.Machines.Deploy(machine.Id);
            _energy.TrackMachine(machine);

            using (IEnergyReadModel model = Model())
            {
                _energy.Tick(0L, 1f);
                model.Refresh();

                Assert.That(Field(model.Snapshot.Summary, "充满／耗尽估算").Value, Does.Contain("估算"),
                    "规格要求估算必须标注会随负载、昼夜和设施状态变化，不能伪装成实时测量。");
                Assert.That(Field(model.Snapshot.Summary, "缺电停机设备").Value, Does.Contain("台"));
            }
        }

        [Test]
        public void WithoutStorageTheSummarySaysSoInsteadOfShowingAZero()
        {
            _energy.Register(Facility(RegionEnergyFacilityKind.EnvironmentGenerator, "太阳能板", rated: 15f));

            using (IEnergyReadModel model = Model())
            {
                Assert.That(Field(model.Snapshot.Summary, "当前储电").Value, Does.Contain("没有蓄电设施"));
                Assert.That(Field(model.Snapshot.Summary, "充满／耗尽估算").Value, Does.Contain("没有蓄电设施"));
            }
        }

        // ---------------------------------------------------------------- 用电对象

        [Test]
        public void ConsumersListOnlyDeployedMachines()
        {
            _energy.Register(Facility(RegionEnergyFacilityKind.FuelGenerator, "生物质发电机", rated: 60f, biomass: 10f));
            MachineInstance libraryOnly = _world.Machines.Create(Definition());

            using (IEnergyReadModel model = Model())
            {
                Assert.That(model.Snapshot.Consumers, Is.Empty, "库中机器不占区域功率，也不该出现在用电对象里。");

                _world.Machines.Deploy(libraryOnly.Id);
                _energy.Reconcile(_world.Machines);
                model.Refresh();

                Assert.That(model.Snapshot.Consumers.Count, Is.EqualTo(1));
                Assert.That(model.Snapshot.Consumers[0].Group, Is.EqualTo("机器"));
                Assert.That(model.Snapshot.Consumers[0].Priority, Is.EqualTo("普通生产"),
                    "第一版优先级只读，且取的是区域电网的默认优先级。");
                Assert.That(model.Snapshot.Consumers[0].State, Is.Not.Null.And.Not.Empty);
            }
        }

        // ---------------------------------------------------------------- 选中与写入

        [Test]
        public void SelectionPublishesWhetherThatFacilitySupportsChargingPolicy()
        {
            _energy.Register(Facility(RegionEnergyFacilityKind.FuelGenerator, "生物质发电机", rated: 60f, biomass: 10f));
            _energy.Register(Facility(RegionEnergyFacilityKind.Battery, "蓄电池", capacity: 240f, charge: 0f));

            using (IEnergyReadModel model = Model())
            {
                Assert.That(model.Snapshot.HasSelection, Is.False, "刚打开时不该替玩家选中一台设施。");

                Assert.That(model.Select(0), Is.True);
                Assert.That(model.Snapshot.HasSelection, Is.True);
                Assert.That(model.Snapshot.SelectedFacility, Is.Zero);
                Assert.That(model.Snapshot.Selected.SupportsChargingPolicy, Is.True, "燃料设施开放充电策略。");

                Assert.That(model.Select(1), Is.True);
                Assert.That(model.Snapshot.Selected.SupportsChargingPolicy, Is.False, "储能设施没有充电策略设置。");

                Assert.That(model.Select(9), Is.False, "越界选中必须被拒，不能悄悄改选中项。");
                Assert.That(model.Snapshot.SelectedFacility, Is.EqualTo(1));

                model.ClearSelection();
                Assert.That(model.Snapshot.HasSelection, Is.False);
            }
        }

        [Test]
        public void FuelFacilityAcceptsChargingPolicyWrites()
        {
            RegionEnergyFacility fuel = Facility(RegionEnergyFacilityKind.FuelGenerator, "生物质发电机",
                rated: 60f, biomass: 10f);
            _energy.Register(fuel);

            using (IEnergyReadModel model = Model())
            {
                model.Select(0);

                Assert.That(model.SetChargingAllowed(true, out string allowReason), Is.True, allowReason);
                Assert.That(fuel.Generator.AllowsCharging, Is.True, "写入必须真的落到领域的发电机上。");
                Assert.That(model.Snapshot.Selected.ChargingAllowed, Is.True,
                    "写入后快照必须重建，否则控件会弹回旧值。");

                Assert.That(model.SetChargeTargetRatio(0.6f, out string ratioReason), Is.True, ratioReason);
                Assert.That(fuel.Generator.ChargeTargetRatio, Is.EqualTo(0.6f).Within(1e-4f));
                Assert.That(model.Snapshot.Selected.ChargeTargetRatio, Is.EqualTo(0.6f).Within(1e-4f));
            }
        }

        [Test]
        public void ChargingPolicyWritesAreRefusedWithAReasonWhenTheyDoNotApply()
        {
            _energy.Register(Facility(RegionEnergyFacilityKind.EnvironmentGenerator, "太阳能板", rated: 15f));
            _energy.Register(Facility(RegionEnergyFacilityKind.Battery, "蓄电池", capacity: 240f, charge: 0f));

            using (IEnergyReadModel model = Model())
            {
                Assert.That(model.SetChargingAllowed(true, out string noSelection), Is.False,
                    "没有选中设施时写入必须被拒。");
                Assert.That(noSelection, Is.Not.Null.And.Not.Empty, "被拒必须给出可展示的原因，不能静默失败。");

                model.Select(0); // 环境能源
                Assert.That(model.SetChargeTargetRatio(0.5f, out string environment), Is.False);
                Assert.That(environment, Does.Contain("燃料"), "原因必须说清只有燃料设施开放这项设置。");

                model.Select(1); // 储能
                Assert.That(model.SetChargingAllowed(true, out string storage), Is.False);
                Assert.That(storage, Does.Contain("燃料"));
            }
        }

        [Test]
        public void ChargeTargetRatioIsClampedToTheValidRange()
        {
            RegionEnergyFacility fuel = Facility(RegionEnergyFacilityKind.FuelGenerator, "生物质发电机",
                rated: 60f, biomass: 10f);
            _energy.Register(fuel);

            using (IEnergyReadModel model = Model())
            {
                model.Select(0);

                Assert.That(model.SetChargeTargetRatio(1.5f, out _), Is.True);
                Assert.That(fuel.Generator.ChargeTargetRatio, Is.EqualTo(1f).Within(1e-4f));

                Assert.That(model.SetChargeTargetRatio(-0.5f, out _), Is.True);
                Assert.That(fuel.Generator.ChargeTargetRatio, Is.Zero.Within(1e-4f));

                Assert.That(model.SetChargeTargetRatio(float.NaN, out string nan), Is.False);
                Assert.That(nan, Is.Not.Null.And.Not.Empty);
            }
        }

        // ---------------------------------------------------------------- 订阅

        [Test]
        public void RefreshPublishesANewSnapshotAndNotifies()
        {
            _energy.Register(Facility(RegionEnergyFacilityKind.EnvironmentGenerator, "太阳能板", rated: 15f));

            using (IEnergyReadModel model = Model())
            {
                int notifications = 0;
                model.Changed += () => notifications++;

                _energy.Tick(0L, 60f);
                model.Refresh();
                Assert.That(notifications, Is.EqualTo(1));

                _energy.Tick(17L * 60L * 1000L, 60f);
                model.Refresh();

                Assert.That(notifications, Is.EqualTo(2), "每次刷新都要通知，页面才可能跟着结算走。");
                Assert.That(Field(model.Snapshot.Summary, "当前发电").Value, Is.EqualTo("0 功率"),
                    "通知之外还必须换掉快照本身——只发事件不重建快照是这块踩过的老坑。");
            }
        }

        [Test]
        public void DisposeStopsNotifying()
        {
            _energy.Register(Facility(RegionEnergyFacilityKind.EnvironmentGenerator, "太阳能板", rated: 15f));

            IEnergyReadModel model = Model();
            int notifications = 0;
            model.Changed += () => notifications++;
            model.Dispose();

            model.Refresh();

            Assert.That(notifications, Is.Zero, "释放之后不得再回调已销毁的界面。");
        }

        [Test]
        public void SelectDoesNotRefireForTheSameFacility()
        {
            _energy.Register(Facility(RegionEnergyFacilityKind.EnvironmentGenerator, "太阳能板", rated: 15f));
            _energy.Register(Facility(RegionEnergyFacilityKind.Battery, "蓄电池", capacity: 240f, charge: 0f));

            using (IEnergyReadModel model = Model())
            {
                model.Select(1);

                int notifications = 0;
                model.Changed += () => notifications++;
                Assert.That(model.Select(1), Is.True);
                Assert.That(notifications, Is.Zero,
                    "重复选中同一台设施不该重绘整页——那会让每帧刷新变成抖动源。");
            }
        }
    }
}
