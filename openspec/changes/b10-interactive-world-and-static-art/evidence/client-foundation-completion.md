# 基础剩余项实施记录

2026-09-10，b10-client-foundation-completion，延续b02；不自动Git、不直接编辑xlsx。
执行前任务表SHA-256：DFBE954F3907E965B25EEE5C769413C6B5176483D1982C582FD318E57068EEB5。

## 时间单元（实施前精确范围）

- `Assets/Game/Scripts/AutoEra/World/Time/WorldClock.cs`：仅Editor/Development编译的倍率推进入口，验证有限非负输入，乘积溢出失败不改变时间；正常入口不变。
- `Assets/Game/Scripts/AutoEra/World/Region/InitialRegionScene.cs`：仅开发编译下的非序列化倍率，默认1，无玩家UI控制；正式构建始终调用原真实时间推进。
- `Assets/Game/Tests/AutoEra/Editor/WorldClockEditModeTests.cs`、`AutoEraRuntimeSettingsEditModeTests.cs`：数值、分帧/直接跨昼夜和配置错误边界。
- `Assets/Game/Tests/AutoEra/Editor/AutoEraStartupFlowEditModeTests.cs`：实际GF管理UI打开时世界继续推进；不将fake UI代替真实打开。
- 必要时通过既有产品Editor取证入口调用GF.UI，不改框架/程序集；独立QA记录结果后才勾选。

快速执行候选：本单元包含浮点/溢出与生命周期判断，专业实现直接完成；冻结后的独立测试交QA。

时间单元已通过独立QA13/13，见b02 `evidence/segment-23-world-time-boundaries.md`；5.1/5.2/5.5按该证据勾选。

## 数据单元精确输入与生成范围

- 保留 `GameData/AIData/Configs/Foundation/Runtime.json` 的五项已有启动/世界时间配置，不新增第二时间权威。
- 新建 `GameData/AIData/DataTables/Foundation/StartupMessages.json`：Id(int，1/2)、MessageKey(string，AutoEra.Startup.MenuReady/WorldReady)。仅验证启动阶段消息引用，不添加对象/玩法配置。
- 新建 `GameData/AIData/Languages/Foundation/Startup.json`：上述两键对应“主菜单已就绪”“初始区域已就绪”。语言表作为已选语言基础词典的项目补充，通过既有AppConfigs追加加载。
- 工具产物：GameData三类Foundation对应xlsx；Assets/Game/DataTable/Foundation/StartupMessages.txt、Config/Foundation/Runtime.txt、Language/Foundation/Startup.json；LoadFromBytes启用时对应bytes；AutoEra/DataTable/StartupMessages.cs及必要meta由既有生成/刷新产生。AI不直接写xlsx/生成C#。
- AppConfigs仅登记DataTables Foundation/StartupMessages和Languages Foundation/Startup，保留已有全部Core项与Runtime配置。
- 产品启动验证真实GF已加载表ID/MessageKey及本地化字典键存在；非法引用可定位。测试负例不改正式Core。

## 事务修复历史

制作人已授权六个精确框架文件。底层独占目标句柄贯穿写入/恢复，覆盖失败中的目标，恢复失败保留备份；提交前比较基线，聚合只读依赖持读锁。多文件不是OS级原子事务，进程崩溃/断电不在本次自动恢复保证内。
QA `610d4c4d` 20/20仅证明早期底层局部回归；后续整合新增测试另行记录，不复用此结果冒充新程序集测试。
QA `05930a16` 24/25：Core特殊代码比对因ConstGroupScriptFileFullName残留Common/Core路径失败；Config/Language重复Reverse通过。已获常量单行授权并修正为现有Common/Const.Groups.cs；既有GUID `720fc83d0ecd24540a801e8ea9f29fb9` 未变。后续v2回归包含28项，当前待结果，不将24/25写为全过。

上述为当时检查点；后续结果与失败原因完整保留于b02 `segment-24-data-transactions-and-foundation.md`，不是当前仍等待旧v2。框架授权最终为8个精确文件，见派发单追加条目。

## 剩余生命周期与声音验收切口（实施前）

- 在现有AutoEraStartupFlowEditModeTests增量验证真实加载中GameEntry.Shutdown(Restart)，保留旧会话引用检查释放，重启后菜单可进入且无重复实体；闭包在EnterPlayMode域重载后创建。
- 半初始化失败用测试实例的缺失种子/实体映射触发，不改正式场景或Prefab；检查Region、注册表和事件订阅对称释放，再验证新会话仍可用。
- 声音负例通过既有SoundComponent请求一个不存在的测试音频路径，核验PlaySoundFailure事件的资源/组/错误字段；不新增声音组、不生产音频。随后原UI、Entity分组和菜单/世界流程仍有效。预期负例日志单独记录，恢复后普通Console检查不忽略业务错误。
- 框架/场景重启若暴露授权外核心问题，准确报告入口，不用仅Dispose纯对象假冒真实Restart覆盖。

## 联合验证与交付边界

最终九组已由QA完成：普通52/52、Startup真实PlayMode XML6/6；逐项job与内容见b02 `segment-25-startup-failure-and-group-recovery.md`。恢复后普通Console Error=0、Warning=0。文本模式类型解析补验独立记录在segment-24，不复用旧程序集结果。

2026-09-10收口检查：产品profile框架纯度与项目边界审计通过，Python审计测试14+7通过，b02/B10 strict通过，候选diff check通过。任务表执行前后SHA-256一致：DFBE954F3907E965B25EEE5C769413C6B5176483D1982C582FD318E57068EEB5。

保留边界：不覆盖断电/进程崩溃的多文件原子恢复；不把缺失声音资源负例视为全部音频硬件链通过；物理键鼠/手柄完整实机链、完整P0-011对象配置、生产/物流/存档和离线调度未在本包实施。UI、实体、已有正式美术不重新制作。

工时口径：队列Active起点2026-09-10 08:58:26 UTC；截至09:41约43分钟为本次连续执行墙钟时间（含QA），不是人工工时，也不回填历史P0实际工时。最终结束时间以队列完成记录为准。建议用户根据本包证据核对P0-003～006基础实现状态；P0-012只增加资源失败隔离证据，其他任务不因本次测试数量自动完成。

候选文件：数据单元精确清单见segment-24；其外本包涉及WorldClock.cs、InitialRegionScene.cs、InitialRegionRuntimeEvidence.cs、WorldClockEditModeTests.cs、AutoEraStartupFlowEditModeTests.cs，分别位于既有AutoEra产品/Editor测试目录；b02 tasks及segment-23～25、本文件为证据变更。既有其他窗口未提交现场不归本包，用户任务表严格排除。没有使用Git索引或自动提交请求。

时间/生命周期额外精确路径：
- Assets/Game/Scripts/AutoEra/World/Time/WorldClock.cs
- Assets/Game/Scripts/AutoEra/World/Region/InitialRegionScene.cs
- Assets/Game/Scripts/AutoEra/Editor/InitialRegionRuntimeEvidence.cs
- Assets/Game/Tests/AutoEra/Editor/WorldClockEditModeTests.cs
- Assets/Game/Tests/AutoEra/Editor/AutoEraStartupFlowEditModeTests.cs
- openspec/changes/b02-parallel-program-p0003-p0006-runtime-foundations/tasks.md
- openspec/changes/b02-parallel-program-p0003-p0006-runtime-foundations/evidence/segment-23-world-time-boundaries.md
- openspec/changes/b02-parallel-program-p0003-p0006-runtime-foundations/evidence/segment-24-data-transactions-and-foundation.md
- openspec/changes/b02-parallel-program-p0003-p0006-runtime-foundations/evidence/segment-25-startup-failure-and-group-recovery.md
- openspec/changes/b10-interactive-world-and-static-art/evidence/client-foundation-completion.md

## 本包完成检查点

09:46 UTC左右完成，约48分钟本次连续墙钟（包含QA等待，不等同人工工时）。最后类型补验`8d439b14`32/32，普通刷新后新鲜Console Error/Warning/Exception/Assert均0；此前九组58/58按原程序集保留，最后数据组由32项证据补充。b02已按实际证据勾选，B10整批静态美术及用户验收不因此完成。

剩余非阻塞问题：既有DataTableProcessor对非法类型输出字段诊断后又捕获空引用，造成两条失败诊断；新事务门禁已拒绝正式写入。未擅自修改授权外Processor，供制作人归集问题及用户决定后续处理。

本包资源释放：QA已释放8090，客户端不再驱动；Git索引未使用；客户端Pending/Suspended为空。正式完成消息另经窗口接口发送，非仅本文件声明。
