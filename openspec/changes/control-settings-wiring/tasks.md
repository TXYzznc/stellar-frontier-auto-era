# 任务

## 1. 镜头域：把参数变成可注入的目标

- [x] 1.1 新增 `AutoEra.World.Region.RegionCameraControl.cs`：
      `IRegionCameraTarget`（`PanSpeed`／`RotationSpeed`／`ZoomSpeed`／`InvertHorizontal`／`InvertVertical`）
      与 `RegionCameraParameters`（区间与默认值的**单一来源**：平移 4–40 默认 15、
      旋转 1–8 默认 3、缩放 1–15 默认 5，加 `ClampPan/ClampRotation/ClampZoom`，NaN 取默认）。
      默认值取镜头组件现有的序列化值，保证不改变现有手感。
- [x] 1.2 `RegionCameraController` 实现 `IRegionCameraTarget`：序列化字段初值改从
      `RegionCameraParameters` 取；`Apply()` **真的消费** `_invertHorizontal` / `_invertVertical`
      （水平反转作用于轨道旋转与平移的水平分量，垂直反转作用于俯仰与滚轮方向）。
      **反转此前根本没有实现**——这是本批补的实现，不是接线。
- [x] 1.3 `RegionInputModule` 新增 `CameraTarget => _camera`（一行），把现场镜头交给设置域。
      设置域因此不需要认识 `MonoBehaviour`，也不需要自己到场景里搜组件。

## 2. 设置域：操作分页

- [x] 2.1 新增 `AutoEra.Settings.AutoEraControlSettings`：五个键
      `AutoEra.Control.PanSpeed|RotationSpeed|ZoomSpeed|InvertHorizontal|InvertVertical`；
      读 `PanSpeed/RotationSpeed/ZoomSpeed/InvertHorizontal/InvertVertical`；
      写 `SetPanSpeed/SetRotationSpeed/SetZoomSpeed/SetInvertHorizontal/SetInvertVertical`
      （越界或 NaN 一律**拒绝并给原因**，不留下半份状态）；
      `ResetToDefaults`（只动本页）、`Save`、`ApplyTo(IRegionCameraTarget)`、`DifferenceFromDefaults()`。
- [x] 2.2 区间不在设置域里另写一份：`ClampPan/ClampRotation/ClampZoom` 就是滑条区间与镜头取值的那一套。
      「滑条能拖到 40、镜头在 15 处封顶」这类界面撒谎，源头就是三处各写一份。
- [x] 2.3 `AutoEraUiFormat` 新增 `KeyLabel(KeyCode)`：`Mouse0`→鼠标左键、`Mouse1`→鼠标右键、
      `Escape`→Esc……只读按键栏要用它把 `KeyCode` 译成界面用词。
- [x] 2.4 `SettingsReadModel` 操作页改为 Ready，三栏：
      ① 镜头栏（三项速度 **值 ＋ 区间**，区间与滑条同源 ＋ 两项反转状态）；
      ② 本机配置状态栏（与默认值差异条数）；
      ③ **只读**当前按键栏，从 `session.RegionInput.Bindings` 现场读取，
      顺序与规格一致：移动／旋转／缩放／聚焦／返回／放置旋转。
      没有现场输入模块（世界外打开设置）时**如实说明读不到**，不拿默认映射冒充。
- [x] 2.5 「缩放」不编键名：滚轮与指针在设备层是**通道**而不是按键，所以照实写
      「鼠标滚轮（设备通道，不可改键）」。
- [x] 2.6 `SettingsForm`：三个滑条 ＋ 两个开关接到写入口；渲染用 `SetValueWithoutNotify` ＋
      `_rendering` 兜底（与前两页同一套，防「渲染变写入」）；
      「恢复本页默认」只动本页；写入后立刻 `ApplyTo(session.RegionInput.CameraTarget)`，
      世界外没有镜头时静默通过（参数已存好，进区域时由现场镜头取用）。
- [x] 2.7 契约：`Tools/ui_spec_to_contract.py` 的 `SettingsForm` 条目加两个 Toggle 绑定
      （`_controlSettingsInvertHorizontalToggle` / `_controlSettingsInvertVerticalToggle`）——
      **结构不变**，只是让两个开关有字段可接。
