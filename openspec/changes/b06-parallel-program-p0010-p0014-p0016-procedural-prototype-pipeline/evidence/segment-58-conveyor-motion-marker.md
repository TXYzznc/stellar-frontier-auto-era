# Segment 58：传送带网格内运动追踪筋

## 根因与修正

- 单一连续闭环带体Mesh已按带速更新凸起履带筋顶点；但带体和履带筋同色，连续带面会掩盖运动，导致视觉上难以辨识实际推进。
- 传送带仍只有一个MeshFilter和一张连续闭环Mesh。Mesh内拆为带体与运动追踪筋两个submesh；每九条凸起履带筋中一条使用橙色追踪材质。
- 追踪筋不创建`GameObject`，不生成独立路径，不拥有独立速度；其顶点与其余履带筋共用相同的闭环采样和`线位移 = 滚筒半径 × 角位移`关系。

## 自动验证

- `FunctionalRigPrototypeCatalogEditModeTests` — job `3978ff1f`，1/1；覆盖一个连续Mesh、两个submesh、两材质槽以及不存在`BeltTread_*`子对象。
- `ConveyorLoopPresentationEditModeTests` — job `9f092b37`，2/2；覆盖闭环和滚筒弧长同步。
- `FunctionalRigAcceptanceDemoDirectorEditModeTests` — job `24c72c4d`，1/1；覆盖验收场绑定。

合计4/4通过，失败0。结束时8090为Bypass、非PlayMode、未编译、未更新；Console 10 Log、0 Warning、0 Error。
