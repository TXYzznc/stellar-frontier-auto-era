# Tasks: 本机货舱读取与任务查询节点

## 1. 机器货舱契约

- [x] 1.1 新增 `MachineCargo` 类（`AutoEra.Machines`）：`int Capacity/Used`、`Remaining`、`Dictionary<string,int>` 物品明细、`event Action Changed`，含装载/卸载与超容拒绝。
- [x] 1.2 挂接 `MachineCargo` 到 `MachineExecutionContext`，与 Tasks/Compute 并列并暴露只读访问。

## 2. 查询接口

- [x] 2.1 在 `IAlgorithmCommandSink` 新增 `TryReadCargo(string field, string itemType, out AlgorithmValue value)` 与 `TryQueryTask(string name, out AlgorithmValue task)`，默认返回 false。
- [x] 2.2 `AlgorithmMachineAdapter` 实现两个查询接口（读本机货舱 / 查 `MachineTaskQueue` 未结束任务）。

## 3. 货舱读取节点

- [x] 3.1 `AlgorithmNodeKind` 新增 `Cargo`；`AlgorithmCatalog` 输出 `capacity/remaining/amount/has_item/changed` 端口，成本 1。
- [x] 3.2 `AlgorithmEvaluation.Value()` 处理 `Cargo` 节点，经查询接口求值（无实现时返回无效值）。

## 4. QueryTask 数据返回

- [x] 4.1 `AlgorithmEvaluation.Value()` 处理 `QueryTask` 节点，经 `TryQueryTask` 返回 `found`/`task`；任务名取节点 `Field`。
- [x] 4.2 移除 `AlgorithmMachineAdapter.Submit` 对 QueryTask 的 rejected 桩，改为不产生命令（纯数据节点）。

## 5. 固定运输模板与收口

- [x] 5.1 录入固定路线运输模板（DEC-123）：来源缓存监测 + 货舱读取 + QueryTask 防重 + Transfer 装卸 + Navigate 移动 + Delay 重试，成本按实际节点核定（35，见 proposal 偏差说明）。
- [x] 5.2 补 EditMode 测试：货舱契约、Cargo 节点求值、QueryTask found/task 返回、五模板 Seed 全绿；全量算法回归。

## 6. 收尾

- [x] 6.1 `openspec validate b19-cargo-and-task-query` 通过。
- [x] 6.2 更新 b17 tasks.md 五模板录入完成状态（如适用）。
