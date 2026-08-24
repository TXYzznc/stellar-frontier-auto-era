using System;
using System.Collections.Generic;
using GameFramework.Editor.DataTableTools;
using NUnit.Framework;
using UGF.EditorTools;
using UnityGameFramework.Runtime;

namespace AutoEra.Tests.Editor
{
    public sealed class DataTableGenerationProfileEditModeTests
    {
        [TearDown]
        public void TearDown()
        {
            Assert.That(AIDataGenerationProfileLoader.ReloadDataTableProfiles(), Is.True);
        }

        [Test]
        public void UnconfiguredCoreTable_UsesDefaultCodeOutput()
        {
            Assert.That(GameDataGenerator.TryGetDataTableCodeGenerationProfile("Core/UITable", out _), Is.False);
        }

        [Test]
        public void ConfiguredProfile_UsesRelativeOutputAndNamespace()
        {
            var profile = new GameDataGenerator.DataTableCodeGenerationProfile("Project", "Assets/Game/Scripts/Project/DataTable", "Project.DataTable");
            GameDataGenerator.SetDataTableCodeGenerationProfiles(new[] { profile });

            Assert.That(GameDataGenerator.TryGetDataTableCodeGenerationProfile("Project/WorldSettings", out var matched), Is.True);
            Assert.That(matched, Is.SameAs(profile));
            Assert.That(GameDataGenerator.GetDataTableCodeOutputRelativePath("Project/WorldSettings", matched), Is.EqualTo("WorldSettings"));
        }

        [Test]
        public void MatchingRegistration_IsIdempotent()
        {
            var profile = new GameDataGenerator.DataTableCodeGenerationProfile("Project", "Assets/Game/Scripts/Project/DataTable", "Project.DataTable");
            GameDataGenerator.RegisterDataTableCodeGenerationProfile(profile);
            GameDataGenerator.RegisterDataTableCodeGenerationProfile(profile);

            Assert.That(GameDataGenerator.TryGetDataTableCodeGenerationProfile("Project/WorldSettings", out var matched), Is.True);
            Assert.That(matched, Is.SameAs(profile));
        }

        [Test]
        public void DataRowFallback_RejectsAmbiguousShortNames()
        {
            Type resolved = DataTableExtension.ResolveDataRowType("DuplicateRow", new[] { typeof(First.DuplicateRow), typeof(Second.DuplicateRow) });
            Assert.That(resolved, Is.Null);
        }

        [Test]
        public void DataRowFallback_RejectsMissingShortName()
        {
            Type resolved = DataTableExtension.ResolveDataRowType("MissingRow", Array.Empty<Type>());
            Assert.That(resolved, Is.Null);
        }

        [Test]
        public void DataRowFallback_AcceptsUniqueNamespacedRow()
        {
            Type resolved = DataTableExtension.ResolveDataRowType("UniqueRow", new[] { typeof(First.UniqueRow) });
            Assert.That(resolved, Is.EqualTo(typeof(First.UniqueRow)));
        }

        [Test]
        public void JsonProfile_ParsesFoundationOutputWithoutRuntimeRegistration()
        {
            const string json = "{\"schemaVersion\":1,\"dataTables\":[{\"sourceRelativePath\":\"Foundation\",\"codeOutputRoot\":\"Assets/Game/Scripts/AutoEra/DataTable\",\"namespace\":\"AutoEra.DataTable\"}]}";

            bool loaded = AIDataGenerationProfileLoader.TryParseDataTableProfiles(json, out var profiles, out var errors);

            Assert.That(loaded, Is.True, string.Join(" | ", errors));
            Assert.That(profiles, Has.Count.EqualTo(1));
            Assert.That(profiles[0].SourceRelativePath, Is.EqualTo("Foundation"));
            Assert.That(profiles[0].CodeOutputRoot, Is.EqualTo("Assets/Game/Scripts/AutoEra/DataTable"));
            Assert.That(profiles[0].Namespace, Is.EqualTo("AutoEra.DataTable"));
        }

        [Test]
        public void ProjectProfile_LoadsFoundationRuleFromEditorOnlyJson()
        {
            bool loaded = AIDataGenerationProfileLoader.TryLoadDataTableProfiles(out var profiles, out var errors);

            Assert.That(loaded, Is.True, string.Join(" | ", errors));
            Assert.That(profiles, Has.Count.EqualTo(1));
            Assert.That(profiles[0].SourceRelativePath, Is.EqualTo("Foundation"));
            Assert.That(profiles[0].CodeOutputRoot, Is.EqualTo("Assets/Game/Scripts/AutoEra/DataTable"));
            Assert.That(profiles[0].Namespace, Is.EqualTo("AutoEra.DataTable"));
        }

