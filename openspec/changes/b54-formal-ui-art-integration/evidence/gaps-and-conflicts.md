# 缺失独立页与素材冲突

本轮只调整现有界面。以下两项没有另建独立页面根，使用已有界面或模式承载。

| 设计项 | 当前承载 | 本轮处理 |
|---|---|---|
| 05 / RemoteMachine | 04 族 BaseCommandHubForm / Panel_PageRemoteMachine | 保留共享页，不新建 Form |
| 13 / AlgorithmDiagnosis | 现有算法编辑器诊断模式 | 保留已有模式，不新建 Form |

效果图包对以下 19 项给出了不同素材路由。按用户已确认边界，本轮保持正式设计文档的 ID 和工程资源引用；不执行这些互换。原 PNG 字节均未修改。

| 正式文档 ID | 当前工程源文件 | 效果图包建议使用 ID |
|---|---|---|
| `S02-002` | `scr-02-002-category-emblem-display@128x128.png` | `S02-004` |
| `S02-003` | `scr-02-003-category-emblem-control@128x128.png` | `S02-005` |
| `S02-004` | `scr-02-004-category-emblem-exit@128x128.png` | `S02-006` |
| `S02-005` | `scr-02-005-display-preview-standard@560x420.png` | `S02-002` |
| `S02-006` | `scr-02-006-display-preview-reduced@560x420.png` | `S02-007` |
| `S02-007` | `scr-02-007-audio-category-tile@256x256.png` | `S02-008` |
| `S02-008` | `scr-02-008-exit-badge-confirm@96x96.png` | `S02-003` |
| `M007-track-thin-disabled` | `ui-master-007-track-thin-disabled@512x16.png` | `M007-knob-normal` |
| `M007-track-thin-warning` | `ui-master-007-track-thin-warning@512x16.png` | `M007-knob-focus` |
| `M007-fill-thin` | `ui-master-007-fill-thin@512x16.png` | `M007-track-thin-warning` |
| `M007-knob-normal` | `ui-master-007-knob-normal@64x64.png` | `M007-fill-thin` |
| `M007-knob-focus` | `ui-master-007-knob-focus@64x64.png` | `M007-segment-meter` |
| `M007-segment-meter` | `ui-master-007-segment-meter@512x32.png` | `M007-track-thin-disabled` |
| `M014-resource-biomass` | `ui-master-014-resource-biomass@128x128.png` | `M014-seed-red` |
| `M014-energy-power` | `ui-master-014-energy-power@128x128.png` | `M014-resource-biomass` |
| `M014-seed-wheat` | `ui-master-014-seed-wheat@128x128.png` | `M014-energy-power` |
| `M014-seed-red` | `ui-master-014-seed-red@128x128.png` | `M014-crop-wheat` |
| `M014-crop-wheat` | `ui-master-014-crop-wheat@128x128.png` | `M014-bundle-seed-wheat` |
| `M014-bundle-seed-wheat` | `ui-master-014-bundle-seed-wheat@128x128.png` | `M014-seed-wheat` |

详细源路径和指纹见 [覆盖矩阵](../art/coverage.json)。这些源文件语义差异仍可能影响滑条、分类图标与资源图标的外观。
