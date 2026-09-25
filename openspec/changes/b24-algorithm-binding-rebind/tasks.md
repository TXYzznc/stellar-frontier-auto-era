## 1. 领域层

- [x] 1.1 `AlgorithmInstanceService.Rebind(id, expectedRevision, bindingKey, componentId, targetId, generation)`（Type 从节点派生、Revision 自增、Changed）

## 2. 读模型与命令

- [x] 2.1 新增 `UiAlgorithmBindingRow`（BindingKey/Kind/Field/Action/Bound/ComponentId/TargetId）
- [x] 2.2 `AlgorithmDomainSnapshot` 加 `PendingBindings` + `PendingBindingCount`
- [x] 2.3 `MachineAlgorithmReadModel.BuildGraph` 派生待绑定项
- [x] 2.4 `IAlgorithmReadModel.Rebind(...)` 接口命令；机器域实现，库页/不可用域返回 false

## 3. 绑定面板

- [x] 3.1 `AlgorithmBindingForm.Render` 渲染待绑定清单 + 需求说明，移除 `DisableDomainActions()`

## 4. 回归与收口

- [x] 4.1 领域层回归：`Rebind_UpdatesDraftBinding_AndIncrementsRevision`（`AlgorithmInstanceEditModeTests` 7/7）
- [x] 4.2 读模型回归：`MachineDomain_DraftInstance_ExposesPendingBindings_AndRebindMarksBound`（`AlgorithmReadModelEditModeTests` 20/20）
- [x] 4.3 普通编译 0 错误 0 警告（仅 FMOD 良性告警）、Console 0 错误、`openspec validate b24-algorithm-binding-rebind --strict` 通过
- [x] 4.4 更新本 tasks/design 收口，回传结果

> 组件/世界对象**选择器**交互（`NodeComponentPickerForm` 回写 `Rebind`）本批未做，属交互层，后续批；`Rebind` 的 `componentId`/`targetId` 参数由测试/后续 UI 提供。
