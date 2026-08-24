## ART-004 任务 2.3：Terrain 适配样机

### 结论

- 通过范围：同一水平入口平台可分别配合基础脚、连续台阶、20°／35°入口坡道、裙边／填充件和不平地形代理使用；建筑主体本身未为各测试条件改形。
- 20°／35°件按建筑入口坡道验收，不与 ART-005 机器的 35°连续坡度通行门槛混用。
- 本段仅完成模块语义和实例级装配门禁，不代表最终材质、LOD、性能或完整 ART-004／005 验收通过。

### 直接证据（ArtResource）

- 20°入口坡道：`Assets/Art/Evidence/ART004_ART005_VerticalSlice/ART004_TerrainAdaptation_Ramp20_SemanticGate_Side_V7.png`
- 35°入口坡道：`Assets/Art/Evidence/ART004_ART005_VerticalSlice/ART004_TerrainAdaptation_Ramp35_EntranceGate_Side_V12.png`
- 不平地形、不同长度基础脚与裙边／填充：`Assets/Art/Evidence/ART004_ART005_VerticalSlice/ART004_TerrainAdaptation_UnevenFill_Gate_Profile_V13.png`
- 连续 0.25m／0.5m 入口台阶：`Assets/Art/Evidence/ART004_ART005_VerticalSlice/ART004_TerrainAdaptation_Step_EntranceGate_V2.png`
- 过程、失败迭代与源文件记录：`Docs/ArtPipeline/Evidence/b03-parallel-art-art004-art005-production-rnd/nightly-2026-08-24.md`

### 数值与场景复核

- Unity 场景：`Assets/Art/LookDev/ART005_ProceduralMachine_Validation.unity`。
- 不平地形四脚分为两档：前组 `scaleY=0.61`，后组 `scaleY=0.41`；高低地形代理中心高度分别为 `Y=-0.5` 与 `Y=-0.4`。
- 平台根保持水平；基础脚承担承重，裙边／填充件遮蔽暴露缝隙，入口台阶／坡道位于平台外侧而非中央托举。
- 最终保存复核：`isDirty=false`，当前场景 15 个根对象；未切换场景、删除历史失败区或修改主工程。

### 失败证据边界

- `ART004_TerrainAdaptation_Unity_V1.png`、`ART004_TerrainAdaptation_Unity_Overview_V2`～`V5`、早期 AABB／Ramp20-Ramp35 对照和语义 V5 图均保留为失败迭代，不得作为 2.3 通过证据。
- `ART004_TerrainAdaptation_Ramp20_ProxyGate_V8/V9` 属于已撤回的 Terrain 代理过度约束尝试，不进入正式合同。
