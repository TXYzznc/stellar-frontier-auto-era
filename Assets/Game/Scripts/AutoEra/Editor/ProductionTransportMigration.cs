using System;
using AutoEra.Logistics;
using AutoEra.World.Region;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor
{
    public static class ProductionTransportMigration
    {
        [MenuItem("Game Framework/AutoEra/Resources/检查机械臂装卸层级")]
        public static void InspectArm()
        {
            var root=PrefabUtility.LoadPrefabContents("Assets/Game/Prefabs/Entity/Machines/MultiJointArm.prefab");
            try
            {
                var report=new System.Text.StringBuilder(); var rig=root.GetComponentInChildren<AutoEra.Motion.MotionRig>();
                foreach(var joint in rig.JointBindings)
                { var path=joint.JointTransform.name; var parent=joint.JointTransform.parent; while(parent!=null && parent!=root.transform) { path=parent.name+"/"+path; parent=parent.parent; } report.AppendLine(joint.StableId+"="+path); }
                System.IO.File.WriteAllText("Temp/b45-arm-hierarchy.txt",report.ToString());
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        [MenuItem("Game Framework/AutoEra/Resources/接入生产运输装卸点")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode.");
            const string configPath = "Assets/Game/Config/ResourceProduction/ResourceTransfer.asset";
            var config = AssetDatabase.LoadAssetAtPath<ResourceTransferConfig>(configPath);
            if (config == null) { config=ScriptableObject.CreateInstance<ResourceTransferConfig>(); config.Read(); AssetDatabase.CreateAsset(config,configPath); }
            Configure("Assets/Game/Prefabs/Entity/InitialRegion/Forest.prefab",config,false);
            Configure("Assets/Game/Prefabs/Entity/InitialRegion/MineralVein.prefab",config,false);
            Configure("Assets/Game/Prefabs/Entity/InitialRegion/Warehouse.prefab",config,true);
            ConfigureArmYaw();
            AssetDatabase.SaveAssets(); Debug.Log("[AutoEra.Transport] Explicit load/unload contact and docking anchors configured.");
        }
        private static void ConfigureArmYaw()
        {
            const string path="Assets/Game/Prefabs/Entity/Machines/MultiJointArm.prefab"; var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var rig=root.GetComponentInChildren<AutoEra.Motion.MotionRig>();
                if(rig==null || !rig.TryGetBinding("base_yaw",out var yaw) || !rig.TryGetBinding("wrist_roll",out var tip)) throw new InvalidOperationException("Approved arm joints missing.");
                if(!tip.JointTransform.IsChildOf(yaw.JointTransform))
                { var visual=root.transform.Find("RigRoot/VisualRoot/VisualModel"); if(visual==null) throw new InvalidOperationException("Approved arm model missing."); visual.SetParent(yaw.JointTransform,true); }
                foreach(var joint in rig.JointBindings) if(joint.StableId!="wrist_roll" && !tip.JointTransform.IsChildOf(joint.JointTransform)) throw new InvalidOperationException("Joint does not control actual arm tip: "+joint.StableId);
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static void Configure(string path,ResourceTransferConfig config,bool warehouse)
        {
            var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var view=root.GetComponent<RegionObjectView>(); if(view==null) throw new InvalidOperationException("Missing region view.");
                Vector3 position;
                if (warehouse)
                { var serialized = new SerializedObject(view); Vector2 size = serialized.FindProperty("_footprint").vector2Value; position = new Vector3(0,.15f,-size.y*.5f-.3f); }
                else
                { var pile=root.transform.Find("GroundPileAnchor"); if(pile==null) throw new InvalidOperationException("Approved pile anchor missing."); position=pile.localPosition+Vector3.up*.15f; }
                var contact=Anchor(root,"TransferContact",position); var dock=Anchor(root,"TransferDock",position+new Vector3(0,-.15f,-2.4f));
                var endpoint=root.GetComponent<RegionTransferEndpoint>() ?? root.AddComponent<RegionTransferEndpoint>(); endpoint.ConfigureForEditor(config,contact,dock,warehouse);
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static Transform Anchor(GameObject root,string name,Vector3 position)
        { var value=root.transform.Find(name); if(value==null) { var go=new GameObject(name); value=go.transform; value.SetParent(root.transform,false); } value.localPosition=position; return value; }
    }
}
