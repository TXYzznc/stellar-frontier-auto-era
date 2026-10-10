# 运输与生产观察

原始任务映射：P4-011、P4-014、P4-015、P4-016。本文件为增量合同；正式规则引用design.md，不覆盖未完成的原任务范围。

## ADDED Requirements

### Requirement: Transport uses authoritative steps
固定路线运输 SHALL 通过真实导航、作业点和原子装卸形成任务步骤，且防止相同责任重复提交。

#### Scenario: Repeat trigger
- **WHEN** 同一路线已有进行中任务或待交付货物时算法再次触发
- **THEN** 不创建重复责任，按既有规则继续交付或返回已存在状态。

#### Scenario: Arrive without work slot
- **WHEN** 机器到达目标附近但未获得合法作业点
- **THEN** 不提前扣来源或增加货舱，等待/失败原因可观察。

### Requirement: Failures preserve cargo responsibility
运输失败或取消 SHALL 保留已装货物及未解决交付责任。

#### Scenario: Path becomes blocked
- **WHEN** 机器装载后目的地不可达
- **THEN** 物品仍在货舱，任务报告失败或等待，诊断显示目的地与原因。

#### Scenario: Capacity recovers
- **WHEN** 目的仓库曾满载后重新有容量
- **THEN** 按已有重试合同继续交付，实际入库只记一次。

### Requirement: Production UI observes real settlement
仓库与本批现场页 SHALL 展示真实状态和结算记录，所有写操作经过领域命令。

#### Scenario: Two resource loops
- **WHEN** 林木和矿脉分别完成自主作业、装载、移动与交付
- **THEN** 仓库/余额与现场变化一致，每次入库可追溯到任务，不通过UI直接改余额。

#### Scenario: UI closed
- **WHEN** 运输运行期间关闭仓库及详情页
- **THEN** 任务继续，订阅释放，再打开看到真实最新状态。

### Requirement: Partial delivery cannot certify full milestone
本批 SHALL 区分两类资源纵切证据与完整G4阶段验收。

#### Scenario: Missing other production domains
- **WHEN** 两类资源循环通过但水泵、农业或P4-020尚未完成
- **THEN** 记录本批子集通过，保留G4完整前置未满足状态。


