## Context

Codex窗口消息是即时输入，不适合作为可靠的待办队列。队列必须跨窗口回合和应用重启保留，同时
不能把本机执行状态提交到仓库。

## Decisions

1. 使用`.ai/dispatch/task-queue.local.json`保存本机队列，正式规则与工具进入仓库。
2. 每个职能保存Active、Pending、Suspended、Completed；Active唯一。
3. Pending按数值优先级升序、同级入队序号升序；Active不参与重排。
4. 合法抢占要求原因类型、批准来源和安全检查点；紧急项完成后优先恢复Suspended。
5. 下发方只在目标空闲且没有Active时发送唤醒消息，运行中只写队列。
6. 3D原画生产图阶段门以制作人对主美技术文档的验收结果为入队前置。

## Safety

- JSON写入使用独占锁和临时文件原子替换。
- 重复taskId、非Active完成、非Pending抢占、缺少检查点或批准来源均拒绝。
- 队列不授予文件、Unity、DCC、Git或任务表权限；任务仍引用正式派发单／OpenSpec。
