# P2-007 传感器服务准备核对

## 2026-09-11制作人收口：用户已确认实施（DEC-197）

本节覆盖下方原始准备稿中“未批准”、10/12等级成本及二级提速建议：采用纯C#服务＋显式只读提供者＋组件锚点适配；保留等级但本阶段各等级沿用一级参数，对象/土壤统一1秒、6/4米、各10算力。超距保留监测租约；未绑定、永久删除或无提供者时释放。其余生命周期、公开逐格数据与测试边界按原稿，基础服务与农业/林业正式提供者接入分开验收。完整权威规则已同步GameDesign《机器自动化与算法》及《统一数值模型》。历史准备稿保留供追溯，不再作为待确认阻塞。

下一实施单元及精确授权见 `../Active/b14-sensor-binding-and-public-readouts.md`；原准备任务已完成，不重新领取。

2026-09-11；任务 `client-p2007-sensor-preparation`。本文件仅为实施建议，不是已批准实现、不新建OpenSpec、不修改GameDesign。任务名称及P2-002/P1-001/P2-006依赖来自制作人本次只读任务表核对的派发单；本轮没有再次打开或写入xlsx。

## 1. 目标与边界

建议下一单元完成：已安装传感器的单目标永久引用、有效性/原因、固定周期采样、持续算力租约、公开强类型读数及生命周期。复用机器名册、区域身份和计算池；不增加算法执行器、作业专用节点、自动浇水/选树、UI、Shader、扫描探索、真实通信、资源生产或离线追赶。

依据：`Docs/GameDesign/02-系统设计/01-机器自动化与算法.md`的“第一版农业组件端点”“运行算力计量”“第一版算力与逻辑容量基准”和采样优先/合并规则；`03-资源种植与生产.md`逐格土地与公开状态；`13-统一数值模型.md`；`90-设计管理/决策记录.md` DEC-093、166、168及后续固定语言/土地单元修订。已用grill-me按继承规则查证，不为历史三轮门槛重复提问。

已确认继承：一级对象状态/土壤范围6m/4m，组件锚点到最近有效交互区域；一个传感器仅一目标通道，绑定ID不绑定名称。一级持续算力各10、二级各12；采样间隔按型号固定，玩家/算法不能调整。休眠/断电保留绑定，恢复只取当前样本，不补发停机期间历史采样。对象引用不授予远程读取能力。扫描器/通信第一版只能力自检，不能按表中未来持续成本开启真实监听。

## 2. 依赖实况（检查的是代码，不是提交标题）

| 项目 | 已有证据 | 真实缺口 |
|---|---|---|
| 槽位、启停、归属 | `Assets/Game/Scripts/AutoEra/Machines/MachineInstance.cs`：ComponentInstance.Id/OwnerId/Enabled、IsComponentWorking、SetComponentEnabled；既有MachineManagement测试 | 没有传感器绑定目标或读数服务 |
| 永久身份 | `World/Identity/PersistentObjectReference.cs`按ID+ExpectedKind解析；`World/Region/InitialRegion.cs`ObjectRemoved；B12真实机器区域映射 | 需将安装组件ID、目标引用与所在区域一起校验，不能只看全局注册表仍存在 |
| 算力 | `Machines/MachineComputePool.cs`有Sampling、SensorSample、完整租约、YieldAtBoundary和等待去重 | 服务必须持有租约、边界让出、停止释放；枚举存在不等于采样已实现 |
| 上下文 | `Machines/MachineExecutionContext.cs`已组合任务/效应器/导航、订阅机器与计算池 | 只有效应器绑定和导航owner，没有传感器所有权/释放入口；不能因空闲传感器永久占租约堵死硬件停止 |
| 区域公开数据 | `World/Region/RegionObject.cs`公开Status、可空ResourceAmount、Infinite、Changed、位置/尺寸/Yaw | Status为展示字符串，不是农田阶段枚举；ResourceAmount不等于成熟作物/矿产量；没有湿度、作物、树木格网 |
| 范围 | RegionObject有几何足迹；RegionWorkQueue有作业区域 | 足迹/施工范围/作业通道不天然等于传感器有效交互区域；`RegionFieldAccess`是玩家视野门禁，不可复用为传感器距离 |
| 配置 | `GameData/AIData/DataTables/Machines/ComponentDefinitions.json`和生成`AutoEra/DataTable/ComponentDefinitions.cs`有型号/等级/槽类型/价格/功率/资源路径 | 无SensorKind、Range、SampleInterval、持续采样成本；领域ComponentDefinition也未保留型号语义供传感映射 |
| 时序 | `World/Time/WorldEventSortKey.cs`含Energy→WorldState→Sensor→Algorithm排序 | 它是排序键，不是完整调度器；新服务注入世界时间并输出带序号事件，不宣称已有完整调度总线 |

在产品代码目录按Sensor/Soil/Crop/Tree/Farm/Moisture查找，未找到农业/林业逐格生产权威实现；现存传感器测试仅槽位/管理状态，不含实际读取。因此P2-007基础可独立实施，完整农业传感效果不能凭测试夹具宣布完成。

## 3. 三种技术路径与推荐

| 路径 | 优点 | 缺点 |
|---|---|---|
| A：纯C#采样服务＋显式只读提供者＋运行锚点适配 | 可控时钟测试、原计算池复用、资源生产独立接入、无设备/场景依赖 | 需要清楚定义提供者支持字段与版本 |
| B：每个传感器MonoBehaviour自行Update扫描 | 快速看见场景结果 | 隐式全场查找、生命周期与算力分散、难以确定性测试 |
| C：先建完整农业/林业生产再实现传感器 | 可一次看全真实输出 | 大幅越过本任务，依赖未授权资源系统 |

推荐A。不新增包或业务asmdef，仍在现有Hotfix产品程序集，测试进入既有Editor程序集。只读接口不应提供生产写操作，不使用Dictionary<string,object>任意字段或反射把世界内部数据全部暴露。

### 最小合同建议（待整体批准）

- SensorProfile：绑定ComponentDefinition.Id、种类、固定间隔、范围、持续算力。建议独立Sensors/SensorDefinitions JSON表，避免把采样字段塞进所有效应器/核心。通过既有JSON→Validate→Reverse→正式生成工具链；不手改生成C#或xlsx。本轮仅列候选路径。
- SensorBinding：组件永久ID、目标PersistentObjectReference、绑定代次；一次只能一个目标。换目标先使旧代失效、卸订阅，再采样新目标；旧代回调不能覆盖新读数。
- SensorReadState：有效性、原因码、目标引用、绑定代次、样本版本、世界采样时间。有效值与LastSample诊断分离：旧值可保留作诊断，但TryRead必须在失效时拒绝，不能用0冒充无数据或把旧值当实时值。
- ObjectStateReader只暴露目标支持的强类型端口；SoilReader仅接受有土壤提供者的农田。能力描述与读数分离，没有该能力返回UnsupportedTarget/ProviderUnavailable，不生成默认农田数据。
- 提供者负责公开有效区域最近点查询、当前不可变/版本化快照；锚点适配由调用方显式提供当前位置。禁止全场Find或用机器根代替偏置传感器锚点。没有合法锚点返回AnchorUnavailable，而非默认原点。
- 湿度格：稳定土地单元ID、世界坐标、有效面积、0–100湿度、有效性；作物格：同一土地ID、局部适宜度、当前生长速度、有效性。土壤有效不以有作物为前提。整田面积加权数值只能附加，不能替代完整格网。树木格只定义资源公开读合同，树ID/HP等由树木权威供给。
- 同一版本快照内字段原子一致，不向消费者泄露可修改数组。低频数据变化时生成只读快照，未变化时复用；不在每帧复制全格网。

## 4. 生命周期与租约建议

| 状态/事件 | 读数/绑定 | 算力与事件 |
|---|---|---|
| 未安装/休眠/整机停止或断电 | 保留配置引用，立即无效；不移除算法连线 | 取消等待并释放运行租约一次，撤销未来采样；恢复重取当前状态 |
| 有效且获完整预算 | 固定间隔读取，不变值不重复Changed | 10/12持续保留，即使数据不变；每次有效采样产生NewSample |
| 算力不足/被高优先级暂缓 | WaitingCompute，不能继续发布旧值为实时 | 至多一份等待请求，以组件ID合并；仅采样边界Yield，恢复不补历史 |
| 超距 | OutOfRange但永久引用保留；返回范围重新采样 | 建议保留已启动监测成本，避免离距免费反复探测；见待确认项 |
| 目标删除/当前区域退出 | TargetMissing/RegionUnavailable；不按名称重绑 | 建议释放租约/撤订阅，目标失效事件按状态迁移一次；不存在无限重试 |
| 换绑定/卸载/Dispose | 提升代次、禁止旧回调回写 | 旧订阅与租约准确释放；硬件删除前对称停止传感服务 |

原因应区分NotInstalled、Sleeping、PowerOff、NotRunning、NoTarget、OutOfRange、TargetMissing、RegionUnavailable、UnsupportedTarget、ProviderUnavailable、AnchorUnavailable、WaitingCompute、InvalidConfiguration。多原因同时存在时规定确定性优先顺序，以机器/组件存续与安全状态优先；不能把正常暂时失效标为算法结构错误。

目标删除是目标事件；休眠/离距等是读数有效性变化，不重复伪造“对象被删除”。恢复发布新鲜样本和必要值变化，不主动创建世界作业。只提供事件合同，不实现算法节点求值、根任务自动创建或完整离线调度。

## 5. 可独立验收的范围

1. 基础服务：安装归属、重命名保持ID、重复绑定拒绝/显式重绑、范围边界/旋转区域/锚点偏移、目标删除/区域退出、启停及旧代数据隔离。
2. 租约：不变样本仍占10/12、预算不足无部分采样、单等待去重、采样边界让出、恢复只一次新采样、停机/卸载释放及B11硬件管理兼容。禁止新服务让停止等待永久不结束。
3. 提供者：真实RegionObject仅验证它实际公开的基础状态；土壤/作物/树木用Tests内显式夹具，验证稳定ID关联、空地湿度、非等面积加权、无阈值泄露、缺失/坏数据明确失效。不写入正式场景或运行生成假格网。
4. 时间：固定时间推进、大步跨越只保留最新采样、事件序号与Sensor阶段排序；不依赖Time.time或每帧轮询整图。
5. 后续集成：普通Unity编译、既有名册/算力/硬件回归、无Missing引用/Console错误。正式农业/林业生产者接入必须另列未完成边界，不能以Fake通过代替。

## 6. 建议精确写入候选（尚未授权）

建议中型OpenSpec名称：`p2007-sensor-binding-and-public-readouts`；批次编号由制作人登记，本轮不创建目录。

产品新增：
- `Assets/Game/Scripts/AutoEra/Machines/Sensors/SensorProfile.cs`
- `Assets/Game/Scripts/AutoEra/Machines/Sensors/SensorReadContracts.cs`
- `Assets/Game/Scripts/AutoEra/Machines/Sensors/MachineSensor.cs`
- `Assets/Game/Scripts/AutoEra/Machines/Sensors/MachineSensorSet.cs`
- `Assets/Game/Scripts/AutoEra/World/Region/RegionSensorReadProvider.cs`
- `Assets/Game/Scripts/AutoEra/World/Region/SensorPublicDataContracts.cs`

必要既有接入：`Assets/Game/Scripts/AutoEra/Machines/MachineExecutionContext.cs`（传感器停止/生命周期顺序），`Assets/Game/Scripts/AutoEra/World/Region/InitialRegionScene.cs`（明确世界采样调用与释放，不自动安装传感器）。

测试新增：`Assets/Game/Tests/AutoEra/Editor/MachineSensorEditModeTests.cs`、`SensorPublicDataEditModeTests.cs`、`MachineSensorIntegrationTests.cs`，全部对应meta；临时提供者只在这些测试内定义。

配置候选：`GameData/AIData/DataTables/Sensors/SensorDefinitions.json`；派生表/代码路径由正式生成器实际输出确认，未列出的生成/登记路径在实施前核对并纳入授权，不预先手写。若批准首段仅构造注入Profile测试，应明确“正式配置接入未完成”，不得假称P2-007完整完成。禁止ScriptsBuiltin、正式Prefab/场景资源、农业生产、UI、任何直接xlsx写入及Git操作。

## 7. 制作人需组织收口的点

- 已确认规则无需再问：单目标、永久身份、6/4m范围、10/12持续成本、逐格公开、不带玩家阈值、扫描/通信范围不实施。
- 文档只规定固定采样间隔，未查到明确秒数。建议首版一级1秒、二级按已确认速度升级规律计算；这是待批准数值，不擅自落配置，也不把每秒土壤生产结算等同传感器采样频率。
- 超距/未绑定/提供者未就绪时是否继续占用“已启动监测”的持续成本，现行条款未逐态明确。建议超距维持监测租约，未绑定/永久目标删除/提供者未就绪不占并返回明确原因；需随整体方案确认，不自行给玩家增加隐藏惩罚。
- 正式资源公开提供者尚未实现：建议批准基础服务＋基础Region适配＋测试夹具，保留农业/林业完整端口的正式接入验收。不是批准新增生产系统。
- `01-机器自动化与算法.md`448行仍有“专用树木节点”旧措辞，固定语言条款与DEC-175明确受限通用格网运算；本方案遵循后者。该文件末尾仍有“一种载体”旧摘要，与已批准固定旋转载体不一致。本轮仅留痕，不在唯一写范围外改权威设计。

本轮仅此Planning文件及工具维护的队列变化；未驱动Unity、未运行新的实现测试、未新建change或改变代码/资源/数据。完成后实际回传制作人，由其组织整体实施范围确认。
