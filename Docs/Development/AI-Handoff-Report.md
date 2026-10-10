# 星际拓荒：自动纪元 —— AI 开发交接报告

> 用途：交给接手的 AI（任何平台），让它在一轮内快速理解项目现状、已完成工作、未提交改动、已知问题与下一步。
> 原报告时间：2026-09；2026-10-09增量更新。下文历史段保留原来源，不代表最新完成状态。

---

## 0. 一句话定位

**B40～B46七批增量已完成；算法真实驱动、硬件、资源结算、林矿生产运输和能源优化已有原生证据。B47/B48/B49/B50仅独立核心或子集完成，正式开局、完整经济/成长和全部离线领域仍缺，不能用GM试运行替代正常玩家链。**

最新功能/验收边界见[2026-10-09功能证据索引](FunctionalEvidenceIndex-20261009.md)和[总计划](ImprovementPlan-20261008.md)。本轮51项相关UI回归、健康5/5通过；仓储门1六项既有问题及全量Player性能门尚未通过。

### 2026-09历史定位（保留）

**Unity 2022.3 的「自动纪元」经营/自动化游戏，产品代码已搭好核心领域（机器/组件/算法/警报/能源/输入/区域部署），当前正处于「G3-001 算法配置闭环」验收前置阶段；经济域（购买机器/组件）尚未接入，正式 UI 无法从零造机器，需靠刚做的 GM 面板注入机器来打通验收链路。**

---

## 1. 项目与环境

| 项 | 值 |
|---|---|
| 项目名 | 星际拓荒：自动纪元（Stellar Frontier: Auto Era） |
| 工作目录 | `D:\unity\UnityProject\stellar-frontier-auto-era` |
| Unity | 2022.3.62f3c1 |
| 框架 | GameFramework（GF_X）+ HybridCLR 热更 |
| 产品代码 | `Assets/Game/Scripts/AutoEra/`，命名空间 `AutoEra.*` |
| 框架核心 | `Assets/Game/ScriptsBuiltin/`（「框架层禁改」红线已解除，可按项目需要修改但需留痕） |
| Git 状态 | 2026-09记录为main/A–F未提交；当前以只读git status为准，本轮未暂存/提交 |
| 设计文档 | `Docs/GameDesign/`（正式设计来源）、`Docs/Development/`（开发/派发/验收） |
| 任务表 | `Docs/GameDesign/05-开发计划/第一版开发任务表.xlsx`（禁止手改） |

**代码边界铁律**：产品业务代码只能进 `Assets/Game/Scripts/AutoEra/` 且用 `AutoEra.*` 命名空间；框架纯度审计（`python tools/audit_framework_purity.py`）是自动回归围栏。

---

## 2. 开发工具链

### 2.1 UnitySkills REST 自动化
- **主项目端口会漂移**：当前 `8093`（曾为 8092），`instanceId` 恒为 `_1D033F4D`。
- 定位法：`Get-NetTCPConnection` 列 8080–8100 端口 → 逐个 `GET /health` 看 `instanceId == _1D033F4D`。**别写死 8092**。
- 另两个实例：`8090` = GameDesinger_C4724723、`8091` = GameDesinger/ArtResource（ArtResource_8CDFD6CE）。
- 常用：`asset_refresh`（触发编译，之后轮询 `/health` 的 `compilation.isCompiling` 直到 false）、`debug_get_errors`、`gameobject_find`、`component_list`、`prefab_*`、`test_run_by_name`。
- `asset_refresh` 在 Play 模式下会触发域重载、**退出 Play 并可能卡在弹窗/联网超时**（日志反复 `Curl error 28` 连 cdp.cloud.unity3d.com）。

### 2.2 工具箱面板（Editor）
- 接口 `IToolHubPanel`（`OnEnable/OnDisable/OnGUI/OnDestroy/GetHelpText`）+ 特性 `[ToolHubItem(menuName, description, priority)]`，位于 `Assets/Game/ScriptsBuiltin/Editor/MigratedToolbox/`。
- 面板放 `Assets/Game/Scripts/AutoEra/Editor/`（独立程序集 `AutoEra.Editor.asmdef`，Editor-only，引用 Builtin.Editor + Hotfix），**不需要 `#if UNITY_EDITOR`**。
- 参考实现：`UiPanelTestPanel.cs`（`[ToolHubItem("测试/UI面板测试管理工具",...,40)]`）。

