using System;
using System.IO;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class ResourceExportHistoryStoreEditModeTests
    {
        private string _root;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "AutoEraResourceExportHistoryTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(_root, "Assets", "Export"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, true);
        }

        [Test]
        public void Compare_ReportsAddedModifiedAndDeletedAssets()
        {
            string unchanged = WriteAsset("Assets/Export/Unchanged.asset", "first");
            string changed = WriteAsset("Assets/Export/Changed.asset", "first");
            string deleted = WriteAsset("Assets/Export/Deleted.asset", "first");
            var store = new ResourceExportHistoryStore(_root);
            var baseline = store.SaveSnapshot("baseline", new[] { unchanged, changed, deleted });

            File.WriteAllText(Path.Combine(_root, changed.Replace('/', Path.DirectorySeparatorChar)), "second");
            File.Delete(Path.Combine(_root, deleted.Replace('/', Path.DirectorySeparatorChar)));
            string added = WriteAsset("Assets/Export/Added.asset", "new");

            ResourceExportChangeSet result = store.Compare(baseline, new[] { unchanged, changed, added });

            Assert.That(result.added, Is.EqualTo(new[] { added }));
            Assert.That(result.modified, Is.EqualTo(new[] { changed }));
            Assert.That(result.deleted, Is.EqualTo(new[] { deleted }));
        }

        [Test]
        public void SaveSnapshot_PersistsAndLoadsHistoricalBaselines()
        {
            string asset = WriteAsset("Assets/Export/One.asset", "contents");
            var store = new ResourceExportHistoryStore(_root);
            ResourceExportSnapshot saved = store.SaveSnapshot("delivery", new[] { asset });

            var reloaded = new ResourceExportHistoryStore(_root).LoadSnapshots();

            Assert.That(reloaded, Has.Count.EqualTo(1));
            Assert.That(reloaded[0].id, Is.EqualTo(saved.id));
            Assert.That(reloaded[0].entries[0].assetPath, Is.EqualTo(asset));
        }

        private string WriteAsset(string assetPath, string contents)
        {
            string fullPath = Path.Combine(_root, assetPath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllText(fullPath, contents);
            return assetPath;
        }
    }
}
