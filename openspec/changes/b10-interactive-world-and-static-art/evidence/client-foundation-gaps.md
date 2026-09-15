# B10 基础剩余项只读对账

2026-09-10；任务 `b10-client-foundation-gap-reconciliation`。
本包只核对、只写此文件；未执行Unity、测试、资源修改、Git或xlsx写入。
快速执行候选检查：需要区分已有实现、历史证据与任务DoD，并判断授权边界，属于专业复核，客户端直接处理。

## 输入与判定方法

- 任务表 `Docs/GameDesign/05-开发计划/第一版开发任务表.xlsx`：以只读ZIP/XML读取sheet2第4–14行；P0-007不在本包。
- 本次读取SHA-256：`DFBE954F3907E965B25EEE5C769413C6B5176483D1982C582FD318E57068EEB5`。旧实施哈希不同不能认定AI修改：任务表由用户持续维护，不能要求恢复旧哈希。
- 对照b02 tasks、segment-09/10/15/19/22，B10 client-baseline、client-coverage、qa-g0-g1-matrix与当前代码/config。
- 以下代码路径在 `Assets/Game/Scripts/AutoEra/` 下；测试类对应 `Assets/Game/Tests/AutoEra/Editor/<类名>.cs`。框架路径另行明确。
- “已存在”不等于所有DoD通过；旧未勾选不等于未实现。没有本次运行新测试，不复用失败程序集或只凭测试数量判断覆盖。

## 逐项结论

| 任务及表中目标 | 实际实现和可定位证据 | 剩余／授权边界 |
| --- | --- | --- |
| P0-003 启动Procedure与业务场景 | `Application/AutoEraApplicationContext.cs`、`AutoEraSceneFlow.cs`、`Procedures/AutoEraStartupProcedure.cs`、`AutoEraMainMenuProcedure.cs`、`AutoEraWorldProcedure.cs`；AppConfigs登记三Procedure；Launch/MainMenu/InitialRegion启用。`AutoEraStartupFlowEditModeTests` adb08994 XML：两轮8实体释放、双点击单次提交、缺失场景恢复、真实GF取消旧0/新1与释放。b02 3.2/6.1/6.3/6.4已有证据，6.2不能视为尚无实现。 | b02 6.5仍缺加载中框架重启、半初始化会话全部异常组合与订阅释放完整断言；只补现有SceneFlow/Procedure，不改通用ChangeSceneProcedure。可在原b02范围安排精确补验；不可因此重新写启动系统。 |
| P0-004 永久ID与注册表 | `World/Identity/PersistentId.cs`、`PersistentIdAllocator.cs`、`PersistentObjectRegistry.cs`、`PersistentObjectReference.cs`。`PersistentIdEditModeTests`验证高水位/乱序恢复/最大值耗尽；Registry测试拒重复/类别与实例错误；Reference测试Missing、同ID重载、跨世界不继承。b02 4.1–4.4及segment-11–13。B10两次8实体生命周期复用同一世界注册表。 | 基础服务不是缺项；正式存档DTO、任务/行为实际实例接入属于各自后续系统，不因这些对象未实现重建ID系统。重新全量回归只在后续集成门禁需要时执行。 |
| P0-005 确定性世界时间 | `World/Time/WorldClock.cs`含整数毫秒、double余量、直接推进/倒退/溢出拒绝；`WorldDayNightRules.cs`与`WorldEventSortKey.cs`含昼夜和稳定排序；UTC Provider/TimeUtil已存在。`WorldClockEditModeTests`的60帧/1秒、余量、负值与临界；DayNight测试959999/960000/1439999/1440000。`AutoEraWorldProcedure`调用`InitialRegionScene.Advance(realElapsed)`。segment-15、16、19；RuntimeSettings QA60e4f4f9。 | b02 5.1未含明确开发倍率接口/边界；5.5没有“不同帧分割＋直接推进＋跨昼夜＋打开实际UI”组合证据。5.2配置读取已实现，不是从零补；全部非法配置边界还需核对。开发倍率限开发用途，不增加玩家暂停/加速；完整离线调度器排除。 |
| P0-006 项目DataTable/Config/Language | 框架 `Assets/Game/ScriptsBuiltin/Editor/AIData/AIConfigAdapter.cs`、`AILanguageAdapter.cs`均有Parse/BuildRows/Read/Export/Reverse及指纹校验；`DataTableGenerationProfileEditModeTests`覆盖Schema、重复Key、路径、Core解析、回滚和缺失JSON前置失败。注意segment-09明确未实际执行工作簿Reverse，仅负例；segment-10仅代码接入诊断。`GameData/AIData/GenerationProfiles.json`存在，当前Foundation仅`Configs/Foundation/Runtime.json`；正式`Assets/Game/Config/Foundation/Runtime.txt`、AppConfigs mConfigs登记存在。 | b02 2.7不是无实现，而是三类等价成功路径/正式入口/诊断实跑证据不足。2.8缺Foundation DataTable和Language最小产品输入及正式读取证据；2.9 AppConfigs当前DataTable全Core、Language仅English，不能以Core加载替代项目三类贯通。按原b02最小启动/时间用途补JSON，不扩大P0-011；xlsx只能既有工具安全转换，框架改动仍须精确授权。 |
| P0-008 UI层级 | `UI/AutoEraUiFormBase.cs`继承UIFormBase、版本门、注册/注销、焦点恢复；FieldHudForm/BaseCommandHubForm与`AutoEraUiIntentRouter.cs`。正式Operations独立Prefab，表入口6001/6002，B10菜单6000。b05/b09已验收证据与B10 `AutoEraUiPrefabBindingEditModeTests` 7efef700；启动实跑GF.UI/HUD。 | 不重开B05、不重做视觉；“打开管理UI世界时钟仍推进”需要与P0-005补联合断言，现只从realElapsed调用可判断设计意图，不声称已测。各后续业务页面不纳本包。 |
| P0-009 InputModule与PC镜头 | `Input/RegionInputModule.cs`集中设备读取、IRegionInputSource可替换；`World/Region/RegionCameraController.cs`仅接语义；WASD/旋转/缩放/F/Esc/UI阻挡已接。`InitialRegionSceneEditModeTests`使用FixedSource验证阻挡/相机/选择；`RegionFieldAccessEditModeTests`验证进入退出迟滞。 | 基础实现已存在；独立物理鼠标/键盘完整链和焦点视觉记录未覆盖。不要因测试Provider是假源而改写输入抽象，也不新增右键作业指令。 |
| P0-010 EntityBase根与视觉子层 | `World/Region/InitialRegionEntity.cs : EntityBase`、`RegionObjectView.cs`、`InitialRegionScene.cs`；8枚`Assets/Game/Prefabs/Entity/InitialRegion/*.prefab`通过GF.Entity Show/Hide，对象ID/碰撞/锚点在逻辑侧，视觉子层复用。adb08994两轮8 Available与释放；b09正式Entity/Motion引用证据复用。 | B06 MotionRig资产本身不能替代GF生命周期，B10已补此缺口。当前只覆盖8对象最小模板；所有第一版对象可配置实例化属于P0-011，不扩成完整机器系统，也不重开B08/B09。 |
| P0-011 完整基础配置骨架 | 当前最小Runtime Config、Core UI/分组表和8对象Prefab不是全对象ID/分类/等级/能力/UI/Entity/Sound数据骨架。 | 明确尚未整体实现且本批排除；不能作为本包实现缺陷或顺手建表。后续需单独范围/任务表衔接，保持用户独占xlsx。 |
| P0-012 UI/实体/声音分组与降级 | `Assets/Game/DataTable/Core/UIGroupTable.txt`、`EntityGroupTable.txt`、`SoundGroupTable.txt`由AppConfigs加载；Entity组Default/Effect/Persistent，Sound组Music/Sound；UI/Entity真实加载已由B05/B10验证。 | 没有找到本包证据证明声音资源缺失仍安全启动、声音播放失败可定位且不破坏其他组。存在框架分组不能宣称项目声音安全降级通过。先独立测试既有组；若需新增资源/产品音频用例明确精确输入，不新造音频或改框架核心。 |
| P0-013 编译与纯度入口 | `tools/audit_framework_purity.py`、`tools/audit_product_profile.json`、`tools/audit_project_boundaries.py`；ProjectBaseline明确用法。B10已有普通编译/引用/启动记录、产品模式通过、纯度单测14/14、严格模式保留9项预期产品差异。 | 审计入口已实现，不得把严格框架模式对产品的拒绝算成待修故障，也不扩大全局白名单。随数据/分组剩余项完成后补一次对应集成检查；P0-012未覆盖不靠审计绿色替代。 |

