# 第 2 段：AI GameData Profile 与安全管线基础

日期：2026-08-24

## 本段完成项

- 完成任务 `2.4`：新增仅供 Editor 使用的 `GameData/AIData/GenerationProfiles.json`，为 `Foundation/` 输入配置 `Assets/Game/Scripts/AutoEra/DataTable/` 输出和 `AutoEra.DataTable` namespace。
- 新增领域无关的三类 AI 数据根目录常量、Profile 读取/校验和 Domain Reload 后重载逻辑；Profile 不进入 `AppConfigs`，也未新增 asmdef。
- 建立共享同步基础：数据种类根目录映射、拒绝绝对路径和 `..` 路径、规范化逻辑内容 SHA-256 指纹、导出基线硬冲突门禁、临时备份的可回滚替换原语和结构化报告字段。
- 未创建、编辑或暂存任何 `.xlsx`；Config／Language 适配器、DataTable 现有入口接入和正式 Reverse 将在后续段完成。

## 验证

- Unity Skills 8090 已核验项目为 `星际拓荒：自动纪元`，Unity `2022.3.62f3c1`；结构变更期间未进入 Play Mode。
- 普通 `AssetDatabase.Refresh` 后 Unity 编译完成，`isCompiling=false`。
- `AutoEra.Tests.Editor.DataTableGenerationProfileEditModeTests` EditMode 测试通过：`11/11`，覆盖既有 Core 兼容、Profile JSON 读取／重复拒绝、路径越界、指纹稳定性与基线冲突硬失败。
