## 1. 契约与生成器

- [x] 1.1 `TYPE_BY_KIND` 新增 `TMP_InputField → TMP_InputField`
- [x] 1.2 契约 `_algorithmEditorNodeSearch` 绑定（`TMP_InputField`）+ 重新生成 `AlgorithmEditorForm.Fields.cs`

## 2. 搜索过滤

- [x] 2.1 `AlgorithmEditorForm` 监听 `onValueChanged`，按名称/状态（忽略大小写）过滤节点栏
- [x] 2.2 命中计数回显；点选按过滤后行的稳定 Id 选中

## 3. 回归与收口

- [x] 3.1 普通编译 0 错误 0 警告、Console 0 错误
- [x] 3.2 读模型/实例服务测试回归通过（过滤为纯 UI 逻辑，不动领域）
- [x] 3.3 `openspec validate b29-algorithm-node-search --strict` 通过
- [x] 3.4 更新本 tasks 收口，回传结果
