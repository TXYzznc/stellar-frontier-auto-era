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

        [Test]
        public void ConfigAndLanguageAdapters_BuildExistingExcelLayoutsFromValidJson()
        {
            const string configJson = "{\"schemaVersion\":1,\"kind\":\"GF_X.Config.AI\",\"relativePath\":\"Foundation/WorldSettings\",\"entries\":[{\"key\":\"DayLength\",\"comment\":\"world milliseconds\",\"value\":\"1440000\"}]}";
            const string languageJson = "{\"schemaVersion\":1,\"kind\":\"GF_X.Language.AI\",\"relativePath\":\"Foundation/English\",\"entries\":[{\"key\":\"Start\",\"value\":\"Start\"}]}";

            Assert.That(AIConfigAdapter.TryBuildExcelRows(configJson, out _, out var configRows, out var configErrors), Is.True, string.Join(" | ", configErrors));
            Assert.That(configRows[1], Is.EqualTo(new[] { "#", "Key", "备注", "Value" }));
            Assert.That(configRows[2], Is.EqualTo(new[] { string.Empty, "DayLength", "world milliseconds", "1440000" }));

            Assert.That(AILanguageAdapter.TryBuildExcelRows(languageJson, out _, out var languageRows, out var languageErrors), Is.True, string.Join(" | ", languageErrors));
            Assert.That(languageRows, Has.Count.EqualTo(1));
            Assert.That(languageRows[0], Is.EqualTo(new[] { string.Empty, "Start", "Start" }));
        }

        [Test]
        public void LanguageAdapter_ReadsExistingOfficialWorkbookWithoutWritingIt()
        {
            string projectRoot = System.IO.Directory.GetParent(UnityEngine.Application.dataPath).FullName;
            string excelFile = System.IO.Path.Combine(projectRoot, "GameData", "Languages", "English.xlsx");

            bool loaded = AILanguageAdapter.TryCreateManifestFromExcel(excelFile, out var manifest, out var errors);

            Assert.That(loaded, Is.True, string.Join(" | ", errors));
            Assert.That(manifest.relativePath, Is.EqualTo("English"));
            Assert.That(manifest.sourceFingerprint, Is.Not.Empty);
            Assert.That(manifest.entries.Exists(entry => entry.key == "Framework.Ready" && entry.value == "Ready"), Is.True);
        }

        [Test]
        public void SyncPipeline_ResolvesJsonOutputsOnlyInsideMatchingAiDataRoots()
        {
            Assert.That(AIDataSyncPipeline.TryResolveAIJsonPath(AIDataKind.Config, "Foundation/WorldSettings", out var configPath, out _), Is.True);
            Assert.That(configPath.Replace('\\', '/'), Does.Contain("GameData/AIData/Configs/Foundation/WorldSettings.json"));
            Assert.That(AIDataSyncPipeline.TryResolveAIJsonPath(AIDataKind.Language, "../escape", out _, out _), Is.False);
        }

        [Test]
        public void ConfigAndLanguageReverse_RejectMissingJsonBeforeAnyWorkbookOperation()
        {
            string missingJson = System.IO.Path.Combine(System.IO.Directory.GetParent(UnityEngine.Application.dataPath).FullName, "Temp", "missing-ai-data.json");

            Assert.That(AIConfigAdapter.TryReverseJsonToExcel(missingJson, out var configErrors), Is.False);
            Assert.That(configErrors, Has.Some.Contains("Config JSON does not exist"));
            Assert.That(AILanguageAdapter.TryReverseJsonToExcel(missingJson, out var languageErrors), Is.False);
            Assert.That(languageErrors, Has.Some.Contains("Language JSON does not exist"));
        }

        [Test]
        public void SyncPipeline_RollsBackEarlierReplacementWhenLaterReplacementFails()
        {
            string root = System.IO.Path.Combine(System.IO.Directory.GetParent(UnityEngine.Application.dataPath).FullName, "Temp", "AutoEraSyncPipelineTests", Guid.NewGuid().ToString("N"));
            string source = System.IO.Path.Combine(root, "source.txt");
            string destination = System.IO.Path.Combine(root, "destination.txt");
            System.IO.Directory.CreateDirectory(root);
            System.IO.File.WriteAllText(source, "new");
            System.IO.File.WriteAllText(destination, "old");

            try
            {
                var report = new AIDataSyncReportItem();
                bool replaced = AIDataSyncPipeline.ReplaceFilesTransactionally(new List<AIDataFileReplacement>
                {
                    new AIDataFileReplacement { sourceFile = source, destinationFile = destination },
                    new AIDataFileReplacement { sourceFile = System.IO.Path.Combine(root, "missing.txt"), destinationFile = System.IO.Path.Combine(root, "later.txt") },
                }, report);

                Assert.That(replaced, Is.False);
                Assert.That(report.rollbackSucceeded, Is.True);
                Assert.That(System.IO.File.ReadAllText(destination), Is.EqualTo("old"));
            }
            finally
            {
                if (System.IO.Directory.Exists(root))
                {
                    System.IO.Directory.Delete(root, true);
                }
            }
        }

        [Test]
        public void SyncPipeline_RejectsChangedBaselineWithoutOverwritingExternalEdit()
        {
            string root = System.IO.Path.Combine(UnityEngine.Application.temporaryCachePath, Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(root);
            try
            {
                string source = System.IO.Path.Combine(root, "source.txt");
                string target = System.IO.Path.Combine(root, "target.txt");
                System.IO.File.WriteAllText(source, "generated");
                System.IO.File.WriteAllText(target, "original");
                string baseline = AIDataSyncPipeline.ComputeFileFingerprint(target);
                System.IO.File.WriteAllText(target, "external edit");
                var report = new AIDataSyncReportItem();
                Assert.That(AIDataSyncPipeline.ReplaceFilesTransactionally(new[] {
                    new AIDataFileReplacement { sourceFile = source, destinationFile = target, expectedDestinationFingerprint = baseline }
                }, report), Is.False);
                Assert.That(System.IO.File.ReadAllText(target), Is.EqualTo("external edit"));
            }
            finally { System.IO.Directory.Delete(root, true); }
        }

        [Test]
        public void SyncPipeline_LockedLaterTargetRestoresEarlierWriteAndRemovesNewOutput()
        {
            string root = System.IO.Path.Combine(UnityEngine.Application.temporaryCachePath, Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(root);
            try
            {
                string source = System.IO.Path.Combine(root, "source.txt");
                string existing = System.IO.Path.Combine(root, "existing.txt");
                string created = System.IO.Path.Combine(root, "created.txt");
                string locked = System.IO.Path.Combine(root, "locked.txt");
                System.IO.File.WriteAllText(source, "generated");
                System.IO.File.WriteAllText(existing, "original");
                System.IO.File.WriteAllText(locked, "other owner");
                using (var owner = new System.IO.FileStream(locked, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None))
                {
                    var report = new AIDataSyncReportItem();
                    Assert.That(AIDataSyncPipeline.ReplaceFilesTransactionally(new[] {
                        new AIDataFileReplacement { sourceFile = source, destinationFile = existing },
                        new AIDataFileReplacement { sourceFile = source, destinationFile = created },
                        new AIDataFileReplacement { sourceFile = source, destinationFile = locked }
                    }, report), Is.False);
                    Assert.That(report.rollbackSucceeded, Is.True);
                }
                Assert.That(System.IO.File.ReadAllText(existing), Is.EqualTo("original"));
                Assert.That(System.IO.File.Exists(created), Is.False);
                Assert.That(System.IO.File.ReadAllText(locked), Is.EqualTo("other owner"));
            }
            finally { System.IO.Directory.Delete(root, true); }
        }

        [Test]
        public void StagedCodeGeneration_PreservesLogicalNameAndProfileOutsideAssetRoot()
        {
            string root = System.IO.Path.Combine(UnityEngine.Application.temporaryCachePath, Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(root);
            try
            {
                string input = System.IO.Path.Combine(ConstEditor.DataTablePath, "Core/UITable.txt");
                string copy = System.IO.Path.Combine(root, "arbitrary.txt");
                System.IO.File.Copy(input, copy);
                string output = System.IO.Path.Combine(root, "row.cs");
                var processor = DataTableGenerator.CreateDataTableProcessor(copy);
                Assert.That(DataTableGenerator.GenerateCodeFile(processor, "Foundation/StartupMessages", output), Is.True);
                string code = System.IO.File.ReadAllText(output);
                Assert.That(code, Does.Contain("namespace AutoEra.DataTable"));
                Assert.That(code, Does.Contain("class StartupMessages"));
                Assert.That(code, Does.Not.Contain("class arbitrary"));
                Assert.That(DataTableGenerator.GenerateCodeFile(processor, "Core/UITable", output), Is.True);
                Assert.That(System.IO.File.ReadAllText(output), Does.Contain("class UITable"));
                Assert.That(System.IO.File.ReadAllText(output), Does.Not.Contain("namespace AutoEra.DataTable"));
            }
            finally { System.IO.Directory.Delete(root, true); }
        }

        [Test]
        public void SyncPipeline_RejectsChangedReadDependencyBeforeAnyOutput()
        {
            string root = System.IO.Path.Combine(UnityEngine.Application.temporaryCachePath, Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(root);
            try
            {
                string source = System.IO.Path.Combine(root, "source");
                string dependency = System.IO.Path.Combine(root, "sibling");
                string output = System.IO.Path.Combine(root, "output");
                System.IO.File.WriteAllText(source, "generated");
                System.IO.File.WriteAllText(dependency, "old");
                var dependencies = new Dictionary<string,string> { [dependency] = AIDataSyncPipeline.ComputeFileFingerprint(dependency) };
                System.IO.File.WriteAllText(dependency, "new external version");
                Assert.That(AIDataSyncPipeline.ReplaceFilesTransactionally(new[] {
                    new AIDataFileReplacement {sourceFile=source,destinationFile=output}
                }, new AIDataSyncReportItem(), dependencies), Is.False);
                Assert.That(System.IO.File.Exists(output), Is.False);
            }
            finally { System.IO.Directory.Delete(root, true); }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ConfigAndLanguage_RepeatReverseProducesReadableOutputsAndStableFingerprint(bool language)
        {
            string id = "B10Probe" + Guid.NewGuid().ToString("N");
            string relative = "Foundation/" + id;
            string json = System.IO.Path.Combine(UnityEngine.Application.temporaryCachePath, id + ".json");
            var kind = language ? AIDataKind.Language : AIDataKind.Config;
            Assert.That(AIDataSyncPipeline.TryResolveGameDataPath(kind, relative, ".xlsx", out string excel, out _), Is.True);
            string output = GameDataGenerator.GetGameDataExcelOutputFile(language ? GameDataType.Language : GameDataType.Config, excel);
            try
            {
                object manifest = language ? (object)new AILanguageManifest {
                    relativePath=relative, entries=new List<AILanguageEntry> {new AILanguageEntry {key=id,value="Probe text"}}
                } : new AIConfigManifest {
                    relativePath=relative, entries=new List<AIConfigEntry> {new AIConfigEntry {key=id,value="42",comment=""}}
                };
                System.IO.File.WriteAllText(json, UnityEngine.JsonUtility.ToJson(manifest));
                for (int round=0; round<2; round++)
                {
                    List<string> errors;
                    bool result = language ? AILanguageAdapter.TryReverseJsonToExcel(json,out errors) : AIConfigAdapter.TryReverseJsonToExcel(json,out errors);
                    Assert.That(result, Is.True, string.Join(";",errors));
                    Assert.That(System.IO.File.Exists(excel), Is.True);
                    Assert.That(System.IO.File.ReadAllText(output), Does.Contain(id));
                    string expected = UnityEngine.JsonUtility.FromJson<AIConfigManifest>(System.IO.File.ReadAllText(json)).sourceFingerprint;
                    if (language)
                    {
                        Assert.That(AILanguageAdapter.TryCreateManifestFromExcel(excel,out var read,out errors), Is.True);
                        Assert.That(read.sourceFingerprint, Is.EqualTo(expected));
                    }
                    else
                    {
                        Assert.That(AIConfigAdapter.TryCreateManifestFromExcel(excel,out var read,out errors), Is.True);
                        Assert.That(read.sourceFingerprint, Is.EqualTo(expected));
                    }
                }
            }
            finally
            {
                System.IO.File.Delete(json);
                System.IO.File.Delete(excel);
                UnityEditor.AssetDatabase.Refresh();
                UnityEditor.AssetDatabase.DeleteAsset(output.Replace('\\','/'));
                UnityEditor.AssetDatabase.DeleteAsset(System.IO.Path.ChangeExtension(output,".bytes").Replace('\\','/'));
            }
        }

        [Test]
        public void CoreSpecialScripts_StagedGenerationMatchesExistingOutput()
        {
            string root = System.IO.Path.Combine(UnityEngine.Application.temporaryCachePath, Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(root);
            try
            {
                string ui = System.IO.Path.Combine(root,"UIViews.cs");
                string groups = System.IO.Path.Combine(root,"groups.cs");
                GameDataGenerator.GenerateUIFormNamesScript(ConstEditor.UITableExcelFullPath,ui);
                GameDataGenerator.GenerateGroupEnumScript(ConstEditor.DataTableExcelPath,groups);
                Assert.That(System.IO.File.ReadAllText(ui),Is.EqualTo(System.IO.File.ReadAllText(ConstEditor.UIViewScriptFile)));
                // Existing framework defaults have a hand-maintained documentation header.
                // Compare every enum/member in order, without treating comments as runtime data.
                var pattern = new System.Text.RegularExpressions.Regex(@"public\s+enum\s+\w+\s*\{[^}]*\}");
                var expected = pattern.Matches(System.IO.File.ReadAllText(ConstEditor.ConstGroupScriptFileFullName));
                var actual = pattern.Matches(System.IO.File.ReadAllText(groups));
                Assert.That(actual.Count, Is.EqualTo(expected.Count));
                for (int i=0;i<expected.Count;i++)
                    Assert.That(System.Text.RegularExpressions.Regex.Replace(actual[i].Value,@"\s+", ""),
                        Is.EqualTo(System.Text.RegularExpressions.Regex.Replace(expected[i].Value,@"\s+", "")));
            }
            finally { System.IO.Directory.Delete(root,true); }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SyncPipeline_FailedCurrentWriteRestoresOrRetainsRecoveryBackup(bool failRestore)
        {
            string root = System.IO.Path.Combine(UnityEngine.Application.temporaryCachePath, Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(root);
            string recovery = null;
            try
            {
                string source = System.IO.Path.Combine(root,"source");
                string target = System.IO.Path.Combine(root,"target");
                System.IO.File.WriteAllText(source,"new data");
                System.IO.File.WriteAllText(target,"recover me");
                var report = new AIDataSyncReportItem();
                bool result = AIDataSyncPipeline.ReplaceFilesTransactionally(new[] {
                    new AIDataFileReplacement {sourceFile=source,destinationFile=target}
                }, report, null, (path, restoring) => {
                    if (!restoring || failRestore) throw new System.IO.IOException("Controlled write failure");
                });
                Assert.That(result,Is.False);
                Assert.That(report.rollbackSucceeded,Is.EqualTo(!failRestore));
                if (failRestore)
                {
                    string prefix = "Recovery backups retained at: ";
                    string message = report.errors.Find(e => e.StartsWith(prefix,StringComparison.Ordinal));
                    Assert.That(message,Is.Not.Null);
                    recovery = message.Substring(prefix.Length);
                    Assert.That(System.IO.File.ReadAllText(System.IO.Path.Combine(recovery,"0000.bak")),Is.EqualTo("recover me"));
                }
                else Assert.That(System.IO.File.ReadAllText(target),Is.EqualTo("recover me"));
            }
            finally
            {
                System.IO.Directory.Delete(root,true);
                if (recovery != null)
                {
                    string expectedRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Temp/AIDataSyncBackups")) + System.IO.Path.DirectorySeparatorChar;
                    Assert.That(System.IO.Path.GetFullPath(recovery).StartsWith(expectedRoot,StringComparison.OrdinalIgnoreCase),Is.True);
                    System.IO.Directory.Delete(recovery,true);
                }
            }
        }

        [Test]
        public void DataTable_RepeatReverseAndImportPreserveFingerprintWithoutTouchingCore()
        {
            string id = "B10Probe" + Guid.NewGuid().ToString("N");
            string relative = "Foundation/" + id;
            string json = System.IO.Path.Combine(UnityEngine.Application.temporaryCachePath,id+".json");
            string baselinePath = System.IO.Path.Combine(ConstEditor.AIDataTablePath,"Core/UITable.json");
            string coreBaseline = AIDataSyncPipeline.ComputeFileFingerprint(baselinePath);
            string excel = GameDataGenerator.GameDataExcelRelative2FullPath(GameDataType.DataTable,relative);
            string output = GameDataGenerator.GetGameDataExcelOutputFile(GameDataType.DataTable,excel);
            try
            {
                string content = System.IO.File.ReadAllText(baselinePath).Replace("\"relativePath\": \"Core/UITable\"", "\"relativePath\": \""+relative+"\"");
                Assert.That(content, Does.Contain("\"relativePath\": \""+relative+"\""));
                System.IO.File.WriteAllText(json,content);
                for (int i=0;i<2;i++)
                {
                    var report = AIGameDataTableGenerator.ImportDataTablesFromAIJson(new[] {json},true,true);
                    Assert.That(report.failureCount,Is.Zero);
                    Assert.That(report.successCount,Is.EqualTo(1));
                    Assert.That(System.IO.File.Exists(output),Is.True);
                }
                Assert.That(AIDataSyncPipeline.ComputeFileFingerprint(baselinePath),Is.EqualTo(coreBaseline));
            }
            finally
            {
                System.IO.File.Delete(json);
                System.IO.File.Delete(excel);
                UnityEditor.AssetDatabase.Refresh();
                UnityEditor.AssetDatabase.DeleteAsset(output.Replace('\\','/'));
                UnityEditor.AssetDatabase.DeleteAsset(System.IO.Path.ChangeExtension(output,".bytes").Replace('\\','/'));
            }
        }

        [Test]
        public void Watcher_SuppressesOnlyTheCommittedContentVersion()
        {
            string path = System.IO.Path.Combine(UnityEngine.Application.temporaryCachePath,Guid.NewGuid().ToString("N")+".xlsx");
            Assert.That(DataTableUpdater.IsCommittedVersion(path,"first"),Is.False);
            DataTableUpdater.RecordCommittedVersion(path,"first");
            Assert.That(DataTableUpdater.IsCommittedVersion(path,"first"),Is.True);
            Assert.That(DataTableUpdater.IsCommittedVersion(path,"user edit"),Is.False);
            Assert.That(DataTableUpdater.IsCommittedVersion(path+".other","first"),Is.False);
        }

        [Test]
        public void EmptyExplicitRefresh_DoesNotRegenerateAnyCoreCode()
        {
            string path = DataTableGenerator.GetCodeOutputFile("Core/UITable");
            string fingerprint = AIDataSyncPipeline.ComputeFileFingerprint(path);
            var timestamp = System.IO.File.GetLastWriteTimeUtc(path);
            GameDataGenerator.RefreshAllDataTable(Array.Empty<string>());
            Assert.That(AIDataSyncPipeline.ComputeFileFingerprint(path),Is.EqualTo(fingerprint));
            Assert.That(System.IO.File.GetLastWriteTimeUtc(path),Is.EqualTo(timestamp));
        }

        [Test]
        public void DataTable_InvalidIdAndSchema_DoNotModifyFormalStartupOutputs()
        {
            string source="GameData/AIData/DataTables/Foundation/StartupMessages.json";
            string temporary=System.IO.Path.Combine(UnityEngine.Application.temporaryCachePath,Guid.NewGuid().ToString("N")+".json");
            string output=System.IO.Path.Combine(ConstEditor.DataTablePath,"Foundation/StartupMessages.txt");
            string baseline=AIDataSyncPipeline.ComputeFileFingerprint(output);
            try
            {
                string valid=System.IO.File.ReadAllText(source);
                foreach(string invalid in new[] {
                    valid.Replace("\"Id\": \"2\"","\"Id\": \"01\""),
                    valid.Replace("\"Id\": \"2\"","\"Id\": \"invalid\""),
                    valid.Replace("\"schemaVersion\": 1","\"schemaVersion\": 99")})
                {
                    Assert.That(invalid,Is.Not.EqualTo(valid));
                    System.IO.File.WriteAllText(temporary,invalid);
                    var report=AIGameDataTableGenerator.ImportDataTablesFromAIJson(new[]{temporary},true,true);
                    Assert.That(report.failureCount,Is.EqualTo(1));
                    Assert.That(AIDataSyncPipeline.ComputeFileFingerprint(output),Is.EqualTo(baseline));
                }
            }
            finally {System.IO.File.Delete(temporary);}
        }

        [Test]
        public void DataTable_TextModeRejectsInvalidTypedCellBeforeFormalWrites()
        {
            string source = "GameData/AIData/DataTables/Foundation/StartupMessages.json";
            string temporary = System.IO.Path.Combine(UnityEngine.Application.temporaryCachePath, Guid.NewGuid().ToString("N") + ".json");
            string output = System.IO.Path.Combine(ConstEditor.DataTablePath, "Foundation/StartupMessages.txt");
            string baseline = AIDataSyncPipeline.ComputeFileFingerprint(output);
            try
            {
                string valid = System.IO.File.ReadAllText(source);
                string invalid = valid.Replace("\"type\": \"string\"", "\"type\": \"int\"");
                Assert.That(invalid, Is.Not.EqualTo(valid));
                System.IO.File.WriteAllText(temporary, invalid);
                UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Error,
                    new System.Text.RegularExpressions.Regex("Parse raw value failure\\..*Name='MessageKey'.*Type='int'.*RawValue='AutoEra.Startup.MenuReady'"));
                UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Error,
                    new System.Text.RegularExpressions.Regex("Parse data table .* failure, exception is"));
                var report = AIGameDataTableGenerator.ImportDataTablesFromAIJson(new[] { temporary }, true, true);
                Assert.That(report.failureCount, Is.EqualTo(1));
                Assert.That(AIDataSyncPipeline.ComputeFileFingerprint(output), Is.EqualTo(baseline));
            }
            finally { System.IO.File.Delete(temporary); }
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
