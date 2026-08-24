# 第 16 段：同刻事件稳定排序键

## 实施范围

- 增加 `WorldEventSortKey`，按世界毫秒、固定事件阶段、永久 ID、事件序号依次比较。
- 固定阶段使用已确认的第一版因果顺序：能源、世界状态、传感器、算法、任务与行为、资源与奖励。
- 键拒绝负世界时刻与无效永久 ID；本段只提供确定性值契约，不实现事件调度器。

## 验证

- Unity 普通刷新/编译完成，编辑器未进入 Play Mode，`isCompiling=false`。
- `AutoEra.Tests.Editor.WorldEventSortKeyEditModeTests`：2/2 通过，覆盖不同插入顺序得到一致排序及非法键拒绝。
- 本段未创建或修改任何 `.xlsx`。
