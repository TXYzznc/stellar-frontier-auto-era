# UI Prefab 全量结构与优化审查

> 生成命令：`python tools/audit_ui_prefabs.py --write`。本报告只读扫描，不修改 Unity 资产。

扫描范围：`Assets/Game/Prefabs/UI`，共 **40** 个 Prefab，合计 **5253** 个 GameObject。
契约存在性、节点命名和绑定仍以门 1 检查为准；本报告额外关注运行时对象生命周期、分页耦合和脚本热点。

## 最高复杂度页面

| 页面 | GameObject | Button | ScrollRect | Item 模板 | 脚本 LOC | 主要动作 |
|---|---:|---:|---:|---:|---:|---|
| `FieldHudDetailForm` | 786 | 102 | 36 | 36 | 374 | 拆成独立 UIForm 页或子 Form；保留导航壳，避免一次实例化全部页面, 36 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `OperationDialogForm` | 539 | 54 | 23 | 23 | 784 | 拆成独立 UIForm 页或子 Form；保留导航壳，避免一次实例化全部页面, 23 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `ProgressReportForm` | 355 | 38 | 16 | 16 | 247 | 拆成独立 UIForm 页或子 Form；保留导航壳，避免一次实例化全部页面, 16 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `BaseCommandHubForm` | 344 | 49 | 16 | 16 | 725 | 拆成独立 UIForm 页或子 Form；保留导航壳，避免一次实例化全部页面, 16 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `SettingsForm` | 239 | 47 | 6 | 6 | 770 | 将独立分页/详情区拆为子 Form；长列表继续使用 UIItem 对象池, 6 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |

## 逐页审查

| 页面 | 资产规模 | 结构 | 脚本热点 | 结论 |
|---|---:|---|---|---|
| `AlertForm` | 164.1 KB | 57 GO / 8 Btn / 2 Scroll / 2 Item / 1 页 / 5 状态 | 258 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 2 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理；操作确认/提示型页面可迁移到 GF UIDialog 模板，Form 只保留业务回调 |
| `AlgorithmBindingForm` | 164.7 KB | 57 GO / 8 Btn / 2 Scroll / 2 Item / 1 页 / 5 状态 | 196 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 2 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `AlgorithmEdgeItem` | 3.1 KB | 1 GO / 0 Btn / 0 Scroll / 0 Item / 0 页 / 0 状态 | 82 LOC；Instantiate 0；Destroy 0；层级查找 1；Update 0 | 缺契约；结构规模可控；维持契约生成、对象池和统一外壳生命周期 |
| `AlgorithmEditorForm` | 332.9 KB | 106 GO / 22 Btn / 4 Scroll / 3 Item / 1 页 / 5 状态 | 1288 LOC；Instantiate 1；Destroy 1；层级查找 5；Update 0 | 3 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理；脚本仍含运行时 Instantiate/Destroy；优先改为 UIItem 或局部复用池 |
| `AlgorithmLibraryForm` | 449.6 KB | 153 GO / 22 Btn / 6 Scroll / 6 Item / 3 页 / 15 状态 | 381 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 将独立分页/详情区拆为子 Form；长列表继续使用 UIItem 对象池；6 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `AlgorithmNodeItem` | 43.3 KB | 16 GO / 3 Btn / 2 Scroll / 2 Item / 0 页 / 0 状态 | 254 LOC；Instantiate 0；Destroy 0；层级查找 6；Update 0 | 缺契约；2 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `AlgorithmPublicParametersForm` | 183.0 KB | 63 GO / 9 Btn / 2 Scroll / 2 Item / 1 页 / 5 状态 | 270 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 2 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `BaseCommandEnergyForm` | 212.1 KB | 74 GO / 10 Btn / 3 Scroll / 3 Item / 1 页 / 5 状态 | 364 LOC；Instantiate 0；Destroy 0；层级查找 1；Update 0 | 3 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `BaseCommandHubForm` | 1021.7 KB | 344 GO / 49 Btn / 16 Scroll / 16 Item / 6 页 / 30 状态 | 725 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 拆成独立 UIForm 页或子 Form；保留导航壳，避免一次实例化全部页面；16 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `BuildCatalogForm` | 156.8 KB | 55 GO / 7 Btn / 2 Scroll / 2 Item / 1 页 / 5 状态 | 108 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 2 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `ComponentLibraryForm` | 487.9 KB | 165 GO / 24 Btn / 7 Scroll / 7 Item / 3 页 / 15 状态 | 373 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 将独立分页/详情区拆为子 Form；长列表继续使用 UIItem 对象池；7 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `ComponentPickerForm` | 157.3 KB | 55 GO / 7 Btn / 2 Scroll / 2 Item / 1 页 / 5 状态 | 311 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 2 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `CropKnowledgeForm` | 149.5 KB | 53 GO / 6 Btn / 2 Scroll / 2 Item / 1 页 / 5 状态 | 108 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 2 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `ExitFlowForm` | 156.9 KB | 55 GO / 7 Btn / 2 Scroll / 2 Item / 1 页 / 5 状态 | 129 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 2 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理；操作确认/提示型页面可迁移到 GF UIDialog 模板，Form 只保留业务回调 |
| `FeatureHelpForm` | 156.8 KB | 55 GO / 7 Btn / 2 Scroll / 2 Item / 1 页 / 5 状态 | 108 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 2 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `FieldHudDetailForm` | 2277.3 KB | 786 GO / 102 Btn / 36 Scroll / 36 Item / 16 页 / 80 状态 | 374 LOC；Instantiate 0；Destroy 0；层级查找 4；Update 0 | 拆成独立 UIForm 页或子 Form；保留导航壳，避免一次实例化全部页面；36 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `FieldHudForm` | 12.6 KB | 2 GO / 0 Btn / 0 Scroll / 0 Item / 0 页 / 0 状态 | 164 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 结构规模可控；维持契约生成、对象池和统一外壳生命周期 |
| `FieldHudMachineOverviewForm` | 180.5 KB | 59 GO / 12 Btn / 2 Scroll / 2 Item / 1 页 / 5 状态 | 63 LOC；Instantiate 0；Destroy 0；层级查找 2；Update 0 | 2 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `FieldHudResidentForm` | 199.2 KB | 58 GO / 17 Btn / 0 Scroll / 0 Item / 5 页 / 0 状态 | 113 LOC；Instantiate 0；Destroy 0；层级查找 2；Update 0 | 结构规模可控；维持契约生成、对象池和统一外壳生命周期 |
| `HelpForm` | 156.5 KB | 55 GO / 7 Btn / 2 Scroll / 2 Item / 1 页 / 5 状态 | 108 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 2 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `MachineLibraryForm` | 503.7 KB | 169 GO / 26 Btn / 7 Scroll / 7 Item / 3 页 / 15 状态 | 787 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 将独立分页/详情区拆为子 Form；长列表继续使用 UIItem 对象池；7 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `MainMenuForm` | 171.8 KB | 59 GO / 9 Btn / 2 Scroll / 2 Item / 1 页 / 5 状态 | 151 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 2 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `NodeComponentPickerForm` | 157.3 KB | 55 GO / 7 Btn / 2 Scroll / 2 Item / 1 页 / 5 状态 | 210 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 2 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `OperationDialogForm` | 1527.9 KB | 539 GO / 54 Btn / 23 Scroll / 23 Item / 13 页 / 65 状态 | 784 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 拆成独立 UIForm 页或子 Form；保留导航壳，避免一次实例化全部页面；23 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `OperationFeedbackForm` | 283.3 KB | 99 GO / 12 Btn / 4 Scroll / 4 Item / 2 页 / 10 状态 | 115 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 4 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理；操作确认/提示型页面可迁移到 GF UIDialog 模板，Form 只保留业务回调 |
| `ProgressReportForm` | 1013.9 KB | 355 GO / 38 Btn / 16 Scroll / 16 Item / 8 页 / 40 状态 | 247 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 拆成独立 UIForm 页或子 Form；保留导航壳，避免一次实例化全部页面；16 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `QuestForm` | 314.9 KB | 107 GO / 16 Btn / 4 Scroll / 4 Item / 2 页 / 10 状态 | 151 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 4 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `RecordReaderForm` | 410.6 KB | 143 GO / 17 Btn / 6 Scroll / 6 Item / 3 页 / 15 状态 | 335 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 6 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `RuleHelpForm` | 111.3 KB | 41 GO / 4 Btn / 1 Scroll / 1 Item / 1 页 / 5 状态 | 102 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 1 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `SaveRecoveryForm` | 149.5 KB | 53 GO / 6 Btn / 2 Scroll / 2 Item / 1 页 / 5 状态 | 253 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 2 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理；操作确认/提示型页面可迁移到 GF UIDialog 模板，Form 只保留业务回调 |
| `SaveSlotsForm` | 410.4 KB | 143 GO / 17 Btn / 6 Scroll / 6 Item / 3 页 / 15 状态 | 428 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 6 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `SettingsForm` | 717.7 KB | 239 GO / 47 Btn / 6 Scroll / 6 Item / 3 页 / 15 状态 | 770 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 将独立分页/详情区拆为子 Form；长列表继续使用 UIItem 对象池；6 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `ShopForm` | 435.3 KB | 150 GO / 19 Btn / 6 Scroll / 6 Item / 3 页 / 15 状态 | 174 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 将独立分页/详情区拆为子 Form；长列表继续使用 UIItem 对象池；6 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `SystemMenuForm` | 172.0 KB | 59 GO / 9 Btn / 2 Scroll / 2 Item / 1 页 / 5 状态 | 137 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 2 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `TutorialForm` | 156.6 KB | 55 GO / 7 Btn / 2 Scroll / 2 Item / 1 页 / 5 状态 | 108 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 2 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `UpgradeForm` | 449.3 KB | 157 GO / 16 Btn / 8 Scroll / 8 Item / 3 页 / 15 状态 | 166 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 将独立分页/详情区拆为子 Form；长列表继续使用 UIItem 对象池；8 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `WarehouseForm` | 402.7 KB | 141 GO / 16 Btn / 6 Scroll / 6 Item / 3 页 / 15 状态 | 154 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 6 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `WorkshopForm` | 567.5 KB | 197 GO / 23 Btn / 9 Scroll / 9 Item / 4 页 / 20 状态 | 183 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 将独立分页/详情区拆为子 Form；长列表继续使用 UIItem 对象池；9 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `WorldObjectPickerForm` | 164.9 KB | 57 GO / 8 Btn / 2 Scroll / 2 Item / 1 页 / 5 状态 | 284 LOC；Instantiate 0；Destroy 0；层级查找 0；Update 0 | 2 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |
| `WorldPlacementForm` | 482.6 KB | 166 GO / 19 Btn / 9 Scroll / 9 Item / 3 页 / 15 状态 | 691 LOC；Instantiate 0；Destroy 0；层级查找 1；Update 1 | 将独立分页/详情区拆为子 Form；长列表继续使用 UIItem 对象池；9 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理 |

