# B11 V02 正式迁入与业务接线（客户端）

日期：2026-09-11。对应任务 `b11-client-machine-runtime`，客户端接线、显示缺陷修复及最终 QA 技术签收完成；不代表整个 G2 或用户运行态验收。

## 权威来源和导入边界

- 依据派发单中用户对 V02 整包的批准；ArtResource 的 `Docs/ArtPipeline/UIRequirements/B11_MachineHardware/CandidateManifest_V02.json`、AssemblyReview_V02 与 InteractionContract。
- 从清单精确选取 1 个面板和 26 张 Sprite，逐文件 SHA-256、meta SHA-256 校验，检查主工程 GUID 碰撞后导入。所有 27 个源 GUID 保留；未覆盖既有目标。
- 正式入口：`Assets/Game/Prefabs/UI/Operations/MachineHardwarePanel.prefab`，作为既有 FieldHudForm 的子层；没有新建 UIForm / UIViews / UITable 项。
- Sprite 进入 Operations/MachineHardware 的 Common、Picker、Feedback 分类；去除过程版本后缀。未导入候选 Canvas、截图、对象图标、静态示例库存、字体。
- 面板 TMP 引用改用项目既有 `Assets/Game/Fonts/UI/Operations/AlibabaPuHuiTi-3-85-Bold SDF.asset`（GUID 17c9702fd35c6d347ac1707a09be81f2），只改面板字体/材质引用，不改字体资源。共享输入/焦点 Sprite 仍使用主工程既有 GUID。
- 编辑器入口 Import Approved Machine Hardware 拒绝覆盖；Bind Machine Hardware 可重复接线。UI 子节点与 Prefab 保存经 Unity Editor API 完成。

## 已接线与明确边界

- MachineRoster 是机器/组件实例权威；选择器以实例永久 ID 选择，不以型号或名称代替身份，同型号多实例可区分，已安装实例禁用。
- FieldHudForm 通过区域同 ID 的机器选择打开面板；InitialRegionScene 将会话 Machines 注入 HUD。非机器区域对象仍走原通用概况。中枢现有对象页状态字段显示真实机器数量、连接数量和可运行数量。
- 概况读取激活、请求运行状态、供电、连接、算力占用、逻辑/容器容量及真实活动/算力等待。没有虚构任务名称、进度或电网供给。
- 现场硬件选择/安装/拆卸、重命名、运行/停止、组件启停通过既有业务门禁。远程只允许既定管理能力，不安装拆卸或首次激活。
- 确认先显示真实容量影响；资源归属/槽位/容量和机器版本再次核对。等待真实行为安全结束后变更，完成保持停止；不模拟物理完成。
- 已接受的等待请求由 MachineRoster 持有，关闭/释放 UI 不取消请求。只有显式取消待处理变更才撤销该意图，且不自动恢复运行；等待期间不允许从 UI 重新启动机器。
- 使用既有统一意图路由；子层 Cancel 优先，确认默认取消焦点，关闭后恢复来源焦点。释放订阅，不读取物理设备或逐帧轮询业务数据。
- 清除四个静态视觉库存行，只从隐藏 ItemTemplate 创建真实候选行；不在运行时重造整页美术。新增关闭/第二槽启停控件复用通过样式，修正透明命中区默认黑字不可读问题；页签图片和文字随当前页同步。
- 本段未接入完整能源网络、物理执行器、算法求值器、购买部署 UI、存档磁盘实现；服务适配输入不等于这些系统已完成。没有新增手动作业入口。

## 验证记录

- 前期 EditMode：857503ae 6/6；18a83726 7/7。最终实现变更后另行回归，不能将此前计数冒充最新。
- GF PlayMode 10e19a4e：新鲜原生 XML 2026-09-11 09:54:12Z–09:54:14Z，1/1；权威安装、关闭不取消、远程门禁和 GF 生命周期断言通过，但当时截图被主菜单遮挡，**不作为视觉证据**。
- 修正测试先等待并经 GF 关闭异步主菜单后：4637325b，原生 XML 09:59:28Z–09:59:31Z，1/1；1920×1080 截图实际显示硬件面板，随后继续修正启停可读性。
- 早期失败 334610f7 是“关闭硬件子层后整个 Launch 都不应阻挡输入”的错误测试假设；主菜单仍拥有输入层，已改为验证 HUD 自身门禁，不作为通过证据。
- 最终快速执行 EditMode：08de1e8a MachineHardwareUiEditModeTests 7/7；fd75feb2 MachineControlFlowEditModeTests 2/2；ece7e7dc AutoEraUiIntentRouterEditModeTests 2/2。未修改实现后重复使用这三组结果。
- 该包 PlayMode dfe17d0b 的断言虽通过，但 Console 暴露测试提前关闭主菜单后、Procedure 退出再次关闭同 serial 的错误，整包当时按 FAIL 处理。修正为走正式主菜单进入区域，由 WorldProcedure 返回菜单负责关闭自己的 HUD，未绕过或吞掉异常。
- 随后 21125854 失败于显式现场测试来源被真实相机输入适配器覆盖为远程；现隔离测试期间暂挂设备适配器、显式设置测试来源，finally 恢复。这是测试夹具，不宣称覆盖鼠标/相机真实命中距离。
- 最终 PlayMode 70ec395b：原生 XML 2026-09-11 10:08:49Z–10:08:53Z，1/1，通过完整菜单→区域→面板→菜单流程；Console 14 Log、0 Warning、0 Error。截图1920×1080已实际查看，非主菜单遮挡。QA只读签收入列 b11-hardware-ui-final-signoff。
- 三个正式 UI Prefab 的静态引用扫描：75 个 GUID，67 个在项目资源中解析，其余 8 个全部解析到已安装 UGUI/TMP PackageCache 脚本；无未解析 GUID。这是 GUID 级扫描，不等于所有内嵌 fileID 或视觉正确性。
- OpenSpec strict、product-profile 纯度、项目边界审计通过。代码 diff check 通过；Unity 保存的 Prefab 有标准空字段尾随空格，未为消除提示手工重写 YAML，不能声称全资产 diff check 为零。
- 任务表 SHA-256 未变：DFBE954F3907E965B25EEE5C769413C6B5176483D1982C582FD318E57068EEB5。无 Git 索引/提交，无 xlsx 或字体资源修改。

