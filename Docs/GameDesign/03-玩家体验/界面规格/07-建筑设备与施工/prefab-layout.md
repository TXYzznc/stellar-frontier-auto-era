# 07-建筑设备与施工 — prefab-layout

状态：2026-09-19完整设计候选，待用户集中验收；未据此修改Prefab。

继承[通用合同](../00-通用合同.md)和[共享外壳](../00-共享外壳-prefab-layout.md)。下列子树预制在所属Form的Grp_PageHost中；公共外壳由共享文档唯一声明。跨家族同属一个Form的子树合并，禁止重复根节点。表中所有Image／文本颜色、资源、射线、默认字号与状态继承通用合同，列出的数据是字段说明，不是示例业务数值。

## BuildingOverview：通用建筑现场面板

功能文档：[通用建筑现场面板](BuildingOverview.md)；归属 `FieldHudForm`；内容 520×832。

```text
Panel_PageBuildingOverview [Image]
  Txt_BuildingOverviewTitle [TextMeshProUGUI]
  Grp_BuildingOverviewActions [GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)]
    Btn_BuildingOverviewRun [Button + Image]
      Txt_BuildingOverviewRunLabel [TextMeshProUGUI]
    Btn_BuildingOverviewSpecial [Button + Image]
      Txt_BuildingOverviewSpecialLabel [TextMeshProUGUI]
    Btn_BuildingOverviewRule [Button + Image]
      Txt_BuildingOverviewRuleLabel [TextMeshProUGUI]
    Btn_BuildingOverviewFocus [Button + Image]
      Txt_BuildingOverviewFocusLabel [TextMeshProUGUI]
  Panel_BuildingOverviewIdentity [Image]
    Txt_BuildingOverviewIdentityHeading [TextMeshProUGUI]
    List_BuildingOverviewIdentity [ScrollRect vertical=true horizontal=false]
      Viewport_BuildingOverviewIdentity [RectMask2D]
        Content_BuildingOverviewIdentity [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_BuildingOverviewIdentityBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_BuildingOverviewIdentityTemplate [LayoutElement + Image；默认inactive]
            Btn_BuildingOverviewIdentityRow [Button + Image]
              Txt_BuildingOverviewIdentityRowLabel [TextMeshProUGUI]
              Txt_BuildingOverviewIdentityRowValue [TextMeshProUGUI]
  Panel_BuildingOverviewOperation [Image]
    Txt_BuildingOverviewOperationHeading [TextMeshProUGUI]
    List_BuildingOverviewOperation [ScrollRect vertical=true horizontal=false]
      Viewport_BuildingOverviewOperation [RectMask2D]
        Content_BuildingOverviewOperation [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_BuildingOverviewOperationBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_BuildingOverviewOperationTemplate [LayoutElement + Image；默认inactive]
            Btn_BuildingOverviewOperationRow [Button + Image]
              Txt_BuildingOverviewOperationRowLabel [TextMeshProUGUI]
              Txt_BuildingOverviewOperationRowValue [TextMeshProUGUI]
  Grp_BuildingOverviewLoadingState [无Graphic]
    Panel_BuildingOverviewLoadingMessage [Image]
      Txt_BuildingOverviewLoadingMessage [TextMeshProUGUI]
  Grp_BuildingOverviewEmptyState [无Graphic]
    Panel_BuildingOverviewEmptyMessage [Image]
      Txt_BuildingOverviewEmptyMessage [TextMeshProUGUI]
  Grp_BuildingOverviewErrorState [无Graphic]
    Panel_BuildingOverviewErrorMessage [Image]
      Txt_BuildingOverviewErrorMessage [TextMeshProUGUI]
  Grp_BuildingOverviewSuccessState [无Graphic]
    Panel_BuildingOverviewSuccessMessage [Image]
      Txt_BuildingOverviewSuccessMessage [TextMeshProUGUI]
  Grp_BuildingOverviewDisabledState [无Graphic]
    Panel_BuildingOverviewDisabledMessage [Image]
      Txt_BuildingOverviewDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageBuildingOverview | min(1,1) max(1,1); pivot(1,1); sizeDelta(520,832); pos(-24,-120) | absolute | Image；通用建筑现场面板；内部页面根 |
| Txt_BuildingOverviewTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(432,32); pos(8,0) | absolute | TextMeshProUGUI；通用建筑现场面板 |
| Grp_BuildingOverviewActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,104); pos(0,0) | absolute | GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)；操作区 |
| Btn_BuildingOverviewRun | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；仅建筑支持且权限允许时操作，显示权威结果 |
| Txt_BuildingOverviewRunLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；启动或停止 |
| Btn_BuildingOverviewSpecial | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；按类型进入水泵／发电／蓄电／仓库／工坊／传送带子页 |
| Txt_BuildingOverviewSpecialLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；打开专用内容 |
| Btn_BuildingOverviewRule | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；16-通用规则 |
| Txt_BuildingOverviewRuleLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；规则说明 |
| Btn_BuildingOverviewFocus | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；镜头聚焦 |
| Txt_BuildingOverviewFocusLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；聚焦建筑 |
| Panel_BuildingOverviewIdentity | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,334); pos(0,-36) | absolute | Image；建筑概况 |
| Txt_BuildingOverviewIdentityHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；建筑概况 |
| List_BuildingOverviewIdentity | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_BuildingOverviewIdentity | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_BuildingOverviewIdentity | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_BuildingOverviewIdentityBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；名称／类型；建造、运行、停止、断电状态；固有功能 |
| Item_BuildingOverviewIdentityTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_BuildingOverviewIdentityRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_BuildingOverviewIdentityRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_BuildingOverviewIdentityRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_BuildingOverviewOperation | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,334); pos(0,-382) | absolute | Image；运行详情 |
| Txt_BuildingOverviewOperationHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；运行详情 |
| List_BuildingOverviewOperation | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_BuildingOverviewOperation | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_BuildingOverviewOperation | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_BuildingOverviewOperationBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；功率；对应类型支持的缓存；阻塞原因；现场操作权限 |
| Item_BuildingOverviewOperationTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_BuildingOverviewOperationRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_BuildingOverviewOperationRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_BuildingOverviewOperationRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_BuildingOverviewLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BuildingOverviewLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BuildingOverviewLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_BuildingOverviewEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BuildingOverviewEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BuildingOverviewEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_BuildingOverviewErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BuildingOverviewErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BuildingOverviewErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_BuildingOverviewSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BuildingOverviewSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BuildingOverviewSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_BuildingOverviewDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BuildingOverviewDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BuildingOverviewDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## Pump：水泵

功能文档：[水泵](Pump.md)；归属 `FieldHudForm`；内容 520×832。

```text
Panel_PagePump [Image]
  Txt_PumpTitle [TextMeshProUGUI]
  Grp_PumpActions [GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)]
    Btn_PumpRun [Button + Image]
      Txt_PumpRunLabel [TextMeshProUGUI]
    Btn_PumpFocus [Button + Image]
      Txt_PumpFocusLabel [TextMeshProUGUI]
    Btn_PumpRule [Button + Image]
      Txt_PumpRuleLabel [TextMeshProUGUI]
  Panel_PumpProduction [Image]
    Txt_PumpProductionHeading [TextMeshProUGUI]
    List_PumpProduction [ScrollRect vertical=true horizontal=false]
      Viewport_PumpProduction [RectMask2D]
        Content_PumpProduction [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_PumpProductionBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_PumpProductionTemplate [LayoutElement + Image；默认inactive]
            Btn_PumpProductionRow [Button + Image]
              Txt_PumpProductionRowLabel [TextMeshProUGUI]
              Txt_PumpProductionRowValue [TextMeshProUGUI]
  Panel_PumpBlock [Image]
    Txt_PumpBlockHeading [TextMeshProUGUI]
    List_PumpBlock [ScrollRect vertical=true horizontal=false]
      Viewport_PumpBlock [RectMask2D]
        Content_PumpBlock [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_PumpBlockBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_PumpBlockTemplate [LayoutElement + Image；默认inactive]
            Btn_PumpBlockRow [Button + Image]
              Txt_PumpBlockRowLabel [TextMeshProUGUI]
              Txt_PumpBlockRowValue [TextMeshProUGUI]
  Grp_PumpLoadingState [无Graphic]
    Panel_PumpLoadingMessage [Image]
      Txt_PumpLoadingMessage [TextMeshProUGUI]
  Grp_PumpEmptyState [无Graphic]
    Panel_PumpEmptyMessage [Image]
      Txt_PumpEmptyMessage [TextMeshProUGUI]
  Grp_PumpErrorState [无Graphic]
    Panel_PumpErrorMessage [Image]
      Txt_PumpErrorMessage [TextMeshProUGUI]
  Grp_PumpSuccessState [无Graphic]
    Panel_PumpSuccessMessage [Image]
      Txt_PumpSuccessMessage [TextMeshProUGUI]
  Grp_PumpDisabledState [无Graphic]
    Panel_PumpDisabledMessage [Image]
      Txt_PumpDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PagePump | min(1,1) max(1,1); pivot(1,1); sizeDelta(520,832); pos(-24,-120) | absolute | Image；水泵；内部页面根 |
| Txt_PumpTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(432,32); pos(8,0) | absolute | TextMeshProUGUI；水泵 |
| Grp_PumpActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,104); pos(0,0) | absolute | GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)；操作区 |
| Btn_PumpRun | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；按建筑控制修改运行请求；缓存满时启动不保证产出 |
| Txt_PumpRunLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；启动或停止 |
| Btn_PumpFocus | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；定位关联水域 |
| Txt_PumpFocusLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看水源 |
| Btn_PumpRule | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；解释满载停产及腾空恢复条件 |
| Txt_PumpRuleLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；规则说明 |
| Panel_PumpProduction | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,334); pos(0,-36) | absolute | Image；取水状态 |
| Txt_PumpProductionHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；取水状态 |
| List_PumpProduction | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_PumpProduction | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_PumpProduction | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_PumpProductionBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；启动状态；供电；关联水域；额定／实际产出；暂存水量／容量 |
| Item_PumpProductionTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_PumpProductionRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_PumpProductionRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_PumpProductionRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_PumpBlock | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,334); pos(0,-382) | absolute | Image；阻塞与交接 |
| Txt_PumpBlockHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；阻塞与交接 |
| List_PumpBlock | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_PumpBlock | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_PumpBlock | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_PumpBlockBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；满载停产；供电或水域无效等原因；机械臂装卸可用信息 |
| Item_PumpBlockTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_PumpBlockRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_PumpBlockRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_PumpBlockRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_PumpLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_PumpLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_PumpLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_PumpEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_PumpEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_PumpEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_PumpErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_PumpErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_PumpErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_PumpSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_PumpSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_PumpSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_PumpDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_PumpDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_PumpDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## Generator：生物质发电机