        [Test]
        public void JsonProfile_RejectsDuplicateSourcePaths()
        {
            const string json = "{\"schemaVersion\":1,\"dataTables\":[{\"sourceRelativePath\":\"Foundation\",\"codeOutputRoot\":\"Assets/Game/Scripts/A\",\"namespace\":\"A\"},{\"sourceRelativePath\":\"Foundation\",\"codeOutputRoot\":\"Assets/Game/Scripts/B\",\"namespace\":\"B\"}]}";

            bool loaded = AIDataGenerationProfileLoader.TryParseDataTableProfiles(json, out _, out var errors);

            Assert.That(loaded, Is.False);
            Assert.That(errors, Has.Some.Contains("Duplicate generation profile source path"));
        }

        [Test]
        public void SyncPipeline_RejectsPathTraversalAndPreservesLogicalFingerprint()
        {
            Assert.That(AIDataSyncPipeline.TryNormalizeRelativePath("Foundation/../Core", out _, out _), Is.False);
            Assert.That(AIDataSyncPipeline.TryNormalizeRelativePath("Foundation\\World", out var normalized, out _), Is.True);
            Assert.That(normalized, Is.EqualTo("Foundation/World"));

            string first = AIDataSyncPipeline.ComputeLogicalFingerprint(new[] { new[] { "alpha", "beta" } });
            string equivalent = AIDataSyncPipeline.ComputeLogicalFingerprint(new[] { new[] { "alpha", "beta" } });
            string changed = AIDataSyncPipeline.ComputeLogicalFingerprint(new[] { new[] { "alpha", "gamma" } });
            Assert.That(equivalent, Is.EqualTo(first));
            Assert.That(changed, Is.Not.EqualTo(first));
        }

        [Test]
        public void SyncPipeline_HardFailsWhenSourceDiffersFromExportBaseline()
        {
            var manifest = new AIDataSyncManifest { sourceFingerprint = "baseline" };
            var report = new AIDataSyncReportItem();

            bool accepted = AIDataSyncPipeline.ValidateBaseline(manifest, "changed", report);

            Assert.That(accepted, Is.False);
            Assert.That(report.errors, Has.Some.Contains("differs from the JSON export baseline"));
        }

        [Test]
        public void DataTableReverseGate_ReportsAndRejectsChangedSourceFingerprint()
        {
            var manifest = new AIDataTableManifest { sourceFingerprint = "exported" };
            var report = new AIDataTableReportItem();

            bool accepted = AIGameDataTableGenerator.ValidateSourceFingerprint(manifest, "changed", report);

            Assert.That(accepted, Is.False);
            Assert.That(report.sourceFingerprint, Is.EqualTo("exported"));
            Assert.That(report.currentFingerprint, Is.EqualTo("changed"));
            Assert.That(report.errors, Has.Some.Contains("differs from the JSON export baseline"));
        }

        [Test]
        public void ConfigAndLanguageAdapters_RejectDuplicateKeysAndPathTraversal()
        {
            const string configJson = "{\"schemaVersion\":1,\"kind\":\"GF_X.Config.AI\",\"relativePath\":\"Foundation/../Core\",\"entries\":[{\"key\":\"DayLength\",\"value\":\"1440000\"}]}";
            const string languageJson = "{\"schemaVersion\":1,\"kind\":\"GF_X.Language.AI\",\"relativePath\":\"Foundation/English\",\"entries\":[{\"key\":\"Start\",\"value\":\"Start\"},{\"key\":\"Start\",\"value\":\"Begin\"}]}";

            Assert.That(AIConfigAdapter.TryParseManifest(configJson, out _, out var configErrors), Is.False);
            Assert.That(configErrors, Has.Some.Contains("relative path is invalid"));
            Assert.That(AILanguageAdapter.TryParseManifest(languageJson, out _, out var languageErrors), Is.False);
            Assert.That(languageErrors, Has.Some.Contains("Duplicate language key"));
        }

        private static class First
        {
            public sealed class DuplicateRow : DataRowBase { public override int Id => 1; }
            public sealed class UniqueRow : DataRowBase { public override int Id => 1; }
        }

        private static class Second
        {
            public sealed class DuplicateRow : DataRowBase { public override int Id => 1; }
        }
    }
}
