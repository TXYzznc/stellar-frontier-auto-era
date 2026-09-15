# B14 精确候选路径

仅供用户手动触发Git集成时复核；本窗口不暂存、不提交、不自动请求。以下既有文件可能包含前序未提交工作，不可据此把全部工作区归为本轮。

## 服务与接入

- Assets/Game/Scripts/AutoEra/Machines/Sensors.meta
- Assets/Game/Scripts/AutoEra/Machines/Sensors/SensorProfile.cs
- Assets/Game/Scripts/AutoEra/Machines/Sensors/SensorProfile.cs.meta
- Assets/Game/Scripts/AutoEra/Machines/Sensors/SensorReadContracts.cs
- Assets/Game/Scripts/AutoEra/Machines/Sensors/SensorReadContracts.cs.meta
- Assets/Game/Scripts/AutoEra/Machines/Sensors/MachineSensor.cs
- Assets/Game/Scripts/AutoEra/Machines/Sensors/MachineSensor.cs.meta
- Assets/Game/Scripts/AutoEra/Machines/Sensors/MachineSensorSet.cs
- Assets/Game/Scripts/AutoEra/Machines/Sensors/MachineSensorSet.cs.meta
- Assets/Game/Scripts/AutoEra/Machines/Sensors/SensorCatalog.cs
- Assets/Game/Scripts/AutoEra/Machines/Sensors/SensorCatalog.cs.meta
- Assets/Game/Scripts/AutoEra/World/Region/RegionSensorReadProvider.cs
- Assets/Game/Scripts/AutoEra/World/Region/RegionSensorReadProvider.cs.meta
- Assets/Game/Scripts/AutoEra/World/Region/SensorPublicDataContracts.cs
- Assets/Game/Scripts/AutoEra/World/Region/SensorPublicDataContracts.cs.meta
- Assets/Game/Scripts/AutoEra/Machines/MachineExecutionContext.cs（仅Sensors创建/释放）
- Assets/Game/Scripts/AutoEra/World/Region/InitialRegionScene.cs（仅显式传感集合接入/采样/释放）

## 输入、工具和受控产物

- GameData/AIData/DataTables/Sensors/SensorDefinitions.json
- GameData/AIData/DataTables/Machines/ComponentDefinitions.json（仅四行授权功率/注释及工具逻辑指纹）
- GameData/AIData/GenerationProfiles.json（仅Sensors映射）
- Assets/Game/Scripts/AutoEra/Editor/SensorDataSetup.cs
- Assets/Game/Scripts/AutoEra/Editor/SensorDataSetup.cs.meta
- Assets/Game/ScriptableAssets/Core/AppConfigs.asset（仅Sensors/SensorDefinitions幂等登记）
- GameData/DataTables/Sensors/SensorDefinitions.xlsx（既有工具Reverse产物）
- GameData/DataTables/Machines/ComponentDefinitions.xlsx（既有工具Reverse产物）
- Assets/Game/DataTable/Sensors.meta
- Assets/Game/DataTable/Sensors/SensorDefinitions.txt
- Assets/Game/DataTable/Sensors/SensorDefinitions.txt.meta
- Assets/Game/Scripts/AutoEra/DataTable/SensorDefinitions.cs
- Assets/Game/Scripts/AutoEra/DataTable/SensorDefinitions.cs.meta
- Assets/Game/DataTable/Machines/ComponentDefinitions.txt

ComponentDefinitions.cs正式生成后内容无schema变化，不作为本轮代码修正。既有meta不改GUID；上述新meta仅清理空字段尾随空格。

## 测试与OpenSpec

- Assets/Game/Tests/AutoEra/Editor/MachineSensorEditModeTests.cs
- Assets/Game/Tests/AutoEra/Editor/MachineSensorEditModeTests.cs.meta
- Assets/Game/Tests/AutoEra/Editor/SensorPublicDataEditModeTests.cs
- Assets/Game/Tests/AutoEra/Editor/SensorPublicDataEditModeTests.cs.meta
- Assets/Game/Tests/AutoEra/Editor/MachineSensorIntegrationTests.cs
- Assets/Game/Tests/AutoEra/Editor/MachineSensorIntegrationTests.cs.meta
- openspec/changes/b14-sensor-binding-and-public-readouts/.openspec.yaml
- openspec/changes/b14-sensor-binding-and-public-readouts/proposal.md
- openspec/changes/b14-sensor-binding-and-public-readouts/design.md
- openspec/changes/b14-sensor-binding-and-public-readouts/tasks.md
- openspec/changes/b14-sensor-binding-and-public-readouts/specs/sensor-readout-lifecycle/spec.md
- openspec/changes/b14-sensor-binding-and-public-readouts/specs/sensor-public-data/spec.md
- openspec/changes/b14-sensor-binding-and-public-readouts/evidence/client-verification.md
- openspec/changes/b14-sensor-binding-and-public-readouts/evidence/rapid-regression.md
- openspec/changes/b14-sensor-binding-and-public-readouts/evidence/candidate-paths.md

严格排除开发任务表、其它xlsx、ScriptsBuiltin、其它窗口派发/文档、UI、美术、Prefab、场景、字体、Library/Temp及队列本机状态。
