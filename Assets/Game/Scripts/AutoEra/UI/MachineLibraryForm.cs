using System.Collections.Generic;
using AutoEra.UI.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 机器库与机器整备（规格 09-机器与组件两分页 ＋ 05-机器整备）。
    ///
    /// 三页看的是**同一个机器花名册**：整备页、库中机器（未部署）、已部署机器。
    /// 因此数据来源是同一个机器域读模型，页面只按 <see cref="UiMachineRow.Deployed"/> 过滤，
    /// 不各自去问领域对象。绑定字段在同名的 .Fields.cs 里，结构由契约生成。
    ///
    /// 关于「库中／已部署」的空态：判定用的是**本页过滤后**的条数，而不是整个花名册的状态——
    /// 规格要求单一分区为空不得抹掉其它有效分区，所以库中页为空时已部署页照常显示。
    ///
    /// 需要机器改装、库存与经济域的操作（安装／卸载／升级／出售／部署／改名）一律**禁用并写明原因**；
    /// 当前真正可用的只有「在中枢查看」与两页之间的切换。
    /// </summary>
    public sealed partial class MachineLibraryForm : AutoEraShellFormBase
    {
        /// <summary>规格页序：0 整备、1 库中机器、2 已部署机器。</summary>
        public const int PagePreparation = 0;
        public const int PageUndeployed = 1;
        public const int PageDeployed = 2;

        private static readonly UiDetailField[] NoFields = new UiDetailField[0];

        /// <summary>本页可见行的快照索引（复用，避免每次刷新分配）。</summary>
        private readonly List<int> _visibleRows = new List<int>(32);

        private const string RefitMissing = "机器改装、库存与经济系统尚未接入：安装／卸载／升级／出售／部署／改名暂不可用。";
        private const string AssemblyMissing = "装配、载体与就绪数据尚未接入：本栏暂无可显示内容。";

        /// <summary>导航按钮 → 规格页索引（来源：00-共享外壳-prefab-layout.md 的导航表）。</summary>
        private static readonly int[] NavigationPageIndex = { PageUndeployed, PageDeployed };

        private IMachineReadModel _machines;

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

            // 两页之间的切换与「在中枢查看」是当前真正可用的动作。
            if (_undeployedViewDeployedButton != null) _undeployedViewDeployedButton.onClick.AddListener(() => ShowLibraryPage(PageDeployed));
            if (_deployedReturnButton != null) _deployedReturnButton.onClick.AddListener(() => ShowLibraryPage(PageUndeployed));
            if (_deployedHubButton != null) _deployedHubButton.onClick.AddListener(OpenSelectedInHub);
        }

        protected override void OnAutoEraOpen()
        {
            int initialPage = TryGetRequest(out AutoEraUiPageRequest pageRequest) ? pageRequest.Page : PageUndeployed;
            ShowPage(_pageRoots, initialPage);
            ApplyDefaultFocus(
                _backButton != null ? _backButton.gameObject : null,
                _navButtons != null && _navButtons.Length > 0 && _navButtons[0] != null
                    ? _navButtons[0].gameObject
                    : null);

            _machines = MachineReadModels.Create(TryGetSession(out AutoEraUiSession session) ? session : null);
            _machines.Changed += OnMachineSectionChanged;

            RenderAll(_machines.Snapshot);
        }

        protected override void OnAutoEraClose(bool isShutdown) => ReleaseMachines();

        protected override void OnAutoEraRecycle()
        {
            ReleaseMachines();
            base.OnAutoEraRecycle();
        }

        private void ReleaseMachines()
        {
            if (_machines == null)
            {
                return;
            }

            _machines.Changed -= OnMachineSectionChanged;
            _machines.Dispose();
            _machines = null;
        }

        /// <summary>按规格页序切换内容页；越界调用无副作用。</summary>
        public bool ShowLibraryPage(int page) => ShowPage(_pageRoots, page);

        /// <summary>当前机器花名册的数据状态；null 表示读模型尚未建立。测试用。</summary>
        public UiDataState? MachineDataState => _machines?.Snapshot.State;

        /// <summary>当前选中机器的名称；未选中时为空串。测试与调试用。</summary>
        public string SelectedMachineName
        {
            get
            {
                MachineDomainSnapshot snapshot = _machines != null ? _machines.Snapshot : default;
                int index = SelectedIndex;
                return index >= 0 ? snapshot.Machines[index].Name : string.Empty;
            }
        }

        private int SelectedIndex
        {
            get
            {
                if (_machines == null)
                {
                    return -1;
                }

                MachineDomainSnapshot snapshot = _machines.Snapshot;
                for (int i = 0; i < snapshot.Count; i++)
                {
                    if (snapshot.Machines[i].Id == _machines.SelectedId)
                    {
                        return i;
                    }
                }

                return -1;
            }
        }

        private void RequestCancel() => TryHandleIntent(AutoEraUiIntent.Cancel);

        private void OnMachineSectionChanged(MachineDomainSection section)
        {
            if (_machines == null)
            {
                return;
            }

            if (section == MachineDomainSection.List)
            {
                RenderCatalog(_machines.Snapshot, deployed: false);
                RenderCatalog(_machines.Snapshot, deployed: true);
            }
            else
            {
                RenderDetail(_machines.Snapshot);
                RenderPreparation(_machines.Snapshot);
            }
        }

        private void RenderAll(MachineDomainSnapshot snapshot)
        {
            RenderCatalog(snapshot, deployed: false);
            RenderCatalog(snapshot, deployed: true);
            RenderDetail(snapshot);
            RenderPreparation(snapshot);
            ApplyUnavailableActions();
        }

        // -------------------------------------------------- 库中／已部署两页

        private void RenderCatalog(MachineDomainSnapshot snapshot, bool deployed)
        {
            RectTransform content = deployed ? _deployedMachinesCatalogContent : _undeployedMachinesCatalogContent;
            GameObject template = deployed ? _deployedMachinesCatalogTemplate : _undeployedMachinesCatalogTemplate;
            TMPro.TMP_Text body = deployed ? _deployedMachinesCatalogBody : _undeployedMachinesCatalogBody;

            int shown = 0;
            if (snapshot.State != UiDataState.Unavailable && template != null && content != null)
            {
                // 先按部署状态过滤出索引，再整体交给池渲染：过滤与渲染分开，行索引不会与快照索引混。
                _visibleRows.Clear();
                for (int i = 0; i < snapshot.Count; i++)
                {
                    if (snapshot.Machines[i].Deployed == deployed)
                    {
                        _visibleRows.Add(i);
                    }
                }

                shown = RenderListRows(template, content, _visibleRows.Count,
                    (position, item) => item.Bind(_visibleRows[position], snapshot.Machines[_visibleRows[position]].Name,
                        snapshot.Machines[_visibleRows[position]].Status, OnMachineRowClicked));
            }

            bool unavailable = snapshot.State == UiDataState.Unavailable;
            bool empty = !unavailable && shown == 0;

            SetState(deployed ? _deployedMachinesLoadingState : _undeployedMachinesLoadingState, false);
            SetState(deployed ? _deployedMachinesErrorState : _undeployedMachinesErrorState, false);
            SetState(deployed ? _deployedMachinesEmptyState : _undeployedMachinesEmptyState, empty);
            SetState(deployed ? _deployedMachinesSuccessState : _undeployedMachinesSuccessState, !unavailable && !empty);
            SetState(deployed ? _deployedMachinesDisabledState : _undeployedMachinesDisabledState, unavailable);

            if (body == null)
            {
                return;
            }

            if (unavailable)
            {
                body.SetText(snapshot.UnavailableReason ?? "机器数据不可用");
            }
            else if (empty)
            {
                body.SetText(deployed
                    ? "还没有已部署的机器；在库中机器页选择一台并部署后会出现在这里。"
                    : "库中还没有机器；在机器库创建后会出现在这里。");
            }
            else
            {
                body.SetText((deployed ? "已部署 " : "库中 ") + AutoEraUiFormat.Count(shown) + " 台");
            }
        }

        private void OnMachineRowClicked(int index)
        {
            if (_machines == null)
            {
                return;
            }

            MachineDomainSnapshot snapshot = _machines.Snapshot;
            if (snapshot.Machines == null || index < 0 || index >= snapshot.Machines.Count)
            {
                return;
            }

            _machines.Select(snapshot.Machines[index].Id);
        }

        private void RenderDetail(MachineDomainSnapshot snapshot)
        {
            RenderDetailRows(_undeployedMachinesDetailTemplate, _undeployedMachinesDetailContent, snapshot.Detail);
            RenderDetailRows(_deployedMachinesDetailTemplate, _deployedMachinesDetailContent, snapshot.Detail);

            SetBody(_undeployedMachinesDetailBody, snapshot, RefitMissing);
            SetBody(_deployedMachinesDetailBody, snapshot, RefitMissing);
        }

        /// <summary>整备页的三栏需要装配／载体／就绪数据；这些域还没接入，因此明确说明而不是留空。</summary>
        private void RenderPreparation(MachineDomainSnapshot snapshot)
        {
            SetState(_machinePreparationLoadingState, false);
            SetState(_machinePreparationErrorState, false);
            SetState(_machinePreparationEmptyState, !snapshot.HasSelection);
            SetState(_machinePreparationSuccessState, snapshot.HasSelection);
            SetState(_machinePreparationDisabledState, snapshot.State == UiDataState.Unavailable);

            SetBody(_machinePreparationCarrierBody, snapshot, AssemblyMissing);
            SetBody(_machinePreparationAssemblyBody, snapshot, AssemblyMissing);
            SetBody(_machinePreparationReadinessBody, snapshot, AssemblyMissing);

            // 三栏没有数据来源，显式清空而不是留着预制体里的示例行。
            RenderDetailRows(_machinePreparationCarrierTemplate, _machinePreparationCarrierContent, NoFields);
            RenderDetailRows(_machinePreparationAssemblyTemplate, _machinePreparationAssemblyContent, NoFields);
            RenderDetailRows(_machinePreparationReadinessTemplate, _machinePreparationReadinessContent, NoFields);
        }

        private static void SetBody(TMPro.TMP_Text body, MachineDomainSnapshot snapshot, string hasSelectionReason)
        {
            if (body == null)
            {
                return;
            }

            if (snapshot.State == UiDataState.Unavailable)
            {
                body.SetText(snapshot.UnavailableReason ?? "机器数据不可用");
                return;
            }

            if (snapshot.HasSelection)
            {
                body.SetText(hasSelectionReason ?? string.Empty);
                return;
            }

            body.SetText("未选择机器：在列表中选择一台查看详情。");
        }

        // -------------------------------------------------- 可用／不可用的动作

        private void ApplyUnavailableActions()
        {
            // 改装、库存与经济域未接入：这些按钮禁用，原因写在整备页与列表页的说明文本里。
            Disable(_preparationRenameButton, _preparationInstallButton, _preparationUnloadButton,
                _preparationUpgradeButton, _preparationSellButton, _preparationDeployButton,
                _undeployedPrepareButton, _undeployedRenameButton, _undeployedDeployButton, _undeployedSellButton,
                _deployedRenameButton, _deployedLocateButton);
        }

        private static void Disable(params Button[] buttons)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null)
                {
                    buttons[i].interactable = false;
                }
            }
        }

        /// <summary>把当前选中的机器带到中枢的远程机器详情页——当前唯一能真正走通的动作。</summary>
        private void OpenSelectedInHub()
        {
            if (_machines == null || !_machines.Snapshot.HasSelection)
            {
                return;
            }

            AutoEraUiNavigator.Open(this, UIViews.BaseCommandHubForm,
                new AutoEraUiPageRequest(BaseCommandHubForm.PageRemoteMachine));
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
