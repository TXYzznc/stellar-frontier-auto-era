# 任务

## 1. 落位事务的生产入口

- [x] 1.1 把机器占地显式化：`MachineDefinition` 增加占地字段，数据表 `MachineDefinitions` 加列
      （只加列、不改既有列），数据表生成走既有 Validate→Reverse 流程，不手改生成 C#。
      集成测试里散落的 `new Vector2(1.8f, 2.4f)` 改为读定义。
      （已实现，走完整受控通路：`GameData/AIData/DataTables/Machines/MachineDefinitions.json` 加
      `FootprintX`/`FootprintZ` 两列（**轴名显式写死**——设计文档按「长×宽」记占地，不写死轴向会在实现侧再次推错）
      → `Validate DataTables Json` → `Reverse DataTables Json To Excel` → `Refresh All Excels`；
      生成物 `Assets/Game/DataTable/Machines/MachineDefinitions.txt` 与
      `Assets/Game/Scripts/AutoEra/DataTable/MachineDefinitions.cs` 均含两列与取值，字符串与二进制两条解析路径都已生成。
      **数值来源是设计文档而不是臆造**：轮式载体交互占地 2.6×1.8 米（DEC-147 与灰盒对象规格），
      按文档的三元组顺序（长×宽×高 = Z×X×Y）落成 `FootprintX=1.8, FootprintZ=2.6`；
      固定旋转载体地基占地 2×2 米 → `2, 2`。
      `MachineDefinition` 新增 `FootprintX`/`FootprintZ`/`HasFootprint`/`Footprint`，
      参数带缺省以便既有夹具不改；`MachineCatalog` 的数据校验层拒绝负数／非有限值／**半个轴配了值**。
      **0 表示「未配置」而不是零尺寸**，放置流程据此拒绝（见 1.2）。
      注：本次 Reverse 顺带把三个 Core 表的工作表名由 `Sheet1` 规范成 `Sheet 1`（**逐行内容完全相同**，
      已用 openpyxl 与 HEAD 逐格比对确认）；`Core/UITable` 因源 xlsx 指纹不符被工具**拒绝写入**（安全机制生效）。）
- [x] 1.2 新增 `MachineDeploymentFlow`：`TryBegin` 前置校验（在册、未部署、区域活跃、定义可用）、
      `Preview` 暴露给界面读 `IsValid`/`Reason`、`Commit` 只调 `DeployMachine`、`Cancel`。
      （已实现：`Assets/Game/Scripts/AutoEra/World/Region/MachineDeploymentFlow.cs`。
      校验完全复用现成的 `RegionPlacementPreview`（含「确认时再校验一次」），本类只负责三件事：
      前置条件、占地从定义取、提交去向。
      **不做结算**；视图／导航／运行时也刻意不在这里（它们是部署成功后的派生步骤，见 design.md D2–D5）。
      新增 `MachineDeploymentOutcome` 七个可辨结局与 `Describe(outcome)`——界面直接展示原因字符串，不再各自拼。
      `LastOutcome` 是一次历史事实，`Cancel` 不清它。为让既有夹具零改动，占地参数走缺省值。）
- [x] 1.3 `RegionInputModule` 增加「驱动已存在预览」的入口，与原 `BeginPlacement(size, confirmed)`
      合并为同一个预览所有者；编辑器证据工具的「Begin Placement Preview」改为走新入口，
      并去掉「no construction transaction」的占位回调（改为调用流程或明确标注仅预览）。
      （已实现：新增 `BeginPlacement(MachineDeploymentFlow flow)`。两个入口都先 `EndPlacement()`，
      因此任意时刻只有一个预览。新增非序列化 `_ownsPlacement`：**只销毁自己建的预览**，
      流程拥有的预览由流程自己管——与 `RegionObjectView._ownsModel` 同一个判据。
      **关键点**：点击提交时模块调用流程的 `TryCommit` 而**不是**预览自己的 `Confirm`，
      否则会绕过结局映射，界面就只能拿到「成功/失败」而没有可展示原因。
      失败不结束落位（玩家可继续挪位置重试），与接入前的既有行为一致。
      编辑器证据工具保留原地：它的回调本就写着「preview only」，属于**明确标注的仅预览**用法，不接流程。）
- [x] 1.4 测试：`TryBegin` 的四条前置拒绝各自可辨；`Commit` 成功后区域对象出现、花名册转入已部署；
      重复 `Commit` 不产生第二个对象；`Cancel` 后区域无变化。
      （已实现：`Assets/Game/Tests/AutoEra/Editor/MachineDeploymentFlowEditModeTests.cs`，**7/7 通过**：
      ① 未配置占地被拒且不留预览；② 未知机器与已部署机器各自被拒；③ 占地确实来自定义
      （断言 `Size` 等于定义值而不是常量）＋未选点时提交什么都不做；④ 占地冲突时原因**来自区域校验**
      而不是泛化失败；⑤ 选点之后场地变化 → 确认时再校验并拒绝（「位置已经失效」）；
      ⑥ 取消幂等、区域无变化，且同一流程实例可复用（界面就是反复开的）；
      ⑦ 重复部署同一台机器到不同位置不瞬移，且已部署机器的 `TryBegin` 被拒。）

## 2. 机器视图绑定（不重复注册）

- [x] 2.1 `RegionObjectView` 增加 `BindDeployed(region, id)`：绑定**已存在**的区域对象，不调 `Register`；
      取不到则抛，调用方必须先部署成功。
      （已实现：`Assets/Game/Scripts/AutoEra/World/Region/RegionObjectView.cs`。除「取不到就抛」外，
      还会拒绝无效 id 与 null 区域，并在失败时不留半绑状态。**刻意不设置公开状态**——
      机器的公开状态属于领域，视图不该替它决定，这也是它无法复用 `Initialize` 的第二个理由。）
- [x] 2.2 `Release` 区分来源：注册来的对象移除，绑定来的对象**不移除**领域对象。
      用一个内部来源标志，不靠 `Model != null` 推断。
      （已实现：新增非序列化字段 `_ownsModel`；`Initialize` 置 true、`BindDeployed` 置 false。
      顺带把选中轮廓的尺寸从「视图序列化占地」改为「领域对象的实际占地」——
      场景种子两者相等故行为不变，绑定机器时序列化占地并非权威值。）
- [x] 2.3 测试：`BindDeployed` 后区域对象数**不增加**（这是本条的核心断言）；
      `Release` 后领域对象仍在且无副作用；注册路径行为完全不变（回归）。
      （已实现：`Assets/Game/Tests/AutoEra/Editor/RegionObjectViewBindingEditModeTests.cs`，**5/5 通过**：
      ① 绑定后 `region.Count` 不变且 `Model` 是同一个对象；② 释放后区域对象仍在、`machine.Deployed` 仍为真、
      且**能再绑一个新视图**（「表现缺失不等于数据损坏」的可验证形态）；③ 未部署时绑定必抛且不留半绑状态；
      ④ 同一视图不得重复绑定；⑤ 场景种子路径仍然注册并在释放时移除自己——这条通路的行为刻意保持不变。）

## 3. 机器实体生成与导航绑定

- [x] 3.1 预制体解析：从 `MachineDefinitions.Prefab` 拼 `Assets/Game/Prefabs/Entity/{Prefab}.prefab`，
      不硬编码映射；缺失或无效时走 3.4 的降级。
      （已实现，且**发现并补掉一个真实缺口**：预制体名原本只在 `MachineCatalog` 里按**数据行 Id**（10011）
      索引，而运行时只拿得到 `MachineDefinition.Id`（ModelId，1001），中间少一层映射会让
      「有实例却找不到预制体」变成必然。因此把 `Prefab` 加进 `MachineDefinition`（它本就是定义数据），
      由目录从数据行填。路径规则不在本变更里硬编码：框架已有
      `UtilityBuiltin.GetEntityPath(name)` → `Assets/Game/Prefabs/Entity/{name}.prefab`，
      与区域种子的 `InitialRegion/Warehouse` 用法一致；而 `MachineCatalogEditModeTests` 已经在验证
      `Prefab` 列的路径可加载。）
- [x] 3.2 部署成功后按解析到的预制体生成实体（复用 `InitialRegionEntity` + `RegionObjectView.BindDeployed`），
      位置/朝向取自 `RegionObject`。
      （已实现，但**没有复用 `InitialRegionEntity`**——核实后发现机器实体预制体**不带 `RegionObjectView`**
      （那是区域种子的配置，用来把场景里摆好的对象**注册**成区域对象），而 `InitialRegionEntity` 用
      `RequireComponent` 依赖它。因此新增 `InitialRegionMachineEntity`：初始化时补上 `RegionObjectView`，
      再走 `BindDeployed`。**不需要改动 B08 交付的正式实体预制体**——框架的 `Entity.ShowEntity` 在预制体
      没有实体逻辑时会 `AddComponent` 补上（`Entity.cs:122`），这正是这条链路能成立的前提。
      位置/朝向取自区域对象（`model.Position`/`model.Yaw`），通过 `EntityParams` 交给实体。
      实现落在 `InitialRegionScene.TrySpawnMachine`（表现层持有区域的地方），并带
      `HasMachineEntity`/`MachineEntityCount`/`FindMachineView` 三个测试与调试入口，
      `Release` 统一回收机器实体。
      **降级语义**：`TrySpawnMachine` 返回 false 只表示「没有表现」，不表示部署失败——
      区域对象与花名册状态在 `DeployMachine` 成功时就已成事实。）
- [x] 3.3 视图就绪且定义 `CanMove` 且导航面就绪时执行 `RegionNavigation.Bind`；
      失败**不得**是异常，转为 D3 的两类状态。
      **【顺序修正 2026-09-21】本条不能按原顺序做**：`RegionNavigation.Bind` 的第一个参数就是
      `MachineExecutionContext`，所以「运行时创建」（第 4 节）必须**先于**导航绑定。
      正确顺序是 3.1/3.2 → 4.1/4.2（运行时注册表）→ 3.3/3.4（绑定与降级）。
      （已实现，落在 `RegionMachineRuntimeRegistry.TryAttach`：`Bind` 要 `MachineExecutionContext`，
      所以绑定必须与运行时创建在同一处按顺序发生，分到两个类里只会制造一个假依赖方向。
      验证 `MachineDeploymentRuntimePlayModeTests`：真实 NavMesh 下部署并生成实体后
      运行时 `HasNavigation == true`、`IsNavigationDegraded == false`、`Adapter.HasNavigation == true`、
      区域导航 `BindingCount` 由 0 增到 1。）
- [x] 3.4 降级路径：可移动但导航面未就绪 / 预制体缺 `MotionRig` / 位置不匹配 / 定义不可移动 ——
      四种情形各自给出可展示原因，且机器保持已部署。
      （已实现，四个原因字符串互不相同，顺序即优先级：
      ① `没有实体视图，无法绑定导航`；② `该区域未配置导航地面`；③ `导航面尚未就绪`；
      ④ `导航绑定失败：<Bind 的原始消息>`——④ 兜住「预制体缺 `MotionRig`」「位置不匹配」「已重复绑定」等
      `Bind` 自己的前置，因为它在生产里都可能不满足，所以是**降级**而不是异常：
      机器保持已部署、运行时照常建立，只是不能自主移动。
      **定义不可移动（`CanMove == false`）不是降级**：`NavigationUnavailableReason` 为 null，
      是否有导航能力只看 `HasNavigation`——写成原因会让界面把「本来就该如此」显示成故障。
      验证：EditMode 用例 ② 断言三个原因字符串 `Is.Unique`；
      PlayMode 用例 ③ 用「位置正确但没有 `MotionRig` 的裸对象」逼出④，并断言
      `BindingCount` 不增、区域对象仍在、`blind.Deployed` 仍为真。）
