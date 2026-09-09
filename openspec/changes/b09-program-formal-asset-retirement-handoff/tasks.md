## 1. 退役前清单与安全门

- [x] 1.1 记录所有候选组的 GUID、引用者、文件哈希、目录大小和替代正式路径；将扫描结果写入 b09 evidence，明确 ContractSample、ART-006、FunctionalPrototypes、旧 MotionGraph、旧演示场与截图的边界。（证据：`evidence/segment-01-retirement-preflight.md`）
- [x] 1.2 对每个候选组执行 AssetDatabase 依赖、场景/Prefab、源码和配置路径扫描；发现任一正式引用即记录阻塞并禁止该组删除。（证据：`evidence/segment-01-retirement-preflight.md`）
- [x] 1.3 归档可审计的过程截图至 b05/b06/b09 evidence，核对原/归档文件哈希；不删除任何运行资产。（证据：`evidence/archived-art006-ui/` 与 `segment-01-retirement-preflight.md`）

## 2. Operations 正式入口独立化

- [x] 2.1 只通过 Unity Prefab/AssetDatabase 工作流将 `Operations/BaseCommandHubForm` 与 `Operations/FieldHudForm` 展开并保存为独立正式入口，保留 GF、TMP、Sprite、输入与状态绑定。（证据：`segment-01-retirement-preflight.md` 的“Operations 独立入口”）
- [ ] 2.2 更新 UITable JSON 的稳定入口（如路径发生变化），仅按 AI JSON→Validate→Reverse→生成流程同步；不手改任何 xlsx 或生成 C#。
- [x] 2.3 运行 Missing Reference、实际 GF UIForm 开关、默认页、Cancel/焦点恢复、输入和状态 EditMode/PlayMode 回归；记录 Console Error=0 证据。（证据：b05 既有真实 UIForm 回归，加上 job `83460b2c` 的 b09 独立化复验）
- [x] 2.4 确认候选链引用归零后，使用 AssetDatabase 退役 `Assets/Game/Prefabs/UI/ART006_UI/` 及 `Assets/Game/Art/UI/ART006_UI/` 的过程副本和空目录。（证据：`evidence/segment-02-formal-entity-handoff-and-retirement.md`）

## 3. 正式 Entity 接管 b06 动作预览

- [x] 3.1 将 b06 Catalog/预览/验收构建器、MotionGraph 资产、测试和演示场从 `FunctionalPrototypes` 路径切换为对应正式 Entity Prefab 与 B08 MotionContracts；不改变动作语义或已验收低模内容。（证据：`evidence/segment-02-formal-entity-handoff-and-retirement.md`）
- [x] 3.2 创建或更新固定正式动作预览场，使其仅引用正式 Entity，并覆盖各兼容 MotionGraph、停止恢复和预览时间轴。（证据：`evidence/segment-02-formal-entity-handoff-and-retirement.md`）
- [x] 3.3 运行正式 Entity 的结构、MotionGraph、动作预览、确定性、Missing Reference、普通编译、Console 和必要 PlayMode 回归；失败时保留旧原型组并修复接管。（证据：`evidence/segment-02-formal-entity-handoff-and-retirement.md`）

## 4. 成组退役与收口

- [x] 4.1 在无引用门禁通过后，使用 AssetDatabase 成组退役 `Prefabs/ContractSample`、`Materials/ContractSample`、`Config/ContractSample` 和 `Scene/ArtValidation/ART001_ImportValidation.unity`。（证据：`evidence/segment-01-retirement-preflight.md` 的“ART001 ContractSample 原子组”）
- [x] 4.2 在正式 Entity 接管门禁通过后，使用 AssetDatabase 成组退役 `Prefabs/FunctionalPrototypes`、`Materials/FunctionalPrototypes`、`MotionGraphs/FunctionalPrototypes`、旧 `FunctionalRigAcceptanceDemo` 场及其 Assets 内截图和空 `Scenes` 根。（证据：`evidence/segment-02-formal-entity-handoff-and-retirement.md`）
- [x] 4.3 删除每批后的项目边界/框架纯度审计、引用扫描、Unity 编译和目标测试，确认不影响正式 Entity、Operations、Motion Core、B08 MotionContracts 或任务表。（证据：`evidence/segment-02-formal-entity-handoff-and-retirement.md`）
- [x] 4.4 更新 b09 tasks 与证据，列出保留项、已退役项、恢复路径和未纳入本批次的 `Font`/`Fonts` 独立规范化建议；保留未提交现场供用户手动 Git 流程处理。（证据：`evidence/segment-01-retirement-preflight.md`、`evidence/segment-02-formal-entity-handoff-and-retirement.md`）
