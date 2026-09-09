using AutoEra.Motion;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class B08FormalEntityPrefabEditModeTests
    {
        private const string MotionGraphFolder = "Assets/Game/MotionGraphs/Entity/";
        [TestCase("Assets/Game/Prefabs/Entity/Machines/Modules/WheelModule.prefab", "Assets/Game/Models/Machines/Modules/WheelModule.fbx")]
        [TestCase("Assets/Game/Prefabs/Entity/Machines/Carriers/WheeledCarrier.prefab", "Assets/Game/Models/Machines/Carriers/WheeledCarrier.fbx")]
        [TestCase("Assets/Game/Prefabs/Entity/Machines/Modules/CargoPod.prefab", "Assets/Game/Models/Machines/Modules/CargoPod.fbx")]
        [TestCase("Assets/Game/Prefabs/Entity/Machines/Carriers/FixedRotaryCarrier.prefab", "Assets/Game/Models/Machines/Carriers/FixedRotaryCarrier.fbx")]
        [TestCase("Assets/Game/Prefabs/Entity/Machines/Effectors/MultiJointArm.prefab", "Assets/Game/Models/Machines/Effectors/MultiJointArm.fbx")]
        [TestCase("Assets/Game/Prefabs/Entity/Machines/Effectors/WaterCannon.prefab", "Assets/Game/Models/Machines/Effectors/WaterCannon.fbx")]
        [TestCase("Assets/Game/Prefabs/Entity/Machines/Effectors/RotarySaw.prefab", "Assets/Game/Models/Machines/Effectors/RotarySaw.fbx")]
        [TestCase("Assets/Game/Prefabs/Entity/Machines/Effectors/RotaryDrill.prefab", "Assets/Game/Models/Machines/Effectors/RotaryDrill.fbx")]
        [TestCase("Assets/Game/Prefabs/Entity/Buildings/Logistics/Conveyor.prefab", "Assets/Game/Models/Buildings/Logistics/Conveyor/Conveyor.fbx")]
        [TestCase("Assets/Game/Prefabs/Entity/Buildings/Mechanisms/SlidingDoors/SlidingDoor_D24.prefab", "Assets/Game/Models/Buildings/Mechanisms/SlidingDoors/SlidingDoor_D24.fbx")]
        [TestCase("Assets/Game/Prefabs/Entity/Buildings/Mechanisms/SlidingDoors/SlidingDoor_D40.prefab", "Assets/Game/Models/Buildings/Mechanisms/SlidingDoors/SlidingDoor_D40.fbx")]
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

        [TestCase("wheel_module_steer_roll", "Assets/Game/Prefabs/Entity/Machines/Modules/WheelModule.prefab")]
        [TestCase("carrier_drive", "Assets/Game/Prefabs/Entity/Machines/Carriers/WheeledCarrier.prefab")]
        [TestCase("carrier_crab", "Assets/Game/Prefabs/Entity/Machines/Carriers/WheeledCarrier.prefab")]
        [TestCase("cargo_transfer_open", "Assets/Game/Prefabs/Entity/Machines/Modules/CargoPod.prefab")]
        [TestCase("fixed_rotary_scan", "Assets/Game/Prefabs/Entity/Machines/Carriers/FixedRotaryCarrier.prefab")]
        [TestCase("arm_work_envelope", "Assets/Game/Prefabs/Entity/Machines/Effectors/MultiJointArm.prefab")]
        [TestCase("water_aim_spray", "Assets/Game/Prefabs/Entity/Machines/Effectors/WaterCannon.prefab")]
        [TestCase("saw_cut_cycle", "Assets/Game/Prefabs/Entity/Machines/Effectors/RotarySaw.prefab")]
        [TestCase("drill_mine_cycle", "Assets/Game/Prefabs/Entity/Machines/Effectors/RotaryDrill.prefab")]
        [TestCase("conveyor_run", "Assets/Game/Prefabs/Entity/Buildings/Logistics/Conveyor.prefab")]
        [TestCase("sliding_door_open", "Assets/Game/Prefabs/Entity/Buildings/Mechanisms/SlidingDoors/SlidingDoor_D24.prefab")]
        [TestCase("sliding_door_open", "Assets/Game/Prefabs/Entity/Buildings/Mechanisms/SlidingDoors/SlidingDoor_D40.prefab")]
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
