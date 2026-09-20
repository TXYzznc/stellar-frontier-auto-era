# Tasks

## 1. 接入层骨架（阶段 1：一个域 + 一个样板页）

- [x] 1.1 新增 `AutoEraUiParamKeys` 键名常量与 `AutoEraUiSession` 会话句柄。
- [x] 1.2 `AutoEraUiFormBase` 增加 `TryGetSession` / `TryGetRequest` 与「本页不可用」呈现辅助。
- [x] 1.3 新增 `UiDataState` 与共享显示值类型（文本、数量、时间）。
- [x] 1.4 新增显示格式化服务（数量、世界时间、单位），界面不自行拼字符串。
- [x] 1.5 机器域读取模型（`IMachineReadModel` + 快照 + 实现），订阅 `MachineRoster.Changed`。
- [x] 1.6 样板页：`BaseCommandHubForm` 的「对象与系统」页接入机器域（列表用 GF 的 `SpawnItem<T>`、状态组映射）。
- [x] 1.7 样板页数据流测试：领域变化 → 界面更新 → 关闭退订。
- [x] 1.8 绑定字段基建：契约字段拆分为生成物 `<Form>.Fields.cs`（与手写 `<Form>.cs` 同属一个
      partial 类），33 个 Form 共 1291 个字段全部由契约产出；手写脚本不再声明任何 SerializeField。
      这一步只改 C# 归属不改结构：预制体是按契约重跑得出的，`Docs/Development/UI-PrefabLayouts/*.contract.json`
      仍是结构的唯一来源，门1 保持 33/33。

## 2. 领域就绪部分全量接入（阶段 2）

- [x] 2.1 存档域 + 启动存档三页、系统菜单、退出流程。
      新增存档域读模型 `ISaveSlotReadModel`（只读快照 + 选中 + 显式 `Refresh`）：存档域背后是
      文件系统、**没有领域推送**，所以不假装有事件流，与机器域的写法刻意不同。
      `AutoEraApplicationContext` 增加应用级 `SaveSlots`（根目录可注入，测试走临时目录不碰真实存档），
      `AutoEraUiSession` 把它交给界面。
      `SaveSlotsForm` 三页全部接入：槽位列表走对象池、详情页元数据/健康两栏、状态组按
      Ready/Empty/Unavailable 互斥。
      `SaveRecoveryForm` 接入：诊断栏陈述真实文件状态、候选栏列出可读内容，「恢复」只在**确有备份**
      时可点。`SaveSlotService` 新增 `HasBackup` 与 `RestoreFromBackup`——注意区分「能读出来」
      （`Read` 会回退读备份）与「已经修好」（备份被提升为主文件），只有后者才算恢复。
      `SystemMenuForm` / `ExitFlowForm` 转为手写接入（已加入生成器的 `HANDWRITTEN`，生成器继续供字段）。
      「返回主菜单」经应用上下文的意图标志交给世界流程消费：界面拿不到流程实例，也不引入全局定位器。
      诚实空态：读档、建档、保存退出都依赖尚未接入的世界进度层，这些入口一律**禁用并写明原因**，
      同时保证「返回游戏」永远可用。
      测试：`SaveSlotReadModelEditModeTests` 9/9（含损坏标记、从备份恢复、无备份不得伪造成功）；
      `AutoEraUiNavigationPlayModeTests` 扩展为「存档域 → 界面」端到端（先写两格真实存档再打开界面，
      断言数据状态为 Ready 且列表真的渲染出行，而不只是界面能打开）。
- [x] 2.2 世界会话注入：`AutoEraWorldProcedure` 打开 HUD 时携带会话句柄。
      `AutoEraMainMenuProcedure` 同步注入（该时刻世界会话为空，`session.HasWorld == false`，
      页面据此显示空态而非报错）。剩余缺口：`InitialRegionRuntimeEvidence` 的编辑器证据入口
      拿不到 application，需要 `AutoEraWorldSession` 暴露其所属 application 后补齐。
