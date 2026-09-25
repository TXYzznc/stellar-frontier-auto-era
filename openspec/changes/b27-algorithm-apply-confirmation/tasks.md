## 1. 读模型命令

- [x] 1.1 `IAlgorithmReadModel.ConfirmWarnings`/`CancelApply` 接口；机器域转发实例服务；库页/不可用域返回 false

## 2. 工作台接线

- [x] 2.1 `AlgorithmEditorForm.OnApplyClicked` 按请求状态分支（AwaitingWarningConfirmation → ConfirmWarnings / 否则 Apply）
- [x] 2.2 `SetApplyButtonInteractable` 在 WaitingSafePoint/Applying 时禁用按钮

## 3. 回归与收口

- [x] 3.1 读模型回归：`MachineDomain_ConfirmWarningsAndCancelApply_DispatchToService`（`AlgorithmReadModelEditModeTests` 23/23）
- [x] 3.2 普通编译 0 错误 0 警告（仅 FMOD 良性告警）、Console 0 错误、`openspec validate b27-algorithm-apply-confirmation --strict` 通过
- [x] 3.3 更新本 tasks 收口，回传结果

> 独立「确认警告」对话框与「取消」按钮 UI 属后续批；本批复用「应用草稿」按钮动态行为。
