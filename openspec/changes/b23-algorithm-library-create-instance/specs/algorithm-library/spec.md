## MODIFIED Requirements

### Requirement: 库页创建实例按钮字段映射

算法库页契约 SHALL 为三个「创建实例」按钮声明字段映射，生成对应 `.Fields.cs` 字段。

#### Scenario: 字段生成

- **WHEN** 契约 JSON 的 `bindings` 含 `_systemTemplatesCreateButton`／`_playerTemplatesCreateButton`／`_templateDetailCreateButton`（kind `Button`）并运行生成器
- **THEN** `AlgorithmLibraryForm.Fields.cs` 含三个 `Button` 字段与同名只读属性，路径指向对应 `Btn_*Create` 节点

### Requirement: 创建实例按钮接线

算法库页 SHALL 在有模板时启用「创建实例」按钮，点击后在当前选中机器创建草稿实例并跳转算法工作台。

#### Scenario: 有模板时按钮可点

- **WHEN** 快照 `Count > 0`
- **THEN** 三个「创建实例」按钮 `interactable` 为 true，且不被整域禁用

#### Scenario: 点击创建并跳转

- **WHEN** 选中模板且当前机器可解析，点击「创建实例」
- **THEN** 调用 `InstantiateTemplate` 返回非零实例 Id，并打开 `AlgorithmEditorForm`

#### Scenario: 无机器或模板无效时不跳转

- **WHEN** `InstantiateTemplate` 返回 0
- **THEN** 保持库页、不打开算法工作台
