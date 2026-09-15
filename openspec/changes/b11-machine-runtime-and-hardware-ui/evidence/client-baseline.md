# B11 客户端实施基线

2026-09-10，Active b11-client-machine-runtime。OpenSpec spec-driven，开始0/19；不是只读盘点终点。

## 复用与边界

- ID：Assets/Game/Scripts/AutoEra/World/Identity/PersistentId.cs、PersistentIdAllocator.cs、PersistentObjectRegistry.cs；世界会话拥有服务，不新造全局ID。
- 已有RegionWorkQueue是作业点排队，不作为机器任务队列。Motion执行器只消费动作，不让它理解算法或拥有机器管理状态。
- 数据现有类别仅Core/Foundation；新设Machines配置，不覆盖旧类别。GenerationProfiles增加Machines映射到现有AutoEra.DataTable，禁止新增程序集。
- 业务代码计划：Assets/Game/Scripts/AutoEra/Machines/MachineDefinition.cs、MachineRoster.cs、MachineInstance.cs、MachineScheduling.cs；测试位于既有Assets/Game/Tests/AutoEra/Editor/。
- 数据计划：GameData/AIData/DataTables/Machines/MachineDefinitions.json、ComponentDefinitions.json；完整第一版覆盖登记放Catalog，只有明确数值及真实资源才可实例化。所有xlsx/C#必须安全工具生成。
- 型号稳定ID预留：1001轮式、1002固定旋转；组件2001基础核心、2101对象状态/2102土壤/2103探索/2104通信接收器、2201机械臂/2202水枪/2203切割/2204钻探/2205通信发射器/2206货舱。ID为新表独立命名域，未复用Core/实体实例ID。
- 真实资源：Assets/Game/Prefabs/Entity/Machines/{WheeledCarrier,FixedRotaryCarrier,MultiJointArm,WaterCannon,RotarySaw,RotaryDrill,CargoPod}.prefab。传感器/核心不得虚构Prefab路径。

## 当前规则来源

01机器系统状态/队列/算力、13数值、14容量、11通信第一版边界、DEC162/185～189及玩家体验现场界面。
- 两载体Sensor/Core/Effector为2/1/2与2/1/1，基础容量30/10，货舱仅加总到唯一容器。
- 固定售价350/回收280覆盖内容清单残留750；两载体覆盖01末尾旧单轮式摘要。
- 机器任务32、效应器等待16、算力等待64；五级优先级/FIFO，满拒绝，不用高优先驱逐未开始任务。任务/效应器/算力互相独立。
- 效应器不读取算法图，货舱无行为队列；不提供玩家手动作业入口。
- 通信接收器第一版无持续监听算力，只有自检测试输入；不实现真实通信/电网/导航。

## 需来源补齐，但不阻止纯服务

货舱容量/经济/功率及固定载体完整度/二级值未找到确定依据，已请求制作人核对；配置明确PendingConfiguration，不用测试数值冒充正式值。测试可注入独立数值验证容量算法。

快速执行候选：初次业务状态、身份恢复和数据结构包含专业判断，本窗口完成；冻结后测试/资源扫描按QA/快速执行队列交接。无自动Git、无任务表写入。
