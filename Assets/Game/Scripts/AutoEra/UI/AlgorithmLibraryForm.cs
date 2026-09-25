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
    /// 模板列表/详情的读模型通道已在 b17 接线（<see cref="AlgorithmReadModels.Create"/> 的库页域），
    /// 三个「创建实例」按钮在 b23 接线：有模板时启用，点击后在当前选中机器上创建草稿实例并
    /// 跳转算法工作台（工作台机器域读模型自动选中新实例，显示图结构与「缺少绑定」校验问题）。
    /// 集中待绑定面板（绑定重绑）尚未接线，后续批完成后再调整创建后的导航目标。
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
            if (_systemTemplatesCreateButton != null) _systemTemplatesCreateButton.onClick.AddListener(OnCreateInstanceFromTemplate);
            if (_playerTemplatesCreateButton != null) _playerTemplatesCreateButton.onClick.AddListener(OnCreateInstanceFromTemplate);
            if (_templateDetailCreateButton != null) _templateDetailCreateButton.onClick.AddListener(OnCreateInstanceFromTemplate);
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
                SetCreateButtonInteractable(false);
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
            SetCreateButtonInteractable(hasTemplates);
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

        /// <summary>
        /// 点「创建实例」→ 取当前选中模板（无选中则取第一个），在当前选中机器上创建草稿实例并
        /// 跳转算法工作台。无选中机器／机器无运行时／模板无效时 <c>InstantiateTemplate</c> 返回 0，
        /// 保持库页不跳转（库页从 FieldHud 机器按钮打开，正常路径下机器上下文已存在）。
        /// </summary>
        private void OnCreateInstanceFromTemplate()
        {
            if (_algorithms == null)
            {
                return;
            }

            AlgorithmDomainSnapshot snapshot = _algorithms.Snapshot;
            if (snapshot.Templates == null || snapshot.Templates.Count == 0)
            {
                return;
            }

            ulong templateId = snapshot.SelectedTemplate?.Id ?? snapshot.Templates[0].Id;
            ulong instanceId = _algorithms.InstantiateTemplate(templateId);
            if (instanceId == 0)
            {
                return;
            }

            AutoEraUiNavigator.Open(this, UIViews.AlgorithmEditorForm);
        }

        /// <summary>按快照是否有模板统一启用/禁用三个「创建实例」按钮。</summary>
        private void SetCreateButtonInteractable(bool interactable)
        {
            if (_systemTemplatesCreateButton != null) _systemTemplatesCreateButton.interactable = interactable;
            if (_playerTemplatesCreateButton != null) _playerTemplatesCreateButton.interactable = interactable;
            if (_templateDetailCreateButton != null) _templateDetailCreateButton.interactable = interactable;
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
