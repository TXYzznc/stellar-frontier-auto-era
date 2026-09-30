using System.Collections;
using AutoEra.Algorithms;
using AutoEra.Application;
using AutoEra.Machines;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AutoEra.Tests.PlayMode
{
    /// <summary>
    /// b33 的端到端验收：算法编辑器的**真实预制体**在 Play Mode 里完成
    /// 「节点库 → 添加 → 两步连线 → 选边删除 → 选节点删除」全链路。
    ///
    /// 交互全部经由真实 UI 元素驱动（按钮 onClick / 输入框 onValueChanged），
    /// 不反射私有方法——这正好把「字段没绑定＝按钮点了没反应」这类静默失灵
    /// 与「逻辑对但路径名对不上」这类结构错误同时钉住。
    /// 命令级规则（类型不兼容/输入占用/过期修订拒绝）已在 EditMode 覆盖，
    /// 这里只走 UI 能走通的路径与一条取消路径。
    /// </summary>
    public sealed class AlgorithmEditorCanvasPlayModeTests
    {
        private const string LaunchSceneName = "Launch";
        private const string RegionSceneName = "InitialRegion";

        private const string EditorRoot =
            "Panel_Frame/Grp_PageHost/Panel_PageAlgorithmEditor";
        private const string NodesContentPath =
            EditorRoot + "/Panel_AlgorithmEditorNodes/List_AlgorithmEditorNodes/Viewport_AlgorithmEditorNodes/Content_AlgorithmEditorNodes";
        private const string GraphContentPath =
            EditorRoot + "/Panel_AlgorithmEditorCanvas/List_AlgorithmGraph/Viewport_AlgorithmGraph/Content_AlgorithmGraph";
        private const string AddButtonPath = EditorRoot + "/Grp_AlgorithmEditorActions/Btn_AlgorithmEditorAdd";
        private const string DeleteButtonPath =
            EditorRoot + "/Panel_AlgorithmEditorToolbar/Grp_AlgorithmDraftTools/Btn_AlgorithmDeleteSelected";
        private const string SearchInputPath =
            NodesContentPath + "/Panel_AlgorithmEditorControls/Panel_AlgorithmEditorNodeSearch";

        [UnityTest]
        public IEnumerator EditorCanvas_LibraryAddConnectSelectDelete_EndToEnd()
        {
            yield return EnsureLaunchSceneLoaded();
            yield return WaitForRuntimeReady();
            yield return Verify();
        }

        private static IEnumerator Verify()
        {
            double until = Time.realtimeSinceStartupAsDouble + 30;
            while (!MachineCatalog.IsGameDataLoaded && Time.realtimeSinceStartupAsDouble < until) yield return null;
            Assert.That(MachineCatalog.IsGameDataLoaded, Is.True, "机器数据表必须在运行期就绪。");

            AsyncOperation loading = SceneManager.LoadSceneAsync(RegionSceneName, LoadSceneMode.Additive);
            Assert.That(loading, Is.Not.Null, "InitialRegion 必须在 Build Settings 里启用。");
            while (!loading.isDone) yield return null;
            Scene scene = SceneManager.GetSceneByName(RegionSceneName);
            Assert.That(scene.IsValid() && scene.isLoaded, Is.True);
            InitialRegionScene entry = null;
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.TryGetComponent(out InitialRegionScene found)) entry = found;
            Assert.That(entry, Is.Not.Null);
            var input = entry.GetComponent<AutoEra.Input.RegionInputModule>();
            Assert.That(input, Is.Not.Null);

            using (AutoEraApplicationContext context = new AutoEraApplicationCompositionRoot().Create())
            {
                int editorFormId = 0;
                bool editorOpen = false;
                try
                {
                    Assert.That(context.TryCreateWorldSession(0, out AutoEraWorldSession session), Is.True);
                    bool ready = false;
                    string failure = null;
                    entry.InitializeRuntime(session, () => ready = true, e => failure = e);
                    until = Time.realtimeSinceStartupAsDouble + 25;
                    while (!ready && failure == null && Time.realtimeSinceStartupAsDouble < until) yield return null;
                    Assert.That(failure, Is.Null);
                    Assert.That(ready, Is.True);

                    MachineCatalog catalog = MachineCatalog.FromLoadedGameData();
                    Assert.That(catalog.TryGetMachine(10011, out MachineDefinition wheeled), Is.True);
                    MachineInstance carrier = session.Machines.Create(wheeled);
                    ComponentInstance core = session.Machines.CreateComponent(
                        new ComponentDefinition(20011, HardwareKind.Core, 1, 0, 10, 10, false));
                    Assert.That(session.Machines.Install(carrier.Id, ManagementOrigin.Library, core.Id, 0),
                        Is.EqualTo(MachineManagementResult.Completed));

                    using (var flow = new MachineDeploymentFlow(session, entry.Region))
                    {
                        Assert.That(flow.TryBegin(carrier.Id, out string begin), Is.True, begin);
                        flow.Preview.Move(new Vector2(20, -25));
                        Assert.That(flow.TryCommit(out _, out string commit), Is.True, commit);
                    }

                    Assert.That(entry.TrySpawnMachine(carrier.Id, out string spawn), Is.True, spawn);
                    until = Time.realtimeSinceStartupAsDouble + 20;
                    RegionObjectView view = null;
                    while (view == null && Time.realtimeSinceStartupAsDouble < until)
                    {
                        view = entry.FindMachineView(carrier.Id);
                        if (view == null) yield return null;
                    }
                    Assert.That(view, Is.Not.Null, "机器实体必须生成并完成视图绑定。");
                    Assert.That(entry.MachineRuntimes.TryGet(carrier.Id, out RegionMachineRuntime runtime), Is.True,
                        "部署成功之后必须出现运行时。");
                    Assert.That(entry.Region.Select(carrier.Id, false), Is.True, "算法界面按稳定身份取机器。");

                    AutoEraUiSession uiSession = AutoEraUiSession.ForWorld(context, session, entry.Region, input,
                        entry.MachineRuntimes);
                    UIParams parameters = uiSession.WriteTo(UIParams.Create());
                    parameters.Set(AutoEraUiParamKeys.Request, new AutoEraUiPageRequest(AlgorithmEditorForm.PageEditor));
                    editorFormId = GF.UI.OpenUIForm(UIViews.AlgorithmEditorForm, parameters);
                    editorOpen = true;
                    yield return WaitForForm(editorFormId, expectedLoaded: true);
                    ScreenCapture.CaptureScreenshot("Assets/Screenshots/algorithm-editor-empty.png");

                    var editor = GF.UI.GetUIForm(editorFormId).Logic as AlgorithmEditorForm;
                    Assert.That(editor, Is.Not.Null);
                    Assert.That(editor.AlgorithmDataState, Is.EqualTo(UiDataState.Empty),
                        "运行时在、还没有实例时是 Empty。");

                    AlgorithmPlan plan = CompileFixturePlan(runtime);
                    var instance = new AlgorithmRuntime(new PersistentId(700), plan, runtime.Context.Compute, runtime.Adapter);
                    Assert.That(runtime.Instances.Add(instance), Is.True);
                    yield return null;
                    Assert.That(editor.AlgorithmDataState, Is.EqualTo(UiDataState.Ready),
                        "加入实例之后必须变成 Ready（订阅而非轮询）。");
                    ScreenCapture.CaptureScreenshot("Assets/Screenshots/algorithm-editor-ready.png");

                    Transform root = editor.transform;

                    // ── ① 节点库：目录行数＝枚举种类数，未选实例内容不可点（canAdd=false）。──
                    Transform nodesContent = root.Find(NodesContentPath);
                    Assert.That(nodesContent, Is.Not.Null, "节点库内容必须存在。");
                    yield return null;
                    Assert.That(CountLibraryRows(nodesContent), Is.EqualTo(AlgorithmNodeLibrary.Rows.Length),
                        "节点库必须列出全部种类。");

                    // ── ② 搜索过滤：常量 → 1 行；清空 → 全量。──
                    TMP_InputField search = root.Find(SearchInputPath).GetComponent<TMP_InputField>();
                    Assert.That(search, Is.Not.Null, "搜索框必须接线。");
                    search.text = "常量";
                    yield return null;
                    Assert.That(CountLibraryRows(nodesContent), Is.EqualTo(1), "搜索「常量」必须只剩一行。");
                    search.text = string.Empty;
                    yield return null;
                    Assert.That(CountLibraryRows(nodesContent), Is.EqualTo(AlgorithmNodeLibrary.Rows.Length),
                        "清空搜索必须恢复全量目录。");

                    Button deleteButton = root.Find(DeleteButtonPath).GetComponent<Button>();
                    Assert.That(deleteButton.interactable, Is.False, "没有任何选择时删除按钮必须禁用。");

                    // ── ③ 添加节点：点库行只选择种类，再点「添加节点」按钮创建。──
                    Assert.That(ClickLibraryRow(nodesContent, "常量"), Is.True, "必须找到「常量」库行。");
                    yield return null;
                    AlgorithmDocument draft = runtime.Instances.ReadDraft(700);
                    int nodesAfterFirst = draft.Nodes.Count;
                    Assert.That(nodesAfterFirst, Is.EqualTo(4), "选择节点种类不应直接修改草稿。");

                    Button addButton = root.Find(AddButtonPath).GetComponent<Button>();
                    Assert.That(addButton, Is.Not.Null, "添加按钮必须接线。");
                    Assert.That(addButton.interactable, Is.True, "选中种类之后添加按钮必须可用。");
                    addButton.onClick.Invoke();
                    yield return null;
                    draft = runtime.Instances.ReadDraft(700);
                    Assert.That(draft.Nodes.Count, Is.EqualTo(5), "添加按钮必须按选中种类创建一个常量。");
                    addButton.onClick.Invoke();
                    yield return null;
                    draft = runtime.Instances.ReadDraft(700);
                    Assert.That(draft.Nodes.Count, Is.EqualTo(6), "再次点击添加应创建第二个常量。");

                    // 常量 A（第 5 个节点）与常量 B（第 6 个节点）。
                    ulong constantA = draft.Nodes[4].Id;
                    ulong constantB = draft.Nodes[5].Id;

                    // ── ④ 再添加一个设置变量节点，作为连线目标。──
                    Assert.That(ClickLibraryRow(nodesContent, "设置变量"), Is.True, "必须找到「设置变量」库行。");
                    yield return null;
                    Assert.That(addButton.interactable, Is.True, "选择设置变量之后添加按钮必须可用。");
                    addButton.onClick.Invoke();
                    yield return null;
                    draft = runtime.Instances.ReadDraft(700);
                    Assert.That(draft.Nodes.Count, Is.EqualTo(7), "设置变量节点必须创建。");
                    ulong setVar = draft.Nodes[6].Id;

                    Transform graphContent = root.Find(GraphContentPath);
                    Assert.That(graphContent, Is.Not.Null, "画布内容必须存在。");
                    yield return null;
                    Button firstNodeButton = FindNodeElement(graphContent, "Constant #" + constantA)
                        ?.Find("Grp_AlgorithmNode/Btn_AlgorithmNodeSelect")?.GetComponent<Button>();
                    Assert.That(firstNodeButton, Is.Not.Null, "新增节点必须出现在画布层级中。");
                    RectTransform firstNodeRect = firstNodeButton.transform.parent.parent.GetComponent<RectTransform>();
                    Assert.That(firstNodeRect, Is.Not.Null);
                    Assert.That(firstNodeRect.anchorMin, Is.EqualTo(new Vector2(0.5f, 0.5f)),
                        "动态节点必须使用画布中心锚点，否则模型坐标会落到 4000x4000 Content 外。");
                    Assert.That(firstNodeRect.anchorMax, Is.EqualTo(new Vector2(0.5f, 0.5f)),
                        "动态节点必须使用画布中心锚点，否则模型坐标会落到 4000x4000 Content 外。");

                    // ── ⑤ 两步连线·取消路径：点输出端口两次＝取消，再点输入不产生边。──
                    // 端口行点击会局部重建该节点的端口行，每次点击前都必须重新查找按钮引用。
                    Button outputA = FindOutputPortButton(graphContent, constantA);
                    Assert.That(outputA, Is.Not.Null, "常量 A 必须有输出端口按钮。");
                    outputA.onClick.Invoke(); // 待连
                    yield return null;
                    outputA = FindOutputPortButton(graphContent, constantA);
                    Assert.That(outputA, Is.Not.Null, "待连刷新后常量 A 的输出端口按钮必须仍在。");
                    outputA.onClick.Invoke(); // 取消
                    yield return null;
                    Button valueInput = FindInputPortButton(graphContent, setVar, "value");
                    Assert.That(valueInput, Is.Not.Null, "设置变量必须有 value 输入端口。");
                    valueInput.onClick.Invoke(); // 无待连源：不应产生边
                    yield return null;
                    draft = runtime.Instances.ReadDraft(700);
                    Assert.That(draft.Edges.Find(e => e.To == setVar && e.Input == "value"), Is.Null,
                        "取消待连之后再点输入端口不得产生边。");

                    // ── ⑥ 两步连线·成功路径：常量 A value → 设置变量 value。──
                    outputA = FindOutputPortButton(graphContent, constantA);
                    Assert.That(outputA, Is.Not.Null);
                    outputA.onClick.Invoke(); // 待连
                    yield return null;
                    valueInput = FindInputPortButton(graphContent, setVar, "value");
                    Assert.That(valueInput, Is.Not.Null, "待连状态下设置变量的 value 输入端口必须可点。");
                    valueInput.onClick.Invoke(); // 连接
                    yield return null;
                    draft = runtime.Instances.ReadDraft(700);
                    Assert.That(draft.Edges.Find(e => e.From == constantA && e.To == setVar && e.Input == "value"),
                        Is.Not.Null, "两步连线必须在草稿里产生边。");

                    // ── ⑦ 输入占用拒绝：常量 B → 同一输入（EditMode 已覆盖规则，这里验证 UI 路径不崩溃）。──
                    // ⑥ 的连线成功触发了重渲染（旧元素已销毁重建），所有按钮引用必须重新查找。
                    Button outputB = FindOutputPortButton(graphContent, constantB);
                    Assert.That(outputB, Is.Not.Null, "常量 B 必须有输出端口按钮。");
                    Button valueInputAfterConnect = FindInputPortButton(graphContent, setVar, "value");
                    Assert.That(valueInputAfterConnect, Is.Not.Null, "连线后设置变量的 value 输入端口必须仍在。");
                    outputB.onClick.Invoke();
                    yield return null;
                    valueInputAfterConnect.onClick.Invoke();
                    yield return null;
                    draft = runtime.Instances.ReadDraft(700);
                    int edgesToSetVar = draft.Edges.FindAll(e => e.To == setVar && e.Input == "value").Count;
                    Assert.That(edgesToSetVar, Is.EqualTo(1), "已占用输入必须保持只有一条边。");

                    // ── ⑧ 选边 → 删除按钮可用 → 删除选中 → 边消失。──
                    Button edgeSelect = FindEdgeSelectButton(graphContent, constantA, setVar);
                    Assert.That(edgeSelect, Is.Not.Null, "画布必须渲染边元素。");
                    Assert.That(deleteButton.interactable, Is.False, "选边之前删除按钮必须禁用。");
                    edgeSelect.onClick.Invoke();
                    yield return null;
                    Assert.That(deleteButton.interactable, Is.True, "选中边之后删除按钮必须可用。");
                    deleteButton.onClick.Invoke();
                    yield return null;
                    draft = runtime.Instances.ReadDraft(700);
                    Assert.That(draft.Edges.Find(e => e.From == constantA && e.To == setVar && e.Input == "value"),
                        Is.Null, "删除选中必须断开这条边。");
                    Assert.That(deleteButton.interactable, Is.False, "边删掉之后删除按钮必须回到禁用。");

                    // ── ⑨ 选节点 → 删除按钮可用 → 删除选中 → 节点软删。──
                    Button nodeSelect = FindNodeSelectButton(graphContent, setVar);
                    Assert.That(nodeSelect, Is.Not.Null, "设置变量节点必须有选择按钮。");
                    nodeSelect.onClick.Invoke();
                    yield return null;
                    Assert.That(deleteButton.interactable, Is.True, "选中节点之后删除按钮必须可用。");
                    deleteButton.onClick.Invoke();
                    yield return null;
                    draft = runtime.Instances.ReadDraft(700);
                    Assert.That(draft.Nodes.Find(n => n.Id == setVar).Deleted, Is.True, "删除选中必须软删节点。");
                    Assert.That(draft.Edges.Find(e => e.From == setVar || e.To == setVar), Is.Null,
                        "节点删除必须级联移除关联边。");
                    Assert.That(deleteButton.interactable, Is.False, "选中的节点被删掉之后删除按钮必须禁用。");

                    // ── ⑩ 验证、应用、诊断与返回编辑：四个入口必须沿真实 UI 接线。──
                    Button validateButton = root.Find(EditorRoot + "/Grp_AlgorithmEditorActions/Btn_AlgorithmEditorValidate")
                        .GetComponent<Button>();
                    Assert.That(validateButton, Is.Not.Null, "验证按钮必须存在。");
                    Assert.That(validateButton.interactable, Is.True, "Ready 状态下验证按钮必须可用。");
                    validateButton.onClick.Invoke();
                    yield return null;

                    Button applyButton = root.Find(EditorRoot + "/Grp_AlgorithmEditorActions/Btn_AlgorithmEditorApply")
                        .GetComponent<Button>();
                    Assert.That(applyButton, Is.Not.Null, "应用草稿按钮必须存在。");
                    if (applyButton.interactable)
                    {
                        applyButton.onClick.Invoke();
                        yield return null;
                    }

                    Button diagnoseButton = root.Find(EditorRoot + "/Grp_AlgorithmEditorActions/Btn_AlgorithmEditorDiagnose")
                        .GetComponent<Button>();
                    Assert.That(diagnoseButton, Is.Not.Null, "诊断入口必须存在。");
                    diagnoseButton.onClick.Invoke();
                    yield return null;
                    Assert.That(root.Find(EditorRoot + "/Grp_AlgorithmDiagnosisActions").gameObject.activeSelf,
                        Is.True, "切换诊断后诊断工具条必须显示。");

                    Button returnEditButton = root.Find(
                        EditorRoot + "/Grp_AlgorithmDiagnosisActions/Btn_AlgorithmDiagnosisReturnEdit").GetComponent<Button>();
                    Assert.That(returnEditButton, Is.Not.Null, "返回编辑按钮必须存在。");
                    returnEditButton.onClick.Invoke();
                    yield return null;
                    Assert.That(root.Find(EditorRoot + "/Grp_AlgorithmEditorActions").gameObject.activeSelf,
                        Is.True, "返回编辑后编辑工具条必须恢复。");
                }
                finally
                {
                    if (editorOpen && GF.UI != null && GF.UI.HasUIForm(editorFormId)) GF.UI.CloseUIForm(editorFormId);
                    entry.Release();
                    context.ReleaseActiveWorldSession();
                }
            }

            yield return SceneManager.UnloadSceneAsync(scene);
        }

        private static int CountLibraryRows(Transform content)
        {
            int count = 0;
            for (int i = 0; i < content.childCount; i++)
            {
                Transform child = content.GetChild(i);
                // 模板自身也在 content 下（名字不含 "(Clone)"）且处于非激活态，必须排除。
                if (child.name != "Item_AlgorithmEditorNodesTemplate"
                    && child.name.Contains("Item_AlgorithmEditorNodesTemplate")
                    && child.gameObject.activeSelf)
                {
                    count++;
                }
            }

            return count;
        }

        private static bool ClickLibraryRow(Transform content, string label)
        {
            for (int i = 0; i < content.childCount; i++)
            {
                Transform row = content.GetChild(i);
                if (!row.name.Contains("Item_AlgorithmEditorNodesTemplate"))
                {
                    continue;
                }

                TMP_Text[] texts = row.GetComponentsInChildren<TMP_Text>(true);
                for (int t = 0; t < texts.Length; t++)
                {
                    if (texts[t].name.EndsWith("RowLabel") && texts[t].text == label)
                    {
                        Button button = row.GetComponentInChildren<Button>(true);
                        Assert.That(button, Is.Not.Null, "库行必须有按钮。");
                        Assert.That(button.interactable, Is.True, "Ready 状态下的库行必须可点。");
                        button.onClick.Invoke();
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>按节点名文本（如 "Constant #5"）在画布里定位节点元素。</summary>
        private static Transform FindNodeElement(Transform graphContent, string nodeName)
        {
            for (int i = 0; i < graphContent.childCount; i++)
            {
                Transform element = graphContent.GetChild(i);
                if (element.name == "Item_AlgorithmGraphElementTemplate" || !element.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Transform nodeGroup = element.Find("Grp_AlgorithmNode");
                if (nodeGroup == null || !nodeGroup.gameObject.activeSelf)
                {
                    continue;
                }

                TMP_Text name = nodeGroup.Find("Btn_AlgorithmNodeSelect/Txt_AlgorithmNodeName")?.GetComponent<TMP_Text>();
                if (name != null && name.text == nodeName)
                {
                    return element;
                }
            }

            return null;
        }

        private static Button FindOutputPortButton(Transform graphContent, ulong nodeId)
        {
            Transform element = FindNodeElement(graphContent, "Constant #" + nodeId);
            Assert.That(element, Is.Not.Null, "画布必须渲染常量 #" + nodeId + " 的节点元素。");
            Transform outputContent = element.Find(
                "Grp_AlgorithmNode/List_AlgorithmOutputPorts/Viewport_AlgorithmOutputPorts/Content_AlgorithmOutputPorts");
            Assert.That(outputContent, Is.Not.Null, "节点元素必须有输出端口列表。");
            for (int i = 0; i < outputContent.childCount; i++)
            {
                Transform row = outputContent.GetChild(i);
                if (row.name != "Item_AlgorithmOutputPortTemplate"
                    && row.name.Contains("Item_AlgorithmOutputPortTemplate") && row.gameObject.activeSelf)
                {
                    return row.Find("Btn_AlgorithmOutputPort").GetComponent<Button>();
                }
            }

            return null;
        }

        private static Button FindInputPortButton(Transform graphContent, ulong nodeId, string portKey)
        {
            Transform element = FindNodeElement(graphContent, "SetVariable #" + nodeId);
            Assert.That(element, Is.Not.Null, "画布必须渲染设置变量 #" + nodeId + " 的节点元素。");
            Transform inputContent = element.Find(
                "Grp_AlgorithmNode/List_AlgorithmInputPorts/Viewport_AlgorithmInputPorts/Content_AlgorithmInputPorts");
            Assert.That(inputContent, Is.Not.Null, "节点元素必须有输入端口列表。");
            for (int i = 0; i < inputContent.childCount; i++)
            {
                Transform row = inputContent.GetChild(i);
                if (row.name == "Item_AlgorithmInputPortTemplate"
                    || !row.name.Contains("Item_AlgorithmInputPortTemplate") || !row.gameObject.activeSelf)
                {
                    continue;
                }

                TMP_Text label = row.Find("Btn_AlgorithmInputPort/Txt_AlgorithmInputPort").GetComponent<TMP_Text>();
                if (label != null && label.text.StartsWith(portKey))
                {
                    return row.Find("Btn_AlgorithmInputPort").GetComponent<Button>();
                }
            }

            return null;
        }

        /// <summary>按两端节点的中点定位特定边的元素（边元素渲染在两端坐标中点）。</summary>
        private static Button FindEdgeSelectButton(Transform graphContent, ulong fromId, ulong toId)
        {
            Transform from = FindNodeElement(graphContent, "Constant #" + fromId);
            Transform to = FindNodeElement(graphContent, "SetVariable #" + toId);
            Assert.That(from, Is.Not.Null, "画布必须渲染常量 #" + fromId + "。");
            Assert.That(to, Is.Not.Null, "画布必须渲染设置变量 #" + toId + "。");
            Vector2 mid = (from.GetComponent<RectTransform>().anchoredPosition
                + to.GetComponent<RectTransform>().anchoredPosition) * 0.5f;

            for (int i = 0; i < graphContent.childCount; i++)
            {
                Transform element = graphContent.GetChild(i);
                if (element.name == "Item_AlgorithmGraphElementTemplate" || !element.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Transform edgeGroup = element.Find("Grp_AlgorithmEdge");
                if (edgeGroup != null && edgeGroup.gameObject.activeSelf
                    && Vector2.Distance(element.GetComponent<RectTransform>().anchoredPosition, mid) < 1f)
                {
                    return edgeGroup.Find("Btn_AlgorithmEdgeSelect").GetComponent<Button>();
                }
            }

            return null;
        }

        private static Button FindNodeSelectButton(Transform graphContent, ulong nodeId)
        {
            Transform element = FindNodeElement(graphContent, "SetVariable #" + nodeId);
            Assert.That(element, Is.Not.Null, "画布必须渲染设置变量节点元素。");
            return element.Find("Grp_AlgorithmNode/Btn_AlgorithmNodeSelect").GetComponent<Button>();
        }

        private static AlgorithmPlan CompileFixturePlan(RegionMachineRuntime runtime)
        {
            var graph = new AlgorithmDocument { DocumentId = 700 };
            // 夹具必须提供真实画布坐标：正常验收场景是在已有布局的算法上拖动节点，
            // 不应使用“所有节点都没有坐标”的新图状态来验证拖拽行为。
            graph.Nodes.Add(new AlgorithmNode { Id = 1, Kind = AlgorithmNodeKind.Startup, LayoutX = -420f, LayoutY = 120f });
            graph.Nodes.Add(new AlgorithmNode { Id = 2, Kind = AlgorithmNodeKind.Constant, Default = AlgorithmValue.Numeric(12), LayoutX = -420f, LayoutY = -160f });
            graph.Nodes.Add(new AlgorithmNode { Id = 3, Kind = AlgorithmNodeKind.SetVariable, StateKey = "counter", LayoutX = 0f, LayoutY = 0f });
            graph.Nodes.Add(new AlgorithmNode { Id = 4, Kind = AlgorithmNodeKind.Log, LayoutX = 360f, LayoutY = 120f });
            graph.Edges.Add(new AlgorithmEdge { From = 1, To = 3, Output = "event", Input = "event" });
            graph.Edges.Add(new AlgorithmEdge { From = 2, To = 3, Input = "value" });
            graph.Edges.Add(new AlgorithmEdge { From = 1, To = 4, Output = "event", Input = "event" });
            Assert.That(AlgorithmValidator.TryCompile(graph, runtime.Context.Compute.LogicCapacity,
                out AlgorithmPlan plan, out _), Is.True, "夹具图必须能在本机的逻辑算力预算内编译。");
            return plan;
        }

        private static IEnumerator EnsureLaunchSceneLoaded()
        {
            if (SceneManager.GetActiveScene().name == LaunchSceneName)
            {
                yield break;
            }

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(LaunchSceneName, LoadSceneMode.Single);
            Assert.That(loadOperation, Is.Not.Null, "Launch 必须在 Build Settings 里启用。");
            yield return loadOperation;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(LaunchSceneName));
            yield return null;
        }

        private static IEnumerator WaitForRuntimeReady()
        {
            const int maxFrames = 600;
            for (int frame = 0; frame < maxFrames; frame++)
            {
                if (GF.UI != null && GF.DataTable != null &&
                    GF.DataTable.HasDataTable<UITable>() &&
                    GF.DataTable.HasDataTable<UIGroupTable>() &&
                    GF.UI.HasUIGroup("Default"))
                {
                    yield break;
                }

                yield return null;
            }

            Assert.Fail("GF UI runtime or required UI data tables did not become ready within 600 frames.");
        }

        private static IEnumerator WaitForForm(int serialId, bool expectedLoaded)
        {
            const int maxFrames = 300;
            for (int frame = 0; frame < maxFrames; frame++)
            {
                if (GF.UI.HasUIForm(serialId) == expectedLoaded)
                {
                    yield break;
                }

                yield return null;
            }

            Assert.Fail($"UI form serial {serialId} did not reach loaded={expectedLoaded} within 300 frames.");
        }
    }
}
