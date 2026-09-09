using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;

/// <summary>
/// Resolves project-root .gitignore patterns for assets that Unity can export.
/// It deliberately returns primary AssetDatabase paths only: Unity owns .meta inclusion.
/// </summary>
public static class GitIgnoreAssetGroupCollector
{
    public const string GeneratedGroupName = "Git 忽略资源";

    public static List<string> CollectIgnoredAssetPaths(string projectRoot)
    {
        if (string.IsNullOrWhiteSpace(projectRoot))
            throw new ArgumentException("项目根目录不能为空。", nameof(projectRoot));

        string ignoreFile = Path.Combine(projectRoot, ".gitignore");
        if (!File.Exists(ignoreFile))
            return new List<string>();

        string assetsDirectory = Path.Combine(projectRoot, "Assets");
        if (!Directory.Exists(assetsDirectory))
            return new List<string>();

        string[] rules = File.ReadAllLines(ignoreFile);
        var result = new List<string>();
        foreach (string filePath in Directory.GetFiles(assetsDirectory, "*", SearchOption.AllDirectories))
        {
            if (filePath.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                continue;

            string assetPath = ToProjectRelativePath(projectRoot, filePath);
            if (!IsIgnoredByGitIgnore(assetPath, rules))
                continue;

            if (AssetDatabase.LoadMainAssetAtPath(assetPath) != null)
                result.Add(assetPath);
        }

        result.Sort(StringComparer.Ordinal);
        return result;
    }

    /// <summary>
    /// Matches the subset of root .gitignore syntax needed by asset delivery:
    /// comments, negation, root anchoring, directory rules, *, **, ?, and character classes.
    /// The last matching rule wins, mirroring Git's ordinary precedence.
    /// </summary>
    public static bool IsIgnoredByGitIgnore(string projectRelativePath, IEnumerable<string> lines)
    {
        if (string.IsNullOrWhiteSpace(projectRelativePath))
            return false;

        string path = NormalizePath(projectRelativePath);
        bool ignored = false;
        foreach (string rawLine in lines ?? Enumerable.Empty<string>())
        {
            if (!TryParseRule(rawLine, out string pattern, out bool negate, out bool directoryOnly, out bool anchored))
                continue;

            if (!Matches(path, pattern, directoryOnly, anchored))
                continue;

            ignored = !negate;
        }

        return ignored;
    }

    private static bool TryParseRule(string rawLine, out string pattern, out bool negate, out bool directoryOnly, out bool anchored)
    {
        pattern = string.Empty;
        negate = false;
        directoryOnly = false;
        anchored = false;
        if (string.IsNullOrWhiteSpace(rawLine))
            return false;

        string line = rawLine.Trim();
        if (line.Length == 0 || line[0] == '#')
            return false;

        if (line.StartsWith("\\#", StringComparison.Ordinal))
            line = line.Substring(1);
        else if (line[0] == '!')
        {
            negate = true;
            line = line.Substring(1);
        }
        else if (line.StartsWith("\\!", StringComparison.Ordinal))
            line = line.Substring(1);

        if (line.Length == 0)
            return false;

        anchored = line[0] == '/';
        if (anchored)
            line = line.Substring(1);
        directoryOnly = line.EndsWith("/", StringComparison.Ordinal);
        if (directoryOnly)
            line = line.TrimEnd('/');
        pattern = NormalizePath(line);
        return pattern.Length > 0;
    }

    private static bool Matches(string path, string pattern, bool directoryOnly, bool anchored)
    {
        bool containsSlash = pattern.IndexOf('/') >= 0;
        string prefix = anchored || containsSlash ? "^" : "(^|.*/)";
        string suffix = directoryOnly ? "(/|$)" : "$";
        return Regex.IsMatch(path, prefix + GlobToRegex(pattern) + suffix, RegexOptions.CultureInvariant);
    }

    private static string GlobToRegex(string pattern)
    {
        var builder = new StringBuilder();
        for (int index = 0; index < pattern.Length; index++)
        {
            char current = pattern[index];
            if (current == '*')
            {
                bool isDouble = index + 1 < pattern.Length && pattern[index + 1] == '*';
                if (isDouble)
                {
                    bool includesSlash = index + 2 < pattern.Length && pattern[index + 2] == '/';
                    builder.Append(includesSlash ? "(.*/)?" : ".*");
                    index += includesSlash ? 2 : 1;
                }
                else
                {
                    builder.Append("[^/]*");
                }
                continue;
            }

            if (current == '?')
            {
                builder.Append("[^/]");
                continue;
            }

            if (current == '[')
            {
                int closeIndex = pattern.IndexOf(']', index + 1);
                if (closeIndex > index + 1)
                {
                    if (pattern[index + 1] == '!')
                        builder.Append("[^").Append(pattern, index + 2, closeIndex - index - 2).Append(']');
                    else
                        builder.Append(pattern, index, closeIndex - index + 1);
                    index = closeIndex;
                    continue;
                }
            }

            builder.Append(Regex.Escape(current.ToString()));
        }

        return builder.ToString();
    }

    private static string ToProjectRelativePath(string projectRoot, string filePath)
    {
        string relativePath = filePath.Substring(projectRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Length)
            .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return NormalizePath(relativePath);
    }

    private static string NormalizePath(string value)
    {
        return value.Replace('\\', '/').TrimStart('/');
    }
}
