# 16-教学规则与知识 — prefab-layout

状态：2026-09-19完整设计候选，待用户集中验收；未据此修改Prefab。

继承[通用合同](../00-通用合同.md)和[共享外壳](../00-共享外壳-prefab-layout.md)。下列子树预制在所属Form的Grp_PageHost中；公共外壳由共享文档唯一声明。跨家族同属一个Form的子树合并，禁止重复根节点。表中所有Image／文本颜色、资源、射线、默认字号与状态继承通用合同，列出的数据是字段说明，不是示例业务数值。

## Tutorial：基础引导

功能文档：[基础引导](Tutorial.md)；归属 `TutorialForm`；内容 1488×730。

```text
Panel_PageTutorial [Image]
  Txt_TutorialTitle [TextMeshProUGUI]
  Grp_TutorialActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_TutorialGo [Button + Image]
      Txt_TutorialGoLabel [TextMeshProUGUI]
    Btn_TutorialRule [Button + Image]
      Txt_TutorialRuleLabel [TextMeshProUGUI]
    Btn_TutorialClose [Button + Image]
      Txt_TutorialCloseLabel [TextMeshProUGUI]
  Panel_TutorialLesson [Image]
    Txt_TutorialLessonHeading [TextMeshProUGUI]
    List_TutorialLesson [ScrollRect vertical=true horizontal=false]
      Viewport_TutorialLesson [RectMask2D]
        Content_TutorialLesson [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_TutorialLessonBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_TutorialLessonTemplate [LayoutElement + Image；默认inactive]
            Btn_TutorialLessonRow [Button + Image]
              Txt_TutorialLessonRowLabel [TextMeshProUGUI]
              Txt_TutorialLessonRowValue [TextMeshProUGUI]
  Panel_TutorialLinks [Image]
    Txt_TutorialLinksHeading [TextMeshProUGUI]
    List_TutorialLinks [ScrollRect vertical=true horizontal=false]
      Viewport_TutorialLinks [RectMask2D]
        Content_TutorialLinks [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_TutorialLinksBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_TutorialLinksTemplate [LayoutElement + Image；默认inactive]
            Btn_TutorialLinksRow [Button + Image]
              Txt_TutorialLinksRowLabel [TextMeshProUGUI]
              Txt_TutorialLinksRowValue [TextMeshProUGUI]
  Grp_TutorialLoadingState [无Graphic]
    Panel_TutorialLoadingMessage [Image]
      Txt_TutorialLoadingMessage [TextMeshProUGUI]
  Grp_TutorialEmptyState [无Graphic]
    Panel_TutorialEmptyMessage [Image]
      Txt_TutorialEmptyMessage [TextMeshProUGUI]
  Grp_TutorialErrorState [无Graphic]
    Panel_TutorialErrorMessage [Image]
      Txt_TutorialErrorMessage [TextMeshProUGUI]
  Grp_TutorialSuccessState [无Graphic]
    Panel_TutorialSuccessMessage [Image]
      Txt_TutorialSuccessMessage [TextMeshProUGUI]
  Grp_TutorialDisabledState [无Graphic]
    Panel_TutorialDisabledMessage [Image]
      Txt_TutorialDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageTutorial | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；基础引导；内部页面根 |
| Txt_TutorialTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；基础引导 |
| Grp_TutorialActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_TutorialGo | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开已解锁目标，保留教学返回入口 |
| Txt_TutorialGoLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；前往相关功能 |
| Btn_TutorialRule | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；16通用规则 |
| Txt_TutorialRuleLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看规则 |
| Btn_TutorialClose | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；仍保留HUD图标，引导继续监听真实游戏状态 |
| Txt_TutorialCloseLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；暂时关闭 |
| Panel_TutorialLesson | min(0,1) max(0,1); pivot(0,1); sizeDelta(736,634); pos(0,-36) | absolute | Image；当前步骤 |
| Txt_TutorialLessonHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,32); pos(12,-8) | absolute | TextMeshProUGUI；当前步骤 |
| List_TutorialLesson | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_TutorialLesson | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_TutorialLesson | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_TutorialLessonBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,120)初始化; pos(0,0)初始化; LayoutElement preferred(712,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；目的；操作文字；配图；当前进度；常见错误 |
| Item_TutorialLessonTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,104)初始化; pos(0,0)初始化; LayoutElement preferred(712,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_TutorialLessonRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_TutorialLessonRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_TutorialLessonRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_TutorialLinks | min(0,1) max(0,1); pivot(0,1); sizeDelta(736,634); pos(752,-36) | absolute | Image；实际操作入口 |
| Txt_TutorialLinksHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,32); pos(12,-8) | absolute | TextMeshProUGUI；实际操作入口 |
| List_TutorialLinks | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_TutorialLinks | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_TutorialLinks | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_TutorialLinksBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,120)初始化; pos(0,0)初始化; LayoutElement preferred(712,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；关联界面；相关规则；步骤验收条件的玩家可读说明 |
| Item_TutorialLinksTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,104)初始化; pos(0,0)初始化; LayoutElement preferred(712,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_TutorialLinksRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_TutorialLinksRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_TutorialLinksRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_TutorialLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_TutorialLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_TutorialLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_TutorialEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_TutorialEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_TutorialEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_TutorialErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_TutorialErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_TutorialErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_TutorialSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_TutorialSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_TutorialSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_TutorialDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_TutorialDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_TutorialDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## FeatureHelp：新功能说明

功能文档：[新功能说明](FeatureHelp.md)；归属 `FeatureHelpForm`；内容 1488×730。

```text
Panel_PageFeatureHelp [Image]
  Txt_FeatureHelpTitle [TextMeshProUGUI]
  Grp_FeatureHelpActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_FeatureHelpOpen [Button + Image]
      Txt_FeatureHelpOpenLabel [TextMeshProUGUI]
    Btn_FeatureHelpRule [Button + Image]
      Txt_FeatureHelpRuleLabel [TextMeshProUGUI]
    Btn_FeatureHelpClose [Button + Image]
      Txt_FeatureHelpCloseLabel [TextMeshProUGUI]
  Panel_FeatureHelpExplanation [Image]
    Txt_FeatureHelpExplanationHeading [TextMeshProUGUI]
    List_FeatureHelpExplanation [ScrollRect vertical=true horizontal=false]
      Viewport_FeatureHelpExplanation [RectMask2D]
        Content_FeatureHelpExplanation [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_FeatureHelpExplanationBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_FeatureHelpExplanationTemplate [LayoutElement + Image；默认inactive]
            Btn_FeatureHelpExplanationRow [Button + Image]
              Txt_FeatureHelpExplanationRowLabel [TextMeshProUGUI]
              Txt_FeatureHelpExplanationRowValue [TextMeshProUGUI]
  Panel_FeatureHelpNext [Image]
    Txt_FeatureHelpNextHeading [TextMeshProUGUI]
    List_FeatureHelpNext [ScrollRect vertical=true horizontal=false]
      Viewport_FeatureHelpNext [RectMask2D]
        Content_FeatureHelpNext [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_FeatureHelpNextBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_FeatureHelpNextTemplate [LayoutElement + Image；默认inactive]
            Btn_FeatureHelpNextRow [Button + Image]
              Txt_FeatureHelpNextRowLabel [TextMeshProUGUI]
              Txt_FeatureHelpNextRowValue [TextMeshProUGUI]
  Grp_FeatureHelpLoadingState [无Graphic]
    Panel_FeatureHelpLoadingMessage [Image]
      Txt_FeatureHelpLoadingMessage [TextMeshProUGUI]
  Grp_FeatureHelpEmptyState [无Graphic]
    Panel_FeatureHelpEmptyMessage [Image]
      Txt_FeatureHelpEmptyMessage [TextMeshProUGUI]
  Grp_FeatureHelpErrorState [无Graphic]
    Panel_FeatureHelpErrorMessage [Image]
      Txt_FeatureHelpErrorMessage [TextMeshProUGUI]
  Grp_FeatureHelpSuccessState [无Graphic]
    Panel_FeatureHelpSuccessMessage [Image]
      Txt_FeatureHelpSuccessMessage [TextMeshProUGUI]
  Grp_FeatureHelpDisabledState [无Graphic]
    Panel_FeatureHelpDisabledMessage [Image]
      Txt_FeatureHelpDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageFeatureHelp | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；新功能说明；内部页面根 |
| Txt_FeatureHelpTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；新功能说明 |
| Grp_FeatureHelpActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_FeatureHelpOpen | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；已解锁时打开对应页面 |
| Txt_FeatureHelpOpenLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；前往功能 |
| Btn_FeatureHelpRule | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；16通用规则 |
| Txt_FeatureHelpRuleLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；规则说明 |
| Btn_FeatureHelpClose | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；保留已读标记，不反复弹出 |
| Txt_FeatureHelpCloseLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；关闭说明 |
| Panel_FeatureHelpExplanation | min(0,1) max(0,1); pivot(0,1); sizeDelta(736,634); pos(0,-36) | absolute | Image；新能力说明 |
| Txt_FeatureHelpExplanationHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,32); pos(12,-8) | absolute | TextMeshProUGUI；新能力说明 |
| List_FeatureHelpExplanation | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_FeatureHelpExplanation | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_FeatureHelpExplanation | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_FeatureHelpExplanationBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,120)初始化; pos(0,0)初始化; LayoutElement preferred(712,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；功能名；作用；开放原因；操作图片；相关界面 |
| Item_FeatureHelpExplanationTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,104)初始化; pos(0,0)初始化; LayoutElement preferred(712,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_FeatureHelpExplanationRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_FeatureHelpExplanationRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_FeatureHelpExplanationRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_FeatureHelpNext | min(0,1) max(0,1); pivot(0,1); sizeDelta(736,634); pos(752,-36) | absolute | Image；开始使用 |
| Txt_FeatureHelpNextHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,32); pos(12,-8) | absolute | TextMeshProUGUI；开始使用 |
| List_FeatureHelpNext | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_FeatureHelpNext | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_FeatureHelpNext | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_FeatureHelpNextBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,120)初始化; pos(0,0)初始化; LayoutElement preferred(712,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；必要条件；推荐第一步；相关规则 |
| Item_FeatureHelpNextTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,104)初始化; pos(0,0)初始化; LayoutElement preferred(712,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_FeatureHelpNextRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_FeatureHelpNextRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_FeatureHelpNextRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_FeatureHelpLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_FeatureHelpLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_FeatureHelpLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_FeatureHelpEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_FeatureHelpEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_FeatureHelpEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_FeatureHelpErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_FeatureHelpErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_FeatureHelpErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_FeatureHelpSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_FeatureHelpSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_FeatureHelpSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_FeatureHelpDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_FeatureHelpDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_FeatureHelpDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## Help：帮助

功能文档：[帮助](Help.md)；归属 `HelpForm`；内容 1488×730。

```text
Panel_PageHelp [Image]
  Txt_HelpTitle [TextMeshProUGUI]
  Grp_HelpActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_HelpSelect [Button + Image]
      Txt_HelpSelectLabel [TextMeshProUGUI]
    Btn_HelpGo [Button + Image]
      Txt_HelpGoLabel [TextMeshProUGUI]
    Btn_HelpRule [Button + Image]
      Txt_HelpRuleLabel [TextMeshProUGUI]
  Panel_HelpTopics [Image]
    Txt_HelpTopicsHeading [TextMeshProUGUI]
    List_HelpTopics [ScrollRect vertical=true horizontal=false]
      Viewport_HelpTopics [RectMask2D]
        Content_HelpTopics [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_HelpTopicsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_HelpTopicsTemplate [LayoutElement + Image；默认inactive]
            Btn_HelpTopicsRow [Button + Image]
              Txt_HelpTopicsRowLabel [TextMeshProUGUI]
              Txt_HelpTopicsRowValue [TextMeshProUGUI]
  Panel_HelpArticle [Image]
    Txt_HelpArticleHeading [TextMeshProUGUI]
    List_HelpArticle [ScrollRect vertical=true horizontal=false]
      Viewport_HelpArticle [RectMask2D]
        Content_HelpArticle [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_HelpArticleBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_HelpArticleTemplate [LayoutElement + Image；默认inactive]
            Btn_HelpArticleRow [Button + Image]
              Txt_HelpArticleRowLabel [TextMeshProUGUI]
              Txt_HelpArticleRowValue [TextMeshProUGUI]
  Grp_HelpLoadingState [无Graphic]
    Panel_HelpLoadingMessage [Image]
      Txt_HelpLoadingMessage [TextMeshProUGUI]
  Grp_HelpEmptyState [无Graphic]
    Panel_HelpEmptyMessage [Image]
      Txt_HelpEmptyMessage [TextMeshProUGUI]
  Grp_HelpErrorState [无Graphic]
    Panel_HelpErrorMessage [Image]
      Txt_HelpErrorMessage [TextMeshProUGUI]
  Grp_HelpSuccessState [无Graphic]
    Panel_HelpSuccessMessage [Image]
      Txt_HelpSuccessMessage [TextMeshProUGUI]
  Grp_HelpDisabledState [无Graphic]
    Panel_HelpDisabledMessage [Image]
      Txt_HelpDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageHelp | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；帮助；内部页面根 |
| Txt_HelpTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；帮助 |
| Grp_HelpActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_HelpSelect | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；更新正文并将阅读滚动回顶部 |
| Txt_HelpSelectLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；选择主题 |
| Btn_HelpGo | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；只允许已解锁目标 |
| Txt_HelpGoLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；前往相关界面 |
| Btn_HelpRule | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；16通用规则 |
| Txt_HelpRuleLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；机制说明 |
| Panel_HelpTopics | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；帮助主题 |
| Txt_HelpTopicsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；帮助主题 |
| List_HelpTopics | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_HelpTopics | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_HelpTopics | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_HelpTopicsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；基本操作、机器配置、算法、建造、能源、存档等已公开主题 |
| Item_HelpTopicsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_HelpTopicsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_HelpTopicsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_HelpTopicsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_HelpArticle | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；主题正文 |
| Txt_HelpArticleHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；主题正文 |
| List_HelpArticle | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_HelpArticle | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_HelpArticle | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_HelpArticleBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；目的；操作步骤；图片；常见错误；相关入口 |
| Item_HelpArticleTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_HelpArticleRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_HelpArticleRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_HelpArticleRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_HelpLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HelpLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HelpLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_HelpEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HelpEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HelpEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_HelpErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HelpErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HelpErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_HelpSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HelpSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HelpSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_HelpDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HelpDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HelpDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## RuleHelp：通用规则说明

功能文档：[通用规则说明](RuleHelp.md)；归属 `RuleHelpForm`；内容 848×630。

```text
Panel_PageRuleHelp [Image]
  Txt_RuleHelpTitle [TextMeshProUGUI]
  Grp_RuleHelpActions [GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)]
    Btn_RuleHelpClose [Button + Image]
      Txt_RuleHelpCloseLabel [TextMeshProUGUI]
  Panel_RuleHelpDescription [Image]
    Txt_RuleHelpDescriptionHeading [TextMeshProUGUI]
    List_RuleHelpDescription [ScrollRect vertical=true horizontal=false]
      Viewport_RuleHelpDescription [RectMask2D]
        Content_RuleHelpDescription [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_RuleHelpDescriptionBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_RuleHelpDescriptionTemplate [LayoutElement + Image；默认inactive]
            Btn_RuleHelpDescriptionRow [Button + Image]
              Txt_RuleHelpDescriptionRowLabel [TextMeshProUGUI]
              Txt_RuleHelpDescriptionRowValue [TextMeshProUGUI]
  Grp_RuleHelpLoadingState [无Graphic]
    Panel_RuleHelpLoadingMessage [Image]
      Txt_RuleHelpLoadingMessage [TextMeshProUGUI]
  Grp_RuleHelpEmptyState [无Graphic]
    Panel_RuleHelpEmptyMessage [Image]
      Txt_RuleHelpEmptyMessage [TextMeshProUGUI]
  Grp_RuleHelpErrorState [无Graphic]
    Panel_RuleHelpErrorMessage [Image]
      Txt_RuleHelpErrorMessage [TextMeshProUGUI]
  Grp_RuleHelpSuccessState [无Graphic]
    Panel_RuleHelpSuccessMessage [Image]
      Txt_RuleHelpSuccessMessage [TextMeshProUGUI]
  Grp_RuleHelpDisabledState [无Graphic]
    Panel_RuleHelpDisabledMessage [Image]
      Txt_RuleHelpDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageRuleHelp | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；通用规则说明；内部页面根 |
| Txt_RuleHelpTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(832,32); pos(8,0) | absolute | TextMeshProUGUI；通用规则说明 |
| Grp_RuleHelpActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)；操作区 |
| Btn_RuleHelpClose | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；恢复来源分页、筛选、滚动、选择与焦点 |
| Txt_RuleHelpCloseLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；关闭 |
| Panel_RuleHelpDescription | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | Image；规则内容 |
| Txt_RuleHelpDescriptionHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(824,32); pos(12,-8) | absolute | TextMeshProUGUI；规则内容 |
| List_RuleHelpDescription | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_RuleHelpDescription | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_RuleHelpDescription | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_RuleHelpDescriptionBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(824,120)初始化; pos(0,0)初始化; LayoutElement preferred(824,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；标题；完整独立正文；可选配图 |
| Item_RuleHelpDescriptionTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(824,104)初始化; pos(0,0)初始化; LayoutElement preferred(824,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_RuleHelpDescriptionRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_RuleHelpDescriptionRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_RuleHelpDescriptionRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_RuleHelpLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_RuleHelpLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_RuleHelpLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_RuleHelpEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_RuleHelpEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_RuleHelpEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_RuleHelpErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_RuleHelpErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_RuleHelpErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_RuleHelpSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_RuleHelpSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_RuleHelpSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_RuleHelpDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_RuleHelpDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_RuleHelpDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## CropKnowledge：作物图鉴详情

功能文档：[作物图鉴详情](CropKnowledge.md)；归属 `CropKnowledgeForm`；内容 1488×730。

```text
Panel_PageCropKnowledge [Image]
  Txt_CropKnowledgeTitle [TextMeshProUGUI]
  Grp_CropKnowledgeActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_CropKnowledgeBack [Button + Image]
      Txt_CropKnowledgeBackLabel [TextMeshProUGUI]
    Btn_CropKnowledgeRule [Button + Image]
      Txt_CropKnowledgeRuleLabel [TextMeshProUGUI]
  Panel_CropKnowledgeIdentity [Image]
    Txt_CropKnowledgeIdentityHeading [TextMeshProUGUI]
    List_CropKnowledgeIdentity [ScrollRect vertical=true horizontal=false]
      Viewport_CropKnowledgeIdentity [RectMask2D]
        Content_CropKnowledgeIdentity [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_CropKnowledgeIdentityBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_CropKnowledgeIdentityTemplate [LayoutElement + Image；默认inactive]
            Btn_CropKnowledgeIdentityRow [Button + Image]
              Txt_CropKnowledgeIdentityRowLabel [TextMeshProUGUI]
              Txt_CropKnowledgeIdentityRowValue [TextMeshProUGUI]
  Panel_CropKnowledgeKnowledge [Image]
    Txt_CropKnowledgeKnowledgeHeading [TextMeshProUGUI]
    List_CropKnowledgeKnowledge [ScrollRect vertical=true horizontal=false]
      Viewport_CropKnowledgeKnowledge [RectMask2D]
        Content_CropKnowledgeKnowledge [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_CropKnowledgeKnowledgeBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_CropKnowledgeKnowledgeTemplate [LayoutElement + Image；默认inactive]
            Btn_CropKnowledgeKnowledgeRow [Button + Image]
              Txt_CropKnowledgeKnowledgeRowLabel [TextMeshProUGUI]
              Txt_CropKnowledgeKnowledgeRowValue [TextMeshProUGUI]
  Grp_CropKnowledgeLoadingState [无Graphic]
    Panel_CropKnowledgeLoadingMessage [Image]
      Txt_CropKnowledgeLoadingMessage [TextMeshProUGUI]
  Grp_CropKnowledgeEmptyState [无Graphic]
    Panel_CropKnowledgeEmptyMessage [Image]
      Txt_CropKnowledgeEmptyMessage [TextMeshProUGUI]
  Grp_CropKnowledgeErrorState [无Graphic]
    Panel_CropKnowledgeErrorMessage [Image]
      Txt_CropKnowledgeErrorMessage [TextMeshProUGUI]
  Grp_CropKnowledgeSuccessState [无Graphic]
    Panel_CropKnowledgeSuccessMessage [Image]
      Txt_CropKnowledgeSuccessMessage [TextMeshProUGUI]
  Grp_CropKnowledgeDisabledState [无Graphic]
    Panel_CropKnowledgeDisabledMessage [Image]
      Txt_CropKnowledgeDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageCropKnowledge | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；作物图鉴详情；内部页面根 |
| Txt_CropKnowledgeTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；作物图鉴详情 |
| Grp_CropKnowledgeActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_CropKnowledgeBack | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；恢复选中农田 |
| Txt_CropKnowledgeBackLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；返回农田 |
| Btn_CropKnowledgeRule | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开该知识对应规则说明 |
| Txt_CropKnowledgeRuleLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看规则 |
| Panel_CropKnowledgeIdentity | min(0,1) max(0,1); pivot(0,1); sizeDelta(736,634); pos(0,-36) | absolute | Image；作物资料 |
| Txt_CropKnowledgeIdentityHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,32); pos(12,-8) | absolute | TextMeshProUGUI；作物资料 |
| List_CropKnowledgeIdentity | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_CropKnowledgeIdentity | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_CropKnowledgeIdentity | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_CropKnowledgeIdentityBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,120)初始化; pos(0,0)初始化; LayoutElement preferred(712,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；名称；图像；知识解锁状态；可公开的基础说明 |
| Item_CropKnowledgeIdentityTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,104)初始化; pos(0,0)初始化; LayoutElement preferred(712,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_CropKnowledgeIdentityRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_CropKnowledgeIdentityRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_CropKnowledgeIdentityRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_CropKnowledgeKnowledge | min(0,1) max(0,1); pivot(0,1); sizeDelta(736,634); pos(752,-36) | absolute | Image；已解锁知识 |
| Txt_CropKnowledgeKnowledgeHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,32); pos(12,-8) | absolute | TextMeshProUGUI；已解锁知识 |
| List_CropKnowledgeKnowledge | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_CropKnowledgeKnowledge | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_CropKnowledgeKnowledge | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_CropKnowledgeKnowledgeBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,120)初始化; pos(0,0)初始化; LayoutElement preferred(712,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；环境影响变量；最佳／容忍区间；生长和产出规则；数据单位 |
| Item_CropKnowledgeKnowledgeTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,104)初始化; pos(0,0)初始化; LayoutElement preferred(712,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_CropKnowledgeKnowledgeRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_CropKnowledgeKnowledgeRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_CropKnowledgeKnowledgeRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_CropKnowledgeLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_CropKnowledgeLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_CropKnowledgeLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_CropKnowledgeEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_CropKnowledgeEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_CropKnowledgeEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_CropKnowledgeErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_CropKnowledgeErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_CropKnowledgeErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_CropKnowledgeSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_CropKnowledgeSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_CropKnowledgeSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_CropKnowledgeDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_CropKnowledgeDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_CropKnowledgeDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |
