## ADDED Requirements

### Requirement: 草稿编译激活命令

算法实例服务 SHALL 支持把编译好的运行时挂接到草稿实例（`Runtime == null`）。

#### Scenario: 草稿实例挂接运行时

- **WHEN** 以有效实例 Id、Id 一致、`IsSafe` 且算力容量充足的运行时调用 `CompileDraft`
- **THEN** 该实例的运行时从 null 变为该运行时，并触发 `Changed`

#### Scenario: 非草稿或校验不通过

- **WHEN** 实例不存在、已持有运行时、运行时 Id 不一致、运行时非 `IsSafe` 或算力容量不足
- **THEN** 返回 false 且实例状态不变

### Requirement: 草稿激活读模型命令

算法读模型 SHALL 提供 `ActivateDraft`，把「绑定完整且校验通过」的草稿编译为运行时。

#### Scenario: 校验通过激活

- **WHEN** 选中草稿实例、`TryCompile(draft, 真实算力容量, …, template:false)` 通过
- **THEN** 构造 `AlgorithmRuntime`（adapter 作 sink）、`Adapter.Attach`、`CompileDraft` 返回 true，实例从「草稿」进入「可应用」

#### Scenario: 校验失败或域不支持

- **WHEN** 草稿含错误（如 `RequiredBinding`）、超算力容量，或调用来自库页/不可用域
- **THEN** 返回 false 且实例仍是草稿