- [x] 3.5 测试：可移动机器绑定成功后 `Navigation.BindingCount` 增加；
      不可移动机器不绑导航也不报错；四种降级原因各自可辨。
      （已实现，两个新测试类：
      `Assets/Game/Tests/AutoEra/Editor/MachineDeploymentRuntimeEditModeTests.cs` **5/5 通过**，
      `Assets/Game/Tests/AutoEra/Editor/MachineDeploymentRuntimePlayModeTests.cs` **1/1 通过**。
      可移动机器绑定 → `BindingCount` 0→1；不可移动机器 → 有运行时、`HasNavigation == false`、
      原因 null、且 `Bind` 从未被调用；四种降级原因如上各自可辨。）

## 4. 区域机器运行时

- [x] 4.1 新增 `RegionMachineRuntimeRegistry`（区域级、`IDisposable`），
      按机器 id 持有 `RegionMachineRuntime`（`MachineExecutionContext` + `AlgorithmInstanceService`
      + `AlgorithmMachineAdapter` + 可空 `MachineNavigation`）。
      （已实现：`Assets/Game/Scripts/AutoEra/World/Region/RegionMachineRuntimeRegistry.cs` 与
      `RegionMachineRuntime.cs`。注册表**没有任何序列化状态**——这条本身就是 D4 的实现手段。
      刻意保留的取舍：`AlgorithmInstanceService` 的 `hardwareRevision` 目前传
      `() => (ulong)machine.Revision`。这是**比设计更严的超集**（任何机器状态变化都会让已应用的
      算法请求失效），因为 `MachineInstance` 没有独立的硬件修订号；等出现可编辑的硬件装配流程时
      应当换成专用编号，否则会退化成过度的失效。）
- [x] 4.2 `InitialRegionScene` 在部署成功后向注册表登记，`Release` 时统一销毁；
      区域切换与场景卸载不得泄漏订阅。
      （已实现：`InitializeNavigation` 建立注册表（**即使没有导航地面也建**——不可移动机器与
      「导航面未就绪」的机器同样需要算力池、任务队列与传感器）；机器实体的
      `OnShowCallback` 在视图绑定之后紧接着 `TryAttachMachineRuntime`；
      `Release` 的顺序是 传感器 → 机器实体 → 运行时 → 导航 → 区域对象，
      边界由 `MachineDeploymentRuntimePlayModeTests` ⑤ 钉住：释放后注册表为 null、
      `MachineEntityCount == 0`、`BindingCount == 0`、`navigation.IsReady == false`。）
- [x] 4.3 `AlgorithmMachineAdapter` 支持**无导航**形态（不可移动机器）：构造可空或以显式
      「无导航」标识代替伪造对象，`IsSafe` 语义明确。
      （已实现：`_navigation` 可空 + 公开 `HasNavigation`；`IsSafe` 在无导航时只看是否有未完成的意图
      （`_navigation == null || !_navigation.IsActive`）；`Submit` 收到 `Navigate` 时**立刻按拒绝回报**
      （`Publish(..., "rejected")`）而不是排队等一个永远不会发生的执行；`Pump` 里另有一条兜底同样报拒绝。
      刻意不伪造一个「永远失败的导航对象」——那会把「不可移动」伪装成「导航出错」。）
- [x] 4.4 明确并测试「运行时不进存档」：区域重建后运行时重新创建、机器回到空闲态，
      且重建不依赖任何序列化状态。
      （已实现并由 `RuntimesAreNotPersisted_TheRegionRebuildsThemFromDomainFactsOnly` 守着：
      先往运行时队列里放一件「进行中的工作」使 `WaitingCount == 1`，销毁注册表后重建 → 新运行时
      `WaitingCount == 0`（派生数据不带回来），而 `machine.Deployed` 与区域对象仍然成立（领域事实与运行时无关）。
      存档里只有部署事实（`machine.Deployed` + `RegionBindingOwner`），重建只读它。）
- [x] 4.5 测试：部署后运行时存在且可枚举；区域 `Release` 后注册表为空、无回调残留；
      不可移动机器同样拥有运行时（算力/传感器与移动无关）。
      （已实现：EditMode 用例 ① 断言不可移动机器有运行时且 `Context.Tasks`/`Compute`/`Sensors` 全部非空；
      用例 ③④ 断言登记幂等、`Detach` 只撤指定一台、释放后 `Count == 0` 且再登记被明确拒绝；
      PlayMode 用例 ⑤ 断言区域释放后绑定清空。**「无回调残留」的判据取区域导航的 `BindingCount`**——
      `RegionMachineNavigationBinding` 订阅了 `Tasks.CancellationRequested`、`Machine.Changed` 与
      `Region.ObjectRemoved`，如果绑定没被释放，`_bindings` 字典就会留下条目。）

