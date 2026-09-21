# 设计

## 约束

1. **结构不可动**：本变更不改任何界面契约或预制体结构，门1 必须持续 33/33。
2. **不新造领域能力**：只接线。`RegionPlacement` / `RegionPlacementPreview` / `DeployMachine` /
   `RegionNavigation.Bind` / `MachineExecutionContext` / `AlgorithmInstanceService` 全部复用，不重写。
3. **领域是权威，视图与运行时是派生**：机器是否已部署由 `MachineRoster` + `InitialRegion` 决定；
   视图与运行时是它的表现与派生，缺失或失败**不得**回滚领域事实。
4. **不轮询**：走已有事件（`ObjectsChanged` / `MachineInstance.Changed` / `RegionNavigation.Tick`）。

## D1 落位是「校验事务」与「领域提交」两段，且提交不做结算

现成零件已经就是这个形状：`RegionPlacementPreview` 只做校验，注释写着
「Construction/payment belongs to the caller」；`RegionInputModule.BeginPlacement(size, confirmed)`
负责指针与轮廓，确认时回调调用方。

所以**不引入新的落位事务类型**，而是补上调用方：

```csharp
public sealed class MachineDeploymentFlow : IDisposable
{
    bool TryBegin(PersistentId machineId, out string reason);  // 前置校验 + 取尺寸
    Vector2 Size { get; }
    RegionPlacementPreview Preview { get; }   // UI 读 IsValid / Reason / Position / Yaw
    MachineDeploymentOutcome Commit();        // 只调 DeployMachine，不结算、不扣费
    void Cancel();  void Dispose();
}
```

- `TryBegin` 的前置：机器在花名册里且**未部署**、区域活跃、定义可用。
  「未部署」是硬条件——`DeployMachine` 对已部署机器返回 `AlreadyBound`，但流程层就应挡住，
  否则玩家会拿到一个「点了没反应」的按钮。
- 尺寸来源：机器定义的占地。当前 `MachineDefinition` 没有尺寸字段，
  集成测试统一用 `new Vector2(1.8f, 2.4f)`，因此本变更**把占地作为定义的一部分显式化**
  （数据表加列或按 `CanMove` 给常量），不散落在调用点。**这是本变更唯一需要动数据表的地方**，
  且只加列不改既有列。
- 指针驱动：给 `RegionInputModule` 增加「驱动一个已存在的预览」的入口，
  保留原 `BeginPlacement(size, confirmed)` 给编辑器证据工具用。
  两套都改成一个预览的所有者，避免同时存在两个预览。

## D2 视图必须「绑定已部署对象」，不能复用注册路径

`RegionObjectView.Initialize(region)` 的语义是**注册**：它用自身序列化字段
（`_kind` / `_displayName` / `_footprint` / transform 位置）调 `region.Register(...)`，并让 `Model` 指向新对象。
`Release()` 会 `region.Remove(Model.Id)`。

对已部署机器直接用它会**重复注册**——`DeployMachine` 已经建了 `RegionObject`，
再 `Register` 一次就会出现两个同 id 意图的对象（或撞 id）。

因此新增绑定路径（同一组件上加方法，不新建组件类）：

```csharp
public void BindDeployed(InitialRegion region, PersistentId id);  // 从 region 取已存在的 RegionObject，不注册
```

- `BindDeployed` 只认**已经存在**的区域对象；取不到就抛（调用方必须先 `DeployMachine` 成功）。
- `Release()` 需要区分两种来源：注册来的对象**要**移除；绑定来的对象**不得**移除
  ——机器的存续由领域决定，视图只是它的呈现。用一个内部标志记录来源，而不是靠 `Model != null` 猜。
- 视图销毁时若领域对象仍在，机器保持已部署（玩家看到的是「机器还在，只是没有表现」，
  这是可展示的降级，不是数据损坏）。

## D3 导航绑定失败是**可展示状态**，不是异常

`RegionNavigation.Bind` 的前置很严：导航面就绪、机器同身份已部署、位置误差 ≤0.01、子节点有 `MotionRig`、
定义 `CanMove`。这些条件在生产里**都可能不满足**（地面网格缺失、预制体没挂 MotionRig、
机器定义不可移动），而现在它一律 `throw`。

接线后必须区分两类：

| 情形 | 处理 |
|---|---|
| 机器定义不可移动（`CanMove == false`） | **正常**：不绑导航，运行时用「无导航」形态（见 D4） |
| 可移动但导航面未就绪 / 预制体缺 `MotionRig` / 位置不匹配 | **降级**：记录原因，机器保持已部署但不可自主移动，界面显示原因 |

所以不在 `RegionNavigation` 里改判据（那是移动域的合同），而是在接线层捕获并转成状态。

## D4 运行时按区域持有，且**不进存档**

按已确认的候选 A：部署成功后创建运行时；生命周期跟随区域。

```
InitialRegionScene
  └ RegionMachineRuntimeRegistry (IDisposable)
       └ per machine: RegionMachineRuntime (IDisposable)
            ├ MachineExecutionContext        （任务队列、算力池、传感器集）
            ├ AlgorithmInstanceService       （草稿、模板、待绑定）
            ├ AlgorithmMachineAdapter        （把算法意图接到机器权威）
            └ MachineNavigation?             （可移动机器才有；来自 RegionNavigation.Bind）
```

- **候选 A 的落点**：不是 `DeployMachine` 内部（那是纯领域方法，不接触 GameObject），
  而是**部署成功后**由接线层创建——`AlgorithmMachineAdapter` 正需要
  `MachineNavigation`（表现层产物）与 `InitialRegion` 两样东西。
- **不进存档**：`MachineExecutionContext` 持有任务队列与传感器订阅，都是运行时对象，
  不可序列化也不该序列化。存档只记「哪些机器在哪个区域的哪个位置已部署」（已由
  `RegionObject` + `machine.Deployed` + `RegionBindingOwner` 表达）。区域就绪时重建运行时，
  重建后机器回到空闲态——这是可接受的语义，因为任务队列本身也没有存档合同。
- **不可移动机器的运行时**：`MachineExecutionContext` 需要算力池与传感器集，这两样与移动无关，
  因此不可移动机器同样拥有运行时；只有 `AlgorithmMachineAdapter` 的导航依赖需要区分
  （不可移动机器不绑导航、也不产生移动步骤）。
  **若 `AlgorithmMachineAdapter` 目前硬依赖非空导航**，则本变更给它一个显式的
  「无导航」形态（可空 + 明确的 `IsSafe` 语义），而不是给不可移动机器伪造一个导航对象。

## D5 预制体解析走数据表，缺失时降级为「无视图部署」

`MachineDefinitions` 数据表已有 `Prefab` 列（值如 `Machines/WheeledCarrier`，相对
`Assets/Game/Prefabs/Entity/`）。接线层据此解析实体预制体，**不硬编码机器→路径映射**。

预制体缺失或路径无效时：领域部署**仍然成立**（`DeployMachine` 已成功），
只是不产出视图与运行时，并记录可展示原因。理由是 D3 的同一条原则——
领域是权威，表现缺失不该否定它。

## D6 界面回填的口径

| 界面 | 回填内容 | 仍不可用的部分与原因 |
|---|---|---|
| `WorldPlacementForm` 机器部署页 | 选机器 → 落位 → 确认 → 结果 | 建造放置与世界绑定页仍不可用（不属本变更） |
| `MachineLibraryForm` 部署按钮 | 改为启动本流程 | 安装/卸载/升级/出售/改名仍依赖未接入的组件与库存域 |
| `AlgorithmEditorForm` / `AlgorithmLibraryForm` / `AlgorithmBindingForm` / `NodeComponentPickerForm` | 由「域未接线」转为读真实实例服务 | 节点库与草稿编辑的完整体验按算法域自身合同推进 |
| `BaseCommandHubForm` 远程机器详情 | 机器运行时状态（任务、算力占用） | 统计聚合仍不可用 |

## 验证

- 门1 契约自检 33/33（结构未被破坏）。
- `run_project_checks.py` 5/5。
- EditMode/PlayMode 既有全部回归保持通过。
- 新增数据流测试：部署成功 → 区域出现对象 → 视图绑定不重复注册 → 运行时出现 →
  算法界面读到 `Ready`；以及三条降级路径（导航面未就绪、预制体缺失、机器不可移动）各自可辨。
