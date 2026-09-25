## ADDED Requirements

### Requirement: 草稿绑定重绑命令

算法实例服务 SHALL 提供按 `BindingKey` 更新草稿绑定的命令。

#### Scenario: 新增或更新绑定

- **WHEN** 以有效实例 Id、匹配的草稿修订与 `BindingKey` 调用 `Rebind`
- **THEN** 草稿 `Bindings` 中该 `Key` 的 `ComponentId`/`TargetId`/`Generation` 被更新（无则新增），`Type` 派生自对应节点，`Revision` 自增，并触发 `Changed`

#### Scenario: 无效参数或修订不匹配

- **WHEN** 实例不存在、`BindingKey` 为空或 `expectedRevision` 与草稿修订不符
- **THEN** 返回 false 且草稿不变

### Requirement: 待绑定项读模型

算法读模型 SHALL 暴露选中实例草稿的待绑定端点。

#### Scenario: 模板实例化后的草稿

- **WHEN** 选中实例的草稿含 `Input`/`Effector` 节点（`BindingKey` 非空）且 `Bindings` 无对应绑定
- **THEN** `PendingBindings` 含这些端点，`PendingBindingCount` 等于其数量，每项标注「待绑定」

#### Scenario: 绑定已补齐

- **WHEN** 某端点已有有效绑定（`ComponentId != 0`）
- **THEN** 该项标注「已绑定」并展示组件与目标 Id

### Requirement: 绑定面板渲染待绑定清单

集中待绑定面板 SHALL 在有实例时渲染真实待绑定清单。

#### Scenario: 有选中实例

- **WHEN** 算法域 `Ready` 且存在选中实例
- **THEN** 绑定栏列出 `PendingBindings`，需求栏说明「系统模板不预置绑定」，业务动作不再整域禁用
