# B10 客户端基础复用与实施清单

2026-09-10。已领取 b10-client-initial-region。当前阶段为实施前核对，不代表整批验收完成。

## 已检查的基础

- P0-003：ApplicationContext、CompositionRoot、ProcedureContextSlot、WorldSession/Factory已有实现和对应EditMode测试；不重写。实际缺少产品Startup/MainMenu/World Procedure与产品场景加载链，b02 6.*仍需补齐。
- P0-004：PersistentId、Allocator、Registry及Reference已经实现；区域机器、建筑、资源点直接使用同一个会话注册表，不增加另一套ID系统。
- P0-005：WorldClock、WorldDayNightRules、WorldEventSortKey、UTC Provider均存在。世界时钟配置加载、在线时钟驱动与正式入口仍待贯通；旧未勾选不等于无实现。
- P0-006：AI JSON共享安全管线与Config/Language适配器已有代码；本次不修改ScriptsBuiltin。现有Core UITable已提供FieldHudForm/BaseCommandHubForm稳定入口。
- P0-008～013：复用现有UI语义路由、GF加载与生命周期、正式Entity/MotionRig资产、资源目录规范。P1对象模型、占地服务和作业申请尚无实现；不把Motion演示状态当作业务权威。具体任务覆盖继续与QA矩阵核对，不凭标题勾选。
- b05/b09：Operations两个独立Prefab及UI打开/关闭测试已有；FieldHud仍有预览文案，需改为初始区域的真实公开摘要。b06/b08正式Entity保持原样，仅实例接入。

## 模块与精确文件计划

先实施不依赖美术的纯数据服务，随后Unity组合、输入、UI和场景接线：

- `Assets/Game/Scripts/AutoEra/World/Region/RegionObject.cs`：三类对象公开摘要及区域归属。
- `Assets/Game/Scripts/AutoEra/World/Region/InitialRegion.cs`：复用会话注册表、对象注销、选择与公开事件。
- `Assets/Game/Scripts/AutoEra/World/Region/RegionPlacement.cs`：0.5m位置/15°旋转吸附、区域边界与有向矩形占地；建造占用与导航阻挡分离。
- `Assets/Game/Scripts/AutoEra/World/Region/RegionWorkQueue.cs`：作业位拥有者、FIFO等待、取消/失效释放、矿脉有效范围。
- `Assets/Game/Tests/AutoEra/Editor/InitialRegionEditModeTests.cs`：身份、失效、输入阻挡、空间与竞争测试。
- 后续Unity和Procedure精确路径在实施前追加于本文件；禁止扩大到完整资源生产、物流权威或算法编辑器。

## UI对接公开字段

区域ID、对象永久ID、对象类别、显示名称、世界位置、公开状态文本、资料公开标志、可选公开资源数量、作业拥有者与等待数。
未实现的生产/电力/库存显示未接入或隐藏，不能输出示例常数。对象失效立即清除选择；机器申请由服务返回状态，30秒等待只提示，不自动失败。
建议复用 `Panel_ObjectQuick`、`Txt_HudObject`、`Art_Txt_HudObjectVisual`、`Art_HudEmptyState`、`Txt_HudTime`；准确视觉映射交2D核对。

## 快速执行候选检查

当前首段含对象生命周期、占地几何和作业授权判断，需客户端直接实现和复核；不是机械搬运。已有rapid路径审计独立进行。冻结后的测试重跑经QA/快速执行链完成。

## 工具与限制

驱动8090前核对health/project身份、退出PlayMode、与QA点对点交接。禁止Git索引和xlsx写入。所有任务勾选以实际验证为准。

## 首段编译与独立QA交接

2026-09-10：四个Region服务及客户端4项测试已写入。QA另外提供独立4项边界测试，未修改其断言。
已修正会话先释放时区域Count、Objects、SelectedId仍公开陈旧数据的问题，以及随后Dispose不能注销本地对象的清理路径。
普通asset_refresh后主工程2022.3.62f3c1非PlayMode、非编译，Console Error=0；已将8090交给QA运行两组测试。
这只是本段编译结果，不代表测试通过或B10 2.1～2.6完成。

## UI映射核对

