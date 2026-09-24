## 1. 数据模型扩展

- [x] 1.1 `AlgorithmNodeKind` 新增 `Effector`/`SubmitTask`/`QueryTask`/`CancelTask`；新增 `AlgorithmEffectorAction` 枚举（Spray/ModifySpray/StopSpray/Sow/Harvest/Clean/Transfer/Cut/Drill）；`AlgorithmNode` 增加 `Action` 字段与 `Copy`
- [x] 1.2 `AlgorithmIntent` 增加 `Action`（可空）、`Parameters`（`Dictionary<string, AlgorithmValue>`）与 `BindingKey`，保持既有 `Value` 语义不变

## 2. 端口与验证

- [x] 2.1 `AlgorithmCatalog.Inputs/Outputs` 为 `Effector` 按 `Action` 返回动态端口（event+target+动作参数 / 统一结果事件）；为任务控制节点定义端口；`Cost` 效应器=1、任务控制=4
- [x] 2.2 `AlgorithmValidator.TryCompile` 的必填端口检查覆盖新端口（经 `AlgorithmCatalog.Inputs` 自动驱动）；新增 `Effector` 节点绑定检查（非模板图缺少绑定 → `RequiredBinding`）

## 3. 求值与提交

- [x] 3.1 `AlgorithmEvaluation.Emit` 对 `Effector`/`SubmitTask`/`QueryTask`/`CancelTask` 组装意图（效应器读入全部参数端口并带 `BindingKey`；任务控制读入对应端口）
- [x] 3.2 `AlgorithmMachineAdapter` 增加效应器队列索引 `RegisterEffector`，`Submit` 处理 `Effector` 意图 → 队列 `Submit`，`Ended` 结果事件回传算法；`SubmitTask` 创建显式任务；`QueryTask`/`CancelTask` 暂以 `rejected` 桩回传【完整实现延后到固定运输模板】
- [x] 3.3 效应器未注册/参数不匹配时发布 `rejected` 而非抛异常；`Ended` 结果映射到算法结果事件端口

## 4. 测试与回归

- [x] 4.1 效应器行为节点 EditMode 测试：动作端口、求值产生意图、必填缺失失败、绑定缺失拒绝（5/5）
- [ ] 4.2 任务控制节点 EditMode 测试【延后：QueryTask/CancelTask 完整实现随固定运输模板，届时补测试】
- [x] 4.3 普通编译 + 算法核心回归（Graph 4/4、Execution 7/7、Instance 5/5 不回归）；框架纯度 + OpenSpec strict 收口时执行
