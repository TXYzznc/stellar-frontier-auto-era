# 01-启动与存档 — prefab-layout

状态：2026-09-19完整设计候选，待用户集中验收；未据此修改Prefab。

继承[通用合同](../00-通用合同.md)和[共享外壳](../00-共享外壳-prefab-layout.md)。下列子树预制在所属Form的Grp_PageHost中；公共外壳由共享文档唯一声明。跨家族同属一个Form的子树合并，禁止重复根节点。表中所有Image／文本颜色、资源、射线、默认字号与状态继承通用合同，列出的数据是字段说明，不是示例业务数值。

## MainMenu：主菜单

功能文档：[主菜单](MainMenu.md)；归属 `MainMenuForm`；内容 1008×650。

```text
Panel_PageMainMenu [Image]
  Txt_MainMenuTitle [TextMeshProUGUI]
  Grp_MainMenuActions [GridLayoutGroup fixedColumns=5 cell=(192,48) spacing=(8,8)]
    Btn_MainMenuContinue [Button + Image]
      Txt_MainMenuContinueLabel [TextMeshProUGUI]
    Btn_MainMenuNew [Button + Image]
      Txt_MainMenuNewLabel [TextMeshProUGUI]
    Btn_MainMenuSlots [Button + Image]
      Txt_MainMenuSlotsLabel [TextMeshProUGUI]
    Btn_MainMenuSettings [Button + Image]
      Txt_MainMenuSettingsLabel [TextMeshProUGUI]
    Btn_MainMenuExit [Button + Image]
      Txt_MainMenuExitLabel [TextMeshProUGUI]
  Panel_MainMenuIdentity [Image]
    Txt_MainMenuIdentityHeading [TextMeshProUGUI]
    List_MainMenuIdentity [ScrollRect vertical=true horizontal=false]
      Viewport_MainMenuIdentity [RectMask2D]
        Content_MainMenuIdentity [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_MainMenuIdentityBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_MainMenuIdentityTemplate [LayoutElement + Image；默认inactive]
            Btn_MainMenuIdentityRow [Button + Image]
              Txt_MainMenuIdentityRowLabel [TextMeshProUGUI]
              Txt_MainMenuIdentityRowValue [TextMeshProUGUI]
  Panel_MainMenuEntry [Image]
    Txt_MainMenuEntryHeading [TextMeshProUGUI]
    List_MainMenuEntry [ScrollRect vertical=true horizontal=false]
      Viewport_MainMenuEntry [RectMask2D]
        Content_MainMenuEntry [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_MainMenuEntryBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_MainMenuEntryTemplate [LayoutElement + Image；默认inactive]
            Btn_MainMenuEntryRow [Button + Image]
              Txt_MainMenuEntryRowLabel [TextMeshProUGUI]
              Txt_MainMenuEntryRowValue [TextMeshProUGUI]
  Grp_MainMenuLoadingState [无Graphic]
    Panel_MainMenuLoadingMessage [Image]
      Txt_MainMenuLoadingMessage [TextMeshProUGUI]
  Grp_MainMenuEmptyState [无Graphic]
    Panel_MainMenuEmptyMessage [Image]
      Txt_MainMenuEmptyMessage [TextMeshProUGUI]
  Grp_MainMenuErrorState [无Graphic]
    Panel_MainMenuErrorMessage [Image]
      Txt_MainMenuErrorMessage [TextMeshProUGUI]
  Grp_MainMenuSuccessState [无Graphic]
    Panel_MainMenuSuccessMessage [Image]
      Txt_MainMenuSuccessMessage [TextMeshProUGUI]
  Grp_MainMenuDisabledState [无Graphic]
    Panel_MainMenuDisabledMessage [Image]
      Txt_MainMenuDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageMainMenu | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；主菜单；内部页面根 |
| Txt_MainMenuTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(992,32); pos(8,0) | absolute | TextMeshProUGUI；主菜单 |
| Grp_MainMenuActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=5 cell=(192,48) spacing=(8,8)；操作区 |
| Btn_MainMenuContinue | min(0,1) max(0,1); pivot(0,1); sizeDelta(192,48)初始化; pos(0,0)初始化; LayoutElement preferred(192,48); 最终位置/尺寸由组驱动 | group | Button + Image；读取最后使用的有效槽位→18-加载→18-离线结算→18-回归报告；无有效槽位时禁用并说明 |
| Txt_MainMenuContinueLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；继续游戏 |
| Btn_MainMenuNew | min(0,1) max(0,1); pivot(0,1); sizeDelta(192,48)初始化; pos(0,0)初始化; LayoutElement preferred(192,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开01-存档槽列表，选空槽创建；有内容的槽位先走强确认 |
| Txt_MainMenuNewLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；新游戏 |
| Btn_MainMenuSlots | min(0,1) max(0,1); pivot(0,1); sizeDelta(192,48)初始化; pos(0,0)初始化; LayoutElement preferred(192,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开01-存档槽列表 |
| Txt_MainMenuSlotsLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；选择进度 |
| Btn_MainMenuSettings | min(0,1) max(0,1); pivot(0,1); sizeDelta(192,48)初始化; pos(0,0)初始化; LayoutElement preferred(192,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开02-声音设置并保留主菜单来源 |
| Txt_MainMenuSettingsLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；设置 |
| Btn_MainMenuExit | min(0,1) max(0,1); pivot(0,1); sizeDelta(192,48)初始化; pos(0,0)初始化; LayoutElement preferred(192,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开17-普通确认，确定后退出 |
| Txt_MainMenuExitLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；退出游戏 |
| Panel_MainMenuIdentity | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,554); pos(0,-36) | absolute | Image；游戏标题与启动状态 |
| Txt_MainMenuIdentityHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(472,32); pos(12,-8) | absolute | TextMeshProUGUI；游戏标题与启动状态 |
| List_MainMenuIdentity | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_MainMenuIdentity | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_MainMenuIdentity | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_MainMenuIdentityBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(472,120)初始化; pos(0,0)初始化; LayoutElement preferred(472,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；游戏名称；版本；资源就绪状态 |
| Item_MainMenuIdentityTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(472,104)初始化; pos(0,0)初始化; LayoutElement preferred(472,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_MainMenuIdentityRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_MainMenuIdentityRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_MainMenuIdentityRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_MainMenuEntry | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,554); pos(512,-36) | absolute | Image；进度入口 |
| Txt_MainMenuEntryHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(472,32); pos(12,-8) | absolute | TextMeshProUGUI；进度入口 |
| List_MainMenuEntry | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_MainMenuEntry | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_MainMenuEntry | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_MainMenuEntryBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(472,120)初始化; pos(0,0)初始化; LayoutElement preferred(472,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；最后使用的有效槽位摘要；没有有效进度时的说明 |
| Item_MainMenuEntryTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(472,104)初始化; pos(0,0)初始化; LayoutElement preferred(472,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_MainMenuEntryRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_MainMenuEntryRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_MainMenuEntryRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_MainMenuLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1008,554); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MainMenuLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MainMenuLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_MainMenuEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1008,554); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MainMenuEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MainMenuEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_MainMenuErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1008,554); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MainMenuErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MainMenuErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_MainMenuSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1008,554); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MainMenuSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MainMenuSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_MainMenuDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1008,554); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MainMenuDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MainMenuDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## SaveSlots：存档槽列表

功能文档：[存档槽列表](SaveSlots.md)；归属 `SaveSlotsForm`；内容 1488×730。

```text
Panel_PageSaveSlots [Image]
  Txt_SaveSlotsTitle [TextMeshProUGUI]
  Grp_SaveSlotsActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_SaveSlotsSelect [Button + Image]
      Txt_SaveSlotsSelectLabel [TextMeshProUGUI]
    Btn_SaveSlotsDetails [Button + Image]
      Txt_SaveSlotsDetailsLabel [TextMeshProUGUI]
    Btn_SaveSlotsCreate [Button + Image]
      Txt_SaveSlotsCreateLabel [TextMeshProUGUI]
    Btn_SaveSlotsContinue [Button + Image]
      Txt_SaveSlotsContinueLabel [TextMeshProUGUI]
  Panel_SaveSlotsSlots [Image]
    Txt_SaveSlotsSlotsHeading [TextMeshProUGUI]
    List_SaveSlotsSlots [ScrollRect vertical=true horizontal=false]
      Viewport_SaveSlotsSlots [RectMask2D]
        Content_SaveSlotsSlots [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_SaveSlotsSlotsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_SaveSlotsSlotsTemplate [LayoutElement + Image；默认inactive]
            Btn_SaveSlotsSlotsRow [Button + Image]
              Txt_SaveSlotsSlotsRowLabel [TextMeshProUGUI]
              Txt_SaveSlotsSlotsRowValue [TextMeshProUGUI]
  Panel_SaveSlotsPreview [Image]
    Txt_SaveSlotsPreviewHeading [TextMeshProUGUI]
    List_SaveSlotsPreview [ScrollRect vertical=true horizontal=false]
      Viewport_SaveSlotsPreview [RectMask2D]
        Content_SaveSlotsPreview [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_SaveSlotsPreviewBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_SaveSlotsPreviewTemplate [LayoutElement + Image；默认inactive]
            Btn_SaveSlotsPreviewRow [Button + Image]
              Txt_SaveSlotsPreviewRowLabel [TextMeshProUGUI]
              Txt_SaveSlotsPreviewRowValue [TextMeshProUGUI]
  Grp_SaveSlotsLoadingState [无Graphic]
    Panel_SaveSlotsLoadingMessage [Image]
      Txt_SaveSlotsLoadingMessage [TextMeshProUGUI]
  Grp_SaveSlotsEmptyState [无Graphic]
    Panel_SaveSlotsEmptyMessage [Image]
      Txt_SaveSlotsEmptyMessage [TextMeshProUGUI]
  Grp_SaveSlotsErrorState [无Graphic]
    Panel_SaveSlotsErrorMessage [Image]
      Txt_SaveSlotsErrorMessage [TextMeshProUGUI]
  Grp_SaveSlotsSuccessState [无Graphic]
    Panel_SaveSlotsSuccessMessage [Image]
      Txt_SaveSlotsSuccessMessage [TextMeshProUGUI]
  Grp_SaveSlotsDisabledState [无Graphic]
    Panel_SaveSlotsDisabledMessage [Image]
      Txt_SaveSlotsDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageSaveSlots | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；存档槽列表；内部页面根 |
| Txt_SaveSlotsTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；存档槽列表 |
| Grp_SaveSlotsActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_SaveSlotsSelect | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；更新右侧预览；不自动读取 |
| Txt_SaveSlotsSelectLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；选择槽位 |
| Btn_SaveSlotsDetails | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开01-存档详情子页 |
| Txt_SaveSlotsDetailsLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看详情 |
| Btn_SaveSlotsCreate | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开01-新建进度；已有进度时显示覆盖风险 |
| Txt_SaveSlotsCreateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；在此新建 |
| Btn_SaveSlotsContinue | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；有效时读取；损坏时进入01-损坏存档恢复；版本不兼容时禁止读取 |
| Txt_SaveSlotsContinueLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；继续所选进度 |
| Panel_SaveSlotsSlots | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；三个进度槽 |
| Txt_SaveSlotsSlotsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；三个进度槽 |
| List_SaveSlotsSlots | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_SaveSlotsSlots | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_SaveSlotsSlots | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_SaveSlotsSlotsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；槽位编号；空／有效／损坏／不兼容状态；中枢等级；主线阶段；游戏内天数；游玩时间；最后保存时间 |
| Item_SaveSlotsSlotsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_SaveSlotsSlotsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_SaveSlotsSlotsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_SaveSlotsSlotsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_SaveSlotsPreview | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；选中进度 |
| Txt_SaveSlotsPreviewHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；选中进度 |
| List_SaveSlotsPreview | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_SaveSlotsPreview | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_SaveSlotsPreview | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_SaveSlotsPreviewBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；所选槽位的完整摘要；可继续或需要恢复的原因 |
| Item_SaveSlotsPreviewTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_SaveSlotsPreviewRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_SaveSlotsPreviewRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_SaveSlotsPreviewRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_SaveSlotsLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SaveSlotsLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SaveSlotsLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_SaveSlotsEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SaveSlotsEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SaveSlotsEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_SaveSlotsErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SaveSlotsErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SaveSlotsErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_SaveSlotsSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SaveSlotsSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SaveSlotsSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_SaveSlotsDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SaveSlotsDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SaveSlotsDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## SaveDetail：存档详情

功能文档：[存档详情](SaveDetail.md)；归属 `SaveSlotsForm`；内容 1488×730。

```text
Panel_PageSaveDetail [Image]
  Txt_SaveDetailTitle [TextMeshProUGUI]
  Grp_SaveDetailActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_SaveDetailContinue [Button + Image]
      Txt_SaveDetailContinueLabel [TextMeshProUGUI]
    Btn_SaveDetailDelete [Button + Image]
      Txt_SaveDetailDeleteLabel [TextMeshProUGUI]
    Btn_SaveDetailRecover [Button + Image]
      Txt_SaveDetailRecoverLabel [TextMeshProUGUI]
  Panel_SaveDetailMetadata [Image]
    Txt_SaveDetailMetadataHeading [TextMeshProUGUI]
    List_SaveDetailMetadata [ScrollRect vertical=true horizontal=false]
      Viewport_SaveDetailMetadata [RectMask2D]
        Content_SaveDetailMetadata [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_SaveDetailMetadataBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_SaveDetailMetadataTemplate [LayoutElement + Image；默认inactive]
            Btn_SaveDetailMetadataRow [Button + Image]
              Txt_SaveDetailMetadataRowLabel [TextMeshProUGUI]
              Txt_SaveDetailMetadataRowValue [TextMeshProUGUI]
  Panel_SaveDetailHealth [Image]
    Txt_SaveDetailHealthHeading [TextMeshProUGUI]
    List_SaveDetailHealth [ScrollRect vertical=true horizontal=false]
      Viewport_SaveDetailHealth [RectMask2D]
        Content_SaveDetailHealth [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_SaveDetailHealthBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_SaveDetailHealthTemplate [LayoutElement + Image；默认inactive]
            Btn_SaveDetailHealthRow [Button + Image]
              Txt_SaveDetailHealthRowLabel [TextMeshProUGUI]
              Txt_SaveDetailHealthRowValue [TextMeshProUGUI]
  Grp_SaveDetailLoadingState [无Graphic]
    Panel_SaveDetailLoadingMessage [Image]
      Txt_SaveDetailLoadingMessage [TextMeshProUGUI]
  Grp_SaveDetailEmptyState [无Graphic]
    Panel_SaveDetailEmptyMessage [Image]
      Txt_SaveDetailEmptyMessage [TextMeshProUGUI]
  Grp_SaveDetailErrorState [无Graphic]
    Panel_SaveDetailErrorMessage [Image]
      Txt_SaveDetailErrorMessage [TextMeshProUGUI]
  Grp_SaveDetailSuccessState [无Graphic]
    Panel_SaveDetailSuccessMessage [Image]
      Txt_SaveDetailSuccessMessage [TextMeshProUGUI]
  Grp_SaveDetailDisabledState [无Graphic]
    Panel_SaveDetailDisabledMessage [Image]
      Txt_SaveDetailDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageSaveDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；存档详情；内部页面根 |
| Txt_SaveDetailTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；存档详情 |
| Grp_SaveDetailActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_SaveDetailContinue | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；重验槽位版本与校验状态后读取 |
| Txt_SaveDetailContinueLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；继续 |
| Btn_SaveDetailDelete | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入17-删除覆盖强确认；成功回槽位列表并显示空槽 |
| Txt_SaveDetailDeleteLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；删除进度 |
| Btn_SaveDetailRecover | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；仅正式存档损坏且找到有效备份时打开01-损坏存档恢复 |
| Txt_SaveDetailRecoverLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；恢复备份 |
| Panel_SaveDetailMetadata | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；进度资料 |
| Txt_SaveDetailMetadataHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；进度资料 |
| List_SaveDetailMetadata | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_SaveDetailMetadata | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_SaveDetailMetadata | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_SaveDetailMetadataBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；槽位ID；中枢等级；主线阶段；天数；累计游玩时间；保存时间；格式版本 |
| Item_SaveDetailMetadataTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_SaveDetailMetadataRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_SaveDetailMetadataRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_SaveDetailMetadataRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_SaveDetailHealth | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；读取状态 |
| Txt_SaveDetailHealthHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；读取状态 |
| List_SaveDetailHealth | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_SaveDetailHealth | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_SaveDetailHealth | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_SaveDetailHealthBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；校验状态；正式存档是否可读；可用恢复路径；删除将移除正式存档及三份内部备份 |
| Item_SaveDetailHealthTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_SaveDetailHealthRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_SaveDetailHealthRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_SaveDetailHealthRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_SaveDetailLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SaveDetailLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SaveDetailLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_SaveDetailEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SaveDetailEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SaveDetailEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_SaveDetailErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SaveDetailErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SaveDetailErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_SaveDetailSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SaveDetailSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SaveDetailSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_SaveDetailDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SaveDetailDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SaveDetailDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## NewProgress：新建进度

功能文档：[新建进度](NewProgress.md)；归属 `SaveSlotsForm`；内容 1488×730。

```text
Panel_PageNewProgress [Image]
  Txt_NewProgressTitle [TextMeshProUGUI]
  Grp_NewProgressActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_NewProgressCreate [Button + Image]
      Txt_NewProgressCreateLabel [TextMeshProUGUI]
    Btn_NewProgressBack [Button + Image]
      Txt_NewProgressBackLabel [TextMeshProUGUI]
  Panel_NewProgressTarget [Image]
    Txt_NewProgressTargetHeading [TextMeshProUGUI]
    List_NewProgressTarget [ScrollRect vertical=true horizontal=false]
      Viewport_NewProgressTarget [RectMask2D]
        Content_NewProgressTarget [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_NewProgressTargetBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_NewProgressTargetTemplate [LayoutElement + Image；默认inactive]
            Btn_NewProgressTargetRow [Button + Image]
              Txt_NewProgressTargetRowLabel [TextMeshProUGUI]
              Txt_NewProgressTargetRowValue [TextMeshProUGUI]
  Panel_NewProgressConsequences [Image]
    Txt_NewProgressConsequencesHeading [TextMeshProUGUI]
    List_NewProgressConsequences [ScrollRect vertical=true horizontal=false]
      Viewport_NewProgressConsequences [RectMask2D]
        Content_NewProgressConsequences [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_NewProgressConsequencesBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_NewProgressConsequencesTemplate [LayoutElement + Image；默认inactive]
            Btn_NewProgressConsequencesRow [Button + Image]
              Txt_NewProgressConsequencesRowLabel [TextMeshProUGUI]
              Txt_NewProgressConsequencesRowValue [TextMeshProUGUI]
  Grp_NewProgressLoadingState [无Graphic]
    Panel_NewProgressLoadingMessage [Image]
      Txt_NewProgressLoadingMessage [TextMeshProUGUI]
  Grp_NewProgressEmptyState [无Graphic]
    Panel_NewProgressEmptyMessage [Image]
      Txt_NewProgressEmptyMessage [TextMeshProUGUI]
  Grp_NewProgressErrorState [无Graphic]
    Panel_NewProgressErrorMessage [Image]
      Txt_NewProgressErrorMessage [TextMeshProUGUI]
  Grp_NewProgressSuccessState [无Graphic]
    Panel_NewProgressSuccessMessage [Image]
      Txt_NewProgressSuccessMessage [TextMeshProUGUI]
  Grp_NewProgressDisabledState [无Graphic]
    Panel_NewProgressDisabledMessage [Image]
      Txt_NewProgressDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageNewProgress | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；新建进度；内部页面根 |
| Txt_NewProgressTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；新建进度 |
| Grp_NewProgressActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_NewProgressCreate | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；空槽一次确认后建立新进度并进入加载；非空槽进入17-删除覆盖强确认 |
| Txt_NewProgressCreateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；创建进度 |
| Btn_NewProgressBack | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；放弃尚未提交的新建，回到原槽位选择 |
| Txt_NewProgressBackLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；返回槽位 |
| Panel_NewProgressTarget | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；目标槽位 |
| Txt_NewProgressTargetHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；目标槽位 |
| List_NewProgressTarget | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_NewProgressTarget | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_NewProgressTarget | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_NewProgressTargetBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；槽位编号；当前为空或已有进度；新进度从初始世界开始 |
| Item_NewProgressTargetTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_NewProgressTargetRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_NewProgressTargetRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_NewProgressTargetRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_NewProgressConsequences | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；创建说明 |
| Txt_NewProgressConsequencesHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；创建说明 |
| List_NewProgressConsequences | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_NewProgressConsequences | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_NewProgressConsequences | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_NewProgressConsequencesBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；空槽直接建立新世界；覆盖槽将移除正式进度和三份备份；当前版本初始内容摘要 |
| Item_NewProgressConsequencesTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_NewProgressConsequencesRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_NewProgressConsequencesRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_NewProgressConsequencesRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_NewProgressLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_NewProgressLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_NewProgressLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_NewProgressEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_NewProgressEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_NewProgressEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_NewProgressErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_NewProgressErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_NewProgressErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_NewProgressSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_NewProgressSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_NewProgressSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_NewProgressDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_NewProgressDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_NewProgressDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## Recovery：损坏存档恢复

功能文档：[损坏存档恢复](Recovery.md)；归属 `SaveRecoveryForm`；内容 848×630。

```text
Panel_PageRecovery [Image]
  Txt_RecoveryTitle [TextMeshProUGUI]
  Grp_RecoveryActions [GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)]
    Btn_RecoveryRestore [Button + Image]
      Txt_RecoveryRestoreLabel [TextMeshProUGUI]
    Btn_RecoveryReturn [Button + Image]
      Txt_RecoveryReturnLabel [TextMeshProUGUI]
  Panel_RecoveryDiagnosis [Image]
    Txt_RecoveryDiagnosisHeading [TextMeshProUGUI]
    List_RecoveryDiagnosis [ScrollRect vertical=true horizontal=false]
      Viewport_RecoveryDiagnosis [RectMask2D]
        Content_RecoveryDiagnosis [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_RecoveryDiagnosisBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_RecoveryDiagnosisTemplate [LayoutElement + Image；默认inactive]
            Btn_RecoveryDiagnosisRow [Button + Image]
              Txt_RecoveryDiagnosisRowLabel [TextMeshProUGUI]
              Txt_RecoveryDiagnosisRowValue [TextMeshProUGUI]
  Panel_RecoveryCandidate [Image]
    Txt_RecoveryCandidateHeading [TextMeshProUGUI]
    List_RecoveryCandidate [ScrollRect vertical=true horizontal=false]
      Viewport_RecoveryCandidate [RectMask2D]
        Content_RecoveryCandidate [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_RecoveryCandidateBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_RecoveryCandidateTemplate [LayoutElement + Image；默认inactive]
            Btn_RecoveryCandidateRow [Button + Image]
              Txt_RecoveryCandidateRowLabel [TextMeshProUGUI]
              Txt_RecoveryCandidateRowValue [TextMeshProUGUI]
  Grp_RecoveryLoadingState [无Graphic]
    Panel_RecoveryLoadingMessage [Image]
      Txt_RecoveryLoadingMessage [TextMeshProUGUI]
  Grp_RecoveryEmptyState [无Graphic]
    Panel_RecoveryEmptyMessage [Image]
      Txt_RecoveryEmptyMessage [TextMeshProUGUI]
  Grp_RecoveryErrorState [无Graphic]
    Panel_RecoveryErrorMessage [Image]
      Txt_RecoveryErrorMessage [TextMeshProUGUI]
  Grp_RecoverySuccessState [无Graphic]
    Panel_RecoverySuccessMessage [Image]
      Txt_RecoverySuccessMessage [TextMeshProUGUI]
  Grp_RecoveryDisabledState [无Graphic]
    Panel_RecoveryDisabledMessage [Image]
      Txt_RecoveryDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageRecovery | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；损坏存档恢复；内部页面根 |
| Txt_RecoveryTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(832,32); pos(8,0) | absolute | TextMeshProUGUI；损坏存档恢复 |
| Grp_RecoveryActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)；操作区 |
| Btn_RecoveryRestore | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；明确确认所选备份后恢复，进入18-备份恢复进度；不静默回退 |
| Txt_RecoveryRestoreLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；恢复并继续 |
| Btn_RecoveryReturn | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；保持现有文件，退出恢复流程 |
| Txt_RecoveryReturnLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；返回主菜单 |
| Panel_RecoveryDiagnosis | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(0,-36) | absolute | Image；恢复原因 |
| Txt_RecoveryDiagnosisHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；恢复原因 |
| List_RecoveryDiagnosis | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_RecoveryDiagnosis | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_RecoveryDiagnosis | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_RecoveryDiagnosisBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；正式存档无法读取说明；槽位与原保存时间；备份搜索结果 |
| Item_RecoveryDiagnosisTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_RecoveryDiagnosisRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_RecoveryDiagnosisRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_RecoveryDiagnosisRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_RecoveryCandidate | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(432,-36) | absolute | Image；可恢复备份 |
| Txt_RecoveryCandidateHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；可恢复备份 |
| List_RecoveryCandidate | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_RecoveryCandidate | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_RecoveryCandidate | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_RecoveryCandidateBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；由新到旧找到的首个有效备份时间；可能丢失的时间区间；无法准确估算时明确说明 |
| Item_RecoveryCandidateTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_RecoveryCandidateRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_RecoveryCandidateRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_RecoveryCandidateRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_RecoveryLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_RecoveryLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_RecoveryLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_RecoveryEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_RecoveryEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_RecoveryEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_RecoveryErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_RecoveryErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_RecoveryErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_RecoverySuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_RecoverySuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_RecoverySuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_RecoveryDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_RecoveryDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_RecoveryDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |
