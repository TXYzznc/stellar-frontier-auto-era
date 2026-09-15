# 设计

## 状态层

为每个任务维护 `taskId`、`priority`、`readySince`、`state`、`activeStep`、`steps`、`dependencies`、`allowedPaths`、`acceptance`、`handoff` 和 `decisionRequired`。子步骤状态为 `Ready`、`Active`、`WaitingDependency`、`BlockedDecision` 或 `Done`。普通进度写入状态，不发消息。

## 执行层

窗口领取 Active 后必须持续执行到完成或真实阻塞；收到新任务只能追加 Pending。依赖未满足时只阻塞依赖子步骤，并继续其它安全步骤。调度顺序为优先级降序、同优先级 `Ready` 时间 FIFO；协作结果只执行 `WaitingDependency → Ready`，不抢占当前 Active。当前子步骤等待时，先选同一任务的 Ready 子步骤，再从本窗口其它任务选最高优先级 Ready 子步骤。只有所有剩余子步骤均不可执行时，任务才整体 Waiting。

## 协作层

生产者、客户端、美术和QA通过共享状态文件读取前置完成信号。跨窗口即时消息仅用于不可延迟的决策、资源锁冲突和安全风险。

## 责任层

制作人对所有窗口的健康、顺序、依赖和收口负责，用户只对制作人提出决策。新增“任务调度与健康监控”辅助职能，负责机械性读取与维护状态，不拥有范围、视觉或产品决策权，不创建未经授权的任务，不触发 Git。辅助职能发现异常时先写状态并尝试安全恢复，只有需要授权、冲突裁决或用户判断时才升级制作人。

## 调度层

每个角色维护 `Active`、`Ready`、`Waiting`、`Decision` 四个逻辑队列；`Active` 最多一个。任务以依赖图表达，等待只冻结依赖子步骤。Ready 按优先级降序、同级 FIFO 调度。每个 Active 子步骤必须有 `lastActivity` 心跳和 `lastEvidence`；超时标记 `SuspectedStopped`，由调度监控记录、恢复或升级，不得将其静默视为空闲。

## 重启层

重启前生成角色快照；新窗口读取角色快照、任务交接摘要和当前文件现场，先验证队列状态，再接管未完成 Active。不得读取完整历史作为工作依据。

## 验收

验证：无普通通知抢占；无任务未完成即空闲；决策等待不占 Active；Waiting 恢复不插队；同任务可执行步骤优先；优先级＋FIFO 顺序正确；窗口重建后能从快照继续；完成任务有证据、交接摘要和队列收口；Git仍仅用户手动触发。

## 通知事件队列增量

共享事件工具 	ools/dispatch_events.py 提供 append/list/ack，事件写入被 Git 忽略的 .ai/dispatch/events.local.jsonl。调度窗口使用未确认事件唤醒空闲角色，Active 任务只在安全检查点读取；确认以 eventId 幂等写回。

