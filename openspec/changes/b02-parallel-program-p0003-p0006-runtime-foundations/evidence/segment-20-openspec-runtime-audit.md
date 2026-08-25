# 第 20 段：运行时基础 OpenSpec 对账

日期：2026-08-25

## 对账依据

- 逐文件复核 `Assets/Game/Scripts/AutoEra/` 与 `Assets/Game/Tests/AutoEra/Editor/`，不以提交标题作为完成依据。
- 既有实现提交：`54ed5dd`、`d494860`、`1a25553`、`e2917a9`、`ac37c44`、`b3a61b9`、`1b8e0bb`、`03a6a80`、`10c3060`。
- 既有测试证据：第 11～19 段；每个已勾选运行时任务都能对应到实现类型及其 EditMode 覆盖。

## 本次同步为已完成的任务

- `2.5`：`AIGameDataContracts`、`AIDataSyncPipeline` 和三类适配器已提供共享路径、指纹、冲突、事务回滚与报告基础；第 2～10 段证据覆盖路径、冲突和回滚。
- `2.6`：`AIGameDataTableGenerator` 已有 Export、Validate、Reverse、Import 入口，并使用逻辑指纹而非时间戳；第 1、3、5、8 段证据覆盖 Core 兼容、冲突和路径门禁。
- `3.1`、`3.3`：`AutoEraApplicationContext`、`AutoEraWorldSession` 与工厂已建立并通过会话／上下文 EditMode 覆盖重复创建、幂等释放和隔离。
- `4.1`～`4.4`：永久 ID、分配器、注册表和 ID／类别引用均有实现和对应 EditMode 覆盖。
- `5.3`、`5.4`：稳定排序键、`IUtcTimeProvider`、`SystemUtcTimeProvider` 和无状态 `TimeUtil` 均已实现，并有对应 EditMode 覆盖。

## 明确保留为未完成

- `2.7`：Config／Language 的 JSON 读写和 Reverse 已实现，但尚未提供与 DataTable 同等的正式批量生成入口；不勾选。
- `5.1`：`WorldClock` 的整数推进、余量与拒绝边界已实现，但开发倍率输入边界未接入；不勾选。
- `5.2`：昼夜值对象与 1,440,000／960,000 边界已存在，但尚未从项目配置读取新世界初始时刻；不勾选。
- `5.5`：尚无“等量直接推进与不同帧切分同时得到相同昼夜阶段”的联合覆盖；不勾选。
- `3.2` 以及 `6.*`：组合根、受控 Procedure 上下文槽、配置和场景流转尚未实现；不勾选。

## 当前验证状态

本次只读复核确认 8090 指向“星际拓荒：自动纪元”Unity 2022.3.62f3c1，且不在 PlayMode。运行时基础的复跑已点对点交给测试窗口；本文件不把待回传的复跑结果表述为新的通过结论。