- [x] 2.3 机器域接入机器管理四页、机器库、机器整备、机器选择器。
      `UiMachineRow` 增加 `Deployed`：机器库的「库中机器」与「已部署机器」看的是**同一个花名册**，
      部署状态必须由行携带，页面只做过滤，不各自去问领域对象。
      `MachineLibraryForm`（3 页）接入：两页各自按部署状态渲染，空态用**本页过滤后**的条数判定
      ——规格要求单一分区为空不得抹掉其它有效分区，所以库中页为空时已部署页照常显示；
      整备页三栏因装配／载体／就绪数据未接入而明确说明。需要改装、库存与经济域的动作（安装／卸载／
      升级／出售／部署／改名／定位）全部禁用并写明原因。
      `FieldHudForm` 的四个机器现场页接入读模型：概况页的身份栏显示真实机器详情，其余栏位陈述原因。
      **删除 `FieldHudForm.BindMachines` 与 `InitialRegionScene` 的调用**——HUD 不再持有花名册，
      机器信息只经读模型；区域数据仍走 `BindRegion`（区域域，属 2.5）。
      现场选中复用「世界秒节拍 + 打开现场页」两个已有钩子同步到读模型：现场选中不产生领域事件，
      为它新增一条订阅机制不划算。
      测试：机器域 EditMode 8/8（新增「按部署状态分页」的过滤测试）；
      `AutoEraUiNavigationPlayModeTests` 再延伸一环：HUD 的「机器」入口 → 机器库，
      断言数据状态为 Ready 且库中机器页真的渲染出未部署的那台。
      说明：机器选择器（`ComponentPickerForm` 属组件域、`WorldObjectPickerForm` 属区域域）
      不在本项，随各自域在 2.5／2.7 处理。
- [x] 2.4 算法域接入算法工作台、模板库、待绑定、节点选择器（中枢规则页随 8.1 的缺口一并处理）。
      **发现并暴露一个真实缺口**：算法域的服务层是完整的（`AlgorithmInstanceService`、
      `AlgorithmTemplateLibrary`、`AlgorithmMachineAdapter` 都有实现），
      `AlgorithmServicesIntegrationTests` 甚至演示了完整链路——但**生产运行路径从未创建它们**：
      `new MachineExecutionContext(...)` 只出现在测试里。没有实例服务就没有草稿、节点库、
      问题清单、应用请求与待绑定项，因此界面能做的最诚实的事就是把这件事说明白。
      落地：新增算法域读模型（`IAlgorithmReadModel` + 快照 + 诚实空实现），接口已冻结；
      `AlgorithmReadModels.Create` 按「没有会话／没有世界／世界已就绪但域未接线」给出**三种不同的原因**，
      排查的人能直接看出缺的是哪一环。
      四个算法界面（编辑器、模板库、待绑定、节点选择器）加入 `HANDWRITTEN` 并接入读模型：
      整页切到 Disabled 并写明原因。新增基类辅助 `DisableDomainActions()`——它按结构名保留
      返回／关闭与 `Grp_Navigation` 下的页导航，把该域其余按钮全部禁用；这样不必为不可用的域
      声明几十个按钮绑定（那既臃肿又容易漏）。
      测试：`AlgorithmReadModelEditModeTests` 4/4（三种原因各自可辨，空实现的选中/刷新是 no-op）。
      **待决策见第 8.1 节**：算法域的运行路径接线需要确认生命周期归属。
