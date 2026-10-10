# 传感器与效应器生命周期

原始任务映射：P2-005、P2-007、P3-006。本文件为增量合同；正式规则引用design.md，不覆盖未完成的原任务范围。

## ADDED Requirements

### Requirement: Installed components own bindings
运行宿主 SHALL 为真实安装组件幂等建立传感器与效应器绑定，并明确其释放责任。

#### Scenario: Repeated reconciliation
- **WHEN** 相同机器及硬件修订被重复对账
- **THEN** 每个组件仍只有一份传感订阅或行为队列。

#### Scenario: Hardware removed
- **WHEN** 已绑定效应器被合法拆卸
- **THEN** 在安全边界停止其行为、释放预留及算力，其他效应器继续按规则运行。

### Requirement: Readouts preserve authority
传感提供者 SHALL 只返回绑定目标允许读取的真实领域状态，缺能力不得伪造有效数据。

#### Scenario: Provider unavailable
- **WHEN** 目标存在但生产域尚无对应字段提供者
- **THEN** 读取返回带原因的不可用，算法不把默认零值解释为真实状态。

#### Scenario: Old generation arrives
- **WHEN** 目标重绑定后旧代次采样回调到达
- **THEN** 回调被丢弃，不能触发新绑定的算法或行为。

### Requirement: Effector outcomes are real
效应器端点 SHALL 将算法意图提交到真实行为权威并回传已提交、等待、拒绝、完成、取消或失败结果。

#### Scenario: Two effectors receive work
- **WHEN** 两个不同效应器同时收到合法命令
- **THEN** 按现有规则并行；对同一效应器的两个命令严格排队，不重复结算。

#### Scenario: Unknown action
- **WHEN** 尚未接入的行为类型被提交
- **THEN** 返回明确拒绝/不可用原因，不播放一次动画即回报完成。

### Requirement: Presentation follows domain
动作表现 SHALL 消费领域行为状态，不能成为资源数量或行为提交的权威。

#### Scenario: Visual instance absent
- **WHEN** 合法行为执行时其表现对象未加载
- **THEN** 领域按合同处理，明确表现缺失；不得凭渲染状态增加或减少资源。