已完整读取美术交付P1_UI_GapAudit_and_NodeMapping.md。实际可见摘要绑定Art_Txt_HudObjectVisual、Art_Txt_HudTimeVisual；
不启用inactive旧Txt_HudObject/Txt_HudTime，不新增字体或PNG。空态/错误/摘要互斥；无数据隐藏或显示未接入，不填演示常数。
运行1920×1080截图仍待真实场景接线后取得。

首段独立回归：InitialRegionEditModeTests job 13ca3eb1 4/4、QAInitialRegionBoundaryEditModeTests job 24fc698d 4/4；Console Error=0、Warning=0。QA已释放8090。

## 下一段精确路径：公开摘要投影

- `Assets/Game/Scripts/AutoEra/UI/RegionHudPresenter.cs`：构造注入区域，订阅选择和当前对象变更，对外只发布公开摘要；Dispose取消全部订阅，不控制生产或查找节点。
- `Assets/Game/Tests/AutoEra/Editor/RegionHudPresenterEditModeTests.cs`：选择切换、旧对象通知隔离、无资源不填0、注销及Dispose。
- `Assets/Game/Scripts/AutoEra/UI/FieldHudForm.cs` 后续增加显式显示接口和序列化TMP引用，由场景组合入口绑定；本段先验证投影，不修改Prefab。
本段包含生命周期/公开信息判断，由客户端执行；回归再交QA。

- `Assets/Game/Scripts/AutoEra/Editor/InitialRegionHudBinder.cs`：仅绑定FieldHudForm现有可见TMP/互斥状态，不运行旧全页重建器；通过PrefabUtility保存，无模型/字体移动。

## 后续场景与输入组合计划

在QA释放8090后按以下精确边界继续，不通过展示脚本伪造生产：

- `Assets/Game/Scripts/AutoEra/World/Region/RegionObjectView.cs`：配置化类型/占地/选择锚点，显式初始化和注销。
- `Assets/Game/Scripts/AutoEra/World/Region/InitialRegionScene.cs`：世界Procedure注入会话，拥有区域服务与视图绑定、作业通道，关闭时先释放区域后释放会话。
- `Assets/Game/Scripts/AutoEra/Input/RegionInputModule.cs`：唯一世界设备适配边界，可替换输入源；先判UI消费，再分发选择/镜头/放置意图。
- `Assets/Game/Scripts/AutoEra/World/Region/RegionCameraController.cs`：只消费语义数据，WASD平移/旋转/缩放/F聚焦，不读取设备。
- `Assets/Game/Scripts/AutoEra/Editor/InitialRegionSceneBuilder.cs`：新独立场景及本次资源点代理的可重复生成/显式序列化，禁止移动或覆盖既有正式Entity。
- 场景目标 `Assets/Game/Scene/InitialRegion.unity`；代理只进 `Assets/Game/Prefabs/Entity/InitialRegion/`，不进入Development或重建旧样机。

正式启动前仍需按b02补齐配置驱动Startup/MainMenu/World与GF场景协调器；当前AppConfigs没有产品Procedure或Config登记，不能声称Launch已进入区域。

2026-09-10：第二段QA投影1/1（482ef2e7）、UI回归6/6（7efef700），Missing Reference=0、Console Error=0。
InitialRegion.unity已通过独立additive场景生成并关闭，未覆盖原活动场景。含初始载体、仓库、农田、人工林、地表矿脉、水域、发电机、太阳能8个配置对象；生产数值仍未接入。
选择层经追加授权使用空槽8。修改前碰撞矩阵为全部允许；修改后只清除第8行/列（含自身），旧层间逐对比较不变。
Unity自动升级DynamicsManager序列化版本13→14及默认字段显式化，最初曾请求确认（历史状态）；现已由派发单2026-09-10补充授权允许同版本原生保存，未手改YAML。升级前未采集的有效默认字段注明未验证，不推断其等价。导航排除已由InitialRegionSceneEditModeTests job 878e25a3验证，后续输入扩展仍需新回归。

- `Assets/Game/Tests/AutoEra/Editor/InitialRegionSceneEditModeTests.cs`：独立加载保存场景，实际初始化8对象、重复会话绑定/注销、独立选择碰撞层和导航采集排除。

