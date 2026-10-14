# UI逐页视觉与运行状态检查报告

日期：2026-10-14。结论：**当前不能通过视觉验收。本报告只记录检查结果，没有修复正式资源、Prefab或产品代码。**

范围：18组页面族，37个可见Form、105个页面；另检查2个算法Item与1个无可视HUD路由宿主，共40个现有Prefab。发现 **435项已确认视觉/交互问题**、**1项需复核观察**、**46项业务接入缺口**。这里按受影响页面/节点计数，同一共性根因会影响多页；不是独立根因数量。

## 验收依据与边界

- 十二项标准：1. 布局与结构；2. 图片实际可见位置；3. 尺寸与比例；4. 遮挡与裁切；5. 九宫格与边角；6. 对齐间距留白；7. 文字与可读性；8. 信息层级与主操作；9. 素材与状态语义；10. 点击命中与焦点；11. 动态内容与运行状态；12. 同类页面一致性。
- 布局参照效果图，结构/资源身份以“界面”设计文档为准；素材母版安全区按“共用素材”文档核对。只查1920×1080，不扩展未实现业务，不修改原PNG。
- 实际GF生命周期取证315张主运行图（应用上下文、已有内存测试会话、无存档附着的真实初始区域各105页）；另含真实对象选择、滚动到底、机器选择和诊断模式样本。
- 500个声明状态组逐一激活并检查几何与素材，覆盖Loading/Empty/Error/Success/Disabled；它们是**状态布局检查**，不等于500次真实业务转换。每页下方明确列出实际出现和未触发的状态。
- 所有105页基础图和内存运行图逐页目检；真实区域图重点复核数据/状态改变页；状态按18族代表图和全部几何结果检查；完整证据供逐图追溯。
- 运行中只进行界面打开、翻页、滚动、选择与只读取证，没有提交部署、交易、删除、改装、保存。实际区域会话SaveCoordinator为空；检查结束退出Play Mode。
- 快速执行候选检查：本单元包含尚未冻结的视觉判定和Unity独占取证现场，主窗口自行执行，没有派发其它窗口。

## 优先处理的问题

1. 公开参数页主体运行时未激活，用户只能看到外框和返回。
2. 强确认开关滚动可见后仍没有鼠标命中目标；控制位置与确认说明不在同一分区。
3. 能源/现场详情等列表的视口仅42–58px，记录行104px，滚动也无法一次看全。
4. 100个页面的状态提示文字容器只内缩约12px，未进入母版40px安全区；机器准备页的多个状态框还越出画布。
5. 页壳与页内标题叠印，HUD警报按钮超出右边界，部分插画被卡片遮住，窄栏按钮挤压。
6. 正常目录数据触发“操作成功”；空玩家模板页遗留系统模板详情；资源选择/目录排版占用第一格或遮挡首屏。
7. 19处素材包文件身份与像素语义错位仍未解决，逐页列出实际引用。

## 状态覆盖与未覆盖项目

声明状态组的布局已经检查；17族除硬件确认外的12页仅有布局，18族的结果/进度处理为空，14族建造与世界绑定没有业务调用者。真实进度、错误恢复、保存失败、交易/升级结果、机器运行诊断等业务转换尚未全部触发。缺少业务数据源/处理器的界面，已列为接入缺口，不能以激活状态组宣布通过。键盘/手柄焦点全路径、所有缩放/分辨率、最大列表、真实最长名称和实时状态竞争仍需后续专项验证。

报告/反馈Form的操作呈现处理为空，实际显示占位字段。工坊、交易、改装、任务、知识/帮助等多处直接显示“尚未接入运行路径”；这些属于既有业务缺口，统一布局修复不能擅自补做业务系统。

设计覆盖缺失：05族RemoteMachine完整远程管理页面未实现；13族AlgorithmDiagnosis在编辑器已有诊断模式，但本次缺少可运行机器的完整诊断状态证据，不能当作独立页验收完成。其余对照与原B54覆盖矩阵见 [覆盖矩阵](../../art/coverage.json)。

## 已排除的误报

- 中枢页逻辑索引与物理索引不同：已重拍应用/内存全部6页，未把取错页面计成产品缺陷。
- 直接Open侧栏与正常OpenSub的深度不同：七个正常世界选择入口仅存在一个详情实例，关闭入口命中正常；独立打开造成的关闭遮挡已排除。
- 常驻HUD重复实例、主菜单未关闭导致的同路径射线碰撞、正常滚动边缘的半行、端口类型小徽标等，不作为已确认缺陷；剩余疑点在候选表保留。
- 算法画布网格为运行时生成，静态预览中没有网格不构成缺陷。
- Item首轮预览复用副本时调用运行时Destroy产生编辑器警告；已改为各态独立副本重拍，最终取证日志为0；该警告不计产品缺陷。
- 左上角Debugger属于开发环境，不作为正式UI布局缺陷。

## 素材语义冲突（原图保留）

| 正式身份 | 包内登记的像素身份 | 处理边界 |
|---|---|---|
| S02-002 | S02-004 | 记录缺陷；保留PNG与设计指定引用，不自动互换 |
| S02-003 | S02-005 | 记录缺陷；保留PNG与设计指定引用，不自动互换 |
| S02-004 | S02-006 | 记录缺陷；保留PNG与设计指定引用，不自动互换 |
| S02-005 | S02-002 | 记录缺陷；保留PNG与设计指定引用，不自动互换 |
| S02-006 | S02-007 | 记录缺陷；保留PNG与设计指定引用，不自动互换 |
| S02-007 | S02-008 | 记录缺陷；保留PNG与设计指定引用，不自动互换 |
| S02-008 | S02-003 | 记录缺陷；保留PNG与设计指定引用，不自动互换 |
| M007-track-thin-disabled | M007-knob-normal | 记录缺陷；保留PNG与设计指定引用，不自动互换 |
| M007-track-thin-warning | M007-knob-focus | 记录缺陷；保留PNG与设计指定引用，不自动互换 |
| M007-fill-thin | M007-track-thin-warning | 记录缺陷；保留PNG与设计指定引用，不自动互换 |
| M007-knob-normal | M007-fill-thin | 记录缺陷；保留PNG与设计指定引用，不自动互换 |
| M007-knob-focus | M007-segment-meter | 记录缺陷；保留PNG与设计指定引用，不自动互换 |
| M007-segment-meter | M007-track-thin-disabled | 记录缺陷；保留PNG与设计指定引用，不自动互换 |
| M014-resource-biomass | M014-seed-red | 记录缺陷；保留PNG与设计指定引用，不自动互换 |
| M014-energy-power | M014-resource-biomass | 记录缺陷；保留PNG与设计指定引用，不自动互换 |
| M014-seed-wheat | M014-energy-power | 记录缺陷；保留PNG与设计指定引用，不自动互换 |
| M014-seed-red | M014-crop-wheat | 记录缺陷；保留PNG与设计指定引用，不自动互换 |
| M014-crop-wheat | M014-bundle-seed-wheat | 记录缺陷；保留PNG与设计指定引用，不自动互换 |
| M014-bundle-seed-wheat | M014-seed-wheat | 记录缺陷；保留PNG与设计指定引用，不自动互换 |

## 按01→18顺序逐页清单

每页同时列出已确认问题、候选观察、业务缺口及声明状态。候选不计入已确认视觉问题数；没有发现新增问题的标准也不等于所有状态通过。

### 001 / 01 / 主菜单 · MainMenuForm/Panel_PageMainMenu

Prefab：[Assets/Game/Prefabs/UI/Startup/MainMenuForm.prefab](../../../../../Assets/Game/Prefabs/UI/Startup/MainMenuForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/MainMenuForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/MainMenuForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-01-MainMenu-occupied.png)；证据：[application](runtime/01-MainMenuForm-Panel_PageMainMenu-application.png) · [memory](runtime/01-MainMenuForm-Panel_PageMainMenu-memory.png) · [world](runtime/01-MainMenuForm-Panel_PageMainMenu-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0001, UIA-0002 |
| 5. 九宫格与边角 | 发现问题 | UIA-0001 |
| 6. 对齐间距留白 | 发现问题 | UIA-0001 |
| 7. 文字与可读性 | 发现问题 | UIA-0001, UIA-0002 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 发现问题 | UIA-0001 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0001 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0001 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `MainMenuForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/01-MainMenuForm-Panel_PageMainMenu-state-MainMenuDisabledState.json) · [证据2](geometry/01-MainMenuForm-Panel_PageMainMenu-state-MainMenuEmptyState.json) · [证据3](geometry/01-MainMenuForm-Panel_PageMainMenu-state-MainMenuErrorState.json) · [证据4](geometry/01-MainMenuForm-Panel_PageMainMenu-state-MainMenuLoadingState.json) |
| UIA-0002 / P1 / 已确认 | **TEXT_OVERLAP** · `MainMenuForm/Panel_Frame/Grp_PageHost/Panel_PageMainMenu/Txt_MainMenuTitle ↔ MainMenuForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/01-MainMenuForm-Panel_PageMainMenu-application.json) · [证据2](geometry/01-MainMenuForm-Panel_PageMainMenu-memory.json) · [证据3](geometry/01-MainMenuForm-Panel_PageMainMenu-scroll-bottom.json) · [证据4](geometry/01-MainMenuForm-Panel_PageMainMenu-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_MainMenuLoadingState](state-layout/01-MainMenuForm-Panel_PageMainMenu-MainMenuLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MainMenuEmptyState](state-layout/01-MainMenuForm-Panel_PageMainMenu-MainMenuEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MainMenuErrorState](state-layout/01-MainMenuForm-Panel_PageMainMenu-MainMenuErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MainMenuSuccessState](state-layout/01-MainMenuForm-Panel_PageMainMenu-MainMenuSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MainMenuDisabledState](state-layout/01-MainMenuForm-Panel_PageMainMenu-MainMenuDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[occupied](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-01-MainMenu-occupied.png)、[occupied](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-01.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 002 / 01 / 损坏存档恢复 · SaveRecoveryForm/Panel_PageRecovery

Prefab：[Assets/Game/Prefabs/UI/Startup/SaveRecoveryForm.prefab](../../../../../Assets/Game/Prefabs/UI/Startup/SaveRecoveryForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/SaveRecoveryForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/SaveRecoveryForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-01-Recovery-recovering.png)；证据：[application](runtime/01-SaveRecoveryForm-Panel_PageRecovery-application.png) · [memory](runtime/01-SaveRecoveryForm-Panel_PageRecovery-memory.png) · [world](runtime/01-SaveRecoveryForm-Panel_PageRecovery-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0003, UIA-0004 |
| 5. 九宫格与边角 | 发现问题 | UIA-0003 |
| 6. 对齐间距留白 | 发现问题 | UIA-0003 |
| 7. 文字与可读性 | 发现问题 | UIA-0003, UIA-0004 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 发现问题 | UIA-0003 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0003 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0003 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `SaveRecoveryForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/01-SaveRecoveryForm-Panel_PageRecovery-application.json) · [证据2](geometry/01-SaveRecoveryForm-Panel_PageRecovery-memory.json) · [证据3](geometry/01-SaveRecoveryForm-Panel_PageRecovery-scroll-bottom.json) · [证据4](geometry/01-SaveRecoveryForm-Panel_PageRecovery-scroll-top.json) |
| UIA-0004 / P1 / 已确认 | **TEXT_OVERLAP** · `SaveRecoveryForm/Panel_Frame/Grp_PageHost/Panel_PageRecovery/Txt_RecoveryTitle ↔ SaveRecoveryForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/01-SaveRecoveryForm-Panel_PageRecovery-application.json) · [证据2](geometry/01-SaveRecoveryForm-Panel_PageRecovery-memory.json) · [证据3](geometry/01-SaveRecoveryForm-Panel_PageRecovery-scroll-bottom.json) · [证据4](geometry/01-SaveRecoveryForm-Panel_PageRecovery-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_RecoveryLoadingState](state-layout/01-SaveRecoveryForm-Panel_PageRecovery-RecoveryLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_RecoveryEmptyState](state-layout/01-SaveRecoveryForm-Panel_PageRecovery-RecoveryEmptyState.png) | 已检查 | application、memory、world |
| [Grp_RecoveryErrorState](state-layout/01-SaveRecoveryForm-Panel_PageRecovery-RecoveryErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_RecoverySuccessState](state-layout/01-SaveRecoveryForm-Panel_PageRecovery-RecoverySuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_RecoveryDisabledState](state-layout/01-SaveRecoveryForm-Panel_PageRecovery-RecoveryDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[recovering](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-01-Recovery-recovering.png)、[corrupt](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-01-Recovery-corrupt.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 003 / 01 / 新建进度 · SaveSlotsForm/Panel_PageNewProgress

Prefab：[Assets/Game/Prefabs/UI/Startup/SaveSlotsForm.prefab](../../../../../Assets/Game/Prefabs/UI/Startup/SaveSlotsForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/SaveSlotsForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/SaveSlotsForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-01-NewProgress-empty.png)；证据：[application](runtime/01-SaveSlotsForm-Panel_PageNewProgress-application.png) · [memory](runtime/01-SaveSlotsForm-Panel_PageNewProgress-memory.png) · [world](runtime/01-SaveSlotsForm-Panel_PageNewProgress-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0005, UIA-0006, UIA-0007 |
| 5. 九宫格与边角 | 发现问题 | UIA-0006 |
| 6. 对齐间距留白 | 发现问题 | UIA-0005, UIA-0006 |
| 7. 文字与可读性 | 发现问题 | UIA-0006, UIA-0007 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 发现问题 | UIA-0006 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0006 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0005 / P1 / 已确认 | **CARD_ACTION_SPACING** · `Panel_NewProgressInstructions / 底部操作区`<br>右侧说明卡底边紧贴底部操作条，边框与按钮端帽相接，没有独立留白。<br>期望：满足标准 4,6：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/01-SaveSlotsForm-Panel_PageNewProgress.png) |
| UIA-0006 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `SaveSlotsForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/01-SaveSlotsForm-Panel_PageNewProgress-state-NewProgressDisabledState.json) · [证据2](geometry/01-SaveSlotsForm-Panel_PageNewProgress-state-NewProgressEmptyState.json) · [证据3](geometry/01-SaveSlotsForm-Panel_PageNewProgress-state-NewProgressErrorState.json) · [证据4](geometry/01-SaveSlotsForm-Panel_PageNewProgress-state-NewProgressLoadingState.json) |
| UIA-0007 / P1 / 已确认 | **TEXT_OVERLAP** · `SaveSlotsForm/Panel_Frame/Grp_PageHost/Panel_PageNewProgress/Txt_NewProgressTitle ↔ SaveSlotsForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/01-SaveSlotsForm-Panel_PageNewProgress-application.json) · [证据2](geometry/01-SaveSlotsForm-Panel_PageNewProgress-memory.json) · [证据3](geometry/01-SaveSlotsForm-Panel_PageNewProgress-scroll-top.json) · [证据4](geometry/01-SaveSlotsForm-Panel_PageNewProgress-state-base.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_NewProgressLoadingState](state-layout/01-SaveSlotsForm-Panel_PageNewProgress-NewProgressLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_NewProgressEmptyState](state-layout/01-SaveSlotsForm-Panel_PageNewProgress-NewProgressEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_NewProgressErrorState](state-layout/01-SaveSlotsForm-Panel_PageNewProgress-NewProgressErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_NewProgressSuccessState](state-layout/01-SaveSlotsForm-Panel_PageNewProgress-NewProgressSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_NewProgressDisabledState](state-layout/01-SaveSlotsForm-Panel_PageNewProgress-NewProgressDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[empty](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-01-NewProgress-empty.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 004 / 01 / 存档详情 · SaveSlotsForm/Panel_PageSaveDetail

Prefab：[Assets/Game/Prefabs/UI/Startup/SaveSlotsForm.prefab](../../../../../Assets/Game/Prefabs/UI/Startup/SaveSlotsForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/SaveSlotsForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/SaveSlotsForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-01-SaveDetail-occupied.png)；证据：[application](runtime/01-SaveSlotsForm-Panel_PageSaveDetail-application.png) · [memory](runtime/01-SaveSlotsForm-Panel_PageSaveDetail-memory.png) · [world](runtime/01-SaveSlotsForm-Panel_PageSaveDetail-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0009, UIA-0010 |
| 5. 九宫格与边角 | 发现问题 | UIA-0009 |
| 6. 对齐间距留白 | 发现问题 | UIA-0009 |
| 7. 文字与可读性 | 发现问题 | UIA-0009, UIA-0010 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0008 |
| 9. 素材与状态语义 | 发现问题 | UIA-0008, UIA-0009 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0008 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0009 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0008 / P1 / 已确认 | **DANGEROUS_ACTION_HIERARCHY** · `底部确认/取消/删除等按钮`<br>保留、取消、提交、删除或放弃进度采用近似同权重按钮，危险操作缺少足够醒目的差异提示，无法快速区分安全出口与执行动作。<br>期望：满足标准 8,9,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/01-SaveSlotsForm-Panel_PageSaveDetail.png) |
| UIA-0009 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `SaveSlotsForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/01-SaveSlotsForm-Panel_PageSaveDetail-application.json) · [证据2](geometry/01-SaveSlotsForm-Panel_PageSaveDetail-memory.json) · [证据3](geometry/01-SaveSlotsForm-Panel_PageSaveDetail-scroll-top.json) · [证据4](geometry/01-SaveSlotsForm-Panel_PageSaveDetail-state-SaveDetailDisabledState.json) |
| UIA-0010 / P1 / 已确认 | **TEXT_OVERLAP** · `SaveSlotsForm/Panel_Frame/Grp_PageHost/Panel_PageSaveDetail/Txt_SaveDetailTitle ↔ SaveSlotsForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/01-SaveSlotsForm-Panel_PageSaveDetail-application.json) · [证据2](geometry/01-SaveSlotsForm-Panel_PageSaveDetail-memory.json) · [证据3](geometry/01-SaveSlotsForm-Panel_PageSaveDetail-scroll-top.json) · [证据4](geometry/01-SaveSlotsForm-Panel_PageSaveDetail-state-base.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_SaveDetailLoadingState](state-layout/01-SaveSlotsForm-Panel_PageSaveDetail-SaveDetailLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_SaveDetailEmptyState](state-layout/01-SaveSlotsForm-Panel_PageSaveDetail-SaveDetailEmptyState.png) | 已检查 | application、memory、world |
| [Grp_SaveDetailErrorState](state-layout/01-SaveSlotsForm-Panel_PageSaveDetail-SaveDetailErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_SaveDetailSuccessState](state-layout/01-SaveSlotsForm-Panel_PageSaveDetail-SaveDetailSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_SaveDetailDisabledState](state-layout/01-SaveSlotsForm-Panel_PageSaveDetail-SaveDetailDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[occupied](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-01-SaveDetail-occupied.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 005 / 01 / 存档槽列表 · SaveSlotsForm/Panel_PageSaveSlots

Prefab：[Assets/Game/Prefabs/UI/Startup/SaveSlotsForm.prefab](../../../../../Assets/Game/Prefabs/UI/Startup/SaveSlotsForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/SaveSlotsForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/SaveSlotsForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-01-SaveSlots-occupied.png)；证据：[application](runtime/01-SaveSlotsForm-Panel_PageSaveSlots-application.png) · [memory](runtime/01-SaveSlotsForm-Panel_PageSaveSlots-memory.png) · [world](runtime/01-SaveSlotsForm-Panel_PageSaveSlots-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0011, UIA-0012 |
| 5. 九宫格与边角 | 发现问题 | UIA-0011 |
| 6. 对齐间距留白 | 发现问题 | UIA-0011 |
| 7. 文字与可读性 | 发现问题 | UIA-0011, UIA-0012 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 发现问题 | UIA-0011 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0011 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0011 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `SaveSlotsForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/01-SaveSlotsForm-Panel_PageSaveSlots-state-SaveSlotsDisabledState.json) · [证据2](geometry/01-SaveSlotsForm-Panel_PageSaveSlots-state-SaveSlotsErrorState.json) · [证据3](geometry/01-SaveSlotsForm-Panel_PageSaveSlots-state-SaveSlotsLoadingState.json) · [证据4](geometry/01-SaveSlotsForm-Panel_PageSaveSlots-state-SaveSlotsSuccessState.json) |
| UIA-0012 / P1 / 已确认 | **TEXT_OVERLAP** · `SaveSlotsForm/Panel_Frame/Grp_PageHost/Panel_PageSaveSlots/Txt_SaveSlotsTitle ↔ SaveSlotsForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/01-SaveSlotsForm-Panel_PageSaveSlots-application.json) · [证据2](geometry/01-SaveSlotsForm-Panel_PageSaveSlots-memory.json) · [证据3](geometry/01-SaveSlotsForm-Panel_PageSaveSlots-scroll-top.json) · [证据4](geometry/01-SaveSlotsForm-Panel_PageSaveSlots-state-base.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_SaveSlotsLoadingState](state-layout/01-SaveSlotsForm-Panel_PageSaveSlots-SaveSlotsLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_SaveSlotsEmptyState](state-layout/01-SaveSlotsForm-Panel_PageSaveSlots-SaveSlotsEmptyState.png) | 已检查 | application、memory、world |
| [Grp_SaveSlotsErrorState](state-layout/01-SaveSlotsForm-Panel_PageSaveSlots-SaveSlotsErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_SaveSlotsSuccessState](state-layout/01-SaveSlotsForm-Panel_PageSaveSlots-SaveSlotsSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_SaveSlotsDisabledState](state-layout/01-SaveSlotsForm-Panel_PageSaveSlots-SaveSlotsDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[occupied](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-01-SaveSlots-occupied.png)、[empty](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-01-SaveSlots-empty.png)、[corrupt](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-01-SaveSlots-corrupt.png)、[recovering](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-01-SaveSlots-recovering.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 006 / 02 / 保存返回与退出失败处理 · ExitFlowForm/Panel_PageExitFlow

Prefab：[Assets/Game/Prefabs/UI/System/ExitFlowForm.prefab](../../../../../Assets/Game/Prefabs/UI/System/ExitFlowForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/ExitFlowForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/ExitFlowForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-02-ExitFlow-confirm.png)；证据：[application](runtime/02-ExitFlowForm-Panel_PageExitFlow-application.png) · [memory](runtime/02-ExitFlowForm-Panel_PageExitFlow-memory.png) · [world](runtime/02-ExitFlowForm-Panel_PageExitFlow-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0013, UIA-0014 |
| 5. 九宫格与边角 | 发现问题 | UIA-0013 |
| 6. 对齐间距留白 | 发现问题 | UIA-0013 |
| 7. 文字与可读性 | 发现问题 | UIA-0013, UIA-0014 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 发现问题 | UIA-0013 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0013 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0013 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `ExitFlowForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/02-ExitFlowForm-Panel_PageExitFlow-application.json) · [证据2](geometry/02-ExitFlowForm-Panel_PageExitFlow-memory.json) · [证据3](geometry/02-ExitFlowForm-Panel_PageExitFlow-scroll-bottom.json) · [证据4](geometry/02-ExitFlowForm-Panel_PageExitFlow-scroll-top.json) |
| UIA-0014 / P1 / 已确认 | **TEXT_OVERLAP** · `ExitFlowForm/Panel_Frame/Grp_PageHost/Panel_PageExitFlow/Txt_ExitFlowTitle ↔ ExitFlowForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/02-ExitFlowForm-Panel_PageExitFlow-application.json) · [证据2](geometry/02-ExitFlowForm-Panel_PageExitFlow-memory.json) · [证据3](geometry/02-ExitFlowForm-Panel_PageExitFlow-scroll-bottom.json) · [证据4](geometry/02-ExitFlowForm-Panel_PageExitFlow-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_ExitFlowLoadingState](state-layout/02-ExitFlowForm-Panel_PageExitFlow-ExitFlowLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ExitFlowEmptyState](state-layout/02-ExitFlowForm-Panel_PageExitFlow-ExitFlowEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ExitFlowErrorState](state-layout/02-ExitFlowForm-Panel_PageExitFlow-ExitFlowErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ExitFlowSuccessState](state-layout/02-ExitFlowForm-Panel_PageExitFlow-ExitFlowSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ExitFlowDisabledState](state-layout/02-ExitFlowForm-Panel_PageExitFlow-ExitFlowDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[confirm](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-02-ExitFlow-confirm.png)、[unsaved](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-02-ExitFlow-unsaved.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 007 / 02 / 声音设置 · SettingsForm/Panel_PageAudioSettings

Prefab：[Assets/Game/Prefabs/UI/System/SettingsForm.prefab](../../../../../Assets/Game/Prefabs/UI/System/SettingsForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/SettingsForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/SettingsForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-02-AudioSettings-default.png)；证据：[application](runtime/02-SettingsForm-Panel_PageAudioSettings-application.png) · [memory](runtime/02-SettingsForm-Panel_PageAudioSettings-memory.png) · [world](runtime/02-SettingsForm-Panel_PageAudioSettings-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0017, UIA-0018 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0017, UIA-0019, UIA-0020 |
| 5. 九宫格与边角 | 发现问题 | UIA-0018, UIA-0019 |
| 6. 对齐间距留白 | 发现问题 | UIA-0019 |
| 7. 文字与可读性 | 发现问题 | UIA-0017, UIA-0019, UIA-0020 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 发现问题 | UIA-0015, UIA-0016, UIA-0018, UIA-0019 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0017, UIA-0018 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0017, UIA-0019 |
| 12. 同类页面一致性 | 发现问题 | UIA-0015, UIA-0016 |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0015 / P1 / 已确认 | **ASSET_SEMANTIC_CONFLICT** · `SettingsForm/Panel_Frame/Grp_PageHost/Panel_PageAudioSettings/Panel_AudioSettingsVolumes/List_AudioSettingsVolumes/Viewport_AudioSettingsVolumes/Content_AudioSettingsVolumes/Panel_AudioSettingsControls/Sld_AudioSettingsAmbientVolume/Grp_AudioSettingsAmbientVolumeFillArea/Bar_AudioSettingsAmbientVolumeFill / SettingsForm/Panel_Frame/Grp_PageHost/Panel_PageAudioSettings/Panel_AudioSettingsVolumes/List_AudioSettingsVolumes/Viewport_AudioSettingsVolumes/Content_AudioSettingsVolumes/Panel_AudioSettingsControls/Sld_AudioSettingsMachineVolume/Grp_AudioSettingsMachineVolumeFillArea/Bar_AudioSettingsMachineVolumeFill / SettingsForm/Panel_Frame/Grp_PageHost/Panel_PageAudioSettings/Panel_AudioSettingsVolumes/List_AudioSettingsVolumes/Viewport_AudioSettingsVolumes/Content_AudioSettingsVolumes/Panel_AudioSettingsControls/Sld_AudioSettingsMainVolume/Grp_AudioSettingsMainVolumeFillArea/Bar_AudioSettingsMainVolumeFill / SettingsForm/Panel_Frame/Grp_PageHost/Panel_PageAudioSettings/Panel_AudioSettingsVolumes/List_AudioSettingsVolumes/Viewport_AudioSettingsVolumes/Content_AudioSettingsVolumes/Panel_AudioSettingsControls/Sld_AudioSettingsMusicVolume/Grp_AudioSettingsMusicVolumeFillArea/Bar_AudioSettingsMusicVolumeFill / SettingsForm/Panel_Frame/Grp_PageHost/Panel_PageAudioSettings/Panel_AudioSettingsVolumes/List_AudioSettingsVolumes/Viewport_AudioSettingsVolumes/Content_AudioSettingsVolumes/Panel_AudioSettingsControls/Sld_AudioSettingsUiVolume/Grp_AudioSettingsUiVolumeFillArea/Bar_AudioSettingsUiVolumeFill`<br>正式文件 M007-fill-thin 的像素内容被素材包登记为 M007-track-thin-warning；遵守文件身份并不能消除实际图形语义错位。保留原PNG，资源互换仍不纳入本轮自动修复。<br>期望：满足标准 9,12：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/02-SettingsForm-Panel_PageAudioSettings-application.json) · [证据2](geometry/02-SettingsForm-Panel_PageAudioSettings-memory.json) · [证据3](geometry/02-SettingsForm-Panel_PageAudioSettings-scroll-bottom.json) · [证据4](geometry/02-SettingsForm-Panel_PageAudioSettings-scroll-top.json) |
| UIA-0016 / P1 / 已确认 | **ASSET_SEMANTIC_CONFLICT** · `SettingsForm/Panel_Frame/Grp_PageHost/Panel_PageAudioSettings/Panel_AudioSettingsVolumes/List_AudioSettingsVolumes/Viewport_AudioSettingsVolumes/Content_AudioSettingsVolumes/Panel_AudioSettingsControls/Sld_AudioSettingsAmbientVolume/Grp_AudioSettingsAmbientVolumeHandleArea/Img_AudioSettingsAmbientVolumeHandle / SettingsForm/Panel_Frame/Grp_PageHost/Panel_PageAudioSettings/Panel_AudioSettingsVolumes/List_AudioSettingsVolumes/Viewport_AudioSettingsVolumes/Content_AudioSettingsVolumes/Panel_AudioSettingsControls/Sld_AudioSettingsMachineVolume/Grp_AudioSettingsMachineVolumeHandleArea/Img_AudioSettingsMachineVolumeHandle / SettingsForm/Panel_Frame/Grp_PageHost/Panel_PageAudioSettings/Panel_AudioSettingsVolumes/List_AudioSettingsVolumes/Viewport_AudioSettingsVolumes/Content_AudioSettingsVolumes/Panel_AudioSettingsControls/Sld_AudioSettingsMainVolume/Grp_AudioSettingsMainVolumeHandleArea/Img_AudioSettingsMainVolumeHandle / SettingsForm/Panel_Frame/Grp_PageHost/Panel_PageAudioSettings/Panel_AudioSettingsVolumes/List_AudioSettingsVolumes/Viewport_AudioSettingsVolumes/Content_AudioSettingsVolumes/Panel_AudioSettingsControls/Sld_AudioSettingsMusicVolume/Grp_AudioSettingsMusicVolumeHandleArea/Img_AudioSettingsMusicVolumeHandle / SettingsForm/Panel_Frame/Grp_PageHost/Panel_PageAudioSettings/Panel_AudioSettingsVolumes/List_AudioSettingsVolumes/Viewport_AudioSettingsVolumes/Content_AudioSettingsVolumes/Panel_AudioSettingsControls/Sld_AudioSettingsUiVolume/Grp_AudioSettingsUiVolumeHandleArea/Img_AudioSettingsUiVolumeHandle`<br>正式文件 M007-knob-normal 的像素内容被素材包登记为 M007-fill-thin；遵守文件身份并不能消除实际图形语义错位。保留原PNG，资源互换仍不纳入本轮自动修复。<br>期望：满足标准 9,12：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/02-SettingsForm-Panel_PageAudioSettings-application.json) · [证据2](geometry/02-SettingsForm-Panel_PageAudioSettings-memory.json) · [证据3](geometry/02-SettingsForm-Panel_PageAudioSettings-scroll-bottom.json) · [证据4](geometry/02-SettingsForm-Panel_PageAudioSettings-scroll-top.json) |
| UIA-0017 / P1 / 已确认 | **SCROLL_ROW_TOO_TALL** · `SettingsForm/Panel_Frame/Grp_PageHost/Panel_PageAudioSettings/Panel_AudioSettingsPersistence/List_AudioSettingsPersistence/Viewport_AudioSettingsPersistence/Content_AudioSettingsPersistence/Item_AudioSettingsPersistenceTemplate/Btn_AudioSettingsPersistenceRow`<br>单条记录高 104.0px，大于列表可视高度 58.0px；无法完整显示一条记录<br>期望：满足标准 3,4,7,10,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/02-SettingsForm-Panel_PageAudioSettings-scroll-bottom.json) · [证据2](geometry/02-SettingsForm-Panel_PageAudioSettings-scroll-top.json) · [证据3](geometry/02-SettingsForm-Panel_PageAudioSettings-world-scroll-bottom.json) · [证据4](geometry/02-SettingsForm-Panel_PageAudioSettings-world-scroll-top.json) |
| UIA-0018 / P1 / 已确认 | **SLIDER_AFFORDANCE** · `Sld_AudioSettings*`<br>五条音量轨道在 1080p 下近似细线，把手和填充不易辨识；当前引用的 knob/fill 源图存在已登记语义错位。<br>期望：满足标准 3,5,9,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/02-SettingsForm-Panel_PageAudioSettings.png) |
| UIA-0019 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `SettingsForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/02-SettingsForm-Panel_PageAudioSettings-state-AudioSettingsDisabledState.json) · [证据2](geometry/02-SettingsForm-Panel_PageAudioSettings-state-AudioSettingsEmptyState.json) · [证据3](geometry/02-SettingsForm-Panel_PageAudioSettings-state-AudioSettingsErrorState.json) · [证据4](geometry/02-SettingsForm-Panel_PageAudioSettings-state-AudioSettingsLoadingState.json) |
| UIA-0020 / P1 / 已确认 | **TEXT_OVERLAP** · `SettingsForm/Panel_Frame/Grp_PageHost/Panel_PageAudioSettings/Txt_AudioSettingsTitle ↔ SettingsForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/02-SettingsForm-Panel_PageAudioSettings-application.json) · [证据2](geometry/02-SettingsForm-Panel_PageAudioSettings-memory.json) · [证据3](geometry/02-SettingsForm-Panel_PageAudioSettings-scroll-bottom.json) · [证据4](geometry/02-SettingsForm-Panel_PageAudioSettings-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_AudioSettingsLoadingState](state-layout/02-SettingsForm-Panel_PageAudioSettings-AudioSettingsLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_AudioSettingsEmptyState](state-layout/02-SettingsForm-Panel_PageAudioSettings-AudioSettingsEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_AudioSettingsErrorState](state-layout/02-SettingsForm-Panel_PageAudioSettings-AudioSettingsErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_AudioSettingsSuccessState](state-layout/02-SettingsForm-Panel_PageAudioSettings-AudioSettingsSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_AudioSettingsDisabledState](state-layout/02-SettingsForm-Panel_PageAudioSettings-AudioSettingsDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[default](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-02-AudioSettings-default.png)、[modified](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-02-AudioSettings-modified.png)、[reset](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-02-AudioSettings-reset.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 008 / 02 / 操作设置 · SettingsForm/Panel_PageControlSettings

Prefab：[Assets/Game/Prefabs/UI/System/SettingsForm.prefab](../../../../../Assets/Game/Prefabs/UI/System/SettingsForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/SettingsForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/SettingsForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-02-ControlSettings-default.png)；证据：[application](runtime/02-SettingsForm-Panel_PageControlSettings-application.png) · [memory](runtime/02-SettingsForm-Panel_PageControlSettings-memory.png) · [world](runtime/02-SettingsForm-Panel_PageControlSettings-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0023, UIA-0024 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0023, UIA-0024, UIA-0025, UIA-0026 |
| 5. 九宫格与边角 | 发现问题 | UIA-0023, UIA-0025 |
| 6. 对齐间距留白 | 发现问题 | UIA-0025 |
| 7. 文字与可读性 | 发现问题 | UIA-0023, UIA-0024, UIA-0025, UIA-0026 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 发现问题 | UIA-0021, UIA-0022, UIA-0025 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0024 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0023, UIA-0024, UIA-0025 |
| 12. 同类页面一致性 | 发现问题 | UIA-0021, UIA-0022 |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0021 / P1 / 已确认 | **ASSET_SEMANTIC_CONFLICT** · `SettingsForm/Panel_Frame/Grp_PageHost/Panel_PageControlSettings/Panel_ControlSettingsCamera/List_ControlSettingsCamera/Viewport_ControlSettingsCamera/Content_ControlSettingsCamera/Panel_ControlSettingsControls/Sld_ControlSettingsPanSpeed/Grp_ControlSettingsPanSpeedFillArea/Bar_ControlSettingsPanSpeedFill / SettingsForm/Panel_Frame/Grp_PageHost/Panel_PageControlSettings/Panel_ControlSettingsCamera/List_ControlSettingsCamera/Viewport_ControlSettingsCamera/Content_ControlSettingsCamera/Panel_ControlSettingsControls/Sld_ControlSettingsRotateSpeed/Grp_ControlSettingsRotateSpeedFillArea/Bar_ControlSettingsRotateSpeedFill / SettingsForm/Panel_Frame/Grp_PageHost/Panel_PageControlSettings/Panel_ControlSettingsCamera/List_ControlSettingsCamera/Viewport_ControlSettingsCamera/Content_ControlSettingsCamera/Panel_ControlSettingsControls/Sld_ControlSettingsZoomSpeed/Grp_ControlSettingsZoomSpeedFillArea/Bar_ControlSettingsZoomSpeedFill`<br>正式文件 M007-fill-thin 的像素内容被素材包登记为 M007-track-thin-warning；遵守文件身份并不能消除实际图形语义错位。保留原PNG，资源互换仍不纳入本轮自动修复。<br>期望：满足标准 9,12：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/02-SettingsForm-Panel_PageControlSettings-application.json) · [证据2](geometry/02-SettingsForm-Panel_PageControlSettings-memory.json) · [证据3](geometry/02-SettingsForm-Panel_PageControlSettings-scroll-bottom.json) · [证据4](geometry/02-SettingsForm-Panel_PageControlSettings-scroll-top.json) |
| UIA-0022 / P1 / 已确认 | **ASSET_SEMANTIC_CONFLICT** · `SettingsForm/Panel_Frame/Grp_PageHost/Panel_PageControlSettings/Panel_ControlSettingsCamera/List_ControlSettingsCamera/Viewport_ControlSettingsCamera/Content_ControlSettingsCamera/Panel_ControlSettingsControls/Sld_ControlSettingsPanSpeed/Grp_ControlSettingsPanSpeedHandleArea/Img_ControlSettingsPanSpeedHandle / SettingsForm/Panel_Frame/Grp_PageHost/Panel_PageControlSettings/Panel_ControlSettingsCamera/List_ControlSettingsCamera/Viewport_ControlSettingsCamera/Content_ControlSettingsCamera/Panel_ControlSettingsControls/Sld_ControlSettingsRotateSpeed/Grp_ControlSettingsRotateSpeedHandleArea/Img_ControlSettingsRotateSpeedHandle / SettingsForm/Panel_Frame/Grp_PageHost/Panel_PageControlSettings/Panel_ControlSettingsCamera/List_ControlSettingsCamera/Viewport_ControlSettingsCamera/Content_ControlSettingsCamera/Panel_ControlSettingsControls/Sld_ControlSettingsZoomSpeed/Grp_ControlSettingsZoomSpeedHandleArea/Img_ControlSettingsZoomSpeedHandle`<br>正式文件 M007-knob-normal 的像素内容被素材包登记为 M007-fill-thin；遵守文件身份并不能消除实际图形语义错位。保留原PNG，资源互换仍不纳入本轮自动修复。<br>期望：满足标准 9,12：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/02-SettingsForm-Panel_PageControlSettings-application.json) · [证据2](geometry/02-SettingsForm-Panel_PageControlSettings-memory.json) · [证据3](geometry/02-SettingsForm-Panel_PageControlSettings-scroll-bottom.json) · [证据4](geometry/02-SettingsForm-Panel_PageControlSettings-scroll-top.json) |
| UIA-0023 / P1 / 已确认 | **CONTROL_SLIDER_LAYOUT** · `Panel_ControlSettingsControls / Sld_*`<br>操作参数滑条将整块行底图拉成控件背景，边框压住数值文字；后续滑条被容器裁切。<br>期望：满足标准 3,4,5,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/02-SettingsForm-Panel_PageControlSettings.png) |
| UIA-0024 / P1 / 已确认 | **SCROLL_ROW_TOO_TALL** · `SettingsForm/Panel_Frame/Grp_PageHost/Panel_PageControlSettings/Panel_ControlSettingsBindings/List_ControlSettingsBindings/Viewport_ControlSettingsBindings/Content_ControlSettingsBindings/Item_ControlSettingsBindingsTemplate/Btn_ControlSettingsBindingsRow`<br>单条记录高 104.0px，大于列表可视高度 58.0px；无法完整显示一条记录<br>期望：满足标准 3,4,7,10,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/02-SettingsForm-Panel_PageControlSettings-world-scroll-bottom.json) · [证据2](geometry/02-SettingsForm-Panel_PageControlSettings-world-scroll-top.json) · [证据3](geometry/02-SettingsForm-Panel_PageControlSettings-world.json) |
| UIA-0025 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `SettingsForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/02-SettingsForm-Panel_PageControlSettings-state-ControlSettingsDisabledState.json) · [证据2](geometry/02-SettingsForm-Panel_PageControlSettings-state-ControlSettingsEmptyState.json) · [证据3](geometry/02-SettingsForm-Panel_PageControlSettings-state-ControlSettingsErrorState.json) · [证据4](geometry/02-SettingsForm-Panel_PageControlSettings-state-ControlSettingsLoadingState.json) |
| UIA-0026 / P1 / 已确认 | **TEXT_OVERLAP** · `SettingsForm/Panel_Frame/Grp_PageHost/Panel_PageControlSettings/Txt_ControlSettingsTitle ↔ SettingsForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/02-SettingsForm-Panel_PageControlSettings-application.json) · [证据2](geometry/02-SettingsForm-Panel_PageControlSettings-memory.json) · [证据3](geometry/02-SettingsForm-Panel_PageControlSettings-scroll-bottom.json) · [证据4](geometry/02-SettingsForm-Panel_PageControlSettings-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_ControlSettingsLoadingState](state-layout/02-SettingsForm-Panel_PageControlSettings-ControlSettingsLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ControlSettingsEmptyState](state-layout/02-SettingsForm-Panel_PageControlSettings-ControlSettingsEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ControlSettingsErrorState](state-layout/02-SettingsForm-Panel_PageControlSettings-ControlSettingsErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ControlSettingsSuccessState](state-layout/02-SettingsForm-Panel_PageControlSettings-ControlSettingsSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ControlSettingsDisabledState](state-layout/02-SettingsForm-Panel_PageControlSettings-ControlSettingsDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[default](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-02-ControlSettings-default.png)、[modified](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-02-ControlSettings-modified.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 009 / 02 / 显示与性能设置 · SettingsForm/Panel_PageDisplaySettings

Prefab：[Assets/Game/Prefabs/UI/System/SettingsForm.prefab](../../../../../Assets/Game/Prefabs/UI/System/SettingsForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/SettingsForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/SettingsForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-02-DisplaySettings-standard.png)；证据：[application](runtime/02-SettingsForm-Panel_PageDisplaySettings-application.png) · [memory](runtime/02-SettingsForm-Panel_PageDisplaySettings-memory.png) · [world](runtime/02-SettingsForm-Panel_PageDisplaySettings-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0028, UIA-0029 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0028, UIA-0030, UIA-0031, UIA-0032, UIA-0033, UIA-0034 |
| 5. 九宫格与边角 | 发现问题 | UIA-0030 |
| 6. 对齐间距留白 | 发现问题 | UIA-0030 |
| 7. 文字与可读性 | 发现问题 | UIA-0028, UIA-0030, UIA-0031, UIA-0032, UIA-0033, UIA-0034 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 发现问题 | UIA-0027, UIA-0029, UIA-0030 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0028 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0030, UIA-0031, UIA-0032, UIA-0033 |
| 12. 同类页面一致性 | 发现问题 | UIA-0027 |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0027 / P1 / 已确认 | **ASSET_SEMANTIC_CONFLICT** · `SettingsForm/Panel_Frame/Grp_PageHost/Panel_PageDisplaySettings/Panel_DisplaySettingsQuality/Img_FormalDisplaySettings_Art00`<br>正式文件 S02-005 的像素内容被素材包登记为 S02-002；遵守文件身份并不能消除实际图形语义错位。保留原PNG，资源互换仍不纳入本轮自动修复。<br>期望：满足标准 9,12：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/02-SettingsForm-Panel_PageDisplaySettings-application.json) · [证据2](geometry/02-SettingsForm-Panel_PageDisplaySettings-memory.json) · [证据3](geometry/02-SettingsForm-Panel_PageDisplaySettings-scroll-bottom.json) · [证据4](geometry/02-SettingsForm-Panel_PageDisplaySettings-scroll-top.json) |
| UIA-0028 / P1 / 已确认 | **DISPLAY_CONTROL_LAYOUT** · `Panel_DisplaySettingsMode / Toggle 控件`<br>显示模式控件重复呈现细长装饰条，标签和勾选位置不清楚，底部控件在列表边界处被截断。<br>期望：满足标准 3,4,7,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/02-SettingsForm-Panel_PageDisplaySettings.png) |
| UIA-0029 / P1 / 已确认 | **DISPLAY_WRONG_PREVIEW** · `显示设置右侧预览图`<br>显示预览位置出现操作方向盘图案，无法表达标准/降低显示效果；对应 S02 素材语义错位须保留为缺陷。<br>期望：满足标准 3,9：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/02-SettingsForm-Panel_PageDisplaySettings.png) |
| UIA-0030 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `SettingsForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/02-SettingsForm-Panel_PageDisplaySettings-state-DisplaySettingsDisabledState.json) · [证据2](geometry/02-SettingsForm-Panel_PageDisplaySettings-state-DisplaySettingsEmptyState.json) · [证据3](geometry/02-SettingsForm-Panel_PageDisplaySettings-state-DisplaySettingsErrorState.json) · [证据4](geometry/02-SettingsForm-Panel_PageDisplaySettings-state-DisplaySettingsLoadingState.json) |
| UIA-0031 / P1 / 已确认 | **TEXT_MASK_CLIP** · `SettingsForm/Panel_Frame/Grp_PageHost/Panel_PageDisplaySettings/Panel_DisplaySettingsDisplay/List_DisplaySettingsDisplay/Viewport_DisplaySettingsDisplay/Content_DisplaySettingsDisplay/Item_DisplaySettingsDisplayTemplate/Btn_DisplaySettingsDisplayRow/Txt_DisplaySettingsDisplayRowValue`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/02-SettingsForm-Panel_PageDisplaySettings-scroll-bottom.json) · [证据2](geometry/02-SettingsForm-Panel_PageDisplaySettings-world-scroll-bottom.json) |
| UIA-0032 / P1 / 已确认 | **TEXT_MASK_CLIP** · `SettingsForm/Panel_Frame/Grp_PageHost/Panel_PageDisplaySettings/Panel_DisplaySettingsDisplay/List_DisplaySettingsDisplay/Viewport_DisplaySettingsDisplay/Content_DisplaySettingsDisplay/Txt_DisplaySettingsDisplayBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/02-SettingsForm-Panel_PageDisplaySettings-application.json) · [证据2](geometry/02-SettingsForm-Panel_PageDisplaySettings-memory.json) · [证据3](geometry/02-SettingsForm-Panel_PageDisplaySettings-scroll-top.json) · [证据4](geometry/02-SettingsForm-Panel_PageDisplaySettings-world-scroll-top.json) |
| UIA-0033 / P1 / 已确认 | **TEXT_MASK_CLIP** · `SettingsForm/Panel_Frame/Grp_PageHost/Panel_PageDisplaySettings/Panel_DisplaySettingsQuality/List_DisplaySettingsQuality/Viewport_DisplaySettingsQuality/Content_DisplaySettingsQuality/Txt_DisplaySettingsQualityBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/02-SettingsForm-Panel_PageDisplaySettings-application.json) · [证据2](geometry/02-SettingsForm-Panel_PageDisplaySettings-memory.json) · [证据3](geometry/02-SettingsForm-Panel_PageDisplaySettings-scroll-top.json) · [证据4](geometry/02-SettingsForm-Panel_PageDisplaySettings-world-scroll-top.json) |
| UIA-0034 / P1 / 已确认 | **TEXT_OVERLAP** · `SettingsForm/Panel_Frame/Grp_PageHost/Panel_PageDisplaySettings/Txt_DisplaySettingsTitle ↔ SettingsForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/02-SettingsForm-Panel_PageDisplaySettings-application.json) · [证据2](geometry/02-SettingsForm-Panel_PageDisplaySettings-memory.json) · [证据3](geometry/02-SettingsForm-Panel_PageDisplaySettings-scroll-bottom.json) · [证据4](geometry/02-SettingsForm-Panel_PageDisplaySettings-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_DisplaySettingsLoadingState](state-layout/02-SettingsForm-Panel_PageDisplaySettings-DisplaySettingsLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_DisplaySettingsEmptyState](state-layout/02-SettingsForm-Panel_PageDisplaySettings-DisplaySettingsEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_DisplaySettingsErrorState](state-layout/02-SettingsForm-Panel_PageDisplaySettings-DisplaySettingsErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_DisplaySettingsSuccessState](state-layout/02-SettingsForm-Panel_PageDisplaySettings-DisplaySettingsSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_DisplaySettingsDisabledState](state-layout/02-SettingsForm-Panel_PageDisplaySettings-DisplaySettingsDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[standard](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-02-DisplaySettings-standard.png)、[reduced](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-02-DisplaySettings-reduced.png)、[modified](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-02-DisplaySettings-modified.png)、[standard](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-02.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 010 / 02 / 游戏内系统菜单 · SystemMenuForm/Panel_PageSystemMenu

Prefab：[Assets/Game/Prefabs/UI/System/SystemMenuForm.prefab](../../../../../Assets/Game/Prefabs/UI/System/SystemMenuForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/SystemMenuForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/SystemMenuForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-02-SystemMenu-default.png)；证据：[application](runtime/02-SystemMenuForm-Panel_PageSystemMenu-application.png) · [memory](runtime/02-SystemMenuForm-Panel_PageSystemMenu-memory.png) · [world](runtime/02-SystemMenuForm-Panel_PageSystemMenu-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0035, UIA-0036 |
| 5. 九宫格与边角 | 发现问题 | UIA-0035 |
| 6. 对齐间距留白 | 发现问题 | UIA-0035 |
| 7. 文字与可读性 | 发现问题 | UIA-0035, UIA-0036 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 发现问题 | UIA-0035 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0035 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0035 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `SystemMenuForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/02-SystemMenuForm-Panel_PageSystemMenu-state-SystemMenuDisabledState.json) · [证据2](geometry/02-SystemMenuForm-Panel_PageSystemMenu-state-SystemMenuEmptyState.json) · [证据3](geometry/02-SystemMenuForm-Panel_PageSystemMenu-state-SystemMenuErrorState.json) · [证据4](geometry/02-SystemMenuForm-Panel_PageSystemMenu-state-SystemMenuLoadingState.json) |
| UIA-0036 / P1 / 已确认 | **TEXT_OVERLAP** · `SystemMenuForm/Panel_Frame/Grp_PageHost/Panel_PageSystemMenu/Txt_SystemMenuTitle ↔ SystemMenuForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/02-SystemMenuForm-Panel_PageSystemMenu-application.json) · [证据2](geometry/02-SystemMenuForm-Panel_PageSystemMenu-memory.json) · [证据3](geometry/02-SystemMenuForm-Panel_PageSystemMenu-scroll-bottom.json) · [证据4](geometry/02-SystemMenuForm-Panel_PageSystemMenu-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_SystemMenuLoadingState](state-layout/02-SystemMenuForm-Panel_PageSystemMenu-SystemMenuLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_SystemMenuEmptyState](state-layout/02-SystemMenuForm-Panel_PageSystemMenu-SystemMenuEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_SystemMenuErrorState](state-layout/02-SystemMenuForm-Panel_PageSystemMenu-SystemMenuErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_SystemMenuSuccessState](state-layout/02-SystemMenuForm-Panel_PageSystemMenu-SystemMenuSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_SystemMenuDisabledState](state-layout/02-SystemMenuForm-Panel_PageSystemMenu-SystemMenuDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[default](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-02-SystemMenu-default.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 011 / 03 / HudAlerts · FieldHudResidentForm/Panel_PageHudAlerts

Prefab：[Assets/Game/Prefabs/UI/Hud/FieldHudResidentForm.prefab](../../../../../Assets/Game/Prefabs/UI/Hud/FieldHudResidentForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/FieldHudResidentForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/FieldHudResidentForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-03-HudAlerts-warning.png)；证据：[application](runtime/03-FieldHudResidentForm-Panel_PageHudAlerts-application.png) · [memory](runtime/03-FieldHudResidentForm-Panel_PageHudAlerts-memory.png) · [world](runtime/03-FieldHudResidentForm-Panel_PageHudAlerts-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 发现问题 | UIA-0041, UIA-0042, UIA-0043, UIA-0044 |
| 3. 尺寸与比例 | 发现问题 | UIA-0037 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0037, UIA-0038, UIA-0039, UIA-0040, UIA-0041, UIA-0042, UIA-0043, UIA-0044 |
| 5. 九宫格与边角 | 发现问题 | UIA-0037 |
| 6. 对齐间距留白 | 发现问题 | UIA-0038, UIA-0039, UIA-0040 |
| 7. 文字与可读性 | 发现问题 | UIA-0038, UIA-0039, UIA-0040 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 当前样本未发现新增问题 |  |
| 10. 点击命中与焦点 | 发现问题 | UIA-0037 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0041, UIA-0042, UIA-0043, UIA-0044 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0037 / P1 / 已确认 | **HUD_ACTION_COMPRESSION** · `HUD 右侧任务与警报按钮`<br>任务、引导、定位及警报入口挤在卡片底沿/右沿，按钮尺寸与边角比例失衡，文字靠近端帽。<br>期望：满足标准 3,4,5,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/03-FieldHudResidentForm-Panel_PageHudAlerts.png) |
| UIA-0038 / P1 / 已确认 | **HUD_EDGE_PADDING** · `HUD 顶部状态/任务/警报区域`<br>资源、保存、任务和警报文字紧贴装饰边缘，部分文字与上边框相交；顶栏和右侧卡片的内边距不足。<br>期望：满足标准 4,6,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/03-FieldHudResidentForm-Panel_PageHudAlerts.png) |
| UIA-0039 / P1 / 已确认 | **SAFE_MARGIN_TEXT** · `FieldHudResidentForm/Grp_PageHost/Panel_PageHudAlerts/Grp_HudAlertsActions/Btn_HudAlertsLocate/Txt_HudAlertsLocateLabel`<br>文字实际轮廓进入 24px 外边距<br>期望：满足标准 4,6,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/03-FieldHudResidentForm-Panel_PageHudAlerts-application.json) · [证据2](geometry/03-FieldHudResidentForm-Panel_PageHudAlerts-memory.json) · [证据3](geometry/03-FieldHudResidentForm-Panel_PageHudAlerts-scroll-top.json) · [证据4](geometry/03-FieldHudResidentForm-Panel_PageHudAlerts-state-base.json) |
| UIA-0040 / P1 / 已确认 | **SAFE_MARGIN_TEXT** · `FieldHudResidentForm/Grp_PageHost/Panel_PageHudAlerts/Grp_HudAlertsActions/Btn_HudAlertsOpen/Txt_HudAlertsOpenLabel`<br>文字实际轮廓进入 24px 外边距<br>期望：满足标准 4,6,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/03-FieldHudResidentForm-Panel_PageHudAlerts-application.json) · [证据2](geometry/03-FieldHudResidentForm-Panel_PageHudAlerts-memory.json) · [证据3](geometry/03-FieldHudResidentForm-Panel_PageHudAlerts-scroll-top.json) · [证据4](geometry/03-FieldHudResidentForm-Panel_PageHudAlerts-state-base.json) |
| UIA-0041 / P1 / 已确认 | **SCREEN_OVERFLOW** · `FieldHudResidentForm/Grp_PageHost/Panel_PageHudAlerts/Grp_HudAlertsActions/Btn_HudAlertsLocate`<br>节点矩形超出 1920×1080 画布<br>期望：满足标准 2,4,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/03-FieldHudResidentForm-Panel_PageHudAlerts-application.json) · [证据2](geometry/03-FieldHudResidentForm-Panel_PageHudAlerts-memory.json) · [证据3](geometry/03-FieldHudResidentForm-Panel_PageHudAlerts-scroll-top.json) · [证据4](geometry/03-FieldHudResidentForm-Panel_PageHudAlerts-state-base.json) |
| UIA-0042 / P1 / 已确认 | **SCREEN_OVERFLOW** · `FieldHudResidentForm/Grp_PageHost/Panel_PageHudAlerts/Grp_HudAlertsActions/Btn_HudAlertsLocate/Txt_HudAlertsLocateLabel`<br>节点矩形超出 1920×1080 画布<br>期望：满足标准 2,4,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/03-FieldHudResidentForm-Panel_PageHudAlerts-application.json) · [证据2](geometry/03-FieldHudResidentForm-Panel_PageHudAlerts-memory.json) · [证据3](geometry/03-FieldHudResidentForm-Panel_PageHudAlerts-scroll-top.json) · [证据4](geometry/03-FieldHudResidentForm-Panel_PageHudAlerts-state-base.json) |
| UIA-0043 / P1 / 已确认 | **SCREEN_OVERFLOW** · `FieldHudResidentForm/Grp_PageHost/Panel_PageHudAlerts/Grp_HudAlertsActions/Btn_HudAlertsOpen`<br>节点矩形超出 1920×1080 画布<br>期望：满足标准 2,4,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/03-FieldHudResidentForm-Panel_PageHudAlerts-application.json) · [证据2](geometry/03-FieldHudResidentForm-Panel_PageHudAlerts-memory.json) · [证据3](geometry/03-FieldHudResidentForm-Panel_PageHudAlerts-scroll-top.json) · [证据4](geometry/03-FieldHudResidentForm-Panel_PageHudAlerts-state-base.json) |
| UIA-0044 / P1 / 已确认 | **SCREEN_OVERFLOW** · `FieldHudResidentForm/Grp_PageHost/Panel_PageHudAlerts/Grp_HudAlertsActions/Btn_HudAlertsOpen/Txt_HudAlertsOpenLabel`<br>节点矩形超出 1920×1080 画布<br>期望：满足标准 2,4,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/03-FieldHudResidentForm-Panel_PageHudAlerts-application.json) · [证据2](geometry/03-FieldHudResidentForm-Panel_PageHudAlerts-memory.json) · [证据3](geometry/03-FieldHudResidentForm-Panel_PageHudAlerts-scroll-top.json) · [证据4](geometry/03-FieldHudResidentForm-Panel_PageHudAlerts-state-base.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| 常驻HUD模块 | 已检查整体共存 | 初始区域；警报/保存/追踪业务变化未完全覆盖 |

效果图要求的代表态：[warning](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-03-HudAlerts-warning.png)、[critical](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-03-HudAlerts-critical.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 012 / 03 / HudNavigation · FieldHudResidentForm/Panel_PageHudNavigation

Prefab：[Assets/Game/Prefabs/UI/Hud/FieldHudResidentForm.prefab](../../../../../Assets/Game/Prefabs/UI/Hud/FieldHudResidentForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/FieldHudResidentForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/FieldHudResidentForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-03-HudNavigation-normal.png)；证据：[application](runtime/03-FieldHudResidentForm-Panel_PageHudNavigation-application.png) · [memory](runtime/03-FieldHudResidentForm-Panel_PageHudNavigation-memory.png) · [world](runtime/03-FieldHudResidentForm-Panel_PageHudNavigation-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0045 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0045, UIA-0046 |
| 5. 九宫格与边角 | 发现问题 | UIA-0045 |
| 6. 对齐间距留白 | 发现问题 | UIA-0046 |
| 7. 文字与可读性 | 发现问题 | UIA-0046 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 当前样本未发现新增问题 |  |
| 10. 点击命中与焦点 | 发现问题 | UIA-0045 |
| 11. 动态内容与运行状态 | 当前样本未发现新增问题 |  |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0045 / P1 / 已确认 | **HUD_ACTION_COMPRESSION** · `HUD 右侧任务与警报按钮`<br>任务、引导、定位及警报入口挤在卡片底沿/右沿，按钮尺寸与边角比例失衡，文字靠近端帽。<br>期望：满足标准 3,4,5,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/03-FieldHudResidentForm-Panel_PageHudNavigation.png) |
| UIA-0046 / P1 / 已确认 | **HUD_EDGE_PADDING** · `HUD 顶部状态/任务/警报区域`<br>资源、保存、任务和警报文字紧贴装饰边缘，部分文字与上边框相交；顶栏和右侧卡片的内边距不足。<br>期望：满足标准 4,6,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/03-FieldHudResidentForm-Panel_PageHudNavigation.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| 常驻HUD模块 | 已检查整体共存 | 初始区域；警报/保存/追踪业务变化未完全覆盖 |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-03-HudNavigation-normal.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 013 / 03 / HudSave · FieldHudResidentForm/Panel_PageHudSave

Prefab：[Assets/Game/Prefabs/UI/Hud/FieldHudResidentForm.prefab](../../../../../Assets/Game/Prefabs/UI/Hud/FieldHudResidentForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/FieldHudResidentForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/FieldHudResidentForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-03-HudSave-success.png)；证据：[application](runtime/03-FieldHudResidentForm-Panel_PageHudSave-application.png) · [memory](runtime/03-FieldHudResidentForm-Panel_PageHudSave-memory.png) · [world](runtime/03-FieldHudResidentForm-Panel_PageHudSave-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0047, UIA-0048 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0048, UIA-0049 |
| 5. 九宫格与边角 | 发现问题 | UIA-0047, UIA-0048 |
| 6. 对齐间距留白 | 发现问题 | UIA-0049 |
| 7. 文字与可读性 | 发现问题 | UIA-0049 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 当前样本未发现新增问题 |  |
| 10. 点击命中与焦点 | 发现问题 | UIA-0048 |
| 11. 动态内容与运行状态 | 当前样本未发现新增问题 |  |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0047 / P1 / 已确认 | **BORDER_COMPRESSION** · `FieldHudResidentForm/Grp_PageHost/Panel_PageHudSave/Grp_HudSaveActions/Btn_HudSaveDetails`<br>控件 88×48 小于九宫格固定边总和 112.0×16.0，边角会被压缩<br>期望：满足标准 3,5：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/03-FieldHudResidentForm-Panel_PageHudSave-application.json) · [证据2](geometry/03-FieldHudResidentForm-Panel_PageHudSave-memory.json) · [证据3](geometry/03-FieldHudResidentForm-Panel_PageHudSave-scroll-top.json) · [证据4](geometry/03-FieldHudResidentForm-Panel_PageHudSave-state-base.json) |
| UIA-0048 / P1 / 已确认 | **HUD_ACTION_COMPRESSION** · `HUD 右侧任务与警报按钮`<br>任务、引导、定位及警报入口挤在卡片底沿/右沿，按钮尺寸与边角比例失衡，文字靠近端帽。<br>期望：满足标准 3,4,5,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/03-FieldHudResidentForm-Panel_PageHudSave.png) |
| UIA-0049 / P1 / 已确认 | **HUD_EDGE_PADDING** · `HUD 顶部状态/任务/警报区域`<br>资源、保存、任务和警报文字紧贴装饰边缘，部分文字与上边框相交；顶栏和右侧卡片的内边距不足。<br>期望：满足标准 4,6,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/03-FieldHudResidentForm-Panel_PageHudSave.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| 常驻HUD模块 | 已检查整体共存 | 初始区域；警报/保存/追踪业务变化未完全覆盖 |

效果图要求的代表态：[success](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-03-HudSave-success.png)、[saving](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-03-HudSave-saving.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 014 / 03 / HudStatus · FieldHudResidentForm/Panel_PageHudStatus

Prefab：[Assets/Game/Prefabs/UI/Hud/FieldHudResidentForm.prefab](../../../../../Assets/Game/Prefabs/UI/Hud/FieldHudResidentForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/FieldHudResidentForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/FieldHudResidentForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-03-HudStatus-normal.png)；证据：[application](runtime/03-FieldHudResidentForm-Panel_PageHudStatus-application.png) · [memory](runtime/03-FieldHudResidentForm-Panel_PageHudStatus-memory.png) · [world](runtime/03-FieldHudResidentForm-Panel_PageHudStatus-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0050 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0050, UIA-0051 |
| 5. 九宫格与边角 | 发现问题 | UIA-0050 |
| 6. 对齐间距留白 | 发现问题 | UIA-0051 |
| 7. 文字与可读性 | 发现问题 | UIA-0051 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 当前样本未发现新增问题 |  |
| 10. 点击命中与焦点 | 发现问题 | UIA-0050 |
| 11. 动态内容与运行状态 | 当前样本未发现新增问题 |  |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0050 / P1 / 已确认 | **HUD_ACTION_COMPRESSION** · `HUD 右侧任务与警报按钮`<br>任务、引导、定位及警报入口挤在卡片底沿/右沿，按钮尺寸与边角比例失衡，文字靠近端帽。<br>期望：满足标准 3,4,5,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/03-FieldHudResidentForm-Panel_PageHudStatus.png) |
| UIA-0051 / P1 / 已确认 | **HUD_EDGE_PADDING** · `HUD 顶部状态/任务/警报区域`<br>资源、保存、任务和警报文字紧贴装饰边缘，部分文字与上边框相交；顶栏和右侧卡片的内边距不足。<br>期望：满足标准 4,6,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/03-FieldHudResidentForm-Panel_PageHudStatus.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| 常驻HUD模块 | 已检查整体共存 | 初始区域；警报/保存/追踪业务变化未完全覆盖 |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-03-HudStatus-normal.png)、[warning](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-03-HudStatus-warning.png)、[critical](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-03-HudStatus-critical.png)、[night](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-03-HudStatus-night.png)、[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-03.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 015 / 03 / HudTracker · FieldHudResidentForm/Panel_PageHudTracker

Prefab：[Assets/Game/Prefabs/UI/Hud/FieldHudResidentForm.prefab](../../../../../Assets/Game/Prefabs/UI/Hud/FieldHudResidentForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/FieldHudResidentForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/FieldHudResidentForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-03-HudTracker-active.png)；证据：[application](runtime/03-FieldHudResidentForm-Panel_PageHudTracker-application.png) · [memory](runtime/03-FieldHudResidentForm-Panel_PageHudTracker-memory.png) · [world](runtime/03-FieldHudResidentForm-Panel_PageHudTracker-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0052, UIA-0053, UIA-0054, UIA-0055 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0055, UIA-0056 |
| 5. 九宫格与边角 | 发现问题 | UIA-0052, UIA-0053, UIA-0054, UIA-0055 |
| 6. 对齐间距留白 | 发现问题 | UIA-0056 |
| 7. 文字与可读性 | 发现问题 | UIA-0056 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 当前样本未发现新增问题 |  |
| 10. 点击命中与焦点 | 发现问题 | UIA-0055 |
| 11. 动态内容与运行状态 | 当前样本未发现新增问题 |  |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0052 / P1 / 已确认 | **BORDER_COMPRESSION** · `FieldHudResidentForm/Grp_PageHost/Panel_PageHudTracker/Grp_HudTrackerActions/Btn_HudTrackerGuide`<br>控件 96×44 小于九宫格固定边总和 112.0×16.0，边角会被压缩<br>期望：满足标准 3,5：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/03-FieldHudResidentForm-Panel_PageHudTracker-application.json) · [证据2](geometry/03-FieldHudResidentForm-Panel_PageHudTracker-memory.json) · [证据3](geometry/03-FieldHudResidentForm-Panel_PageHudTracker-scroll-top.json) · [证据4](geometry/03-FieldHudResidentForm-Panel_PageHudTracker-state-base.json) |
| UIA-0053 / P1 / 已确认 | **BORDER_COMPRESSION** · `FieldHudResidentForm/Grp_PageHost/Panel_PageHudTracker/Grp_HudTrackerActions/Btn_HudTrackerLocate`<br>控件 96×44 小于九宫格固定边总和 112.0×16.0，边角会被压缩<br>期望：满足标准 3,5：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/03-FieldHudResidentForm-Panel_PageHudTracker-application.json) · [证据2](geometry/03-FieldHudResidentForm-Panel_PageHudTracker-memory.json) · [证据3](geometry/03-FieldHudResidentForm-Panel_PageHudTracker-scroll-top.json) · [证据4](geometry/03-FieldHudResidentForm-Panel_PageHudTracker-state-base.json) |
| UIA-0054 / P1 / 已确认 | **BORDER_COMPRESSION** · `FieldHudResidentForm/Grp_PageHost/Panel_PageHudTracker/Grp_HudTrackerActions/Btn_HudTrackerTask`<br>控件 96×44 小于九宫格固定边总和 112.0×16.0，边角会被压缩<br>期望：满足标准 3,5：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/03-FieldHudResidentForm-Panel_PageHudTracker-application.json) · [证据2](geometry/03-FieldHudResidentForm-Panel_PageHudTracker-memory.json) · [证据3](geometry/03-FieldHudResidentForm-Panel_PageHudTracker-scroll-top.json) · [证据4](geometry/03-FieldHudResidentForm-Panel_PageHudTracker-state-base.json) |
| UIA-0055 / P1 / 已确认 | **HUD_ACTION_COMPRESSION** · `HUD 右侧任务与警报按钮`<br>任务、引导、定位及警报入口挤在卡片底沿/右沿，按钮尺寸与边角比例失衡，文字靠近端帽。<br>期望：满足标准 3,4,5,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/03-FieldHudResidentForm-Panel_PageHudTracker.png) |
| UIA-0056 / P1 / 已确认 | **HUD_EDGE_PADDING** · `HUD 顶部状态/任务/警报区域`<br>资源、保存、任务和警报文字紧贴装饰边缘，部分文字与上边框相交；顶栏和右侧卡片的内边距不足。<br>期望：满足标准 4,6,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/03-FieldHudResidentForm-Panel_PageHudTracker.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| 常驻HUD模块 | 已检查整体共存 | 初始区域；警报/保存/追踪业务变化未完全覆盖 |

效果图要求的代表态：[active](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-03-HudTracker-active.png)、[complete](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-03-HudTracker-complete.png)、[blocked](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-03-HudTracker-blocked.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 016 / 04 / 能源系统详情 · BaseCommandEnergyForm/Panel_PageHubEnergy

Prefab：[Assets/Game/Prefabs/UI/Operations/BaseCommandEnergyForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/BaseCommandEnergyForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/BaseCommandEnergyForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/BaseCommandEnergyForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-04-HubEnergy-full.png)；证据：[application](runtime/04-BaseCommandEnergyForm-Panel_PageHubEnergy-application.png) · [memory](runtime/04-BaseCommandEnergyForm-Panel_PageHubEnergy-memory.png) · [world](runtime/04-BaseCommandEnergyForm-Panel_PageHubEnergy-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 发现问题 | UIA-0057 |
| 3. 尺寸与比例 | 发现问题 | UIA-0057, UIA-0058 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0057, UIA-0058, UIA-0059, UIA-0060, UIA-0061, UIA-0062 |
| 5. 九宫格与边角 | 发现问题 | UIA-0059, UIA-0063 |
| 6. 对齐间距留白 | 发现问题 | UIA-0059 |
| 7. 文字与可读性 | 发现问题 | UIA-0058, UIA-0059, UIA-0060, UIA-0061, UIA-0062, UIA-0063 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0063 |
| 9. 素材与状态语义 | 发现问题 | UIA-0059, UIA-0063 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0058 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0058, UIA-0059, UIA-0060, UIA-0061 |
| 12. 同类页面一致性 | 发现问题 | UIA-0057, UIA-0063 |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0057 / P1 / 已确认 | **OVERSIZED_EXTRA_FRAME** · `页面外层装饰框`<br>除页面主框外还有放大到近乎全屏的第二层装饰框，贴近画布四边并出现巨大斜角，破坏与同类页面的一致性。<br>期望：满足标准 2,3,4,12：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/04-BaseCommandEnergyForm-Panel_PageHubEnergy.png) |
| UIA-0058 / P1 / 已确认 | **SCROLL_ROW_TOO_TALL** · `BaseCommandEnergyForm/Panel_Frame/Grp_PageHost/Panel_PageHubEnergy/Panel_HubEnergyFacilities/List_HubEnergyFacilities/Viewport_HubEnergyFacilities/Content_HubEnergyFacilities/Item_HubEnergyFacilitiesTemplate/Btn_HubEnergyFacilitiesRow`<br>单条记录高 104.0px，大于列表可视高度 49.3px；无法完整显示一条记录<br>期望：满足标准 3,4,7,10,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/04-BaseCommandEnergyForm-Panel_PageHubEnergy-world-scroll-bottom.json) · [证据2](geometry/04-BaseCommandEnergyForm-Panel_PageHubEnergy-world-scroll-top.json) · [证据3](geometry/04-BaseCommandEnergyForm-Panel_PageHubEnergy-world.json) |
| UIA-0059 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `BaseCommandEnergyForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/04-BaseCommandEnergyForm-Panel_PageHubEnergy-application.json) · [证据2](geometry/04-BaseCommandEnergyForm-Panel_PageHubEnergy-memory.json) · [证据3](geometry/04-BaseCommandEnergyForm-Panel_PageHubEnergy-scroll-bottom.json) · [证据4](geometry/04-BaseCommandEnergyForm-Panel_PageHubEnergy-scroll-top.json) |
| UIA-0060 / P1 / 已确认 | **TEXT_MASK_CLIP** · `BaseCommandEnergyForm/Panel_Frame/Grp_PageHost/Panel_PageHubEnergy/Panel_HubEnergySummary/List_HubEnergySummary/Viewport_HubEnergySummary/Content_HubEnergySummary/Item_HubEnergySummaryTemplate/Btn_HubEnergySummaryRow/Txt_HubEnergySummaryRowValue`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/04-BaseCommandEnergyForm-Panel_PageHubEnergy-world-scroll-bottom.json) |
| UIA-0061 / P1 / 已确认 | **TEXT_MASK_CLIP** · `BaseCommandEnergyForm/Panel_Frame/Grp_PageHost/Panel_PageHubEnergy/Panel_HubEnergySummary/List_HubEnergySummary/Viewport_HubEnergySummary/Content_HubEnergySummary/Txt_HubEnergySummaryBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/04-BaseCommandEnergyForm-Panel_PageHubEnergy-application.json) · [证据2](geometry/04-BaseCommandEnergyForm-Panel_PageHubEnergy-memory.json) · [证据3](geometry/04-BaseCommandEnergyForm-Panel_PageHubEnergy-scroll-top.json) · [证据4](geometry/04-BaseCommandEnergyForm-Panel_PageHubEnergy-world-scroll-top.json) |
| UIA-0062 / P1 / 已确认 | **TEXT_OVERLAP** · `BaseCommandEnergyForm/Panel_Frame/Grp_PageHost/Panel_PageHubEnergy/Txt_HubEnergyTitle ↔ BaseCommandEnergyForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/04-BaseCommandEnergyForm-Panel_PageHubEnergy-application.json) · [证据2](geometry/04-BaseCommandEnergyForm-Panel_PageHubEnergy-memory.json) · [证据3](geometry/04-BaseCommandEnergyForm-Panel_PageHubEnergy-scroll-bottom.json) · [证据4](geometry/04-BaseCommandEnergyForm-Panel_PageHubEnergy-scroll-top.json) |
| UIA-0063 / P2 / 已确认 | **READONLY_USES_DISABLED_SKIN** · `只读属性/摘要条目`<br>正常可读数据被渲染为禁用 Button，出现斜纹禁用底图和按钮端帽；信息字段看起来像不可用操作，弱化正文。<br>期望：满足标准 5,7,8,9,12：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/04-BaseCommandEnergyForm-Panel_PageHubEnergy-world.png) · [证据2](geometry/04-BaseCommandEnergyForm-Panel_PageHubEnergy-world.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_HubEnergyLoadingState](state-layout/04-BaseCommandEnergyForm-Panel_PageHubEnergy-HubEnergyLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HubEnergyEmptyState](state-layout/04-BaseCommandEnergyForm-Panel_PageHubEnergy-HubEnergyEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HubEnergyErrorState](state-layout/04-BaseCommandEnergyForm-Panel_PageHubEnergy-HubEnergyErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HubEnergySuccessState](state-layout/04-BaseCommandEnergyForm-Panel_PageHubEnergy-HubEnergySuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HubEnergyDisabledState](state-layout/04-BaseCommandEnergyForm-Panel_PageHubEnergy-HubEnergyDisabledState.png) | 已检查 | application、memory |

效果图要求的代表态：[full](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-04-HubEnergy-full.png)、[low](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-04-HubEnergy-low.png)、[blackout](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-04-HubEnergy-blackout.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 017 / 04 / 对象与系统 · BaseCommandHubForm/Panel_PageHubObjects

Prefab：[Assets/Game/Prefabs/UI/Operations/BaseCommandHubForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/BaseCommandHubForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/BaseCommandHubForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/BaseCommandHubForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-04-HubObjects-normal.png)；证据：[application](runtime/04-BaseCommandHubForm-Panel_PageHubObjects-application.png) · [memory](runtime/04-BaseCommandHubForm-Panel_PageHubObjects-memory.png) · [world](runtime/04-BaseCommandHubForm-Panel_PageHubObjects-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0064 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0065, UIA-0066, UIA-0067 |
| 5. 九宫格与边角 | 发现问题 | UIA-0066 |
| 6. 对齐间距留白 | 发现问题 | UIA-0065, UIA-0066 |
| 7. 文字与可读性 | 发现问题 | UIA-0064, UIA-0066, UIA-0067 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 发现问题 | UIA-0064, UIA-0066 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0065 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0066 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0064 / P1 / 已确认 | **LOW_CONTRAST_OBJECT_ART** · `对象摘要插画`<br>对象摘要里的线框建筑与卡片背景明度接近，正常观看几乎不可辨认。<br>期望：满足标准 3,7,9：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/04-BaseCommandHubForm-Panel_PageHubObjects.png) |
| UIA-0065 / P1 / 已确认 | **SEARCH_AT_PANEL_EDGE** · `对象索引搜索输入框`<br>搜索框贴在索引卡下边缘，输入框端部与卡片边框重叠，缺少输入区域留白。<br>期望：满足标准 4,6,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/04-BaseCommandHubForm-Panel_PageHubObjects.png) |
| UIA-0066 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `BaseCommandHubForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/04-BaseCommandHubForm-Panel_PageHubObjects-application.json) · [证据2](geometry/04-BaseCommandHubForm-Panel_PageHubObjects-state-HubObjectsDisabledState.json) · [证据3](geometry/04-BaseCommandHubForm-Panel_PageHubObjects-state-HubObjectsEmptyState.json) · [证据4](geometry/04-BaseCommandHubForm-Panel_PageHubObjects-state-HubObjectsErrorState.json) |
| UIA-0067 / P1 / 已确认 | **TEXT_OVERLAP** · `BaseCommandHubForm/Panel_Frame/Grp_PageHost/Panel_PageHubObjects/Txt_HubObjectsTitle ↔ BaseCommandHubForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/04-BaseCommandHubForm-Panel_PageHubObjects-application.json) · [证据2](geometry/04-BaseCommandHubForm-Panel_PageHubObjects-memory.json) · [证据3](geometry/04-BaseCommandHubForm-Panel_PageHubObjects-scroll-bottom.json) · [证据4](geometry/04-BaseCommandHubForm-Panel_PageHubObjects-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_HubObjectsLoadingState](state-layout/04-BaseCommandHubForm-Panel_PageHubObjects-HubObjectsLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HubObjectsEmptyState](state-layout/04-BaseCommandHubForm-Panel_PageHubObjects-HubObjectsEmptyState.png) | 已检查 | world |
| [Grp_HubObjectsErrorState](state-layout/04-BaseCommandHubForm-Panel_PageHubObjects-HubObjectsErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HubObjectsSuccessState](state-layout/04-BaseCommandHubForm-Panel_PageHubObjects-HubObjectsSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HubObjectsDisabledState](state-layout/04-BaseCommandHubForm-Panel_PageHubObjects-HubObjectsDisabledState.png) | 已检查 | application |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-04-HubObjects-normal.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 018 / 04 / 基地概况 · BaseCommandHubForm/Panel_PageHubOverview

Prefab：[Assets/Game/Prefabs/UI/Operations/BaseCommandHubForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/BaseCommandHubForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/BaseCommandHubForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/BaseCommandHubForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-04-HubOverview-overview.png)；证据：[application](runtime/04-BaseCommandHubForm-Panel_PageHubOverview-application.png) · [memory](runtime/04-BaseCommandHubForm-Panel_PageHubOverview-memory.png) · [world](runtime/04-BaseCommandHubForm-Panel_PageHubOverview-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0068, UIA-0069, UIA-0070 |
| 5. 九宫格与边角 | 发现问题 | UIA-0068 |
| 6. 对齐间距留白 | 发现问题 | UIA-0068 |
| 7. 文字与可读性 | 发现问题 | UIA-0068, UIA-0069, UIA-0070 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 发现问题 | UIA-0068 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0068, UIA-0069 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0068 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `BaseCommandHubForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/04-BaseCommandHubForm-Panel_PageHubOverview-state-HubOverviewDisabledState.json) · [证据2](geometry/04-BaseCommandHubForm-Panel_PageHubOverview-state-HubOverviewEmptyState.json) · [证据3](geometry/04-BaseCommandHubForm-Panel_PageHubOverview-state-HubOverviewErrorState.json) · [证据4](geometry/04-BaseCommandHubForm-Panel_PageHubOverview-state-HubOverviewLoadingState.json) |
| UIA-0069 / P1 / 已确认 | **TEXT_MASK_CLIP** · `BaseCommandHubForm/Panel_Frame/Grp_PageHost/Panel_PageHubOverview/Panel_HubOverviewEconomy/List_HubOverviewEconomy/Viewport_HubOverviewEconomy/Content_HubOverviewEconomy/Txt_HubOverviewEconomyBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/04-BaseCommandHubForm-Panel_PageHubOverview-application.json) · [证据2](geometry/04-BaseCommandHubForm-Panel_PageHubOverview-memory.json) · [证据3](geometry/04-BaseCommandHubForm-Panel_PageHubOverview-scroll-top.json) · [证据4](geometry/04-BaseCommandHubForm-Panel_PageHubOverview-state-base.json) |
| UIA-0070 / P1 / 已确认 | **TEXT_OVERLAP** · `BaseCommandHubForm/Panel_Frame/Grp_PageHost/Panel_PageHubOverview/Txt_HubOverviewTitle ↔ BaseCommandHubForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/04-BaseCommandHubForm-Panel_PageHubOverview-application.json) · [证据2](geometry/04-BaseCommandHubForm-Panel_PageHubOverview-memory.json) · [证据3](geometry/04-BaseCommandHubForm-Panel_PageHubOverview-scroll-top.json) · [证据4](geometry/04-BaseCommandHubForm-Panel_PageHubOverview-state-base.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_HubOverviewLoadingState](state-layout/04-BaseCommandHubForm-Panel_PageHubOverview-HubOverviewLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HubOverviewEmptyState](state-layout/04-BaseCommandHubForm-Panel_PageHubOverview-HubOverviewEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HubOverviewErrorState](state-layout/04-BaseCommandHubForm-Panel_PageHubOverview-HubOverviewErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HubOverviewSuccessState](state-layout/04-BaseCommandHubForm-Panel_PageHubOverview-HubOverviewSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HubOverviewDisabledState](state-layout/04-BaseCommandHubForm-Panel_PageHubOverview-HubOverviewDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[overview](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-04-HubOverview-overview.png)、[empty](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-04-HubOverview-empty.png)、[alert](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-04-HubOverview-alert.png)、[loading](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-04-HubOverview-loading.png)、[level2](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-04-HubOverview-level2.png)、[level3](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-04-HubOverview-level3.png)、[level4](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-04-HubOverview-level4.png)、[overview](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-04.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 019 / 04 / 规则自动化 · BaseCommandHubForm/Panel_PageHubRules

Prefab：[Assets/Game/Prefabs/UI/Operations/BaseCommandHubForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/BaseCommandHubForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/BaseCommandHubForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/BaseCommandHubForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-04-HubRules-open.png)；证据：[application](runtime/04-BaseCommandHubForm-Panel_PageHubRules-application.png) · [memory](runtime/04-BaseCommandHubForm-Panel_PageHubRules-memory.png) · [world](runtime/04-BaseCommandHubForm-Panel_PageHubRules-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0071, UIA-0072 |
| 5. 九宫格与边角 | 发现问题 | UIA-0071 |
| 6. 对齐间距留白 | 发现问题 | UIA-0071 |
| 7. 文字与可读性 | 发现问题 | UIA-0071, UIA-0072 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 发现问题 | UIA-0071 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0071 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0071 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `BaseCommandHubForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/04-BaseCommandHubForm-Panel_PageHubRules-state-HubRulesDisabledState.json) · [证据2](geometry/04-BaseCommandHubForm-Panel_PageHubRules-state-HubRulesEmptyState.json) · [证据3](geometry/04-BaseCommandHubForm-Panel_PageHubRules-state-HubRulesErrorState.json) · [证据4](geometry/04-BaseCommandHubForm-Panel_PageHubRules-state-HubRulesLoadingState.json) |
| UIA-0072 / P1 / 已确认 | **TEXT_OVERLAP** · `BaseCommandHubForm/Panel_Frame/Grp_PageHost/Panel_PageHubRules/Txt_HubRulesTitle ↔ BaseCommandHubForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/04-BaseCommandHubForm-Panel_PageHubRules-application.json) · [证据2](geometry/04-BaseCommandHubForm-Panel_PageHubRules-memory.json) · [证据3](geometry/04-BaseCommandHubForm-Panel_PageHubRules-scroll-top.json) · [证据4](geometry/04-BaseCommandHubForm-Panel_PageHubRules-state-base.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_HubRulesLoadingState](state-layout/04-BaseCommandHubForm-Panel_PageHubRules-HubRulesLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HubRulesEmptyState](state-layout/04-BaseCommandHubForm-Panel_PageHubRules-HubRulesEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HubRulesErrorState](state-layout/04-BaseCommandHubForm-Panel_PageHubRules-HubRulesErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HubRulesSuccessState](state-layout/04-BaseCommandHubForm-Panel_PageHubRules-HubRulesSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HubRulesDisabledState](state-layout/04-BaseCommandHubForm-Panel_PageHubRules-HubRulesDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[open](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-04-HubRules-open.png)、[locked](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-04-HubRules-locked.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 020 / 04 / 统计 · BaseCommandHubForm/Panel_PageHubStats

Prefab：[Assets/Game/Prefabs/UI/Operations/BaseCommandHubForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/BaseCommandHubForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/BaseCommandHubForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/BaseCommandHubForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-04-HubStats-normal.png)；证据：[application](runtime/04-BaseCommandHubForm-Panel_PageHubStats-application.png) · [memory](runtime/04-BaseCommandHubForm-Panel_PageHubStats-memory.png) · [world](runtime/04-BaseCommandHubForm-Panel_PageHubStats-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0076 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0073, UIA-0074, UIA-0075 |
| 5. 九宫格与边角 | 发现问题 | UIA-0073 |
| 6. 对齐间距留白 | 发现问题 | UIA-0073 |
| 7. 文字与可读性 | 发现问题 | UIA-0073, UIA-0074, UIA-0075 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0076 |
| 9. 素材与状态语义 | 发现问题 | UIA-0073 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0073, UIA-0074 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0073 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `BaseCommandHubForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/04-BaseCommandHubForm-Panel_PageHubStats-application.json) · [证据2](geometry/04-BaseCommandHubForm-Panel_PageHubStats-memory.json) · [证据3](geometry/04-BaseCommandHubForm-Panel_PageHubStats-scroll-bottom.json) · [证据4](geometry/04-BaseCommandHubForm-Panel_PageHubStats-scroll-top.json) |
| UIA-0074 / P1 / 已确认 | **TEXT_MASK_CLIP** · `BaseCommandHubForm/Panel_Frame/Grp_PageHost/Panel_PageHubStats/Panel_HubStatsMetrics/List_HubStatsMetrics/Viewport_HubStatsMetrics/Content_HubStatsMetrics/Txt_HubStatsMetricsBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/04-BaseCommandHubForm-Panel_PageHubStats-application.json) · [证据2](geometry/04-BaseCommandHubForm-Panel_PageHubStats-memory.json) · [证据3](geometry/04-BaseCommandHubForm-Panel_PageHubStats-scroll-bottom.json) · [证据4](geometry/04-BaseCommandHubForm-Panel_PageHubStats-scroll-top.json) |
| UIA-0075 / P1 / 已确认 | **TEXT_OVERLAP** · `BaseCommandHubForm/Panel_Frame/Grp_PageHost/Panel_PageHubStats/Txt_HubStatsTitle ↔ BaseCommandHubForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/04-BaseCommandHubForm-Panel_PageHubStats-application.json) · [证据2](geometry/04-BaseCommandHubForm-Panel_PageHubStats-memory.json) · [证据3](geometry/04-BaseCommandHubForm-Panel_PageHubStats-scroll-bottom.json) · [证据4](geometry/04-BaseCommandHubForm-Panel_PageHubStats-scroll-top.json) |
| UIA-0076 / P2 / 已确认 | **DECORATION_DOMINATES_RECORDS** · `Panel_HubStatsRecords`<br>记录入口被大幅空装饰框占据，实际记录说明很小，装饰与信息的主次关系失衡。<br>期望：满足标准 3,8：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/04-BaseCommandHubForm-Panel_PageHubStats.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_HubStatsLoadingState](state-layout/04-BaseCommandHubForm-Panel_PageHubStats-HubStatsLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HubStatsEmptyState](state-layout/04-BaseCommandHubForm-Panel_PageHubStats-HubStatsEmptyState.png) | 已检查 | memory、world |
| [Grp_HubStatsErrorState](state-layout/04-BaseCommandHubForm-Panel_PageHubStats-HubStatsErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HubStatsSuccessState](state-layout/04-BaseCommandHubForm-Panel_PageHubStats-HubStatsSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HubStatsDisabledState](state-layout/04-BaseCommandHubForm-Panel_PageHubStats-HubStatsDisabledState.png) | 已检查 | application |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-04-HubStats-normal.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 021 / 04 / 待处理任务 · BaseCommandHubForm/Panel_PageHubTasks

Prefab：[Assets/Game/Prefabs/UI/Operations/BaseCommandHubForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/BaseCommandHubForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/BaseCommandHubForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/BaseCommandHubForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-04-HubTasks-todo.png)；证据：[application](runtime/04-BaseCommandHubForm-Panel_PageHubTasks-application.png) · [memory](runtime/04-BaseCommandHubForm-Panel_PageHubTasks-memory.png) · [world](runtime/04-BaseCommandHubForm-Panel_PageHubTasks-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0077, UIA-0078 |
| 5. 九宫格与边角 | 发现问题 | UIA-0077 |
| 6. 对齐间距留白 | 发现问题 | UIA-0077 |
| 7. 文字与可读性 | 发现问题 | UIA-0077, UIA-0078 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 发现问题 | UIA-0077 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0077 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0077 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `BaseCommandHubForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/04-BaseCommandHubForm-Panel_PageHubTasks-state-HubTasksDisabledState.json) · [证据2](geometry/04-BaseCommandHubForm-Panel_PageHubTasks-state-HubTasksEmptyState.json) · [证据3](geometry/04-BaseCommandHubForm-Panel_PageHubTasks-state-HubTasksErrorState.json) · [证据4](geometry/04-BaseCommandHubForm-Panel_PageHubTasks-state-HubTasksLoadingState.json) |
| UIA-0078 / P1 / 已确认 | **TEXT_OVERLAP** · `BaseCommandHubForm/Panel_Frame/Grp_PageHost/Panel_PageHubTasks/Txt_HubTasksTitle ↔ BaseCommandHubForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/04-BaseCommandHubForm-Panel_PageHubTasks-application.json) · [证据2](geometry/04-BaseCommandHubForm-Panel_PageHubTasks-memory.json) · [证据3](geometry/04-BaseCommandHubForm-Panel_PageHubTasks-scroll-top.json) · [证据4](geometry/04-BaseCommandHubForm-Panel_PageHubTasks-state-base.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_HubTasksLoadingState](state-layout/04-BaseCommandHubForm-Panel_PageHubTasks-HubTasksLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HubTasksEmptyState](state-layout/04-BaseCommandHubForm-Panel_PageHubTasks-HubTasksEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HubTasksErrorState](state-layout/04-BaseCommandHubForm-Panel_PageHubTasks-HubTasksErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HubTasksSuccessState](state-layout/04-BaseCommandHubForm-Panel_PageHubTasks-HubTasksSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HubTasksDisabledState](state-layout/04-BaseCommandHubForm-Panel_PageHubTasks-HubTasksDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[todo](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-04-HubTasks-todo.png)、[empty](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-04-HubTasks-empty.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 022 / 04 / 中枢机器详情 · BaseCommandHubForm/Panel_PageRemoteMachine

Prefab：[Assets/Game/Prefabs/UI/Operations/BaseCommandHubForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/BaseCommandHubForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/BaseCommandHubForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/BaseCommandHubForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-04-RemoteMachine-normal.png)；证据：[application](runtime/04-BaseCommandHubForm-Panel_PageRemoteMachine-application.png) · [memory](runtime/04-BaseCommandHubForm-Panel_PageRemoteMachine-memory.png) · [world](runtime/04-BaseCommandHubForm-Panel_PageRemoteMachine-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0079, UIA-0080 |
| 5. 九宫格与边角 | 发现问题 | UIA-0079 |
| 6. 对齐间距留白 | 发现问题 | UIA-0079 |
| 7. 文字与可读性 | 发现问题 | UIA-0079, UIA-0080 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 发现问题 | UIA-0079 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0079 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0079 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `BaseCommandHubForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/04-BaseCommandHubForm-Panel_PageRemoteMachine-state-RemoteMachineDisabledState.json) · [证据2](geometry/04-BaseCommandHubForm-Panel_PageRemoteMachine-state-RemoteMachineEmptyState.json) · [证据3](geometry/04-BaseCommandHubForm-Panel_PageRemoteMachine-state-RemoteMachineErrorState.json) · [证据4](geometry/04-BaseCommandHubForm-Panel_PageRemoteMachine-state-RemoteMachineLoadingState.json) |
| UIA-0080 / P1 / 已确认 | **TEXT_OVERLAP** · `BaseCommandHubForm/Panel_Frame/Grp_PageHost/Panel_PageRemoteMachine/Txt_RemoteMachineTitle ↔ BaseCommandHubForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/04-BaseCommandHubForm-Panel_PageRemoteMachine-application.json) · [证据2](geometry/04-BaseCommandHubForm-Panel_PageRemoteMachine-memory.json) · [证据3](geometry/04-BaseCommandHubForm-Panel_PageRemoteMachine-scroll-top.json) · [证据4](geometry/04-BaseCommandHubForm-Panel_PageRemoteMachine-state-base.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_RemoteMachineLoadingState](state-layout/04-BaseCommandHubForm-Panel_PageRemoteMachine-RemoteMachineLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_RemoteMachineEmptyState](state-layout/04-BaseCommandHubForm-Panel_PageRemoteMachine-RemoteMachineEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_RemoteMachineErrorState](state-layout/04-BaseCommandHubForm-Panel_PageRemoteMachine-RemoteMachineErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_RemoteMachineSuccessState](state-layout/04-BaseCommandHubForm-Panel_PageRemoteMachine-RemoteMachineSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_RemoteMachineDisabledState](state-layout/04-BaseCommandHubForm-Panel_PageRemoteMachine-RemoteMachineDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-04-RemoteMachine-normal.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 023 / 05 / 机器算法 · FieldHudDetailForm/Panel_PageMachineAlgorithm

Prefab：[Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-05-MachineAlgorithm-normal.png)；证据：[application](runtime/05-FieldHudDetailForm-Panel_PageMachineAlgorithm-application.png) · [memory](runtime/05-FieldHudDetailForm-Panel_PageMachineAlgorithm-memory.png) · [world](runtime/05-FieldHudDetailForm-Panel_PageMachineAlgorithm-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0081 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0081, UIA-0082 |
| 5. 九宫格与边角 | 发现问题 | UIA-0082 |
| 6. 对齐间距留白 | 发现问题 | UIA-0081, UIA-0082, UIA-0083 |
| 7. 文字与可读性 | 发现问题 | UIA-0081, UIA-0082 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0081, UIA-0083 |
| 9. 素材与状态语义 | 发现问题 | UIA-0082 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0081 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0082, UIA-0083 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0081 / P1 / 已确认 | **SIDEBAR_ACTION_DENSITY** · `侧栏底部操作区`<br>底部按钮直接叠在装饰框底沿，同一行容纳多个长动作名；端帽占用大部分宽度，文字与操作层级不清楚。<br>期望：满足标准 3,4,6,7,8,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/05-FieldHudDetailForm-Panel_PageMachineAlgorithm.png) |
| UIA-0082 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `FieldHudDetailForm/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/05-FieldHudDetailForm-Panel_PageMachineAlgorithm-application.json) · [证据2](geometry/05-FieldHudDetailForm-Panel_PageMachineAlgorithm-memory.json) · [证据3](geometry/05-FieldHudDetailForm-Panel_PageMachineAlgorithm-scroll-bottom.json) · [证据4](geometry/05-FieldHudDetailForm-Panel_PageMachineAlgorithm-scroll-top.json) |
| UIA-0083 / P2 / 已确认 | **SIDEBAR_UNUSED_AREA** · `侧栏内容区`<br>主要字段集中在中段的少数窄条卡片，上下留白较大；需要结合动态内容重新分配高度，不能仅按图片摆放后再塞文字。<br>期望：满足标准 6,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/05-FieldHudDetailForm-Panel_PageMachineAlgorithm.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_MachineAlgorithmLoadingState](state-layout/05-FieldHudDetailForm-Panel_PageMachineAlgorithm-MachineAlgorithmLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MachineAlgorithmEmptyState](state-layout/05-FieldHudDetailForm-Panel_PageMachineAlgorithm-MachineAlgorithmEmptyState.png) | 已检查 | world |
| [Grp_MachineAlgorithmErrorState](state-layout/05-FieldHudDetailForm-Panel_PageMachineAlgorithm-MachineAlgorithmErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MachineAlgorithmSuccessState](state-layout/05-FieldHudDetailForm-Panel_PageMachineAlgorithm-MachineAlgorithmSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MachineAlgorithmDisabledState](state-layout/05-FieldHudDetailForm-Panel_PageMachineAlgorithm-MachineAlgorithmDisabledState.png) | 已检查 | application、memory |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-05-MachineAlgorithm-normal.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 024 / 05 / 机器任务与诊断 · FieldHudDetailForm/Panel_PageMachineDiagnostics

Prefab：[Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-05-MachineDiagnostics-normal.png)；证据：[application](runtime/05-FieldHudDetailForm-Panel_PageMachineDiagnostics-application.png) · [memory](runtime/05-FieldHudDetailForm-Panel_PageMachineDiagnostics-memory.png) · [world](runtime/05-FieldHudDetailForm-Panel_PageMachineDiagnostics-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0084 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0084, UIA-0085 |
| 5. 九宫格与边角 | 发现问题 | UIA-0085 |
| 6. 对齐间距留白 | 发现问题 | UIA-0084, UIA-0085 |
| 7. 文字与可读性 | 发现问题 | UIA-0084, UIA-0085 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0084 |
| 9. 素材与状态语义 | 发现问题 | UIA-0085 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0084 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0085 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0084 / P1 / 已确认 | **SIDEBAR_ACTION_DENSITY** · `侧栏底部操作区`<br>底部按钮直接叠在装饰框底沿，同一行容纳多个长动作名；端帽占用大部分宽度，文字与操作层级不清楚。<br>期望：满足标准 3,4,6,7,8,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/05-FieldHudDetailForm-Panel_PageMachineDiagnostics.png) |
| UIA-0085 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `FieldHudDetailForm/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/05-FieldHudDetailForm-Panel_PageMachineDiagnostics-application.json) · [证据2](geometry/05-FieldHudDetailForm-Panel_PageMachineDiagnostics-memory.json) · [证据3](geometry/05-FieldHudDetailForm-Panel_PageMachineDiagnostics-scroll-bottom.json) · [证据4](geometry/05-FieldHudDetailForm-Panel_PageMachineDiagnostics-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_MachineDiagnosticsLoadingState](state-layout/05-FieldHudDetailForm-Panel_PageMachineDiagnostics-MachineDiagnosticsLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MachineDiagnosticsEmptyState](state-layout/05-FieldHudDetailForm-Panel_PageMachineDiagnostics-MachineDiagnosticsEmptyState.png) | 已检查 | world |
| [Grp_MachineDiagnosticsErrorState](state-layout/05-FieldHudDetailForm-Panel_PageMachineDiagnostics-MachineDiagnosticsErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MachineDiagnosticsSuccessState](state-layout/05-FieldHudDetailForm-Panel_PageMachineDiagnostics-MachineDiagnosticsSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MachineDiagnosticsDisabledState](state-layout/05-FieldHudDetailForm-Panel_PageMachineDiagnostics-MachineDiagnosticsDisabledState.png) | 已检查 | application、memory |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-05-MachineDiagnostics-normal.png)、[error](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-05-MachineDiagnostics-error.png)、[blocked](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-05-MachineDiagnostics-blocked.png)、[recoverable](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-05-MachineDiagnostics-recoverable.png)、[unrecoverable](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-05-MachineDiagnostics-unrecoverable.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 025 / 05 / 机器硬件 · FieldHudDetailForm/Panel_PageMachineHardware

Prefab：[Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-05-MachineHardware-normal.png)；证据：[application](runtime/05-FieldHudDetailForm-Panel_PageMachineHardware-application.png) · [memory](runtime/05-FieldHudDetailForm-Panel_PageMachineHardware-memory.png) · [world](runtime/05-FieldHudDetailForm-Panel_PageMachineHardware-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0086 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0086, UIA-0087 |
| 5. 九宫格与边角 | 发现问题 | UIA-0087 |
| 6. 对齐间距留白 | 发现问题 | UIA-0086, UIA-0087, UIA-0088 |
| 7. 文字与可读性 | 发现问题 | UIA-0086, UIA-0087 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0086, UIA-0088 |
| 9. 素材与状态语义 | 发现问题 | UIA-0087 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0086 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0087, UIA-0088 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0086 / P1 / 已确认 | **SIDEBAR_ACTION_DENSITY** · `侧栏底部操作区`<br>底部按钮直接叠在装饰框底沿，同一行容纳多个长动作名；端帽占用大部分宽度，文字与操作层级不清楚。<br>期望：满足标准 3,4,6,7,8,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/05-FieldHudDetailForm-Panel_PageMachineHardware.png) |
| UIA-0087 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `FieldHudDetailForm/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/05-FieldHudDetailForm-Panel_PageMachineHardware-application.json) · [证据2](geometry/05-FieldHudDetailForm-Panel_PageMachineHardware-memory.json) · [证据3](geometry/05-FieldHudDetailForm-Panel_PageMachineHardware-scroll-bottom.json) · [证据4](geometry/05-FieldHudDetailForm-Panel_PageMachineHardware-scroll-top.json) |
| UIA-0088 / P2 / 已确认 | **SIDEBAR_UNUSED_AREA** · `侧栏内容区`<br>主要字段集中在中段的少数窄条卡片，上下留白较大；需要结合动态内容重新分配高度，不能仅按图片摆放后再塞文字。<br>期望：满足标准 6,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/05-FieldHudDetailForm-Panel_PageMachineHardware.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_MachineHardwareLoadingState](state-layout/05-FieldHudDetailForm-Panel_PageMachineHardware-MachineHardwareLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MachineHardwareEmptyState](state-layout/05-FieldHudDetailForm-Panel_PageMachineHardware-MachineHardwareEmptyState.png) | 已检查 | world |
| [Grp_MachineHardwareErrorState](state-layout/05-FieldHudDetailForm-Panel_PageMachineHardware-MachineHardwareErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MachineHardwareSuccessState](state-layout/05-FieldHudDetailForm-Panel_PageMachineHardware-MachineHardwareSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MachineHardwareDisabledState](state-layout/05-FieldHudDetailForm-Panel_PageMachineHardware-MachineHardwareDisabledState.png) | 已检查 | application、memory |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-05-MachineHardware-normal.png)、[overload](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-05-MachineHardware-overload.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 026 / 05 / 现场机器 · FieldHudMachineOverviewForm/Panel_PageMachineOverview

Prefab：[Assets/Game/Prefabs/UI/Operations/FieldHudMachineOverviewForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/FieldHudMachineOverviewForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/FieldHudMachineOverviewForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/FieldHudMachineOverviewForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-05-MachineOverview-idle.png)；证据：[application](runtime/05-FieldHudMachineOverviewForm-Panel_PageMachineOverview-application.png) · [memory](runtime/05-FieldHudMachineOverviewForm-Panel_PageMachineOverview-memory.png) · [world](runtime/05-FieldHudMachineOverviewForm-Panel_PageMachineOverview-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 发现问题 | UIA-0089 |
| 3. 尺寸与比例 | 发现问题 | UIA-0089, UIA-0090 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0090, UIA-0091 |
| 5. 九宫格与边角 | 发现问题 | UIA-0091 |
| 6. 对齐间距留白 | 发现问题 | UIA-0089, UIA-0090, UIA-0091 |
| 7. 文字与可读性 | 发现问题 | UIA-0090, UIA-0091, UIA-0092, UIA-0093 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0090 |
| 9. 素材与状态语义 | 发现问题 | UIA-0091 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0090 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0091 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0089 / P1 / 已确认 | **OVERVIEW_ART_EDGE** · `MachineOverview 机器插画`<br>机器插画占满窄侧栏宽度并贴近左右装饰边框，与身份/状态卡之间缺少缓冲。<br>期望：满足标准 2,3,6：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/05-FieldHudMachineOverviewForm-Panel_PageMachineOverview.png) |
| UIA-0090 / P1 / 已确认 | **SIDEBAR_ACTION_DENSITY** · `侧栏底部操作区`<br>底部按钮直接叠在装饰框底沿，同一行容纳多个长动作名；端帽占用大部分宽度，文字与操作层级不清楚。<br>期望：满足标准 3,4,6,7,8,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/05-FieldHudMachineOverviewForm-Panel_PageMachineOverview.png) |
| UIA-0091 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `FieldHudMachineOverviewForm/Panel_PageMachineOverview/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/05-FieldHudMachineOverviewForm-Panel_PageMachineOverview-application.json) · [证据2](geometry/05-FieldHudMachineOverviewForm-Panel_PageMachineOverview-memory.json) · [证据3](geometry/05-FieldHudMachineOverviewForm-Panel_PageMachineOverview-scroll-bottom.json) · [证据4](geometry/05-FieldHudMachineOverviewForm-Panel_PageMachineOverview-scroll-top.json) |
| UIA-0092 / P2 / 已确认 | **SMALL_TEXT** · `FieldHudMachineOverviewForm/Panel_PageMachineOverview/Grp_MachineOverviewActions/Btn_MachineOverviewActivate/Txt_MachineOverviewActivateLabel`<br>长操作名称被自动缩到约15–17px，明显小于相邻22px正文；窄按钮端帽挤占阅读区，需统一动作字号与按钮宽度。<br>期望：满足标准 7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/05-FieldHudMachineOverviewForm-Panel_PageMachineOverview-application.json) · [证据2](geometry/05-FieldHudMachineOverviewForm-Panel_PageMachineOverview-memory.json) · [证据3](geometry/05-FieldHudMachineOverviewForm-Panel_PageMachineOverview-scroll-bottom.json) · [证据4](geometry/05-FieldHudMachineOverviewForm-Panel_PageMachineOverview-scroll-top.json) |
| UIA-0093 / P2 / 已确认 | **SMALL_TEXT** · `FieldHudMachineOverviewForm/Panel_PageMachineOverview/Grp_MachineOverviewActions/Btn_MachineOverviewPower/Txt_MachineOverviewPowerLabel`<br>长操作名称被自动缩到约15–17px，明显小于相邻22px正文；窄按钮端帽挤占阅读区，需统一动作字号与按钮宽度。<br>期望：满足标准 7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/05-FieldHudMachineOverviewForm-Panel_PageMachineOverview-application.json) · [证据2](geometry/05-FieldHudMachineOverviewForm-Panel_PageMachineOverview-memory.json) · [证据3](geometry/05-FieldHudMachineOverviewForm-Panel_PageMachineOverview-scroll-bottom.json) · [证据4](geometry/05-FieldHudMachineOverviewForm-Panel_PageMachineOverview-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_MachineOverviewLoadingState](state-layout/05-FieldHudMachineOverviewForm-Panel_PageMachineOverview-MachineOverviewLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MachineOverviewEmptyState](state-layout/05-FieldHudMachineOverviewForm-Panel_PageMachineOverview-MachineOverviewEmptyState.png) | 已检查 | world |
| [Grp_MachineOverviewErrorState](state-layout/05-FieldHudMachineOverviewForm-Panel_PageMachineOverview-MachineOverviewErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MachineOverviewSuccessState](state-layout/05-FieldHudMachineOverviewForm-Panel_PageMachineOverview-MachineOverviewSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MachineOverviewDisabledState](state-layout/05-FieldHudMachineOverviewForm-Panel_PageMachineOverview-MachineOverviewDisabledState.png) | 已检查 | application、memory |

效果图要求的代表态：[idle](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-05-MachineOverview-idle.png)、[active](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-05-MachineOverview-active.png)、[blocked](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-05-MachineOverview-blocked.png)、[error](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-05-MachineOverview-error.png)、[fixed-rotary](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-05-MachineOverview-fixed-rotary.png)、[level-up](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-05-MachineOverview-level-up.png)、[idle](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-05.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 027 / 05 / 机器整备 · MachineLibraryForm/Panel_PageMachinePreparation

Prefab：[Assets/Game/Prefabs/UI/Operations/MachineLibraryForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/MachineLibraryForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/MachineLibraryForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/MachineLibraryForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-05-MachinePreparation-normal.png)；证据：[application](runtime/05-MachineLibraryForm-Panel_PageMachinePreparation-application.png) · [memory](runtime/05-MachineLibraryForm-Panel_PageMachinePreparation-memory.png) · [world](runtime/05-MachineLibraryForm-Panel_PageMachinePreparation-world.png) · [memory/selected-preparation](runtime/machine-preparation-selected.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 发现问题 | UIA-0094, UIA-0097, UIA-0098, UIA-0099, UIA-0100, UIA-0101, UIA-0102, UIA-0103 |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0094, UIA-0095, UIA-0096, UIA-0097, UIA-0098, UIA-0099, UIA-0100, UIA-0101, UIA-0102, UIA-0103, UIA-0104, UIA-0105, UIA-0106, UIA-0107, UIA-0108, UIA-0109 |
| 5. 九宫格与边角 | 发现问题 | UIA-0104 |
| 6. 对齐间距留白 | 发现问题 | UIA-0094, UIA-0095, UIA-0096, UIA-0104 |
| 7. 文字与可读性 | 发现问题 | UIA-0095, UIA-0096, UIA-0104, UIA-0105, UIA-0106, UIA-0107, UIA-0108, UIA-0109 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 发现问题 | UIA-0104 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0094, UIA-0097, UIA-0098, UIA-0099, UIA-0100, UIA-0101, UIA-0102, UIA-0103, UIA-0104, UIA-0105 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0094 / P1 / 已确认 | **PREPARATION_OVERFLOW** · `MachinePreparation 第四信息卡及操作区`<br>底部“设备展示”卡伸出侧栏主框，操作按钮压在部署说明卡和框底之间。<br>期望：满足标准 2,4,6,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/05-MachineLibraryForm-Panel_PageMachinePreparation.png) |
| UIA-0095 / P1 / 已确认 | **SAFE_MARGIN_TEXT** · `MachineLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageMachinePreparation/Panel_MachinePreparationAssembly/List_MachinePreparationAssembly/Viewport_MachinePreparationAssembly/Content_MachinePreparationAssembly/Txt_MachinePreparationAssemblyBody`<br>文字实际轮廓进入 24px 外边距<br>期望：满足标准 4,6,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/machine-preparation-selected.json) |
| UIA-0096 / P1 / 已确认 | **SAFE_MARGIN_TEXT** · `MachineLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageMachinePreparation/Panel_MachinePreparationDisplay/Txt_MachinePreparationDisplayCaption`<br>文字实际轮廓进入 24px 外边距<br>期望：满足标准 4,6,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-application.json) · [证据2](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-memory.json) · [证据3](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-scroll-bottom.json) · [证据4](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-scroll-top.json) |
| UIA-0097 / P1 / 已确认 | **SCREEN_OVERFLOW** · `MachineLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageMachinePreparation/Grp_MachinePreparationDisabledState/Panel_MachinePreparationDisabledMessage`<br>节点矩形超出 1920×1080 画布<br>期望：满足标准 2,4,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-application.json) · [证据2](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-state-MachinePreparationDisabledState.json) |
| UIA-0098 / P1 / 已确认 | **SCREEN_OVERFLOW** · `MachineLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageMachinePreparation/Grp_MachinePreparationEmptyState/Panel_MachinePreparationEmptyMessage`<br>节点矩形超出 1920×1080 画布<br>期望：满足标准 2,4,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-application.json) · [证据2](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-memory.json) · [证据3](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-scroll-bottom.json) · [证据4](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-scroll-top.json) |
| UIA-0099 / P1 / 已确认 | **SCREEN_OVERFLOW** · `MachineLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageMachinePreparation/Grp_MachinePreparationErrorState/Panel_MachinePreparationErrorMessage`<br>节点矩形超出 1920×1080 画布<br>期望：满足标准 2,4,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-state-MachinePreparationErrorState.json) |
| UIA-0100 / P1 / 已确认 | **SCREEN_OVERFLOW** · `MachineLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageMachinePreparation/Grp_MachinePreparationLoadingState/Panel_MachinePreparationLoadingMessage`<br>节点矩形超出 1920×1080 画布<br>期望：满足标准 2,4,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-state-MachinePreparationLoadingState.json) |
| UIA-0101 / P1 / 已确认 | **SCREEN_OVERFLOW** · `MachineLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageMachinePreparation/Grp_MachinePreparationSuccessState/Panel_MachinePreparationSuccessMessage`<br>节点矩形超出 1920×1080 画布<br>期望：满足标准 2,4,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-state-MachinePreparationSuccessState.json) |
| UIA-0102 / P1 / 已确认 | **SCREEN_OVERFLOW** · `MachineLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageMachinePreparation/Panel_MachinePreparationDisplay/Grp_MachinePreparationDisplayVisual/Img_MachinePreparationDisplayPlaceholder`<br>节点矩形超出 1920×1080 画布<br>期望：满足标准 2,4,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-application.json) · [证据2](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-memory.json) · [证据3](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-scroll-bottom.json) · [证据4](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-scroll-top.json) |
| UIA-0103 / P1 / 已确认 | **SCREEN_OVERFLOW** · `MachineLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageMachinePreparation/Panel_MachinePreparationDisplay/Txt_MachinePreparationDisplayCaption`<br>节点矩形超出 1920×1080 画布<br>期望：满足标准 2,4,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-application.json) · [证据2](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-memory.json) · [证据3](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-scroll-bottom.json) · [证据4](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-scroll-top.json) |
| UIA-0104 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `MachineLibraryForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-application.json) · [证据2](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-memory.json) · [证据3](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-scroll-bottom.json) · [证据4](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-scroll-top.json) |
| UIA-0105 / P1 / 已确认 | **TEXT_MASK_CLIP** · `MachineLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageMachinePreparation/Panel_MachinePreparationAssembly/List_MachinePreparationAssembly/Viewport_MachinePreparationAssembly/Content_MachinePreparationAssembly/Txt_MachinePreparationAssemblyBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-memory.json) · [证据2](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-scroll-bottom.json) · [证据3](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-scroll-top.json) · [证据4](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-world-scroll-bottom.json) |
| UIA-0106 / P1 / 已确认 | **TEXT_OVERLAP** · `MachineLibraryForm/Panel_Frame/Btn_FormBack/Txt_FormBackLabel ↔ MachineLibraryForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-application.json) · [证据2](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-memory.json) · [证据3](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-scroll-bottom.json) · [证据4](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-scroll-top.json) |
| UIA-0107 / P1 / 已确认 | **TEXT_OVERLAP** · `MachineLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageMachinePreparation/Grp_MachinePreparationActions/Btn_MachinePreparationInstall/Txt_MachinePreparationInstallLabel ↔ MachineLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageMachinePreparation/Panel_MachinePreparationReadiness/List_MachinePreparationReadiness/Viewport_MachinePreparationReadiness/Content_MachinePreparationReadiness/Txt_MachinePreparationReadinessBody`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-application.json) · [证据2](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-memory.json) · [证据3](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-scroll-bottom.json) · [证据4](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-scroll-top.json) |
| UIA-0108 / P1 / 已确认 | **TEXT_OVERLAP** · `MachineLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageMachinePreparation/Grp_MachinePreparationActions/Btn_MachinePreparationRename/Txt_MachinePreparationRenameLabel ↔ MachineLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageMachinePreparation/Panel_MachinePreparationReadiness/List_MachinePreparationReadiness/Viewport_MachinePreparationReadiness/Content_MachinePreparationReadiness/Txt_MachinePreparationReadinessBody`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-application.json) · [证据2](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-memory.json) · [证据3](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-scroll-bottom.json) · [证据4](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-scroll-top.json) |
| UIA-0109 / P1 / 已确认 | **TEXT_OVERLAP** · `MachineLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageMachinePreparation/Txt_MachinePreparationTitle ↔ MachineLibraryForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-application.json) · [证据2](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-memory.json) · [证据3](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-scroll-bottom.json) · [证据4](geometry/05-MachineLibraryForm-Panel_PageMachinePreparation-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_MachinePreparationLoadingState](state-layout/05-MachineLibraryForm-Panel_PageMachinePreparation-MachinePreparationLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MachinePreparationEmptyState](state-layout/05-MachineLibraryForm-Panel_PageMachinePreparation-MachinePreparationEmptyState.png) | 已检查 | application、memory、world |
| [Grp_MachinePreparationErrorState](state-layout/05-MachineLibraryForm-Panel_PageMachinePreparation-MachinePreparationErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MachinePreparationSuccessState](state-layout/05-MachineLibraryForm-Panel_PageMachinePreparation-MachinePreparationSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MachinePreparationDisabledState](state-layout/05-MachineLibraryForm-Panel_PageMachinePreparation-MachinePreparationDisabledState.png) | 已检查 | application |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-05-MachinePreparation-normal.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 028 / 06 / 农田详情 · FieldHudDetailForm/Panel_PageFarm

Prefab：[Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-06-Farm-normal.png)；证据：[application](runtime/06-FieldHudDetailForm-Panel_PageFarm-application.png) · [memory](runtime/06-FieldHudDetailForm-Panel_PageFarm-memory.png) · [real-world/natural-selection-and-page-API](runtime/06-FieldHudDetailForm-Panel_PageFarm-natural.png) · [world](runtime/06-FieldHudDetailForm-Panel_PageFarm-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 发现问题 | UIA-0110, UIA-0111 |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0111, UIA-0112, UIA-0113 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0111, UIA-0112, UIA-0113, UIA-0114 |
| 5. 九宫格与边角 | 发现问题 | UIA-0114 |
| 6. 对齐间距留白 | 发现问题 | UIA-0113, UIA-0114 |
| 7. 文字与可读性 | 发现问题 | UIA-0110, UIA-0111, UIA-0112, UIA-0113, UIA-0114 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0110, UIA-0111, UIA-0113 |
| 9. 素材与状态语义 | 发现问题 | UIA-0114 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0112, UIA-0113 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0110, UIA-0111, UIA-0112, UIA-0114 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0110 / P1 / 已确认 | **DETAIL_FIELDS_WRONG_SECTION** · `公开状态与生产记录列表`<br>对象名称、类别、状态等公开字段渲染在生产记录列表中；公开状态卡保留“作物/产量”等占位正文，字段分区与阅读顺序不符。<br>期望：满足标准 1,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/06-FieldHudDetailForm-Panel_PageFarm-natural.png) · [证据2](geometry/06-FieldHudDetailForm-Panel_PageFarm-natural.json) |
| UIA-0111 / P1 / 已确认 | **DETAIL_ROW_DEFAULT_HIDDEN** · `现场详情主数据列表`<br>选中有效现场对象后，条目在说明文本/控件之后；列表可视区仅约42px，而记录行104px，默认只见“作物/功率/容量”等占位提示，真实字段几乎不见。<br>期望：满足标准 1,3,4,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/06-FieldHudDetailForm-Panel_PageFarm-natural.png) · [证据2](geometry/06-FieldHudDetailForm-Panel_PageFarm-natural.json) |
| UIA-0112 / P1 / 已确认 | **SCROLL_ROW_TOO_TALL** · `FieldHudDetailForm/Grp_PageHost/Panel_PageFarm/Panel_FarmRecord/List_FarmRecord/Viewport_FarmRecord/Content_FarmRecord/Item_FarmPublicTemplate/Btn_FarmPublicRow`<br>单条记录高 104.0px，大于列表可视高度 42.0px；无法完整显示一条记录<br>期望：满足标准 3,4,7,10,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/06-FieldHudDetailForm-Panel_PageFarm-natural.json) · [证据2](geometry/06-FieldHudDetailForm-Panel_PageFarm-selected.json) |
| UIA-0113 / P1 / 已确认 | **SIDEBAR_ACTION_DENSITY** · `侧栏底部操作区`<br>底部按钮直接叠在装饰框底沿，同一行容纳多个长动作名；端帽占用大部分宽度，文字与操作层级不清楚。<br>期望：满足标准 3,4,6,7,8,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/06-FieldHudDetailForm-Panel_PageFarm.png) |
| UIA-0114 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `FieldHudDetailForm/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/06-FieldHudDetailForm-Panel_PageFarm-application.json) · [证据2](geometry/06-FieldHudDetailForm-Panel_PageFarm-memory.json) · [证据3](geometry/06-FieldHudDetailForm-Panel_PageFarm-scroll-bottom.json) · [证据4](geometry/06-FieldHudDetailForm-Panel_PageFarm-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_FarmLoadingState](state-layout/06-FieldHudDetailForm-Panel_PageFarm-FarmLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_FarmEmptyState](state-layout/06-FieldHudDetailForm-Panel_PageFarm-FarmEmptyState.png) | 已检查 | world |
| [Grp_FarmErrorState](state-layout/06-FieldHudDetailForm-Panel_PageFarm-FarmErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_FarmSuccessState](state-layout/06-FieldHudDetailForm-Panel_PageFarm-FarmSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_FarmDisabledState](state-layout/06-FieldHudDetailForm-Panel_PageFarm-FarmDisabledState.png) | 已检查 | application、memory |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-06-Farm-normal.png)、[dry](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-06-Farm-dry.png)、[wet](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-06-Farm-wet.png)、[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-06.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 029 / 06 / 人工林详情 · FieldHudDetailForm/Panel_PageForest

Prefab：[Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-06-Forest-normal.png)；证据：[application](runtime/06-FieldHudDetailForm-Panel_PageForest-application.png) · [memory](runtime/06-FieldHudDetailForm-Panel_PageForest-memory.png) · [real-world/natural-selection-and-page-API](runtime/06-FieldHudDetailForm-Panel_PageForest-natural.png) · [world](runtime/06-FieldHudDetailForm-Panel_PageForest-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0115 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0115, UIA-0116 |
| 5. 九宫格与边角 | 发现问题 | UIA-0116 |
| 6. 对齐间距留白 | 发现问题 | UIA-0115, UIA-0116 |
| 7. 文字与可读性 | 发现问题 | UIA-0115, UIA-0116 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0115 |
| 9. 素材与状态语义 | 发现问题 | UIA-0116 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0115 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0116 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0115 / P1 / 已确认 | **SIDEBAR_ACTION_DENSITY** · `侧栏底部操作区`<br>底部按钮直接叠在装饰框底沿，同一行容纳多个长动作名；端帽占用大部分宽度，文字与操作层级不清楚。<br>期望：满足标准 3,4,6,7,8,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/06-FieldHudDetailForm-Panel_PageForest.png) |
| UIA-0116 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `FieldHudDetailForm/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/06-FieldHudDetailForm-Panel_PageForest-application.json) · [证据2](geometry/06-FieldHudDetailForm-Panel_PageForest-memory.json) · [证据3](geometry/06-FieldHudDetailForm-Panel_PageForest-scroll-top.json) · [证据4](geometry/06-FieldHudDetailForm-Panel_PageForest-state-ForestDisabledState.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_ForestLoadingState](state-layout/06-FieldHudDetailForm-Panel_PageForest-ForestLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ForestEmptyState](state-layout/06-FieldHudDetailForm-Panel_PageForest-ForestEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ForestErrorState](state-layout/06-FieldHudDetailForm-Panel_PageForest-ForestErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ForestSuccessState](state-layout/06-FieldHudDetailForm-Panel_PageForest-ForestSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ForestDisabledState](state-layout/06-FieldHudDetailForm-Panel_PageForest-ForestDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-06-Forest-normal.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 030 / 06 / 地表矿脉详情 · FieldHudDetailForm/Panel_PageMineral

Prefab：[Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-06-Mineral-normal.png)；证据：[application](runtime/06-FieldHudDetailForm-Panel_PageMineral-application.png) · [memory](runtime/06-FieldHudDetailForm-Panel_PageMineral-memory.png) · [real-world/natural-selection-and-page-API](runtime/06-FieldHudDetailForm-Panel_PageMineral-natural.png) · [world](runtime/06-FieldHudDetailForm-Panel_PageMineral-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0117 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0117, UIA-0118 |
| 5. 九宫格与边角 | 发现问题 | UIA-0118 |
| 6. 对齐间距留白 | 发现问题 | UIA-0117, UIA-0118 |
| 7. 文字与可读性 | 发现问题 | UIA-0117, UIA-0118 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0117 |
| 9. 素材与状态语义 | 发现问题 | UIA-0118 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0117 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0118 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0117 / P1 / 已确认 | **SIDEBAR_ACTION_DENSITY** · `侧栏底部操作区`<br>底部按钮直接叠在装饰框底沿，同一行容纳多个长动作名；端帽占用大部分宽度，文字与操作层级不清楚。<br>期望：满足标准 3,4,6,7,8,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/06-FieldHudDetailForm-Panel_PageMineral.png) |
| UIA-0118 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `FieldHudDetailForm/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/06-FieldHudDetailForm-Panel_PageMineral-application.json) · [证据2](geometry/06-FieldHudDetailForm-Panel_PageMineral-memory.json) · [证据3](geometry/06-FieldHudDetailForm-Panel_PageMineral-scroll-top.json) · [证据4](geometry/06-FieldHudDetailForm-Panel_PageMineral-state-MineralDisabledState.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_MineralLoadingState](state-layout/06-FieldHudDetailForm-Panel_PageMineral-MineralLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MineralEmptyState](state-layout/06-FieldHudDetailForm-Panel_PageMineral-MineralEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MineralErrorState](state-layout/06-FieldHudDetailForm-Panel_PageMineral-MineralErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MineralSuccessState](state-layout/06-FieldHudDetailForm-Panel_PageMineral-MineralSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MineralDisabledState](state-layout/06-FieldHudDetailForm-Panel_PageMineral-MineralDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-06-Mineral-normal.png)、[sparse](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-06-Mineral-sparse.png)、[depleted](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-06-Mineral-depleted.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 031 / 06 / 关联传感数据与资源记录 · FieldHudDetailForm/Panel_PageSensorRecords

Prefab：[Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-06-SensorRecords-soil.png)；证据：[application](runtime/06-FieldHudDetailForm-Panel_PageSensorRecords-application.png) · [memory](runtime/06-FieldHudDetailForm-Panel_PageSensorRecords-memory.png) · [world](runtime/06-FieldHudDetailForm-Panel_PageSensorRecords-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0120 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0119, UIA-0120, UIA-0121 |
| 5. 九宫格与边角 | 发现问题 | UIA-0121 |
| 6. 对齐间距留白 | 发现问题 | UIA-0120, UIA-0121, UIA-0122 |
| 7. 文字与可读性 | 发现问题 | UIA-0119, UIA-0120, UIA-0121, UIA-0123 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0120, UIA-0122 |
| 9. 素材与状态语义 | 发现问题 | UIA-0121 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0120 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0121, UIA-0122 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0119 / P1 / 已确认 | **LONG_SIDEBAR_TITLE** · `Txt_SensorRecordsTitle / 返回入口`<br>“关联传感数据与资源记录”标题占用窄侧栏顶栏，挤入返回/关闭区域。<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/06-FieldHudDetailForm-Panel_PageSensorRecords.png) |
| UIA-0120 / P1 / 已确认 | **SIDEBAR_ACTION_DENSITY** · `侧栏底部操作区`<br>底部按钮直接叠在装饰框底沿，同一行容纳多个长动作名；端帽占用大部分宽度，文字与操作层级不清楚。<br>期望：满足标准 3,4,6,7,8,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/06-FieldHudDetailForm-Panel_PageSensorRecords.png) |
| UIA-0121 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `FieldHudDetailForm/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/06-FieldHudDetailForm-Panel_PageSensorRecords-application.json) · [证据2](geometry/06-FieldHudDetailForm-Panel_PageSensorRecords-memory.json) · [证据3](geometry/06-FieldHudDetailForm-Panel_PageSensorRecords-scroll-bottom.json) · [证据4](geometry/06-FieldHudDetailForm-Panel_PageSensorRecords-scroll-top.json) |
| UIA-0122 / P2 / 已确认 | **SIDEBAR_UNUSED_AREA** · `侧栏内容区`<br>主要字段集中在中段的少数窄条卡片，上下留白较大；需要结合动态内容重新分配高度，不能仅按图片摆放后再塞文字。<br>期望：满足标准 6,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/06-FieldHudDetailForm-Panel_PageSensorRecords.png) |
| UIA-0123 / P2 / 已确认 | **SMALL_TEXT** · `FieldHudDetailForm/Grp_PageHost/Panel_PageSensorRecords/Grp_SensorRecordsActions/Btn_SensorRecordsFilter/Txt_SensorRecordsFilterLabel`<br>长操作名称被自动缩到约15–17px，明显小于相邻22px正文；窄按钮端帽挤占阅读区，需统一动作字号与按钮宽度。<br>期望：满足标准 7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/06-FieldHudDetailForm-Panel_PageSensorRecords-application.json) · [证据2](geometry/06-FieldHudDetailForm-Panel_PageSensorRecords-memory.json) · [证据3](geometry/06-FieldHudDetailForm-Panel_PageSensorRecords-scroll-bottom.json) · [证据4](geometry/06-FieldHudDetailForm-Panel_PageSensorRecords-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_SensorRecordsLoadingState](state-layout/06-FieldHudDetailForm-Panel_PageSensorRecords-SensorRecordsLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_SensorRecordsEmptyState](state-layout/06-FieldHudDetailForm-Panel_PageSensorRecords-SensorRecordsEmptyState.png) | 已检查 | world |
| [Grp_SensorRecordsErrorState](state-layout/06-FieldHudDetailForm-Panel_PageSensorRecords-SensorRecordsErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_SensorRecordsSuccessState](state-layout/06-FieldHudDetailForm-Panel_PageSensorRecords-SensorRecordsSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_SensorRecordsDisabledState](state-layout/06-FieldHudDetailForm-Panel_PageSensorRecords-SensorRecordsDisabledState.png) | 已检查 | application、memory |

效果图要求的代表态：[soil](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-06-SensorRecords-soil.png)、[scanner](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-06-SensorRecords-scanner.png)、[remote](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-06-SensorRecords-remote.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 032 / 06 / 水域详情 · FieldHudDetailForm/Panel_PageWater

Prefab：[Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-06-Water-normal.png)；证据：[application](runtime/06-FieldHudDetailForm-Panel_PageWater-application.png) · [memory](runtime/06-FieldHudDetailForm-Panel_PageWater-memory.png) · [real-world/natural-selection-and-page-API](runtime/06-FieldHudDetailForm-Panel_PageWater-natural.png) · [world](runtime/06-FieldHudDetailForm-Panel_PageWater-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 发现问题 | UIA-0124, UIA-0125 |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0125, UIA-0126, UIA-0127 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0125, UIA-0126, UIA-0127, UIA-0128 |
| 5. 九宫格与边角 | 发现问题 | UIA-0128 |
| 6. 对齐间距留白 | 发现问题 | UIA-0127, UIA-0128 |
| 7. 文字与可读性 | 发现问题 | UIA-0124, UIA-0125, UIA-0126, UIA-0127, UIA-0128 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0124, UIA-0125, UIA-0127 |
| 9. 素材与状态语义 | 发现问题 | UIA-0128 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0126, UIA-0127 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0124, UIA-0125, UIA-0126, UIA-0128 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0124 / P1 / 已确认 | **DETAIL_FIELDS_WRONG_SECTION** · `公开状态与生产记录列表`<br>对象名称、类别、状态等公开字段渲染在生产记录列表中；公开状态卡保留“作物/产量”等占位正文，字段分区与阅读顺序不符。<br>期望：满足标准 1,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/06-FieldHudDetailForm-Panel_PageWater-natural.png) · [证据2](geometry/06-FieldHudDetailForm-Panel_PageWater-natural.json) |
| UIA-0125 / P1 / 已确认 | **DETAIL_ROW_DEFAULT_HIDDEN** · `现场详情主数据列表`<br>选中有效现场对象后，条目在说明文本/控件之后；列表可视区仅约42px，而记录行104px，默认只见“作物/功率/容量”等占位提示，真实字段几乎不见。<br>期望：满足标准 1,3,4,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/06-FieldHudDetailForm-Panel_PageWater-natural.png) · [证据2](geometry/06-FieldHudDetailForm-Panel_PageWater-natural.json) |
| UIA-0126 / P1 / 已确认 | **SCROLL_ROW_TOO_TALL** · `FieldHudDetailForm/Grp_PageHost/Panel_PageWater/Panel_WaterRecord/List_WaterRecord/Viewport_WaterRecord/Content_WaterRecord/Item_WaterPublicTemplate/Btn_WaterPublicRow`<br>单条记录高 104.0px，大于列表可视高度 42.0px；无法完整显示一条记录<br>期望：满足标准 3,4,7,10,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/06-FieldHudDetailForm-Panel_PageWater-natural.json) · [证据2](geometry/06-FieldHudDetailForm-Panel_PageWater-selected.json) |
| UIA-0127 / P1 / 已确认 | **SIDEBAR_ACTION_DENSITY** · `侧栏底部操作区`<br>底部按钮直接叠在装饰框底沿，同一行容纳多个长动作名；端帽占用大部分宽度，文字与操作层级不清楚。<br>期望：满足标准 3,4,6,7,8,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/06-FieldHudDetailForm-Panel_PageWater.png) |
| UIA-0128 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `FieldHudDetailForm/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/06-FieldHudDetailForm-Panel_PageWater-application.json) · [证据2](geometry/06-FieldHudDetailForm-Panel_PageWater-memory.json) · [证据3](geometry/06-FieldHudDetailForm-Panel_PageWater-scroll-bottom.json) · [证据4](geometry/06-FieldHudDetailForm-Panel_PageWater-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_WaterLoadingState](state-layout/06-FieldHudDetailForm-Panel_PageWater-WaterLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_WaterEmptyState](state-layout/06-FieldHudDetailForm-Panel_PageWater-WaterEmptyState.png) | 已检查 | world |
| [Grp_WaterErrorState](state-layout/06-FieldHudDetailForm-Panel_PageWater-WaterErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_WaterSuccessState](state-layout/06-FieldHudDetailForm-Panel_PageWater-WaterSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_WaterDisabledState](state-layout/06-FieldHudDetailForm-Panel_PageWater-WaterDisabledState.png) | 已检查 | application、memory |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-06-Water-normal.png)、[infinite](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-06-Water-infinite.png)、[depleted](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-06-Water-depleted.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 033 / 07 / 蓄电池 · FieldHudDetailForm/Panel_PageBattery

Prefab：[Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-07-Battery-complete.png)；证据：[application](runtime/07-FieldHudDetailForm-Panel_PageBattery-application.png) · [memory](runtime/07-FieldHudDetailForm-Panel_PageBattery-memory.png) · [world](runtime/07-FieldHudDetailForm-Panel_PageBattery-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0129 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0129, UIA-0130 |
| 5. 九宫格与边角 | 发现问题 | UIA-0130 |
| 6. 对齐间距留白 | 发现问题 | UIA-0129, UIA-0130, UIA-0131 |
| 7. 文字与可读性 | 发现问题 | UIA-0129, UIA-0130 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0129, UIA-0131 |
| 9. 素材与状态语义 | 发现问题 | UIA-0130 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0129 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0130, UIA-0131 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0129 / P1 / 已确认 | **SIDEBAR_ACTION_DENSITY** · `侧栏底部操作区`<br>底部按钮直接叠在装饰框底沿，同一行容纳多个长动作名；端帽占用大部分宽度，文字与操作层级不清楚。<br>期望：满足标准 3,4,6,7,8,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/07-FieldHudDetailForm-Panel_PageBattery.png) |
| UIA-0130 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `FieldHudDetailForm/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/07-FieldHudDetailForm-Panel_PageBattery-application.json) · [证据2](geometry/07-FieldHudDetailForm-Panel_PageBattery-memory.json) · [证据3](geometry/07-FieldHudDetailForm-Panel_PageBattery-scroll-bottom.json) · [证据4](geometry/07-FieldHudDetailForm-Panel_PageBattery-scroll-top.json) |
| UIA-0131 / P2 / 已确认 | **SIDEBAR_UNUSED_AREA** · `侧栏内容区`<br>主要字段集中在中段的少数窄条卡片，上下留白较大；需要结合动态内容重新分配高度，不能仅按图片摆放后再塞文字。<br>期望：满足标准 6,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/07-FieldHudDetailForm-Panel_PageBattery.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_BatteryLoadingState](state-layout/07-FieldHudDetailForm-Panel_PageBattery-BatteryLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_BatteryEmptyState](state-layout/07-FieldHudDetailForm-Panel_PageBattery-BatteryEmptyState.png) | 已检查 | world |
| [Grp_BatteryErrorState](state-layout/07-FieldHudDetailForm-Panel_PageBattery-BatteryErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_BatterySuccessState](state-layout/07-FieldHudDetailForm-Panel_PageBattery-BatterySuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_BatteryDisabledState](state-layout/07-FieldHudDetailForm-Panel_PageBattery-BatteryDisabledState.png) | 已检查 | application、memory |

效果图要求的代表态：[complete](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-07-Battery-complete.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 034 / 07 / 通用建筑现场面板 · FieldHudDetailForm/Panel_PageBuildingOverview

Prefab：[Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-07-BuildingOverview-complete.png)；证据：[application](runtime/07-FieldHudDetailForm-Panel_PageBuildingOverview-application.png) · [memory](runtime/07-FieldHudDetailForm-Panel_PageBuildingOverview-memory.png) · [world](runtime/07-FieldHudDetailForm-Panel_PageBuildingOverview-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0132 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0132, UIA-0133 |
| 5. 九宫格与边角 | 发现问题 | UIA-0133 |
| 6. 对齐间距留白 | 发现问题 | UIA-0132, UIA-0133 |
| 7. 文字与可读性 | 发现问题 | UIA-0132, UIA-0133 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0132 |
| 9. 素材与状态语义 | 发现问题 | UIA-0133 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0132 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0133 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0132 / P1 / 已确认 | **SIDEBAR_ACTION_DENSITY** · `侧栏底部操作区`<br>底部按钮直接叠在装饰框底沿，同一行容纳多个长动作名；端帽占用大部分宽度，文字与操作层级不清楚。<br>期望：满足标准 3,4,6,7,8,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/07-FieldHudDetailForm-Panel_PageBuildingOverview.png) |
| UIA-0133 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `FieldHudDetailForm/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/07-FieldHudDetailForm-Panel_PageBuildingOverview-application.json) · [证据2](geometry/07-FieldHudDetailForm-Panel_PageBuildingOverview-memory.json) · [证据3](geometry/07-FieldHudDetailForm-Panel_PageBuildingOverview-scroll-bottom.json) · [证据4](geometry/07-FieldHudDetailForm-Panel_PageBuildingOverview-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_BuildingOverviewLoadingState](state-layout/07-FieldHudDetailForm-Panel_PageBuildingOverview-BuildingOverviewLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_BuildingOverviewEmptyState](state-layout/07-FieldHudDetailForm-Panel_PageBuildingOverview-BuildingOverviewEmptyState.png) | 已检查 | world |
| [Grp_BuildingOverviewErrorState](state-layout/07-FieldHudDetailForm-Panel_PageBuildingOverview-BuildingOverviewErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_BuildingOverviewSuccessState](state-layout/07-FieldHudDetailForm-Panel_PageBuildingOverview-BuildingOverviewSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_BuildingOverviewDisabledState](state-layout/07-FieldHudDetailForm-Panel_PageBuildingOverview-BuildingOverviewDisabledState.png) | 已检查 | application、memory |

效果图要求的代表态：[complete](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-07-BuildingOverview-complete.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 035 / 07 / 施工建筑 · FieldHudDetailForm/Panel_PageConstruction

Prefab：[Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-07-Construction-preview.png)；证据：[application](runtime/07-FieldHudDetailForm-Panel_PageConstruction-application.png) · [memory](runtime/07-FieldHudDetailForm-Panel_PageConstruction-memory.png) · [world](runtime/07-FieldHudDetailForm-Panel_PageConstruction-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0134 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0134, UIA-0135 |
| 5. 九宫格与边角 | 发现问题 | UIA-0135 |
| 6. 对齐间距留白 | 发现问题 | UIA-0134, UIA-0135 |
| 7. 文字与可读性 | 发现问题 | UIA-0134, UIA-0135 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0134 |
| 9. 素材与状态语义 | 发现问题 | UIA-0135 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0134 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0135 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0134 / P1 / 已确认 | **SIDEBAR_ACTION_DENSITY** · `侧栏底部操作区`<br>底部按钮直接叠在装饰框底沿，同一行容纳多个长动作名；端帽占用大部分宽度，文字与操作层级不清楚。<br>期望：满足标准 3,4,6,7,8,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/07-FieldHudDetailForm-Panel_PageConstruction.png) |
| UIA-0135 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `FieldHudDetailForm/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/07-FieldHudDetailForm-Panel_PageConstruction-application.json) · [证据2](geometry/07-FieldHudDetailForm-Panel_PageConstruction-memory.json) · [证据3](geometry/07-FieldHudDetailForm-Panel_PageConstruction-scroll-bottom.json) · [证据4](geometry/07-FieldHudDetailForm-Panel_PageConstruction-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_ConstructionLoadingState](state-layout/07-FieldHudDetailForm-Panel_PageConstruction-ConstructionLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ConstructionEmptyState](state-layout/07-FieldHudDetailForm-Panel_PageConstruction-ConstructionEmptyState.png) | 已检查 | world |
| [Grp_ConstructionErrorState](state-layout/07-FieldHudDetailForm-Panel_PageConstruction-ConstructionErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ConstructionSuccessState](state-layout/07-FieldHudDetailForm-Panel_PageConstruction-ConstructionSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ConstructionDisabledState](state-layout/07-FieldHudDetailForm-Panel_PageConstruction-ConstructionDisabledState.png) | 已检查 | application、memory |

效果图要求的代表态：[preview](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-07-Construction-preview.png)、[building](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-07-Construction-building.png)、[complete](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-07-Construction-complete.png)、[blocked](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-07-Construction-blocked.png)、[building](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-07.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 036 / 07 / 传送带设备详情 · FieldHudDetailForm/Panel_PageConveyor

Prefab：[Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-07-Conveyor-complete.png)；证据：[application](runtime/07-FieldHudDetailForm-Panel_PageConveyor-application.png) · [memory](runtime/07-FieldHudDetailForm-Panel_PageConveyor-memory.png) · [world](runtime/07-FieldHudDetailForm-Panel_PageConveyor-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0136 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0136, UIA-0137 |
| 5. 九宫格与边角 | 发现问题 | UIA-0137 |
| 6. 对齐间距留白 | 发现问题 | UIA-0136, UIA-0137 |
| 7. 文字与可读性 | 发现问题 | UIA-0136, UIA-0137 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0136 |
| 9. 素材与状态语义 | 发现问题 | UIA-0137 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0136 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0137 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0136 / P1 / 已确认 | **SIDEBAR_ACTION_DENSITY** · `侧栏底部操作区`<br>底部按钮直接叠在装饰框底沿，同一行容纳多个长动作名；端帽占用大部分宽度，文字与操作层级不清楚。<br>期望：满足标准 3,4,6,7,8,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/07-FieldHudDetailForm-Panel_PageConveyor.png) |
| UIA-0137 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `FieldHudDetailForm/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/07-FieldHudDetailForm-Panel_PageConveyor-application.json) · [证据2](geometry/07-FieldHudDetailForm-Panel_PageConveyor-memory.json) · [证据3](geometry/07-FieldHudDetailForm-Panel_PageConveyor-scroll-bottom.json) · [证据4](geometry/07-FieldHudDetailForm-Panel_PageConveyor-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_ConveyorLoadingState](state-layout/07-FieldHudDetailForm-Panel_PageConveyor-ConveyorLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ConveyorEmptyState](state-layout/07-FieldHudDetailForm-Panel_PageConveyor-ConveyorEmptyState.png) | 已检查 | world |
| [Grp_ConveyorErrorState](state-layout/07-FieldHudDetailForm-Panel_PageConveyor-ConveyorErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ConveyorSuccessState](state-layout/07-FieldHudDetailForm-Panel_PageConveyor-ConveyorSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ConveyorDisabledState](state-layout/07-FieldHudDetailForm-Panel_PageConveyor-ConveyorDisabledState.png) | 已检查 | application、memory |

效果图要求的代表态：[complete](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-07-Conveyor-complete.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 037 / 07 / 生物质发电机 · FieldHudDetailForm/Panel_PageGenerator

Prefab：[Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-07-Generator-complete.png)；证据：[application](runtime/07-FieldHudDetailForm-Panel_PageGenerator-application.png) · [memory](runtime/07-FieldHudDetailForm-Panel_PageGenerator-memory.png) · [real-world/natural-selection-and-page-API](runtime/07-FieldHudDetailForm-Panel_PageGenerator-natural.png) · [world](runtime/07-FieldHudDetailForm-Panel_PageGenerator-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 发现问题 | UIA-0138 |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0138, UIA-0139, UIA-0140, UIA-0141, UIA-0142 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0138, UIA-0139, UIA-0140, UIA-0141, UIA-0142, UIA-0143 |
| 5. 九宫格与边角 | 发现问题 | UIA-0143 |
| 6. 对齐间距留白 | 发现问题 | UIA-0142, UIA-0143, UIA-0144 |
| 7. 文字与可读性 | 发现问题 | UIA-0138, UIA-0139, UIA-0140, UIA-0141, UIA-0142, UIA-0143 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0138, UIA-0142, UIA-0144 |
| 9. 素材与状态语义 | 发现问题 | UIA-0143 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0139, UIA-0140, UIA-0141, UIA-0142 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0138, UIA-0139, UIA-0140, UIA-0141, UIA-0143, UIA-0144 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0138 / P1 / 已确认 | **DETAIL_ROW_DEFAULT_HIDDEN** · `现场详情主数据列表`<br>选中有效现场对象后，条目在说明文本/控件之后；列表可视区仅约42px，而记录行104px，默认只见“作物/功率/容量”等占位提示，真实字段几乎不见。<br>期望：满足标准 1,3,4,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/07-FieldHudDetailForm-Panel_PageGenerator-natural.png) · [证据2](geometry/07-FieldHudDetailForm-Panel_PageGenerator-natural.json) |
| UIA-0139 / P1 / 已确认 | **SCROLL_ROW_TOO_TALL** · `FieldHudDetailForm/Grp_PageHost/Panel_PageGenerator/Panel_GeneratorPower/List_GeneratorPower/Viewport_GeneratorPower/Content_GeneratorPower/Item_GeneratorPowerTemplate/Btn_GeneratorPowerRow`<br>单条记录高 104.0px，大于列表可视高度 42.0px；无法完整显示一条记录<br>期望：满足标准 3,4,7,10,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/07-FieldHudDetailForm-Panel_PageGenerator-natural.json) · [证据2](geometry/07-FieldHudDetailForm-Panel_PageGenerator-selected.json) |
| UIA-0140 / P1 / 已确认 | **SCROLL_ROW_TOO_TALL** · `FieldHudDetailForm/Grp_PageHost/Panel_PageGenerator/Panel_GeneratorPower/List_GeneratorPower/Viewport_GeneratorPower/Content_GeneratorPower/Panel_GeneratorControls/Sld_GeneratorChargeTarget`<br>单条记录高 56.0px，大于列表可视高度 42.0px；无法完整显示一条记录<br>期望：满足标准 3,4,7,10,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/07-FieldHudDetailForm-Panel_PageGenerator-natural.json) · [证据2](geometry/07-FieldHudDetailForm-Panel_PageGenerator-scroll-bottom.json) · [证据3](geometry/07-FieldHudDetailForm-Panel_PageGenerator-scroll-top.json) · [证据4](geometry/07-FieldHudDetailForm-Panel_PageGenerator-selected.json) |
| UIA-0141 / P1 / 已确认 | **SCROLL_ROW_TOO_TALL** · `FieldHudDetailForm/Grp_PageHost/Panel_PageGenerator/Panel_GeneratorPower/List_GeneratorPower/Viewport_GeneratorPower/Content_GeneratorPower/Panel_GeneratorControls/Tgl_GeneratorChargingAllowed`<br>单条记录高 48.0px，大于列表可视高度 42.0px；无法完整显示一条记录<br>期望：满足标准 3,4,7,10,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/07-FieldHudDetailForm-Panel_PageGenerator-natural.json) · [证据2](geometry/07-FieldHudDetailForm-Panel_PageGenerator-scroll-bottom.json) · [证据3](geometry/07-FieldHudDetailForm-Panel_PageGenerator-scroll-top.json) · [证据4](geometry/07-FieldHudDetailForm-Panel_PageGenerator-selected.json) |
| UIA-0142 / P1 / 已确认 | **SIDEBAR_ACTION_DENSITY** · `侧栏底部操作区`<br>底部按钮直接叠在装饰框底沿，同一行容纳多个长动作名；端帽占用大部分宽度，文字与操作层级不清楚。<br>期望：满足标准 3,4,6,7,8,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/07-FieldHudDetailForm-Panel_PageGenerator.png) |
| UIA-0143 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `FieldHudDetailForm/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/07-FieldHudDetailForm-Panel_PageGenerator-application.json) · [证据2](geometry/07-FieldHudDetailForm-Panel_PageGenerator-memory.json) · [证据3](geometry/07-FieldHudDetailForm-Panel_PageGenerator-scroll-bottom.json) · [证据4](geometry/07-FieldHudDetailForm-Panel_PageGenerator-scroll-top.json) |
| UIA-0144 / P2 / 已确认 | **SIDEBAR_UNUSED_AREA** · `侧栏内容区`<br>主要字段集中在中段的少数窄条卡片，上下留白较大；需要结合动态内容重新分配高度，不能仅按图片摆放后再塞文字。<br>期望：满足标准 6,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/07-FieldHudDetailForm-Panel_PageGenerator.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_GeneratorLoadingState](state-layout/07-FieldHudDetailForm-Panel_PageGenerator-GeneratorLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_GeneratorEmptyState](state-layout/07-FieldHudDetailForm-Panel_PageGenerator-GeneratorEmptyState.png) | 已检查 | world |
| [Grp_GeneratorErrorState](state-layout/07-FieldHudDetailForm-Panel_PageGenerator-GeneratorErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_GeneratorSuccessState](state-layout/07-FieldHudDetailForm-Panel_PageGenerator-GeneratorSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_GeneratorDisabledState](state-layout/07-FieldHudDetailForm-Panel_PageGenerator-GeneratorDisabledState.png) | 已检查 | application、memory |

效果图要求的代表态：[complete](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-07-Generator-complete.png)、[blackout](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-07-Generator-blackout.png)、[overload](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-07-Generator-overload.png)、[damaged](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-07-Generator-damaged.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 038 / 07 / 水泵 · FieldHudDetailForm/Panel_PagePump

Prefab：[Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-07-Pump-complete.png)；证据：[application](runtime/07-FieldHudDetailForm-Panel_PagePump-application.png) · [memory](runtime/07-FieldHudDetailForm-Panel_PagePump-memory.png) · [world](runtime/07-FieldHudDetailForm-Panel_PagePump-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0145 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0145, UIA-0146 |
| 5. 九宫格与边角 | 发现问题 | UIA-0146 |
| 6. 对齐间距留白 | 发现问题 | UIA-0145, UIA-0146, UIA-0147 |
| 7. 文字与可读性 | 发现问题 | UIA-0145, UIA-0146 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0145, UIA-0147 |
| 9. 素材与状态语义 | 发现问题 | UIA-0146 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0145 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0146, UIA-0147 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0145 / P1 / 已确认 | **SIDEBAR_ACTION_DENSITY** · `侧栏底部操作区`<br>底部按钮直接叠在装饰框底沿，同一行容纳多个长动作名；端帽占用大部分宽度，文字与操作层级不清楚。<br>期望：满足标准 3,4,6,7,8,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/07-FieldHudDetailForm-Panel_PagePump.png) |
| UIA-0146 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `FieldHudDetailForm/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/07-FieldHudDetailForm-Panel_PagePump-application.json) · [证据2](geometry/07-FieldHudDetailForm-Panel_PagePump-memory.json) · [证据3](geometry/07-FieldHudDetailForm-Panel_PagePump-scroll-bottom.json) · [证据4](geometry/07-FieldHudDetailForm-Panel_PagePump-scroll-top.json) |
| UIA-0147 / P2 / 已确认 | **SIDEBAR_UNUSED_AREA** · `侧栏内容区`<br>主要字段集中在中段的少数窄条卡片，上下留白较大；需要结合动态内容重新分配高度，不能仅按图片摆放后再塞文字。<br>期望：满足标准 6,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/07-FieldHudDetailForm-Panel_PagePump.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_PumpLoadingState](state-layout/07-FieldHudDetailForm-Panel_PagePump-PumpLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_PumpEmptyState](state-layout/07-FieldHudDetailForm-Panel_PagePump-PumpEmptyState.png) | 已检查 | world |
| [Grp_PumpErrorState](state-layout/07-FieldHudDetailForm-Panel_PagePump-PumpErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_PumpSuccessState](state-layout/07-FieldHudDetailForm-Panel_PagePump-PumpSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_PumpDisabledState](state-layout/07-FieldHudDetailForm-Panel_PagePump-PumpDisabledState.png) | 已检查 | application、memory |

效果图要求的代表态：[complete](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-07-Pump-complete.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 039 / 07 / 初始太阳能设施 · FieldHudDetailForm/Panel_PageSolar

Prefab：[Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-07-Solar-complete.png)；证据：[application](runtime/07-FieldHudDetailForm-Panel_PageSolar-application.png) · [memory](runtime/07-FieldHudDetailForm-Panel_PageSolar-memory.png) · [real-world/natural-selection-and-page-API](runtime/07-FieldHudDetailForm-Panel_PageSolar-natural.png) · [world](runtime/07-FieldHudDetailForm-Panel_PageSolar-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 发现问题 | UIA-0148 |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0148, UIA-0149, UIA-0150 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0148, UIA-0149, UIA-0150, UIA-0151 |
| 5. 九宫格与边角 | 发现问题 | UIA-0151 |
| 6. 对齐间距留白 | 发现问题 | UIA-0150, UIA-0151, UIA-0152 |
| 7. 文字与可读性 | 发现问题 | UIA-0148, UIA-0149, UIA-0150, UIA-0151 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0148, UIA-0150, UIA-0152 |
| 9. 素材与状态语义 | 发现问题 | UIA-0151 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0149, UIA-0150 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0148, UIA-0149, UIA-0151, UIA-0152 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0148 / P1 / 已确认 | **DETAIL_ROW_DEFAULT_HIDDEN** · `现场详情主数据列表`<br>选中有效现场对象后，条目在说明文本/控件之后；列表可视区仅约42px，而记录行104px，默认只见“作物/功率/容量”等占位提示，真实字段几乎不见。<br>期望：满足标准 1,3,4,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/07-FieldHudDetailForm-Panel_PageSolar-natural.png) · [证据2](geometry/07-FieldHudDetailForm-Panel_PageSolar-natural.json) |
| UIA-0149 / P1 / 已确认 | **SCROLL_ROW_TOO_TALL** · `FieldHudDetailForm/Grp_PageHost/Panel_PageSolar/Panel_SolarPower/List_SolarPower/Viewport_SolarPower/Content_SolarPower/Item_SolarPowerTemplate/Btn_SolarPowerRow`<br>单条记录高 104.0px，大于列表可视高度 42.0px；无法完整显示一条记录<br>期望：满足标准 3,4,7,10,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/07-FieldHudDetailForm-Panel_PageSolar-natural.json) · [证据2](geometry/07-FieldHudDetailForm-Panel_PageSolar-selected.json) |
| UIA-0150 / P1 / 已确认 | **SIDEBAR_ACTION_DENSITY** · `侧栏底部操作区`<br>底部按钮直接叠在装饰框底沿，同一行容纳多个长动作名；端帽占用大部分宽度，文字与操作层级不清楚。<br>期望：满足标准 3,4,6,7,8,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/07-FieldHudDetailForm-Panel_PageSolar.png) |
| UIA-0151 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `FieldHudDetailForm/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/07-FieldHudDetailForm-Panel_PageSolar-application.json) · [证据2](geometry/07-FieldHudDetailForm-Panel_PageSolar-memory.json) · [证据3](geometry/07-FieldHudDetailForm-Panel_PageSolar-scroll-bottom.json) · [证据4](geometry/07-FieldHudDetailForm-Panel_PageSolar-scroll-top.json) |
| UIA-0152 / P2 / 已确认 | **SIDEBAR_UNUSED_AREA** · `侧栏内容区`<br>主要字段集中在中段的少数窄条卡片，上下留白较大；需要结合动态内容重新分配高度，不能仅按图片摆放后再塞文字。<br>期望：满足标准 6,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/07-FieldHudDetailForm-Panel_PageSolar.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_SolarLoadingState](state-layout/07-FieldHudDetailForm-Panel_PageSolar-SolarLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_SolarEmptyState](state-layout/07-FieldHudDetailForm-Panel_PageSolar-SolarEmptyState.png) | 已检查 | world |
| [Grp_SolarErrorState](state-layout/07-FieldHudDetailForm-Panel_PageSolar-SolarErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_SolarSuccessState](state-layout/07-FieldHudDetailForm-Panel_PageSolar-SolarSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_SolarDisabledState](state-layout/07-FieldHudDetailForm-Panel_PageSolar-SolarDisabledState.png) | 已检查 | application、memory |

效果图要求的代表态：[complete](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-07-Solar-complete.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 040 / 07 / 仓库现场概要 · FieldHudDetailForm/Panel_PageWarehouseBuilding

Prefab：[Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-07-WarehouseBuilding-complete.png)；证据：[application](runtime/07-FieldHudDetailForm-Panel_PageWarehouseBuilding-application.png) · [memory](runtime/07-FieldHudDetailForm-Panel_PageWarehouseBuilding-memory.png) · [real-world/natural-selection-and-page-API](runtime/07-FieldHudDetailForm-Panel_PageWarehouseBuilding-natural.png) · [world](runtime/07-FieldHudDetailForm-Panel_PageWarehouseBuilding-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 发现问题 | UIA-0153 |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0153, UIA-0154, UIA-0155 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0153, UIA-0154, UIA-0155, UIA-0156 |
| 5. 九宫格与边角 | 发现问题 | UIA-0156 |
| 6. 对齐间距留白 | 发现问题 | UIA-0155, UIA-0156, UIA-0157 |
| 7. 文字与可读性 | 发现问题 | UIA-0153, UIA-0154, UIA-0155, UIA-0156 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0153, UIA-0155, UIA-0157 |
| 9. 素材与状态语义 | 发现问题 | UIA-0156 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0154, UIA-0155 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0153, UIA-0154, UIA-0156, UIA-0157 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0153 / P1 / 已确认 | **DETAIL_ROW_DEFAULT_HIDDEN** · `现场详情主数据列表`<br>选中有效现场对象后，条目在说明文本/控件之后；列表可视区仅约42px，而记录行104px，默认只见“作物/功率/容量”等占位提示，真实字段几乎不见。<br>期望：满足标准 1,3,4,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/07-FieldHudDetailForm-Panel_PageWarehouseBuilding-natural.png) · [证据2](geometry/07-FieldHudDetailForm-Panel_PageWarehouseBuilding-natural.json) |
| UIA-0154 / P1 / 已确认 | **SCROLL_ROW_TOO_TALL** · `FieldHudDetailForm/Grp_PageHost/Panel_PageWarehouseBuilding/Panel_WarehouseBuildingCapacity/List_WarehouseBuildingCapacity/Viewport_WarehouseBuildingCapacity/Content_WarehouseBuildingCapacity/Item_WarehouseBuildingCapacityTemplate/Btn_WarehouseBuildingCapacityRow`<br>单条记录高 104.0px，大于列表可视高度 42.0px；无法完整显示一条记录<br>期望：满足标准 3,4,7,10,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/07-FieldHudDetailForm-Panel_PageWarehouseBuilding-natural.json) · [证据2](geometry/07-FieldHudDetailForm-Panel_PageWarehouseBuilding-selected.json) |
| UIA-0155 / P1 / 已确认 | **SIDEBAR_ACTION_DENSITY** · `侧栏底部操作区`<br>底部按钮直接叠在装饰框底沿，同一行容纳多个长动作名；端帽占用大部分宽度，文字与操作层级不清楚。<br>期望：满足标准 3,4,6,7,8,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/07-FieldHudDetailForm-Panel_PageWarehouseBuilding.png) |
| UIA-0156 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `FieldHudDetailForm/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/07-FieldHudDetailForm-Panel_PageWarehouseBuilding-application.json) · [证据2](geometry/07-FieldHudDetailForm-Panel_PageWarehouseBuilding-memory.json) · [证据3](geometry/07-FieldHudDetailForm-Panel_PageWarehouseBuilding-scroll-bottom.json) · [证据4](geometry/07-FieldHudDetailForm-Panel_PageWarehouseBuilding-scroll-top.json) |
| UIA-0157 / P2 / 已确认 | **SIDEBAR_UNUSED_AREA** · `侧栏内容区`<br>主要字段集中在中段的少数窄条卡片，上下留白较大；需要结合动态内容重新分配高度，不能仅按图片摆放后再塞文字。<br>期望：满足标准 6,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/07-FieldHudDetailForm-Panel_PageWarehouseBuilding.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_WarehouseBuildingLoadingState](state-layout/07-FieldHudDetailForm-Panel_PageWarehouseBuilding-WarehouseBuildingLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_WarehouseBuildingEmptyState](state-layout/07-FieldHudDetailForm-Panel_PageWarehouseBuilding-WarehouseBuildingEmptyState.png) | 已检查 | world |
| [Grp_WarehouseBuildingErrorState](state-layout/07-FieldHudDetailForm-Panel_PageWarehouseBuilding-WarehouseBuildingErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_WarehouseBuildingSuccessState](state-layout/07-FieldHudDetailForm-Panel_PageWarehouseBuilding-WarehouseBuildingSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_WarehouseBuildingDisabledState](state-layout/07-FieldHudDetailForm-Panel_PageWarehouseBuilding-WarehouseBuildingDisabledState.png) | 已检查 | application、memory |

效果图要求的代表态：[complete](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-07-WarehouseBuilding-complete.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 041 / 08 / 当前制造与等待队列 · WorkshopForm/Panel_PageManufacturingQueue

Prefab：[Assets/Game/Prefabs/UI/Operations/WorkshopForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/WorkshopForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/WorkshopForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/WorkshopForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-08-ManufacturingQueue-running.png)；证据：[application](runtime/08-WorkshopForm-Panel_PageManufacturingQueue-application.png) · [memory](runtime/08-WorkshopForm-Panel_PageManufacturingQueue-memory.png) · [world](runtime/08-WorkshopForm-Panel_PageManufacturingQueue-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0163 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0159, UIA-0160, UIA-0161, UIA-0162 |
| 5. 九宫格与边角 | 发现问题 | UIA-0159 |
| 6. 对齐间距留白 | 发现问题 | UIA-0159, UIA-0162 |
| 7. 文字与可读性 | 发现问题 | UIA-0159, UIA-0160, UIA-0161, UIA-0162 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0162, UIA-0163 |
| 9. 素材与状态语义 | 发现问题 | UIA-0158, UIA-0159 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0158, UIA-0159, UIA-0160, UIA-0162, UIA-0163 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0158 / P1 / 业务缺口 | **BUSINESS_SOURCE_NOT_CONNECTED** · `运行时数据正文`<br>本次实际运行说明：工坊与配方域尚未接入运行路径：配方、队列与产出都没有创建者。<br>期望：满足标准 9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/08-WorkshopForm-Panel_PageManufacturingQueue-world.png) · [证据2](geometry/08-WorkshopForm-Panel_PageManufacturingQueue-world.json) |
| UIA-0159 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `WorkshopForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/08-WorkshopForm-Panel_PageManufacturingQueue-application.json) · [证据2](geometry/08-WorkshopForm-Panel_PageManufacturingQueue-memory.json) · [证据3](geometry/08-WorkshopForm-Panel_PageManufacturingQueue-scroll-top.json) · [证据4](geometry/08-WorkshopForm-Panel_PageManufacturingQueue-state-ManufacturingQueueDisabledState.json) |
| UIA-0160 / P1 / 已确认 | **TEXT_MASK_CLIP** · `WorkshopForm/Panel_Frame/Grp_PageHost/Panel_PageManufacturingQueue/Panel_ManufacturingQueueCurrent/List_ManufacturingQueueCurrent/Viewport_ManufacturingQueueCurrent/Content_ManufacturingQueueCurrent/Txt_ManufacturingQueueCurrentBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/08-WorkshopForm-Panel_PageManufacturingQueue-application.json) · [证据2](geometry/08-WorkshopForm-Panel_PageManufacturingQueue-memory.json) · [证据3](geometry/08-WorkshopForm-Panel_PageManufacturingQueue-scroll-top.json) · [证据4](geometry/08-WorkshopForm-Panel_PageManufacturingQueue-world-scroll-top.json) |
| UIA-0161 / P1 / 已确认 | **TEXT_OVERLAP** · `WorkshopForm/Panel_Frame/Grp_PageHost/Panel_PageManufacturingQueue/Txt_ManufacturingQueueTitle ↔ WorkshopForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/08-WorkshopForm-Panel_PageManufacturingQueue-application.json) · [证据2](geometry/08-WorkshopForm-Panel_PageManufacturingQueue-memory.json) · [证据3](geometry/08-WorkshopForm-Panel_PageManufacturingQueue-scroll-top.json) · [证据4](geometry/08-WorkshopForm-Panel_PageManufacturingQueue-state-base.json) |
| UIA-0162 / P2 / 已确认 | **UNAVAILABLE_REASON_REPEATED** · `WorkshopForm/Panel_Frame/Grp_PageHost/Panel_PageManufacturingQueue/Grp_ManufacturingQueueDisabledState/Panel_ManufacturingQueueDisabledMessage/Txt_ManufacturingQueueDisabledMessage`<br>相同不可用原因同时出现在状态条和多个信息卡中；浮动状态条压在插画/内容区域，重复文字分散焦点。<br>期望：满足标准 4,6,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/08-WorkshopForm-Panel_PageManufacturingQueue-world.json) · [证据2](runtime/08-WorkshopForm-Panel_PageManufacturingQueue-world.png) |
| UIA-0163 / P2 / 已确认 | **WORKSHOP_ART_PRIORITY** · `制造工坊各页的台座插画`<br>台座插画占据队列、阻塞说明或成本信息区域的大块空间，动态信息区的可读面积不足，需检查实际条目与图片共同出现时的排布。<br>期望：满足标准 3,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/08-WorkshopForm-Panel_PageManufacturingQueue.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_ManufacturingQueueLoadingState](state-layout/08-WorkshopForm-Panel_PageManufacturingQueue-ManufacturingQueueLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ManufacturingQueueEmptyState](state-layout/08-WorkshopForm-Panel_PageManufacturingQueue-ManufacturingQueueEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ManufacturingQueueErrorState](state-layout/08-WorkshopForm-Panel_PageManufacturingQueue-ManufacturingQueueErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ManufacturingQueueSuccessState](state-layout/08-WorkshopForm-Panel_PageManufacturingQueue-ManufacturingQueueSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ManufacturingQueueDisabledState](state-layout/08-WorkshopForm-Panel_PageManufacturingQueue-ManufacturingQueueDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[running](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-08-ManufacturingQueue-running.png)、[material](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-08-ManufacturingQueue-material.png)、[output](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-08-ManufacturingQueue-output.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 042 / 08 / 输出缓存与阻塞 · WorkshopForm/Panel_PageOutputCache

Prefab：[Assets/Game/Prefabs/UI/Operations/WorkshopForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/WorkshopForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/WorkshopForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/WorkshopForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-08-OutputCache-normal.png)；证据：[application](runtime/08-WorkshopForm-Panel_PageOutputCache-application.png) · [memory](runtime/08-WorkshopForm-Panel_PageOutputCache-memory.png) · [world](runtime/08-WorkshopForm-Panel_PageOutputCache-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0169 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0165, UIA-0166, UIA-0167, UIA-0168 |
| 5. 九宫格与边角 | 发现问题 | UIA-0165 |
| 6. 对齐间距留白 | 发现问题 | UIA-0165, UIA-0168 |
| 7. 文字与可读性 | 发现问题 | UIA-0165, UIA-0166, UIA-0167, UIA-0168 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0168, UIA-0169 |
| 9. 素材与状态语义 | 发现问题 | UIA-0164, UIA-0165 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0164, UIA-0165, UIA-0166, UIA-0168, UIA-0169 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0164 / P1 / 业务缺口 | **BUSINESS_SOURCE_NOT_CONNECTED** · `运行时数据正文`<br>本次实际运行说明：工坊与配方域尚未接入运行路径：配方、队列与产出都没有创建者。<br>期望：满足标准 9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/08-WorkshopForm-Panel_PageOutputCache-world.png) · [证据2](geometry/08-WorkshopForm-Panel_PageOutputCache-world.json) |
| UIA-0165 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `WorkshopForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/08-WorkshopForm-Panel_PageOutputCache-application.json) · [证据2](geometry/08-WorkshopForm-Panel_PageOutputCache-memory.json) · [证据3](geometry/08-WorkshopForm-Panel_PageOutputCache-scroll-top.json) · [证据4](geometry/08-WorkshopForm-Panel_PageOutputCache-state-OutputCacheDisabledState.json) |
| UIA-0166 / P1 / 已确认 | **TEXT_MASK_CLIP** · `WorkshopForm/Panel_Frame/Grp_PageHost/Panel_PageOutputCache/Panel_OutputCacheItems/List_OutputCacheItems/Viewport_OutputCacheItems/Content_OutputCacheItems/Txt_OutputCacheItemsBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/08-WorkshopForm-Panel_PageOutputCache-application.json) · [证据2](geometry/08-WorkshopForm-Panel_PageOutputCache-memory.json) · [证据3](geometry/08-WorkshopForm-Panel_PageOutputCache-scroll-top.json) · [证据4](geometry/08-WorkshopForm-Panel_PageOutputCache-world-scroll-top.json) |
| UIA-0167 / P1 / 已确认 | **TEXT_OVERLAP** · `WorkshopForm/Panel_Frame/Grp_PageHost/Panel_PageOutputCache/Txt_OutputCacheTitle ↔ WorkshopForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/08-WorkshopForm-Panel_PageOutputCache-application.json) · [证据2](geometry/08-WorkshopForm-Panel_PageOutputCache-memory.json) · [证据3](geometry/08-WorkshopForm-Panel_PageOutputCache-scroll-top.json) · [证据4](geometry/08-WorkshopForm-Panel_PageOutputCache-state-base.json) |
| UIA-0168 / P2 / 已确认 | **UNAVAILABLE_REASON_REPEATED** · `WorkshopForm/Panel_Frame/Grp_PageHost/Panel_PageOutputCache/Grp_OutputCacheDisabledState/Panel_OutputCacheDisabledMessage/Txt_OutputCacheDisabledMessage`<br>相同不可用原因同时出现在状态条和多个信息卡中；浮动状态条压在插画/内容区域，重复文字分散焦点。<br>期望：满足标准 4,6,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/08-WorkshopForm-Panel_PageOutputCache-world.json) · [证据2](runtime/08-WorkshopForm-Panel_PageOutputCache-world.png) |
| UIA-0169 / P2 / 已确认 | **WORKSHOP_ART_PRIORITY** · `制造工坊各页的台座插画`<br>台座插画占据队列、阻塞说明或成本信息区域的大块空间，动态信息区的可读面积不足，需检查实际条目与图片共同出现时的排布。<br>期望：满足标准 3,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/08-WorkshopForm-Panel_PageOutputCache.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_OutputCacheLoadingState](state-layout/08-WorkshopForm-Panel_PageOutputCache-OutputCacheLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_OutputCacheEmptyState](state-layout/08-WorkshopForm-Panel_PageOutputCache-OutputCacheEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_OutputCacheErrorState](state-layout/08-WorkshopForm-Panel_PageOutputCache-OutputCacheErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_OutputCacheSuccessState](state-layout/08-WorkshopForm-Panel_PageOutputCache-OutputCacheSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_OutputCacheDisabledState](state-layout/08-WorkshopForm-Panel_PageOutputCache-OutputCacheDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-08-OutputCache-normal.png)、[near-full](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-08-OutputCache-near-full.png)、[full](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-08-OutputCache-full.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 043 / 08 / 配方详情 · WorkshopForm/Panel_PageRecipeDetail

Prefab：[Assets/Game/Prefabs/UI/Operations/WorkshopForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/WorkshopForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/WorkshopForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/WorkshopForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-08-RecipeDetail-ready.png)；证据：[application](runtime/08-WorkshopForm-Panel_PageRecipeDetail-application.png) · [memory](runtime/08-WorkshopForm-Panel_PageRecipeDetail-memory.png) · [world](runtime/08-WorkshopForm-Panel_PageRecipeDetail-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0175 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0171, UIA-0172, UIA-0173, UIA-0174 |
| 5. 九宫格与边角 | 发现问题 | UIA-0171 |
| 6. 对齐间距留白 | 发现问题 | UIA-0171, UIA-0174 |
| 7. 文字与可读性 | 发现问题 | UIA-0171, UIA-0172, UIA-0173, UIA-0174 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0174, UIA-0175 |
| 9. 素材与状态语义 | 发现问题 | UIA-0170, UIA-0171 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0170, UIA-0171, UIA-0172, UIA-0174, UIA-0175 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0170 / P1 / 业务缺口 | **BUSINESS_SOURCE_NOT_CONNECTED** · `运行时数据正文`<br>本次实际运行说明：工坊与配方域尚未接入运行路径：配方、队列与产出都没有创建者。<br>期望：满足标准 9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/08-WorkshopForm-Panel_PageRecipeDetail-world.png) · [证据2](geometry/08-WorkshopForm-Panel_PageRecipeDetail-world.json) |
| UIA-0171 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `WorkshopForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/08-WorkshopForm-Panel_PageRecipeDetail-application.json) · [证据2](geometry/08-WorkshopForm-Panel_PageRecipeDetail-memory.json) · [证据3](geometry/08-WorkshopForm-Panel_PageRecipeDetail-scroll-top.json) · [证据4](geometry/08-WorkshopForm-Panel_PageRecipeDetail-state-RecipeDetailDisabledState.json) |
| UIA-0172 / P1 / 已确认 | **TEXT_MASK_CLIP** · `WorkshopForm/Panel_Frame/Grp_PageHost/Panel_PageRecipeDetail/Panel_RecipeDetailProduct/List_RecipeDetailProduct/Viewport_RecipeDetailProduct/Content_RecipeDetailProduct/Txt_RecipeDetailProductBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/08-WorkshopForm-Panel_PageRecipeDetail-application.json) · [证据2](geometry/08-WorkshopForm-Panel_PageRecipeDetail-memory.json) · [证据3](geometry/08-WorkshopForm-Panel_PageRecipeDetail-scroll-top.json) · [证据4](geometry/08-WorkshopForm-Panel_PageRecipeDetail-world-scroll-top.json) |
| UIA-0173 / P1 / 已确认 | **TEXT_OVERLAP** · `WorkshopForm/Panel_Frame/Grp_PageHost/Panel_PageRecipeDetail/Txt_RecipeDetailTitle ↔ WorkshopForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/08-WorkshopForm-Panel_PageRecipeDetail-application.json) · [证据2](geometry/08-WorkshopForm-Panel_PageRecipeDetail-memory.json) · [证据3](geometry/08-WorkshopForm-Panel_PageRecipeDetail-scroll-top.json) · [证据4](geometry/08-WorkshopForm-Panel_PageRecipeDetail-state-base.json) |
| UIA-0174 / P2 / 已确认 | **UNAVAILABLE_REASON_REPEATED** · `WorkshopForm/Panel_Frame/Grp_PageHost/Panel_PageRecipeDetail/Grp_RecipeDetailDisabledState/Panel_RecipeDetailDisabledMessage/Txt_RecipeDetailDisabledMessage`<br>相同不可用原因同时出现在状态条和多个信息卡中；浮动状态条压在插画/内容区域，重复文字分散焦点。<br>期望：满足标准 4,6,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/08-WorkshopForm-Panel_PageRecipeDetail-world.json) · [证据2](runtime/08-WorkshopForm-Panel_PageRecipeDetail-world.png) |
| UIA-0175 / P2 / 已确认 | **WORKSHOP_ART_PRIORITY** · `制造工坊各页的台座插画`<br>台座插画占据队列、阻塞说明或成本信息区域的大块空间，动态信息区的可读面积不足，需检查实际条目与图片共同出现时的排布。<br>期望：满足标准 3,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/08-WorkshopForm-Panel_PageRecipeDetail.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_RecipeDetailLoadingState](state-layout/08-WorkshopForm-Panel_PageRecipeDetail-RecipeDetailLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_RecipeDetailEmptyState](state-layout/08-WorkshopForm-Panel_PageRecipeDetail-RecipeDetailEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_RecipeDetailErrorState](state-layout/08-WorkshopForm-Panel_PageRecipeDetail-RecipeDetailErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_RecipeDetailSuccessState](state-layout/08-WorkshopForm-Panel_PageRecipeDetail-RecipeDetailSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_RecipeDetailDisabledState](state-layout/08-WorkshopForm-Panel_PageRecipeDetail-RecipeDetailDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[ready](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-08-RecipeDetail-ready.png)、[material](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-08-RecipeDetail-material.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 044 / 08 / 制造工坊工作台 · WorkshopForm/Panel_PageWorkshop

Prefab：[Assets/Game/Prefabs/UI/Operations/WorkshopForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/WorkshopForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/WorkshopForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/WorkshopForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-08-Workshop-idle.png)；证据：[application](runtime/08-WorkshopForm-Panel_PageWorkshop-application.png) · [memory](runtime/08-WorkshopForm-Panel_PageWorkshop-memory.png) · [world](runtime/08-WorkshopForm-Panel_PageWorkshop-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0181 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0177, UIA-0178, UIA-0179, UIA-0180 |
| 5. 九宫格与边角 | 发现问题 | UIA-0177 |
| 6. 对齐间距留白 | 发现问题 | UIA-0177, UIA-0180 |
| 7. 文字与可读性 | 发现问题 | UIA-0177, UIA-0178, UIA-0179, UIA-0180 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0180, UIA-0181 |
| 9. 素材与状态语义 | 发现问题 | UIA-0176, UIA-0177 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0176, UIA-0177, UIA-0178, UIA-0180, UIA-0181 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0176 / P1 / 业务缺口 | **BUSINESS_SOURCE_NOT_CONNECTED** · `运行时数据正文`<br>本次实际运行说明：工坊与配方域尚未接入运行路径：配方、队列与产出都没有创建者。<br>期望：满足标准 9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/08-WorkshopForm-Panel_PageWorkshop-world.png) · [证据2](geometry/08-WorkshopForm-Panel_PageWorkshop-world.json) |
| UIA-0177 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `WorkshopForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/08-WorkshopForm-Panel_PageWorkshop-application.json) · [证据2](geometry/08-WorkshopForm-Panel_PageWorkshop-memory.json) · [证据3](geometry/08-WorkshopForm-Panel_PageWorkshop-scroll-top.json) · [证据4](geometry/08-WorkshopForm-Panel_PageWorkshop-state-WorkshopDisabledState.json) |
| UIA-0178 / P1 / 已确认 | **TEXT_MASK_CLIP** · `WorkshopForm/Panel_Frame/Grp_PageHost/Panel_PageWorkshop/Panel_WorkshopRecipes/List_WorkshopRecipes/Viewport_WorkshopRecipes/Content_WorkshopRecipes/Txt_WorkshopRecipesBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/08-WorkshopForm-Panel_PageWorkshop-application.json) · [证据2](geometry/08-WorkshopForm-Panel_PageWorkshop-memory.json) · [证据3](geometry/08-WorkshopForm-Panel_PageWorkshop-scroll-top.json) · [证据4](geometry/08-WorkshopForm-Panel_PageWorkshop-world-scroll-top.json) |
| UIA-0179 / P1 / 已确认 | **TEXT_OVERLAP** · `WorkshopForm/Panel_Frame/Grp_PageHost/Panel_PageWorkshop/Txt_WorkshopTitle ↔ WorkshopForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/08-WorkshopForm-Panel_PageWorkshop-application.json) · [证据2](geometry/08-WorkshopForm-Panel_PageWorkshop-memory.json) · [证据3](geometry/08-WorkshopForm-Panel_PageWorkshop-scroll-top.json) · [证据4](geometry/08-WorkshopForm-Panel_PageWorkshop-state-base.json) |
| UIA-0180 / P2 / 已确认 | **UNAVAILABLE_REASON_REPEATED** · `WorkshopForm/Panel_Frame/Grp_PageHost/Panel_PageWorkshop/Grp_WorkshopDisabledState/Panel_WorkshopDisabledMessage/Txt_WorkshopDisabledMessage`<br>相同不可用原因同时出现在状态条和多个信息卡中；浮动状态条压在插画/内容区域，重复文字分散焦点。<br>期望：满足标准 4,6,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/08-WorkshopForm-Panel_PageWorkshop-world.json) · [证据2](runtime/08-WorkshopForm-Panel_PageWorkshop-world.png) |
| UIA-0181 / P2 / 已确认 | **WORKSHOP_ART_PRIORITY** · `制造工坊各页的台座插画`<br>台座插画占据队列、阻塞说明或成本信息区域的大块空间，动态信息区的可读面积不足，需检查实际条目与图片共同出现时的排布。<br>期望：满足标准 3,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/08-WorkshopForm-Panel_PageWorkshop.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_WorkshopLoadingState](state-layout/08-WorkshopForm-Panel_PageWorkshop-WorkshopLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_WorkshopEmptyState](state-layout/08-WorkshopForm-Panel_PageWorkshop-WorkshopEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_WorkshopErrorState](state-layout/08-WorkshopForm-Panel_PageWorkshop-WorkshopErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_WorkshopSuccessState](state-layout/08-WorkshopForm-Panel_PageWorkshop-WorkshopSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_WorkshopDisabledState](state-layout/08-WorkshopForm-Panel_PageWorkshop-WorkshopDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[idle](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-08-Workshop-idle.png)、[running](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-08-Workshop-running.png)、[blocked](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-08-Workshop-blocked.png)、[running](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-08.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 045 / 09 / 玩家算法模板 · AlgorithmLibraryForm/Panel_PagePlayerTemplates

Prefab：[Assets/Game/Prefabs/UI/Operations/AlgorithmLibraryForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/AlgorithmLibraryForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/AlgorithmLibraryForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/AlgorithmLibraryForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-PlayerTemplates-owned.png)；证据：[application](runtime/09-AlgorithmLibraryForm-Panel_PagePlayerTemplates-application.png) · [memory](runtime/09-AlgorithmLibraryForm-Panel_PagePlayerTemplates-memory.png) · [world](runtime/09-AlgorithmLibraryForm-Panel_PagePlayerTemplates-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0183, UIA-0184 |
| 5. 九宫格与边角 | 发现问题 | UIA-0183 |
| 6. 对齐间距留白 | 发现问题 | UIA-0183 |
| 7. 文字与可读性 | 发现问题 | UIA-0183, UIA-0184 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0182 |
| 9. 素材与状态语义 | 发现问题 | UIA-0182, UIA-0183 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0182, UIA-0183 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0182 / P1 / 已确认 | **EMPTY_CATEGORY_STALE_DETAIL** · `玩家模板页右侧详情`<br>切到没有玩家模板的类别后，左侧提示暂无模板，右侧仍显示系统模板“基础灌溉”的名称、类型、版本和节点数，没有说明该详情属于另一类别。<br>期望：满足标准 8,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/09-AlgorithmLibraryForm-Panel_PagePlayerTemplates-world.png) · [证据2](geometry/09-AlgorithmLibraryForm-Panel_PagePlayerTemplates-world.json) |
| UIA-0183 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `AlgorithmLibraryForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-AlgorithmLibraryForm-Panel_PagePlayerTemplates-application.json) · [证据2](geometry/09-AlgorithmLibraryForm-Panel_PagePlayerTemplates-memory.json) · [证据3](geometry/09-AlgorithmLibraryForm-Panel_PagePlayerTemplates-scroll-bottom.json) · [证据4](geometry/09-AlgorithmLibraryForm-Panel_PagePlayerTemplates-scroll-top.json) |
| UIA-0184 / P1 / 已确认 | **TEXT_OVERLAP** · `AlgorithmLibraryForm/Panel_Frame/Grp_PageHost/Panel_PagePlayerTemplates/Txt_PlayerTemplatesTitle ↔ AlgorithmLibraryForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-AlgorithmLibraryForm-Panel_PagePlayerTemplates-application.json) · [证据2](geometry/09-AlgorithmLibraryForm-Panel_PagePlayerTemplates-memory.json) · [证据3](geometry/09-AlgorithmLibraryForm-Panel_PagePlayerTemplates-scroll-bottom.json) · [证据4](geometry/09-AlgorithmLibraryForm-Panel_PagePlayerTemplates-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_PlayerTemplatesLoadingState](state-layout/09-AlgorithmLibraryForm-Panel_PagePlayerTemplates-PlayerTemplatesLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_PlayerTemplatesEmptyState](state-layout/09-AlgorithmLibraryForm-Panel_PagePlayerTemplates-PlayerTemplatesEmptyState.png) | 已检查 | memory、world |
| [Grp_PlayerTemplatesErrorState](state-layout/09-AlgorithmLibraryForm-Panel_PagePlayerTemplates-PlayerTemplatesErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_PlayerTemplatesSuccessState](state-layout/09-AlgorithmLibraryForm-Panel_PagePlayerTemplates-PlayerTemplatesSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_PlayerTemplatesDisabledState](state-layout/09-AlgorithmLibraryForm-Panel_PagePlayerTemplates-PlayerTemplatesDisabledState.png) | 已检查 | application |

效果图要求的代表态：[owned](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-PlayerTemplates-owned.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 046 / 09 / 系统算法模板 · AlgorithmLibraryForm/Panel_PageSystemTemplates

Prefab：[Assets/Game/Prefabs/UI/Operations/AlgorithmLibraryForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/AlgorithmLibraryForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/AlgorithmLibraryForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/AlgorithmLibraryForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-SystemTemplates-owned.png)；证据：[application](runtime/09-AlgorithmLibraryForm-Panel_PageSystemTemplates-application.png) · [memory](runtime/09-AlgorithmLibraryForm-Panel_PageSystemTemplates-memory.png) · [world](runtime/09-AlgorithmLibraryForm-Panel_PageSystemTemplates-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 发现问题 | UIA-0185 |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0186, UIA-0187, UIA-0188 |
| 5. 九宫格与边角 | 发现问题 | UIA-0186, UIA-0189 |
| 6. 对齐间距留白 | 发现问题 | UIA-0185, UIA-0186 |
| 7. 文字与可读性 | 发现问题 | UIA-0186, UIA-0187, UIA-0189 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0185, UIA-0188, UIA-0189 |
| 9. 素材与状态语义 | 发现问题 | UIA-0186, UIA-0188, UIA-0189 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0185, UIA-0186, UIA-0188 |
| 12. 同类页面一致性 | 发现问题 | UIA-0189 |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0185 / P1 / 已确认 | **GRID_SUMMARY_AS_CELL** · `目录 GridLayoutGroup 的说明文本与条目`<br>说明文本参与网格排版，占去第一格；第一条从后一格开始，首排空洞，正常数据的排列不连续。<br>期望：满足标准 1,6,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/09-AlgorithmLibraryForm-Panel_PageSystemTemplates-world.png) · [证据2](geometry/09-AlgorithmLibraryForm-Panel_PageSystemTemplates-world.json) |
| UIA-0186 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `AlgorithmLibraryForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-AlgorithmLibraryForm-Panel_PageSystemTemplates-application.json) · [证据2](geometry/09-AlgorithmLibraryForm-Panel_PageSystemTemplates-memory.json) · [证据3](geometry/09-AlgorithmLibraryForm-Panel_PageSystemTemplates-scroll-bottom.json) · [证据4](geometry/09-AlgorithmLibraryForm-Panel_PageSystemTemplates-scroll-top.json) |
| UIA-0187 / P1 / 已确认 | **TEXT_OVERLAP** · `AlgorithmLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageSystemTemplates/Txt_SystemTemplatesTitle ↔ AlgorithmLibraryForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-AlgorithmLibraryForm-Panel_PageSystemTemplates-application.json) · [证据2](geometry/09-AlgorithmLibraryForm-Panel_PageSystemTemplates-memory.json) · [证据3](geometry/09-AlgorithmLibraryForm-Panel_PageSystemTemplates-scroll-bottom.json) · [证据4](geometry/09-AlgorithmLibraryForm-Panel_PageSystemTemplates-scroll-top.json) |
| UIA-0188 / P1 / 已确认 | **UNSOLICITED_SUCCESS** · `AlgorithmLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageSystemTemplates/Grp_SystemTemplatesSuccessState/Panel_SystemTemplatesSuccessMessage/Txt_SystemTemplatesSuccessMessage`<br>仅打开目录/读取数据即显示“操作成功”，提示叠在列表或插画上，成功语义与当前动作不符<br>期望：满足标准 4,8,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-AlgorithmLibraryForm-Panel_PageSystemTemplates-memory.json) · [证据2](geometry/09-AlgorithmLibraryForm-Panel_PageSystemTemplates-world.json) |
| UIA-0189 / P2 / 已确认 | **READONLY_USES_DISABLED_SKIN** · `只读属性/摘要条目`<br>正常可读数据被渲染为禁用 Button，出现斜纹禁用底图和按钮端帽；信息字段看起来像不可用操作，弱化正文。<br>期望：满足标准 5,7,8,9,12：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/09-AlgorithmLibraryForm-Panel_PageSystemTemplates-world.png) · [证据2](geometry/09-AlgorithmLibraryForm-Panel_PageSystemTemplates-world.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_SystemTemplatesLoadingState](state-layout/09-AlgorithmLibraryForm-Panel_PageSystemTemplates-SystemTemplatesLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_SystemTemplatesEmptyState](state-layout/09-AlgorithmLibraryForm-Panel_PageSystemTemplates-SystemTemplatesEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_SystemTemplatesErrorState](state-layout/09-AlgorithmLibraryForm-Panel_PageSystemTemplates-SystemTemplatesErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_SystemTemplatesSuccessState](state-layout/09-AlgorithmLibraryForm-Panel_PageSystemTemplates-SystemTemplatesSuccessState.png) | 已检查 | memory、world |
| [Grp_SystemTemplatesDisabledState](state-layout/09-AlgorithmLibraryForm-Panel_PageSystemTemplates-SystemTemplatesDisabledState.png) | 已检查 | application |

效果图要求的代表态：[owned](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-SystemTemplates-owned.png)、[locked](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-SystemTemplates-locked.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 047 / 09 / 算法模板详情 · AlgorithmLibraryForm/Panel_PageTemplateDetail

Prefab：[Assets/Game/Prefabs/UI/Operations/AlgorithmLibraryForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/AlgorithmLibraryForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/AlgorithmLibraryForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/AlgorithmLibraryForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-TemplateDetail-normal.png)；证据：[application](runtime/09-AlgorithmLibraryForm-Panel_PageTemplateDetail-application.png) · [memory](runtime/09-AlgorithmLibraryForm-Panel_PageTemplateDetail-memory.png) · [world](runtime/09-AlgorithmLibraryForm-Panel_PageTemplateDetail-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0190, UIA-0191, UIA-0192, UIA-0193 |
| 5. 九宫格与边角 | 发现问题 | UIA-0190, UIA-0194 |
| 6. 对齐间距留白 | 发现问题 | UIA-0190 |
| 7. 文字与可读性 | 发现问题 | UIA-0190, UIA-0191, UIA-0192, UIA-0194 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0193, UIA-0194 |
| 9. 素材与状态语义 | 发现问题 | UIA-0190, UIA-0193, UIA-0194 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0190, UIA-0191, UIA-0193 |
| 12. 同类页面一致性 | 发现问题 | UIA-0194 |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0190 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `AlgorithmLibraryForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-AlgorithmLibraryForm-Panel_PageTemplateDetail-application.json) · [证据2](geometry/09-AlgorithmLibraryForm-Panel_PageTemplateDetail-memory.json) · [证据3](geometry/09-AlgorithmLibraryForm-Panel_PageTemplateDetail-scroll-bottom.json) · [证据4](geometry/09-AlgorithmLibraryForm-Panel_PageTemplateDetail-scroll-top.json) |
| UIA-0191 / P1 / 已确认 | **TEXT_MASK_CLIP** · `AlgorithmLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageTemplateDetail/Panel_TemplateDetailRequirements/List_TemplateDetailRequirements/Viewport_TemplateDetailRequirements/Content_TemplateDetailRequirements/Txt_TemplateDetailRequirementsBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-AlgorithmLibraryForm-Panel_PageTemplateDetail-memory.json) · [证据2](geometry/09-AlgorithmLibraryForm-Panel_PageTemplateDetail-scroll-bottom.json) · [证据3](geometry/09-AlgorithmLibraryForm-Panel_PageTemplateDetail-scroll-top.json) · [证据4](geometry/09-AlgorithmLibraryForm-Panel_PageTemplateDetail-world-scroll-bottom.json) |
| UIA-0192 / P1 / 已确认 | **TEXT_OVERLAP** · `AlgorithmLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageTemplateDetail/Txt_TemplateDetailTitle ↔ AlgorithmLibraryForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-AlgorithmLibraryForm-Panel_PageTemplateDetail-application.json) · [证据2](geometry/09-AlgorithmLibraryForm-Panel_PageTemplateDetail-memory.json) · [证据3](geometry/09-AlgorithmLibraryForm-Panel_PageTemplateDetail-scroll-bottom.json) · [证据4](geometry/09-AlgorithmLibraryForm-Panel_PageTemplateDetail-scroll-top.json) |
| UIA-0193 / P1 / 已确认 | **UNSOLICITED_SUCCESS** · `AlgorithmLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageTemplateDetail/Grp_TemplateDetailSuccessState/Panel_TemplateDetailSuccessMessage/Txt_TemplateDetailSuccessMessage`<br>仅打开目录/读取数据即显示“操作成功”，提示叠在列表或插画上，成功语义与当前动作不符<br>期望：满足标准 4,8,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-AlgorithmLibraryForm-Panel_PageTemplateDetail-memory.json) · [证据2](geometry/09-AlgorithmLibraryForm-Panel_PageTemplateDetail-world.json) |
| UIA-0194 / P2 / 已确认 | **READONLY_USES_DISABLED_SKIN** · `只读属性/摘要条目`<br>正常可读数据被渲染为禁用 Button，出现斜纹禁用底图和按钮端帽；信息字段看起来像不可用操作，弱化正文。<br>期望：满足标准 5,7,8,9,12：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/09-AlgorithmLibraryForm-Panel_PageTemplateDetail-world.png) · [证据2](geometry/09-AlgorithmLibraryForm-Panel_PageTemplateDetail-world.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_TemplateDetailLoadingState](state-layout/09-AlgorithmLibraryForm-Panel_PageTemplateDetail-TemplateDetailLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_TemplateDetailEmptyState](state-layout/09-AlgorithmLibraryForm-Panel_PageTemplateDetail-TemplateDetailEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_TemplateDetailErrorState](state-layout/09-AlgorithmLibraryForm-Panel_PageTemplateDetail-TemplateDetailErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_TemplateDetailSuccessState](state-layout/09-AlgorithmLibraryForm-Panel_PageTemplateDetail-TemplateDetailSuccessState.png) | 已检查 | memory、world |
| [Grp_TemplateDetailDisabledState](state-layout/09-AlgorithmLibraryForm-Panel_PageTemplateDetail-TemplateDetailDisabledState.png) | 已检查 | application |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-TemplateDetail-normal.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 048 / 09 / 组件详情与比较 · ComponentLibraryForm/Panel_PageComponentDetail

Prefab：[Assets/Game/Prefabs/UI/Operations/ComponentLibraryForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/ComponentLibraryForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/ComponentLibraryForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/ComponentLibraryForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-ComponentDetail-normal.png)；证据：[application](runtime/09-ComponentLibraryForm-Panel_PageComponentDetail-application.png) · [memory](runtime/09-ComponentLibraryForm-Panel_PageComponentDetail-memory.png) · [world](runtime/09-ComponentLibraryForm-Panel_PageComponentDetail-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 发现问题 | UIA-0196 |
| 3. 尺寸与比例 | 发现问题 | UIA-0196 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0196, UIA-0197, UIA-0198 |
| 5. 九宫格与边角 | 发现问题 | UIA-0197 |
| 6. 对齐间距留白 | 发现问题 | UIA-0197 |
| 7. 文字与可读性 | 发现问题 | UIA-0197, UIA-0198 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 发现问题 | UIA-0195, UIA-0197 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0195, UIA-0197 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0195 / P1 / 业务缺口 | **BUSINESS_SOURCE_NOT_CONNECTED** · `运行时数据正文`<br>本次实际运行说明：对比尚未接入：本页还需要「选择第二个组件作为参照」这条通道。<br>期望：满足标准 9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/09-ComponentLibraryForm-Panel_PageComponentDetail-world.png) · [证据2](geometry/09-ComponentLibraryForm-Panel_PageComponentDetail-world.json) |
| UIA-0196 / P1 / 已确认 | **COMPONENT_ART_HIDDEN** · `ComponentDetail 组件资料/属性卡之间`<br>正式插画被两块信息卡遮去大部分，只剩中间窄条可见，图片层与内容层没有预留独立空间。<br>期望：满足标准 2,3,4：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/09-ComponentLibraryForm-Panel_PageComponentDetail.png) |
| UIA-0197 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `ComponentLibraryForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-ComponentLibraryForm-Panel_PageComponentDetail-application.json) · [证据2](geometry/09-ComponentLibraryForm-Panel_PageComponentDetail-memory.json) · [证据3](geometry/09-ComponentLibraryForm-Panel_PageComponentDetail-scroll-top.json) · [证据4](geometry/09-ComponentLibraryForm-Panel_PageComponentDetail-state-ComponentDetailDisabledState.json) |
| UIA-0198 / P1 / 已确认 | **TEXT_OVERLAP** · `ComponentLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageComponentDetail/Txt_ComponentDetailTitle ↔ ComponentLibraryForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-ComponentLibraryForm-Panel_PageComponentDetail-application.json) · [证据2](geometry/09-ComponentLibraryForm-Panel_PageComponentDetail-memory.json) · [证据3](geometry/09-ComponentLibraryForm-Panel_PageComponentDetail-scroll-top.json) · [证据4](geometry/09-ComponentLibraryForm-Panel_PageComponentDetail-state-base.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_ComponentDetailLoadingState](state-layout/09-ComponentLibraryForm-Panel_PageComponentDetail-ComponentDetailLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ComponentDetailEmptyState](state-layout/09-ComponentLibraryForm-Panel_PageComponentDetail-ComponentDetailEmptyState.png) | 已检查 | memory、world |
| [Grp_ComponentDetailErrorState](state-layout/09-ComponentLibraryForm-Panel_PageComponentDetail-ComponentDetailErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ComponentDetailSuccessState](state-layout/09-ComponentLibraryForm-Panel_PageComponentDetail-ComponentDetailSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ComponentDetailDisabledState](state-layout/09-ComponentLibraryForm-Panel_PageComponentDetail-ComponentDetailDisabledState.png) | 已检查 | application |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-ComponentDetail-normal.png)、[compare](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-ComponentDetail-compare.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 049 / 09 / 已安装组件 · ComponentLibraryForm/Panel_PageInstalledComponents

Prefab：[Assets/Game/Prefabs/UI/Operations/ComponentLibraryForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/ComponentLibraryForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/ComponentLibraryForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/ComponentLibraryForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-InstalledComponents-installed.png)；证据：[application](runtime/09-ComponentLibraryForm-Panel_PageInstalledComponents-application.png) · [memory](runtime/09-ComponentLibraryForm-Panel_PageInstalledComponents-memory.png) · [world](runtime/09-ComponentLibraryForm-Panel_PageInstalledComponents-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 发现问题 | UIA-0199 |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0200, UIA-0201, UIA-0202 |
| 5. 九宫格与边角 | 发现问题 | UIA-0200, UIA-0203 |
| 6. 对齐间距留白 | 发现问题 | UIA-0199, UIA-0200 |
| 7. 文字与可读性 | 发现问题 | UIA-0200, UIA-0201, UIA-0202, UIA-0203 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0199, UIA-0203 |
| 9. 素材与状态语义 | 发现问题 | UIA-0200, UIA-0203 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0199, UIA-0200, UIA-0201 |
| 12. 同类页面一致性 | 发现问题 | UIA-0203 |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0199 / P1 / 已确认 | **GRID_SUMMARY_AS_CELL** · `目录 GridLayoutGroup 的说明文本与条目`<br>说明文本参与网格排版，占去第一格；第一条从后一格开始，首排空洞，正常数据的排列不连续。<br>期望：满足标准 1,6,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/09-ComponentLibraryForm-Panel_PageInstalledComponents-memory.png) · [证据2](geometry/09-ComponentLibraryForm-Panel_PageInstalledComponents-memory.json) |
| UIA-0200 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `ComponentLibraryForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-ComponentLibraryForm-Panel_PageInstalledComponents-application.json) · [证据2](geometry/09-ComponentLibraryForm-Panel_PageInstalledComponents-memory.json) · [证据3](geometry/09-ComponentLibraryForm-Panel_PageInstalledComponents-scroll-top.json) · [证据4](geometry/09-ComponentLibraryForm-Panel_PageInstalledComponents-state-InstalledComponentsDisabledState.json) |
| UIA-0201 / P1 / 已确认 | **TEXT_MASK_CLIP** · `ComponentLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageInstalledComponents/Panel_InstalledComponentsDetails/List_InstalledComponentsDetails/Viewport_InstalledComponentsDetails/Content_InstalledComponentsDetails/Txt_InstalledComponentsDetailsBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-ComponentLibraryForm-Panel_PageInstalledComponents-application.json) · [证据2](geometry/09-ComponentLibraryForm-Panel_PageInstalledComponents-memory.json) · [证据3](geometry/09-ComponentLibraryForm-Panel_PageInstalledComponents-scroll-top.json) · [证据4](geometry/09-ComponentLibraryForm-Panel_PageInstalledComponents-world-scroll-top.json) |
| UIA-0202 / P1 / 已确认 | **TEXT_OVERLAP** · `ComponentLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageInstalledComponents/Txt_InstalledComponentsTitle ↔ ComponentLibraryForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-ComponentLibraryForm-Panel_PageInstalledComponents-application.json) · [证据2](geometry/09-ComponentLibraryForm-Panel_PageInstalledComponents-memory.json) · [证据3](geometry/09-ComponentLibraryForm-Panel_PageInstalledComponents-scroll-top.json) · [证据4](geometry/09-ComponentLibraryForm-Panel_PageInstalledComponents-state-base.json) |
| UIA-0203 / P2 / 已确认 | **READONLY_USES_DISABLED_SKIN** · `只读属性/摘要条目`<br>正常可读数据被渲染为禁用 Button，出现斜纹禁用底图和按钮端帽；信息字段看起来像不可用操作，弱化正文。<br>期望：满足标准 5,7,8,9,12：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/09-ComponentLibraryForm-Panel_PageInstalledComponents-memory.png) · [证据2](geometry/09-ComponentLibraryForm-Panel_PageInstalledComponents-memory.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_InstalledComponentsLoadingState](state-layout/09-ComponentLibraryForm-Panel_PageInstalledComponents-InstalledComponentsLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_InstalledComponentsEmptyState](state-layout/09-ComponentLibraryForm-Panel_PageInstalledComponents-InstalledComponentsEmptyState.png) | 已检查 | memory、world |
| [Grp_InstalledComponentsErrorState](state-layout/09-ComponentLibraryForm-Panel_PageInstalledComponents-InstalledComponentsErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_InstalledComponentsSuccessState](state-layout/09-ComponentLibraryForm-Panel_PageInstalledComponents-InstalledComponentsSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_InstalledComponentsDisabledState](state-layout/09-ComponentLibraryForm-Panel_PageInstalledComponents-InstalledComponentsDisabledState.png) | 已检查 | application |

效果图要求的代表态：[installed](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-InstalledComponents-installed.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 050 / 09 / 未安装组件 · ComponentLibraryForm/Panel_PageLooseComponents

Prefab：[Assets/Game/Prefabs/UI/Operations/ComponentLibraryForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/ComponentLibraryForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/ComponentLibraryForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/ComponentLibraryForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-LooseComponents-owned.png)；证据：[application](runtime/09-ComponentLibraryForm-Panel_PageLooseComponents-application.png) · [memory](runtime/09-ComponentLibraryForm-Panel_PageLooseComponents-memory.png) · [world](runtime/09-ComponentLibraryForm-Panel_PageLooseComponents-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 发现问题 | UIA-0204 |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0205, UIA-0206, UIA-0207 |
| 5. 九宫格与边角 | 发现问题 | UIA-0205, UIA-0208 |
| 6. 对齐间距留白 | 发现问题 | UIA-0204, UIA-0205 |
| 7. 文字与可读性 | 发现问题 | UIA-0205, UIA-0206, UIA-0207, UIA-0208 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0204, UIA-0208 |
| 9. 素材与状态语义 | 发现问题 | UIA-0205, UIA-0208 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0204, UIA-0205, UIA-0206 |
| 12. 同类页面一致性 | 发现问题 | UIA-0208 |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0204 / P1 / 已确认 | **GRID_SUMMARY_AS_CELL** · `目录 GridLayoutGroup 的说明文本与条目`<br>说明文本参与网格排版，占去第一格；第一条从后一格开始，首排空洞，正常数据的排列不连续。<br>期望：满足标准 1,6,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/09-ComponentLibraryForm-Panel_PageLooseComponents-memory.png) · [证据2](geometry/09-ComponentLibraryForm-Panel_PageLooseComponents-memory.json) |
| UIA-0205 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `ComponentLibraryForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-ComponentLibraryForm-Panel_PageLooseComponents-application.json) · [证据2](geometry/09-ComponentLibraryForm-Panel_PageLooseComponents-memory.json) · [证据3](geometry/09-ComponentLibraryForm-Panel_PageLooseComponents-scroll-top.json) · [证据4](geometry/09-ComponentLibraryForm-Panel_PageLooseComponents-state-LooseComponentsDisabledState.json) |
| UIA-0206 / P1 / 已确认 | **TEXT_MASK_CLIP** · `ComponentLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageLooseComponents/Panel_LooseComponentsDetails/List_LooseComponentsDetails/Viewport_LooseComponentsDetails/Content_LooseComponentsDetails/Txt_LooseComponentsDetailsBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-ComponentLibraryForm-Panel_PageLooseComponents-application.json) · [证据2](geometry/09-ComponentLibraryForm-Panel_PageLooseComponents-memory.json) · [证据3](geometry/09-ComponentLibraryForm-Panel_PageLooseComponents-scroll-top.json) · [证据4](geometry/09-ComponentLibraryForm-Panel_PageLooseComponents-world-scroll-top.json) |
| UIA-0207 / P1 / 已确认 | **TEXT_OVERLAP** · `ComponentLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageLooseComponents/Txt_LooseComponentsTitle ↔ ComponentLibraryForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-ComponentLibraryForm-Panel_PageLooseComponents-application.json) · [证据2](geometry/09-ComponentLibraryForm-Panel_PageLooseComponents-memory.json) · [证据3](geometry/09-ComponentLibraryForm-Panel_PageLooseComponents-scroll-top.json) · [证据4](geometry/09-ComponentLibraryForm-Panel_PageLooseComponents-state-base.json) |
| UIA-0208 / P2 / 已确认 | **READONLY_USES_DISABLED_SKIN** · `只读属性/摘要条目`<br>正常可读数据被渲染为禁用 Button，出现斜纹禁用底图和按钮端帽；信息字段看起来像不可用操作，弱化正文。<br>期望：满足标准 5,7,8,9,12：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/09-ComponentLibraryForm-Panel_PageLooseComponents-memory.png) · [证据2](geometry/09-ComponentLibraryForm-Panel_PageLooseComponents-memory.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_LooseComponentsLoadingState](state-layout/09-ComponentLibraryForm-Panel_PageLooseComponents-LooseComponentsLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_LooseComponentsEmptyState](state-layout/09-ComponentLibraryForm-Panel_PageLooseComponents-LooseComponentsEmptyState.png) | 已检查 | memory、world |
| [Grp_LooseComponentsErrorState](state-layout/09-ComponentLibraryForm-Panel_PageLooseComponents-LooseComponentsErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_LooseComponentsSuccessState](state-layout/09-ComponentLibraryForm-Panel_PageLooseComponents-LooseComponentsSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_LooseComponentsDisabledState](state-layout/09-ComponentLibraryForm-Panel_PageLooseComponents-LooseComponentsDisabledState.png) | 已检查 | application |

效果图要求的代表态：[owned](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-LooseComponents-owned.png)、[locked](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-LooseComponents-locked.png)、[empty](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-LooseComponents-empty.png)、[owned](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 051 / 09 / 已部署机器 · MachineLibraryForm/Panel_PageDeployedMachines

Prefab：[Assets/Game/Prefabs/UI/Operations/MachineLibraryForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/MachineLibraryForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/MachineLibraryForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/MachineLibraryForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-DeployedMachines-deployed.png)；证据：[application](runtime/09-MachineLibraryForm-Panel_PageDeployedMachines-application.png) · [memory](runtime/09-MachineLibraryForm-Panel_PageDeployedMachines-memory.png) · [world](runtime/09-MachineLibraryForm-Panel_PageDeployedMachines-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 发现问题 | UIA-0209 |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0210, UIA-0211, UIA-0212 |
| 5. 九宫格与边角 | 发现问题 | UIA-0210, UIA-0213 |
| 6. 对齐间距留白 | 发现问题 | UIA-0209, UIA-0210 |
| 7. 文字与可读性 | 发现问题 | UIA-0210, UIA-0211, UIA-0213 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0209, UIA-0212, UIA-0213 |
| 9. 素材与状态语义 | 发现问题 | UIA-0210, UIA-0212, UIA-0213 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0209, UIA-0210, UIA-0212 |
| 12. 同类页面一致性 | 发现问题 | UIA-0213 |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0209 / P1 / 已确认 | **GRID_SUMMARY_AS_CELL** · `目录 GridLayoutGroup 的说明文本与条目`<br>说明文本参与网格排版，占去第一格；第一条从后一格开始，首排空洞，正常数据的排列不连续。<br>期望：满足标准 1,6,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/09-MachineLibraryForm-Panel_PageDeployedMachines-memory.png) · [证据2](geometry/09-MachineLibraryForm-Panel_PageDeployedMachines-memory.json) |
| UIA-0210 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `MachineLibraryForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-MachineLibraryForm-Panel_PageDeployedMachines-application.json) · [证据2](geometry/09-MachineLibraryForm-Panel_PageDeployedMachines-memory.json) · [证据3](geometry/09-MachineLibraryForm-Panel_PageDeployedMachines-scroll-top.json) · [证据4](geometry/09-MachineLibraryForm-Panel_PageDeployedMachines-state-DeployedMachinesDisabledState.json) |
| UIA-0211 / P1 / 已确认 | **TEXT_OVERLAP** · `MachineLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageDeployedMachines/Txt_DeployedMachinesTitle ↔ MachineLibraryForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-MachineLibraryForm-Panel_PageDeployedMachines-application.json) · [证据2](geometry/09-MachineLibraryForm-Panel_PageDeployedMachines-memory.json) · [证据3](geometry/09-MachineLibraryForm-Panel_PageDeployedMachines-scroll-top.json) · [证据4](geometry/09-MachineLibraryForm-Panel_PageDeployedMachines-state-base.json) |
| UIA-0212 / P1 / 已确认 | **UNSOLICITED_SUCCESS** · `MachineLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageDeployedMachines/Grp_DeployedMachinesSuccessState/Panel_DeployedMachinesSuccessMessage/Txt_DeployedMachinesSuccessMessage`<br>仅打开目录/读取数据即显示“操作成功”，提示叠在列表或插画上，成功语义与当前动作不符<br>期望：满足标准 4,8,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-MachineLibraryForm-Panel_PageDeployedMachines-memory.json) |
| UIA-0213 / P2 / 已确认 | **READONLY_USES_DISABLED_SKIN** · `只读属性/摘要条目`<br>正常可读数据被渲染为禁用 Button，出现斜纹禁用底图和按钮端帽；信息字段看起来像不可用操作，弱化正文。<br>期望：满足标准 5,7,8,9,12：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/09-MachineLibraryForm-Panel_PageDeployedMachines-memory.png) · [证据2](geometry/09-MachineLibraryForm-Panel_PageDeployedMachines-memory.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_DeployedMachinesLoadingState](state-layout/09-MachineLibraryForm-Panel_PageDeployedMachines-DeployedMachinesLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_DeployedMachinesEmptyState](state-layout/09-MachineLibraryForm-Panel_PageDeployedMachines-DeployedMachinesEmptyState.png) | 已检查 | world |
| [Grp_DeployedMachinesErrorState](state-layout/09-MachineLibraryForm-Panel_PageDeployedMachines-DeployedMachinesErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_DeployedMachinesSuccessState](state-layout/09-MachineLibraryForm-Panel_PageDeployedMachines-DeployedMachinesSuccessState.png) | 已检查 | memory |
| [Grp_DeployedMachinesDisabledState](state-layout/09-MachineLibraryForm-Panel_PageDeployedMachines-DeployedMachinesDisabledState.png) | 已检查 | application |

效果图要求的代表态：[deployed](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-DeployedMachines-deployed.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 052 / 09 / 未部署机器 · MachineLibraryForm/Panel_PageUndeployedMachines

Prefab：[Assets/Game/Prefabs/UI/Operations/MachineLibraryForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/MachineLibraryForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/MachineLibraryForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/MachineLibraryForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-UndeployedMachines-owned.png)；证据：[application](runtime/09-MachineLibraryForm-Panel_PageUndeployedMachines-application.png) · [memory](runtime/09-MachineLibraryForm-Panel_PageUndeployedMachines-memory.png) · [world](runtime/09-MachineLibraryForm-Panel_PageUndeployedMachines-world.png) · [memory/selected-machine](runtime/machine-selected.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 发现问题 | UIA-0214 |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0215, UIA-0216, UIA-0217, UIA-0218 |
| 5. 九宫格与边角 | 发现问题 | UIA-0215, UIA-0219 |
| 6. 对齐间距留白 | 发现问题 | UIA-0214, UIA-0215 |
| 7. 文字与可读性 | 发现问题 | UIA-0215, UIA-0216, UIA-0217, UIA-0219 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0214, UIA-0218, UIA-0219 |
| 9. 素材与状态语义 | 发现问题 | UIA-0215, UIA-0218, UIA-0219 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0214, UIA-0215, UIA-0216, UIA-0218 |
| 12. 同类页面一致性 | 发现问题 | UIA-0219 |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0214 / P1 / 已确认 | **GRID_SUMMARY_AS_CELL** · `目录 GridLayoutGroup 的说明文本与条目`<br>说明文本参与网格排版，占去第一格；第一条从后一格开始，首排空洞，正常数据的排列不连续。<br>期望：满足标准 1,6,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/09-MachineLibraryForm-Panel_PageUndeployedMachines-memory.png) · [证据2](geometry/09-MachineLibraryForm-Panel_PageUndeployedMachines-memory.json) |
| UIA-0215 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `MachineLibraryForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-MachineLibraryForm-Panel_PageUndeployedMachines-application.json) · [证据2](geometry/09-MachineLibraryForm-Panel_PageUndeployedMachines-memory.json) · [证据3](geometry/09-MachineLibraryForm-Panel_PageUndeployedMachines-scroll-top.json) · [证据4](geometry/09-MachineLibraryForm-Panel_PageUndeployedMachines-state-UndeployedMachinesDisabledState.json) |
| UIA-0216 / P1 / 已确认 | **TEXT_MASK_CLIP** · `MachineLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageUndeployedMachines/Panel_UndeployedMachinesDetail/List_UndeployedMachinesDetail/Viewport_UndeployedMachinesDetail/Content_UndeployedMachinesDetail/Txt_UndeployedMachinesDetailBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/machine-selected.json) |
| UIA-0217 / P1 / 已确认 | **TEXT_OVERLAP** · `MachineLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageUndeployedMachines/Txt_UndeployedMachinesTitle ↔ MachineLibraryForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-MachineLibraryForm-Panel_PageUndeployedMachines-application.json) · [证据2](geometry/09-MachineLibraryForm-Panel_PageUndeployedMachines-memory.json) · [证据3](geometry/09-MachineLibraryForm-Panel_PageUndeployedMachines-scroll-top.json) · [证据4](geometry/09-MachineLibraryForm-Panel_PageUndeployedMachines-state-base.json) |
| UIA-0218 / P1 / 已确认 | **UNSOLICITED_SUCCESS** · `MachineLibraryForm/Panel_Frame/Grp_PageHost/Panel_PageUndeployedMachines/Grp_UndeployedMachinesSuccessState/Panel_UndeployedMachinesSuccessMessage/Txt_UndeployedMachinesSuccessMessage`<br>仅打开目录/读取数据即显示“操作成功”，提示叠在列表或插画上，成功语义与当前动作不符<br>期望：满足标准 4,8,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-MachineLibraryForm-Panel_PageUndeployedMachines-memory.json) |
| UIA-0219 / P2 / 已确认 | **READONLY_USES_DISABLED_SKIN** · `只读属性/摘要条目`<br>正常可读数据被渲染为禁用 Button，出现斜纹禁用底图和按钮端帽；信息字段看起来像不可用操作，弱化正文。<br>期望：满足标准 5,7,8,9,12：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/09-MachineLibraryForm-Panel_PageUndeployedMachines-memory.png) · [证据2](geometry/09-MachineLibraryForm-Panel_PageUndeployedMachines-memory.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_UndeployedMachinesLoadingState](state-layout/09-MachineLibraryForm-Panel_PageUndeployedMachines-UndeployedMachinesLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_UndeployedMachinesEmptyState](state-layout/09-MachineLibraryForm-Panel_PageUndeployedMachines-UndeployedMachinesEmptyState.png) | 已检查 | world |
| [Grp_UndeployedMachinesErrorState](state-layout/09-MachineLibraryForm-Panel_PageUndeployedMachines-UndeployedMachinesErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_UndeployedMachinesSuccessState](state-layout/09-MachineLibraryForm-Panel_PageUndeployedMachines-UndeployedMachinesSuccessState.png) | 已检查 | memory、memory/selected-machine |
| [Grp_UndeployedMachinesDisabledState](state-layout/09-MachineLibraryForm-Panel_PageUndeployedMachines-UndeployedMachinesDisabledState.png) | 已检查 | application |

效果图要求的代表态：[owned](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-UndeployedMachines-owned.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 053 / 09 / 仓库分类库存 · WarehouseForm/Panel_PageInventory

Prefab：[Assets/Game/Prefabs/UI/Operations/WarehouseForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/WarehouseForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/WarehouseForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/WarehouseForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-Inventory-owned.png)；证据：[application](runtime/09-WarehouseForm-Panel_PageInventory-application.png) · [memory](runtime/09-WarehouseForm-Panel_PageInventory-memory.png) · [world](runtime/09-WarehouseForm-Panel_PageInventory-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 发现问题 | UIA-0220 |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0221, UIA-0222 |
| 5. 九宫格与边角 | 发现问题 | UIA-0221, UIA-0223 |
| 6. 对齐间距留白 | 发现问题 | UIA-0220, UIA-0221 |
| 7. 文字与可读性 | 发现问题 | UIA-0221, UIA-0222, UIA-0223 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0220, UIA-0223 |
| 9. 素材与状态语义 | 发现问题 | UIA-0221, UIA-0223 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0220, UIA-0221 |
| 12. 同类页面一致性 | 发现问题 | UIA-0223 |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0220 / P1 / 已确认 | **GRID_SUMMARY_AS_CELL** · `目录 GridLayoutGroup 的说明文本与条目`<br>说明文本参与网格排版，占去第一格；第一条从后一格开始，首排空洞，正常数据的排列不连续。<br>期望：满足标准 1,6,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/09-WarehouseForm-Panel_PageInventory-world.png) · [证据2](geometry/09-WarehouseForm-Panel_PageInventory-world.json) |
| UIA-0221 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `WarehouseForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-WarehouseForm-Panel_PageInventory-application.json) · [证据2](geometry/09-WarehouseForm-Panel_PageInventory-memory.json) · [证据3](geometry/09-WarehouseForm-Panel_PageInventory-scroll-top.json) · [证据4](geometry/09-WarehouseForm-Panel_PageInventory-state-InventoryDisabledState.json) |
| UIA-0222 / P1 / 已确认 | **TEXT_OVERLAP** · `WarehouseForm/Panel_Frame/Grp_PageHost/Panel_PageInventory/Txt_InventoryTitle ↔ WarehouseForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-WarehouseForm-Panel_PageInventory-application.json) · [证据2](geometry/09-WarehouseForm-Panel_PageInventory-memory.json) · [证据3](geometry/09-WarehouseForm-Panel_PageInventory-scroll-top.json) · [证据4](geometry/09-WarehouseForm-Panel_PageInventory-state-base.json) |
| UIA-0223 / P2 / 已确认 | **READONLY_USES_DISABLED_SKIN** · `只读属性/摘要条目`<br>正常可读数据被渲染为禁用 Button，出现斜纹禁用底图和按钮端帽；信息字段看起来像不可用操作，弱化正文。<br>期望：满足标准 5,7,8,9,12：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/09-WarehouseForm-Panel_PageInventory-world.png) · [证据2](geometry/09-WarehouseForm-Panel_PageInventory-world.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_InventoryLoadingState](state-layout/09-WarehouseForm-Panel_PageInventory-InventoryLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_InventoryEmptyState](state-layout/09-WarehouseForm-Panel_PageInventory-InventoryEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_InventoryErrorState](state-layout/09-WarehouseForm-Panel_PageInventory-InventoryErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_InventorySuccessState](state-layout/09-WarehouseForm-Panel_PageInventory-InventorySuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_InventoryDisabledState](state-layout/09-WarehouseForm-Panel_PageInventory-InventoryDisabledState.png) | 已检查 | application、memory |

效果图要求的代表态：[owned](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-Inventory-owned.png)、[empty](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-Inventory-empty.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 054 / 09 / 库存物品详情 · WarehouseForm/Panel_PageItemDetail

Prefab：[Assets/Game/Prefabs/UI/Operations/WarehouseForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/WarehouseForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/WarehouseForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/WarehouseForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-ItemDetail-normal.png)；证据：[application](runtime/09-WarehouseForm-Panel_PageItemDetail-application.png) · [memory](runtime/09-WarehouseForm-Panel_PageItemDetail-memory.png) · [world](runtime/09-WarehouseForm-Panel_PageItemDetail-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0224, UIA-0225 |
| 5. 九宫格与边角 | 发现问题 | UIA-0224, UIA-0226 |
| 6. 对齐间距留白 | 发现问题 | UIA-0224 |
| 7. 文字与可读性 | 发现问题 | UIA-0224, UIA-0225, UIA-0226 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0226 |
| 9. 素材与状态语义 | 发现问题 | UIA-0224, UIA-0226 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0224 |
| 12. 同类页面一致性 | 发现问题 | UIA-0226 |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0224 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `WarehouseForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-WarehouseForm-Panel_PageItemDetail-application.json) · [证据2](geometry/09-WarehouseForm-Panel_PageItemDetail-memory.json) · [证据3](geometry/09-WarehouseForm-Panel_PageItemDetail-scroll-top.json) · [证据4](geometry/09-WarehouseForm-Panel_PageItemDetail-state-ItemDetailDisabledState.json) |
| UIA-0225 / P1 / 已确认 | **TEXT_OVERLAP** · `WarehouseForm/Panel_Frame/Grp_PageHost/Panel_PageItemDetail/Txt_ItemDetailTitle ↔ WarehouseForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-WarehouseForm-Panel_PageItemDetail-application.json) · [证据2](geometry/09-WarehouseForm-Panel_PageItemDetail-memory.json) · [证据3](geometry/09-WarehouseForm-Panel_PageItemDetail-scroll-top.json) · [证据4](geometry/09-WarehouseForm-Panel_PageItemDetail-state-base.json) |
| UIA-0226 / P2 / 已确认 | **READONLY_USES_DISABLED_SKIN** · `只读属性/摘要条目`<br>正常可读数据被渲染为禁用 Button，出现斜纹禁用底图和按钮端帽；信息字段看起来像不可用操作，弱化正文。<br>期望：满足标准 5,7,8,9,12：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/09-WarehouseForm-Panel_PageItemDetail-world.png) · [证据2](geometry/09-WarehouseForm-Panel_PageItemDetail-world.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_ItemDetailLoadingState](state-layout/09-WarehouseForm-Panel_PageItemDetail-ItemDetailLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ItemDetailEmptyState](state-layout/09-WarehouseForm-Panel_PageItemDetail-ItemDetailEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ItemDetailErrorState](state-layout/09-WarehouseForm-Panel_PageItemDetail-ItemDetailErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ItemDetailSuccessState](state-layout/09-WarehouseForm-Panel_PageItemDetail-ItemDetailSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ItemDetailDisabledState](state-layout/09-WarehouseForm-Panel_PageItemDetail-ItemDetailDisabledState.png) | 已检查 | application、memory |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-ItemDetail-normal.png)、[compare](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-ItemDetail-compare.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 055 / 09 / 仓库记录 · WarehouseForm/Panel_PageWarehouseRecords

Prefab：[Assets/Game/Prefabs/UI/Operations/WarehouseForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/WarehouseForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/WarehouseForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/WarehouseForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-WarehouseRecords-normal.png)；证据：[application](runtime/09-WarehouseForm-Panel_PageWarehouseRecords-application.png) · [memory](runtime/09-WarehouseForm-Panel_PageWarehouseRecords-memory.png) · [world](runtime/09-WarehouseForm-Panel_PageWarehouseRecords-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0228, UIA-0229 |
| 5. 九宫格与边角 | 发现问题 | UIA-0228 |
| 6. 对齐间距留白 | 发现问题 | UIA-0228 |
| 7. 文字与可读性 | 发现问题 | UIA-0228, UIA-0229 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 发现问题 | UIA-0227, UIA-0228 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0227, UIA-0228 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0227 / P1 / 业务缺口 | **BUSINESS_SOURCE_NOT_CONNECTED** · `运行时数据正文`<br>本次实际运行说明：任务、事务和序列可追溯；筛选与定位尚未接入。<br>期望：满足标准 9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/09-WarehouseForm-Panel_PageWarehouseRecords-world.png) · [证据2](geometry/09-WarehouseForm-Panel_PageWarehouseRecords-world.json) |
| UIA-0228 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `WarehouseForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-WarehouseForm-Panel_PageWarehouseRecords-application.json) · [证据2](geometry/09-WarehouseForm-Panel_PageWarehouseRecords-memory.json) · [证据3](geometry/09-WarehouseForm-Panel_PageWarehouseRecords-scroll-top.json) · [证据4](geometry/09-WarehouseForm-Panel_PageWarehouseRecords-state-WarehouseRecordsDisabledState.json) |
| UIA-0229 / P1 / 已确认 | **TEXT_OVERLAP** · `WarehouseForm/Panel_Frame/Grp_PageHost/Panel_PageWarehouseRecords/Txt_WarehouseRecordsTitle ↔ WarehouseForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/09-WarehouseForm-Panel_PageWarehouseRecords-application.json) · [证据2](geometry/09-WarehouseForm-Panel_PageWarehouseRecords-memory.json) · [证据3](geometry/09-WarehouseForm-Panel_PageWarehouseRecords-scroll-top.json) · [证据4](geometry/09-WarehouseForm-Panel_PageWarehouseRecords-state-base.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_WarehouseRecordsLoadingState](state-layout/09-WarehouseForm-Panel_PageWarehouseRecords-WarehouseRecordsLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_WarehouseRecordsEmptyState](state-layout/09-WarehouseForm-Panel_PageWarehouseRecords-WarehouseRecordsEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_WarehouseRecordsErrorState](state-layout/09-WarehouseForm-Panel_PageWarehouseRecords-WarehouseRecordsErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_WarehouseRecordsSuccessState](state-layout/09-WarehouseForm-Panel_PageWarehouseRecords-WarehouseRecordsSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_WarehouseRecordsDisabledState](state-layout/09-WarehouseForm-Panel_PageWarehouseRecords-WarehouseRecordsDisabledState.png) | 已检查 | application、memory |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-09-WarehouseRecords-normal.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 056 / 10 / 建筑图纸目录与详情 · BuildCatalogForm/Panel_PageBlueprints

Prefab：[Assets/Game/Prefabs/UI/Operations/BuildCatalogForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/BuildCatalogForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/BuildCatalogForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/BuildCatalogForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-10-Blueprints-ready.png)；证据：[application](runtime/10-BuildCatalogForm-Panel_PageBlueprints-application.png) · [memory](runtime/10-BuildCatalogForm-Panel_PageBlueprints-memory.png) · [world](runtime/10-BuildCatalogForm-Panel_PageBlueprints-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0231, UIA-0232, UIA-0233, UIA-0234 |
| 5. 九宫格与边角 | 发现问题 | UIA-0231 |
| 6. 对齐间距留白 | 发现问题 | UIA-0231, UIA-0234 |
| 7. 文字与可读性 | 发现问题 | UIA-0231, UIA-0232, UIA-0233, UIA-0234 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0234 |
| 9. 素材与状态语义 | 发现问题 | UIA-0230, UIA-0231 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0230, UIA-0231, UIA-0232, UIA-0234 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0230 / P1 / 业务缺口 | **BUSINESS_SOURCE_NOT_CONNECTED** · `运行时数据正文`<br>本次实际运行说明：建造图纸域尚未接入运行路径：图纸目录与解锁状态没有数据来源。<br>期望：满足标准 9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/10-BuildCatalogForm-Panel_PageBlueprints-world.png) · [证据2](geometry/10-BuildCatalogForm-Panel_PageBlueprints-world.json) |
| UIA-0231 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `BuildCatalogForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/10-BuildCatalogForm-Panel_PageBlueprints-application.json) · [证据2](geometry/10-BuildCatalogForm-Panel_PageBlueprints-memory.json) · [证据3](geometry/10-BuildCatalogForm-Panel_PageBlueprints-scroll-top.json) · [证据4](geometry/10-BuildCatalogForm-Panel_PageBlueprints-state-BlueprintsDisabledState.json) |
| UIA-0232 / P1 / 已确认 | **TEXT_MASK_CLIP** · `BuildCatalogForm/Panel_Frame/Grp_PageHost/Panel_PageBlueprints/Panel_BlueprintsBlueprint/List_BlueprintsBlueprint/Viewport_BlueprintsBlueprint/Content_BlueprintsBlueprint/Txt_BlueprintsBlueprintBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/10-BuildCatalogForm-Panel_PageBlueprints-application.json) · [证据2](geometry/10-BuildCatalogForm-Panel_PageBlueprints-memory.json) · [证据3](geometry/10-BuildCatalogForm-Panel_PageBlueprints-scroll-top.json) · [证据4](geometry/10-BuildCatalogForm-Panel_PageBlueprints-world-scroll-top.json) |
| UIA-0233 / P1 / 已确认 | **TEXT_OVERLAP** · `BuildCatalogForm/Panel_Frame/Grp_PageHost/Panel_PageBlueprints/Txt_BlueprintsTitle ↔ BuildCatalogForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/10-BuildCatalogForm-Panel_PageBlueprints-application.json) · [证据2](geometry/10-BuildCatalogForm-Panel_PageBlueprints-memory.json) · [证据3](geometry/10-BuildCatalogForm-Panel_PageBlueprints-scroll-top.json) · [证据4](geometry/10-BuildCatalogForm-Panel_PageBlueprints-state-base.json) |
| UIA-0234 / P2 / 已确认 | **UNAVAILABLE_REASON_REPEATED** · `BuildCatalogForm/Panel_Frame/Grp_PageHost/Panel_PageBlueprints/Grp_BlueprintsDisabledState/Panel_BlueprintsDisabledMessage/Txt_BlueprintsDisabledMessage`<br>相同不可用原因同时出现在状态条和多个信息卡中；浮动状态条压在插画/内容区域，重复文字分散焦点。<br>期望：满足标准 4,6,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/10-BuildCatalogForm-Panel_PageBlueprints-world.json) · [证据2](runtime/10-BuildCatalogForm-Panel_PageBlueprints-world.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_BlueprintsLoadingState](state-layout/10-BuildCatalogForm-Panel_PageBlueprints-BlueprintsLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_BlueprintsEmptyState](state-layout/10-BuildCatalogForm-Panel_PageBlueprints-BlueprintsEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_BlueprintsErrorState](state-layout/10-BuildCatalogForm-Panel_PageBlueprints-BlueprintsErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_BlueprintsSuccessState](state-layout/10-BuildCatalogForm-Panel_PageBlueprints-BlueprintsSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_BlueprintsDisabledState](state-layout/10-BuildCatalogForm-Panel_PageBlueprints-BlueprintsDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[ready](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-10-Blueprints-ready.png)、[locked](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-10-Blueprints-locked.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 057 / 10 / 商品与礼包详情 · ShopForm/Panel_PageOfferDetail

Prefab：[Assets/Game/Prefabs/UI/Operations/ShopForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/ShopForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/ShopForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/ShopForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-10-OfferDetail-normal.png)；证据：[application](runtime/10-ShopForm-Panel_PageOfferDetail-application.png) · [memory](runtime/10-ShopForm-Panel_PageOfferDetail-memory.png) · [world](runtime/10-ShopForm-Panel_PageOfferDetail-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0239 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0236, UIA-0237, UIA-0238, UIA-0240 |
| 5. 九宫格与边角 | 发现问题 | UIA-0236 |
| 6. 对齐间距留白 | 发现问题 | UIA-0236, UIA-0240 |
| 7. 文字与可读性 | 发现问题 | UIA-0236, UIA-0237, UIA-0238, UIA-0240 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0239, UIA-0240 |
| 9. 素材与状态语义 | 发现问题 | UIA-0235, UIA-0236, UIA-0239 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0235, UIA-0236, UIA-0237, UIA-0239, UIA-0240 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0235 / P1 / 业务缺口 | **BUSINESS_SOURCE_NOT_CONNECTED** · `运行时数据正文`<br>本次实际运行说明：交易域尚未接入运行路径：报价与结算服务都还没有创建者。<br>期望：满足标准 9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/10-ShopForm-Panel_PageOfferDetail-world.png) · [证据2](geometry/10-ShopForm-Panel_PageOfferDetail-world.json) |
| UIA-0236 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `ShopForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/10-ShopForm-Panel_PageOfferDetail-application.json) · [证据2](geometry/10-ShopForm-Panel_PageOfferDetail-memory.json) · [证据3](geometry/10-ShopForm-Panel_PageOfferDetail-scroll-top.json) · [证据4](geometry/10-ShopForm-Panel_PageOfferDetail-state-OfferDetailDisabledState.json) |
| UIA-0237 / P1 / 已确认 | **TEXT_MASK_CLIP** · `ShopForm/Panel_Frame/Grp_PageHost/Panel_PageOfferDetail/Panel_OfferDetailTerms/List_OfferDetailTerms/Viewport_OfferDetailTerms/Content_OfferDetailTerms/Txt_OfferDetailTermsBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/10-ShopForm-Panel_PageOfferDetail-application.json) · [证据2](geometry/10-ShopForm-Panel_PageOfferDetail-memory.json) · [证据3](geometry/10-ShopForm-Panel_PageOfferDetail-scroll-top.json) · [证据4](geometry/10-ShopForm-Panel_PageOfferDetail-world-scroll-top.json) |
| UIA-0238 / P1 / 已确认 | **TEXT_OVERLAP** · `ShopForm/Panel_Frame/Grp_PageHost/Panel_PageOfferDetail/Txt_OfferDetailTitle ↔ ShopForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/10-ShopForm-Panel_PageOfferDetail-application.json) · [证据2](geometry/10-ShopForm-Panel_PageOfferDetail-memory.json) · [证据3](geometry/10-ShopForm-Panel_PageOfferDetail-scroll-top.json) · [证据4](geometry/10-ShopForm-Panel_PageOfferDetail-state-base.json) |
| UIA-0239 / P2 / 已确认 | **SHOP_CRATE_PRIORITY** · `商店左侧礼包插画`<br>礼包插画成为购买目录、出售资源和详情页的主要视觉内容，实际条目及数量信息的区域被压缩，且出售资源也被同一礼包图代表。<br>期望：满足标准 3,8,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/10-ShopForm-Panel_PageOfferDetail.png) |
| UIA-0240 / P2 / 已确认 | **UNAVAILABLE_REASON_REPEATED** · `ShopForm/Panel_Frame/Grp_PageHost/Panel_PageOfferDetail/Grp_OfferDetailDisabledState/Panel_OfferDetailDisabledMessage/Txt_OfferDetailDisabledMessage`<br>相同不可用原因同时出现在状态条和多个信息卡中；浮动状态条压在插画/内容区域，重复文字分散焦点。<br>期望：满足标准 4,6,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/10-ShopForm-Panel_PageOfferDetail-world.json) · [证据2](runtime/10-ShopForm-Panel_PageOfferDetail-world.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_OfferDetailLoadingState](state-layout/10-ShopForm-Panel_PageOfferDetail-OfferDetailLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_OfferDetailEmptyState](state-layout/10-ShopForm-Panel_PageOfferDetail-OfferDetailEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_OfferDetailErrorState](state-layout/10-ShopForm-Panel_PageOfferDetail-OfferDetailErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_OfferDetailSuccessState](state-layout/10-ShopForm-Panel_PageOfferDetail-OfferDetailSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_OfferDetailDisabledState](state-layout/10-ShopForm-Panel_PageOfferDetail-OfferDetailDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-10-OfferDetail-normal.png)、[cost-up](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-10-OfferDetail-cost-up.png)、[cost-down](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-10-OfferDetail-cost-down.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 058 / 10 / 商店购买 · ShopForm/Panel_PageShopBuy

Prefab：[Assets/Game/Prefabs/UI/Operations/ShopForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/ShopForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/ShopForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/ShopForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-10-ShopBuy-buy.png)；证据：[application](runtime/10-ShopForm-Panel_PageShopBuy-application.png) · [memory](runtime/10-ShopForm-Panel_PageShopBuy-memory.png) · [world](runtime/10-ShopForm-Panel_PageShopBuy-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0245 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0242, UIA-0243, UIA-0244, UIA-0246 |
| 5. 九宫格与边角 | 发现问题 | UIA-0242 |
| 6. 对齐间距留白 | 发现问题 | UIA-0242, UIA-0246 |
| 7. 文字与可读性 | 发现问题 | UIA-0242, UIA-0243, UIA-0244, UIA-0246 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0245, UIA-0246 |
| 9. 素材与状态语义 | 发现问题 | UIA-0241, UIA-0242, UIA-0245 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0241, UIA-0242, UIA-0243, UIA-0245, UIA-0246 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0241 / P1 / 业务缺口 | **BUSINESS_SOURCE_NOT_CONNECTED** · `运行时数据正文`<br>本次实际运行说明：交易域尚未接入运行路径：报价与结算服务都还没有创建者。<br>期望：满足标准 9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/10-ShopForm-Panel_PageShopBuy-world.png) · [证据2](geometry/10-ShopForm-Panel_PageShopBuy-world.json) |
| UIA-0242 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `ShopForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/10-ShopForm-Panel_PageShopBuy-application.json) · [证据2](geometry/10-ShopForm-Panel_PageShopBuy-memory.json) · [证据3](geometry/10-ShopForm-Panel_PageShopBuy-scroll-top.json) · [证据4](geometry/10-ShopForm-Panel_PageShopBuy-state-ShopBuyDisabledState.json) |
| UIA-0243 / P1 / 已确认 | **TEXT_MASK_CLIP** · `ShopForm/Panel_Frame/Grp_PageHost/Panel_PageShopBuy/Panel_ShopBuyOffer/List_ShopBuyOffer/Viewport_ShopBuyOffer/Content_ShopBuyOffer/Txt_ShopBuyOfferBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/10-ShopForm-Panel_PageShopBuy-application.json) · [证据2](geometry/10-ShopForm-Panel_PageShopBuy-memory.json) · [证据3](geometry/10-ShopForm-Panel_PageShopBuy-scroll-top.json) · [证据4](geometry/10-ShopForm-Panel_PageShopBuy-world-scroll-top.json) |
| UIA-0244 / P1 / 已确认 | **TEXT_OVERLAP** · `ShopForm/Panel_Frame/Grp_PageHost/Panel_PageShopBuy/Txt_ShopBuyTitle ↔ ShopForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/10-ShopForm-Panel_PageShopBuy-application.json) · [证据2](geometry/10-ShopForm-Panel_PageShopBuy-memory.json) · [证据3](geometry/10-ShopForm-Panel_PageShopBuy-scroll-top.json) · [证据4](geometry/10-ShopForm-Panel_PageShopBuy-state-base.json) |
| UIA-0245 / P2 / 已确认 | **SHOP_CRATE_PRIORITY** · `商店左侧礼包插画`<br>礼包插画成为购买目录、出售资源和详情页的主要视觉内容，实际条目及数量信息的区域被压缩，且出售资源也被同一礼包图代表。<br>期望：满足标准 3,8,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/10-ShopForm-Panel_PageShopBuy.png) |
| UIA-0246 / P2 / 已确认 | **UNAVAILABLE_REASON_REPEATED** · `ShopForm/Panel_Frame/Grp_PageHost/Panel_PageShopBuy/Grp_ShopBuyDisabledState/Panel_ShopBuyDisabledMessage/Txt_ShopBuyDisabledMessage`<br>相同不可用原因同时出现在状态条和多个信息卡中；浮动状态条压在插画/内容区域，重复文字分散焦点。<br>期望：满足标准 4,6,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/10-ShopForm-Panel_PageShopBuy-world.json) · [证据2](runtime/10-ShopForm-Panel_PageShopBuy-world.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_ShopBuyLoadingState](state-layout/10-ShopForm-Panel_PageShopBuy-ShopBuyLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ShopBuyEmptyState](state-layout/10-ShopForm-Panel_PageShopBuy-ShopBuyEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ShopBuyErrorState](state-layout/10-ShopForm-Panel_PageShopBuy-ShopBuyErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ShopBuySuccessState](state-layout/10-ShopForm-Panel_PageShopBuy-ShopBuySuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ShopBuyDisabledState](state-layout/10-ShopForm-Panel_PageShopBuy-ShopBuyDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[buy](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-10-ShopBuy-buy.png)、[locked](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-10-ShopBuy-locked.png)、[insufficient](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-10-ShopBuy-insufficient.png)、[buy](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-10.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 059 / 10 / 商店出售 · ShopForm/Panel_PageShopSell

Prefab：[Assets/Game/Prefabs/UI/Operations/ShopForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/ShopForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/ShopForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/ShopForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-10-ShopSell-sell.png)；证据：[application](runtime/10-ShopForm-Panel_PageShopSell-application.png) · [memory](runtime/10-ShopForm-Panel_PageShopSell-memory.png) · [world](runtime/10-ShopForm-Panel_PageShopSell-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0252 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0248, UIA-0249, UIA-0250, UIA-0251, UIA-0253 |
| 5. 九宫格与边角 | 发现问题 | UIA-0249 |
| 6. 对齐间距留白 | 发现问题 | UIA-0249, UIA-0253 |
| 7. 文字与可读性 | 发现问题 | UIA-0248, UIA-0249, UIA-0250, UIA-0251, UIA-0253 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0252, UIA-0253 |
| 9. 素材与状态语义 | 发现问题 | UIA-0247, UIA-0249, UIA-0252 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0248 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0247, UIA-0249, UIA-0250, UIA-0252, UIA-0253 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0247 / P1 / 业务缺口 | **BUSINESS_SOURCE_NOT_CONNECTED** · `运行时数据正文`<br>本次实际运行说明：交易域尚未接入运行路径：报价与结算服务都还没有创建者。<br>期望：满足标准 9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/10-ShopForm-Panel_PageShopSell-world.png) · [证据2](geometry/10-ShopForm-Panel_PageShopSell-world.json) |
| UIA-0248 / P1 / 已确认 | **SELL_QUANTITY_CLIPPED** · `ShopSell 数量输入框`<br>数量输入框位于列表内容底部，默认可见区域只露出上沿和一部分文字。<br>期望：满足标准 4,7,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/10-ShopForm-Panel_PageShopSell.png) |
| UIA-0249 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `ShopForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/10-ShopForm-Panel_PageShopSell-application.json) · [证据2](geometry/10-ShopForm-Panel_PageShopSell-memory.json) · [证据3](geometry/10-ShopForm-Panel_PageShopSell-scroll-bottom.json) · [证据4](geometry/10-ShopForm-Panel_PageShopSell-scroll-top.json) |
| UIA-0250 / P1 / 已确认 | **TEXT_MASK_CLIP** · `ShopForm/Panel_Frame/Grp_PageHost/Panel_PageShopSell/Panel_ShopSellQuote/List_ShopSellQuote/Viewport_ShopSellQuote/Content_ShopSellQuote/Txt_ShopSellQuoteBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/10-ShopForm-Panel_PageShopSell-application.json) · [证据2](geometry/10-ShopForm-Panel_PageShopSell-memory.json) · [证据3](geometry/10-ShopForm-Panel_PageShopSell-scroll-bottom.json) · [证据4](geometry/10-ShopForm-Panel_PageShopSell-scroll-top.json) |
| UIA-0251 / P1 / 已确认 | **TEXT_OVERLAP** · `ShopForm/Panel_Frame/Grp_PageHost/Panel_PageShopSell/Txt_ShopSellTitle ↔ ShopForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/10-ShopForm-Panel_PageShopSell-application.json) · [证据2](geometry/10-ShopForm-Panel_PageShopSell-memory.json) · [证据3](geometry/10-ShopForm-Panel_PageShopSell-scroll-bottom.json) · [证据4](geometry/10-ShopForm-Panel_PageShopSell-scroll-top.json) |
| UIA-0252 / P2 / 已确认 | **SHOP_CRATE_PRIORITY** · `商店左侧礼包插画`<br>礼包插画成为购买目录、出售资源和详情页的主要视觉内容，实际条目及数量信息的区域被压缩，且出售资源也被同一礼包图代表。<br>期望：满足标准 3,8,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/10-ShopForm-Panel_PageShopSell.png) |
| UIA-0253 / P2 / 已确认 | **UNAVAILABLE_REASON_REPEATED** · `ShopForm/Panel_Frame/Grp_PageHost/Panel_PageShopSell/Grp_ShopSellDisabledState/Panel_ShopSellDisabledMessage/Txt_ShopSellDisabledMessage`<br>相同不可用原因同时出现在状态条和多个信息卡中；浮动状态条压在插画/内容区域，重复文字分散焦点。<br>期望：满足标准 4,6,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/10-ShopForm-Panel_PageShopSell-world.json) · [证据2](runtime/10-ShopForm-Panel_PageShopSell-world.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_ShopSellLoadingState](state-layout/10-ShopForm-Panel_PageShopSell-ShopSellLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ShopSellEmptyState](state-layout/10-ShopForm-Panel_PageShopSell-ShopSellEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ShopSellErrorState](state-layout/10-ShopForm-Panel_PageShopSell-ShopSellErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ShopSellSuccessState](state-layout/10-ShopForm-Panel_PageShopSell-ShopSellSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ShopSellDisabledState](state-layout/10-ShopForm-Panel_PageShopSell-ShopSellDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[sell](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-10-ShopSell-sell.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 060 / 11 / 载体升级 · UpgradeForm/Panel_PageCarrierUpgrade

Prefab：[Assets/Game/Prefabs/UI/Operations/UpgradeForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/UpgradeForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/UpgradeForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/UpgradeForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-11-CarrierUpgrade-before.png)；证据：[application](runtime/11-UpgradeForm-Panel_PageCarrierUpgrade-application.png) · [memory](runtime/11-UpgradeForm-Panel_PageCarrierUpgrade-memory.png) · [world](runtime/11-UpgradeForm-Panel_PageCarrierUpgrade-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0260 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0255, UIA-0256, UIA-0257, UIA-0258, UIA-0259, UIA-0261 |
| 5. 九宫格与边角 | 发现问题 | UIA-0255 |
| 6. 对齐间距留白 | 发现问题 | UIA-0255, UIA-0260, UIA-0261 |
| 7. 文字与可读性 | 发现问题 | UIA-0255, UIA-0256, UIA-0257, UIA-0258, UIA-0259, UIA-0260, UIA-0261 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0260, UIA-0261 |
| 9. 素材与状态语义 | 发现问题 | UIA-0254, UIA-0255 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0254, UIA-0255, UIA-0256, UIA-0257, UIA-0260, UIA-0261 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0254 / P1 / 业务缺口 | **BUSINESS_SOURCE_NOT_CONNECTED** · `运行时数据正文`<br>本次实际运行说明：机器改装与组件域尚未接入运行路径：改装项与代价无法计算。<br>期望：满足标准 9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/11-UpgradeForm-Panel_PageCarrierUpgrade-world.png) · [证据2](geometry/11-UpgradeForm-Panel_PageCarrierUpgrade-world.json) |
| UIA-0255 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `UpgradeForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/11-UpgradeForm-Panel_PageCarrierUpgrade-application.json) · [证据2](geometry/11-UpgradeForm-Panel_PageCarrierUpgrade-memory.json) · [证据3](geometry/11-UpgradeForm-Panel_PageCarrierUpgrade-scroll-top.json) · [证据4](geometry/11-UpgradeForm-Panel_PageCarrierUpgrade-state-CarrierUpgradeDisabledState.json) |
| UIA-0256 / P1 / 已确认 | **TEXT_MASK_CLIP** · `UpgradeForm/Panel_Frame/Grp_PageHost/Panel_PageCarrierUpgrade/Panel_CarrierUpgradeCost/List_CarrierUpgradeCost/Viewport_CarrierUpgradeCost/Content_CarrierUpgradeCost/Txt_CarrierUpgradeCostBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/11-UpgradeForm-Panel_PageCarrierUpgrade-application.json) · [证据2](geometry/11-UpgradeForm-Panel_PageCarrierUpgrade-memory.json) · [证据3](geometry/11-UpgradeForm-Panel_PageCarrierUpgrade-scroll-top.json) · [证据4](geometry/11-UpgradeForm-Panel_PageCarrierUpgrade-world-scroll-top.json) |
| UIA-0257 / P1 / 已确认 | **TEXT_OVERFLOW** · `UpgradeForm/Panel_Frame/Grp_PageHost/Panel_PageCarrierUpgrade/Panel_CarrierUpgradeCost/Txt_CarrierUpgradeCostHeading`<br>文字超出容器，字号 21；容器 92×30<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/11-UpgradeForm-Panel_PageCarrierUpgrade-application.json) · [证据2](geometry/11-UpgradeForm-Panel_PageCarrierUpgrade-memory.json) · [证据3](geometry/11-UpgradeForm-Panel_PageCarrierUpgrade-scroll-top.json) · [证据4](geometry/11-UpgradeForm-Panel_PageCarrierUpgrade-state-base.json) |
| UIA-0258 / P1 / 已确认 | **TEXT_OVERLAP** · `UpgradeForm/Panel_Frame/Grp_PageHost/Panel_PageCarrierUpgrade/Panel_CarrierUpgradeCost/List_CarrierUpgradeCost/Viewport_CarrierUpgradeCost/Content_CarrierUpgradeCost/Txt_CarrierUpgradeCostBody ↔ UpgradeForm/Panel_Frame/Grp_PageHost/Panel_PageCarrierUpgrade/Panel_CarrierUpgradeCost/Txt_CarrierUpgradeCostHeading`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/11-UpgradeForm-Panel_PageCarrierUpgrade-application.json) · [证据2](geometry/11-UpgradeForm-Panel_PageCarrierUpgrade-memory.json) · [证据3](geometry/11-UpgradeForm-Panel_PageCarrierUpgrade-scroll-top.json) · [证据4](geometry/11-UpgradeForm-Panel_PageCarrierUpgrade-state-base.json) |
| UIA-0259 / P1 / 已确认 | **TEXT_OVERLAP** · `UpgradeForm/Panel_Frame/Grp_PageHost/Panel_PageCarrierUpgrade/Txt_CarrierUpgradeTitle ↔ UpgradeForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/11-UpgradeForm-Panel_PageCarrierUpgrade-application.json) · [证据2](geometry/11-UpgradeForm-Panel_PageCarrierUpgrade-memory.json) · [证据3](geometry/11-UpgradeForm-Panel_PageCarrierUpgrade-scroll-top.json) · [证据4](geometry/11-UpgradeForm-Panel_PageCarrierUpgrade-state-base.json) |
| UIA-0260 / P1 / 已确认 | **UPGRADE_COST_TOO_NARROW** · `费用与限制卡片`<br>费用与限制被分配到约 128px 宽的中间竖栏，标题、费用/折价文字被迫拆成短行，无法承载实际价格和说明。<br>期望：满足标准 3,6,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/11-UpgradeForm-Panel_PageCarrierUpgrade.png) |
| UIA-0261 / P2 / 已确认 | **UNAVAILABLE_REASON_REPEATED** · `UpgradeForm/Panel_Frame/Grp_PageHost/Panel_PageCarrierUpgrade/Grp_CarrierUpgradeDisabledState/Panel_CarrierUpgradeDisabledMessage/Txt_CarrierUpgradeDisabledMessage`<br>相同不可用原因同时出现在状态条和多个信息卡中；浮动状态条压在插画/内容区域，重复文字分散焦点。<br>期望：满足标准 4,6,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/11-UpgradeForm-Panel_PageCarrierUpgrade-world.json) · [证据2](runtime/11-UpgradeForm-Panel_PageCarrierUpgrade-world.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_CarrierUpgradeLoadingState](state-layout/11-UpgradeForm-Panel_PageCarrierUpgrade-CarrierUpgradeLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_CarrierUpgradeEmptyState](state-layout/11-UpgradeForm-Panel_PageCarrierUpgrade-CarrierUpgradeEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_CarrierUpgradeErrorState](state-layout/11-UpgradeForm-Panel_PageCarrierUpgrade-CarrierUpgradeErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_CarrierUpgradeSuccessState](state-layout/11-UpgradeForm-Panel_PageCarrierUpgrade-CarrierUpgradeSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_CarrierUpgradeDisabledState](state-layout/11-UpgradeForm-Panel_PageCarrierUpgrade-CarrierUpgradeDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[before](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-11-CarrierUpgrade-before.png)、[after](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-11-CarrierUpgrade-after.png)、[locked](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-11-CarrierUpgrade-locked.png)、[confirm](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-11-CarrierUpgrade-confirm.png)、[before](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-11.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 061 / 11 / 组件升级 · UpgradeForm/Panel_PageComponentUpgrade

Prefab：[Assets/Game/Prefabs/UI/Operations/UpgradeForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/UpgradeForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/UpgradeForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/UpgradeForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-11-ComponentUpgrade-before.png)；证据：[application](runtime/11-UpgradeForm-Panel_PageComponentUpgrade-application.png) · [memory](runtime/11-UpgradeForm-Panel_PageComponentUpgrade-memory.png) · [world](runtime/11-UpgradeForm-Panel_PageComponentUpgrade-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0268 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0263, UIA-0264, UIA-0265, UIA-0266, UIA-0267, UIA-0269 |
| 5. 九宫格与边角 | 发现问题 | UIA-0263 |
| 6. 对齐间距留白 | 发现问题 | UIA-0263, UIA-0268, UIA-0269 |
| 7. 文字与可读性 | 发现问题 | UIA-0263, UIA-0264, UIA-0265, UIA-0266, UIA-0267, UIA-0268, UIA-0269 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0268, UIA-0269 |
| 9. 素材与状态语义 | 发现问题 | UIA-0262, UIA-0263 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0262, UIA-0263, UIA-0264, UIA-0265, UIA-0268, UIA-0269 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0262 / P1 / 业务缺口 | **BUSINESS_SOURCE_NOT_CONNECTED** · `运行时数据正文`<br>本次实际运行说明：机器改装与组件域尚未接入运行路径：改装项与代价无法计算。<br>期望：满足标准 9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/11-UpgradeForm-Panel_PageComponentUpgrade-world.png) · [证据2](geometry/11-UpgradeForm-Panel_PageComponentUpgrade-world.json) |
| UIA-0263 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `UpgradeForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/11-UpgradeForm-Panel_PageComponentUpgrade-application.json) · [证据2](geometry/11-UpgradeForm-Panel_PageComponentUpgrade-memory.json) · [证据3](geometry/11-UpgradeForm-Panel_PageComponentUpgrade-scroll-top.json) · [证据4](geometry/11-UpgradeForm-Panel_PageComponentUpgrade-state-ComponentUpgradeDisabledState.json) |
| UIA-0264 / P1 / 已确认 | **TEXT_MASK_CLIP** · `UpgradeForm/Panel_Frame/Grp_PageHost/Panel_PageComponentUpgrade/Panel_ComponentUpgradeCost/List_ComponentUpgradeCost/Viewport_ComponentUpgradeCost/Content_ComponentUpgradeCost/Txt_ComponentUpgradeCostBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/11-UpgradeForm-Panel_PageComponentUpgrade-application.json) · [证据2](geometry/11-UpgradeForm-Panel_PageComponentUpgrade-memory.json) · [证据3](geometry/11-UpgradeForm-Panel_PageComponentUpgrade-scroll-top.json) · [证据4](geometry/11-UpgradeForm-Panel_PageComponentUpgrade-world-scroll-top.json) |
| UIA-0265 / P1 / 已确认 | **TEXT_OVERFLOW** · `UpgradeForm/Panel_Frame/Grp_PageHost/Panel_PageComponentUpgrade/Panel_ComponentUpgradeCost/Txt_ComponentUpgradeCostHeading`<br>文字超出容器，字号 21；容器 92×30<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/11-UpgradeForm-Panel_PageComponentUpgrade-application.json) · [证据2](geometry/11-UpgradeForm-Panel_PageComponentUpgrade-memory.json) · [证据3](geometry/11-UpgradeForm-Panel_PageComponentUpgrade-scroll-top.json) · [证据4](geometry/11-UpgradeForm-Panel_PageComponentUpgrade-state-base.json) |
| UIA-0266 / P1 / 已确认 | **TEXT_OVERLAP** · `UpgradeForm/Panel_Frame/Grp_PageHost/Panel_PageComponentUpgrade/Panel_ComponentUpgradeCost/List_ComponentUpgradeCost/Viewport_ComponentUpgradeCost/Content_ComponentUpgradeCost/Txt_ComponentUpgradeCostBody ↔ UpgradeForm/Panel_Frame/Grp_PageHost/Panel_PageComponentUpgrade/Panel_ComponentUpgradeCost/Txt_ComponentUpgradeCostHeading`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/11-UpgradeForm-Panel_PageComponentUpgrade-application.json) · [证据2](geometry/11-UpgradeForm-Panel_PageComponentUpgrade-memory.json) · [证据3](geometry/11-UpgradeForm-Panel_PageComponentUpgrade-scroll-top.json) · [证据4](geometry/11-UpgradeForm-Panel_PageComponentUpgrade-state-base.json) |
| UIA-0267 / P1 / 已确认 | **TEXT_OVERLAP** · `UpgradeForm/Panel_Frame/Grp_PageHost/Panel_PageComponentUpgrade/Txt_ComponentUpgradeTitle ↔ UpgradeForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/11-UpgradeForm-Panel_PageComponentUpgrade-application.json) · [证据2](geometry/11-UpgradeForm-Panel_PageComponentUpgrade-memory.json) · [证据3](geometry/11-UpgradeForm-Panel_PageComponentUpgrade-scroll-top.json) · [证据4](geometry/11-UpgradeForm-Panel_PageComponentUpgrade-state-base.json) |
| UIA-0268 / P1 / 已确认 | **UPGRADE_COST_TOO_NARROW** · `费用与限制卡片`<br>费用与限制被分配到约 128px 宽的中间竖栏，标题、费用/折价文字被迫拆成短行，无法承载实际价格和说明。<br>期望：满足标准 3,6,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/11-UpgradeForm-Panel_PageComponentUpgrade.png) |
| UIA-0269 / P2 / 已确认 | **UNAVAILABLE_REASON_REPEATED** · `UpgradeForm/Panel_Frame/Grp_PageHost/Panel_PageComponentUpgrade/Grp_ComponentUpgradeDisabledState/Panel_ComponentUpgradeDisabledMessage/Txt_ComponentUpgradeDisabledMessage`<br>相同不可用原因同时出现在状态条和多个信息卡中；浮动状态条压在插画/内容区域，重复文字分散焦点。<br>期望：满足标准 4,6,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/11-UpgradeForm-Panel_PageComponentUpgrade-world.json) · [证据2](runtime/11-UpgradeForm-Panel_PageComponentUpgrade-world.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_ComponentUpgradeLoadingState](state-layout/11-UpgradeForm-Panel_PageComponentUpgrade-ComponentUpgradeLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ComponentUpgradeEmptyState](state-layout/11-UpgradeForm-Panel_PageComponentUpgrade-ComponentUpgradeEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ComponentUpgradeErrorState](state-layout/11-UpgradeForm-Panel_PageComponentUpgrade-ComponentUpgradeErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ComponentUpgradeSuccessState](state-layout/11-UpgradeForm-Panel_PageComponentUpgrade-ComponentUpgradeSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ComponentUpgradeDisabledState](state-layout/11-UpgradeForm-Panel_PageComponentUpgrade-ComponentUpgradeDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[before](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-11-ComponentUpgrade-before.png)、[after](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-11-ComponentUpgrade-after.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 062 / 11 / 改装影响预览 · UpgradeForm/Panel_PageModificationImpact

Prefab：[Assets/Game/Prefabs/UI/Operations/UpgradeForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/UpgradeForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/UpgradeForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/UpgradeForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-11-ModificationImpact-danger.png)；证据：[application](runtime/11-UpgradeForm-Panel_PageModificationImpact-application.png) · [memory](runtime/11-UpgradeForm-Panel_PageModificationImpact-memory.png) · [world](runtime/11-UpgradeForm-Panel_PageModificationImpact-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0271, UIA-0272, UIA-0273 |
| 5. 九宫格与边角 | 发现问题 | UIA-0271 |
| 6. 对齐间距留白 | 发现问题 | UIA-0271, UIA-0273 |
| 7. 文字与可读性 | 发现问题 | UIA-0271, UIA-0272, UIA-0273 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0273 |
| 9. 素材与状态语义 | 发现问题 | UIA-0270, UIA-0271 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0270, UIA-0271, UIA-0273 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0270 / P1 / 业务缺口 | **BUSINESS_SOURCE_NOT_CONNECTED** · `运行时数据正文`<br>本次实际运行说明：机器改装与组件域尚未接入运行路径：改装项与代价无法计算。<br>期望：满足标准 9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/11-UpgradeForm-Panel_PageModificationImpact-world.png) · [证据2](geometry/11-UpgradeForm-Panel_PageModificationImpact-world.json) |
| UIA-0271 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `UpgradeForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/11-UpgradeForm-Panel_PageModificationImpact-application.json) · [证据2](geometry/11-UpgradeForm-Panel_PageModificationImpact-memory.json) · [证据3](geometry/11-UpgradeForm-Panel_PageModificationImpact-scroll-top.json) · [证据4](geometry/11-UpgradeForm-Panel_PageModificationImpact-state-ModificationImpactDisabledState.json) |
| UIA-0272 / P1 / 已确认 | **TEXT_OVERLAP** · `UpgradeForm/Panel_Frame/Grp_PageHost/Panel_PageModificationImpact/Txt_ModificationImpactTitle ↔ UpgradeForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/11-UpgradeForm-Panel_PageModificationImpact-application.json) · [证据2](geometry/11-UpgradeForm-Panel_PageModificationImpact-memory.json) · [证据3](geometry/11-UpgradeForm-Panel_PageModificationImpact-scroll-top.json) · [证据4](geometry/11-UpgradeForm-Panel_PageModificationImpact-state-base.json) |
| UIA-0273 / P2 / 已确认 | **UNAVAILABLE_REASON_REPEATED** · `UpgradeForm/Panel_Frame/Grp_PageHost/Panel_PageModificationImpact/Grp_ModificationImpactDisabledState/Panel_ModificationImpactDisabledMessage/Txt_ModificationImpactDisabledMessage`<br>相同不可用原因同时出现在状态条和多个信息卡中；浮动状态条压在插画/内容区域，重复文字分散焦点。<br>期望：满足标准 4,6,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/11-UpgradeForm-Panel_PageModificationImpact-world.json) · [证据2](runtime/11-UpgradeForm-Panel_PageModificationImpact-world.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_ModificationImpactLoadingState](state-layout/11-UpgradeForm-Panel_PageModificationImpact-ModificationImpactLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ModificationImpactEmptyState](state-layout/11-UpgradeForm-Panel_PageModificationImpact-ModificationImpactEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ModificationImpactErrorState](state-layout/11-UpgradeForm-Panel_PageModificationImpact-ModificationImpactErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ModificationImpactSuccessState](state-layout/11-UpgradeForm-Panel_PageModificationImpact-ModificationImpactSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ModificationImpactDisabledState](state-layout/11-UpgradeForm-Panel_PageModificationImpact-ModificationImpactDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[danger](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-11-ModificationImpact-danger.png)、[confirm](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-11-ModificationImpact-confirm.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 063 / 12 / 模板集中待绑定 · AlgorithmBindingForm/Panel_PagePendingBindings

Prefab：[Assets/Game/Prefabs/UI/Operations/AlgorithmBindingForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/AlgorithmBindingForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/AlgorithmBindingForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/AlgorithmBindingForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-12-PendingBindings-pending.png)；证据：[application](runtime/12-AlgorithmBindingForm-Panel_PagePendingBindings-application.png) · [memory](runtime/12-AlgorithmBindingForm-Panel_PagePendingBindings-memory.png) · [world](runtime/12-AlgorithmBindingForm-Panel_PagePendingBindings-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0274, UIA-0275, UIA-0276 |
| 5. 九宫格与边角 | 发现问题 | UIA-0274 |
| 6. 对齐间距留白 | 发现问题 | UIA-0274, UIA-0276 |
| 7. 文字与可读性 | 发现问题 | UIA-0274, UIA-0275, UIA-0276 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0276 |
| 9. 素材与状态语义 | 发现问题 | UIA-0274 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0274, UIA-0276 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0274 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `AlgorithmBindingForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/12-AlgorithmBindingForm-Panel_PagePendingBindings-application.json) · [证据2](geometry/12-AlgorithmBindingForm-Panel_PagePendingBindings-memory.json) · [证据3](geometry/12-AlgorithmBindingForm-Panel_PagePendingBindings-scroll-top.json) · [证据4](geometry/12-AlgorithmBindingForm-Panel_PagePendingBindings-state-PendingBindingsDisabledState.json) |
| UIA-0275 / P1 / 已确认 | **TEXT_OVERLAP** · `AlgorithmBindingForm/Panel_Frame/Grp_PageHost/Panel_PagePendingBindings/Txt_PendingBindingsTitle ↔ AlgorithmBindingForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/12-AlgorithmBindingForm-Panel_PagePendingBindings-application.json) · [证据2](geometry/12-AlgorithmBindingForm-Panel_PagePendingBindings-memory.json) · [证据3](geometry/12-AlgorithmBindingForm-Panel_PagePendingBindings-scroll-top.json) · [证据4](geometry/12-AlgorithmBindingForm-Panel_PagePendingBindings-state-base.json) |
| UIA-0276 / P2 / 已确认 | **UNAVAILABLE_REASON_REPEATED** · `AlgorithmBindingForm/Panel_Frame/Grp_PageHost/Panel_PagePendingBindings/Grp_PendingBindingsDisabledState/Panel_PendingBindingsDisabledMessage/Txt_PendingBindingsDisabledMessage`<br>相同不可用原因同时出现在状态条和多个信息卡中；浮动状态条压在插画/内容区域，重复文字分散焦点。<br>期望：满足标准 4,6,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/12-AlgorithmBindingForm-Panel_PagePendingBindings-world.json) · [证据2](runtime/12-AlgorithmBindingForm-Panel_PagePendingBindings-world.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_PendingBindingsLoadingState](state-layout/12-AlgorithmBindingForm-Panel_PagePendingBindings-PendingBindingsLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_PendingBindingsEmptyState](state-layout/12-AlgorithmBindingForm-Panel_PagePendingBindings-PendingBindingsEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_PendingBindingsErrorState](state-layout/12-AlgorithmBindingForm-Panel_PagePendingBindings-PendingBindingsErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_PendingBindingsSuccessState](state-layout/12-AlgorithmBindingForm-Panel_PagePendingBindings-PendingBindingsSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_PendingBindingsDisabledState](state-layout/12-AlgorithmBindingForm-Panel_PagePendingBindings-PendingBindingsDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[pending](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-12-PendingBindings-pending.png)、[multi](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-12-PendingBindings-multi.png)、[linked](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-12-PendingBindings-linked.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 064 / 12 / 安装替换组件选择器 · ComponentPickerForm/Panel_PageComponentPicker

Prefab：[Assets/Game/Prefabs/UI/Operations/ComponentPickerForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/ComponentPickerForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/ComponentPickerForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/ComponentPickerForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-12-ComponentPicker-compatible.png)；证据：[application](runtime/12-ComponentPickerForm-Panel_PageComponentPicker-application.png) · [memory](runtime/12-ComponentPickerForm-Panel_PageComponentPicker-memory.png) · [world](runtime/12-ComponentPickerForm-Panel_PageComponentPicker-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0277, UIA-0278 |
| 5. 九宫格与边角 | 发现问题 | UIA-0277 |
| 6. 对齐间距留白 | 发现问题 | UIA-0277 |
| 7. 文字与可读性 | 发现问题 | UIA-0277, UIA-0278 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 发现问题 | UIA-0277 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0277 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0277 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `ComponentPickerForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/12-ComponentPickerForm-Panel_PageComponentPicker-application.json) · [证据2](geometry/12-ComponentPickerForm-Panel_PageComponentPicker-memory.json) · [证据3](geometry/12-ComponentPickerForm-Panel_PageComponentPicker-scroll-top.json) · [证据4](geometry/12-ComponentPickerForm-Panel_PageComponentPicker-state-ComponentPickerDisabledState.json) |
| UIA-0278 / P1 / 已确认 | **TEXT_OVERLAP** · `ComponentPickerForm/Panel_Frame/Grp_PageHost/Panel_PageComponentPicker/Txt_ComponentPickerTitle ↔ ComponentPickerForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/12-ComponentPickerForm-Panel_PageComponentPicker-application.json) · [证据2](geometry/12-ComponentPickerForm-Panel_PageComponentPicker-memory.json) · [证据3](geometry/12-ComponentPickerForm-Panel_PageComponentPicker-scroll-top.json) · [证据4](geometry/12-ComponentPickerForm-Panel_PageComponentPicker-state-base.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_ComponentPickerLoadingState](state-layout/12-ComponentPickerForm-Panel_PageComponentPicker-ComponentPickerLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ComponentPickerEmptyState](state-layout/12-ComponentPickerForm-Panel_PageComponentPicker-ComponentPickerEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ComponentPickerErrorState](state-layout/12-ComponentPickerForm-Panel_PageComponentPicker-ComponentPickerErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ComponentPickerSuccessState](state-layout/12-ComponentPickerForm-Panel_PageComponentPicker-ComponentPickerSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ComponentPickerDisabledState](state-layout/12-ComponentPickerForm-Panel_PageComponentPicker-ComponentPickerDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[compatible](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-12-ComponentPicker-compatible.png)、[incompatible](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-12-ComponentPicker-incompatible.png)、[empty](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-12-ComponentPicker-empty.png)、[compatible](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-12.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 065 / 12 / 节点组件选择器 · NodeComponentPickerForm/Panel_PageNodeComponentPicker

Prefab：[Assets/Game/Prefabs/UI/Operations/NodeComponentPickerForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/NodeComponentPickerForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/NodeComponentPickerForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/NodeComponentPickerForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-12-NodeComponentPicker-compatible.png)；证据：[application](runtime/12-NodeComponentPickerForm-Panel_PageNodeComponentPicker-application.png) · [memory](runtime/12-NodeComponentPickerForm-Panel_PageNodeComponentPicker-memory.png) · [world](runtime/12-NodeComponentPickerForm-Panel_PageNodeComponentPicker-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0279, UIA-0280, UIA-0281 |
| 5. 九宫格与边角 | 发现问题 | UIA-0279 |
| 6. 对齐间距留白 | 发现问题 | UIA-0279, UIA-0281 |
| 7. 文字与可读性 | 发现问题 | UIA-0279, UIA-0280, UIA-0281 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0281 |
| 9. 素材与状态语义 | 发现问题 | UIA-0279 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0279, UIA-0281 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0279 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `NodeComponentPickerForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/12-NodeComponentPickerForm-Panel_PageNodeComponentPicker-application.json) · [证据2](geometry/12-NodeComponentPickerForm-Panel_PageNodeComponentPicker-memory.json) · [证据3](geometry/12-NodeComponentPickerForm-Panel_PageNodeComponentPicker-scroll-top.json) · [证据4](geometry/12-NodeComponentPickerForm-Panel_PageNodeComponentPicker-state-NodeComponentPickerDisabledState.json) |
| UIA-0280 / P1 / 已确认 | **TEXT_OVERLAP** · `NodeComponentPickerForm/Panel_Frame/Grp_PageHost/Panel_PageNodeComponentPicker/Txt_NodeComponentPickerTitle ↔ NodeComponentPickerForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/12-NodeComponentPickerForm-Panel_PageNodeComponentPicker-application.json) · [证据2](geometry/12-NodeComponentPickerForm-Panel_PageNodeComponentPicker-memory.json) · [证据3](geometry/12-NodeComponentPickerForm-Panel_PageNodeComponentPicker-scroll-top.json) · [证据4](geometry/12-NodeComponentPickerForm-Panel_PageNodeComponentPicker-state-base.json) |
| UIA-0281 / P2 / 已确认 | **UNAVAILABLE_REASON_REPEATED** · `NodeComponentPickerForm/Panel_Frame/Grp_PageHost/Panel_PageNodeComponentPicker/Grp_NodeComponentPickerDisabledState/Panel_NodeComponentPickerDisabledMessage/Txt_NodeComponentPickerDisabledMessage`<br>相同不可用原因同时出现在状态条和多个信息卡中；浮动状态条压在插画/内容区域，重复文字分散焦点。<br>期望：满足标准 4,6,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/12-NodeComponentPickerForm-Panel_PageNodeComponentPicker-world.json) · [证据2](runtime/12-NodeComponentPickerForm-Panel_PageNodeComponentPicker-world.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_NodeComponentPickerLoadingState](state-layout/12-NodeComponentPickerForm-Panel_PageNodeComponentPicker-NodeComponentPickerLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_NodeComponentPickerEmptyState](state-layout/12-NodeComponentPickerForm-Panel_PageNodeComponentPicker-NodeComponentPickerEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_NodeComponentPickerErrorState](state-layout/12-NodeComponentPickerForm-Panel_PageNodeComponentPicker-NodeComponentPickerErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_NodeComponentPickerSuccessState](state-layout/12-NodeComponentPickerForm-Panel_PageNodeComponentPicker-NodeComponentPickerSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_NodeComponentPickerDisabledState](state-layout/12-NodeComponentPickerForm-Panel_PageNodeComponentPicker-NodeComponentPickerDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[compatible](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-12-NodeComponentPicker-compatible.png)、[incompatible](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-12-NodeComponentPicker-incompatible.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 066 / 12 / 世界对象选择器 · WorldObjectPickerForm/Panel_PageWorldObjectPicker

Prefab：[Assets/Game/Prefabs/UI/Operations/WorldObjectPickerForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/WorldObjectPickerForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/WorldObjectPickerForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/WorldObjectPickerForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-12-WorldObjectPicker-matching.png)；证据：[application](runtime/12-WorldObjectPickerForm-Panel_PageWorldObjectPicker-application.png) · [memory](runtime/12-WorldObjectPickerForm-Panel_PageWorldObjectPicker-memory.png) · [world](runtime/12-WorldObjectPickerForm-Panel_PageWorldObjectPicker-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 发现问题 | UIA-0282 |
| 3. 尺寸与比例 | 发现问题 | UIA-0282 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0282, UIA-0283, UIA-0284, UIA-0285 |
| 5. 九宫格与边角 | 发现问题 | UIA-0283 |
| 6. 对齐间距留白 | 发现问题 | UIA-0283 |
| 7. 文字与可读性 | 发现问题 | UIA-0283, UIA-0284 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0282, UIA-0285 |
| 9. 素材与状态语义 | 发现问题 | UIA-0283, UIA-0285 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0282 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0282, UIA-0283, UIA-0285 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0282 / P1 / 已确认 | **PICKER_ART_PUSHES_LIST** · `世界对象选择器候选列表`<br>区域中已有7个候选，但大幅地图插画、候选计数与上方留白占去首屏；默认只有第一条的上沿露出。<br>期望：满足标准 2,3,4,8,10,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/12-WorldObjectPickerForm-Panel_PageWorldObjectPicker-world.png) · [证据2](geometry/12-WorldObjectPickerForm-Panel_PageWorldObjectPicker-world.json) |
| UIA-0283 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `WorldObjectPickerForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/12-WorldObjectPickerForm-Panel_PageWorldObjectPicker-application.json) · [证据2](geometry/12-WorldObjectPickerForm-Panel_PageWorldObjectPicker-memory.json) · [证据3](geometry/12-WorldObjectPickerForm-Panel_PageWorldObjectPicker-scroll-top.json) · [证据4](geometry/12-WorldObjectPickerForm-Panel_PageWorldObjectPicker-state-WorldObjectPickerDisabledState.json) |
| UIA-0284 / P1 / 已确认 | **TEXT_OVERLAP** · `WorldObjectPickerForm/Panel_Frame/Grp_PageHost/Panel_PageWorldObjectPicker/Txt_WorldObjectPickerTitle ↔ WorldObjectPickerForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/12-WorldObjectPickerForm-Panel_PageWorldObjectPicker-application.json) · [证据2](geometry/12-WorldObjectPickerForm-Panel_PageWorldObjectPicker-memory.json) · [证据3](geometry/12-WorldObjectPickerForm-Panel_PageWorldObjectPicker-scroll-top.json) · [证据4](geometry/12-WorldObjectPickerForm-Panel_PageWorldObjectPicker-state-base.json) |
| UIA-0285 / P1 / 已确认 | **UNSOLICITED_SUCCESS** · `WorldObjectPickerForm/Panel_Frame/Grp_PageHost/Panel_PageWorldObjectPicker/Grp_WorldObjectPickerSuccessState/Panel_WorldObjectPickerSuccessMessage/Txt_WorldObjectPickerSuccessMessage`<br>仅打开目录/读取数据即显示“操作成功”，提示叠在列表或插画上，成功语义与当前动作不符<br>期望：满足标准 4,8,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/12-WorldObjectPickerForm-Panel_PageWorldObjectPicker-world.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_WorldObjectPickerLoadingState](state-layout/12-WorldObjectPickerForm-Panel_PageWorldObjectPicker-WorldObjectPickerLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_WorldObjectPickerEmptyState](state-layout/12-WorldObjectPickerForm-Panel_PageWorldObjectPicker-WorldObjectPickerEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_WorldObjectPickerErrorState](state-layout/12-WorldObjectPickerForm-Panel_PageWorldObjectPicker-WorldObjectPickerErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_WorldObjectPickerSuccessState](state-layout/12-WorldObjectPickerForm-Panel_PageWorldObjectPicker-WorldObjectPickerSuccessState.png) | 已检查 | world |
| [Grp_WorldObjectPickerDisabledState](state-layout/12-WorldObjectPickerForm-Panel_PageWorldObjectPicker-WorldObjectPickerDisabledState.png) | 已检查 | application、memory |

效果图要求的代表态：[matching](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-12-WorldObjectPicker-matching.png)、[empty](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-12-WorldObjectPicker-empty.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 067 / 13 / 算法编辑模式｜编辑 · AlgorithmEditorForm/Panel_PageAlgorithmEditor

Prefab：[Assets/Game/Prefabs/UI/Operations/AlgorithmEditorForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/AlgorithmEditorForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/AlgorithmEditorForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/AlgorithmEditorForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-13-AlgorithmEditor-edit.png)；证据：[application](runtime/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-application.png) · [memory](runtime/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-memory.png) · [world](runtime/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-world.png) · [memory/diagnosis-mode-harness](runtime/algorithm-diagnosis.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 发现问题 | UIA-0290 |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0286, UIA-0287, UIA-0288, UIA-0289 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0286, UIA-0290, UIA-0291, UIA-0292, UIA-0293, UIA-0294, UIA-0295, UIA-0296 |
| 5. 九宫格与边角 | 发现问题 | UIA-0287, UIA-0288, UIA-0289, UIA-0291 |
| 6. 对齐间距留白 | 发现问题 | UIA-0286, UIA-0290, UIA-0291, UIA-0296 |
| 7. 文字与可读性 | 发现问题 | UIA-0290, UIA-0291, UIA-0292, UIA-0293, UIA-0294, UIA-0295, UIA-0296 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0286, UIA-0296 |
| 9. 素材与状态语义 | 发现问题 | UIA-0291 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0286 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0290, UIA-0291, UIA-0292, UIA-0296 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0286 / P1 / 已确认 | **ALGORITHM_TOOLBAR_DENSITY** · `算法工作台顶部工具条与底部动作区`<br>撤销/重做/删除等控件挤在很窄的顶栏；底部七个动作分两排且贴近验证区和框底，主操作与编辑工具没有清楚分层。<br>期望：满足标准 3,4,6,8,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor.png) |
| UIA-0287 / P1 / 已确认 | **BORDER_COMPRESSION** · `AlgorithmEditorForm/Panel_Frame/Grp_PageHost/Panel_PageAlgorithmEditor/Panel_AlgorithmEditorToolbar`<br>控件 909.95×44 小于九宫格固定边总和 64.0×48.0，边角会被压缩<br>期望：满足标准 3,5：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-application.json) · [证据2](geometry/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-memory.json) · [证据3](geometry/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-scroll-bottom.json) · [证据4](geometry/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-scroll-top.json) |
| UIA-0288 / P1 / 已确认 | **BORDER_COMPRESSION** · `AlgorithmEditorForm/Panel_Frame/Grp_PageHost/Panel_PageAlgorithmEditor/Panel_AlgorithmEditorToolbar/Grp_AlgorithmDraftTools/Btn_AlgorithmRedo`<br>控件 99.99×32 小于九宫格固定边总和 112.0×16.0，边角会被压缩<br>期望：满足标准 3,5：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-application.json) · [证据2](geometry/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-memory.json) · [证据3](geometry/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-scroll-bottom.json) · [证据4](geometry/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-scroll-top.json) |
| UIA-0289 / P1 / 已确认 | **BORDER_COMPRESSION** · `AlgorithmEditorForm/Panel_Frame/Grp_PageHost/Panel_PageAlgorithmEditor/Panel_AlgorithmEditorToolbar/Grp_AlgorithmDraftTools/Btn_AlgorithmUndo`<br>控件 99.99×32 小于九宫格固定边总和 112.0×16.0，边角会被压缩<br>期望：满足标准 3,5：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-application.json) · [证据2](geometry/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-memory.json) · [证据3](geometry/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-scroll-bottom.json) · [证据4](geometry/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-scroll-top.json) |
| UIA-0290 / P1 / 已确认 | **NODE_TITLE_PORT_OVERLAP** · `AlgorithmNodeItem / Txt_AlgorithmNodeName`<br>以原生端口渲染器生成4入4出、长名称节点时，标题位于卡片中部并与端口文字相交；未使用母版顶部标题带，节点身份与端口无法同时阅读。<br>期望：满足标准 1,4,6,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 预览副本调用 OnInit、Bind、AlgorithmGraphPortView.Resize 等价尺寸和 Render；4入4出及长名称，未修改正式 Prefab。<br>[证据1](state-layout/AlgorithmNodeItem-long.png) · [证据2](geometry/AlgorithmNodeItem-long.json) · [证据3](sheets/items.jpg) |
| UIA-0291 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `AlgorithmEditorForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-application.json) · [证据2](geometry/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-memory.json) · [证据3](geometry/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-scroll-bottom.json) · [证据4](geometry/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-scroll-top.json) |
| UIA-0292 / P1 / 已确认 | **TEXT_MASK_CLIP** · `AlgorithmEditorForm/Panel_Frame/Grp_PageHost/Panel_PageAlgorithmEditor/Panel_AlgorithmEditorNodes/List_AlgorithmEditorNodes/Viewport_AlgorithmEditorNodes/Content_AlgorithmEditorNodes/Txt_AlgorithmEditorNodesBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-application.json) · [证据2](geometry/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-memory.json) · [证据3](geometry/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-scroll-bottom.json) · [证据4](geometry/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-scroll-top.json) |
| UIA-0293 / P1 / 已确认 | **TEXT_OVERLAP** · `AlgorithmEditorForm/Panel_Frame/Grp_PageHost/Panel_PageAlgorithmEditor/Txt_AlgorithmEditorTitle ↔ AlgorithmEditorForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-application.json) · [证据2](geometry/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-memory.json) · [证据3](geometry/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-scroll-bottom.json) · [证据4](geometry/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-scroll-top.json) |
| UIA-0294 / P1 / 已确认 | **TEXT_OVERLAP** · `AlgorithmNodeItem/Grp_AlgorithmNode/Btn_AlgorithmNodeSelect/Txt_AlgorithmNodeName ↔ AlgorithmNodeItem/Grp_AlgorithmNode/List_AlgorithmInputPorts/Viewport_AlgorithmInputPorts/Content_AlgorithmInputPorts/Item_AlgorithmInputPortTemplate(累计生产数量与目标材料要求1)/Btn_AlgorithmInputPort/Txt_AlgorithmInputPort`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/AlgorithmNodeItem-long.json) |
| UIA-0295 / P1 / 已确认 | **TEXT_OVERLAP** · `AlgorithmNodeItem/Grp_AlgorithmNode/Btn_AlgorithmNodeSelect/Txt_AlgorithmNodeName ↔ AlgorithmNodeItem/Grp_AlgorithmNode/List_AlgorithmOutputPorts/Viewport_AlgorithmOutputPorts/Content_AlgorithmOutputPorts/Item_AlgorithmOutputPortTemplate(累计生产数量与目标材料要求1)/Btn_AlgorithmOutputPort/Txt_AlgorithmOutputPort`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/AlgorithmNodeItem-long.json) |
| UIA-0296 / P2 / 已确认 | **UNAVAILABLE_REASON_REPEATED** · `AlgorithmEditorForm/Panel_Frame/Grp_PageHost/Panel_PageAlgorithmEditor/Grp_AlgorithmEditorDisabledState/Panel_AlgorithmEditorDisabledMessage/Txt_AlgorithmEditorDisabledMessage`<br>相同不可用原因同时出现在状态条和多个信息卡中；浮动状态条压在插画/内容区域，重复文字分散焦点。<br>期望：满足标准 4,6,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-world.json) · [证据2](runtime/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-world.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_AlgorithmEditorLoadingState](state-layout/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-AlgorithmEditorLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_AlgorithmEditorEmptyState](state-layout/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-AlgorithmEditorEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_AlgorithmEditorErrorState](state-layout/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-AlgorithmEditorErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_AlgorithmEditorSuccessState](state-layout/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-AlgorithmEditorSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_AlgorithmEditorDisabledState](state-layout/13-AlgorithmEditorForm-Panel_PageAlgorithmEditor-AlgorithmEditorDisabledState.png) | 已检查 | application、memory、world、memory/diagnosis-mode-harness |

效果图要求的代表态：[edit](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-13-AlgorithmEditor-edit.png)、[selected](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-13-AlgorithmEditor-selected.png)、[error](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-13-AlgorithmEditor-error.png)、[selected](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-13.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 068 / 13 / 算法公开参数 · AlgorithmPublicParametersForm/Panel_PagePublicParameters

Prefab：[Assets/Game/Prefabs/UI/Operations/AlgorithmPublicParametersForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/AlgorithmPublicParametersForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/AlgorithmPublicParametersForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/AlgorithmPublicParametersForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-13-PublicParameters-normal.png)；证据：[application](runtime/13-AlgorithmPublicParametersForm-Panel_PagePublicParameters-application.png) · [memory](runtime/13-AlgorithmPublicParametersForm-Panel_PagePublicParameters-memory.png) · [world](runtime/13-AlgorithmPublicParametersForm-Panel_PagePublicParameters-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 发现问题 | UIA-0297 |
| 2. 图片实际可见位置 | 发现问题 | UIA-0298 |
| 3. 尺寸与比例 | 发现问题 | UIA-0298, UIA-0299 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0298, UIA-0300, UIA-0301 |
| 5. 九宫格与边角 | 发现问题 | UIA-0299, UIA-0300 |
| 6. 对齐间距留白 | 发现问题 | UIA-0299, UIA-0300 |
| 7. 文字与可读性 | 发现问题 | UIA-0300, UIA-0301 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 发现问题 | UIA-0297, UIA-0299, UIA-0300 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0297, UIA-0299 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0297, UIA-0300 |
| 12. 同类页面一致性 | 发现问题 | UIA-0298 |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0297 / P0 / 已确认 | **PAGE_NOT_VISIBLE** · `Panel_PagePublicParameters`<br>请求页面未出现在当前可见图形树；实际页=<br>期望：满足标准 1,9,10,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/13-AlgorithmPublicParametersForm-Panel_PagePublicParameters-application.json) · [证据2](geometry/13-AlgorithmPublicParametersForm-Panel_PagePublicParameters-memory.json) · [证据3](geometry/13-AlgorithmPublicParametersForm-Panel_PagePublicParameters-world.json) |
| UIA-0298 / P1 / 已确认 | **OVERSIZED_EXTRA_FRAME** · `页面外层装饰框`<br>除页面主框外还有放大到近乎全屏的第二层装饰框，贴近画布四边并出现巨大斜角，破坏与同类页面的一致性。<br>期望：满足标准 2,3,4,12：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/13-AlgorithmPublicParametersForm-Panel_PagePublicParameters.png) |
| UIA-0299 / P1 / 已确认 | **PARAMETER_CONTROLS_STYLE** · `公开参数数值输入与布尔开关`<br>数值输入框和布尔区域形成粗重嵌套边框，布尔框只呈现黑色小块，标签与控件没有形成清楚的一行布局。<br>期望：满足标准 3,5,6,9,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/13-AlgorithmPublicParametersForm-Panel_PagePublicParameters.png) |
| UIA-0300 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `AlgorithmPublicParametersForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/13-AlgorithmPublicParametersForm-Panel_PagePublicParameters-state-PublicParametersDisabledState.json) · [证据2](geometry/13-AlgorithmPublicParametersForm-Panel_PagePublicParameters-state-PublicParametersEmptyState.json) · [证据3](geometry/13-AlgorithmPublicParametersForm-Panel_PagePublicParameters-state-PublicParametersErrorState.json) · [证据4](geometry/13-AlgorithmPublicParametersForm-Panel_PagePublicParameters-state-PublicParametersLoadingState.json) |
| UIA-0301 / P1 / 已确认 | **TEXT_OVERLAP** · `AlgorithmPublicParametersForm/Panel_Frame/Grp_PageHost/Panel_PagePublicParameters/Txt_PublicParametersTitle ↔ AlgorithmPublicParametersForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/13-AlgorithmPublicParametersForm-Panel_PagePublicParameters-state-base.json) · [证据2](geometry/13-AlgorithmPublicParametersForm-Panel_PagePublicParameters-state-PublicParametersDisabledState.json) · [证据3](geometry/13-AlgorithmPublicParametersForm-Panel_PagePublicParameters-state-PublicParametersEmptyState.json) · [证据4](geometry/13-AlgorithmPublicParametersForm-Panel_PagePublicParameters-state-PublicParametersErrorState.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_PublicParametersLoadingState](state-layout/13-AlgorithmPublicParametersForm-Panel_PagePublicParameters-PublicParametersLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_PublicParametersEmptyState](state-layout/13-AlgorithmPublicParametersForm-Panel_PagePublicParameters-PublicParametersEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_PublicParametersErrorState](state-layout/13-AlgorithmPublicParametersForm-Panel_PagePublicParameters-PublicParametersErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_PublicParametersSuccessState](state-layout/13-AlgorithmPublicParametersForm-Panel_PagePublicParameters-PublicParametersSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_PublicParametersDisabledState](state-layout/13-AlgorithmPublicParametersForm-Panel_PagePublicParameters-PublicParametersDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-13-PublicParameters-normal.png)、[pending](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-13-PublicParameters-pending.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 069 / 14 / 建筑放置 · WorldPlacementForm/Panel_PageBuildPlacement

Prefab：[Assets/Game/Prefabs/UI/Operations/WorldPlacementForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/WorldPlacementForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/WorldPlacementForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/WorldPlacementForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-14-BuildPlacement-valid.png)；证据：[application](runtime/14-WorldPlacementForm-Panel_PageBuildPlacement-application.png) · [memory](runtime/14-WorldPlacementForm-Panel_PageBuildPlacement-memory.png) · [world](runtime/14-WorldPlacementForm-Panel_PageBuildPlacement-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 发现问题 | UIA-0304 |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0304, UIA-0305, UIA-0306, UIA-0307, UIA-0308, UIA-0309, UIA-0310, UIA-0311, UIA-0312 |
| 5. 九宫格与边角 | 发现问题 | UIA-0308 |
| 6. 对齐间距留白 | 发现问题 | UIA-0304, UIA-0305, UIA-0306, UIA-0307, UIA-0308, UIA-0312 |
| 7. 文字与可读性 | 发现问题 | UIA-0305, UIA-0306, UIA-0307, UIA-0308, UIA-0309, UIA-0310, UIA-0311, UIA-0312 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0312 |
| 9. 素材与状态语义 | 发现问题 | UIA-0302, UIA-0303, UIA-0308 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0303, UIA-0304 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0302, UIA-0303, UIA-0308, UIA-0309, UIA-0310, UIA-0312 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0302 / P1 / 业务缺口 | **BUSINESS_SOURCE_NOT_CONNECTED** · `运行时数据正文`<br>本次实际运行说明：建造放置事务尚未接入运行路径：图纸→建筑的落位流程还没有调用者，因此选择目标、校验与确认暂不可用。<br>期望：满足标准 9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/14-WorldPlacementForm-Panel_PageBuildPlacement-world.png) · [证据2](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-world.json) |
| UIA-0303 / P1 / 业务缺口 | **PLACEMENT_BUSINESS_PATH_MISSING** · `WorldPlacementForm 建造/世界绑定`<br>源实现明确说明：建造放置事务或世界状态绑定尚没有调用者；目前只能检查声明布局及不可用呈现，无法执行其预览、校验与确认状态。<br>期望：满足标准 9,10,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/14-WorldPlacementForm-Panel_PageBuildPlacement-world.png) · [证据2](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-world.json) |
| UIA-0304 / P1 / 已确认 | **PLACEMENT_HINT_OVER_ACTION** · `底部操作提示卡 / 操作条`<br>操作提示卡直接覆盖在操作条中部，遮住旋转/选中等按钮区域；提示与交互元素没有分层留白。<br>期望：满足标准 2,4,6,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/14-WorldPlacementForm-Panel_PageBuildPlacement.png) |
| UIA-0305 / P1 / 已确认 | **SAFE_MARGIN_TEXT** · `WorldPlacementForm/Grp_PageHost/Panel_PageBuildPlacement/Panel_BuildPlacementHints/List_BuildPlacementHints/Viewport_BuildPlacementHints/Content_BuildPlacementHints/Txt_BuildPlacementHintsBody`<br>文字实际轮廓进入 24px 外边距<br>期望：满足标准 4,6,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-world-scroll-top.json) · [证据2](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-world.json) |
| UIA-0306 / P1 / 已确认 | **SAFE_MARGIN_TEXT** · `WorldPlacementForm/Grp_PageHost/Panel_PageBuildPlacement/Panel_BuildPlacementSelection/List_BuildPlacementSelection/Viewport_BuildPlacementSelection/Content_BuildPlacementSelection/Txt_BuildPlacementSelectionBody`<br>文字实际轮廓进入 24px 外边距<br>期望：满足标准 4,6,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-application.json) · [证据2](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-memory.json) · [证据3](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-scroll-bottom.json) · [证据4](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-scroll-top.json) |
| UIA-0307 / P1 / 已确认 | **SAFE_MARGIN_TEXT** · `WorldPlacementForm/Grp_PageHost/Panel_PageBuildPlacement/Panel_BuildPlacementValidity/List_BuildPlacementValidity/Viewport_BuildPlacementValidity/Content_BuildPlacementValidity/Txt_BuildPlacementValidityBody`<br>文字实际轮廓进入 24px 外边距<br>期望：满足标准 4,6,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-application.json) · [证据2](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-memory.json) · [证据3](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-scroll-bottom.json) · [证据4](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-scroll-top.json) |
| UIA-0308 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `WorldPlacementForm/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-application.json) · [证据2](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-memory.json) · [证据3](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-scroll-bottom.json) · [证据4](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-scroll-top.json) |
| UIA-0309 / P1 / 已确认 | **TEXT_MASK_CLIP** · `WorldPlacementForm/Grp_PageHost/Panel_PageBuildPlacement/Panel_BuildPlacementSelection/List_BuildPlacementSelection/Viewport_BuildPlacementSelection/Content_BuildPlacementSelection/Txt_BuildPlacementSelectionBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-application.json) · [证据2](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-memory.json) · [证据3](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-scroll-bottom.json) · [证据4](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-scroll-top.json) |
| UIA-0310 / P1 / 已确认 | **TEXT_MASK_CLIP** · `WorldPlacementForm/Grp_PageHost/Panel_PageBuildPlacement/Panel_BuildPlacementValidity/List_BuildPlacementValidity/Viewport_BuildPlacementValidity/Content_BuildPlacementValidity/Txt_BuildPlacementValidityBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-application.json) · [证据2](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-memory.json) · [证据3](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-scroll-bottom.json) · [证据4](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-scroll-top.json) |
| UIA-0311 / P1 / 已确认 | **TEXT_OVERLAP** · `WorldPlacementForm/Grp_PageHost/Panel_PageBuildPlacement/Panel_BuildPlacementSelection/Txt_BuildPlacementSelectionHeading ↔ WorldPlacementForm/Grp_PageHost/Panel_PageBuildPlacement/Txt_BuildPlacementTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-application.json) · [证据2](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-memory.json) · [证据3](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-scroll-bottom.json) · [证据4](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-scroll-top.json) |
| UIA-0312 / P2 / 已确认 | **UNAVAILABLE_REASON_REPEATED** · `WorldPlacementForm/Grp_PageHost/Panel_PageBuildPlacement/Grp_BuildPlacementDisabledState/Panel_BuildPlacementDisabledMessage/Txt_BuildPlacementDisabledMessage`<br>相同不可用原因同时出现在状态条和多个信息卡中；浮动状态条压在插画/内容区域，重复文字分散焦点。<br>期望：满足标准 4,6,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/14-WorldPlacementForm-Panel_PageBuildPlacement-world.json) · [证据2](runtime/14-WorldPlacementForm-Panel_PageBuildPlacement-world.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_BuildPlacementLoadingState](state-layout/14-WorldPlacementForm-Panel_PageBuildPlacement-BuildPlacementLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_BuildPlacementEmptyState](state-layout/14-WorldPlacementForm-Panel_PageBuildPlacement-BuildPlacementEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_BuildPlacementErrorState](state-layout/14-WorldPlacementForm-Panel_PageBuildPlacement-BuildPlacementErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_BuildPlacementSuccessState](state-layout/14-WorldPlacementForm-Panel_PageBuildPlacement-BuildPlacementSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_BuildPlacementDisabledState](state-layout/14-WorldPlacementForm-Panel_PageBuildPlacement-BuildPlacementDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[valid](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-14-BuildPlacement-valid.png)、[invalid](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-14-BuildPlacement-invalid.png)、[rotating](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-14-BuildPlacement-rotating.png)、[confirmed](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-14-BuildPlacement-confirmed.png)、[valid](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-14.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 070 / 14 / 机器部署 · WorldPlacementForm/Panel_PageMachineDeployment

Prefab：[Assets/Game/Prefabs/UI/Operations/WorldPlacementForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/WorldPlacementForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/WorldPlacementForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/WorldPlacementForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-14-MachineDeployment-valid.png)；证据：[application](runtime/14-WorldPlacementForm-Panel_PageMachineDeployment-application.png) · [memory](runtime/14-WorldPlacementForm-Panel_PageMachineDeployment-memory.png) · [world](runtime/14-WorldPlacementForm-Panel_PageMachineDeployment-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 发现问题 | UIA-0313 |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0313, UIA-0314, UIA-0315, UIA-0316, UIA-0317, UIA-0318, UIA-0319 |
| 5. 九宫格与边角 | 发现问题 | UIA-0316 |
| 6. 对齐间距留白 | 发现问题 | UIA-0313, UIA-0314, UIA-0315, UIA-0316 |
| 7. 文字与可读性 | 发现问题 | UIA-0314, UIA-0315, UIA-0316, UIA-0317, UIA-0318, UIA-0319 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 发现问题 | UIA-0316 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0313 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0316, UIA-0317, UIA-0318 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0313 / P1 / 已确认 | **PLACEMENT_HINT_OVER_ACTION** · `底部操作提示卡 / 操作条`<br>操作提示卡直接覆盖在操作条中部，遮住旋转/选中等按钮区域；提示与交互元素没有分层留白。<br>期望：满足标准 2,4,6,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/14-WorldPlacementForm-Panel_PageMachineDeployment.png) |
| UIA-0314 / P1 / 已确认 | **SAFE_MARGIN_TEXT** · `WorldPlacementForm/Grp_PageHost/Panel_PageMachineDeployment/Panel_MachineDeploymentSelection/List_MachineDeploymentSelection/Viewport_MachineDeploymentSelection/Content_MachineDeploymentSelection/Txt_MachineDeploymentSelectionBody`<br>文字实际轮廓进入 24px 外边距<br>期望：满足标准 4,6,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/14-WorldPlacementForm-Panel_PageMachineDeployment-application.json) · [证据2](geometry/14-WorldPlacementForm-Panel_PageMachineDeployment-memory.json) · [证据3](geometry/14-WorldPlacementForm-Panel_PageMachineDeployment-scroll-bottom.json) · [证据4](geometry/14-WorldPlacementForm-Panel_PageMachineDeployment-scroll-top.json) |
| UIA-0315 / P1 / 已确认 | **SAFE_MARGIN_TEXT** · `WorldPlacementForm/Grp_PageHost/Panel_PageMachineDeployment/Panel_MachineDeploymentValidity/List_MachineDeploymentValidity/Viewport_MachineDeploymentValidity/Content_MachineDeploymentValidity/Txt_MachineDeploymentValidityBody`<br>文字实际轮廓进入 24px 外边距<br>期望：满足标准 4,6,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/14-WorldPlacementForm-Panel_PageMachineDeployment-application.json) · [证据2](geometry/14-WorldPlacementForm-Panel_PageMachineDeployment-memory.json) · [证据3](geometry/14-WorldPlacementForm-Panel_PageMachineDeployment-scroll-bottom.json) · [证据4](geometry/14-WorldPlacementForm-Panel_PageMachineDeployment-scroll-top.json) |
| UIA-0316 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `WorldPlacementForm/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/14-WorldPlacementForm-Panel_PageMachineDeployment-application.json) · [证据2](geometry/14-WorldPlacementForm-Panel_PageMachineDeployment-memory.json) · [证据3](geometry/14-WorldPlacementForm-Panel_PageMachineDeployment-scroll-bottom.json) · [证据4](geometry/14-WorldPlacementForm-Panel_PageMachineDeployment-scroll-top.json) |
| UIA-0317 / P1 / 已确认 | **TEXT_MASK_CLIP** · `WorldPlacementForm/Grp_PageHost/Panel_PageMachineDeployment/Panel_MachineDeploymentSelection/List_MachineDeploymentSelection/Viewport_MachineDeploymentSelection/Content_MachineDeploymentSelection/Txt_MachineDeploymentSelectionBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/14-WorldPlacementForm-Panel_PageMachineDeployment-application.json) · [证据2](geometry/14-WorldPlacementForm-Panel_PageMachineDeployment-memory.json) · [证据3](geometry/14-WorldPlacementForm-Panel_PageMachineDeployment-scroll-bottom.json) · [证据4](geometry/14-WorldPlacementForm-Panel_PageMachineDeployment-scroll-top.json) |
| UIA-0318 / P1 / 已确认 | **TEXT_MASK_CLIP** · `WorldPlacementForm/Grp_PageHost/Panel_PageMachineDeployment/Panel_MachineDeploymentValidity/List_MachineDeploymentValidity/Viewport_MachineDeploymentValidity/Content_MachineDeploymentValidity/Txt_MachineDeploymentValidityBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/14-WorldPlacementForm-Panel_PageMachineDeployment-application.json) · [证据2](geometry/14-WorldPlacementForm-Panel_PageMachineDeployment-memory.json) · [证据3](geometry/14-WorldPlacementForm-Panel_PageMachineDeployment-scroll-bottom.json) · [证据4](geometry/14-WorldPlacementForm-Panel_PageMachineDeployment-scroll-top.json) |
| UIA-0319 / P1 / 已确认 | **TEXT_OVERLAP** · `WorldPlacementForm/Grp_PageHost/Panel_PageMachineDeployment/Panel_MachineDeploymentSelection/Txt_MachineDeploymentSelectionHeading ↔ WorldPlacementForm/Grp_PageHost/Panel_PageMachineDeployment/Txt_MachineDeploymentTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/14-WorldPlacementForm-Panel_PageMachineDeployment-application.json) · [证据2](geometry/14-WorldPlacementForm-Panel_PageMachineDeployment-memory.json) · [证据3](geometry/14-WorldPlacementForm-Panel_PageMachineDeployment-scroll-bottom.json) · [证据4](geometry/14-WorldPlacementForm-Panel_PageMachineDeployment-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_MachineDeploymentLoadingState](state-layout/14-WorldPlacementForm-Panel_PageMachineDeployment-MachineDeploymentLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MachineDeploymentEmptyState](state-layout/14-WorldPlacementForm-Panel_PageMachineDeployment-MachineDeploymentEmptyState.png) | 已检查 | world |
| [Grp_MachineDeploymentErrorState](state-layout/14-WorldPlacementForm-Panel_PageMachineDeployment-MachineDeploymentErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MachineDeploymentSuccessState](state-layout/14-WorldPlacementForm-Panel_PageMachineDeployment-MachineDeploymentSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MachineDeploymentDisabledState](state-layout/14-WorldPlacementForm-Panel_PageMachineDeployment-MachineDeploymentDisabledState.png) | 已检查 | application、memory |

效果图要求的代表态：[valid](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-14-MachineDeployment-valid.png)、[invalid](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-14-MachineDeployment-invalid.png)、[rotating](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-14-MachineDeployment-rotating.png)、[confirmed](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-14-MachineDeployment-confirmed.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 071 / 14 / 世界对象选择与定位辅助 · WorldPlacementForm/Panel_PageWorldBinding

Prefab：[Assets/Game/Prefabs/UI/Operations/WorldPlacementForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/WorldPlacementForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/WorldPlacementForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/WorldPlacementForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-14-WorldBinding-valid.png)；证据：[application](runtime/14-WorldPlacementForm-Panel_PageWorldBinding-application.png) · [memory](runtime/14-WorldPlacementForm-Panel_PageWorldBinding-memory.png) · [world](runtime/14-WorldPlacementForm-Panel_PageWorldBinding-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 发现问题 | UIA-0322 |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0322, UIA-0323, UIA-0324, UIA-0325, UIA-0326, UIA-0327, UIA-0328, UIA-0329, UIA-0330, UIA-0331, UIA-0332 |
| 5. 九宫格与边角 | 发现问题 | UIA-0325 |
| 6. 对齐间距留白 | 发现问题 | UIA-0322, UIA-0323, UIA-0324, UIA-0325, UIA-0332 |
| 7. 文字与可读性 | 发现问题 | UIA-0323, UIA-0324, UIA-0325, UIA-0326, UIA-0327, UIA-0328, UIA-0329, UIA-0330, UIA-0331, UIA-0332 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0332 |
| 9. 素材与状态语义 | 发现问题 | UIA-0320, UIA-0321, UIA-0325 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0321, UIA-0322, UIA-0333 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0320, UIA-0321, UIA-0325, UIA-0326, UIA-0327, UIA-0328, UIA-0332 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0320 / P1 / 业务缺口 | **BUSINESS_SOURCE_NOT_CONNECTED** · `运行时数据正文`<br>本次实际运行说明：世界状态绑定尚未接入运行路径：绑定候选与绑定事务都还没有调用者，因此本页暂不可用。<br>期望：满足标准 9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/14-WorldPlacementForm-Panel_PageWorldBinding-world.png) · [证据2](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-world.json) |
| UIA-0321 / P1 / 业务缺口 | **PLACEMENT_BUSINESS_PATH_MISSING** · `WorldPlacementForm 建造/世界绑定`<br>源实现明确说明：建造放置事务或世界状态绑定尚没有调用者；目前只能检查声明布局及不可用呈现，无法执行其预览、校验与确认状态。<br>期望：满足标准 9,10,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/14-WorldPlacementForm-Panel_PageWorldBinding-world.png) · [证据2](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-world.json) |
| UIA-0322 / P1 / 已确认 | **PLACEMENT_HINT_OVER_ACTION** · `底部操作提示卡 / 操作条`<br>操作提示卡直接覆盖在操作条中部，遮住旋转/选中等按钮区域；提示与交互元素没有分层留白。<br>期望：满足标准 2,4,6,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/14-WorldPlacementForm-Panel_PageWorldBinding.png) |
| UIA-0323 / P1 / 已确认 | **SAFE_MARGIN_TEXT** · `WorldPlacementForm/Grp_PageHost/Panel_PageWorldBinding/Panel_WorldBindingCandidate/List_WorldBindingCandidate/Viewport_WorldBindingCandidate/Content_WorldBindingCandidate/Txt_WorldBindingCandidateBody`<br>文字实际轮廓进入 24px 外边距<br>期望：满足标准 4,6,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-application.json) · [证据2](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-memory.json) · [证据3](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-scroll-bottom.json) · [证据4](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-scroll-top.json) |
| UIA-0324 / P1 / 已确认 | **SAFE_MARGIN_TEXT** · `WorldPlacementForm/Grp_PageHost/Panel_PageWorldBinding/Panel_WorldBindingPurpose/List_WorldBindingPurpose/Viewport_WorldBindingPurpose/Content_WorldBindingPurpose/Txt_WorldBindingPurposeBody`<br>文字实际轮廓进入 24px 外边距<br>期望：满足标准 4,6,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-application.json) · [证据2](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-memory.json) · [证据3](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-scroll-bottom.json) · [证据4](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-scroll-top.json) |
| UIA-0325 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `WorldPlacementForm/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-application.json) · [证据2](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-memory.json) · [证据3](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-scroll-bottom.json) · [证据4](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-scroll-top.json) |
| UIA-0326 / P1 / 已确认 | **TEXT_MASK_CLIP** · `WorldPlacementForm/Grp_PageHost/Panel_PageWorldBinding/Panel_WorldBindingCandidate/List_WorldBindingCandidate/Viewport_WorldBindingCandidate/Content_WorldBindingCandidate/Txt_WorldBindingCandidateBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-application.json) · [证据2](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-memory.json) · [证据3](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-scroll-bottom.json) · [证据4](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-scroll-top.json) |
| UIA-0327 / P1 / 已确认 | **TEXT_MASK_CLIP** · `WorldPlacementForm/Grp_PageHost/Panel_PageWorldBinding/Panel_WorldBindingPurpose/List_WorldBindingPurpose/Viewport_WorldBindingPurpose/Content_WorldBindingPurpose/Txt_WorldBindingPurposeBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-application.json) · [证据2](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-memory.json) · [证据3](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-scroll-bottom.json) · [证据4](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-scroll-top.json) |
| UIA-0328 / P1 / 已确认 | **TEXT_OVERFLOW** · `WorldPlacementForm/Grp_PageHost/Panel_PageWorldBinding/Txt_WorldBindingTitle`<br>文字超出容器，字号 28；容器 300×36<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-application.json) · [证据2](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-memory.json) · [证据3](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-scroll-bottom.json) · [证据4](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-scroll-top.json) |
| UIA-0329 / P1 / 已确认 | **TEXT_OVERLAP** · `WorldPlacementForm/Grp_PageHost/Panel_PageWorldBinding/Grp_WorldBindingActions/Btn_WorldBindingConfirm/Txt_WorldBindingConfirmLabel ↔ WorldPlacementForm/Grp_PageHost/Panel_PageWorldBinding/Panel_WorldBindingHints/List_WorldBindingHints/Viewport_WorldBindingHints/Content_WorldBindingHints/Txt_WorldBindingHintsBody`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-application.json) · [证据2](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-memory.json) · [证据3](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-scroll-top.json) · [证据4](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-world-scroll-top.json) |
| UIA-0330 / P1 / 已确认 | **TEXT_OVERLAP** · `WorldPlacementForm/Grp_PageHost/Panel_PageWorldBinding/Panel_WorldBindingPurpose/List_WorldBindingPurpose/Viewport_WorldBindingPurpose/Content_WorldBindingPurpose/Txt_WorldBindingPurposeBody ↔ WorldPlacementForm/Grp_PageHost/Panel_PageWorldBinding/Txt_WorldBindingTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-application.json) · [证据2](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-memory.json) · [证据3](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-scroll-bottom.json) · [证据4](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-scroll-top.json) |
| UIA-0331 / P1 / 已确认 | **TEXT_OVERLAP** · `WorldPlacementForm/Grp_PageHost/Panel_PageWorldBinding/Panel_WorldBindingPurpose/Txt_WorldBindingPurposeHeading ↔ WorldPlacementForm/Grp_PageHost/Panel_PageWorldBinding/Txt_WorldBindingTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-application.json) · [证据2](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-memory.json) · [证据3](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-scroll-bottom.json) · [证据4](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-scroll-top.json) |
| UIA-0332 / P2 / 已确认 | **UNAVAILABLE_REASON_REPEATED** · `WorldPlacementForm/Grp_PageHost/Panel_PageWorldBinding/Grp_WorldBindingDisabledState/Panel_WorldBindingDisabledMessage/Txt_WorldBindingDisabledMessage`<br>相同不可用原因同时出现在状态条和多个信息卡中；浮动状态条压在插画/内容区域，重复文字分散焦点。<br>期望：满足标准 4,6,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-world.json) · [证据2](runtime/14-WorldPlacementForm-Panel_PageWorldBinding-world.png) |
| UIA-0333 / P1 / 待复核 | **POINTER_CENTER_BLOCKED** · `WorldPlacementForm/Grp_PageHost/Panel_PageWorldBinding/Grp_WorldBindingActions/Btn_WorldBindingCancel`<br>控件中心射线未命中自身；顶层=GameFramework/Builtin/UI/UICanvasRoot/UI Group - Default/FieldHudResidentForm(Clone)/Grp_PageHost/Panel_PageHudNavigation/Grp_HudNavigationActions/Btn_HudNavigationSystem<br>期望：满足标准 10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-world-scroll-bottom.json) · [证据2](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-world-scroll-top.json) · [证据3](geometry/14-WorldPlacementForm-Panel_PageWorldBinding-world.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_WorldBindingLoadingState](state-layout/14-WorldPlacementForm-Panel_PageWorldBinding-WorldBindingLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_WorldBindingEmptyState](state-layout/14-WorldPlacementForm-Panel_PageWorldBinding-WorldBindingEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_WorldBindingErrorState](state-layout/14-WorldPlacementForm-Panel_PageWorldBinding-WorldBindingErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_WorldBindingSuccessState](state-layout/14-WorldPlacementForm-Panel_PageWorldBinding-WorldBindingSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_WorldBindingDisabledState](state-layout/14-WorldPlacementForm-Panel_PageWorldBinding-WorldBindingDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[valid](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-14-WorldBinding-valid.png)、[invalid](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-14-WorldBinding-invalid.png)、[rotating](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-14-WorldBinding-rotating.png)、[confirmed](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-14-WorldBinding-confirmed.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 072 / 15 / 警报列表与详情 · AlertForm/Panel_PageAlerts

Prefab：[Assets/Game/Prefabs/UI/Operations/AlertForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/AlertForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/AlertForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/AlertForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-15-Alerts-normal.png)；证据：[application](runtime/15-AlertForm-Panel_PageAlerts-application.png) · [memory](runtime/15-AlertForm-Panel_PageAlerts-memory.png) · [world](runtime/15-AlertForm-Panel_PageAlerts-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0334, UIA-0335 |
| 5. 九宫格与边角 | 发现问题 | UIA-0334 |
| 6. 对齐间距留白 | 发现问题 | UIA-0334 |
| 7. 文字与可读性 | 发现问题 | UIA-0334, UIA-0335 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 发现问题 | UIA-0334 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0334 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0334 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `AlertForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/15-AlertForm-Panel_PageAlerts-application.json) · [证据2](geometry/15-AlertForm-Panel_PageAlerts-memory.json) · [证据3](geometry/15-AlertForm-Panel_PageAlerts-scroll-top.json) · [证据4](geometry/15-AlertForm-Panel_PageAlerts-state-AlertsDisabledState.json) |
| UIA-0335 / P1 / 已确认 | **TEXT_OVERLAP** · `AlertForm/Panel_Frame/Grp_PageHost/Panel_PageAlerts/Txt_AlertsTitle ↔ AlertForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/15-AlertForm-Panel_PageAlerts-application.json) · [证据2](geometry/15-AlertForm-Panel_PageAlerts-memory.json) · [证据3](geometry/15-AlertForm-Panel_PageAlerts-scroll-top.json) · [证据4](geometry/15-AlertForm-Panel_PageAlerts-state-AlertsDisabledState.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_AlertsLoadingState](state-layout/15-AlertForm-Panel_PageAlerts-AlertsLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_AlertsEmptyState](state-layout/15-AlertForm-Panel_PageAlerts-AlertsEmptyState.png) | 已检查 | world |
| [Grp_AlertsErrorState](state-layout/15-AlertForm-Panel_PageAlerts-AlertsErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_AlertsSuccessState](state-layout/15-AlertForm-Panel_PageAlerts-AlertsSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_AlertsDisabledState](state-layout/15-AlertForm-Panel_PageAlerts-AlertsDisabledState.png) | 已检查 | application、memory |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-15-Alerts-normal.png)、[critical](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-15-Alerts-critical.png)、[merged](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-15-Alerts-merged.png)、[read](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-15-Alerts-read.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 073 / 15 / 任务奖励与解锁详情 · QuestForm/Panel_PageQuestRewards

Prefab：[Assets/Game/Prefabs/UI/Operations/QuestForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/QuestForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/QuestForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/QuestForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-15-QuestRewards-complete.png)；证据：[application](runtime/15-QuestForm-Panel_PageQuestRewards-application.png) · [memory](runtime/15-QuestForm-Panel_PageQuestRewards-memory.png) · [world](runtime/15-QuestForm-Panel_PageQuestRewards-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0337, UIA-0338, UIA-0339 |
| 5. 九宫格与边角 | 发现问题 | UIA-0337 |
| 6. 对齐间距留白 | 发现问题 | UIA-0337, UIA-0339 |
| 7. 文字与可读性 | 发现问题 | UIA-0337, UIA-0338, UIA-0339 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0339 |
| 9. 素材与状态语义 | 发现问题 | UIA-0336, UIA-0337 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0336, UIA-0337, UIA-0339 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0336 / P1 / 业务缺口 | **BUSINESS_SOURCE_NOT_CONNECTED** · `运行时数据正文`<br>本次实际运行说明：任务域尚未接入运行路径：任务状态与领取结算都还没有创建者。<br>期望：满足标准 9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/15-QuestForm-Panel_PageQuestRewards-world.png) · [证据2](geometry/15-QuestForm-Panel_PageQuestRewards-world.json) |
| UIA-0337 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `QuestForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/15-QuestForm-Panel_PageQuestRewards-application.json) · [证据2](geometry/15-QuestForm-Panel_PageQuestRewards-memory.json) · [证据3](geometry/15-QuestForm-Panel_PageQuestRewards-scroll-top.json) · [证据4](geometry/15-QuestForm-Panel_PageQuestRewards-state-QuestRewardsDisabledState.json) |
| UIA-0338 / P1 / 已确认 | **TEXT_OVERLAP** · `QuestForm/Panel_Frame/Grp_PageHost/Panel_PageQuestRewards/Txt_QuestRewardsTitle ↔ QuestForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/15-QuestForm-Panel_PageQuestRewards-application.json) · [证据2](geometry/15-QuestForm-Panel_PageQuestRewards-memory.json) · [证据3](geometry/15-QuestForm-Panel_PageQuestRewards-scroll-top.json) · [证据4](geometry/15-QuestForm-Panel_PageQuestRewards-state-base.json) |
| UIA-0339 / P2 / 已确认 | **UNAVAILABLE_REASON_REPEATED** · `QuestForm/Panel_Frame/Grp_PageHost/Panel_PageQuestRewards/Grp_QuestRewardsDisabledState/Panel_QuestRewardsDisabledMessage/Txt_QuestRewardsDisabledMessage`<br>相同不可用原因同时出现在状态条和多个信息卡中；浮动状态条压在插画/内容区域，重复文字分散焦点。<br>期望：满足标准 4,6,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/15-QuestForm-Panel_PageQuestRewards-world.json) · [证据2](runtime/15-QuestForm-Panel_PageQuestRewards-world.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_QuestRewardsLoadingState](state-layout/15-QuestForm-Panel_PageQuestRewards-QuestRewardsLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_QuestRewardsEmptyState](state-layout/15-QuestForm-Panel_PageQuestRewards-QuestRewardsEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_QuestRewardsErrorState](state-layout/15-QuestForm-Panel_PageQuestRewards-QuestRewardsErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_QuestRewardsSuccessState](state-layout/15-QuestForm-Panel_PageQuestRewards-QuestRewardsSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_QuestRewardsDisabledState](state-layout/15-QuestForm-Panel_PageQuestRewards-QuestRewardsDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[complete](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-15-QuestRewards-complete.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 074 / 15 / 主线任务 · QuestForm/Panel_PageQuests

Prefab：[Assets/Game/Prefabs/UI/Operations/QuestForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/QuestForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/QuestForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/QuestForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-15-Quests-active.png)；证据：[application](runtime/15-QuestForm-Panel_PageQuests-application.png) · [memory](runtime/15-QuestForm-Panel_PageQuests-memory.png) · [world](runtime/15-QuestForm-Panel_PageQuests-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0341, UIA-0342, UIA-0343 |
| 5. 九宫格与边角 | 发现问题 | UIA-0341 |
| 6. 对齐间距留白 | 发现问题 | UIA-0341, UIA-0343 |
| 7. 文字与可读性 | 发现问题 | UIA-0341, UIA-0342, UIA-0343 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0343 |
| 9. 素材与状态语义 | 发现问题 | UIA-0340, UIA-0341 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0340, UIA-0341, UIA-0343 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0340 / P1 / 业务缺口 | **BUSINESS_SOURCE_NOT_CONNECTED** · `运行时数据正文`<br>本次实际运行说明：任务域尚未接入运行路径：任务状态与领取结算都还没有创建者。<br>期望：满足标准 9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/15-QuestForm-Panel_PageQuests-world.png) · [证据2](geometry/15-QuestForm-Panel_PageQuests-world.json) |
| UIA-0341 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `QuestForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/15-QuestForm-Panel_PageQuests-application.json) · [证据2](geometry/15-QuestForm-Panel_PageQuests-memory.json) · [证据3](geometry/15-QuestForm-Panel_PageQuests-scroll-top.json) · [证据4](geometry/15-QuestForm-Panel_PageQuests-state-QuestsDisabledState.json) |
| UIA-0342 / P1 / 已确认 | **TEXT_OVERLAP** · `QuestForm/Panel_Frame/Grp_PageHost/Panel_PageQuests/Txt_QuestsTitle ↔ QuestForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/15-QuestForm-Panel_PageQuests-application.json) · [证据2](geometry/15-QuestForm-Panel_PageQuests-memory.json) · [证据3](geometry/15-QuestForm-Panel_PageQuests-scroll-top.json) · [证据4](geometry/15-QuestForm-Panel_PageQuests-state-base.json) |
| UIA-0343 / P2 / 已确认 | **UNAVAILABLE_REASON_REPEATED** · `QuestForm/Panel_Frame/Grp_PageHost/Panel_PageQuests/Grp_QuestsDisabledState/Panel_QuestsDisabledMessage/Txt_QuestsDisabledMessage`<br>相同不可用原因同时出现在状态条和多个信息卡中；浮动状态条压在插画/内容区域，重复文字分散焦点。<br>期望：满足标准 4,6,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/15-QuestForm-Panel_PageQuests-world.json) · [证据2](runtime/15-QuestForm-Panel_PageQuests-world.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_QuestsLoadingState](state-layout/15-QuestForm-Panel_PageQuests-QuestsLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_QuestsEmptyState](state-layout/15-QuestForm-Panel_PageQuests-QuestsEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_QuestsErrorState](state-layout/15-QuestForm-Panel_PageQuests-QuestsErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_QuestsSuccessState](state-layout/15-QuestForm-Panel_PageQuests-QuestsSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_QuestsDisabledState](state-layout/15-QuestForm-Panel_PageQuests-QuestsDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[active](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-15-Quests-active.png)、[complete](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-15-Quests-complete.png)、[active](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-15.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 075 / 15 / 算法运行记录 · RecordReaderForm/Panel_PageAlgorithmHistory

Prefab：[Assets/Game/Prefabs/UI/Operations/RecordReaderForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/RecordReaderForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/RecordReaderForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/RecordReaderForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-15-AlgorithmHistory-normal.png)；证据：[application](runtime/15-RecordReaderForm-Panel_PageAlgorithmHistory-application.png) · [memory](runtime/15-RecordReaderForm-Panel_PageAlgorithmHistory-memory.png) · [world](runtime/15-RecordReaderForm-Panel_PageAlgorithmHistory-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0344, UIA-0345 |
| 5. 九宫格与边角 | 发现问题 | UIA-0344 |
| 6. 对齐间距留白 | 发现问题 | UIA-0344 |
| 7. 文字与可读性 | 发现问题 | UIA-0344, UIA-0345 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 发现问题 | UIA-0344 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0344 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0344 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `RecordReaderForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/15-RecordReaderForm-Panel_PageAlgorithmHistory-application.json) · [证据2](geometry/15-RecordReaderForm-Panel_PageAlgorithmHistory-memory.json) · [证据3](geometry/15-RecordReaderForm-Panel_PageAlgorithmHistory-scroll-top.json) · [证据4](geometry/15-RecordReaderForm-Panel_PageAlgorithmHistory-state-AlgorithmHistoryDisabledState.json) |
| UIA-0345 / P1 / 已确认 | **TEXT_OVERLAP** · `RecordReaderForm/Panel_Frame/Grp_PageHost/Panel_PageAlgorithmHistory/Txt_AlgorithmHistoryTitle ↔ RecordReaderForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/15-RecordReaderForm-Panel_PageAlgorithmHistory-application.json) · [证据2](geometry/15-RecordReaderForm-Panel_PageAlgorithmHistory-memory.json) · [证据3](geometry/15-RecordReaderForm-Panel_PageAlgorithmHistory-scroll-top.json) · [证据4](geometry/15-RecordReaderForm-Panel_PageAlgorithmHistory-state-AlgorithmHistoryDisabledState.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_AlgorithmHistoryLoadingState](state-layout/15-RecordReaderForm-Panel_PageAlgorithmHistory-AlgorithmHistoryLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_AlgorithmHistoryEmptyState](state-layout/15-RecordReaderForm-Panel_PageAlgorithmHistory-AlgorithmHistoryEmptyState.png) | 已检查 | memory、world |
| [Grp_AlgorithmHistoryErrorState](state-layout/15-RecordReaderForm-Panel_PageAlgorithmHistory-AlgorithmHistoryErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_AlgorithmHistorySuccessState](state-layout/15-RecordReaderForm-Panel_PageAlgorithmHistory-AlgorithmHistorySuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_AlgorithmHistoryDisabledState](state-layout/15-RecordReaderForm-Panel_PageAlgorithmHistory-AlgorithmHistoryDisabledState.png) | 已检查 | application |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-15-AlgorithmHistory-normal.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 076 / 15 / 能源停机记录 · RecordReaderForm/Panel_PageEnergyHistory

Prefab：[Assets/Game/Prefabs/UI/Operations/RecordReaderForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/RecordReaderForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/RecordReaderForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/RecordReaderForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-15-EnergyHistory-normal.png)；证据：[application](runtime/15-RecordReaderForm-Panel_PageEnergyHistory-application.png) · [memory](runtime/15-RecordReaderForm-Panel_PageEnergyHistory-memory.png) · [world](runtime/15-RecordReaderForm-Panel_PageEnergyHistory-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0346, UIA-0347 |
| 5. 九宫格与边角 | 发现问题 | UIA-0346 |
| 6. 对齐间距留白 | 发现问题 | UIA-0346 |
| 7. 文字与可读性 | 发现问题 | UIA-0346, UIA-0347 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 发现问题 | UIA-0346 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0346 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0346 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `RecordReaderForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/15-RecordReaderForm-Panel_PageEnergyHistory-application.json) · [证据2](geometry/15-RecordReaderForm-Panel_PageEnergyHistory-memory.json) · [证据3](geometry/15-RecordReaderForm-Panel_PageEnergyHistory-scroll-top.json) · [证据4](geometry/15-RecordReaderForm-Panel_PageEnergyHistory-state-EnergyHistoryDisabledState.json) |
| UIA-0347 / P1 / 已确认 | **TEXT_OVERLAP** · `RecordReaderForm/Panel_Frame/Grp_PageHost/Panel_PageEnergyHistory/Txt_EnergyHistoryTitle ↔ RecordReaderForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/15-RecordReaderForm-Panel_PageEnergyHistory-application.json) · [证据2](geometry/15-RecordReaderForm-Panel_PageEnergyHistory-memory.json) · [证据3](geometry/15-RecordReaderForm-Panel_PageEnergyHistory-scroll-top.json) · [证据4](geometry/15-RecordReaderForm-Panel_PageEnergyHistory-state-base.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_EnergyHistoryLoadingState](state-layout/15-RecordReaderForm-Panel_PageEnergyHistory-EnergyHistoryLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_EnergyHistoryEmptyState](state-layout/15-RecordReaderForm-Panel_PageEnergyHistory-EnergyHistoryEmptyState.png) | 已检查 | memory、world |
| [Grp_EnergyHistoryErrorState](state-layout/15-RecordReaderForm-Panel_PageEnergyHistory-EnergyHistoryErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_EnergyHistorySuccessState](state-layout/15-RecordReaderForm-Panel_PageEnergyHistory-EnergyHistorySuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_EnergyHistoryDisabledState](state-layout/15-RecordReaderForm-Panel_PageEnergyHistory-EnergyHistoryDisabledState.png) | 已检查 | application |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-15-EnergyHistory-normal.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 077 / 15 / 机器任务历史 · RecordReaderForm/Panel_PageMachineHistory

Prefab：[Assets/Game/Prefabs/UI/Operations/RecordReaderForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/RecordReaderForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/RecordReaderForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/RecordReaderForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-15-MachineHistory-normal.png)；证据：[application](runtime/15-RecordReaderForm-Panel_PageMachineHistory-application.png) · [memory](runtime/15-RecordReaderForm-Panel_PageMachineHistory-memory.png) · [world](runtime/15-RecordReaderForm-Panel_PageMachineHistory-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0348, UIA-0349 |
| 5. 九宫格与边角 | 发现问题 | UIA-0348 |
| 6. 对齐间距留白 | 发现问题 | UIA-0348 |
| 7. 文字与可读性 | 发现问题 | UIA-0348, UIA-0349 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 发现问题 | UIA-0348 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0348 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0348 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `RecordReaderForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/15-RecordReaderForm-Panel_PageMachineHistory-application.json) · [证据2](geometry/15-RecordReaderForm-Panel_PageMachineHistory-memory.json) · [证据3](geometry/15-RecordReaderForm-Panel_PageMachineHistory-scroll-top.json) · [证据4](geometry/15-RecordReaderForm-Panel_PageMachineHistory-state-MachineHistoryDisabledState.json) |
| UIA-0349 / P1 / 已确认 | **TEXT_OVERLAP** · `RecordReaderForm/Panel_Frame/Grp_PageHost/Panel_PageMachineHistory/Txt_MachineHistoryTitle ↔ RecordReaderForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/15-RecordReaderForm-Panel_PageMachineHistory-application.json) · [证据2](geometry/15-RecordReaderForm-Panel_PageMachineHistory-memory.json) · [证据3](geometry/15-RecordReaderForm-Panel_PageMachineHistory-scroll-top.json) · [证据4](geometry/15-RecordReaderForm-Panel_PageMachineHistory-state-base.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_MachineHistoryLoadingState](state-layout/15-RecordReaderForm-Panel_PageMachineHistory-MachineHistoryLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MachineHistoryEmptyState](state-layout/15-RecordReaderForm-Panel_PageMachineHistory-MachineHistoryEmptyState.png) | 已检查 | memory、world |
| [Grp_MachineHistoryErrorState](state-layout/15-RecordReaderForm-Panel_PageMachineHistory-MachineHistoryErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MachineHistorySuccessState](state-layout/15-RecordReaderForm-Panel_PageMachineHistory-MachineHistorySuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MachineHistoryDisabledState](state-layout/15-RecordReaderForm-Panel_PageMachineHistory-MachineHistoryDisabledState.png) | 已检查 | application |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-15-MachineHistory-normal.png)、[merged](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-15-MachineHistory-merged.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 078 / 16 / 作物图鉴详情 · CropKnowledgeForm/Panel_PageCropKnowledge

Prefab：[Assets/Game/Prefabs/UI/Operations/CropKnowledgeForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/CropKnowledgeForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/CropKnowledgeForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/CropKnowledgeForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-16-CropKnowledge-wheat.png)；证据：[application](runtime/16-CropKnowledgeForm-Panel_PageCropKnowledge-application.png) · [memory](runtime/16-CropKnowledgeForm-Panel_PageCropKnowledge-memory.png) · [world](runtime/16-CropKnowledgeForm-Panel_PageCropKnowledge-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0351 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0352, UIA-0353, UIA-0354, UIA-0355 |
| 5. 九宫格与边角 | 发现问题 | UIA-0352 |
| 6. 对齐间距留白 | 发现问题 | UIA-0352, UIA-0355 |
| 7. 文字与可读性 | 发现问题 | UIA-0352, UIA-0353, UIA-0354, UIA-0355 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0355 |
| 9. 素材与状态语义 | 发现问题 | UIA-0350, UIA-0351, UIA-0352 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0350, UIA-0352, UIA-0353, UIA-0355 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0350 / P1 / 业务缺口 | **BUSINESS_SOURCE_NOT_CONNECTED** · `运行时数据正文`<br>本次实际运行说明：作物知识尚未接入运行路径：作物图鉴数据没有来源。<br>期望：满足标准 9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/16-CropKnowledgeForm-Panel_PageCropKnowledge-world.png) · [证据2](geometry/16-CropKnowledgeForm-Panel_PageCropKnowledge-world.json) |
| UIA-0351 / P1 / 已确认 | **CROP_IDENTITY_MISSING** · `作物知识左侧身份图`<br>作物知识使用通用工坊示意图，未呈现文档要求的 UI-MASTER-014 种子、作物或资源点身份图，无法从图片辨认作物。<br>期望：满足标准 3,9：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/16-CropKnowledgeForm-Panel_PageCropKnowledge.png) |
| UIA-0352 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `CropKnowledgeForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/16-CropKnowledgeForm-Panel_PageCropKnowledge-application.json) · [证据2](geometry/16-CropKnowledgeForm-Panel_PageCropKnowledge-memory.json) · [证据3](geometry/16-CropKnowledgeForm-Panel_PageCropKnowledge-scroll-top.json) · [证据4](geometry/16-CropKnowledgeForm-Panel_PageCropKnowledge-state-CropKnowledgeDisabledState.json) |
| UIA-0353 / P1 / 已确认 | **TEXT_MASK_CLIP** · `CropKnowledgeForm/Panel_Frame/Grp_PageHost/Panel_PageCropKnowledge/Panel_CropKnowledgeKnowledge/List_CropKnowledgeKnowledge/Viewport_CropKnowledgeKnowledge/Content_CropKnowledgeKnowledge/Txt_CropKnowledgeKnowledgeBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/16-CropKnowledgeForm-Panel_PageCropKnowledge-application.json) · [证据2](geometry/16-CropKnowledgeForm-Panel_PageCropKnowledge-memory.json) · [证据3](geometry/16-CropKnowledgeForm-Panel_PageCropKnowledge-scroll-top.json) · [证据4](geometry/16-CropKnowledgeForm-Panel_PageCropKnowledge-world-scroll-top.json) |
| UIA-0354 / P1 / 已确认 | **TEXT_OVERLAP** · `CropKnowledgeForm/Panel_Frame/Grp_PageHost/Panel_PageCropKnowledge/Txt_CropKnowledgeTitle ↔ CropKnowledgeForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/16-CropKnowledgeForm-Panel_PageCropKnowledge-application.json) · [证据2](geometry/16-CropKnowledgeForm-Panel_PageCropKnowledge-memory.json) · [证据3](geometry/16-CropKnowledgeForm-Panel_PageCropKnowledge-scroll-top.json) · [证据4](geometry/16-CropKnowledgeForm-Panel_PageCropKnowledge-state-base.json) |
| UIA-0355 / P2 / 已确认 | **UNAVAILABLE_REASON_REPEATED** · `CropKnowledgeForm/Panel_Frame/Grp_PageHost/Panel_PageCropKnowledge/Grp_CropKnowledgeDisabledState/Panel_CropKnowledgeDisabledMessage/Txt_CropKnowledgeDisabledMessage`<br>相同不可用原因同时出现在状态条和多个信息卡中；浮动状态条压在插画/内容区域，重复文字分散焦点。<br>期望：满足标准 4,6,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/16-CropKnowledgeForm-Panel_PageCropKnowledge-world.json) · [证据2](runtime/16-CropKnowledgeForm-Panel_PageCropKnowledge-world.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_CropKnowledgeLoadingState](state-layout/16-CropKnowledgeForm-Panel_PageCropKnowledge-CropKnowledgeLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_CropKnowledgeEmptyState](state-layout/16-CropKnowledgeForm-Panel_PageCropKnowledge-CropKnowledgeEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_CropKnowledgeErrorState](state-layout/16-CropKnowledgeForm-Panel_PageCropKnowledge-CropKnowledgeErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_CropKnowledgeSuccessState](state-layout/16-CropKnowledgeForm-Panel_PageCropKnowledge-CropKnowledgeSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_CropKnowledgeDisabledState](state-layout/16-CropKnowledgeForm-Panel_PageCropKnowledge-CropKnowledgeDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[wheat](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-16-CropKnowledge-wheat.png)、[red](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-16-CropKnowledge-red.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 079 / 16 / 新功能说明 · FeatureHelpForm/Panel_PageFeatureHelp

Prefab：[Assets/Game/Prefabs/UI/Operations/FeatureHelpForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/FeatureHelpForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/FeatureHelpForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/FeatureHelpForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-16-FeatureHelp-normal.png)；证据：[application](runtime/16-FeatureHelpForm-Panel_PageFeatureHelp-application.png) · [memory](runtime/16-FeatureHelpForm-Panel_PageFeatureHelp-memory.png) · [world](runtime/16-FeatureHelpForm-Panel_PageFeatureHelp-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0360 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0357, UIA-0358, UIA-0359, UIA-0361 |
| 5. 九宫格与边角 | 发现问题 | UIA-0357 |
| 6. 对齐间距留白 | 发现问题 | UIA-0357, UIA-0361 |
| 7. 文字与可读性 | 发现问题 | UIA-0357, UIA-0358, UIA-0359, UIA-0361 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0360, UIA-0361 |
| 9. 素材与状态语义 | 发现问题 | UIA-0356, UIA-0357 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0356, UIA-0357, UIA-0358, UIA-0361 |
| 12. 同类页面一致性 | 发现问题 | UIA-0360 |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0356 / P1 / 业务缺口 | **BUSINESS_SOURCE_NOT_CONNECTED** · `运行时数据正文`<br>本次实际运行说明：功能说明尚未接入运行路径：说明文本还没有录入本地化表。<br>期望：满足标准 9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/16-FeatureHelpForm-Panel_PageFeatureHelp-world.png) · [证据2](geometry/16-FeatureHelpForm-Panel_PageFeatureHelp-world.json) |
| UIA-0357 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `FeatureHelpForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/16-FeatureHelpForm-Panel_PageFeatureHelp-application.json) · [证据2](geometry/16-FeatureHelpForm-Panel_PageFeatureHelp-memory.json) · [证据3](geometry/16-FeatureHelpForm-Panel_PageFeatureHelp-scroll-top.json) · [证据4](geometry/16-FeatureHelpForm-Panel_PageFeatureHelp-state-FeatureHelpDisabledState.json) |
| UIA-0358 / P1 / 已确认 | **TEXT_MASK_CLIP** · `FeatureHelpForm/Panel_Frame/Grp_PageHost/Panel_PageFeatureHelp/Panel_FeatureHelpNext/List_FeatureHelpNext/Viewport_FeatureHelpNext/Content_FeatureHelpNext/Txt_FeatureHelpNextBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/16-FeatureHelpForm-Panel_PageFeatureHelp-application.json) · [证据2](geometry/16-FeatureHelpForm-Panel_PageFeatureHelp-memory.json) · [证据3](geometry/16-FeatureHelpForm-Panel_PageFeatureHelp-scroll-top.json) · [证据4](geometry/16-FeatureHelpForm-Panel_PageFeatureHelp-world-scroll-top.json) |
| UIA-0359 / P1 / 已确认 | **TEXT_OVERLAP** · `FeatureHelpForm/Panel_Frame/Grp_PageHost/Panel_PageFeatureHelp/Txt_FeatureHelpTitle ↔ FeatureHelpForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/16-FeatureHelpForm-Panel_PageFeatureHelp-application.json) · [证据2](geometry/16-FeatureHelpForm-Panel_PageFeatureHelp-memory.json) · [证据3](geometry/16-FeatureHelpForm-Panel_PageFeatureHelp-scroll-top.json) · [证据4](geometry/16-FeatureHelpForm-Panel_PageFeatureHelp-state-base.json) |
| UIA-0360 / P2 / 已确认 | **GUIDE_INFORMATION_PRIORITY** · `教学/帮助/规则插画与正文区域`<br>同一通用工坊图占据较大面积，规则正文、步骤和主题信息被压缩；需按页面用途调整插画和阅读区域的主次。<br>期望：满足标准 3,8,12：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/16-FeatureHelpForm-Panel_PageFeatureHelp.png) |
| UIA-0361 / P2 / 已确认 | **UNAVAILABLE_REASON_REPEATED** · `FeatureHelpForm/Panel_Frame/Grp_PageHost/Panel_PageFeatureHelp/Grp_FeatureHelpDisabledState/Panel_FeatureHelpDisabledMessage/Txt_FeatureHelpDisabledMessage`<br>相同不可用原因同时出现在状态条和多个信息卡中；浮动状态条压在插画/内容区域，重复文字分散焦点。<br>期望：满足标准 4,6,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/16-FeatureHelpForm-Panel_PageFeatureHelp-world.json) · [证据2](runtime/16-FeatureHelpForm-Panel_PageFeatureHelp-world.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_FeatureHelpLoadingState](state-layout/16-FeatureHelpForm-Panel_PageFeatureHelp-FeatureHelpLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_FeatureHelpEmptyState](state-layout/16-FeatureHelpForm-Panel_PageFeatureHelp-FeatureHelpEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_FeatureHelpErrorState](state-layout/16-FeatureHelpForm-Panel_PageFeatureHelp-FeatureHelpErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_FeatureHelpSuccessState](state-layout/16-FeatureHelpForm-Panel_PageFeatureHelp-FeatureHelpSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_FeatureHelpDisabledState](state-layout/16-FeatureHelpForm-Panel_PageFeatureHelp-FeatureHelpDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-16-FeatureHelp-normal.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 080 / 16 / 帮助 · HelpForm/Panel_PageHelp

Prefab：[Assets/Game/Prefabs/UI/Operations/HelpForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/HelpForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/HelpForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/HelpForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-16-Help-normal.png)；证据：[application](runtime/16-HelpForm-Panel_PageHelp-application.png) · [memory](runtime/16-HelpForm-Panel_PageHelp-memory.png) · [world](runtime/16-HelpForm-Panel_PageHelp-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0366 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0363, UIA-0364, UIA-0365, UIA-0367 |
| 5. 九宫格与边角 | 发现问题 | UIA-0363 |
| 6. 对齐间距留白 | 发现问题 | UIA-0363, UIA-0367 |
| 7. 文字与可读性 | 发现问题 | UIA-0363, UIA-0364, UIA-0365, UIA-0367 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0366, UIA-0367 |
| 9. 素材与状态语义 | 发现问题 | UIA-0362, UIA-0363 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0362, UIA-0363, UIA-0364, UIA-0367 |
| 12. 同类页面一致性 | 发现问题 | UIA-0366 |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0362 / P1 / 业务缺口 | **BUSINESS_SOURCE_NOT_CONNECTED** · `运行时数据正文`<br>本次实际运行说明：帮助内容尚未接入运行路径：帮助条目还没有录入本地化表。<br>期望：满足标准 9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/16-HelpForm-Panel_PageHelp-world.png) · [证据2](geometry/16-HelpForm-Panel_PageHelp-world.json) |
| UIA-0363 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `HelpForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/16-HelpForm-Panel_PageHelp-application.json) · [证据2](geometry/16-HelpForm-Panel_PageHelp-memory.json) · [证据3](geometry/16-HelpForm-Panel_PageHelp-scroll-top.json) · [证据4](geometry/16-HelpForm-Panel_PageHelp-state-HelpDisabledState.json) |
| UIA-0364 / P1 / 已确认 | **TEXT_MASK_CLIP** · `HelpForm/Panel_Frame/Grp_PageHost/Panel_PageHelp/Panel_HelpArticle/List_HelpArticle/Viewport_HelpArticle/Content_HelpArticle/Txt_HelpArticleBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/16-HelpForm-Panel_PageHelp-application.json) · [证据2](geometry/16-HelpForm-Panel_PageHelp-memory.json) · [证据3](geometry/16-HelpForm-Panel_PageHelp-scroll-top.json) · [证据4](geometry/16-HelpForm-Panel_PageHelp-world-scroll-top.json) |
| UIA-0365 / P1 / 已确认 | **TEXT_OVERLAP** · `HelpForm/Panel_Frame/Grp_PageHost/Panel_PageHelp/Txt_HelpTitle ↔ HelpForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/16-HelpForm-Panel_PageHelp-application.json) · [证据2](geometry/16-HelpForm-Panel_PageHelp-memory.json) · [证据3](geometry/16-HelpForm-Panel_PageHelp-scroll-top.json) · [证据4](geometry/16-HelpForm-Panel_PageHelp-state-base.json) |
| UIA-0366 / P2 / 已确认 | **GUIDE_INFORMATION_PRIORITY** · `教学/帮助/规则插画与正文区域`<br>同一通用工坊图占据较大面积，规则正文、步骤和主题信息被压缩；需按页面用途调整插画和阅读区域的主次。<br>期望：满足标准 3,8,12：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/16-HelpForm-Panel_PageHelp.png) |
| UIA-0367 / P2 / 已确认 | **UNAVAILABLE_REASON_REPEATED** · `HelpForm/Panel_Frame/Grp_PageHost/Panel_PageHelp/Grp_HelpDisabledState/Panel_HelpDisabledMessage/Txt_HelpDisabledMessage`<br>相同不可用原因同时出现在状态条和多个信息卡中；浮动状态条压在插画/内容区域，重复文字分散焦点。<br>期望：满足标准 4,6,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/16-HelpForm-Panel_PageHelp-world.json) · [证据2](runtime/16-HelpForm-Panel_PageHelp-world.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_HelpLoadingState](state-layout/16-HelpForm-Panel_PageHelp-HelpLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HelpEmptyState](state-layout/16-HelpForm-Panel_PageHelp-HelpEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HelpErrorState](state-layout/16-HelpForm-Panel_PageHelp-HelpErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HelpSuccessState](state-layout/16-HelpForm-Panel_PageHelp-HelpSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HelpDisabledState](state-layout/16-HelpForm-Panel_PageHelp-HelpDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-16-Help-normal.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 081 / 16 / 通用规则说明 · RuleHelpForm/Panel_PageRuleHelp

Prefab：[Assets/Game/Prefabs/UI/Operations/RuleHelpForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/RuleHelpForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/RuleHelpForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/RuleHelpForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-16-RuleHelp-normal.png)；证据：[application](runtime/16-RuleHelpForm-Panel_PageRuleHelp-application.png) · [memory](runtime/16-RuleHelpForm-Panel_PageRuleHelp-memory.png) · [world](runtime/16-RuleHelpForm-Panel_PageRuleHelp-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0372 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0369, UIA-0370, UIA-0371 |
| 5. 九宫格与边角 | 发现问题 | UIA-0369 |
| 6. 对齐间距留白 | 发现问题 | UIA-0369 |
| 7. 文字与可读性 | 发现问题 | UIA-0369, UIA-0370, UIA-0371 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0372 |
| 9. 素材与状态语义 | 发现问题 | UIA-0368, UIA-0369 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0368, UIA-0369, UIA-0370 |
| 12. 同类页面一致性 | 发现问题 | UIA-0372 |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0368 / P1 / 业务缺口 | **BUSINESS_SOURCE_NOT_CONNECTED** · `运行时数据正文`<br>本次实际运行说明：通用规则说明尚未接入运行路径：规则条目还没有录入本地化表。<br>期望：满足标准 9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/16-RuleHelpForm-Panel_PageRuleHelp-world.png) · [证据2](geometry/16-RuleHelpForm-Panel_PageRuleHelp-world.json) |
| UIA-0369 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `RuleHelpForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/16-RuleHelpForm-Panel_PageRuleHelp-application.json) · [证据2](geometry/16-RuleHelpForm-Panel_PageRuleHelp-memory.json) · [证据3](geometry/16-RuleHelpForm-Panel_PageRuleHelp-scroll-top.json) · [证据4](geometry/16-RuleHelpForm-Panel_PageRuleHelp-state-RuleHelpDisabledState.json) |
| UIA-0370 / P1 / 已确认 | **TEXT_MASK_CLIP** · `RuleHelpForm/Panel_Frame/Grp_PageHost/Panel_PageRuleHelp/Panel_RuleHelpDescription/List_RuleHelpDescription/Viewport_RuleHelpDescription/Content_RuleHelpDescription/Txt_RuleHelpDescriptionBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/16-RuleHelpForm-Panel_PageRuleHelp-application.json) · [证据2](geometry/16-RuleHelpForm-Panel_PageRuleHelp-memory.json) · [证据3](geometry/16-RuleHelpForm-Panel_PageRuleHelp-scroll-top.json) · [证据4](geometry/16-RuleHelpForm-Panel_PageRuleHelp-world-scroll-top.json) |
| UIA-0371 / P1 / 已确认 | **TEXT_OVERLAP** · `RuleHelpForm/Panel_Frame/Grp_PageHost/Panel_PageRuleHelp/Txt_RuleHelpTitle ↔ RuleHelpForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/16-RuleHelpForm-Panel_PageRuleHelp-application.json) · [证据2](geometry/16-RuleHelpForm-Panel_PageRuleHelp-memory.json) · [证据3](geometry/16-RuleHelpForm-Panel_PageRuleHelp-scroll-top.json) · [证据4](geometry/16-RuleHelpForm-Panel_PageRuleHelp-state-base.json) |
| UIA-0372 / P2 / 已确认 | **GUIDE_INFORMATION_PRIORITY** · `教学/帮助/规则插画与正文区域`<br>同一通用工坊图占据较大面积，规则正文、步骤和主题信息被压缩；需按页面用途调整插画和阅读区域的主次。<br>期望：满足标准 3,8,12：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/16-RuleHelpForm-Panel_PageRuleHelp.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_RuleHelpLoadingState](state-layout/16-RuleHelpForm-Panel_PageRuleHelp-RuleHelpLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_RuleHelpEmptyState](state-layout/16-RuleHelpForm-Panel_PageRuleHelp-RuleHelpEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_RuleHelpErrorState](state-layout/16-RuleHelpForm-Panel_PageRuleHelp-RuleHelpErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_RuleHelpSuccessState](state-layout/16-RuleHelpForm-Panel_PageRuleHelp-RuleHelpSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_RuleHelpDisabledState](state-layout/16-RuleHelpForm-Panel_PageRuleHelp-RuleHelpDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[normal](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-16-RuleHelp-normal.png)、[empty](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-16-RuleHelp-empty.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 082 / 16 / 基础引导 · TutorialForm/Panel_PageTutorial

Prefab：[Assets/Game/Prefabs/UI/Operations/TutorialForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/TutorialForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/TutorialForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/TutorialForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-16-Tutorial-current.png)；证据：[application](runtime/16-TutorialForm-Panel_PageTutorial-application.png) · [memory](runtime/16-TutorialForm-Panel_PageTutorial-memory.png) · [world](runtime/16-TutorialForm-Panel_PageTutorial-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 发现问题 | UIA-0377 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0374, UIA-0375, UIA-0376, UIA-0378 |
| 5. 九宫格与边角 | 发现问题 | UIA-0374 |
| 6. 对齐间距留白 | 发现问题 | UIA-0374, UIA-0378 |
| 7. 文字与可读性 | 发现问题 | UIA-0374, UIA-0375, UIA-0376, UIA-0378 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0377, UIA-0378 |
| 9. 素材与状态语义 | 发现问题 | UIA-0373, UIA-0374 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0373, UIA-0374, UIA-0375, UIA-0378 |
| 12. 同类页面一致性 | 发现问题 | UIA-0377 |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0373 / P1 / 业务缺口 | **BUSINESS_SOURCE_NOT_CONNECTED** · `运行时数据正文`<br>本次实际运行说明：教程内容尚未接入运行路径：步骤与触发条件还没有配置载体。<br>期望：满足标准 9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/16-TutorialForm-Panel_PageTutorial-world.png) · [证据2](geometry/16-TutorialForm-Panel_PageTutorial-world.json) |
| UIA-0374 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `TutorialForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/16-TutorialForm-Panel_PageTutorial-application.json) · [证据2](geometry/16-TutorialForm-Panel_PageTutorial-memory.json) · [证据3](geometry/16-TutorialForm-Panel_PageTutorial-scroll-top.json) · [证据4](geometry/16-TutorialForm-Panel_PageTutorial-state-TutorialDisabledState.json) |
| UIA-0375 / P1 / 已确认 | **TEXT_MASK_CLIP** · `TutorialForm/Panel_Frame/Grp_PageHost/Panel_PageTutorial/Panel_TutorialLinks/List_TutorialLinks/Viewport_TutorialLinks/Content_TutorialLinks/Txt_TutorialLinksBody`<br>实际字符横向越出列表遮罩，右侧正文/数值被硬裁切；当前模板未提供省略或可完整阅读的布局。<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/16-TutorialForm-Panel_PageTutorial-application.json) · [证据2](geometry/16-TutorialForm-Panel_PageTutorial-memory.json) · [证据3](geometry/16-TutorialForm-Panel_PageTutorial-scroll-top.json) · [证据4](geometry/16-TutorialForm-Panel_PageTutorial-world-scroll-top.json) |
| UIA-0376 / P1 / 已确认 | **TEXT_OVERLAP** · `TutorialForm/Panel_Frame/Grp_PageHost/Panel_PageTutorial/Txt_TutorialTitle ↔ TutorialForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/16-TutorialForm-Panel_PageTutorial-application.json) · [证据2](geometry/16-TutorialForm-Panel_PageTutorial-memory.json) · [证据3](geometry/16-TutorialForm-Panel_PageTutorial-scroll-top.json) · [证据4](geometry/16-TutorialForm-Panel_PageTutorial-state-base.json) |
| UIA-0377 / P2 / 已确认 | **GUIDE_INFORMATION_PRIORITY** · `教学/帮助/规则插画与正文区域`<br>同一通用工坊图占据较大面积，规则正文、步骤和主题信息被压缩；需按页面用途调整插画和阅读区域的主次。<br>期望：满足标准 3,8,12：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/16-TutorialForm-Panel_PageTutorial.png) |
| UIA-0378 / P2 / 已确认 | **UNAVAILABLE_REASON_REPEATED** · `TutorialForm/Panel_Frame/Grp_PageHost/Panel_PageTutorial/Grp_TutorialDisabledState/Panel_TutorialDisabledMessage/Txt_TutorialDisabledMessage`<br>相同不可用原因同时出现在状态条和多个信息卡中；浮动状态条压在插画/内容区域，重复文字分散焦点。<br>期望：满足标准 4,6,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/16-TutorialForm-Panel_PageTutorial-world.json) · [证据2](runtime/16-TutorialForm-Panel_PageTutorial-world.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_TutorialLoadingState](state-layout/16-TutorialForm-Panel_PageTutorial-TutorialLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_TutorialEmptyState](state-layout/16-TutorialForm-Panel_PageTutorial-TutorialEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_TutorialErrorState](state-layout/16-TutorialForm-Panel_PageTutorial-TutorialErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_TutorialSuccessState](state-layout/16-TutorialForm-Panel_PageTutorial-TutorialSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_TutorialDisabledState](state-layout/16-TutorialForm-Panel_PageTutorial-TutorialDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[current](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-16-Tutorial-current.png)、[locked](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-16-Tutorial-locked.png)、[unlocked](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-16-Tutorial-unlocked.png)、[empty](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-16-Tutorial-empty.png)、[current](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-16.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 083 / 17 / 算法应用确认 · OperationDialogForm/Panel_PageAlgorithmApply

Prefab：[Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-AlgorithmApply-pending.png)；证据：[application](runtime/17-OperationDialogForm-Panel_PageAlgorithmApply-application.png) · [memory](runtime/17-OperationDialogForm-Panel_PageAlgorithmApply-memory.png) · [world](runtime/17-OperationDialogForm-Panel_PageAlgorithmApply-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 发现问题 | UIA-0379 |
| 3. 尺寸与比例 | 发现问题 | UIA-0379 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0379, UIA-0382, UIA-0383 |
| 5. 九宫格与边角 | 发现问题 | UIA-0382 |
| 6. 对齐间距留白 | 发现问题 | UIA-0379, UIA-0382 |
| 7. 文字与可读性 | 发现问题 | UIA-0382, UIA-0383 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0380 |
| 9. 素材与状态语义 | 发现问题 | UIA-0380, UIA-0381, UIA-0382 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0380, UIA-0381 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0381, UIA-0382 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0379 / P1 / 已确认 | **CONFIRM_ART_OBSCURED** · `确认页左侧插画与对象信息卡`<br>左下对象信息卡覆盖插画底部，插画被明显截断，台座和对象没有完整展示空间。<br>期望：满足标准 2,3,4,6：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/17-OperationDialogForm-Panel_PageAlgorithmApply.png) |
| UIA-0380 / P1 / 已确认 | **DANGEROUS_ACTION_HIERARCHY** · `底部确认/取消/删除等按钮`<br>保留、取消、提交、删除或放弃进度采用近似同权重按钮，危险操作缺少足够醒目的差异提示，无法快速区分安全出口与执行动作。<br>期望：满足标准 8,9,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/17-OperationDialogForm-Panel_PageAlgorithmApply.png) |
| UIA-0381 / P1 / 业务缺口 | **DIALOG_BUSINESS_PATH_MISSING** · `OperationDialogForm 非硬件确认页`<br>原生实现仅接入HardwareConfirm；本页没有对应请求消费、运行数据渲染和底部动作绑定。当前页面是声明布局，不能验证其业务成功/失败、确认/取消转换。<br>期望：缺少接入时明确不可用；真正业务流程需在批准范围内单独接入。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/17-OperationDialogForm-Panel_PageAlgorithmApply-world.png) · [证据2](geometry/17-OperationDialogForm-Panel_PageAlgorithmApply-world.json) |
| UIA-0382 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `OperationDialogForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageAlgorithmApply-state-AlgorithmApplyDisabledState.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageAlgorithmApply-state-AlgorithmApplyEmptyState.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageAlgorithmApply-state-AlgorithmApplyErrorState.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageAlgorithmApply-state-AlgorithmApplyLoadingState.json) |
| UIA-0383 / P1 / 已确认 | **TEXT_OVERLAP** · `OperationDialogForm/Panel_Frame/Grp_PageHost/Panel_PageAlgorithmApply/Txt_AlgorithmApplyTitle ↔ OperationDialogForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageAlgorithmApply-application.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageAlgorithmApply-memory.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageAlgorithmApply-scroll-bottom.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageAlgorithmApply-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_AlgorithmApplyLoadingState](state-layout/17-OperationDialogForm-Panel_PageAlgorithmApply-AlgorithmApplyLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_AlgorithmApplyEmptyState](state-layout/17-OperationDialogForm-Panel_PageAlgorithmApply-AlgorithmApplyEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_AlgorithmApplyErrorState](state-layout/17-OperationDialogForm-Panel_PageAlgorithmApply-AlgorithmApplyErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_AlgorithmApplySuccessState](state-layout/17-OperationDialogForm-Panel_PageAlgorithmApply-AlgorithmApplySuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_AlgorithmApplyDisabledState](state-layout/17-OperationDialogForm-Panel_PageAlgorithmApply-AlgorithmApplyDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[pending](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-AlgorithmApply-pending.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 084 / 17 / 普通操作确认 · OperationDialogForm/Panel_PageBasicConfirm

Prefab：[Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-BasicConfirm-neutral.png)；证据：[application](runtime/17-OperationDialogForm-Panel_PageBasicConfirm-application.png) · [memory](runtime/17-OperationDialogForm-Panel_PageBasicConfirm-memory.png) · [world](runtime/17-OperationDialogForm-Panel_PageBasicConfirm-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0386, UIA-0387 |
| 5. 九宫格与边角 | 发现问题 | UIA-0386 |
| 6. 对齐间距留白 | 发现问题 | UIA-0386 |
| 7. 文字与可读性 | 发现问题 | UIA-0386, UIA-0387 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0384 |
| 9. 素材与状态语义 | 发现问题 | UIA-0384, UIA-0385, UIA-0386 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0384, UIA-0385 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0385, UIA-0386 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0384 / P1 / 已确认 | **DANGEROUS_ACTION_HIERARCHY** · `底部确认/取消/删除等按钮`<br>保留、取消、提交、删除或放弃进度采用近似同权重按钮，危险操作缺少足够醒目的差异提示，无法快速区分安全出口与执行动作。<br>期望：满足标准 8,9,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/17-OperationDialogForm-Panel_PageBasicConfirm.png) |
| UIA-0385 / P1 / 业务缺口 | **DIALOG_BUSINESS_PATH_MISSING** · `OperationDialogForm 非硬件确认页`<br>原生实现仅接入HardwareConfirm；本页没有对应请求消费、运行数据渲染和底部动作绑定。当前页面是声明布局，不能验证其业务成功/失败、确认/取消转换。<br>期望：缺少接入时明确不可用；真正业务流程需在批准范围内单独接入。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/17-OperationDialogForm-Panel_PageBasicConfirm-world.png) · [证据2](geometry/17-OperationDialogForm-Panel_PageBasicConfirm-world.json) |
| UIA-0386 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `OperationDialogForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageBasicConfirm-state-BasicConfirmDisabledState.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageBasicConfirm-state-BasicConfirmEmptyState.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageBasicConfirm-state-BasicConfirmErrorState.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageBasicConfirm-state-BasicConfirmLoadingState.json) |
| UIA-0387 / P1 / 已确认 | **TEXT_OVERLAP** · `OperationDialogForm/Panel_Frame/Grp_PageHost/Panel_PageBasicConfirm/Txt_BasicConfirmTitle ↔ OperationDialogForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageBasicConfirm-application.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageBasicConfirm-memory.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageBasicConfirm-scroll-top.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageBasicConfirm-state-base.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_BasicConfirmLoadingState](state-layout/17-OperationDialogForm-Panel_PageBasicConfirm-BasicConfirmLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_BasicConfirmEmptyState](state-layout/17-OperationDialogForm-Panel_PageBasicConfirm-BasicConfirmEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_BasicConfirmErrorState](state-layout/17-OperationDialogForm-Panel_PageBasicConfirm-BasicConfirmErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_BasicConfirmSuccessState](state-layout/17-OperationDialogForm-Panel_PageBasicConfirm-BasicConfirmSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_BasicConfirmDisabledState](state-layout/17-OperationDialogForm-Panel_PageBasicConfirm-BasicConfirmDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[neutral](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-BasicConfirm-neutral.png)、[danger](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-BasicConfirm-danger.png)、[pending](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-BasicConfirm-pending.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 085 / 17 / 施工制造取消与退款 · OperationDialogForm/Panel_PageCancelProduction

Prefab：[Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-CancelProduction-danger.png)；证据：[application](runtime/17-OperationDialogForm-Panel_PageCancelProduction-application.png) · [memory](runtime/17-OperationDialogForm-Panel_PageCancelProduction-memory.png) · [world](runtime/17-OperationDialogForm-Panel_PageCancelProduction-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 发现问题 | UIA-0388 |
| 3. 尺寸与比例 | 发现问题 | UIA-0388 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0388, UIA-0391, UIA-0392 |
| 5. 九宫格与边角 | 发现问题 | UIA-0391 |
| 6. 对齐间距留白 | 发现问题 | UIA-0388, UIA-0391 |
| 7. 文字与可读性 | 发现问题 | UIA-0391, UIA-0392 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0389 |
| 9. 素材与状态语义 | 发现问题 | UIA-0389, UIA-0390, UIA-0391 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0389, UIA-0390 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0390, UIA-0391 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0388 / P1 / 已确认 | **CONFIRM_ART_OBSCURED** · `确认页左侧插画与对象信息卡`<br>左下对象信息卡覆盖插画底部，插画被明显截断，台座和对象没有完整展示空间。<br>期望：满足标准 2,3,4,6：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/17-OperationDialogForm-Panel_PageCancelProduction.png) |
| UIA-0389 / P1 / 已确认 | **DANGEROUS_ACTION_HIERARCHY** · `底部确认/取消/删除等按钮`<br>保留、取消、提交、删除或放弃进度采用近似同权重按钮，危险操作缺少足够醒目的差异提示，无法快速区分安全出口与执行动作。<br>期望：满足标准 8,9,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/17-OperationDialogForm-Panel_PageCancelProduction.png) |
| UIA-0390 / P1 / 业务缺口 | **DIALOG_BUSINESS_PATH_MISSING** · `OperationDialogForm 非硬件确认页`<br>原生实现仅接入HardwareConfirm；本页没有对应请求消费、运行数据渲染和底部动作绑定。当前页面是声明布局，不能验证其业务成功/失败、确认/取消转换。<br>期望：缺少接入时明确不可用；真正业务流程需在批准范围内单独接入。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/17-OperationDialogForm-Panel_PageCancelProduction-world.png) · [证据2](geometry/17-OperationDialogForm-Panel_PageCancelProduction-world.json) |
| UIA-0391 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `OperationDialogForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageCancelProduction-state-CancelProductionDisabledState.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageCancelProduction-state-CancelProductionEmptyState.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageCancelProduction-state-CancelProductionErrorState.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageCancelProduction-state-CancelProductionLoadingState.json) |
| UIA-0392 / P1 / 已确认 | **TEXT_OVERLAP** · `OperationDialogForm/Panel_Frame/Grp_PageHost/Panel_PageCancelProduction/Txt_CancelProductionTitle ↔ OperationDialogForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageCancelProduction-application.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageCancelProduction-memory.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageCancelProduction-scroll-bottom.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageCancelProduction-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_CancelProductionLoadingState](state-layout/17-OperationDialogForm-Panel_PageCancelProduction-CancelProductionLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_CancelProductionEmptyState](state-layout/17-OperationDialogForm-Panel_PageCancelProduction-CancelProductionEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_CancelProductionErrorState](state-layout/17-OperationDialogForm-Panel_PageCancelProduction-CancelProductionErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_CancelProductionSuccessState](state-layout/17-OperationDialogForm-Panel_PageCancelProduction-CancelProductionSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_CancelProductionDisabledState](state-layout/17-OperationDialogForm-Panel_PageCancelProduction-CancelProductionDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[danger](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-CancelProduction-danger.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 086 / 17 / 显示模式保留恢复 · OperationDialogForm/Panel_PageDisplayKeep

Prefab：[Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-DisplayKeep-pending.png)；证据：[application](runtime/17-OperationDialogForm-Panel_PageDisplayKeep-application.png) · [memory](runtime/17-OperationDialogForm-Panel_PageDisplayKeep-memory.png) · [world](runtime/17-OperationDialogForm-Panel_PageDisplayKeep-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 发现问题 | UIA-0393 |
| 3. 尺寸与比例 | 发现问题 | UIA-0393 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0393, UIA-0396, UIA-0397 |
| 5. 九宫格与边角 | 发现问题 | UIA-0396 |
| 6. 对齐间距留白 | 发现问题 | UIA-0393, UIA-0396 |
| 7. 文字与可读性 | 发现问题 | UIA-0396, UIA-0397 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0394 |
| 9. 素材与状态语义 | 发现问题 | UIA-0394, UIA-0395, UIA-0396 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0394, UIA-0395 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0395, UIA-0396 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0393 / P1 / 已确认 | **CONFIRM_ART_OBSCURED** · `确认页左侧插画与对象信息卡`<br>左下对象信息卡覆盖插画底部，插画被明显截断，台座和对象没有完整展示空间。<br>期望：满足标准 2,3,4,6：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/17-OperationDialogForm-Panel_PageDisplayKeep.png) |
| UIA-0394 / P1 / 已确认 | **DANGEROUS_ACTION_HIERARCHY** · `底部确认/取消/删除等按钮`<br>保留、取消、提交、删除或放弃进度采用近似同权重按钮，危险操作缺少足够醒目的差异提示，无法快速区分安全出口与执行动作。<br>期望：满足标准 8,9,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/17-OperationDialogForm-Panel_PageDisplayKeep.png) |
| UIA-0395 / P1 / 业务缺口 | **DIALOG_BUSINESS_PATH_MISSING** · `OperationDialogForm 非硬件确认页`<br>原生实现仅接入HardwareConfirm；本页没有对应请求消费、运行数据渲染和底部动作绑定。当前页面是声明布局，不能验证其业务成功/失败、确认/取消转换。<br>期望：缺少接入时明确不可用；真正业务流程需在批准范围内单独接入。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/17-OperationDialogForm-Panel_PageDisplayKeep-world.png) · [证据2](geometry/17-OperationDialogForm-Panel_PageDisplayKeep-world.json) |
| UIA-0396 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `OperationDialogForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageDisplayKeep-state-DisplayKeepDisabledState.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageDisplayKeep-state-DisplayKeepEmptyState.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageDisplayKeep-state-DisplayKeepErrorState.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageDisplayKeep-state-DisplayKeepLoadingState.json) |
| UIA-0397 / P1 / 已确认 | **TEXT_OVERLAP** · `OperationDialogForm/Panel_Frame/Grp_PageHost/Panel_PageDisplayKeep/Txt_DisplayKeepTitle ↔ OperationDialogForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageDisplayKeep-application.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageDisplayKeep-memory.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageDisplayKeep-scroll-bottom.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageDisplayKeep-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_DisplayKeepLoadingState](state-layout/17-OperationDialogForm-Panel_PageDisplayKeep-DisplayKeepLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_DisplayKeepEmptyState](state-layout/17-OperationDialogForm-Panel_PageDisplayKeep-DisplayKeepEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_DisplayKeepErrorState](state-layout/17-OperationDialogForm-Panel_PageDisplayKeep-DisplayKeepErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_DisplayKeepSuccessState](state-layout/17-OperationDialogForm-Panel_PageDisplayKeep-DisplayKeepSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_DisplayKeepDisabledState](state-layout/17-OperationDialogForm-Panel_PageDisplayKeep-DisplayKeepDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[pending](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-DisplayKeep-pending.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 087 / 17 / 算法草稿离开 · OperationDialogForm/Panel_PageDraftExit

Prefab：[Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-DraftExit-danger.png)；证据：[application](runtime/17-OperationDialogForm-Panel_PageDraftExit-application.png) · [memory](runtime/17-OperationDialogForm-Panel_PageDraftExit-memory.png) · [world](runtime/17-OperationDialogForm-Panel_PageDraftExit-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0400, UIA-0401 |
| 5. 九宫格与边角 | 发现问题 | UIA-0400 |
| 6. 对齐间距留白 | 发现问题 | UIA-0400 |
| 7. 文字与可读性 | 发现问题 | UIA-0400, UIA-0401 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0398 |
| 9. 素材与状态语义 | 发现问题 | UIA-0398, UIA-0399, UIA-0400 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0398, UIA-0399 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0399, UIA-0400 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0398 / P1 / 已确认 | **DANGEROUS_ACTION_HIERARCHY** · `底部确认/取消/删除等按钮`<br>保留、取消、提交、删除或放弃进度采用近似同权重按钮，危险操作缺少足够醒目的差异提示，无法快速区分安全出口与执行动作。<br>期望：满足标准 8,9,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/17-OperationDialogForm-Panel_PageDraftExit.png) |
| UIA-0399 / P1 / 业务缺口 | **DIALOG_BUSINESS_PATH_MISSING** · `OperationDialogForm 非硬件确认页`<br>原生实现仅接入HardwareConfirm；本页没有对应请求消费、运行数据渲染和底部动作绑定。当前页面是声明布局，不能验证其业务成功/失败、确认/取消转换。<br>期望：缺少接入时明确不可用；真正业务流程需在批准范围内单独接入。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/17-OperationDialogForm-Panel_PageDraftExit-world.png) · [证据2](geometry/17-OperationDialogForm-Panel_PageDraftExit-world.json) |
| UIA-0400 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `OperationDialogForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageDraftExit-state-DraftExitDisabledState.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageDraftExit-state-DraftExitEmptyState.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageDraftExit-state-DraftExitErrorState.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageDraftExit-state-DraftExitLoadingState.json) |
| UIA-0401 / P1 / 已确认 | **TEXT_OVERLAP** · `OperationDialogForm/Panel_Frame/Grp_PageHost/Panel_PageDraftExit/Txt_DraftExitTitle ↔ OperationDialogForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageDraftExit-application.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageDraftExit-memory.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageDraftExit-scroll-top.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageDraftExit-state-base.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_DraftExitLoadingState](state-layout/17-OperationDialogForm-Panel_PageDraftExit-DraftExitLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_DraftExitEmptyState](state-layout/17-OperationDialogForm-Panel_PageDraftExit-DraftExitEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_DraftExitErrorState](state-layout/17-OperationDialogForm-Panel_PageDraftExit-DraftExitErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_DraftExitSuccessState](state-layout/17-OperationDialogForm-Panel_PageDraftExit-DraftExitSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_DraftExitDisabledState](state-layout/17-OperationDialogForm-Panel_PageDraftExit-DraftExitDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[danger](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-DraftExit-danger.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 088 / 17 / 未保存强退确认 · OperationDialogForm/Panel_PageForceExit

Prefab：[Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-ForceExit-danger.png)；证据：[application](runtime/17-OperationDialogForm-Panel_PageForceExit-application.png) · [memory](runtime/17-OperationDialogForm-Panel_PageForceExit-memory.png) · [world](runtime/17-OperationDialogForm-Panel_PageForceExit-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0404, UIA-0405 |
| 5. 九宫格与边角 | 发现问题 | UIA-0404 |
| 6. 对齐间距留白 | 发现问题 | UIA-0404 |
| 7. 文字与可读性 | 发现问题 | UIA-0404, UIA-0405 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0402 |
| 9. 素材与状态语义 | 发现问题 | UIA-0402, UIA-0403, UIA-0404 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0402, UIA-0403 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0403, UIA-0404 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0402 / P1 / 已确认 | **DANGEROUS_ACTION_HIERARCHY** · `底部确认/取消/删除等按钮`<br>保留、取消、提交、删除或放弃进度采用近似同权重按钮，危险操作缺少足够醒目的差异提示，无法快速区分安全出口与执行动作。<br>期望：满足标准 8,9,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/17-OperationDialogForm-Panel_PageForceExit.png) |
| UIA-0403 / P1 / 业务缺口 | **DIALOG_BUSINESS_PATH_MISSING** · `OperationDialogForm 非硬件确认页`<br>原生实现仅接入HardwareConfirm；本页没有对应请求消费、运行数据渲染和底部动作绑定。当前页面是声明布局，不能验证其业务成功/失败、确认/取消转换。<br>期望：缺少接入时明确不可用；真正业务流程需在批准范围内单独接入。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/17-OperationDialogForm-Panel_PageForceExit-world.png) · [证据2](geometry/17-OperationDialogForm-Panel_PageForceExit-world.json) |
| UIA-0404 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `OperationDialogForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageForceExit-state-ForceExitDisabledState.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageForceExit-state-ForceExitEmptyState.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageForceExit-state-ForceExitErrorState.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageForceExit-state-ForceExitLoadingState.json) |
| UIA-0405 / P1 / 已确认 | **TEXT_OVERLAP** · `OperationDialogForm/Panel_Frame/Grp_PageHost/Panel_PageForceExit/Txt_ForceExitTitle ↔ OperationDialogForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageForceExit-application.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageForceExit-memory.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageForceExit-scroll-top.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageForceExit-state-base.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_ForceExitLoadingState](state-layout/17-OperationDialogForm-Panel_PageForceExit-ForceExitLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ForceExitEmptyState](state-layout/17-OperationDialogForm-Panel_PageForceExit-ForceExitEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ForceExitErrorState](state-layout/17-OperationDialogForm-Panel_PageForceExit-ForceExitErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ForceExitSuccessState](state-layout/17-OperationDialogForm-Panel_PageForceExit-ForceExitSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ForceExitDisabledState](state-layout/17-OperationDialogForm-Panel_PageForceExit-ForceExitDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[danger](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-ForceExit-danger.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 089 / 17 / 硬件修改确认 · OperationDialogForm/Panel_PageHardwareConfirm

Prefab：[Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-HardwareConfirm-neutral.png)；证据：[application](runtime/17-OperationDialogForm-Panel_PageHardwareConfirm-application.png) · [memory](runtime/17-OperationDialogForm-Panel_PageHardwareConfirm-memory.png) · [world](runtime/17-OperationDialogForm-Panel_PageHardwareConfirm-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 发现问题 | UIA-0406 |
| 3. 尺寸与比例 | 发现问题 | UIA-0406 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0406, UIA-0408, UIA-0409, UIA-0410, UIA-0411, UIA-0412 |
| 5. 九宫格与边角 | 发现问题 | UIA-0408 |
| 6. 对齐间距留白 | 发现问题 | UIA-0406, UIA-0408, UIA-0412 |
| 7. 文字与可读性 | 发现问题 | UIA-0408, UIA-0409, UIA-0410, UIA-0411, UIA-0412 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0407, UIA-0412 |
| 9. 素材与状态语义 | 发现问题 | UIA-0407, UIA-0408 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0407 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0408, UIA-0409, UIA-0410, UIA-0412 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0406 / P1 / 已确认 | **CONFIRM_ART_OBSCURED** · `确认页左侧插画与对象信息卡`<br>左下对象信息卡覆盖插画底部，插画被明显截断，台座和对象没有完整展示空间。<br>期望：满足标准 2,3,4,6：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/17-OperationDialogForm-Panel_PageHardwareConfirm.png) |
| UIA-0407 / P1 / 已确认 | **DANGEROUS_ACTION_HIERARCHY** · `底部确认/取消/删除等按钮`<br>保留、取消、提交、删除或放弃进度采用近似同权重按钮，危险操作缺少足够醒目的差异提示，无法快速区分安全出口与执行动作。<br>期望：满足标准 8,9,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/17-OperationDialogForm-Panel_PageHardwareConfirm.png) |
| UIA-0408 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `OperationDialogForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageHardwareConfirm-application.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageHardwareConfirm-memory.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageHardwareConfirm-scroll-bottom.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageHardwareConfirm-scroll-top.json) |
| UIA-0409 / P1 / 已确认 | **TEXT_OVERFLOW** · `OperationDialogForm/Panel_Frame/Grp_PageHost/Panel_PageHardwareConfirm/Panel_HardwareConfirmChange/List_HardwareConfirmChange/Viewport_HardwareConfirmChange/Content_HardwareConfirmChange/Txt_HardwareConfirmChangeBody`<br>文字超出容器，字号 22；容器 392×120<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageHardwareConfirm-application.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageHardwareConfirm-memory.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageHardwareConfirm-scroll-bottom.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageHardwareConfirm-scroll-top.json) |
| UIA-0410 / P1 / 已确认 | **TEXT_OVERFLOW** · `OperationDialogForm/Panel_Frame/Grp_PageHost/Panel_PageHardwareConfirm/Panel_HardwareConfirmEffects/List_HardwareConfirmEffects/Viewport_HardwareConfirmEffects/Content_HardwareConfirmEffects/Txt_HardwareConfirmEffectsBody`<br>文字超出容器，字号 22；容器 392×120<br>期望：满足标准 4,7,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageHardwareConfirm-application.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageHardwareConfirm-memory.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageHardwareConfirm-scroll-bottom.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageHardwareConfirm-scroll-top.json) |
| UIA-0411 / P1 / 已确认 | **TEXT_OVERLAP** · `OperationDialogForm/Panel_Frame/Grp_PageHost/Panel_PageHardwareConfirm/Txt_HardwareConfirmTitle ↔ OperationDialogForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageHardwareConfirm-application.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageHardwareConfirm-memory.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageHardwareConfirm-scroll-bottom.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageHardwareConfirm-scroll-top.json) |
| UIA-0412 / P2 / 已确认 | **UNAVAILABLE_REASON_REPEATED** · `OperationDialogForm/Panel_Frame/Grp_PageHost/Panel_PageHardwareConfirm/Grp_HardwareConfirmDisabledState/Panel_HardwareConfirmDisabledMessage/Txt_HardwareConfirmDisabledMessage`<br>相同不可用原因同时出现在状态条和多个信息卡中；浮动状态条压在插画/内容区域，重复文字分散焦点。<br>期望：满足标准 4,6,7,8,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageHardwareConfirm-world.json) · [证据2](runtime/17-OperationDialogForm-Panel_PageHardwareConfirm-world.png) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_HardwareConfirmLoadingState](state-layout/17-OperationDialogForm-Panel_PageHardwareConfirm-HardwareConfirmLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HardwareConfirmEmptyState](state-layout/17-OperationDialogForm-Panel_PageHardwareConfirm-HardwareConfirmEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HardwareConfirmErrorState](state-layout/17-OperationDialogForm-Panel_PageHardwareConfirm-HardwareConfirmErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HardwareConfirmSuccessState](state-layout/17-OperationDialogForm-Panel_PageHardwareConfirm-HardwareConfirmSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_HardwareConfirmDisabledState](state-layout/17-OperationDialogForm-Panel_PageHardwareConfirm-HardwareConfirmDisabledState.png) | 已检查 | application、memory、world |

效果图要求的代表态：[neutral](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-HardwareConfirm-neutral.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 090 / 17 / 机器回收确认 · OperationDialogForm/Panel_PageMachineRecovery

Prefab：[Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-MachineRecovery-danger.png)；证据：[application](runtime/17-OperationDialogForm-Panel_PageMachineRecovery-application.png) · [memory](runtime/17-OperationDialogForm-Panel_PageMachineRecovery-memory.png) · [world](runtime/17-OperationDialogForm-Panel_PageMachineRecovery-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 发现问题 | UIA-0413 |
| 3. 尺寸与比例 | 发现问题 | UIA-0413 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0413, UIA-0416, UIA-0417 |
| 5. 九宫格与边角 | 发现问题 | UIA-0416 |
| 6. 对齐间距留白 | 发现问题 | UIA-0413, UIA-0416 |
| 7. 文字与可读性 | 发现问题 | UIA-0416, UIA-0417 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0414 |
| 9. 素材与状态语义 | 发现问题 | UIA-0414, UIA-0415, UIA-0416 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0414, UIA-0415 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0415, UIA-0416 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0413 / P1 / 已确认 | **CONFIRM_ART_OBSCURED** · `确认页左侧插画与对象信息卡`<br>左下对象信息卡覆盖插画底部，插画被明显截断，台座和对象没有完整展示空间。<br>期望：满足标准 2,3,4,6：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/17-OperationDialogForm-Panel_PageMachineRecovery.png) |
| UIA-0414 / P1 / 已确认 | **DANGEROUS_ACTION_HIERARCHY** · `底部确认/取消/删除等按钮`<br>保留、取消、提交、删除或放弃进度采用近似同权重按钮，危险操作缺少足够醒目的差异提示，无法快速区分安全出口与执行动作。<br>期望：满足标准 8,9,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/17-OperationDialogForm-Panel_PageMachineRecovery.png) |
| UIA-0415 / P1 / 业务缺口 | **DIALOG_BUSINESS_PATH_MISSING** · `OperationDialogForm 非硬件确认页`<br>原生实现仅接入HardwareConfirm；本页没有对应请求消费、运行数据渲染和底部动作绑定。当前页面是声明布局，不能验证其业务成功/失败、确认/取消转换。<br>期望：缺少接入时明确不可用；真正业务流程需在批准范围内单独接入。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/17-OperationDialogForm-Panel_PageMachineRecovery-world.png) · [证据2](geometry/17-OperationDialogForm-Panel_PageMachineRecovery-world.json) |
| UIA-0416 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `OperationDialogForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageMachineRecovery-state-MachineRecoveryDisabledState.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageMachineRecovery-state-MachineRecoveryEmptyState.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageMachineRecovery-state-MachineRecoveryErrorState.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageMachineRecovery-state-MachineRecoveryLoadingState.json) |
| UIA-0417 / P1 / 已确认 | **TEXT_OVERLAP** · `OperationDialogForm/Panel_Frame/Grp_PageHost/Panel_PageMachineRecovery/Txt_MachineRecoveryTitle ↔ OperationDialogForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageMachineRecovery-application.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageMachineRecovery-memory.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageMachineRecovery-scroll-bottom.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageMachineRecovery-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_MachineRecoveryLoadingState](state-layout/17-OperationDialogForm-Panel_PageMachineRecovery-MachineRecoveryLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MachineRecoveryEmptyState](state-layout/17-OperationDialogForm-Panel_PageMachineRecovery-MachineRecoveryEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MachineRecoveryErrorState](state-layout/17-OperationDialogForm-Panel_PageMachineRecovery-MachineRecoveryErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MachineRecoverySuccessState](state-layout/17-OperationDialogForm-Panel_PageMachineRecovery-MachineRecoverySuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_MachineRecoveryDisabledState](state-layout/17-OperationDialogForm-Panel_PageMachineRecovery-MachineRecoveryDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[danger](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-MachineRecovery-danger.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 091 / 17 / 建造与部署确认 · OperationDialogForm/Panel_PagePlacementConfirm

Prefab：[Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-PlacementConfirm-neutral.png)；证据：[application](runtime/17-OperationDialogForm-Panel_PagePlacementConfirm-application.png) · [memory](runtime/17-OperationDialogForm-Panel_PagePlacementConfirm-memory.png) · [world](runtime/17-OperationDialogForm-Panel_PagePlacementConfirm-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 发现问题 | UIA-0418 |
| 3. 尺寸与比例 | 发现问题 | UIA-0418 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0418, UIA-0421, UIA-0422 |
| 5. 九宫格与边角 | 发现问题 | UIA-0421 |
| 6. 对齐间距留白 | 发现问题 | UIA-0418, UIA-0421 |
| 7. 文字与可读性 | 发现问题 | UIA-0421, UIA-0422 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0419 |
| 9. 素材与状态语义 | 发现问题 | UIA-0419, UIA-0420, UIA-0421 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0419, UIA-0420 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0420, UIA-0421 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0418 / P1 / 已确认 | **CONFIRM_ART_OBSCURED** · `确认页左侧插画与对象信息卡`<br>左下对象信息卡覆盖插画底部，插画被明显截断，台座和对象没有完整展示空间。<br>期望：满足标准 2,3,4,6：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/17-OperationDialogForm-Panel_PagePlacementConfirm.png) |
| UIA-0419 / P1 / 已确认 | **DANGEROUS_ACTION_HIERARCHY** · `底部确认/取消/删除等按钮`<br>保留、取消、提交、删除或放弃进度采用近似同权重按钮，危险操作缺少足够醒目的差异提示，无法快速区分安全出口与执行动作。<br>期望：满足标准 8,9,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/17-OperationDialogForm-Panel_PagePlacementConfirm.png) |
| UIA-0420 / P1 / 业务缺口 | **DIALOG_BUSINESS_PATH_MISSING** · `OperationDialogForm 非硬件确认页`<br>原生实现仅接入HardwareConfirm；本页没有对应请求消费、运行数据渲染和底部动作绑定。当前页面是声明布局，不能验证其业务成功/失败、确认/取消转换。<br>期望：缺少接入时明确不可用；真正业务流程需在批准范围内单独接入。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/17-OperationDialogForm-Panel_PagePlacementConfirm-world.png) · [证据2](geometry/17-OperationDialogForm-Panel_PagePlacementConfirm-world.json) |
| UIA-0421 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `OperationDialogForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PagePlacementConfirm-state-PlacementConfirmDisabledState.json) · [证据2](geometry/17-OperationDialogForm-Panel_PagePlacementConfirm-state-PlacementConfirmEmptyState.json) · [证据3](geometry/17-OperationDialogForm-Panel_PagePlacementConfirm-state-PlacementConfirmErrorState.json) · [证据4](geometry/17-OperationDialogForm-Panel_PagePlacementConfirm-state-PlacementConfirmLoadingState.json) |
| UIA-0422 / P1 / 已确认 | **TEXT_OVERLAP** · `OperationDialogForm/Panel_Frame/Grp_PageHost/Panel_PagePlacementConfirm/Txt_PlacementConfirmTitle ↔ OperationDialogForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PagePlacementConfirm-application.json) · [证据2](geometry/17-OperationDialogForm-Panel_PagePlacementConfirm-memory.json) · [证据3](geometry/17-OperationDialogForm-Panel_PagePlacementConfirm-scroll-bottom.json) · [证据4](geometry/17-OperationDialogForm-Panel_PagePlacementConfirm-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_PlacementConfirmLoadingState](state-layout/17-OperationDialogForm-Panel_PagePlacementConfirm-PlacementConfirmLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_PlacementConfirmEmptyState](state-layout/17-OperationDialogForm-Panel_PagePlacementConfirm-PlacementConfirmEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_PlacementConfirmErrorState](state-layout/17-OperationDialogForm-Panel_PagePlacementConfirm-PlacementConfirmErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_PlacementConfirmSuccessState](state-layout/17-OperationDialogForm-Panel_PagePlacementConfirm-PlacementConfirmSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_PlacementConfirmDisabledState](state-layout/17-OperationDialogForm-Panel_PagePlacementConfirm-PlacementConfirmDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[neutral](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-PlacementConfirm-neutral.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 092 / 17 / 重命名 · OperationDialogForm/Panel_PageRename

Prefab：[Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-Rename-neutral.png)；证据：[application](runtime/17-OperationDialogForm-Panel_PageRename-application.png) · [memory](runtime/17-OperationDialogForm-Panel_PageRename-memory.png) · [world](runtime/17-OperationDialogForm-Panel_PageRename-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0424, UIA-0425, UIA-0426 |
| 5. 九宫格与边角 | 发现问题 | UIA-0425 |
| 6. 对齐间距留白 | 发现问题 | UIA-0424, UIA-0425 |
| 7. 文字与可读性 | 发现问题 | UIA-0425, UIA-0426 |
| 8. 信息层级与主操作 | 当前样本未发现新增问题 |  |
| 9. 素材与状态语义 | 发现问题 | UIA-0423, UIA-0425 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0423, UIA-0424 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0423, UIA-0425 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0423 / P1 / 业务缺口 | **DIALOG_BUSINESS_PATH_MISSING** · `OperationDialogForm 非硬件确认页`<br>原生实现仅接入HardwareConfirm；本页没有对应请求消费、运行数据渲染和底部动作绑定。当前页面是声明布局，不能验证其业务成功/失败、确认/取消转换。<br>期望：缺少接入时明确不可用；真正业务流程需在批准范围内单独接入。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/17-OperationDialogForm-Panel_PageRename-world.png) · [证据2](geometry/17-OperationDialogForm-Panel_PageRename-world.json) |
| UIA-0424 / P1 / 已确认 | **RENAME_INPUT_AT_EDGE** · `Rename 新名称输入框`<br>输入框贴在名称卡的下沿并突出卡片边缘，当前名称、输入标签和输入区间距不足。<br>期望：满足标准 4,6,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/17-OperationDialogForm-Panel_PageRename.png) |
| UIA-0425 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `OperationDialogForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageRename-state-RenameDisabledState.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageRename-state-RenameEmptyState.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageRename-state-RenameErrorState.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageRename-state-RenameLoadingState.json) |
| UIA-0426 / P1 / 已确认 | **TEXT_OVERLAP** · `OperationDialogForm/Panel_Frame/Grp_PageHost/Panel_PageRename/Txt_RenameTitle ↔ OperationDialogForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageRename-application.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageRename-memory.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageRename-scroll-bottom.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageRename-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_RenameLoadingState](state-layout/17-OperationDialogForm-Panel_PageRename-RenameLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_RenameEmptyState](state-layout/17-OperationDialogForm-Panel_PageRename-RenameEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_RenameErrorState](state-layout/17-OperationDialogForm-Panel_PageRename-RenameErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_RenameSuccessState](state-layout/17-OperationDialogForm-Panel_PageRename-RenameSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_RenameDisabledState](state-layout/17-OperationDialogForm-Panel_PageRename-RenameDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[neutral](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-Rename-neutral.png)、[invalid](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-Rename-invalid.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 093 / 17 / 删除覆盖与算法重置 · OperationDialogForm/Panel_PageStrongConfirm

Prefab：[Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-StrongConfirm-danger.png)；证据：[application](runtime/17-OperationDialogForm-Panel_PageStrongConfirm-application.png) · [memory](runtime/17-OperationDialogForm-Panel_PageStrongConfirm-memory.png) · [world](runtime/17-OperationDialogForm-Panel_PageStrongConfirm-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 发现问题 | UIA-0428 |
| 2. 图片实际可见位置 | 发现问题 | UIA-0428 |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0432, UIA-0433 |
| 5. 九宫格与边角 | 发现问题 | UIA-0432 |
| 6. 对齐间距留白 | 发现问题 | UIA-0432 |
| 7. 文字与可读性 | 发现问题 | UIA-0432, UIA-0433 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0428, UIA-0429 |
| 9. 素材与状态语义 | 发现问题 | UIA-0427, UIA-0429, UIA-0430, UIA-0432 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0427, UIA-0428, UIA-0429, UIA-0430, UIA-0431 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0427, UIA-0428, UIA-0430, UIA-0432 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0427 / P1 / 已确认 | **ACKNOWLEDGEMENT_NO_POINTER_TARGET** · `Tgl_StrongConfirmAcknowledged`<br>滚动到复选框完整可见后，中心射线仍命中装饰背景；复选框、勾选图和标签均不接收射线，无法用鼠标完成确认。<br>期望：可见的确认开关拥有有效的鼠标点击区域，且在右侧明确确认卡内易于发现。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/17-OperationDialogForm-Panel_PageStrongConfirm-world-scroll-bottom.png) · [证据2](geometry/17-OperationDialogForm-Panel_PageStrongConfirm-world-scroll-bottom.json) |
| UIA-0428 / P1 / 已确认 | **ACKNOWLEDGEMENT_WRONG_PANEL** · `强确认左侧目标卡与右侧明确确认卡`<br>确认开关实际放在左侧不可逆对象卡的滚动底部；右侧“明确确认”卡只显示占位说明。默认视口看不到开关，难以理解确认步骤。<br>期望：满足标准 1,2,8,10,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/17-OperationDialogForm-Panel_PageStrongConfirm-world-scroll-bottom.png) · [证据2](geometry/17-OperationDialogForm-Panel_PageStrongConfirm-world-scroll-bottom.json) |
| UIA-0429 / P1 / 已确认 | **DANGEROUS_ACTION_HIERARCHY** · `底部确认/取消/删除等按钮`<br>保留、取消、提交、删除或放弃进度采用近似同权重按钮，危险操作缺少足够醒目的差异提示，无法快速区分安全出口与执行动作。<br>期望：满足标准 8,9,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/17-OperationDialogForm-Panel_PageStrongConfirm.png) |
| UIA-0430 / P1 / 业务缺口 | **DIALOG_BUSINESS_PATH_MISSING** · `OperationDialogForm 非硬件确认页`<br>原生实现仅接入HardwareConfirm；本页没有对应请求消费、运行数据渲染和底部动作绑定。当前页面是声明布局，不能验证其业务成功/失败、确认/取消转换。<br>期望：缺少接入时明确不可用；真正业务流程需在批准范围内单独接入。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/17-OperationDialogForm-Panel_PageStrongConfirm-world.png) · [证据2](geometry/17-OperationDialogForm-Panel_PageStrongConfirm-world.json) |
| UIA-0431 / P1 / 已确认 | **POINTER_CENTER_BLOCKED** · `OperationDialogForm/Panel_Frame/Grp_PageHost/Panel_PageStrongConfirm/Panel_StrongConfirmTarget/List_StrongConfirmTarget/Viewport_StrongConfirmTarget/Content_StrongConfirmTarget/Panel_StrongConfirmControls/Tgl_StrongConfirmAcknowledged`<br>控件中心射线未命中自身；顶层=OperationDialogForm(Clone)/Panel_Frame/Grp_PageHost/Panel_PageStrongConfirm/Grp_FormalArtwork_StrongConfirm/Bg_FormalFrame_StrongConfirm<br>期望：满足标准 10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageStrongConfirm-scroll-bottom.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageStrongConfirm-world-scroll-bottom.json) |
| UIA-0432 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `OperationDialogForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageStrongConfirm-state-StrongConfirmDisabledState.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageStrongConfirm-state-StrongConfirmEmptyState.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageStrongConfirm-state-StrongConfirmErrorState.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageStrongConfirm-state-StrongConfirmLoadingState.json) |
| UIA-0433 / P1 / 已确认 | **TEXT_OVERLAP** · `OperationDialogForm/Panel_Frame/Grp_PageHost/Panel_PageStrongConfirm/Txt_StrongConfirmTitle ↔ OperationDialogForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageStrongConfirm-application.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageStrongConfirm-memory.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageStrongConfirm-scroll-bottom.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageStrongConfirm-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_StrongConfirmLoadingState](state-layout/17-OperationDialogForm-Panel_PageStrongConfirm-StrongConfirmLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_StrongConfirmEmptyState](state-layout/17-OperationDialogForm-Panel_PageStrongConfirm-StrongConfirmEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_StrongConfirmErrorState](state-layout/17-OperationDialogForm-Panel_PageStrongConfirm-StrongConfirmErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_StrongConfirmSuccessState](state-layout/17-OperationDialogForm-Panel_PageStrongConfirm-StrongConfirmSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_StrongConfirmDisabledState](state-layout/17-OperationDialogForm-Panel_PageStrongConfirm-StrongConfirmDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[danger](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-StrongConfirm-danger.png)、[hold](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-StrongConfirm-hold.png)、[stage2](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-StrongConfirm-stage2.png)、[danger](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 094 / 17 / 交易确认 · OperationDialogForm/Panel_PageTransactionConfirm

Prefab：[Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-TransactionConfirm-neutral.png)；证据：[application](runtime/17-OperationDialogForm-Panel_PageTransactionConfirm-application.png) · [memory](runtime/17-OperationDialogForm-Panel_PageTransactionConfirm-memory.png) · [world](runtime/17-OperationDialogForm-Panel_PageTransactionConfirm-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 发现问题 | UIA-0434 |
| 3. 尺寸与比例 | 发现问题 | UIA-0434 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0434, UIA-0437, UIA-0438 |
| 5. 九宫格与边角 | 发现问题 | UIA-0437 |
| 6. 对齐间距留白 | 发现问题 | UIA-0434, UIA-0437 |
| 7. 文字与可读性 | 发现问题 | UIA-0437, UIA-0438 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0435 |
| 9. 素材与状态语义 | 发现问题 | UIA-0435, UIA-0436, UIA-0437 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0435, UIA-0436 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0436, UIA-0437 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0434 / P1 / 已确认 | **CONFIRM_ART_OBSCURED** · `确认页左侧插画与对象信息卡`<br>左下对象信息卡覆盖插画底部，插画被明显截断，台座和对象没有完整展示空间。<br>期望：满足标准 2,3,4,6：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/17-OperationDialogForm-Panel_PageTransactionConfirm.png) |
| UIA-0435 / P1 / 已确认 | **DANGEROUS_ACTION_HIERARCHY** · `底部确认/取消/删除等按钮`<br>保留、取消、提交、删除或放弃进度采用近似同权重按钮，危险操作缺少足够醒目的差异提示，无法快速区分安全出口与执行动作。<br>期望：满足标准 8,9,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/17-OperationDialogForm-Panel_PageTransactionConfirm.png) |
| UIA-0436 / P1 / 业务缺口 | **DIALOG_BUSINESS_PATH_MISSING** · `OperationDialogForm 非硬件确认页`<br>原生实现仅接入HardwareConfirm；本页没有对应请求消费、运行数据渲染和底部动作绑定。当前页面是声明布局，不能验证其业务成功/失败、确认/取消转换。<br>期望：缺少接入时明确不可用；真正业务流程需在批准范围内单独接入。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/17-OperationDialogForm-Panel_PageTransactionConfirm-world.png) · [证据2](geometry/17-OperationDialogForm-Panel_PageTransactionConfirm-world.json) |
| UIA-0437 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `OperationDialogForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageTransactionConfirm-state-TransactionConfirmDisabledState.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageTransactionConfirm-state-TransactionConfirmEmptyState.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageTransactionConfirm-state-TransactionConfirmErrorState.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageTransactionConfirm-state-TransactionConfirmLoadingState.json) |
| UIA-0438 / P1 / 已确认 | **TEXT_OVERLAP** · `OperationDialogForm/Panel_Frame/Grp_PageHost/Panel_PageTransactionConfirm/Txt_TransactionConfirmTitle ↔ OperationDialogForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageTransactionConfirm-application.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageTransactionConfirm-memory.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageTransactionConfirm-scroll-bottom.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageTransactionConfirm-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_TransactionConfirmLoadingState](state-layout/17-OperationDialogForm-Panel_PageTransactionConfirm-TransactionConfirmLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_TransactionConfirmEmptyState](state-layout/17-OperationDialogForm-Panel_PageTransactionConfirm-TransactionConfirmEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_TransactionConfirmErrorState](state-layout/17-OperationDialogForm-Panel_PageTransactionConfirm-TransactionConfirmErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_TransactionConfirmSuccessState](state-layout/17-OperationDialogForm-Panel_PageTransactionConfirm-TransactionConfirmSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_TransactionConfirmDisabledState](state-layout/17-OperationDialogForm-Panel_PageTransactionConfirm-TransactionConfirmDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[neutral](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-TransactionConfirm-neutral.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 095 / 17 / 升级确认 · OperationDialogForm/Panel_PageUpgradeConfirm

Prefab：[Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/OperationDialogForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-UpgradeConfirm-danger.png)；证据：[application](runtime/17-OperationDialogForm-Panel_PageUpgradeConfirm-application.png) · [memory](runtime/17-OperationDialogForm-Panel_PageUpgradeConfirm-memory.png) · [world](runtime/17-OperationDialogForm-Panel_PageUpgradeConfirm-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 发现问题 | UIA-0439 |
| 3. 尺寸与比例 | 发现问题 | UIA-0439 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0439, UIA-0442, UIA-0443 |
| 5. 九宫格与边角 | 发现问题 | UIA-0442 |
| 6. 对齐间距留白 | 发现问题 | UIA-0439, UIA-0442 |
| 7. 文字与可读性 | 发现问题 | UIA-0442, UIA-0443 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0440 |
| 9. 素材与状态语义 | 发现问题 | UIA-0440, UIA-0441, UIA-0442 |
| 10. 点击命中与焦点 | 发现问题 | UIA-0440, UIA-0441 |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0441, UIA-0442 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0439 / P1 / 已确认 | **CONFIRM_ART_OBSCURED** · `确认页左侧插画与对象信息卡`<br>左下对象信息卡覆盖插画底部，插画被明显截断，台座和对象没有完整展示空间。<br>期望：满足标准 2,3,4,6：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/17-OperationDialogForm-Panel_PageUpgradeConfirm.png) |
| UIA-0440 / P1 / 已确认 | **DANGEROUS_ACTION_HIERARCHY** · `底部确认/取消/删除等按钮`<br>保留、取消、提交、删除或放弃进度采用近似同权重按钮，危险操作缺少足够醒目的差异提示，无法快速区分安全出口与执行动作。<br>期望：满足标准 8,9,10：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/17-OperationDialogForm-Panel_PageUpgradeConfirm.png) |
| UIA-0441 / P1 / 业务缺口 | **DIALOG_BUSINESS_PATH_MISSING** · `OperationDialogForm 非硬件确认页`<br>原生实现仅接入HardwareConfirm；本页没有对应请求消费、运行数据渲染和底部动作绑定。当前页面是声明布局，不能验证其业务成功/失败、确认/取消转换。<br>期望：缺少接入时明确不可用；真正业务流程需在批准范围内单独接入。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/17-OperationDialogForm-Panel_PageUpgradeConfirm-world.png) · [证据2](geometry/17-OperationDialogForm-Panel_PageUpgradeConfirm-world.json) |
| UIA-0442 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `OperationDialogForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageUpgradeConfirm-state-UpgradeConfirmDisabledState.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageUpgradeConfirm-state-UpgradeConfirmEmptyState.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageUpgradeConfirm-state-UpgradeConfirmErrorState.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageUpgradeConfirm-state-UpgradeConfirmLoadingState.json) |
| UIA-0443 / P1 / 已确认 | **TEXT_OVERLAP** · `OperationDialogForm/Panel_Frame/Grp_PageHost/Panel_PageUpgradeConfirm/Txt_UpgradeConfirmTitle ↔ OperationDialogForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/17-OperationDialogForm-Panel_PageUpgradeConfirm-application.json) · [证据2](geometry/17-OperationDialogForm-Panel_PageUpgradeConfirm-memory.json) · [证据3](geometry/17-OperationDialogForm-Panel_PageUpgradeConfirm-scroll-bottom.json) · [证据4](geometry/17-OperationDialogForm-Panel_PageUpgradeConfirm-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_UpgradeConfirmLoadingState](state-layout/17-OperationDialogForm-Panel_PageUpgradeConfirm-UpgradeConfirmLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_UpgradeConfirmEmptyState](state-layout/17-OperationDialogForm-Panel_PageUpgradeConfirm-UpgradeConfirmEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_UpgradeConfirmErrorState](state-layout/17-OperationDialogForm-Panel_PageUpgradeConfirm-UpgradeConfirmErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_UpgradeConfirmSuccessState](state-layout/17-OperationDialogForm-Panel_PageUpgradeConfirm-UpgradeConfirmSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_UpgradeConfirmDisabledState](state-layout/17-OperationDialogForm-Panel_PageUpgradeConfirm-UpgradeConfirmDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[danger](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-17-UpgradeConfirm-danger.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 096 / 18 / 成功拒绝与重试反馈 · OperationFeedbackForm/Panel_PageActionResult

Prefab：[Assets/Game/Prefabs/UI/Operations/OperationFeedbackForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/OperationFeedbackForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/OperationFeedbackForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/OperationFeedbackForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-ActionResult-success.png)；证据：[application](runtime/18-OperationFeedbackForm-Panel_PageActionResult-application.png) · [memory](runtime/18-OperationFeedbackForm-Panel_PageActionResult-memory.png) · [world](runtime/18-OperationFeedbackForm-Panel_PageActionResult-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 发现问题 | UIA-0444 |
| 3. 尺寸与比例 | 发现问题 | UIA-0444 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0444, UIA-0446, UIA-0447 |
| 5. 九宫格与边角 | 发现问题 | UIA-0446 |
| 6. 对齐间距留白 | 发现问题 | UIA-0446 |
| 7. 文字与可读性 | 发现问题 | UIA-0445, UIA-0446, UIA-0447 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0444, UIA-0445 |
| 9. 素材与状态语义 | 发现问题 | UIA-0445, UIA-0446 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0445, UIA-0446 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0444 / P1 / 已确认 | **REPORT_ART_OBSCURED** · `报告页左侧插画与摘要卡`<br>摘要卡盖住大型报告/工坊插画的下半部；插画、摘要和右侧报告条目的空间分配失衡。<br>期望：满足标准 2,3,4,8：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/18-OperationFeedbackForm-Panel_PageActionResult.png) |
| UIA-0445 / P1 / 业务缺口 | **REPORT_RUNTIME_NOT_BOUND** · `ProgressReportForm / OperationFeedbackForm`<br>实际打开后仍显示“对象/当前行为/成功恢复点/当前中枢等级”等占位字段。OnOperationPresentationChanged 为空，OnInit仅绑定返回/关闭；底部领域动作与进度、失败、成功结果尚未接入。本次无法验证业务结果转换。<br>期望：已有状态由真实快照填充；缺少业务接入时显示明确不可用原因，而非正常结果占位。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/18-OperationFeedbackForm-Panel_PageActionResult-world.png) · [证据2](geometry/18-OperationFeedbackForm-Panel_PageActionResult-world.json) |
| UIA-0446 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `OperationFeedbackForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/18-OperationFeedbackForm-Panel_PageActionResult-state-ActionResultDisabledState.json) · [证据2](geometry/18-OperationFeedbackForm-Panel_PageActionResult-state-ActionResultEmptyState.json) · [证据3](geometry/18-OperationFeedbackForm-Panel_PageActionResult-state-ActionResultErrorState.json) · [证据4](geometry/18-OperationFeedbackForm-Panel_PageActionResult-state-ActionResultLoadingState.json) |
| UIA-0447 / P1 / 已确认 | **TEXT_OVERLAP** · `OperationFeedbackForm/Panel_Frame/Grp_PageHost/Panel_PageActionResult/Txt_ActionResultTitle ↔ OperationFeedbackForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/18-OperationFeedbackForm-Panel_PageActionResult-application.json) · [证据2](geometry/18-OperationFeedbackForm-Panel_PageActionResult-memory.json) · [证据3](geometry/18-OperationFeedbackForm-Panel_PageActionResult-scroll-bottom.json) · [证据4](geometry/18-OperationFeedbackForm-Panel_PageActionResult-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_ActionResultLoadingState](state-layout/18-OperationFeedbackForm-Panel_PageActionResult-ActionResultLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ActionResultEmptyState](state-layout/18-OperationFeedbackForm-Panel_PageActionResult-ActionResultEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ActionResultErrorState](state-layout/18-OperationFeedbackForm-Panel_PageActionResult-ActionResultErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ActionResultSuccessState](state-layout/18-OperationFeedbackForm-Panel_PageActionResult-ActionResultSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ActionResultDisabledState](state-layout/18-OperationFeedbackForm-Panel_PageActionResult-ActionResultDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[success](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-ActionResult-success.png)、[failure](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-ActionResult-failure.png)、[partial](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-ActionResult-partial.png)、[pending](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-ActionResult-pending.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 097 / 18 / 安全停止等待 · OperationFeedbackForm/Panel_PageSafeWait

Prefab：[Assets/Game/Prefabs/UI/Operations/OperationFeedbackForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/OperationFeedbackForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/OperationFeedbackForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/OperationFeedbackForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-SafeWait-pending.png)；证据：[application](runtime/18-OperationFeedbackForm-Panel_PageSafeWait-application.png) · [memory](runtime/18-OperationFeedbackForm-Panel_PageSafeWait-memory.png) · [world](runtime/18-OperationFeedbackForm-Panel_PageSafeWait-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0450, UIA-0451 |
| 5. 九宫格与边角 | 发现问题 | UIA-0450 |
| 6. 对齐间距留白 | 发现问题 | UIA-0450 |
| 7. 文字与可读性 | 发现问题 | UIA-0449, UIA-0450, UIA-0451 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0448, UIA-0449 |
| 9. 素材与状态语义 | 发现问题 | UIA-0448, UIA-0449, UIA-0450 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0448, UIA-0449, UIA-0450 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0448 / P1 / 已确认 | **PROGRESS_PRESENTATION_MISSING** · `等待/恢复/加载页面主体`<br>布局只有普通说明卡，缺少清楚的进行中视觉焦点；需要区分确定进度与不确定等待，当前占位布局无法表达正在推进还是已停止。<br>期望：满足标准 8,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/18-OperationFeedbackForm-Panel_PageSafeWait.png) |
| UIA-0449 / P1 / 业务缺口 | **REPORT_RUNTIME_NOT_BOUND** · `ProgressReportForm / OperationFeedbackForm`<br>实际打开后仍显示“对象/当前行为/成功恢复点/当前中枢等级”等占位字段。OnOperationPresentationChanged 为空，OnInit仅绑定返回/关闭；底部领域动作与进度、失败、成功结果尚未接入。本次无法验证业务结果转换。<br>期望：已有状态由真实快照填充；缺少业务接入时显示明确不可用原因，而非正常结果占位。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/18-OperationFeedbackForm-Panel_PageSafeWait-world.png) · [证据2](geometry/18-OperationFeedbackForm-Panel_PageSafeWait-world.json) |
| UIA-0450 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `OperationFeedbackForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/18-OperationFeedbackForm-Panel_PageSafeWait-state-SafeWaitDisabledState.json) · [证据2](geometry/18-OperationFeedbackForm-Panel_PageSafeWait-state-SafeWaitEmptyState.json) · [证据3](geometry/18-OperationFeedbackForm-Panel_PageSafeWait-state-SafeWaitErrorState.json) · [证据4](geometry/18-OperationFeedbackForm-Panel_PageSafeWait-state-SafeWaitLoadingState.json) |
| UIA-0451 / P1 / 已确认 | **TEXT_OVERLAP** · `OperationFeedbackForm/Panel_Frame/Grp_PageHost/Panel_PageSafeWait/Txt_SafeWaitTitle ↔ OperationFeedbackForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/18-OperationFeedbackForm-Panel_PageSafeWait-application.json) · [证据2](geometry/18-OperationFeedbackForm-Panel_PageSafeWait-memory.json) · [证据3](geometry/18-OperationFeedbackForm-Panel_PageSafeWait-scroll-bottom.json) · [证据4](geometry/18-OperationFeedbackForm-Panel_PageSafeWait-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_SafeWaitLoadingState](state-layout/18-OperationFeedbackForm-Panel_PageSafeWait-SafeWaitLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_SafeWaitEmptyState](state-layout/18-OperationFeedbackForm-Panel_PageSafeWait-SafeWaitEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_SafeWaitErrorState](state-layout/18-OperationFeedbackForm-Panel_PageSafeWait-SafeWaitErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_SafeWaitSuccessState](state-layout/18-OperationFeedbackForm-Panel_PageSafeWait-SafeWaitSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_SafeWaitDisabledState](state-layout/18-OperationFeedbackForm-Panel_PageSafeWait-SafeWaitDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[pending](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-SafeWait-pending.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 098 / 18 / 备份恢复进度与结果 · ProgressReportForm/Panel_PageBackupProgress

Prefab：[Assets/Game/Prefabs/UI/Operations/ProgressReportForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/ProgressReportForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/ProgressReportForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/ProgressReportForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-BackupProgress-loading.png)；证据：[application](runtime/18-ProgressReportForm-Panel_PageBackupProgress-application.png) · [memory](runtime/18-ProgressReportForm-Panel_PageBackupProgress-memory.png) · [world](runtime/18-ProgressReportForm-Panel_PageBackupProgress-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0454, UIA-0455 |
| 5. 九宫格与边角 | 发现问题 | UIA-0454 |
| 6. 对齐间距留白 | 发现问题 | UIA-0454 |
| 7. 文字与可读性 | 发现问题 | UIA-0453, UIA-0454, UIA-0455 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0452, UIA-0453 |
| 9. 素材与状态语义 | 发现问题 | UIA-0452, UIA-0453, UIA-0454 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0452, UIA-0453, UIA-0454 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0452 / P1 / 已确认 | **PROGRESS_PRESENTATION_MISSING** · `等待/恢复/加载页面主体`<br>布局只有普通说明卡，缺少清楚的进行中视觉焦点；需要区分确定进度与不确定等待，当前占位布局无法表达正在推进还是已停止。<br>期望：满足标准 8,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/18-ProgressReportForm-Panel_PageBackupProgress.png) |
| UIA-0453 / P1 / 业务缺口 | **REPORT_RUNTIME_NOT_BOUND** · `ProgressReportForm / OperationFeedbackForm`<br>实际打开后仍显示“对象/当前行为/成功恢复点/当前中枢等级”等占位字段。OnOperationPresentationChanged 为空，OnInit仅绑定返回/关闭；底部领域动作与进度、失败、成功结果尚未接入。本次无法验证业务结果转换。<br>期望：已有状态由真实快照填充；缺少业务接入时显示明确不可用原因，而非正常结果占位。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/18-ProgressReportForm-Panel_PageBackupProgress-world.png) · [证据2](geometry/18-ProgressReportForm-Panel_PageBackupProgress-world.json) |
| UIA-0454 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `ProgressReportForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/18-ProgressReportForm-Panel_PageBackupProgress-state-BackupProgressDisabledState.json) · [证据2](geometry/18-ProgressReportForm-Panel_PageBackupProgress-state-BackupProgressEmptyState.json) · [证据3](geometry/18-ProgressReportForm-Panel_PageBackupProgress-state-BackupProgressErrorState.json) · [证据4](geometry/18-ProgressReportForm-Panel_PageBackupProgress-state-BackupProgressLoadingState.json) |
| UIA-0455 / P1 / 已确认 | **TEXT_OVERLAP** · `ProgressReportForm/Panel_Frame/Grp_PageHost/Panel_PageBackupProgress/Txt_BackupProgressTitle ↔ ProgressReportForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/18-ProgressReportForm-Panel_PageBackupProgress-application.json) · [证据2](geometry/18-ProgressReportForm-Panel_PageBackupProgress-memory.json) · [证据3](geometry/18-ProgressReportForm-Panel_PageBackupProgress-scroll-bottom.json) · [证据4](geometry/18-ProgressReportForm-Panel_PageBackupProgress-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_BackupProgressLoadingState](state-layout/18-ProgressReportForm-Panel_PageBackupProgress-BackupProgressLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_BackupProgressEmptyState](state-layout/18-ProgressReportForm-Panel_PageBackupProgress-BackupProgressEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_BackupProgressErrorState](state-layout/18-ProgressReportForm-Panel_PageBackupProgress-BackupProgressErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_BackupProgressSuccessState](state-layout/18-ProgressReportForm-Panel_PageBackupProgress-BackupProgressSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_BackupProgressDisabledState](state-layout/18-ProgressReportForm-Panel_PageBackupProgress-BackupProgressDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[loading](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-BackupProgress-loading.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 099 / 18 / 星拓联日常补给 · ProgressReportForm/Panel_PageDailySupply

Prefab：[Assets/Game/Prefabs/UI/Operations/ProgressReportForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/ProgressReportForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/ProgressReportForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/ProgressReportForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-DailySupply-success.png)；证据：[application](runtime/18-ProgressReportForm-Panel_PageDailySupply-application.png) · [memory](runtime/18-ProgressReportForm-Panel_PageDailySupply-memory.png) · [world](runtime/18-ProgressReportForm-Panel_PageDailySupply-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 发现问题 | UIA-0456 |
| 3. 尺寸与比例 | 发现问题 | UIA-0456 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0456, UIA-0458, UIA-0459 |
| 5. 九宫格与边角 | 发现问题 | UIA-0458 |
| 6. 对齐间距留白 | 发现问题 | UIA-0458 |
| 7. 文字与可读性 | 发现问题 | UIA-0457, UIA-0458, UIA-0459 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0456, UIA-0457 |
| 9. 素材与状态语义 | 发现问题 | UIA-0457, UIA-0458 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0457, UIA-0458 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0456 / P1 / 已确认 | **REPORT_ART_OBSCURED** · `报告页左侧插画与摘要卡`<br>摘要卡盖住大型报告/工坊插画的下半部；插画、摘要和右侧报告条目的空间分配失衡。<br>期望：满足标准 2,3,4,8：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/18-ProgressReportForm-Panel_PageDailySupply.png) |
| UIA-0457 / P1 / 业务缺口 | **REPORT_RUNTIME_NOT_BOUND** · `ProgressReportForm / OperationFeedbackForm`<br>实际打开后仍显示“对象/当前行为/成功恢复点/当前中枢等级”等占位字段。OnOperationPresentationChanged 为空，OnInit仅绑定返回/关闭；底部领域动作与进度、失败、成功结果尚未接入。本次无法验证业务结果转换。<br>期望：已有状态由真实快照填充；缺少业务接入时显示明确不可用原因，而非正常结果占位。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/18-ProgressReportForm-Panel_PageDailySupply-world.png) · [证据2](geometry/18-ProgressReportForm-Panel_PageDailySupply-world.json) |
| UIA-0458 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `ProgressReportForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/18-ProgressReportForm-Panel_PageDailySupply-state-DailySupplyDisabledState.json) · [证据2](geometry/18-ProgressReportForm-Panel_PageDailySupply-state-DailySupplyEmptyState.json) · [证据3](geometry/18-ProgressReportForm-Panel_PageDailySupply-state-DailySupplyErrorState.json) · [证据4](geometry/18-ProgressReportForm-Panel_PageDailySupply-state-DailySupplyLoadingState.json) |
| UIA-0459 / P1 / 已确认 | **TEXT_OVERLAP** · `ProgressReportForm/Panel_Frame/Grp_PageHost/Panel_PageDailySupply/Txt_DailySupplyTitle ↔ ProgressReportForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/18-ProgressReportForm-Panel_PageDailySupply-application.json) · [证据2](geometry/18-ProgressReportForm-Panel_PageDailySupply-memory.json) · [证据3](geometry/18-ProgressReportForm-Panel_PageDailySupply-scroll-bottom.json) · [证据4](geometry/18-ProgressReportForm-Panel_PageDailySupply-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_DailySupplyLoadingState](state-layout/18-ProgressReportForm-Panel_PageDailySupply-DailySupplyLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_DailySupplyEmptyState](state-layout/18-ProgressReportForm-Panel_PageDailySupply-DailySupplyEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_DailySupplyErrorState](state-layout/18-ProgressReportForm-Panel_PageDailySupply-DailySupplyErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_DailySupplySuccessState](state-layout/18-ProgressReportForm-Panel_PageDailySupply-DailySupplySuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_DailySupplyDisabledState](state-layout/18-ProgressReportForm-Panel_PageDailySupply-DailySupplyDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[success](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-DailySupply-success.png)、[empty](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-DailySupply-empty.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 100 / 18 / 合并交付结果 · ProgressReportForm/Panel_PageDelivery

Prefab：[Assets/Game/Prefabs/UI/Operations/ProgressReportForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/ProgressReportForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/ProgressReportForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/ProgressReportForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-Delivery-pending.png)；证据：[application](runtime/18-ProgressReportForm-Panel_PageDelivery-application.png) · [memory](runtime/18-ProgressReportForm-Panel_PageDelivery-memory.png) · [world](runtime/18-ProgressReportForm-Panel_PageDelivery-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 发现问题 | UIA-0460 |
| 3. 尺寸与比例 | 发现问题 | UIA-0460 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0460, UIA-0462, UIA-0463 |
| 5. 九宫格与边角 | 发现问题 | UIA-0462 |
| 6. 对齐间距留白 | 发现问题 | UIA-0462 |
| 7. 文字与可读性 | 发现问题 | UIA-0461, UIA-0462, UIA-0463 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0460, UIA-0461 |
| 9. 素材与状态语义 | 发现问题 | UIA-0461, UIA-0462 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0461, UIA-0462 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0460 / P1 / 已确认 | **REPORT_ART_OBSCURED** · `报告页左侧插画与摘要卡`<br>摘要卡盖住大型报告/工坊插画的下半部；插画、摘要和右侧报告条目的空间分配失衡。<br>期望：满足标准 2,3,4,8：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/18-ProgressReportForm-Panel_PageDelivery.png) |
| UIA-0461 / P1 / 业务缺口 | **REPORT_RUNTIME_NOT_BOUND** · `ProgressReportForm / OperationFeedbackForm`<br>实际打开后仍显示“对象/当前行为/成功恢复点/当前中枢等级”等占位字段。OnOperationPresentationChanged 为空，OnInit仅绑定返回/关闭；底部领域动作与进度、失败、成功结果尚未接入。本次无法验证业务结果转换。<br>期望：已有状态由真实快照填充；缺少业务接入时显示明确不可用原因，而非正常结果占位。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/18-ProgressReportForm-Panel_PageDelivery-world.png) · [证据2](geometry/18-ProgressReportForm-Panel_PageDelivery-world.json) |
| UIA-0462 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `ProgressReportForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/18-ProgressReportForm-Panel_PageDelivery-state-DeliveryDisabledState.json) · [证据2](geometry/18-ProgressReportForm-Panel_PageDelivery-state-DeliveryEmptyState.json) · [证据3](geometry/18-ProgressReportForm-Panel_PageDelivery-state-DeliveryErrorState.json) · [证据4](geometry/18-ProgressReportForm-Panel_PageDelivery-state-DeliveryLoadingState.json) |
| UIA-0463 / P1 / 已确认 | **TEXT_OVERLAP** · `ProgressReportForm/Panel_Frame/Grp_PageHost/Panel_PageDelivery/Txt_DeliveryTitle ↔ ProgressReportForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/18-ProgressReportForm-Panel_PageDelivery-application.json) · [证据2](geometry/18-ProgressReportForm-Panel_PageDelivery-memory.json) · [证据3](geometry/18-ProgressReportForm-Panel_PageDelivery-scroll-bottom.json) · [证据4](geometry/18-ProgressReportForm-Panel_PageDelivery-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_DeliveryLoadingState](state-layout/18-ProgressReportForm-Panel_PageDelivery-DeliveryLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_DeliveryEmptyState](state-layout/18-ProgressReportForm-Panel_PageDelivery-DeliveryEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_DeliveryErrorState](state-layout/18-ProgressReportForm-Panel_PageDelivery-DeliveryErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_DeliverySuccessState](state-layout/18-ProgressReportForm-Panel_PageDelivery-DeliverySuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_DeliveryDisabledState](state-layout/18-ProgressReportForm-Panel_PageDelivery-DeliveryDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[pending](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-Delivery-pending.png)、[success](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-Delivery-success.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 101 / 18 / 成长与功能解锁 · ProgressReportForm/Panel_PageGrowth

Prefab：[Assets/Game/Prefabs/UI/Operations/ProgressReportForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/ProgressReportForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/ProgressReportForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/ProgressReportForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-Growth-pending.png)；证据：[application](runtime/18-ProgressReportForm-Panel_PageGrowth-application.png) · [memory](runtime/18-ProgressReportForm-Panel_PageGrowth-memory.png) · [world](runtime/18-ProgressReportForm-Panel_PageGrowth-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 发现问题 | UIA-0464 |
| 3. 尺寸与比例 | 发现问题 | UIA-0464 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0464, UIA-0466, UIA-0467 |
| 5. 九宫格与边角 | 发现问题 | UIA-0466 |
| 6. 对齐间距留白 | 发现问题 | UIA-0466 |
| 7. 文字与可读性 | 发现问题 | UIA-0465, UIA-0466, UIA-0467 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0464, UIA-0465 |
| 9. 素材与状态语义 | 发现问题 | UIA-0465, UIA-0466 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0465, UIA-0466 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0464 / P1 / 已确认 | **REPORT_ART_OBSCURED** · `报告页左侧插画与摘要卡`<br>摘要卡盖住大型报告/工坊插画的下半部；插画、摘要和右侧报告条目的空间分配失衡。<br>期望：满足标准 2,3,4,8：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/18-ProgressReportForm-Panel_PageGrowth.png) |
| UIA-0465 / P1 / 业务缺口 | **REPORT_RUNTIME_NOT_BOUND** · `ProgressReportForm / OperationFeedbackForm`<br>实际打开后仍显示“对象/当前行为/成功恢复点/当前中枢等级”等占位字段。OnOperationPresentationChanged 为空，OnInit仅绑定返回/关闭；底部领域动作与进度、失败、成功结果尚未接入。本次无法验证业务结果转换。<br>期望：已有状态由真实快照填充；缺少业务接入时显示明确不可用原因，而非正常结果占位。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/18-ProgressReportForm-Panel_PageGrowth-world.png) · [证据2](geometry/18-ProgressReportForm-Panel_PageGrowth-world.json) |
| UIA-0466 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `ProgressReportForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/18-ProgressReportForm-Panel_PageGrowth-state-GrowthDisabledState.json) · [证据2](geometry/18-ProgressReportForm-Panel_PageGrowth-state-GrowthEmptyState.json) · [证据3](geometry/18-ProgressReportForm-Panel_PageGrowth-state-GrowthErrorState.json) · [证据4](geometry/18-ProgressReportForm-Panel_PageGrowth-state-GrowthLoadingState.json) |
| UIA-0467 / P1 / 已确认 | **TEXT_OVERLAP** · `ProgressReportForm/Panel_Frame/Grp_PageHost/Panel_PageGrowth/Txt_GrowthTitle ↔ ProgressReportForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/18-ProgressReportForm-Panel_PageGrowth-application.json) · [证据2](geometry/18-ProgressReportForm-Panel_PageGrowth-memory.json) · [证据3](geometry/18-ProgressReportForm-Panel_PageGrowth-scroll-bottom.json) · [证据4](geometry/18-ProgressReportForm-Panel_PageGrowth-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_GrowthLoadingState](state-layout/18-ProgressReportForm-Panel_PageGrowth-GrowthLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_GrowthEmptyState](state-layout/18-ProgressReportForm-Panel_PageGrowth-GrowthEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_GrowthErrorState](state-layout/18-ProgressReportForm-Panel_PageGrowth-GrowthErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_GrowthSuccessState](state-layout/18-ProgressReportForm-Panel_PageGrowth-GrowthSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_GrowthDisabledState](state-layout/18-ProgressReportForm-Panel_PageGrowth-GrowthDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[pending](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-Growth-pending.png)、[success](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-Growth-success.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 102 / 18 / 加载 · ProgressReportForm/Panel_PageLoading

Prefab：[Assets/Game/Prefabs/UI/Operations/ProgressReportForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/ProgressReportForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/ProgressReportForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/ProgressReportForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-Loading-loading.png)；证据：[application](runtime/18-ProgressReportForm-Panel_PageLoading-application.png) · [memory](runtime/18-ProgressReportForm-Panel_PageLoading-memory.png) · [world](runtime/18-ProgressReportForm-Panel_PageLoading-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0470, UIA-0471 |
| 5. 九宫格与边角 | 发现问题 | UIA-0470 |
| 6. 对齐间距留白 | 发现问题 | UIA-0470 |
| 7. 文字与可读性 | 发现问题 | UIA-0469, UIA-0470, UIA-0471 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0468, UIA-0469 |
| 9. 素材与状态语义 | 发现问题 | UIA-0468, UIA-0469, UIA-0470 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0468, UIA-0469, UIA-0470 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0468 / P1 / 已确认 | **PROGRESS_PRESENTATION_MISSING** · `等待/恢复/加载页面主体`<br>布局只有普通说明卡，缺少清楚的进行中视觉焦点；需要区分确定进度与不确定等待，当前占位布局无法表达正在推进还是已停止。<br>期望：满足标准 8,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/18-ProgressReportForm-Panel_PageLoading.png) |
| UIA-0469 / P1 / 业务缺口 | **REPORT_RUNTIME_NOT_BOUND** · `ProgressReportForm / OperationFeedbackForm`<br>实际打开后仍显示“对象/当前行为/成功恢复点/当前中枢等级”等占位字段。OnOperationPresentationChanged 为空，OnInit仅绑定返回/关闭；底部领域动作与进度、失败、成功结果尚未接入。本次无法验证业务结果转换。<br>期望：已有状态由真实快照填充；缺少业务接入时显示明确不可用原因，而非正常结果占位。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/18-ProgressReportForm-Panel_PageLoading-world.png) · [证据2](geometry/18-ProgressReportForm-Panel_PageLoading-world.json) |
| UIA-0470 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `ProgressReportForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/18-ProgressReportForm-Panel_PageLoading-state-LoadingDisabledState.json) · [证据2](geometry/18-ProgressReportForm-Panel_PageLoading-state-LoadingEmptyState.json) · [证据3](geometry/18-ProgressReportForm-Panel_PageLoading-state-LoadingErrorState.json) · [证据4](geometry/18-ProgressReportForm-Panel_PageLoading-state-LoadingLoadingState.json) |
| UIA-0471 / P1 / 已确认 | **TEXT_OVERLAP** · `ProgressReportForm/Panel_Frame/Grp_PageHost/Panel_PageLoading/Txt_LoadingTitle ↔ ProgressReportForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/18-ProgressReportForm-Panel_PageLoading-application.json) · [证据2](geometry/18-ProgressReportForm-Panel_PageLoading-memory.json) · [证据3](geometry/18-ProgressReportForm-Panel_PageLoading-scroll-bottom.json) · [证据4](geometry/18-ProgressReportForm-Panel_PageLoading-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_LoadingLoadingState](state-layout/18-ProgressReportForm-Panel_PageLoading-LoadingLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_LoadingEmptyState](state-layout/18-ProgressReportForm-Panel_PageLoading-LoadingEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_LoadingErrorState](state-layout/18-ProgressReportForm-Panel_PageLoading-LoadingErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_LoadingSuccessState](state-layout/18-ProgressReportForm-Panel_PageLoading-LoadingSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_LoadingDisabledState](state-layout/18-ProgressReportForm-Panel_PageLoading-LoadingDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[loading](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-Loading-loading.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 103 / 18 / 离线结算 · ProgressReportForm/Panel_PageOfflineSettlement

Prefab：[Assets/Game/Prefabs/UI/Operations/ProgressReportForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/ProgressReportForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/ProgressReportForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/ProgressReportForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-OfflineSettlement-success.png)；证据：[application](runtime/18-ProgressReportForm-Panel_PageOfflineSettlement-application.png) · [memory](runtime/18-ProgressReportForm-Panel_PageOfflineSettlement-memory.png) · [world](runtime/18-ProgressReportForm-Panel_PageOfflineSettlement-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 发现问题 | UIA-0472 |
| 3. 尺寸与比例 | 发现问题 | UIA-0472 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0472, UIA-0474, UIA-0475 |
| 5. 九宫格与边角 | 发现问题 | UIA-0474 |
| 6. 对齐间距留白 | 发现问题 | UIA-0474 |
| 7. 文字与可读性 | 发现问题 | UIA-0473, UIA-0474, UIA-0475 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0472, UIA-0473 |
| 9. 素材与状态语义 | 发现问题 | UIA-0473, UIA-0474 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0473, UIA-0474 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0472 / P1 / 已确认 | **REPORT_ART_OBSCURED** · `报告页左侧插画与摘要卡`<br>摘要卡盖住大型报告/工坊插画的下半部；插画、摘要和右侧报告条目的空间分配失衡。<br>期望：满足标准 2,3,4,8：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/18-ProgressReportForm-Panel_PageOfflineSettlement.png) |
| UIA-0473 / P1 / 业务缺口 | **REPORT_RUNTIME_NOT_BOUND** · `ProgressReportForm / OperationFeedbackForm`<br>实际打开后仍显示“对象/当前行为/成功恢复点/当前中枢等级”等占位字段。OnOperationPresentationChanged 为空，OnInit仅绑定返回/关闭；底部领域动作与进度、失败、成功结果尚未接入。本次无法验证业务结果转换。<br>期望：已有状态由真实快照填充；缺少业务接入时显示明确不可用原因，而非正常结果占位。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/18-ProgressReportForm-Panel_PageOfflineSettlement-world.png) · [证据2](geometry/18-ProgressReportForm-Panel_PageOfflineSettlement-world.json) |
| UIA-0474 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `ProgressReportForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/18-ProgressReportForm-Panel_PageOfflineSettlement-state-OfflineSettlementDisabledState.json) · [证据2](geometry/18-ProgressReportForm-Panel_PageOfflineSettlement-state-OfflineSettlementEmptyState.json) · [证据3](geometry/18-ProgressReportForm-Panel_PageOfflineSettlement-state-OfflineSettlementErrorState.json) · [证据4](geometry/18-ProgressReportForm-Panel_PageOfflineSettlement-state-OfflineSettlementLoadingState.json) |
| UIA-0475 / P1 / 已确认 | **TEXT_OVERLAP** · `ProgressReportForm/Panel_Frame/Grp_PageHost/Panel_PageOfflineSettlement/Txt_OfflineSettlementTitle ↔ ProgressReportForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/18-ProgressReportForm-Panel_PageOfflineSettlement-application.json) · [证据2](geometry/18-ProgressReportForm-Panel_PageOfflineSettlement-memory.json) · [证据3](geometry/18-ProgressReportForm-Panel_PageOfflineSettlement-scroll-bottom.json) · [证据4](geometry/18-ProgressReportForm-Panel_PageOfflineSettlement-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_OfflineSettlementLoadingState](state-layout/18-ProgressReportForm-Panel_PageOfflineSettlement-OfflineSettlementLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_OfflineSettlementEmptyState](state-layout/18-ProgressReportForm-Panel_PageOfflineSettlement-OfflineSettlementEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_OfflineSettlementErrorState](state-layout/18-ProgressReportForm-Panel_PageOfflineSettlement-OfflineSettlementErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_OfflineSettlementSuccessState](state-layout/18-ProgressReportForm-Panel_PageOfflineSettlement-OfflineSettlementSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_OfflineSettlementDisabledState](state-layout/18-ProgressReportForm-Panel_PageOfflineSettlement-OfflineSettlementDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[success](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-OfflineSettlement-success.png)、[partial](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-OfflineSettlement-partial.png)、[failure](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-OfflineSettlement-failure.png)、[empty](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-OfflineSettlement-empty.png)、[success](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 104 / 18 / 回归报告 · ProgressReportForm/Panel_PageReturnReport

Prefab：[Assets/Game/Prefabs/UI/Operations/ProgressReportForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/ProgressReportForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/ProgressReportForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/ProgressReportForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-ReturnReport-success.png)；证据：[application](runtime/18-ProgressReportForm-Panel_PageReturnReport-application.png) · [memory](runtime/18-ProgressReportForm-Panel_PageReturnReport-memory.png) · [world](runtime/18-ProgressReportForm-Panel_PageReturnReport-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 发现问题 | UIA-0476 |
| 3. 尺寸与比例 | 发现问题 | UIA-0476 |
| 4. 遮挡与裁切 | 发现问题 | UIA-0476, UIA-0478, UIA-0479 |
| 5. 九宫格与边角 | 发现问题 | UIA-0478 |
| 6. 对齐间距留白 | 发现问题 | UIA-0478 |
| 7. 文字与可读性 | 发现问题 | UIA-0477, UIA-0478, UIA-0479 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0476, UIA-0477 |
| 9. 素材与状态语义 | 发现问题 | UIA-0477, UIA-0478 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0477, UIA-0478 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0476 / P1 / 已确认 | **REPORT_ART_OBSCURED** · `报告页左侧插画与摘要卡`<br>摘要卡盖住大型报告/工坊插画的下半部；插画、摘要和右侧报告条目的空间分配失衡。<br>期望：满足标准 2,3,4,8：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](../previews/18-ProgressReportForm-Panel_PageReturnReport.png) |
| UIA-0477 / P1 / 业务缺口 | **REPORT_RUNTIME_NOT_BOUND** · `ProgressReportForm / OperationFeedbackForm`<br>实际打开后仍显示“对象/当前行为/成功恢复点/当前中枢等级”等占位字段。OnOperationPresentationChanged 为空，OnInit仅绑定返回/关闭；底部领域动作与进度、失败、成功结果尚未接入。本次无法验证业务结果转换。<br>期望：已有状态由真实快照填充；缺少业务接入时显示明确不可用原因，而非正常结果占位。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/18-ProgressReportForm-Panel_PageReturnReport-world.png) · [证据2](geometry/18-ProgressReportForm-Panel_PageReturnReport-world.json) |
| UIA-0478 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `ProgressReportForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/18-ProgressReportForm-Panel_PageReturnReport-state-ReturnReportDisabledState.json) · [证据2](geometry/18-ProgressReportForm-Panel_PageReturnReport-state-ReturnReportEmptyState.json) · [证据3](geometry/18-ProgressReportForm-Panel_PageReturnReport-state-ReturnReportErrorState.json) · [证据4](geometry/18-ProgressReportForm-Panel_PageReturnReport-state-ReturnReportLoadingState.json) |
| UIA-0479 / P1 / 已确认 | **TEXT_OVERLAP** · `ProgressReportForm/Panel_Frame/Grp_PageHost/Panel_PageReturnReport/Txt_ReturnReportTitle ↔ ProgressReportForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/18-ProgressReportForm-Panel_PageReturnReport-application.json) · [证据2](geometry/18-ProgressReportForm-Panel_PageReturnReport-memory.json) · [证据3](geometry/18-ProgressReportForm-Panel_PageReturnReport-scroll-bottom.json) · [证据4](geometry/18-ProgressReportForm-Panel_PageReturnReport-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_ReturnReportLoadingState](state-layout/18-ProgressReportForm-Panel_PageReturnReport-ReturnReportLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ReturnReportEmptyState](state-layout/18-ProgressReportForm-Panel_PageReturnReport-ReturnReportEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ReturnReportErrorState](state-layout/18-ProgressReportForm-Panel_PageReturnReport-ReturnReportErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ReturnReportSuccessState](state-layout/18-ProgressReportForm-Panel_PageReturnReport-ReturnReportSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_ReturnReportDisabledState](state-layout/18-ProgressReportForm-Panel_PageReturnReport-ReturnReportDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[success](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-ReturnReport-success.png)、[partial](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-ReturnReport-partial.png)、[failure](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-ReturnReport-failure.png)、[empty](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-ReturnReport-empty.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

### 105 / 18 / 第一版完成报告 · ProgressReportForm/Panel_PageVersionComplete

Prefab：[Assets/Game/Prefabs/UI/Operations/ProgressReportForm.prefab](../../../../../Assets/Game/Prefabs/UI/Operations/ProgressReportForm.prefab)。结构合同：[Docs/Development/UI-PrefabLayouts/ProgressReportForm.contract.json](../../../../../Docs/Development/UI-PrefabLayouts/ProgressReportForm.contract.json)。
参考：[效果图](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-VersionComplete-success.png)；证据：[application](runtime/18-ProgressReportForm-Panel_PageVersionComplete-application.png) · [memory](runtime/18-ProgressReportForm-Panel_PageVersionComplete-memory.png) · [world](runtime/18-ProgressReportForm-Panel_PageVersionComplete-world.png)。

| 标准 | 结果 | 对应问题 |
|---|---|---|
| 1. 布局与结构 | 当前样本未发现新增问题 |  |
| 2. 图片实际可见位置 | 当前样本未发现新增问题 |  |
| 3. 尺寸与比例 | 当前样本未发现新增问题 |  |
| 4. 遮挡与裁切 | 发现问题 | UIA-0481, UIA-0482 |
| 5. 九宫格与边角 | 发现问题 | UIA-0481 |
| 6. 对齐间距留白 | 发现问题 | UIA-0481 |
| 7. 文字与可读性 | 发现问题 | UIA-0480, UIA-0481, UIA-0482 |
| 8. 信息层级与主操作 | 发现问题 | UIA-0480 |
| 9. 素材与状态语义 | 发现问题 | UIA-0480, UIA-0481 |
| 10. 点击命中与焦点 | 当前样本未发现新增问题 |  |
| 11. 动态内容与运行状态 | 发现问题 | UIA-0480, UIA-0481 |
| 12. 同类页面一致性 | 当前样本未发现新增问题 |  |

| 编号／级别／性质 | 位置与现象 | 复现与证据 |
|---|---|---|
| UIA-0480 / P1 / 业务缺口 | **REPORT_RUNTIME_NOT_BOUND** · `ProgressReportForm / OperationFeedbackForm`<br>实际打开后仍显示“对象/当前行为/成功恢复点/当前中枢等级”等占位字段。OnOperationPresentationChanged 为空，OnInit仅绑定返回/关闭；底部领域动作与进度、失败、成功结果尚未接入。本次无法验证业务结果转换。<br>期望：已有状态由真实快照填充；缺少业务接入时显示明确不可用原因，而非正常结果占位。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](runtime/18-ProgressReportForm-Panel_PageVersionComplete-world.png) · [证据2](geometry/18-ProgressReportForm-Panel_PageVersionComplete-world.json) |
| UIA-0481 / P1 / 已确认 | **STATE_TEXT_ON_CHROME** · `ProgressReportForm/Panel_Frame/Grp_*State`<br>状态卡文字容器仅约12px内缩，未进入 M006 规定的40px四周安全区；五种声明状态均复用此布局<br>期望：满足标准 4,5,6,7,9,11：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/18-ProgressReportForm-Panel_PageVersionComplete-state-VersionCompleteDisabledState.json) · [证据2](geometry/18-ProgressReportForm-Panel_PageVersionComplete-state-VersionCompleteEmptyState.json) · [证据3](geometry/18-ProgressReportForm-Panel_PageVersionComplete-state-VersionCompleteErrorState.json) · [证据4](geometry/18-ProgressReportForm-Panel_PageVersionComplete-state-VersionCompleteLoadingState.json) |
| UIA-0482 / P1 / 已确认 | **TEXT_OVERLAP** · `ProgressReportForm/Panel_Frame/Grp_PageHost/Panel_PageVersionComplete/Txt_VersionCompleteTitle ↔ ProgressReportForm/Panel_Frame/Txt_FormTitle`<br>两个可见文字轮廓相交<br>期望：满足标准 4,7：相关元素可完整阅读/操作，布局与文档分区一致，图形保持正确语义。 | 在1920×1080下打开此页，按证据文件所示上下文和滚动位置观察；状态证据通过预览副本单独激活声明组。<br>[证据1](geometry/18-ProgressReportForm-Panel_PageVersionComplete-application.json) · [证据2](geometry/18-ProgressReportForm-Panel_PageVersionComplete-memory.json) · [证据3](geometry/18-ProgressReportForm-Panel_PageVersionComplete-scroll-bottom.json) · [证据4](geometry/18-ProgressReportForm-Panel_PageVersionComplete-scroll-top.json) |

| 声明运行状态 | 布局检查 | 实际出现的上下文 |
|---|---|---|
| [Grp_VersionCompleteLoadingState](state-layout/18-ProgressReportForm-Panel_PageVersionComplete-VersionCompleteLoadingState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_VersionCompleteEmptyState](state-layout/18-ProgressReportForm-Panel_PageVersionComplete-VersionCompleteEmptyState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_VersionCompleteErrorState](state-layout/18-ProgressReportForm-Panel_PageVersionComplete-VersionCompleteErrorState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_VersionCompleteSuccessState](state-layout/18-ProgressReportForm-Panel_PageVersionComplete-VersionCompleteSuccessState.png) | 已检查 | **未由真实业务触发验证** |
| [Grp_VersionCompleteDisabledState](state-layout/18-ProgressReportForm-Panel_PageVersionComplete-VersionCompleteDisabledState.png) | 已检查 | **未由真实业务触发验证** |

效果图要求的代表态：[success](../../../../../Docs/GameDesign/11-美术规范/界面设计文档/异星拓荒_UI效果图_20261014/EffectBoards/mock-ui-18-VersionComplete-success.png)。这些代表态若未在上表实际出现，仍保留业务转换验证缺口。

## 算法Item与无可视宿主

- AlgorithmNodeItem：普通、选中、失败样式与4入4出长名称压力布局均使用正式素材；发现标题/端口相交，见算法工作台问题。[八态对照](sheets/items.jpg)。
- AlgorithmEdgeItem：正常、选中、悬停样式可见，曲线端点随原生几何生成；本次样本未见断线。多节点密集交叉、缩放下点击与运行链路仍未覆盖。八态图片中的error列实际为边悬停，long列为同几何对照，边没有错误/文字状态。
- FieldHudForm：无可视路由宿主；真实区域中已检查其自然选择侧栏路径。不是可单独作图像验收的页面。

## 修复顺序与回归要求

1. 共用控件与状态提示：安全区、九宫格最小尺寸、射线目标、标题去重、默认焦点。
2. 01→18逐页修复：保留结构/资源身份，修正图片与内容分区、视口高度、动态条目和动作区，禁止只把问题隐藏。
3. 每页在原缺陷状态重拍并对照；共性修改回归全部受影响页。候选先复现再修改，业务缺口另行明确范围。
4. 最后统一运行现有UI回归、编译/引用/九宫格检查，补齐可触发业务状态。未覆盖项不能勾为通过。

## 文件保护检查

检查前后的1746个保护文件哈希一致；UI Prefab、UI Sprite、AutoEra脚本、字体与布局合同无新增改动。没有Git暂存或提交。本轮新增内容仅为审计证据/报告与OpenSpec检查记录。
