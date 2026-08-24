# 第 9 段：Config / Language 安全反向同步

## 实施范围

- 为 `AIConfigAdapter` 和 `AILanguageAdapter` 增加 JSON 到官方工作簿的工具入口。
- 入口先校验 JSON 架构、相对路径与既有工作簿逻辑指纹；仅在全部前置校验通过后，才生成临时工作簿并通过事务替换目标文件。
- 替换成功后从官方工作簿重新读取清单并回写 AI JSON 基线指纹，避免 JSON 与工作簿基线漂移。
- 临时工作簿位于项目 `Temp/AIDataSyncStaging`，失败或替换后清理。

## 安全边界

- 本段没有调用反向同步入口，未创建、修改或提交任何 `.xlsx`。
- EditMode 用例仅将不存在的 JSON 路径传入入口，断言其在任何工作簿操作前失败。

## 验证

- Unity EditMode：`AutoEra.Tests.Editor.DataTableGenerationProfileEditModeTests`，18/18 通过。
- Unity 2022.3.62f3c1，测试结束后未在 PlayMode，未处于脚本编译状态。
- 待提交前执行 `git diff --check`；任务表及其他窗口文件不在本段提交范围内。
