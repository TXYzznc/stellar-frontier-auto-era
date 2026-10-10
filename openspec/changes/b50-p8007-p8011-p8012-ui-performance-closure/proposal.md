# B50 UI加载、性能与证据收口

## Why

当前40个UI预制体合计5253个GameObject，现场详情与操作弹窗体量大。需要以真实测量指导按需加载和刷新优化，并统一本次1920×1080验收与结构适配要求，清理代码/接线/验收状态混用的证据说明。

本变更依据2026-10-08用户确认的[逐变更决策摘要](../../../Docs/Development/ImprovementPlanDecisionSummary-20261008.md)与DEC-204/205建立。用户随后已授权实施；现已交付独立UI修复/回归与文档子集，整批因真实领域/G7/Player前置未交付而未完成，见[evidence](evidence/segment-01-ui-lifecycle-and-evidence.md)。

## What Changes

- 按基准识别大页热点，继续使用独立子Form、GF列表池和按修订刷新。
- 以1920×1080为唯一运行验收分辨率；结构门检查锚点、拉伸、缩放来源与布局冲突。
- 执行10/50/100台机器性能基准、50台60分钟稳定性和反复UI开关，建立可追溯证据与状态索引。
- 继承现有玩法与数量规则，仅补齐本批运行合同；验收子集不等于原任务或里程碑全量完成。

## Capabilities

### New Capabilities

- `ui-performance-evidence-closure`：UI加载、性能与证据收口的增量集成与验收合同。

### Modified Capabilities

无。本能力尚未在 `openspec/specs/` 建立同名正式规格；不修改已有主规格。既有change作为前置设计，差异与替代范围见design.md，不自动归档或同步其未验收能力。

## Impact

- 原始任务映射：`P8-007`、`P8-011`、`P8-012`。表示本批补齐/优化范围，不改写用户工作簿状态。
- 本计划依赖：[B42 算法工作台可用性与闭环](../b42-p3010-p3011-p3013-p3014-algorithm-workbench-closure/proposal.md)；[B45 运输与生产观察](../b45-p4011-p4014-p4015-p4016-production-transport-loop/proposal.md)；[B46 电网热路径优化](../b46-p5006-p5009-p5010-energy-hotpath/proposal.md)；[B49 离线领域推进与回归报告](../b49-p7008-p7009-p7010-p7012-offline-domain-reports/proposal.md)
- 外部前置：G7-001、P8-001以及P8-011依赖的P6-010/P6-011/P7-010；既有ui-form-decoupling及UI契约生成链；B50全批验收不得提前于任务表阶段8前置。
- 预计实现触及：
  - `Assets/Game/Scripts/AutoEra/UI/`
  - `Assets/Game/Scripts/AutoEra/Editor/UiProto/`
  - `Assets/Game/Prefabs/UI/`
  - `Docs/Development/UI-PrefabLayouts/`
  - `Docs/Development/AI-Handoff-Report.md`
  - `Docs/Development/CompilePurityCheck.md`
  - `Docs/Development/ImprovementPlan-20261008.md`
  - `Docs/Development/FunctionalEvidenceIndex-20261009.md`
  - `tools/audit_ui_prefabs.py`、`tools/run_project_checks.py`（只读报告/端口标签）
- 验证范围：新增规格中的场景、领域测试与正式宿主集成；公共门与实施顺序见[总计划](../../../Docs/Development/ImprovementPlan-20261008.md)。
- 明确不做：不机械拆每个装饰节点、不新增多分辨率验收、不全面换UI框架/美术风格；不顺手实现所有未接入页面或宣称G8完成，不修改任务表/共享Git索引。
- 当前修改范围见精确[核验清单](evidence/preflight-20261009.md)，包含产品UI、契约生成/检查链、单一Settings资产与只读审查/健康工具标签；不修改任务表或Git索引，剩余依赖范围未实施。无新框架、第三方依赖或程序集调整，禁止本批修改ScriptsBuiltin。

