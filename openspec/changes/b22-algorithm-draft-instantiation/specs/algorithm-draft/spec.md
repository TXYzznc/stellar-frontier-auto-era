## ADDED Requirements

### Requirement: 实例服务持有草稿实例

算法实例服务 SHALL 支持「草稿实例」：一个只有草稿、没有运行时、未绑定未编译的实例。

#### Scenario: 从未绑定文档创建草稿实例

- **WHEN** 以一份 `DocumentId` 非零、`Nodes`/`Edges`/`Bindings` 均非空且 Id 未占用的文档调用 `AddDraft`
- **THEN** 实例服务新增该实例，其 `Draft` 为文档副本、`Saved` 为同副本、`Runtime` 为空，并触发 `Changed`

#### Scenario: 草稿实例的只读出口不抛异常

- **WHEN** 实例列表中存在草稿实例并调用 `ListInstances`/`ReadDraft`/`ReadHistory`/`SavedDraftRevision`
- **THEN** `ListInstances` 该行的 `AppliedRevision` 与 `LogicCost` 为 0、`DraftRevision` 为草稿修订，`ReadDraft` 返回草稿副本，`ReadHistory` 返回空数组，`SavedDraftRevision` 返回草稿修订，均不抛异常

#### Scenario: 草稿实例不可应用或捕获

- **WHEN** 对草稿实例调用 `Apply`/`Capture`/`Restore`
- **THEN** 返回 false 且不改变实例状态

### Requirement: 从模板创建草稿实例的命令

算法读模型 SHALL 提供从模板在当前选中机器上创建草稿实例的命令。

#### Scenario: 有选中机器与有效模板

- **WHEN** 库页读模型选中机器存在且模板 Id 有效，调用 `InstantiateTemplate(templateId)`
- **THEN** 返回新实例的稳定 Id（非零），该机器实例服务新增一份未绑定草稿实例

#### Scenario: 无法解析机器或模板无效

- **WHEN** 无选中机器、机器无运行时，或模板 Id 无效
- **THEN** `InstantiateTemplate` 返回 0 且不新增实例

### Requirement: 草稿实例的实例行展示

算法读模型 SHALL 把未应用的草稿实例展示为「未应用」而非「已应用 r0」。

#### Scenario: 草稿实例行状态

- **WHEN** 快照中某实例的 `AppliedRevision` 为 0
- **THEN** 该实例行的状态文本含「未应用」且不显示「已应用 r0」
