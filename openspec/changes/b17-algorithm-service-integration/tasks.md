## 1. 世界级模板库接线

- [x] 1.1 `AutoEraWorldSession` 增加 `AlgorithmTemplateLibrary AlgorithmTemplates` 公开属性，构造时以 `IdAllocator` 创建，`Dispose` 随世界清理
- [x] 1.2 新增五套系统模板定义（基础灌溉、农田基础作业、生长资源采集、储量资源开采、固定路线运输），纯 `AlgorithmDocument`，不预置绑定【阻塞：依赖效应器行为节点，见 b18】
- [x] 1.3 世界创建时向模板库录入五套系统模板；实际成本：灌溉 9（DEC-121）、农田 14、开采 15、采集 17（DEC-122 各 14，偏差见 b19 proposal）、固定运输 35（DEC-123 34 +1 位置参数简化）【阻塞：同 1.2】
- [x] 1.4 普通编译 + 模板库 EditMode 回归（空态可枚举、`TryGetDocument` 只读查询、未绑定）

## 2. UI 读模型真实实现

- [x] 2.1 新增真实 `IAlgorithmReadModel` 实现，从 `session.World.AlgorithmTemplates` 读模板列表与详情
- [x] 2.2 替换 `AlgorithmReadModels.Create` 分支：世界内返回真实读模型，世界外/无世界返回带原因的 Unavailable
- [x] 2.3 模板行映射（`UiAlgorithmTemplateRow`）与详情字段映射（`UiDetailField`），刷新/选中/清选语义落地
- [x] 2.4 读模型 EditMode 回归（世界内 Empty、世界外降级，4/4 通过）

## 3. 机器级实例服务接线（阶段 B）

- [ ] 3.1 在机器级宿主（`MachineExecutionContext` 或区域部署+导航绑定处）创建 `AlgorithmInstanceService`，以 `MachineComputePool` + `IdAllocator` + 硬件版本 + `AlgorithmMachineAdapter.ValidateBindings` 构造
- [ ] 3.2 编辑/诊断读模型：从机器实例服务读草稿/应用状态/问题清单，供 `AlgorithmEditorForm`/`AlgorithmBindingForm` 使用
- [ ] 3.3 实例服务与编辑/诊断 EditMode 回归

## 4. 收口

- [ ] 4.1 普通编译 / Console / 引用 / OpenSpec strict / 框架纯度 / 项目边界检查
- [ ] 4.2 界面打开验证：算法库浏览显示五套系统模板，公开参数页展示真实详情
- [ ] 4.3 明确未覆盖（五模板成本定值、节点画布手势交互、磁盘存档/离线），回传结果
