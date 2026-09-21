using AutoEra.Energy;
using AutoEra.Machines;
using AutoEra.World;
using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 机器作为电网负载：耗电必须**按部件逐项求和**，且电网的结论要能写回机器的供电状态。
    ///
    /// 这一批要修掉的旧行为是「功率数据在数据表里有、在运行时被丢掉」：
    /// `MachineDefinitions`／`ComponentDefinitions` 早就配好了 `IdlePower`／`WorkingPower`
    /// （旋转载体 0.1／1.0、计算核心 0.3／3.0、传感器 0.2／2.0），目录也校验了它们，
    /// 但构造运行时定义时没有传下去——于是「机器耗多少电」在工程里根本算不出来。
    ///
    /// 用例全部对齐设计里已经算过的那几个数（规格 06「第一版阶段耗电目标」附近的例子）：
    /// 完整农田机器休眠约 1.9 功率、持续运行核心与两个传感器且效应器待机约 8.2 功率、
    /// 执行浇水或收获约 10.9～14.5 功率。数值对不上就说明求和方式错了。
    /// </summary>
    public sealed class MachineEnergyConsumerEditModeTests
    {
        // 规格给出的示例机型与部件（数值取自设计文档与数据表）。
        private const double CarrierIdle = 0.1d;
        private const double CarrierWorking = 1.0d;
        private const double CoreIdle = 0.3d;
        private const double CoreWorking = 3.0d;
        private const double SensorIdle = 0.2d;
        private const double SensorWorking = 2.0d;
        private const double EffectorIdle = 1.1d;
        private const double EffectorWorking = 3.8d;

        private static MachineInstance FarmMachine(AutoEraWorldSession session)
            => session.Machines.Create(new MachineDefinition(1002, "固定旋转载体", 1, 2, 1, 1, 10, false, true, 100d,
                2d, 2d, "Machines/FixedRotaryCarrier", CarrierIdle, CarrierWorking));

        private static ComponentInstance Add(AutoEraWorldSession session, MachineInstance machine,
            HardwareKind kind, int rowId, double idle, double working, int index,
            int compute = 0, int logic = 0)
        {
            var definition = new ComponentDefinition(rowId, kind, 1, 0, compute, logic, false, idle, working);
            ComponentInstance component = session.Machines.CreateComponent(definition);
            Assert.That(session.Machines.Install(machine.Id, ManagementOrigin.Library, component.Id, index),
                Is.EqualTo(MachineManagementResult.Completed));
            return component;
        }

        private static MachineInstance Farm(AutoEraWorldSession session, out ComponentInstance core,
            out ComponentInstance effector)
        {
            MachineInstance machine = FarmMachine(session);
            Add(session, machine, HardwareKind.Sensor, 21011, SensorIdle, SensorWorking, 0);
            Add(session, machine, HardwareKind.Sensor, 21021, SensorIdle, SensorWorking, 1);
            core = Add(session, machine, HardwareKind.Core, 20011, CoreIdle, CoreWorking, 0, 50, 40);
            effector = Add(session, machine, HardwareKind.Effector, 22011, EffectorIdle, EffectorWorking, 0);
            session.Machines.Deploy(machine.Id);
            // 休眠是「已激活」之后才成立的状态，所以夹具要先把机器激活——
            // 否则 SetRunState(Sleeping) 会以 NotActivated 被拒绝，休眠相关的用例就测不到东西。
            machine.Activate(ManagementOrigin.Field);
            return machine;
        }

        // ---------------------------------------------------------------- 逐部件求和

        [Test]
        public void SleepingMachineMatchesTheDesignsIdleExample()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Farm(session, out _, out _);

                // 休眠：载体与所有部件都按待机 → 0.1 + 0.2 + 0.2 + 0.3 + 1.1 = 1.9
                machine.SetRunState(ManagementOrigin.Field, MachineRunState.Sleeping);

                Assert.That(machine.IdlePowerDraw, Is.EqualTo(1.9d).Within(1e-6));
                Assert.That(machine.CurrentPowerDraw, Is.EqualTo(1.9d).Within(1e-6),
                    "规格：完整农田机器休眠约为 1.9 功率。");
            }
        }

        [Test]
        public void ContinuousSensingAndCoreMatchTheDesignsWorkingExample()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Farm(session, out _, out _);

                // 持续运行核心与两个传感器、效应器待机：
                // 载体 0.1 + 核心 3.0 + 两个传感器各 2.0 + 效应器待机 1.1 = 8.2
                machine.UpdateComputeUsage(10, 10, 0);

                Assert.That(machine.CurrentPowerDraw, Is.EqualTo(8.2d).Within(1e-6),
                    "规格：持续运行核心与两个传感器、效应器待机时约为 8.2 功率。");
            }
        }

        [Test]
        public void ExecutingAnEffectorAddsItsWorkingPowerOnTop()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Farm(session, out _, out ComponentInstance effector);
                machine.UpdateComputeUsage(10, 10, 0);

                machine.SetComponentActivity(effector.Id, true);

                Assert.That(machine.CurrentPowerDraw, Is.EqualTo(8.2d - EffectorIdle + EffectorWorking).Within(1e-6),
                    "执行浇水或收获时约 10.9 功率：只把那个效应器从待机换成稳定。");
                Assert.That(machine.IsComponentActive(effector.Id), Is.True);

                machine.SetComponentActivity(effector.Id, false);
                Assert.That(machine.CurrentPowerDraw, Is.EqualTo(8.2d).Within(1e-6));
            }
        }

        [Test]
        public void PerComponentSummingIsNotAMachineWideMultiplier()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Farm(session, out _, out _);

                // 停用一个传感器 + 让核心在跑算法：这就是一台真实的机器里最常见的混合状态。
                Assert.That(machine.SetComponentEnabled(ManagementOrigin.Field, HardwareKind.Sensor, 0, false),
                    Is.EqualTo(MachineManagementResult.Completed));
                machine.UpdateComputeUsage(10, 10, 0);

                // 载体待机 0.1 + 停用传感器 0.2 + 采样传感器 2.0 + 核心 3.0 + 效应器待机 1.1 = 6.4
                double mixed = machine.CurrentPowerDraw;
                Assert.That(mixed, Is.EqualTo(6.4d).Within(1e-6),
                    "混合状态必须逐部件求和，不能被整机倍率抹平。");
                Assert.That(mixed, Is.Not.EqualTo(machine.IdlePowerDraw), "也不是「全部待机」。");
                Assert.That(mixed, Is.Not.EqualTo(0.1d + SensorWorking * 2 + CoreWorking + EffectorWorking),
                    "也不是「全部工作」。");
            }
        }

        [Test]
        public void MovingMachineUsesTheCarrierWorkingPower()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Farm(session, out _, out _);
                machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);

                double idle = machine.CurrentPowerDraw;
                machine.UpdateNavigationActivity(true);

                Assert.That(machine.IsMoving, Is.True);
                Assert.That(machine.CurrentPowerDraw, Is.EqualTo(idle - CarrierIdle + CarrierWorking).Within(1e-6),
                    "只有载体在位：移动时载体换成稳定功率，部件状态不受影响。");
            }
        }

        [Test]
        public void ASleepingMachineNeverUsesWorkingPowerEvenWhenComponentsClaimActivity()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Farm(session, out _, out ComponentInstance effector);
                machine.SetRunState(ManagementOrigin.Field, MachineRunState.Sleeping);
                machine.UpdateComputeUsage(10, 10, 0);
                machine.SetComponentActivity(effector.Id, true);
                machine.UpdateNavigationActivity(true);

                Assert.That(machine.CurrentPowerDraw, Is.EqualTo(machine.IdlePowerDraw).Within(1e-6),
                    "休眠时载体、计算核心、传感器和效应器全部使用各自待机功率（规格原文）。");
            }
        }

        [Test]
        public void DestroyedMachineDrawsNothing()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Farm(session, out _, out _);
                machine.UpdateIntegrity(0d);

                Assert.That(machine.CurrentPowerDraw, Is.Zero);
            }
        }

        [Test]
        public void PowerProfileComesFromTheDataTableNotFromAssumptions()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = FarmMachine(session);

                Assert.That(machine.Definition.IdlePower, Is.EqualTo(CarrierIdle));
                Assert.That(machine.Definition.WorkingPower, Is.EqualTo(CarrierWorking));
                Assert.That(machine.IdlePowerDraw, Is.EqualTo(CarrierIdle).Within(1e-6),
                    "没有装任何部件时，整机耗电就是载体自己的功率。");
            }
        }

        [Test]
        public void DisabledComponentFallsBackToIdlePower()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Farm(session, out _, out _);
                machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);

                // 传感器未启用时不该按「持续采样」计稳定功率。
                Assert.That(machine.SetComponentEnabled(ManagementOrigin.Field, HardwareKind.Sensor, 0, false),
                    Is.EqualTo(MachineManagementResult.Completed));

                Assert.That(machine.CurrentPowerDraw,
                    Is.EqualTo(machine.IdlePowerDraw + (SensorWorking - SensorIdle)).Within(1e-6),
                    "未启用的传感器把那一格退回到待机功率。");
            }
        }

        // ---------------------------------------------------------------- 电网侧的适配

        [Test]
        public void ConsumerRequestsTheMachinesSummedPower()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Farm(session, out _, out _);
                var consumer = new MachineEnergyConsumer(machine, PowerPriority.Production);

                Assert.That(consumer.IsDemandActive, Is.True);
                Assert.That(consumer.Id, Is.EqualTo(machine.Id));
                Assert.That(consumer.RequestedPower, Is.EqualTo((float)machine.CurrentPowerDraw).Within(1e-5f));
                Assert.That(consumer.IdlePower, Is.EqualTo((float)machine.IdlePowerDraw).Within(1e-5f));
            }
        }

        [Test]
        public void ALibraryMachineDoesNotDemandPower()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = FarmMachine(session); // 未部署
                var consumer = new MachineEnergyConsumer(machine, PowerPriority.Production);

                Assert.That(consumer.IsDemandActive, Is.False, "库中机器不在电网里。");
                Assert.That(consumer.RequestedPower, Is.Zero);
            }
        }

        [Test]
        public void FieldPowerSwitchOffMeansNoDemand_NotAShortage()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Farm(session, out _, out _);
                var consumer = new MachineEnergyConsumer(machine, PowerPriority.Production);

                Assert.That(machine.SetPowerSwitch(ManagementOrigin.Field, false),
                    Is.EqualTo(MachineManagementResult.Completed));

                Assert.That(consumer.IsDemandActive, Is.False,
                    "现场断电的机器不请求用电：让它继续请求会被电网当成真实缺口去停别人的电。");
                Assert.That(consumer.RequestedPower, Is.Zero);
            }
        }

        [Test]
        public void UnpoweredMachineStillRequests_SoItCanRecover()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Farm(session, out _, out _);
                machine.UpdateSupply(false);
                var consumer = new MachineEnergyConsumer(machine, PowerPriority.Production);

                Assert.That(machine.Powered, Is.False);
                Assert.That(consumer.RequestedPower, Is.GreaterThan(0f),
                    "断电的机器仍然请求功率——否则它会请求 0 而永远拿不回供电（闭环死锁）。");
            }
        }

        [Test]
        public void ApplySupplyWritesTheGridConclusionBackToTheMachine()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Farm(session, out _, out _);
                var consumer = new MachineEnergyConsumer(machine, PowerPriority.Production);
                machine.UpdateSupply(false);

                consumer.IsPowered = true;
                Assert.That(consumer.ApplySupply(), Is.True);
                Assert.That(machine.SupplyAvailable, Is.True);

                Assert.That(consumer.ApplySupply(), Is.False, "结论没变就不要广播：那会变成一次全量刷新。");
            }
        }

        [Test]
        public void ApplySupplyNeverTouchesTheRegionalSignal()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Farm(session, out _, out _);
                machine.UpdateEnvironment(false, true); // 有信号、没电
                var consumer = new MachineEnergyConsumer(machine, PowerPriority.Production);

                consumer.IsPowered = true;
                consumer.ApplySupply();

                Assert.That(machine.SupplyAvailable, Is.True);
                Assert.That(machine.SignalAvailable, Is.True,
                    "供电与区域信号是两个来源，电网顺手把信号写掉是一个看不出来的越权。");
            }
        }

        [Test]
        public void GridSettlesAMachineLoadFromItsOwnSummedPower()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Farm(session, out _, out _);
                machine.SetRunState(ManagementOrigin.Field, MachineRunState.Sleeping);
                var consumer = new MachineEnergyConsumer(machine, PowerPriority.Production);

                var grid = new EnergyGrid();
                grid.AddGenerator(new FuelGenerator(new PersistentId(9002), 60f, 600f));
                grid.AddConsumer(consumer);
                machine.UpdateSupply(true); // 第一次结算前由外部建立初始供电

                EnergyGridSnapshot snapshot = grid.Tick(60f, daylight: false);

                Assert.That(snapshot.ConsumedPower, Is.EqualTo(1.9f).Within(1e-4f),
                    "电网读到的是机器逐部件求和的结果。");
                Assert.That(consumer.IsPowered, Is.True);
                Assert.That(consumer.ApplySupply(), Is.False, "供电源本来就成立。");

                consumer.IsPowered = false;
                consumer.ApplySupply();
                Assert.That(machine.SupplyAvailable, Is.False);
                Assert.That(machine.Powered, Is.False);
            }
        }

        [Test]
        public void MachinePriorityIsCarriedIntoTheGridsShutdownOrder()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Farm(session, out _, out _);
                var consumer = new MachineEnergyConsumer(machine, PowerPriority.Pausable);
                machine.UpdateSupply(true); // 先有电，缺电停机才有可观察的结论

                var grid = new EnergyGrid();
                grid.AddConsumer(consumer);

                EnergyGridSnapshot snapshot = grid.Tick(60f, daylight: false);

                Assert.That(snapshot.StoppedByShortage, Does.Contain(machine.Id));
                Assert.That(consumer.IsStoppedByShortage, Is.True);
                Assert.That(consumer.ApplySupply(), Is.True);
                Assert.That(machine.SupplyAvailable, Is.False, "缺电停机必须真的落到机器上。");
            }
        }
    }
}
