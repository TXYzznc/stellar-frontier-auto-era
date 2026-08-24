## ART-005 任务 3.4：四轮转向与重定位安全收口样机

### 结论

- 独立 V2 验证副本已建立四个 Unity 权威 `SteerPivot` wrapper；每个 wrapper 均包含对应 Hub、Suspension 和 Wheel，并以实际导入轮心作为轴心。
- 已验证前后轮反向转向、四轮同向低速蟹行、大幅移动收臂规则、抬起对位姿态的有限补偿，以及 `RepositionRequired` 安全保持边界。
- 本段不实现导航、路径规划、目标求解或玩法权威；高速同向转向保持禁用，黄色世界内状态灯留给任务 4.4。

### 直接证据（ArtResource）

- 隔离组件：`Assets/Art/Authoring/ART005_ProceduralMachine/Scripts/ART005SteeringSafetyPreview.cs`
- 数值、失败迭代与状态边界：`Docs/ArtPipeline/Evidence/b03-parallel-art-art004-art005-production-rnd/ART005-3.4-SteeringSafetyEvidence.md`
- 反向转向：`Assets/Art/Evidence/ART004_ART005_VerticalSlice/ART005_V3_Opposite.png`
- 低速蟹行：`Assets/Art/Evidence/ART004_ART005_VerticalSlice/ART005_V3_Crab.png`
- 重定位安全保持：`Assets/Art/Evidence/ART004_ART005_VerticalSlice/ART005_V3_Reposition.png`

### 数值与恢复复核

- Travel：四轮 `0°`；Opposite：前轮 `+35°/+35°`、后轮 `-35°/-35°`；Crab：四轮 `+35°`；Disable：四轮恢复 `0°`。
- 两轮完整状态循环后角度结果一致，无基线累积漂移；`highSpeedSameDirectionSteeringAllowed=false`。
- 四个 Wheel 中心到对应 Pivot 的世界距离在各状态均为 `0.0m`，满足 `≤0.001m` 门禁；转向只改变朝向，不再绕错误远轴公转。
- 缺失一个 Pivot 与完整节点两种回归均为 Console Error=`0`；缓存只有在四 Pivot、Yaw、Shoulder 全部有效时才启用。
- 最终场景原生保存后 `isDirty=false`、21 个根对象；原 `MachineRoot_V3` 未修改，错误 V1 rig 停用保留。

### 可视与失败边界

- V1 三图中轮组脱离底盘，明确为失败证据；V2 修复轮心轴但仍含无关坡道背景，只作候选。
- V3 三图隔离所有地形代理和失败 rig；轮组保持与底盘连接，重定位图以较高侧前视角显示机械臂 `12°/8°` 有限安全保持。
- `RepositionRequired` 只允许支撑收回后的低速微调，机械臂保持或回到安全姿态；不得强行伸到极限、忽略 KeepOut 或暗示已完成导航重定位。
