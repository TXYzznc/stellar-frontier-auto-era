# Prefab 与 RectTransform 规范

## 结构原则

- 每个独立 GF 页面一个 UIForm Prefab；根节点直接持有该页视觉结构。
- 只在两个以上页面复用、具备稳定职责与生命周期的组件才拆子 Prefab，并记录复用者。
- 根节点不保留停用的旧结构、第二套视觉节点或运行时生成兜底；迁移完成后删除旧路径。
- 动态列表使用已存在且默认 inactive 的 `Item_*Template`，由所属 Form 管理对象池。

## 命名与层级

Prefab、脚本和公开类型用 PascalCase；节点采用 `前缀_语义`，同一 Form 内唯一且不含本地化文字、
路径分隔符或无意义序号。**本表是唯一的命名规范**——不存在第二套前缀，任何不在表内的前缀
（如历史遗留的 `Art_`）都视为违规，必须改名并入本表。

### 节点前缀词表

| 前缀 | 职责 | 必备组件 | 硬性禁止 |
|---|---|---|---|
| `Bg_` | 背景 / 遮罩 | Image | 不得含交互控件（除非本身是拦截遮罩） |
| `Overlay_` | 模态 / 覆盖层根 | Image 或视图脚本 | — |
| `Panel_` | 有视觉的容器 | Image | — |
| `Grp_` | **无视觉**分组（纯层级） | 无 Graphic | **不得有 Image/TMP** |
| `Txt_` | 文本 | `TextMeshProUGUI` | **不得挂 Button** |
| `Img_` | 图片 / 装饰 | Image | 装饰件 `raycastTarget` 必须 false |
| `Icon_` | 小图标 | Image | `raycastTarget` 必须 false |
| `Btn_` | 按钮 | **Button + targetGraphic(Image)** | 名字不得挂在纯文本节点上 |
| `Tgl_` | 开关 | Toggle | — |
| `Sld_` | 滑条 | Slider | — |
| `List_` | 列表 / 滚动区根 | ScrollRect | 必须配 `Viewport_` + `Content_` |
| `Viewport_` | 滚动视口 | RectMask2D | 只能作为 `List_` 子节点 |
| `Content_` | 滚动内容 | LayoutGroup(+ContentSizeFitter) | 只能作为 `Viewport_` 子节点 |
| `Item_` | 列表项模板 | 与行内容一致 | 默认必须 inactive |
| `Bar_` | 进度 / 数值条 | Image(Filled) | — |
| `Deco_` | **纯装饰**（分隔线、间距、色块、光效） | Image 或空 RectTransform | `raycastTarget` 必须 false；**不得承载任何文本或交互** |
| `Grp_*State` | 区域状态分组 | 无 Graphic 或 `Panel_` | 同区域同时只激活一个 |

`Deco_` 的用途是把"纯装饰"与"真的图片资源"分开：分隔线、占位间距、纯色块和光效都属于
装饰，不应借用 `Img_`，否则"装饰性 Img_"与"要绑定 Sprite 的 Img_"无法用规则区分。

### 命名硬性规则

- 节点名 = `前缀_语义`，语义段 PascalCase，**同一 Form 内唯一**。
- 根节点是唯一例外：名为 Form 类名（无前缀），与预制体同名。
- 禁止本地化文字、路径分隔符、无意义序号（`_01`、`_02`）。
- **前缀决定职责与必备组件；前缀与组件不匹配即违规**——这是可机器判定的部分，也是自检的依据。
- `Item_<语义>Template`，同一列表唯一且默认 inactive；禁止具名预览行（如 `Txt_TaskRow_01`）。
- 废弃 `Btn_*_Background` 两段式写法：Button 与 Image 同节点（`Btn_<语义>`），标签作为其子节点
  `Txt_<语义>`。
- 状态节点的两种表达：控件级状态用 `Selectable` transition，**不建节点**；区域级状态用
  `Grp_<区域><状态>State`（Empty / Loading / Error / Success / Disabled），同一区域同时只激活一个。
  不为此把每个控件状态都建成独立节点。

