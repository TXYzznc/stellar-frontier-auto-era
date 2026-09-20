# 13-算法工作台 — prefab-layout

状态：2026-09-19完整设计候选，待用户集中验收；未据此修改Prefab。

继承[通用合同](../00-通用合同.md)和[共享外壳](../00-共享外壳-prefab-layout.md)。下列子树预制在所属Form的Grp_PageHost中；公共外壳由共享文档唯一声明。跨家族同属一个Form的子树合并，禁止重复根节点。表中所有Image／文本颜色、资源、射线、默认字号与状态继承通用合同，列出的数据是字段说明，不是示例业务数值。

## AlgorithmEditor：算法编辑模式

功能文档：[算法编辑模式](AlgorithmEditor.md)；归属 `AlgorithmEditorForm`；内容 1728×830。

```text
Panel_PageAlgorithmEditor [Image]
  Txt_AlgorithmEditorTitle [TextMeshProUGUI]
  Grp_AlgorithmEditorActions [GridLayoutGroup fixedColumns=9 cell=(183,48) spacing=(8,8)]
    Btn_AlgorithmEditorAdd [Button + Image]
      Txt_AlgorithmEditorAddLabel [TextMeshProUGUI]
    Btn_AlgorithmEditorEdit [Button + Image]
      Txt_AlgorithmEditorEditLabel [TextMeshProUGUI]
    Btn_AlgorithmEditorBind [Button + Image]
      Txt_AlgorithmEditorBindLabel [TextMeshProUGUI]
    Btn_AlgorithmEditorValidate [Button + Image]
      Txt_AlgorithmEditorValidateLabel [TextMeshProUGUI]
    Btn_AlgorithmEditorApply [Button + Image]
      Txt_AlgorithmEditorApplyLabel [TextMeshProUGUI]
    Btn_AlgorithmEditorDiagnose [Button + Image]
      Txt_AlgorithmEditorDiagnoseLabel [TextMeshProUGUI]
    Btn_AlgorithmEditorTemplate [Button + Image]
      Txt_AlgorithmEditorTemplateLabel [TextMeshProUGUI]
  Panel_AlgorithmEditorNodes [Image]
    Txt_AlgorithmEditorNodesHeading [TextMeshProUGUI]
    List_AlgorithmEditorNodes [ScrollRect vertical=true horizontal=false]
      Viewport_AlgorithmEditorNodes [RectMask2D]
        Content_AlgorithmEditorNodes [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_AlgorithmEditorNodesBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_AlgorithmEditorNodesTemplate [LayoutElement + Image；默认inactive]
            Btn_AlgorithmEditorNodesRow [Button + Image]
              Txt_AlgorithmEditorNodesRowLabel [TextMeshProUGUI]
              Txt_AlgorithmEditorNodesRowValue [TextMeshProUGUI]
          Panel_AlgorithmEditorControls [Image + LayoutElement]
            Panel_AlgorithmEditorNodeSearch [Image + TMP_InputField]
              Grp_AlgorithmEditorNodeSearchTextViewport [RectMask2D]
                Txt_AlgorithmEditorNodeSearchValue [TextMeshProUGUI]
                Txt_AlgorithmEditorNodeSearchPlaceholder [TextMeshProUGUI]
  Panel_AlgorithmEditorCanvas [Image]
    Txt_AlgorithmEditorCanvasHeading [TextMeshProUGUI]
    List_AlgorithmGraph [ScrollRect horizontal=true vertical=true]
      Viewport_AlgorithmGraph [RectMask2D]
        Content_AlgorithmGraph [GraphLayoutGroup : LayoutGroup（待实现）；不挂ContentSizeFitter]
          Item_AlgorithmGraphElementTemplate [LayoutElement + Image；默认inactive]
            Grp_AlgorithmNode [无Graphic]
              Btn_AlgorithmNodeSelect [Button + Image]
                Txt_AlgorithmNodeName [TextMeshProUGUI]
              List_AlgorithmInputPorts [ScrollRect vertical=true]
                Viewport_AlgorithmInputPorts [RectMask2D]
                  Content_AlgorithmInputPorts [VerticalLayoutGroup spacing=4 + ContentSizeFitter vertical=Preferred]
                    Item_AlgorithmInputPortTemplate [LayoutElement + Image；默认inactive]
                      Btn_AlgorithmInputPort [Button + Image]
                        Txt_AlgorithmInputPort [TextMeshProUGUI]
              List_AlgorithmOutputPorts [ScrollRect vertical=true]
                Viewport_AlgorithmOutputPorts [RectMask2D]
                  Content_AlgorithmOutputPorts [VerticalLayoutGroup spacing=4 + ContentSizeFitter vertical=Preferred]
                    Item_AlgorithmOutputPortTemplate [LayoutElement + Image；默认inactive]
                      Btn_AlgorithmOutputPort [Button + Image]
                        Txt_AlgorithmOutputPort [TextMeshProUGUI]
            Grp_AlgorithmEdge [无Graphic]
              Img_AlgorithmEdge [Image + 连线表现组件]
              Btn_AlgorithmEdgeSelect [Button + Image]
  Panel_AlgorithmEditorInspector [Image]
    Txt_AlgorithmEditorInspectorHeading [TextMeshProUGUI]
    List_AlgorithmEditorInspector [ScrollRect vertical=true horizontal=false]
      Viewport_AlgorithmEditorInspector [RectMask2D]
        Content_AlgorithmEditorInspector [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_AlgorithmEditorInspectorBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_AlgorithmEditorInspectorTemplate [LayoutElement + Image；默认inactive]
            Btn_AlgorithmEditorInspectorRow [Button + Image]
              Txt_AlgorithmEditorInspectorRowLabel [TextMeshProUGUI]
              Txt_AlgorithmEditorInspectorRowValue [TextMeshProUGUI]
  Panel_AlgorithmEditorProblems [Image]
    Txt_AlgorithmEditorProblemsHeading [TextMeshProUGUI]
    List_AlgorithmEditorProblems [ScrollRect vertical=true horizontal=false]
      Viewport_AlgorithmEditorProblems [RectMask2D]
        Content_AlgorithmEditorProblems [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_AlgorithmEditorProblemsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_AlgorithmEditorProblemsTemplate [LayoutElement + Image；默认inactive]
            Btn_AlgorithmEditorProblemsRow [Button + Image]
              Txt_AlgorithmEditorProblemsRowLabel [TextMeshProUGUI]
              Txt_AlgorithmEditorProblemsRowValue [TextMeshProUGUI]
  Panel_AlgorithmEditorToolbar [Image]
    Txt_AlgorithmEditorToolbarHeading [TextMeshProUGUI]
    Txt_AlgorithmToolbarStatus [TextMeshProUGUI]
  Grp_AlgorithmEditorLoadingState [无Graphic]
    Panel_AlgorithmEditorLoadingMessage [Image]
      Txt_AlgorithmEditorLoadingMessage [TextMeshProUGUI]
  Grp_AlgorithmEditorEmptyState [无Graphic]
    Panel_AlgorithmEditorEmptyMessage [Image]
      Txt_AlgorithmEditorEmptyMessage [TextMeshProUGUI]
  Grp_AlgorithmEditorErrorState [无Graphic]
    Panel_AlgorithmEditorErrorMessage [Image]
      Txt_AlgorithmEditorErrorMessage [TextMeshProUGUI]
  Grp_AlgorithmEditorSuccessState [无Graphic]
    Panel_AlgorithmEditorSuccessMessage [Image]
      Txt_AlgorithmEditorSuccessMessage [TextMeshProUGUI]
  Grp_AlgorithmEditorDisabledState [无Graphic]
    Panel_AlgorithmEditorDisabledMessage [Image]
      Txt_AlgorithmEditorDisabledMessage [TextMeshProUGUI]
  Grp_AlgorithmDiagnosisActions [HorizontalLayoutGroup spacing=8]
    Btn_AlgorithmDiagnosisPreviousStep [Button + Image + LayoutElement preferredWidth=200 preferredHeight=48]
      Txt_AlgorithmDiagnosisPreviousStepLabel [TextMeshProUGUI]
    Btn_AlgorithmDiagnosisNextStep [Button + Image + LayoutElement preferredWidth=200 preferredHeight=48]
      Txt_AlgorithmDiagnosisNextStepLabel [TextMeshProUGUI]
    Btn_AlgorithmDiagnosisLocate [Button + Image + LayoutElement preferredWidth=200 preferredHeight=48]
      Txt_AlgorithmDiagnosisLocateLabel [TextMeshProUGUI]
    Btn_AlgorithmDiagnosisReturnEdit [Button + Image + LayoutElement preferredWidth=200 preferredHeight=48]
      Txt_AlgorithmDiagnosisReturnEditLabel [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageAlgorithmEditor | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；算法编辑模式；内部页面根 |
| Txt_AlgorithmEditorTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1712,32); pos(8,0) | absolute | TextMeshProUGUI；算法编辑模式 |
| Grp_AlgorithmEditorActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=9 cell=(183,48) spacing=(8,8)；操作区 |
| Btn_AlgorithmEditorAdd | min(0,1) max(0,1); pivot(0,1); sizeDelta(183,48)初始化; pos(0,0)初始化; LayoutElement preferred(183,48); 最终位置/尺寸由组驱动 | group | Button + Image；从节点库加入；拖线仅兼容端口高亮；错误说明到类型／单位／能力 |
| Txt_AlgorithmEditorAddLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；添加或连接节点 |
| Btn_AlgorithmEditorEdit | min(0,1) max(0,1); pivot(0,1); sizeDelta(183,48)初始化; pos(0,0)初始化; LayoutElement preferred(183,48); 最终位置/尺寸由组驱动 | group | Button + Image；只修改草稿；移动、删除、连线操作可撤销／重做属于本次编辑交互建议 |
| Txt_AlgorithmEditorEditLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；修改节点和参数 |
| Btn_AlgorithmEditorBind | min(0,1) max(0,1); pivot(0,1); sizeDelta(183,48)初始化; pos(0,0)初始化; LayoutElement preferred(183,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入12对应选择器 |
| Txt_AlgorithmEditorBindLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；选择绑定 |
| Btn_AlgorithmEditorValidate | min(0,1) max(0,1); pivot(0,1); sizeDelta(183,48)初始化; pos(0,0)初始化; LayoutElement preferred(183,48); 最终位置/尺寸由组驱动 | group | Button + Image；更新问题栏并可点击定位 |
| Txt_AlgorithmEditorValidateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；验证 |
| Btn_AlgorithmEditorApply | min(0,1) max(0,1); pivot(0,1); sizeDelta(183,48)初始化; pos(0,0)初始化; LayoutElement preferred(183,48); 最终位置/尺寸由组驱动 | group | Button + Image；完整验证；错误阻止，警告可确认；17应用确认后安全点生效 |
| Txt_AlgorithmEditorApplyLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；应用草稿 |
| Btn_AlgorithmEditorDiagnose | min(0,1) max(0,1); pivot(0,1); sizeDelta(183,48)初始化; pos(0,0)初始化; LayoutElement preferred(183,48); 最终位置/尺寸由组驱动 | group | Button + Image；13-诊断模式，保留草稿 |
| Txt_AlgorithmEditorDiagnoseLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；切换诊断模式 |
| Btn_AlgorithmEditorTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(183,48)初始化; pos(0,0)初始化; LayoutElement preferred(183,48); 最终位置/尺寸由组驱动 | group | Button + Image；清除实例绑定后保存玩家模板，重名覆盖走强确认 |
| Txt_AlgorithmEditorTemplateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；保存为模板 |
| Panel_AlgorithmEditorNodes | min(0,1) max(0,1); pivot(0,1); sizeDelta(280,478); pos(0,-124) | absolute | Image；节点库 |
| Txt_AlgorithmEditorNodesHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(256,32); pos(12,-8) | absolute | TextMeshProUGUI；节点库 |
| List_AlgorithmEditorNodes | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_AlgorithmEditorNodes | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_AlgorithmEditorNodes | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_AlgorithmEditorNodesBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(256,120)初始化; pos(0,0)初始化; LayoutElement preferred(256,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；输入、判断与运算、状态、流程、行为分类；搜索；解锁状态；端口能力 |
| Item_AlgorithmEditorNodesTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(256,104)初始化; pos(0,0)初始化; LayoutElement preferred(256,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_AlgorithmEditorNodesRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_AlgorithmEditorNodesRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_AlgorithmEditorNodesRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_AlgorithmEditorControls | min(0,1) max(0,1); pivot(0,1); sizeDelta(256,88)初始化; pos(0,0)初始化; LayoutElement preferred(256,88); 最终位置/尺寸由组驱动 | group | Image + LayoutElement；真实输入字段 |
| Panel_AlgorithmEditorNodeSearch | min(0,1) max(0,1); pivot(0,1); sizeDelta(224,48); pos(16,-8) | absolute | Image + TMP_InputField；搜索节点；onEndEdit验证 |
| Grp_AlgorithmEditorNodeSearchTextViewport | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-8); pos(0,0) | absolute | RectMask2D；textViewport，无Graphic |
| Txt_AlgorithmEditorNodeSearchValue | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；— |
| Txt_AlgorithmEditorNodeSearchPlaceholder | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；搜索节点 |
| Panel_AlgorithmEditorCanvas | min(0,1) max(0,1); pivot(0,1); sizeDelta(1112,478); pos(296,-124) | absolute | Image；节点画布 |
| Txt_AlgorithmEditorCanvasHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(1088,32); pos(12,-8) | absolute | TextMeshProUGUI；节点画布 |
| Panel_AlgorithmEditorInspector | min(0,1) max(0,1); pivot(0,1); sizeDelta(304,478); pos(1424,-124) | absolute | Image；节点检查器 |
| Txt_AlgorithmEditorInspectorHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(280,32); pos(12,-8) | absolute | TextMeshProUGUI；节点检查器 |
| List_AlgorithmEditorInspector | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_AlgorithmEditorInspector | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_AlgorithmEditorInspector | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_AlgorithmEditorInspectorBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(280,120)初始化; pos(0,0)初始化; LayoutElement preferred(280,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；绑定；节点参数；默认值；优先级；端口类型／单位；公开参数定义 |
| Item_AlgorithmEditorInspectorTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(280,104)初始化; pos(0,0)初始化; LayoutElement preferred(280,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_AlgorithmEditorInspectorRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_AlgorithmEditorInspectorRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_AlgorithmEditorInspectorRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_AlgorithmEditorProblems | min(0,1) max(0,1); pivot(0,1); sizeDelta(1728,152); pos(0,-618) | absolute | Image；验证问题 |
| Txt_AlgorithmEditorProblemsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(1704,32); pos(12,-8) | absolute | TextMeshProUGUI；验证问题 |
| List_AlgorithmEditorProblems | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_AlgorithmEditorProblems | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_AlgorithmEditorProblems | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_AlgorithmEditorProblemsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(1704,120)初始化; pos(0,0)初始化; LayoutElement preferred(1704,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；错误与警告列表；节点／端口定位；逻辑容量与算力构成 |
| Item_AlgorithmEditorProblemsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(1704,104)初始化; pos(0,0)初始化; LayoutElement preferred(1704,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_AlgorithmEditorProblemsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_AlgorithmEditorProblemsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_AlgorithmEditorProblemsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_AlgorithmEditorToolbar | min(0,1) max(0,1); pivot(0,1); sizeDelta(1728,72); pos(0,-36) | absolute | Image；算法状态 |
| Txt_AlgorithmEditorToolbarHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(1704,32); pos(12,-8) | absolute | TextMeshProUGUI；算法状态 |
| Grp_AlgorithmEditorLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1728,734); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_AlgorithmEditorLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_AlgorithmEditorLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_AlgorithmEditorEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1728,734); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_AlgorithmEditorEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_AlgorithmEditorEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_AlgorithmEditorErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1728,734); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_AlgorithmEditorErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_AlgorithmEditorErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_AlgorithmEditorSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1728,734); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_AlgorithmEditorSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_AlgorithmEditorSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_AlgorithmEditorDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1728,734); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_AlgorithmEditorDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_AlgorithmEditorDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |
| List_AlgorithmGraph | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect horizontal=true vertical=true；图画布拖动与缩放只消费画布内输入 |
| Viewport_AlgorithmGraph | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；图裁切 |
| Content_AlgorithmGraph | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(4000,4000); pos(0,0) | absolute | GraphLayoutGroup : LayoutGroup（待实现）；不挂ContentSizeFitter；根据模型画布坐标布局，单一布局所有权；不与手写Transform争夺 |
| Item_AlgorithmGraphElementTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(240,168); pos(0,0)初始化 | group | LayoutElement + Image；默认inactive；统一图元素模板：节点或连线；位置由GraphLayoutGroup及图快照驱动 |
| Grp_AlgorithmNode | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | 无Graphic；节点模式启用，连线模式隐藏 |
| Btn_AlgorithmNodeSelect | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选择节点；拖拽意图修改草稿图坐标 |
| Txt_AlgorithmNodeName | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,36); pos(0,-12) | absolute | TextMeshProUGUI；节点名：— |
| List_AlgorithmInputPorts | min(0,0.5) max(0,0.5); pivot(0,0.5); sizeDelta(112,104); pos(0,-16) | absolute | ScrollRect vertical=true；端口列表 |
| Viewport_AlgorithmInputPorts | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；端口裁切 |
| Content_AlgorithmInputPorts | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=4 + ContentSizeFitter vertical=Preferred；端口按模型稳定排序 |
| Item_AlgorithmInputPortTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(112,28); pos(0,0)初始化 | group | LayoutElement + Image；默认inactive；LayoutElement preferred=(112,28)；端口真实数据 |
| Btn_AlgorithmInputPort | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选择端口建立／断开连接；兼容性验证 |
| Txt_AlgorithmInputPort | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；端口名／类型：— |
| List_AlgorithmOutputPorts | min(1,0.5) max(1,0.5); pivot(1,0.5); sizeDelta(112,104); pos(0,-16) | absolute | ScrollRect vertical=true；端口列表 |
| Viewport_AlgorithmOutputPorts | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；端口裁切 |
| Content_AlgorithmOutputPorts | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=4 + ContentSizeFitter vertical=Preferred；端口按模型稳定排序 |
| Item_AlgorithmOutputPortTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(112,28); pos(0,0)初始化 | group | LayoutElement + Image；默认inactive；LayoutElement preferred=(112,28)；端口真实数据 |
| Btn_AlgorithmOutputPort | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选择端口建立／断开连接；兼容性验证 |
| Txt_AlgorithmOutputPort | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；端口名／类型：— |
| Grp_AlgorithmEdge | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | 无Graphic；连线模式启用，节点模式隐藏 |
| Img_AlgorithmEdge | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image + 连线表现组件；由图模型计算端点和形状；诊断高亮；raycastTarget=false |
| Btn_AlgorithmEdgeSelect | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；透明命中代理沿边包络，选中后可删除；诊断禁用写操作 |
| Txt_AlgorithmToolbarStatus | min(0,1) max(0,1); pivot(0,1); sizeDelta(1704,28); pos(12,-40) | absolute | TextMeshProUGUI；算法／机器／容量／草稿／运行版本：— |
| Grp_AlgorithmDiagnosisActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | HorizontalLayoutGroup spacing=8；诊断启用时替代Grp_AlgorithmEditorActions，编辑时隐藏 |
| Btn_AlgorithmDiagnosisPreviousStep | min(0,1) max(0,1); pivot(0,1); sizeDelta(200,48)初始化; pos(0,0)初始化 | group | Button + Image + LayoutElement preferredWidth=200 preferredHeight=48；上一步 |
| Txt_AlgorithmDiagnosisPreviousStepLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；上一步 |
| Btn_AlgorithmDiagnosisNextStep | min(0,1) max(0,1); pivot(0,1); sizeDelta(200,48)初始化; pos(0,0)初始化 | group | Button + Image + LayoutElement preferredWidth=200 preferredHeight=48；下一步 |
| Txt_AlgorithmDiagnosisNextStepLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；下一步 |
| Btn_AlgorithmDiagnosisLocate | min(0,1) max(0,1); pivot(0,1); sizeDelta(200,48)初始化; pos(0,0)初始化 | group | Button + Image + LayoutElement preferredWidth=200 preferredHeight=48；定位关联对象 |
| Txt_AlgorithmDiagnosisLocateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；定位关联对象 |
| Btn_AlgorithmDiagnosisReturnEdit | min(0,1) max(0,1); pivot(0,1); sizeDelta(200,48)初始化; pos(0,0)初始化 | group | Button + Image + LayoutElement preferredWidth=200 preferredHeight=48；返回编辑 |
| Txt_AlgorithmDiagnosisReturnEditLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；返回编辑 |

### AlgorithmEditor工具栏补充节点

以下节点加入Panel_AlgorithmEditorToolbar，属于该页完整树的一部分；仅编辑模式显示，诊断模式隐藏。

```text
Panel_AlgorithmEditorToolbar
  Grp_AlgorithmDraftTools
    Btn_AlgorithmUndo
      Txt_AlgorithmUndoLabel
    Btn_AlgorithmRedo
      Txt_AlgorithmRedoLabel
    Btn_AlgorithmDeleteSelected
      Txt_AlgorithmDeleteSelectedLabel
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Grp_AlgorithmDraftTools | min(1,1) max(1,1); pivot(1,1); sizeDelta(360,32); pos(-12,-4) | absolute | HorizontalLayoutGroup spacing=8；无Graphic |
| Btn_AlgorithmUndo | min(0,1) max(0,1); pivot(0,1); sizeDelta(112,32)初始化; pos(0,0)初始化 | group | Button + Image + LayoutElement preferred=(112,32)；撤销草稿；无历史时禁用 |
| Txt_AlgorithmUndoLabel | min(0,0) max(1,1); pivot(.5,.5); sizeDelta(-8,-4); pos(0,0) | absolute | TextMeshProUGUI；撤销 |
| Btn_AlgorithmRedo | min(0,1) max(0,1); pivot(0,1); sizeDelta(112,32)初始化; pos(0,0)初始化 | group | Button + Image + LayoutElement preferred=(112,32)；重做草稿；无历史时禁用 |
| Txt_AlgorithmRedoLabel | min(0,0) max(1,1); pivot(.5,.5); sizeDelta(-8,-4); pos(0,0) | absolute | TextMeshProUGUI；重做 |
| Btn_AlgorithmDeleteSelected | min(0,1) max(0,1); pivot(0,1); sizeDelta(112,32)初始化; pos(0,0)初始化 | group | Button + Image + LayoutElement preferred=(112,32)；删除选中节点／连线，仅修改草稿 |
| Txt_AlgorithmDeleteSelectedLabel | min(0,0) max(1,1); pivot(.5,.5); sizeDelta(-8,-4); pos(0,0) | absolute | TextMeshProUGUI；删除选中 |

