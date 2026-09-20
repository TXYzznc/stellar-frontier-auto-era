using AutoEra.UI.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// ProgressReportForm 的 GF 桥。结构由 Docs/Development/UI-PrefabLayouts/ProgressReportForm.contract.json 生成
    /// （设计来源：Docs/GameDesign/03-玩家体验/界面规格）。
    ///
    /// 绑定字段在同名的 ProgressReportForm.Fields.cs 里（同一 partial 类）；本文件只有类逻辑：
    /// 规格页序切换、取消意图与默认焦点。Grp_PageHost 下的内容页顺序即规格页序。
    /// </summary>
    public sealed partial class ProgressReportForm : AutoEraShellFormBase
    {
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            if (_backButton != null)
            {
                _backButton.onClick.AddListener(RequestCancel);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(RequestCancel);
            }

        }

        protected override void OnAutoEraOpen()
        {
            ShowPage(_pageRoots, 0);
            ApplyDefaultFocus(_backButton != null ? _backButton.gameObject : null, null);
        }

        /// <summary>按规格页序切换内容页；越界调用无副作用。</summary>
        public bool ShowFormPage(int page) => ShowPage(_pageRoots, page);

        private void RequestCancel() => TryHandleIntent(AutoEraUiIntent.Cancel);

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
