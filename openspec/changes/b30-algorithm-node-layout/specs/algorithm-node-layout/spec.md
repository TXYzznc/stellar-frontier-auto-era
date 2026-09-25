## ADDED Requirements

### Requirement: 移动草稿节点画布坐标

算法实例服务 SHALL 支持移动草稿节点的画布坐标。

#### Scenario: 移动节点

- **WHEN** 以匹配的修订移动一个存在且未删除的节点
- **THEN** 该节点的 `LayoutX`/`LayoutY` 更新为给定坐标，草稿 `Revision` 加一

#### Scenario: 未找到节点

- **WHEN** 移动一个不存在或已删除的节点
- **THEN** 返回 false 且草稿不变

#### Scenario: 修订不匹配

- **WHEN** 以过期修订移动节点
- **THEN** 返回 false 且草稿不变

### Requirement: 读模型移动命令

机器域算法读模型 SHALL 提供移动节点的命令。

#### Scenario: 机器域

- **WHEN** 机器域读模型移动节点
- **THEN** 读草稿修订后转发实例服务

#### Scenario: 库页/不可用域

- **WHEN** 库页/不可用域读模型移动节点
- **THEN** 返回 false

### Requirement: 画布节点定位与拖拽

算法工作台画布 SHALL 按布局坐标定位图节点并支持拖拽回写。

#### Scenario: 定位节点

- **WHEN** 图快照含布局坐标的节点
- **THEN** 画布按各节点 `LayoutX`/`LayoutY` 实例化并定位节点，填节点名

#### Scenario: 拖拽回写

- **WHEN** 玩家拖动画布节点并松手
- **THEN** 该节点经 `MoveNode` 以新坐标写回草稿
