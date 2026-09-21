using System.Collections.Generic;
using AutoEra.Energy;
using AutoEra.Machines;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 区域电网的组合与推进（规格 06-能源储存与物流「区域级电网」）。
    ///
    /// 这一批要修掉的旧行为是「电网只是个算得对的计算器，没有任何人用它」：
    /// 结算器与机器负载都在，却没有任何地方建过一张电网。这里钉住的是**谁拥有电网、谁参与、
    /// 结论怎么落回机器**：
    /// <list type="bullet">
    /// <item>电网**由区域持有**，不是全局单例——否则「前线电网独立运行」立刻不成立；</item>
    /// <item>参与方是「场景声明过的设施 ＋ **已部署**的机器」：库中机器不占区域功率；</item>
    /// <item>**没有设施就不建电网**：接线不该顺手把还没接内容的世界改成一片漆黑；</item>
    /// <item>对账而不是全量重建：队列顺序是同级停机顺序的判据，重建会让它变成时间的函数。</item>
    /// </list>
    /// </summary>
    public sealed class RegionEnergyServiceEditModeTests
    {
        private static readonly PersistentId FacilityId = new PersistentId(5001);

        private readonly List<GameObject> _hosts = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _hosts.Count; i++)
            {
                if (_hosts[i] != null) Object.DestroyImmediate(_hosts[i]);
            }

            _hosts.Clear();
        }

        /// <summary>
        /// 造一个声明好的能源设施。字段是 private 序列化字段，所以走 `SerializedObject`
        /// ——这与编辑器工具写预制体走的是同一条路径。
        /// </summary>
        private RegionEnergyFacility Facility(RegionEnergyFacilityKind kind, float rated = 0f,
            float biomass = 0f, float capacity = 0f, float charge = 0f)
        {
            var host = new GameObject("EnergyFacility");
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

        private static MachineInstance Deployed(AutoEraWorldSession session)
        {
            MachineInstance machine = session.Machines.Create(Definition());
            session.Machines.Deploy(machine.Id);
            return machine;
        }

        // ---------------------------------------------------------------- 设施与建立

        [Test]
        public void FacilityDefaultsMatchTheDesignValues()
        {
            var host = new GameObject("DefaultFacility");
            _hosts.Add(host);
            var facility = host.AddComponent<RegionEnergyFacility>();

            Assert.That(facility.Kind, Is.EqualTo(RegionEnergyFacilityKind.EnvironmentGenerator),
                "不配参数时应当是「初始太阳能发电器」。");

            facility.Initialize(FacilityId);

            Assert.That(facility.IsInitialized, Is.True);
            Assert.That(facility.ObjectId, Is.EqualTo(FacilityId));
            Assert.That(facility.Generator.RatedPower, Is.EqualTo(FirstVersionEnergy.SolarDaylightPower));
            Assert.That(facility.Storage, Is.Null);
        }

        [Test]
        public void FuelAndBatteryFacilitiesUseTheDesignNumbers()
        {
            RegionEnergyFacility fuel = Facility(RegionEnergyFacilityKind.FuelGenerator, rated: 60f, biomass: 10f);
            RegionEnergyFacility battery = Facility(RegionEnergyFacilityKind.Battery, capacity: 240f, charge: 0f);

            Assert.That(fuel.Generator.RatedPower, Is.EqualTo(FirstVersionEnergy.BiomassGeneratorRatedPower));
            Assert.That(fuel.RemainingBiomass, Is.EqualTo(10f).Within(1e-4f));
            Assert.That(battery.Storage.Capacity, Is.EqualTo(FirstVersionEnergy.BatteryCapacity));
            Assert.That(battery.Generator, Is.Null, "储能设施不是发电设施。");
        }

        [Test]
        public void WithoutFacilitiesTheRegionHasNoGridAtAll()
        {
            var service = new RegionEnergyService();

            Assert.That(service.HasSupply, Is.False);
            Assert.That(service.FacilityCount, Is.Zero);

            EnergyGridSnapshot snapshot = service.Tick(0L, 1f);

            Assert.That(snapshot.GeneratedPower, Is.Zero);
            Assert.That(service.ApplySupply(), Is.Zero, "没有电网就不该去改任何机器的供电状态。");
        }

        [Test]
        public void RegisteringFacilitiesFeedsTheGrid()
        {
            var service = new RegionEnergyService();
            service.Register(Facility(RegionEnergyFacilityKind.FuelGenerator, rated: 60f, biomass: 10f));
            service.Register(Facility(RegionEnergyFacilityKind.Battery, capacity: 240f, charge: 0f));

            Assert.That(service.HasSupply, Is.True);
            Assert.That(service.FacilityCount, Is.EqualTo(2));
            Assert.That(service.Grid.Generators.Count, Is.EqualTo(1));
            Assert.That(service.Grid.Storages.Count, Is.EqualTo(1));
        }

        [Test]
        public void RegisteringTheSameFacilityTwiceIsIdempotent()
        {
            var service = new RegionEnergyService();
            RegionEnergyFacility facility = Facility(RegionEnergyFacilityKind.FuelGenerator, rated: 60f, biomass: 10f);

            service.Register(facility);
            service.Register(facility);

            Assert.That(service.FacilityCount, Is.EqualTo(1), "重复登记不该让同一台设施算两次功率。");
        }

        [Test]
        public void FacilityInitializationIsIdempotentSoFuelIsNotReset()
        {
            using (var session = new AutoAraSession())
            {
                RegionEnergyFacility facility = Facility(RegionEnergyFacilityKind.FuelGenerator, rated: 60f, biomass: 10f);
                var service = new RegionEnergyService();
                service.Register(facility);
                service.TrackMachine(Deployed(session.Session));

                service.Tick(0L, 60f); // 一分钟额定输出 → 消耗 60 电量

                float afterBurn = facility.RemainingBiomass;
                facility.Initialize(FacilityId); // 实体重新显示时可能再调一次

                Assert.That(facility.RemainingBiomass, Is.EqualTo(afterBurn).Within(1e-4f),
                    "再次初始化不该把烧掉的燃料加回来。");
            }
        }

        // ---------------------------------------------------------------- 参与方

        [Test]
        public void OnlyDeployedMachinesJoinTheGrid()
        {
            using (var session = new AutoAraSession())
            {
                var service = new RegionEnergyService();
                service.Register(Facility(RegionEnergyFacilityKind.FuelGenerator, rated: 60f, biomass: 10f));

                MachineInstance libraryOnly = session.Session.Machines.Create(Definition());
                Assert.That(service.TrackMachine(libraryOnly), Is.False, "库中机器不占区域功率。");
                Assert.That(service.MachineCount, Is.Zero);

                session.Session.Machines.Deploy(libraryOnly.Id);
                Assert.That(service.TrackMachine(libraryOnly), Is.True);
                Assert.That(service.Tracks(libraryOnly.Id), Is.True);
                Assert.That(service.MachineCount, Is.EqualTo(1));
            }
        }

        [Test]
        public void ReconcileBringsInNewMachinesAndDropsRecoveredOnes()
        {
            using (var session = new AutoAraSession())
            {
                var service = new RegionEnergyService();
                service.Register(Facility(RegionEnergyFacilityKind.FuelGenerator, rated: 60f, biomass: 10f));
                MachineInstance machine = session.Session.Machines.Create(Definition());

                service.Reconcile(session.Session.Machines);
                Assert.That(service.MachineCount, Is.Zero, "还没部署的机器不参与。");

                session.Session.Machines.Deploy(machine.Id);
                service.Reconcile(session.Session.Machines);
                Assert.That(service.MachineCount, Is.EqualTo(1), "部署之后应当被接进来。");

                session.Session.Machines.RecoverToLibrary(machine.Id, ManagementOrigin.Field);
                service.Reconcile(session.Session.Machines);
                Assert.That(service.MachineCount, Is.Zero, "回收之后应当被摘掉。");
            }
        }

        [Test]
        public void MachinePriorityUsesTheDocumentedDefault()
        {
            Assert.That(RegionEnergyService.DefaultMachinePriority, Is.EqualTo(PowerPriority.Production),
                "规格说系统提供默认优先级但没逐个列出；实现取「普通生产」并把它放在唯一一处。");
        }

        // ---------------------------------------------------------------- 结算

        [Test]
        public void DaylightDrivesTheSolarOutput()
        {
            var service = new RegionEnergyService();
            service.Register(Facility(RegionEnergyFacilityKind.EnvironmentGenerator, rated: 15f));

            // 世界时间 0 是日出，第 17 分钟已经入夜。
            Assert.That(service.Tick(0L, 60f).GeneratedPower, Is.EqualTo(15f).Within(1e-4f));
            Assert.That(service.Tick(17L * 60L * 1000L, 60f).GeneratedPower, Is.Zero.Within(1e-4f));
        }

        [Test]
        public void FuelIsConsumedAtTheActualOutput()
        {
            using (var session = new AutoAraSession())
            {
                var service = new RegionEnergyService();
                RegionEnergyFacility fuel = Facility(RegionEnergyFacilityKind.FuelGenerator, rated: 60f, biomass: 10f);
                service.Register(fuel);

                // 没有负载：待命，不烧燃料。
                service.Tick(0L, 60f);
                Assert.That(fuel.RemainingBiomass, Is.EqualTo(10f).Within(1e-3f), "开着但没有需求时是待命。");

                // 接入一台机器后按实际负载消耗。
                service.TrackMachine(Deployed(session.Session));
                service.Tick(0L, 60f);
                Assert.That(fuel.RemainingBiomass, Is.LessThan(10f));
            }
        }

        [Test]
        public void TheGridWritesItsConclusionBackToTheMachine()
        {
            using (var session = new AutoAraSession())
            {
                var service = new RegionEnergyService();
                service.Register(Facility(RegionEnergyFacilityKind.EnvironmentGenerator, rated: 15f));
                MachineInstance machine = Deployed(session.Session);
                service.TrackMachine(machine);

                Assert.That(machine.SupplyAvailable, Is.False);
                service.Tick(0L, 1f);
                int changed = service.ApplySupply();

                Assert.That(changed, Is.EqualTo(1));
                Assert.That(machine.SupplyAvailable, Is.True, "白天 15 功率足够带一台待机机器。");
                Assert.That(machine.Powered, Is.True);

                // 结论没变时不重复广播。
                service.Tick(0L, 1f);
                Assert.That(service.ApplySupply(), Is.Zero);
            }
        }

        [Test]
        public void AnEmptyBatteryIsStillAGrid_SoAShortfallReallyStopsMachines()
        {
            using (var session = new AutoAraSession())
            {
                var service = new RegionEnergyService();
                service.Register(Facility(RegionEnergyFacilityKind.Battery, capacity: 240f, charge: 0f));
                MachineInstance machine = Deployed(session.Session);
                service.TrackMachine(machine);
                machine.UpdateSupply(true); // 先给上电，才能观察到「被停机」

                EnergyGridSnapshot snapshot = service.Tick(0L, 1f);
                int changed = service.ApplySupply();

                Assert.That(service.HasSupply, Is.True, "储能设施也算供电能力（它只是没有电）。");
                Assert.That(snapshot.HasShortfall, Is.True);
                Assert.That(changed, Is.EqualTo(1));
                Assert.That(machine.SupplyAvailable, Is.False, "缺电停机必须真的落到机器上。");
            }
        }

        /// <summary>测试用的世界会话包装：统一在断言结束后释放，避免每个用例各写一遍 using。</summary>
        private sealed class AutoAraSession : System.IDisposable
        {
            public AutoAraSession()
            {
                Session = new AutoEraWorldSessionFactory().Create(0);
            }

            public AutoEraWorldSession Session { get; }

            public void Dispose() => Session?.Dispose();
        }
    }
}
