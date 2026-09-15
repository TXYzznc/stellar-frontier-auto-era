## Why

B11–B14提供了机器、队列、导航和传感器基础，但尚无玩家算法图服务。用户2026-09-11已确认纯C#核心＋GF适配路线与S1–S3实施，需要形成独立可验证的算法服务闭环。

## What Changes

- 新增固定类型图模型、验证及不可变执行计划，不复用MotionGraph。
- 实现触发快照、批次求值、算力/任务/导航适配、结果与因果记录。
- 实现草稿、生效版本、安全应用、清绑定模板和内存恢复。
- 联合验证既有硬件UI观察、传感器、导航生命周期；不改变正式UI/场景。

## Capabilities

### New Capabilities

- `algorithm-graph-core`: 固定图文档、类型、验证与计划。
- `algorithm-service-execution`: 事件求值、权威服务适配与诊断。
- `algorithm-instance-lifecycle`: 草稿、应用、模板和内存恢复。

### Modified Capabilities

无。复用既有机器服务合同，不重新定义B11–B14。

## Impact

只在AutoEra/Algorithms新增核心，按需最小修改Machines/World/Region/Application消费层及现有测试目录；无新依赖/产品asmdef/ScriptsBuiltin/正式资产/配置/Git/xlsx修改。对应P3-001～009、013的服务子集；不宣称全部P3或G3，UI与五生产模板及成本、农业/能源/磁盘/离线仍独立门禁。
