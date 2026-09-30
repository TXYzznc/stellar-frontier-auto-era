using System;
using AutoEra.Algorithms;
using AutoEra.Alerts;
using AutoEra.Machines;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor.Gm
{
    /// <summary>
    /// GM 管理面板：Play Mode 下经**正式领域 API**（花名册／区域部署／警报账本）注入与修改数据，
    /// 用于验收（尤其是 G3-001 算法配置闭环）与调试。不做任何旁路——所有操作都走与游戏相同的
    /// <c>MachineRoster.Create / region.DeployMachine / AutoEraAlertService.Raise</c> 路径。
    ///
    /// 每一步都同时写面板提示与 <c>Debug.Log</c>（前缀 [GM]）：面板提示给「点一下就走的」即时反馈，
    /// 控制台日志给「为什么失败」的完整链路，失败时能直接定位到具体枚举值。
    /// </summary>
    [ToolHubItem("调试工具/GM管理面板", "Play Mode 下注入机器/组件、部署、修改机器状态与警报、时间倍速，用于验收与调试", 50)]
    public sealed class GmPanel : IToolHubPanel
    {
        private const string LogTag = "[GM]";
        private const int WheeledMachineRowId = 10011; // 轮式载体（可移动）
        private const int CoreComponentRowId = 20011;  // 计算核心（提供逻辑算力）

        private string _lastResult = string.Empty;
        private Vector2 _scroll;
        private float _teleportX = 12f;
        private float _teleportY = -12f;
        private double _integrity = 100d;
        private bool _powerSwitch = true;
        private MachineRunState _runState = MachineRunState.Running;
        private double _timeMultiplier = 1d;

        public void OnEnable() { }
        public void OnDisable() { }
        public void OnDestroy() { }

        public string GetHelpText() =>
            "Play Mode 下经正式领域 API 注入与修改数据，用于 G3-001 验收与调试。\n"
            + "· 一键部署带核心的机器：创建轮式载体 10011 + 核心 20011、装入、部署、生成实体与运行时、激活、选中。\n"
            + "· 机器状态：选中机器后改完整度／供电开关／运行状态、传送到坐标。\n"
            + "· 警报：按 4 类警报制造／消除。\n"
            + "· 时间倍速：只对开发构建生效（Development Time Multiplier）。\n"
            + "所有操作同时写 Console（前缀 [GM]），失败时看 Console 定位具体原因。";

        public void OnGUI()
        {
            EditorGUILayout.Space(4);
            InitialRegionScene scene = FindScene();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("未进入 Play Mode：请打开 Launch 场景按 Play，再进入区域后使用本面板。", MessageType.Warning);
                return;
            }

            if (scene == null)
            {
                EditorGUILayout.HelpBox("未找到 InitialRegionScene：请先进入游戏并加载区域（Launch → 进入世界 → InitialRegion）。", MessageType.Warning);
                return;
            }

            if (scene.Region == null || scene.Session == null)
            {
                EditorGUILayout.HelpBox("区域尚未就绪（Region/Session 为空），请等区域初始化完成。", MessageType.Warning);
                return;
            }

            if (!string.IsNullOrEmpty(_lastResult))
            {
                EditorGUILayout.HelpBox(_lastResult, MessageType.Info);
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawDeploy(scene);
            DrawMachines(scene);
            DrawAlerts(scene);
            DrawTime(scene);
            EditorGUILayout.EndScrollView();
        }

        private static InitialRegionScene FindScene() => UnityEngine.Object.FindObjectOfType<InitialRegionScene>();

        // ---------------------------------------------------------------- 日志

        private static void Log(string message) => Debug.Log(LogTag + " " + message);
        private static void Warn(string message) => Debug.LogWarning(LogTag + " " + message);
        private static void Error(string message) => Debug.LogError(LogTag + " " + message);

        private void Result(string message)
        {
            _lastResult = message;
            Log(message);
        }

        private void Fail(string message)
        {
            _lastResult = message;
            Error(message);
        }

        // ---------------------------------------------------------------- 一键部署

        private void DrawDeploy(InitialRegionScene scene)
        {
            EditorGUILayout.LabelField("验收准备", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                _teleportX = EditorGUILayout.FloatField("落位 X", _teleportX);
                _teleportY = EditorGUILayout.FloatField("落位 Z", _teleportY);
            }

            if (GUILayout.Button("一键部署带核心的机器（10011 + 20011）", GUILayout.Height(28)))
            {
                DeployMachineWithCore(scene, new Vector2(_teleportX, _teleportY));
            }

            EditorGUILayout.Space(6);
        }

        private void DeployMachineWithCore(InitialRegionScene scene, Vector2 position)
        {
            Log($"开始一键部署：机器行 {WheeledMachineRowId}、核心行 {CoreComponentRowId}、目标落位 ({position.x:F1}, {position.y:F1})。");

            try
            {
                if (!MachineCatalog.IsGameDataLoaded)
                {
                    Fail("机器数据表未加载（MachineCatalog.IsGameDataLoaded == false）。请确认已进入游戏且数据表预加载完成。");
                    return;
                }

                MachineCatalog catalog = MachineCatalog.FromLoadedGameData();
                Log("机器数据表已加载。");

                if (!catalog.TryGetMachine(WheeledMachineRowId, out MachineDefinition wheeled))
                {
                    Fail($"机器数据表缺少行 {WheeledMachineRowId}，无法创建机器。");
                    return;
                }

                Log($"机器定义：{wheeled.Name}（模型 Id {wheeled.Id}，核心槽 {wheeled.CoreSlots}，占地 {wheeled.FootprintX}×{wheeled.FootprintZ}）。");

                if (!catalog.TryGetComponent(CoreComponentRowId, out ComponentDefinition coreDef))
                {
                    Fail($"组件数据表缺少行 {CoreComponentRowId}，无法创建核心。");
                    return;
                }

                Log($"组件定义：{coreDef.Kind}（模型 Id {coreDef.Id}，算力 {coreDef.ComputeCapacity}，逻辑 {coreDef.LogicCapacity}）。");

                if (!wheeled.HasFootprint)
                {
                    Fail($"机器 {wheeled.Name} 未配置占地（Footprint 为 0），无法部署。");
                    return;
                }

                MachineRoster roster = scene.Session.Machines;
                Log($"花名册就绪（当前 {CountRoster(roster)} 台机器）。");

                MachineInstance machine = roster.Create(wheeled);
                Log($"已创建机器「{machine.Name}」#{machine.Id.Value}。");

                ComponentInstance core = roster.CreateComponent(coreDef);
                Log($"已创建核心组件 #{core.Id.Value}。");

                MachineManagementResult install = roster.Install(machine.Id, ManagementOrigin.Library, core.Id, 0);
                if (install != MachineManagementResult.Completed)
                {
                    Fail($"核心装入失败：{install}（机器核心槽 {wheeled.CoreSlots}，装入位置 0）。");
                    return;
                }

                Log($"核心已装入「{machine.Name}」的核心槽 0，逻辑算力 {machine.LogicCapacity}。");

                // 先在目标位置部署；撞上场景种子对象时，在附近找一块空地回退。
                Vector2 placed = position;
                RegionMachineDeploymentResult deploy = scene.Region.DeployMachine(
                    machine.Id, placed, wheeled.Footprint, out RegionObject placedModel, 0f, true);
                if (deploy != RegionMachineDeploymentResult.Bound && deploy == RegionMachineDeploymentResult.InvalidPlacement)
                {
                    Warn($"目标落位 ({position.x:F1}, {position.y:F1}) 部署失败：{deploy}。开始在附近寻找空地。");
                    if (TryFindFreePosition(scene, wheeled, position, out Vector2 fallback))
                    {
                        placed = fallback;
                        deploy = scene.Region.DeployMachine(machine.Id, placed, wheeled.Footprint, out placedModel, 0f, true);
                        Log($"已回退到空地 ({placed.x:F1}, {placed.y:F1})，部署结果：{deploy}。");
                    }
                }

                if (deploy != RegionMachineDeploymentResult.Bound)
                {
                    Fail($"部署失败：{deploy}（位置 ({placed.x:F1}, {placed.y:F1})、占地 {wheeled.Footprint}）。可能是区域未就绪或目标与既有对象重叠。");
                    return;
                }

                Log($"机器已部署：落位 ({placed.x:F1}, {placed.y:F1})、占地 {wheeled.Footprint}。");

                MachineManagementResult activate = machine.Activate(ManagementOrigin.Field);
                Log($"激活结果：{activate}。");

                MachineManagementResult run = machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);
                Log($"设运行状态结果：{run}。");

                machine.UpdateEnvironment(true, true);
                Log("已设置供电与信号可用（UpdateEnvironment(true, true)）。");

                scene.TrySpawnMachine(machine.Id, out string spawnReason);
                if (string.IsNullOrEmpty(spawnReason))
                {
                    Log("已请求生成实体与运行时（异步建立）。");
                }
                else
                {
                    Warn($"实体生成结果：{spawnReason}（部署不受影响，但可能没有运行时/视图）。");
                }

                scene.Region.Select(machine.Id, false);
                ScheduleSeedAlgorithmDraft(scene, machine.Id);
                Log($"已选中「{machine.Name}」，算法工作台可读取该机器的运行时。");

                Result($"部署完成：机器「{machine.Name}」落位 ({placed.x:F1}, {placed.y:F1})，核心已装入，逻辑算力 {machine.LogicCapacity}，已激活运行。"
                    + (string.IsNullOrEmpty(spawnReason) ? " 实体与运行时异步建立，稍候片刻即可在算法工作台看到 Ready。" : ""));
            }
            catch (Exception exception)
            {
                Fail("一键部署异常：" + exception.Message);
                Debug.LogException(exception);
            }
        }

        private static void ScheduleSeedAlgorithmDraft(InitialRegionScene scene, PersistentId machineId)
        {
            int attempts = 0;
            void TrySeed()
            {
                attempts++;
                if (scene == null || scene.Session == null || scene.MachineRuntimes == null)
                {
                    return;
                }

                if (scene.MachineRuntimes.TryGet(machineId, out RegionMachineRuntime runtime) && runtime.Instances != null)
                {
                    AlgorithmTemplateInfo[] templates = scene.Session.AlgorithmTemplates.List();
                    if (templates.Length > 0)
                    {
                        AlgorithmDocument draft = scene.Session.AlgorithmTemplates.Instantiate(templates[0].Id);
                        if (draft != null && runtime.Instances.AddDraft(draft))
                        {
                            Log($"已为机器 #{machineId.Value} 注入算法草稿「{templates[0].Name}」，算法编辑器现在可编辑。");
                        }
                    }
                    return;
                }

                if (attempts < 30)
                {
                    UnityEditor.EditorApplication.delayCall += TrySeed;
                }
                else
                {
                    Warn($"机器 #{machineId.Value} 的运行时在 30 次编辑器回调后仍未就绪，未注入算法草稿。");
                }
            }

            UnityEditor.EditorApplication.delayCall += TrySeed;
        }

        private static int CountRoster(MachineRoster roster)
        {
            int count = 0;
            foreach (MachineInstance _ in roster.Machines) count++;
            return count;
        }

        /// <summary>在目标点附近按环形扫描找一块不冲突的空地（保持 yaw=0、blocksNavigation=true）。</summary>
        private static bool TryFindFreePosition(InitialRegionScene scene, MachineDefinition definition, Vector2 origin, out Vector2 found)
        {
            found = origin;
            for (int ring = 1; ring <= 6; ring++)
            {
                for (int angle = 0; angle < 8; angle++)
                {
                    float radians = angle * Mathf.PI / 4f;
                    Vector2 candidate = origin + new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * (ring * 4f);
                    candidate.x = Mathf.Round(candidate.x);
                    candidate.y = Mathf.Round(candidate.y);
                    if (scene.Region.CanPlace(candidate, definition.Footprint, 0f, PersistentId.Invalid, out _))
                    {
                        found = candidate;
                        return true;
                    }
                }
            }

            return false;
        }

        // ---------------------------------------------------------------- 机器状态

        private void DrawMachines(InitialRegionScene scene)
        {
            EditorGUILayout.LabelField("机器状态", EditorStyles.boldLabel);

            MachineRoster roster = scene.Session.Machines;
            int listed = 0;
            foreach (MachineInstance machine in roster.Machines)
            {
                if (!machine.Deployed)
                {
                    continue;
                }

                listed++;
                using (new EditorGUILayout.HorizontalScope())
                {
                    bool selected = scene.Region.SelectedId == machine.Id;
                    EditorGUILayout.LabelField(
                        $"{(selected ? "▶" : "  ")} {machine.Name}  #{machine.Id.Value}",
                        EditorStyles.miniLabel, GUILayout.Width(200));
                    EditorGUILayout.LabelField(
                        $"完整度 {machine.Integrity:F0} · 供电 {(machine.Powered ? "正常" : "无")} · {machine.RequestedRunState}",
                        EditorStyles.miniLabel);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(selected ? "已选中" : "选中", GUILayout.Width(52)))
                    {
                        scene.Region.Select(machine.Id, false);
                        Result($"已选中 {machine.Name}。");
                    }
                }
            }

            if (listed == 0)
            {
                EditorGUILayout.HelpBox("还没有已部署的机器：用上方「一键部署带核心的机器」先部署一台。", MessageType.None);
                EditorGUILayout.Space(4);
                return;
            }

            EditorGUILayout.Space(4);
            MachineInstance target = FindSelectedMachine(scene);
            if (target == null)
            {
                EditorGUILayout.HelpBox("未选中机器：点列表里的「选中」后即可修改这台机器的状态。", MessageType.None);
                EditorGUILayout.Space(6);
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField($"修改「{target.Name}」", EditorStyles.miniBoldLabel);

                _integrity = EditorGUILayout.DoubleField("完整度", Math.Clamp(_integrity, 0d, target.Definition.MaximumIntegrity));
                if (GUILayout.Button("写入完整度"))
                {
                    double value = Math.Clamp(_integrity, 0d, target.Definition.MaximumIntegrity);
                    target.UpdateIntegrity(value);
                    Log($"已把 {target.Name} 完整度设为 {value:F0}（上限 {target.Definition.MaximumIntegrity:F0}）。");
                    Result($"已把 {target.Name} 完整度设为 {value:F0}。");
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    _teleportX = EditorGUILayout.FloatField("传送 X", _teleportX);
                    _teleportY = EditorGUILayout.FloatField("传送 Z", _teleportY);
                }

                using (new EditorGUI.DisabledScope(!target.Definition.CanMove))
                {
                    if (GUILayout.Button("传送（仅可移动机型）"))
                    {
                        float yaw = scene.Region.TryGet(target.Id, out RegionObject body) ? body.Yaw : 0f;
                        if (scene.Region.TryUpdateMachinePose(target.Id, new Vector2(_teleportX, _teleportY), yaw))
                        {
                            Log($"已把 {target.Name} 传送到 ({_teleportX:F1}, {_teleportY:F1})，yaw {yaw:F1}。");
                            Result($"已把 {target.Name} 传送到 ({_teleportX}, {_teleportY})。");
                        }
                        else
                        {
                            Fail($"传送失败：({_teleportX:F1}, {_teleportY:F1}) 不可导航（超出边界、机型不可移动、或与障碍重叠）。");
                        }
                    }
                }

                _powerSwitch = EditorGUILayout.Toggle("供电开关", _powerSwitch);
                _runState = (MachineRunState)EditorGUILayout.EnumPopup("运行状态", _runState);

                if (GUILayout.Button("写入供电与运行状态"))
                {
                    MachineManagementResult power = target.SetPowerSwitch(ManagementOrigin.Field, _powerSwitch);
                    MachineManagementResult run = target.SetRunState(ManagementOrigin.Field, _runState);
                    Log($"{target.Name} 写入供电开关={_powerSwitch} 结果 {power}；写入运行状态={_runState} 结果 {run}。");
                    Result($"已写入：供电={(_powerSwitch ? "开" : "关")}（{power}）、状态={_runState}（{run}）。");
                }
            }

            EditorGUILayout.Space(6);
        }

        private static MachineInstance FindSelectedMachine(InitialRegionScene scene)
        {
            PersistentId selected = scene.Region.SelectedId;
            if (!selected.IsValid)
            {
                return null;
            }

            return scene.Session.Machines.TryGet(selected, out MachineInstance machine) ? machine : null;
        }

        // ---------------------------------------------------------------- 警报

        private void DrawAlerts(InitialRegionScene scene)
        {
            EditorGUILayout.LabelField("警报", EditorStyles.boldLabel);

            AutoEraAlertService alerts = scene.Alerts;
            if (alerts == null)
            {
                EditorGUILayout.HelpBox("区域警报账本未建立。", MessageType.None);
                EditorGUILayout.Space(6);
                return;
            }

            MachineInstance selected = FindSelectedMachine(scene);
            PersistentId source = selected != null ? selected.Id : PersistentId.Invalid;
            long now = scene.WorldMilliseconds;

            EditorGUILayout.LabelField(
                $"来源：{(selected != null ? selected.Name : "区域口径（Invalid）")} · 当前 {alerts.ActiveCount} 活跃 / {alerts.UnreadCount} 未读",
                EditorStyles.miniLabel);

            foreach (AlertKind kind in (AlertKind[])Enum.GetValues(typeof(AlertKind)))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(AlertCatalog.Label(kind), GUILayout.Width(120));
                    if (GUILayout.Button("制造"))
                    {
                        bool raised = alerts.Raise(kind, source, now);
                        Log($"制造警报 {AlertCatalog.Label(kind)}（来源 {SourceLabel(selected)}，t={now}ms）：{(raised ? "新开/重新激活" : "已是活跃，无变化")}。");
                        Result($"已制造警报：{AlertCatalog.Label(kind)}（{(raised ? "新开" : "已是活跃")}）。");
                    }

                    if (GUILayout.Button("消除"))
                    {
                        bool resolved = alerts.Resolve(kind, source, now);
                        Log($"消除警报 {AlertCatalog.Label(kind)}（来源 {SourceLabel(selected)}，t={now}ms）：{(resolved ? "已转历史" : "本就不活跃或不存在")}。");
                        Result($"已消除警报：{AlertCatalog.Label(kind)}（{(resolved ? "已恢复" : "本就不活跃")}）。");
                    }
                }
            }

            if (GUILayout.Button("清空全部警报", GUILayout.Width(160)))
            {
                alerts.Clear();
                Log("已清空全部警报。");
                Result("已清空全部警报。");
            }

            EditorGUILayout.Space(6);
        }

        private static string SourceLabel(MachineInstance machine) =>
            machine != null ? $"{machine.Name}#{machine.Id.Value}" : "区域口径(Invalid)";

        // ---------------------------------------------------------------- 时间

        private void DrawTime(InitialRegionScene scene)
        {
            EditorGUILayout.LabelField("时间", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"世界时间：{scene.WorldMilliseconds / 1000L} s", EditorStyles.miniLabel);

            _timeMultiplier = EditorGUILayout.DoubleField("时间倍速（0=暂停，仅开发构建）", _timeMultiplier);
            if (GUILayout.Button("应用时间倍速", GUILayout.Width(160)))
            {
                bool ok = scene.TrySetDevelopmentTimeMultiplier(_timeMultiplier);
                Log($"设置时间倍速 {_timeMultiplier}：{(ok ? "成功" : "失败（须是 ≥0 的有限数值）")}。");
                Result(ok ? $"已设置时间倍速 {_timeMultiplier}。" : "设置失败：倍速必须是 ≥0 的有限数值。");
            }
        }
    }
}
