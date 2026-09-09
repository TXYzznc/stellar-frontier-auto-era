## Context

本项目已拥有 11 类已验收的低模正式资产、材质和 Entity Prefab，B08 的 `B08FormalEntityPrefabBuilder` 与 MotionContracts 已将它们绑定至既有动作合同。然而，b06 的 Catalog Builder、MotionGraph Builder、测试及 `FunctionalRigAcceptanceDemo` 仍使用 `FunctionalPrototypes` 的基础几何路径；ART-006 的两个运行入口仍从 `ART006_UI` V03 候选链取得视觉层；ART001 ContractSample 与 ART-006 过程证据树仍留在产品资源根。

用户已确认以下冻结决策：

1. 正式 Entity 是低模验收后的动作预览和运行时视觉权威，旧 b06 原型只可作为迁移前参考，不能继续作为正式接管阻碍。
2. Operations 的两枚稳定运行入口必须先独立化，之后才退役 V01–V04、候选场景和 StructurePrototype。
3. 过程截图保留为 OpenSpec 证据；过程资产和其 `.meta` 通过 AssetDatabase 在引用为零后成组退役。

约束：不修改 `ScriptsBuiltin`、任何 xlsx、任务表或 Git 索引；Unity 资产移动、重命名、删除维持 GUID；结构变化在退出 Play Mode 后执行普通编译与回归。

## Goals / Non-Goals

**Goals:**

- 让正式 Entity Prefab 接管 b06 预览、动作图、固定演示场和相关自动化测试的资产路径。
- 让 `Operations/BaseCommandHubForm` 与 `Operations/FieldHudForm` 成为不依赖候选链的完整运行入口。
- 归档可审计截图，原子退役 ContractSample、ART-006 过程根、旧开发原型及演示场。
- 每个退役批次均具有引用扫描、回归、Missing Reference 与 Console 门禁，并可在失败时保留旧组回滚。

**Non-Goals:**

- 不改变 MotionGraph 语义、运行时玩法、物流、物理权威、数据表或 UI 交互合同。
- 不改动正式低模的网格、材质、贴图、骨骼、动作范围或视觉设计。
- 不删除 Motion Core UPM 导出、B08 MotionContracts、正式 Entity、正式 Operations 资源或永久回归测试。

## Decisions

### 1. 正式 Entity 优先，而非保留双轨运行

`Prefabs/Entity`、B08 MotionContracts 和正式 MotionGraph 路径将成为 b06 预览和验收唯一资产来源。候选方案包括：保留原型直到未来版本、让两套路径长期并存、立即切换正式 Entity。选择立即切换，因为正式低模已通过用户验收且 B08 已验证同一合同；长期双轨会造成动作数据和视觉表现漂移。

旧基础几何不会单独逐个删除。先迁移所有 Builder、测试、演示场和 MotionGraph 引用，运行完整回归；只有引用扫描为零后才按目录组删除。

### 2. Operations 入口以独立 Prefab 收口

Operations 两枚运行入口通过 Unity Prefab/AssetDatabase 工作流从当前已验收视觉装配展开为独立正式 Prefab，并保留现有正式 Sprite、TMP、GF UIForm 和输入绑定。候选方案包括：继续保留候选链、让正式入口继续继承 V03、独立化正式入口。选择独立化，避免运行时依赖带版本的候选资源。

### 3. 证据归档与 Unity 资产退役分离

截图先复制至相应 OpenSpec evidence（作为非运行时文档证据），校验哈希和引用后，再由 AssetDatabase 删除 Assets 内的过程副本及其 `.meta`。这避免将过程截图继续放在运行资源根，同时不把验证历史一并删除。

### 4. 以原子退役批次执行

按以下顺序实施：

1. ContractSample 与 ART001 验证场；
2. Operations 入口独立化、验证和 `ART006_UI` 候选链退役；
3. 正式 Entity 接管 b06 工具、测试、动作图和演示场；
4. 归档 b06 截图并退役 FunctionalPrototypes、对应材质/动作图及旧 `Scenes` 根；
5. 对 `Font`/`Fonts` 完成独立 GUID/引用审计后，才决定是否另开字体规范化批次。

每一批均先做快照/引用清单，失败时停止在删除前，保留旧资产并恢复引用；不会用文件系统移动 Unity 资产来绕过 GUID。

## Risks / Trade-offs

- [正式 Entity 层级与旧基础几何不完全等价] → 在删除前跑动作图、锚点、关节范围、预览时间轴和固定演示场回归；缺口仅修正绑定/配置，不更改已验收模型。
- [UI 独立化遗漏 V03 子资源] → Prefab 展开后执行 Missing Reference、真实 Form 开关、输入焦点和 Console 回归；失败则保留候选链。
- [证据移动破坏历史可追溯性] → 复制后记录哈希、原路径与新路径，确认 OpenSpec 链接存在再删除 Assets 副本。
- [误删仍被静态路径引用的资产] → 同时使用 AssetDatabase GUID 依赖、源码路径搜索、场景/Prefab 扫描；任一引用存在即中止对应批次。

## Migration Plan

1. 记录当前 GUID、依赖图、截图哈希和正式资产映射作为 b09 evidence。
2. 依任务顺序完成正式入口接管和每批回归，再执行 AssetDatabase 删除。
3. 每批删除后 Refresh、普通编译、Missing Reference、目标 EditMode/PlayMode 与 Console 检查。
4. 若任一门禁失败，在本批次内恢复上一份仍保留的路径/资产；不继续下一批。

## Open Questions

- 无。字体根的规范化不在本变更中执行，待独立审计后另行确认。
