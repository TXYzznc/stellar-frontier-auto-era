## Why

FieldHudForm 目前把五个常驻 HUD 模块和全部现场内容页放在同一个 UIForm Prefab 中。界面层级、字段绑定、读模型生命周期和导航入口互相耦合，导致维护成本高，并且全屏界面返回后容易把世界输入状态错误地留在阻塞态。

## What Changes

- 将现场 HUD 的常驻摘要模块与现场内容页拆为可独立加载的 UIForm Prefab。
- 统一通过 AutoEraUiNavigator 透传 AutoEraUiSession，保持数据来源唯一。
- 明确常驻 HUD、现场半屏页和全屏管理页的世界输入策略。
- 迁移选中对象打开现场页、关闭现场页和返回焦点逻辑。
- 为拆分后的界面补齐 Prefab、UITable、绑定和 PlayMode 验收。

