# 数据暂存事务与Foundation最小输入

2026-09-10，B10基础补齐，精确框架授权见 `Docs/Development/Dispatch/Active/b10-client-foundation-completion.md`。

## 已实施

- DataTable在Temp生成并校验；xlsx、TXT、可选bytes、Profile C#、JSON指纹及适用的UIViews/分组枚举统一进入本次替换清单。Core分组读取未变更兄弟表快照，提交前比较依赖指纹。
- Config/Language Reverse同时暂存实际运行输出与JSON指纹，不再先提交xlsx后单独写JSON；配置工作表指纹按完整矩形补齐空列。
- 目标文件用独占句柄保持到提交/恢复结束；比较准备期指纹。正在失败的写入纳入恢复，后续锁冲突恢复前项及移除新项，恢复失败保留备份目录并报告。
- 同一同步操作的文件集合具有失败补偿，不是操作系统级多文件原子提交；不覆盖进程崩溃/断电恢复。
- 原生成签名保留，同一模板实现增加显式暂存输出，逻辑表名/Profile与Temp路径分离。
- Const.Groups输出路径纠正为现有Common/Const.Groups.cs；未搬移/重写正式分组，GUID仍为720fc83d0ecd24540a801e8ea9f29fb9。
- Excel监听去LastAccess，线程安全收集、主线程快照消费，保留Created/Renamed，精确版本抑制事务重复事件；空列表无生成，有限表列表不再遍历全部Core。

## QA逐轮事实

- 610d4c4d：20/20，早期事务基础。
- 05930a16：24/25，分组输出旧路径失败，不计全过。
- 30b8b81b：27/28，路径修正后文档头与生成文档头逐字比较失败；另外3条Core原始列错误暴露旧自动监听全量生成，非预期错误，随后修复。
- 65da2dc9：30/30，含三类重复Reverse、Core特殊枚举语义、Profile暂存、基线/依赖冲突、当前写入失败、恢复失败保留备份、空刷新及版本抑制。普通Console Error=0、Warning=0。
- 2c020366：31/31；正式数据加载与启动联合回归见segment-25。
- 最后静态复核发现CheckRawData仅检查列名，LoadFromBytes=false时未逐项解析非ID数值。现用同一既有序列化器在Temp验证所有类型，仍只按配置发布bytes；新增非法int且零正式输出改动用例，32项补验待回传。
- 补验首轮未启动：测试程序集没有Newtonsoft引用，已移除测试的该依赖，不扩asmdef。v2 `084bf038`31/32：非法类型已被拒绝，但测试漏接旧解析器首条字段诊断；v3改为精确接收字段和表两条诊断，不使用全局忽略。

诊断质量残留：旧DataTableProcessor的GetRowBytes在非法值时报告字段错误并返回null，GenerateDataFile随后捕获NullReference并返回false。事务入口拒绝写入，不存在该异常逃逸后继续提交；但二次诊断不够清晰。该Processor不在本包新增授权范围，没有擅自改它；交付时单独列出，不声称已修复解析器内部诊断。

v3 QA job `8d439b14`，本机权威TestResults.xml（09:43:42Z至09:43:56Z）32/32通过，非法类型负例与三类重复往返均通过。测试后普通asset_refresh，负例保留；新鲜Console捕获Error/Warning/Exception/Assert均0，非PlayMode、非编译，8090释放。

## 正式生成

通过 `Game Framework/AutoEra/Generate Foundation Startup Data` 原生工具生成（非AI直接写xlsx）：
- Config Foundation/Runtime：已有五项场景/时间配置。
- DataTable Foundation/StartupMessages：Id 1/2引用AutoEra.Startup.MenuReady/WorldReady。
- Language Foundation/Startup：上述两键文本。
- AppConfigs保留原Core项，追加Foundation/StartupMessages及Foundation/Startup。
- 当前LoadFromBytes=false，正式输出TXT/Language JSON/Profile C#；测试覆盖适配器及暂存二进制入口，不为正式配置伪造多余bytes文件。

正式运行入口验证GF加载的StartupMessages全部键引用存在，必须具备阶段1/2；配置仍由原AutoEraRuntimeSettings校验，不建立完整对象配置或第二时间权威。

## 候选路径（仅本单元；不自动Git）

- Assets/Game/ScriptsBuiltin/Editor/AIGameDataTableGenerator.cs
- Assets/Game/ScriptsBuiltin/Editor/AIData/AIGameDataSyncPipeline.cs
- Assets/Game/ScriptsBuiltin/Editor/AIData/AIConfigAdapter.cs
- Assets/Game/ScriptsBuiltin/Editor/AIData/AILanguageAdapter.cs
- Assets/Game/ScriptsBuiltin/Editor/DataTableGenerator/DataTableGenerator.cs
- Assets/Game/ScriptsBuiltin/Editor/GameDataGenerator.cs
- Assets/Game/ScriptsBuiltin/Editor/Common/ConstEditor.cs
- Assets/Game/ScriptsBuiltin/Editor/DataTableUpdater.cs
- Assets/Game/Scripts/AutoEra/Editor/AutoEraFoundationDataSetup.cs
- Assets/Game/Scripts/AutoEra/Application/AutoEraRuntimeSettings.cs
- Assets/Game/Scripts/AutoEra/Procedures/AutoEraStartupProcedure.cs
- Assets/Game/Tests/AutoEra/Editor/DataTableGenerationProfileEditModeTests.cs
- Assets/Game/Tests/AutoEra/Editor/AutoEraRuntimeSettingsEditModeTests.cs
- Assets/Game/ScriptableAssets/Core/AppConfigs.asset
- GameData/AIData/Configs/Foundation/Runtime.json
- GameData/AIData/DataTables/Foundation/StartupMessages.json
- GameData/AIData/Languages/Foundation/Startup.json
- GameData/Configs/Foundation/Runtime.xlsx（原生工具产物）
- GameData/DataTables/Foundation/StartupMessages.xlsx（原生工具产物）
- GameData/Languages/Foundation/Startup.xlsx（原生工具产物）
- Assets/Game/Config/Foundation/Runtime.txt及对应meta
- Assets/Game/DataTable/Foundation/StartupMessages.txt及对应meta
- Assets/Game/Language/Foundation/Startup.json及对应meta
- Assets/Game/Scripts/AutoEra/DataTable/StartupMessages.cs及对应meta
- 上述新增目录的Unity meta（如有）；提交前按实际清单核对。

排除用户任务表、其它窗口文件、既有未提交但不属本单元的改动；Git只由用户手动触发。
