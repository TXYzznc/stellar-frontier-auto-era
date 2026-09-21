using AutoEra.Machines;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 一键卸下全部（规格 05-机器整备 `Btn_MachinePreparationUnload`：「确认全部卸下影响并原子回库」）。
    ///
    /// 这一批的**唯一**硬要求就是「原子」两个字，而它最容易在实现里丢掉：
    /// 最自然的写法是「遍历每个槽位，逐件调 Remove」——那会在中途遇到余量不足时失败，
    /// 留下一台「卸了一半」的机器；玩家看到的是三件里少了一件，而没有任何一处告诉他为什么。
    /// 所以这里的用例守着三件事：
    ///   ① **全有或全无**（一件卸不下就一件都不动），
    ///   ② 它是**整备环境**的动作（已部署机器走现场逐项修改，不是这个入口），
    ///   ③ 卸下的组件**归属被清空**（于是它们回到组件库成为散件——不需要另一套库存系统）。
    ///
    /// 另一条边界同样重要：**一件都没装时不是「成功」**。把空机器报告成卸下成功，
    /// 会让界面上的成功状态变成一句没有内容的话。
    /// </summary>
    public sealed class MachineUnloadAllEditModeTests
    {
        private const int BaseCapacity = 10;

        /// <summary>三类槽位各 1 格、基础容量 10 的载体。</summary>
        private static MachineInstance Machine(AutoEraWorldSession session)
            => session.Machines.Create(new MachineDefinition(1, "Fixture", 1, 1, 1, 1, BaseCapacity, true, true, 100, 2d, 3d));

        private static ComponentInstance Component(AutoEraWorldSession session, HardwareKind kind, int id,
            int addedCapacity = 0, int compute = 0, int logic = 0)
            => session.Machines.CreateComponent(new ComponentDefinition(id, kind, 1, addedCapacity, compute, logic, false));

        private static void Install(AutoEraWorldSession session, MachineInstance machine, ComponentInstance component, int index)
            => Assert.That(session.Machines.Install(machine.Id, ManagementOrigin.Library, component.Id, index),
                Is.EqualTo(MachineManagementResult.Completed));

        [Test]
        public void UnloadAllRequest_SaysItTakesEverything_AndCarriesNoSlot()
        {
            var request = AutoEraHardwareRequest.UnloadAll(new PersistentId(1001));

            Assert.That(request.RemoveAll, Is.True);
            Assert.That(request.Remove, Is.True, "一键卸下是拆下方向，不是装入。");
            Assert.That(request.SlotIndex, Is.EqualTo(AutoEraHardwareRequest.AllSlots),
                "「全部」不是某一格：填一个 0 号槽会让确认页显示出一个并不存在的目标槽位。");
            Assert.That(request.ComponentId.IsValid, Is.False);
            StringAssert.Contains("一键卸下", request.Intent);
            StringAssert.Contains("全部", request.Intent);
        }

        [Test]
        public void SingleSlotRequest_IsNotMistakenForUnloadAll()
        {
            var request = new AutoEraHardwareRequest(new PersistentId(1001), HardwareKind.Core, 0,
                PersistentId.Invalid, remove: true);

            Assert.That(request.RemoveAll, Is.False, "0 号槽是合法槽位，不能被当成「全部」的哨兵。");
            StringAssert.DoesNotContain("一键卸下", request.Intent);
        }

        [Test]
        public void RemoveAll_TakesEveryInstalledComponentOffInOneGo()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Machine(session);
                ComponentInstance sensor = Component(session, HardwareKind.Sensor, 201);
                ComponentInstance core = Component(session, HardwareKind.Core, 202);
                ComponentInstance effector = Component(session, HardwareKind.Effector, 203);
                Install(session, machine, sensor, 0);
                Install(session, machine, core, 0);
                Install(session, machine, effector, 0);
                Assert.That(machine.InstalledComponentCount, Is.EqualTo(3));

                Assert.That(session.Machines.RemoveAll(machine.Id, ManagementOrigin.Library),
                    Is.EqualTo(MachineManagementResult.Completed));

                for (int k = 0; k < 3; k++)
                {
                    Assert.That(machine.GetComponent((HardwareKind)k, 0), Is.Null, "每个类别都被清空。");
                }

                Assert.That(machine.InstalledComponentCount, Is.Zero);

                // 「回库」在实现里就是**清空归属**：组件因此重新成为组件库里的散件。
                // 如果这里还给一个有效 OwnerId，玩家就会看到一件既不在机器上、也不在库里的组件。
                Assert.That(sensor.OwnerId.IsValid, Is.False);
                Assert.That(core.OwnerId.IsValid, Is.False);
                Assert.That(effector.OwnerId.IsValid, Is.False);
            }
        }

        [Test]
        public void RemoveAll_LeavesTheMachineItselfIntact()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Machine(session);
                ComponentInstance core = Component(session, HardwareKind.Core, 202, compute: 4, logic: 2);
                Install(session, machine, core, 0);
                double integrity = machine.Integrity;

                session.Machines.RemoveAll(machine.Id, ManagementOrigin.Library);

                Assert.That(session.Machines.TryGet(machine.Id, out MachineInstance still), Is.True,
                    "卸下组件不等于销毁载体。");
                Assert.That(still, Is.SameAs(machine));
                Assert.That(machine.Integrity, Is.EqualTo(integrity));
                Assert.That(machine.TotalCapacity, Is.EqualTo(BaseCapacity), "只剩基础容量。");
                Assert.That(machine.ComputeCapacity, Is.Zero);
                Assert.That(machine.LogicCapacity, Is.Zero);
            }
        }

        [Test]
        public void RemoveAll_IsAtomic_WhenTheContainerWouldNotFitNothingIsRemoved()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Machine(session);
                // 只有执行器能加容量（组件定义的校验就是这么定的），所以扩容件是执行器。
                ComponentInstance expander = Component(session, HardwareKind.Effector, 203, addedCapacity: 5);
                ComponentInstance core = Component(session, HardwareKind.Core, 202);
                Install(session, machine, expander, 0);
                Install(session, machine, core, 0);

                // 容器里装着 12 件，而全部卸下之后只剩基础容量 10 —— 逐件卸会卸掉第一件、
                // 在第二件上以 CapacityInUse 失败，留下半卸状态。原子实现必须在这里**整体拒绝**。
                machine.UpdateContainerUsage(12);

                Assert.That(session.Machines.RemoveAll(machine.Id, ManagementOrigin.Library),
                    Is.EqualTo(MachineManagementResult.CapacityInUse));

                Assert.That(machine.GetComponent(HardwareKind.Effector, 0), Is.SameAs(expander),
                    "被拒绝时必须一件都不动。");
                Assert.That(machine.GetComponent(HardwareKind.Core, 0), Is.SameAs(core));
                Assert.That(expander.OwnerId, Is.EqualTo(machine.Id));
            }
        }

        [Test]
        public void RemoveAll_IsAtomic_WhenComputeOrLogicIsStillReserved()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Machine(session);
                ComponentInstance core = Component(session, HardwareKind.Core, 202, compute: 6, logic: 3);
                ComponentInstance effector = Component(session, HardwareKind.Effector, 203);
                Install(session, machine, core, 0);
                Install(session, machine, effector, 0);

                // 算力被占着：卸掉核心会让占用无处安放。
                machine.UpdateComputeUsage(2, 0);
                Assert.That(session.Machines.RemoveAll(machine.Id, ManagementOrigin.Library),
                    Is.EqualTo(MachineManagementResult.ComputeInUse));
                Assert.That(machine.InstalledComponentCount, Is.EqualTo(2), "被拒绝时一件都不动。");

                // 逻辑容量被占着同理。
                machine.UpdateComputeUsage(0, 1);
                Assert.That(session.Machines.RemoveAll(machine.Id, ManagementOrigin.Library),
                    Is.EqualTo(MachineManagementResult.LogicCapacityInUse));
                Assert.That(machine.InstalledComponentCount, Is.EqualTo(2));
            }
        }

        [Test]
        public void RemoveAll_WithNothingInstalled_IsRejectedInsteadOfReportedAsSuccess()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Machine(session);

                Assert.That(session.Machines.RemoveAll(machine.Id, ManagementOrigin.Library),
                    Is.EqualTo(MachineManagementResult.MissingComponent),
                    "空机器上报「卸下成功」会让界面上的成功状态变成一句没有内容的话。");
            }
        }

        [Test]
        public void RemoveAll_BelongsToThePreparationEnvironment_NotTheField()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Machine(session);
                ComponentInstance core = Component(session, HardwareKind.Core, 202);
                Install(session, machine, core, 0);
                session.Machines.Deploy(machine.Id);

                // 规格 05：一键卸下属于「未部署机器处于整备环境」的能力；
                // 已部署机器的硬件修改必须现场逐项完成，不是这个入口。
                Assert.That(session.Machines.RemoveAll(machine.Id, ManagementOrigin.Field),
                    Is.EqualTo(MachineManagementResult.InvalidOrigin));
                Assert.That(machine.GetComponent(HardwareKind.Core, 0), Is.SameAs(core));

                Assert.That(session.Machines.RemoveAll(machine.Id, ManagementOrigin.Library),
                    Is.EqualTo(MachineManagementResult.InvalidOrigin));
                Assert.That(machine.GetComponent(HardwareKind.Core, 0), Is.SameAs(core),
                    "来源不对时必须一件都不动。");
            }
        }

        [Test]
        public void RemoveAll_WaitsForSafeStop_InsteadOfTearingHardwareOutFromUnderABehavior()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Machine(session);
                ComponentInstance core = Component(session, HardwareKind.Core, 202);
                Install(session, machine, core, 0);
                machine.UpdateBehaviorActivity(true);

                Assert.That(session.Machines.RemoveAll(machine.Id, ManagementOrigin.Library),
                    Is.EqualTo(MachineManagementResult.WaitingForSafeStop));
                Assert.That(machine.GetComponent(HardwareKind.Core, 0), Is.SameAs(core),
                    "等到安全点之前不得动硬件。");

                machine.UpdateBehaviorActivity(false);
                Assert.That(session.Machines.RemoveAll(machine.Id, ManagementOrigin.Library),
                    Is.EqualTo(MachineManagementResult.Completed));
                Assert.That(machine.InstalledComponentCount, Is.Zero);
            }
        }

        [Test]
        public void RemoveAll_UnknownMachine_IsAnInvalidState_NotACrash()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                Assert.That(session.Machines.RemoveAll(new PersistentId(999999), ManagementOrigin.Library),
                    Is.EqualTo(MachineManagementResult.InvalidState));
            }
        }

        [Test]
        public void Operation_RemoveAll_GoesThroughTheSameCoordinatorAsASingleSlotRemoval()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Machine(session);
                ComponentInstance core = Component(session, HardwareKind.Core, 202);
                ComponentInstance effector = Component(session, HardwareKind.Effector, 203);
                Install(session, machine, core, 0);
                Install(session, machine, effector, 0);

                MachineHardwareOperation operation = session.Machines.GetHardwareOperation(machine.Id);
                Assert.That(operation.BeginRemoveAll(machine.Id, ManagementOrigin.Library), Is.True);

                Assert.That(operation.State, Is.EqualTo(HardwareOperationState.Completed));
                Assert.That(operation.Result, Is.EqualTo(MachineManagementResult.Completed));
                Assert.That(machine.InstalledComponentCount, Is.Zero,
                    "提交之后由执行器完成整台清空。");
                Assert.That(core.OwnerId.IsValid, Is.False);
            }
        }

        [Test]
        public void Operation_RemoveAll_IsRejectedByTheSameOriginGate()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Machine(session);
                ComponentInstance core = Component(session, HardwareKind.Core, 202);
                Install(session, machine, core, 0);

                MachineHardwareOperation operation = session.Machines.GetHardwareOperation(machine.Id);
                operation.BeginRemoveAll(machine.Id, ManagementOrigin.Hub);

                Assert.That(operation.State, Is.EqualTo(HardwareOperationState.Rejected));
                Assert.That(operation.Result, Is.EqualTo(MachineManagementResult.InvalidOrigin),
                    "中枢不能远程停机改硬件——这条门禁对一键卸下同样成立。");
                Assert.That(machine.InstalledComponentCount, Is.EqualTo(1));
            }
        }

        [Test]
        public void Operation_CancellingAnUncommittedUnloadAll_NeverTouchesTheHardware()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineInstance machine = Machine(session);
                ComponentInstance core = Component(session, HardwareKind.Core, 202);
                Install(session, machine, core, 0);
                machine.UpdateBehaviorActivity(true);

                MachineHardwareOperation operation = session.Machines.GetHardwareOperation(machine.Id);
                operation.BeginRemoveAll(machine.Id, ManagementOrigin.Library);
                Assert.That(operation.State, Is.EqualTo(HardwareOperationState.Waiting),
                    "有进行中的行为时先等安全停机。");

                Assert.That(operation.Cancel(operation.RequestVersion), Is.True);
                Assert.That(operation.State, Is.EqualTo(HardwareOperationState.Cancelled));
                Assert.That(machine.GetComponent(HardwareKind.Core, 0), Is.SameAs(core),
                    "取消一个还没提交的意图不会撤销任何东西——因为本来就什么都没做。");
            }
        }
    }
}
