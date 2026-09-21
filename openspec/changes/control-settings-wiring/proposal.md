# 操作设置接线（镜头速度与反转）

## Why

设置页三页中，显示与性能在 `user-settings-display-wiring` 接线，声音在 `audio-settings-wiring` 接线。
操作分页一直标着「未接入」，理由写在读模型里，原文是两条：

1. 「镜头速度需要拿到场景里的相机控制组件」；
2. 「键位重绑需要可持久化的输入绑定表」。

侦察后发现**两条都不成立**（这是同一类误判的**第五次**）：

| 当时写的缺口 | 生产里的既有实现 | 结论 |
|---|---|---|
| 「拿不到相机控制组件」 | `RegionInputModule.CameraTarget`（本批新增的一行属性）直接交出 `RegionCameraController`；`RegionCameraController` 本来就有序列化的 `_panSpeed=15`／`_rotationSpeed=3`／`_zoomSpeed=5` | **假缺口**。速度是真实后端，缺的只是一个可注入的目标接口 |
| 「缺可持久化的绑定表」 | 设计**明确排除**改键：`03-玩家体验/01-视角交互与信息呈现.md` 写「第一版不开放改键，只读显示 InputModule 当前绑定」，并另有一条「输入实现必须统一走项目 InputModule」 | **不是缺口，是范围**。把设计排除掉的东西写成待办，是第二个方向的错 |

真正缺的只有两样，都不是「没有后端」：

1. **反转没有实现**——`RegionCameraController` 里没有任何 `Invert*` 字段，勾选框无处可落。
   这是本批补的实现，不是接线。
2. **参数没有单一来源**——滑条区间、本机设置的夹取、镜头自己的取值各写一份，
   表现就是「滑条能拖到 40、镜头在 15 处封顶」。本批把区间收到 `RegionCameraParameters`。

第五次和第四次形态完全一样：**把「还没有这一页的读模型」当成了「这个域没有后端」**。
前四次见 `ui-game-system-integration` 的 2.7 与 `audio-settings-wiring` 的 §6。

## What Changes

### 镜头域（把参数变成可注入的目标）

- 新增 `AutoEra.World.Region.RegionCameraControl.cs`：
  - `IRegionCameraTarget`——`PanSpeed` / `RotationSpeed` / `ZoomSpeed` / `InvertHorizontal` / `InvertVertical`
    五个可读写属性。设置域因此既不需要认识 `MonoBehaviour`，也不需要自己到场景里搜组件。
  - `RegionCameraParameters`——区间与默认值的**单一来源**：平移 4–40（默认 15）、
    旋转 1–8（默认 3）、缩放 1–15（默认 5），加 `ClampPan/ClampRotation/ClampZoom`（NaN 取默认值）。
    默认值取镜头组件现有的序列化值，保证不改变现有手感；具体数值设计留给灰盒调优
    （`01-视角交互与信息呈现.md`：「镜头移动速度、旋转速度、缩放上下限……在灰盒原型中调优，
    不改变本文交互规则」），所以这里给的是一个**可用的调优区间**，不是臆造的精确值。
- `RegionCameraController`：实现 `IRegionCameraTarget`；序列化字段的初值改为从
  `RegionCameraParameters` 取（单一来源）；`Apply()` 真的消费 `_invertHorizontal` / `_invertVertical`
  ——水平反转作用于轨道旋转与平移的水平分量，垂直反转作用于俯仰与滚轮方向。
- `RegionInputModule`：新增 `CameraTarget` 属性（一行），把现场镜头交出去。

### 设置域

- 新增 `AutoEra.Settings.AutoEraControlSettings`：键
  `AutoEra.Control.PanSpeed|RotationSpeed|ZoomSpeed|InvertHorizontal|InvertVertical`；
  `PanSpeed/RotationSpeed/ZoomSpeed/InvertHorizontal/InvertVertical` 读、
  `SetPanSpeed/SetRotationSpeed/SetZoomSpeed/SetInvertHorizontal/SetInvertVertical` 写（越界即拒绝并给原因）、
  `ResetToDefaults`（只动本页）、`Save`、`ApplyTo(IRegionCameraTarget)`、`DifferenceFromDefaults()`。
- `AutoEraUiFormat`：新增 `KeyLabel(KeyCode)`，把 `KeyCode` 译成界面用词
  （`Mouse0`→鼠标左键、`Mouse1`→鼠标右键、`Escape`→Esc 等）——只读按键栏要用它。
- `SettingsReadModel`：操作页改为 Ready，给出
  - 镜头栏：三项速度（**值 ＋ 区间**，区间与滑条同源）＋ 两项反转开关状态；
  - 本机配置状态栏：与默认值差异条数；
  - **只读**的当前按键栏：从 `session.RegionInput.Bindings` 现场读取（`移动` / `旋转` / `缩放` / `聚焦` / `返回` / `放置旋转`）。
    「缩放」照实写「鼠标滚轮（设备通道，不可改键）」——滚轮与指针在设备层是**通道**而不是按键，
    不编一个键名出来。
- `SettingsForm`：三个滑条 ＋ 两个开关接到写入口；渲染用 `SetValueWithoutNotify` ＋ `_rendering`
  兜底（与前两页同一套，防「渲染变写入」）；「恢复本页默认」只动操作页；
  写入后立刻 `ApplyTo(session.RegionInput.CameraTarget)`，世界外没有镜头时静默通过（参数已存好，进区域时取用）。
- **区域入口应用一次**：`SettingsReadModels.CreateControlSettings()` 单独给出操作分页的设置对象，
  `AutoEraWorldProcedure` 在区域就绪、镜头刚建好时把保存的参数推上去。
  只做「写入时应用」是不够的——玩家在世界外调好的手感，不打开设置页就不生效，
  那等于让保存这件事对玩家撒谎。

## Capabilities

### New Capabilities

- `control-settings-wiring`：镜头速度与反转的持久化、应用到现场镜头，以及只读按键栏。

### Modified Capabilities

- `ui-game-system-integration` 的 2.7：设置页的操作分页从「未接入」变为已接线，理由更正（留痕写在那一节）。

## Impact

- 产品层：`World/Region/RegionCameraControl.cs`（新）、`World/Region/RegionCameraController.cs`、
  `Input/RegionInputModule.cs`、`Settings/AutoEraControlSettings.cs`（新）、
  `UI/Integration/SettingsReadModel.cs`、`UI/Integration/AutoEraUiFormat.cs`、`UI/SettingsForm.cs`。
- 契约：`Tools/ui_spec_to_contract.py` 的 `SettingsForm` 条目加两个 Toggle 绑定
  （结构不变，只是让「反转」两个开关有字段可接）；随后必须跑
  `Game Framework/AutoEra/UI/按契约刷新所有页面绑定（不改结构）` 与门1。
- 验收：门1 必须持续全绿（33 契约）；`run_project_checks.py` 5/5；
  EditMode 全类、PlayMode 12 套全绿；新增 `ControlSettingsEditModeTests` 与
  `SettingsFormPlayModeTests` 的操作页端到端断言。
- **不在本变更内**：改键（设计明确不开放）、镜头灵敏度曲线／加速度（设计留给灰盒调优）、
  手柄与多设备映射（规格未要求）。
- **风险点**：这一页最容易留下的是**假接线**——界面能勾选、存储里有值、镜头不读它。
  用例 `ApplyTo_PushesEveryParameterOntoTheCamera` 与 PlayMode 的端到端断言专门守这条；
  区间用语例 `SliderRangesAndClampingShareOneSource` 守着「滑条区间＝夹取区间＝镜头取值」。