## 5. 界面回填

- [x] 5.1 `WorldPlacementForm` 机器部署页接入：选机器 → 落位预览（读 `IsValid`/`Reason`）→
      确认 → 结果；建造放置与世界绑定两页保持诚实不可用且写明原因。
      （已实现。这一页相对其它管理页有一条**反向性质**：它**不挡世界输入**，因为规格写明
      「世界虚影可见，底部居中操作条，只拦截 UI 占用区域，不用全屏遮罩」。
      落地做法是把「谁挡输入」从路由里的类型判断改成界面自己的声明：
      `AutoEraUiFormBase.BlocksWorldInput`（默认 true，`FieldHudForm` 改成覆写，
      `AutoEraUiRouter` 不再认识任何具体界面类型），`WorldPlacementForm` 按当前页覆写为
      「只有机器部署页不挡」。若照默认挡住，玩家会一边看着页面一边点不到世界，预览永远不动。
      接线要点：
      ① 打开参数带**稳定身份**——`AutoEraUiPageRequest` 增加 `Target`（`PersistentId`），
         `MachineLibraryForm` 的「部署」按钮（未部署页与整备页两处）据此打开本页；
         身份失效时本页禁用并说明，**不按名字回退查找对象**（规格要求）。
      ② 现场输入模块随会话传递——`AutoEraUiSession` 增加 `RegionInput`，
         `AutoEraWorldProcedure` 从区域入口同一个根节点取 `RegionInputModule`。
         理由：指针→区域坐标、旋转、点击提交都已经由它承担，界面自己再实现一套就会造出第二个预览。
      ③ `RegionInputModule` 新增 `ConfirmPlacement`（与鼠标点击**同一条提交路径**）、
         `RotatePlacement`、`CancelPlacement`（只清自己的引用，流程拥有的预览由流程释放）。
      ④ 预览合法性每帧复核但**只在真的变化时重绘**（比较字段，不造字符串），
         否则每帧造字符串与重排列表就是稳定的 GC 来源。
      ⑤ 取消意图在本 Form 每一页都接管为「关闭本页」：本 Form 在 UITable 里 `EscapeClose=false`，
         基类会返回 false，而本页不挡输入，`RegionInputModule` 就会接着把预览结束掉——
         表现为「按 Esc 预览没了、页面还开着」。接管后与「取消」按钮同一条路径。
      ⑥ 契约新增三个按钮绑定 `_machineDeploymentRotateButton`/`Confirm`/`Cancel`
         （`EXTRA_BINDINGS`）并跑完整生成链：`ui_spec_to_contract.py` → `ui_contract_to_form_script.py`
         → 编译 → `按契约刷新所有页面绑定（不改结构）`；**预制体结构未被触碰**，门1 仍 33/33。
      验证：`Assets/Game/Tests/AutoEra/PlayMode/WorldPlacementFormPlayModeTests.cs` **1/1 通过**——
      页面打开后 `AutoEraUiRuntime.BlocksWorldInput == false`、预览由输入模块持有、
      `MovePlacementTo` → `PlacementValid == true` → `ConfirmPlacement` → 结局 `Deployed`、
      花名册转入已部署、区域对象正好 +1、按钮可点性正确、关闭后预览不留；
      并用建造放置页做反向对照（它挡住输入、业务按钮被禁用、出口保留）。
      建造放置与世界绑定两页仍整页不可用且各自写明原因，段落行显式清空不留示例行。）
