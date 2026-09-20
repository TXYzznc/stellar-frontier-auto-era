## Why

本仓库实例已进入正式产品开发阶段。`Docs/Development/ProjectBaseline.md` 已于 2026-09-18
记录「框架部分内容此前已有备份，本仓库按具体业务项目管理，原『框架层禁改』红线解除」，
但仓内其余权威文档与两份 OpenSpec 规范仍保留旧口径，形成互相矛盾的规则源：

- `AGENTS.md`（框架纯度、项目约束）
- `.claude/CLAUDE.md`（Unity/GF_X 约束）
- `.claude/conventions.md`（目录边界）
- `README.md`（纯度与提交前检查）
- `openspec/specs/project-development-baseline/spec.md`（框架与产品职责边界）
- `openspec/specs/project-boundary-audit/spec.md`（独立项目边界审计、结构变更完整验证）

规则源不一致会让后续 UI 预制体工作流、框架模板改造和审计判定互相打架。本变更把两份规范
对齐到已确认口径，落盘其余文档的同步修改。

## What Changes

- 规范口径统一为：`Assets/Game/ScriptsBuiltin/` 是 GF_X 框架核心，**允许修改**（模板、菜单、
  目录常量、配置项），修改需留痕；框架核心仍**不得依赖 `AutoEra` 业务类型**。
- 「框架层禁改」不再作为 `project-development-baseline` 的能力要求；改为「可修改 + 需留痕 +
  不得依赖产品业务类型」。
- 「结构变更完整验证」不再以「本次实施没有修改 `ScriptsBuiltin`」作为验收断言；改为验证
  框架核心未引入产品业务类型依赖，且修改已在 OpenSpec 或决策记录中留痕。
- `project-boundary-audit` 的目的表述从「阻止产品实现反向污染框架核心」改为「约束产品代码
  根与命名空间、维持框架核心与产品业务类型的依赖方向」。
- `audit_project_boundaries.py` 的 `framework-dependency` 规则**不削弱**：它扫描的是框架核心
  代码（注释与字符串字面量已剥离），因此项目模板路径字符串不触发它；只有真正声明或引用
  `AutoEra` 业务类型才失败。该规则继续作为有效围栏。

## Capabilities

### New Capabilities

无。

### Modified Capabilities

- `project-development-baseline`：框架与产品职责边界、以及正式开发基线入口中关于
  `ScriptsBuiltin` 承载产品业务的表述。
- `project-boundary-audit`：独立项目边界审计的目的表述与结构变更完整验证的验收断言。

## Impact

- 仅文档与规范文本：`AGENTS.md`、`.claude/CLAUDE.md`、`.claude/conventions.md`、`README.md`、
  `Docs/Development/ProjectBaseline.md`（已在红线解除时更新）与本变更的两份 spec delta。
- 不修改 `audit_framework_purity.py`、`audit_project_boundaries.py` 的任何规则或代码。
- 不修改任何运行时代码、资源、场景或配置；不涉及 asmdef / HybridCLR / Obfuz / FSR。
- 后续 UI 预制体工作流（命名规范、原型两阶段、生成器与模板改造）另行建立变更，不在本变更内。
