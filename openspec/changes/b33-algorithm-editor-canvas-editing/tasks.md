## 1. Read-model and command layer

- [x] 1.1 Add `AlgorithmDocument.RemoveNode(ulong id)` (soft-delete node + remove all edges referencing it, one revision bump) and service `DeleteNode(instanceId, expectedRevision, nodeId)` with revision protection
  - 交付：`AlgorithmDocument.RemoveNode` 软删节点并级联移除全部关联边（单次 Revision++）；`AlgorithmInstanceService.DeleteNode` 走标准守卫链（disposed/entry/revision/Applying）；测试 `DeleteNode_CascadesEdges_BumpsRevision_StaleRevisionRejects` 覆盖级联、修订保护、重复删除与不存在节点拒绝。
- [x] 1.2 Extend `IAlgorithmReadModel` + machine/template/unavailable implementations with `DeleteNode`; build node kind catalog rows (kind/label/category/cost/ports) in the snapshot, identical across domains
  - 交付：三域接口实现齐备（机器域转发服务、库/不可用域恒 false）；`AlgorithmNodeLibrary` 静态目录 20 种类（中文显示名+分类），快照构造缺省携带、三域一致（测试 `NodeKindLibrary_CatalogRows_IdenticalAcrossDomains_CoverEveryKind` 钉住）。
- [x] 1.3 Add per-node input/output port rows (port id/name/type/connected state) to `UiAlgorithmNodeRow` or a companion snapshot list
  - 交付：`UiAlgorithmPortRow`（Key/TypeLabel/Connected）+ `BuildPortRows`（按 `AlgorithmCatalog` 端口定义 + 草稿边扫描连接态）；`UiAlgorithmNodeRow` 可选构造参数注入 InputPorts/OutputPorts；测试 `GraphNodeRows_ExposePortRowsWithConnectedState` 钉住已连/未连/无端口三种形态。

## 2. Contract, fields, prefab

- [x] 2.1 Append bindings to `AlgorithmEditorForm.contract.json`: `_algorithmEditorAddButton`, `_algorithmUndoButton`, `_algorithmRedoButton`, `_algorithmDeleteSelectedButton` (under `Panel_AlgorithmEditorToolbar`)
  - 交付：契约新增 `Grp_AlgorithmDraftTools` 子树（横排 3 键、撤销/重做 interactable=false）与 4 条绑定（合计 36 条），JSON 结构与树路径核验通过。
- [x] 2.2 Regenerate `AlgorithmEditorForm.Fields.cs` via `tools/ui_contract_to_form_script.py`; revert EOL-only churn in unrelated generated files
  - 交付：仅 `AlgorithmEditorForm.Fields.cs` 保留改动（+4 字段），其余 6 个无关 `.Fields.cs` 的 CRLF 噪音已 `git checkout` 还原。
- [x] 2.3 UnitySkills: build `Grp_AlgorithmDraftTools` (+3 buttons styled from existing action buttons, 撤销/重做 interactable=false) in the prefab; bind all missing serialized fields (apply button, node search, graph content, graph element template, add, undo, redo, delete-selected); prefab_apply + verify
  - 交付：临时实例法建组+复制现有按钮组件（Image/Button/LayoutElement/TMP 标签），绑定 8 个序列化字段（含 b26/b29/b30 遗留漏绑的 4 个），prefab_apply 回写、文本核验 8 个 fileID 引用与节点名齐全、无 MergeTemp 残留；EditMode `AlgorithmEditorFormBindingsEditModeTests` 3/3 绿作长期围栏。

## 3. Form interaction

- [x] 3.1 Node panel → node library (catalog rows + search filter + category); row click / add button → CreateNode with grid-stepped layout position (only when Ready + instance selected)
  - 交付：`RenderNodeLibrary` 按 `_nodeFilter` 过滤目录行（非 Ready 传 null 回调禁点）；行点击/「添加」按钮都走 `TryCreateNode`（列距 300/行距 220/8 列换行的网格步进落位，仅 Ready+选中实例）。
- [x] 3.2 Canvas node element: render input/output port rows from templates; two-step connect (output click → pending; input click → Connect; same/other output click → cancel/replace; failure keeps pending + problem panel note)
  - 交付：`RenderPortList` 从 `Item_AlgorithmInputPortTemplate/OutputPortTemplate` 实例化端口行（`*` 待连标记 /「已连」状态，ASCII 规避 SIMHEI 缺字形）；两步连线含取消/换源路径；失败保留待连源并局部刷新源节点端口行（`RefreshPortsForNode` 不重建整个画布）。
- [x] 3.3 Edge element at midpoint + select; node select via `Btn_AlgorithmNodeSelect`; "删除选中" → edge Disconnect or node DeleteNode; delete button enabled exactly when selection exists and domain Ready
  - 交付：边元素按两端节点中点定位渲染（模板 `Grp_AlgorithmEdge` 模式复用）；删除按钮可用性＝Ready 且（选中边或选中仍在图中的节点）；删除前先清本地选择再发命令（规避 service.Changed 同步重入 Render 的时序坑，PlayMode 测试抓到并修复）。
- [x] 3.4 Inspector/problems panels stay in sync with the new mutations (revision, edge/node counts, port connected state) through the existing Changed event re-render
  - 交付：全部命令经读模型转发，`OnServiceChanged → Publish → Render` 链路复用；检视器/问题栏读同一份重建快照（BuildGraph 过滤软删节点），PlayMode 端到端测试全程经真实 UI 断言草稿状态。

## 4. Tests and verification

- [x] 4.1 EditMode: RemoveNode/DeleteNode cascade + revision protection; node library catalog rows (all domains); port rows connected state; CreateNode layout grid
  - 结果：`AlgorithmInstanceEditModeTests` 13/13、`AlgorithmReadModelEditModeTests` 32/32（新增 DeleteNode 转发/目录三域一致/端口行连接态三组用例）。
- [x] 4.2 EditMode/PlayMode form tests: library render + search; add node via row and via button; two-step connect success/incompatible/occupied/cancel; edge select + delete; node select + delete; delete button enable/disable states; prefab bindings non-null
  - 结果：`AlgorithmEditorFormBindingsEditModeTests` 3/3（绑定非空+工具组+模板双模式）；`AlgorithmEditorCanvasPlayModeTests` 1/1（真实预制体上库渲染/搜索/行添加/按钮添加/连线成功/取消/占用拒绝/选边删除/选节点删除/删除按钮状态机全链路）。
- [x] 4.3 Compile check, `openspec validate --strict`, run full algorithm test set, update task table P3-010/P3-011 and dispatch queue
  - 结果：编译 0 错误；`AlgorithmGraphEditModeTests` 4/4、`AlgorithmExecutionEditModeTests` 8/8 回归绿；validate --strict 通过；任务表 P3-010/P3-011 → 已完成。
