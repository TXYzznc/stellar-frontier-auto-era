# 10-商店与建造目录 — prefab-layout

状态：2026-09-19完整设计候选，待用户集中验收；未据此修改Prefab。

继承[通用合同](../00-通用合同.md)和[共享外壳](../00-共享外壳-prefab-layout.md)。下列子树预制在所属Form的Grp_PageHost中；公共外壳由共享文档唯一声明。跨家族同属一个Form的子树合并，禁止重复根节点。表中所有Image／文本颜色、资源、射线、默认字号与状态继承通用合同，列出的数据是字段说明，不是示例业务数值。

## ShopBuy：商店购买

功能文档：[商店购买](ShopBuy.md)；归属 `ShopForm`；内容 1488×730。

```text
Panel_PageShopBuy [Image]
  Txt_ShopBuyTitle [TextMeshProUGUI]
  Grp_ShopBuyActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_ShopBuyDetails [Button + Image]
      Txt_ShopBuyDetailsLabel [TextMeshProUGUI]
    Btn_ShopBuyBuy [Button + Image]
      Txt_ShopBuyBuyLabel [TextMeshProUGUI]
    Btn_ShopBuySellTab [Button + Image]
      Txt_ShopBuySellTabLabel [TextMeshProUGUI]
  Panel_ShopBuyGoods [Image]
    Txt_ShopBuyGoodsHeading [TextMeshProUGUI]
    List_ShopBuyGoods [ScrollRect vertical=true horizontal=false]
      Viewport_ShopBuyGoods [RectMask2D]
        Content_ShopBuyGoods [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ShopBuyGoodsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ShopBuyGoodsTemplate [LayoutElement + Image；默认inactive]
            Btn_ShopBuyGoodsRow [Button + Image]
              Txt_ShopBuyGoodsRowLabel [TextMeshProUGUI]
              Txt_ShopBuyGoodsRowValue [TextMeshProUGUI]
  Panel_ShopBuyOffer [Image]
    Txt_ShopBuyOfferHeading [TextMeshProUGUI]
    List_ShopBuyOffer [ScrollRect vertical=true horizontal=false]
      Viewport_ShopBuyOffer [RectMask2D]
        Content_ShopBuyOffer [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ShopBuyOfferBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ShopBuyOfferTemplate [LayoutElement + Image；默认inactive]
            Btn_ShopBuyOfferRow [Button + Image]
              Txt_ShopBuyOfferRowLabel [TextMeshProUGUI]
              Txt_ShopBuyOfferRowValue [TextMeshProUGUI]
  Grp_ShopBuyLoadingState [无Graphic]
    Panel_ShopBuyLoadingMessage [Image]
      Txt_ShopBuyLoadingMessage [TextMeshProUGUI]
  Grp_ShopBuyEmptyState [无Graphic]
    Panel_ShopBuyEmptyMessage [Image]
      Txt_ShopBuyEmptyMessage [TextMeshProUGUI]
  Grp_ShopBuyErrorState [无Graphic]
    Panel_ShopBuyErrorMessage [Image]
      Txt_ShopBuyErrorMessage [TextMeshProUGUI]
  Grp_ShopBuySuccessState [无Graphic]
    Panel_ShopBuySuccessMessage [Image]
      Txt_ShopBuySuccessMessage [TextMeshProUGUI]
  Grp_ShopBuyDisabledState [无Graphic]
    Panel_ShopBuyDisabledMessage [Image]
      Txt_ShopBuyDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageShopBuy | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；商店购买；内部页面根 |
| Txt_ShopBuyTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；商店购买 |
| Grp_ShopBuyActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_ShopBuyDetails | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；10-商品详情 |
| Txt_ShopBuyDetailsLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；商品或礼包详情 |
| Btn_ShopBuyBuy | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；17交易确认，重验余额、库存容量、限购与解锁后原子成交 |
| Txt_ShopBuyBuyLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；购买 |
| Btn_ShopBuySellTab | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；10-商店出售 |
| Txt_ShopBuySellTabLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；出售分页 |
| Panel_ShopBuyGoods | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；商品分类 |
| Txt_ShopBuyGoodsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；商品分类 |
| List_ShopBuyGoods | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ShopBuyGoods | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ShopBuyGoods | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ShopBuyGoodsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；礼包、载体、传感器、效应器、计算核心、种子与消耗品；价格与解锁 |
| Item_ShopBuyGoodsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ShopBuyGoodsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ShopBuyGoodsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ShopBuyGoodsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_ShopBuyOffer | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；选中商品 |
| Txt_ShopBuyOfferHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；选中商品 |
| List_ShopBuyOffer | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ShopBuyOffer | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ShopBuyOffer | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ShopBuyOfferBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；完整内容；数量；单价／总价；限购；余额；分类交付去向 |
| Item_ShopBuyOfferTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ShopBuyOfferRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ShopBuyOfferRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ShopBuyOfferRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_ShopBuyLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ShopBuyLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ShopBuyLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_ShopBuyEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ShopBuyEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ShopBuyEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_ShopBuyErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ShopBuyErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ShopBuyErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_ShopBuySuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ShopBuySuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ShopBuySuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_ShopBuyDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ShopBuyDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ShopBuyDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## ShopSell：商店出售

功能文档：[商店出售](ShopSell.md)；归属 `ShopForm`；内容 1488×730。

```text
Panel_PageShopSell [Image]
  Txt_ShopSellTitle [TextMeshProUGUI]
  Grp_ShopSellActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_ShopSellQuantity [Button + Image]
      Txt_ShopSellQuantityLabel [TextMeshProUGUI]
    Btn_ShopSellSell [Button + Image]
      Txt_ShopSellSellLabel [TextMeshProUGUI]
    Btn_ShopSellBuyTab [Button + Image]
      Txt_ShopSellBuyTabLabel [TextMeshProUGUI]
  Panel_ShopSellStock [Image]
    Txt_ShopSellStockHeading [TextMeshProUGUI]
    List_ShopSellStock [ScrollRect vertical=true horizontal=false]
      Viewport_ShopSellStock [RectMask2D]
        Content_ShopSellStock [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ShopSellStockBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ShopSellStockTemplate [LayoutElement + Image；默认inactive]
            Btn_ShopSellStockRow [Button + Image]
              Txt_ShopSellStockRowLabel [TextMeshProUGUI]
              Txt_ShopSellStockRowValue [TextMeshProUGUI]
          Panel_ShopSellControls [Image + LayoutElement]
            Panel_ShopSellQuantity [Image + TMP_InputField]
              Grp_ShopSellQuantityTextViewport [RectMask2D]
                Txt_ShopSellQuantityValue [TextMeshProUGUI]
                Txt_ShopSellQuantityPlaceholder [TextMeshProUGUI]
  Panel_ShopSellQuote [Image]
    Txt_ShopSellQuoteHeading [TextMeshProUGUI]
    List_ShopSellQuote [ScrollRect vertical=true horizontal=false]
      Viewport_ShopSellQuote [RectMask2D]
        Content_ShopSellQuote [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ShopSellQuoteBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ShopSellQuoteTemplate [LayoutElement + Image；默认inactive]
            Btn_ShopSellQuoteRow [Button + Image]
              Txt_ShopSellQuoteRowLabel [TextMeshProUGUI]
              Txt_ShopSellQuoteRowValue [TextMeshProUGUI]
  Grp_ShopSellLoadingState [无Graphic]
    Panel_ShopSellLoadingMessage [Image]
      Txt_ShopSellLoadingMessage [TextMeshProUGUI]
  Grp_ShopSellEmptyState [无Graphic]
    Panel_ShopSellEmptyMessage [Image]
      Txt_ShopSellEmptyMessage [TextMeshProUGUI]
  Grp_ShopSellErrorState [无Graphic]
    Panel_ShopSellErrorMessage [Image]
      Txt_ShopSellErrorMessage [TextMeshProUGUI]
  Grp_ShopSellSuccessState [无Graphic]
    Panel_ShopSellSuccessMessage [Image]
      Txt_ShopSellSuccessMessage [TextMeshProUGUI]
  Grp_ShopSellDisabledState [无Graphic]
    Panel_ShopSellDisabledMessage [Image]
      Txt_ShopSellDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageShopSell | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；商店出售；内部页面根 |
| Txt_ShopSellTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；商店出售 |
| Grp_ShopSellActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_ShopSellQuantity | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；加减按钮与数值输入；限制为实际可售整数范围 |
| Txt_ShopSellQuantityLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；调整数量 |
| Btn_ShopSellSell | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；17交易确认后原子扣物加金币 |
| Txt_ShopSellSellLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；确认出售 |
| Btn_ShopSellBuyTab | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；10-商店购买 |
| Txt_ShopSellBuyTabLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；购买分页 |
| Panel_ShopSellStock | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；可售资产 |
| Txt_ShopSellStockHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；可售资产 |
| List_ShopSellStock | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ShopSellStock | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ShopSellStock | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ShopSellStockBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；木材、矿石、水、生物质；主基地基础仓库实体物品 |
| Item_ShopSellStockTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ShopSellStockRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ShopSellStockRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ShopSellStockRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_ShopSellControls | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,88)初始化; pos(0,0)初始化; LayoutElement preferred(541,88); 最终位置/尺寸由组驱动 | group | Image + LayoutElement；真实输入字段 |
| Panel_ShopSellQuantity | min(0,1) max(0,1); pivot(0,1); sizeDelta(509,48); pos(16,-8) | absolute | Image + TMP_InputField；出售数量；onEndEdit验证 |
| Grp_ShopSellQuantityTextViewport | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-8); pos(0,0) | absolute | RectMask2D；textViewport，无Graphic |
| Txt_ShopSellQuantityValue | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；— |
| Txt_ShopSellQuantityPlaceholder | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；出售数量 |
| Panel_ShopSellQuote | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；出售报价 |
| Txt_ShopSellQuoteHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；出售报价 |
| List_ShopSellQuote | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ShopSellQuote | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ShopSellQuote | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ShopSellQuoteBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；当前可售量；选择数量；实际单价；总收入 |
| Item_ShopSellQuoteTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ShopSellQuoteRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ShopSellQuoteRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ShopSellQuoteRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_ShopSellLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ShopSellLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ShopSellLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_ShopSellEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ShopSellEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ShopSellEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_ShopSellErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ShopSellErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ShopSellErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_ShopSellSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ShopSellSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ShopSellSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_ShopSellDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ShopSellDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ShopSellDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## OfferDetail：商品与礼包详情

