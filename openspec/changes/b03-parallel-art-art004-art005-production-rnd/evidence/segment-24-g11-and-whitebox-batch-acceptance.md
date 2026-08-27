# Segment 24：G11 与九组白模批次验收

日期：2026-08-27
结论：G11 通过制作人技术验收；G02→G03→G04→G05→G07→G08→G09→G10→G11 九组顺序白模批次完成。

## G11 合同复核

- G11 保持独立 R&D 身份，不替代 G01 物流点、等待点、输出缓存，也未接入正式物流、管线玩法或玩家建造系统。
- `Conveyor_Root` 为 identity；`PresentationRoot` 承载带面表现；`RollerPivot_A/B=(0,.8,±1.8)`，两端 RollerVisual local0、scale1，滚轮源级轴向已应用。
- `LoadPoint=(0,.8,-2.2)`、`UnloadPoint=(0,.8,+2.2)`、`BlockPoint=(0,.8,0)` 均为 Conveyor_Root 直接子级非渲染锚点。
- 首版护边缺少立柱的失败迭代未作为最终候选；V02 增加四角 GuardPost，形成护边→立柱→下梁→支脚→脚垫→地面的连续支撑链。
- Belt/ChainPlate 由 Rails/CrossBrace 承接；Status_Housing 通过 SupportStrut 挂接机架，Green/Yellow 灯均附着 Housing，未发现悬浮灯点或游离散件。
- Normal、YellowBlocked、Recovery 仅作为 Blender 结构取证；BlockProxy 未进入 Unity，未实现物流判定、库存、运行时速度或货物生成。
- 五份纯模型 Importer 均为 scale=1、无动画、无材质、无 Collider、`meshCompression=Off`；Console Error/Warning 为 0；验证场保存后 `isDirty=false`。

## G11 证据

- 自检：`ArtSource/ART005_BuildingMechanisms/G11_RndConveyor/Evidence/G11_RndConveyor_SelfCheck_V01.md`。
- Blender 结构候选：`ArtSource/ART005_BuildingMechanisms/G11_RndConveyor/Evidence/G11_RndConveyor_BlenderPreview_V02_Supported.png`。
- Blender 三态：同目录 `G11_RndConveyor_{Normal,YellowBlocked,Recovery}_BlenderPreview_V02.png`。
- Unity 候选：`Assets/Art/Authoring/ART005_BuildingMechanisms/G11_RndConveyor/Evidence/G11_RndConveyor_Unity_Assembly_V01.png`。
- 验证场：`Assets/Art/Authoring/ART005_BuildingMechanisms/G11_RndConveyor/Validation/ART005_G11_RndConveyor_Whitebox_Validation_V01.unity`。

## 九组白模批次结论

- 九组均在各自技术合同和 Blender 路线通过后才开始白模。
- 每组均完成 Blender 有色结构预览、纯模型 FBX、Unity 尺度／轴向／层级检查、无悬浮自检与制作人技术验收。
- 正式候选与 R&D 对象按合同分源、分 FBX、分验证场；失败迭代保留但不冒充最终候选。
- 本批次完成结构白模门禁，不等同于材质、LOD、运行时动作、物理、玩法或最终用户视觉验收完成。
