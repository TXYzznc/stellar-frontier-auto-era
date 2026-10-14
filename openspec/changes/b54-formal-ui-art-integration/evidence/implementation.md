# 正式 UI 接入记录

## 已确认边界

2026-10-14 用户经过三轮确认：只调整现有界面及其已有状态；保留源 PNG；布局以本次效果图为参照，结构与资源身份遵循正式界面文档；按 01→18 页面族实施，采用 1920×1080 验证。不补建领域功能、改动框架核心或操作 Git 索引。

## 交付

- 正式资源 349 张；71 张按用途配置切片，图标／端点／插画保持独立比例，322 张通过 Sprite 范围排除透明留白。源文件、素材 GUID 与资源身份保持不变。
- 当前 40 个 UI Prefab 清单中的 37 个页面 Form，共 105 个页面根，完成区域布局、画框、卡片、操作、导航、输入、提示与插画接入。两个算法 Item 保留程序连线并接入节点正式状态；原有无画面的 HUD 路由宿主保留职责。
- 目录类条目使用卡片网格，选择器／记录使用列表，算法工作区保留左侧节点库、中部画布、右侧检查器与下方问题区，现场页面保留世界可见区。
- `FormalUiPagePresentation` 在现有页面启用时恢复共享标题、导航和出口的对应布局；业务字段、页面根、对象池、操作回调与输入抽象保留。
- 已回写 `Docs/Development/UI-PrefabLayouts/*.contract.json`；正式节点、位置、布局组件、资源与射线设置可由合同门复验。
- SIMHEI 单图集补齐既有界面缺失的“展”字；不改变字体源文件。

## 验证入口

- [逐页图集](index.html)：105 张原尺寸 Unity 独立 Prefab 渲染，带对应效果图链接。
- [导入与源文件检查](source-and-coverage-audit.json)：源 PNG SHA256、GUID、导入标志、覆盖。
- `importer-errors.txt`：Unity 读回的 Sprite 范围、border 与 importer 检查，空文件表示无问题。
- `contract-gate.txt` / `font-gate.txt`：合同、绑定、字体引用与字符覆盖；空文件表示无问题。
- `ui-tests.json`：原有 UI NUnit 用例的原生 Test Runner 结果；以最终完整运行数据为准。
- `runtime-ready.json` 及运行记录：另行标明实际运行态证据，不能把静态 Prefab 渲染当作所有业务状态通过。
- `summary.json`：最终交付计数。

## 继承与缺口

- 设计包中的 `RemoteMachine` 出现在两个页面族，项目采用现有远程机器页；`AlgorithmDiagnosis` 是现有算法编辑器的诊断模式。二者均未额外补建独立 Form 或页面根，覆盖矩阵保留这两个共享项。
- 源文件尺寸与名义尺寸不同，导入使用真实像素范围。源包记录的 19 项语义错位不在本次擅自互换：按用户确认保持设计文档的资源身份，详见 `../art/coverage.json` 的 `semanticConflicts`。
- 既有领域尚未交付的数据／真实开局／离线与综合性能验收继续显示原有不可用原因，不以静态图片、测试世界或占位数值宣称业务完成。
- 未增加动效制作、额外分辨率验收、资源包发版或 Git 提交。

## 可重复执行

在当前项目根目录使用已安装的 Codely Unity 桥接（不创建额外 Unity 实例）：

```powershell
python tools/prepare_formal_ui_art.py
python tools/formal_ui_bridge.py apply-ui
python tools/formal_ui_bridge.py verify-ui
python tools/audit_formal_ui_art.py
python tools/report_formal_ui_art.py
```

`apply-ui` 会修改现有导入与 Prefab；`capture-ui` 只重新生成渲染。执行前先确认 Edit Mode、项目实例与写入范围。变更前备份保存在本机 `.ai/backups/b54-before-art/`，只用于恢复本次涉及资产，不覆盖用户其他工作。

## 最终验证结果

- 349 张源 PNG 的 SHA256 与原始清单一致，素材 GUID 不变；71 张九宫格通过 Unity 读回检查。
- 18 族共 105 个现有页面根的 1920×1080 独立 Prefab 渲染已落盘；结构合同、绑定、射线职责和字体检查通过。
- 原生 Test Runner 的 10 组现有 UI 测试：68 通过、0 失败。未把未完成的广域测试批次计入通过数。
- 实际 Launch 场景加载正式 UI；通过现有按钮回调完成主菜单 → 存档 → 返回 → 设置 → 返回。三个槽位完整显示、空态不覆盖列表、五个音量滑条均位于可视区域。没有创建、加载、删除存档或改动设置。
- [运行录屏](runtime/ui-navigation.mp4) 为原生 Game View 1920×1080、15fps、83 帧；[存档页](runtime/save-slots.png)、[声音设置](runtime/settings.png)、[主菜单](runtime/main-menu.png) 为录屏原帧提取。
- 编译无错误。重新编译时保留项目原有 `UiPanelTestPanel.cs` 的 `badgeType` 未使用警告；最终刷新诊断见 `final-compilation.json`。
- 框架纯度、产品边界与 OpenSpec 严格验证通过，Unity 已退出 Play Mode。未暂存或提交 Git。
- [缺失项与 19 项素材冲突](gaps-and-conflicts.md) 已逐项列出。独立 Prefab 渲染与启动导航检查不代表所有领域数据、离线状态、真机或 Player 性能已验收。
