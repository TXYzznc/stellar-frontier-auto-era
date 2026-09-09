# B08 正式低模替换恢复

派发ID：b08-formal-lowpoly-replacement-restore
负责人：client
授权：2026-09-09 用户确认全部低模已通过，取消四项旧Pending，并要求客户端重新执行正式替换。疑似近期变更回退导致成果缺失。

## 目标与顺序

在当前b05 Active完成后优先领取，恢复11项已验收低模在正式项目中的模型、贴图、材质和Entity可见层引用。保持既有已通过资源的验收结论，不恢复旧代理试点、候选复验或旧货舱行程任务。

## 现状与输入

- 当前正式模型中WheeledCarrier、FixedRotaryCarrier、MultiJointArm、RotarySaw、Conveyor与下述交付清单FBX哈希一致；另6项未找到正式低模，Entity目录仅有占位文件。
- ArtResource交付索引：`D:/unity/UnityProject/ArtResource/ArtSource/FirstVersion/B08_RuntimeLowPoly_20260908_v01/RuntimeCandidateHandoff_v03.json`。该旧索引仍标候选，但用户已明确整批验收通过；使用其逐资产路径、版本和哈希定位输入，并核对有无后续最终交付修订。
- 全部对象：WheelModule、WheeledCarrier、CargoPod、FixedRotaryCarrier、MultiJointArm、WaterCannon、RotarySaw、RotaryDrill、SlidingDoor_D24、SlidingDoor_D40、Conveyor。
- 优先恢复可定位的最终Prefab、meta/GUID及已确认动作配置；无法找到时按已有MotionRig合同重建正式可见层绑定。禁止从过时任务摘要推导货舱门改回固定0.75m或0.45m直线位移。

## 写入边界

- 按项目现行资源目录规范写入上述对象对应的`Assets/Game/Models/`、`Textures/`、`Materials/`和`Prefabs/Entity/`精确子目录及meta。
- 允许为恢复既有动作绑定修改相关`Assets/Game/Scripts/AutoEra/`接入代码、MotionRig/MotionGraph资产及相关验证证据；不扩大玩法或重做已验收造型。
- ArtResource只读作为源；保留已存在正式GUID与引用。已一致文件不重复覆盖。
- 不改ScriptsBuiltin、UI资源、任务表/xlsx或Git索引；不自动触发Git集成。旧Development资产本次不做无关批量删除。

## 完成标准

1. 11项逐一列明最终模型、贴图、材质、Entity入口及源版本/哈希。
2. Unity刷新后引用完整、材质与贴图通道正确，相关编译及Console无新增错误。
3. 仅对恢复/变更的绑定执行必要动作接线检查，覆盖轴向、尺度与关键运动；不重开造型验收或旧候选流程。
4. 保存受影响资产与场景，记录恢复清单、缺失来源与实际处理；回传替换完成结果。

执行Unity前按8090当前占用规则核验项目与场景。机械批量工作按快速执行规则评估，原负责人承担恢复正确性。
