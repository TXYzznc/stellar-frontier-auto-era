using System.Collections.Generic;
using AutoEra.Algorithms;
using AutoEra.UI.Contracts;
using TMPro;
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
    /// <item>Empty：运行时在，但这台机器还没有算法实例；从模板库创建草稿实例。</item>
    /// <item>Ready：有真实实例，节点栏列出选中实例草稿的图节点与连线，检视器列出机器/实例/节点属性，问题栏列出校验问题。</item>
    /// </list>
    /// 「应用草稿」、节点编辑、端口连线、绑定入口、验证、公开参数草稿与默认值恢复已接线；
    /// 诊断模式目前切换为只读展示，历史记录选择与定位仍在后续验收项中。
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
        private bool _diagnosisMode;
        private string _nodeFilter = string.Empty;

        /// <summary>节点库当前选中的种类（添加入口用）；未选为 null。</summary>
        private AlgorithmNodeKind? _selectedKind;

        /// <summary>两步连线的待连源（输出侧）：null＝无待连。</summary>
        private (ulong nodeId, string port)? _pendingConnection;

        /// <summary>画布上选中的连线（完整边身份）；未选为 null。</summary>
        private (ulong from, string output, ulong to, string input)? _selectedEdge;
        private readonly List<AlgorithmNodeItemObject> _nodeItems = new List<AlgorithmNodeItemObject>(32);
        private readonly List<AlgorithmEdgeItemObject> _edgeItems = new List<AlgorithmEdgeItemObject>(64);
        private readonly List<AlgorithmGraphEdgeGraphic> _graphEdgeGraphics = new List<AlgorithmGraphEdgeGraphic>(64);
        private readonly Dictionary<ulong, GameObject> _nodeInstances = new Dictionary<ulong, GameObject>(32);
        private readonly Dictionary<(ulong node, string port, bool input), RectTransform> _portButtons =
            new Dictionary<(ulong node, string port, bool input), RectTransform>(64);
        private readonly Dictionary<ulong, Vector2> _graphPositions = new Dictionary<ulong, Vector2>(32);
        private System.Func<ulong, Vector2?> _graphPositionResolver;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            AddPanelBorder("Panel_AlgorithmEditorNodes");
            AddPanelBorder("Panel_AlgorithmEditorCanvas");
            AddPanelBorder("Panel_AlgorithmEditorInspector");
            AddPanelBorder("Panel_AlgorithmEditorProblems");
            _graphPositionResolver = ResolveGraphPosition;
            RectTransform graphViewport = _algorithmGraphContent != null
                ? _algorithmGraphContent.parent as RectTransform : null;
            if (graphViewport != null)
            {
                // Viewport 原本只有 Mask，没有 Graphic，空白区域不会产生 PointerEvent。
                // 透明 Image 只负责承接画布背景事件；节点自身的 Graphic 仍优先命中，
                // 因而不会遮挡节点按钮、端口按钮或其它子对象交互。
                Image interactionSurface = graphViewport.GetComponent<Image>();
                if (interactionSurface == null)
                {
                    interactionSurface = graphViewport.gameObject.AddComponent<Image>();
                }
                interactionSurface.color = new Color(1f, 1f, 1f, 0f);
                interactionSurface.raycastTarget = true;
                AlgorithmGraphCanvasInteraction interaction = graphViewport.GetComponent<AlgorithmGraphCanvasInteraction>();
                if (interaction == null)
                {
                    interaction = graphViewport.gameObject.AddComponent<AlgorithmGraphCanvasInteraction>();
                }
                interaction.Initialize(_algorithmGraphContent);
            }
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
                    if (page < 0) button.onClick.AddListener(() => SetActionGroups(true));
                    else button.onClick.AddListener(() => SetActionGroups(false));
                }
            }

            if (_backButton != null) _backButton.onClick.AddListener(RequestCancel);
            if (_closeButton != null) _closeButton.onClick.AddListener(RequestCancel);
            if (_algorithmEditorApplyButton != null) _algorithmEditorApplyButton.onClick.AddListener(OnApplyClicked);
            if (_algorithmEditorNodeSearch != null) _algorithmEditorNodeSearch.onValueChanged.AddListener(OnNodeSearchChanged);
            if (_algorithmEditorAddButton != null) _algorithmEditorAddButton.onClick.AddListener(OnAddClicked);
            if (_algorithmDeleteSelectedButton != null) _algorithmDeleteSelectedButton.onClick.AddListener(OnDeleteSelectedClicked);
            if (_algorithmEditorBindButton != null) _algorithmEditorBindButton.onClick.AddListener(OpenBindingForm);
            if (_algorithmEditorTemplateButton != null) _algorithmEditorTemplateButton.onClick.AddListener(OpenTemplateLibrary);
            if (_algorithmEditorValidateButton != null) _algorithmEditorValidateButton.onClick.AddListener(RefreshValidation);
            if (_algorithmEditorDiagnoseButton != null) _algorithmEditorDiagnoseButton.onClick.AddListener(ToggleDiagnosisMode);
            Button parameterButton = FindChild(transform, "Btn_AlgorithmEditorEdit")?.GetComponent<Button>();
            if (parameterButton != null) parameterButton.onClick.AddListener(() => ShowEditorPage(PagePublicParameters));
            if (_publicParametersApplyButton != null) _publicParametersApplyButton.onClick.AddListener(OnApplyClicked);
            if (_publicParametersChangeButton != null) _publicParametersChangeButton.onClick.AddListener(SelectFirstParameter);
            if (_publicParametersDefaultButton != null) _publicParametersDefaultButton.onClick.AddListener(ResetSelectedParameter);
            if (_publicParametersNumberValue != null) _publicParametersNumberValue.onEndEdit.AddListener(OnNumberParameterEdited);
            if (_publicParametersBooleanValue != null) _publicParametersBooleanValue.onValueChanged.AddListener(OnBooleanParameterEdited);
            if (_algorithmDiagnosisLocateButton != null) _algorithmDiagnosisLocateButton.onClick.AddListener(OpenDiagnosisTarget);
            if (_algorithmDiagnosisReturnEditButton != null) _algorithmDiagnosisReturnEditButton.onClick.AddListener(() => SetActionGroups(false));
            if (_algorithmDiagnosisNextStepButton != null) _algorithmDiagnosisNextStepButton.onClick.AddListener(SelectNextDiagnosticRun);
            if (_algorithmDiagnosisPreviousStepButton != null) _algorithmDiagnosisPreviousStepButton.onClick.AddListener(SelectPreviousDiagnosticRun);
            // 撤销/重做是占位（命令历史属后续批次），OnInit 里保持禁用、不再改状态。
        }

        private void AddPanelBorder(string panelName)
        {
            Transform panel = FindChild(transform, panelName);
            if (panel == null) return;
            Outline outline = panel.GetComponent<Outline>();
            if (outline == null) outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.22f, 0.48f, 0.66f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;
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

            SetActionGroups(diagnosis: false);
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
            RecycleGraphItems();
            if (_algorithms == null)
            {
                return;
            }

            _algorithms.Changed -= OnAlgorithmSectionChanged;
            _algorithms.Dispose();
            _algorithms = null;
        }

        private void SetActionGroups(bool diagnosis)
        {
            _diagnosisMode = diagnosis;
            if (_pageRoots == null || _pageRoots.Length <= PageEditor || _pageRoots[PageEditor] == null)
            {
                return;
            }

            ShowEditorPage(PageEditor);
            Transform editorActions = FindChild(_pageRoots[PageEditor].transform, "Grp_AlgorithmEditorActions");
            Transform diagnosisActions = FindChild(_pageRoots[PageEditor].transform, "Grp_AlgorithmDiagnosisActions");
            if (editorActions != null) editorActions.gameObject.SetActive(!diagnosis);
            if (diagnosisActions != null) diagnosisActions.gameObject.SetActive(diagnosis);
            if (_algorithmEditorDiagnoseButton != null)
            {
                TMPro.TMP_Text label = _algorithmEditorDiagnoseButton.transform.Find("Txt_AlgorithmEditorDiagnoseLabel")?.GetComponent<TMPro.TMP_Text>();
                if (label != null) label.SetText(diagnosis ? "返回编辑" : "切换诊断模式");
            }
            TMPro.TMP_Text title = FindChild(transform, "Txt_AlgorithmEditorTitle")?.GetComponent<TMPro.TMP_Text>();
            if (title != null) title.SetText(diagnosis ? "算法编辑模式｜诊断" : "算法编辑模式｜编辑");
            Debug.Log("[AutoEra][AlgorithmEditor] 模式切换：" + (diagnosis ? "诊断（只读）" : "编辑（可修改草稿）"));
            if (_algorithms != null) Render(_algorithms.Snapshot);
        }

        private static Transform FindChild(Transform root, string childName)
        {
            if (root == null) return null;
            if (root.name == childName) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindChild(root.GetChild(i), childName);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>按规格页序切换内容页；越界调用无副作用。</summary>
        public bool ShowEditorPage(int page) => ShowPage(_pageRoots, page);

        /// <summary>算法域当前数据状态；null 表示读模型尚未建立。测试与调试用。</summary>
        public UiDataState? AlgorithmDataState => _algorithms?.Snapshot.State;

        private void RequestCancel() => TryHandleIntent(AutoEraUiIntent.Cancel);

        private void OpenBindingForm()
        {
            if (_algorithms == null || !_algorithms.Snapshot.SelectedInstance.HasValue)
            {
                return;
            }

            AutoEraUiNavigator.Open(this, UIViews.AlgorithmBindingForm);
        }

        private void OpenTemplateLibrary()
        {
            Debug.Log("[AutoEra][AlgorithmEditor] 打开模板库：从当前机器上下文选择并实例化算法模板。");
            AutoEraUiNavigator.Open(this, UIViews.AlgorithmLibraryForm);
        }

        private void RefreshValidation()
        {
            _algorithms?.Refresh();
        }

        private void OpenDiagnosisTarget()
        {
            FocusSelectedGraphObject();
            AutoEraUiNavigator.Open(this, UIViews.FieldHudDetailForm, new AutoEraUiPageRequest(5));
        }

        /// <summary>把当前选中节点或连线带到画布视口中心。</summary>
        public bool FocusSelectedGraphObject()
        {
            if (_algorithmGraphContent == null) return false;

            Vector2? target = null;
            if (_algorithms != null && _algorithms.Snapshot.SelectedNode.HasValue)
            {
                ulong nodeId = _algorithms.Snapshot.SelectedNode.Value.Id;
                if (_graphPositions.TryGetValue(nodeId, out Vector2 nodePosition)) target = nodePosition;
            }

            if (!target.HasValue && _selectedEdge.HasValue
                && _graphPositions.TryGetValue(_selectedEdge.Value.from, out Vector2 from)
                && _graphPositions.TryGetValue(_selectedEdge.Value.to, out Vector2 to))
            {
                target = (from + to) * 0.5f;
            }

            if (!target.HasValue) return false;
            float zoom = _algorithmGraphContent.localScale.x;
            if (zoom <= 0f) zoom = 1f;
            _algorithmGraphContent.anchoredPosition = -target.Value * zoom;
            return true;
        }

        private void ToggleDiagnosisMode()
        {
            SetActionGroups(!_diagnosisMode);
        }

        private void SelectFirstParameter()
        {
            if (_algorithms == null || !_algorithms.Snapshot.SelectedInstance.HasValue)
            {
                return;
            }

            SetText(_publicParametersImpactBody, "参数编辑入口已选中；修改值后仍需通过统一应用验证。");
        }

        private void SelectNextDiagnosticRun()
        {
            if (_algorithms != null && _algorithms.SelectNextRun())
                SetText(_algorithmEditorProblemsBody, "已切换到下一条运行记录。执行节点状态已同步。");
            else
                SetText(_algorithmEditorProblemsBody, "已经是最新运行记录，或当前没有运行历史。");
        }

        private void SelectPreviousDiagnosticRun()
        {
            if (_algorithms != null && _algorithms.SelectPreviousRun())
                SetText(_algorithmEditorProblemsBody, "已切换到上一条运行记录。执行节点状态已同步。");
            else
                SetText(_algorithmEditorProblemsBody, "已经是最早运行记录，或当前没有运行历史。");
        }

        private void ResetSelectedParameter()
        {
            if (_algorithms == null || !_algorithms.Snapshot.SelectedInstance.HasValue || !_algorithms.Snapshot.SelectedNode.HasValue)
            {
                SetText(_publicParametersImpactBody, "请先选择一个公开参数。");
                return;
            }

            bool reset = _algorithms.ResetNodeDefault(_algorithms.Snapshot.SelectedInstance.Value.Id,
                _algorithms.Snapshot.SelectedNode.Value.Id);
            SetText(_publicParametersImpactBody, reset ? "已恢复到保存版本的默认值，应用前仍可继续修改。" : "该参数没有可恢复的保存默认值。");
        }

        private void OnNumberParameterEdited(string text)
        {
            if (_algorithms == null || !_algorithms.Snapshot.SelectedInstance.HasValue || !_algorithms.Snapshot.SelectedNode.HasValue)
                return;
            if (double.TryParse(text, out double number))
            {
                _algorithms.SetNodeDefault(_algorithms.Snapshot.SelectedInstance.Value.Id,
                    _algorithms.Snapshot.SelectedNode.Value.Id, AlgorithmValue.Numeric(number));
            }
        }

        private void OnBooleanParameterEdited(bool value)
        {
            if (_algorithms == null || !_algorithms.Snapshot.SelectedInstance.HasValue || !_algorithms.Snapshot.SelectedNode.HasValue)
                return;
            _algorithms.SetNodeDefault(_algorithms.Snapshot.SelectedInstance.Value.Id,
                _algorithms.Snapshot.SelectedNode.Value.Id, AlgorithmValue.Bool(value));
        }

        private void OnAlgorithmSectionChanged(AlgorithmDomainSection section) => Render(_algorithms.Snapshot);

        /// <summary>搜索框输入 → 更新过滤词并重渲染节点库。</summary>
        private void OnNodeSearchChanged(string value)
        {
            _nodeFilter = value ?? string.Empty;
            if (_algorithms != null)
            {
                Render(_algorithms.Snapshot);
            }
        }

        private static bool MatchesKindFilter(UiAlgorithmNodeKindRow row, string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return true;
            }

            string label = row.Label ?? string.Empty;
            string category = row.Category ?? string.Empty;
            return label.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) >= 0
                || category.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// 节点栏＝节点库（规格 13：分类/成本/搜索）。三个域都展示目录——它是能力清单；
        /// 点击行为受域状态约束：非 Ready 或未选实例时不发起草稿变更。
        /// </summary>
        private void RenderNodeLibrary(AlgorithmDomainSnapshot snapshot)
        {
            var visible = new List<UiAlgorithmNodeKindRow>(AlgorithmNodeLibrary.Rows.Length);
            for (int i = 0; i < AlgorithmNodeLibrary.Rows.Length; i++)
            {
                if (MatchesKindFilter(AlgorithmNodeLibrary.Rows[i], _nodeFilter))
                {
                    visible.Add(AlgorithmNodeLibrary.Rows[i]);
                }
            }

            bool canAdd = CanEditDraft(snapshot);
            RenderListRows(_algorithmEditorNodesTemplate, _algorithmEditorNodesContent, visible.Count,
                (index, item) => item.Bind(index, visible[index].Label, visible[index].Status,
                    canAdd ? (System.Action<int>)OnKindRowClicked : null));
            SetText(_algorithmEditorNodesBody, _pendingConnection.HasValue
                ? "连接模式：已选择节点 " + _pendingConnection.Value.nodeId + " 的输出端口“" + _pendingConnection.Value.port + "”；请点击目标输入端口，再次点击源端口可取消。"
                : canAdd
                ? "实例 #" + InstanceIdLabel(snapshot) + " · " + (_selectedKind.HasValue ? "已选 " + _selectedKind.Value + "；点击底部“添加节点”。" : "先点左侧种类行，再点击底部“添加节点”；端口连接在画布中完成。")
                : _diagnosisMode ? "诊断模式只读；点击顶部“编辑”后可添加节点。"
                : "先点“打开模板库”创建算法实例，再点左侧节点种类添加节点。");
        }

        /// <summary>域 Ready 且选中实例＝可编辑草稿。</summary>
        private bool CanEditDraft(AlgorithmDomainSnapshot snapshot)
            => !_diagnosisMode && snapshot.State == UiDataState.Ready && snapshot.SelectedInstance.HasValue;

        /// <summary>点节点库行：只选择种类；由底部“添加节点”命令创建草稿节点。</summary>
        private void OnKindRowClicked(int index)
        {
            UiAlgorithmNodeKindRow? picked = KindRowAt(index);
            if (!picked.HasValue)
            {
                return;
            }

            _selectedKind = picked.Value.Kind;
            Debug.Log("[AutoEra][AlgorithmEditor] 选择节点种类：" + picked.Value.Kind + "，等待点击“添加节点”。");
            SetText(_algorithmEditorNodesBody, "已选节点类型：" + picked.Value.Label + "。点击底部“添加节点”创建到画布。" );

            // 创建失败（如修订竞争）时 Changed 不会触发，这里即时同步一次按钮状态。
            if (_algorithmEditorAddButton != null && _algorithms != null)
            {
                _algorithmEditorAddButton.interactable = CanEditDraft(_algorithms.Snapshot) && _selectedKind.HasValue;
            }
        }

        /// <summary>过滤后的可见库行（与 RenderNodeLibrary 同一推导，避免存第二份状态）。</summary>
        private UiAlgorithmNodeKindRow? KindRowAt(int index)
        {
            if (index < 0)
            {
                return null;
            }

            int seen = 0;
            for (int i = 0; i < AlgorithmNodeLibrary.Rows.Length; i++)
            {
                if (!MatchesKindFilter(AlgorithmNodeLibrary.Rows[i], _nodeFilter))
                {
                    continue;
                }

                if (seen == index)
                {
                    return AlgorithmNodeLibrary.Rows[i];
                }

                seen++;
            }

            return null;
        }

        /// <summary>「添加节点」按钮：按当前选中的库种类创建（未选种类时无操作）。</summary>
        private void OnAddClicked()
        {
            if (_selectedKind.HasValue)
            {
                Debug.Log("[AutoEra][AlgorithmEditor] 点击添加节点：" + _selectedKind.Value);
                TryCreateNode(_selectedKind.Value);
            }
            else Debug.Log("[AutoEra][AlgorithmEditor] 点击添加节点但尚未选择节点种类。");
        }

        /// <summary>创建节点：域 Ready + 选中实例 + 网格步进落位（列距 300、行距 220、8 列换行）。</summary>
        private void TryCreateNode(AlgorithmNodeKind kind)
        {
            if (_algorithms == null)
            {
                return;
            }

            AlgorithmDomainSnapshot snapshot = _algorithms.Snapshot;
            if (!CanEditDraft(snapshot))
            {
                return;
            }

            int count = snapshot.GraphNodeCount;
            float x = (count % 8) * 300f;
            float y = -(count / 8) * 220f;
            _algorithms.CreateNode(snapshot.SelectedInstance.Value.Id, kind, x, y);
            // 结果经 Changed 事件刷新（成功→画布多一个节点；失败→快照不变）。
        }

        /// <summary>两步连线·第一步：点输出端口记录/取消/换源，并局部刷新让「*」待连标记可见。</summary>
        private void OnOutputPortClicked(ulong nodeId, string port)
        {
            if (_algorithms == null || !CanEditDraft(_algorithms.Snapshot)) return;
            Debug.Log("[AutoEra][AlgorithmEditor] 选择输出端口：节点 " + nodeId + " / " + port);
            if (_pendingConnection.HasValue && _pendingConnection.Value.nodeId == nodeId && _pendingConnection.Value.port == port)
            {
                _pendingConnection = null; // 再点同一端口＝取消
                SetText(_algorithmEditorNodesBody, "已取消连接，请选择节点类型或端口继续编辑。");
            }
            else
            {
                _pendingConnection = (nodeId, port); // 记录或换源
                SetText(_algorithmEditorNodesBody, "连接模式：已选择节点 " + nodeId + " 的输出端口“" + port + "”；请点击目标输入端口，再次点击源端口可取消。");
            }

            // 待连源不发命令、不触发 Changed：局部重绘该节点的端口行才有可见反馈。
            // 刻意不做全量 Render——那会重建整个画布，仅为了一个「*」标记代价过高。
            RefreshPortsForNode(nodeId);
        }

        /// <summary>两步连线·第二步：点输入端口 → Connect；失败保留待连源（规格语义）。</summary>
        private void OnInputPortClicked(ulong nodeId, string port)
        {
            if (_algorithms == null || !_pendingConnection.HasValue)
            {
                return;
            }

            AlgorithmDomainSnapshot snapshot = _algorithms.Snapshot;
            if (!CanEditDraft(snapshot))
            {
                _pendingConnection = null;
                return;
            }

            (ulong from, string output) = _pendingConnection.Value;
            Debug.Log("[AutoEra][AlgorithmEditor] 尝试连接：节点 " + from + " / " + output + " -> 节点 " + nodeId + " / " + port);
            // 先清待连源再发命令：Connect 成功会经 service.Changed **同步重入** Render，
            // 那时端口行的「*」标记必须已经消失（否则重入渲染之后没人再刷新它）。
            _pendingConnection = null;
            if (!_algorithms.Connect(snapshot.SelectedInstance.Value.Id, from, output, nodeId, port))
            {
                // 失败（类型不兼容/输入已占用/修订竞争）：保留待连源供重选，
                // 并局部重绘源节点让「*」继续可见；失败原因经问题栏校验结果呈现。
                _pendingConnection = (from, output);
                SetText(_algorithmEditorNodesBody, "连接失败：输入端口类型不匹配、已被占用或草稿版本已变化。请重新选择目标输入端口。");
                RefreshPortsForNode(from);
            }
            else
            {
                SetText(_algorithmEditorNodesBody, "连接已创建：节点 " + from + " / " + output + " → 节点 " + nodeId + " / " + port + "。");
            }
        }

        /// <summary>局部重绘单个节点的端口行（待连标记变化时不重建整个画布）。</summary>
        private void RefreshPortsForNode(ulong nodeId)
        {
            if (_algorithms == null || _algorithmGraphContent == null)
            {
                return;
            }

            AlgorithmDomainSnapshot snapshot = _algorithms.Snapshot;
            if (snapshot.GraphNodes == null)
            {
                return;
            }

            UiAlgorithmNodeRow? row = null;
            for (int i = 0; i < snapshot.GraphNodes.Count; i++)
            {
                if (snapshot.GraphNodes[i].Id == nodeId)
                {
                    row = snapshot.GraphNodes[i];
                    break;
                }
            }

            if (!row.HasValue)
            {
                return;
            }

            for (int i = 0; i < _algorithmGraphContent.childCount; i++)
            {
                Transform element = _algorithmGraphContent.GetChild(i);
                if (!element.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Transform nodeGroup = element.Find("Grp_AlgorithmNode");
                if (nodeGroup == null || !nodeGroup.gameObject.activeSelf)
                {
                    continue;
                }

                TMPro.TMP_Text nameText = nodeGroup.Find("Btn_AlgorithmNodeSelect/Txt_AlgorithmNodeName")?.GetComponent<TMPro.TMP_Text>();
                AlgorithmNodeItem nodeItem = element.GetComponent<AlgorithmNodeItem>();
                if (nodeItem != null && nodeItem.NodeId == nodeId)
                {
                    RenderNodePorts(element.gameObject, row.Value);
                    return;
                }
            }
        }

        /// <summary>画布选中连线（边元素按钮）。</summary>
        private void OnEdgeSelected(ulong from, string output, ulong to, string input)
        {
            _selectedEdge = (from, output, to, input);
            // 重新渲染一次，让选中的边立即使用高亮色反馈；节点选择仍由读模型保持。
            if (_algorithms != null) Render(_algorithms.Snapshot);
            RefreshDraftToolButtons(_algorithms != null ? _algorithms.Snapshot : default);
        }

        /// <summary>画布选中节点（节点按钮）→ 读模型选中供检视器展示。</summary>
        private void OnCanvasNodeClicked(ulong nodeId)
        {
            if (_algorithms == null)
            {
                return;
            }

            _selectedEdge = null;
            _algorithms.SelectNode(nodeId);
            RefreshDraftToolButtons(_algorithms.Snapshot);
        }

        /// <summary>「删除选中」：优先删除选中的边，其次选中的节点。</summary>
        private void OnDeleteSelectedClicked()
        {
            if (_algorithms == null)
            {
                return;
            }

            AlgorithmDomainSnapshot snapshot = _algorithms.Snapshot;
            if (!CanEditDraft(snapshot))
            {
                return;
            }

            ulong instanceId = snapshot.SelectedInstance.Value.Id;
            if (_selectedEdge.HasValue)
            {
                (ulong from, string output, ulong to, string input) = _selectedEdge.Value;
                // 先清本地状态再发命令：命令成功会经 service.Changed **同步重入** Render，
                // 那时 RefreshDraftToolButtons 必须已经看不到这条边的选择，否则按钮
                // 停留在启用态（重入发生在 _selectedEdge=null 之前的话就没人再刷新它）。
                _selectedEdge = null;
                _algorithms.Disconnect(instanceId, from, output, to, input);
                return;
            }

            if (snapshot.SelectedNode.HasValue)
            {
                ulong nodeId = snapshot.SelectedNode.Value.Id;
                _pendingConnection = null; // 同上：先清再发，避免重入渲染读到残留。
                _algorithms.DeleteNode(instanceId, nodeId);
            }
        }

        /// <summary>删除按钮可用性：域 Ready 且（选中边 或 选中节点仍在图中）。</summary>
        private void RefreshDraftToolButtons(AlgorithmDomainSnapshot snapshot)
        {
            if (_algorithmDeleteSelectedButton == null)
            {
                return;
            }

            bool nodeSelected = false;
            if (snapshot.SelectedNode.HasValue && snapshot.GraphNodes != null)
            {
                ulong id = snapshot.SelectedNode.Value.Id;
                for (int i = 0; i < snapshot.GraphNodes.Count; i++)
                {
                    if (snapshot.GraphNodes[i].Id == id)
                    {
                        nodeSelected = true;
                        break;
                    }
                }
            }

            bool hasSelection = _selectedEdge.HasValue || nodeSelected;
            _algorithmDeleteSelectedButton.interactable = CanEditDraft(snapshot) && hasSelection;
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
                RefreshActionAvailability(snapshot);
                return;
            }

            bool hasInstances = snapshot.InstanceCount > 0;
            bool hasGraph = snapshot.GraphNodeCount > 0;

            SetState(_algorithmEditorLoadingState, false);
            SetState(_algorithmEditorEmptyState, !hasInstances);
            SetState(_algorithmEditorErrorState, false);
            // Ready 表示数据已加载，不是运行成功；全屏状态卡片会遮住画布，Ready 时必须隐藏。
            SetState(_algorithmEditorSuccessState, false);
            SetState(_algorithmEditorDisabledState, false);

            // 节点栏＝节点库（规格语义）：种类目录 + 成本 + 分类；搜索按显示名过滤。
            // 图节点的选择与检视经画布节点按钮完成，不再混进节点栏（b20 权宜已移除）。
            RenderNodeLibrary(snapshot);

            // 画布：按布局坐标定位图节点与连线（节点含端口列表；拖拽回写 MoveNode）。
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
            SetState(_publicParametersSuccessState, false);
            SetState(_publicParametersDisabledState, false);
            RenderParameterRows(snapshot);
            RenderDetailRows(_publicParametersImpactTemplate, _publicParametersImpactContent, NoFields);
            SetText(_publicParametersParametersBody, hasInstances
                ? "选择公开参数行，修改数值或布尔值，再点击应用草稿。"
                : AlgorithmReadModels.NoInstanceReason);
            SetText(_publicParametersImpactBody, hasInstances
                ? "修改只影响草稿；应用时会进行统一校验。"
                : AlgorithmReadModels.NoInstanceReason);

            // 应用按钮随选中与域状态启用/禁用（DisableDomainActions 已把它关掉，这里按真实状态重开）。
            SetApplyButtonInteractable(snapshot);
            // 添加/删除按钮同理：DisableDomainActions 关掉后按真实状态重开。
            if (_algorithmEditorAddButton != null)
            {
                _algorithmEditorAddButton.interactable = CanEditDraft(snapshot) && _selectedKind.HasValue;
            }

            RefreshDraftToolButtons(snapshot);
            RefreshActionAvailability(snapshot);
            UpdateEditorTitle(snapshot);
        }

        private void UpdateEditorTitle(AlgorithmDomainSnapshot snapshot)
        {
            TMPro.TMP_Text title = FindChild(transform, "Txt_AlgorithmEditorTitle")?.GetComponent<TMPro.TMP_Text>();
            if (title != null)
            {
                string state = snapshot.State == UiDataState.Ready
                    ? "已加载 · 节点 " + snapshot.GraphNodeCount + " · 连线 " + (snapshot.GraphEdges == null ? 0 : snapshot.GraphEdges.Count)
                    : snapshot.State.ToString();
                title.SetText("算法编辑模式｜" + (_diagnosisMode ? "诊断" : "编辑") + " · " + state);
            }
            Debug.Log("[AutoEra][AlgorithmEditor] 快照：状态=" + snapshot.State
                + "，实例=" + snapshot.InstanceCount + "，节点=" + snapshot.GraphNodeCount
                + "，连线=" + (snapshot.GraphEdges == null ? 0 : snapshot.GraphEdges.Count));
        }

        private void RefreshActionAvailability(AlgorithmDomainSnapshot snapshot)
        {
            bool hasWorld = TryGetSession(out AutoEraUiSession session) && session.World != null;
            bool hasInstance = snapshot.State == UiDataState.Ready && snapshot.SelectedInstance.HasValue;
            if (_algorithmEditorTemplateButton != null) _algorithmEditorTemplateButton.interactable = hasWorld;
            if (_algorithmEditorBindButton != null) _algorithmEditorBindButton.interactable = hasInstance && !_diagnosisMode;
            if (_algorithmEditorValidateButton != null) _algorithmEditorValidateButton.interactable = hasInstance;
            if (_algorithmEditorDiagnoseButton != null) _algorithmEditorDiagnoseButton.interactable = hasInstance;
            Button parameterButton = FindChild(transform, "Btn_AlgorithmEditorEdit")?.GetComponent<Button>();
            if (parameterButton != null) parameterButton.interactable = hasInstance && !_diagnosisMode;
            if (_publicParametersChangeButton != null) _publicParametersChangeButton.interactable = hasInstance && !_diagnosisMode;
            if (_publicParametersDefaultButton != null) _publicParametersDefaultButton.interactable = hasInstance && !_diagnosisMode;
            if (_publicParametersApplyButton != null) _publicParametersApplyButton.interactable = hasInstance && !_diagnosisMode;
            if (_algorithmDiagnosisNextStepButton != null) _algorithmDiagnosisNextStepButton.interactable = hasInstance;
            if (_algorithmDiagnosisPreviousStepButton != null) _algorithmDiagnosisPreviousStepButton.interactable = hasInstance;
            if (_algorithmDiagnosisLocateButton != null) _algorithmDiagnosisLocateButton.interactable = hasInstance;
        }

        private void RenderParameterRows(AlgorithmDomainSnapshot snapshot)
        {
            if (_publicParametersParametersTemplate == null || _publicParametersParametersContent == null || snapshot.GraphNodes == null)
            {
                return;
            }

            var parameters = new List<UiAlgorithmNodeRow>();
            for (int i = 0; i < snapshot.GraphNodes.Count; i++)
            {
                UiAlgorithmNodeRow row = snapshot.GraphNodes[i];
                if (row.Label.StartsWith("Parameter ", System.StringComparison.OrdinalIgnoreCase)
                    || row.Label.StartsWith("参数 ", System.StringComparison.OrdinalIgnoreCase))
                {
                    parameters.Add(row);
                }
            }

            RenderListRows(_publicParametersParametersTemplate, _publicParametersParametersContent, parameters.Count,
                (index, item) => item.Bind(index, parameters[index].Label,
                    FormatParameterValue(parameters[index].Default), SelectParameterRow));
            SetText(_publicParametersParametersBody, parameters.Count == 0 ? "当前算法没有公开参数。" : "选择参数后可修改草稿值。");

            UiAlgorithmNodeRow? selected = snapshot.SelectedNode;
            if (selected.HasValue && selected.Value.Default != null)
            {
                AlgorithmValue value = selected.Value.Default;
                if (value.Type != null && value.Type.Kind == AlgorithmValueKind.Boolean)
                {
                    if (_publicParametersBooleanValue != null) _publicParametersBooleanValue.SetIsOnWithoutNotify(value.Boolean);
                }
                else if (_publicParametersNumberValue != null && value.Type != null && value.Type.Kind == AlgorithmValueKind.Number)
                {
                    _publicParametersNumberValue.SetTextWithoutNotify(value.Number.ToString("0.##"));
                }
            }
        }

        private void SelectParameterRow(int index)
        {
            if (_algorithms == null || !_algorithms.Snapshot.SelectedInstance.HasValue || _algorithms.Snapshot.GraphNodes == null) return;
            int seen = 0;
            for (int i = 0; i < _algorithms.Snapshot.GraphNodes.Count; i++)
            {
                UiAlgorithmNodeRow row = _algorithms.Snapshot.GraphNodes[i];
                if (!(row.Label.StartsWith("Parameter ", System.StringComparison.OrdinalIgnoreCase)
                    || row.Label.StartsWith("参数 ", System.StringComparison.OrdinalIgnoreCase))) continue;
                if (seen++ == index) { _algorithms.SelectNode(row.Id); return; }
            }
        }

        private static string FormatParameterValue(AlgorithmValue value)
        {
            if (value == null || value.Type == null) return "—";
            return value.Type.Kind == AlgorithmValueKind.Boolean ? (value.Boolean ? "真" : "假") : value.Number.ToString("0.##");
        }

        /// <summary>
        /// 画布渲染：节点与连线来自独立 Item 预制体，按布局坐标定位。
        /// </summary>
        private void RenderGraphCanvas(AlgorithmDomainSnapshot snapshot)
        {
            if (_algorithmGraphContent == null || _algorithmNodeItemPrefab == null)
            {
                return;
            }

            RectTransform viewport = _algorithmGraphContent.parent as RectTransform;
            Debug.Log("[AutoEra][AlgorithmEditor] 开始渲染画布：节点=" + snapshot.GraphNodeCount
                + "，连线=" + snapshot.GraphEdgeCount
                + "，Content尺寸=" + _algorithmGraphContent.rect.size
                + "，Viewport尺寸=" + (viewport != null ? viewport.rect.size.ToString() : "<null>"));

            RecycleGraphItems();

            if (snapshot.GraphNodes == null)
            {
                return;
            }

            var positions = new Dictionary<ulong, Vector2>(snapshot.GraphNodeCount);
            for (int i = 0; i < snapshot.GraphNodes.Count; i++)
            {
                UiAlgorithmNodeRow node = snapshot.GraphNodes[i];
                Vector2 modelPosition = new Vector2(node.LayoutX, node.LayoutY);
                // 节点位置只由模型坐标决定；没有位置数据时使用统一原点兜底，
                // 不在刷新阶段额外生成固定网格，也不覆盖任何节点的真实坐标。
                Vector2 displayPosition = modelPosition;
                positions[node.Id] = displayPosition;
                _graphPositions[node.Id] = displayPosition;

                AlgorithmNodeItemObject item = SpawnItem<AlgorithmNodeItemObject>(_algorithmNodeItemPrefab, _algorithmGraphContent);
                _nodeItems.Add(item);
                GameObject instance = item.gameObject;
                _nodeInstances[node.Id] = instance;
                RectTransform rect = instance.GetComponent<RectTransform>();
                if (rect != null)
                {
                    ConfigureGraphElementRect(rect);
                    rect.anchoredPosition = positions[node.Id];
                }

                item.Logic?.Bind(node.Id, node.Label,
                    snapshot.SelectedNode.HasValue && snapshot.SelectedNode.Value.Id == node.Id,
                    OnCanvasNodeClicked, OnGraphNodeMoved);
                AlgorithmGraphNodeDragHandler drag = instance.GetComponent<AlgorithmGraphNodeDragHandler>();
                if (drag != null)
                {
                    ulong id = node.Id;
                    drag.OnDragging = position => OnGraphNodeDragging(id, position);
                }
                RenderNodePorts(instance, node);

                if (i == 0)
                {
                    Transform nodeGroup = instance.transform.Find("Grp_AlgorithmNode");
                    Transform nameText = instance.transform.Find("Grp_AlgorithmNode/Btn_AlgorithmNodeSelect/Txt_AlgorithmNodeName");
                    Debug.Log("[AutoEra][AlgorithmEditor] 首个节点实例：active=" + instance.activeInHierarchy
                        + "，anchored=" + (rect != null ? rect.anchoredPosition.ToString() : "<null>")
                        + "，size=" + (rect != null ? rect.rect.size.ToString() : "<null>")
                        + "，节点组=" + (nodeGroup != null && nodeGroup.gameObject.activeInHierarchy)
                        + "，名称=" + (nameText != null ? nameText.GetComponent<TMPro.TMP_Text>()?.text : "<null>"));
                }
            }

            if (snapshot.GraphEdges == null)
            {
                return;
            }

            for (int i = 0; i < snapshot.GraphEdges.Count; i++)
            {
                UiAlgorithmEdgeRow edge = snapshot.GraphEdges[i];
                if (!positions.TryGetValue(edge.From, out Vector2 from) || !positions.TryGetValue(edge.To, out Vector2 to))
                {
                    continue;
                }

                Vector2 edgeCenter = (from + to) * 0.5f;
                GameObject edgeObject = new GameObject("AlgorithmEdge_" + edge.From + "_" + edge.To,
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(AlgorithmGraphEdgeGraphic));
                edgeObject.transform.SetParent(_algorithmGraphContent, false);
                RectTransform edgeRect = edgeObject.GetComponent<RectTransform>();
                ConfigureGraphElementRect(edgeRect);
                edgeRect.anchoredPosition = edgeCenter;
                AlgorithmGraphEdgeGraphic graphic = edgeObject.GetComponent<AlgorithmGraphEdgeGraphic>();
                _graphEdgeGraphics.Add(graphic);
                graphic.raycastTarget = false;
                graphic.Bind(edge.From, edge.Output, edge.To, edge.Input);
                bool selected = _selectedEdge.HasValue && _selectedEdge.Value.from == edge.From
                    && _selectedEdge.Value.output == edge.Output && _selectedEdge.Value.to == edge.To
                    && _selectedEdge.Value.input == edge.Input;
                graphic.SetPoints(ResolvePortPosition(edge.From, edge.Output, false) - edgeCenter,
                    ResolvePortPosition(edge.To, edge.Input, true) - edgeCenter, selected);
                GameObject edgeGroup = new GameObject("Grp_AlgorithmEdge", typeof(RectTransform));
                edgeGroup.transform.SetParent(edgeObject.transform, false);
                GameObject selectObject = new GameObject("Btn_AlgorithmEdgeSelect", typeof(RectTransform),
                    typeof(CanvasRenderer), typeof(Image), typeof(Button));
                selectObject.transform.SetParent(edgeGroup.transform, false);
                RectTransform selectRect = selectObject.GetComponent<RectTransform>();
                selectRect.anchorMin = new Vector2(0.5f, 0.5f);
                selectRect.anchorMax = new Vector2(0.5f, 0.5f);
                selectRect.sizeDelta = new Vector2(32f, 32f);
                selectRect.anchoredPosition = Vector2.zero;
                Image selectImage = selectObject.GetComponent<Image>();
                selectImage.color = new Color(1f, 1f, 1f, 0f);
                selectImage.raycastTarget = false;
                Button selectButton = selectObject.GetComponent<Button>();
                selectButton.targetGraphic = selectImage;
                ulong fromId = edge.From, toId = edge.To;
                string outputPort = edge.Output, inputPort = edge.Input;
                selectButton.onClick.AddListener(() => OnEdgeSelected(fromId, outputPort, toId, inputPort));
                edgeObject.transform.SetAsFirstSibling();
                Debug.Log("[AutoEra][AlgorithmEditor] 程序化边=" + edgeObject.name
                    + "，位置=" + edgeRect.anchoredPosition
                    + "，中点=" + edgeCenter
                    + "，组=" + (edgeObject.transform.Find("Grp_AlgorithmEdge") != null)
                    + "，按钮=" + (edgeObject.transform.Find("Grp_AlgorithmEdge/Btn_AlgorithmEdgeSelect") != null));
            }

            Debug.Log("[AutoEra][AlgorithmEditor] 画布实例汇总：子节点=" + _algorithmGraphContent.childCount);
            for (int i = 0; i < _algorithmGraphContent.childCount; i++)
            {
                Transform child = _algorithmGraphContent.GetChild(i);
                Transform nodeGroup = child.Find("Grp_AlgorithmNode");
                TMP_Text nodeName = child.Find("Grp_AlgorithmNode/Btn_AlgorithmNodeSelect/Txt_AlgorithmNodeName")?.GetComponent<TMP_Text>();
                Debug.Log("[AutoEra][AlgorithmEditor] 画布子节点[" + i + "]=" + child.name + "，active=" + child.gameObject.activeInHierarchy
                    + "，nodeGroup=" + (nodeGroup != null && nodeGroup.gameObject.activeSelf)
                    + "，name=" + (nodeName != null ? nodeName.text : "<null>"));
            }
        }

        private void RecycleGraphItems()
        {
            if (_algorithmNodeItemPrefab != null)
            {
                for (int i = 0; i < _nodeItems.Count; i++) UnspawnItem(_algorithmNodeItemPrefab, _nodeItems[i]);
            }
            if (_algorithmEdgeItemPrefab != null)
            {
                for (int i = 0; i < _edgeItems.Count; i++) UnspawnItem(_algorithmEdgeItemPrefab, _edgeItems[i]);
            }
            _nodeItems.Clear();
            _edgeItems.Clear();
            for (int i = 0; i < _graphEdgeGraphics.Count; i++)
            {
                if (_graphEdgeGraphics[i] != null)
                {
                    Destroy(_graphEdgeGraphics[i].gameObject);
                }
            }
            _graphEdgeGraphics.Clear();
            _nodeInstances.Clear();
            _portButtons.Clear();
            _graphPositions.Clear();
        }

        /// <summary>
        /// 图内容区以中心为坐标原点（4000×4000 内容用于滚动画布），
        /// 模板自身的左上锚点只适合静态布局，会把 (0,0) 节点放到内容区外。
        /// 动态实例统一改用中心锚点，模型 LayoutX/LayoutY 才能直接对应画布坐标。
        /// </summary>
        private static void ConfigureGraphElementRect(RectTransform rect)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        /// <summary>渲染节点元素上的输入/输出端口列表（模板行：端口名 + 连接状态）。</summary>
        private void RenderNodePorts(GameObject instance, UiAlgorithmNodeRow node)
        {
            RenderPortList(
                instance, "Grp_AlgorithmNode/List_AlgorithmInputPorts/Viewport_AlgorithmInputPorts/Content_AlgorithmInputPorts",
                "Item_AlgorithmInputPortTemplate", "Btn_AlgorithmInputPort", "Txt_AlgorithmInputPort",
                node.InputPorts, node.Id, isInput: true);
            RenderPortList(
                instance, "Grp_AlgorithmNode/List_AlgorithmOutputPorts/Viewport_AlgorithmOutputPorts/Content_AlgorithmOutputPorts",
                "Item_AlgorithmOutputPortTemplate", "Btn_AlgorithmOutputPort", "Txt_AlgorithmOutputPort",
                node.OutputPorts, node.Id, isInput: false);
        }

        private void RenderPortList(GameObject instance, string contentPath, string templateName,
            string buttonName, string textName, IReadOnlyList<UiAlgorithmPortRow> ports, ulong nodeId, bool isInput)
        {
            Transform contentNode = instance.transform.Find(contentPath);
            Transform template = contentNode != null ? contentNode.Find(templateName) : null;
            if (contentNode == null || template == null)
            {
                return;
            }

            // 清空旧行（保留模板自身）。
            for (int i = contentNode.childCount - 1; i >= 0; i--)
            {
                Transform child = contentNode.GetChild(i);
                if (child != template)
                {
                    Destroy(child.gameObject);
                }
            }

            if (ports == null)
            {
                return;
            }

            for (int i = 0; i < ports.Count; i++)
            {
                UiAlgorithmPortRow port = ports[i];
                GameObject row = Instantiate(template.gameObject, contentNode);
                row.SetActive(true);

                Transform textTransform = row.transform.Find(buttonName + "/" + textName);
                TMPro.TMP_Text label = textTransform != null ? textTransform.GetComponent<TMPro.TMP_Text>() : null;
                if (label != null)
                {
                    // 标记用 ASCII（*＝待连源、「已连」＝连接状态）：SIMHEI SDF 动态图集不含
                    // ▶/● 等几何符号，缺字会被渲染成方块。中文短语短于符号排版也不溢出。
                    bool pending = !isInput && _pendingConnection.HasValue
                        && _pendingConnection.Value.nodeId == nodeId && _pendingConnection.Value.port == port.Key;
                    label.SetText((pending ? "* " : string.Empty) + port.Label + (port.Connected ? " 已连" : string.Empty));
                }

                Transform buttonTransform = row.transform.Find(buttonName);
                Button button = buttonTransform != null ? buttonTransform.GetComponent<Button>() : null;
                if (button != null)
                {
                    _portButtons[(nodeId, port.Key, isInput)] = button.transform as RectTransform;
                    ulong id = nodeId;
                    string key = port.Key;
                    button.onClick.AddListener(isInput
                        ? (UnityEngine.Events.UnityAction)(() => OnInputPortClicked(id, key))
                        : () => OnOutputPortClicked(id, key));
                }
            }
        }

        /// <summary>节点选择按钮：点选 → SelectNode 供检视器展示。</summary>
        private void BindNodeSelectButton(GameObject instance, ulong nodeId, bool selected)
        {
            Transform buttonTransform = instance.transform.Find("Grp_AlgorithmNode/Btn_AlgorithmNodeSelect");
            Button button = buttonTransform != null ? buttonTransform.GetComponent<Button>() : null;
            if (button != null)
            {
                Image image = button.GetComponent<Image>();
                if (image != null) image.color = selected
                    ? new Color(0.95f, 0.67f, 0.24f, 1f)
                    : new Color(0.32f, 0.50f, 0.64f, 1f);
                button.onClick.AddListener(() => OnCanvasNodeClicked(nodeId));
            }
        }

        /// <summary>边选择按钮：点选记录完整边身份（「删除选中」按它断开）。</summary>
        private void BindEdgeSelectButton(GameObject instance, UiAlgorithmEdgeRow edge, bool selected)
        {
            Transform buttonTransform = instance.transform.Find("Grp_AlgorithmEdge/Btn_AlgorithmEdgeSelect");
            Button button = buttonTransform != null ? buttonTransform.GetComponent<Button>() : null;
            if (button != null)
            {
                Image image = button.GetComponent<Image>();
                if (image != null) image.color = selected
                    ? new Color(0.95f, 0.67f, 0.24f, 1f)
                    : new Color(0.32f, 0.50f, 0.64f, 1f);
                ulong from = edge.From, to = edge.To;
                string output = edge.Output, input = edge.Input;
                button.onClick.AddListener(() => OnEdgeSelected(from, output, to, input));
            }
        }

        /// <summary>画布节点拖拽松手 → 写回 MoveNode（新坐标）。</summary>
        private void OnGraphNodeMoved(ulong nodeId, Vector2 position)
        {
            if (_algorithms == null || !_algorithms.Snapshot.SelectedInstance.HasValue)
            {
                return;
            }

            ulong instanceId = _algorithms.Snapshot.SelectedInstance.Value.Id;
            bool moved = _algorithms.MoveNode(instanceId, nodeId, position.x, position.y);
            // 节点拖拽结束与服务 Changed 通知可能在同一帧交错；失败时刷新一次快照并重试，
            // 避免 Startup 等首个节点因短暂修订竞争立即回到旧坐标。
            if (!moved)
            {
                _algorithms.Refresh();
                moved = _algorithms.MoveNode(instanceId, nodeId, position.x, position.y);
            }
            Debug.Log("[AutoEra][AlgorithmEditor] 节点拖拽写回：实例=" + instanceId
                + "，节点=" + nodeId + "，位置=" + position + "，结果=" + moved);
        }

        private void OnGraphNodeDragging(ulong nodeId, Vector2 position)
        {
            _graphPositions[nodeId] = position;
            for (int i = 0; i < _graphEdgeGraphics.Count; i++)
            {
                RefreshProceduralEdge(_graphEdgeGraphics[i]);
            }
        }

        private void RefreshProceduralEdge(AlgorithmGraphEdgeGraphic graphic)
        {
            if (graphic == null) return;
            bool selected = _selectedEdge.HasValue && _selectedEdge.Value.from == graphic.FromNode
                && _selectedEdge.Value.output == graphic.OutputPort
                && _selectedEdge.Value.to == graphic.ToNode
                && _selectedEdge.Value.input == graphic.InputPort;
            Vector2 from = ResolvePortPosition(graphic.FromNode, graphic.OutputPort, false);
            Vector2 to = ResolvePortPosition(graphic.ToNode, graphic.InputPort, true);
            Vector2 center = (_graphPositions.TryGetValue(graphic.FromNode, out Vector2 fromNode)
                ? fromNode : from) + (_graphPositions.TryGetValue(graphic.ToNode, out Vector2 toNode)
                ? toNode : to);
            center *= 0.5f;
            RectTransform edgeRect = graphic.transform as RectTransform;
            if (edgeRect != null) edgeRect.anchoredPosition = center;
            graphic.SetPoints(from - center, to - center, selected);
        }

        private Vector2 ResolvePortPosition(ulong nodeId, string portName, bool input)
        {
            if (!_nodeInstances.TryGetValue(nodeId, out GameObject node) || node == null)
            {
                return _graphPositions.TryGetValue(nodeId, out Vector2 fallback) ? fallback : Vector2.zero;
            }

            if (_portButtons.TryGetValue((nodeId, portName, input), out RectTransform button) && button != null)
            {
                Vector3[] corners = new Vector3[4];
                button.GetWorldCorners(corners);
                Vector3 socket = input
                    ? (corners[0] + corners[1]) * 0.5f
                    : (corners[2] + corners[3]) * 0.5f;
                return _algorithmGraphContent.InverseTransformPoint(socket);
            }

            Vector2 center = _graphPositions.TryGetValue(nodeId, out Vector2 position) ? position : Vector2.zero;
            return center + (input ? new Vector2(-120f, 0f) : new Vector2(120f, 0f));
        }

        private Vector2? ResolveGraphPosition(ulong nodeId)
        {
            return _graphPositions.TryGetValue(nodeId, out Vector2 position) ? position : (Vector2?)null;
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

            if (_diagnosisMode || snapshot.State != UiDataState.Ready || !snapshot.SelectedInstance.HasValue)
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
