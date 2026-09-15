# 实施前精确范围

2026-09-11，S1结构单元。主对话执行：固定类型/验证包含专业判断，尚不适合机械执行；冻结后测试交快速执行/QA。

新增实际文件及各自.meta：
- Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmDocument.cs：专用序列化DTO、稳定ID、深克隆；不依赖Motion。
- Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmCatalog.cs：固定节点/端口/类型兼容。
- Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmValidator.cs：结构和绑定验证、计划私有快照。
- Assets/Game/Tests/AutoEra/Editor/AlgorithmGraphEditModeTests.cs：模型/验证回归。

目录.meta仅上述实际文件所在Algorithms。沿用现有Hotfix和Editor测试程序集，不新增asmdef/包/框架修改。新增结构必须非PlayMode普通编译；正式场景/Prefab/配置不改。后续S2/S3在动手前增量列清单。

S2下一单元（S1测试交接后写代码）：
- Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmEvaluation.cs及.meta：纯批次计算、写集、行为意图、输入快照，不调用Unity。
- Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmRuntime.cs及.meta：算力等待、事件32、诊断50、泵与代次/修订生命周期。
- Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmMachineAdapter.cs及.meta：消费既有Sensor/Tasks/Navigation，输出不可变结果，不改原服务权威。
- Assets/Game/Tests/AutoEra/Editor/AlgorithmExecutionEditModeTests.cs及.meta：原子错误、排队、代次与诊断。

兼容影响：仅新消费层；第一批固定子集不包含五生产模板、TreeGrid生产实现和UI，不从Motion求世界结果。确认DEC-097整机重启暂停其它实例但保留状态，S3测试单列。

S2精确兼容扩展：`Assets/Game/Scripts/AutoEra/Machines/MachineScheduling.cs`增加TryStart(id)，仅当id是当前优先级/FIFO应启动项才启动，避免算法适配器调用StartNext偷取其它来源任务；不改变原StartNext行为或队列容量。

S3结构单元（服务实现包含专业判断，由客户端执行；冻结测试另派）：
- Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmInstanceService.cs及.meta：草稿、提交请求、整机安全点、结构暂停/参数保持、独立模板与版本门。
- Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmMemorySnapshot.cs及.meta：只读内存状态副本；不存磁盘、不重新下发正在执行的命令。
- Assets/Game/Tests/AutoEra/Editor/AlgorithmInstanceEditModeTests.cs及.meta：隔离/过期/模板/恢复/暂停回归。
- Assets/Game/Tests/AutoEra/Editor/AlgorithmServicesIntegrationTests.cs及.meta：真实区域、NavMesh、公开传感与已验收硬件面板联合验证；仅临时实例/测试报告，不改场景Prefab。
兼容：Runtime新增明确安全点与快照接口；同机实例由服务统一调度，无自动启动生产算法。正式界面本批不改。

S2成本复核补充精确文件：`Assets/Game/Scripts/AutoEra/Machines/MachineComputePool.cs`增加已运行租约的原子拆分（总Used不变，无新调度），使批次预先取得“瞬时＋新增状态”算力后将状态部分持续保留，避免先写变量再争抢状态算力。仅增加API，不改变旧Submit/Release策略；AlgorithmExecutionEditModeTests补充持续占用/暂停释放断言。
