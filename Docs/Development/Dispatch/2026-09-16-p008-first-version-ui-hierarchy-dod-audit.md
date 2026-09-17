# P0-008 第一版 UI 层级 DoD 核验（2026-09-16）

- **任务**：`p0008-first-version-ui-hierarchy`
- **核验范围**：第一版 HUD、现场面板、中枢管理界面及其模态确认层的 `UIFormBase/GF.UI` 层级、取消输入、焦点恢复与世界输入阻断。
- **不在范围**：重做 UI 视觉或预制体、设计新的玩家入口、修改 `.xlsx`、`Assets/Game/ScriptsBuiltin/` 或 Git。
- **结论**：正式 UIForm 配置、GF.UI 打开链、子层归属和输入优先级已存在并符合 P0-008 的结构性 DoD；本任务不需要新增生产代码或视觉资源。发现一个**产品入口缺口**：中枢目前只找到开发者运行时证据菜单入口，尚未发现面向玩家的正式触发入口；此项不应在没有交互规格的情况下擅自补键位或按钮。

## 1. 任务表、队列与项目状态对照

| 核对对象 | 实际结果 |
| --- | --- |
| 任务表 | `Docs/GameDesign/05-开发计划/第一版开发任务表.xlsx` 的 `P0-008` 要求“基于 ART-006 和 UIForm 层级建立 HUD、现场面板、管理页和模态层”，DoD 为“UI 均通过 GF.UI 打开、Esc 层级正确、场景不因 UIForm 打开而停止”。任务表单元格仍显示“未开始”，但该状态受本任务禁止修改 `.xlsx` 约束，不能作为实际完成状态回写。 |
| 本窗口队列 | `python tools/window_task_queue.py status --role client`：仅有 Active `p0008-first-version-ui-hierarchy`；没有 pending 或 suspended 项。此前 P0-009、P0-012 已完成。 |
| 工作区 | P0-008 核验未修改任何生产脚本、Prefab、数据表或视觉资源；工作区中存在其他窗口的未提交改动，未清理、还原、暂存或触碰。 |

## 2. 正式 UIForm 注册与入口

`Assets/Game/DataTable/Core/UITable.txt` 已登记三个正式 UI：

| UIViews / ID | Prefab | UI Group | Sort order | Pause Covered UI | Escape close | 正式打开点 |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| `MainMenuForm` / 6000 | `Startup/MainMenuForm` | 1 | 0 | false | false | `AutoEraMainMenuProcedure.OnEnter` |
| `FieldHudForm` / 6001 | `Operations/FieldHudForm` | 1 | 0 | false | false | `AutoEraWorldProcedure.OnEnter` |
| `BaseCommandHubForm` / 6002 | `Operations/BaseCommandHubForm` | 1 | 100 | true | true | `InitialRegionRuntimeEvidence.OpenCommandHub`（仅开发者菜单） |

对应枚举由 `Assets/Game/Scripts/UI/Core/UIViews.cs` 生成。启动菜单和世界流程分别经 `GF.UI.OpenUIForm(UIViews.MainMenuForm, ...)` 与 `GF.UI.OpenUIForm(UIViews.FieldHudForm, ...)` 打开；不是场景内临时 Canvas。

中枢同样通过 `GF.UI.OpenUIForm(UIViews.BaseCommandHubForm)` 打开，但当前项目中只检索到 `Assets/Game/Scripts/AutoEra/Editor/InitialRegionRuntimeEvidence.cs` 的 `Game Framework/AutoEra/Runtime Evidence/Open Command Hub` 菜单入口。没有发现生产玩家输入、HUD 按钮或流程入口。P0-008 未定义入口交互，故不越权添加。

## 3. 实际层级与生命周期

1. 所有独立页面继承 `AutoEraUiFormBase`。该基类在 `OnOpen` 注册到 `AutoEraUiRuntime`，在 `OnClose` / `OnRecycle` 反注册，并保存和恢复打开控件焦点：
   - `Assets/Game/Scripts/AutoEra/UI/AutoEraUiFormBase.cs`
2. `FieldHudForm` 是独立 GF.UI Form；它的 `MachineHardwarePanel` 是已序列化的预制子层（字段 `_machinePanel`），并非新的 `UIViews` 或独立 Form：
   - `Assets/Game/Prefabs/UI/Operations/FieldHudForm.prefab`
   - `Assets/Game/Prefabs/UI/Operations/MachineHardwarePanel.prefab`
   - `Assets/Game/Scripts/AutoEra/UI/FieldHudForm.cs`
3. `BaseCommandHubForm` 是独立 GF.UI Form，包含 Overview、Tasks、Objects、Rules、Statistics 五页绑定。规则影响确认由 `Overlay_RulesImpact` 这个中枢预制子层承载，绑定 `AutoEraDangerConfirmationView`；符合“所属页面 Overlay 不另建 UIForm”的规范：
   - `Assets/Game/Prefabs/UI/Operations/BaseCommandHubForm.prefab`
   - `Assets/Game/Scripts/AutoEra/UI/BaseCommandHubForm.cs`
   - `Assets/Game/Scripts/AutoEra/UI/AutoEraDangerConfirmationView.cs`

## 4. Esc / Cancel 优先级与焦点

- `AutoEraUiIntentRouter` 以注册顺序倒序分发，顶层 Form 消费输入后立即终止，不会向下重复派发。
- `AutoEraUiFormBase.TryHandleIntent` 先调用 `OnBeforeFormIntent`，再按 Cancel 走 GF 的关闭路径。
- 中枢先把输入交给 `AutoEraDangerConfirmationView`：可见确认层收到 Cancel 时只隐藏确认层并恢复其触发控件焦点；确认层关闭后，才由中枢响应 Escape Close。
- HUD 先把输入交给 `MachineHardwarePanel`；其子面板接收 Cancel 时关闭自身，不关闭 `FieldHudForm`。
- Form 关闭时，`AutoEraUiFormBase` 恢复 Form 打开前保存的 EventSystem 焦点。

这与 `Docs/Development/GF-UI-Standards/05-输入与界面生命周期规范.md` 的“顶层 UI 消费后底层不得再次响应”一致，也符合 ART-006 已确认的“确认层 → 右侧现场面板 → 中枢全页”关闭顺序。

## 5. 世界输入和世界模拟

- `AutoEraUiRuntime.BlocksWorldInput` 由运行时已注册 Form 计算。
- HUD 只有在其 `MachineHardwarePanel.IsOpen` 时阻断世界输入；中枢等其它活跃 Form 则阻断世界输入。
- `RegionInputModule.Tick` 在 `blocked || managementOpen` 时清除悬停并提前返回，因此不继续相机、选择、放置或对象交互。
- `AutoEraWorldProcedure.OnUpdate` 仍执行 `_entry?.Advance(realElapsed)`。

因此，任务表中“场景不因 UIForm 打开而停止”在当前项目的实际含义是：**UI 打开时阻断世界交互，但不未经产品规格授权地停止世界模拟或设置 `Time.timeScale = 0`**。这也与 GF UI 标准“页面暂停不等于世界暂停；由游戏状态/流程服务决定模拟是否推进”一致。

## 6. 验证证据

### 已完成的可复核验证

| 验证 | 结果 | 证据 |
| --- | --- | --- |
| UI 配置、正式 Prefab、生产打开点静态核验 | 通过；中枢玩家入口缺口已记录 | 本文第 2 节；`UITable.txt`、三个 Procedure / Editor 入口文件 |
| 顶层输入不向底层重复派发 | 通过 | `AutoEraUiIntentRouterEditModeTests` 的两个用例均通过 |
| Hub 五页、导航、规则 Overlay / 危险确认引用 | 通过 | `AutoEraUiPrefabBindingEditModeTests.CommandHub_BindsFivePagesAndNavigationButtons` 通过 |
| HUD Form 和运行时绑定 | 通过 | `AutoEraUiPrefabBindingEditModeTests.FieldHud_HasStableRuntimeFormComponent` 通过 |
| 启动菜单 / 世界 / HUD 生命周期回归 | 通过 | `AutoEraStartupFlowEditModeTests` 的 6 个用例均通过 |
| 全量 EditMode 基线 | 480 总，477 通过，3 失败 | 2026-09-16 17:03:09，`C:/Users/WIN10/AppData/LocalLow/ZZNC/星际拓荒：自动纪元/TestResults-p0012-full-editmode-20260916.xml` |

### 既存失败（不归因 P0-008）

全量 EditMode 的 3 个失败中，唯一相关测试是：

```text
AutoEraUiPrefabBindingEditModeTests.OperationsEntryPrefab_IsIndependentOfTheRetiredVisualCandidateChain("Assets/Game/Prefabs/UI/Operations/FieldHudForm.prefab")
```

该失败指向 `FieldHudForm.prefab` 仍有已退休 ART-006 视觉候选链依赖；本任务未修改该 Prefab，且这不是 UI 层级或 GF.UI 接入的新失败。另两项失败分别位于 `DataTableGenerationProfileEditModeTests` 和 `InitialRegionSceneEditModeTests`，与 P0-008 无关。

### 运行期补充验证限制

`MachineHardwareUiPlayModeTests` 已覆盖“现场面板打开时 `BlocksWorldInput` 为真，Cancel 只关闭子面板而 HUD Form 保留”。本次无法通过本机执行该针对性 PlayMode：任务技能定义的 `uloop` CLI 在当前 shell 不存在，而项目正由用户启动的 Unity Editor 占用，不能安全地并发启动第二个 batchmode Editor 争用项目 `Library`。此项被明确记录为**未新增的运行期复跑证据**，不是伪报为通过。

## 7. 交付与后续建议

- P0-008 的层级接入、GF.UI 入口（已有入口范围内）、Esc 处理与输入阻断均已核验；无生产代码或资源改动。
- 后续应由具有产品交互范围的任务定义并接入“打开中枢”的正式玩家触发控件／输入；不要以开发者 Runtime Evidence 菜单代替正式功能。
- 后续 ART/Prefab 维护应单独处理 `FieldHudForm.prefab` 的退休候选视觉依赖，并在处理后重跑对应 Prefab 绑定测试。
