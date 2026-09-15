# B15 客户端验证

## 已有证据

- S1初版图测试：job `7365788f`，3/3；当时Console有宿主FMOD Error60，不作为整包清洁门禁。
- S2初版原子批次：job `52c52ae9`，4/4；新鲜Console 3 Log/0 Warning/0 Error，8090正常释放。后续增加状态持续租约，须以新回归为准。
- 当前普通编译：AlgorithmServicesIntegrationTests经GF公开UIComponent入口修正后无编译错误；未增加程序集引用。
- 首轮联合测试实际原生XML失败于临时目标与机器占地重叠，已调整夹具位置，不修改正式场景。
- 二轮 `b15-services-regression-v2`：图 `f9101e32` 4/4；执行 `a70cc374` 6/6；实例 `3132adff` 5/5。集成 `44de1fff` 的REST句柄在Play转换后失效，新鲜XML（2026-09-11 15:05:11Z～15:05:17Z）0/1：退出Expected 0/实际10，原因夹具未将传感器集合AttachSensors到区域释放。此前导航、面板关闭不取消、结果历史断言已执行，但不能据此标联合完成。
- 三轮修正：显式AttachSensors；世界毫秒与导航实时秒分参；用户/机器/应用暂停原因独立；显式新图发布后仅重新绑定已注册且身份/代次匹配的传感器。普通编译无错误。新回归 `b15-services-regression-v3` 已派，待新证据。

## 三轮新鲜回归结果

2026-09-11，快速执行队列 `b15-services-regression-v3`：

| 测试 | job | 结果 |
|---|---|---|
| AlgorithmGraphEditModeTests | 3e5796a1 | 4/4 |
| AlgorithmExecutionEditModeTests | 246af699 | 7/7 |
| AlgorithmInstanceEditModeTests | a2d5125a | 5/5 |
| AlgorithmServicesIntegrationTests | 082b1abf（提交句柄） | 原生XML 1/1 |

联合测试经历真实Enter/ExitPlayMode，REST句柄失效；采用新鲜原生XML `Temp/b15-services-regression-v3-result-20260911.xml`，start 2026-09-11 15:13:09Z / end 15:13:16Z，而非历史job。17/17通过。Console 14 Log、0 Warning、0 Error；8090已释放，非PlayMode/非编译/非更新。

联合断言涵盖：真实NavMesh到达、传感器绑定代次拒绝、4条因果事件且同一任务ID、任务完成历史、同一机器权威身份、硬件面板关闭不取消与重开、区域退出Compute Used/Waiting均0且传感器不可读。截图仅Temp/b15联合输出，不写正式场景或Prefab。

纯执行测试另测错误零提交、算力等待后完整提交、状态持续租约、延迟溢出零提交、暂停冻结计时、独立暂停原因及恢复、影子状态顺序、32事件上限/50历史、显式连续输入合并与瞬时顺序、1000次空闲Pump当前线程分配0。

## 实际子集与边界

固定节点子集：常量、参数、输入、启动、基础算术、比较/布尔、分支、合并、变量读写、延迟、导航、诊断。已命名但未实现的集合/通信/农林结构端点明确报 `StructuredEndpointNotImplemented`，不把空结构当有效读数。复合量纲运算当前明确拒绝，不能宣称完整固定语言实现。

模板仅内存库与独立实例数据，不包含五套生产模板。恢复仅无物理指令在途的内存安全检查点；包含任务上下文的等待事件拒绝捕获，不冒充完整机器/任务磁盘存档。完整P3、G3、UI画布与农业/能源/物流均未覆盖。

运行状态以变量键及活动延迟保留持续算力；新增状态与当前批次一并取得算力，拆分运行租约后保留状态部分。暂停/销毁释放租约，恢复需重新取得。导航/传感器算力仍由B13/B14独立持有，不重复收费。

联合夹具复用正式Launch、InitialRegion、NavMesh、WheeledCarrier与FieldHud/硬件面板；公开资源数量来自明确临时RegionObject，不代表真实农林生产者。测试不改任何正式资产。

## 既有服务兼容与静态门禁

`b15-existing-machine-compatibility`：MachineSchedulingEditModeTests `d08615f1` 3/3；MachineQueueBoundaryEditModeTests `d8c000bc` 3/3；MachineExecutionContextEditModeTests `7ae86d07` 3/3。9/9，Console 3 Log/0 Warning/0 Error；8090非PlayMode/非编译/非更新并释放。

本批最终选定回归共26/26（17新服务＋9既有兼容），不把历史失败计入通过，也不把这些计数称为项目全量测试。

OpenSpec strict、`audit_framework_purity.py --product-profile tools/audit_product_profile.json`、`audit_project_boundaries.py`通过；精确范围git diff --check与新增源码/文档尾随空白扫描无报告。没有新增asmdef，未写正式资产/配置/xlsx、未操作Git索引或提交。接口已实际点对点发送2D，最终任务交付另外实际发送制作人。

## 精确候选路径

所有文件保留未提交；既有MachineComputePool/MachineScheduling在当前工作区本身尚未跟踪，B15只增加implementation-scope列明API，后续人工集成不能误将其全部历史内容归因B15。

```text
Assets/Game/Scripts/AutoEra/Algorithms.meta
Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmCatalog.cs
Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmCatalog.cs.meta
Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmDocument.cs
Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmDocument.cs.meta
Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmEvaluation.cs
Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmEvaluation.cs.meta
Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmInstanceService.cs
Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmInstanceService.cs.meta
Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmMachineAdapter.cs
Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmMachineAdapter.cs.meta
Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmMemorySnapshot.cs
Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmMemorySnapshot.cs.meta
Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmRuntime.cs
Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmRuntime.cs.meta
Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmValidator.cs
Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmValidator.cs.meta
Assets/Game/Scripts/AutoEra/Machines/MachineComputePool.cs
Assets/Game/Scripts/AutoEra/Machines/MachineScheduling.cs
Assets/Game/Tests/AutoEra/Editor/AlgorithmGraphEditModeTests.cs
Assets/Game/Tests/AutoEra/Editor/AlgorithmGraphEditModeTests.cs.meta
Assets/Game/Tests/AutoEra/Editor/AlgorithmExecutionEditModeTests.cs
Assets/Game/Tests/AutoEra/Editor/AlgorithmExecutionEditModeTests.cs.meta
Assets/Game/Tests/AutoEra/Editor/AlgorithmInstanceEditModeTests.cs
Assets/Game/Tests/AutoEra/Editor/AlgorithmInstanceEditModeTests.cs.meta
Assets/Game/Tests/AutoEra/Editor/AlgorithmServicesIntegrationTests.cs
Assets/Game/Tests/AutoEra/Editor/AlgorithmServicesIntegrationTests.cs.meta
openspec/changes/b15-algorithm-core-services/.openspec.yaml
openspec/changes/b15-algorithm-core-services/proposal.md
openspec/changes/b15-algorithm-core-services/design.md
openspec/changes/b15-algorithm-core-services/tasks.md
openspec/changes/b15-algorithm-core-services/specs/algorithm-graph-core/spec.md
openspec/changes/b15-algorithm-core-services/specs/algorithm-service-execution/spec.md
openspec/changes/b15-algorithm-core-services/specs/algorithm-instance-lifecycle/spec.md
openspec/changes/b15-algorithm-core-services/evidence/implementation-scope.md
openspec/changes/b15-algorithm-core-services/evidence/client-verification.md
openspec/changes/b15-algorithm-core-services/evidence/service-interface-handoff.md
```