### 2.3 测试
- 测试运行器：`test_run_by_name`（`{testName, testMode:"EditMode"}`）→ jobId → `test_get_result`。
- 命名空间：`AutoEra.Tests.Editor.*`（EditMode）、`AutoEra.Tests.PlayMode.*`（PlayMode）。
- **脏场景会阻塞 EditMode 测试**——先 `scene_save`。
- **已知基础设施降级**：累计跑 ~80 次后 EditMode 测试报 `Unity Test Runner did not leave 'starting' within 90 seconds`，编辑器日志循环 `IPrebuildSetup...TestRunBuilder` + `Curl error 28`。属测试基础设施问题（非代码），恢复 = 重启编辑器。

### 2.4 Git
- **只在用户明确触发时提交**，提交信息必须中文。
- **超大 blob 陷阱**：`Assets/Game/Fonts/UI/SIMHEI SDF.asset` 是 TMP 动态图集，重导入后 ~128MB，超 GitHub 100MB 限制（GH001）。当前 `M` 状态，**不要提交**；根治待用户选（git-lfs / .gitignore / Unity 限制图集尺寸）。

---

## 3. 核心领域模型速查（接手必读）

### 3.1 机器域
- `MachineRoster`（`AutoEra.Machines`）= 世界级机器/组件花名册。API：`Create(MachineDefinition)`、`CreateComponent(ComponentDefinition)`、`Install(machineId, origin, componentId, index)`、`Deploy(id)`、`RecoverToLibrary(id, origin)`、`TryGet(id, out)`。
- `MachineInstance`：`Activate(ManagementOrigin)`、`SetRunState(origin, state)`、`SetPowerSwitch(origin, on)`、`UpdateEnvironment(supply, signal)`、`UpdateIntegrity(value)`。状态派生：`Powered`/`Connected`/`CanRun`。
- `ManagementOrigin`：`Field/Hub/Library`。**未部署机器只能用 Library 装组件；已部署只能用 Field 操作**。
- `MachineCatalog.FromLoadedGameData()` 读数据表；`TryGetMachine(rowId, out MachineDefinition)`、`TryGetComponent(rowId, out ComponentDefinition)`。
- 关键数据行 ID：`10011`=通用轮式载体（可移动，占地 1.8×2.6）、`10021`=固定底座、`20011`=计算核心（算力 50、逻辑 40）。

### 3.2 区域域
- `InitialRegion`（`AutoEra.World.Region`）：`Register(kind,name,pos,size,yaw,blocksNavigation)`、`Select(id,inputBlocked)`、`TryGet(id,out)`、`CanPlace(...)`、`SelectedId`、事件 `SelectionChanged`/`ObjectsChanged`。
- `DeployMachine(id, position, size, out model, yaw, blocksNavigation)` → `RegionMachineDeploymentResult`（`Bound/AlreadyBound/InvalidPlacement/...`）。
- `InitialRegionScene`（场景入口 MonoBehaviour）：公开 `Region`、`MachineRuntimes`（RegionMachineRuntimeRegistry）、`Energy`、`Alerts`、`Session`（本会话新加的公开访问器）、`WorldMilliseconds`、`TrySpawnMachine(id, out reason)`、`TrySetDevelopmentTimeMultiplier`。
- `RegionObject`：`Kind`（`PersistentObjectKind`: `None/Machine/Building/ResourcePoint/Task/Behavior`）、`Name`、`Size`、`Machine`。

### 3.3 部署链路（权威，来自 PlayMode 测试）
```
roster.Create(机器) → roster.CreateComponent(核心) → roster.Install(机器.Id, Library, 核心.Id, 0)
→ region.DeployMachine(机器.Id, 位置, 占地, out _, 0, true)
→ scene.TrySpawnMachine(机器.Id, out reason)   // 异步建实体+运行时
→ machine.Activate(Field) → machine.SetRunState(Field, Running) → machine.UpdateEnvironment(true,true)
→ region.Select(机器.Id, false)
```

