# 03-世界HUD — prefab-layout

状态：2026-09-19完整设计候选，待用户集中验收；未据此修改Prefab。

继承[通用合同](../00-通用合同.md)和[共享外壳](../00-共享外壳-prefab-layout.md)。下列子树预制在所属Form的Grp_PageHost中；公共外壳由共享文档唯一声明。跨家族同属一个Form的子树合并，禁止重复根节点。表中所有Image／文本颜色、资源、射线、默认字号与状态继承通用合同，列出的数据是字段说明，不是示例业务数值。

## HudStatus：顶部状态栏

功能文档：[顶部状态栏](HudStatus.md)；归属 `FieldHudForm`；内容 1872×96。

```text
Panel_PageHudStatus [Image]
  Grp_HudStatusContent [HorizontalLayoutGroup spacing=8 padding=0 childControlWidth/Height=true]
    Txt_HudStatusResources [TextMeshProUGUI + LayoutElement]
    Txt_HudStatusSystems [TextMeshProUGUI + LayoutElement]
  Grp_HudStatusActions [HorizontalLayoutGroup spacing=8 childControlWidth/Height=true]
    Btn_HudStatusGrowth [Button + Image]
      Txt_HudStatusGrowthLabel [TextMeshProUGUI]
    Btn_HudStatusCompute [Button + Image]
      Txt_HudStatusComputeLabel [TextMeshProUGUI]
    Btn_HudStatusEnergy [Button + Image]
      Txt_HudStatusEnergyLabel [TextMeshProUGUI]
    Btn_HudStatusResource [Button + Image]
      Txt_HudStatusResourceLabel [TextMeshProUGUI]
  Txt_HudStatusStatus [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageHudStatus | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-48,96); pos(0,-16) | absolute | Image；顶部状态栏；内部页面根 |
| Grp_HudStatusContent | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-16,36); pos(0,-4) | absolute | HorizontalLayoutGroup spacing=8 padding=0 childControlWidth/Height=true；模块文本行 |
| Txt_HudStatusResources | min(0,1) max(0,1); pivot(0,1); sizeDelta(928,36)初始化; pos(0,0)初始化; LayoutElement preferred(928,36); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement；资源：— |
| Txt_HudStatusSystems | min(0,1) max(0,1); pivot(0,1); sizeDelta(928,36)初始化; pos(0,0)初始化; LayoutElement preferred(928,36); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement；系统摘要：— |
| Grp_HudStatusActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,8) | absolute | HorizontalLayoutGroup spacing=8 childControlWidth/Height=true；入口操作区 |
| Btn_HudStatusGrowth | min(0,1) max(0,1); pivot(0,1); sizeDelta(456,48)初始化; pos(0,0)初始化; LayoutElement preferred(456,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开18-成长解锁的只读说明 |
| Txt_HudStatusGrowthLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看成长 |
| Btn_HudStatusCompute | min(0,1) max(0,1); pivot(0,1); sizeDelta(456,48)初始化; pos(0,0)初始化; LayoutElement preferred(456,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开04-对象与系统并按高负载机器筛选 |
| Txt_HudStatusComputeLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看算力对象 |
| Btn_HudStatusEnergy | min(0,1) max(0,1); pivot(0,1); sizeDelta(456,48)初始化; pos(0,0)初始化; LayoutElement preferred(456,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开04-能源系统详情 |
| Txt_HudStatusEnergyLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看能源 |
| Btn_HudStatusResource | min(0,1) max(0,1); pivot(0,1); sizeDelta(456,48)初始化; pos(0,0)初始化; LayoutElement preferred(456,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开04-统计并选中对应资源 |
| Txt_HudStatusResourceLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看资源详情 |
| Txt_HudStatusStatus | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-16,36); pos(0,-4) | absolute | TextMeshProUGUI；无数据／保存失败等；仅替代对应文本区显示，正常隐藏 |

## HudTracker：任务追踪与引导入口

功能文档：[任务追踪与引导入口](HudTracker.md)；归属 `FieldHudForm`；内容 380×240。

```text
Panel_PageHudTracker [Image]
  Grp_HudTrackerContent [HorizontalLayoutGroup spacing=8 padding=0 childControlWidth/Height=true]
    Txt_HudTrackerObjective [TextMeshProUGUI + LayoutElement]
    Txt_HudTrackerGuide [TextMeshProUGUI + LayoutElement]
  Grp_HudTrackerActions [HorizontalLayoutGroup spacing=8 childControlWidth/Height=true]
    Btn_HudTrackerTask [Button + Image]
      Txt_HudTrackerTaskLabel [TextMeshProUGUI]
    Btn_HudTrackerGuide [Button + Image]
      Txt_HudTrackerGuideLabel [TextMeshProUGUI]
    Btn_HudTrackerLocate [Button + Image]
      Txt_HudTrackerLocateLabel [TextMeshProUGUI]
  Txt_HudTrackerStatus [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageHudTracker | min(1,1) max(1,1); pivot(1,1); sizeDelta(380,240); pos(-24,-128) | absolute | Image；任务追踪与引导入口；内部页面根 |
| Grp_HudTrackerContent | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-16,180); pos(0,-4) | absolute | HorizontalLayoutGroup spacing=8 padding=0 childControlWidth/Height=true；模块文本行 |
| Txt_HudTrackerObjective | min(0,1) max(0,1); pivot(0,1); sizeDelta(182,180)初始化; pos(0,0)初始化; LayoutElement preferred(182,180); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement；当前目标：— |
| Txt_HudTrackerGuide | min(0,1) max(0,1); pivot(0,1); sizeDelta(182,180)初始化; pos(0,0)初始化; LayoutElement preferred(182,180); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement；基础引导：— |
| Grp_HudTrackerActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,8) | absolute | HorizontalLayoutGroup spacing=8 childControlWidth/Height=true；入口操作区 |
| Btn_HudTrackerTask | min(0,1) max(0,1); pivot(0,1); sizeDelta(113,48)初始化; pos(0,0)初始化; LayoutElement preferred(113,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开15-主线任务并选中追踪任务 |
| Txt_HudTrackerTaskLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；打开任务 |
| Btn_HudTrackerGuide | min(0,1) max(0,1); pivot(0,1); sizeDelta(113,48)初始化; pos(0,0)初始化; LayoutElement preferred(113,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开16-基础引导当前步骤 |
| Txt_HudTrackerGuideLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；打开基础引导 |
| Btn_HudTrackerLocate | min(0,1) max(0,1); pivot(0,1); sizeDelta(113,48)初始化; pos(0,0)初始化; LayoutElement preferred(113,48); 最终位置/尺寸由组驱动 | group | Button + Image；目标存在且已开放时定位；不能代替配置购买等玩家决策 |
| Txt_HudTrackerLocateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；定位目标 |
| Txt_HudTrackerStatus | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-16,180); pos(0,-4) | absolute | TextMeshProUGUI；无数据／保存失败等；仅替代对应文本区显示，正常隐藏 |

## HudAlerts：警报摘要

功能文档：[警报摘要](HudAlerts.md)；归属 `FieldHudForm`；内容 600×144。

```text
Panel_PageHudAlerts [Image]
  Grp_HudAlertsContent [HorizontalLayoutGroup spacing=8 padding=0 childControlWidth/Height=true]
    Txt_HudAlertsHighest [TextMeshProUGUI + LayoutElement]
    Txt_HudAlertsChanges [TextMeshProUGUI + LayoutElement]
  Grp_HudAlertsActions [HorizontalLayoutGroup spacing=8 childControlWidth/Height=true]
    Btn_HudAlertsOpen [Button + Image]
      Txt_HudAlertsOpenLabel [TextMeshProUGUI]
    Btn_HudAlertsLocate [Button + Image]
      Txt_HudAlertsLocateLabel [TextMeshProUGUI]
  Txt_HudAlertsStatus [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageHudAlerts | min(0.5,1) max(0.5,1); pivot(0.5,1); sizeDelta(600,144); pos(0,-128) | absolute | Image；警报摘要；内部页面根 |
| Grp_HudAlertsContent | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-16,84); pos(0,-4) | absolute | HorizontalLayoutGroup spacing=8 padding=0 childControlWidth/Height=true；模块文本行 |
| Txt_HudAlertsHighest | min(0,1) max(0,1); pivot(0,1); sizeDelta(292,84)初始化; pos(0,0)初始化; LayoutElement preferred(292,84); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement；最高等级警报：— |
| Txt_HudAlertsChanges | min(0,1) max(0,1); pivot(0,1); sizeDelta(292,84)初始化; pos(0,0)初始化; LayoutElement preferred(292,84); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement；状态变化：— |
| Grp_HudAlertsActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,8) | absolute | HorizontalLayoutGroup spacing=8 childControlWidth/Height=true；入口操作区 |
| Btn_HudAlertsOpen | min(0,1) max(0,1); pivot(0,1); sizeDelta(284,48)初始化; pos(0,0)初始化; LayoutElement preferred(284,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开15-警报列表并筛选活跃 |
| Txt_HudAlertsOpenLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看全部警报 |
| Btn_HudAlertsLocate | min(0,1) max(0,1); pivot(0,1); sizeDelta(284,48)初始化; pos(0,0)初始化; LayoutElement preferred(284,48); 最终位置/尺寸由组驱动 | group | Button + Image；来源仍在当前世界才定位，否则打开记录详情 |
| Txt_HudAlertsLocateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；定位警报来源 |
| Txt_HudAlertsStatus | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-16,84); pos(0,-4) | absolute | TextMeshProUGUI；无数据／保存失败等；仅替代对应文本区显示，正常隐藏 |

## HudNavigation：底部功能入口栏

功能文档：[底部功能入口栏](HudNavigation.md)；归属 `FieldHudForm`；内容 1248×80。

```text
Panel_PageHudNavigation [Image]
  Grp_HudNavigationContent [HorizontalLayoutGroup spacing=8 padding=0 childControlWidth/Height=true]
  Grp_HudNavigationActions [HorizontalLayoutGroup spacing=8 childControlWidth/Height=true]
    Btn_HudNavigationBuild [Button + Image]
      Txt_HudNavigationBuildLabel [TextMeshProUGUI]
    Btn_HudNavigationShop [Button + Image]
      Txt_HudNavigationShopLabel [TextMeshProUGUI]
    Btn_HudNavigationComponents [Button + Image]
      Txt_HudNavigationComponentsLabel [TextMeshProUGUI]
    Btn_HudNavigationMachines [Button + Image]
      Txt_HudNavigationMachinesLabel [TextMeshProUGUI]
    Btn_HudNavigationHub [Button + Image]
      Txt_HudNavigationHubLabel [TextMeshProUGUI]
    Btn_HudNavigationTasks [Button + Image]
      Txt_HudNavigationTasksLabel [TextMeshProUGUI]
    Btn_HudNavigationSystem [Button + Image]
      Txt_HudNavigationSystemLabel [TextMeshProUGUI]
  Txt_HudNavigationStatus [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageHudNavigation | min(0.5,0) max(0.5,0); pivot(0.5,0); sizeDelta(1248,80); pos(0,24) | absolute | Image；底部功能入口栏；内部页面根 |
| Grp_HudNavigationContent | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-16,20); pos(0,-4) | absolute | HorizontalLayoutGroup spacing=8 padding=0 childControlWidth/Height=true；模块文本行 |
| Grp_HudNavigationActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,8) | absolute | HorizontalLayoutGroup spacing=8 childControlWidth/Height=true；入口操作区 |
| Btn_HudNavigationBuild | min(0,1) max(0,1); pivot(0,1); sizeDelta(168,48)初始化; pos(0,0)初始化; LayoutElement preferred(168,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开10-建筑图纸 |
| Txt_HudNavigationBuildLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；建造 |
| Btn_HudNavigationShop | min(0,1) max(0,1); pivot(0,1); sizeDelta(168,48)初始化; pos(0,0)初始化; LayoutElement preferred(168,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开10-商店购买 |
| Txt_HudNavigationShopLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；商店 |
| Btn_HudNavigationComponents | min(0,1) max(0,1); pivot(0,1); sizeDelta(168,48)初始化; pos(0,0)初始化; LayoutElement preferred(168,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开09-未安装组件 |
| Txt_HudNavigationComponentsLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；组件库 |
| Btn_HudNavigationMachines | min(0,1) max(0,1); pivot(0,1); sizeDelta(168,48)初始化; pos(0,0)初始化; LayoutElement preferred(168,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开09-未部署机器 |
| Txt_HudNavigationMachinesLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；机器库 |
| Btn_HudNavigationHub | min(0,1) max(0,1); pivot(0,1); sizeDelta(168,48)初始化; pos(0,0)初始化; LayoutElement preferred(168,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开04-总览或恢复中枢上次分页 |
| Txt_HudNavigationHubLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；基地中枢 |
| Btn_HudNavigationTasks | min(0,1) max(0,1); pivot(0,1); sizeDelta(168,48)初始化; pos(0,0)初始化; LayoutElement preferred(168,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开15-主线任务 |
| Txt_HudNavigationTasksLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；任务 |
| Btn_HudNavigationSystem | min(0,1) max(0,1); pivot(0,1); sizeDelta(168,48)初始化; pos(0,0)初始化; LayoutElement preferred(168,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开02-系统菜单 |
| Txt_HudNavigationSystemLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；系统 |
| Txt_HudNavigationStatus | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-16,20); pos(0,-4) | absolute | TextMeshProUGUI；无数据／保存失败等；仅替代对应文本区显示，正常隐藏 |

## HudSave：保存状态提示

功能文档：[保存状态提示](HudSave.md)；归属 `FieldHudForm`；内容 380×96。

```text
Panel_PageHudSave [Image]
  Grp_HudSaveContent [HorizontalLayoutGroup spacing=8 padding=0 childControlWidth/Height=true]
    Txt_HudSaveSave [TextMeshProUGUI + LayoutElement]
  Grp_HudSaveActions [HorizontalLayoutGroup spacing=8 childControlWidth/Height=true]
    Btn_HudSaveDetails [Button + Image]
      Txt_HudSaveDetailsLabel [TextMeshProUGUI]
  Txt_HudSaveStatus [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageHudSave | min(1,0) max(1,0); pivot(1,0); sizeDelta(380,96); pos(-24,120) | absolute | Image；保存状态提示；内部页面根 |
| Grp_HudSaveContent | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-16,36); pos(0,-4) | absolute | HorizontalLayoutGroup spacing=8 padding=0 childControlWidth/Height=true；模块文本行 |
| Txt_HudSaveSave | min(0,1) max(0,1); pivot(0,1); sizeDelta(364,36)初始化; pos(0,0)初始化; LayoutElement preferred(364,36); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement；存档状态：— |
| Grp_HudSaveActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,8) | absolute | HorizontalLayoutGroup spacing=8 childControlWidth/Height=true；入口操作区 |
| Btn_HudSaveDetails | min(0,1) max(0,1); pivot(0,1); sizeDelta(356,48)初始化; pos(0,0)初始化; LayoutElement preferred(356,48); 最终位置/尺寸由组驱动 | group | Button + Image；失败时打开02-系统菜单保留失败说明 |
| Txt_HudSaveDetailsLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看保存状态 |
| Txt_HudSaveStatus | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-16,36); pos(0,-4) | absolute | TextMeshProUGUI；无数据／保存失败等；仅替代对应文本区显示，正常隐藏 |
