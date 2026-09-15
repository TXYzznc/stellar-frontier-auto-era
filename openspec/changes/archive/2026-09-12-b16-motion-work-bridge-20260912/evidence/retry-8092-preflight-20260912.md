# B16 重试安全检查点与前述结果更正

任务：`b16-motion-work-bridge-retry-20260912`。窗口：客户端3号。检查时间：2026-09-12 14:46 UTC。

## 结论

本次重试未完成，不能以历史测试计数作为本次运行结果。此前登记的 result-ready 已通过 `tools/task_lifecycle.py rework` 撤回为 Rework；原记录保留以便审计。没有自行 Accepted。

## 8092 实测与注册冲突

- `.ai/dispatch/window-registry.local.json` 指定正式项目和客户端3号端口为 8092。
- `python .claude/skills/unity-skills/scripts/unity_skills.py health --port=8092` 返回 `ok=false`，连接 `/health` 超时。
- `Get-NetTCPConnection -LocalPort 8092` 未返回本机监听项。
- 服务端 `C:/Users/WIN10/.unity_skills/registry.json` 将同一正式项目登记在 8090、PID 32860；该 Unity 进程存在。这与调度注册表冲突，不等于 Unity 未安装或未运行。
- 遵守本次端口限制，没有向 8090 发送任何调用，没有使用旧客户端窗口，也未改端口配置或重启编辑器。
- 已向任务调度与健康监控3号发出路由协作请求。当前无法确认运行中的 Unity 是否处于 PlayMode、编译或更新状态；之前“8092 已释放且无 pending”的报告没有本次实测依据，应撤回。

## 历史证据实际覆盖

- `AlgorithmServicesIntegrationTests` 源码包含真实 InitialRegion 初始化、NavMesh 导航、`panel.Close()` 后导航继续和最终导航完成断言；未构造 `MotionWorkBridge`，未创建效应器作业请求。
- `MotionWorkBridgeEditModeTests` 四项仅覆盖 null 参数、反射返回类型和公开成员存在；不能据此证明作业预约状态、取消、释放或实际动作表现正确。
- 归档 `client-verification.md` 本身明确保留真实 NavMesh→桥接→效应器联合场景缺口。
- 历史 1/1、4/4、15/15 是历史记录；本次未重新运行，不能作为重试验收。

## 待完成验证

在授权的正式项目 8092 恢复、身份和工具占用核验通过后，用真实区域/NavMesh、正式载体和一个既有效应器验证同一机器/任务/请求 ID 的导航、表现、预约、等待、安全取消和释放。实际关闭观察窗口后工作继续；断电/退出及重复释放无残留。保存本次新结果、时间、失败断言（如有）及编辑器结束态后，再登记 result-ready。

本检查点只读取源码和环境状态、修正生命周期记录并补充证据，未修改实现、场景、Prefab、任务表、xlsx 或 Git。
快速执行候选检查：本单元涉及证据有效性判断和路由冲突诊断，不是已冻结机械重跑包，由客户端负责人直接处理。
