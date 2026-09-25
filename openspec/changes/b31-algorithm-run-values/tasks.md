## 1. 领域层

- [x] 1.1 `AlgorithmBatch.NodeValues` + `AlgorithmEvaluation.Value` 记录主输出值
- [x] 1.2 `AlgorithmRunRecord.CopyNodeValues()`（深拷贝）

## 2. 读模型

- [x] 2.1 `UiAlgorithmRunRow.NodeValues` + `_latestRun` 携带节点值快照
- [x] 2.2 `BuildNodeDetail` 选中节点有当时值时追加「当时值」行

## 3. 回归与收口

- [x] 3.1 `AlgorithmExecutionEditModeTests` 8/8（新增 `NodeValues_SnapshotExecutedValueNodes`）
- [x] 3.2 `AlgorithmInstanceEditModeTests` 10/10
- [x] 3.3 `AlgorithmReadModelEditModeTests` 28/28（新增 `MachineDomain_AfterRun_SelectNode_ExposesThenValue`）
- [x] 3.4 普通编译 0 错误 0 警告、Console 0 错误
- [x] 3.5 `openspec validate b31-algorithm-run-values --strict` 通过
- [x] 3.6 更新本 tasks 收口，回传结果
