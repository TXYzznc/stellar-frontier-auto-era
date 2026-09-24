## 1. 读模型图快照与行类型

- [x] 1.1 新增只读行结构 `UiAlgorithmNodeRow`/`UiAlgorithmEdgeRow`/`UiAlgorithmIssueRow`（Label/Status 展示属性）
- [x] 1.2 `AlgorithmDomainSnapshot` 新增 `GraphNodes`/`GraphEdges`/`Issues`/`NodeDetail`/`SelectedNode`（可选构造参数，模板域/不可用域保持空）
- [x] 1.3 `IAlgorithmReadModel` 新增 `SelectNode(ulong nodeId)`；`UnavailableAlgorithmReadModel`/`TemplateAlgorithmReadModel` 返回 `false`
- [x] 1.4 `MachineAlgorithmReadModel` 在选中实例后读草稿（`ReadDraft`）并 `TryCompile(draft, int.MaxValue, ..., false)`，映射节点/连线/问题；清空实例选中时清空节点选中；顺带修正 `BuildDetail` 用 `_rows[_selectedIndex]` 替代旧快照回读

## 2. 编辑器真实渲染

- [x] 2.1 `AlgorithmEditorForm.Render` 节点栏改渲染 `GraphNodes`（点击回调 `SelectNode`）
- [x] 2.2 检视器改渲染机器/实例详情 + 图结构摘要 + `NodeDetail`（无选中节点时提示点选）
- [x] 2.3 问题栏改渲染 `Issues`（错误/警告 + 节点定位），无问题时明确写「校验通过」

## 3. 回归与收口

- [x] 3.1 读模型 EditMode 回归：选中实例后图快照非空、`SelectNode` 语义、未选中/清空时空快照、悬空连线草稿报校验错误（`AlgorithmReadModelEditModeTests` 15/15 全绿，含 3 个新图快照用例）
- [x] 3.2 普通编译 0 错误 0 警告、Console 0 错误、`openspec validate b20-algorithm-editor-graph-wiring --strict` 通过、`MachineDeploymentRuntimeEditModeTests` 5/5、框架纯度 11 项为存量债务非本变更引入
- [x] 3.3 更新本 tasks/design 收口，回传结果
