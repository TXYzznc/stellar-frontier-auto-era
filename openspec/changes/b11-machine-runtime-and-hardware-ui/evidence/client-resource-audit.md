# B11 Catalog Resource Audit

日期：2026-09-10
角色：rapid-executor
输入：`GameData/AIData/DataTables/Catalog/FirstVersionObjects.json`

## 结果

- Catalog JSON 可解析，`schemaVersion=1`。
- 总行数：40；唯一 `Id`：40；空 `Id`：0。
- 非空 `Prefab`：15；正式 Entity Prefab 缺失：0；`.meta` 缺失：0；GUID 缺失或无效：0。
- 空 `Prefab` 资源：25，其中 `DataOnly` 8 项、`PendingResource` 17 项。
- 本次仅做文件系统静态检查，未执行 Unity `MissingReferences` 验证。

## 非空 Prefab 映射

| Id | Prefab | GUID |
|---|---|---|
| 30016 | `InitialRegion/Warehouse` | `5d7277dffaa8cd74496a47650641a79c` |
| 30018 | `InitialRegion/Generator` | `9672f85ef12e75c4bab5bea22b3092ed` |
| 30019 | `InitialRegion/SolarArray` | `5529c1652b87481428621b39cabb79b3` |
| 30022 | `Buildings/Logistics/Conveyor` | `549bbab4f9d2a514994fcf9aa3edd147` |
| 30023 | `InitialRegion/Farmland` | `2e77248f8e274d04ab6ea5ef801afbe0` |
| 30024 | `InitialRegion/Forest` | `7fa30fe2fa7d4114590b554c291a3bc8` |
| 30025 | `InitialRegion/MineralVein` | `1719eb59dbd911c459d98db125102f54` |
| 30026 | `InitialRegion/Water` | `ceb784c0a2cad6c4d889ed628c97b13f` |
| 30027 | `Machines/Carriers/WheeledCarrier` | `a6a7d5e2324154a499c0f25aa2611b9f` |
| 30028 | `Machines/Carriers/FixedRotaryCarrier` | `e798bc0cf96632647958f0868ee0ef93` |
| 30034 | `Machines/Effectors/MultiJointArm` | `87971bafd04be804eb5fdcbc028c9bd9` |
| 30035 | `Machines/Effectors/WaterCannon` | `6e5966c83c82c484f818bb89dcffa7b5` |
| 30036 | `Machines/Effectors/RotarySaw` | `6f8604b6bfe68b64ea413c131f8be007` |
| 30037 | `Machines/Effectors/RotaryDrill` | `0605b80f7b87172489d055bfc0e436a5` |
| 30039 | `Machines/Modules/CargoPod` | `085700f3ace54dd4fb2090c13b716e7f` |

每条非空映射均检查了 `Assets/Game/Prefabs/Entity/<Prefab>.prefab`、同名 `.meta` 及其中的 `guid`。

## 空资源分组

- `DataOnly`：`30001`, `30006`, `30011`, `30012`, `30013`, `30014`, `30015`, `30040`
- `PendingResource`：`30002`, `30003`, `30004`, `30005`, `30007`, `30008`, `30009`, `30010`, `30017`, `30020`, `30021`, `30029`, `30030`, `30031`, `30032`, `30033`, `30038`

## 边界

- 未修改 Catalog JSON、Assets、代码、任务表、xlsx 或 Git。
- 空资源仅按 Catalog 的 `ResourceState` 记录，不代表运行时或配置完成状态。

