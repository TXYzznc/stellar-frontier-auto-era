using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AutoEra.Editor
{
    /// <summary>
    /// 启动链接线：场景登记 + 流程注册。
    ///
    /// 界面结构**不在这里构建**。MainMenuForm 等预制体是契约管线的产物
    /// （Docs/Development/UI-PrefabLayouts/&lt;Form&gt;.contract.json →
    /// 菜单「Game Framework/AutoEra/UI/从契约重建所有页面」），任何绕过契约手工
    /// 搭层级的行为都会与管线打架，因此本工具只保留两件与界面结构无关的事：
    /// ① 把启动场景加进 Build Settings（保留既有顺序与既有启用状态）；
    /// ② 把启动链的三个流程注册进 AppConfigs。
    ///
    /// 历史：本类曾有 Fix Minimal Menu Contrast / Build Minimal Startup Assets 两个菜单，
    /// 手工搭建 MenuPanel + EnterButton 的示例层级并回写 MainMenuForm 的序列化字段。
    /// 该层级与旧字段已随界面规格管线一并作废，两个菜单删除；场景与流程注册保留。
    /// </summary>
    public static class AutoEraStartupAssetBuilder
    {
        private const string MenuFormPrefabPath = "Assets/Game/Prefabs/UI/Startup/MainMenuForm.prefab";
        private const string ScenePath = "Assets/Game/Scene/MainMenu.unity";
        private const string WorldScenePath = "Assets/Game/Scene/InitialRegion.unity";
        private const string AppConfigsPath = "Assets/Game/ScriptableAssets/Core/AppConfigs.asset";

        private static readonly string[] StartupProcedures =
        {
            "AutoEra.Procedures.AutoEraStartupProcedure",
            "AutoEra.Procedures.AutoEraMainMenuProcedure",
            "AutoEra.Procedures.AutoEraWorldProcedure",
        };

        [MenuItem("Game Framework/AutoEra/Register Startup Scenes")]
        public static void RegisterScenes()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode.");

            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (string path in new[] { ScenePath, WorldScenePath })
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) throw new InvalidOperationException("Missing scene: " + path);
                int index = scenes.FindIndex(s => s.path == path);
                if (index >= 0)
                {
                    if (!scenes[index].enabled) throw new InvalidOperationException("Existing scene is disabled; preserve existing settings: " + path);
                    continue;
                }

                scenes.Add(new EditorBuildSettingsScene(path, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("[AutoEra][Startup] Registered enabled scenes without reordering existing entries.");
        }

        /// <summary>
        /// 把启动链流程写进 AppConfigs。菜单名的历史含义是「把启动资产准备好」，
        /// 其中唯一的资产构建职责（手工搭 MenuPanel）已删除，这里只留流程注册，
        /// 并在预制体缺失时明确报错，指向契约管线而不是就地补一个。
        /// </summary>
        [MenuItem("Game Framework/AutoEra/Register Startup Procedures")]
        public static void RegisterProcedures()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode.");

            if (AssetDatabase.LoadAssetAtPath<GameObject>(MenuFormPrefabPath) == null)
            {
                throw new InvalidOperationException(
                    "MainMenuForm.prefab missing; run 'Game Framework/AutoEra/UI/从契约重建所有页面' first. " +
                    "界面结构由契约管线生成，不在此处补建。");
            }

            var settings = new SerializedObject(AssetDatabase.LoadMainAssetAtPath(AppConfigsPath));
            SerializedProperty procedures = settings.FindProperty("mProcedures");
            if (procedures == null) throw new InvalidOperationException("AppConfigs.mProcedures not found: " + AppConfigsPath);

            foreach (string name in StartupProcedures)
            {
                bool exists = false;
                for (int i = 0; i < procedures.arraySize; i++) exists |= procedures.GetArrayElementAtIndex(i).stringValue == name;
                if (exists) continue;

                int index = procedures.arraySize;
                procedures.InsertArrayElementAtIndex(index);
                procedures.GetArrayElementAtIndex(index).stringValue = name;
            }

            settings.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            Debug.Log("[AutoEra][Startup] Startup procedures registered.");
        }
    }
}
