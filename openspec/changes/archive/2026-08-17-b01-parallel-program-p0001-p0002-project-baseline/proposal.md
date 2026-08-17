## Why

第一版正式开发即将开始，但当前工程入口仍以领域无关框架为中心，尚未为自动纪元建立统一的设计导航、产品代码归属和可自动验证的项目边界。现在先完成任务 `P0-001` 与 `P0-002`，可以避免后续业务代码、资源和开发流程反向污染 `ScriptsBuiltin` 或形成无法追踪的目录约定。

## What Changes

- 覆盖第一版开发任务表中的原始任务 `P0-001`（建立第一版开发基线）与 `P0-002`（建立自动纪元项目层目录与命名空间）。
- 新增简洁的项目开发基线入口，集中链接正式设计来源、冻结范围、排除项、框架边界、OpenSpec 门禁和审计命令。
- 从 Codex、Claude Code 和普通开发者入口链接项目开发基线。
- 在 `Assets/Game/Scripts/AutoEra/` 建立产品代码根目录，产品类型统一使用 `AutoEra` 或 `AutoEra.*` 命名空间，并按业务领域渐进创建子目录。
- 继续使用现有 `Hotfix` 程序集；不新增或重命名项目 asmdef，不调整 HybridCLR、Obfuz 或发布热更新策略。
- 修复实施验收中发现的 FSR/Harmony 与 Burst 兼容问题：保持 FSR 2.4.2 API 与功能不变，将存在无效元数据的 Fat Harmony 替换为官方 `Lib.Harmony.Thin 2.4.2` net48 及其固定版本依赖，并补充来源与哈希记录。
- 产品资源沿用现有资源类型根目录并直接按业务类别组织，不机械增加 `AutoEra` 资源目录层。
- 保留领域无关的框架纯度审计，新增独立的项目边界审计及最小自动化测试。
- 将 OpenSpec 批次号修订为 CLI 兼容的 `b<两位序号>` 格式；本变更使用 `b01`，与后续单独确认的同批并行美术变更共享批次号。

## Capabilities

### New Capabilities

- `project-development-baseline`: 定义自动纪元第一版的正式开发入口、设计来源、冻结范围、排除项、任务表权限和框架边界。
- `project-boundary-audit`: 定义产品代码目录与命名空间规则、资源目录规则及其自动化验证行为。

### Modified Capabilities

- 无。

## Impact

- 文档入口：`AGENTS.md`、`.claude/CLAUDE.md`、`README.md`、`Docs/Development/ProjectBaseline.md` 和相关开发计划/决策记录。
- 产品代码边界：`Assets/Game/Scripts/AutoEra/`，仍编译进现有 `Hotfix` 程序集。
- 项目工具：新增项目边界审计脚本及其测试；现有 `tools/audit_framework_purity.py` 保持领域无关。
- 验证流程：结构变更使用普通 Unity 编译，FSR 不替代完整编译；两个审计均必须通过。
- 编辑器开发依赖：`LocalPackages/FastScriptReload/` 内的 Harmony 分发形态和传递依赖；不启用 Player 热重载。
- 权限：`Docs/GameDesign/05-开发计划/第一版开发任务表.xlsx` 始终由用户维护，本变更不得写入或替换该文件。
