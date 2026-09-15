# B08 正式低模恢复证据

派发：`b08-formal-lowpoly-replacement-restore`。输入为 ArtResource 只读交接
`B08_RuntimeLowPoly_20260908_v01/RuntimeCandidateHandoff_v03.json`。该交接文件的候选
状态已由用户于 2026-09-09 明确覆写为整批通过；本次未重开任何美术造型验收。

## 恢复清单

所有目标均使用同名的 BaseColor、Normal、AmbientOcclusion、MetallicSmoothness 四通道贴图；
四通道的逐文件 SHA-256 保存在输入交接 JSON 的对应 `textures` 条目。本表列出模型源哈希、
正式纹理目录、材质和最终运行入口。

| 资源 | 源版本 / FBX SHA-256 | 正式模型与贴图目录 | 材质 | Entity 入口 |
| --- | --- | --- | --- | --- |
| WheelModule | v01 / `e68826b7953593d0331358f0d7d9fa0f7d2ab174a55fe80955b40c54d0ae870a` | `Models/Machines/WheelModule.fbx`; `Textures/Machines/Modules/WheelModule/` | `Materials/Machines/Modules/WheelModule.mat` | `Prefabs/Entity/Machines/WheelModule.prefab` |
| WheeledCarrier | v02 / `1cee1d7792da99ec32a0aa8b048071548db04a05b9545495df3f5b658d2e96e4` | `Models/Machines/WheeledCarrier.fbx`; `Textures/Machines/Carriers/WheeledCarrier/` | `Materials/Machines/Carriers/WheeledCarrier.mat` | `Prefabs/Entity/Machines/WheeledCarrier.prefab` |
| CargoPod | v04 / `c42ad76c8e1bd9ec5346b9a4e4ab5d34136ccffda9d92ddb239cbbbfe058ab54` | `Models/Machines/CargoPod.fbx`; `Textures/Machines/Modules/CargoPod/` | `Materials/Machines/Modules/CargoPod.mat` | `Prefabs/Entity/Machines/CargoPod.prefab` |
| FixedRotaryCarrier | v02 / `161c0457616354a36808dbc314904dd94e5c3139aea8ad0392f2503e63fe212d` | `Models/Machines/FixedRotaryCarrier.fbx`; `Textures/Machines/Carriers/FixedRotaryCarrier/` | `Materials/Machines/Carriers/FixedRotaryCarrier.mat` | `Prefabs/Entity/Machines/FixedRotaryCarrier.prefab` |
| MultiJointArm | v02 / `af8282c04cc50beaf5e9717022ae2922872307056275982aae2665be5e7b033d` | `Models/Machines/MultiJointArm.fbx`; `Textures/Machines/Effectors/MultiJointArm/` | `Materials/Machines/Effectors/MultiJointArm.mat` | `Prefabs/Entity/Machines/MultiJointArm.prefab` |
| WaterCannon | v04 / `29957ec3e4c1d9a9b1f42ffd74a08dfc92ee3a68e813a4bf8cc6765f16cb6ab6` | `Models/Machines/WaterCannon.fbx`; `Textures/Machines/Effectors/WaterCannon/` | `Materials/Machines/Effectors/WaterCannon.mat` | `Prefabs/Entity/Machines/WaterCannon.prefab` |
| RotarySaw | v02 / `7e1bfa4e1d8129fea3c6e66f62b16feff2367b4ec1a23ccd079d03caa3a43686` | `Models/Machines/RotarySaw.fbx`; `Textures/Machines/Effectors/RotarySaw/` | `Materials/Machines/Effectors/RotarySaw.mat` | `Prefabs/Entity/Machines/RotarySaw.prefab` |
| RotaryDrill | v02 / `6411f3c563122b4bcedcca8970cda1d56d615f00f6ec65ce9dfd301edac67ce3` | `Models/Machines/RotaryDrill.fbx`; `Textures/Machines/Effectors/RotaryDrill/` | `Materials/Machines/Effectors/RotaryDrill.mat` | `Prefabs/Entity/Machines/RotaryDrill.prefab` |
| SlidingDoor_D24 | v01 / `8f507e69e7da278c6847d19fc96008cb382a0e1160eddceaf018de72e40e60f9` | `Models/Buildings/SlidingDoor_D24.fbx`; `Textures/Buildings/Mechanisms/SlidingDoors/SlidingDoor_D24/` | `Materials/Buildings/Mechanisms/SlidingDoors/SlidingDoor_D24.mat` | `Prefabs/Entity/Buildings/SlidingDoor_D24.prefab` |
| SlidingDoor_D40 | v01 / `1ce5495a4fa88783a9b29c0df4f6c8e492b1a95089bd84b61827f42c946f4032` | `Models/Buildings/SlidingDoor_D40.fbx`; `Textures/Buildings/Mechanisms/SlidingDoors/SlidingDoor_D40/` | `Materials/Buildings/Mechanisms/SlidingDoors/SlidingDoor_D40.mat` | `Prefabs/Entity/Buildings/SlidingDoor_D40.prefab` |
| Conveyor | v02 / `9818f3d6dc717ebe9d8e4e5f83acb07820ed2112747474d50be850a9bccb43fc` | `Models/Buildings/Conveyor.fbx`; `Textures/Buildings/Logistics/Conveyor/` | `Materials/Buildings/Logistics/Conveyor/Conveyor.mat` | `Prefabs/Entity/Buildings/Conveyor.prefab` |

## 绑定修复

- B08 合同、材质和 Entity 生成器从已接受项目历史 `e481db8f` 恢复；缺失正式资源才从只读交接导入，已匹配的正式 FBX 未重复覆盖。
- 11 张正式动作图已通过主工程 `8090` 的 AssetDatabase 从
  `Assets/Game/MotionGraphs/FunctionalPrototypes/` 移至
  `Assets/Game/MotionGraphs/Entity/`，保持原 `.meta` GUID；正式 Entity 测试不再指向开发目录。
- 早先的 `B08FormalEntityPrefabEditModeTests` 失败为 Entity 路径不存在导致的 12 个图兼容断言失败；修复后回归由快速执行包
  `b08-formal-lowpoly-motiongraph-binding-regression-rapid` 执行：job `8d6c1107`，23/23 通过，Console Error=0、Warning=0。
- 本机再次核对 11 个正式 FBX 与交接 JSON 的 SHA-256，全部一致；`git diff --check` 与框架纯度审计均通过。
