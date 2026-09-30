## Why

P2-010「中枢机器列表与状态摘要」与 P2-012「移动、选中与状态基础反馈（程序部分）」的领域与读模型早已交付，但逐项审计后发现一个真实缺口：中枢「对象与系统」页的机器索引渲染了**全部**花名册机器（含库中蓝图），违反 DoD「中枢只显示已连接机器」。其余审计项（MachineReadModel 列表／详情／整备三栏、FieldHudForm 的选中同步与定位、选中轮廓与轮组运动）均已接线并有测试，无需改动。

## What Changes

- `BaseCommandHubForm.RenderObjectsIndex` 过滤到「已部署」机器（库中蓝图不属中枢远程视图），行点击映射同步改到过滤后列表。
- 机器库（MachineLibraryForm）继续按 `Deployed` 标志分「库中／已部署」两页，不受影响。

## Capabilities

### Modified Capabilities

- `hub-machine-list`: 中枢机器索引只列已部署到现场的机器，库中蓝图移出。

## Impact

- `Assets/Game/Scripts/AutoEra/UI/BaseCommandHubForm.cs`（索引过滤 + 行点击映射）

## Non-Goals

- 中枢总览页三栏摘要（基地与资源／运行与生产／近期关注）聚合：属 P6-009 中枢业务数据聚合批次（经济／成长／生产域尚未接线）。
- P2-012 视觉表现（激活灯、断电、等待的 VFX）：技术美术，由美术批次验收，程序部分（选中轮廓、轮组运动）已交付。
- 「已连接」的严格语义（Connected=Activated∧Powered∧Signal）过滤：按「已部署」过滤，避免机器失电即从中枢消失（玩家看不到问题机器）；「已断电／无连接」状态由行摘要文本呈现。
