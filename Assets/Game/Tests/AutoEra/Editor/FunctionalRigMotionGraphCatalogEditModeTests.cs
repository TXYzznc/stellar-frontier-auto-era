using AutoEra.Motion;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class FunctionalRigMotionGraphCatalogEditModeTests
    {
        private const string GraphFolder = "Assets/Game/MotionGraphs/FunctionalPrototypes/";
        private const string PrefabFolder = "Assets/Game/Prefabs/FunctionalPrototypes/Catalog/";

        [TestCase("carrier_drive", "wheeled_carrier")]
        [TestCase("carrier_crab", "wheeled_carrier")]
        [TestCase("wheel_module_steer_roll", "four_wheel_module")]
        [TestCase("arm_work_envelope", "multi_joint_arm")]
        [TestCase("water_aim_spray", "water_sprayer")]
        [TestCase("saw_cut_cycle", "rotary_saw")]
        [TestCase("drill_mine_cycle", "rotary_drill")]
        [TestCase("sliding_door_open", "sliding_door")]
        [TestCase("conveyor_run", "conveyor")]
        [TestCase("effector_ui_swap", "replaceable_effector")]
        [TestCase("fixed_rotary_scan", "fixed_rotary_carrier")]
        [TestCase("cargo_transfer_open", "cargo_bay")]
        public void CatalogGraph_IsCompatibleOnlyWithItsDeclaredPrototype(string graphId, string familyId)
        {
            MotionGraphAsset graph = AssetDatabase.LoadAssetAtPath<MotionGraphAsset>(GraphFolder + graphId + ".asset");
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + familyId + ".prefab");

            Assert.That(graph, Is.Not.Null, graphId);
            Assert.That(prefab, Is.Not.Null, familyId);
            MotionRig rig = prefab.GetComponent<MotionRig>();
            Assert.That(rig, Is.Not.Null, familyId);
            Assert.That(graph.TargetContractId, Is.EqualTo(rig.ContractId));
            Assert.That(graph.IsCompatibleWith(rig), Is.True);
        }

        [Test]
        public void EveryFunctionalPrototypeFamily_HasAtLeastOneCompatiblePreviewGraph()
        {
            MotionGraphAsset[] graphs = AssetDatabase.FindAssets("t:MotionGraphAsset", new[] { GraphFolder.TrimEnd('/') })
                .Select(guid => AssetDatabase.LoadAssetAtPath<MotionGraphAsset>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(graph => graph != null)
                .ToArray();

            foreach (string familyId in FunctionalRigPrototypeCatalog.AssetFamilyIds)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + familyId + ".prefab");
                MotionRig rig = prefab == null ? null : prefab.GetComponent<MotionRig>();
                Assert.That(rig, Is.Not.Null, familyId);
                Assert.That(graphs.Any(graph => graph.IsCompatibleWith(rig)), Is.True, familyId + " requires a compatible preview graph.");
            }
        }
    }
}
