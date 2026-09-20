using System;
using System.IO;
using AutoEra.Application;
using AutoEra.Save;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Time;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 存档域读取模型的数据流：空态、占用、选中详情、损坏恢复标记、删除后刷新与退订。
    ///
    /// 测试用临时目录注入 <see cref="SaveSlotService"/>，因此不碰玩家的真实存档。
    /// 与机器域一样，测试走正式入口 <see cref="AutoEraUiSession"/>，顺带覆盖「会话 → 域模型」这条链。
    /// </summary>
    public sealed class SaveSlotReadModelEditModeTests
    {
        private string _root;
        private AutoEraApplicationContext _context;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "AutoEraSaveSlotTests", Guid.NewGuid().ToString("N"));
            _context = new AutoEraApplicationContext(
                new SystemUtcTimeProvider(),
                new AutoEraWorldSessionFactory(),
                null,
                new SaveSlotService(_root));
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
            _context = null;

            try
            {
                if (Directory.Exists(_root))
                {
                    Directory.Delete(_root, true);
                }
            }
            catch (IOException)
            {
                // 临时目录清不掉不影响结论，交给系统回收。
            }
        }

        private ISaveSlotReadModel Model() => SaveSlotReadModels.Create(AutoEraUiSession.ForApplication(_context));

        [Test]
        public void MissingSession_ReportsUnavailableWithReason()
        {
            using (ISaveSlotReadModel model = SaveSlotReadModels.Create(null))
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                Assert.That(model.Snapshot.UnavailableReason, Is.Not.Null.And.Not.Empty,
                    "不可用态必须给出可展示的原因。");
                Assert.That(model.Snapshot.Count, Is.Zero, "不可用时不得伪造槽位。");
            }
        }

        [Test]
        public void EmptyService_ReportsEmptyStateWithAllSlots()
        {
            using (ISaveSlotReadModel model = Model())
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Empty),
                    "三个槽位都空时是 Empty，不是 Unavailable——空是正常状态。");
                Assert.That(model.Snapshot.Count, Is.EqualTo(SaveSlotService.SlotCount));
                Assert.That(model.Snapshot.HasRecoverableSlot, Is.False);
                foreach (UiSaveSlotRow row in model.Snapshot.Slots)
                {
                    Assert.That(row.Occupied, Is.False);
                    Assert.That(row.Summary, Is.Not.Null.And.Not.Empty, "空槽也要有可展示的说明。");
                }
            }
        }

        [Test]
        public void OccupiedSlot_ExposesSummaryAndWorldTime()
        {
            _context.SaveSlots.Create(1, "第一次远征", 3600_000L, "{}");

            using (ISaveSlotReadModel model = Model())
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Ready));
                UiSaveSlotRow row = model.Snapshot.Slots[1];
                Assert.That(row.Occupied, Is.True);
                Assert.That(row.Summary, Is.EqualTo("第一次远征"));
                Assert.That(row.WorldTime, Is.EqualTo(AutoEraUiFormat.WorldTime(3600_000L)));
            }
        }

        [Test]
        public void SelectingSlot_PublishesDetailAndMetadata()
        {
            _context.SaveSlots.Create(2, "第三次远征", 7200_000L, "{}");

            using (ISaveSlotReadModel model = Model())
            {
                SaveSlotDomainSection? notified = null;
                model.Changed += section => notified = section;

                Assert.That(model.Select(2), Is.True);
                Assert.That(model.SelectedIndex, Is.EqualTo(2));
                Assert.That(notified, Is.EqualTo(SaveSlotDomainSection.Detail),
                    "选中只影响详情区，不应触发整列表刷新。");
                Assert.That(model.Snapshot.HasSelection, Is.True);
                Assert.That(model.Snapshot.Metadata, Is.Not.Empty);
                Assert.That(model.Snapshot.Health, Is.Not.Empty);
                Assert.That(Contains(model.Snapshot.Metadata, "第三次远征"), Is.True,
                    "详情必须能读到该槽位的摘要。");

                model.ClearSelection();
                Assert.That(model.Snapshot.HasSelection, Is.False);
            }
        }

        [Test]
        public void CorruptSlot_IsMarkedAsRecoverable()
        {
            _context.SaveSlots.Create(0, "会被弄坏", 1000L, "{}");
            File.WriteAllText(Path.Combine(_root, "slot_0.json"), "{ 这不是合法的 JSON");
            File.Delete(Path.Combine(_root, "slot_0.json.bak"));

            using (ISaveSlotReadModel model = Model())
            {
                UiSaveSlotRow row = model.Snapshot.Slots[0];
                Assert.That(row.Occupied, Is.True, "有文件就算占用，不能当成空槽。");
                Assert.That(row.NeedsRecovery, Is.True);
                Assert.That(model.Snapshot.HasRecoverableSlot, Is.True,
                    "界面据此提示走恢复流程。");

                Assert.That(model.Select(0), Is.True);
                Assert.That(Contains(model.Snapshot.Health, "损坏"), Is.True,
                    "健康栏必须说明真实文件状态。");
            }
        }

        [Test]
        public void DeleteThenRefresh_ReturnsSlotToEmpty()
        {
            _context.SaveSlots.Create(0, "待删除", 1000L, "{}");

            using (ISaveSlotReadModel model = Model())
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Ready));

                _context.SaveSlots.Delete(0);
                model.Refresh();

                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Empty));
                Assert.That(model.Snapshot.Slots[0].Occupied, Is.False);
            }
        }

        [Test]
        public void Dispose_StopsNotifying()
        {
            ISaveSlotReadModel model = Model();
            int notifications = 0;
            model.Changed += _ => notifications++;
            model.Select(0);
            int before = notifications;

            model.Dispose();
            model.Refresh();
            model.Select(1);

            Assert.That(notifications, Is.EqualTo(before), "释放后不得再通知（界面可能已经销毁）。");
        }

        [Test]
        public void RestoreFromBackup_RecoversCorruptMainFile()
        {
            // 第一次写建主文件，第二次覆盖会留下第一版的备份。
            _context.SaveSlots.Create(0, "第一版", 1000L, "{}");
            _context.SaveSlots.Overwrite(0, "第二版", 2000L, "{}");
            Assert.That(_context.SaveSlots.HasBackup(0), Is.True, "覆盖写必须留下备份。");

            File.WriteAllText(Path.Combine(_root, "slot_0.json"), "{ 坏了");
            SaveSlotReadResult damaged = _context.SaveSlots.Read(0);
            Assert.That(damaged.IsSuccess, Is.True, "主文件坏掉时应回退读到备份。");
            Assert.That(damaged.Record.Summary, Is.EqualTo("第一版"));

            Assert.That(_context.SaveSlots.RestoreFromBackup(0), Is.True);
            SaveSlotReadResult recovered = _context.SaveSlots.Read(0);
            Assert.That(recovered.IsSuccess, Is.True);
            Assert.That(recovered.Record.Summary, Is.EqualTo("第一版"),
                "恢复后主文件应当就是备份的内容，否则玩家重启游戏又会看到损坏。");
        }

        [Test]
        public void RestoreFromBackup_WithoutBackup_ReportsFalseAndKeepsSave()
        {
            _context.SaveSlots.Create(0, "孤本", 1000L, "{}");
            Assert.That(_context.SaveSlots.HasBackup(0), Is.False);
            Assert.That(_context.SaveSlots.RestoreFromBackup(0), Is.False, "没有备份时不得伪造恢复成功。");
            Assert.That(_context.SaveSlots.Read(0).Record.Summary, Is.EqualTo("孤本"), "失败不得破坏原存档。");
        }

        private static bool Contains(System.Collections.Generic.IReadOnlyList<UiDetailField> fields, string fragment)
        {
            for (int i = 0; i < fields.Count; i++)
            {
                if ((fields[i].Label != null && fields[i].Label.Contains(fragment))
                    || (fields[i].Value != null && fields[i].Value.Contains(fragment)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
