# Segment 20：G07 轮组与 R&D 稳定支撑白模验收

日期：2026-08-27
结论：通过制作人技术验收，允许按冻结顺序进入 G08；本段不代表 ART-005 任务 3.1 整体完成。

## 正式轮组

- 正式源、FBX 与 Unity 验证场均位于 `ART005_FormalCarrier/G07_WheelSupport` 范围，未混入 R&D 稳定支撑。
- 四轮中心为 `(±0.90, 0.32, ±0.78)m`，轮胎半径 `0.32m`、宽 `0.18m`。
- 四条活动权威链均为 `WheelModuleRuntime → SuspensionPivot → WheelSteerPivot → WheelRollPivot`；四个 `WheelContactPoint` 位于 Roll 节点下且不含 Renderer。
- 固定轮拱为独立静态结构，不进入转向、滚动或悬挂运动链；±35°扫掠证据通过，验证场已恢复绑定态。
- 最终 V06 单件 FBX 使用 `globalScale=1`、无动画、无材质导入、`meshCompression=Off`；旧完整层级候选均停用，仅保留为失败迭代证据。
- 绑定态与扫掠截图显示轮胎、轮毂、悬挂壳和轮拱内部连接可信；四个轮组作为独立验证实例分开展示，不构成资产内部悬浮。

## R&D 稳定支撑

- R&D 源、FBX 与 Unity 验证场均位于 `ART005_ProceduralMachine/G07_WheelSupport` 范围，未混入正式轮组。
- 仅验证一个支撑原型；数量、部署位置、行程和受力规则均保持未冻结，不继承旧四支撑或旧行程合同。
- 活动权威链为 `SupportRoot_Unity → DeployPivot_Unity → SlidePivot_Unity → SupportContactPoint_Unity`；接地点 local `(0,-0.89,0)` 且不含 Renderer。
- 根级失败实例和 V01～V03 候选均已只读确认 `active=false`；V04 Clean 是唯一活动候选，场景仅一个活动 MeshRenderer。
- 最终可见结构由顶座、外套筒、内撑杆与脚垫连续组成；截图未发现无支撑悬浮或游离散件。
- Importer 为 `globalScale=1`、无动画、无材质导入、`meshCompression=Off`；Console Error 为 0。

## 场景与证据

- 正式验证场：`Assets/Art/Authoring/ART005_FormalCarrier/G07_WheelSupport/Validation/ART005_G07_FormalWheelModule_Validation_V01.unity`，保存后 `isDirty=false`。
- R&D 验证场：`Assets/Art/Authoring/ART005_ProceduralMachine/G07_WheelSupport/Validation/ART005_G07_Stabilizer_RND_Validation_V01.unity`，保存后 `isDirty=false`。
- 正式自检：`ArtSource/ART005_FormalCarrier/G07_WheelSupport/Evidence/G07_FormalWheelModule_SelfCheck_V01.md`。
- R&D 自检：`ArtSource/ART005_ProceduralMachine/G07_WheelSupport/Evidence/G07_Stabilizer_RND_SelfCheck_V01.md`。

## 未完成边界

- G07 只完成轮组、轮拱、悬挂外观与稳定支撑原型的白模合同，不完成 G08 效应器、G10反馈部件或 ART-005 任务 3.1 整体。
- 不把静态白模扫掠检查计作运行时转向、贴地、重定位或物理验证完成。
