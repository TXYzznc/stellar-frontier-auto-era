# B10 G0/G1 独立验收矩阵

日期：2026-09-10  
负责人：QA  
范围：`b10-initial-region-independent-validation` 的工程基础与初始区域独立验收。本文只记录复用证据、待测边界和后续准入条件；不把旧任务表未勾选状态判为失败。

## G0：既有基础复用核验

| 验收项 | 复用依据 | 当前结论 | B10 后续动作 |
|---|---|---|---|
| 应用组合根与过程上下文 | b02 `segment-18-application-context.md`：`AutoEraApplicationContextEditModeTests` 1/1 通过 | 已有基础可复用；不是 B10 重写对象 | 初始区域接入后复跑相关生命周期/注销边界 |
| Operations UI 与公开状态边界 | b05 `segment-07-operations-runtime-binding.md`：`AutoEraUiPrefabBindingEditModeTests` 4/4、`AutoEraUiOperationContractsEditModeTests` 7/7；b09 job `10ba35c9` 为 6/6 | UI 结构与权威状态/可信进度合同已有证据 | P1 侧栏/HUD 新字段只验最小新增绑定；不重做既有页面 |
| 正式 Entity、目录和动作图 | b09 `segment-02-formal-entity-handoff-and-retirement.md`：Entity 23/23、MotionGraph 12/12；当前回归 job `51cce3b3` 12/12 | 正式 Entity 资产与动作目录已接管 | 初始区域代理只验证注册、摘要、选择和作业语义 |
| 机械表现与传送带 | b06 `segment-59-conveyor-editor-preview-timeline.md`、b09最终回归 | 现有动作/传送带表现可复用 | 不把表现状态作为资源生产结论 |
| UI 引用完整性 | `font-directory-unification-regression-editmode`：job `f92e81e5` 6/6，`validate_missing_references=0` | 字体迁移后 Launch 引用有效 | 每次场景/Prefab接入后检查相关 Missing Reference 与 Console |

## G1：初始区域用例矩阵

| ID | 能力/场景 | 预期可复跑证据 | 状态 |
|---|---|---|---|
| G1-01 | 正式入口进入固定初始区域 | 保存场景入口、普通编译、Console Error=0 | 待客户端基础/区域场景可测 |
| G1-02 | 机器、建筑、资源点注册及公开摘要 | 稳定 ID；摘要不包含伪造生产结果 | 待实现 |
| G1-03 | 选择、无选中、失效注销与 UI 拦截 | 注销后侧栏不保留可执行失效操作；输入不穿透 UI | 待实现 |
| G1-04 | 占地与旋转吸附 | 合法占地成功；冲突/非法位置拒绝且无残留占用 | 待实现 |
| G1-05 | 有限作业位竞争与等待队列 | 按既有顺序移交；无重复拥有/重叠作业 | 待实现 |
| G1-06 | 地表矿脉有效作业区域 | 可通行；不恢复旧立体矿山禁行或手动钻探点规则 | 待实现 |
| G1-07 | 阶段收口 | 用户可查看入口、通过/未覆盖项清单、Missing Reference=0、相关 Console Error=0 | 待 G1-01 至 G1-06 |

## 新增测试协调与资源规则

- 若现有测试不能覆盖 G1-02 至 G1-06，先向客户端发送精确测试类/文件建议并取得不重叠确认，再在 `Assets/Game/Tests/AutoEra/` 创建明确 QA 命名测试。
- 8090 仅在客户端点对点交接后驱动；不保存场景、不修改实现或资源、不操作 Git 或 xlsx。
- 本矩阵不构成阶段通过；G1 只有在客户端提供可测初始区域入口后才产生实际验收结论。

## 已确认的首段独立测试

客户端已确认其首段四项不覆盖以下边界。QA 已新增
`QAInitialRegionBoundaryEditModeTests`（待客户端普通刷新后执行）：

1. 会话先结束后，区域不得继续公开陈旧对象；后续 `Dispose` 必须幂等。
2. 两个不同资源点的作业通道可以并行持有不同机器。
3. 等待者注销后，拥有权移交必须跳过该失效等待者。
4. 45° 旋转的边界临界与接触不重叠必须稳定。

## 首段实际回归（2026-09-10）

