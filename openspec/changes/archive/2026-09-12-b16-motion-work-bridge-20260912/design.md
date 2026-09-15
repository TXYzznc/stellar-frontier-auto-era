# 设计

`MachineNavigationMotionAdapter` 接收 `MachineNavigation` 的结果与实际载体位姿采样，调用 `MotionExecutor` 的准备/状态/释放接口。`RegionWorkQueue` 与 `EffectorBehaviorQueue<TParameters>` 保持现有权威，桥接只转发预约状态，不引入玩家手动作业、自动重试或物流结算。取消顺序为请求方取消→导航/作业释放→表现恢复；区域退出按算法/桥接→导航/传感器→Context释放。

实施前精确候选：新增 `Assets/Game/Scripts/AutoEra/Motion/MotionWorkBridge.cs` 及对应 Editor 测试；仅必要时修改既有 Motion adapter，禁止正式场景/Prefab和框架文件。联合验证使用隔离临时实例。
