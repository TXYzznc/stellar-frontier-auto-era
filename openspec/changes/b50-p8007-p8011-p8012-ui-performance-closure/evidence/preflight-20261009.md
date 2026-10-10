# B50 实施前核验

2026-10-09。沿用已确认角色、conventions、OpenSpec与Unity技能规范，主工程Unity2022.3.62f3c1/8091，新增结构非Play Mode普通编译，无子agent、Git索引/提交或工作簿写入。

B40～B46已有各自原生证据，B48独立核心176项、B49合同91项和五类健康门通过。B47正式开局、B48完整领域、B49完整离线及G7/P8-001/P6文案前置未交付；B50整批/G8不得提前通过。局部现有UI故障、只读结构盘点、独立生命周期测试及证据整理可继续。

第一执行单元精确允许：Assets/Game/Scripts/AutoEra/UI/AutoEraUiNavigator.cs、AutoEraUiRuntime.cs；Assets/Game/Tests/AutoEra/Editor/UiLifetimeStressPlayModeTests.cs、UiStructureContractEditModeTests.cs；本change/evidence及必要Temp测试编排脚本。先记录“首次异步打开即取消”的实际失败，再修复产品导航，不修改GF库、ScriptsBuiltin、通用UIExtension、asmdef、生成字段或Prefab。其他代码/资产修改须先记录真实热点与精确清单。

只运行1920×1080；同时独立核对5个既有大页的根Stretch/offset、统一GF缩放及语义容器。不以Editor耗时或静态节点数代替Development Player预算，不依据节点数量机械拆页。50台60分钟及完整10/50/100生产负载须真实前置交付后验收。

快速执行候选检查：异步取消、参数所有权、GF原生加载及共享Unity现场包含未冻结的专业判断且不能安全拆交；主对话实施并保留复核。测试使用AutoEraB50Ui-随机前缀临时目录，删除前核对绝对父目录及前缀。

子页增量精确允许：Assets/Game/Scripts/AutoEra/UI/BaseCommandHubForm.cs、FieldHudDetailForm.cs、AlgorithmEditorForm.cs，仅将既有OpenSubUIForm调用转接产品层参数所有权跟踪，保留GF子Form排序及父关闭机制。实际原生失败f94d32f4证实父窗取消冷加载子页后UIParams占用0→1，修复前保留XML。不改通用UIFormBase/GF库。

设置页增量允许：Assets/Game/Scripts/AutoEra/UI/SettingsForm.cs；Assets/Game/Scripts/AutoEra/Editor/UiProto/AutoEraUiPrefabGenerator.cs；Assets/Game/Prefabs/UI/System/SettingsForm.prefab；Docs/Development/UI-PrefabLayouts/SettingsForm.contract.json；Docs/GameDesign/03-玩家体验/界面规格/02-系统与设置/prefab-layout.md。截图与序列化检查证实正常态Success卡遮挡、8个Slider填充/柄引用为空、柄颜色近黑且值标签保持占位。沿用既有几何与蓝色/白色配色，补齐本地控件接线及标签；不新增布局/业务/平衡设计，不编辑Fields或手写YAML。由主对话执行：需共享Unity资产现场和视觉复核，交接成本不低于本地完成。

视觉增量：原生首轮修复截图仍显示Slider纵向锚点导致图形拉伸，合同补16个专用FillArea/HandleArea（8/24高）并保持680×56根几何；填充改Simple，由Slider驱动横向尺寸。水平/垂直反转2个Toggle的graphic/targetGraphic也为空，按已有Box/Check节点补引用与同一蓝/白配色。此范围仍限上述Settings合同/代码/生成器/单一Prefab。回归增加原生Rect尺寸/填充比例和Toggle图形显隐检查，不写本机设置。首轮通过80a074a3仅为生命周期证据，不代表最终视觉通过。

工具/文档增量允许：tools/audit_ui_prefabs.py、tools/run_project_checks.py（仅修正检查标签固定8092，与实际--port一致）、Docs/Development/UI-PrefabLayouts/UIOptimizationAudit.md及既有Impact列出的交接/检查说明。只读盘点必须将节点数标为实测候选，识别已有AutoEraUIDialogFormBase并保留历史实施段，不改变任何玩法/资产。当前主工程现场8091；修复创建临时节点产生Launch dirty标记，文件无diff，根对象仍为GameFramework/UICamera/EventSystem，恢复同一已保存场景后重跑。

导航增量允许：Assets/Game/Scripts/AutoEra/UI/FieldHudResidentForm.cs，仅为既有Btn_HudNavigationHub/Btn_HudNavigationMachines绑定AutoEraUiNavigator及既有目标Form。原生d7001716失败表明HUD基地中枢按钮后300帧未打开，源代码只有按钮缓存/公开Getter，没有导航监听；不属于正式开局领域缺失，不以依赖阻塞掩盖。既有AutoEraUiNavigationPlayModeTests覆盖HUD→中枢/机器库的真实会话透传。两按钮OnInit仅注册一次，不更改Prefab/生成字段或宣称完整常驻HUD/正式新建闭环。

测试契约增量允许：Assets/Game/Tests/AutoEra/PlayMode/AutoEraUiNavigationPlayModeTests.cs，仅将已抽出机器总览页的诊断按钮断言迁到真实加载的FieldHudMachineOverviewForm；保留HUD→中枢/机器库、会话透传、记录数据等原断言。1cb72af5已证明两HUD入口成功，后续失败为父窗过时按钮归属；修复后不得删断言换取通过。

契约门增量允许：Assets/Game/Scripts/AutoEra/Editor/UiProto/AutoEraContractGate1Checker.cs。实际门1报告26项：Settings新增20项（10个被动Img拦截输入、10个驱动锚点固定比较）和Warehouse既有6项L5尺寸声明。Settings修复后新增项清零；Warehouse6项保持基线归属，不称全量门1通过。将输入命中移到Sld_/Tgl_根透明Image，被动Img raycast=false；根图形加入合同。Checker只在真实Slider.fillRect/handleRect引用一致时跳过其驱动锚点，仍校验pivot/sizeDelta/position，声明slider但没接线必须报错。不是全局跳过Rect检查；新原生回归还验证根命中区、真实填充尺寸及开关显隐。保存失败报告为gate1-before-driver-support.txt。
