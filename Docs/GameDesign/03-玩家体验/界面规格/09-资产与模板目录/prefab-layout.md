# 09-资产与模板目录 — prefab-layout

状态：2026-09-19完整设计候选，待用户集中验收；未据此修改Prefab。

继承[通用合同](../00-通用合同.md)和[共享外壳](../00-共享外壳-prefab-layout.md)。下列子树预制在所属Form的Grp_PageHost中；公共外壳由共享文档唯一声明。跨家族同属一个Form的子树合并，禁止重复根节点。表中所有Image／文本颜色、资源、射线、默认字号与状态继承通用合同，列出的数据是字段说明，不是示例业务数值。

## LooseComponents：未安装组件

功能文档：[未安装组件](LooseComponents.md)；归属 `ComponentLibraryForm`；内容 1488×730。

```text
Panel_PageLooseComponents [Image]
  Txt_LooseComponentsTitle [TextMeshProUGUI]
  Grp_LooseComponentsActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_LooseComponentsFilter [Button + Image]
      Txt_LooseComponentsFilterLabel [TextMeshProUGUI]
    Btn_LooseComponentsInspect [Button + Image]
      Txt_LooseComponentsInspectLabel [TextMeshProUGUI]
    Btn_LooseComponentsUpgrade [Button + Image]
      Txt_LooseComponentsUpgradeLabel [TextMeshProUGUI]
    Btn_LooseComponentsSell [Button + Image]
      Txt_LooseComponentsSellLabel [TextMeshProUGUI]
    Btn_LooseComponentsInstalled [Button + Image]
      Txt_LooseComponentsInstalledLabel [TextMeshProUGUI]
  Panel_LooseComponentsCatalog [Image]
    Txt_LooseComponentsCatalogHeading [TextMeshProUGUI]
    List_LooseComponentsCatalog [ScrollRect vertical=true horizontal=false]
      Viewport_LooseComponentsCatalog [RectMask2D]
        Content_LooseComponentsCatalog [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_LooseComponentsCatalogBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_LooseComponentsCatalogTemplate [LayoutElement + Image；默认inactive]
            Btn_LooseComponentsCatalogRow [Button + Image]
              Txt_LooseComponentsCatalogRowLabel [TextMeshProUGUI]
              Txt_LooseComponentsCatalogRowValue [TextMeshProUGUI]
  Panel_LooseComponentsDetails [Image]
    Txt_LooseComponentsDetailsHeading [TextMeshProUGUI]
    List_LooseComponentsDetails [ScrollRect vertical=true horizontal=false]
      Viewport_LooseComponentsDetails [RectMask2D]
        Content_LooseComponentsDetails [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_LooseComponentsDetailsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_LooseComponentsDetailsTemplate [LayoutElement + Image；默认inactive]
            Btn_LooseComponentsDetailsRow [Button + Image]
              Txt_LooseComponentsDetailsRowLabel [TextMeshProUGUI]
              Txt_LooseComponentsDetailsRowValue [TextMeshProUGUI]
  Grp_LooseComponentsLoadingState [无Graphic]
    Panel_LooseComponentsLoadingMessage [Image]
      Txt_LooseComponentsLoadingMessage [TextMeshProUGUI]
  Grp_LooseComponentsEmptyState [无Graphic]
    Panel_LooseComponentsEmptyMessage [Image]
      Txt_LooseComponentsEmptyMessage [TextMeshProUGUI]
  Grp_LooseComponentsErrorState [无Graphic]
    Panel_LooseComponentsErrorMessage [Image]
      Txt_LooseComponentsErrorMessage [TextMeshProUGUI]
  Grp_LooseComponentsSuccessState [无Graphic]
    Panel_LooseComponentsSuccessMessage [Image]
      Txt_LooseComponentsSuccessMessage [TextMeshProUGUI]
  Grp_LooseComponentsDisabledState [无Graphic]
    Panel_LooseComponentsDisabledMessage [Image]
      Txt_LooseComponentsDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageLooseComponents | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；未安装组件；内部页面根 |
| Txt_LooseComponentsTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；未安装组件 |
| Grp_LooseComponentsActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_LooseComponentsFilter | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；筛选库存不重排当前正在阅读的详情 |
| Txt_LooseComponentsFilterLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；筛选 |
| Btn_LooseComponentsInspect | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入09-组件详情 |
| Txt_LooseComponentsInspectLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；组件详情与比较 |
| Btn_LooseComponentsUpgrade | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入11-组件升级 |
| Txt_LooseComponentsUpgradeLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；升级 |
| Btn_LooseComponentsSell | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；只选可出售实例，进入17交易确认 |
| Txt_LooseComponentsSellLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；出售 |
| Btn_LooseComponentsInstalled | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入09-已安装组件 |
| Txt_LooseComponentsInstalledLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；已安装分页 |
| Panel_LooseComponentsCatalog | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；分类与库存 |
| Txt_LooseComponentsCatalogHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；分类与库存 |
| List_LooseComponentsCatalog | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_LooseComponentsCatalog | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_LooseComponentsCatalog | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_LooseComponentsCatalogBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；类型、型号、等级、能力、状态筛选；相同未安装组件合并数量 |
| Item_LooseComponentsCatalogTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_LooseComponentsCatalogRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_LooseComponentsCatalogRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_LooseComponentsCatalogRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_LooseComponentsDetails | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；选中组件 |
| Txt_LooseComponentsDetailsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；选中组件 |
| List_LooseComponentsDetails | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_LooseComponentsDetails | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_LooseComponentsDetails | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_LooseComponentsDetailsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；属性；能耗与算力；兼容载体／槽位；可展开实例；锁定状态 |
| Item_LooseComponentsDetailsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_LooseComponentsDetailsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_LooseComponentsDetailsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_LooseComponentsDetailsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_LooseComponentsLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_LooseComponentsLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_LooseComponentsLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_LooseComponentsEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_LooseComponentsEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_LooseComponentsEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_LooseComponentsErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_LooseComponentsErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_LooseComponentsErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_LooseComponentsSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_LooseComponentsSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_LooseComponentsSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_LooseComponentsDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_LooseComponentsDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_LooseComponentsDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## InstalledComponents：已安装组件

功能文档：[已安装组件](InstalledComponents.md)；归属 `ComponentLibraryForm`；内容 1488×730。

```text
Panel_PageInstalledComponents [Image]
  Txt_InstalledComponentsTitle [TextMeshProUGUI]
  Grp_InstalledComponentsActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_InstalledComponentsInspect [Button + Image]
      Txt_InstalledComponentsInspectLabel [TextMeshProUGUI]
    Btn_InstalledComponentsUpgrade [Button + Image]
      Txt_InstalledComponentsUpgradeLabel [TextMeshProUGUI]
    Btn_InstalledComponentsLocate [Button + Image]
      Txt_InstalledComponentsLocateLabel [TextMeshProUGUI]
    Btn_InstalledComponentsLoose [Button + Image]
      Txt_InstalledComponentsLooseLabel [TextMeshProUGUI]
  Panel_InstalledComponentsCatalog [Image]
    Txt_InstalledComponentsCatalogHeading [TextMeshProUGUI]
    List_InstalledComponentsCatalog [ScrollRect vertical=true horizontal=false]
      Viewport_InstalledComponentsCatalog [RectMask2D]
        Content_InstalledComponentsCatalog [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_InstalledComponentsCatalogBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_InstalledComponentsCatalogTemplate [LayoutElement + Image；默认inactive]
            Btn_InstalledComponentsCatalogRow [Button + Image]
              Txt_InstalledComponentsCatalogRowLabel [TextMeshProUGUI]
              Txt_InstalledComponentsCatalogRowValue [TextMeshProUGUI]
  Panel_InstalledComponentsDetails [Image]
    Txt_InstalledComponentsDetailsHeading [TextMeshProUGUI]
    List_InstalledComponentsDetails [ScrollRect vertical=true horizontal=false]
      Viewport_InstalledComponentsDetails [RectMask2D]
        Content_InstalledComponentsDetails [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_InstalledComponentsDetailsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_InstalledComponentsDetailsTemplate [LayoutElement + Image；默认inactive]
            Btn_InstalledComponentsDetailsRow [Button + Image]
              Txt_InstalledComponentsDetailsRowLabel [TextMeshProUGUI]
              Txt_InstalledComponentsDetailsRowValue [TextMeshProUGUI]
  Grp_InstalledComponentsLoadingState [无Graphic]
    Panel_InstalledComponentsLoadingMessage [Image]
      Txt_InstalledComponentsLoadingMessage [TextMeshProUGUI]
  Grp_InstalledComponentsEmptyState [无Graphic]
    Panel_InstalledComponentsEmptyMessage [Image]
      Txt_InstalledComponentsEmptyMessage [TextMeshProUGUI]
  Grp_InstalledComponentsErrorState [无Graphic]
    Panel_InstalledComponentsErrorMessage [Image]
      Txt_InstalledComponentsErrorMessage [TextMeshProUGUI]
  Grp_InstalledComponentsSuccessState [无Graphic]
    Panel_InstalledComponentsSuccessMessage [Image]
      Txt_InstalledComponentsSuccessMessage [TextMeshProUGUI]
  Grp_InstalledComponentsDisabledState [无Graphic]
    Panel_InstalledComponentsDisabledMessage [Image]
      Txt_InstalledComponentsDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageInstalledComponents | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；已安装组件；内部页面根 |
| Txt_InstalledComponentsTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；已安装组件 |
| Grp_InstalledComponentsActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_InstalledComponentsInspect | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；09-组件详情 |
| Txt_InstalledComponentsInspectLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；组件详情与比较 |
| Btn_InstalledComponentsUpgrade | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；11-组件升级；提交后等待安全停机 |
| Txt_InstalledComponentsUpgradeLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；安全升级 |
| Btn_InstalledComponentsLocate | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；定位所属机器，现场再开放拆卸／替换／出售 |
| Txt_InstalledComponentsLocateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；前往机器现场 |
| Btn_InstalledComponentsLoose | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；09-未安装组件 |
| Txt_InstalledComponentsLooseLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；未安装分页 |
| Panel_InstalledComponentsCatalog | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；安装索引 |
| Txt_InstalledComponentsCatalogHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；安装索引 |
| List_InstalledComponentsCatalog | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_InstalledComponentsCatalog | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_InstalledComponentsCatalog | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_InstalledComponentsCatalogBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；组件名；型号；等级；所在机器与安装位；运行和锁定状态 |
| Item_InstalledComponentsCatalogTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_InstalledComponentsCatalogRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_InstalledComponentsCatalogRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_InstalledComponentsCatalogRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_InstalledComponentsDetails | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；组件与载体 |
| Txt_InstalledComponentsDetailsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；组件与载体 |
| List_InstalledComponentsDetails | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_InstalledComponentsDetails | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_InstalledComponentsDetails | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_InstalledComponentsDetailsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；属性；兼容性；当前行为；安全升级条件 |
| Item_InstalledComponentsDetailsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_InstalledComponentsDetailsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_InstalledComponentsDetailsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_InstalledComponentsDetailsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_InstalledComponentsLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_InstalledComponentsLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_InstalledComponentsLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_InstalledComponentsEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_InstalledComponentsEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_InstalledComponentsEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_InstalledComponentsErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_InstalledComponentsErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_InstalledComponentsErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_InstalledComponentsSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_InstalledComponentsSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_InstalledComponentsSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_InstalledComponentsDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_InstalledComponentsDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_InstalledComponentsDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## ComponentDetail：组件详情与比较

功能文档：[组件详情与比较](ComponentDetail.md)；归属 `ComponentLibraryForm`；内容 1488×730。

```text
Panel_PageComponentDetail [Image]
  Txt_ComponentDetailTitle [TextMeshProUGUI]
  Grp_ComponentDetailActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_ComponentDetailCompare [Button + Image]
      Txt_ComponentDetailCompareLabel [TextMeshProUGUI]
    Btn_ComponentDetailUpgrade [Button + Image]
      Txt_ComponentDetailUpgradeLabel [TextMeshProUGUI]
    Btn_ComponentDetailLocate [Button + Image]
      Txt_ComponentDetailLocateLabel [TextMeshProUGUI]
    Btn_ComponentDetailLock [Button + Image]
      Txt_ComponentDetailLockLabel [TextMeshProUGUI]
  Panel_ComponentDetailIdentity [Image]
    Txt_ComponentDetailIdentityHeading [TextMeshProUGUI]
    List_ComponentDetailIdentity [ScrollRect vertical=true horizontal=false]
      Viewport_ComponentDetailIdentity [RectMask2D]
        Content_ComponentDetailIdentity [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ComponentDetailIdentityBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ComponentDetailIdentityTemplate [LayoutElement + Image；默认inactive]
            Btn_ComponentDetailIdentityRow [Button + Image]
              Txt_ComponentDetailIdentityRowLabel [TextMeshProUGUI]
              Txt_ComponentDetailIdentityRowValue [TextMeshProUGUI]
  Panel_ComponentDetailAttributes [Image]
    Txt_ComponentDetailAttributesHeading [TextMeshProUGUI]
    List_ComponentDetailAttributes [ScrollRect vertical=true horizontal=false]
      Viewport_ComponentDetailAttributes [RectMask2D]
        Content_ComponentDetailAttributes [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ComponentDetailAttributesBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ComponentDetailAttributesTemplate [LayoutElement + Image；默认inactive]
            Btn_ComponentDetailAttributesRow [Button + Image]
              Txt_ComponentDetailAttributesRowLabel [TextMeshProUGUI]
              Txt_ComponentDetailAttributesRowValue [TextMeshProUGUI]
  Panel_ComponentDetailCompare [Image]
    Txt_ComponentDetailCompareHeading [TextMeshProUGUI]
    List_ComponentDetailCompare [ScrollRect vertical=true horizontal=false]
      Viewport_ComponentDetailCompare [RectMask2D]
        Content_ComponentDetailCompare [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ComponentDetailCompareBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ComponentDetailCompareTemplate [LayoutElement + Image；默认inactive]
            Btn_ComponentDetailCompareRow [Button + Image]
              Txt_ComponentDetailCompareRowLabel [TextMeshProUGUI]
              Txt_ComponentDetailCompareRowValue [TextMeshProUGUI]
  Grp_ComponentDetailLoadingState [无Graphic]
    Panel_ComponentDetailLoadingMessage [Image]
      Txt_ComponentDetailLoadingMessage [TextMeshProUGUI]
  Grp_ComponentDetailEmptyState [无Graphic]
    Panel_ComponentDetailEmptyMessage [Image]
      Txt_ComponentDetailEmptyMessage [TextMeshProUGUI]
  Grp_ComponentDetailErrorState [无Graphic]
    Panel_ComponentDetailErrorMessage [Image]
      Txt_ComponentDetailErrorMessage [TextMeshProUGUI]
  Grp_ComponentDetailSuccessState [无Graphic]
    Panel_ComponentDetailSuccessMessage [Image]
      Txt_ComponentDetailSuccessMessage [TextMeshProUGUI]
  Grp_ComponentDetailDisabledState [无Graphic]
    Panel_ComponentDetailDisabledMessage [Image]
      Txt_ComponentDetailDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageComponentDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；组件详情与比较；内部页面根 |
| Txt_ComponentDetailTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；组件详情与比较 |
| Grp_ComponentDetailActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_ComponentDetailCompare | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；复用12组件选择器的只读比较模式 |
| Txt_ComponentDetailCompareLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；选择比较对象 |
| Btn_ComponentDetailUpgrade | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入11，保持选中实例 |
| Txt_ComponentDetailUpgradeLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看升级 |
| Btn_ComponentDetailLocate | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；已安装时定位机器；未安装显示在库 |
| Txt_ComponentDetailLocateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；定位安装位置 |
| Btn_ComponentDetailLock | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；按资产服务设置保护标记，不能覆盖业务硬锁 |
| Txt_ComponentDetailLockLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；锁定或解锁 |
| Panel_ComponentDetailIdentity | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(0,-36) | absolute | Image；组件资料 |
| Txt_ComponentDetailIdentityHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；组件资料 |
| List_ComponentDetailIdentity | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ComponentDetailIdentity | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ComponentDetailIdentity | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ComponentDetailIdentityBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；名称、型号、等级、实例ID对应显示身份；标签；所在位置 |
| Item_ComponentDetailIdentityTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ComponentDetailIdentityRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ComponentDetailIdentityRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ComponentDetailIdentityRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_ComponentDetailAttributes | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(501,-36) | absolute | Image；属性 |
| Txt_ComponentDetailAttributesHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；属性 |
| List_ComponentDetailAttributes | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ComponentDetailAttributes | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ComponentDetailAttributes | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ComponentDetailAttributesBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；能力、能耗、算力；载体和槽位兼容条件；锁定原因 |
| Item_ComponentDetailAttributesTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ComponentDetailAttributesRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ComponentDetailAttributesRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ComponentDetailAttributesRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_ComponentDetailCompare | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(1002,-36) | absolute | Image；比较对象 |
| Txt_ComponentDetailCompareHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；比较对象 |
| List_ComponentDetailCompare | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ComponentDetailCompare | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ComponentDetailCompare | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ComponentDetailCompareBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；当前组件与候选的逐项差异；单位一致；不能比较项明确标注 |
| Item_ComponentDetailCompareTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ComponentDetailCompareRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ComponentDetailCompareRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ComponentDetailCompareRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_ComponentDetailLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ComponentDetailLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ComponentDetailLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_ComponentDetailEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ComponentDetailEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ComponentDetailEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_ComponentDetailErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ComponentDetailErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ComponentDetailErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_ComponentDetailSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ComponentDetailSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ComponentDetailSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_ComponentDetailDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ComponentDetailDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ComponentDetailDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## UndeployedMachines：未部署机器

功能文档：[未部署机器](UndeployedMachines.md)；归属 `MachineLibraryForm`；内容 1488×730。

```text
Panel_PageUndeployedMachines [Image]
  Txt_UndeployedMachinesTitle [TextMeshProUGUI]
  Grp_UndeployedMachinesActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_UndeployedMachinesPrepare [Button + Image]
      Txt_UndeployedMachinesPrepareLabel [TextMeshProUGUI]
    Btn_UndeployedMachinesRename [Button + Image]
      Txt_UndeployedMachinesRenameLabel [TextMeshProUGUI]
    Btn_UndeployedMachinesDeploy [Button + Image]
      Txt_UndeployedMachinesDeployLabel [TextMeshProUGUI]
    Btn_UndeployedMachinesSell [Button + Image]
      Txt_UndeployedMachinesSellLabel [TextMeshProUGUI]
    Btn_UndeployedMachinesDeployed [Button + Image]
      Txt_UndeployedMachinesDeployedLabel [TextMeshProUGUI]
  Panel_UndeployedMachinesCatalog [Image]
    Txt_UndeployedMachinesCatalogHeading [TextMeshProUGUI]
    List_UndeployedMachinesCatalog [ScrollRect vertical=true horizontal=false]
      Viewport_UndeployedMachinesCatalog [RectMask2D]
        Content_UndeployedMachinesCatalog [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_UndeployedMachinesCatalogBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_UndeployedMachinesCatalogTemplate [LayoutElement + Image；默认inactive]
            Btn_UndeployedMachinesCatalogRow [Button + Image]
              Txt_UndeployedMachinesCatalogRowLabel [TextMeshProUGUI]
              Txt_UndeployedMachinesCatalogRowValue [TextMeshProUGUI]
  Panel_UndeployedMachinesDetail [Image]
    Txt_UndeployedMachinesDetailHeading [TextMeshProUGUI]
    List_UndeployedMachinesDetail [ScrollRect vertical=true horizontal=false]
      Viewport_UndeployedMachinesDetail [RectMask2D]
        Content_UndeployedMachinesDetail [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_UndeployedMachinesDetailBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_UndeployedMachinesDetailTemplate [LayoutElement + Image；默认inactive]
            Btn_UndeployedMachinesDetailRow [Button + Image]
              Txt_UndeployedMachinesDetailRowLabel [TextMeshProUGUI]
              Txt_UndeployedMachinesDetailRowValue [TextMeshProUGUI]
  Grp_UndeployedMachinesLoadingState [无Graphic]
    Panel_UndeployedMachinesLoadingMessage [Image]
      Txt_UndeployedMachinesLoadingMessage [TextMeshProUGUI]
  Grp_UndeployedMachinesEmptyState [无Graphic]
    Panel_UndeployedMachinesEmptyMessage [Image]
      Txt_UndeployedMachinesEmptyMessage [TextMeshProUGUI]
  Grp_UndeployedMachinesErrorState [无Graphic]
    Panel_UndeployedMachinesErrorMessage [Image]
      Txt_UndeployedMachinesErrorMessage [TextMeshProUGUI]
  Grp_UndeployedMachinesSuccessState [无Graphic]
    Panel_UndeployedMachinesSuccessMessage [Image]
      Txt_UndeployedMachinesSuccessMessage [TextMeshProUGUI]
  Grp_UndeployedMachinesDisabledState [无Graphic]
    Panel_UndeployedMachinesDisabledMessage [Image]
      Txt_UndeployedMachinesDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageUndeployedMachines | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；未部署机器；内部页面根 |
| Txt_UndeployedMachinesTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；未部署机器 |
| Grp_UndeployedMachinesActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_UndeployedMachinesPrepare | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入05-未部署机器整备 |
| Txt_UndeployedMachinesPrepareLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；整备机器 |
| Btn_UndeployedMachinesRename | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；17-重命名 |
| Txt_UndeployedMachinesRenameLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；重命名 |
| Btn_UndeployedMachinesDeploy | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；14-机器部署 |
| Txt_UndeployedMachinesDeployLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；部署 |
| Btn_UndeployedMachinesSell | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；仅空载完好载体，17交易确认 |
| Txt_UndeployedMachinesSellLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；出售 |
| Btn_UndeployedMachinesDeployed | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；09-已部署机器 |
| Txt_UndeployedMachinesDeployedLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；已部署分页 |
| Panel_UndeployedMachinesCatalog | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；独立机器列表 |
| Txt_UndeployedMachinesCatalogHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；独立机器列表 |
| List_UndeployedMachinesCatalog | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_UndeployedMachinesCatalog | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_UndeployedMachinesCatalog | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_UndeployedMachinesCatalogBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；名称；型号；等级；组件配置；未部署状态 |
| Item_UndeployedMachinesCatalogTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_UndeployedMachinesCatalogRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_UndeployedMachinesCatalogRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_UndeployedMachinesCatalogRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_UndeployedMachinesDetail | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；机器摘要 |
| Txt_UndeployedMachinesDetailHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；机器摘要 |
| List_UndeployedMachinesDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_UndeployedMachinesDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_UndeployedMachinesDetail | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_UndeployedMachinesDetailBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；容量；已安装组件；整备入口；部署条件；是否空载完好 |
| Item_UndeployedMachinesDetailTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_UndeployedMachinesDetailRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_UndeployedMachinesDetailRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_UndeployedMachinesDetailRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_UndeployedMachinesLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_UndeployedMachinesLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_UndeployedMachinesLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_UndeployedMachinesEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_UndeployedMachinesEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_UndeployedMachinesEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_UndeployedMachinesErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_UndeployedMachinesErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_UndeployedMachinesErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_UndeployedMachinesSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_UndeployedMachinesSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_UndeployedMachinesSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_UndeployedMachinesDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_UndeployedMachinesDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_UndeployedMachinesDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## DeployedMachines：已部署机器

功能文档：[已部署机器](DeployedMachines.md)；归属 `MachineLibraryForm`；内容 1488×730。

```text
Panel_PageDeployedMachines [Image]
  Txt_DeployedMachinesTitle [TextMeshProUGUI]
  Grp_DeployedMachinesActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_DeployedMachinesLocate [Button + Image]
      Txt_DeployedMachinesLocateLabel [TextMeshProUGUI]
    Btn_DeployedMachinesHub [Button + Image]
      Txt_DeployedMachinesHubLabel [TextMeshProUGUI]
    Btn_DeployedMachinesRename [Button + Image]
      Txt_DeployedMachinesRenameLabel [TextMeshProUGUI]
    Btn_DeployedMachinesUndeployed [Button + Image]
      Txt_DeployedMachinesUndeployedLabel [TextMeshProUGUI]
  Panel_DeployedMachinesCatalog [Image]
    Txt_DeployedMachinesCatalogHeading [TextMeshProUGUI]
    List_DeployedMachinesCatalog [ScrollRect vertical=true horizontal=false]
      Viewport_DeployedMachinesCatalog [RectMask2D]
        Content_DeployedMachinesCatalog [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_DeployedMachinesCatalogBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_DeployedMachinesCatalogTemplate [LayoutElement + Image；默认inactive]
            Btn_DeployedMachinesCatalogRow [Button + Image]
              Txt_DeployedMachinesCatalogRowLabel [TextMeshProUGUI]
              Txt_DeployedMachinesCatalogRowValue [TextMeshProUGUI]
  Panel_DeployedMachinesDetail [Image]
    Txt_DeployedMachinesDetailHeading [TextMeshProUGUI]
    List_DeployedMachinesDetail [ScrollRect vertical=true horizontal=false]
      Viewport_DeployedMachinesDetail [RectMask2D]
        Content_DeployedMachinesDetail [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_DeployedMachinesDetailBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_DeployedMachinesDetailTemplate [LayoutElement + Image；默认inactive]
            Btn_DeployedMachinesDetailRow [Button + Image]
              Txt_DeployedMachinesDetailRowLabel [TextMeshProUGUI]
              Txt_DeployedMachinesDetailRowValue [TextMeshProUGUI]
  Grp_DeployedMachinesLoadingState [无Graphic]
    Panel_DeployedMachinesLoadingMessage [Image]
      Txt_DeployedMachinesLoadingMessage [TextMeshProUGUI]
  Grp_DeployedMachinesEmptyState [无Graphic]
    Panel_DeployedMachinesEmptyMessage [Image]
      Txt_DeployedMachinesEmptyMessage [TextMeshProUGUI]
  Grp_DeployedMachinesErrorState [无Graphic]
    Panel_DeployedMachinesErrorMessage [Image]
      Txt_DeployedMachinesErrorMessage [TextMeshProUGUI]
  Grp_DeployedMachinesSuccessState [无Graphic]
    Panel_DeployedMachinesSuccessMessage [Image]
      Txt_DeployedMachinesSuccessMessage [TextMeshProUGUI]
  Grp_DeployedMachinesDisabledState [无Graphic]
    Panel_DeployedMachinesDisabledMessage [Image]
      Txt_DeployedMachinesDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageDeployedMachines | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；已部署机器；内部页面根 |
| Txt_DeployedMachinesTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；已部署机器 |
| Grp_DeployedMachinesActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_DeployedMachinesLocate | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；聚焦机器后打开现场面板 |
| Txt_DeployedMachinesLocateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；前往现场 |
| Btn_DeployedMachinesHub | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；05-中枢机器详情 |
| Txt_DeployedMachinesHubLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；中枢详情 |
| Btn_DeployedMachinesRename | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；17-重命名 |
| Txt_DeployedMachinesRenameLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；重命名 |
| Btn_DeployedMachinesUndeployed | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；09-未部署机器 |
| Txt_DeployedMachinesUndeployedLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；未部署分页 |
| Panel_DeployedMachinesCatalog | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；已部署索引 |
| Txt_DeployedMachinesCatalogHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；已部署索引 |
| List_DeployedMachinesCatalog | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_DeployedMachinesCatalog | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_DeployedMachinesCatalog | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_DeployedMachinesCatalogBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；名称；型号；等级；位置；激活／运行／异常状态 |
| Item_DeployedMachinesCatalogTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_DeployedMachinesCatalogRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_DeployedMachinesCatalogRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_DeployedMachinesCatalogRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_DeployedMachinesDetail | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；所选机器 |
| Txt_DeployedMachinesDetailHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；所选机器 |
| List_DeployedMachinesDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_DeployedMachinesDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_DeployedMachinesDetail | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_DeployedMachinesDetailBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；组件摘要；当前任务；主要等待；可用远程入口 |
| Item_DeployedMachinesDetailTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_DeployedMachinesDetailRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_DeployedMachinesDetailRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_DeployedMachinesDetailRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_DeployedMachinesLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DeployedMachinesLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DeployedMachinesLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_DeployedMachinesEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DeployedMachinesEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DeployedMachinesEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_DeployedMachinesErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DeployedMachinesErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DeployedMachinesErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_DeployedMachinesSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DeployedMachinesSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DeployedMachinesSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_DeployedMachinesDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DeployedMachinesDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DeployedMachinesDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## Inventory：仓库分类库存

功能文档：[仓库分类库存](Inventory.md)；归属 `WarehouseForm`；内容 1488×730。

```text
Panel_PageInventory [Image]
  Txt_InventoryTitle [TextMeshProUGUI]
  Grp_InventoryActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_InventoryInspect [Button + Image]
      Txt_InventoryInspectLabel [TextMeshProUGUI]
    Btn_InventoryRecords [Button + Image]
      Txt_InventoryRecordsLabel [TextMeshProUGUI]
    Btn_InventorySell [Button + Image]
      Txt_InventorySellLabel [TextMeshProUGUI]
  Panel_InventoryCatalog [Image]
    Txt_InventoryCatalogHeading [TextMeshProUGUI]
    List_InventoryCatalog [ScrollRect vertical=true horizontal=false]
      Viewport_InventoryCatalog [RectMask2D]
        Content_InventoryCatalog [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_InventoryCatalogBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_InventoryCatalogTemplate [LayoutElement + Image；默认inactive]
            Btn_InventoryCatalogRow [Button + Image]
              Txt_InventoryCatalogRowLabel [TextMeshProUGUI]
              Txt_InventoryCatalogRowValue [TextMeshProUGUI]
  Panel_InventoryDetail [Image]
    Txt_InventoryDetailHeading [TextMeshProUGUI]
    List_InventoryDetail [ScrollRect vertical=true horizontal=false]
      Viewport_InventoryDetail [RectMask2D]
        Content_InventoryDetail [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_InventoryDetailBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_InventoryDetailTemplate [LayoutElement + Image；默认inactive]
            Btn_InventoryDetailRow [Button + Image]
              Txt_InventoryDetailRowLabel [TextMeshProUGUI]
              Txt_InventoryDetailRowValue [TextMeshProUGUI]
  Grp_InventoryLoadingState [无Graphic]
    Panel_InventoryLoadingMessage [Image]
      Txt_InventoryLoadingMessage [TextMeshProUGUI]
  Grp_InventoryEmptyState [无Graphic]
    Panel_InventoryEmptyMessage [Image]
      Txt_InventoryEmptyMessage [TextMeshProUGUI]
  Grp_InventoryErrorState [无Graphic]
    Panel_InventoryErrorMessage [Image]
      Txt_InventoryErrorMessage [TextMeshProUGUI]
  Grp_InventorySuccessState [无Graphic]
    Panel_InventorySuccessMessage [Image]
      Txt_InventorySuccessMessage [TextMeshProUGUI]
  Grp_InventoryDisabledState [无Graphic]
    Panel_InventoryDisabledMessage [Image]
      Txt_InventoryDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageInventory | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；仓库分类库存；内部页面根 |
| Txt_InventoryTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；仓库分类库存 |
| Grp_InventoryActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_InventoryInspect | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；09-库存物品详情 |
| Txt_InventoryInspectLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；物品详情 |
| Btn_InventoryRecords | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；09-仓库记录 |
| Txt_InventoryRecordsLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；入库记录 |
| Btn_InventorySell | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；10-商店出售，携带物品类型；非主基地基础仓库不可跨仓销售 |
| Txt_InventorySellLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；前往出售 |
| Panel_InventoryCatalog | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；分类库存 |
| Txt_InventoryCatalogHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；分类库存 |
| List_InventoryCatalog | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_InventoryCatalog | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_InventoryCatalog | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_InventoryCatalogBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；物品类别；同类型同品质／等级堆叠；数量；容量占用 |
| Item_InventoryCatalogTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_InventoryCatalogRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_InventoryCatalogRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_InventoryCatalogRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_InventoryDetail | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；物品摘要 |
| Txt_InventoryDetailHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；物品摘要 |
| List_InventoryDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_InventoryDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_InventoryDetail | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_InventoryDetailBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；名称、用途、标签；实际回收价；数量；是否可出售 |
| Item_InventoryDetailTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_InventoryDetailRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_InventoryDetailRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_InventoryDetailRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_InventoryLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_InventoryLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_InventoryLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_InventoryEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_InventoryEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_InventoryEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_InventoryErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_InventoryErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_InventoryErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_InventorySuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_InventorySuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_InventorySuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_InventoryDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_InventoryDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_InventoryDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## ItemDetail：库存物品详情

功能文档：[库存物品详情](ItemDetail.md)；归属 `WarehouseForm`；内容 1488×730。

```text
Panel_PageItemDetail [Image]
  Txt_ItemDetailTitle [TextMeshProUGUI]
  Grp_ItemDetailActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_ItemDetailSell [Button + Image]
      Txt_ItemDetailSellLabel [TextMeshProUGUI]
    Btn_ItemDetailBack [Button + Image]
      Txt_ItemDetailBackLabel [TextMeshProUGUI]
  Panel_ItemDetailItem [Image]
    Txt_ItemDetailItemHeading [TextMeshProUGUI]
    List_ItemDetailItem [ScrollRect vertical=true horizontal=false]
      Viewport_ItemDetailItem [RectMask2D]
        Content_ItemDetailItem [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ItemDetailItemBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ItemDetailItemTemplate [LayoutElement + Image；默认inactive]
            Btn_ItemDetailItemRow [Button + Image]
              Txt_ItemDetailItemRowLabel [TextMeshProUGUI]
              Txt_ItemDetailItemRowValue [TextMeshProUGUI]
  Panel_ItemDetailStock [Image]
    Txt_ItemDetailStockHeading [TextMeshProUGUI]
    List_ItemDetailStock [ScrollRect vertical=true horizontal=false]
      Viewport_ItemDetailStock [RectMask2D]
        Content_ItemDetailStock [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ItemDetailStockBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ItemDetailStockTemplate [LayoutElement + Image；默认inactive]
            Btn_ItemDetailStockRow [Button + Image]
              Txt_ItemDetailStockRowLabel [TextMeshProUGUI]
              Txt_ItemDetailStockRowValue [TextMeshProUGUI]
  Grp_ItemDetailLoadingState [无Graphic]
    Panel_ItemDetailLoadingMessage [Image]
      Txt_ItemDetailLoadingMessage [TextMeshProUGUI]
  Grp_ItemDetailEmptyState [无Graphic]
    Panel_ItemDetailEmptyMessage [Image]
      Txt_ItemDetailEmptyMessage [TextMeshProUGUI]
  Grp_ItemDetailErrorState [无Graphic]
    Panel_ItemDetailErrorMessage [Image]
      Txt_ItemDetailErrorMessage [TextMeshProUGUI]
  Grp_ItemDetailSuccessState [无Graphic]
    Panel_ItemDetailSuccessMessage [Image]
      Txt_ItemDetailSuccessMessage [TextMeshProUGUI]
  Grp_ItemDetailDisabledState [无Graphic]
    Panel_ItemDetailDisabledMessage [Image]
      Txt_ItemDetailDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageItemDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；库存物品详情；内部页面根 |
| Txt_ItemDetailTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；库存物品详情 |
| Grp_ItemDetailActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_ItemDetailSell | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；10出售页并带当前物品筛选 |
| Txt_ItemDetailSellLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；前往出售 |
| Btn_ItemDetailBack | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；保持分类、选中堆叠与滚动位置 |
| Txt_ItemDetailBackLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；返回库存 |
| Panel_ItemDetailItem | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；物品说明 |
| Txt_ItemDetailItemHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；物品说明 |
| List_ItemDetailItem | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ItemDetailItem | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ItemDetailItem | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ItemDetailItemBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；名称、说明、标签、品质／等级、用途 |
| Item_ItemDetailItemTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ItemDetailItemRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ItemDetailItemRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ItemDetailItemRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_ItemDetailStock | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；数量与回收 |
| Txt_ItemDetailStockHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；数量与回收 |
| List_ItemDetailStock | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ItemDetailStock | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ItemDetailStock | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ItemDetailStockBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；当前数量；所在仓库；实际回收单价；出售条件 |
| Item_ItemDetailStockTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ItemDetailStockRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ItemDetailStockRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ItemDetailStockRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_ItemDetailLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ItemDetailLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ItemDetailLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_ItemDetailEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ItemDetailEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ItemDetailEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_ItemDetailErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ItemDetailErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ItemDetailErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_ItemDetailSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ItemDetailSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ItemDetailSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_ItemDetailDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ItemDetailDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ItemDetailDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## WarehouseRecords：仓库记录

功能文档：[仓库记录](WarehouseRecords.md)；归属 `WarehouseForm`；内容 1488×730。

```text
Panel_PageWarehouseRecords [Image]
  Txt_WarehouseRecordsTitle [TextMeshProUGUI]
  Grp_WarehouseRecordsActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_WarehouseRecordsFilter [Button + Image]
      Txt_WarehouseRecordsFilterLabel [TextMeshProUGUI]
    Btn_WarehouseRecordsLocate [Button + Image]
      Txt_WarehouseRecordsLocateLabel [TextMeshProUGUI]
    Btn_WarehouseRecordsDestination [Button + Image]
      Txt_WarehouseRecordsDestinationLabel [TextMeshProUGUI]
  Panel_WarehouseRecordsEvents [Image]
    Txt_WarehouseRecordsEventsHeading [TextMeshProUGUI]
    List_WarehouseRecordsEvents [ScrollRect vertical=true horizontal=false]
      Viewport_WarehouseRecordsEvents [RectMask2D]
        Content_WarehouseRecordsEvents [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_WarehouseRecordsEventsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_WarehouseRecordsEventsTemplate [LayoutElement + Image；默认inactive]
            Btn_WarehouseRecordsEventsRow [Button + Image]
              Txt_WarehouseRecordsEventsRowLabel [TextMeshProUGUI]
              Txt_WarehouseRecordsEventsRowValue [TextMeshProUGUI]
  Panel_WarehouseRecordsDetail [Image]
    Txt_WarehouseRecordsDetailHeading [TextMeshProUGUI]
    List_WarehouseRecordsDetail [ScrollRect vertical=true horizontal=false]
      Viewport_WarehouseRecordsDetail [RectMask2D]
        Content_WarehouseRecordsDetail [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_WarehouseRecordsDetailBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_WarehouseRecordsDetailTemplate [LayoutElement + Image；默认inactive]
            Btn_WarehouseRecordsDetailRow [Button + Image]
              Txt_WarehouseRecordsDetailRowLabel [TextMeshProUGUI]
              Txt_WarehouseRecordsDetailRowValue [TextMeshProUGUI]
  Grp_WarehouseRecordsLoadingState [无Graphic]
    Panel_WarehouseRecordsLoadingMessage [Image]
      Txt_WarehouseRecordsLoadingMessage [TextMeshProUGUI]
  Grp_WarehouseRecordsEmptyState [无Graphic]
    Panel_WarehouseRecordsEmptyMessage [Image]
      Txt_WarehouseRecordsEmptyMessage [TextMeshProUGUI]
  Grp_WarehouseRecordsErrorState [无Graphic]
    Panel_WarehouseRecordsErrorMessage [Image]
      Txt_WarehouseRecordsErrorMessage [TextMeshProUGUI]
  Grp_WarehouseRecordsSuccessState [无Graphic]
    Panel_WarehouseRecordsSuccessMessage [Image]
      Txt_WarehouseRecordsSuccessMessage [TextMeshProUGUI]
  Grp_WarehouseRecordsDisabledState [无Graphic]
    Panel_WarehouseRecordsDisabledMessage [Image]
      Txt_WarehouseRecordsDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageWarehouseRecords | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；仓库记录；内部页面根 |
| Txt_WarehouseRecordsTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；仓库记录 |
| Grp_WarehouseRecordsActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_WarehouseRecordsFilter | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；按事件类型和来源过滤 |
| Txt_WarehouseRecordsFilterLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；筛选记录 |
| Btn_WarehouseRecordsLocate | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；有效则定位机器或设施，失效则说明 |
| Txt_WarehouseRecordsLocateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；定位来源 |
| Btn_WarehouseRecordsDestination | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；组件→组件库；载体→机器库；物品→本库存 |
| Txt_WarehouseRecordsDestinationLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看去向 |
| Panel_WarehouseRecordsEvents | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；事件列表 |
| Txt_WarehouseRecordsEventsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；事件列表 |
| List_WarehouseRecordsEvents | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_WarehouseRecordsEvents | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_WarehouseRecordsEvents | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_WarehouseRecordsEventsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；时间；来源；入库、自动分类、卸货失败；数量与结果 |
| Item_WarehouseRecordsEventsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_WarehouseRecordsEventsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_WarehouseRecordsEventsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_WarehouseRecordsEventsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_WarehouseRecordsDetail | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；事件详情 |
| Txt_WarehouseRecordsDetailHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；事件详情 |
| List_WarehouseRecordsDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_WarehouseRecordsDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_WarehouseRecordsDetail | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_WarehouseRecordsDetailBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；物品；分类后去向；机器／请求关联；失败原因 |
| Item_WarehouseRecordsDetailTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_WarehouseRecordsDetailRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_WarehouseRecordsDetailRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_WarehouseRecordsDetailRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_WarehouseRecordsLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WarehouseRecordsLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WarehouseRecordsLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_WarehouseRecordsEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WarehouseRecordsEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WarehouseRecordsEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_WarehouseRecordsErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WarehouseRecordsErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WarehouseRecordsErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_WarehouseRecordsSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WarehouseRecordsSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WarehouseRecordsSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_WarehouseRecordsDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WarehouseRecordsDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WarehouseRecordsDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## SystemTemplates：系统算法模板

功能文档：[系统算法模板](SystemTemplates.md)；归属 `AlgorithmLibraryForm`；内容 1488×730。

```text
Panel_PageSystemTemplates [Image]
  Txt_SystemTemplatesTitle [TextMeshProUGUI]
  Grp_SystemTemplatesActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_SystemTemplatesDetails [Button + Image]
      Txt_SystemTemplatesDetailsLabel [TextMeshProUGUI]
    Btn_SystemTemplatesCreate [Button + Image]
      Txt_SystemTemplatesCreateLabel [TextMeshProUGUI]
    Btn_SystemTemplatesCopy [Button + Image]
      Txt_SystemTemplatesCopyLabel [TextMeshProUGUI]
    Btn_SystemTemplatesTabs [Button + Image]
      Txt_SystemTemplatesTabsLabel [TextMeshProUGUI]
  Panel_SystemTemplatesCatalog [Image]
    Txt_SystemTemplatesCatalogHeading [TextMeshProUGUI]
    List_SystemTemplatesCatalog [ScrollRect vertical=true horizontal=false]
      Viewport_SystemTemplatesCatalog [RectMask2D]
        Content_SystemTemplatesCatalog [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_SystemTemplatesCatalogBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_SystemTemplatesCatalogTemplate [LayoutElement + Image；默认inactive]
            Btn_SystemTemplatesCatalogRow [Button + Image]
              Txt_SystemTemplatesCatalogRowLabel [TextMeshProUGUI]
              Txt_SystemTemplatesCatalogRowValue [TextMeshProUGUI]
  Panel_SystemTemplatesDetail [Image]
    Txt_SystemTemplatesDetailHeading [TextMeshProUGUI]
    List_SystemTemplatesDetail [ScrollRect vertical=true horizontal=false]
      Viewport_SystemTemplatesDetail [RectMask2D]
        Content_SystemTemplatesDetail [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_SystemTemplatesDetailBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_SystemTemplatesDetailTemplate [LayoutElement + Image；默认inactive]
            Btn_SystemTemplatesDetailRow [Button + Image]
              Txt_SystemTemplatesDetailRowLabel [TextMeshProUGUI]
              Txt_SystemTemplatesDetailRowValue [TextMeshProUGUI]
  Grp_SystemTemplatesLoadingState [无Graphic]
    Panel_SystemTemplatesLoadingMessage [Image]
      Txt_SystemTemplatesLoadingMessage [TextMeshProUGUI]
  Grp_SystemTemplatesEmptyState [无Graphic]
    Panel_SystemTemplatesEmptyMessage [Image]
      Txt_SystemTemplatesEmptyMessage [TextMeshProUGUI]
  Grp_SystemTemplatesErrorState [无Graphic]
    Panel_SystemTemplatesErrorMessage [Image]
      Txt_SystemTemplatesErrorMessage [TextMeshProUGUI]
  Grp_SystemTemplatesSuccessState [无Graphic]
    Panel_SystemTemplatesSuccessMessage [Image]
      Txt_SystemTemplatesSuccessMessage [TextMeshProUGUI]
  Grp_SystemTemplatesDisabledState [无Graphic]
    Panel_SystemTemplatesDisabledMessage [Image]
      Txt_SystemTemplatesDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageSystemTemplates | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；系统算法模板；内部页面根 |
| Txt_SystemTemplatesTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；系统算法模板 |
| Grp_SystemTemplatesActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_SystemTemplatesDetails | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；09-模板详情 |
| Txt_SystemTemplatesDetailsLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；模板详情 |
| Btn_SystemTemplatesCreate | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；条件满足后12-集中待绑定，再13编辑器检查应用 |
| Txt_SystemTemplatesCreateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；创建实例 |
| Btn_SystemTemplatesCopy | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；复制为玩家模板，进入17重命名 |
| Txt_SystemTemplatesCopyLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；复制模板 |
| Btn_SystemTemplatesTabs | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；系统／玩家分页切换 |
| Txt_SystemTemplatesTabsLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；切换来源 |
| Panel_SystemTemplatesCatalog | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；模板目录 |
| Txt_SystemTemplatesCatalogHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；模板目录 |
| List_SystemTemplatesCatalog | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_SystemTemplatesCatalog | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_SystemTemplatesCatalog | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_SystemTemplatesCatalogBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；系统模板名称；用途；来源；逻辑成本；能力需求 |
| Item_SystemTemplatesCatalogTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_SystemTemplatesCatalogRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_SystemTemplatesCatalogRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_SystemTemplatesCatalogRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_SystemTemplatesDetail | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；模板条件 |
| Txt_SystemTemplatesDetailHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；模板条件 |
| List_SystemTemplatesDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_SystemTemplatesDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_SystemTemplatesDetail | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_SystemTemplatesDetailBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；目标机器；缺失组件或能力；环境兼容性；模板内容摘要 |
| Item_SystemTemplatesDetailTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_SystemTemplatesDetailRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_SystemTemplatesDetailRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_SystemTemplatesDetailRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_SystemTemplatesLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SystemTemplatesLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SystemTemplatesLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_SystemTemplatesEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SystemTemplatesEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SystemTemplatesEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_SystemTemplatesErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SystemTemplatesErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SystemTemplatesErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_SystemTemplatesSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SystemTemplatesSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SystemTemplatesSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_SystemTemplatesDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SystemTemplatesDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SystemTemplatesDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## PlayerTemplates：玩家算法模板

功能文档：[玩家算法模板](PlayerTemplates.md)；归属 `AlgorithmLibraryForm`；内容 1488×730。

```text
Panel_PagePlayerTemplates [Image]
  Txt_PlayerTemplatesTitle [TextMeshProUGUI]
  Grp_PlayerTemplatesActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_PlayerTemplatesDetails [Button + Image]
      Txt_PlayerTemplatesDetailsLabel [TextMeshProUGUI]
    Btn_PlayerTemplatesCreate [Button + Image]
      Txt_PlayerTemplatesCreateLabel [TextMeshProUGUI]
    Btn_PlayerTemplatesCopy [Button + Image]
      Txt_PlayerTemplatesCopyLabel [TextMeshProUGUI]
    Btn_PlayerTemplatesTabs [Button + Image]
      Txt_PlayerTemplatesTabsLabel [TextMeshProUGUI]
  Panel_PlayerTemplatesCatalog [Image]
    Txt_PlayerTemplatesCatalogHeading [TextMeshProUGUI]
    List_PlayerTemplatesCatalog [ScrollRect vertical=true horizontal=false]
      Viewport_PlayerTemplatesCatalog [RectMask2D]
        Content_PlayerTemplatesCatalog [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_PlayerTemplatesCatalogBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_PlayerTemplatesCatalogTemplate [LayoutElement + Image；默认inactive]
            Btn_PlayerTemplatesCatalogRow [Button + Image]
              Txt_PlayerTemplatesCatalogRowLabel [TextMeshProUGUI]
              Txt_PlayerTemplatesCatalogRowValue [TextMeshProUGUI]
  Panel_PlayerTemplatesDetail [Image]
    Txt_PlayerTemplatesDetailHeading [TextMeshProUGUI]
    List_PlayerTemplatesDetail [ScrollRect vertical=true horizontal=false]
      Viewport_PlayerTemplatesDetail [RectMask2D]
        Content_PlayerTemplatesDetail [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_PlayerTemplatesDetailBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_PlayerTemplatesDetailTemplate [LayoutElement + Image；默认inactive]
            Btn_PlayerTemplatesDetailRow [Button + Image]
              Txt_PlayerTemplatesDetailRowLabel [TextMeshProUGUI]
              Txt_PlayerTemplatesDetailRowValue [TextMeshProUGUI]
  Grp_PlayerTemplatesLoadingState [无Graphic]
    Panel_PlayerTemplatesLoadingMessage [Image]
      Txt_PlayerTemplatesLoadingMessage [TextMeshProUGUI]
  Grp_PlayerTemplatesEmptyState [无Graphic]
    Panel_PlayerTemplatesEmptyMessage [Image]
      Txt_PlayerTemplatesEmptyMessage [TextMeshProUGUI]
  Grp_PlayerTemplatesErrorState [无Graphic]
    Panel_PlayerTemplatesErrorMessage [Image]
      Txt_PlayerTemplatesErrorMessage [TextMeshProUGUI]
  Grp_PlayerTemplatesSuccessState [无Graphic]
    Panel_PlayerTemplatesSuccessMessage [Image]
      Txt_PlayerTemplatesSuccessMessage [TextMeshProUGUI]
  Grp_PlayerTemplatesDisabledState [无Graphic]
    Panel_PlayerTemplatesDisabledMessage [Image]
      Txt_PlayerTemplatesDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PagePlayerTemplates | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；玩家算法模板；内部页面根 |
| Txt_PlayerTemplatesTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；玩家算法模板 |
| Grp_PlayerTemplatesActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_PlayerTemplatesDetails | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；09-模板详情 |
| Txt_PlayerTemplatesDetailsLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；模板详情 |
| Btn_PlayerTemplatesCreate | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；条件满足后12-集中待绑定，再13编辑器检查应用 |
| Txt_PlayerTemplatesCreateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；创建实例 |
| Btn_PlayerTemplatesCopy | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；复制为玩家模板，进入17重命名 |
| Txt_PlayerTemplatesCopyLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；复制模板 |
| Btn_PlayerTemplatesTabs | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；系统／玩家分页切换 |
| Txt_PlayerTemplatesTabsLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；切换来源 |
| Panel_PlayerTemplatesCatalog | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；模板目录 |
| Txt_PlayerTemplatesCatalogHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；模板目录 |
| List_PlayerTemplatesCatalog | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_PlayerTemplatesCatalog | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_PlayerTemplatesCatalog | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_PlayerTemplatesCatalogBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；玩家模板名称；用途；来源；逻辑成本；能力需求 |
| Item_PlayerTemplatesCatalogTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_PlayerTemplatesCatalogRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_PlayerTemplatesCatalogRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_PlayerTemplatesCatalogRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_PlayerTemplatesDetail | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；模板条件 |
| Txt_PlayerTemplatesDetailHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；模板条件 |
| List_PlayerTemplatesDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_PlayerTemplatesDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_PlayerTemplatesDetail | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_PlayerTemplatesDetailBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；目标机器；缺失组件或能力；环境兼容性；模板内容摘要 |
| Item_PlayerTemplatesDetailTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_PlayerTemplatesDetailRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_PlayerTemplatesDetailRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_PlayerTemplatesDetailRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_PlayerTemplatesLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_PlayerTemplatesLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_PlayerTemplatesLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_PlayerTemplatesEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_PlayerTemplatesEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_PlayerTemplatesEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_PlayerTemplatesErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_PlayerTemplatesErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_PlayerTemplatesErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_PlayerTemplatesSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_PlayerTemplatesSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_PlayerTemplatesSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_PlayerTemplatesDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_PlayerTemplatesDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_PlayerTemplatesDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## TemplateDetail：算法模板详情

功能文档：[算法模板详情](TemplateDetail.md)；归属 `AlgorithmLibraryForm`；内容 1488×730。

```text
Panel_PageTemplateDetail [Image]
  Txt_TemplateDetailTitle [TextMeshProUGUI]
  Grp_TemplateDetailActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_TemplateDetailCreate [Button + Image]
      Txt_TemplateDetailCreateLabel [TextMeshProUGUI]
    Btn_TemplateDetailCopy [Button + Image]
      Txt_TemplateDetailCopyLabel [TextMeshProUGUI]
    Btn_TemplateDetailRename [Button + Image]
      Txt_TemplateDetailRenameLabel [TextMeshProUGUI]
    Btn_TemplateDetailDelete [Button + Image]
      Txt_TemplateDetailDeleteLabel [TextMeshProUGUI]
  Panel_TemplateDetailDefinition [Image]
    Txt_TemplateDetailDefinitionHeading [TextMeshProUGUI]
    List_TemplateDetailDefinition [ScrollRect vertical=true horizontal=false]
      Viewport_TemplateDetailDefinition [RectMask2D]
        Content_TemplateDetailDefinition [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_TemplateDetailDefinitionBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_TemplateDetailDefinitionTemplate [LayoutElement + Image；默认inactive]
            Btn_TemplateDetailDefinitionRow [Button + Image]
              Txt_TemplateDetailDefinitionRowLabel [TextMeshProUGUI]
              Txt_TemplateDetailDefinitionRowValue [TextMeshProUGUI]
  Panel_TemplateDetailRequirements [Image]
    Txt_TemplateDetailRequirementsHeading [TextMeshProUGUI]
    List_TemplateDetailRequirements [ScrollRect vertical=true horizontal=false]
      Viewport_TemplateDetailRequirements [RectMask2D]
        Content_TemplateDetailRequirements [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_TemplateDetailRequirementsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_TemplateDetailRequirementsTemplate [LayoutElement + Image；默认inactive]
            Btn_TemplateDetailRequirementsRow [Button + Image]
              Txt_TemplateDetailRequirementsRowLabel [TextMeshProUGUI]
              Txt_TemplateDetailRequirementsRowValue [TextMeshProUGUI]
  Grp_TemplateDetailLoadingState [无Graphic]
    Panel_TemplateDetailLoadingMessage [Image]
      Txt_TemplateDetailLoadingMessage [TextMeshProUGUI]
  Grp_TemplateDetailEmptyState [无Graphic]
    Panel_TemplateDetailEmptyMessage [Image]
      Txt_TemplateDetailEmptyMessage [TextMeshProUGUI]
  Grp_TemplateDetailErrorState [无Graphic]
    Panel_TemplateDetailErrorMessage [Image]
      Txt_TemplateDetailErrorMessage [TextMeshProUGUI]
  Grp_TemplateDetailSuccessState [无Graphic]
    Panel_TemplateDetailSuccessMessage [Image]
      Txt_TemplateDetailSuccessMessage [TextMeshProUGUI]
  Grp_TemplateDetailDisabledState [无Graphic]
    Panel_TemplateDetailDisabledMessage [Image]
      Txt_TemplateDetailDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageTemplateDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；算法模板详情；内部页面根 |
| Txt_TemplateDetailTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；算法模板详情 |
| Grp_TemplateDetailActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_TemplateDetailCreate | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入12集中待绑定，绝不自动替玩家选择对象 |
| Txt_TemplateDetailCreateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；创建实例 |
| Btn_TemplateDetailCopy | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；创建玩家副本 |
| Txt_TemplateDetailCopyLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；复制 |
| Btn_TemplateDetailRename | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；仅玩家模板→17输入 |
| Txt_TemplateDetailRenameLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；重命名 |
| Btn_TemplateDetailDelete | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；仅玩家模板→17强确认 |
| Txt_TemplateDetailDeleteLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；删除模板 |
| Panel_TemplateDetailDefinition | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；模板信息 |
| Txt_TemplateDetailDefinitionHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；模板信息 |
| List_TemplateDetailDefinition | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_TemplateDetailDefinition | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_TemplateDetailDefinition | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_TemplateDetailDefinitionBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；名称；说明；来源；逻辑成本；所需能力；公开参数定义 |
| Item_TemplateDetailDefinitionTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_TemplateDetailDefinitionRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_TemplateDetailDefinitionRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_TemplateDetailDefinitionRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_TemplateDetailRequirements | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；使用条件 |
| Txt_TemplateDetailRequirementsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；使用条件 |
| List_TemplateDetailRequirements | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_TemplateDetailRequirements | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_TemplateDetailRequirements | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_TemplateDetailRequirementsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；目标机器能力差异；待绑定组件／世界对象；系统模板只读标识 |
| Item_TemplateDetailRequirementsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_TemplateDetailRequirementsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_TemplateDetailRequirementsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_TemplateDetailRequirementsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_TemplateDetailLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_TemplateDetailLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_TemplateDetailLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_TemplateDetailEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_TemplateDetailEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_TemplateDetailEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_TemplateDetailErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_TemplateDetailErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_TemplateDetailErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_TemplateDetailSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_TemplateDetailSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_TemplateDetailSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_TemplateDetailDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_TemplateDetailDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_TemplateDetailDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |
