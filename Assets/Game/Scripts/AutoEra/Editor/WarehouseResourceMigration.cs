using System;
using AutoEra.World.Region;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor
{
    public static class WarehouseResourceMigration
    {
        // Frozen B43 input: BuildingDefinitions row 50011, Prefab=InitialRegion/Warehouse, StorageCapacity=120.
        private const string Path = "Assets/Game/Prefabs/Entity/InitialRegion/Warehouse.prefab";
        [MenuItem("Game Framework/AutoEra/Resources/接入初始仓库权威库存")]
        public static void Apply()
        {
            var root = PrefabUtility.LoadPrefabContents(Path);
            try
            {
                if (root.GetComponent<RegionObjectView>() == null) throw new InvalidOperationException("Warehouse has no region view.");
                var facility = root.GetComponent<RegionWarehouseFacility>() ?? root.AddComponent<RegionWarehouseFacility>();
                facility.ConfigureForEditor(50011); PrefabUtility.SaveAsPrefabAsset(root, Path);
                Debug.Log("[AutoEra.Resources] Warehouse definition binding applied.");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
    }
}
