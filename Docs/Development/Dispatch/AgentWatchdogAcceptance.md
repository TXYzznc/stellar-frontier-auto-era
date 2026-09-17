# 守护程序验收标准

1. 队列 Active + 窗口 idle/notLoaded 必须产生 wake。
2. 队列 Active + 窗口 running 必须不产生 wake。
3. 生命周期 AwaitingProducerAcceptance/WaitingCollaboration/Accepted 即使窗口 idle 也不得 wake。
3b. 生命周期 `Cancelled`（用户/制作人决定不再执行）属于终态：即使窗口 idle 也不得 wake，且不得与 `Accepted` 混用，也不得计入 `completed`。
4. 明确暂停不得 wake。
5. 没有可执行任务不得 wake。
6. 同一 decisionId 只能投递一次。
7. 唤醒消息不携带任务正文，只要求窗口读取自己的队列。
8. CLI 投递失败必须落盘记录且不会修改任务状态。
9. 角色窗口状态与任务状态分离，任一状态缺失不得伪称运行。
10. 守护器崩溃或重启后可从追加决策日志恢复，不重复投递。
