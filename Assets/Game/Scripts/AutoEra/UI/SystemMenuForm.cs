using AutoEra.UI.Contracts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 系统菜单（规格 06-系统与设置/SystemMenu）。
    ///
    /// 单体式暂停菜单：继续游戏、设置、帮助、返回主菜单、保存退出。
    /// 绑定字段在同名的 .Fields.cs 里，结构由契约生成。
    ///
    /// 「返回主菜单」不能直接切流程——界面拿不到流程实例。它把意图写进应用上下文
    /// （<see cref="AutoEra.Application.AutoEraApplicationContext.RequestReturnToMenu"/>），
    /// 由 <c>AutoEraWorldProcedure</c> 在下一帧消费。这样界面既不持有流程，也不需要全局定位器。
    /// </summary>
    public sealed partial class SystemMenuForm : AutoEraShellFormBase
    {
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            // 面板上的返回／关闭与「继续游戏」是同一件事：回到游戏，不经过 EscapeClose 登记值。
            if (_backButton != null) _backButton.onClick.AddListener(Resume);
            if (_closeButton != null) _closeButton.onClick.AddListener(Resume);
            if (_resumeButton != null) _resumeButton.onClick.AddListener(Resume);
            if (_settingsButton != null) _settingsButton.onClick.AddListener(OpenSettings);
            if (_helpButton != null) _helpButton.onClick.AddListener(OpenHelp);
            if (_quitButton != null) _quitButton.onClick.AddListener(OpenExitFlow);
            if (_returnToMenuButton != null) _returnToMenuButton.onClick.AddListener(ReturnToMainMenu);
        }

        protected override void OnAutoEraOpen()
        {
            ShowPage(_pageRoots, 0);
            // 「继续游戏」是最安全的首选动作，优先于面板上的其它命令。
            ApplyDefaultFocus(_resumeButton != null ? _resumeButton.gameObject : null, null);
            RenderSessionSummary();
        }

        /// <summary>按规格页序切换内容页；越界调用无副作用。</summary>
        public bool ShowFormPage(int page) => ShowPage(_pageRoots, page);

        private void OpenSettings() => AutoEraUiNavigator.Open(this, UIViews.SettingsForm);

        private void OpenHelp() => AutoEraUiNavigator.Open(this, UIViews.HelpForm);

        private void OpenExitFlow() => AutoEraUiNavigator.Open(this, UIViews.ExitFlowForm);

        private void Resume() => CloseSelf();

        private void ReturnToMainMenu()
        {
            if (TryGetSession(out AutoEraUiSession session) && session.Application != null)
            {
                session.Application.RequestReturnToMenu();
            }

            CloseSelf();
        }

        /// <summary>会话栏只陈述真实状态：有没有进行中的区域。没有数据就不编造。</summary>
        private void RenderSessionSummary()
        {
            if (_systemMenuSessionBody == null)
            {
                return;
            }

            bool hasWorld = TryGetSession(out AutoEraUiSession session) && session.HasWorld;
            _systemMenuSessionBody.SetText(hasWorld
                ? "进行中：初始区域"
                : "没有进行中的区域（从主菜单进入后才有现场）。");
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
