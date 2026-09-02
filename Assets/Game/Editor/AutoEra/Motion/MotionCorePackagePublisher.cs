using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor.Motion
{
    /// <summary>Publishes the Motion Core source as a self-contained UPM package while preserving source meta GUIDs.</summary>
    public static class MotionCorePackagePublisher
    {
        public const string PackageId = "com.autoera.motion-core";
        public const string Version = "1.1.0";
        public const string OutputRelativePath = "Tools/Exports/AutoEra.MotionCore-1.1.0";

        private static readonly string[] RuntimeFiles =
        {
            "Assets/Game/Scripts/AutoEra/Motion/MotionRig.cs",
            "Assets/Game/Scripts/AutoEra/Motion/MotionGraphAsset.cs",
            "Assets/Game/Scripts/AutoEra/Motion/MotionGraphComposition.cs",
            "Assets/Game/Scripts/AutoEra/Motion/MotionExecutor.cs",
            "Assets/Game/Scripts/AutoEra/Motion/MotionInterruption.cs",
            "Assets/Game/Scripts/AutoEra/Motion/MotionParameterContext.cs",
            "Assets/Game/Scripts/AutoEra/Motion/MotionPrimitives.cs",
            "Assets/Game/Scripts/AutoEra/Motion/MotionPresentationLeasePool.cs",
            "Assets/Game/Scripts/AutoEra/Motion/MotionPresentationUpdateLevel.cs",
            "Assets/Game/Scripts/AutoEra/Motion/FourWheelPresentation.cs",
            "Assets/Game/Scripts/AutoEra/Motion/ArmPresentationSolver.cs",
            "Assets/Game/Scripts/AutoEra/Motion/EffectorPresentationSequence.cs",
            "Assets/Game/Scripts/AutoEra/Motion/DoorAndConveyorPresentation.cs",
            "Assets/Game/Scripts/AutoEra/Motion/FunctionalRigPrototypeHierarchy.cs",
            "Assets/Game/Scripts/AutoEra/Motion/FunctionalRigPrototypeStableId.cs",
            "Assets/Game/Scripts/AutoEra/Motion/FunctionalRigPrototypeCatalog.cs",
            "Assets/Game/Scripts/AutoEra/Motion/Contracts/FunctionalRigContract.cs",
            "Assets/Game/Scripts/AutoEra/Motion/Contracts/FunctionalRigContractJson.cs"
        };

        private static readonly string[] EditorFiles =
        {
            "Assets/Game/Editor/AutoEra/Motion/MotionAuthoringInspectors.cs",
            "Assets/Game/Editor/AutoEra/Motion/MotionRigPreviewUtility.cs",
            "Assets/Game/Editor/AutoEra/Motion/MotionStaticValidator.cs",
            "Assets/Game/Editor/AutoEra/Motion/FunctionalRigPrototypeBuilder.cs",
            "Assets/Game/Editor/AutoEra/Motion/FunctionalRigPrototypeCatalogBuilder.cs",
            "Assets/Game/Editor/AutoEra/Motion/FunctionalRigPrototypeHierarchyTemplateBuilder.cs",
            "Assets/Game/Editor/AutoEra/Motion/FunctionalRigPrototypeStructureValidator.cs",
            "Assets/Game/Editor/AutoEra/Motion/FunctionalRigPrototypeValidationPanel.cs"
        };

        [MenuItem("AutoEra/Motion/Publish Motion Core Package")]
        public static void PublishFromMenu()
        {
            Publish();
            Debug.Log("[AutoEra.Motion] Package published: " + OutputRelativePath);
        }

        public static string Publish()
        {
            string root = Path.GetFullPath(OutputRelativePath);
            if (Directory.Exists(root)) Directory.Delete(root, true);
            Directory.CreateDirectory(root);
            CopyFiles(root, "Runtime", RuntimeFiles);
            CopyFiles(root, "Editor", EditorFiles);
            WriteText(root, "package.json", "{\n  \"name\": \"" + PackageId + "\",\n  \"version\": \"" + Version + "\",\n  \"displayName\": \"AutoEra Motion Core\",\n  \"description\": \"Functional prototype motion authoring and deterministic runtime.\",\n  \"unity\": \"2022.3\"\n}\n");
            WriteText(root, "Runtime/AutoEra.MotionCore.Runtime.asmdef", "{\n  \"name\": \"AutoEra.MotionCore.Runtime\",\n  \"rootNamespace\": \"AutoEra.Motion\",\n  \"references\": [],\n  \"includePlatforms\": [],\n  \"excludePlatforms\": [],\n  \"allowUnsafeCode\": false,\n  \"overrideReferences\": false,\n  \"autoReferenced\": true\n}\n");
            WriteText(root, "Editor/AutoEra.MotionCore.Editor.asmdef", "{\n  \"name\": \"AutoEra.MotionCore.Editor\",\n  \"rootNamespace\": \"AutoEra.Editor.Motion\",\n  \"references\": [\"AutoEra.MotionCore.Runtime\"],\n  \"includePlatforms\": [\"Editor\"],\n  \"excludePlatforms\": [],\n  \"allowUnsafeCode\": false,\n  \"overrideReferences\": false,\n  \"autoReferenced\": true\n}\n");
            WriteText(root, "README.md", "# AutoEra Motion Core\n\nVersion " + Version + ". Import this directory as a local UPM package. The package intentionally excludes AutoEra Adapter, gameplay, GF_X, scenes, art assets and spreadsheets.\n");
            ValidatePublishedPackage(root, out string error);
            if (error != null) throw new InvalidOperationException(error);
            return root;
        }

        public static bool ValidatePublishedPackage(string absoluteRoot, out string error)
        {
            if (!File.Exists(Path.Combine(absoluteRoot, "package.json")) || !File.Exists(Path.Combine(absoluteRoot, "Runtime/AutoEra.MotionCore.Runtime.asmdef")) || !File.Exists(Path.Combine(absoluteRoot, "Editor/AutoEra.MotionCore.Editor.asmdef")))
            {
                error = "Package manifest or assembly definitions are missing.";
                return false;
            }

            foreach (string source in RuntimeFiles) if (!ValidateCopiedFile(absoluteRoot, "Runtime", source, out error)) return false;
            foreach (string source in EditorFiles) if (!ValidateCopiedFile(absoluteRoot, "Editor", source, out error)) return false;
            string[] forbidden = { "Adapter", "ScriptsBuiltin", ".unity", ".xlsx", "GF_X" };
            foreach (string path in Directory.GetFiles(absoluteRoot, "*", SearchOption.AllDirectories))
            {
                foreach (string token in forbidden)
                {
                    if (path.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        error = "Package contains forbidden content: " + path;
                        return false;
                    }
                }
            }

            error = null;
            return true;
        }

        private static bool ValidateCopiedFile(string root, string category, string source, out string error)
        {
            string target = Path.Combine(root, category, Path.GetFileName(source));
            if (!File.Exists(target) || !File.Exists(target + ".meta"))
            {
                error = "Package is missing source or meta: " + source;
                return false;
            }

            string sourceMeta = File.ReadAllText(source + ".meta");
            string targetMeta = File.ReadAllText(target + ".meta");
            if (!string.Equals(sourceMeta, targetMeta, StringComparison.Ordinal))
            {
                error = "Package changed meta GUID: " + source;
                return false;
            }

            error = null;
            return true;
        }

        private static void CopyFiles(string root, string category, IEnumerable<string> sources)
        {
            string folder = Path.Combine(root, category);
            Directory.CreateDirectory(folder);
            foreach (string source in sources)
            {
                string target = Path.Combine(folder, Path.GetFileName(source));
                File.Copy(source, target, true);
                File.Copy(source + ".meta", target + ".meta", true);
            }
        }

        private static void WriteText(string root, string relativePath, string contents)
        {
            string fullPath = Path.Combine(root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllText(fullPath, contents);
        }
    }
}
