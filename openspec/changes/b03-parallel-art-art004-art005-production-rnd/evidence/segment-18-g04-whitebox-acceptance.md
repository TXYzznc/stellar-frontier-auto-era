# Segment 18 — G04 服务接口白模验收

日期：2026-08-27
范围：管线视觉接口、操作面板、局部状态窗。

## 制作人验收结论

- G04 白模通过技术验收；允许按既定顺序进入 G05，未放行其他后续组。
- 三类资产均保持不超过1m级的独立模块职责：管线接口 `1×0.35×1m`、操作面板 `0.8×0.2×1m`、局部状态窗 `0.5×0.1×0.25m`。
- Blender预览中法兰、盲盖、键位、状态窗与各自背板／壳体形成明确实体挂接；未发现无支撑悬浮、游离件、长管、容器或物流玩法暗示。
- Unity中 `G04_ServiceInterfaces_ValidationRoot` 为identity／单位缩放，`DCCImportRoot_G04` 是唯一Y=180°静态视觉轴补偿。
- 抽检 `Pipe_Backplate_Export`、`Panel_Housing_Export`、`Status_Housing_Export` 均为单位缩放，无额外网格级轴补偿。
- 三份FBX Importer均为 `globalScale=1`、`importAnimation=false`、`materialImportMode=None`、`meshCompression=Off`；Console Error为0。
- 验证场已安全保存并只读核验 `isDirty=false`。

## 证据索引

- Blender源：`ArtSource/ART004_ModularWorkshop/G04_ServiceInterfaces/ART004_G04_ServiceInterfaces_Whitebox_V01.blend`
- Blender预览：`ArtSource/ART004_ModularWorkshop/G04_ServiceInterfaces/Evidence/G04_ServiceInterfaces_BlenderPreview_V01.png`
- 自检：`ArtSource/ART004_ModularWorkshop/G04_ServiceInterfaces/Evidence/G04_ServiceInterfaces_SelfCheck_V01.md`
- Unity预览：`Assets/Art/Authoring/ART004_ModularWorkshop/G04_ServiceInterfaces/Evidence/G04_ServiceInterfaces_UnityPreview_V02_Close.png`
- 验证场：`Assets/Art/Authoring/ART004_ModularWorkshop/G04_ServiceInterfaces/Validation/ART004_G04_ServiceInterfaces_Whitebox_Validation_V01.unity`

## 已知边界

- 本轮不实现管线网络、物流、资源输送、发光、按钮动画、状态脚本或材质贴图。
- G01工坊、G03入口门与G04服务接口尚未在同一正式组合场完成最终集成，因此OpenSpec任务2.2暂不标记完成。
