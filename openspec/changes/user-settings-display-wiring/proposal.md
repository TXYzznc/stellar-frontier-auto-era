# 用户设置接入：显示与性能分页

## Why

`ui-game-system-integration` 的 2.7 把 `SettingsForm` 判为「未接入」，理由是
「声音、操作与显示设置还没有持久化载体」。代码侦察的结论**与这个判定不符**：

| 零件 | 状态 | 生产调用者 |
|---|---|---|
| `GF.Setting`（`SettingComponent`，带文件落盘） | 已交付 | `PreloadProcedure` 在启动时用它读写**语言**与**两个声音分组的音量**，`SettingExtension` 已有 `SetMediaVolume`/`GetMediaVolume` 封装 |
| 显示与性能的真实后端（`Screen.fullScreenMode` / `QualitySettings` / `Application.targetFrameRate`） | Unity 自带 | 无人调用 |
| 工程画质档位（`Performant` / `Balanced` / `High Fidelity`，各档带真实阴影与抗锯齿配置） | 已配置 | 无人展示 |
| 设置三页的契约、预制体与控件（滑条、档位按钮、开关按钮） | 齐备（含 `Sld_*` 与各档位 `Btn_*`） | 只有 `NOT_WIRED` 注入的「整页未接入」 |

也就是说，**持久化载体早就在生产里**，缺的是两样东西：
① 界面能注入的**存储边界**（界面只能直接摸 `GF.Setting` 这个全局组件，既不可测，
也会把「界面依赖全局状态」种回来）；② 控件与后端之间的接线。

同时侦察也发现**真的缺口**，而且三页的缺口各不相同——这正是不能再用一句
「设置未接入」盖住整页的原因：

- 声音分页：规格要求主音量／音乐／环境／机器与生产／UI 与警报**五路**独立音量，
  而 `SoundGroupTable` 只有 `Music` 与 `Sound` 两组。缺的几路需要先新增音频分组与其资源。
- 操作分页：镜头速度与键位重绑需要一张**可持久化的输入绑定表**，现场输入用的
  `RegionInputBindingSet` 目前没有持久化载体。
- 显示与性能分页：**没有缺口**——`Screen` / `QualitySettings` / `Application` 都是真实后端。

## What Changes

- 新增**设置存储边界** `ISettingsStore`（字符串／整数／布尔三类的读写 + 落盘），
  生产实现是 `GfSettingsStore`（`SettingComponent` 的适配器），测试用内存实现走**同一条**读写路径。
  落盘的诚实性写在契约里：框架的 `SettingComponent.Save()` 返回 `void`，因此只能把「抛异常」
  当作失败信号；界面据此说的是「已写入本机设置」而不是「已验证保存成功」。
- 新增**显示与性能设置服务** `AutoEraDisplaySettings`：读取、写入、应用三件事集中一处。
  - 读：显示模式／垂直同步／帧率上限／画质档位，缺省值分别取自工程当前状态；
  - 写：只接受规格列出的取值（30／60／不限），非法值给出原因且不改动已存值；
  - 应用：**只在 `Application.isPlaying` 时写回引擎**——`QualitySettings.SetQualityLevel`
    在编辑器非播放模式下会改写 `ProjectSettings/QualitySettings.asset`，那会变成一次意外提交。
- **垂直同步与帧率上限的关系显式化**：规格原文要求「垂直同步控制帧率时说明帧率上限的实际作用，
  不显示互相矛盾的已生效状态」。服务把它作为一条可展示事实（`FrameLimitEffective` +
  `FrameLimitExplanation`）暴露，界面照它说话，不各写一份判断。
- 新增**设置域读模型** `ISettingsReadModel`：三个分页各自一态——显示与性能 `Ready`，
  声音与操作 `Unavailable` 且**原因互不相同**。
- `SettingsForm` 转为手写接入：显示与性能分页渲染真实行并可操作；另外两页 Disabled 并写明各自缺口；
  **初始页落在第一个已接线的分页**（规格未规定初始页，把玩家丢在打不开的页上没有任何好处）。
  刻意不复用 `DisableDomainActions()`：它按结构名禁用除出口外的所有按钮，
  而本页现在唯一真实的动作就在其中。

## Capabilities

### New Capabilities

- `user-settings-persistence`：本机设置的存储边界、显示与性能的读写与应用、三页的接线程度与原因。

### Modified Capabilities

- 无。本变更不改界面契约结构（只新增控件绑定）、不改预制体结构、不改 `UIViews` 登记、不改数据表。

## Impact

- 代码：`Assets/Game/Scripts/AutoEra/Settings/`（新增）、
  `Assets/Game/Scripts/AutoEra/UI/Integration/SettingsReadModel.cs`（新增）、
  `Assets/Game/Scripts/AutoEra/UI/SettingsForm.cs`（转为手写）、
  `Tools/ui_contract_to_form_script.py`（`SettingsForm` 从 `NOT_WIRED` 移到 `HANDWRITTEN`；
  新增 `Slider` / `Toggle` 的 kind 映射）、`Tools/ui_spec_to_contract.py`（`EXTRA_BINDINGS`）。
- 验收：门1 必须持续 33/33；`run_project_checks.py` 5/5；EditMode/PlayMode 回归全绿；
  新增设置域 EditMode 用例与「三页接线程度不同」的 PlayMode 用例。
- **不在本变更内**：声音分页（需要先新增音频分组与资源）、操作分页（需要可持久化的输入绑定表）、
  显示模式切换的「保留／恢复」确认（`DisplayKeep` 属于 `OperationDialogForm`，未接线）。
- **风险点**：`QualitySettings` 在编辑器非播放模式下写入会污染工程资产——已由
  `Application.isPlaying` 判据挡住，并由 EditMode 用例守着。