## b02配置贯通精确路径

- `GameData/AIData/Configs/Foundation/Runtime.json`：仅启动场景名及既定世界时钟参数；不存在对应正式xlsx时才允许经既有AIConfigAdapter工具首次Reverse。
- `Assets/Game/Scripts/AutoEra/Application/AutoEraRuntimeSettings.cs`：严格解析必需配置，禁止缺失时静默默认。
- `Assets/Game/Scripts/AutoEra/Editor/AutoEraFoundationDataSetup.cs`：产品Editor接线调用既有Validate/Reverse/RefreshAllConfig，按精确文件生成；不修改ScriptsBuiltin，不直接写xlsx。
- `Assets/Game/ScriptableAssets/Core/AppConfigs.asset`：仅通过SerializedObject追加Foundation/Runtime和产品Procedure，不改Core条目；主菜单入口最初待确认的记录已过期，现按派发单最小MainMenuForm授权实施。

Runtime配置已由既有工具生成xlsx及Assets/Game/Config/Foundation/Runtime.txt，AppConfigs只追加Foundation/Runtime。
逻辑指纹645C1A94BCFC2EEE3F9F46EE7885A4E80B46222DDBD95B9C2C2688EDCEEB75E4。
首次Editor编译暴露产品程序集不能直接引用Builtin.Editor；已改为按唯一完整类型名调用现有Editor API，不增加程序集依赖或修改框架。
生成入口成功执行且Console Error=0。未修改任务表；配置预加载与正式启动仍待PlayMode验证。
- `Assets/Game/Tests/AutoEra/Editor/AutoEraRuntimeSettingsEditModeTests.cs`：必需键、非法路径、世界初始日照及配置昼夜边界验证。

## 当前恢复入口（2026-09-10）

以 `Docs/Development/Dispatch/Active/b10-role-state-reconciliation-20260910.md` 及B10派发单最新补充授权为准。
旧b05、B08低模、b09退役与字体归并已完成，不重开。唯一Active为B10，暂无Pending。
最小MainMenuForm和八对象GF.Entity逻辑根／最小配置已授权，不再列为等待项；完整P0-011不在本批范围。
继续已有Startup/SceneFlow/World接线，普通编译后完成实际场景/UI/输入和独立QA，未完成的运行验收不提前勾选。

## 2026-09-12 客户端恢复只读复核

本次从 `b10-client-initial-region-resume` 恢复，仅对当前代码、B10证据和OpenSpec做对账，未重新实现已完成的区域/选择/作业服务，也未修改任务表。`openspec validate b10-interactive-world-and-static-art --strict`、`python tools/audit_project_boundaries.py` 与 `python tools/audit_framework_purity.py --product-profile tools/audit_product_profile.json` 均通过。

现有 B10 证据仍支持 1.2、2.1–2.7 的已记录客户端实现与回归；B10 tasks 中 1.4（制作人覆盖核对）、3.6（正式静态资源交付）、4.3（制作人整合验收）和4.4（整批收口）仍未勾选，不能由本次静态复核代替。P0-011完整数据骨架、算法→作业端到端、物流/农业/能源生产与正式静态资产交付继续排除。

当前未发现不依赖UI/美术且可在本任务内安全新增的代码缺口。下一独立客户端候选仍是建立专门OpenSpec后的“导航结果→Motion表现桥接＋作业安全点桥接”联合单元；其结果权威、取消/阻挡语义和任务表映射尚未冻结，本文件不授予实现权限。

## 下一独立单元：八对象GF.Entity逻辑根

