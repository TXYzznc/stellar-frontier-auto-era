## Why

P1-005「作业点申请与等待队列」的 DoD 是「多个机器竞争同一点时不会重叠作业或重复寻路，释放后按序接替」。RegionWorkQueue / MachineNavigation / MotionWorkBridge 已交付并被 4 个测试文件覆盖，但逐项审计后仍发现两个真实缺口：

1. **优先级升级缺失**：等待中的机器带着更高优先级重新申请，`Request` 直接返回 Waiting 而不提位——机器接到高优先级任务后无法在队列里提前。
2. **MotionWorkBridge.WorkState 陈旧**：桥接层不订阅 `queue.Changed`，原持有者释放、本机被唤醒后，`WorkState` 仍停留在申请时的 Waiting 快照；集成测试靠手动二次 `RequestWork` 绕过。

## What Changes

- `RegionWorkQueue.Request`：等待中的机器以**严格更高**优先级重新申请时按新优先级提位重排；同级/降级保持 FIFO 原位（不惩罚重新申请）。
- `MotionWorkBridge`：`RequestWork` 后订阅 `queue.Changed`，回调用 `GetRequestState` 同步 `WorkState`；`ReleaseWork` / `Dispose` / 换队列时对称退订。

## Capabilities

### Modified Capabilities

- `region-work-queue`: 作业点占用队列补齐优先级升级与桥接层状态同步。

## Impact

- `Assets/Game/Scripts/AutoEra/World/Region/RegionWorkQueue.cs`（优先级升级）
- `Assets/Game/Scripts/AutoEra/Motion/MotionWorkBridge.cs`（WorkState 跟随）
- `Assets/Game/Tests/AutoEra/Editor/InitialRegionEditModeTests.cs`（升级测试）
- `Assets/Game/Tests/AutoEra/Editor/MotionWorkBridgeEditModeTests.cs`（状态跟随测试）

## Non-Goals

- 公平性饥饿防护（静态优先级下的持续申请流饿死场景，当前生产无持续申请流，记录不实现）。
- 同 target 同 channel 双队列投影互相覆盖的防御（生产由 RegionObjectView 的 `_workChannels` 配置唯一性保证）。
- 到达后保留占用的上层契约变更（「到达 = 占用直到显式释放」，已有集成测试钉住）。