## 可查看入口

### 后续真实截图增量与最终补验

46e007d9（10:13:39Z–10:13:43Z）通过同ID区域选择自动打开面板；41b518a1（10:14:44Z–10:14:48Z）补拍概况/选择器/确认页并通过，但截图发现选择器绑定到了隐藏命中标签、标题仍为固定“效应器2”、安装确认仍写“拆卸”。已改为明确的ItemTemplateName及按当前槽/意图更新标题，增加标题回归断言；71bf496d新截图已显示正确列表条目，不能仅凭较早的测试通过忽略这些显示问题。

美术只读增量复核报告启停父Image直接压缩有透明边距的Sprite，外框仅约166×22；已复用通过的Choose按钮Art子层结构，不改PNG/字体。第二槽占用时以整卡选择、独立启停替代叠加两个按钮；补已安装第二槽的测试与截图。三组截图在同一Temp目录，尚非用户最终视觉裁决。

f9e859ef 的新鲜原生 XML 10:35:30Z–10:35:34Z 为 1/1：安装第二个货舱，选择第二槽并停用；截图已人工查看，两槽的启停按钮边框/文字清晰，第二槽选中与已停用状态一致。随后补齐第二槽整卡 Image 的 raycastTarget，并在编辑器测试中明确断言，Bind Machine Hardware 已重新保存。因此最后一项变更另交 `b11-hardware-ui-final-regression-v2` 回归，不能直接复用 f9e859ef 作为最终结果。

当前启动区域既有示例对象仍不等于完整 MachineRoster 部署入口；本测试通过真实 InitialRegion.DeployMachine/Select 的同 ID 路径，但机器创建与供电/连接是显式测试输入。没有宣称启动种子、购买部署、鼠标距离、能源网络或整个 G2 已交付。

最终 v2 回归：MachineHardwareUiEditModeTests `41511996` 7/7、MachineControlFlowEditModeTests `25e87701` 2/2、AutoEraUiIntentRouterEditModeTests `f3135234` 2/2。PlayMode `d750572d` 在域重载后无法查询作业；快速执行最初报告无法签收，但 QA `b11-hardware-ui-v2-xml-signoff` 独立核验原生 XML 最后写入 `2026-09-11T10:44:43.8366007Z`，运行区间 10:44:39Z–10:44:43Z，完整用例 `AutoEra.Tests.PlayMode.MachineHardwareUiPlayModeTests.FormalHud_UsesAuthorityAndClosesWithoutCancellingAcceptedRequest` Passed 1/1。共12/12，非历史结果；QA未重跑覆盖证据。三正式Operations Prefab可解析、Missing Scripts=0，Console Error=0/Warning=0，非PlayMode/编译，8090已释放。

最新1920×1080截图已查看：选择器当前槽名/真实货舱实例、确认安装与30→60容量变化、两货舱槽启停均有可读显示；不是用户最终视觉裁决，也不是性能基准。Temp截图SHA-256：overview `0270CB8889342963E6E4EB801737F2B8945C17A59A27E0AADF3F93165FA1FB96`；picker `1BE7E6F4D10A1A6D97DED07FD5C6A003C32387F5943883F6E80F3DFFCF9DCD29`；confirm `5B3F6F431D3999314385C5F2B8F215EE1C0A95D78AC0939D095037E1C0C19B0A`；hardware `7BB87C1A6D3ED702B7BF8396AF670F48D0526DAEB3D93858927B675EB608FF98`。本机Temp证据可再生，后续运行可能覆盖。

QA `b11-hardware-ui-final-signoff` 已实际回传：最后 XML 写入 2026-09-11T10:08:53.3229058Z，与上述新鲜测试一致；三 Prefab 均可解析，validate_find_missing_scripts=0，Console 14 Log/0 Warning/0 Error。编辑器非 PlayMode、非编译，8090 释放。没有重跑或覆盖原生 XML。

