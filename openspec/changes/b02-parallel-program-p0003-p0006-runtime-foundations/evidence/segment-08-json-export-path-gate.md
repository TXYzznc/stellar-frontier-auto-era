# 第 8 段：Config 与 Language JSON 导出路径门禁

日期：2026-08-24

## 本段完成项

- 共享同步管线提供按数据类型解析的 AI JSON 输出路径，并拒绝路径穿越。
- Config 与 Language 适配器提供 xlsx→JSON 导出函数；输出只能进入对应 `GameData/AIData/{Configs,Languages}` 根目录。
- 本段不调用导出函数，不写入 JSON 或 xlsx。

## 验证

- Unity 普通刷新/编译完成，编辑器未进入 Play Mode，`isCompiling=false`。
- `AutoEra.Tests.Editor.DataTableGenerationProfileEditModeTests` EditMode 测试通过：`17/17`。