- [x] 5.2 `MachineLibraryForm` 部署按钮从禁用改为启动 `MachineDeploymentFlow`；
      其余依赖组件/库存域的动作保持禁用与原因。
      （已实现：未部署页与整备页两处「部署」按钮都走 `OpenDeploymentForSelection` → 打开
      `WorldPlacementForm` 的机器部署页并带上选中机器的**稳定身份**。按钮可点性跟着选中项走：
      没选机器、或选中的是已部署机器时明确禁用（而不是点了没反应）；`ApplyUnavailableActions`
      在选中变化时也会重算。改名／安装／卸载／升级／出售仍然禁用以待接入。
      验证：`Assets/Game/Tests/AutoEra/PlayMode/MachineLibraryDeployEntryPlayModeTests.cs`
      **1/1 通过**——点真实的行按钮选中机器后按钮才变可点，点它打开的是机器部署页且
      `DeploymentTarget` 等于那台机器的 Id；同时断言其余动作仍不可点。
      注：列表行的 `UiListRowItem` 是 GF 对象池对象（`UIItemObject : ObjectBase`）不是 Component，
      `GetComponentsInChildren<UiListRowItem>` 会直接抛异常——按 `Button` 找行才是稳的。）
- [x] 5.3 四个算法界面（`AlgorithmEditorForm` / `AlgorithmLibraryForm` / `AlgorithmBindingForm` /
      `NodeComponentPickerForm`）替换 `AlgorithmReadModels.Create` 的接线分支，
      有运行时则读真实实例服务，无运行时仍报 `Unavailable` 并给出可辨原因。
      （已实现，且**偏离了原设计的一句话**：原设计说「接线后四个界面无需改动」，实际不成立——
      四个界面的 `Render` 原本无条件 `ShowPageUnavailable`，读模型一变 Empty/Ready 它们就在撒谎。
      现在 `AlgorithmReadModels.Create` 逐层解析并各给可辨原因：会话 → 世界 → 现场区域 →
      运行时注册表 → 区域**选中的机器** → 那台机器的运行时。机器身份取区域选中对象
      （稳定 Id，不按名字查找；也没有「只有一台机器就猜它」的回退）。
      真实实现 `MachineAlgorithmReadModel` 读 `RegionMachineRuntime.Instances`，
      三态：Unavailable（缺能力）／Empty（运行时在、还没有实例）／Ready（有实例）。
      为此新增 `AlgorithmInstanceService.ListInstances()` + `HasInstance()`（`_entries` 原本私有，
      界面没有任何观察入口）与 `AutoEraUiSession.MachineRuntimes`（世界流程从区域入口取）。
      `AlgorithmEditorForm` 现在按三种状态分别渲染：节点栏列实例与版本，检视器列机器、算力占用与
      导航状态；另外三个界面把 Empty 与 Disabled 分开（新增 `AutoEraShellFormBase.ShowPageEmpty`），
      模板库仍无创建者这一缺口在该状态里写明。
      验证：`AlgorithmReadModelEditModeTests` **9/9 通过**（六层原因各自可辨 + Empty + Ready +
      按稳定 Id 选中 + 不可用时选中是 no-op）；数据流用例另断言真实界面从 Empty 变 Ready。
      顺带修掉一个真实缺陷：`AlgorithmInstanceService.Add` **不触发 `Changed`**，
      新加实例时订阅者永远停在旧状态——`MachineDeploymentDataFlowPlayModeTests` 抓到了它。）
- [x] 5.4 `BaseCommandHubForm` 远程机器详情读运行时状态（任务、算力占用）；统计聚合仍不可用。
      （已实现：`RenderObjectsDetail` 在机器域详情行之后追加运行时行——任务队列等待数、
      算力占用／等待／逻辑上限、导航状态（含降级原因）、算法实例数、传感器是否建立。
      三层缺失各给可辨原因：没有运行时注册表／机器没有运行时／运行时就绪。
      枢纽只读既有事实，不创建任何东西；统计页的指标与长期汇总仍陈述「聚合层未接入」。）
- [x] 5.5 门1 复核 33/33，确认界面改动未触碰结构。
      （已复核：门1 通过（`Docs/Development/UI-PrefabLayouts` 下全部契约满足 L1/L2/L3）。
      本轮界面改动**没有动结构**：契约只新增了 `WorldPlacementForm` 的三个按钮绑定
      （`EXTRA_BINDINGS`），预制体是走「按契约刷新所有页面绑定（不改结构）」更新的，
      节点树与 RectTransform 一字未改。）

