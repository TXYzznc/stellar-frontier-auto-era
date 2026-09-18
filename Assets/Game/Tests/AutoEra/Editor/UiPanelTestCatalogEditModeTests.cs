using System.Linq;

using AutoEra.Editor.UiPanelTest;

using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class UiPanelTestCatalogEditModeTests
    {
        private const string SampleUiTable = "#\tUI table\n"
            + "#\tId\t\tSortOrder\tUIPrefab\tPauseCoveredUI\tUIGroupId\tEscapeClose\n"
            + "\t6000\t最小启动菜单\t0\tStartup/MainMenuForm\tfalse\t1\tfalse\n"
            + "\t6001\tART-006 field HUD\t0\tOperations/FieldHudForm\tfalse\t1\tfalse\n"
            + "\t6002\tART-006 base command hub\t100\tOperations/BaseCommandHubForm\ttrue\t1\ttrue\n";

        private const string SampleUiGroups = "#\tUIGroup\n"
            + "#\tID\t\tName\tDepth\n"
            + "\t1\tDefault interface\tDefault\t1\n"
            + "\t2\tDialog interface\tDialog\t200\n"
            + "\t3\tOverlay interface\tOverlay\t500\n";

        [Test]
        public void ParseUiTable_ReadsAllConfiguredRows()
        {
            var rows = UiPanelTestCatalog.ParseUiTable(SampleUiTable);

            Assert.AreEqual(3, rows.Count);
            UiTableRawRow hub = rows.Single(row => row.Id == 6002);
            Assert.AreEqual("ART-006 base command hub", hub.Note);
            Assert.AreEqual(100, hub.SortOrder);
            Assert.AreEqual("Operations/BaseCommandHubForm", hub.UIPrefab);
            Assert.IsTrue(hub.PauseCoveredUI);
            Assert.AreEqual(1, hub.UIGroupId);
            Assert.IsTrue(hub.EscapeClose);
        }

        [Test]
        public void ParseUiTable_IgnoresHeaderCommentsAndShortLines()
        {
            var rows = UiPanelTestCatalog.ParseUiTable("# comment\n\n\tbad\trow\n" + SampleUiTable);

            Assert.AreEqual(3, rows.Count);
        }

        [Test]
        public void ParseUiGroups_ReadsNamesAndDepths()
        {
            var rows = UiPanelTestCatalog.ParseUiGroups(SampleUiGroups);

            Assert.AreEqual(3, rows.Count);
            UiGroupRawRow dialog = rows.Single(row => row.Id == 2);
            Assert.AreEqual("Dialog", dialog.Name);
            Assert.AreEqual(200, dialog.Depth);
        }

        [Test]
        public void ParseUiTable_EmptyContent_ReturnsEmptyList()
        {
            Assert.IsEmpty(UiPanelTestCatalog.ParseUiTable(null));
            Assert.IsEmpty(UiPanelTestCatalog.ParseUiTable(string.Empty));
        }
    }
}
