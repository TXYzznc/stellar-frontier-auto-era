using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AutoEra.Editor
{
    public static class ProductionTransportUiMigration
    {
        [MenuItem("Game Framework/AutoEra/UI/接入生产运输列表布局")]
        public static void Apply()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode.");
            Configure("Assets/Game/Prefabs/UI/Operations/WarehouseForm.prefab",false);
            Configure("Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab",true); AssetDatabase.SaveAssets();
        }
        private static bool Scoped(Transform value,bool field)
        { if(!field) return true; for(var p=value;p!=null;p=p.parent) if(p.name=="Panel_PageForest" || p.name=="Panel_PageMineral") return true; return false; }
        private static void Configure(string path,bool field)
        {
            var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach(var rect in root.GetComponentsInChildren<RectTransform>(true))
                {
                    if(!Scoped(rect,field)) continue;
                    if(rect.name.StartsWith("Content_",StringComparison.Ordinal))
                    { var group=rect.GetComponent<VerticalLayoutGroup>(); if(group!=null) { group.spacing=4; group.childControlWidth=group.childControlHeight=true; group.childForceExpandWidth=true; group.childForceExpandHeight=false; } }
                    if(rect.name.StartsWith("Txt_",StringComparison.Ordinal) && rect.name.EndsWith("Body",StringComparison.Ordinal))
                    { var layout=rect.GetComponent<LayoutElement>(); if(layout!=null) { layout.minHeight=layout.preferredHeight=field?24:36; layout.flexibleHeight=0; } var text=rect.GetComponent<TMP_Text>(); if(text!=null) text.fontSize=16; }
                    if(!rect.name.StartsWith("Item_",StringComparison.Ordinal) || !rect.name.EndsWith("Template",StringComparison.Ordinal)) continue;
                    rect.anchorMin=new Vector2(0,1); rect.anchorMax=Vector2.one; rect.pivot=new Vector2(.5f,1); rect.sizeDelta=new Vector2(0,44); rect.anchoredPosition=Vector2.zero;
                    var element=rect.GetComponent<LayoutElement>(); if(element==null) element=rect.gameObject.AddComponent<LayoutElement>(); element.minHeight=element.preferredHeight=44; element.preferredWidth=-1; element.flexibleWidth=1; element.flexibleHeight=0;
                    var button=rect.GetComponentInChildren<Button>(true); if(button!=null) { var b=(RectTransform)button.transform; b.anchorMin=Vector2.zero; b.anchorMax=Vector2.one; b.offsetMin=b.offsetMax=Vector2.zero; }
                    foreach(var text in rect.GetComponentsInChildren<TMP_Text>(true))
                    { bool label=text.name.EndsWith("RowLabel",StringComparison.Ordinal); var t=text.rectTransform; t.anchorMin=new Vector2(label?0:.42f,.5f); t.anchorMax=new Vector2(label?.42f:1,.5f); t.pivot=new Vector2(0,.5f); t.sizeDelta=new Vector2(-24,32); t.anchoredPosition=new Vector2(12,0); text.fontSize=16; text.alignment=TextAlignmentOptions.MidlineLeft; text.enableWordWrapping=true; text.overflowMode=TextOverflowModes.Ellipsis; }
                }
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
    }
}
