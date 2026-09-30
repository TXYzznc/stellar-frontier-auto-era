# region-work-queue

## ADDED Requirements

### Requirement: 等待者优先级升级

RegionWorkQueue MUST 允许等待中的机器以更高优先级重新申请并在队列内提前；同级或更低优先级的重复申请 MUST 保持既有 FIFO 次序，不得改变位置。

#### Scenario: 严格更高优先级提位

- **WHEN** 等待中的机器以严格高于其当前记录的优先级再次申请
- **THEN** 该机器按新优先级重排到等待队列的对应位置（同优先级内 FIFO 稳定）

#### Scenario: 同级或降级申请保持原位

- **WHEN** 等待中的机器以等于或低于其当前记录的优先级再次申请
- **THEN** 等待次序不变，返回 Waiting

### Requirement: 桥接层工作状态跟随权威队列

MotionWorkBridge 的 `WorkState` MUST 订阅 `queue.Changed` 并据 `GetRequestState` 同步；原持有者释放后本机被唤醒时，无需再次申请即可读到 Granted。

#### Scenario: 被唤醒后状态升级

- **WHEN** 桥接机器处于 Waiting 且队列把其提升为持有者
- **THEN** 桥接层 `WorkState` 经队列 Changed 事件自动变为 Granted

#### Scenario: 释放后状态归零

- **WHEN** 桥接层释放其占用
- **THEN** `WorkState` 变为 None，且退订队列事件（Dispose 后不再回调）
