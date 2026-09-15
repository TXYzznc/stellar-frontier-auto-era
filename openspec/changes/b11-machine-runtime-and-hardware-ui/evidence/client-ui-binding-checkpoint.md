# B11 UI 接线前检查点

2026-09-10。仅为已实现服务到后续视觉交付的接线映射；不表示UI通过或已接入玩家场景。

## 已有权威入口

- `AutoEraWorldSession.Machines` 持有当前世界 `MachineRoster`；关闭世界后不可复用旧实例。
- `MachineRoster.TryGet(PersistentId, out MachineInstance)` 按永久ID重新解析；名称只用于展示，不能作为绑定键。
- `MachineInstance.Changed` / `Revision` 驱动快照更新；切换目标或关闭面板必须解绑旧对象。不能由UI计时推断业务失败。
- 状态独立读取 `Activated`、`RequestedRunState`、`PowerSwitchOn`、`SupplyAvailable`、`Powered`、`SignalAvailable`、`Connected`、`Integrity`。不可把运行请求当实际可工作。
- 容器读取 `Definition.BaseCapacity`、`TotalCapacity`、`UsedCapacity`。核心读取 `ComputeCapacity`、`ReservedCompute`、`ComputeWaitingCount`、`LogicCapacity`、`AppliedLogicCost`。
- 槽位按 `Definition` 的三类数量及 `GetComponent(kind,index)` 枚举，组件身份为 `ComponentInstance.Id`，所属为 `OwnerId`。没有核心开关，不把货舱另列第四类。
- `MachineHardwareOperation.Begin` 仅接受已经确认的管理意图；绑定其 `RequestVersion`、`State`、`Result` 和 `Changed`。等待取消使用同一版本，返回成功并不代表硬件已提交，最终以State判定。
- 管理来源由现场/中枢/库入口确定，不可由按钮自行选取更宽权限。`Rename`当前是服务原语，未来UI适配层须先核对来源权限再调用，不能把无来源参数理解为远程无条件可写。

## 仍待实际接线的缺口

1. `RegionHudPresenter` 当前显示 `RegionObject` 公共摘要，尚未用区域选择ID解析机器Roster；不能把区域机器代理重复分配为另一个实体并保留双重身份。
2. 需要经批准的机器面板候选和字段绑定，复用FieldHud/Hub与现有意图路由，不新增UIForm或设备轮询。
3. 当前只有服务配置恢复，未声称活动任务/行为/算法存档。供电和信号是外部适配事实，没有新增电网或通信模拟。
4. DEC-195已补齐固定载体HP/二级规格、货舱容量/价格/功率，生成与实例回归见client-dec195-config.md；任何未来PendingConfiguration仍不能仅凭美术Ready实例化。
5. 已有GF集成测试验证配置加载，不能替代现场改名、装卸、焦点、权限和晚到回调的端到端验收。

## 对2D的差异同步

已实际发送：`ComputeInUse`、`LogicCapacityInUse`、`RepairRequired`三项拒绝原因；停止只等待已有物理行为安全收尾，不等待全部排队任务执行完毕。未要求2D抢占或跳过用户视觉确认。

## 恢复接线所需输入

视觉用户确认及完整交付路径到位后，再进行正式Prefab/GF接线；DEC-195配置已独立恢复执行，不等待视觉同步到齐。不得将本检查点或服务测试通过标为整个B11完成。
