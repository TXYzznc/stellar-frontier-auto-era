# 设计

## 约束

1. **结构不可动**：界面节点、组件与绑定只由 `Docs/Development/UI-PrefabLayouts/*.contract.json` 决定。接入代码只能声明绑定、订阅与意图，不得新增/改名节点，不得新增 `SerializeField`。门1 必须持续 33/33。
2. **Form 负责意图与显示，领域服务结算**（界面通用合同）。界面不做业务判定，也不自行超时重发。
3. **不轮询**：按变化刷新，不在 `Update` 轮询或分配。
4. **不引入新框架**：不加 DI 容器，不改 `UIFormBase` 等框架基类。

## D1 服务通道：用 UIParams 携带会话句柄

`UIParams : RefParams`，`RefParams` 已提供：

```csharp
public void Set(string key, object value);
public bool TryGet<T>(string key, out T value) where T : Variable;
```

底层按 `RefParams.Id + key` 存于 `GF.VariablePool`。因此不需要改框架。

- 新增 `AutoEraUiParamKeys`（键名常量）与 `AutoEraUiSession`（不可变会话句柄，持有 `AutoEraApplicationContext` 与可空的 `AutoEraWorldSession`）。
- 打开方（procedure 或父界面）在 `UIParams` 上注入会话句柄与打开参数。
- Form 侧由 `AutoEraUiFormBase` 提供 `TryGetSession` / `TryGetRequest`。
- **必须用 `TryGet`**：`RefParams.Get(string)` 在缺 key 时对 `null` 调 `.Value`，会抛 `NullReferenceException`。
- 生命周期由框架承担：`UIFormBase.OnClose` → `ReferencePool.Release(Params)` → `RefParams.Clear()` → `VariablePool.ClearVariables(Id)`。

不选「扩展 `AutoEraUiRuntime` 做服务定位器」的理由：引入全局可变状态，且它是 MonoBehaviour，EditMode 测试必须先造场景。现有 `BindMachineRoster` 正是该方案的雏形，其缺陷（只对中枢生效、Form 必须知道 runtime 存在）已经显现。

## D2 读取契约：按数据域划分的只读门面

### 分域而非分页

只对以下数据域建立读取模型：**被两个以上页面复用**，或**需要跨多个领域聚合**。仅被单页使用且只读单个服务的，页面直接读，不套门面（例如设置页）。

已实现域及其复用面：机器（约 6 页）、存档（约 5 页）、算法（约 5 页）、资源点/传感（约 5 页）、区域/世界对象（约 4 页）、时间与事件日志（约 3 页）。

### 形态

```csharp
public enum UiDataState { Ready, Empty, Unavailable }

public interface IXxxReadModel : IDisposable
{
    XxxSnapshot Snapshot { get; }
    event Action<XxxSection> SectionChanged;
}
```

- 快照为只读值类型或不可变类，只含页面需要的字段，不泄漏领域内部结构。
- `SectionChanged` 按区域细分，使 Form 只刷新变化区域（对应契约里的分区与 `Grp_*State`）。
- 读取模型为**会话级**：由世界会话侧持有，页面只订阅，不负责创建与释放。

### 三段状态

| 状态 | 界面表现 | 对应契约节点 |
|---|---|---|
| Ready | 显示真实内容 | 区域内容本体 |
| Empty | 无记录说明 | `Grp_<区域>EmptyState` |
| Unavailable | 显示 `—` 与具体原因，写操作禁用 | `Grp_<区域>DisabledState` |

`Unavailable` 是关键：它把「系统尚未接入」变成正常可展示状态，而不是异常或假数据，也让未就绪界面能先接成诚实空态。

### 与 AutoEraUiOperationSnapshot 的分工

| 通道 | 职责 | 方向 |
|---|---|---|
| 读取模型 | 快照与变化刷新 | 领域 → 界面 |
| `AutoEraUiOperationSnapshot` | 写操作的等待／失败／成功／长等待／请求版本 | 界面 → 领域 → 界面 |

