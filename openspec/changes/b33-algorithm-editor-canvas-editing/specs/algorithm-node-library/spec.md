# algorithm-node-library

## ADDED Requirements

### Requirement: 节点种类目录行

读模型 SHALL 将完整 AlgorithmNodeKind 目录暴露为稳定行：种类、显示名、分类（输入／判断与运算／状态／流程／行为／数值）、逻辑成本与输入/输出端口描述（端口名＋值类型）；目录在机器、模板与不可用三个域 MUST 一致，因为它是能力清单而非实例数据。

#### Scenario: 目录覆盖全部种类

- **WHEN** 界面请求节点库
- **THEN** 每个已定义的 AlgorithmNodeKind 恰好出现一次，带分类与成本

#### Scenario: 不可用域仍展示目录

- **WHEN** 算法运行时不可用
- **THEN** 库行保持可见（创建由域状态禁用，而非隐藏目录）

### Requirement: 节点栏是节点库

AlgorithmEditorForm 节点栏 MUST 展示节点种类目录（支持按显示名搜索过滤），而非当前实例的图节点列表；图节点选择 MUST 经画布节点按钮完成。

#### Scenario: 搜索过滤

- **WHEN** 用户在节点搜索框输入文字
- **THEN** 仅显示显示名包含该词（大小写不敏感）的行

#### Scenario: 域未就绪时点击

- **WHEN** 未选中机器实例或域状态非 Ready 时点击行
- **THEN** 行仍列出但不发生草稿变更

### Requirement: 快照含逐节点端口行

读模型快照 SHALL 为选中实例的每个图节点提供输入与输出端口行：端口 Id、显示名、值类型与连接状态（是否有边指向/离开该端口）；画布以稳定顺序渲染到既有端口模板。

#### Scenario: 已连接端口

- **WHEN** 草稿边绑定节点 A 的输出端口 value 到节点 B 的输入端口 a
- **THEN** B 的输入端口行与 A 的输出端口行均报告已连接

#### Scenario: 连线后端口行刷新

- **WHEN** 用户完成两步连线
- **THEN** 受影响端口行的连接状态经 Changed 事件重渲染后更新
