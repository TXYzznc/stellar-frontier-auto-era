# 第 7 段：Config 与 Language xlsx 只读导出

日期：2026-08-24

## 本段完成项

- Config 与 Language 适配器可以只读现有 xlsx，建立带镜像相对路径和规范化逻辑内容指纹的 JSON Manifest。
- Language 回归直接读取现有正式 `GameData/Languages/English.xlsx`，确认 `Framework.Ready → Ready` 被读取；测试不写入该文件。
- 本段不执行 JSON 写盘、Reverse 或正式生成，不创建、修改或暂存任何 `.xlsx`。

## 验证

- Unity 普通刷新/编译完成，编辑器未进入 Play Mode，`isCompiling=false`。
- `AutoEra.Tests.Editor.DataTableGenerationProfileEditModeTests` EditMode 测试通过：`16/16`。
