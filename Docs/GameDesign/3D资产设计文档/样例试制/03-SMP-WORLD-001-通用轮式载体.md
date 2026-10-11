# SMP-WORLD-001 · 通用轮式载体

## 1. 目的与技术合同

这是第一版唯一的移动机器载体样例。它必须清楚表达 2.40×1.60×1.20m 的紧凑轮式底盘、2 个传感器安装位、2 个效应器安装位、1 个计算核心舱和自带 30 单位通用容器。样例用于判断机器造型是否能承载后续传感器与效应器，而不是直接作为最终高模。

## 2. 生成规格

| 项目 | 要求 |
|---|---|
| 输出 | NEW_smp_world_001_wheeled_carrier_turntable@1536x1024.png |
| 存放路径 | `Assets/Game/Sprites/World/Standalone/01-MachinesAndCores/` |
| 画布 | 1536×1024，浅中性背景或透明背景均可，但主体边缘必须清晰 |
| 构图 | 中央三分之四轴测主视；右侧放主视、顶部视、背视小图，不放文字标签 |
| 尺度 | 以同一台机器为基准；轮胎接地点、底盘最低点和安装面必须可读 |
| 状态 | 本次只做 idle；不画运动残影、作业粒子或故障变体 |
| 依赖 | 世界资产设计文档/03-可放置对象/01-机器载体与计算核心.md、世界资产设计文档/02-共享组件/02-接口锚点与模块化边界.md |

## 3. 可复制中文提示词

原创工业二次元异星拓荒机器概念设计，一台通用轮式载体，真实比例 2.40×1.60×1.20 米，紧凑四轮底盘，四个轮组具有可信悬挂和接地点，浅灰暖白外壳配深灰蓝结构层，橙色只用于锁扣、作业方向标记和少量安全件。顶部清楚留出两个传感器安装位、两个效应器安装位和一个可拆卸计算核心舱；后部或底盘内部表达30单位通用容器，但不要画出文字或数字。机器具有前进方向、维修舱盖、散热片、护栏和模块化接口，所有受力件有支撑，不悬浮，不把安装位画成装饰贴纸。中央为三分之四轴测主视，右侧同一模型的主视、顶部视、背视小图，几何关系对齐。清线赛璐璐，平整色块，硬边阴影，少量高光线，原创异星拓荒符号，功能可读优先。不要人物、动物、武器、战斗、旧模型、旧贴图、参考游戏机体、照片写实、厚重锈蚀、霓虹、随机零件拼贴、可读文字、Logo、运动残影、粒子特效。

## 4. 可复制 English prompt

Original anime industrial frontier machine concept, one general-purpose wheeled carrier at a believable 2.40 x 1.60 x 1.20 meter proportion. Compact four-wheel chassis with credible suspension and ground contact. Use warm light gray and ivory panels over deep blue-gray structural layers; restrained orange only on latches, work-direction markers and a few safety parts. Clearly reserve two sensor mounts, two effector mounts and one removable computing-core bay on top. Suggest the built-in 30-unit universal container in the rear or chassis volume without rendering text or numbers. Include a forward direction, service hatch, cooling fins, guard rails and modular interfaces; every load-bearing part must be supported, with no floating pieces and no decorative-only mount sockets. Centered three-quarter axonometric hero view, with aligned front, top and rear orthographic thumbnails on the right, all showing the same model. Clean cel-shaded digital art, flat color masses, hard-edged shadows, sparse highlight lines, original alien frontier symbols, function-first readability. No character, animal, weapon, combat, old model, old texture, copied game mech, photorealism, heavy rust, neon, random part collage, readable text, logo, motion trail or particle effect.

## 5. 评审与通过

- 不看文字时，能分辨移动载体、传感器位和效应器位。
- 三个小视图与主视是同一台机器，安装位没有在不同视图中漂移。
- 轮胎接地、底盘间隙、维修舱和核心舱有真实结构。
- 橙色只形成作业焦点，没有把整台机器变成橙色主体。
- 通过后才允许制作 moving、working、blocked 状态板和正式 3D 三视图；样例本身不代表高模已验收。

## AI 单文档生成合同（可独立投喂）

### 共同输入与身份控制

