## ADDED Requirements

### Requirement: 统一应用命令

算法读模型 SHALL 提供 `Apply(instanceId)`，按实例状态分派「激活」或「应用」。

#### Scenario: 草稿实例首次应用

- **WHEN** 实例 `AppliedRevision == 0` 且 `Apply` 被调用
- **THEN** 走 `ActivateDraft` 路径（校验通过则编译为运行时，返回 true）

#### Scenario: 已激活实例应用草稿

- **WHEN** 实例 `AppliedRevision > 0` 且草稿领先（`DraftRevision > AppliedRevision`）
- **THEN** 走 `Apply` 路径，应用请求创建成功返回 true；草稿未领先返回 false

#### Scenario: 库页或不可用域

- **WHEN** 调用来自库页/不可用域读模型
- **THEN** 返回 false

### Requirement: 实例服务应用重载

算法实例服务 SHALL 提供无需显式硬件修订的 `Apply` 重载。

#### Scenario: 内部取硬件修订

- **WHEN** 以实例 Id、期望草稿修订、期望已应用修订调用无 hardware 的 `Apply`
- **THEN** 服务内部用 `_hardwareRevision()` 取当前硬件修订，行为与 4 参数重载一致

### Requirement: 工作台应用按钮接线

算法工作台 SHALL 把「应用草稿」按钮接到统一应用命令。

#### Scenario: 点击应用草稿

- **WHEN** 存在选中实例且点击 `Btn_AlgorithmEditorApply`
- **THEN** 调用 `Apply(选中实例 Id)`，状态随 `Changed` 事件刷新
