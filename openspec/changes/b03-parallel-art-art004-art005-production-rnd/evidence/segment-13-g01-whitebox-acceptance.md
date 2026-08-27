# Segment 13：G01 基础制造工坊白模验收

日期：2026-08-27

## 结论

G01 V06 白模通过制作人技术复核，可结束白模阶段；本结论不等同于中模、材质或完整模块库任务完成。

## 已核验

- 主体壳体占地为 8m × 6m，并由 G02 网格模块组合，不是单一整体可渲染大网格。
- 正面顺序为入口／门洞 → 制造状态区 → 输出缓存 01 → 输出缓存 02。
- 左侧物流口与等待平台具有连续、可信的接地和连接关系。
- V06 包含 57 个可见白模网格、0 个材质槽；未发现无支撑悬浮或游离可见件。
- `WorkshopRoot` 为 identity 部署权威；`DCCImportRoot_G01` 只承担静态 FBX 轴转换，不承担运行时交互权威。
- 六个非渲染锚点为 `WorkshopRoot` 直接子级且位置符合技术合同；实际交互方向中，`LogisticsPoint` 为 `-X`，`WaitPoint` 为 `+X`。
- `G01-Workshop-ProductionNotes-v1.1.md` 已修正锚点坐标并完成技术合同一致性核对。

## 用户裁决

预览相机和仅用于效果预览的 `CameraAnchor` 不属于运行时相机或程序接口，后续不以其精确位置、旋转或朝向作为资产验收门。只有任务明确声明运行时或确定性取证依赖时才检查该合同。

## 证据来源

- `D:/unity/UnityProject/ArtResource/ArtSource/ART004_ModularWorkshop/G01_Workshop_Whitebox/ART004_G01_Workshop_Whitebox_V01.blend`
- `D:/unity/UnityProject/ArtResource/Assets/Art/Authoring/ART004_ModularWorkshop/G01_Workshop_Whitebox/Models/ART004_G01_Workshop_Whitebox_V06.fbx`
- `D:/unity/UnityProject/ArtResource/Assets/Art/Authoring/ART004_ModularWorkshop/G01_Workshop_Whitebox/Validation/ART004_G01_Workshop_Whitebox_Validation_V07.unity`
- `D:/unity/UnityProject/ArtResource/ArtSource/ART004_ModularWorkshop/G01_Workshop_Whitebox/Evidence/G01_Workshop_Whitebox_SelfCheck_V01.md`
- `D:/unity/UnityProject/ArtResource/ArtSource/ART004_ModularWorkshop/G01_Workshop_Whitebox/Evidence/G01_Workshop_AnchorOrientation_Verification_V01.md`

## 场景恢复记录

用户手动切换场景后，V06路径被当前`ART001_ContractSample`现场误保存覆盖。V06不再作为正式验收场，
仅保留故障证据；主美从已通过的V06 FBX重新建立独立V07验证场，恢复identity `WorkshopRoot`、
静态轴转换视觉子根和六个非渲染锚点。V07磁盘文件包含完整`WorkshopRoot`并已保存，成为正式白模验证场。

G06 V19材质／UV返修随后依据用户此前“停止材质／UV处理、只保留已通过白模”的决定延期关闭，
未重新启动材质生产；该延期不代表OpenSpec任务3.8完成。
