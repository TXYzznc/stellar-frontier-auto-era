using System.Collections.Generic;
using AutoEra.Energy;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 离散能源事件的识别（规格 06「第一版能源界面」：第一版不做连续功率曲线，
    /// 能源历史只记电量过低／耗尽、缺电停机／恢复、燃料耗尽和负载首次超过供给等离散事件）。
    ///
    /// 这一层要证明的核心不是「能认出状态」，而是**不会刷屏也不会漏**：
    /// <list type="bullet">
    /// <item>同一次停机只记一条：结算每帧都在跑，若每帧记一条，日志环形缓冲几分钟就被冲干净，
    ///       能源历史会变成「最近 512 帧的噪声」而不是历史；</item>
    /// <item>恢复必须成对出现：没有恢复记录，「活跃或已恢复」和「持续时长」就无从谈起；</item>
    /// <item>第一次结算不是一次状态跨越：打开游戏的第一帧不该凭空出现「电量耗尽」；</item>
    /// <item>没有储能设施的区域不记电量事件——「一直没有电」不等于「刚刚耗尽」。</item>
    /// </list>
    /// </summary>
    public sealed class EnergyEventRecorderEditModeTests
    {
        private static readonly PersistentId FacilityId = new PersistentId(6001);
        private static readonly PersistentId MachineA = new PersistentId(101);
        private static readonly PersistentId MachineB = new PersistentId(102);

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

        /// <summary>造一个声明好的设施（字段是 private 序列化字段，所以走 SerializedObject）。</summary>
        private RegionEnergyFacility Facility(RegionEnergyFacilityKind kind, float rated = 0f, float biomass = 0f,
            float capacity = 0f, float charge = 0f)
        {
            var host = new GameObject("EnergyFacility");
            _hosts.Add(host);
            var facility = host.AddComponent<RegionEnergyFacility>();
            var data = new SerializedObject(facility);
            data.FindProperty("_kind").enumValueIndex = (int)kind;
            // 三个数值字段**无条件写入**：用例里有「初始燃料为 0」这种完全合法的场景，
            // 而设施的默认值不是 0（新存档初始生物质），只在 >0 时才写会让那条用例测不到东西。
            data.FindProperty("_ratedPower").floatValue = rated;
            data.FindProperty("_initialBiomass").floatValue = biomass;
            data.FindProperty("_capacity").floatValue = capacity;
            data.FindProperty("_initialCharge").floatValue = charge;
            data.ApplyModifiedPropertiesWithoutUndo();
            facility.Initialize(FacilityId);
            return facility;
        }

        private static EnergyGridSnapshot Snapshot(float generated = 0f, float consumed = 0f, float stored = 0f,
            float capacity = 0f, IReadOnlyList<PersistentId> stopped = null, bool shortfall = false) =>
            new EnergyGridSnapshot(generated, consumed, stored, capacity, 0f, 0f, 0f, stopped, shortfall);

        private static List<EnergyDiscreteEvent> Capture(EnergyEventRecorder recorder, EnergyGridSnapshot snapshot,
            IReadOnlyList<RegionEnergyFacility> facilities = null)
        {
            var events = new List<EnergyDiscreteEvent>(8);
            recorder.Capture(snapshot, facilities, events);
            return events;
        }

        // ---------------------------------------------------------------- 缺电停机／恢复

        [Test]
        public void StoppedMachine_IsReportedOnce_NotOnEverySettlement()
        {
            var recorder = new EnergyEventRecorder();
            EnergyGridSnapshot stopped = Snapshot(consumed: 8.2f, stopped: new[] { MachineA });

            Assert.That(Capture(recorder, stopped).Count, Is.EqualTo(1), "第一次结算就该记下这次停机。");
            Assert.That(Capture(recorder, stopped), Is.Empty, "还在停机的同一台机器不能每帧再记一条。");
            Assert.That(Capture(recorder, stopped), Is.Empty);
        }

        [Test]
        public void RecoveringMachine_ProducesARecoveryEvent()
        {
            var recorder = new EnergyEventRecorder();
            Capture(recorder, Snapshot(stopped: new[] { MachineA }));

            List<EnergyDiscreteEvent> events = Capture(recorder, Snapshot(stopped: null));

            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].Kind, Is.EqualTo(EnergyEventKind.ShortageRecovered));
            Assert.That(events[0].Subject, Is.EqualTo(MachineA));
        }

        [Test]
        public void SeveralMachines_AreTrackedIndependently()
        {
            var recorder = new EnergyEventRecorder();
            List<EnergyDiscreteEvent> first = Capture(recorder, Snapshot(stopped: new[] { MachineA, MachineB }));
            Assert.That(first.Count, Is.EqualTo(2));

            // A 恢复、B 还在停 → 只应记 A 的恢复。
            List<EnergyDiscreteEvent> second = Capture(recorder, Snapshot(stopped: new[] { MachineB }));

            Assert.That(second.Count, Is.EqualTo(1));
            Assert.That(second[0].Kind, Is.EqualTo(EnergyEventKind.ShortageRecovered));
            Assert.That(second[0].Subject, Is.EqualTo(MachineA));
        }

        // ---------------------------------------------------------------- 电量耗尽／恢复

        [Test]
        public void FirstSettlementWithCharge_IsNotReportedAsDepletion()
        {
            var recorder = new EnergyEventRecorder();

            // 第一次结算：储能是满的。这不是「刚刚恢复」，也不该反过来。
            Assert.That(Capture(recorder, Snapshot(stored: 240f, capacity: 240f)), Is.Empty,
                "打开游戏的第一帧不该凭空出现一条「电量耗尽」。");
            Assert.That(Capture(recorder, Snapshot(stored: 9f, capacity: 240f)), Is.Empty,
                "从满到 9 不是耗尽。");

            List<EnergyDiscreteEvent> depleted = Capture(recorder, Snapshot(stored: 0f, capacity: 240f));
            Assert.That(depleted.Count, Is.EqualTo(1));
            Assert.That(depleted[0].Kind, Is.EqualTo(EnergyEventKind.StorageDepleted));

            Assert.That(Capture(recorder, Snapshot(stored: 0f, capacity: 240f)), Is.Empty,
                "停在 0 不该反复记。");

            List<EnergyDiscreteEvent> recovered = Capture(recorder, Snapshot(stored: 12f, capacity: 240f));
            Assert.That(recovered.Count, Is.EqualTo(1));
            Assert.That(recovered[0].Kind, Is.EqualTo(EnergyEventKind.StorageRecovered));
        }

        [Test]
        public void RegionWithoutStorage_NeverReportsChargeEvents()
        {
            var recorder = new EnergyEventRecorder();

            Assert.That(Capture(recorder, Snapshot(capacity: 0f)), Is.Empty);
            Assert.That(Capture(recorder, Snapshot(stored: 0f, capacity: 0f)), Is.Empty,
                "没有储能设施的区域，「一直没有电」不是「刚刚耗尽」。");
        }

        [Test]
        public void StorageEvent_CarriesTheFacilityWhenThereIsExactlyOne()
        {
            var recorder = new EnergyEventRecorder();
            RegionEnergyFacility battery = Facility(RegionEnergyFacilityKind.Battery, capacity: 240f, charge: 240f);
            var facilities = new List<RegionEnergyFacility> { battery };

            Capture(recorder, Snapshot(stored: 240f, capacity: 240f), facilities);
            List<EnergyDiscreteEvent> events = Capture(recorder, Snapshot(stored: 0f, capacity: 240f), facilities);

            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].Subject, Is.EqualTo(FacilityId),
                "只有一台蓄电池时事件主体就是它，界面才能显示是哪台设施。");
        }

        // ---------------------------------------------------------------- 燃料耗尽

        [Test]
        public void FuelExhaustion_IsReportedOnce_AndRearmsAfterRefuelling()
        {
            var recorder = new EnergyEventRecorder();
            RegionEnergyFacility fuel = Facility(RegionEnergyFacilityKind.FuelGenerator, rated: 60f, biomass: 10f);
            var facilities = new List<RegionEnergyFacility> { fuel };

            Assert.That(Capture(recorder, Snapshot(generated: 60f), facilities), Is.Empty,
                "还有燃料时不记。");

            // 烧光：FuelEnergyAvailable 是燃料的唯一真实来源（RemainingBiomass 由它换算，
            // 见 FuelGenerator），所以这里改它就等于「这罐燃料用完了」。
            fuel.Generator.FuelEnergyAvailable = 0f;
            List<EnergyDiscreteEvent> exhausted = Capture(recorder, Snapshot(generated: 0f), facilities);

            Assert.That(exhausted.Count, Is.EqualTo(1));
            Assert.That(exhausted[0].Kind, Is.EqualTo(EnergyEventKind.FuelExhausted));
            Assert.That(exhausted[0].Subject, Is.EqualTo(FacilityId));
            Assert.That(Capture(recorder, Snapshot(generated: 0f), facilities), Is.Empty,
                "一直在耗尽状态不重复记。");

            // 补上燃料后重新武装：再次耗尽必须是一条**新**事件，否则第二罐烧完就查不到了。
            fuel.Generator.FuelEnergyAvailable = FirstVersionEnergy.BiomassToEnergy(2f);
            Assert.That(Capture(recorder, Snapshot(generated: 60f), facilities), Is.Empty);
            fuel.Generator.FuelEnergyAvailable = 0f;
            Assert.That(Capture(recorder, Snapshot(generated: 0f), facilities).Count, Is.EqualTo(1));
        }

        [Test]
        public void FuelExhaustion_RecognisesAZeroBiomassFacility()
        {
            var recorder = new EnergyEventRecorder();
            // 初始生物质为 0 的燃料设施：第一次结算就是耗尽状态，应当记一条（这是事实）。
            RegionEnergyFacility fuel = Facility(RegionEnergyFacilityKind.FuelGenerator, rated: 60f, biomass: 0f);
            var facilities = new List<RegionEnergyFacility> { fuel };

            List<EnergyDiscreteEvent> events = Capture(recorder, Snapshot(generated: 60f), facilities);

            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].Kind, Is.EqualTo(EnergyEventKind.FuelExhausted));
            Assert.That(events[0].Subject, Is.EqualTo(FacilityId));
            Assert.That(Capture(recorder, Snapshot(generated: 60f), facilities), Is.Empty,
                "一直在耗尽状态不重复记。");
        }

        [Test]
        public void EnvironmentGeneratorNeverReportsFuelExhaustion()
        {
            var recorder = new EnergyEventRecorder();
            RegionEnergyFacility solar = Facility(RegionEnergyFacilityKind.EnvironmentGenerator, rated: 15f);

            Assert.That(Capture(recorder, Snapshot(generated: 15f), new List<RegionEnergyFacility> { solar }), Is.Empty,
                "环境能源没有燃料，不该有燃料耗尽事件。");
        }

        // ---------------------------------------------------------------- 供电缺口

        [Test]
        public void Shortfall_IsReportedOnlyWhenItStarts()
        {
            var recorder = new EnergyEventRecorder();

            Assert.That(Capture(recorder, Snapshot(generated: 15f, consumed: 8.2f)), Is.Empty);
            List<EnergyDiscreteEvent> start = Capture(recorder, Snapshot(generated: 0f, consumed: 8.2f, shortfall: true));

            Assert.That(start.Count, Is.EqualTo(1));
            Assert.That(start[0].Kind, Is.EqualTo(EnergyEventKind.SupplyExceeded));
            Assert.That(Capture(recorder, Snapshot(generated: 0f, consumed: 8.2f, shortfall: true)), Is.Empty,
                "缺口持续期间不重复记。");
        }

        // ---------------------------------------------------------------- 重置

        [Test]
        public void Reset_MakesTheNextStateLookFresh()
        {
            var recorder = new EnergyEventRecorder();
            Capture(recorder, Snapshot(stored: 240f, capacity: 240f, stopped: new[] { MachineA }));

            recorder.Reset();

            Assert.That(Capture(recorder, Snapshot(stored: 240f, capacity: 240f, stopped: new[] { MachineA })).Count,
                Is.EqualTo(1),
                "区域重建后，同一台机器再次停机应当重新记一条。");
        }

        // ---------------------------------------------------------------- 词汇表

        [Test]
        public void LabelsRoundTripThroughTheSharedVocabulary()
        {
            foreach (EnergyEventKind kind in new[]
                     {
                         EnergyEventKind.ShortageStopped, EnergyEventKind.ShortageRecovered,
                         EnergyEventKind.StorageDepleted, EnergyEventKind.StorageRecovered,
                         EnergyEventKind.FuelExhausted, EnergyEventKind.SupplyExceeded,
                     })
            {
                Assert.That(EnergyEventText.Parse(EnergyEventText.Label(kind)), Is.EqualTo(kind),
                    $"生产侧写的动作名必须能被读取侧认回来（{kind}）。");
            }

            Assert.That(EnergyEventText.Parse("移动到位"), Is.Null,
                "别的事件域的动作名不该被当成能源事件。");
        }

        [Test]
        public void RecoveryPairsAreSymmetric()
        {
            Assert.That(EnergyEventText.RecoveryOf(EnergyEventKind.ShortageStopped),
                Is.EqualTo(EnergyEventKind.ShortageRecovered));
            Assert.That(EnergyEventText.StopOf(EnergyEventKind.ShortageRecovered),
                Is.EqualTo(EnergyEventKind.ShortageStopped));
            Assert.That(EnergyEventText.RecoveryOf(EnergyEventKind.FuelExhausted), Is.Null,
                "燃料耗尽没有恢复事件（补充燃料尚未实现），界面据此显示为「活跃」。");
            Assert.That(EnergyEventText.RecoveryOf(EnergyEventKind.SupplyExceeded), Is.Null);
        }
    }
}
