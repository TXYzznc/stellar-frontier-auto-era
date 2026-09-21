using System.Collections.Generic;
using AutoEra.Machines;
using AutoEra.UI.Contracts;
using AutoEra.World.Identity;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 操作确认与输入（规格 17：重命名、普通确认、交易确认、升级确认、建造与部署确认、
    /// 施工取消、**硬件修改确认**、机器回收、算法应用、强确认、草稿离开、未保存强退、显示模式保留恢复）。
    ///
    /// 这是一个**参数驱动**的多页对话框：它不自己决定显示哪一页，而是由调用方经打开参数
    /// 说明「要确认什么」。本批只接入**硬件修改确认**一页——它是整备环境安装／拆卸的必经关卡
    /// （规格 HardwareConfirm 的入口就是「组件安装、替换、拆卸、一键卸下」）。
    ///
    /// 其余页在没有请求时呈现 Disabled 并说明「本界面由调用方带参数打开」——
    /// 它们是各自域的确认页，随各自的域接入，不在这里编造内容。
    ///
    /// **关键分工**：本页只收集确认，不执行任何修改。真正的执行交给
    /// `MachineHardwareOperation`（它掌管来源门禁、等待安全停机、失败判定），
    /// 界面因此不可能绕过确认改硬件，也不可能自己实现一套「什么时候能改」的判断。
    /// 提交后的状态与原因都从那个 operation 读回来，不是界面自己猜的。
    /// </summary>
    public sealed partial class OperationDialogForm : AutoEraShellFormBase
    {
        /// <summary>规格页序（`Grp_PageHost` 下内容页的顺序）。</summary>
        public const int PageRename = 0;
        public const int PageBasicConfirm = 1;
        public const int PageTransactionConfirm = 2;
        public const int PageUpgradeConfirm = 3;
        public const int PagePlacementConfirm = 4;
        public const int PageCancelProduction = 5;
        public const int PageHardwareConfirm = 6;
        public const int PageMachineRecovery = 7;
        public const int PageAlgorithmApply = 8;
        public const int PageStrongConfirm = 9;
        public const int PageDraftExit = 10;
        public const int PageForceExit = 11;
        public const int PageDisplayKeep = 12;

        private const string NoRequestReason =
            "本界面由调用方带参数打开：它承载重命名、各类确认与保留恢复等多个对话框页面，"
            + "每一页都由发起方说明「要确认什么」。当前没有请求，因此没有可显示的内容。";

        private static readonly UiDetailField[] NoFields = new UiDetailField[0];

        private readonly List<UiDetailField> _change = new List<UiDetailField>(8);
        private readonly List<UiDetailField> _effects = new List<UiDetailField>(8);

        private AutoEraHardwareRequest _hardware;
        private MachineHardwareOperation _operation;
        private MachineInstance _machine;

        /// <summary>组件目录，只用于把型号显示成名字；拿不到就退化成型号编号，**不编造名字**。</summary>
        private MachineCatalog _catalog;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            if (_backButton != null) _backButton.onClick.AddListener(RequestCancel);
            if (_closeButton != null) _closeButton.onClick.AddListener(RequestCancel);
            if (_hardwareConfirmKeepButton != null) _hardwareConfirmKeepButton.onClick.AddListener(RequestCancel);
            if (_hardwareConfirmCommitButton != null) _hardwareConfirmCommitButton.onClick.AddListener(CommitHardware);
        }

        protected override void OnAutoEraOpen()
        {
            AutoEraHardwareRequest request = null;
            TryGetRequest(out request);

            if (request == null)
            {
                ShowPage(_pageRoots, PageRename);
                ApplyDefaultFocus(_backButton != null ? _backButton.gameObject : null, null);
                ShowPageUnavailable(NoRequestReason,
                    _hardwareConfirmLoadingState, _hardwareConfirmEmptyState, _hardwareConfirmErrorState,
                    _hardwareConfirmSuccessState, _hardwareConfirmDisabledState,
                    _hardwareConfirmChangeBody, _hardwareConfirmEffectsBody);
                return;
            }

            _hardware = request;
            ShowPage(_pageRoots, PageHardwareConfirm);
            ApplyDefaultFocus(
                _hardwareConfirmKeepButton != null ? _hardwareConfirmKeepButton.gameObject : null,
                _hardwareConfirmCommitButton != null ? _hardwareConfirmCommitButton.gameObject : null);
            PrepareHardware();
        }

        protected override void OnAutoEraClose(bool isShutdown) => ReleaseOperation();

        protected override void OnAutoEraRecycle()
        {
            ReleaseOperation();
            base.OnAutoEraRecycle();
        }

        /// <summary>
        /// 只退订与清引用：**不取消已提交的修改**——规格明确「未成功前组件归属不变」
        /// 且「返回页面不自动取消等待」，所以关掉对话框不该撤销一个已经在等安全停机的意图。
        /// </summary>
        private void ReleaseOperation()
        {
            if (_operation != null)
            {
                _operation.Changed -= OnOperationChanged;
                _operation = null;
            }

            _hardware = null;
            _machine = null;
            _catalog = null;
        }

        /// <summary>硬件修改确认页当前是否有请求。测试与调试用。</summary>
        public bool HasHardwareRequest => _hardware != null;

        /// <summary>当前请求是不是「一键卸下全部」。测试与调试用。</summary>
        public bool HardwareRequestIsUnloadAll => _hardware != null && _hardware.RemoveAll;

        /// <summary>最近一次提交的领域结果；未提交时为 null。测试与调试用。</summary>
        public MachineManagementResult? HardwareResult => _operation != null ? _operation.Result : (MachineManagementResult?)null;

        /// <summary>最近一次提交的操作状态；未提交时为 null。测试与调试用。</summary>
        public HardwareOperationState? HardwareState => _operation?.State;

        /// <summary>本次请求对应的机器；没有请求时为 null。测试与调试用。</summary>
        public MachineInstance HardwareMachine => _machine;

        private void PrepareHardware()
        {
            _machine = null;
            if (!TryGetSession(out AutoEraUiSession session) || !session.HasWorld)
            {
                RenderHardwareUnavailable("没有世界会话：无法核对机器与槽位，因此不允许提交硬件修改。");
                return;
            }

            if (!session.World.Machines.TryGet(_hardware.MachineId, out MachineInstance machine))
            {
                RenderHardwareUnavailable("请求里的机器不在花名册里：它可能已经被移除。");
                return;
            }

            _machine = machine;
            ResolveCatalog();
            _operation = session.World.Machines.GetHardwareOperation(machine.Id);
            _operation.Changed += OnOperationChanged;
            BuildHardwareRows();
            RenderHardware();
        }

        /// <summary>
        /// 解析组件目录。解析失败只降级显示（退化成型号编号），**不影响任何写入判定**——
        /// 目录是显示层的辅助，不是「这次能不能改硬件」的依据。
        /// </summary>
        private void ResolveCatalog()
        {
            try
            {
                _catalog = MachineCatalog.FromLoadedGameData();
            }
            catch (System.InvalidOperationException)
            {
                _catalog = null;
            }
            catch (System.NullReferenceException)
            {
                _catalog = null;
            }
        }

        private void OnOperationChanged(MachineHardwareOperation operation) => RenderHardware();

        private void RenderHardwareUnavailable(string reason)
        {
            _change.Clear();
            _effects.Clear();
            ShowPageUnavailable(reason,
                _hardwareConfirmLoadingState, _hardwareConfirmEmptyState, _hardwareConfirmErrorState,
                _hardwareConfirmSuccessState, _hardwareConfirmDisabledState,
                _hardwareConfirmChangeBody, _hardwareConfirmEffectsBody);
            RenderDetailRows(_hardwareConfirmChangeTemplate, _hardwareConfirmChangeContent, NoFields);
            RenderDetailRows(_hardwareConfirmEffectsTemplate, _hardwareConfirmEffectsContent, NoFields);
            SetInteractable(_hardwareConfirmCommitButton, false);
        }

        private void BuildHardwareRows()
        {
            _change.Clear();
            _effects.Clear();

            MachineDefinition definition = _machine.Definition;
            _change.Add(new UiDetailField("机器", _machine.Name));

            if (_hardware.RemoveAll)
            {
                BuildUnloadAllRows(definition);
            }
            else
            {
                _change.Add(new UiDetailField("槽位", AutoEraUiFormat.Slot(_hardware.Kind, _hardware.SlotIndex)));

                ComponentInstance occupant = SlotOccupant();
                _change.Add(new UiDetailField("卸下", _hardware.Remove
                    ? Describe(occupant)
                    : "无（本次不拆下任何组件）"));
                _change.Add(new UiDetailField("装入", _hardware.Remove
                    ? "无（本次只拆下）"
                    : DescribeComponent(_hardware.ComponentId)));
                _change.Add(new UiDetailField("库存去向", _hardware.Remove
                    ? "拆下的组件回到组件库，成为可再安装的散件。"
                    : "装入的组件离开组件库（散件）；它不能再同时装到其它机器上。"));
            }

            // 运行影响（规格：当前行为；等待安全停机；能力和绑定失效；保持激活；不自动重绑）
            _effects.Add(new UiDetailField("当前行为", _machine.HasActiveBehavior
                ? "有正在执行的行为：提交后先等它到安全点，不会被打断。"
                : "没有正在执行的行为，提交后立即生效。"));
            _effects.Add(new UiDetailField("能力变化", CapacityChange()));
            _effects.Add(new UiDetailField("保持激活", _machine.Activated
                ? "机器保持激活；本次改动不会替玩家重新开机。"
                : "机器当前未激活。"));
            _effects.Add(new UiDetailField("算法绑定", "不自动重新绑定：算法里指向该槽位的端点会失效，需要重新应用算法。"));
            _effects.Add(new UiDetailField("提交状态", MachineStatusText()));
        }

        /// <summary>
        /// 一键卸下的变更清单：**逐槽位列出将要卸下的每一件**。
        ///
        /// 只写「会卸下 3 件」是不够的——玩家在按下去之前要能核对「哪三件、装在哪」，
        /// 而这正是规格把「槽位」和「卸下实例」都列进变更清单的原因。
        /// 空槽位不列（它们本次没有任何变化）。
        /// </summary>
        private void BuildUnloadAllRows(MachineDefinition definition)
        {
            int listed = 0;
            if (definition != null)
            {
                HardwareKind[] kinds = { HardwareKind.Sensor, HardwareKind.Core, HardwareKind.Effector };
                for (int k = 0; k < kinds.Length; k++)
                {
                    HardwareKind kind = kinds[k];
                    int slots = definition.SlotCount(kind);
                    for (int index = 0; index < slots; index++)
                    {
                        ComponentInstance component = _machine.GetComponent(kind, index);
                        if (component == null)
                        {
                            continue;
                        }

                        _change.Add(new UiDetailField(AutoEraUiFormat.Slot(kind, index), Describe(component)));
                        listed++;
                    }
                }
            }

            if (listed == 0)
            {
                _change.Add(new UiDetailField("槽位", "这台机器上一个组件都没有装。"));
            }

            _change.Add(new UiDetailField("卸下", listed == 0
                ? "无"
                : "以上 " + listed + " 件全部卸下（原子操作：有一件卸不下就一件都不卸）。"));
            _change.Add(new UiDetailField("库存去向", listed == 0
                ? "没有组件需要回到组件库。"
                : "卸下的组件全部回到组件库，成为可再安装的散件；载体与组件不会被一起卖掉。"));
        }

        private string MachineStatusText()
        {
            if (_operation == null || _operation.State == HardwareOperationState.Idle)
            {
                return "尚未提交。";
            }

            return _operation.State switch
            {
                HardwareOperationState.Waiting => "已提交，等待安全停机：" + AutoEraUiFormat.ManagementResult(_operation.Result),
                HardwareOperationState.Completed => "已提交并完成。",
                HardwareOperationState.Rejected => "被拒绝：" + AutoEraUiFormat.ManagementResult(_operation.Result),
                HardwareOperationState.Cancelled => "已取消，未做任何修改。",
                _ => "尚未提交。",
            };
        }

        private string CapacityChange()
        {
            MachineDefinition machineDefinition = _machine.Definition;
            if (_hardware.RemoveAll)
            {
                // 一键卸下：全部卸完之后只剩载体自身的基础容量，算力与逻辑占用归零。
                if (machineDefinition == null)
                {
                    return "本次不改变容量、算力或逻辑容量。";
                }

                return "卸下后：容量 " + machineDefinition.BaseCapacity
                    + "（只剩载体基础容量）、算力 0、逻辑 0；当前 容量 " + _machine.TotalCapacity
                    + "、算力 " + _machine.ComputeCapacity + "、逻辑 " + _machine.LogicCapacity + "。";
            }

            ComponentInstance occupant = SlotOccupant();
            if (occupant == null || occupant.Definition == null || machineDefinition == null)
            {
                return "本次不改变容量、算力或逻辑容量。";
            }

            string sign = _hardware.Remove ? "减少" : "增加";
            ComponentDefinition component = occupant.Definition;
            return "容量 " + sign + " " + component.AddedCapacity
                + " ／ 算力 " + sign + " " + component.ComputeCapacity
                + " ／ 逻辑 " + sign + " " + component.LogicCapacity
                + "；当前 容量 " + _machine.TotalCapacity + "、算力 " + _machine.ComputeCapacity
                + "、逻辑 " + _machine.LogicCapacity + "。";
        }

        private ComponentInstance SlotOccupant()
        {
            if (_machine == null || _hardware == null)
            {
                return null;
            }

            MachineDefinition definition = _machine.Definition;
            if (definition == null || _hardware.SlotIndex < 0 || _hardware.SlotIndex >= definition.SlotCount(_hardware.Kind))
            {
                return null;
            }

            return _machine.GetComponent(_hardware.Kind, _hardware.SlotIndex);
        }

        private string Describe(ComponentInstance component)
        {
            if (component == null)
            {
                return "槽位是空的（没有可拆下的组件）";
            }

            // 型号名 + 实例身份：同一个型号可能有好几件，只说名字分不清拆的是哪一件。
            return ComponentName(component.Definition) + "（实例 " + component.Id.Value + "）";
        }

        /// <summary>
        /// 组件型号的显示名。目录拿不到名字就退化成型号编号——整备页对同一件组件用的是同一条规则，
        /// 两处必须说同一句话，否则玩家会以为看到的是两件不同的东西。
        /// </summary>
        private string ComponentName(ComponentDefinition definition)
        {
            if (definition == null)
            {
                return "未知型号";
            }

            if (_catalog != null && _catalog.TryGetComponentRow(definition, out ComponentDisplayRow row) && row != null)
            {
                return row.Name;
            }

            return "型号 " + definition.Id;
        }

        /// <summary>装入的组件用稳定身份描述；名字要靠目录，拿不到就只说身份，不编造名字。</summary>
        private string DescribeComponent(PersistentId componentId)
        {
            if (!componentId.IsValid)
            {
                return "无";
            }

            if (!TryGetSession(out AutoEraUiSession session) || !session.HasWorld)
            {
                return "实例 " + componentId.Value;
            }

            foreach (ComponentInstance component in session.World.Machines.Components)
            {
                if (component.Id == componentId)
                {
                    return ComponentName(component.Definition) + "（实例 " + component.Id.Value + "）";
                }
            }

            return "实例 " + componentId.Value + "（不在花名册里——提交会被领域拒绝）";
        }

        private void RenderHardware()
        {
            if (_machine == null || _operation == null)
            {
                return;
            }

            bool rejected = _operation.State == HardwareOperationState.Rejected;
            bool completed = _operation.State == HardwareOperationState.Completed;
            bool waiting = _operation.State == HardwareOperationState.Waiting;

            SetState(_hardwareConfirmLoadingState, false);
            SetState(_hardwareConfirmEmptyState, false);
            SetState(_hardwareConfirmErrorState, rejected);
            SetState(_hardwareConfirmSuccessState, completed);
            SetState(_hardwareConfirmDisabledState, false);

            // 提交状态可能变（等待 → 完成），所以每次事件都重建一次文本。
            if (_effects.Count > 0)
            {
                _effects[_effects.Count - 1] = new UiDetailField("提交状态", MachineStatusText());
            }

            SetText(_hardwareConfirmChangeBody, _hardware.Intent);
            SetText(_hardwareConfirmEffectsBody, completed
                ? "硬件修改已完成。"
                : rejected
                    ? "硬件修改被拒绝：" + AutoEraUiFormat.ManagementResult(_operation.Result)
                    : waiting ? "已提交，等待安全停机。" : "提交后不会自动重新绑定算法。");

            RenderDetailRows(_hardwareConfirmChangeTemplate, _hardwareConfirmChangeContent, _change);
            RenderDetailRows(_hardwareConfirmEffectsTemplate, _hardwareConfirmEffectsContent, _effects);

            // 已提交的意图不再接受第二次提交；「保留配置」（不提交并关闭）始终可用。
            SetInteractable(_hardwareConfirmCommitButton,
                !waiting && !completed && !rejected && _operation.State == HardwareOperationState.Idle);
            SetInteractable(_hardwareConfirmKeepButton, true);
        }

        /// <summary>
        /// 提交硬件修改。来源**按机器当前状态推导**（未部署 → 整备环境 Library；已部署 → 现场 Field）——
        /// 那正是领域门禁的判据；让界面传一个可能与机器状态矛盾的来源只会制造假分歧。
        /// </summary>
        public void CommitHardware()
        {
            if (_machine == null || _operation == null)
            {
                return;
            }

            ManagementOrigin origin = _machine.Deployed ? ManagementOrigin.Field : ManagementOrigin.Library;
            bool accepted;
            if (_hardware.RemoveAll)
            {
                accepted = _operation.BeginRemoveAll(_hardware.MachineId, origin);
            }
            else
            {
                PersistentId componentId = _hardware.Remove ? PersistentId.Invalid : _hardware.ComponentId;
                accepted = _operation.Begin(_hardware.MachineId, origin, _hardware.Remove,
                    _hardware.Kind, _hardware.SlotIndex, componentId);
            }

            if (!accepted)
            {
                // Begin 只在「上一次仍在等待」时返回 false；这是一种真实状态，必须说明而不是静默。
                SetText(_hardwareConfirmEffectsBody, "上一次硬件修改仍在等待安全停机，本次没有提交。");
                return;
            }

            RenderHardware();
        }

        private static void SetInteractable(Button button, bool value)
        {
            if (button != null)
            {
                button.interactable = value;
            }
        }

        private static void SetText(TMPro.TMP_Text text, string value)
        {
            if (text != null)
            {
                text.SetText(value ?? string.Empty);
            }
        }

        private void RequestCancel() => TryHandleIntent(AutoEraUiIntent.Cancel);

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
