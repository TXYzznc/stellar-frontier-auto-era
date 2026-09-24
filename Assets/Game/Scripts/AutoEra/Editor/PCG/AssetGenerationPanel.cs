using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AutoEra.Editor.PCG
{

    /// <summary>
    /// 资产生成面板（ArtResource）：程序化生成岩石 / 晶簇 / 矿石簇。
    /// 顶部标签栏切换资产类型，各标签独立显示参数；底部共享预览区（左键旋转 + 滚轮缩放）。
    /// 预设区自动扫描 Assets/Game/ScriptableAssets/PCG/ 下的预设资产生成列表（点击即载入），新增预设自动同步。
    /// 「生成到场景」= 按当前标签参数动态生成 mesh（不保存 Prefab）；
    /// 「保存参数」= 存成 RockPreset / CrystalClusterPreset。
    /// 用法：Tools/Auto Era Art/资产生成
    /// </summary>
    public class AssetGenerationPanel : EditorWindow
    {
        // ===== 标签 =====
        private int _selectedTab = 0;   // 0=岩石 1=晶簇 2=矿石簇

        // ===== 岩石参数 =====
        private int _rockCount = 8;
        private int _rockSubdivisions = 3;
        private Vector2 _rockNoiseRange = new Vector2(0.4f, 0.9f);
        private Vector2 _rockSquashRange = new Vector2(0.5f, 0.85f);
        private Color _rockColor = new Color(0.46f, 0.44f, 0.41f, 1f);
        private float _rockMetallic = 0f;
        private float _rockSmoothness = 0.18f;
        private Vector2 _rockFootprintRange = new Vector2(1f, 2f);
        private int _rockSeed = 20260930;
        private string _rockPresetName = "新岩石";

        // ===== 晶体参数 =====
        private int _crystalCount = 6;
        private int _crystalSubdivisions = 2;
        private Vector2 _spikeRatioRange = new Vector2(0.15f, 0.55f);
        private Vector2 _spikeLenRange = new Vector2(1.4f, 3.2f);
        private Vector2 _crystalBaseRadiusRange = new Vector2(0.7f, 1.0f);
        private Color _crystalColor = new Color(0.50f, 0.58f, 0.90f, 0.55f);
        private float _crystalMetallic = 0.05f;
        private float _crystalSmoothness = 0.95f;
        private Vector2 _crystalFootprintRange = new Vector2(0.5f, 1f);
        private int _crystalSeed = 20261002;
        private string _crystalPresetName = "新晶簇";

        // ===== 矿石簇参数 =====
        private int _clusterCount = 6;
        private OreType _oreType = OreType.Rubble;
        private Vector2 _blocksRange = new Vector2(3f, 8f);
        private Vector2 _blockSizeRange = new Vector2(0.25f, 0.55f);
        private Vector2 _clusterRadiusRange = new Vector2(0.5f, 0.9f);
        private Color _oreColor = new Color(0.60f, 0.46f, 0.30f, 1f);
        private float _oreOpacity = 1f;
        private float _oreMetallic = 0.65f;
        private float _oreSmoothness = 0.45f;
        private Vector2 _oreFootprintRange = new Vector2(0.5f, 1f);
        private string _orePresetName = "新矿石簇";

        // ===== 材质（替代颜色/金属度/平滑度独立参数，用材质属性面板编辑） =====
        private Material _rockMaterial;
        private UnityEditor.Editor _rockMaterialEditor;
        private Material _crystalMaterial;
        private UnityEditor.Editor _crystalMaterialEditor;
        private Material _oreMaterial;
        private UnityEditor.Editor _oreMaterialEditor;

        // ===== 预览 =====
        private PreviewRenderUtility _preview;
        private int _previewVariant = 0;
        private float _previewYaw = 30f;
        private float _previewPitch = 15f;
        private float _previewDistance = 4.5f;
        private bool _draggingPreview = false;

        // ===== 场景生成预览根 =====
        private GameObject _scenePreviewRoot;

        // ===== 预设列表（自动扫描） =====
        private List<RockPreset> _rockPresets = new List<RockPreset>();
        private List<CrystalClusterPreset> _crystalPresets = new List<CrystalClusterPreset>();
        private Vector2 _presetListScroll = Vector2.zero;

        private Vector2 _scroll;
        private string _lastMessage = string.Empty;

        [MenuItem("Tools/Auto Era Art/资产生成")]
        public static void Open()
        {
            AssetGenerationPanel window = GetWindow<AssetGenerationPanel>("资产生成");
            window.minSize = new Vector2(500f, 680f);
            window.Show();
        }

        private void OnEnable()
        {
            if (_preview == null)
            {
                _preview = new PreviewRenderUtility();
                _preview.camera.fieldOfView = 45f;
                _preview.camera.nearClipPlane = 0.1f;
                _preview.camera.farClipPlane = 100f;
            }
            RefreshPresets();
        }

        private void OnDisable()
        {
            if (_preview != null)
            {
                _preview.Cleanup();
                _preview = null;
            }
            DestroyMaterialEditor(ref _rockMaterialEditor);
            DestroyMaterialEditor(ref _crystalMaterialEditor);
            DestroyMaterialEditor(ref _oreMaterialEditor);
        }

        private static void DestroyMaterialEditor(ref UnityEditor.Editor editor)
        {
            if (editor != null)
            {
                UnityEngine.Object.DestroyImmediate(editor);
                editor = null;
            }
        }

        private void OnGUI()
        {
            // ===== 顶部标签栏 =====
            EditorGUILayout.Space(4f);
            _selectedTab = GUILayout.Toolbar(_selectedTab, new[] { "岩石", "晶簇", "矿石簇" });
            EditorGUILayout.Space(4f);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            RefreshPresets();

            switch (_selectedTab)
            {
                case 0: DrawRockTab(); break;
                case 1: DrawCrystalTab(); break;
                case 2: DrawOreTab(); break;
            }

            // ===== 共享预览区 =====
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("预览（左键旋转 · 滚轮缩放）", EditorStyles.boldLabel);
            int maxVariant = _selectedTab == 0 ? _rockCount : (_selectedTab == 1 ? _crystalCount : _clusterCount);
            _previewVariant = EditorGUILayout.IntSlider("变体", _previewVariant, 0, Mathf.Max(0, maxVariant - 1));
            Rect previewRect = GUILayoutUtility.GetRect(320f, 300f);
            DrawPreview(previewRect);

            EditorGUILayout.EndScrollView();
        }

        // ============ 标签页 ============

        private void DrawRockTab()
        {
            EditorGUILayout.LabelField("岩石（噪声位移 + Y 压扁）", EditorStyles.boldLabel);
            _rockCount = EditorGUILayout.IntSlider("变体数", _rockCount, 1, 20);
            _rockSubdivisions = EditorGUILayout.IntSlider("细分（面数）", _rockSubdivisions, 2, 4);
            DrawRange("噪声强度范围", ref _rockNoiseRange, 0.05f, 1.5f);
            DrawRange("Y 压扁范围", ref _rockSquashRange, 0.3f, 1f);
            DrawRange("占地大小（正方形，米）", ref _rockFootprintRange, 0.1f, 10f);
            DrawSeed(ref _rockSeed);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("材质（颜色/金属度/平滑度在下方材质面板中编辑）", EditorStyles.boldLabel);
            DrawMaterialProperty("岩石材质", ref _rockMaterial, ref _rockMaterialEditor);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("生成到场景", GUILayout.Height(30f)))
            {
                GenerateToScene();
            }
            if (GUILayout.Button("保存岩石参数", GUILayout.Height(30f)))
            {
                SaveRockPreset();
            }
            EditorGUILayout.EndHorizontal();
            _rockPresetName = EditorGUILayout.TextField("预设名", _rockPresetName);

            if (!string.IsNullOrEmpty(_lastMessage))
            {
                EditorGUILayout.HelpBox(_lastMessage, MessageType.None);
            }

            DrawPresetList("岩石预设", () =>
            {
                foreach (RockPreset p in _rockPresets)
                {
                    if (GUILayout.Button(p.name))
                    {
                        LoadRockPreset(p);
                    }
                }
            });
        }

        private void DrawCrystalTab()
        {
            EditorGUILayout.LabelField("晶体（尖刺多面体）", EditorStyles.boldLabel);
            _crystalCount = EditorGUILayout.IntSlider("变体数", _crystalCount, 1, 20);
            _crystalSubdivisions = EditorGUILayout.IntSlider("细分（面数）", _crystalSubdivisions, 1, 4);
            DrawRange("尖刺比例范围", ref _spikeRatioRange, 0.02f, 0.9f);
            DrawRange("尖刺长度范围", ref _spikeLenRange, 1.1f, 4.5f);
            DrawRange("底部半径范围", ref _crystalBaseRadiusRange, 0.6f, 1.3f);
            DrawRange("占地大小（正方形，米）", ref _crystalFootprintRange, 0.1f, 10f);
            DrawSeed(ref _crystalSeed);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("材质（颜色/透明度/金属度/平滑度在下方材质面板中编辑）", EditorStyles.boldLabel);
            DrawMaterialProperty("晶簇材质", ref _crystalMaterial, ref _crystalMaterialEditor);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("生成到场景", GUILayout.Height(30f)))
            {
                GenerateToScene();
            }
            if (GUILayout.Button("保存晶簇参数", GUILayout.Height(30f)))
            {
                SaveCrystalPreset();
            }
            EditorGUILayout.EndHorizontal();
            _crystalPresetName = EditorGUILayout.TextField("预设名", _crystalPresetName);

            if (!string.IsNullOrEmpty(_lastMessage))
            {
                EditorGUILayout.HelpBox(_lastMessage, MessageType.None);
            }

            DrawPresetList("晶簇预设", () =>
            {
                foreach (CrystalClusterPreset p in _crystalPresets)
                {
                    if (p.CrystalCount <= 0)
                    {
                        continue;
                    }
                    if (GUILayout.Button(p.name))
                    {
                        LoadCrystalPreset(p);
                    }
                }
            });
        }

        private void DrawOreTab()
        {
            EditorGUILayout.LabelField("矿石簇（多矿块聚集）", EditorStyles.boldLabel);
            OreType newType = (OreType)EditorGUILayout.EnumPopup("矿石类型", _oreType);
            if (newType != _oreType)
            {
                _oreType = newType;
                _oreColor = PCGMeshFactory.GetDefaultOreColor(_oreType);
            }
            _clusterCount = EditorGUILayout.IntSlider("变体数", _clusterCount, 1, 20);
            DrawRange("块数范围", ref _blocksRange, 2f, 20f);
            DrawRange("块大小范围", ref _blockSizeRange, 0.1f, 1f);
            DrawRange("聚集半径范围", ref _clusterRadiusRange, 0.2f, 2.5f);
            DrawRange("占地大小（正方形，米）", ref _oreFootprintRange, 0.1f, 10f);
            DrawSeed(ref _crystalSeed);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("材质（颜色/透明度/金属度/平滑度在下方材质面板中编辑）", EditorStyles.boldLabel);
            DrawMaterialProperty("矿石簇材质", ref _oreMaterial, ref _oreMaterialEditor);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("生成到场景", GUILayout.Height(30f)))
            {
                GenerateToScene();
            }
            if (GUILayout.Button("保存矿石簇参数", GUILayout.Height(30f)))
            {
                SaveOrePreset();
            }
            EditorGUILayout.EndHorizontal();
            _orePresetName = EditorGUILayout.TextField("预设名", _orePresetName);

            if (!string.IsNullOrEmpty(_lastMessage))
            {
                EditorGUILayout.HelpBox(_lastMessage, MessageType.None);
            }

            DrawPresetList("矿石簇预设", () =>
            {
                foreach (CrystalClusterPreset p in _crystalPresets)
                {
                    if (p.ClusterCount <= 0)
                    {
                        continue;
                    }
                    if (GUILayout.Button(p.name))
                    {
                        LoadOrePreset(p);
                    }
                }
            });
        }

        private static void DrawRange(string label, ref Vector2 range, float min, float max)
        {
            float mn = range.x, mx = range.y;
            EditorGUILayout.MinMaxSlider(label, ref mn, ref mx, min, max);
            mn = Mathf.Clamp(mn, min, max);
            mx = Mathf.Clamp(mx, min, max);
            if (mn > mx)
            {
                float tmp = mn;
                mn = mx;
                mx = tmp;
            }
            range = new Vector2(mn, mx);
            EditorGUILayout.LabelField("  范围：" + range.x.ToString("F2") + " ~ " + range.y.ToString("F2"));
        }

        private static void DrawSeed(ref int seed)
        {
            EditorGUILayout.BeginHorizontal();
            seed = EditorGUILayout.IntField("种子", seed);
            if (GUILayout.Button("随机", GUILayout.Width(50f)))
            {
                seed = UnityEngine.Random.Range(0, 100000000);
            }
            EditorGUILayout.EndHorizontal();
        }

        // ============ 预设列表（自动扫描） ============

        private void RefreshPresets()
        {
            _rockPresets = LoadAllAssets<RockPreset>("Assets/Game/ScriptableAssets/PCG/Rock");
            _crystalPresets = LoadAllAssets<CrystalClusterPreset>("Assets/Game/ScriptableAssets/PCG/CrystalCluster");
        }

        private static List<T> LoadAllAssets<T>(string folder) where T : ScriptableObject
        {
            List<T> result = new List<T>();
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { folder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    result.Add(asset);
                }
            }
            return result;
        }

        private void DrawPresetList(string title, System.Action drawItems)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            if (GUILayout.Button("刷新预设列表", GUILayout.Height(22f)))
            {
                RefreshPresets();
            }
            _presetListScroll = EditorGUILayout.BeginScrollView(_presetListScroll, GUILayout.Height(120f));
            drawItems();
            EditorGUILayout.EndScrollView();
        }

        // ============ 预览 ============

        private void DrawPreview(Rect rect)
        {
            if (_preview == null || rect.width <= 1f || rect.height <= 1f)
            {
                return;
            }

            HandlePreviewInput(rect);

            Mesh m = null;
            Material mat = null;
            if (_selectedTab == 0)
            {
                m = CreateRockMesh(_previewVariant);
                mat = CreateRockMaterial();
            }
            else if (_selectedTab == 1)
            {
                m = CreateCrystalMesh(_previewVariant);
                mat = CreateCrystalMaterial();
            }
            else
            {
                m = CreateClusterMesh(_previewVariant);
                mat = CreateClusterMaterial();
            }

            _preview.BeginPreview(rect, GUIStyle.none);
            _preview.lights[0].intensity = 1.1f;
            _preview.lights[0].transform.rotation = Quaternion.Euler(30f, 30f, 0f);
            _preview.lights[1].intensity = 0.5f;
            Vector3 target = Vector3.zero;
            Quaternion rot = Quaternion.Euler(_previewPitch, _previewYaw, 0f);
            _preview.camera.transform.position = target + rot * new Vector3(0f, 0f, -_previewDistance);
            _preview.camera.transform.LookAt(target);
            _preview.camera.backgroundColor = new Color(0.30f, 0.32f, 0.38f, 1f);
            _preview.DrawMesh(m, Matrix4x4.identity, mat, 0);
            _preview.camera.Render();
            Texture tex = _preview.EndPreview();
            GUI.DrawTexture(rect, tex, ScaleMode.StretchToFill);

            UnityEngine.Object.DestroyImmediate(m);
            if (mat != null && !AssetDatabase.Contains(mat))
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        private void HandlePreviewInput(Rect rect)
        {
            Event e = Event.current;
            if (e == null)
            {
                return;
            }
            bool inside = rect.Contains(e.mousePosition);

            if (e.type == EventType.MouseDown && inside && e.button == 0)
            {
                _draggingPreview = true;
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                _draggingPreview = false;
                if (inside)
                {
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseDrag && _draggingPreview && e.button == 0)
            {
                _previewYaw += e.delta.x * 0.5f;
                _previewPitch += e.delta.y * 0.5f;
                _previewPitch = Mathf.Clamp(_previewPitch, -85f, 85f);
                e.Use();
                Repaint();
            }
            else if (e.type == EventType.ScrollWheel && inside)
            {
                _previewDistance *= Mathf.Clamp(1f - e.delta.y * 0.05f, 0.5f, 2f);
                _previewDistance = Mathf.Clamp(_previewDistance, 1.2f, 15f);
                e.Use();
                Repaint();
            }
        }

        private Mesh CreateRockMesh(int variantIndex)
        {
            return PCGMeshFactory.CreateRock(_rockSeed + variantIndex, _rockSubdivisions, _rockNoiseRange, _rockSquashRange);
        }

        private Mesh CreateCrystalMesh(int variantIndex)
        {
            return PCGMeshFactory.CreateCrystal(_crystalSeed + variantIndex, _crystalSubdivisions,
                _spikeRatioRange, _spikeLenRange, _crystalBaseRadiusRange);
        }

        private Mesh CreateClusterMesh(int variantIndex)
        {
            int span = Mathf.Max(1, (int)_blocksRange.y - (int)_blocksRange.x + 1);
            int blocks = (int)_blocksRange.x + (variantIndex % span);
            return PCGMeshFactory.CreateOreCluster(_oreType, _crystalSeed + 100 + variantIndex, blocks,
                _blockSizeRange, _clusterRadiusRange);
        }

        // ============ 材质 ============

        private Material CreateRockMaterial()
        {
            if (_rockMaterial != null)
            {
                return _rockMaterial;
            }
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = _rockColor;
            mat.SetFloat("_Metallic", _rockMetallic);
            mat.SetFloat("_Smoothness", _rockSmoothness);
            return mat;
        }

        private Material CreateCrystalMaterial()
        {
            if (_crystalMaterial != null)
            {
                return _crystalMaterial;
            }
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = _crystalColor;
            mat.SetFloat("_Metallic", _crystalMetallic);
            mat.SetFloat("_Smoothness", _crystalSmoothness);
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.DisableKeyword("_SURFACE_TYPE_OPAQUE");
            return mat;
        }

        private Material CreateClusterMaterial()
        {
            if (_oreMaterial != null)
            {
                return _oreMaterial;
            }
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(_oreColor.r, _oreColor.g, _oreColor.b, _oreOpacity);
            mat.SetFloat("_Smoothness", _oreSmoothness);
            mat.SetFloat("_Metallic", _oreMetallic);
            if (_oreOpacity < 0.999f)
            {
                ConfigureTransparentMaterial(mat);
            }
            return mat;
        }

        private static void ConfigureTransparentMaterial(Material mat)
        {
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.DisableKeyword("_SURFACE_TYPE_OPAQUE");
        }

        // ============ 材质属性面板 ============

        /// <summary>绘制材质引用字段 + 内嵌材质属性面板（MaterialEditor）：颜色/金属度/平滑度/Shader 都由此编辑。</summary>
        private void DrawMaterialProperty(string assetName, ref Material material, ref UnityEditor.Editor materialEditor)
        {
            EditorGUILayout.BeginHorizontal();
            Material newMat = (Material)EditorGUILayout.ObjectField("材质", material, typeof(Material), false);
            if (GUILayout.Button("新建", GUILayout.Width(44f)))
            {
                newMat = CreateNewMaterialAsset(assetName);
            }
            EditorGUILayout.EndHorizontal();

            if (newMat != material)
            {
                material = newMat;
                if (materialEditor != null)
                {
                    UnityEngine.Object.DestroyImmediate(materialEditor);
                    materialEditor = null;
                }
            }

            if (material == null)
            {
                EditorGUILayout.HelpBox("未指定材质：生成时回退使用内置颜色/金属度/平滑度参数。", MessageType.None);
                return;
            }

            if (materialEditor == null || materialEditor.target != material)
            {
                if (materialEditor != null)
                {
                    UnityEngine.Object.DestroyImmediate(materialEditor);
                }
                materialEditor = UnityEditor.Editor.CreateEditor(material);
            }

            EditorGUI.indentLevel++;
            materialEditor.OnInspectorGUI();
            EditorGUI.indentLevel--;
        }

        private static Material CreateNewMaterialAsset(string assetName)
        {
            string dir = "Assets/Game/Materials";
            if (!AssetDatabase.IsValidFolder(dir))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Game"))
                {
                    AssetDatabase.CreateFolder("Assets", "Game");
                }
                AssetDatabase.CreateFolder("Assets/Game", "Materials");
            }
            string path = AssetDatabase.GenerateUniqueAssetPath(dir + "/" + assetName + ".mat");
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = mat;
            return mat;
        }

        // ============ 生成到场景 / 保存 / 载入 ============

        private void GenerateToScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                _lastMessage = "没有打开的场景，请先打开或新建场景。";
                return;
            }

            if (_scenePreviewRoot != null)
            {
                Undo.DestroyObjectImmediate(_scenePreviewRoot);
            }
            Transform root = new GameObject("PCG_ASSET_GEN_PREVIEW").transform;
            _scenePreviewRoot = root.gameObject;

            int placed = 0;
            if (_selectedTab == 0)
            {
                Material mat = CreateRockMaterial();
                for (int i = 0; i < _rockCount; i++)
                {
                    Mesh m = CreateRockMesh(i);
                    GameObject go = new GameObject("Rock_" + (i + 1).ToString("D2"));
                    go.AddComponent<MeshFilter>().sharedMesh = m;
                    go.AddComponent<MeshRenderer>().sharedMaterial = mat;
                    go.transform.SetParent(root, true);
                    go.transform.position = new Vector3(i * 3f, 1.5f, 0f);
                    go.transform.localScale = Vector3.one * PCGMeshFactory.ScaleForFootprint(m, PCGMeshFactory.RandomFootprint(_rockFootprintRange, new System.Random(_rockSeed + i)));
                    placed++;
                }
            }
            else if (_selectedTab == 1)
            {
                Material mat = CreateCrystalMaterial();
                for (int i = 0; i < _crystalCount; i++)
                {
                    Mesh m = CreateCrystalMesh(i);
                    GameObject go = new GameObject("Crystal_" + (i + 1).ToString("D2"));
                    go.AddComponent<MeshFilter>().sharedMesh = m;
                    go.AddComponent<MeshRenderer>().sharedMaterial = mat;
                    go.transform.SetParent(root, true);
                    go.transform.position = new Vector3(i * 3f, 1.5f, 0f);
                    go.transform.localScale = Vector3.one * PCGMeshFactory.ScaleForFootprint(m, PCGMeshFactory.RandomFootprint(_crystalFootprintRange, new System.Random(_crystalSeed + i)));
                    placed++;
                }
            }
            else
            {
                Material mat = CreateClusterMaterial();
                for (int i = 0; i < _clusterCount; i++)
                {
                    Mesh m = CreateClusterMesh(i);
                    GameObject go = new GameObject("Cluster_" + (i + 1).ToString("D2"));
                    go.AddComponent<MeshFilter>().sharedMesh = m;
                    go.AddComponent<MeshRenderer>().sharedMaterial = mat;
                    go.transform.SetParent(root, true);
                    go.transform.position = new Vector3(i * 3f, 0.8f, 0f);
                    go.transform.localScale = Vector3.one * PCGMeshFactory.ScaleForFootprint(m, PCGMeshFactory.RandomFootprint(_oreFootprintRange, new System.Random(_crystalSeed + i)));
                    placed++;
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            _lastMessage = "已在场景生成 " + placed + " 个对象（动态 mesh，未保存 Prefab）。";
            Repaint();
        }

        private void SaveRockPreset()
        {
            string name = string.IsNullOrWhiteSpace(_rockPresetName) ? "新岩石" : _rockPresetName.Trim();
            string dir = "Assets/Game/ScriptableAssets/PCG/Rock";
            EnsureFolder(dir, "Assets/Game/ScriptableAssets/PCG", "Rock");
            string path = dir + "/" + name + ".asset";

            RockPreset preset = AssetDatabase.LoadAssetAtPath<RockPreset>(path);
            if (preset == null)
            {
                preset = ScriptableObject.CreateInstance<RockPreset>();
                AssetDatabase.CreateAsset(preset, path);
            }
            preset.RockCount = _rockCount;
            preset.Subdivisions = _rockSubdivisions;
            preset.NoiseRange = _rockNoiseRange;
            preset.SquashRange = _rockSquashRange;
            preset.Material = _rockMaterial;
            preset.Color = _rockColor;
            preset.Metallic = _rockMetallic;
            preset.Smoothness = _rockSmoothness;
            preset.FootprintRange = _rockFootprintRange;
            preset.Seed = _rockSeed;
            EditorUtility.SetDirty(preset);
            AssetDatabase.SaveAssets();
            RefreshPresets();
            _lastMessage = "岩石参数已保存到 " + path;
            Repaint();
        }

        private void SaveCrystalPreset()
        {
            string name = string.IsNullOrWhiteSpace(_crystalPresetName) ? "新晶簇" : _crystalPresetName.Trim();
            string dir = "Assets/Game/ScriptableAssets/PCG/CrystalCluster";
            EnsureFolder(dir, "Assets/Game/ScriptableAssets/PCG", "CrystalCluster");
            string path = dir + "/" + name + ".asset";

            CrystalClusterPreset preset = AssetDatabase.LoadAssetAtPath<CrystalClusterPreset>(path);
            if (preset == null)
            {
                preset = ScriptableObject.CreateInstance<CrystalClusterPreset>();
                AssetDatabase.CreateAsset(preset, path);
            }
            preset.CrystalCount = _crystalCount;
            preset.CrystalSubdivisions = _crystalSubdivisions;
            preset.SpikeRatioRange = _spikeRatioRange;
            preset.SpikeLenRange = _spikeLenRange;
            preset.CrystalBaseRadiusRange = _crystalBaseRadiusRange;
            preset.CrystalMaterial = _crystalMaterial;
            preset.CrystalColor = _crystalColor;
            preset.CrystalMetallic = _crystalMetallic;
            preset.CrystalSmoothness = _crystalSmoothness;
            preset.CrystalFootprintRange = _crystalFootprintRange;
            preset.ClusterCount = 0;   // 纯晶簇
            preset.Seed = _crystalSeed;
            EditorUtility.SetDirty(preset);
            AssetDatabase.SaveAssets();
            RefreshPresets();
            _lastMessage = "晶簇参数已保存到 " + path;
            Repaint();
        }

        private void SaveOrePreset()
        {
            string name = string.IsNullOrWhiteSpace(_orePresetName) ? "新矿石簇" : _orePresetName.Trim();
            string dir = "Assets/Game/ScriptableAssets/PCG/CrystalCluster";
            EnsureFolder(dir, "Assets/Game/ScriptableAssets/PCG", "CrystalCluster");
            string path = dir + "/" + name + ".asset";

            CrystalClusterPreset preset = AssetDatabase.LoadAssetAtPath<CrystalClusterPreset>(path);
            if (preset == null)
            {
                preset = ScriptableObject.CreateInstance<CrystalClusterPreset>();
                AssetDatabase.CreateAsset(preset, path);
            }
            preset.CrystalCount = 0;   // 纯矿石簇
            preset.OreKind = _oreType;
            preset.ClusterCount = _clusterCount;
            preset.BlocksRange = _blocksRange;
            preset.BlockSizeRange = _blockSizeRange;
            preset.ClusterRadiusRange = _clusterRadiusRange;
            preset.OreMaterial = _oreMaterial;
            preset.OreColor = _oreColor;
            preset.OreOpacity = _oreOpacity;
            preset.OreMetallic = _oreMetallic;
            preset.OreSmoothness = _oreSmoothness;
            preset.OreFootprintRange = _oreFootprintRange;
            preset.Seed = _crystalSeed;
            EditorUtility.SetDirty(preset);
            AssetDatabase.SaveAssets();
            RefreshPresets();
            _lastMessage = "矿石簇参数已保存到 " + path;
            Repaint();
        }

        private void LoadRockPreset(RockPreset p)
        {
            _rockCount = p.RockCount;
            _rockSubdivisions = p.Subdivisions;
            _rockNoiseRange = p.NoiseRange;
            _rockSquashRange = p.SquashRange;
            _rockMaterial = p.Material;
            if (_rockMaterialEditor != null) { UnityEngine.Object.DestroyImmediate(_rockMaterialEditor); _rockMaterialEditor = null; }
            _rockColor = p.Color;
            _rockMetallic = p.Metallic;
            _rockSmoothness = p.Smoothness;
            _rockFootprintRange = p.FootprintRange;
            _rockSeed = p.Seed;
            _rockPresetName = p.name;
            _lastMessage = "已载入岩石预设「" + p.name + "」。";
            Repaint();
        }

        private void LoadCrystalPreset(CrystalClusterPreset p)
        {
            _crystalCount = p.CrystalCount;
            _crystalSubdivisions = p.CrystalSubdivisions;
            _spikeRatioRange = p.SpikeRatioRange;
            _spikeLenRange = p.SpikeLenRange;
            _crystalBaseRadiusRange = p.CrystalBaseRadiusRange;
            _crystalMaterial = p.CrystalMaterial;
            if (_crystalMaterialEditor != null) { UnityEngine.Object.DestroyImmediate(_crystalMaterialEditor); _crystalMaterialEditor = null; }
            _crystalColor = p.CrystalColor;
            _crystalMetallic = p.CrystalMetallic;
            _crystalSmoothness = p.CrystalSmoothness;
            _crystalFootprintRange = p.CrystalFootprintRange;
            _crystalSeed = p.Seed;
            _crystalPresetName = p.name;
            _lastMessage = "已载入晶簇预设「" + p.name + "」。";
            Repaint();
        }

        private void LoadOrePreset(CrystalClusterPreset p)
        {
            _clusterCount = p.ClusterCount;
            _oreType = p.OreKind;
            _blocksRange = p.BlocksRange;
            _blockSizeRange = p.BlockSizeRange;
            _clusterRadiusRange = p.ClusterRadiusRange;
            _oreMaterial = p.OreMaterial;
            if (_oreMaterialEditor != null) { UnityEngine.Object.DestroyImmediate(_oreMaterialEditor); _oreMaterialEditor = null; }
            _oreColor = p.OreColor;
            _oreOpacity = p.OreOpacity;
            _oreMetallic = p.OreMetallic;
            _oreSmoothness = p.OreSmoothness;
            _oreFootprintRange = p.OreFootprintRange;
            _crystalSeed = p.Seed;
            _orePresetName = p.name;
            _lastMessage = "已载入矿石簇预设「" + p.name + "」。";
            Repaint();
        }

        private static void EnsureFolder(string fullPath, string parent, string leaf)
        {
            if (AssetDatabase.IsValidFolder(fullPath))
            {
                return;
            }
            if (!AssetDatabase.IsValidFolder(parent))
            {
                string[] parts = parent.Split('/');
                string cur = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = cur + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                    {
                        AssetDatabase.CreateFolder(cur, parts[i]);
                    }
                    cur = next;
                }
            }
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
