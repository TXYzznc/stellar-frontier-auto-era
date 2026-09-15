# B11 客户端机器服务进度与回归

2026-09-10；仅记录实际完成的服务层，不代表机器UI、完整P0-011或整个G2验收。

## 已实现的独立职责

- `Assets/Game/Scripts/AutoEra/Machines/MachineDefinition.cs`、`MachineCatalog.cs`：强类型配置、数据行/型号/等级校验、缺失资源/配置分离；数据生成与覆盖见client-config-coverage.md。
- `MachineInstance.cs`、`MachineRoster.cs`：世界ID、唯一命名、型号历史序号、配置槽位、组件单一归属、载体唯一容量、现场/库/中枢权限、供电/运行/休眠分离；核心拆卸验证占用算力与已应用逻辑容量。
- `MachineRosterSnapshot.cs`：安全点的内存配置快照、隔离验证后恢复、保留ID高水位/历史号/名字/槽位/组件开关；真实电力和信号重新取得。不是磁盘存档，也不序列化未结束行为；有活动行为时明确拒绝快照。
- `MachineHardwareOperation.cs`：已确认管理意图的停止→安全等待→重新检查→安装/拆卸，取消仅取消尚未提交的意图；请求版本拒绝旧取消，退出解绑。不是玩家作业按钮。
- `MachineScheduling.cs`：32等待任务/16等待行为、五档FIFO、多效应器并行、保护安全点、恰好一次结果；`MachineComputePool.cs`：64等待、共享整额算力、显式合并与边界让出，无算法求值器或导航实现。

## 已收到独立QA证据

| 回归 | job | 结果 |
|---|---|---|
| MachineManagement 初次 | 1dbe92e6 | 3/3 |
| MachineManagement 后续 | 7b1c586a | 3/3 |
| MachineCatalog | 3a34309e | 2/2 |
| MachineScheduling | 789747e0 | 3/3 |
| MachineRecovery | ac85bbbc | 2/2 |
| MachineManagement 恢复后 | 7c3651c6 | 3/3 |

以上由QA回传Console Error/Warning=0，结束非PlayMode/非编译、8090释放。计数是不同轮回归，不能当新增用例数累加。

## 本轮发现与修复

控制流job66c7b851为1/2，不计通过。父任务已经Cancelling时，最后一个子行为结束调用Cancel因幂等提前返回，漏掉TryFinish。已改为活动计数减少后始终TryFinish；保留原失败测试作为回归，不放宽断言。

修正后：ControlFlow 110806c3 2/2；Scheduling cc72bb4c 3/3；Recovery 9667c0d0 2/2；Management 9f3e7372 3/3；ExecutionContext e69ee397 1/1；Catalog 387a3a46 2/2，共13/13，Console0。MachineExecutionContext连接状态与任务/算力/效应器分发许可，停电保留同一请求，复电恢复；本地任务不强制依赖远程信号。WorldSession拥有Roster并对称释放。

GF集成测试两次门禁问题均明确记录：第一次测试直接访问GF导致测试程序集缺Builtin.Runtime依赖，已改为调用产品MachineCatalog读入口，不扩asmdef；随后job260eacac因TestRunner隔离空场景未加载Launch而失败，尚未运行数据断言。已复用既有Startup测试做法，仅对隔离空场景Additive载入已保存Launch，不保存用户场景；等待新回归，不引用失败作业为GF通过。

随后独立回归：MachineQueueBoundary job4807cfc4 3/3（16/64满拒绝、不抢占不可中断、重复结束保护、100条历史、重复执行确定性）；MachineGameDataIntegration jobeed36ae3经PlayMode域重载，以XML权威1/1通过，读取GF真实两张机器表及语言、二级核心75/50与资源缺口。Console0，回到非PlayMode，8090释放。该轮之后又增加Catalog40项真实GF读取断言，新增断言另行回归。

资源扫描见client-resource-audit.md：40唯一ID，15条非空Prefab及meta/GUID存在，25空项有明确状态；这不是Unity内部MissingReferences扫描。产品模式纯度审计与项目边界审计通过。默认严格框架审计报告9项已授权产品MainMenu/InitialRegion项；使用ProjectBaseline指定的 `--product-profile tools/audit_product_profile.json` 才是当前产品审计口径，未修改审计豁免配置。

用户任务表SHA-256仍为DFBE954F3907E965B25EEE5C769413C6B5176483D1982C582FD318E57068EEB5。B11没有新增ScriptsBuiltin修改；工作区既存框架修改属于之前B10/导出工具现场，未清理或纳入本段声明。

## 最新独立回归与安全停止

- 管理完整度/维修边界：64432ccd，4/4。
- 执行上下文、组件拆下后队列失效与重新绑定：21cde315，2/2。
- 三表目录与真实资源路径：3572111c，3/3；队列边界：c83afcf4，3/3。
- GF实际启动读取含40项目录：698bc778，XML 1/1；以上一轮共13/13，Console Error/Warning=0。
- 安全停止收尾：21e3b72c，MachineExecutionContext 3/3；bfd86376，MachineControlFlow 2/2；c74309fb，WorldSession 2/2，共7/7。Console Error/Warning=0，非PlayMode/非编译，QA释放8090。

停止不再同时禁止已有物理动作到达安全点：新行为分发关闭，但已激活、供电、组件启用且非休眠时允许当前行为安全收尾。可安全点中断的行为报告安全点后以Partial结束，然后硬件操作重新检查并提交拆卸；不可中断行为保持当前指令直至执行器报告结束。停电/休眠不伪造动作进度或终态。此处只提供执行许可和结果合同，不声明已接入全部实体动画。

## 仍未覆盖的交付边界

最后配置门禁包 `b11-catalog-invalid-inputs`：快速执行队列记录Unity job6853fba8，MachineCatalogEditModeTests 14/14，失败/跳过0，Console Warning/Error=0，非编译/更新。主窗口复核测试按真实TXT第二行字段名修改内存字符串，不写正式数据；覆盖负价格、NaN/负功率、非法等级/身份/可用性、越界Prefab、非法组件Kind、Ready缺Prefab。途中测试辅助函数把行文本当文件路径、以及忽略输入重载的问题已修正；最终测试结果仅引用6853fba8。

受控生成复用既有DataTableGenerationProfileEditModeTests的指纹、路径越界与事务回滚合同，B11未改转换器；本批真实生成和GF读取成功已记录。未对整个多表批次承诺原子提交，也未将缺正式数值条目冒充可实例化。

缺正式参数尚不能实例化的定义保持PendingConfiguration；核心/传感器美术未交付标PendingResource。测试Fixture容量不是正式平衡数据。当前服务尚未全部接入世界场景与GF硬件UI；不以纯C#通过替代现场交互、UI视觉用户确认或完整运行工作流。后续依次补调度与状态组合、实际GF数据读取、经批准的UI接线。没有自动Git请求或任务表写入。
