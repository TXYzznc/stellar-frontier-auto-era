# ART-003 场景路线确认

用户于2026-08-21确认ART-003方向通过。代表性48×40米测试区已在同一玩法骨架下完成Terrain、模块化、手工布景和混合路线对照，最终冻结以下职责：

- Terrain：连续地表、缓坡和大尺度综合色彩。
- 模块化：道路边缘、工程平台、资源点硬边与稳定拼接结构。
- Editor生成：只在授权区域生成无玩法身份的植被、碎石和资源点外围碎片。
- 手工布景：基地入口、资源点焦点、镜头构图和生成后修整。

混合样片包含108个生成装饰并通过0净空违规、保存重开、人工修改保留、未选区域保留和选区重生成验证。日间／夜间Far、Mid、Near证据以及四路线Far对照均保存在美术项目。

High Fidelity、4×MSAA、固定Far相机和GTX 1650环境下，三次Editor Play Mode瞬时采样一致：291个GameObject、223个MeshRenderer、306,187三角形、790 draw calls、15 SetPass、390个阴影投射计数，无动态／静态／实例化批处理。该结果不冻结最终预算；正式资产需在Renderer合并、实例化、LOD和阴影分级后，以独立Player重新Profile。

完整报告：美术项目`Docs/ArtPipeline/ART-003-RouteComparison.md`。联合移交：美术项目`Docs/ArtPipeline/ART-001-ART-003-Handoff.md`。
