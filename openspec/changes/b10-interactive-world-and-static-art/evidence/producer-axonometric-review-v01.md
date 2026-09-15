# B10 十二项轴测：制作人实际看图报告 v01

2026-09-10。用户已确认本报告的四项最小返修建议、其余八项保留；新候选仍需回报用户，不放行三视图或建模。执行派发：Docs/Development/Dispatch/Active/b10-static-axonometric-minimal-rework.md。本报告下文保留当时观察与建议。

## 范围与结论

逐张查看12张正式PNG（排除Soil rejected），结合主美技术支持与组件配合补充v02、建筑合同核对。工业硬表面、分件厚度和材质风格整体一致；未见需整批推翻的依据。按art-direction方法区分视觉表达偏差与尚待几何验证，不将圆形螺栓、未做碰撞检测等直接判为失败。

建议用户考虑仅对扫描器、计算核心、通信发射器、制造工坊四项作局部调整，其余八项保留候选。也可选择保留现图，在生产说明中限制歧义；最终由用户决定，未下发返工。

## 逐项事实、问题与影响

### 对象状态传感器

光学窗、壳体与承接清楚。短圆柱颈有回转联想，但不足以认定结构错误；建议保留造型，生产说明约束为固定连接。

![对象状态传感器](D:/unity/UnityProject/ArtResource/ArtSource/Concept/b10-interactive-world-and-static-art/Axonometric/v01/B10-ObjectStateSensor-Axonometric-v01.png)

### 土壤传感器

下倾检测窗、实体背撑清楚，无入土探针；底座偏厚属于比例候选，建模按接口合同收敛。

![土壤传感器](D:/unity/UnityProject/ArtResource/ArtSource/Concept/b10-interactive-world-and-static-art/Axonometric/v01/B10-SoilSensor-Axonometric-v01.png)

### 基础探索扫描器

宽阵列可辨，但成对大轴端与背架明显像俯仰机构。建议仅将连接收敛为固定支架/止转肋，不新增运动。也可保留外观并明确静态。

![基础探索扫描器](D:/unity/UnityProject/ArtResource/ArtSource/Concept/b10-interactive-world-and-static-art/Axonometric/v01/B10-ExplorationScanner-Axonometric-v01.png)

### 通信接收器

天线、封装壳和实体底座清楚。整体偏高，图中没有宿主尺度参照；不据像素断言尺寸超标。

![通信接收器](D:/unity/UnityProject/ArtResource/ArtSource/Concept/b10-interactive-world-and-static-art/Axonometric/v01/B10-CommunicationReceiver-Axonometric-v01.png)

### 基础计算核心

主图抬高的四脚与厚环底座偏离v02低矮薄鞍意图；辅图方座与主图也不够统一。建议保留壳体语言，仅改紧凑贴底承接，避免建模照搬高脚。

![基础计算核心](D:/unity/UnityProject/ArtResource/ArtSource/Concept/b10-interactive-world-and-static-art/Axonometric/v01/B10-BasicComputeCore-Axonometric-v01.png)

### 通信发射器

壳体和散热结构可用，但辅图仍是通用四脚底座，未清楚表达固定旋转载体的前侧折形挂接。建议保留本体，补正确安装辅图；不改已验收宿主。

![通信发射器](D:/unity/UnityProject/ArtResource/ArtSource/Concept/b10-interactive-world-and-static-art/Axonometric/v01/B10-CommunicationTransmitter-Axonometric-v01.png)

### 基础仓库

封闭仓库外壳与入口明确，没有内部库存或容量四档；本次未见需新增功能的图面问题。

![基础仓库](D:/unity/UnityProject/ArtResource/ArtSource/Concept/b10-interactive-world-and-static-art/Axonometric/v01/B10-BasicWarehouse-Axonometric-v01.png)

### 制造工坊

封闭壳体与两输出平台明确，但两台集中挡在主门前，入口与交接关系拥挤。建议局部调整平台与入口位置，使服务面分开；这只是二维关系问题，不是已实测导航阻塞，入口亦不因此新增可进入玩法。

![制造工坊](D:/unity/UnityProject/ArtResource/ArtSource/Concept/b10-interactive-world-and-static-art/Axonometric/v01/B10-ManufacturingWorkshop-Axonometric-v01.png)

### 生物质发电机

机组、排气与承载框连续，无裸露燃料或内部产线；本次未见明显需返修的功能表达问题。

![生物质发电机](D:/unity/UnityProject/ArtResource/ArtSource/Concept/b10-interactive-world-and-static-art/Axonometric/v01/B10-BiomassGenerator-Axonometric-v01.png)

### 太阳能发电机

斜撑三角关系明确；圆形紧固件本身不足以认定有追日机构，建议保留并注明固定角度。

![太阳能发电机](D:/unity/UnityProject/ArtResource/ArtSource/Concept/b10-interactive-world-and-static-art/Axonometric/v01/B10-SolarGenerator-Axonometric-v01.png)

### 基础蓄电池

封闭低矮壳体可辨；三道青蓝格栅可能被误读为电量档，建议注明恒定装饰而非容量反馈，可不重画。

![基础蓄电池](D:/unity/UnityProject/ArtResource/ArtSource/Concept/b10-interactive-world-and-static-art/Axonometric/v01/B10-BasicBattery-Axonometric-v01.png)

### 岸边水泵

泵体、短入水管、支撑和两平台可辨。局部硬岸只作场景示意，不能证明任意岸形适配；包装流程保持抽象，不因此增加装瓶机械。

![岸边水泵](D:/unity/UnityProject/ArtResource/ArtSource/Concept/b10-interactive-world-and-static-art/Axonometric/v01/B10-ShoreWaterPump-Axonometric-v01.png)

## 后续边界

- 图只确认造型意图；不能按像素确认精确尺寸、装配接触、碰撞、邻槽扫掠或交接可达。
- 六组件共同的厚底座/偏高比例需在宿主配合设计中收敛；不自动修改B08模型，不新增运行时关节。
- 用户决定后才派发指定图的最小返修或下一生产图阶段；未决定前不启动三视图、Blender、Unity或Git。
