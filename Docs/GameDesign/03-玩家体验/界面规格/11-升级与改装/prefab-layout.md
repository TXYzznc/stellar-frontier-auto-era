# 11-升级与改装 — prefab-layout

状态：2026-09-19完整设计候选，待用户集中验收；未据此修改Prefab。

继承[通用合同](../00-通用合同.md)和[共享外壳](../00-共享外壳-prefab-layout.md)。下列子树预制在所属Form的Grp_PageHost中；公共外壳由共享文档唯一声明。跨家族同属一个Form的子树合并，禁止重复根节点。表中所有Image／文本颜色、资源、射线、默认字号与状态继承通用合同，列出的数据是字段说明，不是示例业务数值。

## CarrierUpgrade：载体升级

功能文档：[载体升级](CarrierUpgrade.md)；归属 `UpgradeForm`；内容 1488×730。

```text
Panel_PageCarrierUpgrade [Image]
  Txt_CarrierUpgradeTitle [TextMeshProUGUI]
  Grp_CarrierUpgradeActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_CarrierUpgradeUpgrade [Button + Image]
      Txt_CarrierUpgradeUpgradeLabel [TextMeshProUGUI]
    Btn_CarrierUpgradeBack [Button + Image]
      Txt_CarrierUpgradeBackLabel [TextMeshProUGUI]
  Panel_CarrierUpgradeBefore [Image]
    Txt_CarrierUpgradeBeforeHeading [TextMeshProUGUI]
    List_CarrierUpgradeBefore [ScrollRect vertical=true horizontal=false]
      Viewport_CarrierUpgradeBefore [RectMask2D]
        Content_CarrierUpgradeBefore [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_CarrierUpgradeBeforeBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_CarrierUpgradeBeforeTemplate [LayoutElement + Image；默认inactive]
            Btn_CarrierUpgradeBeforeRow [Button + Image]
              Txt_CarrierUpgradeBeforeRowLabel [TextMeshProUGUI]
              Txt_CarrierUpgradeBeforeRowValue [TextMeshProUGUI]
  Panel_CarrierUpgradeAfter [Image]
    Txt_CarrierUpgradeAfterHeading [TextMeshProUGUI]
    List_CarrierUpgradeAfter [ScrollRect vertical=true horizontal=false]
      Viewport_CarrierUpgradeAfter [RectMask2D]
        Content_CarrierUpgradeAfter [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_CarrierUpgradeAfterBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_CarrierUpgradeAfterTemplate [LayoutElement + Image；默认inactive]
            Btn_CarrierUpgradeAfterRow [Button + Image]
              Txt_CarrierUpgradeAfterRowLabel [TextMeshProUGUI]
              Txt_CarrierUpgradeAfterRowValue [TextMeshProUGUI]
  Panel_CarrierUpgradeCost [Image]
    Txt_CarrierUpgradeCostHeading [TextMeshProUGUI]
    List_CarrierUpgradeCost [ScrollRect vertical=true horizontal=false]
      Viewport_CarrierUpgradeCost [RectMask2D]
        Content_CarrierUpgradeCost [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_CarrierUpgradeCostBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_CarrierUpgradeCostTemplate [LayoutElement + Image；默认inactive]
            Btn_CarrierUpgradeCostRow [Button + Image]
              Txt_CarrierUpgradeCostRowLabel [TextMeshProUGUI]
              Txt_CarrierUpgradeCostRowValue [TextMeshProUGUI]
  Grp_CarrierUpgradeLoadingState [无Graphic]
    Panel_CarrierUpgradeLoadingMessage [Image]
      Txt_CarrierUpgradeLoadingMessage [TextMeshProUGUI]
  Grp_CarrierUpgradeEmptyState [无Graphic]
    Panel_CarrierUpgradeEmptyMessage [Image]
      Txt_CarrierUpgradeEmptyMessage [TextMeshProUGUI]
  Grp_CarrierUpgradeErrorState [无Graphic]
    Panel_CarrierUpgradeErrorMessage [Image]
      Txt_CarrierUpgradeErrorMessage [TextMeshProUGUI]
  Grp_CarrierUpgradeSuccessState [无Graphic]
    Panel_CarrierUpgradeSuccessMessage [Image]
      Txt_CarrierUpgradeSuccessMessage [TextMeshProUGUI]
  Grp_CarrierUpgradeDisabledState [无Graphic]
    Panel_CarrierUpgradeDisabledMessage [Image]
      Txt_CarrierUpgradeDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageCarrierUpgrade | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；载体升级；内部页面根 |
| Txt_CarrierUpgradeTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；载体升级 |
| Grp_CarrierUpgradeActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_CarrierUpgradeUpgrade | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；17升级确认，最终再验等级、资源与对象，需安全停机时转18等待 |
| Txt_CarrierUpgradeUpgradeLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；确认升级 |
| Btn_CarrierUpgradeBack | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；保持选中实例，不因关闭取消已提交等待 |
| Txt_CarrierUpgradeBackLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；返回来源 |
| Panel_CarrierUpgradeBefore | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(0,-36) | absolute | Image；当前属性 |
| Txt_CarrierUpgradeBeforeHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；当前属性 |
| List_CarrierUpgradeBefore | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_CarrierUpgradeBefore | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_CarrierUpgradeBefore | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_CarrierUpgradeBeforeBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；机器载体身份；当前等级；各属性；安装／部署状态 |
| Item_CarrierUpgradeBeforeTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_CarrierUpgradeBeforeRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_CarrierUpgradeBeforeRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_CarrierUpgradeBeforeRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_CarrierUpgradeAfter | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(501,-36) | absolute | Image；升级后对比 |
| Txt_CarrierUpgradeAfterHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；升级后对比 |
| List_CarrierUpgradeAfter | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_CarrierUpgradeAfter | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_CarrierUpgradeAfter | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_CarrierUpgradeAfterBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；目标等级；属性差值；能耗与算力变化；能力变化 |
| Item_CarrierUpgradeAfterTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_CarrierUpgradeAfterRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_CarrierUpgradeAfterRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_CarrierUpgradeAfterRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_CarrierUpgradeCost | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(1002,-36) | absolute | Image；费用与限制 |
| Txt_CarrierUpgradeCostHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；费用与限制 |
| List_CarrierUpgradeCost | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_CarrierUpgradeCost | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_CarrierUpgradeCost | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_CarrierUpgradeCostBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；原价；实价；星拓联前期扶持50%；专有材料不打折；中枢等级上限；安全停机影响 |
| Item_CarrierUpgradeCostTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_CarrierUpgradeCostRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_CarrierUpgradeCostRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_CarrierUpgradeCostRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_CarrierUpgradeLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_CarrierUpgradeLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_CarrierUpgradeLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_CarrierUpgradeEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_CarrierUpgradeEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_CarrierUpgradeEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_CarrierUpgradeErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_CarrierUpgradeErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_CarrierUpgradeErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_CarrierUpgradeSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_CarrierUpgradeSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_CarrierUpgradeSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_CarrierUpgradeDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_CarrierUpgradeDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_CarrierUpgradeDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## ComponentUpgrade：组件升级

功能文档：[组件升级](ComponentUpgrade.md)；归属 `UpgradeForm`；内容 1488×730。

```text
Panel_PageComponentUpgrade [Image]
  Txt_ComponentUpgradeTitle [TextMeshProUGUI]
  Grp_ComponentUpgradeActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_ComponentUpgradeUpgrade [Button + Image]
      Txt_ComponentUpgradeUpgradeLabel [TextMeshProUGUI]
    Btn_ComponentUpgradeBack [Button + Image]
      Txt_ComponentUpgradeBackLabel [TextMeshProUGUI]
  Panel_ComponentUpgradeBefore [Image]
    Txt_ComponentUpgradeBeforeHeading [TextMeshProUGUI]
    List_ComponentUpgradeBefore [ScrollRect vertical=true horizontal=false]
      Viewport_ComponentUpgradeBefore [RectMask2D]
        Content_ComponentUpgradeBefore [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ComponentUpgradeBeforeBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ComponentUpgradeBeforeTemplate [LayoutElement + Image；默认inactive]
            Btn_ComponentUpgradeBeforeRow [Button + Image]
              Txt_ComponentUpgradeBeforeRowLabel [TextMeshProUGUI]
              Txt_ComponentUpgradeBeforeRowValue [TextMeshProUGUI]
  Panel_ComponentUpgradeAfter [Image]
    Txt_ComponentUpgradeAfterHeading [TextMeshProUGUI]
    List_ComponentUpgradeAfter [ScrollRect vertical=true horizontal=false]
      Viewport_ComponentUpgradeAfter [RectMask2D]
        Content_ComponentUpgradeAfter [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ComponentUpgradeAfterBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ComponentUpgradeAfterTemplate [LayoutElement + Image；默认inactive]
            Btn_ComponentUpgradeAfterRow [Button + Image]
              Txt_ComponentUpgradeAfterRowLabel [TextMeshProUGUI]
              Txt_ComponentUpgradeAfterRowValue [TextMeshProUGUI]
  Panel_ComponentUpgradeCost [Image]
    Txt_ComponentUpgradeCostHeading [TextMeshProUGUI]
    List_ComponentUpgradeCost [ScrollRect vertical=true horizontal=false]
      Viewport_ComponentUpgradeCost [RectMask2D]
        Content_ComponentUpgradeCost [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ComponentUpgradeCostBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ComponentUpgradeCostTemplate [LayoutElement + Image；默认inactive]
            Btn_ComponentUpgradeCostRow [Button + Image]
              Txt_ComponentUpgradeCostRowLabel [TextMeshProUGUI]
              Txt_ComponentUpgradeCostRowValue [TextMeshProUGUI]
  Grp_ComponentUpgradeLoadingState [无Graphic]
    Panel_ComponentUpgradeLoadingMessage [Image]
      Txt_ComponentUpgradeLoadingMessage [TextMeshProUGUI]
  Grp_ComponentUpgradeEmptyState [无Graphic]
    Panel_ComponentUpgradeEmptyMessage [Image]
      Txt_ComponentUpgradeEmptyMessage [TextMeshProUGUI]
  Grp_ComponentUpgradeErrorState [无Graphic]
    Panel_ComponentUpgradeErrorMessage [Image]
      Txt_ComponentUpgradeErrorMessage [TextMeshProUGUI]
  Grp_ComponentUpgradeSuccessState [无Graphic]
    Panel_ComponentUpgradeSuccessMessage [Image]
      Txt_ComponentUpgradeSuccessMessage [TextMeshProUGUI]
  Grp_ComponentUpgradeDisabledState [无Graphic]
    Panel_ComponentUpgradeDisabledMessage [Image]
      Txt_ComponentUpgradeDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageComponentUpgrade | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；组件升级；内部页面根 |
| Txt_ComponentUpgradeTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；组件升级 |
| Grp_ComponentUpgradeActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_ComponentUpgradeUpgrade | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；17升级确认，最终再验等级、资源与对象，需安全停机时转18等待 |
| Txt_ComponentUpgradeUpgradeLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；确认升级 |
| Btn_ComponentUpgradeBack | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；保持选中实例，不因关闭取消已提交等待 |
| Txt_ComponentUpgradeBackLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；返回来源 |
| Panel_ComponentUpgradeBefore | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(0,-36) | absolute | Image；当前属性 |
| Txt_ComponentUpgradeBeforeHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；当前属性 |
| List_ComponentUpgradeBefore | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ComponentUpgradeBefore | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ComponentUpgradeBefore | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ComponentUpgradeBeforeBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；传感器／效应器／计算核心身份；当前等级；各属性；安装／部署状态 |
| Item_ComponentUpgradeBeforeTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ComponentUpgradeBeforeRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ComponentUpgradeBeforeRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ComponentUpgradeBeforeRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_ComponentUpgradeAfter | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(501,-36) | absolute | Image；升级后对比 |
| Txt_ComponentUpgradeAfterHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；升级后对比 |
| List_ComponentUpgradeAfter | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ComponentUpgradeAfter | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ComponentUpgradeAfter | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ComponentUpgradeAfterBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；目标等级；属性差值；能耗与算力变化；能力变化 |
| Item_ComponentUpgradeAfterTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ComponentUpgradeAfterRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ComponentUpgradeAfterRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ComponentUpgradeAfterRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_ComponentUpgradeCost | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(1002,-36) | absolute | Image；费用与限制 |
| Txt_ComponentUpgradeCostHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；费用与限制 |
| List_ComponentUpgradeCost | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ComponentUpgradeCost | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ComponentUpgradeCost | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ComponentUpgradeCostBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；原价；实价；星拓联前期扶持50%；专有材料不打折；中枢等级上限；安全停机影响 |
| Item_ComponentUpgradeCostTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ComponentUpgradeCostRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ComponentUpgradeCostRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ComponentUpgradeCostRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_ComponentUpgradeLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ComponentUpgradeLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ComponentUpgradeLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_ComponentUpgradeEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ComponentUpgradeEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ComponentUpgradeEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_ComponentUpgradeErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ComponentUpgradeErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ComponentUpgradeErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_ComponentUpgradeSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ComponentUpgradeSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ComponentUpgradeSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_ComponentUpgradeDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ComponentUpgradeDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ComponentUpgradeDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## ModificationImpact：改装影响预览

功能文档：[改装影响预览](ModificationImpact.md)；归属 `UpgradeForm`；内容 1488×730。

```text
Panel_PageModificationImpact [Image]
  Txt_ModificationImpactTitle [TextMeshProUGUI]
  Grp_ModificationImpactActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_ModificationImpactConfirm [Button + Image]
      Txt_ModificationImpactConfirmLabel [TextMeshProUGUI]
    Btn_ModificationImpactCancel [Button + Image]
      Txt_ModificationImpactCancelLabel [TextMeshProUGUI]
  Panel_ModificationImpactChange [Image]
    Txt_ModificationImpactChangeHeading [TextMeshProUGUI]
    List_ModificationImpactChange [ScrollRect vertical=true horizontal=false]
      Viewport_ModificationImpactChange [RectMask2D]
        Content_ModificationImpactChange [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ModificationImpactChangeBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ModificationImpactChangeTemplate [LayoutElement + Image；默认inactive]
            Btn_ModificationImpactChangeRow [Button + Image]
              Txt_ModificationImpactChangeRowLabel [TextMeshProUGUI]
              Txt_ModificationImpactChangeRowValue [TextMeshProUGUI]
  Panel_ModificationImpactImpact [Image]
    Txt_ModificationImpactImpactHeading [TextMeshProUGUI]
    List_ModificationImpactImpact [ScrollRect vertical=true horizontal=false]
      Viewport_ModificationImpactImpact [RectMask2D]
        Content_ModificationImpactImpact [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ModificationImpactImpactBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ModificationImpactImpactTemplate [LayoutElement + Image；默认inactive]
            Btn_ModificationImpactImpactRow [Button + Image]
              Txt_ModificationImpactImpactRowLabel [TextMeshProUGUI]
              Txt_ModificationImpactImpactRowValue [TextMeshProUGUI]
  Grp_ModificationImpactLoadingState [无Graphic]
    Panel_ModificationImpactLoadingMessage [Image]
      Txt_ModificationImpactLoadingMessage [TextMeshProUGUI]
  Grp_ModificationImpactEmptyState [无Graphic]
    Panel_ModificationImpactEmptyMessage [Image]
      Txt_ModificationImpactEmptyMessage [TextMeshProUGUI]
  Grp_ModificationImpactErrorState [无Graphic]
    Panel_ModificationImpactErrorMessage [Image]
      Txt_ModificationImpactErrorMessage [TextMeshProUGUI]
  Grp_ModificationImpactSuccessState [无Graphic]
    Panel_ModificationImpactSuccessMessage [Image]
      Txt_ModificationImpactSuccessMessage [TextMeshProUGUI]
  Grp_ModificationImpactDisabledState [无Graphic]
    Panel_ModificationImpactDisabledMessage [Image]
      Txt_ModificationImpactDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageModificationImpact | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；改装影响预览；内部页面根 |
| Txt_ModificationImpactTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；改装影响预览 |
| Grp_ModificationImpactActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_ModificationImpactConfirm | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；只有业务合同明确开放的操作才能进入17影响确认 |
| Txt_ModificationImpactConfirmLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；确认受支持变更 |
| Btn_ModificationImpactCancel | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；保留原配置 |
| Txt_ModificationImpactCancelLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；放弃预览 |
| Panel_ModificationImpactChange | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；拟议变更 |
| Txt_ModificationImpactChangeHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；拟议变更 |
| List_ModificationImpactChange | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ModificationImpactChange | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ModificationImpactChange | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ModificationImpactChangeBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；来源操作；载体／组件；前后能力和安装变化 |
| Item_ModificationImpactChangeTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ModificationImpactChangeRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ModificationImpactChangeRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ModificationImpactChangeRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_ModificationImpactImpact | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；能力与算法影响 |
| Txt_ModificationImpactImpactHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；能力与算法影响 |
| List_ModificationImpactImpact | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ModificationImpactImpact | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ModificationImpactImpact | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ModificationImpactImpactBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；逻辑容量；算力；功率；兼容性；受影响绑定；安全停机 |
| Item_ModificationImpactImpactTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ModificationImpactImpactRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ModificationImpactImpactRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ModificationImpactImpactRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_ModificationImpactLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ModificationImpactLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ModificationImpactLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_ModificationImpactEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ModificationImpactEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ModificationImpactEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_ModificationImpactErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ModificationImpactErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ModificationImpactErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_ModificationImpactSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ModificationImpactSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ModificationImpactSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_ModificationImpactDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ModificationImpactDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ModificationImpactDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |
