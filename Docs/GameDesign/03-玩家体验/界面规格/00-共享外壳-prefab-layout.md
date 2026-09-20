# 共享外壳 — prefab-layout

状态：2026-09-19设计候选。与18家族子树拼接形成完整Form结构；本文件只声明公共外壳一次。

所有节点采用[通用合同](00-通用合同.md)的组件与资源默认值。每个Form节点唯一；同一通用结构在不同Form重复不构成同Form重名。下表每一行对应一个真实预制节点，根名使用具体Form名称替换。

## 普通管理／模态外壳

```text
<FormName>
  Bg_InputBlocker
  Panel_Frame
    Deco_Frame
    Txt_FormTitle
    Grp_Navigation
      Btn_<Form语义><分页语义>
        Txt_<Form语义><分页语义>Label
    Grp_PageHost
      （引入所属页面的Panel_Page*，见下方清单）
    Btn_FormBack
      Txt_FormBackLabel
    Btn_FormClose
      Txt_FormCloseLabel
```

| 节点 | 组件 | anchorMin→Max / pivot / sizeDelta / anchoredPosition |
|---|---|---|
| FormName | 项目Form脚本＋CanvasGroup；无Canvas系列组件 | (0,0)→(1,1) / (.5,.5) / (0,0) / (0,0) |
| Bg_InputBlocker | Image，透明遮罩raycast=true | (0,0)→(1,1) / (.5,.5) / (0,0) / (0,0) |
| Panel_Frame | Image，Surface，raycast=true | (.5,.5)→(.5,.5) / (.5,.5) / (W,H) / (0,0)，中枢pos=(0,-12) |
| Deco_Frame | Image，raycast=false，Sliced | (0,0)→(1,1) / (.5,.5) / (0,0) / (0,0) |
| Txt_FormTitle | TMP，标题28 | (0,1)→(0,1) / (0,1) / (W-300,40) / (56,-20) |
| Grp_Navigation | HorizontalLayoutGroup spacing=8 padding=0；无Graphic | (0,1)→(1,1) / (.5,1) / (-200,52) / (-44,-68) |
| Btn_分页 | Button＋自身Image＋LayoutElement | (0,1)→(0,1) / (0,1) / (136,48)初始化 / (0,0)初始化；preferred=(136,48)，由组布局 |
| Txt_分页Label | TMP，按钮22；raycast=false | (0,0)→(1,1) / (.5,.5) / (-16,-8) / (0,0) |
| Grp_PageHost | RectTransform，无Graphic | (0,1)→(0,1) / (0,1) / (W-112,H-170) / (56,-140) |
| Btn_FormBack | Button＋Image | (1,1)→(1,1) / (1,1) / (88,48) / (-112,-20) |
| Txt_FormBackLabel | TMP，返回 | (0,0)→(1,1) / (.5,.5) / (-8,-8) / (0,0) |
| Btn_FormClose | Button＋Image，导航最后 | (1,1)→(1,1) / (1,1) / (80,48) / (-24,-20) |
| Txt_FormCloseLabel | TMP，关闭 | (0,0)→(1,1) / (.5,.5) / (-8,-8) / (0,0) |

无分页的Form中Grp_Navigation为空分组，隐藏不占焦点；不添加假分页按钮。返回退子页，关闭退整个Form；有未提交草稿时两者都经过离开处理。加载、结算与安全写入阶段按业务许可禁用返回和关闭并说明；主菜单关闭走退出确认。

## 每个Form的尺寸、分页及内容组合

### MainMenuForm

外壳W=1120、H=820；Grp_PageHost=1008×650。

子页组合：[主菜单](01-启动与存档/MainMenu.md)。

### SaveSlotsForm

外壳W=1600、H=900；Grp_PageHost=1488×730。

子页组合：[存档槽列表](01-启动与存档/SaveSlots.md)、[存档详情](01-启动与存档/SaveDetail.md)、[新建进度](01-启动与存档/NewProgress.md)。

### SaveRecoveryForm

外壳W=960、H=800；Grp_PageHost=848×630。

子页组合：[损坏存档恢复](01-启动与存档/Recovery.md)。

### SystemMenuForm

外壳W=800、H=840；Grp_PageHost=688×670。

子页组合：[游戏内系统菜单](02-系统与设置/SystemMenu.md)。

### SettingsForm

外壳W=1600、H=900；Grp_PageHost=1488×730。

| 导航节点（另有同Key的Txt_*Label子节点） | 标签 | 目标 |
|---|---|---|
| Btn_SettingsAudio | 声音 | AudioSettings |
| Btn_SettingsControl | 操作 | ControlSettings |
| Btn_SettingsDisplay | 显示与性能 | DisplaySettings |

