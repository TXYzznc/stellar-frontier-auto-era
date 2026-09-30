## 1. 出口与导航

- 普通全屏 Form 使用一个“返回”按钮关闭当前 Form；GF 的覆盖栈负责恢复来源界面。
- `ProgressReportForm`、`OperationFeedbackForm` 等 `EscapeClose=false` 的报告型界面，显式按钮直接调用安全关闭路径，不依赖 EscapeClose。
- 只有多步确认、放置、退出流程等需要区分“返回上一步”和“放弃当前流程”的界面保留两个出口，并在契约中说明差异。

## 2. 算法编辑器

- 继续使用 `IAlgorithmReadModel` 作为唯一 UI 数据入口；不在 Form 内创建第二份算法数据库。
- 编辑状态只修改草稿；模板实例化创建草稿实例；节点增删、端口连线和绑定均通过实例服务的版本检查接口。
- 公开参数修改复用草稿修订和统一 Apply 流程；错误阻止应用，警告走确认。
- 诊断模式只读最近运行记录和节点执行路径，不执行算法、不覆盖历史值。

## 3. FieldHudForm 拆分

- `FieldHudForm` 退化为常驻 HUD 协调器。
- 常驻状态、导航、保存和选中对象详情分别成为可独立加载的 Form/Prefab；首批迁移机器详情、资源点详情和建筑详情。
- 所有子 Form 通过 `AutoEraUiNavigator.Open(source, view, request)` 透传 `AutoEraUiSession`，关闭后恢复 HUD 焦点和区域选择。
- 常驻 HUD 与现场详情页不阻塞世界输入，全屏管理页继续阻塞世界输入。

## 4. 验收

- ProgressReportForm 的返回按钮和关闭按钮（若保留）实际关闭。
- 算法从模板实例化到绑定、连线、参数修改、应用、诊断可完成一条可重复测试链。
- HUD 任意入口打开并关闭后，WASD、镜头、点选和世界对象交互均恢复。
- 编译、引用、契约、项目边界、框架纯度和相关 Unity 测试全部通过。