Txt_AlgorithmEditorToolbarHeading的宽度在本补充覆盖为400（左上位置仍(12,-8)、高32），为右侧工具留出空间。

## AlgorithmDiagnosis：算法诊断模式

功能文档：[算法诊断模式](AlgorithmDiagnosis.md)；归属 `AlgorithmEditorForm`；内容 1728×830。

本模式复用本文件AlgorithmEditor节整棵节点树，不产生Panel_PageAlgorithmDiagnosis或第二画布。页面映射见[模式合同](../00-页面关系与复用.md)。

## PublicParameters：算法公开参数

功能文档：[算法公开参数](PublicParameters.md)；归属 `AlgorithmEditorForm`；内容 1728×830。

```text
Panel_PagePublicParameters [Image]
  Txt_PublicParametersTitle [TextMeshProUGUI]
  Grp_PublicParametersActions [GridLayoutGroup fixedColumns=9 cell=(183,48) spacing=(8,8)]
    Btn_PublicParametersChange [Button + Image]
      Txt_PublicParametersChangeLabel [TextMeshProUGUI]
    Btn_PublicParametersDefault [Button + Image]
      Txt_PublicParametersDefaultLabel [TextMeshProUGUI]
    Btn_PublicParametersApply [Button + Image]
      Txt_PublicParametersApplyLabel [TextMeshProUGUI]
  Panel_PublicParametersParameters [Image]
    Txt_PublicParametersParametersHeading [TextMeshProUGUI]
    List_PublicParametersParameters [ScrollRect vertical=true horizontal=false]
      Viewport_PublicParametersParameters [RectMask2D]
        Content_PublicParametersParameters [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_PublicParametersParametersBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_PublicParametersParametersTemplate [LayoutElement + Image；默认inactive]
            Btn_PublicParametersParametersRow [Button + Image]
              Txt_PublicParametersParametersRowLabel [TextMeshProUGUI]
              Txt_PublicParametersParametersRowValue [TextMeshProUGUI]
          Panel_PublicParametersControls [Image + LayoutElement]
            Panel_PublicParametersNumberValue [Image + TMP_InputField]
              Grp_PublicParametersNumberValueTextViewport [RectMask2D]
                Txt_PublicParametersNumberValueValue [TextMeshProUGUI]
                Txt_PublicParametersNumberValuePlaceholder [TextMeshProUGUI]
            Tgl_PublicParametersBooleanValue [Toggle]
              Img_PublicParametersBooleanValueBox [Image]
                Icon_PublicParametersBooleanValueCheck [Image]
              Txt_PublicParametersBooleanValueLabel [TextMeshProUGUI]
  Panel_PublicParametersImpact [Image]
    Txt_PublicParametersImpactHeading [TextMeshProUGUI]
    List_PublicParametersImpact [ScrollRect vertical=true horizontal=false]
      Viewport_PublicParametersImpact [RectMask2D]
        Content_PublicParametersImpact [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_PublicParametersImpactBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_PublicParametersImpactTemplate [LayoutElement + Image；默认inactive]
            Btn_PublicParametersImpactRow [Button + Image]
              Txt_PublicParametersImpactRowLabel [TextMeshProUGUI]
              Txt_PublicParametersImpactRowValue [TextMeshProUGUI]
  Grp_PublicParametersLoadingState [无Graphic]
    Panel_PublicParametersLoadingMessage [Image]
      Txt_PublicParametersLoadingMessage [TextMeshProUGUI]
  Grp_PublicParametersEmptyState [无Graphic]
    Panel_PublicParametersEmptyMessage [Image]
      Txt_PublicParametersEmptyMessage [TextMeshProUGUI]
  Grp_PublicParametersErrorState [无Graphic]
    Panel_PublicParametersErrorMessage [Image]
      Txt_PublicParametersErrorMessage [TextMeshProUGUI]
  Grp_PublicParametersSuccessState [无Graphic]
    Panel_PublicParametersSuccessMessage [Image]
      Txt_PublicParametersSuccessMessage [TextMeshProUGUI]
  Grp_PublicParametersDisabledState [无Graphic]
    Panel_PublicParametersDisabledMessage [Image]
      Txt_PublicParametersDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PagePublicParameters | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；算法公开参数；内部页面根 |
| Txt_PublicParametersTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1712,32); pos(8,0) | absolute | TextMeshProUGUI；算法公开参数 |
| Grp_PublicParametersActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=9 cell=(183,48) spacing=(8,8)；操作区 |
| Btn_PublicParametersChange | min(0,1) max(0,1); pivot(0,1); sizeDelta(183,48)初始化; pos(0,0)初始化; LayoutElement preferred(183,48); 最终位置/尺寸由组驱动 | group | Button + Image；类型匹配的输入／Toggle／数值控件写入草稿 |
| Txt_PublicParametersChangeLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；修改参数 |
| Btn_PublicParametersDefault | min(0,1) max(0,1); pivot(0,1); sizeDelta(183,48)初始化; pos(0,0)初始化; LayoutElement preferred(183,48); 最终位置/尺寸由组驱动 | group | Button + Image；仅修改本次草稿并显示差值 |
| Txt_PublicParametersDefaultLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；恢复参数默认 |
| Btn_PublicParametersApply | min(0,1) max(0,1); pivot(0,1); sizeDelta(183,48)初始化; pos(0,0)初始化; LayoutElement preferred(183,48); 最终位置/尺寸由组驱动 | group | Button + Image；复用17算法应用，不绕过完整验证 |
| Txt_PublicParametersApplyLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；验证并应用 |
| Panel_PublicParametersParameters | min(0,1) max(0,1); pivot(0,1); sizeDelta(656,734); pos(0,-36) | absolute | Image；参数列表 |
| Txt_PublicParametersParametersHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(632,32); pos(12,-8) | absolute | TextMeshProUGUI；参数列表 |
| List_PublicParametersParameters | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_PublicParametersParameters | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_PublicParametersParameters | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_PublicParametersParametersBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(632,120)初始化; pos(0,0)初始化; LayoutElement preferred(632,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；名称；类型；单位；当前值；默认值；合法区间；公开说明 |
| Item_PublicParametersParametersTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(632,104)初始化; pos(0,0)初始化; LayoutElement preferred(632,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_PublicParametersParametersRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_PublicParametersParametersRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_PublicParametersParametersRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_PublicParametersControls | min(0,1) max(0,1); pivot(0,1); sizeDelta(632,160)初始化; pos(0,0)初始化; LayoutElement preferred(632,160); 最终位置/尺寸由组驱动 | group | Image + LayoutElement；真实输入字段 |
| Panel_PublicParametersNumberValue | min(0,1) max(0,1); pivot(0,1); sizeDelta(600,48); pos(16,-8) | absolute | Image + TMP_InputField；选中数值参数；onEndEdit验证 |
| Grp_PublicParametersNumberValueTextViewport | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-8); pos(0,0) | absolute | RectMask2D；textViewport，无Graphic |
| Txt_PublicParametersNumberValueValue | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；— |
| Txt_PublicParametersNumberValuePlaceholder | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；选中数值参数 |
| Tgl_PublicParametersBooleanValue | min(0,1) max(0,1); pivot(0,1); sizeDelta(600,48); pos(16,-80) | absolute | Toggle；选中布尔参数 |
| Img_PublicParametersBooleanValueBox | min(0,1) max(0,1); pivot(0,1); sizeDelta(32,32); pos(0,-8) | absolute | Image；开关背景 |
| Icon_PublicParametersBooleanValueCheck | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；Toggle.graphic |
| Txt_PublicParametersBooleanValueLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-48,0); pos(24,0) | absolute | TextMeshProUGUI；选中布尔参数 |
| Panel_PublicParametersImpact | min(0,1) max(0,1); pivot(0,1); sizeDelta(1056,734); pos(672,-36) | absolute | Image；参数校验 |
| Txt_PublicParametersImpactHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(1032,32); pos(12,-8) | absolute | TextMeshProUGUI；参数校验 |
| List_PublicParametersImpact | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_PublicParametersImpact | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_PublicParametersImpact | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_PublicParametersImpactBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(1032,120)初始化; pos(0,0)初始化; LayoutElement preferred(1032,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；变更前后值；受影响节点；错误／警告；当前生效或草稿 |
| Item_PublicParametersImpactTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(1032,104)初始化; pos(0,0)初始化; LayoutElement preferred(1032,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_PublicParametersImpactRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_PublicParametersImpactRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_PublicParametersImpactRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_PublicParametersLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1728,734); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_PublicParametersLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_PublicParametersLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_PublicParametersEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1728,734); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_PublicParametersEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_PublicParametersEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_PublicParametersErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1728,734); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_PublicParametersErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_PublicParametersErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_PublicParametersSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1728,734); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_PublicParametersSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_PublicParametersSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_PublicParametersDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1728,734); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_PublicParametersDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_PublicParametersDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |
