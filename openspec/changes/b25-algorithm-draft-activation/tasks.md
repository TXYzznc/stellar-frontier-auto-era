## 1. 领域层

- [x] 1.1 `AlgorithmInstanceService.CompileDraft(ulong id, AlgorithmRuntime runtime)`（仅草稿实例、Id 一致、IsSafe、容量、挂接 + Changed）

## 2. 读模型命令

- [x] 2.1 `IAlgorithmReadModel.ActivateDraft(ulong instanceId)` 接口；机器域实现（读草稿 → TryCompile 真实容量 → new AlgorithmRuntime → Adapter.Attach → CompileDraft）；库页/不可用域返回 false
- [x] 2.2 `AlgorithmMachineAdapter.HasRuntime`（单运行时前置检查，多实例激活后续批）

## 3. 回归与收口

- [x] 3.1 领域层回归：`CompileDraft_ActivatesRuntime_AndRejectsNonDraftOrMismatch`（`AlgorithmInstanceEditModeTests` 8/8）
- [x] 3.2 读模型回归：`MachineDomain_ActivateDraft_CompilesBoundDraft_AndRejectsUnbound`（`AlgorithmReadModelEditModeTests` 21/21）
- [x] 3.3 普通编译 0 错误 0 警告（仅 FMOD 良性告警）、Console 0 错误、`openspec validate b25-algorithm-draft-activation --strict` 通过
- [x] 3.4 更新本 tasks/design 收口，回传结果

> 激活后 `AppliedRevision` = 编译时草稿修订（非恒为 1）：`Rebind` 每次自增草稿修订，`TryCompile` 的 plan.Revision 继承该值。测试断言用「非 0 + 无未应用草稿」表达「不再是草稿」，避免耦合端点数量。
>
> 编辑器「应用」按钮接线（`Btn_AlgorithmEditorApply` onClick → 应用请求链/`ConfirmWarnings` UI）属后续批（交互层）。
