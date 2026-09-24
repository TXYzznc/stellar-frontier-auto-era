# 机器作为电网负载：逐部件求和的耗电与供电回写

## ADDED Requirements

### Requirement: 机器耗电按部件逐项求和

机器的当前耗电 SHALL 由载体与每个已装组件的当前功率**逐项相加**得出；
SHALL NOT 用一个整机倍率替代。

#### Scenario: 休眠的机器

- **WHEN** 机器处于休眠
- **THEN** 载体、计算核心、传感器与效应器 SHALL 全部按各自待机功率计算
- **AND** 结果 SHALL 等于 IdlePowerDraw

#### Scenario: 核心运行算法而效应器待机

- **WHEN** 计算核心正在运行算法、效应器没有执行动作
- **THEN** 核心 SHALL 按稳定功率、效应器 SHALL 按待机功率计入
- **AND** 传感器在已启用且未休眠时 SHALL 按稳定功率计入（持续采样）

#### Scenario: 效应器执行动作

- **WHEN** 某个效应器开始执行动作
- **THEN** 只有该效应器 SHALL 从待机功率切换为稳定功率
- **AND** 其他部件的取值 SHALL 不受影响

#### Scenario: 混合状态

- **WHEN** 部分部件在工作、部分待机
- **THEN** 总耗电 SHALL 既不是「全部待机」也不是「全部工作」

#### Scenario: 移动的机器

- **WHEN** 机器正在移动
- **THEN** 载体 SHALL 按稳定功率计入

#### Scenario: 已损坏的机器

- **WHEN** 机器结构完整度为 0
- **THEN** 请求功率 SHALL 为 0

### Requirement: 请求功率不依赖当前供电状态

机器请求的功率 SHALL 只取决于它的部件状态与完好度；
SHALL NOT 因为当前没有供电而变成 0。

#### Scenario: 断电的机器仍然请求功率

- **WHEN** 一台机器当前没有供电但仍在运行状态
- **THEN** 它请求的功率 SHALL 大于 0
- **AND** 它 SHALL NOT 因为「没电所以请求 0」而永远无法恢复供电

#### Scenario: 断电机器的实际消耗

- **WHEN** 电网没有给这台机器供电
- **THEN** 它的实际耗电 SHALL 由电网置为 0

### Requirement: 不请求用电的机器不参与停机判定

未部署（库中）机器与现场断电的机器 SHALL NOT 请求用电，
SHALL NOT 被记为因缺电停机。

#### Scenario: 库中的机器

- **WHEN** 机器尚未部署
- **THEN** 它 SHALL NOT 请求用电

#### Scenario: 现场断电

- **WHEN** 玩家在现场关闭机器电源
- **THEN** 它 SHALL NOT 请求用电
- **AND** SHALL NOT 因为它的请求而让电网去停别的设备

### Requirement: 供电回写只改供电

把电网结论写回机器时 SHALL 只更新供电事实，
SHALL NOT 覆盖区域信号；结论没有变化时 SHALL NOT 广播变更。

#### Scenario: 区域信号独立

- **WHEN** 机器有区域信号但没有供电，电网随后给它供电
- **THEN** 供电 SHALL 变为可用
- **AND** 区域信号 SHALL 保持原值

#### Scenario: 结论未变

- **WHEN** 电网结论与机器当前供电状态一致
- **THEN** 回写 SHALL 不产生变更通知

### Requirement: 缺电停机落到机器上

电网因缺电停机某个用电对象时，该机器 SHALL 变为未供电。

#### Scenario: 缺电停机

- **WHEN** 供电不足且该机器被选中停机
- **THEN** 机器的供电 SHALL 变为不可用
- **AND** 它 SHALL 被记为因缺电停机（而不是玩家关闭）

### Requirement: 机器功率来自数据表

机器与组件的待机／稳定功率 SHALL 来自数据表配置，
SHALL NOT 在代码里假定倍率（例如「待机是稳定功率的 10%」）。

#### Scenario: 未装组件的机器

- **WHEN** 机器上没有任何组件
- **THEN** 它的功率 SHALL 等于载体自身配置的待机功率

#### Scenario: 停用的组件

- **WHEN** 某个传感器被停用
- **THEN** 它 SHALL 按待机功率计入，而不是稳定功率
