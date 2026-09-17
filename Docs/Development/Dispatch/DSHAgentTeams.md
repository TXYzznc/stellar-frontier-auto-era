# DSH AgentTeams 协作手册

AutoEra 项目在 DSH（DeepSeek Harness）主平台上的协作总则。主力平台已从 Codex 多窗口迁移到
DSH AgentTeams 团队；Codex 多窗口语义作为遗留路径保留，既有文档（`README.md`、`TaskQueue.md`、
`RoleRouting.md`、`GitIntegration.md`、`AgentWatchdog.md` 等）不删除，仅在与本文件冲突时以本文件为准。

## 定位

- DSH 团队 id：`autoera-dsh`。队长（captain）＝制作人＋策划＋Git 集成。
- 成员按 DAG 自治执行；协作语义由本文件与队长派发规格确定，成员不必了解 AgentTeams 内部实现。
- 状态可见：队长每次状态变更导出 `.ai/dispatch/dsh-team-state.local.json`，供 AutoEraWatchdog
  「DSH 团队」页显示（见 `AgentWatchdog.md` 的 DSH 模式一节）。

## 角色映射

逻辑角色是文档/派发中的规范用语；实际成员名是 AgentTeams 在线名册（`.agent-teams/autoera-dsh/team.json`）
与 `.ai/dispatch/dsh-team-state.local.json` 中 `name` 字段的真实值。二者一一对应、语义等价。

| 逻辑角色 | 实际成员名 | 合并职责 | 说明 |
|---|---|---|---|
| 队长 | captain | 制作人＋策划＋Git 集成 | Git 仅用户明确触发：显式路径、中文提交、不推送 |
| dev | programmer | client-lead＋client-unity＋client-ta＋tools-engineer | 程序＋工具工程师 |
| qa | tester | qa-engineer | 测试验证，默认只诊断和报告 |
| art-design | art-writer | art-director＋原画职责（概念层） | 提示词包＋设计文档＋视觉合同，不驱动出图/Unity/Blender |
| art-3d | art-maker | 制作层（DCC） | Blender/MaterialMaker/FBX 导出/ArtResource Unity 8091 导入验证 |

- 队长＝制作人＋策划＋Git 集成：范围/优先级/依赖变化、跨职能冲突、批次验收，以及用户明确手动
  触发后的 Git 集成（显式路径、中文提交、只提交不推送）。
- dev 合并 client-lead/client-unity/client-ta/tools-engineer；架构与范围决策升级队长，不直接联系用户。
- art-design 是概念层：交付提示词包、设计文档与视觉合同（语义 Token、字号层级、prefab-layout
  语义等），不驱动图像生成、Unity 或 Blender。
- art-3d 是制作层：只执行 art-design 冻结的技术合同与路线，不自行改需求、不做概念决策，遵守
  `../3DArtProductionWorkflow.md` 的阶段门（三视图→路线审批→建模→烘焙→导入验证）。
- net-*、devops、rapid-executor 按需再加，不常驻。

## 批次流程

1. captain 规划 staged 计划：角色、任务、依赖 DAG 一次性给出。
2. 用户通过 Web 审阅并批准计划。
3. 成员按 DAG 自治执行，完成后把结果与验证证据写入 `update_task`（含 `commandsRun`、
   `changedPaths`；质量类任务另带 `acceptanceResults`/`verdict`/`findings`）。
4. 批次收口：队长汇总验收、回写状态文件与队列/生命周期。

成员自主推进派发边界内工作，普通进度不打断；只有缺少决定/授权/交接且无法继续其它安全工作时才等待。

## 状态同步合同

- 队长在批次里程碑把任务结果回写 `.ai/dispatch/task-queue.local.json` 与
  `.ai/dispatch/task-lifecycle.local.json`，沿用 `window_task_queue.py` / `task_lifecycle.py`
  的既有写协议（锁文件＋原子替换），不新造协议。
- 每次状态变更导出 `.ai/dispatch/dsh-team-state.local.json` 供守护器显示。schema：

```json
{
  "schemaVersion": 1,
  "generatedAt": "ISO 时间戳",
  "team": "autoera-dsh",
  "members": [{ "name": "programmer", "role": "程序·工具工程师", "activity": "working" }],
  "tasks": [{ "id": "t1", "subject": "...", "status": "in_progress", "assignee": "programmer", "output": "" }]
}
```

- `name` / `assignee` 字段写入实际成员名（`programmer`/`tester`/`art-writer`/`art-maker`），与
  上方角色映射表的「实际成员名」列一致。

- `generatedAt` 是守护器判断数据新鲜度的唯一依据；超过阈值（默认 300 秒）时 UI 高亮“已过期”。

## DCC 环境策略

- 全权限模式已实证（用户开启 `danger-full-access`＋审批 `never`）：本会话可直读写
  `D:\unity\UnityProject\ArtResource` 并驱动 Unity 8091（UnitySkills HTTP，
  instanceId `ArtResource_8CDFD6CE`）。
- art-3d 直接写入 ArtResource，无需逐次审批；路径边界（产物只进 ArtResource、主项目只读、
  不 Git）写进成员执行提示词，靠纪律执行而非沙箱强制。
- MaterialMaker 沿用 B04 已建立的可重复批处理路径。
- 跨工作区：DSH 团队绑定本会话，跨目录工作无需另开会话。默认不把美术产线拆成 ArtResource
  专属会话；若未来拆分，需要文件/记忆交接，默认不拆。
- 每次驱动 Unity 8091 前用 health / project_get_info 确认 instanceId 对应 ArtResource；
  主项目 8092 禁改。

## 冲突规避（Codex 遗留窗口）

- Codex 客户端窗口 Active=`p0011-baseline-config-skeleton` 期间，其写入范围对 DSH 成员只读。
- 其 Pending（`P0-013` 建立项目层编译与框架纯度检查入口、`P7-001` 实现三槽存档与元数据）仍由
  Codex 客户端窗口消化，不重复入 DSH。
- Codex 遗留窗口的 Active 任务范围对 DSH 成员只读；DSH 不抢占、不代领 Codex 队列。

## Git 集成（队长）

- 仅用户明确手动触发；显式路径、中文提交、只提交不推送。语义详见 `GitIntegration.md`。
- DSH 成员（dev/qa/art-design/art-3d）不自行暂存或提交，不自动请求 Git 工作。

## 与既有文档的关系

- 本目录 `README.md`、`TaskQueue.md`、`RoleRouting.md`、`GitIntegration.md`、
  `AgentWatchdog.md` 等作为 Codex 遗留路径保留，描述多窗口语义。
- DSH 为主平台；DSH 团队协作与本文件冲突时以本文件为准。
- 禁止：编辑 `.codex/**`、直接手改队列 JSON、删除既有 Codex 文档。