- 客户端既有 `InitialRegionEditModeTests`：Unity job `13ca3eb1`，4/4 通过。
- QA 独立 `QAInitialRegionBoundaryEditModeTests`：Unity job `24fc698d`，4/4 通过；包括会话先结束时不再暴露区域对象、随后区域释放幂等。
- 结束状态：Unity 2022.3.62f3c1，非 PlayMode、非编译；Console Error=0、Warning=0。

该结果只覆盖纯 C# 区域基础。正式初始区域场景、对象摘要 UI、占地输入接线和用户可查看入口尚未提供，故 G1 阶段不通过也不失败，继续保持待测。

## 第二段 HUD 序列化接线回归（2026-09-10）

- `RegionHudPresenterEditModeTests`：Unity job `482ef2e7`，1/1 通过。
- `AutoEraUiPrefabBindingEditModeTests`：Unity job `7efef700`，6/6 通过。
- `validate_missing_references`：0 项；结束时 Console Error=0、Warning=0，Unity 非 PlayMode、非编译。

该段只确认 `FieldHudForm` 现有节点的序列化接线和既有 UI 不回归；没有正式区域场景、对象选择输入或用户可查看入口，因此不改变 G1 待测结论。

## 第三段独立区域场景回归（2026-09-10）

- `InitialRegionSceneEditModeTests`：Unity job `878e25a3`，1/1 通过。
- 该用例覆盖独立 Additive 的 `InitialRegion` 加载/关闭、8对象实际注册、释放后重进 ID 不复用、`NewRegionSelection=8` 全碰撞隔离及 NavMesh 采集显式排除。
- 结束状态：Console Error=0、Warning=0，Unity 2022.3.62f3c1 非 PlayMode、非编译；未保存其他场景。

正式 Launch 流程尚未接入，故这不是 G1 入口通过证据。

## 配置段回归（2026-09-10）

- `AutoEraRuntimeSettingsEditModeTests`：Unity job `60e4f4f9`，2/2 通过；客户端前置已验证既有 AIConfigAdapter Validate→Reverse 与 GameDataGenerator 的 Foundation/Runtime 正式输出，未直接写入 xlsx。
- 结束状态：Console Error=0、Warning=0，Unity 2022.3.62f3c1 非 PlayMode、非编译。

## G1 补充边界核对（2026-09-10）

| 边界 | 实际证据 | 当前结论 |
|---|---|---|
| 输入与 UI 拦截 | `InitialRegionSceneEditModeTests` 的已运行 job `818280b2`/`c823804a`：实际 `RegionInputModule` 接收 `FixedSource`，`Tick(..., true)` 不移动相机、`Tick(..., false)` 移动，且选择/Highlight 清除通过 | UI 拦截分支已自动覆盖；真实物理鼠标/触屏全时序尚未独立覆盖 |
| 加载期间禁重入 | `AutoEraStartupFlowEditModeTests` 仅等待 Menu Button 可交互后单次点击，代码有 loading 时不可交互路径 | 无双击/重复提交的自动化断言；待补 |
| 失败重试与晚回调 | 最近 `TestResults.xml` 中 `MissingWorldScene_ReturnsToExistingMenu_AndRetryRecovers` Passed，10.308966s：失败返回既有菜单、按钮可重试、恢复后8个实体 | 失败/重试已自动覆盖；没有独立晚回调拒绝测试 |
| 队列拥有者释放移交 | `InitialRegionEditModeTests` job `56769587` 的 FIFO、无效拥有者/目标释放；优先级不抢占与双通道公开投影；QA边界 job `24fc698d` 的失效等待者跳过/独立通道 | 纯C#移交语义已自动覆盖；尚无经真实场景输入触发的端到端队列移交 |
| 运行视觉增量 | ArtResource `RuntimeIncrementReview.md` 已逐张复核E03–E13，均为1920×1080；覆盖菜单对比、无选中、三类摘要、注销、非零等待、长状态、只读、失败/恢复 | 不替代输入、禁重入、晚回调或队列移交测试；保留视觉组合缺口 |

### 明确未覆盖的视觉项

- 长对象名 + 长状态 + 只读提示的组合极限；
- 菜单悬停/焦点的可见区别；
- 稳定的瞬时加载禁用截图。

这些项目仅留作整批报告的视觉未覆盖项，不据现有截图或客户端口述勾选，也不自动扩展正式菜单设计。

## 占地预览与高亮回归（2026-09-10）

- `RegionPlacementPreviewEditModeTests`：Unity job `d932224a`，1/1 通过，覆盖吸附、提交时再验证、取消与回调只触发一次。
- `InitialRegionSceneEditModeTests`：Unity job `c823804a`，1/1 通过，覆盖新增 Highlight 选择/清除断言。
- `AutoEraStartupAssetsEditModeTests`：Unity job `cfcad580`，2/2 通过；菜单配色 Prefab 最小修正未使入口资产回归。
- 结束状态：Console Error=0、Warning=0，Unity 2022.3.62f3c1 非 PlayMode、非编译。
- Physics 设置边界：升级前未采集有效的新增默认字段，故**未验证等价**；不能由当前新增字段或旧层矩阵（仅层8行列差异已核对）推断没有行为差异。本项留作后续具备升级前基线时的独立核验，不阻塞本次 Runtime Settings 测试结论。

## 入口与基础上下文回归（2026-09-10）

- `AutoEraStartupAssetsEditModeTests`：Unity job `29b052c0`，2/2 通过，覆盖菜单正式绑定/6000生成路径及 LanguagesTable 末尾空列解析。
- `InitialRegionSceneEditModeTests`：Unity job `5858421c`，1/1 通过，覆盖新增 fake 输入 UI 阻挡断言。
- `AutoEraApplicationContextEditModeTests`：Unity job `875eb4f3`，2/2 通过。
- `AutoEraProcedureContextSlotEditModeTests`：Unity job `cfa6ab2b`，3/3 通过。
- 结束状态：Console Error=0、Warning=0，Unity 2022.3.62f3c1 非 PlayMode、非编译。

客户端已进行一次 Launch→MainMenu→InitialRegion→FieldHud 首次手动流程，但未交付完整 G1 或重复进出证据；本节也不替代该待测范围。

## Entity 入口与两轮流程回归（2026-09-10）

- `AutoEraStartupFlowEditModeTests`：Unity job `2327354d`，1/1 通过；该 UnityTest 会进入并退出 PlayMode，覆盖两轮菜单→世界8个 Available 实体/HUD→菜单实体释放。
- `InitialRegionSceneEditModeTests`：Unity job `67c63c9a`，1/1 通过。
- `AutoEraStartupAssetsEditModeTests`：Unity job `0e1551b0`，2/2 通过。
- 结束时 Unity 已退出 PlayMode、非编译。Console 的全局统计为 Error=2、Warning=0，但 `console_get_logs(filter=Error)` 返回0条可捕获 Error 日志；因此该轮不能把全局 Console 记为 Error=0，须由客户端核对/清理既存历史错误后再建立新鲜门禁。没有测试失败，故无失败栈可回传。

### 回归作废说明

客户端随后取得两条精确编译日志：`AutoEraStartupFlowEditModeTests.cs:47` 的 `CS0117`（`GF.Procedure`）和 `CS0012`（测试程序集缺少 Builtin.Runtime 引用）。该问题不是环境音频错误。客户端已改为通过已引用的 `GameEntry.GetComponent<ProcedureComponent>()` 访问，且不扩大 asmdef 引用。故 job `2327354d` 仅保留为修复前历史运行记录，**不得作为本次修改后的 Startup Flow 验收**；待普通刷新完成、Console建立新鲜 capture、客户端交接8090后重跑。

### 新程序集回归失败（待客户端修复）

