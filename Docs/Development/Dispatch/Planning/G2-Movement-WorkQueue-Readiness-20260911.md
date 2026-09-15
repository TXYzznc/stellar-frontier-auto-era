# G2 移动与作业下一工作单元准备

日期：2026-09-12。本文是只读盘点交付，不构成实现授权、OpenSpec 或任务表修改。

## 现有可复用能力与证据

| 能力 | 真实入口 | 当前边界 |
|---|---|---|
| 机器生命周期与唯一身份 | `MachineInstance`、`MachineRoster`、`InitialRegion.DeployMachine` | 已覆盖部署、供电、运行状态和永久对象身份；不等同于玩家完整建造流程 |
| 计算与指令队列 | `MachineExecutionContext`、`MachineTaskQueue`、`MachineComputePool` | B15 已验证有界队列、算力等待、取消、释放；算法服务不替代任务结果权威 |
| 传感器公开读数 | `MachineSensorSet`、`MachineSensor`、`RegionSensorEnvironment` | 显式绑定、代次门禁和区域释放已验证；不提供未实现的农业/能源生产端点 |
| 寻路 | `MachineNavigation`、`RegionNavigation`、`MachineNavigationSettings` | 真实 NavMesh 导航目标与结果已在 B15 联合测试验证；不是完整避障、道路规划或车辆动力学 |
| 动作表现 | `MotionRig`、`MotionGraphAsset`、`MotionExecutor`、`MachineNavigationMotionAdapter` | 负责关节/履带/机构表现和确定性预览；不读取算法图、不结算玩法、不决定导航结果 |
| 作业申请 | `RegionWorkQueue`、`EffectorBehaviorQueue<TParameters>` | 有排队/释放边界；尚未形成 G2 玩家作业编排与跨区域物流闭环 |

## 已验证范围

- B06：代表性 MotionRig/MotionGraph、轮组、机械臂、效应器、货舱、传送带和固定旋转载体动作合同与预览已通过相关 EditMode 回归；这些是表现与合同代理，不是导航或生产权威。
- B10/B11/B13/B14：区域、机器身份、硬件状态、寻路、传感器和 UI 观察层已有实现/验证；UI Form 仅观察状态，不能直接替代算法或作业按钮。
- B15：基础算法服务支持显式传感器→判断→任务→真实导航→结果历史链；不包含完整玩家算法编辑器、五套生产模板、磁盘离线推进、农业/能源/物流生产权威。

## 未接通链与最小下一单元

当前最小、可独立验收且不依赖 UI/美术的链是：

1. **导航结果到动作表现桥接**：以已存在的 `MachineNavigationMotionAdapter` 和 `MotionExecutor` 为边界，将导航状态（准备、行进、到达、取消、失败）映射到一个已绑定 MotionRig 的表现请求；表现完成不得改变导航/任务终态。
2. **作业安全点桥接**：使用 `RegionWorkQueue.Request/Release` 与 `EffectorBehaviorQueue<TParameters>`，验证作业申请等待、取消、机器退出释放，以及表现层只消费状态。
3. **联合回归**：真实 `InitialRegion`、机器实例、NavMesh、一个效应器和一条 MotionGraph，证明导航、作业、表现的身份/请求ID一致，关闭观察 UI 不取消工作，退出释放无残留。

上述单元都不需要新导航系统，也不需要玩家手动按钮；需要先建立独立 OpenSpec 明确任务 ID、结果权威、作业参数和取消语义后才能实现。

## 真实设计待决项

- G2 是否允许玩家直接配置“移动到目标”指令，还是仅由算法服务产生移动意图（现行文档偏向后者）。
- 移动失败、部分到达、阻挡和不可达时，任务状态与表现恢复的唯一权威及是否允许自动重试。
- 作业申请的目标引用、占用/预约、可安全取消边界，以及作业表现结束与任务完成的先后关系。
- 多机器同一区域的避让/优先级是否属于本阶段；当前 `MachineNavigation` 不应被扩写成完整交通系统。

## 禁止路径与验收建议

禁止修改 `ScriptsBuiltin`、正式场景/Prefab、任务表和任何 xlsx；禁止把 MotionGraph 当作导航权威，禁止由 UI 直接推进作业，禁止提前实现物流/农业/能源生产或离线存档。

后续 OpenSpec 的自动验收至少应覆盖：唯一机器/任务/请求 ID；导航结果与表现状态不互相改写；作业队列 FIFO/容量/取消/释放；机器断电、区域退出和 UI 关闭；重复运行无残留；普通编译、Console Error=0、Missing Reference=0、严格边界审计。UI 无依赖部分可先行，正式 UI 接线继续遵守 B05 前置。
