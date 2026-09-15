# 提案：任务交付闭环与可恢复调度

## 目标

修复任务已写入 completed 但制作人未收到结果、证据失效后仍被视为完成、以及单点阻塞导致窗口长期停工的问题。

## 决策

任务状态拆分为 `Active`、`ResultReady`、`AwaitingProducerAcceptance`、`Accepted`、`Rework` 和 `Blocked`。窗口达到验收标准后提交结果包并进入待验收；制作人核验产物和证据后才 Accepted。回传失败自动重试。阻塞只冻结受影响子步骤，全部不可执行时才整体 Blocked。
