using AutoEra.Art.PCG;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor
{
    [CustomEditor(typeof(PCGStreamSettings))]
    public sealed class PCGEnvironmentSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("正式环境：镜头大小 10～20。地貌水平尺度控制水平距离，地形起伏倍率控制外围高差。建设区内部起伏保持 0，以匹配现有平面导航和对象坐标；建设区过渡宽度与边缘自然度控制外侧恢复过程。",MessageType.Info);
            var settings=(PCGStreamSettings)target;
            serializedObject.Update();DrawPropertiesExcluding(serializedObject,"m_Script","LandscapeScale","ReliefScale");serializedObject.ApplyModifiedProperties();
            using(new EditorGUI.DisabledScope(UnityEngine.Application.isPlaying))
            {
                EditorGUI.BeginChangeCheck();float scale=EditorGUILayout.Slider("地貌水平尺度",settings.LandscapeScale,.15f,1);
                if(EditorGUI.EndChangeCheck())
                {Undo.RecordObject(settings,"调整地貌水平尺度");settings.SetLandscapeScale(scale);EditorUtility.SetDirty(settings);}
                EditorGUI.BeginChangeCheck();float relief=EditorGUILayout.Slider("地形起伏倍率",settings.ReliefScale,.35f,1.5f);
                if(EditorGUI.EndChangeCheck())
                {Undo.RecordObject(settings,"调整地形起伏倍率");settings.ReliefScale=relief;EditorUtility.SetDirty(settings);}
            }
            if(UnityEngine.Application.isPlaying)EditorGUILayout.HelpBox("运行时请在 Environment runtime 的 PCGStreamWorld Inspector 中应用地貌水平尺度或地形起伏倍率。",MessageType.Info);
        }
    }

    [CustomEditor(typeof(PCGStreamWorld))]
    public sealed class PCGEnvironmentWorldEditor : UnityEditor.Editor
    {
        private float _scale;
        private float _relief;
        private void OnEnable(){var world=(PCGStreamWorld)target;_scale=world.Settings!=null?world.Settings.LandscapeScale:.25f;_relief=world.Settings!=null?world.Settings.ReliefScale:.65f;}
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();if(!UnityEngine.Application.isPlaying)return;
            var world=(PCGStreamWorld)target;if(world.Field==null)return;
            _scale=EditorGUILayout.Slider("地貌水平尺度",_scale,.15f,1);
            if(GUILayout.Button("应用地貌水平尺度"))world.ApplyLandscapeScale(_scale);
            _relief=EditorGUILayout.Slider("地形起伏倍率",_relief,.35f,1.5f);
            if(GUILayout.Button("应用地形起伏倍率"))world.ApplyReliefScale(_relief);
            if(GUILayout.Button("保存当前地貌参数到配置")){world.SaveLandscapeScale();world.SaveReliefScale();}
            EditorGUILayout.LabelField("显示 / 缓存",world.DisplayCount+" / "+world.CacheCount);
        }
    }
}
