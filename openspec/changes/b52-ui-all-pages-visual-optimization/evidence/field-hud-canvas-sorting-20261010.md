# FieldHud 详情 Canvas 排序验证

日期：2026-10-10

## 根因

`FieldHudResidentForm` 与 `FieldHudDetailForm` 都在 `UIGroup_Default` 中通过普通 `Open` 打开。`UITable` 的两行 `SortOrder` 都是 `0`，Default 组深度为 `1`；`UIFormBase.OnOpen` 又会在每个 Form 根节点上添加 `overrideSorting` 的 Canvas 并写入该值，所以两者运行期都是同序 Canvas。

详情是异步加载的独立 Form。同序 Canvas 的实际绘制顺序会受实例加载／挂接顺序影响，Hierarchy 兄弟顺序不能提供稳定的详情置顶合同。

## 修正

`FieldHudForm` 的常驻 HUD 与详情都改为 `AutoEraUiNavigator.OpenSub(this, ...)`；详情使用更高的 `subUiOrder`。GF 的 `OpenSubUIForm` 将两个 Form 设为宿主的相对 Canvas 层级，并纳入宿主生命周期。详情关闭时仍由导航器处理加载取消、参数回收和关闭动画，宿主关闭时由 `UIFormBase` 清理子界面登记。

同一检查还覆盖了两个当前存在的非遮挡子层入口：`MachineLibraryForm → WorldPlacementForm` 和 `AlgorithmEditorForm → FieldHudDetailForm`，均已改为 `OpenSub`。`AutoEraUiNavigator` 的 API 注释现在明确规定：不遮挡来源的视觉子层必须使用 `OpenSub`，普通 `Open` 只用于独立顶层或会暂停/遮挡来源的页面。

## 登记结论与防回归规则

本条记录对应用户发现的现象：点击世界可交互对象后，Hierarchy 中层级看似正确，但 `FieldHudResidentForm` 实际绘制到 `FieldHudDetailForm` 上方。问题根因是同组同序 Canvas 与异步独立 Form 的组合，而不是 Prefab 层级显示错误。

后续新增或修改 UI 打开入口时，先判断目标是否为来源页面上的视觉子层：是则必须使用 `AutoEraUiNavigator.OpenSub` 并分配明确的相对顺序；只有独立顶层页面，或会暂停／遮挡来源的页面，才使用普通 `Open`。导航测试必须同时验证相对 `SortOrder` 和 Canvas `renderOrder`，不能只依赖 Hierarchy 兄弟顺序。

## 自动验证

- Unity 编译：通过，0 errors（Job `b6adcebb`）。
- `AutoEraUiNavigationPlayModeTests`：1/1 通过（Job `64634627`）。测试同时断言详情 `SortOrder` 高于常驻 HUD，并临时把常驻 HUD 移到兄弟节点末尾后检查详情 `Canvas.renderOrder` 仍更高。
- `AutoEraOperationsUiFormPlayModeTests`：1/1 通过（Job `8de8a395`）。
- 统一子层路径后重新验证：`AutoEraUiNavigationPlayModeTests` 1/1（Job `b8987435`）、`MachineDeploymentPage_DoesNotBlockTheWorld_AndCommitsThroughItsOwnEntryPoints` 1/1（Job `b095f4b2`）、`AutoEraOperationsUiFormPlayModeTests` 1/1（Job `efe419c4`）。
- Unity Console：0 errors、0 warnings。

`FieldHudLocatePlayModeTests` 两次均在既有镜头目标断言处失败（期望 `(12, 0, -14)`，实际 `(8, 0, 12)`）；日志显示详情按钮已进入既有 `RegionInputModule.FocusSelection` 路径，该失败与 Canvas 排序改动无关，未扩大本修正范围。
