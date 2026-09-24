# 现场定位：F 键、双击与界面「聚焦」按钮汇入同一条意图

## Why

设计里「定位／聚焦」出现在十几个页面上（机器概况、四个资源点、建筑总览、已部署机器、
中枢对象与任务、警报、历史记录、回归报告……）。实现里只有一半：

- **快捷键与鼠标已经有了**：`RegionInputModule` 里 F 键与双击都调 `_camera.Focus(...)`；
- **界面按钮一个都没有**：`Btn_MachineOverviewFocus`、`Btn_FarmFocus`、`Btn_ForestFocus`、
  `Btn_MineralFocus`、`Btn_WaterFocus`、`Btn_BuildingOverviewFocus` 这些节点存在、页面也有真实数据，
  但契约里没有绑定，于是它们点下去什么也不发生。

而规格对这件事的要求不仅是「能聚焦」，还要求三者**是同一件事**：

- `14-世界放置与定位/WorldBinding.md`：`Btn_WorldBindingFocus | 聚焦 | 有效对象双击或F聚焦`；
- `00-通用合同`：「输入、Button和快捷键汇入同一意图」。

如果界面自己实现一份（比如取包围盒中心），就会出现「双击聚焦到锚点、按钮聚焦到别处」这种
看得见的偏差——而两处代码都「没错」，只是各算各的。所以本变更的做法是：先把
`RegionInputModule.FocusSelection()` 抽成**唯一实现**，再让按钮调它。

## What Changes

### 输入边界（唯一实现）

- `RegionInputModule` 新增：
  - `FocusSelection()` —— 把镜头带到当前选中的现场对象；没有可定位对象时返回 false。
    F 键与双击改为调用它（行为不变，但现在只有一份实现）。
  - `CanFocusSelection` —— 界面据此决定「聚焦」按钮能不能点。
  - `SelectedObjectId` —— 当前选中的对象身份（界面禁用时说明原因用）。

### 界面

- `FieldHudForm`：接上六个「聚焦当前对象」的按钮——现场机器概况、四个资源观察页（农田／林地／
  矿脉／水源）与建筑总览。它们全部调 `_regionInput.FocusSelection()`。
  - 可点性来自现场输入模块（它才知道谁被选中），不是「页面上有没有选中行」——
    现场页与选中状态是两条同步路径，只有输入模块那一份是权威的。
  - 与世界秒同一个节拍刷新（`ShowWorldTime`）：现场点选不产生领域事件，
    而世界秒是这一页已有的稳定节拍；漏掉它就会出现「明明选着对象，聚焦按钮却是灰的」。

### 契约

- `Tools/ui_spec_to_contract.py` 的 `FieldHudForm` 条目加六个绑定。
  **只声明「聚焦当前对象」的按钮**：`Btn_PumpFocus`（查看水源）、`Btn_ConveyorLocate`、
  `Btn_MachineDiagnosticsLocate` 定位的是**另一个**对象，需要跨对象定位通道，不在本变更内。
- 世界绑定页的 `Btn_WorldBindingFocus` 虽然语义相同，但**该页整域未接线**
  （页级状态是 Disabled）。在一个自称不可用的页面上点亮一个可用按钮比一个死按钮更让人困惑，
  所以它随世界绑定域一起接，本变更不声明。

## Capabilities

### New Capabilities

- `field-locate-wiring`：现场定位意图的统一实现与界面入口。

### Modified Capabilities

- `ui-game-system-integration` 的现场内容页：六个「聚焦」按钮从「未接线」变为已接线。

## Impact

- 输入：`Assets/Game/Scripts/AutoEra/Input/RegionInputModule.cs`。
- 界面：`Assets/Game/Scripts/AutoEra/UI/FieldHudForm.cs`。
- 契约与预制体绑定：`Tools/ui_spec_to_contract.py` → 33 份契约 →
  `Game Framework/AutoEra/UI/按契约刷新所有页面绑定（不改结构）`。结构不变。
- 验收：门1 必须持续全绿（33 契约）；`run_project_checks.py` 5/5；
  EditMode 全类、PlayMode **13 套**全绿；
  新增 `FieldHudLocatePlayModeTests`（1/1，PlayMode 套数 12 → 13）。
- **不在本变更内**：跨对象定位（`查看水源`／`定位关联对象`／`前往现场`）、
  中枢与记录页的「定位」（要先关闭当前界面再回到现场）、自动聚焦与镜头缓动。
- **风险点**：这一批最容易犯的错是「按钮能聚焦，但聚焦到别处」——它不会报错。
  用例断言两件事：按钮按下后的焦点**就是该对象的锚点**，以及**喂一帧 F 得到完全相同的焦点**；
  第二条才是「两条路汇入同一实现」的证据。
