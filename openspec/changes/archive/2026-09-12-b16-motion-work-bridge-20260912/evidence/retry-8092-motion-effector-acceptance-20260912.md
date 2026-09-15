# B16 8092 真实动作/效应器联合验收

时间：2026-09-12 15:12–15:13 UTC。仅使用正式项目 Unity 8092（PID 90340），未调用 8090。

## 新鲜测试结果

- `MotionWorkBridgeIntegrationTests` job `538950d3`：原生 XML `Passed`，1/1，start `15:12:40Z`，end `15:12:52Z`。测试实例化 InitialRegion/NavMesh、正式 `WheeledCarrier`、正式 `WaterCannon` 组件定义 22021、`MotionWorkBridge`、`RegionWorkQueue` 与 `EffectorBehaviorQueue<EffectorWorkKind>`，连续执行两轮。
- 每轮验证同一机器/任务/请求链：NavMesh 到达并驱动 MotionExecutor；作业预约从 Waiting 到 Granted；关闭 FieldHud 观察面板和完整 UI Form 均不取消当前效应器；水枪动作图可应用并恢复 bind pose；断电暂停后恢复；任务取消在安全点结束当前请求、取消等待请求并释放导航/作业预约；重复 Dispose、重复释放和第二轮运行无残留。
- `MachineExecutionContextEditModeTests` job `77229381`：3/3，通过安全点、硬件移除和暂停投影回归。
- 编辑器结束态：`isPlaying=false`、`isPaused=false`、`isCompiling=false`；8092 health OK；Console 17 条 Log、0 Warning、0 Error。

## 证据

本次原生结果保存为 `retry-8092-motion-effector-result-20260912.xml`。测试输出含两轮稳定日志：`B16 cycle=0` 与 `B16 cycle=1`，并记录 machine/task/request/queued ID。

## 范围与资源

本次没有修改生产实现、场景、Prefab、任务表、xlsx 或 Git；新增仅为客户端 Editor 联合测试和证据。8092 保持可用，Unity 非 PlayMode、非编译/更新。