- 新 job `f2bb3b07`：`AutoEraStartupFlowEditModeTests` 0/1；失败方法为 `Launch_Menu_World_Menu_World_ReleasesEntities`。该 job 是修正程序集后的有效回归，不能以旧 job 覆盖。
- 本次运行前已启动新鲜 Console capture；运行结束 global/capture 均为 Error=0、Warning=0、Exception=0、Assert=0，Unity 已退出 PlayMode、非编译。因此失败不是 Console Error。
- UnitySkills 的 `test_get_result` 和 `Library/UnitySkills/batch_state.json` 仅保留失败测试名与计数，未持久化 NUnit assertion stack；capture 中亦没有错误日志。QA 未伪造堆栈，已将测试断言顺序和此限制回退客户端，以原生 Test Runner 或补充诊断日志定位。
- 客户端随后从 `C:/Users/WIN10/AppData/LocalLow/ZZNC/星际拓荒：自动纪元/TestResults.xml` 取得精确失败栈：失败在测试 line 20 的活动场景断言，预期 `Assets/Game/Scene/Launch.unity`、实际为空路径。原因为原生 TestRunner 的空隔离场景，产品断言尚未执行。测试已改为隔离场景时 Additive 显式打开已保存 Launch 并设为 active，不保存其他场景；待刷新后重新交接。后续失败证据优先读取该 XML 的 `failure/message/stack-trace`。
- 修复版 job `9bcb76a0` 仍失败；PlayMode 域重载后 UnitySkills 的直接 job 查询为空，故以 XML 为权威。失败信息为 `Expected: not null / But was: null`，栈定位 `AutoEraStartupFlowEditModeTests.cs:49`：菜单点击后的30秒等待结束时 `InitialRegionScene` 未出现。此前活动 Launch 隔离问题已跨过。XML output 另含“2 audio listeners”警告，但新鲜 Console Error=0，且它不在失败栈中。该问题已退回客户端检查 MainMenu→WorldProcedure→InitialRegion 加载链。
- MainMenu 初始 loading/禁用、Procedure 真正 ready 后启用，以及 InitialRegion 额外 AudioListener 移除后，新 job `3093292a` 已执行。PlayMode 域重载后 UnitySkills 未保留该 job 的直接查询，但权威 `TestResults.xml`（2026-09-10 07:52:01Z～07:52:03Z）记录 `AutoEraStartupFlowEditModeTests.Launch_Menu_World_Menu_World_ReleasesEntities` 为 `Passed`，1/1，duration `6.592468s`。XML output 不再含双 AudioListener 警告；结束 Console global Error=0、Warning=0，Unity 非 PlayMode、非编译。

## 审计适配复核（2026-09-10）

- `python -m unittest tools.tests.test_audit_framework_purity`：14/14 通过。
- `python tools/audit_framework_purity.py --product-profile tools/audit_product_profile.json`：通过。
- `python tools/audit_project_boundaries.py`：通过。
- `python tools/audit_framework_purity.py --strict-framework`：按派发单预期保留9项失败，均为严格框架规则对已授权 MainMenu/InitialRegion 构建场景与受限 MainMenu 标识的拒绝。产品模式未把该9项当作自动通过，也不构成 Startup Flow 失败的根因。

## 字段访问与公开投影回归（2026-09-10）

- `RegionFieldAccessEditModeTests`：Unity job `6d9e9c69`，2/2 通过。
- `RegionHudPresenterEditModeTests`：Unity job `4ad31ba8`，1/1 通过。
- `InitialRegionEditModeTests`：Unity job `56769587`，5/5 通过，包含新增优先级/双通道公开投影。
- `InitialRegionSceneEditModeTests`：Unity job `818280b2`，1/1 通过。
- 结束状态：Console Error=0、Warning=0，Unity 2022.3.62f3c1 非 PlayMode、非编译。

## 启动失败恢复与代理／预览回归（2026-09-10）

- `AutoEraStartupFlowEditModeTests`：Unity job `f7fa00a6`。该用例进入 PlayMode 导致 UnitySkills 在域重载后未保留直接 job 查询，故以 `C:/Users/WIN10/AppData/LocalLow/ZZNC/星际拓荒：自动纪元/TestResults.xml` 为权威记录：2/2 通过。`Launch_Menu_World_Menu_World_ReleasesEntities` 用时 `6.543715s`；`MissingWorldScene_ReturnsToExistingMenu_AndRetryRecovers` 用时 `10.308966s`。
- 后者实际覆盖缺失世界场景失败后返回既有菜单、可再次提交，并在恢复后得到 8 个实体；它不替代独立的晚回调拒绝断言。
- 预期的 `[AutoEra][World] Enter failed` Warning 未出现在该 XML output，也未在域重载后的 UnitySkills Console 捕获中保留。功能性断言通过，但**不能据此声称该 Warning 已被留痕**；如将该日志作为验收要求，仍需可重复的专门证据。
- `InitialRegionSceneEditModeTests`：Unity job `76e19d05`，1/1 通过；`RegionPlacementPreviewEditModeTests`：Unity job `61d0b904`，1/1 通过。
- 结束状态：Unity 非 PlayMode、非编译；Console Error=0、Warning=0；8090 已释放。

## 启动取消与重复提交补验（2026-09-10）

