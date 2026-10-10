# B52 全量结构推广与验证记录

日期：2026-10-10

## 范围

- 覆盖矩阵：40 个 Prefab（38 个 Form、2 个共享 Item），分为系统/流程、目录/选择、生产/现场、算法、中枢/阅读五个结构家族。
- 页面预览：`evidence/preview-index.json` 收录 105 个页面根；105/105 PNG 存在，分辨率全部为 1920×1080。
- 正式美术、效果图资源、动效参数仍属于独立阶段，不在本次结构实施验收内。

## 实施结果

- `art/build_rollout.py` 生成并应用 B52 家族布局合同；批次 2–6 均已重新应用到现有 Prefab。
- 仓库长列表正文节点补齐 `LayoutElement.preferredWidth`，消除 VerticalLayoutGroup 下的 L5 宽度缺口。
- 算法编辑页按 fixed/stretch 的 `sizeDelta` 语义修正三栏高度、问题区、状态工具条和诊断工具条，保留节点库、画布、检查器与撤销/重做/删除动作的运行时接线。
- 算法运行期测试继续生成 `Assets/Screenshots/algorithm-editor-empty.png` 与 `Assets/Screenshots/algorithm-editor-ready.png`，两张均为 1920×1080；连线、选中、删除、应用和诊断入口由同一测试链路验证。
- `ProgressReportForm`、`OperationFeedbackForm` 的显式返回/关闭按钮改为直接 `CloseSelf()`；两者在 `UITable` 中 `EscapeClose=false`，显式动作不再被取消意图的 EscapeClose 门控拦截。
- 结构预览导出在释放 `RenderTexture` 前清除 Camera 绑定，批量截图不再向 Console 写入 RenderTexture 错误。

## 自动验证

### Gate1 与静态检查

- Gate1 报告：`[Gate1] 通过：Docs/Development/UI-PrefabLayouts 下全部契约满足 L1/L2/L3/L5。`
- `evidence/gate1-batch2.txt` 至 `gate1-batch6.txt` 均为 0 行问题。
- `AutoEraContractPrefabGate1EditModeTests` 最终重跑 Job `1e6b97b0`：1/1 通过。
- Unity 编译：成功，0 errors。
- Unity Console：0 errors。
- `python tools/audit_project_boundaries.py`：通过。
- `python tools/audit_ui_prefabs.py --write`：完成并更新 `Docs/Development/UI-PrefabLayouts/UIOptimizationAudit.md`。
- `python tools/audit_framework_purity.py`：仍有 15 项仓库既有问题（Launch 场景启用、历史 sample-launch 标识、sample 目录）；这些问题不由 B52 结构推广引入，本记录不将其标记为已修复。

### Unity Test Runner

以下测试均为 1/1 通过：

| 测试 | Job ID |
|---|---|
| `AutoEraContractPrefabGate1EditModeTests` | `1e6b97b0`（最终重跑；此前 `c01ce4fc` 亦通过） |
| `AutoEraAllUiFormsPlayModeTests` | `29dd3430`（最终重跑；此前 `efe6242a` 亦通过） |
| `AutoEraUiNavigationPlayModeTests` | `740efe5e` |
| `AutoEraOperationsUiFormPlayModeTests` | `6c8957dc` |
| `EditorCanvas_LibraryAddConnectSelectDelete_EndToEnd` | `db37c6b4` |
| `MachineDeploymentPage_DoesNotBlockTheWorld_AndCommitsThroughItsOwnEntryPoints` | `ec3f315c` |
| `ComponentLibrary_RendersRealRosterRows_AndFollowsTheComponentAcrossPages` | `b3e80c05` |
| `Settings_AllThreePagesAreWired_AndAudioAndCameraWritesReachTheirRealBackends` | `b8b31fd1` |
| `ThreeWorkspaces_PreserveSelectionNavigationAndWorldInput` | `c4069a3c` |

## 验收状态

B51 代表结构已有用户确认，记录见 `../../b51-ui-representative-layout-prototypes/evidence/user-acceptance-20261010.md`。B52 全量结构已完成实现、门禁和自动验证，`tasks.md` 的 6.5 仍等待用户对全量结构作最终确认；正式美术任务 7.x 保持未开始。