- [x] 2.8 **区域入口也要应用一次**：`SettingsReadModels.CreateControlSettings()`（只取操作分页的设置对象，
      不需要整套读模型）＋ `AutoEraWorldProcedure` 在区域就绪、镜头刚建好时 `ApplyTo(CameraTarget)`。
      不做这一步，「在世界外调好的手感」只有玩家再打开一次设置页才会生效——
      等于让保存这件事对玩家撒谎。`CameraTarget` 为 null 时静默通过（场景没配镜头是合法情况）。

## 3. 验收

- [x] 3.1 编译 0 错。
- [x] 3.2 跑 `Game Framework/AutoEra/UI/按契约刷新所有页面绑定（不改结构）`（加绑定后的必备步骤），
      门1 契约自检全绿（**33/33**）；`run_project_checks.py` 5/5。
- [x] 3.3 新增 `ControlSettingsEditModeTests`（**10/10**）：
      默认值与镜头序列化值一致；滑条区间与夹取同源；越界／NaN 被拒绝且有原因；
      重新读取能看到存的值；**`ApplyTo` 把五个参数都推到镜头上**（反转也算）；
      `ApplyTo(null)` 是世界外的正常情况而非错误；恢复默认只动本页（显示与声音分页保留）；
      写入失败保留内存值并说明后果；一次写入只落盘一次；
      **本页没有改键 API**（范围守卫——一旦有人偷偷加，这条会失败）。
- [x] 3.4 `SettingsReadModelEditModeTests` 扩展到 **12/12**：操作页 Ready 且给出三项速度（带区间）＋
      两项反转 ＋ 与默认值差异共 6 行；没有现场输入模块时按键栏为空并在页面上说明
      （不拿默认映射冒充）；有模块时给出 6 行且「缩放」照实说是设备通道；
      **`Refresh` 不得累积重复行**（每次 Publish 五个列表都要清空重建）。
      顺带更正：操作页不可用的原因从「绑定表」改为「镜头参数没有可读写存储载体」。
- [x] 3.5 `SettingsFormPlayModeTests` 扩成三页端到端（仍是**一个 `[UnityTest]`**，每类只能一个）：
      拖操作页滑条 → 本机设置值变化 **且现场镜头组件上的对应属性真的变了**；
      勾反转 → 镜头的反转开关真的打开；本页恢复默认只恢复本页。
- [x] 3.6 回归：EditMode 全类、PlayMode **12 套**全绿；进 Play Mode 的 EditMode 类全绿；
      门1 33/33 持续绿。

## 4. 边界

本变更不实现：改键（设计原文「第一版不开放改键，只读显示 InputModule 当前绑定」）、
镜头灵敏度曲线与加速度（设计留给灰盒调优）、手柄与多设备映射（规格未要求）。

不改界面契约的结构与预制体节点；不改 `UIViews` 登记。

## 5. 一条反复出现的判据错误（第五次）

操作分页当初被写成「未接入」，理由是「拿不到相机控制组件」与「缺可持久化的绑定表」。这次侦察发现：

- 相机控制组件**本来就在会话里**（现场输入模块持有它），本批只加了一行 `CameraTarget` 属性；
  速度也早就是真实后端（`RegionCameraController` 的三个序列化字段）。真正的缺口是
  **反转没有实现**与**参数没有单一来源**——两件都不是「接线」。
- 「绑定表」不是缺口而是**范围**：设计明确写「第一版不开放改键」。把设计排除掉的东西写成待办，
  是同一个判据错误的另一个方向——**把「设计不让做」当成了「还没做」**。

前四次误判是 `ComponentLibraryForm`、`SettingsForm` 显示页、`ComponentPickerForm`、
`SettingsForm` 声音页（见 `ui-game-system-integration` 的 2.7 与 `audio-settings-wiring` 的 §6）。
五次全是同一类错误：**把「还没有这一页的读模型／数据」当成了「这个域没有后端」**。

因此本批把那条纪律再补一句：在给任何界面写「未接入」之前先做代码侦察——
判据是「该界面依赖的领域服务在生产里没有创建者」，**并且**要区分
「没有创建者」（缺口）与「设计明确不做」（范围）。后者该写进「不在本变更内」。
