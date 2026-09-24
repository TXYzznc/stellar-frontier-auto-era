## ADDED Requirements

### Requirement: 机器货舱统一容量单位契约

机器 SHALL 拥有一个本机货舱，以统一容量单位计量容量、已用与剩余空间；第一版不区分体积、重量或特殊容器。

#### Scenario: 剩余空间计算
- **WHEN** 货舱容量为 20、已用为 6
- **THEN** 剩余空间 SHALL 为 14

#### Scenario: 超容拒绝
- **WHEN** 尝试使已用超过容量
- **THEN** 系统 SHALL 拒绝该变更并保持原状态

### Requirement: 机器货舱物品明细

货舱 SHALL 按物品类型维护整数数量明细，用于判断是否已有目标货物。

#### Scenario: 已有目标物品查询
- **WHEN** 货舱已有类型为「银穗麦」的物品且数量大于 0
- **THEN** 按类型查询 SHALL 返回真

#### Scenario: 空舱查询
- **WHEN** 货舱为空
- **THEN** 任意类型查询 SHALL 返回假

### Requirement: 机器货舱变化事件

货舱容量或物品明细变化时 SHALL 发出变化通知，供算法监听。

#### Scenario: 装载触发变化
- **WHEN** 货舱物品数量发生变化
- **THEN** 变化事件 SHALL 触发一次
