# BaseCommandHubForm.prefab-layout.md

> 本文是该页面的**结构契约**（GF-UI-Standards 01/03 要求的先行产物）。重建工具
> `AutoEraOperationsPrefabBinder.BindHubPrefab` 严格按本文建树；结构变更必须先改本文再重跑工具。
> 对应 Prefab：`Assets/Game/Prefabs/UI/Operations/BaseCommandHubForm.prefab`
> Form 脚本：`AutoEra.UI.BaseCommandHubForm : AutoEraUiFormBase : UIFormBase`（UIViews=6002）
>
> 基准分辨率 1920×1080。根节点**不含 Canvas**（GF 动态实例化到项目 Canvas 下）。
> 命名只出规范词表：`Bg_/Panel_/Grp_/Txt_/Img_/Icon_/Btn_/List_/Item_/Viewport_/Content_/Bar_/Overlay_`。
> 占位文本（`—`、`--`）为运行时可覆盖的功能占位，**不含任何假业务数据**。
> 颜色为结构期中性 token（见 §5），美术微调只改颜色/sprite/字号，禁止动结构。

## 1. 结构树

```text
BaseCommandHubForm（根，stretch 全屏；组件：BaseCommandHubForm）
├─ Bg_Terminal                      Image #0E1826
├─ Panel_TopBar                     Image #142438
│  ├─ Grp_ResourceTimeSpeed         HorizontalLayoutGroup（无视觉分组）
│  │  ├─ Txt_ResourceCrew / Water / Food / Power
│  ├─ Txt_MachineRosterSummary      ← Form._machineRosterSummary
│  └─ Grp_Navigation                HorizontalLayoutGroup
│     └─ ×5 页签（Overview/Tasks/Objects/Rules/Statistics）：
│        Btn_Tab{Page}              Image(c02-action-secondary, Sliced) + Button + Explicit 导航
│           ├─ Txt_Tab{Page}        ← Form._pageBindings[i]._navigationLabel
│           └─ Img_TabActive{Page}  默认停用 ← Form._pageBindings[i]._activeNavigationVisual
├─ Grp_Content                      纯分组（顶部让出 92）
│  ├─ Panel_PageOverview            默认激活
│  ├─ Panel_PageTasks / Objects / Rules / Statistics   默认停用
│  └─（各页结构见 §3）
└─ Overlay_RulesImpact              默认停用；AutoEraDangerConfirmationView
   ├─ Bg_ImpactScrim                raycast=true（阻断穿透）
   └─ Panel_ImpactDialog            （结构见 §4）
```

## 2. 顶栏

| 节点 | anchor | pivot | size | pos | 组件/备注 |
|---|---|---|---|---|---|
| Panel_TopBar | (0,1)-(1,1) | (0.5,1) | (0,92) | (0,0) | Image #142438 |
| Grp_ResourceTimeSpeed | (0,1) | (0,1) | (620,36) | (24,-12) | HorizontalLayoutGroup spacing 24 |
| Txt_ResourceCrew…Power | 布局组控制 | — | — | — | TMP 20px，占位 `--`，ray=false |
| Txt_MachineRosterSummary | (0,1) | (0,1) | (620,24) | (24,-56) | TMP 16px，占位 `机器：-- · 已连接：-- · 可运行：--` |
| Grp_Navigation | (1,1) | (1,1) | (808,56) | (-24,-18) | HorizontalLayoutGroup spacing 8 |
| Btn_Tab{Page} | 布局组 | — | (152,56) | — | Image(sprite)+Button(ColorTint)；Text 22px |
| Txt_Tab{Page} | stretch | — | (0,0) | — | TMP 22px ray=false；文案：总览/任务/对象/规则/统计 |
| Img_TabActive{Page} | (0.5,0) | (0.5,0) | (64,4) | (0,2) | Image #FFA438，默认停用（形状通道高亮） |

页签文案与 `AutoEraHubPage` 枚举顺序一致：Overview→Tasks→Objects→Rules→Statistics。

## 3. 五个页面（互斥激活，`Panel_Page*` 均 stretch 铺满 Grp_Content，底图 #101C2C ray=false）

### Panel_PageOverview
| 节点 | 结构 | 备注 |
|---|---|---|
| Panel_MapViewport | anchor (0,0)-(0.62,1) | Image #0B1420；Txt_MapTitle（24,-16）「星域地图」26px |
| Grp_MapNodes | stretch | 运行时布置地图节点 |
| Item_MapNode_Template | 停用，center，220×64 | Image #1B3040；子：Icon_NodeStatus(14×14 #3AD6A0)、Icon_NodeEffector(14×14 #E8B44C)、Txt_NodeName（左，18px） |
| Panel_SelectionDetail | anchor (0.62,0)-(1,1) | Image #122236；标题 26px + Txt_SelectionName/Summary/Task/State/Action 占位 `—` |

### Panel_PageTasks
| 节点 | 结构 | 备注 |
|---|---|---|
| List_TaskQueue | anchor (0,0)-(0.55,1) | Image #0B1420 **ray=true**（滚轮事件）；ScrollRect(horizontal=false) |
| Viewport_TaskQueue | stretch −32/−24 | RectMask2D |
| Content_TaskQueue | (0,1)-(1,1) pivot(0.5,1) | VerticalLayoutGroup(spacing 8, padding 16) + ContentSizeFitter(Vertical=Preferred) |
| Item_TaskRow_Template | 停用，h=64 | Image #16283C + HorizontalLayoutGroup；Txt_RowTaskName(stretch) + Txt_RowTaskState(140) |
| Panel_TaskDetail | anchor (0.55,0)-(1,1) | Image #122236；Txt_TaskDetailTitle/Stage/Cause/Impact/Action 占位 |
| Btn_TaskPrimaryAction | 底部居中 220×56 | Image(sprite)+Button；Txt_TaskActionLabel「执行」；**待业务接线** |

### Panel_PageObjects
| 节点 | 结构 | 备注 |
|---|---|---|
| Panel_ObjectFilters | 顶部 h=56 | Txt_ObjectSearchPlaceholder「搜索对象…」/Txt_ObjectFilterCategory「全部分类」/Txt_ObjectFilterStatus「全部状态」 |
| List_Objects | 左 (0,0)-(0.55,1)，顶部 −56 | ScrollRect + Viewport_Objects(RectMask2D) + Content_Objects(VerticalLayoutGroup) |
| Item_ObjectRow_Template | 停用，h=64 | Txt_RowObjectName(stretch) + Txt_RowObjectState(140) |
| Txt_ObjectsEmpty | 居中 | 「暂无对象数据」默认激活（空态提示，非假数据） |
| Panel_ObjectDetail | 右 (0.55,0)-(1,1)，顶部 −56 | Txt_ObjectDetailTitle/State/Info/Input/Output/Power/TaskLabel/TaskValue/More/Locate 占位 |

### Panel_PageRules
| 节点 | 结构 | 备注 |
|---|---|---|
| List_Rules | 左 (0,0)-(0.42,1) | ScrollRect + Viewport_Rules + Content_Rules(VerticalLayoutGroup)；Item_RuleRow_Template(Txt_RowRuleName+Txt_RowRuleState)；Txt_RulesEmpty「暂无自动化规则」 |
| Panel_RuleEditor | 右上（高 = H−280） | Txt_RuleEditorTitle「规则编辑」/Condition/Action 占位 |
| Panel_RuleSummary | 右中 h=190 y=90 | Txt_RuleSummaryTitle/Impact/Enable 占位 |
| Grp_RuleOperation | 右下 h=90 | 操作状态区，绑定见 §6 |

### Panel_PageStatistics
| 节点 | 结构 | 备注 |
|---|---|---|
| List_MetricCards | 顶部 h=240 | Image #0B1420 + GridLayoutGroup(cell 300×96, spacing 16, padding 24, 4 列)；Item_MetricCard_Template(Txt_MetricCardName+Txt_MetricCardValue) 停用 |
| Panel_TrendChart | 中部（高 = H−400） | Txt_TrendTitle「趋势」；Img_TrendPlaceholder（明确命名占位）；Txt_TrendReadout |
| Panel_MetricDetail | 底部 h=110 | Txt_MetricDetail 占位 |

## 4. Overlay_RulesImpact（危险确认模态）

| 节点 | 结构 | 备注 |
|---|---|---|
| Overlay_RulesImpact | stretch，停用 | 挂 AutoEraDangerConfirmationView |
| Bg_ImpactScrim | stretch | Image #000000 a=0.6，**raycast=true** |
| Panel_ImpactDialog | center 640×420 | Image #142438 |
| Txt_ImpactTitle | 顶部 | 「危险操作确认」24px |
| Txt_ImpactBody | 中部 | ← DangerView._bodyText |
| Txt_ImpactScope | 底部 h=32 | 停用；「影响范围：—」 |
| Img_ImpactErrorBox | 底部 h=56 y=116 | Image #8A1E17 停用 ← DangerView._errorBox；子 Txt_ImpactError ← DangerView._errorText |
| Grp_ImpactActions | 底部 h=56 y=24 | HorizontalLayoutGroup |
| Btn_ImpactCancel / Btn_ImpactConfirm | 220×48 | ← DangerView._cancelButton/_dangerButton；子 Txt_ImpactCancelLabel「取消」/Txt_ImpactConfirmLabel「确认执行」 |

## 5. 结构期色彩 token（重建工具内常量，美术验收后可挪入主题表）

| Token | 值 | 用途 |
|---|---|---|
| BgTerminal | #0E1826 | 全屏底 |
| BgTopBar | #142438 | 顶栏 |
| BgPage | #101C2C | 页面底 |
| BgSection | #0B1420 | 列表/图表区 |
| BgPanel | #122236 | 详情面板 |
| BgItem | #16283C | 列表行/卡片 |
| Accent | #FFA438 | 激活/高亮 |
| Ok #3AD6A0 / Warn #E8B44C / Danger #FF5C47 / DangerBg #8A1E17 | 状态色 | 图标与错误框 |
| TextPrimary #F6F1E8 / TextSecondary #B8C4CE | 文字 | |

## 6. 运行时契约绑定（重建工具负责填写）

| 契约字段 | 目标节点 |
|---|---|
| Form._pageBindings[0..4]._pageRoot | Panel_PageOverview/Tasks/Objects/Rules/Statistics |
| Form._pageBindings[i]._navigationLabel | Txt_Tab{Page} |
| Form._pageBindings[i]._activeNavigationVisual | Img_TabActive{Page} |
| Tab Button.onClick（持久） | form.SelectPage(0..4)，Explicit 左右环形导航 |
| Form._defaultFocus | Btn_TabOverview（Button） |
| Form._machineRosterSummary | Txt_MachineRosterSummary |
| Form._rulesImpactOverlay | Overlay_RulesImpact |
| Form._dangerConfirmationView | Overlay_RulesImpact 上的 AutoEraDangerConfirmationView |
| Op._sourceId / _spinner / _progress / _phaseText / _progressFill | "rules" / Txt_RuleOperationBusy / Bar_RuleOperationProgress / Txt_RuleOperationPhase / 同 Bar 的 Image |
| Op._longWaitHint / _cancelledState / _successState | Txt_RuleOperationLongWait / Txt_RuleOperationCancelled / Txt_RuleOperationSuccess |
| Op._failureState | Img_RuleConfigError |
| Op._cancelAction / _retryAction / _detailsAction（+同名 Button 字段） | Btn_RuleOperationCancel / Retry / Details |
| HoldToConfirmView（挂 Txt_RuleDangerHint） | _hintText=自身；_progressRing=_progressRoot=Img_HoldProgressRing |
| AutoEraReduceMotionEntry（挂 Btn_ReduceMotion） | _label=Txt_ReduceMotionLabel；_button=自身 Button |
| AutoEraUiAccessibilityDescription | Txt_RuleOperationLongWait、Btn_RuleOperationDetails |
| AutoEraUiCancelIntentProxy | 全部 Selectable（工具自动补挂） |
| 运行时代码 AddListener（不落 prefab） | 操作三按钮、危险确认二按钮、减弱动态按钮 |

## 7. 验收口径

- 命名 100% 在词表内；Form 内节点名唯一；无序号节点。
- 全部 SerializeField 引用非空且语义对应；无 Missing。
- 三个 List_* 均为 ScrollRect+Viewport+Content+LayoutGroup+唯一 Item_*Template；无具名预览行。
- 装饰 Graphic raycast=false；List 底图/Scrim/Hold 提示 ray=true（可交互面）。
- 零假业务数据；占位文案均为功能提示。
