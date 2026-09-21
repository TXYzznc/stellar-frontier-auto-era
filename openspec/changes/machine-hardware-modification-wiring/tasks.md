# 任务

## 1. 请求与显示层

- [x] 1.1 新增 `AutoEraHardwareRequest`：机器身份 ＋ 硬件类别 ＋ 槽位序号 ＋ 是否拆卸 ＋ 要装入的组件身份。
      （**刻意不带来源**。来源由确认页按机器部署状态推导（未部署 → `ManagementOrigin.Library`，
      已部署 → `ManagementOrigin.Field`），因为那正是领域 `HardwareGate` 的判据。
      让调用方传一个可能与机器状态矛盾的来源，只会制造「界面说整备、领域说现场」的假分歧。
      用例 `Request_DeliberatelyCarriesNoOrigin` 用反射把这条约定钉住。）
- [x] 1.2 `AutoEraUiFormat` 增加 `Slot(kind,index)` / `SlotKind(kind)` / `ManagementResult(result)`。
      （槽位名字与拒绝原因**只有一处**：整备页、确认页与将来的组件选择器必须说同一句话，
      否则同一格会出现「核心槽 1」与「核心 1 号槽」两种说法。
      拒绝原因必须是可展示的中文而不是枚举名——把 `Occupied` 端给玩家等于没解释。）

## 2. 17-硬件修改确认页

- [x] 2.1 `OperationDialogForm` 改为**参数驱动**：带 `AutoEraHardwareRequest` 打开时落在
      HardwareConfirm 页（页码 6），渲染「本次改动」（机器／槽位／卸下／装入／库存去向）与
      「运行影响」（当前行为／能力变化／保持激活／算法绑定／提交状态）两栏。
      没有请求时呈现 Disabled 并说明「本界面由调用方带参数打开」——其余页是各自域的确认页，
      随各自的域接入，不在这里编造内容。
- [x] 2.2 提交交给 `MachineHardwareOperation`（`MachineRoster.GetHardwareOperation(id)`）：
      界面不自己判断「什么时候能改」，只把意图交出去，状态与拒绝原因从 operation 读回来。
      已提交（Waiting／Completed／Rejected）的意图不再接受第二次提交；
      「保留配置」（不提交并关闭）始终可用，它是这一页的安全出口。
      **关闭对话框不取消已提交的修改**（规格：未成功前组件归属不变、返回页面不自动取消等待）。
- [x] 2.3 组件型号显示名统一走目录（`MachineCatalog.TryGetComponentRow`），
      拿不到就退化成型号编号，**不编造名字**。与整备页对同一件组件用同一条规则。
      （第一次实现漏了这一步，确认页只显示「型号 2001（实例 2）」而整备页显示
      「基础计算核心」——同一件东西在两页里长得不一样，用例把它抓了出来。）

## 3. 整备页槽位选择与入口

- [x] 3.1 整备页的**槽位行可选中**（`HasSlotSelection` / `SelectedSlotLabel` / `SelectedSlotComponent()`）；
      摘要行（已装组件／库存候选／一键卸下影响）传 null 回调，保持纯展示且不抢焦点。
- [x] 3.2 「安装或拆卸」从 `ApplyUnavailableActions` 的无条件禁用名单中移出，
      改由 `RefreshSlotActions` 按选中情况决定：**只有选中已占用槽位时可点**。
      选中空槽位时明说装入方向缺 12-组件选择器，而不是静默什么都不做。
- [x] 3.3 槽位选中态用「它属于哪台机器」自我校验（`_slotSelectionMachine`），
      换机器或被移除时自动作废，不需要在每条切换路径上补清理代码。
- [x] 3.4 花名册变化（`MachineDomainSection.List`）也要重画详情与整备页：
      装机、撤收都会改到**选中机器自己的数据**，只刷列表会留下过期的详情。

## 4. 契约与生成器

