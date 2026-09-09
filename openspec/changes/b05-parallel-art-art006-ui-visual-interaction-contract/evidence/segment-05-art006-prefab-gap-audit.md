# ART-006 视觉 Prefab 缺口只读审计

日期：2026-09-04
执行包：`b05-art006-prefab-gap-audit`
接收负责人：`art-2d`

## 审计范围

本次仅读取以下 ArtResource 内容及当前候选文件，未修改 ArtResource、Prefab、场景、脚本、任务表、Git 或 Unity 状态：

- `D:\unity\UnityProject\ArtResource\Docs\ArtPipeline\UIRequirements\ART006_UI_ProductionPackage_20260902\00_通用UI素材\prefab-layout.md`
- `D:\unity\UnityProject\ArtResource\Docs\ArtPipeline\UIRequirements\ART006_UI_ProductionPackage_20260902\01_基地主界面_总览\prefab-layout.md`
- `D:\unity\UnityProject\ArtResource\Docs\ArtPipeline\UIRequirements\ART006_UI_ProductionPackage_20260902\02_基地主界面_待处理任务\prefab-layout.md`
- `D:\unity\UnityProject\ArtResource\Docs\ArtPipeline\UIRequirements\ART006_UI_ProductionPackage_20260902\03_基地主界面_对象与系统\prefab-layout.md`
- `D:\unity\UnityProject\ArtResource\Docs\ArtPipeline\UIRequirements\ART006_UI_ProductionPackage_20260902\04_基地主界面_规则自动化\prefab-layout.md`
- `D:\unity\UnityProject\ArtResource\Docs\ArtPipeline\UIRequirements\ART006_UI_ProductionPackage_20260902\05_基地主界面_统计\prefab-layout.md`
- `D:\unity\UnityProject\ArtResource\Docs\ArtPipeline\UIRequirements\ART006_UI_ProductionPackage_20260902\06_现场HUD与对象面板\prefab-layout.md`
- `D:\unity\UnityProject\ArtResource\Docs\ArtPipeline\UIRequirements\ART006_UI_ProductionPackage_20260902\07_影响预览确认弹窗\prefab-layout.md`
- `D:\unity\UnityProject\ArtResource\Assets\Art\Authoring\ART006_UI\PrefabCandidates\VisualCandidates\BaseCommandHubForm_VisualCandidate_V01.prefab`
- `D:\unity\UnityProject\ArtResource\Assets\Art\Authoring\ART006_UI\PrefabCandidates\VisualCandidates\BaseCommandHubForm_VisualCandidate_V02.prefab`
- `D:\unity\UnityProject\ArtResource\Assets\Art\Authoring\ART006_UI\PrefabCandidates\VisualCandidates\FieldHudForm_VisualCandidate_V01.prefab`
- `D:\unity\UnityProject\ArtResource\Assets\Art\Authoring\ART006_UI\PrefabCandidates\VisualCandidates\FieldHudForm_VisualCandidate_V02.prefab`
- `D:\unity\UnityProject\ArtResource\Assets\Art\Authoring\ART006_UI\PrefabCandidates\VisualCandidates\ART006_UI_VisualCandidate_V02.unity`

## 门禁结论

结论：**FAIL（存在可复核的装配缺口）**。

8 份 `prefab-layout.md` 均可定位；候选文件也存在，但当前 V01/V02 候选并未形成可覆盖 8 页合同的完整视觉装配。审计只依据序列化节点名、激活字段和组件引用静态证据，不把截图或资源目录存在误判为 Prefab 已装配。

## 缺口清单

### 1. 公共 Form 骨架：缺失或未在候选节点树中出现

- `Panel_ContentClip`
- `Panel_Page*` 页面容器集合
- `Grp_Terminal`
- `Panel_TopBar`
- `Grp_Navigation`
- `Grp_ResourceTimeSpeed`
- `Btn_ExitPhysical`
- `Overlay_RulesImpact`
- 5 个导航按钮：`Btn_TabOverview`、`Btn_TabTasks`、`Btn_TabObjects`、`Btn_TabRules`、`Btn_TabStatistics`

