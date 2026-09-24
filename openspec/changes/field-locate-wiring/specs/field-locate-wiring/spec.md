# 现场定位：输入、按钮与快捷键汇入同一意图

## ADDED Requirements

### Requirement: 定位只有一个实现

把镜头带到现场对象的动作 SHALL 只有一处实现；
键盘快捷键、鼠标双击与界面按钮 SHALL 全部经过它。

#### Scenario: 按钮与快捷键结果一致

- **WHEN** 选中一个现场对象，先按界面的「聚焦」按钮，再把一帧「按了 F」喂给输入模块
- **THEN** 两次得到的镜头焦点 SHALL 完全相同

#### Scenario: 聚焦到对象自己的锚点

- **WHEN** 按下「聚焦」
- **THEN** 镜头焦点 SHALL 是该对象的焦点锚点
- **AND** SHALL NOT 是包围盒中心或别的计算结果

### Requirement: 没有可定位对象时入口不可点

没有选中现场对象、或没有可用的现场镜头时，「聚焦」按钮 SHALL 不可点，
定位调用 SHALL 返回失败，SHALL NOT 编造一个结果。

#### Scenario: 未选中任何对象

- **WHEN** 现场没有选中对象
- **THEN** 现场内容页的「聚焦」按钮 SHALL 不可点
- **AND** 定位调用 SHALL 返回 false

#### Scenario: 选中之后入口变可用

- **WHEN** 现场选中一个对象
- **THEN** 「聚焦」按钮 SHALL 变为可点
- **AND** SHALL 在下一个世界秒节拍内反映出来

#### Scenario: 没有现场输入模块

- **WHEN** 界面被打开时没有现场输入模块（编辑器直接打开、测试装置）
- **THEN** 「聚焦」按钮 SHALL 不可点
- **AND** SHALL NOT 因此报错

### Requirement: 可点性以现场选中为准

「聚焦」按钮的可点性 SHALL 来自现场选区（输入模块），
SHALL NOT 由页面自身的选中行推断。

#### Scenario: 现场选区为空

- **WHEN** 页面显示了某个对象的内容，但现场选区为空
- **THEN** 「聚焦」按钮 SHALL 不可点

## MODIFIED Requirements

### Requirement: 现场内容页的聚焦入口

现场机器概况、四个资源观察页（农田／林地／矿脉／水源）与建筑总览的「聚焦」按钮
SHALL 接到现场定位意图；SHALL NOT 保持死按钮。

#### Scenario: 现场机器概况

- **WHEN** 现场选中一台机器并按下「镜头聚焦」
- **THEN** 镜头 SHALL 定位到该机器

#### Scenario: 资源观察页

- **WHEN** 现场选中一个资源点并按下该页的「聚焦」
- **THEN** 镜头 SHALL 定位到该资源点

#### Scenario: 建筑总览

- **WHEN** 现场选中一个建筑并按下「聚焦建筑」
- **THEN** 镜头 SHALL 定位到该建筑

#### Scenario: 定位关联对象的按钮

- **WHEN** 检查「查看水源」「定位关联对象」这类按钮
- **THEN** 它们 SHALL NOT 在本能力内被接成「聚焦当前对象」——
      它们定位的是**另一个**对象，需要单独的通道
