using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoEra.Application;
using AutoEra.Events;
using AutoEra.Machines;
using AutoEra.Save;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using AutoEra.World.Time;
using GameFramework.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityGameFramework.Runtime;

namespace AutoEra.Tests.PlayMode
{
    /// <summary>
    /// 界面导航链的运行期验证：按 00-页面关系与复用 的入口表，按钮真的能把玩家带到目标界面，
    /// 并把本页拿到的会话透传下去。
    ///
    /// 与 <c>AutoEraAllUiFormsPlayModeTests</c> 的分工：那个测试回答「每个界面单独能否打开」，
    /// 本测试回答「界面之间是否连得起来」——后者才是「界面接进游戏」的实际判据。
    ///
    /// 会话透传不靠读私有字段证明，而是靠**界面真的显示出了世界里的数据**：中枢只有拿到
    /// 带世界会话的打开参数，才可能把测试预置的那台机器列出来。
    ///
    /// 三条纪律（都是实测踩出来的）：
    /// ① 两条链写在同一个 [UnityTest] 里：Launch 场景的产品流程对场景重载很敏感，
    ///    反复加载会命中 GFBuiltin.Start 访问已销毁 UIComponent 的残留问题；
    /// ② Launch 场景里产品流程同时在跑，收尾只允许关闭「本测试新增的」界面，否则会关掉
    ///    流程持有的界面，让流程在 OnLeave 里抛 GameFrameworkException；
    /// ③ Logic 挂上不等于 OnOpen 跑完——读界面内容前必须等外壳完成首次页选择。
    /// </summary>
    public sealed class AutoEraUiNavigationPlayModeTests
    {
        private AutoEraApplicationContext _context;
        private string _saveRoot;
        private InitialRegion _region;

        [UnityTest]
        public IEnumerator EntryTables_MainMenuAndHudReachTheirTargets()
        {
            yield return EnsureLaunchSceneLoaded();
            yield return WaitForRuntimeReady();

            HashSet<int> preexisting = SnapshotSerialIds();
            // 存档服务指向临时目录：测试既不读玩家的真实存档，也不往那里写。
            _saveRoot = Path.Combine(Path.GetTempPath(), "AutoEraUiNavTests", Guid.NewGuid().ToString("N"));
            _context = new AutoEraApplicationContext(
                new SystemUtcTimeProvider(),
                new AutoEraWorldSessionFactory(),
                null,
                new SaveSlotService(_saveRoot));

            try
            {
                yield return MainMenuChain();
                yield return WorldChain();
            }
            finally
            {
                if (GF.UI != null)
                {
                    foreach (int serialId in SnapshotSerialIds())
                    {
                        if (!preexisting.Contains(serialId))
                        {
                            GF.UI.CloseUIForm(serialId);
                        }
                    }
                }

                _region?.Dispose();
                _region = null;
                _context?.Dispose();
                _context = null;

                try
                {
                    if (_saveRoot != null && Directory.Exists(_saveRoot))
                    {
                        Directory.Delete(_saveRoot, true);
                    }
                }
                catch (IOException)
                {
                    // 临时目录清不掉不影响结论。
                }
            }
        }

        /// <summary>主菜单 →「选择进度」→ 存档槽列表；→「新游戏」→ 新建进度页。</summary>
        private IEnumerator MainMenuChain()
        {
            int serialId = AutoEraUiNavigator.Open(UIViews.MainMenuForm, AutoEraUiSession.ForApplication(_context));
            Assert.That(serialId, Is.GreaterThan(0), "主菜单必须能经导航服务打开。");
            yield return WaitForForm(serialId, true);

            var menu = (MainMenuForm)GF.UI.GetUIForm(serialId).Logic;
            Assert.That(menu, Is.Not.Null, "主菜单 Logic 未挂上。");
            Assert.That(menu.SlotsButton, Is.Not.Null, "Btn_MainMenuSlots 必须绑进契约（存档入口）。");
            Assert.That(menu.SettingsButton, Is.Not.Null, "Btn_MainMenuSettings 必须绑进契约。");
            Assert.That(menu.NewButton, Is.Not.Null, "Btn_MainMenuNew 必须绑进契约。");
            Assert.That(menu.ExitButton, Is.Not.Null, "Btn_MainMenuExit 必须绑进契约。");
            yield return WaitForPageChosen(menu, "MainMenuForm");

            // 先放两个真实存档，再打开存档界面：这样验证的是「存档域 → 界面」整条链，
            // 而不只是「界面能打开」。临时目录保证不碰玩家真实存档。
            _context.SaveSlots.Create(0, "端到端存档", 3600_000L, "{}");
            _context.SaveSlots.Create(2, "第二格", 7200_000L, "{}");

            menu.SlotsButton.onClick.Invoke();
            SaveSlotsForm slots = null;
            yield return WaitForLogic<SaveSlotsForm>(found => slots = found, "SaveSlotsForm");
            yield return WaitForPageChosen(slots, "SaveSlotsForm");
            Assert.That(slots.CurrentPage, Is.EqualTo(SaveSlotsForm.PageSlots),
                "从「选择进度」进入应落在存档槽列表页。");
            Assert.That(slots.CanCloseByInputModule, Is.True,
                "SaveSlotsForm 在 UITable 里登记 EscapeClose=true，打开参数必须让表值兜底生效。");
            Assert.That(slots.SaveSlotDataState, Is.EqualTo(UiDataState.Ready),
                "存档目录里有两格存档，界面必须显示为可读而不是空态。");
            Assert.That(slots.SlotListContent, Is.Not.Null, "Content_SaveSlotsSlots 必须绑进契约。");
            Assert.That(slots.SlotListContent.childCount, Is.GreaterThan(1),
                "槽位列表必须真的渲染出行（对象池实例），而不是只留模板。");

            slots.CloseButton.onClick.Invoke();
            yield return WaitForLogicGone<SaveSlotsForm>("SaveSlotsForm");

            menu.NewButton.onClick.Invoke();
            SaveSlotsForm newProgress = null;
            yield return WaitForLogic<SaveSlotsForm>(found => newProgress = found, "SaveSlotsForm（新游戏）");
            yield return WaitForPageChosen(newProgress, "SaveSlotsForm（新游戏）");
            Assert.That(newProgress.CurrentPage, Is.EqualTo(SaveSlotsForm.PageNewProgress),
                "从「新游戏」进入应落在新建进度页。");
        }

