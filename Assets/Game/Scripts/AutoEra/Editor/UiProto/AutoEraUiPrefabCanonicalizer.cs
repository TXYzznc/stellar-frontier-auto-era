#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AutoEra.Editor.UiProto
{
    /// <summary>
    /// 预制体「固化」：把加载时会被重算/回填的字段先算定并写进资产，消除「打开即变脏」。
    ///
    /// 为什么预制体会自己变脏——两个独立来源，都能落到具体代码行：
    ///
    /// 1. **布局组接管**：Horizontal/Vertical/GridLayoutGroup 一旦启用，就会把子节点的
    ///    anchor / anchoredPosition / sizeDelta 登记为 driven，并在每次布局时重写。
    ///    生成器按契约写的是「初始化值」（例如 anchor (0,1)），布局组算出来的却是另一套
    ///    （HVLG 走 SetInsetAndSizeFromParentEdge 的锚点基准是 (0,0)）。两者不等 ⇒ 打开就脏。
    /// 2. **TMP 的惰性缓存**：<c>m_TextStyleHashCode</c> 由 `textStyle` getter 的兜底分支写入
    ///    （TMP_Text.cs:368：GetStyle 查不到就退回 NormalStyle 并写回它的 hashCode），
    ///    <c>m_fontColor32</c> 由文本解析时同步（TMPro_UGUI_Private.cs:1771 `m_fontColor32 = m_fontColor`）。
    ///    新建的 TMP 组件这两个字段是默认值（0 / 白），第一次被编辑器碰到就被回填 ⇒ 打开就脏。
    ///
    /// 本工具做的事就是「把这两类值先跑一遍再保存」，于是再打开时它们已经是计算结果、不再变化。
    /// 注意第 1 类只能做到「同一母尺寸下幂等」：勾了 Child Force Expand 的布局组，子节点尺寸是
    /// 容器可用宽度的函数，容器宽度又取决于打开时的画布尺寸——所以换一个窗口尺寸打开仍可能有差异。
    /// 第 2 类（TMP 缓存）是一次性根治。
    /// </summary>
    public static class AutoEraUiPrefabCanonicalizer
    {
        public const string UiPrefabRoot = "Assets/Game/Prefabs/UI";
        private const string ReportPath = "tools/_prefab_canonicalize_report.txt";

        /// <summary>固化时承载预制体根的母尺寸：项目设计分辨率，与 AppSettings 一致。</summary>
        private static readonly Vector2 HostSize = new Vector2(1920f, 1080f);

        // ------------------------------------------------------------------ 菜单

        [MenuItem("Game Framework/AutoEra/UI/固化预制体（把加载时会重算的值写进资产）", priority = 2004)]
        public static void CanonicalizeAllFromMenu()
        {
            var log = new StringBuilder();
            CanonicalizeAll(log);
            Report("canonicalize", log.ToString());
        }

        // ------------------------------------------------------------------ 主流程

        /// <summary>逐个固化界面预制体，并把「改了哪些字段、分别由谁负责」写进 log。</summary>
        public static int CanonicalizeAll(StringBuilder log)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                log.AppendLine("请先退出 Play Mode。");
                return 0;
            }

            string[] prefabs = Directory
                .GetFiles(UiPrefabRoot, "*.prefab", SearchOption.AllDirectories)
                .Select(p => p.Replace('\\', '/'))
                .OrderBy(p => p, StringComparer.Ordinal)
                .ToArray();

            var owners = new SortedDictionary<string, int>(StringComparer.Ordinal);
            int changed = 0;

            foreach (string path in prefabs)
            {
                string before = ReadAllText(path);
                Canonicalize(path);
                string after = ReadAllText(path);
                if (before == after)
                {
                    continue;
                }

                changed++;
                SortedDictionary<string, int> fields = DiffFields(before, after);
                foreach (KeyValuePair<string, int> pair in fields)
                {
                    owners.TryGetValue(pair.Key, out int sum);
                    owners[pair.Key] = sum + pair.Value;
                }
                log.AppendLine($"  {Path.GetFileName(path)}: {fields.Sum(p => p.Value)} 行变化（{Describe(fields)}）");
            }

            log.AppendLine();
            log.AppendLine($"固化：{prefabs.Length} 个预制体，{changed} 个文件发生变化。");
            if (owners.Count > 0)
            {
                log.AppendLine("变化字段归属（再打开就不该再变的就是第 2 类）：");
                foreach (KeyValuePair<string, int> pair in owners.OrderByDescending(p => p.Value))
                {
                    log.AppendLine($"  {pair.Key,-24} {pair.Value,5} 行   ← {Owner(pair.Key)}");
                }
            }
            else
            {
                log.AppendLine("没有任何字段变化——说明预制体已经是固化的（打开不再产生脏数据）。");
            }
            return changed;
        }

        /// <summary>
        /// 把单个预制体加载到「有确定母尺寸」的环境里跑一遍布局与 TMP 网格生成，然后原样保存。
        /// </summary>
        public static void Canonicalize(string assetPath)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(assetPath);
            if (root == null)
            {
                return;
            }

            Canonicalize(root);
            PrefabUtility.SaveAsPrefabAsset(root, assetPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        /// <summary>
        /// 对内存里的预制体根跑一遍「加载时会发生的事」：给确定母尺寸 → 强制布局 → 强制 TMP 生成网格。
        /// 生成器在保存前调用它，产出就是固化的（<see cref="Canonicalize(string)"/> 也走这里）。
        /// </summary>
        public static void Canonicalize(GameObject root)
        {
            if (root == null || !(root.transform is RectTransform rootRect))
            {
                return;
            }

            // 刻意不造任何临时父级/预览场景：在活动场景里 new GameObject 会把它标脏，
            // 脏场景会让 EditMode 测试直接拒绝启动（实测 Launch.unity 被弄脏后测试条条失败）。
            // 预制体根是全拉伸的，脱离父级时 rect 是 0×0，布局跑出来没意义——所以临时把根
            // 自己改成固定尺寸（设计分辨率）跑一遍，最后原样还原它自己的 rect。
            Vector2 savedMin = rootRect.anchorMin;
            Vector2 savedMax = rootRect.anchorMax;
            Vector2 savedSize = rootRect.sizeDelta;
            Vector2 savedPosition = rootRect.anchoredPosition;
            Vector2 savedPivot = rootRect.pivot;

            try
            {
                rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
                rootRect.pivot = new Vector2(0.5f, 0.5f);
                rootRect.anchoredPosition = Vector2.zero;
                rootRect.sizeDelta = HostSize;

                // 布局组把子节点的 anchor/sizeDelta 重写成组计算值——这一步就是把它们算定。
                LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);
                foreach (LayoutGroup group in root.GetComponentsInChildren<LayoutGroup>(true))
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)group.transform);
                }

                // TMP 的惰性缓存（m_TextStyleHashCode / m_fontColor32）在**文本解析**时回填，
                // 而解析只在内容被判为脏时才跑——所以必须 forceTextReparsing=true，
                // 否则对刚加载、文本没变的节点会整段跳过解析，缓存仍是生成器写下的默认值，
                // 等到 Prefab Mode 里以「全新对象」加载时才被回填，于是打开一次就变脏。
                foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    text.ForceMeshUpdate(true, true);
                }
            }
            finally
            {
                rootRect.anchorMin = savedMin;
                rootRect.anchorMax = savedMax;
                rootRect.sizeDelta = savedSize;
                rootRect.anchoredPosition = savedPosition;
                rootRect.pivot = savedPivot;
            }
        }

        // ------------------------------------------------------------------ 报告

        /// <summary>
        /// 实测：把每个界面预制体真的用 Prefab Mode 打开一次，强制跑一遍舞台里的布局与 TMP，
        /// 按「Auto Save 会写回什么」保存一次，再比对文件——列出打开一次到底改了哪些字段。
        /// 这是回答「为什么每次打开就变脏」的唯一硬证据；跑完请再跑一次固化把状态收回来。
        /// </summary>
        [MenuItem("Game Framework/AutoEra/UI/实测：Prefab Mode 打开一次会改哪些字段", priority = 2005)]
        public static void MeasurePrefabModeFromMenu()
        {
            var log = new StringBuilder();
            string[] prefabs = Directory
                .GetFiles(UiPrefabRoot, "*.prefab", SearchOption.AllDirectories)
                .Select(p => p.Replace('\\', '/'))
                .OrderBy(p => p, StringComparer.Ordinal)
                .ToArray();

            var owners = new SortedDictionary<string, int>(StringComparer.Ordinal);
            var perFile = new List<string>();
            int changed = 0;

            foreach (string path in prefabs)
            {
                string before = ReadAllText(path);
                PrefabStage stage = PrefabStageUtility.OpenPrefab(path);
                if (stage == null || stage.prefabContentsRoot == null)
                {
                    perFile.Add($"  {Path.GetFileName(path)}: Prefab Mode 打开失败");
                    continue;
                }

                try
                {
                    Canvas.ForceUpdateCanvases();
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)stage.prefabContentsRoot.transform);
                    foreach (TMP_Text text in stage.prefabContentsRoot.GetComponentsInChildren<TMP_Text>(true))
                    {
                        text.ForceMeshUpdate(true, false);
                    }
                    PrefabUtility.SaveAsPrefabAsset(stage.prefabContentsRoot, path);
                }
                finally
                {
                    StageUtility.GoBackToPreviousStage();
                }

                string after = ReadAllText(path);
                if (before == after)
                {
                    continue;
                }

                changed++;
                SortedDictionary<string, int> fields = DiffFields(before, after);
                foreach (KeyValuePair<string, int> pair in fields)
                {
                    owners.TryGetValue(pair.Key, out int sum);
                    owners[pair.Key] = sum + pair.Value;
                }
                perFile.Add($"  {Path.GetFileName(path)}: {fields.Sum(p => p.Value)} 行（{Describe(fields)}）");
            }

            log.AppendLine($"Prefab Mode 实测：{prefabs.Length} 个预制体，打开并保存后 {changed} 个文件发生变化。");
            log.AppendLine();
            log.AppendLine(string.Join(Environment.NewLine, perFile));
            log.AppendLine();
            log.AppendLine("字段归属：");
            foreach (KeyValuePair<string, int> pair in owners.OrderByDescending(p => p.Value))
            {
                log.AppendLine($"  {pair.Key,-24} {pair.Value,5} 行   ← {Owner(pair.Key)}");
            }

            Report("prefabmode", log.ToString());
        }

        private static void Report(string name, string text)
        {
            try
            {
                File.WriteAllText(ReportPath, text, new UTF8Encoding(false));
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[AutoEraUiPrefabCanonicalizer] 写报告失败：{exception.Message}");
            }
            Debug.Log($"[AutoEraUiPrefabCanonicalizer] {name} 完成，完整结论见 {ReportPath}");
        }

        private static string ReadAllText(string path)
        {
            return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        }

        /// <summary>逐行比对，按「字段名」汇总差异行数。行数不同则整体计为一次结构变化。</summary>
        private static SortedDictionary<string, int> DiffFields(string before, string after)
        {
            var result = new SortedDictionary<string, int>(StringComparer.Ordinal);
            string[] oldLines = before.Replace("\r\n", "\n").Split('\n');
            string[] newLines = after.Replace("\r\n", "\n").Split('\n');
            if (oldLines.Length != newLines.Length)
            {
                result["<行数变化>"] = Math.Abs(newLines.Length - oldLines.Length);
                return result;
            }

            for (int i = 0; i < oldLines.Length; i++)
            {
                if (oldLines[i] == newLines[i])
                {
                    continue;
                }
                string key = FieldKey(oldLines[i]);
                result.TryGetValue(key, out int count);
                result[key] = count + 1;
            }
            return result;
        }

        private static string FieldKey(string line)
        {
            string trimmed = line.Trim().TrimStart('-').Trim();
            int colon = trimmed.IndexOf(':');
            if (colon > 0)
            {
                return trimmed.Substring(0, colon).Trim();
            }
            return trimmed.Length > 24 ? trimmed.Substring(0, 24) : trimmed;
        }

        private static string Describe(SortedDictionary<string, int> fields)
        {
            return string.Join(", ", fields.OrderByDescending(p => p.Value).Take(6)
                .Select(p => $"{p.Key} {p.Value}"));
        }

        /// <summary>字段 → 责任方。这就是「为什么打开就脏」的分类答案。</summary>
        private static string Owner(string field)
        {
            switch (field)
            {
                case "m_AnchorMin":
                case "m_AnchorMax":
                case "m_AnchoredPosition":
                case "m_SizeDelta":
                case "m_Pivot":
                    return "布局组接管（每次布局重写 driven 值；勾了 Child Force Expand 的组还会随容器宽度变）";
                case "m_TextStyleHashCode":
                    return "TMP 惰性缓存（TMP_Text.cs:368 textStyle getter 兜底写回）";
                case "m_fontColor32":
                case "m_htmlColor":
                case "m_underlineColor":
                case "m_strikethroughColor":
                case "m_faceColor":
                case "rgba":
                    return "TMP 惰性缓存（生成网格时 m_fontColor32 = m_fontColor）";
                case "m_FontMaterials":
                case "m_FontSharedMaterials":
                case "m_fontMaterials":
                case "m_fontSharedMaterials":
                    return "TMP 材质缓存（按图集张数重算）";
                case "<行数变化>":
                    return "序列化结构变化（需要人工确认）";
                default:
                    return "其它";
            }
        }
    }
}
#endif