- [x] 2.5 区域与传感域接入资源点四页、世界放置、世界对象选择器。
      - **会话携带区域**：`AutoEraUiSession` 新增 `Region` / `HasRegion`，`ForWorld` 增加可选区域参数，
        `AutoEraWorldProcedure` 在场景就绪后打开 HUD 时把区域一起交出去。区域是**场景级**对象
        （持有地形、放置体与导航），不属于世界会话，所以它作为可选句柄随会话传递；
        使用它的界面必须判 `InitialRegion.IsActive`——会话可能比区域活得久。
      - **区域域读模型** `IRegionReadModel`：区域对象列表（按机器／非机器分组）+ 选中对象详情。
        它订阅**三种变化源**：`ObjectsChanged`（增删）、`SelectionChanged`（选中）与每个
        `RegionObject.Changed`（公开状态）；订阅走**对账**而不是全量重建，否则每次刷新都会累积重复订阅。
        选中由区域自己持有（`Select` 带 inputBlocked 语义），读模型只读。
      - 测试 `RegionReadModelEditModeTests` 7/7：三种不可用原因、空区域是 Empty 而非 Unavailable、
        注册即出现、状态变化不让对象重复入列、选中只走选中区、释放后退订。
      - **HUD 现场内容页已接入**：四个资源观察页与八个建筑类型页走同一个区域域读模型。
        数据分配有明确规定——公开状态栏用区域详情；传感器栏、记录栏，以及建筑页的领域栏
        （生产／储能／施工进度／输送…）一律陈述原因。只有「建筑总览」的身份栏用区域详情，
        因为它本来就是身份信息；其余页的第一栏是具体领域数据，拿区域详情去填并不诚实。
        十二页的渲染放在 `FieldHudForm.RegionPages.cs`（partial），避免主文件被十几组参数淹没。
        `BindRegion` 保留为**兜底通道**：会话没带区域时（编辑器直接打开、测试装置）用它替换
        不可用的读模型，否则界面会一直声称自己没有区域而区域其实就在手上。
      - **世界对象选择器**：候选来自区域域读模型并按请求过滤（要不要机器／要不要建筑与资源点）。
        界面内部的选中**只影响预览**，不碰区域选中状态——区域选中是现场输入的事，选择器没有资格改它。
        结果写回调用方持有的 `AutoEraUiSelectionRequest`：确认／取消／被外部关掉三种结局调用方都能区分，
        因此不需要回调注册，也就没有「回调在界面关闭后才到达」的时序问题。
        预览用读模型新增的 `DescribeObject(id)`——它给出与选中详情**同一份字段**，
        从而不必让读模型交出领域对象。「在区域中直接拾取」需要光标→对象通道，未接入，故禁用并说明。
      - **世界放置**（建造放置／机器部署／世界绑定三页）呈现整页不可用：落位事务要同时改区域布局、
        机器绑定与世界状态，而生产中没有这条通道（`InitialRegion.CanPlace` 只做校验）。
        取消类按钮被**安全出口规则**保留——一个尚未接入的域不该把玩家关在里面；本 Form 没有
        `Btn_FormBack`，取消是唯一出口，所以 `DisableDomainActions` 的判据里加了「名字以 Cancel 结尾」。
      - 测试：区域域 8/8（含 `DescribeObject` 在未选中状态下可用）、选择请求 5/5。
      - `ComponentPickerForm` 不在本项：它依赖尚未接入的组件域，随 2.7 统一处理。
      - 传感域与算法域同病：`RegionSensorEnvironment` / `RegionSensorReadProvider` / `MachineSensorSet`
        只在测试里被创建，`InitialRegionScene.AttachSensors` 没有生产调用者。
- [x] 2.6 事件域接入记录阅读与中枢统计的只读部分。
      事件域**是活的**：世界会话创建时就建好事件服务与日志，所以这一域与算法／传感不同，真的读得到记录。
      `EventJournal` 原本只有 `Append`／`NextSequence`／`TryGetTrace`，没有枚举入口；新增
      `CopyRecent(List, maxCount)`——写入调用方列表且不清空它，界面每次刷新零分配。
      新增事件域读模型 `IEventReadModel`：日志是追加型环形缓冲、**没有变化事件**，所以只有显式
      `Refresh()`（与存档域同理，不假装有推送）。快照给出「全部记录」与三个展示分组：
      机器历史 = 任务域 + 执行域（机器做的事以这两类记账），算法历史 = 算法域，
      能源历史恒空并说明原因——**`EventDomain` 里根本没有能源域**（只有任务／算法／执行／资源／警报），
      且能源系统本身也未接入；这里不拿资源域冒充能源。
      接入：`RecordReaderForm` 三页（列表走对象池、点行选中并读追溯链；工具栏动作统一禁用，
      **但列表行不受影响**）；中枢统计页的记录栏用真实日志，指标栏与长期汇总栏陈述「统计聚合尚未接入」。
      配套：`DisableDomainActions` 再放宽一条判据——**`Item_*Template` 子树内的按钮不参与整域禁用**，
      它们是数据行而不是工具栏动作；否则「浏览只读记录」这类只需点行的页面会完全不可交互。
      测试：事件域 6/6（空日志是 Empty、三分组正确、资源域不进能源列表、选中能取到追溯链、
      未知序号被拒、刷新保留仍然存在的选中项）。