## Canvas 与根节点

- UIForm 根使用全屏 Stretch：anchorMin `(0,0)`、anchorMax `(1,1)`、offsets 为 `0`。
- **根节点不带 Canvas、GraphicRaycaster 或 CanvasScaler**。UIForm 由 GF 动态实例化到场景中
  预先配置好的根 Canvas 下，缩放策略由根 Canvas 统一承担。
  - 这是本项目对 GF 自带 `UIFormTemplate.prefab`（其根带 Canvas + GraphicRaycaster + CanvasGroup）
    的**刻意偏离**，理由：页面根若各自带 Canvas，等于每页私设一套渲染与缩放策略。
  - 参考分辨率来自 `AppSettings.DesignResolution`（当前 1920×1080），由 `GFBuiltin.UpdateCanvasScaler()`
    下发给根 Canvas。**不得在 Form 脚本里给无 Canvas 的根节点添加 CanvasScaler**——在自身
    GameObject 上没有 Canvas 时它不生效。
- 根只放 UIForm 生命周期组件、项目 Form 脚本与必要的 CanvasGroup。
- 页面层级、Sorting Order、UI Layer 和打开组由 GF 配置统一管理，不在子节点硬编码对抗。

## 原型阶段与效果图阶段

本项目界面分两个阶段交付，两阶段的验收门完全不同，不得互相取代：

| 阶段 | 产出 | 责任方 | 判定内容 |
|---|---|---|---|
| 原型 | 结构 / 功能 / 布局正确的 Prefab | AI | 结构、命名、绑定、交互对象与布局四层自检；**不含美术** |
| 效果图 | 以原型为基准绘制的视觉稿 | 用户 | 外观是否达标，由用户手动判断 |
| 微调 | 把原型调整为效果图外观 | 用户手动 | 只改颜色 / Sprite / 字号 / 位置微调 |

流程方向是**先从原型出效果图**，不是先有视觉稿再拼 Prefab。原型阶段使用结构期中性 token
配色、命名明确的占位 Image 和零假数据；美术阶段才替换为正式资源。

因此：**效果图不是原型阶段的输入，也不构成 AI 任务的完成门**。原型交付的判定标准是结构与
功能正确，不是像不像效果图。

## 锚点、轴心与尺寸

- 全屏背景/遮罩：Stretch/Stretch；模态框：Center/Center；HUD：按语义锚定对应边缘或中心。
- 元素相对于哪条屏幕边稳定，就锚定哪条边；禁止用 1920 坐标模拟另一侧对齐。
- 左上定位必须同时使用左上 anchor 与左上 pivot；锚点改变后重新核对 `anchoredPosition`。
- 内部连续行、按钮列和列表优先 Layout Group；不要同时以 Layout Group 和手写绝对坐标争夺同一子节点。
- 每个节点在 layout 中记录 anchor、pivot、sizeDelta、anchoredPosition、组件、颜色、资源、文本样例与状态。

## UGUI 组件

- 正式文本只用 `TextMeshProUGUI`，禁止 Legacy Text。
- 可视面板使用 Image；装饰 Graphic 的 raycastTarget 为 false，遮罩和可交互 Graphic 为 true。
- Button 的 targetGraphic 指向自身 Image；优先 Sprite Swap，缺资源时才使用统一 Tint。
- ScrollRect 必须具备 Viewport 和 Content；Content 的锚点、布局和滚动方向一致。
- 每个可交互控件有清晰 target、导航和 disabled 状态；关闭按钮是模态框最后一个可选中节点。

## Prefab 制作步骤

1. 根据确认的 `prefab-layout.md` 创建完整节点树和 RectTransform。
2. 设置组件、锚点、轴心、导航与射线规则。
3. 绑定已验收资源；缺失项保留命名明确的占位 Image。
4. 在 Form 脚本声明 SerializeField 并显式绑定。
5. 逐节点复核 layout，打开页面检查多宽高比、焦点和遮挡。
