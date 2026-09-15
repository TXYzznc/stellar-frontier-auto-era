# 规范增量

## ADDED Requirements

### Requirement: Active 任务必须持有可续租的执行租约
Active 任务 MUST 记录 leaseId、lastActivity、lastHeartbeat 和 lastEvidence。租约超时后 MUST 进入 SuspectedStopped 并触发恢复或制作人升级。

#### Scenario: 窗口回合结束但任务未完成
- **WHEN** 任务仍为 Active 且心跳超过阈值
- **THEN** watchdog 标记 SuspectedStopped 并重新唤醒原窗口，不将其视为空闲完成

### Requirement: 事件通知必须可重试并确认
事件 MUST 记录投递尝试和确认状态；未确认事件 MUST 按退避策略重试，目标窗口 Active 时不得抢占。

#### Scenario: 空闲窗口存在未确认事件
- **WHEN** watchdog 发现目标无 Active 且事件有效
- **THEN** 发送最小唤醒信号并保留事件直到 ack

### Requirement: 结果必须经过制作人验收
结果 MUST 进入 AwaitingProducerAcceptance；只有制作人确认才进入 Accepted，证据无效时进入 Rework。

#### Scenario: 结果证据失效
- **WHEN** 制作人检查发现证据路径不可读
- **THEN** 任务进入 Rework 并通知原窗口修复
