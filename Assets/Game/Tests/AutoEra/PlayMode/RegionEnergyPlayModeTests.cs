using System;
using System.Collections;
using System.Collections.Generic;
using AutoEra.Application;
using AutoEra.Energy;
using AutoEra.Machines;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Region;
using GameFramework.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace AutoEra.Tests.PlayMode
{
    /// <summary>
    /// 初始基地的供电端到端：**场景预制体上声明的设施 → 区域电网 → 机器的供电状态**。
    ///
    /// 这一段要证明的是「电网真的有人用」：在批次 13／14 里，结算器与机器负载都算得对，
    /// 但没有任何地方建过一张电网——机器有没有电仍然由测试手动 `UpdateEnvironment` 决定。
    /// 这里从**场景预制体**出发（生物质发电机 ＋ 太阳能阵列，规格 06「第一版能源范围」写的
    /// 「初始基地已经建有一台基础生物质发电机和一台低功率太阳能发电器」），
    /// 走完「区域推进 → 结算 → 写回机器」整条链。
    ///
    /// 夜间那一段特别重要：它同时验证了日照周期真的在改环境能源的出力，
    /// 而机器**没有因此断电**——因为生物质发电机接上了缺口。
    ///
    /// 第 ④ 步把这条链接到界面：中枢的能源系统详情页读的必须是**同一份结算快照**
    /// （界面自己再算一遍是这一页最容易犯的错），而规格允许的两个写入口
    /// （燃料设施的充电许可与目标储电比例）必须经过「配置选中发电设施」这一次提交
    /// 真的落回设施——字段控件只改草稿，见 04-基地中枢/HubEnergy.md。
    /// </summary>
    public sealed class RegionEnergyPlayModeTests
    {
        private const string LaunchSceneName = "Launch";
        private const string RegionSceneName = "InitialRegion";

        [UnityTest]
        public IEnumerator InitialBase_PowersDeployedMachines_AndKeepsThemRunningAtNight()
        {
            yield return EnsureLaunchSceneLoaded();
            yield return WaitForRuntimeReady();

            double until = Time.realtimeSinceStartupAsDouble + 30;
            while (!MachineCatalog.IsGameDataLoaded && Time.realtimeSinceStartupAsDouble < until) yield return null;
            Assert.That(MachineCatalog.IsGameDataLoaded, Is.True, "机器数据表必须在运行期就绪。");

            AsyncOperation loading = SceneManager.LoadSceneAsync(RegionSceneName, LoadSceneMode.Additive);
            Assert.That(loading, Is.Not.Null, "InitialRegion 必须在 Build Settings 里启用。");
            while (!loading.isDone) yield return null;
            Scene scene = SceneManager.GetSceneByName(RegionSceneName);
            InitialRegionScene entry = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.TryGetComponent(out InitialRegionScene found)) entry = found;
            }

            Assert.That(entry, Is.Not.Null, "现场场景必须有 InitialRegionScene 入口。");

            using (AutoEraApplicationContext context = new AutoEraApplicationCompositionRoot().Create())
            {
                try
                {
                    Assert.That(context.TryCreateWorldSession(0, out AutoEraWorldSession session), Is.True);
                    bool ready = false;
                    string failure = null;
                    entry.InitializeRuntime(session, () => ready = true, e => failure = e);
                    until = Time.realtimeSinceStartupAsDouble + 40;
                    while (!ready && failure == null && Time.realtimeSinceStartupAsDouble < until) yield return null;
                    Assert.That(failure, Is.Null);
                    Assert.That(ready, Is.True);

                    // ① 场景预制体上声明的两台设施被区域接住了。
                    RegionEnergyService energy = entry.Energy;
                    Assert.That(energy, Is.Not.Null,
                        "初始基地声明了发电设施，区域就必须建起电网——否则机器只能靠外部喂 bool 才有电。");
                    Assert.That(energy.HasSupply, Is.True);
                    Assert.That(energy.FacilityCount, Is.EqualTo(2),
                        "初始基地是「一台基础生物质发电机 ＋ 一台低功率太阳能发电器」。");

                    // ② 部署一台机器：它应当被电网接管，并从基地拿到供电。
                    MachineCatalog catalog = MachineCatalog.FromLoadedGameData();
                    Assert.That(catalog.TryGetMachine(10011, out MachineDefinition wheeled), Is.True);
                    MachineInstance machine = session.Machines.Create(wheeled);
                    Assert.That(entry.Region.DeployMachine(machine.Id, new Vector2(20, -25), wheeled.Footprint, out _),
                        Is.EqualTo(RegionMachineDeploymentResult.Bound));
                    // 区域部署可能已经把机器标成已部署（生产路径由流程统一做这两件事），
                    // 所以这里按状态补齐，而不是无脑再调一次——重复部署会被花名册判为 InvalidState。
                    if (!machine.Deployed) session.Machines.Deploy(machine.Id);
                    Assert.That(machine.Deployed, Is.True);

                    for (int i = 0; i < 5; i++) entry.Advance(0.2);
                    yield return null;

                    Assert.That(energy.Tracks(machine.Id), Is.True, "已部署的机器必须进入本区域电网。");
                    Assert.That(machine.SupplyAvailable, Is.True,
                        "白天 15 功率足够带一台待机机器；拿不到供电就说明基地根本没接上。");
                    Assert.That(machine.Powered, Is.True);
                    Assert.That(energy.Snapshot.GeneratedPower, Is.GreaterThan(0f));
                    Assert.That(energy.Snapshot.ConsumedPower,
                        Is.EqualTo((float)machine.CurrentPowerDraw).Within(0.01f),
                        "电网读到的负载就是机器逐部件求和的结果。");

                    // ③ 跑到夜间：太阳能归零，生物质发电机接上缺口，机器不能因此断电。
                    float fuelBeforeNight = 0f;
                    Assert.That(entry.TrySetDevelopmentTimeMultiplier(600d), Is.True);
                    for (int i = 0; i < 4; i++) entry.Advance(1.0); // 600 倍速 → 约 40 分钟世界时间
                    yield return null;

                    Assert.That(entry.WorldMilliseconds, Is.GreaterThan(17L * 60L * 1000L), "应当已经入夜。");
                    Assert.That(DaylightCycle.IsDaylight(entry.WorldMilliseconds), Is.False);
                    Assert.That(energy.Snapshot.GeneratedPower, Is.GreaterThan(0f),
                        "入夜后太阳能归零，但燃料发电机必须补上缺口。");
                    Assert.That(machine.SupplyAvailable, Is.True, "夜里机器仍然要有电。");

                    fuelBeforeNight = energy.Snapshot.StoredCharge; // 这里没有储能，恒为 0；留作台账
                    Assert.That(fuelBeforeNight, Is.Zero, "初始基地没有蓄电设施——规格把它留到蓄电任务之后。");

                    // ④ 中枢能源页：读的是**同一份结算快照**，写的是**同一台设施**。
                    //    这一步要证明的不是「界面能打开」，而是「界面没有自己再算一遍」与
                    //    「规格允许的那两个写入口真的落回了领域」——否则能源页仍是一张死页。
                    int hubId = AutoEraUiNavigator.Open(UIViews.BaseCommandHubForm,
                        AutoEraUiSession.ForWorld(context, session, entry.Region, null, null, energy));
                    Assert.That(hubId, Is.GreaterThan(AutoEraUiNavigator.InvalidSerialId));
                    yield return WaitForForm(hubId, expectedLoaded: true);

                    var hub = GF.UI.GetUIForm(hubId).Logic as BaseCommandHubForm;
                    Assert.That(hub, Is.Not.Null, "中枢界面必须挂上它的 Logic。");
                    Assert.That(hub.ShowHubPage(BaseCommandHubForm.PageEnergy), Is.True,
                        "能源系统详情是 Form 内的真实页，必须能切过去。");
                    yield return null;

                    Assert.That(hub.EnergyDataState, Is.EqualTo(UiDataState.Ready),
                        "区域电网已经活着，能源页就该显示为可读——否则说明会话里的电网没有透传进界面。");
                    Assert.That(hub.EnergyFacilityCount, Is.EqualTo(2));
                    Assert.That(hub.EnergyConsumerCount, Is.EqualTo(1), "已部署的那台机器必须是本页的用电对象。");
                    // 有真实数据时，覆盖在内容区上的状态卡片必须全部收起：
                    // 它们是不透明卡片，点亮等于把内置占位文案（「Success：—」）盖在供需数字上。
                    Assert.That(hub.HubEnergySuccessState.activeSelf, Is.False,
                        "读一次快照不是提交，不得点亮成功卡片（规格：未发提交不伪造 success）。");
                    Assert.That(hub.HubEnergyDisabledState.activeSelf, Is.False);
                    Assert.That(hub.HubEnergyEmptyState.activeSelf, Is.False);
                    Assert.That(hub.HubEnergyFacilitiesContent, Is.Not.Null, "设施列表容器未绑进契约。");
                    Assert.That(hub.HubEnergyFacilitiesContent.childCount, Is.GreaterThan(1),
                        "设施列表必须真的渲染出行（对象池实例），而不是只留模板。");

                    // 燃料设施那一行：规格说只有燃料发电设施开放充电许可与目标储电比例。
                    RegionEnergyFacility fuelFacility = null;
                    IReadOnlyList<RegionEnergyFacility> declared = energy.Facilities;
                    for (int i = 0; i < declared.Count; i++)
                    {
                        if (declared[i].SupportsChargingPolicy) fuelFacility = declared[i];
                    }

                    Assert.That(fuelFacility, Is.Not.Null, "初始基地必须有一台燃料发电设施。");

                    Button fuelRow = FindFacilityRow(hub.HubEnergyFacilitiesContent, "燃料发电");
                    Assert.That(fuelRow, Is.Not.Null, "设施列表里必须有一行是燃料发电设施。");

                    Assert.That(hub.HubEnergyConfigureButton.interactable, Is.False,
                        "还没有选中设施时，配置按钮没有内容可提交，必须是禁用的而不是按了没反应。");

                    fuelRow.onClick.Invoke();
                    yield return null;

                    Assert.That(hub.HubEnergyChargingAllowedToggle.interactable, Is.True,
                        "选中燃料设施后充电许可必须可点（规格：仅燃料设施开放）。");
                    Assert.That(hub.HubEnergyChargeTargetSlider.interactable, Is.True);
                    Assert.That(hub.HubEnergyConfigureButton.interactable, Is.False,
                        "刚刚选中、还没有改动时，配置按钮仍然没有内容可提交。");

                    // 字段控件只改草稿：这是「字段控制与确认按钮职责分离」的可观察证据。
                    bool allowedBefore = fuelFacility.Generator.AllowsCharging;
                    hub.HubEnergyChargingAllowedToggle.isOn = !allowedBefore;
                    hub.HubEnergyChargeTargetSlider.value = 0.75f;

                    Assert.That(fuelFacility.Generator.AllowsCharging, Is.EqualTo(allowedBefore),
                        "拨动开关不得直接写进领域——写入由配置按钮那一次提交完成。");
                    Assert.That(hub.HubEnergyConfigureButton.interactable, Is.True,
                        "有未提交的修改时，配置按钮必须变成可点。");

                    hub.HubEnergyConfigureButton.onClick.Invoke();
                    yield return null;

                    Assert.That(fuelFacility.Generator.AllowsCharging, Is.EqualTo(!allowedBefore),
                        "提交必须真的落到领域。");
                    Assert.That(fuelFacility.Generator.ChargeTargetRatio, Is.EqualTo(0.75f).Within(1e-3f));
                    Assert.That(hub.HubEnergyConfigureButton.interactable, Is.False,
                        "提交成功后草稿清空，按钮回到「无内容可提交」的状态。");

                    // ⑤ 让基地真的缺电：烧光燃料发电设施的燃料，夜里没有太阳能，
                    //    那台机器就会被停机。这一步验证的是「电网的状态跨越被记进了事件日志」，
                    //    以及「中枢的查看能源事件入口真的打开了能源停机记录页并读到那些记录」——
                    //    在这之前，能源历史是一句「没有数据来源」的说明。
                    //    为什么烧燃料而不是关发电机：`IEnergyGenerator.IsOn` 是只读的
                    //    （开关属于现场操作，界面也只读），而燃料是设施侧可写的事实。
                    fuelFacility.Generator.FuelEnergyAvailable = 0f;

                    //    等**回到夜里**再断言：上一步把世界推到了夜里，但 600 倍速下
                    //    界面打开的那几帧也在推进世界时间，很可能已经跨过午夜进入第二个白天。
                    //    所以先按「是否日照」用小步推进（0.05 秒 ≈ 0.5 世界分钟）收敛到刚入夜，
                    //    再把倍速降回 1 倍——否则紧接着的那一帧 `yield return null` 会一次吃掉
                    //    10 世界分钟，把世界推过午夜、重新变成白天，太阳能又把缺口补上。
                    for (int i = 0; i < 200 && DaylightCycle.IsDaylight(entry.WorldMilliseconds); i++)
                    {
                        entry.Advance(0.05);
                    }

                    //    入夜之后还要再推几拍：上面的循环是**一入夜就退出**的，
                    //    如果退出时这一帧正好是「刚变暗但电网还没在这个状态下结算」，
                    //    机器的供电状态仍然是上一个白天结算留下的结果。
                    //    夜长 8 世界分钟，这里 4 × 0.5 分钟仍在夜里，不会跨过午夜。
                    for (int i = 0; i < 4; i++)
                    {
                        entry.Advance(0.05);
                    }

                    Assert.That(entry.TrySetDevelopmentTimeMultiplier(1d), Is.True);
                    yield return null;

                    Assert.That(DaylightCycle.IsDaylight(entry.WorldMilliseconds), Is.False,
                        "这一步必须发生在夜里，否则太阳能会把缺口补上。");
                    Assert.That(machine.SupplyAvailable, Is.False,
                        "没有发电来源时，机器应当因缺电停机（这正是要记录的事件）。");
                    Assert.That(energy.Snapshot.StoppedByShortage, Does.Contain(machine.Id));

                    Assert.That(hub.HubEnergyHistoryButton, Is.Not.Null,
                        "Btn_HubEnergyHistory 必须绑进契约（能源页的记录入口）。");
                    hub.HubEnergyHistoryButton.onClick.Invoke();

                    RecordReaderForm reader = null;
                    yield return WaitForLogic<RecordReaderForm>(found => reader = found, "RecordReaderForm");
                    yield return null;

                    Assert.That(reader.CurrentPage, Is.EqualTo(RecordReaderForm.PageEnergyHistory),
                        "从能源页的记录入口进入，应当落在能源停机记录页。");
                    Assert.That(reader.EventDataState, Is.EqualTo(UiDataState.Ready),
                        $"事件域应可读，实际={reader.EventDataState}，记录数={reader.EventRecordCount}");
                    Assert.That(reader.EnergyHistoryListTemplate, Is.Not.Null,
                        "Item 模板未绑进契约——渲染会在第一行提前返回。");
                    Assert.That(reader.EnergyHistoryListContent, Is.Not.Null, "Content 未绑进契约。");
                    Assert.That(reader.EnergyHistoryListContent.childCount, Is.GreaterThan(1),
                        "刚才那次缺电停机必须真的出现在能源历史里，而不是只留模板。");

                    // 选中那一条 → 详情区必须给出规格要求的事件详情（对象／状态／持续时长／原因／影响）。
                    Button energyRow = FindRowButton(reader.EnergyHistoryListContent, "Btn_EnergyHistoryListRow");
                    Assert.That(energyRow, Is.Not.Null, "能源历史必须渲染出可点选的行。");
                    energyRow.onClick.Invoke();
                    yield return null;

                    Assert.That(reader.EnergyHistoryDetailContent, Is.Not.Null, "事件详情容器未绑进契约。");
                    Assert.That(reader.EnergyHistoryDetailContent.childCount, Is.GreaterThan(1),
                        "选中一条能源事件后，详情区必须给出可读的字段。");

                    AutoEraUiNavigator.Close(hubId);
                    yield return WaitForForm(hubId, expectedLoaded: false);
                }
                finally
                {
                    entry.Release();
                    context.ReleaseActiveWorldSession();
                }
            }

            yield return SceneManager.UnloadSceneAsync(scene);
        }

        /// <summary>等一个类型的界面 Logic 出现（入口按钮走导航服务，打开不是当帧完成的）。</summary>
        private static IEnumerator WaitForLogic<T>(Action<T> onFound, string what) where T : UIFormLogic
        {
            const int maxFrames = 300;
            for (int frame = 0; frame < maxFrames; frame++)
            {
                foreach (IUIGroup group in GF.UI.GetAllUIGroups())
                {
                    foreach (IUIForm form in group.GetAllUIForms())
                    {
                        // GameFramework 的 IUIForm 不暴露 Logic；运行期实现是 UnityGameFramework 的 UIForm。
                        if (form is UIForm runtimeForm && runtimeForm.Logic is T typed)
                        {
                            onFound(typed);
                            yield break;
                        }
                    }
                }

                yield return null;
            }

            Assert.Fail($"{what} 在 {maxFrames} 帧内没有被打开——入口按钮可能没有接上导航服务。");
        }

        /// <summary>在内容区里按节点名找第一个行按钮（行由对象池渲染，节点名保留预制体里的那一个）。</summary>
        private static Button FindRowButton(Transform content, string nodeName)
        {
            Button[] buttons = content.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].name == nodeName) return buttons[i];
            }

            return null;
        }

        /// <summary>
        /// 等界面装载／卸载到位。界面的生命周期是 GF 的（SpawnItem 与 OnOpen 都不在同一帧完成），
        /// 所以断言之前必须先把「已经打开」这件事等出来，而不是靠猜帧数。
        /// </summary>
        private static IEnumerator WaitForForm(int serialId, bool expectedLoaded)
        {
            const int maxFrames = 300;
            for (int frame = 0; frame < maxFrames; frame++)
            {
                if (GF.UI.HasUIForm(serialId) == expectedLoaded)
                {
                    yield break;
                }

                yield return null;
            }

            Assert.Fail($"UI form serial {serialId} did not reach loaded={expectedLoaded} within 300 frames.");
        }

        /// <summary>
        /// 在设施列表里按行文案找那一行按钮（行由对象池渲染，节点名保留预制体里的
        /// <c>Btn_HubEnergyFacilitiesRow</c>，标签是本行的「名称（类别）」）。
        /// 不按序号取行：设施顺序由场景声明顺序决定，写死序号会在内容调整后假失败。
        /// </summary>
        private static Button FindFacilityRow(Transform content, string labelFragment)
        {
            Button[] buttons = content.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].name != "Btn_HubEnergyFacilitiesRow") continue;
                TMP_Text[] texts = buttons[i].GetComponentsInChildren<TMP_Text>(true);
                for (int t = 0; t < texts.Length; t++)
                {
                    if (!string.IsNullOrEmpty(texts[t].text) && texts[t].text.Contains(labelFragment))
                    {
                        return buttons[i];
                    }
                }
            }

            return null;
        }

        private static IEnumerator EnsureLaunchSceneLoaded()
        {
            if (SceneManager.GetActiveScene().name == LaunchSceneName)
            {
                yield break;
            }

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(LaunchSceneName, LoadSceneMode.Single);
            Assert.That(loadOperation, Is.Not.Null, "Launch 必须在 Build Settings 里启用。");
            yield return loadOperation;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(LaunchSceneName));
            yield return null;
        }

        private static IEnumerator WaitForRuntimeReady()
        {
            const int maxFrames = 600;
            for (int frame = 0; frame < maxFrames; frame++)
            {
                if (GF.UI != null && GF.DataTable != null &&
                    GF.DataTable.HasDataTable<UITable>() &&
                    GF.DataTable.HasDataTable<UIGroupTable>() &&
                    GF.UI.HasUIGroup("Default"))
                {
                    yield break;
                }

                yield return null;
            }

            Assert.Fail("GF UI runtime or required UI data tables did not become ready within 600 frames.");
        }
    }
}
