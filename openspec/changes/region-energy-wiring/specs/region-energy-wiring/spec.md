# 区域电网：谁拥有电网、谁参与、结论怎么落回机器

## ADDED Requirements

### Requirement: 电网由区域持有

每个区域 SHALL 拥有自己的电网；电网 SHALL NOT 是全局单例，
一个区域的富余电力 SHALL NOT 自动补充另一个区域。

#### Scenario: 每个区域一套结算

- **WHEN** 两个区域各自建立电网
- **THEN** 它们 SHALL 各自结算自己的发电、耗电与储能
- **AND** 一个区域的余电 SHALL NOT 出现在另一个区域

### Requirement: 没有设施的区域不建电网

区域 SHALL 只在场景声明了能源设施时才建立电网；
没有设施时 SHALL NOT 改变任何机器的供电状态。

#### Scenario: 场景里没有设施

- **WHEN** 一个区域里没有任何能源设施
- **THEN** 该区域 SHALL 报告没有供电能力
- **AND** 结算 SHALL NOT 改变机器的供电状态
- **AND** 应用结论 SHALL 返回 0 个变化

### Requirement: 参与方是场景设施与已部署机器

电网的参与方 SHALL 是「场景声明过的能源设施」与「**已部署**的机器」；
未部署（库中）机器 SHALL NOT 占用区域功率。

#### Scenario: 库中的机器

- **WHEN** 机器尚未部署
- **THEN** 它 SHALL NOT 被接进电网

#### Scenario: 部署与回收

- **WHEN** 机器被部署
- **THEN** 对账 SHALL 把它接进电网
- **WHEN** 机器被回收或不在花名册里
- **THEN** 对账 SHALL 把它摘掉
- **AND** 其余参与方的进入队列顺序 SHALL 保持不变

### Requirement: 设施声明挂在实体预制体上

能源设施的声明 SHALL 随**运行时真正存在的对象**（实体预制体）走，
SHALL NOT 只挂在建造期模板（场景里的种子对象）上。

#### Scenario: 区域初始化后设施可用

- **WHEN** 区域初始化完成
- **THEN** 场景预制体上声明的设施 SHALL 已被区域登记
- **AND** 它们的额定功率与燃料 SHALL 与设计数值一致

#### Scenario: 实体重新显示

- **WHEN** 同一个设施被再次初始化
- **THEN** 已消耗的燃料 SHALL NOT 被重置

### Requirement: 推进顺序固定为先对账再结算再落结论

每个世界节拍 SHALL 先按花名册对账、再结算、最后把结论落回机器。

#### Scenario: 本帧新部署的机器

- **WHEN** 一台机器在本帧被部署
- **THEN** 它 SHALL 在同一节拍内进入电网
- **AND** SHALL 不必白等一个节拍才通电

#### Scenario: 结论未变

- **WHEN** 结算结论与机器当前供电状态一致
- **THEN** SHALL NOT 产生变更通知

### Requirement: 日照驱动环境能源出力

环境能源（太阳能）的可用出力 SHALL 按世界时间所处的日照状态决定；
入夜后 SHALL 归零，缺口 SHALL 由燃料发电或储能补足。

#### Scenario: 夜间

- **WHEN** 世界时间进入无日照阶段
- **THEN** 环境能源出力 SHALL 为 0
- **AND** 已部署机器 SHALL 在燃料发电支持下保持供电

#### Scenario: 燃料消耗

- **WHEN** 燃料发电站开着但没有需求
- **THEN** 它 SHALL NOT 消耗燃料
- **WHEN** 有真实负载
- **THEN** 燃料消耗 SHALL 与**实际输出**对应

### Requirement: 默认供电优先级唯一且明确

系统 SHALL 为机器／建筑提供默认供电优先级，且该默认值 SHALL 只有一处定义。

#### Scenario: 默认档位

- **WHEN** 机器被接进电网而未显式指定优先级
- **THEN** 它 SHALL 使用「普通生产」档