- [x] 2.7 未就绪域接成诚实空态（`Unavailable`），并跑通导航与返回链。
      「未接入」的判定标准是**该界面依赖的领域服务在生产里没有创建者**，而不是「界面没人写」。
      按此标准圈出 15 个界面：组件域（ComponentLibrary／ComponentPicker／Upgrade）、
      库存与交易（Warehouse／Shop）、建造与工坊（BuildCatalog／Workshop）、任务与警报（Quest／Alert）、
      用户设置（Settings）、作物知识与帮助系（CropKnowledge／Tutorial／FeatureHelp／Help／RuleHelp）。
      不在此列的三个参数化界面（OperationDialog／OperationFeedback／ProgressReport）由调用方传参驱动、
      不依赖任何领域服务，因此保持空骨架即可——它们不需要「未接入」这种状态。
      做法：让**生成器**按契约里的页面字段自动注入统一空态——每页一句
      `ShowPageUnavailable(NotWiredReason, …)`，加常量 `NotWiredReason`、
      `IsDomainWired => false` 与 `DisableDomainActions()`。好处有两个：15 个界面一次到位且口径一致；
      领域接入时只要把该 Form 从生成器的 `NOT_WIRED` 集合移走，生成器就不再注入，改为接读模型。
      回归围栏：`AutoEraNotWiredFormsEditModeTests` 16/16——15 个界面逐个断言「带原因、声明未接入、
      渲染空态、禁用本域动作」，另有一条断言已接入的界面**绝不**被注入这段脚手架
      （否则会出现「明明有数据却宣称没有」的反向错误）。
- [x] 2.8 删除与 `AutoEraUiSession` 并行的旧直通通道：`AutoEraUiRuntime.BindMachineRoster`
      的 `_machines`/`_hub` 分支在生产路径下永不赋值（只有测试后门在用），
      页面数据来源应收敛到读模型一条路。`FieldHudForm.BindMachines` 同理，随 HUD 读模型迁移一并处置。
      已完成部分：`AutoEraUiRuntime` 只留意图路由（不再持有任何领域数据）、`BaseCommandHubForm`
      交出 `_machines`/`BindMachines`、测试装置改为经 UIParams 注入内存世界会话（与生产同一条路）。
      已完成：`FieldHudForm.BindMachines` 与其在 `InitialRegionScene` 的调用随 2.3 的 HUD 读模型迁移
      一并删除，`FieldHudForm._machines` 字段同时移除、机器信息只经 `IMachineReadModel` 取。
      复核方式：全仓已无 `BindMachines` / `BindMachineRoster` 方法定义，只剩三处说明删除原因的
      历史注释（`AutoEraUiRuntime` / `FieldHudForm` / `BaseCommandHubFormTestSetup`）。
      HUD 现存的第二条数据路是 **区域域**的 `BindRegion`（属 2.5，是兜底通道而非旁路，见 2.5 说明）。

## 3. 导航、列表与本地化

