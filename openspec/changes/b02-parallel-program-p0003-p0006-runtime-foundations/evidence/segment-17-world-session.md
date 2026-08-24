# 第 17 段：世界会话与工厂

## 实施范围

- 增加纯 C# `AutoEraWorldSession` 与 `AutoEraWorldSessionFactory`，每个会话独占永久 ID 分配器、对象注册表和世界时钟。
- 会话没有静态状态或场景依赖；释放幂等，并清空当前世界注册表中的全部对象引用。
- 工厂每次创建新的独立对象图，不复用另一会话的注册表、时钟或 ID 高水位。

## 验证

- Unity 普通刷新/编译完成，编辑器未进入 Play Mode，`isCompiling=false`。
- `AutoEra.Tests.Editor.AutoEraWorldSessionEditModeTests`：2/2 通过，覆盖释放清理、幂等释放与跨会话状态隔离。
- 本段未创建或修改任何 `.xlsx`。

## 后续边界

- 应用上下文、受控 Procedure 上下文槽和场景协调器仍未接入；本段不访问全局运行时状态。
