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
            GameDataGenerator.SetDataTableCodeGenerationProfiles(null);
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
        public void DataRowFallback_RejectsAmbiguousShortNames()
        {
            Type resolved = DataTableExtension.ResolveDataRowType("DuplicateRow", new[] { typeof(First.DuplicateRow), typeof(Second.DuplicateRow) });
            Assert.That(resolved, Is.Null);
        }

        [Test]
        public void DataRowFallback_AcceptsUniqueNamespacedRow()
        {
            Type resolved = DataTableExtension.ResolveDataRowType("UniqueRow", new[] { typeof(First.UniqueRow) });
            Assert.That(resolved, Is.EqualTo(typeof(First.UniqueRow)));
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
