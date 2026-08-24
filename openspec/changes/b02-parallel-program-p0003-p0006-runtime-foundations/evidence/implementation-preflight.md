# 实施前核验记录

核验时间：2026-08-24 10:54:09 +08:00

## 任务表

- 文件：`Docs/GameDesign/05-开发计划/第一版开发任务表.xlsx`
- 权限：AI永久只读；本次仅核验，未修改。
- SHA-256：`08CD62F223F92D41CA3AC42591978BA9CFEF334AF463149E6DB265D1B010F089`
- 已核验为`进行中`：`P0-003`、`P0-004`、`P0-005`、`P0-006`、`ART-004`、`ART-005`、`ART-006`。
- 工作簿已由用户保存并关闭；Git工作区中的该文件修改属于用户，禁止纳入AI提交。

## Unity

- 地址：`http://localhost:8090`
- 产品：`星际拓荒：自动纪元`
- Unity：`2022.3.62f3c1`
- 渲染管线：URP（`URP-Balanced`）
- 核验状态：未进入PlayMode、未暂停、未编译。
- 结论：满足结构性实施的普通Unity编译前置条件，8090锁已授予客户端窗口。

## Git

- 核验时分支：`main`
- 相对远端：ahead 7
- 用户未提交修改：仅任务表工作簿；不得暂存。
- Git索引／提交锁：空闲，客户端每次分段提交前另行申请短期锁。

## 框架核心单次授权

用户明确允许本change修改以下三个文件：

1. `Assets/Game/ScriptsBuiltin/Editor/GameDataGenerator.cs`
2. `Assets/Game/ScriptsBuiltin/Editor/DataTableGenerator/DataTableGenerator.cs`
3. `Assets/Game/ScriptsBuiltin/Editor/DataTableGenerator/DataTableCodeTemplate/DataTableCodeTemplate.txt`

授权不覆盖任何其他`ScriptsBuiltin`文件。
