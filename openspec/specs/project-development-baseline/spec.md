# Project Development Baseline Specification

## Purpose

定义自动纪元项目的正式开发入口、任务表权限、框架与产品职责边界，以及 OpenSpec 方案确认门禁。
## Requirements
### Requirement: 正式开发基线入口
为完成原始任务 `P0-001`，项目 MUST 提供一份可由普通开发者、Codex 和 Claude Code 共同访问的开发基线入口。该入口 MUST 定位正式产品设计来源、第一版冻结范围与排除项、框架与产品边界、OpenSpec 确认门禁以及必要审计命令，并 MUST 通过链接引用权威文档而不是复制完整产品规格。

#### Scenario: 开发者从仓库入口定位正式设计
- **WHEN** 开发者从 `README.md`、`AGENTS.md` 或 `.claude/CLAUDE.md` 进入项目
- **THEN** 每个入口都能定位项目开发基线，并从该基线继续定位 `Docs/GameDesign/` 中的正式设计与开发执行约束

#### Scenario: 基线声明第一版边界
- **WHEN** 开发者阅读项目开发基线
- **THEN** 基线明确列出或链接第一版冻结范围、范围外能力、`ScriptsBuiltin` 禁止承载产品业务的规则以及产品代码根

### Requirement: 任务表永久只读边界
项目基线 MUST 声明 `Docs/GameDesign/05-开发计划/第一版开发任务表.xlsx` 只能由用户本人修改。AI、Agent、SKILL、脚本和自动化工具 MUST NOT 编辑、覆盖、另存替换、重算回写或更新该工作簿中的状态与字段。

#### Scenario: AI 执行开发变更
- **WHEN** AI 根据任务表创建 OpenSpec、文档、代码、资源或测试
- **THEN** AI 只读使用任务表，并把状态、工时或依赖变化作为建议交给用户，不写入工作簿

#### Scenario: 验证本次实施未回写任务表
- **WHEN** `P0-001` 与 `P0-002` 的实施开始和结束
- **THEN** 系统分别计算任务表文件哈希，并确认实施期间哈希保持一致

### Requirement: 框架与产品职责边界
项目基线 MUST 将 `Assets/Game/ScriptsBuiltin/` 定义为不可写入产品业务的框架核心，将 `Assets/Game/Scripts/` 中的既有内容视为框架通用扩展和项目接入边界，并将 `Assets/Game/Scripts/AutoEra/` 定义为自动纪元产品代码根。

#### Scenario: 新增产品业务能力
- **WHEN** 后续任务需要新增自动纪元领域类型
- **THEN** 类型进入 `Assets/Game/Scripts/AutoEra/` 下对应业务领域，而不是写入 `ScriptsBuiltin` 或既有通用扩展目录

### Requirement: 已确认方案后方可建立 OpenSpec
项目基线 MUST 记录 OpenSpec 的方案确认门禁：需求、范围和任务依赖已确认不代表实现方案已确认；每个 OpenSpec MUST 在 artifact 创建前完成实现方向讨论、决策摘要和用户明确确认，并行 OpenSpec MUST 分别确认。

#### Scenario: 同批程序与美术并行
- **WHEN** 一个执行批次同时包含程序和美术 OpenSpec
- **THEN** 两者共享 `b<两位序号>` 批次号，但分别完成方案讨论和用户确认，任一确认不能授权另一个

### Requirement: 方案讨论必须增量归档
项目基线 MUST 要求连续方案讨论每累计两轮至少完成一次增量归档。归档 MUST 保存已确认决策检查点、明确排除、待决事项和受影响任务，并 MUST 同步稳定系统文档、决策记录、设计状态与开放问题。增量归档 MUST 由当前负责窗口自行写入其派发单、OpenSpec或指定专业文档，并通过Git集成窗口提交；普通归档完成 MUST NOT 要求制作人代写、逐项备案或放行。增量归档 MUST NOT 被视为 OpenSpec 最终决策摘要或 artifact 创建授权。

#### Scenario: 长讨论尚未完成
- **WHEN** 同一方案已经累计完成两轮关键讨论但仍有后续问题
- **THEN** 当前负责窗口先把已确认内容写入自身归档来源并交Git集成窗口提交，再继续下一轮讨论且不通知制作人
