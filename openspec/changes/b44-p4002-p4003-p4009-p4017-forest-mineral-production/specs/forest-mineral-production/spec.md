# 林木与矿脉真实作业

原始任务映射：P4-002、P4-003、P4-009、P4-017。本文件为增量合同；正式规则引用design.md，不覆盖未完成的原任务范围。

## ADDED Requirements

### Requirement: Forest state is per tree
人工林 SHALL 以稳定树ID保存生长与采集状态，并由真实行为提交决定产出。

#### Scenario: Resume cutting
- **WHEN** 机器在切割中断电后恢复
- **THEN** 继续剩余工作，已提交木材不再生成，目标仍是原树。

#### Scenario: Target removed
- **WHEN** 等待中的树目标永久失效
- **THEN** 行为报告目标失效，释放预留，不按名称自动绑定另一树。

### Requirement: Mineral stock is independent from visuals
矿脉 SHALL 独立保存权威储量并按DEC-203派生可见矿石数。

#### Scenario: Visual count changes
- **WHEN** 剩余储量跨越显示映射阈值
- **THEN** 只改变确定性选取的显示矿石，数量结算不依赖显示对象数。

#### Scenario: Deposit cleanup
- **WHEN** 耗尽矿脉到达规定换日清场点
- **THEN** 释放其建造占用，保留遗留货物、交付责任和无关设施。

### Requirement: Production commits at safe units
切割与钻探 SHALL 按现行数值/贡献/单位边界结算，取消与供电变化不能重复提交。

#### Scenario: Power changes mid unit
- **WHEN** 功率或可运行状态在一个作业阶段中变化
- **THEN** 按既有贡献与安全点规则处理，累计消耗及产出守恒。

#### Scenario: Ground display cap
- **WHEN** 矿脉产物达到地面可见堆上限
- **THEN** 遵循矿脉不因显示封顶停产的规则，权威货物仍有唯一归属。

### Requirement: Production observations reflect facts
资源点 SHALL 向传感器与UI提供真实、授权的生产状态。

#### Scenario: Production updates sensor
- **WHEN** 一次合法产出或耗尽提交发生
- **THEN** 相应状态在后续采样可见并可触发算法，日志能追溯任务和行为。
