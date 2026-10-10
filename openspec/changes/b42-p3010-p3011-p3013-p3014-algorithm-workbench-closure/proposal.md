# B42 算法工作台可用性与闭环

## Why

算法工作台已能编辑图，但现有截图显示端口标签拥挤及信息层级问题，已有UI测试主要证明图操作。需要在B40/B41的真实运行链上验收应用、历史与诊断，并遵循用户仅1920×1080验收、节点布局仍适配的要求。

本变更依据2026-10-08用户确认的[逐变更决策摘要](../../../Docs/Development/ImprovementPlanDecisionSummary-20261008.md)与DEC-204/205建立。当前授权为方案文档，未开始实施。

## What Changes

- 修正节点端口排版、选中/连线/验证反馈及诊断定位；保留强类型和版本检查。
- 按视图、编辑命令和诊断读模型局部拆分，运行时生命周期归B40。
- 建立1920×1080视觉/交互验收与RectTransform结构适配检查，形成G3真实执行证据。
- 继承现有玩法与数量规则，仅补齐本批运行合同；验收子集不等于原任务或里程碑全量完成。

## Capabilities

### New Capabilities

- `algorithm-workbench-production-closure`：算法工作台可用性与闭环的增量集成与验收合同。

### Modified Capabilities

无。本能力尚未在 `openspec/specs/` 建立同名正式规格；不修改已有主规格。既有change作为前置设计，差异与替代范围见design.md，不自动归档或同步其未验收能力。

## Impact

- 原始任务映射：`P3-010`、`P3-011`、`P3-013`、`P3-014`。表示本批补齐/优化范围，不改写用户工作簿状态。
- 本计划依赖：[B40 算法正式运行驱动](../b40-p3003-p3005-p3007-p3008-algorithm-runtime-drive/proposal.md)；[B41 传感器与效应器生命周期](../b41-p2005-p2007-p3006-hardware-runtime-wiring/proposal.md)
- 外部前置：P0-008、P3-001、P3-002、P0-007及P3-009/P3-012/P3-015的既有模板链；G3-001前置任务全部证据；完整正常新进度入口的综合复验等待B47及正式初始配置。
- 预计实现触及：
  - `Assets/Game/Scripts/AutoEra/UI/AlgorithmEditorForm.cs`
  - `Assets/Game/Scripts/AutoEra/UI/AlgorithmNodeItem.cs`
  - `Assets/Game/Scripts/AutoEra/UI/AlgorithmGraphCanvasInteraction.cs`
  - `Assets/Game/Scripts/AutoEra/UI/Integration/AlgorithmReadModel.cs`
  - `Assets/Game/Scripts/AutoEra/Editor/UiProto/AlgorithmEditorPrefabMigration.cs`
  - `Assets/Game/Prefabs/UI/`
  - `Docs/Development/UI-PrefabLayouts/AlgorithmEditorForm.contract.json`
- 验证范围：新增规格中的场景、领域测试与正式宿主集成；公共门与实施顺序见[总计划](../../../Docs/Development/ImprovementPlan-20261008.md)。
- 明确不做：不重写图框架，不增加移动端/其他分辨率验收，不默认实现撤销重做历史，不复制模板规则或升级美术风格。UI资产允许清单仅算法编辑器及其现有节点/连线项和直接契约，不覆盖整个UI目录。
- 本次不修改产品代码、资源、任务表或Git索引；未来实施仍需授权与依赖就绪。无新框架、第三方依赖或程序集调整，禁止本批修改ScriptsBuiltin。
