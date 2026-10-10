# 离线领域推进与回归报告

原始任务映射：P7-008、P7-009、P7-010、P7-012。本文件为增量合同；正式规则引用design.md，不覆盖未完成的原任务范围。

## ADDED Requirements

### Requirement: Offline domains honor constraints
离线领域推进 SHALL 遵守与在线相同的资源、容量、供电、路径和算法限制。

#### Scenario: Storage becomes full
- **WHEN** 离线运输使仓库达到容量上限
- **THEN** 后续依赖容量的动作等待，不穿透容量增加收益。

#### Scenario: Fuel runs out
- **WHEN** 离线途中燃料耗尽且储能不足
- **THEN** 在准确事件边界停机，日照恢复后仅恢复条件成立的行为。

### Requirement: Navigation is resumable without frame simulation
离线导航 SHALL 使用有效路径与速度推算抵达，并能恢复中途位置及责任。

#### Scenario: Catchup stops mid journey
- **WHEN** 结算目标时刻落在移动途中
- **THEN** 快照保留路径进度，进入世界的位置与剩余行程一致。

#### Scenario: No valid path
- **WHEN** 离线任务提交时无法找到有效路径
- **THEN** 返回不可达结果并保留货物责任，不直接瞬移到目的地。

### Requirement: Manufacturing and rewards preserve player choices
离线施工制造与任务结算 SHALL 延续既有操作，不产生新的玩家选择或重复奖励。

#### Scenario: Recipe not selected
- **WHEN** 工坊空闲且玩家未选择新配方
- **THEN** 离线不自动选择配方或消费材料。

#### Scenario: Reward requires claiming
- **WHEN** 离线达到一个需要玩家领取的奖励条件
- **THEN** 保留待领取状态，不因重新读报告自动发放。

### Requirement: Reports are persistent read models
结算报告 SHALL 绑定唯一结算身份并从已提交事实聚合，重看不得再次结算。

#### Scenario: Reopen report
- **WHEN** 同一离线报告从中枢历史重复打开
- **THEN** 资源、经验、奖励与通知提交计数保持不变。

#### Scenario: Crash near completion
- **WHEN** 完成领域推进后、报告确认前中断并重启
- **THEN** 恢复相同runId及报告状态，不重复应用收益。

### Requirement: World entry waits for settlement
离线未完成时 SHALL 禁止世界操作，同时提供真实进度与可恢复状态。

#### Scenario: Budget spread over frames
- **WHEN** 长离线需要多个帧处理
- **THEN** 界面持续响应且进度真实，不能进入未结算世界；完成后仅开放一次。


