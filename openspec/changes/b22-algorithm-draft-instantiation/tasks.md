## 1. 实例服务草稿实例

- [x] 1.1 `AlgorithmInstanceService.AddDraft(AlgorithmDocument)` 创建 `Runtime=null` 草稿实例（校验 DocumentId/Nodes/Edges/Bindings/Id 冲突，初始 Saved=Draft.Copy()）
- [x] 1.2 空运行时守卫：`ListInstances`/`SavedDraftRevision`/`Apply`/`Capture`/`Restore`/`Pump`/`TotalCost`/`Dispose`

## 2. 命令与读模型

- [x] 2.1 `IAlgorithmReadModel.InstantiateTemplate(ulong templateId)` 接口命令
- [x] 2.2 `TemplateAlgorithmReadModel` 构造函数增 `AutoEraUiSession`，实现 `InstantiateTemplate`（解析机器 → 实例服务 → `_library.Instantiate` → `AddDraft`）；`MachineAlgorithmReadModel`/`UnavailableAlgorithmReadModel` 返回 0
- [x] 2.3 `AlgorithmReadModels.TryResolveMachine` 提升为 internal 供库页读模型复用
- [x] 2.4 `UiAlgorithmInstanceRow.Status` 对 `AppliedRevision == 0` 显示「未应用」

## 3. 回归与收口

- [x] 3.1 领域层回归：`DraftInstance_NoRuntime_ListsReadsAndGuardsWithoutThrow` 单测 1/1 通过（AddDraft/ListInstances/ReadHistory/Apply/Capture/Pump/Dispose 守卫全绿）
- [x] 3.2 读模型回归：`LibraryDomain_InstantiateTemplate_CreatesDraftInstance_VisibleInMachineDomain` 通过（`AlgorithmReadModelEditModeTests` 19/19，含新测试；草稿实例图 + 缺少绑定问题）
- [x] 3.3 普通编译 0 错误 0 警告、Console 0 错误、`openspec validate b22-algorithm-draft-instantiation --strict` 通过
- [x] 3.4 更新本 tasks/design 收口，回传结果

> 备注：`AlgorithmInstanceEditModeTests` 类级 `test_run_by_name` 报 5/5（漏第 6 个），但 DLL 内 6 个方法全在、单个测试名运行 1/1 通过——Unity Test Runner 发现缓存 quirk，不影响功能正确性；已在 memory 记录。
