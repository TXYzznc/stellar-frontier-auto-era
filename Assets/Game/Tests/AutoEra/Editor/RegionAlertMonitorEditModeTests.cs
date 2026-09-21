using System.Collections.Generic;
using AutoEra.Alerts;
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
    /// 警报的判据来自**领域当前的真值**，不是「谁发过一条消息」：
    /// 每个节拍读一遍机器与设施，只在跨越时让账本变化。
    ///
    /// 这里钉住三件事：
    /// <list type="bullet">
    /// <item>缺电停机用的是「电网因缺电把它停了」而不是「它现在没电」——
    ///       后者在刚部署、还没轮到结算时也成立，会把「还没接上」报成一次事故；</item>
    /// <item>条件恢复时警报自己转历史（没有手动清除按钮，玩家要做的是解决问题）；</item>
    /// <item>每帧调用是零写入：持续存在的问题不会每帧通知界面。</item>
    /// </list>
    /// </summary>
    public sealed class RegionAlertMonitorEditModeTests
    {
        private static readonly PersistentId FacilityId = new PersistentId(7001);

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

        private RegionEnergyFacility Facility(RegionEnergyFacilityKind kind, float rated = 0f, float biomass = 0f,
            float capacity = 0f, float charge = 0f)
        {
            var host = new GameObject("EnergyFacility");
            _hosts.Add(host);
            var facility = host.AddComponent<RegionEnergyFacility>();
            var data = new SerializedObject(facility);
            data.FindProperty("_kind").enumValueIndex = (int)kind;
            data.FindProperty("_ratedPower").floatValue = rated;
            data.FindProperty("_initialBiomass").floatValue = biomass;
            data.FindProperty("_capacity").floatValue = capacity;
            data.FindProperty("_initialCharge").floatValue = charge;
            data.ApplyModifiedPropertiesWithoutUndo();
            facility.Initialize(FacilityId);
            return facility;
        }

        private static MachineDefinition Definition() =>
            new MachineDefinition(1001, "警报验证机", 1, 2, 1, 2, 30, true, true, 100d, 1.8d, 2.6d,
                "Machines/WheeledCarrier", 0.2d, 2d);

        private static MachineInstance Deployed(AutoEraWorldSession session)
        {
            MachineInstance machine = session.Machines.Create(Definition());
            session.Machines.Deploy(machine.Id);
            return machine;
        }

        // ---------------------------------------------------------------- 缺电停机

        [Test]
        public void ShortageRaisesAnAlert_AndRecoveryResolvesIt()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                var energy = new RegionEnergyService();
                // 没燃料的燃料发电机：算供电能力，但发不出功率 → 机器被缺电停机。
                // 用「补上燃料」来让它真的恢复，而不是在测试里手改停机标志——
                // 那样测的就变成了测试自己，而不是监视器读到的真值。
                RegionEnergyFacility fuel = Facility(RegionEnergyFacilityKind.FuelGenerator, rated: 60f, biomass: 0f);
                energy.Register(fuel);
                MachineInstance machine = Deployed(session);
                energy.TrackMachine(machine);
                machine.UpdateSupply(true);
                energy.Tick(0L, 1f);
                energy.ApplySupply();

                Assert.That(machine.SupplyAvailable, Is.False, "没有燃料时机器应当被缺电停机。");

                var alerts = new AutoEraAlertService();
                var monitor = new RegionAlertMonitor(alerts);
                var transitions = new List<AlertTransition>();

                monitor.Capture(session.Machines, energy, 1000L, transitions);

                Assert.That(transitions.Count, Is.EqualTo(2), "缺电停机与燃料耗尽各是一条真实的问题。");
                Assert.That(transitions.Exists(t => t.Kind == AlertKind.EnergyShortage
                    && t.Source == machine.Id && t.Raised), Is.True);
                Assert.That(transitions.Exists(t => t.Kind == AlertKind.FuelExhausted), Is.True);
                Assert.That(alerts.ActiveCount, Is.EqualTo(2));

                // 再读几遍：问题还在，账本不该有任何变化。
                for (long t = 2000L; t <= 5000L; t += 1000L)
                {
                    transitions.Clear();
                    monitor.Capture(session.Machines, energy, t, transitions);
                    Assert.That(transitions, Is.Empty, "持续缺电不是一次次新的发生。");
                }

                // 补上燃料 → 电网重新供电 → 两条警报都该自己转历史。
                fuel.Generator.FuelEnergyAvailable = FirstVersionEnergy.BiomassToEnergy(1f);
                energy.Tick(0L, 1f);
                energy.ApplySupply();
                Assert.That(machine.SupplyAvailable, Is.True, "补上燃料后机器应当重新拿到供电。");

                transitions.Clear();
                monitor.Capture(session.Machines, energy, 6000L, transitions);

                Assert.That(transitions.Count, Is.EqualTo(2));
                Assert.That(transitions.Exists(t => t.Kind == AlertKind.EnergyShortage && !t.Raised), Is.True);
                Assert.That(transitions.Exists(t => t.Kind == AlertKind.FuelExhausted && !t.Raised), Is.True);
                Assert.That(alerts.ActiveCount, Is.Zero);
            }
        }

        [Test]
        public void AMachineThatWasNeverPowered_IsNotReportedAsAnIncident()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                var energy = new RegionEnergyService();
                energy.Register(Facility(RegionEnergyFacilityKind.EnvironmentGenerator, rated: 15f));
                MachineInstance machine = Deployed(session);
                energy.TrackMachine(machine);
                // 故意不 Tick：机器还没被结算过，SupplyAvailable 是 false，
                // 但它并没有「因为缺电被停机」——警报不得把「还没接上」报成事故。

                var alerts = new AutoEraAlertService();
                var monitor = new RegionAlertMonitor(alerts);

                monitor.Capture(session.Machines, energy, 1000L, new List<AlertTransition>());

                Assert.That(alerts.ActiveCount, Is.Zero);
            }
        }

        // ---------------------------------------------------------------- 机器损坏

        [Test]
        public void DestroyedMachineRaisesACriticalAlert_AndRepairResolvesIt()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                var energy = new RegionEnergyService();
                energy.Register(Facility(RegionEnergyFacilityKind.EnvironmentGenerator, rated: 15f));
                MachineInstance machine = Deployed(session);

                var alerts = new AutoEraAlertService();
                var monitor = new RegionAlertMonitor(alerts);
                var transitions = new List<AlertTransition>();

                machine.UpdateIntegrity(0d);
                monitor.Capture(session.Machines, energy, 1000L, transitions);

                Assert.That(transitions.Count, Is.EqualTo(1));
                Assert.That(transitions[0].Kind, Is.EqualTo(AlertKind.MachineDestroyed));
                Assert.That(alerts.HighestActiveSeverity, Is.EqualTo(AlertSeverity.Critical));

                machine.UpdateIntegrity(50d);
                transitions.Clear();
                monitor.Capture(session.Machines, energy, 2000L, transitions);

                Assert.That(transitions.Count, Is.EqualTo(1));
                Assert.That(transitions[0].Raised, Is.False);
                Assert.That(alerts.ActiveCount, Is.Zero, "完整度恢复后警报必须自己转历史。");
            }
        }

        // ---------------------------------------------------------------- 能源设施

        [Test]
        public void EmptyStorageRaisesDepletion_AndChargingResolvesIt()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                var energy = new RegionEnergyService();
                RegionEnergyFacility battery = Facility(RegionEnergyFacilityKind.Battery, capacity: 240f, charge: 0f);
                energy.Register(battery);

                var alerts = new AutoEraAlertService();
                var monitor = new RegionAlertMonitor(alerts);
                var transitions = new List<AlertTransition>();

                monitor.Capture(session.Machines, energy, 1000L, transitions);
                Assert.That(transitions.Count, Is.EqualTo(1));
                Assert.That(transitions[0].Kind, Is.EqualTo(AlertKind.StorageDepleted));
                Assert.That(transitions[0].Source, Is.EqualTo(FacilityId),
                    "只有一台蓄电池时来源就是它，玩家才能看出是哪台设施。");

                battery.Storage.Charge = 30f;
                transitions.Clear();
                monitor.Capture(session.Machines, energy, 2000L, transitions);
                Assert.That(transitions.Count, Is.EqualTo(1));
                Assert.That(transitions[0].Raised, Is.False);
            }
        }

        [Test]
        public void RegionWithoutStorage_NeverReportsDepletion()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                var energy = new RegionEnergyService();
                energy.Register(Facility(RegionEnergyFacilityKind.FuelGenerator, rated: 60f, biomass: 10f));

                var alerts = new AutoEraAlertService();
                new RegionAlertMonitor(alerts).Capture(session.Machines, energy, 1000L, new List<AlertTransition>());

                Assert.That(alerts.ActiveCount, Is.Zero, "没有储能设施的区域，「一直是 0」不是耗尽。");
            }
        }

        [Test]
        public void FuelExhaustionRaisesAnAlert_AndRefuellingResolvesIt()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                var energy = new RegionEnergyService();
                RegionEnergyFacility fuel = Facility(RegionEnergyFacilityKind.FuelGenerator, rated: 60f, biomass: 10f);
                energy.Register(fuel);

                var alerts = new AutoEraAlertService();
                var monitor = new RegionAlertMonitor(alerts);
                var transitions = new List<AlertTransition>();

                monitor.Capture(session.Machines, energy, 1000L, transitions);
                Assert.That(transitions, Is.Empty, "还有燃料时不报警。");

                fuel.Generator.FuelEnergyAvailable = 0f;
                transitions.Clear();
                monitor.Capture(session.Machines, energy, 2000L, transitions);

                Assert.That(transitions.Count, Is.EqualTo(1));
                Assert.That(transitions[0].Kind, Is.EqualTo(AlertKind.FuelExhausted));
                Assert.That(transitions[0].Source, Is.EqualTo(FacilityId));

                fuel.Generator.FuelEnergyAvailable = FirstVersionEnergy.BiomassToEnergy(1f);
                transitions.Clear();
                monitor.Capture(session.Machines, energy, 3000L, transitions);
                Assert.That(transitions.Count, Is.EqualTo(1));
                Assert.That(transitions[0].Raised, Is.False);
            }
        }

        [Test]
        public void EnvironmentGeneratorIsNotTreatedAsAFuelFacility()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                var energy = new RegionEnergyService();
                energy.Register(Facility(RegionEnergyFacilityKind.EnvironmentGenerator, rated: 15f));

                var alerts = new AutoEraAlertService();
                new RegionAlertMonitor(alerts).Capture(session.Machines, energy, 1000L, new List<AlertTransition>());

                Assert.That(alerts.Count, Is.Zero, "环境能源没有燃料，不该有燃料警报。");
            }
        }

        [Test]
        public void UndeployedMachinesAreNotWatched()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                var energy = new RegionEnergyService();
                energy.Register(Facility(RegionEnergyFacilityKind.EnvironmentGenerator, rated: 15f));
                MachineInstance libraryOnly = session.Machines.Create(Definition());
                libraryOnly.UpdateIntegrity(0d);

                var alerts = new AutoEraAlertService();
                new RegionAlertMonitor(alerts).Capture(session.Machines, energy, 1000L, new List<AlertTransition>());

                Assert.That(alerts.ActiveCount, Is.Zero,
                    "库中机器不在区域里运行，它的损坏是整备页的事，不是区域警报。");
            }
        }

        [Test]
        public void MissingEnergyServiceStillReportsMachineDamage()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Deployed(session);
                machine.UpdateIntegrity(0d);

                var alerts = new AutoEraAlertService();
                new RegionAlertMonitor(alerts).Capture(session.Machines, null, 1000L, new List<AlertTransition>());

                Assert.That(alerts.ActiveCount, Is.EqualTo(1),
                    "没有电网的区域照样会损坏机器——警报不能因为能源不可用就整体失效。");
            }
        }
    }
}
