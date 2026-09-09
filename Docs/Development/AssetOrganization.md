# 自动纪元资源组织规范

本文件是“星际拓荒：自动纪元”产品资源目录的唯一细化规范。
`.claude/conventions.md` 保留框架级资源类型骨架；若两者对产品资源的具体
目录命名存在差异，以本文件为准。

## 原则

- 资源首先按类型根目录组织，再按稳定的业务域、用途或交付状态细分。
- Unity 资源的移动、重命名和删除必须通过 `AssetDatabase`，保留 `.meta` 与
  GUID；不得用文件系统移动 Unity 资产。
- 过程标识、批次号、候选版号和外部美术项目名不得进入正式资源路径。
- 运行时正式资源不得与验证、灰盒、合同代理或生成样机混放；开发验证资源
  使用 `Development/` 明确隔离。

## 类型根目录

正式资源只能置于 `Assets/Game/` 下已有的类型根目录，例如 `Animations/`、
`Audio/`、`Fonts/`、`Materials/`、`Models/`、`Prefabs/`、`Scene/`、
`Sprites/`、`Textures/`、`VFX/`。其下再使用业务域目录，例如
`Prefabs/UI/Operations/` 与 `Prefabs/Entity/`。

不得新建或继续使用单数旧根目录 `Assets/Game/Font/`、`Material/`；其中遗留
资产必须先完成引用迁移和验证，再删除旧根。

## 字体

唯一字体根目录是 `Assets/Game/Fonts/`。当前 UI 字体结构如下：

| 路径 | 用途 |
| --- | --- |
| `Fonts/UI/Source/` | TTF/OTF 源字体；TMP Font Asset 必须引用这里的源字体 GUID。 |
| `Fonts/UI/Fallback/` | CJK 等回退 TMP Font Asset。 |
| `Fonts/UI/Launch/` | Launch 场景专用 TMP Font Asset 与标题材质。 |
| `Fonts/UI/Operations/` | 基地运营 UI 的 TMP Font Asset。 |

同一字体文件不得在两个路径以不同 GUID 重复保存。不同用途的 TMP SDF Font
Asset 即使源字体相同，也可以并存：它们的字形图集、回退链或材质配置可能不同，
必须以用途区分路径和名称，不能按文件名擅自删除。

新增字体前应先扫描 `Fonts/` 内同内容源文件、既有 TMP 资产和 GUID 引用；
需要新增时必须写入上述用途目录。修改字体引用后，至少完成 Missing Reference
扫描、普通编译、相关 UI 或场景打开验证以及 Console Error=0。

## Prefab 与开发资源

- `Prefabs/UI/<Domain>/`：稳定 UIForm 入口。
- `Prefabs/Entity/<Domain>/`：正式实体运行入口。
- `Prefabs/Development/<Purpose>/`：仅开发、技术演示、验收或合同代理；不得被
  正式配置、UITable、Addressables 正式组或发布内容引用。

本规范的目录变动必须同步更新 `ProjectBaseline.md` 入口与
`audit_project_boundaries.py`，使规范和自动门禁一起演进。