## b02 未勾选的精确分类

1. **已有实现，主要需对账／补验**：2.7适配器、5.2配置时间读取、6.2场景协调器。不得重写。
2. **部分已实现仍有真实缺项**：2.8/2.9仅Runtime Config接入，5.1开发倍率，5.5组合确定性/UI不暂停，6.5重启/半初始化异常。
3. **集成项不能提前结账**：7.1三类往返与异常全套、7.3三类数据读取（两轮进出本身已测，不重复）；7.2/7.4已有当前编译和审计结果但后续代码增量后需对应验证。
4. **历史哈希门槛需制作人核对**：7.5“不变于最初任务表哈希”与用户后续维护现实不一致；本包只记录当前值，不改任务表、不恢复旧表、不擅改OpenSpec。

## 下一执行顺序（候选，需制作人按本清单续接）

1. **时间单元**：原b02 5.1/5.5；先补现有测试的直接/分帧跨昼夜、实际UI打开不暂停及数值边界，按已冻结规则补开发倍率边界。复用时钟，不写离线调度器。
2. **三类最小数据单元**：原b02 2.7–2.9；仅启动/时间所需Foundation JSON，安全工具成功往返、正式GF读取与诊断；不碰完整P0-011。精确数据字段先沿已有design/spec核对，若不够则请求裁定，不自行造领域表。
3. **既有启动异常单元**：原b02 6.5；只补重启/半初始化失败缺项；不重跑adb08994已覆盖取消/双击作为新交付，不修改通用框架。
4. **分组安全降级单元**：P0-012；先检查既有Sound加载失败处理与测试入口，确定精确产品测试范围后执行；不新增分组或音频资产作为默认前提。
5. **合并证据**：P0-008/009/010/013只复用和验新增差异，汇总G0未覆盖项给用户；P0-011、完整业务/物流/静态美术继续排除。

## 本包完成条件与移交

B10 1.2的“逐项对账”在此完成；“实际补齐全部基础”**没有完成**，本包不改其勾选。
没有Unity/资源占用，无技术执行阻塞；后续需要制作人依据上述剩余项续接精确实施范围，并核对7.5历史哈希口径。
用户可看已交付初始区域仍从 `Assets/Game/Scene/Launch.unity` 进入；不把该演示入口当作三类项目数据全部完成。
