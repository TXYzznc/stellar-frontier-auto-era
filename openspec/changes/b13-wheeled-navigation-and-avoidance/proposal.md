## Why

B12 已建立机器与区域同一永久身份及作业预约，但预约不能代替实际移动。P2-008 需要把已确认的轮式导航、算力、安全停止与真实运动表现接起来，不扩大为物流、交通或玩家路径编程系统。

## What Changes

- 增加载体固有导航服务，复用机器任务、算力及区域作业队列；固定载体拒绝移动。
- 使用现有 Unity NavMesh 模块完成路径、静态绕障、多机基础避让、停稳与面向对齐。
- 配置化初次/重规划 10 算力、移动受保护 5 算力及 3/10/30 秒恢复规则；取消、目标失效和退出一次清理。
- 从真实位移投影四轮 Motion；用正式轮式资源在隔离场验证，不修改正式场景或美术。

## Capabilities

### New Capabilities

- `carrier-navigation`: 载体移动、算力与安全终态合同。
- `navigation-work-approach`: 区域预约、兼容接近位置及真实运动接入。

### Modified Capabilities

无；继承 B11/B12 与现行游戏设计，不改其既有语义。

## Impact

仅在派发授权的 Machines、World/Region、Motion/Adapter、AutoEra 测试及本 change 内修改。使用现有 Unity 2022.3 内置 AI 模块，不新增包、程序集或全局设置。不修改 ScriptsBuiltin、UI、正式 Prefab/MotionGraph、任何 xlsx 或 Git 索引。正式组合根接入若需要授权外路径，单独列明，不据隔离验证宣称 G2 或 B11 已完成。