- [x] 3.1 实现 `00-页面关系与复用` 的入口/返回表，使 33 个界面真正可达。
      框架已完成：新增 `AutoEraUiNavigator`（把来源界面的 `AutoEraUiSession` 透传给目标界面，
      统一装配打开参数）；覆盖/恢复交给 GF 的 UIGroup（按 UITable 的 `PauseCoveredUI` 自动
      Cover/Pause 与 Resume），返回时焦点由 `AutoEraUiFormBase` 记忆恢复；新增 `AutoEraUiPageRequest`
      让调用方指定目标界面的初始页（页索引用目标 Form 自己公开的常量）。
      已接通两条链并有运行期测试 `AutoEraUiNavigationPlayModeTests` 守着：
      主菜单 →（选择进度）存档槽列表 /（新游戏）新建进度页；世界 HUD →（中枢）基地中枢，
      并以「中枢显示出世界会话里的机器」证明会话确实透传到了界面。
      **本轮补齐入口表剩下的现场侧入口**（此前只有 8 个顶栏入口接了线，现场内容页的按钮全是死按钮）：
      - 「现场／HUD → 记录阅读」：机器诊断与四个资源观察页的 8 个记录入口 →
        `RecordReaderForm` 的机器历史页。事件域是唯一「世界会话一建好就已经活着」的域，
        所以这些入口点开就有真实记录，而不是只能陈述原因。
      - 「现场机器 → 算法编辑」：`Btn_MachineOverviewAlgorithm` / `Btn_MachineAlgorithmEdit` →
        `AlgorithmEditorForm`，`Btn_MachineAlgorithmTemplate` → `AlgorithmLibraryForm`。
        算法域没有生产运行路径，两个界面会整页切 Disabled 并写明原因——**仍然接线**，因为
        「未就绪」是设计里可展示的正常状态，死按钮不是（这是 2.x 一贯的口径）。
      - 「作物知识入口放农田详情」：四个资源观察页的知识入口 → `CropKnowledgeForm`。
      - 状态栏的「警报→活跃警报列表」→ `AlertForm`，「主线追踪→任务详情」→ `QuestForm`。
      落地方式：绑定写进生成器的 `EXTRA_BINDINGS`（FieldHud 从 8 个按钮升到 25 个），
      契约、`FieldHudForm.Fields.cs` 与预制体绑定全部由管线重出，结构零改动。
      11 个**已就绪**的界面因此全部可达；其余 15 个未就绪域界面按 2.7 打开即陈述原因。
      现状说明：`MainMenuForm`、`FieldHudForm` 由真实流程打开，其余界面由上述入口进入。
      回归（全绿）：编译 0 错、门1 33/33、`run_project_checks` 5/5、
      **EditMode 12/12 类 83/83**、**PlayMode 3/3**。导航 PlayMode 测试本轮延伸到
      「HUD → 记录阅读」：先往事件日志写一条事实，再断言记录阅读页真的渲染出日志行
      （只断言「界面能打开」不足以证明这条入口通了）。
      过程中一度出现「进入 Play Mode 的测试全部 `totalTests=0`」，判定为编辑器侧 Test Runner
      状态问题而非代码缺陷——重启编辑器后同一套测试全部通过，详见第 7 节。
- [x] 3.2 列表统一改用 GF 的 `SpawnItem<T>` 对象池 + `Item_*Template`。
      **审计结论：界面层的列表渲染已经全部走对象池**——`Instantiate`／`Destroy` 只出现在
      `AutoEraUiRuntime`（单例宿主）与 `AutoEraUiVisualTimer`（计时器宿主）里，与列表无关。
      顺手把重复了八处的「清空池 + 循环 Spawn + Bind」抽成基类 `RenderListRows`：
      这个模式最容易犯的错是忘记先清空池（旧行与新行叠加显示），集中之后不会再各漏一处。
      两个要点写进了方法注释：① 每列列表每次刷新只能调用它一次（内部会先清空）；
      ② 需要过滤时先过滤成索引列表再整体渲染，不要在循环里反复调用。
