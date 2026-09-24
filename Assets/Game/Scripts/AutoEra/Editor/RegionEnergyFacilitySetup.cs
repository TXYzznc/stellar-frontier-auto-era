using System;
using AutoEra.Energy;
using AutoEra.World.Region;
using UnityEditor;
using UnityEngine;

namespace AutoEra.EditorTools
{
    /// <summary>
    /// 把初始基地的两台发电设施**声明成能源设施**（规格 06「第一版能源范围」：
    /// 「初始基地已经建有一台基础生物质发电机和一台低功率太阳能发电器」）。
    ///
    /// 为什么要在预制体上加组件、而不是在代码里按名字找对象：
    /// 场景里那批 `_objects` 只是建造期模板，`InitializeRuntime` 会把它们整批停用、
    /// 再按实体预制体实例化真正的对象。所以运行时真正存在的是**预制体实例**，
    /// 能力声明必须跟着预制体走；否则就会出现「场景里配了、运行时没有」这种最难查的偏差。
    ///
    /// 这个菜单是**幂等**的：已有的组件只更新参数，不会重复添加，也不会动别的字段。
    /// </summary>
    public static class RegionEnergyFacilitySetup
    {
        private const string GeneratorPrefab = "Assets/Game/Prefabs/Entity/InitialRegion/Generator.prefab";
        private const string SolarPrefab = "Assets/Game/Prefabs/Entity/InitialRegion/SolarArray.prefab";

        [MenuItem("Game Framework/AutoEra/Region/声明初始基地能源设施", priority = 2100)]
        public static void Apply()
        {
            int changed = 0;
            changed += Configure(GeneratorPrefab, RegionEnergyFacilityKind.FuelGenerator);
            changed += Configure(SolarPrefab, RegionEnergyFacilityKind.EnvironmentGenerator);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AutoEra][Energy] 初始基地能源设施已声明：生物质发电机（燃料，额定 "
                + FirstVersionEnergy.BiomassGeneratorRatedPower + " 功率，初始 "
                + FirstVersionEnergy.StartupBiomass + " 生物质）、太阳能阵列（环境，白天 "
                + FirstVersionEnergy.SolarDaylightPower + " 功率）。本次写入 " + changed + " 个预制体。");
        }

        /// <summary>只读检查：报告两个预制体当前是否已经声明过设施（不写任何东西）。</summary>
        [MenuItem("Game Framework/AutoEra/Region/检查初始基地能源设施声明", priority = 2101)]
        public static void Inspect()
        {
            Report(GeneratorPrefab);
            Report(SolarPrefab);
        }

        private static void Report(string path)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                RegionEnergyFacility facility = root.GetComponent<RegionEnergyFacility>();
                Debug.Log("[AutoEra][Energy] " + path + " → "
                    + (facility == null
                        ? "未声明能源设施"
                        : facility.Kind + "，额定 " + Serialized(facility, "_ratedPower")
                          + "，初始生物质 " + Serialized(facility, "_initialBiomass")));
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static string Serialized(RegionEnergyFacility facility, string field)
        {
            var data = new SerializedObject(facility);
            SerializedProperty property = data.FindProperty(field);
            return property == null ? "?" : property.floatValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 在预制体上声明（或更新）能源设施；返回 1 表示这一份被写过。
        ///
        /// 序列化字段只能经 `SerializedObject` 写：它们是 private 且没有公开 setter，
        /// 直接赋 C# 属性不会落盘（那样会得到「编译通过、场景里却是默认值」这种最难查的偏差）。
        /// </summary>
        private static int Configure(string path, RegionEnergyFacilityKind kind)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                RegionEnergyFacility facility = root.GetComponent<RegionEnergyFacility>();
                if (facility == null)
                {
                    facility = root.AddComponent<RegionEnergyFacility>();
                }

                var data = new SerializedObject(facility);
                data.FindProperty("_kind").enumValueIndex = (int)kind;
                if (kind == RegionEnergyFacilityKind.FuelGenerator)
                {
                    data.FindProperty("_ratedPower").floatValue = FirstVersionEnergy.BiomassGeneratorRatedPower;
                    data.FindProperty("_initialBiomass").floatValue = FirstVersionEnergy.StartupBiomass;
                }
                else if (kind == RegionEnergyFacilityKind.EnvironmentGenerator)
                {
                    data.FindProperty("_ratedPower").floatValue = FirstVersionEnergy.SolarDaylightPower;
                }
                else if (kind == RegionEnergyFacilityKind.Battery)
                {
                    data.FindProperty("_capacity").floatValue = FirstVersionEnergy.BatteryCapacity;
                    data.FindProperty("_initialCharge").floatValue = 0f;
                }

                data.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, path);
                return 1;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
