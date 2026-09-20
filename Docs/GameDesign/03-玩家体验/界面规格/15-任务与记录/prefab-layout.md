# 15-任务与记录 — prefab-layout

状态：2026-09-19完整设计候选，待用户集中验收；未据此修改Prefab。

继承[通用合同](../00-通用合同.md)和[共享外壳](../00-共享外壳-prefab-layout.md)。下列子树预制在所属Form的Grp_PageHost中；公共外壳由共享文档唯一声明。跨家族同属一个Form的子树合并，禁止重复根节点。表中所有Image／文本颜色、资源、射线、默认字号与状态继承通用合同，列出的数据是字段说明，不是示例业务数值。

## Quests：主线任务

功能文档：[主线任务](Quests.md)；归属 `QuestForm`；内容 1488×730。

```text
Panel_PageQuests [Image]
  Txt_QuestsTitle [TextMeshProUGUI]
  Grp_QuestsActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_QuestsTrack [Button + Image]
      Txt_QuestsTrackLabel [TextMeshProUGUI]
    Btn_QuestsClaim [Button + Image]
      Txt_QuestsClaimLabel [TextMeshProUGUI]
    Btn_QuestsGo [Button + Image]
      Txt_QuestsGoLabel [TextMeshProUGUI]
    Btn_QuestsTabs [Button + Image]
      Txt_QuestsTabsLabel [TextMeshProUGUI]
  Panel_QuestsList [Image]
    Txt_QuestsListHeading [TextMeshProUGUI]
    List_QuestsList [ScrollRect vertical=true horizontal=false]
      Viewport_QuestsList [RectMask2D]
        Content_QuestsList [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_QuestsListBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_QuestsListTemplate [LayoutElement + Image；默认inactive]
            Btn_QuestsListRow [Button + Image]
              Txt_QuestsListRowLabel [TextMeshProUGUI]
              Txt_QuestsListRowValue [TextMeshProUGUI]
  Panel_QuestsDetail [Image]
    Txt_QuestsDetailHeading [TextMeshProUGUI]
    List_QuestsDetail [ScrollRect vertical=true horizontal=false]
      Viewport_QuestsDetail [RectMask2D]
        Content_QuestsDetail [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_QuestsDetailBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_QuestsDetailTemplate [LayoutElement + Image；默认inactive]
            Btn_QuestsDetailRow [Button + Image]
              Txt_QuestsDetailRowLabel [TextMeshProUGUI]
              Txt_QuestsDetailRowValue [TextMeshProUGUI]
  Grp_QuestsLoadingState [无Graphic]
    Panel_QuestsLoadingMessage [Image]
      Txt_QuestsLoadingMessage [TextMeshProUGUI]
  Grp_QuestsEmptyState [无Graphic]
    Panel_QuestsEmptyMessage [Image]
      Txt_QuestsEmptyMessage [TextMeshProUGUI]
  Grp_QuestsErrorState [无Graphic]
    Panel_QuestsErrorMessage [Image]
      Txt_QuestsErrorMessage [TextMeshProUGUI]
  Grp_QuestsSuccessState [无Graphic]
    Panel_QuestsSuccessMessage [Image]
      Txt_QuestsSuccessMessage [TextMeshProUGUI]
  Grp_QuestsDisabledState [无Graphic]
    Panel_QuestsDisabledMessage [Image]
      Txt_QuestsDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageQuests | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；主线任务；内部页面根 |
| Txt_QuestsTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；主线任务 |
| Grp_QuestsActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_QuestsTrack | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；HUD仅保留一个任务组，最多三条当前目标 |
| Txt_QuestsTrackLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；追踪任务 |
| Btn_QuestsClaim | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；只结算待领取金币／资源，防重复；经验和解锁不再结算 |
| Txt_QuestsClaimLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；领取奖励 |
| Btn_QuestsGo | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；定位或打开已解锁界面，不自动购买配置 |
| Txt_QuestsGoLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；前往目标 |
| Btn_QuestsTabs | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；保留每类选择位置；已完成主线仍在本页 |
| Txt_QuestsTabsLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；切换分类 |
| Panel_QuestsList | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；任务分类 |
| Txt_QuestsListHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；任务分类 |
| List_QuestsList | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_QuestsList | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_QuestsList | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_QuestsListBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；进行中／可领取／已完成；任务组名；状态；仅已派发任务 |
| Item_QuestsListTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_QuestsListRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_QuestsListRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_QuestsListRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_QuestsDetail | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；任务详情 |
| Txt_QuestsDetailHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；任务详情 |
| List_QuestsDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_QuestsDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_QuestsDetail | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_QuestsDetailBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；目的；全部目标；实时进度；金币／资源奖励；自动结算经验／解锁；快捷入口 |
| Item_QuestsDetailTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_QuestsDetailRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_QuestsDetailRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_QuestsDetailRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_QuestsLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_QuestsLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_QuestsLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_QuestsEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_QuestsEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_QuestsEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_QuestsErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_QuestsErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_QuestsErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_QuestsSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_QuestsSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_QuestsSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_QuestsDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_QuestsDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_QuestsDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## QuestRewards：任务奖励与解锁详情

功能文档：[任务奖励与解锁详情](QuestRewards.md)；归属 `QuestForm`；内容 1488×730。

```text
Panel_PageQuestRewards [Image]
  Txt_QuestRewardsTitle [TextMeshProUGUI]
  Grp_QuestRewardsActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_QuestRewardsClaim [Button + Image]
      Txt_QuestRewardsClaimLabel [TextMeshProUGUI]
    Btn_QuestRewardsOpen [Button + Image]
      Txt_QuestRewardsOpenLabel [TextMeshProUGUI]
    Btn_QuestRewardsBack [Button + Image]
      Txt_QuestRewardsBackLabel [TextMeshProUGUI]
  Panel_QuestRewardsRewards [Image]
    Txt_QuestRewardsRewardsHeading [TextMeshProUGUI]
    List_QuestRewardsRewards [ScrollRect vertical=true horizontal=false]
      Viewport_QuestRewardsRewards [RectMask2D]
        Content_QuestRewardsRewards [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_QuestRewardsRewardsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_QuestRewardsRewardsTemplate [LayoutElement + Image；默认inactive]
            Btn_QuestRewardsRewardsRow [Button + Image]
              Txt_QuestRewardsRewardsRowLabel [TextMeshProUGUI]
              Txt_QuestRewardsRewardsRowValue [TextMeshProUGUI]
  Panel_QuestRewardsUnlocks [Image]
    Txt_QuestRewardsUnlocksHeading [TextMeshProUGUI]
    List_QuestRewardsUnlocks [ScrollRect vertical=true horizontal=false]
      Viewport_QuestRewardsUnlocks [RectMask2D]
        Content_QuestRewardsUnlocks [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_QuestRewardsUnlocksBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_QuestRewardsUnlocksTemplate [LayoutElement + Image；默认inactive]
            Btn_QuestRewardsUnlocksRow [Button + Image]
              Txt_QuestRewardsUnlocksRowLabel [TextMeshProUGUI]
              Txt_QuestRewardsUnlocksRowValue [TextMeshProUGUI]
  Grp_QuestRewardsLoadingState [无Graphic]
    Panel_QuestRewardsLoadingMessage [Image]
      Txt_QuestRewardsLoadingMessage [TextMeshProUGUI]
  Grp_QuestRewardsEmptyState [无Graphic]
    Panel_QuestRewardsEmptyMessage [Image]
      Txt_QuestRewardsEmptyMessage [TextMeshProUGUI]
  Grp_QuestRewardsErrorState [无Graphic]
    Panel_QuestRewardsErrorMessage [Image]
      Txt_QuestRewardsErrorMessage [TextMeshProUGUI]
  Grp_QuestRewardsSuccessState [无Graphic]
    Panel_QuestRewardsSuccessMessage [Image]
      Txt_QuestRewardsSuccessMessage [TextMeshProUGUI]
  Grp_QuestRewardsDisabledState [无Graphic]
    Panel_QuestRewardsDisabledMessage [Image]
      Txt_QuestRewardsDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageQuestRewards | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；任务奖励与解锁详情；内部页面根 |
| Txt_QuestRewardsTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；任务奖励与解锁详情 |
| Grp_QuestRewardsActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_QuestRewardsClaim | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；同一任务领取请求防重；成功18结果 |
| Txt_QuestRewardsClaimLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；领取 |
| Btn_QuestRewardsOpen | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；已开放时跳到图纸、商店或算法库 |
| Txt_QuestRewardsOpenLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看解锁内容 |
| Btn_QuestRewardsBack | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；恢复任务组选择 |
| Txt_QuestRewardsBackLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；返回任务 |
| Panel_QuestRewardsRewards | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；可领取奖励 |
| Txt_QuestRewardsRewardsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；可领取奖励 |
| List_QuestRewardsRewards | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_QuestRewardsRewards | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_QuestRewardsRewards | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_QuestRewardsRewardsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；金币与资源数量；领取状态；交付去向 |
| Item_QuestRewardsRewardsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_QuestRewardsRewardsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_QuestRewardsRewardsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_QuestRewardsRewardsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_QuestRewardsUnlocks | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；已自动生效 |
| Txt_QuestRewardsUnlocksHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；已自动生效 |
| List_QuestRewardsUnlocks | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_QuestRewardsUnlocks | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_QuestRewardsUnlocks | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_QuestRewardsUnlocksBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；经验；中枢等级变化；图纸；功能解锁条件 |
| Item_QuestRewardsUnlocksTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_QuestRewardsUnlocksRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_QuestRewardsUnlocksRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_QuestRewardsUnlocksRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_QuestRewardsLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_QuestRewardsLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_QuestRewardsLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_QuestRewardsEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_QuestRewardsEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_QuestRewardsEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_QuestRewardsErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_QuestRewardsErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_QuestRewardsErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_QuestRewardsSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_QuestRewardsSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_QuestRewardsSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_QuestRewardsDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_QuestRewardsDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_QuestRewardsDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## Alerts：警报列表与详情

功能文档：[警报列表与详情](Alerts.md)；归属 `AlertForm`；内容 1488×730。

```text
Panel_PageAlerts [Image]
  Txt_AlertsTitle [TextMeshProUGUI]
  Grp_AlertsActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_AlertsFilter [Button + Image]
      Txt_AlertsFilterLabel [TextMeshProUGUI]
    Btn_AlertsRead [Button + Image]
      Txt_AlertsReadLabel [TextMeshProUGUI]
    Btn_AlertsLocate [Button + Image]
      Txt_AlertsLocateLabel [TextMeshProUGUI]
    Btn_AlertsDetails [Button + Image]
      Txt_AlertsDetailsLabel [TextMeshProUGUI]
  Panel_AlertsList [Image]
    Txt_AlertsListHeading [TextMeshProUGUI]
    List_AlertsList [ScrollRect vertical=true horizontal=false]
      Viewport_AlertsList [RectMask2D]
        Content_AlertsList [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_AlertsListBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_AlertsListTemplate [LayoutElement + Image；默认inactive]
            Btn_AlertsListRow [Button + Image]
              Txt_AlertsListRowLabel [TextMeshProUGUI]
              Txt_AlertsListRowValue [TextMeshProUGUI]
  Panel_AlertsDetail [Image]
    Txt_AlertsDetailHeading [TextMeshProUGUI]
    List_AlertsDetail [ScrollRect vertical=true horizontal=false]
      Viewport_AlertsDetail [RectMask2D]
        Content_AlertsDetail [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_AlertsDetailBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_AlertsDetailTemplate [LayoutElement + Image；默认inactive]
            Btn_AlertsDetailRow [Button + Image]
              Txt_AlertsDetailRowLabel [TextMeshProUGUI]
              Txt_AlertsDetailRowValue [TextMeshProUGUI]
  Grp_AlertsLoadingState [无Graphic]
    Panel_AlertsLoadingMessage [Image]
      Txt_AlertsLoadingMessage [TextMeshProUGUI]
  Grp_AlertsEmptyState [无Graphic]
    Panel_AlertsEmptyMessage [Image]
      Txt_AlertsEmptyMessage [TextMeshProUGUI]
  Grp_AlertsErrorState [无Graphic]
    Panel_AlertsErrorMessage [Image]
      Txt_AlertsErrorMessage [TextMeshProUGUI]
  Grp_AlertsSuccessState [无Graphic]
    Panel_AlertsSuccessMessage [Image]
      Txt_AlertsSuccessMessage [TextMeshProUGUI]
  Grp_AlertsDisabledState [无Graphic]
    Panel_AlertsDisabledMessage [Image]
      Txt_AlertsDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageAlerts | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；警报列表与详情；内部页面根 |
| Txt_AlertsTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；警报列表与详情 |
| Grp_AlertsActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_AlertsFilter | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；活跃、历史、等级、来源分别筛选 |
| Txt_AlertsFilterLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；筛选警报 |
| Btn_AlertsRead | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；只改变阅读状态，不解决问题 |
| Txt_AlertsReadLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；标记已读 |
| Btn_AlertsLocate | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；有效对象聚焦 |
| Txt_AlertsLocateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；定位 |
| Btn_AlertsDetails | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开机器、能源或其它业务详情 |
| Txt_AlertsDetailsLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；相关详情 |
| Panel_AlertsList | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；警报列表 |
| Txt_AlertsListHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；警报列表 |
| List_AlertsList | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_AlertsList | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_AlertsList | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_AlertsListBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；活跃／已恢复；提醒／警告／严重；已读／未读；来源；首次／最近时间；次数 |
| Item_AlertsListTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_AlertsListRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_AlertsListRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_AlertsListRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_AlertsDetail | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；警报详情 |
| Txt_AlertsDetailHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；警报详情 |
| List_AlertsDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_AlertsDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_AlertsDetail | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_AlertsDetailBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；原因；影响；来源与目标；真实恢复条件；相关记录 |
| Item_AlertsDetailTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_AlertsDetailRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_AlertsDetailRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_AlertsDetailRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_AlertsLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_AlertsLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_AlertsLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_AlertsEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_AlertsEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_AlertsEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_AlertsErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_AlertsErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_AlertsErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_AlertsSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_AlertsSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_AlertsSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_AlertsDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_AlertsDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_AlertsDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## MachineHistory：机器任务历史

功能文档：[机器任务历史](MachineHistory.md)；归属 `RecordReaderForm`；内容 1488×730。

```text
Panel_PageMachineHistory [Image]
  Txt_MachineHistoryTitle [TextMeshProUGUI]
  Grp_MachineHistoryActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_MachineHistoryFilter [Button + Image]
      Txt_MachineHistoryFilterLabel [TextMeshProUGUI]
    Btn_MachineHistoryTrace [Button + Image]
      Txt_MachineHistoryTraceLabel [TextMeshProUGUI]
    Btn_MachineHistoryLocate [Button + Image]
      Txt_MachineHistoryLocateLabel [TextMeshProUGUI]
  Panel_MachineHistoryList [Image]
    Txt_MachineHistoryListHeading [TextMeshProUGUI]
    List_MachineHistoryList [ScrollRect vertical=true horizontal=false]
      Viewport_MachineHistoryList [RectMask2D]
        Content_MachineHistoryList [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_MachineHistoryListBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_MachineHistoryListTemplate [LayoutElement + Image；默认inactive]
            Btn_MachineHistoryListRow [Button + Image]
              Txt_MachineHistoryListRowLabel [TextMeshProUGUI]
              Txt_MachineHistoryListRowValue [TextMeshProUGUI]
  Panel_MachineHistoryDetail [Image]
    Txt_MachineHistoryDetailHeading [TextMeshProUGUI]
    List_MachineHistoryDetail [ScrollRect vertical=true horizontal=false]
      Viewport_MachineHistoryDetail [RectMask2D]
        Content_MachineHistoryDetail [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_MachineHistoryDetailBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_MachineHistoryDetailTemplate [LayoutElement + Image；默认inactive]
            Btn_MachineHistoryDetailRow [Button + Image]
              Txt_MachineHistoryDetailRowLabel [TextMeshProUGUI]
              Txt_MachineHistoryDetailRowValue [TextMeshProUGUI]
  Grp_MachineHistoryLoadingState [无Graphic]
    Panel_MachineHistoryLoadingMessage [Image]
      Txt_MachineHistoryLoadingMessage [TextMeshProUGUI]
  Grp_MachineHistoryEmptyState [无Graphic]
    Panel_MachineHistoryEmptyMessage [Image]
      Txt_MachineHistoryEmptyMessage [TextMeshProUGUI]
  Grp_MachineHistoryErrorState [无Graphic]
    Panel_MachineHistoryErrorMessage [Image]
      Txt_MachineHistoryErrorMessage [TextMeshProUGUI]
  Grp_MachineHistorySuccessState [无Graphic]
    Panel_MachineHistorySuccessMessage [Image]
      Txt_MachineHistorySuccessMessage [TextMeshProUGUI]
  Grp_MachineHistoryDisabledState [无Graphic]
    Panel_MachineHistoryDisabledMessage [Image]
      Txt_MachineHistoryDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageMachineHistory | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；机器任务历史；内部页面根 |
| Txt_MachineHistoryTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；机器任务历史 |
| Grp_MachineHistoryActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_MachineHistoryFilter | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；按机器、结果、任务类别 |
| Txt_MachineHistoryFilterLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；筛选 |
| Btn_MachineHistoryTrace | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入13诊断或15算法运行记录 |
| Txt_MachineHistoryTraceLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看因果 |
| Btn_MachineHistoryLocate | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；失效时仅保留历史身份 |
| Txt_MachineHistoryLocateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；定位相关对象 |
| Panel_MachineHistoryList | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；任务历史 |
| Txt_MachineHistoryListHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；任务历史 |
| List_MachineHistoryList | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_MachineHistoryList | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_MachineHistoryList | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_MachineHistoryListBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；每台机器最近100条；状态；开始／结束；合并信息；主要原因 |
| Item_MachineHistoryListTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_MachineHistoryListRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_MachineHistoryListRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_MachineHistoryListRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_MachineHistoryDetail | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；任务详情 |
| Txt_MachineHistoryDetailHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；任务详情 |
| List_MachineHistoryDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_MachineHistoryDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_MachineHistoryDetail | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_MachineHistoryDetailBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；父子任务；行为阶段；实际贡献；停止原因；未解决责任；关联算法 |
| Item_MachineHistoryDetailTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_MachineHistoryDetailRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_MachineHistoryDetailRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_MachineHistoryDetailRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_MachineHistoryLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineHistoryLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineHistoryLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_MachineHistoryEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineHistoryEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineHistoryEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_MachineHistoryErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineHistoryErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineHistoryErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_MachineHistorySuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineHistorySuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineHistorySuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_MachineHistoryDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineHistoryDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineHistoryDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## AlgorithmHistory：算法运行记录

功能文档：[算法运行记录](AlgorithmHistory.md)；归属 `RecordReaderForm`；内容 1488×730。

```text
Panel_PageAlgorithmHistory [Image]
  Txt_AlgorithmHistoryTitle [TextMeshProUGUI]
  Grp_AlgorithmHistoryActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_AlgorithmHistoryFilter [Button + Image]
      Txt_AlgorithmHistoryFilterLabel [TextMeshProUGUI]
    Btn_AlgorithmHistoryDiagnose [Button + Image]
      Txt_AlgorithmHistoryDiagnoseLabel [TextMeshProUGUI]
    Btn_AlgorithmHistoryLocate [Button + Image]
      Txt_AlgorithmHistoryLocateLabel [TextMeshProUGUI]
  Panel_AlgorithmHistoryList [Image]
    Txt_AlgorithmHistoryListHeading [TextMeshProUGUI]
    List_AlgorithmHistoryList [ScrollRect vertical=true horizontal=false]
      Viewport_AlgorithmHistoryList [RectMask2D]
        Content_AlgorithmHistoryList [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_AlgorithmHistoryListBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_AlgorithmHistoryListTemplate [LayoutElement + Image；默认inactive]
            Btn_AlgorithmHistoryListRow [Button + Image]
              Txt_AlgorithmHistoryListRowLabel [TextMeshProUGUI]
              Txt_AlgorithmHistoryListRowValue [TextMeshProUGUI]
  Panel_AlgorithmHistoryDetail [Image]
    Txt_AlgorithmHistoryDetailHeading [TextMeshProUGUI]
    List_AlgorithmHistoryDetail [ScrollRect vertical=true horizontal=false]
      Viewport_AlgorithmHistoryDetail [RectMask2D]
        Content_AlgorithmHistoryDetail [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_AlgorithmHistoryDetailBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_AlgorithmHistoryDetailTemplate [LayoutElement + Image；默认inactive]
            Btn_AlgorithmHistoryDetailRow [Button + Image]
              Txt_AlgorithmHistoryDetailRowLabel [TextMeshProUGUI]
              Txt_AlgorithmHistoryDetailRowValue [TextMeshProUGUI]
  Grp_AlgorithmHistoryLoadingState [无Graphic]
    Panel_AlgorithmHistoryLoadingMessage [Image]
      Txt_AlgorithmHistoryLoadingMessage [TextMeshProUGUI]
  Grp_AlgorithmHistoryEmptyState [无Graphic]
    Panel_AlgorithmHistoryEmptyMessage [Image]
      Txt_AlgorithmHistoryEmptyMessage [TextMeshProUGUI]
  Grp_AlgorithmHistoryErrorState [无Graphic]
    Panel_AlgorithmHistoryErrorMessage [Image]
      Txt_AlgorithmHistoryErrorMessage [TextMeshProUGUI]
  Grp_AlgorithmHistorySuccessState [无Graphic]
    Panel_AlgorithmHistorySuccessMessage [Image]
      Txt_AlgorithmHistorySuccessMessage [TextMeshProUGUI]
  Grp_AlgorithmHistoryDisabledState [无Graphic]
    Panel_AlgorithmHistoryDisabledMessage [Image]
      Txt_AlgorithmHistoryDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageAlgorithmHistory | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；算法运行记录；内部页面根 |
| Txt_AlgorithmHistoryTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；算法运行记录 |
| Grp_AlgorithmHistoryActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_AlgorithmHistoryFilter | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；切换对应历史范围 |
| Txt_AlgorithmHistoryFilterLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；选择算法与结果 |
| Btn_AlgorithmHistoryDiagnose | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；13诊断模式携带运行ID和版本 |
| Txt_AlgorithmHistoryDiagnoseLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；打开诊断画布 |
| Btn_AlgorithmHistoryLocate | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；有效时聚焦 |
| Txt_AlgorithmHistoryLocateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；定位机器 |
| Panel_AlgorithmHistoryList | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；运行历史 |
| Txt_AlgorithmHistoryListHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；运行历史 |
| List_AlgorithmHistoryList | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_AlgorithmHistoryList | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_AlgorithmHistoryList | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_AlgorithmHistoryListBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；每算法最近50次；时间；结果；连续相同无行为可合并次数 |
| Item_AlgorithmHistoryListTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_AlgorithmHistoryListRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_AlgorithmHistoryListRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_AlgorithmHistoryListRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_AlgorithmHistoryDetail | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；运行摘要 |
| Txt_AlgorithmHistoryDetailHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；运行摘要 |
| List_AlgorithmHistoryDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_AlgorithmHistoryDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_AlgorithmHistoryDetail | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_AlgorithmHistoryDetailBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；当时算法版本；输入输出；分支；拒绝与等待；关联任务 |
| Item_AlgorithmHistoryDetailTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_AlgorithmHistoryDetailRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_AlgorithmHistoryDetailRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_AlgorithmHistoryDetailRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_AlgorithmHistoryLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_AlgorithmHistoryLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_AlgorithmHistoryLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_AlgorithmHistoryEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_AlgorithmHistoryEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_AlgorithmHistoryEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_AlgorithmHistoryErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_AlgorithmHistoryErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_AlgorithmHistoryErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_AlgorithmHistorySuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_AlgorithmHistorySuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_AlgorithmHistorySuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_AlgorithmHistoryDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_AlgorithmHistoryDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_AlgorithmHistoryDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## EnergyHistory：能源停机记录

功能文档：[能源停机记录](EnergyHistory.md)；归属 `RecordReaderForm`；内容 1488×730。

```text
Panel_PageEnergyHistory [Image]
  Txt_EnergyHistoryTitle [TextMeshProUGUI]
  Grp_EnergyHistoryActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_EnergyHistoryFilter [Button + Image]
      Txt_EnergyHistoryFilterLabel [TextMeshProUGUI]
    Btn_EnergyHistoryLocate [Button + Image]
      Txt_EnergyHistoryLocateLabel [TextMeshProUGUI]
    Btn_EnergyHistoryEnergy [Button + Image]
      Txt_EnergyHistoryEnergyLabel [TextMeshProUGUI]
  Panel_EnergyHistoryList [Image]
    Txt_EnergyHistoryListHeading [TextMeshProUGUI]
    List_EnergyHistoryList [ScrollRect vertical=true horizontal=false]
      Viewport_EnergyHistoryList [RectMask2D]
        Content_EnergyHistoryList [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_EnergyHistoryListBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_EnergyHistoryListTemplate [LayoutElement + Image；默认inactive]
            Btn_EnergyHistoryListRow [Button + Image]
              Txt_EnergyHistoryListRowLabel [TextMeshProUGUI]
              Txt_EnergyHistoryListRowValue [TextMeshProUGUI]
  Panel_EnergyHistoryDetail [Image]
    Txt_EnergyHistoryDetailHeading [TextMeshProUGUI]
    List_EnergyHistoryDetail [ScrollRect vertical=true horizontal=false]
      Viewport_EnergyHistoryDetail [RectMask2D]
        Content_EnergyHistoryDetail [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_EnergyHistoryDetailBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_EnergyHistoryDetailTemplate [LayoutElement + Image；默认inactive]
            Btn_EnergyHistoryDetailRow [Button + Image]
              Txt_EnergyHistoryDetailRowLabel [TextMeshProUGUI]
              Txt_EnergyHistoryDetailRowValue [TextMeshProUGUI]
  Grp_EnergyHistoryLoadingState [无Graphic]
    Panel_EnergyHistoryLoadingMessage [Image]
      Txt_EnergyHistoryLoadingMessage [TextMeshProUGUI]
  Grp_EnergyHistoryEmptyState [无Graphic]
    Panel_EnergyHistoryEmptyMessage [Image]
      Txt_EnergyHistoryEmptyMessage [TextMeshProUGUI]
  Grp_EnergyHistoryErrorState [无Graphic]
    Panel_EnergyHistoryErrorMessage [Image]
      Txt_EnergyHistoryErrorMessage [TextMeshProUGUI]
  Grp_EnergyHistorySuccessState [无Graphic]
    Panel_EnergyHistorySuccessMessage [Image]
      Txt_EnergyHistorySuccessMessage [TextMeshProUGUI]
  Grp_EnergyHistoryDisabledState [无Graphic]
    Panel_EnergyHistoryDisabledMessage [Image]
      Txt_EnergyHistoryDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageEnergyHistory | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；能源停机记录；内部页面根 |
| Txt_EnergyHistoryTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；能源停机记录 |
| Grp_EnergyHistoryActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_EnergyHistoryFilter | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；保留事件稳定ID |
| Txt_EnergyHistoryFilterLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；按设施或事件筛选 |
| Btn_EnergyHistoryLocate | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；有效时聚焦 |
| Txt_EnergyHistoryLocateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；定位设施 |
| Btn_EnergyHistoryEnergy | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；返回04能源系统详情 |
| Txt_EnergyHistoryEnergyLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；当前电网 |
| Panel_EnergyHistoryList | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；离散能源事件 |
| Txt_EnergyHistoryListHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；离散能源事件 |
| List_EnergyHistoryList | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_EnergyHistoryList | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_EnergyHistoryList | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_EnergyHistoryListBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；电量过低／耗尽；缺电停机／恢复；燃料耗尽；负载首次超供给 |
| Item_EnergyHistoryListTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_EnergyHistoryListRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_EnergyHistoryListRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_EnergyHistoryListRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_EnergyHistoryDetail | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；事件详情 |
| Txt_EnergyHistoryDetailHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；事件详情 |
| List_EnergyHistoryDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_EnergyHistoryDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_EnergyHistoryDetail | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_EnergyHistoryDetailBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；时间；对象；持续时长；原因；影响；活跃或已恢复 |
| Item_EnergyHistoryDetailTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_EnergyHistoryDetailRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_EnergyHistoryDetailRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_EnergyHistoryDetailRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_EnergyHistoryLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_EnergyHistoryLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_EnergyHistoryLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_EnergyHistoryEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_EnergyHistoryEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_EnergyHistoryEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_EnergyHistoryErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_EnergyHistoryErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_EnergyHistoryErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_EnergyHistorySuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_EnergyHistorySuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_EnergyHistorySuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_EnergyHistoryDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_EnergyHistoryDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_EnergyHistoryDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |
