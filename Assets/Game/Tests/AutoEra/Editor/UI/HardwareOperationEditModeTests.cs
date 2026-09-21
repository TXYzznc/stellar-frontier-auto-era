using System;
using System.Linq;
using System.Reflection;
using AutoEra.Machines;
using AutoEra.UI;
using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 硬件修改请求与显示格式化（规格 17-HardwareConfirm、05-机器整备）。
    ///
    /// 这些断言守的是**分工**，不是字符串好不好看：
    ///   * 请求只能说「哪台机器、哪一格、装还是拆」，不能自己带来源——来源由确认页按机器的
    ///     部署状态推导，因为那正是领域门禁的判据。请求带上来源就会制造「界面说整备、
    ///     领域说现场」的假分歧。
    ///   * 槽位名字与拒绝原因只有一处（<see cref="AutoEraUiFormat"/>），整备页、确认页与
    ///     组件选择器必须说同一句话。
    /// </summary>
    public sealed class HardwareOperationEditModeTests
    {
        private static readonly PersistentId Machine = new PersistentId(1001);

        [Test]
        public void RemoveRequest_DescribesItselfAsTakingOffTheSlotOccupant()
        {
            var request = new AutoEraHardwareRequest(Machine, HardwareKind.Core, 1, PersistentId.Invalid, remove: true);

            Assert.That(request.Remove, Is.True);
            Assert.That(request.MachineId, Is.EqualTo(Machine));
            Assert.That(request.Kind, Is.EqualTo(HardwareKind.Core));
            Assert.That(request.SlotIndex, Is.EqualTo(1));
            Assert.That(request.ComponentId.IsValid, Is.False,
                "拆下时没有要装上去的组件，身份必须是 Invalid 而不是随便一个 Id。");
            StringAssert.Contains("核心槽 1", request.Intent);
            StringAssert.Contains("拆下", request.Intent);
        }

        [Test]
        public void InstallRequest_DescribesItselfAsPuttingTheComponentIn()
        {
            var component = new PersistentId(2002);
            var request = new AutoEraHardwareRequest(Machine, HardwareKind.Sensor, 0, component, remove: false);

            Assert.That(request.Remove, Is.False);
            Assert.That(request.ComponentId, Is.EqualTo(component));
            StringAssert.Contains("传感器槽 0", request.Intent);
            StringAssert.Contains("装入", request.Intent);
        }

        [Test]
        public void Request_DeliberatelyCarriesNoOrigin()
        {
            PropertyInfo origin = typeof(AutoEraHardwareRequest)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p => p.Name.IndexOf("Origin", StringComparison.OrdinalIgnoreCase) >= 0);

            Assert.That(origin, Is.Null,
                "请求不得携带来源：来源由确认页按机器部署状态推导（未部署→整备环境，已部署→现场），"
                + "那正是领域门禁的判据；让调用方传一个可能与机器状态矛盾的来源只会制造假分歧。");
        }

        [Test]
        public void SlotNames_AreStableAcrossEveryHardwareKind()
        {
            Assert.That(AutoEraUiFormat.Slot(HardwareKind.Sensor, 2), Is.EqualTo("传感器槽 2"));
            Assert.That(AutoEraUiFormat.Slot(HardwareKind.Core, 0), Is.EqualTo("核心槽 0"));
            Assert.That(AutoEraUiFormat.Slot(HardwareKind.Effector, 3), Is.EqualTo("执行器槽 3"));
        }

        [TestCase(MachineManagementResult.InvalidOrigin)]
        [TestCase(MachineManagementResult.InvalidSlot)]
        [TestCase(MachineManagementResult.Occupied)]
        [TestCase(MachineManagementResult.MissingComponent)]
        [TestCase(MachineManagementResult.WaitingForSafeStop)]
        public void RejectedResults_AllHaveADistinctReadableReason(MachineManagementResult result)
        {
            string text = AutoEraUiFormat.ManagementResult(result);

            Assert.That(text, Is.Not.Null.And.Not.Empty);
            Assert.That(text, Is.Not.EqualTo(AutoEraUiFormat.Missing));
            // 拒绝原因必须说人话：把枚举名直接端给玩家等于没解释。
            Assert.That(text, Does.Not.Contain(result.ToString()));
        }

        [Test]
        public void EveryRejectionHasItsOwnText_SoThePageCanNeverLieAboutWhy()
        {
            MachineManagementResult[] results =
            {
                MachineManagementResult.InvalidOrigin,
                MachineManagementResult.InvalidSlot,
                MachineManagementResult.Occupied,
                MachineManagementResult.AlreadyInstalled,
                MachineManagementResult.MissingComponent,
                MachineManagementResult.CapacityInUse,
                MachineManagementResult.ComputeInUse,
                MachineManagementResult.LogicCapacityInUse,
                MachineManagementResult.InvalidState,
            };

            string[] texts = results.Select(AutoEraUiFormat.ManagementResult).ToArray();
            Assert.That(texts.Distinct().Count(), Is.EqualTo(results.Length),
                "两种不同的拒绝原因不能显示成同一句话。");
        }
    }
}
