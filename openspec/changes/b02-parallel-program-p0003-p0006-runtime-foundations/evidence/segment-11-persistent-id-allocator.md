# 第 11 段：永久实例 ID 与高水位分配

## 实施范围

- 在 `AutoEra.World.Identity` 中建立不可变的 `PersistentId` 值类型；其底层为 `ulong`，`0` 为无效值。
- 建立每个世界会话独占的 `PersistentIdAllocator`：所有对象类别共享同一单调递增序列，已分配或恢复的值不会复用。
- 恢复乱序 ID 时分配器只向前推进高水位；遇到 `ulong.MaxValue` 后进入耗尽状态，后续分配明确失败而不回绕。

## 验证

- Unity 普通刷新/编译完成，编辑器未进入 Play Mode，`isCompiling=false`。
- `AutoEra.Tests.Editor.PersistentIdEditModeTests`：4/4 通过，覆盖值语义、连续分配、乱序恢复、高水位推进、无效 ID 和溢出耗尽。
- 本段未创建或修改任何 `.xlsx`。

## 后续边界

- 对象类别验证、注册/注销、持久引用解析与世界会话释放属于后续注册表和生命周期分段。
