## 1. 读模型

- [x] 1.1 `UiAlgorithmComponentCandidate` struct + 快照 `ComponentCandidates`/`ComponentCandidateCount`
- [x] 1.2 `MachineAlgorithmReadModel` 持有 `MachineCatalog`，扫机器 Sensor/Effector 槽位构建候选
- [x] 1.3 `AlgorithmReadModels.Create` 机器域传 `MachineCatalog`

## 2. 选择器渲染

- [x] 2.1 `NodeComponentPickerForm.Render` 从 Empty 改为渲染候选列表 + 状态组切换

## 3. 点选回写（跨表单）

- [x] 3.1 `AutoEraAlgorithmBindingPickRequest`（实例 + BindingKey + 端点类别）
- [x] 3.2 `AlgorithmBindingForm` 点待绑定项 → 打开 `NodeComponentPickerForm`（传请求）
- [x] 3.3 `NodeComponentPickerForm` 按端点类别筛选候选（Input→Sensor / Effector→Effector），点选 → `Rebind(组件 Id)` 并返回

## 4. 回归与收口

- [x] 4.1 读模型回归：`MachineDomain_ExposesInstalledComponentCandidates` + `AlgorithmBindingPickRequest_CarriesInstanceAndEndpoint`（`AlgorithmReadModelEditModeTests` 25/25）
- [x] 4.2 普通编译 0 错误 0 警告、Console 0 错误、`openspec validate b28-algorithm-component-picker --strict` 通过
- [x] 4.3 更新本 tasks 收口，回传结果

> 目标对象（`TargetId`）由后续世界对象选择器补齐；Input 端点绑定组件后仍需选目标对象（`Rebind` 里 TargetId 暂为 0）。
