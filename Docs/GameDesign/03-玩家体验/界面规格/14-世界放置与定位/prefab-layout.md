# 14-世界放置与定位 — prefab-layout

状态：2026-09-19完整设计候选，待用户集中验收；未据此修改Prefab。

继承[通用合同](../00-通用合同.md)和[共享外壳](../00-共享外壳-prefab-layout.md)。下列子树预制在所属Form的Grp_PageHost中；公共外壳由共享文档唯一声明。跨家族同属一个Form的子树合并，禁止重复根节点。表中所有Image／文本颜色、资源、射线、默认字号与状态继承通用合同，列出的数据是字段说明，不是示例业务数值。

## BuildPlacement：建筑放置

功能文档：[建筑放置](BuildPlacement.md)；归属 `WorldPlacementForm`；内容 1200×280。

```text
Panel_PageBuildPlacement [Image]
  Txt_BuildPlacementTitle [TextMeshProUGUI]
  Grp_BuildPlacementActions [GridLayoutGroup fixedColumns=6 cell=(190,48) spacing=(8,8)]
    Btn_BuildPlacementRotate [Button + Image]
      Txt_BuildPlacementRotateLabel [TextMeshProUGUI]
    Btn_BuildPlacementConfirm [Button + Image]
      Txt_BuildPlacementConfirmLabel [TextMeshProUGUI]
    Btn_BuildPlacementCancel [Button + Image]
      Txt_BuildPlacementCancelLabel [TextMeshProUGUI]
  Panel_BuildPlacementSelection [Image]
    Txt_BuildPlacementSelectionHeading [TextMeshProUGUI]
    List_BuildPlacementSelection [ScrollRect vertical=true horizontal=false]
      Viewport_BuildPlacementSelection [RectMask2D]
        Content_BuildPlacementSelection [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_BuildPlacementSelectionBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_BuildPlacementSelectionTemplate [LayoutElement + Image；默认inactive]
            Btn_BuildPlacementSelectionRow [Button + Image]
              Txt_BuildPlacementSelectionRowLabel [TextMeshProUGUI]
              Txt_BuildPlacementSelectionRowValue [TextMeshProUGUI]
  Panel_BuildPlacementValidity [Image]
    Txt_BuildPlacementValidityHeading [TextMeshProUGUI]
    List_BuildPlacementValidity [ScrollRect vertical=true horizontal=false]
      Viewport_BuildPlacementValidity [RectMask2D]
        Content_BuildPlacementValidity [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_BuildPlacementValidityBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_BuildPlacementValidityTemplate [LayoutElement + Image；默认inactive]
            Btn_BuildPlacementValidityRow [Button + Image]
              Txt_BuildPlacementValidityRowLabel [TextMeshProUGUI]
              Txt_BuildPlacementValidityRowValue [TextMeshProUGUI]
  Panel_BuildPlacementHints [Image]
    Txt_BuildPlacementHintsHeading [TextMeshProUGUI]
    List_BuildPlacementHints [ScrollRect vertical=true horizontal=false]
      Viewport_BuildPlacementHints [RectMask2D]
        Content_BuildPlacementHints [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_BuildPlacementHintsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_BuildPlacementHintsTemplate [LayoutElement + Image；默认inactive]
            Btn_BuildPlacementHintsRow [Button + Image]
              Txt_BuildPlacementHintsRowLabel [TextMeshProUGUI]
              Txt_BuildPlacementHintsRowValue [TextMeshProUGUI]
  Grp_BuildPlacementLoadingState [无Graphic]
    Panel_BuildPlacementLoadingMessage [Image]
      Txt_BuildPlacementLoadingMessage [TextMeshProUGUI]
  Grp_BuildPlacementEmptyState [无Graphic]
    Panel_BuildPlacementEmptyMessage [Image]
      Txt_BuildPlacementEmptyMessage [TextMeshProUGUI]
  Grp_BuildPlacementErrorState [无Graphic]
    Panel_BuildPlacementErrorMessage [Image]
      Txt_BuildPlacementErrorMessage [TextMeshProUGUI]
  Grp_BuildPlacementSuccessState [无Graphic]
    Panel_BuildPlacementSuccessMessage [Image]
      Txt_BuildPlacementSuccessMessage [TextMeshProUGUI]
  Grp_BuildPlacementDisabledState [无Graphic]
    Panel_BuildPlacementDisabledMessage [Image]
      Txt_BuildPlacementDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageBuildPlacement | min(0.5,0) max(0.5,0); pivot(0.5,0); sizeDelta(1200,280); pos(0,24) | absolute | Image；建筑放置；内部页面根 |
| Txt_BuildPlacementTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1184,32); pos(8,0) | absolute | TextMeshProUGUI；建筑放置 |
| Grp_BuildPlacementActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=6 cell=(190,48) spacing=(8,8)；操作区 |
| Btn_BuildPlacementRotate | min(0,1) max(0,1); pivot(0,1); sizeDelta(190,48)初始化; pos(0,0)初始化; LayoutElement preferred(190,48); 最终位置/尺寸由组驱动 | group | Button + Image；更新朝向与合法性预览 |
| Txt_BuildPlacementRotateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；旋转建筑 |
| Btn_BuildPlacementConfirm | min(0,1) max(0,1); pivot(0,1); sizeDelta(190,48)初始化; pos(0,0)初始化; LayoutElement preferred(190,48); 最终位置/尺寸由组驱动 | group | Button + Image；位置合法后17建造确认；提交时再验位置与资源，原子扣除并生成施工 |
| Txt_BuildPlacementConfirmLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；确认建造 |
| Btn_BuildPlacementCancel | min(0,1) max(0,1); pivot(0,1); sizeDelta(190,48)初始化; pos(0,0)初始化; LayoutElement preferred(190,48); 最终位置/尺寸由组驱动 | group | Button + Image；回图纸目录并保持选中项，不扣资源 |
| Txt_BuildPlacementCancelLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；取消放置 |
| Panel_BuildPlacementSelection | min(0,1) max(0,1); pivot(0,1); sizeDelta(389,184); pos(0,-36) | absolute | Image；当前建筑 |
| Txt_BuildPlacementSelectionHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,32); pos(12,-8) | absolute | TextMeshProUGUI；当前建筑 |
| List_BuildPlacementSelection | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_BuildPlacementSelection | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_BuildPlacementSelection | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_BuildPlacementSelectionBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,120)初始化; pos(0,0)初始化; LayoutElement preferred(365,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；名称；功能；完整成本；当前可用资源；施工时间 |
| Item_BuildPlacementSelectionTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,104)初始化; pos(0,0)初始化; LayoutElement preferred(365,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_BuildPlacementSelectionRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_BuildPlacementSelectionRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_BuildPlacementSelectionRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_BuildPlacementValidity | min(0,1) max(0,1); pivot(0,1); sizeDelta(389,184); pos(405,-36) | absolute | Image；世界预览状态 |
| Txt_BuildPlacementValidityHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,32); pos(12,-8) | absolute | TextMeshProUGUI；世界预览状态 |
| List_BuildPlacementValidity | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_BuildPlacementValidity | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_BuildPlacementValidity | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_BuildPlacementValidityBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,120)初始化; pos(0,0)初始化; LayoutElement preferred(365,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；位置；朝向；占地轮廓；合法／非法及主要原因 |
| Item_BuildPlacementValidityTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,104)初始化; pos(0,0)初始化; LayoutElement preferred(365,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_BuildPlacementValidityRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_BuildPlacementValidityRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_BuildPlacementValidityRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_BuildPlacementHints | min(0,1) max(0,1); pivot(0,1); sizeDelta(389,184); pos(810,-36) | absolute | Image；操作提示 |
| Txt_BuildPlacementHintsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,32); pos(12,-8) | absolute | TextMeshProUGUI；操作提示 |
| List_BuildPlacementHints | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_BuildPlacementHints | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_BuildPlacementHints | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_BuildPlacementHintsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,120)初始化; pos(0,0)初始化; LayoutElement preferred(365,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；鼠标移动预览；左键选择确认位置；R旋转；Esc取消；右键旋转镜头；滚轮缩放 |
| Item_BuildPlacementHintsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,104)初始化; pos(0,0)初始化; LayoutElement preferred(365,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_BuildPlacementHintsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_BuildPlacementHintsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_BuildPlacementHintsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_BuildPlacementLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1200,184); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BuildPlacementLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BuildPlacementLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_BuildPlacementEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1200,184); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BuildPlacementEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BuildPlacementEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_BuildPlacementErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1200,184); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BuildPlacementErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BuildPlacementErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_BuildPlacementSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1200,184); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BuildPlacementSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BuildPlacementSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_BuildPlacementDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1200,184); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BuildPlacementDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BuildPlacementDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## MachineDeployment：机器部署

功能文档：[机器部署](MachineDeployment.md)；归属 `WorldPlacementForm`；内容 1200×280。

```text
Panel_PageMachineDeployment [Image]
  Txt_MachineDeploymentTitle [TextMeshProUGUI]
  Grp_MachineDeploymentActions [GridLayoutGroup fixedColumns=6 cell=(190,48) spacing=(8,8)]
    Btn_MachineDeploymentRotate [Button + Image]
      Txt_MachineDeploymentRotateLabel [TextMeshProUGUI]
    Btn_MachineDeploymentConfirm [Button + Image]
      Txt_MachineDeploymentConfirmLabel [TextMeshProUGUI]
    Btn_MachineDeploymentCancel [Button + Image]
      Txt_MachineDeploymentCancelLabel [TextMeshProUGUI]
  Panel_MachineDeploymentSelection [Image]
    Txt_MachineDeploymentSelectionHeading [TextMeshProUGUI]
    List_MachineDeploymentSelection [ScrollRect vertical=true horizontal=false]
      Viewport_MachineDeploymentSelection [RectMask2D]
        Content_MachineDeploymentSelection [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_MachineDeploymentSelectionBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_MachineDeploymentSelectionTemplate [LayoutElement + Image；默认inactive]
            Btn_MachineDeploymentSelectionRow [Button + Image]
              Txt_MachineDeploymentSelectionRowLabel [TextMeshProUGUI]
              Txt_MachineDeploymentSelectionRowValue [TextMeshProUGUI]
  Panel_MachineDeploymentValidity [Image]
    Txt_MachineDeploymentValidityHeading [TextMeshProUGUI]
    List_MachineDeploymentValidity [ScrollRect vertical=true horizontal=false]
      Viewport_MachineDeploymentValidity [RectMask2D]
        Content_MachineDeploymentValidity [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_MachineDeploymentValidityBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_MachineDeploymentValidityTemplate [LayoutElement + Image；默认inactive]
            Btn_MachineDeploymentValidityRow [Button + Image]
              Txt_MachineDeploymentValidityRowLabel [TextMeshProUGUI]
              Txt_MachineDeploymentValidityRowValue [TextMeshProUGUI]
  Panel_MachineDeploymentHints [Image]
    Txt_MachineDeploymentHintsHeading [TextMeshProUGUI]
    List_MachineDeploymentHints [ScrollRect vertical=true horizontal=false]
      Viewport_MachineDeploymentHints [RectMask2D]
        Content_MachineDeploymentHints [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_MachineDeploymentHintsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_MachineDeploymentHintsTemplate [LayoutElement + Image；默认inactive]
            Btn_MachineDeploymentHintsRow [Button + Image]
              Txt_MachineDeploymentHintsRowLabel [TextMeshProUGUI]
              Txt_MachineDeploymentHintsRowValue [TextMeshProUGUI]
  Grp_MachineDeploymentLoadingState [无Graphic]
    Panel_MachineDeploymentLoadingMessage [Image]
      Txt_MachineDeploymentLoadingMessage [TextMeshProUGUI]
  Grp_MachineDeploymentEmptyState [无Graphic]
    Panel_MachineDeploymentEmptyMessage [Image]
      Txt_MachineDeploymentEmptyMessage [TextMeshProUGUI]
  Grp_MachineDeploymentErrorState [无Graphic]
    Panel_MachineDeploymentErrorMessage [Image]
      Txt_MachineDeploymentErrorMessage [TextMeshProUGUI]
  Grp_MachineDeploymentSuccessState [无Graphic]
    Panel_MachineDeploymentSuccessMessage [Image]
      Txt_MachineDeploymentSuccessMessage [TextMeshProUGUI]
  Grp_MachineDeploymentDisabledState [无Graphic]
    Panel_MachineDeploymentDisabledMessage [Image]
      Txt_MachineDeploymentDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageMachineDeployment | min(0.5,0) max(0.5,0); pivot(0.5,0); sizeDelta(1200,280); pos(0,24) | absolute | Image；机器部署；内部页面根 |
| Txt_MachineDeploymentTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1184,32); pos(8,0) | absolute | TextMeshProUGUI；机器部署 |
| Grp_MachineDeploymentActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=6 cell=(190,48) spacing=(8,8)；操作区 |
| Btn_MachineDeploymentRotate | min(0,1) max(0,1); pivot(0,1); sizeDelta(190,48)初始化; pos(0,0)初始化; LayoutElement preferred(190,48); 最终位置/尺寸由组驱动 | group | Button + Image；更新预览朝向 |
| Txt_MachineDeploymentRotateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；旋转 |
| Btn_MachineDeploymentConfirm | min(0,1) max(0,1); pivot(0,1); sizeDelta(190,48)初始化; pos(0,0)初始化; LayoutElement preferred(190,48); 最终位置/尺寸由组驱动 | group | Button + Image；17部署确认后原子移出未部署库并生成同一实例的已部署状态 |
| Txt_MachineDeploymentConfirmLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；确认部署 |
| Btn_MachineDeploymentCancel | min(0,1) max(0,1); pivot(0,1); sizeDelta(190,48)初始化; pos(0,0)初始化; LayoutElement preferred(190,48); 最终位置/尺寸由组驱动 | group | Button + Image；回机器库或整备，保持原配置 |
| Txt_MachineDeploymentCancelLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；取消 |
| Panel_MachineDeploymentSelection | min(0,1) max(0,1); pivot(0,1); sizeDelta(389,184); pos(0,-36) | absolute | Image；待部署机器 |
| Txt_MachineDeploymentSelectionHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,32); pos(12,-8) | absolute | TextMeshProUGUI；待部署机器 |
| List_MachineDeploymentSelection | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_MachineDeploymentSelection | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_MachineDeploymentSelection | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_MachineDeploymentSelectionBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,120)初始化; pos(0,0)初始化; LayoutElement preferred(365,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；独立实例；名称／型号；组件配置；当前区域（唯一经营区域） |
| Item_MachineDeploymentSelectionTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,104)初始化; pos(0,0)初始化; LayoutElement preferred(365,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_MachineDeploymentSelectionRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_MachineDeploymentSelectionRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_MachineDeploymentSelectionRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_MachineDeploymentValidity | min(0,1) max(0,1); pivot(0,1); sizeDelta(389,184); pos(405,-36) | absolute | Image；位置与朝向 |
| Txt_MachineDeploymentValidityHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,32); pos(12,-8) | absolute | TextMeshProUGUI；位置与朝向 |
| List_MachineDeploymentValidity | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_MachineDeploymentValidity | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_MachineDeploymentValidity | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_MachineDeploymentValidityBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,120)初始化; pos(0,0)初始化; LayoutElement preferred(365,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；占地；朝向；合法性及原因；部署后未激活说明 |
| Item_MachineDeploymentValidityTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,104)初始化; pos(0,0)初始化; LayoutElement preferred(365,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_MachineDeploymentValidityRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_MachineDeploymentValidityRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_MachineDeploymentValidityRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_MachineDeploymentHints | min(0,1) max(0,1); pivot(0,1); sizeDelta(389,184); pos(810,-36) | absolute | Image；操作提示 |
| Txt_MachineDeploymentHintsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,32); pos(12,-8) | absolute | TextMeshProUGUI；操作提示 |
| List_MachineDeploymentHints | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_MachineDeploymentHints | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_MachineDeploymentHints | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_MachineDeploymentHintsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,120)初始化; pos(0,0)初始化; LayoutElement preferred(365,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；放置、旋转、确认、取消；镜头旋转与缩放 |
| Item_MachineDeploymentHintsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,104)初始化; pos(0,0)初始化; LayoutElement preferred(365,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_MachineDeploymentHintsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_MachineDeploymentHintsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_MachineDeploymentHintsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_MachineDeploymentLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1200,184); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineDeploymentLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineDeploymentLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_MachineDeploymentEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1200,184); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineDeploymentEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineDeploymentEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_MachineDeploymentErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1200,184); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineDeploymentErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineDeploymentErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_MachineDeploymentSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1200,184); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineDeploymentSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineDeploymentSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_MachineDeploymentDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1200,184); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineDeploymentDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineDeploymentDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## WorldBinding：世界对象选择与定位辅助

