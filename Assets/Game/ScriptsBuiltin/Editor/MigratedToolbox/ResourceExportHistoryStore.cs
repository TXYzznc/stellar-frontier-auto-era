using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// Keeps local-only export snapshots so ignored source assets can be compared without Git.
/// </summary>
public sealed class ResourceExportHistoryStore
{
    private const int MaximumSnapshots = 50;
    private readonly string _projectRoot;
    private readonly string _historyPath;

    public ResourceExportHistoryStore(string projectRoot)
    {
        _projectRoot = projectRoot;
        _historyPath = Path.Combine(projectRoot, "Library", "MigratedToolbox", "ResourceExportHistory.json");
    }

    public List<ResourceExportSnapshot> LoadSnapshots()
    {
        if (!File.Exists(_historyPath))
            return new List<ResourceExportSnapshot>();

        try
        {
            var history = JsonUtility.FromJson<ResourceExportHistory>(File.ReadAllText(_historyPath));
            return history?.snapshots ?? new List<ResourceExportSnapshot>();
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[ResourceExport] 无法读取本地导出历史，将创建新历史：{exception.Message}");
            return new List<ResourceExportSnapshot>();
        }
    }

    public ResourceExportSnapshot SaveSnapshot(string displayName, IEnumerable<string> assetPaths)
    {
        var snapshot = new ResourceExportSnapshot
        {
            id = Guid.NewGuid().ToString("N"),
            createdUtc = DateTime.UtcNow.ToString("O"),
            displayName = displayName,
            entries = BuildEntries(assetPaths)
        };

        List<ResourceExportSnapshot> snapshots = LoadSnapshots();
        snapshots.Insert(0, snapshot);
        if (snapshots.Count > MaximumSnapshots)
            snapshots.RemoveRange(MaximumSnapshots, snapshots.Count - MaximumSnapshots);

        string directory = Path.GetDirectoryName(_historyPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        File.WriteAllText(_historyPath, JsonUtility.ToJson(new ResourceExportHistory { snapshots = snapshots }, true));
        return snapshot;
    }

    public ResourceExportChangeSet Compare(ResourceExportSnapshot baseline, IEnumerable<string> currentPaths)
    {
        Dictionary<string, string> current = BuildEntries(currentPaths)
            .ToDictionary(entry => entry.assetPath, entry => entry.contentHash, StringComparer.Ordinal);
        Dictionary<string, string> previous = (baseline?.entries ?? new List<ResourceExportSnapshotEntry>())
            .ToDictionary(entry => entry.assetPath, entry => entry.contentHash, StringComparer.Ordinal);

        var result = new ResourceExportChangeSet { baselineId = baseline?.id };
        foreach (var pair in current)
        {
            if (!previous.TryGetValue(pair.Key, out string previousHash))
                result.added.Add(pair.Key);
            else if (!string.Equals(previousHash, pair.Value, StringComparison.Ordinal))
                result.modified.Add(pair.Key);
        }

        foreach (string path in previous.Keys)
        {
            if (!current.ContainsKey(path))
                result.deleted.Add(path);
        }

        result.Sort();
        return result;
    }

    private List<ResourceExportSnapshotEntry> BuildEntries(IEnumerable<string> assetPaths)
    {
        var entries = new List<ResourceExportSnapshotEntry>();
        foreach (string assetPath in (assetPaths ?? Enumerable.Empty<string>()).Distinct(StringComparer.Ordinal).OrderBy(path => path, StringComparer.Ordinal))
        {
            string fullPath = Path.Combine(_projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullPath))
                continue;

            entries.Add(new ResourceExportSnapshotEntry
            {
                assetPath = assetPath,
                contentHash = CalculateHash(fullPath)
            });
        }
        return entries;
    }

    private static string CalculateHash(string path)
    {
        using var algorithm = SHA256.Create();
        using var stream = File.OpenRead(path);
        return BitConverter.ToString(algorithm.ComputeHash(stream)).Replace("-", string.Empty);
    }
}

[Serializable]
public sealed class ResourceExportHistory
{
    public List<ResourceExportSnapshot> snapshots = new();
}

[Serializable]
public sealed class ResourceExportSnapshot
{
    public string id;
    public string createdUtc;
    public string displayName;
    public List<ResourceExportSnapshotEntry> entries = new();
}

[Serializable]
public sealed class ResourceExportSnapshotEntry
{
    public string assetPath;
    public string contentHash;
}

public sealed class ResourceExportChangeSet
{
    public string baselineId;
    public readonly List<string> added = new();
    public readonly List<string> modified = new();
    public readonly List<string> deleted = new();

    public IEnumerable<string> ExportablePaths => added.Concat(modified);

    public void Sort()
    {
        added.Sort(StringComparer.Ordinal);
        modified.Sort(StringComparer.Ordinal);
        deleted.Sort(StringComparer.Ordinal);
    }
}
