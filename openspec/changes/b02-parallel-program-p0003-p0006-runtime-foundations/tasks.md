本清单覆盖第一版开发任务表中的原始任务 `P0-003`、`P0-004`、`P0-005`、`P0-006`。任务表给出的合计估算为 **30～52 小时**；任务表仍由用户独占维护，本清单不得自动回写状态、实际工时、依赖或备注。

## 1. 只读保护与实施基线

- [x] 1.1 记录 `第一版开发任务表.xlsx` 的实施前 SHA-256、当前 Git 状态和 Unity 编辑器编译/PlayMode 状态，确认工作簿保持只读（P0-003～P0-006）
- [x] 1.2 复核 AppConfigs、现有 Procedure 链、GameData 生成器、场景资源和 `Assets/Game/Scripts/AutoEra/` 边界，确认不新增 asmdef、DI 包或发布热更新配置（P0-003、P0-006）

## 2. 项目 GameData 生成与加载边界

- [x] 2.1 先为现有 Core 表的默认代码输出和运行时加载建立回归测试，锁定未配置 Profile 时的兼容行为（P0-006）
- [x] 2.2 实现领域无关的项目生成 Profile、路径校验和命名空间模板支持，使匹配规则可以配置源路径、代码输出根与 namespace，且不硬编码 AutoEra（P0-006）
- [x] 2.3 为 DataTable 运行时类型解析增加唯一 namespaced `DataRowBase` 回退，并测试零匹配、唯一匹配和短类名歧义（P0-006）
- [x] 2.4 建立Editor-only `GameData/AIData/GenerationProfiles.json`加载与校验，使`Foundation/`产品表代码进入`Assets/Game/Scripts/AutoEra/DataTable/`并使用`AutoEra.DataTable` namespace；不写入AppConfigs、不新增asmdef（P0-006）
- [ ] 2.5 实现三类共享AI中间层契约、业务路径镜像、规范化逻辑内容指纹、硬冲突门禁、临时构建、备份／事务替换、失败回滚和结构化报告（P0-006）
- [ ] 2.6 将现有DataTable JSON导出／校验／Reverse／Import接入共享安全管线，保持Core兼容并移除按时间戳继续覆盖的行为（P0-006）
- [ ] 2.7 为Config和Language实现等价JSON适配、Schema校验、Reverse、同步检查和正式生成入口（P0-006）
- [ ] 2.8 仅在`GameData/AIData/{DataTables,Configs,Languages}/Foundation/`创建服务于启动、场景和世界时间贯通的最小JSON，由工具首次生成xlsx、TXT／bytes／C#；AI不得直接写xlsx，不提前建立P0-011完整对象配置（P0-005、P0-006）
- [ ] 2.9 在AppConfigs登记工具生成的项目DataTable、Config、Language，并实现缺字段、重复ID／Key、非法值、非法引用和Profile／路径越界的可定位验证（P0-006）

## 3. 应用上下文与世界会话

- [ ] 3.1 在 `Assets/Game/Scripts/AutoEra/` 下按实际职责建立运行时目录和 `AutoEra.*` 类型，定义应用上下文、世界会话、会话工厂和释放契约（P0-003～P0-005）
- [ ] 3.2 实现轻量组合根，以构造函数装配纯 C# 服务，并为 Procedure FSM 建立唯一、受控的流程上下文数据槽；不得新增通用 Service Locator（P0-003）
- [ ] 3.3 实现世界会话的创建、活动状态、重复创建保护和幂等释放，并测试返回主菜单、切换会话和关闭时无残留（P0-003～P0-005）

## 4. 永久实例 ID 与对象注册表

- [ ] 4.1 实现不可变 `PersistentId`、`0` 无效规则、比较/哈希/格式化和统一 `ulong` 高水位分配器（P0-004）
- [ ] 4.2 实现新分配、既有 ID 恢复、高水位推进、溢出拒绝和已注销 ID 永不复用，并建立 EditMode 测试（P0-004）
- [ ] 4.3 实现带对象类别验证的注册、注销和查询结果，拒绝无效 ID、重复 ID、类别不匹配和静默覆盖（P0-004）
- [ ] 4.4 实现只保存 ID 与预期类别的持久引用语义，测试目标缺失、重新载入同一 ID、同名对象不自动重绑及跨会话隔离（P0-004）

## 5. 确定性世界时间与现实 UTC

- [ ] 5.1 实现整数毫秒世界时钟、双精度在线余量累积、直接推进、负值/倒退/溢出保护和开发倍率输入边界（P0-005）
- [ ] 5.2 从已校验项目配置读取新世界初始时刻、1,440,000 毫秒昼夜周期和 960,000 毫秒日照窗口，并测试全部边界（P0-005、P0-006）
- [ ] 5.3 实现同刻比较键及按阶段、永久 ID、事件序号的稳定比较，使用不同插入顺序验证结果一致（P0-004、P0-005）
- [ ] 5.4 定义 `IUtcTimeProvider`，实现本地 UTC Provider，并实现无状态 `TimeUtil` 的换算与离线时长计算；测试固定时间、时区无关和时间倒退归零（P0-005）
- [ ] 5.5 验证不同在线帧切分和等量直接推进得到相同世界毫秒与昼夜阶段，普通 UI 状态不参与世界时间倍率（P0-005）

## 6. 业务 Procedure 与场景流转

- [ ] 6.1 实现唯一 `AutoEraStartupProcedure`、`AutoEraMainMenuProcedure` 和 `AutoEraWorldProcedure`，并在 AppConfigs 中登记完整类型名且只保留一个业务启动标记（P0-003）
- [ ] 6.2 实现产品场景切换协调器，封装 GF.Scene 的加载、卸载、进度、失败、代次失效和订阅释放，不修改通用 `ChangeSceneProcedure`（P0-003）
- [ ] 6.3 创建最小主菜单场景和第一版空世界场景，场景名从项目配置读取，保持业务逻辑和产品状态不写入 `ScriptsBuiltin`（P0-003、P0-006）
- [ ] 6.4 贯通 `Launch → Preload → AutoEraStartupProcedure → AutoEraMainMenuProcedure → AutoEraWorldProcedure`，验证世界创建、返回主菜单和再次进入（P0-003）
- [ ] 6.5 覆盖数据缺失、场景加载失败、加载期间退出、框架重启和过期回调，确认半初始化会话被释放且诊断可定位（P0-003、P0-006）

## 7. 集成验证与交付

- [ ] 7.1 运行三类JSON／xlsx往返、并发冲突、新表创建、路径越界、注入失败回滚、Core兼容，以及永久ID、注册表、时间、UTC和生命周期EditMode测试，确认全部通过（P0-003～P0-006）
- [ ] 7.2 退出 Play Mode 后执行普通 Unity 刷新与完整编译，确认无编译错误；结构、字段、场景和生成器变更不得用 FSR 验证代替（P0-003～P0-006）
- [ ] 7.3 从 `Launch` 执行 PlayMode 冒烟测试，连续两次进入/退出空世界，确认 AppConfigs 只选择一个启动入口、三类项目数据可读且无残留订阅/会话（P0-003～P0-006）
- [ ] 7.4 运行 `python tools/audit_framework_purity.py`、`python tools/audit_project_boundaries.py` 和相关 Python 测试，确认通用生成器改动不含产品硬编码且生成代码满足产品边界（P0-003、P0-006）
- [ ] 7.5 严格校验本 OpenSpec，重新计算任务表 SHA-256 并确认与步骤 1.1 一致；汇总建议状态、实际工时和问题供用户手动更新任务表（P0-003～P0-006）
