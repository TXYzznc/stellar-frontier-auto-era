## ADDED Requirements

### Requirement: 应用确认与取消命令

算法读模型 SHALL 提供 `ConfirmWarnings` 与 `CancelApply` 命令，转发到实例服务。

#### Scenario: 确认警告

- **WHEN** 实例存在且请求处于 `AwaitingWarningConfirmation`，调用 `ConfirmWarnings`
- **THEN** 请求推进到 `WaitingSafePoint`，返回 true

#### Scenario: 取消待处理请求

- **WHEN** 实例存在且请求处于 `WaitingSafePoint`/`AwaitingWarningConfirmation`，调用 `CancelApply`
- **THEN** 请求进入 `Cancelled`，返回 true

#### Scenario: 库页或不可用域

- **WHEN** 调用来自库页/不可用域读模型，或无待处理请求
- **THEN** 返回 false

### Requirement: 工作台应用按钮动态行为

算法工作台「应用草稿」按钮 SHALL 按请求状态切换行为。

#### Scenario: 有待确认警告

- **WHEN** 请求处于 `AwaitingWarningConfirmation` 且点击应用按钮
- **THEN** 调用 `ConfirmWarnings`

#### Scenario: 等待安全点

- **WHEN** 请求处于 `WaitingSafePoint`/`Applying`
- **THEN** 应用按钮禁用
