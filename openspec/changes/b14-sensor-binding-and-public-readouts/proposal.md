## Why

P2-007已有槽位、永久身份和算力池，但尚无实际传感读取服务。按DEC-197建立可独立验证的基础，不以测试农业数据冒充资源生产。

## What Changes

- 单目标永久引用、固定周期采样、有效性与旧样本隔离。
- 显式公开提供者、合法组件锚点及逐土地只读合同。
- 统一各等级1秒、6/4米、10算力；生命周期释放、边界让出。
- 配置路径精确申请后接入；未接入时不勾选完成。

## Capabilities

### New Capabilities
- `sensor-readout-lifecycle`: 绑定、范围、采样与计算租约。
- `sensor-public-data`: 只读提供者与逐单元快照。

### Modified Capabilities
无。

## Impact

仅原派发Sensors目录、两个Region文件、MachineExecutionContext/InitialRegionScene必要接入及指定三测试文件。本change管理证据。禁止ScriptsBuiltin、场景/Prefab、UI、直接xlsx写入或Git；不新增包/程序集，不重开B11/B12/B13。
