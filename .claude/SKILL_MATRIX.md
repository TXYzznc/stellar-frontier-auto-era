# Agent ↔ SKILL 推荐清单

`.claude/agents/*.md` 是 agent 配置的唯一来源，`.codex/agents/*.toml`
由 `python tools/sync-agents.py` 生成。本表及 frontmatter 的 skills 都是推荐，不是权限白名单。
所有角色（包括长期窗口与子Agent）可以按任务选用其他已提供的 SKILL，无需因此请求批准。
技能选择不扩大任务授权、可写路径、框架核心权限或共享工具占用。未安装能力先检查可用替代，
不擅自安装软件或启动独立任务。原生能力与插件能力均按当前实际接口使用。

## 保留门槛

保留的 agent 和 SKILL 必须满足以下条件：

1. 属于框架工程、工具、构建、质量、资源技术或元工作流；
2. 不绑定项目名称、固定业务目录、业务数据模型或内容类型；
3. 不以玩法、角色、关卡、经济、任务或其它产品领域知识为核心；
4. 输入、输出和路径必须从任务上下文或配置获得；
5. 删除 SKILL 时必须同步清理 agent frontmatter、本文和索引。

## 推荐清单

| Agent | Tier | SKILL |
|---|---|---|
| `art-2d` | impl | `gpt-image-2-style-library`, `ai-art`, `codex-image-gen` |
| `art-3d` | impl | `3d-modeling`, `texture-art`, `blender-mcp` |
| `art-anim` | impl | `animation-systems`, `rigging` |
| `art-director` | lead | `art-direction`, `game-art`, `grill-me`, `ai-art`, `codex-image-gen` |
| `art-font` | impl | `typeset`, `font-pairing-suggester`, `font-selection-cjk`, `font-subsetting`, `pixel-font-rendering` |
| `art-ui` | impl | `game-ui-design`, `art-direction`, `ai-art`, `codex-image-gen`, `unity-rect-transform` |
| `art-vfx` | impl | `vfx-realtime`, `shader-effects` |
| `client-lead` | lead | `unity-foundations`, `unity-architecture-di`, `unity-async-patterns`, `grill-me`, `openspec` |
| `client-ta` | impl | `unity-shaders-rendering`, `unity-lighting-vfx`, `shader-effects`, `agency-unity-shader-graph-artist` |
| `client-unity` | impl | `unity-foundations`, `unity-ui`, `unity-input-correctness`, `save-serialization`, `state-machine`, `physics-collision`, `localization-i18n`, `unity-skills`, `unity-rect-transform` |
| `devops-engineer` | impl | `devops-deployment`, `github-actions-docs`, `mobile-cicd`, `secrets-management`, `deploy-checklist`, `feature-flags` |
| `net-backend` | impl | `arch-api`, `jwt-auth`, `oauth-implementation`, `backend-testing` |
| `net-db` | system | `database-schema-design`, `redis-best-practices` |
| `net-lead` | lead | `arch-api`, `game-networking`, `grill-me`, `openspec` |
| `producer` | lead | `project-management`, `task-estimation`, `risk-assessment`, `milestone-tracker`, `grill-me`, `openspec`, `deep-research` |
| `qa-engineer` | impl | `testing-strategies`, `backend-testing`, `crash-analytics`, `k6` |
| `tools-engineer` | impl | `unity-editor-scripting`, `unity-skills`, `uloop-execute-dynamic-code`, `skill-creator`, `find-skills` |

## 补充能力推荐

测试角色在 Unity 项目中优先使用当前已提供的 Unity 测试、Console、Debug、验证技能；
工具角色优先使用当前宿主的 skill-creator；出图优先使用原生 imagegen。
这些推荐不要求新装同名工具，也不改变每项能力本身的使用条件。

## 通用交回规则

只有缺少下列条件会使下一步无法安全执行时，才请求相应负责人裁决：

- 缺少必要权限、输入或存在无法安全解决的资源冲突；
- 需要扩大任务范围、改变公共合同或作出未授权的跨职能决策；
- 重大不可逆决策或框架核心修改缺少明确授权。

缺少某个 MCP、达到讨论轮数或需要其他技能本身不是停工条件。先查可用替代并继续已授权的
安全工作；确需外部动作时实际发送请求、记录阻塞和完成信号，不把本窗口文字当作通知成功。
本文件的框架技能源库存检查仅维护分发内容，不限制角色调用宿主或插件提供的其他技能。
