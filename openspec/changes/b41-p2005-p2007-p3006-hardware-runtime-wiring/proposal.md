# B41 传感器与效应器生命周期

## Why

机器上下文已拥有传感器集合和行为队列能力，但生产路径未建立传感环境、采样注册和效应器执行绑定。只在测试里接通提供者，会让配置界面有绑定而运行时无数据或无法执行。

本变更依据2026-10-08用户确认的[逐变更决策摘要](../../../Docs/Development/ImprovementPlanDecisionSummary-20261008.md)与DEC-204/205建立。当前授权为方案文档，未开始实施。

## What Changes

- 建立部署/装配变化到传感器及效应器运行绑定的生产注册链。
- 按真实永久ID、能力和绑定代次验证目标，动态硬件变更幂等对账。
- 结果回传连接到算法与责任事件，动作表现仅消费结果；为生产域提供明确适配接入点。
- 继承现有玩法与数量规则，仅补齐本批运行合同；验收子集不等于原任务或里程碑全量完成。

## Capabilities

### New Capabilities

- `hardware-runtime-lifecycle`：传感器与效应器生命周期的增量集成与验收合同。

### Modified Capabilities

无。本能力尚未在 `openspec/specs/` 建立同名正式规格；不修改已有主规格。既有change作为前置设计，差异与替代范围见design.md，不自动归档或同步其未验收能力。

## Impact

- 原始任务映射：`P2-005`、`P2-007`、`P3-006`。表示本批补齐/优化范围，不改写用户工作簿状态。
- 本计划依赖：[B40 算法正式运行驱动](../b40-p3003-p3005-p3007-p3008-algorithm-runtime-drive/proposal.md)
- 外部前置：P2-002、P2-004、P2-006、P1-001；b14-sensor-binding-and-public-readouts、b18-effector-behavior-nodes与已归档b16动作桥接合同。
- 预计实现触及：
  - `Assets/Game/Scripts/AutoEra/World/Region/RegionMachineRuntimeRegistry.cs`
  - `Assets/Game/Scripts/AutoEra/World/Region/InitialRegionScene.cs`
  - `Assets/Game/Scripts/AutoEra/World/Region/RegionSensorReadProvider.cs`
  - `Assets/Game/Scripts/AutoEra/Machines/Sensors/`
  - `Assets/Game/Scripts/AutoEra/Machines/MachineScheduling.cs`
  - `Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmMachineAdapter.cs`
  - `Assets/Game/Scripts/AutoEra/Motion/MotionWorkBridge.cs`
- 验证范围：新增规格中的场景、领域测试与正式宿主集成；公共门与实施顺序见[总计划](../../../Docs/Development/ImprovementPlan-20261008.md)。
- 明确不做：不伪造农田湿度、矿产数量或成熟状态；不新增职业机器、传感器等级差异或通信能力；生产行为权威由B44/B43提供。
- 本次不修改产品代码、资源、任务表或Git索引；未来实施仍需授权与依赖就绪。无新框架、第三方依赖或程序集调整，禁止本批修改ScriptsBuiltin。