### 2. UI-01 总览页：缺失或未完成绑定

- `Panel_PageOverview`、`Panel_MapViewport`、`Grp_MachineNodes`
- `Btn_Machine*` 节点集合及对应状态层
- `Panel_SelectionDetail`
- `Panel_AlertPrimary` 与 `Btn_AlertPrimary`
- 机器节点名称、状态、载体与效应器的完整 Image/TMP 绑定集合
- `Normal`、`Selected`、`Warning`、`Danger`、`Inactive`、`Loading`、`Error`、`Success` 状态层

### 3. UI-02/UI-03：只覆盖少量模板文本，缺少页面装配

现有候选中可见 `List_TaskQueue`、`Item_TaskTemplate`、`Panel_TaskDetail`、`List_Objects`、`Item_ObjectTemplate` 及部分 `Txt_*` 文本名，但未形成可复核的完整页面树。缺少或未确认：

- UI-02 的 `Panel_PageTasks`、`Panel_TaskFilters`、`Viewport_TaskQueue`、`Content_TaskQueue`、`Viewport_TaskDetail`
- UI-02 的过滤器、空态、错误/危险确认、加载与成功状态节点
- UI-03 的 `Panel_PageObjects`、`Panel_ObjectCategories`、`Panel_ObjectFilters`、`Viewport_Objects`、`Content_Objects`、`Panel_ObjectDetail`
- UI-03 的搜索输入、分类筛选、对象状态、对象详情与异步状态节点

### 4. UI-04/UI-05：规则与统计页缺少可见装配证据

- UI-04 的 `Panel_PageRules`、`Panel_RuleSummary`、`Panel_RuleList`、`Panel_RuleEditor`、连接线/条件/动作节点
- UI-04 的启用、草稿、危险确认、加载、错误与成功状态节点
- UI-05 的 `Panel_PageStatistics`、`List_MetricCards`、`Panel_MetricDetail`、趋势图及时间范围控件
- UI-05 的指标 Normal/Danger/Anomaly 状态和空态/错误状态节点

### 5. UI-06/UI-07：HUD、对象轻量面板与影响预览不完整

- Field HUD V02 当前显式节点主要是 `Txt_HudAlert`、`Txt_HudTime`、`Txt_HudFood`、`Txt_HudWater`、`Txt_HudPower`、`Txt_HudCrew`、`Txt_HudObject`；未见与布局合同对应的 HUD 图标、状态、边框和安全区域节点
- UI-06 的对象面板标题、焦点框、等待/普通/危险状态和世界 Callout 的完整 Image/TMP 绑定未确认
- UI-07 的 `Overlay_RulesImpact`、`Panel_ImpactPreview`、范围说明、影响项列表、确认/取消按钮及其 Danger/Loading/Success 状态未确认

### 6. 状态层与交互层：未满足合同级逐节点验收

各页合同要求的 `Normal`、`Hover`、`Pressed`、`Focused`、`Disabled`，以及按场景追加的 `Loading`、`Error`、`Success`、`Danger`、`Empty` 状态，需要在 Prefab 节点或组件绑定中逐项可复核。当前静态候选未提供覆盖 8 页的完整状态节点集合，因此该门禁不能标记 PASS。

## 已有项

- 8 份布局合同文件存在且可读取。
- `BaseCommandHubForm_VisualCandidate_V01/V02.prefab`、`FieldHudForm_VisualCandidate_V01/V02.prefab` 与 `ART006_UI_VisualCandidate_V02.unity` 存在。
- V02 候选包含部分任务、对象、规则、统计、影响弹窗和 HUD 文本/列表节点，可作为后续装配基底。
- 现有 evidence 已记录 1920x1080 视觉候选与 ArtResource 资源交付，但这些记录不等同于 8 页 Prefab 节点已完成绑定。

## 回退与下一步