        /// <summary>世界链：HUD 顶栏「中枢」→ 中枢，且中枢显示出了世界会话里的机器。</summary>
        private IEnumerator WorldChain()
        {
            Assert.That(_context.TryCreateWorldSession(0L, out AutoEraWorldSession world), Is.True,
                "应能创建测试世界会话。");
            world.Machines.Create(new MachineDefinition(1001, "透传验证机", 1, 2, 1, 2, 30, true, true, 100));

            // 往事件日志写一条机器相关的事实：后面的「记录阅读」入口要断言的是
            // 「界面真的显示出了日志内容」，所以日志必须先有内容。
            world.Events.Journal.Append(new EventJournalRecord(
                EventKind.Fact, EventDomain.Task, new CorrelationId(1), CorrelationId.Invalid,
                PersistentId.Invalid, "端到端记录", 1000L, 1UL, true, EventOutcome.Succeeded));

            // 区域随会话一起交给界面：现场 12 个内容页的数据都来自它。
            _region = new InitialRegion(world, new Rect(-50, -50, 100, 100));
            RegionObject site = _region.Register(PersistentObjectKind.Building, "端到端温室", new Vector2(4, 4), new Vector2(4, 4));

            int hudId = AutoEraUiNavigator.Open(UIViews.FieldHudForm, AutoEraUiSession.ForWorld(_context, world, _region));
            Assert.That(hudId, Is.GreaterThan(0), "现场 HUD 必须能经导航服务打开。");
            yield return WaitForForm(hudId, true);

            var hud = (FieldHudForm)GF.UI.GetUIForm(hudId).Logic;
            Assert.That(hud, Is.Not.Null, "现场 HUD 的 Logic 未挂上。");
            Assert.That(hud.HudHubButton, Is.Not.Null, "Btn_HudNavigationHub 必须绑进契约（世界链入口）。");
            Assert.That(hud.RegionDataState, Is.EqualTo(UiDataState.Ready),
                "区域随会话到达后，现场内容页必须显示为可读，而不是「区域尚未加载」。");
            Assert.That(hud.RegionObjectCount, Is.EqualTo(1));

            // 选中区域对象后，资源观察页的公开状态栏必须真的渲染出详情行。
            Assert.That(_region.Select(site.Id, inputBlocked: false), Is.True);
            yield return WaitFrames(2);
            Assert.That(hud.FarmPublicContent, Is.Not.Null, "Content_FarmPublic 必须绑进契约。");
            Assert.That(hud.FarmPublicContent.childCount, Is.GreaterThan(1),
                "资源观察页必须渲染出选中对象的公开状态，而不是只留模板。");
            // HUD 是「常驻模块 + 互斥侧栏」模型，不走 ShowPage，因此没有页选择可等。
            yield return WaitFrames(5);

            hud.HudHubButton.onClick.Invoke();
            BaseCommandHubForm hub = null;
            yield return WaitForLogic<BaseCommandHubForm>(found => hub = found, "BaseCommandHubForm");
            yield return WaitForPageChosen(hub, "BaseCommandHubForm");

            Assert.That(hub.HubObjectsIndexBody, Is.Not.Null, "Txt_HubObjectsIndexBody 必须绑进契约。");
            Assert.That(hub.MachineDataState, Is.EqualTo(UiDataState.Ready),
                "中枢没有拿到可用的机器域数据——会话没有透传到界面，或 OnAutoEraOpen 中途中断。" +
                $" 数据状态={(hub.MachineDataState.HasValue ? hub.MachineDataState.Value.ToString() : "读模型未创建")}" +
                $" 页={hub.CurrentPage}" +
                $" 世界机器数={world.Machines.Machines.Count()}");

            // 期望文本用产品自己的格式化函数拼：既证明「界面真的写入了数据」，又不会因为
            // 数字格式调整而误报（写死 "机器 1 台" 会随格式化规则变化而失效）。
            Assert.That(hub.HubObjectsIndexTemplate, Is.Not.Null,
                "Item_HubObjectsIndexTemplate 未绑进契约——渲染会在第一行提前返回。");
            Assert.That(hub.HubObjectsIndexContent, Is.Not.Null,
                "Content_HubObjectsIndex 未绑进契约——渲染会在第一行提前返回。");
            Assert.That(hub.HubObjectsIndexBody.text, Is.EqualTo("机器 " + AutoEraUiFormat.Count(1) + " 台"),
                "中枢的索引标题必须由数据写入，而不是保留预制体里的静态占位文案。");

            // 同一条世界链上再走一步：HUD 的「机器」入口 → 机器库，并确认它真的按部署状态过滤。
            Assert.That(hud.HudMachinesButton, Is.Not.Null, "Btn_HudNavigationMachines 必须绑进契约。");
            hud.HudMachinesButton.onClick.Invoke();
            MachineLibraryForm library = null;
            yield return WaitForLogic<MachineLibraryForm>(found => library = found, "MachineLibraryForm");
            yield return WaitForPageChosen(library, "MachineLibraryForm");
            Assert.That(library.MachineDataState, Is.EqualTo(UiDataState.Ready),
                "机器库必须从同一个会话拿到机器域数据。");
            Assert.That(library.UndeployedMachinesCatalogContent, Is.Not.Null);
            Assert.That(library.UndeployedMachinesCatalogContent.childCount, Is.GreaterThan(1),
                "世界里的机器未部署，库中机器页必须渲染出它。");

            // 世界链再走一步：HUD 的「记录」入口 → 记录阅读，且真的渲染出日志行。
            // 事件域是唯一「世界会话一建好就已经活着」的域，所以这个入口必须读出真实记录，
            // 而不是像算法／传感那样只能陈述原因——这是它值得单独断言的理由。
            Assert.That(hud.FarmRecordButton, Is.Not.Null, "Btn_FarmRecord 必须绑进契约（记录入口）。");
            Assert.That(hud.MachineOverviewDiagnosticButton, Is.Not.Null,
                "Btn_MachineOverviewDiagnostic 必须绑进契约（机器诊断的记录入口）。");
            hud.FarmRecordButton.onClick.Invoke();
            RecordReaderForm reader = null;
            yield return WaitForLogic<RecordReaderForm>(found => reader = found, "RecordReaderForm");
            yield return WaitForPageChosen(reader, "RecordReaderForm");
            Assert.That(reader.CurrentPage, Is.EqualTo(RecordReaderForm.PageMachineHistory),
                "从现场记录入口进入应落在机器历史页。");
            Assert.That(reader.EventDataState, Is.EqualTo(UiDataState.Ready),
                $"事件域应可读，实际={reader.EventDataState}，记录数={reader.EventRecordCount}");
            Assert.That(reader.MachineHistoryListTemplate, Is.Not.Null,
                "Item 模板未绑进契约——渲染会在第一行提前返回。");
            Assert.That(reader.MachineHistoryListContent, Is.Not.Null, "Content 未绑进契约。");
            Assert.That(reader.MachineHistoryListContent.childCount, Is.GreaterThan(1),
                "机器历史页必须真的渲染出日志行，而不是只留模板。");
        }