功能文档：[世界对象选择与定位辅助](WorldBinding.md)；归属 `WorldPlacementForm`；内容 1200×280。

```text
Panel_PageWorldBinding [Image]
  Txt_WorldBindingTitle [TextMeshProUGUI]
  Grp_WorldBindingActions [GridLayoutGroup fixedColumns=6 cell=(190,48) spacing=(8,8)]
    Btn_WorldBindingPick [Button + Image]
      Txt_WorldBindingPickLabel [TextMeshProUGUI]
    Btn_WorldBindingConfirm [Button + Image]
      Txt_WorldBindingConfirmLabel [TextMeshProUGUI]
    Btn_WorldBindingCancel [Button + Image]
      Txt_WorldBindingCancelLabel [TextMeshProUGUI]
    Btn_WorldBindingFocus [Button + Image]
      Txt_WorldBindingFocusLabel [TextMeshProUGUI]
  Panel_WorldBindingPurpose [Image]
    Txt_WorldBindingPurposeHeading [TextMeshProUGUI]
    List_WorldBindingPurpose [ScrollRect vertical=true horizontal=false]
      Viewport_WorldBindingPurpose [RectMask2D]
        Content_WorldBindingPurpose [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_WorldBindingPurposeBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_WorldBindingPurposeTemplate [LayoutElement + Image；默认inactive]
            Btn_WorldBindingPurposeRow [Button + Image]
              Txt_WorldBindingPurposeRowLabel [TextMeshProUGUI]
              Txt_WorldBindingPurposeRowValue [TextMeshProUGUI]
  Panel_WorldBindingCandidate [Image]
    Txt_WorldBindingCandidateHeading [TextMeshProUGUI]
    List_WorldBindingCandidate [ScrollRect vertical=true horizontal=false]
      Viewport_WorldBindingCandidate [RectMask2D]
        Content_WorldBindingCandidate [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_WorldBindingCandidateBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_WorldBindingCandidateTemplate [LayoutElement + Image；默认inactive]
            Btn_WorldBindingCandidateRow [Button + Image]
              Txt_WorldBindingCandidateRowLabel [TextMeshProUGUI]
              Txt_WorldBindingCandidateRowValue [TextMeshProUGUI]
  Panel_WorldBindingHints [Image]
    Txt_WorldBindingHintsHeading [TextMeshProUGUI]
    List_WorldBindingHints [ScrollRect vertical=true horizontal=false]
      Viewport_WorldBindingHints [RectMask2D]
        Content_WorldBindingHints [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_WorldBindingHintsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_WorldBindingHintsTemplate [LayoutElement + Image；默认inactive]
            Btn_WorldBindingHintsRow [Button + Image]
              Txt_WorldBindingHintsRowLabel [TextMeshProUGUI]
              Txt_WorldBindingHintsRowValue [TextMeshProUGUI]
  Grp_WorldBindingLoadingState [无Graphic]
    Panel_WorldBindingLoadingMessage [Image]
      Txt_WorldBindingLoadingMessage [TextMeshProUGUI]
  Grp_WorldBindingEmptyState [无Graphic]
    Panel_WorldBindingEmptyMessage [Image]
      Txt_WorldBindingEmptyMessage [TextMeshProUGUI]
  Grp_WorldBindingErrorState [无Graphic]
    Panel_WorldBindingErrorMessage [Image]
      Txt_WorldBindingErrorMessage [TextMeshProUGUI]
  Grp_WorldBindingSuccessState [无Graphic]
    Panel_WorldBindingSuccessMessage [Image]
      Txt_WorldBindingSuccessMessage [TextMeshProUGUI]
  Grp_WorldBindingDisabledState [无Graphic]
    Panel_WorldBindingDisabledMessage [Image]
      Txt_WorldBindingDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageWorldBinding | min(0.5,0) max(0.5,0); pivot(0.5,0); sizeDelta(1200,280); pos(0,24) | absolute | Image；世界对象选择与定位辅助；内部页面根 |
| Txt_WorldBindingTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1184,32); pos(8,0) | absolute | TextMeshProUGUI；世界对象选择与定位辅助 |
| Grp_WorldBindingActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=6 cell=(190,48) spacing=(8,8)；操作区 |
| Btn_WorldBindingPick | min(0,1) max(0,1); pivot(0,1); sizeDelta(190,48)初始化; pos(0,0)初始化; LayoutElement preferred(190,48); 最终位置/尺寸由组驱动 | group | Button + Image；射线选择整对象，不直接运行世界作业 |
| Txt_WorldBindingPickLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；选中对象 |
| Btn_WorldBindingConfirm | min(0,1) max(0,1); pivot(0,1); sizeDelta(190,48)初始化; pos(0,0)初始化; LayoutElement preferred(190,48); 最终位置/尺寸由组驱动 | group | Button + Image；返回12选择器预选，再由调用方最终提交 |
| Txt_WorldBindingConfirmLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；确认所选 |
| Btn_WorldBindingCancel | min(0,1) max(0,1); pivot(0,1); sizeDelta(190,48)初始化; pos(0,0)初始化; LayoutElement preferred(190,48); 最终位置/尺寸由组驱动 | group | Button + Image；恢复来源页、草稿、焦点 |
| Txt_WorldBindingCancelLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；取消 |
| Btn_WorldBindingFocus | min(0,1) max(0,1); pivot(0,1); sizeDelta(190,48)初始化; pos(0,0)初始化; LayoutElement preferred(190,48); 最终位置/尺寸由组驱动 | group | Button + Image；有效对象双击或F聚焦 |
| Txt_WorldBindingFocusLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；聚焦 |
| Panel_WorldBindingPurpose | min(0,1) max(0,1); pivot(0,1); sizeDelta(389,184); pos(0,-36) | absolute | Image；选择目的 |
| Txt_WorldBindingPurposeHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,32); pos(12,-8) | absolute | TextMeshProUGUI；选择目的 |
| List_WorldBindingPurpose | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_WorldBindingPurpose | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_WorldBindingPurpose | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_WorldBindingPurposeBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,120)初始化; pos(0,0)初始化; LayoutElement preferred(365,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；调用方；绑定字段；要求的对象类型与接口 |
| Item_WorldBindingPurposeTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,104)初始化; pos(0,0)初始化; LayoutElement preferred(365,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_WorldBindingPurposeRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_WorldBindingPurposeRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_WorldBindingPurposeRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_WorldBindingCandidate | min(0,1) max(0,1); pivot(0,1); sizeDelta(389,184); pos(405,-36) | absolute | Image；悬停与选中 |
| Txt_WorldBindingCandidateHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,32); pos(12,-8) | absolute | TextMeshProUGUI；悬停与选中 |
| List_WorldBindingCandidate | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_WorldBindingCandidate | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_WorldBindingCandidate | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_WorldBindingCandidateBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,120)初始化; pos(0,0)初始化; LayoutElement preferred(365,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；对象名；兼容／不兼容；原因；确认将写入的引用 |
| Item_WorldBindingCandidateTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,104)初始化; pos(0,0)初始化; LayoutElement preferred(365,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_WorldBindingCandidateRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_WorldBindingCandidateRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_WorldBindingCandidateRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_WorldBindingHints | min(0,1) max(0,1); pivot(0,1); sizeDelta(389,184); pos(810,-36) | absolute | Image；操作提示 |
| Txt_WorldBindingHintsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,32); pos(12,-8) | absolute | TextMeshProUGUI；操作提示 |
| List_WorldBindingHints | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_WorldBindingHints | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_WorldBindingHints | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_WorldBindingHintsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,120)初始化; pos(0,0)初始化; LayoutElement preferred(365,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；点击兼容对象；确认／取消；定位对象的短时轮廓 |
| Item_WorldBindingHintsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(365,104)初始化; pos(0,0)初始化; LayoutElement preferred(365,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_WorldBindingHintsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_WorldBindingHintsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_WorldBindingHintsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_WorldBindingLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1200,184); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WorldBindingLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WorldBindingLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_WorldBindingEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1200,184); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WorldBindingEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WorldBindingEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_WorldBindingErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1200,184); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WorldBindingErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WorldBindingErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_WorldBindingSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1200,184); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WorldBindingSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WorldBindingSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_WorldBindingDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1200,184); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WorldBindingDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WorldBindingDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |
