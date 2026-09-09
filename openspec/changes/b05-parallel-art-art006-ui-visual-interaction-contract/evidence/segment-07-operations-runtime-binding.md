# Segment 07：Operations UIForm 与异步交互接入现场

日期：2026-09-09

## 已完成实现

- 已建立 `AutoEraUiRuntime`、`AutoEraUiIntentRouter` 与无设备轮询的 `AutoEraUiIntentInputAdapter`；Form 在 `OnOpen` 注册、在 `OnClose`／`OnRecycle` 对称注销。
- `BaseCommandHubForm` 已绑定五个稳定页面入口、显式左右导航和规则影响确认层；`FieldHudForm` 已作为独立稳定 Form 接入。
- 危险确认层支持确认描述正文、缺失配置红色错误框、Danger 禁用、默认取消焦点、取消／确认统一意图处理与关闭后触发控件焦点恢复。
- 已新增 `AutoEraHoldToConfirmView`：使用既有 `c05-hold-danger-holding` 视觉资源，固定 1.2 秒门禁，松开或移出即取消，只有到达门槛才触发确认事件。
- 原位操作绑定以权威 `AutoEraUiOperationSnapshot` 驱动：仅可信 0–1 进度显示进度，长时等待仅补充“仍在处理中／查看详情”，不会由本地计时改写成功、失败、取消或请求丢失终态。

## 资源目录迁移

- `rapid-executor` 已通过 AssetDatabase 将 201 个 ART-006 PNG 迁入 `Assets/Game/Sprites/UI/Operations/`：
  - `Common`：102；
  - `CommandHub`：56；
  - `FieldHud`：10；
  - `MachineNodes`：33。
- 过程根 `Assets/Game/Art/UI/ART006_UI/Sprites/` 当前不再保留 PNG。`c05-hold-danger-holding.png` 的 GUID 为 `0d5c1cbd0136ea94c8f52806fd66f48b`，正式 `BaseCommandHubForm.prefab` 仍以该 GUID 引用。
- 迁移后已执行常规 AssetDatabase Refresh 和 Operations Prefab 重绑；当前 Console：Error=0、Warning=0。

## 数据表生成对齐

`Launch.unity` 已由场景所有方保存；编辑器当前非 PlayMode、非编译。随后完成以下受保护的数据对齐，不直接编辑任何 `.xlsx`：

1. 在 `Temp/b05-ui-table-baseline-checkpoint-20260909/` 保存对齐前的 `UITable.json` 与 `UITable.xlsx`，并记录两条确认行及 SHA-256；
2. 用正式 `Export DataTables Json` 从当前工作簿建立新基线；
3. 只回填稳定入口 6001 `Operations/FieldHudForm` 与 6002 `Operations/BaseCommandHubForm`，字段、备注和 `_cells` 保持一致；
4. `Validate DataTables Json` 为 5/5 成功、0 警告；框架精确 Reverse `Core/UITable` 报告为 1/1 成功，仅改 2 行、14 个单元格；
5. 使用框架 `GenerateUIFormNamesScript` 生成 `UIViews.FieldHudForm = 6001` 和 `UIViews.BaseCommandHubForm = 6002`，不手改生成文件。

## 原生 PlayMode 结果桥接与运行时绑定验收

- UnitySkills 的 `test_run_by_name` PlayMode 作业在域重载后会丢失可查询 job；为避免把不可追溯的 job 视为结果，新增原生 Unity Test Runner 回调。它只为 `AutoEraOperationsUiFormPlayModeTests.OperationsForms_OpenAndCloseThroughUiExtension` 将 NUnit XML 写到 `Temp/AutoEraTestResults/operations-uiform-playmode.xml`，不写入 Assets 或业务数据。
- 用例显式加载 Build Settings 中唯一启用的正式 `Assets/Game/Scene/Launch.unity`，等待 `UITable`、`UIGroupTable` 与 `Default` UIGroup 均就绪后，分别通过 `UIExtension.OpenUIForm` 打开 `FieldHudForm`（6001）和 `BaseCommandHubForm`（6002），并逐一确认关闭完成。
- 最终原生 XML：2026-09-09 07:13:15Z～07:13:17Z，结果 `Passed`，耗时 1.611115 秒；编辑器返回 EditMode，非暂停、非编译，Console Error=0。
- 初次真实运行暴露 Prefab 文本使用 `LiberationSans SDF`，中文和状态符号会变为方框。现有正式动态 TMP 字体 `Assets/Game/Fonts/UI/AlibabaPuHuiTi-3-85-Bold SDF.asset` 已由 `AutoEraOperationsPrefabBinder` 统一绑定到两枚 Operations 运行入口。复跑后的原生 XML `missingGlyphWarnings=0`，不再出现缺字替代。
- 为覆盖 EventSystem 物理输入边界，所有 Operations `Selectable` 已配置 `AutoEraUiCancelIntentProxy`：它只转发 EventSystem 的 Cancel 事件到 `AutoEraUiIntent.Cancel`，不轮询 `Input`／`KeyCode`。Hub 在打开时将焦点置于总览页签；Cancel 由顶层 Form 消费、关闭 Hub 并恢复打开控件焦点。2026-09-09 的最终 Launch 回归 `Passed`，耗时 1.593233 秒，Console Error=0、缺字警告=0。

## Launch 运行态性能基线（非 4.4 验收）

- 2026-09-09 以保存的 `Launch.unity` 进入 PlayMode 后采集只读快照：75 triangles、115 vertices、9 batches／draw calls／set-pass calls；两个瞬时采样的 frame time 为 0.0085ms 与 0.0213ms，render time 约 0.0011ms；allocated memory 约 404.37MB。
- 该快照只说明干净 Launch 基线可运行，未包含两枚 Operations Form 同时打开后的 Canvas rebuild、GC、列表池化／虚拟化或图片内存证据，故不能作为 4.4 通过依据。
- 2026-09-09 完成 Operations 原位操作节点补齐：`Btn_RuleOperationCancel`、`Btn_RuleOperationRetry`、`Btn_RuleOperationDetails`、`Bar_RuleOperationProgress`、`Txt_RuleOperationLongWait`、`Txt_RuleOperationCancelled` 与 `Btn_ReduceMotion` 全部从已验收 Operations 文本／状态样式克隆，写入两枚稳定运行入口而未改动 V01～V04 视觉链。
- `AutoEraUiOperationVisualBinding` 对取消、重试、详情按钮进行权威状态门禁，并将包含 request ID、source ID、详情目标与重试确认标记的 `AutoEraUiOperationActionRequest` 交由外部权威操作拥有者处理；UI 不自行执行或伪造结果。成功状态通过 `AutoEraUiVisualTimer` 保留约 2 秒后仅复位视觉，操作权威状态不被改写。
- “减弱动态”入口在运行时切换非必要旋转图标，阶段文字、长时等待说明和可信进度仍保留。`Txt_RuleOperationLongWait` 挂接可访问性说明“操作仍在处理中，可查看详情”，供项目的焦点／读屏适配层消费。
- EditMode 自动门禁：`AutoEraUiPrefabBindingEditModeTests` job `6af79849` 3/3；`AutoEraUiOperationContractsEditModeTests` job `d2318778` 7/7；Console Error=0、Warning=0。此前同一节点门禁 job `a585695f` 3/3 与 `ebd784a8` 7/7 亦通过。
- 原生 PlayMode 回归（保存的 Launch）：`OperationsForms_OpenAndCloseThroughUiExtension` 于 2026-09-09 07:46:49Z～07:46:53Z `Passed`，3.760832 秒。覆盖 GF 真实打开／关闭 6001、6002，默认焦点和取消恢复，以及执行中→失败→成功复位状态序列；Console Error=0，测试输出无字体缺字替代。故勾选 3.4、3.5；4.2～4.5 仍待完整输入／状态、性能和 GF UI Standards 清单验收。
- 4.2 输入证据：既有 `AutoEraUiIntentRouterEditModeTests` 覆盖顶层消费、不可关闭 FieldHud 跳过和无设备轮询；正式 PlayMode 覆盖 EventSystem Cancel→Hub 关闭→触发控件焦点恢复。五页导航按钮均采用显式左右 `Selectable` 导航并汇聚到同一页选择逻辑。
- 4.3 状态矩阵证据：`AutoEraUiPrefabBindingEditModeTests` job `da7db42f` 4/4 通过，断言正式 Prefab 中 `idle`、执行中、可信进度、长时等待、失败、取消、请求丢失及状态门禁的可见节点；Console Error=0、Warning=0。危险确认缺失配置、默认取消焦点与 1.2 秒长按由 `AutoEraUiOperationContractsEditModeTests` 的 7/7 门禁覆盖。故勾选 4.2、4.3；4.4、4.5 仍待性能与清单收口。

## Operations 状态节点美术复核闭环

- 复核发现并已修正按钮背景最初作为 TMP 文字子节点时可能遮挡文字的风险；正式结构现在固定为“可点击背景父节点 → TMP 文字子节点”。背景使用既有 `c02-action-secondary`（GUID `5baf81677504ba848ae9266f0bf534ac`），`Image.Type=Sliced`、`raycastTarget=true`；TMP 为 `raycastTarget=false`，`Button.targetGraphic` 指向背景自身。
- v2 复核确认默认显示、长等待宽度与射线目标正确；v3 发现重复绑定后 TMP 会保留页面级坐标。Binder 现每次都将文字重置为填满背景：`anchorMin=(0,0)`、`anchorMax=(1,1)`、`anchoredPosition=(0,0)`、`sizeDelta=(0,0)`；父背景保留页面坐标。v4 只读复核确认四个按钮均满足这一层级与布局合同，未发现剩余静态布局缺陷。
- 状态矩阵最终 EditMode 门禁：`AutoEraUiPrefabBindingEditModeTests` job `970c3ad0` 4/4 通过；其后按钮文字布局防回归门禁 job `970c3ad0` 的同一结构已覆盖。最终回归 job `970c3ad0` 后由 v4 复核确认。Console Error=0、Warning=0。
- 最终原生 PlayMode：保存后的 `Launch` 场景执行既有 `Game Framework/AutoEra/QA/Run Operations UIForm PlayMode Test`，`Temp/AutoEraTestResults/operations-uiform-playmode.xml` 中 `OperationsForms_OpenAndCloseThroughUiExtension=Passed`，耗时 `3.791461s`；退出 PlayMode 后 Console Error=0、Warning=0。
