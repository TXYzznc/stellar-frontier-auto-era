using System.Collections.Generic;
using AutoEra.UI.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 算法模板库（规格 12-算法/模板库：系统模板、玩家模板、模板详情）。
    ///
    /// 绑定字段在同名的 .Fields.cs 里，结构由契约生成。
    ///
    /// 算法域的服务层已接入世界与机器运行路径（`AutoEraWorldSession.AlgorithmTemplates`
    /// 与 `RegionMachineRuntimeRegistry` 创建的实例服务），但**模板列表/详情的读模型通道
    /// 尚未接线**——见 <see cref="AlgorithmReadModels.NotWiredReason"/>。所以本页当前呈现的是
    /// 「模板列表未接线」这一真实状态：三个页面写明原因，业务按钮全部禁用，只有返回、
    /// 关闭与两页导航仍然可用。模板读模型接线后本页需要补上模板行渲染，见 b17 design Open Questions。
    /// </summary>
    public sealed partial class AlgorithmLibraryForm : AutoEraShellFormBase
    {
        /// <summary>规格页序：0 系统模板、1 玩家模板、2 模板详情。</summary>
        public const int PageSystemTemplates = 0;
        public const int PagePlayerTemplates = 1;
        public const int PageTemplateDetail = 2;

        /// <summary>导航按钮 → 规格页索引（来源：00-共享外壳-prefab-layout.md 的导航表）。</summary>
        private static readonly int[] NavigationPageIndex = { PageSystemTemplates, PagePlayerTemplates };

        private static readonly UiDetailField[] NoFields = new UiDetailField[0];

        private IAlgorithmReadModel _algorithms;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            if (_navButtons != null)
            {
                for (int i = 0; i < _navButtons.Length; i++)
                {
                    Button button = _navButtons[i];
                    if (button == null || i >= NavigationPageIndex.Length)
                    {
                        continue;
                    }

                    int page = NavigationPageIndex[i];
                    if (page < 0)
                    {
                        continue;
                    }

                    button.onClick.AddListener(() => ShowLibraryPage(page));
                }
            }

            if (_backButton != null) _backButton.onClick.AddListener(RequestCancel);
            if (_closeButton != null) _closeButton.onClick.AddListener(RequestCancel);
        }

        protected override void OnAutoEraOpen()
        {
            int initialPage = TryGetRequest(out AutoEraUiPageRequest pageRequest) ? pageRequest.Page : PageSystemTemplates;
            ShowPage(_pageRoots, initialPage);
            ApplyDefaultFocus(
                _backButton != null ? _backButton.gameObject : null,
                _navButtons != null && _navButtons.Length > 0 && _navButtons[0] != null ? _navButtons[0].gameObject : null);

            _algorithms = AlgorithmReadModels.Create(TryGetSession(out AutoEraUiSession session) ? session : null,
                AlgorithmReadModelDomain.Library);
            _algorithms.Changed += OnAlgorithmSectionChanged;

            Render(_algorithms.Snapshot);
            DisableDomainActions();
        }

        protected override void OnAutoEraClose(bool isShutdown) => ReleaseAlgorithms();

        protected override void OnAutoEraRecycle()
        {
            ReleaseAlgorithms();
            base.OnAutoEraRecycle();
        }

        private void ReleaseAlgorithms()
        {
            if (_algorithms == null)
            {
                return;
            }

            _algorithms.Changed -= OnAlgorithmSectionChanged;
            _algorithms.Dispose();
            _algorithms = null;
        }

        /// <summary>按规格页序切换内容页；越界调用无副作用。</summary>
        public bool ShowLibraryPage(int page) => ShowPage(_pageRoots, page);

        /// <summary>算法域当前数据状态；null 表示读模型尚未建立。测试与调试用。</summary>
        public UiDataState? AlgorithmDataState => _algorithms?.Snapshot.State;

        private void RequestCancel() => TryHandleIntent(AutoEraUiIntent.Cancel);

        private void OnAlgorithmSectionChanged(AlgorithmDomainSection section) => Render(_algorithms.Snapshot);

        private void Render(AlgorithmDomainSnapshot snapshot)
        {
            if (snapshot.State == UiDataState.Unavailable)
            {
                string reason = snapshot.UnavailableReason ?? "算法模板暂不可用。";
                ShowPageUnavailable(reason,
                    _systemTemplatesLoadingState, _systemTemplatesEmptyState, _systemTemplatesErrorState,
                    _systemTemplatesSuccessState, _systemTemplatesDisabledState,
                    _systemTemplatesCatalogBody, _systemTemplatesDetailBody);
                ShowPageUnavailable(reason,
                    _playerTemplatesLoadingState, _playerTemplatesEmptyState, _playerTemplatesErrorState,
                    _playerTemplatesSuccessState, _playerTemplatesDisabledState,
                    _playerTemplatesCatalogBody, _playerTemplatesDetailBody);
                ShowPageUnavailable(reason,
                    _templateDetailLoadingState, _templateDetailEmptyState, _templateDetailErrorState,
                    _templateDetailSuccessState, _templateDetailDisabledState,
                    _templateDetailDefinitionBody, _templateDetailRequirementsBody);
                RenderDetailRows(_systemTemplatesCatalogTemplate, _systemTemplatesCatalogContent, NoFields);
                RenderDetailRows(_systemTemplatesDetailTemplate, _systemTemplatesDetailContent, NoFields);
                RenderDetailRows(_playerTemplatesCatalogTemplate, _playerTemplatesCatalogContent, NoFields);
                RenderDetailRows(_playerTemplatesDetailTemplate, _playerTemplatesDetailContent, NoFields);
                RenderDetailRows(_templateDetailDefinitionTemplate, _templateDetailDefinitionContent, NoFields);
                RenderDetailRows(_templateDetailRequirementsTemplate, _templateDetailRequirementsContent, NoFields);
                return;
            }

            // Empty/Ready＝模板库已接线：按「系统／玩家」分域，各自渲染目录行与选中模板详情。
            var system = new List<UiAlgorithmTemplateRow>();
            var player = new List<UiAlgorithmTemplateRow>();
            if (snapshot.Templates != null)
            {
                for (int i = 0; i < snapshot.Templates.Count; i++)
                {
                    UiAlgorithmTemplateRow row = snapshot.Templates[i];
                    if (row.IsSystem)
                    {
                        system.Add(row);
                    }
                    else
                    {
                        player.Add(row);
                    }
                }
            }

            bool hasTemplates = snapshot.Count > 0;
            IReadOnlyList<UiDetailField> detail = snapshot.TemplateDetail ?? NoFields;

            RenderCatalogPage(
                _systemTemplatesLoadingState, _systemTemplatesEmptyState, _systemTemplatesErrorState,
                _systemTemplatesSuccessState, _systemTemplatesDisabledState, _systemTemplatesCatalogBody,
                _systemTemplatesCatalogTemplate, _systemTemplatesCatalogContent, _systemTemplatesDetailBody,
                _systemTemplatesDetailTemplate, _systemTemplatesDetailContent, system, hasTemplates, detail, "系统模板");

            RenderCatalogPage(
                _playerTemplatesLoadingState, _playerTemplatesEmptyState, _playerTemplatesErrorState,
                _playerTemplatesSuccessState, _playerTemplatesDisabledState, _playerTemplatesCatalogBody,
                _playerTemplatesCatalogTemplate, _playerTemplatesCatalogContent, _playerTemplatesDetailBody,
                _playerTemplatesDetailTemplate, _playerTemplatesDetailContent, player, hasTemplates, detail, "玩家模板");

            RenderTemplateDetailPage(detail, hasTemplates);
        }

        private void RenderCatalogPage(
            GameObject loadingState, GameObject emptyState, GameObject errorState, GameObject successState,
            GameObject disabledState, TMPro.TMP_Text catalogBody, GameObject catalogTemplate, RectTransform catalogContent,
            TMPro.TMP_Text detailBody, GameObject detailTemplate, RectTransform detailContent,
            List<UiAlgorithmTemplateRow> rows, bool hasTemplates, IReadOnlyList<UiDetailField> detail, string category)
        {
            if (!hasTemplates)
            {
                ShowPageEmpty("模板库为空。", loadingState, emptyState, errorState, successState, disabledState,
                    catalogBody, detailBody);
                RenderDetailRows(catalogTemplate, catalogContent, NoFields);
                RenderDetailRows(detailTemplate, detailContent, NoFields);
                return;
            }

            SetState(loadingState, false);
            SetState(emptyState, rows.Count == 0);
            SetState(errorState, false);
            SetState(successState, rows.Count > 0);
            SetState(disabledState, false);

            RenderListRows(catalogTemplate, catalogContent, rows.Count,
                (index, item) => item.Bind(index, rows[index].Label, rows[index].Status,
                    i => SelectTemplateRow(rows[i].Id)));
            SetText(catalogBody, rows.Count > 0 ? category + " " + rows.Count + " 个（点一行查看详情）" : category + "暂无模板。");

            RenderDetailRows(detailTemplate, detailContent, detail);
            SetText(detailBody, rows.Count > 0 ? "选中模板详情：" : "该类别暂无模板。");
        }

        private void RenderTemplateDetailPage(IReadOnlyList<UiDetailField> detail, bool hasTemplates)
        {
            if (!hasTemplates)
            {
                ShowPageEmpty("模板库为空。", _templateDetailLoadingState, _templateDetailEmptyState,
                    _templateDetailErrorState, _templateDetailSuccessState, _templateDetailDisabledState,
                    _templateDetailDefinitionBody, _templateDetailRequirementsBody);
                RenderDetailRows(_templateDetailDefinitionTemplate, _templateDetailDefinitionContent, NoFields);
                RenderDetailRows(_templateDetailRequirementsTemplate, _templateDetailRequirementsContent, NoFields);
                return;
            }

            SetState(_templateDetailLoadingState, false);
            SetState(_templateDetailEmptyState, false);
            SetState(_templateDetailErrorState, false);
            SetState(_templateDetailSuccessState, true);
            SetState(_templateDetailDisabledState, false);

            RenderDetailRows(_templateDetailDefinitionTemplate, _templateDetailDefinitionContent, detail);
            RenderDetailRows(_templateDetailRequirementsTemplate, _templateDetailRequirementsContent, NoFields);
            SetText(_templateDetailDefinitionBody, "模板定义：名称、版本、结构摘要与逻辑成本。");
            SetText(_templateDetailRequirementsBody, "绑定需求：系统模板不预置绑定，实例化时由玩家补全。");
        }

        private void SelectTemplateRow(ulong templateId)
        {
            if (_algorithms != null)
            {
                _algorithms.SelectTemplate(templateId);
            }
        }

        private static void SetText(TMPro.TMP_Text text, string value)
        {
            if (text != null)
            {
                text.SetText(value ?? string.Empty);
            }
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
