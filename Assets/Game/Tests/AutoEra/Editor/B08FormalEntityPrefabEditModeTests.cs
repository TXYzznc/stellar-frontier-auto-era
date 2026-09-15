using AutoEra.Motion;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class B08FormalEntityPrefabEditModeTests
    {
        private const string MotionGraphFolder = "Assets/Game/MotionGraphs/Entity/";
        [TestCase("Assets/Game/Prefabs/Entity/Machines/WheelModule.prefab", "Assets/Game/Models/Machines/WheelModule.fbx")]
        [TestCase("Assets/Game/Prefabs/Entity/Machines/WheeledCarrier.prefab", "Assets/Game/Models/Machines/WheeledCarrier.fbx")]
        [TestCase("Assets/Game/Prefabs/Entity/Machines/CargoPod.prefab", "Assets/Game/Models/Machines/CargoPod.fbx")]
        [TestCase("Assets/Game/Prefabs/Entity/Machines/FixedRotaryCarrier.prefab", "Assets/Game/Models/Machines/FixedRotaryCarrier.fbx")]
        [TestCase("Assets/Game/Prefabs/Entity/Machines/MultiJointArm.prefab", "Assets/Game/Models/Machines/MultiJointArm.fbx")]
        [TestCase("Assets/Game/Prefabs/Entity/Machines/WaterCannon.prefab", "Assets/Game/Models/Machines/WaterCannon.fbx")]
        [TestCase("Assets/Game/Prefabs/Entity/Machines/RotarySaw.prefab", "Assets/Game/Models/Machines/RotarySaw.fbx")]
        [TestCase("Assets/Game/Prefabs/Entity/Machines/RotaryDrill.prefab", "Assets/Game/Models/Machines/RotaryDrill.fbx")]
        [TestCase("Assets/Game/Prefabs/Entity/Buildings/Conveyor.prefab", "Assets/Game/Models/Buildings/Conveyor.fbx")]
        [TestCase("Assets/Game/Prefabs/Entity/Buildings/SlidingDoor_D24.prefab", "Assets/Game/Models/Buildings/SlidingDoor_D24.fbx")]
        [TestCase("Assets/Game/Prefabs/Entity/Buildings/SlidingDoor_D40.prefab", "Assets/Game/Models/Buildings/SlidingDoor_D40.fbx")]
        public void FormalEntityPrefab_UsesAcceptedVisualModelAndValidMotionBoundary(string prefabPath, string modelPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.That(prefab, Is.Not.Null, prefabPath);

            FunctionalRigPrototypeHierarchy hierarchy = prefab.GetComponent<FunctionalRigPrototypeHierarchy>();
            Assert.That(hierarchy, Is.Not.Null, prefabPath);
            Assert.That(hierarchy.TryValidate(out string hierarchyError), Is.True, hierarchyError);

            MotionRig rig = prefab.GetComponent<MotionRig>();
            Assert.That(rig, Is.Not.Null, prefabPath);
            Assert.That(rig.TryValidate(out string rigError), Is.True, rigError);
            Assert.That(rig.JointBindings.Count, Is.GreaterThan(0), prefabPath);

            Transform visualRoot = hierarchy.VisualRoot;
            Assert.That(visualRoot.GetComponentsInChildren<Renderer>(true).Length, Is.GreaterThan(0), prefabPath);
            Assert.That(AssetDatabase.GetDependencies(prefabPath, true), Does.Contain(modelPath), prefabPath);
            foreach (MotionJointBinding binding in rig.JointBindings)
                Assert.That(binding.JointTransform.IsChildOf(visualRoot), Is.True, binding.StableId);
        }

        [TestCase("wheel_module_steer_roll", "Assets/Game/Prefabs/Entity/Machines/WheelModule.prefab")]
        [TestCase("carrier_drive", "Assets/Game/Prefabs/Entity/Machines/WheeledCarrier.prefab")]
        [TestCase("carrier_crab", "Assets/Game/Prefabs/Entity/Machines/WheeledCarrier.prefab")]
        [TestCase("cargo_transfer_open", "Assets/Game/Prefabs/Entity/Machines/CargoPod.prefab")]
        [TestCase("fixed_rotary_scan", "Assets/Game/Prefabs/Entity/Machines/FixedRotaryCarrier.prefab")]
        [TestCase("arm_work_envelope", "Assets/Game/Prefabs/Entity/Machines/MultiJointArm.prefab")]
        [TestCase("water_aim_spray", "Assets/Game/Prefabs/Entity/Machines/WaterCannon.prefab")]
        [TestCase("saw_cut_cycle", "Assets/Game/Prefabs/Entity/Machines/RotarySaw.prefab")]
        [TestCase("drill_mine_cycle", "Assets/Game/Prefabs/Entity/Machines/RotaryDrill.prefab")]
        [TestCase("conveyor_run", "Assets/Game/Prefabs/Entity/Buildings/Conveyor.prefab")]
        [TestCase("sliding_door_open", "Assets/Game/Prefabs/Entity/Buildings/SlidingDoor_D24.prefab")]
        [TestCase("sliding_door_open", "Assets/Game/Prefabs/Entity/Buildings/SlidingDoor_D40.prefab")]
        public void FormalEntityPrefab_HasAnExecutableMotionGraph(string graphId, string prefabPath)
        {
            MotionGraphAsset graph = AssetDatabase.LoadAssetAtPath<MotionGraphAsset>(MotionGraphFolder + graphId + ".asset");
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.That(graph, Is.Not.Null, graphId);
            Assert.That(prefab, Is.Not.Null, prefabPath);
            Assert.That(graph.IsCompatibleWith(prefab.GetComponent<MotionRig>()), Is.True, graphId + " must play on " + prefabPath);
        }
    }
}
