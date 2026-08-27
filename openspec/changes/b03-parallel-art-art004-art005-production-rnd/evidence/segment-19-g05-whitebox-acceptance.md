# Segment 19 — G05 地形适配白模验收

日期：2026-08-27
范围：正式基础脚、0.25m／0.5m台阶、20°坡道、裙边、填充件，以及独立35° R&D坡道代理。

## 制作人验收结论

- G05 白模通过技术验收，OpenSpec任务 `2.3` 完成；允许按既定顺序进入G07，未放行其他后续组。
- 正式候选与35° R&D代理采用独立Blender源、候选目录、FBX和Unity验证场；正式场内未发现Ramp35对象，R&D场只含R&D坡道版本。
- 正式六件均为纯模型导入，基础脚数量／位置／行程仍保持待定；临时四脚与平台/地面代理只作为接触验证，不构成正式配置冻结。
- 台阶按0.25m→0.5m外置入口关系连续摆位；裙边和填充件只承担侧缘封缝／补洞，不作为平台中央承重结构。
- 首轮20°与35°坡道因边缘水平悬浮、20° Nose脱离坡面而退回。V02已将正式边缘旋转约20°、R&D边缘旋转约35°并与坡面端点连续贴合；正式Nose已移至坡面高端实体位置，R&D标识与高端实体接触。
- V02 Unity bounds显示正式坡面和边缘共享Y/Z覆盖，R&D坡面与边缘同样共享Y/Z覆盖；新近景中未发现无支撑悬浮。
- 正式与R&D部署根均为identity／单位缩放，各自 `DCCImportRoot` 是唯一Y=180°静态视觉轴补偿；V01实例停用、V02实例活动。
- 正式Ramp20 V02与R&D Ramp35 V02 Importer均为 `globalScale=1`、`importAnimation=false`、`materialImportMode=None`、`meshCompression=Off`；Console Error为0。
- 两套验证场均已保存为 `isDirty=false`；最终复验后活动场为正式场。

## 证据索引

- 自检：`ArtSource/ART004_ModularWorkshop/G05_TerrainAdaptation/Evidence/G05_TerrainAdaptation_SelfCheck_V01.md`
- 正式场：`Assets/Art/Authoring/ART004_ModularWorkshop/G05_TerrainAdaptation/Validation/ART004_G05_TerrainAdaptation_Formal_Validation_V01.unity`
- R&D场：`Assets/Art/Authoring/ART004_ModularWorkshop/G05_TerrainAdaptation/Validation/ART004_G05_Ramp35_RND_Validation_V01.unity`
- 正式20° V02：`Assets/Art/Authoring/ART004_ModularWorkshop/G05_TerrainAdaptation/Formal/Models/ART004_G05_Ramp20_V02_Continuous.fbx`
- R&D 35° V02：`Assets/Art/Authoring/ART004_ModularWorkshop/G05_TerrainAdaptation/RND/Models/ART004_G05_Ramp35_RND_V02_Continuous.fbx`
- 正式连续证据：`Assets/Art/Authoring/ART004_ModularWorkshop/G05_TerrainAdaptation/Formal/Evidence/G05_Formal_Ramp20_Continuous_V06.png`
- R&D连续证据：`Assets/Art/Authoring/ART004_ModularWorkshop/G05_TerrainAdaptation/RND/Evidence/G05_Ramp35_RND_Continuous_V04.png`

## 已知边界

- 35°仅是R&D极端可视／部署代理，不进入正式入口库，也不对ART-005机器通行能力作结论。
- 本轮不实现Terrain系统、物理接地、导航、碰撞权威、材质贴图或建筑主体修改。
