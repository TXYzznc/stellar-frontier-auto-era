## MODIFIED Requirements

### Requirement: 正式开发基线入口
为完成原始任务 `P0-001`，项目 MUST 提供一份可由普通开发者、Codex 和 Claude Code 共同访问的开发基线入口。该入口 MUST 定位正式产品设计来源、第一版冻结范围与排除项、框架与产品边界、OpenSpec 确认门禁以及必要审计命令，并 MUST 通过链接引用权威文档而不是复制完整产品规格。

#### Scenario: 开发者从仓库入口定位正式设计
- **WHEN** 开发者从 `README.md`、`AGENTS.md` 或 `.claude/CLAUDE.md` 进入项目
- **THEN** 每个入口都能定位项目开发基线，并从该基线继续定位 `Docs/GameDesign/` 中的正式设计与开发执行约束

#### Scenario: 基线声明第一版边界
- **WHEN** 开发者阅读项目开发基线
- **THEN** 基线明确列出或链接第一版冻结范围、范围外能力、框架核心可修改但不得依赖 `AutoEra` 业务类型的规则，以及产品代码根

### Requirement: 框架与产品职责边界
项目基线 MUST 将 `Assets/Game/ScriptsBuiltin/` 定义为 GF_X 框架核心，并 MUST 允许为项目需要修改该核心与 Editor 工具链（模板、创建菜单、目录常量、配置项）；此类修改 MUST 可追溯，即在 OpenSpec 或决策记录中留痕。框架核心 MUST NOT 依赖 `AutoEra` 业务类型。项目基线 MUST 将 `Assets/Game/Scripts/` 中的既有内容视为框架通用扩展和项目接入边界，并将 `Assets/Game/Scripts/AutoEra/` 定义为自动纪元产品代码根。

#### Scenario: 新增产品业务能力
- **WHEN** 后续任务需要新增自动纪元领域类型
- **THEN** 类型进入 `Assets/Game/Scripts/AutoEra/` 下对应业务领域，而不是写入 `ScriptsBuiltin` 或既有通用扩展目录

#### Scenario: 项目需要修改框架工具链
- **WHEN** 项目接入需要修改框架模板、创建菜单、目录常量或配置项
- **THEN** 该修改被允许，并在 OpenSpec 或决策记录中留痕，且未在框架核心引入 `AutoEra` 业务类型依赖

#### Scenario: 框架核心引入产品业务类型依赖
- **WHEN** `Assets/Game/ScriptsBuiltin/` 中的代码声明或引用 `AutoEra` 业务类型
- **THEN** 该修改不被允许，项目边界审计报告框架核心越界
