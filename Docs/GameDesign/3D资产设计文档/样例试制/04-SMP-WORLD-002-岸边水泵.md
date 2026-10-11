# SMP-WORLD-002 · 岸边水泵

## 1. 目的与技术合同

岸边水泵是已确认但需要单独表现的第一版建筑。它负责从无限浅层水域取水，包含泵体、短取水管、封装交接位和暂存区；水域不会被抽空，画面不能暗示枯竭资源或复杂管网。

## 2. 生成规格

| 项目 | 要求 |
|---|---|
| 输出 | NEW_smp_world_002_shore_pump_idle@1536x1024.png |
| 存放路径 | `Assets/Game/Sprites/World/Standalone/03-BuildingsAndConstruction/` |
| 画布 | 1536×1024，三分之一轴测，主体与岸线同时可见 |
| 场景 | 清朗异星浅层水域、低矮岸坡、少量人工铺装；不出现人物和其它建筑 |
| 状态 | idle 主视；泵体状态灯可有一个低亮蓝色待机点 |
| 结构 | 泵体、进水短管、出水／封装交接位、维护面板、地脚和安全边界清楚 |
| 依赖 | 世界资产设计文档/03-可放置对象/03-建筑与施工.md、世界资产设计文档/02-共享组件/03-地表与环境散布.md |

## 3. 可复制中文提示词

原创工业二次元异星拓荒建筑概念图：岸边水泵，三分之一轴测视角，单独坐落在浅层水域与低矮岸坡交界处。泵体为暖白和浅灰色模块化外壳，深灰蓝底座与结构梁，少量橙色锁扣和维护把手；一根短而清楚的进水管伸入浅水，一侧有出水或封装交接位，旁边是小型暂存平台和可读的地脚。画面明确表达“从无限浅层水域取水”，不要画干涸水面、储量刻度、长距离管网或复杂工业园。水面使用平整蓝绿色色块和硬边反光，岸坡为自然异星土色与少量低矮植被，设施保持干净、磨损极少。清线赛璐璐，平整色块，硬边阴影，少量高光线，原创拓荒符号，结构和接地点清楚。不要人物、动物、旧建筑、参考游戏场景、照片写实、锈蚀、烟雾、霓虹、可读文字、Logo、管道迷宫、干涸资源或战斗元素。

## 4. 可复制 English prompt

Original anime industrial frontier building concept: a shoreline water pump in a three-quarter axonometric view, placed alone where a shallow alien water body meets a low bank. Use warm ivory and light gray modular housing, deep blue-gray base beams, restrained orange latches and maintenance handles. Show one short clear intake pipe dipping into shallow water, a visible output or sealed-transfer bay on one side, a small temporary staging platform and readable ground feet. The scene must communicate drawing water from an infinite shallow source; do not show a drained shoreline, storage meter, long-distance pipeline network or a complex industrial park. Use flat blue-green water planes with hard-edged reflections, natural alien soil colors and sparse low vegetation. Clean cel-shaded digital art, flat color masses, hard-edged shadows, sparse highlight lines, original frontier symbols, clear structure and ground contact. No character, animal, old building, copied game scene, photorealism, rust, smoke, neon, readable text, logo, pipe maze, depleted resource or combat element.

## 5. 评审与通过

- 进水管、出水／交接位和暂存平台在第一眼能被找到。
- 水泵与岸线的比例像可建造设施，而不是漂浮道具。
- 画面表现无限浅层水域，没有枯竭、污染或储量耗尽暗示。
- 机器样例的暖白／深蓝灰／橙色关系在建筑上保持一致。
- 通过后再制作 pumping、full、no-water 状态；no-water 只表示管路或目标失效，不表示浅层水域被抽干。

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

`04-SMP-WORLD-002-岸边水泵，原创工业二次元清线赛璐璐科幻世界资产，暖白浅灰工业结构 #D9D5C7/#AEB6B8，深灰蓝骨架 #26333D，异星岩土 #B8A98F/#66727A，少量琥珀作业焦点 #E68A37，硬边阴影，少量冷白高光线，三分之一轴测或正交镜头，明确接地、支撑、接口、作业方向、维护口和 KeepOut，状态通过结构和姿态表达，不生成文字和 Logo，不使用旧截图或参考游戏造型。`

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
