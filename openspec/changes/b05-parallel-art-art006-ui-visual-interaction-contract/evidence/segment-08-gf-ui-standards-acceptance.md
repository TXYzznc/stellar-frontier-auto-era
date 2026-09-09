# Segment 08：ART-006 GF UI Standards 接入与性能验收

日期：2026-09-09

## 项目接入表（对应 GF UI Standards 08）

| 项目 | 已接入内容 |
| --- | --- |
| Unity / GF 版本 | Unity 2022.3.62f3c1；GF UIComponent / `UIExtension` |
| 参考分辨率与缩放 | 两枚 `AutoEraUiFormBase` 运行实例统一添加 `CanvasScaler.ScaleWithScreenSize`，参考 `1920×1080`，宽高匹配 `0.5` |
| 运行入口 | `Assets/Game/Prefabs/UI/Operations/FieldHudForm.prefab`、`BaseCommandHubForm.prefab` |
| 加载与表 | `UITable` 6001/6002，`UIGroupTable` 的 `Default` 组；通过 `UIExtension.OpenUIForm` 打开 |
| Form / 生命周期 | `FieldHudForm`、`BaseCommandHubForm` → `AutoEraUiFormBase`；OnOpen 注册输入与请求版本、OnClose/OnRecycle 对称清理 |
| 输入 | EventSystem 归一化后经 `AutoEraUiCancelIntentProxy` / `AutoEraUiRuntime` / `AutoEraUiIntentRouter`；无 `Input`、`KeyCode` 轮询 |
| 字体 | `Assets/Game/Fonts/UI/AlibabaPuHuiTi-3-85-Bold SDF.asset`；正式入口中已移除该字体不支持的装饰符号 |
| 页面 owner | Field HUD 与基地指挥中枢均由 AutoEra 客户端 UI 层负责；视觉样式由已验收 Operations 资源复用 |

## 性能与结构检查

- Native PlayMode 用例打开两枚正式 Form 后的采样：frame time `0.0045472ms`、render time `0.0013231ms`、`9` batches / draw calls / set-pass calls、`75` triangles、`115` vertices；采样内存 allocated `429.95MB`。该数值为 Editor 测试环境快照，不等价于发布版性能预算。
- 正式 Operations Prefab 的静态扫描未发现 `ContentSizeFitter`、`VerticalLayoutGroup`、`HorizontalLayoutGroup`、`GridLayoutGroup` 或 `ScrollRect`。当前首屏为固定数量摘要卡，不存在大数据动态列表，因而没有运行期列表实例化、对象池或虚拟化路径。
- `Assets/Game/Scripts/AutoEra/UI/` 中没有以刷新 UI 数据为目的的 `Update` 轮询；`AutoEraHoldToConfirmView.Update` 只在用户持续按住 1.2 秒确认控件时运行，`AutoEraUiVisualTimer` 只在成功反馈的约两秒窗口运行，均不读取领域状态或分配列表项。
- 原生 PlayMode `OperationsForms_OpenAndCloseThroughUiExtension` 在保存的 `Launch` 场景运行 3.883532 秒通过，并确认 `CanvasScaler`、GF 打开关闭、状态序列、焦点恢复；Console Error=0。最后一次同类回归 3.760832 秒通过且无字体缺字输出。
- EditMode 结构状态门禁：`AutoEraUiPrefabBindingEditModeTests` job `da7db42f` 4/4 通过；操作合同门禁 `d2318778` 7/7 通过。没有 Missing Reference/Script 的运行期异常证据。
- 最终 Operations 节点接线与布局复核：`AutoEraUiPrefabBindingEditModeTests` job `970c3ad0` 4/4 通过，覆盖权威状态、可信进度、长等待、失败／取消／请求丢失、按钮目标图形及文字填满父背景的布局门禁；美术（2D）v4 只读复核确认四个操作节点没有二次偏移或背景遮字。原生 Launch PlayMode 最终 XML 通过，耗时 `3.791461s`，Console Error=0、Warning=0。

## 仍适用的边界

- 当前 AI 强制验收只针对 1920×1080；16:10、21:9 与其它比例保留合理缩放结构但不在本次验收范围。
- 当前不存在真实的高容量日志、背包或聊天列表；当相应业务页落地时，必须另行提供 Item 池化／虚拟化和 Canvas rebuild / GC 采样，不得把本段固定摘要页证据复用于该类页面。
