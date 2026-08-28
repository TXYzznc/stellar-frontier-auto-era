# Segment 10：G06独立Play Mode验收预览

- 范围：仅在既有G06验证场增加专用、可回退的验收预览层。
- 脚本：`Assets/Art/LookDev/ART005_G06_FormalCarrier_Validation/RuntimePreview/ART005G06CarrierDemoPreview.cs`。
- 场景对象：`G06_RuntimePreviewCarrier`、专用Camera与Ground。
- 控制：默认自动分段；`Space`暂停／继续；左右方向键切段；`R`重置。
- 驱动范围：已通过的四轮Steer／Roll Pivot与`CargoDoorMotionPivot`。
- 恢复验证：退出Play Mode后货舱门恢复原父级和绑定态。
- 编译验证：Unity编译Error为0。
- 隔离边界：未修改V17/V18源FBX、旧验证对象、正式玩法、导航、碰撞或存档。
- ArtResource记录：`G06-ModelingExecutionRecord`已补充本段实施记录。
- 结论：满足当前ART-005单资产验收预览任务；不构成`b03-parallel-program-art004-art005-animation-demo`七段完整演示的开始或完成证据。
