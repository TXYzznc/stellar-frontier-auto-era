# B13 客户端验证记录

## 已核验基线（2026-09-11）

- `openspec validate b13-wheeled-navigation-and-avoidance --strict` 已通过；proposal/design/两份 specs/tasks 完整，不代表实现验收完成。
- 现有 Hotfix 与 Editor/PlayMode 测试程序集复用；使用已安装 `com.unity.modules.ai`，无包/asmdef/全局设置修改。
- 8090 health 与 project_get_info 核验主工程“星际拓荒：自动纪元”/2022.3.62f3c1；普通 asset_refresh 后非 PlayMode、非编译，Console Error=0。
- `rapid-executor/b13-navigation-service-editmode` 已完成：MachineNavigationEditModeTests `86800c7f` 13/13、MachineRegionIdentityEditModeTests `5c734e8d` 13/13、MachineExecutionContextEditModeTests `1c6453b3` 3/3，合计 29/29；失败/跳过 0，Console Error/Warning=0，8090 已释放。覆盖可控时钟和身份兼容，不代替真实场景验收。

## 权威与范围

矿脉接近为有效区域，水枪包络由能力输入判定，未写死射程。移动是载体活动，不增加效应器。10/5 算力复用 MachineComputePool；预约复用 RegionWorkQueue；区域位置更新保持机器永久 ID。

## 正式入口差距

只读检查 `Assets/Game/Scene/InitialRegion.unity` 的 `m_NavMeshData` 为 0。`InitialRegionScene.InitializeRuntime` 当前经 GF 加载 RegionObjectView，机器种子仍是区域代理加载，不自动从本导航测试实例接管。独立 PlayMode 将引用正式轮式 Prefab，但不会因此声称正式区域已有导航调用入口。

制作人已在原派发追加该场景精确授权。通过 Unity 场景工具检查 Launch 与 InitialRegion 均无未保存修改后，使用 serialized property 工具绑定 `_navigationGround` 至既有“区域地面”MeshFilter（场景 fileID 1321054550），随后受控保存 InitialRegion 并回到 Launch。半径/高度为 1.6/2；未改 Prefab 或全局配置。该场景在当前 Git 现场本来为未跟踪，不能用空的 `git diff` 声称无改动。正式运行时建立/卸载验证尚待执行。

## 独立 PlayMode 首轮诊断

- 首次请求 job `4b959973` 在 Enter/ExitPlayMode 域重载后不再可查；不能引用旧 `test_get_last_result`。
- 新鲜原生 `TestResults.xml`（03:39:59–03:40:19 UTC）显示 0/1：末尾 `LogAssert.NoUnexpectedReceived` 将 UnitySkills 自检普通 Log 也视为失败。已移除这一过严全日志断言，保留 TestRunner 默认 Error/Exception/Assert 失败门禁，不关闭错误检查。
- 此轮运行断言执行到末尾，输出 3642 个采样、导航/轮组调用累计 119.1894ms、托管分配 0、绕障最大横向偏移 6.02069m、两机最近距离 3.20065m；这是诊断数据，不作为通过结论。
- 第二轮 `90920c09` 的新 XML 在 03:44:48–03:45:14 UTC 记录未在测试 25 秒墙钟上限内完成（状态仍 Moving），未算通过。将测试容许墙钟改为 45 秒并增加姿态/剩余路径/速度/时间缩放/规划次数失败诊断；产品 3/10/30 秒规则未放宽。
- 第三轮请求 `ad592991` 对应新鲜 XML：03:47:56–03:48:15 UTC，`FormalCarrier_NavigatesAvoidsAlignsAndCancels_WithoutMutatingAssets` **1/1 Passed**。真实障碍绕行、两机相向避让、朝向、轮周位移、取消后 0.5 秒位置稳定及清理通过。最终测量值见 `playmode-metrics.json`；5 张 navigation-*.png 为实际相机渲染。测量仅该规模导航 Tick＋Motion Sample，不包含原生 NavMesh 或整帧总开销。

## 检查状态

该段为早期检查点，最终结果见下方“最终核验”；不以历史 B12 测试替代 B13 新鲜结果。未操作 Git 索引或任何 xlsx。

## 正式区域集成测试诊断

