# Windows 常驻触发器

执行：`powershell -ExecutionPolicy Bypass -File tools/Start-AgentWatchdog.ps1 -Once`

正式常驻时用 Windows 任务计划程序每 30 秒启动一次 `-Once`。脚本使用十分钟锁避免并发实例；守护器只生成和消费 wake 决策，失败不会修改任务状态。
