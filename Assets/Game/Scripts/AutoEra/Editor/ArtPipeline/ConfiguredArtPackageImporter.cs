using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor.ArtPipeline
{
    internal static class ConfiguredArtPackageImporter
    {
        private const string ConfigRelativePath = "Library/AutoEraArtPackageImport.txt";
        private const string MenuPath = "Tools/Auto Era Art/Import Configured Package";

        [MenuItem(MenuPath)]
        private static void Import()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string configPath = Path.Combine(projectRoot, ConfigRelativePath);
            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException(
                    $"Art package import config is missing: {ConfigRelativePath}",
                    configPath);
            }

            string packagePath = File.ReadAllText(configPath).Trim();
            if (!string.Equals(Path.GetExtension(packagePath), ".unitypackage", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("Configured art package must use the .unitypackage extension.");
            }

            packagePath = Path.GetFullPath(packagePath);
            if (!File.Exists(packagePath))
            {
                throw new FileNotFoundException("Configured art package does not exist.", packagePath);
            }

            AssetDatabase.ImportPackage(packagePath, false);
            Debug.Log($"ART_PACKAGE_IMPORT_OK|file={Path.GetFileName(packagePath)}");
        }
    }
}
