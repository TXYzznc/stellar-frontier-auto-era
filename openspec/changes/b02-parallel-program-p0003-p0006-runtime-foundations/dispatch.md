# b02-parallel-program-p0003-p0006-runtime-foundations：实施派发

> 本派发单补充既有OpenSpec的多窗口实施边界，不改变已由用户确认的proposal、design、specs或tasks。

## 1. 身份信息

| 字段 | 内容 |
|---|---|
| 派发ID／OpenSpec | `b02-parallel-program-p0003-p0006-runtime-foundations` |
| 管理批次 | `b02` |
| 原始任务ID | `P0-003`、`P0-004`、`P0-005`、`P0-006` |
| 实施窗口 | `AutoEra｜程序（客户端）` |
| 验证窗口 | `AutoEra｜测试`；实现完成并释放8090后接手 |
| 协作状态 | `实施中` |
| 制作人入口 | `AutoEra｜制作人` |

## 2. 实施基线

- 方案以本change现有`proposal.md`、`design.md`、`specs/`和`tasks.md`为唯一实施依据。
- 既有设计于2026-08-21提交，用户已在此前讨论中逐项确认；本轮不重新开放无冲突决策。
- 若实施前检查发现当前框架、FSR、Unity、任务表依赖或公共契约已经变化，必须暂停并回传，
  不得自行改变设计后继续。
- 任务表永久只读。P0-003～P0-006的状态、实际工时和备注只能由用户手动维护。

## 3. 实施前检查

- [ ] 记录任务表SHA-256、Git状态、Unity编译／PlayMode状态，确认用户工作簿修改不进入提交。
- [ ] 严格校验当前OpenSpec，确认所有spec与tasks可读取且没有未决问题。
- [ ] 只读复核AppConfigs、Procedure链、GameData生成器、场景和AutoEra目录的当前状态。
- [ ] 向制作人返回精确写入目录、高冲突文件、Unity操作计划和分段提交计划。
- [ ] 制作人登记写锁和Unity 8090锁后才开始修改。

## 4. 修改权限与占用

### 已登记写入范围

- `Assets/Game/Scripts/AutoEra/`
- `Assets/Game/Tests/AutoEra/`
- `GameData/AIData/DataTables/Foundation/`
- `GameData/AIData/Configs/Foundation/`
- `GameData/AIData/Languages/Foundation/`
- `GameData/AIData/GenerationProfiles.json`
- 工具生成目标：`GameData/{DataTables,Configs,Languages}/Foundation/`及对应`Assets/Game/{DataTable,Config,Language}/Foundation/`输出；AI不得直接创建或修改这些xlsx
- `Assets/Game/ScriptableAssets/Core/AppConfigs.asset`
- `Assets/Game/Scene/`中本change新增的主菜单与第一版空世界场景及其`.meta`
- `Assets/Game/Scripts/Extension/DataTableExtension.cs`
- OpenSpec现有`tasks.md`、本change验证证据和与实施直接对应的项目开发文档

### 用户单次授权的框架核心文件

用户于2026-08-24明确允许本change修改以下三个文件，仅限完成P0-006所需的通用生成器
配置化和类型解析兼容工作：

- `Assets/Game/ScriptsBuiltin/Editor/GameDataGenerator.cs`
- `Assets/Game/ScriptsBuiltin/Editor/DataTableGenerator/DataTableGenerator.cs`
- `Assets/Game/ScriptsBuiltin/Editor/DataTableGenerator/DataTableCodeTemplate/DataTableCodeTemplate.txt`

该授权不扩展到`Assets/Game/ScriptsBuiltin/`中的任何其他文件；如发现需要第四个文件，必须暂停并
重新取得用户对精确路径的授权。

### 方案修订后用户新增授权的框架文件

用户于2026-08-24明确确认允许本change修改／新增以下精确路径，用于实现已确认的
三类JSON中间层、安全同步管线和Editor-only生成Profile：

- `Assets/Game/ScriptsBuiltin/Editor/AIGameDataTableGenerator.cs`
- `Assets/Game/ScriptsBuiltin/Editor/AppConfigsInspector.cs`
- `Assets/Game/ScriptsBuiltin/Editor/Common/ConstEditor.cs`
- `Assets/Game/ScriptsBuiltin/Editor/Diagnostics/GFDiagnosticRunner.cs`
- 新增 `Assets/Game/ScriptsBuiltin/Editor/AIData/AIGameDataContracts.cs`
- 新增 `Assets/Game/ScriptsBuiltin/Editor/AIData/AIGameDataSyncPipeline.cs`
- 新增 `Assets/Game/ScriptsBuiltin/Editor/AIData/AIConfigAdapter.cs`
- 新增 `Assets/Game/ScriptsBuiltin/Editor/AIData/AILanguageAdapter.cs`
- 上述新增`AIData`目录、四个新文件及Unity生成的对应`.meta`文件

新增文件保持领域无关，不包含`AutoEra`值；产品输出规则只存在于项目JSON Profile。
该新增授权不扩展到此清单之外的其他`Assets/Game/ScriptsBuiltin/`文件。

### 必须在预检后精确登记的高冲突范围

- `AppConfigs`及其Procedure／DataTable／Config／Language登记文件
- 主菜单与第一版空世界场景及其`.meta`
- 通用DataTable生成器、类型解析和相关回归测试
- 其他现有共享配置、生成输入或场景文件

### 只读／禁止修改

- `Docs/GameDesign/05-开发计划/第一版开发任务表.xlsx`永久只读。
- 除上一节明确列出的三个文件外，`Assets/Game/ScriptsBuiltin/`继续禁止修改。
- 不修改asmdef、HybridCLR、Obfuz、FSR或发布热更新策略。

### 工具与Git锁

| 资源 | 当前状态 | 规则 |
|---|---|---|
| 主工程Unity 8090 | 已核验并授予客户端实施占用 | 客户端释放后测试窗口才能接手 |
| Git索引／提交 | 未授予长期占用 | 每个分段提交前向制作人取得短期锁，显式暂存范围路径，提交后立即释放 |

所有窗口只提交、不推送。禁止`git add -A`、`git add .`或把用户任务表修改带入提交。

## 5. 实施与回传

- 按`tasks.md`顺序推进，适合解耦的纯C#测试和数据生成回归可以先行。
- 每完成一个可独立验证的实施段，先运行对应测试／审计，再申请Git索引锁提交。
- 结构、字段、序列化、场景、生成器和程序集相关变更必须退出PlayMode按普通编译验证；
  FSR不得替代完整编译。
- 适合人工触发观察的能力按既有功能验证中心规则提供入口；不适合面板的底层能力提供测试、
  日志或报告证据。
- 回传必须包含完成的task编号、修改范围、测试／Unity证据、提交号、用户验收入口、限制、
  未决项和已释放的锁。

## 6. 暂停与升级条件

- 需要改变现有OpenSpec决策、拆分asmdef、引入第三方DI或修改发布热更新策略。
- 需要修改任务表、任务依赖、DoD或框架核心禁止范围。
- 当前AppConfigs、场景、生成器或Git索引存在未交接修改。
- Unity 8090项目身份、编译状态或PlayMode状态不满足结构性实施要求。
- 实现发现四项基础无法继续形成共同生命周期或验收边界。

发生以上情况时按`Docs/Development/Dispatch/PauseAndRecovery.md`暂停并回传制作人。
