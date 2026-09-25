## Why

b23 已把「创建实例」接到跳转算法工作台，但工作台只显示图结构与「缺少绑定」校验问题，玩家无处**逐项绑定**。模板实例化的草稿里 `Input`/`Effector` 节点带 `BindingKey`（如 `soil`、`gun`），`Bindings` 却为空——`TryCompile(template:false)` 因此报 `RequiredBinding`。本变更落「绑定重绑」的接线：读模型暴露「待绑定项」，领域层提供 `Rebind` 命令，集中待绑定面板（`AlgorithmBindingForm`）渲染真实待绑定清单。

## What Changes

- `AlgorithmInstanceService` 新增 `Rebind(id, expectedRevision, bindingKey, componentId, targetId, generation)`：编辑草稿 `Bindings` 里某个 `Key` 的绑定（无则新增），绑定 `Type` 从对应节点的 `ValueType` 派生，`Revision` 自增。
- 读模型新增 `UiAlgorithmBindingRow` 与快照 `PendingBindings`/`PendingBindingCount`；`MachineAlgorithmReadModel.BuildGraph` 从选中实例草稿派生「有 `BindingKey` 但无有效绑定」的 Input/Effector 端点。
- `IAlgorithmReadModel` 新增 `bool Rebind(ulong instanceId, string bindingKey, ulong componentId, ulong targetId, ulong generation)`；机器域实现，库页/不可用域返回 false。
- `AlgorithmBindingForm` 接线：渲染待绑定清单（替换「未接线」空态），移除 `DisableDomainActions()`。

## Capabilities

### New Capabilities

- `algorithm-binding`: 算法实例草稿的绑定端点——Input/Effector 节点的 `BindingKey` 与「已绑定/待绑定」状态，可被 `Rebind` 命令更新。

## Impact

- 修改 `Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmInstanceService.cs`（`Rebind`）。
- 修改 `Assets/Game/Scripts/AutoEra/UI/Integration/AlgorithmReadModel.cs`（`UiAlgorithmBindingRow`、快照字段、`BuildGraph` 派生、接口命令、机器域实现）。
- 修改 `Assets/Game/Scripts/AutoEra/UI/AlgorithmBindingForm.cs`（渲染待绑定清单 + 移除 DisableDomainActions）。
- 修改 `Assets/Game/Tests/AutoEra/Editor/AlgorithmInstanceEditModeTests.cs` 与 `UI/AlgorithmReadModelEditModeTests.cs`（Rebind + 待绑定项回归）。

## Non-Goals

- 不做组件/世界对象**选择器**交互（`NodeComponentPickerForm` 回写）——本批只接线「待绑定项可见 + `Rebind` 命令」，选组件的 UI 交互后续批。
- 不做传感器运行时注册（`MachineSensorSet.Bind`/`BindResourceAmount`）——那是「应用后」adapter 的事，与「草稿绑定数据」分层。
- 不做编译应用（b25）。
