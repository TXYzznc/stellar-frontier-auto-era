# Segment 59：传送带编辑器动作预览时间轴

## 根因与修正

- 编辑器动作预览此前只通过`MotionPreviewEvaluator`驱动`MotionGraphAsset`声明的关节；`conveyor_run`声明了各滚筒，但`ConveyorLoopRigPreview`只在Play Mode的`Update`内推进，所以预览窗口中只见滚筒转动。
- 增加`IMotionPreviewPresentation`。`MotionPreviewEvaluator`在同一预览时间轴中通知挂在Rig子树上的表现参与者；传送带据此同步推进连续带体、橙色追踪履带筋和演示货物。
- 编辑器预览首次播放时克隆Prefab带体Mesh为内存预览副本；重置或安全中断恢复Prefab原始Mesh与货物绑定位置，保证预览不写回资产。

## 验证

- `MotionPreviewEvaluatorEditModeTests` — 2/2，新增断言证明同一预览时间会改变连续履带顶点，并能恢复原始网格。
- `MotionPreviewTimelineEditModeTests` — 2/2。
- `FunctionalRigMotionGraphCatalogEditModeTests` — 13/13。
- `ConveyorLoopPresentationEditModeTests` — job `9728a5f9`，2/2。

合计19/19通过，失败0、跳过0、不确定0。结束时8090为Bypass、非PlayMode、未编译、未更新；Console 10 Log、0 Warning、0 Error。快速执行回传未逐项保留前三项jobId，后续执行包须逐项回传jobId；不影响本次已签收结论。
