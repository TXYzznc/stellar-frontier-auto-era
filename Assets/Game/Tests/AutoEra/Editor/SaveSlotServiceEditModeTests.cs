using System;
using System.IO;
using AutoEra.Save;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class SaveSlotServiceEditModeTests
    {
        private string _root;
        private SaveSlotService _service;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "AutoEraSaveSlotTests", Guid.NewGuid().ToString("N"));
            _service = new SaveSlotService(_root);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }

        [Test]
        public void ThreeSlots_RemainIsolatedFromEachOther()
        {
            Assert.That(_service.Overwrite(0, "slot-0", 100L, "content-0"), Is.True);
            Assert.That(_service.Overwrite(1, "slot-1", 200L, "content-1"), Is.True);
            Assert.That(_service.Overwrite(2, "slot-2", 300L, "content-2"), Is.True);

            var slot0 = _service.Read(0);
            var slot1 = _service.Read(1);
            var slot2 = _service.Read(2);

            Assert.That(slot0.IsSuccess, Is.True);
            Assert.That(slot0.Record.Summary, Is.EqualTo("slot-0"));
            Assert.That(slot0.Record.ContentJson, Is.EqualTo("content-0"));
            Assert.That(slot0.Record.WorldTimeMilliseconds, Is.EqualTo(100L));

            Assert.That(slot1.Record.Summary, Is.EqualTo("slot-1"));
            Assert.That(slot1.Record.ContentJson, Is.EqualTo("content-1"));
            Assert.That(slot2.Record.Summary, Is.EqualTo("slot-2"));
            Assert.That(slot2.Record.ContentJson, Is.EqualTo("content-2"));

            Assert.That(slot0.Record.ContentJson, Is.Not.EqualTo(slot1.Record.ContentJson));
            Assert.That(slot1.Record.ContentJson, Is.Not.EqualTo(slot2.Record.ContentJson));
        }

        [Test]
        public void Create_OnEmptySlot_RoundTripsMetadataAndContent()
        {
            Assert.That(_service.Create(0, "第一天", 123456L, "{\"machineCount\":3}"), Is.True);

            var result = _service.Read(0);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Record.Version, Is.EqualTo(SaveSlotRecord.CurrentVersion));
            Assert.That(result.Record.WorldTimeMilliseconds, Is.EqualTo(123456L));
            Assert.That(result.Record.Summary, Is.EqualTo("第一天"));
            Assert.That(result.Record.ContentJson, Is.EqualTo("{\"machineCount\":3}"));
        }

        [Test]
        public void Create_OnOccupiedSlot_ReturnsFalse()
        {
            Assert.That(_service.Create(1, "first", 1L, "a"), Is.True);
            Assert.That(_service.Create(1, "second", 2L, "b"), Is.False);

            var result = _service.Read(1);
            Assert.That(result.Record.Summary, Is.EqualTo("first"));
            Assert.That(result.Record.ContentJson, Is.EqualTo("a"));
        }

        [Test]
        public void Overwrite_ReplacesPreviousContent()
        {
            Assert.That(_service.Overwrite(0, "before", 10L, "old"), Is.True);
            Assert.That(_service.Overwrite(0, "after", 20L, "new"), Is.True);

            var result = _service.Read(0);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Record.Summary, Is.EqualTo("after"));
            Assert.That(result.Record.WorldTimeMilliseconds, Is.EqualTo(20L));
            Assert.That(result.Record.ContentJson, Is.EqualTo("new"));
        }

        [Test]
        public void Read_OnEmptySlot_ReturnsEmpty()
        {
            Assert.That(_service.Read(0).Status, Is.EqualTo(SaveSlotReadStatus.Empty));
        }

        [Test]
        public void Delete_ThenRead_ReturnsEmpty()
        {
            Assert.That(_service.Overwrite(0, "s", 1L, "c"), Is.True);
            Assert.That(_service.Delete(0), Is.True);
            Assert.That(_service.Read(0).Status, Is.EqualTo(SaveSlotReadStatus.Empty));
            Assert.That(_service.Exists(0), Is.False);
        }

        [Test]
        public void Delete_RemovesEveryBackupAndTemporaryFile()
        {
            Assert.That(_service.Overwrite(0, "v1", 1L, "c1"), Is.True);
            Assert.That(_service.Overwrite(0, "v2", 2L, "c2"), Is.True);
            string main = Path.Combine(_root, "slot_0.json");
            string backup = _service.GetBackupPath(0, 1);
            Assert.That(File.Exists(main), Is.True);
            Assert.That(File.Exists(backup), Is.True);

            Assert.That(_service.Delete(0), Is.True);
            Assert.That(File.Exists(main), Is.False);
            for (int index = 1; index <= SaveSlotService.BackupCount; index++)
            {
                Assert.That(File.Exists(_service.GetBackupPath(0, index)), Is.False,
                    "删掉进度却留下备份，会让「已删除的进度」还能被恢复出来。");
            }
        }

        [Test]
        public void Read_CorruptMainFile_FallsBackToBackupAndSaysSo()
        {
            Assert.That(_service.Overwrite(0, "v1", 1L, "c1"), Is.True);
            Assert.That(_service.Overwrite(0, "v2", 2L, "c2"), Is.True);

            string main = Path.Combine(_root, "slot_0.json");
            File.WriteAllText(main, "{ this is not valid json");

            var result = _service.Read(0);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Record.Summary, Is.EqualTo("v1"));
            Assert.That(result.Record.ContentJson, Is.EqualTo("c1"));
            Assert.That(result.Status, Is.EqualTo(SaveSlotReadStatus.RecoveredFromBackup),
                "回退读备份必须是**独立状态**：报 Success 就等于静默回退。");
            Assert.That(result.IsFromBackup, Is.True);
            Assert.That(result.BackupIndex, Is.EqualTo(1), "读的是最新那一份备份。");
        }

        [Test]
        public void Read_CorruptBothFiles_ReturnsCorrupt()
        {
            Assert.That(_service.Overwrite(0, "v1", 1L, "c1"), Is.True);
            Assert.That(_service.Overwrite(0, "v2", 2L, "c2"), Is.True);

            File.WriteAllText(Path.Combine(_root, "slot_0.json"), "garbage");
            File.WriteAllText(_service.GetBackupPath(0, 1), "garbage");

            Assert.That(_service.Read(0).Status, Is.EqualTo(SaveSlotReadStatus.Corrupt));
        }

        [Test]
        public void Read_NewerVersion_ReturnsNewerVersion()
        {
            string main = Path.Combine(_root, "slot_0.json");
            Directory.CreateDirectory(_root);
            File.WriteAllText(main, "{\"Version\":99,\"WorldTimeMilliseconds\":0,\"Summary\":\"future\",\"ContentJson\":\"x\"}");

            Assert.That(_service.Read(0).Status, Is.EqualTo(SaveSlotReadStatus.NewerVersion));
        }

        [Test]
        public void InvalidSlotIndex_ThrowsArgumentOutOfRange()
        {
            Assert.That(() => _service.Create(-1, "s", 0L, "c"), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => _service.Create(3, "s", 0L, "c"), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => _service.Read(3), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => _service.Delete(-1), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void NullSummaryAndContent_AreNormalizedToEmptyStrings()
        {
            Assert.That(_service.Overwrite(0, null, 5L, null), Is.True);
            var result = _service.Read(0);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Record.Summary, Is.EqualTo(string.Empty));
            Assert.That(result.Record.ContentJson, Is.EqualTo(string.Empty));
        }
    }
}
