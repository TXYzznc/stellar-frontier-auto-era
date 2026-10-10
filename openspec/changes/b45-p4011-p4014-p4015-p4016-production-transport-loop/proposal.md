# B45 运输与生产观察

## Why

算法、生产、转移和仓库分别可测仍不足以证明机器能持续把产物送入库。需要明确运输任务防重、途中货物责任、异常恢复与玩家观察链，形成两类资源的可玩子集。

本变更依据2026-10-08用户确认的[逐变更决策摘要](../../../Docs/Development/ImprovementPlanDecisionSummary-20261008.md)与DEC-204/205建立。用户随后明确授权实施；本批增量已完成，见[evidence/implementation-20261008.md](evidence/implementation-20261008.md)。

## What Changes

- 固定路线任务使用已有任务/货舱查询防重复，串联真实移动、装载、卸载与入库结果。
- 保留取消/失败后的货物及未解决责任，恢复优先完成既有交付。
- 仓库与林木/矿脉现场页接真实读模型，动作反馈和诊断消费真实事件。
- 继承现有玩法与数量规则，仅补齐本批运行合同；验收子集不等于原任务或里程碑全量完成。

## Capabilities

### New Capabilities

- `production-transport-observation`：运输与生产观察的增量集成与验收合同。

### Modified Capabilities

无。本能力尚未在 `openspec/specs/` 建立同名正式规格；不修改已有主规格。既有change作为前置设计，差异与替代范围见design.md，不自动归档或同步其未验收能力。

## Impact

- 原始任务映射：`P4-011`、`P4-014`、`P4-015`、`P4-016`。表示本批补齐/优化范围，不改写用户工作簿状态。
- 本计划依赖：[B42 算法工作台可用性与闭环](../b42-p3010-p3011-p3013-p3014-algorithm-workbench-closure/proposal.md)；[B43 资源权威与入库结算](../b43-p4001-p4010-p4012-p4013-resource-settlement/proposal.md)；[B44 林木与矿脉真实作业](../b44-p4002-p4003-p4009-p4017-forest-mineral-production/proposal.md)
- 外部前置：P2-008、P1-003、P0-008、P1-007、P2-011；P4-014/P4-016全量完成仍等待水泵、农业行为；G4-001仍等待全部原始前置及P4-020/b07；正常菜单新进度综合验证等待B47及正式初始资源配置；不能用GM冒充正式体验。
- 预计实现触及：
  - `Assets/Game/Scripts/AutoEra/Machines/MachineScheduling.cs`
  - `Assets/Game/Scripts/AutoEra/Machines/MachineNavigation.cs`
  - `Assets/Game/Scripts/AutoEra/Algorithms/InitialAlgorithmTemplates.cs`
  - `Assets/Game/Scripts/AutoEra/UI/WarehouseForm.cs`
  - `Assets/Game/Scripts/AutoEra/UI/FieldHudDetailForm.cs`
  - `Assets/Game/Scripts/AutoEra/UI/Integration/`
  - `Assets/Game/Scripts/AutoEra/Motion/MotionWorkBridge.cs`
  - `Docs/Development/UI-PrefabLayouts/（仅WarehouseForm与本批现场页契约）`
  - `Assets/Game/Prefabs/UI/（仅对应仓库与现场页）`
- 验证范围：新增规格中的场景、领域测试与正式宿主集成；公共门与实施顺序见[总计划](../../../Docs/Development/ImprovementPlan-20261008.md)。
- 明确不做：不把两类资源纵切等同于完整G4；不新增复杂物流调度、不绕过b07所有权，不实现全部农业/水域面板，不提供跨仓库瞬移按钮。
- 本批在已授权范围内修改产品代码与资源，未写任务表或Git索引；完整外部依赖仍保留。无新框架、第三方依赖或程序集调整，禁止本批修改ScriptsBuiltin。
