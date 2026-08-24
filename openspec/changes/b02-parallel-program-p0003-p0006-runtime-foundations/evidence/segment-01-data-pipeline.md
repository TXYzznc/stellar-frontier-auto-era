# 第 1 段：数据生成与类型解析

日期：2026-08-24

## 实施前基线

- 任务表 SHA-256：`08CD62F223F92D41CA3AC42591978BA9CFEF334AF463149E6DB265D1B010F089`。
- 用户维护的任务表保持未暂存、未提交。
- Unity Skills 8090 指向 `星际拓荒：自动纪元`，Unity `2022.3.62f3c1`；开始时未进入 Play Mode、未编译。
- OpenSpec 严格校验：`openspec validate b02-parallel-program-p0003-p0006-runtime-foundations --strict` 通过。

## 交付与验证

- 提交 `74b60cbe27b14345a501b2f533f2acfa7af22929` 提供领域无关的 DataTable 生成 Profile、可选 namespace 模板和唯一短名 DataRow 回退。
- 默认未配置 Profile 的 Core 表保持原输出路径；项目 Profile 使用源相对路径、代码输出根和 namespace 三元配置，不在通用生成器中出现产品名称。
- Unity 普通刷新/编译完成，Console Error 为 0。
- `AutoEra.Tests.Editor.DataTableGenerationProfileEditModeTests` 的 EditMode 回归测试通过：4/4。

## 第 2 段设计纠偏

最小GameData的三份xlsx尚未创建或修改。实现中确认框架现有AI JSON／Reverse只覆盖DataTable，
且原设计的`AutoEra/` GameData输入目录和Hotfix侧Profile注册分别与项目基线、程序集边界冲突，
因此第2段在写入数据前暂停。

用户随后确认：AI不得直接创建或修改任何GameData xlsx；DataTable、Config、Language统一使用JSON
中间层，由工具进行逻辑内容指纹校验、安全Reverse、正式生成和失败回滚；xlsx保持人工正式数据源。
Profile改由Editor-only `GameData/AIData/GenerationProfiles.json`承载，三类基础数据使用镜像的
`Foundation/`业务路径。既有第1段提交保持有效，后续实现以修订后的OpenSpec为准。
