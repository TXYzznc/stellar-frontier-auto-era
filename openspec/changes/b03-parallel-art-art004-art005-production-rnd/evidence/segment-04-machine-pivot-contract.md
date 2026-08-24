## ART-005 任务 3.1：机器结构与 Pivot 源级合同

### 结论

- Blender 源已建立职责分明的底盘、四轮独立转向／悬挂、五级机械臂、可替换效应器标识、四个稳定支撑、作业点、净空区和状态灯接口层级。
- 四轮的父子、局部矩阵、薄轴方向和世界 AABB 已通过独立数值复核；稳定标识均为非渲染 Empty。
- 本段只收口源级结构与 Pivot 合同。未导出或覆盖已通过的 Unity Machine V3，未把 Unity Prefab、运行时参数、接地、转向、IK、装卸或安全中断验证计入 3.1。

### 直接证据（ArtResource）

- Blender 源：`ArtSource/ART005_ProceduralMachine/ART005_WheeledMachine_High.blend`
- 源级合同：`Docs/ArtPipeline/ART-005-Machine-Pivot-Contract.md`
- 轮轴与 AABB 数值复核：`Docs/ArtPipeline/Evidence/b03-parallel-art-art004-art005-production-rnd/ART005-WheelAxis-AABB-V2.md`
- Blender 可视证据：`Assets/Art/Evidence/ART004_ART005_VerticalSlice/ART005_Machine_PivotContract_Blender_V1.png`

### 独立数值复核

- DCC 坐标：前方 `+X`、侧向 `+Y`、上方 `+Z`；转向绕 `+Z`，悬挂沿 `Z`，轮滚动绕本地 `+Y`。
- 四轮均符合 `MachineRoot → SteerPivot → SuspensionPivot → WheelMesh`；轮网格 localPosition=`(0,0,-0.40)`、localRotation=`(0,0,0)`、scale=`(1,1,1)`。
- FL／FR／RL／RR 世界 AABB 均为 `(1.04,0.32,1.04)`，薄轴为 Blender `+Y`。
- `StatusSocket` 已由诊断 Mesh 改为 `MachineRoot` 下真正的 Empty；`WorkPoint_Default`、`KeepOut_Chassis` 同为隐藏 Empty，不进入正式渲染和导出选择集。

### 后续边界

- 继续遵守“单件 FBX＋Unity Prefab 权威层级”：不以多级 FBX Empty 在 Unity 中的局部矩阵作为运行时通过依据。
- 任务 3.2 负责参数模板、默认／安全姿态、锚点与接地点配置；3.3～3.6 分别承担地形接触、转向／重定位、支撑／IK 和效应器安全流程验证。
