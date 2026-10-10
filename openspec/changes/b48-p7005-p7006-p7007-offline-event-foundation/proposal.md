# B48 跨域快照与离线调度

## Why

完整存档需要跨资源、经济、能源、任务成长保持同一提交边界；离线推进必须复用这些领域规则而非按离线时长直接乘收益。已有世界时钟和排序模型可复用，但尚缺持久化事件队列及跨域快照适配。

本变更依据2026-10-08用户确认的[逐变更决策摘要](../../../Docs/Development/ImprovementPlanDecisionSummary-20261008.md)与DEC-204/205建立。当前授权为方案文档，未开始实施。

## What Changes

- 将生产/余额/能源/任务成长适配到B47不可变快照与恢复事务。
- 建立有效事件优先队列、稳定同刻排序和可恢复离线检查点。
- 明确事件预算、取消续算、UTC目标时刻与奖励/每日补给防重边界，提供领域下一事件接口。
- 继承现有玩法与数量规则，仅补齐本批运行合同；验收子集不等于原任务或里程碑全量完成。

## Capabilities

### New Capabilities

- `offline-event-foundation`：跨域快照与离线调度的增量集成与验收合同。

### Modified Capabilities

无。本能力尚未在 `openspec/specs/` 建立同名正式规格；不修改已有主规格。既有change作为前置设计，差异与替代范围见design.md，不自动归档或同步其未验收能力。

## Impact

- 原始任务映射：`P7-005`、`P7-006`、`P7-007`。表示本批补齐/优化范围，不改写用户工作簿状态。
- 本计划依赖：[B43 资源权威与入库结算](../b43-p4001-p4010-p4012-p4013-resource-settlement/proposal.md)；[B44 林木与矿脉真实作业](../b44-p4002-p4003-p4009-p4017-forest-mineral-production/proposal.md)；[B45 运输与生产观察](../b45-p4011-p4014-p4015-p4016-production-transport-loop/proposal.md)；[B46 电网热路径优化](../b46-p5006-p5009-p5010-energy-hotpath/proposal.md)；[B47 安全快照与存读档入口](../b47-p7002-p7003-p7004-p7011-world-save-flow/proposal.md)
- 外部前置：P0-005、G5-001、G6-001、P6-007、P5-013；农业/水泵/经济/施工制造/任务成长及b07实现与领域保存合同；缺失时仅允许调度器与接口验证。
- 预计实现触及：
  - `Assets/Game/Scripts/AutoEra/Save/`
  - `Assets/Game/Scripts/AutoEra/World/Time/`
  - `Assets/Game/Scripts/AutoEra/World/AutoEraWorldSession.cs`
  - `Assets/Game/Scripts/AutoEra/Resources/（B43建立后）`
  - `Assets/Game/Scripts/AutoEra/Energy/`
  - `Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmMemorySnapshot.cs`
  - `对应经济/施工制造/任务成长领域适配文件（领域交付后精确列入范围）`
- 验证范围：新增规格中的场景、领域测试与正式宿主集成；公共门与实施顺序见[总计划](../../../Docs/Development/ImprovementPlan-20261008.md)。
- 明确不做：不实现经济/成长/建筑本身来绕开G5/G6；不离线逐帧跑物理，不用闭式平均收益替代有阻塞的模拟；B49负责完整领域事件和报告。
- 本次不修改产品代码、资源、任务表或Git索引；未来实施仍需授权与依赖就绪。无新框架、第三方依赖或程序集调整，禁止本批修改ScriptsBuiltin。

