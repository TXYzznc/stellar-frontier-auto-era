# 04-基地中枢 — prefab-layout

状态：2026-09-19完整设计候选，待用户集中验收；未据此修改Prefab。

继承[通用合同](../00-通用合同.md)和[共享外壳](../00-共享外壳-prefab-layout.md)。下列子树预制在所属Form的Grp_PageHost中；公共外壳由共享文档唯一声明。跨家族同属一个Form的子树合并，禁止重复根节点。表中所有Image／文本颜色、资源、射线、默认字号与状态继承通用合同，列出的数据是字段说明，不是示例业务数值。

## HubOverview：总览

功能文档：[总览](HubOverview.md)；归属 `BaseCommandHubForm`；内容 1488×730。

```text
Panel_PageHubOverview [Image]
  Txt_HubOverviewTitle [TextMeshProUGUI]
  Grp_HubOverviewActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_HubOverviewObjects [Button + Image]
      Txt_HubOverviewObjectsLabel [TextMeshProUGUI]
    Btn_HubOverviewEnergy [Button + Image]
      Txt_HubOverviewEnergyLabel [TextMeshProUGUI]
    Btn_HubOverviewTasks [Button + Image]
      Txt_HubOverviewTasksLabel [TextMeshProUGUI]
    Btn_HubOverviewStats [Button + Image]
      Txt_HubOverviewStatsLabel [TextMeshProUGUI]
    Btn_HubOverviewSupply [Button + Image]
      Txt_HubOverviewSupplyLabel [TextMeshProUGUI]
  Panel_HubOverviewEconomy [Image]
    Txt_HubOverviewEconomyHeading [TextMeshProUGUI]
    List_HubOverviewEconomy [ScrollRect vertical=true horizontal=false]
      Viewport_HubOverviewEconomy [RectMask2D]
        Content_HubOverviewEconomy [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_HubOverviewEconomyBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_HubOverviewEconomyTemplate [LayoutElement + Image；默认inactive]
            Btn_HubOverviewEconomyRow [Button + Image]
              Txt_HubOverviewEconomyRowLabel [TextMeshProUGUI]
              Txt_HubOverviewEconomyRowValue [TextMeshProUGUI]
  Panel_HubOverviewOperation [Image]
    Txt_HubOverviewOperationHeading [TextMeshProUGUI]
    List_HubOverviewOperation [ScrollRect vertical=true horizontal=false]
      Viewport_HubOverviewOperation [RectMask2D]
        Content_HubOverviewOperation [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_HubOverviewOperationBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_HubOverviewOperationTemplate [LayoutElement + Image；默认inactive]
            Btn_HubOverviewOperationRow [Button + Image]
              Txt_HubOverviewOperationRowLabel [TextMeshProUGUI]
              Txt_HubOverviewOperationRowValue [TextMeshProUGUI]
  Panel_HubOverviewAttention [Image]
    Txt_HubOverviewAttentionHeading [TextMeshProUGUI]
    List_HubOverviewAttention [ScrollRect vertical=true horizontal=false]
      Viewport_HubOverviewAttention [RectMask2D]
        Content_HubOverviewAttention [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_HubOverviewAttentionBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_HubOverviewAttentionTemplate [LayoutElement + Image；默认inactive]
            Btn_HubOverviewAttentionRow [Button + Image]
              Txt_HubOverviewAttentionRowLabel [TextMeshProUGUI]
              Txt_HubOverviewAttentionRowValue [TextMeshProUGUI]
  Grp_HubOverviewLoadingState [无Graphic]
    Panel_HubOverviewLoadingMessage [Image]
      Txt_HubOverviewLoadingMessage [TextMeshProUGUI]
  Grp_HubOverviewEmptyState [无Graphic]
    Panel_HubOverviewEmptyMessage [Image]
      Txt_HubOverviewEmptyMessage [TextMeshProUGUI]
  Grp_HubOverviewErrorState [无Graphic]
    Panel_HubOverviewErrorMessage [Image]
      Txt_HubOverviewErrorMessage [TextMeshProUGUI]
  Grp_HubOverviewSuccessState [无Graphic]
    Panel_HubOverviewSuccessMessage [Image]
      Txt_HubOverviewSuccessMessage [TextMeshProUGUI]
  Grp_HubOverviewDisabledState [无Graphic]
    Panel_HubOverviewDisabledMessage [Image]
      Txt_HubOverviewDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageHubOverview | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；总览；内部页面根 |
| Txt_HubOverviewTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；总览 |
| Grp_HubOverviewActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_HubOverviewObjects | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；到04-对象与系统并传入状态筛选 |
| Txt_HubOverviewObjectsLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看运行对象 |
| Btn_HubOverviewEnergy | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；到04-能源系统详情 |
| Txt_HubOverviewEnergyLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看能源 |
| Btn_HubOverviewTasks | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；到04-待处理任务 |
| Txt_HubOverviewTasksLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看待办 |
| Btn_HubOverviewStats | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；到04-统计并保留选中指标 |
| Txt_HubOverviewStatsLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看统计 |
| Btn_HubOverviewSupply | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开18-日常补给面板 |
| Txt_HubOverviewSupplyLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；领取日常补给 |
| Panel_HubOverviewEconomy | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(0,-36) | absolute | Image；基地与资源 |
| Txt_HubOverviewEconomyHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；基地与资源 |
| List_HubOverviewEconomy | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_HubOverviewEconomy | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_HubOverviewEconomy | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_HubOverviewEconomyBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；中枢等级／经验；五种通用资源与净增速；当前主线阶段 |
| Item_HubOverviewEconomyTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_HubOverviewEconomyRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_HubOverviewEconomyRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_HubOverviewEconomyRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_HubOverviewOperation | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(501,-36) | absolute | Image；运行与生产 |
| Txt_HubOverviewOperationHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；运行与生产 |
| List_HubOverviewOperation | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_HubOverviewOperation | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_HubOverviewOperation | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_HubOverviewOperationBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；机器运行／休眠／异常数量；本机算力汇总及高负载／过载数量；能源供需和蓄电；生产摘要 |
| Item_HubOverviewOperationTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_HubOverviewOperationRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_HubOverviewOperationRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_HubOverviewOperationRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_HubOverviewAttention | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(1002,-36) | absolute | Image；近期需要关注 |
| Txt_HubOverviewAttentionHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；近期需要关注 |
| List_HubOverviewAttention | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_HubOverviewAttention | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_HubOverviewAttention | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_HubOverviewAttentionBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；最近活跃警报；主线进度；日常补给是否可领；无异常提示 |
| Item_HubOverviewAttentionTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_HubOverviewAttentionRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_HubOverviewAttentionRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_HubOverviewAttentionRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_HubOverviewLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubOverviewLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubOverviewLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_HubOverviewEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubOverviewEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubOverviewEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_HubOverviewErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubOverviewErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubOverviewErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_HubOverviewSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubOverviewSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubOverviewSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_HubOverviewDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubOverviewDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubOverviewDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## HubTasks：待处理任务

功能文档：[待处理任务](HubTasks.md)；归属 `BaseCommandHubForm`；内容 1488×730。

```text
Panel_PageHubTasks [Image]
  Txt_HubTasksTitle [TextMeshProUGUI]
  Grp_HubTasksActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_HubTasksFilter [Button + Image]
      Txt_HubTasksFilterLabel [TextMeshProUGUI]
    Btn_HubTasksInspect [Button + Image]
      Txt_HubTasksInspectLabel [TextMeshProUGUI]
    Btn_HubTasksLocate [Button + Image]
      Txt_HubTasksLocateLabel [TextMeshProUGUI]
    Btn_HubTasksAction [Button + Image]
      Txt_HubTasksActionLabel [TextMeshProUGUI]
  Panel_HubTasksQueue [Image]
    Txt_HubTasksQueueHeading [TextMeshProUGUI]
    List_HubTasksQueue [ScrollRect vertical=true horizontal=false]
      Viewport_HubTasksQueue [RectMask2D]
        Content_HubTasksQueue [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_HubTasksQueueBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_HubTasksQueueTemplate [LayoutElement + Image；默认inactive]
            Btn_HubTasksQueueRow [Button + Image]
              Txt_HubTasksQueueRowLabel [TextMeshProUGUI]
              Txt_HubTasksQueueRowValue [TextMeshProUGUI]
  Panel_HubTasksDetail [Image]
    Txt_HubTasksDetailHeading [TextMeshProUGUI]
    List_HubTasksDetail [ScrollRect vertical=true horizontal=false]
      Viewport_HubTasksDetail [RectMask2D]
        Content_HubTasksDetail [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_HubTasksDetailBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_HubTasksDetailTemplate [LayoutElement + Image；默认inactive]
            Btn_HubTasksDetailRow [Button + Image]
              Txt_HubTasksDetailRowLabel [TextMeshProUGUI]
              Txt_HubTasksDetailRowValue [TextMeshProUGUI]
  Grp_HubTasksLoadingState [无Graphic]
    Panel_HubTasksLoadingMessage [Image]
      Txt_HubTasksLoadingMessage [TextMeshProUGUI]
  Grp_HubTasksEmptyState [无Graphic]
    Panel_HubTasksEmptyMessage [Image]
      Txt_HubTasksEmptyMessage [TextMeshProUGUI]
  Grp_HubTasksErrorState [无Graphic]
    Panel_HubTasksErrorMessage [Image]
      Txt_HubTasksErrorMessage [TextMeshProUGUI]
  Grp_HubTasksSuccessState [无Graphic]
    Panel_HubTasksSuccessMessage [Image]
      Txt_HubTasksSuccessMessage [TextMeshProUGUI]
  Grp_HubTasksDisabledState [无Graphic]
    Panel_HubTasksDisabledMessage [Image]
      Txt_HubTasksDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageHubTasks | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；待处理任务；内部页面根 |
| Txt_HubTasksTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；待处理任务 |
| Grp_HubTasksActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_HubTasksFilter | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；切换事项类型并保留独立滚动位置 |
| Txt_HubTasksFilterLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；筛选待办 |
| Btn_HubTasksInspect | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；主线→15-主线任务；警报→15-警报详情；等待→18-安全停止等待 |
| Txt_HubTasksInspectLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看相关详情 |
| Btn_HubTasksLocate | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；关闭中枢并聚焦有效对象 |
| Txt_HubTasksLocateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；定位来源 |
| Btn_HubTasksAction | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；仅路由到对应业务确认／详情，不下达机器作业、不调整队列优先级 |
| Txt_HubTasksActionLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；执行对应操作 |
| Panel_HubTasksQueue | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；待办列表 |
| Txt_HubTasksQueueHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；待办列表 |
| List_HubTasksQueue | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_HubTasksQueue | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_HubTasksQueue | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_HubTasksQueueBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；主线目标／待领取／活跃警报／生产阻塞／安全停止等待分类；严重度；来源；更新时间 |
| Item_HubTasksQueueTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_HubTasksQueueRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_HubTasksQueueRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_HubTasksQueueRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_HubTasksDetail | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；选中事项 |
| Txt_HubTasksDetailHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；选中事项 |
| List_HubTasksDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_HubTasksDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_HubTasksDetail | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_HubTasksDetailBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；发生原因；影响；当前阶段；可执行下一步；关联对象及操作权限 |
| Item_HubTasksDetailTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_HubTasksDetailRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_HubTasksDetailRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_HubTasksDetailRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_HubTasksLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubTasksLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubTasksLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_HubTasksEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubTasksEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubTasksEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_HubTasksErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubTasksErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubTasksErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_HubTasksSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubTasksSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubTasksSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_HubTasksDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubTasksDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubTasksDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## HubObjects：对象与系统

功能文档：[对象与系统](HubObjects.md)；归属 `BaseCommandHubForm`；内容 1488×730。

```text
Panel_PageHubObjects [Image]
  Txt_HubObjectsTitle [TextMeshProUGUI]
  Grp_HubObjectsActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_HubObjectsFilter [Button + Image]
      Txt_HubObjectsFilterLabel [TextMeshProUGUI]
    Btn_HubObjectsDetails [Button + Image]
      Txt_HubObjectsDetailsLabel [TextMeshProUGUI]
    Btn_HubObjectsLocate [Button + Image]
      Txt_HubObjectsLocateLabel [TextMeshProUGUI]
  Panel_HubObjectsIndex [Image]
    Txt_HubObjectsIndexHeading [TextMeshProUGUI]
    List_HubObjectsIndex [ScrollRect vertical=true horizontal=false]
      Viewport_HubObjectsIndex [RectMask2D]
        Content_HubObjectsIndex [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_HubObjectsIndexBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_HubObjectsIndexTemplate [LayoutElement + Image；默认inactive]
            Btn_HubObjectsIndexRow [Button + Image]
              Txt_HubObjectsIndexRowLabel [TextMeshProUGUI]
              Txt_HubObjectsIndexRowValue [TextMeshProUGUI]
          Panel_HubObjectsControls [Image + LayoutElement]
            Panel_HubObjectsSearch [Image + TMP_InputField]
              Grp_HubObjectsSearchTextViewport [RectMask2D]
                Txt_HubObjectsSearchValue [TextMeshProUGUI]
                Txt_HubObjectsSearchPlaceholder [TextMeshProUGUI]
  Panel_HubObjectsDetail [Image]
    Txt_HubObjectsDetailHeading [TextMeshProUGUI]
    List_HubObjectsDetail [ScrollRect vertical=true horizontal=false]
      Viewport_HubObjectsDetail [RectMask2D]
        Content_HubObjectsDetail [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_HubObjectsDetailBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_HubObjectsDetailTemplate [LayoutElement + Image；默认inactive]
            Btn_HubObjectsDetailRow [Button + Image]
              Txt_HubObjectsDetailRowLabel [TextMeshProUGUI]
              Txt_HubObjectsDetailRowValue [TextMeshProUGUI]
  Grp_HubObjectsLoadingState [无Graphic]
    Panel_HubObjectsLoadingMessage [Image]
      Txt_HubObjectsLoadingMessage [TextMeshProUGUI]
  Grp_HubObjectsEmptyState [无Graphic]
    Panel_HubObjectsEmptyMessage [Image]
      Txt_HubObjectsEmptyMessage [TextMeshProUGUI]
  Grp_HubObjectsErrorState [无Graphic]
    Panel_HubObjectsErrorMessage [Image]
      Txt_HubObjectsErrorMessage [TextMeshProUGUI]
  Grp_HubObjectsSuccessState [无Graphic]
    Panel_HubObjectsSuccessMessage [Image]
      Txt_HubObjectsSuccessMessage [TextMeshProUGUI]
  Grp_HubObjectsDisabledState [无Graphic]
    Panel_HubObjectsDisabledMessage [Image]
      Txt_HubObjectsDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageHubObjects | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；对象与系统；内部页面根 |
| Txt_HubObjectsTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；对象与系统 |
| Grp_HubObjectsActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_HubObjectsFilter | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；通过输入区与分类按钮筛选，实时更新不重置选中项 |
| Txt_HubObjectsFilterLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；搜索与筛选 |
| Btn_HubObjectsDetails | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；机器→05-中枢机器详情；能源→04-能源系统详情；其它对象打开同中枢内远程只读详情 |
| Txt_HubObjectsDetailsLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看完整详情 |
| Btn_HubObjectsLocate | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；聚焦对应对象，现场条件满足后打开06或07的现场面板 |
| Txt_HubObjectsLocateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；前往现场 |
| Panel_HubObjectsIndex | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；对象索引 |
| Txt_HubObjectsIndexHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；对象索引 |
| List_HubObjectsIndex | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_HubObjectsIndex | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_HubObjectsIndex | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_HubObjectsIndexBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；机器／建筑／资源点／能源系统分类；名称搜索；状态筛选；状态、任务、位置摘要 |
| Item_HubObjectsIndexTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_HubObjectsIndexRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_HubObjectsIndexRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_HubObjectsIndexRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_HubObjectsControls | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,88)初始化; pos(0,0)初始化; LayoutElement preferred(541,88); 最终位置/尺寸由组驱动 | group | Image + LayoutElement；真实输入字段 |
| Panel_HubObjectsSearch | min(0,1) max(0,1); pivot(0,1); sizeDelta(509,48); pos(16,-8) | absolute | Image + TMP_InputField；按名称搜索；onEndEdit验证 |
| Grp_HubObjectsSearchTextViewport | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-8); pos(0,0) | absolute | RectMask2D；textViewport，无Graphic |
| Txt_HubObjectsSearchValue | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；— |
| Txt_HubObjectsSearchPlaceholder | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；按名称搜索 |
| Panel_HubObjectsDetail | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；对象摘要 |
| Txt_HubObjectsDetailHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；对象摘要 |
| List_HubObjectsDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_HubObjectsDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_HubObjectsDetail | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_HubObjectsDetailBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；名称和类型；当前状态；主要等待原因；可见数据来源与采样时间；权限提示 |
| Item_HubObjectsDetailTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_HubObjectsDetailRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_HubObjectsDetailRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_HubObjectsDetailRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_HubObjectsLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubObjectsLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubObjectsLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_HubObjectsEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubObjectsEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubObjectsEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_HubObjectsErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubObjectsErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubObjectsErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_HubObjectsSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubObjectsSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubObjectsSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_HubObjectsDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubObjectsDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubObjectsDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## HubEnergy：能源系统详情

功能文档：[能源系统详情](HubEnergy.md)；归属 `BaseCommandHubForm`；内容 1488×730。

```text
Panel_PageHubEnergy [Image]
  Txt_HubEnergyTitle [TextMeshProUGUI]
  Grp_HubEnergyActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_HubEnergyConfigure [Button + Image]
      Txt_HubEnergyConfigureLabel [TextMeshProUGUI]
    Btn_HubEnergyLocate [Button + Image]
      Txt_HubEnergyLocateLabel [TextMeshProUGUI]
    Btn_HubEnergyHistory [Button + Image]
      Txt_HubEnergyHistoryLabel [TextMeshProUGUI]
  Panel_HubEnergySummary [Image]
    Txt_HubEnergySummaryHeading [TextMeshProUGUI]
    List_HubEnergySummary [ScrollRect vertical=true horizontal=false]
      Viewport_HubEnergySummary [RectMask2D]
        Content_HubEnergySummary [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_HubEnergySummaryBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_HubEnergySummaryTemplate [LayoutElement + Image；默认inactive]
            Btn_HubEnergySummaryRow [Button + Image]
              Txt_HubEnergySummaryRowLabel [TextMeshProUGUI]
              Txt_HubEnergySummaryRowValue [TextMeshProUGUI]
          Panel_HubEnergyControls [Image + LayoutElement]
            Tgl_HubEnergyChargingAllowed [Toggle]
              Img_HubEnergyChargingAllowedBox [Image]
                Icon_HubEnergyChargingAllowedCheck [Image]
              Txt_HubEnergyChargingAllowedLabel [TextMeshProUGUI]
            Sld_HubEnergyChargeTarget [Slider]
              Bg_HubEnergyChargeTargetTrack [Image]
              Bar_HubEnergyChargeTargetFill [Image(Filled)]
              Img_HubEnergyChargeTargetHandle [Image]
              Txt_HubEnergyChargeTargetValue [TextMeshProUGUI]
  Panel_HubEnergyFacilities [Image]
    Txt_HubEnergyFacilitiesHeading [TextMeshProUGUI]
    List_HubEnergyFacilities [ScrollRect vertical=true horizontal=false]
      Viewport_HubEnergyFacilities [RectMask2D]
        Content_HubEnergyFacilities [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_HubEnergyFacilitiesBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_HubEnergyFacilitiesTemplate [LayoutElement + Image；默认inactive]
            Btn_HubEnergyFacilitiesRow [Button + Image]
              Txt_HubEnergyFacilitiesRowLabel [TextMeshProUGUI]
              Txt_HubEnergyFacilitiesRowValue [TextMeshProUGUI]
  Panel_HubEnergyConsumers [Image]
    Txt_HubEnergyConsumersHeading [TextMeshProUGUI]
    List_HubEnergyConsumers [ScrollRect vertical=true horizontal=false]
      Viewport_HubEnergyConsumers [RectMask2D]
        Content_HubEnergyConsumers [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_HubEnergyConsumersBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_HubEnergyConsumersTemplate [LayoutElement + Image；默认inactive]
            Btn_HubEnergyConsumersRow [Button + Image]
              Txt_HubEnergyConsumersRowLabel [TextMeshProUGUI]
              Txt_HubEnergyConsumersRowValue [TextMeshProUGUI]
  Grp_HubEnergyLoadingState [无Graphic]
    Panel_HubEnergyLoadingMessage [Image]
      Txt_HubEnergyLoadingMessage [TextMeshProUGUI]
  Grp_HubEnergyEmptyState [无Graphic]
    Panel_HubEnergyEmptyMessage [Image]
      Txt_HubEnergyEmptyMessage [TextMeshProUGUI]
  Grp_HubEnergyErrorState [无Graphic]
    Panel_HubEnergyErrorMessage [Image]
      Txt_HubEnergyErrorMessage [TextMeshProUGUI]
  Grp_HubEnergySuccessState [无Graphic]
    Panel_HubEnergySuccessMessage [Image]
      Txt_HubEnergySuccessMessage [TextMeshProUGUI]
  Grp_HubEnergyDisabledState [无Graphic]
    Panel_HubEnergyDisabledMessage [Image]
      Txt_HubEnergyDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageHubEnergy | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；能源系统详情；内部页面根 |
| Txt_HubEnergyTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；能源系统详情 |
| Grp_HubEnergyActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_HubEnergyConfigure | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；仅燃料设施开放充电许可与目标比例，使用Toggle＋百分比滑条；最终提交再验权限 |
| Txt_HubEnergyConfigureLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；配置选中发电设施 |
| Btn_HubEnergyLocate | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入世界对应现场 |
| Txt_HubEnergyLocateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；定位设施 |
| Btn_HubEnergyHistory | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入15-能源停机记录 |
| Txt_HubEnergyHistoryLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看能源事件 |
| Panel_HubEnergySummary | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(0,-36) | absolute | Image；供需概要 |
| Txt_HubEnergySummaryHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；供需概要 |
| List_HubEnergySummary | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_HubEnergySummary | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_HubEnergySummary | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_HubEnergySummaryBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；发电；用电；净功率；当前储电／上限；充满／耗尽估算；缺电停机数 |
| Item_HubEnergySummaryTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_HubEnergySummaryRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_HubEnergySummaryRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_HubEnergySummaryRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_HubEnergyControls | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,160)初始化; pos(0,0)初始化; LayoutElement preferred(461,160); 最终位置/尺寸由组驱动 | group | Image + LayoutElement；真实输入字段 |
| Tgl_HubEnergyChargingAllowed | min(0,1) max(0,1); pivot(0,1); sizeDelta(429,48); pos(16,-8) | absolute | Toggle；所选燃料设施充电许可 |
| Img_HubEnergyChargingAllowedBox | min(0,1) max(0,1); pivot(0,1); sizeDelta(32,32); pos(0,-8) | absolute | Image；开关背景 |
| Icon_HubEnergyChargingAllowedCheck | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；Toggle.graphic |
| Txt_HubEnergyChargingAllowedLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-48,0); pos(24,0) | absolute | TextMeshProUGUI；所选燃料设施充电许可 |
| Sld_HubEnergyChargeTarget | min(0,1) max(0,1); pivot(0,1); sizeDelta(429,56); pos(16,-80) | absolute | Slider；目标储电比例 |
| Bg_HubEnergyChargeTargetTrack | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,8); pos(0,8) | absolute | Image；轨道 |
| Bar_HubEnergyChargeTargetFill | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,8); pos(0,8) | absolute | Image(Filled)；Slider.fillRect驱动 |
| Img_HubEnergyChargeTargetHandle | min(0,0) max(0,0); pivot(0.5,0.5); sizeDelta(24,24); pos(12,12) | absolute | Image；Slider.handleRect驱动位置 |
| Txt_HubEnergyChargeTargetValue | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,28); pos(0,0) | absolute | TextMeshProUGUI；目标储电比例：— |
| Panel_HubEnergyFacilities | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(501,-36) | absolute | Image；发电与蓄电设施 |
| Txt_HubEnergyFacilitiesHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；发电与蓄电设施 |
| List_HubEnergyFacilities | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_HubEnergyFacilities | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_HubEnergyFacilities | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_HubEnergyFacilitiesBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；名称；额定／实际功率；燃料与预计维持时间；充放电状态；充电许可和目标比例 |
| Item_HubEnergyFacilitiesTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_HubEnergyFacilitiesRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_HubEnergyFacilitiesRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_HubEnergyFacilitiesRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_HubEnergyConsumers | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(1002,-36) | absolute | Image；用电对象 |
| Txt_HubEnergyConsumersHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；用电对象 |
| List_HubEnergyConsumers | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_HubEnergyConsumers | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_HubEnergyConsumers | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_HubEnergyConsumersBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；机器／建筑分组；状态；功率；固定优先级；缺电状态；定位 |
| Item_HubEnergyConsumersTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_HubEnergyConsumersRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_HubEnergyConsumersRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_HubEnergyConsumersRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_HubEnergyLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubEnergyLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubEnergyLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_HubEnergyEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubEnergyEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubEnergyEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_HubEnergyErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubEnergyErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubEnergyErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_HubEnergySuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubEnergySuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubEnergySuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_HubEnergyDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubEnergyDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubEnergyDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## HubRules：规则自动化

