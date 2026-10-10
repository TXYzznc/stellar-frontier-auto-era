using System;
using AutoEra.ResourcePoints;
using AutoEra.Motion;
using AutoEra.World.Region;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor
{
    public static class ForestMineralProductionMigration
    {
        private const string ConfigPath = "Assets/Game/Config/ResourceProduction/ForestMineralProduction.asset";
        private const string TreePath = "Assets/Game/Prefabs/植株/树/ForestTree01_Optimized.prefab";
        [MenuItem("Game Framework/AutoEra/Resources/检查生产树网格几何")]
        public static void InspectGeometry()
        {
            var root = PrefabUtility.LoadPrefabContents("Assets/Game/Prefabs/Entity/InitialRegion/Forest.prefab");
            try
            {
                var slot = root.transform.Find("ProductionTrees/TreeSlot_0");
                foreach (var filter in slot.GetComponentsInChildren<MeshFilter>(true))
                {
                    float radius = 0;
                    foreach (var vertex in filter.sharedMesh.vertices)
                    { var local = slot.InverseTransformPoint(filter.transform.TransformPoint(vertex)); if (local.y <= .25f) radius = Mathf.Max(radius, new Vector2(local.x, local.z).magnitude); }
                    Debug.Log("[AutoEra.ProductionGeometry] " + filter.name + " active=" + filter.gameObject.activeInHierarchy + " mesh=" + filter.sharedMesh.name + " raw=" + filter.sharedMesh.bounds + " world=" + filter.GetComponent<Renderer>()?.bounds + " trunkRadius=" + radius);
                }
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        [MenuItem("Game Framework/AutoEra/Resources/接入林木矿脉生产配置")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode.");
            if (!AssetDatabase.IsValidFolder("Assets/Game/Config/ResourceProduction")) AssetDatabase.CreateFolder("Assets/Game/Config", "ResourceProduction");
            var config = AssetDatabase.LoadAssetAtPath<ForestMineralProductionConfig>(ConfigPath);
            if (config == null) { config = ScriptableObject.CreateInstance<ForestMineralProductionConfig>(); config.Read(); AssetDatabase.CreateAsset(config, ConfigPath); }
            var treeAsset = AssetDatabase.LoadAssetAtPath<GameObject>(TreePath);
            if (treeAsset == null) throw new InvalidOperationException("Approved tree prefab missing.");
            foreach (var filter in treeAsset.GetComponentsInChildren<MeshFilter>(true))
            {
                var sourcePath = AssetDatabase.GetAssetPath(filter.sharedMesh);
                if (AssetImporter.GetAtPath(sourcePath) is ModelImporter model && !model.isReadable)
                { model.isReadable = true; model.SaveAndReimport(); Debug.Log("[AutoEra.Production] Runtime fracture mesh readable: " + sourcePath); }
            }
            ConfigureForest(config, treeAsset); ConfigureMineral(config);
            ConfigureMounts("Assets/Game/Prefabs/Entity/Machines/WheeledCarrier.prefab", 2, 1.8f, 2.6f);
            ConfigureMounts("Assets/Game/Prefabs/Entity/Machines/FixedRotaryCarrier.prefab", 1, 2, 2);
            ConfigureToolYaw("Assets/Game/Prefabs/Entity/Machines/RotarySaw.prefab");
            ConfigureToolYaw("Assets/Game/Prefabs/Entity/Machines/RotaryDrill.prefab");
            AssetDatabase.SaveAssets();
            Debug.Log("[AutoEra.Production] Approved trial configuration, tree references and actual tool mounts bound; mineral visuals remain existing greybox content.");
        }
        private static void ConfigureToolYaw(string path)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var rig = root.GetComponent<MotionRig>();
                if (rig == null || !rig.TryGetBinding("mount_yaw", out var yaw)) throw new InvalidOperationException("Approved yaw binding missing: " + path);
                var visual = root.transform.Find("RigRoot/VisualRoot/VisualModel") ?? yaw.JointTransform.Find("VisualModel");
                if (visual == null) throw new InvalidOperationException("Approved visual assembly missing: " + path);
                // The legacy marker was a sibling of the model. A joint must own the geometry
                // it rotates; preserve the approved bind pose, meshes, ranges and stable IDs.
                if (visual.parent != yaw.JointTransform) visual.SetParent(yaw.JointTransform, true);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static void ConfigureForest(ForestMineralProductionConfig config, GameObject treeAsset)
        {
            const string path = "Assets/Game/Prefabs/Entity/InitialRegion/Forest.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                Transform old = root.transform.Find("Visual/ProxyDetails"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
                Transform band = root.transform.Find("ProductionTrees");
                if (band == null) { var go = new GameObject("ProductionTrees"); band = go.transform; band.SetParent(root.transform, false); }
                var trees = new Transform[config.Read().TreeCount(0)];
                // Right-angle open band: a three-tree main stem and branching arms; bottom row leaves a two-cell entrance.
                Vector2[] positions = { new Vector2(-3,-3), new Vector2(-3,-1), new Vector2(-3,1), new Vector2(-3,3),
                    new Vector2(-1,3), new Vector2(1,3), new Vector2(3,3), new Vector2(1,1) };
                for (int i = 0; i < trees.Length; i++)
                {
                    var slot = band.Find("TreeSlot_" + i);
                    if (slot == null) { var go = new GameObject("TreeSlot_" + i); slot = go.transform; slot.SetParent(band, false); }
                    slot.localPosition = new Vector3(positions[i].x, 0, positions[i].y);
                    Transform visual = slot.Find("ApprovedTree");
                    if (visual == null)
                    {
                        var instance = (GameObject)PrefabUtility.InstantiatePrefab(treeAsset); instance.name = "ApprovedTree"; instance.transform.SetParent(slot, false); visual = instance.transform;
                        Bounds bounds = RendererBounds(instance); if (bounds.size.y <= 0) throw new InvalidOperationException("Approved tree has no valid geometry.");
                        float scale = (float)config.Read().MatureHeight / bounds.size.y; visual.localScale *= scale;
                        bounds = RendererBounds(instance); visual.localPosition -= Vector3.up * (bounds.min.y - slot.position.y);
                        foreach (var collider in instance.GetComponentsInChildren<Collider>(true)) UnityEngine.Object.DestroyImmediate(collider);
                    }
                    // Asset authoring roots retain their source-scene coordinates. Content slots
                    // own placement, so normalize every instance explicitly, including prior migrations.
                    visual.localPosition = Vector3.zero;
                    Bounds normalized = RendererBounds(visual.gameObject);
                    visual.localScale *= (float)config.Read().MatureHeight / normalized.size.y;
                    normalized = RendererBounds(visual.gameObject);
                    visual.localPosition -= Vector3.up * (normalized.min.y - slot.position.y);
                    var presentation = slot.GetComponent<ProductionTreePresentation>() ?? slot.gameObject.AddComponent<ProductionTreePresentation>();
                    float radius = 0;
                    foreach (var renderer in GeometryRenderers(visual.gameObject))
                    {
                        var filter = renderer.GetComponent<MeshFilter>();
                        if (filter == null) continue;
                        foreach (var vertex in filter.sharedMesh.vertices)
                        {
                            var local = slot.InverseTransformPoint(filter.transform.TransformPoint(vertex));
                            if (local.y <= .25f) radius = Mathf.Max(radius, new Vector2(local.x, local.z).magnitude);
                        }
                    }
                    if (radius <= 0) throw new InvalidOperationException("Approved tree lacks measurable trunk geometry.");
                    presentation.ConfigureForEditor(visual, (float)config.Read().MatureHeight, radius); trees[i] = slot;
                }
                var facility = root.GetComponent<RegionProductionFacility>() ?? root.AddComponent<RegionProductionFacility>();
                facility.ConfigureForEditor(ProductionFacilityKind.Forest, 0, config, trees, null, PileAnchor(root, new Vector3(4,0,-3)));
                var data = new SerializedObject(root.GetComponent<RegionObjectView>()); data.FindProperty("_blocksNavigation").boolValue = false; data.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static void ConfigureMineral(ForestMineralProductionConfig config)
        {
            const string path = "Assets/Game/Prefabs/Entity/InitialRegion/MineralVein.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                Transform markers = root.transform.Find("Visual/ProxyDetails");
                if (markers == null || markers.childCount < 6) throw new InvalidOperationException("Existing mineral greybox markers missing.");
                var rocks = new Transform[6];
                for (int i = 0; i < markers.childCount; i++) { markers.GetChild(i).gameObject.SetActive(i < 6); if (i < 6) rocks[i] = markers.GetChild(i); }
                var facility = root.GetComponent<RegionProductionFacility>() ?? root.AddComponent<RegionProductionFacility>();
                facility.ConfigureForEditor(ProductionFacilityKind.Mineral, 0, config, null, rocks, PileAnchor(root, new Vector3(5,0,-3)));
                var data = new SerializedObject(root.GetComponent<RegionObjectView>()); data.FindProperty("_blocksNavigation").boolValue = false; data.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static Transform PileAnchor(GameObject root, Vector3 position)
        { var anchor = root.transform.Find("GroundPileAnchor"); if (anchor == null) { var go = new GameObject("GroundPileAnchor"); anchor = go.transform; anchor.SetParent(root.transform, false); } anchor.localPosition = position; return anchor; }
        private static void ConfigureMounts(string path, int count, float width, float length)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var mounts = new Transform[count];
                for (int i = 0; i < count; i++)
                {
                    string name = "ProductionToolMount_" + i; var anchor = root.transform.Find(name);
                    if (anchor == null) { var go = new GameObject(name); anchor = go.transform; anchor.SetParent(root.transform, false); }
                    // Installation geometry is explicit authoring data, derived from the confirmed carrier footprint.
                    anchor.localPosition = new Vector3(count == 1 ? 0 : (i == 0 ? -1 : 1) * width * .25f, .25f, length * .5f);
                    mounts[i] = anchor;
                }
                var binding = root.GetComponent<ProductionToolMounts>() ?? root.AddComponent<ProductionToolMounts>(); binding.ConfigureForEditor(mounts);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static Bounds RendererBounds(GameObject root)
        {
            Renderer[] renderers = GeometryRenderers(root);
            if (renderers.Length == 0) throw new InvalidOperationException("Asset has no renderer.");
            Bounds bounds = renderers[0].bounds; for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds); return bounds;
        }
        private static Renderer[] GeometryRenderers(GameObject root)
        {
            var group = root.GetComponentInChildren<LODGroup>(true);
            if (group != null && group.GetLODs().Length > 0) return group.GetLODs()[0].renderers;
            return root.GetComponentsInChildren<Renderer>(true);
        }
    }
}
