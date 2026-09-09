# Segment 02：正式 Entity 接管与过程资产退役

日期：2026-09-09。

## 正式 Entity 接管

- 正式预览场：`Assets/Game/Scene/B08FormalEntityMotionPreview.unity`。它直接实例化 10 个已验收的 `Prefabs/Entity` 入口，移除了“演示_可替换效应器”。
- 稳定关节匹配兼容两种已存在的合同命名：`stableId` 与 `Joint_stableId`。因此 B08 模型的真实关节会被动作导演驱动，而不要求模型改回灰盒命名。
- 水枪、旋锯、旋钻、货舱和传送带的演示表现只附着于该正式预览场实例：水枪弧线/落点、锯钻粒子、0.45m 单门横移与 0.75m 货盘、连续闭环履带/示例货物均不重新引入旧灰盒 Prefab。
- 验收：结构 `c4c8b261` 23/23；初始路径接管 `8b79aec7` 12/12 与 `d0fde566` 1/1；正式预览场 `7db30f4b` 2/2、`4cfbfc38` 1/1；表现修正回归为 `e667ea57` 1/1、`9f71d0a7` 2/2、`72b80b41` 1/1、`a7995c27` 6/6、`1f307b90` 2/2。各次 Console Error=0。

## 删除批次

通过 8090 `AssetDatabase.DeleteAsset` 成组删除，6/6 成功：

- `Assets/Game/Prefabs/UI/ART006_UI/`
- `Assets/Game/Art/UI/ART006_UI/`
- `Assets/Game/Prefabs/FunctionalPrototypes/`
- `Assets/Game/Materials/FunctionalPrototypes/`
- `Assets/Game/MotionGraphs/FunctionalPrototypes/`
- `Assets/Game/Scenes/AutoEra/`

删除后所有六个路径均不存在；源码扫描只留下“不得依赖旧路径”的负向测试断言与历史文档证据，不存在可执行运行、生成或配置引用。保留的合同工具改为显式生成至 `Prefabs/Development/MotionRig` / `Materials/Development/MotionRig`，不会复建 `FunctionalPrototypes` 根。

## 删除后完整性

- `AssetDatabase.Refresh` 后普通编译完成；Console Error=0。
- 正式预览场 `validate_scene`：0 issues；`validate_missing_references`：0；`validate_find_missing_scripts(searchInPrefabs=true)`：0。
- 最终 UI、正式动作场与 Motion Core 回归仍待本批的删除后统一 QA 门禁。

## 删除后最终门禁

- `B08FormalEntityPrefabEditModeTests` job `3da37a85`：23/23。
- `B08FormalEntityMotionPreviewSceneEditModeTests` job `daa2a377`：2/2。
- `FunctionalRigMotionGraphCatalogEditModeTests` job `51cce3b3`：12/12。
- `AutoEraUiPrefabBindingEditModeTests` job `10ba35c9`：6/6。
- 原生 `Game Framework/AutoEra/QA/Run Operations UIForm PlayMode Test`：`OperationsForms_OpenAndCloseThroughUiExtension` 为 `Passed`，耗时 3.798081 秒。
- Console Error=0、Warning=0；Unity 2022.3.62f3c1 退出后非 PlayMode、非编译。框架纯度审计通过；`openspec validate b09-program-formal-asset-retirement-handoff --strict` 通过。