- `AutoEraStartupFlowEditModeTests`：Unity job `734b976b` 进入 PlayMode 后直接结果不可稳定回查，以下以 `TestResults.xml` 为权威：总计 3，2 通过、1 失败。
- `Launch_Menu_World_Menu_World_ReleasesEntities` 通过，用时 `10.493498s`；该版测试包含同帧两次 `Invoke`，故重复提交路径未使既有两轮流程失败，但未单独作为取消回调的通过依据。
- `MissingWorldScene_ReturnsToExistingMenu_AndRetryRecovers` 通过，用时 `6.475852s`。
- `CancelledSceneLoad_CompletesWithoutCallingOldOwner_ThenFreshLoadSucceeds` 失败，用时 `6.369237s`。在试图读取 `AutoEra.Scene.World` 前发生 `NullReferenceException`，栈定位 `AutoEraStartupFlowEditModeTests.cs:31`；`GameEntry.GetComponent<ConfigComponent>()` 为 null。测试尚未到达实际 `GF.Scene.Load`／`Cancel` 或旧、新回调计数断言，故取消与晚回调拒绝**仍未验收**，已退回客户端修正测试前提后重跑。
- 结束状态：Unity 非 PlayMode、非编译；Console Error=0、Warning=0；8090 已释放。

### 取消夹具 v2 复跑

- Unity job `3327f117` 同样因 PlayMode 域重载以 `TestResults.xml` 为权威：总计 3，2 通过、1 失败。既有两轮流程通过（`11.259845s`），缺失场景重试通过（`6.657341s`）。
- v2 已由 `FindObjectOfType` 取得 live `ConfigComponent` 与 `SceneComponent`，且两个 NotNull 断言均已跨过；但 `config.GetString("AutoEra.Scene.World")` 内部仍引发 `NullReferenceException`，栈定位第 `33` 行。故问题由组件查找转为该时点的 Config 内部数据／读取前提，仍未进入 `Scene.Load`／`Cancel` 或回调断言。
- 该用例继续保持失败，等待以明确的测试输入或已确认配置来源重建夹具；结束 Unity 非 PlayMode、非编译，Console Error=0、Warning=0，8090 已释放。

### 取消夹具 v3 复跑与程序集可追溯性缺口

- Unity job `a8ee103b`（XML 权威）仍为总计 3，2 通过、1 失败；两轮流程通过（`10.549536s`），缺失场景重试通过（`6.619332s`）。
- 失败继续标为 `AutoEraStartupFlowEditModeTests.cs:33` 的 `NullReferenceException`，但运行前读取的当前源码第 33 行已是 `int oldCallbacks = 0, freshCallbacks = 0;`，不含可空解引用；v3 亦已移除 `Config.GetString`。UnityTest 迭代器的源码行映射或域重载后的程序集／符号可追溯性不足，不能将该行号作为产品根因。
- 本轮停止盲目重跑。待客户端加入可识别的测试执行版本及阶段性夹具断言、确认刷新后的程序集实际生效后，才重新执行取消／旧回调验证。结束 Unity 非 PlayMode、非编译，Console Error=0、Warning=0；8090 已释放。

### 取消夹具 v4 有效回归

- Unity job `adb08994` 进入 PlayMode，直接结果受域重载影响，故以 `TestResults.xml` 为权威：`AutoEraStartupFlowEditModeTests` 总计 3，3 通过、0 失败、0 跳过、0 不确定。
- `CancelledSceneLoad_CompletesWithoutCallingOldOwner_ThenFreshLoadSucceeds` 通过，用时 `6.744335s`。XML output 记录 v4 已在域重载后 helper 中运行，真实 GF load 返回后核验到：旧回调 `0`、新回调 `1`、受管场景已释放。该证据覆盖取消后的旧拥有者不被激活、后续新加载可成功及释放完成。
- `Launch_Menu_World_Menu_World_ReleasesEntities` 通过，用时 `10.474363s`；本版包含同帧两次 Invoke 的单次提交流程断言。`MissingWorldScene_ReturnsToExistingMenu_AndRetryRecovers` 通过，用时 `6.641913s`。
- 结束状态：Unity 非 PlayMode、非编译；Console Error=0、Warning=0；8090 已释放。缺失场景 Warning 不在本轮验收要求内，未声称留痕。

