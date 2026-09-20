using AutoEra.UI;
using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 选择类界面的结果载体。<see cref="AutoEraUiSelectionRequest"/> 是「界面把结果交回调用方」
    /// 的通道，所以它的三种结局必须互斥且可区分——调用方要靠它判断「玩家选了东西」「玩家取消了」
    /// 还是「界面被外部关掉了」。
    /// </summary>
    public sealed class AutoEraUiSelectionRequestEditModeTests
    {
        [Test]
        public void FreshRequest_IsPendingAndHasNoSelection()
        {
            var request = new AutoEraUiSelectionRequest();
            Assert.That(request.IsPending, Is.True);
            Assert.That(request.Confirmed, Is.False);
            Assert.That(request.Cancelled, Is.False);
            Assert.That(request.SelectedId.IsValid, Is.False);
        }

        [Test]
        public void Confirm_WithValidId_RecordsSelection()
        {
            var request = new AutoEraUiSelectionRequest();
            var id = new PersistentId(42);

            request.Confirm(id);

            Assert.That(request.Confirmed, Is.True);
            Assert.That(request.Cancelled, Is.False);
            Assert.That(request.IsPending, Is.False);
            Assert.That(request.SelectedId, Is.EqualTo(id));
        }

        [Test]
        public void Confirm_WithInvalidId_DoesNotCountAsConfirmation()
        {
            var request = new AutoEraUiSelectionRequest();

            request.Confirm(PersistentId.Invalid);

            Assert.That(request.Confirmed, Is.False,
                "无效 Id 不得被当成「选中了」——调用方会拿着它去找一个不存在的对象。");
            Assert.That(request.SelectedId.IsValid, Is.False);
        }

        [Test]
        public void Cancel_ClearsAnyPreviousSelection()
        {
            var request = new AutoEraUiSelectionRequest();
            request.Confirm(new PersistentId(7));

            request.Cancel();

            Assert.That(request.Cancelled, Is.True);
            Assert.That(request.Confirmed, Is.False);
            Assert.That(request.SelectedId.IsValid, Is.False, "取消之后不能再留下选中结果。");
        }

        [Test]
        public void Accepts_RespectsCandidateFilter()
        {
            var onlySites = new AutoEraUiSelectionRequest(includeMachines: false, includeSites: true);
            var onlyMachines = new AutoEraUiSelectionRequest(includeMachines: true, includeSites: false);

            var machine = new UiRegionObjectRow(PersistentId.Invalid, "机器", "机器", "运行", false, isMachine: true);
            var site = new UiRegionObjectRow(PersistentId.Invalid, "温室", "建筑", "待机", false, isMachine: false);

            Assert.That(onlySites.Accepts(machine), Is.False);
            Assert.That(onlySites.Accepts(site), Is.True);
            Assert.That(onlyMachines.Accepts(machine), Is.True);
            Assert.That(onlyMachines.Accepts(site), Is.False);
        }
    }
}
