# hub-machine-list

## MODIFIED Requirements

### Requirement: 中枢机器索引只列已部署机器

BaseCommandHubForm 的机器索引 MUST 只渲染已部署到现场的机器（Deployed），库中蓝图不属中枢远程视图；机器库（MachineLibraryForm）继续按 Deployed 标志分「库中／已部署」两页，两页共享同一份读模型快照。

#### Scenario: 库中蓝图不出现在中枢

- **WHEN** 花名册同时有库中蓝图与已部署机器
- **THEN** 中枢机器索引只列已部署机器，正文报告「已连接」台数

#### Scenario: 点击行选中正确机器

- **WHEN** 用户点过滤后列表的第 N 行
- **THEN** 读模型选中该行对应的稳定机器 Id（不是全量列表第 N 台）
