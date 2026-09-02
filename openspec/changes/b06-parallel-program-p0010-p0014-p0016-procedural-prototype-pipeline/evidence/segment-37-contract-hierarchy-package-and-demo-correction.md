# 第37段：合同层级、独立包与验收场纠正

日期：2026-09-02

## 合同级结构

- `FunctionalRigPrototypeCatalog` 不再为六个 family 返回同一长方体模板：载体含 chassis 与四条独立 Steer/Suspension/Roll/Contact 链；轮组含独立 mount/steer/suspension/roll/contact；机械臂含 Yaw/Shoulder/Extend/Wrist、WorkPoint、Socket 与 KeepOut；效应器含 Socket/Lock/Safety Hold；门含 Frame、双门叶与 SafetyZone；传送带含 Frame、双 Rollers、Belt、Load/Unload/Block。
- 六个 Catalog Prefab 已通过 Builder 从上述确定性合同重建，结构验证保持视觉槽与权威碰撞根分离。
- QA：`FunctionalRigPrototypeCatalogContractEditModeTests` 2/2（job `2c3044b2`）、`FunctionalRigPrototypeCatalogEditModeTests` 1/1（`16a41d21`）、`FunctionalRigPrototypeStructureValidatorEditModeTests` 2/2（`5e8ef46a`），合计 5/5。

## 可重复工具包

- 新增 `MotionCorePackagePublisher`，将受控 Runtime/Contracts/Editor 文件与其原 `.meta` 输出为 `Tools/Exports/AutoEra.MotionCore-1.1.0`；输出含 `package.json`、Runtime/Editor asmdef、README 与固定 GUID 的源码副本。
- 发布器拒绝 Adapter、GF_X、场景、xlsx 和 `ScriptsBuiltin` 内容；同版本重复发布会重建相同清单并校验每个源码 `.meta` 文本保持一致。
- QA：`MotionCorePackagePublisherEditModeTests` 1/1（job `76f51b84`）。

## 固定验收场

- `FunctionalRigAcceptanceDemo` 已按六类实际 Prefab 重新布局，固定镜头以基础色与名称标签区分 family；`FunctionalRigAcceptanceDemoDirector` 驱动实际 chassis、wheel steer/roll、arm joints、effector Socket/Lock/Hold、door leaves 与 conveyor rollers/belt，不再以整体 RigRoot 位移冒充动作。
- 运行前已保存场景；运行态截图为 `FunctionalRigAcceptanceDemo-corrected-playing.png`，绑定态截图为 `FunctionalRigAcceptanceDemo-corrected-preview.png`；停止 Play Mode 后 Unity 非 PlayMode、非编译、无域重载待处理，Console Error=0。
- 用户最终可视验收 `5.4` 仍未勾选；外部 ArtResource 首次／重复导入 `3.5` 将基于本独立包执行。
