# B43 资源权威与入库结算

## Why

仓库与资源点已有表现资产，但没有统一资源缓存、预留和入库权威。若先各自实现采集或运输，容易在并发、取消与存读档时产生重复资源，且会与b07物理货物唯一归属冲突。

本变更依据2026-10-08用户确认的[逐变更决策摘要](../../../Docs/Development/ImprovementPlanDecisionSummary-20261008.md)与DEC-204/205建立。用户随后授权实施，并明确批准补齐b07最小唯一归属事务核心，范围见design D3。

## What Changes

- 建立资源缓存、可用/预留数量、统一转移与实际提交结果。
- 建立机器货舱、来源缓存和仓库的原子交接；复用b07所有权边界，不引入第二套货物拥有者。
- 通用资源入库转换全局余额，实体物品保留仓库身份与容量，事件记录责任与实际数量。
- 继承现有玩法与数量规则，仅补齐本批运行合同；验收子集不等于原任务或里程碑全量完成。

## Capabilities

### New Capabilities

- `resource-transfer-settlement`：资源权威与入库结算的增量集成与验收合同。

### Modified Capabilities

无。本能力尚未在 `openspec/specs/` 建立同名正式规格；不修改已有主规格。既有change作为前置设计，差异与替代范围见design.md，不自动归档或同步其未验收能力。

## Impact

- 原始任务映射：`P4-001`、`P4-010`、`P4-012`、`P4-013`。表示本批补齐/优化范围，不改写用户工作簿状态。
- 本计划依赖：[B40 算法正式运行驱动](../b40-p3003-p3005-p3007-p3008-algorithm-runtime-drive/proposal.md)；[B41 传感器与效应器生命周期](../b41-p2005-p2007-p3006-hardware-runtime-wiring/proposal.md)
- 外部前置：G3-001通过后方进入P4实施；P1-001、P2-001、P2-002、P0-006及b07 physical-cargo-ownership合同；b07运行能力未交付时列为生产接入依赖。
- 预计实现触及：
  - `Assets/Game/Scripts/AutoEra/Machines/MachineCargo.cs`
  - `Assets/Game/Scripts/AutoEra/World/AutoEraWorldSession.cs`
  - `Assets/Game/Scripts/AutoEra/World/Region/`
  - `Assets/Game/Scripts/AutoEra/Resources/（拟新增领域目录）`
  - `Assets/Game/Scripts/AutoEra/Buildings/（按本批仓库能力新增）`
  - `Assets/Game/Scripts/AutoEra/Events/`
- 验证范围：新增规格中的场景、领域测试与正式宿主集成；公共门与实施顺序见[总计划](../../../Docs/Development/ImprovementPlan-20261008.md)。
- 明确不做：不实现传送带运动/出料队列/建造，不实现商品购买、升级或完整农业规则；缓存基础可扩展但本批不宣称四类资源点均完成。
- 实施只修改本批产品接入、初始仓库Prefab与对应验证/文档；任务表与Git索引保持只读。无新框架、第三方依赖或程序集调整，禁止本批修改ScriptsBuiltin。
