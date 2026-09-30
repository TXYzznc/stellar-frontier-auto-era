## 1. 审计与修复

- [x] 1.1 审计 `RegionWorkQueue` / `MachineNavigation` / `MotionWorkBridge` 的申请、排队、公平/优先级、占用、释放、取消、销毁对称路径
  - 结论：DoD 核心路径已被 4 个测试文件（InitialRegionEditModeTests、MachineRegionIdentityEditModeTests、MachineNavigationEditModeTests、QAInitialRegionBoundaryEditModeTests）覆盖，另发现两个真实缺口（优先级升级、桥接 WorkState 陈旧），另有饥饿/双队列覆盖两个低风险记录项（Non-Goals）。
- [x] 1.2 `RegionWorkQueue.Request` 支持等待中机器优先级升级（严格更高才提位，同级/降级保持 FIFO 原位）
- [x] 1.3 `MotionWorkBridge` 订阅 `queue.Changed` 同步 `WorkState`，`ReleaseWork`/`Dispose`/换队列对称退订

## 2. 测试与验收

- [x] 2.1 EditMode：`InitialRegionEditModeTests.WorkQueue_PriorityUpgrade_PromotesWaitingMachine_WithoutPenalizingPeers`（升级提位、同级/降级保持 FIFO）
- [x] 2.2 EditMode：`MotionWorkBridgeEditModeTests.WorkState_TracksQueuePromotionWithoutManualReRequest`（手工 rig 下被唤醒自动 Granted、释放后 None）
- [x] 2.3 回归：InitialRegion 6/6、MotionWorkBridge 5/5、MachineRegionIdentity 13/13、MachineNavigation 15/15、QAInitialRegionBoundary 4/4；编译 0 错误；`openspec validate --strict`；任务表 P1-005 → 已完成
