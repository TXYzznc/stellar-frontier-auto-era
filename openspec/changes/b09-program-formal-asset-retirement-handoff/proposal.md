## Why

正式低模 Entity Prefab 已验收并已具备 Motion 合同绑定，但旧 b06 开发原型、验收场与工具仍直接引用 `FunctionalPrototypes`。同时，ART-006 正式 UI 入口仍依赖 V03 候选链，旧 `Art/UI/ART006_UI` 与 ContractSample 验证资产仍留在项目中。继续保留这些过程资产会模糊正式运行边界，并使后续资源维护误把临时资产当成产品权威。

## What Changes

- 以正式 `Prefabs/Entity`、B08 MotionContracts 和已验收低模为唯一运行时动作预览对象；将旧 b06 预览、构建器、测试和演示场切换到正式路径。
- 将两枚 Operations UIForm 入口固化为不依赖 `ART006_UI` V01–V04 候选链的独立正式 Prefab，并保持既有 GF UIForm、输入和状态合同。
- 将仍有审计价值的截图归档到对应 OpenSpec evidence；通过 AssetDatabase 退役无运行引用的 ContractSample、ART-006 过程根、旧原型目录、旧演示场和其空目录。
- 建立退役门禁：任何删除前均须完成 GUID/引用扫描、正式 Entity/Operations 回归、Missing Reference=0 和 Console Error=0。

## Capabilities

### New Capabilities

- `formal-asset-retirement-handoff`: 正式 Entity 与正式 UI 入口接管过程资产、验证后原子退役的资源治理能力。

### Modified Capabilities

- 无。

## Impact

- Unity Editor 生成器、动作预览工具、b06 验收场及其 EditMode/PlayMode 测试。
- `Assets/Game/Prefabs/Entity/`、`Assets/Game/Prefabs/UI/Operations/`、正式 MotionGraph 路径与 AssetDatabase 引用。
- `Assets/Game/Prefabs/FunctionalPrototypes/`、`Assets/Game/Materials/FunctionalPrototypes/`、`Assets/Game/MotionGraphs/FunctionalPrototypes/`、`Assets/Game/Scenes/AutoEra/`、`Assets/Game/Art/UI/ART006_UI/` 和 ContractSample 历史验证资产。
- 不修改 `ScriptsBuiltin`、任何 xlsx、任务表或 Git 索引。
