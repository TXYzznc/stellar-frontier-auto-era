# B49 离线领域推进与回归报告

## Why

离线调度器本身不能证明生产、机器行为、施工制造和任务结果正确。必须按第一版白名单接真实领域下一事件，并将结算身份、阻塞与报告持久化，避免重复领奖或离线穿过缺电/满载等限制。

本变更依据2026-10-08用户确认的[逐变更决策摘要](../../../Docs/Development/ImprovementPlanDecisionSummary-20261008.md)与DEC-204/205建立。当前授权为方案文档，未开始实施。

## What Changes

- 接资源生产/能源/机器任务行为和施工制造/任务成长的真实离线事件。
- 导航使用有效路径长度/速度计算抵达与中途进度，不逐帧模拟场景。
- 持久化一次性结算报告、阻塞聚合与离线进度；仅结算完成后开放世界操作。
- 继承现有玩法与数量规则，仅补齐本批运行合同；验收子集不等于原任务或里程碑全量完成。

## Capabilities

### New Capabilities

- `offline-domain-settlement-report`：离线领域推进与回归报告的增量集成与验收合同。

### Modified Capabilities

无。本能力尚未在 `openspec/specs/` 建立同名正式规格；不修改已有主规格。既有change作为前置设计，差异与替代范围见design.md，不自动归档或同步其未验收能力。

## Impact

- 原始任务映射：`P7-008`、`P7-009`、`P7-010`、`P7-012`。表示本批补齐/优化范围，不改写用户工作簿状态。
- 本计划依赖：[B48 跨域快照与离线调度](../b48-p7005-p7006-p7007-offline-event-foundation/proposal.md)
- 外部前置：P7-005、P7-006完整领域段和G5/G6证据；第一版农业/水泵/运输/建造制造/任务成长全部白名单提供者以及b07离线所有权接入；正式菜单入口B47、G7-001原始全部前置。
- 预计实现触及：
  - `Assets/Game/Scripts/AutoEra/Save/`
  - `Assets/Game/Scripts/AutoEra/World/Time/`
  - `Assets/Game/Scripts/AutoEra/Machines/MachineNavigation.cs`
  - `Assets/Game/Scripts/AutoEra/World/Region/RegionNavigation.cs`
  - `Assets/Game/Scripts/AutoEra/UI/ProgressReportForm.cs`
  - `Assets/Game/Scripts/AutoEra/UI/RecordReaderForm.cs`
  - `Assets/Game/Scripts/AutoEra/UI/BaseCommandHubForm.cs`
  - `Assets/Game/Scripts/AutoEra/UI/Integration/`
  - `对应生产/能源/施工制造/任务领域的离线适配（依赖交付后列精确文件）`
  - `Docs/Development/UI-PrefabLayouts/ProgressReportForm.contract.json`
- 验证范围：新增规格中的场景、领域测试与正式宿主集成；公共门与实施顺序见[总计划](../../../Docs/Development/ImprovementPlan-20261008.md)。
- 明确不做：不离线替玩家购买/出售、改装、选择配方、部署或领取普通可选择奖励；不新增实际探索/战斗/维修；不把待外部领域用简单乘法估算补齐。
- 本次不修改产品代码、资源、任务表或Git索引；未来实施仍需授权与依赖就绪。无新框架、第三方依赖或程序集调整，禁止本批修改ScriptsBuiltin。