- [x] 3.3 冻结语言 key 约定并挂 `UIStringKey`（文案就绪后回填）。
      约定已冻结并落盘为 `Docs/Development/GF-UI-Standards/09-本地化Key约定.md`：
      `UI.<家族>.<页面>.<节点语义>`，三段全部可从契约**机械推导**（家族来自规格家族目录、
      页面来自 `Panel_Page<Id>`、节点语义来自节点名去掉 `Txt_<PageId>` 前缀）。
      **本轮刻意不执行挂载**，依据是技术事实：`UIFormBase.InitLocalization` 会对每个挂了
      `UIStringKey` 的文本无条件执行 `text = GF.Localization.GetString(Key)`；文案尚未录入时
      Key 为空，挂上去会在界面打开的一瞬间清空所有占位文案，而这种故障看起来像布局问题、
      排查成本远高于收益。因此挂载有明确前置条件（文案已入表），文档写清了「不该进表的三类文本」
      与四步挂载流程。附带一条界线：2.7 注入的 `NotWiredReason` 属开发期诊断文案，**不进本地化表**。

## 4. 无宿主组件处置

- [x] 4.1 逐个判定 `AutoEraUiCancelIntentProxy`、`AutoEraReduceMotionEntry`、`AutoEraDangerConfirmationView`、`AutoEraHoldToConfirmView`、`AutoEraUiVisualTimer`、`AutoEraHubPageSelection`、`MachineHardwarePresenter`：重接或明确废弃。
      判定准则与逐项结论已落盘 `design.md` 的「无宿主组件处置（4.1）」。
      **废弃 1 项**：`AutoEraHubPageSelection`（含 `AutoEraHubPage` 枚举）——职责已被
      `BaseCommandHubForm` 的 7 页页常量 + `AutoEraUiPageRequest` 取代，且其枚举只有 5 页而中枢实际 7 页，
      **语义已经错位**，保留会与新页序冲突；已删除源文件与对应 EditMode 用例。
      **保留 6 项**，其中 5 项的实际接线要等前置域就绪，这属正常而非缺陷：
      `MachineHardwarePresenter`（机器硬件界面）、`AutoEraDangerConfirmationView` + `AutoEraHoldToConfirmView`
      （危险确认宿主）、`AutoEraReduceMotionEntry`（设置界面）、`AutoEraUiVisualTimer`（自建宿主）、
      `AutoEraUiCancelIntentProxy`（宿主是**场景内 EventSystem 节点**——`AutoEraUiIntentInputAdapter` 只是纯提交接口，
      cancel 事件必须有人转发，故属场景配置而非代码创建）。
      回归：编译 0 错、门1 通过、EditMode 83/83、PlayMode 3/3、`run_project_checks` 5/5。

## 5. 边界

13 类领域系统（能源、库存、配方、工坊、升级、图纸、交易、交付、任务、警报、作物、离线结算、用户设置）的实现另立变更，按 `Docs/GameDesign/02-系统设计` 推进；每实现一类即回填对应界面的数据源。

本变更不修改界面规格、不重建预制体、不改 `UIViews` 登记。

## 6. 已清理的失配代码（留痕）
- 旧 `BaseCommandHubForm.prefab` / `FieldHudForm.prefab` 与对应的手搭 Form 脚本。
- `AutoEraStartupAssetBuilder` 的 `Fix Minimal Menu Contrast` 与 `Build Minimal Startup Assets`
  两个菜单：它们手工搭建 `MenuPanel` + `EnterButton` 示例层级并直写旧序列化字段，与契约管线冲突
  （层级已不存在，运行必 NRE）。保留 `Register Startup Scenes` 与新的
  `Register Startup Procedures`，并在预制体缺失时报错指向契约管线而不是就地补建。

## 7. 本轮修掉的框架级缺陷（留痕）