## 基础时间与 Hub 连续推进回归（2026-09-10）

- `WorldClockEditModeTests`：Unity job `4430540f`，5/5 通过。
- `WorldDayNightRulesEditModeTests`：Unity job `d305de9a`，2/2 通过。
- `AutoEraRuntimeSettingsEditModeTests`：Unity job `f94ba799`，3/3 通过。
- `AutoEraStartupFlowEditModeTests`：Unity job `e7660b01` 进入 PlayMode 后以 `TestResults.xml` 为权威，3/3 通过：取消真实 GF 加载后旧回调 `0`、新回调 `1`、受管场景释放（`7.740474s`）；两轮进入／返回／实体释放（`11.511333s`）；缺失场景失败后重试恢复（`6.844022s`）。
- 本轮计数合计为 13/13；结束 Unity 非 PlayMode、非编译，Console Error=0、Warning=0；8090 已释放。

## Watcher 事务与启动恢复回归（2026-09-10）

- `DataTableGenerationProfileEditModeTests`：Unity job `65da2dc9`，30/30 通过，包含 watcher 精确版本／空列表、分组枚举语义、事务冲突与恢复相关回归。
- `AutoEraStartupFlowEditModeTests`：Unity job `3d2de294` 进入 PlayMode 后以 `TestResults.xml` 为权威，5/5 通过。除既有取消、两轮进入／返回与缺失场景重试外，新增 `FrameworkRestartDuringSceneLoad_ReleasesOldSessionAndAllowsFreshEntry` 通过（`8.284579s`），XML output 记录真实框架 Restart 后旧 context/session 已释放、fresh entry 得到八个实体；`HalfInitializedRegionFailure_ReleasesRegistryAndSubscription` 通过（`6.801066s`）。
- 结束状态：Unity 非 PlayMode、非编译；Console Error=0、Warning=0；8090 已释放。

## Foundation 最终回归与声音负例分层（2026-09-10）

- 普通 EditMode 八组：`DataTableGenerationProfileEditModeTests` job `2c020366` 31/31；`AutoEraRuntimeSettingsEditModeTests` `d28bf960` 4/4；`PersistentIdEditModeTests` `38506855` 4/4；`PersistentObjectRegistryEditModeTests` `878a3f11` 3/3；`PersistentObjectReferenceEditModeTests` `904ad607` 3/3；`UtcTimeEditModeTests` `845d8c96` 3/3；`AutoEraWorldSessionEditModeTests` `aaed9584` 2/2；`AutoEraApplicationContextEditModeTests` `c911a531` 2/2。合计 52/52。
- `AutoEraStartupFlowEditModeTests` job `53004793` 进入 PlayMode 后以 `TestResults.xml` 为权威，6/6 通过。包含取消加载、加载中 Restart、半初始化失败释放、两轮进入／退出、缺失世界重试，以及 `MissingSound_ReportsOwnedFailureWithoutBlockingOtherGroups`。
- 声音负例单独证据：该 XML 用例输出精确记录 `Assets/Game/Audio/B10MissingSoundForValidation.wav` 在 `Sound` 组触发一次预期的 `NotExist` 失败诊断，且其他分组继续可用。它是受控负例，不纳入普通干净运行结果。
- 恢复后的普通门禁：结束时 Unity 非 PlayMode、非编译；Console Error=0、Warning=0；8090 已释放。

## 文本数据表类型校验负例分层（2026-09-10）

- `DataTableGenerationProfileEditModeTests`：Unity job `8d439b14`，32/32 通过。新增文本模式负例通过现有二进制解析器验证临时输出，未发布 bytes 或正式数据。
- 受控负例证据保留两条既有解析器 Error：`MessageKey` 的 `int` 类型收到 `AutoEra.Startup.MenuReady`，随后同一临时 `StartupMessages.bytes` 记录 Parse data table failure／`GenerateDataFile` 内部 `NullReferenceException`。当前入口以失败返回拒绝正式写入；第二条诊断是旧解析器的已捕获诊断质量残留，**未声称已修复**。
- 负例结束后执行 AssetDatabase 刷新；历史 Console 仍保留上述两条证据。随后新建独立 Console capture，恢复期统计为 Error=0、Warning=0、Exception=0、Assert=0，Unity 非 PlayMode、非编译。该 capture 仅证明恢复期无新增异常，不删除或忽略负例记录。