## 6. 验收

- [x] 6.1 编译 0 错（程序集比源码新且 console 无 CS 错误）。
      （`tools/_unity_compile.py` 通过：程序集已更新且 console 无 CS 错误。工程检查第 1 项同结论。）
- [x] 6.2 门1 契约自检 33/33；`run_project_checks.py` 5/5。
      （门1 通过；`run_project_checks.py --port 8090` **5/5 PASS**：编译 0 错、悬空引用 0、
      AppConfigs 12 表/1 配置/3 语言/6 流程、AIData 校验 12 成功 0 失败、框架纯度与项目边界通过。）
- [x] 6.3 EditMode 12 类与 PlayMode 3 类回归全绿。
      （实测规模已扩到：EditMode **20 类**全绿——原 12 类 +
      `MachineDeploymentFlowEditModeTests` 7/7、`RegionObjectViewBindingEditModeTests` 5/5、
      `MachineDeploymentRuntimeEditModeTests` 5/5、`MachineCatalogEditModeTests` 16/16、
      `AutoEraUiIntentRouterEditModeTests` 2/2、`AlgorithmInstanceEditModeTests` 5/5、
      `AlgorithmExecutionEditModeTests` 7/7、`AlgorithmGraphEditModeTests` 4/4、
      `AlgorithmServicesIntegrationTests` 1/1；外加三个进 Play Mode 的类
      （`MachineDeploymentSpawnEditModeTests` 1/1、`MachineDeploymentRuntimePlayModeTests` 1/1、
      `AutoEraStartupFlowEditModeTests` 6/6）。
      PlayMode **6 套**全绿：原 3 套（全 UI 打开／操作界面／导航）＋
      `WorldPlacementFormPlayModeTests` 1/1、`MachineLibraryDeployEntryPlayModeTests` 1/1、
      `MachineDeploymentDataFlowPlayModeTests` 1/1。）
- [x] 6.4 新增数据流测试：「部署 → 区域对象 → 视图不重复注册 → 导航绑定 → 运行时出现 →
      算法界面 Ready」，并覆盖三条降级路径。
      （已实现：`Assets/Game/Tests/AutoEra/PlayMode/MachineDeploymentDataFlowPlayModeTests.cs`
      **1/1 通过**。它在**同一次运行**里走完整条链并断言**身份在每一层是同一个**：
      部署只多出一个区域对象 → 实体视图指着那个对象且区域对象数不变（不重复注册）→
      运行时出现且 `HasNavigation == true`、区域导航 `BindingCount == 1` →
      选中该机器后打开 `AlgorithmEditorForm`，先是 `Empty`（运行时在、还没有实例），
      加入一个真实 `AlgorithmRuntime` 后变 `Ready`。
      尾部覆盖三条降级：没有实体视图／缺 `MotionRig`（`Bind` 抛异常被吞成降级且不留半个绑定）／
      定义不可移动（`HasNavigation == false` 且原因是 null）。
      **这条用例当场抓到一个真实缺陷**：`AlgorithmInstanceService.Add` 不触发 `Changed`，
      于是新加实例后界面一直停在 Empty——数据对了、界面是旧状态，看起来像界面没接线。）
- [x] 6.5 把 `ui-game-system-integration` 的 8.1 补记标注为「已由本变更落地」，
      并留痕本变更实际偏离设计的地方。
      （已在 `openspec/changes/ui-game-system-integration/tasks.md` 追加
      「8.1 落地结论（2026-09-21）」：六段批次划分逐段对照到本变更的产物与验收，并留痕三处偏离——
      ① 四个算法界面的 `Render` 必须改（原设计说无需改动）；
      ② 机器身份来源固定为「区域当前选中对象」；
      ③ 模板库仍无生产创建者，实例与模板在快照里分成两个字段。）

## 7. 边界

本变更不实现：建造放置（图纸→建筑）、世界绑定页、机器的库存与经济结算、离线推进、
算法域的完整体验（节点库、草稿编辑、问题清单按算法域自身合同推进）。

不修改界面契约与预制体结构；不改 `UIViews` 登记。

