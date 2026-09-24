using System.Collections.Generic;
using AutoEra.Machines;
using AutoEra.UI.Contracts;
using AutoEra.World.Identity;
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
    /// 需要机器改装、库存与经济域的操作（升级／出售／改名）一律**禁用并写明原因**；
    /// 当前真正可用的动作是：部署（交给世界放置的机器部署页）、整备、两页之间的切换、
    /// 在中枢查看，以及在整备页**选中一格后安装或拆卸**——
    /// 已占用的格子直接进 17-硬件修改确认，空格子先进 12-组件选择器再进 17。
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

        private const string RefitMissing =
            "机器改装、库存与经济系统尚未接入：升级／出售／改名暂不可用"
            + "（部署、整备、拆卸与装入已接入整备页）。";

        /// <summary>整备页尚未接入的动作各自指向**真实的下一步**，而不是一句「未接入」。</summary>
        private const string AssemblyMissing =
            "本栏只读：升级走 11-载体升级，改名走 17-重命名，出售走 17-交易确认——"
            + "这三个对话框尚未接线，因此对应的写动作不可用（安装／拆卸见组件整备栏）。";

        /// <summary>导航按钮 → 规格页索引（来源：00-共享外壳-prefab-layout.md 的导航表）。</summary>
        private static readonly int[] NavigationPageIndex = { PageUndeployed, PageDeployed };

        private IMachineReadModel _machines;

        // 整备页的槽位选择：安装／拆卸都必须先指到**具体一格**，而槽位属于某一台机器的
        // 某一类硬件的第几个位置，所以三样一起记。选中态用「它属于哪台机器」自我校验——
        // 换机器、机器被移除时自动作废，不需要在每一条切换路径上补清理代码。
        private HardwareKind _selectedSlotKind;
        private int _selectedSlotIndex;
        private PersistentId _slotSelectionMachine = PersistentId.Invalid;

        /// <summary>整备页是否选中了当前这台机器的槽位。测试与调试用。</summary>
        public bool HasSlotSelection =>
            _slotSelectionMachine.IsValid
            && _machines != null
            && _machines.SelectedId == _slotSelectionMachine;

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

            // 「部署」是第二个真正走得通的动作：把选中机器的**稳定身份**交给世界放置的机器部署页。
            // 不带名字、也不在那边按名字回退查找——规格明确要求传稳定 ID，失效就禁用并说明。
            if (_undeployedDeployButton != null) _undeployedDeployButton.onClick.AddListener(OpenDeploymentForSelection);
            if (_preparationDeployButton != null) _preparationDeployButton.onClick.AddListener(OpenDeploymentForSelection);

            // 「整备」把选中机器带到整备页（规格 05-机器整备 的入口就是 09-未部署机器→整备）。
            if (_undeployedPrepareButton != null) _undeployedPrepareButton.onClick.AddListener(OpenPreparationForSelection);

            // 「安装或拆卸」在整备页：**选中任意槽位**后即可用——已占用的格子走拆卸确认，
            // 空格子走 12-组件选择器（它再进 17-硬件确认）。两条路都不在这里改硬件。
            if (_preparationInstallButton != null) _preparationInstallButton.onClick.AddListener(OpenSlotAction);

            // 「一键卸下」：把整台机器上装着的组件原子回库（规格 05-机器整备）。
            // 它与单槽拆卸走**同一个**确认页与同一个执行器，区别只是这次的意图是「整台清空」。
            if (_preparationUnloadButton != null) _preparationUnloadButton.onClick.AddListener(OpenUnloadAll);
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

            // 花名册变化同样会改到**选中机器自己的数据**（装机、撤收、状态变化），
            // 所以详情与整备页在两个区域事件上都要重画：只刷列表会留下一份过期的详情。
            RenderDetail(_machines.Snapshot);
            RenderPreparation(_machines.Snapshot);
            // 选中项变了，部署按钮的可点性也要跟着变（否则会出现「选了一台已部署的机器，
            // 部署按钮看起来还能点」这种界面在撒谎的情况）。
            ApplyUnavailableActions();
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

        /// <summary>
        /// 整备页三栏全部来自**同一份机器快照**（`Carrier` / `Assembly` / `Readiness`），
        /// 而不是各自去问花名册——数据来源唯一是这一域的原则，也让三栏天然同步。
        ///
        /// 只有「部署解锁条件」这一行陈述缺口：成长解锁域还没有创建者。
        /// 其余各行都是真实数据（逐槽位占用、容量、算力、出售资格）。
        /// </summary>
        private void RenderPreparation(MachineDomainSnapshot snapshot)
        {
            bool hasSelection = snapshot.HasSelection;
            SetState(_machinePreparationLoadingState, false);
            SetState(_machinePreparationErrorState, false);
            SetState(_machinePreparationEmptyState, !hasSelection);
            SetState(_machinePreparationSuccessState, hasSelection);
            SetState(_machinePreparationDisabledState, snapshot.State == UiDataState.Unavailable);

            SetBody(_machinePreparationCarrierBody, snapshot, AssemblyMissing);
            SetBody(_machinePreparationAssemblyBody, snapshot, AssemblyHint());
            SetBody(_machinePreparationReadinessBody, snapshot, AssemblyMissing);

            RenderDetailRows(_machinePreparationCarrierTemplate, _machinePreparationCarrierContent,
                hasSelection ? snapshot.Carrier : NoFields);
            RenderDetailRows(_machinePreparationReadinessTemplate, _machinePreparationReadinessContent,
                hasSelection ? snapshot.Readiness : NoFields);

            // 组件整备栏的**槽位行可选中**：安装／拆卸都先要知道哪一格，而只显示
            // 「已装 2 件」回答不了这个问题。行的点击回调挂在槽位行上；摘要行传 null 回调
            // （纯展示行，其按钮会被禁用，不会抢焦点）。
            int slotCount = hasSelection ? SlotRowCount() : 0;
            RenderListRows(_machinePreparationAssemblyTemplate, _machinePreparationAssemblyContent,
                hasSelection ? snapshot.Assembly.Count : 0,
                (index, item) =>
                {
                    if (index >= slotCount)
                    {
                        item.Bind(index, snapshot.Assembly[index].Label, snapshot.Assembly[index].Value, null);
                        return;
                    }

                    item.Bind(index, snapshot.Assembly[index].Label, snapshot.Assembly[index].Value, OnSlotRowClicked);
                });

            RefreshSlotActions();
        }

        /// <summary>整备页正文：说清「怎么操作」以及两条路各自通向哪里。</summary>
        private string AssemblyHint() => HasSlotSelection
            ? "已选槽位：" + AutoEraUiFormat.Slot(_selectedSlotKind, _selectedSlotIndex)
              + "。选中后「安装或拆卸」会按格子分流：已占用的格子去 17-硬件修改确认（拆下），"
              + "空格子去 12-组件选择器（挑一件后同样进 17 确认）。"
              + "「一键卸下」作用于整台机器，不走槽位选择。"
            : "选中一行槽位后再决定装或拆；安装／拆卸都要经 17-硬件修改确认。"
              + "「一键卸下」不需要先选槽位，它把整台机器上装着的组件一起原子回库。"
              + "升级走 11-载体升级、改名走 17-重命名、出售走 17-交易确认——这些尚未接线。";

        /// <summary>
        /// 整备页三个槽位动作按钮的可点性。
        ///
        /// 注意「安装或拆卸」的判据是 **`HasSlotSelection`（选中了某一格）而不是「选中了某台机器」**：
        /// 只选了机器还没有目标格子，安装／拆卸无处可去。
        ///
        /// 「一键卸下」的判据不同：它作用于**整台机器**，所以不看槽位选择，而是看
        /// 「选中了机器 ＋ 这台机器真的装着东西」。一件都没装时可点等于让玩家按下一个
        /// 只会被拒绝的按钮——界面不该给出这种入口。已经部署的机器也不在这里（整备环境只针对库中机器，
        /// 已部署机器的硬件修改必须现场完成）。
        /// </summary>
        private void RefreshSlotActions()
        {
            SetInteractable(_preparationInstallButton, HasSlotSelection);
            SetInteractable(_preparationUnloadButton, CanUnloadAll);
        }

        /// <summary>选中的机器现在是否可以一键卸下（测试与调试用）。</summary>
        public bool CanUnloadAll
        {
            get
            {
                if (_machines == null || !TryGetSession(out AutoEraUiSession session) || !session.HasWorld)
                {
                    return false;
                }

                MachineInstance machine = SelectedMachine();
                return machine != null && !machine.Deployed && machine.InstalledComponentCount > 0;
            }
        }

        /// <summary>
        /// 一键卸下：把**整台机器**交给 17-硬件修改确认页（规格 05-机器整备
        /// 「确认全部卸下影响并原子回库」）。
        ///
        /// 这里既不判断来源也不执行修改——「什么时候能改硬件」由领域门禁回答，
        /// 界面只负责说清「哪台机器、要做什么」。
        /// </summary>
        public void OpenUnloadAll()
        {
            MachineInstance machine = SelectedMachine();
            if (machine == null)
            {
                return;
            }

            AutoEraUiNavigator.Open(this, UIViews.OperationDialogForm,
                AutoEraHardwareRequest.UnloadAll(machine.Id));
        }

        /// <summary>装配栏里槽位行的数量（其余是摘要行）。槽位行按类别顺序排列。</summary>
        private int SlotRowCount()
        {
            MachineInstance machine = SelectedMachine();
            MachineDefinition definition = machine?.Definition;
            if (definition == null)
            {
                return 0;
            }

            return definition.SensorSlots + definition.CoreSlots + definition.EffectorSlots;
        }

        private void OnSlotRowClicked(int index)
        {
            MachineDefinition definition = SelectedMachine()?.Definition;
            if (definition == null)
            {
                return;
            }

            int remaining = index;
            if (remaining < definition.SensorSlots)
            {
                _selectedSlotKind = HardwareKind.Sensor;
                _selectedSlotIndex = remaining;
            }
            else if ((remaining -= definition.SensorSlots) < definition.CoreSlots)
            {
                _selectedSlotKind = HardwareKind.Core;
                _selectedSlotIndex = remaining;
            }
            else
            {
                _selectedSlotKind = HardwareKind.Effector;
                _selectedSlotIndex = remaining - definition.CoreSlots;
            }

            _slotSelectionMachine = _machines.SelectedId;
            RenderPreparation(_machines.Snapshot);
        }

        /// <summary>当前选中的机器实例；未选中或已不在花名册时返回 null。测试与调试用。</summary>
        public MachineInstance SelectedMachine()
        {
            if (_machines == null)
            {
                return null;
            }

            int index = SelectedIndex;
            if (index < 0)
            {
                return null;
            }

            // 槽位内容（装了哪件、算力占用）只有领域实例答得出来，快照只带展示文本；
            // 所以这里取的是花名册里的**真身**，而不是又维护一份副本。
            if (!TryGetSession(out AutoEraUiSession session) || !session.HasWorld)
            {
                return null;
            }

            return session.World.Machines.TryGet(_machines.Snapshot.Machines[index].Id, out MachineInstance machine)
                ? machine
                : null;
        }

        /// <summary>选中的整备槽位；没有选中时为 null。测试与调试用。</summary>
        public string SelectedSlotLabel => HasSlotSelection
            ? AutoEraUiFormat.Slot(_selectedSlotKind, _selectedSlotIndex)
            : null;

        /// <summary>选中槽位里已装的组件；没有选中或槽位为空时为 null。测试与调试用。</summary>
        public ComponentInstance SelectedSlotComponent()
        {
            MachineInstance machine = SelectedMachine();
            MachineDefinition definition = machine?.Definition;
            if (definition == null || !HasSlotSelection || _selectedSlotIndex >= definition.SlotCount(_selectedSlotKind))
            {
                return null;
            }

            return machine.GetComponent(_selectedSlotKind, _selectedSlotIndex);
        }

        /// <summary>
        /// 选中槽位后的动作分派：**已占用的格子去拆卸确认、空格子去选组件**。
        ///
        /// 两条路都不在这里改硬件：拆卸与装入都要经 17-硬件修改确认（执行由
        /// `MachineHardwareOperation` 负责，来源门禁与安全停机都在那里）。
        /// 界面只把「哪台机器、哪一格、要装还是拆、装的是谁」作为请求交出去。
        /// </summary>
        public void OpenSlotAction()
        {
            MachineInstance machine = SelectedMachine();
            if (machine == null || !HasSlotSelection)
            {
                return;
            }

            if (SelectedSlotComponent() != null)
            {
                AutoEraUiNavigator.Open(this, UIViews.OperationDialogForm,
                    new AutoEraHardwareRequest(machine.Id, _selectedSlotKind, _selectedSlotIndex,
                        PersistentId.Invalid, remove: true));
                return;
            }

            // 空格子：先去 12-组件选择器挑一件。选择器只返回候选身份，不装任何东西；
            // 它随后自己打开 17-硬件确认（规格：装配模式返回候选ID并进入17硬件确认）。
            AutoEraUiNavigator.Open(this, UIViews.ComponentPickerForm,
                new AutoEraComponentPickRequest(machine.Id, _selectedSlotKind, _selectedSlotIndex));
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
            // 改装、库存与经济域仍未接入：这些按钮继续禁用，原因写在整备页与列表页的说明文本里。
            // 「部署」与「整备」已从这份名单里移除——它们现在都有真实去向；
            // 「安装或拆卸」也不在这里，它的可点性由 RefreshSlotActions 按槽位选中情况决定。
            Disable(_preparationRenameButton,
                _preparationUpgradeButton, _preparationSellButton,
                _undeployedRenameButton, _undeployedSellButton,
                _deployedRenameButton, _deployedLocateButton);

            // 未选中任何机器时这些动作无处可去，明确禁用而不是让按钮点了没反应。
            bool deployable = CanDeploySelection;
            SetInteractable(_undeployedDeployButton, deployable);
            SetInteractable(_preparationDeployButton, deployable);

            // 整备对「库中的机器」成立（规格：未部署机器的整备环境）。已部署的机器不在整备范围。
            bool preparable = CanPrepareSelection;
            SetInteractable(_undeployedPrepareButton, preparable);
        }

        /// <summary>选中的机器是否还没部署（整备只对库中机器成立）。</summary>
        private bool CanPrepareSelection
        {
            get
            {
                if (_machines == null)
                {
                    return false;
                }

                int index = SelectedIndex;
                return index >= 0 && !_machines.Snapshot.Machines[index].Deployed;
            }
        }

        /// <summary>把选中机器带到整备页；整备环境里没有可写动作之前它仍是只读的（见类的说明）。</summary>
        private void OpenPreparationForSelection()
        {
            if (CanPrepareSelection)
            {
                ShowLibraryPage(PagePreparation);
            }
        }

        private static void SetInteractable(Button button, bool value)
        {
            if (button != null)
            {
                button.interactable = value;
            }
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

        /// <summary>选中的机器是否还能部署（在库里、且尚未部署）。</summary>
        private bool CanDeploySelection
        {
            get
            {
                if (_machines == null)
                {
                    return false;
                }

                int index = SelectedIndex;
                if (index < 0)
                {
                    return false;
                }

                MachineDomainSnapshot snapshot = _machines.Snapshot;
                return !snapshot.Machines[index].Deployed;
            }
        }

        /// <summary>
        /// 打开世界放置的机器部署页，并把选中机器的**稳定身份**带过去。
        ///
        /// 目标页只认这个身份：拿不到机器时它禁用写操作并说明原因，不会回到列表里按名字猜一个。
        /// 这也是「已部署的机器不能重复落位」在界面侧的入口抑制——领域侧还会再拒一次。
        /// </summary>
        private void OpenDeploymentForSelection()
        {
            if (!CanDeploySelection)
            {
                return;
            }

            AutoEraUiNavigator.Open(this, UIViews.WorldPlacementForm,
                new AutoEraUiPageRequest(WorldPlacementForm.PageMachineDeployment, _machines.SelectedId));
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