### 3.4 算法域（G3-001 相关，已接通）
- 算法工作台 `AlgorithmEditorForm`：节点库（20 种）、建节点（网格落位）、两步连线（点输出端口→点输入端口）、删除选中（级联断开）、应用草稿、机器运行。
- `RegionMachineRuntimeRegistry` 一台机器建一个 `AlgorithmMachineAdapter` + `AlgorithmInstanceService`。
- **单实例限制**：`AlgorithmMachineAdapter` 是单运行时设计，草稿激活只支持单实例（第二个返回 false）。

### 3.5 UI 域
- 组合根 `AutoEraUiRuntime`：意图路由 + `BlocksWorldInput`（静态聚合所有打开表单的 `BlocksWorldInput`）。
- `AutoEraUiNavigator.Open(source, view[, pageRequest])` → `AutoEraUiSession.ForWorld(...)` 把世界会话写入打开参数 → 各 Form 读模型。
- ReadModel 模式：`MachineReadModels.Create(session)`、`RegionReadModels.Create(session)`，读模型订阅领域事件、发布 `Changed`。

---

## 4. 已完成工作（批次 A–F 程序任务）

已验收关闭的任务：**P1-005、P2-010、P3-010、P3-011、P6-011**。

按领域归纳已完成的核心能力：

- **机器域**：花名册/身份/序列号、组件装载/拆卸、容量与算力、部署流程 `MachineDeploymentFlow`。
- **算法域**：算法工作台全链路（节点库/建图/连线/删除/草稿编译/应用/激活）、系统模板（基础灌溉/资源开采/采集/农田作业/固定运输，逻辑成本 DEC-202 定稿）。
- **警报域**：`AutoEraAlertService`（Raise/Resolve/MarkRead/Clear）、`AlertForm`、警报账本。
- **能源域**：区域电网 `EnergyGrid`、设施（发电机/光伏/蓄电池）、机器负载侧。
- **输入域**：`RegionInputModule`（绑定驱动的 PC 输入、点选、聚焦、落位预览）、`RegionInputBindings`。
- **PCG**：运行时程序化生成（矿脉矿石 `OreDepositVisual`、岩石/植株 `RuntimeScatterer`、地表采样抽象）。
- **事件域**：事件账本基础（P0-007）。
- **UI 测试工具**：`UiPanelTestPanel` + `[UiPanelTestSetup]` 注入内存世界会话跑真实界面。
- **美术**：B08（11 项高模→低模烘焙）、B10（12 项静态资产）、ART-006（UI 视觉合同与 Prefab）等已收口（详见 task-queue 与 openspec archive）。

---

## 5. 本会话最新改动（全部未提交，接手需先了解）

| 文件 | 改动 | 目的 |
|---|---|---|
| `Assets/Game/Scripts/AutoEra/Editor/GM/GmPanel.cs`（新增） | GM 管理面板，`[ToolHubItem("调试工具/GM管理面板",...,50)]` | 一键部署带核心机器、机器状态修改、警报制造/消除、时间倍速；每步写 `[GM]` 日志 |
| `InitialRegionScene.cs` | 加 `public AutoEraWorldSession Session => _session` | 让 GM 面板拿到花名册/区域 |
| `InitialRegionMachineEntity.cs` | 实体 OnInit 启用碰撞体 + 切 RegionSelection 层(8) + Bind 时按占地对齐尺寸 | 修复「机器部署成功但点不中」 |
| `FieldHudForm.cs` | ①`BindRegion` 订阅 `SelectionChanged` ②选中即开现场页（`OnRegionSelectionChanged`→`SyncSelectedMachine`+`OpenFieldPageForSelection`）③`BlocksWorldInput` 改 `_managementOpen` | ①修复「选中机器不出现详情页」②修复输入死锁 |

### 5.1 GM 面板要点
- 路径：工具箱 → 调试工具 → GM管理面板。
- 一键部署：创建 10011 轮式载体 + 20011 核心 → 装入 → 部署（撞种子对象会自动环形找空地回退）→ 生成实体/运行时 → 激活 → 选中。
- **为什么需要它**：经济域（购买机器/组件）未接入，正式 UI 没有「创建机器」入口，所以 G3-001 验收的前置机器必须靠 GM 面板注入。

### 5.2 机器点选修复（关键）
- 根因：机器实体预制体的碰撞体在 Default 层(0) 且**默认禁用**（`m_Enabled:0`），现场点选只对 RegionSelection 层(8) 射线检测。
- 修复：`InitialRegionMachineEntity.OnInit` 用 `LayerMask.NameToLayer("RegionSelection")` 启用碰撞体 + 切层，`Bind` 时 `box.size = new Vector3(body.Size.x, 2f, body.Size.y)` 对齐占地。

### 5.3 现场页自动打开 + 输入死锁修复（关键）
- 现场页下标（实测 prefab `_pageRoots`）：**0–4 = HUD 常驻模块**（HudStatus/Tracker/Alerts/Navigation/Save），**5=机器概况、6=机器硬件、7=机器算法、8=机器诊断、9–12=农田/人工林/地表矿脉/水域、13=传感记录、14=建筑总览、15–21=各建筑页**。
- 选中即开页：机器→5、资源点→按名映射 9–12、建筑→14、空白→关闭。
- **输入死锁坑（已修复）**：`BlocksWorldInput` 曾写 `_managementOpen || _openFieldPage>=0`，而 `_managementOpen` 又从 `AutoEraUiRuntime.BlocksWorldInput` 聚合回来，形成正反馈——现场页一开就挡输入、挡输入又 `SetFieldAccess(true)` 关闭现场页并把 `_managementOpen` 卡 true，导致相机(WASD)和点选全废。改为 `BlocksWorldInput => _managementOpen`（只有全屏管理页才挡世界输入）。

---

## 6. 当前已知问题（用户反馈 + 待解决）

### 6.1 用户明确反馈的问题
1. **「问题还是不少」**（用户原话，未逐一列举）——换平台后需接手 AI 主动盘点。
2. 已修复的：机器点不中、选中后无详情页、WASD/点选死锁（见第 5 节）。
3. 视觉引擎读图失败（本会话 modlens 视觉桥 failed）——用户侧需 `npx @liustack/modlens doctor` 排查。

### 6.2 领域未接通（已知缺口）
- **经济域未接入**：机器库无「创建机器」、组件库购买依赖经济域（注释明写「经济域尚未接入」）。→ 从零造机器只能靠 GM 面板。
- 机器详情页部分栏位（硬件装配/算法实例/诊断）为「陈述原因」空态，依赖后续域接入。
- 算法域生产运行路径（传感器采样/效应器提交）部分待接。

### 6.3 基础设施
- 测试运行器降级（见 2.3）。
- SIMHEI SDF.asset 128MB（见 2.4）。

---

## 7. 关键路径与剩余工作

### 7.1 任务表状态（程序侧）
- **已完成**：P1-005、P2-010、P3-010、P3-011、P6-011。
- **进行中（剩余全为美术/内容，非程序）**：
  - P2-012（VFX 待美术）
  - P6-012（相关详情跨界面跳转，待后续）
  - P8-007（音频资源，待 P8-008/P8-009）

### 7.2 最高杠杆：G3-001 算法配置闭环验收
- **G3-001 是当前最关键的未验收项**：P4-001（资源缓存与预留）显式依赖它，而 P4-001 又是 P4-B/C/D/E/F/G（生产/物流）的前提，P5（经济/能源）、P6（建造/任务/成长）也受其门控。
- 通过 G3-001 后，剩余纯程序工作（P4-A、P5-A、P6-A）可继续自主推进。

### 7.3 G3-001 验收完整流程
1. Play → 进入世界 → 等 InitialRegion 就绪。
2. 工具箱 → GM管理面板 → 设落位坐标 → 「一键部署带核心的机器」（创建+装入+部署+生成运行时+激活+选中）。
3. 等 1–2 秒实体/运行时异步建立。
4. 点机器 → 现场侧栏自动弹出「机器概况」→「算法编辑」按钮 → `AlgorithmEditorForm`。
5. 验证闭环：节点库(20 种)建节点 → 两步连线 → 删除选中 → 应用草稿 → 机器按图运行（Startup 触发 → 常量/变量 → 日志/执行器）。
6. 判据：这条「建→连→删→应用→运行」闭环在真实游戏里走通即通过。

---

## 8. 关键坑与陷阱（必读清单）

1. **FSR（Fast Script Reload）不能热重载结构性变更**：新增 `const` 字段/新方法时，FSR 会把 const 字段注释掉并在 Editor.log 报 `error CS0026: Keyword 'this' is not valid in a static ...`（来自 `SourceCodeCombined.cs`）+ 可能让 `/health` 的 `isCompiling` 长时间卡 True。**这只是 FSR 假错误，非真实编译错误**——判据：grep Editor.log 里 `error CS` 且非 SourceCodeCombined/FSR 来源。处置：等 FSR 放弃或重启 Unity 走普通全量编译。结构性改动不要在 Play 模式靠 FSR 验证。
2. **端口漂移**：8092→8093，见 2.1。
3. **SIMHEI SDF.asset 128MB**：见 2.4。
4. **测试运行器降级**：见 2.3。
5. **经济域未接入**：正式 UI 无法造机器，验收靠 GM 面板。
6. **机器点选层**：碰撞体必须启用 + RegionSelection 层(8)，否则点不中。
7. **现场页下标**：`_pageRoots` 前 5 个是 HUD 常驻模块，现场页从下标 5 起。
8. **`ManagementOrigin` 约束**：未部署机器用 Library、已部署用 Field。
9. **场景脏会阻塞 EditMode 测试**：先 `scene_save`。
10. **PowerShell 5 语法**：无 `??`、双引号内变量用 `${var}:`、`Object` 在双 using 下歧义用 `UnityEngine.Object.DestroyImmediate`。

---

## 9. 建议接手后的第一批动作

1. 读 `Docs/Development/ProjectBaseline.md`（正式设计来源与权限边界）。
2. 读 `Docs/Development/Dispatch/README.md` + `TaskQueue.md`（多窗口派发协议，若沿用）。
3. 用 `dtodo`/`memory` 工具读取项目记忆（`target=project`、`target=key`）。
4. 先跑一次 `asset_refresh` + `debug_get_errors` 确认编译干净（0 错误）。
5. 用 GM 面板走一遍 G3-001 验收流程（第 7.3 节）。
6. 盘点「用户说的问题还是不少」——逐项记录、复现、分类（程序/美术/内容/基础设施）。
7. 决定 Git 提交策略（当前 A–F 改动未提交；提交需用户明确触发 + 中文信息；SIMHEI blob 需先处理）。

---

## 10. 附：关键文件索引

| 文件 | 作用 |
|---|---|
| `Assets/Game/Scripts/AutoEra/Editor/GM/GmPanel.cs` | GM 管理面板（本会话新增） |
| `Assets/Game/Scripts/AutoEra/World/Region/InitialRegionScene.cs` | 区域场景入口 + Session 访问器 |
| `Assets/Game/Scripts/AutoEra/World/Region/InitialRegion.cs` | 区域对象/选中/部署 |
| `Assets/Game/Scripts/AutoEra/World/Region/InitialRegionMachineEntity.cs` | 机器实体（碰撞体修复） |
| `Assets/Game/Scripts/AutoEra/Machines/MachineRoster.cs` | 机器花名册 |
| `Assets/Game/Scripts/AutoEra/UI/FieldHudForm.cs` | 现场 HUD（现场页自动打开 + BlocksWorldInput） |
| `Assets/Game/Scripts/AutoEra/UI/AlgorithmEditorForm.cs` | 算法工作台（G3-001 核心） |
| `Assets/Game/Scripts/AutoEra/Input/RegionInputModule.cs` | 现场输入/点选/相机 |
| `Assets/Game/Tests/AutoEra/PlayMode/MachineDeploymentDataFlowPlayModeTests.cs` | 部署流程权威参考 |
| `Docs/Development/ProjectBaseline.md` | 项目基线/权限 |
| `Docs/GameDesign/05-开发计划/第一版开发任务表.xlsx` | 任务表（只读） |
