# B14 / P2-007 传感器基础读取服务

2026-09-11，用户确认完整方案并要求同步设计。任务ID `b14-client-sensor-readouts`，priority10，负责人客户端。独立于挂起B11，不重开B12/B13。DEC-197为最新权威，Planning原准备稿中的等级差异和待确认措辞已失效。

## 目标、顺序与授权

按已确认纯C#服务＋显式只读提供者＋组件锚点适配方案，先使用OpenSpec propose创建 `b14-sensor-binding-and-public-readouts` 的完整artifacts并strict校验，再连续apply和测试，不把提案完成作为新的等待门槛。先读最新版GameDesign与准备稿收口，再落实现设计。每个冻结工作单元先检查快速执行候选，不整体外包专业判断。

对象状态/土壤统一各等级1秒采样、6/4米、10算力。等级字段保留但运行效果一致，不删等级概念。超距持续监测租约，未绑定/永久目标删除/无提供者释放；停止/休眠/断电/卸载/区域退出对称释放。有效样本与旧样本诊断分离，单目标永久ID、绑定代次、防旧回调、明确失败原因、等待去重和边界让出。范围使用合法组件锚点到公开区域最近点，禁止隐式全场查找及假锚点。

## 写范围

- `openspec/changes/b14-sensor-binding-and-public-readouts/` 全部artifacts、实施证据。
- `Assets/Game/Scripts/AutoEra/Machines/Sensors/`：Profile、读取合同、MachineSensor、MachineSensorSet及本单元必要内部类型；`World/Region/RegionSensorReadProvider.cs`、`SensorPublicDataContracts.cs`。
- 必要接入：`Assets/Game/Scripts/AutoEra/Machines/MachineExecutionContext.cs`、`World/Region/InitialRegionScene.cs`，仅传感器生命周期/显式依赖/世界时钟采样调用，不自动安装传感器、不改场景和导航合同。
- `Assets/Game/Tests/AutoEra/Editor/MachineSensorEditModeTests.cs`、`SensorPublicDataEditModeTests.cs`、`MachineSensorIntegrationTests.cs`及对应meta。测试夹具只放Tests。
- 正式传感器配置先使用注入Profile完成可测服务；配置接入候选 `GameData/AIData/DataTables/Sensors/SensorDefinitions.json`。必须先只读核验实际生成/登记路径并回传精确增量范围，未获得范围确认不写候选表、生成物或工作簿。该依赖不阻塞已授权基础服务；不得把正式配置未接入标为完成。

禁止ScriptsBuiltin、直接修改任何xlsx/任务表、正式Prefab/场景、美术/字体、农业生产、算法执行器、自动作业、UI、实际扫描/通信、Git索引/提交。缺少真实公开数据返回明确不可用，不生成假格网。已有等级参数的配置差异先列出真实影响，精确授权后统一处理，不顺带改其他组件。

## 验收与交接

### 2026-09-11 DEC-197既有四类传感器功率精确修正

制作人已只读核验ComponentDefinitions当前数据。本项执行已确认“各等级沿用一级运行参数”，不是新增产品决策：允许精确修改`GameData/AIData/DataTables/Machines/ComponentDefinitions.json`中四个L2行的功率及其过时等级功率注释：21012 Idle/Working=0.2/2、21022=0.2/2、21032=0.5/5、21042=0.3/3。已有相同字段不制造无意义差异。只保留Level=2与原ID/ModelId、价格、槽位、能力、资源路径等其他值；不改L1、核心、效应器或载体。

允许既有工具仅Validate→Reverse该单表到`GameData/DataTables/Machines/ComponentDefinitions.xlsx`，并正式生成对应`Assets/Game/DataTable/Machines/ComponentDefinitions.txt`、`Assets/Game/Scripts/AutoEra/DataTable/ComponentDefinitions.cs`及必要meta；禁止直接写xlsx或手改生成代码。生成类无schema变化应保持内容不变。记录四行差异和其他行/字段逻辑指纹不变，回归各等级实际功率一致及既有机器硬件行为。该表已有登记，不新增重复AppConfigs项。实际探索与通信仍不开启，不因数据统一新增业务实现。主任务继续，B11仅等待已获视觉批准的完整UI交付，不再称等待效果图批准。

### 2026-09-11正式配置精确增量放行

客户端已核对既有生成器、GenerationProfiles与MachineDataSetup；本增量属于用户已确认的B14正式配置接入，不另起任务。允许以下精确范围：

- `GameData/AIData/DataTables/Sensors/SensorDefinitions.json`。
- `GameData/AIData/GenerationProfiles.json`：仅新增Sensors映射，代码输出根沿用`Assets/Game/Scripts/AutoEra/DataTable`，命名空间`AutoEra.DataTable`，保留全部旧映射。
- `GameData/DataTables/Sensors/SensorDefinitions.xlsx`：仅由既有受控Validate→Reverse工具生成，禁止AI/脚本直接编辑工作簿；开发任务表绝不在范围内。
- `Assets/Game/DataTable/Sensors/SensorDefinitions.txt`与meta、`Assets/Game/Scripts/AutoEra/DataTable/SensorDefinitions.cs`与meta：仅由正式工具生成。
- `Assets/Game/ScriptableAssets/Core/AppConfigs.asset`：仅向mDataTables幂等新增`Sensors/SensorDefinitions`，不得替换列表、改其他配置或保留异常现场的临时删项。
- `Assets/Game/Scripts/AutoEra/Editor/SensorDataSetup.cs`与meta：精确校验/Reverse/生成/登记入口；上述新增Unity目录meta。不得修改框架生成器或MachineDataSetup。

实际加载适配仍在已授权Sensors目录。不改MachineCatalog和其他组件等级参数；若检测到现有功率等与DEC-197不一致，列精确差异请求受控修正，不隐藏问题。保留生成报告、逻辑指纹/范围核验、重复生成幂等及实际加载证据；可自行生成的任务证据写B14 evidence。工具另有未知副作用路径先只读核实，不用全量Reverse碰其他表。基础服务继续，不因普通回传停止。

验证永久身份/改名、范围边界和偏置锚点、启停/换绑/旧回调、目标删除/区域退出、无提供者、不变数据持续10算力、单等待合并/边界让出/恢复不追赶、等级一致。逐土地ID/无作物湿度/只读快照/缺字段明确无效使用受控测试；真实Region基础适配另验，不宣称农林生产完成。

普通Unity编译、既有名册/算力/硬件生命周期回归、Console/Missing引用、strict与项目纯度/边界审计；若需8090则按实际占用协调。保留B11挂起，勿被其UI依赖堵塞。确需新范围时block --claim-next，但先做其它安全工作。完成必须实际通知制作人，明确基础服务、正式配置、资源提供者各自状态，再complete --claim-next；不自动Git。
