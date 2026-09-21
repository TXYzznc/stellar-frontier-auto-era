using System.Collections.Generic;
using AutoEra.UI.Contracts;
using AutoEra.World.Identity;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 组件库（规格 09-资产与模板目录：散件、已安装组件、组件详情三页）。
    ///
    /// 绑定字段在同名的 .Fields.cs 里，结构由契约生成。
    ///
    /// **这一域不需要新建模拟系统**：型号与规格来自 `ComponentDefinitions` 数据表（由
    /// `MachineCatalog` 解析），实例与安装位置来自机器花名册。它此前显示「未接入」的真正原因是
    /// **界面没有观察入口**——花名册把组件当成内部结构，没有任何读模型去枚举它。
    /// 所以本页做的是接线：把两半既有事实按玩家能读的方式摆出来。
    ///
    /// 「安装／卸载／改装」这些**写**动作仍然禁用并写明原因：安装要选目标机器的槽位、
    /// 卸载要过容量与算力占用闸门、改装还没有规则载体，这些都不是只读接线能负责的。
    /// 只读部分是真实数据，写部分是明确不可用——两者不混。
    /// </summary>
    public sealed partial class ComponentLibraryForm : AutoEraShellFormBase
    {
        /// <summary>规格页序：0 散件、1 已安装组件、2 组件详情。</summary>
        public const int PageLooseComponents = 0;
        public const int PageInstalledComponents = 1;
        public const int PageComponentDetail = 2;

        private const string WriteMissing =
            "安装、拆卸与升级在**机器整备页**发起：未部署机器处于整备环境，允许安装、拆卸与升级组件；"
            + "机器部署到世界后，硬件修改必须进入现场交互。本页负责查看、比较与整理。";

        private const string EmptyLooseReason =
            "没有散件：所有组件都已经装在机器上。新组件由组件库购入或从机器拆下后出现在这里（经济域尚未接入）。";

        private const string EmptyInstalledReason =
            "没有已安装组件：在机器整备页把散件装到机器槽位后，这里会显示它的所在机器与槽位。";

        private const string DetailMissingReason =
            "未选择组件：在左侧列表中选择一行查看它的型号规格与位置。";

        private static readonly UiDetailField[] NoFields = new UiDetailField[0];

        /// <summary>导航按钮 → 规格页索引（来源：契约的 `navigationPageIndex`）。</summary>
        private static readonly int[] NavigationPageIndex = { PageLooseComponents, PageInstalledComponents };

        private IComponentReadModel _components;

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
                    button.onClick.AddListener(() => ShowLibraryPage(page));
                }
            }

            if (_backButton != null) _backButton.onClick.AddListener(RequestCancel);
            if (_closeButton != null) _closeButton.onClick.AddListener(RequestCancel);
        }

        protected override void OnAutoEraOpen()
        {
            int initialPage = TryGetRequest(out AutoEraUiPageRequest pageRequest) ? pageRequest.Page : PageLooseComponents;
            ShowPage(_pageRoots, initialPage);
            ApplyDefaultFocus(
                _backButton != null ? _backButton.gameObject : null,
                _navButtons != null && _navButtons.Length > 0 && _navButtons[0] != null ? _navButtons[0].gameObject : null);

            _components = ComponentReadModels.Create(TryGetSession(out AutoEraUiSession session) ? session : null);
            _components.Changed += OnComponentSectionChanged;

            Render(_components.Snapshot);
            // 写动作（安装／卸载／改装）尚未接入，按结构名统一禁用；列表行不受影响。
            DisableDomainActions();
        }

        protected override void OnAutoEraClose(bool isShutdown) => ReleaseComponents();

        protected override void OnAutoEraRecycle()
        {
            ReleaseComponents();
            base.OnAutoEraRecycle();
        }

        private void ReleaseComponents()
        {
            if (_components == null)
            {
                return;
            }

            _components.Changed -= OnComponentSectionChanged;
            _components.Dispose();
            _components = null;
        }

        /// <summary>按规格页序切换内容页；越界调用无副作用。</summary>
        public bool ShowLibraryPage(int page) => ShowPage(_pageRoots, page);

        /// <summary>组件域当前数据状态；null 表示读模型尚未建立。测试与调试用。</summary>
        public UiDataState? ComponentDataState => _components?.Snapshot.State;

        /// <summary>当前选中组件的稳定身份；未选中实例（含选中了合并组）时为 <see cref="PersistentId.Invalid"/>。测试与调试用。</summary>
        public PersistentId SelectedComponentId => _components?.Snapshot.SelectedId ?? PersistentId.Invalid;

        /// <summary>散件页合并后的行数。测试与调试用。</summary>
        public int LooseGroupCount => _components?.Snapshot.LooseGroupCount ?? 0;

        /// <summary>散件个体数（合并前后的区别就是这一对）。测试与调试用。</summary>
        public int LooseComponentCount => _components?.Snapshot.LooseCount ?? 0;

        /// <summary>当前是否选中了一整组散件。测试与调试用。</summary>
        public bool HasGroupSelection => _components?.Snapshot.HasGroupSelection ?? false;

        private void RequestCancel() => TryHandleIntent(AutoEraUiIntent.Cancel);

        private void OnComponentSectionChanged(ComponentDomainSection section) => Render(_components.Snapshot);

        private void OnComponentRowClicked(PersistentId id) => _components?.Select(id);

        private void OnComponentGroupClicked(AutoEra.Machines.HardwareKind kind, int modelId, int level) =>
            _components?.SelectGroup(kind, modelId, level);

        private void Render(ComponentDomainSnapshot snapshot)
        {
            if (snapshot.State == UiDataState.Unavailable)
            {
                string reason = snapshot.UnavailableReason ?? "组件数据不可用。";
                RenderSecondLevelPage(_looseComponentsLoadingState, _looseComponentsEmptyState, _looseComponentsErrorState,
                    _looseComponentsSuccessState, _looseComponentsDisabledState, reason, null, true, false,
                    _looseComponentsCatalogBody, _looseComponentsDetailsBody);
                RenderSecondLevelPage(_installedComponentsLoadingState, _installedComponentsEmptyState, _installedComponentsErrorState,
                    _installedComponentsSuccessState, _installedComponentsDisabledState, reason, null, true, false,
                    _installedComponentsCatalogBody, _installedComponentsDetailsBody);
                RenderDetailStates(reason, true, false,
                    _componentDetailIdentityBody, _componentDetailAttributesBody, _componentDetailCompareBody);

                ClearAllRows();
                return;
            }

            RenderLoosePage(snapshot);
            RenderInstalledPage(snapshot);
            RenderDetailPage(snapshot);
        }

        private void RenderLoosePage(ComponentDomainSnapshot snapshot)
        {
            // 列表用**合并行**（规格：完全相同的未安装组件按类型／型号／等级合并显示数量），
            // 详情与选中仍落到具体实例——组里每一件的稳定身份都保留着。
            bool empty = snapshot.LooseGroupCount == 0;
            string content = "散件 " + snapshot.LooseCount + " 件（合并为 " + snapshot.LooseGroupCount
                + " 行）：完全相同的组件按类型／型号／等级合并显示数量，展开后仍保留各实例。"
                + "安装与拆卸在机器整备页发起。";
            RenderSecondLevelPage(_looseComponentsLoadingState, _looseComponentsEmptyState, _looseComponentsErrorState,
                _looseComponentsSuccessState, _looseComponentsDisabledState,
                EmptyLooseReason, content, false, empty,
                _looseComponentsCatalogBody, _looseComponentsDetailsBody);

            RenderListRows(_looseComponentsCatalogTemplate, _looseComponentsCatalogContent, snapshot.LooseGroupCount,
                (index, item) =>
                {
                    UiComponentGroup group = snapshot.LooseGroups[index];
                    item.Bind(index, group.Label, group.Status,
                        _ => OnComponentGroupClicked(group.Kind, group.ModelId, group.Level));
                });
            RenderDetailRows(_looseComponentsDetailsTemplate, _looseComponentsDetailsContent, snapshot.Detail);
        }

        private void RenderInstalledPage(ComponentDomainSnapshot snapshot)
        {
            bool empty = snapshot.InstalledCount == 0;
            string content = "已安装 " + snapshot.InstalledCount
                + " 件：每件都显示所在机器与槽位。已安装组件不能同时用于其它机器；升级在机器整备页（未部署）"
                + "或现场交互（已部署）发起。";
            RenderSecondLevelPage(_installedComponentsLoadingState, _installedComponentsEmptyState, _installedComponentsErrorState,
                _installedComponentsSuccessState, _installedComponentsDisabledState,
                EmptyInstalledReason, content, false, empty,
                _installedComponentsCatalogBody, _installedComponentsDetailsBody);

            RenderListRows(_installedComponentsCatalogTemplate, _installedComponentsCatalogContent, snapshot.InstalledCount,
                (index, item) => item.Bind(index, snapshot.Installed[index].Name, snapshot.Installed[index].Status,
                    _ => OnComponentRowClicked(snapshot.Installed[index].Id)));
            RenderDetailRows(_installedComponentsDetailsTemplate, _installedComponentsDetailsContent, snapshot.Detail);
        }

        private void RenderDetailPage(ComponentDomainSnapshot snapshot)
        {
            bool selected = snapshot.HasSelection && snapshot.Detail != null && snapshot.Detail.Count > 0;
            RenderDetailStates(selected ? WriteMissing : DetailMissingReason, false, !selected,
                _componentDetailIdentityBody, _componentDetailAttributesBody, _componentDetailCompareBody);

            // 身份栏与规格栏都来自同一份详情（目录给规格、花名册给实例与位置）。
            RenderDetailRows(_componentDetailIdentityTemplate, _componentDetailIdentityContent, snapshot.Detail);
            RenderDetailRows(_componentDetailAttributesTemplate, _componentDetailAttributesContent, snapshot.Detail);

            // 对比需要第二个组件作为参照，本页还没有对比入口——显式清空而不是留示例行。
            RenderDetailRows(_componentDetailCompareTemplate, _componentDetailCompareContent, NoFields);
            SetText(_componentDetailCompareBody,
                "对比尚未接入：本页还需要「选择第二个组件作为参照」这条通道。");
        }

        /// <summary>
        /// 散件页与已安装页的状态组。两页形状相同，状态互斥：
        /// 不可用时 Disabled；这一页自己没有内容时 Empty；有内容时 Success。
        /// 「本页为空」与「整域不可用」必须分开——前者换一页就有内容，后者怎么点都没有。
        /// 有内容时正文陈述**这一页的真实构成与规则**，而不是留一句泛泛的不可用说明。
        /// </summary>
        private void RenderSecondLevelPage(GameObject loadingState, GameObject emptyState, GameObject errorState,
            GameObject successState, GameObject disabledState, string emptyReason, string contentText,
            bool unavailable, bool empty,
            TMPro.TMP_Text firstBody, TMPro.TMP_Text secondBody)
        {
            SetState(loadingState, false);
            SetState(errorState, false);
            SetState(disabledState, unavailable);
            SetState(emptyState, !unavailable && empty);
            SetState(successState, !unavailable && !empty);

            string text = unavailable || empty ? (emptyReason ?? string.Empty) : (contentText ?? string.Empty);
            SetText(firstBody, text);
            SetText(secondBody, text);
        }

        private void RenderDetailStates(string reason, bool unavailable, bool empty, params TMPro.TMP_Text[] bodies)
        {
            SetState(_componentDetailLoadingState, false);
            SetState(_componentDetailErrorState, false);
            SetState(_componentDetailDisabledState, unavailable);
            SetState(_componentDetailEmptyState, !unavailable && empty);
            SetState(_componentDetailSuccessState, !unavailable && !empty);

            bool missing = unavailable || empty;
            for (int i = 0; i < bodies.Length; i++)
            {
                SetText(bodies[i], missing ? (reason ?? string.Empty) : WriteMissing);
            }
        }

        private void ClearAllRows()
        {
            RenderDetailRows(_looseComponentsCatalogTemplate, _looseComponentsCatalogContent, NoFields);
            RenderDetailRows(_looseComponentsDetailsTemplate, _looseComponentsDetailsContent, NoFields);
            RenderDetailRows(_installedComponentsCatalogTemplate, _installedComponentsCatalogContent, NoFields);
            RenderDetailRows(_installedComponentsDetailsTemplate, _installedComponentsDetailsContent, NoFields);
            RenderDetailRows(_componentDetailIdentityTemplate, _componentDetailIdentityContent, NoFields);
            RenderDetailRows(_componentDetailAttributesTemplate, _componentDetailAttributesContent, NoFields);
            RenderDetailRows(_componentDetailCompareTemplate, _componentDetailCompareContent, NoFields);
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
