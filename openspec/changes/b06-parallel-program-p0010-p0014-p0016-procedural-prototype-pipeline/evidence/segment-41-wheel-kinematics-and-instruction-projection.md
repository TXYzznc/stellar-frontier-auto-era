# 第41段：轮组运动学与当前指令投影

日期：2026-09-03

## 实现

- `FourWheelPresentation`新增四轮独立运动学输入／输出。滚动角由实际行驶距离除以轮周长计算；每个轮位分别携带转向角、悬挂行程和滚动角。
- 验收场导演绑定轮式载体四条`Steer → Suspension → Roll`关节链，不再通过底盘根摆动伪造行驶；四轮分别应用反向转向、独立悬挂和基于轮径的滚动。
- `AutoEraMotionParameterAdapter`新增只读当前指令投影：仅把已由产品任务／效应器队列选定的动作类型、阶段、目标、进度、效率和中断状态写为表现参数；不读取算法图、不调度队列、不决定任务结果。

## 验证

- QA job `f04ab669`：`FourWheelPresentationEditModeTests` 1/1 通过。覆盖四轮独立转向、行驶距离／轮径导出滚动角与独立悬挂行程。
- QA job `a7ab6430`：`AutoEraMotionParameterAdapterEditModeTests` 2/2 通过。覆盖当前指令投影写入与Adapter无队列控制边界。
- QA job `ec4b6dc2`：`FourWheelPresentationEditModeTests` 1/1 通过，用普通Unity编译覆盖四轮验收场导演脚本改动。
- 所有作业结束时Unity 2022.3.62f3c1均非PlayMode、未编译、未更新；测试窗口已释放8090。
