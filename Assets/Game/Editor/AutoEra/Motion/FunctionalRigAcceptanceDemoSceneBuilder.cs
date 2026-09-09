using System.Collections.Generic;
using AutoEra.Motion;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AutoEra.Editor.Motion
{
    internal static class FunctionalRigAcceptanceDemoSceneBuilder
    {
        private const string PrefabFolder = "Assets/Game/Prefabs/FunctionalPrototypes/Catalog/";

        [MenuItem("AutoEra/Functional Prototypes/Update Acceptance Demo With Current Catalog")]
        private static void UpdateCurrentDemo()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != "Assets/Game/Scenes/AutoEra/FunctionalRigAcceptanceDemo.unity")
            {
                Debug.LogError("[AutoEra.Motion] Open FunctionalRigAcceptanceDemo before updating it.");
                return;
            }

            Transform carrier = Ensure("wheeled_carrier", "演示_轮式载体", new Vector3(-7f, 0f, 0f));
            Transform wheel = Ensure("four_wheel_module", "演示_四轮机构", new Vector3(-3f, 0f, -3f));
            Transform arm = Ensure("multi_joint_arm", "演示_多关节机械臂", new Vector3(0f, 0f, 0f));
            Remove("演示_可替换效应器");
            Transform door = Ensure("sliding_door", "演示_滑动门", new Vector3(7f, 0f, 1f));
            Transform conveyor = Ensure("conveyor", "演示_传送带", new Vector3(10f, 0f, -3f));
            Transform water = Ensure("water_sprayer", "演示_水枪效应器", new Vector3(-7f, 0f, 6f));
            Transform saw = Ensure("rotary_saw", "演示_旋转锯盘效应器", new Vector3(-2f, 0f, 6f));
            Transform drill = Ensure("rotary_drill", "演示_旋转钻头效应器", new Vector3(3f, 0f, 6f));
            Transform cargo = Ensure("cargo_bay", "演示_货舱", new Vector3(8f, 0f, 6f));
            Transform fixedRotary = Ensure("fixed_rotary_carrier", "演示_固定旋转载体", new Vector3(-11f, 0f, 6f));

            FunctionalRigAcceptanceDemoDirector director = Object.FindObjectOfType<FunctionalRigAcceptanceDemoDirector>();
            if (director == null)
            {
                director = new GameObject("AcceptanceDemoDirector").AddComponent<FunctionalRigAcceptanceDemoDirector>();
            }

            Undo.RecordObject(director, "更新动作验收场");
            director.Configure(wheel, carrier, arm, door, conveyor, water, saw, drill, cargo, fixedRotary);
            EditorUtility.SetDirty(director);
            EditorSceneManager.MarkSceneDirty(scene);
        }

        private static Transform Ensure(string familyId, string objectName, Vector3 position)
        {
            GameObject existing = GameObject.Find(objectName);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + familyId + ".prefab");
            if (existing != null)
            {
                if (familyId == "conveyor")
                {
                    PrefabUtility.RevertPrefabInstance(existing, InteractionMode.AutomatedAction);
                    existing.transform.position = position;
                }

                return existing.transform;
            }
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            instance.name = objectName;
            instance.transform.position = position;
            return instance.transform;
        }

        private static void Remove(string objectName)
        {
            GameObject existing = GameObject.Find(objectName);
            if (existing != null) Object.DestroyImmediate(existing);
        }
    }
}