## 8. 顺带修掉的既存缺陷（留痕）

- **`RegionNavigation` 在编辑态调 `Object.Destroy`，使「在编辑器里打开区域场景再释放」假失败。**
  实测：`InitialRegionSceneEditModeTests.SavedRegion_RegistersAndReleasesActualViews_AndSelectionIsIsolated`
  在**完全原始的 HEAD 代码**下（本地改动与新测试都已移开、编译干净）以完全相同的栈失败：
  `RegionNavigation.ReleaseData()` → `Object.Destroy(_data)` → Unity 记录
  「Destroy may not be called from edit mode」**错误日志** → EditMode 测试把它当未预期日志判失败。
  **定性依据**：先把 `RegionObjectView.cs` 还原为 HEAD 并编译成功，再跑该测试，得到同一栈同一消息；
  这条路径与 `RegionObjectView` 无关（失败点在 `RegionNavigation.Dispose`）。
  因此这是既存缺陷，不是本变更引入的回归，也解释了为什么它一直没被发现——**它不在常规回归的
  EditMode 12 类里**。
  改法：新增 `RegionNavigation.DestroySafely`，按 `Application.isPlaying` 分流
  `Destroy` / `DestroyImmediate`，并把该文件内四处销毁（重建失败回滚、`ReleaseData`、
  绑定构造失败回滚、绑定释放）统一走它。
  验证：该测试由 **0/1 变为 1/1 通过**；区域与导航相关 13 个测试类共 63 项全部通过，无回归。

- **unity-skills 测试运行器的 `batch_state.json` 共享冲突（基础设施，非代码缺陷）**。
  现象：`[UnityTest]` 且 `EnterPlayMode` 的测试（`AutoEraStartupFlowEditModeTests`）或 PlayMode 套件报
  `Failed to reconnect test callbacks: Sharing violation on path Library/UnitySkills/batch_state.json`，
  `totalTests=0`。
  **判据**：失败点与业务代码无关；同一时刻跑不进入 Play Mode 的 EditMode 类全部正常；
  域重载期间插件边写该状态文件边尝试重连，形成竞争。
  **处置（两次实测有效）**：先确认编辑器不在 Play Mode（`editor_get_state` 的 `isPlaying`，
  必要时 `editor_stop`），**间隔 20–30 秒**再单独重跑该类——不要在同一批次里连跑多个进入 Play Mode 的类，
  那会把竞争窗口叠加起来。实测 `AutoEraStartupFlowEditModeTests` 与三个 PlayMode 套件逐个重跑后全部通过
  （6/6 与 3/3）。
  排查时另外确认过：`batch_state.json` 当时并无其它进程占用（可独占打开），系统里也没有残留的
  python/pwsh 进程——所以它不是「忘了关进程」，就是插件内部的写入竞争。

  2026-09-21 **补记（这条推翻了上面「基础设施问题就认了」的结论）**：插件报
  `Failed to reconnect test callbacks` + `totalTests=0` 时，测试**其实已经跑完并通过**。
  证据：同一时刻 Unity 把结果写进了
  `%USERPROFILE%/AppData/LocalLow/<公司>/<产品>/TestResults.xml`（本仓是
  `ZZNC/星际拓荒：自动纪元/TestResults.xml`），里面是
  `AutoEraStartupFlowEditModeTests result="Passed" total="6" passed="6" failed="0"`，
  六条用例的时间戳与本次运行完全对上（18:39:27Z → 18:41:27Z）。
  更进一步：插件是在 ~50 秒时放弃的，而测试还要再跑一分钟才结束——所以**「插件失败」早于
  「测试结束」**，两者根本不是一回事。
  处置：`tools/_unity_test.py` 在插件没拿到计数时，改为**轮询等待**该结果文件出现并等编辑器
  回到空闲，再以它作为结论（`source: TestResults.xml`），`totalTests=0` 不再直接当成失败。
  实测同一条命令现在返回 `6/6 passed (TestResults.xml)`。
  这条值得记住的原因是：它此前已经**两次**把绿色基线误判成红色，并让人去改代码或重启编辑器。
