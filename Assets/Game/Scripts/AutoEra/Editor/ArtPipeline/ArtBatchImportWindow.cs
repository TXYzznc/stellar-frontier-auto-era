using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor.ArtPipeline
{
    /// <summary>
    /// Reads the production documents as the art import catalog and copies a generated batch into Assets.
    /// The source folder is sorted naturally, so names such as -2 and -10 follow the Explorer order.
    /// </summary>
    internal sealed class ArtBatchImportWindow : EditorWindow
    {
        private const string MenuPath = "Tools/Auto Era Art/Batch Import";
        private const string LastSourceFolderKey = "AutoEra.ArtBatchImport.SourceFolder";
        private const string LastImportKey = "AutoEra.ArtBatchImport.LastImport";
        private const int MaximumRequirementsPerDocument = 10;

        private readonly List<ArtImportDocument> _documents = new List<ArtImportDocument>();
        private readonly List<string> _sourceFiles = new List<string>();
        private Vector2 _scroll;
        private int _documentIndex;
        private string _sourceFolder;
        private string _status;
        private MessageType _statusType = MessageType.Info;
        private string _catalogStamp;
        private string _catalogSummary;
        private bool _catalogError;
        private double _nextCatalogCheck;

        [MenuItem(MenuPath)]
        private static void Open()
        {
            GetWindow<ArtBatchImportWindow>("美术批量导入");
        }

        private ArtImportDocument SelectedDocument =>
            _documentIndex >= 0 && _documentIndex < _documents.Count ? _documents[_documentIndex] : null;

        private void OnEnable()
        {
            _sourceFolder = EditorPrefs.GetString(LastSourceFolderKey, string.Empty);
            RefreshCatalog();
        }

        private void OnFocus()
        {
            RefreshCatalog();
        }

        private void OnInspectorUpdate()
        {
            if (EditorApplication.timeSinceStartup < _nextCatalogCheck)
                return;
            _nextCatalogCheck = EditorApplication.timeSinceStartup + 1d;
            try
            {
                if (_catalogError || ArtImportCatalog.GetChangeStamp(GetProjectRoot()) != _catalogStamp)
                    RefreshCatalog();
            }
            catch (Exception exception)
            {
                _catalogError = true;
                SetStatus($"文档同步失败：{exception.Message}", MessageType.Error);
            }
        }

        private void OnGUI()
        {
            DrawToolbar();
            if (_documents.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "没有找到可导入的美术生产文档。请确认 Docs/GameDesign/11-美术规范 仍在项目根目录。",
                    MessageType.Warning);
                return;
            }

            DrawDocumentSelector();
            ArtImportDocument document = SelectedDocument;
            if (document == null)
                return;

            if (document.Requirements.Count > MaximumRequirementsPerDocument)
            {
                EditorGUILayout.HelpBox(
                    $"该文档包含 {document.Requirements.Count} 项需求，超过单批上限 {MaximumRequirementsPerDocument}，已禁止导入。请拆分文档后刷新索引。",
                    MessageType.Error);
                return;
            }

            DrawSourceSelector();
            DrawPreview(document);
            DrawActions(document);
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("刷新文档索引", EditorStyles.toolbarButton, GUILayout.Width(100)))
                    RefreshCatalog();

                GUILayout.Label(_catalogSummary ?? "文档尚未同步", EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("撤销上次导入", EditorStyles.toolbarButton, GUILayout.Width(100)))
                    UndoLastImport();
            }
        }

        private void DrawDocumentSelector()
        {
            string[] names = _documents.Select(x => x.DisplayName).ToArray();
            int newIndex = EditorGUILayout.Popup("生成导入文档", _documentIndex, names);
            if (newIndex != _documentIndex)
            {
                _documentIndex = newIndex;
                _sourceFiles.Clear();
                _status = string.Empty;
            }

            ArtImportDocument document = SelectedDocument;
            if (document == null)
                return;

            EditorGUILayout.LabelField("文档路径", document.RelativeDocumentPath);
            EditorGUILayout.LabelField("需求数量", document.Requirements.Count.ToString());
            if (!string.IsNullOrEmpty(document.Notes))
                EditorGUILayout.HelpBox(document.Notes, MessageType.Info);
        }

        private void DrawSourceSelector()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("生成资源目录", GUILayout.Width(90));
                EditorGUILayout.SelectableLabel(
                    string.IsNullOrEmpty(_sourceFolder) ? "尚未选择" : _sourceFolder,
                    EditorStyles.textField,
                    GUILayout.Height(EditorGUIUtility.singleLineHeight));
                if (GUILayout.Button("选择", GUILayout.Width(55)))
                {
                    string selected = EditorUtility.OpenFolderPanel("选择本次生成的美术资源目录", _sourceFolder, string.Empty);
                    if (!string.IsNullOrEmpty(selected))
                    {
                        _sourceFolder = selected;
                        EditorPrefs.SetString(LastSourceFolderKey, _sourceFolder);
                        ScanSourceFiles();
                    }
                }
                if (GUILayout.Button("扫描", GUILayout.Width(55)))
                    ScanSourceFiles();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("文件顺序", GUILayout.Width(90));
                EditorGUILayout.LabelField("自然排序（例如 -2 在 -10 前）");
                if (GUILayout.Button("重置排序", GUILayout.Width(80)))
                {
                    SortSourceFiles();
                }
            }
        }

        private void DrawPreview(ArtImportDocument document)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("导入预览", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                $"按左侧序号将源文件映射到文档需求。当前源文件 {_sourceFiles.Count} 个，文档需求 {document.Requirements.Count} 个。",
                _sourceFiles.Count == document.Requirements.Count ? MessageType.Info : MessageType.Warning);

            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MinHeight(180), GUILayout.MaxHeight(430));
            for (int i = 0; i < document.Requirements.Count; i++)
            {
                ArtImportRequirement requirement = document.Requirements[i];
                string source = i < _sourceFiles.Count ? Path.GetFileName(_sourceFiles[i]) : "（缺少源文件）";
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label((i + 1).ToString("D2"), GUILayout.Width(28));
                        GUILayout.Label(requirement.Id, GUILayout.Width(180));
                        GUILayout.Label("目标文件：" + requirement.TargetFileName, GUILayout.ExpandWidth(true));
                        if (i < _sourceFiles.Count && GUILayout.Button("↑", GUILayout.Width(24)))
                            MoveSource(i, -1);
                        if (i < _sourceFiles.Count && GUILayout.Button("↓", GUILayout.Width(24)))
                            MoveSource(i, 1);
                    }

                    // The source name gets its own full-width row so long ChatGPT filenames remain readable.
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label("源文件：", GUILayout.Width(55));
                        EditorGUILayout.SelectableLabel(
                            source,
                            EditorStyles.textField,
                            GUILayout.ExpandWidth(true),
                            GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label("目标路径：", GUILayout.Width(55));
                        EditorGUILayout.SelectableLabel(
                            requirement.TargetAssetPath,
                            EditorStyles.label,
                            GUILayout.ExpandWidth(true),
                            GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    }
                }
            }

            if (_sourceFiles.Count > document.Requirements.Count)
            {
                EditorGUILayout.HelpBox(
                    "源文件多于文档需求，多出的文件不会被导入；请调整目录内容或选择正确批次。",
                    MessageType.Error);
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawActions(ArtImportDocument document)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.enabled = _sourceFiles.Count == document.Requirements.Count && document.Requirements.Count > 0;
                if (GUILayout.Button("预览目标路径", GUILayout.Height(28)))
                    SetStatus(BuildPreflightMessage(document), MessageType.Info);
                if (GUILayout.Button("一键导入并重命名", GUILayout.Height(28)))
                    ImportDocument(document);
                GUI.enabled = true;
            }

            if (!string.IsNullOrEmpty(_status))
                EditorGUILayout.HelpBox(_status, _statusType);
        }

        private void RefreshCatalog()
        {
            try
            {
                string root = GetProjectRoot();
                // Capture before reading: a concurrent document edit is detected on the next check.
                string stamp = ArtImportCatalog.GetChangeStamp(root);
                IReadOnlyList<ArtImportDocument> loaded = ArtImportCatalog.Load(root);
                ArtImportDocument previous = SelectedDocument;
                _documents.Clear();
                _documents.AddRange(loaded);
                int selectedIndex = _documents.FindIndex(x =>
                    x.RelativeDocumentPath == previous?.RelativeDocumentPath);
                _documentIndex = selectedIndex >= 0 ? selectedIndex : 0;
                if (!ArtImportCatalog.HasSameMapping(previous, SelectedDocument))
                {
                    _sourceFiles.Clear();
                    _scroll = default;
                    SetStatus("文档映射已更新，请重新扫描并核对源文件顺序。", MessageType.Info);
                }
                _catalogStamp = stamp;
                _catalogError = false;
                _catalogSummary = $"已同步 {_documents.Count} 份文档 · {DateTime.Now:HH:mm:ss}";
                Repaint();
            }
            catch (Exception exception)
            {
                _catalogError = true;
                _catalogSummary = "文档同步失败";
                SetStatus($"文档同步失败：{exception.Message}", MessageType.Error);
            }
        }

        private void ScanSourceFiles()
        {
            _sourceFiles.Clear();
            if (string.IsNullOrEmpty(_sourceFolder) || !Directory.Exists(_sourceFolder))
            {
                SetStatus("源目录不存在，请重新选择。", MessageType.Warning);
                return;
            }

            string[] allowedExtensions = { ".png", ".jpg", ".jpeg", ".webp", ".tga", ".psd", ".gif" };
            _sourceFiles.AddRange(Directory.GetFiles(_sourceFolder, "*.*", SearchOption.TopDirectoryOnly)
                .Where(x => allowedExtensions.Contains(Path.GetExtension(x), StringComparer.OrdinalIgnoreCase)));
            SortSourceFiles();
            SetStatus($"已扫描 {_sourceFiles.Count} 个图片文件。", MessageType.Info);
        }

        private void SortSourceFiles()
        {
            _sourceFiles.Sort(NaturalPathComparer.Instance);
        }

        private void MoveSource(int index, int offset)
        {
            int target = index + offset;
            if (index < 0 || index >= _sourceFiles.Count || target < 0 || target >= _sourceFiles.Count)
                return;
            string value = _sourceFiles[index];
            _sourceFiles.RemoveAt(index);
            _sourceFiles.Insert(target, value);
        }

        private string BuildPreflightMessage(ArtImportDocument document)
        {
            try
            {
                List<string> destinations = BuildDestinations(document);
                return $"预览通过：{destinations.Count} 个文件将导入到 Assets，并按文档顺序重命名。\n首个目标：{destinations[0]}";
            }
            catch (Exception exception)
            {
                return $"预览失败：{exception.Message}";
            }
        }

        private void ImportDocument(ArtImportDocument document)
        {
            try
            {
                List<string> destinations = BuildDestinations(document);
                string projectRoot = GetProjectRoot();
                List<string> importedAssets = new List<string>();

                AssetDatabase.StartAssetEditing();
                try
                {
                    for (int i = 0; i < document.Requirements.Count; i++)
                    {
                        string targetAbsolutePath = Path.Combine(projectRoot, destinations[i].Replace('/', Path.DirectorySeparatorChar));
                        string targetDirectory = Path.GetDirectoryName(targetAbsolutePath);
                        Directory.CreateDirectory(targetDirectory);
                        File.Copy(_sourceFiles[i], targetAbsolutePath, false);
                        importedAssets.Add(destinations[i]);
                    }
                }
                catch
                {
                    foreach (string assetPath in importedAssets)
                        AssetDatabase.DeleteAsset(assetPath);
                    throw;
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                }

                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                SaveLastImport(importedAssets);
                SetStatus($"导入完成：{importedAssets.Count} 个资源。可点击右上角“撤销上次导入”删除本次新增文件。", MessageType.Info);
            }
            catch (Exception exception)
            {
                SetStatus($"导入失败：{exception.Message}", MessageType.Error);
            }
        }

        private List<string> BuildDestinations(ArtImportDocument document)
        {
            // Docs lives outside Assets; Unity's asset refresh alone cannot invalidate this catalog.
            RefreshCatalog();
            if (_catalogError)
                throw new InvalidOperationException("文档同步失败，暂时不能导入，请先刷新文档索引。");
            if (!ArtImportCatalog.HasSameMapping(document, SelectedDocument))
                throw new InvalidOperationException("文档已修改或删除，导入映射已刷新，请重新扫描并核对源文件顺序。");
            if (document.Requirements.Count == 0 || document.Requirements.Count > MaximumRequirementsPerDocument)
                throw new InvalidOperationException("每批必须包含 1 至 10 项需求。");
            if (_sourceFiles.Count != document.Requirements.Count)
                throw new InvalidOperationException("源文件数量必须与文档需求数量完全一致。");

            string projectRoot = GetProjectRoot();
            HashSet<string> uniquePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<string> destinations = new List<string>(document.Requirements.Count);
            for (int i = 0; i < document.Requirements.Count; i++)
            {
                ArtImportRequirement requirement = document.Requirements[i];
                string relativePath = requirement.TargetAssetPath.Replace('\\', '/');
                if (!relativePath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"文档路径不是 Assets 路径：{relativePath}");

                string extension = Path.GetExtension(_sourceFiles[i]);
                string fileName = requirement.GetTargetFileName(extension);
                string destination = relativePath.TrimEnd('/') + "/" + fileName;
                string absolutePath = Path.Combine(projectRoot, destination.Replace('/', Path.DirectorySeparatorChar));
                if (!uniquePaths.Add(destination))
                    throw new InvalidDataException($"文档生成了重复目标路径：{destination}");
                if (File.Exists(absolutePath))
                    throw new IOException($"目标文件已存在，为避免覆盖请先处理：{destination}");
                destinations.Add(destination);
            }

            return destinations;
        }

        private void UndoLastImport()
        {
            string json = EditorPrefs.GetString(LastImportKey, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                SetStatus("没有可撤销的导入记录。", MessageType.Info);
                return;
            }

            LastImportState state = JsonUtility.FromJson<LastImportState>(json);
            if (state == null || state.assetPaths == null || state.assetPaths.Length == 0)
            {
                SetStatus("上次导入记录为空。", MessageType.Info);
                return;
            }

            int deleted = 0;
            foreach (string assetPath in state.assetPaths)
            {
                if (AssetDatabase.DeleteAsset(assetPath))
                    deleted++;
            }
            AssetDatabase.Refresh();
            EditorPrefs.DeleteKey(LastImportKey);
            SetStatus($"已撤销上次导入，删除 {deleted} 个新资源。", MessageType.Info);
        }

        private void SaveLastImport(List<string> assetPaths)
        {
            EditorPrefs.SetString(LastImportKey, JsonUtility.ToJson(new LastImportState { assetPaths = assetPaths.ToArray() }));
        }

        private void SetStatus(string message, MessageType type)
        {
            _status = message;
            _statusType = type;
            Repaint();
        }

        private static string GetProjectRoot()
        {
            // AutoEra also defines an Application namespace; qualify Unity's API explicitly.
            return Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, ".."));
        }

        [Serializable]
        private sealed class LastImportState
        {
            public string[] assetPaths;
        }
    }

    [Serializable]
    internal sealed class ArtImportDocument
    {
        public string DisplayName;
        public string RelativeDocumentPath;
        public string Notes;
        public readonly List<ArtImportRequirement> Requirements = new List<ArtImportRequirement>();
    }

    [Serializable]
    internal sealed class ArtImportRequirement
    {
        public string Id;
        public string DisplayName;
        public string FileNamePattern;
        public string TargetAssetPath;

        public string TargetFileName => GetTargetFileName(".png");

        public string GetTargetFileName(string sourceExtension)
        {
            string pattern = string.IsNullOrWhiteSpace(FileNamePattern)
                ? "NEW_" + Id.ToLowerInvariant() + "@generated.png"
                : FileNamePattern;
            pattern = pattern.Replace("<state>", "all-states")
                .Replace("<view>", "board")
                .Replace("<event>", "event")
                .Replace("<variant>", "variant")
                .Replace("<type>", "asset");
            pattern = pattern.Replace("/", "_").Replace("\\", "_");
            string requestedExtension = Path.GetExtension(pattern);
            if (string.IsNullOrEmpty(requestedExtension))
                pattern += sourceExtension;
            else if (!string.Equals(requestedExtension, sourceExtension, StringComparison.OrdinalIgnoreCase))
                pattern = pattern.Substring(0, pattern.Length - requestedExtension.Length) + sourceExtension;
            return SanitizeFileName(pattern);
        }

        private static string SanitizeFileName(string value)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars())
                value = value.Replace(invalid, '_');
            return value.Trim();
        }
    }

    internal static class ArtImportCatalog
    {
        private const string DocumentationRoot = "Docs/GameDesign/11-美术规范";
        // The UI effect-board delivery keeps the pre-revision page documents for
        // audit purposes. They are historical source material, not import jobs.
        private const string ArchivedSourceDirectory = "Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图/制作记录/源文档/";
        // A target folder may contain Chinese page names, so stop at markdown/table delimiters
        // instead of restricting the path to ASCII characters.
        private static readonly Regex MarkdownPath = new Regex(@"(Assets/Game/[^\s`|)）]+)", RegexOptions.Compiled);
        private static readonly Regex HeadingId = new Regex(@"\b(?:3D-)?(?:IND|FX|AUD|ICO|SCR|SHR)-[A-Z0-9-]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string GetChangeStamp(string projectRoot)
        {
            string root = Path.Combine(projectRoot, DocumentationRoot);
            if (!Directory.Exists(root))
                return string.Empty;
            StringBuilder stamp = new StringBuilder();
            foreach (string path in Directory.GetFiles(root, "*.md", SearchOption.AllDirectories)
                         .Where(path => !IsArchivedSourceDocument(path, projectRoot))
                         .OrderBy(x => x, StringComparer.Ordinal))
            {
                FileInfo file = new FileInfo(path);
                stamp.Append(path).Append('|').Append(file.Length).Append('|')
                    .Append(file.LastWriteTimeUtc.Ticks).Append('\n');
            }
            return stamp.ToString();
        }

        public static bool HasSameMapping(ArtImportDocument left, ArtImportDocument right)
        {
            if (left == null || right == null)
                return left == right;
            if (left.RelativeDocumentPath != right.RelativeDocumentPath ||
                left.Requirements.Count != right.Requirements.Count)
                return false;
            for (int i = 0; i < left.Requirements.Count; i++)
            {
                ArtImportRequirement a = left.Requirements[i];
                ArtImportRequirement b = right.Requirements[i];
                if (a.Id != b.Id || a.FileNamePattern != b.FileNamePattern || a.TargetAssetPath != b.TargetAssetPath)
                    return false;
            }
            return true;
        }

        public static IReadOnlyList<ArtImportDocument> Load(string projectRoot)
        {
            string documentationRoot = Path.Combine(projectRoot, DocumentationRoot.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(documentationRoot))
                return Array.Empty<ArtImportDocument>();

            List<ArtImportDocument> result = new List<ArtImportDocument>();
            foreach (string file in Directory.GetFiles(documentationRoot, "*.md", SearchOption.AllDirectories)
                         .Where(file => !IsArchivedSourceDocument(file, projectRoot))
                         .OrderBy(x => x, NaturalPathComparer.Instance))
            {
                string relative = ToProjectRelative(file, projectRoot);
                ArtImportDocument document = ParseDocument(file, relative);
                if (document != null && document.Requirements.Count > 0)
                    result.Add(document);
            }
            return result;
        }

        private static bool IsArchivedSourceDocument(string absolutePath, string projectRoot)
        {
            string relative = ToProjectRelative(absolutePath, projectRoot).Replace('\\', '/');
            return relative.StartsWith(ArchivedSourceDirectory, StringComparison.OrdinalIgnoreCase);
        }

        private static ArtImportDocument ParseDocument(string file, string relativePath)
        {
            string[] lines = File.ReadAllLines(file, Encoding.UTF8);
            ArtImportDocument document = new ArtImportDocument
            {
                DisplayName = relativePath.Substring(DocumentationRoot.Length).TrimStart('/', '\\'),
                RelativeDocumentPath = relativePath
            };
            HashSet<string> ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            ParseTables(lines, document, ids);
            ParseHeadings(lines, document, ids);
            if (document.Requirements.Count == 0 || IsIndexDocument(document, lines))
                return null;
            if (document.Requirements.Count > 10)
                document.Notes = "此文档超过单批 10 项限制，导入已禁用，请使用对应批次文档。";
            return document;
        }

        private static bool IsIndexDocument(ArtImportDocument document, string[] lines)
        {
            string normalized = string.Join("\n", lines);
            return normalized.Contains("批次索引") && !document.RelativeDocumentPath.Contains("/批次/") &&
                   !document.RelativeDocumentPath.Contains("\\批次\\");
        }

        private static void ParseTables(string[] lines, ArtImportDocument document, HashSet<string> ids)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (!IsTableLine(lines[i]) || !lines[i].Contains("ID", StringComparison.OrdinalIgnoreCase))
                    continue;

                List<string> headers = SplitTableLine(lines[i]);
                int idIndex = FindHeader(headers, "ID");
                int pathIndex = FindHeader(headers, "存放路径", "目标路径");
                if (idIndex < 0 || pathIndex < 0)
                    continue;
                // Explicit filename columns win regardless of their position in the table.
                int fileIndex = FindHeader(headers, "文件名");
                if (fileIndex < 0)
                    fileIndex = FindHeader(headers, "状态级文件");
                if (fileIndex < 0)
                    fileIndex = idIndex;
                int nameIndex = FindHeader(headers, "资源名称", "素材", "资源");
                int methodIndex = FindHeader(headers, "生成方式");

                for (i++; i < lines.Length && IsTableLine(lines[i]); i++)
                {
                    List<string> cells = SplitTableLine(lines[i]);
                    if (cells.Count <= Math.Max(Math.Max(idIndex, pathIndex), fileIndex))
                        continue;
                    string id = Clean(cells[idIndex]);
                    string path = ExtractAssetPath(cells[pathIndex]);
                    if (string.IsNullOrEmpty(id) || id == "---" || string.IsNullOrEmpty(path) || path == "—")
                        continue;
                    if (cells[fileIndex].Contains("程序绘制", StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (methodIndex >= 0 && cells.Count > methodIndex &&
                        cells[methodIndex].Contains("程序绘制", StringComparison.OrdinalIgnoreCase))
                        continue;
                    string filePattern = fileIndex == idIndex ? string.Empty : CleanCode(cells[fileIndex]);
                    // A state list such as normal/hover is not a filename.
                    if (!Regex.IsMatch(filePattern, @"\.(png|jpe?g|webp|tga|psd|gif)$", RegexOptions.IgnoreCase))
                        filePattern = string.Empty;
                    Add(document, ids, new ArtImportRequirement
                    {
                        Id = id,
                        DisplayName = nameIndex >= 0 && cells.Count > nameIndex ? CleanCode(cells[nameIndex]) : id,
                        FileNamePattern = filePattern,
                        TargetAssetPath = path
                    });
                }
                i--;
            }
        }

        private static void ParseHeadings(string[] lines, ArtImportDocument document, HashSet<string> ids)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i].StartsWith("### ", StringComparison.Ordinal))
                    continue;
                Match match = HeadingId.Match(lines[i]);
                if (!match.Success)
                    continue;

                string filePattern = string.Empty;
                string path = string.Empty;
                int end = Math.Min(lines.Length, i + 24);
                for (int j = i + 1; j < end && !lines[j].StartsWith("### ", StringComparison.Ordinal); j++)
                {
                    if (lines[j].Contains("文件命名", StringComparison.OrdinalIgnoreCase))
                        filePattern = CleanCode(lines[j].Substring(lines[j].IndexOf('：') >= 0 ? lines[j].IndexOf('：') + 1 : 0));
                    if (lines[j].Contains("存放路径", StringComparison.OrdinalIgnoreCase))
                        path = ExtractAssetPath(lines[j]);
                }
                if (!string.IsNullOrEmpty(path))
                {
                    Add(document, ids, new ArtImportRequirement
                    {
                        Id = match.Value,
                        DisplayName = Clean(lines[i].Substring(match.Index + match.Length)),
                        FileNamePattern = filePattern,
                        TargetAssetPath = path
                    });
                }
            }
        }

        private static void Add(ArtImportDocument document, HashSet<string> ids, ArtImportRequirement requirement)
        {
            string key = requirement.Id + "|" + requirement.TargetAssetPath + "|" + requirement.FileNamePattern;
            if (ids.Add(key))
                document.Requirements.Add(requirement);
        }

        private static int FindHeader(List<string> headers, params string[] candidates)
        {
            for (int i = 0; i < headers.Count; i++)
                if (candidates.Any(x => headers[i].Contains(x, StringComparison.OrdinalIgnoreCase)))
                    return i;
            return -1;
        }

        private static bool IsTableLine(string line)
        {
            return line.TrimStart().StartsWith("|", StringComparison.Ordinal) && line.TrimEnd().EndsWith("|", StringComparison.Ordinal);
        }

        private static List<string> SplitTableLine(string line)
        {
            return line.Trim().Trim('|').Split('|').Select(x => x.Trim()).ToList();
        }

        private static string ExtractAssetPath(string text)
        {
            Match match = MarkdownPath.Match(CleanCode(text));
            return match.Success ? match.Groups[1].Value.TrimEnd('/', '\\') + "/" : string.Empty;
        }

        private static string CleanCode(string text)
        {
            int start = text.IndexOf('`');
            if (start >= 0)
            {
                int end = text.IndexOf('`', start + 1);
                if (end > start)
                    return text.Substring(start + 1, end - start - 1).Trim();
            }
            return text.Trim().Trim('：', ':', '。');
        }

        private static string Clean(string text)
        {
            return CleanCode(text).Trim('[', ']', '`', '*', ' ');
        }

        private static string ToProjectRelative(string absolutePath, string projectRoot)
        {
            return absolutePath.Substring(projectRoot.Length + 1).Replace('\\', '/');
        }
    }

    internal sealed class NaturalPathComparer : IComparer<string>
    {
        public static readonly NaturalPathComparer Instance = new NaturalPathComparer();
        private static readonly Regex Number = new Regex(@"(\d+)", RegexOptions.Compiled);

        public int Compare(string left, string right)
        {
            string[] a = Number.Split(Path.GetFileName(left));
            string[] b = Number.Split(Path.GetFileName(right));
            for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
            {
                int valueA;
                int valueB;
                if (int.TryParse(a[i], out valueA) && int.TryParse(b[i], out valueB))
                {
                    int numberCompare = valueA.CompareTo(valueB);
                    if (numberCompare != 0)
                        return numberCompare;
                }
                else
                {
                    int textCompare = StringComparer.OrdinalIgnoreCase.Compare(a[i], b[i]);
                    if (textCompare != 0)
                        return textCompare;
                }
            }
            return a.Length.CompareTo(b.Length);
        }
    }
}
