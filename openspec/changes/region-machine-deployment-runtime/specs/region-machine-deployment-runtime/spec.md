# Region Machine Deployment Runtime

## ADDED Requirements

### Requirement: Placement validation and domain commit are separate

落位流程 SHALL 把「校验」与「领域提交」分成两段：预览只做校验并暴露合法性与原因，
提交只调用领域部署（`DeployMachine`），SHALL NOT 在落位流程内做建造结算、资源扣减或经济判定。

#### Scenario: Preview reports validity without committing

- **WHEN** a player moves or rotates the placement preview
- **THEN** the preview reports validity and a human-readable reason
- **AND** no region object, roster state or resource is changed

#### Scenario: Commit deploys exactly once

- **WHEN** the player confirms a valid placement
- **THEN** the region gains exactly one object bound to that machine
- **AND** the roster marks that machine deployed
- **AND** confirming again does not create a second object

#### Scenario: A precondition fails

- **WHEN** the machine is not in the roster, is already deployed, or the region is inactive
- **THEN** the flow refuses to begin and reports the reason
- **AND** no placement preview is left active

### Requirement: Views bind to deployed objects instead of re-registering

机器的视图 SHALL 绑定**已存在**的区域对象，SHALL NOT 用「用自身序列化字段注册新对象」的路径
处理已部署机器；视图释放 SHALL NOT 删除由领域拥有的区域对象。

#### Scenario: Deployed machine gains a view

- **WHEN** a view is bound to an already deployed machine
- **THEN** the number of region objects is unchanged
- **AND** the view exposes that same object as its model

#### Scenario: View is released while the machine is still deployed

- **WHEN** a bound view is released
- **THEN** the region object still exists and the machine remains deployed
- **AND** re-binding a new view to the same machine is possible

#### Scenario: Registration path is unchanged

- **WHEN** a scene-seeded object is initialized
- **THEN** it still registers a new region object as before

### Requirement: Navigation binding failure is a presentable state

机器导航绑定 SHALL 在失败时给出可展示的原因，SHALL NOT 让绑定失败成为未捕获异常；
定义声明不可移动的机器 SHALL NOT 被视为失败。

#### Scenario: A movable machine cannot bind

- **WHEN** a movable machine is deployed but the navigation surface is not ready,
  or its prefab lacks a motion rig, or its instance is not at the deployment position
- **THEN** the machine stays deployed
- **AND** the failure is exposed as a presentable reason
- **AND** no exception escapes the deployment flow

#### Scenario: A non-movable machine

- **WHEN** a machine whose definition cannot move is deployed
- **THEN** no navigation is bound and no failure is reported
- **AND** its runtime is still created

### Requirement: Per-region machine runtimes are derived and not persisted

机器运行时 SHALL 按区域持有、在部署成功后创建、在区域释放时销毁，
SHALL NOT 被写入存档；区域就绪时 SHALL 能仅凭领域状态重建运行时。

#### Scenario: Region release disposes runtimes

- **WHEN** the region is released
- **THEN** every machine runtime is disposed
- **AND** no subscription or callback from them remains

#### Scenario: Runtime is rebuilt after a region restart

- **WHEN** a region becomes ready again with deployed machines
- **THEN** a runtime exists for each deployed machine without reading persisted runtime state
- **AND** machines start idle

### Requirement: Missing art degrades presentation without denying deployment

当机器实体预制体缺失或无效时，领域部署 SHALL 仍然成立，界面 SHALL 显示可展示的降级原因，
SHALL NOT 伪造视图或回滚部署。

#### Scenario: Prefab is missing

- **WHEN** a machine's prefab path cannot be resolved
- **THEN** the machine is still deployed in the region and roster
- **AND** the absence of a view is reported as a reason rather than as an error
