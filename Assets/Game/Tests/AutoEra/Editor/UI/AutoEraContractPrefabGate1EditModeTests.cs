using AutoEra.Editor.UiProto;
using NUnit.Framework;
using System.Linq;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 门 1（原型验收）的 EditMode 入口。
    ///
    /// 判定逻辑在 Editor 程序集的 <see cref="AutoEraContractGate1Checker"/> 里 ——
    /// 契约要按 JSON 解析，而 Newtonsoft 只对该程序集可见。这里只把它的
    /// 问题清单变成一次断言，因此新增页面无需改动本文件。
    ///
    /// L4（编译与运行）由普通编译与 PlayMode 冒烟承担，见 GF-UI-Standards/07。
    /// </summary>
    public sealed class AutoEraContractPrefabGate1EditModeTests
    {
        [Test]
        public void EveryContractPrefab_MeetsGate1()
        {
            var problems = AutoEraContractGate1Checker.CheckAll();

            Assert.That(problems, Is.Empty,
                "门 1 未通过：\n" + string.Join("\n", problems.Select(p => "  - " + p)));
        }
    }
}
