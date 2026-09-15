using System;
using System.Collections.Generic;
using AutoEra.Motion;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor.Motion
{
    /// <summary>
    /// Formal entry point for static entity assets.
    /// 1. Externalizes FBX-embedded materials into standalone Material assets so that
    ///    individual sub-parts can gain their own material later without touching the FBX.
    /// 2. Builds the missing static entity prefabs using the shared entity hierarchy contract.
    /// </summary>
    public static class AutoEraStaticEntityAssetBuilder
    {
        private const string ExternalizeMaterialsMenuPath = "AutoEra/Entity/Externalize Model Materials";
        private const string BuildStaticPrefabsMenuPath = "AutoEra/Entity/Build Static Entity Prefabs";

        private static readonly Entry[] StaticEntries =
        {
            new Entry("BasicBattery", "Assets/Game/Models/Buildings/BasicBattery.fbx", "Assets/Game/Materials/Buildings/Static/BasicBattery/BasicBattery.mat", "Assets/Game/Prefabs/Entity/Buildings/BasicBattery.prefab"),
            new Entry("BasicWarehouse", "Assets/Game/Models/Buildings/BasicWarehouse.fbx", "Assets/Game/Materials/Buildings/Static/BasicWarehouse/BasicWarehouse.mat", "Assets/Game/Prefabs/Entity/Buildings/BasicWarehouse.prefab"),
            new Entry("BiomassGenerator", "Assets/Game/Models/Buildings/BiomassGenerator.fbx", "Assets/Game/Materials/Buildings/Static/BiomassGenerator/BiomassGenerator.mat", "Assets/Game/Prefabs/Entity/Buildings/BiomassGenerator.prefab"),
            new Entry("ManufacturingWorkshop", "Assets/Game/Models/Buildings/ManufacturingWorkshop.fbx", "Assets/Game/Materials/Buildings/Static/ManufacturingWorkshop/ManufacturingWorkshop.mat", "Assets/Game/Prefabs/Entity/Buildings/ManufacturingWorkshop.prefab"),
            new Entry("ShoreWaterPump", "Assets/Game/Models/Buildings/ShoreWaterPump.fbx", "Assets/Game/Materials/Buildings/Static/ShoreWaterPump/ShoreWaterPump.mat", "Assets/Game/Prefabs/Entity/Buildings/ShoreWaterPump.prefab"),
            new Entry("SolarGenerator", "Assets/Game/Models/Buildings/SolarGenerator.fbx", "Assets/Game/Materials/Buildings/Static/SolarGenerator/SolarGenerator.mat", "Assets/Game/Prefabs/Entity/Buildings/SolarGenerator.prefab"),
            new Entry("BasicComputeCore", "Assets/Game/Models/Machines/BasicComputeCore.fbx", "Assets/Game/Materials/Machines/Modules/StaticComponents/BasicComputeCore/BasicComputeCore.mat", "Assets/Game/Prefabs/Entity/Machines/BasicComputeCore.prefab"),
            new Entry("CommunicationReceiver", "Assets/Game/Models/Machines/CommunicationReceiver.fbx", "Assets/Game/Materials/Machines/Modules/StaticComponents/CommunicationReceiver/CommunicationReceiver.mat", "Assets/Game/Prefabs/Entity/Machines/CommunicationReceiver.prefab"),
            new Entry("CommunicationTransmitter", "Assets/Game/Models/Machines/CommunicationTransmitter.fbx", "Assets/Game/Materials/Machines/Modules/StaticComponents/CommunicationTransmitter/CommunicationTransmitter.mat", "Assets/Game/Prefabs/Entity/Machines/CommunicationTransmitter.prefab"),
            new Entry("ExplorationScanner", "Assets/Game/Models/Machines/ExplorationScanner.fbx", "Assets/Game/Materials/Machines/Modules/StaticComponents/ExplorationScanner/ExplorationScanner.mat", "Assets/Game/Prefabs/Entity/Machines/ExplorationScanner.prefab"),
            new Entry("ObjectStateSensor", "Assets/Game/Models/Machines/ObjectStateSensor.fbx", "Assets/Game/Materials/Machines/Modules/StaticComponents/ObjectStateSensor/ObjectStateSensor.mat", "Assets/Game/Prefabs/Entity/Machines/ObjectStateSensor.prefab"),
            new Entry("SoilSensor", "Assets/Game/Models/Machines/SoilSensor.fbx", "Assets/Game/Materials/Machines/Modules/StaticComponents/SoilSensor/SoilSensor.mat", "Assets/Game/Prefabs/Entity/Machines/SoilSensor.prefab")
        };

        /// <summary>
        /// Models whose prefabs already exist and are authored externally. They still need the
        /// FBX material remap so that dragging the raw model never yields embedded materials.
        /// </summary>
        private static readonly ModelMaterialEntry[] AuthoredEntries =
        {
            new ModelMaterialEntry("WheelModule", "Assets/Game/Models/Machines/WheelModule.fbx", "Assets/Game/Materials/Machines/Modules/WheelModule.mat"),
            new ModelMaterialEntry("WheeledCarrier", "Assets/Game/Models/Machines/WheeledCarrier.fbx", "Assets/Game/Materials/Machines/Carriers/WheeledCarrier.mat"),
            new ModelMaterialEntry("CargoPod", "Assets/Game/Models/Machines/CargoPod.fbx", "Assets/Game/Materials/Machines/Modules/CargoPod.mat"),
            new ModelMaterialEntry("FixedRotaryCarrier", "Assets/Game/Models/Machines/FixedRotaryCarrier.fbx", "Assets/Game/Materials/Machines/Carriers/FixedRotaryCarrier.mat"),
            new ModelMaterialEntry("MultiJointArm", "Assets/Game/Models/Machines/MultiJointArm.fbx", "Assets/Game/Materials/Machines/Effectors/MultiJointArm.mat"),
            new ModelMaterialEntry("WaterCannon", "Assets/Game/Models/Machines/WaterCannon.fbx", "Assets/Game/Materials/Machines/Effectors/WaterCannon.mat"),
            new ModelMaterialEntry("RotarySaw", "Assets/Game/Models/Machines/RotarySaw.fbx", "Assets/Game/Materials/Machines/Effectors/RotarySaw.mat"),
            new ModelMaterialEntry("RotaryDrill", "Assets/Game/Models/Machines/RotaryDrill.fbx", "Assets/Game/Materials/Machines/Effectors/RotaryDrill.mat"),
            new ModelMaterialEntry("Conveyor", "Assets/Game/Models/Buildings/Conveyor.fbx", "Assets/Game/Materials/Buildings/Logistics/Conveyor/Conveyor.mat"),
            new ModelMaterialEntry("SlidingDoor_D24", "Assets/Game/Models/Buildings/SlidingDoor_D24.fbx", "Assets/Game/Materials/Buildings/Mechanisms/SlidingDoors/SlidingDoor_D24.mat"),
            new ModelMaterialEntry("SlidingDoor_D40", "Assets/Game/Models/Buildings/SlidingDoor_D40.fbx", "Assets/Game/Materials/Buildings/Mechanisms/SlidingDoors/SlidingDoor_D40.mat")
        };

        [MenuItem(ExternalizeMaterialsMenuPath)]
        public static void ExternalizeAllModelMaterials()
        {
            List<ModelMaterialEntry> all = new List<ModelMaterialEntry>();
            foreach (Entry entry in StaticEntries) all.Add(new ModelMaterialEntry(entry.Id, entry.ModelPath, entry.MaterialPath));
            all.AddRange(AuthoredEntries);

            int remapped = 0;
            foreach (ModelMaterialEntry entry in all) remapped += Externalize(entry);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AutoEra] Material externalization finished. FBX remapped: " + remapped + " / " + all.Count);
        }

        [MenuItem(BuildStaticPrefabsMenuPath)]
        public static void BuildStaticEntityPrefabs()
        {
            int built = 0;
            foreach (Entry entry in StaticEntries)
            {
                if (Build(entry)) built++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AutoEra] Static entity prefabs built: " + built + " / " + StaticEntries.Length);
        }

        private static int Externalize(ModelMaterialEntry entry)
        {
            ModelImporter importer = AssetImporter.GetAtPath(entry.ModelPath) as ModelImporter;
            if (importer == null)
            {
                Debug.LogWarning("[AutoEra] No ModelImporter found at: " + entry.ModelPath);
                return 0;
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(entry.MaterialPath);
            if (material == null)
            {
                Debug.LogWarning("[AutoEra] Missing standalone material: " + entry.MaterialPath);
                return 0;
            }

            List<string> sourceNames = new List<string>();
            foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(entry.ModelPath))
            {
                if (asset is Material embedded && !string.IsNullOrEmpty(embedded.name)) sourceNames.Add(embedded.name);
            }

            if (sourceNames.Count == 0) sourceNames.Add(entry.Id);

            HashSet<string> mapped = new HashSet<string>(StringComparer.Ordinal);
            foreach (KeyValuePair<AssetImporter.SourceAssetIdentifier, UnityEngine.Object> pair in importer.GetExternalObjectMap())
            {
                if (pair.Key.type == typeof(Material)) mapped.Add(pair.Key.name);
            }

            List<string> added = new List<string>();
            foreach (string sourceName in sourceNames)
            {
                if (mapped.Contains(sourceName)) continue;
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), sourceName), material);
                mapped.Add(sourceName);
                added.Add(sourceName);
            }

            if (added.Count == 0) return 0;

            importer.SaveAndReimport();
            Debug.Log("[AutoEra] Externalized " + entry.ModelPath + " material slot(s) [" + string.Join(", ", added) + "] -> " + entry.MaterialPath);
            return 1;
        }

        private static bool Build(Entry entry)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(entry.PrefabPath) != null)
            {
                Debug.LogWarning("[AutoEra] Prefab already exists, skipped: " + entry.PrefabPath);
                return false;
            }

            GameObject sourceModel = AssetDatabase.LoadAssetAtPath<GameObject>(entry.ModelPath);
            if (sourceModel == null) throw new InvalidOperationException("Missing model: " + entry.ModelPath);

            Material material = AssetDatabase.LoadAssetAtPath<Material>(entry.MaterialPath);
            if (material == null) throw new InvalidOperationException("Missing material: " + entry.MaterialPath);

            EnsureParentFolders(entry.PrefabPath);

            GameObject root = new GameObject(entry.Id);
            try
            {
                Transform logicRoot = CreateChild(root.transform, "LogicRoot");
                Transform rigRoot = CreateChild(root.transform, "RigRoot");
                Transform visualRoot = CreateChild(rigRoot, "VisualRoot");
                Transform collisionRoot = CreateChild(root.transform, "AuthorityCollisionRoot");

                BoxCollider authorityCollider = collisionRoot.gameObject.AddComponent<BoxCollider>();
                authorityCollider.enabled = false;

                FunctionalRigPrototypeHierarchy hierarchy = root.AddComponent<FunctionalRigPrototypeHierarchy>();
                hierarchy.Configure(logicRoot, rigRoot, visualRoot, collisionRoot);

                GameObject visualModel = PrefabUtility.InstantiatePrefab(sourceModel) as GameObject;
                if (visualModel == null) throw new InvalidOperationException("Could not instantiate model: " + entry.ModelPath);
                visualModel.transform.SetParent(visualRoot, false);
                visualModel.name = "VisualModel";

                ApplyMaterial(visualModel, material);

                if (!hierarchy.TryValidate(out string hierarchyError))
                    throw new InvalidOperationException(entry.Id + " hierarchy invalid: " + hierarchyError);

                PrefabUtility.SaveAsPrefabAsset(root, entry.PrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }

            return true;
        }

        private static void ApplyMaterial(GameObject visualModel, Material material)
        {
            foreach (Renderer renderer in visualModel.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                if (materials == null || materials.Length == 0)
                {
                    renderer.sharedMaterial = material;
                    continue;
                }

                for (int index = 0; index < materials.Length; index++) materials[index] = material;
                renderer.sharedMaterials = materials;
            }
        }

        private static Transform CreateChild(Transform parent, string name)
        {
            Transform child = new GameObject(name).transform;
            child.SetParent(parent, false);
            return child;
        }

        private static void EnsureParentFolders(string assetPath)
        {
            string[] parts = assetPath.Split('/');
            string current = parts[0];
            for (int index = 1; index < parts.Length - 1; index++)
            {
                string next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[index]);
                current = next;
            }
        }

        private sealed class ModelMaterialEntry
        {
            public readonly string Id;
            public readonly string ModelPath;
            public readonly string MaterialPath;

            public ModelMaterialEntry(string id, string modelPath, string materialPath)
            {
                Id = id;
                ModelPath = modelPath;
                MaterialPath = materialPath;
            }
        }

        private sealed class Entry
        {
            public readonly string Id;
            public readonly string ModelPath;
            public readonly string MaterialPath;
            public readonly string PrefabPath;

            public Entry(string id, string modelPath, string materialPath, string prefabPath)
            {
                Id = id;
                ModelPath = modelPath;
                MaterialPath = materialPath;
                PrefabPath = prefabPath;
            }
        }
    }
}