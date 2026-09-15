# B10 十二项静态资产轴测派发

2026-09-10 用户确认：按制作人复核v02建议，先制作六组件＋六建筑整批轴测。
任务ID：b10-static-assets-axonometric-batch；负责人：art-concept-3d；接收：制作人，抄主美。

## 输入与边界

主仓库B10 `evidence/producer-static-contract-review-v02.md` 为本轮核对依据。
ArtResource `Docs/ArtPipeline/Preproduction/b10-interactive-world-and-static-art/README.md` 的
组件合同v01、B10-ART013-MatingAddendum-v02.md、建筑合同v01、12项Manifest为输入。
本派发覆盖这些文档中旧的“尚未批准轴测”状态，但不覆盖三视图/建模冻结。
新增尺寸、适配形体和水泵岸线仅作为本轮比例参考，不冻结为程序配置或已验证装配。
既有B08仅作批准风格/宿主接口参考，不重画、不改模、不重开其验收。

## 顺序与输出

按Manifest顺序：ObjectStateSensor、SoilSensor、ExplorationScanner、CommunicationReceiver、
BasicComputeCore、CommunicationTransmitter；BasicWarehouse、ManufacturingWorkshop、
BiomassGenerator、SolarGenerator、BasicBattery、ShoreWaterPump。
每项一张完整轴测候选，必要的安装/服务面辅图可放同图，不另扩任务。
输出仅 ArtResource `ArtSource/Concept/b10-interactive-world-and-static-art/Axonometric/v01/`，
命名 `B10-<资产键>-Axonometric-v01.png`，附同目录交付索引与自检记录。
参考InputReadiness中的已批准风格图，精度与已批准WheelModule原画相当；纯净背景、无文字/数字堆叠，
硬表面分件、厚度、承力连接清楚，无无支撑游离件。尺寸和状态说明写索引，不靠图中文字定义合同。

组件要有自身适配承接而不改宿主；仓库只外壳入口；工坊两个独立输出位置且无内部工序；
能源静态；水泵岸上底座、短入水管、岸侧交接清楚，岸线只是参考、不增加环境资产。
禁止Blender/Unity/AI3D建模、三视图、正式资源导入、Git与xlsx修改。

## 连续执行与交付

冻结批次内按顺序连续出完12项，单张完成不暂停请求审批；自检明显不符输入可在本任务内修正，保留迭代记录。
最终候选实际发制作人并抄主美，含绝对路径、满足项/问题/未验证项；不直接找用户判通过。
制作人看图报告后由用户决定是否返工及生产图分支。图完成不是模型通过。
完成任务必须实际通知制作人，再按队列收口；通知不等待“已阅”。
