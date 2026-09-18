using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;
using UnityGameFramework.Runtime;

namespace AutoEra.UI.Testing
{
    /// <summary>
    /// 界面测试准备上下文：由测试工具创建并传入准备钩子。
    /// 钩子通过 <see cref="FormOpened"/> 订阅打开回调，禁止直接改写 <see cref="UIParams.OpenCallback"/>（工具自身持有它）。
    /// </summary>
    public sealed class UiPanelTestSetupContext
    {
        private readonly List<Action<UIFormLogic>> _openHandlers = new List<Action<UIFormLogic>>();

        public UiPanelTestSetupContext(UIViews view, UIParams parameters)
        {
            View = view;
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        /// <summary>本次要打开的界面。</summary>
        public UIViews View { get; }

        /// <summary>打开参数；钩子可就地修改（SortOrder、AllowEscapeClose 等留空项由 UITable 兜底）。</summary>
        public UIParams Parameters { get; }

        /// <summary>界面真正打开后的回调（GF 生命周期内触发，与 UIParams.OpenCallback 同一时机）。</summary>
        public event Action<UIFormLogic> FormOpened
        {
            add => _openHandlers.Add(value);
            remove => _openHandlers.Remove(value);
        }

        /// <summary>由测试工具转调：工具把 UIParams.OpenCallback 指向本方法，界面打开后通知全部订阅者。</summary>
        public void RaiseFormOpened(UIFormLogic form)
        {
            for (int i = 0; i < _openHandlers.Count; i++)
            {
                _openHandlers[i]?.Invoke(form);
            }
        }
    }

    /// <summary>
    /// 界面测试准备钩子：为"正常显示需要流程数据/预加载"的界面登记最小化准备动作。
    /// 每个有依赖的界面配套实现一个类并用 <see cref="UiPanelTestSetupAttribute"/> 登记，
    /// UI面板测试管理工具会在打开该界面前执行 <see cref="PrepareAsync"/>。
    /// 实现必须无构造依赖（工具通过 Activator 创建），并只做测试用准备，不写存档。
    /// </summary>
    public interface IUiPanelTestSetup
    {
        UniTask PrepareAsync(UiPanelTestSetupContext context);
    }

    /// <summary>把准备钩子实现类登记到指定界面。</summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class UiPanelTestSetupAttribute : Attribute
    {
        public UiPanelTestSetupAttribute(UIViews view, string note = "")
        {
            View = view;
            Note = note ?? string.Empty;
        }

        /// <summary>登记到的界面。</summary>
        public UIViews View { get; }

        /// <summary>准备内容说明，显示在测试工具中。</summary>
        public string Note { get; }
    }
}
