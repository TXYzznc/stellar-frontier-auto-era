# 第 8 段：稳定支撑与机械臂安全对位

## 完成范围

- 完成 ART-005 任务 3.5 的 V4 隔离样机验证；未修改既有 `MachineRoot_V3`。
- 固定运行时权威链：`YawPivot_Unity → ShoulderPivot_Unity → ExtensionMount_Unity → ExtensionSlider_Unity → WristPivot_Unity → Socket_Effector_Unity`。
- 固定绑定合同：Yaw／Shoulder／Wrist 为 identity，`ExtensionMount_Unity.localPosition=(1.2,0,0)`，Slider 为零位，Socket 为 `localPosition=(0.25,0,0)`，全链单位缩放。
- DCC 导入层仅提供几何；7 个 `ART005_RuntimeVisual_*` 共享网格副本按 DCC Pivot 相对矩阵映射到 canonical wrapper。Clamp 保留 DCC 相对 Socket 的 `+0.200006m` 几何偏移，并由腕部适配器与 Socket collar 提供连续可见连接。
- 支撑采用固定缓冲 `Physics.RaycastNonAlloc`，每次批处理前只调用一次 `Physics.SyncTransforms()`；未伪造接地状态，未引入完整受力、导航、玩法或网络权威。

## 运行态证据

### FK 可达正例

- `groundedCount=4`，`supportsLocked=true`，`wheelsLocked=true`；
- `targetReachable=true`，`repositionRequired=false`；
- Yaw／Shoulder／Extension／Wrist：`14.87066° / 14.87241° / 0.3003566m / 20.12763°`；
- Socket 位置／法线／接近方向误差：`0.0075626m / 0.071326° / 0.128204°`；
- `visualCloneCount=7`，制作人肉眼复核可见链连续。

截图：`Assets/Art/Evidence/ART004_ART005_VerticalSlice/ART005_SupportArm_Flat_Reachable_V11.png`。

### 不可达安全保持

- 切换到 `ART005_SupportTarget_V4_Unreachable` 后保持 `groundedCount=4` 与支撑／轮锁；
- `targetReachable=false`，`repositionRequired=true`，`safePoseDistance=4.825798m`；
- 实际 Transform 复核 Yaw／Shoulder／Wrist 回到 identity，Slider 回到零位，可见链持续连续。

截图：`Assets/Art/Evidence/ART004_ART005_VerticalSlice/ART005_SupportArm_Unreachable_SafeHold_V12.png`。

### 恢复回归

- 切回 FK 目标后全部数值逐项恢复到正例值；
- 7 个 canonical visual 副本保持连续，无累积漂移。

截图：`Assets/Art/Evidence/ART004_ART005_VerticalSlice/ART005_SupportArm_Recovery_FK_V13.png`。

## 失败迭代与根因

- V6：导入臂件未形成运行时权威可见链；
- V8／V9：旧副本局部矩阵未刷新，夹具悬浮；
- V7 Candidate：仅为编辑态候选，未替代 Play Mode 物理与可视门禁。

最终收口：Model FBX 子层级不承担运行时 Pivot 权威；采用固定 canonical wrapper、DCC 相对矩阵映射及共享网格呈现副本。新增编辑态 Collider 未进入旧 PhysicsScene 的问题通过普通编译／域重载和真实 Play Mode 射线验证排除。

## 权威证据位置

- ArtResource 数值记录：`Docs/ArtPipeline/Evidence/b03-parallel-art-art004-art005-production-rnd/ART005-3.5-SupportArm-NumericEvidence.md`
- 夜间过程记录：`Docs/ArtPipeline/Evidence/b03-parallel-art-art004-art005-production-rnd/nightly-2026-08-24.md`
- 验证场：`Assets/Art/LookDev/ART005_ProceduralMachine_Validation.unity`
- 最终保存复核：活动场景路径正确，`isDirty=false`，39 个根对象。

## 边界

- 本段是 ArtResource 纵向样机与程序化表现合同证据，不是主工程玩法 IK、碰撞、导航或网络权威实现。
- 失败候选图保留用于审计，不能作为通过证据。
- 用户任务表未读取写入、未暂存、未提交。