子页组合：[声音设置](02-系统与设置/AudioSettings.md)、[操作设置](02-系统与设置/ControlSettings.md)、[显示与性能设置](02-系统与设置/DisplaySettings.md)。

### ExitFlowForm

外壳W=960、H=800；Grp_PageHost=848×630。

子页组合：[保存返回与退出失败处理](02-系统与设置/ExitFlow.md)。

### FieldHudForm

全屏根，不使用上述Panel_Frame或Bg_InputBlocker；仅根→Grp_PageHost全Stretch，下面挂本Form的全部内容区，点击空世界仍交给世界输入。

子页组合：[顶部状态栏](03-世界HUD/HudStatus.md)、[任务追踪与引导入口](03-世界HUD/HudTracker.md)、[警报摘要](03-世界HUD/HudAlerts.md)、[底部功能入口栏](03-世界HUD/HudNavigation.md)、[保存状态提示](03-世界HUD/HudSave.md)、[机器现场概况](05-机器管理/MachineOverview.md)、[机器硬件](05-机器管理/MachineHardware.md)、[机器算法](05-机器管理/MachineAlgorithm.md)、[机器任务与诊断](05-机器管理/MachineDiagnostics.md)、[农田详情](06-资源点观察/Farm.md)、[人工林详情](06-资源点观察/Forest.md)、[地表矿脉详情](06-资源点观察/Mineral.md)、[水域详情](06-资源点观察/Water.md)、[关联传感数据与资源记录](06-资源点观察/SensorRecords.md)、[通用建筑现场面板](07-建筑设备与施工/BuildingOverview.md)、[水泵](07-建筑设备与施工/Pump.md)、[生物质发电机](07-建筑设备与施工/Generator.md)、[初始太阳能设施](07-建筑设备与施工/Solar.md)、[蓄电池](07-建筑设备与施工/Battery.md)、[仓库现场概要](07-建筑设备与施工/WarehouseBuilding.md)、[施工建筑](07-建筑设备与施工/Construction.md)、[传送带设备详情](07-建筑设备与施工/Conveyor.md)。

HUD五模块同时常驻；现场内容Panel_Page*只激活一页。侧栏标题预留右80px；Grp_PageHost另有Btn_FieldClose（Button＋Image，右上锚点/pivot=(1,1)，size=(64,48)，pos=(-40,-128)）及Txt_FieldCloseLabel（Stretch，delta=(-8,-8)，关闭）。关闭按钮只在侧栏开启时显示，导航最后；侧栏存在时追踪改左上pos=(24,-128)、anchor/pivot=(0,1)，保存模块改左下pos=(24,120)、anchor/pivot=(0,0)，警报仍居中。无侧栏恢复右侧默认锚点。

### BaseCommandHubForm

外壳W=1600、H=900；Grp_PageHost=1488×730。

| 导航节点（另有同Key的Txt_*Label子节点） | 标签 | 目标 |
|---|---|---|
| Btn_BaseCommandHubOverview | 总览 | HubOverview |
| Btn_BaseCommandHubTasks | 待处理任务 | HubTasks |
| Btn_BaseCommandHubObjects | 对象与系统 | HubObjects |
| Btn_BaseCommandHubRules | 规则自动化 | HubRules |
| Btn_BaseCommandHubStatistics | 统计 | HubStats |

子页组合：[总览](04-基地中枢/HubOverview.md)、[待处理任务](04-基地中枢/HubTasks.md)、[对象与系统](04-基地中枢/HubObjects.md)、[能源系统详情](04-基地中枢/HubEnergy.md)、[规则自动化](04-基地中枢/HubRules.md)、[统计](04-基地中枢/HubStats.md)、[中枢机器详情](05-机器管理/RemoteMachine.md)。

继承现有1600×900终端尺寸、整体pos=(0,-12)和1488×730内容面积；保留象牙白外壳。上述顶栏内部重排为候选改动，旧资源条的人员／食物占位退役，不另外增加第六导航。五页共享同一个退出路径。

### MachineLibraryForm

外壳W=1600、H=900；Grp_PageHost=1488×730。

| 导航节点（另有同Key的Txt_*Label子节点） | 标签 | 目标 |
|---|---|---|
| Btn_MachineLibraryUndeployed | 未部署 | UndeployedMachines |
| Btn_MachineLibraryDeployed | 已部署 | DeployedMachines |

子页组合：[未部署机器整备](05-机器管理/MachinePreparation.md)、[未部署机器](09-资产与模板目录/UndeployedMachines.md)、[已部署机器](09-资产与模板目录/DeployedMachines.md)。

### WorkshopForm

