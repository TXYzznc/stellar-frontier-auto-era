## ART-005 任务 3.3：四轮接地与通行边界表现样机

### 结论

- 独立验证实例已完成四轮非分配地面采样、受限悬挂、四点车体姿态、实际位移轮转、35°连续坡度门槛和轮半径台阶阈值的纵向验证。
- 35°连续坡与 0.52m 台阶允许表现拟合；0.53m 台阶拒绝表现拟合并恢复基线，不强行贴地越障。
- 本段是 ArtResource 表现样机，不实现玩法通行裁决、导航、刚体物理或正式客户端移动；逻辑层仍是可通行性权威。

### 直接证据（ArtResource）

- 隔离组件：`Assets/Art/Authoring/ART005_ProceduralMachine/Scripts/ART005GroundingPreview.cs`
- 数值与失败迭代记录：`Docs/ArtPipeline/Evidence/b03-parallel-art-art004-art005-production-rnd/ART005-3.3-GroundingPreview-NumericEvidence.md`
- 平地：`Assets/Art/Evidence/ART004_ART005_VerticalSlice/ART005_GroundingPreview_Flat_V3.png`
- 35°连续坡：`Assets/Art/Evidence/ART004_ART005_VerticalSlice/ART005_GroundingPreview_Slope35_V4.png`
- 0.52m 台阶通过：`Assets/Art/Evidence/ART004_ART005_VerticalSlice/ART005_GroundingPreview_Step052_Pass.png`
- 0.53m 台阶拒绝：`Assets/Art/Evidence/ART004_ART005_VerticalSlice/ART005_GroundingPreview_Step053_Reject.png`

### 数值与实现复核

- 平地静止：四轮接地，累计距离 `0m`，Raycast 缓冲未溢出。
- 受控移动 `1.04m`：累计距离 `1.04000092m`，四轮各转约 `114.5919°`，与 `1.04 / 0.52 × Rad2Deg = 114.5916°` 一致；停止后数值不再增长。
- 关闭编辑器预览后，车体表现根和轮子恢复缓存基线；自身贴地补偿不会被计为行驶距离。
- 每轮使用固定八命中缓冲的 `Physics.RaycastNonAlloc`，高频采样不使用 `RaycastAll` 分配。
- 世界地面法线显式转换回表现根父空间，避免非零父旋转时混用世界／局部旋转。
- Unity 8091 普通编译完成，Console Error=`0`；最终场景原生保存后 `isDirty=false`、20 个根对象，原 `MachineRoot_V3` 未改动。

### 可视门禁与失败边界

- `Slope35.png` 因相机位于坡板下方而失败，`Slope35_V2/V3` 为返修过程；只有 `Slope35_V4` 进入通过证据。
- `Step053_Candidate.png` 与 `Step053_Candidate_V2.png` 分别为空构图和背景污染候选，不进入正式证据。
- 3.3 的 35°与0.52m规则属于机器连续坡／台阶边界，不与 ART-004 建筑入口坡道混用。
