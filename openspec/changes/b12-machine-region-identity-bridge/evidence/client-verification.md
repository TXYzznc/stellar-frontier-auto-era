# B12 实施与验证证据

2026-09-11；任务 `b12-client-machine-region-identity-bridge`。

## 实施边界与快速执行检查

使用 openspec propose/apply/verify 与 grill-me，复用用户已批准的同身份/生命周期范围，不重新讨论产品规则。三候选比较见 design。身份所有权、关闭次序和失败原子性仍需专业判断，设计和实现由客户端直接执行；冻结七组回归已入 QA，允许按 RapidExecution 交快速执行并由 QA 签收。

程序集：现有 Hotfix 与 AutoEra.Editor.Tests；无新增或修改 asmdef。无组合根、注册表核心或数据修改。

## 实现映射

- 同 ID：InitialRegion.DeployMachine 只取 session.Machines，并核对注册表原实例，不调用 Allocate/Register；RegionObject.Machine 借用身份。
- 唯一区域：MachineInstance.RegionBindingOwner 是非持久 token；重复相同参数 AlreadyBound，改变参数拒绝，不产生移动。
- 生命周期：InitialRegion.RemoveObject 按是否借用身份区分清理；原 RecoverToLibrary 触发 Changed 后解绑；区域关闭不回收名册对象。MachineRoster.Disposed 使世界先退出时区域也关闭，无需改变 AutoEraWorldSession。
- 队列：复用 RegionWorkQueue 的 Request/Release，GetRequestState 不入队；Granted 是预约而非抵达。
- 13 个新用例覆盖同 ID/分配高水位/通知一致性、重复/非法/销毁/错配身份、固定载体、跨区域、回收拒绝与成功、名称同步、竞争优先级/FIFO、取消等待、目标删除、区域/世界双顺序及独立名册关闭。

## 普通编译

先核验8090主项目“星际拓荒：自动纪元”/2022.3.62f3c1，非PlayMode/暂停/编译；asset_refresh触发普通编译，随后unity_diagnose显示非编译/更新、Console Error/Warning=0，详细console.logs仅服务恢复Log，无编译错误。两份新.meta由Unity生成。未使用FSR，不运行场景、不改资产。

## 独立回归

QA任务 `b12-machine-region-identity-editmode` 已完成并实际回传：

| EditMode 类（AutoEra.Tests.Editor） | 新 job | 通过 |
|---|---|---|
| MachineRegionIdentityEditModeTests | 9ebade42 | 13/13 |
| InitialRegionEditModeTests | 95d09f29 | 5/5 |
| QAInitialRegionBoundaryEditModeTests | a586d86d | 4/4 |
| MachineManagementEditModeTests | 1ac91e7c | 4/4 |
| MachineRecoveryEditModeTests | 1f267801 | 2/2 |
| MachineExecutionContextEditModeTests | 76e668f2 | 3/3 |
| AutoEraWorldSessionEditModeTests | 9c61172d | 2/2 |

合计33/33，失败/跳过/不确定均0。QA新鲜Console Error/Warning=0；2022.3.62f3c1非PlayMode、非编译，8090已释放。本单元没有视觉或PlayMode验收要求，不将服务测试冒认为导航或UI闭环通过。

QA之后仅清理两份新.meta的空字段尾随空格，不改变GUID/导入字段值或实现；没有新增代码修订。

## 静态验证与范围

- OpenSpec strict通过。
- `python tools/audit_framework_purity.py --product-profile tools/audit_product_profile.json` 通过。
- `python tools/audit_project_boundaries.py` 通过。
- 仓库级 `git diff --check` 有既存范围外尾随空白（LanguagesTable/UITable生成文件、FieldHudForm.prefab、ResourceExportSettings.asset）；本次不改写这些文件。需单独核验本次精确候选，不能声称全仓diff通过。
- 本次16个精确候选逐文件 `git diff --no-index --check -- NUL <path>` 无空白诊断；其中exit=1仅为相对空文件存在内容差异，未把它误认作检查错误。
- 任务表SHA-256保持 `DFBE954F3907E965B25EEE5C769413C6B5176483D1982C582FD318E57068EEB5`。未写入任何xlsx，未操作Git索引或发起提交。

## 本次精确候选文件

以下为本次编辑清单，不等于授权提交整个未跟踪 Machines/Region 目录；其中已有文件包含先前 B10/B11 未提交基线。

1. `Assets/Game/Scripts/AutoEra/Machines/MachineInstance.cs`
2. `Assets/Game/Scripts/AutoEra/Machines/MachineRoster.cs`
3. `Assets/Game/Scripts/AutoEra/World/Region/InitialRegion.cs`
4. `Assets/Game/Scripts/AutoEra/World/Region/InitialRegion.Machines.cs`
5. `Assets/Game/Scripts/AutoEra/World/Region/InitialRegion.Machines.cs.meta`
6. `Assets/Game/Scripts/AutoEra/World/Region/RegionObject.cs`
7. `Assets/Game/Scripts/AutoEra/World/Region/RegionWorkQueue.cs`
8. `Assets/Game/Tests/AutoEra/Editor/MachineRegionIdentityEditModeTests.cs`
9. `Assets/Game/Tests/AutoEra/Editor/MachineRegionIdentityEditModeTests.cs.meta`
10. `openspec/changes/b12-machine-region-identity-bridge/.openspec.yaml`
11. `openspec/changes/b12-machine-region-identity-bridge/proposal.md`
12. `openspec/changes/b12-machine-region-identity-bridge/design.md`
13. `openspec/changes/b12-machine-region-identity-bridge/specs/machine-region-identity/spec.md`
14. `openspec/changes/b12-machine-region-identity-bridge/specs/machine-region-work-channel/spec.md`
15. `openspec/changes/b12-machine-region-identity-bridge/tasks.md`
16. `openspec/changes/b12-machine-region-identity-bridge/evidence/client-verification.md`

## 保留边界

本次是服务层身份及预约衔接，不创建GF实体或场景实例、不产生路径、不移动固定或轮式机器、不结算资源，不代表 P2-008/G2/B11 UI 完成。B11仍仅等UI视觉批准与完整交付。

## OpenSpec 实现复核

完整性：四项requirement、十项scenario对应上述13条新测试与20条既有回归，无遗漏的本单元实现。正确性：注册表仍返回原MachineInstance；区域清理不注销借用身份；工作位置未用作移动；竞争不抢占Owner。连贯性：采用design的窄区域投影方案，未新增程序集、依赖或第二套队列。未发现本单元CRITICAL/WARNING；全仓既存空白诊断仅保留为范围外记录。

已通过跨窗口消息实际通知制作人交付、候选与边界；客户端队列已complete --claim-next，暂无下一可执行项，B11保留UI阻塞。未归档change，未自动发起Git。
