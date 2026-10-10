# 已验收 PCG 环境的正式接入

授权：用户于 2026-10-09 验收 ArtResource 版本，并要求迁移、接入正式项目。变更合同见 `openspec/changes/pcg-environment-product-integration/`。

## 使用与调节

从 `Assets/Game/Scene/Launch.unity` 运行，经正式主菜单“继续”进入世界。`InitialRegion` 显式绑定 `RegionEnvironmentController`；正式镜头输入和设置继续走原来的 `RegionInputModule` / `RegionCameraController`。

- 配置：`Assets/Game/ScriptableAssets/Environment/EnvironmentWorld.asset`。
- 正交镜头：默认 14，范围 10～20。
- 地貌水平尺度：默认 0.25，范围 0.15～1；越小越快遇到新的地貌。地形起伏倍率默认 1.10，范围 0.35～1.5；建设区内部起伏固定为 0，建设区气候引导默认 0.70。编辑配置的 Inspector 会同步转换定位点，建设区与对象避让保持物理坐标。
- 运行中：选中 `InitialRegion/Environment runtime` 的 `PCGStreamWorld`，调节倍率并点击“应用到运行环境”；“保存当前倍率到配置”可保留选择。
- 地表配置：`Assets/Game/ScriptableAssets/Environment/EnvironmentSurface.asset`。

已有建设/导航区域仍为 80×80，保留原有对象与导航坐标合同；内部保持世界高度 0 的平地，边界外默认 24m，并用低频边缘扰动改变恢复距离，连续恢复到完整 PCG 地形，避免固定圆形平地边界。镜头可离开建设区继续浏览流式环境；外围地形显示不扩大现有领域建设或机器导航权限。正式世界关闭原点搬移，保留权威对象坐标；请求坐标保持原样机 ±1,000,000m 验证范围。

## 本次地形形态修正

初始区域之前看起来接近圆形，原因来自三处实现叠加：建设区权重使用欧氏距离，水面和熔岩着色器也用 `length(delta)` 做圆形裁剪，正式装饰逻辑还保留了样机时代的中心圆形安全带。外围高度则在原公式中直接乘建设区权重，且正式参数的起伏倍率偏低，因此中心被压成几乎没有高度差，过渡边界也像一块人工贴上的圆盘。

现行算法保留建设区的 80×80 平地合同：矩形内部直接取“建设区基准高度”（正式值为 0），矩形外采用倒角矩形距离，边缘扰动只改变过渡宽度，使用平滑函数将高度连续恢复到自然地形。水面、熔岩和装饰净空采用同一矩形合同，不再生成圆形安全带。自然地形的垂直起伏由“地形起伏倍率”控制，地貌水平尺度仍只影响地貌在水平面的展开速度。

### 参数与实现边界

| 目标 | 参数调整即可 | 需要改算法或代码 |
| --- | --- | --- |
| 外围整体高差 | `地形起伏倍率`，当前 1.10 | 无 |
| 地貌出现的水平距离 | `地貌水平尺度`、`气候变化尺度` | 无 |
| 建设区大小与过渡宽度 | `建设区范围`、`建设区过渡宽度` | 无 |
| 边缘规则程度 | `建设区边缘自然度` | 无 |
| 建设区是否保持可导航平地 | `建设区内部起伏`、`建设区基准高度` | 只有要改变“平地坐标合同”时才改代码 |
| 圆形边界、圆形液体裁剪、样机固定圆形净空 | 不能靠参数可靠修复 | `PCGWorldField`、水/熔岩 Shader、正式装饰保留逻辑已修正 |
| 高度图归一化截断 | 不能靠镜头或地貌尺度修复 | “地形垂直编码范围”需与最高自然高度一起校准，当前为 96 |

这些字段和地表/散布相关配置的 Inspector 显示名已统一为中文；Shader 的属性显示名也已同步中文化，内部属性标识保持不变以兼容材质引用。

## 接入内容

七地貌、随机密集草与稀疏斑块、压草变暗及恢复、共享风场、雪/沙/泥地表覆盖和接触历史、液体涟漪均来自验收版本。正式机器呈现成功后显式注册 `PCGGrassInfluence` 与 `PCGSurfaceContact`；未来对象可通过 `RegisterReceiver` 选择 Wheels / Feet / Drag，不需要草碰撞体。

只停用旧的固定 `RuntimeScatterer` 和导航平面的可见 Renderer；导航 MeshFilter/Collider、初始对象、业务矿脉表现与实体入口继续使用已有配置。装饰及草避让初始对象占地。

环境继承样机的三色环境光、暖色主光和柔和阴影。运行时克隆当前 URP 质量配置提供足够的正交镜头阴影距离，兼容画质切换并在退出时恢复各画质资产；不保存运行时克隆。Unity 2022.3 的质量级覆盖规则依据 [QualitySettings.GetRenderPipelineAssetAt](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/QualitySettings.GetRenderPipelineAssetAt.html)。

退出世界对称取消数据任务并释放 Terrain、实例、覆盖网格、动态纹理和质量配置，重进不沿用旧接触历史。样机面板、直接设备输入、测试代理没有进入正式运行时。

## 迁移与复现

`tools/migrate_pcg_environment.py` 仅复制新资产，冲突时停止，不覆盖共享项目文件。导入清单为 `Evidence/PCGProductIntegration/asset-manifest.json`：104 项依赖，10 项内容和 `.meta` 均与正式项目一致并复用；新增文件约 1.95MB。清单保留源摘要及正式摘要，适配修改单独标记。

安装器：`Game Framework/AutoEra/Environment/Install Accepted Environment`。安装前场景备份在 `Library/PCGEnvironment/InitialRegion.before-environment.unity`，只增量保存 InitialRegion 并恢复原打开场景。

Editor 原生数据/引用测试为 `PCGProductEnvironmentTests` 的两项 `[Test]`；正式入口集成断言为 `VerifyFormalWorld`。UnitySkills 的长跨域 Test Runner 作业曾报告 `failed_runner_not_restored`，因此另提供 `Game Framework/AutoEra/Environment/Validate Formal Runtime`，从 Launch 进入实际 Play Mode 执行相同集成断言，产生机器可读结果并退出。该工具只存在于 Editor 程序集。

证据目录：`Docs/ArtPipeline/Evidence/PCGProductIntegration/`。最终检查结果和限制见该目录 `verification.md`。
