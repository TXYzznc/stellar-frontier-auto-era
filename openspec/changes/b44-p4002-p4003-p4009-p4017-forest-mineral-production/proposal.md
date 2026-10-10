# B44 林木与矿脉真实作业

## Why

两类资源点已有场景对象和外观，但尚无驱动真实产出与耗尽的生产权威。用动画或静态公开数值验证算法会掩盖资源生产链未接通。

本变更依据2026-10-08用户确认的[逐变更决策摘要](../../../Docs/Development/ImprovementPlanDecisionSummary-20261008.md)与DEC-204/205建立。用户已授权实施；实际增量交付见evidence/implementation-20261008.md。

## What Changes

- 实现人工林逐树状态、矿脉独立储量与换日清场，接B43缓存。
- 切割/钻探使用已确认行为结算、安全中断和能源规则，向B41暴露真实读取。
- 录入本批资源/行为数值并校验单位，表现从实际状态和整数提交事件派生。
- 继承现有玩法与数量规则，仅补齐本批运行合同；验收子集不等于原任务或里程碑全量完成。

## Capabilities

### New Capabilities

- `forest-mineral-production`：林木与矿脉真实作业的增量集成与验收合同。

### Modified Capabilities

无。本能力尚未在 `openspec/specs/` 建立同名正式规格；不修改已有主规格。既有change作为前置设计，差异与替代范围见design.md，不自动归档或同步其未验收能力。

## Impact

- 原始任务映射：`P4-002`、`P4-003`、`P4-009`、`P4-017`。表示本批补齐/优化范围，不改写用户工作簿状态。
- 本计划依赖：[B41 传感器与效应器生命周期](../b41-p2005-p2007-p3006-hardware-runtime-wiring/proposal.md)；[B43 资源权威与入库结算](../b43-p4001-p4010-p4012-p4013-resource-settlement/proposal.md)
- 外部前置：G3-001、P0-005、P0-011；用户提供的植被/切割工具与必要动作锚点；缺少正式资源时可作逻辑验证，不能据此通过正式视觉门。
- 预计实现触及：
  - `Assets/Game/Scripts/AutoEra/ResourcePoints/（按林木/矿脉领域新增）`
  - `Assets/Game/Scripts/AutoEra/World/Region/RegionSensorReadProvider.cs`
  - `Assets/Game/Scripts/AutoEra/Machines/`
  - `Assets/Game/Scripts/AutoEra/Motion/MotionWorkBridge.cs`
  - `Assets/Game/Scripts/AutoEra/DataTable/ResourcePointDefinitions.cs`
  - `GameData/（仅现有资源点与对应行为配置源，实施前列精确文件）`
  - `Assets/Game/Prefabs/Entity/InitialRegion/Forest.prefab`
  - `Assets/Game/Prefabs/Entity/InitialRegion/MineralVein.prefab`
- 验证范围：新增规格中的场景、领域测试与正式宿主集成；公共门与实施顺序见[总计划](../../../Docs/Development/ImprovementPlan-20261008.md)。
- 明确不做：不实现农田批次、灌溉、水泵或全量P4-017；不生成替代正式美术，不恢复被推翻的固定每秒产速，缺失参数仅采用用户明确批准的试玩初值，见design D6。
- 本次产品代码/资源改动已授权，精确路径见evidence/preflight-20261008.md；不修改任务表或Git索引。无新框架、第三方依赖或程序集调整，禁止本批修改ScriptsBuiltin。
