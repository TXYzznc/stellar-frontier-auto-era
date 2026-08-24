#if UNITY_EDITOR
using GameFramework.Editor.DataTableTools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UGF.EditorTools
{
    public enum AIDataKind
    {
        DataTable,
        Config,
        Language,
    }

    [Serializable]
    public sealed class AIDataSyncManifest
    {
        public const int CurrentSchemaVersion = 1;

        public int schemaVersion = CurrentSchemaVersion;
        public string kind;
        public string relativePath;
        public string sourceFingerprint;
    }

    [Serializable]
    public sealed class AIDataSyncReportItem
    {
        public string kind;
        public string relativePath;
        public string baselineFingerprint;
        public string currentFingerprint;
        public int changedCellCount;
        public int changedRowCount;
        public bool rollbackSucceeded;
        public List<string> warnings = new List<string>();
        public List<string> errors = new List<string>();
    }

    [Serializable]
    public sealed class AIDataGenerationProfiles
    {
        public const int CurrentSchemaVersion = 1;

        public int schemaVersion = CurrentSchemaVersion;
        public List<AIDataTableGenerationProfileDefinition> dataTables = new List<AIDataTableGenerationProfileDefinition>();
    }

    [Serializable]
    public sealed class AIDataTableGenerationProfileDefinition
    {
        public string sourceRelativePath;
        public string codeOutputRoot;

        [JsonProperty("namespace")]
        public string codeNamespace;
    }

    /// <summary>
    /// Loads the project-owned, Editor-only generation profile.  The framework keeps this
    /// data format generic; product paths and namespaces live exclusively in the json file.
    /// </summary>
    public static class AIDataGenerationProfileLoader
    {
        [InitializeOnLoadMethod]
        private static void LoadProfilesAfterDomainReload()
        {
            ReloadDataTableProfiles();
        }

        public static bool ReloadDataTableProfiles()
        {
            if (!TryLoadDataTableProfiles(out var profiles, out var errors))
            {
                GameDataGenerator.SetDataTableCodeGenerationProfiles(null);
                Debug.LogError($"AI data generation profiles were rejected: {string.Join(" | ", errors)}");
                return false;
            }

            GameDataGenerator.SetDataTableCodeGenerationProfiles(profiles);
            return true;
        }

        public static bool TryLoadDataTableProfiles(
            out IReadOnlyList<GameDataGenerator.DataTableCodeGenerationProfile> profiles,
            out List<string> errors)
        {
            profiles = Array.Empty<GameDataGenerator.DataTableCodeGenerationProfile>();
            errors = new List<string>();
            if (!File.Exists(ConstEditor.AIDataGenerationProfilePath))
            {
                return true;
            }

            try
            {
                return TryParseDataTableProfiles(File.ReadAllText(ConstEditor.AIDataGenerationProfilePath), out profiles, out errors);
            }
            catch (Exception exception)
            {
                errors.Add($"Cannot read generation profiles: {exception.Message}");
                return false;
            }
        }

        public static bool TryParseDataTableProfiles(
            string json,
            out IReadOnlyList<GameDataGenerator.DataTableCodeGenerationProfile> profiles,
            out List<string> errors)
        {
            profiles = Array.Empty<GameDataGenerator.DataTableCodeGenerationProfile>();
            errors = new List<string>();
            if (string.IsNullOrWhiteSpace(json))
            {
                errors.Add("Generation profile json is empty.");
                return false;
            }

            AIDataGenerationProfiles document;
            try
            {
                document = JsonConvert.DeserializeObject<AIDataGenerationProfiles>(json);
            }
            catch (JsonException exception)
            {
                errors.Add($"Generation profile json is invalid: {exception.Message}");
                return false;
            }

            if (document == null)
            {
                errors.Add("Generation profile json has no document.");
                return false;
            }

            if (document.schemaVersion != AIDataGenerationProfiles.CurrentSchemaVersion)
            {
                errors.Add($"Unsupported generation profile schema version: {document.schemaVersion}.");
            }

            var result = new List<GameDataGenerator.DataTableCodeGenerationProfile>();
            var sourcePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (AIDataTableGenerationProfileDefinition definition in document.dataTables ?? Enumerable.Empty<AIDataTableGenerationProfileDefinition>())
            {
                if (definition == null)
                {
                    errors.Add("Generation profile cannot be null.");
                    continue;
                }

                try
                {
                    var profile = new GameDataGenerator.DataTableCodeGenerationProfile(
                        definition.sourceRelativePath,
                        definition.codeOutputRoot,
                        definition.codeNamespace);
                    string validationError;
                    if (!profile.IsValid(out validationError))
                    {
                        errors.Add($"Invalid generation profile '{definition.sourceRelativePath}': {validationError}");
                        continue;
                    }

                    if (!sourcePaths.Add(profile.SourceRelativePath))
                    {
                        errors.Add($"Duplicate generation profile source path: {profile.SourceRelativePath}");
                        continue;
                    }

                    result.Add(profile);
                }
                catch (ArgumentException exception)
                {
                    errors.Add(exception.Message);
                }
            }

            if (errors.Count > 0)
            {
                return false;
            }

            profiles = result;
            return true;
        }
    }
}
#endif
