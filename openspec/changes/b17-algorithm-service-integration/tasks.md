## 1. 世界级模板库接线

- [x] 1.1 `AutoEraWorldSession` 增加 `AlgorithmTemplateLibrary AlgorithmTemplates` 公开属性，构造时以 `IdAllocator` 创建，`Dispose` 随世界清理
- [x] 1.2 新增五套系统模板定义（基础灌溉、农田基础作业、生长资源采集、储量资源开采、固定路线运输），纯 `AlgorithmDocument`，不预置绑定【阻塞：依赖效应器行为节点，见 b18】
- [x] 1.3 世界创建时向模板库录入五套系统模板；实际逻辑成本定稿（DEC-202）：灌溉 9、农田 14、开采 15、采集 17、固定运输 35【阻塞：同 1.2】
- [x] 1.4 普通编译 + 模板库 EditMode 回归（空态可枚举、`TryGetDocument` 只读查询、未绑定）

## 2. UI 读模型真实实现

- [x] 2.1 新增真实 `IAlgorithmReadModel` 实现 `TemplateAlgorithmReadModel`，从 `session.World.AlgorithmTemplates` 读模板列表与详情（`TemplateDetail`）
- [x] 2.2 替换 `AlgorithmReadModels.Create` 分支：按 `AlgorithmReadModelDomain.Library/Machine` 分域，库页读模板（世界级）、编辑/绑定页读实例（机器级），缺能力各给带原因的 Unavailable
- [x] 2.3 模板行映射（`UiAlgorithmTemplateRow` 的 Label/Status）与详情字段映射（`UiDetailField`），`SelectTemplate`/`ClearSelection` 选中语义落地
- [x] 2.4 读模型 EditMode 回归（`AlgorithmReadModelEditModeTests` 12/12：实例域 9 项 + 模板域 3 项全绿）

## 3. 机器级实例服务接线（阶段 B）

- [x] 3.1 在机器级宿主（区域部署处 `RegionMachineRuntimeRegistry.TryAttach`）创建 `AlgorithmInstanceService`，以 `MachineComputePool` + `IdAllocator` + 硬件版本 + `AlgorithmMachineAdapter.ValidateBindings` 构造
- [x] 3.2 编辑/诊断读模型：`MachineAlgorithmReadModel` 从机器实例服务读草稿/应用状态（版本三元组/逻辑算力/应用请求），供 `AlgorithmEditorForm`/`AlgorithmBindingForm` 使用
- [x] 3.3 实例服务与编辑/诊断 EditMode 回归（`AlgorithmReadModelEditModeTests` 9/9、`MachineDeploymentRuntimeEditModeTests` 5/5 全绿）

## 4. 收口

- [x] 4.1 普通编译 / Console / 引用 / OpenSpec strict / 框架纯度 / 项目边界检查（编译 0 错误、Console 0 错误 0 警告、`openspec validate b17 --strict` 通过、框架纯度 11 项为存量债务非本变更引入）
- [x] 4.2 界面打开验证（代码层）：公开参数页展示真实实例详情（阶段 B）+ 算法库浏览显示五套系统模板（2.x 模板读模型已补齐，`TemplateAlgorithmReadModel` + 库页渲染系统/玩家目录与详情）；**视觉打开验证待 PlayMode/手动确认**（REST 对 PlayMode 域重载跟踪丢失，无法自动取回结果）
- [ ] 4.3 明确未覆盖（节点画布手势交互、编辑/绑定写入口、磁盘存档/离线），回传结果
