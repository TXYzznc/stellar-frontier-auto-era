# Segment 56：传送带运行时绑定修复

## 根因与修复

- 验收场Director此前以`VisualRoot/Joint_*`查找关节，漏掉实例层级中的`RigRoot`，因此不能绑定传送带滚筒等可见关节。
- 传送带履带节与演示货物改由Prefab内`ConveyorLoopRigPreview`直接驱动；其只消费共享的闭环几何和带速，不读取或结算物流权威。
- Director的关节查找修正为`RigRoot/VisualRoot/Joint_*`并保留递归回退；停止播放时跳过已销毁节点，避免恢复绑定姿态出现空引用。

## 运行时实测

8090 Play Mode连续采样中：`BeltTread_00`由约`z=-5.16`移动到约`z=-4.90`；`DemoCargo_00`由约`z=-4.92`移动到约`z=-4.90`。停止Play Mode后 Console Error=0。

## QA

- `FunctionalRigPrototypeCatalogEditModeTests` — job `e57aa127`，1/1。
- `ConveyorLoopPresentationEditModeTests` — job `fa14a19d`，2/2。
- `FunctionalRigAcceptanceDemoDirectorEditModeTests` — job `a9bcebd3`，1/1。
- `FunctionalRigMotionGraphCatalogEditModeTests` — job `acaefb9b`，13/13。

合计17/17通过，失败0。Unity 8090为Bypass、非PlayMode、未编译、未更新；Console 0 Warning、0 Error。
