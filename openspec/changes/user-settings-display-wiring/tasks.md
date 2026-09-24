# 任务

## 1. 设置存储边界与显示与性能服务

- [x] 1.1 新增 `ISettingsStore`（字符串／整数／布尔读写 + 落盘）与生产适配器 `GfSettingsStore`。
      （已实现于 `Assets/Game/Scripts/AutoEra/Settings/AutoEraDisplaySettings.cs`。
      为什么必须有这条边界：界面需要一个**可注入**的存储，否则测试只能依赖 `GF.Setting`
      这个全局组件在 EditMode 下也可用——那既不可靠，也会把「界面依赖框架全局状态」种回来。
      落盘接口的**诚实性**：框架的 `SettingComponent.Save()` 返回 `void`，不报告结果；
      因此 `GfSettingsStore.Save` 只能把「抛异常」当失败信号（磁盘满／目录不可写都会抛），
      并把这一点写进注释，而不是假装验证过落盘结果。）
- [x] 1.2 新增 `AutoEraDisplaySettings`：读取（显示模式／垂直同步／帧率上限／画质档位）、
      写入（各带原因）、应用（`Screen` / `QualitySettings` / `Application`）。
      （已实现。三个要点：
      ① **编辑器非播放模式不写回引擎**——`QualitySettings.SetQualityLevel` 会改写
      `ProjectSettings/QualitySettings.asset`，那会变成一次意外提交；判据是 `Application.isPlaying`，
      由 EditMode 用例守着。
      ② **垂直同步与帧率上限的关系显式化**：Unity 在 `vSyncCount > 0` 时忽略 `targetFrameRate`，
      规格要求不得显示互相矛盾的状态；服务把它作为 `FrameLimitEffective` + `FrameLimitExplanation`
      暴露，界面照它说话。
      ③ 默认画质取工程当前档位而不是写死索引，工程改默认档时这里自动跟上。
      阴影与抗锯齿的展示读**引擎真实数值**（`QualitySettings.shadows` / `shadowDistance` /
      `antiAliasing`），而不是设计文档里的期望值——它们是画质档位打包的一部分，
      只有「当前档位下实际生效的值」可以诚实回答。）
- [x] 1.3 写入失败的行为：值仍写入内存与设置组件，并把可展示原因交回调用方。
      （已实现：`SetXxx` 返回 false 并给出 `reason`，界面保留新值 + 显示原因，
      与规格「写入失败保留内存值并说明重启可能丢失」一致。）

## 2. 设置域读模型

- [x] 2.1 新增 `ISettingsReadModel` + `SettingsDomainSnapshot`（三页各自一态）与诚实空实现。
- [x] 2.2 真实实现 `DisplaySettingsReadModel`：显示与性能 `Ready`（两栏行），
      声音与操作 `Unavailable` 且**原因互不相同**；`InitialPage` 给出第一个已接线的分页。
      （已实现。设置**没有领域事件源**——改动只可能由本页发起，所以只有显式 `Refresh()`，
      与存档域「文件系统没有推送」同理，不去伪造一条并不存在的事件流。
      值类型快照在每次刷新时重建。）
- [x] 2.3 `SettingsReadModels.Create(session)` / `Create(store)`：设置组件不可用时给可辨原因。
      （已实现。`GF.Setting` 在框架未就绪时可能抛异常，已捕获并转成可展示原因。）

## 3. 设置界面接入

- [x] 3.1 `SettingsForm` 转为手写接入：显示与性能分页渲染真实行并可操作，
      声音与操作分页 Disabled 并各写原因；初始页落在第一个已接线的分页。
- [x] 3.2 `EXTRA_BINDINGS` 新增 21 个控件绑定（11 个显示档位／开关按钮 +
      5 个音量滑条与重置 + 3 个镜头速度滑条与重置），生成器新增 `Slider`/`Toggle` 类型映射。
      （为什么**未接线分页的控件也要绑定**：靠「没有绑定所以点不动」会让排查的人
      分不清「未接线」与「忘了接」。现在未接线分页的控件由 `SetXxxOptionsInteractable(false)`
      **明确**禁用，原因写在页面上。）
- [x] 3.3 `SettingsForm` 从生成器的 `NOT_WIRED` 移到 `HANDWRITTEN`；回归围栏名单同步迁移。
      （`AutoEraNotWiredFormsEditModeTests` 的未接入名单 14 个，已接入名单加入 `SettingsForm`。）

## 4. 验收

- [x] 4.1 编译 0 错。
      （`tools/_unity_compile.py` 通过：程序集已更新且 console 无 CS 错误；工程检查第 1 项同结论。）
- [x] 4.2 门1 契约自检 33/33；`run_project_checks.py` 5/5。
      （门1 通过（本轮只新增控件绑定，节点树与 RectTransform 一字未改）；
      `run_project_checks.py --port 8090` **5/5 PASS**：编译 0 错、悬空引用 0、
      AppConfigs 12 表/1 配置/3 语言/6 流程、AIData 校验 12 成功 0 失败、框架纯度与项目边界通过。）
- [x] 4.3 新增 `SettingsReadModelEditModeTests`：三个原因各自可辨、真实存储下显示页 Ready
      且行齐备、写入真的落盘并刷新行、**垂直同步压制帧率上限且行文说明它**、
      非法值被拒且不改动已存值、恢复默认只动本页的键、写入失败保留内存值并给原因、
      **编辑器非播放模式不把画质写回引擎**。
      （**8/8 通过**，见 `Assets/Game/Tests/AutoEra/Editor/UI/SettingsReadModelEditModeTests.cs`。
      测试完全脱离框架组件——因为读写只经过 `ISettingsStore`，内存实现与生产适配器走同一条路径。）
- [x] 4.4 新增 `SettingsFormPlayModeTests`：真实运行时下打开设置，
      断言显示与性能 `Ready`／声音 `Unavailable`、初始页落在显示与性能、
      显示页控件可点而声音与操作页控件确实禁用、切到声音页后正文给出「音频分组」这一真实缺口。
      （**1/1 通过**。刻意不点显示模式按钮——那会真的切换编辑器全屏模式。）
- [x] 4.5 EditMode / PlayMode 全量回归。
      （EditMode **22 类**全绿：原 21 类 ＋ `SettingsReadModelEditModeTests` 8/8；
      `AutoEraNotWiredFormsEditModeTests` 由 15 项变为 **14 项**（`SettingsForm` 已迁出），
      并加入「已接入界面不得带未接入脚手架」那条断言。
      PlayMode **8 套**全绿：原 7 套 ＋ `SettingsFormPlayModeTests` 1/1。
      进 Play Mode 的三个 EditMode 类全绿：1/1、1/1、6/6。）

## 5. 边界

本变更不实现：声音分页（需先新增音频分组与资源）、操作分页（需可持久化的输入绑定表）、
显示模式切换的「保留／恢复」确认（`DisplayKeep` 属于 `OperationDialogForm`，未接线）。

不修改界面契约结构（只新增控件绑定）、不改预制体结构、不改 `UIViews` 登记、不改数据表。
