# Agent Watchdog

`tools/agent_watchdog.py` is a model-free decision engine. It reads role queues and the window registry, then emits idempotent `wake` decisions only when a role has runnable work and no active task. It never reads task bodies or sends messages.

Use `python tools/agent_watchdog.py --role <role> --append` from an external host scheduler. The scheduler is responsible for consuming `watchdog-decisions.local.jsonl` and invoking the supported Codex window wake API. Explicit pause flags suppress wakeups; `待验收` and `等待协作` are not runnable and do not wake a role.

## DSH 模式

主力平台迁到 DSH AgentTeams 后，Codex 唤醒链路（`CODEX_HOME` sqlite 窗口枚举＋PowerShell 唤醒）
失效；`.ai/dispatch` 文件状态层保留。守护器在以下条件进入 DSH 模式：

- 显式 `--backend dsh`；或
- `.ai/dispatch/dsh-team-state.local.json` 存在且未显式 `--backend codex`。

DSH 模式下行为：

- 决策照常写入 `watchdog-decisions.local.jsonl`，但 `action` 固定为 `record`；
- 不生成唤醒目标（`threadId` 置空）、不调用窗口唤醒；
- 无 dsh 状态文件时保持旧行为（`wake`/`none` 判定不变）。

```powershell
python tools/agent_watchdog.py --backend dsh --append
```

守护面板 `tools/AgentWatchdogUI.py` 新增「DSH 团队」页：读取 `dsh-team-state.local.json`
（schema `{schemaVersion, generatedAt, team, members[{name,role,activity}], tasks[{id,subject,status,assignee,output}]}`），
显示成员/任务状态与数据新鲜度（`generatedAt` 相对时长，超过阈值高亮“已过期”）。DSH 模式下禁用
Codex 窗口重绑定与手动唤醒，保留队列/生命周期只读视图；「入队」语义改为写给 DSH 队长收件箱
（`.agent-teams/autoera-dsh/inbox/captain.jsonl`）。

状态同步由队长负责：批次里程碑回写 `task-queue.local.json` / `task-lifecycle.local.json`，并在每次
状态变更导出 `dsh-team-state.local.json`。详见 `DSHAgentTeams.md`。