专业判断单元由客户端执行，冻结后的回归交QA；不把完整P0-011数据骨架移入本批。
- 新增 `Assets/Game/Scripts/AutoEra/World/Region/InitialRegionEntity.cs`：继承已有EntityBase，持有RegionObjectView，仅在显示绑定，在隐藏/回收注销。
- 更新 `InitialRegionScene.cs`：运行时通过GF.Entity异步显示八对象，版本失效不绑定旧世界；GF实例Id与永久Id分离，使用现有EntityParams.Id分配，不自造第二套永久Id。
- 新增 `Assets/Game/Scripts/AutoEra/Editor/InitialRegionEntityBuilder.cs`：仅将本批场景对象配置为独立Entity Prefab，保留已验收载体为视觉子层，不改其源模型。
- 精确新Prefab：`Assets/Game/Prefabs/Entity/InitialRegion/{Carrier,Warehouse,Farmland,Forest,MineralVein,Water,Generator,SolarArray}.prefab` 及对应.meta。
- 使用已预加载Core/EntityGroupTable的Default组1；八对象资源路径/初始位置来自场景序列化种子，不新增完整EntityTable或分配全版本物品ID。
- 更新本批InitialRegion场景种子引用与独立测试；取消/显示失败/重复进入后注册表、GF实例和UI选择都必须释放。

## 自动审计当前差异

项目目录边界审计通过。框架纯度审计当前9项失败：Launch-only构建限制与新批准两场景冲突，
`MainMenu`字串遗留样例规则同时命中新产品类与生成UIViews。已向制作人请求产品实例审计范围裁定；
未修改门禁、未删除已批准入口，不把该结果记录为纯度通过。该门禁差异不阻止继续本批已授权实现。

上述为历史失败。获得派发单精确授权后已实现显式`--product-profile tools/audit_product_profile.json`与`--strict-framework`；
通用脚本无产品路径硬编码，仅MainMenu字串和精确场景例外，其他规则继续执行。
`python -m unittest tools.tests.test_audit_framework_purity` 14/14通过；产品模式通过，严格模式仍原9项，项目边界通过。
本单元解析/失败关闭语义需专业判断，客户端原子实现并复核；没有静默扩大豁免、修改框架核心或自动Git。

启动/基础独立QA：29b052c0 2/2、5858421c 1/1、875eb4f3 2/2、cfa6ab2b 3/3，总计8/8，Console Error/Warning=0。
随后八对象GF接线进入独立运行回归，尚不以纯编辑器断言替代G1。

最新运行回归3093292a对应NUnit XML Passed 1/1，两轮菜单→8个GF Entity/HUD→返回菜单释放，Console Error/Warning=0；旧失败保留于b02 segment-22。

下一段精确路径：新增World/Region/RegionFieldAccess.cs、Editor测试RegionFieldAccessEditModeTests.cs；
修改RegionCameraController.cs、RegionInputModule.cs、InitialRegionScene.cs、FieldHudForm.cs和AutoEraUiIntentRouter/Runtime.cs，
实现配置化距离/画面/缩放迟滞、管理层世界输入阻挡和只读现场提示。原权威为GameDesign/03-玩家体验/01-视角交互与信息呈现.md第一版PC键鼠与现场判定。
不开放硬件/交易操作、不绕过传感器、不增加玩家角色；灰盒距离/高度为可调默认值，不冻结新玩法数值。

### 当前 HUD 与作业摘要补齐（待独立回归）

- InitialRegion.cs 新增对象集合变更事件；RegionHudPresenter.cs/FieldHudForm.cs 订阅并更新真实计数，销毁后解除订阅。
- RegionObject.cs/RegionWorkQueue.cs/RegionObjectView.cs 将配置化作业通道的占用者、等待数投影到公开摘要；等待按优先级、同级FIFO，不能抢占当前拥有者。照料与综合保持独立通道。
- InitialRegionEntityBuilder.cs 仅给本批 Warehouse/Farmland/Forest/MineralVein Prefab 配置通道；不赋予生产/物流结算，不新增自动采矿或导航。
- AutoEraStartupAssetBuilder.cs 修复最小菜单按钮白字白底，沿用现有深色Surface；只改新MainMenuForm，不重做Operations样式。
- 测试：RegionFieldAccessEditModeTests、RegionHudPresenterEditModeTests、InitialRegionEditModeTests；等待QA本次新作业证据。快速执行候选检查：本段含生命周期/通道投影专业判断，客户端原子实施，独立测试交QA。
- 本次首次1920×1080运行图已暴露菜单对比不足与调试器遮挡HUD；旧图仅作为修正前证据，不作为视觉通过。

独立QA：RegionFieldAccess 6d9e9c69 2/2、RegionHudPresenter 4ad31ba8 1/1、InitialRegion 56769587 5/5、InitialRegionScene 818280b2 1/1，共9/9，Console Error/Warning=0。

