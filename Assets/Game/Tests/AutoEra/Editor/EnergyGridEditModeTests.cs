using AutoEra.Energy;
using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 区域电网结算（规格 06-能源储存与物流；任务 P5-006）。
    ///
    /// 这是项目里**第一个真正算得清的领域**：每个断言都能手算复核（电量 ＝ 功率 × 秒 ÷ 60），
    /// 所以用例全部用设计里给出的基准数值——太阳能 15、生物质发电机 60、蓄电池 240、
    /// 1 生物质 ＝ 60 电量——而不是随便造几个数字。
    ///
    /// 四条最容易写错、而且写错都不会报错的规则，各自有用例守着：
    ///   ① 供电顺序（免费环境能源 → 燃料发电 → 蓄电池 → 停机）不能凭直觉换序；
    ///   ② 燃料充电默认关闭，开启后也只充到目标比例；
    ///   ③ 同一时刻蓄电池不能同时充放电；
    ///   ④ 停机与恢复是确定的（优先级、同级按进入队列的时间）。
    /// </summary>
    public sealed class EnergyGridEditModeTests
    {
        private static readonly PersistentId Solar = new PersistentId(9001);
        private static readonly PersistentId Biomass = new PersistentId(9002);
        private static readonly PersistentId Battery = new PersistentId(9003);
        private static readonly PersistentId A = new PersistentId(9101);
        private static readonly PersistentId B = new PersistentId(9102);
        private static readonly PersistentId C = new PersistentId(9103);

        private static EnvironmentGenerator SolarGenerator() => new EnvironmentGenerator(Solar, FirstVersionEnergy.SolarDaylightPower);

        private static FuelGenerator BiomassGenerator(float biomass = FirstVersionEnergy.StartupBiomass)
            => new FuelGenerator(Biomass, FirstVersionEnergy.BiomassGeneratorRatedPower,
                FirstVersionEnergy.BiomassToEnergy(biomass));

        private static BatteryStorage FullBattery()
            => new BatteryStorage(Battery, FirstVersionEnergy.BatteryCapacity, FirstVersionEnergy.BatteryCapacity);

        // ---------------------------------------------------------------- 换算与周期

        [Test]
        public void EnergyConversion_MatchesTheDesignFormula()
        {
            // 规格：1 功率持续运行 1 分钟 ＝ 1 电量；一块满电蓄电池支撑 60 功率缺口 4 分钟。
            Assert.That(EnergyGrid.PowerToEnergy(1f, 60f), Is.EqualTo(1f).Within(1e-5f));
            Assert.That(EnergyGrid.PowerToEnergy(60f, 240f), Is.EqualTo(240f).Within(1e-3f),
                "60 功率跑 4 分钟正好是一块满电蓄电池的容量。");
            Assert.That(EnergyGrid.EnergyToPower(240f, 240f), Is.EqualTo(60f).Within(1e-3f));
            Assert.That(FirstVersionEnergy.BiomassToEnergy(10f), Is.EqualTo(600f).Within(1e-3f));
            Assert.That(FirstVersionEnergy.BiomassToEnergy(30f), Is.EqualTo(1800f).Within(1e-3f));
        }

        [Test]
        public void DaylightCycle_IsTwentyFourMinutesWithSixteenMinutesOfLight()
        {
            Assert.That(DaylightCycle.DayMilliseconds, Is.EqualTo(24L * 60L * 1000L));
            Assert.That(DaylightCycle.DaylightMilliseconds, Is.EqualTo(16L * 60L * 1000L));

            // 世界时间 0 就是日出，所以「从白天早期开始」不需要任何偏移量。
            Assert.That(DaylightCycle.IsDaylight(0), Is.True);
            Assert.That(DaylightCycle.IsDaylight(15L * 60L * 1000L), Is.True);
            Assert.That(DaylightCycle.IsDaylight(17L * 60L * 1000L), Is.False, "第 16 分钟起进入无日照。");
            Assert.That(DaylightCycle.IsDaylight(23L * 60L * 1000L), Is.False);
            Assert.That(DaylightCycle.IsDaylight(24L * 60L * 1000L), Is.True, "满一天之后回到白天。");

            // 相位是循环的：世界时间很大或为负都不该抛错。
            Assert.That(DaylightCycle.Phase(-1000L), Is.InRange(0L, DaylightCycle.DayMilliseconds - 1));
            Assert.That(DaylightCycle.MillisecondsUntilSwitch(0L), Is.EqualTo(16L * 60L * 1000L));
            Assert.That(DaylightCycle.MillisecondsUntilSwitch(17L * 60L * 1000L), Is.EqualTo(7L * 60L * 1000L));
        }

        // ---------------------------------------------------------------- 供电顺序

        [Test]
        public void EnvironmentEnergyCoversLoadFirst_AndSurplusChargesTheBattery()
        {
            var grid = new EnergyGrid();
            EnvironmentGenerator solar = SolarGenerator();
            solar.EnvironmentPower = FirstVersionEnergy.SolarDaylightPower;
            grid.AddGenerator(solar);
            grid.AddStorage(new BatteryStorage(Battery, FirstVersionEnergy.BatteryCapacity, 0f));
            EnergyConsumer load = Consumer(A, PowerPriority.Production, 0f, 10f);
            grid.AddConsumer(load);
            load.IsWorking = true;

            EnergyGridSnapshot snapshot = grid.Tick(60f, daylight: true);

            Assert.That(snapshot.GeneratedPower, Is.EqualTo(15f).Within(1e-4f));
            Assert.That(snapshot.ConsumedPower, Is.EqualTo(10f).Within(1e-4f));
            Assert.That(snapshot.ChargingPower, Is.EqualTo(5f).Within(1e-4f));
            Assert.That(snapshot.StoredCharge, Is.EqualTo(5f).Within(1e-4f), "5 功率充 1 分钟 ＝ 5 电量。");
            Assert.That(snapshot.DiscardedPower, Is.Zero);
            Assert.That(load.IsPowered, Is.True);
        }

        [Test]
        public void SurplusEnvironmentEnergyIsDiscardedOnceStorageIsFull()
        {
            var grid = new EnergyGrid();
            EnvironmentGenerator solar = SolarGenerator();
            solar.EnvironmentPower = FirstVersionEnergy.SolarDaylightPower;
            grid.AddGenerator(solar);
            grid.AddStorage(FullBattery());
            EnergyConsumer load = Consumer(A, PowerPriority.Production, 0f, 10f);
            grid.AddConsumer(load);
            load.IsWorking = true;

            EnergyGridSnapshot snapshot = grid.Tick(60f, daylight: true);

            Assert.That(snapshot.DiscardedPower, Is.EqualTo(5f).Within(1e-4f), "储满之后盈余直接舍弃。");
            Assert.That(snapshot.StoredCharge, Is.EqualTo(FirstVersionEnergy.BatteryCapacity).Within(1e-4f));
            Assert.That(snapshot.ChargingPower, Is.Zero);
        }

        [Test]
        public void FuelGeneratorOnlyCoversWhatEnvironmentEnergyMissed()
        {
            var grid = new EnergyGrid();
            EnvironmentGenerator solar = SolarGenerator();
            solar.EnvironmentPower = 0f; // 夜间
            grid.AddGenerator(solar);
            FuelGenerator fuel = BiomassGenerator();
            grid.AddGenerator(fuel);
            EnergyConsumer load = Consumer(A, PowerPriority.Production, 0f, 20f);
            grid.AddConsumer(load);
            load.IsWorking = true;

            EnergyGridSnapshot snapshot = grid.Tick(60f, daylight: false);

            Assert.That(fuel.ActualOutputPower, Is.EqualTo(20f).Within(1e-4f),
                "实际输出 ＝ min（额定，电网分配的剩余需求）＝ 20，而不是满额 60。");
            Assert.That(snapshot.GeneratedPower, Is.EqualTo(20f).Within(1e-4f));
            Assert.That(fuel.FuelEnergyAvailable, Is.EqualTo(580f).Within(1e-3f),
                "20 功率跑 1 分钟消耗 20 电量。");
        }

        [Test]
        public void IdleFuelGeneratorBurnsNothing()
        {
            var grid = new EnergyGrid();
            FuelGenerator fuel = BiomassGenerator();
            grid.AddGenerator(fuel);

            EnergyGridSnapshot snapshot = grid.Tick(60f, daylight: false);

            Assert.That(fuel.ActualOutputPower, Is.Zero);
            Assert.That(fuel.IsStandby, Is.True, "开着但没有需求时是「待命」。");
            Assert.That(fuel.FuelEnergyAvailable, Is.EqualTo(600f).Within(1e-3f));
            Assert.That(snapshot.GeneratedPower, Is.Zero);
        }

        [Test]
        public void SeveralFuelGeneratorsShareTheNeedInBuildOrder()
        {
            var grid = new EnergyGrid();
            FuelGenerator first = new FuelGenerator(new PersistentId(9002), 60f, 600f);
            FuelGenerator second = new FuelGenerator(new PersistentId(9004), 60f, 600f);
            grid.AddGenerator(first);
            grid.AddGenerator(second);
            EnergyConsumer load = Consumer(A, PowerPriority.Production, 0f, 80f);
            grid.AddConsumer(load);
            load.IsWorking = true;

            EnergyGridSnapshot snapshot = grid.Tick(60f, daylight: false);

            Assert.That(first.ActualOutputPower, Is.EqualTo(60f).Within(1e-4f), "先登记的先用满额定。");
            Assert.That(second.ActualOutputPower, Is.EqualTo(20f).Within(1e-4f), "第二台只补剩下的 20。");
            Assert.That(snapshot.GeneratedPower, Is.EqualTo(80f).Within(1e-4f));
        }

        // ---------------------------------------------------------------- 储能

        [Test]
        public void BatteryCoversTheGap_AndFourMinutesAtSixtyMatchesTheDesign()
        {
            var grid = new EnergyGrid();
            grid.AddStorage(FullBattery());
            EnergyConsumer load = Consumer(A, PowerPriority.Production, 0f, 60f);
            grid.AddConsumer(load);
            load.IsWorking = true;

            EnergyGridSnapshot first = grid.Tick(60f, daylight: false);

            Assert.That(first.DischargingPower, Is.EqualTo(60f).Within(1e-3f));
            Assert.That(first.StoredCharge, Is.EqualTo(180f).Within(1e-3f));
            Assert.That(first.HasShortfall, Is.False);
            Assert.That(load.IsPowered, Is.True, "缺口由蓄电池补足时不停机。");

            // 再把 180 电量正好跑完：满电蓄电池在 60 功率缺口下撑 4 分钟（规格的标准作业周期）。
            EnergyGridSnapshot second = grid.Tick(180f, daylight: false);
            Assert.That(second.StoredCharge, Is.Zero.Within(1e-3f));
            Assert.That(second.StoppedByShortage, Is.Empty, "电量刚好够用就不该停机。");
            Assert.That(load.IsPowered, Is.True);

            // 再往后一秒就没电了：这时才停机，并如实报告「本时刻缺电」。
            EnergyGridSnapshot third = grid.Tick(1f, daylight: false);
            Assert.That(third.StoppedByShortage, Does.Contain(A), "电池耗尽后才开始停机。");
            Assert.That(third.HasShortfall, Is.True);
            Assert.That(load.IsPowered, Is.False);
        }

        [Test]
        public void BatteryCannotChargeAndDischargeInTheSameMoment()
        {
            var grid = new EnergyGrid();
            EnvironmentGenerator solar = SolarGenerator();
            solar.EnvironmentPower = 15f;
            grid.AddGenerator(solar);
            BatteryStorage battery = new BatteryStorage(Battery, 240f, 100f);
            grid.AddStorage(battery);
            EnergyConsumer load = Consumer(A, PowerPriority.Production, 0f, 5f);
            grid.AddConsumer(load);
            load.IsWorking = true;

            EnergyGridSnapshot snapshot = grid.Tick(60f, daylight: true);

            Assert.That(battery.State, Is.EqualTo(StorageState.Charging));
            Assert.That(snapshot.DischargingPower, Is.Zero, "同一时刻只能走一边。");
            Assert.That(snapshot.StoredCharge, Is.EqualTo(110f).Within(1e-3f));
        }

        [Test]
        public void LoadIsServedBeforeAnyChargingNeed()
        {
            var grid = new EnergyGrid();
            FuelGenerator fuel = BiomassGenerator(2f); // 只有 120 电量 = 2 分钟额定输出
            fuel.AllowsCharging = true;
            fuel.ChargeTargetRatio = 1f;
            grid.AddGenerator(fuel);
            BatteryStorage battery = new BatteryStorage(Battery, 240f, 0f);
            grid.AddStorage(battery);
            EnergyConsumer load = Consumer(A, PowerPriority.Production, 0f, 60f);
            grid.AddConsumer(load);
            load.IsWorking = true;

            EnergyGridSnapshot snapshot = grid.Tick(60f, daylight: false);

            Assert.That(load.IsPowered, Is.True, "实时负载优先于任何充电需求。");
            Assert.That(snapshot.ConsumedPower, Is.EqualTo(60f).Within(1e-3f));
            Assert.That(fuel.ActualOutputPower, Is.EqualTo(60f).Within(1e-3f),
                "额定已经全给负载，没有余量充电。");
            Assert.That(snapshot.StoredCharge, Is.Zero.Within(1e-3f));
        }

        [Test]
        public void FuelChargingIsOffByDefault_AndOnlyFillsUpToTheTargetRatio()
        {
            // 默认：燃料发电不主动为储能充电。
            var off = new EnergyGrid();
            FuelGenerator idle = BiomassGenerator(10f);
            off.AddGenerator(idle);
            off.AddStorage(new BatteryStorage(Battery, 240f, 0f));
            EnergyGridSnapshot whenOff = off.Tick(60f, daylight: false);

            Assert.That(whenOff.StoredCharge, Is.Zero.Within(1e-3f), "没开许可就不该为储能发电。");
            Assert.That(idle.ActualOutputPower, Is.Zero);

            // 开启许可并设置目标 50%：只充到 120 电量为止。
            var on = new EnergyGrid();
            FuelGenerator charging = BiomassGenerator(10f);
            charging.AllowsCharging = true;
            charging.ChargeTargetRatio = 0.5f;
            on.AddGenerator(charging);
            BatteryStorage battery = new BatteryStorage(Battery, 240f, 0f);
            on.AddStorage(battery);

            EnergyGridSnapshot target = on.Tick(60f, daylight: false);
            Assert.That(target.StoredCharge, Is.EqualTo(60f).Within(1e-3f),
                "一分钟内只能充进 60 功率对应的 60 电量（额定就是充电功率上限）。");

            // 连续跑：达到目标比例后必须停下，不再继续充。
            for (int minute = 0; minute < 5; minute++) on.Tick(60f, daylight: false);

            Assert.That(battery.Charge, Is.EqualTo(120f).Within(1e-3f), "目标 50% ＝ 120 电量。");
            Assert.That(charging.ActualOutputPower, Is.Zero.Within(1e-4f),
                "达到目标后发电站自动恢复为只满足实时负载。");
        }

        // ---------------------------------------------------------------- 停机与恢复

        [Test]
        public void ShortageStopsTheLowestPriorityLoadFirst()
        {
            var grid = new EnergyGrid();
            grid.AddStorage(FullBattery());
            EnergyConsumer critical = Consumer(A, PowerPriority.Critical, 0f, 60f);
            EnergyConsumer production = Consumer(B, PowerPriority.Production, 0f, 60f);
            EnergyConsumer pausable = Consumer(C, PowerPriority.Pausable, 0f, 60f);
            grid.AddConsumer(critical);
            grid.AddConsumer(production);
            grid.AddConsumer(pausable);
            critical.IsWorking = true;
            production.IsWorking = true;
            pausable.IsWorking = true;

            // 总负载 180，储能最多给 240 电量 / 60 秒 ＝ 240 功率，先够用。
            EnergyGridSnapshot first = grid.Tick(60f, daylight: false);
            Assert.That(first.StoppedByShortage, Is.Empty);

            // 电池只剩下位置：跑掉 240 电量之后（已经用了 180），再要 180 就不够了。
            EnergyGridSnapshot second = grid.Tick(60f, daylight: false);
            Assert.That(second.StoppedByShortage, Is.EqualTo(new[] { C, B }),
                "从优先级最低的开始停：可暂停 → 次要／普通生产，关键设备最后。");
            Assert.That(critical.IsPowered, Is.True);
            Assert.That(pausable.IsStoppedByShortage, Is.True);
        }

        [Test]
        public void SamePriorityStopsTheLatestQueuedFirst_AndTheConclusionDoesNotPersist()
        {
            var grid = new EnergyGrid();
            BatteryStorage battery = new BatteryStorage(Battery, 240f, 60f); // 只够 60 功率跑 1 分钟
            grid.AddStorage(battery);
            EnergyConsumer first = Consumer(A, PowerPriority.Production, 0f, 60f);
            EnergyConsumer second = Consumer(B, PowerPriority.Production, 0f, 60f);
            grid.AddConsumer(first);
            grid.AddConsumer(second);
            first.IsWorking = true;
            second.IsWorking = true;

            EnergyGridSnapshot short1 = grid.Tick(60f, daylight: false);

            Assert.That(first.QueueOrder, Is.LessThan(second.QueueOrder));
            Assert.That(short1.StoppedByShortage, Is.EqualTo(new[] { B }),
                "同优先级按进入供电队列的时间**后进先停**（先进的那台先恢复）。");
            Assert.That(first.IsPowered, Is.True);
            Assert.That(second.IsPowered, Is.False);
            Assert.That(battery.Charge, Is.Zero.Within(1e-3f), "那一分钟正好把电池跑空。");

            // 补上电、并让一个负载不再请求用电：供需重新成立，被停的对象应当自己回来
            // （不必等玩家操作）。
            battery.Charge = 240f;
            first.IsDemandActive = false;
            EnergyGridSnapshot recovered = grid.Tick(60f, daylight: false);

            Assert.That(recovered.StoppedByShortage, Is.Empty,
                "每次结算重新判定停机，缺电结论不延续——否则一次缺电会让对象永久停机。");
            Assert.That(recovered.HasShortfall, Is.False);
            Assert.That(second.IsPowered, Is.True);
        }

        [Test]
        public void WithoutAnythingToStop_TheShortfallIsReportedHonestly()
        {
            var grid = new EnergyGrid();
            EnergyConsumer load = Consumer(A, PowerPriority.Critical, 0f, 60f);
            grid.AddConsumer(load);
            load.IsWorking = true;

            EnergyGridSnapshot snapshot = grid.Tick(60f, daylight: false);

            Assert.That(snapshot.HasShortfall, Is.True, "一点电都没有时必须如实报告缺口。");
            Assert.That(snapshot.GeneratedPower, Is.Zero);
            Assert.That(snapshot.ConsumedPower, Is.Zero, "没电的对象不耗电。");
            Assert.That(load.IsPowered, Is.False);
        }

        [Test]
        public void StoppedConsumersDrawNothingAndPlayerDisabledOnesNeverDo()
        {
            var grid = new EnergyGrid();
            EnvironmentGenerator solar = SolarGenerator();
            solar.EnvironmentPower = 0f;
            grid.AddGenerator(solar);

            EnergyConsumer standby = Consumer(A, PowerPriority.Production, 1.9f, 8.2f);
            EnergyConsumer working = Consumer(B, PowerPriority.Production, 1.9f, 8.2f);
            working.IsWorking = true;
            EnergyConsumer switchedOff = Consumer(C, PowerPriority.Production, 5f, 9f);
            switchedOff.IsDemandActive = false;
            grid.AddConsumer(standby);
            grid.AddConsumer(working);
            grid.AddConsumer(switchedOff);

            EnergyGridSnapshot snapshot = grid.Tick(60f, daylight: false);

            Assert.That(snapshot.StoppedByShortage.Count, Is.EqualTo(2), "只有两个对象请求用电。");
            Assert.That(switchedOff.IsStoppedByShortage, Is.False,
                "玩家自己关掉的对象不算「因缺电停机」——界面要区分这两件事。");
            Assert.That(snapshot.ConsumedPower, Is.Zero);
        }

        [Test]
        public void ZeroElapsedTickDecidesPowerStateWithoutChangingAnyCharge()
        {
            var grid = new EnergyGrid();
            grid.AddStorage(FullBattery());
            EnergyConsumer load = Consumer(A, PowerPriority.Production, 0f, 30f);
            grid.AddConsumer(load);
            load.IsWorking = true;

            EnergyGridSnapshot snapshot = grid.Tick(0f, daylight: false);

            Assert.That(snapshot.StoredCharge, Is.EqualTo(240f).Within(1e-3f),
                "「看一眼当前状态」不该给电池充电或放电。");
            Assert.That(load.IsPowered, Is.True);
        }

        [Test]
        public void EstimatesFollowTheDesignWording()
        {
            var grid = new EnergyGrid();
            grid.AddStorage(new BatteryStorage(Battery, 240f, 240f));
            EnergyConsumer load = Consumer(A, PowerPriority.Production, 0f, 60f);
            grid.AddConsumer(load);
            load.IsWorking = true;

            EnergyGridSnapshot discharging = grid.Tick(60f, daylight: false);
            Assert.That(discharging.NetPower, Is.EqualTo(-60f).Within(1e-3f));
            Assert.That(discharging.EstimatedRuntimeSeconds, Is.EqualTo(180f).Within(1f),
                "剩下 180 电量、缺口 60 功率 → 还能撑 3 分钟。");

            var chargingGrid = new EnergyGrid();
            EnvironmentGenerator solar = SolarGenerator();
            solar.EnvironmentPower = 15f;
            chargingGrid.AddGenerator(solar);
            chargingGrid.AddStorage(new BatteryStorage(Battery, 240f, 0f));

            // 先做一次 0 秒结算：只看状态、不改电量，于是「充满需要多久」的答案是干净的。
            EnergyGridSnapshot idle = chargingGrid.Tick(0f, daylight: true);
            Assert.That(idle.EstimatedRuntimeSeconds, Is.Null,
                "净功率非负时不显示「剩余耗尽时间」（规格原文）。");
            Assert.That(idle.EstimatedFullSeconds, Is.EqualTo(960f).Within(2f),
                "240 电量 ÷ 15 功率 × 60 ＝ 960 秒 ＝ 16 分钟，正好是一个日照阶段。");

            EnergyGridSnapshot charging = chargingGrid.Tick(60f, daylight: true);
            Assert.That(charging.StoredCharge, Is.EqualTo(15f).Within(1e-3f), "15 功率充 1 分钟 ＝ 15 电量。");
        }

        private static EnergyConsumer Consumer(PersistentId id, PowerPriority priority, float standby, float working)
            => new EnergyConsumer(id, priority, standby, working);
    }
}
