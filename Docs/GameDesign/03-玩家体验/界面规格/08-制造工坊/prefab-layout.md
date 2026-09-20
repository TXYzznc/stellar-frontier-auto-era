# 08-制造工坊 — prefab-layout

状态：2026-09-19完整设计候选，待用户集中验收；未据此修改Prefab。

继承[通用合同](../00-通用合同.md)和[共享外壳](../00-共享外壳-prefab-layout.md)。下列子树预制在所属Form的Grp_PageHost中；公共外壳由共享文档唯一声明。跨家族同属一个Form的子树合并，禁止重复根节点。表中所有Image／文本颜色、资源、射线、默认字号与状态继承通用合同，列出的数据是字段说明，不是示例业务数值。

## Workshop：制造工坊工作台

功能文档：[制造工坊工作台](Workshop.md)；归属 `WorkshopForm`；内容 1488×730。

```text
Panel_PageWorkshop [Image]
  Txt_WorkshopTitle [TextMeshProUGUI]
  Grp_WorkshopActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_WorkshopRecipe [Button + Image]
      Txt_WorkshopRecipeLabel [TextMeshProUGUI]
    Btn_WorkshopEnqueue [Button + Image]
      Txt_WorkshopEnqueueLabel [TextMeshProUGUI]
    Btn_WorkshopPause [Button + Image]
      Txt_WorkshopPauseLabel [TextMeshProUGUI]
    Btn_WorkshopCancel [Button + Image]
      Txt_WorkshopCancelLabel [TextMeshProUGUI]
    Btn_WorkshopOutput [Button + Image]
      Txt_WorkshopOutputLabel [TextMeshProUGUI]
  Panel_WorkshopRecipes [Image]
    Txt_WorkshopRecipesHeading [TextMeshProUGUI]
    List_WorkshopRecipes [ScrollRect vertical=true horizontal=false]
      Viewport_WorkshopRecipes [RectMask2D]
        Content_WorkshopRecipes [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_WorkshopRecipesBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_WorkshopRecipesTemplate [LayoutElement + Image；默认inactive]
            Btn_WorkshopRecipesRow [Button + Image]
              Txt_WorkshopRecipesRowLabel [TextMeshProUGUI]
              Txt_WorkshopRecipesRowValue [TextMeshProUGUI]
  Panel_WorkshopQueue [Image]
    Txt_WorkshopQueueHeading [TextMeshProUGUI]
    List_WorkshopQueue [ScrollRect vertical=true horizontal=false]
      Viewport_WorkshopQueue [RectMask2D]
        Content_WorkshopQueue [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_WorkshopQueueBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_WorkshopQueueTemplate [LayoutElement + Image；默认inactive]
            Btn_WorkshopQueueRow [Button + Image]
              Txt_WorkshopQueueRowLabel [TextMeshProUGUI]
              Txt_WorkshopQueueRowValue [TextMeshProUGUI]
  Panel_WorkshopOutput [Image]
    Txt_WorkshopOutputHeading [TextMeshProUGUI]
    List_WorkshopOutput [ScrollRect vertical=true horizontal=false]
      Viewport_WorkshopOutput [RectMask2D]
        Content_WorkshopOutput [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_WorkshopOutputBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_WorkshopOutputTemplate [LayoutElement + Image；默认inactive]
            Btn_WorkshopOutputRow [Button + Image]
              Txt_WorkshopOutputRowLabel [TextMeshProUGUI]
              Txt_WorkshopOutputRowValue [TextMeshProUGUI]
  Grp_WorkshopLoadingState [无Graphic]
    Panel_WorkshopLoadingMessage [Image]
      Txt_WorkshopLoadingMessage [TextMeshProUGUI]
  Grp_WorkshopEmptyState [无Graphic]
    Panel_WorkshopEmptyMessage [Image]
      Txt_WorkshopEmptyMessage [TextMeshProUGUI]
  Grp_WorkshopErrorState [无Graphic]
    Panel_WorkshopErrorMessage [Image]
      Txt_WorkshopErrorMessage [TextMeshProUGUI]
  Grp_WorkshopSuccessState [无Graphic]
    Panel_WorkshopSuccessMessage [Image]
      Txt_WorkshopSuccessMessage [TextMeshProUGUI]
  Grp_WorkshopDisabledState [无Graphic]
    Panel_WorkshopDisabledMessage [Image]
      Txt_WorkshopDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageWorkshop | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；制造工坊工作台；内部页面根 |
| Txt_WorkshopTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；制造工坊工作台 |
| Grp_WorkshopActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_WorkshopRecipe | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；展开08-配方详情 |
| Txt_WorkshopRecipeLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；配方详情 |
| Btn_WorkshopEnqueue | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；说明未预留资源；每次仅加入一项，服务确认后排队 |
| Txt_WorkshopEnqueueLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；加入一个项目 |
| Btn_WorkshopPause | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；按工坊运行合同处理，不清空进度 |
| Txt_WorkshopPauseLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；暂停或恢复工坊 |
| Btn_WorkshopCancel | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开17取消确认；等待项无退款，制造中按剩余比例退款 |
| Txt_WorkshopCancelLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；取消选中项目 |
| Btn_WorkshopOutput | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；展开08-输出缓存 |
| Txt_WorkshopOutputLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看输出 |
| Panel_WorkshopRecipes | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(0,-36) | absolute | Image；配方目录 |
| Txt_WorkshopRecipesHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；配方目录 |
| List_WorkshopRecipes | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_WorkshopRecipes | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_WorkshopRecipes | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_WorkshopRecipesBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；配方名称；图纸；产物；完整成本；时间；资源满足状态；当前拥有数量 |
| Item_WorkshopRecipesTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_WorkshopRecipesRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_WorkshopRecipesRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_WorkshopRecipesRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_WorkshopQueue | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(501,-36) | absolute | Image；当前制造与等待 |
| Txt_WorkshopQueueHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；当前制造与等待 |
| List_WorkshopQueue | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_WorkshopQueue | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_WorkshopQueue | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_WorkshopQueueBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；队首进度；剩余时间；功率；全部阻塞原因；等待项目顺序 |
| Item_WorkshopQueueTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_WorkshopQueueRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_WorkshopQueueRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_WorkshopQueueRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_WorkshopOutput | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(1002,-36) | absolute | Image；输出缓存 |
| Txt_WorkshopOutputHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；输出缓存 |
| List_WorkshopOutput | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_WorkshopOutput | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_WorkshopOutput | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_WorkshopOutputBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；成品类型与数量；容量；满载；运输等待 |
| Item_WorkshopOutputTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_WorkshopOutputRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_WorkshopOutputRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_WorkshopOutputRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_WorkshopLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WorkshopLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WorkshopLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_WorkshopEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WorkshopEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WorkshopEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_WorkshopErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WorkshopErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WorkshopErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_WorkshopSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WorkshopSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WorkshopSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_WorkshopDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WorkshopDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WorkshopDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## RecipeDetail：配方详情

功能文档：[配方详情](RecipeDetail.md)；归属 `WorkshopForm`；内容 1488×730。

```text
Panel_PageRecipeDetail [Image]
  Txt_RecipeDetailTitle [TextMeshProUGUI]
  Grp_RecipeDetailActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_RecipeDetailEnqueue [Button + Image]
      Txt_RecipeDetailEnqueueLabel [TextMeshProUGUI]
    Btn_RecipeDetailBack [Button + Image]
      Txt_RecipeDetailBackLabel [TextMeshProUGUI]
  Panel_RecipeDetailProduct [Image]
    Txt_RecipeDetailProductHeading [TextMeshProUGUI]
    List_RecipeDetailProduct [ScrollRect vertical=true horizontal=false]
      Viewport_RecipeDetailProduct [RectMask2D]
        Content_RecipeDetailProduct [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_RecipeDetailProductBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_RecipeDetailProductTemplate [LayoutElement + Image；默认inactive]
            Btn_RecipeDetailProductRow [Button + Image]
              Txt_RecipeDetailProductRowLabel [TextMeshProUGUI]
              Txt_RecipeDetailProductRowValue [TextMeshProUGUI]
  Panel_RecipeDetailRequirements [Image]
    Txt_RecipeDetailRequirementsHeading [TextMeshProUGUI]
    List_RecipeDetailRequirements [ScrollRect vertical=true horizontal=false]
      Viewport_RecipeDetailRequirements [RectMask2D]
        Content_RecipeDetailRequirements [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_RecipeDetailRequirementsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_RecipeDetailRequirementsTemplate [LayoutElement + Image；默认inactive]
            Btn_RecipeDetailRequirementsRow [Button + Image]
              Txt_RecipeDetailRequirementsRowLabel [TextMeshProUGUI]
              Txt_RecipeDetailRequirementsRowValue [TextMeshProUGUI]
  Grp_RecipeDetailLoadingState [无Graphic]
    Panel_RecipeDetailLoadingMessage [Image]
      Txt_RecipeDetailLoadingMessage [TextMeshProUGUI]
  Grp_RecipeDetailEmptyState [无Graphic]
    Panel_RecipeDetailEmptyMessage [Image]
      Txt_RecipeDetailEmptyMessage [TextMeshProUGUI]
  Grp_RecipeDetailErrorState [无Graphic]
    Panel_RecipeDetailErrorMessage [Image]
      Txt_RecipeDetailErrorMessage [TextMeshProUGUI]
  Grp_RecipeDetailSuccessState [无Graphic]
    Panel_RecipeDetailSuccessMessage [Image]
      Txt_RecipeDetailSuccessMessage [TextMeshProUGUI]
  Grp_RecipeDetailDisabledState [无Graphic]
    Panel_RecipeDetailDisabledMessage [Image]
      Txt_RecipeDetailDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageRecipeDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；配方详情；内部页面根 |
| Txt_RecipeDetailTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；配方详情 |
| Grp_RecipeDetailActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_RecipeDetailEnqueue | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；加入一个项目，实际开工时原子检查扣除成本 |
| Txt_RecipeDetailEnqueueLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；加入制造队列 |
| Btn_RecipeDetailBack | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；保留配方选择与队列滚动 |
| Txt_RecipeDetailBackLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；返回工作台 |
| Panel_RecipeDetailProduct | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；成品 |
| Txt_RecipeDetailProductHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；成品 |
| List_RecipeDetailProduct | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_RecipeDetailProduct | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_RecipeDetailProduct | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_RecipeDetailProductBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；名称；能力；产物数量；图纸状态；当前拥有数量 |
| Item_RecipeDetailProductTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_RecipeDetailProductRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_RecipeDetailProductRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_RecipeDetailProductRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_RecipeDetailRequirements | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；制造条件 |
| Txt_RecipeDetailRequirementsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；制造条件 |
| List_RecipeDetailRequirements | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_RecipeDetailRequirements | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_RecipeDetailRequirements | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_RecipeDetailRequirementsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；每项成本；全局可用量；制造时间；资源不会在入队时预留 |
| Item_RecipeDetailRequirementsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_RecipeDetailRequirementsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_RecipeDetailRequirementsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_RecipeDetailRequirementsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_RecipeDetailLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_RecipeDetailLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_RecipeDetailLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_RecipeDetailEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_RecipeDetailEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_RecipeDetailEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_RecipeDetailErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_RecipeDetailErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_RecipeDetailErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_RecipeDetailSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_RecipeDetailSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_RecipeDetailSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_RecipeDetailDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_RecipeDetailDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_RecipeDetailDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## ManufacturingQueue：当前制造与等待队列

功能文档：[当前制造与等待队列](ManufacturingQueue.md)；归属 `WorkshopForm`；内容 1488×730。

```text
Panel_PageManufacturingQueue [Image]
  Txt_ManufacturingQueueTitle [TextMeshProUGUI]
  Grp_ManufacturingQueueActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_ManufacturingQueueCancel [Button + Image]
      Txt_ManufacturingQueueCancelLabel [TextMeshProUGUI]
    Btn_ManufacturingQueueRecipe [Button + Image]
      Txt_ManufacturingQueueRecipeLabel [TextMeshProUGUI]
  Panel_ManufacturingQueueCurrent [Image]
    Txt_ManufacturingQueueCurrentHeading [TextMeshProUGUI]
    List_ManufacturingQueueCurrent [ScrollRect vertical=true horizontal=false]
      Viewport_ManufacturingQueueCurrent [RectMask2D]
        Content_ManufacturingQueueCurrent [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ManufacturingQueueCurrentBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ManufacturingQueueCurrentTemplate [LayoutElement + Image；默认inactive]
            Btn_ManufacturingQueueCurrentRow [Button + Image]
              Txt_ManufacturingQueueCurrentRowLabel [TextMeshProUGUI]
              Txt_ManufacturingQueueCurrentRowValue [TextMeshProUGUI]
  Panel_ManufacturingQueueWaiting [Image]
    Txt_ManufacturingQueueWaitingHeading [TextMeshProUGUI]
    List_ManufacturingQueueWaiting [ScrollRect vertical=true horizontal=false]
      Viewport_ManufacturingQueueWaiting [RectMask2D]
        Content_ManufacturingQueueWaiting [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ManufacturingQueueWaitingBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ManufacturingQueueWaitingTemplate [LayoutElement + Image；默认inactive]
            Btn_ManufacturingQueueWaitingRow [Button + Image]
              Txt_ManufacturingQueueWaitingRowLabel [TextMeshProUGUI]
              Txt_ManufacturingQueueWaitingRowValue [TextMeshProUGUI]
  Grp_ManufacturingQueueLoadingState [无Graphic]
    Panel_ManufacturingQueueLoadingMessage [Image]
      Txt_ManufacturingQueueLoadingMessage [TextMeshProUGUI]
  Grp_ManufacturingQueueEmptyState [无Graphic]
    Panel_ManufacturingQueueEmptyMessage [Image]
      Txt_ManufacturingQueueEmptyMessage [TextMeshProUGUI]
  Grp_ManufacturingQueueErrorState [无Graphic]
    Panel_ManufacturingQueueErrorMessage [Image]
      Txt_ManufacturingQueueErrorMessage [TextMeshProUGUI]
  Grp_ManufacturingQueueSuccessState [无Graphic]
    Panel_ManufacturingQueueSuccessMessage [Image]
      Txt_ManufacturingQueueSuccessMessage [TextMeshProUGUI]
  Grp_ManufacturingQueueDisabledState [无Graphic]
    Panel_ManufacturingQueueDisabledMessage [Image]
      Txt_ManufacturingQueueDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageManufacturingQueue | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；当前制造与等待队列；内部页面根 |
| Txt_ManufacturingQueueTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；当前制造与等待队列 |
| Grp_ManufacturingQueueActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_ManufacturingQueueCancel | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；携带稳定队列项ID打开17取消确认；确认后重新检查是否已开始 |
| Txt_ManufacturingQueueCancelLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；取消项目 |
| Btn_ManufacturingQueueRecipe | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开同工坊配方详情 |
| Txt_ManufacturingQueueRecipeLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看配方 |
| Panel_ManufacturingQueueCurrent | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；制造详情 |
| Txt_ManufacturingQueueCurrentHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；制造详情 |
| List_ManufacturingQueueCurrent | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ManufacturingQueueCurrent | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ManufacturingQueueCurrent | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ManufacturingQueueCurrentBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；实际已扣成本；进度；剩余时间；运行／暂停；全部阻塞 |
| Item_ManufacturingQueueCurrentTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ManufacturingQueueCurrentRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ManufacturingQueueCurrentRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ManufacturingQueueCurrentRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_ManufacturingQueueWaiting | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；等待项目 |
| Txt_ManufacturingQueueWaitingHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；等待项目 |
| List_ManufacturingQueueWaiting | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ManufacturingQueueWaiting | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ManufacturingQueueWaiting | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ManufacturingQueueWaitingBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；顺序；配方；未开始状态；尚未扣费 |
| Item_ManufacturingQueueWaitingTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ManufacturingQueueWaitingRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ManufacturingQueueWaitingRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ManufacturingQueueWaitingRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_ManufacturingQueueLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ManufacturingQueueLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ManufacturingQueueLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_ManufacturingQueueEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ManufacturingQueueEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ManufacturingQueueEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_ManufacturingQueueErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ManufacturingQueueErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ManufacturingQueueErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_ManufacturingQueueSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ManufacturingQueueSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ManufacturingQueueSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_ManufacturingQueueDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ManufacturingQueueDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ManufacturingQueueDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## OutputCache：输出缓存与阻塞

功能文档：[输出缓存与阻塞](OutputCache.md)；归属 `WorkshopForm`；内容 1488×730。

```text
Panel_PageOutputCache [Image]
  Txt_OutputCacheTitle [TextMeshProUGUI]
  Grp_OutputCacheActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_OutputCacheLocate [Button + Image]
      Txt_OutputCacheLocateLabel [TextMeshProUGUI]
    Btn_OutputCacheRule [Button + Image]
      Txt_OutputCacheRuleLabel [TextMeshProUGUI]
    Btn_OutputCacheBack [Button + Image]
      Txt_OutputCacheBackLabel [TextMeshProUGUI]
  Panel_OutputCacheItems [Image]
    Txt_OutputCacheItemsHeading [TextMeshProUGUI]
    List_OutputCacheItems [ScrollRect vertical=true horizontal=false]
      Viewport_OutputCacheItems [RectMask2D]
        Content_OutputCacheItems [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_OutputCacheItemsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_OutputCacheItemsTemplate [LayoutElement + Image；默认inactive]
            Btn_OutputCacheItemsRow [Button + Image]
              Txt_OutputCacheItemsRowLabel [TextMeshProUGUI]
              Txt_OutputCacheItemsRowValue [TextMeshProUGUI]
  Panel_OutputCacheBlock [Image]
    Txt_OutputCacheBlockHeading [TextMeshProUGUI]
    List_OutputCacheBlock [ScrollRect vertical=true horizontal=false]
      Viewport_OutputCacheBlock [RectMask2D]
        Content_OutputCacheBlock [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_OutputCacheBlockBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_OutputCacheBlockTemplate [LayoutElement + Image；默认inactive]
            Btn_OutputCacheBlockRow [Button + Image]
              Txt_OutputCacheBlockRowLabel [TextMeshProUGUI]
              Txt_OutputCacheBlockRowValue [TextMeshProUGUI]
  Grp_OutputCacheLoadingState [无Graphic]
    Panel_OutputCacheLoadingMessage [Image]
      Txt_OutputCacheLoadingMessage [TextMeshProUGUI]
  Grp_OutputCacheEmptyState [无Graphic]
    Panel_OutputCacheEmptyMessage [Image]
      Txt_OutputCacheEmptyMessage [TextMeshProUGUI]
  Grp_OutputCacheErrorState [无Graphic]
    Panel_OutputCacheErrorMessage [Image]
      Txt_OutputCacheErrorMessage [TextMeshProUGUI]
  Grp_OutputCacheSuccessState [无Graphic]
    Panel_OutputCacheSuccessMessage [Image]
      Txt_OutputCacheSuccessMessage [TextMeshProUGUI]
  Grp_OutputCacheDisabledState [无Graphic]
    Panel_OutputCacheDisabledMessage [Image]
      Txt_OutputCacheDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageOutputCache | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；输出缓存与阻塞；内部页面根 |
| Txt_OutputCacheTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；输出缓存与阻塞 |
| Grp_OutputCacheActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_OutputCacheLocate | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；回世界聚焦工坊，方便配置运输机器 |
| Txt_OutputCacheLocateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；定位工坊 |
| Btn_OutputCacheRule | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开16规则说明 |
| Txt_OutputCacheRuleLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；运输规则 |
| Btn_OutputCacheBack | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；保留工作台选择 |
| Txt_OutputCacheBackLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；返回工作台 |
| Panel_OutputCacheItems | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；输出货物 |
| Txt_OutputCacheItemsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；输出货物 |
| List_OutputCacheItems | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_OutputCacheItems | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_OutputCacheItems | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_OutputCacheItemsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；类型；数量；占用／容量；等待运输 |
| Item_OutputCacheItemsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_OutputCacheItemsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_OutputCacheItemsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_OutputCacheItemsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_OutputCacheBlock | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；阻塞原因 |
| Txt_OutputCacheBlockHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；阻塞原因 |
| List_OutputCacheBlock | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_OutputCacheBlock | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_OutputCacheBlock | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_OutputCacheBlockBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；缓存满、缺电、资源不足等全部当前原因；关联对象 |
| Item_OutputCacheBlockTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_OutputCacheBlockRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_OutputCacheBlockRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_OutputCacheBlockRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_OutputCacheLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_OutputCacheLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_OutputCacheLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_OutputCacheEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_OutputCacheEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_OutputCacheEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_OutputCacheErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_OutputCacheErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_OutputCacheErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_OutputCacheSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_OutputCacheSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_OutputCacheSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_OutputCacheDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_OutputCacheDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_OutputCacheDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |
