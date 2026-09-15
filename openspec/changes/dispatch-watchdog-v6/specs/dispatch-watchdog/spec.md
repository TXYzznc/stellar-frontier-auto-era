# 派发监控闭环

## ADDED Requirements

### Requirement: 短周期停工检测
调度 MUST 在不超过 3 分钟无心跳时检测 Active 任务停工。

#### Scenario: Active 无心跳
- **WHEN** 队列有 Active 且最近心跳超过 3 分钟
- **THEN** watch 输出 `WakeRegisteredWindow` 与注册表 threadId

### Requirement: 状态一致性
调度 MUST 检测 Active、租约、心跳和窗口注册状态不一致。

#### Scenario: 任务与心跳不匹配
- **WHEN** 心跳 taskId 与队列 Active taskId 不同或租约过期
- **THEN** 调度唤醒注册窗口并要求其在安全点修正状态

### Requirement: 实际运行状态门禁
调度 MUST 同时核对 Codex 窗口实际状态；队列存在 Active 且注册窗口为 idle/notLoaded 时，即使 heartbeat 未过期，也 MUST 唤醒注册窗口并要求恢复 inProgress。

#### Scenario: Active 但窗口空闲
- **WHEN** 队列 Active、最近 heartbeat 尚未超时，但注册窗口实际为 idle
- **THEN** 调度输出 `WakeRegisteredWindow` 并唤醒当前注册窗口，不能以 heartbeat 作为健康替代

### Requirement: 自动唤醒与升级
调度 MUST 对 `WakeRegisteredWindow` 实际发送最小唤醒消息；唤醒失败或持续无心跳后 MUST 升级制作人。

#### Scenario: 窗口持续无响应
- **WHEN** 正确窗口被唤醒后仍无法恢复
- **THEN** 调度发送含证据的制作人升级事件

### Requirement: 阻塞任务路由
无 Active 但存在 blocked suspended 时，调度 MUST 输出并执行 `RouteBlockedTask`，推动窗口自行恢复或申请专业协作。

#### Scenario: 阻塞任务无人推进
- **WHEN** 角色没有 Active 且仍有 blocked suspended
- **THEN** 调度唤醒注册窗口并路由可用协作，协作失败后才升级制作人

### Requirement: 完成门槛
窗口 MUST NOT 在验收证据不完整或存在可自行修复的失败时登记 `result-ready` 或通知制作人；应保持 Active/Rework 并继续自助恢复或申请协作。

#### Scenario: 截图空白
- **WHEN** 视觉截图落盘但 UI 未显示、引用未呈现或工具调用超时且仍有排查路径
- **THEN** 窗口继续返工，不进入 AwaitingProducerAcceptance
