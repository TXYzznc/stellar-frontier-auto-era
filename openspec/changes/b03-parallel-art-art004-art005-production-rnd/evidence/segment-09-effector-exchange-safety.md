# 第 9 段：效应器交换与安全阶段恢复

## 完成范围

- 完成 ART-005 任务 3.6 的隔离样机验证；未修改既有 `MachineRoot_V3` 与 3.5 通过实例。
- 以 `Approach → Unlock → Detach → TransferSafeHold → Attach → Lock → Confirm → Complete` 表达参数化效应器交换阶段。
- `stageProgress` 使用 0–1 平滑曲线预览阶段内运动；锁环、工具位置、事件标记和状态反馈由同一阶段合同驱动。
- 使用 `InterruptReason` 区分取消、断电、机械故障和确认故障；取消／断电为黄色安全待处理，机械／确认故障为红色。
- 工具与状态颜色通过 `MaterialPropertyBlock` 呈现，不修改共享材质资产。

## 安全边界证据

### 完整成功

- `stage=Complete`，夹具离开 Socket，钻头完成接入并锁定；
- `drillSocketed=true`，`clampSocketed=false`，状态为绿色；
- 事件标记为 `Exchange complete`。

截图：`Assets/Art/Evidence/ART004_ART005_VerticalSlice/ART005_EffectorExchange_Complete_V7.png`。

### 脱离前取消回滚

- `stage=InterruptedBeforeDetach`，`interruptReason=Cancel`；
- 夹具回到中央 Socket 并重新锁定，钻头保留在右侧工具槽；
- `clampSocketed=true`，黄色状态反馈。

截图：`Assets/Art/Evidence/ART004_ART005_VerticalSlice/ART005_EffectorExchange_PreDetachRollback_FinalV3.png`。

### 脱离后断电安全待处理

- `stage=DetachedSafeHold`，`interruptReason=PowerLoss`；
- 中央 Socket 为空，橙灰夹具与青灰钻头分别位于两个无遮挡安全槽位；
- 两个 `socketed` 标志均为 `false`，黄色状态反馈。

截图：`Assets/Art/Evidence/ART004_ART005_VerticalSlice/ART005_EffectorExchange_DetachedSafeHold_FinalV3.png`。

### 恢复与确认故障

- `RecoveryComplete` 使钻头重新接入 Socket，恢复绿色完成态；
- `Confirm + ConfirmFault` 保持工具姿态并显示红色确认故障；
- 取消、断电和故障均只在安全阶段边界收口，不在中间插值点冻结工具。

截图：

- `Assets/Art/Evidence/ART004_ART005_VerticalSlice/ART005_EffectorExchange_RecoveryComplete_V7.png`
- `Assets/Art/Evidence/ART004_ART005_VerticalSlice/ART005_EffectorExchange_ConfirmFault_V2.png`
- `Assets/Art/Evidence/ART004_ART005_VerticalSlice/ART005_EffectorExchange_DetachCurveMidpoint_V3.png`

## 可视复核

- 第一轮远机位无法清楚区分脱离前与脱离后工具位置，未作为通过证据。
- 最终固定近机位下，制作人已逐图复核：脱离前夹具明确位于 Socket；脱离后 Socket 清空，夹具与钻头分别位于两个独立安全槽位。
- 橙灰夹具与青灰钻头仅作为纵向样机识别代理，不是正式量产造型或材质。

## 权威证据位置

- ArtResource 数值记录：`Docs/ArtPipeline/Evidence/b03-parallel-art-art004-art005-production-rnd/ART005-3.6-EffectorExchange-NumericEvidence.md`
- 阶段合同：`Docs/ArtPipeline/ART-005-Effector-Exchange-Contract.md`
- 验证脚本：`Assets/Art/Authoring/ART005_ProceduralMachine/Scripts/ART005EffectorExchangePreview.cs`
- 验证场：`Assets/Art/LookDev/ART005_ProceduralMachine_Validation.unity`
- 最终保存复核：活动场景路径正确，`isDirty=false`，45 个根对象。

## 边界

- 本段验证程序化表现合同和安全阶段恢复，不实现库存、自由装配、完整碰撞、玩法或网络权威。
- 历史候选图保留用于审计，不能替代本段列出的最终证据。
- 用户任务表未读取写入、未暂存、未提交。
