# 设计

## 交付闭环

结果包必须包含 taskId、产物路径、证据路径、验证命令/计数、未覆盖项、资源释放和回退窗口。队列的 `ResultReady` 不等于 `Accepted`；制作人验收写入 `producerAcceptedAt` 或 `reworkReason`。

## 通知可靠性

完成回传写入 `.ai/dispatch/events.local.jsonl`，事件包含 `deliveryAttempts`、`lastDeliveryAttemptAt`、`acknowledgedAt` 和 `deliveryState`。调度窗口对未确认结果按退避重试；只有制作人确认事件才停止重试。

## 阻塞恢复

步骤级阻塞保持同一任务其它 Ready 步骤可执行。调度窗口周期性重试依赖；若所有剩余步骤均不可执行才设置任务 Blocked，并记录原因、检查点、恢复条件和升级对象。