三正式 Prefab GUID：BaseCommandHubForm=284f65a93972dfb4bb26605dddf65d97；FieldHudForm=d610f46beb59504488073f6e49add27e；MachineHardwarePanel=c589d36e0df05a14d8a597dd05a08fc5。

正式 FieldHudForm → 同 ID 区域机器选择 → 概况/硬件。隔离验证可运行 `AutoEra.Tests.PlayMode.MachineHardwareUiPlayModeTests`，通过真实 GF 打开该 Form，以真实 MachineRoster 加显式环境/物理测试输入验证。
截图位于 `Temp/AutoEraTestResults/b11-hardware-runtime.png`，属于可再生本机证据，不进正式资源目录。

## 本段精确候选路径（不是自动提交授权）

工作区还包含其它任务未提交内容，本表不能替代后续 Git 集成的逐文件审查。以下只列本段接线/资源；基础 Machines 未跟踪文件也可能包含先前 B11/B12 等代码，严禁按整个目录暂存。

- `Assets/Game/Prefabs/UI/Operations/BaseCommandHubForm.prefab`
- `Assets/Game/Prefabs/UI/Operations/FieldHudForm.prefab`
- `Assets/Game/Prefabs/UI/Operations/MachineHardwarePanel.prefab`
- `Assets/Game/Prefabs/UI/Operations/MachineHardwarePanel.prefab.meta`
- `Assets/Game/Scripts/AutoEra/Editor/MachineHardwareUiSetup.cs`
- `Assets/Game/Scripts/AutoEra/Editor/MachineHardwareUiSetup.cs.meta`
- `Assets/Game/Scripts/AutoEra/Machines/MachineRoster.cs`
- `Assets/Game/Scripts/AutoEra/Machines/MachineRosterSnapshot.cs`
- `Assets/Game/Scripts/AutoEra/UI/AutoEraUiIntentRouter.cs`
- `Assets/Game/Scripts/AutoEra/UI/AutoEraUiRuntime.cs`
- `Assets/Game/Scripts/AutoEra/UI/BaseCommandHubForm.cs`
- `Assets/Game/Scripts/AutoEra/UI/FieldHudForm.cs`
- `Assets/Game/Scripts/AutoEra/UI/MachineHardwarePanel.cs`
- `Assets/Game/Scripts/AutoEra/UI/MachineHardwarePanel.cs.meta`
- `Assets/Game/Scripts/AutoEra/UI/MachineHardwarePresenter.cs`
- `Assets/Game/Scripts/AutoEra/UI/MachineHardwarePresenter.cs.meta`
- `Assets/Game/Scripts/AutoEra/World/Region/InitialRegionScene.cs`
- `Assets/Game/Sprites/UI/Operations/MachineHardware.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/BodyBackground.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/BodyBackground.png.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/ButtonDisabled.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/ButtonDisabled.png.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/ButtonFocused.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/ButtonFocused.png.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/ButtonHover.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/ButtonHover.png.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/ButtonNormal.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/ButtonNormal.png.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/ButtonPressed.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/ButtonPressed.png.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/CoreCardNormal.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/CoreCardNormal.png.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/FooterPlate.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/FooterPlate.png.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/HeaderPlate.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/HeaderPlate.png.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/IconWell.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/IconWell.png.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/OuterFrame.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/OuterFrame.png.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/ProgressFill.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/ProgressFill.png.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/ProgressTrack.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/ProgressTrack.png.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/SlotCardNormal.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/SlotCardNormal.png.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/StatusWell.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/StatusWell.png.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/TabNormal.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/TabNormal.png.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/TabSelected.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Common/TabSelected.png.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Feedback.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Feedback/FeedbackConfirm.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Feedback/FeedbackConfirm.png.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Feedback/FeedbackRejected.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Feedback/FeedbackRejected.png.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Feedback/FeedbackSuccess.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Feedback/FeedbackSuccess.png.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Feedback/FeedbackWaiting.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Feedback/FeedbackWaiting.png.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Picker.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Picker/DetailPanel.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Picker/DetailPanel.png.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Picker/ListItemDisabled.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Picker/ListItemDisabled.png.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Picker/ListItemNormal.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Picker/ListItemNormal.png.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Picker/ListItemSelected.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Picker/ListItemSelected.png.meta`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Picker/PickerFrame.png`
- `Assets/Game/Sprites/UI/Operations/MachineHardware/Picker/PickerFrame.png.meta`
- `Assets/Game/Tests/AutoEra/Editor/MachineHardwareUiEditModeTests.cs`
- `Assets/Game/Tests/AutoEra/Editor/MachineHardwareUiEditModeTests.cs.meta`
- `Assets/Game/Tests/AutoEra/PlayMode/MachineHardwareUiPlayModeTests.cs`
- `Assets/Game/Tests/AutoEra/PlayMode/MachineHardwareUiPlayModeTests.cs.meta`
- `openspec/changes/b11-machine-runtime-and-hardware-ui/evidence/client-v02-runtime-binding.md`
- `openspec/changes/b11-machine-runtime-and-hardware-ui/tasks.md`（仅实际完成项更新）