功能文档：[规则自动化](HubRules.md)；归属 `BaseCommandHubForm`；内容 1488×730。

```text
Panel_PageHubRules [Image]
  Txt_HubRulesTitle [TextMeshProUGUI]
  Grp_HubRulesActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_HubRulesSelect [Button + Image]
      Txt_HubRulesSelectLabel [TextMeshProUGUI]
    Btn_HubRulesEdit [Button + Image]
      Txt_HubRulesEditLabel [TextMeshProUGUI]
    Btn_HubRulesApply [Button + Image]
      Txt_HubRulesApplyLabel [TextMeshProUGUI]
    Btn_HubRulesToggle [Button + Image]
      Txt_HubRulesToggleLabel [TextMeshProUGUI]
    Btn_HubRulesLibrary [Button + Image]
      Txt_HubRulesLibraryLabel [TextMeshProUGUI]
  Panel_HubRulesInstances [Image]
    Txt_HubRulesInstancesHeading [TextMeshProUGUI]
    List_HubRulesInstances [ScrollRect vertical=true horizontal=false]
      Viewport_HubRulesInstances [RectMask2D]
        Content_HubRulesInstances [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_HubRulesInstancesBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_HubRulesInstancesTemplate [LayoutElement + Image；默认inactive]
            Btn_HubRulesInstancesRow [Button + Image]
              Txt_HubRulesInstancesRowLabel [TextMeshProUGUI]
              Txt_HubRulesInstancesRowValue [TextMeshProUGUI]
  Panel_HubRulesConfiguration [Image]
    Txt_HubRulesConfigurationHeading [TextMeshProUGUI]
    List_HubRulesConfiguration [ScrollRect vertical=true horizontal=false]
      Viewport_HubRulesConfiguration [RectMask2D]
        Content_HubRulesConfiguration [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_HubRulesConfigurationBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_HubRulesConfigurationTemplate [LayoutElement + Image；默认inactive]
            Btn_HubRulesConfigurationRow [Button + Image]
              Txt_HubRulesConfigurationRowLabel [TextMeshProUGUI]
              Txt_HubRulesConfigurationRowValue [TextMeshProUGUI]
  Panel_HubRulesImpact [Image]
    Txt_HubRulesImpactHeading [TextMeshProUGUI]
    List_HubRulesImpact [ScrollRect vertical=true horizontal=false]
      Viewport_HubRulesImpact [RectMask2D]
        Content_HubRulesImpact [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_HubRulesImpactBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_HubRulesImpactTemplate [LayoutElement + Image；默认inactive]
            Btn_HubRulesImpactRow [Button + Image]
              Txt_HubRulesImpactRowLabel [TextMeshProUGUI]
              Txt_HubRulesImpactRowValue [TextMeshProUGUI]
  Grp_HubRulesLoadingState [无Graphic]
    Panel_HubRulesLoadingMessage [Image]
      Txt_HubRulesLoadingMessage [TextMeshProUGUI]
  Grp_HubRulesEmptyState [无Graphic]
    Panel_HubRulesEmptyMessage [Image]
      Txt_HubRulesEmptyMessage [TextMeshProUGUI]
  Grp_HubRulesErrorState [无Graphic]
    Panel_HubRulesErrorMessage [Image]
      Txt_HubRulesErrorMessage [TextMeshProUGUI]
  Grp_HubRulesSuccessState [无Graphic]
    Panel_HubRulesSuccessMessage [Image]
      Txt_HubRulesSuccessMessage [TextMeshProUGUI]
  Grp_HubRulesDisabledState [无Graphic]
    Panel_HubRulesDisabledMessage [Image]
      Txt_HubRulesDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageHubRules | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；规则自动化；内部页面根 |
| Txt_HubRulesTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；规则自动化 |
| Grp_HubRulesActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_HubRulesSelect | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；更新配置和影响，实例不等同算法模板 |
| Txt_HubRulesSelectLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；选择自动化实例 |
| Btn_HubRulesEdit | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入13-编辑模式，关闭／覆盖中枢后保存返回上下文 |
| Txt_HubRulesEditLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；打开算法工作台 |
| Btn_HubRulesApply | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入17-算法应用确认；验证成功提交安全点生效 |
| Txt_HubRulesApplyLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；应用参数修改 |
| Btn_HubRulesToggle | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；按算法服务权限操作；存在危险影响时使用17-影响确认 |
| Txt_HubRulesToggleLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；启用或停用实例 |
| Btn_HubRulesLibrary | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入09-系统模板或玩家模板 |
| Txt_HubRulesLibraryLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；算法库 |
| Panel_HubRulesInstances | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(0,-36) | absolute | Image；自动化实例索引 |
| Txt_HubRulesInstancesHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；自动化实例索引 |
| List_HubRulesInstances | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_HubRulesInstances | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_HubRulesInstances | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_HubRulesInstancesBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；机器／算法名称；启用与有效状态；成本；最近运行；异常与能力缺失 |
| Item_HubRulesInstancesTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_HubRulesInstancesRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_HubRulesInstancesRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_HubRulesInstancesRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_HubRulesConfiguration | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(501,-36) | absolute | Image；选中实例配置 |
| Txt_HubRulesConfigurationHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；选中实例配置 |
| List_HubRulesConfiguration | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_HubRulesConfiguration | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_HubRulesConfiguration | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_HubRulesConfigurationBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；公开参数及合法区间；实际绑定；旧版本与草稿区别；权限 |
| Item_HubRulesConfigurationTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_HubRulesConfigurationRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_HubRulesConfigurationRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_HubRulesConfigurationRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_HubRulesImpact | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(1002,-36) | absolute | Image；影响预览 |
| Txt_HubRulesImpactHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；影响预览 |
| List_HubRulesImpact | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_HubRulesImpact | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_HubRulesImpact | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_HubRulesImpactBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；受影响机器／组件／对象；校验错误与警告；应用安全点；状态重置影响 |
| Item_HubRulesImpactTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_HubRulesImpactRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_HubRulesImpactRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_HubRulesImpactRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_HubRulesLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubRulesLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubRulesLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_HubRulesEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubRulesEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubRulesEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_HubRulesErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubRulesErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubRulesErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_HubRulesSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubRulesSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubRulesSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_HubRulesDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubRulesDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubRulesDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## HubStats：统计

功能文档：[统计](HubStats.md)；归属 `BaseCommandHubForm`；内容 1488×730。

```text
Panel_PageHubStats [Image]
  Txt_HubStatsTitle [TextMeshProUGUI]
  Grp_HubStatsActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_HubStatsMetric [Button + Image]
      Txt_HubStatsMetricLabel [TextMeshProUGUI]
    Btn_HubStatsRecord [Button + Image]
      Txt_HubStatsRecordLabel [TextMeshProUGUI]
    Btn_HubStatsLocate [Button + Image]
      Txt_HubStatsLocateLabel [TextMeshProUGUI]
  Panel_HubStatsMetrics [Image]
    Txt_HubStatsMetricsHeading [TextMeshProUGUI]
    List_HubStatsMetrics [ScrollRect vertical=true horizontal=false]
      Viewport_HubStatsMetrics [RectMask2D]
        Content_HubStatsMetrics [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_HubStatsMetricsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_HubStatsMetricsTemplate [LayoutElement + Image；默认inactive]
            Btn_HubStatsMetricsRow [Button + Image]
              Txt_HubStatsMetricsRowLabel [TextMeshProUGUI]
              Txt_HubStatsMetricsRowValue [TextMeshProUGUI]
  Panel_HubStatsValues [Image]
    Txt_HubStatsValuesHeading [TextMeshProUGUI]
    List_HubStatsValues [ScrollRect vertical=true horizontal=false]
      Viewport_HubStatsValues [RectMask2D]
        Content_HubStatsValues [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_HubStatsValuesBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_HubStatsValuesTemplate [LayoutElement + Image；默认inactive]
            Btn_HubStatsValuesRow [Button + Image]
              Txt_HubStatsValuesRowLabel [TextMeshProUGUI]
              Txt_HubStatsValuesRowValue [TextMeshProUGUI]
  Panel_HubStatsRecords [Image]
    Txt_HubStatsRecordsHeading [TextMeshProUGUI]
    List_HubStatsRecords [ScrollRect vertical=true horizontal=false]
      Viewport_HubStatsRecords [RectMask2D]
        Content_HubStatsRecords [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_HubStatsRecordsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_HubStatsRecordsTemplate [LayoutElement + Image；默认inactive]
            Btn_HubStatsRecordsRow [Button + Image]
              Txt_HubStatsRecordsRowLabel [TextMeshProUGUI]
              Txt_HubStatsRecordsRowValue [TextMeshProUGUI]
  Grp_HubStatsLoadingState [无Graphic]
    Panel_HubStatsLoadingMessage [Image]
      Txt_HubStatsLoadingMessage [TextMeshProUGUI]
  Grp_HubStatsEmptyState [无Graphic]
    Panel_HubStatsEmptyMessage [Image]
      Txt_HubStatsEmptyMessage [TextMeshProUGUI]
  Grp_HubStatsErrorState [无Graphic]
    Panel_HubStatsErrorMessage [Image]
      Txt_HubStatsErrorMessage [TextMeshProUGUI]
  Grp_HubStatsSuccessState [无Graphic]
    Panel_HubStatsSuccessMessage [Image]
      Txt_HubStatsSuccessMessage [TextMeshProUGUI]
  Grp_HubStatsDisabledState [无Graphic]
    Panel_HubStatsDisabledMessage [Image]
      Txt_HubStatsDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageHubStats | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；统计；内部页面根 |
| Txt_HubStatsTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；统计 |
| Grp_HubStatsActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_HubStatsMetric | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；仅选择服务真实保留的范围，数据不足不连假曲线 |
| Txt_HubStatsMetricLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；选择指标与范围 |
| Btn_HubStatsRecord | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；嵌入15对应阅读页或18回归报告，只读重看 |
| Txt_HubStatsRecordLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；打开对应记录 |
| Btn_HubStatsLocate | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；对象仍存在则进入世界，否则保留记录并说明 |
| Txt_HubStatsLocateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；定位关联对象 |
| Panel_HubStatsMetrics | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(0,-36) | absolute | Image；指标选择 |
| Txt_HubStatsMetricsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；指标选择 |
| List_HubStatsMetrics | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_HubStatsMetrics | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_HubStatsMetrics | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_HubStatsMetricsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；资源净变化与稳定窗口净增速；生产完成量；机器运行与异常；能源事件概要 |
| Item_HubStatsMetricsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_HubStatsMetricsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_HubStatsMetricsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_HubStatsMetricsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_HubStatsValues | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(501,-36) | absolute | Image；指标明细 |
| Txt_HubStatsValuesHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；指标明细 |
| List_HubStatsValues | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_HubStatsValues | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_HubStatsValues | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_HubStatsValuesBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；统计时间范围；当前值；可用历史样本；无样本说明；对象分组 |
| Item_HubStatsValuesTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_HubStatsValuesRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_HubStatsValuesRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_HubStatsValuesRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_HubStatsRecords | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(1002,-36) | absolute | Image；记录入口 |
| Txt_HubStatsRecordsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；记录入口 |
| List_HubStatsRecords | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_HubStatsRecords | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_HubStatsRecords | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_HubStatsRecordsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；警报历史；机器任务历史；算法运行记录；能源停机记录；离线回归报告 |
| Item_HubStatsRecordsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_HubStatsRecordsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_HubStatsRecordsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_HubStatsRecordsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_HubStatsLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubStatsLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubStatsLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_HubStatsEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubStatsEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubStatsEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_HubStatsErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubStatsErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubStatsErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_HubStatsSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubStatsSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubStatsSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_HubStatsDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HubStatsDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HubStatsDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |
