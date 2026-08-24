## ART-005 任务 3.2：程序化表现参数与稳定锚点合同

### 结论

- 已形成可复用且机器可读的美术交接合同，覆盖坐标语义、轮组模板、关节限制、默认／安全／重新对位姿态、净空、作业点、效应器接口、状态接口及轮／支撑接地点。
- JSON 明确声明 `art-handoff-only`，Unity Prefab 仍是运行时层级与表现权威；该合同不承担玩法、物理、导航或网络权威。
- 本段未导出或覆盖已通过的 Unity Machine V3，未把运行时接地、通行、IK 或安全阶段流程计入 3.2。

### 直接证据（ArtResource）

- 参数说明：`Docs/ArtPipeline/ART-005-Procedural-Presentation-Parameters.md`
- 机器可读合同：`Assets/Art/Authoring/ART005_ProceduralMachine/Contracts/ART005_ProceduralPresentationParameters.json`
- Unity Meta：`Assets/Art/Authoring/ART005_ProceduralMachine/Contracts/ART005_ProceduralPresentationParameters.json.meta`
- 过程与确定性校验记录：`Docs/ArtPipeline/Evidence/b03-parallel-art-art004-art005-production-rnd/nightly-2026-08-24.md`

### 确定性复核

- Schema=`ART005.ProceduralPresentationParameters.v1`，角色=`art-handoff-only`；JSON 可正常解析。
- 悬挂行程以显式最小／最大值记录：`-0.12m`～`0.12m`；轮半径 `0.52m`，四轮接地点齐全。
- `Socket_Effector` 相对腕部为 `(0.25,0,0)`；效应器根相对 Socket 为 `(0.15,0,0)`，两类偏移已明确分离。
- 四个支撑接地点 local=`(0,0,-0.38)`、世界 Z=`0`；三套姿态键为 `Travel_Retracted`、`Work_Deployed`、`Yellow_RepositionRequired`。
- 重新对位姿态明确要求：底盘运动前收回支撑、仅低速蟹行微调、机械臂只在 KeepOut 内有限补偿。
- 8091 刷新后 JSON 被 AssetDatabase 识别为 TextAsset，Meta GUID=`5703c99e719aea6459c7acac50175fa5`；未修改 Unity 场景。

### 后续边界

- 任务 3.3 起由客户端运行时表现与 ArtResource 样机协同验证，不允许表现层绕过逻辑层通行裁决。
- 3.3 的 35°连续坡度和轮半径台阶阈值属于机器表现／逻辑边界，不与 ART-004 的 20°／35°建筑入口坡道混用。
