# 设计

- `watch` 默认 stale 阈值降为 3 分钟。
- 输出注册表 threadId、租约过期和任务／心跳不一致标志，动作明确为 `WakeRegisteredWindow`。
- 无 Active 但存在 blocked suspended 时输出 `RouteBlockedTask`，推动自助恢复或专业协作，避免阻塞任务静默遗忘。
- 调度窗口每次 heartbeat 自动化运行都必须执行 watch，并对该动作实际调用注册窗口消息；旧窗口永不作为目标。
- 唤醒后仍无心跳或无法恢复时，调度窗口才向制作人发出升级事件；普通进度不升级。
