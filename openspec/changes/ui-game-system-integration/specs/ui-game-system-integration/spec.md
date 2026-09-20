# UI Game System Integration

## ADDED Requirements

### Requirement: Session access without global state

界面 SHALL 通过打开参数（`UIParams`）获得会话句柄，SHALL NOT 依赖全局服务定位器或 DI 容器。

#### Scenario: A form opens with a world session

- **WHEN** a procedure opens a form and injects an `AutoEraUiSession`
- **THEN** the form can reach application and world services through that session
- **AND** no static or global service lookup is introduced

#### Scenario: A required parameter is missing

- **WHEN** a form has no session parameter
- **THEN** it presents an unavailable state with a reason instead of throwing

### Requirement: Read models are per data domain and read-only

读取模型 SHALL 按数据域划分、只暴露页面所需字段，SHALL NOT 让界面直接依赖领域内部结构；仅被单页使用且只读单个服务的域 SHALL NOT 建读取模型。

#### Scenario: Two pages show machine data

- **WHEN** two different pages need machine data
- **THEN** both subscribe to the same machine read model instead of binding `MachineRoster` directly

### Requirement: Three-state data presentation

每个数据区域 SHALL 能区分 `Ready`、`Empty` 与 `Unavailable`；`Unavailable` SHALL 说明具体原因并禁用写操作，SHALL NOT 显示伪造数值。

#### Scenario: A domain system is not implemented yet

- **WHEN** a page whose domain has no runtime service is opened
- **THEN** its regions present `Unavailable` with a reason
- **AND** no fabricated values are shown

### Requirement: Structure is owned by the contract

接入实现 SHALL NOT 改变预制体结构；门1 契约检查 SHALL 对全部界面持续通过。

#### Scenario: Integration code is added

- **WHEN** a form gains read models, subscriptions and intent bindings
- **THEN** the gate-1 contract check still passes for all 33 forms

### Requirement: Subscribe on open and release on close

界面 SHALL 在打开时订阅、关闭时退订，SHALL NOT 在 `Update` 轮询或分配。

#### Scenario: A form closes

- **WHEN** a form with an active read model closes
- **THEN** its subscriptions are removed
- **AND** no later callback reaches the form