由 `art-2d` 补齐或明确以下内容后重新审计：公共 Form 骨架、UI-01 至 UI-07 页面容器、UI-06/UI-07 HUD 与影响预览、各页 Image/TMP 引用，以及合同要求的交互状态节点。当前报告不进行 Unity 导入、Prefab 修改或最终美术签收。

## 2026-09-04 接管复验（V04）

结论：**本报告的 FAIL 已被后续视觉装配修复；ART-006 任务 2.6 的美术视觉 Prefab 门禁 PASS，待技术验收。**

- 复验对象为 `D:\unity\UnityProject\ArtResource\Assets\Art\Authoring\ART006_UI\PrefabCandidates\VisualCandidates\BaseCommandHubForm_VisualFinal_V04.prefab` 与 `FieldHudForm_VisualFinal_V04.prefab`。二者以 V02 已验收候选作为源，保留真实 Sprite/TMP 映射并记录 V04 装配覆盖；未覆盖、重置或删除 V01/V02。
- 中枢 Prefab 复验到共享终端骨架、五页互斥容器、规则影响 Overlay、规则长按与异步反馈，以及总览／任务／对象／规则／统计页面的空、禁用、加载／长时等待、错误／开发诊断与成功视觉状态子层。
- HUD Prefab 复验到安全区、资源与时间摘要、严重告警、对象轻量面板和对象原位异步／空／错误视觉状态子层；这些保持为 `FieldHudForm` 子层，未拆成新 UIForm。
- 逐页 1920×1080 运行证据：`ART006_UI_VisualFinal_V04_1920x1080.png`（总览）、`ART006_UI_VisualFinal_V04_Tasks_1920x1080.png`、`ART006_UI_VisualFinal_V04_Objects_1920x1080.png`、`ART006_UI_VisualFinal_V04_Rules_1920x1080.png`、`ART006_UI_VisualFinal_V04_Statistics_1920x1080.png`、`ART006_UI_VisualFinal_V04_FieldHud_1920x1080.png`、`ART006_UI_VisualFinal_V04_RulesImpact_1920x1080.png`，均在 `Assets/Screenshots/` 下。
- Unity 8091 复验：`validate_missing_references=0`、`Console Error=0`、`isCompiling=false`、`isUpdating=false`。不把本美术结果误称为 GF UIForm、InputModule、生命周期或客户端状态接入完成。

## 2026-09-04 正式项目迁移复验（8090）

结论：**PASS（仅资产、Prefab 与证据迁移；待 Git 集成提交和客户端正式接线）**。

- 迁移目标为 `Assets/Game/Art/UI/ART006_UI/` 与 `Assets/Game/Prefabs/UI/ART006_UI/`；未修改 C#、GF、`ScriptsBuiltin`、任务表或 Git 索引。
- 迁移内容：201 个已验收 Sprite（及全部 `.meta`）、`SIMHEI SDF.asset` 字体（及 `.meta`）、7 张 V04 1920×1080 证据图（及 `.meta`）、10 个 Prefab（`StructurePrototype` 两个父原型及 Base/Field 的 V01–V04 完整变体链，及全部 `.meta`），另含两份候选/原型场景与说明文件。
- 初次刷新发现 V01 依赖的两个结构父 Prefab 不在 `VisualCandidates` 目录，随后从同一已验收 ART-006 `RND/StructurePrototype` 来源原样迁入；按“父原型 → V01 → V02 → V03 → V04”顺序强制重导入，未重建、扁平化或更改任一变体 GUID。
- GUID/引用复核：参与 Prefab 的 38 个 GUID 引用中，36 个由正式项目 Assets 解析（Sprite、字体及 10 个 Prefab）；其余 2 个为 Unity/TMP 内置依赖 GUID。源/目标中可迁移 `.meta` 的 SHA-256 对照为 **0 处不一致**。
- Unity 8090 最终复验：`validate_missing_references=0`、`Console Error=0`、`isCompiling=false`、`isUpdating=false`。导入时产生且只属于本次 Variant 父链缺失的 8 条历史错误已在父链修复、重导入后清除，再复验为零。
