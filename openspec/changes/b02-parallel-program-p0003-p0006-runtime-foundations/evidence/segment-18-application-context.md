# 第 18 段：应用上下文与活动世界会话所有权

## 实施范围

- 增加纯 C# `AutoEraApplicationContext`，以构造函数注入 `IUtcTimeProvider` 和 `AutoEraWorldSessionFactory`。
- 上下文仅持有一个活动世界会话；重复创建被拒绝，释放和应用退出时均幂等地释放当前会话。
- 不提供全局访问器或 Service Locator；Procedure 上下文与场景协调器留在后续集成分段。

## 验证

- 测试窗口在 Unity 2022.3.62f3c1 运行 `AutoEra.Tests.Editor.AutoEraApplicationContextEditModeTests` / EditMode：1/1 通过，失败 0、跳过 0、不确定 0，耗时 7 秒。
- 完成后 Unity 非 PlayMode、未暂停、未编译、未更新、无域重载待处理。
- 本段未创建或修改任何 `.xlsx`。

## 后续边界

- 受控 Procedure FSM 数据槽、场景协调器和产品 Procedure 将在后续独立分段接入。