两者正交，不合并。

## 生命周期

- `OnInit`：绑定预制控件（已有）。
- `OnOpen`：取会话 → 造读取模型（或直接读服务）→ 订阅 → 首焦点。
- `OnClose` / `OnRecycle`：退订、释放读取模型；已提交事务不随 UI 销毁取消。

## 无宿主组件处置（4.1）

判定准则（三条，可复跑）：

1. **绑定旧节点名 / 旧 `sourceId` 的适配类** → 随旧结构作废，删除。
2. **与节点无关的通用能力** → 保留，即使当前无生产引用；它对应规范家族的未来界面能力，删除等于否定已确认的设计。
3. **职责已被新实现取代、且语义已经错位** → 废弃并删除，连同其测试。

| 组件 | 形态 | 判定 | 依据与宿主 |
|---|---|---|---|
| `MachineHardwarePresenter` | 纯 C#，`IDisposable` 领域呈现器 | **保留** | 完全节点无关；底层 `MachineRoster.GetHardwareOperation` 已在；6 处 EditMode 用例覆盖。宿主＝机器硬件/管理界面（04/05 家族，硬件域接入时） |
| `AutoEraDangerConfirmationView` | MonoBehaviour，全字段由预制体接线 | **保留** | 规范 06/17 的能力；逻辑核 `AutoEraConfirmationDescriptionPresentation` 已有用例，本类只是它的表现层，不引用任何旧节点名。宿主＝需要危险确认的 Form 的 Overlay（退出流程、规则禁用、回档确认） |
| `AutoEraHoldToConfirmView` | MonoBehaviour，指针三接口 + `AutoEraHoldToConfirmTracker` | **保留** | 规范 06 的 1.2 秒长按门，已有 EditMode 用例。宿主＝危险确认按钮节点 |
| `AutoEraReduceMotionEntry` | MonoBehaviour 开关，静态 `IsReducedMotionEnabled` / `ReducedMotionChanged` | **保留** | 规范 17 无障碍能力，无节点名依赖。宿主＝设置界面（设置域接入时）；静态状态当前无读取方，随首个动效消费点接入 |
| `AutoEraUiVisualTimer` | `internal` MonoBehaviour，自建隐藏根节点 | **保留** | 临时提示的定时隐藏；自建宿主（`EnsureInstance`），无外部引用即无宿主依赖 |
| `AutoEraUiCancelIntentProxy` | MonoBehaviour，`ICancelHandler` → `AutoEraUiRuntime.DispatchIntent(Cancel)` | **保留（宿主属场景配置）** | `AutoEraUiIntentInputAdapter` 是**纯提交接口**（不读设备），因此「EventSystem 的 cancel 事件 → 语义意图」必须有人转发，本类正是该适配器。宿主＝场景内 EventSystem 节点（或常驻 UI 根）；**属场景/预制体配置，不在代码里创建** |
| `AutoEraHubPageSelection`（含 `AutoEraHubPage` 枚举） | 纯 C#，5 页循环 | **废弃（已删）** | 职责已被 `BaseCommandHubForm` 的 7 页页常量 + `AutoEraUiPageRequest` 取代；其枚举只有 5 页而中枢实际 7 页，**语义已经错位**，保留会与新页序冲突 |

已删除内容留痕：`Assets/Game/Scripts/AutoEra/UI/AutoEraHubPageSelection.cs`（含 `.meta`）与
`AutoEraUiOperationContractsEditModeTests.HubPageSelection_CyclesOnlyAcrossTheFiveFrozenHubPages`。
其余 6 项均保留，其中 5 项的实际接线在前置域就绪之前不会发生——这属正常，不构成缺陷。

## 验证

- 门1 契约自检 33/33（结构未被破坏）。
- `run_project_checks.py` 5/5。
- 全界面 PlayMode 冒烟 33/33。
- 新增数据流测试：构造领域变化 → 断言界面更新 → 关闭 → 断言退订无回调。
