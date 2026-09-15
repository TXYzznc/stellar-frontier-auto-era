# 规范增量

## ADDED Requirements

### Requirement: 任务结果必须经过制作人验收闭环
窗口提交结果后 MUST 进入 `ResultReady`/`AwaitingProducerAcceptance`，结果包必须可定位并包含验证证据；制作人确认后才可进入 `Accepted`，证据缺失或失效时 MUST 进入 `Rework`。

#### Scenario: 结果已产出但制作人尚未查看
- **WHEN** 窗口提交完整结果包
- **THEN** 任务保持 AwaitingProducerAcceptance，调度窗口重试通知，不能领取下一项替代验收

#### Scenario: 证据路径失效
- **WHEN** 制作人验收发现产物或证据无法读取
- **THEN** 任务进入 Rework，记录缺陷和回退窗口，不得标记 Accepted

### Requirement: 阻塞不得冻结无关安全步骤
任务 MUST 仅冻结受影响子步骤；只有剩余全部步骤都不可执行时才整体 Blocked，并记录恢复条件。

#### Scenario: 单一外部端点阻塞
- **WHEN** 一个子步骤依赖不可用端点且任务还有独立步骤
- **THEN** 受影响步骤 WaitingDependency，其它安全步骤继续执行

#### Scenario: 所有剩余步骤均不可执行
- **WHEN** 依赖、授权或资源冲突阻止全部剩余步骤
- **THEN** 任务进入 Blocked，调度窗口记录重试时间和升级对象