        private static HashSet<int> SnapshotSerialIds()
        {
            var ids = new HashSet<int>();
            if (GF.UI == null)
            {
                return ids;
            }

            foreach (IUIGroup group in GF.UI.GetAllUIGroups())
            {
                foreach (IUIForm form in group.GetAllUIForms())
                {
                    ids.Add(form.SerialId);
                }
            }

            return ids;
        }

        private static IEnumerator EnsureLaunchSceneLoaded()
        {
            const string launchSceneName = "Launch";
            if (SceneManager.GetActiveScene().name == launchSceneName)
            {
                yield break;
            }

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(launchSceneName, LoadSceneMode.Single);
            Assert.IsNotNull(loadOperation, "Launch 必须启用并加入 Build Settings。");
            yield return loadOperation;
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

            Assert.Fail("GF UI 运行时或必需的 UI 数据表在 600 帧内没有就绪。");
        }

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

            Assert.Fail($"UI form serial {serialId} 在 300 帧内没有到达 loaded={expectedLoaded}。");
        }

        private static IEnumerator WaitFrames(int frames)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return null;
            }
        }

        /// <summary>Logic 挂上只说明 Init 跑过；外壳完成首次页选择才算真正打开。</summary>
        private static IEnumerator WaitForPageChosen(AutoEraShellFormBase shell, string what)
        {
            const int maxFrames = 120;
            for (int frame = 0; frame < maxFrames; frame++)
            {
                if (shell != null && shell.CurrentPage >= 0)
                {
                    yield break;
                }

                yield return null;
            }

            Assert.Fail($"{what} 在 120 帧内没有完成首次页选择（OnAutoEraOpen 未执行）。");
        }

        private static IEnumerator WaitForLogic<T>(Action<T> onFound, string what) where T : UIFormLogic
        {
            const int maxFrames = 300;
            for (int frame = 0; frame < maxFrames; frame++)
            {
                T found = FindLogic<T>();
                if (found != null)
                {
                    onFound(found);
                    yield break;
                }

                yield return null;
            }

            Assert.Fail($"{what} 在 {maxFrames} 帧内没有被打开——入口按钮可能没有接上导航服务。");
        }

        private static IEnumerator WaitForLogicGone<T>(string what) where T : UIFormLogic
        {
            const int maxFrames = 300;
            for (int frame = 0; frame < maxFrames; frame++)
            {
                if (FindLogic<T>() == null)
                {
                    yield break;
                }

                yield return null;
            }

            Assert.Fail($"{what} 在 {maxFrames} 帧内没有关闭。当前界面：{DescribeOpenForms()}");
        }

        private static string DescribeOpenForms()
        {
            var parts = new List<string>();
            foreach (IUIGroup group in GF.UI.GetAllUIGroups())
            {
                foreach (IUIForm form in group.GetAllUIForms())
                {
                    string logic = form is UIForm runtimeForm && runtimeForm.Logic != null
                        ? runtimeForm.Logic.GetType().Name
                        : "无Logic";
                    parts.Add($"{group.Name}#{form.SerialId}:{logic}");
                }
            }

            return parts.Count == 0 ? "（无）" : string.Join("、", parts);
        }

        private static T FindLogic<T>() where T : UIFormLogic
        {
            foreach (IUIGroup group in GF.UI.GetAllUIGroups())
            {
                foreach (IUIForm form in group.GetAllUIForms())
                {
                    // GameFramework 的 IUIForm 不暴露 Logic；运行期实现是 UnityGameFramework 的 UIForm。
                    if (form is UIForm runtimeForm && runtimeForm.Logic is T typed)
                    {
                        return typed;
                    }
                }
            }

            return null;
        }
    }
}