- `UIFormBase.SpawnItem<T>` 首次创建 Item 对象池时执行 `(IObjectPool<UIItemObject>)(object)pool`：
  `IObjectPool<T>` 不是协变接口，该转换在运行期必抛 `InvalidCastException`，导致
  **所有列表首次渲染立即中断**。实测表现是基地中枢的机器列表在帧内静默失败、界面只显示
  预制体里的静态占位文案——这正是「界面看起来接上了、其实没有数据」的典型形态。
  改法：句柄分两份保存，`ObjectPoolBase` 用于销毁（GF 有对应重载），闭包保留具体泛型用于 `UnspawnAll()`。
- `UIParams.Create(false)` 的第一个参数是 `AllowEscapeClose`，不是「是否子界面」。写 false 会压掉
  UITable 的 `EscapeClose` 登记值，使登记为 true 的界面无法用返回键/关闭按钮退出。
  三处打开点（导航服务、主菜单流程、世界流程）改为 `UIParams.Create()`，交由数据表兜底。
- `AutoEraMainMenuProcedure` / `AutoEraWorldProcedure` 的 `OnLeave` 直接 `CloseUIForm(_uiId)`：
  UI 组件可能已先一步关闭（整体退出、场景卸载、测试收尾），此时会抛
  `GameFrameworkException: Can not find UI form`。关闭前加存在性判断。
- `AutoEraUiNavigationPlayModeTests` 的三条测试纪律（写在测试类的注释里，勿删）：
  ① 两条链写在同一个 `[UnityTest]` 里——Launch 场景的产品流程对场景重载敏感；
  ② 收尾只关「本测试新增的」界面，否则会关掉流程持有的界面并让它抛异常；
  ③ `Logic` 挂上不等于 `OnOpen` 跑完，读界面内容前要等壳完成首次页选择。
- 工具链：Unity 在后台不会自动重编译，`tools/_unity_compile.py` 必须显式触发
  `Assets/Refresh`，并以「任意程序集比任意源码新」为判据（只看 console 会得到假绿）。
- 工具链：`run_project_checks.py` 的编译检查走 `debug_get_errors`，它会报**上一次编译的残留错误**。
  遇到「测试全部能跑、但项目检查说 compile errors」时先调 `console_clear` 再重跑，不要急着改代码。
- 工具链（本轮新增，都是踩出来的）：**给契约加绑定之后必须跑「按契约刷新所有页面绑定」**。
  `AutoEraUiPrefabGenerator` 原有的两个刷新/重建菜单只作用于 `DefaultContractPath`
  （BaseCommandHubForm），对别的 Form 跑它们会**静默地什么都不做**——预制体文件不改、控制台无报错，
  只有门1 的 L3 会指出「绑定为空」。已补对称菜单
  「`Game Framework/AutoEra/UI/按契约刷新所有页面绑定（不改结构）`」，并让
  `RefreshBindingsOnly` 返回未解析条数、按 33 份契约汇总打印。
- 工具链：**门1 的完整清单必须由 checker 自己落盘**。控制台消息经 unity-skills 摘要后会被截断
  （实测「未通过（34 项）」只回得到 3 行），`tools/_unity_gate1.py` 又用这段截断文本覆写了报告文件，
  于是失败原因被吞掉一半。改为 `AutoEraContractGate1Checker.RunFromMenu` 自己写
  `tools/_gate1_report.txt`，脚本只读不写。
- **纪律：EditMode 运行前必须确认编辑器不在 Play Mode**。Unity Test Runner 的第一步是
  `RestoreSceneSetupTask` → `EditorSceneManager.RestoreSceneManagerSetup`，该 API 在 Play Mode 中
  抛 `InvalidOperationException: This cannot be used during play mode`，于是整次运行在**任何测试执行之前**
  就以一句笼统的「An unexpected error happened while running tests」失败（`totalTests=0`），
  同时把编辑器继续留在 Play Mode——下一次运行也失败，形成自锁。
  `tools/_unity_test.py` 已加前置护栏（每个 EditMode 类之前检查并在必要时 `editor_stop`）。
