## Context

依据 `Docs/Development/Dispatch/Active/b13-wheeled-navigation-and-avoidance.md`、现行机器自动化与行为容量设计，以及 B12 同身份区域桥接。机器仍归 MachineRoster；区域表示不重复分配 ID。预约仅表示通道所有权，不能表示实际抵达。矿脉进入有效区域后停稳向下作业，不新增固定钻位；水枪包络来自弹道，不恢复 4 米限制。

现有 Hotfix 与 AutoEra Editor/PlayMode 测试程序集可用；Packages 已有 `com.unity.modules.ai`，无需新包/asmdef。正式轮式入口为 `Assets/Game/Prefabs/Entity/Machines/WheeledCarrier.prefab`，仅在测试中实例化，不改资产。

## Goals / Non-Goals

**Goals:** 载体移动生命周期、受保护算力、NavMesh 静态绕障/基础避让、自动作业接近与真实 Motion 投影；独立可重复测试。

**Non-Goals:** 玩家路径语言、完整物流/编队/交通、矿产/灌溉结算、UI、模型修改、正式场景改造、包安装。B11/G2 整体不随本单元完成。

## Decisions

1. **纯状态机＋注入驱动**。比较直接把全部逻辑写进 NavMeshAgent MonoBehaviour（难以控制时钟和复现算力故障）、自研寻路/避障（超范围且重复引擎能力）、独立状态机＋NavMesh 驱动（采用）。`MachineNavigation` 使用显式 Tick 和驱动接口；Unity 驱动仅负责路径与姿态，不能裁决任务成功。
2. **复用算力与任务活动**。每载体最多一个导航活动，不建立移动效应器；向既有 MachineTaskQueue 登记活动并监听取消。初始/重规划请求 10，移动持续 5 且不可让出；自动 rePath 禁用，由服务按预算触发。重规划等待算力时立即停下，单个请求去重。运动受阻累计 3 秒尝试，连续 10/30 秒无法恢复警告/失败；纯算力初始等待与预约等待不冒充无路径失败。
3. **作业区域候选＋预约**。目标描述提供区域、体积上限、所需能力、面向及可替换包络判定，不按效应器复制导航节点。预计算有限候选，选择可达路径；矿脉使用内部区域候选；水枪由调用方提供弹道包络判定，不由导航硬编码距离。RegionWorkQueue 负责优先级/FIFO，等待期间不反复寻路；可提供公开等待位置，至多规划一次前往等待位置，到达后释放移动算力但不冒充作业抵达；等待位置不可达则在当前安全位置继续等候。无等待位置时原位等待。获批后才规划作业接近。抵达后预约由导航所有者保留到显式作业释放/取消/退出，不因移动结束提前让第二台机器进入。
4. **身份与清理**。机器断电停下并保留可恢复请求；目标/区域失效、回收、取消、抢占、Dispose 终结一次，释放等待或运行算力及预约。执行上下文增加导航活动关联，避免机械运动中被当作可拆硬件的空闲实例。区域姿态更新沿用同 ID，不重复注册；资源有效区域与建筑占地校验区别处理。
5. **实际运动投影**。累计实际位置变化驱动轮周位移，角度由转向/实际朝向变化得出；不使用预览时间作为移动权威。NavMeshAgent 的路径/位移与停稳阈值共同决定抵达，再完成所需朝向。
6. **候选路径清单**。新增 `Machines/MachineNavigationContracts.cs`、`MachineNavigation.cs`、`UnityMachineNavigationDriver.cs`；必要修改 `MachineExecutionContext.cs`。区域新增 `World/Region/InitialRegion.Navigation.cs`、必要扩展 `RegionWorkQueue.cs`；新增 `Motion/Adapter/MachineNavigationMotionAdapter.cs`；测试 `Tests/AutoEra/Editor/MachineNavigationEditModeTests.cs`、`MachineNavigationIntegrationTests.cs` 及对应 meta。测试场景/截图仅写测试或本 evidence 范围。

## Risks / Trade-offs

- NavMesh 局部避障不是全局交通保证 → 记录两机验证规模和卡住恢复，不宣称无限规模保证。
- 同步算力事件可重入 → 服务 Tick 检查请求状态，不在 Started 回调递归推进；终态先置位再通知。
- NavMesh 边缘采样可能把作业位置投射出有效区 → 采样后重新检查包络与候选区域，不以投射成功代替合法性。
- 测试会进入 PlayMode → 先核验 8090 和场景保存状态，临时实例/NavMeshData 对称清理，不覆盖用户场景。
- 实际正式组合根尚未新增授权 → 隔离验证后如需接入，先列精确现有路径申请；该项保持未完成，不伪称运行入口已交付。

## Migration Plan

增量加入产品服务与测试，无存档迁移。所有新资源通过普通 Unity 编译生成 meta。验证 B11/B12 回归；不自动 Git。回滚仅撤回本单元文件与明确改动，保留其它窗口现场。

## Open Questions

已授权服务与隔离验证无待决问题。2026-09-11 制作人已在原派发精确追加 `Assets/Game/Scene/InitialRegion.unity` 必要引用保存权限：`InitialRegionScene._navigationGround` 绑定现有区域地面 MeshFilter，使用原 World/Region 范围内 `RegionNavigation` 构建/释放导航数据；不新增包或全局配置。对合法运行实例显式 `Navigation.Bind`，不自动创建机器或移动演示。退出清理、再次初始化、障碍拓扑变化按事件重建；同步构建没有异步旧回调复活风险。新增 `RegionNavigation.cs` 和相应集成测试属于本段精确文件清单。
