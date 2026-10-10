# B46 电网热路径优化

## Why

EnergyGrid在逐帧路径构建引用类型Plan，缺电时反复扫描负载，停机快照复制数组。优化必须证明结算、停机优先级与历史快照语义不变，而非仅减少代码或观察一张低帧率截图。

本变更依据2026-10-08用户确认的[逐变更决策摘要](../../../Docs/Development/ImprovementPlanDecisionSummary-20261008.md)与DEC-204/205建立。当前授权为方案文档，未开始实施。

## What Changes

- 消除稳定电网推进中的Plan及重复停机快照分配，缓存可复用计算数据。
- 以参与者/优先级变化重建排序，避免每停一个负载全表选一次受害者。
- 增加参考结算对照、历史快照不可变与10/50/100机器负载基准。
- 继承现有玩法与数量规则，仅补齐本批运行合同；验收子集不等于原任务或里程碑全量完成。

## Capabilities

### New Capabilities

- `energy-hotpath-equivalence`：电网热路径优化的增量集成与验收合同。

### Modified Capabilities

无。本能力尚未在 `openspec/specs/` 建立同名正式规格；不修改已有主规格。既有change作为前置设计，差异与替代范围见design.md，不自动归档或同步其未验收能力。

## Impact

- 原始任务映射：`P5-006`、`P5-009`、`P5-010`。表示本批补齐/优化范围，不改写用户工作簿状态。
- 本计划依赖：无本计划内强制前置；仍须核对下列原任务/既有合同。
- 外部前置：P0-005、P1-001、P2-003、P5-007、P5-008运行合同及证据；既有energy-grid-settlement、machine-energy-load、region-energy-wiring；本批不代替G5商品/升级/配置前置。
- 预计实现触及：
  - `Assets/Game/Scripts/AutoEra/Energy/EnergyGrid.cs`
  - `Assets/Game/Scripts/AutoEra/Energy/MachineEnergyConsumer.cs`
  - `Assets/Game/Scripts/AutoEra/World/Region/RegionEnergyService.cs`
  - `Assets/Game/Scripts/AutoEra/World/Region/InitialRegionScene.cs`
  - `Assets/Game/Scripts/AutoEra/UI/Integration/EnergyReadModel.cs`
- 验证范围：新增规格中的场景、领域测试与正式宿主集成；公共门与实施顺序见[总计划](../../../Docs/Development/ImprovementPlan-20261008.md)。
- 明确不做：不改变能源玩法、容量、优先级、昼夜数值或功率/电量单位；不以全局低频Tick替代结算精度，不新建ECS或并行调度，不宣称完整G5通过。
- 本次不修改产品代码、资源、任务表或Git索引；未来实施仍需授权与依赖就绪。无新框架、第三方依赖或程序集调整，禁止本批修改ScriptsBuiltin。
