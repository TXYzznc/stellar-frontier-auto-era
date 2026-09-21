# 硬件修改链接线（整备页槽位选择 → 17-硬件修改确认 → 领域执行）

## Why

上一批（`machine-preparation-readout`）把整备页三栏变成了真实数据，但页面仍然是**只读**的：
「安装或拆卸」按钮被 `ApplyUnavailableActions` 无条件禁用，槽位行也只是纯展示行。
玩家能看到「核心槽 0 里装着一颗核心」，却没有任何办法把它拆下来。

侦察后的结论是：这一条链需要的每一段**都已经在生产里**，缺的只是把它们接起来——

| 链路上的环节 | 生产里的既有实现 | 状态 |
|---|---|---|
| 硬件修改的执行（来源门禁、等待安全停机、失败判定、请求版本取消） | `MachineHardwareOperation`（`MachineRoster.GetHardwareOperation(id)` 创建） | 完整，**但除测试外无调用者** |
| 槽位内容与占用判定 | `MachineInstance.GetComponent(kind,index)` / `HardwareGate` | 真实 |
| 确认对话框的页面与控件 | `Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json` 的 HardwareConfirm 页（页码 6） | 预制体已在 |
| 界面把「拆哪一格」交出去 | 无 | **缺**（本次补） |
| 界面把「装什么」交出去 | 需要 12-组件选择器的选中结果 | **仍缺**（见下） |

所以这不是新建系统，而是**接线**：界面只负责收集意图，执行全部交给 `MachineHardwareOperation`。
唯一的真缺口是「装入」方向——它必须先经 12-组件选择器取得一个散件身份，而选择器尚未接线；
本次只接通**拆卸**方向，并在界面上把装入方向的缺口明说。

## What Changes

- 新增 `AutoEraHardwareRequest`（`Assets/Game/Scripts/AutoEra/UI/Integration/AutoEraHardwareRequest.cs`）：
  「哪台机器、哪一类硬件的第几格、装还是拆、装的是谁」。
  **刻意不带来源（origin）**——来源由确认页按机器的部署状态推导
  （未部署 → `ManagementOrigin.Library`，已部署 → `ManagementOrigin.Field`），
  因为那正是领域 `HardwareGate` 的判据；让调用方传一个可能与机器状态矛盾的来源，
  只会制造「界面说整备、领域说现场」的假分歧。
- `AutoEraUiFormat` 增加 `Slot(kind,index)` / `SlotKind(kind)` / `ManagementResult(result)`：
  槽位名字与拒绝原因**只有一处**，整备页、确认页与将来的组件选择器必须说同一句话。
- `OperationDialogForm` 改为**参数驱动**：带 `AutoEraHardwareRequest` 打开时直接落在
  HardwareConfirm 页，渲染「本次改动」「运行影响」两栏并提交给 `MachineHardwareOperation`；
  没有请求时呈现 Disabled 并说明「本界面由调用方带参数打开」。提交状态与拒绝原因都从
  operation 读回来，不是界面自己猜的。
- `MachineLibraryForm`：整备页的**槽位行可选中**（`HasSlotSelection` /
  `SelectedSlotLabel` / `SelectedSlotComponent()`）；「安装或拆卸」只在选中**已占用**槽位时可点，
  点击后打开 17 的硬件修改确认页。选中态用「它属于哪台机器」自我校验，换机器自动作废。
- 契约与生成器：`Tools/ui_spec_to_contract.py` 的 `EXTRA_BINDINGS` 为 `OperationDialogForm`
  增加 `_hardwareConfirmKeepButton` / `_hardwareConfirmCommitButton`，
  `Tools/ui_contract_to_form_script.py` 的 `HANDWRITTEN` 收入 `OperationDialogForm`。
  **预制体结构不变**，只是补了两个绑定引用。

## Capabilities

### New Capabilities

- `machine-hardware-modification`：硬件修改的界面链路（意图 → 确认 → 领域执行）。

### Modified Capabilities

- 无。本变更不改预制体结构、不改 `UIViews` 登记、不改数据表、不改本地化表。

## Impact

- 代码：
  - 新增 `Assets/Game/Scripts/AutoEra/UI/Integration/AutoEraHardwareRequest.cs`
  - 修改 `Assets/Game/Scripts/AutoEra/UI/Integration/AutoEraUiFormat.cs`
  - 重写 `Assets/Game/Scripts/AutoEra/UI/OperationDialogForm.cs`（改为手写，生成器只供字段）
  - 修改 `Assets/Game/Scripts/AutoEra/UI/MachineLibraryForm.cs`
  - 工具：`Tools/ui_spec_to_contract.py`、`Tools/ui_contract_to_form_script.py`、
    新增 `Tools/_unity_refresh_bindings.py`（加绑定后必须刷新的那个菜单）
- 验收：门1 必须持续 33/33；`run_project_checks.py` 5/5；EditMode/PlayMode 回归全绿；
  新增 `HardwareOperationEditModeTests`、`MachineLibrarySlotSelectionPlayModeTests`、
  `OperationDialogHardwareConfirmPlayModeTests`。
- **不在本变更内**：**装入**方向（等 12-组件选择器接线）、一键卸下全部、
  升级／改名／出售（各自要经 11／17 家族的对话框）。
- **风险点**：界面很容易图省事直接在整备页调 `MachineRoster.Remove`，
  那样「硬件修改必须经确认」这条规格就名存实亡。所以三条用例的断言分别落在
  「未选槽位时入口不可点」「确认之前机器没有被改动」「确认之后改动真的发生」上。