首轮 `e7ee6c7d` 在 TestRunner 创建的空测试场景中未找到 Launch；测试现沿用项目既有 MachineGameDataIntegrationTests 的只在空测试场景加载 Launch 方式。随后 `44e7b9ef`、`30e01385` 在场景加载完成前查询入口失败；已改用异步操作并显式等待 isDone，不放宽入口非空断言。Unity 2022.3 场景加载 API 返回不代表立即加载完成，参考 [Unity 2022.3 LoadSceneInPlayMode](https://docs.unity3d.com/ja/2022.3/ScriptReference/SceneManagement.EditorSceneManager.LoadSceneInPlayMode.html)。这些失败均不作为通过结果。

`903ee656` 的新鲜 XML（03:54:26–03:54:28 UTC）已加载正式入口，但移除障碍后的 0.25m 地面采样失败。扩大诊断采样并要求水平误差小于0.1m后，`a86e0a96`（04:00:55–04:01:12 UTC）确认地面存在，但实际导航未抵达。根因是运行时生成使用半径/3的默认体素；1.6m半径产生约0.533m体素，与驱动0.25m目标采样容差不匹配。RegionNavigation现明确使用0.1m体素，测试恢复0.25m精度门禁，并继续验证障碍中心2m内不可走、真实到达、卸载和重入。没有修改全局导航设置、正式Prefab或抵达容差；上述两轮不算通过。

修复后请求 `1bf0db89` 对应原生 XML **04:02:20–04:02:29 UTC，1/1 Passed**，用例 `FormalRegion_BuildsBindsMovesRebuildsAndReenters`。两次建立正式 InitialRegion 的 GF 实体/导航入口，验证障碍增删后的阻挡与恢复、正式 WheeledCarrier 运行实例绑定名册ID、实际移动/抵达、静止帧不重复建网、Release 移除导航面与绑定，再次初始化成功。未修改 Prefab；运行实例仅为隔离测试对象，正式入口不会自动创建测试机器或发送演示指令。

## 本批精确候选路径（不操作 Git）

- `Assets/Game/Scripts/AutoEra/Machines/MachineNavigationContracts.cs` 与 `.meta`
- `Assets/Game/Scripts/AutoEra/Machines/MachineNavigation.cs` 与 `.meta`
- `Assets/Game/Scripts/AutoEra/Machines/UnityMachineNavigationDriver.cs` 与 `.meta`
- `Assets/Game/Scripts/AutoEra/Machines/MachineExecutionContext.cs`（必要活动所有权接入；原有文件）
- `Assets/Game/Scripts/AutoEra/World/Region/InitialRegion.Navigation.cs` 与 `.meta`
- `Assets/Game/Scripts/AutoEra/World/Region/RegionNavigation.cs` 与 `.meta`
- `Assets/Game/Scripts/AutoEra/World/Region/RegionWorkQueue.cs`（公开作业区域及归属查询；原有文件）
- `Assets/Game/Scripts/AutoEra/World/Region/InitialRegionScene.cs`（导航初始化/更新/释放与地面字段；原有文件）
- `Assets/Game/Scripts/AutoEra/Motion/Adapter/MachineNavigationMotionAdapter.cs` 与 `.meta`
- `Assets/Game/Tests/AutoEra/Editor/MachineNavigationEditModeTests.cs` 与 `.meta`
- `Assets/Game/Tests/AutoEra/Editor/MachineNavigationIntegrationTests.cs` 与 `.meta`
- `Assets/Game/Tests/AutoEra/Editor/RegionNavigationIntegrationTests.cs` 与 `.meta`
- `Assets/Game/Scene/InitialRegion.unity`（仅已授权导航引用/参数序列化，保留原meta/GUID）
- 本 change：`.openspec.yaml`、`proposal.md`、`design.md`、`tasks.md`、两份 `specs/*/spec.md`、`evidence/client-verification.md`、`evidence/playmode-metrics.json`、五份 `evidence/navigation-*.png`。

Unity生成的九份新增代码meta仅移除空字段尾随空格，GUID未变。已有文件/场景在工作区可能本就未跟踪，不能将它们完整内容都归为B13新增，也不能一键暂存父目录；后续Git仅由用户手动触发。

## 最终核验（2026-09-11）

- 快速执行最终兼容包：`MachineNavigationEditModeTests` job `07d19ad5` **15/15**；`MachineRegionIdentityEditModeTests` job `caabaec8` **13/13**；`MachineExecutionContextEditModeTests` job `fa1f0715` **3/3**。合计31/31，含新增管理安全停机、公共等待位置只规划一次后释放移动预算断言；全部0失败/跳过。快速执行已释放8090。
- 正式集成1/1与隔离动作/避障1/1证据分别见前文，不将31项服务回归冒充正式集成测试。
- 正式 InitialRegion `validate_scene`：Error=0、Warning=0，无Missing Script/Prefab；两项Info仅是既有8个Selection和8个Visual同名子节点。场景7个非内置GUID全部可解析，地面字段在正式集成中实际使用。检查后返回已保存Launch，未另保存场景。
- 普通刷新编译及新鲜Console Error=0、Warning=0；最终非PlayMode、非编译。框架纯度（product profile）、项目边界审计与OpenSpec strict通过。
- 精确12个C#文件、9份新增meta及本change文本的空白检查无诊断。InitialRegion.unity全文件检查有24个Unity生成的m_Name/m_EditorClassIdentifier空值尾随空格；本次新增三项导航序列化字段无该问题。保留序列化输出，不手写场景YAML，不声称全仓或该场景全文件diff-check零问题。
- 任务表SHA-256仍为`DFBE954F3907E965B25EEE5C769413C6B5176483D1982C582FD318E57068EEB5`；未写任何xlsx，未操作Git索引/提交。
- 性能仅报告该隔离场规模：3665次服务Tick/轮组采样，托管分配0，累计107.7543ms；不包含原生NavMesh、建网或全帧成本，不保证大规模交通吞吐。实测最大绕障偏移6.0317m，两机最小间距3.201m。已人工查看实际navigation-detour.png，正式载体在橙色测试障碍外绕行。
- 完整性/正确性/连贯性：已实现载体固有导航、算力生命周期、公共区域预约、真实运动投影及正式区域建网/释放入口；没有新增移动效应器、固定水枪射程、自动演示或玩家算法语言。B11机器UI仍待用户视觉批准/完整交付；不据此宣告B11、G2或用户可视验收完成。
- 已通过跨窗口接口实际发送本批技术完成回传至制作人`019ff522-c33e-76f2-98c9-f51e7f80ca95`，接口返回成功；8090释放。按队列完成B13，B11阻塞不自动解除，无Pending；不等待“已阅”。
