## ADDED Requirements

### Requirement: 实例服务暴露运行时历史

算法实例服务 SHALL 提供只读历史出口，返回该实例运行时最近运行记录的快照。

#### Scenario: 有实例且有运行历史

- **WHEN** 机器存在算法实例且该实例运行时已产生运行记录
- **THEN** `ReadHistory(instanceId)` 返回非空记录数组，元素为该实例 `AlgorithmRuntime.History()` 的快照

#### Scenario: 无实例或运行时尚未运行

- **WHEN** 传入的实例 Id 不存在，或运行时尚无任何运行记录
- **THEN** `ReadHistory` 返回空数组，不抛异常

### Requirement: 读模型标记节点执行状态

机器读模型 SHALL 依据最近一次运行记录，把图节点标记为「已执行／失败／未执行」状态。

#### Scenario: 最近运行有执行路径

- **WHEN** 选中实例的最近运行记录包含执行路径 `CopyPath()`
- **THEN** 快照中路径命中的图节点 `Diagnostic` 为 `Executed`，未命中的为 `None`

#### Scenario: 最近运行有失败节点

- **WHEN** 最近运行记录的 `FailedNode` 非零
- **THEN** 该节点的 `Diagnostic` 为 `Failed`（失败优先于已执行）

#### Scenario: 无运行历史

- **WHEN** 选中实例但运行时无任何运行记录
- **THEN** 快照 `LatestRun` 为 null，所有图节点 `Diagnostic` 为 `None`

### Requirement: 快照暴露运行摘要

算法读模型 SHALL 在快照暴露最近一次运行的摘要（运行号、错误、失败节点、成本）。

#### Scenario: 有最近运行

- **WHEN** 选中实例存在最近运行记录
- **THEN** `LatestRun` 非空，其 `RunId`/`Cost` 与记录一致，`Error`/`FailedNode` 与记录一致

#### Scenario: 无运行历史

- **WHEN** 选中实例无运行记录
- **THEN** `LatestRun` 为 null

### Requirement: 编辑器展示诊断状态

算法编辑器 MUST 在诊断读路径中展示最近运行摘要与节点执行状态。

#### Scenario: 展示运行摘要

- **WHEN** 打开算法编辑器且选中实例存在运行历史
- **THEN** 检视器展示最近运行的运行号、错误/正常、失败节点与成本

#### Scenario: 无运行历史时如实说明

- **WHEN** 选中实例无运行历史
- **THEN** 检视器明确说明「暂无运行记录」，不伪装成「运行正常」
