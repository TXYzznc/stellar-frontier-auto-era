# 12-选择与绑定 — prefab-layout

状态：2026-09-19完整设计候选，待用户集中验收；未据此修改Prefab。

继承[通用合同](../00-通用合同.md)和[共享外壳](../00-共享外壳-prefab-layout.md)。下列子树预制在所属Form的Grp_PageHost中；公共外壳由共享文档唯一声明。跨家族同属一个Form的子树合并，禁止重复根节点。表中所有Image／文本颜色、资源、射线、默认字号与状态继承通用合同，列出的数据是字段说明，不是示例业务数值。

## ComponentPicker：安装替换组件选择器

功能文档：[安装替换组件选择器](ComponentPicker.md)；归属 `ComponentPickerForm`；内容 848×630。

```text
Panel_PageComponentPicker [Image]
  Txt_ComponentPickerTitle [TextMeshProUGUI]
  Grp_ComponentPickerActions [GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)]
    Btn_ComponentPickerSelect [Button + Image]
      Txt_ComponentPickerSelectLabel [TextMeshProUGUI]
    Btn_ComponentPickerConfirm [Button + Image]
      Txt_ComponentPickerConfirmLabel [TextMeshProUGUI]
    Btn_ComponentPickerCancel [Button + Image]
      Txt_ComponentPickerCancelLabel [TextMeshProUGUI]
  Panel_ComponentPickerCandidates [Image]
    Txt_ComponentPickerCandidatesHeading [TextMeshProUGUI]
    List_ComponentPickerCandidates [ScrollRect vertical=true horizontal=false]
      Viewport_ComponentPickerCandidates [RectMask2D]
        Content_ComponentPickerCandidates [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ComponentPickerCandidatesBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ComponentPickerCandidatesTemplate [LayoutElement + Image；默认inactive]
            Btn_ComponentPickerCandidatesRow [Button + Image]
              Txt_ComponentPickerCandidatesRowLabel [TextMeshProUGUI]
              Txt_ComponentPickerCandidatesRowValue [TextMeshProUGUI]
  Panel_ComponentPickerComparison [Image]
    Txt_ComponentPickerComparisonHeading [TextMeshProUGUI]
    List_ComponentPickerComparison [ScrollRect vertical=true horizontal=false]
      Viewport_ComponentPickerComparison [RectMask2D]
        Content_ComponentPickerComparison [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ComponentPickerComparisonBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ComponentPickerComparisonTemplate [LayoutElement + Image；默认inactive]
            Btn_ComponentPickerComparisonRow [Button + Image]
              Txt_ComponentPickerComparisonRowLabel [TextMeshProUGUI]
              Txt_ComponentPickerComparisonRowValue [TextMeshProUGUI]
  Grp_ComponentPickerLoadingState [无Graphic]
    Panel_ComponentPickerLoadingMessage [Image]
      Txt_ComponentPickerLoadingMessage [TextMeshProUGUI]
  Grp_ComponentPickerEmptyState [无Graphic]
    Panel_ComponentPickerEmptyMessage [Image]
      Txt_ComponentPickerEmptyMessage [TextMeshProUGUI]
  Grp_ComponentPickerErrorState [无Graphic]
    Panel_ComponentPickerErrorMessage [Image]
      Txt_ComponentPickerErrorMessage [TextMeshProUGUI]
  Grp_ComponentPickerSuccessState [无Graphic]
    Panel_ComponentPickerSuccessMessage [Image]
      Txt_ComponentPickerSuccessMessage [TextMeshProUGUI]
  Grp_ComponentPickerDisabledState [无Graphic]
    Panel_ComponentPickerDisabledMessage [Image]
      Txt_ComponentPickerDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageComponentPicker | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；安装替换组件选择器；内部页面根 |
| Txt_ComponentPickerTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(832,32); pos(8,0) | absolute | TextMeshProUGUI；安装替换组件选择器 |
| Grp_ComponentPickerActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)；操作区 |
| Btn_ComponentPickerSelect | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；只改变预选项 |
| Txt_ComponentPickerSelectLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；选择候选 |
| Btn_ComponentPickerConfirm | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；装配模式返回候选ID并进入17硬件确认；比较模式只返回比较对象 |
| Txt_ComponentPickerConfirmLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；使用该组件 |
| Btn_ComponentPickerCancel | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；返回来源槽位，不消费库存 |
| Txt_ComponentPickerCancelLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；取消 |
| Panel_ComponentPickerCandidates | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(0,-36) | absolute | Image；候选组件 |
| Txt_ComponentPickerCandidatesHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；候选组件 |
| List_ComponentPickerCandidates | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ComponentPickerCandidates | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ComponentPickerCandidates | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ComponentPickerCandidatesBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；未安装实例；型号／等级；兼容条件；占用与锁定状态 |
| Item_ComponentPickerCandidatesTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ComponentPickerCandidatesRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ComponentPickerCandidatesRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ComponentPickerCandidatesRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_ComponentPickerComparison | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(432,-36) | absolute | Image；与当前安装比较 |
| Txt_ComponentPickerComparisonHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；与当前安装比较 |
| List_ComponentPickerComparison | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ComponentPickerComparison | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ComponentPickerComparison | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ComponentPickerComparisonBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；属性差值；能耗／算力；槽位；算法绑定影响；不兼容原因 |
| Item_ComponentPickerComparisonTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ComponentPickerComparisonRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ComponentPickerComparisonRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ComponentPickerComparisonRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_ComponentPickerLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ComponentPickerLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ComponentPickerLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_ComponentPickerEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ComponentPickerEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ComponentPickerEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_ComponentPickerErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ComponentPickerErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ComponentPickerErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_ComponentPickerSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ComponentPickerSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ComponentPickerSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_ComponentPickerDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ComponentPickerDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ComponentPickerDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## PendingBindings：模板集中待绑定

功能文档：[模板集中待绑定](PendingBindings.md)；归属 `AlgorithmBindingForm`；内容 1488×730。

```text
Panel_PagePendingBindings [Image]
  Txt_PendingBindingsTitle [TextMeshProUGUI]
  Grp_PendingBindingsActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_PendingBindingsBind [Button + Image]
      Txt_PendingBindingsBindLabel [TextMeshProUGUI]
    Btn_PendingBindingsLocate [Button + Image]
      Txt_PendingBindingsLocateLabel [TextMeshProUGUI]
    Btn_PendingBindingsReview [Button + Image]
      Txt_PendingBindingsReviewLabel [TextMeshProUGUI]
    Btn_PendingBindingsCancel [Button + Image]
      Txt_PendingBindingsCancelLabel [TextMeshProUGUI]
  Panel_PendingBindingsBindings [Image]
    Txt_PendingBindingsBindingsHeading [TextMeshProUGUI]
    List_PendingBindingsBindings [ScrollRect vertical=true horizontal=false]
      Viewport_PendingBindingsBindings [RectMask2D]
        Content_PendingBindingsBindings [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_PendingBindingsBindingsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_PendingBindingsBindingsTemplate [LayoutElement + Image；默认inactive]
            Btn_PendingBindingsBindingsRow [Button + Image]
              Txt_PendingBindingsBindingsRowLabel [TextMeshProUGUI]
              Txt_PendingBindingsBindingsRowValue [TextMeshProUGUI]
  Panel_PendingBindingsRequirement [Image]
    Txt_PendingBindingsRequirementHeading [TextMeshProUGUI]
    List_PendingBindingsRequirement [ScrollRect vertical=true horizontal=false]
      Viewport_PendingBindingsRequirement [RectMask2D]
        Content_PendingBindingsRequirement [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_PendingBindingsRequirementBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_PendingBindingsRequirementTemplate [LayoutElement + Image；默认inactive]
            Btn_PendingBindingsRequirementRow [Button + Image]
              Txt_PendingBindingsRequirementRowLabel [TextMeshProUGUI]
              Txt_PendingBindingsRequirementRowValue [TextMeshProUGUI]
  Grp_PendingBindingsLoadingState [无Graphic]
    Panel_PendingBindingsLoadingMessage [Image]
      Txt_PendingBindingsLoadingMessage [TextMeshProUGUI]
  Grp_PendingBindingsEmptyState [无Graphic]
    Panel_PendingBindingsEmptyMessage [Image]
      Txt_PendingBindingsEmptyMessage [TextMeshProUGUI]
  Grp_PendingBindingsErrorState [无Graphic]
    Panel_PendingBindingsErrorMessage [Image]
      Txt_PendingBindingsErrorMessage [TextMeshProUGUI]
  Grp_PendingBindingsSuccessState [无Graphic]
    Panel_PendingBindingsSuccessMessage [Image]
      Txt_PendingBindingsSuccessMessage [TextMeshProUGUI]
  Grp_PendingBindingsDisabledState [无Graphic]
    Panel_PendingBindingsDisabledMessage [Image]
      Txt_PendingBindingsDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PagePendingBindings | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；模板集中待绑定；内部页面根 |
| Txt_PendingBindingsTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；模板集中待绑定 |
| Grp_PendingBindingsActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_PendingBindingsBind | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；按类型进入12节点组件或世界对象选择器 |
| Txt_PendingBindingsBindLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；选择绑定 |
| Btn_PendingBindingsLocate | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开13草稿并定位待绑定节点，保留未完成状态 |
| Txt_PendingBindingsLocateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；定位节点 |
| Btn_PendingBindingsReview | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；草稿转13，未绑定项仍标问题，不能跳过应用验证 |
| Txt_PendingBindingsReviewLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；进入编辑器检查 |
| Btn_PendingBindingsCancel | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；按草稿保存／放弃处理，不安装到机器 |
| Txt_PendingBindingsCancelLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；返回模板库 |
| Panel_PendingBindingsBindings | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；待绑定清单 |
| Txt_PendingBindingsBindingsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；待绑定清单 |
| List_PendingBindingsBindings | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_PendingBindingsBindings | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_PendingBindingsBindings | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_PendingBindingsBindingsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；节点与端口；要求能力／类型；组件或世界对象；未绑定／有效／失效 |
| Item_PendingBindingsBindingsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_PendingBindingsBindingsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_PendingBindingsBindingsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_PendingBindingsBindingsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_PendingBindingsRequirement | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；选中绑定要求 |
| Txt_PendingBindingsRequirementHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；选中绑定要求 |
| List_PendingBindingsRequirement | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_PendingBindingsRequirement | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_PendingBindingsRequirement | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_PendingBindingsRequirementBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；来源模板节点；所需单位／能力／权限；候选不足原因 |
| Item_PendingBindingsRequirementTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_PendingBindingsRequirementRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_PendingBindingsRequirementRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_PendingBindingsRequirementRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_PendingBindingsLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_PendingBindingsLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_PendingBindingsLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_PendingBindingsEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_PendingBindingsEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_PendingBindingsEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_PendingBindingsErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_PendingBindingsErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_PendingBindingsErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_PendingBindingsSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_PendingBindingsSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_PendingBindingsSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_PendingBindingsDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_PendingBindingsDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_PendingBindingsDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## NodeComponentPicker：节点组件选择器

功能文档：[节点组件选择器](NodeComponentPicker.md)；归属 `NodeComponentPickerForm`；内容 848×630。

```text
Panel_PageNodeComponentPicker [Image]
  Txt_NodeComponentPickerTitle [TextMeshProUGUI]
  Grp_NodeComponentPickerActions [GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)]
    Btn_NodeComponentPickerSelect [Button + Image]
      Txt_NodeComponentPickerSelectLabel [TextMeshProUGUI]
    Btn_NodeComponentPickerConfirm [Button + Image]
      Txt_NodeComponentPickerConfirmLabel [TextMeshProUGUI]
    Btn_NodeComponentPickerCancel [Button + Image]
      Txt_NodeComponentPickerCancelLabel [TextMeshProUGUI]
  Panel_NodeComponentPickerCandidates [Image]
    Txt_NodeComponentPickerCandidatesHeading [TextMeshProUGUI]
    List_NodeComponentPickerCandidates [ScrollRect vertical=true horizontal=false]
      Viewport_NodeComponentPickerCandidates [RectMask2D]
        Content_NodeComponentPickerCandidates [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_NodeComponentPickerCandidatesBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_NodeComponentPickerCandidatesTemplate [LayoutElement + Image；默认inactive]
            Btn_NodeComponentPickerCandidatesRow [Button + Image]
              Txt_NodeComponentPickerCandidatesRowLabel [TextMeshProUGUI]
              Txt_NodeComponentPickerCandidatesRowValue [TextMeshProUGUI]
  Panel_NodeComponentPickerContract [Image]
    Txt_NodeComponentPickerContractHeading [TextMeshProUGUI]
    List_NodeComponentPickerContract [ScrollRect vertical=true horizontal=false]
      Viewport_NodeComponentPickerContract [RectMask2D]
        Content_NodeComponentPickerContract [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_NodeComponentPickerContractBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_NodeComponentPickerContractTemplate [LayoutElement + Image；默认inactive]
            Btn_NodeComponentPickerContractRow [Button + Image]
              Txt_NodeComponentPickerContractRowLabel [TextMeshProUGUI]
              Txt_NodeComponentPickerContractRowValue [TextMeshProUGUI]
  Grp_NodeComponentPickerLoadingState [无Graphic]
    Panel_NodeComponentPickerLoadingMessage [Image]
      Txt_NodeComponentPickerLoadingMessage [TextMeshProUGUI]
  Grp_NodeComponentPickerEmptyState [无Graphic]
    Panel_NodeComponentPickerEmptyMessage [Image]
      Txt_NodeComponentPickerEmptyMessage [TextMeshProUGUI]
  Grp_NodeComponentPickerErrorState [无Graphic]
    Panel_NodeComponentPickerErrorMessage [Image]
      Txt_NodeComponentPickerErrorMessage [TextMeshProUGUI]
  Grp_NodeComponentPickerSuccessState [无Graphic]
    Panel_NodeComponentPickerSuccessMessage [Image]
      Txt_NodeComponentPickerSuccessMessage [TextMeshProUGUI]
  Grp_NodeComponentPickerDisabledState [无Graphic]
    Panel_NodeComponentPickerDisabledMessage [Image]
      Txt_NodeComponentPickerDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageNodeComponentPicker | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；节点组件选择器；内部页面根 |
| Txt_NodeComponentPickerTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(832,32); pos(8,0) | absolute | TextMeshProUGUI；节点组件选择器 |
| Grp_NodeComponentPickerActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)；操作区 |
| Btn_NodeComponentPickerSelect | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；预选实际组件及端口 |
| Txt_NodeComponentPickerSelectLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；选择端点 |
| Btn_NodeComponentPickerConfirm | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；返回稳定实例／安装位引用给草稿，未直接应用 |
| Txt_NodeComponentPickerConfirmLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；绑定端点 |
| Btn_NodeComponentPickerCancel | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；原绑定不变 |
| Txt_NodeComponentPickerCancelLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；取消 |
| Panel_NodeComponentPickerCandidates | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(0,-36) | absolute | Image；本机端点 |
| Txt_NodeComponentPickerCandidatesHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；本机端点 |
| List_NodeComponentPickerCandidates | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_NodeComponentPickerCandidates | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_NodeComponentPickerCandidates | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_NodeComponentPickerCandidatesBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；组件名称；安装位；能力端口；数据类型；单位；启动与安装状态 |
| Item_NodeComponentPickerCandidatesTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_NodeComponentPickerCandidatesRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_NodeComponentPickerCandidatesRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_NodeComponentPickerCandidatesRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_NodeComponentPickerContract | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(432,-36) | absolute | Image；匹配说明 |
| Txt_NodeComponentPickerContractHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；匹配说明 |
| List_NodeComponentPickerContract | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_NodeComponentPickerContract | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_NodeComponentPickerContract | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_NodeComponentPickerContractBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；所需输入／输出类型；能力；不兼容原因；选中端点解释 |
| Item_NodeComponentPickerContractTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_NodeComponentPickerContractRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_NodeComponentPickerContractRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_NodeComponentPickerContractRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_NodeComponentPickerLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_NodeComponentPickerLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_NodeComponentPickerLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_NodeComponentPickerEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_NodeComponentPickerEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_NodeComponentPickerEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_NodeComponentPickerErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_NodeComponentPickerErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_NodeComponentPickerErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_NodeComponentPickerSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_NodeComponentPickerSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_NodeComponentPickerSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_NodeComponentPickerDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_NodeComponentPickerDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_NodeComponentPickerDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## WorldObjectPicker：世界对象选择器

功能文档：[世界对象选择器](WorldObjectPicker.md)；归属 `WorldObjectPickerForm`；内容 848×630。

```text
Panel_PageWorldObjectPicker [Image]
  Txt_WorldObjectPickerTitle [TextMeshProUGUI]
  Grp_WorldObjectPickerActions [GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)]
    Btn_WorldObjectPickerSelect [Button + Image]
      Txt_WorldObjectPickerSelectLabel [TextMeshProUGUI]
    Btn_WorldObjectPickerWorld [Button + Image]
      Txt_WorldObjectPickerWorldLabel [TextMeshProUGUI]
    Btn_WorldObjectPickerConfirm [Button + Image]
      Txt_WorldObjectPickerConfirmLabel [TextMeshProUGUI]
    Btn_WorldObjectPickerCancel [Button + Image]
      Txt_WorldObjectPickerCancelLabel [TextMeshProUGUI]
  Panel_WorldObjectPickerCandidates [Image]
    Txt_WorldObjectPickerCandidatesHeading [TextMeshProUGUI]
    List_WorldObjectPickerCandidates [ScrollRect vertical=true horizontal=false]
      Viewport_WorldObjectPickerCandidates [RectMask2D]
        Content_WorldObjectPickerCandidates [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_WorldObjectPickerCandidatesBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_WorldObjectPickerCandidatesTemplate [LayoutElement + Image；默认inactive]
            Btn_WorldObjectPickerCandidatesRow [Button + Image]
              Txt_WorldObjectPickerCandidatesRowLabel [TextMeshProUGUI]
              Txt_WorldObjectPickerCandidatesRowValue [TextMeshProUGUI]
  Panel_WorldObjectPickerPreview [Image]
    Txt_WorldObjectPickerPreviewHeading [TextMeshProUGUI]
    List_WorldObjectPickerPreview [ScrollRect vertical=true horizontal=false]
      Viewport_WorldObjectPickerPreview [RectMask2D]
        Content_WorldObjectPickerPreview [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_WorldObjectPickerPreviewBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_WorldObjectPickerPreviewTemplate [LayoutElement + Image；默认inactive]
            Btn_WorldObjectPickerPreviewRow [Button + Image]
              Txt_WorldObjectPickerPreviewRowLabel [TextMeshProUGUI]
              Txt_WorldObjectPickerPreviewRowValue [TextMeshProUGUI]
  Grp_WorldObjectPickerLoadingState [无Graphic]
    Panel_WorldObjectPickerLoadingMessage [Image]
      Txt_WorldObjectPickerLoadingMessage [TextMeshProUGUI]
  Grp_WorldObjectPickerEmptyState [无Graphic]
    Panel_WorldObjectPickerEmptyMessage [Image]
      Txt_WorldObjectPickerEmptyMessage [TextMeshProUGUI]
  Grp_WorldObjectPickerErrorState [无Graphic]
    Panel_WorldObjectPickerErrorMessage [Image]
      Txt_WorldObjectPickerErrorMessage [TextMeshProUGUI]
  Grp_WorldObjectPickerSuccessState [无Graphic]
    Panel_WorldObjectPickerSuccessMessage [Image]
      Txt_WorldObjectPickerSuccessMessage [TextMeshProUGUI]
  Grp_WorldObjectPickerDisabledState [无Graphic]
    Panel_WorldObjectPickerDisabledMessage [Image]
      Txt_WorldObjectPickerDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageWorldObjectPicker | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；世界对象选择器；内部页面根 |
| Txt_WorldObjectPickerTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(832,32); pos(8,0) | absolute | TextMeshProUGUI；世界对象选择器 |
| Grp_WorldObjectPickerActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)；操作区 |
| Btn_WorldObjectPickerSelect | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；预选稳定ID |
| Txt_WorldObjectPickerSelectLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；选择对象 |
| Btn_WorldObjectPickerWorld | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入14世界对象选择辅助，暂存选择器上下文 |
| Txt_WorldObjectPickerWorldLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；在世界选择 |
| Btn_WorldObjectPickerConfirm | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；重新校验对象和接口，返回调用方草稿或关联确认 |
| Txt_WorldObjectPickerConfirmLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；确认绑定 |
| Btn_WorldObjectPickerCancel | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；恢复原引用和来源焦点 |
| Txt_WorldObjectPickerCancelLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；取消 |
| Panel_WorldObjectPickerCandidates | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(0,-36) | absolute | Image；兼容对象 |
| Txt_WorldObjectPickerCandidatesHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；兼容对象 |
| List_WorldObjectPickerCandidates | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_WorldObjectPickerCandidates | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_WorldObjectPickerCandidates | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_WorldObjectPickerCandidatesBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；名称、类型、位置；有效性；所需接口；不可选择原因 |
| Item_WorldObjectPickerCandidatesTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_WorldObjectPickerCandidatesRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_WorldObjectPickerCandidatesRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_WorldObjectPickerCandidatesRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_WorldObjectPickerPreview | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(432,-36) | absolute | Image；选择预览 |
| Txt_WorldObjectPickerPreviewHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；选择预览 |
| List_WorldObjectPickerPreview | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_WorldObjectPickerPreview | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_WorldObjectPickerPreview | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_WorldObjectPickerPreviewBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；绑定用途；所选对象；能力／范围约束；对象状态 |
| Item_WorldObjectPickerPreviewTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_WorldObjectPickerPreviewRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_WorldObjectPickerPreviewRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_WorldObjectPickerPreviewRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_WorldObjectPickerLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WorldObjectPickerLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WorldObjectPickerLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_WorldObjectPickerEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WorldObjectPickerEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WorldObjectPickerEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_WorldObjectPickerErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WorldObjectPickerErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WorldObjectPickerErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_WorldObjectPickerSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WorldObjectPickerSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WorldObjectPickerSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_WorldObjectPickerDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WorldObjectPickerDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WorldObjectPickerDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |
