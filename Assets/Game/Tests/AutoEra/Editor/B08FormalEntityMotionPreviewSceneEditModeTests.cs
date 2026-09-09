using System.Collections.Generic;
using AutoEra.Motion;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AutoEra.Tests.Editor
{
    public sealed class B08FormalEntityMotionPreviewSceneEditModeTests
    {
        private const string ScenePath = "Assets/Game/Scene/B08FormalEntityMotionPreview.unity";

        private static readonly KeyValuePair<string, string>[] ExpectedInstances =
        {
            new KeyValuePair<string, string>("演示_轮式载体", "Assets/Game/Prefabs/Entity/Machines/Carriers/WheeledCarrier.prefab"),
            new KeyValuePair<string, string>("演示_四轮机构", "Assets/Game/Prefabs/Entity/Machines/Modules/WheelModule.prefab"),
            new KeyValuePair<string, string>("演示_多关节机械臂", "Assets/Game/Prefabs/Entity/Machines/Effectors/MultiJointArm.prefab"),
            new KeyValuePair<string, string>("演示_滑动门", "Assets/Game/Prefabs/Entity/Buildings/Mechanisms/SlidingDoors/SlidingDoor_D24.prefab"),
            new KeyValuePair<string, string>("演示_传送带", "Assets/Game/Prefabs/Entity/Buildings/Logistics/Conveyor.prefab"),
            new KeyValuePair<string, string>("演示_水枪效应器", "Assets/Game/Prefabs/Entity/Machines/Effectors/WaterCannon.prefab"),
            new KeyValuePair<string, string>("演示_旋转锯盘效应器", "Assets/Game/Prefabs/Entity/Machines/Effectors/RotarySaw.prefab"),
            new KeyValuePair<string, string>("演示_旋转钻头效应器", "Assets/Game/Prefabs/Entity/Machines/Effectors/RotaryDrill.prefab"),
            new KeyValuePair<string, string>("演示_货舱", "Assets/Game/Prefabs/Entity/Machines/Modules/CargoPod.prefab"),
            new KeyValuePair<string, string>("演示_固定旋转载体", "Assets/Game/Prefabs/Entity/Machines/Carriers/FixedRotaryCarrier.prefab")
        };

        [Test]
        public void FormalPreviewScene_UsesOnlyTheExpectedFormalEntityInstances()
        {
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath), Is.Not.Null, ScenePath);

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                foreach (KeyValuePair<string, string> expected in ExpectedInstances)
                {
                    GameObject instance = FindRoot(scene, expected.Key);
                    Assert.That(instance, Is.Not.Null, expected.Key);
                    GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(instance);
                    Assert.That(source, Is.Not.Null, expected.Key + " must remain a Prefab instance.");
                    Assert.That(AssetDatabase.GetAssetPath(source), Is.EqualTo(expected.Value), expected.Key);
                }

                Assert.That(FindRoot(scene, "演示_可替换效应器"), Is.Null,
                    "The retired replaceable-effector demo must not return to the formal preview scene.");
                Assert.That(FindRoot(scene, "演示_传送带").GetComponentInChildren<ConveyorLoopRigPreview>(true), Is.Not.Null);
                Assert.That(FindRoot(scene, "演示_水枪效应器").GetComponent<EffectorWorkRigPreview>(), Is.Not.Null);
                Assert.That(FindRoot(scene, "演示_旋转锯盘效应器").GetComponent<EffectorWorkRigPreview>(), Is.Not.Null);
                Assert.That(FindRoot(scene, "演示_旋转钻头效应器").GetComponent<EffectorWorkRigPreview>(), Is.Not.Null);
                Assert.That(FindRoot(scene, "演示_货舱").GetComponent<CargoBayPreview>(), Is.Not.Null);
                Assert.That(Object.FindObjectsOfType<FunctionalRigAcceptanceDemoDirector>(true), Has.Length.GreaterThanOrEqualTo(1));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void FormalPreviewScene_HasNoLegacyPrototypeDependencies()
        {
            foreach (string dependency in AssetDatabase.GetDependencies(ScenePath, true))
            {
                Assert.That(dependency.StartsWith("Assets/Game/Prefabs/FunctionalPrototypes"), Is.False, dependency);
                Assert.That(dependency.StartsWith("Assets/Game/Materials/FunctionalPrototypes"), Is.False, dependency);
                Assert.That(dependency.StartsWith("Assets/Game/MotionGraphs/FunctionalPrototypes"), Is.False, dependency);
                Assert.That(dependency.StartsWith("Assets/Game/Scenes/AutoEra"), Is.False, dependency);
            }
        }

        private static GameObject FindRoot(Scene scene, string objectName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == objectName)
                {
                    return root;
                }
            }

            return null;
        }
    }
}
