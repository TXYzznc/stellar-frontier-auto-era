# 17-操作确认与输入 — prefab-layout

状态：2026-09-19完整设计候选，待用户集中验收；未据此修改Prefab。

继承[通用合同](../00-通用合同.md)和[共享外壳](../00-共享外壳-prefab-layout.md)。下列子树预制在所属Form的Grp_PageHost中；公共外壳由共享文档唯一声明。跨家族同属一个Form的子树合并，禁止重复根节点。表中所有Image／文本颜色、资源、射线、默认字号与状态继承通用合同，列出的数据是字段说明，不是示例业务数值。

## Rename：重命名

功能文档：[重命名](Rename.md)；归属 `OperationDialogForm`；内容 848×630。

```text
Panel_PageRename [Image]
  Txt_RenameTitle [TextMeshProUGUI]
  Grp_RenameActions [GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)]
    Btn_RenameSubmit [Button + Image]
      Txt_RenameSubmitLabel [TextMeshProUGUI]
    Btn_RenameCancel [Button + Image]
      Txt_RenameCancelLabel [TextMeshProUGUI]
  Panel_RenameInput [Image]
    Txt_RenameInputHeading [TextMeshProUGUI]
    List_RenameInput [ScrollRect vertical=true horizontal=false]
      Viewport_RenameInput [RectMask2D]
        Content_RenameInput [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_RenameInputBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_RenameInputTemplate [LayoutElement + Image；默认inactive]
            Btn_RenameInputRow [Button + Image]
              Txt_RenameInputRowLabel [TextMeshProUGUI]
              Txt_RenameInputRowValue [TextMeshProUGUI]
          Panel_RenameControls [Image + LayoutElement]
            Panel_RenameNewName [Image + TMP_InputField]
              Grp_RenameNewNameTextViewport [RectMask2D]
                Txt_RenameNewNameValue [TextMeshProUGUI]
                Txt_RenameNewNamePlaceholder [TextMeshProUGUI]
  Grp_RenameLoadingState [无Graphic]
    Panel_RenameLoadingMessage [Image]
      Txt_RenameLoadingMessage [TextMeshProUGUI]
  Grp_RenameEmptyState [无Graphic]
    Panel_RenameEmptyMessage [Image]
      Txt_RenameEmptyMessage [TextMeshProUGUI]
  Grp_RenameErrorState [无Graphic]
    Panel_RenameErrorMessage [Image]
      Txt_RenameErrorMessage [TextMeshProUGUI]
  Grp_RenameSuccessState [无Graphic]
    Panel_RenameSuccessMessage [Image]
      Txt_RenameSuccessMessage [TextMeshProUGUI]
  Grp_RenameDisabledState [无Graphic]
    Panel_RenameDisabledMessage [Image]
      Txt_RenameDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageRename | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；重命名；内部页面根 |
| Txt_RenameTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(832,32); pos(8,0) | absolute | TextMeshProUGUI；重命名 |
| Grp_RenameActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)；操作区 |
| Btn_RenameSubmit | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；空值、长度和领域命名规则验证；机器当前拥有范围内不得重名；成功刷新全部引用显示 |
| Txt_RenameSubmitLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；保存名称 |
| Btn_RenameCancel | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；不改变原名并恢复触发控件 |
| Txt_RenameCancelLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；取消 |
| Panel_RenameInput | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | Image；名称输入 |
| Txt_RenameInputHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(824,32); pos(12,-8) | absolute | TextMeshProUGUI；名称输入 |
| List_RenameInput | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_RenameInput | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_RenameInput | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_RenameInputBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(824,120)初始化; pos(0,0)初始化; LayoutElement preferred(824,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；当前名称；新名称输入；有效性；错误说明 |
| Item_RenameInputTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(824,104)初始化; pos(0,0)初始化; LayoutElement preferred(824,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_RenameInputRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_RenameInputRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_RenameInputRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_RenameControls | min(0,1) max(0,1); pivot(0,1); sizeDelta(824,88)初始化; pos(0,0)初始化; LayoutElement preferred(824,88); 最终位置/尺寸由组驱动 | group | Image + LayoutElement；真实输入字段 |
| Panel_RenameNewName | min(0,1) max(0,1); pivot(0,1); sizeDelta(792,48); pos(16,-8) | absolute | Image + TMP_InputField；新名称；onEndEdit验证 |
| Grp_RenameNewNameTextViewport | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-8); pos(0,0) | absolute | RectMask2D；textViewport，无Graphic |
| Txt_RenameNewNameValue | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；— |
| Txt_RenameNewNamePlaceholder | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；新名称 |
| Grp_RenameLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_RenameLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_RenameLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_RenameEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_RenameEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_RenameEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_RenameErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_RenameErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_RenameErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_RenameSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_RenameSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_RenameSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_RenameDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_RenameDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_RenameDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## BasicConfirm：普通操作确认

功能文档：[普通操作确认](BasicConfirm.md)；归属 `OperationDialogForm`；内容 848×630。

```text
Panel_PageBasicConfirm [Image]
  Txt_BasicConfirmTitle [TextMeshProUGUI]
  Grp_BasicConfirmActions [GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)]
    Btn_BasicConfirmCancel [Button + Image]
      Txt_BasicConfirmCancelLabel [TextMeshProUGUI]
    Btn_BasicConfirmConfirm [Button + Image]
      Txt_BasicConfirmConfirmLabel [TextMeshProUGUI]
  Panel_BasicConfirmContext [Image]
    Txt_BasicConfirmContextHeading [TextMeshProUGUI]
    List_BasicConfirmContext [ScrollRect vertical=true horizontal=false]
      Viewport_BasicConfirmContext [RectMask2D]
        Content_BasicConfirmContext [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_BasicConfirmContextBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_BasicConfirmContextTemplate [LayoutElement + Image；默认inactive]
            Btn_BasicConfirmContextRow [Button + Image]
              Txt_BasicConfirmContextRowLabel [TextMeshProUGUI]
              Txt_BasicConfirmContextRowValue [TextMeshProUGUI]
  Grp_BasicConfirmLoadingState [无Graphic]
    Panel_BasicConfirmLoadingMessage [Image]
      Txt_BasicConfirmLoadingMessage [TextMeshProUGUI]
  Grp_BasicConfirmEmptyState [无Graphic]
    Panel_BasicConfirmEmptyMessage [Image]
      Txt_BasicConfirmEmptyMessage [TextMeshProUGUI]
  Grp_BasicConfirmErrorState [无Graphic]
    Panel_BasicConfirmErrorMessage [Image]
      Txt_BasicConfirmErrorMessage [TextMeshProUGUI]
  Grp_BasicConfirmSuccessState [无Graphic]
    Panel_BasicConfirmSuccessMessage [Image]
      Txt_BasicConfirmSuccessMessage [TextMeshProUGUI]
  Grp_BasicConfirmDisabledState [无Graphic]
    Panel_BasicConfirmDisabledMessage [Image]
      Txt_BasicConfirmDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageBasicConfirm | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；普通操作确认；内部页面根 |
| Txt_BasicConfirmTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(832,32); pos(8,0) | absolute | TextMeshProUGUI；普通操作确认 |
| Grp_BasicConfirmActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)；操作区 |
| Btn_BasicConfirmCancel | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；关闭不执行；初始焦点 |
| Txt_BasicConfirmCancelLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；取消 |
| Btn_BasicConfirmConfirm | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；携带类型化请求提交一次，成功18轻提示，失败留原因与重试条件 |
| Txt_BasicConfirmConfirmLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；执行所述动作 |
| Panel_BasicConfirmContext | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | Image；操作说明 |
| Txt_BasicConfirmContextHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(824,32); pos(12,-8) | absolute | TextMeshProUGUI；操作说明 |
| List_BasicConfirmContext | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_BasicConfirmContext | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_BasicConfirmContext | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_BasicConfirmContextBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(824,120)初始化; pos(0,0)初始化; LayoutElement preferred(824,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；动作名称；目标；可逆性；具体后果；提交前条件 |
| Item_BasicConfirmContextTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(824,104)初始化; pos(0,0)初始化; LayoutElement preferred(824,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_BasicConfirmContextRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_BasicConfirmContextRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_BasicConfirmContextRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_BasicConfirmLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BasicConfirmLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BasicConfirmLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_BasicConfirmEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BasicConfirmEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BasicConfirmEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_BasicConfirmErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BasicConfirmErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BasicConfirmErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_BasicConfirmSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BasicConfirmSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BasicConfirmSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_BasicConfirmDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BasicConfirmDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BasicConfirmDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## TransactionConfirm：交易确认

功能文档：[交易确认](TransactionConfirm.md)；归属 `OperationDialogForm`；内容 848×630。

```text
Panel_PageTransactionConfirm [Image]
  Txt_TransactionConfirmTitle [TextMeshProUGUI]
  Grp_TransactionConfirmActions [GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)]
    Btn_TransactionConfirmCancel [Button + Image]
      Txt_TransactionConfirmCancelLabel [TextMeshProUGUI]
    Btn_TransactionConfirmCommit [Button + Image]
      Txt_TransactionConfirmCommitLabel [TextMeshProUGUI]
  Panel_TransactionConfirmItems [Image]
    Txt_TransactionConfirmItemsHeading [TextMeshProUGUI]
    List_TransactionConfirmItems [ScrollRect vertical=true horizontal=false]
      Viewport_TransactionConfirmItems [RectMask2D]
        Content_TransactionConfirmItems [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_TransactionConfirmItemsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_TransactionConfirmItemsTemplate [LayoutElement + Image；默认inactive]
            Btn_TransactionConfirmItemsRow [Button + Image]
              Txt_TransactionConfirmItemsRowLabel [TextMeshProUGUI]
              Txt_TransactionConfirmItemsRowValue [TextMeshProUGUI]
  Panel_TransactionConfirmQuote [Image]
    Txt_TransactionConfirmQuoteHeading [TextMeshProUGUI]
    List_TransactionConfirmQuote [ScrollRect vertical=true horizontal=false]
      Viewport_TransactionConfirmQuote [RectMask2D]
        Content_TransactionConfirmQuote [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_TransactionConfirmQuoteBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_TransactionConfirmQuoteTemplate [LayoutElement + Image；默认inactive]
            Btn_TransactionConfirmQuoteRow [Button + Image]
              Txt_TransactionConfirmQuoteRowLabel [TextMeshProUGUI]
              Txt_TransactionConfirmQuoteRowValue [TextMeshProUGUI]
  Grp_TransactionConfirmLoadingState [无Graphic]
    Panel_TransactionConfirmLoadingMessage [Image]
      Txt_TransactionConfirmLoadingMessage [TextMeshProUGUI]
  Grp_TransactionConfirmEmptyState [无Graphic]
    Panel_TransactionConfirmEmptyMessage [Image]
      Txt_TransactionConfirmEmptyMessage [TextMeshProUGUI]
  Grp_TransactionConfirmErrorState [无Graphic]
    Panel_TransactionConfirmErrorMessage [Image]
      Txt_TransactionConfirmErrorMessage [TextMeshProUGUI]
  Grp_TransactionConfirmSuccessState [无Graphic]
    Panel_TransactionConfirmSuccessMessage [Image]
      Txt_TransactionConfirmSuccessMessage [TextMeshProUGUI]
  Grp_TransactionConfirmDisabledState [无Graphic]
    Panel_TransactionConfirmDisabledMessage [Image]
      Txt_TransactionConfirmDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageTransactionConfirm | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；交易确认；内部页面根 |
| Txt_TransactionConfirmTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(832,32); pos(8,0) | absolute | TextMeshProUGUI；交易确认 |
| Grp_TransactionConfirmActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)；操作区 |
| Btn_TransactionConfirmCancel | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；不扣物资 |
| Txt_TransactionConfirmCancelLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；取消交易 |
| Btn_TransactionConfirmCommit | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；最终重验并原子执行；条件变动拒绝整笔，刷新后重新确认 |
| Txt_TransactionConfirmCommitLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；购买或出售 |
| Panel_TransactionConfirmItems | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(0,-36) | absolute | Image；交易内容 |
| Txt_TransactionConfirmItemsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；交易内容 |
| List_TransactionConfirmItems | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_TransactionConfirmItems | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_TransactionConfirmItems | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_TransactionConfirmItemsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；全部商品或实例；数量；礼包展开；交付去向 |
| Item_TransactionConfirmItemsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_TransactionConfirmItemsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_TransactionConfirmItemsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_TransactionConfirmItemsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_TransactionConfirmQuote | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(432,-36) | absolute | Image；实际金额 |
| Txt_TransactionConfirmQuoteHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；实际金额 |
| List_TransactionConfirmQuote | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_TransactionConfirmQuote | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_TransactionConfirmQuote | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_TransactionConfirmQuoteBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；实际单价与总价／总收入；余额变化；限购；空间；实时条件 |
| Item_TransactionConfirmQuoteTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_TransactionConfirmQuoteRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_TransactionConfirmQuoteRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_TransactionConfirmQuoteRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_TransactionConfirmLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_TransactionConfirmLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_TransactionConfirmLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_TransactionConfirmEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_TransactionConfirmEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_TransactionConfirmEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_TransactionConfirmErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_TransactionConfirmErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_TransactionConfirmErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_TransactionConfirmSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_TransactionConfirmSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_TransactionConfirmSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_TransactionConfirmDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_TransactionConfirmDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_TransactionConfirmDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## UpgradeConfirm：升级确认

功能文档：[升级确认](UpgradeConfirm.md)；归属 `OperationDialogForm`；内容 848×630。

```text
Panel_PageUpgradeConfirm [Image]
  Txt_UpgradeConfirmTitle [TextMeshProUGUI]
  Grp_UpgradeConfirmActions [GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)]
    Btn_UpgradeConfirmCancel [Button + Image]
      Txt_UpgradeConfirmCancelLabel [TextMeshProUGUI]
    Btn_UpgradeConfirmCommit [Button + Image]
      Txt_UpgradeConfirmCommitLabel [TextMeshProUGUI]
  Panel_UpgradeConfirmComparison [Image]
    Txt_UpgradeConfirmComparisonHeading [TextMeshProUGUI]
    List_UpgradeConfirmComparison [ScrollRect vertical=true horizontal=false]
      Viewport_UpgradeConfirmComparison [RectMask2D]
        Content_UpgradeConfirmComparison [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_UpgradeConfirmComparisonBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_UpgradeConfirmComparisonTemplate [LayoutElement + Image；默认inactive]
            Btn_UpgradeConfirmComparisonRow [Button + Image]
              Txt_UpgradeConfirmComparisonRowLabel [TextMeshProUGUI]
              Txt_UpgradeConfirmComparisonRowValue [TextMeshProUGUI]
  Panel_UpgradeConfirmCosts [Image]
    Txt_UpgradeConfirmCostsHeading [TextMeshProUGUI]
    List_UpgradeConfirmCosts [ScrollRect vertical=true horizontal=false]
      Viewport_UpgradeConfirmCosts [RectMask2D]
        Content_UpgradeConfirmCosts [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_UpgradeConfirmCostsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_UpgradeConfirmCostsTemplate [LayoutElement + Image；默认inactive]
            Btn_UpgradeConfirmCostsRow [Button + Image]
              Txt_UpgradeConfirmCostsRowLabel [TextMeshProUGUI]
              Txt_UpgradeConfirmCostsRowValue [TextMeshProUGUI]
  Grp_UpgradeConfirmLoadingState [无Graphic]
    Panel_UpgradeConfirmLoadingMessage [Image]
      Txt_UpgradeConfirmLoadingMessage [TextMeshProUGUI]
  Grp_UpgradeConfirmEmptyState [无Graphic]
    Panel_UpgradeConfirmEmptyMessage [Image]
      Txt_UpgradeConfirmEmptyMessage [TextMeshProUGUI]
  Grp_UpgradeConfirmErrorState [无Graphic]
    Panel_UpgradeConfirmErrorMessage [Image]
      Txt_UpgradeConfirmErrorMessage [TextMeshProUGUI]
  Grp_UpgradeConfirmSuccessState [无Graphic]
    Panel_UpgradeConfirmSuccessMessage [Image]
      Txt_UpgradeConfirmSuccessMessage [TextMeshProUGUI]
  Grp_UpgradeConfirmDisabledState [无Graphic]
    Panel_UpgradeConfirmDisabledMessage [Image]
      Txt_UpgradeConfirmDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageUpgradeConfirm | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；升级确认；内部页面根 |
| Txt_UpgradeConfirmTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(832,32); pos(8,0) | absolute | TextMeshProUGUI；升级确认 |
| Grp_UpgradeConfirmActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)；操作区 |
| Btn_UpgradeConfirmCancel | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；保持原等级与库存 |
| Txt_UpgradeConfirmCancelLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；取消 |
| Btn_UpgradeConfirmCommit | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；对象、等级与成本重验；安全停机等待期间不扣费；执行成功才扣除 |
| Txt_UpgradeConfirmCommitLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；升级 |
| Panel_UpgradeConfirmComparison | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(0,-36) | absolute | Image；升级对象 |
| Txt_UpgradeConfirmComparisonHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；升级对象 |
| List_UpgradeConfirmComparison | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_UpgradeConfirmComparison | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_UpgradeConfirmComparison | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_UpgradeConfirmComparisonBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；实例；当前／目标等级；属性变化 |
| Item_UpgradeConfirmComparisonTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_UpgradeConfirmComparisonRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_UpgradeConfirmComparisonRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_UpgradeConfirmComparisonRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_UpgradeConfirmCosts | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(432,-36) | absolute | Image；成本与影响 |
| Txt_UpgradeConfirmCostsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；成本与影响 |
| List_UpgradeConfirmCosts | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_UpgradeConfirmCosts | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_UpgradeConfirmCosts | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_UpgradeConfirmCostsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；原价；实价；扶持折扣；资源；安全停机与绑定影响 |
| Item_UpgradeConfirmCostsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_UpgradeConfirmCostsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_UpgradeConfirmCostsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_UpgradeConfirmCostsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_UpgradeConfirmLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_UpgradeConfirmLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_UpgradeConfirmLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_UpgradeConfirmEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_UpgradeConfirmEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_UpgradeConfirmEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_UpgradeConfirmErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_UpgradeConfirmErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_UpgradeConfirmErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_UpgradeConfirmSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_UpgradeConfirmSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_UpgradeConfirmSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_UpgradeConfirmDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_UpgradeConfirmDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_UpgradeConfirmDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## PlacementConfirm：建造与部署确认

功能文档：[建造与部署确认](PlacementConfirm.md)；归属 `OperationDialogForm`；内容 848×630。

```text
Panel_PagePlacementConfirm [Image]
  Txt_PlacementConfirmTitle [TextMeshProUGUI]
  Grp_PlacementConfirmActions [GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)]
    Btn_PlacementConfirmCancel [Button + Image]
      Txt_PlacementConfirmCancelLabel [TextMeshProUGUI]
    Btn_PlacementConfirmCommit [Button + Image]
      Txt_PlacementConfirmCommitLabel [TextMeshProUGUI]
  Panel_PlacementConfirmTarget [Image]
    Txt_PlacementConfirmTargetHeading [TextMeshProUGUI]
    List_PlacementConfirmTarget [ScrollRect vertical=true horizontal=false]
      Viewport_PlacementConfirmTarget [RectMask2D]
        Content_PlacementConfirmTarget [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_PlacementConfirmTargetBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_PlacementConfirmTargetTemplate [LayoutElement + Image；默认inactive]
            Btn_PlacementConfirmTargetRow [Button + Image]
              Txt_PlacementConfirmTargetRowLabel [TextMeshProUGUI]
              Txt_PlacementConfirmTargetRowValue [TextMeshProUGUI]
  Panel_PlacementConfirmImpact [Image]
    Txt_PlacementConfirmImpactHeading [TextMeshProUGUI]
    List_PlacementConfirmImpact [ScrollRect vertical=true horizontal=false]
      Viewport_PlacementConfirmImpact [RectMask2D]
        Content_PlacementConfirmImpact [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_PlacementConfirmImpactBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_PlacementConfirmImpactTemplate [LayoutElement + Image；默认inactive]
            Btn_PlacementConfirmImpactRow [Button + Image]
              Txt_PlacementConfirmImpactRowLabel [TextMeshProUGUI]
              Txt_PlacementConfirmImpactRowValue [TextMeshProUGUI]
  Grp_PlacementConfirmLoadingState [无Graphic]
    Panel_PlacementConfirmLoadingMessage [Image]
      Txt_PlacementConfirmLoadingMessage [TextMeshProUGUI]
  Grp_PlacementConfirmEmptyState [无Graphic]
    Panel_PlacementConfirmEmptyMessage [Image]
      Txt_PlacementConfirmEmptyMessage [TextMeshProUGUI]
  Grp_PlacementConfirmErrorState [无Graphic]
    Panel_PlacementConfirmErrorMessage [Image]
      Txt_PlacementConfirmErrorMessage [TextMeshProUGUI]
  Grp_PlacementConfirmSuccessState [无Graphic]
    Panel_PlacementConfirmSuccessMessage [Image]
      Txt_PlacementConfirmSuccessMessage [TextMeshProUGUI]
  Grp_PlacementConfirmDisabledState [无Graphic]
    Panel_PlacementConfirmDisabledMessage [Image]
      Txt_PlacementConfirmDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PagePlacementConfirm | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；建造与部署确认；内部页面根 |
| Txt_PlacementConfirmTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(832,32); pos(8,0) | absolute | TextMeshProUGUI；建造与部署确认 |
| Grp_PlacementConfirmActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)；操作区 |
| Btn_PlacementConfirmCancel | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；保留候选位置不扣费 |
| Txt_PlacementConfirmCancelLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；返回预览 |
| Btn_PlacementConfirmCommit | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；最终重验位置与业务条件；成功退出放置；失败回预览显示原因 |
| Txt_PlacementConfirmCommitLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；建造或部署 |
| Panel_PlacementConfirmTarget | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(0,-36) | absolute | Image；位置与对象 |
| Txt_PlacementConfirmTargetHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；位置与对象 |
| List_PlacementConfirmTarget | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_PlacementConfirmTarget | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_PlacementConfirmTarget | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_PlacementConfirmTargetBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；图纸或机器实例；区域；位置；朝向；占地 |
| Item_PlacementConfirmTargetTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_PlacementConfirmTargetRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_PlacementConfirmTargetRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_PlacementConfirmTargetRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_PlacementConfirmImpact | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(432,-36) | absolute | Image；提交后效果 |
| Txt_PlacementConfirmImpactHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；提交后效果 |
| List_PlacementConfirmImpact | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_PlacementConfirmImpact | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_PlacementConfirmImpact | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_PlacementConfirmImpactBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；建造成本／施工时间，或机器移出库且未激活；位置仍可能变化 |
| Item_PlacementConfirmImpactTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_PlacementConfirmImpactRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_PlacementConfirmImpactRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_PlacementConfirmImpactRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_PlacementConfirmLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_PlacementConfirmLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_PlacementConfirmLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_PlacementConfirmEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_PlacementConfirmEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_PlacementConfirmEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_PlacementConfirmErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_PlacementConfirmErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_PlacementConfirmErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_PlacementConfirmSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_PlacementConfirmSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_PlacementConfirmSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_PlacementConfirmDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_PlacementConfirmDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_PlacementConfirmDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## CancelProduction：施工制造取消与退款

功能文档：[施工制造取消与退款](CancelProduction.md)；归属 `OperationDialogForm`；内容 848×630。

```text
Panel_PageCancelProduction [Image]
  Txt_CancelProductionTitle [TextMeshProUGUI]
  Grp_CancelProductionActions [GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)]
    Btn_CancelProductionKeep [Button + Image]
      Txt_CancelProductionKeepLabel [TextMeshProUGUI]
    Btn_CancelProductionCancelJob [Button + Image]
      Txt_CancelProductionCancelJobLabel [TextMeshProUGUI]
  Panel_CancelProductionTarget [Image]
    Txt_CancelProductionTargetHeading [TextMeshProUGUI]
    List_CancelProductionTarget [ScrollRect vertical=true horizontal=false]
      Viewport_CancelProductionTarget [RectMask2D]
        Content_CancelProductionTarget [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_CancelProductionTargetBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_CancelProductionTargetTemplate [LayoutElement + Image；默认inactive]
            Btn_CancelProductionTargetRow [Button + Image]
              Txt_CancelProductionTargetRowLabel [TextMeshProUGUI]
              Txt_CancelProductionTargetRowValue [TextMeshProUGUI]
  Panel_CancelProductionRefund [Image]
    Txt_CancelProductionRefundHeading [TextMeshProUGUI]
    List_CancelProductionRefund [ScrollRect vertical=true horizontal=false]
      Viewport_CancelProductionRefund [RectMask2D]
        Content_CancelProductionRefund [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_CancelProductionRefundBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_CancelProductionRefundTemplate [LayoutElement + Image；默认inactive]
            Btn_CancelProductionRefundRow [Button + Image]
              Txt_CancelProductionRefundRowLabel [TextMeshProUGUI]
              Txt_CancelProductionRefundRowValue [TextMeshProUGUI]
  Grp_CancelProductionLoadingState [无Graphic]
    Panel_CancelProductionLoadingMessage [Image]
      Txt_CancelProductionLoadingMessage [TextMeshProUGUI]
  Grp_CancelProductionEmptyState [无Graphic]
    Panel_CancelProductionEmptyMessage [Image]
      Txt_CancelProductionEmptyMessage [TextMeshProUGUI]
  Grp_CancelProductionErrorState [无Graphic]
    Panel_CancelProductionErrorMessage [Image]
      Txt_CancelProductionErrorMessage [TextMeshProUGUI]
  Grp_CancelProductionSuccessState [无Graphic]
    Panel_CancelProductionSuccessMessage [Image]
      Txt_CancelProductionSuccessMessage [TextMeshProUGUI]
  Grp_CancelProductionDisabledState [无Graphic]
    Panel_CancelProductionDisabledMessage [Image]
      Txt_CancelProductionDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageCancelProduction | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；施工制造取消与退款；内部页面根 |
| Txt_CancelProductionTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(832,32); pos(8,0) | absolute | TextMeshProUGUI；施工制造取消与退款 |
| Grp_CancelProductionActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)；操作区 |
| Btn_CancelProductionKeep | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；关闭且不改变项目 |
| Txt_CancelProductionKeepLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；继续保留 |
| Btn_CancelProductionCancelJob | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；提交再算退款；进度或阶段变化时刷新报价要求重新确认；已完成拒绝取消 |
| Txt_CancelProductionCancelJobLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；取消项目 |
| Panel_CancelProductionTarget | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(0,-36) | absolute | Image；待取消项目 |
| Txt_CancelProductionTargetHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；待取消项目 |
| List_CancelProductionTarget | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_CancelProductionTarget | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_CancelProductionTarget | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_CancelProductionTargetBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；稳定ID；施工／制造／未开始队列项；最新进度 |
| Item_CancelProductionTargetTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_CancelProductionTargetRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_CancelProductionTargetRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_CancelProductionTargetRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_CancelProductionRefund | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(432,-36) | absolute | Image；实际返还 |
| Txt_CancelProductionRefundHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；实际返还 |
| List_CancelProductionRefund | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_CancelProductionRefund | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_CancelProductionRefund | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_CancelProductionRefundBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；逐成本项原投入；剩余比例；向下取整退款；取消不产生成品 |
| Item_CancelProductionRefundTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_CancelProductionRefundRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_CancelProductionRefundRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_CancelProductionRefundRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_CancelProductionLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_CancelProductionLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_CancelProductionLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_CancelProductionEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_CancelProductionEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_CancelProductionEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_CancelProductionErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_CancelProductionErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_CancelProductionErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_CancelProductionSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_CancelProductionSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_CancelProductionSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_CancelProductionDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_CancelProductionDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_CancelProductionDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## HardwareConfirm：硬件修改确认

功能文档：[硬件修改确认](HardwareConfirm.md)；归属 `OperationDialogForm`；内容 848×630。

```text
Panel_PageHardwareConfirm [Image]
  Txt_HardwareConfirmTitle [TextMeshProUGUI]
  Grp_HardwareConfirmActions [GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)]
    Btn_HardwareConfirmKeep [Button + Image]
      Txt_HardwareConfirmKeepLabel [TextMeshProUGUI]
    Btn_HardwareConfirmCommit [Button + Image]
      Txt_HardwareConfirmCommitLabel [TextMeshProUGUI]
  Panel_HardwareConfirmChange [Image]
    Txt_HardwareConfirmChangeHeading [TextMeshProUGUI]
    List_HardwareConfirmChange [ScrollRect vertical=true horizontal=false]
      Viewport_HardwareConfirmChange [RectMask2D]
        Content_HardwareConfirmChange [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_HardwareConfirmChangeBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_HardwareConfirmChangeTemplate [LayoutElement + Image；默认inactive]
            Btn_HardwareConfirmChangeRow [Button + Image]
              Txt_HardwareConfirmChangeRowLabel [TextMeshProUGUI]
              Txt_HardwareConfirmChangeRowValue [TextMeshProUGUI]
  Panel_HardwareConfirmEffects [Image]
    Txt_HardwareConfirmEffectsHeading [TextMeshProUGUI]
    List_HardwareConfirmEffects [ScrollRect vertical=true horizontal=false]
      Viewport_HardwareConfirmEffects [RectMask2D]
        Content_HardwareConfirmEffects [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_HardwareConfirmEffectsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_HardwareConfirmEffectsTemplate [LayoutElement + Image；默认inactive]
            Btn_HardwareConfirmEffectsRow [Button + Image]
              Txt_HardwareConfirmEffectsRowLabel [TextMeshProUGUI]
              Txt_HardwareConfirmEffectsRowValue [TextMeshProUGUI]
  Grp_HardwareConfirmLoadingState [无Graphic]
    Panel_HardwareConfirmLoadingMessage [Image]
      Txt_HardwareConfirmLoadingMessage [TextMeshProUGUI]
  Grp_HardwareConfirmEmptyState [无Graphic]
    Panel_HardwareConfirmEmptyMessage [Image]
      Txt_HardwareConfirmEmptyMessage [TextMeshProUGUI]
  Grp_HardwareConfirmErrorState [无Graphic]
    Panel_HardwareConfirmErrorMessage [Image]
      Txt_HardwareConfirmErrorMessage [TextMeshProUGUI]
  Grp_HardwareConfirmSuccessState [无Graphic]
    Panel_HardwareConfirmSuccessMessage [Image]
      Txt_HardwareConfirmSuccessMessage [TextMeshProUGUI]
  Grp_HardwareConfirmDisabledState [无Graphic]
    Panel_HardwareConfirmDisabledMessage [Image]
      Txt_HardwareConfirmDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageHardwareConfirm | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；硬件修改确认；内部页面根 |
| Txt_HardwareConfirmTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(832,32); pos(8,0) | absolute | TextMeshProUGUI；硬件修改确认 |
| Grp_HardwareConfirmActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)；操作区 |
| Btn_HardwareConfirmKeep | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；不提交修改 |
| Txt_HardwareConfirmKeepLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；保留配置 |
| Btn_HardwareConfirmCommit | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；现场或整备权限重验；冲突操作锁定；转18安全停止等待或成功 |
| Txt_HardwareConfirmCommitLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；提交硬件修改 |
| Panel_HardwareConfirmChange | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(0,-36) | absolute | Image；变更清单 |
| Txt_HardwareConfirmChangeHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；变更清单 |
| List_HardwareConfirmChange | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_HardwareConfirmChange | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_HardwareConfirmChange | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_HardwareConfirmChangeBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；机器；槽位；卸下和装入实例；库存去向 |
| Item_HardwareConfirmChangeTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_HardwareConfirmChangeRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_HardwareConfirmChangeRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_HardwareConfirmChangeRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_HardwareConfirmEffects | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(432,-36) | absolute | Image；运行影响 |
| Txt_HardwareConfirmEffectsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；运行影响 |
| List_HardwareConfirmEffects | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_HardwareConfirmEffects | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_HardwareConfirmEffects | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_HardwareConfirmEffectsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；当前行为；等待安全停机；能力和绑定失效；保持激活；不自动重绑 |
| Item_HardwareConfirmEffectsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_HardwareConfirmEffectsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_HardwareConfirmEffectsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_HardwareConfirmEffectsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_HardwareConfirmLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HardwareConfirmLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HardwareConfirmLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_HardwareConfirmEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HardwareConfirmEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HardwareConfirmEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_HardwareConfirmErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HardwareConfirmErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HardwareConfirmErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_HardwareConfirmSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HardwareConfirmSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HardwareConfirmSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_HardwareConfirmDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_HardwareConfirmDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_HardwareConfirmDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## MachineRecovery：机器回收确认

功能文档：[机器回收确认](MachineRecovery.md)；归属 `OperationDialogForm`；内容 848×630。

```text
Panel_PageMachineRecovery [Image]
  Txt_MachineRecoveryTitle [TextMeshProUGUI]
  Grp_MachineRecoveryActions [GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)]
    Btn_MachineRecoveryKeep [Button + Image]
      Txt_MachineRecoveryKeepLabel [TextMeshProUGUI]
    Btn_MachineRecoveryCommit [Button + Image]
      Txt_MachineRecoveryCommitLabel [TextMeshProUGUI]
  Panel_MachineRecoveryMachine [Image]
    Txt_MachineRecoveryMachineHeading [TextMeshProUGUI]
    List_MachineRecoveryMachine [ScrollRect vertical=true horizontal=false]
      Viewport_MachineRecoveryMachine [RectMask2D]
        Content_MachineRecoveryMachine [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_MachineRecoveryMachineBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_MachineRecoveryMachineTemplate [LayoutElement + Image；默认inactive]
            Btn_MachineRecoveryMachineRow [Button + Image]
              Txt_MachineRecoveryMachineRowLabel [TextMeshProUGUI]
              Txt_MachineRecoveryMachineRowValue [TextMeshProUGUI]
  Panel_MachineRecoveryResult [Image]
    Txt_MachineRecoveryResultHeading [TextMeshProUGUI]
    List_MachineRecoveryResult [ScrollRect vertical=true horizontal=false]
      Viewport_MachineRecoveryResult [RectMask2D]
        Content_MachineRecoveryResult [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_MachineRecoveryResultBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_MachineRecoveryResultTemplate [LayoutElement + Image；默认inactive]
            Btn_MachineRecoveryResultRow [Button + Image]
              Txt_MachineRecoveryResultRowLabel [TextMeshProUGUI]
              Txt_MachineRecoveryResultRowValue [TextMeshProUGUI]
  Grp_MachineRecoveryLoadingState [无Graphic]
    Panel_MachineRecoveryLoadingMessage [Image]
      Txt_MachineRecoveryLoadingMessage [TextMeshProUGUI]
  Grp_MachineRecoveryEmptyState [无Graphic]
    Panel_MachineRecoveryEmptyMessage [Image]
      Txt_MachineRecoveryEmptyMessage [TextMeshProUGUI]
  Grp_MachineRecoveryErrorState [无Graphic]
    Panel_MachineRecoveryErrorMessage [Image]
      Txt_MachineRecoveryErrorMessage [TextMeshProUGUI]
  Grp_MachineRecoverySuccessState [无Graphic]
    Panel_MachineRecoverySuccessMessage [Image]
      Txt_MachineRecoverySuccessMessage [TextMeshProUGUI]
  Grp_MachineRecoveryDisabledState [无Graphic]
    Panel_MachineRecoveryDisabledMessage [Image]
      Txt_MachineRecoveryDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageMachineRecovery | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；机器回收确认；内部页面根 |
| Txt_MachineRecoveryTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(832,32); pos(8,0) | absolute | TextMeshProUGUI；机器回收确认 |
| Grp_MachineRecoveryActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)；操作区 |
| Btn_MachineRecoveryKeep | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；不改变世界 |
| Txt_MachineRecoveryKeepLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；保留部署 |
| Btn_MachineRecoveryCommit | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；资格失败显示原因；不得销毁未处理货物或责任；服务成功才移出世界 |
| Txt_MachineRecoveryCommitLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；回收机器 |
| Panel_MachineRecoveryMachine | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(0,-36) | absolute | Image；回收对象 |
| Txt_MachineRecoveryMachineHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；回收对象 |
| List_MachineRecoveryMachine | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_MachineRecoveryMachine | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_MachineRecoveryMachine | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_MachineRecoveryMachineBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；名称；配置；激活与行为；货舱和未解决责任 |
| Item_MachineRecoveryMachineTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_MachineRecoveryMachineRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_MachineRecoveryMachineRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_MachineRecoveryMachineRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_MachineRecoveryResult | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(432,-36) | absolute | Image；回收条件与去向 |
| Txt_MachineRecoveryResultHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；回收条件与去向 |
| List_MachineRecoveryResult | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_MachineRecoveryResult | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_MachineRecoveryResult | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_MachineRecoveryResultBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；领域回收资格；需要安全停止；保留组件配置返回未部署库 |
| Item_MachineRecoveryResultTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_MachineRecoveryResultRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_MachineRecoveryResultRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_MachineRecoveryResultRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_MachineRecoveryLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineRecoveryLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineRecoveryLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_MachineRecoveryEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineRecoveryEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineRecoveryEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_MachineRecoveryErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineRecoveryErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineRecoveryErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_MachineRecoverySuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineRecoverySuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineRecoverySuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_MachineRecoveryDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineRecoveryDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineRecoveryDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## AlgorithmApply：算法应用确认

功能文档：[算法应用确认](AlgorithmApply.md)；归属 `OperationDialogForm`；内容 848×630。

```text
Panel_PageAlgorithmApply [Image]
  Txt_AlgorithmApplyTitle [TextMeshProUGUI]
  Grp_AlgorithmApplyActions [GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)]
    Btn_AlgorithmApplyBack [Button + Image]
      Txt_AlgorithmApplyBackLabel [TextMeshProUGUI]
    Btn_AlgorithmApplyApply [Button + Image]
      Txt_AlgorithmApplyApplyLabel [TextMeshProUGUI]
  Panel_AlgorithmApplyVersion [Image]
    Txt_AlgorithmApplyVersionHeading [TextMeshProUGUI]
    List_AlgorithmApplyVersion [ScrollRect vertical=true horizontal=false]
      Viewport_AlgorithmApplyVersion [RectMask2D]
        Content_AlgorithmApplyVersion [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_AlgorithmApplyVersionBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_AlgorithmApplyVersionTemplate [LayoutElement + Image；默认inactive]
            Btn_AlgorithmApplyVersionRow [Button + Image]
              Txt_AlgorithmApplyVersionRowLabel [TextMeshProUGUI]
              Txt_AlgorithmApplyVersionRowValue [TextMeshProUGUI]
  Panel_AlgorithmApplyValidation [Image]
    Txt_AlgorithmApplyValidationHeading [TextMeshProUGUI]
    List_AlgorithmApplyValidation [ScrollRect vertical=true horizontal=false]
      Viewport_AlgorithmApplyValidation [RectMask2D]
        Content_AlgorithmApplyValidation [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_AlgorithmApplyValidationBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_AlgorithmApplyValidationTemplate [LayoutElement + Image；默认inactive]
            Btn_AlgorithmApplyValidationRow [Button + Image]
              Txt_AlgorithmApplyValidationRowLabel [TextMeshProUGUI]
              Txt_AlgorithmApplyValidationRowValue [TextMeshProUGUI]
  Grp_AlgorithmApplyLoadingState [无Graphic]
    Panel_AlgorithmApplyLoadingMessage [Image]
      Txt_AlgorithmApplyLoadingMessage [TextMeshProUGUI]
  Grp_AlgorithmApplyEmptyState [无Graphic]
    Panel_AlgorithmApplyEmptyMessage [Image]
      Txt_AlgorithmApplyEmptyMessage [TextMeshProUGUI]
  Grp_AlgorithmApplyErrorState [无Graphic]
    Panel_AlgorithmApplyErrorMessage [Image]
      Txt_AlgorithmApplyErrorMessage [TextMeshProUGUI]
  Grp_AlgorithmApplySuccessState [无Graphic]
    Panel_AlgorithmApplySuccessMessage [Image]
      Txt_AlgorithmApplySuccessMessage [TextMeshProUGUI]
  Grp_AlgorithmApplyDisabledState [无Graphic]
    Panel_AlgorithmApplyDisabledMessage [Image]
      Txt_AlgorithmApplyDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageAlgorithmApply | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；算法应用确认；内部页面根 |
| Txt_AlgorithmApplyTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(832,32); pos(8,0) | absolute | TextMeshProUGUI；算法应用确认 |
| Grp_AlgorithmApplyActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)；操作区 |
| Btn_AlgorithmApplyBack | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；保留草稿 |
| Txt_AlgorithmApplyBackLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；继续编辑 |
| Btn_AlgorithmApplyApply | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；错误阻止；警告允许明确确认；服务安全点原子切换，旧版本此前继续运行 |
| Txt_AlgorithmApplyApplyLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；应用版本 |
| Panel_AlgorithmApplyVersion | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(0,-36) | absolute | Image；版本变更 |
| Txt_AlgorithmApplyVersionHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；版本变更 |
| List_AlgorithmApplyVersion | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_AlgorithmApplyVersion | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_AlgorithmApplyVersion | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_AlgorithmApplyVersionBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；机器／算法；当前与草稿版本；逻辑和算力变化 |
| Item_AlgorithmApplyVersionTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_AlgorithmApplyVersionRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_AlgorithmApplyVersionRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_AlgorithmApplyVersionRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_AlgorithmApplyValidation | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(432,-36) | absolute | Image；校验与影响 |
| Txt_AlgorithmApplyValidationHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；校验与影响 |
| List_AlgorithmApplyValidation | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_AlgorithmApplyValidation | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_AlgorithmApplyValidation | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_AlgorithmApplyValidationBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；错误／警告；绑定；受影响实例；安全点等待；状态迁移说明 |
| Item_AlgorithmApplyValidationTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_AlgorithmApplyValidationRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_AlgorithmApplyValidationRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_AlgorithmApplyValidationRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_AlgorithmApplyLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_AlgorithmApplyLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_AlgorithmApplyLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_AlgorithmApplyEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_AlgorithmApplyEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_AlgorithmApplyEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_AlgorithmApplyErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_AlgorithmApplyErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_AlgorithmApplyErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_AlgorithmApplySuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_AlgorithmApplySuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_AlgorithmApplySuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_AlgorithmApplyDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_AlgorithmApplyDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_AlgorithmApplyDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## StrongConfirm：删除覆盖与算法重置

功能文档：[删除覆盖与算法重置](StrongConfirm.md)；归属 `OperationDialogForm`；内容 848×630。

```text
Panel_PageStrongConfirm [Image]
  Txt_StrongConfirmTitle [TextMeshProUGUI]
  Grp_StrongConfirmActions [GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)]
    Btn_StrongConfirmCancel [Button + Image]
      Txt_StrongConfirmCancelLabel [TextMeshProUGUI]
    Btn_StrongConfirmCommit [Button + Image]
      Txt_StrongConfirmCommitLabel [TextMeshProUGUI]
  Panel_StrongConfirmTarget [Image]
    Txt_StrongConfirmTargetHeading [TextMeshProUGUI]
    List_StrongConfirmTarget [ScrollRect vertical=true horizontal=false]
      Viewport_StrongConfirmTarget [RectMask2D]
        Content_StrongConfirmTarget [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_StrongConfirmTargetBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_StrongConfirmTargetTemplate [LayoutElement + Image；默认inactive]
            Btn_StrongConfirmTargetRow [Button + Image]
              Txt_StrongConfirmTargetRowLabel [TextMeshProUGUI]
              Txt_StrongConfirmTargetRowValue [TextMeshProUGUI]
          Panel_StrongConfirmControls [Image + LayoutElement]
            Tgl_StrongConfirmAcknowledged [Toggle]
              Img_StrongConfirmAcknowledgedBox [Image]
                Icon_StrongConfirmAcknowledgedCheck [Image]
              Txt_StrongConfirmAcknowledgedLabel [TextMeshProUGUI]
  Panel_StrongConfirmConsequences [Image]
    Txt_StrongConfirmConsequencesHeading [TextMeshProUGUI]
    List_StrongConfirmConsequences [ScrollRect vertical=true horizontal=false]
      Viewport_StrongConfirmConsequences [RectMask2D]
        Content_StrongConfirmConsequences [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_StrongConfirmConsequencesBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_StrongConfirmConsequencesTemplate [LayoutElement + Image；默认inactive]
            Btn_StrongConfirmConsequencesRow [Button + Image]
              Txt_StrongConfirmConsequencesRowLabel [TextMeshProUGUI]
              Txt_StrongConfirmConsequencesRowValue [TextMeshProUGUI]
  Panel_StrongConfirmAcknowledgement [Image]
    Txt_StrongConfirmAcknowledgementHeading [TextMeshProUGUI]
    List_StrongConfirmAcknowledgement [ScrollRect vertical=true horizontal=false]
      Viewport_StrongConfirmAcknowledgement [RectMask2D]
        Content_StrongConfirmAcknowledgement [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_StrongConfirmAcknowledgementBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_StrongConfirmAcknowledgementTemplate [LayoutElement + Image；默认inactive]
            Btn_StrongConfirmAcknowledgementRow [Button + Image]
              Txt_StrongConfirmAcknowledgementRowLabel [TextMeshProUGUI]
              Txt_StrongConfirmAcknowledgementRowValue [TextMeshProUGUI]
  Grp_StrongConfirmLoadingState [无Graphic]
    Panel_StrongConfirmLoadingMessage [Image]
      Txt_StrongConfirmLoadingMessage [TextMeshProUGUI]
  Grp_StrongConfirmEmptyState [无Graphic]
    Panel_StrongConfirmEmptyMessage [Image]
      Txt_StrongConfirmEmptyMessage [TextMeshProUGUI]
  Grp_StrongConfirmErrorState [无Graphic]
    Panel_StrongConfirmErrorMessage [Image]
      Txt_StrongConfirmErrorMessage [TextMeshProUGUI]
  Grp_StrongConfirmSuccessState [无Graphic]
    Panel_StrongConfirmSuccessMessage [Image]
      Txt_StrongConfirmSuccessMessage [TextMeshProUGUI]
  Grp_StrongConfirmDisabledState [无Graphic]
    Panel_StrongConfirmDisabledMessage [Image]
      Txt_StrongConfirmDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageStrongConfirm | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；删除覆盖与算法重置；内部页面根 |
| Txt_StrongConfirmTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(832,32); pos(8,0) | absolute | TextMeshProUGUI；删除覆盖与算法重置 |
| Grp_StrongConfirmActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)；操作区 |
| Btn_StrongConfirmCancel | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；初始焦点；无变更 |
| Txt_StrongConfirmCancelLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；取消 |
| Btn_StrongConfirmCommit | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；核对类型与目标版本后执行；按钮显示删除进度／覆盖进度／删除模板／重置状态 |
| Txt_StrongConfirmCommitLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；执行强确认动作 |
| Panel_StrongConfirmTarget | min(0,1) max(0,1); pivot(0,1); sizeDelta(272,534); pos(0,-36) | absolute | Image；不可逆对象 |
| Txt_StrongConfirmTargetHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,32); pos(12,-8) | absolute | TextMeshProUGUI；不可逆对象 |
| List_StrongConfirmTarget | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_StrongConfirmTarget | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_StrongConfirmTarget | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_StrongConfirmTargetBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,120)初始化; pos(0,0)初始化; LayoutElement preferred(248,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；动作；目标名称与身份；具体影响范围 |
| Item_StrongConfirmTargetTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,104)初始化; pos(0,0)初始化; LayoutElement preferred(248,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_StrongConfirmTargetRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_StrongConfirmTargetRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_StrongConfirmTargetRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_StrongConfirmControls | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,88)初始化; pos(0,0)初始化; LayoutElement preferred(248,88); 最终位置/尺寸由组驱动 | group | Image + LayoutElement；真实输入字段 |
| Tgl_StrongConfirmAcknowledged | min(0,1) max(0,1); pivot(0,1); sizeDelta(216,48); pos(16,-8) | absolute | Toggle；我已了解影响 |
| Img_StrongConfirmAcknowledgedBox | min(0,1) max(0,1); pivot(0,1); sizeDelta(32,32); pos(0,-8) | absolute | Image；开关背景 |
| Icon_StrongConfirmAcknowledgedCheck | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；Toggle.graphic |
| Txt_StrongConfirmAcknowledgedLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-48,0); pos(24,0) | absolute | TextMeshProUGUI；我已了解影响 |
| Panel_StrongConfirmConsequences | min(0,1) max(0,1); pivot(0,1); sizeDelta(272,534); pos(288,-36) | absolute | Image；损失说明 |
| Txt_StrongConfirmConsequencesHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,32); pos(12,-8) | absolute | TextMeshProUGUI；损失说明 |
| List_StrongConfirmConsequences | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_StrongConfirmConsequences | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_StrongConfirmConsequences | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_StrongConfirmConsequencesBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,120)初始化; pos(0,0)初始化; LayoutElement preferred(248,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；进度含正式存档及三份备份；模板删除范围；算法计时／计数／锁存等状态重置 |
| Item_StrongConfirmConsequencesTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,104)初始化; pos(0,0)初始化; LayoutElement preferred(248,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_StrongConfirmConsequencesRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_StrongConfirmConsequencesRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_StrongConfirmConsequencesRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_StrongConfirmAcknowledgement | min(0,1) max(0,1); pivot(0,1); sizeDelta(272,534); pos(576,-36) | absolute | Image；明确确认 |
| Txt_StrongConfirmAcknowledgementHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,32); pos(12,-8) | absolute | TextMeshProUGUI；明确确认 |
| List_StrongConfirmAcknowledgement | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_StrongConfirmAcknowledgement | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_StrongConfirmAcknowledgement | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_StrongConfirmAcknowledgementBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,120)初始化; pos(0,0)初始化; LayoutElement preferred(248,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；“我已了解影响”确认开关；未勾选时提交禁用 |
| Item_StrongConfirmAcknowledgementTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,104)初始化; pos(0,0)初始化; LayoutElement preferred(248,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_StrongConfirmAcknowledgementRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_StrongConfirmAcknowledgementRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_StrongConfirmAcknowledgementRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_StrongConfirmLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_StrongConfirmLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_StrongConfirmLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_StrongConfirmEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_StrongConfirmEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_StrongConfirmEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_StrongConfirmErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_StrongConfirmErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_StrongConfirmErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_StrongConfirmSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_StrongConfirmSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_StrongConfirmSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_StrongConfirmDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_StrongConfirmDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_StrongConfirmDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## DraftExit：算法草稿离开

功能文档：[算法草稿离开](DraftExit.md)；归属 `OperationDialogForm`；内容 848×630。

```text
Panel_PageDraftExit [Image]
  Txt_DraftExitTitle [TextMeshProUGUI]
  Grp_DraftExitActions [GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)]
    Btn_DraftExitStay [Button + Image]
      Txt_DraftExitStayLabel [TextMeshProUGUI]
    Btn_DraftExitSave [Button + Image]
      Txt_DraftExitSaveLabel [TextMeshProUGUI]
    Btn_DraftExitDiscard [Button + Image]
      Txt_DraftExitDiscardLabel [TextMeshProUGUI]
  Panel_DraftExitDraft [Image]
    Txt_DraftExitDraftHeading [TextMeshProUGUI]
    List_DraftExitDraft [ScrollRect vertical=true horizontal=false]
      Viewport_DraftExitDraft [RectMask2D]
        Content_DraftExitDraft [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_DraftExitDraftBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_DraftExitDraftTemplate [LayoutElement + Image；默认inactive]
            Btn_DraftExitDraftRow [Button + Image]
              Txt_DraftExitDraftRowLabel [TextMeshProUGUI]
              Txt_DraftExitDraftRowValue [TextMeshProUGUI]
  Grp_DraftExitLoadingState [无Graphic]
    Panel_DraftExitLoadingMessage [Image]
      Txt_DraftExitLoadingMessage [TextMeshProUGUI]
  Grp_DraftExitEmptyState [无Graphic]
    Panel_DraftExitEmptyMessage [Image]
      Txt_DraftExitEmptyMessage [TextMeshProUGUI]
  Grp_DraftExitErrorState [无Graphic]
    Panel_DraftExitErrorMessage [Image]
      Txt_DraftExitErrorMessage [TextMeshProUGUI]
  Grp_DraftExitSuccessState [无Graphic]
    Panel_DraftExitSuccessMessage [Image]
      Txt_DraftExitSuccessMessage [TextMeshProUGUI]
  Grp_DraftExitDisabledState [无Graphic]
    Panel_DraftExitDisabledMessage [Image]
      Txt_DraftExitDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageDraftExit | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；算法草稿离开；内部页面根 |
| Txt_DraftExitTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(832,32); pos(8,0) | absolute | TextMeshProUGUI；算法草稿离开 |
| Grp_DraftExitActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)；操作区 |
| Btn_DraftExitStay | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；初始焦点，关闭弹窗 |
| Txt_DraftExitStayLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；继续编辑 |
| Btn_DraftExitSave | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；保存草稿成功才返回来源；不修改运行版本 |
| Txt_DraftExitSaveLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；保存草稿并离开 |
| Btn_DraftExitDiscard | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；明确放弃本次草稿，运行版本保持 |
| Txt_DraftExitDiscardLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；放弃草稿并离开 |
| Panel_DraftExitDraft | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | Image；未应用修改 |
| Txt_DraftExitDraftHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(824,32); pos(12,-8) | absolute | TextMeshProUGUI；未应用修改 |
| List_DraftExitDraft | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_DraftExitDraft | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_DraftExitDraft | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_DraftExitDraftBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(824,120)初始化; pos(0,0)初始化; LayoutElement preferred(824,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；算法与机器；草稿状态；保存草稿不等于应用 |
| Item_DraftExitDraftTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(824,104)初始化; pos(0,0)初始化; LayoutElement preferred(824,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_DraftExitDraftRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_DraftExitDraftRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_DraftExitDraftRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_DraftExitLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DraftExitLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DraftExitLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_DraftExitEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DraftExitEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DraftExitEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_DraftExitErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DraftExitErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DraftExitErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_DraftExitSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DraftExitSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DraftExitSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_DraftExitDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DraftExitDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DraftExitDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## ForceExit：未保存强退确认

功能文档：[未保存强退确认](ForceExit.md)；归属 `OperationDialogForm`；内容 848×630。

```text
Panel_PageForceExit [Image]
  Txt_ForceExitTitle [TextMeshProUGUI]
  Grp_ForceExitActions [GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)]
    Btn_ForceExitCancel [Button + Image]
      Txt_ForceExitCancelLabel [TextMeshProUGUI]
    Btn_ForceExitForce [Button + Image]
      Txt_ForceExitForceLabel [TextMeshProUGUI]
  Panel_ForceExitLoss [Image]
    Txt_ForceExitLossHeading [TextMeshProUGUI]
    List_ForceExitLoss [ScrollRect vertical=true horizontal=false]
      Viewport_ForceExitLoss [RectMask2D]
        Content_ForceExitLoss [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ForceExitLossBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ForceExitLossTemplate [LayoutElement + Image；默认inactive]
            Btn_ForceExitLossRow [Button + Image]
              Txt_ForceExitLossRowLabel [TextMeshProUGUI]
              Txt_ForceExitLossRowValue [TextMeshProUGUI]
  Grp_ForceExitLoadingState [无Graphic]
    Panel_ForceExitLoadingMessage [Image]
      Txt_ForceExitLoadingMessage [TextMeshProUGUI]
  Grp_ForceExitEmptyState [无Graphic]
    Panel_ForceExitEmptyMessage [Image]
      Txt_ForceExitEmptyMessage [TextMeshProUGUI]
  Grp_ForceExitErrorState [无Graphic]
    Panel_ForceExitErrorMessage [Image]
      Txt_ForceExitErrorMessage [TextMeshProUGUI]
  Grp_ForceExitSuccessState [无Graphic]
    Panel_ForceExitSuccessMessage [Image]
      Txt_ForceExitSuccessMessage [TextMeshProUGUI]
  Grp_ForceExitDisabledState [无Graphic]
    Panel_ForceExitDisabledMessage [Image]
      Txt_ForceExitDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageForceExit | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；未保存强退确认；内部页面根 |
| Txt_ForceExitTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(832,32); pos(8,0) | absolute | TextMeshProUGUI；未保存强退确认 |
| Grp_ForceExitActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)；操作区 |
| Btn_ForceExitCancel | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；默认焦点 |
| Txt_ForceExitCancelLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；返回失败处理 |
| Btn_ForceExitForce | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；明确确认后按原定目标退出，不显示已保存成功 |
| Txt_ForceExitForceLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；放弃未保存进度并退出 |
| Panel_ForceExitLoss | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | Image；未保存风险 |
| Txt_ForceExitLossHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(824,32); pos(12,-8) | absolute | TextMeshProUGUI；未保存风险 |
| List_ForceExitLoss | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ForceExitLoss | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ForceExitLoss | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ForceExitLossBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(824,120)初始化; pos(0,0)初始化; LayoutElement preferred(824,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；最近成功保存时间；本次保存失败；可能丢失其后进度；退出目标 |
| Item_ForceExitLossTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(824,104)初始化; pos(0,0)初始化; LayoutElement preferred(824,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ForceExitLossRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ForceExitLossRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ForceExitLossRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_ForceExitLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ForceExitLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ForceExitLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_ForceExitEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ForceExitEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ForceExitEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_ForceExitErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ForceExitErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ForceExitErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_ForceExitSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ForceExitSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ForceExitSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_ForceExitDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ForceExitDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ForceExitDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## DisplayKeep：显示模式保留恢复

功能文档：[显示模式保留恢复](DisplayKeep.md)；归属 `OperationDialogForm`；内容 848×630。

```text
Panel_PageDisplayKeep [Image]
  Txt_DisplayKeepTitle [TextMeshProUGUI]
  Grp_DisplayKeepActions [GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)]
    Btn_DisplayKeepRestore [Button + Image]
      Txt_DisplayKeepRestoreLabel [TextMeshProUGUI]
    Btn_DisplayKeepKeep [Button + Image]
      Txt_DisplayKeepKeepLabel [TextMeshProUGUI]
  Panel_DisplayKeepModes [Image]
    Txt_DisplayKeepModesHeading [TextMeshProUGUI]
    List_DisplayKeepModes [ScrollRect vertical=true horizontal=false]
      Viewport_DisplayKeepModes [RectMask2D]
        Content_DisplayKeepModes [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_DisplayKeepModesBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_DisplayKeepModesTemplate [LayoutElement + Image；默认inactive]
            Btn_DisplayKeepModesRow [Button + Image]
              Txt_DisplayKeepModesRowLabel [TextMeshProUGUI]
              Txt_DisplayKeepModesRowValue [TextMeshProUGUI]
  Panel_DisplayKeepRecovery [Image]
    Txt_DisplayKeepRecoveryHeading [TextMeshProUGUI]
    List_DisplayKeepRecovery [ScrollRect vertical=true horizontal=false]
      Viewport_DisplayKeepRecovery [RectMask2D]
        Content_DisplayKeepRecovery [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_DisplayKeepRecoveryBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_DisplayKeepRecoveryTemplate [LayoutElement + Image；默认inactive]
            Btn_DisplayKeepRecoveryRow [Button + Image]
              Txt_DisplayKeepRecoveryRowLabel [TextMeshProUGUI]
              Txt_DisplayKeepRecoveryRowValue [TextMeshProUGUI]
  Grp_DisplayKeepLoadingState [无Graphic]
    Panel_DisplayKeepLoadingMessage [Image]
      Txt_DisplayKeepLoadingMessage [TextMeshProUGUI]
  Grp_DisplayKeepEmptyState [无Graphic]
    Panel_DisplayKeepEmptyMessage [Image]
      Txt_DisplayKeepEmptyMessage [TextMeshProUGUI]
  Grp_DisplayKeepErrorState [无Graphic]
    Panel_DisplayKeepErrorMessage [Image]
      Txt_DisplayKeepErrorMessage [TextMeshProUGUI]
  Grp_DisplayKeepSuccessState [无Graphic]
    Panel_DisplayKeepSuccessMessage [Image]
      Txt_DisplayKeepSuccessMessage [TextMeshProUGUI]
  Grp_DisplayKeepDisabledState [无Graphic]
    Panel_DisplayKeepDisabledMessage [Image]
      Txt_DisplayKeepDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageDisplayKeep | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；显示模式保留恢复；内部页面根 |
| Txt_DisplayKeepTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(832,32); pos(8,0) | absolute | TextMeshProUGUI；显示模式保留恢复 |
| Grp_DisplayKeepActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)；操作区 |
| Btn_DisplayKeepRestore | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；默认焦点；Esc同此行为 |
| Txt_DisplayKeepRestoreLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；恢复原模式 |
| Btn_DisplayKeepKeep | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；保存本机设置；失败说明内存已生效但持久化失败 |
| Txt_DisplayKeepKeepLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；保留新模式 |
| Panel_DisplayKeepModes | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(0,-36) | absolute | Image；显示变更 |
| Txt_DisplayKeepModesHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；显示变更 |
| List_DisplayKeepModes | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_DisplayKeepModes | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_DisplayKeepModes | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_DisplayKeepModesBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；原模式；新模式；剩余确认时间 |
| Item_DisplayKeepModesTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_DisplayKeepModesRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_DisplayKeepModesRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_DisplayKeepModesRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_DisplayKeepRecovery | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(432,-36) | absolute | Image；自动恢复说明 |
| Txt_DisplayKeepRecoveryHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；自动恢复说明 |
| List_DisplayKeepRecovery | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_DisplayKeepRecovery | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_DisplayKeepRecovery | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_DisplayKeepRecoveryBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；未确认将回到原模式；应用失败原因 |
| Item_DisplayKeepRecoveryTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_DisplayKeepRecoveryRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_DisplayKeepRecoveryRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_DisplayKeepRecoveryRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_DisplayKeepLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DisplayKeepLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DisplayKeepLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_DisplayKeepEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DisplayKeepEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DisplayKeepEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_DisplayKeepErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DisplayKeepErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DisplayKeepErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_DisplayKeepSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DisplayKeepSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DisplayKeepSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_DisplayKeepDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DisplayKeepDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DisplayKeepDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |
