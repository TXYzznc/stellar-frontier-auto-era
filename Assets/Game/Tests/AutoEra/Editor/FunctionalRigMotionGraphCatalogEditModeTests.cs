using AutoEra.Motion;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class FunctionalRigMotionGraphCatalogEditModeTests
    {
        private const string GraphFolder = "Assets/Game/MotionGraphs/Entity/";

        [TestCase("carrier_drive", "Assets/Game/Prefabs/Entity/Machines/Carriers/WheeledCarrier.prefab")]
        [TestCase("carrier_crab", "Assets/Game/Prefabs/Entity/Machines/Carriers/WheeledCarrier.prefab")]
        [TestCase("wheel_module_steer_roll", "Assets/Game/Prefabs/Entity/Machines/Modules/WheelModule.prefab")]
        [TestCase("arm_work_envelope", "Assets/Game/Prefabs/Entity/Machines/Effectors/MultiJointArm.prefab")]
        [TestCase("water_aim_spray", "Assets/Game/Prefabs/Entity/Machines/Effectors/WaterCannon.prefab")]
        [TestCase("saw_cut_cycle", "Assets/Game/Prefabs/Entity/Machines/Effectors/RotarySaw.prefab")]
        [TestCase("drill_mine_cycle", "Assets/Game/Prefabs/Entity/Machines/Effectors/RotaryDrill.prefab")]
        [TestCase("sliding_door_open", "Assets/Game/Prefabs/Entity/Buildings/Mechanisms/SlidingDoors/SlidingDoor_D24.prefab")]
        [TestCase("conveyor_run", "Assets/Game/Prefabs/Entity/Buildings/Logistics/Conveyor.prefab")]
        [TestCase("fixed_rotary_scan", "Assets/Game/Prefabs/Entity/Machines/Carriers/FixedRotaryCarrier.prefab")]
        [TestCase("cargo_transfer_open", "Assets/Game/Prefabs/Entity/Machines/Modules/CargoPod.prefab")]
        public void CatalogGraph_IsCompatibleWithItsDeclaredFormalEntity(string graphId, string prefabPath)
        {
            MotionGraphAsset graph = AssetDatabase.LoadAssetAtPath<MotionGraphAsset>(GraphFolder + graphId + ".asset");
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            Assert.That(graph, Is.Not.Null, graphId);
            Assert.That(prefab, Is.Not.Null, prefabPath);
            MotionRig rig = prefab.GetComponent<MotionRig>();
            Assert.That(rig, Is.Not.Null, prefabPath);
            Assert.That(graph.TargetContractId, Is.EqualTo(rig.ContractId));
            Assert.That(graph.IsCompatibleWith(rig), Is.True);
        }

        [Test]
        public void EveryFormalEntityMotionGraph_HasACompatibleFormalEntity()
        {
            MotionGraphAsset[] graphs = AssetDatabase.FindAssets("t:MotionGraphAsset", new[] { GraphFolder.TrimEnd('/') })
                .Select(guid => AssetDatabase.LoadAssetAtPath<MotionGraphAsset>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(graph => graph != null)
                .ToArray();

            foreach (string prefabPath in new[]
            {
                "Assets/Game/Prefabs/Entity/Machines/Carriers/WheeledCarrier.prefab",
                "Assets/Game/Prefabs/Entity/Machines/Modules/WheelModule.prefab",
                "Assets/Game/Prefabs/Entity/Machines/Effectors/MultiJointArm.prefab",
                "Assets/Game/Prefabs/Entity/Machines/Effectors/WaterCannon.prefab",
                "Assets/Game/Prefabs/Entity/Machines/Effectors/RotarySaw.prefab",
                "Assets/Game/Prefabs/Entity/Machines/Effectors/RotaryDrill.prefab",
                "Assets/Game/Prefabs/Entity/Machines/Modules/CargoPod.prefab",
                "Assets/Game/Prefabs/Entity/Machines/Carriers/FixedRotaryCarrier.prefab",
                "Assets/Game/Prefabs/Entity/Buildings/Logistics/Conveyor.prefab",
                "Assets/Game/Prefabs/Entity/Buildings/Mechanisms/SlidingDoors/SlidingDoor_D24.prefab",
                "Assets/Game/Prefabs/Entity/Buildings/Mechanisms/SlidingDoors/SlidingDoor_D40.prefab"
            })
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                MotionRig rig = prefab == null ? null : prefab.GetComponent<MotionRig>();
                Assert.That(rig, Is.Not.Null, prefabPath);
                Assert.That(graphs.Any(graph => graph.IsCompatibleWith(rig)), Is.True, prefabPath + " requires a compatible preview graph.");
            }
        }
    }
}
