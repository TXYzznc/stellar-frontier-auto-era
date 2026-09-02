# 第 3.4 段：Motion 工具包清单与 GUID

日期：2026-09-02

- 建立 `MotionCorePackageManifest.json`（`autoera.motion-core-tools`，版本 `1.0.0`），声明 Core／Editor 的固定 GUID 保持策略。
- 清单明确排除 AutoEra Adapter、GF_X、玩法代码、美术资源、场景与 xlsx。
- 使用 JSON 解析与 `.meta` 比对验证：5 个清单文件均存在，GUID 不匹配为 0。
