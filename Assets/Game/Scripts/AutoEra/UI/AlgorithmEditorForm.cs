using System.Collections.Generic;
using AutoEra.Algorithms;
using AutoEra.UI.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 算法工作台（规格 13-算法编辑器：编辑与诊断两种模式、公开参数为精简内容页）。
    ///
    /// 绑定字段在同名的 .Fields.cs 里，结构由契约生成。
    ///
    /// 数据来源是**这台机器的区域运行时**：`RegionMachineRuntimeRegistry` 在部署时创建的
    /// `MachineExecutionContext` 与 `AlgorithmInstanceService`。机器身份取区域当前的选中对象
    /// （稳定 Id，不按名字查找）。因此本页现在有三种真实状态，而不是只有「整页不可用」：
    /// <list type="bullet">
    /// <item>Unavailable：缺会话／世界／区域／运行时／选中的机器——各自写明缺什么；</item>
    /// <item>Empty：运行时在，但这台机器还没有算法实例（实例由模板实例化创建，模板库还没接线）；</item>
    /// <item>Ready：有真实实例，节点栏列出选中实例草稿的图节点与连线，检视器列出机器/实例/节点属性，问题栏列出校验问题。</item>
    /// </list>
    /// 「应用草稿」按钮已接线（统一应用命令：草稿首应用＝激活、已激活草稿＝Apply）；
    /// 其余写入口（改图、诊断运行、参数编辑）仍未接线，因此除应用外的业务按钮仍由
    /// <c>DisableDomainActions</c> 处置——但只读部分是真实数据（图结构 + 校验问题）。
    ///
    /// 注：`Content_AlgorithmGraph` 上挂的是 <see cref="AutoEraGraphLayoutGroup"/>——规格提到
    /// `GraphLayoutGroup` 是设计提出、仓库未实现的组件，原型阶段用它满足「Content_ 必须有
    /// LayoutGroup」的结构契约，同时不覆写画布自由坐标。
    /// </summary>
    public sealed partial class AlgorithmEditorForm : AutoEraShellFormBase
    {
        /// <summary>规格页序：0 算法编辑、1 公开参数。</summary>
        public const int PageEditor = 0;
        public const int PagePublicParameters = 1;

        private static readonly int[] NavigationPageIndex = { PageEditor, -1 };

        private static readonly UiDetailField[] NoFields = new UiDetailField[0];

        private IAlgorithmReadModel _algorithms;
        private string _nodeFilter = string.Empty;
        private readonly List<UiAlgorithmNodeRow> _visibleNodes = new List<UiAlgorithmNodeRow>(16);

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

                    button.onClick.AddListener(() => ShowEditorPage(page));
                }
            }

            if (_backButton != null) _backButton.onClick.AddListener(RequestCancel);
            if (_closeButton != null) _closeButton.onClick.AddListener(RequestCancel);
            if (_algorithmEditorApplyButton != null) _algorithmEditorApplyButton.onClick.AddListener(OnApplyClicked);
            if (_algorithmEditorNodeSearch != null) _algorithmEditorNodeSearch.onValueChanged.AddListener(OnNodeSearchChanged);
        }

        protected override void OnAutoEraOpen()
        {
            int initialPage = TryGetRequest(out AutoEraUiPageRequest pageRequest) ? pageRequest.Page : PageEditor;
            ShowPage(_pageRoots, initialPage);
            ApplyDefaultFocus(
                _backButton != null ? _backButton.gameObject : null,
                _navButtons != null && _navButtons.Length > 0 && _navButtons[0] != null ? _navButtons[0].gameObject : null);

            _algorithms = AlgorithmReadModels.Create(TryGetSession(out AutoEraUiSession session) ? session : null);
            _algorithms.Changed += OnAlgorithmSectionChanged;

            Render(_algorithms.Snapshot);
            DisableDomainActions();
            // 应用命令已接线：整域禁用后单独启用「应用草稿」按钮，其余业务按钮仍未接线、保持禁用。
            SetApplyButtonInteractable(_algorithms.Snapshot);
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
        public bool ShowEditorPage(int page) => ShowPage(_pageRoots, page);

        /// <summary>算法域当前数据状态；null 表示读模型尚未建立。测试与调试用。</summary>
        public UiDataState? AlgorithmDataState => _algorithms?.Snapshot.State;

        private void RequestCancel() => TryHandleIntent(AutoEraUiIntent.Cancel);

        private void OnAlgorithmSectionChanged(AlgorithmDomainSection section) => Render(_algorithms.Snapshot);

        /// <summary>点一行图节点 → 交给读模型按稳定 Id 选中，供检视器展示节点属性。</summary>
        private void OnNodeRowClicked(int index)
        {
            if (_algorithms == null || index < 0 || index >= _visibleNodes.Count)
            {
                return;
            }

            _algorithms.SelectNode(_visibleNodes[index].Id);
        }

        /// <summary>搜索框输入 → 更新过滤词并重渲染节点栏。</summary>
        private void OnNodeSearchChanged(string value)
        {
            _nodeFilter = value ?? string.Empty;
            if (_algorithms != null)
            {
                Render(_algorithms.Snapshot);
            }
        }

        private static bool MatchesNodeFilter(UiAlgorithmNodeRow node, string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return true;
            }

            string label = node.Label ?? string.Empty;
            string status = node.Status ?? string.Empty;
            return label.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) >= 0
                || status.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// 三种状态各自渲染，不再一律切 Disabled：
        /// Unavailable＝缺能力（写清缺什么）；Empty＝运行时在、实例还没有（写清为什么没有）；
        /// Ready＝有真实实例，节点栏列实例、检视器列机器与版本、问题栏陈述诊断尚未运行。
        /// 把这三者混成一个「整页不可用」正是接线前的老行为。
        /// </summary>
        private void Render(AlgorithmDomainSnapshot snapshot)
        {
            if (snapshot.State == UiDataState.Unavailable)
            {
                string reason = snapshot.UnavailableReason ?? "算法实例暂不可用。";

                ShowPageUnavailable(reason,
                    _algorithmEditorLoadingState, _algorithmEditorEmptyState, _algorithmEditorErrorState,
                    _algorithmEditorSuccessState, _algorithmEditorDisabledState,
                    _algorithmEditorNodesBody, _algorithmEditorInspectorBody, _algorithmEditorProblemsBody);

                ShowPageUnavailable(reason,
                    _publicParametersLoadingState, _publicParametersEmptyState, _publicParametersErrorState,
                    _publicParametersSuccessState, _publicParametersDisabledState,
                    _publicParametersParametersBody, _publicParametersImpactBody);

                RenderDetailRows(_algorithmEditorNodesTemplate, _algorithmEditorNodesContent, NoFields);
                RenderDetailRows(_algorithmEditorInspectorTemplate, _algorithmEditorInspectorContent, NoFields);
                RenderDetailRows(_algorithmEditorProblemsTemplate, _algorithmEditorProblemsContent, NoFields);
                RenderDetailRows(_publicParametersParametersTemplate, _publicParametersParametersContent, NoFields);
                RenderDetailRows(_publicParametersImpactTemplate, _publicParametersImpactContent, NoFields);
                return;
            }

            bool hasInstances = snapshot.InstanceCount > 0;
            bool hasGraph = snapshot.GraphNodeCount > 0;

            SetState(_algorithmEditorLoadingState, false);
            SetState(_algorithmEditorEmptyState, !hasInstances);
            SetState(_algorithmEditorErrorState, false);
            SetState(_algorithmEditorSuccessState, hasInstances);
            SetState(_algorithmEditorDisabledState, false);

            // 节点栏：选中实例草稿图的真实节点（按搜索框过滤；点一行在检视器查看属性）。
            _visibleNodes.Clear();
            if (snapshot.GraphNodes != null)
            {
                for (int i = 0; i < snapshot.GraphNodes.Count; i++)
                {
                    if (MatchesNodeFilter(snapshot.GraphNodes[i], _nodeFilter))
                    {
                        _visibleNodes.Add(snapshot.GraphNodes[i]);
                    }
                }
            }

            int visibleCount = _visibleNodes.Count;
            RenderListRows(_algorithmEditorNodesTemplate, _algorithmEditorNodesContent, visibleCount,
                (index, item) => item.Bind(index, _visibleNodes[index].Label, _visibleNodes[index].Status,
                    OnNodeRowClicked));
            SetText(_algorithmEditorNodesBody, hasGraph
                ? "实例 #" + InstanceIdLabel(snapshot)
                    + " · 图节点 " + snapshot.GraphNodeCount + " 个 · 连线 " + snapshot.GraphEdgeCount
                    + " 条" + (visibleCount != snapshot.GraphNodeCount
                        ? "（搜索命中 " + visibleCount + " 个）" : "")
                    + "（点一行查看属性）。"
                : hasInstances
                    ? "请选择一个实例查看它的图结构。"
                    : AlgorithmReadModels.NoInstanceReason);

            // 画布：按布局坐标定位图节点（画布自由布局；拖拽回写属后续）。
            RenderGraphCanvas(snapshot);

            // 检视器：机器 + 实例 + 图摘要；点选节点后追加节点属性。
            RenderDetailRows(_algorithmEditorInspectorTemplate, _algorithmEditorInspectorContent, BuildInspectorDetail(snapshot));
            SetText(_algorithmEditorInspectorBody, snapshot.MachineName != null
                ? "机器：" + snapshot.MachineName
                : "机器：—");

            // 问题栏：选中实例草稿的校验问题（错误/警告 + 节点定位），不再写死「诊断尚未运行」。
            RenderDetailRows(_algorithmEditorProblemsTemplate, _algorithmEditorProblemsContent, BuildProblems(snapshot));
            SetText(_algorithmEditorProblemsBody, hasGraph
                ? ProblemsSummary(snapshot)
                : hasInstances
                    ? "请选择一个实例查看校验问题。"
                    : AlgorithmReadModels.NoInstanceReason);

            // 公开参数页与编辑页同源：参数是实例草稿的一部分。
            SetState(_publicParametersLoadingState, false);
            SetState(_publicParametersEmptyState, !hasInstances);
            SetState(_publicParametersErrorState, false);
            SetState(_publicParametersSuccessState, hasInstances);
            SetState(_publicParametersDisabledState, false);
            RenderDetailRows(_publicParametersParametersTemplate, _publicParametersParametersContent, snapshot.Detail);
            RenderDetailRows(_publicParametersImpactTemplate, _publicParametersImpactContent, NoFields);
            SetText(_publicParametersParametersBody, hasInstances
                ? "以下是从实例草稿读出的真实版本与算力占用；参数编辑入口尚未接线。"
                : AlgorithmReadModels.NoInstanceReason);
            SetText(_publicParametersImpactBody, hasInstances
                ? "影响评估尚未运行：本页还没有接上校验入口。"
                : AlgorithmReadModels.NoInstanceReason);

            // 应用按钮随选中与域状态启用/禁用（DisableDomainActions 已把它关掉，这里按真实状态重开）。
            SetApplyButtonInteractable(snapshot);
        }

        /// <summary>画布渲染：按布局坐标定位图节点（画布自由布局；拖拽回写属后续）。</summary>
        private void RenderGraphCanvas(AlgorithmDomainSnapshot snapshot)
        {
            if (_algorithmGraphContent == null || _algorithmGraphElementTemplate == null)
            {
                return;
            }

            // 清空旧节点（保留模板自身）。
            for (int i = _algorithmGraphContent.childCount - 1; i >= 0; i--)
            {
                Transform child = _algorithmGraphContent.GetChild(i);
                if (child.gameObject != _algorithmGraphElementTemplate)
                {
                    Destroy(child.gameObject);
                }
            }

            if (snapshot.GraphNodes == null)
            {
                return;
            }

            for (int i = 0; i < snapshot.GraphNodes.Count; i++)
            {
                UiAlgorithmNodeRow node = snapshot.GraphNodes[i];
                GameObject instance = Instantiate(_algorithmGraphElementTemplate, _algorithmGraphContent);
                instance.SetActive(true);
                RectTransform rect = instance.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = new Vector2(node.LayoutX, node.LayoutY);
                }

                SetGraphNodeName(instance, node.Label);

                AlgorithmGraphNodeDragHandler drag = instance.GetComponent<AlgorithmGraphNodeDragHandler>();
                if (drag == null)
                {
                    drag = instance.AddComponent<AlgorithmGraphNodeDragHandler>();
                }

                ulong nodeId = node.Id;
                drag.OnMoved = position => OnGraphNodeMoved(nodeId, position);
            }
        }

        /// <summary>画布节点拖拽松手 → 写回 MoveNode（新坐标）。</summary>
        private void OnGraphNodeMoved(ulong nodeId, Vector2 position)
        {
            if (_algorithms == null || !_algorithms.Snapshot.SelectedInstance.HasValue)
            {
                return;
            }

            _algorithms.MoveNode(_algorithms.Snapshot.SelectedInstance.Value.Id, nodeId, position.x, position.y);
        }

        private static void SetGraphNodeName(GameObject instance, string label)
        {
            Transform nameText = instance.transform.Find("Grp_AlgorithmNode/Btn_AlgorithmNodeSelect/Txt_AlgorithmNodeName");
            if (nameText == null)
            {
                return;
            }

            TMPro.TMP_Text text = nameText.GetComponent<TMPro.TMP_Text>();
            if (text != null)
            {
                text.SetText(label ?? string.Empty);
            }
        }

        private void OnApplyClicked()
        {
            if (_algorithms == null)
            {
                return;
            }

            AlgorithmDomainSnapshot snapshot = _algorithms.Snapshot;
            if (!snapshot.SelectedInstance.HasValue)
            {
                return;
            }

            UiAlgorithmInstanceRow instance = snapshot.SelectedInstance.Value;
            if (instance.RequestState == AlgorithmApplyState.AwaitingWarningConfirmation)
            {
                // 有待确认的警告：同一按钮变「确认并应用」，走确认链。
                _algorithms.ConfirmWarnings(instance.Id, instance.RequestId);
            }
            else
            {
                // 统一应用命令：草稿首应用＝激活，已激活草稿＝Apply；结果经 Changed 事件刷新呈现。
                _algorithms.Apply(instance.Id);
            }
        }

        private void SetApplyButtonInteractable(AlgorithmDomainSnapshot snapshot)
        {
            if (_algorithmEditorApplyButton == null)
            {
                return;
            }

            if (snapshot.State != UiDataState.Ready || !snapshot.SelectedInstance.HasValue)
            {
                _algorithmEditorApplyButton.interactable = false;
                return;
            }

            AlgorithmApplyState state = snapshot.SelectedInstance.Value.RequestState;
            // 无请求＝可应用；待确认警告＝可确认；等待安全点/应用中＝禁用等待。
            _algorithmEditorApplyButton.interactable =
                state == AlgorithmApplyState.None || state == AlgorithmApplyState.AwaitingWarningConfirmation;
        }

        private static void SetText(TMPro.TMP_Text text, string value)
        {
            if (text != null)
            {
                text.SetText(value ?? string.Empty);
            }
        }

        private static string InstanceIdLabel(AlgorithmDomainSnapshot snapshot)
        {
            return snapshot.SelectedInstance.HasValue ? snapshot.SelectedInstance.Value.Id.ToString() : "—";
        }

        /// <summary>检视器内容＝机器/实例详情 + 图结构摘要 +（选中时）节点属性。</summary>
        private static IReadOnlyList<UiDetailField> BuildInspectorDetail(AlgorithmDomainSnapshot snapshot)
        {
            var list = new List<UiDetailField>(16);
            if (snapshot.Detail != null)
            {
                for (int i = 0; i < snapshot.Detail.Count; i++)
                {
                    list.Add(snapshot.Detail[i]);
                }
            }

            if (snapshot.InstanceCount > 0)
            {
                list.Add(new UiDetailField("图结构", "节点 " + snapshot.GraphNodeCount
                    + " · 连线 " + snapshot.GraphEdgeCount + " · 校验问题 " + snapshot.IssueCount));
            }

            // 诊断读路径：最近一次运行摘要；无历史时如实说明，不伪装「运行正常」。
            if (snapshot.LatestRun.HasValue)
            {
                UiAlgorithmRunRow run = snapshot.LatestRun.Value;
                list.Add(new UiDetailField("最近运行", run.Label));
                list.Add(new UiDetailField("结果", run.Succeeded ? "正常" : "错误：" + run.Error));
                if (run.FailedNode != 0)
                {
                    list.Add(new UiDetailField("失败节点", "#" + run.FailedNode));
                }

                list.Add(new UiDetailField("瞬时成本", run.Cost.ToString()));
            }
            else if (snapshot.GraphNodeCount > 0)
            {
                list.Add(new UiDetailField("最近运行", "暂无运行记录"));
            }

            if (snapshot.NodeDetail != null && snapshot.NodeDetail.Count > 0)
            {
                for (int i = 0; i < snapshot.NodeDetail.Count; i++)
                {
                    list.Add(snapshot.NodeDetail[i]);
                }
            }
            else if (snapshot.GraphNodeCount > 0)
            {
                list.Add(new UiDetailField("节点", "点选左侧节点查看属性"));
            }

            return list;
        }

        /// <summary>问题栏内容＝校验问题（错误/警告 + 节点定位）；无问题时明确写「通过」。</summary>
        private static IReadOnlyList<UiDetailField> BuildProblems(AlgorithmDomainSnapshot snapshot)
        {
            if (snapshot.Issues == null || snapshot.Issues.Count == 0)
            {
                return snapshot.GraphNodeCount > 0
                    ? new UiDetailField[] { new UiDetailField("校验", "通过（无错误、无警告）") }
                    : NoFields;
            }

            var list = new List<UiDetailField>(snapshot.Issues.Count);
            for (int i = 0; i < snapshot.Issues.Count; i++)
            {
                UiAlgorithmIssueRow issue = snapshot.Issues[i];
                list.Add(new UiDetailField(issue.Label, issue.Status));
            }

            return list;
        }

        private static string ProblemsSummary(AlgorithmDomainSnapshot snapshot)
        {
            int errors = 0;
            int warnings = 0;
            if (snapshot.Issues != null)
            {
                for (int i = 0; i < snapshot.Issues.Count; i++)
                {
                    if (snapshot.Issues[i].IsError)
                    {
                        errors++;
                    }
                    else
                    {
                        warnings++;
                    }
                }
            }

            return snapshot.IssueCount == 0
                ? "校验通过：无错误、无警告。"
                : "校验问题 " + snapshot.IssueCount + " 个（错误 " + errors + " · 警告 " + warnings + "）。";
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
