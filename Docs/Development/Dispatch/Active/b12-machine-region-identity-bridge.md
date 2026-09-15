# B12 机器永久身份、区域部署与作业通道衔接

日期：2026-09-11。用户已确认实施；负责人 client；任务 ID `b12-client-machine-region-identity-bridge`，优先级 10。

## 授权与顺序

依据 `Docs/Development/Dispatch/Planning/G2-Movement-WorkQueue-Readiness-20260911.md`，先完成同一机器永久 ID → 区域部署/查询 → 既有作业通道申请、等待、释放的窄衔接。继承现有产品规则，不重新讨论已确认事项。B11 的 UI 依赖继续挂起，不阻塞本单元。

1. 阅读现有实现、设计与角色规范；创建独立 OpenSpec `b12-machine-region-identity-bridge`，按 openspec 技能生成 proposal/design/specs/tasks，比较同 ID 绑定的生命周期方案，确定最小产品层适配，并严格校验。
2. 文档就绪后直接实现，无须等待普通文档已阅；若发现需修改框架核心或新产品决策，才报告精确冲突，同时继续其它安全工作。
3. 完成自动化验证、回归与证据；如需 QA，点对点入队并释放 8090，不假称后台运行。
4. 完成后实际通知制作人交付与未完成边界，按队列 complete --claim-next。不将本单元视为整个 G2 或 P2-008 完成。

## 冻结结果与生命周期

- MachineRoster 中已有机器，在 InitialRegion 中可用相同永久 ID 查询，不重新分配或双注册。
- 明确名册持有的永久身份与区域表示的所有权；部署、回收、目标删除、区域/世界卸载按职责清理，不误删仍属于名册的身份。
- RegionWorkQueue 复用既有优先级/FIFO、申请与释放语义；申请者使用相同机器 ID。提供可被服务查询的等待/占用结果，不伪造已经抵达。
- 重复操作与失败路径可预测、无半状态、无泄漏；固定载体不因区域部署接口获得移动能力。
- 实际类型名和接口在核对代码后写入 design；优先窄适配，不顺带重构注册表体系。

## 精确写入范围

- `openspec/changes/b12-machine-region-identity-bridge/`：本单元规范、任务与证据。
- `Assets/Game/Scripts/AutoEra/Machines/`、`Assets/Game/Scripts/AutoEra/World/Region/`：仅身份绑定、部署生命周期和已有作业通道衔接所必需的代码及对应 .meta。
- `Assets/Game/Tests/AutoEra/Editor/`：本单元测试及对应 .meta。
- 如确需范围外程序集、组合根或持久化核心修改，先列精确文件、原因及影响，由制作人另行处理；不要扩张目录授权。
- 任务队列仅通过正式队列工具操作。已有用户改动需保留，重叠冲突先核对。

禁止修改 ScriptsBuiltin、UI、数据工作簿/任务表、模型、材质、Prefab、正式场景、MotionGraph、导航资源或 B08/B09 已验收资源；不新增 NavMesh、物流、完整算法或新移动效应器；不操作 Git 索引、不提交、不唤醒 Git。

## 验收

- 同机同 ID，重复部署/释放幂等，名册查询与区域请求者一致，无双注册。
- 两机竞争遵守优先级及同级 FIFO；等待取消、占用释放能正确接替。
- 目标移除、机器回收、区域卸载/世界卸载各自无残留；区域退出不误销毁名册身份。
- 非法/失败部署不遗留部署标志、区域成员或作业租约；重复清理无副作用。
- 新测试与已有相关机器、区域/作业队列回归通过，普通编译及新鲜 Console 检查通过；记录真实 job、数量与边界，不用历史通过代替本次。
- OpenSpec strict validation、git diff --check 与适用框架纯度审计通过；tasks 只勾选实际完成项。

每个冻结工作单元执行快速执行候选检查；可分离的机械验证可直接派快速执行，客户端保留专业复核。每次回复列本角色全部任务及顺序，真实等待进入 blocked 并让出 Active。普通进度不构成暂停。
