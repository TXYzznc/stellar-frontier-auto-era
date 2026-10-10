# 算法工作台可用性与闭环

原始任务映射：P3-010、P3-011、P3-013、P3-014。本文件为增量合同；正式规则引用design.md，不覆盖未完成的原任务范围。

## ADDED Requirements

### Requirement: Workbench acceptance uses one resolution
算法工作台 SHALL 仅以1920×1080作为本批运行视觉与交互验收分辨率，同时以结构合同保障布局适配。

#### Scenario: Reference resolution review
- **WHEN** 在1920×1080打开空图、复杂图和诊断模式
- **THEN** 标题、端口、列表与按钮无重叠裁切，连线可辨，无额外分辨率验收项。

#### Scenario: Layout structure review
- **WHEN** 检查算法Form及节点预制体结构
- **THEN** 根Stretch和GF统一缩放成立，锚点/pivot/边距语义一致，同一尺寸轴不存在布局双重驱动。

### Requirement: Editing preserves active configuration
工作台 SHALL 隔离草稿编辑与已应用运行，并把应用结果准确反馈给玩家。

#### Scenario: Invalid connection or apply
- **WHEN** 玩家连接不兼容端口或提交无效草稿
- **THEN** 非法连接被拒或问题定位到节点，旧运行实例保持有效。

#### Scenario: Close after applying
- **WHEN** 玩家提交合法应用后关闭工作台
- **THEN** 请求由运行宿主持有并处理，重新打开可读到真实状态。

### Requirement: Diagnostics use captured revision
诊断 SHALL 使用被选择记录的实际修订、输入与责任链，不以当前图冒充历史执行。

#### Scenario: Inspect older run
- **WHEN** 当前草稿已变化而玩家选择旧运行记录
- **THEN** 高亮与说明对应旧修订，无法映射的节点明确说明，任务结果可追溯。

### Requirement: Acceptance proves product execution
算法闭环验收 SHALL 同时证明UI操作、正式运行驱动与世界结果。

#### Scenario: Run template end to end
- **WHEN** 玩家从模板建立实例、绑定、应用并触发真实任务
- **THEN** 正式宿主执行且产生可诊断结果，测试未手动Pump或伪造完成事件。