外壳W=1600、H=900；Grp_PageHost=1488×730。

子页组合：[制造工坊工作台](08-制造工坊/Workshop.md)、[配方详情](08-制造工坊/RecipeDetail.md)、[当前制造与等待队列](08-制造工坊/ManufacturingQueue.md)、[输出缓存与阻塞](08-制造工坊/OutputCache.md)。

### ComponentLibraryForm

外壳W=1600、H=900；Grp_PageHost=1488×730。

| 导航节点（另有同Key的Txt_*Label子节点） | 标签 | 目标 |
|---|---|---|
| Btn_ComponentLibraryLoose | 未安装 | LooseComponents |
| Btn_ComponentLibraryInstalled | 已安装 | InstalledComponents |

子页组合：[未安装组件](09-资产与模板目录/LooseComponents.md)、[已安装组件](09-资产与模板目录/InstalledComponents.md)、[组件详情与比较](09-资产与模板目录/ComponentDetail.md)。

### WarehouseForm

外壳W=1600、H=900；Grp_PageHost=1488×730。

子页组合：[仓库分类库存](09-资产与模板目录/Inventory.md)、[库存物品详情](09-资产与模板目录/ItemDetail.md)、[仓库记录](09-资产与模板目录/WarehouseRecords.md)。

### AlgorithmLibraryForm

外壳W=1600、H=900；Grp_PageHost=1488×730。

| 导航节点（另有同Key的Txt_*Label子节点） | 标签 | 目标 |
|---|---|---|
| Btn_AlgorithmLibrarySystem | 系统模板 | SystemTemplates |
| Btn_AlgorithmLibraryPlayer | 玩家模板 | PlayerTemplates |

子页组合：[系统算法模板](09-资产与模板目录/SystemTemplates.md)、[玩家算法模板](09-资产与模板目录/PlayerTemplates.md)、[算法模板详情](09-资产与模板目录/TemplateDetail.md)。

### ShopForm

外壳W=1600、H=900；Grp_PageHost=1488×730。

| 导航节点（另有同Key的Txt_*Label子节点） | 标签 | 目标 |
|---|---|---|
| Btn_ShopBuy | 购买 | ShopBuy |
| Btn_ShopSell | 出售 | ShopSell |

子页组合：[商店购买](10-商店与建造目录/ShopBuy.md)、[商店出售](10-商店与建造目录/ShopSell.md)、[商品与礼包详情](10-商店与建造目录/OfferDetail.md)。

### BuildCatalogForm

外壳W=1600、H=900；Grp_PageHost=1488×730。

子页组合：[建筑图纸目录与详情](10-商店与建造目录/Blueprints.md)。

### UpgradeForm

外壳W=1600、H=900；Grp_PageHost=1488×730。

子页组合：[载体升级](11-升级与改装/CarrierUpgrade.md)、[组件升级](11-升级与改装/ComponentUpgrade.md)、[改装影响预览](11-升级与改装/ModificationImpact.md)。

### ComponentPickerForm

外壳W=960、H=800；Grp_PageHost=848×630。

子页组合：[安装替换组件选择器](12-选择与绑定/ComponentPicker.md)。

### AlgorithmBindingForm

外壳W=1600、H=900；Grp_PageHost=1488×730。

子页组合：[模板集中待绑定](12-选择与绑定/PendingBindings.md)。

### NodeComponentPickerForm

外壳W=960、H=800；Grp_PageHost=848×630。

子页组合：[节点组件选择器](12-选择与绑定/NodeComponentPicker.md)。

### WorldObjectPickerForm

外壳W=960、H=800；Grp_PageHost=848×630。

子页组合：[世界对象选择器](12-选择与绑定/WorldObjectPicker.md)。

### AlgorithmEditorForm

外壳W=1840、H=1000；Grp_PageHost=1728×830。

| 导航节点（另有同Key的Txt_*Label子节点） | 标签 | 目标 |
|---|---|---|
| Btn_AlgorithmEditorEdit | 编辑 | AlgorithmEditor |
| Btn_AlgorithmEditorDiagnosis | 诊断 | AlgorithmDiagnosis |

子页组合：[算法编辑模式](13-算法工作台/AlgorithmEditor.md)、[算法诊断模式](13-算法工作台/AlgorithmDiagnosis.md)、[算法公开参数](13-算法工作台/PublicParameters.md)。

### WorldPlacementForm

全屏根，不使用上述Panel_Frame或Bg_InputBlocker；仅根→Grp_PageHost全Stretch，下面挂本Form的全部内容区，点击空世界仍交给世界输入。

