# 设计

窗口领取任务生成 leaseId；Active 必须周期性写 lastActivity、lastHeartbeat、lastEvidence。Watchdog 每分钟检查：空闲且有事件则发送最小唤醒；租约超时标记 SuspectedStopped 并尝试重新唤醒原窗口；连续失败才升级制作人。结果包进入 AwaitingProducerAcceptance，只有制作人验收才 Accepted。

事件 Outbox 记录 deliveryAttempts、lastDeliveryAttemptAt、acknowledgedAt、deliveryState，采用指数退避和幂等 eventId。Watchdog 不修改业务结果，不抢占 Active，不触发 Git。
