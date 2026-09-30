## Context

b32 的命令层（CreateNode/Connect/Disconnect）已在服务与读模型就绪并测试。AlgorithmEditorForm 预制体存在两类缺口：存量字段未绑定（应用按钮/搜索/画布内容/画布模板，b26/b29/b30 引入）、规格节点未落地（Grp_AlgorithmDraftTools）。规格（13-算法工作台 prefab-layout.md）明确：节点栏＝节点库；画布元素模板双模式（节点：端口列表左右分列；连线：Img_AlgorithmEdge+透明命中代理）；端口按钮职责「选择端口建立／断开连接」；「删除选中」删除选中节点／连线且仅修改草稿。

## Decisions

### 预制体修复走 UnitySkills 实例化流

不手编 prefab YAML。流程：prefab_instantiate 临时实例 → component_set_serialized_property 补六个字段引用（含新建工具条按钮）→ gameobject_create 建工具条（样式抄既有按钮：Image 颜色 (0.32,0.50,0.64) imageType=Sliced + Button + TMP 标签，撤销/重做 interactable=false）→ prefab_apply → 删临时实例 → asset_refresh → 文本核验。契约 bindings 同步追加两个字段后重跑生成器，只保留 AlgorithmEditorForm.Fields.cs（其余 32 个文件的 EOL 噪声还原）。

### 节点栏回归规格语义：节点库

节点栏从「图节点列表」切回「节点种类目录」：按规格分类（输入/判断与运算/状态/流程/行为/数值）分组，行显示种类名+逻辑成本；搜索框过滤目录。图节点的选择与检视经画布节点按钮（Btn_AlgorithmNodeSelect → SelectNode）完成，不再依赖节点栏。节点库数据为静态目录（AlgorithmNodeKind 枚举派生），三个域（机器/模板/不可用）都提供——它是能力目录而非实例数据。

### 两步端口连线（规格允许的模式）

点输出端口 → 记录待连源并在端口文字上标注「待连」；点输入端口 → Connect；成功清除状态，失败在问题栏首行呈现原因（端口缺失/类型/单位/能力不兼容/输入已占用——服务已区分，读模型透传布尔结果，界面按失败时保留源状态并提示重选）。再次点击源端口或切换源＝取消/换源。不做拖线。

### 边元素中点渲染

每条边一个画布元素（Grp_AlgorithmEdge 模式）：位置取两端节点布局坐标中点，Btn_AlgorithmEdgeSelect 点击选中（记录 SelectedEdge）；「删除选中」→ Disconnect 该边。节点选中（Btn_AlgorithmNodeSelect）→「删除选中」→ DeleteNode。Img_AlgorithmEdge 的端点形状不做（非目标）。

### DeleteNode 级联断开

删除节点时同步移除草稿中所有 From 或 To 指向该节点的边（图编辑器标准语义；否则留下悬空边只能靠校验问题呈现）。文档层新增 `RemoveNode(id)`：软删节点+移除关联边，一次修订自增。服务层 `DeleteNode(id, expectedRevision, nodeId)` 修订保护。与既有 `DeleteNode`（仅软删）并存：旧方法保留给既有调用方（检查器/校验流）。

### 新增节点的画布落位

CreateNode 的布局坐标由界面决定：以画布原点为基准、按当前节点数网格步进（列距 300、行距 220，8 列换行）偏移，避免与现有节点重叠；不做视口中心计算（画布 4000×4000，原点即初始可视中心）。

## Risks / Trade-offs

- 节点栏语义切换会让 b20 的「节点栏点击选中图节点」行为消失（被画布节点按钮替代）；相关既有测试若断言该行为需同步更新——以测试实际断言为准。
- 工具条撤销/重做是禁用占位：不伪装可用，按钮存在但 interactable=false。
- 端口行进快照增加每节点数据量：仅选中实例的草稿（数十节点级），可接受。
