# B45 实施前核验

环境：Windows10、Unity2022.3.62f3c1、核验主工程UnitySkills8091，运行验收仅1920×1080；新增结构停止Play Mode，普通编译，conventions与客户端/QA职责已读取。codebase-memory当前AutoEra查询为空，改用产品目录内最小范围rg。

B42/B43/B44已交付增量：真实算法/硬件、同一货物权威、实际矿石与木材生产；B44最新102/102原生测试、健康5/5。正式矿脉美术、完整b07/P4-020、农业/水泵、完整G4与B47正常新进度入口未交付，本批不冒充这些门。

运输继承DEC-111/123/202、行为容量与结算规格：正式费用规则34；现有模板因额外Delay常量节点实测35逻辑点，本批保持原有费用与结构，目的容量不足30秒延迟重试；机械臂准备0.5秒、每秒5单位、一级作用距离2米，二级速度+20%、距离2.2米。这些为既有参数，不新增平衡数值。当前生成数据表未提供作业参数列，以专用配置资产复制成不可变规则，不手改生成C#。

现场缺口：模板两次Transfer使用同一arm目标，Cargo.Field=target_item没有跟随物品参数；source_amount原读剩余天然矿量不代表可装缓存。修正为显式arm_load/arm_unload绑定与加载/卸载标记、所选物品和实际缓存；保留费用、坐标参数及重试合同。效应器代次属于硬件生命周期，支持同组件的多个显式目标绑定，不按空间猜目标。

快速执行候选检查：运输责任、安全单位边界、实际机械臂可达性与当前独占Unity现场尚含专业判断，无法安全拆交；主对话直接实施，不启动子agent。

允许文件：

- Assets/Game/Scripts/AutoEra/Resources/：新增真实转移执行器、世界持有交付责任和不可变读出，扩展ResourceWorldService；继续借用同一CargoOwnershipAuthority，禁止第二份库存。
- Assets/Game/Scripts/AutoEra/ResourcePoints/RegionProductionTools.cs：加载批准MultiJointArm实体并驱动实际MotionRig/机械臂几何；ProductionToolMounts保持既有配置。
- Assets/Game/Scripts/AutoEra/World/Region/InitialRegionScene.cs、RegionMachineRuntimeRegistry.cs、InitialRegionEntity.cs、RegionWarehouseFacility.cs：显式装卸端点创建、工具接入与GF池释放。
- Assets/Game/Scripts/AutoEra/Machines/MachineScheduling.cs：固定转移方向与责任目标参数；不重写通用调度。
- Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmMachineAdapter.cs、AlgorithmEvaluation.cs、AlgorithmCatalog.cs、InitialAlgorithmTemplates.cs：固定运输绑定、物品参数与真实缓存查询。
- Assets/Game/Scripts/AutoEra/UI/WarehouseForm.cs、FieldHudDetailForm.cs、Integration/WarehouseReadModel.cs、RegionReadModel.cs及必要真实生产读出。
- Assets/Game/Scripts/AutoEra/Editor/ProductionTransportMigration.cs、Assets/Game/Config/ResourceProduction/ResourceTransfer.asset：Unity原生配置与端点绑定。
- Assets/Game/Prefabs/Entity/InitialRegion/Forest.prefab、MineralVein.prefab、Warehouse.prefab；必要批准机械臂Prefab仅修真实动作绑定，不替换美术。
- Assets/Game/Prefabs/UI/Operations/WarehouseForm.prefab、Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab及两页契约/原始prefab-layout.md：语义anchor/stretch和列表结构。
- tools/ui_contract_to_form_script.py：WarehouseForm从未接入生成集合转手写保护，不覆盖业务逻辑。
- Assets/Game/Tests/AutoEra/Editor/：本批运输、读模型与正式宿主测试及受影响模板断言。
- 本change文档/证据及总计划增量同步。

禁止：ScriptsBuiltin、asmdef、依赖/FSR、xlsx写入、Git索引/提交、完整农业/水泵/传送带、空间猜测目标、手改Unity YAML、生成假库存或把测试输入当生产产出。存档/离线恢复由B47～B49继续。

装卸朝向使用已绑定端点的contact/dock锚点，通过既有MachineNavigationTarget.FacingYaw执行真实停稳与对齐；不从坐标选择来源或目的地。单位装卸跨多个真实批次连续预留，仍逐单位提交，准备时间每次行为仅一次。

追加必要接入范围：AlgorithmParameterText.cs、UI/AlgorithmPublicParametersForm.cs、UI/FieldHudDetailForm.Production.cs，以及批准MultiJointArm.prefab仅把VisualModel原生重挂到现有base_yaw；保持模型、关节ID、绑定姿态和角度限制。物品枚举与坐标参数沿用现有输入控件，保持类型/单位/精度。

最终必要接入：UI/Integration/MachineReadModel.cs读取同一世界运输责任并在关闭时退订；Editor/ProductionTransportUiMigration.cs原生调整两页列表，FieldHudDetailForm最小根契约只记录页面数组发现机制，实际子节点由原始规格与art/prefab-layout.md定义。