功能文档：[商品与礼包详情](OfferDetail.md)；归属 `ShopForm`；内容 1488×730。

```text
Panel_PageOfferDetail [Image]
  Txt_OfferDetailTitle [TextMeshProUGUI]
  Grp_OfferDetailActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_OfferDetailBuy [Button + Image]
      Txt_OfferDetailBuyLabel [TextMeshProUGUI]
    Btn_OfferDetailBack [Button + Image]
      Txt_OfferDetailBackLabel [TextMeshProUGUI]
  Panel_OfferDetailContents [Image]
    Txt_OfferDetailContentsHeading [TextMeshProUGUI]
    List_OfferDetailContents [ScrollRect vertical=true horizontal=false]
      Viewport_OfferDetailContents [RectMask2D]
        Content_OfferDetailContents [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_OfferDetailContentsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_OfferDetailContentsTemplate [LayoutElement + Image；默认inactive]
            Btn_OfferDetailContentsRow [Button + Image]
              Txt_OfferDetailContentsRowLabel [TextMeshProUGUI]
              Txt_OfferDetailContentsRowValue [TextMeshProUGUI]
  Panel_OfferDetailTerms [Image]
    Txt_OfferDetailTermsHeading [TextMeshProUGUI]
    List_OfferDetailTerms [ScrollRect vertical=true horizontal=false]
      Viewport_OfferDetailTerms [RectMask2D]
        Content_OfferDetailTerms [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_OfferDetailTermsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_OfferDetailTermsTemplate [LayoutElement + Image；默认inactive]
            Btn_OfferDetailTermsRow [Button + Image]
              Txt_OfferDetailTermsRowLabel [TextMeshProUGUI]
              Txt_OfferDetailTermsRowValue [TextMeshProUGUI]
  Grp_OfferDetailLoadingState [无Graphic]
    Panel_OfferDetailLoadingMessage [Image]
      Txt_OfferDetailLoadingMessage [TextMeshProUGUI]
  Grp_OfferDetailEmptyState [无Graphic]
    Panel_OfferDetailEmptyMessage [Image]
      Txt_OfferDetailEmptyMessage [TextMeshProUGUI]
  Grp_OfferDetailErrorState [无Graphic]
    Panel_OfferDetailErrorMessage [Image]
      Txt_OfferDetailErrorMessage [TextMeshProUGUI]
  Grp_OfferDetailSuccessState [无Graphic]
    Panel_OfferDetailSuccessMessage [Image]
      Txt_OfferDetailSuccessMessage [TextMeshProUGUI]
  Grp_OfferDetailDisabledState [无Graphic]
    Panel_OfferDetailDisabledMessage [Image]
      Txt_OfferDetailDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageOfferDetail | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；商品与礼包详情；内部页面根 |
| Txt_OfferDetailTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；商品与礼包详情 |
| Grp_OfferDetailActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_OfferDetailBuy | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；17交易确认 |
| Txt_OfferDetailBuyLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；购买 |
| Btn_OfferDetailBack | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；恢复类别和滚动 |
| Txt_OfferDetailBackLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；返回商品列表 |
| Panel_OfferDetailContents | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；实际内容 |
| Txt_OfferDetailContentsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；实际内容 |
| List_OfferDetailContents | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_OfferDetailContents | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_OfferDetailContents | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_OfferDetailContentsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；单品属性或礼包全量清单；每项数量和去向 |
| Item_OfferDetailContentsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_OfferDetailContentsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_OfferDetailContentsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_OfferDetailContentsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_OfferDetailTerms | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；购买条件 |
| Txt_OfferDetailTermsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；购买条件 |
| List_OfferDetailTerms | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_OfferDetailTerms | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_OfferDetailTerms | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_OfferDetailTermsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；价格；数量；限购余量；解锁；交付容量限制 |
| Item_OfferDetailTermsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_OfferDetailTermsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_OfferDetailTermsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_OfferDetailTermsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_OfferDetailLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_OfferDetailLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_OfferDetailLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_OfferDetailEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_OfferDetailEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_OfferDetailEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_OfferDetailErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_OfferDetailErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_OfferDetailErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_OfferDetailSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_OfferDetailSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_OfferDetailSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_OfferDetailDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_OfferDetailDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_OfferDetailDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## Blueprints：建筑图纸目录与详情

功能文档：[建筑图纸目录与详情](Blueprints.md)；归属 `BuildCatalogForm`；内容 1488×730。

```text
Panel_PageBlueprints [Image]
  Txt_BlueprintsTitle [TextMeshProUGUI]
  Grp_BlueprintsActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_BlueprintsPlace [Button + Image]
      Txt_BlueprintsPlaceLabel [TextMeshProUGUI]
    Btn_BlueprintsRule [Button + Image]
      Txt_BlueprintsRuleLabel [TextMeshProUGUI]
    Btn_BlueprintsSelect [Button + Image]
      Txt_BlueprintsSelectLabel [TextMeshProUGUI]
  Panel_BlueprintsCatalog [Image]
    Txt_BlueprintsCatalogHeading [TextMeshProUGUI]
    List_BlueprintsCatalog [ScrollRect vertical=true horizontal=false]
      Viewport_BlueprintsCatalog [RectMask2D]
        Content_BlueprintsCatalog [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_BlueprintsCatalogBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_BlueprintsCatalogTemplate [LayoutElement + Image；默认inactive]
            Btn_BlueprintsCatalogRow [Button + Image]
              Txt_BlueprintsCatalogRowLabel [TextMeshProUGUI]
              Txt_BlueprintsCatalogRowValue [TextMeshProUGUI]
  Panel_BlueprintsBlueprint [Image]
    Txt_BlueprintsBlueprintHeading [TextMeshProUGUI]
    List_BlueprintsBlueprint [ScrollRect vertical=true horizontal=false]
      Viewport_BlueprintsBlueprint [RectMask2D]
        Content_BlueprintsBlueprint [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_BlueprintsBlueprintBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_BlueprintsBlueprintTemplate [LayoutElement + Image；默认inactive]
            Btn_BlueprintsBlueprintRow [Button + Image]
              Txt_BlueprintsBlueprintRowLabel [TextMeshProUGUI]
              Txt_BlueprintsBlueprintRowValue [TextMeshProUGUI]
  Grp_BlueprintsLoadingState [无Graphic]
    Panel_BlueprintsLoadingMessage [Image]
      Txt_BlueprintsLoadingMessage [TextMeshProUGUI]
  Grp_BlueprintsEmptyState [无Graphic]
    Panel_BlueprintsEmptyMessage [Image]
      Txt_BlueprintsEmptyMessage [TextMeshProUGUI]
  Grp_BlueprintsErrorState [无Graphic]
    Panel_BlueprintsErrorMessage [Image]
      Txt_BlueprintsErrorMessage [TextMeshProUGUI]
  Grp_BlueprintsSuccessState [无Graphic]
    Panel_BlueprintsSuccessMessage [Image]
      Txt_BlueprintsSuccessMessage [TextMeshProUGUI]
  Grp_BlueprintsDisabledState [无Graphic]
    Panel_BlueprintsDisabledMessage [Image]
      Txt_BlueprintsDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageBlueprints | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；建筑图纸目录与详情；内部页面根 |
| Txt_BlueprintsTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；建筑图纸目录与详情 |
| Grp_BlueprintsActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_BlueprintsPlace | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；14-建筑放置，浏览图纸不扣费 |
| Txt_BlueprintsPlaceLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；进入放置模式 |
| Btn_BlueprintsRule | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；16通用规则 |
| Txt_BlueprintsRuleLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；建造规则 |
| Btn_BlueprintsSelect | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；更新详情与条件，不启动建造 |
| Txt_BlueprintsSelectLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；切换图纸 |
| Panel_BlueprintsCatalog | min(0,1) max(0,1); pivot(0,1); sizeDelta(565,634); pos(0,-36) | absolute | Image；建筑图纸 |
| Txt_BlueprintsCatalogHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,32); pos(12,-8) | absolute | TextMeshProUGUI；建筑图纸 |
| List_BlueprintsCatalog | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_BlueprintsCatalog | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_BlueprintsCatalog | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_BlueprintsCatalogBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,120)初始化; pos(0,0)初始化; LayoutElement preferred(541,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；类型；名称；解锁条件；已持有图纸；可建数量限制 |
| Item_BlueprintsCatalogTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(541,104)初始化; pos(0,0)初始化; LayoutElement preferred(541,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_BlueprintsCatalogRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_BlueprintsCatalogRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_BlueprintsCatalogRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_BlueprintsBlueprint | min(0,1) max(0,1); pivot(0,1); sizeDelta(907,634); pos(581,-36) | absolute | Image；选中图纸 |
| Txt_BlueprintsBlueprintHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,32); pos(12,-8) | absolute | TextMeshProUGUI；选中图纸 |
| List_BlueprintsBlueprint | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_BlueprintsBlueprint | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_BlueprintsBlueprint | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_BlueprintsBlueprintBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,120)初始化; pos(0,0)初始化; LayoutElement preferred(883,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；功能；全部成本；资源满足状态；施工时间；占地与放置条件；规则说明 |
| Item_BlueprintsBlueprintTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(883,104)初始化; pos(0,0)初始化; LayoutElement preferred(883,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_BlueprintsBlueprintRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_BlueprintsBlueprintRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_BlueprintsBlueprintRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_BlueprintsLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BlueprintsLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BlueprintsLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_BlueprintsEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BlueprintsEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BlueprintsEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_BlueprintsErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BlueprintsErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BlueprintsErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_BlueprintsSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BlueprintsSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BlueprintsSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_BlueprintsDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_BlueprintsDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_BlueprintsDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |
