# 第 14 段：可替换 UTC 来源与无状态换算

## 实施范围

- 定义 `IUtcTimeProvider`，并提供以 `DateTimeOffset.UtcNow` 为唯一来源的 `SystemUtcTimeProvider`。
- 增加无状态 `TimeUtil`：依赖调用方传入的 Provider 计算离线时长，所有输入先归一到 UTC；时间倒退时返回零。
- 提供 Unix 毫秒转换帮助函数，不保存当前 Provider 或世界状态。

## 验证

- Unity 普通刷新/编译完成，编辑器未进入 Play Mode，`isCompiling=false`。
- `AutoEra.Tests.Editor.UtcTimeEditModeTests`：3/3 通过，覆盖固定 Provider、时区偏移无关和系统时间倒退归零。
- 本段未创建或修改任何 `.xlsx`。

## 后续边界

- Provider 的应用上下文注入与世界会话装配留给生命周期分段；本段不实现服务器 UTC、防作弊或离线事件结算。
