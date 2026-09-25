## ADDED Requirements

### Requirement: 候选组件暴露

算法域机器读模型 SHALL 暴露机器上已安装、可被算法端点绑定的候选组件（传感器/效应器）。

#### Scenario: 候选来自机器槽位

- **WHEN** 机器已安装传感器或效应器组件
- **THEN** 快照 `ComponentCandidates` 按槽位顺序列出这些组件（含展示名、类别、等级、启用状态）

#### Scenario: 没有可绑定组件

- **WHEN** 机器只装了核心或尚未安装传感器/效应器
- **THEN** `ComponentCandidates` 为空列表（非 null）

#### Scenario: 非机器域

- **WHEN** 库页/不可用域读模型
- **THEN** `ComponentCandidates` 为 null

### Requirement: 按端点类别筛选候选

组件选择器 SHALL 按目标端点类别筛选候选组件。

#### Scenario: Input 端点

- **WHEN** 请求端点类别为 Input
- **THEN** 候选只含传感器组件

#### Scenario: Effector 端点

- **WHEN** 请求端点类别为 Effector
- **THEN** 候选只含效应器组件

### Requirement: 点选回写绑定

组件选择器点选候选 SHALL 回写该端点的组件绑定。

#### Scenario: 点选候选

- **WHEN** 玩家点选一个候选组件
- **THEN** 该端点的 `Rebind` 以候选组件 Id 为 `ComponentId` 写入草稿，并关闭选择器
