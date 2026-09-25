## 1. 领域层

- [x] 1.1 `AlgorithmInstanceService.Apply(id, expectedDraft, expectedApplied, out request)` 无 hardware 重载（内部 `_hardwareRevision()`）

## 2. 读模型命令

- [x] 2.1 `IAlgorithmReadModel.Apply(ulong instanceId)` 接口；机器域按状态分支（草稿 → ActivateDraft / 已激活 → Apply）；库页/不可用域返回 false

## 3. 工作台接线

- [x] 3.1 contract `bindings` 加 `Btn_AlgorithmEditorApply`（kind Button），重新生成 `AlgorithmEditorForm.Fields.cs`（处理 EOL churn，checkout 6 个无关 `.Fields.cs`）
- [x] 3.2 `AlgorithmEditorForm` 接线 onClick → `Apply(选中实例 Id)`；`SetApplyButtonInteractable` 在 `DisableDomainActions` 后按域状态重开该按钮

## 4. 回归与收口

- [x] 4.1 领域层回归：`ApplyOverload_UsesInternalHardwareRevision`（`AlgorithmInstanceEditModeTests` 9/9）
- [x] 4.2 读模型回归：`MachineDomain_Apply_DispatchesActivationThenApply`（`AlgorithmReadModelEditModeTests` 22/22）
- [x] 4.3 普通编译 0 错误 0 警告、Console 0 错误、`openspec validate b26-algorithm-apply-command --strict` 通过
- [x] 4.4 更新本 tasks/design 收口，回传结果

> 「应用」命令的 `AwaitingWarningConfirmation` 确认交互（`ConfirmWarnings`/`CancelApply` UI）属后续批；本批只触发命令，界面靠 `Changed` 刷新 `RequestState` 呈现当前状态。
