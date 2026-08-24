# 第 10 段：Config / Language JSON 诊断入口

## 实施范围

- 将 Config 与 Language 的 AI JSON 校验接入既有 `GFDiagnosticRunner`。
- 诊断按各自 JSON 根目录递归读取输入，并复用适配器的 Schema、路径、必填字段与重复 Key 校验。
- 空输入目录或空输入集继续视为基础框架基线的有效状态；非法输入在诊断报告中附带 JSON 文件路径与错误内容。

## 安全边界

- 本段仅读取 JSON；不调用 Export、Reverse、Import 或正式生成入口。
- 未创建、修改或提交任何 `.xlsx`。
- 未执行诊断菜单，避免在本段生成运行时诊断报告；代码路径由现有适配器解析用例覆盖。

## 验证

- Unity EditMode：`AutoEra.Tests.Editor.DataTableGenerationProfileEditModeTests`，18/18 通过。
- Unity 2022.3.62f3c1；测试完成后未在 PlayMode，未处于脚本编译状态。
