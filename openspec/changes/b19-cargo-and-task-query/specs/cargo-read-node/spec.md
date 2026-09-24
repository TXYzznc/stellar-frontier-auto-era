## ADDED Requirements

### Requirement: 货舱读取节点端口

算法图 SHALL 提供货舱读取数据节点，输出容量、剩余空间、目标物品判断数据端口与变化事件端口，且不要求传感器绑定。

#### Scenario: 端口清单
- **WHEN** 查询货舱读取节点的输出端口
- **THEN** 端口 SHALL 包含 `capacity`、`remaining`、`amount`、`has_item` 与 `changed`

#### Scenario: 无绑定要求
- **WHEN** 货舱读取节点应用在非模板图且未声明绑定
- **THEN** 校验 SHALL 不报 RequiredBinding

### Requirement: 货舱字段求值

货舱读取节点 SHALL 在本机货舱查询接口上求值，返回当前容量、剩余空间与目标物品判断；目标物品类型由节点 Field 指定。

#### Scenario: 读取容量与剩余
- **WHEN** 本机货舱容量为 20、已用为 6
- **THEN** 节点 `capacity` 求值为 20、`remaining` 求值为 14

#### Scenario: 目标物品判断
- **WHEN** 节点 Field 指定物品类型且本机货舱已有该类型物品
- **THEN** `has_item` 求值为真

#### Scenario: 无货舱查询实现
- **WHEN** 命令接收方未实现货舱查询接口
- **THEN** 字段求值 SHALL 返回无效值而非抛出异常
