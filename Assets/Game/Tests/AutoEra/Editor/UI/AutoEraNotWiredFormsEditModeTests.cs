using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 「领域尚未接入」的界面必须把自己讲清楚，而不是留一个空壳。
    ///
    /// 这是对**生成器行为**的回归围栏：那十五个界面没有对应的读模型可查（领域服务在生产里
    /// 根本没有创建者），所以它们唯一诚实的信息就是「本域未接入」。生成器会按契约里的页面字段
    /// 自动注入空态渲染与常量；一旦有人移除了注入、或新增了界面却忘了登记，这里会失败。
    ///
    /// 名单与 `tools/ui_contract_to_form_script.py` 的 `NOT_WIRED` 集合一一对应。
    /// 领域真正接入后，应把该 Form 从两边一起移走。
    /// </summary>
    public sealed class AutoEraNotWiredFormsEditModeTests
    {
        private const string UiDirectory = "Assets/Game/Scripts/AutoEra/UI/";

        private static readonly string[] NotWiredForms =
        {
            "ComponentLibraryForm",
            "ComponentPickerForm",
            "UpgradeForm",
            "WarehouseForm",
            "ShopForm",
            "BuildCatalogForm",
            "WorkshopForm",
            "QuestForm",
            "AlertForm",
            "SettingsForm",
            "CropKnowledgeForm",
            "TutorialForm",
            "FeatureHelpForm",
            "HelpForm",
            "RuleHelpForm",
        };

        private static IEnumerable<TestCaseData> NotWiredCases()
        {
            foreach (string form in NotWiredForms)
            {
                yield return new TestCaseData(form).SetName($"NotWired_{form}");
            }
        }

        [TestCaseSource(nameof(NotWiredCases))]
        public void NotWiredForm_ExplainsItselfAndDisablesItsActions(string form)
        {
            string path = UiDirectory + form + ".cs";
            Assert.That(File.Exists(path), Is.True, $"{form}.cs 不存在——生成器没有产出该界面？");

            string text = File.ReadAllText(path);

            Assert.That(text, Does.Contain("NotWiredReason"),
                $"{form} 必须带一条可展示的未接入原因。");
            Assert.That(text, Does.Contain("IsDomainWired => false"),
                $"{form} 必须声明自己尚未接入，供调用方与测试判断。");
            Assert.That(text, Does.Contain("ShowPageUnavailable"),
                $"{form} 必须把原因写到页面的状态组与说明文本上，而不是留空壳。");
            Assert.That(text, Does.Contain("DisableDomainActions"),
                $"{form} 必须禁用本域业务动作——不可行的操作不该看起来可点。");
        }

        [Test]
        public void WiredForms_AreNotInjectedWithNotWiredScaffolding()
        {
            // 已接入的界面绝不能带上「未接入」的脚手架：那会让它们明明有数据却宣称没有。
            foreach (string form in new[] { "MainMenuForm", "BaseCommandHubForm", "FieldHudForm", "RecordReaderForm", "MachineLibraryForm" })
            {
                string path = UiDirectory + form + ".cs";
                Assert.That(File.Exists(path), Is.True);
                Assert.That(File.ReadAllText(path), Does.Not.Contain("NotWiredReason"),
                    $"{form} 已接入领域，不该被注入未接入脚手架。");
            }
        }
    }
}
