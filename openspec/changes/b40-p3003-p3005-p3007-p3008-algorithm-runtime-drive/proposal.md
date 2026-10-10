# B40 算法正式运行驱动

## Why

正式场景已创建算法实例服务，却未持续驱动实例服务与命令适配器；UI激活会先挂接适配器，再尝试提交实例，失败时存在半挂接风险。已有集成测试自行Pump，不能证明产品入口形成运行闭环。

本变更依据2026-10-08用户确认的[逐变更决策摘要](../../../Docs/Development/ImprovementPlanDecisionSummary-20261008.md)与DEC-204/205建立。当前授权为方案文档，未开始实施。

## What Changes

- 给机器运行时提供稳定排序的推进入口，实例服务与适配器每业务步只推进一次。
- 把首次激活和应用请求交给机器级操作入口，读模型委托调用，生命周期不依赖Form。
- 明确首次启动、恢复、参数应用、结构应用、安全点及失败回滚；补正式宿主集成测试。
- 继承现有玩法与数量规则，仅补齐本批运行合同；验收子集不等于原任务或里程碑全量完成。

## Capabilities

### New Capabilities

- `production-algorithm-driving`：算法正式运行驱动的增量集成与验收合同。

### Modified Capabilities

无。本能力尚未在 `openspec/specs/` 建立同名正式规格；不修改已有主规格。既有change作为前置设计，差异与替代范围见design.md，不自动归档或同步其未验收能力。

## Impact

- 原始任务映射：`P3-003`、`P3-005`、`P3-007`、`P3-008`。表示本批补齐/优化范围，不改写用户工作簿状态。
- 本计划依赖：无本计划内强制前置；仍须核对下列原任务/既有合同。
- 外部前置：G0-001/G1-001/G2-001的可追溯通过证据；P3-001、P3-002、P2-006、P2-008及现有b15/b17/b25/b26合同。
- 预计实现触及：
  - `Assets/Game/Scripts/AutoEra/World/Region/InitialRegionScene.cs`
  - `Assets/Game/Scripts/AutoEra/World/Region/RegionMachineRuntime.cs`
  - `Assets/Game/Scripts/AutoEra/World/Region/RegionMachineRuntimeRegistry.cs`
  - `Assets/Game/Scripts/AutoEra/Algorithms/`
  - `Assets/Game/Scripts/AutoEra/UI/Integration/AlgorithmReadModel.cs`
- 验证范围：新增规格中的场景、领域测试与正式宿主集成；公共门与实施顺序见[总计划](../../../Docs/Development/ImprovementPlan-20261008.md)。
- 明确不做：不新增节点语言、五套模板规则、算法市场或新调度框架；硬件提供者由B41建立；磁盘恢复由B47接入；不把现有单适配器限制升级为新的玩法规则。
- 本次不修改产品代码、资源、任务表或Git索引；未来实施仍需授权与依赖就绪。无新框架、第三方依赖或程序集调整，禁止本批修改ScriptsBuiltin。

