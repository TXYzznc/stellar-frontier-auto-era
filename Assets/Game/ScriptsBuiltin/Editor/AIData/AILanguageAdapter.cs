#if UNITY_EDITOR
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UGF.EditorTools
{
    [Serializable]
    public sealed class AILanguageManifest
    {
        public const int CurrentSchemaVersion = 1;
        public const string Kind = "GF_X.Language.AI";

        public int schemaVersion = CurrentSchemaVersion;
        public string kind = Kind;
        public string relativePath;
        public string sourceFingerprint;
        public List<AILanguageEntry> entries = new List<AILanguageEntry>();
    }

    [Serializable]
    public sealed class AILanguageEntry
    {
        public string key;
        public string value;
    }

    public static class AILanguageAdapter
    {
        public static bool TryParseManifest(string json, out AILanguageManifest manifest, out List<string> errors)
        {
            manifest = null;
            errors = new List<string>();
            try
            {
                manifest = JsonConvert.DeserializeObject<AILanguageManifest>(json);
            }
            catch (JsonException exception)
            {
                errors.Add($"Language JSON is invalid: {exception.Message}");
                return false;
            }

            if (manifest == null)
            {
                errors.Add("Language JSON has no manifest.");
                return false;
            }

            if (manifest.schemaVersion != AILanguageManifest.CurrentSchemaVersion)
            {
                errors.Add($"Unsupported language schema version: {manifest.schemaVersion}.");
            }

            if (!string.Equals(manifest.kind, AILanguageManifest.Kind, StringComparison.Ordinal))
            {
                errors.Add($"Unexpected language manifest kind: {manifest.kind}.");
            }

            if (!AIDataSyncPipeline.TryNormalizeRelativePath(manifest.relativePath, out manifest.relativePath, out string pathError))
            {
                errors.Add($"Language relative path is invalid: {pathError}");
            }

            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (AILanguageEntry entry in manifest.entries ?? new List<AILanguageEntry>())
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.key))
                {
                    errors.Add("Language entries require a non-empty key.");
                    continue;
                }

                if (!keys.Add(entry.key))
                {
                    errors.Add($"Duplicate language key: {entry.key}.");
                }
            }

            return errors.Count == 0;
        }
    }
}
#endif
