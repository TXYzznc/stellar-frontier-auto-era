#if UNITY_EDITOR
using System;
using System.Collections.Generic;

using AutoEra.UI.Testing;

using Cysharp.Threading.Tasks;
using UnityGameFramework.Runtime;

using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor.UiPanelTest
{
    /// <summary>
    /// UI 面板测试管理工具：在 Play Mode 下经 GF.UI 完整链路直接打开项目任意已配置界面，
    /// 其页面切换与子表单机制原样生效。对"正常显示需要流程数据/预加载"的界面，
    /// 通过登记 <see cref="UiPanelTestSetupAttribute"/> 准备钩子在打开前补齐前置。
    /// </summary>
    [ToolHubItem("测试/UI面板测试管理工具", "快速打开/关闭项目任意 UI 界面；准备钩子解决流程依赖界面的数据与预加载前置", 40)]
    public sealed class UiPanelTestPanel : IToolHubPanel
    {
        private const string LogTag = "[AutoEra][UiPanelTest]";

        private enum RuntimeReadiness
        {
            NotPlaying,
            Initializing,
            Ready,
        }

        private readonly List<(int serialId, string label)> _openedForms = new List<(int serialId, string label)>();
        private List<UiPanelTestEntry> _entries = new List<UiPanelTestEntry>();
        private string _search = string.Empty;
        private bool _onlyRegistered;
        private bool _showHelp = true;
        private bool _busy;
        private string _lastResult = string.Empty;
        private Vector2 _scroll;

        public void OnEnable()
        {
            RebuildCatalog();
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        public void OnDisable()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
        }

        public void OnDestroy()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
        }

        public string GetHelpText()
        {
            return "Play Mode 下经 GF.UI 完整链路直接打开任意已配置界面（子页面/子表单原样生效）；"
                + "有流程数据依赖的界面用 [UiPanelTestSetup] 登记准备钩子，打开前自动执行。";
        }

        public void OnGUI()
        {
            DrawStatusHeader();
            DrawToolbar();
            DrawEntryList();
            DrawOpenedForms();
            DrawHelp();
        }

        private void DrawStatusHeader()
        {
            RuntimeReadiness readiness = GetReadiness();
            switch (readiness)
            {
                case RuntimeReadiness.NotPlaying:
                    EditorGUILayout.HelpBox("未进入 Play Mode：请先打开 Launch 场景并按 Play，再用本工具打开界面。", MessageType.Warning);
                    break;
                case RuntimeReadiness.Initializing:
                    EditorGUILayout.HelpBox("框架初始化中：GF UI 或 UI 数据表尚未就绪，请稍候。", MessageType.Info);
                    break;
                default:
                    EditorGUILayout.HelpBox(
                        "运行时就绪：GF UI + UITable/UIGroupTable 已加载。打开走完整 GF 链路，界面内页面切换与子表单机制原样生效。",
                        MessageType.Info);
                    break;
            }

            if (!string.IsNullOrEmpty(_lastResult))
            {
                EditorGUILayout.HelpBox(_lastResult, MessageType.Info);
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                _search = EditorGUILayout.TextField("过滤", _search);
                _onlyRegistered = GUILayout.Toggle(_onlyRegistered, "仅显示已登记准备", GUILayout.Width(130));
                if (GUILayout.Button("刷新清单", GUILayout.Width(80)))
                {
                    RebuildCatalog();
                    _lastResult = "清单已刷新。";
                }
            }
        }

        private void DrawEntryList()
        {
            EditorGUILayout.LabelField($"可测界面（{_entries.Count} 个）", EditorStyles.boldLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (UiPanelTestEntry entry in GetFilteredEntries())
            {
                DrawEntry(entry);
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.EndScrollView();
        }

        private void DrawEntry(UiPanelTestEntry entry)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"{entry.ViewName}  #{entry.ViewId}", EditorStyles.boldLabel);

                    string badge;
                    MessageType badgeType;
                    if (!entry.CanOpenDirectly)
                    {
                        badge = "缺 UITable 配置";
                        badgeType = MessageType.Error;
                    }
                    else if (entry.SetupType != null)
                    {
                        badge = "已登记准备";
                        badgeType = MessageType.Info;
                    }
                    else
                    {
                        badge = "直接可开";
                        badgeType = MessageType.None;
                    }

                    EditorGUILayout.LabelField(badge, EditorStyles.miniLabel, GUILayout.Width(96));
                    GUILayout.FlexibleSpace();

                    bool ready = GetReadiness() == RuntimeReadiness.Ready;
                    using (new EditorGUI.DisabledScope(!ready || !entry.CanOpenDirectly || _busy))
                    {
                        if (GUILayout.Button("打开", GUILayout.Width(48)))
                        {
                            OpenEntryAsync(entry);
                        }
                    }

                    using (new EditorGUI.DisabledScope(!HasOpenRecord(entry)))
                    {
                        if (GUILayout.Button("关闭", GUILayout.Width(48)))
                        {
                            CloseLatestForView(entry);
                        }
                    }

                    if (GUILayout.Button("定位", GUILayout.Width(48)))
                    {
                        PingPrefab(entry);
                    }
                }

                if (!string.IsNullOrEmpty(entry.TableNote))
                {
                    EditorGUILayout.LabelField(entry.TableNote, EditorStyles.miniLabel);
                }

                if (entry.HasUiTableRow)
                {
                    EditorGUILayout.LabelField(
                        $"{entry.PrefabAssetPath}   组:{(string.IsNullOrEmpty(entry.GroupName) ? entry.GroupDepth.ToString() : entry.GroupName)}"
                        + $"   排序:{entry.SortOrder}   遮罩:{entry.PauseCoveredUI}   Esc关闭:{entry.EscapeClose}",
                        EditorStyles.miniLabel);
                }

                if (entry.SetupType != null)
                {
                    EditorGUILayout.LabelField(
                        "准备钩子：" + entry.SetupType.Name + (string.IsNullOrEmpty(entry.SetupNote) ? string.Empty : " — " + entry.SetupNote),
                        EditorStyles.miniLabel);
                }
                else if (!entry.HasUiTableRow)
                {
                    EditorGUILayout.HelpBox("UIViews 枚举缺少该行或 UITable 尚未配置：请补齐 UITable 行后执行 UIViews 生成。", MessageType.Error);
                }
            }
        }

        private void DrawOpenedForms()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("已打开界面（本工具记录）", EditorStyles.boldLabel);
            CleanupStaleRecords();

            if (_openedForms.Count == 0)
            {
                EditorGUILayout.LabelField("暂无。", EditorStyles.miniLabel);
                return;
            }

            for (int i = _openedForms.Count - 1; i >= 0; i--)
            {
                (int serialId, string label) = _openedForms[i];
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"{label}  (serial {serialId})", EditorStyles.miniLabel);
                    GUILayout.FlexibleSpace();
                    using (new EditorGUI.DisabledScope(GetReadiness() != RuntimeReadiness.Ready))
                    {
                        if (GUILayout.Button("关闭", GUILayout.Width(48)))
                        {
                            CloseSerial(serialId);
                        }
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("关闭全部测试界面", GUILayout.Height(24)))
                {
                    CloseAllTracked();
                }

                if (GUILayout.Button("关闭所有界面（含 HUD）", GUILayout.Height(24)))
                {
                    CloseEverything();
                }
            }
        }

        private void DrawHelp()
        {
            EditorGUILayout.Space(8);
            _showHelp = EditorGUILayout.Foldout(_showHelp, "使用说明：如何让任何界面都能直接打开");
            if (!_showHelp)
            {
                return;
            }

            EditorGUILayout.HelpBox(
                "1. 纯静态界面：UITable 配置完成即自动出现在上方清单，进 Play 后点“打开”即可，无需任何登记。\n"
                + "2. 需要数据/预加载的界面：写一个 XxxFormTestSetup : IUiPanelTestSetup 并打 [UiPanelTestSetup(UIViews.Xxx, \"说明\")]，"
                + "工具会在打开前执行其 PrepareAsync（可改 UIParams、订阅 FormOpened、准备伪造或真实数据）。样例参考 BaseCommandHubFormTestSetup。\n"
                + "3. 打开链路 = GF.UI.OpenUIForm 完整生命周期：OnInit/OnOpen/动画/焦点/子表单(OpenSubUIForm) 与正式流程完全一致；"
                + "差异只在数据来源由准备钩子补齐，而不是跳过框架。\n"
                + "4. 深度场景依赖的界面（如 FieldHudForm 依赖世界场景）也可直接打开查看布局与空状态；深度交互仍建议走正式流程验证，"
                + "并在登记说明里注明该限制。",
                MessageType.Info);
        }

        private IEnumerable<UiPanelTestEntry> GetFilteredEntries()
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                UiPanelTestEntry entry = _entries[i];
                if (_onlyRegistered && entry.SetupType == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(_search)
                    && entry.ViewName.IndexOf(_search, StringComparison.OrdinalIgnoreCase) < 0
                    && entry.TableNote.IndexOf(_search, StringComparison.OrdinalIgnoreCase) < 0
                    && entry.ViewId.ToString().IndexOf(_search, StringComparison.Ordinal) < 0)
                {
                    continue;
                }

                yield return entry;
            }
        }

        private void RebuildCatalog()
        {
            _entries = UiPanelTestCatalog.Build();
        }

        private static RuntimeReadiness GetReadiness()
        {
            if (!EditorApplication.isPlaying)
            {
                return RuntimeReadiness.NotPlaying;
            }

            if (GF.UI == null || GF.DataTable == null
                || !GF.DataTable.HasDataTable<UITable>()
                || !GF.DataTable.HasDataTable<UIGroupTable>()
                || !GF.UI.HasUIGroup("Default"))
            {
                return RuntimeReadiness.Initializing;
            }

            return RuntimeReadiness.Ready;
        }

        /// <summary>Editor 用户事件适配层：异步打开单个界面；全程捕获异常，Play 边界做了防穿越检查。</summary>
        private async void OpenEntryAsync(UiPanelTestEntry entry)
        {
            if (_busy)
            {
                return;
            }

            _busy = true;
            try
            {
                if (GetReadiness() != RuntimeReadiness.Ready)
                {
                    _lastResult = "运行时未就绪，无法打开。";
                    return;
                }

                var parameters = UIParams.Create();
                var context = new UiPanelTestSetupContext(entry.View, parameters);
                parameters.OpenCallback = logic => context.RaiseFormOpened(logic);

                if (entry.SetupType != null)
                {
                    var setup = (IUiPanelTestSetup)Activator.CreateInstance(entry.SetupType);
                    _lastResult = "执行准备钩子：" + entry.SetupType.Name + " …";
                    await setup.PrepareAsync(context);
                    if (!EditorApplication.isPlaying)
                    {
                        _lastResult = "准备期间退出了 Play Mode，已取消打开。";
                        return;
                    }
                }

                int serialId = GF.UI.OpenUIForm(entry.View, context.Parameters);
                if (serialId < 0)
                {
                    _lastResult = "打开失败：GF.UI 拒绝请求（检查 UITable 行与 Prefab 是否存在）。";
                    Debug.LogWarning($"{LogTag} OpenUIForm returned failure for {entry.ViewName}.");
                }
                else
                {
                    _openedForms.Add((serialId, entry.ViewName));
                    _lastResult = $"已请求打开：{entry.ViewName} (serial {serialId})";
                }
            }
            catch (Exception exception)
            {
                _lastResult = "打开异常：" + exception.Message;
                Debug.LogException(exception);
            }
            finally
            {
                _busy = false;
            }
        }

        private bool HasOpenRecord(UiPanelTestEntry entry)
        {
            for (int i = 0; i < _openedForms.Count; i++)
            {
                if (_openedForms[i].label == entry.ViewName)
                {
                    return true;
                }
            }

            return false;
        }

        private void CloseLatestForView(UiPanelTestEntry entry)
        {
            for (int i = _openedForms.Count - 1; i >= 0; i--)
            {
                if (_openedForms[i].label == entry.ViewName)
                {
                    CloseSerial(_openedForms[i].serialId);
                    return;
                }
            }
        }

        private void CloseSerial(int serialId)
        {
            if (GF.UI == null)
            {
                return;
            }

            if (GF.UI.HasUIForm(serialId) && !GF.UI.IsLoadingUIForm(serialId))
            {
                GF.UI.Close(serialId);
            }
            else if (GF.UI.IsLoadingUIForm(serialId))
            {
                GF.UI.CloseUIForm(serialId);
            }

            RemoveRecord(serialId);
            _lastResult = $"已请求关闭 serial {serialId}。";
        }

        private void CloseAllTracked()
        {
            for (int i = _openedForms.Count - 1; i >= 0; i--)
            {
                CloseSerial(_openedForms[i].serialId);
            }

            _lastResult = "已请求关闭全部测试界面。";
        }

        private void CloseEverything()
        {
            if (GF.UI == null)
            {
                return;
            }

            UIForm[] loadedForms = GF.UI.GetAllLoadedUIForms();
            for (int i = 0; i < loadedForms.Length; i++)
            {
                GF.UI.CloseUIForm(loadedForms[i].SerialId);
            }

            _openedForms.Clear();
            _lastResult = "已关闭所有已加载界面。";
        }

        private void CleanupStaleRecords()
        {
            for (int i = _openedForms.Count - 1; i >= 0; i--)
            {
                int serialId = _openedForms[i].serialId;
                if (GF.UI == null || (!GF.UI.HasUIForm(serialId) && !GF.UI.IsLoadingUIForm(serialId)))
                {
                    _openedForms.RemoveAt(i);
                }
            }
        }

        private void RemoveRecord(int serialId)
        {
            for (int i = _openedForms.Count - 1; i >= 0; i--)
            {
                if (_openedForms[i].serialId == serialId)
                {
                    _openedForms.RemoveAt(i);
                }
            }
        }

        private static void PingPrefab(UiPanelTestEntry entry)
        {
            if (string.IsNullOrEmpty(entry.PrefabAssetPath))
            {
                return;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(entry.PrefabAssetPath);
            if (prefab == null)
            {
                Debug.LogWarning($"{LogTag} Prefab not found: {entry.PrefabAssetPath}");
                return;
            }

            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
        }

        private void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                _openedForms.Clear();
                _busy = false;
                _lastResult = "已退出 Play Mode，打开记录已清空。";
            }
        }
    }
}
#endif
