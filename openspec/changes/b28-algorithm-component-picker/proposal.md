## Why

节点组件选择器（`NodeComponentPickerForm`，规格 13）此前对整页显示 Empty，理由是「候选需要一个输入节点作为上下文、通道尚未接线」。但候选组件本身并不依赖那个上下文——「机器上已安装哪些可绑定的组件（传感器/效应器）」是算法域可以直接回答的事实。本变更把候选从「撒谎的空态」变成真实数据，并接上「绑定面板点待绑定项 → 选择器点选组件 → 回写 Rebind」的完整链路。

## What Changes

- `AlgorithmReadModel` 新增 `UiAlgorithmComponentCandidate`（组件 Id + 展示名 + 类别 + 等级 + 是否启用）。
- `AlgorithmDomainSnapshot` 新增 `ComponentCandidates`/`ComponentCandidateCount`；机器域读模型扫机器槽位（Sensor/Effector）构建候选，展示名经 `MachineCatalog` 反查。
- `AlgorithmReadModels.Create` 机器域传 `MachineCatalog`（数据未加载时为 null，候选名降级为「组件 #id」）。
- `NodeComponentPickerForm.Render` 从 Empty 改为渲染候选列表，按端点类别筛选（Input→Sensor / Effector→Effector）。
- 新增 `AutoEraAlgorithmBindingPickRequest`（实例 + BindingKey + 端点类别）；`AlgorithmBindingForm` 点待绑定项打开选择器，选择器点选候选回写 `Rebind`（组件 Id）并返回。

## Capabilities

### New Capabilities

- `algorithm-component-candidates`: 机器域算法读模型暴露可绑定候选组件，选择器渲染并点选回写绑定。

## Impact

- 修改 `Assets/Game/Scripts/AutoEra/UI/Integration/AlgorithmReadModel.cs`（struct + 快照字段 + 读模型构建）。
- 新增 `Assets/Game/Scripts/AutoEra/UI/Integration/AutoEraAlgorithmBindingPickRequest.cs`。
- 修改 `Assets/Game/Scripts/AutoEra/UI/NodeComponentPickerForm.cs`（筛选 + 点选回写）。
- 修改 `Assets/Game/Scripts/AutoEra/UI/AlgorithmBindingForm.cs`（点待绑定项打开选择器）。
- 修改 `Assets/Game/Tests/AutoEra/Editor/UI/AlgorithmReadModelEditModeTests.cs`（候选暴露 + 请求回归）。

## Non-Goals

- 不做目标对象（`TargetId`）选择：Input 端点绑定组件后仍需由后续世界对象选择器补齐目标对象，`Rebind` 里 `TargetId` 暂为 0。
- 不做「重新绑定」的代数自增（首次绑定 generation=1；重绑代数自增属后续）。
