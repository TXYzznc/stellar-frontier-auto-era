using System;
using System.Text;
using AutoEra.UI.Contracts;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 现场 HUD（规格 03-世界HUD 五模块 ＋ 05／06／07 的现场内容页）。
    ///
    /// 结构完全由 Docs/Development/UI-PrefabLayouts/FieldHudForm.contract.json 生成，
    /// 绑定字段在同名的 .Fields.cs 里。
    ///
    /// 两条结构事实（来自 00-共享外壳 的 FieldHudForm 段）：
    /// ① HUD 五模块同时常驻，只有现场内容页互斥，默认全部关闭；
    /// ② 侧栏关闭按钮挂在 Grp_PageHost 下且只在侧栏开启时显示。
    ///
    /// 数据来源分两条，各有各的域：
    /// ① **区域域**：<see cref="BindRegion"/>（InitialRegionScene.BindHud 注入），提供选中对象；
    /// ② **机器域**：打开参数里的 `AutoEraUiSession` → 机器域读模型，提供机器详情。
    /// 历史：这里曾有一个 `BindMachines(MachineRoster)` 直通入口，与读模型形成第二条数据路；
    /// 已删除——HUD 不再持有花名册，机器信息只经读模型拿。
    /// </summary>
    public sealed partial class FieldHudForm : AutoEraShellFormBase
    {
        // 绑定字段由 FieldHudForm.Fields.cs 依契约生成，与本文件同属一个 partial 类；
        // 新增节点引用请改契约后重新生成，不要在此手写字段。
        private const string ResidentHudPrefix = "Panel_PageHud";

        private static readonly UiDetailField[] NoFields = new UiDetailField[0];

        private readonly StringBuilder _timeBuffer = new StringBuilder(32);
        private InitialRegion _region;
        private IMachineReadModel _machineReadModel;
        private IRegionReadModel _regionReadModel;
        private PersistentId _selectedMachineId = PersistentId.Invalid;
        private bool _fieldAccessible = true;
        private bool _managementOpen;
        private int _openFieldPage = -1;
        private long _worldMilliseconds;

        /// <summary>现场侧栏或管理页占用输入时，世界输入必须让位。</summary>
        public bool BlocksWorldInput => _managementOpen || _openFieldPage >= 0;

        public InitialRegion Region => _region;
        public long WorldMilliseconds => _worldMilliseconds;
        public int OpenFieldPage => _openFieldPage;

        /// <summary>机器域当前数据状态；null 表示读模型尚未建立。测试与调试用。</summary>
        public UiDataState? MachineDataState => _machineReadModel?.Snapshot.State;

        /// <summary>区域域当前数据状态；null 表示读模型尚未建立。测试与调试用。</summary>
        public UiDataState? RegionDataState => _regionReadModel?.Snapshot.State;

        /// <summary>区域里的对象数量；读模型未建立时为 0。测试与调试用。</summary>
        public int RegionObjectCount => _regionReadModel?.Snapshot.Count ?? 0;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            if (_fieldCloseButton != null)
            {
                _fieldCloseButton.onClick.AddListener(CloseFieldPanel);
            }

            // 世界链入口（来源：00-页面关系与复用 的主要入口表：HUD → 中枢五页／各集中界面）。
            // 会话由导航服务从本页透传，目标界面不自己去解析服务。
            if (_hudHubButton != null) _hudHubButton.onClick.AddListener(() => OpenHub(BaseCommandHubForm.PageOverview));
            if (_hudTasksButton != null) _hudTasksButton.onClick.AddListener(() => OpenHub(BaseCommandHubForm.PageTasks));
            if (_hudMachinesButton != null) _hudMachinesButton.onClick.AddListener(() => AutoEraUiNavigator.Open(this, UIViews.MachineLibraryForm));
            if (_hudBuildButton != null) _hudBuildButton.onClick.AddListener(() => AutoEraUiNavigator.Open(this, UIViews.BuildCatalogForm));
            if (_hudShopButton != null) _hudShopButton.onClick.AddListener(() => AutoEraUiNavigator.Open(this, UIViews.ShopForm));
            if (_hudComponentsButton != null) _hudComponentsButton.onClick.AddListener(() => AutoEraUiNavigator.Open(this, UIViews.ComponentLibraryForm));
            if (_hudSystemButton != null) _hudSystemButton.onClick.AddListener(() => AutoEraUiNavigator.Open(this, UIViews.SystemMenuForm));

            // 记录阅读：机器诊断与四个资源观察页的记录入口共用同一个 RecordReaderForm 的机器历史页。
            // 事件域已经活着（世界会话创建时就建好日志），所以这些入口点开就有真实记录；
            // 能源历史页会陈述「事件分类里没有能源域」，这是诚实空态而不是错误。
            WireRecords(_machineOverviewDiagnosticButton);
            WireRecords(_machineDiagnosticsTaskRecordButton);
            WireRecords(_machineDiagnosticsRunRecordButton);
            WireRecords(_farmRecordButton);
            WireRecords(_forestRecordButton);
            WireRecords(_mineralRecordButton);
            WireRecords(_waterRecordButton);
            WireRecords(_warehouseBuildingRecordsButton);

            // 算法入口：算法域没有生产运行路径，编辑器与模板库会整页切 Disabled 并写明原因。
            // 仍然接线——「未就绪」是设计里可展示的正常状态，死按钮不是。
            if (_machineOverviewAlgorithmButton != null) _machineOverviewAlgorithmButton.onClick.AddListener(() => OpenAlgorithmEditor());
            if (_machineAlgorithmEditButton != null) _machineAlgorithmEditButton.onClick.AddListener(() => OpenAlgorithmEditor());
            if (_machineAlgorithmTemplateButton != null) _machineAlgorithmTemplateButton.onClick.AddListener(() => AutoEraUiNavigator.Open(this, UIViews.AlgorithmLibraryForm));

            // 知识入口（00-页面关系与复用：作物知识入口放农田详情）。
            WireKnowledge(_farmKnowledgeButton);
            WireKnowledge(_forestKnowledgeButton);
            WireKnowledge(_mineralKnowledgeButton);
            WireKnowledge(_waterKnowledgeButton);

            // 状态栏与追踪栏的两个聚合入口。
            if (_hudAlertsOpenButton != null) _hudAlertsOpenButton.onClick.AddListener(() => AutoEraUiNavigator.Open(this, UIViews.AlertForm));
            if (_hudTrackerTaskButton != null) _hudTrackerTaskButton.onClick.AddListener(() => AutoEraUiNavigator.Open(this, UIViews.QuestForm));
        }

        private void WireRecords(Button button)
        {
            if (button != null)
            {
                button.onClick.AddListener(() => AutoEraUiNavigator.Open(this, UIViews.RecordReaderForm,
                    new AutoEraUiPageRequest(RecordReaderForm.PageMachineHistory)));
            }
        }

        private void WireKnowledge(Button button)
        {
            if (button != null)
            {
                button.onClick.AddListener(() => AutoEraUiNavigator.Open(this, UIViews.CropKnowledgeForm));
            }
        }

        private void OpenAlgorithmEditor() =>
            AutoEraUiNavigator.Open(this, UIViews.AlgorithmEditorForm);

        private void OpenHub(int page) =>
            AutoEraUiNavigator.Open(this, UIViews.BaseCommandHubForm, new AutoEraUiPageRequest(page));

        protected override void OnAutoEraOpen()
        {
            SetFieldPanel(-1);
            // 全屏 HUD 没有安全返回按钮，首焦点落在顶栏第一个入口（00-通用合同的首焦点顺序）。
            ApplyDefaultFocus(null, _firstInteractable);

            _machineReadModel = MachineReadModels.Create(TryGetSession(out AutoEraUiSession session) ? session : null);
            _machineReadModel.Changed += OnMachineSectionChanged;

            // 区域域：现场 12 个内容页（四个资源观察页与八个建筑页）的数据来源。
            // 区域已随会话到达（AutoEraWorldProcedure 在场景就绪后把它一起交出去）。
            _regionReadModel = RegionReadModels.Create(session);
            _regionReadModel.Changed += OnRegionSectionChanged;

            RenderMachinePages(_machineReadModel.Snapshot);
            RenderRegionPages(_regionReadModel.Snapshot);
        }

        protected override void OnAutoEraClose(bool isShutdown)
        {
            ReleaseMachines();
            ReleaseRegion();
            _region = null;
            _openFieldPage = -1;
        }

        protected override void OnAutoEraRecycle()
        {
            ReleaseMachines();
            ReleaseRegion();
            _region = null;
            _openFieldPage = -1;
            base.OnAutoEraRecycle();
        }

        private void ReleaseMachines()
        {
            if (_machineReadModel == null)
            {
                return;
            }

            _machineReadModel.Changed -= OnMachineSectionChanged;
            _machineReadModel.Dispose();
            _machineReadModel = null;
            _selectedMachineId = PersistentId.Invalid;
        }

        private void ReleaseRegion()
        {
            if (_regionReadModel == null)
            {
                return;
            }

            _regionReadModel.Changed -= OnRegionSectionChanged;
            _regionReadModel.Dispose();
            _regionReadModel = null;
        }

        private void OnRegionSectionChanged(RegionDomainSection section) =>
            RenderRegionPages(_regionReadModel.Snapshot);

        /// <summary>
        /// 世界区域接入点：InitialRegionScene.BindHud 调用。
        ///
        /// 正常情况下区域已经随会话到达（见 <see cref="AutoEraUiSession.Region"/>）；这条通道服务于
        /// 会话没带区域的场合（编辑器直接打开 HUD、测试装置）。此时若读模型仍是不可用态，
        /// 就用注入的区域把它换掉——否则界面会一直声称自己没有区域，而区域其实就在手上。
        /// </summary>
        public void BindRegion(InitialRegion region)
        {
            _region = region;
            if (region == null || _regionReadModel == null
                || _regionReadModel.Snapshot.State != UiDataState.Unavailable)
            {
                return;
            }

            _regionReadModel.Changed -= OnRegionSectionChanged;
            _regionReadModel.Dispose();
            _regionReadModel = RegionReadModels.CreateForRegion(region);
            _regionReadModel.Changed += OnRegionSectionChanged;
            RenderRegionPages(_regionReadModel.Snapshot);
        }

        /// <summary>现场可访问性与管理页覆盖：InitialRegionScene.ShowFieldAccess 调用。</summary>
        public void SetFieldAccess(bool accessible, bool managementOpen)
        {
            if (_fieldAccessible == accessible && _managementOpen == managementOpen)
            {
                return;
            }

            _fieldAccessible = accessible;
            _managementOpen = managementOpen;
            if (_managementOpen)
            {
                SetFieldPanel(-1);
            }

            RefreshFieldChrome();
        }

        /// <summary>
        /// 世界时间接入点：InitialRegionScene.Advance 每个世界秒调用一次。
        ///
        /// 规格 03 的顶部状态栏没有单列时间字段，这里把接入点映射到最接近的
        /// 「系统摘要」通道；正式显示格式应由显示模型提供，届时替换本方法内的排版。
        /// 用 StringBuilder 复用缓冲，避免在世界秒节拍上产生 GC 分配。
        ///
        /// 它也顺带承担「把区域选中的对象同步到机器域读模型」这件事：现场选中不产生
        /// 领域事件，而世界秒是本页已有的稳定节拍，不需要为同步再引入一条新机制。
        /// </summary>
        public void ShowWorldTime(long worldMilliseconds)
        {
            _worldMilliseconds = worldMilliseconds;
            SyncSelectedMachine();

            if (_statusSummary == null)
            {
                return;
            }

            _timeBuffer.Clear();
            _timeBuffer.Append("世界时间：").Append(worldMilliseconds / 1000L).Append('s');
            _statusSummary.SetText(_timeBuffer);
        }

        /// <summary>打开一个现场内容页；传入常驻 HUD 页或越界索引等同于关闭侧栏。</summary>
        public bool ShowFieldPage(int pageIndex) => SetFieldPanel(pageIndex);

        private static bool IsResidentHud(GameObject page)
            => page != null && page.name.StartsWith(ResidentHudPrefix, StringComparison.Ordinal);

        private bool SetFieldPanel(int pageIndex)
        {
            if (_pageRoots == null || _pageRoots.Length == 0)
            {
                return false;
            }

            bool withinRange = pageIndex >= 0 && pageIndex < _pageRoots.Length && !IsResidentHud(_pageRoots[pageIndex]);
            int target = withinRange ? pageIndex : -1;

            for (int i = 0; i < _pageRoots.Length; i++)
            {
                GameObject page = _pageRoots[i];
                if (page == null || IsResidentHud(page))
                {
                    continue;
                }

                page.SetActive(i == target);
            }

            _openFieldPage = target;
            RefreshFieldChrome();

            // 打开现场页时立即同步一次选中，避免等待下一个世界秒才刷新内容。
            if (target >= 0)
            {
                SyncSelectedMachine();
            }

            return true;
        }

        private void CloseFieldPanel() => SetFieldPanel(-1);

        private void RefreshFieldChrome()
        {
            if (_fieldCloseButton != null)
            {
                _fieldCloseButton.gameObject.SetActive(_openFieldPage >= 0);
            }
        }

        // -------------------------------------------------- 机器域

        private void SyncSelectedMachine()
        {
            if (_machineReadModel == null)
            {
                return;
            }

            PersistentId selected = _region != null && _region.SelectedId.IsValid ? _region.SelectedId : PersistentId.Invalid;
            if (selected.IsValid == _selectedMachineId.IsValid && selected == _selectedMachineId)
            {
                return;
            }

            _selectedMachineId = selected;
            if (selected.IsValid)
            {
                _machineReadModel.Select(selected);
            }
            else
            {
                _machineReadModel.ClearSelection();
            }
        }

        private void OnMachineSectionChanged(MachineDomainSection section) => RenderMachinePages(_machineReadModel.Snapshot);

        private void RenderMachinePages(MachineDomainSnapshot snapshot)
        {
            // 现场四页只有「概况／身份」一栏有真实数据（机器域详情）；其余栏位依赖尚未接入的
            // 硬件装配、算法实例与诊断域，因此陈述原因而不是留空或编造。
            RenderMachinePage(
                snapshot,
                _machineOverviewIdentityTemplate, _machineOverviewIdentityContent, _machineOverviewIdentityBody,
                _machineOverviewCapacityBody,
                _machineOverviewLoadingState, _machineOverviewEmptyState, _machineOverviewErrorState,
                _machineOverviewSuccessState, _machineOverviewDisabledState,
                "容量明细尚未接入：这里只显示机器身份与基本状态。");

            RenderMachinePage(
                snapshot,
                _machineHardwareSlotsTemplate, _machineHardwareSlotsContent, _machineHardwareSlotsBody,
                _machineHardwareComponentBody,
                _machineHardwareLoadingState, _machineHardwareEmptyState, _machineHardwareErrorState,
                _machineHardwareSuccessState, _machineHardwareDisabledState,
                "硬件装配与组件库尚未接入：槽位与候选组件暂无可显示内容。");

            RenderMachinePage(
                snapshot,
                _machineAlgorithmInstancesTemplate, _machineAlgorithmInstancesContent, _machineAlgorithmInstancesBody,
                _machineAlgorithmParametersBody,
                _machineAlgorithmLoadingState, _machineAlgorithmEmptyState, _machineAlgorithmErrorState,
                _machineAlgorithmSuccessState, _machineAlgorithmDisabledState,
                "算法域尚未接入：实例与公开参数暂无可显示内容。");

            RenderMachinePage(
                snapshot,
                _machineDiagnosticsTasksTemplate, _machineDiagnosticsTasksContent, _machineDiagnosticsTasksBody,
                _machineDiagnosticsComputeBody,
                _machineDiagnosticsLoadingState, _machineDiagnosticsEmptyState, _machineDiagnosticsErrorState,
                _machineDiagnosticsSuccessState, _machineDiagnosticsDisabledState,
                "执行队列与算力明细尚未接入：诊断栏暂无可显示内容。");
        }

        private void RenderMachinePage(
            MachineDomainSnapshot snapshot,
            GameObject primaryTemplate,
            RectTransform primaryContent,
            TMP_Text primaryBody,
            TMP_Text secondaryBody,
            GameObject loadingState,
            GameObject emptyState,
            GameObject errorState,
            GameObject successState,
            GameObject disabledState,
            string missingReason)
        {
            bool unavailable = snapshot.State == UiDataState.Unavailable;
            bool hasSelection = snapshot.HasSelection;

            SetState(loadingState, false);
            SetState(errorState, false);
            SetState(emptyState, !unavailable && !hasSelection);
            SetState(successState, !unavailable && hasSelection);
            SetState(disabledState, unavailable);

            RenderDetailRows(primaryTemplate, primaryContent, hasSelection ? snapshot.Detail : NoFields);

            if (primaryBody != null)
            {
                primaryBody.SetText(hasSelection
                    ? string.Empty
                    : unavailable
                        ? snapshot.UnavailableReason ?? "机器数据不可用"
                        : "未选择对象：在区域中选择一台机器后这里会显示它的详情。");
            }

            if (secondaryBody != null)
            {
                secondaryBody.SetText(missingReason);
            }
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
