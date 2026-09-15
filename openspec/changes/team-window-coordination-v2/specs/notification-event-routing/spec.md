# 通知事件路由增量

## MODIFIED Requirements

### Requirement: 协作状态必须可可靠投递并可确认
系统 MUST 将跨窗口协作事件追加写入 `.ai/dispatch/events.local.jsonl`，每条事件包含唯一 `eventId`、目标角色、事件类型、优先级、创建时间、有效期和确认状态。事件 MUST 可重试且不得因窗口回合结束而丢失。

#### Scenario: 目标窗口正在执行 Active 任务
- **WHEN** 依赖就绪或资源释放事件产生
- **THEN** 事件先写入共享事件队列，目标任务保持 Active，不发送会抢占任务的普通消息

#### Scenario: 目标窗口空闲
- **WHEN** 调度窗口发现存在未确认的有效事件且目标窗口没有 Active 任务
- **THEN** 调度窗口发送最小唤醒信号，目标窗口读取事件队列并确认事件

### Requirement: 调度窗口必须维护心跳并升级真实异常
Active 任务 MUST 维护 `lastActivity`、`lastHeartbeat` 和 `lastEvidence`。调度窗口 MUST 根据超时规则标记 `SuspectedStopped`，并在需要制作人决策、授权、资源冲突或安全风险时升级；普通事件不得打断 Active 任务。

#### Scenario: Active 任务心跳超时
- **WHEN** `lastHeartbeat` 超过配置阈值且没有新的证据
- **THEN** 调度窗口标记 `SuspectedStopped`，记录恢复尝试和事件，不静默将任务视为空闲

#### Scenario: 事件已被目标窗口确认
- **WHEN** 目标窗口读取并处理事件
- **THEN** 目标窗口写入 `acknowledgedAt`，重复唤醒不得再次执行同一事件