- [x] 4.1 `Tools/ui_spec_to_contract.py` 的 `EXTRA_BINDINGS` 为 `OperationDialogForm` 增加
      `_hardwareConfirmKeepButton` / `_hardwareConfirmCommitButton`。
- [x] 4.2 `Tools/ui_contract_to_form_script.py` 的 `HANDWRITTEN` 收入 `OperationDialogForm`
      （生成器继续供字段，业务逻辑手写）。
- [x] 4.3 新增 `Tools/_unity_refresh_bindings.py`：加绑定之后必须跑
      「按契约刷新所有页面绑定（不改结构）」，这是把新绑定写进预制体的正式入口。
      （理由：原有的「从契约重建预制体」「按契约刷新绑定（不改结构）」只作用于
      `DefaultContractPath`，对别的 Form 跑它们会**静默地什么都不做**。）
- [x] 4.4 **预制体结构未变**，只补了两个绑定引用；门1 的 L1/L2/L3 全绿即为证据。

## 5. 验收

- [x] 5.1 编译 0 错。
- [x] 5.2 门1 契约自检全绿（33 份契约满足 L1/L2/L3）；`run_project_checks.py` **5/5 PASS**。
- [x] 5.3 新增 `HardwareOperationEditModeTests`（**10/10**）：请求的两种意图与自我描述、
      请求不带来源（反射）、槽位名字三类别稳定、每一种拒绝原因都可读且互不相同。
- [x] 5.4 新增 `OperationDialogHardwareConfirmPlayModeTests`（**1/1**）：
      真实运行时下带请求打开 → 落在 HardwareConfirm 页 → **确认之前机器一点没动**（槽位与归属都不变）
      → 提交后领域真的把组件拆下来（槽位空、组件回到组件库成为散件）→ 不可二次提交、
      安全出口始终可用。库中机器走 Library 来源；来源传错会被领域以 InvalidOrigin 拒绝，用例即红。
- [x] 5.5 新增 `MachineLibrarySlotSelectionPlayModeTests`（**1/1**）：
      未选槽位时入口不可点 → 选中装着核心的那一格后入口变可用 →
      点击打开 17 的确认页，且**此时机器仍未被改动**。
- [x] 5.6 回归：EditMode **23 类**全绿（新增 1 类；`HardwareOperationEditModeTests` 10/10，
      `MachineReadModelEditModeTests` 12/12、`ComponentReadModelEditModeTests` 13/13、
      `MachineCatalogEditModeTests` 16/16 等维持原状）；PlayMode **11 套**全绿（新增 2 套，
      既有 9 套各 1/1）；进 Play Mode 的 EditMode 类 `AutoEraStartupFlowEditModeTests` **6/6**。

## 6. 边界

本变更不实现：**装入方向**（要先经 12-组件选择器取得散件身份）、一键卸下全部、
升级／改名／出售（各自要经 11／17 家族的对话框）、以及整备环境的现场激活入口。

不修改预制体结构、不改 `UIViews` 登记、不改数据表、不改本地化表。

## 7. 过程中踩到的两件事（留痕）

- **同一件组件的显示名必须只有一个规则。** 确认页最初写的是 `"型号 " + Definition.Id`，
  而整备页走目录取名字，于是同一颗组件在整备页叫「基础计算核心」、在确认页叫「型号 2001」。
  这不是「测试期望写错了」——期望本身是对的，实现不一致才是缺陷；
  修法是让确认页也走 `MachineCatalog`，拿不到才退化成编号。
  （用例先红后绿的顺序值得记住：断言写「应当显示名字」时，先确认另一页是不是已经这么做了。）
- **在既有 PlayMode 类里加第二条用例会重新引导框架**（上一批已踩过），所以这次两条新的
  端到端链路各自成类：`MachineLibrarySlotSelectionPlayModeTests` 与
  `OperationDialogHardwareConfirmPlayModeTests`。
