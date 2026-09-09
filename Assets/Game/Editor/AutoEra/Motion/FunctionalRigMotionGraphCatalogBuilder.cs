using System;
using AutoEra.Motion;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor.Motion
{
    /// <summary>Regenerates named preview actions from the contract families. These assets are preview/runtime candidates, never task instructions.</summary>
    internal static class FunctionalRigMotionGraphCatalogBuilder
    {
        private const string Folder = "Assets/Game/MotionGraphs/Entity";

        [MenuItem("AutoEra/Motion/Build Formal Entity Action Catalog")]
        private static void BuildCatalog()
        {
            EnsureFolder("Assets/Game/MotionGraphs");
            EnsureFolder(Folder);
            RemoveLegacyGraph("effector_lock");
            Build("轮式载体_常规行驶", "carrier_drive", "wheeled_carrier_prototype", Nodes(
                Rotate("front_left_roll", MotionNodeKind.ContinuousRotate), Rotate("front_right_roll", MotionNodeKind.ContinuousRotate),
                Rotate("rear_left_roll", MotionNodeKind.ContinuousRotate), Rotate("rear_right_roll", MotionNodeKind.ContinuousRotate)));
            Build("轮式载体_低速蟹行", "carrier_crab", "wheeled_carrier_prototype", Nodes(
                Rotate("front_left_steer", MotionNodeKind.Rotate), Rotate("front_right_steer", MotionNodeKind.Rotate),
                Rotate("rear_left_steer", MotionNodeKind.Rotate), Rotate("rear_right_steer", MotionNodeKind.Rotate),
                Rotate("front_left_roll", MotionNodeKind.ContinuousRotate), Rotate("front_right_roll", MotionNodeKind.ContinuousRotate),
                Rotate("rear_left_roll", MotionNodeKind.ContinuousRotate), Rotate("rear_right_roll", MotionNodeKind.ContinuousRotate)));
            Build("四轮机构_转向悬挂滚动", "wheel_module_steer_roll", "four_wheel_module_prototype", Nodes(
                Rotate("steer", MotionNodeKind.Rotate), Rotate("suspension", MotionNodeKind.Translate), Rotate("roll", MotionNodeKind.ContinuousRotate)));
            Build("机械臂_工作空间扫描", "arm_work_envelope", "multi_joint_arm_prototype", Nodes(
                Rotate("base_yaw", MotionNodeKind.ContinuousRotate), Rotate("shoulder_pitch", MotionNodeKind.Rotate), Rotate("elbow_pitch", MotionNodeKind.Rotate),
                Rotate("wrist_pitch", MotionNodeKind.Rotate), Rotate("wrist_roll", MotionNodeKind.Rotate)));
            Build("水枪_瞄准喷射", "water_aim_spray", "water_sprayer_prototype", Nodes(
                Rotate("mount_yaw", MotionNodeKind.Rotate), Rotate("nozzle_pitch", MotionNodeKind.Rotate), Rotate("flow_valve", MotionNodeKind.Rotate)));
            Build("锯盘_切割循环", "saw_cut_cycle", "rotary_saw_prototype", Nodes(
                Rotate("mount_yaw", MotionNodeKind.Rotate), Rotate("lift_rail", MotionNodeKind.Translate), Rotate("feed_slide", MotionNodeKind.Translate), Rotate("saw_spindle", MotionNodeKind.ContinuousRotate)));
            Build("钻头_钻探循环", "drill_mine_cycle", "rotary_drill_prototype", Nodes(
                Rotate("mount_yaw", MotionNodeKind.Rotate), Rotate("press_slide", MotionNodeKind.Translate), Rotate("drill_rotor", MotionNodeKind.ContinuousRotate)));
            Build("滑动门_开启", "sliding_door_open", "sliding_door_prototype", Nodes(
                Rotate("left_leaf", MotionNodeKind.OpenClose), Rotate("right_leaf", MotionNodeKind.OpenClose)));
            Build("传送带_运行", "conveyor_run", "conveyor_prototype", Nodes(
                Rotate("drive_roller", MotionNodeKind.ContinuousRotate), Rotate("tail_roller", MotionNodeKind.ContinuousRotate),
                Rotate("upper_idler_front", MotionNodeKind.ContinuousRotate), Rotate("upper_idler_rear", MotionNodeKind.ContinuousRotate),
                Rotate("lower_idler_front", MotionNodeKind.ContinuousRotate), Rotate("lower_idler_rear", MotionNodeKind.ContinuousRotate)));
            Build("固定旋转载体_巡扫", "fixed_rotary_scan", "fixed_rotary_carrier_prototype", Nodes(
                Rotate("yaw_pivot", MotionNodeKind.ContinuousRotate)));
            Build("货舱_传输开门", "cargo_transfer_open", "cargo_bay_prototype", Nodes(
                Rotate("left_door", MotionNodeKind.OpenClose), Rotate("right_door", MotionNodeKind.OpenClose), Rotate("transfer_tray", MotionNodeKind.Translate)));
            AssetDatabase.SaveAssets();
        }

        private static void Build(string displayName, string graphId, string targetContractId, MotionNodeDefinition[] nodes)
        {
            string path = Folder + "/" + graphId + ".asset";
            MotionGraphAsset graph = AssetDatabase.LoadAssetAtPath<MotionGraphAsset>(path);
            if (graph == null)
            {
                graph = ScriptableObject.CreateInstance<MotionGraphAsset>();
                AssetDatabase.CreateAsset(graph, path);
            }

            graph.name = displayName;
            graph.Configure(1, graphId, "1.0.0", Array.Empty<MotionParameterDefinition>(), nodes, Array.Empty<MotionConnectionDefinition>(), targetContractId);
            EditorUtility.SetDirty(graph);
        }

        private static MotionNodeDefinition[] Nodes(params MotionNodeDefinition[] nodes) => nodes;

        private static MotionNodeDefinition Rotate(string jointId, MotionNodeKind kind)
        {
            return new MotionNodeDefinition("preview_" + jointId, kind, jointId, string.Empty);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
        }

        private static void RemoveLegacyGraph(string graphId)
        {
            string path = Folder + "/" + graphId + ".asset";
            if (AssetDatabase.LoadAssetAtPath<MotionGraphAsset>(path) != null) AssetDatabase.DeleteAsset(path);
        }
    }
}
