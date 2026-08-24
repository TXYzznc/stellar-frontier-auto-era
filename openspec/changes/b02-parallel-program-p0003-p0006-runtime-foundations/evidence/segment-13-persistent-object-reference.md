# 第 13 段：持久对象引用

## 实施范围

- 增加不可变、可序列化的 `PersistentObjectReference`，仅保存 `PersistentId` 与预期 `PersistentObjectKind`。
- 引用解析只委托当前世界的注册表；缺失目标返回明确结果并保留原 ID，不持有显示名称、Prefab 或对象实例用于猜测重绑。

## 验证

- Unity 普通刷新/编译完成，编辑器未进入 Play Mode，`isCompiling=false`。
- `AutoEra.Tests.Editor.PersistentObjectReferenceEditModeTests`：3/3 通过，覆盖目标缺失、同 ID 重载、同名异 ID 不重绑和跨世界隔离。
- 本段未创建或修改任何 `.xlsx`。

## 后续边界

- 引用与具体世界会话释放的装配将在应用上下文/会话生命周期分段中完成。
