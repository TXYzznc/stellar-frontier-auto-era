# 设计

## 关联 ID 独立分配

CorrelationId 是会话内单调递增的 ulong（0 为 Invalid），由独立 CorrelationAllocator 分配，
不复用 PersistentIdAllocator。理由：关联只用于运行期追溯，不需要跨存档存活；独立计数器避免与
持久对象、临时请求共用一个分配预算，也保证任务系统缺席时（自发警报、资源耗尽）仍可分配。

## 命令、查询与事实的结构化边界

- 事件总线（GF_X EventComponent）只承载事实：只有 `AutoEraFactEventArgs` 派生类型允许进入
  `PublishFact`，命令与查询没有对应的事件类型，从类型系统上杜绝借总线传递命令。
- 命令：`OpenCommand(domain, source, action)` 显式打开关联并返回 CommandTicket；命令不表达
  "已发生"，不产生事件，只在日志中登记待决关联。
- 查询：`TryGetTrace` 等只读接口，不写日志、不分配关联；查询永远不改变可观测状态。
- 事实：领域生产者构造事实包并调用 `PublishFact`；`CausationId` 指向其响应的命令关联；
  `Terminal` 事实为该关联登记最终 EventOutcome。

## 事实日志与追溯

固定容量（512）环形缓冲，元素为只读结构体，预分配、稳态零 GC，容量满时回绕覆盖最旧记录。
追溯按线性扫描：命令记录 + 关联或因果等于该关联的事实记录 + 最新终态结果。追溯是诊断路径，
允许一次受控分配。事件包通过 ReferencePool 获取与释放，遵循 GF 生命周期。

## 发布器抽象

`IEventPublisher.Publish` 接管事实包生命周期：GF 桥交由 EventComponent.Fire 的既有释放路径，
`NullEventPublisher`（仅日志会话）在记录完成后释放回 ReferencePool。核心层不引用 GameEntry 等
Unity 静态入口；Application 层的 GfEventPublisher 完成唯一桥接。

## 会话集成

AutoEraWorldSession 持有 AutoEraEventService（含时钟、分配器、日志、发布器），随会话构造与
释放；工厂提供 `Create(long)`（仅日志）与 `Create(long, IEventPublisher)` 重载，运行期由应用
上下文注入 GF 桥。任务队列等既有构造签名用可选参数扩展，缺席服务时不发布、不影响既有测试。

## 参考接入范围

本变更接入机器任务生命周期事实作为参考实现（Queued/Started/Ended，Ended 为终态事实），证明
一次提交可从关联追溯到来源机器与最终状态。效应器、算法求值事实在后续段接入；资源与警报域仅
定义 EventDomain 与事实包合同，生产者随 P6-011、资源系统任务实现。