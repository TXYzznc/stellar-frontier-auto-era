## ADDED Requirements

### Requirement: 选中实例后读模型暴露图快照

算法编辑器使用的读模型在用户选中一个算法实例后，SHALL 暴露该实例草稿的图结构（图节点、图连线）与完整校验结果（错误/警告，含节点定位）。

#### Scenario: 有实例且已选中

- **WHEN** 机器存在算法实例且读模型选中了其中一个实例
- **THEN** 快照的 `GraphNodes` 非空且数量等于草稿中未删除节点数；`GraphEdges` 非空且数量等于草稿连线数；`Issues` 非空（至少包含一条编译产生的错误或警告，或为空的合法「无问题」结果）

#### Scenario: 有实例但未选中

- **WHEN** 机器存在算法实例但读模型未选中任何实例
- **THEN** `GraphNodes`/`GraphEdges`/`Issues` 均为空（null 或空集合），不得伪造上一实例的图

#### Scenario: 无实例或域不可用

- **WHEN** 机器没有算法实例，或算法域不可用
- **THEN** `GraphNodes`/`GraphEdges`/`Issues` 均为空

### Requirement: 校验问题映射错误与警告

读模型 SHALL 把 `AlgorithmValidator.TryCompile` 产出的问题清单映射为界面可展示的问题行，保留严重度、代码与节点定位。

#### Scenario: 草稿含结构错误

- **WHEN** 选中实例的草稿编译失败（存在类型不匹配、同步环或缺失绑定等错误）
- **THEN** `Issues` 中存在严重度为「错误」的行，且该行的节点定位与 `AlgorithmIssue.NodeId` 一致

#### Scenario: 草稿编译通过但有警告

- **WHEN** 草稿编译通过但存在警告（如绑定暂时不可用）
- **THEN** `Issues` 中存在严重度为「警告」的行，且没有严重度为「错误」的行

### Requirement: 节点选中与节点详情

读模型 SHALL 支持按节点 Id 选中图节点，并把该节点的属性映射为检视器可展示的详情字段。

#### Scenario: 选中存在的节点

- **WHEN** 已选中实例且传入的节点 Id 存在于草稿
- **THEN** `SelectNode` 返回 `true`，快照的 `SelectedNode` 指向该节点，`NodeDetail` 非空且包含节点 Kind 与 Operator

#### Scenario: 选中不存在的节点或未选中实例

- **WHEN** 传入的节点 Id 不存在于草稿，或尚未选中任何实例
- **THEN** `SelectNode` 返回 `false`，`SelectedNode` 保持空，`NodeDetail` 为空

#### Scenario: 清空实例选中同时清空节点选中

- **WHEN** 读模型清空实例选中（`ClearSelection`）
- **THEN** `SelectedNode` 与 `NodeDetail` 一并清空

### Requirement: 编辑器以真实图数据渲染

算法编辑器打开时，节点栏、检视器、问题栏 MUST 展示选中实例的真实图数据，而不是实例列表/机器详情/「诊断尚未运行」占位。

#### Scenario: 编辑器渲染图节点与问题

- **WHEN** 打开算法编辑器且选中了一个实例
- **THEN** 节点栏渲染 `GraphNodes`（可点选），问题栏渲染 `Issues`；未选中节点时检视器展示实例图摘要而非机器详情占位

#### Scenario: 点选节点后检视器更新

- **WHEN** 用户在节点栏点选一个图节点
- **THEN** 检视器展示该节点的 `NodeDetail` 属性
