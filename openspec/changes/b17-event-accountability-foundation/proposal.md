## Why

P0-007（第一版开发任务表）要求在GF_X事件机制上为任务、算法、效应器、资源和警报提供关联ID。
B02-B16 已建立身份、时钟、机器调度与算法服务，但项目事件仍只依赖进程内 Action 委托与框架级
ShowEntity/PlaySound 事件；中枢 UI（P6-009）、警报（P6-011）、任务队列（P2-004）、算法诊断
（P3-013）与离线报告（P7-010）都要求"一次触发可追溯到来源与最终结果"。工程约定同时规定事件
系统只描述事实、命令与查询必须使用不同接口。

## What Changes

- 新增 AutoEra.Events 核心层：会话级 CorrelationId 分配、EventDomain/EventOutcome 分类、
  仅承载事实的事件包基类、固定容量事实日志与追溯查询。
- 新增命令/查询边界：命令通过显式 OpenCommand 打开关联并返回凭据，查询使用显式只读接口，
  二者都不借用事件总线传递同步依赖。
- 会话集成：AutoEraWorldSession 持有事件服务，工厂支持注入发布器；Application 层提供
  GF 事件桥（纯 C# 会话与测试使用仅日志发布器）。
- 参考接入：机器任务生命周期事实（排队、开始、终态）携带命令关联，验证端到端追溯。
- 资源与警报域只提供领域分类与事实包合同，生产者随各自任务（P6-011 等）接入。

## Capabilities

### New Capabilities

- `event-accountability-core`: 关联 ID、命令/事实边界、事实日志与追溯。
- `event-session-integration`: 会话持有事件服务、GF 发布桥与生命周期对称释放。

### Modified Capabilities

无。机器任务队列仅新增可选事件服务参数与事实发布，不改变既有调度合同。

## Impact

只在 AutoEra/Events 新增核心文件；MachineScheduling/MachineExecutionContext/MachineRoster/World 会话
按可选参数最小扩展；Application 新增 GF 发布桥。无 ScriptsBuiltin、资产、场景、Git、xlsx 修改。
EditMode 测试进入现有测试目录，覆盖关联分配、命令-事实追溯、日志容量回绕与 GF 桥接。