# Segment 17 — G03 入口与滑动门白模验收

日期：2026-08-27
范围：4m入口平台、围栏、4m双扇滑动门、Unity权威门叶Pivot与安全区。

## 制作人验收结论

- G03 白模通过技术验收；允许按既定顺序进入 G04，未放行其他后续组。
- 平台为 `4×3×0.2m`，不含G05地形件；围栏端柱与平台接触；门洞净宽4m、净高3m。
- 两扇门叶为 `2×2.7×0.16m`，Unity权威 `DoorLeafPivot_L/R` 闭位为 `X=-1/+1m`、全开为 `X=-3/+3m`，只沿本地X移动2m并完整释放4m净开口。
- `DoorSafetyZone` 为无Renderer的 `BoxCollider Trigger`，尺寸 `4×3×3m`；部署根和运行时Pivot均为identity／单位缩放，`DCCImportRoot_G03` 只承担静态视觉轴补偿。
- 首轮静态门状态窗因无支撑悬浮被退回；V05又因子网格 `scale=100` 与Importer回退被废弃。最终V06将状态安装座实体搭接到左门柱外侧，未侵入净开口。
- V06代表网格 `DoorStatusMount_Export`、`DoorFrame_Post_Export`、`DoorHeader_Export` 均为单位缩放，网格级旋转仅保留Unity世界Y=180°的统一静态方向；旧V02／V04／V05实例已停用，V06为唯一活动静态门。
- V06 Importer为 `globalScale=1`、`importAnimation=false`、`materialImportMode=None`、`meshCompression=Off`；Console Error为0。
- 截图完成后门叶已恢复闭位；验证场已安全保存并只读核验 `isDirty=false`。

## 证据索引

- Blender源：`ArtSource/ART004_ModularWorkshop/G03_EntryDoor/ART004_G03_EntryDoor_Whitebox_V01.blend`
- 自检：`ArtSource/ART004_ModularWorkshop/G03_EntryDoor/Evidence/G03_EntryDoor_SelfCheck_V01.md`
- 最终静态门：`Assets/Art/Authoring/ART004_ModularWorkshop/G03_EntryDoor/Models/ART005_G03_Door_Static_V06_StatusAttachedRooted.fbx`
- 最终全开图：`Assets/Art/Authoring/ART004_ModularWorkshop/G03_EntryDoor/Evidence/G03_EntryDoor_UnityOpen4m_V04_StatusAttachedUnitSafe.png`
- 验证场：`Assets/Art/Authoring/ART004_ModularWorkshop/G03_EntryDoor/Validation/ART004_G03_EntryDoor_Whitebox_Validation_V01.unity`

## 已知边界

- V01／V02／V04／V05只作为悬浮、轴向或单位失败证据，不构成交付权威。
- V06使用自检文档记录的标准顶层导出根流程；后续重导必须复现该流程，不得恢复 `apply_unit_scale=false` 或在Unity手工修正网格。
- 本轮只验收白模结构、净空、Pivot、安全区、尺度与轴向；不包含门占用逻辑、状态驱动、材质和G05地形连接。
