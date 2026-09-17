# B17 证据 — 段1：核心事件层、会话集成与任务域参考接入

日期：2026-09-16。任务：P0-007 建立项目事件与责任追踪基础。

## 交付

- `Assets/Game/Scripts/AutoEra/Events/`：CorrelationId、CorrelationAllocator、EventClassification
  （EventDomain/EventKind/EventOutcome）、AutoEraFactEventArgs（事实包基类）、EventJournal
  （512 容量环形日志 + 追溯）、AutoEraEventService（OpenCommand/PublishFact/TryGetTrace）、
  IEventPublisher、NullEventPublisher。
- `Assets/Game/Scripts/AutoEra/Application/GfEventPublisher.cs`：GF EventComponent 桥；
  EditMode/无组件时回池，不丢包不漏释放。
- 会话集成：`AutoEraWorldSession.Events`（随会话构造/释放）、工厂 `Create(long[, publisher])`
  重载、应用上下文可选发布器注入、组装根默认接线 GF 桥。
- 任务域参考接入：`MachineTaskQueue` 可选事件服务参数 + `MachineTaskRecord.Correlation`；
  Queued/Started/Ended 事实（Ended 为终态事实，映射 Completed/Failed/Cancelled）；
  `MachineExecutionContext` 可选参数传递；`MachineTaskFactEventArgs` 事实包。
- OpenSpec：`openspec/changes/b17-event-accountability-foundation/`（proposal/design/tasks/
  specs/event-accountability-core、specs/event-session-integration）。

## 验证

- `AutoEra.Tests.Editor.EventAccountabilityEditModeTests`：7/7 通过（jobId b902ead2）。
  覆盖：关联单调分配、命令入册不上总线、事实终态解析、日志回绕恒定容量、查询只读、
  释放后拒绝、命令动作校验。
- `AutoEra.Tests.Editor.EventSessionIntegrationEditModeTests`：7/7 通过（jobId ed2791c3）。
  覆盖：仅日志会话、释放后拒绝、注入发布器、GF 桥 EditMode 回池、任务域事实序列与追溯、
  取消终态、无服务旧行为不变。
- 全量 EditMode 回归：474 用例（460 存量 + 14 新增），471 通过，3 失败均为改动前既存基线
  （FieldHudForm、DataTableProfile、InitialRegionScene Destroy），本次改动零新增失败。
- 编译：域重载后 Console 无编译错误。

## 边界与遗留

- 资源、警报域仅定义 EventDomain 与事实包合同，生产者随 P6-011 / 资源系统任务接入。
- GF 桥运行期路径由 EditMode 回池测试覆盖；PlayMode 全链路待后续真实 UI/警报消费时验证。
- 未修改 ScriptsBuiltin、资产、场景、Git、xlsx。