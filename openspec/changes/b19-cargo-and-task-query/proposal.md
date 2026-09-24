# Proposal: 本机货舱读取与任务查询节点

## Why

固定路线运输模板（DEC-123）是五套第一版系统模板的最后一套，但它的录入依赖两个算法运行时尚未具备的能力：本机货舱读取（DEC-111 规定本地算法可直接读取本机货舱，不需传感器）和任务查询防重（DEC-123 要求"绑定到自身提交节点的任务查询节点显式防重"）。当前代码库没有机器货舱数据结构，`AlgorithmMachineAdapter` 对 QueryTask/CancelTask 只发布 `rejected` 桩，导致第五套模板无法录入。

## What Changes

- 新增机器货舱契约 `MachineCargo`（统一容量单位、剩余空间、物品明细、变化事件），挂接到机器执行上下文。
- 新增货舱读取数据节点（`AlgorithmNodeKind.Cargo`），本地直接读取本机货舱容量/剩余空间/物品明细，不需要传感器绑定。
- 完整实现任务查询节点 QueryTask 的数据返回（`found` 布尔 + `task` 对象引用），供运输模板显式防重。
- 录入固定路线运输模板（DEC-123），完成五套系统模板的收口。
- 效应器动作参数沿用既有 `count`/`item` 契约，无破坏性变更。

## Capabilities

### New Capabilities

- `machine-cargo`: 本机货舱契约——统一容量单位的容量、已用/剩余空间、物品明细与变化事件。
- `cargo-read-node`: 货舱读取数据节点——算法图本地读取本机货舱字段，不依赖传感器绑定。
- `task-query-node`: 任务查询节点——按任务名/身份查询本机任务队列，向数据端口返回 found 与 task。

### Modified Capabilities

<!-- 无既有 spec 需求变更 -->

## Impact

- 受影响代码：`AutoEra.Machines`（机器货舱契约与上下文）、`AutoEra.Algorithms`（`AlgorithmDocument` 节点类型、`AlgorithmCatalog` 端口/成本、`AlgorithmEvaluation` 求值、`AlgorithmMachineAdapter` 提交、`InitialAlgorithmTemplates` 固定运输模板）。
- 新增节点类型：`AlgorithmNodeKind.Cargo`；QueryTask 从事件桩升级为数据返回。
- 依赖：DEC-108/111/113/123 与 DEC-117 节点成本口径。
