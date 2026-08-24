#if UNITY_EDITOR
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UGF.EditorTools
{
    [Serializable]
    public sealed class AIConfigManifest
    {
        public const int CurrentSchemaVersion = 1;
        public const string Kind = "GF_X.Config.AI";

        public int schemaVersion = CurrentSchemaVersion;
        public string kind = Kind;
        public string relativePath;
        public string sourceFingerprint;
        public List<AIConfigEntry> entries = new List<AIConfigEntry>();
    }

    [Serializable]
    public sealed class AIConfigEntry
    {
        public string key;
        public string comment;
        public string value;
    }

    public static class AIConfigAdapter
    {
        public static bool TryParseManifest(string json, out AIConfigManifest manifest, out List<string> errors)
        {
            manifest = null;
            errors = new List<string>();
            try
            {
                manifest = JsonConvert.DeserializeObject<AIConfigManifest>(json);
            }
            catch (JsonException exception)
            {
                errors.Add($"Config JSON is invalid: {exception.Message}");
                return false;
            }

            if (manifest == null)
            {
                errors.Add("Config JSON has no manifest.");
                return false;
            }

            if (manifest.schemaVersion != AIConfigManifest.CurrentSchemaVersion)
            {
                errors.Add($"Unsupported config schema version: {manifest.schemaVersion}.");
            }

            if (!string.Equals(manifest.kind, AIConfigManifest.Kind, StringComparison.Ordinal))
            {
                errors.Add($"Unexpected config manifest kind: {manifest.kind}.");
            }

            if (!AIDataSyncPipeline.TryNormalizeRelativePath(manifest.relativePath, out manifest.relativePath, out string pathError))
            {
                errors.Add($"Config relative path is invalid: {pathError}");
            }

            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (AIConfigEntry entry in manifest.entries ?? new List<AIConfigEntry>())
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.key))
                {
                    errors.Add("Config entries require a non-empty key.");
                    continue;
                }

                if (!keys.Add(entry.key))
                {
                    errors.Add($"Duplicate config key: {entry.key}.");
                }
            }

            return errors.Count == 0;
        }
    }
}
#endif
