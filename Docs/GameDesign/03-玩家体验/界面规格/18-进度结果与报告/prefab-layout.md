# 18-进度结果与报告 — prefab-layout

状态：2026-09-19完整设计候选，待用户集中验收；未据此修改Prefab。

继承[通用合同](../00-通用合同.md)和[共享外壳](../00-共享外壳-prefab-layout.md)。下列子树预制在所属Form的Grp_PageHost中；公共外壳由共享文档唯一声明。跨家族同属一个Form的子树合并，禁止重复根节点。表中所有Image／文本颜色、资源、射线、默认字号与状态继承通用合同，列出的数据是字段说明，不是示例业务数值。

## Loading：加载

功能文档：[加载](Loading.md)；归属 `ProgressReportForm`；内容 1168×730。

```text
Panel_PageLoading [Image]
  Txt_LoadingTitle [TextMeshProUGUI]
  Grp_LoadingActions [GridLayoutGroup fixedColumns=6 cell=(185,48) spacing=(8,8)]
    Btn_LoadingRetry [Button + Image]
      Txt_LoadingRetryLabel [TextMeshProUGUI]
    Btn_LoadingReturn [Button + Image]
      Txt_LoadingReturnLabel [TextMeshProUGUI]
  Panel_LoadingStage [Image]
    Txt_LoadingStageHeading [TextMeshProUGUI]
    List_LoadingStage [ScrollRect vertical=true horizontal=false]
      Viewport_LoadingStage [RectMask2D]
        Content_LoadingStage [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_LoadingStageBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_LoadingStageTemplate [LayoutElement + Image；默认inactive]
            Btn_LoadingStageRow [Button + Image]
              Txt_LoadingStageRowLabel [TextMeshProUGUI]
              Txt_LoadingStageRowValue [TextMeshProUGUI]
  Panel_LoadingFailure [Image]
    Txt_LoadingFailureHeading [TextMeshProUGUI]
    List_LoadingFailure [ScrollRect vertical=true horizontal=false]
      Viewport_LoadingFailure [RectMask2D]
        Content_LoadingFailure [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_LoadingFailureBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_LoadingFailureTemplate [LayoutElement + Image；默认inactive]
            Btn_LoadingFailureRow [Button + Image]
              Txt_LoadingFailureRowLabel [TextMeshProUGUI]
              Txt_LoadingFailureRowValue [TextMeshProUGUI]
  Grp_LoadingLoadingState [无Graphic]
    Panel_LoadingLoadingMessage [Image]
      Txt_LoadingLoadingMessage [TextMeshProUGUI]
  Grp_LoadingEmptyState [无Graphic]
    Panel_LoadingEmptyMessage [Image]
      Txt_LoadingEmptyMessage [TextMeshProUGUI]
  Grp_LoadingErrorState [无Graphic]
    Panel_LoadingErrorMessage [Image]
      Txt_LoadingErrorMessage [TextMeshProUGUI]
  Grp_LoadingSuccessState [无Graphic]
    Panel_LoadingSuccessMessage [Image]
      Txt_LoadingSuccessMessage [TextMeshProUGUI]
  Grp_LoadingDisabledState [无Graphic]
    Panel_LoadingDisabledMessage [Image]
      Txt_LoadingDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageLoading | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；加载；内部页面根 |
| Txt_LoadingTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1152,32); pos(8,0) | absolute | TextMeshProUGUI；加载 |
| Grp_LoadingActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=6 cell=(185,48) spacing=(8,8)；操作区 |
| Btn_LoadingRetry | min(0,1) max(0,1); pivot(0,1); sizeDelta(185,48)初始化; pos(0,0)初始化; LayoutElement preferred(185,48); 最终位置/尺寸由组驱动 | group | Button + Image；失败且允许重试时执行，同槽位请求防重 |
| Txt_LoadingRetryLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；重试加载 |
| Btn_LoadingReturn | min(0,1) max(0,1); pivot(0,1); sizeDelta(185,48)初始化; pos(0,0)初始化; LayoutElement preferred(185,48); 最终位置/尺寸由组驱动 | group | Button + Image；在流程允许的安全点退出加载 |
| Txt_LoadingReturnLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；返回主菜单 |
| Panel_LoadingStage | min(0,1) max(0,1); pivot(0,1); sizeDelta(576,634); pos(0,-36) | absolute | Image；加载阶段 |
| Txt_LoadingStageHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,32); pos(12,-8) | absolute | TextMeshProUGUI；加载阶段 |
| List_LoadingStage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_LoadingStage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_LoadingStage | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_LoadingStageBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,120)初始化; pos(0,0)初始化; LayoutElement preferred(552,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；当前真实阶段：读取、校验、资源准备、世界恢复；可用可信进度 |
| Item_LoadingStageTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,104)初始化; pos(0,0)初始化; LayoutElement preferred(552,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_LoadingStageRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_LoadingStageRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_LoadingStageRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_LoadingFailure | min(0,1) max(0,1); pivot(0,1); sizeDelta(576,634); pos(592,-36) | absolute | Image；异常信息 |
| Txt_LoadingFailureHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,32); pos(12,-8) | absolute | TextMeshProUGUI；异常信息 |
| List_LoadingFailure | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_LoadingFailure | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_LoadingFailure | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_LoadingFailureBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,120)初始化; pos(0,0)初始化; LayoutElement preferred(552,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；失败原因；是否可重试；返回主菜单说明 |
| Item_LoadingFailureTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,104)初始化; pos(0,0)初始化; LayoutElement preferred(552,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_LoadingFailureRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_LoadingFailureRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_LoadingFailureRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_LoadingLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_LoadingLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_LoadingLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_LoadingEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_LoadingEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_LoadingEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_LoadingErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_LoadingErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_LoadingErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_LoadingSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_LoadingSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_LoadingSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_LoadingDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_LoadingDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_LoadingDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## OfflineSettlement：离线结算

功能文档：[离线结算](OfflineSettlement.md)；归属 `ProgressReportForm`；内容 1168×730。

```text
Panel_PageOfflineSettlement [Image]
  Txt_OfflineSettlementTitle [TextMeshProUGUI]
  Grp_OfflineSettlementActions [GridLayoutGroup fixedColumns=6 cell=(185,48) spacing=(8,8)]
    Btn_OfflineSettlementRetry [Button + Image]
      Txt_OfflineSettlementRetryLabel [TextMeshProUGUI]
    Btn_OfflineSettlementReturn [Button + Image]
      Txt_OfflineSettlementReturnLabel [TextMeshProUGUI]
  Panel_OfflineSettlementStage [Image]
    Txt_OfflineSettlementStageHeading [TextMeshProUGUI]
    List_OfflineSettlementStage [ScrollRect vertical=true horizontal=false]
      Viewport_OfflineSettlementStage [RectMask2D]
        Content_OfflineSettlementStage [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_OfflineSettlementStageBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_OfflineSettlementStageTemplate [LayoutElement + Image；默认inactive]
            Btn_OfflineSettlementStageRow [Button + Image]
              Txt_OfflineSettlementStageRowLabel [TextMeshProUGUI]
              Txt_OfflineSettlementStageRowValue [TextMeshProUGUI]
  Panel_OfflineSettlementResume [Image]
    Txt_OfflineSettlementResumeHeading [TextMeshProUGUI]
    List_OfflineSettlementResume [ScrollRect vertical=true horizontal=false]
      Viewport_OfflineSettlementResume [RectMask2D]
        Content_OfflineSettlementResume [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_OfflineSettlementResumeBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_OfflineSettlementResumeTemplate [LayoutElement + Image；默认inactive]
            Btn_OfflineSettlementResumeRow [Button + Image]
              Txt_OfflineSettlementResumeRowLabel [TextMeshProUGUI]
              Txt_OfflineSettlementResumeRowValue [TextMeshProUGUI]
  Grp_OfflineSettlementLoadingState [无Graphic]
    Panel_OfflineSettlementLoadingMessage [Image]
      Txt_OfflineSettlementLoadingMessage [TextMeshProUGUI]
  Grp_OfflineSettlementEmptyState [无Graphic]
    Panel_OfflineSettlementEmptyMessage [Image]
      Txt_OfflineSettlementEmptyMessage [TextMeshProUGUI]
  Grp_OfflineSettlementErrorState [无Graphic]
    Panel_OfflineSettlementErrorMessage [Image]
      Txt_OfflineSettlementErrorMessage [TextMeshProUGUI]
  Grp_OfflineSettlementSuccessState [无Graphic]
    Panel_OfflineSettlementSuccessMessage [Image]
      Txt_OfflineSettlementSuccessMessage [TextMeshProUGUI]
  Grp_OfflineSettlementDisabledState [无Graphic]
    Panel_OfflineSettlementDisabledMessage [Image]
      Txt_OfflineSettlementDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageOfflineSettlement | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；离线结算；内部页面根 |
| Txt_OfflineSettlementTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1152,32); pos(8,0) | absolute | TextMeshProUGUI；离线结算 |
| Grp_OfflineSettlementActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=6 cell=(185,48) spacing=(8,8)；操作区 |
| Btn_OfflineSettlementRetry | min(0,1) max(0,1); pivot(0,1); sizeDelta(185,48)初始化; pos(0,0)初始化; LayoutElement preferred(185,48); 最终位置/尺寸由组驱动 | group | Button + Image；从最后已保存安全批次继续，不重复结算 |
| Txt_OfflineSettlementRetryLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；失败后重试 |
| Btn_OfflineSettlementReturn | min(0,1) max(0,1); pivot(0,1); sizeDelta(185,48)初始化; pos(0,0)初始化; LayoutElement preferred(185,48); 最终位置/尺寸由组驱动 | group | Button + Image；仅流程提供安全退出时开放；当前批次不可中断则等待保存 |
| Txt_OfflineSettlementReturnLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；返回主菜单 |
| Panel_OfflineSettlementStage | min(0,1) max(0,1); pivot(0,1); sizeDelta(576,634); pos(0,-36) | absolute | Image；确定性推进 |
| Txt_OfflineSettlementStageHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,32); pos(12,-8) | absolute | TextMeshProUGUI；确定性推进 |
| List_OfflineSettlementStage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_OfflineSettlementStage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_OfflineSettlementStage | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_OfflineSettlementStageBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,120)初始化; pos(0,0)初始化; LayoutElement preferred(552,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；离线区间；已处理世界时间；当前事件批次；可信进度或阶段 |
| Item_OfflineSettlementStageTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,104)初始化; pos(0,0)初始化; LayoutElement preferred(552,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_OfflineSettlementStageRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_OfflineSettlementStageRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_OfflineSettlementStageRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_OfflineSettlementResume | min(0,1) max(0,1); pivot(0,1); sizeDelta(576,634); pos(592,-36) | absolute | Image；可恢复进度 |
| Txt_OfflineSettlementResumeHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,32); pos(12,-8) | absolute | TextMeshProUGUI；可恢复进度 |
| List_OfflineSettlementResume | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_OfflineSettlementResume | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_OfflineSettlementResume | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_OfflineSettlementResumeBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,120)初始化; pos(0,0)初始化; LayoutElement preferred(552,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；最近安全批次；已保存检查点；异常算法处理摘要 |
| Item_OfflineSettlementResumeTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,104)初始化; pos(0,0)初始化; LayoutElement preferred(552,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_OfflineSettlementResumeRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_OfflineSettlementResumeRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_OfflineSettlementResumeRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_OfflineSettlementLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_OfflineSettlementLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_OfflineSettlementLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_OfflineSettlementEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_OfflineSettlementEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_OfflineSettlementEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_OfflineSettlementErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_OfflineSettlementErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_OfflineSettlementErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_OfflineSettlementSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_OfflineSettlementSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_OfflineSettlementSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_OfflineSettlementDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_OfflineSettlementDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_OfflineSettlementDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## ReturnReport：回归报告

功能文档：[回归报告](ReturnReport.md)；归属 `ProgressReportForm`；内容 1168×730。

```text
Panel_PageReturnReport [Image]
  Txt_ReturnReportTitle [TextMeshProUGUI]
  Grp_ReturnReportActions [GridLayoutGroup fixedColumns=6 cell=(185,48) spacing=(8,8)]
    Btn_ReturnReportEnter [Button + Image]
      Txt_ReturnReportEnterLabel [TextMeshProUGUI]
    Btn_ReturnReportLocate [Button + Image]
      Txt_ReturnReportLocateLabel [TextMeshProUGUI]
    Btn_ReturnReportSection [Button + Image]
      Txt_ReturnReportSectionLabel [TextMeshProUGUI]
  Panel_ReturnReportSections [Image]
    Txt_ReturnReportSectionsHeading [TextMeshProUGUI]
    List_ReturnReportSections [ScrollRect vertical=true horizontal=false]
      Viewport_ReturnReportSections [RectMask2D]
        Content_ReturnReportSections [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ReturnReportSectionsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ReturnReportSectionsTemplate [LayoutElement + Image；默认inactive]
            Btn_ReturnReportSectionsRow [Button + Image]
              Txt_ReturnReportSectionsRowLabel [TextMeshProUGUI]
              Txt_ReturnReportSectionsRowValue [TextMeshProUGUI]
  Panel_ReturnReportDetails [Image]
    Txt_ReturnReportDetailsHeading [TextMeshProUGUI]
    List_ReturnReportDetails [ScrollRect vertical=true horizontal=false]
      Viewport_ReturnReportDetails [RectMask2D]
        Content_ReturnReportDetails [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ReturnReportDetailsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ReturnReportDetailsTemplate [LayoutElement + Image；默认inactive]
            Btn_ReturnReportDetailsRow [Button + Image]
              Txt_ReturnReportDetailsRowLabel [TextMeshProUGUI]
              Txt_ReturnReportDetailsRowValue [TextMeshProUGUI]
  Grp_ReturnReportLoadingState [无Graphic]
    Panel_ReturnReportLoadingMessage [Image]
      Txt_ReturnReportLoadingMessage [TextMeshProUGUI]
  Grp_ReturnReportEmptyState [无Graphic]
    Panel_ReturnReportEmptyMessage [Image]
      Txt_ReturnReportEmptyMessage [TextMeshProUGUI]
  Grp_ReturnReportErrorState [无Graphic]
    Panel_ReturnReportErrorMessage [Image]
      Txt_ReturnReportErrorMessage [TextMeshProUGUI]
  Grp_ReturnReportSuccessState [无Graphic]
    Panel_ReturnReportSuccessMessage [Image]
      Txt_ReturnReportSuccessMessage [TextMeshProUGUI]
  Grp_ReturnReportDisabledState [无Graphic]
    Panel_ReturnReportDisabledMessage [Image]
      Txt_ReturnReportDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageReturnReport | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；回归报告；内部页面根 |
| Txt_ReturnReportTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1152,32); pos(8,0) | absolute | TextMeshProUGUI；回归报告 |
| Grp_ReturnReportActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=6 cell=(185,48) spacing=(8,8)；操作区 |
| Btn_ReturnReportEnter | min(0,1) max(0,1); pivot(0,1); sizeDelta(185,48)初始化; pos(0,0)初始化; LayoutElement preferred(185,48); 最终位置/尺寸由组驱动 | group | Button + Image；首次报告关闭后授予操作权；重看时返回来源 |
| Txt_ReturnReportEnterLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；进入世界 |
| Btn_ReturnReportLocate | min(0,1) max(0,1); pivot(0,1); sizeDelta(185,48)初始化; pos(0,0)初始化; LayoutElement preferred(185,48); 最终位置/尺寸由组驱动 | group | Button + Image；对象有效才聚焦；失效显示原因 |
| Txt_ReturnReportLocateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；进入世界并定位 |
| Btn_ReturnReportSection | min(0,1) max(0,1); pivot(0,1); sizeDelta(185,48)初始化; pos(0,0)初始化; LayoutElement preferred(185,48); 最终位置/尺寸由组驱动 | group | Button + Image；只改变阅读内容 |
| Txt_ReturnReportSectionLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；切换报告分类 |
| Panel_ReturnReportSections | min(0,1) max(0,1); pivot(0,1); sizeDelta(443,634); pos(0,-36) | absolute | Image；报告分类 |
| Txt_ReturnReportSectionsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(419,32); pos(12,-8) | absolute | TextMeshProUGUI；报告分类 |
| List_ReturnReportSections | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ReturnReportSections | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ReturnReportSections | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ReturnReportSectionsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(419,120)初始化; pos(0,0)初始化; LayoutElement preferred(419,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；未解决阻塞／严重异常优先；任务升级；施工制造；资源；物品；消耗损失；停机 |
| Item_ReturnReportSectionsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(419,104)初始化; pos(0,0)初始化; LayoutElement preferred(419,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ReturnReportSectionsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ReturnReportSectionsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ReturnReportSectionsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_ReturnReportDetails | min(0,1) max(0,1); pivot(0,1); sizeDelta(709,634); pos(459,-36) | absolute | Image；报告条目 |
| Txt_ReturnReportDetailsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(685,32); pos(12,-8) | absolute | TextMeshProUGUI；报告条目 |
| List_ReturnReportDetails | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ReturnReportDetails | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ReturnReportDetails | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ReturnReportDetailsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(685,120)初始化; pos(0,0)初始化; LayoutElement preferred(685,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；实际离线时间；合并数量和持续时间；对象关联；具体净变化；未解决责任 |
| Item_ReturnReportDetailsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(685,104)初始化; pos(0,0)初始化; LayoutElement preferred(685,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ReturnReportDetailsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ReturnReportDetailsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ReturnReportDetailsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_ReturnReportLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ReturnReportLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ReturnReportLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_ReturnReportEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ReturnReportEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ReturnReportEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_ReturnReportErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ReturnReportErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ReturnReportErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_ReturnReportSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ReturnReportSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ReturnReportSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_ReturnReportDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ReturnReportDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ReturnReportDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## BackupProgress：备份恢复进度与结果

功能文档：[备份恢复进度与结果](BackupProgress.md)；归属 `ProgressReportForm`；内容 1168×730。

```text
Panel_PageBackupProgress [Image]
  Txt_BackupProgressTitle [TextMeshProUGUI]
  Grp_BackupProgressActions [GridLayoutGroup fixedColumns=6 cell=(185,48) spacing=(8,8)]
    Btn_BackupProgressContinue [Button + Image]
      Txt_BackupProgressContinueLabel [TextMeshProUGUI]
    Btn_BackupProgressRetry [Button + Image]
      Txt_BackupProgressRetryLabel [TextMeshProUGUI]
    Btn_BackupProgressReturn [Button + Image]
      Txt_BackupProgressReturnLabel [TextMeshProUGUI]
  Panel_BackupProgressRecovery [Image]
    Txt_BackupProgressRecoveryHeading [TextMeshProUGUI]
    List_BackupProgressRecovery [ScrollRect vertical=true horizontal=false]
      Viewport_BackupProgressRecovery [RectMask2D]
        Content_BackupProgressRecovery [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_BackupProgressRecoveryBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_BackupProgressRecoveryTemplate [LayoutElement + Image；默认inactive]
            Btn_BackupProgressRecoveryRow [Button + Image]
              Txt_BackupProgressRecoveryRowLabel [TextMeshProUGUI]
              Txt_BackupProgressRecoveryRowValue [TextMeshProUGUI]
  Panel_BackupProgressOutcome [Image]
    Txt_BackupProgressOutcomeHeading [TextMeshProUGUI]
    List_BackupProgressOutcome [ScrollRect vertical=true horizontal=false]
      Viewport_BackupProgressOutcome [RectMask2D]
        Content_BackupProgressOutcome [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_BackupProgressOutcomeBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_BackupProgressOutcomeTemplate [LayoutElement + Image；默认inactive]
            Btn_BackupProgressOutcomeRow [Button + Image]
              Txt_BackupProgressOutcomeRowLabel [TextMeshProUGUI]
              Txt_BackupProgressOutcomeRowValue [TextMeshProUGUI]
  Grp_BackupProgressLoadingState [无Graphic]
    Panel_BackupProgressLoadingMessage [Image]
      Txt_BackupProgressLoadingMessage [TextMeshProUGUI]
  Grp_BackupProgressEmptyState [无Graphic]
    Panel_BackupProgressEmptyMessage [Image]
      Txt_BackupProgressEmptyMessage [TextMeshProUGUI]
  Grp_BackupProgressErrorState [无Graphic]
    Panel_BackupProgressErrorMessage [Image]
      Txt_BackupProgressErrorMessage [TextMeshProUGUI]
  Grp_BackupProgressSuccessState [无Graphic]
    Panel_BackupProgressSuccessMessage [Image]
      Txt_BackupProgressSuccessMessage [TextMeshProUGUI]
  Grp_BackupProgressDisabledState [无Graphic]
    Panel_BackupProgressDisabledMessage [Image]
      Txt_BackupProgressDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageBackupProgress | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；备份恢复进度与结果；内部页面根 |
| Txt_BackupProgressTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1152,32); pos(8,0) | absolute | TextMeshProUGUI；备份恢复进度与结果 |
| Grp_BackupProgressActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=6 cell=(185,48) spacing=(8,8)；操作区 |
| Btn_BackupProgressContinue | min(0,1) max(0,1); pivot(0,1); sizeDelta(185,48)初始化; pos(0,0)初始化; LayoutElement preferred(185,48); 最终位置/尺寸由组驱动 | group | Button + Image；仅恢复成功后→18加载 |
| Txt_BackupProgressContinueLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；继续加载 |
| Btn_BackupProgressRetry | min(0,1) max(0,1); pivot(0,1); sizeDelta(185,48)初始化; pos(0,0)初始化; LayoutElement preferred(185,48); 最终位置/尺寸由组驱动 | group | Button + Image；仅允许重试的失败，复用确认备份身份 |
| Txt_BackupProgressRetryLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；重试恢复 |
| Btn_BackupProgressReturn | min(0,1) max(0,1); pivot(0,1); sizeDelta(185,48)初始化; pos(0,0)初始化; LayoutElement preferred(185,48); 最终位置/尺寸由组驱动 | group | Button + Image；失败或允许关闭时返回 |
| Txt_BackupProgressReturnLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；返回主菜单 |
| Panel_BackupProgressRecovery | min(0,1) max(0,1); pivot(0,1); sizeDelta(576,634); pos(0,-36) | absolute | Image；恢复阶段 |
| Txt_BackupProgressRecoveryHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,32); pos(12,-8) | absolute | TextMeshProUGUI；恢复阶段 |
| List_BackupProgressRecovery | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_BackupProgressRecovery | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_BackupProgressRecovery | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_BackupProgressRecoveryBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,120)初始化; pos(0,0)初始化; LayoutElement preferred(552,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；已确认备份时间；读取／校验／恢复；真实结果 |
| Item_BackupProgressRecoveryTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,104)初始化; pos(0,0)初始化; LayoutElement preferred(552,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_BackupProgressRecoveryRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_BackupProgressRecoveryRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_BackupProgressRecoveryRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_BackupProgressOutcome | min(0,1) max(0,1); pivot(0,1); sizeDelta(576,634); pos(592,-36) | absolute | Image；恢复结果 |
| Txt_BackupProgressOutcomeHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,32); pos(12,-8) | absolute | TextMeshProUGUI；恢复结果 |
| List_BackupProgressOutcome | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_BackupProgressOutcome | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_BackupProgressOutcome | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_BackupProgressOutcomeBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,120)初始化; pos(0,0)初始化; LayoutElement preferred(552,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；成功恢复点；失败原因；可以继续或返回 |
| Item_BackupProgressOutcomeTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,104)初始化; pos(0,0)初始化; LayoutElement preferred(552,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_BackupProgressOutcomeRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_BackupProgressOutcomeRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_BackupProgressOutcomeRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_BackupProgressLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BackupProgressLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BackupProgressLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_BackupProgressEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BackupProgressEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BackupProgressEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_BackupProgressErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BackupProgressErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BackupProgressErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_BackupProgressSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BackupProgressSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BackupProgressSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_BackupProgressDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BackupProgressDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BackupProgressDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## SafeWait：安全停止等待

功能文档：[安全停止等待](SafeWait.md)；归属 `OperationFeedbackForm`；内容 848×630。

```text
Panel_PageSafeWait [Image]
  Txt_SafeWaitTitle [TextMeshProUGUI]
  Grp_SafeWaitActions [GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)]
    Btn_SafeWaitDismiss [Button + Image]
      Txt_SafeWaitDismissLabel [TextMeshProUGUI]
    Btn_SafeWaitCancel [Button + Image]
      Txt_SafeWaitCancelLabel [TextMeshProUGUI]
    Btn_SafeWaitDetails [Button + Image]
      Txt_SafeWaitDetailsLabel [TextMeshProUGUI]
  Panel_SafeWaitRequest [Image]
    Txt_SafeWaitRequestHeading [TextMeshProUGUI]
    List_SafeWaitRequest [ScrollRect vertical=true horizontal=false]
      Viewport_SafeWaitRequest [RectMask2D]
        Content_SafeWaitRequest [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_SafeWaitRequestBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_SafeWaitRequestTemplate [LayoutElement + Image；默认inactive]
            Btn_SafeWaitRequestRow [Button + Image]
              Txt_SafeWaitRequestRowLabel [TextMeshProUGUI]
              Txt_SafeWaitRequestRowValue [TextMeshProUGUI]
  Panel_SafeWaitWaiting [Image]
    Txt_SafeWaitWaitingHeading [TextMeshProUGUI]
    List_SafeWaitWaiting [ScrollRect vertical=true horizontal=false]
      Viewport_SafeWaitWaiting [RectMask2D]
        Content_SafeWaitWaiting [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_SafeWaitWaitingBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_SafeWaitWaitingTemplate [LayoutElement + Image；默认inactive]
            Btn_SafeWaitWaitingRow [Button + Image]
              Txt_SafeWaitWaitingRowLabel [TextMeshProUGUI]
              Txt_SafeWaitWaitingRowValue [TextMeshProUGUI]
  Grp_SafeWaitLoadingState [无Graphic]
    Panel_SafeWaitLoadingMessage [Image]
      Txt_SafeWaitLoadingMessage [TextMeshProUGUI]
  Grp_SafeWaitEmptyState [无Graphic]
    Panel_SafeWaitEmptyMessage [Image]
      Txt_SafeWaitEmptyMessage [TextMeshProUGUI]
  Grp_SafeWaitErrorState [无Graphic]
    Panel_SafeWaitErrorMessage [Image]
      Txt_SafeWaitErrorMessage [TextMeshProUGUI]
  Grp_SafeWaitSuccessState [无Graphic]
    Panel_SafeWaitSuccessMessage [Image]
      Txt_SafeWaitSuccessMessage [TextMeshProUGUI]
  Grp_SafeWaitDisabledState [无Graphic]
    Panel_SafeWaitDisabledMessage [Image]
      Txt_SafeWaitDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageSafeWait | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；安全停止等待；内部页面根 |
| Txt_SafeWaitTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(832,32); pos(8,0) | absolute | TextMeshProUGUI；安全停止等待 |
| Grp_SafeWaitActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)；操作区 |
| Btn_SafeWaitDismiss | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；关闭显示但继续业务请求，来源处保留等待状态 |
| Txt_SafeWaitDismissLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；收起等待 |
| Btn_SafeWaitCancel | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；仅服务明确可取消时提供；取消成功保持原机器状态 |
| Txt_SafeWaitCancelLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；取消等待 |
| Btn_SafeWaitDetails | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开对应机器详情 |
| Txt_SafeWaitDetailsLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；相关对象 |
| Panel_SafeWaitRequest | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(0,-36) | absolute | Image；已提交操作 |
| Txt_SafeWaitRequestHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；已提交操作 |
| List_SafeWaitRequest | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_SafeWaitRequest | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_SafeWaitRequest | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_SafeWaitRequestBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；对象；动作；请求ID对应简明说明；当前权威状态 |
| Item_SafeWaitRequestTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_SafeWaitRequestRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_SafeWaitRequestRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_SafeWaitRequestRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_SafeWaitWaiting | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(432,-36) | absolute | Image；等待原因 |
| Txt_SafeWaitWaitingHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；等待原因 |
| List_SafeWaitWaiting | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_SafeWaitWaiting | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_SafeWaitWaiting | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_SafeWaitWaitingBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；当前行为；安全边界；是否允许取消；成本尚未扣除 |
| Item_SafeWaitWaitingTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_SafeWaitWaitingRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_SafeWaitWaitingRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_SafeWaitWaitingRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_SafeWaitLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SafeWaitLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SafeWaitLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_SafeWaitEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SafeWaitEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SafeWaitEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_SafeWaitErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SafeWaitErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SafeWaitErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_SafeWaitSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SafeWaitSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SafeWaitSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_SafeWaitDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SafeWaitDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SafeWaitDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## ActionResult：成功拒绝与重试反馈

功能文档：[成功拒绝与重试反馈](ActionResult.md)；归属 `OperationFeedbackForm`；内容 848×630。

```text
Panel_PageActionResult [Image]
  Txt_ActionResultTitle [TextMeshProUGUI]
  Grp_ActionResultActions [GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)]
    Btn_ActionResultRetry [Button + Image]
      Txt_ActionResultRetryLabel [TextMeshProUGUI]
    Btn_ActionResultDetails [Button + Image]
      Txt_ActionResultDetailsLabel [TextMeshProUGUI]
    Btn_ActionResultDismiss [Button + Image]
      Txt_ActionResultDismissLabel [TextMeshProUGUI]
  Panel_ActionResultResult [Image]
    Txt_ActionResultResultHeading [TextMeshProUGUI]
    List_ActionResultResult [ScrollRect vertical=true horizontal=false]
      Viewport_ActionResultResult [RectMask2D]
        Content_ActionResultResult [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ActionResultResultBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ActionResultResultTemplate [LayoutElement + Image；默认inactive]
            Btn_ActionResultResultRow [Button + Image]
              Txt_ActionResultResultRowLabel [TextMeshProUGUI]
              Txt_ActionResultResultRowValue [TextMeshProUGUI]
  Panel_ActionResultNext [Image]
    Txt_ActionResultNextHeading [TextMeshProUGUI]
    List_ActionResultNext [ScrollRect vertical=true horizontal=false]
      Viewport_ActionResultNext [RectMask2D]
        Content_ActionResultNext [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ActionResultNextBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ActionResultNextTemplate [LayoutElement + Image；默认inactive]
            Btn_ActionResultNextRow [Button + Image]
              Txt_ActionResultNextRowLabel [TextMeshProUGUI]
              Txt_ActionResultNextRowValue [TextMeshProUGUI]
  Grp_ActionResultLoadingState [无Graphic]
    Panel_ActionResultLoadingMessage [Image]
      Txt_ActionResultLoadingMessage [TextMeshProUGUI]
  Grp_ActionResultEmptyState [无Graphic]
    Panel_ActionResultEmptyMessage [Image]
      Txt_ActionResultEmptyMessage [TextMeshProUGUI]
  Grp_ActionResultErrorState [无Graphic]
    Panel_ActionResultErrorMessage [Image]
      Txt_ActionResultErrorMessage [TextMeshProUGUI]
  Grp_ActionResultSuccessState [无Graphic]
    Panel_ActionResultSuccessMessage [Image]
      Txt_ActionResultSuccessMessage [TextMeshProUGUI]
  Grp_ActionResultDisabledState [无Graphic]
    Panel_ActionResultDisabledMessage [Image]
      Txt_ActionResultDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageActionResult | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；成功拒绝与重试反馈；内部页面根 |
| Txt_ActionResultTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(832,32); pos(8,0) | absolute | TextMeshProUGUI；成功拒绝与重试反馈 |
| Grp_ActionResultActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)；操作区 |
| Btn_ActionResultRetry | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；原请求已明确结束且可重试才发新请求；有风险动作回17重确认 |
| Txt_ActionResultRetryLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；重新尝试 |
| Btn_ActionResultDetails | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；回业务来源或对应记录 |
| Txt_ActionResultDetailsLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看详情 |
| Btn_ActionResultDismiss | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；恢复来源焦点 |
| Txt_ActionResultDismissLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；关闭提示 |
| Panel_ActionResultResult | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(0,-36) | absolute | Image；操作结果 |
| Txt_ActionResultResultHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；操作结果 |
| List_ActionResultResult | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ActionResultResult | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ActionResultResult | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ActionResultResultBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；对象；动作；成功／拒绝／已取消；真实原因 |
| Item_ActionResultResultTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ActionResultResultRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ActionResultResultRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ActionResultResultRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_ActionResultNext | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(432,-36) | absolute | Image；后续动作 |
| Txt_ActionResultNextHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；后续动作 |
| List_ActionResultNext | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ActionResultNext | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ActionResultNext | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ActionResultNextBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；最新条件；可重试性；详情入口 |
| Item_ActionResultNextTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ActionResultNextRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ActionResultNextRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ActionResultNextRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_ActionResultLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ActionResultLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ActionResultLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_ActionResultEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ActionResultEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ActionResultEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_ActionResultErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ActionResultErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ActionResultErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_ActionResultSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ActionResultSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ActionResultSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_ActionResultDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ActionResultDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ActionResultDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## Delivery：合并交付结果

功能文档：[合并交付结果](Delivery.md)；归属 `ProgressReportForm`；内容 1168×730。

```text
Panel_PageDelivery [Image]
  Txt_DeliveryTitle [TextMeshProUGUI]
  Grp_DeliveryActions [GridLayoutGroup fixedColumns=6 cell=(185,48) spacing=(8,8)]
    Btn_DeliveryDestination [Button + Image]
      Txt_DeliveryDestinationLabel [TextMeshProUGUI]
    Btn_DeliveryClose [Button + Image]
      Txt_DeliveryCloseLabel [TextMeshProUGUI]
  Panel_DeliverySummary [Image]
    Txt_DeliverySummaryHeading [TextMeshProUGUI]
    List_DeliverySummary [ScrollRect vertical=true horizontal=false]
      Viewport_DeliverySummary [RectMask2D]
        Content_DeliverySummary [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_DeliverySummaryBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_DeliverySummaryTemplate [LayoutElement + Image；默认inactive]
            Btn_DeliverySummaryRow [Button + Image]
              Txt_DeliverySummaryRowLabel [TextMeshProUGUI]
              Txt_DeliverySummaryRowValue [TextMeshProUGUI]
  Panel_DeliveryDestinations [Image]
    Txt_DeliveryDestinationsHeading [TextMeshProUGUI]
    List_DeliveryDestinations [ScrollRect vertical=true horizontal=false]
      Viewport_DeliveryDestinations [RectMask2D]
        Content_DeliveryDestinations [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_DeliveryDestinationsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_DeliveryDestinationsTemplate [LayoutElement + Image；默认inactive]
            Btn_DeliveryDestinationsRow [Button + Image]
              Txt_DeliveryDestinationsRowLabel [TextMeshProUGUI]
              Txt_DeliveryDestinationsRowValue [TextMeshProUGUI]
  Grp_DeliveryLoadingState [无Graphic]
    Panel_DeliveryLoadingMessage [Image]
      Txt_DeliveryLoadingMessage [TextMeshProUGUI]
  Grp_DeliveryEmptyState [无Graphic]
    Panel_DeliveryEmptyMessage [Image]
      Txt_DeliveryEmptyMessage [TextMeshProUGUI]
  Grp_DeliveryErrorState [无Graphic]
    Panel_DeliveryErrorMessage [Image]
      Txt_DeliveryErrorMessage [TextMeshProUGUI]
  Grp_DeliverySuccessState [无Graphic]
    Panel_DeliverySuccessMessage [Image]
      Txt_DeliverySuccessMessage [TextMeshProUGUI]
  Grp_DeliveryDisabledState [无Graphic]
    Panel_DeliveryDisabledMessage [Image]
      Txt_DeliveryDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageDelivery | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；合并交付结果；内部页面根 |
| Txt_DeliveryTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1152,32); pos(8,0) | absolute | TextMeshProUGUI；合并交付结果 |
| Grp_DeliveryActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=6 cell=(185,48) spacing=(8,8)；操作区 |
| Btn_DeliveryDestination | min(0,1) max(0,1); pivot(0,1); sizeDelta(185,48)初始化; pos(0,0)初始化; LayoutElement preferred(185,48); 最终位置/尺寸由组驱动 | group | Button + Image；按当前选中项类型进入09相应目录 |
| Txt_DeliveryDestinationLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看对应库存 |
| Btn_DeliveryClose | min(0,1) max(0,1); pivot(0,1); sizeDelta(185,48)初始化; pos(0,0)初始化; LayoutElement preferred(185,48); 最终位置/尺寸由组驱动 | group | Button + Image；返回原交易或任务页 |
| Txt_DeliveryCloseLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；关闭 |
| Panel_DeliverySummary | min(0,1) max(0,1); pivot(0,1); sizeDelta(576,634); pos(0,-36) | absolute | Image；交付总览 |
| Txt_DeliverySummaryHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,32); pos(12,-8) | absolute | TextMeshProUGUI；交付总览 |
| List_DeliverySummary | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_DeliverySummary | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_DeliverySummary | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_DeliverySummaryBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,120)初始化; pos(0,0)初始化; LayoutElement preferred(552,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；交易／奖励来源；成功状态；一次合并结果 |
| Item_DeliverySummaryTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,104)初始化; pos(0,0)初始化; LayoutElement preferred(552,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_DeliverySummaryRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_DeliverySummaryRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_DeliverySummaryRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_DeliveryDestinations | min(0,1) max(0,1); pivot(0,1); sizeDelta(576,634); pos(592,-36) | absolute | Image；分类去向 |
| Txt_DeliveryDestinationsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,32); pos(12,-8) | absolute | TextMeshProUGUI；分类去向 |
| List_DeliveryDestinations | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_DeliveryDestinations | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_DeliveryDestinations | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_DeliveryDestinationsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,120)初始化; pos(0,0)初始化; LayoutElement preferred(552,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；全局资源；仓库实体；组件库；机器库，各项实际数量 |
| Item_DeliveryDestinationsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,104)初始化; pos(0,0)初始化; LayoutElement preferred(552,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_DeliveryDestinationsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_DeliveryDestinationsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_DeliveryDestinationsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_DeliveryLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DeliveryLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DeliveryLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_DeliveryEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DeliveryEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DeliveryEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_DeliveryErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DeliveryErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DeliveryErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_DeliverySuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DeliverySuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DeliverySuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_DeliveryDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DeliveryDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DeliveryDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## Growth：成长与功能解锁

功能文档：[成长与功能解锁](Growth.md)；归属 `ProgressReportForm`；内容 1168×730。

```text
Panel_PageGrowth [Image]
  Txt_GrowthTitle [TextMeshProUGUI]
  Grp_GrowthActions [GridLayoutGroup fixedColumns=6 cell=(185,48) spacing=(8,8)]
    Btn_GrowthOpen [Button + Image]
      Txt_GrowthOpenLabel [TextMeshProUGUI]
    Btn_GrowthExplain [Button + Image]
      Txt_GrowthExplainLabel [TextMeshProUGUI]
    Btn_GrowthClose [Button + Image]
      Txt_GrowthCloseLabel [TextMeshProUGUI]
  Panel_GrowthLevel [Image]
    Txt_GrowthLevelHeading [TextMeshProUGUI]
    List_GrowthLevel [ScrollRect vertical=true horizontal=false]
      Viewport_GrowthLevel [RectMask2D]
        Content_GrowthLevel [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_GrowthLevelBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_GrowthLevelTemplate [LayoutElement + Image；默认inactive]
            Btn_GrowthLevelRow [Button + Image]
              Txt_GrowthLevelRowLabel [TextMeshProUGUI]
              Txt_GrowthLevelRowValue [TextMeshProUGUI]
  Panel_GrowthUnlocks [Image]
    Txt_GrowthUnlocksHeading [TextMeshProUGUI]
    List_GrowthUnlocks [ScrollRect vertical=true horizontal=false]
      Viewport_GrowthUnlocks [RectMask2D]
        Content_GrowthUnlocks [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_GrowthUnlocksBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_GrowthUnlocksTemplate [LayoutElement + Image；默认inactive]
            Btn_GrowthUnlocksRow [Button + Image]
              Txt_GrowthUnlocksRowLabel [TextMeshProUGUI]
              Txt_GrowthUnlocksRowValue [TextMeshProUGUI]
  Grp_GrowthLoadingState [无Graphic]
    Panel_GrowthLoadingMessage [Image]
      Txt_GrowthLoadingMessage [TextMeshProUGUI]
  Grp_GrowthEmptyState [无Graphic]
    Panel_GrowthEmptyMessage [Image]
      Txt_GrowthEmptyMessage [TextMeshProUGUI]
  Grp_GrowthErrorState [无Graphic]
    Panel_GrowthErrorMessage [Image]
      Txt_GrowthErrorMessage [TextMeshProUGUI]
  Grp_GrowthSuccessState [无Graphic]
    Panel_GrowthSuccessMessage [Image]
      Txt_GrowthSuccessMessage [TextMeshProUGUI]
  Grp_GrowthDisabledState [无Graphic]
    Panel_GrowthDisabledMessage [Image]
      Txt_GrowthDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageGrowth | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；成长与功能解锁；内部页面根 |
| Txt_GrowthTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1152,32); pos(8,0) | absolute | TextMeshProUGUI；成长与功能解锁 |
| Grp_GrowthActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=6 cell=(185,48) spacing=(8,8)；操作区 |
| Btn_GrowthOpen | min(0,1) max(0,1); pivot(0,1); sizeDelta(185,48)初始化; pos(0,0)初始化; LayoutElement preferred(185,48); 最终位置/尺寸由组驱动 | group | Button + Image；跳转对应目录，并带解锁项筛选 |
| Txt_GrowthOpenLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；前往已开放功能 |
| Btn_GrowthExplain | min(0,1) max(0,1); pivot(0,1); sizeDelta(185,48)初始化; pos(0,0)初始化; LayoutElement preferred(185,48); 最终位置/尺寸由组驱动 | group | Button + Image；16新功能说明 |
| Txt_GrowthExplainLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；功能说明 |
| Btn_GrowthClose | min(0,1) max(0,1); pivot(0,1); sizeDelta(185,48)初始化; pos(0,0)初始化; LayoutElement preferred(185,48); 最终位置/尺寸由组驱动 | group | Button + Image；不影响已经生效的升级 |
| Txt_GrowthCloseLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；关闭 |
| Panel_GrowthLevel | min(0,1) max(0,1); pivot(0,1); sizeDelta(576,634); pos(0,-36) | absolute | Image；等级与经验 |
| Txt_GrowthLevelHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,32); pos(12,-8) | absolute | TextMeshProUGUI；等级与经验 |
| List_GrowthLevel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_GrowthLevel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_GrowthLevel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_GrowthLevelBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,120)初始化; pos(0,0)初始化; LayoutElement preferred(552,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；当前中枢等级；当前经验与阈值；第一版3级上限 |
| Item_GrowthLevelTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,104)初始化; pos(0,0)初始化; LayoutElement preferred(552,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_GrowthLevelRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_GrowthLevelRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_GrowthLevelRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_GrowthUnlocks | min(0,1) max(0,1); pivot(0,1); sizeDelta(576,634); pos(592,-36) | absolute | Image；开放内容 |
| Txt_GrowthUnlocksHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,32); pos(12,-8) | absolute | TextMeshProUGUI；开放内容 |
| List_GrowthUnlocks | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_GrowthUnlocks | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_GrowthUnlocks | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_GrowthUnlocksBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,120)初始化; pos(0,0)初始化; LayoutElement preferred(552,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；新增等级上限；已满足的图纸／功能；仍需任务条件的内容 |
| Item_GrowthUnlocksTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,104)初始化; pos(0,0)初始化; LayoutElement preferred(552,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_GrowthUnlocksRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_GrowthUnlocksRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_GrowthUnlocksRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_GrowthLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_GrowthLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_GrowthLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_GrowthEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_GrowthEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_GrowthEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_GrowthErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_GrowthErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_GrowthErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_GrowthSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_GrowthSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_GrowthSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_GrowthDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_GrowthDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_GrowthDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## DailySupply：星拓联日常补给

功能文档：[星拓联日常补给](DailySupply.md)；归属 `ProgressReportForm`；内容 1168×730。

```text
Panel_PageDailySupply [Image]
  Txt_DailySupplyTitle [TextMeshProUGUI]
  Grp_DailySupplyActions [GridLayoutGroup fixedColumns=6 cell=(185,48) spacing=(8,8)]
    Btn_DailySupplyClaim [Button + Image]
      Txt_DailySupplyClaimLabel [TextMeshProUGUI]
    Btn_DailySupplyClose [Button + Image]
      Txt_DailySupplyCloseLabel [TextMeshProUGUI]
  Panel_DailySupplyOffer [Image]
    Txt_DailySupplyOfferHeading [TextMeshProUGUI]
    List_DailySupplyOffer [ScrollRect vertical=true horizontal=false]
      Viewport_DailySupplyOffer [RectMask2D]
        Content_DailySupplyOffer [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_DailySupplyOfferBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_DailySupplyOfferTemplate [LayoutElement + Image；默认inactive]
            Btn_DailySupplyOfferRow [Button + Image]
              Txt_DailySupplyOfferRowLabel [TextMeshProUGUI]
              Txt_DailySupplyOfferRowValue [TextMeshProUGUI]
  Panel_DailySupplyRules [Image]
    Txt_DailySupplyRulesHeading [TextMeshProUGUI]
    List_DailySupplyRules [ScrollRect vertical=true horizontal=false]
      Viewport_DailySupplyRules [RectMask2D]
        Content_DailySupplyRules [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_DailySupplyRulesBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_DailySupplyRulesTemplate [LayoutElement + Image；默认inactive]
            Btn_DailySupplyRulesRow [Button + Image]
              Txt_DailySupplyRulesRowLabel [TextMeshProUGUI]
              Txt_DailySupplyRulesRowValue [TextMeshProUGUI]
  Grp_DailySupplyLoadingState [无Graphic]
    Panel_DailySupplyLoadingMessage [Image]
      Txt_DailySupplyLoadingMessage [TextMeshProUGUI]
  Grp_DailySupplyEmptyState [无Graphic]
    Panel_DailySupplyEmptyMessage [Image]
      Txt_DailySupplyEmptyMessage [TextMeshProUGUI]
  Grp_DailySupplyErrorState [无Graphic]
    Panel_DailySupplyErrorMessage [Image]
      Txt_DailySupplyErrorMessage [TextMeshProUGUI]
  Grp_DailySupplySuccessState [无Graphic]
    Panel_DailySupplySuccessMessage [Image]
      Txt_DailySupplySuccessMessage [TextMeshProUGUI]
  Grp_DailySupplyDisabledState [无Graphic]
    Panel_DailySupplyDisabledMessage [Image]
      Txt_DailySupplyDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageDailySupply | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；星拓联日常补给；内部页面根 |
| Txt_DailySupplyTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1152,32); pos(8,0) | absolute | TextMeshProUGUI；星拓联日常补给 |
| Grp_DailySupplyActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=6 cell=(185,48) spacing=(8,8)；操作区 |
| Btn_DailySupplyClaim | min(0,1) max(0,1); pivot(0,1); sizeDelta(185,48)初始化; pos(0,0)初始化; LayoutElement preferred(185,48); 最终位置/尺寸由组驱动 | group | Button + Image；资格再验后一次加全局金币并存档；成功就地更新已领 |
| Txt_DailySupplyClaimLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；领取补给 |
| Btn_DailySupplyClose | min(0,1) max(0,1); pivot(0,1); sizeDelta(185,48)初始化; pos(0,0)初始化; LayoutElement preferred(185,48); 最终位置/尺寸由组驱动 | group | Button + Image；未领不自动发放 |
| Txt_DailySupplyCloseLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；关闭 |
| Panel_DailySupplyOffer | min(0,1) max(0,1); pivot(0,1); sizeDelta(576,634); pos(0,-36) | absolute | Image；当日补给 |
| Txt_DailySupplyOfferHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,32); pos(12,-8) | absolute | TextMeshProUGUI；当日补给 |
| List_DailySupplyOffer | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_DailySupplyOffer | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_DailySupplyOffer | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_DailySupplyOfferBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,120)初始化; pos(0,0)初始化; LayoutElement preferred(552,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；800金币（实际配置为准）；当前槽位；可领／已领；资格日期 |
| Item_DailySupplyOfferTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,104)初始化; pos(0,0)初始化; LayoutElement preferred(552,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_DailySupplyOfferRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_DailySupplyOfferRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_DailySupplyOfferRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_DailySupplyRules | min(0,1) max(0,1); pivot(0,1); sizeDelta(576,634); pos(592,-36) | absolute | Image；领取规则 |
| Txt_DailySupplyRulesHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,32); pos(12,-8) | absolute | TextMeshProUGUI；领取规则 |
| List_DailySupplyRules | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_DailySupplyRules | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_DailySupplyRules | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_DailySupplyRulesBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,120)初始化; pos(0,0)初始化; LayoutElement preferred(552,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；每现实自然日一次；不累计、不补发；无连续签到；日期倒退防重复 |
| Item_DailySupplyRulesTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,104)初始化; pos(0,0)初始化; LayoutElement preferred(552,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_DailySupplyRulesRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_DailySupplyRulesRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_DailySupplyRulesRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_DailySupplyLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DailySupplyLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DailySupplyLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_DailySupplyEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DailySupplyEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DailySupplyEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_DailySupplyErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DailySupplyErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DailySupplyErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_DailySupplySuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DailySupplySuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DailySupplySuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_DailySupplyDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DailySupplyDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DailySupplyDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## VersionComplete：第一版完成报告

功能文档：[第一版完成报告](VersionComplete.md)；归属 `ProgressReportForm`；内容 1168×730。

```text
Panel_PageVersionComplete [Image]
  Txt_VersionCompleteTitle [TextMeshProUGUI]
  Grp_VersionCompleteActions [GridLayoutGroup fixedColumns=6 cell=(185,48) spacing=(8,8)]
    Btn_VersionCompleteContinue [Button + Image]
      Txt_VersionCompleteContinueLabel [TextMeshProUGUI]
    Btn_VersionCompleteReview [Button + Image]
      Txt_VersionCompleteReviewLabel [TextMeshProUGUI]
    Btn_VersionCompleteMenu [Button + Image]
      Txt_VersionCompleteMenuLabel [TextMeshProUGUI]
  Panel_VersionCompleteAchievement [Image]
    Txt_VersionCompleteAchievementHeading [TextMeshProUGUI]
    List_VersionCompleteAchievement [ScrollRect vertical=true horizontal=false]
      Viewport_VersionCompleteAchievement [RectMask2D]
        Content_VersionCompleteAchievement [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_VersionCompleteAchievementBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_VersionCompleteAchievementTemplate [LayoutElement + Image；默认inactive]
            Btn_VersionCompleteAchievementRow [Button + Image]
              Txt_VersionCompleteAchievementRowLabel [TextMeshProUGUI]
              Txt_VersionCompleteAchievementRowValue [TextMeshProUGUI]
  Panel_VersionCompleteBoundary [Image]
    Txt_VersionCompleteBoundaryHeading [TextMeshProUGUI]
    List_VersionCompleteBoundary [ScrollRect vertical=true horizontal=false]
      Viewport_VersionCompleteBoundary [RectMask2D]
        Content_VersionCompleteBoundary [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_VersionCompleteBoundaryBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_VersionCompleteBoundaryTemplate [LayoutElement + Image；默认inactive]
            Btn_VersionCompleteBoundaryRow [Button + Image]
              Txt_VersionCompleteBoundaryRowLabel [TextMeshProUGUI]
              Txt_VersionCompleteBoundaryRowValue [TextMeshProUGUI]
  Grp_VersionCompleteLoadingState [无Graphic]
    Panel_VersionCompleteLoadingMessage [Image]
      Txt_VersionCompleteLoadingMessage [TextMeshProUGUI]
  Grp_VersionCompleteEmptyState [无Graphic]
    Panel_VersionCompleteEmptyMessage [Image]
      Txt_VersionCompleteEmptyMessage [TextMeshProUGUI]
  Grp_VersionCompleteErrorState [无Graphic]
    Panel_VersionCompleteErrorMessage [Image]
      Txt_VersionCompleteErrorMessage [TextMeshProUGUI]
  Grp_VersionCompleteSuccessState [无Graphic]
    Panel_VersionCompleteSuccessMessage [Image]
      Txt_VersionCompleteSuccessMessage [TextMeshProUGUI]
  Grp_VersionCompleteDisabledState [无Graphic]
    Panel_VersionCompleteDisabledMessage [Image]
      Txt_VersionCompleteDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageVersionComplete | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；第一版完成报告；内部页面根 |
| Txt_VersionCompleteTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1152,32); pos(8,0) | absolute | TextMeshProUGUI；第一版完成报告 |
| Grp_VersionCompleteActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=6 cell=(185,48) spacing=(8,8)；操作区 |
| Btn_VersionCompleteContinue | min(0,1) max(0,1); pivot(0,1); sizeDelta(185,48)初始化; pos(0,0)初始化; LayoutElement preferred(185,48); 最终位置/尺寸由组驱动 | group | Button + Image；返回当前世界继续已有玩法 |
| Txt_VersionCompleteContinueLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；继续经营 |
| Btn_VersionCompleteReview | min(0,1) max(0,1); pivot(0,1); sizeDelta(185,48)初始化; pos(0,0)初始化; LayoutElement preferred(185,48); 最终位置/尺寸由组驱动 | group | Button + Image；15任务已完成分页 |
| Txt_VersionCompleteReviewLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看完成任务 |
| Btn_VersionCompleteMenu | min(0,1) max(0,1); pivot(0,1); sizeDelta(185,48)初始化; pos(0,0)初始化; LayoutElement preferred(185,48); 最终位置/尺寸由组驱动 | group | Button + Image；02系统菜单，按正常保存退出流程 |
| Txt_VersionCompleteMenuLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；系统菜单 |
| Panel_VersionCompleteAchievement | min(0,1) max(0,1); pivot(0,1); sizeDelta(576,634); pos(0,-36) | absolute | Image；完成内容 |
| Txt_VersionCompleteAchievementHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,32); pos(12,-8) | absolute | TextMeshProUGUI；完成内容 |
| List_VersionCompleteAchievement | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_VersionCompleteAchievement | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_VersionCompleteAchievement | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_VersionCompleteAchievementBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,120)初始化; pos(0,0)初始化; LayoutElement preferred(552,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；中枢3级；探索准备条件；主线完成摘要 |
| Item_VersionCompleteAchievementTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,104)初始化; pos(0,0)初始化; LayoutElement preferred(552,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_VersionCompleteAchievementRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_VersionCompleteAchievementRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_VersionCompleteAchievementRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_VersionCompleteBoundary | min(0,1) max(0,1); pivot(0,1); sizeDelta(576,634); pos(592,-36) | absolute | Image；版本范围说明 |
| Txt_VersionCompleteBoundaryHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,32); pos(12,-8) | absolute | TextMeshProUGUI；版本范围说明 |
| List_VersionCompleteBoundary | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_VersionCompleteBoundary | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_VersionCompleteBoundary | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_VersionCompleteBoundaryBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,120)初始化; pos(0,0)初始化; LayoutElement preferred(552,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；已解锁探索能力；实际探索不在第一版；继续经营现有基地 |
| Item_VersionCompleteBoundaryTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(552,104)初始化; pos(0,0)初始化; LayoutElement preferred(552,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_VersionCompleteBoundaryRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_VersionCompleteBoundaryRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_VersionCompleteBoundaryRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_VersionCompleteLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_VersionCompleteLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_VersionCompleteLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_VersionCompleteEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_VersionCompleteEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_VersionCompleteEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_VersionCompleteErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_VersionCompleteErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_VersionCompleteErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_VersionCompleteSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_VersionCompleteSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_VersionCompleteSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_VersionCompleteDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1168,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_VersionCompleteDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_VersionCompleteDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |
