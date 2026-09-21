using System.Collections.Generic;
using AutoEra.UI.Contracts;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 世界放置（规格 14-世界放置与定位：建造放置、机器部署、世界绑定三页）。
    ///
    /// 绑定字段在同名的 .Fields.cs 里，结构由契约生成。
    ///
    /// 三页都在描述「把一个东西放到世界里的某个位置」：选目标、看校验、看提示、确认。
    /// 其中**机器部署**已经有真正的运行路径（`MachineDeploymentFlow` + 区域导航 +
    /// 机器实体生成），所以本页真的能用：选中的机器来自打开参数里的稳定身份，
    /// 落位预览由现场的 `RegionInputModule` 驱动，确认走流程的 `TryCommit`。
    ///
    /// 建造放置与世界绑定两页仍然整页不可用并写明原因——图纸→建筑的放置事务与世界状态绑定
    /// 都还没有调用者，这两条不属于本变更范围（见 tasks.md 第 7 节边界）。
    ///
    /// **本页刻意不挡世界输入**（覆写 <see cref="BlocksWorldInput"/>）：规格写明
    /// 「世界虚影可见，底部居中操作条，只拦截 UI 占用区域，不用全屏遮罩」。若它挡输入，
    /// 玩家就一边看着页面一边点不到世界，落位预览也永远不动——这条判据由
    /// `WorldPlacementForm` 的 PlayMode 用例守着。
    /// </summary>
    public sealed partial class WorldPlacementForm : AutoEraShellFormBase
    {
        /// <summary>规格页序：0 建造放置、1 机器部署、2 世界绑定。</summary>
        public const int PageBuildPlacement = 0;
        public const int PageMachineDeployment = 1;
        public const int PageWorldBinding = 2;

        private static readonly UiDetailField[] NoFields = new UiDetailField[0];

        private const string PlacementMissing =
            "建造放置事务尚未接入运行路径：图纸→建筑的落位流程还没有调用者，"
            + "因此选择目标、校验与确认暂不可用。";

        private const string BindingMissing =
            "世界状态绑定尚未接入运行路径：绑定候选与绑定事务都还没有调用者，因此本页暂不可用。";

        /// <summary>本页「整页不可用」的三种不同原因——它们对玩家意味着不同的事，不能合成一句。</summary>
        private enum DeploymentUnavailable
        {
            /// <summary>不是不可用态，正在正常落位。</summary>
            None,
            /// <summary>缺能力：没有现场区域或没有世界输入模块。</summary>
            Capability,
            /// <summary>缺对象：打开时没带要部署的机器身份。</summary>
            NoTarget,
            /// <summary>流程拒绝了这次落位（已部署、未配置占地、机器不在册……）。</summary>
            Refused,
        }

        private readonly List<UiDetailField> _selectionFields = new List<UiDetailField>(8);
        private readonly List<UiDetailField> _validityFields = new List<UiDetailField>(8);
        private readonly List<UiDetailField> _hintFields = new List<UiDetailField>(8);

        private IRegionReadModel _region;
        private MachineDeploymentFlow _deployment;
        private AutoEra.Input.RegionInputModule _input;
        private PersistentId _deploymentTarget = PersistentId.Invalid;
        private DeploymentUnavailable _unavailable = DeploymentUnavailable.None;
        private string _notice;

        // 预览状态的上一次取值：只在这些字段变化时才重绘，避免每帧造字符串与重排列表。
        private bool _hasPreviewState;
        private bool _lastValid;
        private bool _lastPlaced;
        private string _lastReason;
        private Vector2 _lastPosition;
        private float _lastYaw;

        /// <summary>
        /// 只有机器部署页不挡世界输入：它需要在世界里挪动预览、旋转镜头、点击落位。
        /// 另外两页当前整页不可用，挡住输入反而是诚实的（它们什么都做不了）。
        /// </summary>
        public override bool BlocksWorldInput => CurrentPage != PageMachineDeployment;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            // 取消是唯一出口：关闭本页等同于取消本次放置（不写任何世界状态，因此总是安全）。
            foreach (Button button in GetComponentsInChildren<Button>(true))
            {
                if (button != null && button.name.EndsWith("Cancel", System.StringComparison.Ordinal))
                {
                    button.onClick.AddListener(CloseSelf);
                }
            }

            if (_machineDeploymentRotateButton != null) _machineDeploymentRotateButton.onClick.AddListener(RotatePlacement);
            if (_machineDeploymentConfirmButton != null) _machineDeploymentConfirmButton.onClick.AddListener(ConfirmPlacement);
            // 取消不另外挂监听：上面的 Cancel 通配已经让它关闭本页，而 OnAutoEraClose 会
            // 释放落位预览。两条监听指向同一件事只会让执行顺序变成隐患。
        }

        protected override void OnAutoEraOpen()
        {
            AutoEraUiPageRequest pageRequest = null;
            TryGetRequest(out pageRequest);
            int initialPage = pageRequest != null ? pageRequest.Page : PageBuildPlacement;
            _deploymentTarget = pageRequest != null ? pageRequest.Target : PersistentId.Invalid;
            ShowPage(_pageRoots, initialPage);

            _region = RegionReadModels.Create(TryGetSession(out AutoEraUiSession session) ? session : null);
            _region.Changed += OnRegionSectionChanged;

            if (CurrentPage == PageMachineDeployment)
            {
                ApplyDefaultFocus(
                    _machineDeploymentCancelButton != null ? _machineDeploymentCancelButton.gameObject : null,
                    _machineDeploymentRotateButton != null ? _machineDeploymentRotateButton.gameObject : null);
                BeginDeployment();
            }
            else
            {
                ApplyDefaultFocus(null, _firstInteractable);
                Render(_region.Snapshot);
                DisableDomainActions();
            }
        }

        protected override void OnAutoEraClose(bool isShutdown) => ReleaseDeploymentAndRegion();

        /// <summary>
        /// 取消意图在本 Form 的**每一页**都表示「关闭本页」。
        ///
        /// 为什么必须在这里拦：本 Form 在 UITable 里的 `EscapeClose` 是 false，所以基类
        /// `TryHandleIntent(Cancel)` 会返回 false。在机器部署页上后果更明显——本页**不挡世界输入**，
        /// 于是 <c>RegionInputModule</c> 会接着按自己的取消分支把落位预览结束掉，
        /// 结果是「按 Esc 预览消失了，页面还开着」：玩家既没在落位也不知道为什么。
        /// （另外两页整页不可用，但「取消」是它们唯一保留的出口，同样必须真的能出去。）
        ///
        /// 本页没有 <c>Btn_FormBack</c>（全屏根），关闭 = 放弃本次放置且不写任何世界状态，
        /// 因此永远安全。预览由 <see cref="ReleaseDeploymentAndRegion"/> 统一释放。
        /// </summary>
        protected override bool OnBeforeFormIntent(AutoEraUiIntent intent)
        {
            if (intent == AutoEraUiIntent.Cancel)
            {
                CloseSelf();
                return true;
            }

            return base.OnBeforeFormIntent(intent);
        }

        protected override void OnAutoEraRecycle()
        {
            ReleaseDeploymentAndRegion();
            base.OnAutoEraRecycle();
        }

        /// <summary>
        /// 预览会随指针移动，所以合法性／位置／朝向必须每帧复核——但**只在真的变化时重绘**。
        /// 落位结束后本方法自然什么都不做（<c>_deployment.IsActive</c> 为 false）。
        /// </summary>
        private void Update()
        {
            MachineDeploymentFlow flow = _deployment;
            if (flow == null) return;

            RegionPlacementPreview preview = flow.IsActive ? flow.Preview : null;
            bool valid = preview != null && preview.IsValid;
            bool placed = preview != null && preview.IsActive;
            string reason = preview != null ? preview.Reason : null;
            Vector2 position = preview != null ? preview.Position : Vector2.zero;
            float yaw = preview != null ? preview.Yaw : 0f;

            if (_hasPreviewState && valid == _lastValid && placed == _lastPlaced
                && string.Equals(reason, _lastReason, System.StringComparison.Ordinal)
                && position == _lastPosition && Mathf.Approximately(yaw, _lastYaw))
            {
                return;
            }

            _hasPreviewState = true;
            _lastValid = valid;
            _lastPlaced = placed;
            _lastReason = reason;
            _lastPosition = position;
            _lastYaw = yaw;
            RenderMachineDeployment();
        }

        private void ReleaseDeploymentAndRegion()
        {
            // 顺序：先撤「流程拥有」的预览（流程自己释放），再让输入模块忘掉它，最后放读模型。
            // 反过来的话输入模块会继续指向一个已经释放的预览，并在指针移动时踩空。
            if (_deployment != null)
            {
                if (_input != null) _input.CancelPlacement();
                _deployment.Dispose();
                _deployment = null;
            }

            _input = null;
            _unavailable = DeploymentUnavailable.None;
            _notice = null;
            _hasPreviewState = false;
            _lastReason = null;

            if (_region == null)
            {
                return;
            }

            _region.Changed -= OnRegionSectionChanged;
            _region.Dispose();
            _region = null;
        }

        /// <summary>按规格页序切换内容页；越界调用无副作用。切到机器部署页时开始落位，切走时撤销。</summary>
        public bool ShowFormPage(int page)
        {
            if (!ShowPage(_pageRoots, page))
            {
                return false;
            }

            if (page == PageMachineDeployment) BeginDeployment();
            else CancelPlacement();
            return true;
        }

        /// <summary>区域域当前数据状态；null 表示读模型尚未建立。测试与调试用。</summary>
        public UiDataState? RegionDataState => _region?.Snapshot.State;

        /// <summary>本次落位要部署的机器身份。测试与调试用。</summary>
        public PersistentId DeploymentTarget => _deploymentTarget;

        /// <summary>是否正处于一次落位中。测试与调试用。</summary>
        public bool IsDeploying => _deployment != null && _deployment.IsActive;

        /// <summary>当前落位预览的合法性；没有进行中的落位时为 null。测试与调试用。</summary>
        public bool? PlacementValid => _deployment != null && _deployment.IsActive ? _deployment.Preview.IsValid : (bool?)null;

        /// <summary>最近一次提交的结局；从未提交时为 null。测试与调试用。</summary>
        public MachineDeploymentOutcome? DeploymentOutcome => _deployment?.LastOutcome;

        // ------------------------------------------------------------ 机器部署页

        private void BeginDeployment()
        {
            CancelPlacement();

            if (!TryGetSession(out AutoEraUiSession session) || !session.HasWorld || !session.HasRegion)
            {
                RenderDeploymentUnavailable(DeploymentUnavailable.Capability,
                    "机器部署需要一个已就绪的现场区域：当前会话里没有可用的世界会话或区域。");
                return;
            }

            if (!session.HasRegionInput)
            {
                RenderDeploymentUnavailable(DeploymentUnavailable.Capability,
                    "该现场没有世界输入模块：落位预览没有可驱动它的输入，本页暂不能部署。");
                return;
            }

            if (!_deploymentTarget.IsValid)
            {
                RenderDeploymentUnavailable(DeploymentUnavailable.NoTarget,
                    "没有指定要部署的机器：请从「未部署机器」或「机器整备」页的部署入口进入本页。");
                return;
            }

            var flow = new MachineDeploymentFlow(session.World, session.Region);
            if (!flow.TryBegin(_deploymentTarget, out string reason))
            {
                flow.Dispose();
                RenderDeploymentUnavailable(DeploymentUnavailable.Refused, reason);
                return;
            }

            _input = session.RegionInput;
            _deployment = flow;
            _unavailable = DeploymentUnavailable.None;
            _notice = null;
            _hasPreviewState = false;
            _input.BeginPlacement(flow);
            RenderMachineDeployment();
        }

        /// <summary>旋转：与键盘旋转键同一条路径（改的是同一个预览），不做任何领域提交。</summary>
        public void RotatePlacement()
        {
            _input?.RotatePlacement();
        }

        /// <summary>
        /// 把预览挪到指定区域坐标，返回是否挪动成功。
        ///
        /// **生产路径不经过这里**：世界里指针移动由 <c>RegionInputModule</c> 换算并驱动同一个预览。
        /// 这个方法存在是为了让「选点 → 校验 → 提交」这段可被确定性验证（测试与调试入口，
        /// 与 <c>TrySpawnMachine</c>／<c>FindMachineView</c> 同一性质）。
        /// </summary>
        public bool MovePlacementTo(Vector2 position)
        {
            if (_deployment == null || !_deployment.IsActive || _deployment.Preview == null)
            {
                return false;
            }

            _deployment.Preview.Move(position);
            _hasPreviewState = false;
            RenderMachineDeployment();
            return true;
        }

        /// <summary>确认部署：走输入模块里与鼠标点击完全相同的那条提交路径。</summary>
        public void ConfirmPlacement()
        {
            if (_input == null || _deployment == null)
            {
                return;
            }

            _input.ConfirmPlacement();
            _hasPreviewState = false;
            RenderMachineDeployment();
        }

        /// <summary>取消：放弃本次落位并把世界里的预览收干净。已提交的结局是历史事实，不会被清掉。</summary>
        public void CancelPlacement()
        {
            if (_deployment != null)
            {
                if (_input != null) _input.CancelPlacement();
                _deployment.Cancel();
                _deployment = null;
            }

            _input = null;
            _hasPreviewState = false;
        }

        private void RenderDeploymentUnavailable(DeploymentUnavailable kind, string reason)
        {
            _unavailable = kind;
            _notice = reason;

            SetState(_machineDeploymentLoadingState, false);
            SetState(_machineDeploymentEmptyState, kind == DeploymentUnavailable.NoTarget);
            SetState(_machineDeploymentErrorState, kind == DeploymentUnavailable.Refused);
            SetState(_machineDeploymentSuccessState, false);
            SetState(_machineDeploymentDisabledState, kind == DeploymentUnavailable.Capability);

            SetText(_machineDeploymentSelectionBody, reason);
            SetText(_machineDeploymentValidityBody, reason);
            SetText(_machineDeploymentHintsBody, reason);

            RenderDetailRows(_machineDeploymentSelectionTemplate, _machineDeploymentSelectionContent, NoFields);
            RenderDetailRows(_machineDeploymentValidityTemplate, _machineDeploymentValidityContent, NoFields);
            RenderDetailRows(_machineDeploymentHintsTemplate, _machineDeploymentHintsContent, NoFields);

            // 没有可用的落位时，旋转与确认必须禁用；取消保留（唯一的出口）。
            SetInteractable(_machineDeploymentRotateButton, false);
            SetInteractable(_machineDeploymentConfirmButton, false);
            SetInteractable(_machineDeploymentCancelButton, true);
        }

        private void RenderMachineDeployment()
        {
            MachineDeploymentFlow flow = _deployment;
            if (flow == null || _unavailable != DeploymentUnavailable.None)
            {
                return;
            }

            bool deployed = flow.LastOutcome == MachineDeploymentOutcome.Deployed;
            bool failed = flow.LastOutcome.HasValue && !deployed;

            SetState(_machineDeploymentLoadingState, false);
            SetState(_machineDeploymentEmptyState, false);
            SetState(_machineDeploymentErrorState, failed);
            SetState(_machineDeploymentSuccessState, deployed);
            SetState(_machineDeploymentDisabledState, false);

            SetText(_machineDeploymentSelectionBody, SelectionSummary());
            SetText(_machineDeploymentValidityBody, ValiditySummary(flow));
            SetText(_machineDeploymentHintsBody, HintSummary());

            RenderDetailRows(_machineDeploymentSelectionTemplate, _machineDeploymentSelectionContent, BuildSelectionFields());
            RenderDetailRows(_machineDeploymentValidityTemplate, _machineDeploymentValidityContent, BuildValidityFields(flow));
            RenderDetailRows(_machineDeploymentHintsTemplate, _machineDeploymentHintsContent, BuildHintFields());

            // 部署成功后本次落位就结束了（预览已交给区域对象），按钮随之失效；
            // 取消仍然可用——它是这个全屏根唯一的出口。
            SetInteractable(_machineDeploymentRotateButton, flow.IsActive);
            SetInteractable(_machineDeploymentConfirmButton, flow.IsActive);
            SetInteractable(_machineDeploymentCancelButton, true);
        }

        private string SelectionSummary()
        {
            if (_region == null)
            {
                return PlacementMissing;
            }

            return _region.Snapshot.State == UiDataState.Unavailable
                ? _region.Snapshot.UnavailableReason ?? "区域数据不可用"
                : "待部署机器（来自「未部署机器」／「机器整备」页的部署入口）。";
        }

        private string ValiditySummary(MachineDeploymentFlow flow)
        {
            if (flow.LastOutcome == MachineDeploymentOutcome.Deployed)
            {
                return "已部署。机器生成为**未激活**状态，请在现场面板完成首次激活。";
            }

            if (flow.LastOutcome.HasValue)
            {
                return "部署失败：" + MachineDeploymentFlow.Describe(flow.LastOutcome.Value);
            }

            if (!flow.IsActive || flow.Preview == null)
            {
                return _notice ?? "本次落位已经结束。";
            }

            if (!flow.Preview.IsActive)
            {
                return "请在世界里选择落点：" + flow.Preview.Reason;
            }

            return flow.Preview.IsValid
                ? "当前位置可以部署。"
                : "当前位置不可部署：" + (flow.Preview.Reason ?? "原因未知");
        }

        private static string HintSummary() =>
            "在世界里移动指针选点，按旋转键或「旋转」改朝向，点击世界或「确认部署」提交。";

        private IReadOnlyList<UiDetailField> BuildSelectionFields()
        {
            _selectionFields.Clear();
            _selectionFields.Add(new UiDetailField("实例", _deploymentTarget.IsValid ? _deploymentTarget.Value.ToString() : "—"));

            if (!TryGetSession(out AutoEraUiSession session) || !session.HasWorld
                || !session.World.Machines.TryGet(_deploymentTarget, out AutoEra.Machines.MachineInstance machine))
            {
                _selectionFields.Add(new UiDetailField("机器", "—"));
                return _selectionFields;
            }

            _selectionFields.Add(new UiDetailField("型号", machine.Definition.Name));
            _selectionFields.Add(new UiDetailField("等级", machine.Definition.Level.ToString()));
            _selectionFields.Add(new UiDetailField("槽位", string.Format("传感器 {0} ／ 核心 {1} ／ 执行器 {2}",
                machine.Definition.SensorSlots, machine.Definition.CoreSlots, machine.Definition.EffectorSlots)));
            _selectionFields.Add(new UiDetailField("算力", machine.ComputeCapacity.ToString()));
            // 第一版只有唯一经营区域（规格：区域字段只读，不做第二区域或跨区域重部署）。
            _selectionFields.Add(new UiDetailField("当前区域", "现场区域（第一版唯一经营区域）"));
            return _selectionFields;
        }

        private IReadOnlyList<UiDetailField> BuildValidityFields(MachineDeploymentFlow flow)
        {
            _validityFields.Clear();
            Vector2 size = flow.Size != Vector2.zero ? flow.Size : flow.Preview != null ? flow.Preview.Size : Vector2.zero;
            _validityFields.Add(new UiDetailField("占地", string.Format("{0:0.##} × {1:0.##} 米", size.x, size.y)));

            if (flow.IsActive && flow.Preview != null)
            {
                RegionPlacementPreview preview = flow.Preview;
                _validityFields.Add(new UiDetailField("位置", preview.IsActive
                    ? string.Format("X {0:0.##} ／ Z {1:0.##}", preview.Position.x, preview.Position.y)
                    : "—"));
                _validityFields.Add(new UiDetailField("朝向", preview.IsActive ? string.Format("{0:0}°", preview.Yaw) : "—"));
                _validityFields.Add(new UiDetailField("合法性", preview.IsActive
                    ? preview.IsValid ? "可以部署" : preview.Reason ?? "不可部署"
                    : preview.Reason ?? "请选择位置"));
            }
            else
            {
                _validityFields.Add(new UiDetailField("位置", "—"));
                _validityFields.Add(new UiDetailField("朝向", "—"));
                _validityFields.Add(new UiDetailField("合法性",
                    flow.LastOutcome.HasValue ? MachineDeploymentFlow.Describe(flow.LastOutcome.Value) : "本次落位已结束"));
            }

            _validityFields.Add(new UiDetailField("部署后状态", "未激活；需在现场面板完成首次激活"));
            return _validityFields;
        }

        private IReadOnlyList<UiDetailField> BuildHintFields()
        {
            _hintFields.Clear();
            _hintFields.Add(new UiDetailField("放置", "移动指针，预览跟随指针落在区域地面上"));
            _hintFields.Add(new UiDetailField("旋转", "旋转键或「旋转」按钮改变预览朝向"));
            _hintFields.Add(new UiDetailField("确认", "点击世界，或按「确认部署」"));
            _hintFields.Add(new UiDetailField("取消", "「取消」放弃本次落位，机器的配置保持不变"));
            _hintFields.Add(new UiDetailField("镜头", "拖动旋转视角，滚轮缩放"));
            return _hintFields;
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

        // ------------------------------------------------------------ 尚未接入的两页

        private void OnRegionSectionChanged(RegionDomainSection section) => Render(_region.Snapshot);

        private void Render(RegionDomainSnapshot snapshot)
        {
            // 建造放置与世界绑定两页的形状一致：选择栏 + 校验栏 + 提示栏。
            // 它们都还没有数据来源，所以三栏都陈述原因，并显式清空段落行——
            // 不能留着预制体里的示例行冒充真实数据。
            RenderUnavailablePage(snapshot, PlacementMissing,
                _buildPlacementSelectionBody, _buildPlacementValidityBody, _buildPlacementHintsBody,
                _buildPlacementSelectionTemplate, _buildPlacementSelectionContent,
                _buildPlacementValidityTemplate, _buildPlacementValidityContent,
                _buildPlacementHintsTemplate, _buildPlacementHintsContent,
                _buildPlacementLoadingState, _buildPlacementEmptyState, _buildPlacementErrorState,
                _buildPlacementSuccessState, _buildPlacementDisabledState);

            RenderUnavailablePage(snapshot, BindingMissing,
                _worldBindingPurposeBody, _worldBindingCandidateBody, _worldBindingHintsBody,
                _worldBindingPurposeTemplate, _worldBindingPurposeContent,
                _worldBindingCandidateTemplate, _worldBindingCandidateContent,
                _worldBindingHintsTemplate, _worldBindingHintsContent,
                _worldBindingLoadingState, _worldBindingEmptyState, _worldBindingErrorState,
                _worldBindingSuccessState, _worldBindingDisabledState);
        }

        private void RenderUnavailablePage(
            RegionDomainSnapshot snapshot,
            string missing,
            TMPro.TMP_Text firstBody,
            TMPro.TMP_Text secondBody,
            TMPro.TMP_Text thirdBody,
            GameObject firstTemplate,
            RectTransform firstContent,
            GameObject secondTemplate,
            RectTransform secondContent,
            GameObject thirdTemplate,
            RectTransform thirdContent,
            GameObject loadingState,
            GameObject emptyState,
            GameObject errorState,
            GameObject successState,
            GameObject disabledState)
        {
            ShowPageUnavailable(
                snapshot.State == UiDataState.Unavailable
                    ? snapshot.UnavailableReason ?? missing
                    : missing,
                loadingState, emptyState, errorState, successState, disabledState,
                firstBody, secondBody, thirdBody);

            RenderDetailRows(firstTemplate, firstContent, NoFields);
            RenderDetailRows(secondTemplate, secondContent, NoFields);
            RenderDetailRows(thirdTemplate, thirdContent, NoFields);
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