功能文档：[生物质发电机](Generator.md)；归属 `FieldHudForm`；内容 520×832。

```text
Panel_PageGenerator [Image]
  Txt_GeneratorTitle [TextMeshProUGUI]
  Grp_GeneratorActions [GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)]
    Btn_GeneratorRun [Button + Image]
      Txt_GeneratorRunLabel [TextMeshProUGUI]
    Btn_GeneratorCharge [Button + Image]
      Txt_GeneratorChargeLabel [TextMeshProUGUI]
    Btn_GeneratorTarget [Button + Image]
      Txt_GeneratorTargetLabel [TextMeshProUGUI]
    Btn_GeneratorEnergy [Button + Image]
      Txt_GeneratorEnergyLabel [TextMeshProUGUI]
  Panel_GeneratorPower [Image]
    Txt_GeneratorPowerHeading [TextMeshProUGUI]
    List_GeneratorPower [ScrollRect vertical=true horizontal=false]
      Viewport_GeneratorPower [RectMask2D]
        Content_GeneratorPower [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_GeneratorPowerBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_GeneratorPowerTemplate [LayoutElement + Image；默认inactive]
            Btn_GeneratorPowerRow [Button + Image]
              Txt_GeneratorPowerRowLabel [TextMeshProUGUI]
              Txt_GeneratorPowerRowValue [TextMeshProUGUI]
          Panel_GeneratorControls [Image + LayoutElement]
            Tgl_GeneratorChargingAllowed [Toggle]
              Img_GeneratorChargingAllowedBox [Image]
                Icon_GeneratorChargingAllowedCheck [Image]
              Txt_GeneratorChargingAllowedLabel [TextMeshProUGUI]
            Sld_GeneratorChargeTarget [Slider]
              Bg_GeneratorChargeTargetTrack [Image]
              Bar_GeneratorChargeTargetFill [Image(Filled)]
              Img_GeneratorChargeTargetHandle [Image]
              Txt_GeneratorChargeTargetValue [TextMeshProUGUI]
  Panel_GeneratorCharging [Image]
    Txt_GeneratorChargingHeading [TextMeshProUGUI]
    List_GeneratorCharging [ScrollRect vertical=true horizontal=false]
      Viewport_GeneratorCharging [RectMask2D]
        Content_GeneratorCharging [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_GeneratorChargingBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_GeneratorChargingTemplate [LayoutElement + Image；默认inactive]
            Btn_GeneratorChargingRow [Button + Image]
              Txt_GeneratorChargingRowLabel [TextMeshProUGUI]
              Txt_GeneratorChargingRowValue [TextMeshProUGUI]
  Grp_GeneratorLoadingState [无Graphic]
    Panel_GeneratorLoadingMessage [Image]
      Txt_GeneratorLoadingMessage [TextMeshProUGUI]
  Grp_GeneratorEmptyState [无Graphic]
    Panel_GeneratorEmptyMessage [Image]
      Txt_GeneratorEmptyMessage [TextMeshProUGUI]
  Grp_GeneratorErrorState [无Graphic]
    Panel_GeneratorErrorMessage [Image]
      Txt_GeneratorErrorMessage [TextMeshProUGUI]
  Grp_GeneratorSuccessState [无Graphic]
    Panel_GeneratorSuccessMessage [Image]
      Txt_GeneratorSuccessMessage [TextMeshProUGUI]
  Grp_GeneratorDisabledState [无Graphic]
    Panel_GeneratorDisabledMessage [Image]
      Txt_GeneratorDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageGenerator | min(1,1) max(1,1); pivot(1,1); sizeDelta(520,832); pos(-24,-120) | absolute | Image；生物质发电机；内部页面根 |
| Txt_GeneratorTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(432,32); pos(8,0) | absolute | TextMeshProUGUI；生物质发电机 |
| Grp_GeneratorActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,104); pos(0,0) | absolute | GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)；操作区 |
| Btn_GeneratorRun | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；提交建筑启停意图 |
| Txt_GeneratorRunLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；启动或停止 |
| Btn_GeneratorCharge | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；Toggle提交，显示影响摘要 |
| Txt_GeneratorChargeLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；设置充电许可 |
| Btn_GeneratorTarget | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；百分比滑条提交有效区间值；最终检查设施权限 |
| Txt_GeneratorTargetLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；设置目标比例 |
| Btn_GeneratorEnergy | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开04-能源系统详情 |
| Txt_GeneratorEnergyLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看区域电网 |
| Panel_GeneratorPower | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,334); pos(0,-36) | absolute | Image；发电状态 |
| Txt_GeneratorPowerHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；发电状态 |
| List_GeneratorPower | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_GeneratorPower | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_GeneratorPower | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_GeneratorPowerBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；额定／实际功率；燃料存量；消耗速度；预计维持时间；运行与阻塞 |
| Item_GeneratorPowerTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_GeneratorPowerRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_GeneratorPowerRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_GeneratorPowerRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_GeneratorControls | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,160)初始化; pos(0,0)初始化; LayoutElement preferred(496,160); 最终位置/尺寸由组驱动 | group | Image + LayoutElement；真实输入字段 |
| Tgl_GeneratorChargingAllowed | min(0,1) max(0,1); pivot(0,1); sizeDelta(464,48); pos(16,-8) | absolute | Toggle；允许充电 |
| Img_GeneratorChargingAllowedBox | min(0,1) max(0,1); pivot(0,1); sizeDelta(32,32); pos(0,-8) | absolute | Image；开关背景 |
| Icon_GeneratorChargingAllowedCheck | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；Toggle.graphic |
| Txt_GeneratorChargingAllowedLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-48,0); pos(24,0) | absolute | TextMeshProUGUI；允许充电 |
| Sld_GeneratorChargeTarget | min(0,1) max(0,1); pivot(0,1); sizeDelta(464,56); pos(16,-80) | absolute | Slider；目标储电比例 |
| Bg_GeneratorChargeTargetTrack | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,8); pos(0,8) | absolute | Image；轨道 |
| Bar_GeneratorChargeTargetFill | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,8); pos(0,8) | absolute | Image(Filled)；Slider.fillRect驱动 |
| Img_GeneratorChargeTargetHandle | min(0,0) max(0,0); pivot(0.5,0.5); sizeDelta(24,24); pos(12,12) | absolute | Image；Slider.handleRect驱动位置 |
| Txt_GeneratorChargeTargetValue | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,28); pos(0,0) | absolute | TextMeshProUGUI；目标储电比例：— |
| Panel_GeneratorCharging | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,334); pos(0,-382) | absolute | Image；蓄电配置 |
| Txt_GeneratorChargingHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；蓄电配置 |
| List_GeneratorCharging | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_GeneratorCharging | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_GeneratorCharging | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_GeneratorChargingBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；允许为蓄电池充电；目标储电比例；当前实际策略结果 |
| Item_GeneratorChargingTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_GeneratorChargingRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_GeneratorChargingRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_GeneratorChargingRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_GeneratorLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_GeneratorLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_GeneratorLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_GeneratorEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_GeneratorEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_GeneratorEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_GeneratorErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_GeneratorErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_GeneratorErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_GeneratorSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_GeneratorSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_GeneratorSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_GeneratorDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_GeneratorDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_GeneratorDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## Solar：初始太阳能设施

功能文档：[初始太阳能设施](Solar.md)；归属 `FieldHudForm`；内容 520×832。

```text
Panel_PageSolar [Image]
  Txt_SolarTitle [TextMeshProUGUI]
  Grp_SolarActions [GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)]
    Btn_SolarRun [Button + Image]
      Txt_SolarRunLabel [TextMeshProUGUI]
    Btn_SolarEnergy [Button + Image]
      Txt_SolarEnergyLabel [TextMeshProUGUI]
    Btn_SolarRule [Button + Image]
      Txt_SolarRuleLabel [TextMeshProUGUI]
  Panel_SolarPower [Image]
    Txt_SolarPowerHeading [TextMeshProUGUI]
    List_SolarPower [ScrollRect vertical=true horizontal=false]
      Viewport_SolarPower [RectMask2D]
        Content_SolarPower [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_SolarPowerBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_SolarPowerTemplate [LayoutElement + Image；默认inactive]
            Btn_SolarPowerRow [Button + Image]
              Txt_SolarPowerRowLabel [TextMeshProUGUI]
              Txt_SolarPowerRowValue [TextMeshProUGUI]
  Panel_SolarRecovery [Image]
    Txt_SolarRecoveryHeading [TextMeshProUGUI]
    List_SolarRecovery [ScrollRect vertical=true horizontal=false]
      Viewport_SolarRecovery [RectMask2D]
        Content_SolarRecovery [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_SolarRecoveryBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_SolarRecoveryTemplate [LayoutElement + Image；默认inactive]
            Btn_SolarRecoveryRow [Button + Image]
              Txt_SolarRecoveryRowLabel [TextMeshProUGUI]
              Txt_SolarRecoveryRowValue [TextMeshProUGUI]
  Grp_SolarLoadingState [无Graphic]
    Panel_SolarLoadingMessage [Image]
      Txt_SolarLoadingMessage [TextMeshProUGUI]
  Grp_SolarEmptyState [无Graphic]
    Panel_SolarEmptyMessage [Image]
      Txt_SolarEmptyMessage [TextMeshProUGUI]
  Grp_SolarErrorState [无Graphic]
    Panel_SolarErrorMessage [Image]
      Txt_SolarErrorMessage [TextMeshProUGUI]
  Grp_SolarSuccessState [无Graphic]
    Panel_SolarSuccessMessage [Image]
      Txt_SolarSuccessMessage [TextMeshProUGUI]
  Grp_SolarDisabledState [无Graphic]
    Panel_SolarDisabledMessage [Image]
      Txt_SolarDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageSolar | min(1,1) max(1,1); pivot(1,1); sizeDelta(520,832); pos(-24,-120) | absolute | Image；初始太阳能设施；内部页面根 |
| Txt_SolarTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(432,32); pos(8,0) | absolute | TextMeshProUGUI；初始太阳能设施 |
| Grp_SolarActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,104); pos(0,0) | absolute | GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)；操作区 |
| Btn_SolarRun | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；在全网无电时仍允许开启 |
| Txt_SolarRunLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；开启或关闭 |
| Btn_SolarEnergy | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入04能源详情 |
| Txt_SolarEnergyLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看能源系统 |
| Btn_SolarRule | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；解释日照和初始保障属性 |
| Txt_SolarRuleLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；规则说明 |
| Panel_SolarPower | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,334); pos(0,-36) | absolute | Image；环境发电 |
| Txt_SolarPowerHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；环境发电 |
| List_SolarPower | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_SolarPower | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_SolarPower | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_SolarPowerBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；日照；额定／实际功率；运行状态；区域供给贡献 |
| Item_SolarPowerTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_SolarPowerRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_SolarPowerRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_SolarPowerRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_SolarRecovery | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,334); pos(0,-382) | absolute | Image；恢复保障 |
| Txt_SolarRecoveryHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；恢复保障 |
| List_SolarRecovery | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_SolarRecovery | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_SolarRecovery | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_SolarRecoveryBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；初始保障设施；无燃料；无电时仍可重新开启 |
| Item_SolarRecoveryTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_SolarRecoveryRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_SolarRecoveryRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_SolarRecoveryRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_SolarLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SolarLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SolarLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_SolarEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SolarEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SolarEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_SolarErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SolarErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SolarErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_SolarSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SolarSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SolarSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_SolarDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SolarDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SolarDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## Battery：蓄电池

功能文档：[蓄电池](Battery.md)；归属 `FieldHudForm`；内容 520×832。

```text
Panel_PageBattery [Image]
  Txt_BatteryTitle [TextMeshProUGUI]
  Grp_BatteryActions [GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)]
    Btn_BatteryEnergy [Button + Image]
      Txt_BatteryEnergyLabel [TextMeshProUGUI]
    Btn_BatteryHistory [Button + Image]
      Txt_BatteryHistoryLabel [TextMeshProUGUI]
    Btn_BatteryRule [Button + Image]
      Txt_BatteryRuleLabel [TextMeshProUGUI]
  Panel_BatteryStorage [Image]
    Txt_BatteryStorageHeading [TextMeshProUGUI]
    List_BatteryStorage [ScrollRect vertical=true horizontal=false]
      Viewport_BatteryStorage [RectMask2D]
        Content_BatteryStorage [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_BatteryStorageBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_BatteryStorageTemplate [LayoutElement + Image；默认inactive]
            Btn_BatteryStorageRow [Button + Image]
              Txt_BatteryStorageRowLabel [TextMeshProUGUI]
              Txt_BatteryStorageRowValue [TextMeshProUGUI]
  Panel_BatteryEstimate [Image]
    Txt_BatteryEstimateHeading [TextMeshProUGUI]
    List_BatteryEstimate [ScrollRect vertical=true horizontal=false]
      Viewport_BatteryEstimate [RectMask2D]
        Content_BatteryEstimate [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_BatteryEstimateBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_BatteryEstimateTemplate [LayoutElement + Image；默认inactive]
            Btn_BatteryEstimateRow [Button + Image]
              Txt_BatteryEstimateRowLabel [TextMeshProUGUI]
              Txt_BatteryEstimateRowValue [TextMeshProUGUI]
  Grp_BatteryLoadingState [无Graphic]
    Panel_BatteryLoadingMessage [Image]
      Txt_BatteryLoadingMessage [TextMeshProUGUI]
  Grp_BatteryEmptyState [无Graphic]
    Panel_BatteryEmptyMessage [Image]
      Txt_BatteryEmptyMessage [TextMeshProUGUI]
  Grp_BatteryErrorState [无Graphic]
    Panel_BatteryErrorMessage [Image]
      Txt_BatteryErrorMessage [TextMeshProUGUI]
  Grp_BatterySuccessState [无Graphic]
    Panel_BatterySuccessMessage [Image]
      Txt_BatterySuccessMessage [TextMeshProUGUI]
  Grp_BatteryDisabledState [无Graphic]
    Panel_BatteryDisabledMessage [Image]
      Txt_BatteryDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageBattery | min(1,1) max(1,1); pivot(1,1); sizeDelta(520,832); pos(-24,-120) | absolute | Image；蓄电池；内部页面根 |
| Txt_BatteryTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(432,32); pos(8,0) | absolute | TextMeshProUGUI；蓄电池 |
| Grp_BatteryActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,104); pos(0,0) | absolute | GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)；操作区 |
| Btn_BatteryEnergy | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入04能源详情 |
| Txt_BatteryEnergyLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；区域能源 |
| Btn_BatteryHistory | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开15能源事件 |
| Txt_BatteryHistoryLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；停机记录 |
| Btn_BatteryRule | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；展示蓄电机制 |
| Txt_BatteryRuleLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；规则说明 |
| Panel_BatteryStorage | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,334); pos(0,-36) | absolute | Image；储电状态 |
| Txt_BatteryStorageHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；储电状态 |
| List_BatteryStorage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_BatteryStorage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_BatteryStorage | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_BatteryStorageBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；电量／容量；充电／放电／待机；当前功率 |
| Item_BatteryStorageTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_BatteryStorageRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_BatteryStorageRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_BatteryStorageRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_BatteryEstimate | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,334); pos(0,-382) | absolute | Image；估算与事件 |
| Txt_BatteryEstimateHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；估算与事件 |
| List_BatteryEstimate | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_BatteryEstimate | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_BatteryEstimate | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_BatteryEstimateBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；按当前状态剩余时间；估算前提；最近电量耗尽／恢复事件 |
| Item_BatteryEstimateTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_BatteryEstimateRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_BatteryEstimateRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_BatteryEstimateRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_BatteryLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BatteryLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BatteryLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_BatteryEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BatteryEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BatteryEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_BatteryErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BatteryErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BatteryErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_BatterySuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BatterySuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BatterySuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_BatteryDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BatteryDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BatteryDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## WarehouseBuilding：仓库现场概要

功能文档：[仓库现场概要](WarehouseBuilding.md)；归属 `FieldHudForm`；内容 520×832。

```text
Panel_PageWarehouseBuilding [Image]
  Txt_WarehouseBuildingTitle [TextMeshProUGUI]
  Grp_WarehouseBuildingActions [GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)]
    Btn_WarehouseBuildingInventory [Button + Image]
      Txt_WarehouseBuildingInventoryLabel [TextMeshProUGUI]
    Btn_WarehouseBuildingRecords [Button + Image]
      Txt_WarehouseBuildingRecordsLabel [TextMeshProUGUI]
    Btn_WarehouseBuildingRule [Button + Image]
      Txt_WarehouseBuildingRuleLabel [TextMeshProUGUI]
  Panel_WarehouseBuildingCapacity [Image]
    Txt_WarehouseBuildingCapacityHeading [TextMeshProUGUI]
    List_WarehouseBuildingCapacity [ScrollRect vertical=true horizontal=false]
      Viewport_WarehouseBuildingCapacity [RectMask2D]
        Content_WarehouseBuildingCapacity [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_WarehouseBuildingCapacityBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_WarehouseBuildingCapacityTemplate [LayoutElement + Image；默认inactive]
            Btn_WarehouseBuildingCapacityRow [Button + Image]
              Txt_WarehouseBuildingCapacityRowLabel [TextMeshProUGUI]
              Txt_WarehouseBuildingCapacityRowValue [TextMeshProUGUI]
  Panel_WarehouseBuildingRecent [Image]
    Txt_WarehouseBuildingRecentHeading [TextMeshProUGUI]
    List_WarehouseBuildingRecent [ScrollRect vertical=true horizontal=false]
      Viewport_WarehouseBuildingRecent [RectMask2D]
        Content_WarehouseBuildingRecent [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_WarehouseBuildingRecentBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_WarehouseBuildingRecentTemplate [LayoutElement + Image；默认inactive]
            Btn_WarehouseBuildingRecentRow [Button + Image]
              Txt_WarehouseBuildingRecentRowLabel [TextMeshProUGUI]
              Txt_WarehouseBuildingRecentRowValue [TextMeshProUGUI]
  Grp_WarehouseBuildingLoadingState [无Graphic]
    Panel_WarehouseBuildingLoadingMessage [Image]
      Txt_WarehouseBuildingLoadingMessage [TextMeshProUGUI]
  Grp_WarehouseBuildingEmptyState [无Graphic]
    Panel_WarehouseBuildingEmptyMessage [Image]
      Txt_WarehouseBuildingEmptyMessage [TextMeshProUGUI]
  Grp_WarehouseBuildingErrorState [无Graphic]
    Panel_WarehouseBuildingErrorMessage [Image]
      Txt_WarehouseBuildingErrorMessage [TextMeshProUGUI]
  Grp_WarehouseBuildingSuccessState [无Graphic]
    Panel_WarehouseBuildingSuccessMessage [Image]
      Txt_WarehouseBuildingSuccessMessage [TextMeshProUGUI]
  Grp_WarehouseBuildingDisabledState [无Graphic]
    Panel_WarehouseBuildingDisabledMessage [Image]
      Txt_WarehouseBuildingDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageWarehouseBuilding | min(1,1) max(1,1); pivot(1,1); sizeDelta(520,832); pos(-24,-120) | absolute | Image；仓库现场概要；内部页面根 |
| Txt_WarehouseBuildingTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(432,32); pos(8,0) | absolute | TextMeshProUGUI；仓库现场概要 |
| Grp_WarehouseBuildingActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,104); pos(0,0) | absolute | GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)；操作区 |
| Btn_WarehouseBuildingInventory | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开09-仓库分类库存，携带仓库ID |
| Txt_WarehouseBuildingInventoryLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；完整库存 |
| Btn_WarehouseBuildingRecords | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开09-仓库记录 |
| Txt_WarehouseBuildingRecordsLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；入库记录 |
| Btn_WarehouseBuildingRule | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；解释资源结算与自动分类 |
| Txt_WarehouseBuildingRuleLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；规则说明 |
| Panel_WarehouseBuildingCapacity | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,334); pos(0,-36) | absolute | Image；实体容量 |
| Txt_WarehouseBuildingCapacityHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；实体容量 |
| List_WarehouseBuildingCapacity | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_WarehouseBuildingCapacity | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_WarehouseBuildingCapacity | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_WarehouseBuildingCapacityBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；实体库存已用／上限；空间状态；入库接口状态 |
| Item_WarehouseBuildingCapacityTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_WarehouseBuildingCapacityRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_WarehouseBuildingCapacityRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_WarehouseBuildingCapacityRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_WarehouseBuildingRecent | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,334); pos(0,-382) | absolute | Image；最近入库 |
| Txt_WarehouseBuildingRecentHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；最近入库 |
| List_WarehouseBuildingRecent | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_WarehouseBuildingRecent | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_WarehouseBuildingRecent | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_WarehouseBuildingRecentBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；入库与自动分类；卸货失败；主要阻塞 |
| Item_WarehouseBuildingRecentTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_WarehouseBuildingRecentRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_WarehouseBuildingRecentRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_WarehouseBuildingRecentRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_WarehouseBuildingLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WarehouseBuildingLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WarehouseBuildingLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_WarehouseBuildingEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WarehouseBuildingEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WarehouseBuildingEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_WarehouseBuildingErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WarehouseBuildingErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WarehouseBuildingErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_WarehouseBuildingSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WarehouseBuildingSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WarehouseBuildingSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_WarehouseBuildingDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WarehouseBuildingDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WarehouseBuildingDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## Construction：施工建筑

