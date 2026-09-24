# 一键卸下全部（整备环境原子回库）

## Why

批次 7 打通了装入与拆卸两个方向，但整备页的第三个硬件动作一直是空的：
`RefreshSlotActions()` 里写死 `SetInteractable(_preparationUnloadButton, false)`，
按钮永远不可点。阅读模型里那条「一键卸下影响」反而**早就在算**（会卸下几件、回到组件库、
容量与算力随之下降）——界面在解释一个它不允许玩家做的动作。

这个缺口不需要新系统：`MachineRoster` 已经能逐槽位拆、`MachineHardwareOperation` 已经管着
来源门禁／等待安全停机／请求版本取消。缺的只有一件事——**「整台一起卸」这个领域动作本身**，
以及它必须满足的那条字符串：规格 05-机器整备写的是「确认全部卸下影响并**原子**回库」。

「原子」是这个变更唯一真正的新要求，而且它最容易被实现丢掉：最自然的写法是
「遍历每个槽位，逐件调 `Remove`」，那会在中途撞上余量不足而失败，留下一台「卸了一半」的机器——
三件里少了一件，而没有任何一处告诉玩家为什么。

顺带修正一处**过期的范围判断**：批次 9 之前的备注把「一键卸下」与「升级／出售」并列成
「经济域未接入」，但侦察发现它根本不依赖经济域——它只依赖机器域的槽位归属，
「回库」在实现里就是**清空 `OwnerId`**（组件因此重新成为组件库里的散件）。

## What Changes

### 领域（机器域）

- `MachineInstance.RemoveAll(ManagementOrigin)`（internal）：
  - 来源**比单槽拆卸更严**：只接受整备环境（`Library`）。规格把「一键卸下」列在
    「未部署机器处于整备环境，可以……一键卸下」之下，而已部署机器「硬件修改与回收必须现场完成」
    ——批量拆一台正在服役的机器不是这一版设计里的动作，所以这里如实拒绝而不是顺手放开。
  - **先算「全部卸下之后」的余量，通过了再一次性清空**：卸完只剩 `BaseCapacity`，
    算力与逻辑占用必须归零（含等待算力的计数）。任一条不满足就整体拒绝，**一件都不动**。
  - 一件都没装时返回 `MissingComponent`——这个意图没有意义，如实拒绝而不是报成功。
  - 应用时清空每个槽位并把组件的 `OwnerId` 置为 `Invalid`（＝回库）。
- `MachineRoster.RemoveAll(machineId, origin)`：按 id 转发，未知机器返回 `InvalidState`。
- `MachineInstance.InstalledComponentCount`：跨三个类别的已装件数（界面与测试都要用它判断「有没有东西可卸」）。
- `MachineHardwareOperation.BeginRemoveAll(machineId, origin)`：与单槽拆卸走**同一条**协调器
  （同样的来源门禁、同样的等待安全停机、同样的请求版本取消）。另起一条路径就会让
  「什么时候不能改硬件」出现第二份判断。内部只多一个 `_removeAll` 标志。

### 界面

- `AutoEraHardwareRequest`：增加**一键卸下**形态——`UnloadAll(machineId)` 工厂 ＋ `RemoveAll` 属性，
  用 `AllSlots = -1` 这个**哨兵**表达「不是某一格」（填 0 号槽会让确认页显示一个并不存在的目标槽位）。
- `OperationDialogForm`（17-硬件修改确认）：
  - 一键卸下时变更清单**逐槽位列出将要卸下的每一件**（规格把「槽位」与「卸下实例」都列进了变更清单），
    加一行汇总与「库存去向」；空槽位不列。
  - 能力变化按「卸完之后」算：只剩载体基础容量、算力与逻辑归零。
  - 提交走 `BeginRemoveAll`；来源仍按机器的部署状态推导（界面不自己判断）。
- `MachineLibraryForm`（05-机器整备）：
  - 「一键卸下」接上入口：`CanUnloadAll`＝选中机器 ＋ 未部署 ＋ 真的装着东西；
    一件都没装或已部署时保持禁用（不给一个只会被拒绝的按钮）。
  - 整备页正文说明它不需要先选槽位，且它作用于整台机器。

## Capabilities

### New Capabilities

- `machine-unload-all-wiring`：整备环境的一键卸下全部（原子）、回库语义与确认页接线。

### Modified Capabilities

- `ui-game-system-integration` 的 2.7：整备页的「一键卸下」从「经济域未接入」更正为**已接线**
  （它不依赖经济域）。

## Impact

- 领域：`Assets/Game/Scripts/AutoEra/Machines/MachineInstance.cs`、`MachineRoster.cs`、
  `MachineHardwareOperation.cs`。
- 界面：`UI/Integration/AutoEraHardwareRequest.cs`、`UI/OperationDialogForm.cs`、`UI/MachineLibraryForm.cs`。
- 验收：门1 必须持续全绿（**不改契约**，`_preparationUnloadButton` 早有绑定）；
  `run_project_checks.py` 5/5；EditMode 全类、PlayMode 12 套全绿；
  新增 `MachineUnloadAllEditModeTests` 13 项，并把
  `MachineLibrarySlotSelectionPlayModeTests` 扩成「槽位拆卸＋一键卸下」两段端到端。
- **不在本变更内**：升级载体（11-载体升级）、出售（经济域）、一键卸下在**已部署**机器上的形态
  （设计把现场硬件修改写成逐项完成）。
- **风险点**：把「原子」实现成「逐件卸」是本变更唯一会造成真实伤害的错法——
  它不会报错，只会留下一台半卸的机器。用例
  `RemoveAll_IsAtomic_WhenTheContainerWouldNotFitNothingIsRemoved` 与
  `RemoveAll_IsAtomic_WhenComputeOrLogicIsStillReserved` 专门构造「先成功一件、第二件必失败」
  的场景守住它。
