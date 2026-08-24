# 第 4 段：Config 与 Language JSON Schema 基础

日期：2026-08-24

## 本段完成项

- 新增领域无关的 Config 与 Language JSON Manifest/Entry Schema，并分别校验版本、种类、镜像相对路径和非空唯一 Key。
- 两类适配器复用共享路径规范化门禁；绝对路径、`..` 越界和重复 Key 都在任何 Reverse 之前失败。
- 本段不创建、修改或暂存任何 `.xlsx`；Excel 导出/Reverse 和正式生成入口留待后续实现。

## 验证

- Unity 普通刷新/编译完成，编辑器未进入 Play Mode，`isCompiling=false`。
- `AutoEra.Tests.Editor.DataTableGenerationProfileEditModeTests` EditMode 测试通过：`13/13`，包含 Config/Language 的路径越界和重复 Key 回归。
