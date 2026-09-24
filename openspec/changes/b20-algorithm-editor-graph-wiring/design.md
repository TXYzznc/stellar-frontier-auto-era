## Context

B17 把 `AlgorithmInstanceService` 接入区域机器运行路径，`MachineAlgorithmReadModel` 已能从 `runtime.Instances` 读出实例列表（版本三元组/逻辑算力/应用请求），`AlgorithmEditorForm` 据此列出实例并展示机器检视信息。但读模型只暴露「有哪些实例」，没有暴露选中实例的**图结构**（`AlgorithmDocument.Nodes/Edges/Bindings`）与**校验结果**（`AlgorithmValidator.TryCompile` 的 `AlgorithmIssue` 列表），于是编辑器节点栏仍是实例列表、检视器仍是机器详情、问题栏仍写死「诊断尚未运行」。

本变更只补「读」这一半：把选中实例的草稿图与校验问题映射成快照，让编辑器真实展示。写路径（Apply/ConfirmWarnings）、诊断运行历史、拖拽连线交互均不在本变更。

## Goals / Non-Goals

**Goals:**

- 读模型为选中实例暴露图快照：图节点、图连线、校验问题，及选中节点的详情字段。
- `AlgorithmEditorForm` 用真实数据渲染节点栏（图节点、可点选）、检视器（选中节点属性）、问题栏（校验问题）。
- 只读，不改 `AlgorithmInstanceService`/`AlgorithmValidator`/`AlgorithmRuntime`。

**Non-Goals:**

- 不做节点画布拖拽/连线、节点库搜索、应用写路径、诊断运行历史/路径高亮、绑定重绑。
- 不做节点属性编辑（检视器只展示，不写回草稿）。

## Decisions

1. **图快照挂在既有快照类型上**：`AlgorithmDomainSnapshot` 新增 `GraphNodes`/`GraphEdges`/`Issues`/`NodeDetail`/`SelectedNode`。模板域（`TemplateAlgorithmReadModel`）与不可用域（`UnavailableAlgorithmReadModel`）这些字段恒为空，避免为模板库页引入图语义。
2. **选中实例即读草稿**：`MachineAlgorithmReadModel.Publish()` 在 `SelectedInstanceIndex` 有效时调用 `runtime.Instances.ReadDraft(selectedId)` 取草稿副本（`ReadDraft` 返回 `Copy()`，不触碰服务内部状态），映射节点/连线；再以 `AlgorithmValidator.TryCompile(draft, int.MaxValue, out _, out issues, false)` 编译，`false`＝实例完整校验（含绑定），`int.MaxValue`＝不因算力容量报错，问题栏只呈现结构正确性（类型不匹配、同步环、缺失绑定、不可达等）。
3. **节点选中独立于实例选中**：`IAlgorithmReadModel` 新增 `bool SelectNode(ulong nodeId)`，仅在已选中实例且该节点存在于草稿时成功；成功后重建 `NodeDetail`（Kind/Operator/值类型/默认值/绑定键/状态键/字段/动作），失败返回 `false`。清空实例选中时同时清空节点选中。
4. **行类型映射**：新增只读行结构 `UiAlgorithmNodeRow`（`Label`＝Kind·Operator `#Id`，`Status`＝值类型/绑定键/字段摘要）、`UiAlgorithmEdgeRow`（`Label`＝`From → To`，`Status`＝输出→输入端口）、`UiAlgorithmIssueRow`（`Label`＝代码＋节点，`Status`＝严重度＋消息），三者与既有 `UiAlgorithmTemplateRow` 一样是纯展示载体，不含行为。
5. **编辑器渲染复用既有行 API**：节点栏 `RenderListRows` 绑定 `GraphNodes`（点击回调 `SelectNode`），检视器 `RenderDetailRows` 绑定 `NodeDetail`（无选中节点时回退实例图摘要），问题栏 `RenderDetailRows`/`RenderListRows` 绑定 `Issues`。不改界面预制体与 `.Fields.cs`。

## Risks / Trade-offs

- **编译成本**：每次 `Publish` 对选中实例全量 `TryCompile`，实例规模有限（第一版一台机器少量实例），可接受；后续若实例/节点量大再考虑缓存按 `SavedDraftRevision` 失效。
- **问题栏语义**：`int.MaxValue` 容量刻意不报算力不足——算力占用已在检视器 `Detail` 展示，避免同一信息两处出现造成误导。
- **节点详情只读**：本变更展示而不写回，避免把「看图」和「改图」耦合进一个读模型；写路径届时另建命令模型。
