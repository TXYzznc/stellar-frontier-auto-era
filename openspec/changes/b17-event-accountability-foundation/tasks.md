# 任务

## 1. 核心事件层（event-accountability-core）

- [x] 1.1 CorrelationId 只读结构与 CorrelationAllocator（会话级单调分配，Invalid=0）
- [x] 1.2 EventDomain/EventOutcome/EventKind 分类
- [x] 1.3 AutoEraFactEventArgs 事实包基类（关联、因果、领域、来源、世界毫秒、序号，Clear 对称）
- [x] 1.4 EventJournal 固定容量环形日志与追溯查询
- [x] 1.5 AutoEraEventService：OpenCommand/PublishFact/TryGetTrace 与 IEventPublisher 抽象

## 2. 会话与 GF 桥（event-session-integration）

- [x] 2.1 AutoEraWorldSession 持有事件服务，工厂提供仅日志与注入发布器两种创建路径
- [x] 2.2 Application 层 GfEventPublisher 桥接 EventComponent.Fire
- [x] 2.3 会话释放路径覆盖事件服务，无静态状态

## 3. 任务域参考接入

- [x] 3.1 MachineTaskQueue 可选事件服务参数与 MachineTaskRecord.Correlation
- [x] 3.2 Queued/Started/Ended 事实发布，Ended 为终态事实
- [x] 3.3 MachineExecutionContext/MachineRoster 传递服务，既有测试保持通过

## 4. 验证

- [x] 4.1 EditMode：关联分配、命令-事实追溯、日志回绕、边界（查询不写日志）
- [x] 4.2 EditMode：会话生命周期与 GF 桥发布（stub 发布器）
- [x] 4.3 EditMode：任务域事实序列与终态追溯
- [x] 4.4 既有 EditMode 回归保持基线