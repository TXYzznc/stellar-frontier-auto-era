#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor.UiProto
{
    /// <summary>
    /// 界面字体资产（SIMHEI SDF）的单图集化、预烘与 TMP 子网格清理。
    ///
    /// 要解决的问题：TMP 为「一段文字需要不止一个材质」的文本生成
    /// <c>TMP SubMeshUI [...]</c> 子对象。材质数量的唯一来源是字体图集张数——本工程的
    /// SIMHEI SDF 是 Dynamic + Multi Atlas 的 1024×1024 字体，图集写满就自动追加
    /// Atlas 1 / Atlas 2 …，于是同一段文字的汉字被拆到多张图集上，层级里就冒出 3~4 个子网格。
    ///
    /// 治法不是删子对象（它们是 TMP 运行时生成的、带 HideFlags.DontSave，本来就不在资产里），
    /// 而是让「一段文字只需一张图集」：
    ///   · 把图集分辨率提到 4096×4096（字形采样字号 90 / padding 9 保持不变 ⇒ 字宽与观感不变）；
    ///   · 关掉 Multi Atlas Textures，图集只有一张、材质只有一个；
    ///   · 把界面预制体 + 运行期数据文本用到的字符全部预烘进去，运行期不再追加。
    ///
    /// 实现要点：<see cref="TMP_FontAsset.ClearFontAssetData"/> 内部会把 atlas[0]
    /// **原地** 缩放到 <c>m_AtlasWidth/m_AtlasHeight</c>、销毁 index≥1 的旧图集并把数组收缩到 1，
    /// 图集与材质对象本身不换 ⇒ 33 个预制体里 1980 条文本指向的
    /// <c>m_sharedMaterial</c>（字体资产内的材质子资产）引用不会失效。
    /// </summary>
    public static class AutoEraUiFontTool
    {
        public const string FontAssetPath = "Assets/Game/Fonts/UI/SIMHEI SDF.asset";
        public const string UiPrefabRoot = "Assets/Game/Prefabs/UI";

        /// <summary>目标图集边长。实测界面文本 564 字、界面+数据文本并集 672 字，4096 有约一倍余量。</summary>
        public const int AtlasSize = 4096;

        private const string SubMeshNamePrefix = "TMP SubMeshUI";
        private const string SubObjectNamePrefix = "TMP UI SubObject";

        /// <summary>除界面预制体之外还要预烘的字符来源（运行期会显示的数据文本与文案规格）。</summary>
        private static readonly string[] ExtraTextSources =
        {
            "Assets/Game/DataTable",
            "GameData/AIData",
            "Docs/GameDesign/04-第一版/07-玩家可见文案规格.md",
        };

        // ------------------------------------------------------------------ 菜单

        [MenuItem("Game Framework/AutoEra/UI/字体：单图集重建并预热（消除 TMP 子网格）", priority = 2010)]
        public static void RebuildFromMenu()
        {
            string report = Rebuild();
            Report("rebuild", report);
        }

        [MenuItem("Game Framework/AutoEra/UI/字体：检查（图集/字符覆盖/子网格/引用）", priority = 2011)]
        public static void CheckFromMenu()
        {
            List<string> problems = Check();
            string report = problems.Count == 0
                ? "通过：字体单图集、字符全覆盖、预制体无 TMP 子网格、字体引用统一。"
                : $"未通过（{problems.Count} 项）：{Environment.NewLine}" + string.Join(Environment.NewLine, problems);
            Report("check", report);
        }

        [MenuItem("Game Framework/AutoEra/UI/字体：清理预制体里的 TMP 子网格并统一引用", priority = 2012)]
        public static void CleanPrefabsFromMenu()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            var log = new StringBuilder();
            RepairPrefabs(font, log);
            AssetDatabase.SaveAssets();
            Report("clean", log.ToString());
        }

        // 不弹模态对话框：菜单既给人点、也给自动化（unity-skills 的 editor_execute_menu）调，
        // 模态框会把 REST 调用挂住直到有人去点确定。结论一律写进 tools/_font_report.txt，
        // console 摘要在长文本下会被截断（与门1 同一个理由）。
        private const string ReportPath = "tools/_font_report.txt";

        private static void Report(string name, string text)
        {
            try
            {
                File.WriteAllText(ReportPath, text, new UTF8Encoding(false));
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[AutoEraUiFontTool] 写报告失败：{exception.Message}");
            }
            Debug.Log($"[AutoEraUiFontTool] {name} 完成，完整结论见 {ReportPath}");
        }

        // ------------------------------------------------------------------ 重建

        /// <summary>把字体资产重建为单图集并预烘全部已知字符，然后修复预制体。</summary>
        public static string Rebuild()
        {
            var log = new StringBuilder();
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return "请先退出 Play Mode 再重建字体资产。";
            }

            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (font == null)
            {
                return "找不到字体资产：" + FontAssetPath;
            }
            if (font.atlasPopulationMode != AtlasPopulationMode.Dynamic)
            {
                return $"字体 {font.name} 的 Atlas Population Mode = {font.atlasPopulationMode}（不是 Dynamic），" +
                       "无法预烘字形；请先在字体资产检视面板把模式改回 Dynamic。";
            }

            int prefabCount = 0;
            int textCount = 0;
            HashSet<char> chars = CollectCharacters(ref prefabCount, ref textCount);
            string requested = new string(chars.OrderBy(c => c).ToArray());

            log.AppendLine($"来源：界面预制体 {prefabCount} 个 / TMP 文本 {textCount} 条 + 数据文本");
            log.AppendLine($"必需字符 {requested.Length} 个");
            log.AppendLine($"重建前：图集 {font.atlasTextureCount} 张 {font.atlasWidth}×{font.atlasHeight}，" +
                           $"多图集={font.isMultiAtlasTexturesEnabled}，字形 {font.characterTable.Count}");

            ResizeAtlasAndClear(font, AtlasSize);

            font.TryAddCharacters(requested, out string missing);
            int missingCount = string.IsNullOrEmpty(missing) ? 0 : missing.Length;

            log.AppendLine($"重建后：图集 {font.atlasTextureCount} 张 {font.atlasWidth}×{font.atlasHeight}，" +
                           $"多图集={font.isMultiAtlasTexturesEnabled}，字形 {font.characterTable.Count}");
            if (missingCount > 0)
            {
                log.AppendLine($"警告：仍有 {missingCount} 个字符没进图集（图集可能已满，需降低采样字号或再扩大图集）：");
                log.AppendLine("  " + missing);
            }

            EditorUtility.SetDirty(font);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(FontAssetPath, ImportAssetOptions.ForceUpdate);

            RepairPrefabs(font, log);
            AssetDatabase.SaveAssets();
            return log.ToString();
        }

        /// <summary>
        /// 只改分辨率、不换对象地重建图集，并同步材质上的图集参数。
        /// </summary>
        private static void ResizeAtlasAndClear(TMP_FontAsset font, int size)
        {
            font.isMultiAtlasTexturesEnabled = false;

            // atlasWidth/atlasHeight 是 internal set，编辑器里用 SerializedObject 写入。
            var so = new SerializedObject(font);
            so.FindProperty("m_AtlasWidth").intValue = size;
            so.FindProperty("m_AtlasHeight").intValue = size;
            so.ApplyModifiedPropertiesWithoutUndo();

            // ClearFontAssetData → ClearAtlasTextures：
            //   · 销毁 index≥1 的旧图集，数组收缩到 1（atlas[0] 的对象保持不变）
            //   · 把 atlas[0] 原地 Resize 到 m_AtlasWidth × m_AtlasHeight 并清空
            //   · 重置 m_AtlasTextureIndex / m_AtlasTexture 与空闲矩形，随后 ReadFontAssetDefinition 读回新尺寸
            font.ClearFontAssetData(false);

            var material = font.material;
            if (material != null)
            {
                material.SetTexture(Shader.PropertyToID("_MainTex"), font.atlasTexture);
                material.SetFloat(Shader.PropertyToID("_TextureWidth"), size);
                material.SetFloat(Shader.PropertyToID("_TextureHeight"), size);
                material.SetFloat(Shader.PropertyToID("_GradientScale"), font.atlasPadding + 1);
                EditorUtility.SetDirty(material);
            }
            if (font.atlasTexture != null)
            {
                EditorUtility.SetDirty(font.atlasTexture);
            }
        }

        // ------------------------------------------------------------------ 预制体修复

        /// <summary>
        /// 清掉预制体里被误序列化的 TMP 子网格对象，并把所有 TMP 文本统一到目标字体。
        /// 正常情况下 TMP 用 HideFlags.DontSave 不落盘，这一段是「一旦落盘就清掉」的兜底。
        /// </summary>
        public static void RepairPrefabs(TMP_FontAsset font, StringBuilder log)
        {
            int files = 0;
            int removed = 0;
            int retargeted = 0;
            int materialOverrides = 0;

            foreach (string file in Directory.GetFiles(UiPrefabRoot, "*.prefab", SearchOption.AllDirectories))
            {
                string path = file.Replace('\\', '/');
                bool dirty = false;
                GameObject contents = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    foreach (var sub in contents.GetComponentsInChildren<TMP_SubMeshUI>(true).Cast<Component>()
                                 .Concat(contents.GetComponentsInChildren<TMP_SubMesh>(true)).ToArray())
                    {
                        if (sub == null)
                        {
                            continue;
                        }
                        removed++;
                        dirty = true;
                        UnityEngine.Object.DestroyImmediate(sub.gameObject);
                    }

                    // 名字兜底：脚本丢失时按组件找不到，但节点名还在。
                    foreach (Transform node in contents.GetComponentsInChildren<Transform>(true).ToArray())
                    {
                        if (node == null || node == contents.transform)
                        {
                            continue;
                        }
                        if (node.name.StartsWith(SubMeshNamePrefix, StringComparison.Ordinal) ||
                            node.name.StartsWith(SubObjectNamePrefix, StringComparison.Ordinal))
                        {
                            removed++;
                            dirty = true;
                            UnityEngine.Object.DestroyImmediate(node.gameObject);
                        }
                    }

                    foreach (TMP_Text text in contents.GetComponentsInChildren<TMP_Text>(true))
                    {
                        if (font != null && text.font != font)
                        {
                            text.font = font;
                            retargeted++;
                            dirty = true;
                        }

                        var textSo = new SerializedObject(text);
                        SerializedProperty overrides = textSo.FindProperty("m_fontSharedMaterials");
                        if (overrides != null && overrides.arraySize > 1)
                        {
                            overrides.arraySize = 0;
                            textSo.ApplyModifiedPropertiesWithoutUndo();
                            materialOverrides++;
                            dirty = true;
                        }
                    }

                    if (dirty)
                    {
                        PrefabUtility.SaveAsPrefabAsset(contents, path);
                        files++;
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }
            }

            log.AppendLine($"预制体修复：改动 {files} 个文件，删除 TMP 子网格对象 {removed} 个，" +
                           $"改字体引用 {retargeted} 处，清多余材质覆盖 {materialOverrides} 处");
        }

        // ------------------------------------------------------------------ 检查

        /// <summary>返回问题清单；空表示字体单图集、字符全覆盖、预制体干净且引用统一。</summary>
        public static List<string> Check()
        {
            var problems = new List<string>();
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (font == null)
            {
                problems.Add($"找不到字体资产：{FontAssetPath}");
                return problems;
            }

            if (font.atlasTextureCount != 1)
            {
                problems.Add($"字体 {font.name} 用了 {font.atlasTextureCount} 张图集（应为 1）：" +
                             "一段文字跨图集时 TMP 就会生成 TMP SubMeshUI 子对象。");
            }
            if (font.isMultiAtlasTexturesEnabled)
            {
                problems.Add($"字体 {font.name} 仍开着 Multi Atlas Textures：图集写满会自动追加新图集并再次产生子网格。");
            }

            var known = new HashSet<uint>(font.characterTable.Select(c => c.unicode));
            int prefabCount = 0;
            int textCount = 0;
            var chars = CollectCharacters(ref prefabCount, ref textCount);
            char[] missing = chars.Where(c => !known.Contains(c)).OrderBy(c => c).ToArray();
            if (missing.Length > 0)
            {
                problems.Add($"有 {missing.Length} 个字符不在字体图集里（运行期会动态追加，图集满了就会产生子网格）：" +
                             new string(missing.Take(80).ToArray()));
            }

            foreach (string file in Directory.GetFiles(UiPrefabRoot, "*.prefab", SearchOption.AllDirectories))
            {
                string path = file.Replace('\\', '/');
                GameObject contents = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    int components = contents.GetComponentsInChildren<TMP_SubMeshUI>(true).Length +
                                     contents.GetComponentsInChildren<TMP_SubMesh>(true).Length;
                    int named = contents.GetComponentsInChildren<Transform>(true)
                        .Count(t => t != contents.transform &&
                                    (t.name.StartsWith(SubMeshNamePrefix, StringComparison.Ordinal) ||
                                     t.name.StartsWith(SubObjectNamePrefix, StringComparison.Ordinal)));
                    if (components + named > 0)
                    {
                        problems.Add($"{path} 里残留 {components + named} 个 TMP 子网格对象。");
                    }

                    foreach (TMP_Text text in contents.GetComponentsInChildren<TMP_Text>(true))
                    {
                        if (text.font != font)
                        {
                            problems.Add($"{path} 的 {text.name} 使用的字体是 " +
                                         $"{(text.font == null ? "空" : text.font.name)}，不是 {font.name}。");
                        }
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }
            }

            return problems;
        }

        // ------------------------------------------------------------------ 字符采集

        /// <summary>采集界面与运行期数据文本里出现的全部字符（空白与控制符交给 TMP 合成）。</summary>
        public static HashSet<char> CollectCharacters(ref int prefabCount, ref int textCount)
        {
            var set = new HashSet<char>();

            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { UiPrefabRoot }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject contents = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    prefabCount++;
                    foreach (TMP_Text text in contents.GetComponentsInChildren<TMP_Text>(true))
                    {
                        textCount++;
                        AddCharacters(set, text.text);
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }
            }

            foreach (string source in ExtraTextSources)
            {
                if (File.Exists(source))
                {
                    AddCharacters(set, ReadTextFile(source));
                }
                else if (Directory.Exists(source))
                {
                    foreach (string file in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
                    {
                        string ext = Path.GetExtension(file).ToLowerInvariant();
                        if (ext == ".txt" || ext == ".json" || ext == ".md" || ext == ".csv")
                        {
                            AddCharacters(set, ReadTextFile(file));
                        }
                    }
                }
            }

            return set;
        }

        private static void AddCharacters(HashSet<char> set, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            foreach (char c in text)
            {
                if (c == '\uFFFD' || char.IsControl(c) || char.IsWhiteSpace(c))
                {
                    continue;
                }
                if (char.IsSurrogate(c))
                {
                    continue;
                }
                set.Add(c);
            }
        }

        /// <summary>按 UTF-8 宽松解码：数据表文本里混入非法字节时用替换字符兜底，不抛异常。</summary>
        private static string ReadTextFile(string path)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                return new UTF8Encoding(false, false).GetString(bytes);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[AutoEraUiFontTool] 读取文本失败：{path}（{exception.Message}）");
                return string.Empty;
            }
        }
    }
}
#endif
