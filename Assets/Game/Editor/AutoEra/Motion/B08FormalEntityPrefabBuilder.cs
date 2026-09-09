using System;
using System.Collections.Generic;
using AutoEra.Motion;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor.Motion
{
    /// <summary>
    /// Builds the formal runtime prefab entry points from the accepted B08 visual models.
    /// The B08 JSON is the joint/anchor authority; ContractProxy meshes remain development-only
    /// validation assets and are deliberately not nested in Entity prefabs.
    /// </summary>
    public static class B08FormalEntityPrefabBuilder
    {
        private const string MenuPath = "AutoEra/Motion/Build B08 Formal Entity Prefabs";

        private static readonly Entry[] Entries =
        {
            new Entry("WheelModule", "four_wheel_module_prototype", "Assets/Game/Models/Machines/Modules/WheelModule.fbx", "Assets/Game/Materials/Machines/Modules/WheelModule.mat", "Assets/Game/Textures/Machines/Modules/WheelModule", "Assets/Game/Config/MotionContracts/B08/WheelModule.json", "Assets/Game/Prefabs/Entity/Machines/Modules/WheelModule.prefab"),
            new Entry("WheeledCarrier", "wheeled_carrier_prototype", "Assets/Game/Models/Machines/Carriers/WheeledCarrier.fbx", "Assets/Game/Materials/Machines/Carriers/WheeledCarrier.mat", "Assets/Game/Textures/Machines/Carriers/WheeledCarrier", "Assets/Game/Config/MotionContracts/B08/WheeledCarrier.json", "Assets/Game/Prefabs/Entity/Machines/Carriers/WheeledCarrier.prefab"),
            new Entry("CargoPod", "cargo_bay_prototype", "Assets/Game/Models/Machines/Modules/CargoPod.fbx", "Assets/Game/Materials/Machines/Modules/CargoPod.mat", "Assets/Game/Textures/Machines/Modules/CargoPod", "Assets/Game/Config/MotionContracts/B08/CargoPod.json", "Assets/Game/Prefabs/Entity/Machines/Modules/CargoPod.prefab"),
            new Entry("FixedRotaryCarrier", "fixed_rotary_carrier_prototype", "Assets/Game/Models/Machines/Carriers/FixedRotaryCarrier.fbx", "Assets/Game/Materials/Machines/Carriers/FixedRotaryCarrier.mat", "Assets/Game/Textures/Machines/Carriers/FixedRotaryCarrier", "Assets/Game/Config/MotionContracts/B08/FixedRotaryCarrier.json", "Assets/Game/Prefabs/Entity/Machines/Carriers/FixedRotaryCarrier.prefab"),
            new Entry("MultiJointArm", "multi_joint_arm_prototype", "Assets/Game/Models/Machines/Effectors/MultiJointArm.fbx", "Assets/Game/Materials/Machines/Effectors/MultiJointArm.mat", "Assets/Game/Textures/Machines/Effectors/MultiJointArm", "Assets/Game/Config/MotionContracts/B08/MultiJointArm.json", "Assets/Game/Prefabs/Entity/Machines/Effectors/MultiJointArm.prefab"),
            new Entry("WaterCannon", "water_sprayer_prototype", "Assets/Game/Models/Machines/Effectors/WaterCannon.fbx", "Assets/Game/Materials/Machines/Effectors/WaterCannon.mat", "Assets/Game/Textures/Machines/Effectors/WaterCannon", "Assets/Game/Config/MotionContracts/B08/WaterCannon.json", "Assets/Game/Prefabs/Entity/Machines/Effectors/WaterCannon.prefab"),
            new Entry("RotarySaw", "rotary_saw_prototype", "Assets/Game/Models/Machines/Effectors/RotarySaw.fbx", "Assets/Game/Materials/Machines/Effectors/RotarySaw.mat", "Assets/Game/Textures/Machines/Effectors/RotarySaw", "Assets/Game/Config/MotionContracts/B08/RotarySaw.json", "Assets/Game/Prefabs/Entity/Machines/Effectors/RotarySaw.prefab"),
            new Entry("RotaryDrill", "rotary_drill_prototype", "Assets/Game/Models/Machines/Effectors/RotaryDrill.fbx", "Assets/Game/Materials/Machines/Effectors/RotaryDrill.mat", "Assets/Game/Textures/Machines/Effectors/RotaryDrill", "Assets/Game/Config/MotionContracts/B08/RotaryDrill.json", "Assets/Game/Prefabs/Entity/Machines/Effectors/RotaryDrill.prefab"),
            new Entry("Conveyor", "conveyor_prototype", "Assets/Game/Models/Buildings/Logistics/Conveyor/Conveyor.fbx", "Assets/Game/Materials/Buildings/Logistics/Conveyor/Conveyor.mat", "Assets/Game/Textures/Buildings/Logistics/Conveyor", "Assets/Game/Config/MotionContracts/B08/Conveyor.json", "Assets/Game/Prefabs/Entity/Buildings/Logistics/Conveyor.prefab"),
            new Entry("SlidingDoor_D24", "sliding_door_prototype", "Assets/Game/Models/Buildings/Mechanisms/SlidingDoors/SlidingDoor_D24.fbx", "Assets/Game/Materials/Buildings/Mechanisms/SlidingDoors/SlidingDoor_D24.mat", "Assets/Game/Textures/Buildings/Mechanisms/SlidingDoors/SlidingDoor_D24", "Assets/Game/Config/MotionContracts/B08/SlidingDoor_D24.json", "Assets/Game/Prefabs/Entity/Buildings/Mechanisms/SlidingDoors/SlidingDoor_D24.prefab"),
            new Entry("SlidingDoor_D40", "sliding_door_prototype", "Assets/Game/Models/Buildings/Mechanisms/SlidingDoors/SlidingDoor_D40.fbx", "Assets/Game/Materials/Buildings/Mechanisms/SlidingDoors/SlidingDoor_D40.mat", "Assets/Game/Textures/Buildings/Mechanisms/SlidingDoors/SlidingDoor_D40", "Assets/Game/Config/MotionContracts/B08/SlidingDoor_D40.json", "Assets/Game/Prefabs/Entity/Buildings/Mechanisms/SlidingDoors/SlidingDoor_D40.prefab")
        };

        public static IReadOnlyList<string> FormalPrefabPaths
        {
            get
            {
                List<string> paths = new List<string>(Entries.Length);
                foreach (Entry entry in Entries) paths.Add(entry.PrefabPath);
                return paths;
            }
        }

        [MenuItem(MenuPath)]
        public static void BuildAll()
        {
            foreach (Entry entry in Entries) Build(entry);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void Build(Entry entry)
        {
            GameObject sourceModel = AssetDatabase.LoadAssetAtPath<GameObject>(entry.ModelPath);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(entry.MaterialPath);
            TextAsset contractAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(entry.ContractPath);
            if (sourceModel == null) throw new InvalidOperationException("Missing accepted B08 model: " + entry.ModelPath);
            if (material == null) throw new InvalidOperationException("Missing accepted B08 material: " + entry.MaterialPath);
            if (contractAsset == null) throw new InvalidOperationException("Missing B08 motion contract: " + entry.ContractPath);

            ApplyRuntimeTextures(material, entry);

            B08MotionContract contract = JsonUtility.FromJson<B08MotionContract>(contractAsset.text);
            if (contract == null || contract.objects == null || contract.objects.Length == 0)
                throw new InvalidOperationException("Invalid B08 motion contract: " + entry.ContractPath);

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
                if (visualModel == null) throw new InvalidOperationException("Could not instantiate accepted B08 model: " + entry.ModelPath);
                visualModel.transform.SetParent(visualRoot, false);
                visualModel.name = "VisualModel";
                ApplyMaterial(visualModel, material);

                MotionRig rig = root.AddComponent<MotionRig>();
                rig.Configure(CreateBindings(visualRoot, contract, entry.ContractId), entry.ContractId);
                if (!hierarchy.TryValidate(out string hierarchyError)) throw new InvalidOperationException(entry.Id + " hierarchy invalid: " + hierarchyError);
                if (!rig.TryValidate(out string rigError)) throw new InvalidOperationException(entry.Id + " rig invalid: " + rigError);

                PrefabUtility.SaveAsPrefabAsset(root, entry.PrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static MotionJointBinding[] CreateBindings(Transform visualRoot, B08MotionContract contract, string contractId)
        {
            Dictionary<string, Transform> transformsByName = new Dictionary<string, Transform>(StringComparer.Ordinal);
            foreach (Transform candidate in visualRoot.GetComponentsInChildren<Transform>(true))
            {
                if (!transformsByName.ContainsKey(candidate.name)) transformsByName.Add(candidate.name, candidate);
            }

            Dictionary<string, B08ContractObject> objectsByStableId = new Dictionary<string, B08ContractObject>(StringComparer.Ordinal);
            foreach (B08ContractObject item in contract.objects) objectsByStableId.Add(item.stableId, item);
            Dictionary<string, Transform> transformsByStableId = new Dictionary<string, Transform>(StringComparer.Ordinal);

            List<MotionJointBinding> bindings = new List<MotionJointBinding>();
            foreach (B08ContractObject item in contract.objects)
            {
                if (!string.Equals(item.role, "joint", StringComparison.Ordinal)) continue;
                Transform transform = ResolveContractTransform(item, visualRoot, transformsByName, objectsByStableId, transformsByStableId, contractId);

                Vector3 axis = ToVector(item.localAxis, Vector3.up);
                MotionJointChannel channel = string.Equals(item.jointType, "translation", StringComparison.Ordinal)
                    ? MotionJointChannel.Translation
                    : MotionJointChannel.Rotation;
                Vector3 bindPosition = ToVector(item.localPosition, transform.localPosition);
                Vector3 bindEuler = ToVector(item.localEulerDegrees, transform.localEulerAngles);
                // B08 publishes scalar safe values, while MotionRig stores full local poses. Preserve
                // the authored bind pose here; graph commands apply the scalar ranges at preview/runtime.
                bindings.Add(new MotionJointBinding(item.stableId, transform, channel, axis, item.rangeMin, item.rangeMax, bindPosition, bindEuler, bindPosition, bindEuler));
            }

            return bindings.ToArray();
        }

        private static Transform ResolveContractTransform(
            B08ContractObject item,
            Transform visualRoot,
            Dictionary<string, Transform> transformsByName,
            Dictionary<string, B08ContractObject> objectsByStableId,
            Dictionary<string, Transform> transformsByStableId,
            string contractId)
        {
            if (transformsByStableId.TryGetValue(item.stableId, out Transform resolved)) return resolved;

            if (!TryFindModelTransform(item.name, transformsByName, out resolved))
            {
                Transform parent = visualRoot;
                if (!string.IsNullOrEmpty(item.parentStableId))
                {
                    if (!objectsByStableId.TryGetValue(item.parentStableId, out B08ContractObject parentItem))
                        throw new InvalidOperationException(contractId + " references unknown parent stable ID '" + item.parentStableId + "'.");
                    parent = ResolveContractTransform(parentItem, visualRoot, transformsByName, objectsByStableId, transformsByStableId, contractId);
                }

                // Some B08 entries (for example wheel contact points) are logical control nodes
                // without a separately modelled mesh node. Materialize only those missing nodes;
                // all physical model hierarchies remain intact and directly drive their meshes.
                resolved = new GameObject(item.name).transform;
                resolved.SetParent(parent, false);
                resolved.localPosition = ToVector(item.localPosition, Vector3.zero);
                resolved.localEulerAngles = ToVector(item.localEulerDegrees, Vector3.zero);
                transformsByName.Add(item.name, resolved);
            }

            transformsByStableId.Add(item.stableId, resolved);
            return resolved;
        }

        private static bool TryFindModelTransform(string contractName, Dictionary<string, Transform> transformsByName, out Transform transform)
        {
            if (transformsByName.TryGetValue(contractName, out transform)) return true;
            const string jointPrefix = "Joint_";
            return contractName.StartsWith(jointPrefix, StringComparison.Ordinal)
                && transformsByName.TryGetValue(contractName.Substring(jointPrefix.Length), out transform);
        }

        private static Vector3 ToVector(float[] values, Vector3 fallback)
        {
            return values != null && values.Length == 3 ? new Vector3(values[0], values[1], values[2]) : fallback;
        }

        private static void ApplyMaterial(GameObject visualModel, Material material)
        {
            foreach (Renderer renderer in visualModel.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                if (materials == null || materials.Length == 0) renderer.sharedMaterial = material;
                else
                {
                    for (int index = 0; index < materials.Length; index++) materials[index] = material;
                    renderer.sharedMaterials = materials;
                }
            }
        }

        private static void ApplyRuntimeTextures(Material material, Entry entry)
        {
            SetRequiredTexture(material, entry, "BaseColor.png", "_BaseMap");
            SetRequiredTexture(material, entry, "Normal.png", "_BumpMap");
            SetRequiredTexture(material, entry, "AmbientOcclusion.png", "_OcclusionMap");
            SetRequiredTexture(material, entry, "MetallicSmoothness.png", "_MetallicGlossMap");
        }

        private static void SetRequiredTexture(Material material, Entry entry, string fileName, string propertyName)
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(entry.TextureDirectory + "/" + fileName);
            if (texture == null) throw new InvalidOperationException(entry.Id + " missing runtime texture: " + fileName);
            if (!material.HasProperty(propertyName)) throw new InvalidOperationException(entry.MaterialPath + " missing shader property: " + propertyName);
            material.SetTexture(propertyName, texture);
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

        [Serializable]
        private sealed class B08MotionContract
        {
            public B08ContractObject[] objects;
        }

        [Serializable]
        private sealed class B08ContractObject
        {
            public string role;
            public string stableId;
            public string name;
            public string parentStableId;
            public string jointType;
            public float[] localAxis;
            public float[] localPosition;
            public float[] localEulerDegrees;
            public float rangeMin;
            public float rangeMax;
        }

        private sealed class Entry
        {
            public readonly string Id;
            public readonly string ContractId;
            public readonly string ModelPath;
            public readonly string MaterialPath;
            public readonly string TextureDirectory;
            public readonly string ContractPath;
            public readonly string PrefabPath;

            public Entry(string id, string contractId, string modelPath, string materialPath, string textureDirectory, string contractPath, string prefabPath)
            {
                Id = id;
                ContractId = contractId;
                ModelPath = modelPath;
                MaterialPath = materialPath;
                TextureDirectory = textureDirectory;
                ContractPath = contractPath;
                PrefabPath = prefabPath;
            }
        }
    }
}
