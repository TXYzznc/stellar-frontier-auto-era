# 任务

- [x] 1. 实现任务租约与 heartbeat 命令（tools/dispatch_events.py，编译通过）。
- [ ] 2. 实现 Outbox 事件投递状态、确认和指数退避。
- [ ] 3. 实现 watchdog：空闲唤醒、Active 不抢占、SuspectedStopped 恢复和制作人升级。
- [x] 4. 实现 ResultReady/AwaitingProducerAcceptance/Accepted/Rework 门禁（tools/task_lifecycle.py，编译通过）。
- [ ] 5. 更新调度自动化为每分钟巡检并验证重试与恢复。
- [ ] 6. 运行自动化测试并记录证据。
