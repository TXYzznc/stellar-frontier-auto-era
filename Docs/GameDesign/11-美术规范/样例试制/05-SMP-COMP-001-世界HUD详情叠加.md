# SMP-COMP-001 · 世界 HUD 详情叠加效果图

## 1. 目的与组合合同

这是本批最后生成的组合效果图，用来判断单件素材放进实际页面后是否仍然可读。画面必须保留世界场景可见面积，并明确 FieldHudDetailForm 的详情层高于 FieldHudResidentForm；层级关系以视觉遮挡和焦点边界表达，不在图中写对象名称。

## 2. 生成规格

| 项目 | 要求 |
|---|---|
| 输出 | NEW_smp_comp_001_world_hud_detail@1920x1080.png |
| 视图 | 1920×1080，世界 HUD 运行中效果图，正交／轻三分之一轴测均可 |
| 世界层 | 使用一个浅色异星地表、岸边水泵和远处轮式载体；不添加其它未确认建筑或角色 |
| HUD 层 | 左侧或顶部为窄的常驻摘要；右侧为更宽的浅色详情卡，详情卡视觉上覆盖常驻摘要可能重叠区域 |
| 动态内容 | 留空文本槽、数值槽、图标槽；不得生成可读文字、数字、Logo |
| 依赖 | 界面设计文档/界面/03-世界HUD.md、SMP-UI-001、SMP-UI-002、SMP-WORLD-001、SMP-WORLD-002 |

## 3. 可复制中文提示词

制作一张原创工业二次元科幻经营游戏的 1920×1080 世界 HUD 组合效果图。背景是清朗的异星浅色地表和浅层水域，画面中景放置一台通用轮式载体，岸边放置一台岸边水泵，世界区域必须占画面主要面积并保持可辨识。UI 使用深灰蓝基底、浅色冷灰蓝信息卡、极少量橙色焦点和锐角切角模块网格。左侧或顶部是一条窄而克制的常驻 HUD 摘要，只占小面积；右侧是一张明显更宽的对象详情信息卡，卡片有清晰边界、局部遮挡关系和最高阅读焦点，必须在视觉上覆盖常驻 HUD 与世界背景的重叠区域，表现 FieldHudDetailForm 位于 FieldHudResidentForm 之上。详情卡内部保留标题、状态、位置、操作按钮的空白槽位，但不要生成任何可读文字、数字或 Logo；资源图标只放一个抽象水资源图标位置。清线赛璐璐，平整色块，硬边阴影，少量高光线，世界明亮、UI 稳定、焦点明确。不要复制任何现有游戏界面，不要旧项目截图，不要人物、动物、战斗、探索灰雾、棋盘、复杂管网、乱码、伪文字、霓虹、全屏白色面板或让常驻 HUD 压住详情卡。

## 4. 可复制 English prompt

Create an original anime industrial science-fiction management game world HUD composition at 1920x1080. The background is a clear alien light-colored terrain and shallow water. Place one general-purpose wheeled carrier in the mid-ground and one shoreline water pump near the bank. The world area must occupy most of the frame and remain readable. Use a deep blue-gray UI base, light cool gray-blue information cards, very restrained orange focus accents, sharp corner cuts and modular grid panels. A narrow, quiet resident HUD summary sits on the left or top and uses only a small area. A wider object-detail information card sits on the right with a clear boundary, local occlusion and the highest reading priority; it must visibly cover the resident HUD and world background wherever they overlap, communicating that FieldHudDetailForm is above FieldHudResidentForm. Leave empty slots for title, status, location and action buttons, but render no readable text, numbers or logo. Reserve one abstract water-resource icon slot only. Clean cel-shaded digital art, flat color masses, hard-edged shadows, sparse highlight lines, bright world, stable UI and clear focus. Do not copy any existing game interface, old project screenshot, character, animal, combat, exploration fog, chessboard, complex pipe network, gibberish, fake text, neon, full-screen white panel or a resident HUD covering the detail card.

## 5. 强制验收

- 详情卡在任何重叠区域都位于常驻摘要之上；不能只靠层级树说明，必须从遮挡边界和焦点边框直接看出来。
- 世界可见区域不少于画面宽度的 55%，水泵和轮式载体不能被详情卡完全遮住。
- UI 明暗关系与 SMP-UI-001 一致，水图标轮廓与 SMP-UI-002 一致，机器和建筑不能变成另一套材质语言。
- 动态文字、数字、进度、连线、警报计数全部留给程序绘制；效果图只提供空槽和结构。
- 若生图模型无法稳定表达层级，必须拆成“世界底图”和“UI 叠加构图”两次生成再合成，不能把错误层级当作可接受样例。
- 未通过时标记 REJECTED-LAYERING 或 REJECTED-STYLE 并保留原图；通过后标记 PROMPT-READY，仍不代表 Unity 接入或运行时排序已验收。

