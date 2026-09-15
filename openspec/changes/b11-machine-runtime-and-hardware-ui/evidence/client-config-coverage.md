# B11 第一版配置覆盖与资源缺口

2026-09-10。以 `Docs/GameDesign/04-第一版/01-内容清单.md` 与最新决策为范围，不把清单设计勾选当实现验收。

## 已生成机器配置

入口：`GameData/AIData/DataTables/Machines/{MachineDefinitions,ComponentDefinitions}.json`、`GameData/AIData/Languages/Machines/Hardware.json`；通过产品Editor菜单 `Game Framework/AutoEra/Generate Machine Data` 调用既有Validate/AI Reverse与Language Reverse生成。GenerationProfiles只新增Machines映射；AppConfigs只登记新表/语言。

型号ID与数据行ID不同：行ID=型号ID×10+等级。轮式型号1001对应10011/10012；固定1002目前仅10021。组件2001/2101..2104/2201..2206对应一级行尾1。永久实例ID由世界分配器产生，不拿型号/行ID冒充。

生成物位于 `GameData/DataTables/Machines/*.xlsx`、`GameData/Languages/Machines/Hardware.xlsx`、`Assets/Game/DataTable/Machines/*.txt`、`Assets/Game/Language/Machines/Hardware.json`、`Assets/Game/Scripts/AutoEra/DataTable/{MachineDefinitions,ComponentDefinitions}.cs`；本窗口未手写这些生成物。用户任务表不参与。

| 类别/对象 | 数据进度 | 主工程真实资源/缺口 |
|---|---|---|
| 轮式载体1/2级 | 已有两行 | Entity/Machines/Carriers/WheeledCarrier.prefab |
| 固定旋转载体 | DEC-195补齐两级，工具生成Ready | Entity/Machines/Carriers/FixedRotaryCarrier.prefab；详见client-dec195-config.md |
| 基础计算核心 | 一级/二级PendingResource，配置可读 | 无正式Prefab；纯服务可使用定义，不虚构资源 |
| 对象状态/土壤/探索/通信接收传感器 | 一级/二级PendingResource | 无正式Prefab；B10静态美术生产依赖 |
| 机械臂/水枪/切割/钻探 | 一级/二级已有行 | Entity/Machines/Effectors/MultiJointArm、WaterCannon、RotarySaw、RotaryDrill.prefab |
| 通信发射器 | 一级/二级PendingResource | 无正式Prefab；第一版不实现真实通信行为 |
| 货舱 | DEC-195补齐两级，工具生成Ready | Entity/Machines/Modules/CargoPod.prefab；增加30/36、合法零功率，详见client-dec195-config.md |

表中相对资源均以 `Assets/Game/Prefabs/` 为根。不存在的项保持空路径且明确PendingResource，不创建虚假路径。WheelModule是轮式结构模块，不额外扩展第一版独立商店组件。

## 第一版其余目录（清单覆盖，不声明系统已实现）

- 通用资源：金币/木材/矿石/水/生物质；电能独立。两种种子/农产品（银穗麦、赤芯芒）、两种种子包、两种作业礼包、应急商品。现有Machines表不承载这些，完整Catalog数据仍待分批接入，不捏造价格或Prefab。
- 建筑：基础仓库、制造工坊、生物质发电机（两台共型号）、太阳能发电器、基础蓄电池、岸边水泵；另外玩家可建传送带沿b07独立物流范围。正式传送带已有 `Entity/Buildings/Logistics/Conveyor.prefab`；滑动门两规格属于建筑机构，不当新建筑种类。
- 初始区域资源点：农田、人工林、地表矿脉、无限水域。`Entity/InitialRegion/{Farmland,Forest,MineralVein,Water}.prefab` 是明确区域功能代理，不当最终植被/水面美术。Warehouse/Generator/SolarArray同目录也属已建场地代理；静态建筑正式生产未完成。
- 配方/图纸、五个正式算法模板、任务、成长/能源/生产/物流数据仍由对应职责与现行表类接入；本批不实现这些系统，不复制为MachineDefinition字段。
- 封装组件/载体沿通用物品分类和机器/组件型号引用，不新造一套物品资源或独立空桶经济。

## 验证进度与不得超报

已接收QA：MachineManagement 3/3 job7b1c586a；MachineCatalog 2/2 job3a34309e；MachineScheduling 3/3 job789747e0。Console Error/Warning=0。Catalog测试读取工具生成TXT并解析验证ID、语言键门禁与真实Prefab路径；这不等同已完成运行时GF加载验收。

已补组件10条二级行：核心75算力/50逻辑，稳定功率4；其余按现行数值模型1.2倍、一位小数生成独立功率字段。经济字段仍是型号基础价值，不伪装二级出售结算或升级总价。货舱二级未推算。

`GameData/AIData/DataTables/Catalog/FirstVersionObjects.json` 已建立40项范围/资源元数据，覆盖两载体、11组件、通用资源/能源、种子/作物、礼包/补给、建筑与资源点。ResourceState仅表示该资源交付状态，不等同MachineCatalog可实例化状态；特别是固定载体/货舱美术Ready不覆盖其PendingConfiguration。Name为目录注释，不是UI本地化文本。不存在的Prefab为空，不冒充合法路径。

补验：Machines与Catalog均已通过原生工具受控生成；GF启动回归698bc778的XML为1/1，实际读取机器定义、核心二级75/50以及40项目录。Catalog测试3572111c为3/3，验证重复身份/缺本地化及非空资源路径；最终6853fba8为14/14，增加负价格、非有限/负功率、等级/身份/路径/组件类别与Ready缺Prefab门禁。工具事务沿既有单表安全转换，不宣称三表加语言为一个全批次原子事务。

更新：DEC-195已解除此前固定载体和货舱缺值；生成及新增实例回归见client-dec195-config.md，历史PendingConfiguration描述仅作先前状态。待补UI端到端及尚未交付资源。P0-011不能整体勾选完成。
