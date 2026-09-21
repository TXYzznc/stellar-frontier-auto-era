# 安装替换组件选择器接线（装入方向）

## Why

上一批（`machine-hardware-modification-wiring`）把硬件修改链接到了**拆卸**方向：整备页选中
已占用的槽位 → 17-硬件修改确认 → `MachineHardwareOperation` 执行。**装入**方向当时留了缺口——
它需要先有人回答「装哪一件」，而那个回答在规格里是独立的一页（12-选择与绑定 · ComponentPicker）。

侦察后的结论与前三批一样：这一页需要的数据**全部已经在生产里**——

| 规格要求的分区 | 生产里的既有实现 | 状态 |
|---|---|---|
| 候选组件：未安装实例；型号／等级 | `MachineRoster.Components` 里 `OwnerId` 无效的那些 ＋ `ComponentDefinitions` 表 | 真实（`ComponentReadModel` 的散件一列已经在用） |
| 占用与锁定状态 | 目标槽位的 `MachineInstance.GetComponent(kind,index)` | 真实 |
| 与当前安装比较：属性差值／槽位／算法绑定影响／不兼容原因 | 组件型号的 `AddedCapacity` / `ComputeCapacity` / `LogicCapacity` 与机器当前值 | 真实 |
| 结果交回调用方 | `AutoEraUiSelectionRequest`（本工程「界面把结果交回调用方」的既定做法） | 已在 |
| 进入 17-硬件确认 | `AutoEraHardwareRequest(..., remove: false)` ＋ `OperationDialogForm` 的装入方向（上一批已写好并渲染） | 已在 |

所以这仍然是**接线**，不是新建领域。唯一真缺口是候选列表与比较栏这一层投影，以及一个入口。

顺带记一条判据上的教训：`ComponentPickerForm` 原本被 `ui-game-system-integration` 的 2.7
列为「领域未接入」（理由写作「组件域尚未接入运行路径」），但组件域在
`component-domain-readonly-wiring` 那一批就已经接线。这已经是**第三次**同一类误判
（前两次是 `ComponentLibraryForm` 与 `SettingsForm`），三次的错误形态完全一样：
把「还没有这一页的读模型」当成了「这个域没有后端」。

## What Changes

- 新增 `AutoEraComponentPickRequest`：**目标槽位**（机器身份 ＋ 硬件类别 ＋ 槽位序号）＋
  结果写回处。它刻意不描述「挑哪一件」——那是玩家在页面里做的决定。
- 新增 `ComponentPickerReadModel.cs`：
  - `UiComponentCandidate`：一件散件 ＋ 它对这个槽位是否兼容。**兼容判据只有「类别与槽位类别一致」一条**，
    其余不可安装因素（可用性等）照原样展示——界面不自己定第二套「什么时候能装」的规则，
    那是领域 `HardwareGate` 的职责。
  - `ComponentPickerSnapshot`：目标槽位、当前安装、候选、预选、比较行、确认被拦下的原因。
  - `RosterComponentPickerReadModel` **组合** `IComponentReadModel`（不自己再扫一遍花名册）：
    「哪些组件算散件」「型号叫什么名字」都已经有一处实现，复制一份迟早出现两种口径
    （上一批刚踩过：确认页按型号编号显示、整备页按目录名显示）。
  - 在花名册变化时重建快照；**预选项在候选里找不到时自动作废**（被装到别处之后不得再拿着它提交）。
- `ComponentPickerForm` 改为手写：候选走对象池、点行只改预选项、比较栏随预选更新、
  「使用该组件」写回候选身份并打开 17-硬件确认、取消写回「玩家取消」。
  确认按钮只在**预选了兼容候选**时可用，被拦下时把原因写在页面上。
- `MachineLibraryForm`：「安装或拆卸」按格子分流——已占用的格子去拆卸确认，**空格子去选择器**；
  选中任意槽位即可用，不再要求「已占用」。
- 契约与生成器：`EXTRA_BINDINGS` 为 `ComponentPickerForm` 增加标题与三个动作按钮；
  `HANDWRITTEN` 收入 `ComponentPickerForm`，`NOT_WIRED` 移除它。
  **预制体结构不变**，只是补了绑定引用。

## Capabilities

### New Capabilities

- `component-picker-install`：装入方向的界面链路（散件候选 → 比较 → 选择器 → 17 确认 → 领域执行）。

### Modified Capabilities

- `ui-game-system-integration` 的 2.7：`ComponentPickerForm` 从「未就绪域」名单迁出，
  围栏名单由 13 个界面调整为 12 个。

## Impact

- 代码：
  - 新增 `Assets/Game/Scripts/AutoEra/UI/Integration/AutoEraComponentPickRequest.cs`
  - 新增 `Assets/Game/Scripts/AutoEra/UI/Integration/ComponentPickerReadModel.cs`
  - 重写 `Assets/Game/Scripts/AutoEra/UI/ComponentPickerForm.cs`（改为手写，生成器只供字段）
  - 修改 `Assets/Game/Scripts/AutoEra/UI/MachineLibraryForm.cs`（空槽位入口）
  - 工具：`Tools/ui_spec_to_contract.py`、`Tools/ui_contract_to_form_script.py`
- 验收：门1 必须持续全绿；`run_project_checks.py` 5/5；EditMode/PlayMode 回归全绿；
  新增 `ComponentPickerReadModelEditModeTests` 与 `ComponentPickerInstallPlayModeTests`。
- **不在本变更内**：一键卸下全部、升级替换（11-改装）、组件比较模式（从组件库发起的比较）、
  经济域（买进散件的唯一现实来源，尚未接入）。
- **风险点**：界面很容易图省事在候选被选中时自己装上去。所以本批的核心断言是
  「确认之前槽位仍然是空的、组件还没有归属」，以及「确认之后领域真的装上了」——
  只断言「确认页打开了」会漏掉「三段都开了但谁都没装」。
