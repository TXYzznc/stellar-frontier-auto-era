## Context

`AlgorithmRuntime` 在每次 `Pump` 结束时产出一条 `AlgorithmRunRecord`（`RunId`/`Error`/`FailedNode`/`Cost`/`CopyTrigger()`/`CopyPath()`），并以队列 `History()` 暴露。B20 的 `MachineAlgorithmReadModel` 已从 `runtime.Instances`（`AlgorithmInstanceService`）读草稿图，但 `Entry.Runtime`（底层 `AlgorithmRuntime`）与它的历史对读模型不可见——`AlgorithmInstanceService` 没有暴露历史的只读入口。

本变更只补「读历史」这一半：给实例服务开一个只读历史出口，读模型据此标记节点执行状态。写路径（创建/应用实例）与历史记录的**生产触发**（机器运行时 Pump）都不是本变更，测试里由夹具直接 Enqueue+Pump 驱动。

## Goals / Non-Goals

**Goals:**

- `AlgorithmInstanceService.ReadHistory(id)` 返回该实例运行时最近运行记录快照。
- 读模型把最近一次运行的执行路径/失败节点映射为每个图节点的「已执行／失败／未执行」状态，并在快照暴露运行摘要。
- 编辑器在检视器/节点栏展示诊断摘要与节点状态。

**Non-Goals:**

- 不做暂停逐节点执行/断点/单步/重放/跨运行选择（设计文档第一版明确不做）。
- 不做逐值「当时值 vs 当前值」展示；不做写路径与绑定重绑。

## Decisions

1. **历史出口放在实例服务**：新增 `AlgorithmInstanceService.ReadHistory(ulong id)`，返回 `_entries[id].Runtime.History()` 的快照（无实例/无运行时返回空数组）。这是界面观察运行历史的唯一入口，与 `ListInstances`/`ReadDraft` 同层，不绕过实例服务直取运行时。
2. **只取最近一条运行记录**：`History()` 是 FIFO 队列（最近 50 条上限）。诊断读路径只标记**最近一次**运行（队列尾部），避免把多轮历史混在一张静态图上；跨运行选择留待后续。
3. **节点诊断状态用枚举**：`UiAlgorithmNodeRow` 增加 `Diagnostic`（`None/Executed/Failed`）。最近运行的 `CopyPath()` 命中即 `Executed`，`FailedNode` 命中即 `Failed`（失败优先），否则 `None`。状态并入节点 `Status` 文本，供列表行直接展示，不新增预制体字段。
4. **快照增加运行摘要**：新增 `UiAlgorithmRunRow`（`RunId`/`Error`/`FailedNode`/`Cost`）与 `AlgorithmDomainSnapshot.LatestRun`（无历史为 null）。编辑器检视器用 `RenderDetailRows` 追加摘要字段。
5. **只读不变式不变**：`ReadHistory` 返回快照，读模型不触发 Pump、不改运行时状态；模板域/不可用域的 `LatestRun` 恒空。

## Risks / Trade-offs

- **历史为空是常态**：生产里没有已应用实例或实例尚未被唤醒时，`LatestRun` 为 null——编辑器要如实显示「暂无运行记录」，不得伪造「无问题」。这与 B20 问题栏「校验通过」的语义区分开：一个是静态校验、一个是运行时历史。
- **队列尾部即最近**：`History()` 是 `Queue`，`ToArray()` 保持入队顺序，取末元素即最近运行；若后续改为环形缓冲需同步此处取尾逻辑。
