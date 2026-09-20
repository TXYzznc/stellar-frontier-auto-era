using AutoEra.UI.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 保存退出流程（规格 06-系统与设置/ExitFlow）。
    ///
    /// 绑定字段在同名的 .Fields.cs 里，结构由契约生成。
    ///
    /// 「保存并退出」需要世界进度层把世界状态写成存档内容，该层尚未接入，
    /// 因此重试与强制退出被**禁用并说明原因**；「返回游戏」照常可用——
    /// 无论流程走到哪一步，玩家永远有一条走出去的路（规格：失败可重试／返回游戏／明确强退）。
    /// </summary>
    public sealed partial class ExitFlowForm : AutoEraShellFormBase
    {
        private const string SaveLayerMissing = "世界进度层尚未接入：保存并退出暂不可用。返回游戏不受影响。";

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            if (_backButton != null) _backButton.onClick.AddListener(Resume);
            if (_closeButton != null) _closeButton.onClick.AddListener(Resume);
            if (_resumeButton != null) _resumeButton.onClick.AddListener(Resume);
        }

        protected override void OnAutoEraOpen()
        {
            ShowPage(_pageRoots, 0);
            ApplyDefaultFocus(_resumeButton != null ? _resumeButton.gameObject : null, null);
            ApplySaveLayerAvailability();
        }

        /// <summary>按规格页序切换内容页；越界调用无副作用。</summary>
        public bool ShowFormPage(int page) => ShowPage(_pageRoots, page);

        /// <summary>退出流程是否可以真正执行保存；false 表示本页只做说明。</summary>
        public bool CanSaveAndExit => false;

        private void Resume() => CloseSelf();

        private void ApplySaveLayerAvailability()
        {
            if (_retryButton != null)
            {
                _retryButton.interactable = false;
            }

            if (_forceButton != null)
            {
                _forceButton.interactable = false;
            }

            SetState(_exitFlowLoadingState, false);
            SetState(_exitFlowSuccessState, false);
            SetState(_exitFlowErrorState, false);
            SetState(_exitFlowEmptyState, true);
            SetState(_exitFlowDisabledState, !CanSaveAndExit);

            if (_exitFlowStageBody != null)
            {
                _exitFlowStageBody.SetText(SaveLayerMissing);
            }

            if (_exitFlowFailureBody != null)
            {
                _exitFlowFailureBody.SetText("还没有执行过保存，因此没有失败记录。");
            }
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
