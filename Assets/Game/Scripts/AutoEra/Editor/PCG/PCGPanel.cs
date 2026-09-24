using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AutoEra.Editor.PCG
{

    /// <summary>
    /// 统一 PCG 调试面板（ArtResource）：地形 / 散布 / 水体 / 资源点。
    /// - 散布支持按类型分层（草/花/灌木/树/岩石/晶簇），每层独立密度、间距、权重、种子；
    /// - 素材池「引用整个文件夹」自动加载 Prefab；
    /// - 支持 JSON 风格预设：配置列表按名称选择、一键应用 / 保存当前参数为配置。
    /// 用法：Tools/Auto Era Art/PCG Debug Panel
    /// </summary>
    /// <summary>生成对象的落位域：陆地上 / 水里。</summary>
    public enum PlacementDomain { Land, Water }

    public class PCGPanel : EditorWindow
    {
        private enum Tab { Terrain, Scatter, Water, Resources }

        private enum ScatterLayerType { Grass, Flower, Shrub, Tree, Rock }

        private static readonly string[] ScatterLayerTypeNames = { "草", "花", "灌木", "树", "岩石" };

        [Serializable]
        private class RockVariety
        {
            public RockPreset Preset;   // 岩石种类（资产生成的岩石预设）
            public float Weight = 1f;   // 比例权重
        }

        [Serializable]
        private class LayerEntry
        {
            public string DisplayName = "";          // 层名称（自定义标识）
            public ScatterLayerType Type = ScatterLayerType.Grass;
            public DefaultAsset Folder;              // 植株类：素材文件夹
            public List<RockVariety> RockVarieties = new List<RockVariety>();  // 岩石类：多种类 + 权重
            public float Density = 0.1f;             // 密度（株/㎡）
            public float MinDistance = 1.0f;         // 泊松盘最小间距
            public float Weight = 1.0f;              // 层权重（越大越常被选中）
            public int Seed = 20260921;
            public Vector2 ScaleRange = new Vector2(0.85f, 1.3f);
            public PlacementDomain Domain = PlacementDomain.Land;   // 陆地上 / 水里
        }

        [Serializable]
        private class TerrainConfig
        {
            public float Freq = 1.2f;
            public int Octaves = 3;
            public float RemapPow = 1.4f;
            public int Seed = 20260930;
            public int Res = 513;
            public int Size = 200;
            public float MaxHeight = 30f;
        }

        [Serializable]
        private class RockVarietyConfig
        {
            public string PresetPath;   // RockPreset 路径
            public float Weight = 1f;
        }

        [Serializable]
        private class LayerConfig
        {
            public string Name;          // displayName
            public int Type;             // ScatterLayerType 枚举值
            public string Folder;
            public float Density = 0.1f;
            public float MinDistance = 1.0f;
            public float Weight = 1.0f;
            public int Seed;
            public Vector2 ScaleRange = new Vector2(0.85f, 1.3f);
            public int Domain = 0;   // PlacementDomain 枚举值（0=Land 1=Water）
            public List<RockVarietyConfig> RockVarieties = new List<RockVarietyConfig>();
        }

        [Serializable]
        private class PCGConfig
        {
            public string Name;
            public TerrainConfig Terrain = new TerrainConfig();
            public List<LayerConfig> Layers = new List<LayerConfig>();
            public float WaterLevel = 9f;
            public bool AlignToSurface = true;
            public float MaxSlope = 40f;
        }

        private Tab _tab = Tab.Terrain;
        private Vector2 _scroll;
        private string _lastMessage = string.Empty;

        // ===== 地形参数 =====
        private TerrainConfig _terrain = new TerrainConfig();
        private float _warpAmount = 0.5f;      // 域扭曲强度（0=关）
        private int _terraceSteps = 0;         // 台地化级数（0/1=关）
        private int _surfaceTexSize = 512;

        // ===== 散布参数 =====
        private readonly List<LayerEntry> _layers = new List<LayerEntry>();
        private bool _alignToSurface = true;
        private float _maxSlope = 40f;

        // ===== 水体参数 =====
        private float _waterLevel = 9f;
        private Color _waterBaseColor = new Color(0.12f, 0.45f, 0.60f, 0.75f);
        private Color _waterDeepColor = new Color(0.03f, 0.18f, 0.32f, 0.92f);
        private float _waterWaveScale = 6f;
        private float _waterWaveSpeed = 0.4f;
        private float _waterWaveStrength = 0.45f;
        private float _waterHighlight = 0.6f;

        // ===== 资源点参数 =====
        private int _farmCount = 4;
        private int _forestCount = 5;
        private int _mineCount = 3;
        private float _farmFlatSlope = 5f;   // 农田平坦阈值（度，比地形起伏更严格，保证农田落在平缓区）
        private float _forestMaxSlope = 25f;
        private float _mineMinSlope = 20f;
        private int _resourceSeed = 20261015;
        private OreDistributionConfig _oreDistribution;   // 矿石概率配置（决定矿山矿物种类与概率）

        // ===== 资源点大小分级（占地 / 显示数量 / 间距） =====
        private float _smallFootprint = 3f;       // 小型占地边长（米）
        private float _mediumFootprint = 5f;      // 中型占地边长（米）
        private float _largeFootprint = 8f;       // 大型占地边长（米）
        private float _minPointSpacing = 12f;     // 资源点最小间距（米）
        private float _resourceEdgeMargin = 1f;   // 资源个体距占地边缘的最小距离（米）

        // 资源点实际占地区域（xz 平面矩形），散布装饰时排除这些区域
        private readonly List<Rect> _resourceZones = new List<Rect>();
        private float _scatterClearance = 2f;   // 散布装饰与资源点占地的净空距离（米，抵消贴地偏移/对象体积）
        private float _rockSinkMin = 0.1f;      // 岩石下沉下限（下表面至少埋入这么多，避免浮空；上限=中心贴地即半埋）

        // ===== 农田矩阵（小/中/大：m 行、n 列各自在 [min,max] 内随机） =====
        private Vector2Int _farmSmallRange = new Vector2Int(2, 4);
        private Vector2Int _farmMediumRange = new Vector2Int(4, 6);
        private Vector2Int _farmLargeRange = new Vector2Int(6, 8);
        private float _farmSpacing = 1f;   // 农田相邻田块间隔（米）

        // ===== 人工林树带（小/中/大：总树数范围） =====
        private Vector2Int _forestSmallCount = new Vector2Int(6, 10);
        private Vector2Int _forestMediumCount = new Vector2Int(14, 22);
        private Vector2Int _forestLargeCount = new Vector2Int(28, 42);
        private float _forestSpacing = 1f; // 人工林相邻树间隔（米）

        // ===== 矿山满显示矿石数（小/中/大；储量分档见 DEC-203：小100/中250/大500） =====
        private int _mineSmallDisplay = 6;
        private int _mineMediumDisplay = 8;
        private int _mineLargeDisplay = 10;

        private const float ResourceMargin = 8f;         // 资源点距地形边界的最小距离（米）

        // ===== JSON 配置 =====
        private const string ConfigDir = "Assets/Game/ScriptableAssets/PCGConfigs";
        private List<string> _configNames = new List<string>();
        private int _configIndex = -1;
        private string _newConfigName = "新风格";

        private const string ScatterRootName = "PCG_SCATTER_PREVIEW";

        private GameObject _scatterRoot;
        private GameObject _waterRoot;
        private GameObject _resourcePointsRoot;

        [MenuItem("Tools/Auto Era Art/PCG Debug Panel")]
        [MenuItem("Tools/Auto Era Art/PCG 调试面板")]
        public static void Open()
        {
            PCGPanel window = GetWindow<PCGPanel>("PCG 调试");
            window.minSize = new Vector2(480f, 640f);
            window.Show();
        }

        private void OnEnable()
        {
            InitDefaultLayers();
            RefreshConfigs();
        }

        private void InitDefaultLayers()
        {
            if (_layers.Count > 0)
            {
                return;
            }

            string[][] defaults = new string[][]
            {
                new[] { "草",   "Assets/Game/Prefabs/植株/草",    "0.12",  "0.7", "1.0" },
                new[] { "花",   "Assets/Game/Prefabs/植株/花",    "0.03",  "1.2", "0.5" },
                new[] { "灌木", "Assets/Game/Prefabs/植株/灌木",  "0.006", "2.8", "1.0" },
                new[] { "树",   "Assets/Game/Prefabs/植株/树",    "0.003", "5.0", "1.0" },
                new[] { "岩石", "Assets/Game/Prefabs/岩石",       "0.002", "5.0", "0.5" }
            };

            ScatterLayerType[] types =
            {
                ScatterLayerType.Grass, ScatterLayerType.Flower, ScatterLayerType.Shrub,
                ScatterLayerType.Tree, ScatterLayerType.Rock
            };

            for (int i = 0; i < defaults.Length; i++)
            {
                string[] d = defaults[i];
                LayerEntry entry = new LayerEntry();
                entry.DisplayName = d[0];
                entry.Type = types[i];
                entry.Folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(d[1]);
                entry.Density = float.Parse(d[2], System.Globalization.CultureInfo.InvariantCulture);
                entry.MinDistance = float.Parse(d[3], System.Globalization.CultureInfo.InvariantCulture);
                entry.Weight = float.Parse(d[4], System.Globalization.CultureInfo.InvariantCulture);
                entry.Seed = 20260921;

                if (entry.Type == ScatterLayerType.Rock)
                {
                    foreach (string path in FindAllPresetPaths<RockPreset>("Assets/Game/ScriptableAssets/PCG/Rock"))
                    {
                        RockPreset preset = AssetDatabase.LoadAssetAtPath<RockPreset>(path);
                        if (preset != null)
                        {
                            entry.RockVarieties.Add(new RockVariety { Preset = preset, Weight = 1f });
                        }
                    }
                }
                _layers.Add(entry);
            }
        }

        private static List<string> FindAllPresetPaths<T>(string folder) where T : UnityEngine.Object
        {
            List<string> result = new List<string>();
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { folder });
            foreach (string guid in guids)
            {
                result.Add(AssetDatabase.GUIDToAssetPath(guid));
            }
            return result;
        }

        private void OnGUI()
        {
            _tab = (Tab)GUILayout.Toolbar((int)_tab, new[] { "地形", "散布", "水体", "资源点" });

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            switch (_tab)
            {
                case Tab.Terrain: DrawTerrain(); break;
                case Tab.Scatter: DrawScatter(); break;
                case Tab.Water: DrawWater(); break;
                case Tab.Resources: DrawResources(); break;
            }

            if (!string.IsNullOrEmpty(_lastMessage))
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(_lastMessage, MessageType.None);
            }
            EditorGUILayout.EndScrollView();
        }

        // ==================== 地形 ====================

        private void DrawTerrain()
        {
            EditorGUILayout.LabelField("① 地形（噪声高度场 → Terrain）", EditorStyles.boldLabel);

            _terrain.Freq = EditorGUILayout.Slider("噪声频率（越大起伏越密集）", _terrain.Freq, 0.3f, 6f);
            _terrain.Octaves = EditorGUILayout.IntSlider("Octave 层数（细节）", _terrain.Octaves, 1, 6);
            _terrain.RemapPow = EditorGUILayout.Slider("重映射幂（>1 压低、拉开分布）", _terrain.RemapPow, 0.5f, 3f);
            _warpAmount = EditorGUILayout.Slider("域扭曲（地形蜿蜒程度）", _warpAmount, 0f, 2f);
            _terraceSteps = EditorGUILayout.IntSlider("台地化级数（0=关）", _terraceSteps, 0, 10);
            EditorGUILayout.BeginHorizontal();
            _terrain.Seed = EditorGUILayout.IntField("噪声种子", _terrain.Seed);
            if (GUILayout.Button("随机", GUILayout.Width(50f)))
            {
                _terrain.Seed = UnityEngine.Random.Range(0, 100000000);
            }
            EditorGUILayout.EndHorizontal();
            _terrain.Res = EditorGUILayout.IntPopup("高度图分辨率", _terrain.Res,
                new[] { "129", "257", "513", "1025" }, new[] { 129, 257, 513, 1025 });
            _terrain.Size = EditorGUILayout.IntField("地形边长（米）", _terrain.Size);
            _terrain.MaxHeight = EditorGUILayout.FloatField("最大高度（米）", _terrain.MaxHeight);

            EditorGUILayout.Space();
            if (GUILayout.Button("生成地形", GUILayout.Height(30f)))
            {
                GenerateTerrain();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("② 地表贴图（高程分带）", EditorStyles.boldLabel);
            _surfaceTexSize = EditorGUILayout.IntPopup("贴图尺寸", _surfaceTexSize,
                new[] { "256", "512", "1024" }, new[] { 256, 512, 1024 });

            if (GUILayout.Button("生成地表贴图（铺到地形）", GUILayout.Height(30f)))
            {
                GenerateSurface();
            }
        }

        private void GenerateTerrain()
        {
            Stopwatch sw = Stopwatch.StartNew();
            Scene scene = EditorSceneManager.GetActiveScene();

            // 删除旧地形，避免重复生成
            Terrain existing = UnityEngine.Object.FindObjectOfType<Terrain>();
            if (existing != null)
            {
                TerrainData oldData = existing.terrainData;
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
                if (oldData != null && !AssetDatabase.Contains(oldData))
                {
                    UnityEngine.Object.DestroyImmediate(oldData);
                }
            }

            TerrainData data = new TerrainData();
            data.heightmapResolution = _terrain.Res;
            data.size = new Vector3(_terrain.Size, _terrain.MaxHeight, _terrain.Size);
            Terrain.CreateTerrainGameObject(data).name = "PCG_Terrain";

            float[,] heights = new float[_terrain.Res, _terrain.Res];
            for (int y = 0; y < _terrain.Res; y++)
            {
                for (int x = 0; x < _terrain.Res; x++)
                {
                    heights[y, x] = Fbm((float)x / _terrain.Res, (float)y / _terrain.Res,
                        _terrain.Freq, _terrain.Octaves, _terrain.RemapPow, _terrain.Seed, _warpAmount, _terraceSteps);
                }
            }
            data.SetHeights(0, 0, heights);

            EditorSceneManager.MarkSceneDirty(scene);
            sw.Stop();
            _lastMessage = "地形已生成（res=" + _terrain.Res + "，freq=" + _terrain.Freq +
                          "，octaves=" + _terrain.Octaves + "，" + sw.ElapsedMilliseconds + " ms）";
            Repaint();
        }

        private void GenerateSurface()
        {
            Terrain t = UnityEngine.Object.FindObjectOfType<Terrain>();
            if (t == null)
            {
                _lastMessage = "场景里没有 Terrain，请先生成地形。";
                return;
            }

            int size = _surfaceTexSize;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGB24, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float h = Fbm((float)x / size, (float)y / size,
                        _terrain.Freq, _terrain.Octaves, _terrain.RemapPow, _terrain.Seed, _warpAmount, _terraceSteps);
                    tex.SetPixel(x, y, SurfaceColor(h));
                }
            }
            tex.Apply();

            string texPath = "Assets/Game/Textures/PCG/PCG_Surface_Albedo.png";
            string layerPath = "Assets/Game/Materials/PCG/PCG_Surface.terrainlayer";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(texPath)));
            System.IO.File.WriteAllBytes(System.IO.Path.GetFullPath(texPath), tex.EncodeToPNG());
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);

            if (!AssetDatabase.IsValidFolder("Assets/Game/Materials/PCG"))
            {
                AssetDatabase.CreateFolder("Assets/Game/Materials", "PCG");
            }
            TerrainLayer layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(layerPath);
            if (layer == null)
            {
                layer = new TerrainLayer();
                AssetDatabase.CreateAsset(layer, layerPath);
            }
            layer.diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            layer.tileSize = new Vector2(_terrain.Size, _terrain.Size);
            EditorUtility.SetDirty(layer);
            AssetDatabase.SaveAssets();
            t.terrainData.terrainLayers = new TerrainLayer[] { layer };
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            _lastMessage = "地表贴图已生成并铺到地形（" + size + "×" + size + "）。";
            Repaint();
        }

        // ==================== 散布 ====================

        private void DrawScatter()
        {
            EditorGUILayout.LabelField("③ 散布（按类型分层）", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("每层＝一种类型：植株类引用素材文件夹，岩石类按多种类 + 权重动态生成；晶簇/矿石簇在「资源点」里配置。", MessageType.Info);

            for (int i = 0; i < _layers.Count; i++)
            {
                LayerEntry layer = _layers[i];
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();
                layer.DisplayName = EditorGUILayout.TextField("名称", layer.DisplayName);
                layer.Type = (ScatterLayerType)EditorGUILayout.Popup("类型", (int)layer.Type, ScatterLayerTypeNames);
                if (GUILayout.Button("×", GUILayout.Width(22f)))
                {
                    _layers.RemoveAt(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                EditorGUILayout.EndHorizontal();

                if (layer.Type == ScatterLayerType.Rock)
                {
                    DrawRockVarieties(layer);
                }
                else
                {
                    layer.Folder = (DefaultAsset)EditorGUILayout.ObjectField("素材文件夹", layer.Folder, typeof(DefaultAsset), false);
                }

                layer.Density = EditorGUILayout.FloatField("密度（株/㎡）", layer.Density);
                layer.MinDistance = EditorGUILayout.FloatField("间距（米）", layer.MinDistance);
                layer.Weight = EditorGUILayout.FloatField("权重（层占比）", layer.Weight);

                EditorGUILayout.BeginHorizontal();
                layer.Seed = EditorGUILayout.IntField("种子", layer.Seed);
                if (GUILayout.Button("随机", GUILayout.Width(50f)))
                {
                    layer.Seed = UnityEngine.Random.Range(0, 100000000);
                }
                EditorGUILayout.EndHorizontal();
                layer.ScaleRange = EditorGUILayout.Vector2Field("缩放范围", layer.ScaleRange);
                layer.Domain = (PlacementDomain)EditorGUILayout.Popup("落位域", (int)layer.Domain, new[] { "陆地上", "水里" });

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("+ 添加一层"))
            {
                _layers.Add(new LayerEntry());
            }

            EditorGUILayout.Space();
            _alignToSurface = EditorGUILayout.Toggle("对齐地表法线（贴合坡面）", _alignToSurface);
            _maxSlope = EditorGUILayout.Slider("最大坡度（度，超过不散布）", _maxSlope, 5f, 60f);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("散布（贴到地形）", GUILayout.Height(30f)))
            {
                Scatter();
            }
            if (GUILayout.Button("清除预览", GUILayout.Height(30f)))
            {
                if (_scatterRoot != null)
                {
                    Undo.DestroyObjectImmediate(_scatterRoot);
                }
            }
            EditorGUILayout.EndHorizontal();

            DrawConfigSection();
        }

        private void DrawRockVarieties(LayerEntry layer)
        {
            EditorGUILayout.LabelField("岩石种类（比例权重）", EditorStyles.boldLabel);
            for (int j = 0; j < layer.RockVarieties.Count; j++)
            {
                RockVariety v = layer.RockVarieties[j];
                EditorGUILayout.BeginHorizontal();
                v.Preset = (RockPreset)EditorGUILayout.ObjectField(v.Preset, typeof(RockPreset), false);
                v.Weight = EditorGUILayout.FloatField("权重", v.Weight, GUILayout.Width(90f));
                if (GUILayout.Button("×", GUILayout.Width(22f)))
                {
                    layer.RockVarieties.RemoveAt(j);
                    EditorGUILayout.EndHorizontal();
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("+ 添加岩石种类"))
            {
                layer.RockVarieties.Add(new RockVariety());
            }
        }

        private void Scatter()
        {
            Stopwatch sw = Stopwatch.StartNew();
            Terrain terrain = UnityEngine.Object.FindObjectOfType<Terrain>();
            if (terrain == null)
            {
                _lastMessage = "场景里没有 Terrain，请先生成地形。";
                return;
            }

            if (_scatterRoot != null)
            {
                Undo.DestroyObjectImmediate(_scatterRoot);
            }
            Transform root = new GameObject(ScatterRootName).transform;
            _scatterRoot = root.gameObject;
            Rect area = new Rect(0f, 0f, terrain.terrainData.size.x, terrain.terrainData.size.z);
            int total = 0;

            // 跨层互斥记录：岩石位置（先放岩石占位，树/灌木后放避开岩石）
            List<Vector2> rockPos = new List<Vector2>();
            List<float> rockRadius = new List<float>();

            void PlaceLayer(LayerEntry layer)
            {
                bool isRock = layer.Type == ScatterLayerType.Rock;
                List<GameObject> prefabs = null;
                if (isRock)
                {
                    if (!HasValidRockVariety(layer))
                    {
                        return;
                    }
                }
                else
                {
                    prefabs = LoadPrefabsFromFolder(layer.Folder);
                    if (prefabs.Count == 0)
                    {
                        return;
                    }
                }

                int target = Mathf.Clamp(
                    Mathf.RoundToInt(layer.Density * area.width * area.height), 1, 30000);
                List<Vector2> points = PoissonDisk(area.width, area.height, layer.MinDistance, layer.Seed, 30000);
                System.Random rng = new System.Random(layer.Seed ^ 0x5F3759DF);
                if (points.Count > target)
                {
                    for (int i = points.Count - 1; i > 0; i--)
                    {
                        int j = rng.Next(i + 1);
                        Vector2 tmp = points[i];
                        points[i] = points[j];
                        points[j] = tmp;
                    }
                    points = points.GetRange(0, target);
                }

                // 层内排斥记录（每层独立，同类对象之间不重叠）
                List<Vector2> placedPos = new List<Vector2>();
                List<float> placedRadius = new List<float>();

                foreach (Vector2 point in points)
                {
                    Vector3 world = new Vector3(area.x + point.x, 0f, area.y + point.y);
                    float h = terrain.SampleHeight(world);
                    // 按落位域过滤：陆地对象跳过水域，水里对象跳过陆地
                    if (layer.Domain == PlacementDomain.Land && h < _waterLevel)
                    {
                        continue;
                    }
                    if (layer.Domain == PlacementDomain.Water && h >= _waterLevel)
                    {
                        continue;
                    }
                    world.y = h;

                    Vector3 normal = Vector3.up;
                    if (_alignToSurface)
                    {
                        normal = GetTerrainNormal(terrain, world);
                        if (Vector3.Angle(Vector3.up, normal) > _maxSlope)
                        {
                            continue;
                        }
                    }

                    Quaternion rotation = _alignToSurface
                        ? Quaternion.FromToRotation(Vector3.up, normal) * Quaternion.Euler(0f, rng.Next(360), 0f)
                        : Quaternion.Euler(0f, rng.Next(360), 0f);

                    // 创建/实例化对象并设置 transform，再读实际包围盒半径做排斥校验
                    GameObject go;
                    float scale;
                    float bottomOffset;

                    if (isRock)
                    {
                        RockPreset rockPreset = PickWeightedRock(layer.RockVarieties, rng);
                        if (rockPreset == null)
                        {
                            continue;
                        }
                        int variant = rng.Next(Mathf.Max(1, rockPreset.RockCount));
                        Mesh rockMesh = rockPreset.CreateRockMesh(variant);
                        float footprint = PCGMeshFactory.RandomFootprint(rockPreset.FootprintRange, rng);
                        scale = PCGMeshFactory.ScaleForFootprint(rockMesh, footprint);
                        bottomOffset = rockMesh.bounds.extents.y - rockMesh.bounds.center.y;
                        // 下沉：下表面从「埋入 _rockSinkMin」到「中心贴地（半埋）」随机，丰富岩石姿态
                        float sinkMax = rockMesh.bounds.extents.y;
                        float sink = UnityEngine.Random.Range(Mathf.Min(_rockSinkMin, sinkMax), sinkMax);
                        bottomOffset -= sink;
                        Material mat = ResolveMaterial(rockPreset.Material, rockPreset.Color, 1f, rockPreset.Smoothness, rockPreset.Metallic);
                        go = new GameObject(layer.DisplayName + "_" + (total + 1));
                        go.AddComponent<MeshFilter>().sharedMesh = rockMesh;
                        go.AddComponent<MeshRenderer>().sharedMaterial = mat;
                        go.AddComponent<MeshCollider>().sharedMesh = rockMesh;
                    }
                    else
                    {
                        GameObject prefab = prefabs[rng.Next(prefabs.Count)];
                        scale = UnityEngine.Random.Range(layer.ScaleRange.x, layer.ScaleRange.y);
                        bottomOffset = GetPrefabBottomOffset(prefab);
                        go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    }

                    Vector3 actualPos = new Vector3(world.x, h, world.z) + normal * (bottomOffset * scale);
                    go.transform.position = actualPos;
                    go.transform.localScale = Vector3.one * scale;
                    go.transform.rotation = rotation;

                    // 占地半径：树木/灌木读碰撞体，岩石回退 renderer 包围盒（含缩放与旋转）
                    float radius = GetOccupancyRadius(go, out bool hasCollider);
                    bool isWoody = !isRock && hasCollider;

                    // 资源点过滤：仅岩石/灌木/树木避开资源点，花草可进资源点
                    if ((isRock || isWoody) && InsideResourceZone(world.x, world.z))
                    {
                        UnityEngine.Object.DestroyImmediate(go);
                        continue;
                    }

                    // 排斥：仅岩石/灌木/树木参与，花草不排斥
                    if (isRock || isWoody)
                    {
                        // 层内排斥：与同类对象实际中心距离 >= 半径之和
                        bool overlap = OverlapAny(placedPos, placedRadius, actualPos.x, actualPos.z, radius);
                        // 跨层排斥：树/灌木避开已放置的岩石
                        if (!overlap && isWoody)
                        {
                            overlap = OverlapAny(rockPos, rockRadius, actualPos.x, actualPos.z, radius);
                        }
                        if (overlap)
                        {
                            UnityEngine.Object.DestroyImmediate(go);
                            continue;
                        }
                    }

                    Undo.RegisterCreatedObjectUndo(go, "PCG Scatter");
                    go.transform.SetParent(root, false);

                    if (isRock || isWoody)
                    {
                        placedPos.Add(new Vector2(actualPos.x, actualPos.z));
                        placedRadius.Add(radius);
                        if (isRock)
                        {
                            rockPos.Add(new Vector2(actualPos.x, actualPos.z));
                            rockRadius.Add(radius);
                        }
                    }
                    total++;
                }
            }

            // 第一遍：岩石（先占位，保证岩石数量不被树/灌木挤没）
            foreach (LayerEntry layer in _layers)
            {
                if (layer.Type == ScatterLayerType.Rock)
                {
                    PlaceLayer(layer);
                }
            }
            // 第二遍：树/灌木/草（树/灌木避开岩石，花草不参与排斥）
            foreach (LayerEntry layer in _layers)
            {
                if (layer.Type != ScatterLayerType.Rock)
                {
                    PlaceLayer(layer);
                }
            }

            sw.Stop();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            _lastMessage = "已散布 " + total + " 个实例（" + sw.ElapsedMilliseconds + " ms）";
            Repaint();
        }

        /// <summary>判断某位置是否与列表中的任一对象体积重叠（距离 &lt; 半径和）。</summary>
        private static bool OverlapAny(List<Vector2> positions, List<float> radii, float x, float z, float r)
        {
            for (int i = 0; i < positions.Count; i++)
            {
                float dx = x - positions[i].x;
                float dz = z - positions[i].y;
                float min = r + radii[i];
                if (dx * dx + dz * dz < min * min)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>实例化对象水平占地半径：优先碰撞体（树木/灌木），无碰撞体时回退 renderer 包围盒（岩石）。</summary>
        private static float GetOccupancyRadius(GameObject instance, out bool hasCollider)
        {
            hasCollider = false;
            float maxHalf = 0f;
            Collider[] colliders = instance.GetComponentsInChildren<Collider>();
            foreach (Collider c in colliders)
            {
                if (c == null) continue;
                hasCollider = true;
                Bounds b = c.bounds;
                maxHalf = Mathf.Max(maxHalf, Mathf.Max(b.size.x, b.size.z) * 0.5f);
            }
            if (hasCollider)
            {
                return maxHalf > 0f ? maxHalf : 0.5f;
            }
            Renderer[] renderers = null;
            LODGroup lodGroup = instance.GetComponentInChildren<LODGroup>();
            if (lodGroup != null && lodGroup.GetLODs().Length > 0)
            {
                renderers = lodGroup.GetLODs()[0].renderers;
            }
            if (renderers == null || renderers.Length == 0)
            {
                renderers = instance.GetComponentsInChildren<Renderer>();
            }
            if (renderers != null)
            {
                foreach (Renderer r in renderers)
                {
                    if (r == null) continue;
                    maxHalf = Mathf.Max(maxHalf, Mathf.Max(r.bounds.size.x, r.bounds.size.z) * 0.5f);
                }
            }
            return maxHalf > 0f ? maxHalf : 0.5f;
        }

        private static bool HasValidRockVariety(LayerEntry layer)
        {
            foreach (RockVariety v in layer.RockVarieties)
            {
                if (v.Preset != null && v.Weight > 0f)
                {
                    return true;
                }
            }
            return false;
        }

        private static RockPreset PickWeightedRock(List<RockVariety> varieties, System.Random rng)
        {
            float total = 0f;
            foreach (RockVariety v in varieties)
            {
                if (v.Preset != null && v.Weight > 0f)
                {
                    total += v.Weight;
                }
            }
            if (total <= 0f)
            {
                return null;
            }
            float roll = (float)rng.NextDouble() * total;
            foreach (RockVariety v in varieties)
            {
                if (v.Preset == null || v.Weight <= 0f)
                {
                    continue;
                }
                roll -= v.Weight;
                if (roll <= 0f)
                {
                    return v.Preset;
                }
            }
            foreach (RockVariety v in varieties)
            {
                if (v.Preset != null)
                {
                    return v.Preset;
                }
            }
            return null;
        }

        private static Material CreateScatterMaterial(Color color, float opacity, float smoothness, float metallic)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(color.r, color.g, color.b, opacity);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", metallic);
            if (opacity < 0.999f)
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
            return mat;
        }

        /// <summary>优先用预设引用的材质资产；为空则按内置颜色/透明度/平滑度/金属度生成临时材质。</summary>
        private static Material ResolveMaterial(Material asset, Color color, float opacity, float smoothness, float metallic)
        {
            if (asset != null)
            {
                return asset;
            }
            return CreateScatterMaterial(color, opacity, smoothness, metallic);
        }

        // ==================== JSON 配置 ====================

        private void DrawConfigSection()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("风格预设（JSON）", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("配置文件在 " + ConfigDir + "，选择后一键应用；也可把当前参数保存为新风格。", MessageType.None);

            EditorGUILayout.BeginHorizontal();
            if (_configNames.Count == 0)
            {
                EditorGUILayout.LabelField("（无配置，请先保存或手动创建 JSON）");
            }
            else
            {
                _configIndex = EditorGUILayout.Popup("选择配置", _configIndex, _configNames.ToArray());
            }
            if (GUILayout.Button("刷新", GUILayout.Width(50f)))
            {
                RefreshConfigs();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("应用选中配置", GUILayout.Height(26f)))
            {
                ApplySelectedConfig();
            }
            if (GUILayout.Button("保存当前为配置", GUILayout.Height(26f)))
            {
                SaveCurrentConfig();
            }
            EditorGUILayout.EndHorizontal();

            _newConfigName = EditorGUILayout.TextField("新配置名", _newConfigName);
        }

        private void RefreshConfigs()
        {
            _configNames.Clear();
            _configIndex = -1;
            if (!AssetDatabase.IsValidFolder(ConfigDir))
            {
                return;
            }
            string[] guids = AssetDatabase.FindAssets("t:TextAsset", new[] { ConfigDir });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    _configNames.Add(System.IO.Path.GetFileNameWithoutExtension(path));
                }
            }
            _configNames.Sort();
            if (_configNames.Count > 0)
            {
                _configIndex = 0;
            }
        }

        private void ApplySelectedConfig()
        {
            if (_configIndex < 0 || _configIndex >= _configNames.Count)
            {
                _lastMessage = "没有可应用的配置。";
                return;
            }
            ApplyConfigFile(_configNames[_configIndex]);
        }

        private void ApplyConfigFile(string name)
        {
            string path = ConfigDir + "/" + name + ".json";
            string full = System.IO.Path.GetFullPath(path);
            if (!System.IO.File.Exists(full))
            {
                _lastMessage = "配置文件不存在：" + path;
                return;
            }

            string json = System.IO.File.ReadAllText(full);
            PCGConfig config = JsonUtility.FromJson<PCGConfig>(json);
            if (config == null)
            {
                _lastMessage = "配置解析失败：" + name;
                return;
            }

            _terrain = config.Terrain ?? new TerrainConfig();
            _waterLevel = config.WaterLevel;
            _alignToSurface = config.AlignToSurface;
            _maxSlope = config.MaxSlope;

            _layers.Clear();
            foreach (LayerConfig lc in config.Layers)
            {
                LayerEntry entry = new LayerEntry();
                entry.DisplayName = lc.Name;
                entry.Type = (ScatterLayerType)lc.Type;
                entry.Folder = string.IsNullOrEmpty(lc.Folder)
                    ? null
                    : AssetDatabase.LoadAssetAtPath<DefaultAsset>(lc.Folder);
                entry.Density = lc.Density;
                entry.MinDistance = lc.MinDistance;
                entry.Weight = lc.Weight;
                entry.Seed = lc.Seed;
                entry.ScaleRange = lc.ScaleRange;
                entry.Domain = (PlacementDomain)lc.Domain;
                if (lc.RockVarieties != null)
                {
                    foreach (RockVarietyConfig rvc in lc.RockVarieties)
                    {
                        RockPreset preset = string.IsNullOrEmpty(rvc.PresetPath)
                            ? null
                            : AssetDatabase.LoadAssetAtPath<RockPreset>(rvc.PresetPath);
                        if (preset != null)
                        {
                            entry.RockVarieties.Add(new RockVariety { Preset = preset, Weight = rvc.Weight });
                        }
                    }
                }
                _layers.Add(entry);
            }

            _lastMessage = "已应用配置「" + name + "」（" + _layers.Count + " 层）。";
            Repaint();
        }

        private void SaveCurrentConfig()
        {
            string name = string.IsNullOrWhiteSpace(_newConfigName) ? "新风格" : _newConfigName.Trim();
            PCGConfig config = new PCGConfig();
            config.Name = name;
            config.Terrain = _terrain;
            config.WaterLevel = _waterLevel;
            config.AlignToSurface = _alignToSurface;
            config.MaxSlope = _maxSlope;

            foreach (LayerEntry layer in _layers)
            {
                LayerConfig lc = new LayerConfig();
                lc.Name = layer.DisplayName;
                lc.Type = (int)layer.Type;
                lc.Folder = layer.Folder == null ? "" : AssetDatabase.GetAssetPath(layer.Folder);
                lc.Density = layer.Density;
                lc.MinDistance = layer.MinDistance;
                lc.Weight = layer.Weight;
                lc.Seed = layer.Seed;
                lc.ScaleRange = layer.ScaleRange;
                lc.Domain = (int)layer.Domain;
                foreach (RockVariety v in layer.RockVarieties)
                {
                    if (v.Preset != null)
                    {
                        lc.RockVarieties.Add(new RockVarietyConfig
                        {
                            PresetPath = AssetDatabase.GetAssetPath(v.Preset),
                            Weight = v.Weight
                        });
                    }
                }
                config.Layers.Add(lc);
            }

            if (!AssetDatabase.IsValidFolder(ConfigDir))
            {
                string parent = "Assets/Game";
                string leaf = "PCGConfigs";
                if (!AssetDatabase.IsValidFolder(parent))
                {
                    AssetDatabase.CreateFolder("Assets", "Game");
                }
                AssetDatabase.CreateFolder(parent, leaf);
            }

            string json = JsonUtility.ToJson(config, true);
            string path = ConfigDir + "/" + name + ".json";
            System.IO.File.WriteAllText(System.IO.Path.GetFullPath(path), json);
            AssetDatabase.Refresh();
            RefreshConfigs();
            _lastMessage = "已保存配置「" + name + "」到 " + path;
            Repaint();
        }

        // ==================== 水体 ====================

        private void DrawWater()
        {
            EditorGUILayout.LabelField("④ 水体（程序化波浪）", EditorStyles.boldLabel);
            _waterLevel = EditorGUILayout.Slider("水面高度（米）", _waterLevel, 0f, 50f);
            _waterBaseColor = EditorGUILayout.ColorField("浅水色", _waterBaseColor);
            _waterDeepColor = EditorGUILayout.ColorField("深水色", _waterDeepColor);
            _waterWaveScale = EditorGUILayout.Slider("波浪尺度", _waterWaveScale, 1f, 20f);
            _waterWaveSpeed = EditorGUILayout.Slider("流动速度", _waterWaveSpeed, 0f, 2f);
            _waterWaveStrength = EditorGUILayout.Slider("波浪强度", _waterWaveStrength, 0f, 1f);
            _waterHighlight = EditorGUILayout.Slider("波峰高光", _waterHighlight, 0f, 2f);

            if (GUILayout.Button("生成水体（程序化波浪）", GUILayout.Height(30f)))
            {
                GenerateWater();
            }
        }

        private void GenerateWater()
        {
            Terrain terrain = UnityEngine.Object.FindObjectOfType<Terrain>();
            if (terrain == null)
            {
                _lastMessage = "场景里没有 Terrain。";
                return;
            }

            if (_waterRoot != null)
            {
                Undo.DestroyObjectImmediate(_waterRoot);
            }

            GameObject waterGo = GameObject.CreatePrimitive(PrimitiveType.Plane);
            waterGo.name = "PCG_Water";
            _waterRoot = waterGo;
            float size = terrain.terrainData.size.x;
            waterGo.transform.position = new Vector3(size * 0.5f, _waterLevel, size * 0.5f);
            waterGo.transform.localScale = new Vector3(size / 10f, 1f, size / 10f);

            Shader waterShader = Shader.Find("PCG/Water");
            if (waterShader == null)
            {
                _lastMessage = "找不到 PCG/Water shader。";
                return;
            }
            Material waterMat = new Material(waterShader);
            waterMat.SetColor("_BaseColor", _waterBaseColor);
            waterMat.SetColor("_DeepColor", _waterDeepColor);
            waterMat.SetFloat("_WaveScale", _waterWaveScale);
            waterMat.SetFloat("_WaveSpeed", _waterWaveSpeed);
            waterMat.SetFloat("_WaveStrength", _waterWaveStrength);
            waterMat.SetFloat("_Highlight", _waterHighlight);
            waterGo.GetComponent<Renderer>().sharedMaterial = waterMat;
            UnityEngine.Object.DestroyImmediate(waterGo.GetComponent<Collider>());

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            _lastMessage = "水体已生成（水面高度 " + _waterLevel + " 米）。";
            Repaint();
        }

        private void DrawResources()
        {
            EditorGUILayout.LabelField("⑤ 资源点 + 初始基地（模拟正式流程落位）", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("按地形条件自动落位：农田=平地、人工林=缓坡、矿山=山地，初始基地=中心平地。生成色块占位（可后续替换正式模型）。", MessageType.Info);

            _farmCount = EditorGUILayout.IntSlider("农田数量", _farmCount, 0, 20);
            _forestCount = EditorGUILayout.IntSlider("人工林数量", _forestCount, 0, 20);
            _mineCount = EditorGUILayout.IntSlider("矿山数量", _mineCount, 0, 20);
            EditorGUILayout.Space();
            _farmFlatSlope = EditorGUILayout.Slider("农田平坦阈值（度）", _farmFlatSlope, 1f, 15f);
            _forestMaxSlope = EditorGUILayout.Slider("人工林最大坡度（度）", _forestMaxSlope, 5f, 45f);
            _mineMinSlope = EditorGUILayout.Slider("矿山最小坡度（度）", _mineMinSlope, 10f, 60f);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("资源点大小分级", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("农田=m×n矩阵；人工林=直角树带；矿山=满显示矿石数（储量见 DEC-203）。三者独立配置。", MessageType.None);
            _smallFootprint = EditorGUILayout.Slider("小型占地边长（米）", _smallFootprint, 2f, 6f);
            _mediumFootprint = EditorGUILayout.Slider("中型占地边长（米）", _mediumFootprint, 3f, 8f);
            _largeFootprint = EditorGUILayout.Slider("大型占地边长（米）", _largeFootprint, 5f, 12f);
            _minPointSpacing = EditorGUILayout.Slider("资源点最小间距（米）", _minPointSpacing, 6f, 20f);
            _resourceEdgeMargin = EditorGUILayout.Slider("资源距占地边缘（米）", _resourceEdgeMargin, 0.2f, 2f);
            _scatterClearance = EditorGUILayout.Slider("散布净空（米，避开资源点）", _scatterClearance, 0f, 5f);
            _rockSinkMin = EditorGUILayout.Slider("岩石下沉下限（米，下表面埋入）", _rockSinkMin, 0f, 1f);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("农田矩阵（m×n，行/列各自在范围内随机）", EditorStyles.boldLabel);
            _farmSmallRange = EditorGUILayout.Vector2IntField("小（行/列范围）", _farmSmallRange);
            _farmMediumRange = EditorGUILayout.Vector2IntField("中（行/列范围）", _farmMediumRange);
            _farmLargeRange = EditorGUILayout.Vector2IntField("大（行/列范围）", _farmLargeRange);
            _farmSpacing = EditorGUILayout.Slider("农田相邻田块间隔（米）", _farmSpacing, 0.5f, 4f);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("人工林树带（总树数范围）", EditorStyles.boldLabel);
            _forestSmallCount = EditorGUILayout.Vector2IntField("小（树数范围）", _forestSmallCount);
            _forestMediumCount = EditorGUILayout.Vector2IntField("中（树数范围）", _forestMediumCount);
            _forestLargeCount = EditorGUILayout.Vector2IntField("大（树数范围）", _forestLargeCount);
            _forestSpacing = EditorGUILayout.Slider("人工林相邻树间隔（米）", _forestSpacing, 0.5f, 4f);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("矿山满显示矿石数（小/中/大）", EditorStyles.boldLabel);
            _mineSmallDisplay = EditorGUILayout.IntField("小", _mineSmallDisplay);
            _mineMediumDisplay = EditorGUILayout.IntField("中", _mineMediumDisplay);
            _mineLargeDisplay = EditorGUILayout.IntField("大", _mineLargeDisplay);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("矿山矿石（按概率随机）", EditorStyles.boldLabel);
            _oreDistribution = (OreDistributionConfig)EditorGUILayout.ObjectField(
                "矿石概率配置", _oreDistribution, typeof(OreDistributionConfig), false);
            if (_oreDistribution == null)
            {
                EditorGUILayout.HelpBox("未指定矿石概率配置：请用 Tools/Auto Era Art/创建矿石概率配置 生成默认配置并拖入。", MessageType.Warning);
            }
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            _resourceSeed = EditorGUILayout.IntField("种子", _resourceSeed);
            if (GUILayout.Button("随机", GUILayout.Width(50f)))
            {
                _resourceSeed = UnityEngine.Random.Range(0, 100000000);
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("一键生成（资源点 → 散布装饰）", GUILayout.Height(30f)))
            {
                GenerateResources();
                Scatter();
            }
            if (GUILayout.Button("生成资源点 + 初始基地", GUILayout.Height(30f)))
            {
                GenerateResources();
            }
            if (GUILayout.Button("清除资源点", GUILayout.Height(24f)))
            {
                if (_resourcePointsRoot != null)
                {
                    Undo.DestroyObjectImmediate(_resourcePointsRoot);
                }
            }
        }

        private void GenerateResources()
        {
            Stopwatch sw = Stopwatch.StartNew();
            Terrain terrain = UnityEngine.Object.FindObjectOfType<Terrain>();
            if (terrain == null)
            {
                _lastMessage = "场景里没有 Terrain，请先生成地形。";
                return;
            }

            if (_resourcePointsRoot != null)
            {
                Undo.DestroyObjectImmediate(_resourcePointsRoot);
            }
            _resourceZones.Clear();
            Transform root = new GameObject("PCG_RESOURCE_POINTS").transform;
            _resourcePointsRoot = root.gameObject;

            Vector3 size = terrain.terrainData.size;
            System.Random rng = new System.Random(_resourceSeed);

            List<Vector3> farmSpots = new List<Vector3>();
            List<Vector3> forestSpots = new List<Vector3>();
            List<Vector3> mineSpots = new List<Vector3>();

            int samples = 3000;
            for (int i = 0; i < samples; i++)
            {
                float x = ResourceMargin + (float)rng.NextDouble() * (size.x - ResourceMargin * 2f);
                float z = ResourceMargin + (float)rng.NextDouble() * (size.z - ResourceMargin * 2f);
                Vector3 world = new Vector3(x, 0f, z);
                float h = terrain.SampleHeight(world);
                if (h < _waterLevel)
                {
                    continue;
                }
                world.y = h;
                Vector3 normal = GetTerrainNormal(terrain, world);
                float slope = Vector3.Angle(Vector3.up, normal);

                if (slope <= _farmFlatSlope)
                {
                    farmSpots.Add(world);
                }
                if (slope <= _forestMaxSlope && slope > _farmFlatSlope)
                {
                    forestSpots.Add(world);
                }
                if (slope >= _mineMinSlope)
                {
                    mineSpots.Add(world);
                }
            }

            Shuffle(farmSpots, rng);
            Shuffle(forestSpots, rng);
            Shuffle(mineSpots, rng);

            // ===== 第一层：选取资源点（最小间距 + 大小分级） =====
            List<ResourcePoint> farmPoints = SelectPoints(farmSpots, _farmCount, rng);
            List<ResourcePoint> forestPoints = SelectPoints(forestSpots, _forestCount, rng);
            List<ResourcePoint> minePoints = SelectPoints(mineSpots, _mineCount, rng);

            Color farmColor = new Color(0.30f, 0.70f, 0.30f, 0.7f);
            Color forestColor = new Color(0.12f, 0.42f, 0.18f, 0.7f);
            Color baseColor = new Color(0.30f, 0.50f, 0.90f, 0.8f);

            // ===== 第二层：挨个为每个资源点生成其中的资源（在占地范围内） =====
            int placed = 0;
            placed += PlaceFarmMatrix(terrain, root, farmPoints, farmColor, rng);
            placed += PlaceForestBelt(terrain, root, forestPoints, forestColor, rng);
            placed += PlaceMinePoints(terrain, root, minePoints, rng);

            Vector3 basePos = FindBaseSpot(terrain, size);
            PlaceBaseSpot(root, basePos, baseColor);

            sw.Stop();
            _lastMessage = "已生成 " + placed + " 资源点 + 1 初始基地（" + sw.ElapsedMilliseconds + " ms）";
            Repaint();
        }

        private enum ResourcePointSize { Small, Medium, Large }

        private class ResourcePoint
        {
            public Vector3 Center;
            public ResourcePointSize Size;
            public float Footprint;
            public int DisplayCount;
            public PlacementDomain Domain = PlacementDomain.Land;   // 目前资源点都在陆地
        }

        /// <summary>资源点大小分配：小 50% / 中 30% / 大 20%。</summary>
        private static ResourcePointSize PickPointSize(System.Random rng)
        {
            double roll = rng.NextDouble();
            if (roll < 0.5) return ResourcePointSize.Small;
            if (roll < 0.8) return ResourcePointSize.Medium;
            return ResourcePointSize.Large;
        }

        private float GetFootprint(ResourcePointSize size)
        {
            switch (size)
            {
                case ResourcePointSize.Small: return _smallFootprint;
                case ResourcePointSize.Medium: return _mediumFootprint;
                default: return _largeFootprint;
            }
        }

        /// <summary>矿山满显示矿石数（固定 小6/中8/大10；储量分档见 DEC-203）。</summary>
        private int GetMineDisplayCount(ResourcePointSize size)
        {
            switch (size)
            {
                case ResourcePointSize.Small: return _mineSmallDisplay;
                case ResourcePointSize.Medium: return _mineMediumDisplay;
                default: return _mineLargeDisplay;
            }
        }

        /// <summary>第一层：从候选中选取资源点，保证最小间距，并分配大小/占地/显示数量。</summary>
        private List<ResourcePoint> SelectPoints(List<Vector3> candidates, int count, System.Random rng)
        {
            List<ResourcePoint> result = new List<ResourcePoint>();
            float minSq = _minPointSpacing * _minPointSpacing;
            foreach (Vector3 c in candidates)
            {
                if (result.Count >= count)
                {
                    break;
                }
                bool tooClose = false;
                for (int i = 0; i < result.Count; i++)
                {
                    float dx = c.x - result[i].Center.x;
                    float dz = c.z - result[i].Center.z;
                    if (dx * dx + dz * dz < minSq)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose)
                {
                    continue;
                }
                ResourcePointSize size = PickPointSize(rng);
                result.Add(new ResourcePoint
                {
                    Center = c,
                    Size = size,
                    Footprint = GetFootprint(size),
                    DisplayCount = GetMineDisplayCount(size)
                });
            }
            return result;
        }

        /// <summary>在资源点占地范围内（距边缘 _resourceEdgeMargin）随机取一个水平偏移。</summary>
        private static Vector3 PointInsideFootprint(ResourcePoint p, float edgeMargin, System.Random rng)
        {
            float half = Mathf.Max(0f, p.Footprint * 0.5f - edgeMargin);
            float ox = ((float)rng.NextDouble() * 2f - 1f) * half;
            float oz = ((float)rng.NextDouble() * 2f - 1f) * half;
            return new Vector3(p.Center.x + ox, 0f, p.Center.z + oz);
        }

        /// <summary>在资源点占地范围内采样一个满足落位域（陆地/水域）的位置，最多重试 attempts 次；返回的 y 已设为地表高度。</summary>
        private Vector3 SampleDomainPosition(Terrain terrain, ResourcePoint p, PlacementDomain domain, System.Random rng, int attempts = 20)
        {
            for (int i = 0; i < attempts; i++)
            {
                Vector3 pos = PointInsideFootprint(p, _resourceEdgeMargin, rng);
                float h = terrain.SampleHeight(pos);
                bool inWater = h < _waterLevel;
                if (domain == PlacementDomain.Land && inWater) continue;
                if (domain == PlacementDomain.Water && !inWater) continue;
                pos.y = h;
                return pos;
            }
            // 兜底：回资源点中心（中心必满足落位域，因资源点采样时已按水面过滤）
            Vector3 c = p.Center;
            c.y = terrain.SampleHeight(c);
            return c;
        }

        /// <summary>prefab 本地 -Y 方向的底部偏移（根中心到资产空间最小 Y 的距离）。优先用 LOD0（最高精度）的包围盒，避免低精度 LOD 异常包围盒污染。</summary>
        private static float GetPrefabBottomOffset(GameObject prefab)
        {
            Renderer[] renderers = null;
            LODGroup lodGroup = prefab.GetComponentInChildren<LODGroup>();
            if (lodGroup != null && lodGroup.GetLODs().Length > 0)
            {
                renderers = lodGroup.GetLODs()[0].renderers;
            }
            if (renderers == null || renderers.Length == 0)
            {
                renderers = prefab.GetComponentsInChildren<Renderer>();
            }
            if (renderers == null || renderers.Length == 0)
            {
                return 0f;
            }
            float minY = float.MaxValue;
            foreach (Renderer r in renderers)
            {
                if (r == null) continue;
                minY = Mathf.Min(minY, r.bounds.min.y);
            }
            if (minY == float.MaxValue)
            {
                return 0f;
            }
            return prefab.transform.position.y - minY;
        }

        /// <summary>农田：紧密排成 m×n 矩阵（田块数 = m×n = 资源量）。</summary>
        private int PlaceFarmMatrix(Terrain terrain, Transform root, List<ResourcePoint> points, Color color, System.Random rng)
        {
            int placed = 0;
            for (int i = 0; i < points.Count; i++)
            {
                ResourcePoint p = points[i];
                GetFarmMN(p.Size, rng, out int m, out int n);
                float spacing = _farmSpacing;
                AddResourceZone(p.Center, n * spacing, m * spacing, _resourceEdgeMargin);
                for (int r = 0; r < m; r++)
                {
                    for (int c = 0; c < n; c++)
                    {
                        float x = p.Center.x + (c - (n - 1) * 0.5f) * spacing;
                        float z = p.Center.z + (r - (m - 1) * 0.5f) * spacing;
                        Vector3 pos = new Vector3(x, 0f, z);
                        float h = terrain.SampleHeight(pos);
                        if (h < _waterLevel) continue;
                        pos.y = h;
                        GameObject go = CreateMarker(pos, color, "农田_" + (i + 1) + "_" + (r + 1) + "_" + (c + 1), 1f);
                        go.transform.SetParent(root, true);
                    }
                }
                placed++;
            }
            return placed;
        }

        /// <summary>人工林：直角折线树带（可分叉、开放、门长≥2，不限制厚度），总树数=资源量。</summary>
        private int PlaceForestBelt(Terrain terrain, Transform root, List<ResourcePoint> points, Color color, System.Random rng)
        {
            int placed = 0;
            for (int i = 0; i < points.Count; i++)
            {
                ResourcePoint p = points[i];
                int target = GetForestCount(p.Size, rng);
                List<Vector3> cells = GenerateForestBelt(terrain, p, target, rng);
                if (cells.Count > 0)
                {
                    float minX = float.MaxValue, maxX = float.MinValue, minZ = float.MaxValue, maxZ = float.MinValue;
                    foreach (Vector3 pos in cells)
                    {
                        if (pos.x < minX) minX = pos.x;
                        if (pos.x > maxX) maxX = pos.x;
                        if (pos.z < minZ) minZ = pos.z;
                        if (pos.z > maxZ) maxZ = pos.z;
                    }
                    AddResourceZone(new Vector3((minX + maxX) * 0.5f, 0f, (minZ + maxZ) * 0.5f),
                        (maxX - minX) + _forestSpacing, (maxZ - minZ) + _forestSpacing, _resourceEdgeMargin);
                }
                for (int k = 0; k < cells.Count; k++)
                {
                    Vector3 pos = cells[k];
                    float h = terrain.SampleHeight(pos);
                    if (h < _waterLevel) continue;
                    pos.y = h;
                    GameObject go = CreateMarker(pos, color, "人工林_" + (i + 1) + "_" + (k + 1), 1f);
                    go.transform.SetParent(root, true);
                }
                placed++;
            }
            return placed;
        }

        /// <summary>在网格上生成人工林树带：直角随机游走 + 分叉 + 门。只种在陆地上，返回世界坐标（y=0）。</summary>
        private List<Vector3> GenerateForestBelt(Terrain terrain, ResourcePoint p, int target, System.Random rng)
        {
            int grid = Mathf.Max(4, Mathf.CeilToInt(p.Footprint));
            HashSet<Vector2Int> cells = new HashSet<Vector2Int>();

            Vector3 WorldOf(Vector2Int c)
            {
                return new Vector3(p.Center.x + (c.x - grid * 0.5f) * _forestSpacing, 0f, p.Center.z + (c.y - grid * 0.5f) * _forestSpacing);
            }
            bool IsLand(Vector2Int c)
            {
                if (!InGrid(c, grid)) return false;
                return terrain.SampleHeight(WorldOf(c)) >= _waterLevel;
            }

            // 找陆地起点
            Vector2Int cur = new Vector2Int(grid / 2, grid / 2);
            if (!IsLand(cur))
            {
                bool found = false;
                for (int r = 1; r <= grid && !found; r++)
                {
                    for (int dx = -r; dx <= r && !found; dx++)
                    {
                        for (int dy = -r; dy <= r && !found; dy++)
                        {
                            Vector2Int c = new Vector2Int(grid / 2 + dx, grid / 2 + dy);
                            if (IsLand(c)) { cur = c; found = true; }
                        }
                    }
                }
                if (!found) return new List<Vector3>();
            }
            cells.Add(cur);
            Vector2Int dir = rng.Next(2) == 0 ? new Vector2Int(1, 0) : new Vector2Int(0, 1);
            List<Vector2Int> junctions = new List<Vector2Int>();
            int gateLen = rng.Next(2, 4);
            int totalTarget = target + gateLen;

            int guard = 0;
            while (cells.Count < totalTarget && guard++ < 4000)
            {
                int steps = rng.Next(2, 4);
                bool moved = false;
                for (int s = 0; s < steps && cells.Count < totalTarget; s++)
                {
                    Vector2Int next = cur + dir;
                    if (!IsLand(next)) break;
                    cur = next;
                    cells.Add(cur);
                    moved = true;
                }
                if (moved && rng.NextDouble() < 0.45f) junctions.Add(cur);
                dir = TurnRight(dir, rng);
            }

            foreach (Vector2Int jp in junctions)
            {
                if (cells.Count >= totalTarget) break;
                Vector2Int d = TurnRight(dir, rng);
                Vector2Int c = jp;
                int steps = rng.Next(2, 4);
                for (int s = 0; s < steps && cells.Count < totalTarget; s++)
                {
                    c += d;
                    if (!IsLand(c)) break;
                    cells.Add(c);
                }
            }

            CarveGate(cells, rng, gateLen);

            List<Vector3> result = new List<Vector3>(cells.Count);
            foreach (Vector2Int c in cells)
            {
                result.Add(WorldOf(c));
            }
            return result;
        }

        /// <summary>在树带上清空一段连续 2~3 格作为入口门（保证最大连续 0 排段 ≥ 2）。</summary>
        private static void CarveGate(HashSet<Vector2Int> cells, System.Random rng, int gateLen)
        {
            if (cells.Count == 0) return;
            List<Vector2Int> list = new List<Vector2Int>(cells);
            Vector2Int start = list[rng.Next(list.Count)];
            Vector2Int dir = rng.Next(2) == 0 ? new Vector2Int(1, 0) : new Vector2Int(0, 1);
            if (rng.Next(2) == 0) dir = -dir;
            for (int g = 0; g < gateLen; g++)
            {
                cells.Remove(start + dir * g);
            }
        }

        private void GetFarmMN(ResourcePointSize size, System.Random rng, out int m, out int n)
        {
            Vector2Int range;
            switch (size)
            {
                case ResourcePointSize.Small: range = _farmSmallRange; break;
                case ResourcePointSize.Medium: range = _farmMediumRange; break;
                default: range = _farmLargeRange; break;
            }
            m = rng.Next(range.x, range.y + 1);
            n = rng.Next(range.x, range.y + 1);
        }

        private int GetForestCount(ResourcePointSize size, System.Random rng)
        {
            Vector2Int range;
            switch (size)
            {
                case ResourcePointSize.Small: range = _forestSmallCount; break;
                case ResourcePointSize.Medium: range = _forestMediumCount; break;
                default: range = _forestLargeCount; break;
            }
            return rng.Next(range.x, range.y + 1);
        }

        private static Vector2Int Perpendicular(Vector2Int dir)
        {
            return new Vector2Int(dir.y, dir.x);
        }

        private static Vector2Int TurnRight(Vector2Int dir, System.Random rng)
        {
            Vector2Int p = Perpendicular(dir);
            return rng.Next(2) == 0 ? p : -p;
        }

        private static bool InGrid(Vector2Int c, int grid)
        {
            return c.x >= 0 && c.x < grid && c.y >= 0 && c.y < grid;
        }

        /// <summary>第二层：为每个矿山资源点按概率随机选一种矿物，在占地范围内生成对应数量的矿石对象。</summary>
        private int PlaceMinePoints(Terrain terrain, Transform root, List<ResourcePoint> points, System.Random rng)
        {
            if (_oreDistribution == null)
            {
                return 0;
            }
            Vector3 terrainSize = terrain.terrainData.size;
            int placed = 0;
            for (int i = 0; i < points.Count; i++)
            {
                ResourcePoint p = points[i];
                AddResourceZone(p.Center, p.Footprint, p.Footprint, _resourceEdgeMargin);
                OreDistributionConfig.OreEntry entry = _oreDistribution.PickRandom(rng);
                if (entry == null)
                {
                    continue;
                }
                for (int k = 0; k < p.DisplayCount; k++)
                {
                    Vector3 pos = SampleDomainPosition(terrain, p, p.Domain, rng);
                    // 约束在地形边界内（距边界保持边距）
                    pos.x = Mathf.Clamp(pos.x, ResourceMargin, terrainSize.x - ResourceMargin);
                    pos.z = Mathf.Clamp(pos.z, ResourceMargin, terrainSize.z - ResourceMargin);

                    Mesh mesh = entry.IsCrystal
                        ? PCGMeshFactory.CreateCrystal(_resourceSeed + i * 17 + k, 2,
                            new Vector2(0.15f, 0.55f), new Vector2(1.4f, 3.2f), new Vector2(0.7f, 1.0f))
                        : PCGMeshFactory.CreateOreCluster(entry.OreKind, _resourceSeed + 100 + i * 17 + k, 6,
                            new Vector2(0.25f, 0.55f), new Vector2(0.5f, 0.9f));

                    Material mat = entry.Material != null
                        ? entry.Material
                        : ResolveMaterial(null,
                            entry.IsCrystal ? new Color(0.5f, 0.58f, 0.9f, 0.55f) : PCGMeshFactory.GetDefaultOreColor(entry.OreKind),
                            entry.IsCrystal ? 0.55f : 1f,
                            entry.IsCrystal ? 0.95f : 0.45f,
                            entry.IsCrystal ? 0.05f : 0.65f);

                    // 按占地大小（正方形）反算缩放：让矿石水平占地 = footprint（米），缩放可与其它对象统一比较
                    float footprint = entry.RandomFootprint(rng);
                    float scale = PCGMeshFactory.ScaleForFootprint(mesh, footprint);

                    // 贴地：让 mesh 底部（而非中心）落到地面，再下沉一点部分埋地，避免悬空
                    float groundY = terrain.SampleHeight(pos);
                    float bottomOffset = (mesh.bounds.center.y - mesh.bounds.extents.y) * scale;
                    pos.y = groundY - bottomOffset - 0.15f;

                    GameObject go = new GameObject(entry.DisplayName + "_" + (i + 1) + "_" + (k + 1));
                    go.AddComponent<MeshFilter>().sharedMesh = mesh;
                    go.AddComponent<MeshRenderer>().sharedMaterial = mat;
                    go.transform.SetParent(root, true);
                    go.transform.position = pos;
                    go.transform.localScale = Vector3.one * scale;
                    go.transform.rotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
                }
                placed++;
            }
            return placed;
        }

        private Vector3 FindBaseSpot(Terrain terrain, Vector3 size)
        {
            System.Random rng = new System.Random(_resourceSeed ^ 0xABCDEF);
            Vector3 center = new Vector3(size.x * 0.5f, 0f, size.z * 0.5f);
            Vector3 best = center;
            float bestDist = float.MaxValue;
            for (int i = 0; i < 500; i++)
            {
                float x = size.x * 0.5f + ((float)rng.NextDouble() - 0.5f) * size.x * 0.5f;
                float z = size.z * 0.5f + ((float)rng.NextDouble() - 0.5f) * size.z * 0.5f;
                Vector3 world = new Vector3(x, 0f, z);
                float h = terrain.SampleHeight(world);
                if (h < _waterLevel)
                {
                    continue;
                }
                world.y = h;
                Vector3 normal = GetTerrainNormal(terrain, world);
                float slope = Vector3.Angle(Vector3.up, normal);
                float dist = Vector3.Distance(world, center);
                if (slope <= 8f && dist < bestDist)
                {
                    bestDist = dist;
                    best = world;
                }
            }
            return best;
        }

        private void PlaceBaseSpot(Transform root, Vector3 pos, Color color)
        {
            GameObject go = CreateMarker(pos, color, "初始基地", 5f);
            go.transform.SetParent(root, true);
            AddResourceZone(pos, 5f, 5f, 0f);
        }

        /// <summary>记录一个资源点的实际占地区域（xz 平面矩形），散布装饰时排除。</summary>
        private void AddResourceZone(Vector3 center, float width, float depth, float margin)
        {
            float m = margin + _scatterClearance;
            _resourceZones.Add(new Rect(
                center.x - width * 0.5f - m,
                center.z - depth * 0.5f - m,
                width + m * 2f,
                depth + m * 2f));
        }

        /// <summary>判断某个水平位置是否落在资源点占地区域内。</summary>
        private bool InsideResourceZone(float x, float z)
        {
            for (int i = 0; i < _resourceZones.Count; i++)
            {
                if (_resourceZones[i].Contains(new Vector2(x, z))) return true;
            }
            return false;
        }

        private static GameObject CreateMarker(Vector3 pos, Color color, string name, float size)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            // 底部贴地：方块高度 0.5，半高 0.25，让底部落到地表（pos.y 为地表高度）
            go.transform.position = new Vector3(pos.x, pos.y + 0.25f, pos.z);
            go.transform.localScale = new Vector3(size, 0.5f, size);
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Smoothness", 0.3f);
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.DisableKeyword("_SURFACE_TYPE_OPAQUE");
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        private static void Shuffle<T>(List<T> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                T tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }

        // ==================== 共享算法 ====================

        private static float FbmRaw(float x, float y, float freq, int octaves, int seed)
        {
            float total = 0f;
            float amp = 1f;
            float offset = seed * 0.001f;
            float maxAmp = 0f;
            float f = freq;
            for (int i = 0; i < octaves; i++)
            {
                total += Mathf.PerlinNoise(x * f + offset, y * f + offset) * amp;
                maxAmp += amp;
                amp *= 0.5f;
                f *= 2f;
            }
            return total / maxAmp;
        }

        private static float Fbm(float x, float y, float freq, int octaves, float remapPow, int seed, float warp, int terrace)
        {
            if (warp > 0.001f)
            {
                float qx = FbmRaw(x + 3.7f, y + 1.9f, freq * 0.5f, 2, seed);
                float qy = FbmRaw(x + 8.3f, y + 6.1f, freq * 0.5f, 2, seed + 7);
                x += warp * (qx - 0.5f) * 2f;
                y += warp * (qy - 0.5f) * 2f;
            }
            float h = Mathf.Pow(FbmRaw(x, y, freq, octaves, seed), remapPow);
            if (terrace > 1)
            {
                float scaled = h * terrace;
                float floored = Mathf.Floor(scaled);
                float frac = scaled - floored;
                h = (floored + frac * frac * (3f - 2f * frac)) / terrace;
            }
            return h;
        }

        private static Color SurfaceColor(float h)
        {
            Color low = new Color(0.34f, 0.46f, 0.24f);
            Color mid = new Color(0.50f, 0.42f, 0.30f);
            Color high = new Color(0.55f, 0.53f, 0.50f);
            Color c = h < 0.42f
                ? Color.Lerp(low, mid, h / 0.42f)
                : Color.Lerp(mid, high, (h - 0.42f) / 0.58f);
            float n = Mathf.PerlinNoise(h * 20f + 3.1f, h * 17f + 7.7f);
            return Color.Lerp(c, c * (0.85f + n * 0.3f), 0.5f);
        }

        private static Vector3 GetTerrainNormal(Terrain terrain, Vector3 world, float eps = 1f)
        {
            float hL = terrain.SampleHeight(world + Vector3.left * eps);
            float hR = terrain.SampleHeight(world + Vector3.right * eps);
            float hD = terrain.SampleHeight(world + Vector3.back * eps);
            float hU = terrain.SampleHeight(world + Vector3.forward * eps);
            return new Vector3(hL - hR, 2f * eps, hD - hU).normalized;
        }

        private static List<GameObject> LoadPrefabsFromFolder(DefaultAsset folder)
        {
            List<GameObject> prefabs = new List<GameObject>();
            if (folder == null)
            {
                return prefabs;
            }
            string folderPath = AssetDatabase.GetAssetPath(folder);
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    prefabs.Add(prefab);
                }
            }
            return prefabs;
        }

        private static List<Vector2> PoissonDisk(float width, float depth, float minDist, int seed, int maxPoints)
        {
            System.Random rng = new System.Random(seed);
            float cell = minDist / Mathf.Sqrt(2f);
            int cols = Mathf.CeilToInt(width / cell) + 1;
            int rows = Mathf.CeilToInt(depth / cell) + 1;
            int[,] grid = new int[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    grid[r, c] = -1;
                }
            }

            List<Vector2> points = new List<Vector2>();
            List<int> active = new List<int>();
            Vector2 first = new Vector2((float)rng.NextDouble() * width, (float)rng.NextDouble() * depth);
            points.Add(first);
            active.Add(0);
            grid[Mathf.FloorToInt(first.y / cell), Mathf.FloorToInt(first.x / cell)] = 0;

            while (active.Count > 0 && points.Count < maxPoints)
            {
                int ai = rng.Next(active.Count);
                Vector2 p = points[active[ai]];
                bool found = false;
                for (int k = 0; k < 30; k++)
                {
                    float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
                    float radius = minDist * (1f + (float)rng.NextDouble());
                    Vector2 q = p + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                    if (q.x < 0f || q.x >= width || q.y < 0f || q.y >= depth)
                    {
                        continue;
                    }
                    int qi = Mathf.FloorToInt(q.y / cell);
                    int qj = Mathf.FloorToInt(q.x / cell);
                    bool ok = true;
                    for (int di = -2; di <= 2 && ok; di++)
                    {
                        for (int dj = -2; dj <= 2; dj++)
                        {
                            int ni = qi + di;
                            int nj = qj + dj;
                            if (ni < 0 || ni >= rows || nj < 0 || nj >= cols || grid[ni, nj] == -1)
                            {
                                continue;
                            }
                            if (Vector2.Distance(points[grid[ni, nj]], q) < minDist)
                            {
                                ok = false;
                                break;
                            }
                        }
                    }
                    if (ok)
                    {
                        points.Add(q);
                        active.Add(points.Count - 1);
                        grid[qi, qj] = points.Count - 1;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    active.RemoveAt(ai);
                }
            }
            return points;
        }
    }
}
