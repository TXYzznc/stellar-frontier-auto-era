# Segment 57：传送带单一连续闭环网格

## 已确认的表现合同

- 传送带带体不再由独立 `BeltTread_*` 方块组成。`Geometry_belt_loop_visual`仅使用一个MeshFilter／MeshRenderer承载一个名为“连续闭环传送带”的Mesh。
- 该Mesh内含64段连续带体和36条横向凸起履带筋；凸起筋只是在同一Mesh中的顶点区间，沿同一闭环更新，不是独立GameObject。
- 闭环仍含上行载料段、端部回绕和下行回程段。演示货物是独立可视物，只在上行段按相同线速度前进；逻辑物流、货物归属和存档仍不在本原型中结算。
- 带速保持0.9m/s；主动／从动滚筒半径0.25m、支承滚筒半径0.14m，各自按 `角位移 = 线位移 / 半径` 转动。

## 本地运行时检查

- 8090 Bypass Play Mode中按正则查询 `BeltTread_` 得到0个对象；`ConveyorLoopRigPreview`仅绑定在 `Geometry_belt_loop_visual`。
- `DemoCargo_00`两秒内从约 `(10.70, 1.12, -4.93)` 移至 `(10.47, 1.12, -4.30)`，仍在上行工作带面运动。
- 停止Play Mode后 Console Error=0。

## 自动回归

- `FunctionalRigPrototypeCatalogEditModeTests` — job `fd26e732`，1/1。
- `ConveyorLoopPresentationEditModeTests` — job `23c75413`，2/2。
- `FunctionalRigAcceptanceDemoDirectorEditModeTests` — job `488ac309`，1/1。
- `FunctionalRigMotionGraphCatalogEditModeTests` — job `f852c254`，13/13。

合计17/17通过，失败0、跳过0、不确定0。结束时8090为Bypass、非PlayMode、未编译、未更新；Console 10 Log、0 Warning、0 Error。
