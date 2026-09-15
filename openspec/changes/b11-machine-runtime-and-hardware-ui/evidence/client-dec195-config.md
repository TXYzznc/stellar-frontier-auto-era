# DEC-195 配置解除与实例验证

2026-09-10。依据GameDesign决策记录DEC-195及B11派发补充；仅解除配置依赖，UI视觉批准仍独立。

## 工具生成前后

- 唯一作者输入：`GameData/AIData/DataTables/Machines/MachineDefinitions.json`、`ComponentDefinitions.json`。保留源指纹，由既有工具校验并转换，不手写xlsx/TXT/C#。
- 10021：HP100、基础容器10、功率0.1/1、基础价350/回收280、Ready；新增10022：HP120、容器12、功率0.1/1.2，其余继承型号。
- 22061：增加容量30、功率0/0、基础价400/回收320、被动效应器、Ready；新增22062：增加36，其余继承，升级不抬回收价。
- 槽位、Prefab路径、动作合同没有修改。零功率是完整合法数据，不代表未配置。
- 原生菜单 `Game Framework/AutoEra/Generate Machine Data` 已执行；实际生成TXT确认四行精确值，普通编译结束、Console Error=0。菜单成功并未单独作为数据正确证据，已读回生成文件核对。

快速录入包初次PASS只验证JSON可解析/ID唯一，漏改一级两行及二级货舱容量，未被客户端签收。客户端逐值复核并修正后才执行正式生成。没有将错误初版转换进正式数据；补丁首次匹配失败没有证实跨窗口并发冲突。

## 回归范围

`MachineCatalogEditModeTests`新增两等级用例：读取工具生成行、检查准确值与合法零功率；单容器安装后40/48，超用不能拆卸，清空可拆回10/12；实际PrefabUtility在隔离PreviewScene实例化正式固定载体与货舱，检查Renderer和序列化Missing引用，finally关闭。没有修改/保存正式Prefab或场景。

`MachineGameDataIntegrationTests`扩展已有真实GF启动读回，确认两个等级从GF取得、Ready可解析。此测试不等于玩家GF Entity展示或硬件UI端到端；后两者仍属4.x。

QA任务：b11-dec195-config-instances。本轮未通过：Catalog e81ec9fb与刷新后ad3909d0均仅14/14，缺新增两条实例用例，不能计DEC-195实例验收；GF 6f1a3748 XML 0/1，等待30秒后FromLoadedGameData发生NullReferenceException，未读回成功。

客户端继续核验：磁盘确有新增两[TestCase]；8090项目和PID对应主工程。普通asset_refresh、debug_force_recompile（源码为RequestScriptCompilation）、定向asset_reimport_batch八个Machine测试脚本后，Library/ScriptAssemblies/AutoEra.Editor.Tests.dll仍为本地19:13:47，而新增测试为21:01后；新test_list仍无ApprovedFixedCarrierAndCargo。Console Error=0不代表已编译新代码。未删除Library、未解除未知锁或重启Editor。已保存Launch，非PlayMode；释放8090并请求制作人协助恢复普通编译门禁。恢复后须重新验证16条及GF，不能沿用14条或旧成功证据。

## 2026-09-11 根因更正与恢复

`unity_diagnose.console.logs`保留了CS7036：新增测试构造PersistentObjectRegistry时漏传allocator。虽然同次summary的consoleErrorCount=0以及先前console_get_logs返回0，不能据此判断没有编译错误。此前“共享编译锁”仅为未证实假设，现撤销要求用户重启/解锁的处理；已实际通知制作人更正。

已将构造修正为`new PersistentObjectRegistry(ids)`，普通asset_refresh后DLL更新至2026-09-11 10:06:21，script_get_compile_feedback该文件hasErrors=false。QA重新登记b11-dec195-config-instances-v2，须发现两条新增实例用例并通过16+GF1，不沿用旧14条。

新DLL后QA v2未运行，因为test_list仍返回旧382项缓存；客户端检查DLL包含新增方法，并显式`test_discover_start`得到fe1942d7，completed384项，明确包含新增(1)/(2)。已再次交QA v3运行16+GF1。未修改工具包或测试断言；发现缓存与编译错误分开处理。

## 最终新程序集验证

2026-09-11 QA v3：MachineCatalogEditModeTests job f19bf9be，16/16通过，明确包含两条ApprovedFixedCarrierAndCargo；MachineGameDataIntegrationTests job a6247136，TestResults.xml 1/1通过。新数据、合法零功率、两等级容量、正式Prefab隔离实例/Missing引用和GF读回均通过。Console Error/Warning=0，Unity非PlayMode/非编译，8090释放。旧GF空引用未在修复编译后的本轮复现，不据此修改业务启动逻辑。

产品纯度（product-profile）、项目边界及OpenSpec严格校验通过；用户任务表SHA-256仍为DFBE954F3907E965B25EEE5C769413C6B5176483D1982C582FD318E57068EEB5。

## 排除

无槽位/动作/经济升级系统改动；无Git操作、无任务表编辑；原B11缺值依赖由本段替代。仍待用户视觉批准及2D完整机器UI交付后接线。
