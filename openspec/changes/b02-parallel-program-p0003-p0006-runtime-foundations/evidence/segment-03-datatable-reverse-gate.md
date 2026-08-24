# 第 3 段：DataTable Reverse 指纹冲突门禁

日期：2026-08-24

## 本段完成项

- DataTable JSON 导出现在记录基于规范化表格逻辑内容的 SHA-256 指纹，而非文件时间戳。
- Reverse 既有 xlsx 前重新构建其规范化逻辑内容并与 JSON 导出基线比较；不一致、缺失基线或非法相对路径均硬失败，不能覆盖 xlsx。
- Reverse 报告记录导出基线与当前 xlsx 指纹；同步成功后刷新 JSON 基线指纹。
- 未创建、编辑或暂存任何 `.xlsx`；本段只实现 Editor 工具中的门禁逻辑和测试。

## 验证

- Unity 普通刷新/编译完成，编辑器未进入 Play Mode，`isCompiling=false`。
- `AutoEra.Tests.Editor.DataTableGenerationProfileEditModeTests` EditMode 测试通过：`12/12`，包含 DataTable Reverse 变更指纹的报告与硬失败回归。