子页组合：[建筑放置](14-世界放置与定位/BuildPlacement.md)、[机器部署](14-世界放置与定位/MachineDeployment.md)、[世界对象选择与定位辅助](14-世界放置与定位/WorldBinding.md)。

放置条之外不设遮罩；世界虚影属于场景预览，不是UI Image；同刻仅建造／部署／对象选择一个模式。进入放置隐藏HUD底部导航，保留顶部状态与警报；退出恢复。

### QuestForm

外壳W=1600、H=900；Grp_PageHost=1488×730。

| 导航节点（另有同Key的Txt_*Label子节点） | 标签 | 目标 |
|---|---|---|
| Btn_QuestActive | 进行中 | Quests |
| Btn_QuestClaimable | 可领取 | Quests |
| Btn_QuestCompleted | 已完成 | Quests |

子页组合：[主线任务](15-任务与记录/Quests.md)、[任务奖励与解锁详情](15-任务与记录/QuestRewards.md)。

### AlertForm

外壳W=1600、H=900；Grp_PageHost=1488×730。

子页组合：[警报列表与详情](15-任务与记录/Alerts.md)。

### RecordReaderForm

外壳W=1600、H=900；Grp_PageHost=1488×730。

子页组合：[机器任务历史](15-任务与记录/MachineHistory.md)、[算法运行记录](15-任务与记录/AlgorithmHistory.md)、[能源停机记录](15-任务与记录/EnergyHistory.md)。

### TutorialForm

外壳W=1600、H=900；Grp_PageHost=1488×730。

子页组合：[基础引导](16-教学规则与知识/Tutorial.md)。

### FeatureHelpForm

外壳W=1600、H=900；Grp_PageHost=1488×730。

子页组合：[新功能说明](16-教学规则与知识/FeatureHelp.md)。

### HelpForm

外壳W=1600、H=900；Grp_PageHost=1488×730。

子页组合：[帮助](16-教学规则与知识/Help.md)。

### RuleHelpForm

外壳W=960、H=800；Grp_PageHost=848×630。

子页组合：[通用规则说明](16-教学规则与知识/RuleHelp.md)。

### CropKnowledgeForm

外壳W=1600、H=900；Grp_PageHost=1488×730。

子页组合：[作物图鉴详情](16-教学规则与知识/CropKnowledge.md)。

### OperationDialogForm

外壳W=960、H=800；Grp_PageHost=848×630。

子页组合：[重命名](17-操作确认与输入/Rename.md)、[普通操作确认](17-操作确认与输入/BasicConfirm.md)、[交易确认](17-操作确认与输入/TransactionConfirm.md)、[升级确认](17-操作确认与输入/UpgradeConfirm.md)、[建造与部署确认](17-操作确认与输入/PlacementConfirm.md)、[施工制造取消与退款](17-操作确认与输入/CancelProduction.md)、[硬件修改确认](17-操作确认与输入/HardwareConfirm.md)、[机器回收确认](17-操作确认与输入/MachineRecovery.md)、[算法应用确认](17-操作确认与输入/AlgorithmApply.md)、[删除覆盖与算法重置](17-操作确认与输入/StrongConfirm.md)、[算法草稿离开](17-操作确认与输入/DraftExit.md)、[未保存强退确认](17-操作确认与输入/ForceExit.md)、[显示模式保留恢复](17-操作确认与输入/DisplayKeep.md)。

### ProgressReportForm

外壳W=1280、H=900；Grp_PageHost=1168×730。

子页组合：[加载](18-进度结果与报告/Loading.md)、[离线结算](18-进度结果与报告/OfflineSettlement.md)、[回归报告](18-进度结果与报告/ReturnReport.md)、[备份恢复进度与结果](18-进度结果与报告/BackupProgress.md)、[合并交付结果](18-进度结果与报告/Delivery.md)、[成长与功能解锁](18-进度结果与报告/Growth.md)、[星拓联日常补给](18-进度结果与报告/DailySupply.md)、[第一版完成报告](18-进度结果与报告/VersionComplete.md)。

### OperationFeedbackForm

外壳W=960、H=800；Grp_PageHost=848×630。

子页组合：[安全停止等待](18-进度结果与报告/SafeWait.md)、[成功拒绝与重试反馈](18-进度结果与报告/ActionResult.md)。

## 资源与默认状态

Bg为中性遮罩；Panel为Surface；Deco_Frame只有中枢绑定已有c01-frame，其它Form用中性占位，待原型后用户处理效果图。所有标签从本地化读取。Modal首焦点安全取消、关闭按钮最后；同组只激活一个顶层管理Form，选择／规则／确认走模态组。UIGroup与资源地址待实际接入在GF配置统一登记，本批不猜测UIViews编号。