下一可视单元复用 RegionObjectView.cs/RegionInputModule.cs 添加地面悬停/选中轮廓与对称释放；
新增 Editor/InitialRegionRuntimeEvidence.cs（及.meta）仅供PlayMode取证，提供隐藏调试覆盖、选中与注销测试入口，不保存运行状态、不构造业务成功。
InitialRegionSceneEditModeTests.cs补选择轮廓状态断言。此单元与客户端当前8090取证现场不可分离，直接执行后交独立QA。

放置预览精确范围：新增World/Region/RegionPlacementPreview.cs及Editor/RegionPlacementPreviewEditModeTests.cs；
RegionInputModule接入指针、R与Esc，白色合法/红色冲突轮廓；确认时重新验证占地，仅调用调用方回调。
本轮Editor取证菜单默认4×3矩形只验证P1吸附，不创建建筑、不扣费、不替代后续施工系统。
RuntimeEvidence补非零队列/超距/长文本取证入口：仅当前PlayMode中测试夹具，不保存，不作实际生产证据。

QA新增回归：d932224a（放置1/1）、c823804a（场景高亮1/1）、cfcad580（启动资产2/2），Console Error/Warning=0。
实际运行图client-menu-fixed/client-hud-{none,machine,building,resource,invalidated,work-waiting,long,readonly}.png已交2D，
2D逐张确认短摘要、非零队列、长文本与只读可读；注销后计数8→7。测试队列的额外2个纯数据机器明确不算GF实体交付。

资源代理最小可辨识性：InitialRegionEntityBuilder.ImproveProxies通过PrefabUtility仅修改本批7个非机器代理，
RegionObjectView使用材质属性块；树林柱体/低矮矿石形体只作代理，无逐树/逐矿数量权威，无碰撞，不影响矿脉可通行规则。
菜单失败取证发现重复加载已在内存的主菜单覆盖原始错误：AutoEraSceneFlow在自身拥有的同路径场景仍加载时直接复用，
AutoEraStartupFlowEditModeTests新增缺失世界场景→原始错误保留→恢复配置重试的运行回归。待本次QA，不复用旧成功job。

### 加载边界补验（2026-09-10）

- QA 已回传 f7fa00a6 对应权威 TestResults.xml：启动两轮和缺失场景恢复共2/2通过；76e19d05场景1/1、61d0b904放置1/1。域重载使job查询失效，采用XML，不复用历史job。
- 缺失场景 Warning 未留存，本证据不宣称该日志已验证；失败说明和重试状态由测试断言验证。
- 下一原子单元只修改 AutoEraMainMenuProcedure.cs 的待进入防重复条件、AutoEraStartupFlowEditModeTests.cs 的同帧双击及真实GF取消/重新加载断言、InitialRegionRuntimeEvidence.cs 的区域内注入禁止条件。不新增产品指令、包或程序集；该异步生命周期验证含专业判断，由客户端完成，独立执行交QA。
- 普通编译已通过，Console Error=0；新增测试先前因无法访问UtilityBuiltin而编译失败，已改为从EditorBuildSettings读取真实启用路径，未扩大程序集依赖。
- 产品模式纯度审计与项目边界审计通过；QA任务 b10-startup-cancel-double-submit-regression 已入列并交出8090。新三例结果待回传，不提前记通过。
- 现行作业验收仅真实服务申请/释放/等待转交和HUD投影；开发夹具不是玩家命令。算法触发端到端、实际生产物流和导航等待点不属于本次已验证内容。

最终加载边界QA adb08994：权威XML三例3/3通过，Console Error/Warning=0，8090已释放。
取消例v4在域重载后创建独立协程/闭包，6.744335秒；两轮双击10.474363秒；缺失场景恢复6.641913秒。
此前734b976b、3327f117、a8ee103b均2/3，仅作失败历史，不能混为当前通过结果；
其iterator空引用不再根据单一源码行号判定为配置/产品SceneFlow故障。v4记录Load前后、旧0新1回调及释放完成。
详细范围与不应推导的结论见client-coverage.md；本证据不代表B10全部美术完成或用户视觉验收。
