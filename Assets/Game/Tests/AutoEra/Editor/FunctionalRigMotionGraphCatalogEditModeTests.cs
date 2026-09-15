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

        [TestCase("carrier_drive", "Assets/Game/Prefabs/Entity/Machines/WheeledCarrier.prefab")]
        [TestCase("carrier_crab", "Assets/Game/Prefabs/Entity/Machines/WheeledCarrier.prefab")]
        [TestCase("wheel_module_steer_roll", "Assets/Game/Prefabs/Entity/Machines/WheelModule.prefab")]
        [TestCase("arm_work_envelope", "Assets/Game/Prefabs/Entity/Machines/MultiJointArm.prefab")]
        [TestCase("water_aim_spray", "Assets/Game/Prefabs/Entity/Machines/WaterCannon.prefab")]
        [TestCase("saw_cut_cycle", "Assets/Game/Prefabs/Entity/Machines/RotarySaw.prefab")]
        [TestCase("drill_mine_cycle", "Assets/Game/Prefabs/Entity/Machines/RotaryDrill.prefab")]
        [TestCase("sliding_door_open", "Assets/Game/Prefabs/Entity/Buildings/SlidingDoor_D24.prefab")]
        [TestCase("conveyor_run", "Assets/Game/Prefabs/Entity/Buildings/Conveyor.prefab")]
        [TestCase("fixed_rotary_scan", "Assets/Game/Prefabs/Entity/Machines/FixedRotaryCarrier.prefab")]
        [TestCase("cargo_transfer_open", "Assets/Game/Prefabs/Entity/Machines/CargoPod.prefab")]
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
                "Assets/Game/Prefabs/Entity/Machines/WheeledCarrier.prefab",
                "Assets/Game/Prefabs/Entity/Machines/WheelModule.prefab",
                "Assets/Game/Prefabs/Entity/Machines/MultiJointArm.prefab",
                "Assets/Game/Prefabs/Entity/Machines/WaterCannon.prefab",
                "Assets/Game/Prefabs/Entity/Machines/RotarySaw.prefab",
                "Assets/Game/Prefabs/Entity/Machines/RotaryDrill.prefab",
                "Assets/Game/Prefabs/Entity/Machines/CargoPod.prefab",
                "Assets/Game/Prefabs/Entity/Machines/FixedRotaryCarrier.prefab",
                "Assets/Game/Prefabs/Entity/Buildings/Conveyor.prefab",
                "Assets/Game/Prefabs/Entity/Buildings/SlidingDoor_D24.prefab",
                "Assets/Game/Prefabs/Entity/Buildings/SlidingDoor_D40.prefab"
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
