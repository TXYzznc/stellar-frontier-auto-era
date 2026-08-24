# 第 6 段：Config 与 Language 既有行布局映射

日期：2026-08-24

## 本段完成项

- Config 适配器把合法 JSON 映射为既有两行表头与 `# / Key / 备注 / Value` 数据布局。
- Language 适配器把合法 JSON 映射为既有 `锁标记 / Key / Value` 三列布局。
- 本段仅构建内存行数据供后续安全工具写入；不创建、修改或暂存任何 `.xlsx`。

## 验证

- Unity 普通刷新/编译完成，编辑器未进入 Play Mode，`isCompiling=false`。
- `AutoEra.Tests.Editor.DataTableGenerationProfileEditModeTests` EditMode 测试通过：`15/15`。
