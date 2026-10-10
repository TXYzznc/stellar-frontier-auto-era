# B47 安全快照与存读档入口

## Why

三槽文件服务已有校验、备份与恢复，但实际新建/继续/保存退出尚未接入世界状态。机器运行时被销毁再重建为空闲不能满足保存行为责任、算法计时及待应用请求的要求。

本变更依据2026-10-08用户确认的[逐变更决策摘要](../../../Docs/Development/ImprovementPlanDecisionSummary-20261008.md)与DEC-204/205建立。当前授权为方案文档，未开始实施。

## What Changes

- 用领域DTO保存世界、对象、机器、任务/行为和算法状态，不序列化运行服务与Unity对象。
- 建立安全快照、单槽串行异步写入、60秒与关键事件保存请求合并。
- 新建/继续/恢复/保存退出连接真实世界，采用先验证候选再切换会话的恢复方式。
- 继承现有玩法与数量规则，仅补齐本批运行合同；验收子集不等于原任务或里程碑全量完成。

## Capabilities

### New Capabilities

- `world-safe-snapshot-flow`：安全快照与存读档入口的增量集成与验收合同。

### Modified Capabilities

无。本能力尚未在 `openspec/specs/` 建立同名正式规格；不修改已有主规格。既有change作为前置设计，差异与替代范围见design.md，不自动归档或同步其未验收能力。

## Impact

- 原始任务映射：`P7-002`、`P7-003`、`P7-004`、`P7-011`。表示本批补齐/优化范围，不改写用户工作簿状态。
- 本计划依赖：[B40 算法正式运行驱动](../b40-p3003-p3005-p3007-p3008-algorithm-runtime-drive/proposal.md)；[B41 传感器与效应器生命周期](../b41-p2005-p2007-p3006-hardware-runtime-wiring/proposal.md)；[B42 算法工作台可用性与闭环](../b42-p3010-p3011-p3013-p3014-algorithm-workbench-closure/proposal.md)
- 外部前置：P7-001、G2-001、G3-001、P1-001及现有save-rolling-backups-and-checksum；完整正常新进度初始化引用既有第一版开局配置，不通过GM注入；未接入的领域由B48/B49完成。
- 预计实现触及：
  - `Assets/Game/Scripts/AutoEra/Save/`
  - `Assets/Game/Scripts/AutoEra/Application/AutoEraApplicationContext.cs`
  - `Assets/Game/Scripts/AutoEra/World/AutoEraWorldSessionFactory.cs`
  - `Assets/Game/Scripts/AutoEra/World/Region/RegionMachineRuntimeRegistry.cs`
  - `Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmInstanceService.cs`
  - `Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmMemorySnapshot.cs`
  - `Assets/Game/Scripts/AutoEra/Machines/MachineRosterSnapshot.cs`
  - `Assets/Game/Scripts/AutoEra/Procedures/`
  - `Assets/Game/Scripts/AutoEra/UI/SaveSlotsForm.cs`
  - `Assets/Game/Scripts/AutoEra/UI/SaveRecoveryForm.cs`
  - `Assets/Game/Scripts/AutoEra/UI/ExitFlowForm.cs`
- 验证范围：新增规格中的场景、领域测试与正式宿主集成；公共门与实施顺序见[总计划](../../../Docs/Development/ImprovementPlan-20261008.md)。
- 明确不做：不新增云存档、任意手动回档或跨设备同步；不把尚无领域实现的生产/经济/成长伪造为已保存；B48/B49未完成前不得将整套存档离线标为G7通过。
- 本次不修改产品代码、资源、任务表或Git索引；未来实施仍需授权与依赖就绪。无新框架、第三方依赖或程序集调整，禁止本批修改ScriptsBuiltin。

