# Segment 01：退役前引用与替代路径清单

日期：2026-09-09。初始清单先只读审计；下方“实施记录”追加每个实际退役批次的门禁与结果。

## 候选组规模

| 候选组 | 文件数（含 `.meta`） | 大小（bytes） | 当前结论 |
| --- | ---: | ---: | --- |
| `Prefabs/ContractSample` | 4 | 67,543 | 可在整组验证后退役 |
| `Materials/ContractSample` | 12 | 23,060 | 与 ContractSample 同组退役 |
| `Config/ContractSample` | 2 | 1,285 | 仅旧 JSON 形态研究，同组退役 |
| `Scene/ArtValidation` | 2 | 8,760 | 仅旧 ART001 验证场，同组退役 |
| `Art/UI/ART006_UI` | 36 | 2,229,435 | 仅过程证据/空目录；先归档截图 |
| `Prefabs/UI/ART006_UI` | 29 | 1,033,643 | 被正式 Operations 入口间接依赖；先独立化 |
| `Prefabs/FunctionalPrototypes` | 27 | 758,165 | 旧 b06 Builder/测试/验收场仍引用；先正式 Entity 接管 |
| `Materials/FunctionalPrototypes` | 28 | 53,865 | 被旧原型 Prefab 引用；同组退役 |
| `MotionGraphs/FunctionalPrototypes` | 2 | 848 | 旧 b06 MotionGraph Builder/测试仍引用；先迁移 |
| `Scenes/AutoEra` | 10 | 226,013 | 旧验收场及截图；先正式预览场接管 |

## 关键基线哈希

| 路径 | SHA-256 |
| --- | --- |
| `Scene/ArtValidation/ART001_ImportValidation.unity` | `89BEEF4FECD56F64F9E0331904EF1CA0A03E137F45434B633D745F72E18C2105` |
| `Prefabs/ContractSample/ENV_ContractSample.prefab` | `F272A632CD0BA9DCE633510449A1300B0909777D76E8ADC3DDA81E00FCF19C47` |
| `Prefabs/ContractSample/MCH_ContractSample.prefab` | `A923A4046E1B86DDBEE587971F4DF5EDC5391624FFC94D9A5B878502C0F18748` |
| `Prefabs/UI/Operations/BaseCommandHubForm.prefab` | `BD7EFABDF4177D0F22448D5D7AEED26C206AB165C23BD2881BEDDF65E5C51286` |
| `Prefabs/UI/Operations/FieldHudForm.prefab` | `7243A97B075F64CFEE2BD887FCC8808C9434DE2460E4C8E3ADE2686EAF9CD5E0` |
| `Scenes/AutoEra/FunctionalRigAcceptanceDemo.unity` | `483142BF337B754FE6B5A634A017D067830F3C0C952DFFD2E67FF7BB5A2087E1` |

## 引用门禁结果

- `ContractSample`：`ENV_ContractSample` 与 `MCH_ContractSample` 仅被 `Scene/ArtValidation/ART001_ImportValidation.unity` 引用；相应材质仅被这两枚 Prefab 引用；`MachineJointContract.json` 在 b06 证据中明确为非权威研究输入。该四路径组可作为一个原子退役组。
- `ART006_UI` 过程根：正式 `Operations` 入口仍经视觉候选链取得内容，故 `Prefabs/UI/ART006_UI` 不可删除；`Art/UI/ART006_UI` 仅保留过程截图、空目录和 `.meta`，须先归档其截图。
- `FunctionalPrototypes`：以下旧 Builder/测试仍直接硬编码旧路径，故不可删除：`FunctionalRigPrototypeCatalogBuilder`、`FunctionalRigPrototypeHierarchyTemplateBuilder`、`FunctionalRigMotionGraphCatalogBuilder`、`FunctionalRigAcceptanceDemoSceneBuilder`、`FunctionalRigPrototypeCatalogEditModeTests`、`FunctionalRigMotionGraphCatalogEditModeTests`、`FunctionalRigVisualCandidateEditModeTests`。
- 正式替代已存在：`Prefabs/Entity` 已含 11 类正式对象；`B08FormalEntityPrefabBuilder` 将其映射到对应 B08 MotionContracts 和既有合同 ID。下一批工作须把旧 b06 消费者切换到这套正式路径。

## 不纳入退役

- `Config/FunctionalRigContracts/Exports`：Motion Core 跨项目确定性合同导出，保留。
- `Tools/Exports/AutoEra.MotionCore-1.1.0`：ArtResource 本地 UPM 消费包，保留。
- `Config/MotionContracts/B08`、`Prefabs/Entity`、正式模型/材质/贴图：正式权威，保留。
- `Font` / `Fonts`：存在非完全重复的字体集合，需独立 GUID/引用审计后另行处理。

## 实施记录

### ART001 ContractSample 原子组

- 执行者：`rapid-executor` 执行包 `b09-contractsample-retirement-rapid`；接收与复核：客户端。
- 删除前门禁：四个目标路径均存在；外部文本引用为 0（仅目标组内互相引用）。
- 通过 8090 AssetDatabase 实际删除：
  - `Assets/Game/Prefabs/ContractSample/`
  - `Assets/Game/Materials/ContractSample/`
  - `Assets/Game/Config/ContractSample/`
  - `Assets/Game/Scene/ArtValidation/ART001_ImportValidation.unity`
- 删除后：AssetDatabase Refresh 后四路径均不存在；`Assets/Game` 内 `ContractSample` / `ART001_ImportValidation` 残留扫描为 0；Missing Reference 扫描无残留；Console Error=0、Warning=0、Editor 非 PlayMode/非编译。
- 编辑器残留处理：AssetDatabase 删除时 Editor 正加载已退役的 `ART001_ImportValidation`，因而产生仅内存的 Dirty 场景。已明确不保存该已退役场景，随后通过 Unity 场景加载切回已保存的 `Assets/Game/Scene/Launch.unity`，`isDirty=false`。这不是丢弃用户或其他窗口的创作改动，也没有重建已退役场景。

### ART006 视觉证据归档

- 已将过程资源根 `Assets/Game/Art/UI/ART006_UI/Evidence/` 的七张 PNG 截图归档至 `evidence/archived-art006-ui/`，不复制 Unity `.meta`。
- 源和归档副本 SHA-256 逐项一致：`1920x1080` `D02B47B7…2AC2`、`FieldHud` `E89D88EF…D99E`、`Objects` / `Tasks` `945345CF…B1BF`、`Rules` `62DB15D7…B181`、`RulesImpact` `ADC9D96F…DD24`、`Statistics` `2E7596D6…33C`。
- 因而后续删除 `Assets/Game/Art/UI/ART006_UI/` 时，审计截图仍保留在 b09 变更证据中。

### Operations 独立入口

- 执行 `Game Framework/AutoEra/Build Independent Operations UI Prefabs`：先按既有绑定器同步 GF/TMP/Sprite/输入/状态字段，再将 `BaseCommandHubForm.prefab` 与 `FieldHudForm.prefab` 完全解包并原路径保存，稳定 GUID/UITable 入口不变。
- `AutoEraUiPrefabBindingEditModeTests` job `83460b2c`：6/6 通过；验证两入口均不再是 Prefab instance，子层级不残留 Prefab instance，且 AssetDatabase 依赖不包含 `Prefabs/UI/ART006_UI/` 或 `Art/UI/ART006_UI/`。Console Error=0、Warning=0，Editor 非 PlayMode/非编译。
