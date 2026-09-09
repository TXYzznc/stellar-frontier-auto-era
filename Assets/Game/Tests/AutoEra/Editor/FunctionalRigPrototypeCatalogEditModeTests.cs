using AutoEra.Motion;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class FunctionalRigPrototypeCatalogEditModeTests
    {
        private static readonly KeyValuePair<string, string>[] FormalPreviewPrefabByFamily =
        {
            new KeyValuePair<string, string>("wheeled_carrier", "Assets/Game/Prefabs/Entity/Machines/Carriers/WheeledCarrier.prefab"),
            new KeyValuePair<string, string>("four_wheel_module", "Assets/Game/Prefabs/Entity/Machines/Modules/WheelModule.prefab"),
            new KeyValuePair<string, string>("multi_joint_arm", "Assets/Game/Prefabs/Entity/Machines/Effectors/MultiJointArm.prefab"),
            new KeyValuePair<string, string>("sliding_door", "Assets/Game/Prefabs/Entity/Buildings/Mechanisms/SlidingDoors/SlidingDoor_D24.prefab"),
            new KeyValuePair<string, string>("conveyor", "Assets/Game/Prefabs/Entity/Buildings/Logistics/Conveyor.prefab"),
            new KeyValuePair<string, string>("water_sprayer", "Assets/Game/Prefabs/Entity/Machines/Effectors/WaterCannon.prefab"),
            new KeyValuePair<string, string>("rotary_saw", "Assets/Game/Prefabs/Entity/Machines/Effectors/RotarySaw.prefab"),
            new KeyValuePair<string, string>("rotary_drill", "Assets/Game/Prefabs/Entity/Machines/Effectors/RotaryDrill.prefab"),
            new KeyValuePair<string, string>("cargo_bay", "Assets/Game/Prefabs/Entity/Machines/Modules/CargoPod.prefab"),
            new KeyValuePair<string, string>("fixed_rotary_carrier", "Assets/Game/Prefabs/Entity/Machines/Carriers/FixedRotaryCarrier.prefab")
        };

        [Test]
        public void Catalog_MapsPreviewableContractsToFormalEntityPrefabs()
        {
            foreach (KeyValuePair<string, string> entry in FormalPreviewPrefabByFamily)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(entry.Value);
                Assert.That(prefab, Is.Not.Null, entry.Key);
                FunctionalRigPrototypeHierarchy hierarchy = prefab.GetComponent<FunctionalRigPrototypeHierarchy>();
                Assert.That(hierarchy, Is.Not.Null, entry.Key);
                Assert.That(hierarchy.TryValidate(out string error), Is.True, error);
                MotionRig rig = prefab.GetComponent<MotionRig>();
                Assert.That(rig, Is.Not.Null, entry.Key + " requires a MotionRig for the unified preview tool.");
                Assert.That(rig.TryValidate(out string rigError), Is.True, rigError);
            }
        }
    }
}