- **环境问题（已由重启编辑器解决，留痕备查）**：编辑器连续运行约 11 小时后，Unity Test Runner
  对**会进入 Play Mode 的运行**失去恢复能力——PlayMode 三个套件与
  `AutoEraStartupFlowEditModeTests`（用 `EnterPlayMode`/`ExitPlayMode`）都在
  `totalTests=0` 下失败，报「The original Unity Test Runner job ... was not restored after domain reload」
  或「An unexpected error happened while running tests」；日志里能看到游戏确实进了 Play Mode
  并跑完了 Preload→MainMenu→InitialRegion，只是运行器不再持有这次运行。
  已排除的因素：测试发现缓存（重跑 `test_discover_start` 后该类仍在 1306 个 EditMode 测试里）、
  场景脏状态（活动场景 Launch、`isDirty=false`）、Play Mode 未退出（护栏已处理）、
  代码编译（0 错）。
  **判据**：同一时刻**不进入 Play Mode 的 EditMode 类全部正常**（`AutoEraUiOperationContractsEditModeTests`
  6/6 通过），据此定位为编辑器侧运行器状态而非代码缺陷。
  **结论**：重启 Unity 编辑器后，同一套测试全部通过——EditMode 12/12 类 83/83（含该类的 6/6）、
  PlayMode 3/3。所以遇到这类「整体 `totalTests=0` 且只有进 Play Mode 的测试受影响」的现象，
  先重启编辑器，不要去改业务代码。

## 8. 待决策（需要用户或设计确认）

### 8.1 算法域与机器执行上下文的运行路径接线

**现状**：`MachineExecutionContext`（含任务队列、算力池、传感器集）与 `AlgorithmInstanceService`
实现完整、有集成测试演示完整链路，但**没有任何生产代码创建它们**——
`new MachineExecutionContext(...)`、`new AlgorithmRuntime(...)`、`new AlgorithmMachineAdapter(...)`
只出现在 `Assets/Game/Tests/` 下。直接后果有两条：
① 机器在生产中没有任务队列与算力记账；
② 算法界面没有实例可显示（2.4 因此只能呈现「域未接线」）。

而 `AlgorithmMachineAdapter` 的构造需要 `InitialRegion` 与 `MachineNavigation`，两者都是**现场**对象，
所以接线的位置基本被限制在区域侧。

**需要确认的是生命周期归属**：

- **候选 A**：`InitialRegion.DeployMachine` 在部署成功时创建执行上下文与算法实例服务，
  撤收／区域释放时销毁。与集成测试演示的顺序一致（部署 → 建 context → 绑导航），
  且所需的两件现场对象就在手边。代价是「未部署的机器没有执行上下文」。
- **候选 B**：`AutoEraWorldSession` 为花名册里的每台机器惰性创建并缓存执行上下文。
  好处是机器无论在库中还是已部署都有运行时；代价是世界会话要长期持有一批依赖现场的对象，
  区域切换与存档的边界都更复杂。

**已决策（2026-09-20，用户确认）**：采用**候选 A**——区域部署时创建执行上下文与算法实例服务。

**但落点比候选描述更靠后，需要留意**：`InitialRegion.DeployMachine` 是**纯领域方法**
（只做空间与身份绑定，不接触 GameObject），而 `MachineExecutionContext` 要在构造之后
经 `RegionNavigation.Bind(context, view, settings, ...)` 绑定导航——其中的 `view` 是场景表现层对象。
所以准确的创建点不是 `DeployMachine` 内部，而是**部署完成后、表现层把导航绑上去的那一步**；
`AlgorithmMachineAdapter` 也正是要拿 `MachineNavigation` 与 `InitialRegion` 两样东西。
生产代码目前没有这条路径（`RegionNavigation.Bind` 只在测试里被调用）。

因此建议**另立一个领域集成变更**来做这件事（它要决定区域视图、导航绑定与执行上下文的
创建／销毁顺序，并考虑存档是否携带运行时），本变更只保证：算法界面在接线之前呈现
「诚实不可用」，且接口已冻结——接线完成后只需替换 `AlgorithmReadModels.Create` 的分支，
四个算法界面无需改动。
