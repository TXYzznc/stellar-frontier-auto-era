## 1. 实例服务历史出口

- [x] 1.1 `AlgorithmInstanceService` 新增 `ReadHistory(ulong id)`，返回 `AlgorithmRunRecord[]` 快照（无实例/无运行时为空）

## 2. 读模型诊断标记与运行摘要

- [x] 2.1 新增 `UiAlgorithmNodeDiagnostic` 枚举（None/Executed/Failed）与 `UiAlgorithmRunRow` 行结构
- [x] 2.2 `UiAlgorithmNodeRow` 增加 `Diagnostic` 字段并并入 `Status` 文本
- [x] 2.3 `AlgorithmDomainSnapshot` 增加 `LatestRun`（无历史为 null）
- [x] 2.4 `MachineAlgorithmReadModel.BuildGraph` 读最近运行记录，标记节点执行/失败状态，构建 `LatestRun`

## 3. 编辑器展示

- [x] 3.1 `AlgorithmEditorForm` 检视器追加运行摘要（运行号/错误或正常/失败节点/成本）；无历史时如实说明「暂无运行记录」

## 4. 回归与收口

- [x] 4.1 读模型 EditMode 回归：运行后历史非空、节点状态正确、无历史时 LatestRun 为空（`AlgorithmReadModelEditModeTests` 18/18 全绿）
- [x] 4.2 普通编译 0 错误 0 警告、Console 0 错误、`openspec validate b21-algorithm-diagnostic-read --strict` 通过
- [x] 4.3 更新本 tasks/design 收口，回传结果
