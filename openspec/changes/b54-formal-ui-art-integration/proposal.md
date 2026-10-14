## Why

2026-10-14 正式 UI 素材和 18 族效果图已由用户交付；当前结构原型的导入、九宫格、布局和资源引用需要同步接入。

## What Changes

- 配置 Assets/Game/Sprites/UI 下的正式 PNG；保留源文件字节和 GUID，以实际像素范围配置 Sprite 与九宫格。
- 按 01→18 页面族顺序调整现有 40 个 Prefab 与已有分页，布局对齐效果图，结构及资源映射遵循界面设计文档。
- 同步工程布局合同，保留业务接线、状态、焦点和输入权限，记录缺失页面及素材冲突。
- 验证 1920×1080 编辑器渲染、原有 UI 回归、引用、编译与项目边界。

## Capabilities

### New Capabilities
- `formal-ui-art-integration`: 正式 UI 图像导入、页面组合和可复验交付。

### Modified Capabilities
无。

## Impact

Assets/Game/Sprites/UI 的 importer、Assets/Game/Prefabs/UI、Assets/Game/Scripts/AutoEra/UI 与 Editor/UiProto、Docs/Development/UI-PrefabLayouts 及本变更证据。禁止修改源 PNG、业务功能、框架核心、包配置、任务工作簿或 Git 索引。
