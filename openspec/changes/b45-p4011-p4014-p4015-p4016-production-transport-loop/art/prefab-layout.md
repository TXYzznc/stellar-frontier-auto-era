# B45 仓库与现场列表布局增量

依据：已批准界面结构与本批实际1920×1080截图。WarehouseForm使用完整contract.json；FieldHudDetailForm保持最小入口契约，只从原生Prefab填充_pageRoots数组。本增量与原始界面规格记录两个页面的实际子节点布局。

适用节点：WarehouseForm内六个列表；FieldHudDetailForm仅Panel_PageForest与Panel_PageMineral下Public、Sensor、Record三列。其余现场页不在本批实现范围。

| 节点 | 尺寸责任与适配 |
|---|---|
| List / Viewport | 保留原有父容器拉伸及RectMask2D；统一Canvas缩放；不新增独立缩放 |
| Content | 顶部横向拉伸，VerticalLayoutGroup间隔4；控制子项宽高，扩展宽度，不扩展高度；ContentSizeFitter仅控制内容高度 |
| Txt_*Body | LayoutElement控制高度：现场24、仓库36；flexibleHeight=0；字号16 |
| Item_*Template | 顶部横向拉伸，pivot(0.5,1)；LayoutElement minHeight=preferredHeight=44、preferredWidth=-1、flexibleWidth=1、flexibleHeight=0；模板保持inactive，由GF列表池复制 |
| Btn_*Row | 充满单行容器，offsetMin/Max=0 |
| Txt_*RowLabel | 横向0～0.42，纵向锚点0.5；pivot(0,0.5)，sizeDelta(-24,32)，pos(12,0)；字号16，左对齐 |
| Txt_*RowValue | 横向0.42～1，其余同行标签；支持换行，超出区域省略；长记录可选择查看详情 |

宽度由父容器与anchor确定，行高由LayoutElement确定；不靠1920固定宽度实现标签和值分栏。只运行1920×1080视觉验收，结构检查单独核对anchor/pivot/offset与布局组责任。

原生迁移入口：Game Framework/AutoEra/UI/接入生产运输列表布局。禁止手改Unity YAML。重新执行必须保持资产字节不变。

实际页面读取生产缓存、公开传感量、实际生产/装卸回执及仓库余额；无领域能力的按钮显示不可用原因，不写入库存。
