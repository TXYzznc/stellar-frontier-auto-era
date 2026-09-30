## 1. 设计与公共出口

- [ ] 1.1 审计所有 Form 的返回/关闭按钮与 EscapeClose 配置。
- [x] 1.2 修复报告型 Form 的显式关闭路径：ProgressReportForm 与 OperationFeedbackForm 的显式按钮直接关闭当前 Form。
- [x] 1.3 普通 Shell Form 默认隐藏重复的 Btn_FormClose，仅保留 Btn_FormBack；显式多步流程可通过 KeepFormCloseButton 覆写保留。

## 2. 算法编辑器闭环

- [x] 2.1 接通模板库到算法编辑器的实例化入口：复用 TemplateAlgorithmReadModel 的真实模板实例化。
- [x] 2.2 接通节点增删、移动、端口连线和选中状态：补齐编辑器操作绑定并保留版本检查。
- [x] 2.3 接通绑定选择器与绑定回写：算法编辑器打开 AlgorithmBindingForm，复用现有选择器回写。
- [x] 2.4 接通公开参数草稿编辑和默认值恢复。
- [x] 2.5 接通统一应用、警告确认和结果刷新。
- [x] 2.6 接通只读诊断模式的记录选择、步骤定位和编辑模式返回。

## 3. HUD 拆分

- [x] 3.1 建立首个现场对象详情子 Form 和 UI 表登记；当前作为迁移适配层，尚未替换全部旧详情页。
- [x] 3.1a 将状态、追踪、告警、导航和存档五个常驻面板搬入独立 FieldHudResidentForm 预制体，由 FieldHudForm 动态加载与释放。
- [x] 3.2 迁移机器、资源点、建筑详情页。
- [x] 3.3 迁移会话、读模型订阅、区域选择和焦点恢复。
- [x] 3.4 删除已迁移内容在 FieldHudForm 中的重复字段和渲染路径。

## 4. 验收

- [x] 4.1 EditMode 契约、出口和读模型写路径测试。
- [x] 4.2 PlayMode 算法完整流程测试。
- [x] 4.3 PlayMode HUD 打开/关闭后输入恢复测试。
- [x] 4.4 编译、引用、审计和 OpenSpec 严格校验。