本文件本身就是完整生成合同。输入只包括本文件和（画面需要 Logo 时）已经生成的 Logo PNG。Logo 不是重新设计对象：保持徽记缺口方向、星核层数、字标比例、负空间和透明背景完全一致；未明确写“出现 Logo”的画面不得自行添加 Logo。不要引用旧截图、旧模型、参考游戏界面或未登记资源。

### 视觉风格与材质

- 原创工业二次元清线赛璐璐世界资产：暖白/浅灰工业结构、深灰蓝骨架、硬边阴影、少量冷白高光线；表面保持简化，不做照片级划痕堆叠。
- 金属为低饱和暖灰，粗糙度视觉表现为宽而硬的高光块；聚合物为深灰蓝哑光；橡胶为近黑并带细微边缘高光；玻璃/液体使用冷蓝半透明，但不得透明到看不出结构。
- 所有主体必须有接地面、支撑、接口、作业方向、维护口和 KeepOut；禁止悬浮、穿插、无支撑结构。

### 配色与状态

- 环境底色：异星岩土 `#B8A98F`、冷灰岩石 `#66727A`、植被/散布 `#647A63`。
- 工业主体：暖白 `#D9D5C7`、浅灰 `#AEB6B8`、深骨架 `#26333D`、信息窗 `#2B5268`。
- 强调色：作业/焦点 `#E68A37`，正常状态灯 `#67C28A`，警告 `#D8B15A`，错误 `#D75E63`；强调色总面积不超过 8%。
- 状态必须同时改变灯形、遮板开合、蒸汽/尘土方向、机械姿态或结构缺口，不能只换灯光颜色。

### 镜头、构图与光照

- 默认正交或轻透视三分之一轴测，水平视线约 35°；主体占画面 55%–75%，周围留出接口、作业区和 KeepOut。
- 主光为左上方暖白大面积软光，辅光为右侧冷蓝弱光，接触阴影清晰但不脏；工作灯和状态灯只照亮局部，不产生全屏光晕。
- 复杂对象按“整体轮廓 → 关键接口 → 状态灯/作业点 → 接地和阴影”组织，避免背景建筑抢主体。

### 输出与负面提示词

- 输出按文档要求生成正视/侧视/顶视或轴测图；透明切图保持统一比例和地面基线，效果图保持 1920×1080。
- `禁止：悬浮建筑、无支撑机械、错误接口、漂浮阴影、圆形 PCG 边界、人物、动物、战斗、血液、旧模型、旧截图、复制游戏造型、可读文字、伪造 Logo、过度发光、照片级噪点、镜头畸变、裁切。`

### 直接提示词

`03-SMP-WORLD-001-通用轮式载体，原创工业二次元清线赛璐璐科幻世界资产，暖白浅灰工业结构 #D9D5C7/#AEB6B8，深灰蓝骨架 #26333D，异星岩土 #B8A98F/#66727A，少量琥珀作业焦点 #E68A37，硬边阴影，少量冷白高光线，三分之一轴测或正交镜头，明确接地、支撑、接口、作业方向、维护口和 KeepOut，状态通过结构和姿态表达，不生成文字和 Logo，不使用旧截图或参考游戏造型。`

### 交付验收

- [ ] 尺寸、视图、接口、Pivot/接地和 KeepOut 与本文件一致。
- [ ] 材质边界清楚，状态差异不只依赖颜色。
- [ ] 主体轮廓在缩略图中可识别，背景不抢主体。
- [ ] 无人物、动物、战斗、旧资产痕迹、乱码和伪造 Logo。

### 负面提示词

`旧截图、旧模型、旧贴图、旧效果图、参考游戏复制、人物、动物、战斗、可读乱码、伪造数字、未登记对象、错误比例、漂浮结构、错误接口、渐变、照片级材质、柔焦、霓虹过曝、Logo 重绘、白边、裁切、背景污染。`

### 风格

原创工业二次元清线赛璐璐；平整色块、硬边阴影、少量结构高光，保持本文件的形体、镜头和色彩合同，不使用照片、柔焦或参考游戏复制。

### 内容

只生成本文件登记的主体、状态、接口、装饰和场景层级；未登记内容不自行补充。动态文字、数字、连线、百分比和 Logo 均按本文合同处理。

### 构图

主体优先，前景/中景/背景或标题区/主体区/反馈区按本文层级排列；四周保留安全区，接口、作业点、状态装饰和运行时留槽必须完整可见，不用空背景或无关装饰填充画面。