功能文档：[施工建筑](Construction.md)；归属 `FieldHudForm`；内容 520×832。

```text
Panel_PageConstruction [Image]
  Txt_ConstructionTitle [TextMeshProUGUI]
  Grp_ConstructionActions [GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)]
    Btn_ConstructionCancel [Button + Image]
      Txt_ConstructionCancelLabel [TextMeshProUGUI]
    Btn_ConstructionFocus [Button + Image]
      Txt_ConstructionFocusLabel [TextMeshProUGUI]
    Btn_ConstructionRule [Button + Image]
      Txt_ConstructionRuleLabel [TextMeshProUGUI]
  Panel_ConstructionProgress [Image]
    Txt_ConstructionProgressHeading [TextMeshProUGUI]
    List_ConstructionProgress [ScrollRect vertical=true horizontal=false]
      Viewport_ConstructionProgress [RectMask2D]
        Content_ConstructionProgress [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ConstructionProgressBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ConstructionProgressTemplate [LayoutElement + Image；默认inactive]
            Btn_ConstructionProgressRow [Button + Image]
              Txt_ConstructionProgressRowLabel [TextMeshProUGUI]
              Txt_ConstructionProgressRowValue [TextMeshProUGUI]
  Panel_ConstructionCost [Image]
    Txt_ConstructionCostHeading [TextMeshProUGUI]
    List_ConstructionCost [ScrollRect vertical=true horizontal=false]
      Viewport_ConstructionCost [RectMask2D]
        Content_ConstructionCost [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ConstructionCostBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ConstructionCostTemplate [LayoutElement + Image；默认inactive]
            Btn_ConstructionCostRow [Button + Image]
              Txt_ConstructionCostRowLabel [TextMeshProUGUI]
              Txt_ConstructionCostRowValue [TextMeshProUGUI]
  Grp_ConstructionLoadingState [无Graphic]
    Panel_ConstructionLoadingMessage [Image]
      Txt_ConstructionLoadingMessage [TextMeshProUGUI]
  Grp_ConstructionEmptyState [无Graphic]
    Panel_ConstructionEmptyMessage [Image]
      Txt_ConstructionEmptyMessage [TextMeshProUGUI]
  Grp_ConstructionErrorState [无Graphic]
    Panel_ConstructionErrorMessage [Image]
      Txt_ConstructionErrorMessage [TextMeshProUGUI]
  Grp_ConstructionSuccessState [无Graphic]
    Panel_ConstructionSuccessMessage [Image]
      Txt_ConstructionSuccessMessage [TextMeshProUGUI]
  Grp_ConstructionDisabledState [无Graphic]
    Panel_ConstructionDisabledMessage [Image]
      Txt_ConstructionDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageConstruction | min(1,1) max(1,1); pivot(1,1); sizeDelta(520,832); pos(-24,-120) | absolute | Image；施工建筑；内部页面根 |
| Txt_ConstructionTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(432,32); pos(8,0) | absolute | TextMeshProUGUI；施工建筑 |
| Grp_ConstructionActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,104); pos(0,0) | absolute | GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)；操作区 |
| Btn_ConstructionCancel | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入17-施工制造取消确认，提交时重新计算实际退款 |
| Txt_ConstructionCancelLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；取消施工 |
| Btn_ConstructionFocus | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；保持现场选中 |
| Txt_ConstructionFocusLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；聚焦施工 |
| Btn_ConstructionRule | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；解释自动施工和取消规则 |
| Txt_ConstructionRuleLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；规则说明 |
| Panel_ConstructionProgress | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,334); pos(0,-36) | absolute | Image；施工状态 |
| Txt_ConstructionProgressHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；施工状态 |
| List_ConstructionProgress | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ConstructionProgress | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ConstructionProgress | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ConstructionProgressBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；建筑类型；进度；剩余时间；既定位置与占地 |
| Item_ConstructionProgressTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ConstructionProgressRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ConstructionProgressRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ConstructionProgressRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_ConstructionCost | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,334); pos(0,-382) | absolute | Image；取消影响 |
| Txt_ConstructionCostHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；取消影响 |
| List_ConstructionCost | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ConstructionCost | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ConstructionCost | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ConstructionCostBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；原始投入；按剩余比例向下取整的预计返还；尚未提供建筑功能 |
| Item_ConstructionCostTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ConstructionCostRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ConstructionCostRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ConstructionCostRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_ConstructionLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ConstructionLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ConstructionLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_ConstructionEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ConstructionEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ConstructionEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_ConstructionErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ConstructionErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ConstructionErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_ConstructionSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ConstructionSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ConstructionSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_ConstructionDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ConstructionDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ConstructionDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## Conveyor：传送带设备详情

功能文档：[传送带设备详情](Conveyor.md)；归属 `FieldHudForm`；内容 520×832。

```text
Panel_PageConveyor [Image]
  Txt_ConveyorTitle [TextMeshProUGUI]
  Grp_ConveyorActions [GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)]
    Btn_ConveyorRun [Button + Image]
      Txt_ConveyorRunLabel [TextMeshProUGUI]
    Btn_ConveyorBind [Button + Image]
      Txt_ConveyorBindLabel [TextMeshProUGUI]
    Btn_ConveyorLocate [Button + Image]
      Txt_ConveyorLocateLabel [TextMeshProUGUI]
    Btn_ConveyorRule [Button + Image]
      Txt_ConveyorRuleLabel [TextMeshProUGUI]
  Panel_ConveyorState [Image]
    Txt_ConveyorStateHeading [TextMeshProUGUI]
    List_ConveyorState [ScrollRect vertical=true horizontal=false]
      Viewport_ConveyorState [RectMask2D]
        Content_ConveyorState [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ConveyorStateBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ConveyorStateTemplate [LayoutElement + Image；默认inactive]
            Btn_ConveyorStateRow [Button + Image]
              Txt_ConveyorStateRowLabel [TextMeshProUGUI]
              Txt_ConveyorStateRowValue [TextMeshProUGUI]
  Panel_ConveyorLink [Image]
    Txt_ConveyorLinkHeading [TextMeshProUGUI]
    List_ConveyorLink [ScrollRect vertical=true horizontal=false]
      Viewport_ConveyorLink [RectMask2D]
        Content_ConveyorLink [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ConveyorLinkBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ConveyorLinkTemplate [LayoutElement + Image；默认inactive]
            Btn_ConveyorLinkRow [Button + Image]
              Txt_ConveyorLinkRowLabel [TextMeshProUGUI]
              Txt_ConveyorLinkRowValue [TextMeshProUGUI]
  Grp_ConveyorLoadingState [无Graphic]
    Panel_ConveyorLoadingMessage [Image]
      Txt_ConveyorLoadingMessage [TextMeshProUGUI]
  Grp_ConveyorEmptyState [无Graphic]
    Panel_ConveyorEmptyMessage [Image]
      Txt_ConveyorEmptyMessage [TextMeshProUGUI]
  Grp_ConveyorErrorState [无Graphic]
    Panel_ConveyorErrorMessage [Image]
      Txt_ConveyorErrorMessage [TextMeshProUGUI]
  Grp_ConveyorSuccessState [无Graphic]
    Panel_ConveyorSuccessMessage [Image]
      Txt_ConveyorSuccessMessage [TextMeshProUGUI]
  Grp_ConveyorDisabledState [无Graphic]
    Panel_ConveyorDisabledMessage [Image]
      Txt_ConveyorDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageConveyor | min(1,1) max(1,1); pivot(1,1); sizeDelta(520,832); pos(-24,-120) | absolute | Image；传送带设备详情；内部页面根 |
| Txt_ConveyorTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(432,32); pos(8,0) | absolute | TextMeshProUGUI；传送带设备详情 |
| Grp_ConveyorActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,104); pos(0,0) | absolute | GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)；操作区 |
| Btn_ConveyorRun | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；仅按传送带已实现的控制权限操作 |
| Txt_ConveyorRunLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；启动或停止 |
| Btn_ConveyorBind | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入12-世界对象选择，过滤合同允许的接收接口 |
| Txt_ConveyorBindLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；选择出料关联 |
| Btn_ConveyorLocate | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；有效才聚焦 |
| Txt_ConveyorLocateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；定位关联对象 |
| Btn_ConveyorRule | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；解释物理货物与关联出口 |
| Txt_ConveyorRuleLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；规则说明 |
| Panel_ConveyorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,334); pos(0,-36) | absolute | Image；物流设备状态 |
| Txt_ConveyorStateHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；物流设备状态 |
| List_ConveyorState | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ConveyorState | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ConveyorState | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ConveyorStateBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；运行／停止／断电；入口、带体、出口状态；缓存与阻塞 |
| Item_ConveyorStateTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ConveyorStateRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ConveyorStateRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ConveyorStateRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_ConveyorLink | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,334); pos(0,-382) | absolute | Image；出料口关联 |
| Txt_ConveyorLinkHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；出料口关联 |
| List_ConveyorLink | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ConveyorLink | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ConveyorLink | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ConveyorLinkBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；当前目的对象／接口；有效性；关联限制与失效原因 |
| Item_ConveyorLinkTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ConveyorLinkRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ConveyorLinkRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ConveyorLinkRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_ConveyorLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ConveyorLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ConveyorLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_ConveyorEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ConveyorEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ConveyorEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_ConveyorErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ConveyorErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ConveyorErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_ConveyorSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ConveyorSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ConveyorSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_ConveyorDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ConveyorDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ConveyorDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |
