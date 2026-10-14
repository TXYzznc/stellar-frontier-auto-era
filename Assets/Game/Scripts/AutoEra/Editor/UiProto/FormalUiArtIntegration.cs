using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoEra.UI;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AutoEra.Editor.UiProto
{
    /// <summary>Source-preserving formal UI migration and actual Unity rendering evidence.</summary>
    public static class FormalUiArtIntegration
    {
        public const string Change = "openspec/changes/b54-formal-ui-art-integration";
        private const string Evidence = Change + "/evidence";
        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
        private static Color Surface => new Color32(27,38,51,255);
        private static Color Card => new Color32(43,55,69,255);
        private static JArray Read(string name) => JArray.Parse(File.ReadAllText(Change + "/art/" + name + ".json"));
        private static Vector2 V(JToken n) => new Vector2((float)n[0],(float)n[1]);
        private static Vector4 R(JToken n) => new Vector4((float)n[0],(float)n[1],(float)n[2],(float)n[3]);

        public static IEnumerator ImportAll()
        {
            RequireEditMode();
            var plan = Read("import-plan");
            Directory.CreateDirectory(Evidence);
            File.WriteAllText(Evidence+"/import-progress.json",new JObject {["imported"]=0,["total"]=plan.Count,["complete"]=false}.ToString());
            int index=0;
            foreach (JObject entry in plan)
            {
                string path=(string)entry["path"];
                var importer=AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer==null) throw new InvalidOperationException("缺少 TextureImporter: "+path);
                if(ImportMatches(entry,importer)) {index++;if(index%12==0){File.WriteAllText(Evidence+"/import-progress.json",new JObject {["imported"]=index,["total"]=plan.Count,["complete"]=false}.ToString());yield return null;}continue;}
                Vector4 rect=R(entry["rect"]),border=R(entry["border"]);
                for(int attempt=0;attempt<6;attempt++)
                {
                importer=AssetImporter.GetAtPath(path) as TextureImporter;
                importer.textureType=TextureImporterType.Sprite;
                importer.spritePixelsPerUnit=(float)entry["ppu"];
                importer.alphaIsTransparency=true; importer.mipmapEnabled=false;
                importer.sRGBTexture=true; importer.isReadable=false;
                importer.filterMode=FilterMode.Bilinear;
                importer.wrapMode=(bool)entry["tile"]?TextureWrapMode.Repeat:TextureWrapMode.Clamp;
                importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.npotScale=TextureImporterNPOTScale.None;
                int max=Mathf.NextPowerOfTwo(Mathf.Max((int)entry["sourceSize"][0],(int)entry["sourceSize"][1]));
                importer.maxTextureSize=Mathf.Clamp(max,32,8192);
                var settings=new TextureImporterSettings(); importer.ReadTextureSettings(settings);
                settings.spriteMeshType=SpriteMeshType.FullRect; settings.spriteExtrude=1;
                importer.SetTextureSettings(settings);
                if ((bool)entry["cropped"])
                {
                    importer.spriteImportMode=SpriteImportMode.Multiple;
                    SetSpriteRect(importer,path,rect,border);
                }
                else
                {
                    importer.spriteImportMode=SpriteImportMode.Single;
                    importer.spriteBorder=border;
                    importer.spritePivot=new Vector2(.5f,.5f);
                }
                foreach (string platform in new [] {"Standalone","Android","iPhone","WebGL"})
                    importer.ClearPlatformTextureSettings(platform);
                    importer.SaveAndReimport();
                    yield return null;
                    if(ImportMatches(entry,importer))break;
                    if(attempt==5)throw new InvalidOperationException("导入读回不一致: "+path);
                    yield return null;
                }
                index++;
                if (index%12==0)
                {
                    File.WriteAllText(Evidence+"/import-progress.json",new JObject {["imported"]=index,["total"]=plan.Count}.ToString());
                    yield return null;
                }
            }
            Sprites.Clear();
            File.WriteAllText(Evidence+"/import-progress.json",new JObject {["imported"]=index,["total"]=plan.Count,["complete"]=true}.ToString());
            Debug.Log("[B54] 正式 UI 导入完成: "+index);
        }
        private static bool ImportMatches(JObject entry,TextureImporter importer)
        {
            var sprite=AssetDatabase.LoadAllAssetsAtPath((string)entry["path"]).OfType<Sprite>().FirstOrDefault();
            if(sprite==null||importer.textureType!=TextureImporterType.Sprite||importer.mipmapEnabled||!importer.alphaIsTransparency||importer.isReadable)return false;
            Vector4 rect=R(entry["rect"]),border=R(entry["border"]);
            return Mathf.Abs(sprite.rect.width-rect.z)<1&&Mathf.Abs(sprite.rect.height-rect.w)<1&&Vector4.Distance(sprite.border,border)<.1f&&Mathf.Abs(sprite.pixelsPerUnit-(float)entry["ppu"])<.1f;
        }

        // The existing AutoEra.Editor assembly intentionally does not depend on the optional
        // Unity.2D.Sprite.Editor assembly. Invoke its installed public data-provider API here
        // rather than changing assembly dependencies or using the obsolete spritesheet setter.
        private static void SetSpriteRect(TextureImporter importer,string path,Vector4 rect,Vector4 border)
        {
            var assembly=AppDomain.CurrentDomain.GetAssemblies().First(a=>a.GetName().Name=="Unity.2D.Sprite.Editor");
            var types=assembly.GetTypes();
            Type factoryType=types.First(t=>t.Name=="SpriteDataProviderFactories");
            object factory=Activator.CreateInstance(factoryType);factoryType.GetMethod("Init").Invoke(factory,null);
            object provider=factoryType.GetMethod("GetSpriteEditorDataProviderFromObject").Invoke(factory,new object[]{importer});
            Type api=types.First(t=>t.Name=="ISpriteEditorDataProvider");
            api.GetMethod("InitSpriteEditorDataProvider").Invoke(provider,null);
            var old=(Array)api.GetMethod("GetSpriteRects").Invoke(provider,null);
            Type rectType=types.First(t=>t.Name=="SpriteRect");object item=Activator.CreateInstance(rectType);
            string name=Path.GetFileNameWithoutExtension(path);
            object id=old.Length>0?rectType.GetProperty("spriteID").GetValue(old.GetValue(0)):GUID.Generate();
            rectType.GetProperty("name").SetValue(item,name);rectType.GetProperty("rect").SetValue(item,new Rect(rect.x,rect.y,rect.z,rect.w));
            rectType.GetProperty("alignment").SetValue(item,SpriteAlignment.Center);rectType.GetProperty("pivot").SetValue(item,new Vector2(.5f,.5f));
            rectType.GetProperty("border").SetValue(item,border);rectType.GetProperty("spriteID").SetValue(item,id);
            Array rects=Array.CreateInstance(rectType,1);rects.SetValue(item,0);api.GetMethod("SetSpriteRects").Invoke(provider,new object[]{rects});
            Type namesType=types.First(t=>t.Name=="ISpriteNameFileIdDataProvider");
            object names=api.GetMethod("GetDataProvider").MakeGenericMethod(namesType).Invoke(provider,null);
            Type pairType=types.First(t=>t.Name=="SpriteNameFileIdPair");Array pairs=Array.CreateInstance(pairType,1);
            pairs.SetValue(Activator.CreateInstance(pairType,new object[]{name,id}),0);
            namesType.GetMethod("SetNameFileIdPairs").Invoke(names,new object[]{pairs});
            api.GetMethod("Apply").Invoke(provider,null);
        }

        private static Sprite Get(string key)
        {
            if (Sprites.TryGetValue(key,out var value)) return value;
            JObject entry=Read("import-plan").OfType<JObject>().FirstOrDefault(e=>(string)e["key"]==key);
            if (entry==null) throw new InvalidOperationException("素材未登记: "+key);
            value=AssetDatabase.LoadAssetAtPath<Sprite>((string)entry["path"]);
            if (value==null) value=AssetDatabase.LoadAllAssetsAtPath((string)entry["path"]).OfType<Sprite>().FirstOrDefault();
            if (value==null) throw new InvalidOperationException("Sprite 未导入: "+key);
            Sprites[key]=value;
            return value;
        }

        public static IEnumerator ApplyFamily(int family)
        {
            RequireEditMode();
            var entries=Read("page-plan").OfType<JObject>().Where(e=>(int)e["family"]==family).ToArray();
            foreach (var group in entries.GroupBy(e=>(string)e["prefab"]))
            {
                var root=PrefabUtility.LoadPrefabContents(group.Key);
                try
                {
                    Style(root);
                    if (family==3) ApplyHud(root);
                    else foreach(var page in group) ApplyPage(root,page);
                    PrefabUtility.SaveAsPrefabAsset(root,group.Key);
                    SyncContract(root,(string)group.First()["contract"]);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
                yield return null;
            }
            if (family==13) StyleItems();
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory(Evidence);
            File.WriteAllText(Evidence+"/family-"+family.ToString("00")+".json",new JObject {["family"]=family,["pages"]=entries.Length,["complete"]=true}.ToString());
            Debug.Log("[B54] 页面族 "+family+" 接入 "+entries.Length+" 页");
        }

        private static Transform Find(GameObject root,string name) => root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name==name);
        private static void Stretch(RectTransform r)
        {
            r.anchorMin=Vector2.zero; r.anchorMax=Vector2.one; r.pivot=new Vector2(.5f,.5f);
            r.anchoredPosition=Vector2.zero; r.sizeDelta=Vector2.zero;
        }
        private static void Put(RectTransform r,Vector4 v)
        {
            r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);
            r.anchoredPosition=new Vector2(v.x,-v.y);r.sizeDelta=new Vector2(v.z,v.w);
        }
        private static Image Image(Transform parent,string name,Vector4 rect,Sprite sprite,Color color,bool sliced=false)
        {
            var t=parent.Find(name) as RectTransform;
            if (t==null) {var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));t=(RectTransform)go.transform;t.SetParent(parent,false);}
            Put(t,rect);
            var image=t.GetComponent<Image>();image.sprite=sprite;image.color=color;image.raycastTarget=false;
            image.type=sliced?UnityEngine.UI.Image.Type.Sliced:UnityEngine.UI.Image.Type.Simple;
            image.preserveAspect=!sliced && sprite!=null;image.pixelsPerUnitMultiplier=1;
            return image;
        }
        private static void Skin(Image image,string key)
        {
            image.sprite=Get(key);image.color=Color.white;image.type=image.sprite.border==Vector4.zero?UnityEngine.UI.Image.Type.Simple:UnityEngine.UI.Image.Type.Sliced;
            image.preserveAspect=image.type==UnityEngine.UI.Image.Type.Simple;
            image.pixelsPerUnitMultiplier=1;
        }
        private static void Style(GameObject root)
        {
            foreach (var image in root.GetComponentsInChildren<Image>(true))
            {
                string n=image.name;
                if(n.StartsWith("Img_Formal")||n.StartsWith("Bg_Formal"))continue;
                if(image.GetComponent<TMP_InputField>()!=null){Skin(image,"M008-input-normal");continue;}
                if(n.StartsWith("Panel_Page")||n=="Deco_Frame"||n=="Panel_Frame") {image.sprite=null;image.color=Color.clear;image.raycastTarget=false;continue;}
                if(n.StartsWith("Panel_") && n.EndsWith("Message"))
                {
                    string state=n.Contains("Loading")?"loading":n.Contains("Error")?"error":n.Contains("Success")?"success":n.Contains("Disabled")?"disabled":"empty";
                    Skin(image,"M006-"+state);continue;
                }
                if(n.StartsWith("Panel_")) {Skin(image,"M011-card-normal");image.raycastTarget=false;continue;}
                if(n.StartsWith("Item_") && n.EndsWith("Template")) {Skin(image,"M005-row-normal");continue;}
                if(n.StartsWith("Bar_") || n.Contains("Track"))
                {
                    bool filled=image.type==UnityEngine.UI.Image.Type.Filled||n.Contains("Fill");
                    Skin(image,filled?"M004-progress":"M004-track");
                    if(filled){image.type=UnityEngine.UI.Image.Type.Filled;image.preserveAspect=false;}
                    continue;
                }
                if(n.StartsWith("Btn_"))continue;
                if(n.StartsWith("Input_")||n.StartsWith("Bg_Input")&&n!="Bg_InputBlocker")Skin(image,"M008-input-normal");
            }
            foreach(var button in root.GetComponentsInChildren<Button>(true))
            {
                var image=button.GetComponent<Image>();if(image==null)continue;
                bool row=button.name.EndsWith("Row"),nav=button.name.Contains("Nav")||button.name.Contains("Tab")||button.transform.parent!=null&&button.transform.parent.name=="Grp_Navigation";
                string normal=row?"M005-row-normal":nav?"M009-nav-item":"M002-normal";
                Skin(image,normal);image.raycastTarget=true;
                button.targetGraphic=image;button.transition=Selectable.Transition.SpriteSwap;
                button.spriteState=new SpriteState {highlightedSprite=Get(row?"M005-row-hover":nav?"M009-tab-hover":"M002-hover"),pressedSprite=Get(row?"M005-row-selected":nav?"M009-tab-pressed":"M002-pressed"),selectedSprite=Get(row?"M005-row-selected":nav?"M009-tab-ear-selected":"M002-selected"),disabledSprite=Get(row?"M005-row-disabled":nav?"M009-tab-disabled":"M002-disabled")};
                if(!row) foreach(var label in button.GetComponentsInChildren<TMP_Text>(true))
                {
                    label.alignment=TextAlignmentOptions.Center;label.fontSize=20;label.enableWordWrapping=false;
                    label.enableAutoSizing=true;label.fontSizeMin=14;label.fontSizeMax=20;
                    Stretch(label.rectTransform);label.rectTransform.sizeDelta=new Vector2(-24,-6);
                }
            }
            foreach(var text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                text.raycastTarget=false;
                string value=text.text;
                if(value.StartsWith("Loading：")||value.StartsWith("Loading:"))text.text="正在加载";
                else if(value.StartsWith("Empty：")||value.StartsWith("Empty:"))text.text="暂无内容";
                else if(value.StartsWith("Error：")||value.StartsWith("Error:"))text.text="发生错误";
                else if(value.StartsWith("Success：")||value.StartsWith("Success:"))text.text="操作成功";
                else if(value.StartsWith("Disabled：")||value.StartsWith("Disabled:"))text.text="当前不可用";
                if(text.name.EndsWith("Title"))text.fontSize=28;
                else if(text.name.EndsWith("Heading"))text.fontSize=21;
                else if(text.fontSize<18)text.fontSize=18;
            }
            foreach(var slider in root.GetComponentsInChildren<Slider>(true))
            {
                var track=slider.GetComponent<Image>();if(track!=null)Skin(track,"M007-track-thin");
                if(slider.fillRect!=null){var fill=slider.fillRect.GetComponent<Image>();if(fill!=null){Skin(fill,"M007-fill-thin");fill.preserveAspect=false;}}
                if(slider.handleRect!=null){var handle=slider.handleRect.GetComponent<Image>();if(handle!=null)Skin(handle,"M007-knob-normal");}
            }
            foreach(var input in root.GetComponentsInChildren<TMP_InputField>(true))
            {
                var image=input.GetComponent<Image>();if(image==null)continue;Skin(image,"M008-input-normal");image.raycastTarget=true;
                input.targetGraphic=image;input.transition=Selectable.Transition.SpriteSwap;
                input.spriteState=new SpriteState {highlightedSprite=Get("M008-input-focus"),pressedSprite=Get("M008-input-focus"),selectedSprite=Get("M008-input-focus"),disabledSprite=Get("M008-input-disabled")};
            }
            foreach(var toggle in root.GetComponentsInChildren<Toggle>(true))
            {
                if(toggle.targetGraphic is Image background)Skin(background,"M008-check-off");
                if(toggle.graphic is Image check)Skin(check,"M008-check-on");
            }
        }

        private static GridLayoutGroup Grid(RectTransform target)
        {
            foreach(var old in target.GetComponents<HorizontalOrVerticalLayoutGroup>())Object.DestroyImmediate(old);
            return target.GetComponent<GridLayoutGroup>()??target.gameObject.AddComponent<GridLayoutGroup>();
        }

        private static void ApplyPage(GameObject root,JObject plan)
        {
            var page=Find(root,(string)plan["page"]) as RectTransform;
            if(page==null)throw new InvalidOperationException("页面缺失 "+plan["page"]);
            int family=(int)plan["family"];
            Vector4 frame=R(plan["frame"]);
            var shell=Find(root,"Panel_Frame") as RectTransform;if(shell!=null)Stretch(shell);
            var host=Find(root,"Grp_PageHost") as RectTransform;if(host!=null)Stretch(host);
            Stretch(page);
            string suffix=page.name.Substring(10);
            foreach(Transform nested in page.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Img_Formal"+suffix+"_Art")).ToArray())Object.DestroyImmediate(nested.gameObject);
            var old=page.Cast<Transform>().FirstOrDefault(t=>t.name.StartsWith("Grp_FormalArtwork"));if(old!=null)Object.DestroyImmediate(old.gameObject);
            var art=new GameObject("Grp_FormalArtwork_"+suffix,typeof(RectTransform));art.transform.SetParent(page,false);Stretch((RectTransform)art.transform);art.transform.SetAsFirstSibling();
            if(family!=14)
            {
                Image(art.transform,"Img_FormalSurface_"+suffix,new Vector4(frame.x+24,frame.y+24,frame.z-48,frame.w-48),null,Surface);
                var frameImage=Image(art.transform,"Bg_FormalFrame_"+suffix,frame,Get("M001-normal"),Color.white,true);frameImage.raycastTarget=true;
            }
            int ai=0;
            foreach(JObject item in (JArray)plan["artwork"])
            {
                Vector4 rect=R(item["rect"]);
                // Vector4 rect uses x,y,width,height; side illustrations occupy the upper slot.
                if(family>=5&&family<=7) {rect.x=frame.x+32;rect.y=frame.y+164;rect.z=frame.z-64;rect.w=240;}
                Image(art.transform,"Img_Formal"+suffix+"_Art"+(ai++).ToString("00"),rect,Get((string)item["key"]),new Color(1,1,1,(float)item["opacity"]),(string)item["mode"]=="slice");
            }
            foreach(JObject section in (JArray)plan["sections"])
            {
                var t=page.Find((string)section["name"]) as RectTransform;if(t==null)continue;
                Vector4 rect=R(section["rect"]);
                if(family>=5&&family<=7&&family!=0)
                {
                    int ordinal=((JArray)plan["sections"]).IndexOf(section);rect.y=550+ordinal*120;rect.w=104;
                }
                Put(t,rect);ConfigurePanel(t,rect);
                foreach(Transform drawing in art.transform.Cast<Transform>().ToArray())
                {
                    if(!drawing.name.Contains("_Art"))continue;
                    var dr=(RectTransform)drawing;Vector2 dp=dr.anchoredPosition;Vector2 ds=dr.sizeDelta;
                    float cx=dp.x+ds.x*.5f,cy=-dp.y+ds.y*.5f;
                    if(cx<rect.x||cx>rect.x+rect.z||cy<rect.y||cy>rect.y+rect.w)continue;
                    drawing.SetParent(t,false);
                    float artHeight=Mathf.Min(rect.w*.55f,320);
                    Put(dr,new Vector4(18,48,rect.z-36,artHeight));
                    drawing.SetSiblingIndex(Mathf.Min(1,t.childCount-1));
                    var list=t.Cast<Transform>().FirstOrDefault(c=>c.name.StartsWith("List_")) as RectTransform;
                    if(list!=null)list.offsetMax=new Vector2(-18,-(60+artHeight));
                }
            }
            var title=page.Cast<Transform>().FirstOrDefault(t=>t.name.StartsWith("Txt_")&&t.name.EndsWith("Title")) as RectTransform;
            bool compact=family>=5&&family<=7;
            if(title!=null)Put(title,family==14?new Vector4(1560,176,300,36):compact?new Vector4(frame.x+32,frame.y+34,frame.z-180,40):new Vector4(frame.x+122,frame.y+40,frame.z-360,40));
            ConfigureActions(page,frame,family);
            var targets=new List<RectTransform>();var rects=new List<Vector4>();
            AddShellTarget(root,"Txt_FormTitle",compact?new Vector4(frame.x+32,frame.y+34,frame.z-180,40):new Vector4(frame.x+122,frame.y+38,frame.z-430,42),targets,rects);
            AddShellTarget(root,"Btn_FormBack",new Vector4(frame.x+frame.z-288,frame.y+38,112,40),targets,rects);
            AddShellTarget(root,"Btn_FormClose",new Vector4(frame.x+frame.z-164,frame.y+38,112,40),targets,rects);
            AddShellTarget(root,"Btn_FieldClose",new Vector4(frame.x+frame.z-140,frame.y+34,112,40),targets,rects);
            var nav=Find(root,"Grp_Navigation") as RectTransform;
            if(nav!=null)
            {
                bool sideNav=family==4||family==9||family==15||family==16||family==2;
                Vector4 nr=compact?new Vector4(frame.x+32,frame.y+94,frame.z-64,112):sideNav?new Vector4(frame.x+64,frame.y+156,family==16?252:210,Mathf.Min(frame.w-300,570)):new Vector4(frame.x+64,frame.y+104,frame.z-128,44);
                var grid=Grid(nav);
                if(grid!=null){grid.constraint=compact||sideNav?GridLayoutGroup.Constraint.FixedColumnCount:GridLayoutGroup.Constraint.FixedRowCount;grid.constraintCount=compact?3:1;grid.cellSize=compact?new Vector2((nr.z-20)/3,32):sideNav?new Vector2(nr.z,60):new Vector2(166,42);grid.spacing=new Vector2(10,8);}
                targets.Add(nav);rects.Add(nr);
                nav.SetAsLastSibling();
            }
            var presentation=page.GetComponent<FormalUiPagePresentation>()??page.gameObject.AddComponent<FormalUiPagePresentation>();
            presentation.Configure(targets.ToArray(),rects.ToArray(),family);
            if(family==9)ConfigureCatalog(page);
            if(family==2&&suffix=="AudioSettings")
            {
                var body=Find(root,"Txt_AudioSettingsVolumesBody");
                if(body!=null)
                {
                    var layout=body.GetComponent<LayoutElement>();
                    if(layout!=null){layout.minHeight=64;layout.preferredHeight=64;}
                    body.GetComponent<TMP_Text>().fontSize=18;
                    ((RectTransform)body).sizeDelta=new Vector2(908,64);
                }
                var content=Find(root,"Content_AudioSettingsVolumes");
                if(content!=null)
                {
                    var group=content.GetComponent<VerticalLayoutGroup>();
                    if(group!=null){group.spacing=6;group.childControlWidth=false;group.childControlHeight=false;group.childForceExpandHeight=false;}
                }
                var controls=Find(root,"Panel_AudioSettingsControls") as RectTransform;
                if(controls!=null){controls.sizeDelta=new Vector2(908,228);var image=controls.GetComponent<Image>();if(image!=null)image.color=Color.clear;}
                int sliderIndex=0;
                foreach(var slider in page.GetComponentsInChildren<Slider>(true))
                {
                    var layout=slider.GetComponent<LayoutElement>();
                    if(layout==null)layout=slider.gameObject.AddComponent<LayoutElement>();
                    layout.minHeight=40;layout.preferredHeight=40;
                    Put((RectTransform)slider.transform,new Vector4(16,6+sliderIndex++*44,876,40));
                    var rootImage=slider.GetComponent<Image>();if(rootImage!=null)rootImage.color=Color.clear;
                    foreach(var label in slider.GetComponentsInChildren<TMP_Text>(true)){label.fontSize=18;label.rectTransform.sizeDelta=new Vector2(0,22);}
                    foreach(var track in slider.GetComponentsInChildren<Image>(true).Where(i=>i.name.EndsWith("Track"))){Skin(track,"M007-track-thin");track.raycastTarget=true;}
                }
            }
            if(family==1&&suffix=="SaveSlots")
            {
                var body=Find(root,"Txt_SaveSlotsSlotsBody");
                if(body!=null)
                {
                    var layout=body.GetComponent<LayoutElement>();if(layout!=null){layout.minHeight=44;layout.preferredHeight=44;}
                    var rect=(RectTransform)body;rect.sizeDelta=new Vector2(rect.sizeDelta.x,44);
                    var text=body.GetComponent<TMP_Text>();if(text!=null)text.fontSize=18;
                }
                var template=Find(root,"Item_SaveSlotsSlotsTemplate");
                if(template!=null)
                {
                    var layout=template.GetComponent<LayoutElement>();if(layout!=null){layout.minHeight=86;layout.preferredHeight=86;}
                    var rect=(RectTransform)template;rect.sizeDelta=new Vector2(rect.sizeDelta.x,86);
                    foreach(var text in template.GetComponentsInChildren<TMP_Text>(true))
                    {
                        text.fontSize=18;
                        if(text.name.EndsWith("RowValue"))text.rectTransform.sizeDelta=new Vector2(-24,36);
                    }
                }
                var empty=page.Find("Grp_SaveSlotsEmptyState");
                var preview=page.GetComponentsInChildren<Image>(true).FirstOrDefault(i=>i.name.StartsWith("Img_FormalSaveSlots_Art")&&AssetDatabase.GetAssetPath(i.sprite).Contains("scr-01-001"));
                if(empty!=null&&preview!=null)
                {
                    var feedback=empty.GetComponent<CanvasGroup>();
                    if(feedback==null)feedback=empty.gameObject.AddComponent<CanvasGroup>();
                    feedback.alpha=0;feedback.blocksRaycasts=false;feedback.interactable=false;
                    var state=empty.GetComponent<FormalUiPagePresentation>()??empty.gameObject.AddComponent<FormalUiPagePresentation>();
                    state.ConfigureState(preview,Get("S01-002"),Get("S01-001"));
                }
            }
            if(family==13&&suffix=="AlgorithmEditor")
            {
                var search=Find(root,"Panel_AlgorithmEditorNodeSearch") as RectTransform;
                if(search!=null){search.anchorMin=new Vector2(0,1);search.anchorMax=new Vector2(1,1);search.pivot=new Vector2(.5f,1);search.sizeDelta=new Vector2(-24,44);search.anchoredPosition=new Vector2(0,-8);}
                var controls=Find(root,"Panel_AlgorithmEditorControls") as RectTransform;
                if(controls!=null)
                {
                    controls.SetAsFirstSibling();
                    var layout=controls.GetComponent<LayoutElement>();if(layout!=null){layout.minHeight=60;layout.preferredHeight=60;}
                    var image=controls.GetComponent<Image>();if(image!=null){image.sprite=null;image.color=Color.clear;}
                }
                var status=Find(root,"Txt_AlgorithmToolbarStatus") as RectTransform;
                if(status!=null){Put(status,new Vector4(12,8,430,28));var text=status.GetComponent<TMP_Text>();text.fontSize=18;text.enableWordWrapping=false;text.overflowMode=TextOverflowModes.Ellipsis;}
                var toolbarHeading=Find(root,"Txt_AlgorithmEditorToolbarHeading");if(toolbarHeading!=null)toolbarHeading.gameObject.SetActive(false);
            }
            foreach(Transform state in page)
                if(state.name.StartsWith("Grp_")&&state.name.EndsWith("State"))
                {
                    var r=(RectTransform)state;
                    Put(r,new Vector4(frame.x+48,frame.y+154,frame.z-96,Mathf.Max(120,frame.w-300)));
                }
            // Brand/illustration are supplied as independent artwork, never as a full-screen screenshot.
            if((bool?)plan["entryMenu"]==true)
            {
                var existingLogo=Find(root,(string)plan["legacyLogo"]);if(existingLogo!=null)existingLogo.gameObject.SetActive(false);
                var actions=page.Find((string)plan["actionGroup"]) as RectTransform;
                if(actions!=null)
                {
                    Put(actions,new Vector4(300,470,352,330));
                    var grid=actions.GetComponent<GridLayoutGroup>();if(grid!=null){grid.constraint=GridLayoutGroup.Constraint.FixedColumnCount;grid.constraintCount=1;grid.cellSize=new Vector2(352,54);grid.spacing=new Vector2(0,12);}
                }
                foreach(JObject panelSpec in (JArray)plan["sections"])
                {
                    var panel=page.Find((string)panelSpec["name"]);if(panel==null)continue;
                    foreach(Transform child in panel)
                    {
                        if(child.name.EndsWith("Heading"))child.gameObject.SetActive(false);
                        if(child.name.StartsWith("List_")) {var list=(RectTransform)child;list.offsetMin=new Vector2(14,8);list.offsetMax=new Vector2(-14,-8);}
                    }
                }
            }
        }
        private static void AddShellTarget(GameObject root,string name,Vector4 rect,List<RectTransform> targets,List<Vector4> rects)
        {
            var t=Find(root,name) as RectTransform;if(t==null)return;targets.Add(t);rects.Add(rect);
            if(!name.StartsWith("Btn_"))return;
            if(t.GetComponentInChildren<TMP_Text>(true)!=null)return;
            var label=new GameObject("Txt_"+name.Substring(4)+"FormalLabel",typeof(RectTransform),typeof(CanvasRenderer),typeof(TextMeshProUGUI));label.transform.SetParent(t,false);
            var text=label.GetComponent<TextMeshProUGUI>();Stretch(text.rectTransform);text.rectTransform.sizeDelta=new Vector2(-12,-6);
            text.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AutoEraUiFontTool.FontAssetPath);text.fontSize=18;text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;
            text.color=new Color32(233,238,242,255);text.text=name.Contains("Close")?"关闭":"返回";
        }

        private static void ConfigurePanel(RectTransform panel,Vector4 rect)
        {
            var oldFill=panel.Find("Bg_FormalCardSurface");if(oldFill!=null)Object.DestroyImmediate(oldFill.gameObject);
            var fill=Image(panel,"Img_FormalCardSurface_"+panel.name,new Vector4(6,6,rect.z-12,rect.w-12),null,Card);fill.transform.SetAsFirstSibling();
            foreach(Transform child in panel)
            {
                if(child.name.EndsWith("Heading"))Put((RectTransform)child,new Vector4(18,10,rect.z-36,30));
                if(child.name.StartsWith("List_"))
                {
                    var r=(RectTransform)child;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.pivot=new Vector2(.5f,.5f);r.offsetMin=new Vector2(18,14);r.offsetMax=new Vector2(-18,-48);
                    var scroll=child.GetComponent<ScrollRect>();if(scroll!=null){scroll.horizontal=false;scroll.vertical=true;}
                }
            }
        }
        private static void ConfigureActions(RectTransform page,Vector4 frame,int family)
        {
            foreach(Transform child in page)
            {
                if(!child.name.StartsWith("Grp_")||!child.name.EndsWith("Actions"))continue;
                int count=child.GetComponentsInChildren<Button>(true).Length;if(count==0)continue;
                bool side=family>=5&&family<=7;
                int cols=side?Mathf.Min(3,count):Mathf.Min(6,count);
                float available=side?frame.z-56:frame.z-128;
                float height=side?38:48;int rows=Mathf.CeilToInt(count/(float)cols);
                float total=rows*(height+8)-8;
                var r=(RectTransform)child;Put(r,new Vector4(frame.x+(side?28:64),frame.y+frame.w-total-(side?28:56),available,total));
                var grid=Grid(r);
                if(grid!=null){grid.constraint=GridLayoutGroup.Constraint.FixedColumnCount;grid.constraintCount=cols;grid.cellSize=new Vector2((available-(cols-1)*8)/cols,height);grid.spacing=new Vector2(8,8);}
            }
        }

        private static void ConfigureCatalog(RectTransform page)
        {
            string id=page.name.Substring(10);
            if(id.EndsWith("Detail")||id=="WarehouseRecords")return;
            var catalog=page.Cast<Transform>().FirstOrDefault(t=>t.name.EndsWith("Catalog"));if(catalog==null)return;
            var content=catalog.GetComponentsInChildren<RectTransform>(true).FirstOrDefault(t=>t.name.StartsWith("Content_"));
            if(content==null)return;
            var grid=Grid(content);grid.constraint=GridLayoutGroup.Constraint.FixedColumnCount;grid.constraintCount=id.Contains("Templates")?2:3;
            float width=738;grid.cellSize=new Vector2((width-16*(grid.constraintCount-1))/grid.constraintCount,218);grid.spacing=new Vector2(16,16);
            foreach(var template in content.GetComponentsInChildren<RectTransform>(true).Where(t=>t.name.StartsWith("Item_")&&t.name.EndsWith("Template")))
            {
                var image=template.GetComponent<Image>();if(image!=null)Skin(image,"M011-card-normal");
                var row=template.GetComponentInChildren<Button>(true);if(row==null)continue;
                var rowImage=row.GetComponent<Image>();Skin(rowImage,"M011-card-normal");
                row.spriteState=new SpriteState {highlightedSprite=Get("M011-card-raised"),pressedSprite=Get("M011-card-selected"),selectedSprite=Get("M011-card-selected"),disabledSprite=Get("M011-card-empty")};
                foreach(var text in template.GetComponentsInChildren<TMP_Text>(true))
                {
                    var r=text.rectTransform;r.anchorMin=new Vector2(0,text.name.EndsWith("RowLabel")?1:0);r.anchorMax=new Vector2(1,text.name.EndsWith("RowLabel")?1:0);r.pivot=new Vector2(.5f,text.name.EndsWith("RowLabel")?1:0);
                    r.sizeDelta=new Vector2(-28,text.name.EndsWith("RowLabel")?64:96);r.anchoredPosition=new Vector2(0,text.name.EndsWith("RowLabel")?-18:18);
                    text.alignment=TextAlignmentOptions.Center;text.fontSize=20;
                }
            }
        }

        private static void ApplyHud(GameObject root)
        {
            var rects=new Dictionary<string,Vector4> {
                {"Panel_PageHudStatus",new Vector4(24,24,842,98)}, {"Panel_PageHudTracker",new Vector4(1488,24,408,188)},
                {"Panel_PageHudAlerts",new Vector4(1488,248,408,152)}, {"Panel_PageHudNavigation",new Vector4(28,978,1260,78)},
                {"Panel_PageHudSave",new Vector4(1170,26,292,76)}};
            foreach(var pair in rects)
            {
                var r=Find(root,pair.Key) as RectTransform;if(r==null)continue;Put(r,pair.Value);
                var image=r.GetComponent<Image>();if(image!=null)Skin(image,"M011-card-normal");
            }
        }

        private static void StyleItems()
        {
            foreach(string path in Directory.GetFiles("Assets/Game/Prefabs/UI/Item","*.prefab"))
            {
                var root=PrefabUtility.LoadPrefabContents(path);
                try
                {
                    Style(root);
                    foreach(var image in root.GetComponentsInChildren<Image>(true))
                    {
                        if(root.name=="AlgorithmNodeItem"&&(image.transform==root.transform||image.name=="Btn_AlgorithmNodeSelect"))Skin(image,"M012-node-normal");
                        else if(image.name.Contains("Port"))Skin(image,"M012-node-port");
                        else if(root.name=="AlgorithmEdgeItem") {Skin(image,"M012-edge-straight");image.preserveAspect=false;}
                    }
                    var node=root.GetComponent<AlgorithmNodeItem>();
                    if(node!=null)
                    {
                        var serialized=new SerializedObject(node);
                        foreach(var pair in new [] {new [] {"_formalNormal","M012-node-normal"},new [] {"_formalSelected","M012-node-selected"},new [] {"_formalError","M012-node-error"}})
                            serialized.FindProperty(pair[0]).objectReferenceValue=Get(pair[1]);
                        serialized.ApplyModifiedPropertiesWithoutUndo();
                    }
                    PrefabUtility.SaveAsPrefabAsset(root,path);
                }
                finally {PrefabUtility.UnloadPrefabContents(root);}
            }
        }

        private static void SyncContract(GameObject root,string path)
        {
            var doc=JObject.Parse(File.ReadAllText(path));
            SyncNode(root.transform,(JObject)doc["root"]);
            doc["formalArtSource"]=Change+"/art/page-plan.json";
            File.WriteAllText(path,doc.ToString());
        }
        private static JArray A(Vector2 v)=>new JArray(v.x,v.y);
        private static void SyncNode(Transform t,JObject node)
        {
            var r=t as RectTransform;
            if(r!=null){node["anchorMin"]=A(r.anchorMin);node["anchorMax"]=A(r.anchorMax);node["pivot"]=A(r.pivot);node["sizeDelta"]=A(r.sizeDelta);node["anchoredPosition"]=A(r.anchoredPosition);}
            node["active"]=t.gameObject.activeSelf;
            if(!(node["components"] is JArray components)) {components=new JArray();node["components"]=components;}
            foreach(var obsolete in components.OfType<JObject>().Where(c=>new [] {"HorizontalLayoutGroup","VerticalLayoutGroup","GridLayoutGroup"}.Contains((string)c["type"])&&!t.GetComponents<Component>().Any(component=>component!=null&&component.GetType().Name==(string)c["type"])).ToArray())obsolete.Remove();
            var image=t.GetComponent<Image>();
            if(image!=null)
            {
                var spec=components.OfType<JObject>().FirstOrDefault(c=>(string)c["type"]=="Image");
                if(spec==null){spec=new JObject {["type"]="Image"};components.Add(spec);}
                spec["spritePath"]=image.sprite==null?"":AssetDatabase.GetAssetPath(image.sprite);
                spec["imageType"]=(int)image.type;spec["preserveAspect"]=image.preserveAspect;spec["raycastTarget"]=image.raycastTarget;
                spec["color"]=new JArray(image.color.r,image.color.g,image.color.b,image.color.a);
            }
            var grid=t.GetComponent<GridLayoutGroup>();
            if(grid!=null)
            {
                var spec=components.OfType<JObject>().FirstOrDefault(c=>(string)c["type"]=="GridLayoutGroup");
                if(spec==null){spec=new JObject {["type"]="GridLayoutGroup"};components.Add(spec);}
                if(spec!=null){spec["cellSize"]=A(grid.cellSize);spec["spacing"]=A(grid.spacing);spec["constraint"]=(int)grid.constraint;spec["constraintCount"]=grid.constraintCount;}
            }
            var linear=t.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if(linear!=null)
            {
                string type=linear.GetType().Name;
                var spec=components.OfType<JObject>().FirstOrDefault(c=>(string)c["type"]==type);
                if(spec==null){spec=new JObject {["type"]=type};components.Add(spec);}
                spec["spacing"]=linear.spacing;spec["ChildControlWidth"]=linear.childControlWidth;spec["ChildControlHeight"]=linear.childControlHeight;
                spec["ChildForceExpandWidth"]=linear.childForceExpandWidth;spec["ChildForceExpandHeight"]=linear.childForceExpandHeight;
            }
            var text=t.GetComponent<TMP_Text>();
            if(text!=null){var spec=components.OfType<JObject>().FirstOrDefault(c=>(string)c["type"]=="TextMeshProUGUI");if(spec==null){spec=new JObject {["type"]="TextMeshProUGUI",["fontPath"]=AssetDatabase.GetAssetPath(text.font),["raycastTarget"]=false};components.Add(spec);}spec["text"]=text.text;spec["fontSize"]=text.fontSize;spec["hAlign"]=(int)text.horizontalAlignment;spec["vAlign"]=(int)text.verticalAlignment;}
            var canvasGroup=t.GetComponent<CanvasGroup>();
            if(canvasGroup!=null)
            {
                var spec=components.OfType<JObject>().FirstOrDefault(c=>(string)c["type"]=="CanvasGroup");
                if(spec==null){spec=new JObject {["type"]="CanvasGroup"};components.Add(spec);}
                spec["alpha"]=canvasGroup.alpha;spec["blocksRaycasts"]=canvasGroup.blocksRaycasts;spec["interactable"]=canvasGroup.interactable;
            }
            var element=t.GetComponent<LayoutElement>();
            if(element!=null)
            {
                var spec=components.OfType<JObject>().FirstOrDefault(c=>(string)c["type"]=="LayoutElement");
                if(spec==null){spec=new JObject {["type"]="LayoutElement"};components.Add(spec);}
                spec["minWidth"]=element.minWidth;spec["minHeight"]=element.minHeight;spec["preferredWidth"]=element.preferredWidth;spec["preferredHeight"]=element.preferredHeight;
                spec["flexibleWidth"]=element.flexibleWidth;spec["flexibleHeight"]=element.flexibleHeight;spec["ignoreLayout"]=element.ignoreLayout;
            }
            var button=t.GetComponent<Button>();
            if(button!=null){var spec=components.OfType<JObject>().FirstOrDefault(c=>(string)c["type"]=="Button");if(spec!=null)spec["transition"]=(int)button.transition;}
            var children=node["children"] as JArray;
            if(children==null){children=new JArray();node["children"]=children;}
            foreach(var obsolete in children.OfType<JObject>().Where(c=>t.Find((string)c["name"])==null).ToArray())obsolete.Remove();
            var ordered=new JArray();
            foreach(Transform child in t)
            {
                var entry=children.OfType<JObject>().FirstOrDefault(c=>(string)c["name"]==child.name);
                if(entry==null){entry=new JObject {["name"]=child.name,["children"]=new JArray(),["components"]=new JArray()};children.Add(entry);}
                SyncNode(child,entry);
                ordered.Add(entry.DeepClone());
            }
            node["children"]=ordered;
        }

        public static IEnumerator CaptureFamily(int family)
        {
            RequireEditMode();Directory.CreateDirectory(Evidence+"/previews");
            foreach(JObject p in Read("page-plan"))
            {
                if((int)p["family"]!=family)continue;
                string output=Evidence+"/previews/"+family.ToString("00")+"-"+p["form"]+"-"+p["page"]+".png";
                Preview((string)p["prefab"],(string)p["page"],output,family==3);
                yield return null;
            }
        }
        private static void Preview(string path,string page,string output,bool hud)
        {
            var scene=EditorSceneManager.NewPreviewScene();RenderTexture target=null;Texture2D pixels=null;var previous=RenderTexture.active;Camera camera=null;
            try
            {
                var canvasGo=new GameObject("B54Preview",typeof(RectTransform),typeof(Canvas));UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(canvasGo,scene);
                var cameraGo=new GameObject("B54Camera",typeof(Camera));UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraGo,scene);
                camera=cameraGo.GetComponent<Camera>();camera.scene=scene;camera.orthographic=true;camera.orthographicSize=540;camera.transform.position=new Vector3(0,0,-1000);
                camera.nearClipPlane=.1f;camera.farClipPlane=2000;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color32(17,25,35,255);
                var canvas=canvasGo.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=100;
                target=new RenderTexture(1920,1080,24);camera.targetTexture=target;
                var instance=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path),canvasGo.transform);instance.SetActive(true);
                foreach(Transform t in instance.GetComponentsInChildren<Transform>(true))
                {
                    if(t.name.StartsWith("Panel_Page"))t.gameObject.SetActive(hud||t.name==page);
                    if(t.name.StartsWith("Grp_")&&t.name.EndsWith("State"))t.gameObject.SetActive(false);
                }
                foreach(var presentation in instance.GetComponentsInChildren<FormalUiPagePresentation>(false))presentation.ApplyForPreview();
                Canvas.ForceUpdateCanvases();foreach(var text in instance.GetComponentsInChildren<TMP_Text>(true))text.ForceMeshUpdate();Canvas.ForceUpdateCanvases();camera.Render();
                RenderTexture.active=target;pixels=new Texture2D(1920,1080,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,1920,1080),0,0);pixels.Apply();File.WriteAllBytes(output,pixels.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active=previous;if(camera!=null)camera.targetTexture=null;
                if(pixels!=null)Object.DestroyImmediate(pixels);if(target!=null){target.Release();Object.DestroyImmediate(target);}EditorSceneManager.ClosePreviewScene(scene);
            }
        }
        private static void RequireEditMode(){if(EditorApplication.isPlaying)throw new InvalidOperationException("请停止 Play Mode 后修改正式 UI 资产。");}
    }
}
