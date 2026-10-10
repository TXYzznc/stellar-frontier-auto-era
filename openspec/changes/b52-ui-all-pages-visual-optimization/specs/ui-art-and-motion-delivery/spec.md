## ADDED Requirements

### Requirement: Formal visuals have a separate approval gate
正式视觉生产 SHALL 依次通过布局、效果图、切片、装配和实机效果验收，结构通过不得代替美术通过。
#### Scenario: Prototype is approved
- **WHEN** 用户通过结构原型
- **THEN** 只记录结构已通过，正式资源、效果图与动效保持待制作状态。
#### Scenario: Formal representative visuals pass
- **WHEN** 用户通过代表实机效果并冻结视觉系统
- **THEN** 才将相同视觉规则推广所有剩余页面。

### Requirement: Motion communicates real state without blocking operation
动效 MUST 跟随真实状态、可中断、在关闭/回收时释放；保持正文可读且不让高频操作等待装饰动画。
#### Scenario: A page closes during an animation
- **WHEN** 界面关闭或切换世界
- **THEN** 动画和订阅停止，旧对象回调不更新新界面，不残留输入遮罩。
#### Scenario: A production result is presented
- **WHEN** 显示成果、路径或趋势
- **THEN** 只使用真实服务结果和已有采样，不补造数据或补播离线历史动作。

### Requirement: Formal visual delivery remains reproducible
美术交付 MUST 保留源文件、切片规则、资源引用、单一字体规则、状态差异和 1920×1080 实机证据。
#### Scenario: Assets are replaced
- **WHEN** 正式图片替换结构占位
- **THEN** 图像保持比例、边框按九宫格伸缩、动态文字独立，合同与 Prefab 同步，装饰不拦截输入。
