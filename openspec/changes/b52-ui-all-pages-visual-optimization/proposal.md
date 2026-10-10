## Why

代表工作区只有经过用户验收，其构图和复用经验才适合全量推广。全量 UI 需要完整覆盖清单与独立美术门禁，避免漏页、重复建设或把结构完成误报为正式视觉完成。

## What Changes

- 基于 b51 用户原型验收记录，按页面家族完成剩余结构优化，覆盖当前 40 个 UI Prefab 及其所有分页。
- 建立目录/物件、工作台、现场检查、管理概况、阅读报告、简洁流程等不同构图；复用控件和行为，不强制所有页面同一外壳比例。
- 保留已批准业务合同、不可用原因、输入焦点、返回上下文与读写权限；不补造缺失领域。
- 结构通过后独立推进正式视觉系统、代表效果图、切片资源、Unity 装配和动效；代表正式效果通过后推广其余美术。
- 全流程只进行 1920×1080 视觉验收，节点布局考虑锚点、拉伸、长文本和滚动。

## Capabilities

### New Capabilities
- `ui-family-rollout`: 全量页面覆盖、分批结构验收和推广门槛。
- `ui-art-and-motion-delivery`: 后续正式美术与动效的独立交付和验收。

### Modified Capabilities
无。继承既有状态颜色、工程科幻方向和领域权限。

## Impact

Docs/Development/UI-PrefabLayouts、界面规格、Assets/Game/Prefabs/UI、AutoEra/UI、AutoEra/Editor/UiProto、相应 Tests；后续美术源文件归 ArtResource，项目只接收交付资源。不改框架核心、不新增玩法、不绕过 b47～b50 的领域阻塞，不操作工作簿或 Git 索引。
