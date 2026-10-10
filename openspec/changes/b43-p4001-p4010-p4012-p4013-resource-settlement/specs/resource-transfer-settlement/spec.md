# 资源权威与入库结算

原始任务映射：P4-001、P4-010、P4-012、P4-013。本文件为增量合同；正式规则引用design.md，不覆盖未完成的原任务范围。

## ADDED Requirements

### Requirement: Transfer commits conserve resources
统一转移 SHALL 在一次安全提交内原子扣源加目标，返回实际数量并保证重试幂等。

#### Scenario: Competing consumers
- **WHEN** 两个行为同时预留同一来源剩余资源
- **THEN** 合计承诺和实际提交不超过可用量，失败方得到可处理原因。

#### Scenario: Repeated completion
- **WHEN** 同一事务完成回调被重试
- **THEN** 资源只转移一次，返回原结果，日志不重复增加数量。

### Requirement: Reservations release safely
预留 SHALL 明确归属任务与目标代次，取消只释放未提交部分。

#### Scenario: Destination becomes full
- **WHEN** 开始动作后目标容量被其他合法事务占用
- **THEN** 在提交边界按合同给出实际量/阻塞，不丢失或复制剩余资源。

#### Scenario: Cancel after partial transfer
- **WHEN** 部分单位已提交后行为取消
- **THEN** 已提交量保留在目标，未提交量仍在来源，预留释放。

### Requirement: Warehouse classification is exclusive
仓库 SHALL 对每件入库对象执行唯一分类，通用资源只在入库时转余额，实体物品保持仓库归属。

#### Scenario: Ore enters warehouse
- **WHEN** 一批矿石从机器货舱真实交付仓库
- **THEN** 矿石实体数量减少且全局矿石余额等量增加，不同时形成实体库存。

#### Scenario: Physical item remains local
- **WHEN** 种子或农产品等实体物品入库
- **THEN** 占该仓库容量并保留永久ID，其他仓库不能直接取用。

#### Scenario: Packaged component follows library routing
- **WHEN** 封装组件入库
- **THEN** 按正式容量与结算规格进入组件库，释放接收空间，不作为占实体仓库容量的物品。

### Requirement: Physical display is not quantity authority
资源结算 SHALL 继承b07唯一归属合同，视觉代理不能成为数量权威。

#### Scenario: Proxy is culled
- **WHEN** 货物视觉代理因LOD或卸载不显示
- **THEN** 库存、拥有者和待解决交付责任均保持不变。

