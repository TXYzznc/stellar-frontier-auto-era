# P0-009 建立项目InputModule并接入PC镜头 — DoD 对照审计与键位清单交付

日期：2026-09-16。任务来源：第一版开发任务表（client 队列 p0009-inputmodule-pc-camera）。
用户已确认：无需 PlayMode 实机操作验收；按差距清单实施键位清单契约。

## DoD 逐条对照

| DoD | 结论 | 依据 |
|---|---|---|
| 产品层不直接读取 UnityEngine.Input | 满足 | 全项目唯一设备读取点为 RegionInputModule.DesktopSource（即项目输入边界自身）；框架 ScriptsBuiltin 无运行时输入代码 |
| 所有语义输入均经过项目 InputModule | 满足 | 世界输入：RegionInputModule（Pan/Orbit/Zoom/Select/Focus/Rotate/Cancel → 镜头/选择/放置）；UI 意图：AutoEraUiIntentInputAdapter 只收语义意图 → AutoEraUiIntentRouter → AutoEraUiFormBase.TryCloseFromInputModule（框架 UI 关闭接点） |
| 可替换输入 Provider | 满足 | IRegionInputSource + SetSource；EditMode 测试以 FixedSource 注入（InitialRegionSceneEditModeTests 等） |
| 统一处理 WASD/旋转/缩放/聚焦/Esc | 满足 | DesktopSource 默认源 + RegionCameraController.Apply/Focus（边界与角度/距离钳制）；Esc 走 Cancel 意图链 |
| UI 拦截 | 满足 | IsPointerOverGameObject + AutoEraUiRuntime.BlocksWorldInput → 整帧跳过；Cancel 优先于拦截 |

## 本次新增交付（键位清单契约）

- `Assets/Game/Scripts/AutoEra/Input/RegionInputBindings.cs`：RegionInputAction（9 语义动作）、
  RegionInputTrigger（Hold/Press）、只读 RegionInputBinding、RegionInputBindingSet
  （CreateDefault 默认清单 / Create 校验自定义清单：未定义枚举、重复动作、重复键拒绝）。
- `RegionInputModule.Bindings`：只读暴露（字段初始化器保证 EditMode 无 Awake 亦可读）；
  DesktopSource 改为清单驱动，按 Trigger 决定 GetKey/GetKeyDown，鼠标键以 KeyCode.Mouse0/1 表达。

## 验证

- `AutoEra.Tests.Editor.RegionInputBindingsEditModeTests`：5/5 通过（jobId 670617cd）。
  覆盖：默认清单 9 动作默认键位/触发、键位唯一、重复动作/重复键拒绝、部分清单 TryGet 缺失返回
  false、模块无 Awake 即暴露清单。
- 全量 EditMode 回归：479 用例（474 存量 + 5 新增），476 通过，3 失败均为既存基线，零新增失败。
- 编译零错误；Console 无新增错误。

## 边界与遗留

- 可重绑按键、手柄 Provider、主菜单/枢纽输入、选择轮廓视觉分别属 P8-007/P6-009/P1-002，不在本任务。
- 键位显示的设置页消费随 P8-007 接入；本契约即其唯一数据来源。
- 未修改 ScriptsBuiltin、场景、Git、xlsx；xlsx 任务行状态由制作人侧更新。