## 统一架构结论

- 所有页面继续保持一个 GF UIForm 入口；列表行通过 `UIItemObject` 对象池复用。
- `AlgorithmEditorForm`、`BaseCommandHubForm`、`FieldHudDetailForm` 是拆分优先级最高的三个页面；算法公开参数、基地能源详情和现场机器总览已拆为独立 Form，其余跨对象详情页继续由动态页宿主承载。
- `AlertForm`、`OperationDialogForm`、`OperationFeedbackForm`、`SaveRecoveryForm`、`ExitFlowForm` 属于提示/确认语义，建议逐步迁移为 UIDialog；迁移前保留现有 UIViews ID 与回调合同。
- 不建议机械拆出每个装饰节点；只有跨页面复用、独立交互和独立生命周期同时成立时才新增 UIItem 资产。
- 本轮代码级优化集中在 `UIItemBase` 空绑定安全、`AutoEraShellFormBase` 控件扫描缓存、动态页契约校验、子 Form 生命周期和现有对象池路径。

## 本轮已实施

- `AlgorithmEditorForm` 已移除公开参数页，新增 `AlgorithmPublicParametersForm`；父子 Form 共享算法读模型，父窗关闭和回收时会取消子窗加载。
- `BaseCommandHubForm` 已移除能源详情页，新增 `BaseCommandEnergyForm`；能源页按需加载并共享父窗读模型，虚拟页索引保持兼容。
- `FieldHudDetailForm` 已把机器总览页提取为 `FieldHudMachineOverviewForm`；其余对象详情页继续由动态页宿主承载，保持选择页索引稳定。`FieldHudResidentForm` 和详情页都缓存控件引用，动态页契约显式校验 `Grp_PageHost`。
- `OperationDialogForm` 归入 `AutoEraUIDialogFormBase`，确认类界面统一使用模态 UIDialog 生命周期语义。
- 门 1 契约校验通过；能源端到端 PlayMode 测试 1/1、现场定位 PlayMode 测试 1/1、操作界面 PlayMode 测试 1/1、算法绑定 EditMode 测试 3/3、UI 结构 EditMode 测试 11/11。
