# 跨域快照与离线调度

原始任务映射：P7-005、P7-006、P7-007。本文件为增量合同；正式规则引用design.md，不覆盖未完成的原任务范围。

## ADDED Requirements

### Requirement: Cross domain snapshot is coherent
跨域保存 SHALL 捕获同一业务修订的生产、经济、能源、任务与成长状态。

#### Scenario: Transaction pending
- **WHEN** 一次购买或奖励提交与保存请求同刻发生
- **THEN** 快照包含完整提交或完整未提交状态，不出现扣款未交付或奖励重复。

#### Scenario: Required domain absent
- **WHEN** 当前世界缺少必需经济/成长快照适配器
- **THEN** 完整保存/恢复验收被阻止并指出缺失域，不以空段替代真实进度。

### Requirement: Offline order is deterministic
离线调度 SHALL 以显式同刻领域顺序、永久ID和序号稳定处理有效事件。

#### Scenario: Same initial state different budgets
- **WHEN** 同一快照和离线区间分别用不同帧预算、注册顺序推进
- **THEN** 最终领域状态、事件责任及提交标记一致。

#### Scenario: Obsolete event
- **WHEN** 事件的目标代次已被取消或替换
- **THEN** 丢弃旧事件并记录必要诊断，不作用于新对象。

### Requirement: Checkpoints resume without duplication
可恢复检查点 SHALL 只发布在完整安全批次后并保留队列与结算身份。

#### Scenario: Interrupted catchup
- **WHEN** 离线结算中断后加载最后有效检查点
- **THEN** 从已完成边界继续，不重复处理已提交事务。

#### Scenario: Frame budget reached
- **WHEN** 某帧达到事件处理预算
- **THEN** 向界面提供真实进度并让出执行，世界操作仍关闭。

### Requirement: Offline scheduling does not invent player actions
调度器 SHALL 保留时间倒退与奖励防重规则，不替玩家进行购买、升级或普通奖励选择。

#### Scenario: Local date rolls back
- **WHEN** 重新载入时本地日期早于已见最晚日期
- **THEN** 不恢复已领取补给资格。

#### Scenario: No progress algorithm
- **WHEN** 某算法不断生成无进展事件
- **THEN** 按批准的保护规则单独停止并报警，其他领域可继续结算。


