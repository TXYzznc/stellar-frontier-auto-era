using System.Threading.Tasks;
using UnityEngine;

namespace AutoEra.Art.PCG
{
    public sealed partial class PCGStreamWorld
    {
        public int ConfigurationRevision {get;private set;}
        public float ClampCameraSize(float size)=>Mathf.Clamp(size,Settings.CameraMinSize,Settings.CameraMaxSize);
        private void CancelPreparation()
        {
            foreach(var p in _pending.Values)
            {
                p.Cancel.Cancel();
                p.Task.ContinueWith(t=>{if(t.IsFaulted){var observed=t.Exception;}p.Cancel.Dispose();},TaskScheduler.Default);
            }
            _pending.Clear();
        }
        public void ApplyLandscapeScale(float scale)
        {
            if(!_started||!Settings.MultiBiome)return;
            scale=Mathf.Clamp(scale,.15f,1);
            if(float.IsNaN(scale)||scale==Settings.LandscapeScale)return;
            Vector2 canonical=new Vector2((float)Field.LandscapeX(CameraFocus.x),(float)Field.LandscapeZ(CameraFocus.y));
            Settings.SetLandscapeScale(scale);RebuildAfterScaleChange(canonical);
        }
        public void ApplyReliefScale(float scale)
        {
            if(!_started||!Settings.MultiBiome)return;
            scale=Mathf.Clamp(scale,.35f,1.5f);
            if(float.IsNaN(scale)||Mathf.Approximately(scale,Settings.ReliefScale))return;
            Vector2 canonical=new Vector2((float)Field.LandscapeX(CameraFocus.x),(float)Field.LandscapeZ(CameraFocus.y));
            Settings.ReliefScale=scale;RebuildAfterScaleChange(canonical);
        }
        private void RebuildAfterScaleChange(Vector2 canonical)
        {
            CancelPreparation();_building=null;
            foreach(var view in _views.Values)view.Dispose();foreach(var view in _pool)view.Dispose();
            _views.Clear();_pool.Clear();_cache.Clear();_remove.Clear();_candidates.Clear();
            Field=new PCGWorldField(Settings);ConfigurationRevision++;
            CameraFocus=Field.WorldPoint(canonical);CameraSize=ClampCameraSize(CameraSize);
            _hasRequest=false;FocusPending=false;InitialReady=false;_nextPlan=0;
            var grass=GetComponent<PCGGrassTrails>();if(grass!=null)grass.ClearTrails();
            if(SurfaceResponse!=null)SurfaceResponse.ClearHistory();
            UpdateCamera();RequestView(CameraFocus,CameraSize,true);
        }
#if UNITY_EDITOR
        public void SaveLandscapeScale()
        {
            if(_sourceSettings==null)return;
            UnityEditor.Undo.RecordObject(_sourceSettings,"保存地貌区域尺度");
            _sourceSettings.SetLandscapeScale(Settings.LandscapeScale);
            UnityEditor.EditorUtility.SetDirty(_sourceSettings);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(_sourceSettings);
        }
        public void SaveReliefScale()
        {
            if(_sourceSettings==null)return;
            UnityEditor.Undo.RecordObject(_sourceSettings,"保存地形起伏倍率");
            _sourceSettings.ReliefScale=Settings.ReliefScale;
            UnityEditor.EditorUtility.SetDirty(_sourceSettings);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(_sourceSettings);
        }
#endif
    }
}
