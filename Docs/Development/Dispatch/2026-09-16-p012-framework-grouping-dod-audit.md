# P0-012 接入项目 UI、实体与声音分组 — DoD 对照审计与回归证据

日期：2026-09-16。任务来源：第一版开发任务表（client 队列 `p0012-ui-entity-sound-grouping`）。

本记录只对照 P0-012 的原始 DoD：**项目 UI、实体和声音能够按配置加载；无资源时不会阻断启动。**
不重做已完成的 P0-010 实体预制体、锚点、动作系统或场景资产。

## 实际配置与启动链路

| 配置/代码 | 实际内容 | 启动消费位置 |
|---|---|---|
| `Assets/Game/ScriptableAssets/Core/AppConfigs.asset` | 已登记 `Core/EntityGroupTable`、`Core/SoundGroupTable`、`Core/UIGroupTable`、`Core/UITable` | `PreloadProcedure` 的预加载表清单 |
| `Assets/Game/DataTable/Core/EntityGroupTable.txt` | `Default`、`Effect`、`Persistent` 三个实体组 | `PreloadProcedure.InitGameFrameworkSettings()` 创建或更新 `GF.Entity` 分组 |
| `Assets/Game/DataTable/Core/SoundGroupTable.txt` | `Music`、`Sound` 两个声音组 | 同一初始化方法创建或更新 `GF.Sound` 分组 |
| `Assets/Game/DataTable/Core/UIGroupTable.txt` | `Default`、`Dialog`、`Overlay` 三个 UI 组 | 同一初始化方法创建或更新 `GF.UI` 分组 |
| `Assets/Game/DataTable/Core/UITable.txt` | `MainMenuForm`、`FieldHudForm`、`BaseCommandHubForm` 三行 | `UIExtension.OpenUIForm` 通过 UI 行与 UIGroup 行解析资源路径和显示层级 |

`UIExtension.OpenUIForm` 对未加载 `UITable`/`UIGroupTable`、未配置 UI ID、未配置 UIGroup 均只记录 Warning 并返回 `-1`，不会抛出启动阻断异常。

## 本次补足的验证

新增 `Assets/Game/Tests/AutoEra/Editor/FrameworkGroupingDegradationEditModeTests.cs`：

- 从 `Assets/Game/Scene/Launch.unity` 正式启动路径进入 Play Mode；
- 等待可交互的 `MainMenuForm`，再断言三类配置组均已创建：
  - Entity：`Default`、`Effect`、`Persistent`；
  - Sound：`Music`、`Sound`；
  - UI：`Default`、`Dialog`、`Overlay`；
- 以未配置枚举值 `6999` 调用 `UIExtension.OpenUIForm`，断言不抛异常、返回 `-1`，且主菜单仍可交互。

## 验证结果

1. `asset_refresh` 后等待 Unity 编译完成；未观察到本次测试引入的编译错误。
2. `FrameworkGroupingDegradationEditModeTests.Startup_AppliesConfiguredGroups_AndUnconfiguredUiDoesNotBlockStartup`：**1/1 通过**。
   - TestResults.xml 时间：2026-09-16 08:56:55Z–08:57:04Z；耗时 8.672 秒。
3. 已有回归 `AutoEraStartupFlowEditModeTests.MissingSound_ReportsOwnedFailureWithoutBlockingOtherGroups`：**1/1 通过**。
   - 验证正式启动后故意缺失的 `Assets/Game/Audio/B10MissingSoundForValidation.wav` 只产生所属声音加载失败，`Music` 组及后续菜单→初始区域流程仍可用。
   - TestResults.xml 时间：2026-09-16 08:57:35Z–08:57:44Z；耗时 9.328 秒。
4. 全量 EditMode 回归：**480 用例，477 通过，3 失败，0 跳过/不确定**。
   - TestResults.xml 时间：2026-09-16 09:01:00Z–09:03:09Z；耗时 129.000 秒。
   - 新增的 P0-012 分组/安全降级用例包含在全量结果内并通过（9.258 秒）。
   - 3 个失败均与此前已知基线一致：
     1. `AutoEraUiPrefabBindingEditModeTests.OperationsEntryPrefab_IsIndependentOfTheRetiredVisualCandidateChain`（`FieldHudForm.prefab` 仍引用已退休候选链）；
     2. `DataTableGenerationProfileEditModeTests.ProjectProfile_LoadsFoundationRuleFromEditorOnlyJson`（期望 1 条、实际 4 条配置）；
     3. `InitialRegionSceneEditModeTests.SavedRegion_RegistersAndReleasesActualViews_AndSelectionIsIsolated`（Edit Mode 调用 `Destroy` 的既存日志错误）。
   - 这 3 项没有新增 P0-012 相关失败。原始结果保留于 `C:\Users\WIN10\AppData\LocalLow\ZZNC\星际拓荒：自动纪元\TestResults.xml`，并已复制为同目录的 `TestResults-p0012-full-editmode-20260916.xml` 备份。

## 边界与结论

- P0-012 的配置、注册、启动消费和 UI 安全降级实现此前已存在；本次没有为凑任务重复修改表、预制体或框架代码。
- 本次唯一代码产物为针对真实 Launch 启动路径的回归测试。
- 未修改 `Assets/Game/ScriptsBuiltin/`、场景、实体预制体、任务表 xlsx 或 Git 索引。
- 对 P0-012 的原始 DoD，配置组创建、未配置 UI 安全降级、缺失声音不阻断后续流程均已有可重复的通过证据；完整 EditMode 回归已补跑，结果仅含上述 3 个既存基线失败。